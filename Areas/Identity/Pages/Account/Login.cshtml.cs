using LMS_THPT.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_THPT.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<NguoiDung> signInManager,
            UserManager<NguoiDung> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống")]
            public string UserName { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect("/Home/Welcome");
            }

            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid) return Page();

            // Kiểm tra user và trạng thái IsActive trước khi đăng nhập
            var user = await _userManager.FindByNameAsync(Input.UserName)
                       ?? await _userManager.FindByEmailAsync(Input.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
                return Page();
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.");
                return Page();
            }

            // Đăng nhập bằng UserName
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("Người dùng đã đăng nhập.");

                // reuse the user we loaded earlier

                // Cập nhật claim AnhDaiDien
                var claims = (await _userManager.GetClaimsAsync(user)).ToList();
                var existingClaim = claims.FirstOrDefault(c => c.Type == "AnhDaiDien");
                if (existingClaim != null)
                    await _userManager.RemoveClaimAsync(user, existingClaim);

                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(
                    "AnhDaiDien",
                    user.AnhDaiDien ?? "/images/default-avatar.png"
                ));

                // Redirect theo role
              

                return LocalRedirect("/Home/Welcome");
            }

            if (result.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Tài khoản bị khóa.");
                return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return Page();
        }
    }
}