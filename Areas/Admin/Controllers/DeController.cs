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
            var list = await _context.De
                .Include(d => d.KyKiemTra)
                .OrderByDescending(d => d.DeId)
                .ToListAsync();

            ViewBag.KyKiemTraList = _context.KyKiemTra
                .OrderBy(x => x.TenKyKiemTra)
                .ToList();

            return View(list);
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
                    .ThenInclude(c => c.ChuongNew)
                        .ThenInclude(ch => ch.LopHoc)   
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
            if (!ModelState.IsValid)
            {
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                ViewBag.LopHoc = _context.LopHocs.AsNoTracking().ToList();
                return View(de);
            }

            // Kiểm tra kỳ kiểm tra đã có đề chưa
            var existingDe = await _context.De
                .FirstOrDefaultAsync(d => d.KyKiemTraId == de.KyKiemTraId);

            if (existingDe != null)
            {
                _notyfService.Error("Đã có đề thi cho kỳ kiểm tra này!");
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                return View(de);
            }

            _context.Add(de);
            de.CauHoi_DeThi = new List<CauHoi_De>();

            int tongSoCauChon = 0;
            bool hasInvalidInput = false;

            // 🔥 DUYỆT TOÀN BỘ INPUT TỪ FORM
            foreach (var key in Request.Form.Keys)
            {
                // Chỉ lấy key dạng: Chuong_x_y
                if (!key.StartsWith("Chuong_")) continue;

                string[] parts = key.Split('_');
                // parts[1] = MaLop
                // parts[2] = MaChuong

                if (parts.Length != 3) continue;

                int maChuong = int.Parse(parts[2]);
                int soCauMuonChon = 0;

                int.TryParse(Request.Form[key], out soCauMuonChon);

                if (soCauMuonChon <= 0) continue;

                // Lấy tổng số câu hiện có của chương này
                int tongCauHoiTrongChuong = await _context.CauHoi
                    .CountAsync(c => c.MaChuong == maChuong);

                if (soCauMuonChon > tongCauHoiTrongChuong)
                {
                    var tenChuong = await _context.ChuongNews
                        .Where(x => x.MaChuong == maChuong)
                        .Select(x => x.TenChuong)
                        .FirstOrDefaultAsync();

                    _notyfService.Error($"Số câu hỏi nhập vào cho chương '{tenChuong}' vượt quá số lượng có trong ngân hàng câu hỏi.");
                    hasInvalidInput = true;
                    break;
                }

                // Lấy danh sách câu hỏi và random
                var dsCauHoi = await _context.CauHoi
                    .Where(c => c.MaChuong == maChuong)
                    .ToListAsync();

                var rng = new Random();
                dsCauHoi = dsCauHoi.OrderBy(x => rng.Next()).ToList();

                // Chọn số câu
                var selected = dsCauHoi.Take(soCauMuonChon).ToList();

                tongSoCauChon += selected.Count;

                // Thêm vào bảng De mapping
                foreach (var cauHoi in selected)
                {
                    de.CauHoi_DeThi.Add(new CauHoi_De
                    {
                        DeId = de.DeId,
                        CauHoiId = cauHoi.CauHoiId
                    });
                }
            }

            // Kiểm tra tổng số câu
            if (hasInvalidInput)
            {
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                ViewBag.LopHoc = _context.LopHocs.AsNoTracking().ToList();
                return View(de);
            }

            if (tongSoCauChon != de.SoCauHoi)
            {
                _notyfService.Error($"Tổng số câu đã chọn ({tongSoCauChon}) không bằng tổng số câu của đề ({de.SoCauHoi}).");
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                ViewBag.LopHoc = _context.LopHocs.AsNoTracking().ToList();
                return View(de);
            }

            // Lưu
            await _context.SaveChangesAsync();
            _notyfService.Success("Thêm đề thi thành công!");
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var de = await _context.De
                .Include(d => d.CauHoi_DeThi)
                    .ThenInclude(chd => chd.CauHoi)
                        .ThenInclude(c => c.ChuongNew)
                .FirstOrDefaultAsync(d => d.DeId == id);

            if (de == null)
                return NotFound();
            

            var ky = await _context.KyKiemTra
                .FirstOrDefaultAsync(k => k.KyKiemTraId == de.KyKiemTraId);

            if (ky.ThoiGianBatDau <= DateTime.Now)
            {
                _notyfService.Error("Kỳ kiểm tra đã bắt đầu, không thể chỉnh sửa đề thi!");
                return RedirectToAction(nameof(Index));
            }


            // Danh sách lớp
            var dsLop = await _context.LopHocs.AsNoTracking().ToListAsync();
            ViewBag.LopHoc = dsLop;

            // Lớp đã chọn
            var lopDaChon = de.CauHoi_DeThi
                .Select(x => x.CauHoi.ChuongNew.MaLopHoc)
                .Distinct()
                .ToList();

            ViewBag.LopDaChon = lopDaChon;

            // ⚠ FIX: Chuẩn bị dictionary số câu đã chọn
            var soCauDaChonDict = de.CauHoi_DeThi
                .GroupBy(x => x.CauHoi.MaChuong)
                .ToDictionary(g => g.Key, g => g.Count());

            // Bảng chương
            var danhSachChuong = new List<object>();

            foreach (var maLop in lopDaChon)
            {
                var chuongTheoLop = await _context.ChuongNews
                    .Where(c => c.MaLopHoc == maLop)
                    .Select(c => new
                    {
                        maLop = maLop,
                        maChuong = c.MaChuong,
                        tenChuong = c.TenChuong,
                        soLuongCauHoi = _context.CauHoi
                            .Where(ch => ch.MaChuong == c.MaChuong)
                            .Count(),

                        // ⚠ FIX: Lấy từ dictionary, không truy vấn cross-context nữa
                        soCauDaChon = soCauDaChonDict.ContainsKey(c.MaChuong)
                            ? soCauDaChonDict[c.MaChuong]
                            : 0
                    })
                    .ToListAsync();

                danhSachChuong.AddRange(chuongTheoLop);
            }

            ViewBag.DanhSachChuong = danhSachChuong;

            ViewData["KyKiemTraId"] = new SelectList(
                _context.KyKiemTra,
                "KyKiemTraId",
                "TenKyKiemTra",
                de.KyKiemTraId
            );

            return View(de);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeId,KyKiemTraId,SoCauHoi,DoKhoDe")] De de)
        {
            if (id != de.DeId)
                return NotFound();

            // Nếu đã có sinh viên làm → không cho sửa
            var kiemtrasv = await _context.BaiLam
                .FirstOrDefaultAsync(x => x.CauHoi_BaiLam.First().CauHoi_De.DeId == id);

            if (kiemtrasv != null)
            {
                _notyfService.Error("Đề thi này đã có sinh viên làm bài, không thể chỉnh sửa!");
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                return View(de);
            }

            var dbDe = await _context.De
                .Include(d => d.CauHoi_DeThi)
                .FirstOrDefaultAsync(d => d.DeId == id);

            if (dbDe == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(de);

            // Cập nhật info cơ bản
            dbDe.KyKiemTraId = de.KyKiemTraId;
            dbDe.SoCauHoi = de.SoCauHoi;
            dbDe.DoKhoDe = de.DoKhoDe;
            dbDe.CauHoi_DeThi.Clear();

            int tongSoCauChon = 0;
            bool hasInvalidInput = false;

            // Duyệt tất cả input dạng: Chuong_maLop_maChuong
            foreach (var key in Request.Form.Keys)
            {
                if (!key.StartsWith("Chuong_"))
                    continue;

                string[] parts = key.Split('_');
                if (parts.Length != 3)
                    continue;

                int maChuong = int.Parse(parts[2]);

                int soCauMuonChon = 0;
                int.TryParse(Request.Form[key], out soCauMuonChon);

                if (soCauMuonChon <= 0)
                    continue;

                // Lấy tổng số câu hỏi trong chương
                int tongCauHoiTrongChuong = await _context.CauHoi
                    .CountAsync(c => c.MaChuong == maChuong);

                if (soCauMuonChon > tongCauHoiTrongChuong)
                {
                    string tenChuong = await _context.ChuongNews
                        .Where(c => c.MaChuong == maChuong)
                        .Select(c => c.TenChuong)
                        .FirstOrDefaultAsync();

                    _notyfService.Error($"Số câu của chương '{tenChuong}' vượt quá số lượng hiện có!");
                    hasInvalidInput = true;
                    break;
                }

                // Random câu
                var dsCauHoi = await _context.CauHoi
                    .Where(c => c.MaChuong == maChuong)
                    .ToListAsync();

                var rng = new Random();
                dsCauHoi = dsCauHoi.OrderBy(x => rng.Next()).ToList();

                var selected = dsCauHoi.Take(soCauMuonChon).ToList();
                tongSoCauChon += selected.Count;

                foreach (var cauHoi in selected)
                {
                    dbDe.CauHoi_DeThi.Add(new CauHoi_De
                    {
                        DeId = dbDe.DeId,
                        CauHoiId = cauHoi.CauHoiId
                    });
                }
            }

            // Nếu có lỗi → trả về view
            if (hasInvalidInput)
                return View(de);

            // Tổng số câu không khớp
            if (tongSoCauChon != dbDe.SoCauHoi)
            {
                _notyfService.Error($"Tổng số câu chọn ({tongSoCauChon}) không bằng tổng đề ({dbDe.SoCauHoi})");
                return View(de);
            }

            // Lưu
            await _context.SaveChangesAsync();

            _notyfService.Success("Cập nhật đề thi thành công!");
            return RedirectToAction(nameof(Index));
        }



        // DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.De == null)
                return NotFound();

            var de = await _context.De.Include(d => d.KyKiemTra)
                .FirstOrDefaultAsync(m => m.DeId == id);

            if (de == null) return NotFound();
            
            if (de.KyKiemTra.ThoiGianBatDau <= DateTime.Now)
            {
                _notyfService.Error("Kỳ kiểm tra đã bắt đầu, không thể xóa đề thi!");
                return RedirectToAction(nameof(Index));
            }


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
