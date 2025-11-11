using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("BAITAPNOP")]
    public class BaiTapNop
    {
        [Key]
        public int MaBaiTapNop { get; set; }

        [StringLength(255)]
        public string FileNop { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? NgayNop { get; set; }

        public int LanNop { get; set; } = 1;

        [Range(0, 10)]
        public double? Diem { get; set; }

        [StringLength(500)]
        public string NhanXet { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? NgayCham { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }

        // FK → Bài tập
        [ForeignKey("BaiTap")]
        public int MaBaiTap { get; set; }
        public virtual BaiTap BaiTap { get; set; }

        // FK → Học viên
        [ForeignKey("HocVien")]
        public int MaHocVien { get; set; }
        public virtual HocVien HocVien { get; set; }
    }
}
