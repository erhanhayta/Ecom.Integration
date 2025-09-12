using System.Text.Json.Serialization;

namespace Ecom.Domain.Entities
{
    public class ProductVariant
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }

        [JsonIgnore] // <-- Döngüyü kırar
        public Product Product { get; set; } = default!;

        public string Color { get; set; } = default!;
        public string Size { get; set; } = default!;
        public string Sku { get; set; } = default!;
        public string? Barcode { get; set; }

        public decimal Price { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
