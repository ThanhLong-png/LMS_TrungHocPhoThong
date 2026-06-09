using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_THPT.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;

        public ProfileController(UserManager<NguoiDung> userManager,
                                 SignInManager<NguoiDung> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Hiển thị form sửa hồ sơ
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Cập nhật hồ sơ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NguoiDung model, Microsoft.AspNetCore.Http.IFormFile AnhDaiDienFile)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.HoTen = model.HoTen;
            user.NgaySinh = model.NgaySinh;
            user.GioiTinh = model.GioiTinh;
            user.DiaChi = model.DiaChi;

            // Xử lý upload ảnh đại diện
            if (AnhDaiDienFile != null && AnhDaiDienFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(AnhDaiDienFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await AnhDaiDienFile.CopyToAsync(stream);
                }

                user.AnhDaiDien = "/uploads/" + fileName;

                // Cập nhật claim "AnhDaiDien"
                var claims = (await _userManager.GetClaimsAsync(user)).ToList();
                var existingClaim = claims.FirstOrDefault(c => c.Type == "AnhDaiDien");
                if (existingClaim != null)
                    await _userManager.RemoveClaimAsync(user, existingClaim);

                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim(
                    "AnhDaiDien",
                    user.AnhDaiDien
                ));
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);

                return View(model);
            }

            // **Refresh cookie để cập nhật claim mới ngay lập tức**
            await _signInManager.RefreshSignInAsync(user);

            TempData["Success"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Edit");
        }
    }
}