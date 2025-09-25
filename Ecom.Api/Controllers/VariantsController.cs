using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers
{
    [Authorize(Roles = "Admin")]
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
    }
}
