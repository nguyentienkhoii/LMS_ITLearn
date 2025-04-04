using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class CauHoi_BaiLam
    {
        [Key]
        public int CauHoi_BaiLamId { get; set; }


        [ForeignKey("BaiLamId")]
        public int BaiLamId { get; set; }
        [ForeignKey("CauHoi_DeId")]
        public int CauHoi_DeId { get; set; }
        [DisplayName("Đáp án chọn")]
        public string DapAnSVChon { get; set; }
        public virtual BaiLam BaiLam { get; set; }
        public virtual CauHoi_De CauHoi_De { get; set; }
    }
}
