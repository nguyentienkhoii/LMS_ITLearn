using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class BaiTap
    {
        public int BaiTapId { get; set; }

        [DisplayName("Tên bài tập")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenBaiTap { get; set; }

        [DisplayName("Nội dung bài tập")]
        public string NoiDung { get; set; }

        // Khóa ngoại liên kết với môn học
        public int MonHocId { get; set; }
        public MonHoc MonHoc { get; set; }
    }
}
