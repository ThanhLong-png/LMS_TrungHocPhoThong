using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LMS_THPT.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using LMS_THPT.Data;
using Microsoft.Extensions.DependencyInjection;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<NguoiDung> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 📋 Danh sách user có phân trang + role
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var allUsers = _userManager.Users.ToList();
            
            // Lấy roles cho toàn bộ user để sắp xếp
            var userRoles = new Dictionary<string, string>();
            var rolePriority = new Dictionary<string, int> {
                { "Admin", 1 },
                { "HieuTruong", 2 },
                { "GiaoVien", 3 },
                { "HocSinh", 4 }
            };

            foreach (var u in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userRoles[u.Id] = roles.FirstOrDefault() ?? "Other";
            }

            // Sắp xếp: Role ưu tiên -> Tên A-Z
            var sortedUsers = allUsers
                .OrderBy(u => rolePriority.GetValueOrDefault(userRoles[u.Id], 5))
                .ThenBy(u => u.HoTen)
                .ToList();

            // Thống kê
            ViewBag.HocSinhCount = userRoles.Values.Count(r => r == "HocSinh");
            ViewBag.GiaoVienCount = userRoles.Values.Count(r => r == "GiaoVien");
            
            // Lấy context để đếm môn học và lớp
            var context = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
            ViewBag.MonHocCount = await context.DanhSachMonHoc.CountAsync();
            ViewBag.LopCount = await context.Lops.CountAsync();

            // Phân trang trên danh sách đã sắp xếp
            int total = sortedUsers.Count;
            var pagedUsers = sortedUsers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.UserRoles  = userRoles;
            ViewBag.Page       = page;
            ViewBag.PageSize   = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.Total      = total;

            return View(pagedUsers);
        }

        // ➕ GET: Tạo user
        public IActionResult Create()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        // ➕ POST: Tạo user + gán role
        [HttpPost]
        public async Task<IActionResult> Create(string hoTen, string email, string password, string role)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email hoặc Password không được để trống");
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View();
            }

            var user = new NguoiDung
            {
                UserName = email,
                Email = email,
                HoTen = hoTen
            };

            // Tạo user
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    Console.WriteLine("Error: " + err.Description);
                }
            }
            else
            {
                Console.WriteLine("User created! ID: " + user.Id);
            }
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View();
            }

            // Kiểm tra role và tạo nếu chưa tồn tại
            if (!string.IsNullOrEmpty(role))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                    return View();
                }
            }

            return RedirectToAction("Index");
        }

        // ❌ Xóa user
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index");
        }

        // ✏️ GET: Sửa role
        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);

            ViewBag.User = user;
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            ViewBag.UserRoles = userRoles;

            return View();
        }

        // ✏️ POST: Sửa role
        [HttpPost]
        public async Task<IActionResult> EditRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!string.IsNullOrEmpty(role))
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction("Index");
        }
    }
}