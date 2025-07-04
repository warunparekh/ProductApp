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
        private readonly IWebHostEnvironment _env;

        public AdminController(
            UserManager<ApplicationUser> um,
            ApplicationDbContext db,
            IWebHostEnvironment env)
        {
            _um = um;
            _db = db;
            _env = env;
        }

        // Dashboard
        public IActionResult Index() => View();

        // --- Users ---
        public async Task<IActionResult> Users()
        {
            var users = _um.Users.ToList();
            var vm = new List<UserViewModel>();
            var currentUserId = _um.GetUserId(User);

            foreach (var u in users)
            {
                var roles = await _um.GetRolesAsync(u);
                vm.Add(new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserAddress = u.UserAddress,
                    Roles = roles,
                    IsCurrentUser = u.Id == currentUserId
                });
            }
            return View(vm);
        }

        // Promote user to Admin
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _um.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Don't allow promoting self
            var currentUserId = _um.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot modify your own roles.";
                return RedirectToAction(nameof(Users));
            }

            var isAlreadyAdmin = await _um.IsInRoleAsync(user, "Admin");
            if (!isAlreadyAdmin)
            {
                await _um.AddToRoleAsync(user, "Admin");
                user.IsAdmin = true;
                await _um.UpdateAsync(user);
                TempData["Success"] = $"User {user.Email} has been promoted to Admin.";
            }
            else
            {
                TempData["Info"] = $"User {user.Email} is already an Admin.";
            }

            return RedirectToAction(nameof(Users));
        }

        // Remove user from Admin role
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromAdmin(string userId)
        {
            var user = await _um.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // Don't allow demoting self
            var currentUserId = _um.GetUserId(User);
            if (user.Id == currentUserId)
            {
                TempData["Error"] = "You cannot modify your own roles.";
                return RedirectToAction(nameof(Users));
            }

            var isAdmin = await _um.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                await _um.RemoveFromRoleAsync(user, "Admin");
                user.IsAdmin = false;
                await _um.UpdateAsync(user);
                TempData["Success"] = $"User {user.Email} has been removed from Admin role.";
            }
            else
            {
                TempData["Info"] = $"User {user.Email} is not an Admin.";
            }

            return RedirectToAction(nameof(Users));
        }

        // --- Categories ---
        public async Task<IActionResult> Categories()
        {
            var categories = await _db.Categories
                .Include(c => c.Products)
                .ToListAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category m)
        {
            if (!ModelState.IsValid) return View(m);
            _db.Categories.Add(m);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Category created successfully.";
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
            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found.";
                return RedirectToAction(nameof(Categories));
            }

            // Check if category has products
            if (category.Products != null && category.Products.Any())
            {
                TempData["Error"] = $"Cannot delete category '{category.CategoryName}' because it contains {category.Products.Count} product(s). Please move or delete the products first.";
                return RedirectToAction(nameof(Categories));
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Category '{category.CategoryName}' deleted successfully.";
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
        public async Task<IActionResult> CreateProduct(Product m, IFormFile ProductImage)
        {
            // Remove ProductImage validation errors since it's handled separately
            ModelState.Remove("ProductImage");

            // Handle file upload
            if (ProductImage != null && ProductImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ProductImage.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProductImage.CopyToAsync(stream);
                }
                m.ProductImage = "/images/" + fileName;
            }

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _db.Categories.ToListAsync();
                return View(m);
            }

            _db.Products.Add(m);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return NotFound();
            ViewData["Categories"] = await _db.Categories.ToListAsync();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product m, IFormFile ProductImage)
        {
            if (id != m.ProductId)
            {
                return NotFound();
            }

            // Remove ProductImage validation errors since it's handled separately
            ModelState.Remove("ProductImage");

            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // Handle file upload
            if (ProductImage != null && ProductImage.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(ProductImage.FileName);
                var filePath = Path.Combine(_env.WebRootPath, "images", fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProductImage.CopyToAsync(stream);
                }
                product.ProductImage = "/images/" + fileName;
            }

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _db.Categories.ToListAsync();
                return View(product);
            }

            // Update product properties
            product.ProductName = m.ProductName;
            product.ProductDescription = m.ProductDescription;
            product.ProductPrice = m.ProductPrice;
            product.ProductStock = m.ProductStock;
            product.CategoryId = m.CategoryId;

            _db.Products.Update(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p != null)
            {
                _db.Products.Remove(p);
                await _db.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully.";
            }
            return RedirectToAction(nameof(Products));
        }
    }
}