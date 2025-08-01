using EmployeeOrderingSystem.Data;
using EmployeeOrderingSystem.Helpers;
using EmployeeOrderingSystem.Interfaces;
using EmployeeOrderingSystem.Models;
using EmployeeOrderingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeOrderingSystem.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderNotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public OrdersController(ApplicationDbContext context, IOrderNotificationService notificationService,
        UserManager<IdentityUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;
        _userManager = userManager;
        }

        // GET: Orders (for admin: see all orders)
        public async Task<IActionResult> Index()
        {
            var orders = _context.Orders.Include(o => o.Employee).Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem);
            return View(await orders.ToListAsync());
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var orders = await _context.Orders
                .Include(o => o.Employee)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .ToListAsync();
            return View(orders);
        }

        // GET: Orders/MyOrders/{employeeId}
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employee == null)
                return RedirectToAction("Index", "Home");

            var orders = await _context.Orders
                .Where(o => o.EmployeeId == employee.Id)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Create/{employeeId}
        public IActionResult Create(int employeeId)
        {
            ViewData["EmployeeId"] = employeeId;
            // Show all restaurants for menu browsing
            ViewData["Restaurants"] = new SelectList(_context.Restaurants, "Id", "Name");
            return View();
        }

        // POST: Orders/Create (the actual order placement logic)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int employeeId, int[] menuItemIds, int[] quantities)
        {
            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null)
                return NotFound();

            if (menuItemIds == null || menuItemIds.Length == 0 || quantities == null || quantities.Length == 0)
            {
                ModelState.AddModelError("", "No menu items selected.");
                return View();
            }

            // Build order items and calculate total
            decimal total = 0;
            var orderItems = new System.Collections.Generic.List<OrderItem>();
            for (int i = 0; i < menuItemIds.Length; i++)
            {
                var menuItem = await _context.MenuItems.FindAsync(menuItemIds[i]);
                if (menuItem == null)
                    continue;

                int qty = quantities[i];
                if (qty <= 0)
                    continue;

                total += menuItem.Price * qty;

                orderItems.Add(new OrderItem
                {
                    MenuItemId = menuItem.Id,
                    Quantity = qty,
                    UnitPriceAtTimeOfOrder = menuItem.Price
                });
            }

            // Balance check
            if (employee.Balance < total)
            {
                ModelState.AddModelError("", "Insufficient balance to place this order.");
                // Ideally: repopulate form info here
                return View();
            }

            // Deduct and create order
            employee.Balance -= total;

            var order = new Order
            {
                EmployeeId = employeeId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Pending",
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            _context.Update(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyOrders), new { employeeId = employeeId });
        }
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems).ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null) return NotFound();
            return View(order);
        }


        // GET: Orders/EditStatus/5 (for admin/delivery)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditStatus(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            ViewData["Statuses"] = new SelectList(new[] { "Pending", "Preparing", "Delivering", "Delivered" }, order.Status);
            return View(order);
        }

        // POST: Orders/EditStatus/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int id, string Status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();
            order.Status = Status;
            order.StatusLastUpdated = DateTime.Now;
            _context.Update(order);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Order status updated!";
            return RedirectToAction(nameof(AdminIndex));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            // 1. Get current user and employee
            var user = await _userManager.GetUserAsync(User);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.UserId == user.Id);

            if (employee == null)
            {
                TempData["Error"] = "Employee profile not found.";
                return RedirectToAction("ViewCart", "Cart");
            }

            // 2. Get cart from session
            var cart = HttpContext.Session.GetObjectFromJson<CartViewModel>("Cart");
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("ViewCart", "Cart");
            }

            // 3. Calculate total
            var total = cart.CartTotal;
            if (employee.Balance < total)
            {
                TempData["Error"] = "Insufficient funds to place this order.";
                return RedirectToAction("ViewCart", "Cart");
            }

            // 4. Create Order
            var order = new Order
            {
                EmployeeId = employee.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                StatusLastUpdated = DateTime.Now,
                TotalAmount = total,
                OrderItems = cart.Items.Select(i => new OrderItem
                {
                    MenuItemId = i.MenuItemId,
                    Quantity = i.Quantity,
                    UnitPriceAtTimeOfOrder = i.UnitPrice
                }).ToList()
            };

            _context.Orders.Add(order);

            // 5. Deduct balance
            employee.Balance -= total;

            // 6. Save changes
            await _context.SaveChangesAsync();

            // 7. Notify user (optional)
            await _notificationService.NotifyOrderPlacedAsync(user.Email, order.OrderId);

            // 8. Clear cart
            HttpContext.Session.Remove("Cart");

            TempData["Message"] = "Order placed successfully!";
            return RedirectToAction("MyOrders");
        }
    }
}
