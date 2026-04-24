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
                .Where(x => x.IsActive && monHocIdsOfLop.Contains(x.MonHocId)); // ✅ Lọc theo lớp

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

            return View(baiGiang);
        }

        // ================= TÀI LIỆU =================
        public async Task<IActionResult> TaiLieu(int? monHocId, LoaiTaiLieu? loai, string? q)
        {
            var query = _context.DanhSachTaiLieu
                .Include(x => x.MonHoc)
                .Include(x => x.BaiGiang)
                .AsQueryable();

            if (monHocId.HasValue)
                query = query.Where(x => x.MonHocId == monHocId.Value);

            if (loai.HasValue)
                query = query.Where(x => x.LoaiTaiLieu == loai.Value);

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.TenTaiLieu.Contains(q));

            var data = await query.OrderByDescending(x => x.Id).ToListAsync();

            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.IsActive)
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
                .Where(x => monHocIdsOfLop.Contains(x.MonHocId)); // ✅ Lọc theo lớp

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
        public async Task<IActionResult> Diem(int? monHocId)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.DanhSachBaiNop
                .Include(x => x.BaiTap)
                    .ThenInclude(x => x!.MonHoc)
                .Where(x => x.HocSinhId == user!.Id);

            if (monHocId.HasValue)
                query = query.Where(x => x.BaiTap!.MonHocId == monHocId.Value);

            var data = await query
                .OrderByDescending(x => x.NgayNop)
                .ToListAsync();

            // Thống kê
            var daCham = data.Where(x => x.Diem.HasValue).ToList();
            double? diemTB = daCham.Count > 0 ? Math.Round(daCham.Average(x => x.Diem!.Value), 2) : null;
            double? diemCao = daCham.Count > 0 ? daCham.Max(x => x.Diem!.Value) : null;
            double? diemThap = daCham.Count > 0 ? daCham.Min(x => x.Diem!.Value) : null;

            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.IsActive)
                .OrderBy(m => m.TenMonHoc)
                .ToListAsync();

            ViewBag.DiemTB = diemTB;
            ViewBag.DiemCao = diemCao;
            ViewBag.DiemThap = diemThap;
            ViewBag.SoDaCham = daCham.Count;
            ViewBag.SoChuaCham = data.Count(x => !x.Diem.HasValue);
            ViewBag.MonHocs = monHocs;
            ViewBag.SelectedMonHoc = monHocId;

            return View(data);
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

            // ✅ Bài tập sắp hết hạn - chỉ lấy theo lớp của học sinh
            var monHocIdsForLich = user?.LopId.HasValue == true
                ? await _context.LopMonHocs
                    .Where(x => x.LopId == user!.LopId!.Value)
                    .Select(x => x.MonHocId)
                    .ToListAsync()
                : new List<int>();

            var baiTapSapHan = await _context.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .Where(x => x.HanNop > DateTime.Now && monHocIdsForLich.Contains(x.MonHocId))
                .OrderBy(x => x.HanNop)
                .Take(10)
                .ToListAsync();

            var baiNopCuaHS = await _context.DanhSachBaiNop
                .Where(x => x.HocSinhId == user!.Id)
                .Select(x => x.BaiTapId)
                .ToListAsync();

            ViewBag.BaiTapSapHan = baiTapSapHan;
            ViewBag.BaiDaNop = baiNopCuaHS;
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
    }
}