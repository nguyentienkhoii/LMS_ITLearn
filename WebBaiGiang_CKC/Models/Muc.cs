using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    [Table("MUCCON")]
    public class Muc
    {
        [Key]
        public int MucId { get; set; }

        [DisplayName("Tên mục")]
        //[Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenMuc { get; set; }

        [DisplayName("Mã bài")]
       // [Required(ErrorMessage = "{0} không được bỏ trống")]
        public int BaiId { get; set; }

        [ForeignKey("BaiId")]
        public Bai Bai { get; set; }

        [DisplayName("Số mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Số mục phải lớn hơn 0")]
        public int MucSo { get; set; }

        [DisplayName("Nội dung")]
        [Column(TypeName = "ntext")]
      //  [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string NoiDung { get; set; }

        // Quan hệ 1-n: 1 mục có thể có nhiều tài liệu
        public List<TaiLieu>? TaiLieus { get; set; }
    }
}
