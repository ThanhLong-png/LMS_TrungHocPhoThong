using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "Admin")]
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
            var khois = await _context.Khois.ToListAsync();
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
                .Include(k => k.Lops)
                    .ThenInclude(l => l.HocSinhs)
                .FirstOrDefaultAsync(k => k.Id == khoiId);

            if (khoi == null) return NotFound();
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
            lop.HocSinhs = lop.HocSinhs.OrderBy(h => h.HoTen).ToList();
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
        private async Task<string> GenerateMaHocSinhAsync(int lopId)
        {
            var lop = await _context.Lops.FindAsync(lopId);
            if (lop == null) return "";

            string year = (DateTime.Now.Year % 100).ToString("D2"); // "26"

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
            DateTime? ngaySinh, string? gioiTinh, string matKhau)
        {
            var lop = await _context.Lops.FindAsync(lopId);
            ViewBag.LopId = lopId;
            ViewBag.TenLop = lop?.TenLop;

            if (string.IsNullOrWhiteSpace(hoTen) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc.");
                ViewBag.MaHocSinhGoi = await GenerateMaHocSinhAsync(lopId);
                return View();
            }

            // Tự động tạo mã học sinh
            string ma = await GenerateMaHocSinhAsync(lopId);

            // Đảm bảo không trùng (race condition)
            if (await _context.Users.AnyAsync(u => u.MaHocSinh == ma))
            {
                // Nếu trùng thì tăng thêm 1
                string year = (DateTime.Now.Year % 100).ToString("D2");
                string tenLop = lop?.TenLop ?? "";
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
                LopId = lopId,
                EmailConfirmed = true,
                IsActive = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                NgayTao = DateTime.Now
            };

            var passwordHasher = new PasswordHasher<NguoiDung>();
            hocSinh.PasswordHash = passwordHasher.HashPassword(hocSinh, matKhau);

            _context.Users.Add(hocSinh);
            await _context.SaveChangesAsync();

            await _userManager.AddToRoleAsync(hocSinh, "HocSinh");

            TempData["Success"] = $"Thêm học sinh \"{hocSinh.HoTen}\" thành công. Mã: {ma}";
            return RedirectToAction(nameof(HocSinhs), new { lopId });
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
        // XÓA HỌC SINH
        // ─────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaHocSinh(string id, int lopId)
        {
            var hs = await _context.Users.FindAsync(id);
            if (hs == null) return NotFound();

            _context.Users.Remove(hs);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa học sinh thành công.";
            return RedirectToAction(nameof(HocSinhs), new { lopId });
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
                var lopInfo = await _context.Lops.FindAsync(lopId);
                string year = (DateTime.Now.Year % 100).ToString("D2");
                string tenLop = lopInfo?.TenLop ?? "";
                string lopCode = System.Text.RegularExpressions.Regex.Replace(tenLop, @"^\d+", "").ToUpper();
                string prefix = year + lopCode;

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
    }
}