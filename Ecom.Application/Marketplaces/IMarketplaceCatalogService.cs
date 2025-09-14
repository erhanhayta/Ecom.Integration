// Ecom.Application/Marketplaces/MarketplaceCatalogServiceFactory.cs
using Ecom.Contracts.Marketplaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Marketplaces;

namespace Ecom.Application.Marketplaces
{
    public interface IMarketplaceCatalogService
    {
        Firm Firm { get; }
        Task<IReadOnlyList<BrandDto>> GetBrandsAsync(MarketplaceShop shop, string? query, CancellationToken ct = default);
        Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(MarketplaceShop shop, int? parentId, string? query, CancellationToken ct = default);
        Task<IReadOnlyList<CategoryAttributeDto>> GetCategoryAttributesAsync(MarketplaceShop shop, int categoryId, CancellationToken ct = default);
    }

    public interface IMarketplaceCatalogServiceFactory
    {
        IMarketplaceCatalogService Get(Firm firm);
    }
}
