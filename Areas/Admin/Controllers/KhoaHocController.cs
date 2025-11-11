using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhoaHocController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;

        public KhoaHocController(WebBaiGiangContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // 📜 Danh sách khóa học
        public async Task<IActionResult> Index()
        {
            var list = await _context.KhoaHocs.OrderByDescending(k => k.MaKhoaHoc).ToListAsync();
            return View(list);
        }

        // ➕ GET: Thêm khóa học
        public IActionResult Create() => View();

        // ➕ POST: Thêm khóa học
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhoaHoc model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                _notyf.Success("Thêm khóa học thành công!");
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ✏️ GET: Sửa
        public async Task<IActionResult> Edit(int id)
        {
            var khoa = await _context.KhoaHocs.FindAsync(id);
            if (khoa == null) return NotFound();
            return View(khoa);
        }

        // ✏️ POST: Sửa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KhoaHoc model)
        {
            if (id != model.MaKhoaHoc) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    _notyf.Success("Cập nhật khóa học thành công!");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.KhoaHocs.Any(e => e.MaKhoaHoc == id))
                        return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // ❌ GET: Xóa
        public async Task<IActionResult> Delete(int id)
        {
            var khoa = await _context.KhoaHocs.FirstOrDefaultAsync(k => k.MaKhoaHoc == id);
            if (khoa == null) return NotFound();
            return View(khoa);
        }

        // ❌ POST: Xóa xác nhận
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var khoa = await _context.KhoaHocs.FindAsync(id);
            if (khoa != null)
            {
                _context.KhoaHocs.Remove(khoa);
                await _context.SaveChangesAsync();
                _notyf.Success("Xóa khóa học thành công!");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
