using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("CAUHOI")]
    public class CauHoi
    {
        [Key]
        public int CauHoiId { get; set; }

        [DisplayName("Chương")]
        [Required(ErrorMessage = "Vui lòng chọn chương")]
        [ForeignKey("ChuongNew")]
        public int MaChuong { get; set; }

        // Liên kết với chương mới
        public ChuongNew ChuongNew { get; set; }

        [DisplayName("Câu hỏi")]
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        public string NoiDung { get; set; }

        [DisplayName("Đáp án A")]
        public string? DapAnA { get; set; }

        [DisplayName("Đáp án B")]
        public string? DapAnB { get; set; }

        [DisplayName("Đáp án C")]
        public string? DapAnC { get; set; }

        [DisplayName("Đáp án D")]
        public string? DapAnD { get; set; }

        [DisplayName("Đáp án đúng")]
        [Required(ErrorMessage = "Vui lòng nhập đáp án đúng")]
        public string DapAnDung { get; set; }

        [DisplayName("Độ khó (%)")]
        [Range(1, 100, ErrorMessage = "Độ khó đề là nguyên dương từ 1% đến 100%")]
        public float DoKho { get; set; }

        public int? SoLanLay { get; set; }
        public int? SoLanTraLoiDung { get; set; }

        // Quan hệ với bảng đề thi
        public virtual ICollection<CauHoi_De> CauHoi_De { get; set; }
    }
}
