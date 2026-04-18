using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace backend_project.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("uid")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // إضافة المستخدم إلى مجموعة خاصة به
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            
            Console.WriteLine($"User {userId} connected to NotificationHub");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("uid")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            Console.WriteLine($"User {userId} disconnected from NotificationHub");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // طريقة لاختبار الاتصال
    public async Task SendMessage(string message)
    {
        var userId = Context.User?.FindFirst("uid")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Clients.User(userId).SendAsync("ReceiveMessage", message);
        }
    }
}