using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "GiangVien")]
    public class GiangVienController : Controller
    {
        // Constructor nếu cần dependency injection
        public GiangVienController()
        {
        }

        // Trang chính của Giảng viên
        public IActionResult Index()
        {
            return View(); // View/ GiangVien/ Index.cshtml
        }

        // Ví dụ trang quản lý lớp
        public IActionResult QuanLyLop()
        {
            return View();
        }
    }
}