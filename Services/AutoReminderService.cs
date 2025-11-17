using Microsoft.EntityFrameworkCore;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Services
{
    public class AutoReminderService
    {
        private readonly WebBaiGiangContext _context;
        private readonly NotificationService _noti;

        public AutoReminderService(WebBaiGiangContext context, NotificationService noti)
        {
            _context = context;
            _noti = noti;
        }

        public async Task SendReminder(int baiTapId)
        {
            var baiTap = await _context.BaiTaps
                .Include(x => x.Bai)
                    .ThenInclude(x => x.Chuong)
                        .ThenInclude(x => x.LopHoc)
                .FirstOrDefaultAsync(x => x.MaBaiTap == baiTapId);

            if (baiTap == null) return;
            if (baiTap.ReminderSent) return;

            // Lấy MaTaiKhoan của giảng viên (an toàn, không null)
            int teacherAccountId = await _context.GiangViens
                .Where(g => g.MaGiangVien == baiTap.Bai.Chuong.LopHoc.MaGiangVien)
                .Select(g => g.MaTaiKhoan)
                .FirstOrDefaultAsync();

            if (teacherAccountId <= 0) return;

            string link = $"/GiangVien/BaiTap/DanhSachBaiNop/{baiTap.MaBaiTap}";

            await _noti.SendToTeacher(
                teacherAccountId,
                "Đến hạn chấm bài",
                $"Đã đến hạn chấm bài: {baiTap.TenBaiTap}",
                1,
                link
            );

            baiTap.ReminderSent = true;
            await _context.SaveChangesAsync();
        }

    }
}
