using System.Diagnostics;
using Hoho_GE_Partnership.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoho_GE_Partnership.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Geo()
        {
            return View();
        }

        public IActionResult Eng()
        {
            return View();
        }

        public IActionResult Ru()
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