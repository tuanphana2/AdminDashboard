﻿using AdminDashboard.Models;
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
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // Hiển thị trang tạo người dùng mới
        public IActionResult Create()
        {
            ViewBag.Roles = new List<string> { "organizer", "attendee" };
            return View();
        }

        // Xử lý yêu cầu tạo người dùng mới
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                return View(user);
            }

            await _userRepository.CreateAsync(user);
            TempData["SuccessMessage"] = "User added successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Hiển thị trang chỉnh sửa người dùng
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound();

            ViewBag.Roles = new List<string> { "organizer", "attendee" };
            return View(user);
        }

        // Xử lý yêu cầu cập nhật thông tin người dùng
        [HttpPost]
        public async Task<IActionResult> Edit(string id, User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new List<string> { "organizer", "attendee" };
                return View(user);
            }

            await _userRepository.UpdateAsync(id, user);
            TempData["SuccessMessage"] = "User updated successfully!";
            return RedirectToAction(nameof(Index));
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