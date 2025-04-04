using AspNetCoreHero.ToastNotification.Abstractions;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using System.Reflection;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class KyKiemTraController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }
        private readonly IConfiguration _configuration;
        public KyKiemTraController(WebBaiGiangContext context, INotyfService notyfService, IConfiguration configuration)
        {
            _context = context;
            _notyfService = notyfService;
            _configuration = configuration;
        }

        // GET: Admin/KyKiemTras
        public async Task<IActionResult> Index()
        {
            return View(await _context.KyKiemTra.ToListAsync());
        }


        // GET: Admin/KyKiemTras/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.KyKiemTra == null)
            {
                return NotFound();
            }

            var kyKiemTra = await _context.KyKiemTra
                .FirstOrDefaultAsync(m => m.KyKiemTraId == id);
            if (kyKiemTra == null)
            {
                return NotFound();
            }

            return View(kyKiemTra);
        }

        // GET: Admin/KyKiemTras/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/KyKiemTras/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("KyKiemTraId,TenKyKiemTra,SoCauHoi,ThoiGianBatDau,ThoiGianKetThuc,ThoiGianLamBai")] KyKiemTra kyKiemTra)
        {


            if (ModelState.IsValid)
            {
                kyKiemTra.TenKyKiemTra = CultureInfo.CurrentCulture.TextInfo.ToUpper(kyKiemTra.TenKyKiemTra);
                _context.Add(kyKiemTra);
                _notyfService.Success("Thêm thành công!");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kyKiemTra);
        }
        public IActionResult CreateList()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateList(IFormFile formFile)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var mainPath = Path.Combine(Directory.GetCurrentDirectory(), "UpLoads", "Files");
                if (!Directory.Exists(mainPath))
                {
                    Directory.CreateDirectory(mainPath);
                }

                var filePath = Path.Combine(mainPath, $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}");

                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }
                var fileName = formFile.FileName;
                string extension = Path.GetExtension(fileName);

                DataTable dt = new DataTable();
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet != null)
                    {
                        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        {
                            dt.Columns.Add(firstRowCell.Text.Trim());
                        }

                        for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                            var newRow = dt.Rows.Add();
                            foreach (var cell in row)
                            {
                                newRow[cell.Start.Column - 1] = cell.Value?.ToString().Trim();
                            }
                        }
                    }
                }
               var conString = _configuration.GetConnectionString("WebBaiGiang");
                using (SqlConnection con = new SqlConnection(conString))
                {
                    con.Open();
                    foreach (DataRow row in dt.Rows)
                    {
                        var kykiemtraid = row["KyKiemTraId"].ToString();
                        var queryKyKiemTra = "SELECT COUNT(*) FROM KyKiemTra WHERE KyKiemTraId = @KyKiemTraId";
                        using (SqlCommand cmdKyKiemTra = new SqlCommand(queryKyKiemTra, con))
                        {
                            cmdKyKiemTra.Parameters.AddWithValue("@KyKiemTraId", kykiemtraid);
                            int kyKiemTraCount = (int)cmdKyKiemTra.ExecuteScalar();
                            if (kyKiemTraCount == 0)
                            {
                                _notyfService.Error($"Kỳ kiểm tra {kykiemtraid} chưa được tạo. Vui lòng tạo kỳ kiểm tra trước khi import danh sách thi!");
                                return RedirectToAction("Index");
                            }
                        }
                        var taikhoanid = row["TaiKhoanId"].ToString();
                        var queryTaiKhoan = "SELECT MSSV FROM TaiKhoan WHERE TaiKhoanId = @TaiKhoanId";
                        using (SqlCommand cmdTaiKhoan = new SqlCommand(queryTaiKhoan, con))
                        {
                            cmdTaiKhoan.Parameters.AddWithValue("@TaiKhoanId", taikhoanid);
                            using (SqlDataReader readerTaiKhoan = cmdTaiKhoan.ExecuteReader())
                            {
                                if (!readerTaiKhoan.Read())
                                {
                                    _notyfService.Error($"Tài khoản {taikhoanid} chưa có tài khoản. Vui lòng tạo tài khoản trước khi import danh sách thi!");
                                    return RedirectToAction("Index");
                                }
                            }
                        }

                        var queryDanhSachThi = "SELECT DST.TaiKhoanId, TK.MSSV FROM DanhSachThi DST JOIN TaiKhoan TK ON DST.TaiKhoanId = TK.TaiKhoanId WHERE DST.TaiKhoanId = @TaiKhoanId";
                        using (SqlCommand cmdDanhSachThi = new SqlCommand(queryDanhSachThi, con))
                        {
                            cmdDanhSachThi.Parameters.AddWithValue("@TaiKhoanId", taikhoanid);
                            using (SqlDataReader readerDanhSachThi = cmdDanhSachThi.ExecuteReader())
                            {
                                if (readerDanhSachThi.Read())
                                {
                                    var mssv = readerDanhSachThi["MSSV"].ToString();
                                    _notyfService.Warning($"Tài khoản {taikhoanid} ({mssv}) đã tồn tại trong danh sách thi!");
                                    return RedirectToAction("Index");
                                }
                            }
                        }
                    }

                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        sqlBulkCopy.DestinationTableName = "DanhSachThi";
                        sqlBulkCopy.ColumnMappings.Add("TaiKhoanId", "TaiKhoanId");
                        sqlBulkCopy.ColumnMappings.Add("KyKiemTraId", "KyKiemTraId");
                        sqlBulkCopy.ColumnMappings.Add("TrangThai", "TrangThai");
                        sqlBulkCopy.WriteToServer(dt);
                        con.Close();
                    }
                }
                _notyfService.Success("Thêm Thành Công!");
                return RedirectToAction("Index");

            }
            catch (Exception)
            {
                _notyfService.Error("Thêm Thất Bại!");
            }


            return RedirectToAction("Index");
        }
        // GET: Admin/KyKiemTras/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.KyKiemTra == null)
            {
                return NotFound();
            }

            var kyKiemTra = await _context.KyKiemTra.FindAsync(id);
            if (kyKiemTra == null)
            {
                return NotFound();
            }


            return View(kyKiemTra);
        }
        // POST: Admin/KyKiemTras/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("KyKiemTraId,TenKyKiemTra,SoCauHoi,ThoiGianBatDau,ThoiGianKetThuc,ThoiGianLamBai")] KyKiemTra kyKiemTra)
        {
            if (id != kyKiemTra.KyKiemTraId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    kyKiemTra.TenKyKiemTra = CultureInfo.CurrentCulture.TextInfo.ToUpper(kyKiemTra.TenKyKiemTra);
                    _context.Update(kyKiemTra);
                    _notyfService.Success("Cập nhật thành công!");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KyKiemTraExists(kyKiemTra.KyKiemTraId))
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


            return View(kyKiemTra);
        }

        // GET: Admin/KyKiemTras/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.KyKiemTra == null)
            {
                return NotFound();
            }

            var kyKiemTra = await _context.KyKiemTra
                .FirstOrDefaultAsync(m => m.KyKiemTraId == id);
            if (kyKiemTra == null)
            {
                return NotFound();
            }

            return View(kyKiemTra);
        }

        // POST: Admin/KyKiemTras/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.KyKiemTra == null)
            {
                return Problem("Entity set 'BaiGiangContext.KyKiemTras'  is null.");
            }
            var kyKiemTra = await _context.KyKiemTra.FindAsync(id);
            if (kyKiemTra != null)
            {
                _context.KyKiemTra.Remove(kyKiemTra);
            }
            _notyfService.Success("Xóa thành công!");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KyKiemTraExists(int id)
        {
            return _context.KyKiemTra.Any(e => e.KyKiemTraId == id);
        }
        public IActionResult ExportExcel(int kykiemtraid)
        {
            try
            {
                var kykiemtra = _context.KyKiemTra.FirstOrDefault(k => k.KyKiemTraId == kykiemtraid);
                var data = _context.BaiLam
                     .Include(x => x.CauHoi_BaiLam)
                    .ThenInclude(x => x.CauHoi_De)
                    .ThenInclude(x => x.De)
                       .Where(x => x.CauHoi_BaiLam.First().CauHoi_De.De.KyKiemTraId == kykiemtraid)
                       .ToList();
                if (data != null && data.Count() > 0)
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(ToConvertDataTable(data.ToList()));

                        using (MemoryStream stream = new MemoryStream())
                        {

                            wb.SaveAs(stream);
                            string fileName = $"BaiThiSinhVien_{kykiemtra.TenKyKiemTra}_{DateTime.Now.ToString("dd/MM/yyyy")}.xlsx";
                            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocuments.spreadsheetml.sheet", fileName);
                        }

                    }

                }
                _notyfService.Error("Kỳ kiểm tra này không có điểm của sinh viên");
            }
            catch (Exception)
            {
                _notyfService.Error("Xuất Excel Thất Bại!");
            }
            return RedirectToAction("Index");
        }
       
        public DataTable ToConvertDataTable<T>(List<T> items)
        {
            DataTable dt = new DataTable(typeof(T).Name);
            PropertyInfo[] propInfo = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<string> columnsToExport = new List<string> { "HoTen", "MSSV", "SoCauDung", "Diem", "ThoiGianBatDau", "ThoiGianDenHan" };
            foreach (PropertyInfo prop in propInfo)
            {
                if (columnsToExport.Contains(prop.Name))
                {
                    dt.Columns.Add(prop.Name);
                }
            }
            foreach (T item in items)
            {
                var values = new object[columnsToExport.Count];
                int j = 0;
                for (int i = 0; i < propInfo.Length; i++)
                {
                    if (columnsToExport.Contains(propInfo[i].Name))
                    {
                        values[j] = propInfo[i].GetValue(item, null);
                        j++;
                    }
                }
                dt.Rows.Add(values);
            }
            return dt;
        }
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UpLoads", "Files", "ImportDanhSachThi.xlsx");
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", Path.GetFileName(filePath));
        }
    }
}
