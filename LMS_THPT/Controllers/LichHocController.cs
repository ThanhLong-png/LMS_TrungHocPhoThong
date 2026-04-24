using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace LMS_THPT.Controllers
{
    public class LichHocController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LichHocController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // 📅 TRANG CHÍNH: THỜI KHÓA BIỂU THEO LỚP
        // =====================================================
        public IActionResult Index()
        {
            return RedirectToAction("ThoiKhoaBieu");
        }

        // =====================================================
        // 📅 XEM THỜI KHÓA BIỂU THEO LỚP
        // =====================================================
        public async Task<IActionResult> ThoiKhoaBieu(int? lopId, DateTime? date)
        {
            ViewBag.Lops = await _context.Lops.ToListAsync();

            DateTime selectedDate = date ?? DateTime.Now.Date;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");
            ViewBag.SelectedLopId = lopId;
            
            // Tính Thứ
            int thu = (int)selectedDate.DayOfWeek + 1;
            if (thu == 1) thu = 8; // Chủ nhật

            var query = _context.LichHocs
                .Include(x => x.Lop)
                .Include(x => x.MonHoc)
                .Include(x => x.GiaoVien)
                .AsQueryable();

            if (lopId.HasValue)
            {
                query = query.Where(x => x.LopId == lopId);
            }

            var data = await query
                .Where(x => x.Thu == thu)
                .OrderBy(x => x.TietHoc)
                .ToListAsync();

            var leaves = await _context.YeuCauGiaoVien
                .Where(y => y.TrangThai == TrangThaiYeuCau.DaDuyet && 
                            y.LoaiYeuCau == LoaiYeuCau.NghiPhep && 
                            y.NgayNghi != null && y.NgayNghi.Value.Date == selectedDate.Date)
                .ToListAsync();
            ViewBag.Leaves = leaves;

            return View(data);
        }

        // =====================================================
        // ➕ CREATE VIEW
        // =====================================================
        public IActionResult Create(int? thu, int? tietHoc, int? lopId)
        {
            ViewBag.Lops = _context.Lops.ToList();

            var model = new LichHoc
            {
                Thu = thu ?? 0,
                TietHoc = tietHoc ?? 0,
                LopId = lopId ?? 0   // 👈 AUTO SELECT LỚP
            };

            return View(model);
        }

        // =====================================================
        // ➕ CREATE POST
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LichHoc model, string? date)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Lops = _context.Lops.ToList();
                return View(model);
            }

            if (IsConflict(model))
            {
                ModelState.AddModelError("", "❌ Trùng lịch lớp / giáo viên / phòng!");
                ViewBag.Lops = _context.Lops.ToList();
                return View(model);
            }

            _context.LichHocs.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("ThoiKhoaBieu", new { lopId = model.LopId, date = date });
        }

        // =====================================================
        // ✏ EDIT VIEW
        // =====================================================
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _context.LichHocs
                .Include(x => x.Lop)
                .Include(x => x.MonHoc)
                .Include(x => x.GiaoVien)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (data == null) return NotFound();

            ViewBag.Lops = _context.Lops.ToList();

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(LichHoc model, string? date)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Lops = _context.Lops.ToList();
                return View(model);
            }

            var data = await _context.LichHocs.FindAsync(model.Id);
            if (data == null) return NotFound();

            data.LopId = model.LopId;
            data.MonHocId = model.MonHocId;
            data.GiaoVienId = model.GiaoVienId;

            data.Thu = model.Thu;
            data.TietHoc = model.TietHoc;
            data.PhongHoc = model.PhongHoc;

            await _context.SaveChangesAsync();

            return RedirectToAction("ThoiKhoaBieu", new { lopId = model.LopId, date = date });
        }
        // =====================================================
        // ❌ DELETE
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? date)
        {
            var data = await _context.LichHocs.FindAsync(id);

            if (data != null)
            {
                int lopId = data.LopId; // 👈 lấy trước khi xóa

                _context.LichHocs.Remove(data);
                await _context.SaveChangesAsync();

                return RedirectToAction("ThoiKhoaBieu", new { lopId = lopId, date = date });
            }

            return RedirectToAction("ThoiKhoaBieu", new { date = date });
        }

        // =====================================================
        // ⚡ AJAX: LẤY MÔN THEO LỚP
        // =====================================================
        [HttpGet]
        public JsonResult GetMonByLop(int lopId)
        {
            var data = _context.MonHocGiaoViens
                .Where(x => x.LopId == lopId)
                .Include(x => x.MonHoc)
                .Select(x => new
                {
                    monHocId = x.MonHocId,
                    tenMonHoc = x.MonHoc.TenMonHoc
                })
                .GroupBy(x => new { x.monHocId, x.tenMonHoc })
                .Select(g => g.First())
                .ToList();

            return Json(data);
        }

        // =====================================================
        // ⚡ AJAX: LẤY GIÁO VIÊN THEO MÔN + LỚP
        // =====================================================
        [HttpGet]
        public JsonResult GetGiaoVienByMonHoc(int lopId, int monHocId)
        {
            var data = _context.MonHocGiaoViens
                .Where(x => x.LopId == lopId && x.MonHocId == monHocId)
                .Include(x => x.GiaoVien)
                .Select(x => new
                {
                    giaoVienId = x.NguoiDungId,
                    hoTen = x.GiaoVien.HoTen
                })
                .GroupBy(x => new { x.giaoVienId, x.hoTen })
                .Select(g => g.First())
                .ToList();

            return Json(data);
        }

        // =====================================================
        // 🚫 CHECK TRÙNG LỊCH
        // =====================================================
        private bool IsConflict(LichHoc lh)
        {
            return _context.LichHocs.Any(x =>
                x.Id != lh.Id &&
                x.Thu == lh.Thu &&
                x.TietHoc == lh.TietHoc &&
                (
                    x.LopId == lh.LopId ||
                    x.GiaoVienId == lh.GiaoVienId ||
                    x.PhongHoc == lh.PhongHoc
                )
            );
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportExcel(IFormFile file, int lopId)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn file Excel!";
                return RedirectToAction("ThoiKhoaBieu", new { lopId });
            }

            int success = 0;
            int fail = 0;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    var sheet = package.Workbook.Worksheets[0];
                    int rowCount = sheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // bỏ header
                    {
                        try
                        {
                            string tenMon = sheet.Cells[row, 1].Text.Trim();
                            int thu = int.Parse(sheet.Cells[row, 2].Text);
                            int tiet = int.Parse(sheet.Cells[row, 3].Text);
                            string phong = sheet.Cells[row, 4].Text.Trim();

                            if (string.IsNullOrEmpty(tenMon) || string.IsNullOrEmpty(phong))
                            {
                                fail++;
                                continue;
                            }

                            // Tìm môn
                            var monHoc = await _context.DanhSachMonHoc
                                .FirstOrDefaultAsync(m => m.TenMonHoc == tenMon);

                            if (monHoc == null)
                            {
                                fail++;
                                continue;
                            }

                            // Tìm giáo viên theo lớp + môn
                            var gv = await _context.MonHocGiaoViens
                                .Where(x => x.LopId == lopId && x.MonHocId == monHoc.Id)
                                .Select(x => x.NguoiDungId)
                                .FirstOrDefaultAsync();

                            if (gv == null)
                            {
                                fail++;
                                continue;
                            }

                            var lich = new LichHoc
                            {
                                LopId = lopId,
                                MonHocId = monHoc.Id,
                                GiaoVienId = gv,
                                Thu = thu,
                                TietHoc = tiet,
                                PhongHoc = phong   // ✅ FIX QUAN TRỌNG
                            };

                            _context.LichHocs.Add(lich);
                            success++;
                        }
                        catch
                        {
                            fail++;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Import thành công: {success} dòng";
            TempData["Error"] = fail > 0 ? $"Lỗi: {fail} dòng" : null;

            return RedirectToAction("ThoiKhoaBieu", new { lopId });
        }
    }
}