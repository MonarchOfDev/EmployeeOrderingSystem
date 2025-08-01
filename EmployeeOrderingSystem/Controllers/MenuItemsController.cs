using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EmployeeOrderingSystem.Data;
using EmployeeOrderingSystem.Models;

namespace EmployeeOrderingSystem.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MenuItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MenuItems
        public async Task<IActionResult> Index()
        {
            var menuItems = _context.MenuItems.Include(m => m.Restaurant);
            return View(await menuItems.ToListAsync());
        }

        // GET: MenuItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
                return NotFound();

            return View(menuItem);
        }
        public async Task<IActionResult> Search(string searchTerm)
        {
            var items = await _context.MenuItems
                .Include(m => m.Restaurant)
                .Where(m => m.Name.Contains(searchTerm) || m.Description.Contains(searchTerm))
                .ToListAsync();
            ViewData["SearchTerm"] = searchTerm;
            return View(items);
        }

        // GET: MenuItems/Create
        public IActionResult Create(int? restaurantId)
        {
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", restaurantId);
            return View();
        }

        // POST: MenuItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RestaurantId,Name,Description,Price")] MenuItem menuItem)
        {
            if (ModelState.IsValid)
            {
                _context.Add(menuItem);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Restaurants", new { id = menuItem.RestaurantId });
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", menuItem.RestaurantId);
            return View(menuItem);
        }

        // GET: MenuItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return NotFound();

            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", menuItem.RestaurantId);
            return View(menuItem);
        }

        // POST: MenuItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RestaurantId,Name,Description,Price")] MenuItem menuItem)
        {
            if (id != menuItem.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(menuItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction("Details", "Restaurants", new { id = menuItem.RestaurantId });
            }
            ViewData["RestaurantId"] = new SelectList(_context.Restaurants, "Id", "Name", menuItem.RestaurantId);
            return View(menuItem);
        }

        // GET: MenuItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var menuItem = await _context.MenuItems
                .Include(m => m.Restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
                return NotFound();

            return View(menuItem);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return NotFound();

            int restaurantId = menuItem.RestaurantId;
            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Restaurants", new { id = restaurantId });
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }
    }
}
