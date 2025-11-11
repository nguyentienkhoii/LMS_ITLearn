using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class GiangVien
    {
        [Key]
        public int MaGiangVien { get; set; } // Khóa chính

        [Required, StringLength(100)]
        public string HoTen { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string SoDienThoai { get; set; }

        public string DiaChi { get; set; }

        [StringLength(100)]
        public string ChuyenMon { get; set; }

        public int MaTaiKhoan { get; set; } // Khóa ngoại

        public virtual TaiKhoanNew TaiKhoan { get; set; }
        public ICollection<LopHoc>? LopHocs { get; set; }
    }
}
