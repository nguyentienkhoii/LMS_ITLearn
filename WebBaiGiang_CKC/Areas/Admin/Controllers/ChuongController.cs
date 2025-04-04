using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.Wordprocessing;
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

        // GET: Admin/Chuong
        public async Task<IActionResult> Index(int? id)
        {
            if (id == null)
            {
                ViewBag.MonHocId = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc");
                var chuong = new Chuong()
                {
                    TenChuong = "",
                    ChuongId = 0,
                    MonHocId = 1,
                };
                var viewModel = new ChuongViewModel
                {
                    ListChuong = await _context.Chuong.Include(c => c.MonHoc).ToListAsync(),
                    Detail = chuong
                };


                return View(viewModel);
            }
            else
            {
                ViewBag.MonHocId = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc");
                List<Chuong> DsChuong = await _context.Chuong.Include(c => c.MonHoc).ToListAsync();
                var chuong = new Chuong()
                {
                    TenChuong = DsChuong.FirstOrDefault(c => c.ChuongId == id).TenChuong,
                    ChuongId = DsChuong.FirstOrDefault(c => c.ChuongId == id).ChuongId,
                    MonHocId = 1,
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
            var chuong = _context.Chuong
                .Include(c => c.MonHoc)
                .SingleOrDefault(c => c.ChuongId == id);

            if (chuong == null)
            {
                return NotFound();
            }

            var data = new
            {
                monHoc = new { tenMonHoc = chuong.MonHoc.TenMonHoc },
                tenChuong = chuong.TenChuong,
                chuongId = chuong.ChuongId
            };

            try
            {
                var jsonData = JsonConvert.SerializeObject(data);
                return Content(jsonData, "application/json");
            }
            catch (Exception)
            {
                // log lỗi serialize JSON vào đây
                return BadRequest();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChuongViewModel chuong)
        {
            if (ModelState.IsValid)
            {
                var existingChuong = await _context.Chuong.FirstOrDefaultAsync(c => c.ChuongId == chuong.Detail.ChuongId || c.TenChuong.Trim() == chuong.Detail.TenChuong.Trim() && c.MonHocId == chuong.Detail.MonHocId);

                if (existingChuong != null)
                {
                    if (existingChuong.ChuongId == chuong.Detail.ChuongId)
                    {
                        ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", chuong.Detail.MonHocId);

                        var model = new ChuongViewModel
                        {
                            Detail = chuong.Detail,
                            ListChuong = await _context.Chuong.ToListAsync()
                        };
                        _notyfService.Error("Số chương đã tồn tại");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {


                        ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", chuong.Detail.MonHocId);

                        var model = new ChuongViewModel
                        {
                            Detail = chuong.Detail,
                            ListChuong = await _context.Chuong.ToListAsync()
                        }; _notyfService.Error("Tên chương đã tồn tại");
                      
                        return RedirectToAction(nameof(Index));
                    }
                }
                _context.Add(chuong.Detail);
                _notyfService.Success("Thêm Thành Công");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", chuong.Detail.MonHocId);

            var viewModel = new ChuongViewModel
            {
                Detail = chuong.Detail,
                ListChuong = await _context.Chuong.ToListAsync() // Cập nhật lại danh sách chương để hiển thị trên view
            };

            return View("Index", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChuongViewModel chuong)
        {
            if (id != chuong.Detail.ChuongId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingChuong = await _context.Chuong.FirstOrDefaultAsync(c => c.TenChuong.Trim() == chuong.Detail.TenChuong.Trim() && c.MonHocId == chuong.Detail.MonHocId);

                    if (existingChuong != null)
                    {
                       

                            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", chuong.Detail.MonHocId);

                            var model = new ChuongViewModel
                            {
                                Detail = chuong.Detail,
                                ListChuong = await _context.Chuong.ToListAsync()
                            }; _notyfService.Error("Tên chương đã tồn tại");

                            return RedirectToAction(nameof(Index));
                        
                    }
                    _context.Update(chuong.Detail);
                    _notyfService.Success("Cập Nhật Thành Công");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuongExists(chuong.Detail.ChuongId))
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
            ViewData["MonHocId"] = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", chuong.Detail.MonHocId);

            var viewModel = new ChuongViewModel
            {
                Detail = chuong.Detail,
                ListChuong = await _context.Chuong.ToListAsync() // Cập nhật lại danh sách chương để hiển thị trên view
            };

            return View("Index", viewModel);
        }

        // GET: Admin/Chuong/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.Chuong == null)
        //    {
        //        return NotFound();
        //    }

        //    var chuong = await _context.Chuong
        //        .Include(c => c.MonHoc)
        //        .FirstOrDefaultAsync(m => m.ChuongId == id);
        //    if (chuong == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(chuong);
        //}

        // POST: Admin/Chuong/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (_context.Chuong == null)
            {
                return Problem("Entity set 'WebBaiGiangContext.Chuong'  is null.");
            }
            var chuong = await _context.Chuong.FindAsync(id);
            if (chuong != null)
            {
                _context.Chuong.Remove(chuong);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChuongExists(int id)
        {
            return _context.Chuong.Any(e => e.ChuongId == id);
        }
    }
}
