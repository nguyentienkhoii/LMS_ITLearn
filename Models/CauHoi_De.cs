using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    public class CauHoi_De
    {
        public int CauHoi_DeId { get; set; }
        [ForeignKey("CauHoiId")]
        public int CauHoiId { get; set; }

        [ForeignKey("DeId")]
        public int DeId { get; set; }


        public virtual CauHoi CauHoi { get; set; }

        public virtual De De { get; set; }
        public ICollection<CauHoi_BaiLam> CauHoi_BaiLam { get; set; }

    }
}
