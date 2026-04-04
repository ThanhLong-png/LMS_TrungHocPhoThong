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
        private readonly UserManager<NguoiDung> _userManager; // thêm

        public QuanLyHocSinhController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager; // thêm
        }

        // =======================
        // XEM MÔN HỌC CỦA LỚP
        // =======================
        public async Task<IActionResult> MonHocs(int lopId)
        {
            var lop = await _context.Lops
                .Include(l => l.Khoi)
                .FirstOrDefaultAsync(l => l.Id == lopId);

            if (lop == null) return NotFound();

            // Lấy tất cả môn của khối
            var monHocs = await _context.DanhSachMonHoc
                .Where(m => m.KhoiId == lop.MaKhoi && m.IsActive)
                .ToListAsync();

            // Lấy người phụ trách trên lớp cho từng môn (LopMonHoc nếu có), hoặc MonHocGiaoVien
            var lopMonHocs = await _context.LopMonHocs
                .Include(lm => lm.MonHoc)
                .Include(lm => lm.GiaoVien)
                .Where(lm => lm.LopId == lop.Id)
                .ToListAsync();

            // Nếu không có LopMonHoc, fallback sang MonHocGiaoVien entries that have LopId == lop.Id
            var monHocGv = await _context.MonHocGiaoViens
                .Include(mg => mg.MonHoc)
                .Include(mg => mg.GiaoVien)
                .Where(mg => mg.LopId == lop.Id)
                .ToListAsync();

            // Build a view model: pair each MonHoc with a teacher if any
            var vm = monHocs.Select(m => new {
                MonHoc = m,
                GiaoVien = (lopMonHocs.FirstOrDefault(x => x.MonHocId == m.Id)?.GiaoVien)
                           ?? monHocGv.FirstOrDefault(x => x.MonHocId == m.Id)?.GiaoVien
            }).ToList();

            ViewBag.Lop = lop;
            return View(vm);
        }
        // =======================
        // DANH SÁCH KHỐI
        // =======================
        public async Task<IActionResult> Index()
        {
            var khois = await _context.Khois.ToListAsync();
            return View(khois);
        }

        // =======================
        // DANH SÁCH LỚP TRONG KHỐI
        // =======================
        public async Task<IActionResult> Lops(int khoiId)
        {
            var khoi = await _context.Khois
                .Include(k => k.Lops)
                    .ThenInclude(l => l.GiaoVienChuNhiem)
                .Include(k => k.Lops)
                    .ThenInclude(l => l.HocSinhs) // <--- PHẢI THÊM DÒNG NÀY ĐỂ ĐẾM SỐ LƯỢNG
                .FirstOrDefaultAsync(k => k.Id == khoiId);

            if (khoi == null) return NotFound();
            return View(khoi);
        }

        // =======================
        // TẠO LỚP
        // =======================
        [HttpGet]
        public async Task<IActionResult> TaoLop(int khoiId)
        {
            var khoi = await _context.Khois.FindAsync(khoiId); // thêm dòng này

            ViewBag.KhoiId = khoiId;
            ViewBag.TenKhoi = khoi?.TenKhoi ?? "Không rõ";    // thêm dòng này
            // Chỉ lấy giáo viên có chuyên môn và chưa là chủ nhiệm của lớp nào
            ViewBag.GiaoViens = await _context.Users
                .Where(u => u.ChuyenMon != null && !_context.Lops.Any(l => l.GiaoVienChuNhiemId == u.Id))
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoLop(int khoiId, string tenLop, string? giaoVienId)
        {
            var khoi = await _context.Khois.FindAsync(khoiId);

            if (string.IsNullOrWhiteSpace(tenLop))
            {
                ModelState.AddModelError("", "Vui lòng nhập tên lớp");
                ViewBag.KhoiId = khoiId;
                ViewBag.TenKhoi = khoi?.TenKhoi ?? "Không rõ";
                ViewBag.GiaoViens = await _context.Users
                    .Where(u => u.ChuyenMon != null)
                    .ToListAsync();
                return View();
            }

            // Tự động viết hoa chữ cái đầu mỗi từ
            // Viết hoa toàn bộ
            tenLop = tenLop.Trim().ToUpper();

            bool exists = await _context.Lops.AnyAsync(l => l.TenLop == tenLop && l.MaKhoi == khoiId);
            if (exists)
            {
                ModelState.AddModelError("", $"Lớp \"{tenLop}\" đã tồn tại trong khối {khoi?.TenKhoi}. Vui lòng nhập tên khác.");
                ViewBag.KhoiId = khoiId;
                ViewBag.TenKhoi = khoi?.TenKhoi ?? "Không rõ";
                ViewBag.GiaoViens = await _context.Users
                    .Where(u => u.ChuyenMon != null)
                    .ToListAsync();
                return View();
            }

            var lop = new Lop
            {
                TenLop = tenLop,
                MaKhoi = khoiId,
                GiaoVienChuNhiemId = giaoVienId
            };

            // Kiểm tra xem giáo viên đã là chủ nhiệm lớp khác chưa
            if (!string.IsNullOrWhiteSpace(giaoVienId))
            {
                var already = await _context.Lops.AnyAsync(l => l.GiaoVienChuNhiemId == giaoVienId);
                if (already)
                {
                    ModelState.AddModelError("", "Giáo viên đã là chủ nhiệm lớp khác, vui lòng chọn giáo viên khác.");
                    ViewBag.KhoiId = khoiId;
                    ViewBag.TenKhoi = khoi?.TenKhoi ?? "Không rõ";
                    ViewBag.GiaoViens = await _context.Users
                        .Where(u => u.ChuyenMon != null && !_context.Lops.Any(l => l.GiaoVienChuNhiemId == u.Id))
                        .ToListAsync();
                    return View();
                }
            }

            _context.Lops.Add(lop);
            await _context.SaveChangesAsync();

            return RedirectToAction("Lops", new { khoiId });
        }

        [HttpGet]
        public async Task<IActionResult> CheckLopExists(int khoiId, string tenLop)
        {
            if (string.IsNullOrWhiteSpace(tenLop))
                return Json(new { exists = false });

            var normalized = tenLop.Trim().ToUpper();
            var exists = await _context.Lops
                .AnyAsync(l => l.MaKhoi == khoiId && l.TenLop.ToUpper() == normalized);

            return Json(new { exists });
        }

        // =======================
        // SỬA LỚP
        // =======================
        [HttpGet]
        public async Task<IActionResult> SuaLop(int id)
        {
            var lop = await _context.Lops.FindAsync(id);
            if (lop == null) return NotFound();

            ViewBag.GiaoViens = await _context.Users
                .Where(u => u.ChuyenMon != null)
                .ToListAsync();

            return View(lop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaLop(Lop model)
        {
            var lop = await _context.Lops.FindAsync(model.Id);
            if (lop == null) return NotFound();

            bool exists = await _context.Lops.AnyAsync(l =>
                l.MaKhoi == lop.MaKhoi &&
                l.TenLop == model.TenLop &&
                l.Id != lop.Id);

            if (exists)
            {
                ModelState.AddModelError("", "Tên lớp đã tồn tại");
                ViewBag.GiaoViens = await _context.Users
                    .Where(u => u.ChuyenMon != null)
                    .ToListAsync();
                return View(model);
            }

            lop.TenLop = model.TenLop;
            lop.GiaoVienChuNhiemId = model.GiaoVienChuNhiemId;

            await _context.SaveChangesAsync();
            return RedirectToAction("Lops", new { khoiId = lop.MaKhoi });
        }

        // =======================
        // XÓA LỚP
        // =======================
        [HttpPost]
        public async Task<IActionResult> XoaLop(int id)
        {
            var lop = await _context.Lops.FindAsync(id);
            if (lop == null) return NotFound();

            _context.Lops.Remove(lop);
            await _context.SaveChangesAsync();

            return RedirectToAction("Lops", new { khoiId = lop.MaKhoi });
        }

        // =======================
        // DANH SÁCH HỌC SINH TRONG LỚP
        // =======================
        public async Task<IActionResult> HocSinhs(int lopId)
        {
            var lop = await _context.Lops
                .Include(l => l.HocSinhs)
                .FirstOrDefaultAsync(l => l.Id == lopId);

            if (lop == null) return NotFound();
            lop.HocSinhs = lop.HocSinhs.OrderBy(h => h.HoTen).ToList();
            return View(lop);
        }

        // =======================
        // THÊM HỌC SINH
        // =======================
        [HttpGet]
        public IActionResult TaoHocSinh(int lopId)
        {
            ViewBag.LopId = lopId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoHocSinh(int lopId, string hoTen, string maHocSinh,
            DateTime? ngaySinh, string? gioiTinh, string matKhau)
        {
            ViewBag.LopId = lopId;

            if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(maHocSinh) ||
                string.IsNullOrWhiteSpace(matKhau))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin bắt buộc");
                return View();
            }

            bool maExists = await _context.Users.AnyAsync(u => u.MaHocSinh == maHocSinh.Trim().ToUpper());
            if (maExists)
            {
                ModelState.AddModelError("", "Mã học sinh đã tồn tại, vui lòng nhập mã khác");
                return View();
            }

            string ma = maHocSinh.Trim().ToUpper();

            var hocSinh = new NguoiDung
            {
                UserName = ma,
                NormalizedUserName = ma,
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

            // Gán role HocSinh sau khi lưu xong
            await _userManager.AddToRoleAsync(hocSinh, "HocSinh");

            return RedirectToAction("HocSinhs", new { lopId });
        }
        // =======================
        // SỬA HỌC SINH
        // =======================
        [HttpGet]
        public async Task<IActionResult> GetHocSinh(string id)
        {
            var hs = await _context.Users
                .Select(u => new {
                    u.Id,
                    u.HoTen,
                    u.MaHocSinh,
                    NgaySinh = u.NgaySinh.HasValue ? u.NgaySinh.Value.ToString("yyyy-MM-dd") : "",
                    u.GioiTinh,
                    u.DiaChi
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (hs == null) return NotFound();
            return Json(hs);
        }

        // 3. XỬ LÝ LƯU CHỈNH SỬA (Sửa lại logic của bạn để chuẩn xác hơn)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaHocSinh(string id, string hoTen, string maHocSinh,
            DateTime? ngaySinh, string? gioiTinh, string? diaChi, string? matKhauMoi)
        {
            var hs = await _context.Users.FindAsync(id);
            if (hs == null) return NotFound();

            string ma = maHocSinh.Trim().ToUpper();

            // Kiểm tra trùng mã (trừ chính nó)
            bool maExists = await _context.Users.AnyAsync(u => u.MaHocSinh == ma && u.Id != id);
            if (maExists)
            {
                TempData["Error"] = "Mã học sinh đã tồn tại!";
                return RedirectToAction("HocSinhs", new { lopId = hs.LopId });
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
                var passwordHasher = new PasswordHasher<NguoiDung>();
                hs.PasswordHash = passwordHasher.HashPassword(hs, matKhauMoi);
                hs.SecurityStamp = Guid.NewGuid().ToString();
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật thành công!";
            return RedirectToAction("HocSinhs", new { lopId = hs.LopId });
        }

      

        // =======================
        // XÓA HỌC SINH
        // =======================
        [HttpPost]
        public async Task<IActionResult> XoaHocSinh(string id, int lopId)
        {
            var hs = await _context.Users.FindAsync(id);
            if (hs == null) return NotFound();

            _context.Users.Remove(hs);
            await _context.SaveChangesAsync();

            return RedirectToAction("HocSinhs", new { lopId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHocSinhActive(string id, int lopId)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("HocSinhs", new { lopId });

            var hs = await _userManager.FindByIdAsync(id);
            if (hs == null) return RedirectToAction("HocSinhs", new { lopId });

            hs.IsActive = !hs.IsActive;
            var result = await _userManager.UpdateAsync(hs);
            if (result.Succeeded)
            {
                TempData["Success"] = hs.IsActive ? "Kích hoạt tài khoản học sinh thành công." : "Khóa tài khoản học sinh thành công.";
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật trạng thái tài khoản học sinh.";
            }

            return RedirectToAction("HocSinhs", new { lopId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile fileExcel, int lopId)
        {
            if (fileExcel == null || fileExcel.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel!";
                return RedirectToAction("HocSinhs", new { lopId });
            }

            int successCount = 0;
            int skipCount = 0;
            List<string> errors = new List<string>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await fileExcel.CopyToAsync(stream);
                    stream.Position = 0;

                    // --- EPPlus 8+: chỉ cần dùng Stream, LicenseContext đã set global ---
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                        if (worksheet == null)
                        {
                            TempData["Error"] = "File Excel không hợp lệ hoặc không có sheet nào.";
                            return RedirectToAction("HocSinhs", new { lopId });
                        }

                        int rowCount = worksheet.Dimension?.Rows ?? 0;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var hoTen = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                            var maHS = worksheet.Cells[row, 2].Value?.ToString()?.Trim()?.ToUpper();
                            var gioiTinh = worksheet.Cells[row, 3].Value?.ToString()?.Trim();
                            var ngaySinhStr = worksheet.Cells[row, 4].Value?.ToString()?.Trim();
                            var diaChi = worksheet.Cells[row, 5].Value?.ToString()?.Trim();

                            if (string.IsNullOrWhiteSpace(maHS) || string.IsNullOrWhiteSpace(hoTen))
                            {
                                skipCount++;
                                continue;
                            }

                            var existed = await _userManager.FindByNameAsync(maHS);
                            if (existed != null)
                            {
                                skipCount++;
                                continue;
                            }

                            DateTime? ngaySinh = null;
                            var cell = worksheet.Cells[row, 4]; // Cột D

                            if (cell.Value != null)
                            {
                                // Trường hợp 1: Excel đã nhận diện là DateTime (Kiểu chuẩn nhất)
                                if (cell.Value is DateTime dt)
                                {
                                    ngaySinh = dt;
                                }
                                // Trường hợp 2: Excel đang lưu dạng Số (Serial Number - ví dụ 38487)
                                else if (double.TryParse(cell.Value.ToString(), out double serialDate))
                                {
                                    ngaySinh = DateTime.FromOADate(serialDate);
                                }
                                // Trường hợp 3: Excel đang lưu dạng Chữ (Text - ví dụ "15/05/2005")
                                else
                                {
                                    string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" };
                                    if (DateTime.TryParseExact(cell.Text.Trim(), formats,
                                        System.Globalization.CultureInfo.InvariantCulture,
                                        System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                                    {
                                        ngaySinh = parsedDate;
                                    }
                                }
                            }

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
                                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                                errors.Add($"Dòng {row} ({maHS}): {errorMsg}");
                                skipCount++;
                            }
                        }
                    }
                }

                if (errors.Any())
                {
                    TempData["Error"] = "Một số dòng bị lỗi: " + string.Join(" | ", errors.Take(3)) +
                                        (errors.Count > 3 ? " ..." : "");
                }

                TempData["Success"] = $"Nhập Excel thành công: {successCount}, Bỏ qua: {skipCount}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
            }

            return RedirectToAction("HocSinhs", new { lopId });
        }
    }
}