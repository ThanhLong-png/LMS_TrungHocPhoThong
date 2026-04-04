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
        public IActionResult Login(string? returnUrl = null)
        {
            // Use Identity UI login page (Area = Identity)
            if (!string.IsNullOrEmpty(returnUrl))
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, bool rememberMe, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                // Redirect to Identity login page when missing credentials
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
            }

            var user = await _userManager.FindByEmailAsync(username);

            if (user == null)
            {
                // Redirect to Identity login page if user not found
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, password, rememberMe, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Contains("Admin"))
                    return RedirectToAction("Index", "Admin");

                if (roles.Contains("GiaoVien"))
                    return RedirectToAction("Index", "GiaoVien");

                if (roles.Contains("HocSinh"))
                    return RedirectToAction("Index", "HocSinh");

                if (roles.Contains("HieuTruong"))
                    return RedirectToAction("Index", "HieuTruong");

                return RedirectToAction("Index", "Home");
            }

            // If sign-in failed, redirect to the Identity login page so UI shows proper errors
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}