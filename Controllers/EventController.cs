﻿using AdminDashboard.Models;
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

        // Hiển thị danh sách sự kiện (có thể có tìm kiếm)
        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 10; // Adjust based on your needs

            // Fetch all events and filter by search query (Title or Location)
            var events = await _eventRepository.GetAllAsync(); // Assuming GetAllEventsAsync fetches all events

            // Apply filtering if searchQuery is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                events = events.Where(e => e.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                            e.Location.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Pagination logic
            var totalEvents = events.Count();
            var totalPages = (int)Math.Ceiling(totalEvents / (double)pageSize);
            var pagedEvents = events.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            page = Math.Max(1, Math.Min(page, totalPages));

            // Set up pagination in ViewData
            ViewData["SearchQuery"] = searchQuery;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PreviousPage"] = page > 1 ? page - 1 : 1;
            ViewData["NextPage"] = page < totalPages ? page + 1 : totalPages;

            return View(pagedEvents);
        }

        // Hiển thị trang tạo sự kiện mới
        public async Task<IActionResult> Create()
        {
            await SetUpViewBags();
            return View(new Event());
        }

        // Xử lý yêu cầu tạo sự kiện mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event evnt)
        {
            // Kiểm tra dữ liệu đầu vào
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid data provided. Please correct the errors and try again.";
                await SetUpViewBags(); // Đảm bảo dữ liệu ViewBag được thiết lập
                return View(evnt);
            }

            // Thiết lập thời gian tạo và cập nhật
            evnt.CreatedAt = DateTime.UtcNow;
            evnt.UpdatedAt = DateTime.UtcNow;

            try
            {
                // Lưu sự kiện vào cơ sở dữ liệu
                await _eventRepository.CreateAsync(evnt);

                TempData["SuccessMessage"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                Console.WriteLine($"Error creating event: {ex.Message}");

                TempData["ErrorMessage"] = "An unexpected error occurred while creating the event. Please try again later.";
                await SetUpViewBags(); // Đảm bảo dữ liệu ViewBag được thiết lập lại
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

            await SetUpViewBags();
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

        // Phương thức này thiết lập các ViewBag cho các tổ chức và danh mục
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
