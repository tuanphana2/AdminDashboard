using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly CategoryEventRepository _categoryEventRepository;
        private readonly EventRepository _eventRepository;

        public HomeController(UserRepository userRepository, CategoryEventRepository categoryEventRepository, EventRepository eventRepository)
        {
            _userRepository = userRepository;
            _categoryEventRepository = categoryEventRepository;
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> Index()
        {
            var userCount = (await _userRepository.GetAllAsync()).Count;
            var categoryCount = (await _categoryEventRepository.GetAllAsync()).Count;
            var attendeesCount = await _userRepository.GetCountByRoleAsync("attendee");
            var organizersCount = await _userRepository.GetCountByRoleAsync("organizer");

            // Lấy dữ liệu cho biểu đồ cột
            var eventCountsByCategory = await _categoryEventRepository.GetEventCountByCategoryAsync(_eventRepository.GetCollection());

            ViewData["UserCount"] = userCount;
            ViewData["CategoryCount"] = categoryCount;
            ViewBag.AttendeesCount = attendeesCount;
            ViewBag.OrganizersCount = organizersCount;
            ViewBag.CategoryNames = eventCountsByCategory.Keys;
            ViewBag.EventCounts = eventCountsByCategory.Values;

            return View();
        }
    }
}
