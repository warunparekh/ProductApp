using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductApp.Repositories;

namespace ProductApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductRepository _productRepo;
        public ProductController(ProductRepository productRepo) => _productRepo = productRepo;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _productRepo.GetAllAsync();
            return View(list);
        }

        [HttpGet, Authorize]        
        public async Task<IActionResult> Details(int id)
        {
            var item = await _productRepo.GetByIdWithCategoryNameAsync(id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}