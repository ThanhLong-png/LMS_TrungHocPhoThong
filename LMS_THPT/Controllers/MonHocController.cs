using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS_THPT.Controllers
{
    public class MonHocController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MonHocController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: /MonHoc/Index
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả các khối
            var khois = await _context.Khois
                .Include(k => k.MonHocs.Where(m => m.IsActive)) // Lấy môn học của từng khối
                    .ThenInclude(m => m.MonHocGiaoViens)
                        .ThenInclude(mg => mg.GiaoVien)
                .ToListAsync();

            return View(khois);
        }
        // GET: /MonHoc/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var monHoc = await _context.DanhSachMonHoc
                .Include(m => m.MonHocGiaoViens)
                    .ThenInclude(mg => mg.GiaoVien)
                .Include(m => m.MonHocGiaoViens)
                    .ThenInclude(mg => mg.Lop)
                .Include(m => m.Khoi) // thêm để lấy khối của môn
                .FirstOrDefaultAsync(m => m.Id == id);

            if (monHoc == null) return NotFound();

            // Lấy giáo viên chưa có trong môn
            var gvList = await _context.Users
     .Where(u => u.ChucVu == "Giáo viên")
     .ToListAsync();

            // Lấy danh sách lớp
            // ❗ Lấy các lớp đã có giáo viên cho môn này
            var lopDaCo = await _context.MonHocGiaoViens
                .Where(mg => mg.MonHocId == monHoc.Id && mg.LopId != null)
                .Select(mg => mg.LopId.Value)
                .ToListAsync();

            // ❗ Chỉ lấy lớp CHƯA có giáo viên
            var lopList = await _context.Lops
                .Where(l => l.MaKhoi == monHoc.KhoiId && !lopDaCo.Contains(l.Id))
                .ToListAsync();
            ViewBag.GiaoViens = new SelectList(gvList, "Id", "HoTen");
            ViewBag.Lops = new SelectList(lopList, "Id", "TenLop");

            // Không còn cung cấp danh sách môn tại đây (chuyển phần chọn môn sang trang tạo giáo viên)

            return View(monHoc);
        }

        // POST: /MonHoc/AddGiaoVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGiaoVien(int monHocId, string nguoiDungId, int[]? lopIds)
        {
            if (string.IsNullOrEmpty(nguoiDungId))
                return RedirectToAction("Details", new { id = monHocId });

            if (lopIds == null || lopIds.Length == 0)
                lopIds = new int?[] { null }.Select(x => x ?? 0).ToArray();

            int added = 0;

            foreach (var lopId in lopIds)
            {
                // ❗ Check: lớp này đã có giáo viên cho môn này chưa
                var daCoGiaoVien = await _context.MonHocGiaoViens
                    .AnyAsync(mg => mg.MonHocId == monHocId
                        && (mg.LopId ?? 0) == (lopId == 0 ? 0 : lopId));

                if (daCoGiaoVien)
                {
                    TempData["Warning"] = "Một lớp chỉ được 1 giáo viên cho môn này!";
                    continue;
                }

                var lopThuc = lopId == 0 ? (int?)null : lopId;

                _context.MonHocGiaoViens.Add(new MonHocGiaoVien
                {
                    MonHocId = monHocId,
                    NguoiDungId = nguoiDungId,
                    LopId = lopThuc
                });

                // ❗ THÊM DÒNG NÀY NGAY TẠI ĐÂY
                await CapNhatLichHoc(monHocId, lopThuc, nguoiDungId);

                added++;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm {added} lớp cho giáo viên!";
            return RedirectToAction("Details", new { id = monHocId });
        }

        // POST: /MonHoc/RemoveGiaoVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGiaoVien(int monHocId, string nguoiDungId, int? lopId)
        {
            var assignments = await _context.MonHocGiaoViens
                .Where(mg => mg.MonHocId == monHocId && mg.NguoiDungId == nguoiDungId)
                .ToListAsync();

            if (!assignments.Any())
            {
                TempData["Error"] = "Không tìm thấy phân công.";
                return RedirectToAction("Details", new { id = monHocId });
            }

            // ❗ GIỮ LỊCH - chỉ set null
            foreach (var a in assignments)
            {
                var lichHocs = await _context.LichHocs
                    .Where(l => l.MonHocId == monHocId && l.LopId == a.LopId)
                    .ToListAsync();

                foreach (var lich in lichHocs)
                {
                    lich.GiaoVienId = null; // ✅ giữ lịch
                }
            }

            _context.MonHocGiaoViens.RemoveRange(assignments);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã xóa phân công, lịch học vẫn được giữ!";
            return RedirectToAction("Details", new { id = monHocId });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int KhoiId, string TenMonHoc)
        {
            if (string.IsNullOrWhiteSpace(TenMonHoc))
            {
                TempData["Error"] = "Tên môn học không được để trống!";
                return RedirectToAction("Index");
            }

            var monHoc = new MonHoc
            {
                TenMonHoc = TenMonHoc,
                KhoiId = KhoiId,
                IsActive = true
            };

            _context.DanhSachMonHoc.Add(monHoc);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm môn học thành công!";
            return RedirectToAction("Index");
        }
        private async Task CapNhatLichHoc(int monHocId, int? lopId, string giaoVienId)
        {
            var lichHocs = await _context.LichHocs
                .Where(l => l.MonHocId == monHocId && l.LopId == lopId)
                .ToListAsync();

            foreach (var lich in lichHocs)
            {
                lich.GiaoVienId = giaoVienId; // ✅ update lại
            }
        }
    }
}