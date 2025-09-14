namespace Ecom.Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }

        // ZORUNLU: Ürüne bağlı
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // OPSİYONEL: Varyanta bağlı olabilir
        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public string FileName { get; set; } = null!;
        public string Url { get; set; } = null!; // istersen sadece FileName de tutabilirsin
        public int SortOrder { get; set; } = 0;
        public bool IsMain { get; set; } = false;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
