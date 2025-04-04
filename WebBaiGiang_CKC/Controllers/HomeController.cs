using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using X.PagedList;

namespace WebBaiGiang_CKC.Controllers
{
    public class HomeController : Controller
    {
        protected WebBaiGiangContext _context;
        protected readonly IWebHostEnvironment _environment;
        public INotyfService _notyfService { get; }

        public HomeController(WebBaiGiangContext context, IWebHostEnvironment environment, INotyfService notyfService)
        {
            _context = context;
            _environment = environment;
            _notyfService = notyfService;
        }

        // Trang danh sách môn học (mặc định)
        /*public IActionResult Index(int? page)
        {
            var forgotPasswordSuccess = HttpContext.Request.Cookies["forgotPasswordSuccess"];
            if (forgotPasswordSuccess != null && forgotPasswordSuccess == "true")
            {
                _notyfService.Success("Đổi mật khẩu thành công!");
                HttpContext.Response.Cookies.Delete("forgotPasswordSuccess");
            }

            int pageNo = page == null || page <= 0 ? 1 : page.Value;
            int pageSize = 8;

            // Lấy tất cả môn học để hiển thị trong menu
            var allMonHoc = _context.MonHoc.AsNoTracking().ToList();
            ViewBag.AllMonHoc = allMonHoc; // ✅ Lưu danh sách môn học vào ViewBag

            // Lấy danh sách môn học có phân trang
            var lstMon = _context.MonHoc
                .Include(a => a.Chuongs)
                .ThenInclude(x => x.Bais.OrderBy(x => x.SoBai))
                .AsNoTracking()
                .ToPagedList(pageNo, pageSize);

            ViewData["lstSubject"] = lstMon;
            return View(lstMon);
        }*/

        public IActionResult Index(int? page)
        {
            var forgotPasswordSuccess = HttpContext.Request.Cookies["forgotPasswordSuccess"];
            if (forgotPasswordSuccess != null && forgotPasswordSuccess == "true")
            {
                _notyfService.Success("Đổi mật khẩu thành công!");
                HttpContext.Response.Cookies.Delete("forgotPasswordSuccess");
            }

            int pageNo = page == null || page <= 0 ? 1 : page.Value;
            int pageSize = 8;

            // Lấy tất cả môn học để hiển thị trong menu
            var allMonHoc = _context.MonHoc
                .AsNoTracking()
                .ToList();
            ViewBag.AllMonHoc = allMonHoc;

            // Lấy danh sách môn học có phân trang, bao gồm thông tin về chương, bài và giảng viên
            var lstMon = _context.MonHoc
                .Include(m => m.GiaoVien) // Lấy thông tin giảng viên
                .Include(a => a.Chuongs)
                    .ThenInclude(c => c.Bais)
                .AsNoTracking()
                .ToPagedList(pageNo, pageSize);

            // Tính số lượng người đăng ký và số lượng bài học cho mỗi môn học
            foreach (var mon in lstMon)
            {
                mon.SoLuongDangKy = _context.DangKyMonHoc.Count(d => d.MonHocId == mon.MonHocId);
                mon.TongSoBai = mon.Chuongs.Sum(c => c.Bais.Count); // Tính tổng số bài học từ các chương
            }

            ViewData["lstSubject"] = lstMon;
            return View(lstMon);
        }


        // Chức năng tìm kiếm môn học
        public IActionResult SearchMonHoc(string keyword, int? page)
        {
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            // Lấy toàn bộ môn học để giữ nguyên danh sách trong menu
            var allMonHoc = _context.MonHoc.AsNoTracking().ToList();
            ViewBag.AllMonHoc = allMonHoc; // ✅ Đảm bảo menu luôn có dữ liệu

            // Lọc danh sách môn học theo từ khóa tìm kiếm
            var filteredMonHoc = _context.MonHoc
                .Where(m => string.IsNullOrEmpty(keyword) || m.TenMonHoc.Contains(keyword))
                .OrderBy(m => m.TenMonHoc)
                .AsNoTracking()
                .ToPagedList(pageNumber, pageSize);

            ViewData["SearchKeyword"] = keyword; // ✅ Giữ lại từ khóa tìm kiếm trong input
            return View("Index", filteredMonHoc);
        }

        // Hiển thị chi tiết môn học
        // Hiển thị chi tiết môn học và kiểm tra tình trạng đăng ký
        public IActionResult MonHoc(int id, DangKyMonHoc dkm)
        {
            var monHoc = _context.MonHoc
                .Include(m => m.Chuongs)
                .ThenInclude(c => c.Bais.OrderBy(b => b.SoBai))
                .AsNoTracking()
                .FirstOrDefault(m => m.MonHocId == id);

            if (monHoc == null)
            {
                return NotFound();
            }

            // Lấy đề cương môn học
            var deCuong = _context.DeCuong.FirstOrDefault(d => d.MonHocId == id);
            ViewBag.DeCuong = deCuong;

            // Kiểm tra nếu người dùng đã đăng ký môn học
            var taiKhoanIdClaim = User.Claims.SingleOrDefault(c => c.Type == "TaiKhoanId");
            if (taiKhoanIdClaim != null)
            {
                // Chuyển đổi từ string sang int
                if (int.TryParse(taiKhoanIdClaim.Value, out int userTaiKhoanId))
                {
                    // Kiểm tra xem người dùng đã đăng ký môn học chưa
                    var isRegistered = _context.DangKyMonHoc
                        .Any(dkm => dkm.TaiKhoanId == userTaiKhoanId && dkm.MonHocId == id);

                    ViewBag.IsRegistered = isRegistered; // Truyền thông tin đăng ký vào ViewBag
                }
                else
                {
                    // Nếu không thể chuyển đổi TaiKhoanId, xử lý lỗi (nếu cần)
                    ViewBag.IsRegistered = false;
                }
            }


            ViewBag.SelectedMonHoc = monHoc;
            return View(monHoc);
        }

        [HttpPost]
        public IActionResult RegisterMonHoc(int id)
        {
            // Kiểm tra xem người dùng đã đăng nhập hay chưa
            var taiKhoanIdClaim = User.Claims.SingleOrDefault(c => c.Type == "TaiKhoanId");
            if (taiKhoanIdClaim == null)
            {
                _notyfService.Warning("Bạn cần đăng nhập để đăng ký môn học!");
                return RedirectToAction("Index");
            }

            // Chuyển đổi TaiKhoanId từ string sang int
            if (!int.TryParse(taiKhoanIdClaim.Value, out int userTaiKhoanId))
            {
                _notyfService.Error("Lỗi xác thực tài khoản!");
                return RedirectToAction("Index");
            }

            // Kiểm tra xem môn học có tồn tại không
            var monHoc = _context.MonHoc.AsNoTracking().FirstOrDefault(m => m.MonHocId == id);
            if (monHoc == null)
            {
                _notyfService.Error("Môn học không tồn tại!");
                return RedirectToAction("Index");
            }

            // Kiểm tra xem người dùng đã đăng ký môn học này chưa
            bool isAlreadyRegistered = _context.DangKyMonHoc
                .Any(dkm => dkm.TaiKhoanId == userTaiKhoanId && dkm.MonHocId == id);

            if (isAlreadyRegistered)
            {
                _notyfService.Warning("Bạn đã đăng ký môn học này rồi!");
                return RedirectToAction("MonHoc", new { id });
            }

            // Thêm đăng ký vào bảng DangKyMonHoc
            var newRegistration = new DangKyMonHoc
            {
                TaiKhoanId = userTaiKhoanId,
                MonHocId = id
            };

            _context.DangKyMonHoc.Add(newRegistration);
            _context.SaveChanges();

            _notyfService.Success("Đăng ký môn học thành công!");
            return RedirectToAction("MonHoc", new { id });
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userIdClaim = User.Claims.SingleOrDefault(c => c.Type == "TaiKhoanId"); // Lấy ID tài khoản từ Claim
            if (userIdClaim != null)
            {
                int taiKhoanId = int.Parse(userIdClaim.Value); // Chuyển về kiểu int

                // Lấy danh sách môn học mà người dùng đã đăng ký
                var lstMon = _context.DangKyMonHoc
                    .Where(dkm => dkm.TaiKhoanId == taiKhoanId)
                    .Select(dkm => dkm.MonHoc)
                    .AsNoTracking()
                    .ToList();

                ViewBag.MonHocDangKy = lstMon;  // Để dùng trong header
                ViewData["lstSubject"] = lstMon; // Nếu cần dùng ở nhiều view
            }
            else
            {
                ViewBag.MonHocDangKy = new List<MonHoc>(); // Nếu chưa đăng nhập, không hiển thị môn nào
            }

            base.OnActionExecuting(filterContext);
        }


    }
}
