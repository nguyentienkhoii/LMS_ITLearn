using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien,Admin")]
    public class GiangVienBaseController : Controller
    {
        protected readonly WebBaiGiangContext _context;

        public GiangVienBaseController(WebBaiGiangContext context)
        {
            _context = context;
        }

        // ⭐ Tự chạy mỗi khi load page
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            try
            {
                // Lấy ID đăng nhập của giảng viên
                var teacherId = User.FindFirstValue("MaTaiKhoan");

                if (!string.IsNullOrEmpty(teacherId))
                {
                    int id = int.Parse(teacherId);

                    // 🔔 Lấy tổng số thông báo chưa đọc
                    ViewBag.Unread = _context.Notifications
                        .Count(n => n.UserId == id && !n.IsRead);
                }
            }
            catch
            {
                ViewBag.Unread = 0; // fallback
            }
        }
    }
}
