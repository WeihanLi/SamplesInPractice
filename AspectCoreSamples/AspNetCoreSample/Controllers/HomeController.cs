using AspNetCoreSample.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AspNetCoreSample.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomService _customService;

        public HomeController(ICustomService customService) => _customService = customService;

        public IActionResult Index()
        {
            _customService.Call();
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}