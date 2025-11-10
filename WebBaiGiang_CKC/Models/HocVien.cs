using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class HocVien
    {
        [Key]
        public int MaHocVien { get; set; } // Khóa chính

        [Required, StringLength(100)]
        public string HoTen { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string SoDienThoai { get; set; }

        public string DiaChi { get; set; }

        public int MaTaiKhoan { get; set; } // Khóa ngoại liên kết với TaiKhoan

        public virtual TaiKhoanNew TaiKhoan { get; set; }

        public List<HocVien_LopHoc>? HocVien_LopHocs { get; set; }

        public virtual ICollection<BaiTapNop>? BaiTapNops { get; set; }


    }
}
