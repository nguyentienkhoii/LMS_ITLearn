using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    public class GiaoVien
    {
        public int Id { get; set; }
        [DisplayName("Tên đăng nhập ")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} từ 6-20 kí tự")]
        public string TenDangNhap { get; set; }
        [DisplayName("Mật khẩu")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "{0} từ 6-50 kí tự")]
        public string MatKhau { get; set; }
        [DisplayName("Họ tên")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} từ 6-20 kí tự")]
        public string HoTen { get; set; }
        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "{0} không hợp lệ")]
        public string Email { get; set; }
        [DisplayName("Hình đại diện")]
        public string AnhDaiDien { get; set; }
        [DisplayName("Giáo viên")]
        [DefaultValue(true)]
        public bool IsGiaoVien { get; set; } = true;
        [DisplayName("Còn hoạt động")]
        [DefaultValue(true)]
        public bool TrangThai { get; set; } = true;
        [NotMapped]
        public string ResetToken { get; set; }

        [NotMapped]
        public DateTime? ResetTokenExpiry { get; set; }
    }
}
