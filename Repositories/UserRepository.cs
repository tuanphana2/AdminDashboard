using AdminDashboard.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AdminDashboard.Repositories
{
    public class UserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("accountusers");
        }

        // Lấy tất cả người dùng
        public async Task<List<User>> GetAllAsync() => await _users.Find(_ => true).ToListAsync();

        // Lấy người dùng theo Id
        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        // Lấy tất cả người dùng có vai trò "organizer"
        public async Task<List<User>> GetOrganizersAsync()
        {
            return await _users.Find(user => user.Role == "organizer").ToListAsync();
        }

        // Tạo người dùng mới
        public async Task CreateAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString(); // Tạo Id mới dưới dạng chuỗi nếu không có Id
            }
            user.CreatedAt = DateTime.UtcNow.Date;
            user.UpdatedAt = DateTime.UtcNow.Date;
            await _users.InsertOneAsync(user);
        }

        // Cập nhật người dùng theo Id
        public async Task UpdateAsync(string id, User user)
        {
            user.UpdatedAt = DateTime.UtcNow.Date;
            await _users.ReplaceOneAsync(u => u.Id == id, user);
        }

        // Xóa người dùng theo Id
        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }

        // Lấy số lượng người dùng theo vai trò
        public async Task<int> GetCountByRoleAsync(string role)
        {
            var count = await _users.CountDocumentsAsync(user => user.Role == role);
            return (int)count;
        }

        // Lấy số lượng người dùng được tạo trong 3 tháng qua
        public async Task<int> GetUserCountByRecentAsync()
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var count = await _users.CountDocumentsAsync(user => user.CreatedAt >= threeMonthsAgo);
            return (int)count;
        }
    }
}
