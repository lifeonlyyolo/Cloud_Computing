using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    public class MenuItemController : Controller
    {
        private readonly DB _context;
        private readonly IAmazonS3 _s3;
        private const string BucketName = "cloudassignmentimagestorage";

        public MenuItemController(DB context, IAmazonS3 s3)
        {
            _context = context;
            _s3 = s3;
        }

        // READ (List)
        public async Task<IActionResult> Index()
        {
            return View(await _context.MenuItems.ToListAsync());
        }

        // READ (Single)
        public async Task<IActionResult> Details(int id)
        {
            var model = await _context.MenuItems.FindAsync(id);
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
        public async Task<IActionResult> Create(MenuItemViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            string s3Key = "";

            // Check if a file was actually uploaded
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                // 1. Generate a unique name to prevent overwriting existing files
                s3Key = $"menu-items/{Guid.NewGuid()}_{vm.ImageFile.FileName}";

                try
                {
                    using (var newMemoryStream = new MemoryStream())
                    {
                        await vm.ImageFile.CopyToAsync(newMemoryStream);

                        var uploadRequest = new Amazon.S3.Model.PutObjectRequest
                        {
                            InputStream = newMemoryStream,
                            BucketName = "cloudassignmentimagestorage",
                            Key = s3Key,
                            ContentType = vm.ImageFile.ContentType
                        };

                        await _s3.PutObjectAsync(uploadRequest);
                    }
                }
                catch (Exception ex)
                {
                    // This will show the error message on your webpage
                    ModelState.AddModelError("", "S3 Upload Failed: " + ex.Message);

                    // Check if there is a more specific error inside
                    if (ex.InnerException != null)
                    {
                        ModelState.AddModelError("", "Inner Error: " + ex.InnerException.Message);
                    }

                    return View(vm); // This keeps you on the page so you can see the error
                }
            }

            var item = new MenuItem
            {
                ItemId = vm.ItemId,
                Name = vm.Name,
                Price = vm.Price,
                Description = vm.Description,
                Category = vm.Category,
                // Save the S3 Key (path) so we can find it later
                ImageUrl = s3Key
            };

            _context.MenuItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }






        // UPDATE (Form)
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _context.MenuItems.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // UPDATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem MenuItems)
        {
            if (id != MenuItems.Id) return NotFound();
            if (!ModelState.IsValid) return View(MenuItems);

            _context.Update(MenuItems);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // DELETE (Confirm)
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.MenuItems.FindAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        // DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.MenuItems.FindAsync(id);
            if (model != null)
            {
                _context.MenuItems.Remove(model);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
