using AdminDashboard.Repositories;
using AdminDashboard.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AdminDashboard.Controllers
{
    public class AdminController : Controller
    {
        private readonly AdminRepository _adminRepository;

        public AdminController(AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<IActionResult> MyDetails()
        {
            string adminId = HttpContext.Session.GetString("AdminId");

            if (string.IsNullOrEmpty(adminId))
            {
                TempData["ErrorMessage"] = "You must be logged in to view details.";
                return RedirectToAction("Login", "Login");
            }

            var admin = await _adminRepository.GetAdminByUsernameAsync(adminId);

            if (admin == null)
            {
                return NotFound("Admin details not found.");
            }

            return View(admin);
        }
    }
}
