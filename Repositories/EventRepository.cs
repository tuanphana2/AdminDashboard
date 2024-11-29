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

        // Lấy sự kiện theo Id
        public async Task<Event> GetByIdAsync(string id)
        {
            if (ObjectId.TryParse(id, out var objectId))
            {
                return await _events.Find(e => e.Id == objectId.ToString()).FirstOrDefaultAsync();
            }
            throw new ArgumentException("Invalid ObjectId format", nameof(id));
        }

        // Tạo sự kiện mới
        public async Task CreateAsync(Event evnt)
        {
            evnt.CreatedAt = DateTime.UtcNow;
            evnt.UpdatedAt = DateTime.UtcNow;
            await _events.InsertOneAsync(evnt);
        }

        // Cập nhật sự kiện theo Id
        public async Task UpdateAsync(string id, Event evnt)
        {
            if (ObjectId.TryParse(id, out var objectId))
            {
                evnt.UpdatedAt = DateTime.UtcNow;
                await _events.ReplaceOneAsync(e => e.Id == objectId.ToString(), evnt);
            }
            else
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(id));
            }
        }

        // Xóa sự kiện theo Id
        public async Task DeleteAsync(string id)
        {
            if (ObjectId.TryParse(id, out var objectId))
            {
                await _events.DeleteOneAsync(e => e.Id == objectId.ToString());
            }
            else
            {
                throw new ArgumentException("Invalid ObjectId format", nameof(id));
            }
        }
    }
}
