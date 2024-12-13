using MongoDB.Driver;
using AdminDashboard.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver.Linq;

namespace AdminDashboard.Repositories
{
    public class NotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;

        public NotificationRepository(IMongoDatabase database)
        {
            _notifications = database.GetCollection<Notification>("notifications");
        }

        // Lấy tất cả thông báo
        public async Task<List<Notification>> GetAllNotificationsAsync() =>
            await _notifications.Find(notification => true).ToListAsync();

        // Lấy thông báo theo ID
        public async Task<Notification> GetNotificationByIdAsync(string id) =>
            await _notifications.Find(notification => notification.Id == id).FirstOrDefaultAsync();

        // Thêm thông báo mới
        public async Task AddNotificationAsync(Notification notification) =>
            await _notifications.InsertOneAsync(notification);

        // Cập nhật thông báo
        public async Task<bool> UpdateNotificationAsync(string id, Notification notification)
        {
            var result = await _notifications.ReplaceOneAsync(n => n.Id == id, notification);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        // Xóa thông báo theo ID
        public async Task DeleteNotificationAsync(string id) =>
            await _notifications.DeleteOneAsync(notification => notification.Id == id);

        // Lấy thông báo với phân trang và tìm kiếm
        public List<Notification> GetNotificationsByPage(int page, string searchQuery, out int totalNotifications)
        {
            const int pageSize = 10; // Số lượng thông báo trên mỗi trang

            // Đảm bảo page luôn là một số dương và ít nhất là 1
            if (page < 1)
            {
                page = 1; // Nếu page nhỏ hơn 1, gán lại về trang 1
            }

            // Tạo truy vấn cơ bản
            var query = _notifications.AsQueryable();

            // Nếu có tìm kiếm, thêm điều kiện lọc vào truy vấn
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(n => n.Title.Contains(searchQuery) || n.Description.Contains(searchQuery));
            }

            // Lấy tổng số thông báo phù hợp với điều kiện tìm kiếm
            totalNotifications = query.Count();

            // Phân trang: bỏ qua các bản ghi trước đó và chỉ lấy bản ghi hiện tại
            var notifications = query
                .Skip((page - 1) * pageSize)  // Bỏ qua các bản ghi của các trang trước
                .Take(pageSize)               // Lấy số lượng thông báo cho trang hiện tại
                .ToList();

            return notifications;
        }
    }
}
