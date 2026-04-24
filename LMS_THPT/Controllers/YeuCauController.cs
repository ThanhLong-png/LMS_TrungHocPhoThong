using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Controllers
{
    // Root YeuCauController chỉ phục vụ Admin.
    // Các actions của GiaoVien đã được chuyển sang Areas/GiaoVien/Controllers/YeuCauController.cs
    [Authorize(Roles = "Admin")]
    public class YeuCauController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public YeuCauController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== ADMIN =====

        /// <summary>
        /// Trang quản lý yêu cầu (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageRequests(string? status)
        {
            IQueryable<YeuCauGiaoVien> query = _context.DanhSachYeuCau
                .Include(y => y.GiaoVien)
                .Include(y => y.Lop);

            // Lọc theo trạng thái
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TrangThaiYeuCau>(status, out var trangThai))
            {
                query = query.Where(y => y.TrangThai == trangThai);
            }

            var requests = await query
                .OrderByDescending(y => y.NgayGui)
                .ToListAsync();

            ViewData["Title"] = "Quản lý yêu cầu giáo viên";
            ViewData["Status"] = status;
            return View(requests);
        }

        /// <summary>
        /// Chi tiết yêu cầu (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RequestDetail(int id)
        {
            var yeuCau = await _context.DanhSachYeuCau
                .Include(y => y.GiaoVien)
                .Include(y => y.Lop).ThenInclude(l => l.Khoi)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (yeuCau == null)
                return NotFound();

            ViewData["Title"] = "Chi tiết yêu cầu";
            return View(yeuCau);
        }

        /// <summary>
        /// Duyệt yêu cầu (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveRequest(int id, string? ghiChu)
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            try
            {
                var yeuCau = await _context.DanhSachYeuCau
                    .Include(y => y.Lop)
                    .FirstOrDefaultAsync(y => y.Id == id);

                if (yeuCau == null)
                    return NotFound();

                if (yeuCau.TrangThai != TrangThaiYeuCau.ChoXuLy)
                    return BadRequest("Chỉ có thể duyệt yêu cầu chờ xử lý");

                // Cập nhật trạng thái
                yeuCau.TrangThai = TrangThaiYeuCau.DaDuyet;
                yeuCau.NgayXuLy = DateTime.Now;
                yeuCau.XuLyBoi = admin.Id;
                yeuCau.GhiChu = ghiChu;

                // Nếu là đăng ký lớp chủ nhiệm, cập nhật lớp
                if (yeuCau.LoaiYeuCau == LoaiYeuCau.DangKyLopChuNhiem && yeuCau.Lop != null)
                {
                    yeuCau.Lop.GiaoVienChuNhiemId = yeuCau.GiaoVienId;
                    _context.Lops.Update(yeuCau.Lop);
                }

                _context.DanhSachYeuCau.Update(yeuCau);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Duyệt yêu cầu thành công!";
                return RedirectToAction("ManageRequests");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Từ chối yêu cầu (Admin)
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> RejectRequest(int id, string ghiChu)
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized();

            try
            {
                var yeuCau = await _context.DanhSachYeuCau.FindAsync(id);
                if (yeuCau == null)
                    return NotFound();

                if (yeuCau.TrangThai != TrangThaiYeuCau.ChoXuLy)
                    return BadRequest("Chỉ có thể từ chối yêu cầu chờ xử lý");

                // Cập nhật trạng thái
                yeuCau.TrangThai = TrangThaiYeuCau.TuChoi;
                yeuCau.NgayXuLy = DateTime.Now;
                yeuCau.XuLyBoi = admin.Id;
                yeuCau.GhiChu = ghiChu ?? "Không rõ lý do";

                _context.DanhSachYeuCau.Update(yeuCau);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Từ chối yêu cầu thành công!";
                return RedirectToAction("ManageRequests");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }
    }
}
