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
            query = query.Where(y => (int)y.TrangThai == trangThai.Value);
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
        ViewBag.ChoDuyet = await _context.YeuCauGiaoVien.CountAsync(y => y.TrangThai == TrangThaiYeuCau.ChoDuyet);
        ViewBag.DaDuyet = await _context.YeuCauGiaoVien.CountAsync(y => y.TrangThai == TrangThaiYeuCau.DaDuyet);
        ViewBag.TuChoi = await _context.YeuCauGiaoVien.CountAsync(y => y.TrangThai == TrangThaiYeuCau.TuChoi);

        return View(danhSach);
    }

    // GET: Trang xử lý yêu cầu
    public async Task<IActionResult> XuLy(int id)
    {
        var yeuCau = await _context.YeuCauGiaoVien
            .Include(y => y.GiaoVien)
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

        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã duyệt yêu cầu thành công.";
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