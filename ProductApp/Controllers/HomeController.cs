using Microsoft.AspNetCore.Mvc;

namespace ProductApp.Controllers
{
    public class HomeController : Controller
    {
        // root  product list
        public IActionResult Index() => RedirectToAction("Index", "Product");
    }
}
