// Controllers/GiaoVienController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_THPT.Data;
using LMS_THPT.Models;
using LMS_THPT.ViewModels;

namespace LMS_THPT.Controllers
{
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
            if (gv == null) return RedirectToAction("Login", "Account");

            var monHocIds = await _db.MonHocGiaoViens
                .Where(x => x.NguoiDungId == gv.Id)
                .Select(x => x.MonHocId)
                .ToListAsync();

            var lopIds = await _db.LopMonHocs
                .Where(x => monHocIds.Contains(x.MonHocId))
                .Select(x => x.LopId)
                .Distinct()
                .ToListAsync();

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
                .Where(x => monHocIds.Contains(x.MonHocId))
                .Take(4)
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
            var lichHomNay = await _db.LichHocs
                .Include(x => x.MonHoc)
                .Where(x => monHocIds.Contains(x.MonHocId) && x.NgayHoc.Date == today)
                .OrderBy(x => x.GioBatDau)
                .Take(3)
                .Select(x => new LichHocItem
                {
                    ThoiGian = x.GioBatDau.ToString(@"hh\:mm") + " – " + x.GioKetThuc.ToString(@"hh\:mm"),
                    TenMon = x.MonHoc!.TenMonHoc ?? "",
                    PhongLop = x.PhongHoc ?? "",
                    NhanManh = false
                })
                .ToListAsync();

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
                LichHomNay = lichHomNay
            };

            return View(vm);
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.1  QUẢN LÝ MÔN HỌC
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> QuanLyMonHoc(int monHocId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account");

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

            var danhSachDiem = await _db.DiemSos
                .Include(x => x.NguoiDung)
                .Where(x => x.MonHocId == monHocId)
                .Select(x => new NhapDiemItem
                {
                    HocSinhId = x.NguoiDungId,
                    TenHocSinh = x.NguoiDung.HoTen ?? x.NguoiDung.UserName ?? "",
                    DiemGiuaKy = x.DiemGiuaKy,
                    DiemCuoiKy = x.DiemCuoiKy,
                    DiemSoId = x.Id
                })
                .ToListAsync();

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
            // 1. Lấy thông tin giáo viên đang đăng nhập
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return Json(new { success = false, message = "Không xác thực giáo viên" });

            var diem = await _db.DiemSos
                .FirstOrDefaultAsync(x => x.NguoiDungId == model.HocSinhId && x.MonHocId == model.MonHocId);

            if (diem == null)
            {
                diem = new DiemSo
                {
                    NguoiDungId = model.HocSinhId,
                    MonHocId = model.MonHocId,
                    DiemGiuaKy = model.DiemGiuaKy,
                    DiemCuoiKy = model.DiemCuoiKy,
                    NgayNhap = DateTime.Now,
                    // QUAN TRỌNG: Phải gán Id của giáo viên đang thực hiện thao tác này
                    GiaoVienId = gv.Id
                };
                _db.DiemSos.Add(diem);
            }
            else
            {
                diem.DiemGiuaKy = model.DiemGiuaKy;
                diem.DiemCuoiKy = model.DiemCuoiKy;
                diem.NgayCapNhat = DateTime.Now;
                // Cập nhật lại người chấm cuối cùng nếu cần
                diem.GiaoVienId = gv.Id;
            }

            await _db.SaveChangesAsync(); // Dòng 356 gây lỗi sẽ hết nếu GiaoVienId đúng
            return Json(new { success = true, message = "Đã lưu điểm thành công!" });
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.3  BÀI TẬP & ĐÁNH GIÁ
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> QuanLyBaiTap(int monHocId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account");

            var monHocIds = await _db.MonHocGiaoViens
                .Where(x => x.NguoiDungId == gv.Id)
                .Select(x => x.MonHocId)
                .ToListAsync();

            if (monHocId == 0 && monHocIds.Any()) monHocId = monHocIds.First();

            var baiNops = await _db.BaiNops
                .Include(x => x.HocSinh)
                .Include(x => x.BaiTap)
                .Where(x => x.BaiTap.MonHocId == monHocId)
                .OrderBy(x => x.Diem)
                .ThenByDescending(x => x.NgayNop)
                .Take(8)
                .Select(x => new BaiNopChopItem
                {
                    BaiNopId = x.Id,
                    TenHocSinh = x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? "",
                    TenVietTat = GetInitials(x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? ""),
                    TenBaiTap = x.BaiTap.TieuDe ?? "",
                    ThoiGianNop = FormatTimeAgo(x.NgayNop),
                    TrangThai = x.Diem == null ? "pending" : "graded",
                    Diem = x.Diem
                })
                .ToListAsync();

            var diemBaiTap = await _db.BaiNops
                .Include(x => x.HocSinh)
                .Include(x => x.BaiTap)
                .Where(x => x.BaiTap.MonHocId == monHocId)
                .Select(x => new BaiNopDiemItem
                {
                    BaiNopId = x.Id,
                    TenHocSinh = x.HocSinh!.HoTen ?? x.HocSinh!.UserName ?? "",
                    TenBaiTap = x.BaiTap.TieuDe ?? "",
                    Diem = x.Diem,
                    NhanXet = x.NhanXet ?? "",
                    TrangThai = x.Diem == null ? "pending" : "graded"
                })
                .ToListAsync();

            var vm = new BaiTapManageViewModel
            {
                BaiNopChoChoam = baiNops,
                DanhSachDiem = diemBaiTap
            };

            ViewBag.MonHocIds = monHocIds;
            ViewBag.MonHocHienTai = monHocId;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoBaiTap(BaiTap model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var gv = await _userManager.GetUserAsync(User);
            model.NguoiDungId = gv!.Id;
            model.NgayTao = DateTime.Now;

            _db.BaiTaps.Add(model);
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Tạo bài tập thành công!", id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> LuuDiemBaiTap([FromBody] LuuDiemBaiTapRequest model)
        {
            var baiNop = await _db.BaiNops.FindAsync(model.BaiNopId);
            if (baiNop == null) return Json(new { success = false, message = "Không tìm thấy bài nộp." });

            baiNop.Diem = model.Diem;
            baiNop.NhanXet = model.NhanXet;
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Đã lưu điểm!" });
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.4  THEO DÕI TIẾN ĐỘ
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> TienDo(int monHocId = 0)
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account");

            var monHocIds = await _db.MonHocGiaoViens
                .Where(x => x.NguoiDungId == gv.Id)
                .Select(x => x.MonHocId)
                .ToListAsync();

            if (monHocId == 0 && monHocIds.Any()) monHocId = monHocIds.First();

            var lopIds = await _db.LopMonHocs
                .Where(x => x.MonHocId == monHocId)
                .Select(x => x.LopId)
                .ToListAsync();

            var hocSinhIds = await _db.NguoiDungs
                .Where(u => u.LopId.HasValue && lopIds.Contains(u.LopId.Value))
                .Select(u => u.Id)
                .Distinct()
                .ToListAsync();

            var diemList = await _db.DiemSos
                .Include(x => x.NguoiDung)
                .Where(x => x.MonHocId == monHocId && hocSinhIds.Contains(x.NguoiDungId))
                .ToListAsync();

            var tongBaiTap = await _db.BaiTaps.CountAsync(x => x.MonHocId == monHocId);

            var tienDoHs = new List<TienDoHocSinhItem>();
            foreach (var hsId in hocSinhIds)
            {
                var nguoiDung = await _userManager.FindByIdAsync(hsId);
                var diem = diemList.FirstOrDefault(d => d.NguoiDungId == hsId);
                var soBaiNop = await _db.BaiNops.CountAsync(x => x.HocSinhId == hsId && x.BaiTap.MonHocId == monHocId);
                var pct = tongBaiTap == 0 ? 0 : Math.Min(100, soBaiNop * 100 / tongBaiTap);
                var tongKet = diem != null && diem.DiemGiuaKy.HasValue && diem.DiemCuoiKy.HasValue
                    ? Math.Round(diem.DiemGiuaKy.Value * 0.4 + diem.DiemCuoiKy.Value * 0.6, 1) : 0;

                tienDoHs.Add(new TienDoHocSinhItem
                {
                    TenHocSinh = nguoiDung?.HoTen ?? nguoiDung?.UserName ?? hsId,
                    TenVietTat = GetInitials(nguoiDung?.HoTen ?? nguoiDung?.UserName ?? ""),
                    Diem = tongKet,
                    PhanTram = pct
                });
            }

            var xepLoai = new List<ThongKeXepLoaiItem>
            {
                new() { NhanXepLoai = "Xuất sắc",  Mau = "#1D4ED8" },
                new() { NhanXepLoai = "Giỏi",       Mau = "#166534" },
                new() { NhanXepLoai = "Khá",         Mau = "#92400E" },
                new() { NhanXepLoai = "Trung bình",  Mau = "#6B7280" },
                new() { NhanXepLoai = "Yếu/Kém",     Mau = "#991B1B" },
            };
            foreach (var hs in tienDoHs)
            {
                var xl = hs.Diem switch
                {
                    >= 9.0 => "Xuất sắc",
                    >= 8.0 => "Giỏi",
                    >= 6.5 => "Khá",
                    >= 5.0 => "Trung bình",
                    _ => "Yếu/Kém"
                };
                var item = xepLoai.First(x => x.NhanXepLoai == xl);
                item.SoLuong++;
            }
            int tongHs = tienDoHs.Count;
            foreach (var x in xepLoai)
                x.PhanTram = tongHs == 0 ? 0 : x.SoLuong * 100 / tongHs;

            var diemTB = tienDoHs.Any() ? Math.Round(tienDoHs.Average(x => x.Diem), 1) : 0;

            var lopList = await _db.LopMonHocs
                .Include(x => x.Lop)
                .Where(x => monHocIds.Contains(x.MonHocId))
                .Select(x => x.Lop.TenLop ?? "")
                .Distinct()
                .ToListAsync();

            var vm = new TienDoViewModel
            {
                DiemTrungBinhLop = diemTB,
                TiLeHoanThanh = tienDoHs.Any() ? tienDoHs.Average(x => x.PhanTram) is var avg ? (int)avg : 0 : 0,
                SoHocSinhXuatSac = tienDoHs.Count(x => x.Diem >= 9.0),
                SoHocSinhCanHoTro = tienDoHs.Count(x => x.Diem < 5.0),
                TienDoHocSinh = tienDoHs.OrderByDescending(x => x.Diem).ToList(),
                ThongKeXepLoai = xepLoai,
                DanhSachLop = lopList
            };

            ViewBag.MonHocIds = monHocIds;
            ViewBag.MonHocHienTai = monHocId;
            return View(vm);
        }

        // ────────────────────────────────────────────────────────────────────
        // 3.5  LỚP CHỦ NHIỆM
        // ────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> LopChuNhiem()
        {
            var gv = await _userManager.GetUserAsync(User);
            if (gv == null) return RedirectToAction("Login", "Account");

            // Lấy lớp mà giáo viên này làm chủ nhiệm
            var lop = await _db.Lops
                .FirstOrDefaultAsync(l => l.GiaoVienChuNhiemId == gv.Id);

            var hocSinhList = new List<HocSinhLopItem>();

            if (lop != null)
            {
                hocSinhList = await _db.NguoiDungs
                    .Where(u => u.LopId == lop.Id)
                    .OrderBy(u => u.HoTen)
                    .Select(u => new HocSinhLopItem
                    {
                        Id = u.Id,
                        HoTen = u.HoTen ?? u.UserName ?? "",
                        TenVietTat = GetInitials(u.HoTen ?? u.UserName ?? ""),
                        GioiTinh = u.GioiTinh ?? "",
                        NgaySinh = u.NgaySinh.HasValue ? u.NgaySinh.Value.ToString("dd/MM/yyyy") : null,
                        DiaChi = u.DiaChi,
                        AnhDaiDien = u.AnhDaiDien,
                        Email = u.Email ?? ""
                    })
                    .ToListAsync();
            }

            // Lịch học của lớp hôm nay
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
                    .Where(x => monHocIdsLop.Contains(x.MonHocId) && x.NgayHoc.Date == today)
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

            // Yêu cầu/thông báo gần đây liên quan đến lớp
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
                YeuCauGanDay = yeuCauGanDay
            };

            ViewData["ActivePage"] = "LopChuNhiem";
            ViewData["Title"] = lop != null ? $"Lớp {lop.TenLop}" : "Lớp chủ nhiệm";
            return View(vm);
        }

        // ────────────────────────────────────────────────────────────────────
        // HELPERS
        // ────────────────────────────────────────────────────────────────────
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
    }
}