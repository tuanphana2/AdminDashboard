using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class UserController : Controller
    {
        private readonly UserRepository _userRepository;

        public UserController(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            int pageSize = 10;

            var users = await _userRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                users = users.Where(u => u.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                          u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var totalUsers = users.Count();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            page = Math.Max(1, Math.Min(page, totalPages));

            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewData["SearchQuery"] = searchQuery;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PreviousPage"] = page > 1 ? page - 1 : 1;
            ViewData["NextPage"] = page < totalPages ? page + 1 : totalPages;

            return View(pagedUsers);
        }

        public IActionResult Create()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            ViewBag.Roles = new List<string> { "organizer", "attendee" };
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user, string hobbiesInput)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(user);
            }

            if (!string.IsNullOrEmpty(hobbiesInput))
            {
                try
                {
                    user.Hobbies = hobbiesInput.Split(',')
                                               .Select(h => h.Trim())
                                               .Where(h => !string.IsNullOrEmpty(h))
                                               .ToList();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while processing hobbies.";
                    Console.WriteLine($"Error processing hobbies: {ex.Message}");

                    ViewBag.Roles = new List<string> { "organizer", "attendee" };
                    return View(user);
                }
            }
            else
            {
                user.Hobbies = new List<string>();
            }

            try
            {
                await _userRepository.CreateAsync(user);

                TempData["SuccessMessage"] = "User added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while saving the user.";
                Console.WriteLine($"Error saving user: {ex.Message}");

                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                return View(user);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.HobbiesInput = string.Join(", ", user.Hobbies);

            ViewBag.Roles = new List<string> { "organizer", "attendee" };
            return View(user);
        }
        
        [HttpPost]
        public async Task<IActionResult> Edit(string id, User user, string hobbiesInput)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                ViewBag.HobbiesInput = hobbiesInput;
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                return View(user);
            }

            if (!string.IsNullOrEmpty(hobbiesInput))
            {
                try
                {
                    user.Hobbies = hobbiesInput.Split(',')
                                               .Select(h => h.Trim())
                                               .Where(h => !string.IsNullOrEmpty(h))
                                               .ToList();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while processing hobbies.";
                    Console.WriteLine($"Error processing hobbies: {ex.Message}");

                    ViewBag.Roles = new List<string> { "organizer", "attendee" };
                    ViewBag.HobbiesInput = hobbiesInput;
                    return View(user);
                }
            }
            else
            {
                user.Hobbies = new List<string>();
            }

            try
            {
                await _userRepository.UpdateAsync(id, user);
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating the user.";
                Console.WriteLine($"Error updating user: {ex.Message}");

                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                ViewBag.HobbiesInput = hobbiesInput;
                return View(user);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? NotFound() : View(user);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "User ID is missing. Unable to delete the user.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _userRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting user: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageLoginStatus()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLoginStatus(string userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.ActiveBlock = !user.ActiveBlock;
                await _userRepository.UpdateAsync(userId, user);
                TempData["SuccessMessage"] = $"Login status for {user.Name} has been updated.";
            }
            else
            {
                TempData["ErrorMessage"] = "User not found.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
