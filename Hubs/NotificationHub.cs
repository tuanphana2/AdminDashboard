// Hubs/NotificationHub.cs
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace AdminDashboard.Hubs
{
    public class NotificationHub : Hub
    {
        // Sử dụng ConcurrentDictionary để lưu trữ mapping giữa userId và connectionId
        private static ConcurrentDictionary<string, string> _userConnections = new ConcurrentDictionary<string, string>();

        // Khi người dùng kết nối
        public override Task OnConnectedAsync()
        {
            // Lấy userId từ Context.User nếu bạn đã thiết lập xác thực
            string userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections[userId] = Context.ConnectionId;
                Groups.AddToGroupAsync(Context.ConnectionId, userId); // Thêm vào group tương ứng với userId
            }
            return base.OnConnectedAsync();
        }

        // Khi người dùng ngắt kết nối
        public override Task OnDisconnectedAsync(System.Exception exception)
        {
            string userId = Context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnections.TryRemove(userId, out _);
                Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        // Phương thức gửi thông báo tới một người dùng cụ thể
        public async Task SendNotificationToUser(string userId, string title, string body)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", title, body);
        }

        // Phương thức gửi thông báo tới nhiều người dùng
        public async Task SendNotificationToUsers(List<string> userIds, string title, string body)
        {
            foreach (var userId in userIds)
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", title, body);
            }
        }
    }
}
