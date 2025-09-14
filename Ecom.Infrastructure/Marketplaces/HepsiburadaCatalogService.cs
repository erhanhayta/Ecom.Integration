using Ecom.Application.Marketplaces;
using Ecom.Contracts.Marketplaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Marketplaces;
using System.Net.Http.Headers;

namespace Ecom.Infrastructure.Marketplaces
{
    public sealed class HepsiburadaCatalogService : IMarketplaceCatalogService
    {
        private readonly IHttpClientFactory _http;

        public HepsiburadaCatalogService(IHttpClientFactory http) => _http = http;

        public Firm Firm => Firm.Hepsiburada;

        HttpClient CreateClient(MarketplaceShop shop)
        {
            var client = _http.CreateClient(nameof(HepsiburadaCatalogService));
            client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(shop.BaseUrl)
                ? "https://listing-external.hepsiburada.com/" : shop.BaseUrl.TrimEnd('/') + "/");
            // HB çoğu uçta Basic Auth kullanır (entegrator key/password) veya bearer – mağaza ayarına göre.
            if (!string.IsNullOrEmpty(shop.Token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", shop.Token);
            return client;
        }

        public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(MarketplaceShop shop, string? query, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            return Array.Empty<BrandDto>();
        }

        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(MarketplaceShop shop, int? parentId, string? query, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            return Array.Empty<CategoryDto>();
        }

        public async Task<IReadOnlyList<CategoryAttributeDto>> GetCategoryAttributesAsync(MarketplaceShop shop, int categoryId, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            return Array.Empty<CategoryAttributeDto>();
        }
    }
}
