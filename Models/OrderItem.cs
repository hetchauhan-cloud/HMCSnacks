using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HMCSnacks.Models
{
    public class OrderItem
    {
        /// <summary>
        /// Primary key for the order item
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Foreign key reference to the Order
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Foreign key reference to the Product
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Name of the product at the time of ordering
        /// </summary>
        [Required]
        public string ProductName { get; set; }

        /// <summary>
        /// Price per unit at the time of order
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Quantity of the product ordered
        /// </summary>
        [Required]
        public int Quantity { get; set; }

        /// <summary>
        /// Navigation property to Product
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Optional navigation to Order (if needed)
        /// </summary>
        public Order Order { get; set; }
    }
}
