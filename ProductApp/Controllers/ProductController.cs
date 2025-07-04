using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;

namespace ProductApp.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var list = await _db.Products.Include(p => p.Category).ToListAsync();
            return View(list);
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}