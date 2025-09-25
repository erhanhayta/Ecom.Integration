using Ecom.Domain.Entities;
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
                };
                _db.MarketplaceProducts.Add(mp);
            }
            else
            {
                mp.CategoryId = dto.CategoryId;
                mp.CategoryName = dto.CategoryName;
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

        // Bizdeki varyantlara göre varianter eşleşmelerini üret
        [HttpPost("map-variants")]
        public async Task<IActionResult> MapVariants(Guid shopId, Guid productId, CancellationToken ct)
        {
            var mp = await _db.MarketplaceProducts
                .FirstOrDefaultAsync(x => x.ProductId == productId
                                       && x.ShopId == shopId
                                       && x.Firm == (int)Ecom.Domain.Marketplaces.Firm.Trendyol, ct);
            if (mp == null) return BadRequest("TY config not found");

            var varAttrs = await _db.MarketplaceProductAttributes.AsNoTracking()
                .Where(x => x.MarketplaceProductId == mp.Id && x.Varianter)
                .ToListAsync(ct);

            // Default: Renk=47, Beden=338 (isimden bulma fallback)
            int? colorAttrId = varAttrs.FirstOrDefault(a => a.AttributeId == 47)?.AttributeId
                            ?? varAttrs.FirstOrDefault(a => (a.AttributeName ?? "").Contains("Renk", StringComparison.OrdinalIgnoreCase))?.AttributeId;

            int? sizeAttrId = varAttrs.FirstOrDefault(a => a.AttributeId == 338)?.AttributeId
                            ?? varAttrs.FirstOrDefault(a => (a.AttributeName ?? "").Contains("Beden", StringComparison.OrdinalIgnoreCase))?.AttributeId;

            var variants = await _db.ProductVariants
                .Where(v => v.ProductId == productId && v.IsActive)
                .ToListAsync(ct);

            var old = _db.MarketplaceVariantAttributes.Where(x => x.MarketplaceProductId == mp.Id);
            _db.MarketplaceVariantAttributes.RemoveRange(old);

            foreach (var v in variants)
            {
                if (colorAttrId.HasValue)
                {
                    _db.MarketplaceVariantAttributes.Add(new MarketplaceVariantAttribute
                    {
                        Id = Guid.NewGuid(),
                        MarketplaceProductId = mp.Id,
                        ProductVariantId = v.Id,
                        AttributeId = colorAttrId.Value,
                        ValueId = null,            // allowCustom:true kabul
                        ValueName = v.Color ?? ""
                    });
                }
                if (sizeAttrId.HasValue)
                {
                    _db.MarketplaceVariantAttributes.Add(new MarketplaceVariantAttribute
                    {
                        Id = Guid.NewGuid(),
                        MarketplaceProductId = mp.Id,
                        ProductVariantId = v.Id,
                        AttributeId = sizeAttrId.Value,
                        ValueId = null,            // ileride TY ValueId eşlemesi eklenebilir
                        ValueName = v.Size ?? ""
                    });
                }
            }

            await _db.SaveChangesAsync(ct);
            return Ok(new { marketplaceProductId = mp.Id, mappedVariants = variants.Count, colorAttrId, sizeAttrId });
        }
    }
}
