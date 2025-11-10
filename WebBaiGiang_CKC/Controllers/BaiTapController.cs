using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using System.Globalization;

namespace WebBaiGiang_CKC.Controllers
{
    public class BaiTapController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;
        private readonly IWebHostEnvironment _env;

        public BaiTapController(WebBaiGiangContext context, INotyfService notyf, IWebHostEnvironment env)
        {
            _context = context;
            _notyf = notyf;
            _env = env;
        }

        // ✅ Xem chi tiết bài tập
        public async Task<IActionResult> ChiTiet(int baiTapId)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyf.Warning("Bạn cần đăng nhập để xem chi tiết bài tập!");
                return RedirectToAction("Index", "Home");
            }

            var baiTap = await _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .FirstOrDefaultAsync(b => b.MaBaiTap == baiTapId);

            if (baiTap == null)
            {
                _notyf.Error("Không tìm thấy bài tập.");
                return RedirectToAction("Index", "Home");
            }

            // 🧩 Load danh sách lớp học học viên đã đăng ký (để hiển thị sidebar)
            var lopHocDangKy = await _context.HocVien_LopHoc
                .Include(x => x.LopHoc)
                .Where(x => x.MaHocVien == maHocVien)
                .Select(x => x.LopHoc)
                .ToListAsync();

            ViewBag.LopHocDangKy = lopHocDangKy;
            ViewBag.ActiveMenu = "LopHoc";
            ViewBag.CurrentLopHocId = baiTap.Bai?.Chuong?.MaLopHoc;

            // 🧾 Lấy lần nộp gần nhất (nếu có)
            var baiTapNop = await _context.BaiTapNops
                .Where(x => x.MaBaiTap == baiTapId && x.MaHocVien == maHocVien)
                .OrderByDescending(x => x.NgayNop)
                .FirstOrDefaultAsync();

            ViewBag.TrangThaiNop = baiTapNop != null ? "Đã nộp" : "Chưa nộp";
            if (baiTapNop != null)
            {
                if (baiTapNop.Diem != null)
                {
                    var trangThai = "<div class='text-success fw-semibold'>Đã chấm</div>";
                    trangThai += $"<div>Điểm: <strong>{baiTapNop.Diem:0.0}</strong></div>";

                    if (!string.IsNullOrEmpty(baiTapNop.NhanXet))
                        trangThai += $"<div>Nhận xét: <em>{baiTapNop.NhanXet}</em></div>";

                    ViewBag.TrangThaiCham = trangThai;
                }
                else
                {
                    ViewBag.TrangThaiCham = "<span class='text-warning fw-semibold'>Chưa chấm</span>";
                }
            }
            else
            {
                ViewBag.TrangThaiCham = "<span class='text-muted'>Chưa có bài nộp</span>";
            }


            ViewBag.HanChot = baiTap.HanNop?.ToString("dddd, dd 'Tháng' MM yyyy, h:mm tt", new CultureInfo("vi-VN")) ?? "Không có hạn chót";
            ViewBag.BaiTapNop = baiTapNop;

            return View(baiTap);
        }

        // ✅ Trang nộp bài
        // ✅ Trang nộp bài
        public async Task<IActionResult> NopBai(int baiTapId)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyf.Warning("Bạn cần đăng nhập để nộp bài!");
                return RedirectToAction("Index", "Home");
            }

            var baiTap = await _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .FirstOrDefaultAsync(x => x.MaBaiTap == baiTapId);

            if (baiTap == null)
            {
                _notyf.Error("Không tìm thấy bài tập.");
                return RedirectToAction("Index", "Home");
            }

            // 🧩 Load danh sách lớp học học viên đã đăng ký (để hiển thị sidebar)
            var lopHocDangKy = await _context.HocVien_LopHoc
                .Include(x => x.LopHoc)
                .Where(x => x.MaHocVien == maHocVien)
                .Select(x => x.LopHoc)
                .ToListAsync();

            ViewBag.LopHocDangKy = lopHocDangKy;
            ViewBag.ActiveMenu = "LopHoc";
            ViewBag.CurrentLopHocId = baiTap.Bai?.Chuong?.MaLopHoc;

            ViewBag.TenBaiTap = baiTap.TenBaiTap;
            ViewBag.BaiTapId = baiTapId;
            ViewBag.MoTa = baiTap.MoTa;
            return View();
        }


        // ✅ Xử lý POST khi nộp bài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NopBai(int baiTapId, List<IFormFile> files)
        {
            var hocVienClaim = User.Claims.FirstOrDefault(c => c.Type == "HocVienId");
            if (hocVienClaim == null || !int.TryParse(hocVienClaim.Value, out int maHocVien))
            {
                _notyf.Error("Không xác thực được học viên!");
                return RedirectToAction("Index", "Home");
            }
            var lopHocDangKy = await _context.HocVien_LopHoc
                .Include(x => x.LopHoc)
                .Where(x => x.MaHocVien == maHocVien)
                .Select(x => x.LopHoc)
                .ToListAsync();

            ViewBag.LopHocDangKy = lopHocDangKy;
            ViewBag.ActiveMenu = "LopHoc";


            if (files == null || !files.Any())
            {
                _notyf.Warning("Vui lòng chọn ít nhất một tệp để nộp.");
                return RedirectToAction("NopBai", new { baiTapId });
            }

            string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "bainop");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = $"{maHocVien}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var lanNop = await _context.BaiTapNops.CountAsync(x => x.MaBaiTap == baiTapId && x.MaHocVien == maHocVien) + 1;

                    var baiTapNop = new BaiTapNop
                    {
                        MaBaiTap = baiTapId,
                        MaHocVien = maHocVien,
                        FileNop = $"/uploads/bainop/{fileName}",
                        NgayNop = DateTime.Now,
                        LanNop = lanNop,
                        TrangThai = "Đã nộp"
                    };

                    _context.BaiTapNops.Add(baiTapNop);
                }
            }

            await _context.SaveChangesAsync();
            _notyf.Success("Nộp bài thành công!");
            return RedirectToAction("ChiTiet", new { baiTapId });
        }
    }
}
