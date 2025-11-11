using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("LOPHOC")]
    public class LopHoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaLopHoc { get; set; }

        [Required, StringLength(100)]
        public string TenLopHoc { get; set; }

        // ✅ Thêm trường Tên viết tắt
        [StringLength(20)]
        public string? TenVietTat { get; set; }

        public string? MoTa { get; set; }

        [Required, StringLength(50)]
        public string TrangThai { get; set; }

        // ✅ Thêm thuộc tính ảnh lớp học (banner)
        [StringLength(255)]
        public string? AnhLopHoc { get; set; }   // lưu đường dẫn, ví dụ: "/MonHoc/macdinhnew.png"

        [Required]
        public int MaKhoaHoc { get; set; }
        [ForeignKey(nameof(MaKhoaHoc))]
        public KhoaHoc? KhoaHoc { get; set; }

        [Required]
        public int MaGiangVien { get; set; }
        [ForeignKey(nameof(MaGiangVien))]
        public GiangVien? GiangVien { get; set; }

        public List<ChuongNew>? Chuongs { get; set; }
        public List<HocVien_LopHoc>? HocVien_LopHocs { get; set; }

        [NotMapped]
        public int SoLuongDangKy { get; set; }

        [NotMapped]
        public int TongSoBai { get; set; }
    }
}
