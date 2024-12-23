using AdminDashboard.Models;
using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class NotificationController : Controller
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly UserRepository _userRepository;

        public NotificationController(
            NotificationRepository notificationRepository,
            UserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        // GET: Create Notification
        public async Task<IActionResult> Create()
        {
            var users = await _userRepository.GetAllAsync();
            if (users == null)
            {
                users = new List<User>();
            }
            ViewBag.Users = users;
            return View();
        }

        // POST: Create Notification
        [HttpPost]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (ModelState.IsValid)
            {
                // Nếu không có người dùng cụ thể, gửi thông báo tới tất cả người dùng
                if (notification.Emails == null || !notification.Emails.Any())
                {
                    var usersList = await _userRepository.GetEmailsAsync();
                    notification.Emails = usersList;
                }

                // Lưu thông báo vào cơ sở dữ liệu (MongoDB)
                await _notificationRepository.AddNotificationAsync(notification);

                // Redirect to the Index page after saving the notification
                return RedirectToAction("Index");
            }

            // Nếu dữ liệu không hợp lệ, lấy lại danh sách người dùng và gửi lại cho view
            ViewBag.Users = await _userRepository.GetAllAsync();
            return View(notification);
        }

        // GET: Edit Notification
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            ViewBag.Users = await _userRepository.GetAllAsync();
            return View(notification);
        }

        // POST: Edit Notification
        [HttpPost]
        public async Task<IActionResult> Edit(string id, Notification notification)
        {
            if (id != notification.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                // Nếu không có người dùng cụ thể, gửi thông báo tới tất cả người dùng
                if (notification.Emails == null || !notification.Emails.Any())
                {
                    var usersList = await _userRepository.GetEmailsAsync();
                    notification.Emails = usersList;
                }

                var isUpdated = await _notificationRepository.UpdateNotificationAsync(id, notification);
                if (isUpdated)
                {
                    // Redirect to the Index page after updating the notification
                    return RedirectToAction("Index");
                }

                return NotFound();
            }

            ViewBag.Users = await _userRepository.GetAllAsync();
            return View(notification);
        }

        // GET: Delete Notification
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

        // POST: Delete Notification
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                TempData["ErrorMessage"] = "Notification not found.";
                return RedirectToAction("Index");
            }

            try
            {
                await _notificationRepository.DeleteNotificationAsync(id);
                TempData["SuccessMessage"] = "Notification deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred while deleting the notification: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // GET: List of Notifications
        public IActionResult Index(int page = 1, string searchQuery = "")
        {
            // Fetch notifications based on the page and search query
            var notifications = _notificationRepository.GetNotificationsByPage(page, searchQuery, out int totalNotifications);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling(totalNotifications / 10.0); // Assuming 10 items per page

            // Ensure the page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages));

            // Pass data to the View
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchQuery"] = searchQuery;
            ViewData["PreviousPage"] = page > 1 ? page - 1 : 1;  // Prevent going to page 0
            ViewData["NextPage"] = page < totalPages ? page + 1 : totalPages; // Prevent exceeding totalPages

            return View(notifications);
        }
    }
}
