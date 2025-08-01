using EmployeeOrderingSystem.Data;
using EmployeeOrderingSystem.Interfaces;
using EmployeeOrderingSystem.Models;
using EmployeeOrderingSystem.Services;
using EmployeeOrderingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeOrderingSystem.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBonusService _bonusService;

        private readonly UserManager<IdentityUser> _userManager;
        public EmployeesController(ApplicationDbContext context, UserManager<IdentityUser> userManager,IBonusService bonusService)
        {
            _context = context;
            _userManager = userManager;
            _bonusService = bonusService;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Employees.ToListAsync());
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Balance()
        {
            var userId = _userManager.GetUserId(User);
            var Bal = await _context.Employees
                .Where(e => e.UserId == userId).ToListAsync();
            return View(Bal);
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,EmployeeNumber,Balance,LastDepositMonth")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                employee.UserId = _userManager.GetUserId(User); // inject UserManager<IdentityUser> in constructor
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }


        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,EmployeeNumber,Balance,LastDepositMonth")] Employee employee)
        {
            if (id != employee.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }

        // --- Deposit Logic (BONUS/DEPOSIT) ---
        // GET: Employees/Deposit/5
        public async Task<IActionResult> Deposit(int? id)
        {
            if (id == null) return NotFound();
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            var vm = new EmployeeDepositViewModel
            {
                EmployeeId = employee.Id
            };
            return View(vm);
        }


        // POST: Employees/Deposit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(EmployeeDepositViewModel vm)
        {
            var employee = await _context.Employees.FindAsync(vm.EmployeeId);
            if (employee == null)
            {
                TempData["Error"] = "Employee not found.";
                return RedirectToAction("Index");
            }
            if (vm.DepositAmount <= 0)
            {
                TempData["Error"] = "Deposit must be greater than zero.";
                return RedirectToAction("Deposit", new { id = vm.EmployeeId });
            }

            var bonus = _bonusService.CalculateBonus(vm.DepositAmount);
            employee.Balance += vm.DepositAmount + bonus;
            employee.LastDepositMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            await _context.SaveChangesAsync();
            TempData["Message"] = $"Deposit successful! Bonus applied: R{bonus:F2}";
            return RedirectToAction("Details", new { id = vm.EmployeeId });
        }


    }
}
