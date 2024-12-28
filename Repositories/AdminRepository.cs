using AdminDashboard.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace AdminDashboard.Repositories
{
    public class AdminRepository
    {
        private readonly IMongoCollection<Admin> _admins;

        public AdminRepository(IMongoDatabase database)
        {
            _admins = database.GetCollection<Admin>("admins");
        }
        public async Task<Admin> GetAdminByIdAsync(string id)
        {
            return await _admins.Find(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Admin> GetAdminByUsernameAsync(string username)
        {
            return await _admins.Find(a => a.Email == username).FirstOrDefaultAsync();
        }

        public async Task<Admin> AuthenticateAdminAsync(string email, string password)
        {
            var admin = await _admins.Find(a => a.Email == email).FirstOrDefaultAsync();

            if (admin == null)
            {
                return null;
            }

            if (admin.Password == password)
            {
                return admin;
            }

            return null;
        }

        public async Task<bool> UpdateAdminAsync(Admin updatedAdmin)
        {
            var filter = Builders<Admin>.Filter.Eq(a => a.Email, updatedAdmin.Email);

            var update = Builders<Admin>.Update
                .Set(a => a.Name, updatedAdmin.Name)
                .Set(a => a.Phone, updatedAdmin.Phone);

            var result = await _admins.UpdateOneAsync(filter, update);

            return result.MatchedCount > 0 && result.ModifiedCount > 0;
        }
        public async Task<bool> ChangePasswordAsync(Admin updatedAdmin)
        {
            var filter = Builders<Admin>.Filter.Eq(a => a.Email, updatedAdmin.Email);

            var update = Builders<Admin>.Update
                .Set(a => a.Password, updatedAdmin.Password);

            var result = await _admins.UpdateOneAsync(filter, update);

            return result.MatchedCount > 0 && result.ModifiedCount > 0;
        }
    }
}
