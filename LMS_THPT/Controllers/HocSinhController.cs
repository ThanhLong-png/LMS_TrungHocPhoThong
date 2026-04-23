using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LMS_THPT.Controllers
{
    [Authorize(Roles = "HocSinh")]
    public class HocSinhController : Controller
    {
        // Trang chính của Học sinh
        public IActionResult Index()
        {
            return View(); // Views/HocSinh/Index.cshtml
        }

        // Ví dụ: xem điểm
        public IActionResult XemDiem()
        {
            return View(); // Views/HocSinh/XemDiem.cshtml
        }
    }
}