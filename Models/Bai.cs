using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("BAI")]
    public class Bai
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Mã bài")]
        public int BaiId { get; set; }

        // Khóa ngoại đến CHUONG_NEW
        [Required]
        [DisplayName("Mã chương")]
        public int MaChuong { get; set; }

        [ForeignKey("MaChuong")]
        public ChuongNew Chuong { get; set; }   // Navigation property

        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(200)]
        [DisplayName("Tên bài")]
        public string TenBai { get; set; }

        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số bài phải lớn hơn 0")]
        [DisplayName("Bài số")]
        public int SoBai { get; set; }

        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [Column(TypeName = "ntext")]
        [DisplayName("Mô tả")]
        public string MoTa { get; set; }

        // Quan hệ 1-n: 1 bài có thể có nhiều mục
        public List<Muc>? Mucs { get; set; }

        // Quan hệ 1-n: Một bài có thể có nhiều bài tập
        public virtual ICollection<BaiTap>? BaiTaps { get; set; }

    }
}
