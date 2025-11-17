using Microsoft.AspNetCore.SignalR;
using WebBaiGiang_CKC.Data;
using WebBaiGiang_CKC.Hubs;
using WebBaiGiang_CKC.Models;

public class NotificationService
{
    private readonly WebBaiGiangContext _context;
    private readonly IHubContext<NotificationsHub> _hub;

    public NotificationService(WebBaiGiangContext context, IHubContext<NotificationsHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    public async Task SendToTeacher(int teacherAccountId, string title, string message, int type, string url)
    {
        // 1) Lưu DB
        var noti = new Notification
        {
            UserId = teacherAccountId,
            Title = title,
            Message = message,
            Type = type,
            Url = url,
            CreatedAt = DateTime.Now,
            IsRead = false
        };

        _context.Notifications.Add(noti);
        await _context.SaveChangesAsync();

        // 2) Gửi realtime qua SignalR
        await _hub.Clients.Group($"user:{teacherAccountId}")
            .SendAsync("ReceiveNotification", new
            {
                id = noti.Id,          // ⭐ QUAN TRỌNG: để click MarkAsRead
                title = noti.Title,
                message = noti.Message,
                type = noti.Type,
                createdAt = noti.CreatedAt,
                url = noti.Url,
                isRead = noti.IsRead   // ⭐ QUAN TRỌNG: frontend dùng để quyết định + badge hay không
            });
    }
}
