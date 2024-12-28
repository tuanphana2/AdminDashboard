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

        public IActionResult Index(int page = 1, string searchQuery = "")
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var notifications = _notificationRepository.GetNotificationsByPage(page, searchQuery, out int totalNotifications);

            int totalPages = (int)Math.Ceiling(totalNotifications / 10.0);

            page = Math.Max(1, Math.Min(page, totalPages));

            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["SearchQuery"] = searchQuery;
            ViewData["PreviousPage"] = page > 1 ? page - 1 : 1;
            ViewData["NextPage"] = page < totalPages ? page + 1 : totalPages;

            return View(notifications);
        }

        public async Task<IActionResult> Create()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var users = await _userRepository.GetAllAsync();
            if (users == null)
            {
                users = new List<User>();
            }
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Notification notification)
        {
            if (ModelState.IsValid)
            {
                if (notification.Emails == null || !notification.Emails.Any())
                {
                    var usersList = await _userRepository.GetEmailsAsync();
                    notification.Emails = usersList;
                }

                await _notificationRepository.AddNotificationAsync(notification);
                
                return RedirectToAction("Index");
            }

            ViewBag.Users = await _userRepository.GetAllAsync();
            return View(notification);
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

        [HttpPost]
        public async Task<IActionResult> Edit(string id, Notification notification)
        {
            if (id != notification.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                if (notification.Emails == null || !notification.Emails.Any())
                {
                    var usersList = await _userRepository.GetEmailsAsync();
                    notification.Emails = usersList;
                }

                var isUpdated = await _notificationRepository.UpdateNotificationAsync(id, notification);
                if (isUpdated)
                {
                    return RedirectToAction("Index");
                }

                return NotFound();
            }

            ViewBag.Users = await _userRepository.GetAllAsync();
            return View(notification);
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
                return NotFound();
            }

            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            return View(notification);
        }

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
    }
}
