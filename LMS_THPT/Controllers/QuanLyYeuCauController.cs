// Controllers/QuanLyYeuCauController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS_THPT.Data;
using LMS_THPT.Models;

[Authorize(Roles = "Admin,HieuTruong")]
public class QuanLyYeuCauController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<NguoiDung> _userManager;

    public QuanLyYeuCauController(ApplicationDbContext context, UserManager<NguoiDung> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Danh sách tất cả yêu cầu
    public async Task<IActionResult> Index(int? trangThai, string? loai)
    {
        var query = _context.YeuCauGiaoVien
            .Include(y => y.GiaoVien)
            .AsQueryable();

        if (trangThai.HasValue)
        {
            if (trangThai.Value == 0)
            {
                query = query.Where(y => y.TrangThai == TrangThaiYeuCau.ChoDuyet || y.TrangThai == TrangThaiYeuCau.ChoXuLy);
            }
            else
            {
                query = query.Where(y => (int)y.TrangThai == trangThai.Value);
            }
            ViewBag.Filter = trangThai.Value.ToString();
        }

        if (!string.IsNullOrEmpty(loai) && Enum.TryParse<LoaiYeuCau>(loai, out var loaiEnum))
        {
            query = query.Where(y => y.LoaiYeuCau == loaiEnum);
            ViewBag.LoaiFilter = loai;
        }

        var danhSach = await query
            .OrderByDescending(y => y.NgayGui)
            .ToListAsync();

        // Thống kê cho dashboard
        ViewBag.TongSo = await _context.YeuCauGiaoVien.CountAsync();
        ViewBag.ChoDuyet = await _context.YeuCauGiaoVien.CountAsync(y => y.TrangThai == TrangThaiYeuCau.ChoDuyet || y.TrangThai == TrangThaiYeuCau.ChoXuLy);
        ViewBag.DaDuyet = await _context.YeuCauGiaoVien.CountAsync(y => y.TrangThai == TrangThaiYeuCau.DaDuyet);
        ViewBag.TuChoi = await _context.YeuCauGiaoVien.CountAsync(y => y.TrangThai == TrangThaiYeuCau.TuChoi);

        return View(danhSach);
    }

    // GET: Trang xử lý yêu cầu
    public async Task<IActionResult> XuLy(int id)
    {
        var yeuCau = await _context.YeuCauGiaoVien
            .Include(y => y.GiaoVien)
            .Include(y => y.Lop)
            .Include(y => y.MonHoc)
            .Include(y => y.NguoiXuLy)
            .FirstOrDefaultAsync(y => y.Id == id);

        if (yeuCau == null)
            return NotFound();

        return View(yeuCau);
    }

    // POST: Duyệt yêu cầu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DuyetYeuCau(int id, string? ghiChu)
    {
        var user = await _userManager.GetUserAsync(User);

        var yeuCau = await _context.YeuCauGiaoVien
            .FirstOrDefaultAsync(y => y.Id == id);

        if (yeuCau == null)
            return NotFound();

        if (yeuCau.TrangThai != TrangThaiYeuCau.ChoDuyet && yeuCau.TrangThai != TrangThaiYeuCau.ChoXuLy)
        {
            TempData["Error"] = "Yêu cầu này đã được xử lý trước đó.";
            return RedirectToAction(nameof(XuLy), new { id });
        }

        yeuCau.TrangThai = TrangThaiYeuCau.DaDuyet;
        yeuCau.GhiChuAdmin = ghiChu;
        yeuCau.NgayXuLy = DateTime.Now;
        yeuCau.NguoiXuLyId = user.Id;

        // Auto insert make-up class
        if (yeuCau.LoaiYeuCau == LoaiYeuCau.HocBu && yeuCau.LopId.HasValue && yeuCau.NgayNghi.HasValue && yeuCau.MonHocId.HasValue)
        {
            int thu = (int)yeuCau.NgayNghi.Value.DayOfWeek + 1;
            if (thu == 1) thu = 8;
            
            // Lấy danh sách tiết từ chuỗi hoặc từ TuTiet (fallback)
            var periods = new List<int>();
            if (!string.IsNullOrEmpty(yeuCau.DanhSachTiet))
            {
                periods = yeuCau.DanhSachTiet.Split(',')
                    .Select(s => int.TryParse(s, out int p) ? p : 0)
                    .Where(p => p > 0)
                    .ToList();
            }
            else if (yeuCau.TuTiet.HasValue)
            {
                periods.Add(yeuCau.TuTiet.Value);
            }

            foreach (var period in periods)
            {
                var lichHocBu = new LichHoc
                {
                    LopId = yeuCau.LopId.Value,
                    MonHocId = yeuCau.MonHocId.Value,
                    GiaoVienId = yeuCau.GiaoVienId,
                    Thu = thu,
                    TietHoc = period,
                    IsHocBu = true,
                    NgayHoc = yeuCau.NgayNghi.Value.Date, // Đảm bảo chỉ lấy phần ngày
                    PhongHoc = "Phòng học bù"
                };
                _context.LichHocs.Add(lichHocBu);
            }
            yeuCau.GhiChu = (yeuCau.GhiChu ?? "") + " [Hệ thống: Đã tự động tạo " + periods.Count + " tiết học bù]";
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã duyệt yêu cầu và tạo lịch học bù thành công.";
        return RedirectToAction(nameof(Index));
    }

    // POST: Từ chối yêu cầu
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TuChoiYeuCau(int id, string? ghiChu)
    {
        var user = await _userManager.GetUserAsync(User);

        var yeuCau = await _context.YeuCauGiaoVien
            .FirstOrDefaultAsync(y => y.Id == id);

        if (yeuCau == null)
            return NotFound();

        if (yeuCau.TrangThai != TrangThaiYeuCau.ChoDuyet && yeuCau.TrangThai != TrangThaiYeuCau.ChoXuLy)
        {
            TempData["Error"] = "Yêu cầu này đã được xử lý trước đó.";
            return RedirectToAction(nameof(XuLy), new { id });
        }

        yeuCau.TrangThai = TrangThaiYeuCau.TuChoi;
        yeuCau.GhiChuAdmin = ghiChu;
        yeuCau.NgayXuLy = DateTime.Now;
        yeuCau.NguoiXuLyId = user.Id;

        await _context.SaveChangesAsync();

        TempData["Error"] = "Đã từ chối yêu cầu.";
        return RedirectToAction(nameof(Index));
    }
}