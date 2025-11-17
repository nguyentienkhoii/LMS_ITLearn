using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("BAITAP")]
    public class BaiTap
    {
        [Key]
        public int MaBaiTap { get; set; }

        [Required, StringLength(255)]
        public string TenBaiTap { get; set; }

        [StringLength(1000)]
        public string MoTa { get; set; }

        [StringLength(255)]
        public string FileDinhKem { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? HanNop { get; set; }

        public DateTime? LateSubmission { get; set; }   // Hạn nộp muộn
        public DateTime? RemindToGrade { get; set; }    // Nhắc chấm điểm (không dùng trong logic)
        public bool ReminderSent { get; set; } = false;


        // FK → Bài giảng
        [ForeignKey("Bai")]
        public int BaiId { get; set; }
        public virtual Bai Bai { get; set; }

        // Quan hệ 1-nhiều với BaiTapNop
        public virtual ICollection<BaiTapNop> BaiTapNops { get; set; }
    }
}
