using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AdminDashboard.Models;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminRepository _adminRepository;

        public AdminController(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // Trang thông tin của Admin
        public async Task<IActionResult> Information()
        {
            // Lấy thông tin email từ Session
            var adminEmail = HttpContext.Session.GetString("AdminEmail");

            if (string.IsNullOrEmpty(adminEmail))
            {
                // Nếu không có email trong session, chuyển hướng đến trang Login
                return RedirectToAction("Login", "Login");
            }

            // Lấy thông tin admin từ database theo email
            var admin = await _adminRepository.GetAdminByUsernameAsync(adminEmail);

            if (admin == null)
            {
                // Nếu không tìm thấy admin trong cơ sở dữ liệu, có thể redirect hoặc thông báo lỗi
                return RedirectToAction("Login", "Login");
            }

            // Trả về view với thông tin admin
            return View(admin);
        }

        // GET: Admin/Edit/{email}
        public async Task<IActionResult> Edit(string id)
        {
            // Lấy thông tin admin từ database bằng email
            var admin = await _adminRepository.GetAdminByIdAsync(id);
            if (admin == null)
            {
                return NotFound(); // Nếu không tìm thấy admin
            }

            return View(admin); // Truyền model vào view
        }

        // POST: Admin/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Admin admin)
        {
            if (ModelState.IsValid)
            {
                // Cập nhật thông tin admin trong DB
                var result = await _adminRepository.UpdateAdminAsync(admin);

                if (result)
                {
                    return RedirectToAction("Information"); // Quay lại trang thông tin admin
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unable update information!");
                }
            }

            return View(admin);
        }

        public IActionResult ChangePassword(string id)
        {
            var admin = _adminRepository.GetAdminByIdAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(new ChangePassword());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePassword model, string id)
        {
            if (ModelState.IsValid)
            {
                var admin = await _adminRepository.GetAdminByIdAsync(id);
                if (admin == null)
                {
                    return NotFound();
                }

                // Kiểm tra mật khẩu cũ
                if (admin.Password != model.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Old password is incorrect!");
                    TempData["ErrorMessage"] = "Old password is incorrect!";
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                admin.Password = model.NewPassword;
                var result = await _adminRepository.ChangePasswordAsync(admin);

                if (result)
                {
                    TempData["SuccessMessage"] = "Change password successfully!";
                    return RedirectToAction("Information");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Unable to update password!");
                }
            }

            return View(model);
        }


        // Đăng xuất, xóa thông tin Admin khỏi Session
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminEmail"); // Xóa thông tin Admin trong session
            return RedirectToAction("Login", ""); // Chuyển hướng đến trang login
        }
    }
}
