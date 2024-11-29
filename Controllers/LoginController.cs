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
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            var admin = await _adminRepository.GetAdminByUsernameAsync(model.Username);
            if (admin == null || admin.Password != model.Password) // Ensure password hashing is implemented in production
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            return RedirectToAction("Index", "Home"); // Redirect to admin dashboard
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
