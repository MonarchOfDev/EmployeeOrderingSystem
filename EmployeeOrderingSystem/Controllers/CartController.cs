using EmployeeOrderingSystem.Data;
using EmployeeOrderingSystem.Models;
using EmployeeOrderingSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;

namespace EmployeeOrderingSystem.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentEmployeeId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employee = _context.Employees.FirstOrDefault(e => e.UserId == userId);
            return employee?.Id;
        }

        private CartViewModel GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
                return new CartViewModel();
            return JsonConvert.DeserializeObject<CartViewModel>(cartJson);
        }

        private void SaveCartToSession(CartViewModel cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }

        private void ClearCartSession()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        public async Task<IActionResult> BrowseMenu(int restaurantId)
        {
            var restaurant = await _context.Restaurants
                .Include(r => r.MenuItems)
                .FirstOrDefaultAsync(r => r.Id == restaurantId);
            if (restaurant == null)
                return NotFound();

            ViewBag.RestaurantId = restaurantId;
            ViewBag.RestaurantName = restaurant.Name;
            return View(restaurant.MenuItems.ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int menuItemId, int quantity, int restaurantId)
        {
            if (quantity < 1)
            {
                TempData["Error"] = "Quantity must be at least 1.";
                return RedirectToAction("BrowseMenu", new { restaurantId });
            }

            var menuItem = _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefault(m => m.Id == menuItemId);

            if (menuItem == null)
            {
                TempData["Error"] = "Menu item not found.";
                return RedirectToAction("BrowseMenu", new { restaurantId });
            }

            var cart = GetCartFromSession();

            if (cart.Items.Any() && cart.RestaurantId != restaurantId)
            {
                cart = new CartViewModel { RestaurantId = restaurantId, RestaurantName = menuItem.Restaurant.Name };
            }

            cart.RestaurantId = restaurantId;
            cart.RestaurantName = menuItem.Restaurant.Name;

            var existing = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (existing == null)
            {
                cart.Items.Add(new CartItemViewModel
                {
                    MenuItemId = menuItem.Id,
                    MenuItemName = menuItem.Name,
                    UnitPrice = menuItem.Price,
                    Quantity = quantity
                });
            }
            else
            {
                existing.Quantity += quantity;
            }

            SaveCartToSession(cart);
            TempData["Message"] = "Item added to cart!";
            return RedirectToAction("BrowseMenu", new { restaurantId });
        }

        public IActionResult ViewCart()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int menuItemId)
        {
            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item != null)
                cart.Items.Remove(item);

            SaveCartToSession(cart);
            TempData["Message"] = "Item removed from cart.";
            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int menuItemId, int quantity)
        {
            if (quantity < 1)
            {
                TempData["Error"] = "Quantity must be at least 1.";
                return RedirectToAction("ViewCart");
            }

            var cart = GetCartFromSession();
            var item = cart.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
            if (item != null)
                item.Quantity = quantity;

            SaveCartToSession(cart);
            TempData["Message"] = "Quantity updated.";
            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCartFromSession();

            if (!cart.Items.Any())
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("ViewCart");
            }

            var employeeId = GetCurrentEmployeeId();
            if (employeeId == null)
            {
                TempData["Error"] = "Employee account not found for current user.";
                return RedirectToAction("ViewCart");
            }

            var employee = await _context.Employees.FindAsync(employeeId.Value);
            if (employee == null)
            {
                TempData["Error"] = "Employee account not found.";
                return RedirectToAction("ViewCart");
            }

            var orderTotal = cart.CartTotal;

            if (employee.Balance < orderTotal)
            {
                TempData["Error"] = "Insufficient balance. Please deposit funds.";
                return RedirectToAction("ViewCart");
            }

            var order = new Order
            {
                EmployeeId = employee.Id,
                OrderDate = DateTime.Now,
                TotalAmount = orderTotal,
                Status = "Pending",
                StatusLastUpdated = DateTime.Now,
                OrderItems = cart.Items.Select(cartItem => new OrderItem
                {
                    MenuItemId = cartItem.MenuItemId,
                    Quantity = cartItem.Quantity,
                    UnitPriceAtTimeOfOrder = cartItem.UnitPrice
                }).ToList()
            };

            employee.Balance -= orderTotal;

            _context.Orders.Add(order);
            _context.Update(employee);
            await _context.SaveChangesAsync();

            ClearCartSession();
            TempData["Message"] = "Order placed successfully!";
            return RedirectToAction("OrderConfirmation", "Orders", new { orderId = order.OrderId });
        }
    }
}
