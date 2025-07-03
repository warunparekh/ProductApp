using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Attributes;
using ProductApp.Models;
using ProductApp.Repositories;

namespace ProductApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _productRepository = new ProductRepository();
            _categoryRepository = new CategoryRepository();
        }

        public IActionResult Index()
        {
            var products = _productRepository.GetAllProducts();
            ViewBag.Categories = _categoryRepository.GetAllCategories();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public IActionResult Products()
        {
            var products = _productRepository.GetAllProducts();
            ViewBag.Categories = _categoryRepository.GetAllCategories();
            return View(products);
        }

        [Authorize]
        public IActionResult ProductDetails(int id)
        {
            var product = _productRepository.GetProductById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult AccessDenied()
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
