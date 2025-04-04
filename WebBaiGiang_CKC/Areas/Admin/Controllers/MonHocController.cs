using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Helper;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class MonHocController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }
        public MonHocController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        /*// GET: Admin/MonHoc
        public async Task<IActionResult> Index()
        {
            return View(await _context.MonHoc.ToListAsync());
        }*/
        //new
        // GET: Admin/MonHoc
        public async Task<IActionResult> Index()
        {
            var monHocs = await _context.MonHoc
                .Include(m => m.GiaoVien) // Thêm để load thông tin giáo viên
                .ToListAsync();
            return View(monHocs);
        }

        // GET: Admin/MonHoc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MonHoc == null)
            {
                return NotFound();
            }

            var monHoc = await _context.MonHoc
                .FirstOrDefaultAsync(m => m.MonHocId == id);
            if (monHoc == null)
            {
                return NotFound();
            }

            return View(monHoc);
        }

        /*// GET: Admin/MonHoc/Create
        public IActionResult Create()
        {
            return View();
        }*/

        //new
        // GET: Admin/MonHoc/Create
        public IActionResult Create()
        {
            // Lấy danh sách giáo viên để hiển thị trong dropdown
            ViewBag.GiaoVienList = _context.GiaoVien
                .Where(g => g.TrangThai == true)
                .OrderBy(g => g.HoTen)
                .ToList();
            return View();
        }



        /*// POST: Admin/MonHoc/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MonHocId,TenMonHoc,MaMonHoc,MoTa")] MonHoc monHoc, IFormFile fAvatar)
        {
            if (ModelState.IsValid)
            {
                // Xử lý tên môn học (ví dụ: chuyển chữ đầu thành chữ hoa)
                monHoc.TenMonHoc = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(monHoc.TenMonHoc);

                // Xử lý ảnh đại diện nếu có
                if (fAvatar != null)
                {
                    string extension = Path.GetExtension(fAvatar.FileName);
                    string image = Utilities.ToUrlFriendly(monHoc.MaMonHoc) + extension;
                    monHoc.AnhDaiDien = await Utilities.UploadFile(fAvatar, @"MonHoc", image.ToLower());
                }

                // Thêm vào cơ sở dữ liệu
                _context.Add(monHoc);
                await _context.SaveChangesAsync();

                _notyfService.Success("Thêm thành công");
                return RedirectToAction(nameof(Index));
            }
            return View(monHoc);
        }
*/

        //new
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MonHocId,TenMonHoc,MaMonHoc,MoTa,GiaoVienId")] MonHoc monHoc, IFormFile fAvatar)
        {
            if (ModelState.IsValid)
            {
                monHoc.TenMonHoc = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(monHoc.TenMonHoc);

                if (fAvatar != null)
                {
                    string extension = Path.GetExtension(fAvatar.FileName);
                    string image = Utilities.ToUrlFriendly(monHoc.MaMonHoc) + extension;
                    monHoc.AnhDaiDien = await Utilities.UploadFile(fAvatar, @"MonHoc", image.ToLower());
                }

                _context.Add(monHoc);
                await _context.SaveChangesAsync();

                _notyfService.Success("Thêm thành công");
                return RedirectToAction(nameof(Index));
            }

            // Nếu có lỗi, load lại danh sách giáo viên
            ViewBag.GiaoVienList = _context.GiaoVien
                .Where(g => g.TrangThai == true)
                .OrderBy(g => g.HoTen)
                .ToList();

            return View(monHoc);
        }

        /*// GET: Admin/MonHoc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MonHoc == null)
            {
                return NotFound();
            }

            var monHoc = await _context.MonHoc.FindAsync(id);
            if (monHoc == null)
            {
                return NotFound();
            }
            return View(monHoc);
        }*/

        //new
        // GET: Admin/MonHoc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MonHoc == null)
            {
                return NotFound();
            }

            var monHoc = await _context.MonHoc.FindAsync(id);
            if (monHoc == null)
            {
                return NotFound();
            }

            ViewBag.GiaoVienList = _context.GiaoVien
                .Where(g => g.TrangThai == true)
                .OrderBy(g => g.HoTen)
                .ToList();

            return View(monHoc);
        }


        // POST: Admin/MonHoc/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MonHocId,TenMonHoc,MaMonHoc,MoTa")] MonHoc monHoc)
        {
            if (id != monHoc.MonHocId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(monHoc);
                    _notyfService.Success("Sửa Thành Công");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MonHocExists(monHoc.MonHocId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(monHoc);
        }*/

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MonHocId,TenMonHoc,MaMonHoc,MoTa,AnhDaiDien")] MonHoc monHoc, IFormFile fAvatar)
        {
            if (id != monHoc.MonHocId)
            {
                return NotFound();
            }

            try
            {
                var monHocFromDb = await _context.MonHoc.FindAsync(monHoc.MonHocId);
                if (monHocFromDb == null)
                {
                    return NotFound();
                }

                // Lưu đường dẫn ảnh cũ
                string oldImagePath = monHocFromDb.AnhDaiDien;

                if (fAvatar != null)
                {
                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        string oldImageFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contents", "Images", "MonHoc", oldImagePath);
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }

                    // Upload ảnh mới
                    string extension = Path.GetExtension(fAvatar.FileName);
                    string imageName = $"{Utilities.ToUrlFriendly(monHoc.MaMonHoc)}_{DateTime.Now.Ticks}{extension}";
                    monHocFromDb.AnhDaiDien = await Utilities.UploadFile(fAvatar, @"MonHoc", imageName.ToLower());
                }

                // Cập nhật các thông tin khác
                monHocFromDb.TenMonHoc = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(monHoc.TenMonHoc);
                monHocFromDb.MaMonHoc = monHoc.MaMonHoc;
                monHocFromDb.MoTa = monHoc.MoTa;

                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyfService.Error($"Lỗi: {ex.Message}");
                return View(monHoc);
            }
        }*/


        //new edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MonHocId,TenMonHoc,MaMonHoc,MoTa,AnhDaiDien,GiaoVienId")] MonHoc monHoc, IFormFile fAvatar)
        {
            if (id != monHoc.MonHocId)
            {
                return NotFound();
            }

            try
            {
                var monHocFromDb = await _context.MonHoc.FindAsync(monHoc.MonHocId);
                if (monHocFromDb == null)
                {
                    return NotFound();
                }

                string oldImagePath = monHocFromDb.AnhDaiDien;

                if (fAvatar != null)
                {
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        string oldImageFullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "contents", "Images", "MonHoc", oldImagePath);
                        if (System.IO.File.Exists(oldImageFullPath))
                        {
                            System.IO.File.Delete(oldImageFullPath);
                        }
                    }

                    string extension = Path.GetExtension(fAvatar.FileName);
                    string imageName = $"{Utilities.ToUrlFriendly(monHoc.MaMonHoc)}_{DateTime.Now.Ticks}{extension}";
                    monHocFromDb.AnhDaiDien = await Utilities.UploadFile(fAvatar, @"MonHoc", imageName.ToLower());
                }

                // Cập nhật thông tin bao gồm cả GiaoVienId
                monHocFromDb.TenMonHoc = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(monHoc.TenMonHoc);
                monHocFromDb.MaMonHoc = monHoc.MaMonHoc;
                monHocFromDb.MoTa = monHoc.MoTa;
                monHocFromDb.GiaoVienId = monHoc.GiaoVienId;

                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công!");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _notyfService.Error($"Lỗi: {ex.Message}");

                // Load lại danh sách giáo viên nếu có lỗi
                ViewBag.GiaoVienList = _context.GiaoVien
                    .Where(g => g.TrangThai == true)
                    .OrderBy(g => g.HoTen)
                    .ToList();

                return View(monHoc);
            }
        }

        // GET: Admin/MonHoc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MonHoc == null)
            {
                return NotFound();
            }

            var monHoc = await _context.MonHoc
                .FirstOrDefaultAsync(m => m.MonHocId == id);
            if (monHoc == null)
            {
                return NotFound();
            }

            return View(monHoc);
        }

        // POST: Admin/MonHoc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MonHoc == null)
            {
                return Problem("Entity set 'BaiGiangContext.MonHoc'  is null.");
            }
            var monHoc = await _context.MonHoc.FindAsync(id);
            if (monHoc != null)
            {
                _context.MonHoc.Remove(monHoc);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MonHocExists(int id)
        {
            return _context.MonHoc.Any(e => e.MonHocId == id);
        }
    }
}
