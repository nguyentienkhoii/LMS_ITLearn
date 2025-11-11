using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebBaiGiang_CKC.Areas.Admin.Data;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ChuongController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }

        public ChuongController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/ChuongNew
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                ViewBag.MaLopHoc = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc");
                var chuong = new ChuongNew()
                {
                    TenChuong = "",
                    MaChuong = 0,
                    MaLopHoc = 1,
                };

                var viewModel = new ChuongViewModel
                {
                    ListChuong = await _context.ChuongNews.Include(c => c.LopHoc).ToListAsync(),
                    Detail = chuong
                };

                return View(viewModel);
            }
            else
            {
                ViewBag.LopHocId = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc");
                List<ChuongNew> DsChuong = await _context.ChuongNews.Include(c => c.LopHoc).ToListAsync();

                var chuong = new ChuongNew()
                {
                    TenChuong = DsChuong.FirstOrDefault(c => c.MaChuong == id)?.TenChuong ?? "",
                    MaChuong = DsChuong.FirstOrDefault(c => c.MaChuong == id)?.MaChuong ?? 0,
                    MaLopHoc = 1,
                };
                var viewModel = new ChuongViewModel
                {
                    ListChuong = DsChuong,
                    Detail = chuong
                };

                return View(viewModel);
            }
        }

        public IActionResult GetDetail(int id)
        {
            var chuong = _context.ChuongNews
                .Include(c => c.LopHoc)
                .SingleOrDefault(c => c.MaChuong == id);

            if (chuong == null)
            {
                return NotFound();
            }

            var data = new
            {
                lopHoc = new { tenLopHoc = chuong.LopHoc.TenLopHoc },
                tenChuong = chuong.TenChuong,
                chuongId = chuong.MaChuong
            };

            try
            {
                var jsonData = JsonConvert.SerializeObject(data);
                return Content(jsonData, "application/json");
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChuongViewModel chuong)
        {
            if (ModelState.IsValid)
            {
                var existingChuong = await _context.ChuongNews
                    .FirstOrDefaultAsync(c =>
                        c.MaChuong == chuong.Detail.MaChuong ||
                        (c.TenChuong.Trim() == chuong.Detail.TenChuong.Trim() && c.MaLopHoc == chuong.Detail.MaLopHoc)
                    );

                if (existingChuong != null)
                {
                    if (existingChuong.MaChuong == chuong.Detail.MaChuong)
                    {
                        ViewData["LopHocId"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc", chuong.Detail.MaLopHoc);
                        _notyfService.Error("Số chương đã tồn tại");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ViewData["LopHocId"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc", chuong.Detail.MaLopHoc);
                        _notyfService.Error("Tên chương đã tồn tại");
                        return RedirectToAction(nameof(Index));
                    }
                }

                _context.Add(chuong.Detail);
                _notyfService.Success("Thêm thành công");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["LopHocId"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc", chuong.Detail.MaLopHoc);
            var viewModel = new ChuongViewModel
            {
                Detail = chuong.Detail,
                ListChuong = await _context.ChuongNews.ToListAsync()
            };
            return View("Index", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChuongViewModel chuong)
        {
            if (id != chuong.Detail.MaChuong)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingChuong = await _context.ChuongNews
                        .FirstOrDefaultAsync(c => c.TenChuong.Trim() == chuong.Detail.TenChuong.Trim() && c.MaLopHoc == chuong.Detail.MaLopHoc);

                    if (existingChuong != null)
                    {
                        ViewData["LopHocId"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc", chuong.Detail.MaLopHoc);
                        _notyfService.Error("Tên chương đã tồn tại");
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(chuong.Detail);
                    _notyfService.Success("Cập nhật thành công");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuongExists(chuong.Detail.MaChuong))
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

            ViewData["LopHocId"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc", chuong.Detail.MaLopHoc);
            var viewModel = new ChuongViewModel
            {
                Detail = chuong.Detail,
                ListChuong = await _context.ChuongNews.ToListAsync()
            };
            return View("Index", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.ChuongNews == null)
            {
                return Problem("Entity set 'WebBaiGiangContext.ChuongNew' is null.");
            }

            var chuong = await _context.ChuongNews.FindAsync(id);
            if (chuong != null)
            {
                _context.ChuongNews.Remove(chuong);
            }
            _notyfService.Success("Xóa thành công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuongExists(int id)
        {
            return _context.ChuongNews.Any(e => e.MaChuong == id);
        }
    }
}
