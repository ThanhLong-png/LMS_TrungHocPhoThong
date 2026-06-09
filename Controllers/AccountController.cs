using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LMS_THPT.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly UserManager<NguoiDung> _userManager;

        public AccountController(SignInManager<NguoiDung> signInManager,
                                 UserManager<NguoiDung> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin")) return RedirectToAction("Index", "Admin");
                    if (roles.Contains("GiaoVien")) return RedirectToAction("Index", "GiaoVien", new { area = "GiaoVien" });
                    if (roles.Contains("HocSinh")) return RedirectToAction("Index", "HocSinh");
                    if (roles.Contains("HieuTruong")) return RedirectToAction("Index", "HieuTruong");
                }
                return RedirectToAction("Welcome", "Home");
            }

            // Redirect sang Identity Razor Pages login (trang đăng nhập thực sự)
            var encodedReturnUrl = string.IsNullOrEmpty(returnUrl) ? "" : $"?ReturnUrl={Uri.EscapeDataString(returnUrl)}";
            return Redirect($"/Identity/Account/Login{encodedReturnUrl}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Tên đăng nhập và mật khẩu không được để trống.");
                return View();
            }

            var user = await _userManager.FindByNameAsync(username) 
                       ?? await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                ModelState.AddModelError("", "Tài khoản không tồn tại.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, rememberMe, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Admin");

                if (roles.Contains("GiaoVien"))
                    return RedirectToAction("Index", "GiaoVien", new { area = "GiaoVien" });

                if (roles.Contains("HocSinh"))
                    return RedirectToAction("Index", "HocSinh");

                if (roles.Contains("HieuTruong"))
                    return RedirectToAction("Index", "HieuTruong");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Sai mật khẩu.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}