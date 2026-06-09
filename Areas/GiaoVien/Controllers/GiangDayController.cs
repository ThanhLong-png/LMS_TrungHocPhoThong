// Areas/GiaoVien/Controllers/GiangDayController.cs

using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LMS_THPT.Areas.GiaoVien.Controllers
{
    [Area("GiaoVien")]
    [Authorize]
    public class GiangDayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _env;
        private readonly UserManager<NguoiDung> _userManager;

        public GiangDayController(
            ApplicationDbContext context,
            Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
            UserManager<NguoiDung> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // Feed - hiển thị các bài giảng / bài tập và Bảng Điểm
        public async Task<IActionResult> Index(int? lopId, int? monHocId)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewData["ActivePage"] = "GiangDay";

            // Fetch assigned classes from Admin
            var assignedClasses = await _context.LopMonHocs
                .Include(x => x.Lop)
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == user.Id)
                .ToListAsync();

            ViewBag.AssignedClasses = assignedClasses;

            // if (!monHocId.HasValue && assignedClasses.Any())
            // {
            //     var first = assignedClasses.First();
            //     lopId = first.LopId;
            //     monHocId = first.MonHocId;
            // }

            // Validate that the teacher is authorized to access the requested class/subject combination (URL tampering check)
            if (lopId.HasValue && monHocId.HasValue)
            {
                var isAuthorized = assignedClasses.Any(x => x.LopId == lopId.Value && x.MonHocId == monHocId.Value);
                if (!isAuthorized && !User.IsInRole("Admin"))
                {
                    if (assignedClasses.Any())
                    {
                        var first = assignedClasses.First();
                        lopId = first.LopId;
                        monHocId = first.MonHocId;
                    }
                    else
                    {
                        lopId = null;
                        monHocId = null;
                    }
                }
            }

            ViewBag.LopId = lopId;
            ViewBag.MonHocId = monHocId;

            if (!monHocId.HasValue)
            {
                ViewBag.BaiTaps = new List<BaiTap>();
                ViewBag.BinhLuanBaiGiang = new List<BinhLuan>();
                ViewBag.BinhLuanBaiTap = new List<BinhLuan>();
                ViewBag.Lop = null;
                ViewBag.MonHoc = null;
                return View(new List<BaiGiang>());
            }

            var lop = ViewBag.Lop = assignedClasses.FirstOrDefault(x => x.LopId == lopId)?.Lop ?? await _context.Lops.FindAsync(lopId);
            var monHoc = ViewBag.MonHoc = assignedClasses.FirstOrDefault(x => x.MonHocId == monHocId)?.MonHoc ?? await _context.DanhSachMonHoc.FindAsync(monHocId);

            // ✅ BÀI GIẢNG
            var posts = await _context.DanhSachBaiGiang
                .Include(b => b.TaiLieus)
                .Include(b => b.MonHoc)
                .Include(b => b.NguoiDung)
                .Where(b => b.MonHocId == monHocId && (b.LopId == null || b.LopId == lopId) && b.IsActive)
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            // ✅ BÀI TẬP
            var taps = await _context.DanhSachBaiTap
                .Where(t => t.MonHocId == monHocId && (t.LopId == null || t.LopId == lopId))
                .OrderByDescending(t => t.NgayTao)
                .Include(x => x.NguoiDung)
                .ToListAsync();

            ViewBag.BaiTaps = taps;

            // ✅ TÀI LIỆU
            var taiLieus = await _context.TaiLieus
                .Where(t => t.MonHocId == monHocId)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();
            ViewBag.TaiLieus = taiLieus;

            // ✅ BÀI NỘP CỦA HỌC SINH TRONG LỚP (để thống kê)
            var tapIds = taps.Select(t => t.Id).ToList();
            var hocSinhLop = await _context.NguoiDungs
                .Where(u => u.LopId == lopId)
                .Select(u => u.Id)
                .ToListAsync();
                
            var baiNops = await _context.BaiNops
                .Where(b => tapIds.Contains(b.BaiTapId) && hocSinhLop.Contains(b.HocSinhId))
                .ToListAsync();
            ViewBag.BaiNops = baiNops;
            ViewBag.HocSinhCount = hocSinhLop.Count;

            // ✅ COMMENT BÀI GIẢNG
            var baiGiangIds = posts.Select(p => p.Id).ToList();

            var binhLuanBaiGiang = await _context.DanhSachBinhLuan
                .Include(b => b.NguoiDung)
                .Include(b => b.Replies).ThenInclude(r => r.NguoiDung)
                .Where(b => b.BaiGiangId.HasValue
                         && baiGiangIds.Contains(b.BaiGiangId.Value)
                         && b.ParentId == null)
                .ToListAsync();

            // ✅ COMMENT BÀI TẬP
            var baiTapIds = taps.Select(t => t.Id).ToList();

            var binhLuanBaiTap = await _context.DanhSachBinhLuan
                .Include(b => b.NguoiDung)
                .Include(b => b.Replies).ThenInclude(r => r.NguoiDung)
                .Where(b => b.BaiTapId.HasValue
                         && baiTapIds.Contains(b.BaiTapId.Value)
                         && b.ParentId == null)
                .ToListAsync();

            ViewBag.BinhLuanBaiGiang = binhLuanBaiGiang;
            ViewBag.BinhLuanBaiTap = binhLuanBaiTap;

            return View(posts);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatMonHoc(int id, string moTa, string mucTieu, int? lopId)
        {
            var user = await _userManager.GetUserAsync(User);
            var monHoc = await _context.DanhSachMonHoc.FindAsync(id);
            if (monHoc != null)
            {
                monHoc.MoTa = moTa;
                monHoc.MucTieu = mucTieu;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin môn học thành công.";
            }
            return RedirectToAction("Index", new { lopId = lopId, monHocId = id });
        }

        // Tạo bài giảng - GET
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateBaiGiang(int? lopId, int? monHocId)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.LopId = lopId;
            ViewBag.MonHocId = monHocId;
            if (monHocId.HasValue)
            {
                var mon = _context.DanhSachMonHoc.Find(monHocId.Value);
                ViewBag.MonHoc = mon;
            }
            ViewBag.TatCaMonHoc = _context.DanhSachMonHoc.Where(m => m.IsActive).ToList();

            // Danh sách lớp giáo viên được phân công theo môn đang chọn
            var lopsOfMon = User.IsInRole("Admin")
                ? await _context.LopMonHocs
                    .Include(x => x.Lop)
                    .Where(x => !monHocId.HasValue || x.MonHocId == monHocId.Value)
                    .ToListAsync()
                : await _context.LopMonHocs
                    .Include(x => x.Lop)
                    .Where(x => x.GiaoVienId == user.Id && (!monHocId.HasValue || x.MonHocId == monHocId.Value))
                    .ToListAsync();
            ViewBag.LopsOfMon = lopsOfMon;
            ViewBag.DefaultLopId = lopId;
            return View();
        }

        // Tạo bài giảng - POST
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBaiGiang(string title, string content, List<int> lopIds, int? monHocId, IFormFile[] attachments, bool tinhTienDo, string linkTracNghiem)
        {
            if (!monHocId.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn môn học trước khi đăng bài.";
                return RedirectToAction("CreateBaiGiang", new { monHocId });
            }
            if (lopIds == null || !lopIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một lớp.";
                return RedirectToAction("CreateBaiGiang", new { monHocId });
            }

            var user = await _userManager.GetUserAsync(User);

            // Xác thực giáo viên có quyền với từng lớp được chọn
            if (!User.IsInRole("Admin"))
            {
                foreach (var lId in lopIds)
                {
                    var ok = await _context.LopMonHocs.AnyAsync(x => x.LopId == lId && x.MonHocId == monHocId.Value && x.GiaoVienId == user.Id);
                    if (!ok)
                    {
                        TempData["Error"] = "Bạn không được phân công dạy môn này cho một hoặc nhiều lớp đã chọn.";
                        return RedirectToAction("Index");
                    }
                }
            }

            // Upload file đính kèm một lần
            var uploadedFiles = new List<(string relPath, string orig, LoaiTaiLieu loai, long size)>();
            if (attachments != null && attachments.Length > 0)
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "baigiang", DateTime.Now.ToString("yyyyMMdd"));
                Directory.CreateDirectory(uploadsRoot);
                foreach (var file in attachments.Where(f => f != null && f.Length > 0))
                {
                    var orig = Path.GetFileName(file.FileName);
                    var safe = Guid.NewGuid().ToString() + "_" + orig;
                    var savePath = Path.Combine(uploadsRoot, safe);
                    using (var fs = new FileStream(savePath, FileMode.Create))
                        await file.CopyToAsync(fs);
                    var relPath = ("/uploads/baigiang/" + DateTime.Now.ToString("yyyyMMdd") + "/" + safe).Replace("\\", "/");
                    var ext = Path.GetExtension(orig).ToLowerInvariant();
                    var loai = ext == ".pdf" ? LoaiTaiLieu.PDF
                             : (ext == ".mp4" || ext == ".avi" || ext == ".mov") ? LoaiTaiLieu.Video
                             : (ext == ".ppt" || ext == ".pptx") ? LoaiTaiLieu.Slide
                             : LoaiTaiLieu.Khac;
                    uploadedFiles.Add((relPath, orig, loai, file.Length));
                }
            }

            // Tạo bài giảng riêng cho từng lớp được chọn
            int? firstLopId = lopIds.FirstOrDefault();
            foreach (var lId in lopIds)
            {
                var bai = new BaiGiang
                {
                    TieuDe = string.IsNullOrWhiteSpace(title) ? "(Không có tiêu đề)" : title.Trim(),
                    MoTa = content,
                    MonHocId = monHocId.Value,
                    LopId = lId,
                    IsActive = true,
                    NgayTao = DateTime.Now,
                    NguoiDungId = user.Id,
                    TinhTienDo = tinhTienDo,
                    LinkTracNghiem = tinhTienDo ? linkTracNghiem : null
                };
                _context.DanhSachBaiGiang.Add(bai);
                await _context.SaveChangesAsync();

                foreach (var (relPath, orig, loai, size) in uploadedFiles)
                {
                    _context.DanhSachTaiLieu.Add(new TaiLieu
                    {
                        TenTaiLieu = orig,
                        DuongDanFile = relPath,
                        LoaiTaiLieu = loai,
                        KichThuocFile = size,
                        NgayTao = DateTime.Now,
                        BaiGiangId = bai.Id,
                        MonHocId = monHocId
                    });
                }

                if (tinhTienDo)
                {
                    _context.DanhSachBaiTap.Add(new BaiTap
                    {
                        TieuDe = "Trắc nghiệm: " + bai.TieuDe,
                        MoTa = $"Vui lòng hoàn thành bài trắc nghiệm tại link sau: <a href='{linkTracNghiem}' target='_blank'>Nhấn vào đây</a><br/><br/>Sau khi hoàn thành, hãy tải lên ảnh chụp màn hình điểm số hoặc nhập 'Đã hoàn thành' vào nội dung bài nộp.",
                        MonHocId = monHocId.Value,
                        LopId = lId,
                        HanNop = DateTime.Now.AddDays(7),
                        DiemToiDa = 10,
                        LoaiDiem = LoaiDiem.BaiTap,
                        TrangThai = TrangThaiBaiTap.DangMo,
                        NgayTao = DateTime.Now,
                        NguoiDungId = user.Id,
                        HocKy = (DateTime.Now.Month >= 8 || DateTime.Now.Month <= 1) ? 1 : 2
                    });
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Đã đăng bài giảng cho {lopIds.Count} lớp.";
            return RedirectToAction("Index", new { lopId = firstLopId, monHocId });
        }

        // Tạo bài tập - GET
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public async Task<IActionResult> CreateBaiTap(int? lopId, int? monHocId)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.LopId = lopId;
            ViewBag.MonHocId = monHocId;
            if (monHocId.HasValue)
            {
                var mon = _context.DanhSachMonHoc.Find(monHocId.Value);
                ViewBag.MonHoc = mon;
            }
            ViewBag.TatCaMonHoc = _context.DanhSachMonHoc.Where(m => m.IsActive).ToList();

            // Danh sách lớp giáo viên được phân công theo môn đang chọn
            var lopsOfMon = User.IsInRole("Admin")
                ? await _context.LopMonHocs
                    .Include(x => x.Lop)
                    .Where(x => !monHocId.HasValue || x.MonHocId == monHocId.Value)
                    .ToListAsync()
                : await _context.LopMonHocs
                    .Include(x => x.Lop)
                    .Where(x => x.GiaoVienId == user.Id && (!monHocId.HasValue || x.MonHocId == monHocId.Value))
                    .ToListAsync();
            ViewBag.LopsOfMon = lopsOfMon;
            ViewBag.DefaultLopId = lopId;
            return View();
        }

        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBaiTap(string title, string content, List<int> lopIds, int? monHocId, string dueDate, string dueTime, IFormFile[] attachments, LoaiDiem loaiDiem, int hocKy, int cotDiemMieng = 1)
        {
            if (!monHocId.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn môn học để tạo bài tập.";
                return RedirectToAction("CreateBaiTap", new { monHocId });
            }
            if (lopIds == null || !lopIds.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một lớp.";
                return RedirectToAction("CreateBaiTap", new { monHocId });
            }

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin"))
            {
                foreach (var lId in lopIds)
                {
                    var ok = await _context.LopMonHocs.AnyAsync(x => x.LopId == lId && x.MonHocId == monHocId.Value && x.GiaoVienId == user.Id);
                    if (!ok)
                    {
                        TempData["Error"] = "Bạn không được phân công dạy môn này cho một hoặc nhiều lớp đã chọn.";
                        return RedirectToAction("Index");
                    }
                }
            }

            DateTime hanNop = DateTime.Now.AddDays(7);
            if (!string.IsNullOrWhiteSpace(dueDate) && DateTime.TryParse(dueDate, out var d))
                hanNop = d.Date;
            if (!string.IsNullOrWhiteSpace(dueTime) && TimeSpan.TryParse(dueTime, out var t))
                hanNop = hanNop.Date + t;

            int? firstLopId = lopIds.FirstOrDefault();
            foreach (var lId in lopIds)
            {
                _context.DanhSachBaiTap.Add(new BaiTap
                {
                    TieuDe = string.IsNullOrWhiteSpace(title) ? "(Không có tiêu đề)" : title.Trim(),
                    MoTa = content,
                    MonHocId = monHocId.Value,
                    LopId = lId,
                    HanNop = hanNop,
                    DiemToiDa = 10,
                    LoaiDiem = loaiDiem,
                    CotDiemMieng = cotDiemMieng,
                    TrangThai = TrangThaiBaiTap.DangMo,
                    NgayTao = DateTime.Now,
                    NguoiDungId = user.Id,
                    HocKy = hocKy
                });
            }
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo bài tập cho {lopIds.Count} lớp.";
            return RedirectToAction("Index", new { lopId = firstLopId, monHocId });
        }

        // Chi tiết bài giảng
        [HttpGet]
        public async Task<IActionResult> DetailsBaiGiang(int id, int? lopId)
        {
            var bai = await _context.DanhSachBaiGiang
                .Include(b => b.TaiLieus)
                .Include(b => b.MonHoc)
                .Include(b => b.NguoiDung)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bai == null) return NotFound();

            ViewBag.LopId = lopId;
            return View(bai);
        }

        // Chi tiết bài tập
        [HttpGet]
        public async Task<IActionResult> DetailsBaiTap(int id, int? lopId)
        {
            var bt = await _context.DanhSachBaiTap
                .Include(b => b.MonHoc)
                .Include(b => b.NguoiDung)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bt == null) return NotFound();

            ViewBag.LopId = lopId;
            return View(bt);
        }
        // Sửa bài tập - GET
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public async Task<IActionResult> EditBaiTap(int id, int? lopId)
        {
            var baitap = await _context.DanhSachBaiTap.Include(x => x.MonHoc).FirstOrDefaultAsync(x => x.Id == id);
            if (baitap == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && baitap.LopId.HasValue)
            {
                var isAuthorized = await _context.LopMonHocs.AnyAsync(x => x.LopId == baitap.LopId.Value && x.MonHocId == baitap.MonHocId && x.GiaoVienId == user.Id);
                if (!isAuthorized)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa bài tập này.";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.LopId = lopId ?? baitap.LopId;
            return View(baitap);
        }

        // Sửa bài tập - POST
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBaiTap(int id, string title, string content, int? lopId, string dueDate, string dueTime, LoaiDiem loaiDiem, int hocKy, int cotDiemMieng = 1)
        {
            var baitap = await _context.DanhSachBaiTap.FirstOrDefaultAsync(x => x.Id == id);
            if (baitap == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && baitap.LopId.HasValue)
            {
                var isAuthorized = await _context.LopMonHocs.AnyAsync(x => x.LopId == baitap.LopId.Value && x.MonHocId == baitap.MonHocId && x.GiaoVienId == user.Id);
                if (!isAuthorized)
                {
                    TempData["Error"] = "Bạn không có quyền chỉnh sửa bài tập này.";
                    return RedirectToAction("Index");
                }
            }

            DateTime hanNop = baitap.HanNop;
            if (!string.IsNullOrWhiteSpace(dueDate) && DateTime.TryParse(dueDate, out var d))
                hanNop = d.Date;

            if (!string.IsNullOrWhiteSpace(dueTime) && TimeSpan.TryParse(dueTime, out var t))
                hanNop = hanNop.Date + t;

            baitap.TieuDe = string.IsNullOrWhiteSpace(title) ? "(Không có tiêu đề)" : title.Trim();
            baitap.MoTa = content;
            baitap.HanNop = hanNop;
            baitap.LoaiDiem = loaiDiem;
            baitap.CotDiemMieng = cotDiemMieng;
            baitap.HocKy = hocKy;

            _context.Update(baitap);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật bài tập thành công.";
            return RedirectToAction("Index", new { lopId, monHocId = baitap.MonHocId });
        }

        // Xóa bài tập - POST
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBaiTap(int id, int? lopId)
        {
            var baitap = await _context.DanhSachBaiTap.FirstOrDefaultAsync(x => x.Id == id);
            if (baitap == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && baitap.LopId.HasValue)
            {
                var isAuthorized = await _context.LopMonHocs.AnyAsync(x => x.LopId == baitap.LopId.Value && x.MonHocId == baitap.MonHocId && x.GiaoVienId == user.Id);
                if (!isAuthorized)
                {
                    TempData["Error"] = "Bạn không có quyền xóa bài tập này.";
                    return RedirectToAction("Index");
                }
            }

            var monHocId = baitap.MonHocId;
            _context.DanhSachBaiTap.Remove(baitap);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa bài tập.";
            return RedirectToAction("Index", new { lopId, monHocId });
        }
        // Xem danh sách nộp bài
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public async Task<IActionResult> Submissions(int baiTapId, int? lopId)
        {
            var bt = await _context.DanhSachBaiTap.FindAsync(baiTapId);
            if (bt == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && bt.LopId.HasValue)
            {
                var isAuthorized = await _context.LopMonHocs.AnyAsync(x => x.LopId == bt.LopId.Value && x.MonHocId == bt.MonHocId && x.GiaoVienId == user.Id);
                if (!isAuthorized)
                {
                    TempData["Error"] = "Bạn không có quyền truy cập bài nộp này.";
                    return RedirectToAction("Index");
                }
            }

            var subs = await _context.DanhSachBaiNop
                .Include(s => s.HocSinh)
                .Where(s => s.BaiTapId == baiTapId)
                .OrderByDescending(s => s.NgayNop)
                .ToListAsync();

            ViewBag.BaiTap = bt;
            ViewBag.LopId = lopId;
            return View(subs);
        }

        // POST API: Chấm điểm bài tập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiemBaiTap([FromBody] ChamDiemRequest req)
        {
            if (req == null) return Json(new { success = false, message = "Dữ liệu không hợp lệ" });

            var baiNop = await _context.DanhSachBaiNop
                .Include(b => b.BaiTap)
                .FirstOrDefaultAsync(b => b.Id == req.BaiNopId);

            if (baiNop == null) return Json(new { success = false, message = "Không tìm thấy bài nộp" });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Không xác thực giáo viên" });

            if (!User.IsInRole("Admin") && baiNop.BaiTap != null && baiNop.BaiTap.LopId.HasValue)
            {
                var servesThisSubject = await _context.LopMonHocs
                    .AnyAsync(lm => lm.LopId == baiNop.BaiTap.LopId.Value && lm.MonHocId == baiNop.BaiTap.MonHocId && lm.GiaoVienId == user.Id);

                if (!servesThisSubject)
                {
                    return Json(new { success = false, message = "Bạn không được phân công giảng dạy môn học này cho lớp của học sinh." });
                }
            }

            // Đồng bộ sang bảng Điểm Số và Điểm Học Kỳ (ghi sổ)
            var hocSinhId = baiNop.HocSinhId;
            var monHocId = baiNop.BaiTap?.MonHocId;
            var loaiDiem = baiNop.BaiTap?.LoaiDiem ?? LoaiDiem.BaiTap;

            if (monHocId.HasValue && loaiDiem != LoaiDiem.BaiTap)
            {
                var hs = await _context.Users.FirstOrDefaultAsync(u => u.Id == hocSinhId);
                string namHoc = hs?.NamHoc;
                if (string.IsNullOrEmpty(namHoc))
                {
                    namHoc = DateTime.Now.Month >= 9
                        ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                        : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
                }
                int hocKy = (baiNop.BaiTap != null && baiNop.BaiTap.HocKy > 0) ? baiNop.BaiTap.HocKy : ((DateTime.Now.Month >= 8 || DateTime.Now.Month <= 1) ? 1 : 2);

                // Kiểm tra chốt điểm học kỳ
                var diemHK = await _context.DiemHocKys
                    .FirstOrDefaultAsync(d => d.HocSinhId == hocSinhId && d.MonHocId == monHocId.Value && d.NamHoc == namHoc && d.HocKy == hocKy);

                if (diemHK != null)
                {
                    if (loaiDiem == LoaiDiem.MiengKiemTra && diemHK.IsChotMieng)
                    {
                        return Json(new { success = false, message = $"Điểm miệng Học kỳ {hocKy} ({namHoc}) đã được chốt, không thể chỉnh sửa!" });
                    }
                    if (loaiDiem == LoaiDiem.GiuaKy && diemHK.IsChotGiuaKy)
                    {
                        return Json(new { success = false, message = $"Điểm giữa kỳ Học kỳ {hocKy} ({namHoc}) đã được chốt, không thể chỉnh sửa!" });
                    }
                    if (loaiDiem == LoaiDiem.CuoiKy && diemHK.IsChotCuoiKy)
                    {
                        return Json(new { success = false, message = $"Điểm cuối kỳ Học kỳ {hocKy} ({namHoc}) đã được chốt, không thể chỉnh sửa!" });
                    }
                }

                // Cập nhật DiemSo
                var diemSo = await _context.DiemSos
                    .FirstOrDefaultAsync(d => d.NguoiDungId == hocSinhId && d.MonHocId == monHocId.Value);

                if (diemSo == null)
                {
                    diemSo = new DiemSo
                    {
                        NguoiDungId = hocSinhId,
                        MonHocId = monHocId.Value,
                        NgayNhap = DateTime.Now
                    };
                    _context.DiemSos.Add(diemSo);
                }

                if (loaiDiem == LoaiDiem.GiuaKy) diemSo.DiemGiuaKy = req.Diem;
                else if (loaiDiem == LoaiDiem.CuoiKy) diemSo.DiemCuoiKy = req.Diem;
                else if (loaiDiem == LoaiDiem.MiengKiemTra)
                {
                    int cot = baiNop.BaiTap?.CotDiemMieng ?? 1;
                    if (cot == 2) diemSo.DiemMieng2 = req.Diem;
                    else if (cot == 3) diemSo.DiemMieng3 = req.Diem;
                    else if (cot == 4) diemSo.DiemMieng4 = req.Diem;
                    else diemSo.Diem = req.Diem; // cot == 1
                }

                diemSo.NgayCapNhat = DateTime.Now;
                diemSo.GiaoVienId = user.Id;

                // Cập nhật DiemHocKy
                if (diemHK == null)
                {
                    diemHK = new DiemHocKy
                    {
                        HocSinhId = hocSinhId,
                        MonHocId = monHocId.Value,
                        LopId = hs?.LopId,
                        NamHoc = namHoc,
                        HocKy = hocKy,
                        NgayNhap = DateTime.Now
                    };
                    _context.DiemHocKys.Add(diemHK);
                }

                if (loaiDiem == LoaiDiem.MiengKiemTra)
                {
                    int cot = baiNop.BaiTap?.CotDiemMieng ?? 1;
                    if (cot == 2) diemHK.DiemMieng2 = req.Diem;
                    else if (cot == 3) diemHK.DiemMieng3 = req.Diem;
                    else if (cot == 4) diemHK.DiemMieng4 = req.Diem;
                    else diemHK.DiemMieng1 = req.Diem; // cot == 1
                }
                else if (loaiDiem == LoaiDiem.GiuaKy)
                {
                    diemHK.DiemGiuaKy = req.Diem;
                }
                else if (loaiDiem == LoaiDiem.CuoiKy)
                {
                    diemHK.DiemCuoiKy = req.Diem;
                }

                diemHK.NgayCapNhat = DateTime.Now;
                diemHK.GiaoVienId = user.Id;

                // Tính điểm tổng kết
                if (diemHK.IsChotMieng && diemHK.IsChotGiuaKy && diemHK.IsChotCuoiKy)
                {
                    var listDiem = new List<double>();
                    if (diemHK.DiemMieng1.HasValue) listDiem.Add(diemHK.DiemMieng1.Value);
                    if (diemHK.DiemMieng2.HasValue) listDiem.Add(diemHK.DiemMieng2.Value);
                    if (diemHK.DiemMieng3.HasValue) listDiem.Add(diemHK.DiemMieng3.Value);
                    if (diemHK.DiemMieng4.HasValue) listDiem.Add(diemHK.DiemMieng4.Value);

                    if (diemHK.DiemGiuaKy.HasValue && diemHK.DiemCuoiKy.HasValue)
                    {
                        double avgMieng = listDiem.Any() ? listDiem.Average() : 0;
                        diemHK.DiemTongKet = Math.Round((avgMieng + diemHK.DiemGiuaKy.Value * 2 + diemHK.DiemCuoiKy.Value * 3) / 6, 1);

                        diemHK.XepLoai = diemHK.DiemTongKet >= 8.0 ? "Giỏi"
                                       : diemHK.DiemTongKet >= 6.5 ? "Khá"
                                       : diemHK.DiemTongKet >= 5.0 ? "Trung bình"
                                       : diemHK.DiemTongKet >= 3.5 ? "Yếu"
                                       : "Kém";
                    }
                }
                else
                {
                    diemHK.DiemTongKet = null;
                    diemHK.XepLoai = null;
                }
            }

            // Lưu điểm vào bài nộp
            baiNop.Diem = req.Diem;
            baiNop.TrangThai = TrangThaiBaiNop.ChamXong;
            baiNop.NgayCham = DateTime.Now;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public class ChamDiemRequest
        {
            public int BaiNopId { get; set; }
            public double? Diem { get; set; }
        }

        // Thêm bình luận bài giảng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuanBaiGiang(int baiGiangId, string noiDung, int? lopId, int? monHocId)
        {
            if (!string.IsNullOrWhiteSpace(noiDung))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var bl = new BinhLuan
                    {
                        NoiDung = noiDung.Trim(),
                        NgayTao = DateTime.Now,
                        NguoiDungId = user.Id,
                        BaiGiangId = baiGiangId
                    };
                    _context.DanhSachBinhLuan.Add(bl);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", new { lopId, monHocId });
        }

        // Thêm bình luận bài tập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuanBaiTap(int baiTapId, string noiDung, int? lopId, int? monHocId)
        {
            if (!string.IsNullOrWhiteSpace(noiDung))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var bl = new BinhLuan
                    {
                        NoiDung = noiDung.Trim(),
                        NgayTao = DateTime.Now,
                        NguoiDungId = user.Id,
                        BaiTapId = baiTapId
                    };
                    _context.DanhSachBinhLuan.Add(bl);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", new { lopId, monHocId });
        }

        // Trả lời bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraLoiBinhLuan(int parentId, string noiDung, int? lopId, int? monHocId)
        {
            if (!string.IsNullOrWhiteSpace(noiDung))
            {
                var user = await _userManager.GetUserAsync(User);
                var parent = await _context.DanhSachBinhLuan.FindAsync(parentId);
                if (user != null && parent != null)
                {
                    var reply = new BinhLuan
                    {
                        NoiDung = noiDung.Trim(),
                        NgayTao = DateTime.Now,
                        NguoiDungId = user.Id,
                        ParentId = parentId,
                        BaiGiangId = parent.BaiGiangId,
                        BaiTapId = parent.BaiTapId
                    };
                    _context.DanhSachBinhLuan.Add(reply);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index", new { lopId, monHocId });
        }
    }
}
