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

        public async Task<Admin> GetAdminByUsernameAsync(string username)
        {
            return await _admins.Find(a => a.Email == username).FirstOrDefaultAsync();
        }
    }
}
