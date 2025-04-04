using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class Bai
    {
        public int BaiId { get; set; }
        [DisplayName("Tên chương")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public int ChuongId { get; set; }
        [DisplayName("Tên bài")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenBai { get; set; }
        [DisplayName("Bài số")]
        [Range(1, int.MaxValue, ErrorMessage = "Số bài phải lớn hơn 0")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public int SoBai { get; set; }
        [DisplayName("Mô tả")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string MoTa { get; set; }
        public Chuong Chuong { get; set; }
        public List<Muc> Mucs { get; set; }
    }
}
