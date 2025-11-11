using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Models;

namespace WebBaiGiang_CKC.Data
{
    public class WebBaiGiangContext : DbContext
    {
        public WebBaiGiangContext(DbContextOptions<WebBaiGiangContext> options) : base(options) { }

        public DbSet<Muc> Muc { get; set; }
        public DbSet<Bai> Bai { get; set; }

        public DbSet<KyKiemTra> KyKiemTra { get; set; }

        public DbSet<CauHoi> CauHoi { get; set; }
        public DbSet<De> De { get; set; }
        public DbSet<DanhSachThi> DanhSachThi { get; set; }

        public DbSet<CauHoi_De> CauHoi_De { get; set; }
        public DbSet<BaiLam> BaiLam { get; set; }
        public DbSet<CauHoi_BaiLam> CauHoi_BaiLam { get; set; }

        // Các DbSet mới
        public DbSet<TaiKhoanNew> TaiKhoanNews { get; set; }  // Thêm bảng TaiKhoanNew
        public DbSet<GiangVien> GiangViens { get; set; }      // Thêm bảng GiangVien
        public DbSet<HocVien> HocViens { get; set; }          // Thêm bảng HocVien
        //them mon hoc, lop hoc
        public DbSet<KhoaHoc> KhoaHocs { get; set; }
        public DbSet<LopHoc> LopHocs { get; set; }
        public DbSet<ChuongNew> ChuongNews { get; set; }

        public DbSet<TaiLieu> TaiLieus { get; set; }




        public DbSet<BaiTap> BaiTaps { get; set; }
        public DbSet<BaiTapNop> BaiTapNops { get; set; }






        // Đã thêm DbSet cho DangKyMonHoc
        public DbSet<HocVien_LopHoc> HocVien_LopHoc { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập quan hệ một-một giữa TaiKhoanNew và GiangVien
            modelBuilder.Entity<GiangVien>()
                .HasOne(g => g.TaiKhoan)
                .WithOne(t => t.GiangVien)
                .HasForeignKey<GiangVien>(g => g.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Restrict);

            // Thiết lập quan hệ một-một giữa TaiKhoanNew và HocVien
            modelBuilder.Entity<HocVien>()
                .HasOne(h => h.TaiKhoan)
                .WithOne(t => t.HocVien)
                .HasForeignKey<HocVien>(h => h.MaTaiKhoan)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LopHoc>()
                .HasOne(l => l.KhoaHoc)
                .WithMany(k => k.LopHocs)
                .HasForeignKey(l => l.MaKhoaHoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LopHoc>()
                .HasOne(l => l.GiangVien)
                .WithMany(g => g.LopHocs)
                .HasForeignKey(l => l.MaGiangVien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChuongNew>()
                .HasOne(c => c.LopHoc)
                .WithMany(l => l.Chuongs)
                .HasForeignKey(c => c.MaLopHoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bai>()
                .HasOne(b => b.Chuong)
                .WithMany(c => c.Bais)
                .HasForeignKey(b => b.MaChuong)
                .HasPrincipalKey(c => c.MaChuong)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HocVien_LopHoc>()
                 .HasKey(hv => new { hv.MaHocVien, hv.MaLopHoc }); // ✅ Khóa chính kép

            modelBuilder.Entity<HocVien_LopHoc>()
                .HasOne(hv => hv.HocVien)
                .WithMany(h => h.HocVien_LopHocs)
                .HasForeignKey(hv => hv.MaHocVien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HocVien_LopHoc>()
                .HasOne(hv => hv.LopHoc)
                .WithMany(l => l.HocVien_LopHocs)
                .HasForeignKey(hv => hv.MaLopHoc)
                .OnDelete(DeleteBehavior.Restrict);

            // Ràng buộc 1-n: 1 Mục có nhiều Tài liệu
            modelBuilder.Entity<TaiLieu>()
                .HasOne(t => t.Muc)
                .WithMany(m => m.TaiLieus)
                .HasForeignKey(t => t.MaMucCon)
                .OnDelete(DeleteBehavior.Cascade);



            // 🔹 Cấu hình BaiTap ↔ BaiGiang (1 - nhiều)
            // ============================
            modelBuilder.Entity<BaiTap>()
                .HasOne(bt => bt.Bai)
                .WithMany(bg => bg.BaiTaps)     // giả sử bạn có ICollection<BaiTap> trong BaiGiang
                .HasForeignKey(bt => bt.BaiId)
                .OnDelete(DeleteBehavior.Cascade);  // Khi xóa bài giảng → xóa luôn bài tập

            // ============================
            // 🔹 Cấu hình BaiTapNop ↔ BaiTap (1 - nhiều)
            // ============================
            modelBuilder.Entity<BaiTapNop>()
                .HasOne(btn => btn.BaiTap)
                .WithMany(bt => bt.BaiTapNops)
                .HasForeignKey(btn => btn.MaBaiTap)
                .OnDelete(DeleteBehavior.Cascade);  // Khi xóa bài tập → xóa luôn bài nộp

            // ============================
            // 🔹 Cấu hình BaiTapNop ↔ HocVien (1 - nhiều)
            // ============================
            modelBuilder.Entity<BaiTapNop>()
                .HasOne(btn => btn.HocVien)
                .WithMany(hv => hv.BaiTapNops)     // thêm ICollection<BaiTapNop> vào HocVien nếu chưa có
                .HasForeignKey(btn => btn.MaHocVien)
                .OnDelete(DeleteBehavior.Cascade); // Khi xóa học viên → xóa luôn bài nộp


        }
    }
}
