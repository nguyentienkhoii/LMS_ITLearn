using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebBaiGiang_CKC.Models
{
    public class De
    {
        public int DeId { get; set; }
        [DisplayName("Kỳ kiểm tra")]
        public int KyKiemTraId { get; set; }


        [DisplayName("Số câu hỏi")]
        [Range(1, 100, ErrorMessage = "Số câu hỏi phải là số nguyên dương từ 1 đến 100")]
        public int SoCauHoi { get; set; }

        public float? DoKhoDe { get; set; }
        public virtual KyKiemTra KyKiemTra { get; set; }
        public virtual ICollection<CauHoi_De> CauHoi_DeThi { get; set; }
    }
}
