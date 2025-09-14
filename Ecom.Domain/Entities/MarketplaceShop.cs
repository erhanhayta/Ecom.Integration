using Ecom.Domain.Marketplaces;

namespace Ecom.Domain.Entities
{
    public class MarketplaceShop
    {
        public Guid Id { get; set; }
        public Firm Firm { get; set; }   // <-- Burada Domain.Marketplaces.Firm kullan
        public string ShopName { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string ApiSecret { get; set; } = "";
        public string Token { get; set; } = "";
        public string AccountId { get; set; } = "";
        public string IntegrationRefCode { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
