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

        public async Task<List<User>> GetAllAsync() => await _users.Find(_ => true).ToListAsync();

        public async Task<User> GetByIdAsync(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetOrganizersAsync()
        {
            return await _users.Find(user => user.Role == "organizer").ToListAsync();
        }

        public async Task CreateAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Id))
            {
                user.Id = Guid.NewGuid().ToString(); 
            }
            user.CreatedAt = DateTime.UtcNow.Date;
            user.UpdatedAt = DateTime.UtcNow.Date;
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateAsync(string id, User user)
        {
            user.UpdatedAt = DateTime.UtcNow.Date;
            await _users.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteAsync(string id)
        {
            await _users.DeleteOneAsync(u => u.Id == id);
        }

        public async Task<int> GetCountByRoleAsync(string role)
        {
            var count = await _users.CountDocumentsAsync(user => user.Role == role);
            return (int)count;
        }

        public async Task<int> GetUserCountByRecentAsync()
        {
            var threeMonthsAgo = DateTime.UtcNow.AddMonths(-3);
            var count = await _users.CountDocumentsAsync(user => user.CreatedAt >= threeMonthsAgo);
            return (int)count;
        }

        public async Task<int> GetTotalUserCountAsync()
        {
            return (int)await _users.CountDocumentsAsync(FilterDefinition<User>.Empty);
        }

        public async Task<List<string>> GetEmailsAsync()
        {
            var emails = await _users.Find(_ => true)
                                     .Project(u => u.Email)
                                     .ToListAsync();
            return emails;
        }
    }
}
