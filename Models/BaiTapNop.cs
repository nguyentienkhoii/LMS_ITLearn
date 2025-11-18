using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("BAITAPNOP")]
    public class BaiTapNop
    {
        [Key]
        public int MaBaiTapNop { get; set; }

        [StringLength(255)]
        public string FileNop { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? NgayNop { get; set; }

        public int LanNop { get; set; } = 1;

        [Range(0, 10)]
        public double? Diem { get; set; }

        [StringLength(500)]
        public string NhanXet { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? NgayCham { get; set; }

        [StringLength(50)]
        public string TrangThai { get; set; }

        // FK → Bài tập
        [ForeignKey("BaiTap")]
        public int MaBaiTap { get; set; }
        public virtual BaiTap BaiTap { get; set; }

        // FK → Học viên
        [ForeignKey("HocVien")]
        public int MaHocVien { get; set; }
        public virtual HocVien HocVien { get; set; }
    }

         public static class SubmissionStatus
        {
            public const string ReOpened   = "REOPENED";
            public const string DaChotSoft = "DA_CHOT_SOFT"; // chưa xác nhận hẳn
            public const string DaChamChot = "DA_CHOT";      // đã công bố & khóa

            public static readonly TimeSpan Grace = TimeSpan.FromHours(1);

            public static bool IsReopened(string s)
                => string.Equals(s, ReOpened, StringComparison.OrdinalIgnoreCase);

            public static bool IsSoftLocked(string s)
                => string.Equals(s, DaChotSoft, StringComparison.OrdinalIgnoreCase);
        
            public static bool IsWithinGrace(string status, DateTime? ngayCham)
            {
                if (!string.Equals(status, DaChotSoft, StringComparison.OrdinalIgnoreCase)) return false;
                if (ngayCham == null) return false;
                return DateTime.Now < ngayCham.Value.Add(Grace);
        
            }
            public static bool IsLocked(string s, DateTime? ngayCham)
            {
                // if (!(string.Equals(s, DaChotSoft, StringComparison.OrdinalIgnoreCase) ||
                //     string.Equals(s, DaChamChot, StringComparison.OrdinalIgnoreCase))) return false;
                if (!string.Equals(s, DaChamChot, StringComparison.OrdinalIgnoreCase)) return false;
                    if (ngayCham == null) return false;
                    return DateTime.Now >= ngayCham.Value.Add(Grace);
            } 
        }
}
