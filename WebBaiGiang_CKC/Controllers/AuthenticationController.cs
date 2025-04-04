using AspNetCoreHero.ToastNotification.Abstractions;
using BaiGiang.Models;
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
        public INotyfService _notyfService { get; }
        private readonly WebBaiGiangContext _context;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(WebBaiGiangContext context, ILogger<AuthenticationController> logger, INotyfService notyfService)
        {
            _context = context;
            _logger = logger;
            _notyfService = notyfService;
        }

        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var mssvClaim = User.Claims.SingleOrDefault(c => c.Type == "MSSV");
            var password = request.Password.ToMD5();
            var user = _context.TaiKhoan.FirstOrDefault(u => u.MSSV == request.Mssv && u.MatKhau == password);

            if (mssvClaim?.Value != null)
            {
                // Tên đăng nhập hoặc mật khẩu không đúng
                return BadRequest("Bạn đang đăng nhập dưới tài khoản " + mssvClaim.Value);
            }

            if (user == null)
            {
                // Tên đăng nhập hoặc mật khẩu không đúng
                return BadRequest("Thông tin đăng nhập không chính xác");
            }
            if (user.MSSV != request.Mssv)
            {
                // Tài khoản không chính xác
                return BadRequest("Tài khoản không chính xác");
            }

            if (user.MatKhau != password)
            {
                // Mật khẩu không chính xác
                return BadRequest("Mật khẩu không chính xác");
            }

            if (user.TrangThai == false)
            {
                // Tài khoản bị khóa
                return BadRequest("Tài khoản của bạn đang bị khóa, vui lòng liên hệ Admin!");
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.HoTen),
            new Claim("MSSV", user.MSSV),
            new Claim("Email", user.Email),
            new Claim("TaiKhoanId", user.TaiKhoanId.ToString()),
        };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
            _notyfService.Success("Đăng nhập thành công");
            return Ok(new { message = "Đăng nhập thành công" });
        }

        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        [HttpPost]
        [Route("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
           
                var mssvClaim = User.Claims.SingleOrDefault(c => c.Type == "MSSV");
                var mssv_ = "";
                if (mssvClaim != null) { mssv_ = mssvClaim.Value; }
                var password = request.CurrentPassword.ToMD5();
                var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.MSSV == mssv_);
                if (user.MatKhau != password)
                {
                    return BadRequest("Mật khẩu cũ không chính xác");
                }
                if (request.NewPassword.Length < 6 || request.NewPassword.Length > 100)
                {
                    return BadRequest("Mật khẩu mới phải trên 6 ký tự và nhỏ hơn 100 ký tự");
                }
                if (request.NewPassword != request.ConfirmPassword)
                {
                    return BadRequest("Mật khẩu mới không đúng với mật khẩu xác nhận");
                }
                user.MatKhau = request.NewPassword.ToMD5();
                _context.TaiKhoan.Update(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đổi mật khẩu thành công" });
           
        }

        [HttpPost]
      
        [Route("forgotpassword")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user =  _context.TaiKhoan.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString();
                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10);
                 _context.SaveChangesAsync();

                HttpContext.Session.SetString("ResetToken", token);
                HttpContext.Session.SetString("Email", model.Email);

                // Gửi email chứa token đến địa chỉ email của người dùng
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("AdminDotnet", "admin@example.com"));
                email.To.Add(MailboxAddress.Parse($"{model.Email}"));
                email.Subject = "Yêu cầu đặt lại mật khẩu";
                email.Body = new TextPart("plain")
                {
                    Text = $"Để đặt lại mật khẩu, vui lòng sử dụng token sau đây: {token} mã token có thời hạn là 10 phút"
                };
                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("baigiangtructuyen2025@gmail.com", "kryx fkgc pxht wfsf");
                smtp.Send(email);
                smtp.Disconnect(true);
                _notyfService.Success("Yêu cầu đặt lại mật khẩu của bạn đã được gửi. Vui lòng kiểm tra email của bạn để tiếp tục.");
                return Ok(new { message = "Yêu cầu đặt lại mật khẩu của bạn đã được gửi. Vui lòng kiểm tra email của bạn để tiếp tục." });
            }
            else
            {
                return BadRequest("Email không tồn tại trong hệ thống");
            }
        }


        [HttpPost("resetpassword")]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
        
                var resetToken = HttpContext.Session.GetString("ResetToken");
                var resetTokenExpiry = HttpContext.Session.GetString("ResetTokenExpiry");
                var email = HttpContext.Session.GetString("Email");
                var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null && resetToken == model.Token)
                {
                    if (model.Password != model.ConfirmPassword)
                    {
                        return BadRequest("Mật khẩu mới và mật khẩu xác nhận không khớp.");
                    }

                    user.MatKhau = (model.Password).ToMD5();
                    await _context.SaveChangesAsync();
                _notyfService.Success("Mật khẩu của bạn đã được đặt lại thành công.");
                return Ok("Mật khẩu của bạn đã được đặt lại thành công.");
                }


            return BadRequest("Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }

        ////////
        [HttpPost]
        [Route("logingv")]
        public IActionResult Logingv([FromBody] LoginRequest request)
        {
            var TenDangNhapclaim = User.Claims.SingleOrDefault(c => c.Type == "TenDangNhap");
            var password = request.Password.ToMD5();
            var user = _context.GiaoVien.FirstOrDefault(u => u.TenDangNhap == request.Mssv && u.MatKhau == password);

            if (TenDangNhapclaim?.Value != null)
            {
                // Tên đăng nhập hoặc mật khẩu không đúng
                return BadRequest("Bạn đang đăng nhập dưới tài khoản " + TenDangNhapclaim.Value);
            }

            if (user == null)
            {
                // Tên đăng nhập hoặc mật khẩu không đúng
                return BadRequest("Thông tin đăng nhập không chính xác");
            }
            if (user.TenDangNhap != request.Mssv)
            {
                // Tài khoản không chính xác
                return BadRequest("Tài khoản không chính xác");
            }

            if (user.MatKhau != password)
            {
                // Mật khẩu không chính xác
                return BadRequest("Mật khẩu không chính xác");
            }

            if (user.TrangThai == false)
            {
                // Tài khoản bị khóa
                return BadRequest("Tài khoản của bạn đang bị khóa, vui lòng liên hệ Admin!");
            }

            List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("TenDangNhap" , user.TenDangNhap),
                         new Claim("AnhDaiDien", "/contents/Images/GiaoVien/" + user.AnhDaiDien) // Thêm đường dẫn đến ảnh đại diện vào claims
                    };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity)
            );
            
            return Ok(new { message = "Đăng nhập thành công" });
        }


        [HttpPost]
        [Route("changepasswordgv")]
        public async Task<IActionResult> ChangePasswordgv([FromBody] ChangePasswordRequest request)
        {

            var TenDangNhapclaim = User.Claims.SingleOrDefault(c => c.Type == "TenDangNhap");
            var tendangnhap = "";
            if (TenDangNhapclaim != null) { tendangnhap = TenDangNhapclaim.Value; }
            var password = request.CurrentPassword.ToMD5();
            var user = await _context.GiaoVien.FirstOrDefaultAsync(u => u.TenDangNhap == tendangnhap);
            if (user.MatKhau != password)
            {
                return BadRequest("Mật khẩu cũ không chính xác");
            }
            if (request.NewPassword.Length < 6 || request.NewPassword.Length > 100)
            {
                return BadRequest("Mật khẩu mới phải trên 6 ký tự và nhỏ hơn 100 ký tự");
            }
            if (request.NewPassword != request.ConfirmPassword)
            {
                return BadRequest("Mật khẩu mới không đúng với mật khẩu xác nhận");
            }
            user.MatKhau = request.NewPassword.ToMD5();
            _context.GiaoVien.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công" });

        }


        [HttpPost]

        [Route("forgotpasswordgv")]
        public IActionResult ForgotPasswordgv([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = _context.GiaoVien.FirstOrDefault(u => u.Email == model.Email);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString();
                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(10);
                _context.SaveChangesAsync();
                HttpContext.Session.SetString("ResetToken", token);
                HttpContext.Session.SetString("Email", model.Email);
                // Gửi email chứa token đến địa chỉ email của người dùng
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("AdminDotnet", "admin@example.com"));
                email.To.Add(MailboxAddress.Parse($"{model.Email}"));
                email.Subject = "Yêu cầu đặt lại mật khẩu";
                email.Body = new TextPart("plain")
                {
                    Text = $"Để đặt lại mật khẩu, vui lòng sử dụng token sau đây: {token} mã token có thời hạn là 10 phút"
                };
                using var smtp = new SmtpClient();
                smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                smtp.Authenticate("baigiangtructuyen2025@gmail.com", "kryx fkgc pxht wfsf");
                smtp.Send(email);
                smtp.Disconnect(true);
                _notyfService.Success("Yêu cầu đặt lại mật khẩu của bạn đã được gửi. Vui lòng kiểm tra email của bạn để tiếp tục.");
                return Ok("Yêu cầu đặt lại mật khẩu của bạn đã được gửi. Vui lòng kiểm tra email của bạn để tiếp tục.");
            }
            else
            {
                return BadRequest("Email không tồn tại trong hệ thống");
            }
        }

        [HttpPost("resetpasswordgv")]
        public async Task<ActionResult> ResetPasswordgv(ResetPasswordViewModel model)
        {

            var resetToken = HttpContext.Session.GetString("ResetToken");
            var resetTokenExpiry = HttpContext.Session.GetString("ResetTokenExpiry");
            var email = HttpContext.Session.GetString("Email");
            var user = await _context.GiaoVien.FirstOrDefaultAsync(u => u.Email == email);
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest("Mật khẩu mới và mật khẩu xác nhận không khớp.");
            }
            if (user != null && resetToken == model.Token)
            {
               

                user.MatKhau = (model.Password).ToMD5();
                await _context.SaveChangesAsync();
                _notyfService.Success("Mật khẩu của bạn đã được đặt lại thành công.");
                return Ok("Mật khẩu của bạn đã được đặt lại thành công.");
            }


            return BadRequest("Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        }


    }
}
