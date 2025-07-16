using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMCSnacks.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required]
        public string ProductName { get; set; }
        [Required]
        public decimal Price { get; set; }
        
        public string Description { get; set; }

        public bool IsActive { get; set; }

        public string Category { get; set; }
        public int StockQuantity { get; set; }
        public string WeightOrSize { get; set; }
        public string PackType { get; set; }
        [NotMapped]
        public IFormFile? ProductImage { get; set; }

        public string? ImagePath { get; set; }
        public bool IsBestSeller { get; set; }
    }
}
