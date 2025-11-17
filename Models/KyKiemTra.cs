using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace WebBaiGiang_CKC.Models
{
    public class KyKiemTra : IValidatableObject
    {
        [DisplayName("ID kỳ kiểm tra")]
        public int KyKiemTraId { get; set; }

        [DisplayName("Tên kỳ kiểm tra")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenKyKiemTra { get; set; }
        [DisplayName("Thời gian bắt đầu")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public DateTime ThoiGianBatDau { get; set; }

        [DisplayName("Thời gian kết thúc")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public DateTime ThoiGianKetThuc { get; set; }

        [DisplayName("Thời gian (phút)")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        [Range(0, int.MaxValue, ErrorMessage = "{0} phải là số nguyên dương không âm")]
        public int ThoiGianLamBai { get; set; }

        public virtual ICollection<De> De { get; set; }
        public virtual ICollection<DanhSachThi> DanhSachThi { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ThoiGianBatDau <= DateTime.Now)
            {
                yield return new ValidationResult("Thời gian bắt đầu phải lớn hơn thời điểm hiện tại", new[] { "ThoiGianBatDau" });
            }

            if (ThoiGianKetThuc <= ThoiGianBatDau)
            {
                yield return new ValidationResult("Thời gian kết thúc phải lớn hơn thời gian bắt đầu", new[] { "ThoiGianKetThuc" });
            }
        }
    }
}
