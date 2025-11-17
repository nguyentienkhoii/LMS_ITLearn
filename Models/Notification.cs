using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBaiGiang_CKC.Models
{
    [Table("NOTIFICATION")]
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }      // Mã tài khoản giảng viên
        public string Title { get; set; }
        public string Message { get; set; }

        public int Type { get; set; }        // 1 = nhắc chấm, 2 = học viên nộp bài, 3 = admin duyệt
        public bool IsRead { get; set; } = false;

        public string Url { get; set; }      // ⭐ Link đến bài tập / bài nộp

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
