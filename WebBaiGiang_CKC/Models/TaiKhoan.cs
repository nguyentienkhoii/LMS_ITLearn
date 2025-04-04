using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    public class TaiKhoan
    {
        public int TaiKhoanId { get; set; }

        [DisplayName("MSSV")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "{0} từ 6-20 kí tự")]
        public string MSSV { get; set; }

        [DisplayName("Mật khẩu")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "{0} từ 6-50 kí tự")]
        public string MatKhau { get; set; }

        [DisplayName("Email")]
        [EmailAddress(ErrorMessage = "{0} không hợp lệ")]
        public string Email { get; set; }

        [DisplayName("Họ tên")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string HoTen { get; set; }

        [DisplayName("Còn hoạt động")]
        [DefaultValue(true)]
        public bool TrangThai { get; set; } = true;
        [NotMapped]
        public string ResetToken { get; set; }

        [NotMapped]
        public DateTime? ResetTokenExpiry { get; set; }
        public virtual ICollection<DangKyMonHoc> DangKyMonHoc { get; set; }
    }
}
