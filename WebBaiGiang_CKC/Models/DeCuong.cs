using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    public class DeCuong
    {
        public int DeCuongId { get; set; }

        [DisplayName("Tiêu đề")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TieuDe { get; set; }

        [DisplayName("Nội dung đề cương")]
        [Column(TypeName = "ntext")]
        public string NoiDung { get; set; }

        // Khóa ngoại liên kết với môn học
        public int MonHocId { get; set; }
        public MonHoc MonHoc { get; set; }
    }
}
