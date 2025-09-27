namespace Ecom.Domain.Entities
{
    public class ProductVariant
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string Color { get; set; } = "";
        public string Size { get; set; } = "";
        public string Sku { get; set; } = "";
        public string? Barcode { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
