using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LMS_THPT.Models; // Nơi để class NguoiDung
using System.Linq;
using System.Threading.Tasks;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // 🔹 Inject UserManager và RoleManager
        public AdminController(UserManager<NguoiDung> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // 📋 Danh sách user
        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
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