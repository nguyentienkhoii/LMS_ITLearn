using System.ComponentModel;

namespace WebBaiGiang_CKC.Models
{
    public class BaiLam
    {
        public int BaiLamId { get; set; }
        public string MSSV { get; set; }

        public string HoTen { get; set; }

        [DisplayName("Số câu đúng")]
        public int? SoCauDung { get; set; } = 0;
        [DisplayName("Điểm")]
        public float? Diem { get; set; } = 0;


        [DisplayName("Thời gian bắt đầu")]

        public DateTime? ThoiGianBatDau { get; set; }

        [DisplayName("Thời gian đến hạn")]

        public DateTime? ThoiGianDenHan { get; set; }
        public virtual ICollection<CauHoi_BaiLam> CauHoi_BaiLam { get; set; }
    }
}
