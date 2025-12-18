using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class OrderController : Controller
    {
        private readonly DB _context;

        public OrderController(DB context)
        {
            _context = context;
        }

        // READ (List)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Orders.ToListAsync());
        }

        // READ (Single)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.Orders.FindAsync(id);
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
        public async Task<IActionResult> Create(Order Orders)
        {
            if (!ModelState.IsValid) return View(Orders);

            _context.Add(Orders);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (Form)
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Orders.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // UPDATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order Orders)
        {
            if (id != Orders.Id) return NotFound();
            if (!ModelState.IsValid) return View(Orders);

            _context.Update(Orders);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (Confirm)
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.Orders.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Orders.FindAsync(id);
            if (model != null)
            {
                _context.Orders.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
