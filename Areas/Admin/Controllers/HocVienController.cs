using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HocVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;
        private readonly IConfiguration _configuration;

        public HocVienController(WebBaiGiangContext context, INotyfService notyf, IConfiguration configuration)
        {
            _context = context;
            _notyf = notyf;
            _configuration = configuration;
        }

        // 📜 Danh sách học viên
        public async Task<IActionResult> Index()
        {
            var list = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .OrderByDescending(h => h.MaHocVien)
                .ToListAsync();
            return View(list);
        }

        // ➕ GET: Tạo mới
        public IActionResult Create() => View();

        // ➕ POST: Tạo tài khoản + học viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,HoTen,Email,SoDienThoai,DiaChi")] HocVienTaiKhoanVM model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.TaiKhoanNews.AnyAsync(x => x.TenDangNhap == model.TenDangNhap))
                {
                    _notyf.Error("Tên đăng nhập đã tồn tại!");
                    return View(model);
                }

                var taiKhoan = new TaiKhoanNew
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = HashMD5.ToMD5(model.MatKhau.Trim()),
                    VaiTro = "HocVien",
                    TrangThai = true
                };

                _context.TaiKhoanNews.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var hocVien = new Models.HocVien
                {
                    HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.HoTen.Trim()),
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    MaTaiKhoan = taiKhoan.MaTaiKhoan
                };

                _context.HocViens.Add(hocVien);
                await _context.SaveChangesAsync();

                _notyf.Success("Tạo tài khoản học viên thành công!");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ✅ IMPORT FILE EXCEL
        public IActionResult ImportExcel() => View();

        [HttpPost]
        public IActionResult ImportExcel(IFormFile formFile)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var mainPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Files");
                if (!Directory.Exists(mainPath))
                    Directory.CreateDirectory(mainPath);

                var filePath = Path.Combine(mainPath, $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}");
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }

                using var package = new ExcelPackage(new FileInfo(filePath));
                var worksheet = package.Workbook.Worksheets[0];
                int totalRows = worksheet.Dimension.End.Row;

                int added = 0;

                for (int row = 2; row <= totalRows; row++)
                {
                    string tenDangNhap = worksheet.Cells[row, 1].Value?.ToString().Trim();
                    string matKhau = worksheet.Cells[row, 2].Value?.ToString().Trim();
                    string hoTen = worksheet.Cells[row, 3].Value?.ToString().Trim();
                    string email = worksheet.Cells[row, 4].Value?.ToString().Trim();
                    string sdt = worksheet.Cells[row, 5].Value?.ToString().Trim();
                    string diaChi = worksheet.Cells[row, 6].Value?.ToString().Trim();

                    if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrWhiteSpace(matKhau))
                        continue;

                    hoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(hoTen?.ToLower() ?? "");

                    // Kiểm tra trùng
                    bool existTK = _context.TaiKhoanNews.Any(x => x.TenDangNhap == tenDangNhap);
                    bool existEmail = _context.HocViens.Any(x => x.Email == email);

                    if (existTK || existEmail)
                        continue;

                    var taiKhoan = new TaiKhoanNew
                    {
                        TenDangNhap = tenDangNhap,
                        MatKhau = HashMD5.ToMD5(matKhau),
                        VaiTro = "HocVien",
                        TrangThai = true
                    };
                    _context.TaiKhoanNews.Add(taiKhoan);
                    _context.SaveChanges();

                    var hocVien = new Models.HocVien
                    {
                        HoTen = hoTen,
                        Email = email,
                        SoDienThoai = sdt,
                        DiaChi = diaChi,
                        MaTaiKhoan = taiKhoan.MaTaiKhoan
                    };
                    _context.HocViens.Add(hocVien);
                    _context.SaveChanges();

                    added++;
                }

                _notyf.Success($"Import thành công {added} học viên!");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _notyf.Error("Import thất bại: " + ex.Message);
                return RedirectToAction("Index");
            }
        }



        // 📥 DOWNLOAD FILE MẪU
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Files", "ImportHocVien.xlsx");
            if (!System.IO.File.Exists(filePath))
            {
                _notyf.Error("File mẫu không tồn tại!");
                return RedirectToAction("Index");
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            return File(memory,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                Path.GetFileName(filePath));
        }

        // ✏️ Sửa học viên
        public async Task<IActionResult> Edit(int id)
        {
            var hv = await _context.HocViens.Include(h => h.TaiKhoan).FirstOrDefaultAsync(h => h.MaHocVien == id);
            if (hv == null) return NotFound();

            var vm = new HocVienTaiKhoanVM
            {
                MaTaiKhoan = hv.TaiKhoan.MaTaiKhoan,
                TenDangNhap = hv.TaiKhoan.TenDangNhap,
                HoTen = hv.HoTen,
                Email = hv.Email,
                SoDienThoai = hv.SoDienThoai,
                DiaChi = hv.DiaChi,
                TrangThai = hv.TaiKhoan.TrangThai
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HocVienTaiKhoanVM model)
        {
            var hv = await _context.HocViens.Include(h => h.TaiKhoan).FirstOrDefaultAsync(h => h.MaHocVien == id);
            if (hv == null) return NotFound();

            hv.HoTen = model.HoTen;
            hv.Email = model.Email;
            hv.SoDienThoai = model.SoDienThoai;
            hv.DiaChi = model.DiaChi;
            hv.TaiKhoan.TenDangNhap = model.TenDangNhap;
            hv.TaiKhoan.TrangThai = model.TrangThai;

            if (!string.IsNullOrEmpty(model.MatKhau))
                hv.TaiKhoan.MatKhau = HashMD5.ToMD5(model.MatKhau.Trim());

            await _context.SaveChangesAsync();
            _notyf.Success("Cập nhật học viên thành công!");
            return RedirectToAction(nameof(Index));
        }

        // ❌ Xóa
        public async Task<IActionResult> Delete(int id)
        {
            var hv = await _context.HocViens.Include(h => h.TaiKhoan).FirstOrDefaultAsync(h => h.MaHocVien == id);
            if (hv == null) return NotFound();
            return View(hv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hv = await _context.HocViens.Include(h => h.TaiKhoan).FirstOrDefaultAsync(h => h.MaHocVien == id);
            if (hv != null)
            {
                _context.HocViens.Remove(hv);
                await _context.SaveChangesAsync();

                if (hv.TaiKhoan != null)
                {
                    _context.TaiKhoanNews.Remove(hv.TaiKhoan);
                    await _context.SaveChangesAsync();
                }

                _notyf.Success("Xóa học viên thành công!");
            }
            return RedirectToAction(nameof(Index));
        }

        //edit status nhé
        [HttpPost]
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var hv = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .FirstOrDefaultAsync(h => h.MaHocVien == id);

            if (hv == null || hv.TaiKhoan == null)
            {
                _notyf.Error("Không tìm thấy học viên!");
                return RedirectToAction("Index");
            }

            hv.TaiKhoan.TrangThai = !hv.TaiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            string msg = hv.TaiKhoan.TrangThai
                ? $"Đã mở khóa tài khoản '{hv.TaiKhoan.TenDangNhap}'!"
                : $"Đã khóa tài khoản '{hv.TaiKhoan.TenDangNhap}'!";
            _notyf.Success(msg);

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ToggleTrangThaiAjax(int id)
        {
            var hv = await _context.HocViens
                .Include(h => h.TaiKhoan)
                .FirstOrDefaultAsync(h => h.MaHocVien == id);

            if (hv == null || hv.TaiKhoan == null)
                return Json(new { success = false, message = "Không tìm thấy học viên!" });

            hv.TaiKhoan.TrangThai = !hv.TaiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            var msg = hv.TaiKhoan.TrangThai ? "Đã mở khóa tài khoản!" : "Đã khóa tài khoản!";
            return Json(new
            {
                success = true,
                trangThai = hv.TaiKhoan.TrangThai,
                message = msg
            });
        }


    }

    // 🔹 ViewModel kết hợp dữ liệu học viên + tài khoản
    public class HocVienTaiKhoanVM
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }

        public bool TrangThai { get; set; }
    }
}
