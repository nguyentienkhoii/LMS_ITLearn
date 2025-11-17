using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class GiangVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;

        public GiangVienController(WebBaiGiangContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // 📋 Danh sách giảng viên
        public async Task<IActionResult> Index()
        {
            var list = await _context.GiangViens
                .Include(g => g.TaiKhoan)
                .OrderByDescending(g => g.MaGiangVien)
                .ToListAsync();
            return View(list);
        }

        // ➕ GET: Tạo mới
        public IActionResult Create() => View();

        // ➕ POST: Tạo tài khoản + giảng viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,HoTen,Email,SoDienThoai,DiaChi,ChuyenMon")] GiangVienTaiKhoanVM model)
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
                    VaiTro = "GiangVien",
                    TrangThai = true
                };

                _context.TaiKhoanNews.Add(taiKhoan);
                await _context.SaveChangesAsync();

                var giangVien = new Models.GiangVien
                {
                    HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(model.HoTen.Trim()),
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    ChuyenMon = model.ChuyenMon,
                    MaTaiKhoan = taiKhoan.MaTaiKhoan
                };

                _context.GiangViens.Add(giangVien);
                await _context.SaveChangesAsync();

                _notyf.Success("Tạo tài khoản giảng viên thành công!");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ✏️ Sửa
        public async Task<IActionResult> Edit(int id)
        {
            var gv = await _context.GiangViens.Include(g => g.TaiKhoan).FirstOrDefaultAsync(g => g.MaGiangVien == id);
            if (gv == null) return NotFound();

            var vm = new GiangVienTaiKhoanVM
            {
                MaTaiKhoan = gv.TaiKhoan.MaTaiKhoan,
                TenDangNhap = gv.TaiKhoan.TenDangNhap,
                HoTen = gv.HoTen,
                Email = gv.Email,
                SoDienThoai = gv.SoDienThoai,
                DiaChi = gv.DiaChi,
                ChuyenMon = gv.ChuyenMon
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GiangVienTaiKhoanVM model)
        {
            var gv = await _context.GiangViens.Include(g => g.TaiKhoan).FirstOrDefaultAsync(g => g.MaGiangVien == id);
            if (gv == null) return NotFound();

            gv.HoTen = model.HoTen;
            gv.Email = model.Email;
            gv.SoDienThoai = model.SoDienThoai;
            gv.DiaChi = model.DiaChi;
            gv.ChuyenMon = model.ChuyenMon;
            gv.TaiKhoan.TenDangNhap = model.TenDangNhap;

            if (!string.IsNullOrEmpty(model.MatKhau))
                gv.TaiKhoan.MatKhau = HashMD5.ToMD5(model.MatKhau.Trim());

            await _context.SaveChangesAsync();
            _notyf.Success("Cập nhật giảng viên thành công!");
            return RedirectToAction(nameof(Index));
        }

        // ❌ Xóa
        public async Task<IActionResult> Delete(int id)
        {
            var gv = await _context.GiangViens.Include(g => g.TaiKhoan).FirstOrDefaultAsync(g => g.MaGiangVien == id);
            if (gv == null) return NotFound();
            return View(gv);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gv = await _context.GiangViens.Include(g => g.TaiKhoan).FirstOrDefaultAsync(g => g.MaGiangVien == id);
            if (gv != null)
            {
                _context.GiangViens.Remove(gv);
                await _context.SaveChangesAsync();

                if (gv.TaiKhoan != null)
                {
                    _context.TaiKhoanNews.Remove(gv.TaiKhoan);
                    await _context.SaveChangesAsync();
                }

                _notyf.Success("Xóa giảng viên thành công!");
            }
            return RedirectToAction(nameof(Index));
        }

        // ✅ Bật/tắt trạng thái (reload version)
        public async Task<IActionResult> ToggleTrangThai(int id)
        {
            var gv = await _context.GiangViens
                .Include(g => g.TaiKhoan)
                .FirstOrDefaultAsync(g => g.MaGiangVien == id);

            if (gv == null || gv.TaiKhoan == null)
            {
                _notyf.Error("Không tìm thấy giảng viên!");
                return RedirectToAction("Index");
            }

            gv.TaiKhoan.TrangThai = !gv.TaiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            string msg = gv.TaiKhoan.TrangThai ? "Đã mở khóa tài khoản!" : "Đã khóa tài khoản!";
            _notyf.Success(msg);

            return RedirectToAction("Index");
        }

        // ✅ Bật/tắt trạng thái (AJAX version – không reload trang)
        [HttpPost]
        public async Task<IActionResult> ToggleTrangThaiAjax(int id)
        {
            var gv = await _context.GiangViens
                .Include(g => g.TaiKhoan)
                .FirstOrDefaultAsync(g => g.MaGiangVien == id);

            if (gv == null || gv.TaiKhoan == null)
                return Json(new { success = false, message = "Không tìm thấy giảng viên!" });

            gv.TaiKhoan.TrangThai = !gv.TaiKhoan.TrangThai;
            await _context.SaveChangesAsync();

            var msg = gv.TaiKhoan.TrangThai ? "Đã mở khóa tài khoản!" : "Đã khóa tài khoản!";
            return Json(new { success = true, trangThai = gv.TaiKhoan.TrangThai, message = msg });
        }


    }

    // 🔹 ViewModel kết hợp dữ liệu giảng viên + tài khoản
    public class GiangVienTaiKhoanVM
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string ChuyenMon { get; set; }

        public bool TrangThai { get; set; }
    }
}
