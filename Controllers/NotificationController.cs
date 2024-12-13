using AdminDashboard.Models;
using AdminDashboard.Repositories;
using AdminDashboard.Hubs;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(
            NotificationRepository notificationRepository,
            UserRepository userRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _hubContext = hubContext;
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
                if (notification.Users == null || !notification.Users.Any())
                {
                    var usersList = await _userRepository.GetAllAsync();
                    notification.Users = usersList;
                }

                // Lưu thông báo vào cơ sở dữ liệu
                await _notificationRepository.AddNotificationAsync(notification);

                // Gửi thông báo qua SignalR đến tất cả người dùng
                foreach (var user in notification.Users)
                {
                    await _hubContext.Clients.User(user.Id).SendAsync("ReceiveNotification", notification.Title, notification.Description);
                }

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
                if (notification.Users == null || !notification.Users.Any())
                {
                    var usersList = await _userRepository.GetAllAsync();
                    notification.Users = usersList;
                }

                var isUpdated = await _notificationRepository.UpdateNotificationAsync(id, notification);
                if (isUpdated)
                {
                    // Gửi thông báo đã cập nhật tới tất cả người dùng
                    foreach (var user in notification.Users)
                    {
                        await _hubContext.Clients.User(user.Id).SendAsync("ReceiveNotification", notification.Title, notification.Description);
                    }

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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var notification = await _notificationRepository.GetNotificationByIdAsync(id);
            if (notification == null)
            {
                return NotFound();
            }

            await _notificationRepository.DeleteNotificationAsync(id);

            // Gửi thông báo xóa tới tất cả người dùng
            foreach (var user in notification.Users)
            {
                await _hubContext.Clients.User(user.Id).SendAsync("ReceiveNotification", "Deleted Notification", "A notification has been deleted.");
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
