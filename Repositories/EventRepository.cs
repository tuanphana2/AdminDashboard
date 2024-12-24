using AdminDashboard.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace AdminDashboard.Repositories
{
    public class EventRepository
    {
        private readonly IMongoCollection<Event> _events;

        public EventRepository(IMongoDatabase database)
        {
            _events = database.GetCollection<Event>("events");
        }

        // Phương thức trả về bộ sưu tập Event
        public IMongoCollection<Event> GetCollection()
        {
            return _events;
        }

        // Lấy tất cả các sự kiện
        public async Task<List<Event>> GetAllAsync()
        {
            return await _events.Find(_ => true).ToListAsync();
        }

        // Lấy số lượng sự kiện diễn ra trong tháng hiện tại
        public async Task<int> GetEventCountThisMonthAsync()
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Cuối tháng

            var count = await _events.CountDocumentsAsync(e => e.Date >= startOfMonth && e.Date <= endOfMonth);
            return (int)count;
        }

        // Tìm kiếm sự kiện theo từ khóa
        public async Task<List<Event>> SearchEventsAsync(string searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery))
                return new List<Event>();

            var filter = Builders<Event>.Filter.Or(
                Builders<Event>.Filter.Regex("Name", new BsonRegularExpression(searchQuery, "i")),
                Builders<Event>.Filter.Regex("Description", new BsonRegularExpression(searchQuery, "i"))
            );

            return await _events.Find(filter).ToListAsync();
        }

        // Lấy sự kiện theo Id
        public async Task<Event> GetByIdAsync(string id)
        {
            var objectId = ParseObjectId(id);
            return await _events.Find(e => e.Id == objectId.ToString()).FirstOrDefaultAsync();
        }

        // Tạo sự kiện mới
        public async Task CreateAsync(Event evnt)
        {
            evnt.CreatedAt = DateTime.UtcNow;
            evnt.UpdatedAt = DateTime.UtcNow;
            evnt.Date = evnt.Date.AddDays(1);
            await _events.InsertOneAsync(evnt);
        }

        // Cập nhật sự kiện theo Id
        public async Task UpdateAsync(string id, Event evnt)
        {
            var objectId = ParseObjectId(id);
            evnt.UpdatedAt = DateTime.UtcNow;
            evnt.Date = evnt.Date.AddDays(1);
            await _events.ReplaceOneAsync(e => e.Id == objectId.ToString(), evnt);
        }

        // Xóa sự kiện theo Id
        public async Task DeleteAsync(string id)
        {
            var objectId = ParseObjectId(id);
            await _events.DeleteOneAsync(e => e.Id == objectId.ToString());
        }

        // Hỗ trợ chuyển đổi String -> ObjectId và ném exception nếu không hợp lệ
        private ObjectId ParseObjectId(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(id));
            }
            return objectId;
        }
    }
}
