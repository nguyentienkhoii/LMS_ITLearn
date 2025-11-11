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
    public class BaiTapController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;

        public BaiTapController(WebBaiGiangContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // GET: GiangVien/BaiTap/Them?baiId=5&maLopHoc=3
        public async Task<IActionResult> Them(int baiId, int maLopHoc)
        {
            var bai = await _context.Bai
                .Include(b => b.Chuong)
                .FirstOrDefaultAsync(b => b.BaiId == baiId);

            if (bai == null)
                return NotFound("Không tìm thấy bài học.");

            ViewBag.Bai = bai;
            ViewBag.MaLopHoc = maLopHoc;

            return View(new BaiTap { BaiId = baiId });
        }

        // POST: GiangVien/BaiTap/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(BaiTap baiTap, int maLopHoc, IFormFile? FileUpload)
        {
            // Kiểm tra dữ liệu hợp lệ
            if (!ModelState.IsValid)
            {
                var bai = await _context.Bai.Include(b => b.Chuong).FirstOrDefaultAsync(b => b.BaiId == baiTap.BaiId);
                ViewBag.Bai = bai;
                ViewBag.MaLopHoc = maLopHoc;
                _notyf.Error("⚠️ Vui lòng nhập đầy đủ thông tin hợp lệ!");
                return View(baiTap);
            }

            // Xử lý file upload nếu có
            if (FileUpload != null && FileUpload.Length > 0)
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads", "baitap");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(FileUpload.FileName)}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(stream);
                }

                baiTap.FileDinhKem = "/uploads/baitap/" + uniqueFileName;
            }

            // Lưu vào DB
            _context.BaiTaps.Add(baiTap);
            await _context.SaveChangesAsync();

            _notyf.Success("✅ Đã thêm bài tập mới thành công!");
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
        }

        // =========================
        // 🔹 SỬA BÀI TẬP
        // =========================

        // GET: GiangVien/BaiTap/Sua?id=10&maLopHoc=3
        public async Task<IActionResult> Sua(int id, int maLopHoc)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == id);

            if (baiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập cần sửa.");
                return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
            }

            ViewBag.Bai = baiTap.Bai;
            ViewBag.MaLopHoc = maLopHoc;
            return View(baiTap);
        }

        // POST: GiangVien/BaiTap/Sua
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(BaiTap baiTap, int maLopHoc, IFormFile? FileUpload)
        {
            if (!ModelState.IsValid)
            {
                var bai = await _context.Bai.Include(b => b.Chuong).FirstOrDefaultAsync(b => b.BaiId == baiTap.BaiId);
                ViewBag.Bai = bai;
                ViewBag.MaLopHoc = maLopHoc;
                _notyf.Warning("⚠️ Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");
                return View(baiTap);
            }

            var existingBaiTap = await _context.BaiTaps.FindAsync(baiTap.MaBaiTap);
            if (existingBaiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập cần sửa!");
                return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
            }

            // Cập nhật thông tin
            existingBaiTap.TenBaiTap = baiTap.TenBaiTap;
            existingBaiTap.MoTa = baiTap.MoTa;
            existingBaiTap.HanNop = baiTap.HanNop;

            // Nếu có file upload mới
            if (FileUpload != null && FileUpload.Length > 0)
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads", "baitap");
                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(FileUpload.FileName)}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await FileUpload.CopyToAsync(stream);
                }

                existingBaiTap.FileDinhKem = "/uploads/baitap/" + uniqueFileName;
            }

            _context.Update(existingBaiTap);
            await _context.SaveChangesAsync();

            _notyf.Success("✅ Đã cập nhật bài tập thành công!");
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = maLopHoc });
        }

        // ======================
        // 🔹 XÓA BÀI TẬP
        // ======================
        [HttpGet]
        public async Task<IActionResult> Xoa(int id, int? maLopHoc)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == id);

            if (baiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập để xóa!");
                return RedirectToAction("Index", "LopHoc");
            }

            // ✅ Lấy mã lớp học từ bài tập nếu không có trong URL
            int lopHocId = maLopHoc ?? baiTap.Bai.Chuong.MaLopHoc;

            try
            {
                _context.BaiTaps.Remove(baiTap);
                await _context.SaveChangesAsync();
                _notyf.Success("🗑️ Đã xóa bài tập thành công!");
            }
            catch (Exception)
            {
                _notyf.Error("⚠️ Không thể xóa bài tập (có thể đang được liên kết)!");
            }

            // ✅ Quay về đúng lớp học
            return RedirectToAction("NoiDung", "LopHoc", new { area = "GiangVien", id = lopHocId });
        }


        // 🔹 DANH SÁCH BÀI NỘP
        // ===========================
        public async Task<IActionResult> DanhSachBaiNop(int id)
        {
            var baiTap = await _context.BaiTaps
                .Include(bt => bt.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .FirstOrDefaultAsync(bt => bt.MaBaiTap == id);

            if (baiTap == null)
            {
                _notyf.Error("❌ Không tìm thấy bài tập!");
                return RedirectToAction("Index", "LopHoc", new { area = "GiangVien" });
            }

            var danhSachNop = await _context.BaiTapNops
                .Include(n => n.HocVien)
                .Where(n => n.MaBaiTap == id)
                .OrderByDescending(n => n.NgayNop)
                .ToListAsync();

            ViewBag.BaiTap = baiTap;
            ViewBag.LopHoc = baiTap.Bai?.Chuong?.LopHoc;
            ViewBag.MaLopHoc = baiTap.Bai?.Chuong?.MaLopHoc;

            return View(danhSachNop);
        }


        // ✳️ Trang chấm điểm
        public async Task<IActionResult> ChamDiem(int id)
        {
            var baiNop = await _context.BaiTapNops
                .Include(x => x.HocVien)
                .Include(x => x.BaiTap)
                    .ThenInclude(bt => bt.Bai)
                        .ThenInclude(b => b.Chuong)
                .FirstOrDefaultAsync(x => x.MaBaiTapNop == id);

            if (baiNop == null)
            {
                TempData["Error"] = "Không tìm thấy bài nộp!";
                return RedirectToAction("Index");
            }

            // ✅ Lấy mã lớp học cho view và menu
            ViewBag.MaLopHoc = baiNop.BaiTap?.Bai?.Chuong?.MaLopHoc;

            return View(baiNop);
        }


        // ✳️ Lưu điểm và nhận xét
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChamDiem(int id, double? diem, string nhanXet)
        {
            var baiNop = await _context.BaiTapNops.FindAsync(id);
            if (baiNop == null)
            {
                TempData["Error"] = "Bài nộp không tồn tại!";
                return RedirectToAction("Index");
            }

            baiNop.Diem = diem;
            baiNop.NhanXet = nhanXet;
            _context.Update(baiNop);
            await _context.SaveChangesAsync();

            _notyf.Success("✅ Chấm điểm thành công!");

            return RedirectToAction("DanhSachBaiNop", new { id = baiNop.MaBaiTap }); // ✅ sửa dòng này
        }


    }
}
