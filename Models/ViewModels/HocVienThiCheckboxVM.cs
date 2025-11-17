namespace WebBaiGiang_CKC.Models.ViewModels
{
    public class HocVienThiCheckboxVM
    {
        public int MaHocVien { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public bool DaTrongDanhSach { get; set; }

        // ✅ Thêm hai dòng này để hiển thị và lọc theo lớp
        public int? MaLopHoc { get; set; }
        public string? TenLopHoc { get; set; }
    }

    public class CapNhatDanhSachThiRequest
    {
        public int KyKiemTraId { get; set; }
        public List<int> MaHocViens { get; set; } = new();
    }
}
