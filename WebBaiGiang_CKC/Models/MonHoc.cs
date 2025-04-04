using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebBaiGiang_CKC.Models
{
    public class MonHoc
    {
        public int MonHocId { get; set; }

        [DisplayName("Tên môn học")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string TenMonHoc { get; set; }

        [DisplayName("Mã môn học")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string MaMonHoc { get; set; }

        [DisplayName("Giới thiệu môn học")]
        [Column(TypeName = "ntext")]
        [Required(ErrorMessage = "{0} không được bỏ trống")]
        public string MoTa { get; set; }
        public string AnhDaiDien { get; set; }


        // Danh sách chương
        public List<Chuong> Chuongs { get; set; }

        // Thêm danh sách đề cương
        public List<DeCuong> DeCuongs { get; set; }

        // Thêm danh sách bài tập
        public List<BaiTap> BaiTaps { get; set; }
        public virtual ICollection<DangKyMonHoc> DangKyMonHoc { get; set; }
        [NotMapped]
        public int SoLuongDangKy { get; set; }

        [NotMapped]
        public int TongSoBai { get; set; } // Tổng số bài học

        // Trường không bắt buộc
        public int? GiaoVienId { get; set; }

        [ForeignKey("GiaoVienId")]
        public virtual GiaoVien GiaoVien { get; set; }
    }
}
