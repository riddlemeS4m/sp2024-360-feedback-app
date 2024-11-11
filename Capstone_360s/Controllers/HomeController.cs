using Capstone_360s.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Capstone_360s.Controllers
{
    public class HomeController : Controller
    {
        public const string Name = "Home";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //var userName = User.Identity.Name;
            //var isAuthenticated = User.Identity.IsAuthenticated;
            //_logger.LogInformation("Login attempt: {0}, {1}", userName, isAuthenticated);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
