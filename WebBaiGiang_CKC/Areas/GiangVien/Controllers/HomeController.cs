using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class HomeController : Controller
    {
        private readonly WebBaiGiangContext _context;

        public HomeController(WebBaiGiangContext context)
        {
            _context = context;
        }

        // Trang chủ giảng viên: hiển thị danh sách lớp mà họ dạy
        public async Task<IActionResult> Index()
        {
            var maTaiKhoan = int.Parse(User.FindFirstValue("MaTaiKhoan"));
            var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            if (gv == null) return NotFound("Không tìm thấy giảng viên.");

            var lopHocs = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Where(l => l.MaGiangVien == gv.MaGiangVien)
                .ToListAsync();

            return View(lopHocs);
        }
    }
}
