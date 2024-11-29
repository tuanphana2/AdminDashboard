using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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

        // Hiển thị danh sách tất cả các danh mục sự kiện
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryEventRepository.GetAllAsync();
            return View(categories);
        }

        // Hiển thị trang chỉnh sửa CategoryEvent
        public async Task<IActionResult> Edit(string id)
        {
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

        // Xử lý yêu cầu cập nhật CategoryEvent
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

        // Hiển thị trang tạo mới CategoryEvent
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> CategoryEventChart(IMongoDatabase database)
        {
            var eventsCollection = database.GetCollection<Event>("Events");
            var categoryEventCounts = await _categoryEventRepository.GetEventCountByCategoryAsync(eventsCollection);

            ViewBag.CategoryNames = categoryEventCounts.Keys.ToArray();
            ViewBag.EventCounts = categoryEventCounts.Values.ToArray();

            return View();
        }

        // Xử lý yêu cầu tạo mới CategoryEvent
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

        // Hiển thị trang xác nhận xóa CategoryEvent
        public async Task<IActionResult> Delete(string id)
        {
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

        // Xử lý yêu cầu xóa CategoryEvent
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
    }
}
