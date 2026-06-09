// Areas/GiaoVien/Controllers/GiaoVienController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_THPT.Data;
using LMS_THPT.Models;
using LMS_THPT.ViewModels;

namespace LMS_THPT.Areas.GiaoVien.Controllers
{
    [Area("GiaoVien")]
    [Authorize(Roles = "GiaoVien")]
    public class GiaoVienController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly IWebHostEnvironment _env;

        public GiaoVienController(
            ApplicationDbContext db,
            UserManager<NguoiDung> userManager,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // ======================
        // Profile APIs for modal
        // ======================
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return Json(new { error = "Not authenticated" });

            return Json(new
            {
                id = gv.Id,
                hoTen = gv.HoTen,
                ngaySinh = gv.NgaySinh?.ToString("yyyy-MM-dd") ?? "",
                gioiTinh = gv.GioiTinh ?? "",
                diaChi = gv.DiaChi ?? "",
                anhDaiDien = string.IsNullOrEmpty(gv.AnhDaiDien) ? null : gv.AnhDaiDien
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangCaNhan()
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return Json(new { success = false, message = "Không xác thực" });

            var form = await Request.ReadFormAsync();
            var hoTen = form["HoTen"].FirstOrDefault();
            var ngaySinh = form["NgaySinh"].FirstOrDefault();
            var gioiTinh = form["GioiTinh"].FirstOrDefault();
            var diaChi = form["DiaChi"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(hoTen)) gv.HoTen = hoTen;
            if (!string.IsNullOrWhiteSpace(ngaySinh) && DateTime.TryParse(ngaySinh, out var dt)) gv.NgaySinh = dt;
            gv.GioiTinh = string.IsNullOrWhiteSpace(gioiTinh) ? null : gioiTinh;
            gv.DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi;

            // Handle avatar upload
            var file = Request.Form.Files.FirstOrDefault();
            string? avatarUrl = null;
            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "avatars");
                Directory.CreateDirectory(uploads);
                var ext = Path.GetExtension(file.FileName);
                var fileName = $"avatar_{gv.Id}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
                var filePath = Path.Combine(uploads, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fs);
                }
                avatarUrl = "/uploads/avatars/" + fileName;
                gv.AnhDaiDien = avatarUrl;
            }

            gv.NgayCapNhat = DateTime.Now;
            var result = await _userManager.UpdateAsync(gv);
            if (result.Succeeded)
                return Json(new { success = true, message = "Cập nhật hồ sơ thành công", avatarUrl });
            else
                return Json(new { success = false, message = string.Join("; ", result.Errors.Select(e => e.Description)) });
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.0  DASHBOARD
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            // Chỉ lấy môn học và lớp mà giáo viên được phân công PHỤ TRÁCH trong bảng LopMonHoc
            // (GiaoVienId trong LopMonHoc là nguồn duy nhất xác định GV phụ trách môn tại lớp)
            var lopMonHocPhuTrach = await _db.LopMonHocs
                .Where(x => x.GiaoVienId == gv.Id)
                .ToListAsync();
            var monHocIds = lopMonHocPhuTrach.Select(x => x.MonHocId).Distinct().ToList();

            var lopIds = lopMonHocPhuTrach.Select(x => x.LopId).Distinct().ToList();

            var tongHocSinh = await _db.NguoiDungs
                .Where(u => u.LopId.HasValue && lopIds.Contains(u.LopId.Value))
                .Select(u => u.Id)
                .Distinct()
                .CountAsync();

            var tongBaiNopPending = await _db.BaiNops
                .Where(x => x.BaiTap.MonHocId != null &&
                            monHocIds.Contains((int)x.BaiTap.MonHocId) &&
                            x.Diem == null)
                .CountAsync();

            var tongTaiLieu = await _db.TaiLieus
                .Where(x => x.MonHocId != null && monHocIds.Contains((int)x.MonHocId))
                .CountAsync();

            var baiNopGanDay = await _db.BaiNops
                .Include(x => x.HocSinh)
                .Include(x => x.BaiTap)
                .Where(x => x.BaiTap.MonHocId != null && monHocIds.Contains((int)x.BaiTap.MonHocId))
                .OrderByDescending(x => x.NgayNop)
                .Take(6)
                .Select(x => new BaiNopGanDayItem
                {
                    BaiNopId = x.Id,
                    TenHocSinh = x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? "",
                    TenVietTat = GetInitials(x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? ""),
                    TenBaiTap = x.BaiTap!.TieuDe ?? "",
                    ThoiGianNop = FormatTimeAgo(x.NgayNop),
                    TrangThai = x.Diem == null ? "pending" : "graded"
                })
                .ToListAsync();

            var tienDo = new List<TienDoLopItem>();
            foreach (var lopMonHoc in await _db.LopMonHocs
                .Include(x => x.Lop)
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id)
                .OrderBy(x => x.Lop!.MaKhoi).ThenBy(x => x.Lop!.TenLop)
                .ToListAsync())
            {
                var hocSinhLop = await _db.NguoiDungs
                    .Where(u => u.LopId.HasValue && u.LopId.Value == lopMonHoc.LopId)
                    .Select(u => u.Id)
                    .ToListAsync();

                var tongBaiTap = await _db.BaiTaps
                    .CountAsync(x => x.MonHocId == lopMonHoc.MonHocId);

                var daHoanThanh = tongBaiTap == 0 ? 0 :
                    await _db.BaiNops
                        .CountAsync(x => hocSinhLop.Contains(x.HocSinhId) &&
                                         x.BaiTap.MonHocId == lopMonHoc.MonHocId);

                var pct = tongBaiTap == 0 ? 0 :
                    Math.Min(100, (int)((double)daHoanThanh / (tongBaiTap * hocSinhLop.Count + 1) * 100));

                tienDo.Add(new TienDoLopItem
                {
                    TenLop = lopMonHoc.Lop?.TenLop ?? "",
                    TenMonHoc = lopMonHoc.MonHoc?.TenMonHoc ?? "",
                    PhanTram = pct
                });
            }

            var today = DateTime.Today;

            // Tập hợp (LopId, MonHocId) GV phụ trách — để lọc lịch dạy chính xác
            var phuTrachSetDashboard = lopMonHocPhuTrach
                .Select(x => (x.LopId, x.MonHocId))
                .ToHashSet();

            var lichHomNayRaw = await _db.LichHocs
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id && x.NgayHoc.Date == today)
                .OrderBy(x => x.GioBatDau)
                .Take(10)
                .ToListAsync();
            var lichHomNay = lichHomNayRaw
                .Where(x => phuTrachSetDashboard.Contains((x.LopId, x.MonHocId)))
                .Take(3)
                .Select(x => new LichHocItem
                {
                    ThoiGian = x.GioBatDau.ToString(@"hh\:mm") + " – " + x.GioKetThuc.ToString(@"hh\:mm"),
                    TenMon = x.MonHoc!.TenMonHoc ?? "",
                    PhongLop = x.PhongHoc ?? "",
                    NhanManh = false
                })
                .ToList();

            var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)today.DayOfWeek + 7) % 7;
            var endOfWeek = today.AddDays(daysUntilSunday);

            var nextDaysSchedulesRaw = await _db.LichHocs
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id && x.NgayHoc.Date > today && x.NgayHoc.Date <= endOfWeek)
                .OrderBy(x => x.NgayHoc).ThenBy(x => x.GioBatDau)
                .ToListAsync();
            var nextDaysSchedules = nextDaysSchedulesRaw
                .Where(x => phuTrachSetDashboard.Contains((x.LopId, x.MonHocId)))
                .ToList();

            var lichCacNgayTiepTheo = nextDaysSchedules
                .GroupBy(x => x.NgayHoc.Date)
                .Select(g => new LichHocNgayItem
                {
                    Ngay = g.Key.ToString("dd/MM/yyyy") + (g.Key == today.AddDays(1) ? " (Ngày mai)" : ""),
                    CacTietHoc = g.Select(x => new LichHocItem
                    {
                        ThoiGian = x.GioBatDau.ToString(@"hh\:mm") + " – " + x.GioKetThuc.ToString(@"hh\:mm"),
                        TenMon = x.MonHoc!.TenMonHoc ?? "",
                        PhongLop = x.PhongHoc ?? "",
                        NhanManh = false
                    }).ToList()
                })
                .ToList();

            // Kiểm tra có lớp chủ nhiệm không (để hiện badge trên sidebar)
            var lopChuNhiem = await _db.Lops
                .FirstOrDefaultAsync(l => l.GiaoVienChuNhiemId == gv.Id);
            ViewBag.CoLopChuNhiem = lopChuNhiem != null;
            ViewBag.TenLopChuNhiem = lopChuNhiem?.TenLop ?? "";

            var vm = new TeacherDashboardViewModel
            {
                GiaoVien = gv,
                TongHocSinh = tongHocSinh,
                TongMonHoc = monHocIds.Count,
                BaiNopChamPending = tongBaiNopPending,
                TongTaiLieu = tongTaiLieu,
                BaiNopGanDay = baiNopGanDay,
                TienDoLop = tienDo,
                LichHomNay = lichHomNay,
                LichCacNgayTiepTheo = lichCacNgayTiepTheo
            };

            return View(vm);
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.1  QUẢN LÝ MÔN HỌC
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> QuanLyMonHoc(int monHocId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            var monHocIds = await _db.MonHocGiaoViens
                .Where(x => x.NguoiDungId == gv.Id)
                .Select(x => x.MonHocId)
                .ToListAsync();

            if (monHocId == 0 && monHocIds.Any())
                monHocId = monHocIds.First();

            var monHoc = await _db.MonHocs.FindAsync(monHocId) ?? new MonHoc();

            var monHocsCuaGiaoVien = await _db.MonHocGiaoViens
                .Where(x => x.NguoiDungId == gv.Id)
                .Include(x => x.MonHoc)
                .Select(x => x.MonHoc!)
                .Distinct()
                .ToListAsync();

            var lopMonHocs = new List<LopMonHoc>();
            if (monHocId != 0)
            {
                lopMonHocs = await _db.LopMonHocs
                    .Include(lm => lm.Lop)
                    .Where(lm => lm.MonHocId == monHocId)
                    .ToListAsync();
            }

            ViewBag.TeacherMonHocs = monHocsCuaGiaoVien;
            ViewBag.LopMonHocs = lopMonHocs;

            var lopIds = await _db.LopMonHocs
                .Where(x => x.MonHocId == monHocId)
                .Select(x => x.LopId)
                .ToListAsync();

            var hocSinhList = await _db.NguoiDungs
                .Include(u => u.Lop)
                .Where(u => u.LopId.HasValue && lopIds.Contains(u.LopId.Value))
                .Select(u => new HocSinhSelectItem
                {
                    Id = u.Id,
                    HoTen = u.HoTen ?? u.UserName ?? "",
                    TenLop = u.Lop != null ? u.Lop.TenLop ?? "" : ""
                })
                .Distinct()
                .ToListAsync();

            var danhSachDiemRaw = await _db.DiemSos
                .Include(x => x.NguoiDung)
                .Where(x => x.MonHocId == monHocId)
                .ToListAsync();

            var danhSachDiem = danhSachDiemRaw.Select(x => {
                double? tongKet = null;
                if (x.DiemGiuaKy.HasValue && x.DiemCuoiKy.HasValue)
                {
                    var dms = new List<double>();
                    if (x.Diem.HasValue) dms.Add(x.Diem.Value);
                    if (x.DiemMieng2.HasValue) dms.Add(x.DiemMieng2.Value);
                    if (x.DiemMieng3.HasValue) dms.Add(x.DiemMieng3.Value);
                    if (x.DiemMieng4.HasValue) dms.Add(x.DiemMieng4.Value);

                    if (dms.Any()) {
                        tongKet = Math.Round((dms.Average() + x.DiemGiuaKy.Value * 2 + x.DiemCuoiKy.Value * 3) / 6, 1);
                    } else {
                        tongKet = Math.Round(x.DiemGiuaKy.Value * 0.4 + x.DiemCuoiKy.Value * 0.6, 1);
                    }
                }

                string xepLoai = "--";
                if (tongKet.HasValue)
                {
                    if (tongKet >= 9) xepLoai = "Xuất sắc";
                    else if (tongKet >= 8) xepLoai = "Giỏi";
                    else if (tongKet >= 6.5) xepLoai = "Khá";
                    else if (tongKet >= 5) xepLoai = "Trung bình";
                    else xepLoai = "Yếu";
                }

                return new NhapDiemItem
                {
                    HocSinhId = x.NguoiDungId,
                    TenHocSinh = x.NguoiDung?.HoTen ?? x.NguoiDung?.UserName ?? "",
                    DiemMieng = x.Diem,
                    DiemMieng2 = x.DiemMieng2,
                    DiemMieng3 = x.DiemMieng3,
                    DiemMieng4 = x.DiemMieng4,
                    DiemGiuaKy = x.DiemGiuaKy,
                    DiemCuoiKy = x.DiemCuoiKy,
                    DiemSoId = x.Id,
                    DiemTongKet = tongKet,
                    XepLoai = xepLoai
                };
            }).ToList();

            foreach (var hs in hocSinhList)
            {
                if (!danhSachDiem.Any(d => d.HocSinhId == hs.Id))
                    danhSachDiem.Add(new NhapDiemItem { HocSinhId = hs.Id, TenHocSinh = hs.HoTen });
            }

            var vm = new MonHocManageViewModel
            {
                MonHoc = monHoc,
                DanhSachHocSinh = hocSinhList,
                DanhSachDiem = danhSachDiem
            };

            ViewBag.MonHocIds = monHocIds;
            ViewBag.MonHocHienTai = monHocId;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatMonHoc(MonHoc model)
        {
            var monHoc = await _db.MonHocs.FindAsync(model.Id);
            if (monHoc == null) return NotFound();

            monHoc.TenMonHoc = model.TenMonHoc;
            monHoc.MoTa = model.MoTa;
            monHoc.MucTieu = model.MucTieu;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Cập nhật môn học thành công!";
            return RedirectToAction(nameof(QuanLyMonHoc), new { monHocId = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> GuiYeuCauTrangThai([FromBody] YeuCauThayDoiTrangThaiModel model)
        {
            var gv = await _userManager.GetUserAsync(User);
            var yeuCau = new YeuCauGiaoVien
            {
                GiaoVienId = gv!.Id,
                TieuDe = model.LyDo ?? "Yêu cầu",
                MoTa = $"Yêu cầu: [{model.TrangThai}] {model.LyDo}",
                TrangThai = TrangThaiYeuCau.ChoXuLy,
                NgayGui = DateTime.Now,
                LopId = null
            };
            _db.YeuCauGiaoViens.Add(yeuCau);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Đã gửi yêu cầu!" });
        }

        [HttpPost]
        public async Task<IActionResult> LuuDiem([FromBody] LuuDiemRequest model)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "luudiem_debug.log");
            try
            {
                var gv = await _userManager.GetUserAsync(User);
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Start LuuDiem. Teacher: {gv?.Id}, Student: {model.HocSinhId}, MonHoc: {model.MonHocId}, HocKy: {model.HocKy}, NamHoc: {model.NamHoc}, DiemMieng: {model.DiemMieng}, DiemMieng2: {model.DiemMieng2}, DiemMieng3: {model.DiemMieng3}, DiemMieng4: {model.DiemMieng4}, DiemGiuaKy: {model.DiemGiuaKy}, DiemCuoiKy: {model.DiemCuoiKy}{Environment.NewLine}");

                if (gv == null)
                {
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Error: Teacher not authenticated.{Environment.NewLine}");
                    return Json(new { success = false, message = "Không xác thực giáo viên" });
                }

                var hs = await _db.Users.FirstOrDefaultAsync(u => u.Id == model.HocSinhId);
                if (hs == null)
                {
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Error: Student {model.HocSinhId} not found.{Environment.NewLine}");
                    return Json(new { success = false, message = "Không tìm thấy học sinh" });
                }

                var servesThisSubject = await _db.LopMonHocs
                    .AnyAsync(lm => lm.LopId == hs.LopId && lm.MonHocId == model.MonHocId && lm.GiaoVienId == gv.Id);

                if (!servesThisSubject)
                {
                    System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Error: Teacher {gv.Id} does not teach subject {model.MonHocId} to class {hs.LopId} of student {hs.Id}.{Environment.NewLine}");
                    return Json(new { success = false, message = "Bạn không được phân công giảng dạy môn học này cho lớp của học sinh." });
                }

                // Xác định năm học và học kỳ
                // Ưu tiên NamHoc từ request (giao diện đang xem), tránh lưu nhầm năm
                string fallbackNamHoc = DateTime.Now.Month >= 9
                    ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                    : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
                string namHoc = !string.IsNullOrEmpty(model.NamHoc) ? model.NamHoc : fallbackNamHoc;
                int hocKy = model.HocKy ?? ((DateTime.Now.Month >= 8 || DateTime.Now.Month <= 1) ? 1 : 2);

                Console.WriteLine("========== LUU DIEM ==========");
                Console.WriteLine($"HocSinh={hs.HoTen}, NamHoc={namHoc}, HocKy={hocKy}, MonHoc={model.MonHocId}");
                Console.WriteLine("==============================");

                var diemHK = await _db.DiemHocKys
                    .FirstOrDefaultAsync(d => d.HocSinhId == hs.Id && d.MonHocId == model.MonHocId && d.NamHoc == namHoc && d.HocKy == hocKy);

                if (diemHK != null)
                {
                    if (diemHK.IsChotMieng && (model.DiemMieng != diemHK.DiemMieng1 || model.DiemMieng2 != diemHK.DiemMieng2 || model.DiemMieng3 != diemHK.DiemMieng3 || model.DiemMieng4 != diemHK.DiemMieng4))
                    {
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Error: Oral grades locked.{Environment.NewLine}");
                        return Json(new { success = false, message = "Điểm miệng đã được chốt, không thể chỉnh sửa!" });
                    }
                    if (diemHK.IsChotGiuaKy && model.DiemGiuaKy != diemHK.DiemGiuaKy)
                    {
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Error: Midterm grades locked.{Environment.NewLine}");
                        return Json(new { success = false, message = "Điểm giữa kỳ đã được chốt, không thể chỉnh sửa!" });
                    }
                    if (diemHK.IsChotCuoiKy && model.DiemCuoiKy != diemHK.DiemCuoiKy)
                    {
                        System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Error: Final grades locked.{Environment.NewLine}");
                        return Json(new { success = false, message = "Điểm cuối kỳ đã được chốt, không thể chỉnh sửa!" });
                    }
                }

                var diem = await _db.DiemSos
                    .FirstOrDefaultAsync(x => x.NguoiDungId == model.HocSinhId && x.MonHocId == model.MonHocId);

                if (diem == null)
                {
                    diem = new DiemSo
                    {
                        NguoiDungId = model.HocSinhId,
                        MonHocId = model.MonHocId,
                        Diem = model.DiemMieng,
                        DiemMieng2 = model.DiemMieng2,
                        DiemMieng3 = model.DiemMieng3,
                        DiemMieng4 = model.DiemMieng4,
                        DiemGiuaKy = model.DiemGiuaKy,
                        DiemCuoiKy = model.DiemCuoiKy,
                        NgayNhap = DateTime.Now,
                        GiaoVienId = gv.Id
                    };
                    _db.DiemSos.Add(diem);
                }
                else
                {
                    diem.Diem = model.DiemMieng;
                    diem.DiemMieng2 = model.DiemMieng2;
                    diem.DiemMieng3 = model.DiemMieng3;
                    diem.DiemMieng4 = model.DiemMieng4;
                    diem.DiemGiuaKy = model.DiemGiuaKy;
                    diem.DiemCuoiKy = model.DiemCuoiKy;
                    diem.NgayCapNhat = DateTime.Now;
                    diem.GiaoVienId = gv.Id;
                }

                await _db.SaveChangesAsync();

                // Đồng bộ sang DiemHocKy cho học sinh và admin thấy
                if (hs != null)
                {
                    if (diemHK == null)
                    {
                        diemHK = new DiemHocKy
                        {
                            HocSinhId = hs.Id,
                            MonHocId = model.MonHocId,
                            LopId = hs.LopId,
                            NamHoc = namHoc,
                            HocKy = hocKy,
                            NgayNhap = DateTime.Now
                        };
                        _db.DiemHocKys.Add(diemHK);
                    }

                    diemHK.DiemMieng1 = model.DiemMieng;
                    diemHK.DiemMieng2 = model.DiemMieng2;
                    diemHK.DiemMieng3 = model.DiemMieng3;
                    diemHK.DiemMieng4 = model.DiemMieng4;
                    diemHK.DiemGiuaKy = model.DiemGiuaKy;
                    diemHK.DiemCuoiKy = model.DiemCuoiKy;
                    diemHK.NgayCapNhat = DateTime.Now;
                    diemHK.GiaoVienId = gv.Id;

                    // Tính điểm tổng kết khi cả 3 cột điểm đã được chốt
                    if (diemHK.IsChotMieng && diemHK.IsChotGiuaKy && diemHK.IsChotCuoiKy)
                    {
                        var listDiem = new List<double>();
                        if (diemHK.DiemMieng1.HasValue) listDiem.Add(diemHK.DiemMieng1.Value);
                        if (diemHK.DiemMieng2.HasValue) listDiem.Add(diemHK.DiemMieng2.Value);
                        if (diemHK.DiemMieng3.HasValue) listDiem.Add(diemHK.DiemMieng3.Value);
                        if (diemHK.DiemMieng4.HasValue) listDiem.Add(diemHK.DiemMieng4.Value);

                        if (diemHK.DiemGiuaKy.HasValue && diemHK.DiemCuoiKy.HasValue)
                        {
                            double avgMieng = listDiem.Any() ? listDiem.Average() : 0;
                            diemHK.DiemTongKet = Math.Round((avgMieng + diemHK.DiemGiuaKy.Value * 2 + diemHK.DiemCuoiKy.Value * 3) / 6, 1);
                            
                            diemHK.XepLoai = diemHK.DiemTongKet >= 8.0 ? "Giỏi"
                                           : diemHK.DiemTongKet >= 6.5 ? "Khá"
                                           : diemHK.DiemTongKet >= 5.0 ? "Trung bình"
                                           : diemHK.DiemTongKet >= 3.5 ? "Yếu"
                                           : "Kém";
                        }
                    }
                    else
                    {
                        diemHK.DiemTongKet = null;
                        diemHK.XepLoai = null;
                    }
                    
                    await _db.SaveChangesAsync();
                }

                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Success. Saved grades for student {model.HocSinhId}.{Environment.NewLine}");
                return Json(new { success = true, message = "Đã lưu điểm thành công!" });
            }
            catch (Exception ex)
            {
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: Exception: {ex.ToString()}{Environment.NewLine}");
                System.IO.File.AppendAllText("luudiem_error.log", DateTime.Now.ToString() + ": " + ex.ToString() + Environment.NewLine);
                return Json(new { success = false, message = "Lỗi máy chủ khi lưu điểm: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChotDiem([FromBody] ChotDiemRequest model)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return Json(new { success = false, message = "Không xác thực giáo viên" });

            // Check if teacher is assigned to this class and subject
            var servesThisSubject = await _db.LopMonHocs
                .AnyAsync(lm => lm.LopId == model.LopId && lm.MonHocId == model.MonHocId && lm.GiaoVienId == gv.Id);

            if (!servesThisSubject)
            {
                return Json(new { success = false, message = "Bạn không được phân công giảng dạy môn học này cho lớp." });
            }

            var hocSinhList = await _db.NguoiDungs
                .Where(u => u.LopId == model.LopId)
                .ToListAsync();

            if (!hocSinhList.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy học sinh nào trong lớp." });
            }

            var hsIds = hocSinhList.Select(h => h.Id).ToList();

            var diemHKList = await _db.DiemHocKys
                .Where(x => x.MonHocId == model.MonHocId && hsIds.Contains(x.HocSinhId) && x.NamHoc == model.NamHoc && x.HocKy == model.HocKy)
                .ToListAsync();

            // Validate that all students have the required grade filled
            foreach (var hs in hocSinhList)
            {
                var dhk = diemHKList.FirstOrDefault(d => d.HocSinhId == hs.Id);
                if (model.LoaiDiem == "Mieng")
                {
                    if (dhk == null || (!dhk.DiemMieng1.HasValue && !dhk.DiemMieng2.HasValue && !dhk.DiemMieng3.HasValue && !dhk.DiemMieng4.HasValue))
                    {
                        return Json(new { success = false, message = $"Học sinh {hs.HoTen} chưa có điểm miệng. Vui lòng nhập đầy đủ điểm miệng cho tất cả học sinh trước khi chốt!" });
                    }
                }
                else if (model.LoaiDiem == "GiuaKy")
                {
                    if (dhk == null || !dhk.DiemGiuaKy.HasValue)
                    {
                        return Json(new { success = false, message = $"Học sinh {hs.HoTen} chưa có điểm giữa kỳ. Vui lòng nhập đầy đủ điểm giữa kỳ cho tất cả học sinh trước khi chốt!" });
                    }
                }
                else if (model.LoaiDiem == "CuoiKy")
                {
                    if (dhk == null || !dhk.DiemCuoiKy.HasValue)
                    {
                        return Json(new { success = false, message = $"Học sinh {hs.HoTen} chưa có điểm cuối kỳ. Vui lòng nhập đầy đủ điểm cuối kỳ cho tất cả học sinh trước khi chốt!" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Loại điểm không hợp lệ!" });
                }
            }

            // Perform locking
            foreach (var hs in hocSinhList)
            {
                var dhk = diemHKList.FirstOrDefault(d => d.HocSinhId == hs.Id);
                if (dhk != null)
                {
                    if (model.LoaiDiem == "Mieng") dhk.IsChotMieng = true;
                    else if (model.LoaiDiem == "GiuaKy") dhk.IsChotGiuaKy = true;
                    else if (model.LoaiDiem == "CuoiKy") dhk.IsChotCuoiKy = true;

                    // Re-evaluate DiemTongKet if all are locked
                    if (dhk.IsChotMieng && dhk.IsChotGiuaKy && dhk.IsChotCuoiKy)
                    {
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
                    }
                    else
                    {
                        dhk.DiemTongKet = null;
                        dhk.XepLoai = null;
                    }
                }
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Chốt điểm thành công!" });
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.3  ĐIỂM SỐ
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> QuanLyDiemSo(int lopId = 0, int monHocId = 0, int khoiId = 0, string? namHoc = null, int? hocKy = null)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            if (!hocKy.HasValue)
            {
                hocKy = (DateTime.Now.Month >= 8 || DateTime.Now.Month <= 1) ? 1 : 2;
            }

            var lopMonHocs = await _db.LopMonHocs
                .Include(x => x.Lop).ThenInclude(l => l!.Khoi)
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id)
                .ToListAsync();

            // Danh sách khối từ các lớp GV dạy
            var danhSachKhoi = lopMonHocs
                .Where(x => x.Lop?.Khoi != null)
                .Select(x => x.Lop!.Khoi)
                .DistinctBy(k => k.Id)
                .OrderBy(k => k.TenKhoi)
                .ToList();

            // Check if teacher is authorized for the selected lopId and monHocId (prevent URL tampering)
            if (lopId > 0 && monHocId > 0)
            {
                var isAuthorized = lopMonHocs.Any(x => x.LopId == lopId && x.MonHocId == monHocId);
                if (!isAuthorized)
                {
                    if (lopMonHocs.Any())
                    {
                        var firstAssigned = lopMonHocs.First();
                        return RedirectToAction("QuanLyDiemSo", new { lopId = firstAssigned.LopId, monHocId = firstAssigned.MonHocId, khoiId = firstAssigned.Lop?.MaKhoi ?? 0 });
                    }
                    else
                    {
                        lopId = 0;
                        monHocId = 0;
                    }
                }
            }

            // Chọn khối mặc định
            if (khoiId == 0 && danhSachKhoi.Any())
                khoiId = danhSachKhoi.First().Id;

            // Lọc LopMonHoc theo khối đang chọn
            var lopMonHocsOfKhoi = khoiId > 0
                ? lopMonHocs.Where(x => x.Lop?.MaKhoi == khoiId).ToList()
                : lopMonHocs;

            if ((lopId == 0 || monHocId == 0) && lopMonHocsOfKhoi.Any())
            {
                if (lopId == 0) lopId = lopMonHocsOfKhoi.First().LopId;
                var monOfLop = lopMonHocsOfKhoi.Where(x => x.LopId == lopId).ToList();
                if (monHocId == 0 && monOfLop.Any()) monHocId = monOfLop.First().MonHocId;
            }

            var hocSinhList = (await _db.NguoiDungs
                .Where(u => u.LopId == lopId)
                .ToListAsync())
                .OrderBy(u => u.HoTen != null && u.HoTen.Contains(' ')
                    ? u.HoTen.Substring(u.HoTen.LastIndexOf(' ') + 1) : u.HoTen)
                .ThenBy(u => u.HoTen)
                .ToList();

            var hsIds = hocSinhList.Select(h => h.Id).ToList();

            // Danh sách năm học có điểm của các học sinh này trong hệ thống
            var dsNamHocAll = await _db.DiemHocKys
                .Where(x => hsIds.Contains(x.HocSinhId))
                .Select(x => x.NamHoc)
                .Distinct()
                .OrderByDescending(n => n)
                .ToListAsync();

            // Năm học hiện tại: ưu tiên NamHoc trên record học sinh,
            // nếu không có thì lấy năm học xuất hiện nhiều nhất trong DiemHocKy (tránh lấy nhầm năm bất thường),
            // cuối cùng mới tính theo ngày hiện tại
            var studentWithNamHoc = hocSinhList.FirstOrDefault(x => !string.IsNullOrEmpty(x.NamHoc));
            string namHocHienTai;
            if (!string.IsNullOrEmpty(studentWithNamHoc?.NamHoc))
            {
                namHocHienTai = studentWithNamHoc.NamHoc!;
            }
            else
            {
                // Lấy năm học xuất hiện nhiều nhất trong DiemHocKy của các học sinh này
                var mostCommonNamHoc = await _db.DiemHocKys
                    .Where(x => hsIds.Contains(x.HocSinhId))
                    .GroupBy(x => x.NamHoc)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync();
                namHocHienTai = mostCommonNamHoc ?? (DateTime.Now.Month >= 9
                    ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                    : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}");
            }

            // Lọc danh sách năm học theo cấp lớp (Khối 10: chỉ năm hiện tại; 11: +1; 12: +2)
            var tenKhoiHienTai = danhSachKhoi.FirstOrDefault(k => k.Id == khoiId)?.TenKhoi ?? "Khối 10";
            var dsNamHoc = LimitNamHocByKhoi(dsNamHocAll, namHocHienTai, tenKhoiHienTai);

            // Nếu namHoc được truyền vào nhưng không nằm trong danh sách được phép → reset
            if (!string.IsNullOrEmpty(namHoc) && !dsNamHoc.Contains(namHoc))
                namHoc = null;

            // Xác định chế độ: lịch sử (namHoc != null && != namHocHienTai) hay hiện tại
            bool isLichSu = !string.IsNullOrEmpty(namHoc) && namHoc != namHocHienTai;

            var diemItems = new List<LMS_THPT.ViewModels.NhapDiemItem>();

            if (isLichSu && !string.IsNullOrEmpty(namHoc))
            {
                // Tìm môn học tương ứng trong năm học đó (dựa trên tiền tố tên môn, ví dụ: "Toán học 12" -> "Toán học")
                var currentMon = await _db.DanhSachMonHoc.FindAsync(monHocId);
                string baseMonName = currentMon?.TenMonHoc ?? "";
                var parts = baseMonName.Split(' ');
                if (parts.Length > 1 && int.TryParse(parts[^1], out _))
                {
                    baseMonName = string.Join(" ", parts.Take(parts.Length - 1));
                }

                var matchingMonHocIds = await _db.DanhSachMonHoc
                    .Where(m => m.TenMonHoc.StartsWith(baseMonName))
                    .Select(m => m.Id)
                    .ToListAsync();

                // ── CHẾ ĐỘ LỊCH SỬ: đọc từ DiemHocKys (read-only) ──
                var queryDiemHK = _db.DiemHocKys
                    .Where(x => matchingMonHocIds.Contains(x.MonHocId) && hsIds.Contains(x.HocSinhId) && x.NamHoc == namHoc);

                if (hocKy.HasValue)
                    queryDiemHK = queryDiemHK.Where(x => x.HocKy == hocKy.Value);

                var diemHKList = await queryDiemHK.ToListAsync();

                foreach (var hs in hocSinhList)
                {
                    // Lấy record HK được chọn (hoặc HK1 nếu không lọc)
                    var dHK = diemHKList.FirstOrDefault(x => x.HocSinhId == hs.Id && (!hocKy.HasValue || x.HocKy == hocKy.Value))
                           ?? diemHKList.FirstOrDefault(x => x.HocSinhId == hs.Id);

                    diemItems.Add(new LMS_THPT.ViewModels.NhapDiemItem
                    {
                        HocSinhId    = hs.Id,
                        TenHocSinh   = hs.HoTen ?? hs.UserName ?? "",
                        DiemMieng    = dHK?.DiemMieng1,
                        DiemMieng2   = dHK?.DiemMieng2,
                        DiemMieng3   = dHK?.DiemMieng3,
                        DiemMieng4   = null,
                        DiemGiuaKy   = dHK?.DiemGiuaKy,
                        DiemCuoiKy   = dHK?.DiemCuoiKy,
                        DiemTongKet  = dHK?.DiemTongKet,
                        XepLoai      = dHK?.XepLoai ?? "--",
                        DiemSoId     = 0,
                        IsReadOnly   = true
                    });
                }
            }
            else
            {
                // ── CHẾ ĐỘ HIỆN TẠI: đọc/ghi từ DiemHocKys theo học kỳ được chọn ──
                var danhSachDiemHK = await _db.DiemHocKys
                    .Where(x => x.MonHocId == monHocId && hsIds.Contains(x.HocSinhId) && x.NamHoc == namHocHienTai && x.HocKy == hocKy)
                    .ToListAsync();

                var firstDiemHK = danhSachDiemHK.FirstOrDefault();
                ViewBag.IsChotMieng = firstDiemHK?.IsChotMieng ?? false;
                ViewBag.IsChotGiuaKy = firstDiemHK?.IsChotGiuaKy ?? false;
                ViewBag.IsChotCuoiKy = firstDiemHK?.IsChotCuoiKy ?? false;

                foreach (var hs in hocSinhList)
                {
                    var diemHK = danhSachDiemHK.FirstOrDefault(d => d.HocSinhId == hs.Id);
                    bool fullyLocked = diemHK?.IsChotMieng == true && diemHK?.IsChotGiuaKy == true && diemHK?.IsChotCuoiKy == true;

                    diemItems.Add(new LMS_THPT.ViewModels.NhapDiemItem
                    {
                        HocSinhId   = hs.Id,
                        TenHocSinh  = hs.HoTen ?? hs.UserName ?? "",
                        DiemGiuaKy  = diemHK?.DiemGiuaKy,
                        DiemCuoiKy  = diemHK?.DiemCuoiKy,
                        DiemTongKet = fullyLocked ? diemHK?.DiemTongKet : null,
                        XepLoai     = fullyLocked ? (diemHK?.XepLoai ?? "--") : "--",
                        DiemSoId    = diemHK?.Id ?? 0,
                        DiemMieng   = diemHK?.DiemMieng1,
                        DiemMieng2  = diemHK?.DiemMieng2,
                        DiemMieng3  = diemHK?.DiemMieng3,
                        DiemMieng4  = diemHK?.DiemMieng4,
                        IsReadOnly  = false,
                        IsChotMieng = diemHK?.IsChotMieng ?? false,
                        IsChotGiuaKy = diemHK?.IsChotGiuaKy ?? false,
                        IsChotCuoiKy = diemHK?.IsChotCuoiKy ?? false
                    });
                }
            }

            ViewBag.LopMonHocs       = lopMonHocsOfKhoi;
            ViewBag.AllLopMonHocs    = lopMonHocs;
            ViewBag.DanhSachKhoi     = danhSachKhoi;
            ViewBag.KhoiHienTai      = khoiId;
            ViewBag.LopHienTai       = lopId;
            ViewBag.MonHocHienTai    = monHocId;
            ViewBag.DsNamHoc         = dsNamHoc;
            ViewBag.SelectedNamHoc   = namHoc;
            ViewBag.SelectedHocKy    = hocKy;
            ViewBag.NamHocHienTai    = namHocHienTai;
            ViewBag.IsLichSu         = isLichSu;

            // Khi xem năm cũ + Cả năm: tính bảng tổng kết mỗi học sinh
            if (isLichSu && !hocKy.HasValue && !string.IsNullOrEmpty(namHoc))
            {
                // Lấy tất cả các records (cả HK1 lẫn HK2) cho lớp+môn+năm
                var currentMon2 = await _db.DanhSachMonHoc.FindAsync(monHocId);
                string baseMonName2 = currentMon2?.TenMonHoc ?? "";
                var parts2 = baseMonName2.Split(' ');
                if (parts2.Length > 1 && int.TryParse(parts2[^1], out _))
                    baseMonName2 = string.Join(" ", parts2.Take(parts2.Length - 1));

                var matchingMonIds2 = await _db.DanhSachMonHoc
                    .Where(m => m.TenMonHoc.StartsWith(baseMonName2))
                    .Select(m => m.Id)
                    .ToListAsync();

                var allDiemHK = await _db.DiemHocKys
                    .Where(x => matchingMonIds2.Contains(x.MonHocId) && hsIds.Contains(x.HocSinhId) && x.NamHoc == namHoc)
                    .ToListAsync();

                var tbHSSummary = hocSinhList.Select(hs =>
                {
                    var d1 = allDiemHK.FirstOrDefault(x => x.HocSinhId == hs.Id && x.HocKy == 1);
                    var d2 = allDiemHK.FirstOrDefault(x => x.HocSinhId == hs.Id && x.HocKy == 2);
                    double? tk1 = d1?.DiemTongKet;
                    double? tk2 = d2?.DiemTongKet;
                    double? tbNam = (tk1.HasValue && tk2.HasValue)
                        ? Math.Round((tk1.Value + tk2.Value * 2) / 3, 2)
                        : (tk1 ?? tk2);
                    string? xepLoai = tbNam >= 8.0 ? "Giỏi"
                                    : tbNam >= 6.5 ? "Khá"
                                    : tbNam >= 5.0 ? "Trung bình"
                                    : tbNam >= 3.5 ? "Yếu"
                                    : tbNam.HasValue ? "Kém" : null;
                    return new
                    {
                        TenHocSinh = hs.HoTen ?? hs.UserName ?? "",
                        TkHK1 = tk1, TkHK2 = tk2, TbNam = tbNam, XepLoai = xepLoai
                    };
                }).ToList();
                ViewBag.TbHSSummary = tbHSSummary;
            }

            var currentLop = lopMonHocs.FirstOrDefault(x => x.LopId == lopId && x.MonHocId == monHocId);
            ViewBag.TenLopHienTai = currentLop?.Lop?.TenLop ?? "";
            ViewBag.TenMonHienTai = currentLop?.MonHoc?.TenMonHoc ?? "";
            ViewData["ActivePage"] = "DiemSo";

            return View(diemItems);
        }


        // ────────────────────────────────────────────────────────────────────
        // 3.4 BÀI TẬP & ĐÁNH GIÁ
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> QuanLyBaiTap(int lopId = 0, int monHocId = 0, int khoiId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            var lopMonHocs = await _db.LopMonHocs
                .Include(x => x.Lop).ThenInclude(l => l!.Khoi)
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id)
                .ToListAsync();

            // Danh sách khối
            var danhSachKhoi = lopMonHocs
                .Where(x => x.Lop?.Khoi != null)
                .Select(x => x.Lop!.Khoi)
                .DistinctBy(k => k.Id)
                .OrderBy(k => k.TenKhoi)
                .ToList();

            // Check if teacher is authorized for the selected lopId and monHocId (prevent URL tampering)
            if (lopId > 0 && monHocId > 0)
            {
                var isAuthorized = lopMonHocs.Any(x => x.LopId == lopId && x.MonHocId == monHocId);
                if (!isAuthorized)
                {
                    if (lopMonHocs.Any())
                    {
                        var firstAssigned = lopMonHocs.First();
                        return RedirectToAction("QuanLyBaiTap", new { lopId = firstAssigned.LopId, monHocId = firstAssigned.MonHocId, khoiId = firstAssigned.Lop?.MaKhoi ?? 0 });
                    }
                    else
                    {
                        lopId = 0;
                        monHocId = 0;
                    }
                }
            }

            if (khoiId == 0 && danhSachKhoi.Any())
                khoiId = danhSachKhoi.First().Id;

            // Lọc theo khối
            var lopMonHocsOfKhoi = khoiId > 0
                ? lopMonHocs.Where(x => x.Lop?.MaKhoi == khoiId).ToList()
                : lopMonHocs;

            // Nếu chưa chọn lớp → dùng lớp đầu tiên của khối
            if (lopId == 0 && lopMonHocsOfKhoi.Any())
                lopId = lopMonHocsOfKhoi.First().LopId;

            // Nếu chưa chọn môn (hoặc môn không thuộc lớp đang chọn) → dùng môn đầu tiên của lớp đó
            var monsOfLop = lopMonHocsOfKhoi.Where(x => x.LopId == lopId).ToList();
            if ((monHocId == 0 || !monsOfLop.Any(x => x.MonHocId == monHocId)) && monsOfLop.Any())
                monHocId = monsOfLop.First().MonHocId;

            var baiNops = await _db.BaiNops
                .Include(x => x.HocSinh)
                .ThenInclude(u => u.Lop)
                .Include(x => x.BaiTap)
                .Where(x => x.BaiTap.MonHocId == monHocId && x.HocSinh!.LopId == lopId)
                .OrderBy(x => x.Diem)
                .ThenByDescending(x => x.NgayNop)
                .Take(8)
                .Select(x => new BaiNopChopItem
                {
                    BaiNopId = x.Id,
                    TenHocSinh = x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? "",
                    TenLop = x.HocSinh!.Lop != null ? x.HocSinh.Lop.TenLop : "",
                    TenVietTat = GetInitials(x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? ""),
                    TenBaiTap = x.BaiTap!.TieuDe ?? "",
                    ThoiGianNop = FormatTimeAgo(x.NgayNop),
                    TrangThai = x.Diem == null ? "pending" : "graded",
                    Diem = x.Diem
                })
                .ToListAsync();

            var diemBaiTap = await _db.BaiNops
                .Include(x => x.HocSinh)
                .ThenInclude(u => u.Lop)
                .Include(x => x.BaiTap)
                .Where(x => x.BaiTap.MonHocId == monHocId && x.HocSinh!.LopId == lopId)
                .Select(x => new BaiNopDiemItem
                {
                    BaiNopId = x.Id,
                    TenHocSinh = x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? "",
                    TenLop = x.HocSinh!.Lop != null ? x.HocSinh.Lop.TenLop : "",
                    TenBaiTap = x.BaiTap!.TieuDe ?? "",
                    NoiDung = x.NoiDung,
                    DuongDanFile = x.DuongDanFile,
                    Diem = x.Diem,
                    NhanXet = x.NhanXet ?? "",
                    TrangThai = x.Diem == null ? "pending" : "graded"
                })
                .ToListAsync();

            // Lấy danh sách bài tập thực tế để hiển thị cards
            var danhSachBaiTap = await _db.BaiTaps
                .Where(x => x.MonHocId == monHocId && (x.LopId == null || x.LopId == lopId))
                .OrderByDescending(x => x.NgayTao)
                .ToListAsync();

            var vm = new BaiTapManageViewModel
            {
                BaiNopChoChoam = baiNops,
                DanhSachDiem = diemBaiTap
            };

            ViewBag.LopMonHocs = lopMonHocsOfKhoi;
            ViewBag.AllLopMonHocs = lopMonHocs;
            ViewBag.DanhSachKhoi = danhSachKhoi;
            ViewBag.KhoiHienTai = khoiId;
            ViewBag.LopHienTai = lopId;
            ViewBag.MonHocHienTai = monHocId;
            ViewBag.DanhSachBaiTap = danhSachBaiTap;

            var currentLop = lopMonHocs.FirstOrDefault(x => x.LopId == lopId && x.MonHocId == monHocId);
            ViewBag.TenLopHienTai = currentLop?.Lop?.TenLop ?? "";
            ViewBag.TenMonHienTai = currentLop?.MonHoc?.TenMonHoc ?? "";

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> LuuDiemBaiTap([FromBody] LuuDiemBaiTapRequest model)
        {
            var baiNop = await _db.BaiNops
                .Include(b => b.BaiTap)
                .FirstOrDefaultAsync(b => b.Id == model.BaiNopId);
            if (baiNop == null) return Json(new { success = false, message = "Không tìm thấy bài nộp." });

            baiNop.Diem = model.Diem;
            baiNop.NhanXet = model.NhanXet;
            baiNop.TrangThai = TrangThaiBaiNop.ChamXong;
            baiNop.NgayCham = DateTime.Now;

            // ── Đồng bộ sang DiemHocKy nếu loại bài tập ghi vào sổ điểm ──
            var loaiDiem = baiNop.BaiTap?.LoaiDiem ?? LoaiDiem.BaiTap;
            var monHocId = baiNop.BaiTap?.MonHocId;
            var hocSinhId = baiNop.HocSinhId;

            if (monHocId.HasValue && loaiDiem != LoaiDiem.BaiTap)
            {
                var gv = await _userManager.GetUserAsync(User);
                var hs = await _db.Users.FirstOrDefaultAsync(u => u.Id == hocSinhId);
                string namHoc = hs?.NamHoc ?? string.Empty;
                if (string.IsNullOrEmpty(namHoc))
                {
                    namHoc = DateTime.Now.Month >= 9
                        ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                        : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
                }
                int hocKy = (baiNop.BaiTap != null && baiNop.BaiTap.HocKy > 0)
                    ? baiNop.BaiTap.HocKy
                    : ((DateTime.Now.Month >= 8 || DateTime.Now.Month <= 1) ? 1 : 2);

                // Kiểm tra chốt điểm
                var diemHK = await _db.DiemHocKys
                    .FirstOrDefaultAsync(d => d.HocSinhId == hocSinhId
                        && d.MonHocId == monHocId.Value
                        && d.NamHoc == namHoc
                        && d.HocKy == hocKy);

                if (diemHK != null)
                {
                    if (loaiDiem == LoaiDiem.MiengKiemTra && diemHK.IsChotMieng)
                        return Json(new { success = false, message = $"Điểm miệng HK{hocKy} ({namHoc}) đã được chốt!" });
                    if (loaiDiem == LoaiDiem.GiuaKy && diemHK.IsChotGiuaKy)
                        return Json(new { success = false, message = $"Điểm giữa kỳ HK{hocKy} ({namHoc}) đã được chốt!" });
                    if (loaiDiem == LoaiDiem.CuoiKy && diemHK.IsChotCuoiKy)
                        return Json(new { success = false, message = $"Điểm cuối kỳ HK{hocKy} ({namHoc}) đã được chốt!" });
                }

                // Cập nhật DiemSo
                var diemSo = await _db.DiemSos
                    .FirstOrDefaultAsync(d => d.NguoiDungId == hocSinhId && d.MonHocId == monHocId.Value);
                if (diemSo == null)
                {
                    diemSo = new DiemSo { NguoiDungId = hocSinhId, MonHocId = monHocId.Value, NgayNhap = DateTime.Now };
                    _db.DiemSos.Add(diemSo);
                }
                if (loaiDiem == LoaiDiem.GiuaKy) diemSo.DiemGiuaKy = model.Diem;
                else if (loaiDiem == LoaiDiem.CuoiKy) diemSo.DiemCuoiKy = model.Diem;
                else if (loaiDiem == LoaiDiem.MiengKiemTra)
                {
                    int cotMieng = baiNop.BaiTap?.CotDiemMieng ?? 1;
                    if (cotMieng == 2) diemSo.DiemMieng2 = model.Diem;
                    else if (cotMieng == 3) diemSo.DiemMieng3 = model.Diem;
                    else if (cotMieng == 4) diemSo.DiemMieng4 = model.Diem;
                    else diemSo.Diem = model.Diem; // cot == 1
                }
                diemSo.NgayCapNhat = DateTime.Now;
                if (gv != null) diemSo.GiaoVienId = gv.Id;

                // Cập nhật DiemHocKy
                if (diemHK == null)
                {
                    diemHK = new DiemHocKy
                    {
                        HocSinhId = hocSinhId,
                        MonHocId = monHocId.Value,
                        LopId = hs?.LopId,
                        NamHoc = namHoc,
                        HocKy = hocKy,
                        NgayNhap = DateTime.Now
                    };
                    _db.DiemHocKys.Add(diemHK);
                }

                if (loaiDiem == LoaiDiem.MiengKiemTra)
                {
                    int cotMieng = baiNop.BaiTap?.CotDiemMieng ?? 1;
                    if (cotMieng == 2) diemHK.DiemMieng2 = model.Diem;
                    else if (cotMieng == 3) diemHK.DiemMieng3 = model.Diem;
                    else if (cotMieng == 4) diemHK.DiemMieng4 = model.Diem;
                    else diemHK.DiemMieng1 = model.Diem; // cot == 1
                }
                else if (loaiDiem == LoaiDiem.GiuaKy) diemHK.DiemGiuaKy = model.Diem;
                else if (loaiDiem == LoaiDiem.CuoiKy) diemHK.DiemCuoiKy = model.Diem;

                diemHK.NgayCapNhat = DateTime.Now;
                if (gv != null) diemHK.GiaoVienId = gv.Id;

                // Tính tổng kết nếu đã chốt cả 3 loại
                if (diemHK.IsChotMieng && diemHK.IsChotGiuaKy && diemHK.IsChotCuoiKy)
                {
                    var listMieng = new List<double>();
                    if (diemHK.DiemMieng1.HasValue) listMieng.Add(diemHK.DiemMieng1.Value);
                    if (diemHK.DiemMieng2.HasValue) listMieng.Add(diemHK.DiemMieng2.Value);
                    if (diemHK.DiemMieng3.HasValue) listMieng.Add(diemHK.DiemMieng3.Value);
                    if (diemHK.DiemMieng4.HasValue) listMieng.Add(diemHK.DiemMieng4.Value);
                    if (diemHK.DiemGiuaKy.HasValue && diemHK.DiemCuoiKy.HasValue)
                    {
                        double avgMieng = listMieng.Any() ? listMieng.Average() : 0;
                        diemHK.DiemTongKet = Math.Round((avgMieng + diemHK.DiemGiuaKy.Value * 2 + diemHK.DiemCuoiKy.Value * 3) / 6, 1);
                        diemHK.XepLoai = diemHK.DiemTongKet >= 8.0 ? "Giỏi"
                                       : diemHK.DiemTongKet >= 6.5 ? "Khá"
                                       : diemHK.DiemTongKet >= 5.0 ? "Trung bình"
                                       : diemHK.DiemTongKet >= 3.5 ? "Yếu" : "Kém";
                    }
                }
                else
                {
                    diemHK.DiemTongKet = null;
                    diemHK.XepLoai = null;
                }
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Đã lưu điểm!" });
        }
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> TestTienDo(int monHocId = 6)
        {
            var gv = await _db.Users.FirstOrDefaultAsync(u => u.Email == "gv1@lms.com");
            var src1 = await _db.MonHocGiaoViens.Where(x => x.NguoiDungId == gv.Id).Select(x => x.MonHocId).ToListAsync();
            var src2 = await _db.LopMonHocs.Where(x => x.GiaoVienId == gv.Id).Select(x => x.MonHocId).ToListAsync();
            var monHocIds = src1.Union(src2).Distinct().ToList();
            var allLopMonHocs = await _db.LopMonHocs.Include(x => x.Lop).Include(x => x.MonHoc).Where(x => monHocIds.Contains(x.MonHocId)).ToListAsync();
            var lopIdsViaMon = allLopMonHocs.Where(x => x.MonHocId == monHocId).Select(x => x.LopId).Distinct().ToList();
            var hocSinhList = await _db.NguoiDungs.Include(u => u.Lop).ThenInclude(l => l!.Khoi).Where(u => u.LopId.HasValue && lopIdsViaMon.Contains(u.LopId.Value)).ToListAsync();
            var result = hocSinhList.Select(u => new { u.HoTen, Lop = u.Lop?.TenLop }).ToList();
            return Json(new { monHocId, lopIdsViaMon, Students = result });
        }

        // ─────────────────────────────────────────────────────────────────────
        // 3.4  THEO DÕI TIẾN ĐỘ
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> TienDo(int monHocId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            // Chỉ lấy LopMonHoc mà giáo viên được phân công PHỤ TRÁCH
            // (GiaoVienId trong LopMonHoc là nguồn duy nhất hợp lệ)
            var allLopMonHocs = await _db.LopMonHocs
                .Include(x => x.Lop)
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id)
                .ToListAsync();

            var monHocIds = allLopMonHocs.Select(x => x.MonHocId).Distinct().ToList();
            
            // Danh sách môn học để hiển thị tab (chỉ các môn GV phụ trách)
            var danhSachMonHoc = allLopMonHocs
                .Where(x => x.MonHoc != null)
                .Select(x => x.MonHoc!)
                .DistinctBy(m => m.Id)
                .OrderBy(m => m.TenMonHoc)
                .ToList();

            if (monHocId == 0 && monHocIds.Any()) monHocId = monHocIds.First();

            // Chỉ lấy học sinh từ các lớp mà GV PHỤ TRÁCH môn đang chọn
            var lopIdsViaMon = allLopMonHocs
                .Where(x => x.MonHocId == monHocId)
                .Select(x => x.LopId).Distinct().ToList();

            // Lấy học sinh - chỉ filter LopId (GV/Admin không có LopId nên không lẫn)
            var hocSinhList = await _db.NguoiDungs
                .Include(u => u.Lop).ThenInclude(l => l!.Khoi)
                .Where(u => u.LopId.HasValue && lopIdsViaMon.Contains(u.LopId.Value))
                .OrderBy(u => u.Lop!.MaKhoi).ThenBy(u => u.Lop!.TenLop).ThenBy(u => u.HoTen)
                .ToListAsync();

            var hsIds = hocSinhList.Select(u => u.Id).ToList();

            var diemList = await _db.DiemSos
                .Where(x => x.MonHocId == monHocId && hsIds.Contains(x.NguoiDungId))
                .ToListAsync();

            // Lấy DiemHocKy cho năm học hiện tại để có điểm tổng kết chính xác
            var studentWithNamHocTD = hocSinhList.FirstOrDefault(x => !string.IsNullOrEmpty(x.NamHoc));
            string namHocHienTaiTD = studentWithNamHocTD?.NamHoc ?? (DateTime.Now.Month >= 9
                ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}");
            var diemHocKyList = await _db.DiemHocKys
                .Where(x => x.MonHocId == monHocId && hsIds.Contains(x.HocSinhId) && x.NamHoc == namHocHienTaiTD)
                .ToListAsync();

            // Đếm bài tập cho môn này (tất cả trạng thái)
            var tongBaiTap = await _db.BaiTaps.CountAsync(x => x.MonHocId == monHocId);

            var baiNopCounts = await _db.BaiNops
                .Where(x => hsIds.Contains(x.HocSinhId) && x.BaiTap.MonHocId == monHocId)
                .GroupBy(x => x.HocSinhId)
                .Select(g => new { HocSinhId = g.Key, SoLuong = g.Count() })
                .ToListAsync();

            // Xây tiến độ từng HS
            var tienDoHs = new List<TienDoHocSinhItem>();
            foreach (var u in hocSinhList)
            {
                var soBaiNop = baiNopCounts.FirstOrDefault(b => b.HocSinhId == u.Id)?.SoLuong ?? 0;
                var pct = tongBaiTap == 0 ? 0 : Math.Min(100, soBaiNop * 100 / tongBaiTap);

                // Ưu tiên DiemHocKy (có DiemTongKet đã tính sẵn)
                double tongKet = 0;
                var dhk = diemHocKyList.FirstOrDefault(d => d.HocSinhId == u.Id);
                if (dhk?.DiemTongKet.HasValue == true)
                {
                    tongKet = dhk.DiemTongKet.Value;
                }
                else
                {
                    // Fallback: tính từ DiemSo
                    var diem = diemList.FirstOrDefault(d => d.NguoiDungId == u.Id);
                    if (diem?.DiemGiuaKy.HasValue == true && diem.DiemCuoiKy.HasValue)
                    {
                        var dms = new List<double>();
                        if (diem.Diem.HasValue) dms.Add(diem.Diem.Value);
                        if (diem.DiemMieng2.HasValue) dms.Add(diem.DiemMieng2.Value);
                        if (diem.DiemMieng3.HasValue) dms.Add(diem.DiemMieng3.Value);
                        if (diem.DiemMieng4.HasValue) dms.Add(diem.DiemMieng4.Value);
                        tongKet = dms.Any()
                            ? Math.Round((dms.Average() + diem.DiemGiuaKy.Value * 2 + diem.DiemCuoiKy.Value * 3) / 6, 1)
                            : Math.Round(diem.DiemGiuaKy.Value * 0.4 + diem.DiemCuoiKy.Value * 0.6, 1);
                    }
                }

                tienDoHs.Add(new TienDoHocSinhItem
                {
                    TenHocSinh = u.HoTen ?? u.UserName ?? u.Id,
                    TenVietTat = GetInitials(u.HoTen ?? u.UserName ?? ""),
                    TenLop = u.Lop?.TenLop ?? "",
                    Diem = tongKet,
                    PhanTram = pct
                });
            }

            var xepLoai = new List<ThongKeXepLoaiItem>
            {
                new() { NhanXepLoai = "Xuất sắc",  Mau = "#1D4ED8" },
                new() { NhanXepLoai = "Giỏi",       Mau = "#166534" },
                new() { NhanXepLoai = "Khá",        Mau = "#92400E" },
                new() { NhanXepLoai = "Trung bình", Mau = "#6B7280" },
                new() { NhanXepLoai = "Yếu/Kém",    Mau = "#991B1B" },
            };
            foreach (var hs in tienDoHs)
            {
                var xl = hs.Diem switch
                {
                    >= 9.0 => "Xuất sắc", >= 8.0 => "Giỏi",
                    >= 6.5 => "Khá",      >= 5.0 => "Trung bình", _ => "Yếu/Kém"
                };
                xepLoai.First(x => x.NhanXepLoai == xl).SoLuong++;
            }
            int tongHs = tienDoHs.Count;
            foreach (var x in xepLoai)
                x.PhanTram = tongHs == 0 ? 0 : x.SoLuong * 100 / tongHs;

            var diemTB = tienDoHs.Any() ? Math.Round(tienDoHs.Average(x => x.Diem), 1) : 0;
            var lopList = tienDoHs.Select(x => x.TenLop).Where(s => !string.IsNullOrEmpty(s))
                .Distinct().OrderBy(s => s).ToList();

            // Thống kê theo từng khối (grade) và từng lớp trong khối
            // lopList dựa trên tienDoHs, nhưng cần lấy thêm TenKhoi từ hocSinhList
            var thongKeLopList = hocSinhList
                .GroupBy(u => new { KhoiName = u.Lop?.Khoi?.TenKhoi ?? u.Lop?.TenLop?.Substring(0, 2) ?? "?", TenLop = u.Lop?.TenLop ?? "" })
                .OrderBy(g => g.Key.KhoiName).ThenBy(g => g.Key.TenLop)
                .Select(g =>
                {
                    var tenLop = g.Key.TenLop;
                    var hsLop = tienDoHs.Where(h => h.TenLop == tenLop).ToList();
                    double diemTBLop = hsLop.Any(h => h.Diem > 0)
                        ? Math.Round(hsLop.Where(h => h.Diem > 0).Average(h => h.Diem), 1) : 0;
                    return new ThongKeLopItem
                    {
                        TenLop = tenLop,
                        TenKhoi = g.Key.KhoiName,
                        SoHocSinh = g.Count(),
                        DiemTB = diemTBLop,
                        TiLeHoanThanh = hsLop.Any() ? (int)hsLop.Average(h => h.PhanTram) : 0,
                        SoXuatSac = hsLop.Count(h => h.Diem >= 9),
                        SoYeu = hsLop.Count(h => h.Diem > 0 && h.Diem < 5)
                    };
                }).ToList();

            var vm = new TienDoViewModel
            {
                DiemTrungBinhLop = diemTB,
                TiLeHoanThanh = tienDoHs.Any() ? (int)tienDoHs.Average(x => x.PhanTram) : 0,
                SoHocSinhXuatSac = tienDoHs.Count(x => x.Diem >= 9.0),
                SoHocSinhCanHoTro = tienDoHs.Count(x => x.Diem > 0 && x.Diem < 5.0),
                TienDoHocSinh = tienDoHs.OrderBy(x => x.TenLop).ThenByDescending(x => x.Diem).ToList(),
                ThongKeXepLoai = xepLoai,
                DanhSachLop = lopList
            };

            ViewBag.MonHocIds = monHocIds;
            ViewBag.MonHocHienTai = monHocId;
            ViewBag.AllLopMonHocs = allLopMonHocs;
            ViewBag.ThongKeLop = thongKeLopList;
            ViewBag.DanhSachMon = danhSachMonHoc;
            ViewBag.TongBaiTap = tongBaiTap;
            ViewBag.SoLopHoc = lopList.Count;
            ViewBag.TongHocSinh = tongHs;
            return View(vm);
        }


        // ────────────────────────────────────────────────────────────────────
        // 3.5  LỚP CHỦ NHIỆM
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> LopChuNhiem(string? namHoc = null, int? hocKy = null)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            var lop = await _db.Lops
                .Include(l => l.Khoi)
                .FirstOrDefaultAsync(l => l.GiaoVienChuNhiemId == gv.Id);

            var hocSinhList = new List<HocSinhLopItem>();

            var danhSachMonHocLop = new List<string>();

            if (lop != null)
            {
                var users = await _db.NguoiDungs
                    .Where(u => u.LopId == lop.Id)
                    .ToListAsync();

                // Xác định năm học hiện tại của học sinh trong lớp
                var studentWithNamHoc = users.FirstOrDefault(x => !string.IsNullOrEmpty(x.NamHoc));
                string namHocHienTai = studentWithNamHoc?.NamHoc ?? (DateTime.Now.Month >= 9
                    ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                    : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}");

                // Lấy danh sách năm học được xem dựa trên Khối
                string tenKhoi = lop.Khoi?.TenKhoi ?? "Khối 10";
                var allPossibleYears = new List<string>
                {
                    namHocHienTai,
                    GetPreviousAcademicYear(namHocHienTai, 1),
                    GetPreviousAcademicYear(namHocHienTai, 2)
                };
                var dsNamHoc = LimitNamHocByKhoi(allPossibleYears, namHocHienTai, tenKhoi);

                // Kiểm tra năm học được chọn hợp lệ
                if (!string.IsNullOrEmpty(namHoc) && !dsNamHoc.Contains(namHoc))
                {
                    namHoc = null;
                }
                string selectedNamHoc = namHoc ?? (dsNamHoc.FirstOrDefault() ?? namHocHienTai);
                int selectedHocKy = hocKy ?? ((DateTime.Now.Month >= 8 || DateTime.Now.Month <= 1) ? 1 : 2);

                bool isPastYear = selectedNamHoc != namHocHienTai;
                ViewBag.IsPastYear = isPastYear;
                ViewBag.SelectedNamHoc = selectedNamHoc;
                ViewBag.SelectedHocKy = selectedHocKy;
                ViewBag.DsNamHoc = dsNamHoc;

                var studentIds = users.Select(u => u.Id).ToList();

                List<LopMonHoc> lopMonHocs = new();
                if (!isPastYear)
                {
                    lopMonHocs = await _db.LopMonHocs
                        .Include(x => x.MonHoc)
                        .Where(x => x.LopId == lop.Id)
                        .ToListAsync();

                    danhSachMonHocLop = lopMonHocs
                        .Where(x => x.MonHoc != null)
                        .Select(m => m.MonHoc!.TenMonHoc ?? "")
                        .Distinct()
                        .ToList();
                }
                else
                {
                    // Cho năm cũ, lấy các môn học có điểm thực tế của nhóm học sinh này trong năm đó
                    var previousDiemHKs = await _db.DiemHocKys
                        .Include(d => d.MonHoc)
                        .Where(d => studentIds.Contains(d.HocSinhId) && d.HocKy == selectedHocKy && d.NamHoc == selectedNamHoc)
                        .ToListAsync();

                    danhSachMonHocLop = previousDiemHKs
                        .Where(x => x.MonHoc != null)
                        .Select(x => x.MonHoc!.TenMonHoc)
                        .Distinct()
                        .OrderBy(name => name)
                        .ToList();
                }

                var diemHocKys = await _db.DiemHocKys
                    .Where(d => studentIds.Contains(d.HocSinhId) && d.HocKy == selectedHocKy && d.NamHoc == selectedNamHoc)
                    .ToListAsync();

                // Sắp xếp theo tên gọi (từ cuối trong họ tên) - đúng chuẩn bảng chữ cái Tiếng Việt
                users = users
                    .OrderBy(u => u.HoTen != null && u.HoTen.Contains(' ')
                        ? u.HoTen.Substring(u.HoTen.LastIndexOf(' ') + 1)
                        : u.HoTen)
                    .ThenBy(u => u.HoTen)
                    .ToList(); // sort in-memory (after ToListAsync above)

                hocSinhList = users.Select(u => {
                    var diemTungMon = new Dictionary<string, double?>();
                    
                    if (!isPastYear)
                    {
                        foreach(var lm in lopMonHocs)
                        {
                            if (lm.MonHoc == null) continue;
                            var dHK = diemHocKys.FirstOrDefault(x => x.HocSinhId == u.Id && x.MonHocId == lm.MonHocId);
                            double? tongKet = null;
                            if (dHK != null && dHK.IsChotMieng && dHK.IsChotGiuaKy && dHK.IsChotCuoiKy)
                            {
                                tongKet = dHK.DiemTongKet;
                            }
                            diemTungMon[lm.MonHoc.TenMonHoc ?? ""] = tongKet;
                        }
                    }
                    else
                    {
                        foreach(var monName in danhSachMonHocLop)
                        {
                            var dHK = diemHocKys.FirstOrDefault(x => x.HocSinhId == u.Id && x.MonHoc?.TenMonHoc == monName);
                            double? tongKet = null;
                            if (dHK != null)
                            {
                                tongKet = dHK.DiemTongKet;
                            }
                            diemTungMon[monName] = tongKet;
                        }
                    }

                    var danhSachCoDiem = diemTungMon.Values.Where(x => x.HasValue).Select(x => x!.Value).ToList();
                    double? diemTb = danhSachCoDiem.Any() ? Math.Round(danhSachCoDiem.Average(), 1) : null;

                    return new HocSinhLopItem
                    {
                        Id = u.Id,
                        HoTen = u.HoTen ?? u.UserName ?? "",
                        TenVietTat = GetInitials(u.HoTen ?? u.UserName ?? ""),
                        GioiTinh = u.GioiTinh ?? "",
                        NgaySinh = u.NgaySinh.HasValue ? u.NgaySinh.Value.ToString("dd/MM/yyyy") : null,
                        DiaChi = u.DiaChi,
                        AnhDaiDien = u.AnhDaiDien,
                        Email = u.Email ?? "",
                        HanhKiem = string.IsNullOrEmpty(u.HanhKiem) ? "Chưa đánh giá" : u.HanhKiem,
                        DiemTrungBinh = diemTb,
                        DiemTungMon = diemTungMon
                    };
                }).ToList();
            }

            var today = DateTime.Today;
            var lichHomNay = new List<LichHocItem>();
            if (lop != null)
            {
                var monHocIdsLop = await _db.LopMonHocs
                    .Where(x => x.LopId == lop.Id)
                    .Select(x => x.MonHocId)
                    .ToListAsync();

                lichHomNay = await _db.LichHocs
                    .Include(x => x.MonHoc)
                    .Where(x => x.LopId == lop.Id && x.NgayHoc.Date == today)
                    .OrderBy(x => x.GioBatDau)
                    .Select(x => new LichHocItem
                    {
                        ThoiGian = x.GioBatDau.ToString(@"hh\:mm") + " – " + x.GioKetThuc.ToString(@"hh\:mm"),
                        TenMon = x.MonHoc!.TenMonHoc ?? "",
                        PhongLop = x.PhongHoc ?? "",
                        NhanManh = false
                    })
                    .ToListAsync();
            }

            var yeuCauGanDay = new List<YeuCauGiaoVien>();
            if (lop != null)
            {
                yeuCauGanDay = await _db.YeuCauGiaoViens
                    .Where(x => x.LopId == lop.Id)
                    .OrderByDescending(x => x.NgayGui)
                    .Take(5)
                    .ToListAsync();
            }

            var vm = new LopChuNhiemViewModel
            {
                LopChuNhiem = lop,
                DanhSachHocSinh = hocSinhList,
                LichHomNay = lichHomNay,
                YeuCauGanDay = yeuCauGanDay,
                DanhSachMonHoc = danhSachMonHocLop
            };

            ViewData["ActivePage"] = "LopChuNhiem";
            ViewData["Title"] = lop != null ? $"Lớp {lop.TenLop}" : "Lớp chủ nhiệm";
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> LuuHanhKiem([FromBody] LuuHanhKiemRequest request)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return Json(new { success = false, message = "Không xác thực" });

            var lop = await _db.Lops.FirstOrDefaultAsync(l => l.GiaoVienChuNhiemId == gv.Id);
            if (lop == null) return Json(new { success = false, message = "Không phải GVCN" });

            var hs = await _db.NguoiDungs.FirstOrDefaultAsync(u => u.Id == request.HocSinhId && u.LopId == lop.Id);
            if (hs == null) return Json(new { success = false, message = "Học sinh không thuộc lớp chủ nhiệm" });

            hs.HanhKiem = request.HanhKiem;
            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.6  LỊCH DẠY
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> LichDay()
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            // Chỉ lấy các cặp (lớp, môn) mà GV được phân công PHỤ TRÁCH
            var lopMonHocs = await _db.LopMonHocs
                .Include(x => x.Lop).ThenInclude(l => l!.Khoi)
                .Include(x => x.MonHoc)
                .Where(x => x.GiaoVienId == gv.Id)
                .ToListAsync();

            // Danh sách lớp GV phụ trách
            var danhSachLop = lopMonHocs
                .Where(x => x.Lop != null)
                .Select(x => x.Lop!)
                .DistinctBy(l => l.Id)
                .OrderBy(l => l.MaKhoi).ThenBy(l => l.TenLop)
                .ToList();

            var monHocIds = lopMonHocs.Select(x => x.MonHocId).Distinct().ToList();

            // Tập hợp (LopId, MonHocId) mà GV phụ trách — để lọc chính xác lịch
            var phuTrachSet = lopMonHocs
                .Select(x => (x.LopId, x.MonHocId))
                .ToHashSet();

            // Lấy lịch chỉ của các (lớp, môn) GV phụ trách
            var lichHocRaw = await _db.LichHocs
                .Include(x => x.MonHoc)
                .Include(x => x.Lop)
                .Where(x => x.GiaoVienId == gv.Id)
                .OrderBy(x => x.Thu).ThenBy(x => x.TietHoc)
                .ToListAsync();

            // Lọc thêm: chỉ giữ lịch mà (LopId, MonHocId) nằm trong tập phụ trách
            var lichHoc = lichHocRaw
                .Where(x => phuTrachSet.Contains((x.LopId, x.MonHocId)))
                .ToList();

            // Bài tập sắp hết hạn
            var baiTapSapHan = await _db.DanhSachBaiTap
                .Include(x => x.MonHoc)
                .Where(x => monHocIds.Contains(x.MonHocId) && x.HanNop > DateTime.Now && x.HanNop <= DateTime.Now.AddDays(7))
                .OrderBy(x => x.HanNop)
                .Take(10)
                .ToListAsync();

            ViewData["ActivePage"] = "LichDay";
            ViewData["Title"] = "Lịch dạy";
            ViewData["PageTitle"] = "Lịch dạy";
            ViewBag.DanhSachLop = danhSachLop;
            ViewBag.BaiTapSapHan = baiTapSapHan;
            ViewBag.TongTiet = lichHoc.Count;
            return View(lichHoc);
        }

        // ────────────────────────────────────────────────────────────────────
        // HELPERS
        // ────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Giới hạn danh sách năm học theo cấp lớp:
        /// Khối 10 → chỉ năm hiện tại, Khối 11 → 1 năm trước, Khối 12 → 2 năm trước.
        /// </summary>
        private static List<string> LimitNamHocByKhoi(List<string> dsNamHoc, string namHocHienTai, string tenKhoi)
        {
            int soNamDuocXem = GetMaxNamLuiByKhoi(tenKhoi);
            // Luôn bao gồm năm hiện tại + số năm lùi tương ứng
            return dsNamHoc
                .Where(n =>
                {
                    if (n == namHocHienTai) return true;
                    // Tính khoảng cách năm
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
            // TenKhoi = "Khối 10", "Khối 11", "Khối 12"
            var parts = tenKhoi.Trim().Split(' ');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int soKhoi))
                return Math.Max(0, soKhoi - 10); // 10→0, 11→1, 12→2
            return 0;
        }

        private static string GetPreviousAcademicYear(string currentNamHoc, int yearsBack)
        {
            var parts = currentNamHoc.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[0], out int startYear) && int.TryParse(parts[1], out int endYear))
            {
                return $"{startYear - yearsBack}-{endYear - yearsBack}";
            }
            return currentNamHoc;
        }

        private static string GetInitials(string hoTen)
        {
            if (string.IsNullOrWhiteSpace(hoTen)) return "?";
            var parts = hoTen.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.Length >= 2
                ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                : hoTen[..Math.Min(2, hoTen.Length)].ToUpper();
        }

        private static string FormatTimeAgo(DateTime? dt)
        {
            if (dt == null) return "";
            var diff = DateTime.Now - dt.Value;
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút trước";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ trước";
            if (diff.TotalDays < 2) return "Hôm qua";
            return dt.Value.ToString("dd/MM/yyyy");
        }

        // ────────────────────────────────────────────────────────────────────
        // LỊCH SỬ ĐIỂM SỐ (Giáo viên xem điểm các năm của học sinh)
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> LichSuDiem(int lopId = 0, string? hocSinhId = null, string? namHoc = null, int khoiId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account", new { area = "" });

            // Lấy tất cả môn GV dạy
            var monHocIds = await _db.MonHocGiaoViens
                .Where(x => x.NguoiDungId == gv.Id)
                .Select(x => x.MonHocId)
                .ToListAsync();

            // Các lớp GV dạy hoặc chủ nhiệm
            var lopMonHocs = await _db.LopMonHocs
                .Include(x => x.Lop).ThenInclude(l => l!.Khoi)
                .Include(x => x.MonHoc)
                .Where(x => monHocIds.Contains(x.MonHocId))
                .ToListAsync();

            // Thêm lớp chủ nhiệm vào list nếu chưa có
            var lopChuNhiem = await _db.Lops
                .Include(l => l.Khoi)
                .FirstOrDefaultAsync(l => l.GiaoVienChuNhiemId == gv.Id);

            // Danh sách khối
            var danhSachKhoi = lopMonHocs
                .Where(x => x.Lop?.Khoi != null)
                .Select(x => x.Lop!.Khoi)
                .DistinctBy(k => k!.Id)
                .OrderBy(k => k!.TenKhoi)
                .ToList();

            if (khoiId == 0 && danhSachKhoi.Any())
                khoiId = danhSachKhoi.First()!.Id;

            // Danh sách lớp theo khối
            var lopsOfKhoi = lopMonHocs
                .Where(x => x.Lop?.MaKhoi == khoiId)
                .Select(x => x.Lop!)
                .DistinctBy(l => l.Id)
                .OrderBy(l => l.TenLop)
                .ToList();

            // Thêm lớp chủ nhiệm nếu thuộc khối đang chọn và chưa có trong list
            if (lopChuNhiem != null && lopChuNhiem.MaKhoi == khoiId && !lopsOfKhoi.Any(l => l.Id == lopChuNhiem.Id))
                lopsOfKhoi.Add(lopChuNhiem);

            if (lopId == 0 && lopsOfKhoi.Any())
                lopId = lopsOfKhoi.First().Id;

            // Học sinh trong lớp đang chọn
            var hocSinhTrongLop = (await _db.NguoiDungs
                .Where(u => u.LopId == lopId)
                .ToListAsync())
                .OrderBy(u => u.HoTen != null && u.HoTen.Contains(' ')
                    ? u.HoTen.Substring(u.HoTen.LastIndexOf(' ') + 1) : u.HoTen)
                .ThenBy(u => u.HoTen)
                .ToList();

            if (string.IsNullOrEmpty(hocSinhId) && hocSinhTrongLop.Any())
                hocSinhId = hocSinhTrongLop.First().Id;

            // Tất cả năm học có điểm của học sinh này
            var dsNamHocAll = !string.IsNullOrEmpty(hocSinhId)
                ? await _db.DiemHocKys
                    .Where(x => x.HocSinhId == hocSinhId)
                    .Select(x => x.NamHoc)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToListAsync()
                : new List<string>();

            // Tính năm học hiện tại và lấy tenKhoi của lớp đang chọn để giới hạn năm được xem
            var hsObj = !string.IsNullOrEmpty(hocSinhId) ? await _db.Users.FindAsync(hocSinhId) : null;
            var namHocHienTaiLS = hsObj?.NamHoc ?? (DateTime.Now.Month >= 9
                ? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}"
                : $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}");
            var tenKhoiLS = danhSachKhoi.FirstOrDefault(k => k!.Id == khoiId)?.TenKhoi
                          ?? lopsOfKhoi.FirstOrDefault(l => l.Id == lopId)?.Khoi?.TenKhoi
                          ?? "Khối 10";
            var dsNamHoc = LimitNamHocByKhoi(dsNamHocAll, namHocHienTaiLS, tenKhoiLS);

            // Nếu namHoc được truyền vào nhưng không nằm trong danh sách được phép → reset
            if (!string.IsNullOrEmpty(namHoc) && !dsNamHoc.Contains(namHoc))
                namHoc = null;

            if (string.IsNullOrEmpty(namHoc) && dsNamHoc.Any())
                namHoc = dsNamHoc.First();

            // Điểm học kỳ theo năm học được chọn
            var diemHocKy = new List<DiemHocKy>();
            NguoiDung? hocSinhHienTai = null;

            if (!string.IsNullOrEmpty(hocSinhId) && !string.IsNullOrEmpty(namHoc))
            {
                hocSinhHienTai = await _db.NguoiDungs
                    .Include(u => u.Lop)
                    .FirstOrDefaultAsync(u => u.Id == hocSinhId);

                diemHocKy = await _db.DiemHocKys
                    .Include(x => x.MonHoc)
                    .Where(x => x.HocSinhId == hocSinhId && x.NamHoc == namHoc)
                    .OrderBy(x => x.HocKy)
                    .ThenBy(x => x.MonHoc!.TenMonHoc)
                    .ToListAsync();
            }

            // Tính tổng kết cả năm theo môn
            var monIds = diemHocKy.Select(x => x.MonHocId).Distinct().ToList();
            var tbCaNam = monIds.Select(mid =>
            {
                var d1 = diemHocKy.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 1);
                var d2 = diemHocKy.FirstOrDefault(x => x.MonHocId == mid && x.HocKy == 2);
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

            ViewBag.DanhSachKhoi = danhSachKhoi;
            ViewBag.LopsOfKhoi = lopsOfKhoi;
            ViewBag.HocSinhTrongLop = hocSinhTrongLop;
            ViewBag.KhoiHienTai = khoiId;
            ViewBag.LopHienTai = lopId;
            ViewBag.HocSinhHienTai = hocSinhHienTai;
            ViewBag.HocSinhId = hocSinhId;
            ViewBag.DsNamHoc = dsNamHoc;
            ViewBag.SelectedNamHoc = namHoc;
            ViewBag.HK1 = diemHocKy.Where(x => x.HocKy == 1).ToList();
            ViewBag.HK2 = diemHocKy.Where(x => x.HocKy == 2).ToList();
            ViewBag.TbCaNam = tbCaNam;
            ViewData["ActivePage"] = "LichSuDiem";
            return View(diemHocKy);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult LogJSError([FromBody] System.Text.Json.JsonElement data)
        {
            try
            {
                var msg = data.GetProperty("message").GetString();
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "js_error.log");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now}: JS Error: {msg}{Environment.NewLine}");
            }
            catch {}
            return Json(new { success = true });
        }
    }
}
