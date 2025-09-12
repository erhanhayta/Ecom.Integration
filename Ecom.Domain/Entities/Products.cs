namespace Ecom.Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string ProductCode { get; set; } = default!;
        public string Barcode { get; set; } = default!;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public decimal BasePrice { get; set; }
        public decimal TaxRate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}

