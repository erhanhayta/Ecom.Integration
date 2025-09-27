using Ecom.Application.Marketplaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Marketplaces;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers.admin
{
    [ApiController]
    [Route("admin/ty/{shopId:guid}/products/{productId:guid}")]
    public class TrendyolConfigsController : ControllerBase
    {
        private readonly EcomDbContext _db;
        public TrendyolConfigsController(EcomDbContext db) { _db = db; }

        public sealed class TyAttrDto
        {
            public int AttributeId { get; set; }
            public string? AttributeName { get; set; }
            public bool Required { get; set; }
            public bool Varianter { get; set; }
            public bool AllowCustom { get; set; }
            public bool Slicer { get; set; }
            public int? ValueId { get; set; }
            public string? ValueName { get; set; }
        }

        public sealed class TyConfigDto
        {
            public int CategoryId { get; set; }
            public string? CategoryName { get; set; }

            public string? SelectedPath { get; set; }
            public List<TyAttrDto> Attributes { get; set; } = new();
        }

        // Upsert config (Kategori + Attribute seçimleri)
        [HttpPost("config")]
        public async Task<IActionResult> SaveConfig(Guid shopId, Guid productId, [FromBody] TyConfigDto dto, CancellationToken ct)
        {
            var mp = await _db.MarketplaceProducts
                .FirstOrDefaultAsync(x => x.ProductId == productId
                                       && x.ShopId == shopId
                                       && x.Firm == (int)Ecom.Domain.Marketplaces.Firm.Trendyol, ct);

            if (mp == null)
            {
                mp = new MarketplaceProduct
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ShopId = shopId,
                    Firm = (int)Ecom.Domain.Marketplaces.Firm.Trendyol,
                    CategoryId = dto.CategoryId,
                    CategoryName = dto.CategoryName,
                    CategoryPath = dto.SelectedPath
                };
                _db.MarketplaceProducts.Add(mp);
            }
            else
            {
                mp.CategoryId = dto.CategoryId;
                mp.CategoryName = dto.CategoryName;
                mp.CategoryPath = dto.SelectedPath;
                mp.UpdatedAtUtc = DateTime.UtcNow;

                var old = _db.MarketplaceProductAttributes.Where(a => a.MarketplaceProductId == mp.Id);
                _db.MarketplaceProductAttributes.RemoveRange(old);
            }

            foreach (var a in dto.Attributes)
            {
                _db.MarketplaceProductAttributes.Add(new MarketplaceProductAttribute
                {
                    Id = Guid.NewGuid(),
                    MarketplaceProductId = mp.Id,
                    CategoryId = dto.CategoryId,
                    AttributeId = a.AttributeId,
                    AttributeName = a.AttributeName,
                    ValueId = a.AllowCustom ? null : a.ValueId,
                    ValueName = a.AllowCustom ? a.ValueName : null,
                    Required = a.Required,
                    Varianter = a.Varianter,
                    AllowCustom = a.AllowCustom,
                    Slicer = a.Slicer
                });
            }

            await _db.SaveChangesAsync(ct);
            return Ok(new { marketplaceProductId = mp.Id });
        }

        // Mevcut config’i getir
        [HttpGet("config")]
        public async Task<IActionResult> GetConfig(Guid shopId, Guid productId, CancellationToken ct)
        {
            var mp = await _db.MarketplaceProducts.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ProductId == productId
                                       && x.ShopId == shopId
                                       && x.Firm == (int)Ecom.Domain.Marketplaces.Firm.Trendyol, ct);
            if (mp == null) return NotFound();

            var attrs = await _db.MarketplaceProductAttributes.AsNoTracking()
                .Where(x => x.MarketplaceProductId == mp.Id)
                .ToListAsync(ct);

            return Ok(new
            {
                marketplaceProductId = mp.Id,
                categoryId = mp.CategoryId,
                categoryName = mp.CategoryName,
                categoryPath = mp.CategoryPath,
                attributes = attrs.Select(x => new
                {
                    x.AttributeId,
                    x.AttributeName,
                    x.Required,
                    x.Varianter,
                    x.AllowCustom,
                    x.Slicer,
                    x.ValueId,
                    x.ValueName
                })
            });
        }

        // TrendyolConfigsController.cs  (POST /admin/ty/{shopId}/products/{productId}/map-variants)

        [HttpPost("map-variants")]
        public async Task<IActionResult> MapVariants(Guid shopId, Guid productId, CancellationToken ct)
        {
            var mp = await _db.MarketplaceProducts
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.ShopId == shopId, ct);
            if (mp == null) return BadRequest("TY config not found");

            var shop = await _db.MarketplaceShops.FirstAsync(s => s.Id == shopId, ct);
            var catalog = HttpContext.RequestServices
                .GetRequiredService<IEnumerable<IMarketplaceCatalogService>>()
                .First(s => s.Firm == Firm.Trendyol);

            // TY attribute listesini çek → beden için ValueId eşleyeceğiz
            var tyAttrs = await catalog.GetCategoryAttributesAsync(shop, mp.CategoryId, ct);
            var colorAttr = tyAttrs.FirstOrDefault(a => a.AttributeId == 47
                             || a.Name.Contains("Renk", StringComparison.OrdinalIgnoreCase));
            var sizeAttr = tyAttrs.FirstOrDefault(a => a.AttributeId == 338
                             || a.Name.Contains("Beden", StringComparison.OrdinalIgnoreCase));

            var sizeValueMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            if (sizeAttr != null)
                foreach (var v in sizeAttr.Values)
                    sizeValueMap[v.Name ?? ""] = v.Id;

            var variants = await _db.ProductVariants
                .Where(v => v.ProductId == productId && v.IsActive)
                .ToListAsync(ct);

            _db.MarketplaceVariantAttributes.RemoveRange(
                _db.MarketplaceVariantAttributes.Where(x => x.MarketplaceProductId == mp.Id));

            foreach (var v in variants)
            {
                if (colorAttr != null)
                {
                    // Renk çoğunlukla allowCustom → ValueName yaz, ValueId boş
                    _db.MarketplaceVariantAttributes.Add(new MarketplaceVariantAttribute
                    {
                        Id = Guid.NewGuid(),
                        MarketplaceProductId = mp.Id,
                        ProductVariantId = v.Id,
                        AttributeId = colorAttr.AttributeId,
                        ValueId = null,
                        ValueName = v.Color ?? ""
                    });
                }

                if (sizeAttr != null)
                {
                    sizeValueMap.TryGetValue(v.Size ?? "", out var tyValId);
                    _db.MarketplaceVariantAttributes.Add(new MarketplaceVariantAttribute
                    {
                        Id = Guid.NewGuid(),
                        MarketplaceProductId = mp.Id,
                        ProductVariantId = v.Id,
                        AttributeId = sizeAttr.AttributeId,
                        ValueId = tyValId != 0 ? tyValId : (int?)null, // eşleştiyse Id yaz
                        ValueName = tyValId != 0 ? null : (v.Size ?? "") // eşleşmediyse free text
                    });
                }
            }

            await _db.SaveChangesAsync(ct);
            return Ok(new { mapped = variants.Count });
        }
    }
}
