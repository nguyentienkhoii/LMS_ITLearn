using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBaiGiang_CKC.Models;

[Table("HOCVIEN_LOPHOC")]
public class HocVien_LopHoc
{
    // 🔹 Khóa chính kép
    [Key, Column(Order = 0)]
    [ForeignKey("HocVien")]
    [Display(Name = "Mã học viên")]
    public int MaHocVien { get; set; }

    [Key, Column(Order = 1)]
    [ForeignKey("LopHoc")]
    [Display(Name = "Mã lớp học")]
    public int MaLopHoc { get; set; }

    // 🔹 Navigation properties
    public HocVien HocVien { get; set; }
    public LopHoc LopHoc { get; set; }
}
