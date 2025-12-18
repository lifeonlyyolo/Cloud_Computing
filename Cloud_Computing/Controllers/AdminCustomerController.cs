using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class AdminCustomerController : Controller
    {
        private readonly DB _context;

        public AdminCustomerController(DB context)
        {
            _context = context;
        }

        // READ (List)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
        }

        // READ (Single)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.Customers.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // CREATE (Form)
        public IActionResult Create()
        {
            return View();
        }

        // CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            _context.Add(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (Form)
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Customers.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // UPDATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            if (id != customer.Id) return NotFound();
            if (!ModelState.IsValid) return View(customer);

            _context.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (Confirm)
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.Customers.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Customers.FindAsync(id);
            if (model != null)
            {
                _context.Customers.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
