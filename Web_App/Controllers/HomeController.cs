using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Web_App.Session;

namespace Web_App.Controllers
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
            if (HttpContext.Session.GetBoolean("LoggedIn"))
            {
                return RedirectToAction("Index", "Patient");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
