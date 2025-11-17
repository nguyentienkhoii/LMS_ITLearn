using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace WebBaiGiang_CKC.Hubs
{
[Authorize]
public class NotificationsHub : Hub
{
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"📌 Hub Connected: userId = {userId}");

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
                Console.WriteLine($"📌 Joined group user:{userId}");
            }

            await base.OnConnectedAsync();
        }

    }

}
