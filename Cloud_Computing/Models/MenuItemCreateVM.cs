using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class MenuItemViewModel
    {
        public int ItemId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string Description { get; set; } = string.Empty;

        public IFormFile ImageFile { get; set; }
        public string Category { get; set; } = string.Empty;
    }

}
