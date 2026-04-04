using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LMS_THPT.Controllers
{
    [Authorize]
    public class DoiMatKhauController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;

        public DoiMatKhauController(UserManager<NguoiDung> userManager,
                                     SignInManager<NguoiDung> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: hiển thị form đổi mật khẩu
        public IActionResult Index()
        {
            return View();
        }

        // POST: xử lý đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Index");
        }
    }
}