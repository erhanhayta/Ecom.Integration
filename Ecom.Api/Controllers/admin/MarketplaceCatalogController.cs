using Ecom.Application.Marketplaces;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers.admin
{
    [ApiController]
    [Route("admin/[controller]")]
    [Authorize] // UI zaten JWT ile çağırıyor
    public sealed class MarketplaceCatalogController : ControllerBase
    {
        private readonly EcomDbContext _db;
        private readonly IMarketplaceCatalogServiceFactory _factory;

        public MarketplaceCatalogController(EcomDbContext db, IMarketplaceCatalogServiceFactory factory)
        {
            _db = db;
            _factory = factory;
        }

        /// GET admin/MarketplaceCatalog/{shopId}/brands?query=
        [HttpGet("{shopId:guid}/brands")]
        public async Task<IActionResult> GetBrands(Guid shopId, [FromQuery] string? query, CancellationToken ct)
        {
            var shop = await _db.MarketplaceShops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == shopId, ct);
            if (shop is null) return NotFound("Shop not found");
            var svc = _factory.Get(shop.Firm);
            var list = await svc.GetBrandsAsync(shop, query, ct);
            return Ok(list);
        }

        /// GET admin/MarketplaceCatalog/{shopId}/categories?parentId=&query=
        [HttpGet("{shopId:guid}/categories")]
        public async Task<IActionResult> GetCategories(Guid shopId, [FromQuery] int? parentId, [FromQuery] string? query, CancellationToken ct)
        {
            var shop = await _db.MarketplaceShops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == shopId, ct);
            if (shop is null) return NotFound("Shop not found");
            var svc = _factory.Get(shop.Firm);
            var list = await svc.GetCategoriesAsync(shop, parentId, query, ct);
            return Ok(list);
        }

        /// GET admin/MarketplaceCatalog/{shopId}/categories/{categoryId}/attributes
        [HttpGet("{shopId:guid}/categories/{categoryId:int}/attributes")]
        public async Task<IActionResult> GetCategoryAttributes(Guid shopId, int categoryId, CancellationToken ct)
        {
            var shop = await _db.MarketplaceShops.AsNoTracking().FirstOrDefaultAsync(x => x.Id == shopId, ct);
            if (shop is null) return NotFound("Shop not found");
            var svc = _factory.Get(shop.Firm);
            var list = await svc.GetCategoryAttributesAsync(shop, categoryId, ct);
            return Ok(list);
        }
    }
}
