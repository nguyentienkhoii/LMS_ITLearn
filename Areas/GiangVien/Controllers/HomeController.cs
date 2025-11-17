using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class HomeController : GiangVienBaseController
    {
        public HomeController(WebBaiGiangContext context) : base(context)
        {
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

        [HttpGet]
        [Authorize(Roles = "GiangVien")]
        public async Task<IActionResult> HoSo()
        {
            var s = User.FindFirst("MaTaiKhoan")?.Value
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? maTaiKhoan = int.TryParse(s, out var id) ? id : null;

            if (maTaiKhoan == null)
            {
                var username = User.FindFirstValue("TenDangNhap") ?? User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(username))
                {
                    var tk = await _context.TaiKhoanNews
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.TenDangNhap == username);
                    if (tk != null) maTaiKhoan = tk.MaTaiKhoan;
                }
            }

            if (maTaiKhoan == null) return Unauthorized("Không xác định được tài khoản.");

            var giangVien = await _context.GiangViens
                .AsNoTracking()
                .AsSplitQuery() // tránh Cartesian explosion khi Include sâu
                .Include(g => g.TaiKhoan)
                .Include(g => g.LopHocs!.OrderBy(l => l.TenLopHoc))
                    .ThenInclude(l => l.KhoaHoc)   // 🔹 quan trọng để có TenKhoaHoc
                .FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan.Value);

            if (giangVien == null)
                return NotFound("Tài khoản chưa có hồ sơ giảng viên.");

            return View(giangVien);
        }
    }
    
}
