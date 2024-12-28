using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class CategoryEventController : Controller
    {
        private readonly CategoryEventRepository _categoryEventRepository;

        public CategoryEventController(CategoryEventRepository categoryEventRepository)
        {
            _categoryEventRepository = categoryEventRepository;
        }

        public async Task<IActionResult> Index(int page = 1, string searchQuery = "")
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");

            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }

            int itemsPerPage = 10;

            var (categories, totalCategories) = await _categoryEventRepository.GetCategoriesByPageAsync(page, itemsPerPage, searchQuery);

            int totalPages = (int)Math.Ceiling((double)totalCategories / itemsPerPage);

            page = Math.Max(1, Math.Min(page, totalPages));

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchQuery"] = searchQuery;

            return View(categories);
        }

        public IActionResult Create()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CategoryEvent categoryEvent)
        {
            if (!ModelState.IsValid)
            {
                return View(categoryEvent);
            }

            try
            {
                await _categoryEventRepository.CreateAsync(categoryEvent);
                TempData["SuccessMessage"] = "Category Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occurred while creating the category event.");
                return View(categoryEvent);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Category ID is required.");
            }

            var categoryEvent = await _categoryEventRepository.GetByIdAsync(id);
            if (categoryEvent == null)
            {
                return NotFound("Category event not found.");
            }

            return View(categoryEvent);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, CategoryEvent categoryEvent)
        {
            if (id != categoryEvent.Id)
            {
                ModelState.AddModelError(string.Empty, "Category ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return View(categoryEvent);
            }

            try
            {
                await _categoryEventRepository.UpdateAsync(id, categoryEvent);
                TempData["SuccessMessage"] = "Category Event updated successfully!";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Category event not found for update.");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An error occurred while updating the category event.");
                return View(categoryEvent);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Category ID is required.");
            }

            var categoryEvent = await _categoryEventRepository.GetByIdAsync(id);
            if (categoryEvent == null)
            {
                return NotFound("Category event not found.");
            }

            return View(categoryEvent);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _categoryEventRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "Category Event deleted successfully!";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Category event not found for deletion.");
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the category event.";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> CategoryEventChart(IMongoDatabase database)
        {
            var eventsCollection = database.GetCollection<Event>("Events");
            var categoryEventCounts = await _categoryEventRepository.GetEventCountByCategoryAsync(eventsCollection);

            ViewBag.CategoryNames = categoryEventCounts.Keys.ToArray();
            ViewBag.EventCounts = categoryEventCounts.Values.ToArray();

            return View();
        }
    }
}
