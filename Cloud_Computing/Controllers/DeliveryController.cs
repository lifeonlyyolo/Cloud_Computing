using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly DB _context;

        public DeliveryController(DB context)
        {
            _context = context;
        }

        // READ (List)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Deliveries.ToListAsync());
        }

        // READ (Single)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.Deliveries.FindAsync(id);
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
        public async Task<IActionResult> Create(Delivery Deliveries)
        {
            if (!ModelState.IsValid) return View(Deliveries);

            _context.Add(Deliveries);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (Form)
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Deliveries.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // UPDATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Delivery Deliveries)
        {
            if (id != Deliveries.Id) return NotFound();
            if (!ModelState.IsValid) return View(Deliveries);

            _context.Update(Deliveries);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (Confirm)
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.Deliveries.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var Deliveries = await _context.Deliveries.FindAsync(id);
            if (Deliveries != null)
            {
                _context.Deliveries.Remove(Deliveries);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
