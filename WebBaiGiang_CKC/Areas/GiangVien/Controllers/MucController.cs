using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.GiangVien.Controllers
{
    [Area("GiangVien")]
    [Authorize(Roles = "GiangVien")]
    public class MucController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;

        public MucController(WebBaiGiangContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // =========================
        // 🔹 THÊM MỤC
        // =========================
        [HttpGet]
        public async Task<IActionResult> Them(int baiId, int maLopHoc)
        {
            var bai = await _context.Bai
                .Include(b => b.Chuong)
                .FirstOrDefaultAsync(b => b.BaiId == baiId);

            if (bai == null)
                return NotFound("Không tìm thấy bài học.");

            ViewBag.Bai = bai;
            ViewBag.MaLopHoc = maLopHoc;
            return View(new Muc { BaiId = baiId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(Muc muc, int maLopHoc, List<IFormFile>? FileTaiLieuList)
        {
            if (!ModelState.IsValid)
            {
                var bai = await _context.Bai.Include(b => b.Chuong).FirstOrDefaultAsync(b => b.BaiId == muc.BaiId);
                ViewBag.Bai = bai;
                ViewBag.MaLopHoc = maLopHoc;
                _notyf.Error("⚠️ Vui lòng nhập đầy đủ thông tin hợp lệ!");
                return View(muc);
            }

            _context.Muc.Add(muc);
            await _context.SaveChangesAsync();

            if (FileTaiLieuList != null && FileTaiLieuList.Any())
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads", "tailieu");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                foreach (var file in FileTaiLieuList)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.TaiLieus.Add(new TaiLieu
                        {
                            FileTaiLieu = "/uploads/tailieu/" + uniqueFileName,
                            MaMucCon = muc.MucId
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }

            _notyf.Success("✅ Thêm mục mới thành công!");
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
        }

        // =========================
        // 🔹 SỬA MỤC
        // =========================
        [HttpGet]
        public async Task<IActionResult> Sua(int id, int maLopHoc)
        {
            var muc = await _context.Muc
                .Include(m => m.Bai)
                    .ThenInclude(b => b.Chuong)
                .Include(m => m.TaiLieus)
                .FirstOrDefaultAsync(m => m.MucId == id);

            if (muc == null)
            {
                _notyf.Error("❌ Không tìm thấy mục cần sửa!");
                return RedirectToAction("NoiDung", "LopHoc", new { id = maLopHoc });
            }

            ViewBag.Bai = muc.Bai;
            ViewBag.MaLopHoc = maLopHoc;
            return View(muc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(Muc muc, int maLopHoc, List<IFormFile>? FileTaiLieuList)
        {
            var existingMuc = await _context.Muc
                .Include(m => m.TaiLieus)
                .FirstOrDefaultAsync(m => m.MucId == muc.MucId);

            if (existingMuc == null)
            {
                _notyf.Error("❌ Không tìm thấy mục cần sửa!");
                return RedirectToAction("NoiDung", "LopHoc", new { id = maLopHoc });
            }

            existingMuc.TenMuc = muc.TenMuc;
            existingMuc.NoiDung = muc.NoiDung;

            if (FileTaiLieuList != null && FileTaiLieuList.Any())
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads", "tailieu");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                foreach (var file in FileTaiLieuList)
                {
                    if (file.Length > 0)
                    {
                        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                        var filePath = Path.Combine(uploadFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.TaiLieus.Add(new TaiLieu
                        {
                            FileTaiLieu = "/uploads/tailieu/" + uniqueFileName,
                            MaMucCon = muc.MucId
                        });
                    }
                }
            }

            _context.Update(existingMuc);
            await _context.SaveChangesAsync();

            _notyf.Success("✅ Cập nhật mục thành công!");
            return RedirectToAction("NoiDung", "LopHoc", new { id = maLopHoc });
        }

        // =========================
        // 🔹 XÓA MỤC
        // =========================
        [HttpGet]
        public async Task<IActionResult> Xoa(int id, int? maLopHoc)
        {
            var muc = await _context.Muc
                .Include(m => m.Bai)
                    .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(m => m.MucId == id);

            if (muc == null)
            {
                _notyf.Error("❌ Không tìm thấy mục để xóa!");
                return RedirectToAction("Index", "LopHoc");
            }

            int lopHocId = maLopHoc ?? muc.Bai.Chuong.MaLopHoc;

            try
            {
                var taiLieus = await _context.TaiLieus.Where(t => t.MaMucCon == id).ToListAsync();
                _context.TaiLieus.RemoveRange(taiLieus);

                _context.Muc.Remove(muc);
                await _context.SaveChangesAsync();

                _notyf.Success("🗑️ Đã xóa mục thành công!");
            }
            catch
            {
                _notyf.Error("⚠️ Không thể xóa mục này (có thể đang được liên kết)!");
            }

            return RedirectToAction("NoiDung", "LopHoc", new { id = lopHocId });
        }

        [HttpPost]
        public async Task<IActionResult> NoiDungImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Không có file nào được chọn");

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "noidung");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"/uploads/noidung/{fileName}";

            // TinyMCE cần trả về JSON có field "location"
            return Json(new { location = fileUrl });
        }

    }
}
