using LMS_THPT.Data;
using LMS_THPT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class ThongBaoController : Controller
{
    private readonly ApplicationDbContext _context;

    public ThongBaoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Danh sách
    public async Task<IActionResult> Index()
    {
        var list = await _context.ThongBaos
            .OrderByDescending(x => x.NgayDang)
            .ToListAsync();

        return View(list);
    }

    // GET: Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ThongBao tb)
    {
        if (ModelState.IsValid)
        {
            tb.NgayDang = DateTime.Now;
            _context.Add(tb);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(tb);
    }

    // Ẩn/hiện
    public async Task<IActionResult> Toggle(int id)
    {
        var tb = await _context.ThongBaos.FindAsync(id);
        if (tb == null) return NotFound();

        tb.HienThi = !tb.HienThi;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> ToggleHienThi(int id)
    {
        var tb = await _context.ThongBaos.FindAsync(id);
        if (tb == null) return NotFound();

        tb.HienThi = !tb.HienThi;
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var tb = await _context.ThongBaos.FindAsync(id);
        if (tb == null) return NotFound();

        _context.ThongBaos.Remove(tb);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}