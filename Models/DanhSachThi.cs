using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("DANHSACHTHI")]
    public class DanhSachThi
    {
        [Key]
        public int DanhSachThiId { get; set; }

        [Required]
        [ForeignKey("HocVien")]
        [DisplayName("Học viên")]
        public int MaHocVien { get; set; }

        [Required]
        [ForeignKey("KyKiemTra")]
        [DisplayName("Kỳ kiểm tra")]
        public int KyKiemTraId { get; set; }

        [DisplayName("Đã nộp")]
        [DefaultValue(false)]
        public bool TrangThai { get; set; } = false;

        // Quan hệ
        public virtual HocVien HocVien { get; set; }
        public virtual KyKiemTra KyKiemTra { get; set; }
        [NotMapped]
        public int SoCauDung { get; set; }
        [NotMapped]
        public float Diem { get; set; }

    }
}
