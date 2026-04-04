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

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoGiaoVien(
            string hoTen, string maGV, string gioiTinh,
            string monDay, string chucVu, int? lopId,
            DateTime? ngaySinh, string diaChi, int[]? monHocIds)
        {
            if (string.IsNullOrWhiteSpace(maGV))
            {
                TempData["Error"] = "Mã giáo viên không được để trống!";
                return RedirectToAction("TaoGiaoVien");
            }

            var maNormalized = maGV.Trim().ToUpper();

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

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaGiaoVien(
            string id, string hoTen, string gioiTinh,
            string chuyenMon, string chucVu, int? lopId,
            DateTime? ngaySinh, string diaChi)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.HoTen = hoTen?.Trim().ToUpper();
            user.GioiTinh = gioiTinh;
            user.ChuyenMon = chuyenMon;
            user.ChucVu = chucVu;
            user.NgaySinh = ngaySinh;
            user.DiaChi = diaChi;

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
                
             

                // ❗ Xóa phân công môn
                var phanCongs = await _context.MonHocGiaoViens
                    .Where(mg => mg.NguoiDungId == id)
                    .ToListAsync();

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

            return Json(new
            {
                id = user.Id,
                hoTen = user.HoTen,
                gioiTinh = user.GioiTinh,
                chucVu = user.ChucVu,
                chuyenMon = user.ChuyenMon,
                ngaySinh = user.NgaySinh?.ToString("yyyy-MM-dd"),
                diaChi = user.DiaChi,
                maLop = lop?.Id ?? 0
            });
        }
        [HttpGet]
        public async Task<IActionResult> CheckMaGV(string maGV)
        {
            if (string.IsNullOrWhiteSpace(maGV))
                return Json(new { exists = false });

            var maNormalized = maGV.Trim().ToUpper();

            var user = await _userManager.FindByNameAsync(maNormalized);

            return Json(new { exists = user != null });
        }
    }
}