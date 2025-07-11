using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Models;
using ProductApp.Models.ViewModels;
using ProductApp.Repositories;
using ProductApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _um;
        private readonly IWebHostEnvironment _env;
        private readonly ProductRepository _productRepo;
        private readonly CategoryRepository _categoryRepo;
        private readonly CartRepository _cartRepo;
        private readonly OrderRepository _orderRepo;

        public AdminController(
            UserManager<ApplicationUser> um,
            IWebHostEnvironment env,
            ProductRepository productRepo,
            CategoryRepository categoryRepo,
            CartRepository cartRepo,
            OrderRepository orderRepo)
        {
            _um = um;
            _env = env;
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _cartRepo = cartRepo;
            _orderRepo = orderRepo;
        }

        // Dashboard
        public IActionResult Index() => View();

        // User Management
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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string userId)
        {
            var user = await _um.FindByIdAsync(userId);
            if (user == null) return NotFound();

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

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromAdmin(string userId)
        {
            var user = await _um.FindByIdAsync(userId);
            if (user == null) return NotFound();

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
            var categories = await _categoryRepo.GetAllWithProductCountAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult CreateCategory() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category m)
        {
            if (!ModelState.IsValid) return View(m);
            await _categoryRepo.AddAsync(m);
            TempData["Success"] = "Category created successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _categoryRepo.GetByIdAsync(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category m)
        {
            if (id != m.CategoryId || !ModelState.IsValid) return View(m);
            await _categoryRepo.UpdateAsync(m);
            TempData["Success"] = "Category updated successfully.";
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            await _categoryRepo.DeleteAsync(id);
            TempData["Success"] = $"Category deleted successfully.";
            return RedirectToAction(nameof(Categories));
        }

        // --- Products ---
        public async Task<IActionResult> Products()
        {
            var products = await _productRepo.GetAllWithCategoryNameAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            ViewData["Categories"] = await _categoryRepo.GetAllAsync();
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product m, IFormFile ProductImage)
        {
            ModelState.Remove("ProductImage");

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
                ViewData["Categories"] = await _categoryRepo.GetAllAsync();
                return View(m);
            }

            await _productRepo.AddAsync(m);
            TempData["Success"] = "Product created successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var p = await _productRepo.GetByIdAsync(id);
            if (p == null) return NotFound();
            ViewData["Categories"] = await _categoryRepo.GetAllAsync();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product m, IFormFile ProductImage)
        {
            if (id != m.ProductId)
            {
                return NotFound();
            }

            ModelState.Remove("ProductImage");

            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
                return NotFound();

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
            else
            {
                m.ProductImage = product.ProductImage;
            }

            if (!ModelState.IsValid)
            {
                ViewData["Categories"] = await _categoryRepo.GetAllAsync();
                return View(m);
            }

            await _productRepo.UpdateAsync(m);
            TempData["Success"] = "Product updated successfully.";
            return RedirectToAction(nameof(Products));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productRepo.DeleteAsync(id);
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Products));
        }

        // --- Cart Management ---
        [HttpGet]
        public async Task<IActionResult> ActiveCarts()
        {
            var allCarts = await _cartRepo.GetAllAsync();
            var allUsers = _um.Users.ToList();

            var activeCarts = allCarts
                .GroupBy(c => c.UserId)
                .Select(g => {
                    var user = allUsers.FirstOrDefault(u => u.Id == g.Key);
                    return new ActiveCartViewModel
                    {
                        UserId = g.Key,
                        UserEmail = user?.Email ?? "Unknown User",
                        ItemCount = g.Sum(c => c.ProductQuantity),
                        TotalPrice = g.Sum(c => c.CartTotalPrice)
                    };
                })
                .Where(vm => vm.ItemCount > 0)
                .ToList();

            return View(activeCarts);
        }

        [HttpGet]
        public async Task<IActionResult> ViewUserCart(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var user = await _um.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var cartItems = (await _cartRepo.GetByUserIdAsync(userId)).ToList();
            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var products = (await _productRepo.GetByIdsAsync(productIds)).ToList();

            var viewModel = cartItems.Select(c => new CartItemViewModel
            {
                Cart = c,
                Product = products.FirstOrDefault(p => p.ProductId == c.ProductId)
            }).ToList();

            ViewBag.UserEmail = user.Email;
            return View(viewModel);
        }

        // --- Order Management ---
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var orders = await _orderRepo.GetAllOrdersAsync();
            var userIds = orders.Select(o => o.UserId).Distinct();
            var users = _um.Users.Where(u => userIds.Contains(u.Id)).ToList();

            ViewBag.Users = users.ToDictionary(u => u.Id, u => u.Email);
            return View(orders.OrderByDescending(o => o.OrderDate));
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _orderRepo.GetOrderByIdAsync(id);
            if (order == null) return NotFound();

            var user = await _um.FindByIdAsync(order.UserId);
            var details = await _orderRepo.GetDetailsByOrderIdAsync(id);
            var productIds = details.Select(d => d.ProductId);
            var products = await _productRepo.GetByIdsAsync(productIds);

            var viewModel = new OrderViewModel
            {
                Order = order,
                User = user,
                OrderDetails = details.Select(d => new OrderDetailViewModel
                {
                    Detail = d,
                    Product = products.FirstOrDefault(p => p.ProductId == d.ProductId)
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, DispatchStatus status)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // **PREVENTION LOGIC**: If order is already cancelled, do not allow changes.
            if (order.Status == DispatchStatus.Cancelled)
            {
                TempData["Error"] = "This order has been cancelled and its status cannot be changed.";
                return RedirectToAction("OrderDetails", new { id = orderId });
            }

            // If the order is being newly cancelled, restock the items.
            if (status == DispatchStatus.Cancelled)
            {
                var orderDetails = await _orderRepo.GetDetailsByOrderIdAsync(orderId);
                var productsToUpdate = await _productRepo.GetByIdsAsync(orderDetails.Select(d => d.ProductId));

                foreach (var detail in orderDetails)
                {
                    var product = productsToUpdate.FirstOrDefault(p => p.ProductId == detail.ProductId);
                    if (product != null)
                    {
                        product.ProductStock += detail.Quantity;
                        await _productRepo.UpdateAsync(product);
                    }
                }
            }
            
            order.Status = status;
            await _orderRepo.UpdateOrderAsync(order);

            TempData["Success"] = "Order status updated successfully.";
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
    }
}