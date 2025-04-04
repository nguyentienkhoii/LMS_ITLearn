using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Models;
using X.PagedList;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly WebBaiGiangContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TaiKhoanController> _logger;
        private readonly INotyfService _notyfService;

        public TaiKhoanController(WebBaiGiangContext context, INotyfService notyfService, IConfiguration configuration, ILogger<TaiKhoanController> logger)
        {
            _context = context;
            _notyfService = notyfService;
            _configuration = configuration;
            _logger = logger;
        }

        // GET: Admin/TaiKhoan
        public IActionResult Index(int? page)
        {

            var _customer = from m in _context.TaiKhoan select m;
            var pageNo = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 12;
            PagedList<TaiKhoan> models = new PagedList<TaiKhoan>(_customer, pageNo, pageSize);
            return View(models);
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
                var mainPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Files");
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

                // Tạo DataTable và đọc dữ liệu từ tệp Excel vào
                DataTable dt = new DataTable();
                using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        dt.Columns.Add(firstRowCell.Text);
                    }
                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                        var newRow = dt.Rows.Add();
                        foreach (var cell in row)
                        {
                            newRow[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                }

                // Thực hiện các kiểm tra và chuẩn hóa dữ liệu
                foreach (DataRow row in dt.Rows)
                {
                    var mssv = row["MSSV"].ToString().Trim();
                    var email = row["Email"].ToString().Trim();
                    var hoTen = row["HoTen"].ToString();

                    // Viết hoa chữ cái đầu của tên
                    hoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(hoTen.ToLower());

                    // Kiểm tra MSSV đã tồn tại trong cơ sở dữ liệu
                    var student = _context.TaiKhoan.FirstOrDefault(x => x.MSSV == mssv);
                    if (student != null)
                    {
                        _notyfService.Error($"MSSV {mssv} đã tồn tại trong cơ sở dữ liệu!");
                        return RedirectToAction("Index");
                    }

                    //  Kiểm tra Email đã tồn tại trong cơ sở dữ liệu
                    student = _context.TaiKhoan.FirstOrDefault(x => x.Email == email);
                    if (student != null)
                    {
                        _notyfService.Error($"Email {email} đã tồn tại trong cơ sở dữ liệu!");
                        return RedirectToAction("Index");
                    }

                    row["HoTen"] = hoTen; // gán giá trị mới cho cột Họ tên
                    row["MatKhau"] = HashMD5.ToMD5(row["MatKhau"].ToString().Trim()); // mã hóa mật khẩu bằng MD5
                    row["TrangThai"] = true; // mặc định là true
                }
                var conString = _configuration.GetConnectionString("WebBaiGiang");
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conString))
                {
                    sqlBulkCopy.DestinationTableName = "TaiKhoan";
                    foreach (DataColumn column in dt.Columns)
                    {
                        sqlBulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }
                    sqlBulkCopy.WriteToServer(dt);

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
        // GET: Admin/TaiKhoan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TaiKhoan == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(m => m.TaiKhoanId == id);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            return View(taiKhoan);
        }

        // GET: Admin/TaiKhoan/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/TaiKhoan/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TaiKhoanId,MSSV,MatKhau,Email,HoTen,TrangThai")] TaiKhoan taiKhoan)
        {
            if (ModelState.IsValid)
            {
                taiKhoan.HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(taiKhoan.HoTen);

                // Kiểm tra xem MSSV đã tồn tại trong cơ sở dữ liệu hay chưa
                if (_context.TaiKhoan.Any(a => a.MSSV == taiKhoan.MSSV))
                {
                    ModelState.AddModelError("MSSV", "MSSV đã tồn tại trong hệ thống.");
                    return View(taiKhoan);
                }

                // Kiểm tra xem Email đã tồn tại trong cơ sở dữ liệu hay chưa
                if (_context.TaiKhoan.Any(a => a.Email == taiKhoan.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                    return View(taiKhoan);
                }
                taiKhoan.MatKhau = taiKhoan.MatKhau.ToMD5();
                _context.Add(taiKhoan);
                _notyfService.Success("Thêm thành công!");
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(taiKhoan);
        }

        // GET: Admin/TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TaiKhoan == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan == null)
            {
                return NotFound();
            }
            return View(taiKhoan);
        }

        // POST: Admin/TaiKhoan/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TaiKhoanId,MSSV,Email,HoTen,TrangThai")] TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.TaiKhoanId)
            {
                return NotFound();
            }

            try
            {
                taiKhoan.HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(taiKhoan.HoTen);
                var existingAccount = await _context.TaiKhoan.FindAsync(id);
                if (existingAccount == null)
                {
                    return NotFound();
                }

                if (await _context.TaiKhoan.AnyAsync(x => x.MSSV == taiKhoan.MSSV && x.TaiKhoanId != taiKhoan.TaiKhoanId))
                {
                    ModelState.AddModelError("MSSV", "MSSV đã tồn tại trong hệ thống.");
                    return View(taiKhoan);
                }

                if (await _context.TaiKhoan.AnyAsync(x => x.Email == taiKhoan.Email && x.TaiKhoanId != taiKhoan.TaiKhoanId))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại trong hệ thống.");
                    return View(taiKhoan);
                }

                taiKhoan.MatKhau = existingAccount.MatKhau;
                _notyfService.Success("Sửa thành công!");
                _context.Entry(existingAccount).State = EntityState.Detached;
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaiKhoanExists(taiKhoan.TaiKhoanId))
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
        // GET: Admin/TaiKhoan/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TaiKhoan == null)
            {
                return NotFound();
            }

            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(m => m.TaiKhoanId == id);
            if (taiKhoan == null)
            {
                return NotFound();
            }

            return View(taiKhoan);
        }

        // POST: Admin/TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TaiKhoan == null)
            {
                return Problem("Entity set 'BaiGiangContext.TaiKhoan'  is null.");
            }
            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan != null)
            {
                _context.TaiKhoan.Remove(taiKhoan);
            }
            _notyfService.Success("Xóa thành công!");
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoan.Any(e => e.TaiKhoanId == id);
        }
        public IActionResult DownloadExcel()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "UpLoads","Files", "ImportTaiKhoan.xlsx");
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
