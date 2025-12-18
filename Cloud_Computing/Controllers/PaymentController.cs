using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class PaymentController : Controller
    {
        private readonly DB _context;

        public PaymentController(DB context)
        {
            _context = context;
        }

        // READ (List)
        public async Task<IActionResult> Index()
        {
            return View(await _context.Payments.ToListAsync());
        }

        // READ (Single)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.Payments.FindAsync(id);
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
        public async Task<IActionResult> Create(Payment Payments)
        {
            if (!ModelState.IsValid) return View(Payments);

            _context.Add(Payments);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // UPDATE (Form)
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.Payments.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // UPDATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment Payments)
        {
            if (id != Payments.Id) return NotFound();
            if (!ModelState.IsValid) return View(Payments);

            _context.Update(Payments);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (Confirm)
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.Payments.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Payments.FindAsync(id);
            if (model != null)
            {
                _context.Payments.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
