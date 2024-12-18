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

        // Constructor
        public HomeController(
            UserRepository userRepository,
            CategoryEventRepository categoryEventRepository,
            EventRepository eventRepository)
        {
            _userRepository = userRepository;
            _categoryEventRepository = categoryEventRepository;
            _eventRepository = eventRepository;
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            // Lấy thông tin tổng quan người dùng, sự kiện
            var userCount = (await _userRepository.GetAllAsync()).Count();
            var categoryCount = (await _categoryEventRepository.GetAllAsync()).Count();
            var attendeesCount = await _userRepository.GetCountByRoleAsync("attendee");
            var organizersCount = await _userRepository.GetCountByRoleAsync("organizer");

            // Lấy số lượng người dùng mới trong 3 tháng qua
            var recentUserCount = await _userRepository.GetUserCountByRecentAsync();

            // Lấy số lượng sự kiện trong tháng
            var eventCountThisMonth = await _eventRepository.GetEventCountThisMonthAsync();

            // Lấy dữ liệu cho biểu đồ cột
            var eventCountsByCategory = await _categoryEventRepository.GetEventCountByCategoryAsync(_eventRepository.GetCollection());

            // Truyền các giá trị vào ViewData
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
    }
}
