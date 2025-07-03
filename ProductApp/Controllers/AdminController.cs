using Microsoft.AspNetCore.Mvc;
using ProductApp.Attributes;
using ProductApp.Models;
using ProductApp.Repositories;

namespace ProductApp.Controllers
{
    [Authorize(AdminRequired = true)]
    public class AdminController : Controller
    {
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly UserRepository _userRepository;

        public AdminController()
        {
            _productRepository = new ProductRepository();
            _categoryRepository = new CategoryRepository();
            _userRepository = new UserRepository();
        }

        public IActionResult Index()
        {
            try
            {
                ViewBag.ProductCount = _productRepository.GetAllProducts().Count();
                ViewBag.CategoryCount = _categoryRepository.GetAllCategories().Count();
                ViewBag.UserCount = _userRepository.GetAllUsers().Count();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading dashboard: " + ex.Message;
                ViewBag.ProductCount = 0;
                ViewBag.CategoryCount = 0;
                ViewBag.UserCount = 0;
                return View();
            }
        }

        // Product Management
        public IActionResult Products()
        {
            try
            {
                var products = _productRepository.GetAllProductsWithCategories();
                return View(products);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading products: " + ex.Message;
                return View(new List<Product>());
            }
        }

        public IActionResult CreateProduct()
        {
            try
            {
                ViewBag.Categories = _categoryRepository.GetAllCategories();
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading categories: " + ex.Message;
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = _productRepository.AddProduct(product);
                    if (result > 0)
                    {
                        TempData["Success"] = "Product created successfully!";
                        return RedirectToAction("Products");
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create product.";
                    }
                }
                else
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                    TempData["Error"] = "Validation errors: " + string.Join("; ", errors);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating product: " + ex.Message;
                Console.WriteLine($"Create Product Error: {ex}");
            }

            ViewBag.Categories = _categoryRepository.GetAllCategories();
            return View(product);
        }

        public IActionResult EditProduct(int id)
        {
            try
            {
                var product = _productRepository.GetProductById(id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Products");
                }
                ViewBag.Categories = _categoryRepository.GetAllCategories();
                return View(product);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading product: " + ex.Message;
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = _productRepository.UpdateProduct(product);
                    if (result)
                    {
                        TempData["Success"] = "Product updated successfully!";
                        return RedirectToAction("Products");
                    }
                    else
                    {
                        TempData["Error"] = "Failed to update product.";
                    }
                }
                else
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                    TempData["Error"] = "Validation errors: " + string.Join("; ", errors);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating product: " + ex.Message;
                Console.WriteLine($"Update Product Error: {ex}");
            }

            ViewBag.Categories = _categoryRepository.GetAllCategories();
            return View(product);
        }

        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                var result = _productRepository.DeleteProduct(id);
                if (result)
                {
                    return Json(new { success = true, message = "Product deleted successfully" });
                }
                return Json(new { success = false, message = "Failed to delete product" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete Product Error: {ex}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Category Management
        public IActionResult Categories()
        {
            try
            {
                var categories = _categoryRepository.GetAllCategories().ToList();


                foreach (var category in categories)
                {
                    category.ProductCount = _categoryRepository.GetProductCountByCategory(category.CategoryId);
                }

                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading categories: " + ex.Message;
                return View(new List<Category>());
            }
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = _categoryRepository.AddCategory(category);
                    if (result > 0)
                    {
                        TempData["Success"] = "Category created successfully!";
                        return RedirectToAction("Categories");
                    }
                    else
                    {
                        TempData["Error"] = "Failed to create category.";
                    }
                }
                else
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage))}");
                    TempData["Error"] = "Validation errors: " + string.Join("; ", errors);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error creating category: " + ex.Message;
                Console.WriteLine($"Create Category Error: {ex}");
            }

            return View(category);
        }

        [HttpPost]
        public JsonResult DeleteCategory(int id)
        {
            try
            {
                var result = _categoryRepository.DeleteCategory(id);
                if (result)
                {
                    return Json(new { success = true, message = "Category deleted successfully" });
                }
                return Json(new { success = false, message = "Failed to delete category" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete Category Error: {ex}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // User Management
        public IActionResult Users()
        {
            try
            {
                var users = _userRepository.GetAllUsers();
                return View(users);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error loading users: " + ex.Message;
                return View(new List<User>());
            }
        }

        [HttpPost]
        public JsonResult ToggleAdminStatus(int id)
        {
            try
            {
                var user = _userRepository.GetUserById(id);
                if (user != null)
                {
                    user.isAdmin = !user.isAdmin;
                    if (_userRepository.UpdateUser(user))
                    {
                        return Json(new { success = true, isAdmin = user.isAdmin });
                    }
                }
                return Json(new { success = false, message = "User not found" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Toggle Admin Status Error: {ex}");
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeleteUser(int id)
        {
            try
            {
                var result = _userRepository.DeleteUser(id);
                if (result)
                {
                    return Json(new { success = true, message = "User deleted successfully" });
                }
                return Json(new { success = false, message = "Failed to delete user" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete User Error: {ex}");
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        
    }
}