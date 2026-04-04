using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Controllers
{
    [Authorize]
    public class YeuCauController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public YeuCauController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===== GIÁO VIÊN =====

        /// <summary>
        /// Trang danh sách yêu cầu của giáo viên
        /// </summary>
        [Authorize(Roles = "GiaoVien")]
        public async Task<IActionResult> MyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var requests = await _context.DanhSachYeuCau
                .Where(y => y.GiaoVienId == user.Id)
                .Include(y => y.Lop)
                .OrderByDescending(y => y.NgayGui)
                .ToListAsync();

            ViewData["Title"] = "Yêu cầu của tôi";
            return View("TeacherRequests", requests);
        }

        /// <summary>
        /// Tạo yêu cầu đăng ký lớp chủ nhiệm
        /// </summary>
        [Authorize(Roles = "GiaoVien")]
        [HttpGet]
        public async Task<IActionResult> RegisterClass()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Lấy danh sách lớp chưa có GVCN
            var availableClasses = await _context.Lops
                .Where(l => l.GiaoVienChuNhiemId == null)
                .Include(l => l.Khoi)
                .ToListAsync();

            ViewData["AvailableClasses"] = availableClasses;
            ViewData["Title"] = "Đăng ký lớp chủ nhiệm";
            return View();
        }

        /// <summary>
        /// Gửi yêu cầu đăng ký lớp chủ nhiệm
        /// </summary>
        [Authorize(Roles = "GiaoVien")]
        [HttpPost]
        public async Task<IActionResult> RegisterClass(int lopId, string moTa)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                // Kiểm tra lớp có tồn tại không
                var lop = await _context.Lops.FindAsync(lopId);
                if (lop == null)
                    return BadRequest("Lớp không tồn tại");

                // Kiểm tra lớp đã có GVCN chưa
                if (lop.GiaoVienChuNhiemId != null)
                    return BadRequest("Lớp này đã có chủ nhiệm");

                // Kiểm tra có yêu cầu chưa giải quyết cho lớp này không
                var existingRequest = await _context.DanhSachYeuCau
                    .FirstOrDefaultAsync(y => 
                        y.LopId == lopId && 
                        y.LoaiYeuCau == LoaiYeuCau.DangKyLopChuNhiem &&
                        y.TrangThai == TrangThaiYeuCau.ChoXuLy);

                if (existingRequest != null)
                    return BadRequest("Lớp này đã có yêu cầu chờ xử lý");

                // Tạo yêu cầu mới
                var yeuCau = new YeuCauGiaoVien
                {
                    LoaiYeuCau = LoaiYeuCau.DangKyLopChuNhiem,
                    TieuDe = $"Đăng ký lớp chủ nhiệm: {lop.TenLop}",
                    MoTa = moTa ?? "Gửi yêu cầu đăng ký lớp chủ nhiệm",
                    GiaoVienId = user.Id,
                    LopId = lopId,
                    TrangThai = TrangThaiYeuCau.ChoXuLy,
                    NgayGui = DateTime.Now
                };

                _context.DanhSachYeuCau.Add(yeuCau);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gửi yêu cầu thành công! Vui lòng chờ xử lý từ admin.";
                return RedirectToAction("MyRequests");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Hủy bỏ yêu cầu
        /// </summary>
        [Authorize(Roles = "GiaoVien")]
        [HttpPost]
        public async Task<IActionResult> CancelRequest(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                var yeuCau = await _context.DanhSachYeuCau.FindAsync(id);
                if (yeuCau == null)
                    return NotFound();

                // Kiểm tra chủ nhân
                if (yeuCau.GiaoVienId != user.Id)
                    return Unauthorized();

                // Chỉ hủy được nếu còn chờ xử lý
                if (yeuCau.TrangThai != TrangThaiYeuCau.ChoXuLy)
                    return BadRequest("Chỉ có thể hủy yêu cầu đang chờ xử lý");

                yeuCau.TrangThai = TrangThaiYeuCau.HuyBo;
                _context.DanhSachYeuCau.Update(yeuCau);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Hủy yêu cầu thành công!";
                return RedirectToAction("MyRequests");
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi: {ex.Message}");
            }
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
