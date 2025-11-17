using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class NotificationController : GiangVienBaseController
    {
        public NotificationController(WebBaiGiangContext context) : base(context) { }

        public async Task<IActionResult> Index()
        {
            var teacherId = int.Parse(User.FindFirstValue("MaTaiKhoan"));

            var list = await _context.Notifications
                .Where(n => n.UserId == teacherId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            foreach (var n in list.Where(x => !x.IsRead))
                n.IsRead = true;

            await _context.SaveChangesAsync();

            return View(list);
        }
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var noti = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (noti == null) return NotFound();

            noti.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

    }
}
