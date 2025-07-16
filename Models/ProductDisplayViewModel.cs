namespace HMCSnacks.Models.ViewModels
{
    public class ProductDisplayViewModel
    {
        public Product Product { get; set; }

        public string StockStatus => Product.StockQuantity == 0
        ? "Out of Stock"
        : Product.StockQuantity <= 20
            ? $"Only {Product.StockQuantity} left"
            : $"{Product.StockQuantity} in stock";

        public bool IsOutOfStock => Product.StockQuantity == 0;

        public bool IsFewLeft => Product.StockQuantity > 0 && Product.StockQuantity <= 20;

    }
}
