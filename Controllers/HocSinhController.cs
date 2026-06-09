using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "HocSinh")]
    public class HocSinhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly IWebHostEnvironment _env;

        public HocSinhController(
            ApplicationDbContext context,
            UserManager<NguoiDung> userManager,
            SignInManager<NguoiDung> signInManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _env = env;
        }

        // ================= DASHBOARD =================
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Lịch học của lớp học sinh (nếu có LopId)
            IList<LichHoc> lichHoc = new List<LichHoc>();
            if (user.LopId.HasValue)
            {
                lichHoc = await _context.LichHocs
                    .Include(x => x.MonHoc)
                    .Include(x => x.GiaoVien)
                    .Where(x => x.LopId == user.LopId.Value)
                    .OrderBy(x => x.Thu)
                    .ThenBy(x => x.TietHoc)
                    .ToListAsync();
            }

            // Lấy danh sách MonHocId theo lớp của học sinh
            var monHocIdsOfLop = user.LopId.HasValue
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            // Bài tập chỉ thuộc các môn của lớp HS
            var tatCaBaiTap = await _context.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .Where(bt => monHocIdsOfLop.Contains(bt.MonHocId))
                .ToListAsync();

            // Bài đã nộp của học sinh
            var baiNop = await _context.DanhSachBaiNop
                .Where(x => x.HocSinhId == user.Id)
                .ToListAsync();

            int tong = tatCaBaiTap.Count;
            int daNop = baiNop.Count(bn => tatCaBaiTap.Any(bt => bt.Id == bn.BaiTapId));
            double progress = tong == 0 ? 0 : Math.Round((double)daNop / tong * 100, 1);

            // Bài chưa nộp
            var chuaNop = tatCaBaiTap
                .Where(bt => !baiNop.Any(bn => bn.BaiTapId == bt.Id))
                .OrderBy(x => x.HanNop)
                .ToList();

            // Bài sắp hết hạn (còn hạn nhưng ≤ 3 ngày)
            var sapHetHan = tatCaBaiTap
                .Where(x => x.HanNop > DateTime.Now && x.HanNop <= DateTime.Now.AddDays(3))
                .OrderBy(x => x.HanNop)
                .ToList();

            // Bài chưa nộp nhưng vẫn còn hạn
            var chuaNopConHan = tatCaBaiTap
                .Where(bt => !baiNop.Any(bn => bn.BaiTapId == bt.Id) && bt.HanNop > DateTime.Now)
                .OrderBy(x => x.HanNop)
                .ToList();

            // Điểm trung bình (chỉ tính bài đã chấm)
            var daDuocCham = baiNop.Where(x => x.Diem.HasValue).ToList();
            double? diemTB = daDuocCham.Count > 0
                ? Math.Round(daDuocCham.Average(x => x.Diem!.Value), 2)
                : null;

            ViewBag.Progress = progress;
            ViewBag.Tong = tong;
            ViewBag.DaNop = daNop;
            ViewBag.ChuaNop = chuaNop;
            ViewBag.SapHetHan = sapHetHan;
            ViewBag.ChuaNopConHan = chuaNopConHan;
            ViewBag.LichHoc = lichHoc;
            ViewBag.DiemTB = diemTB;
            ViewBag.SoBaiDaCham = daDuocCham.Count;
            ViewBag.User = user;

            return View();
        }

        // ================= BÀI GIẢNG =================
        public async Task<IActionResult> BaiGiang(int? monHocId, string? q)
        {
            var user = await _userManager.GetUserAsync(User);

            // Lấy danh sách MonHocId thuộc lớp của học sinh
            var monHocIdsOfLop = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            var query = _context.DanhSachBaiGiang
                .Include(x => x.MonHoc)
                .Include(x => x.NguoiDung)
                .Where(x => x.IsActive && monHocIdsOfLop.Contains(x.MonHocId) && (x.LopId == null || x.LopId == user.LopId)); // ✅ Lọc theo lớp

            if (monHocId.HasValue)
                query = query.Where(x => x.MonHocId == monHocId.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.TieuDe.Contains(q) || (x.MoTa != null && x.MoTa.Contains(q)));

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();

            // Chỉ hiển thị môn học thuộc lớp HS trong bộ lọc
            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.IsActive && monHocIdsOfLop.Contains(m.Id))
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            ViewBag.MonHocs = monHocs;
            ViewBag.SelectedMonHoc = monHocId;
            ViewBag.SearchQuery = q;

            return View(data);
        }

        // ================= CHI TIẾT BÀI GIẢNG =================
        public async Task<IActionResult> ChiTietBaiGiang(int id)
        {
            var baiGiang = await _context.DanhSachBaiGiang
                .Include(x => x.MonHoc)
                .Include(x => x.NguoiDung)
                .Include(x => x.TaiLieus)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (baiGiang == null) return NotFound();

            // Load bình luận
            var binhLuans = await _context.DanhSachBinhLuan
                .Include(bl => bl.NguoiDung)
                .Include(bl => bl.Replies).ThenInclude(r => r.NguoiDung)
                .Where(bl => bl.BaiGiangId == id && bl.ParentId == null)
                .OrderByDescending(bl => bl.NgayTao)
                .ToListAsync();

            ViewBag.BinhLuans = binhLuans;
            return View(baiGiang);
        }

        // ================= TÀI LIỆU =================
        public async Task<IActionResult> TaiLieu(int? monHocId, LoaiTaiLieu? loai, string? q)
        {
            var user = await _context.Users
                .Include(u => u.Lop).ThenInclude(l => l!.Khoi)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            // Lấy danh sách MonHocId thuộc lớp của học sinh
            var monHocIdsOfLop = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            var query = _context.DanhSachTaiLieu
                .Include(x => x.MonHoc)
                .Include(x => x.BaiGiang)
                .Where(x => x.MonHocId.HasValue && monHocIdsOfLop.Contains(x.MonHocId.Value));

            if (monHocId.HasValue)
                query = query.Where(x => x.MonHocId == monHocId.Value);

            if (loai.HasValue)
                query = query.Where(x => x.LoaiTaiLieu == loai.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.TenTaiLieu.Contains(q));

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();

            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.IsActive && monHocIdsOfLop.Contains(m.Id))
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            ViewBag.MonHocs = monHocs;
            ViewBag.SelectedMonHoc = monHocId;
            ViewBag.SelectedLoai = loai;
            ViewBag.SearchQuery = q;

            return View(data);
        }

        // ================= BÀI TẬP =================
        public async Task<IActionResult> BaiTap(int? monHocId, string? trangThai)
        {
            var user = await _userManager.GetUserAsync(User);

            // Lấy danh sách MonHocId thuộc lớp của học sinh
            var monHocIdsOfLop = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            var query = _context.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .Where(x => monHocIdsOfLop.Contains(x.MonHocId) && (x.LopId == null || x.LopId == user.LopId)); // ✅ Lọc theo lớp

            if (monHocId.HasValue)
                query = query.Where(x => x.MonHocId == monHocId.Value);

            var data = await query.OrderBy(x => x.HanNop).ToListAsync();

            var baiNop = await _context.DanhSachBaiNop
                .Where(x => x.HocSinhId == user!.Id)
                .ToListAsync();

            // Lọc theo trạng thái nộp bài (frontend filter)
            var now = DateTime.Now;
            if (trangThai == "chuaNop")
                data = data.Where(bt => !baiNop.Any(bn => bn.BaiTapId == bt.Id)).ToList();
            else if (trangThai == "daNop")
                data = data.Where(bt => baiNop.Any(bn => bn.BaiTapId == bt.Id)).ToList();
            else if (trangThai == "sapHetHan")
                data = data.Where(bt => bt.HanNop > now && bt.HanNop <= now.AddDays(3)).ToList();
            else if (trangThai == "hetHan")
                data = data.Where(bt => bt.HanNop <= now).ToList();

            // Chỉ hiển thị môn học thuộc lớp HS trong bộ lọc
            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.IsActive && monHocIdsOfLop.Contains(m.Id))
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            ViewBag.BaiNop = baiNop;
            ViewBag.MonHocs = monHocs;
            ViewBag.SelectedMonHoc = monHocId;
            ViewBag.TrangThai = trangThai;

            return View(data);
        }

        // ================= CHI TIẾT BÀI TẬP =================
        public async Task<IActionResult> ChiTietBaiTap(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var baiTap = await _context.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .Include(x => x.NguoiDung)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (baiTap == null) return NotFound();

            // Kiểm tra bài tập thuộc lớp HS
            var monHocIdsOfLop = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            if (!monHocIdsOfLop.Contains(baiTap.MonHocId))
                return Forbid();

            // Bài đã nộp
            var daNop = await _context.DanhSachBaiNop
                .FirstOrDefaultAsync(bn => bn.BaiTapId == id && bn.HocSinhId == user!.Id);

            // Load bình luận
            var binhLuans = await _context.DanhSachBinhLuan
                .Include(bl => bl.NguoiDung)
                .Include(bl => bl.Replies).ThenInclude(r => r.NguoiDung)
                .Where(bl => bl.BaiTapId == id && bl.ParentId == null)
                .OrderByDescending(bl => bl.NgayTao)
                .ToListAsync();

            ViewBag.BinhLuans = binhLuans;
            ViewBag.DaNop = daNop;
            return View(baiTap);
        }

        // ================= BÌNH LUẬN (HỌC SINH) =================

        // Thêm bình luận bài giảng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuanBaiGiang(int baiGiangId, string noiDung)
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
            return RedirectToAction("ChiTietBaiGiang", new { id = baiGiangId });
        }

        // Thêm bình luận bài tập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemBinhLuanBaiTap(int baiTapId, string noiDung)
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
            return RedirectToAction("ChiTietBaiTap", new { id = baiTapId });
        }

        // Trả lời bình luận
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TraLoiBinhLuan(int parentId, int? baiGiangId, int? baiTapId, string noiDung)
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

                    if (parent.BaiGiangId.HasValue)
                        return RedirectToAction("ChiTietBaiGiang", new { id = parent.BaiGiangId.Value });
                    if (parent.BaiTapId.HasValue)
                        return RedirectToAction("ChiTietBaiTap", new { id = parent.BaiTapId.Value });
                }
            }
            return RedirectToAction("Index");
        }

        // ================= NỘP BÀI (GET) =================
        public async Task<IActionResult> NopBai(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var baitap = await _context.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (baitap == null) return NotFound();

            // ✅ Validate: bài tập phải thuộc lớp của học sinh
            var monHocIdsOfLop = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            if (!monHocIdsOfLop.Contains(baitap.MonHocId))
            {
                TempData["error"] = "Bài tập này không thuộc lớp của bạn.";
                return RedirectToAction("BaiTap");
            }

            // Kiểm tra nếu bài đã đóng
            if (baitap.TrangThai == TrangThaiBaiTap.DaDong)
            {
                TempData["error"] = "Bài tập này đã đóng, không thể nộp.";
                return RedirectToAction("BaiTap");
            }

            // Lấy bài nộp cũ (nếu có)
            var baiNopCu = await _context.DanhSachBaiNop
                .FirstOrDefaultAsync(x => x.BaiTapId == id && x.HocSinhId == user!.Id);

            ViewBag.BaiNopCu = baiNopCu;

            return View(baitap);
        }

        // ================= NỘP BÀI (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopBai(int BaiTapId, string? NoiDung, string? GhiChu, IFormFile? file)
        {
            var user = await _userManager.GetUserAsync(User);
            var baitap = await _context.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .FirstOrDefaultAsync(x => x.Id == BaiTapId);

            if (baitap == null) return NotFound();

            // Validate: phải có nội dung hoặc file
            if (string.IsNullOrWhiteSpace(NoiDung) && (file == null || file.Length == 0))
            {
                TempData["error"] = "Vui lòng nhập nội dung hoặc đính kèm file bài làm.";
                return RedirectToAction("NopBai", new { id = BaiTapId });
            }

            var existing = await _context.DanhSachBaiNop
                .FirstOrDefaultAsync(x => x.BaiTapId == BaiTapId && x.HocSinhId == user!.Id);

            string? filePath = null;

            if (file != null && file.Length > 0)
            {
                // Giới hạn kích thước file 20MB
                if (file.Length > 20 * 1024 * 1024)
                {
                    TempData["error"] = "File quá lớn. Vui lòng chọn file dưới 20MB.";
                    return RedirectToAction("NopBai", new { id = BaiTapId });
                }

                var folder = Path.Combine(_env.WebRootPath, "uploads", "bainop");
                Directory.CreateDirectory(folder);

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var allowedExts = new[] { ".pdf", ".doc", ".docx", ".png", ".jpg", ".jpeg", ".zip", ".txt" };
                if (!allowedExts.Contains(ext))
                {
                    TempData["error"] = "Định dạng file không được phép. Chỉ chấp nhận: PDF, DOC, DOCX, PNG, JPG, ZIP, TXT.";
                    return RedirectToAction("NopBai", new { id = BaiTapId });
                }

                var fileName = Guid.NewGuid() + ext;
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                filePath = "/uploads/bainop/" + fileName;
            }

            var trangThai = baitap.HanNop < DateTime.Now
                ? TrangThaiBaiNop.NopTre
                : TrangThaiBaiNop.DaNop;

            if (existing != null)
            {
                existing.NoiDung = NoiDung;
                existing.GhiChu = GhiChu;
                existing.DuongDanFile = filePath ?? existing.DuongDanFile;
                existing.NgayNop = DateTime.Now;
                existing.TrangThai = trangThai;
                // Reset điểm khi nộp lại
                existing.Diem = null;
                existing.NhanXet = null;
                existing.NgayCham = null;

                _context.Update(existing);
                TempData["msg"] = "✅ Nộp lại bài thành công!";
            }
            else
            {
                var bainop = new BaiNop
                {
                    BaiTapId = BaiTapId,
                    HocSinhId = user!.Id,
                    NoiDung = NoiDung,
                    GhiChu = GhiChu,
                    DuongDanFile = filePath,
                    TrangThai = trangThai,
                    NgayNop = DateTime.Now
                };

                _context.DanhSachBaiNop.Add(bainop);
                TempData["msg"] = "✅ Nộp bài thành công!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("BaiTap");
        }

        // ================= XEM ĐIỂM =================
        public async Task<IActionResult> Diem(int? monHocId, string? namHoc, int? hocKy)
        {
            var user = await _context.Users
                .Include(u => u.Lop).ThenInclude(l => l!.Khoi)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            // ---- Điểm bài nộp (hiện tại) ----
            var queryBaiNop = _context.DanhSachBaiNop
                .Include(x => x.BaiTap)
                    .ThenInclude(x => x!.MonHoc)
                .Where(x => x.HocSinhId == user!.Id);

            if (monHocId.HasValue)
                queryBaiNop = queryBaiNop.Where(x => x.BaiTap!.MonHocId == monHocId.Value);

            var dataBaiNop = await queryBaiNop
                .OrderByDescending(x => x.NgayNop)
                .ToListAsync();

            // Lấy danh sách MonHocId thuộc lớp của học sinh
            var monHocIdsOfLop = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            // ---- Danh sách năm học có điểm (toàn bộ) ----
            var dsNamHocAll = await _context.DiemHocKys
                .Where(x => x.HocSinhId == user!.Id)
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            // Giới hạn theo cấp lớp
            var namHocHienTai = user?.NamHoc ?? "2024-2025";
            var tenKhoi = user?.Lop?.Khoi?.TenKhoi ?? "Khối 10";
            var dsNamHoc = LimitNamHocByKhoi(dsNamHocAll, namHocHienTai, tenKhoi);

            if (!dsNamHoc.Contains(namHocHienTai))
            {
                dsNamHoc.Add(namHocHienTai);
                dsNamHoc = dsNamHoc.OrderByDescending(x => x).ToList();
            }

            // Nếu namHoc truyền vào nhưng không được phép hoặc rỗng → mặc định là năm học hiện tại
            if (string.IsNullOrEmpty(namHoc) || !dsNamHoc.Contains(namHoc))
            {
                namHoc = namHocHienTai;
            }

            if (!hocKy.HasValue)
            {
                hocKy = 1;
            }

            // ---- Điểm học kỳ chính thức (DiemHocKy) ----
            var queryDiemHK = _context.DiemHocKys
                .Include(x => x.MonHoc)
                .Include(x => x.Lop)
                .Where(x => x.HocSinhId == user!.Id);

            queryDiemHK = queryDiemHK.Where(x => x.NamHoc != namHocHienTai || monHocIdsOfLop.Contains(x.MonHocId));

            if (monHocId.HasValue)
                queryDiemHK = queryDiemHK.Where(x => x.MonHocId == monHocId.Value);
            if (!string.IsNullOrEmpty(namHoc))
                queryDiemHK = queryDiemHK.Where(x => x.NamHoc == namHoc);
            if (hocKy.HasValue)
                queryDiemHK = queryDiemHK.Where(x => x.HocKy == hocKy.Value);

            var dataDiemHK = await queryDiemHK
                .OrderByDescending(x => x.NamHoc)
                .ThenBy(x => x.HocKy)
                .ThenBy(x => x.MonHoc!.TenMonHoc)
                .ToListAsync();

            // Đảm bảo tất cả môn học học sinh đang học đều có dòng trong bảng điểm của năm học được chọn
            if (!string.IsNullOrEmpty(namHoc))
            {
                var listMonHocClass = await _context.DanhSachMonHoc
                    .Where(m => m.IsActive && monHocIdsOfLop.Contains(m.Id))
                    .ToListAsync();

                var hocKysToPopulate = hocKy.HasValue ? new[] { hocKy.Value } : new[] { 1, 2 };

                foreach (var hkVal in hocKysToPopulate)
                {
                    foreach (var mon in listMonHocClass)
                    {
                        var exists = dataDiemHK.Any(x => x.MonHocId == mon.Id && x.NamHoc == namHoc && x.HocKy == hkVal);
                        if (!exists)
                        {
                            dataDiemHK.Add(new DiemHocKy
                            {
                                HocSinhId = user!.Id,
                                HocSinh = user,
                                MonHocId = mon.Id,
                                MonHoc = mon,
                                LopId = user.LopId,
                                NamHoc = namHoc,
                                HocKy = hkVal,
                                IsChotMieng = false,
                                IsChotGiuaKy = false,
                                IsChotCuoiKy = false
                            });
                        }
                    }
                }

                // Sắp xếp lại
                dataDiemHK = dataDiemHK
                    .OrderByDescending(x => x.NamHoc)
                    .ThenBy(x => x.HocKy)
                    .ThenBy(x => x.MonHoc!.TenMonHoc)
                    .ToList();
            }

            // Thống kê được tính ở phần ViewBag dưới

            // ---- Thống kê bài nộp ----
            var daCham = dataBaiNop.Where(x => x.Diem.HasValue).ToList();
            double? diemTBBaiNop = daCham.Count > 0 ? Math.Round(daCham.Average(x => x.Diem!.Value), 2) : null;

            // ---- Thống kê điểm HK (Chỉ tính điểm đã chốt) ----
            var dsTongKet = dataDiemHK
                .Where(x => x.IsChotMieng && x.IsChotGiuaKy && x.IsChotCuoiKy && x.DiemTongKet.HasValue)
                .ToList();
            double? diemTBHK = dsTongKet.Count > 0
                ? Math.Round(dsTongKet.Average(x => x.DiemTongKet!.Value), 2)
                : null;

            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.IsActive && monHocIdsOfLop.Contains(m.Id))
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();



            ViewBag.DiemTBBaiNop = diemTBBaiNop;
            ViewBag.DiemTBHK = diemTBHK;
            ViewBag.SoDaCham = daCham.Count;
            ViewBag.SoChuaCham = dataBaiNop.Count(x => !x.Diem.HasValue);
            ViewBag.MonHocs = monHocs;
            ViewBag.SelectedMonHoc = monHocId;
            ViewBag.DsNamHoc = dsNamHoc;
            ViewBag.SelectedNamHoc = namHoc;
            ViewBag.SelectedHocKy = hocKy;
            ViewBag.NamHocHienTai = namHocHienTai;
            ViewBag.DataDiemHK = dataDiemHK;
            ViewBag.TenKhoi = tenKhoi;
            // ---- Tính TB cả năm khi không lọc theo HK ----
            if (!hocKy.HasValue && !string.IsNullOrEmpty(namHoc))
            {
                var monIds = dataDiemHK.Select(x => x.MonHocId).Distinct().ToList();
                var tbCaNam = monIds.Select(mid =>
                {
                    var d1 = dataDiemHK.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 1);
                    var d2 = dataDiemHK.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 2);
                    double? tk1 = (d1 != null && d1.IsChotMieng && d1.IsChotGiuaKy && d1.IsChotCuoiKy) ? d1.DiemTongKet : null;
                    double? tk2 = (d2 != null && d2.IsChotMieng && d2.IsChotGiuaKy && d2.IsChotCuoiKy) ? d2.DiemTongKet : null;
                    double? tbNam = (tk1.HasValue && tk2.HasValue) ? Math.Round((tk1.Value + tk2.Value * 2) / 3, 2)
                                  : (tk1 ?? tk2);
                    string? xepLoai = tbNam >= 8.0 ? "Giỏi"
                                    : tbNam >= 6.5 ? "Khá"
                                    : tbNam >= 5.0 ? "Trung bình"
                                    : tbNam >= 3.5 ? "Yếu"
                                    : tbNam.HasValue ? "Kém" : null;
                    return new
                    {
                        MonHocId = mid,
                        TenMonHoc = d1?.MonHoc?.TenMonHoc ?? d2?.MonHoc?.TenMonHoc ?? "",
                        TkHK1 = tk1, TkHK2 = tk2, TbNam = tbNam, XepLoai = xepLoai
                    };
                }).OrderBy(x => x.TenMonHoc).ToList();
                ViewBag.TbCaNam = tbCaNam;
            }

            return View(dataBaiNop);
        }

        // ================= ĐIỂM THEO NĂM HỌC (lịch sử) =================
        public async Task<IActionResult> DiemNamHoc(string? namHoc, int? monHocId)
        {
            var user = await _context.Users
                .Include(u => u.Lop).ThenInclude(l => l!.Khoi)
                .FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);

            // Giới hạn theo cấp lớp
            var namHocHT = user?.NamHoc ?? "2024-2025";
            var tenKhoi = user?.Lop?.Khoi?.TenKhoi ?? "Khối 10";
            var dsNamHoc = GetPastNamHocByKhoi(namHocHT, tenKhoi);

            // Nếu namHoc truyền vào nhưng không được phép → reset
            if (!string.IsNullOrEmpty(namHoc) && !dsNamHoc.Contains(namHoc))
                namHoc = null;

            // Năm học được chọn (mặc định là năm học lịch sử mới nhất)
            string? selectedNamHoc = namHoc ?? dsNamHoc.FirstOrDefault();



            var dataDiem = new List<DiemHocKy>();
            if (!string.IsNullOrEmpty(selectedNamHoc))
            {
                var queryDiem = _context.DiemHocKys
                    .Include(x => x.MonHoc)
                    .Include(x => x.Lop)
                    .Where(x => x.HocSinhId == user!.Id && x.NamHoc == selectedNamHoc);

                if (monHocId.HasValue)
                    queryDiem = queryDiem.Where(x => x.MonHocId == monHocId.Value);

                dataDiem = await queryDiem
                    .OrderBy(x => x.HocKy)
                    .ThenBy(x => x.MonHoc!.TenMonHoc)
                    .ToListAsync();
                // Không thêm placeholder — chỉ hiển thị dữ liệu thực tế từ DB
            }

            // Group theo HK
            var hk1 = dataDiem.Where(x => x.HocKy == 1).ToList();
            var hk2 = dataDiem.Where(x => x.HocKy == 2).ToList();

            // Tính điểm TB cả năm theo môn
            var monIds = dataDiem.Select(x => x.MonHocId).Distinct().ToList();
            var tbCaNam = monIds.Select(mid =>
            {
                var d1 = dataDiem.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 1);
                var d2 = dataDiem.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 2);
                // Dùng DiemTongKet trực tiếp (không cần IsChot) vì đây là dữ liệu lịch sử
                double? tk1 = d1?.DiemTongKet;
                double? tk2 = d2?.DiemTongKet;
                double? tbNam = (tk1.HasValue && tk2.HasValue) ? Math.Round((tk1.Value + tk2.Value * 2) / 3, 2)
                              : (tk1 ?? tk2);
                string? xepLoai = tbNam >= 8.0 ? "Giỏi"
                                : tbNam >= 6.5 ? "Khá"
                                : tbNam >= 5.0 ? "Trung bình"
                                : tbNam >= 3.5 ? "Yếu"
                                : tbNam.HasValue ? "Kém" : null;
                return new
                {
                    MonHocId = mid,
                    TenMonHoc = d1?.MonHoc?.TenMonHoc ?? d2?.MonHoc?.TenMonHoc,
                    TkHK1 = tk1,
                    TkHK2 = tk2,
                    TbNam = tbNam,
                    XepLoai = xepLoai
                };
            }).ToList();

            var monHocIds4Filter = dataDiem.Select(x => x.MonHocId).Distinct().ToList();
            var monHocs = await _context.DanhSachMonHoc
                .Where(m => monHocIds4Filter.Contains(m.Id))
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            ViewBag.DsNamHoc = dsNamHoc;
            ViewBag.SelectedNamHoc = selectedNamHoc;
            ViewBag.SelectedMonHoc = monHocId;
            ViewBag.MonHocs = monHocs;
            ViewBag.HK1 = hk1;
            ViewBag.HK2 = hk2;
            ViewBag.TbCaNam = tbCaNam;
            ViewBag.User = user;

            return View(dataDiem);
        }

        // ================= MÔN HỌC =================
        public async Task<IActionResult> MonHoc()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            List<LopMonHoc> lopMonHocs = new();
            if (user.LopId.HasValue)
            {
                lopMonHocs = await _context.LopMonHocs
                    .Include(x => x.MonHoc).ThenInclude(m => m!.GiaoVien)
                    .Include(x => x.GiaoVien)
                    .Where(x => x.LopId == user.LopId.Value)
                    .OrderBy(x => x.MonHoc!.TenMonHoc)
                    .ToListAsync();
            }

            // Nếu chưa có lớp HOẶC lớp chưa được phân môn học → fallback hiển thị tất cả môn active
            if (lopMonHocs.Count == 0)
            {
                var allMons = await _context.DanhSachMonHoc
                    .Include(m => m.GiaoVien)
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.TenMonHoc)
                    .ToListAsync();
                lopMonHocs = allMons.Select(m => new LopMonHoc
                {
                    MonHocId = m.Id,
                    MonHoc   = m
                }).ToList();
            }

            // Eager load Lop for badge display
            if (user.LopId.HasValue)
            {
                await _context.Entry(user)
                    .Reference(u => u.Lop)
                    .LoadAsync();
            }

            ViewBag.User = user;
            return View(lopMonHocs);
        }

        // ================= CHI TIẾT MÔN HỌC =================
        public async Task<IActionResult> ChiTietMonHoc(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var monHoc = await _context.DanhSachMonHoc
                .Include(m => m.GiaoVien)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (monHoc == null) return NotFound();

            // GV phụ trách môn theo lớp (LopMonHoc)
            LopMonHoc? lopMonHoc = null;
            if (user.LopId.HasValue)
            {
                lopMonHoc = await _context.LopMonHocs
                    .Include(x => x.GiaoVien)
                    .FirstOrDefaultAsync(x => x.LopId == user.LopId.Value && x.MonHocId == id);
            }

            // Bài giảng mới nhất của môn (hiển thị tối đa 5 bài)
            var baiGiangs = await _context.DanhSachBaiGiang
                .Include(x => x.NguoiDung)
                .Where(x => x.MonHocId == id && x.IsActive)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            // Toàn bộ bài tập của môn (không giới hạn để thống kê đúng)
            var baiTaps = await _context.DanhSachBaiTap
                .Where(x => x.MonHocId == id)
                .OrderBy(x => x.HanNop)
                .ToListAsync();

            // Bài nộp của học sinh cho các bài tập này
            var baiTapIds = baiTaps.Select(bt => bt.Id).ToList();
            var baiNop = await _context.DanhSachBaiNop
                .Where(x => x.HocSinhId == user.Id && baiTapIds.Contains(x.BaiTapId))
                .ToListAsync();

            // Tài liệu của môn (hiển thị tối đa 5 tài liệu)
            var taiLieus = await _context.DanhSachTaiLieu
                .Where(x => x.MonHocId == id)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.BaiGiangs  = baiGiangs;
            ViewBag.BaiTaps    = baiTaps;
            ViewBag.BaiNop     = baiNop;
            ViewBag.TaiLieus   = taiLieus;
            ViewBag.LopMonHoc  = lopMonHoc;
            ViewBag.User       = user;

            return View(monHoc);
        }

        // ================= LỊCH HỌC =================
        public async Task<IActionResult> LichHoc()
        {
            var user = await _userManager.GetUserAsync(User);

            IList<LichHoc> lichHoc = new List<LichHoc>();
            if (user?.LopId != null)
            {
                lichHoc = await _context.LichHocs
                    .Include(x => x.MonHoc)
                    .Include(x => x.GiaoVien)
                    .Include(x => x.Lop)
                    .Where(x => x.LopId == user.LopId.Value && !x.IsHocBu)
                    .OrderBy(x => x.Thu)
                    .ThenBy(x => x.TietHoc)
                    .ToListAsync();
            }

            ViewBag.LopTen = user?.Lop?.TenLop ?? "Chưa có lớp";

            return View(lichHoc);
        }

        // ================= SỬA THÔNG TIN CÁ NHÂN (GET) =================
        public async Task<IActionResult> SuaThongTin()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (user.LopId.HasValue)
                await _context.Entry(user).Reference(u => u.Lop).LoadAsync();

            return View(user);
        }

        // ================= SỬA THÔNG TIN CÁ NHÂN (POST) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaThongTin(
            string HoTen,
            DateTime? NgaySinh,
            string? GioiTinh,
            string? DiaChi,
            IFormFile? AnhDaiDienFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Validate
            if (string.IsNullOrWhiteSpace(HoTen))
            {
                TempData["error"] = "Họ và tên không được để trống.";
                return RedirectToAction("SuaThongTin");
            }

            user.HoTen = HoTen.Trim();
            user.NgaySinh = NgaySinh;
            user.GioiTinh = GioiTinh;
            user.DiaChi = DiaChi?.Trim();
            user.NgayCapNhat = DateTime.Now;

            // Xử lý upload ảnh đại diện
            if (AnhDaiDienFile != null && AnhDaiDienFile.Length > 0)
            {
                // Giới hạn 5MB
                if (AnhDaiDienFile.Length > 5 * 1024 * 1024)
                {
                    TempData["error"] = "Ảnh quá lớn. Vui lòng chọn ảnh dưới 5MB.";
                    return RedirectToAction("SuaThongTin");
                }

                var ext = Path.GetExtension(AnhDaiDienFile.FileName).ToLowerInvariant();
                var allowedExts = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                if (!allowedExts.Contains(ext))
                {
                    TempData["error"] = "Chỉ chấp nhận ảnh định dạng JPG, PNG, GIF hoặc WEBP.";
                    return RedirectToAction("SuaThongTin");
                }

                var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
                Directory.CreateDirectory(folder);

                var fileName = $"hs_{user.Id[..8]}_{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                    await AnhDaiDienFile.CopyToAsync(stream);

                user.AnhDaiDien = "/uploads/avatars/" + fileName;

                // Cập nhật claim AnhDaiDien để avatar cập nhật ngay
                var allClaims = (await _userManager.GetClaimsAsync(user)).ToList();
                var oldClaim = allClaims.FirstOrDefault(c => c.Type == "AnhDaiDien");
                if (oldClaim != null)
                    await _userManager.RemoveClaimAsync(user, oldClaim);
                await _userManager.AddClaimAsync(user,
                    new Claim("AnhDaiDien", user.AnhDaiDien));
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["error"] = "Cập nhật thất bại: " +
                    string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToAction("SuaThongTin");
            }

            // Refresh cookie để claim mới có hiệu lực ngay
            await _signInManager.RefreshSignInAsync(user);

            TempData["msg"] = "✅ Cập nhật thông tin thành công!";
            return RedirectToAction("SuaThongTin");
        }

        // ─────────────────────────────────────────
        // HELPERS: giới hạn năm học theo cấp lớp
        // ─────────────────────────────────────────
        private static List<string> LimitNamHocByKhoi(List<string> dsNamHoc, string namHocHienTai, string tenKhoi)
        {
            int soNamDuocXem = GetMaxNamLuiByKhoi(tenKhoi);
            return dsNamHoc
                .Where(n =>
                {
                    if (n == namHocHienTai) return true;
                    var startYear = n.Split('-').FirstOrDefault();
                    var curStartYear = namHocHienTai.Split('-').FirstOrDefault();
                    if (int.TryParse(startYear, out int y) && int.TryParse(curStartYear, out int cy))
                        return (cy - y) <= soNamDuocXem && y < cy;
                    return false;
                })
                .OrderByDescending(n => n)
                .ToList();
        }

        private static List<string> GetPastNamHocByKhoi(string namHocHienTai, string tenKhoi)
        {
            var list = new List<string>();
            var startYearStr = namHocHienTai.Split('-').FirstOrDefault();
            if (int.TryParse(startYearStr, out int cy))
            {
                int maxBack = GetMaxNamLuiByKhoi(tenKhoi); // 10→0, 11→1, 12→2
                for (int i = 1; i <= maxBack; i++)
                {
                    int sy = cy - i;
                    int ey = sy + 1;
                    list.Add($"{sy}-{ey}");
                }
            }
            return list;
        }

        private static int GetMaxNamLuiByKhoi(string tenKhoi)
        {
            var parts = tenKhoi.Trim().Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int soKhoi))
                return Math.Max(0, soKhoi - 10); // 10→0, 11→1, 12→2
            return 0;
        }
    }
}