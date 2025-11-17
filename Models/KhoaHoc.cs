using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("KHOAHOC")]
    public class KhoaHoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaKhoaHoc { get; set; }

        [Required]
        [StringLength(100)]
        public string TenKhoaHoc { get; set; }

        public string? MoTa { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ThoiGianBatDau { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ThoiGianKetThuc { get; set; }

        // Quan hệ 1 - n: 1 khóa học có nhiều lớp học
        public ICollection<LopHoc>? LopHocs { get; set; }
    }
}
