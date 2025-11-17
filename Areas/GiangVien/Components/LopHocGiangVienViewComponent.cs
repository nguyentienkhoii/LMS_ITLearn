using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Areas.GiangVien.Components
{
    public class LopHocGiangVienViewComponent : ViewComponent
    {
        private readonly WebBaiGiangContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LopHocGiangVienViewComponent(WebBaiGiangContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync(int maGiangVien)
        {
            // ✅ Nếu thiếu claim MaGiangVien thì fallback sang MaTaiKhoan
            if (maGiangVien == 0)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                var maTaiKhoanStr = user?.FindFirstValue("MaTaiKhoan");
                if (int.TryParse(maTaiKhoanStr, out int maTK))
                {
                    maGiangVien = await _context.GiangViens
                        .Where(g => g.MaTaiKhoan == maTK)
                        .Select(g => g.MaGiangVien)
                        .FirstOrDefaultAsync();
                }
            }

            var lopHocs = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Where(l => l.MaGiangVien == maGiangVien)
                .OrderBy(l => l.TenLopHoc)
                .ToListAsync();

            return View("~/Areas/GiangVien/Views/Shared/Components/LopHocGiangVien/Default.cshtml", lopHocs);
        }
    }
}
