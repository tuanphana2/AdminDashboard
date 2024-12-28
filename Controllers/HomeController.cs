using AdminDashboard.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;

namespace AdminDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly CategoryEventRepository _categoryEventRepository;
        private readonly EventRepository _eventRepository;

        public HomeController(
            UserRepository userRepository,
            CategoryEventRepository categoryEventRepository,
            EventRepository eventRepository)
        {
            _userRepository = userRepository;
            _categoryEventRepository = categoryEventRepository;
            _eventRepository = eventRepository;
        }

        public async Task<IActionResult> Index()
        {
            var adminEmail = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(adminEmail))
            {
                return RedirectToAction("Login", "Login");
            }
            var userCount = (await _userRepository.GetAllAsync()).Count();
            var categoryCount = (await _categoryEventRepository.GetAllAsync()).Count();
            var attendeesCount = await _userRepository.GetCountByRoleAsync("attendee");
            var organizersCount = await _userRepository.GetCountByRoleAsync("organizer");

            var recentUserCount = await _userRepository.GetUserCountByRecentAsync();

            var eventCountThisMonth = await _eventRepository.GetEventCountThisMonthAsync();

            var eventCountsByCategory = await _categoryEventRepository.GetEventCountByCategoryAsync(_eventRepository.GetCollection());

            ViewData["UserCount"] = userCount;
            ViewData["RecentUserCount"] = recentUserCount;
            ViewData["EventCountThisMonth"] = eventCountThisMonth;
            ViewData["CategoryCount"] = categoryCount;
            ViewBag.AttendeesCount = attendeesCount;
            ViewBag.OrganizersCount = organizersCount;
            ViewBag.CategoryNames = eventCountsByCategory.Keys.ToList();
            ViewBag.EventCounts = eventCountsByCategory.Values.ToList();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
