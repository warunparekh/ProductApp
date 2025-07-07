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
            var cartItems = (await _cartRepo.GetByUserIdAsync(userId)).ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var productIds = cartItems.Select(c => c.ProductId).ToList();
            var products = (await _productRepo.GetByIdsAsync(productIds)).ToList();

            // Pass both products and cart items to the view
            ViewBag.Products = products;
            ViewBag.User = await _userManager.FindByIdAsync(userId);

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

            // Validate stock and create order details with live data
            foreach (var item in cartItems)
            {
                var product = productsInCart.FirstOrDefault(p => p.ProductId == item.ProductId);
                if (product == null)
                {
                    TempData["Error"] = "A product in your cart could not be found.";
                    return RedirectToAction("Index", "Cart");
                }
                if (item.ProductQuantity > product.ProductStock)
                {
                    TempData["Error"] = $"Not enough stock for '{product.ProductName}'. Requested: {item.ProductQuantity}, Available: {product.ProductStock}.";
                    return RedirectToAction("Index", "Cart");
                }
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = shippingAddress,
                TotalAmount = cartItems.Sum(item => {
                    var product = productsInCart.FirstOrDefault(p => p.ProductId == item.ProductId);
                    return item.ProductQuantity * (product?.ProductPrice ?? 0);
                }),
                Status = DispatchStatus.Pending
            };
            order.OrderId = await _orderRepo.CreateOrderAsync(order);

            foreach (var item in cartItems)
            {
                var product = productsInCart.First(p => p.ProductId == item.ProductId);
                var orderDetail = new OrderDetails
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.ProductQuantity,
                    UnitPrice = product.ProductPrice // Use live price
                };
                await _orderRepo.CreateOrderDetailAsync(orderDetail);

                product.ProductStock -= item.ProductQuantity;
                await _productRepo.UpdateAsync(product);
            }

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

            if (quantity > product.ProductStock)
            {
                TempData["Error"] = $"Not enough stock for '{product.ProductName}'. Requested: {quantity}, Available: {product.ProductStock}.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            await _cartRepo.ClearCartAsync(userId);
            var cartItem = new Cart { UserId = userId, ProductId = productId, ProductQuantity = quantity };
            await _cartRepo.AddAsync(cartItem);

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