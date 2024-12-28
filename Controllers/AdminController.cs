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

        public async Task<IActionResult> Information()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");

            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var admin = await _adminRepository.GetAdminByUsernameAsync(adminEmail);

            if (admin == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View(admin);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var admin = await _adminRepository.GetAdminByIdAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Admin admin)
        {
            if (ModelState.IsValid)
            {
                var result = await _adminRepository.UpdateAdminAsync(admin);

                if (result)
                {
                    return RedirectToAction("Information");
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
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
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

                if (admin.Password != model.OldPassword)
                {
                    ModelState.AddModelError("OldPassword", "Old password is incorrect!");
                    TempData["ErrorMessage"] = "Old password is incorrect!";
                    return View(model);
                }

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

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminEmail");
            return RedirectToAction("Login", "");
        }
    }
}
