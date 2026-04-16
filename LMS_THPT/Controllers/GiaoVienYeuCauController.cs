// Controllers/GiaoVienYeuCauController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_THPT.Data;
using LMS_THPT.Models;
namespace LMS_THPT.Controllers   // ← THÊM DÒNG NÀY
{
    [Authorize(Roles = "GiangVien")]
    public class GiaoVienYeuCauController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public GiaoVienYeuCauController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Danh sách yêu cầu của giáo viên hiện tại
        public async Task<IActionResult> Index(int? trangThai)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.YeuCauGiaoVien
                .Where(y => y.MaGiaoVien == user.Id)
                .AsQueryable();

            if (trangThai.HasValue)
            {
                query = query.Where(y => (int)y.TrangThai == trangThai.Value);
                ViewBag.Filter = trangThai.Value.ToString();
            }

            var danhSach = await query
                .OrderByDescending(y => y.NgayGui)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: Form tạo yêu cầu mới
        public IActionResult TaoYeuCau()
        {
            return View();
        }

        // POST: Gửi yêu cầu mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoYeuCau(YeuCauGiaoVien model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);

            model.MaGiaoVien = user.Id;
            model.NgayGui = DateTime.Now;
            model.TrangThai = TrangThaiYeuCau.ChoDuyet;

            _context.YeuCauGiaoVien.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Yêu cầu đã được gửi thành công! Vui lòng chờ xét duyệt.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Chi tiết yêu cầu
        public async Task<IActionResult> ChiTiet(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var yeuCau = await _context.YeuCauGiaoVien
                .Include(y => y.NguoiXuLy)
                .FirstOrDefaultAsync(y => y.Id == id && y.MaGiaoVien == user.Id);

            if (yeuCau == null)
                return NotFound();

            return View(yeuCau);
        }

        // POST: Hủy yêu cầu (chỉ khi đang ChoDuyet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyYeuCau(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            var yeuCau = await _context.YeuCauGiaoVien
                .FirstOrDefaultAsync(y => y.Id == id && y.MaGiaoVien == user.Id);

            if (yeuCau == null)
                return NotFound();

            if (yeuCau.TrangThai != TrangThaiYeuCau.ChoDuyet)
            {
                TempData["Error"] = "Chỉ có thể hủy yêu cầu đang chờ duyệt.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            _context.YeuCauGiaoVien.Remove(yeuCau);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã hủy yêu cầu thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}