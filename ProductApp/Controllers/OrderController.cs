using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Models;
using ProductApp.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProductApp.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly CartRepository _cartRepo;
        private readonly OrderRepository _orderRepo;
        private readonly ProductRepository _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            CartRepository cartRepo,
            OrderRepository orderRepo,
            ProductRepository productRepo,
            UserManager<ApplicationUser> userManager)
        {
            _cartRepo = cartRepo;
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = await _cartRepo.GetByUserIdAsync(userId);

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.FindByIdAsync(userId);
            ViewBag.ShippingAddress = user.UserAddress;
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string shippingAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = (await _cartRepo.GetByUserIdAsync(userId)).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Cannot place an order with an empty cart.";
                return RedirectToAction("Index", "Cart");
            }

            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var productsInCart = (await _productRepo.GetByIdsAsync(productIds)).ToList();

            // 1. Validate stock for all items before proceeding
            foreach (var item in cartItems)
            {
                var product = productsInCart.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product == null)
                {
                    TempData["Error"] = $"A product in your cart could not be found.";
                    return RedirectToAction("Index", "Cart");
                }
                if (item.ProductQuantity > product.ProductStock)
                {
                    TempData["Error"] = $"Not enough stock for '{product.ProductName}'. Requested: {item.ProductQuantity}, Available: {product.ProductStock}.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            // 2. Create the main order record
            
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = shippingAddress,
                TotalAmount = cartItems.Sum(item => item.CartTotalPrice),
                Status = DispatchStatus.Pending // Set the default status
            };
            order.OrderId = await _orderRepo.CreateOrderAsync(order);
           

            // 3. Create order details and update product stock
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetails
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.ProductQuantity,
                    UnitPrice = item.ProductNetprice
                };
                await _orderRepo.CreateOrderDetailAsync(orderDetail);

                // Atomically update the product's stock
                var productToUpdate = productsInCart.First(p => p.ProductId == item.ProductId);
                productToUpdate.ProductStock -= item.ProductQuantity;
                await _productRepo.UpdateAsync(productToUpdate);
            }

            // 4. Clear the user's cart
            await _cartRepo.ClearCartAsync(userId);

            return RedirectToAction("Confirmation", new { id = order.OrderId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyNow(int productId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }

            // Validate stock for the "Buy Now" item
            if (quantity > product.ProductStock)
            {
                TempData["Error"] = $"Not enough stock for '{product.ProductName}'. Requested: {quantity}, Available: {product.ProductStock}.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Clear the user's existing cart to ensure only this item is purchased
            await _cartRepo.ClearCartAsync(userId);

            // Add the single item to the now-empty cart
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

            // Proceed directly to checkout
            return RedirectToAction("Checkout");
        }

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}