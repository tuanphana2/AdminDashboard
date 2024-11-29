using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
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

        // Hiển thị danh sách sự kiện
        public async Task<IActionResult> Index()
        {
            var events = await _eventRepository.GetAllAsync();
            return View(events);
        }

        // Hiển thị trang tạo sự kiện mới
        public async Task<IActionResult> Create()
        {
            ViewBag.Organizers = new SelectList(await _userRepository.GetOrganizersAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _categoryEventRepository.GetAllAsync(), "Id", "Name");
            return View();
        }

        // Xử lý yêu cầu tạo sự kiện mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event evnt)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data provided. Please correct the errors and try again.";
                ViewBag.Organizers = new SelectList(await _userRepository.GetOrganizersAsync(), "Id", "Name");
                ViewBag.Categories = new SelectList(await _categoryEventRepository.GetAllAsync(), "Id", "Name");
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
                TempData["ErrorMessage"] = $"Error creating event: {ex.Message}";
                ViewBag.Organizers = new SelectList(await _userRepository.GetOrganizersAsync(), "Id", "Name");
                ViewBag.Categories = new SelectList(await _categoryEventRepository.GetAllAsync(), "Id", "Name");
                return View(evnt);
            }
        }

        // Hiển thị trang chỉnh sửa sự kiện
        public async Task<IActionResult> Edit(string id)
        {
            var evnt = await _eventRepository.GetByIdAsync(id);
            if (evnt == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Organizers = new SelectList(await _userRepository.GetOrganizersAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _categoryEventRepository.GetAllAsync(), "Id", "Name");
            return View(evnt);
        }

        // Xử lý yêu cầu cập nhật sự kiện
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
                ViewBag.Organizers = new SelectList(await _userRepository.GetOrganizersAsync(), "Id", "Name");
                ViewBag.Categories = new SelectList(await _categoryEventRepository.GetAllAsync(), "Id", "Name");
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
                ViewBag.Organizers = new SelectList(await _userRepository.GetOrganizersAsync(), "Id", "Name");
                ViewBag.Categories = new SelectList(await _categoryEventRepository.GetAllAsync(), "Id", "Name");
                return View(evnt);
            }
        }

        // Hiển thị trang xác nhận xóa sự kiện
        public async Task<IActionResult> Delete(string id)
        {
            var evnt = await _eventRepository.GetByIdAsync(id);
            if (evnt == null)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(evnt);
        }

        // Xử lý yêu cầu xóa sự kiện
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Event ID is missing. Unable to delete the event.";
                return RedirectToAction(nameof(Index));
            }

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
    }
}
