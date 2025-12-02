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
        public async Task<IActionResult> Index(int? maLopHoc = null, int? editId = null)
        {
            const string ACTIVE = "Đang hoạt động";

            // Dropdown lọc: chỉ lớp đang hoạt động
            var lopList = await _context.LopHocs
                .Where(l => l.TrangThai == ACTIVE)
                .OrderBy(x => x.TenLopHoc)
                .Select(x => new { x.MaLopHoc, x.TenLopHoc })
                .ToListAsync();

            var selectedLop = maLopHoc ?? (lopList.Count > 0 ? lopList[0].MaLopHoc : 0);

            ChuongNew detail;
            if (editId.HasValue)
            {
                detail = await _context.ChuongNews
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.MaChuong == editId.Value) ?? new ChuongNew();

                if (detail.MaChuong != 0) selectedLop = detail.MaLopHoc;
                else detail.MaLopHoc = selectedLop;
            }
            else
            {
                detail = new ChuongNew { TenChuong = "", MaChuong = 0, MaLopHoc = selectedLop };
            }

            ViewBag.TenLopHoc  = new SelectList(lopList, "MaLopHoc", "TenLopHoc", selectedLop);
            ViewBag.LopHocList = lopList;

            var vm = new ChuongViewModel
            {
                Detail = detail,
                // Chỉ load chương thuộc lớp đang hoạt động
                ListChuong = await _context.ChuongNews
                    .Include(c => c.LopHoc)
                    .Where(c => c.LopHoc.TrangThai == ACTIVE)
                    .OrderBy(c => c.MaLopHoc).ThenBy(c => c.TenChuong)
                    .ToListAsync()
            };

            return View(vm);
        }

        public IActionResult GetDetail(int id)
        {
            var chuong = _context.ChuongNews
                .Include(c => c.LopHoc)
                    .ThenInclude(l => l.KhoaHoc)
                .Include(c => c.LopHoc)
                    .ThenInclude(l => l.GiangVien)
                .SingleOrDefault(c => c.MaChuong == id);

            if (chuong == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chương." });
            }

            var tongSoBai = _context.Bai?.Count(b => b.MaChuong == id) ?? 0;
            var soLuongDangKy = _context.HocVien_LopHoc?.Count(h => h.MaLopHoc == chuong.MaLopHoc) ?? 0;
            var l = chuong.LopHoc;
             return Json(new
            {
                ok = true,
                data = new
                {
                    maChuong = chuong.MaChuong,
                    tenChuong = chuong.TenChuong,
                    maLopHoc = chuong.MaLopHoc,
                    lop = new
                    {
                        maLopHoc = l?.MaLopHoc,
                        tenLopHoc = l?.TenLopHoc,
                        tenVietTat = l?.TenVietTat,
                        moTa = l?.MoTa,
                        trangThai = l?.TrangThai,
                        anhLopHoc = l?.AnhLopHoc,
                        maKhoaHoc = l?.MaKhoaHoc,
                        tenKhoaHoc = l?.KhoaHoc?.TenKhoaHoc,      // nếu có
                        maGiangVien = l?.MaGiangVien,
                        tenGiangVien = l?.GiangVien?.HoTen, // nếu có
                        soLuongDangKy,
                        tongSoBai
                    }
                }
            });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax(int maLopHoc, string tenChuong)
        {
            tenChuong = (tenChuong ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tenChuong))
                return Json(new { ok = false, message = "Tên chương không được để trống." });

            var exists = await _context.ChuongNews
                .AnyAsync(x => x.MaLopHoc == maLopHoc && x.TenChuong == tenChuong);
            if (exists)
                return Json(new { ok = false, message = "Tên chương đã tồn tại trong lớp này." });

            var c = new ChuongNew { MaLopHoc = maLopHoc, TenChuong = tenChuong };
            _context.ChuongNews.Add(c);
            await _context.SaveChangesAsync();

            var tenLopHoc = await _context.LopHocs
                .Where(l => l.MaLopHoc == maLopHoc)
                .Select(l => l.TenLopHoc)
                .FirstOrDefaultAsync();

            return Json(new
            {
                ok = true,
                id = c.MaChuong,
                tenChuong = c.TenChuong,
                maLopHoc = c.MaLopHoc,
                tenLopHoc,
                message = "Thêm chương thành công."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax(int maChuong, int maLopHoc, string tenChuong)
        {
            tenChuong = (tenChuong ?? "").Trim();
            if (string.IsNullOrWhiteSpace(tenChuong))
                return Json(new { ok = false, message = "Tên chương không được để trống." });

            var exists = await _context.ChuongNews.AnyAsync(cn =>
                cn.MaChuong != maChuong &&
                cn.MaLopHoc == maLopHoc &&
                cn.TenChuong == tenChuong);
            if (exists)
                return Json(new { ok = false, message = "Tên chương đã tồn tại trong lớp này." });

            var c = await _context.ChuongNews.FirstOrDefaultAsync(x => x.MaChuong == maChuong);
            if (c == null)
                return Json(new { ok = false, message = "Không tìm thấy chương." });

            c.TenChuong = tenChuong;
            c.MaLopHoc = maLopHoc;
            await _context.SaveChangesAsync();

            var tenLopHoc = await _context.LopHocs
                .Where(l => l.MaLopHoc == maLopHoc)
                .Select(l => l.TenLopHoc)
                .FirstOrDefaultAsync();

            return Json(new
            {
                ok = true,
                id = c.MaChuong,
                tenChuong = c.TenChuong,
                maLopHoc = c.MaLopHoc,
                tenLopHoc,
                message = "Cập nhật chương thành công."
            });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var c = await _context.ChuongNews.FindAsync(id);
            if (c == null) return Json(new { ok = false, message = "Không tìm thấy chương." });

            _context.ChuongNews.Remove(c);
            await _context.SaveChangesAsync();
            return Json(new { ok = true, message = "Đã xoá chương." });
        }

        private bool ChuongExists(int id)
        {
            return _context.ChuongNews.Any(e => e.MaChuong == id);
        }
    }
}
