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

            // ✅ BÀI GIẢNG - lọc theo cả monHocId và lopId (nếu có)
            // Bài giảng hiện chưa lưu lopId trực tiếp, lọc theo monHocId là đủ cho feed của giáo viên
            var postsQuery = _context.DanhSachBaiGiang
                .Include(b => b.TaiLieus)
                .Include(b => b.MonHoc)
                .Include(b => b.NguoiDung)
                .Where(b => b.MonHocId == monHocId && b.IsActive);

            var posts = await postsQuery
                .OrderByDescending(b => b.NgayTao)
                .ToListAsync();

            // ✅ BÀI TẬP - lọc theo monHocId, và nếu có lopId thì lọc thêm theo GV phụ trách lớp đó
            IQueryable<BaiTap> tapsQuery = _context.DanhSachBaiTap
                .Where(t => t.MonHocId == monHocId)
                .Include(x => x.NguoiDung);

            // Nếu có lopId, chỉ lấy bài tập của giáo viên dạy lớp đó (tránh GV cùng môn thấy bài của nhau)
            if (lopId.HasValue)
            {
                var gvCuaLop = await _context.LopMonHocs
                    .Where(lm => lm.LopId == lopId.Value && lm.MonHocId == monHocId.Value)
                    .Select(lm => lm.GiaoVienId)
                    .FirstOrDefaultAsync();

                if (gvCuaLop != null)
                    tapsQuery = tapsQuery.Where(t => t.NguoiDungId == gvCuaLop);
            }

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
        // Chi tiết bài giảng
        [HttpGet]
        public async Task<IActionResult> DetailsBaiGiang(int id, int? lopId)
        {
            var bai = await _context.DanhSachBaiGiang
                .Include(b => b.TaiLieus)
                .Include(b => b.MonHoc)
                .Include(b => b.NguoiDung) // ✅ THÊM DÒNG NÀY
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
                .Include(b => b.NguoiDung) // ✅ THÊM DÒNG NÀY
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bt == null) return NotFound();

            ViewBag.LopId = lopId;
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