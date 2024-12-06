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

        // Phương thức xác thực đăng nhập
        public async Task<Admin> AuthenticateAdminAsync(string email, string password)
        {
            // Lấy admin từ MongoDB bằng email
            var admin = await _admins.Find(a => a.Email == email).FirstOrDefaultAsync();

            if (admin == null)
            {
                return null; // Nếu không tìm thấy admin
            }

            // Kiểm tra mật khẩu (ở đây bạn không dùng bcrypt nữa, chỉ so sánh plaintext)
            if (admin.Password == password)
            {
                return admin; // Nếu mật khẩu đúng
            }

            return null; // Nếu mật khẩu sai
        }

        public async Task<bool> UpdateAdminAsync(Admin updatedAdmin)
        {
            // Tìm admin trong database dựa trên email
            var filter = Builders<Admin>.Filter.Eq(a => a.Email, updatedAdmin.Email);

            // Cập nhật các trường thông tin
            var update = Builders<Admin>.Update
                .Set(a => a.Name, updatedAdmin.Name)
                .Set(a => a.Phone, updatedAdmin.Phone);

            // Thực hiện cập nhật
            var result = await _admins.UpdateOneAsync(filter, update);

            // Kiểm tra xem có cập nhật thành công hay không
            return result.MatchedCount > 0 && result.ModifiedCount > 0;
        }
        public async Task<bool> ChangePasswordAsync(Admin updatedAdmin)
        {
            // Tìm admin trong database dựa trên email
            var filter = Builders<Admin>.Filter.Eq(a => a.Email, updatedAdmin.Email);

            // Cập nhật các trường thông tin
            var update = Builders<Admin>.Update
                .Set(a => a.Password, updatedAdmin.Password); // Nếu có thay đổi mật khẩu

            // Thực hiện cập nhật
            var result = await _admins.UpdateOneAsync(filter, update);

            // Kiểm tra xem có cập nhật thành công hay không
            return result.MatchedCount > 0 && result.ModifiedCount > 0;
        }
    }
}
