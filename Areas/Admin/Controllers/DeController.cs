using AspNetCoreHero.ToastNotification.Abstractions;
using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DeController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }

        public DeController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        // GET: Admin/De
        public async Task<IActionResult> Index()
        {
            var baiGiangContext = _context.De.Include(d => d.KyKiemTra);
            return View(await baiGiangContext.ToListAsync());
        }

        // GET: Admin/De/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.De == null)
            {
                return NotFound();
            }

            var de = await _context.De
    .Include(d => d.KyKiemTra)
    .Include(d => d.CauHoi_DeThi)
        .ThenInclude(chd => chd.CauHoi)
            .ThenInclude(c => c.ChuongNew) // nếu muốn hiển thị tên chương
    .FirstOrDefaultAsync(m => m.DeId == id);


            if (de == null)
            {
                return NotFound();
            }

            return View(de);
        }


        // ✅ Lấy danh sách chương từ CHUONG_NEW
        [HttpGet]
        public async Task<IActionResult> LayDanhSachChuong()
        {
            var danhSachChuong = await _context.CauHoi
                .Include(c => c.ChuongNew)
                .Select(c => new { c.MaChuong, tenChuong = c.ChuongNew.TenChuong })
                .Distinct()
                .ToListAsync();

            return Json(danhSachChuong);
        }

        // GET: Admin/De/Create
        public IActionResult Create()
        {
            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra");
            ViewBag.LopHoc = _context.LopHocs.AsNoTracking().ToList();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> LayDanhSachChuongTheoLop(int maLop)
        {
            // ✅ Lọc chương có câu hỏi thuộc lớp được chọn
            var danhSachChuong = await _context.ChuongNews
                .Where(c => c.MaLopHoc == maLop && _context.CauHoi.Any(ch => ch.MaChuong == c.MaChuong))
                .Select(c => new
                {
                    maChuong = c.MaChuong,
                    tenChuong = c.TenChuong,
                    soLuongCauHoi = _context.CauHoi.Count(ch => ch.MaChuong == c.MaChuong)
                })
                .ToListAsync();

            return Json(danhSachChuong);
        }


        // POST: Admin/De/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeId,KyKiemTraId,SoCauHoi,DoKhoDe")] De de)
        {
            if (ModelState.IsValid)
            {
                var existingDe = await _context.De.FirstOrDefaultAsync(d => d.KyKiemTraId == de.KyKiemTraId);
                if (existingDe != null)
                {
                    _notyfService.Error("Đã có đề thi cho kỳ kiểm tra này!");
                    ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                    return View(de);
                }

                _context.Add(de);

                var danhSachChuong = await _context.CauHoi
                    .Select(c => c.ChuongNew)
                    .Distinct()
                    .ToListAsync();

                de.CauHoi_DeThi = new List<CauHoi_De>();
                int tongSoCauHoi = 0;
                bool hasInvalidInput = false;

                foreach (var chuong in danhSachChuong)
                {
                    if (int.TryParse(Request.Form["CauHoiChuong" + chuong.MaChuong], out var soCauHoiChuong))
                    {
                        var cauHoiChuongCount = await _context.CauHoi
                            .CountAsync(c => c.MaChuong == chuong.MaChuong);

                        if (soCauHoiChuong > cauHoiChuongCount)
                        {
                            _notyfService.Error($"Số câu hỏi cho chương {chuong.TenChuong} vượt quá số câu hỏi hiện có.");
                            hasInvalidInput = true;
                            break;
                        }

                        var cauHoiChuong = await _context.CauHoi
                            .Where(c => c.MaChuong == chuong.MaChuong)
                            .ToListAsync();

                        var rng = new Random();
                        cauHoiChuong = cauHoiChuong.OrderBy(x => rng.Next()).ToList();
                        var cauHoiChuongSelected = cauHoiChuong.Take(soCauHoiChuong).ToList();

                        tongSoCauHoi += cauHoiChuongSelected.Count;

                        foreach (var cauHoi in cauHoiChuongSelected)
                        {
                            de.CauHoi_DeThi.Add(new CauHoi_De
                            {
                                DeId = de.DeId,
                                CauHoiId = cauHoi.CauHoiId
                            });
                        }
                    }
                }

                if (!hasInvalidInput && tongSoCauHoi == de.SoCauHoi)
                {
                    _notyfService.Success("Thêm đề thi thành công");
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else if (hasInvalidInput)
                {
                    _notyfService.Error("Vui lòng kiểm tra lại số câu hỏi nhập cho từng chương.");
                }
                else
                {
                    _notyfService.Error("Tổng số câu hỏi nhập không khớp với số câu hỏi của đề thi.");
                }
            }

            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
            ViewBag.LopHoc = _context.LopHocs.AsNoTracking().ToList();
            return View(de);
        }

        // GET: Admin/De/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.De == null)
            {
                return NotFound();
            }

            var de = await _context.De
                .Include(d => d.KyKiemTra)
                .Include(d => d.CauHoi_DeThi)
                    .ThenInclude(chd => chd.CauHoi)
                        .ThenInclude(c => c.ChuongNew)
                .FirstOrDefaultAsync(d => d.DeId == id);

            if (de == null)
            {
                return NotFound();
            }

            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
            ViewBag.LopHoc = await _context.LopHocs.AsNoTracking().ToListAsync();

            // Lấy danh sách chương có câu hỏi
            ViewBag.DanhSachChuong = await _context.ChuongNews
                .Where(c => _context.CauHoi.Any(ch => ch.MaChuong == c.MaChuong))
                .Select(c => new
                {
                    c.MaChuong,
                    c.TenChuong,
                    SoLuongCauHoi = _context.CauHoi.Count(ch => ch.MaChuong == c.MaChuong)
                })
                .ToListAsync();

            return View(de);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeId,KyKiemTraId,SoCauHoi,DoKhoDe")] De de)
        {
            if (id != de.DeId)
                return NotFound();

            var kiemtrasv = await _context.BaiLam
                .FirstOrDefaultAsync(x => x.CauHoi_BaiLam.First().CauHoi_De.DeId == id);
            if (kiemtrasv != null)
            {
                _notyfService.Error("Đề thi này đã có sinh viên làm bài, không thể chỉnh sửa!");
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                return View(de);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dbDe = await _context.De
                        .Include(d => d.CauHoi_DeThi)
                        .FirstOrDefaultAsync(d => d.DeId == id);

                    if (dbDe == null)
                        return NotFound();

                    dbDe.KyKiemTraId = de.KyKiemTraId;
                    dbDe.SoCauHoi = de.SoCauHoi;
                    dbDe.DoKhoDe = de.DoKhoDe;
                    dbDe.CauHoi_DeThi.Clear();

                    var danhSachChuong = await _context.CauHoi
                        .Select(c => c.ChuongNew)
                        .Distinct()
                        .ToListAsync();

                    int tongSoCauHoi = 0;
                    bool hasInvalidInput = false;

                    foreach (var chuong in danhSachChuong)
                    {
                        if (int.TryParse(Request.Form["CauHoiChuong" + chuong.MaChuong], out var soCauHoiChuong))
                        {
                            var cauHoiChuongCount = await _context.CauHoi.CountAsync(c => c.MaChuong == chuong.MaChuong);

                            if (soCauHoiChuong > cauHoiChuongCount)
                            {
                                _notyfService.Error($"Số câu hỏi cho chương {chuong.TenChuong} vượt quá số câu hỏi hiện có.");
                                hasInvalidInput = true;
                                break;
                            }

                            var cauHoiChuong = await _context.CauHoi
                                .Where(c => c.MaChuong == chuong.MaChuong)
                                .ToListAsync();

                            var rng = new Random();
                            cauHoiChuong = cauHoiChuong.OrderBy(x => rng.Next()).ToList();
                            var cauHoiChuongSelected = cauHoiChuong.Take(soCauHoiChuong).ToList();

                            tongSoCauHoi += cauHoiChuongSelected.Count;

                            foreach (var cauHoi in cauHoiChuongSelected)
                            {
                                dbDe.CauHoi_DeThi.Add(new CauHoi_De
                                {
                                    DeId = dbDe.DeId,
                                    CauHoiId = cauHoi.CauHoiId
                                });
                            }
                        }
                    }
                    _notyfService.Information($"Tổng nhập: {tongSoCauHoi}, Đề yêu cầu: {dbDe.SoCauHoi}");

                    if (hasInvalidInput)
                    {
                        _notyfService.Error("Vui lòng kiểm tra lại số câu hỏi nhập cho từng chương.");
                        return View(de);
                    }

                    if (tongSoCauHoi != dbDe.SoCauHoi)
                    {
                        _notyfService.Error("Tổng số câu hỏi nhập không khớp với số câu hỏi của đề thi.");
                        return View(de);
                    }

                    await _context.SaveChangesAsync();
                    _notyfService.Success("Cập nhật đề thi thành công!");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _notyfService.Error("Lỗi khi cập nhật đề thi: " + ex.Message);
                }
            }

            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
            ViewBag.LopHoc = await _context.LopHocs.AsNoTracking().ToListAsync();
            return View(de);
        }


        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.De == null)
                return NotFound();

            var de = await _context.De.Include(d => d.KyKiemTra)
                .FirstOrDefaultAsync(m => m.DeId == id);

            if (de == null) return NotFound();

            return View(de);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var de = await _context.De.FindAsync(id);
            if (de != null)
            {
                _context.De.Remove(de);
                _notyfService.Success("Xóa đề thi thành công");
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ✅ PDF Xuất đề thi
        public async Task<IActionResult> PdfDeViewer(int deId)
        {
            var kykiemtra = _context.KyKiemTra.FirstOrDefault(x => x.De.FirstOrDefault().DeId == deId);
            var htmlcontent = BuildDeHtml(kykiemtra, deId);
            var pdfStream = new MemoryStream();
            HtmlConverter.ConvertToPdf(htmlcontent, pdfStream);
            return new FileContentResult(pdfStream.ToArray(), "application/pdf");
        }

        private string BuildDeHtml(KyKiemTra kykiemtra, int deId)
        {
            var sb = new StringBuilder();
            sb.Append($@"<!DOCTYPE html><html><body><h2>{kykiemtra.TenKyKiemTra}</h2>");
            var cauhoiList = _context.CauHoi_De.Include(c => c.CauHoi).Where(c => c.DeId == deId).ToList();
            int i = 1;
            foreach (var c in cauhoiList)
            {
                sb.Append($"<p><b>{i}. {c.CauHoi.NoiDung}</b><br/>A. {c.CauHoi.DapAnA}<br/>B. {c.CauHoi.DapAnB}<br/>C. {c.CauHoi.DapAnC}<br/>D. {c.CauHoi.DapAnD}</p>");
                i++;
            }
            sb.Append("</body></html>");
            return sb.ToString();
        }
    }
}
