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

namespace LMS_THPT.Controllers
{
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

        // Feed - hiển thị các bài giảng / bài tập
        public async Task<IActionResult> Index(int? lopId, int? monHocId)
        {
            ViewBag.LopId = lopId;
            ViewBag.MonHocId = monHocId;

            if (!monHocId.HasValue)
            {
                ViewBag.BaiTaps = new List<BaiTap>();
                ViewBag.BinhLuanBaiGiang = new List<BinhLuan>();
                ViewBag.BinhLuanBaiTap = new List<BinhLuan>();
                return View(new List<BaiGiang>());
            }

            // ✅ BÀI GIẢNG - lọc theo monHocId và lopId (chỉ hiển thị bài của đúng lớp hoặc bài không gắn lớp)
            var postsQuery = _context.DanhSachBaiGiang
                .Include(b => b.TaiLieus)
                .Include(b => b.MonHoc)
                .Include(b => b.NguoiDung)
                .Where(b => b.MonHocId == monHocId && b.IsActive
                         && (b.LopId == null || b.LopId == lopId));

            var posts = await postsQuery
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            // ✅ BÀI TẬP - lọc theo monHocId và lopId để không hiển thị bài của lớp khác
            IQueryable<BaiTap> tapsQuery = _context.DanhSachBaiTap
                .Where(t => t.MonHocId == monHocId
                         && (t.LopId == null || t.LopId == lopId))
                .Include(x => x.NguoiDung);

            var taps = await tapsQuery
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            ViewBag.BaiTaps = taps;

            // ✅ COMMENT BÀI GIẢNG
            var baiGiangIds = posts.Select(p => p.Id).ToList();

            var binhLuanBaiGiang = await _context.DanhSachBinhLuan
                .Include(b => b.NguoiDung)
                .Include(b => b.Replies).ThenInclude(r => r.NguoiDung)
                .Where(b => b.BaiGiangId.HasValue
                         && baiGiangIds.Contains(b.BaiGiangId.Value)
                         && b.ParentId == null)
                .ToListAsync();

            // ✅ COMMENT BÀI TậP
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

        // Tạo bài giảng - GET
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public IActionResult CreateBaiGiang(int? lopId, int? monHocId)
        {
            ViewBag.LopId = lopId;
            ViewBag.MonHocId = monHocId;
            if (monHocId.HasValue)
            {
                var mon = _context.DanhSachMonHoc.Find(monHocId.Value);
                ViewBag.MonHoc = mon;
            }
            ViewBag.TatCaMonHoc = _context.DanhSachMonHoc.Where(m => m.IsActive).ToList();
            return View();
        }

        // Tạo bài giảng - POST
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBaiGiang(string title, string content, int? lopId, int? monHocId, IFormFile[] attachments)
        {
            if (!monHocId.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn môn học trước khi đăng bài.";
                return RedirectToAction("CreateBaiGiang", new { lopId, monHocId });
            }

            var user = await _userManager.GetUserAsync(User);

            var bai = new BaiGiang
            {
                TieuDe = string.IsNullOrWhiteSpace(title) ? "(Không có tiêu đề)" : title.Trim(),
                MoTa = content,
                MonHocId = monHocId.Value,
                IsActive = true,
                NgayTao = DateTime.Now,
                NguoiDungId = user.Id   // 🔥 QUAN TRỌNG
            };

            _context.DanhSachBaiGiang.Add(bai);
            await _context.SaveChangesAsync();

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
                    {
                        await file.CopyToAsync(fs);
                    }

                    var relPath = Path.Combine("/uploads/baigiang/", DateTime.Now.ToString("yyyyMMdd"), safe).Replace("\\", "/");

                    var ext = Path.GetExtension(orig).ToLowerInvariant();
                    var loai = LoaiTaiLieu.Khac;
                    if (ext == ".pdf") loai = LoaiTaiLieu.PDF;
                    else if (ext == ".mp4" || ext == ".avi" || ext == ".mov") loai = LoaiTaiLieu.Video;
                    else if (ext == ".ppt" || ext == ".pptx") loai = LoaiTaiLieu.Slide;

                    var tl = new TaiLieu
                    {
                        TenTaiLieu = orig,
                        DuongDanFile = relPath,
                        LoaiTaiLieu = loai,
                        KichThuocFile = file.Length,
                        NgayTao = DateTime.Now,
                        BaiGiangId = bai.Id,
                        MonHocId = monHocId
                    };

                    _context.DanhSachTaiLieu.Add(tl);
                }

                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Đã đăng bài giảng.";
            return RedirectToAction("Index", new { lopId, monHocId });
        }

        // Tạo bài tập - GET
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public IActionResult CreateBaiTap(int? lopId, int? monHocId)
        {
            ViewBag.LopId = lopId;
            ViewBag.MonHocId = monHocId;
            if (monHocId.HasValue)
            {
                var mon = _context.DanhSachMonHoc.Find(monHocId.Value);
                ViewBag.MonHoc = mon;
            }
            ViewBag.TatCaMonHoc = _context.DanhSachMonHoc.Where(m => m.IsActive).ToList();
            return View();
        }

        // Tạo bài tập - POST
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBaiTap(string title, string content, int? lopId, int? monHocId, string dueDate, string dueTime, IFormFile[] attachments)
        {
            if (!monHocId.HasValue)
            {
                TempData["Error"] = "Vui lòng chọn môn học để tạo bài tập.";
                return RedirectToAction("CreateBaiTap", new { lopId, monHocId });
            }

            DateTime hanNop = DateTime.Now.AddDays(7);
            if (!string.IsNullOrWhiteSpace(dueDate) && DateTime.TryParse(dueDate, out var d))
                hanNop = d.Date;

            if (!string.IsNullOrWhiteSpace(dueTime) && TimeSpan.TryParse(dueTime, out var t))
                hanNop = hanNop.Date + t;

            var user = await _userManager.GetUserAsync(User);

            var baitap = new BaiTap
            {
                TieuDe = string.IsNullOrWhiteSpace(title) ? "(Không có tiêu đề)" : title.Trim(),
                MoTa = content,
                MonHocId = monHocId.Value,
                HanNop = hanNop,
                DiemToiDa = 10,
                TrangThai = TrangThaiBaiTap.DangMo,
                NgayTao = DateTime.Now,
                NguoiDungId = user.Id // 🔥 THÊM DÒNG NÀY
            };

            _context.DanhSachBaiTap.Add(baitap);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã tạo bài tập.";
            return RedirectToAction("Index", new { lopId, monHocId });
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

            // Load bình luận của bài giảng này
            var binhLuans = await _context.DanhSachBinhLuan
                .Include(bl => bl.NguoiDung)
                .Include(bl => bl.Replies).ThenInclude(r => r.NguoiDung)
                .Where(bl => bl.BaiGiangId == id && bl.ParentId == null)
                .OrderByDescending(bl => bl.NgayTao)
                .ToListAsync();

            ViewBag.LopId    = lopId;
            ViewBag.BinhLuans = binhLuans;
            ViewBag.MonHocId  = bai.MonHocId;
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

            // Load bình luận của bài tập này
            var binhLuans = await _context.DanhSachBinhLuan
                .Include(bl => bl.NguoiDung)
                .Include(bl => bl.Replies).ThenInclude(r => r.NguoiDung)
                .Where(bl => bl.BaiTapId == id && bl.ParentId == null)
                .OrderByDescending(bl => bl.NgayTao)
                .ToListAsync();

            ViewBag.LopId    = lopId;
            ViewBag.BinhLuans = binhLuans;
            ViewBag.MonHocId  = bt.MonHocId;
            return View(bt);
        }

        // Xem danh sách nộp bài
        [Authorize(Roles = "GiaoVien,Admin")]
        [HttpGet]
        public async Task<IActionResult> Submissions(int baiTapId, int? lopId)
        {
            var bt = await _context.DanhSachBaiTap.FindAsync(baiTapId);
            if (bt == null) return NotFound();

            var subs = await _context.DanhSachBaiNop
                .Include(s => s.HocSinh)
                .Where(s => s.BaiTapId == baiTapId)
                .OrderByDescending(s => s.NgayNop)
                .ToListAsync();

            ViewBag.BaiTap = bt;
            ViewBag.LopId = lopId;
            return View(subs);
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
                    _context.DanhSachBinhLuan.Add(new BinhLuan
                    {
                        NoiDung     = noiDung.Trim(),
                        NgayTao     = DateTime.Now,
                        NguoiDungId = user.Id,
                        BaiGiangId  = baiGiangId
                    });
                    await _context.SaveChangesAsync();
                }
            }
            // Nếu gọi từ trang chi tiết, redirect về chi tiết; ngược lại về Index
            if (Request.Headers["Referer"].ToString().Contains("DetailsBaiGiang"))
                return RedirectToAction("DetailsBaiGiang", new { id = baiGiangId, lopId });
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
                    _context.DanhSachBinhLuan.Add(new BinhLuan
                    {
                        NoiDung     = noiDung.Trim(),
                        NgayTao     = DateTime.Now,
                        NguoiDungId = user.Id,
                        BaiTapId    = baiTapId
                    });
                    await _context.SaveChangesAsync();
                }
            }
            if (Request.Headers["Referer"].ToString().Contains("DetailsBaiTap"))
                return RedirectToAction("DetailsBaiTap", new { id = baiTapId, lopId });
            return RedirectToAction("Index", new { lopId, monHocId });
        }

        // Trả lời bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraLoiBinhLuan(int parentId, string noiDung, int? lopId, int? monHocId)
        {
            if (!string.IsNullOrWhiteSpace(noiDung))
            {
                var user   = await _userManager.GetUserAsync(User);
                var parent = await _context.DanhSachBinhLuan.FindAsync(parentId);
                if (user != null && parent != null)
                {
                    _context.DanhSachBinhLuan.Add(new BinhLuan
                    {
                        NoiDung     = noiDung.Trim(),
                        NgayTao     = DateTime.Now,
                        NguoiDungId = user.Id,
                        ParentId    = parentId,
                        BaiGiangId  = parent.BaiGiangId,
                        BaiTapId    = parent.BaiTapId
                    });
                    await _context.SaveChangesAsync();

                    // Redirect về đúng trang chi tiết nếu đang ở đó
                    if (parent.BaiGiangId.HasValue)
                        return RedirectToAction("DetailsBaiGiang", new { id = parent.BaiGiangId.Value, lopId });
                    if (parent.BaiTapId.HasValue)
                        return RedirectToAction("DetailsBaiTap", new { id = parent.BaiTapId.Value, lopId });
                }
            }
            return RedirectToAction("Index", new { lopId, monHocId });
        }

        // ===== TẢI XUỐNG TÀI LIỆU (có kiểm tra file tồn tại) =====
        [HttpGet]
        public async Task<IActionResult> DownloadTaiLieu(int id)
        {
            var tl = await _context.DanhSachTaiLieu.FindAsync(id);
            if (tl == null)
                return NotFound("Không tìm thấy tài liệu.");

            if (string.IsNullOrEmpty(tl.DuongDanFile))
                return Content("<html><body style='font-family:sans-serif;padding:40px'><h3>⚠️ Tài liệu này chưa có file đính kèm.</h3><a href='javascript:history.back()'>← Quay lại</a></body></html>", "text/html; charset=utf-8");

            // Chuyển URL thành đường dẫn vật lý
            var relativePath = tl.DuongDanFile.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var physicalPath = Path.Combine(_env.WebRootPath, relativePath);

            if (!System.IO.File.Exists(physicalPath))
                return Content($"<html><body style='font-family:sans-serif;padding:40px'><h3>⚠️ File không tồn tại trên máy chủ.</h3><p style='color:#666'>Đường dẫn: {tl.DuongDanFile}</p><p style='color:#888'>Tài liệu có thể đã bị xóa hoặc chưa được tải lên.</p><a href='javascript:history.back()' style='color:#ff6b00'>← Quay lại</a></body></html>", "text/html; charset=utf-8");

            var contentType = Path.GetExtension(tl.DuongDanFile).ToLower() switch {
                ".pdf"  => "application/pdf",
                ".ppt"  => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                ".doc"  => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".mp4"  => "video/mp4",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                _       => "application/octet-stream"
            };

            var fileBytes = await System.IO.File.ReadAllBytesAsync(physicalPath);
            var fileName  = Path.GetFileName(tl.TenTaiLieu ?? tl.DuongDanFile);
            return File(fileBytes, contentType, fileName);
        }
    }
}