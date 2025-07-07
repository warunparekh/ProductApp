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

            // Build the ViewModel by joining cart data with live product data
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
            var existingItem = await _cartRepo.GetCartItemAsync(productId, userId);

            if (existingItem != null)
            {
                existingItem.ProductQuantity += quantity;
                await _cartRepo.UpdateAsync(existingItem);
            }
            else
            {
                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductQuantity = quantity
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _cartRepo.ClearCartAsync(userId);
            return RedirectToAction("Index");
        }
    }
}