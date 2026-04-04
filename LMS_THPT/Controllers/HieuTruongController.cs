using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "HieuTruong")]
    public class HieuTruongController : Controller
    {
        // Trang chính của Hiệu trưởng
        public IActionResult Index()
        {
            return View(); // Views/HieuTruong/Index.cshtml
        }

        // Ví dụ: quản lý giáo viên
        public IActionResult QuanLyGiaoVien()
        {
            return View(); // Views/HieuTruong/QuanLyGiaoVien.cshtml
        }
    }
}