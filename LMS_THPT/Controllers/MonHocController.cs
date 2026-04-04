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
            var gvDaCo = monHoc.MonHocGiaoViens
                .Select(m => m.NguoiDungId)
                .ToList();

            var gvList = await _context.Users
                .Where(u => u.ChucVu == "Giáo viên" && !gvDaCo.Contains(u.Id))
                .ToListAsync();

            // Lấy danh sách lớp
            var lopDaPhan = monHoc.MonHocGiaoViens
                .Where(mg => mg.LopId.HasValue)
                .Select(mg => mg.LopId.Value)
                .ToList();

            // Lọc lớp theo khối và chưa được phân
            var lopList = await _context.Lops
                .Where(l => l.MaKhoi == monHoc.KhoiId && !lopDaPhan.Contains(l.Id))
                .ToListAsync();

            ViewBag.GiaoViens = new SelectList(gvList, "Id", "HoTen");
            ViewBag.Lops = new SelectList(lopList, "Id", "TenLop");

            // Không còn cung cấp danh sách môn tại đây (chuyển phần chọn môn sang trang tạo giáo viên)

            return View(monHoc);
        }

        // POST: /MonHoc/AddGiaoVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGiaoVien(int monHocId, string nguoiDungId, int? lopId, int[]? monHocIds)
        {
            if (string.IsNullOrEmpty(nguoiDungId))
                return RedirectToAction("Details", new { id = monHocId });
            // Nếu user chọn nhiều môn (monHocIds), thêm vào tất cả những môn đó
            var targets = (monHocIds != null && monHocIds.Length > 0) ? monHocIds.Distinct().ToArray() : new[] { monHocId };

            var addedCount = 0;
            foreach (var targetId in targets)
            {
                var exists = await _context.MonHocGiaoViens
                    .AnyAsync(mg => mg.MonHocId == targetId && mg.NguoiDungId == nguoiDungId && mg.LopId == lopId);

                if (exists) continue;

                var monHocGv = new MonHocGiaoVien
                {
                    MonHocId = targetId,
                    NguoiDungId = nguoiDungId,
                    LopId = lopId
                };

                _context.MonHocGiaoViens.Add(monHocGv);
                addedCount++;
            }

            if (addedCount > 0)
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = addedCount == 1 ? "Thêm thành công!" : $"Thêm thành công cho {addedCount} môn.";
            }
            else
            {
                TempData["Error"] = "Giáo viên đã tồn tại trong môn đã chọn.";
            }

            return RedirectToAction("Details", new { id = monHocId });
        }

        // POST: /MonHoc/RemoveGiaoVien
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveGiaoVien(int monHocId, string nguoiDungId, int? lopId)
        {
            if (string.IsNullOrEmpty(nguoiDungId))
                return RedirectToAction("Details", new { id = monHocId });

            var assignment = await _context.MonHocGiaoViens
                .FirstOrDefaultAsync(mg => mg.MonHocId == monHocId
                                        && mg.NguoiDungId == nguoiDungId
                                        && (lopId == null || mg.LopId == lopId));

            if (assignment == null)
            {
                TempData["Error"] = "Không tìm thấy phân công giáo viên.";
                return RedirectToAction("Details", new { id = monHocId });
            }

            _context.MonHocGiaoViens.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa phân công giáo viên thành công!";
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
    }
}