using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class EventController : Controller
    {
        private readonly EventRepository _eventRepository;
        private readonly UserRepository _userRepository;
        private readonly CategoryEventRepository _categoryEventRepository;

        public EventController(EventRepository eventRepository, UserRepository userRepository, CategoryEventRepository categoryEventRepository)
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _categoryEventRepository = categoryEventRepository;
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            int pageSize = 10;

            var events = await _eventRepository.GetAllAsync();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                events = events.Where(e => e.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                            e.Location.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var totalEvents = events.Count();
            var totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize);
            var pagedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            page = Math.Max(1, Math.Min(page, totalPages));

            ViewData["SearchQuery"] = searchQuery;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PreviousPage"] = page > 1 ? page - 1 : 1;
            ViewData["NextPage"] = page < totalPages ? page + 1 : totalPages;

            return View(pagedEvents);
        }

        public async Task<IActionResult> Create()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            await SetUpViewBags();
            return View(new Event());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event evnt)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data provided. Please correct the errors and try again.";
                await SetUpViewBags();
                return View(evnt);
            }

            evnt.CreatedAt = DateTime.UtcNow;
            evnt.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _eventRepository.CreateAsync(evnt);

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating event: {ex.Message}");

                TempData["ErrorMessage"] = "An unexpected error occurred while creating the event. Please try again later.";
                await SetUpViewBags();
                return View(evnt);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var evnt = await _eventRepository.GetByIdAsync(id);
            if (evnt == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            await SetUpViewBags();
            return View(evnt);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Event evnt)
        {
            if (id != evnt.Id)
            {
                TempData["ErrorMessage"] = "Mismatched event ID.";
                return View(evnt);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data provided. Please correct the errors and try again.";
                await SetUpViewBags();
                return View(evnt);
            }

            evnt.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _eventRepository.UpdateAsync(id, evnt);
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating event: {ex.Message}";
                await SetUpViewBags();
                return View(evnt);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var evnt = await _eventRepository.GetByIdAsync(id);
            if (evnt == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(evnt);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _eventRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Event deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting event: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task SetUpViewBags()
        {
            var organizers = await _userRepository.GetOrganizersAsync();
            var categories = await _categoryEventRepository.GetAllAsync();

            ViewBag.Organizers = organizers != null && organizers.Any()
                ? new SelectList(organizers, "Id", "Name")
                : new SelectList(new List<SelectListItem> { new SelectListItem { Text = "No Organizers Available", Value = "" } });

            ViewBag.Categories = categories != null && categories.Any()
                ? new SelectList(categories, "Id", "Name")
                : new SelectList(new List<SelectListItem> { new SelectListItem { Text = "No Categories Available", Value = "" } });
        }
    }
}
