using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "HieuTruong")]
    public class HieuTruongController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<NguoiDung> _userManager;

        public HieuTruongController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ===================== DASHBOARD =====================
        public async Task<IActionResult> Index()
        {
            // Thống kê tổng quan
            ViewBag.SoHocSinh = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == "HocSinh"
                select u
            ).CountAsync();

            ViewBag.SoGiaoVien = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where r.Name == "GiaoVien"
                select u
            ).CountAsync();

            ViewBag.SoLop = await _context.Lops.CountAsync();
            ViewBag.SoMonHoc = await _context.DanhSachMonHoc.CountAsync(m => m.IsActive);
            ViewBag.SoKhoi = await _context.Khois.CountAsync();

            // 5 thông báo gần nhất
            ViewBag.ThongBaoMoiNhat = await _context.ThongBaos
                .OrderByDescending(t => t.NgayDang)
                .Take(5)
                .ToListAsync();

            // Số yêu cầu GV đang chờ (nếu có bảng YeuCauGiaoVien)
            ViewBag.SoYeuCauChoXuLy = await _context.YeuCauGiaoVien
     .CountAsync(y => y.TrangThai == TrangThaiYeuCau.ChoDuyet);

            // Thông tin HT đang đăng nhập
            var ht = await _userManager.GetUserAsync(User);
            ViewBag.HieuTruong = ht;

            return View();
        }

        // ===================== DANH SÁCH GIÁO VIÊN (READ-ONLY) =====================
        public async Task<IActionResult> DanhSachGiaoVien(string? search)
        {
            var gvUsers = await _userManager.GetUsersInRoleAsync("GiaoVien");

            if (!string.IsNullOrEmpty(search))
            {
                gvUsers = gvUsers
                    .Where(gv => gv.HoTen != null &&
                                 gv.HoTen.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var tatCaLop = await _context.Lops.Include(l => l.Khoi).ToListAsync();
            var userIds = gvUsers.Select(u => u.Id).ToList();

            var monGv = await _context.MonHocGiaoViens
                .Include(mg => mg.MonHoc)
                .Where(mg => userIds.Contains(mg.NguoiDungId))
                .ToListAsync();

            var viewModel = gvUsers.Select(gv =>
            {
                var lopCN = tatCaLop.FirstOrDefault(l => l.GiaoVienChuNhiemId == gv.Id);
                var mons = monGv
                    .Where(m => m.NguoiDungId == gv.Id)
                    .Select(m => m.MonHoc?.TenMonHoc)
                    .Where(n => n != null)
                    .Distinct()
                    .ToList();

                return new
                {
                    User = gv,
                    TenLop = lopCN?.TenLop ?? "---",
                    TenKhoi = lopCN?.Khoi?.TenKhoi ?? "Chưa phân",
                    MonPhanCong = mons
                };
            }).ToList();

            ViewBag.Search = search;
            ViewBag.SoLuong = gvUsers.Count;
            return View(viewModel);
        }

        // ===================== DANH SÁCH HỌC SINH (READ-ONLY) =====================
        public async Task<IActionResult> DanhSachHocSinh(string? search, int? lopId)
        {
            var lops = await _context.Lops.Include(l => l.Khoi).OrderBy(l => l.TenLop).ToListAsync();
            ViewBag.DanhSachLop = lops;
            ViewBag.LopId = lopId;
            ViewBag.Search = search;

            var query = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId
                        join r in _context.Roles on ur.RoleId equals r.Id
                        where r.Name == "HocSinh"
                        select u;

            if (lopId.HasValue && lopId.Value > 0)
                query = query.Where(u => u.LopId == lopId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.HoTen.Contains(search) || u.MaHocSinh!.Contains(search));

            var hocSinhs = await query
                .Include(u => u.Lop)
                    .ThenInclude(l => l!.Khoi)
                .OrderBy(u => u.Lop != null ? u.Lop.TenLop : "")
                .ThenBy(u => u.HoTen)
                .ToListAsync();

            ViewBag.SoLuong = hocSinhs.Count;
            return View(hocSinhs);
        }

        // ===================== QUẢN LÝ THÔNG BÁO =====================
        public async Task<IActionResult> QuanLyThongBao()
        {
            var list = await _context.ThongBaos
                .OrderByDescending(x => x.NgayDang)
                .ToListAsync();

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoThongBao(string tieuDe, string noiDung)
        {
            if (string.IsNullOrWhiteSpace(tieuDe) || string.IsNullOrWhiteSpace(noiDung))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return RedirectToAction("QuanLyThongBao");
            }

            var ht = await _userManager.GetUserAsync(User);

            var tb = new ThongBao
            {
                TieuDe = tieuDe.Trim(),
                NoiDung = noiDung.Trim(),
                NgayDang = DateTime.Now,
                HienThi = true,
                NguoiDangId = ht?.Id
            };

            _context.ThongBaos.Add(tb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã đăng thông báo thành công!";
            return RedirectToAction("QuanLyThongBao");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaThongBao(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null)
            {
                TempData["Error"] = "Không tìm thấy thông báo.";
                return RedirectToAction("QuanLyThongBao");
            }

            _context.ThongBaos.Remove(tb);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa thông báo.";
            return RedirectToAction("QuanLyThongBao");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleThongBao(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();

            tb.HienThi = !tb.HienThi;
            await _context.SaveChangesAsync();

            TempData["Success"] = tb.HienThi ? "Đã hiển thị thông báo." : "Đã ẩn thông báo.";
            return RedirectToAction("QuanLyThongBao");
        }

        // ===================== SỬA THÔNG BÁO =====================
        [HttpGet]
        public async Task<IActionResult> SuaThongBao(int id)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();
            return View(tb);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaThongBao(int id, string tieuDe, string noiDung, bool hienThi)
        {
            var tb = await _context.ThongBaos.FindAsync(id);
            if (tb == null) return NotFound();

            if (string.IsNullOrWhiteSpace(tieuDe) || string.IsNullOrWhiteSpace(noiDung))
            {
                TempData["Error"] = "Tiêu đề và nội dung không được để trống.";
                return View(tb);
            }

            tb.TieuDe = tieuDe.Trim();
            tb.NoiDung = noiDung.Trim();
            tb.HienThi = hienThi;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật thông báo thành công!";
            return RedirectToAction("QuanLyThongBao");
        }

        // ===================== QUẢN LÝ YÊU CẦU GIÁO VIÊN =====================
        public async Task<IActionResult> DanhSachYeuCau()
        {
            var yeuCaus = await _context.YeuCauGiaoVien
                .Include(y => y.GiaoVien)
                .OrderByDescending(y => y.NgayGui)
                .ToListAsync();
            return View(yeuCaus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XuLyYeuCau(int id, TrangThaiYeuCau trangThai, string ghiChu)
        {
            var yc = await _context.YeuCauGiaoVien.FindAsync(id);
            if (yc == null) return NotFound();

            var ht = await _userManager.GetUserAsync(User);

            yc.TrangThai = trangThai;
            yc.GhiChuAdmin = ghiChu;
            yc.NgayXuLy = DateTime.Now;
            yc.NguoiXuLyId = ht?.Id;

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã {(trangThai == TrangThaiYeuCau.DaDuyet ? "duyệt" : "từ chối")} yêu cầu.";
            return RedirectToAction("DanhSachYeuCau");
        }

        // ===================== THỐNG KÊ HỌC LỰC =====================
        public async Task<IActionResult> ThongKeHocLuc()
        {
            var diemSo = await _context.DanhSachDiemSo.ToListAsync();
            
            // Tính điểm trung bình của mỗi học sinh
            var hocSinhDiemTB = diemSo
                .GroupBy(d => d.HocSinhId)
                .Select(g => new { 
                    HocSinhId = g.Key, 
                    DiemTB = g.Average(d => d.Diem) 
                }).ToList();

            var thongKe = new {
                Gioi = hocSinhDiemTB.Count(x => x.DiemTB >= 8.0),
                Kha = hocSinhDiemTB.Count(x => x.DiemTB >= 6.5 && x.DiemTB < 8.0),
                TrungBinh = hocSinhDiemTB.Count(x => x.DiemTB >= 5.0 && x.DiemTB < 6.5),
                Yeu = hocSinhDiemTB.Count(x => x.DiemTB < 5.0)
            };

            ViewBag.ThongKe = thongKe;
            return View();
        }

        // ===================== THỜI KHÓA BIỂU TỔNG THỂ =====================
        public async Task<IActionResult> ThoiKhoaBieuTongThe(int? lopId, DateTime? date)
        {
            var lops = await _context.Lops.OrderBy(x => x.TenLop).ToListAsync();
            ViewBag.Lops = lops;
            ViewBag.SelectedLopId = lopId;

            DateTime selectedDate = date ?? DateTime.Now.Date;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            
            // Tính Thứ
            int thu = (int)selectedDate.DayOfWeek + 1;
            if (thu == 1) thu = 8; // Chủ nhật

            var query = _context.LichHocs
                .Include(l => l.Lop)
                .Include(l => l.MonHoc)
                .Include(l => l.GiaoVien)
                .AsQueryable();

            if (lopId.HasValue)
                query = query.Where(l => l.LopId == lopId.Value);

            var lichs = await query
                .Where(x => x.Thu == thu)
                .OrderBy(x => x.TietHoc)
                .ToListAsync();

            var leaves = await _context.YeuCauGiaoVien
                .Where(y => y.TrangThai == TrangThaiYeuCau.DaDuyet && 
                            y.LoaiYeuCau == LoaiYeuCau.NghiPhep && 
                            y.NgayNghi != null && y.NgayNghi.Value.Date == selectedDate.Date)
                .ToListAsync();
            ViewBag.Leaves = leaves;

            return View(lichs);
        }
    }
}