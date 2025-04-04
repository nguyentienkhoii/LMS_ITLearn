using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using X.PagedList;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MucController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }
        public MucController(WebBaiGiangContext context, IConfiguration configuration, INotyfService notyfService)
        {
            _context = context;

            _notyfService = notyfService;
        }

        // GET: Admin/Muc
        public IActionResult Index(int? page)
        {

            var baiGiangContext = _context.Muc.OrderBy(x => x.Bai.Chuong.ChuongId).ThenBy(c => c.Bai.SoBai).ThenBy(m => m.MucSo).Include(m => m.Bai).ThenInclude(x => x.Chuong).AsNoTracking();
            var pageNo = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 12;
            PagedList<Muc> models = new PagedList<Muc>(baiGiangContext, pageNo, pageSize);
            return View(models);
        }

        // GET: Admin/Muc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Muc == null)
            {
                return NotFound();
            }

            var muc = await _context.Muc
                .Include(m => m.Bai).ThenInclude(c => c.Chuong)
                .FirstOrDefaultAsync(m => m.MucId == id);
            if (muc == null)
            {
                return NotFound();
            }

            return View(muc);
        }

        public IActionResult GetBaiByChuongId(int chuongId)
        {
            var danhSachBai = _context.Bai.Where(b => b.ChuongId == chuongId).ToList();
            return Json(danhSachBai);
        }

        // GET: Admin/Muc/Create
        public IActionResult Create()
        {
            var chuongid = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
            ViewData["ChuongId"] = chuongid;
            var firstChuongId = _context.Chuong.FirstOrDefault()?.ChuongId ?? 0;
            if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
            {
                int chuongId = int.Parse(Request.Query["ChuongId"]);
                var danhSachBai = _context.Bai.Where(b => b.ChuongId == chuongId).ToList();
                var baiid = new SelectList(danhSachBai, "BaiId", "TenBai");
                ViewData["BaiId"] = baiid;
            }
            else
            {
                ViewData["BaiId"] = new SelectList(string.Empty, "Value", "Text");
            }

            return View();
        }
        // POST: Admin/Muc/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MucId,TenMuc,BaiId,MucSo,NoiDung")] Muc muc)
        {
            if (muc.BaiId == 0)
            {
                _notyfService.Error("Vui lòng chọn bài học");
                var chuongid = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
                ViewData["ChuongId"] = chuongid;
                var firstChuongId = _context.Chuong.FirstOrDefault()?.ChuongId ?? 0;
                if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
                {

                    int chuongId = int.Parse(Request.Query["ChuongId"]);
                    var danhSachBai = _context.Bai.Where(b => b.ChuongId == chuongId).ToList();
                    var baiid = new SelectList(danhSachBai, "BaiId", "TenBai");
                    ViewData["BaiId"] = baiid;
                }
                return View(muc);
            }
            if (ModelState.IsValid)
            {
                // Kiểm tra xem số mục đã tồn tại hay chưa
                var existingMuc = await _context.Muc.FirstOrDefaultAsync(m => m.MucSo == muc.MucSo && m.BaiId == muc.BaiId);
                if (existingMuc != null)
                {
                    ModelState.AddModelError("MucSo", "Số mục đã tồn tại");
                    var chuongid = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
                    ViewData["ChuongId"] = chuongid;
                    var firstChuongId = _context.Chuong.FirstOrDefault()?.ChuongId ?? 0;
                    if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
                    {

                        int chuongId = int.Parse(Request.Query["ChuongId"]);
                        var danhSachBai = _context.Bai.Where(b => b.ChuongId == chuongId).ToList();
                        var baiid = new SelectList(danhSachBai, "BaiId", "TenBai");
                        ViewData["BaiId"] = baiid;
                    }
                    else
                    {

                        ViewData["BaiId"] = new SelectList(string.Empty, "Value", "Text");
                    }
                    return View(muc);
                }
                _context.Add(muc);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm thành công");
                return RedirectToAction(nameof(Index));
            }

           

            return View(muc);
        }

        // GET: Admin/Muc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var muc = await _context.Muc.FindAsync(id);
            if (muc == null)
            {
                return NotFound();
            }

            var chuongid = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
            ViewData["ChuongId"] = chuongid;
            var firstChuongId = _context.Chuong.FirstOrDefault()?.ChuongId ?? 0;
            if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
            {

                int chuongId = int.Parse(Request.Query["ChuongId"]);
                var danhSachBai = _context.Bai.Where(b => b.BaiId == id && b.ChuongId == chuongId).ToList();

            }

            var baiid = new SelectList(_context.Bai, "BaiId", "TenBai");
            ViewData["BaiId"] = baiid;
            return View(muc);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MucId,TenMuc,BaiId,MucSo,NoiDung")] Muc muc)
        {
            if (id != muc.MucId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Kiểm tra xem số mục đã tồn tại hay chưa
                var existingMuc = await _context.Muc.FirstOrDefaultAsync(m => m.MucSo == muc.MucSo && m.BaiId == muc.BaiId && m.MucId != muc.MucId);
                if (existingMuc != null)
                {
                    ModelState.AddModelError("MucSo", "Số mục đã tồn tại");
                    var chuongid = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
                    ViewData["ChuongId"] = chuongid;
                    var firstChuongId = _context.Chuong.FirstOrDefault()?.ChuongId ?? 0;
                    if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
                    {

                        int chuongId = int.Parse(Request.Query["ChuongId"]);
                        var danhSachBai = _context.Bai.Where(b => b.ChuongId == chuongId).ToList();

                    }
                    var baiid = new SelectList(_context.Bai, "BaiId", "TenBai");
                    ViewData["BaiId"] = baiid;
                    return View(muc);
                }
                try
                {
                    if (muc.BaiId == 0)
                    {
                        _notyfService.Error("Vui lòng chọn bài học");
                        var chuongid = new SelectList(_context.Chuong, "ChuongId", "TenChuong");
                        ViewData["ChuongId"] = chuongid;
                        var firstChuongId = _context.Chuong.FirstOrDefault()?.ChuongId ?? 0;
                        if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
                        {

                            int chuongId = int.Parse(Request.Query["ChuongId"]);
                            var danhSachBai = _context.Bai.Where(b => b.ChuongId == chuongId).ToList();

                        }
                        var baiid = new SelectList(_context.Bai, "BaiId", "TenBai");
                        ViewData["BaiId"] = baiid;
                        return View(muc);
                    }
                    _context.Update(muc);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MucExists(muc.MucId))
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
           
            return View(muc);
        }
        // GET: Admin/Muc/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Muc == null)
            {
                return NotFound();
            }

            var muc = await _context.Muc
                .Include(m => m.Bai).ThenInclude(c => c.Chuong)
                .FirstOrDefaultAsync(m => m.MucId == id);
            if (muc == null)
            {
                return NotFound();
            }

            return View(muc);
        }

        // POST: Admin/Muc/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Muc == null)
            {
                return Problem("Entity set 'BaiGiangContext.Muc'  is null.");
            }
            var muc = await _context.Muc.FindAsync(id);
            if (muc != null)
            {
                _context.Muc.Remove(muc);
            }

            await _context.SaveChangesAsync();
            _notyfService.Success("Xóa thành công");
            return RedirectToAction(nameof(Index));
        }

        private bool MucExists(int id)
        {
            return _context.Muc.Any(e => e.MucId == id);
        }
    }
}
