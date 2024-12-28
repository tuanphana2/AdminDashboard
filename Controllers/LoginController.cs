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

        public IActionResult Login()
        {
            return View(new Login());
        }

        [HttpPost]
        public async Task<IActionResult> Login(Login model)
        {
            if (ModelState.IsValid)
            {
                var admin = await _adminRepository.AuthenticateAdminAsync(model.Email, model.Password);

                if (admin != null)
                {
                    HttpContext.Session.SetString("AdminEmail", admin.Email);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["ErrorMessage"] = "Login failed. Please check your email and password.";
                    return RedirectToAction("Login");
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
