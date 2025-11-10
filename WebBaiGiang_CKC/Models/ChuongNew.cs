using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("CHUONG_NEW")]
    public class ChuongNew
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Mã chương")]
        public int MaChuong { get; set; }

        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [StringLength(200)]
        [DisplayName("Tên chương")]
        public string TenChuong { get; set; }

        [Required]
        [DisplayName("Mã lớp học")]
        public int MaLopHoc { get; set; }

        [ForeignKey("MaLopHoc")]
        public LopHoc LopHoc { get; set; }

        public List<Bai>? Bais { get; set; }
    }
}
