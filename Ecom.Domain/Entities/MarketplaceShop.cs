namespace Ecom.Domain.Entities
{
    public enum MarketplaceFirm
    {
        Trendyol = 1,
        Hepsiburada = 2,
        N11 = 3
    }

    public class MarketplaceShop
    {
        public Guid Id { get; set; }

        public MarketplaceFirm Firm { get; set; }       // Trendyol, HB, N11
        public string ShopName { get; set; } = default!;

        // Ortak alanlar
        public string BaseUrl { get; set; } = default!;
        public string ApiKey { get; set; } = default!;
        public string ApiSecret { get; set; } = default!;
        public string? Token { get; set; }              // Trendyol gibi Token isteyenler için
        public string? AccountId { get; set; }          // Satıcı Id (Hepsiburada = SupplierId, Trendyol = Cari Id)

        // Ek entegrasyon bilgileri
        public string? IntegrationRefCode { get; set; } // Entegrasyon Referans Kodu (Trendyol)

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
