using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Helper;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class GiaoVienController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public static string image;
        public INotyfService _notyfService { get; }
        public GiaoVienController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/GiaoVien
        public async Task<IActionResult> Index()
        {
            return View(await _context.GiaoVien.ToListAsync());
        }

        // GET: Admin/GiaoVien/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.GiaoVien == null)
            {
                return NotFound();
            }

            var giaoVien = await _context.GiaoVien
                .FirstOrDefaultAsync(m => m.Id == id);
            if (giaoVien == null)
            {
                return NotFound();
            }

            return View(giaoVien);
        }

        // GET: Admin/GiaoVien/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/GiaoVien/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenDangNhap,MatKhau,HoTen,Email,AnhDaiDien,IsGiaoVien,TrangThai")] GiaoVien giaoVien, IFormFile fAvatar)
        {
            if (ModelState.IsValid)
            {
                giaoVien.HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(giaoVien.HoTen);
                if (fAvatar != null)
                {
                    string extennsion = Path.GetExtension(fAvatar.FileName);
                    image = Utilities.ToUrlFriendly(giaoVien.TenDangNhap) + extennsion;
                    giaoVien.AnhDaiDien = await Utilities.UploadFile(fAvatar, @"GiaoVien", image.ToLower());
                }

                // Kiểm tra xem MSSV đã tồn tại trong cơ sở dữ liệu hay chưa
                if (_context.GiaoVien.Any(a => a.TenDangNhap == giaoVien.TenDangNhap))
                {
                    ModelState.AddModelError("Tài khoản", "Tài khoản đã tồn tại trong hệ thống.");
                    return View(giaoVien);
                }

                // Kiểm tra xem Email đã tồn tại trong cơ sở dữ liệu hay chưa
                if (_context.GiaoVien.Any(a => a.Email == giaoVien.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                    return View(giaoVien);
                }
                giaoVien.MatKhau = giaoVien.MatKhau.ToMD5();
                _context.Add(giaoVien);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm Thành Công");
                return RedirectToAction(nameof(Index));
            }
            return View(giaoVien);
        }

        // GET: Admin/GiaoVien/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.GiaoVien == null)
            {
                return NotFound();
            }

            var giaoVien = await _context.GiaoVien.FindAsync(id);
            if (giaoVien == null)
            {
                return NotFound();
            }
            return View(giaoVien);
        }

        // POST: Admin/GiaoVien/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenDangNhap,HoTen,Email,AnhDaiDien,IsGiaoVien,TrangThai")] GiaoVien giaoVien, IFormFile fAvatar)
        {
            if (id != giaoVien.Id)
            {
                return NotFound();
            }

            try
            {
               
                var giaoVienFromDb = await _context.GiaoVien.FindAsync(giaoVien.Id);
               
                if (giaoVienFromDb == null)
                {
                    return NotFound();
                }
                if (fAvatar != null)
                {
                    string extennsion = Path.GetExtension(fAvatar.FileName);
                    string image = Utilities.ToUrlFriendly(giaoVien.TenDangNhap) + extennsion;
                    giaoVienFromDb.AnhDaiDien = await Utilities.UploadFile(fAvatar, @"GiaoVien", image.ToLower());
                }
                else
                {
                    giaoVien.AnhDaiDien = _context.GiaoVien.Where(x => x.Id == giaoVien.Id).Select(x => x.AnhDaiDien).FirstOrDefault();
                }
                giaoVienFromDb.TenDangNhap = giaoVien.TenDangNhap;
                giaoVienFromDb.HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(giaoVien.HoTen);
                giaoVienFromDb.Email = giaoVien.Email;
                giaoVienFromDb.IsGiaoVien = giaoVien.IsGiaoVien;
                giaoVienFromDb.TrangThai = giaoVien.TrangThai;

               

                if (await _context.GiaoVien.AnyAsync(x => x.TenDangNhap == giaoVien.TenDangNhap && x.Id != giaoVien.Id))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại trong hệ thống.");
                    return View(giaoVien);
                }

                if (await _context.GiaoVien.AnyAsync(x => x.Email == giaoVien.Email && x.Id != giaoVien.Id))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                    return View(giaoVien);
                }

                _notyfService.Success("Sửa thành công!");
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GiaoVienExists(giaoVien.Id))
                {
                    _notyfService.Error("Lỗi!!!!!!!!!!!!");
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/GiaoVien/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.GiaoVien == null)
            {
                return NotFound();
            }

            var giaoVien = await _context.GiaoVien
                .FirstOrDefaultAsync(m => m.Id == id);
            if (giaoVien == null)
            {
                return NotFound();
            }

            return View(giaoVien);
        }

        // POST: Admin/GiaoVien/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            ClaimsPrincipal claimsPrincipal = HttpContext.User;
            var giaoVien = await _context.GiaoVien.FindAsync(id);

            if (giaoVien == null)
            {
                return NotFound();
            }
            var giaovienid = User.Claims.FirstOrDefault(c => c.Type == "TenDangNhap");
            var tendangnhap = giaovienid.Value;
            if (giaoVien.TenDangNhap ==tendangnhap)
            {
                _notyfService.Error("Bạn không thể xóa tài khoản của mình.");
                return View();
            }

            // Xóa giáo viên khỏi cơ sở dữ liệu
            _context.GiaoVien.Remove(giaoVien);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool GiaoVienExists(int id)
        {
            return _context.GiaoVien.Any(e => e.Id == id);
        }
    }
}
