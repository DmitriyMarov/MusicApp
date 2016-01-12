using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace MusicApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Приложение для поиска всего, что нужно для понравившейся песни: клип, текст, перевод и т.д всего по одному запросу.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Страница контактов";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
