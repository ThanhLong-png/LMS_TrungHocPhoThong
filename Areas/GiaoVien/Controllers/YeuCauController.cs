// Areas/GiaoVien/Controllers/YeuCauController.cs
// Chỉ chứa các actions dành cho vai trò GiaoVien

using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Areas.GiaoVien.Controllers
{
    [Area("GiaoVien")]
    [Authorize(Roles = "GiaoVien")]
    public class YeuCauController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public YeuCauController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Trang danh sách yêu cầu của giáo viên
        /// </summary>
        public async Task<IActionResult> MyRequests(int? trangThai, string? loai)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var query = _context.DanhSachYeuCau
                .Where(y => y.GiaoVienId == user.Id)
                .Include(y => y.Lop)
                .AsQueryable();

            if (trangThai.HasValue)
            {
                if (trangThai.Value == 0 || trangThai.Value == 1)
                {
                    query = query.Where(y => y.TrangThai == TrangThaiYeuCau.ChoXuLy || y.TrangThai == TrangThaiYeuCau.ChoDuyet);
                    ViewBag.Filter = "1"; // chuẩn hóa hiển thị form
                }
                else
                {
                    query = query.Where(y => (int)y.TrangThai == trangThai.Value);
                    ViewBag.Filter = trangThai.Value.ToString();
                }
            }

            if (!string.IsNullOrEmpty(loai) && Enum.TryParse<LoaiYeuCau>(loai, out var loaiEnum))
            {
                query = query.Where(y => y.LoaiYeuCau == loaiEnum);
                ViewBag.LoaiFilter = loai;
            }

            var requests = await query
                .OrderByDescending(y => y.NgayGui)
                .ToListAsync();

            ViewData["Title"] = "Yêu cầu của tôi";
            ViewData["ActivePage"] = "YeuCau";
            return View("TeacherRequests", requests);
        }

        /// <summary>
        /// Tạo yêu cầu đăng ký lớp chủ nhiệm - GET
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RegisterClass()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var availableClasses = await _context.Lops
                .Where(l => l.GiaoVienChuNhiemId == null)
                .Include(l => l.Khoi)
                .ToListAsync();

            ViewData["AvailableClasses"] = availableClasses;
            ViewData["Title"] = "Đăng ký lớp chủ nhiệm";
            ViewData["ActivePage"] = "YeuCau";
            return View();
        }

        /// <summary>
        /// Gửi yêu cầu đăng ký lớp chủ nhiệm - POST
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RegisterClass(int lopId, string moTa)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            try
            {
                var lop = await _context.Lops.FindAsync(lopId);
                if (lop == null)
                    return BadRequest("Lớp không tồn tại");

                if (lop.GiaoVienChuNhiemId != null)
                    return BadRequest("Lớp này đã có chủ nhiệm");

                var existingRequest = await _context.DanhSachYeuCau
                    .FirstOrDefaultAsync(y =>
                        y.LopId == lopId &&
                        y.LoaiYeuCau == LoaiYeuCau.DangKyLopChuNhiem &&
                        y.TrangThai == TrangThaiYeuCau.ChoXuLy);

                if (existingRequest != null)
                    return BadRequest("Lớp này đã có yêu cầu chờ xử lý");

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

                if (yeuCau.GiaoVienId != user.Id)
                    return Unauthorized();

                if (yeuCau.TrangThai != TrangThaiYeuCau.ChoXuLy && yeuCau.TrangThai != TrangThaiYeuCau.ChoDuyet)
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

        /// <summary>
        /// Tạo yêu cầu chung - GET
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var myClasses = await _context.LopMonHocs
                    .Where(m => m.GiaoVienId == user.Id && m.LopId != null)
                    .Include(m => m.Lop)
                    .Select(m => m.Lop)
                    .Distinct()
                    .ToListAsync();
                ViewBag.MyClasses = myClasses;
            }

            ViewData["Title"] = "Tạo yêu cầu mới";
            ViewData["ActivePage"] = "YeuCau";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectsForClass(int lopId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var subjects = await _context.LopMonHocs
                .Include(m => m.MonHoc)
                .Where(m => m.GiaoVienId == user.Id && m.LopId == lopId)
                .Select(m => new { id = m.MonHocId, name = m.MonHoc.TenMonHoc })
                .ToListAsync();

            return Json(subjects);
        }

        public async Task<IActionResult> GetClassScheduleForDate(int lopId, string date)
        {
            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return BadRequest("Invalid date");

            int thu = (int)selectedDate.DayOfWeek + 1;
            if (thu == 1) thu = 8; // Chủ nhật là 8

            // Lấy tất cả lịch học của lớp trong ngày đó (bao gồm lịch cố định và lịch bù đúng ngày)
            var scheduleRecords = await _context.LichHocs
                .Include(l => l.MonHoc)
                .Include(l => l.GiaoVien)
                .Where(l => l.LopId == lopId && l.Thu == thu && (!l.IsHocBu || l.NgayHoc.Date == selectedDate.Date))
                .ToListAsync();

            var result = new List<object>();

            for (int period = 1; period <= 10; period++)
            {
                var lesson = scheduleRecords.FirstOrDefault(l => l.TietHoc == period);
                if (lesson != null)
                {
                    result.Add(new
                    {
                        period = period,
                        isOccupied = true,
                        subjectName = lesson.MonHoc?.TenMonHoc ?? "Không rõ",
                        teacherName = lesson.GiaoVien?.HoTen ?? "Không rõ",
                        isHocBu = lesson.IsHocBu
                    });
                }
                else
                {
                    result.Add(new
                    {
                        period = period,
                        isOccupied = false,
                        subjectName = (string)null,
                        teacherName = (string)null,
                        isHocBu = false
                    });
                }
            }

            return Json(result);
        }

        /// <summary>
        /// Tạo yêu cầu chung - POST
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRequest(YeuCauGiaoVien model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Loại bỏ các trường không cần thiết khỏi ModelState
            ModelState.Remove("GiaoVienId");
            ModelState.Remove("GiaoVien");
            ModelState.Remove("NguoiXuLy");
            ModelState.Remove("XuLyBoi");
            ModelState.Remove("Lop");

            if (ModelState.IsValid)
            {
                model.GiaoVienId = user.Id;
                // Sử dụng ChoDuyet để tương thích với Admin
                model.TrangThai = TrangThaiYeuCau.ChoDuyet; 
                model.NgayGui = DateTime.Now;

                _context.DanhSachYeuCau.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Gửi yêu cầu thành công! Vui lòng chờ xử lý từ admin.";
                return RedirectToAction("MyRequests");
            }

            ViewData["Title"] = "Tạo yêu cầu mới";
            ViewData["ActivePage"] = "YeuCau";
            return View(model);
        }
    }
}
