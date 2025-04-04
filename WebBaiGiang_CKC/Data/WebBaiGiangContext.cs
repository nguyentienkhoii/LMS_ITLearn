using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Data
{
    public class WebBaiGiangContext : DbContext
    {
        public WebBaiGiangContext(DbContextOptions<WebBaiGiangContext> options) : base(options) { }

        public DbSet<Muc> Muc { get; set; }
        public DbSet<Bai> Bai { get; set; }
        public DbSet<Chuong> Chuong { get; set; }
        public DbSet<KyKiemTra> KyKiemTra { get; set; }
        public DbSet<MonHoc> MonHoc { get; set; }
        public DbSet<CauHoi> CauHoi { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<De> De { get; set; }
        public DbSet<DanhSachThi> DanhSachThi { get; set; }
        public DbSet<GiaoVien> GiaoVien { get; set; }
        public DbSet<CauHoi_De> CauHoi_De { get; set; }
        public DbSet<BaiLam> BaiLam { get; set; }
        public DbSet<CauHoi_BaiLam> CauHoi_BaiLam { get; set; }

        //  Thêm bảng DeCuong và BaiTap
        public DbSet<DeCuong> DeCuong { get; set; }
        public DbSet<BaiTap> BaiTap { get; set; }

        // Đã thêm DbSet cho DangKyMonHoc
        public DbSet<DangKyMonHoc> DangKyMonHoc { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập quan hệ giữa DangKyMonHoc và TaiKhoan
            modelBuilder.Entity<DangKyMonHoc>()
                .HasOne(dk => dk.TaiKhoan)
                .WithMany(tk => tk.DangKyMonHoc)
                .HasForeignKey(dk => dk.TaiKhoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thiết lập quan hệ giữa DangKyMonHoc và MonHoc
            modelBuilder.Entity<DangKyMonHoc>()
                .HasOne(dk => dk.MonHoc)
                .WithMany(mh => mh.DangKyMonHoc)
                .HasForeignKey(dk => dk.MonHocId)
                .OnDelete(DeleteBehavior.Cascade);

            // Thêm chỉ mục duy nhất cho cặp TaiKhoanId và MonHocId
            modelBuilder.Entity<DangKyMonHoc>()
                .HasIndex(dk => new { dk.TaiKhoanId, dk.MonHocId })
                .IsUnique();
        }
    }
}
