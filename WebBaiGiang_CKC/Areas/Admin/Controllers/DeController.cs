using AspNetCoreHero.ToastNotification.Abstractions;
using DocumentFormat.OpenXml.InkML;
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
                .FirstOrDefaultAsync(m => m.DeId == id);
            if (de == null)
            {
                return NotFound();
            }

            return View(de);
        }
        [HttpGet]
        public async Task<IActionResult> LayDanhSachChuong()
        {
            var danhSachChuong = await _context.CauHoi
                .Include(c => c.Chuong)
                .Select(c => new { c.ChuongId, tenChuong = c.Chuong.TenChuong })
                .Distinct()
                .ToListAsync();
            return Json(danhSachChuong);
        }
        // GET: Admin/De/Create
        public IActionResult Create()
        {
            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra");
            return View();
        }

        // POST: Admin/De/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    .Select(c => c.Chuong)
                    .Distinct()
                    .ToListAsync();

                de.CauHoi_DeThi = new List<CauHoi_De>();
                int tongSoCauHoi = 0;
                bool hasInvalidInput = false;

                foreach (var chuong in danhSachChuong)
                {
                    if (int.TryParse(Request.Form["CauHoiChuong" + chuong.ChuongId], out var soCauHoiChuong))
                    {
                        var cauHoiChuongCount = await _context.CauHoi
                                      .CountAsync(c => c.ChuongId == chuong.ChuongId);

                        if (soCauHoiChuong > cauHoiChuongCount)
                        {
                            _notyfService.Error("Số câu hỏi nhập bổ sung cho chương " + chuong.TenChuong + " không được lớn hơn số câu hỏi có trong chương.");
                            hasInvalidInput = true;
                            break;
                        }
                        else
                        {
                            var cauHoiChuong = await _context.CauHoi
                                                     .Where(c => c.ChuongId == chuong.ChuongId)
                                                     .ToListAsync();
                            var rng = new Random();
                            int n = cauHoiChuong.Count;
                            while (n > 1)
                            {
                                n--;
                                int k = rng.Next(n + 1);
                                var value = cauHoiChuong[k];
                                cauHoiChuong[k] = cauHoiChuong[n];
                                cauHoiChuong[n] = value;
                            }
                            var cauHoiChuongSelected = cauHoiChuong.Take(soCauHoiChuong).ToList();
                            tongSoCauHoi += cauHoiChuongSelected.Count;

                            foreach (var cauHoi in cauHoiChuongSelected)
                            {
                                var cauHoi_De = new CauHoi_De
                                {
                                    DeId = de.DeId,
                                    CauHoiId = cauHoi.CauHoiId
                                };
                                de.CauHoi_DeThi.Add(cauHoi_De);
                            }
                        }

                    }
                }

                if (!hasInvalidInput && tongSoCauHoi == de.SoCauHoi)
                {
                    _context.Add(de);
                    _notyfService.Success("Thêm Thành Công");
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else if (hasInvalidInput)
                {

                }
                else
                {
                    _notyfService.Error("Vui lòng nhập tổng số câu hỏi nhập từ form  bằng với số câu hỏi của đề thi.");
                }
            }

            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
            return View(de);
        }

        // GET: Admin/De/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.De == null)
            {
                return NotFound();
            }

            var de = await _context.De.FindAsync(id);
            if (de == null)
            {
                return NotFound();
            }
            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
            return View(de);
        }

        // POST: Admin/De/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DeId,KyKiemTraId,SoCauHoi,DoKhoDe")] De de)
        {
            if (id != de.DeId)
            {
                return NotFound();
            }
            var kiemtrasv = _context.BaiLam.FirstOrDefault(x => x.CauHoi_BaiLam.First().CauHoi_De.DeId == id);
            if (kiemtrasv != null)
            {
                _notyfService.Error("Đề thi này đã có sinh viên kiểm tra rồi!");
                ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                return View(de);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingDe = await _context.De.FirstOrDefaultAsync(d => d.KyKiemTraId == de.KyKiemTraId && d.DeId != de.DeId);
                    if (existingDe != null)
                    {
                        _notyfService.Error("Đã có đề thi cho kỳ kiểm tra này!");
                        ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                        return View(de);
                    }

                    var dbDe = await _context.De
                        .Include(d => d.CauHoi_DeThi)
                        .FirstOrDefaultAsync(d => d.DeId == de.DeId);

                    if (dbDe == null)
                    {
                        return NotFound();
                    }

                    var danhSachChuong = await _context.CauHoi
                        .Select(c => c.Chuong)
                        .Distinct()
                        .ToListAsync();

                    dbDe.SoCauHoi = de.SoCauHoi;
                    dbDe.DoKhoDe = de.DoKhoDe;

                    dbDe.CauHoi_DeThi.Clear();

                    int tongSoCauHoi = 0;
                    bool hasInvalidInput = false;

                    foreach (var chuong in danhSachChuong)
                    {
                        if (int.TryParse(Request.Form["CauHoiChuong" + chuong.ChuongId], out var soCauHoiChuong))
                        {
                            var cauHoiChuongCount = await _context.CauHoi
                                          .CountAsync(c => c.ChuongId == chuong.ChuongId);

                            if (soCauHoiChuong > cauHoiChuongCount)
                            {
                                _notyfService.Error("Số câu hỏi nhập bổ sung cho chương " + chuong.TenChuong + " không được lớn hơn số câu hỏi có trong chương.");
                                hasInvalidInput = true;
                                break;
                            }
                            else
                            {
                                var cauHoiChuong = await _context.CauHoi
                                                         .Where(c => c.ChuongId == chuong.ChuongId)
                                                         .ToListAsync();
                                var rng = new Random();
                                int n = cauHoiChuong.Count;
                                while (n > 1)
                                {
                                    n--;
                                    int k = rng.Next(n + 1);
                                    var value = cauHoiChuong[k];
                                    cauHoiChuong[k] = cauHoiChuong[n];
                                    cauHoiChuong[n] = value;
                                }
                                var cauHoiChuongSelected = cauHoiChuong.Take(soCauHoiChuong).ToList();
                                tongSoCauHoi += cauHoiChuongSelected.Count;

                                foreach (var cauHoi in cauHoiChuongSelected)
                                {
                                    var cauHoi_De = new CauHoi_De
                                    {
                                        DeId = dbDe.DeId,
                                        CauHoiId = cauHoi.CauHoiId
                                    };
                                    dbDe.CauHoi_DeThi.Add(cauHoi_De);
                                }
                            }
                        }
                    }

                    // Kiểm tra số câu hỏi nhập vào có vượt quá số câu hỏi tối đa trong các chương hay không
                    var totalNewQuestions = danhSachChuong.Sum(chuong => int.TryParse(Request.Form["CauHoiChuong" + chuong.ChuongId], out var soCauHoiChuong) ? soCauHoiChuong : 0);
                    if (tongSoCauHoi != dbDe.SoCauHoi)
                    {
                        _notyfService.Error("Tổng số câu hỏi nhập vào của các chương phải bằng số câu hỏi của đề thi.");
                        ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                        return View(dbDe);
                    }
                    if (totalNewQuestions > dbDe.SoCauHoi)
                    {
                        _notyfService.Error("Số câu hỏi nhập bổ sung vượt quá số câu hỏi của đề thi.");
                        hasInvalidInput = true;
                    }
                    else if (totalNewQuestions < dbDe.SoCauHoi)
                    {
                        // Kiểm tra xem có đủ số câu hỏi trong các chương không
                        foreach (var chuong in danhSachChuong)
                        {
                            if (int.TryParse(Request.Form["CauHoiChuong" + chuong.ChuongId], out var soCauHoiChuong))
                            {
                                var cauHoiChuongCount = await _context.CauHoi
                                                              .CountAsync(c => c.ChuongId == chuong.ChuongId);
                                if (soCauHoiChuong > cauHoiChuongCount)
                                {
                                    _notyfService.Error("Số câu hỏi nhập bổ sung cho chương " + chuong.TenChuong + " không được lớn hơn số câu hỏi có trong chương.");
                                    hasInvalidInput = true;
                                    break;
                                }
                            }
                        }
                        if (!hasInvalidInput)
                        {
                            var cauHoiKhongThuocChuong = await _context.CauHoi
                                                                  .Where(c => !danhSachChuong.Contains(c.Chuong))
                                                                  .ToListAsync();
                            var rng = new Random();
                            int n = cauHoiKhongThuocChuong.Count;
                            while (n > 1)
                            {
                                n--;
                                int k = rng.Next(n + 1);
                                var value = cauHoiKhongThuocChuong[k];
                                cauHoiKhongThuocChuong[k] = cauHoiKhongThuocChuong[n];
                                cauHoiKhongThuocChuong[n] = value;
                            }
                            var cauHoiKhongThuocChuongSelected = cauHoiKhongThuocChuong.Take(dbDe.SoCauHoi - tongSoCauHoi).ToList();

                            foreach (var cauHoi in cauHoiKhongThuocChuongSelected)
                            {
                                var cauHoi_De = new CauHoi_De
                                {
                                    DeId = dbDe.DeId,
                                    CauHoiId = cauHoi.CauHoiId
                                };
                                dbDe.CauHoi_DeThi.Add(cauHoi_De);
                            }
                        }
                    }

                    if (hasInvalidInput)
                    {
                        ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
                        return View(dbDe);
                    }
                    _notyfService.Success("Cập nhật thành công");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DeExists(de.DeId))
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
            ViewData["KyKiemTraId"] = new SelectList(_context.KyKiemTra, "KyKiemTraId", "TenKyKiemTra", de.KyKiemTraId);
            return View(de);
        }
        // GET: Admin/De/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.De == null)
            {
                return NotFound();
            }

            var de = await _context.De
                .Include(d => d.KyKiemTra)
                .FirstOrDefaultAsync(m => m.DeId == id);
            if (de == null)
            {
                return NotFound();
            }

            return View(de);
        }

        // POST: Admin/De/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.De == null)
            {
                return Problem("Entity set 'BaiGiangContext.De'  is null.");
            }
            var de = await _context.De.FindAsync(id);
            if (de != null)
            {
                _context.De.Remove(de);
            }
            _notyfService.Success("Xóa Thành Công");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeExists(int id)
        {
            return _context.De.Any(e => e.DeId == id);
        }
        public async Task<IActionResult> PdfDeViewer(int deId)
        {
            var kykiemtra = _context.KyKiemTra.FirstOrDefault(x => x.De.FirstOrDefault().DeId == deId);
            var document = new StringBuilder();
            var htmlcontent = @"<!DOCTYPE html>
            <html>

            <head>
            <title>Đề thi trắc nghiệm</title>
              <style>
               
                .container {
                  max-width: 794px;
                  margin: 0 auto;
                  font-family: Helvetica;
                 line-height: 1.5;
                }

                .hedert {
                  height: 150px;
                  width: 370px;
                  float: left;
                }

                .hedert p {
                  font-size: 17px;
                }

                .hedert h4 {
                  padding-left: 50px;
                }

                .hedery {

                  text-align: center;
                }

                .mssv {
                  margin-left: 500px;
                  margin-top: -40px;
                }
                .mini1
                {
                  margin-left: 500px;
                  margin-top: -40px;
                }
    
                h1,
                h2 {
                  text-align: center;
                }

                .cauhoi {
                  padding-left: 40px;
                }

                .cauhoi div {
                  padding-bottom: 40px;
                  font-size: 18px;
                  margin-top: -10px;
                }

                table {
                  border-collapse: collapse;
                  width: 100%;
                  border: 1px solid #ddd;
                  margin-bottom: 20px;
                  
                }

                td,th {
                  text-align: left;
                  border: 1px solid #757575;
                  font-size: 16px;
                }

                th {
                  background-color: #f2f2f2;
                }
 
              </style>
            </head>

            <body>
              <div class='container'>

                <div style=' overflow: hidden;'>

                  <div class='hedert'>
                    <p> TRƯỜNG CAO ĐẲNG KỸ THUẬT CAO THẮNG</p>
                    <h4> KHOA CÔNG NGHỆ THÔNG TIN</h4>
                  </div>

                  <div class='hedery'>";

            htmlcontent += @$"<h2>{kykiemtra.TenKyKiemTra}</h2>";

            htmlcontent += @$"<p>Thời gian: {kykiemtra.ThoiGianLamBai} phút (không kể thời gian phát đề)</p>
                      <p>Sinh viên không được sử dụng tài liệu</p>
                  </div>
                </div>


                <div >
                  <h2>Môn: LẬP TRÌNH ỨNG DỤNG ASP.NET CORE</h2>
                  <h3 style='text-align: center;'>Hệ/Khóa: CĐTH ............. </h3>


                </div>
                <div style='font-size: 19px;overflow: hidden;margin-left:130px;'>
                 <p style='float: left; padding-right: 30px;'>Ngày thi: {kykiemtra.ThoiGianBatDau.ToString("dd/MM/yyyy")}</p>
                  <p>Thời lượng: {kykiemtra.ThoiGianLamBai} phút</p>
                  <p style='margin-left: 150px;'>Mã đề: .........</p>
                </div>
               

                <h2>HƯỚNG DẪN TRẢ LỜI TRẮC NGHIỆM</h2>
                <hr>
                <div style='font-size: 16px;padding-left: 20px;padding-right: 20px;'>
                  <p> Sinh viên ghi HỌ VÀ TÊN, Mã SV, LỚP.</p>
                 <p style='font-style: italic;'>Mỗi câu hỏi trắc nghiệm chỉ chọn một phương án trả lời đúng nhất, nếu chọn từ hai phương án trở lên là trả lời sai.</p>
                </div>
                <h1>ĐỀ THI TRẮC NGHIỆM</h1>
                <hr>";

            //  Lấy danh sách câu hỏi từ bảng cauhoi_de
            var cauhoiList = await _context.CauHoi_De.Include(c => c.CauHoi).Where(c => c.DeId == deId).ToListAsync();

            if (cauhoiList == null || cauhoiList.Count == 0)
            {
                return BadRequest("Không tìm thấy câu hỏi nào");
            }

            //  Hiển thị từng câu hỏi và đáp áTo continue building the HTML code in C# for generating a PDF, you can add the following code after the existing HTML code:
            //  Tạo danh sách câu hỏi
            int i = 1;
            foreach (var cauhoi in cauhoiList)
            {
                string dapAn = cauhoi.CauHoi.DapAnA + cauhoi.CauHoi.DapAnB + cauhoi.CauHoi.DapAnC + cauhoi.CauHoi.DapAnD;
                int doDaiDapAn = TinhDoDaiDapAn(cauhoi.CauHoi.DapAnA, cauhoi.CauHoi.DapAnB, cauhoi.CauHoi.DapAnC, cauhoi.CauHoi.DapAnD);

                string tableHtml = "<table>";
                tableHtml += "<tr>";
                tableHtml += $"<td colspan='4' style='text-align: left;'>{i}. {cauhoi.CauHoi.NoiDung}</td>";
                tableHtml += "</tr>";
                if (doDaiDapAn <= 50)
                {
                    tableHtml += "<tr>";
                    tableHtml += $"<td>A. {cauhoi.CauHoi.DapAnA}</td>";
                    tableHtml += $"<td>B. {cauhoi.CauHoi.DapAnB}</td>";
                    tableHtml += $"<td>C. {cauhoi.CauHoi.DapAnC}</td>";
                    tableHtml += $"<td>D. {cauhoi.CauHoi.DapAnD}</td>";
                    tableHtml += "</tr>";
                }
                else if (doDaiDapAn > 50 && doDaiDapAn <= 100)
                {
                    tableHtml += "<tr>";
                    tableHtml += $"<td colspan='2'>A. {cauhoi.CauHoi.DapAnA}</td>";
                    tableHtml += $"<td colspan='2'>B. {cauhoi.CauHoi.DapAnB}</td>";
                    tableHtml += "</tr>";
                    tableHtml += "<tr>";
                    tableHtml += $"<td colspan='2'>C. {cauhoi.CauHoi.DapAnC}</td>";
                    tableHtml += $"<td colspan='2'>D. {cauhoi.CauHoi.DapAnD}</td>";
                    tableHtml += "</tr>";
                }
                else
                {
                    tableHtml += "<tr>";
                    tableHtml += $"<td colspan='4'>A. {cauhoi.CauHoi.DapAnA}</td>";
                    tableHtml += "</tr>";
                    tableHtml += "<tr>";
                    tableHtml += $"<td colspan='4'>B. {cauhoi.CauHoi.DapAnB}</td>";
                    tableHtml += "</tr>";
                    tableHtml += "<tr>";
                    tableHtml += $"<td colspan='4'>C. {cauhoi.CauHoi.DapAnC}</td>";
                    tableHtml += "</tr>";
                    tableHtml += "<tr>";
                    tableHtml += $"<td colspan='4'>D. {cauhoi.CauHoi.DapAnD}</td>";
                    tableHtml += "</tr>";
                }
                tableHtml += "</table>";

                htmlcontent += tableHtml;
                i++;
            }
            //  Kết thúc HTML
            htmlcontent += @"</div>
              </body>
            </html>";
            var pdfStream = new MemoryStream();
            var pdfWriter = new PdfWriter(pdfStream);
            var pdfDocument = new PdfDocument(pdfWriter);
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            HtmlConverter.ConvertToPdf(htmlcontent, pdfStream);

            return new FileContentResult(pdfStream.ToArray(), "application/pdf");

        }
        static int TinhDoDaiDapAn(string dapAnA, string dapAnB, string dapAnC, string dapAnD)
        {
            string dapAn = dapAnA + dapAnB + dapAnC + dapAnD;
            return dapAn.Length;
        }
        public async Task<IActionResult> PdfDapAnViewer(int deId)
        {
            var kykiemtra = _context.KyKiemTra.FirstOrDefault(x => x.De.FirstOrDefault().DeId == deId);
            var document = new StringBuilder();
            var htmlcontent = @"<!DOCTYPE html>
                <html>

                <head>
                  <title>Đáp án thi trắc nghiệm</title>
                  <style>
                    .container {
                      max-width: 794px;
                      margin: 0 auto;
                      font-family: Helvetica;
                    }


                   .hedert {
                      height: 150px;
                      width: 370px;
                      float: left;
                      margin-top: 10px;
                    }

                    .hedert p {
                      font-size: 17px;
                    }

                    .hedert h4 {
                      padding-left: 50px;
                    }

                    .hedery {

                      text-align: center;
                    }

                    .mssv {
                      margin-left: 500px;
                      margin-top: -40px;
                    }
                    h1,
                    h2 {
                      text-align: center;
                    }

                    .cauhoi {
                      padding-left: 40px;
                    }

                    .cauhoi div {
                      padding-bottom: 40px;
                      font-size: 18px;
                      margin-top: -10px;
                    }

                    .cauhoi p {
                      font-size: 21px;
                    }

                  table {
                      border-collapse: collapse;
                      width: 10%;
                      border: 1px solid #ddd;
                      margin-bottom: 20px;
                      float: left;

                    }

                    td,
                    th {
                      padding: 5px;
                      text-align: center;
                      border: 1px solid #757575;
                      font-size: 16px;
                    }

                    th {
                      background-color: #f2f2f2;
                    }
                  </style>
                </head>

                <body>
                  <div class='container'>

                   <div style=' overflow: hidden;'>

                      <div class='hedert'>
                        <p> TRƯỜNG CAO ĐẲNG KỸ THUẬT CAO THẮNG</p>
                        <h4> KHOA CÔNG NGHỆ THÔNG TIN</h4>
                      </div>

                      <div class='hedery'>";
            htmlcontent += @$" <h2> ĐÁP ÁN {kykiemtra.TenKyKiemTra}</h2>
                          <p>Thời gian: {kykiemtra.ThoiGianLamBai} phút (không kể thời gian phát đề)</p>
                          <p>Sinh viên không được sử dụng tài liệu</p>
                      </div>
                    </div>


                    <div>
                      <h2>Môn: LẬP TRÌNH ỨNG DỤNG ASP.NET CORE</h2>
                      <h3 style='text-align: center;'>Hệ/Khóa: CĐTH ............. </h3>


                    </div>
                    <div style='font-size: 19px;overflow: hidden;margin-left: 200px;'>
                      <p style='float: left; padding-right: 30px;'>Ngày thi: {kykiemtra.ThoiGianBatDau.ToString("dd/MM/yyyy")}</p>
                      <p>Thời lượng: {kykiemtra.ThoiGianLamBai} phút</p>
                      <p style='margin-left: 150px;'>Mã đề: .........</p>
                    </div>
                    <h2>ĐÁN ÁN</h2>
                    <hr/>
                    <hr/>
                    <div style='overflow: hidden;'>
                ";

            //  Lấy danh sách câu hỏi từ bảng cauhoi_de
            var cauhoiList = await _context.CauHoi_De.Include(c => c.CauHoi).Where(c => c.DeId == deId).ToListAsync();

            if (cauhoiList == null || cauhoiList.Count == 0)
            {
                return BadRequest("Không tìm thấy câu hỏi nào");
            }

            //  Hiển thị từng câu hỏi và đáp áTo continue building the HTML code in C# for generating a PDF, you can add the following code after the existing HTML code:
            //  Tạo danh sách câu hỏi
            int i = 1;

            foreach (var cauhoi in cauhoiList)
            {
                htmlcontent += $@"

                    <table>
                        <tr>
                            <td style='text-align: center';>Câu {i}</td>

                        </tr>
                         <tr>

                             <td> {cauhoi.CauHoi.DapAnDung}</td>
                         </tr>


                    </table>
                  ";
                i++;
            }

            //  Kết thúc HTML
            htmlcontent += @" </div>
                        <div style='text-align: center;'>
                              <p>
                                -----Hết-----
                              </p>
                            </div >
                           <div style='overflow: hidden;'>
                            <p style='text-align: right;'>
                              Tp. HCM, ngày..... tháng..... năm 20.....

                            </p>
                           <h3 style='text-align: right;margin-right: 40px; float: right;margin-top: -3px;'>
                              Người lập đáp án
                            </h3>
                            <h3>	Người duyệt đáp án</h3>
                           </div>
                        </div>
              </body>
            </html>";
            // Khởi tạo HTML to PDF converter
            var pdfStream = new MemoryStream();
            var pdfWriter = new PdfWriter(pdfStream);
            var pdfDocument = new PdfDocument(pdfWriter);
            pdfDocument.SetDefaultPageSize(PageSize.A4);
            HtmlConverter.ConvertToPdf(htmlcontent, pdfStream);

            return new FileContentResult(pdfStream.ToArray(), "application/pdf");
        }
    }
}
