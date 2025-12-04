using AspNetCoreHero.ToastNotification.Abstractions;
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
    public class BaiTapController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly INotyfService _notyf;

        public BaiTapController(WebBaiGiangContext context, INotyfService notyf)
        {
            _context = context;
            _notyf = notyf;
        }

        // ========== INDEX ==========
       public IActionResult Index()
        {
            // Lấy danh sách lớp để build dropdown lọc
            ViewBag.LopList = _context.LopHocs
                .OrderBy(x => x.TenLopHoc)
                .ToList();

            var list = _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .OrderByDescending(b => b.MaBaiTap)
                .ToList();

            return View(list);
        }


        // ========== DETAILS ==========
        public IActionResult Details(int id)
        {
            var baitap = _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(x => x.Chuong)
                .FirstOrDefault(x => x.MaBaiTap == id);

            if (baitap == null)
            {
                _notyf.Error("Không tìm thấy bài tập!");
                return RedirectToAction("Index");
            }

            return View(baitap);
        }

        // ========== CREATE ==========
        public IActionResult Create()
        {
            ViewBag.BaiList = new SelectList(_context.Bai
                .Include(x => x.Chuong)
                .Select(b => new
                {
                    b.BaiId,
                    Ten = b.TenBai + " - (" + b.Chuong.TenChuong + ")"
                }),
                "BaiId", "Ten");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BaiTap model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BaiList = new SelectList(_context.Bai, "BaiId", "TenBai");
                return View(model);
            }

            _context.BaiTaps.Add(model);
            _context.SaveChanges();

            _notyf.Success("Thêm bài tập thành công!");
            return RedirectToAction("Index");
        }

        // ========== EDIT ==========
        public IActionResult Edit(int id)
        {
            var baitap = _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(c => c.Chuong)
                .FirstOrDefault(x => x.MaBaiTap == id);

            if (baitap == null)
            {
                _notyf.Error("Không tìm thấy bài tập!");
                return RedirectToAction("Index");
            }

            ViewBag.BaiList = new SelectList(_context.Bai
                .Include(x => x.Chuong)
                .Select(b => new
                {
                    b.BaiId,
                    Ten = b.TenBai + " - (" + b.Chuong.TenChuong + ")"
                }),
                "BaiId", "Ten", baitap.BaiId);

            return View(baitap);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BaiTap model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BaiList = new SelectList(_context.Bai, "BaiId", "TenBai", model.BaiId);
                return View(model);
            }

            _context.BaiTaps.Update(model);
            _context.SaveChanges();

            _notyf.Success("Cập nhật bài tập thành công!");
            return RedirectToAction("Index");
        }

        // ========== DELETE ==========
        public IActionResult Delete(int id)
        {
            var baitap = _context.BaiTaps
                .Include(b => b.Bai)
                .FirstOrDefault(x => x.MaBaiTap == id);

            if (baitap == null)
            {
                _notyf.Error("Không tìm thấy bài tập!");
                return RedirectToAction("Index");
            }

            return View(baitap);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var baitap = _context.BaiTaps.FirstOrDefault(x => x.MaBaiTap == id);

            if (baitap == null)
            {
                _notyf.Error("Không tìm thấy bài tập!");
                return RedirectToAction("Index");
            }

            _context.BaiTaps.Remove(baitap);
            _context.SaveChanges();

            _notyf.Success("Xóa bài tập thành công!");
            return RedirectToAction("Index");
        }

        public IActionResult DanhSachBaiNop(int id, int page = 1)
        {
            var baitap = _context.BaiTaps
                .Include(b => b.Bai)
                    .ThenInclude(c => c.Chuong)
                .FirstOrDefault(x => x.MaBaiTap == id);

            if (baitap == null)
            {
                _notyf.Error("Không tìm thấy bài tập!");
                return RedirectToAction("Index");
            }

            ViewBag.BaiTap = baitap;
            ViewBag.TenBai = baitap.TenBaiTap;

            int pageSize = 15;

            var list = _context.BaiTapNops
                .Where(x => x.MaBaiTap == id)
                .Include(x => x.HocVien)
                    .ThenInclude(h => h.TaiKhoan)
                .OrderByDescending(x => x.NgayNop)
                .ToPagedList(page, pageSize);

            return View(list);
        }

    }
}
