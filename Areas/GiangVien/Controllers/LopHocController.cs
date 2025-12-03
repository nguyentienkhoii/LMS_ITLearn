using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class LopHocController : GiangVienBaseController
    {
        private readonly IWebHostEnvironment _env;

        public LopHocController(WebBaiGiangContext context, IWebHostEnvironment env) : base(context)
        {
            _env = env;
        }

        // ✅ Trang danh sách lớp học
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

        // ✅ Trang chi tiết / nội dung lớp học
        public async Task<IActionResult> NoiDung(int id)
        {
            var maTaiKhoan = int.Parse(User.FindFirstValue("MaTaiKhoan"));
            var gv = await _context.GiangViens.FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            if (gv == null) return Forbid();

            var lop = await _context.LopHocs
                .Include(l => l.KhoaHoc)
                .Include(l => l.HocVien_LopHocs)  // ⭐ để đếm số học viên trong lớp
                .Include(l => l.Chuongs)
                    .ThenInclude(c => c.Bais)
                        .ThenInclude(b => b.Mucs)
                .Include(l => l.Chuongs)
                    .ThenInclude(c => c.Bais)
                        .ThenInclude(b => b.BaiTaps)
                            .ThenInclude(bt => bt.BaiTapNops) // ⭐ để đếm bài nộp
                .FirstOrDefaultAsync(l => l.MaLopHoc == id && l.MaGiangVien == gv.MaGiangVien);

            if (lop == null) return NotFound();

            // ===========================
            // ⭐ CHỈ LẤY SỰ KIỆN SẮP HẾT HẠN NỘP BÀI (≤ 24 GIỜ)
            // ===========================
            var now = DateTime.Now;

            var upcoming = lop.Chuongs
                .SelectMany(c => c.Bais)
                .SelectMany(b => b.BaiTaps)
                .Where(bt => bt.HanNop != null &&
                             bt.HanNop > now &&
                             bt.HanNop <= now.AddDays(1)) // ⭐ chỉ trong 24 giờ trước hạn
                .Select(bt => new
                {
                    Ten = bt.TenBaiTap,
                    Ngay = bt.HanNop,
                    DaNop = bt.BaiTapNops?.Count() ?? 0,
                    TongHV = lop.HocVien_LopHocs?.Count() ?? 0,
                    Text = $"Sắp hết hạn nộp {bt.TenBaiTap} – {(bt.BaiTapNops?.Count() ?? 0)}/{(lop.HocVien_LopHocs?.Count() ?? 0)} học viên đã nộp"
                })
                .OrderBy(x => x.Ngay)
                .ToList();

            ViewBag.Upcoming = upcoming;
            ViewBag.MaLopHoc = id;

            return View(lop);
        }



        // 🖼️ ✅ Upload & cập nhật ảnh banner lớp học
        [HttpPost]
        public async Task<IActionResult> CapNhatAnh(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn một ảnh hợp lệ." });

            var lop = await _context.LopHocs.FindAsync(id);
            if (lop == null)
                return Json(new { success = false, message = "Không tìm thấy lớp học." });

            // 🗂️ Tạo đường dẫn lưu file
            var uploadsFolder = Path.Combine(_env.WebRootPath, "MonHoc");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 🧩 Tên file duy nhất
            var fileName = $"lop_{id}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 📝 Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 🧹 Xóa ảnh cũ (nếu có)
            if (!string.IsNullOrEmpty(lop.AnhLopHoc))
            {
                var oldPath = Path.Combine(_env.WebRootPath, lop.AnhLopHoc.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            // 🗃️ Cập nhật DB
            lop.AnhLopHoc = $"/MonHoc/{fileName}";
            _context.Update(lop);
            await _context.SaveChangesAsync();

            return Json(new { success = true, path = lop.AnhLopHoc });
        }
    }
}
