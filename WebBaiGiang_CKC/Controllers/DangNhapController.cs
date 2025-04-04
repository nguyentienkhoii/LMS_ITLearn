using AspNetCoreHero.ToastNotification.Abstractions;
using MailKit.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

using System.Security.Claims;
using WebBaiGiang_CKC.Extension;
using WebBaiGiang_CKC.Data;

using Microsoft.EntityFrameworkCore;
using BaiGiang.Models;
using MailKit.Net.Smtp;

namespace WebBaiGiang_CKC.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly WebBaiGiangContext _context;
       
        public INotyfService _notyfService { get; }
        public DangNhapController(WebBaiGiangContext context, INotyfService notyfService )
        {
            _context = context;
            _notyfService = notyfService;
        

        }
        #region Đăng nhập
        public IActionResult Login()
        {

            return View();
        }



    //    [HttpPost]


    //    public IActionResult Login(string mssv, string pass)
    //    {
    //        var mssvClaim = User.Claims.SingleOrDefault(c => c.Type == "MSSV");


    //        var password = pass.ToMD5();
    //        var user = _context.TaiKhoan.FirstOrDefault(u => u.MSSV == mssv && u.MatKhau == password);
    //        if (mssvClaim?.Value != null)
    //        {
    //            // Tên đăng nhập hoặc mật khẩu không đúng
    //            _notyfService.Error($"Bạn đang đăng nhập dưới tài khoản {mssvClaim.Value}");
    //            return RedirectToAction("Index", "Home");
    //        }
    //        if (user == null)
    //        {
    //            // Tên đăng nhập hoặc mật khẩu không đúng
    //            _notyfService.Error("Thông tin đăng nhập không chính xác");
    //            return RedirectToAction("Index", "Home");
    //        }
    //        if (user.MSSV != mssv)
    //        {
    //            // Tên đăng nhập hoặc mật khẩu không đúng
    //            _notyfService.Error("Tài khoản không chính xác");
    //            return RedirectToAction("Index", "Home");
    //        }
    //        if (user.MatKhau != password)
    //        {
    //            // Tên đăng nhập hoặc mật khẩu không đúng
    //            _notyfService.Error("Mật khẩu không chính xác");
    //            return RedirectToAction("Index", "Home");
    //        }
    //        if (user.TrangThai == false)
    //        {
    //            // Tên đăng nhập hoặc mật khẩu không đúng
    //            _notyfService.Warning("Tài khoản của bạn đang bị khóa, vui lòng liên hệ Admin!");
    //            return RedirectToAction("Index", "Home");
    //        }
    //        var claims = new List<Claim>
    //{
    //    new Claim(ClaimTypes.Name, user.HoTen),
    //     new Claim("MSSV", user.MSSV),
    //     new Claim("Email", user.Email),

    //    // new Claim(ClaimTypes.Name, user.MSSV),
    //    //new Claim(ClaimTypes.Role, "Administrator"),
    //};

    //        var claimsIdentity = new ClaimsIdentity(
    //            claims, CookieAuthenticationDefaults.AuthenticationScheme);

    //        HttpContext.SignInAsync(
    //            CookieAuthenticationDefaults.AuthenticationScheme,
    //            new ClaimsPrincipal(claimsIdentity)
    //        );
    //        _notyfService.Success("Đăng nhập thành công");
    //        return RedirectToAction("Index", "Home");
    //    }
        #endregion

        #region Đăng xuất

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _notyfService.Success("Đăng xuất thành công");
            return RedirectToAction("Index", "Home");
        }
        #endregion

        //public async Task<IActionResult> DoiMatKhau(string pass, string newpass, string confirmpass)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var mssvClaim = User.Claims.SingleOrDefault(c => c.Type == "MSSV");
        //        var mssv_ = "";
        //        if (mssvClaim != null) { mssv_ = mssvClaim.Value; }
        //        var password = pass.ToMD5();
        //        var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.MSSV == mssv_);
        //        if (user.MatKhau != password)
        //        {
        //            _notyfService.Error("Mật khẩu cũ không chính xác");
        //            return RedirectToAction("HoSo", "BaiGiangs");
        //        }
        //        if (newpass.Length <6 && newpass.Length <100)
        //        {
        //            _notyfService.Error("Mật khẩu mới phải trên 6 ký tự và nhỏ hơn 100 ký tự ");
        //            return RedirectToAction("HoSo", "BaiGiangs");
        //        }
        //        if (newpass != confirmpass)
        //        {
        //            _notyfService.Error("Mật khẩu mới không đúng với mật khẩu xác nhận !");
        //            return RedirectToAction("HoSo", "BaiGiangs");
        //        }
        //        user.MatKhau = newpass.ToMD5();
        //        _context.Update(user);
        //        await _context.SaveChangesAsync();
        //    }
        //    else
        //    {
        //        _notyfService.Error("Vui lòng nhập đầy đủ thông mật khẩu !");

        //    }
        //    _notyfService.Success("Đổi mật khẩu thành công!");
        //    return RedirectToAction("HoSo", "BaiGiangs");
        //}

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> QuenMatKhau(ForgotPasswordModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Email == model.Email);
        //        if (user != null)
        //        {
        //            var token = Guid.NewGuid().ToString();
        //            user.ResetToken = token;
        //            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        //            user.Email = model.Email;
        //            await _context.SaveChangesAsync();
        //            // Lưu token và thời gian hết hạn vào biến Session
        //            HttpContext.Session.SetString("ResetToken", token);
        //            HttpContext.Session.SetString("Email", user.Email);
        //            HttpContext.Session.SetString("ResetTokenExpiry", user.ResetTokenExpiry.ToString());
        //            // Gửi email chứa token đến địa chỉ email của người dùng
        //            // ...
        //            var email = new MimeMessage();
        //            email.From.Add(new MailboxAddress("AdminDotnet", "admin@example.com"));
        //            email.To.Add(MailboxAddress.Parse($"{model.Email}"));
        //            email.Subject = "Yêu cầu đặt lại mật khẩu";

        //            email.Body = new TextPart("plain")
        //            {
        //                Text = $"Để đặt lại mật khẩu, vui lòng sử dụng token sau đây: {token} mã token có thời hạn là 10 phút"
        //            };
        //            using var smtp = new SmtpClient();
        //            smtp.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        //            smtp.Authenticate("0306201451@caothang.edu.vn", "12345qwertKHANG");
        //            smtp.Send(email);
        //            smtp.Disconnect(true);

        //            TempData["SuccessMessage"] = "Yêu cầu đặt lại mật khẩu của bạn đã được gửi đi. Vui lòng kiểm tra email của bạn để tiếp tục.";
        //            return RedirectToAction("DatLaiMatKhau");
        //        }
        //        else
        //        {
        //            _notyfService.Warning("Email không tồn tại trong hệ thống ");
        //        }


        //    }

        //    return View(model);

        //}

        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> DatLaiMatKhau(ResetPasswordViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var resetToken = HttpContext.Session.GetString("ResetToken");
        //        var resetTokenExpiry = HttpContext.Session.GetString("ResetTokenExpiry");
        //        var email = HttpContext.Session.GetString("Email");
        //        var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Email == email);
        //        if (user != null && resetToken == model.Token)
        //        {
        //            if (model.Password != model.ConfirmPassword)
        //            {
        //                TempData["ResetPasswordErrorMessage"] = "Mật khẩu mới và mật khẩu xác nhận không khớp.";
        //                return View(model);
        //            }

        //            user.MatKhau = (model.Password).ToMD5();
        //            await _context.SaveChangesAsync();


        //            _notyfService.Success("Mật khẩu của bạn đã được đặt lại thành công.");
        //            return RedirectToAction("Index", "Home");
        //        }

        //        _notyfService.Error("Yêu cầu đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.");
        //    }

        //    return View(model);
        //}
    }
}
