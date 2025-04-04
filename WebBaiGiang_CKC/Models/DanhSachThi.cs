using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace WebBaiGiang_CKC.Models
{
    public class DanhSachThi
    {
        public int DanhSachThiId { get; set; }
        [ForeignKey("TaiKhoanId")]
        public int TaiKhoanId { get; set; }
        [ForeignKey("KyKiemTraId")]
        public int KyKiemTraId { get; set; }
        [DisplayName("Đã nộp")]
        [DefaultValue(false)]
        public bool TrangThai { get; set; } = false;

        public virtual TaiKhoan TaiKhoan { get; set; }

        public virtual KyKiemTra KyKiemTra { get; set; }
    }
}
