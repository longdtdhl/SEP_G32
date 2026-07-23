using Microsoft.AspNetCore.Mvc;

namespace OPCBS.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
