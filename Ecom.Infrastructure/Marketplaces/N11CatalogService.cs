using Ecom.Application.Marketplaces;
using Ecom.Contracts.Marketplaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Marketplaces;

namespace Ecom.Infrastructure.Marketplaces
{
    public sealed class N11CatalogService : IMarketplaceCatalogService
    {
        private readonly IHttpClientFactory _http;
        public N11CatalogService(IHttpClientFactory http) => _http = http;

        public Firm Firm => Firm.N11;

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
