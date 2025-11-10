/*using System.Globalization;
using AspNetCoreHero.ToastNotification.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<AuthenticationController> _logger;
        public INotyfService _notyfService { get; }

        public AuthenticationController(WebBaiGiangContext context, ILogger<AuthenticationController> logger, INotyfService notyfService)
        {
            _context = context;
            _logger = logger;
            _notyfService = notyfService;
        }

        #region --- Học viên ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var password = request.Password.ToMD5();
            var user = await _context.TaiKhoanNews
                .Include(t => t.HocVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == request.TenDangNhap && t.MatKhau == password && t.VaiTro == "HocVien");

            if (user == null)
                return BadRequest("Thông tin đăng nhập không chính xác hoặc tài khoản bị khóa.");

            if (!user.TrangThai)
                return BadRequest("Tài khoản đang bị khóa.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.HocVien.HoTen),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("VaiTro", "HocVien"),
                new Claim("MaHocVien", user.HocVien.MaHocVien.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            _notyfService.Success("Đăng nhập thành công");
            return Ok(new { message = "Đăng nhập thành công" });
        }

        *//*[HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MSSV) || string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.HoTen))
            {
                return BadRequest("Vui lòng nhập đầy đủ thông tin.");
            }

            if (request.Mssv.Length < 6 || request.Mssv.Length > 20)
                return BadRequest("Tên đăng nhập phải từ 6 đến 20 ký tự.");
            if (request.Password.Length < 6 || request.Password.Length > 50)
                return BadRequest("Mật khẩu phải từ 6 đến 50 ký tự.");

            if (await _context.TaiKhoanNew.AnyAsync(t => t.TenDangNhap == request.Mssv))
                return BadRequest("Tên đăng nhập đã tồn tại.");
            if (await _context.HocVien.AnyAsync(h => h.Email == request.Email))
                return BadRequest("Email đã được sử dụng.");

            var taiKhoan = new TaiKhoanNew
            {
                TenDangNhap = request.Mssv,
                MatKhau = request.Password.ToMD5(),
                VaiTro = "HocVien",
                TrangThai = true
            };

            var hocVien = new HocVien
            {
                HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.HoTen),
                Email = request.Email,
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TaiKhoan = taiKhoan
            };

            _context.TaiKhoanNew.Add(taiKhoan);
            _context.HocVien.Add(hocVien);
            await _context.SaveChangesAsync();

            _notyfService.Success("Đăng ký thành công");
            return Ok(new { message = "Đăng ký thành công" });
        }
*//*
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "TenDangNhap")?.Value;
            if (username == null) return BadRequest("Không tìm thấy tài khoản.");

            var user = await _context.TaiKhoanNews.FirstOrDefaultAsync(t => t.TenDangNhap == username && t.VaiTro == "HocVien");
            if (user == null) return BadRequest("Tài khoản không tồn tại.");

            if (user.MatKhau != request.CurrentPassword.ToMD5())
                return BadRequest("Mật khẩu cũ không chính xác.");
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Mật khẩu mới không khớp.");
            if (request.NewPassword.Length < 6 || request.NewPassword.Length > 100)
                return BadRequest("Mật khẩu mới phải từ 6 đến 100 ký tự.");

            user.MatKhau = request.NewPassword.ToMD5();
            _context.TaiKhoanNews.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
        #endregion

        #region --- Giảng viên/Admin ---
        [HttpPost("logingv")]
        public async Task<IActionResult> LoginGV([FromBody] LoginRequest request)
        {
            var password = request.Password.ToMD5();
            var user = await _context.TaiKhoanNews
                .Include(t => t.GiangVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == request.TenDangNhap && t.MatKhau == password && (t.VaiTro == "GiangVien" || t.VaiTro == "Admin"));

            if (user == null)
                return BadRequest("Thông tin đăng nhập không chính xác hoặc tài khoản bị khóa.");

            if (!user.TrangThai)
                return BadRequest("Tài khoản đang bị khóa.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.GiangVien.HoTen),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("VaiTro", user.VaiTro)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return Ok(new { message = "Đăng nhập thành công" });
        }

        [HttpPost("changepasswordgv")]
        public async Task<IActionResult> ChangePasswordGV([FromBody] ChangePasswordRequest request)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "TenDangNhap")?.Value;
            if (username == null) return BadRequest("Không tìm thấy tài khoản.");

            var user = await _context.TaiKhoanNews.FirstOrDefaultAsync(t => t.TenDangNhap == username && (t.VaiTro == "GiangVien" || t.VaiTro == "Admin"));
            if (user == null) return BadRequest("Tài khoản không tồn tại.");

            if (user.MatKhau != request.CurrentPassword.ToMD5())
                return BadRequest("Mật khẩu cũ không chính xác.");
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Mật khẩu mới không khớp.");
            if (request.NewPassword.Length < 6 || request.NewPassword.Length > 100)
                return BadRequest("Mật khẩu mới phải từ 6 đến 100 ký tự.");

            user.MatKhau = request.NewPassword.ToMD5();
            _context.TaiKhoanNews.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
        #endregion
    }
}
*/
using System.Globalization;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; }

        public AuthenticationController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _context = context;
            _notyfService = notyfService;
        }

        #region --- Login chung cho tất cả vai trò ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var password = request.Password.ToMD5();

            var user = await _context.TaiKhoanNews
                .Include(t => t.HocVien)
                .Include(t => t.GiangVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == request.TenDangNhap && t.MatKhau == password);

            if (user == null || !user.TrangThai)
                return BadRequest(new { message = "Tên đăng nhập hoặc mật khẩu không đúng hoặc tài khoản bị khóa." });

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.VaiTro == "HocVien" ? user.HocVien.HoTen : user.GiangVien.HoTen),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro) // <-- VaiTro = "Admin" phải chính xác
            };

            if (user.VaiTro == "HocVien")
                claims.Add(new Claim("MaHocVien", user.HocVien.MaHocVien.ToString()));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Redirect dựa vào vai trò
            string redirectUrl = "/";
            if (user.VaiTro == "Admin") redirectUrl = "/admin";
            else if (user.VaiTro == "GiangVien") redirectUrl = "/giangvien";
            else if (user.VaiTro == "HocVien") redirectUrl = "/";

            return Ok(new { message = "Đăng nhập thành công", redirectUrl });
        }
        #endregion

        /*#region --- Register Học viên ---
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TenDangNhap) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.HoTen))
            {
                return BadRequest(new { message = "Vui lòng nhập đầy đủ thông tin." });
            }

            if (await _context.TaiKhoanNews.AnyAsync(t => t.TenDangNhap == request.TenDangNhap))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });

            if (await _context.HocVien.AnyAsync(h => h.Email == request.Email))
                return BadRequest(new { message = "Email đã được sử dụng." });

            var taiKhoan = new TaiKhoanNew
            {
                TenDangNhap = request.TenDangNhap,
                MatKhau = request.Password.ToMD5(),
                VaiTro = "HocVien",
                TrangThai = true
            };

            var hocVien = new HocVien
            {
                HoTen = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.HoTen),
                Email = request.Email,
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TaiKhoan = taiKhoan
            };

            _context.TaiKhoanNews.Add(taiKhoan);
            _context.HocVien.Add(hocVien);
            await _context.SaveChangesAsync();

            _notyfService.Success("Đăng ký thành công");
            return Ok(new { message = "Đăng ký thành công", redirectUrl = "/hocvien" });
        }
        #endregion*/


        #region --- ChangePassword chung ---
        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var username = User.Claims.FirstOrDefault(c => c.Type == "TenDangNhap")?.Value;
            if (username == null) return BadRequest(new { message = "Không tìm thấy tài khoản." });

            var user = await _context.TaiKhoanNews.FirstOrDefaultAsync(t => t.TenDangNhap == username);
            if (user == null) return BadRequest(new { message = "Tài khoản không tồn tại." });

            if (user.MatKhau != request.CurrentPassword.ToMD5())
                return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { message = "Mật khẩu mới không khớp." });
            if (request.NewPassword.Length < 6 || request.NewPassword.Length > 100)
                return BadRequest(new { message = "Mật khẩu mới phải từ 6 đến 100 ký tự." });

            user.MatKhau = request.NewPassword.ToMD5();
            _context.TaiKhoanNews.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }
        #endregion
    }
}
