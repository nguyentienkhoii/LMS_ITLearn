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
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MucController(WebBaiGiangContext context, IConfiguration configuration, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;

            _notyfService = notyfService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Muc
        // public IActionResult Index(int? page, int? chuongId)
        // {

        //     ViewBag.ChuongId = chuongId;   // giữ trạng thái dropdown

        //     // Lấy danh sách Chương
        //     ViewBag.ChuongList = _context.ChuongNews
        //         .OrderBy(x => x.MaChuong)
        //         .ToList();

        //     // Query gốc
        //     var query = _context.Muc
        //         .Include(m => m.Bai).ThenInclude(b => b.Chuong)
        //         .AsQueryable();

        //     // Nếu chọn chương
        //     if (chuongId != null && chuongId > 0)
        //     {
        //         query = query.Where(x => x.Bai.MaChuong == chuongId);
        //     }

        //     // Sắp xếp + phân trang
        //     query = query.OrderBy(x => x.Bai.Chuong.MaChuong)
        //                 .ThenBy(x => x.Bai.SoBai)
        //                 .ThenBy(x => x.MucSo);

        //     var pageNo = page ?? 1;
        //     int pageSize = 12;

        //     var models = new PagedList<Muc>(query, pageNo, pageSize);
        //     return View(models);

            // var baiGiangContext = _context.Muc.OrderBy(x => x.Bai.Chuong.MaChuong).ThenBy(c => c.Bai.SoBai).ThenBy(m => m.MucSo).Include(m => m.Bai).ThenInclude(x => x.Chuong).AsNoTracking();
            // var pageNo = page == null || page <= 0 ? 1 : page.Value;
            // var pageSize = 12;
            // PagedList<Muc> models = new PagedList<Muc>(baiGiangContext, pageNo, pageSize);
            // return View(models);
        //}

        // public IActionResult Index()
        // {
        //     ViewBag.LopHocList = _context.LopHocs
        //         .OrderBy(x => x.TenLopHoc)
        //         .ToList();

        //     var query = _context.Muc
        //         .Include(m => m.Bai).ThenInclude(b => b.Chuong)
        //         .OrderBy(m => m.Bai.Chuong.MaChuong)
        //         .ThenBy(m => m.Bai.SoBai)
        //         .ThenBy(m => m.MucSo);

        //     var models = query.ToPagedList(1, 12);
        //     return View(models);
        // }

        // GET: Admin/Muc
        public async Task<IActionResult> Index()
        {
            var list = await _context.Muc
                .Include(m => m.Bai)
                    .ThenInclude(b => b.Chuong)
                        .ThenInclude(c => c.LopHoc)
                .OrderBy(m => m.Bai.Chuong.MaLopHoc)
                .ThenBy(m => m.Bai.MaChuong)
                .ThenBy(m => m.Bai.SoBai)
                .ThenBy(m => m.MucSo)
                .ToListAsync();

            // Lớp chỉ lấy đang hoạt động
            ViewBag.LopHocList = await _context.LopHocs
                .Where(l => l.TrangThai == "Hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            // Chương (kèm MaLopHoc để cascade)
            ViewBag.ChuongList = await _context.ChuongNews
                .OrderBy(c => c.MaLopHoc).ThenBy(c => c.TenChuong)
                .Select(c => new { c.MaChuong, c.TenChuong, c.MaLopHoc })
                .ToListAsync();

            // Bài (kèm MaChuong để cascade)
            ViewBag.BaiList = await _context.Bai
                .OrderBy(b => b.MaChuong).ThenBy(b => b.SoBai)
                .Select(b => new { b.BaiId, b.TenBai, b.MaChuong })
                .ToListAsync();

            return View(list);
        }

        public JsonResult GetChuongByLop(int? lopHocId)
        {
            if (lopHocId == null)
            {
                return Json(new List<object>());
            }

            var chuongs = _context.ChuongNews
                .Where(c => c.MaLopHoc == lopHocId)
                .Select(c => new {
                    maChuong = c.MaChuong,
                    tenChuong = c.TenChuong
                }).ToList();

            return Json(chuongs);
        }


        // GET: Admin/Muc/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Muc == null)
            {
                return NotFound();
            }

            var muc = await _context.Muc
                .Include(m => m.Bai)
                .ThenInclude(c => c.Chuong)
                    .ThenInclude(ch => ch.LopHoc)
                .FirstOrDefaultAsync(m => m.MucId == id);
            if (muc == null)
            {
                return NotFound();
            }

            return View(muc);
        }

        public IActionResult GetBaiByChuongId(int chuongId)
        {
            var danhSachBai = _context.Bai.Where(b => b.MaChuong == chuongId).ToList();
            return Json(danhSachBai);
        }

        // GET: Admin/Muc/Create
        // public IActionResult Create()
        // {
        //     var chuongid = new SelectList(_context.ChuongNews, "MaChuong", "TenChuong");
        //     ViewData["ChuongId"] = chuongid;
        //     var firstChuongId = _context.ChuongNews.FirstOrDefault()?.MaChuong ?? 0;
        //     if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
        //     {
        //         int chuongId = int.Parse(Request.Query["ChuongId"]);
        //         var danhSachBai = _context.Bai.Where(b => b.MaChuong == chuongId).ToList();
        //         var baiid = new SelectList(danhSachBai, "BaiId", "TenBai");
        //         ViewData["BaiId"] = baiid;
        //     }
        //     else
        //     {
        //         ViewData["BaiId"] = new SelectList(string.Empty, "Value", "Text");
        //     }

        //     return View();
        // }
        // GET: Admin/Muc/Create
        public async Task<IActionResult> Create(int? lopId, int? chuongId)
        {
            // Lớp đang hoạt động
            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == "Hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();
            ViewBag.LopList = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", lopId);

            // Chương theo lớp (nếu đã chọn lớp)
            var chuongList = Enumerable.Empty<object>();
            if (lopId.HasValue)
            {
                chuongList = await _context.ChuongNews
                    .Where(c => c.MaLopHoc == lopId.Value)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong })
                    .ToListAsync();
            }
            ViewBag.ChuongId = new SelectList(chuongList, "MaChuong", "TenChuong", chuongId);

            // Bài theo chương (nếu đã chọn chương)
            var baiList = Enumerable.Empty<object>();
            if (chuongId.HasValue)
            {
                baiList = await _context.Bai
                    .Where(b => b.MaChuong == chuongId.Value)
                    .OrderBy(b => b.SoBai)
                    .Select(b => new { b.BaiId, b.TenBai })
                    .ToListAsync();
            }
            ViewBag.BaiId = new SelectList(baiList, "BaiId", "TenBai");

            return View();
        }

        // POST: Admin/Muc/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> Create([Bind("MucId,TenMuc,BaiId,MucSo,NoiDung")] Muc muc)
        // {
        //     if (muc.BaiId == 0)
        //     {
        //         _notyfService.Error("Vui lòng chọn bài học");
        //         var chuongid = new SelectList(_context.ChuongNews, "MaChuong", "TenChuong");
        //         ViewData["ChuongId"] = chuongid;
        //         var firstChuongId = _context.ChuongNews.FirstOrDefault()?.MaChuong ?? 0;
        //         if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
        //         {

        //             int chuongId = int.Parse(Request.Query["ChuongId"]);
        //             var danhSachBai = _context.Bai.Where(b => b.MaChuong == chuongId).ToList();
        //             var baiid = new SelectList(danhSachBai, "BaiId", "TenBai");
        //             ViewData["BaiId"] = baiid;
        //         }
        //         return View(muc);
        //     }
        //     if (ModelState.IsValid)
        //     {
        //         // Kiểm tra xem số mục đã tồn tại hay chưa
        //         var existingMuc = await _context.Muc.FirstOrDefaultAsync(m => m.MucSo == muc.MucSo && m.BaiId == muc.BaiId);
        //         if (existingMuc != null)
        //         {
        //             ModelState.AddModelError("MucSo", "Số mục đã tồn tại");
        //             var chuongid = new SelectList(_context.ChuongNews, "MaChuong", "TenChuong");
        //             ViewData["ChuongId"] = chuongid;
        //             var firstChuongId = _context.ChuongNews.FirstOrDefault()?.MaChuong ?? 0;
        //             if (!string.IsNullOrEmpty(Request.Query["ChuongId"]))
        //             {

        //                 int chuongId = int.Parse(Request.Query["ChuongId"]);
        //                 var danhSachBai = _context.Bai.Where(b => b.MaChuong == chuongId).ToList();
        //                 var baiid = new SelectList(danhSachBai, "BaiId", "TenBai");
        //                 ViewData["BaiId"] = baiid;
        //             }
        //             else
        //             {

        //                 ViewData["BaiId"] = new SelectList(string.Empty, "Value", "Text");
        //             }
        //             return View(muc);
        //         }
        //         _context.Add(muc);
        //         await _context.SaveChangesAsync();
        //         _notyfService.Success("Thêm thành công");
        //         return RedirectToAction(nameof(Index));
        //     }

        //     return View(muc);
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MucId,TenMuc,BaiId,MucSo,NoiDung")] Muc muc)
        {
            // Lấy lại LopId/ChuongId người dùng đã chọn (để rebuild dropdown khi lỗi)
            int.TryParse(Request.Form["LopId"], out var lopId);
            int.TryParse(Request.Form["ChuongId"], out var chuongId);

            if (muc.BaiId == 0)
            {
                _notyfService.Error("Vui lòng chọn bài học");
                await RebuildDropdownsForCreate(lopId, chuongId);
                return View(muc);
            }

            if (ModelState.IsValid)
            {
                // Unique MucSo trong Bài
                var exists = await _context.Muc
                    .AnyAsync(m => m.BaiId == muc.BaiId && m.MucSo == muc.MucSo);
                if (exists)
                {
                    ModelState.AddModelError("MucSo", "Số mục đã tồn tại trong bài này");
                    await RebuildDropdownsForCreate(lopId, chuongId);
                    return View(muc);
                }

                _context.Add(muc);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm thành công");
                return RedirectToAction(nameof(Index));
            }

            await RebuildDropdownsForCreate(lopId, chuongId);
            return View(muc);
        }

        private async Task RebuildDropdownsForCreate(int lopId, int chuongId)
        {
            // Lớp (active)
            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == "Hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();
            ViewBag.LopList = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", lopId == 0 ? null : lopId);

            // Chương theo Lớp
            var chuongList = Enumerable.Empty<object>();
            if (lopId != 0)
            {
                chuongList = await _context.ChuongNews
                    .Where(c => c.MaLopHoc == lopId)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong })
                    .ToListAsync();
            }
            ViewBag.ChuongId = new SelectList(chuongList, "MaChuong", "TenChuong", chuongId == 0 ? null : chuongId);

            // Bài theo Chương
            var baiList = Enumerable.Empty<object>();
            if (chuongId != 0)
            {
                baiList = await _context.Bai
                    .Where(b => b.MaChuong == chuongId)
                    .OrderBy(b => b.SoBai)
                    .Select(b => new { b.BaiId, b.TenBai })
                    .ToListAsync();
            }
            ViewBag.BaiId = new SelectList(baiList, "BaiId", "TenBai");
        }

        // GET: Admin/Muc/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
                var muc = await _context.Muc
                .Include(m => m.Bai).ThenInclude(b => b.Chuong).ThenInclude(c => c.LopHoc)
                .Include(m => m.TaiLieus)
                .FirstOrDefaultAsync(m => m.MucId == id);

            if (muc == null) return NotFound();

            var currentLopId    = muc.Bai?.Chuong?.MaLopHoc;
            var currentChuongId = muc.Bai?.MaChuong;
            var currentBaiId    = muc.BaiId;

            // Lớp đang hoạt động
            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == "Hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            // (hiếm) nếu lớp hiện tại không active, tạm thêm để nhìn thấy
            if (currentLopId.HasValue && !lopActive.Any(l => l.MaLopHoc == currentLopId.Value))
            {
                var cur = await _context.LopHocs
                    .Where(l => l.MaLopHoc == currentLopId.Value)
                    .Select(l => new { l.MaLopHoc, TenLopHoc = l.TenLopHoc + " (tạm thêm)" })
                    .FirstOrDefaultAsync();
                if (cur != null) lopActive.Insert(0, cur);
            }

            ViewBag.LopList = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", currentLopId);

            // Chương chỉ của lớp hiện tại (giữ selected)
            ViewBag.ChuongId = new SelectList(
                _context.ChuongNews
                    .Where(c => currentLopId.HasValue && c.MaLopHoc == currentLopId.Value)
                    .OrderBy(c => c.TenChuong)
                    .Select(c => new { c.MaChuong, c.TenChuong }),
                "MaChuong", "TenChuong", currentChuongId);

            // Bài chỉ của chương hiện tại (giữ selected)
            ViewBag.BaiId = new SelectList(
                _context.Bai
                    .Where(b => currentChuongId.HasValue && b.MaChuong == currentChuongId.Value)
                    .OrderBy(b => b.SoBai)
                    .Select(b => new { b.BaiId, b.TenBai }),
                "BaiId", "TenBai", currentBaiId);

            return View(muc);

        }

        // POST: Admin/Muc/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MucId,TenMuc,BaiId,MucSo,NoiDung")] Muc muc)
        {
            if (id != muc.MucId) return NotFound();

            if (!ModelState.IsValid)
            {
                await RebuildDropdownsForEdit(muc.BaiId);
                return View(muc);
            }

            // unique MucSo trong cùng một Bài
            var dup = await _context.Muc
                .AnyAsync(m => m.BaiId == muc.BaiId && m.MucSo == muc.MucSo && m.MucId != muc.MucId);
            if (dup)
            {
                ModelState.AddModelError("MucSo", "Số mục đã tồn tại trong bài này.");
                await RebuildDropdownsForEdit(muc.BaiId);
                return View(muc);
            }

            try
            {
                _context.Update(muc);
                await _context.SaveChangesAsync();
                _notyfService.Success("Cập nhật thành công");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Muc.AnyAsync(x => x.MucId == muc.MucId)) return NotFound();
                throw;
            }
        }

        // Helper: build lại 3 dropdown khi lỗi validate
        private async Task RebuildDropdownsForEdit(int baiId)
        {
            var mapping = await _context.Bai
                .Include(b => b.Chuong)
                .FirstOrDefaultAsync(b => b.BaiId == baiId);

            var lopId    = mapping?.Chuong?.MaLopHoc;
            var chuongId = mapping?.MaChuong;

            var lopActive = await _context.LopHocs
                .Where(l => l.TrangThai == "Hoạt động")
                .OrderBy(l => l.TenLopHoc)
                .Select(l => new { l.MaLopHoc, l.TenLopHoc })
                .ToListAsync();

            if (lopId.HasValue && !lopActive.Any(l => l.MaLopHoc == lopId.Value))
            {
                var cur = await _context.LopHocs
                    .Where(l => l.MaLopHoc == lopId.Value)
                    .Select(l => new { l.MaLopHoc, TenLopHoc = l.TenLopHoc + " (tạm thêm)" })
                    .FirstOrDefaultAsync();
                if (cur != null) lopActive.Insert(0, cur);
            }
            ViewBag.LopList = new SelectList(lopActive, "MaLopHoc", "TenLopHoc", lopId);

            var chuongList = await _context.ChuongNews
                .Where(c => lopId.HasValue && c.MaLopHoc == lopId.Value)
                .OrderBy(c => c.TenChuong)
                .Select(c => new { c.MaChuong, c.TenChuong })
                .ToListAsync();
            // dùng đúng key mà View đang xài
            ViewBag.ChuongId = new SelectList(chuongList, "MaChuong", "TenChuong", chuongId);

            var baiList = await _context.Bai
                .Where(b => chuongId.HasValue && b.MaChuong == chuongId.Value)
                .OrderBy(b => b.SoBai)
                .Select(b => new { b.BaiId, b.TenBai })
                .ToListAsync();
            // dùng đúng key mà View đang xài
            ViewBag.BaiId = new SelectList(baiList, "BaiId", "TenBai", baiId);
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

        [HttpPost]
        public async Task<IActionResult> DeleteTaiLieu(int id)
        {
            var taiLieu = await _context.TaiLieus.FindAsync(id);
            if (taiLieu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài liệu." });
            }

            // Nếu có lưu file vật lý thì xóa luôn trên ổ đĩa
            if (!string.IsNullOrEmpty(taiLieu.FileTaiLieu))
            {
                var physicalPath = Path.Combine(_webHostEnvironment.WebRootPath,
                    taiLieu.FileTaiLieu.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));

                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            _context.TaiLieus.Remove(taiLieu);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UploadTaiLieu(int mucId, List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return Json(new { success = false, message = "Vui lòng chọn file." });

            var muc = await _context.Muc.FindAsync(mucId);
            if (muc == null)
                return Json(new { success = false, message = "Không tìm thấy mục." });

            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "tailieu");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;

                var fileName = Path.GetFileName(file.FileName);
                var uniqueName = $"{Guid.NewGuid()}_{fileName}";
                var filePath = Path.Combine(uploadFolder, uniqueName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var taiLieu = new TaiLieu
                {
                    MaMucCon = mucId,
                    // Lưu đường dẫn tương đối để view dùng làm href
                    FileTaiLieu = $"/uploads/tailieu/{uniqueName}"
                };

                _context.TaiLieus.Add(taiLieu);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

    }
}
