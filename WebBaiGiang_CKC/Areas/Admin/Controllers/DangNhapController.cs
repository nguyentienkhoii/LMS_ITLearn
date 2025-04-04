using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Data;
using Microsoft.EntityFrameworkCore;
using BaiGiang.Models;
using MailKit.Security;
using MimeKit;
using MailKit.Net.Smtp;

namespace WebBaiGiang_CKC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DangNhapController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public INotyfService _notyfService { get; } 
        public static string image;
        public DangNhapController(WebBaiGiangContext context, INotyfService notyfService)
        {
            _notyfService = notyfService;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region Đăng nhập
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ClaimsPrincipal claimsPrincipal = HttpContext.User;
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.ReturnURL = returnUrl;
            return View();
        }

        [HttpPost]

        public async Task<IActionResult> Login(string tendangnhap, string pass)
        {
            if (ModelState.IsValid)
            {
                var password = pass.ToMD5();
                //var anh = _context.GiaoVien.ToList();
                // Kiểm tra tên đăng nhập và mật khẩu
                var user = await _context.GiaoVien.FirstOrDefaultAsync(u => u.TenDangNhap == tendangnhap && u.MatKhau == password);
                if (user == null)
                {
                    // Tên đăng nhập hoặc mật khẩu không đúng
                    _notyfService.Error("Thông tin đăng nhập không chính xác");
                    return RedirectToAction("Index", "Home");
                }
                if (user.TenDangNhap != tendangnhap)
                {
                    // Tên đăng nhập hoặc mật khẩu không đúng
                    _notyfService.Error("Tài khoản không chính xác");
                    return RedirectToAction("Index", "Home");
                }
                if (user.MatKhau != password)
                {
                    // Tên đăng nhập hoặc mật khẩu không đúng
                    _notyfService.Error("Mật khẩu không chính xác");
                    return RedirectToAction("Index", "Home");
                }
                if (user.TrangThai == false)
                {
                    _notyfService.Error("Tài khoản đã bị khóa");
                    return RedirectToAction("Index", "Home");
                }
                if (user != null)
                {
                    // Lưu thông tin người dùng vào cookie xác thực
                    List<Claim> claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, user.HoTen),
                        new Claim(ClaimTypes.Role, "Admin"),
                        new Claim("TenDangNhap" , user.TenDangNhap),
                        new Claim("Id" , user.Id.ToString()),
                         new Claim("AnhDaiDien", "/contents/Images/GiaoVien/" + user.AnhDaiDien) // Thêm đường dẫn đến ảnh đại diện vào claims
                    };
                    //   Response.Cookies.Append("AnhDaiDien", "Images/GiaoVien/" + user.AnhDaiDien);
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _notyfService.Success("Đăng nhập thành công");
                    // Chuyển hướng đến trang chủ
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    _notyfService.Warning("Tên đăng nhập hoặc mật khẩu không đúng");
                }
            }

            // Nếu có lỗi xảy ra, hiển thị thông báo lỗi bằng NotyfService
            _notyfService.Warning("Tên đăng nhập hoặc mật khẩu không đúng");

            // Chuyển hướng đến trang Login
            return View("Login", "Home");
        }
        #endregion
        #region Đăng xuất

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _notyfService.Success("Đăng xuất thành công");
            return RedirectToAction("Login", "DangNhap");
        }
        #endregion

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QuenMatKhau(ForgotPasswordModel model)
       {
            if (ModelState.IsValid)
            {
                var user = await _context.GiaoVien.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user != null)
                {
                    var token = Guid.NewGuid().ToString();
                    user.ResetToken = token;
                    user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
                    user.Email = model.Email;
                    await _context.SaveChangesAsync();
                    // Lưu token và thời gian hết hạn vào biến Session
                    HttpContext.Session.SetString("ResetToken", token);
                    HttpContext.Session.SetString("Email", user.Email);
                    HttpContext.Session.SetString("ResetTokenExpiry", user.ResetTokenExpiry.ToString());
                    // Gửi email chứa token đến địa chỉ email của người dùng
                    // ...
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
                    smtp.Authenticate("0306201451@caothang.edu.vn", "12345qwertKHANG");
                    smtp.Send(email);
                    smtp.Disconnect(true);

                    TempData["SuccessMessage"] = "Yêu cầu đặt lại mật khẩu của bạn đã được gửi đi. Vui lòng kiểm tra email của bạn để tiếp tục.";
                    return RedirectToAction("DatLaiMatKhau");
                }
             
            }

            _notyfService.Error("Email không tồn tại trong hệ thống ");
            return View(model);

        }


        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DatLaiMatKhau(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var resetToken = HttpContext.Session.GetString("ResetToken");
                var resetTokenExpiry = HttpContext.Session.GetString("ResetTokenExpiry");
                var email = HttpContext.Session.GetString("Email");
                var user = await _context.GiaoVien.FirstOrDefaultAsync(u => u.Email == email);
                if (user != null && resetToken == model.Token)
                {
                    if (model.Password != model.ConfirmPassword)
                    {
                        TempData["ResetPasswordErrorMessage"] = "Mật khẩu mới và mật khẩu xác nhận không khớp.";
                        return View(model);
                    }

                    user.MatKhau = (model.Password).ToMD5();
                    await _context.SaveChangesAsync();


                    _notyfService.Success("Mật khẩu của bạn đã được đặt lại thành công.");
                    return RedirectToAction("Login", "DangNhap");
                }
            }
            _notyfService.Error("Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DoiMatKhau(string pass, string newpass, string confirmpass)
        {
            if (ModelState.IsValid)
            {
                var tendangnhapclam = User.Claims.SingleOrDefault(c => c.Type == "TenDangNhap");
                var tendangnhap = "";
                if (tendangnhapclam != null) { tendangnhap = tendangnhapclam.Value; }
                var password = pass.ToMD5();
                var user = await _context.GiaoVien.FirstOrDefaultAsync(u => u.TenDangNhap == tendangnhap);
                if (user.MatKhau != password)
                {
                    _notyfService.Error("Mật khẩu cũ không chính xác");
                    return RedirectToAction("Index", "Home");
                }
                if (newpass.Length < 6 && newpass.Length < 100)
                {
                    _notyfService.Error("Mật khẩu mới phải trên 6 ký tự và nhỏ hơn 100 ký tự ");
                    return RedirectToAction("Index", "Home");
                }
                if (newpass != confirmpass)
                {
                    _notyfService.Error("Mật khẩu mới không đúng với mật khẩu xác nhận !");
                    return RedirectToAction("Index", "Home");
                }
                user.MatKhau = newpass.ToMD5();
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                _notyfService.Error("Vui lòng nhập đầy đủ thông mật khẩu !");

            }
            _notyfService.Success("Đổi mật khẩu thành công!");
            return RedirectToAction("Index", "Home");
        }


    }
}
