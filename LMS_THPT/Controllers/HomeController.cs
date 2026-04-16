using System.Diagnostics;
using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly UserManager<NguoiDung> _userManager;

        public HomeController(
            ApplicationDbContext context,
            SignInManager<NguoiDung> signInManager,
            UserManager<NguoiDung> userManager)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ================= TRANG CHỦ =================
        public async Task<IActionResult> Index()
        {
            var thongBao = await _context.ThongBaos
                .Where(x => x.HienThi)
                .OrderByDescending(x => x.NgayDang)
                .Take(4)
                .ToListAsync();

            ViewBag.ThongBao = thongBao;

            if (User.Identity?.IsAuthenticated == true)
            {
                ViewBag.SoHocSinh = await _context.Users.CountAsync(u => u.MaHocSinh != null);
                ViewBag.SoGiaoVien = await _context.Users.CountAsync(u => u.ChuyenMon != null);
                ViewBag.SoLop = await _context.Lops.CountAsync();
                ViewBag.SoMonHoc = await _context.DanhSachMonHoc.CountAsync(m => m.IsActive);
            }

            return View();
        }

        // ================= WELCOME (CÓ PHÂN TRANG) =================
        public async Task<IActionResult> Welcome(int page = 1)
        {
            int pageSize = 5;

            // ==== THỐNG KÊ ====
            ViewBag.SoHocSinh = await _context.Users.CountAsync(u => u.MaHocSinh != null);
            var soGiaoVien = await (
           from u in _context.Users
           join ur in _context.UserRoles on u.Id equals ur.UserId
           join r in _context.Roles on ur.RoleId equals r.Id
           where r.Name == "GiaoVien"
           select u
       ).CountAsync();

            ViewBag.SoGiaoVien = soGiaoVien;
            ViewBag.SoLop = await _context.Lops.CountAsync();
            ViewBag.SoMonHoc = await _context.DanhSachMonHoc.CountAsync(m => m.IsActive);

            // ==== THÔNG BÁO ====
            var query = _context.ThongBaos
                .Where(t => t.HienThi)
                .OrderByDescending(t => t.NgayDang);

            int total = await query.CountAsync();

            var thongBaoList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ThongBao = thongBaoList;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            // ==== ROLE ====
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                ViewBag.Role = roles.FirstOrDefault();
            }

            return View();
        }

        // ================= LOGIN =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string userName, string password, bool rememberMe)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.";
                return RedirectToAction("Index");
            }

            var result = await _signInManager.PasswordSignInAsync(
                userName.Trim(), password, rememberMe, false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(userName.Trim());

                if (user != null && !user.IsActive)
                {
                    await _signInManager.SignOutAsync();
                    TempData["LoginError"] = "Tài khoản đã bị khóa.";
                    return RedirectToAction("Index");
                }

                // 👉 CHUYỂN SANG WELCOME (QUAN TRỌNG)
                return RedirectToAction("Welcome");
            }

            if (result.IsLockedOut)
                TempData["LoginError"] = "Tài khoản bị khóa tạm thời.";
            else
                TempData["LoginError"] = "Sai tài khoản hoặc mật khẩu.";

            return RedirectToAction("Index");
        }

        // ================= KHÁC =================
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}