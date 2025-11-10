using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class LoginRequest
    {
        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải có độ dài từ 6 đến 20 ký tự")]
        public string TenDangNhap { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có độ dài tối thiểu 6 ký tự")]
        [MaxLength(100, ErrorMessage = "Mật khẩu không được vượt quá 100 ký tự")]
        public string Password { get; set; }
    }
}
