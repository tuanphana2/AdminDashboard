using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class LoginController : Controller
    {
        private readonly AdminRepository _adminRepository;

        public LoginController(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        // GET: Login Page
        public IActionResult Login()
        {
            return View(new Login());
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                // Xác thực người dùng qua email và mật khẩu
                var admin = await _adminRepository.AuthenticateAdminAsync(model.Email, model.Password);

                if (admin != null)
                {
                    // Lưu thông tin vào session
                    HttpContext.Session.SetString("AdminEmail", admin.Email);

                    // Chuyển hướng đến trang thông tin
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Thêm thông báo lỗi nếu đăng nhập thất bại
                    ModelState.AddModelError(string.Empty, "Wrong Email or Password!");
                }
            }

            // Nếu đăng nhập thất bại, hiển thị lại form đăng nhập với lỗi
            return View(model);
        }

        // Trang đăng xuất
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();  // Xóa Session
            return RedirectToAction("Login");
        }
    }
}
