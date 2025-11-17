using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class TaiKhoanNew
    {
        [Key] 
        public int MaTaiKhoan { get; set; }

        [Required, StringLength(20)]
        public string TenDangNhap { get; set; } = null!;

        [Required, StringLength(50)]
        public string MatKhau { get; set; } = null!;

        [Required, StringLength(20)]
        public string VaiTro { get; set; } = null!;

        public bool TrangThai { get; set; } = true;

        // Quan hệ một-một
        public virtual HocVien HocVien { get; set; }
        public virtual GiangVien GiangVien { get; set; }
    }
}
