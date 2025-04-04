using System.ComponentModel.DataAnnotations;
using WebBaiGiang_CKC.Models;

public class DangKyMonHoc
{
    [Key]
    public int DangKyMonHocId { get; set; }

    [Required]
    public int TaiKhoanId { get; set; }
    public TaiKhoan TaiKhoan { get; set; }

    [Required]
    public int MonHocId { get; set; }
    public MonHoc MonHoc { get; set; }
}
