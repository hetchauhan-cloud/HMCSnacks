namespace HMCSnacks.Models
{
    public class Order
    {
        /// <summary>
        /// Primary key - Order ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Full name of the customer placing the order
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Username of the customer (used for login/session)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Customer's email address
        /// </summary>
        public string Email { get; set; }

        public string DeliveryAddress { get; set; }


        /// <summary>
        /// UTC Date and Time the order was placed
        /// </summary>
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// List of items in the order
        /// </summary>
        public List<OrderItem> Items { get; set; } = new();
        public string OrderStatus { get; set; } = "Pending"; 
        public string? AdminComment { get; set; }

        public bool IsCancelledByAdmin { get; set; }
        public bool IsCancelledByCustomer { get; set; }
        public bool IsOrderDeleted { get; set; }

    }
}
