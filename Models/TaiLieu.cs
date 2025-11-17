using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    [Table("TAILIEU")]
    public class TaiLieu
    {
        [Key]
        public int MaTaiLieu { get; set; }

        [DisplayName("Tên tệp tài liệu")]
        [StringLength(255)]
        public string? FileTaiLieu { get; set; }

        [Required]
        [DisplayName("Mục con")]
        public int MaMucCon { get; set; }

        [ForeignKey("MaMucCon")]
        public Muc Muc { get; set; }
    }
}
