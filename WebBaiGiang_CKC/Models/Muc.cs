using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class Muc
    {
        public int MucId { get; set; }
        [DisplayName("Tên mục")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenMuc { get; set; }
        [DisplayName("Tên bài")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public int BaiId { get; set; }
        [DisplayName("Số mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Số mục phải lớn không 0")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public int MucSo { get; set; }
        [DisplayName("Nội dung")]
        [Column(TypeName = "ntext")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string NoiDung { get; set; }
        public Bai Bai { get; set; }
        
    }
}
