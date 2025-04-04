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
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DeCuongController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }

        public DeCuongController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        public async Task<IActionResult> Index()
        {
            var listDeCuong = await _context.DeCuong.Include(d => d.MonHoc).ToListAsync();
            ViewBag.MonHocId = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc");
            return View(listDeCuong);
        }

        public IActionResult GetDetail(int id)
        {
            var deCuong = _context.DeCuong.Include(d => d.MonHoc).SingleOrDefault(d => d.DeCuongId == id);
            if (deCuong == null)
            {
                return NotFound();
            }
            var data = new { monHoc = deCuong.MonHoc.TenMonHoc, tieuDe = deCuong.TieuDe, noiDung = deCuong.NoiDung };
            return Content(JsonConvert.SerializeObject(data), "application/json");
        }

        // ✅ API kiểm tra môn học đã có đề cương chưa
        [HttpGet]
        public IActionResult CheckExist(int monHocId)
        {
            bool exists = _context.DeCuong.Any(d => d.MonHocId == monHocId);
            return Json(new { exists });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeCuong deCuong)
        {
            if (ModelState.IsValid)
            {
                // ❌ Kiểm tra nếu môn học này đã có đề cương thì không cho thêm
                bool exists = await _context.DeCuong.AnyAsync(d => d.MonHocId == deCuong.MonHocId);
                if (exists)
                {
                    _notyfService.Error("Môn học này đã có đề cương, không thể thêm mới!");
                    return RedirectToAction(nameof(Index));
                }

                _context.Add(deCuong);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm đề cương thành công");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.MonHocId = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", deCuong.MonHocId);
            return View("Index", await _context.DeCuong.Include(d => d.MonHoc).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeCuong deCuong)
        {
            if (id != deCuong.DeCuongId)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // ❌ Kiểm tra nếu môn học này đã có đề cương thì không cho sửa trùng lặp
                    var existingDeCuong = await _context.DeCuong
                        .FirstOrDefaultAsync(d => d.MonHocId == deCuong.MonHocId && d.DeCuongId != id);

                    if (existingDeCuong != null)
                    {
                        _notyfService.Error("Môn học này đã có đề cương, không thể cập nhật!");
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(deCuong);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật đề cương thành công");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeCuongExists(deCuong.DeCuongId))
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
            ViewBag.MonHocId = new SelectList(_context.MonHoc, "MonHocId", "TenMonHoc", deCuong.MonHocId);
            return View("Index", await _context.DeCuong.Include(d => d.MonHoc).ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deCuong = await _context.DeCuong.FindAsync(id);
            if (deCuong != null)
            {
                _context.DeCuong.Remove(deCuong);
                await _context.SaveChangesAsync();
                _notyfService.Success("Xóa đề cương thành công");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DeCuongExists(int id)
        {
            return _context.DeCuong.Any(e => e.DeCuongId == id);
        }
    }
}
