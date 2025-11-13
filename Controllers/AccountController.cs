/*using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using WebBaiGiang_CKC.Extension;
using Microsoft.AspNetCore.Authorization;

namespace WebBaiGiang_CKC.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public AccountController(WebBaiGiangContext context) => _context = context;

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                return Redirect(GetRedirectUrl(role));
            }

            // Nếu returnUrl trỏ về login → bỏ
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.ToLower().Contains("/account/login"))
                returnUrl = null;

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest model, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var password = model.Password.ToMD5();

            var user = await _context.TaiKhoanNews
                .Include(t => t.HocVien)
                .Include(t => t.GiangVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap && t.MatKhau == password && t.TrangThai);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.VaiTro switch
                {
                    "HocVien" => user.HocVien?.HoTen ?? user.TenDangNhap,
                    "GiangVien" => user.GiangVien?.HoTen ?? user.TenDangNhap,
                    "Admin" => user.GiangVien?.HoTen ?? user.TenDangNhap,
                    _ => user.TenDangNhap
                }),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("MaTaiKhoan", user.MaTaiKhoan.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Xử lý returnUrl không trỏ về login
            if (!string.IsNullOrEmpty(returnUrl) && returnUrl.ToLower().Contains("/account/login"))
                returnUrl = null;

            return Redirect(returnUrl ?? GetRedirectUrl(user.VaiTro));
        }

        // GET: /Account/Logout
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Helper redirect theo role
        private string GetRedirectUrl(string role) => role switch
        {
            "Admin" => "/admin",
            "GiangVien" => "/giangvien",
            "HocVien" => "/",
            _ => "/"
        };
    }
}
*/

/*using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Extension;

namespace WebBaiGiang_CKC.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public AccountController(WebBaiGiangContext context) => _context = context;

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                return Redirect(GetRedirectUrl(role));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            var password = model.Password.ToMD5();

            var user = await _context.TaiKhoanNews
                .Include(t => t.HocVien)
                .Include(t => t.GiangVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap
                                           && t.MatKhau == password
                                           && t.TrangThai);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Tạo claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.VaiTro switch
                {
                    "HocVien" => user.HocVien?.HoTen ?? user.TenDangNhap,
                    "GiangVien" => user.GiangVien?.HoTen ?? user.TenDangNhap,
                    "Admin" => user.GiangVien?.HoTen ?? user.TenDangNhap,
                    _ => user.TenDangNhap
                }),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim("TenDangNhap", user.TenDangNhap),
                new Claim("MaTaiKhoan", user.MaTaiKhoan.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Redirect(GetRedirectUrl(user.VaiTro));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        private string GetRedirectUrl(string role) => role switch
        {
            "Admin" => "/admin",
            "GiangVien" => "/giangvien",
            "HocVien" => "/",
            _ => "/"
        };
    }
}*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Models;
using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Extension;
using Microsoft.AspNetCore.Authorization;

namespace WebBaiGiang_CKC.Controllers
{
    public class AccountController : Controller
    {
        private readonly WebBaiGiangContext _context;
        public AccountController(WebBaiGiangContext context) => _context = context;

        [HttpGet]
        public IActionResult Login()
        {
            TempData.Clear();
            // Nếu đã đăng nhập rồi thì chuyển hướng về trang phù hợp
            if (User.Identity.IsAuthenticated)
            {
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                return Redirect(GetRedirectUrl(role));
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ thông tin.";
                return View(model);
            }


            var password = model.Password.ToMD5();

            var user = await _context.TaiKhoanNews
                .Include(t => t.HocVien)
                .Include(t => t.GiangVien)
                .FirstOrDefaultAsync(t => t.TenDangNhap == model.TenDangNhap
                                           && t.MatKhau == password
                                           && t.TrangThai);

            if (user == null)
            {
                TempData["Error"] = "Tên đăng nhập hoặc mật khẩu không đúng.";
                return View(model);
            }

            // Tạo danh sách claim (thêm claim cho học viên)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.VaiTro switch
                {
                    "HocVien" => user.HocVien?.HoTen ?? user.TenDangNhap,
                    "GiangVien" => user.GiangVien?.HoTen ?? user.TenDangNhap,
                    "Admin" => user.GiangVien?.HoTen ?? user.TenDangNhap,
                    _ => user.TenDangNhap
            }),
            new Claim(ClaimTypes.Role, user.VaiTro),
            new Claim("TenDangNhap", user.TenDangNhap),
            new Claim("MaTaiKhoan", user.MaTaiKhoan.ToString()),

            new Claim(ClaimTypes.NameIdentifier, user.MaTaiKhoan.ToString())
            };

            // ✅ Nếu là học viên, thêm claim HocVienId
            if (user.VaiTro == "HocVien" && user.HocVien != null)
            {
                claims.Add(new Claim("HocVienId", user.HocVien.MaHocVien.ToString()));
            }


            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            //TempData["Success"] = $"Chào mừng {user.TenDangNhap}, đăng nhập thành công!";

            return Redirect(GetRedirectUrl(user.VaiTro));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            TempData.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            //TempData["Logout"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Login");
        }

        private string GetRedirectUrl(string role) => role switch
        {
            "Admin" => "/Admin",
            "GiangVien" => "/Giangvien",
            "HocVien" => "/",
            _ => "/"
        };

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}


