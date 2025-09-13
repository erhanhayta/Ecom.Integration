using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers
{
    // Liste ekranı için hafif DTO (variants yok)
    public sealed record ProductListItemDto(
        Guid Id,
        string Name,
        string ProductCode,
        string Barcode,
        string? Brand,
        decimal BasePrice,
        decimal TaxRate,
        string? Description,
        bool IsActive,
        DateTime CreatedAtUtc
    );

    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("admin/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly EcomDbContext _db;
        public ProductsController(EcomDbContext db) => _db = db;

        // --- Listeleme: sade, variants DÖNMEZ ---
        // GET /admin/products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Projeksiyon -> DTO (navigation yok, serialize edilmez)
            var list = await _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => new ProductListItemDto(
                    p.Id,
                    p.Name,
                    p.ProductCode,
                    p.Barcode,
                    p.Brand,
                    p.BasePrice,
                    p.TaxRate,
                    p.Description,
                    p.IsActive,
                    p.CreatedAtUtc
                ))
                .ToListAsync();

            return Ok(list);
        }

        // --- Detay: variants İLE birlikte ---
        // GET /admin/products/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await _db.Products
                .Include(p => p.Variants)       // <-- varyantları da getir
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return item is null ? NotFound() : Ok(item);
        }

        // POST /admin/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product dto)
        {
            dto.Id = Guid.NewGuid();
            dto.CreatedAtUtc = DateTime.UtcNow;
            dto.UpdatedAtUtc = null;

            _db.Products.Add(dto);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        // PUT /admin/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] Product dto)
        {
            var existing = await _db.Products.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Name = dto.Name;
            existing.ProductCode = dto.ProductCode;
            existing.Barcode = dto.Barcode;
            existing.Description = dto.Description;
            existing.Brand = dto.Brand;
            existing.BasePrice = dto.BasePrice;
            existing.TaxRate = dto.TaxRate;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /admin/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _db.Products.FindAsync(id);
            if (existing is null) return NotFound();

            _db.Products.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
