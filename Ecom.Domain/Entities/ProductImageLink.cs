namespace Ecom.Domain.Entities
{
    public class ProductImageLink
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }          // güvenlik/filtre için
        public Guid ProductImageId { get; set; }     // fiziksel dosya kaydı
        public Guid? ProductVariantId { get; set; }  // null => tüm ürüne bağlı

        public int SortOrder { get; set; }
        public bool IsMain { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
