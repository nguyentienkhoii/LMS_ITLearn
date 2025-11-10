using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using X.PagedList;

namespace WebBaiGiang_CKC.Controllers
{
    [Authorize(Roles = "HocVien,GiangVien,Admin")]
    public class HomeController : Controller
    {
        protected WebBaiGiangContext _context;
        private readonly IWebHostEnvironment _environment;
        protected INotyfService _notyfService;

        public HomeController(WebBaiGiangContext context, IWebHostEnvironment environment, INotyfService notyfService)
        {
            _context = context;
            _environment = environment;
            _notyfService = notyfService;
        }

        // --------------------------
        // TRANG CHỦ: Hiển thị danh sách lớp học
        // --------------------------
        public IActionResult Index(int? page)
        {
            ViewBag.ActiveMenu = "TrangChu";
            var forgotPasswordSuccess = HttpContext.Request.Cookies["forgotPasswordSuccess"];
            if (forgotPasswordSuccess == "true")
            {
                _notyfService.Success("Đổi mật khẩu thành công!");
                HttpContext.Response.Cookies.Delete("forgotPasswordSuccess");
            }

            int pageNo = page ?? 1;
            int pageSize = 8;

            // ✅ Nếu là Admin → hiển thị tất cả lớp học
            if (User.IsInRole("Admin"))
            {
                var allLop = _context.LopHocs
                    .Include(l => l.GiangVien)
                    .Include(l => l.KhoaHoc)
                    .OrderByDescending(l => l.MaLopHoc)
                    .AsNoTracking()
                    .ToPagedList(pageNo, pageSize);

                foreach (var lop in allLop)
                {
                    lop.HocVien_LopHocs = _context.HocVien_LopHoc
                        .Where(d => d.MaLopHoc == lop.MaLopHoc)
                        .ToList();
                }

                ViewData["lstLopHoc"] = allLop;
                return View(allLop);
            }

            // ✅ Nếu là Học viên → chỉ hiển thị lớp đã đăng ký
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null || !int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                _notyfService.Error("Không thể xác định học viên!");
                return RedirectToAction("Index", "Account");
            }

            var lopHocCuaToi = _context.HocVien_LopHoc
                .Where(hv => hv.MaHocVien == hocVienId)
                .Include(hv => hv.LopHoc)
                    .ThenInclude(l => l.GiangVien)
                .Include(hv => hv.LopHoc)
                    .ThenInclude(l => l.KhoaHoc)
                .Select(hv => hv.LopHoc)
                .OrderByDescending(l => l.MaLopHoc)
                .AsNoTracking()
                .ToPagedList(pageNo, pageSize);

            //thông báo m chưa đki lớp nào
            /*if (!lopHocCuaToi.Any())
            {
                _notyfService.Information("Bạn chưa đăng ký lớp học nào.");
            }
*/
            ViewData["lstLopHoc"] = lopHocCuaToi;
            return View(lopHocCuaToi);
        }

        // --------------------------
        // TÌM KIẾM LỚP HỌC
        // --------------------------
        public IActionResult SearchLopHoc(string keyword, int? page)
        {
            int pageSize = 8;
            int pageNumber = page ?? 1;

            var filteredLopHoc = _context.LopHocs
                .Include(l => l.GiangVien)
                .Include(l => l.KhoaHoc)
                .Where(l => string.IsNullOrEmpty(keyword) || l.TenLopHoc.Contains(keyword))
                .OrderBy(l => l.TenLopHoc)
                .AsNoTracking()
                .ToPagedList(pageNumber, pageSize);

            ViewBag.ActiveMenu = "TrangChu";
            ViewData["SearchKeyword"] = keyword;
            return View("Index", filteredLopHoc);
        }

        // --------------------------
        // CHI TIẾT LỚP HỌC
        // --------------------------
        public IActionResult LopHoc(int id)
        {
            var lopHoc = _context.LopHocs
                .Include(l => l.Chuongs)
                    .ThenInclude(c => c.Bais.OrderBy(b => b.SoBai))
                .Include(l => l.GiangVien)
                .AsNoTracking()
                .FirstOrDefault(l => l.MaLopHoc == id);

            if (lopHoc == null)
                return NotFound();

            // ✅ Lấy mã học viên từ claim
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim != null && int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                bool isRegistered = _context.HocVien_LopHoc
                    .Any(d => d.MaHocVien == hocVienId && d.MaLopHoc == id);

                ViewBag.IsRegistered = isRegistered;
            }

            ViewBag.ActiveMenu = "TrangChu";
            ViewBag.ActiveMenu = "LopHoc";        // xác định menu chính là lớp học
            ViewBag.CurrentLopHocId = id;

            return View(lopHoc);
        }

        // --------------------------
        // ĐĂNG KÝ LỚP HỌC
        // --------------------------
        [HttpPost]
        public IActionResult RegisterLopHoc(int id)
        {
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim == null)
            {
                _notyfService.Warning("Bạn cần đăng nhập để đăng ký lớp học!");
                return RedirectToAction("Index");
            }

            if (!int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                _notyfService.Error("Lỗi xác thực học viên!");
                return RedirectToAction("Index");
            }

            var lopHoc = _context.LopHocs.AsNoTracking().FirstOrDefault(l => l.MaLopHoc == id);
            if (lopHoc == null)
            {
                _notyfService.Error("Lớp học không tồn tại!");
                return RedirectToAction("Index");
            }

            bool isAlreadyRegistered = _context.HocVien_LopHoc
                .Any(d => d.MaHocVien == hocVienId && d.MaLopHoc == id);

            if (isAlreadyRegistered)
            {
                _notyfService.Warning("Bạn đã đăng ký lớp học này rồi!");
                return RedirectToAction("LopHoc", new { id });
            }

            var newRegistration = new HocVien_LopHoc
            {
                MaHocVien = hocVienId,
                MaLopHoc = id
            };

            _context.HocVien_LopHoc.Add(newRegistration);
            _context.SaveChanges();

            _notyfService.Success("Đăng ký lớp học thành công!");
            return RedirectToAction("LopHoc", new { id });
        }

        // --------------------------
        // DANH SÁCH LỚP HỌC ĐÃ ĐĂNG KÝ (HEADER)
        // --------------------------
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var hocVienIdClaim = User.Claims.SingleOrDefault(c => c.Type == "HocVienId");
            if (hocVienIdClaim != null && int.TryParse(hocVienIdClaim.Value, out int hocVienId))
            {
                var lstLopHoc = _context.HocVien_LopHoc
                    .Where(d => d.MaHocVien == hocVienId)
                    .Include(d => d.LopHoc)
                        .ThenInclude(l => l.GiangVien)
                    .Select(d => d.LopHoc)
                    .AsNoTracking()
                    .ToList();

                ViewBag.LopHocDangKy = lstLopHoc;
                ViewData["lstLopHoc"] = lstLopHoc;
            }
            else
            {
                ViewBag.LopHocDangKy = new List<LopHoc>();
            }

            base.OnActionExecuting(filterContext);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
