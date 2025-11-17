using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebBaiGiang_CKC.Data;

namespace WebBaiGiang_CKC.Services
{
    public class AutoReminderHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AutoReminderHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<WebBaiGiangContext>();
                var reminder = scope.ServiceProvider.GetRequiredService<AutoReminderService>();

                // 🟡 Lấy danh sách bài tập đến giờ nhắc chấm
                var baiTapList = await context.BaiTaps
                    .Where(bt => bt.RemindToGrade != null
                              && bt.RemindToGrade <= DateTime.Now
                              && bt.ReminderSent == false)
                    .Select(bt => bt.MaBaiTap)
                    .ToListAsync();

                foreach (var baiTapId in baiTapList)
                {
                    await reminder.SendReminder(baiTapId);
                }

                // ⏳ Kiểm tra mỗi 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
