using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers
{
    [Authorize(Roles = "Admin,admin")]
    [ApiController]
    [Route("admin")]
    public class VariantsController : ControllerBase
    {
        private readonly EcomDbContext _db;
        public VariantsController(EcomDbContext db) => _db = db;

        // 1) Bir ürünün tüm varyantları
        [HttpGet("products/{productId:guid}/variants")]
        public async Task<IActionResult> GetByProduct(Guid productId)
        {
            var exists = await _db.Products.AnyAsync(p => p.Id == productId);
            if (!exists) return NotFound("Product not found.");

            var list = await _db.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            return Ok(list);
        }

        // 2) Tek varyant çek
        [HttpGet("variants/{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var v = await _db.ProductVariants.FindAsync(id);
            return v is null ? NotFound() : Ok(v);
        }

        // 3) Ürüne varyant ekle
        public record CreateVariantRequest(string Color, string Size, string Sku, string? Barcode, decimal Price, int Stock, bool IsActive);

        [HttpPost("products/{productId:guid}/variants")]
        public async Task<IActionResult> Create(Guid productId, [FromBody] CreateVariantRequest req)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product is null) return NotFound("Product not found.");

            // İsteğe bağlı: Aynı kombinasyonu proaktif kontrol et (unique index zaten korur)
            var duplicate = await _db.ProductVariants.AnyAsync(x =>
                x.ProductId == productId && x.Color == req.Color && x.Size == req.Size);
            if (duplicate) return Conflict("This Color/Size combination already exists for the product.");

            var entity = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                Color = req.Color,
                Size = req.Size,
                Sku = req.Sku,
                Barcode = req.Barcode,
                Price = req.Price,
                Stock = req.Stock,
                IsActive = req.IsActive
            };

            _db.ProductVariants.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
        }

        // 4) Varyant güncelle
        public record UpdateVariantRequest(string Color, string Size, string Sku, string? Barcode, decimal Price, int Stock, bool IsActive);

        [HttpPut("variants/{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVariantRequest req)
        {
            var entity = await _db.ProductVariants.FindAsync(id);
            if (entity is null) return NotFound();

            entity.Color = req.Color;
            entity.Size = req.Size;
            entity.Sku = req.Sku;
            entity.Barcode = req.Barcode;
            entity.Price = req.Price;
            entity.Stock = req.Stock;
            entity.IsActive = req.IsActive;

            // Aynı ürün içinde (Color,Size) çakışması kontrolü
            var clash = await _db.ProductVariants.AnyAsync(x =>
                x.Id != id && x.ProductId == entity.ProductId &&
                x.Color == entity.Color && x.Size == entity.Size);
            if (clash) return Conflict("Another variant with same Color/Size already exists for this product.");

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // 5) Varyant sil
        [HttpDelete("variants/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _db.ProductVariants.FindAsync(id);
            if (entity is null) return NotFound();

            _db.ProductVariants.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("products/{productId:guid}/variants/{variantId:guid}")]
        public async Task<IActionResult> UpdatePartial(Guid productId, Guid variantId, [FromBody] VariantUpdateDto dto, CancellationToken ct)
        {
            var v = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == variantId && x.ProductId == productId, ct);
            if (v == null) return NotFound();

            if (dto.Price.HasValue) v.Price = dto.Price.Value;
            if (dto.Stock.HasValue) v.Stock = dto.Stock.Value;
            if (dto.IsActive.HasValue) v.IsActive = dto.IsActive.Value;

            await _db.SaveChangesAsync(ct);
            return Ok(new { v.Id, v.Price, v.Stock, v.IsActive });
        }

        [HttpPost("products/{productId:guid}/variants/bulk-price")]
        public async Task<IActionResult> BulkPrice(Guid productId, [FromBody] BulkPriceRequest req, CancellationToken ct)
        {
            // scope
            var q = _db.ProductVariants.Where(v => v.ProductId == productId);
            if (!string.IsNullOrWhiteSpace(req.Color))
                q = q.Where(v => v.Color == req.Color);
            if (!string.IsNullOrWhiteSpace(req.Size))
                q = q.Where(v => v.Size == req.Size);

            var list = await q.ToListAsync(ct);

            foreach (var v in list)
            {
                switch (req.Op) // set, inc, dec, incp, decp
                {
                    case "set": v.Price = req.Value; break;
                    case "inc": v.Price += req.Value; break;
                    case "dec": v.Price -= req.Value; break;
                    case "incp": v.Price += v.Price * (req.Value / 100m); break;
                    case "decp": v.Price -= v.Price * (req.Value / 100m); break;
                }
            }
            await _db.SaveChangesAsync(ct);
            return Ok(new { updated = list.Count });
        }
        public record BulkPriceRequest(string? Color, string? Size, string Op, decimal Value);
        public sealed class VariantUpdateDto
        {
            public decimal? Price { get; set; }
            public int? Stock { get; set; }
            public bool? IsActive { get; set; }
        }
    }
}
