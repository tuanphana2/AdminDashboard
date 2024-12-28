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

        public async Task<List<Notification>> GetAllNotificationsAsync() =>
            await _notifications.Find(notification => true).ToListAsync();

        public async Task<Notification> GetNotificationByIdAsync(string id) =>
            await _notifications.Find(notification => notification.Id == id).FirstOrDefaultAsync();

        public async Task AddNotificationAsync(Notification notification) =>
            await _notifications.InsertOneAsync(notification);

        public async Task<bool> UpdateNotificationAsync(string id, Notification notification)
        {
            var result = await _notifications.ReplaceOneAsync(n => n.Id == id, notification);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }

        public async Task DeleteNotificationAsync(string id) =>
            await _notifications.DeleteOneAsync(notification => notification.Id == id);

        public List<Notification> GetNotificationsByPage(int page, string searchQuery, out int totalNotifications)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            var query = _notifications.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(n => n.Title.Contains(searchQuery) || n.Description.Contains(searchQuery));
            }

            totalNotifications = query.Count();

            var notifications = query
                .Skip((page - 1) * pageSize)  
                .Take(pageSize)               
                .ToList();

            return notifications;
        }
    }
}
