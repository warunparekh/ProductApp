using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApp.Data;
using ProductApp.Models;
using ProductApp.ViewModels;

namespace ProductApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _um;
        private readonly ApplicationDbContext _db;

        public AdminController(
            UserManager<ApplicationUser> um,
            ApplicationDbContext db)
        {
            _um = um;
            _db = db;
        }

        // Dashboard
        public IActionResult Index() => View();

        // --- Users ---
        public async Task<IActionResult> Users()
        {
            var users = _um.Users.ToList();
            var vm = new List<UserViewModel>();
            foreach (var u in users)
            {
                var roles = await _um.GetRolesAsync(u);
                vm.Add(new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserAddress = u.UserAddress,
                    Roles = roles
                });
            }
            return View(vm);
        }

        // --- Categories ---
        public async Task<IActionResult> Categories()
            => View(await _db.Categories.ToListAsync());

        [HttpGet] public IActionResult CreateCategory() => View();
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category m)
        {
            if (!ModelState.IsValid) return View(m);
            _db.Categories.Add(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
            => View(await _db.Categories.FindAsync(id));

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category m)
        {
            if (id != m.CategoryId || !ModelState.IsValid) return View(m);
            _db.Categories.Update(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c != null) { _db.Categories.Remove(c); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Categories));
        }

        // --- Products ---
        public async Task<IActionResult> Products()
            => View(await _db.Products.Include(p => p.Category).ToListAsync());

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewData["Categories"] = await _db.Categories.ToListAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product m)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _db.Categories.ToListAsync();
                return View(m);
            }
            _db.Products.Add(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _db.Products.FindAsync(id);
            ViewData["Categories"] = await _db.Categories.ToListAsync();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product m)
        {
            if (id != m.ProductId || !ModelState.IsValid)
            {
                ViewData["Categories"] = await _db.Categories.ToListAsync();
                return View(m);
            }
            _db.Products.Update(m);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null) { _db.Products.Remove(p); await _db.SaveChangesAsync(); }
            return RedirectToAction(nameof(Products));
        }
    }
}
