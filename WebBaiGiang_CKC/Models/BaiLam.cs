using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("BAILAM")]
    public class BaiLam
    {
        [Key]
        public int BaiLamId { get; set; }

        [Required]
        [ForeignKey("HocVien")]
        [DisplayName("Học viên")]
        public int MaHocVien { get; set; }

        [DisplayName("Số câu đúng")]
        public int? SoCauDung { get; set; } = 0;

        [DisplayName("Điểm")]
        public float? Diem { get; set; } = 0;

        [DisplayName("Thời gian bắt đầu")]
        public DateTime? ThoiGianBatDau { get; set; }

        [DisplayName("Thời gian đến hạn")]
        public DateTime? ThoiGianDenHan { get; set; }

        // Quan hệ
        public virtual HocVien HocVien { get; set; }

        public virtual ICollection<CauHoi_BaiLam> CauHoi_BaiLam { get; set; }
    }
}
