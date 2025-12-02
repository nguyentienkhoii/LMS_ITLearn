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
            // var baiGiangContext = _context.Bai.OrderBy(b => b.Chuong.MaChuong).ThenBy(b => b.SoBai).Include(b => b.Chuong);
            // return View(await baiGiangContext.ToListAsync());
             var list = await _context.Bai
                .Include(b => b.Chuong)
                    .ThenInclude(c => c.LopHoc)
                .OrderBy(b => b.BaiId)
                .ToListAsync();

            // Dropdown Lớp
            ViewBag.LopHocList = await _context.LopHocs
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            // Dropdown Chương (kèm MaLopHoc để lọc theo lớp)
            ViewBag.ChuongList = await _context.ChuongNews
                .OrderBy(c => c.MaLopHoc).ThenBy(c => c.TenChuong)
                .Select(c => new { c.MaChuong, c.TenChuong, c.MaLopHoc })
                .ToListAsync();

            return View(list); 
        }

        // GET: Admin/Bai/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var bai = await _context.Bai
                .Include(b => b.Chuong)
                    .ThenInclude(c => c.LopHoc)
                        .ThenInclude(l => l.KhoaHoc)
                .Include(b => b.Chuong)
                    .ThenInclude(c => c.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                .Include(b => b.Mucs)      // nếu muốn đếm số mục
                .Include(b => b.BaiTaps)   // nếu muốn đếm số bài tập
                .FirstOrDefaultAsync(b => b.BaiId == id);

            if (bai == null) return NotFound();

            ViewBag.CountMuc    = bai.Mucs?.Count    ?? 0;
            ViewBag.CountBaiTap = bai.BaiTaps?.Count ?? 0;
            return View(bai);
        }

        public async Task<IActionResult> GetChuongInfo(int chuongId)
        {
            //var chuong = _context.ChuongNews.FirstOrDefault(c => c.MaChuong == chuongId);
            var chuong = await _context.ChuongNews
                .Include(x => x.LopHoc)
                .FirstOrDefaultAsync(x => x.MaChuong == chuongId);
            if (chuong == null) return NotFound();
                var chuongInfo = new { 
                    id          = chuong.MaChuong,
                    tenChuong   = chuong.TenChuong,
                    maLopHoc    = chuong.MaLopHoc,
                    tenLopHoc   = chuong.LopHoc?.TenLopHoc
                };
                return Json(chuongInfo);
        }
        /////////
        // GET: Admin/Bai/Create
        public async Task<IActionResult> Create(int? maLopHoc = null)
        {
            const string ACTIVE = "Đang hoạt động";

            // Lớp chỉ lấy đang hoạt động
            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == ACTIVE)
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            ViewData["MaLopHoc"] = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", maLopHoc);

            // Nếu đã truyền sẵn maLopHoc, có thể pre-load Chương của lớp đó; nếu không thì để trống
            if (maLopHoc.HasValue)
            {
                var chByLop = await _context.ChuongNews
                    .Where(c => c.MaLopHoc == maLopHoc.Value)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong })
                    .ToListAsync();

                ViewData["MaChuong"] = new SelectList(chByLop, "MaChuong", "TenChuong");
            }
            else
            {
                ViewData["MaChuong"] = new SelectList(Enumerable.Empty<SelectListItem>(), "Value", "Text");
            }

            return View();
        }

        // AJAX: lấy chương theo lớp
        [HttpGet]
        public async Task<IActionResult> GetChuongByLop(int maLopHoc)
        {
            var data = await _context.ChuongNews
                .Where(c => c.MaLopHoc == maLopHoc)
                .OrderBy(c => c.TenChuong)
                .Select(c => new { id = c.MaChuong, text = c.TenChuong })
                .ToListAsync();

            return Json(new { ok = true, data });
        }

        // POST: Admin/Bai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaChuong,TenBai,SoBai,MoTa")] Bai bai)
        {
            if (bai.MaChuong == 0)
            {
                ModelState.AddModelError("MaChuong", "Vui lòng chọn chương học!");
            }

            if (ModelState.IsValid)
            {
                // Unique: SoBai trong cùng 1 chương
                var existed = await _context.Bai.AnyAsync(b =>
                    b.MaChuong == bai.MaChuong && b.SoBai == bai.SoBai);
                if (existed)
                {
                    ModelState.AddModelError("SoBai", "Số bài đã tồn tại trong chương đã chọn.");
                }
                else
                {
                    _context.Bai.Add(bai);
                    await _context.SaveChangesAsync();
                    _notyfService.Success("Thêm thành công");
                    return RedirectToAction(nameof(Index));
                }
            }

            // Nếu fail: nạp lại dropdown Lớp (active) + Chương theo Lớp của Chương đang chọn
            const string ACTIVE = "Đang hoạt động";
            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == ACTIVE)
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();
            ViewData["MaLopHoc"] = new SelectList(lopActive, "MaLopHoc", "TenLopHoc");

            int? maLopOfChuong = null;
            if (bai.MaChuong != 0)
            {
                maLopOfChuong = await _context.ChuongNews
                    .Where(c => c.MaChuong == bai.MaChuong)
                    .Select(c => (int?)c.MaLopHoc)
                    .FirstOrDefaultAsync();
            }
            var chuongs = Enumerable.Empty<object>();
            if (maLopOfChuong.HasValue)
            {
                chuongs = await _context.ChuongNews
                    .Where(c => c.MaLopHoc == maLopOfChuong.Value)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong })
                    .ToListAsync();
            }
            ViewData["MaChuong"] = new SelectList(chuongs, "MaChuong", "TenChuong", bai.MaChuong);

            return View(bai);
        }


        // // GET: Admin/Bai/Edit/5
        // public async Task<IActionResult> Edit(int id)
        // {
        //     var bai = await _context.Bai
        //         .Include(b => b.Chuong)
        //             .ThenInclude(c => c.LopHoc)     // lấy Lớp
        //         .Include(b => b.Chuong)
        //             .ThenInclude(c => c.LopHoc)
        //                 .ThenInclude(l => l.KhoaHoc) // nếu muốn hiện tên khóa học
        //         .Include(b => b.Chuong)
        //             .ThenInclude(c => c.LopHoc)
        //                 .ThenInclude(l => l.GiangVien) // nếu muốn hiện GV
        //         .FirstOrDefaultAsync(b => b.BaiId == id);

        //     if (bai == null) return NotFound();
        //     return View(bai); // KHÔNG dùng SelectList nữa
        // }

        // GET: Admin/Bai/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var bai = await _context.Bai
                .Include(b => b.Chuong)
                    .ThenInclude(c => c.LopHoc)
                        .ThenInclude(l => l.KhoaHoc)
                .Include(b => b.Chuong)
                    .ThenInclude(c => c.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                .FirstOrDefaultAsync(b => b.BaiId == id);
            if (bai == null) return NotFound();

            var currentLopId    = bai.Chuong?.MaLopHoc;
            var currentChuongId = bai.MaChuong;

            // Lớp "Đang hoạt động"
            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == "Đang hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            // (hiếm) nếu lớp hiện tại không active → chèn tạm để nhìn thấy
            if (currentLopId.HasValue && !lopActive.Any(l => l.MaLopHoc == currentLopId.Value))
            {
                var cur = await _context.LopHocs
                    .Where(l => l.MaLopHoc == currentLopId.Value)
                    .Select(l => new { l.MaLopHoc, TenLopHoc = l.TenLopHoc + " (tạm thêm)" })
                    .FirstOrDefaultAsync();
                if (cur != null) lopActive.Insert(0, cur);
            }
            ViewBag.LopList = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", currentLopId);

            // Chương theo lớp hiện tại
            ViewBag.ChuongId = new SelectList(
                _context.ChuongNews
                    .Where(c => currentLopId.HasValue && c.MaLopHoc == currentLopId.Value)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong }),
                "MaChuong", "TenChuong", currentChuongId);

            return View(bai);
        }

        // POST: Admin/Bai/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Edit(int id, [Bind("BaiId,TenBai,SoBai,MoTa")] Bai input)
        // {
        //     if (id != input.BaiId) return NotFound();

        //     var bai = await _context.Bai
        //         .Include(b => b.Chuong) // để hiển thị lại nếu validation fail
        //         .FirstOrDefaultAsync(b => b.BaiId == id);
        //     if (bai == null) return NotFound();

        //     if (!ModelState.IsValid) return View(bai);

        //     // Giữ nguyên chương hiện tại, không cho đổi
        //     var maChuong = bai.MaChuong;

        //     // Unique: SoBai trong cùng MaChuong
        //     var existed = await _context.Bai.AnyAsync(b =>
        //         b.BaiId != id && b.MaChuong == maChuong && b.SoBai == input.SoBai);
        //     if (existed)
        //     {
        //         ModelState.AddModelError("SoBai", "Số bài đã tồn tại trong chương này.");
        //         // đổ lại các giá trị vừa nhập để user thấy
        //         bai.TenBai = input.TenBai;
        //         bai.SoBai  = input.SoBai;
        //         bai.MoTa   = input.MoTa;
        //         return View(bai);
        //     }

        //     // Cập nhật các trường cho phép
        //     bai.TenBai = input.TenBai;
        //     bai.SoBai  = input.SoBai;
        //     bai.MoTa   = input.MoTa;

        //     await _context.SaveChangesAsync();
        //     _notyfService.Success("Sửa thành công");
        //     return RedirectToAction(nameof(Index));
        // }

        // POST: Admin/Bai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BaiId,MaChuong,TenBai,SoBai,MoTa")] Bai input)
        {
            if (id != input.BaiId) return NotFound();

            if (!ModelState.IsValid)
            {
                await RebuildDropdownsForEdit(input.MaChuong);
                // nạp lại entity để hiển thị phần Info card
                var baiReload = await _context.Bai
                    .Include(b => b.Chuong).ThenInclude(c => c.LopHoc)
                    .FirstOrDefaultAsync(b => b.BaiId == id);
                // đổ lại các trường đã nhập
                if (baiReload != null)
                {
                    baiReload.TenBai = input.TenBai;
                    baiReload.SoBai  = input.SoBai;
                    baiReload.MoTa   = input.MoTa;
                    baiReload.MaChuong = input.MaChuong;
                }
                return View(baiReload ?? input);
            }

            // Kiểm tra trùng SoBai trong cùng MaChuong mới
            var existed = await _context.Bai.AnyAsync(b =>
                b.BaiId != id && b.MaChuong == input.MaChuong && b.SoBai == input.SoBai);
            if (existed)
            {
                ModelState.AddModelError("SoBai", "Số bài đã tồn tại trong chương này.");
                await RebuildDropdownsForEdit(input.MaChuong);
                var baiReload = await _context.Bai
                    .Include(b => b.Chuong).ThenInclude(c => c.LopHoc)
                    .FirstOrDefaultAsync(b => b.BaiId == id);
                if (baiReload != null)
                {
                    baiReload.TenBai = input.TenBai;
                    baiReload.SoBai  = input.SoBai;
                    baiReload.MoTa   = input.MoTa;
                    baiReload.MaChuong = input.MaChuong;
                }
                return View(baiReload ?? input);
            }

            // Cập nhật
            var bai = await _context.Bai.FirstOrDefaultAsync(b => b.BaiId == id);
            if (bai == null) return NotFound();

            bai.MaChuong = input.MaChuong;   // CHO PHÉP chuyển chương
            bai.TenBai   = input.TenBai;
            bai.SoBai    = input.SoBai;
            bai.MoTa     = input.MoTa;

            await _context.SaveChangesAsync();
            _notyfService.Success("Sửa thành công");
            return RedirectToAction(nameof(Index));
        }

        // build lại dropdown khi fail validation
        private async Task RebuildDropdownsForEdit(int maChuong)
        {
            var chuong = await _context.ChuongNews
                .Include(c => c.LopHoc)
                .FirstOrDefaultAsync(c => c.MaChuong == maChuong);

            int? lopId = chuong?.MaLopHoc;

            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == "Đang hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            if (lopId.HasValue && !lopActive.Any(l => l.MaLopHoc == lopId.Value))
            {
                var cur = await _context.LopHocs.Where(l => l.MaLopHoc == lopId.Value)
                    .Select(l => new { l.MaLopHoc, TenLopHoc = l.TenLopHoc + " (tạm thêm)" })
                    .FirstOrDefaultAsync();
                if (cur != null) lopActive.Insert(0, cur);
            }
            ViewBag.LopList = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", lopId);

            ViewBag.ChuongId = new SelectList(
                _context.ChuongNews
                    .Where(c => lopId.HasValue && c.MaLopHoc == lopId.Value)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong }),
                "MaChuong", "TenChuong", maChuong);
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
