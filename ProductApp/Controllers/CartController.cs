using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Models;
using ProductApp.Models.ViewModels;
using ProductApp.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartRepository _cartRepo;
        private readonly ProductRepository _productRepo;

        public CartController(CartRepository cartRepo, ProductRepository productRepo)
        {
            _cartRepo = cartRepo;
            _productRepo = productRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = (await _cartRepo.GetByUserIdAsync(userId)).ToList();
            if (!cartItems.Any())
            {
                return View(new List<CartItemViewModel>());
            }

            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var products = (await _productRepo.GetByIdsAsync(productIds)).ToList();
            bool cartUpdated = false;

            // **PRICE SYNCHRONIZATION LOGIC**
            // Loop through each item and verify its price against the product catalog.
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product != null && item.ProductNetprice != product.ProductPrice)
                {
                    // Price has changed, update the cart item.
                    item.ProductNetprice = product.ProductPrice;
                    item.CartTotalPrice = item.ProductQuantity * product.ProductPrice;
                    await _cartRepo.UpdateAsync(item);
                    cartUpdated = true;
                }
            }

            if (cartUpdated)
            {
                TempData["Info"] = "Some prices in your cart have been updated to reflect the latest values.";
            }

            var viewModel = cartItems.Select(c => new CartItemViewModel
            {
                Cart = c,
                Product = products.FirstOrDefault(p => p.ProductId == c.ProductId)
            }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = (await _productRepo.GetByIdsAsync(new[] { productId })).FirstOrDefault();
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var existingItem = await _cartRepo.GetCartItemAsync(productId, userId);

            if (existingItem != null)
            {
                existingItem.ProductQuantity += quantity;
                existingItem.ProductNetprice = product.ProductPrice; // Always use latest price
                existingItem.CartTotalPrice = existingItem.ProductQuantity * existingItem.ProductNetprice;
                await _cartRepo.UpdateAsync(existingItem);
            }
            else
            {
                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductQuantity = quantity,
                    ProductNetprice = product.ProductPrice,
                    ProductName = product.ProductName,
                    CartTotalPrice = quantity * product.ProductPrice
                };
                await _cartRepo.AddAsync(cartItem);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCart(List<UpdateCartItem> items)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                foreach (var item in items)
                {
                    var cartItem = await _cartRepo.GetCartItemAsync(item.ProductId, userId);
                    if (cartItem != null)
                    {
                        if (item.Quantity > 0)
                        {
                            cartItem.ProductQuantity = item.Quantity;
                            cartItem.CartTotalPrice = item.Quantity * cartItem.ProductNetprice;
                            await _cartRepo.UpdateAsync(cartItem);
                        }
                        else
                        {
                            await _cartRepo.DeleteAsync(cartItem);
                        }
                    }
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItem = await _cartRepo.GetCartItemAsync(productId, userId);
            if (cartItem != null)
            {
                await _cartRepo.DeleteAsync(cartItem);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _cartRepo.ClearCartAsync(userId);
            return RedirectToAction("Index");
        }
    }
}