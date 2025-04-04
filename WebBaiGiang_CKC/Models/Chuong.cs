using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    public class Chuong
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("Số chương")]
        [Range(1, int.MaxValue, ErrorMessage = "Số chương phải lớn hơn 0")]
        public int ChuongId { get; set; }
        [DisplayName("Tên chương")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenChuong { get; set; }
       
        [DisplayName("Tên môn học")]
        public int MonHocId { get; set; }

        public MonHoc MonHoc { get; set; }
        public List<Bai> Bais { get; set; }
    }

}
