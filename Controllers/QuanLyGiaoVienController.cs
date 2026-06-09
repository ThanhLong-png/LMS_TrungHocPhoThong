using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class QuanLyGiaoVienController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public QuanLyGiaoVienController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return RedirectToAction("Index");

            user.IsActive = !user.IsActive; // toggle
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = user.IsActive ? "Tài khoản đã được kích hoạt." : "Tài khoản đã bị khóa.";
            }
            else
            {
                TempData["Error"] = "Không thể cập nhật trạng thái tài khoản.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(string search)
        {
            var gvUsers = await _userManager.GetUsersInRoleAsync("GiaoVien");
            var tatCaLop = await _context.Lops.Include(l => l.Khoi).ToListAsync();
            ViewBag.TatCaLop = tatCaLop;

            // Lấy tất cả môn để hiển thị thông tin phân công
            var tatCaMon = await _context.DanhSachMonHoc.Include(m => m.Khoi).ToListAsync();
            ViewBag.TatCaMonHoc = tatCaMon;

            if (!string.IsNullOrEmpty(search))
            {
                gvUsers = gvUsers.Where(gv => gv.HoTen != null && gv.HoTen.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var userIds = gvUsers.Select(u => u.Id).ToList();
            var monGv = await _context.MonHocGiaoViens
                .Include(mg => mg.MonHoc)
                .Where(mg => userIds.Contains(mg.NguoiDungId))
                .ToListAsync();

            var viewModel = gvUsers.Select(gv =>
            {
                var lopCN = tatCaLop.FirstOrDefault(l => l.GiaoVienChuNhiemId == gv.Id);
                var mons = monGv.Where(m => m.NguoiDungId == gv.Id).Select(m => m.MonHoc?.TenMonHoc).Where(n => n != null).Distinct().ToList();
                return new
                {
                    User = gv,
                    TenLop = lopCN?.TenLop ?? "---",
                    TenKhoi = lopCN?.Khoi?.TenKhoi ?? "Chưa phân",
                    MonPhanCong = mons
                };
            }).ToList();

            ViewBag.SoLuong = gvUsers.Count;
            ViewBag.Search = search; // để giữ giá trị tìm kiếm ở view
            return View(viewModel);
        }

        // ✅ Helper: tự sinh mã giáo viên dạng GV0001, GV0002...
        private async Task<string> SinhMaGiaoVien()
        {
            // Lấy tất cả username bắt đầu bằng "GV"
            var existingMa = await _userManager.Users
                .Where(u => u.UserName != null && u.UserName.StartsWith("GV"))
                .Select(u => u.UserName!)
                .ToListAsync();

            int maxSo = 0;
            foreach (var ma in existingMa)
            {
                // Lấy phần số sau "GV"
                var phanSo = ma.Substring(2);
                if (int.TryParse(phanSo, out int so))
                {
                    if (so > maxSo) maxSo = so;
                }
            }

            return $"GV{(maxSo + 1):D4}"; // GV0001, GV0002, ...
        }

        [HttpGet]
        public async Task<IActionResult> TaoGiaoVien()
        {
            ViewBag.LopTrong = await _context.Lops
                .Where(l => string.IsNullOrEmpty(l.GiaoVienChuNhiemId))
                .ToListAsync();

            // Lấy tất cả môn học để có thể chọn khi tạo giáo viên
            var tatCaMon = await _context.DanhSachMonHoc
                .Include(m => m.Khoi)
                .Where(m => m.IsActive)
                .ToListAsync();

            ViewBag.TatCaMonHoc = tatCaMon;

            // ✅ Preview mã sẽ được sinh
            ViewBag.MaGVPreview = await SinhMaGiaoVien();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoGiaoVien(
            string hoTen, string gioiTinh,
            string monDay, string chucVu, int? lopId,
            DateTime? ngaySinh, string diaChi, int[]? monHocIds)
        {
            // ✅ Tự sinh mã giáo viên
            var maNormalized = await SinhMaGiaoVien();

            // ✅ Check trùng username
            var existingUser = await _userManager.FindByNameAsync(maNormalized);
            if (existingUser != null)
            {
                TempData["Error"] = "Mã giáo viên đã tồn tại!";
                return RedirectToAction("TaoGiaoVien");
            }

            var user = new NguoiDung
            {
                UserName = maNormalized,
                Email = $"{maNormalized.ToLower()}@truong.edu.vn",
                HoTen = hoTen?.Trim().ToUpper(),
                GioiTinh = gioiTinh,
                ChuyenMon = monDay,
                ChucVu = chucVu,
                NgaySinh = ngaySinh,
                DiaChi = diaChi,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, maNormalized);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "GiaoVien");

                if (lopId.HasValue && lopId.Value > 0)
                {
                    var lop = await _context.Lops.FindAsync(lopId.Value);
                    if (lop != null)
                    {
                        lop.GiaoVienChuNhiemId = user.Id;
                        _context.Update(lop);
                        await _context.SaveChangesAsync();
                    }
                }

                // Nếu có chọn monHocIds thì thêm phân công MonHocGiaoVien
                if (monHocIds != null && monHocIds.Length > 0)
                {
                    foreach (var mhId in monHocIds.Distinct())
                    {
                        // ❗ Check: lớp này đã có giáo viên cho môn này chưa
                        var daCoGV = await _context.MonHocGiaoViens
                            .AnyAsync(mg => mg.MonHocId == mhId
                                && mg.LopId == lopId);

                        if (daCoGV)
                        {
                            TempData["Warning"] = "Lớp đã có giáo viên cho môn này!";
                            continue;
                        }

                        _context.MonHocGiaoViens.Add(new MonHocGiaoVien
                        {
                            MonHocId = mhId,
                            NguoiDungId = user.Id,
                            LopId = lopId
                        });

                        // ✅ Đồng bộ LopMonHoc
                        if (lopId.HasValue && lopId.Value > 0)
                        {
                            var lm = await _context.LopMonHocs
                                .FirstOrDefaultAsync(x => x.LopId == lopId.Value && x.MonHocId == mhId);
                            if (lm != null)
                                lm.GiaoVienId = user.Id;
                            else
                                _context.LopMonHocs.Add(new LopMonHoc { LopId = lopId.Value, MonHocId = mhId, GiaoVienId = user.Id });
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Đã thêm giáo viên {user.HoTen} thành công!";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Lỗi: " + string.Join(" | ", result.Errors.Select(e => e.Description));

            ViewBag.LopTrong = await _context.Lops
                .Where(l => string.IsNullOrEmpty(l.GiaoVienChuNhiemId))
                .ToListAsync();

            ViewBag.TatCaMonHoc = await _context.DanhSachMonHoc
                .Include(m => m.Khoi)
                .Where(m => m.IsActive)
                .ToListAsync();

            ViewBag.MaGVPreview = maNormalized; // giữ lại mã đợ định sinh

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaGiaoVien(
            string id, string hoTen, string gioiTinh,
            string chucVu, int? lopId,
            DateTime? ngaySinh, string diaChi, int[]? monHocIds)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.HoTen = hoTen?.Trim().ToUpper();
            user.GioiTinh = gioiTinh;
            user.ChucVu = chucVu;
            user.NgaySinh = ngaySinh;
            user.DiaChi = diaChi;

            // Cập nhật trường ChuyenMon (chuỗi tên môn học phân công)
            if (monHocIds != null && monHocIds.Length > 0)
            {
                var monNames = await _context.DanhSachMonHoc
                    .Where(m => monHocIds.Contains(m.Id))
                    .Select(m => m.TenMonHoc)
                    .ToListAsync();
                user.ChuyenMon = string.Join(", ", monNames);
            }
            else
            {
                user.ChuyenMon = null;
            }

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // ✅ Xóa tất cả lớp cũ (fix lỗi nhiều lớp)
                var lopsCu = await _context.Lops
                    .Where(l => l.GiaoVienChuNhiemId == user.Id)
                    .ToListAsync();

                foreach (var l in lopsCu)
                {
                    l.GiaoVienChuNhiemId = null;
                    _context.Update(l);
                }

                // ✅ Gán lớp mới
                if (lopId.HasValue && lopId.Value > 0)
                {
                    var lopMoi = await _context.Lops.FindAsync(lopId.Value);
                    if (lopMoi != null)
                    {
                        lopMoi.GiaoVienChuNhiemId = user.Id;
                        _context.Update(lopMoi);
                    }
                }

                // ✅ Cập nhật phân công môn dạy
                var phanCongsCu = await _context.MonHocGiaoViens
                    .Where(mg => mg.NguoiDungId == user.Id)
                    .ToListAsync();

                // Null hóa LopMonHoc cũ trước khi xóa
                foreach (var pc in phanCongsCu)
                {
                    if (pc.LopId.HasValue)
                    {
                        var lmCu = await _context.LopMonHocs
                            .FirstOrDefaultAsync(x => x.LopId == pc.LopId.Value && x.MonHocId == pc.MonHocId && x.GiaoVienId == user.Id);
                        if (lmCu != null)
                            lmCu.GiaoVienId = null;
                    }
                }

                _context.MonHocGiaoViens.RemoveRange(phanCongsCu);

                if (monHocIds != null && monHocIds.Length > 0)
                {
                    foreach (var mhId in monHocIds.Distinct())
                    {
                        _context.MonHocGiaoViens.Add(new MonHocGiaoVien
                        {
                            MonHocId = mhId,
                            NguoiDungId = user.Id,
                            LopId = lopId
                        });

                        // ✅ Upsert LopMonHoc mới
                        if (lopId.HasValue && lopId.Value > 0)
                        {
                            var lmMoi = await _context.LopMonHocs
                                .FirstOrDefaultAsync(x => x.LopId == lopId.Value && x.MonHocId == mhId);
                            if (lmMoi != null)
                                lmMoi.GiaoVienId = user.Id;
                            else
                                _context.LopMonHocs.Add(new LopMonHoc { LopId = lopId.Value, MonHocId = mhId, GiaoVienId = user.Id });
                        }
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin giáo viên thành công!";
            }
            else
            {
                TempData["Error"] = "Lỗi: " + string.Join(" | ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaGiaoVien(string id)
        {
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                
             

                // ❗ Xóa phân công môn và null LopMonHoc
                var phanCongs = await _context.MonHocGiaoViens
                    .Where(mg => mg.NguoiDungId == id)
                    .ToListAsync();

                foreach (var pc in phanCongs)
                {
                    if (pc.LopId.HasValue)
                    {
                        var lm = await _context.LopMonHocs
                            .FirstOrDefaultAsync(x => x.LopId == pc.LopId.Value && x.MonHocId == pc.MonHocId && x.GiaoVienId == id);
                        if (lm != null)
                            lm.GiaoVienId = null;
                    }
                }

                _context.MonHocGiaoViens.RemoveRange(phanCongs);

                // ❗ Xóa lịch học
                var lichHocs = await _context.LichHocs
                    .Where(l => l.GiaoVienId == id)
                    .ToListAsync();

                _context.LichHocs.RemoveRange(lichHocs);

                // ❗ Xóa GVCN
                var lops = await _context.Lops
                    .Where(l => l.GiaoVienChuNhiemId == id)
                    .ToListAsync();

                foreach (var l in lops)
                {
                    l.GiaoVienChuNhiemId = null;
                }

                await _context.SaveChangesAsync();

                // ❗ Xóa user
                await _userManager.DeleteAsync(user);

                TempData["Success"] = "Đã xóa giáo viên khỏi hệ thống!";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetGiaoVien(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var lop = await _context.Lops
                .FirstOrDefaultAsync(l => l.GiaoVienChuNhiemId == id);

            var monHocIds = await _context.MonHocGiaoViens
                .Where(mg => mg.NguoiDungId == id)
                .Select(mg => mg.MonHocId)
                .Distinct()
                .ToListAsync();

            return Json(new
            {
                id = user.Id,
                hoTen = user.HoTen,
                gioiTinh = user.GioiTinh,
                chucVu = user.ChucVu,
                chuyenMon = user.ChuyenMon,
                ngaySinh = user.NgaySinh?.ToString("yyyy-MM-dd"),
                diaChi = user.DiaChi,
                maLop = lop?.Id ?? 0,
                monHocIds = monHocIds
            });
        }
        // CheckMaGV không còn cần thiết vì mã được sinh tự động
    }
}