using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Text.Json;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "Admin,HieuTruong")]
    public class QuanLyHocSinhController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public QuanLyHocSinhController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ─────────────────────────────────────────
        // HELPER: lấy danh sách GV được chọn làm CNhiệm
        // lopId = null  →  tạo lớp mới (loại toàn bộ GV đã có lớp)
        // lopId = id    →  sửa lớp (cho phép giữ GV đang chủ nhiệm lớp này)
        // ─────────────────────────────────────────
        private async Task<List<NguoiDung>> GetGiaoVienCoTheChonAsync(int? lopId = null)
        {
            // Lấy danh sách Id của những người có role GiaoVien
            var giaoVienIds = await _userManager.GetUsersInRoleAsync("GiaoVien");
            var ids = giaoVienIds.Select(u => u.Id).ToList();

            return await _context.Users
                .Where(u => ids.Contains(u.Id)
                    && !_context.Lops.Any(l =>
                        l.GiaoVienChuNhiemId == u.Id
                        && (lopId == null || l.Id != lopId)))
                .OrderBy(u => u.HoTen)
                .ToListAsync();
        }

        // ─────────────────────────────────────────
        // DANH SÁCH KHỐI
        // ─────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var khois = await _context.Khois
                .Include(k => k.Lops)
                .ToListAsync();

            var lopIds = khois.SelectMany(k => k.Lops).Select(l => l.Id).ToList();

            var siSoDict = await _context.Users
                .Where(u => u.LopId.HasValue && lopIds.Contains(u.LopId.Value))
                .GroupBy(u => u.LopId.Value)
                .Select(g => new { LopId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.LopId, x => x.Count);

            foreach (var khoi in khois)
            {
                foreach (var lop in khoi.Lops)
                {
                    int count = siSoDict.TryGetValue(lop.Id, out var c) ? c : 0;
                    lop.HocSinhs = Enumerable.Range(0, count).Select(_ => new NguoiDung()).ToList();
                }
            }

            return View(khois);
        }

        // ─────────────────────────────────────────
        // DANH SÁCH LỚP TRONG KHỐI
        // ─────────────────────────────────────────
        public async Task<IActionResult> Lops(int khoiId)
        {
            var khoi = await _context.Khois
                .Include(k => k.Lops)
                    .ThenInclude(l => l.GiaoVienChuNhiem)
                .FirstOrDefaultAsync(k => k.Id == khoiId);

            if (khoi == null) return NotFound();

            var lopIds = khoi.Lops.Select(l => l.Id).ToList();

            var siSoDict = await _context.Users
                .Where(u => u.LopId.HasValue && lopIds.Contains(u.LopId.Value))
                .GroupBy(u => u.LopId.Value)
                .Select(g => new { LopId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.LopId, x => x.Count);

            foreach (var lop in khoi.Lops)
            {
                int count = siSoDict.TryGetValue(lop.Id, out var c) ? c : 0;
                lop.HocSinhs = Enumerable.Range(0, count).Select(_ => new NguoiDung()).ToList();
            }

            return View(khoi);
        }

        // ─────────────────────────────────────────
        // TẠO LỚP
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> TaoLop(int khoiId)
        {
            var khoi = await _context.Khois.FindAsync(khoiId);
            if (khoi == null) return NotFound();

            ViewBag.KhoiId = khoiId;
            ViewBag.TenKhoi = khoi.TenKhoi;
            ViewBag.GiaoViens = await GetGiaoVienCoTheChonAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoLop(int khoiId, string tenLop, string? giaoVienId)
        {
            var khoi = await _context.Khois.FindAsync(khoiId);
            if (khoi == null) return NotFound();

            // ── Chuẩn hoá ──
            tenLop = (tenLop ?? "").Trim().ToUpper();

            // ── Validate ──
            if (string.IsNullOrWhiteSpace(tenLop))
                ModelState.AddModelError("", "Vui lòng nhập tên lớp.");

            if (!string.IsNullOrWhiteSpace(tenLop) &&
                await _context.Lops.AnyAsync(l => l.TenLop == tenLop && l.MaKhoi == khoiId))
                ModelState.AddModelError("", $"Lớp \"{tenLop}\" đã tồn tại trong {khoi.TenKhoi}. Vui lòng nhập tên khác.");

            if (!string.IsNullOrWhiteSpace(giaoVienId) &&
                await _context.Lops.AnyAsync(l => l.GiaoVienChuNhiemId == giaoVienId))
                ModelState.AddModelError("", "Giáo viên này đã là chủ nhiệm lớp khác. Vui lòng chọn giáo viên khác.");

            if (!ModelState.IsValid)
            {
                ViewBag.KhoiId = khoiId;
                ViewBag.TenKhoi = khoi.TenKhoi;
                ViewBag.GiaoViens = await GetGiaoVienCoTheChonAsync();
                return View();
            }

            var lop = new Lop
            {
                TenLop = tenLop,
                MaKhoi = khoiId,
                GiaoVienChuNhiemId = giaoVienId
            };

            _context.Lops.Add(lop);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Tạo lớp \"{tenLop}\" thành công.";
            return RedirectToAction(nameof(Lops), new { khoiId });
        }

        // ─────────────────────────────────────────
        // AJAX: kiểm tra tên lớp đã tồn tại chưa
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CheckLopExists(int khoiId, string tenLop)
        {
            if (string.IsNullOrWhiteSpace(tenLop))
                return Json(new { exists = false });

            var normalized = tenLop.Trim().ToUpper();
            var exists = await _context.Lops
                .AnyAsync(l => l.MaKhoi == khoiId && l.TenLop == normalized);

            return Json(new { exists });
        }

        // ─────────────────────────────────────────
        // SỬA LỚP
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SuaLop(int id)
        {
            var lop = await _context.Lops.FindAsync(id);
            if (lop == null) return NotFound();

            // Cho phép giữ GV đang chủ nhiệm lớp này; loại GV đang chủ nhiệm lớp KHÁC
            ViewBag.GiaoViens = await GetGiaoVienCoTheChonAsync(id);
            return View(lop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaLop(Lop model)
        {
            var lop = await _context.Lops.FindAsync(model.Id);
            if (lop == null) return NotFound();

            model.TenLop = (model.TenLop ?? "").Trim().ToUpper();

            if (await _context.Lops.AnyAsync(l =>
                    l.MaKhoi == lop.MaKhoi &&
                    l.TenLop == model.TenLop &&
                    l.Id != lop.Id))
                ModelState.AddModelError("", "Tên lớp đã tồn tại trong khối này.");

            // Kiểm tra GV mới chọn có đang chủ nhiệm lớp khác không
            if (!string.IsNullOrWhiteSpace(model.GiaoVienChuNhiemId) &&
                model.GiaoVienChuNhiemId != lop.GiaoVienChuNhiemId &&
                await _context.Lops.AnyAsync(l => l.GiaoVienChuNhiemId == model.GiaoVienChuNhiemId))
                ModelState.AddModelError("", "Giáo viên này đã là chủ nhiệm lớp khác. Vui lòng chọn giáo viên khác.");

            if (!ModelState.IsValid)
            {
                ViewBag.GiaoViens = await GetGiaoVienCoTheChonAsync(model.Id);
                return View(model);
            }

            lop.TenLop = model.TenLop;
            lop.GiaoVienChuNhiemId = model.GiaoVienChuNhiemId;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật lớp thành công.";
            return RedirectToAction(nameof(Lops), new { khoiId = lop.MaKhoi });
        }

        // ─────────────────────────────────────────
        // XÓA LỚP
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaLop(int id)
        {
            var lop = await _context.Lops.FindAsync(id);
            if (lop == null) return NotFound();

            _context.Lops.Remove(lop);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa lớp \"{lop.TenLop}\".";
            return RedirectToAction(nameof(Lops), new { khoiId = lop.MaKhoi });
        }

        // ─────────────────────────────────────────
        // DANH SÁCH HỌC SINH TRONG LỚP
        // ─────────────────────────────────────────
        public async Task<IActionResult> HocSinhs(int lopId)
        {
            var lop = await _context.Lops
                .Include(l => l.HocSinhs)
                .FirstOrDefaultAsync(l => l.Id == lopId);

            if (lop == null) return NotFound();
            // Sắp xếp theo tên gọi (từ cuối trong họ tên) rồi đến họ tên đầy đủ
            // Ví dụ: Nguyễn Văn Anh, Trần Thị Bình, Lê Văn Cường...
            lop.HocSinhs = lop.HocSinhs
                .OrderBy(h => h.HoTen != null && h.HoTen.Contains(' ')
                    ? h.HoTen.Substring(h.HoTen.LastIndexOf(' ') + 1)
                    : h.HoTen)
                .ThenBy(h => h.HoTen)
                .ToList();
            return View(lop);
        }

        // ─────────────────────────────────────────
        // THÊM HỌC SINH (thủ công)
        // ─────────────────────────────────────────
        // ─────────────────────────────────────────
        // HELPER: tạo mã học sinh tự động
        // Format: [2 số năm][phần chữ tên lớp][3 số STT]
        // Ví dụ: lớp "10A1", năm 2026, STT 1 → "26A1001"
        // ─────────────────────────────────────────
        private string GetNamNhapHocPrefix(Lop lop)
        {
            int currentAcademicYear = DateTime.Now.Year;
            string tenLop = lop.TenLop ?? "";
            string tenKhoi = lop.Khoi?.TenKhoi ?? "";
            
            if (tenLop.StartsWith("11") || tenKhoi == "Khối 11") currentAcademicYear -= 1;
            else if (tenLop.StartsWith("12") || tenKhoi == "Khối 12") currentAcademicYear -= 2;

            return (currentAcademicYear % 100).ToString("D2");
        }

        private async Task<string> GenerateMaHocSinhAsync(int lopId)
        {
            var lop = await _context.Lops.Include(l => l.Khoi).FirstOrDefaultAsync(l => l.Id == lopId);
            if (lop == null) return "";

            string year = GetNamNhapHocPrefix(lop);

            // Lấy phần chữ: bỏ các chữ số đứng đầu tên lớp (10A1 → A1)
            string tenLop = lop.TenLop ?? "";
            string lopCode = System.Text.RegularExpressions.Regex.Replace(tenLop, @"^\d+", "").ToUpper();

            // Prefix để đối chiếu các mã đã có
            string prefix = year + lopCode;

            // Đếm số học sinh hiện tại trong lớp có mã bắt đầu bằng prefix
            int currentCount = await _context.Users
                .CountAsync(u => u.LopId == lopId && u.MaHocSinh != null && u.MaHocSinh.StartsWith(prefix));

            int nextStt = currentCount + 1;
            return prefix + nextStt.ToString("D3");
        }

        [HttpGet]
        public async Task<IActionResult> TaoHocSinh(int lopId)
        {
            var lop = await _context.Lops.FindAsync(lopId);
            if (lop == null) return NotFound();

            ViewBag.LopId = lopId;
            ViewBag.TenLop = lop.TenLop;
            ViewBag.MaHocSinhGoi = await GenerateMaHocSinhAsync(lopId);
            return View();
        }

        // AJAX: lấy mã học sinh tiếp theo (dùng khi cần refresh preview)
        [HttpGet]
        public async Task<IActionResult> GetNextMaHocSinh(int lopId)
        {
            var ma = await GenerateMaHocSinhAsync(lopId);
            return Json(new { ma });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoHocSinh(int lopId, string hoTen,
            DateTime? ngaySinh, string? gioiTinh, string? diaChi, string? matKhau)
        {
            var lop = await _context.Lops.FindAsync(lopId);
            ViewBag.LopId = lopId;
            ViewBag.TenLop = lop?.TenLop;

            if (string.IsNullOrWhiteSpace(hoTen))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ họ tên.");
                ViewBag.MaHocSinhGoi = await GenerateMaHocSinhAsync(lopId);
                return View();
            }

            // Tự động tạo mã học sinh
            string ma = await GenerateMaHocSinhAsync(lopId);

            // Đảm bảo không trùng (race condition)
            if (await _context.Users.AnyAsync(u => u.MaHocSinh == ma))
            {
                // Nếu trùng thì tăng thêm 1
                var lopForPrefix = await _context.Lops.Include(l => l.Khoi).FirstOrDefaultAsync(l => l.Id == lopId);
                string year = lopForPrefix != null ? GetNamNhapHocPrefix(lopForPrefix) : (DateTime.Now.Year % 100).ToString("D2");
                string tenLop = lopForPrefix?.TenLop ?? "";
                string lopCode = System.Text.RegularExpressions.Regex.Replace(tenLop, @"^\d+", "").ToUpper();
                string prefix = year + lopCode;

                var maxMa = await _context.Users
                    .Where(u => u.LopId == lopId && u.MaHocSinh != null && u.MaHocSinh.StartsWith(prefix))
                    .OrderByDescending(u => u.MaHocSinh)
                    .Select(u => u.MaHocSinh)
                    .FirstOrDefaultAsync();

                int lastStt = 0;
                if (maxMa != null && maxMa.Length > prefix.Length)
                    int.TryParse(maxMa.Substring(prefix.Length), out lastStt);
                ma = prefix + (lastStt + 1).ToString("D3");
            }

            var hocSinh = new NguoiDung
            {
                UserName = ma,
                NormalizedUserName = ma.ToUpper(),
                Email = ma + "@truong.edu.vn",
                NormalizedEmail = (ma + "@truong.edu.vn").ToUpper(),
                HoTen = hoTen.Trim().ToUpper(),
                MaHocSinh = ma,
                NgaySinh = ngaySinh,
                GioiTinh = gioiTinh,
                DiaChi = diaChi,
                LopId = lopId,
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                NgayTao = DateTime.Now
            };

            var passwordHasher = new PasswordHasher<NguoiDung>();
            // Sử dụng mật khẩu được nhập, nếu không có thì mặc định là mã sinh viên
            string passwordToHash = !string.IsNullOrWhiteSpace(matKhau) ? matKhau.Trim() : ma;
            hocSinh.PasswordHash = passwordHasher.HashPassword(hocSinh, passwordToHash);

            _context.Users.Add(hocSinh);
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(hocSinh, "HocSinh");

            // Khởi tạo điểm rỗng
            string namHocHienTai = DateTime.Now.Month >= 9 ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}" : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
            await KhoiTaoDiemRongChoHocSinhAsync(hocSinh.Id, lopId, namHocHienTai);

            TempData["Success"] = $"Thêm học sinh \"{hocSinh.HoTen}\" thành công. Mã: {ma}";
            return RedirectToAction(nameof(HocSinhs), new { lopId });
        }

        private async Task KhoiTaoDiemRongChoHocSinhAsync(string hocSinhId, int lopId, string namHoc, List<LopMonHoc>? lopMonHocs = null)
        {
            if (lopMonHocs == null)
            {
                lopMonHocs = await _context.LopMonHocs.Where(x => x.LopId == lopId).ToListAsync();
            }
            
            foreach (var lm in lopMonHocs)
            {
                // Khởi tạo DiemSo (điểm hiện tại cho GV)
                var ds = new DiemSo
                {
                    NguoiDungId = hocSinhId,
                    MonHocId = lm.MonHocId,
                    GiaoVienId = lm.GiaoVienId,
                    NgayNhap = DateTime.Now
                };
                _context.DiemSos.Add(ds);

                // Khởi tạo DiemHocKy (HK1 và HK2) cho Học sinh xem
                for (int hk = 1; hk <= 2; hk++)
                {
                    var dhk = new DiemHocKy
                    {
                        HocSinhId = hocSinhId,
                        MonHocId = lm.MonHocId,
                        LopId = lopId,
                        NamHoc = namHoc,
                        HocKy = hk,
                        GiaoVienId = lm.GiaoVienId,
                        NgayNhap = DateTime.Now
                    };
                    _context.DiemHocKys.Add(dhk);
                }
            }
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────
        // SỬA HỌC SINH (inline edit — GET JSON)
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetHocSinh(string id)
        {
            var hs = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.HoTen,
                    u.MaHocSinh,
                    NgaySinh = u.NgaySinh.HasValue
                        ? u.NgaySinh.Value.ToString("yyyy-MM-dd")
                        : "",
                    u.GioiTinh,
                    u.DiaChi
                })
                .FirstOrDefaultAsync();

            if (hs == null) return NotFound();
            return Json(hs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaHocSinh(string id, string hoTen, string maHocSinh,
            DateTime? ngaySinh, string? gioiTinh, string? diaChi, string? matKhauMoi)
        {
            var hs = await _context.Users.FindAsync(id);
            if (hs == null) return NotFound();

            string ma = maHocSinh.Trim().ToUpper();

            if (await _context.Users.AnyAsync(u => u.MaHocSinh == ma && u.Id != id))
            {
                TempData["Error"] = "Mã học sinh đã tồn tại!";
                return RedirectToAction(nameof(HocSinhs), new { lopId = hs.LopId });
            }

            hs.HoTen = hoTen.Trim().ToUpper();
            hs.MaHocSinh = ma;
            hs.UserName = ma;
            hs.NormalizedUserName = ma;
            hs.Email = ma + "@truong.edu.vn";
            hs.NormalizedEmail = (ma + "@truong.edu.vn").ToUpper();
            hs.NgaySinh = ngaySinh;
            hs.GioiTinh = gioiTinh;
            hs.DiaChi = diaChi;
            hs.NgayCapNhat = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(matKhauMoi))
            {
                var hasher = new PasswordHasher<NguoiDung>();
                hs.PasswordHash = hasher.HashPassword(hs, matKhauMoi);
                hs.SecurityStamp = Guid.NewGuid().ToString();
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật học sinh thành công.";
            return RedirectToAction(nameof(HocSinhs), new { lopId = hs.LopId });
        }

        // ─────────────────────────────────────────
        // XÓA HỌC SINH (lưu lịch sử trước khi xóa)
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaHocSinh(string id, int lopId, string? lyDoXoa)
        {
            var hs = await _context.Users
                .Include(u => u.Lop)
                    .ThenInclude(l => l!.Khoi)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (hs == null) return NotFound();

            // Lấy toàn bộ điểm học kỳ của học sinh
            var dsDiemHK = await _context.DiemHocKys
                .Include(d => d.MonHoc)
                .Include(d => d.Lop)
                .Where(d => d.HocSinhId == id)
                .ToListAsync();

            // Tạo bản ghi lịch sử
            var lichSu = new LichSuHocSinh
            {
                HoTen = hs.HoTen,
                MaHocSinh = hs.MaHocSinh,
                NgaySinh = hs.NgaySinh,
                GioiTinh = hs.GioiTinh,
                DiaChi = hs.DiaChi,
                AnhDaiDien = hs.AnhDaiDien,
                TenLop = hs.Lop?.TenLop,
                TenKhoi = hs.Lop?.Khoi?.TenKhoi,
                NamHocCuoi = hs.NamHoc,
                LyDoXoa = string.IsNullOrWhiteSpace(lyDoXoa) ? null : lyDoXoa.Trim(),
                TrangThai = "ĐãXóa",
                NgayXoa = DateTime.Now,
                NguoiXoaId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                NguoiXoaHoTen = User.Identity?.Name,
                NguoiDungIdGoc = hs.Id
            };

            _context.LichSuHocSinhs.Add(lichSu);
            await _context.SaveChangesAsync(); // lưu để có Id

            // Lưu snapshot điểm
            foreach (var d in dsDiemHK)
            {
                _context.LichSuDiemHocSinhs.Add(new LichSuDiemHocSinh
                {
                    LichSuHocSinhId = lichSu.Id,
                    TenMonHoc = d.MonHoc?.TenMonHoc,
                    NamHoc = d.NamHoc,
                    HocKy = d.HocKy,
                    TenLop = d.Lop?.TenLop,
                    DiemMieng1 = d.DiemMieng1,
                    DiemMieng2 = d.DiemMieng2,
                    DiemMieng3 = d.DiemMieng3,
                    Diem15Phut1 = d.Diem15Phut1,
                    Diem15Phut2 = d.Diem15Phut2,
                    DiemMotTiet1 = d.DiemMotTiet1,
                    DiemMotTiet2 = d.DiemMotTiet2,
                    DiemGiuaKy = d.DiemGiuaKy,
                    DiemCuoiKy = d.DiemCuoiKy,
                    DiemTongKet = d.DiemTongKet,
                    XepLoai = d.XepLoai,
                    NhanXet = d.NhanXet
                });
            }

            await _context.SaveChangesAsync();

            // Xóa tất cả dữ liệu liên quan trước khi xóa học sinh để tránh lỗi ràng buộc (FK)
            var dsDiemGV = await _context.DiemSos.Where(d => d.NguoiDungId == id).ToListAsync();
            var dsBaiNop = await _context.BaiNops.Where(b => b.HocSinhId == id).ToListAsync();
            
            _context.DiemSos.RemoveRange(dsDiemGV);
            _context.BaiNops.RemoveRange(dsBaiNop);
            _context.DiemHocKys.RemoveRange(dsDiemHK);

            _context.Users.Remove(hs);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa học sinh \u201c{hs.HoTen}\u201d và lưu lịch sử thành công.";
            return RedirectToAction(nameof(HocSinhs), new { lopId });
        }

        // ─────────────────────────────────────────
        // XEM ĐIỂM HỌC SINH (Admin)
        // ─────────────────────────────────────────
                [HttpGet]
        public async Task<IActionResult> XemDiemHocSinh(string hocSinhId, string? namHoc, int? hocKy)
        {
            var hs = await _context.Users
                .Include(u => u.Lop).ThenInclude(l => l!.Khoi)
                .FirstOrDefaultAsync(u => u.Id == hocSinhId);
            if (hs == null) return NotFound();

            var namHocHienTai = hs.NamHoc ?? "2024-2025";

            // Tự động chốt tất cả điểm HK1 của năm học hiện tại cho học sinh này khi Admin/Hiệu trưởng xem điểm
            if (User.IsInRole("Admin") || User.IsInRole("HieuTruong"))
            {
                var diemHK1HienTai = await _context.DiemHocKys
                    .Where(d => d.HocSinhId == hocSinhId && d.NamHoc == namHocHienTai && d.HocKy == 1)
                    .ToListAsync();

                bool hasUpdates = false;
                foreach (var dhk in diemHK1HienTai)
                {
                    if (!dhk.IsChotMieng || !dhk.IsChotGiuaKy || !dhk.IsChotCuoiKy)
                    {
                        dhk.IsChotMieng = true;
                        dhk.IsChotGiuaKy = true;
                        dhk.IsChotCuoiKy = true;

                        // Tính điểm trung bình và xếp loại nếu có đủ điểm giữa kỳ và cuối kỳ
                        var listDiem = new List<double>();
                        if (dhk.DiemMieng1.HasValue) listDiem.Add(dhk.DiemMieng1.Value);
                        if (dhk.DiemMieng2.HasValue) listDiem.Add(dhk.DiemMieng2.Value);
                        if (dhk.DiemMieng3.HasValue) listDiem.Add(dhk.DiemMieng3.Value);
                        if (dhk.DiemMieng4.HasValue) listDiem.Add(dhk.DiemMieng4.Value);

                        if (dhk.DiemGiuaKy.HasValue && dhk.DiemCuoiKy.HasValue)
                        {
                            double avgMieng = listDiem.Any() ? listDiem.Average() : 0;
                            dhk.DiemTongKet = Math.Round((avgMieng + dhk.DiemGiuaKy.Value * 2 + dhk.DiemCuoiKy.Value * 3) / 6, 1);
                            dhk.XepLoai = dhk.DiemTongKet >= 8.0 ? "Giỏi"
                                           : dhk.DiemTongKet >= 6.5 ? "Khá"
                                           : dhk.DiemTongKet >= 5.0 ? "Trung bình"
                                           : dhk.DiemTongKet >= 3.5 ? "Yếu"
                                           : "Kém";
                        }
                        hasUpdates = true;
                    }
                }
                if (hasUpdates)
                {
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Hệ thống đã tự động chốt toàn bộ điểm Học kỳ 1 của năm học hiện tại.";
                }
            }


            // Danh sách năm học có điểm (toàn bộ trong DB)
            var dsNamHocAll = await _context.DiemHocKys
                .Where(d => d.HocSinhId == hocSinhId)
                .Select(d => d.NamHoc)
                .Distinct()
                .OrderByDescending(x => x)
                .ToListAsync();

            // Admin / HieuTruong: xem tất cả năm học — không giới hạn theo khối
            // HocSinh: chỉ xem theo khối lớp (10→1 năm, 11→2 năm, 12→3 năm)
            List<string> dsNamHoc;
            if (User.IsInRole("Admin") || User.IsInRole("HieuTruong"))
            {
                dsNamHoc = dsNamHocAll.ToList();
                // Nếu Admin chọn một năm chưa có điểm, thêm tạm vào danh sách hiển thị
                if (!string.IsNullOrEmpty(namHoc) && !dsNamHoc.Contains(namHoc))
                {
                    dsNamHoc.Add(namHoc);
                    dsNamHoc = dsNamHoc.OrderByDescending(x => x).ToList();
                }
            }
            else
            {
                var tenKhoi = hs.Lop?.Khoi?.TenKhoi ?? "Khối 10";
                dsNamHoc = LimitNamHocByKhoi(dsNamHocAll, namHocHienTai, tenKhoi);
                
                // Nếu namHoc truyền vào nhưng không có trong danh sách được phép → reset
                if (!string.IsNullOrEmpty(namHoc) && !dsNamHoc.Contains(namHoc))
                    namHoc = null;
            }

            string selNamHoc = namHoc ?? (dsNamHoc.FirstOrDefault() ?? namHocHienTai);
            bool isLichSu = selNamHoc != namHocHienTai;

            if (!isLichSu && !hocKy.HasValue)
            {
                int currentMonth = DateTime.Now.Month;
                hocKy = (currentMonth >= 8 || currentMonth <= 1) ? 1 : 2;
            }

            var query = _context.DiemHocKys
                .Include(d => d.MonHoc)
                .Include(d => d.Lop)
                .Where(d => d.HocSinhId == hocSinhId && d.NamHoc == selNamHoc);

            if (hocKy.HasValue)
                query = query.Where(d => d.HocKy == hocKy.Value);

            var dsDiem = await query
                .OrderBy(d => d.HocKy)
                .ThenBy(d => d.MonHoc!.TenMonHoc)
                .ToListAsync();

            // Môn học: admin thấy tất cả môn; không giới hạn theo khối
            var monHocs = User.IsInRole("Admin") || User.IsInRole("HieuTruong")
                ? await _context.DanhSachMonHoc
                    .Where(m => m.IsActive)
                    .OrderBy(m => m.TenMonHoc)
                    .ToListAsync()
                : await _context.DanhSachMonHoc
                    .Where(m => m.IsActive && (hs.Lop == null || m.KhoiId == hs.Lop.MaKhoi))
                    .OrderBy(m => m.TenMonHoc)
                    .ToListAsync();

            ViewBag.HocSinh = hs;
            ViewBag.DsNamHoc = dsNamHoc;
            ViewBag.SelectedNamHoc = selNamHoc;
            ViewBag.SelectedHocKy = hocKy;
            ViewBag.MonHocs = monHocs;

            ViewBag.IsLichSu = isLichSu;

            // Chỉ tính tbCaNam khi xem năm cũ (đã kết thúc cả năm)
            if (!hocKy.HasValue && isLichSu)
            {
                var monIds = dsDiem.Select(x => x.MonHocId).Distinct().ToList();
                var tbCaNam = monIds.Select(mid =>
                {
                    var d1 = dsDiem.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 1);
                    var d2 = dsDiem.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 2);
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
                        TenMonHoc = d1?.MonHoc?.TenMonHoc ?? d2?.MonHoc?.TenMonHoc ?? "",
                        TkHK1 = tk1,
                        TkHK2 = tk2,
                        TbNam = tbNam,
                        XepLoai = xepLoai
                    };
                }).OrderBy(x => x.TenMonHoc).ToList();
                ViewBag.TbCaNam = tbCaNam;
            }

            return View(dsDiem);
        }

        // ─────────────────────────────────────────
        // NHẪP / SỬA ĐIỂM HỌC KỲ
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhapDiemHocKy(
            string hocSinhId, int monHocId, string namHoc, int hocKy,
            double? diemMieng1, double? diemMieng2, double? diemMieng3, double? diemMieng4,
            double? diem15Phut1, double? diem15Phut2,
            double? diemMotTiet1, double? diemMotTiet2,
            double? diemGiuaKy, double? diemCuoiKy,
            double? diemTongKet, string? nhanXet)
        {
            TempData["Error"] = "Admin/Hiệu trưởng không có quyền chỉnh sửa điểm. Chỉ giáo viên bộ môn mới có quyền nhập/sửa điểm.";
            return RedirectToAction(nameof(XemDiemHocSinh), new { hocSinhId, namHoc, hocKy });
        }

        // ─────────────────────────────────────────
        // ĐẸT NĂM HỌC CHO HỌC SINH
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetNamHoc(string hocSinhId, string namHoc)
        {
            var hs = await _context.Users.FindAsync(hocSinhId);
            if (hs == null) return NotFound();
            hs.NamHoc = namHoc.Trim();
            hs.NgayCapNhat = DateTime.Now;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật năm học \u201c{namHoc}\u201d cho {hs.HoTen}.";
            return RedirectToAction(nameof(XemDiemHocSinh), new { hocSinhId });
        }

        // ─────────────────────────────────────────
        // XEM LỊCH SỬ HỌC SINH ĐÃ NGHỈ
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> LichSuHocSinhs(string? q, int page = 1)
        {
            int pageSize = 20;
            var query = _context.LichSuHocSinhs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(x => x.HoTen.Contains(q) || (x.MaHocSinh != null && x.MaHocSinh.Contains(q)));

            int total = await query.CountAsync();
            var data = await query
                .OrderByDescending(x => x.NgayXoa)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Q = q;
            return View(data);
        }

        // ─────────────────────────────────────────
        // XEM ĐIỂM LỊCH SỬ CỦA HỌC SINH ĐÃ NGHỈ
        // ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> XemDiemLichSu(int lichSuId)
        {
            var lichSu = await _context.LichSuHocSinhs.FindAsync(lichSuId);
            if (lichSu == null) return NotFound();

            var dsDiem = await _context.LichSuDiemHocSinhs
                .Where(d => d.LichSuHocSinhId == lichSuId)
                .OrderBy(d => d.NamHoc)
                .ThenBy(d => d.HocKy)
                .ThenBy(d => d.TenMonHoc)
                .ToListAsync();

            ViewBag.LichSu = lichSu;
            return View(dsDiem);
        }

        // ─────────────────────────────────────────
        // KHÓA / MỞ TÀI KHOẢN HỌC SINH
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHocSinhActive(string id, int lopId)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction(nameof(HocSinhs), new { lopId });

            var hs = await _userManager.FindByIdAsync(id);
            if (hs == null)
                return RedirectToAction(nameof(HocSinhs), new { lopId });

            hs.IsActive = !hs.IsActive;
            var result = await _userManager.UpdateAsync(hs);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? (hs.IsActive ? "Kích hoạt tài khoản thành công." : "Khóa tài khoản thành công.")
                : "Không thể cập nhật trạng thái tài khoản.";

            return RedirectToAction(nameof(HocSinhs), new { lopId });
        }

        // ─────────────────────────────────────────
        // TẢI FILE EXCEL MẪU
        // ─────────────────────────────────────────
        [HttpGet]
        public IActionResult TaiFileMau()
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("MauNhapHocSinh");

                // Tiêu đề cột
                worksheet.Cells[1, 1].Value = "Họ và Tên";
                worksheet.Cells[1, 2].Value = "Giới tính";
                worksheet.Cells[1, 3].Value = "Ngày sinh";
                worksheet.Cells[1, 4].Value = "Địa chỉ";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Dữ liệu mẫu
                worksheet.Cells[2, 1].Value = "NGUYỄN VĂN A";
                worksheet.Cells[2, 2].Value = "Nam";
                worksheet.Cells[2, 3].Value = "01/01/2010";
                worksheet.Cells[2, 4].Value = "Hà Nội";

                worksheet.Cells[3, 1].Value = "TRẦN THỊ B";
                worksheet.Cells[3, 2].Value = "Nữ";
                worksheet.Cells[3, 3].Value = "15/05/2010";
                worksheet.Cells[3, 4].Value = "TP. Hồ Chí Minh";

                worksheet.Cells.AutoFitColumns();

                var fileContent = package.GetAsByteArray();
                return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Mau_Nhap_Hoc_Sinh.xlsx");
            }
        }

        // ─────────────────────────────────────────
        // IMPORT EXCEL
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile fileExcel, int lopId)
        {
            if (fileExcel == null || fileExcel.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel!";
                return RedirectToAction(nameof(HocSinhs), new { lopId });
            }

            int successCount = 0;
            int skipCount = 0;
            var errors = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await fileExcel.CopyToAsync(stream);
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    TempData["Error"] = "File Excel không hợp lệ hoặc không có sheet nào.";
                    return RedirectToAction(nameof(HocSinhs), new { lopId });
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;

                // Tính prefix mã một lần cho cả batch
                var lopInfo = await _context.Lops.Include(l => l.Khoi).FirstOrDefaultAsync(l => l.Id == lopId);
                string year = lopInfo != null ? GetNamNhapHocPrefix(lopInfo) : (DateTime.Now.Year % 100).ToString("D2");
                string tenLop = lopInfo?.TenLop ?? "";
                string lopCode = System.Text.RegularExpressions.Regex.Replace(tenLop, @"^\d+", "").ToUpper();
                string prefix = year + lopCode;

                var lopMonHocs = await _context.LopMonHocs.Where(x => x.LopId == lopId).ToListAsync();
                string namHocHienTai = DateTime.Now.Month >= 9 ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}" : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";

                for (int row = 2; row <= rowCount; row++)
                {
                    // Cột Excel: 1=Họ tên | 2=Giới tính | 3=Ngày sinh | 4=Địa chỉ
                    var hoTen    = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                    var gioiTinh = worksheet.Cells[row, 2].Value?.ToString()?.Trim();
                    var diaChi   = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                    if (string.IsNullOrWhiteSpace(hoTen))
                    {
                        skipCount++;
                        continue;
                    }

                    // ── Phân tích ngày sinh (cột 3) ──
                    DateTime? ngaySinh = null;
                    var cell = worksheet.Cells[row, 3];

                    if (cell.Value != null)
                    {
                        if (cell.Value is DateTime dt)
                        {
                            ngaySinh = dt;
                        }
                        else if (double.TryParse(cell.Value.ToString(), out double serial))
                        {
                            ngaySinh = DateTime.FromOADate(serial);
                        }
                        else
                        {
                            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                            if (DateTime.TryParseExact(cell.Text.Trim(), formats,
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None,
                                    out DateTime parsed))
                                ngaySinh = parsed;
                        }
                    }

                    // ── Tự động sinh mã học sinh tuần tự ──
                    int currentCount = await _context.Users
                        .CountAsync(u => u.LopId == lopId && u.MaHocSinh != null && u.MaHocSinh.StartsWith(prefix));
                    string maHS = prefix + (currentCount + 1).ToString("D3");

                    var user = new NguoiDung
                    {
                        UserName = maHS,
                        NormalizedUserName = maHS.ToUpper(),
                        Email = maHS + "@truong.edu.vn",
                        NormalizedEmail = (maHS + "@truong.edu.vn").ToUpper(),
                        HoTen = hoTen.ToUpper(),
                        MaHocSinh = maHS,
                        GioiTinh = gioiTinh,
                        NgaySinh = ngaySinh,
                        DiaChi = diaChi,
                        LopId = lopId,
                        EmailConfirmed = true,
                        IsActive = true,
                        NgayTao = DateTime.Now,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _userManager.CreateAsync(user, maHS);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "HocSinh");
                        await KhoiTaoDiemRongChoHocSinhAsync(user.Id, lopId, namHocHienTai, lopMonHocs);
                        successCount++;
                    }
                    else
                    {
                        errors.Add($"Dòng {row} ({hoTen}): " +
                                   string.Join(", ", result.Errors.Select(e => e.Description)));
                        skipCount++;
                    }
                }

                if (errors.Any())
                    TempData["Error"] = "Một số dòng bị lỗi: " +
                                        string.Join(" | ", errors.Take(3)) +
                                        (errors.Count > 3 ? " ..." : "");

                TempData["Success"] = $"Nhập Excel thành công: {successCount} học sinh. Bỏ qua: {skipCount}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction(nameof(HocSinhs), new { lopId });
        }

        // ─────────────────────────────────────────
        // XEM MÔN HỌC CỦA LỚP
        // ─────────────────────────────────────────
        public async Task<IActionResult> MonHocs(int lopId)
        {
            var lop = await _context.Lops
                .Include(l => l.Khoi)
                .FirstOrDefaultAsync(l => l.Id == lopId);

            if (lop == null) return NotFound();

            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.KhoiId == lop.MaKhoi && m.IsActive)
                .ToListAsync();

            var lopMonHocs = await _context.LopMonHocs
                .Include(lm => lm.MonHoc)
                .Include(lm => lm.GiaoVien)
                .Where(lm => lm.LopId == lop.Id)
                .ToListAsync();

            var monHocGv = await _context.MonHocGiaoViens
                .Include(mg => mg.MonHoc)
                .Include(mg => mg.GiaoVien)
                .Where(mg => mg.LopId == lop.Id)
                .ToListAsync();

            var vm = monHocs.Select(m => new {
                MonHoc = m,
                GiaoVien = lopMonHocs.FirstOrDefault(x => x.MonHocId == m.Id)?.GiaoVien
                           ?? monHocGv.FirstOrDefault(x => x.MonHocId == m.Id)?.GiaoVien
            }).ToList();

            ViewBag.Lop = lop;
            return View(vm);
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

        private static int GetMaxNamLuiByKhoi(string tenKhoi)
        {
            var parts = tenKhoi.Trim().Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int soKhoi))
                return Math.Max(0, soKhoi - 10); // 10→0, 11→1, 12→2
            return 0;
        }
    }
}