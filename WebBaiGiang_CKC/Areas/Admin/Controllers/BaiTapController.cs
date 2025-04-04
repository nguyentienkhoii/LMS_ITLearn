using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BaiTapController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }

        public BaiTapController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/BaiTap
        public async Task<IActionResult> Index()
        {
            var baiTapContext = _context.BaiTap.OrderBy(bt => bt.MonHoc.TenMonHoc).ThenBy(bt => bt.TenBaiTap).Include(bt => bt.MonHoc);
            return View(await baiTapContext.ToListAsync());
        }

        // GET: Admin/BaiTap/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BaiTap == null)
            {
                return NotFound();
            }

            var baiTap = await _context.BaiTap
                .Include(bt => bt.MonHoc)
                .FirstOrDefaultAsync(m => m.BaiTapId == id);
            if (baiTap == null)
            {
                return NotFound();
            }

            return View(baiTap);
        }

        // GET: Admin/BaiTap/Create
        public IActionResult Create()
        {
            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc");
            return View();
        }

        // POST: Admin/BaiTap/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BaiTapId, MonHocId, TenBaiTap, NoiDung")] BaiTap baiTap)
        {
            if (baiTap.MonHocId == 0)
            {
                _notyfService.Error("Vui lòng chọn môn học!");
            }
            else if (ModelState.IsValid)
            {
                var existingBaiTap = await _context.BaiTap
                    .FirstOrDefaultAsync(bt => bt.TenBaiTap == baiTap.TenBaiTap && bt.MonHocId == baiTap.MonHocId);
                if (existingBaiTap != null)
                {
                    ModelState.AddModelError("TenBaiTap", "Tên bài tập đã tồn tại");
                    ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", baiTap.MonHocId);
                    return View(baiTap);
                }

                _context.Add(baiTap);
                _notyfService.Success("Thêm Bài Tập Thành Công");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", baiTap.MonHocId);
            return View(baiTap);
        }

        // GET: Admin/BaiTap/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BaiTap == null)
            {
                return NotFound();
            }

            var baiTap = await _context.BaiTap.FindAsync(id);
            if (baiTap == null)
            {
                return NotFound();
            }
            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", baiTap.MonHocId);
            return View(baiTap);
        }

        // POST: Admin/BaiTap/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BaiTapId, MonHocId, TenBaiTap, NoiDung")] BaiTap baiTap)
        {
            if (id != baiTap.BaiTapId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingBaiTap = await _context.BaiTap
                    .FirstOrDefaultAsync(bt => bt.TenBaiTap == baiTap.TenBaiTap && bt.MonHocId == baiTap.MonHocId && bt.BaiTapId != baiTap.BaiTapId);
                if (existingBaiTap != null)
                {
                    ModelState.AddModelError("TenBaiTap", "Tên bài tập đã tồn tại");
                    ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", baiTap.MonHocId);
                    return View(baiTap);
                }

                try
                {
                    _context.Update(baiTap);
                    _notyfService.Success("Sửa Bài Tập Thành Công");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaiTapExists(baiTap.BaiTapId))
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
            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", baiTap.MonHocId);
            return View(baiTap);
        }

        // GET: Admin/BaiTap/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BaiTap == null)
            {
                return NotFound();
            }

            var baiTap = await _context.BaiTap
                .Include(bt => bt.MonHoc)
                .FirstOrDefaultAsync(m => m.BaiTapId == id);
            if (baiTap == null)
            {
                return NotFound();
            }

            return View(baiTap);
        }

        // POST: Admin/BaiTap/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BaiTap == null)
            {
                return Problem("Entity set 'WebBaiGiangContext.BaiTap' is null.");
            }
            var baiTap = await _context.BaiTap.FindAsync(id);
            if (baiTap != null)
            {
                _context.BaiTap.Remove(baiTap);
            }
            _notyfService.Success("Xóa Bài Tập Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaiTapExists(int id)
        {
            return _context.BaiTap.Any(e => e.BaiTapId == id);
        }
    }
}
