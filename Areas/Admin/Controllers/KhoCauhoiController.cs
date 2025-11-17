using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using X.PagedList;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class KhoCauHoiController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IConfiguration _configuration;
        public INotyfService _notyfService { get; }

        public KhoCauHoiController(WebBaiGiangContext context, IConfiguration configuration, INotyfService notyfService)
        {
            _context = context;
            _configuration = configuration;
            _notyfService = notyfService;
        }

        // ====================== INDEX ======================
        public IActionResult Index(int? page)
        {
            var list = _context.CauHoi
                .Include(k => k.ChuongNew)
                .ThenInclude(c => c.LopHoc)  
                .OrderByDescending(c => c.CauHoiId);
            int pageNo = page ?? 1;
            int pageSize = 12;
            PagedList<CauHoi> models = new PagedList<CauHoi>(list, pageNo, pageSize);
            return View(models);
        }

        // ====================== DETAILS ======================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cauHoi = await _context.CauHoi
                .Include(k => k.ChuongNew)
                .FirstOrDefaultAsync(m => m.CauHoiId == id);
            if (cauHoi == null) return NotFound();

            return View(cauHoi);
        }

        // ====================== CREATE ======================
        public IActionResult Create()
        {
            ViewData["LopHoc"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc");
            ViewData["MaChuong"] = new SelectList(Enumerable.Empty<SelectListItem>()); // ban đầu trống
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CauHoiId,MaChuong,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,DoKho,SoLanLay,SoLanTraLoiDung")] CauHoi cauHoi)
        {
            if (cauHoi.MaChuong == 0)
            {
                _notyfService.Error("Vui lòng chọn chương học!");
            }
            else if (ModelState.IsValid)
            {
                _context.Add(cauHoi);
                await _context.SaveChangesAsync();
                _notyfService.Success("Thêm câu hỏi thành công!");
                return RedirectToAction(nameof(Index));
            }
            ViewData["LopHoc"] = new SelectList(_context.LopHocs, "MaLopHoc", "TenLopHoc");
            ViewData["MaChuong"] = new SelectList(_context.ChuongNews, "MaChuong", "TenChuong", cauHoi.MaChuong);
            return View(cauHoi);
        }

        // ====================== IMPORT DANH SÁCH CÂU HỎI ======================
        public IActionResult CreateList() => View();

        [HttpPost]
        public IActionResult CreateList(IFormFile formFile)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                List<int> maChuongList = new List<int>();
                string conStr = _configuration.GetConnectionString("WebBaiGiang");
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    string query = "SELECT MaChuong FROM CHUONG_NEW";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                maChuongList.Add(reader.GetInt32(reader.GetOrdinal("MaChuong")));
                            }
                        }
                        con.Close();
                    }
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Files");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                var filePath = Path.Combine(uploadDir, $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}");
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }

                FileInfo fileInfo = new FileInfo(filePath);
                using (ExcelPackage package = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    DataTable dt = new DataTable();

                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        dt.Columns.Add(firstRowCell.Text);

                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var excelRow = worksheet.Cells[row, 1, row, worksheet.Dimension.End.Column];
                        var newRow = dt.Rows.Add();
                        foreach (var cell in excelRow)
                            newRow[cell.Start.Column - 1] = cell.Text;
                    }

                    // Kiểm tra dữ liệu khóa ngoại
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!maChuongList.Contains(Convert.ToInt32(row["MaChuong"])))
                        {
                            _notyfService.Warning("Sai mã chương hoặc chương chưa được tạo!");
                            return RedirectToAction("Index");
                        }
                        row["DapAnDung"] = row["DapAnDung"].ToString().ToUpper();
                    }

                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                        {
                            sqlBulkCopy.DestinationTableName = "CAUHOI";
                            sqlBulkCopy.ColumnMappings.Add("MaChuong", "MaChuong");
                            sqlBulkCopy.ColumnMappings.Add("NoiDung", "NoiDung");
                            sqlBulkCopy.ColumnMappings.Add("DapAnA", "DapAnA");
                            sqlBulkCopy.ColumnMappings.Add("DapAnB", "DapAnB");
                            sqlBulkCopy.ColumnMappings.Add("DapAnC", "DapAnC");
                            sqlBulkCopy.ColumnMappings.Add("DapAnD", "DapAnD");
                            sqlBulkCopy.ColumnMappings.Add("DapAnDung", "DapAnDung");
                            sqlBulkCopy.ColumnMappings.Add("DoKho", "DoKho");
                            sqlBulkCopy.ColumnMappings.Add("SoLanLay", "SoLanLay");
                            sqlBulkCopy.ColumnMappings.Add("SoLanTraLoiDung", "SoLanTraLoiDung");

                            con.Open();
                            sqlBulkCopy.WriteToServer(dt);
                            con.Close();
                        }
                    }

                    _notyfService.Success("Nhập danh sách câu hỏi thành công!");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                _notyfService.Error("Có lỗi khi nhập danh sách câu hỏi!");
                return RedirectToAction("Index");
            }
        }

        // ====================== EDIT ======================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cauHoi = await _context.CauHoi.FindAsync(id);
            if (cauHoi == null) return NotFound();

            ViewData["MaChuong"] = new SelectList(_context.ChuongNews, "MaChuong", "TenChuong", cauHoi.MaChuong);
            return View(cauHoi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CauHoiId,MaChuong,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,DoKho,SoLanLay,SoLanTraLoiDung")] CauHoi cauHoi)
        {
            if (id != cauHoi.CauHoiId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (cauHoi.MaChuong == 0)
                    {
                        _notyfService.Error("Vui lòng chọn chương học!");
                    }
                    else
                    {
                        _context.Update(cauHoi);
                        await _context.SaveChangesAsync();
                        _notyfService.Success("Cập nhật thành công!");
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.CauHoi.Any(e => e.CauHoiId == cauHoi.CauHoiId))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["MaChuong"] = new SelectList(_context.ChuongNews, "MaChuong", "TenChuong", cauHoi.MaChuong);
            return View(cauHoi);
        }

        // ====================== DELETE ======================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cauHoi = await _context.CauHoi
                .Include(k => k.ChuongNew)
                .FirstOrDefaultAsync(m => m.CauHoiId == id);
            if (cauHoi == null) return NotFound();

            return View(cauHoi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cauHoi = await _context.CauHoi.FindAsync(id);
            if (cauHoi != null)
            {
                _context.CauHoi.Remove(cauHoi);
                await _context.SaveChangesAsync();
                _notyfService.Success("Xóa câu hỏi thành công!");
            }
            return RedirectToAction(nameof(Index));
        }

        // ====================== DOWNLOAD MẪU EXCEL ======================
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Files", "ImportCauHoi.xlsx");
            if (!System.IO.File.Exists(filePath))
            {
                _notyfService.Error("Không tìm thấy file mẫu!");
                return RedirectToAction(nameof(Index));
            }

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(filePath));
        }

        // API: Lấy danh sách chương theo lớp học
        [HttpGet]
        public JsonResult GetChuongByLop(int maLop)
        {
            var chuongList = _context.ChuongNews
                .Where(c => c.MaLopHoc == maLop)
                .Select(c => new { c.MaChuong, c.TenChuong })
                .ToList();

            return Json(chuongList);
        }

    }
}
