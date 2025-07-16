namespace HMCSnacks.Models
{
    public class OrderReportFilterViewModel
    {
        public List<Order> Orders { get; set; } = new();

        // Filter fields
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Status { get; set; }
        public string? CustomerName { get; set; } // Admin only
    }
}
