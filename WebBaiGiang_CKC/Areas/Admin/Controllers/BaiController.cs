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
    public class BaiController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }
        public BaiController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/Bai
        public async Task<IActionResult> Index()
        {
            var baiGiangContext = _context.Bai.OrderBy(b => b.Chuong.ChuongId).ThenBy(b => b.SoBai).Include(b => b.Chuong);
            return View(await baiGiangContext.ToListAsync());
        }

        // GET: Admin/Bai/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bai == null)
            {
                return NotFound();
            }

            var bai = await _context.Bai
                .Include(b => b.Chuong)
                .FirstOrDefaultAsync(m => m.BaiId == id);
            if (bai == null)
            {
                return NotFound();
            }

            return View(bai);
        }
        public IActionResult GetChuongInfo(int chuongId)
        {
            var chuong = _context.Chuong.FirstOrDefault(c => c.ChuongId == chuongId);
            if (chuong != null)
            {
                var chuongInfo = new { id = chuong.ChuongId, tenChuong = chuong.TenChuong };
                return Json(chuongInfo);
            }
            return Json(new { });
        }
        // GET: Admin/Bai/Create
        public IActionResult Create()
        {
            ViewData["ChuongId"] = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
            return View();
        }

        // POST: Admin/Bai/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BaiId,ChuongId,TenBai,SoBai,MoTa")] Bai bai)
        {
            if (bai.ChuongId == 0)
            {
                _notyfService.Error("Vui lòng chọn chương học!");
            }
            else if (ModelState.IsValid)
            {
                // Kiểm tra xem số bài đã tồn tại hay chưa
                var existingBai = await _context.Bai.FirstOrDefaultAsync(b => b.SoBai == bai.SoBai && b.ChuongId == bai.ChuongId);
                if (existingBai != null)
                {
                    ModelState.AddModelError("SoBai", "Số bài đã tồn tại");
                    ViewData["ChuongId"] = new SelectList(_context.Chuong, "ChuongId", "TenChuong", bai.ChuongId);
                    return View(bai);
                }

                _context.Add(bai);
                _notyfService.Success("Thêm Thành Công");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChuongId"] = new SelectList(_context.Chuong, "ChuongId", "TenChuong", bai.ChuongId);
            return View(bai);
        }

        // GET: Admin/Bai/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bai == null)
            {
                return NotFound();
            }

            var bai = await _context.Bai.FindAsync(id);
            if (bai == null)
            {
                return NotFound();
            }
            ViewData["ChuongId"] = new SelectList(_context.Chuong, "ChuongId", "TenChuong", bai.ChuongId);
            return View(bai);
        }

        // POST: Admin/Bai/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BaiId,ChuongId,TenBai,SoBai,MoTa")] Bai bai)
        {
            if (id != bai.BaiId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra xem số bài mới đã tồn tại hay chưa
                var existingBai = await _context.Bai.FirstOrDefaultAsync(b => b.SoBai == bai.SoBai && b.ChuongId == bai.ChuongId && b.BaiId != bai.BaiId);
                if (existingBai != null)
                {
                    ModelState.AddModelError("SoBai", "Số bài đã tồn tại");
                    ViewData["ChuongId"] = new SelectList(_context.Chuong, "ChuongId", "TenChuong", bai.ChuongId);
                    return View(bai);
                }

                try
                {
                    if (bai.ChuongId == 0)
                    {
                        _notyfService.Error("Vui lòng chọn chương học!");
                    }
                    else
                    {
                        _context.Update(bai);
                        _notyfService.Success("Sửa Thành Công");
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaiExists(bai.BaiId))
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
            ViewData["ChuongId"] = new SelectList(_context.Chuong, "ChuongId", "TenChuong", bai.ChuongId);
            return View(bai);
        }
        // GET: Admin/Bai/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bai == null)
            {
                return NotFound();
            }

            var bai = await _context.Bai
                .Include(b => b.Chuong)
                .FirstOrDefaultAsync(m => m.BaiId == id);
            if (bai == null)
            {
                return NotFound();
            }

            return View(bai);
        }

        // POST: Admin/Bai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bai == null)
            {
                return Problem("Entity set 'BaiGiangContext.Bai'  is null.");
            }
            var bai = await _context.Bai.FindAsync(id);
            if (bai != null)
            {
                _context.Bai.Remove(bai);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaiExists(int id)
        {
            return _context.Bai.Any(e => e.BaiId == id);
        }
    }
}
