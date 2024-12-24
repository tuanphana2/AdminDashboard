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

        // Hiển thị danh sách người dùng
        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 10; // Adjust based on your needs

            // Fetch all users
            var users = await _userRepository.GetAllAsync();

            // Apply filtering if searchQuery is provided
            if (!string.IsNullOrEmpty(searchQuery))
            {
                users = users.Where(u => u.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                          u.Email.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Pagination logic
            var totalUsers = users.Count();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            // Ensure the page is within valid range
            page = Math.Max(1, Math.Min(page, totalPages));

            var pagedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Set up pagination in ViewData
            ViewData["SearchQuery"] = searchQuery;
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["PreviousPage"] = page > 1 ? page - 1 : 1;  // Prevent going to page 0
            ViewData["NextPage"] = page < totalPages ? page + 1 : totalPages; // Prevent exceeding totalPages

            return View(pagedUsers);
        }

        // Hiển thị trang tạo người dùng mới
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "organizer", "attendee" };
            return View();
        }

        // Xử lý yêu cầu tạo người dùng mới
        [HttpPost]
        public async Task<IActionResult> Create(User user, string hobbiesInput)
        {
            // Kiểm tra ModelState
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                TempData["ErrorMessage"] = "Please correct the errors and try again.";
                return View(user);
            }

            // Xử lý trường Hobbies (chuyển từ chuỗi thành danh sách)
            if (!string.IsNullOrEmpty(hobbiesInput))
            {
                try
                {
                    user.Hobbies = hobbiesInput.Split(',')
                                               .Select(h => h.Trim())
                                               .Where(h => !string.IsNullOrEmpty(h)) // Bỏ qua chuỗi trống
                                               .ToList();
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi (nếu cần)
                    TempData["ErrorMessage"] = "An error occurred while processing hobbies.";
                    Console.WriteLine($"Error processing hobbies: {ex.Message}");

                    // Truyền lại dữ liệu cần thiết và trả về View
                    ViewBag.Roles = new List<string> { "organizer", "attendee" };
                    return View(user);
                }
            }
            else
            {
                user.Hobbies = new List<string>(); // Gán danh sách rỗng nếu không có dữ liệu
            }

            try
            {
                // Gọi repository để lưu người dùng
                await _userRepository.CreateAsync(user);

                TempData["SuccessMessage"] = "User added successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi và xử lý
                TempData["ErrorMessage"] = "An error occurred while saving the user.";
                Console.WriteLine($"Error saving user: {ex.Message}");

                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                return View(user);
            }
        }

        // Hiển thị trang chỉnh sửa người dùng
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            // Chuyển danh sách Hobbies thành chuỗi phân tách bằng dấu phẩy
            ViewBag.HobbiesInput = string.Join(", ", user.Hobbies);

            // Cung cấp danh sách Roles cho View
            ViewBag.Roles = new List<string> { "organizer", "attendee" };
            return View(user);
        }

        // Xử lý cập nhật người dùng sau chỉnh sửa
        [HttpPost]
        public async Task<IActionResult> Edit(string id, User user, string hobbiesInput)
        {
            // Kiểm tra ModelState để phát hiện lỗi trong dữ liệu nhập
            if (!ModelState.IsValid)
            {
                // Cung cấp lại dữ liệu cần thiết nếu ModelState không hợp lệ
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                ViewBag.HobbiesInput = hobbiesInput;
                TempData["ErrorMessage"] = "Please correct the errors in the form.";
                return View(user);
            }

            // Kiểm tra và xử lý hobbiesInput
            if (!string.IsNullOrEmpty(hobbiesInput))
            {
                try
                {
                    user.Hobbies = hobbiesInput.Split(',')
                                               .Select(h => h.Trim())
                                               .Where(h => !string.IsNullOrEmpty(h)) // Bỏ qua các mục trống
                                               .ToList();
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu xảy ra ngoại lệ
                    TempData["ErrorMessage"] = "An error occurred while processing hobbies.";
                    Console.WriteLine($"Error processing hobbies: {ex.Message}");

                    // Truyền lại dữ liệu và trả về View
                    ViewBag.Roles = new List<string> { "organizer", "attendee" };
                    ViewBag.HobbiesInput = hobbiesInput;
                    return View(user);
                }
            }
            else
            {
                user.Hobbies = new List<string>(); // Nếu không có hobbiesInput, gán danh sách rỗng
            }

            try
            {
                // Cập nhật vào MongoDB
                await _userRepository.UpdateAsync(id, user);
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi cập nhật vào cơ sở dữ liệu
                TempData["ErrorMessage"] = "An error occurred while updating the user.";
                Console.WriteLine($"Error updating user: {ex.Message}");

                // Truyền lại dữ liệu và trả về View
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                ViewBag.HobbiesInput = hobbiesInput;
                return View(user);
            }
        }

        // Hiển thị trang xác nhận xóa người dùng
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user == null ? NotFound() : View(user);
        }

        // Xử lý yêu cầu xóa người dùng
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
