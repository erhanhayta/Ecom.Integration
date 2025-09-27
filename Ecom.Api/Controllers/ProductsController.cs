using Ecom.Application.Products.Request;
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

    [Authorize(Roles = "Admin,admin")]
    [ApiController]
    [Route("admin/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly EcomDbContext _db;
        public ProductsController(EcomDbContext db) => _db = db;

        // --- Listeleme: sade, variants DÖNMEZ ---
        // GET /admin/products
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var list = await _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAtUtc)
                .Select(p => new ProductListItemDto(
                    p.Id,
                    p.Name,
                    p.ProductCode,
                    p.Barcode ?? "",
                    p.Brand,
                    p.BasePrice,
                    p.TaxRate,
                    p.Description,
                    p.IsActive,
                    p.CreatedAtUtc
                ))
                .ToListAsync(ct);

            return Ok(list);
        }

        // --- Detay: tek endpoint — query ile dallanır ---
        // GET /admin/products/{id}?includeVariants=true|false
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, [FromQuery] bool includeVariants = false, CancellationToken ct = default)
        {
            IQueryable<Product> q = _db.Products.AsNoTracking();

            if (includeVariants)
                q = q.Include(p => p.Variants);

            var item = await q.FirstOrDefaultAsync(p => p.Id == id, ct);
            return item is null ? NotFound() : Ok(item);
        }

        // POST /admin/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest req, CancellationToken ct)
        {
            var entity = new Product
            {
                Name = req.Name.Trim(),
                ProductCode = req.ProductCode.Trim(),
                Barcode = req.Barcode?.Trim(),
                Brand = req.Brand?.Trim(),
                BasePrice = req.BasePrice,
                TaxRate = req.TaxRate,
                IsActive = req.IsActive
            };

            _db.Products.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Tek detay endpoint'i: nameof(Get)
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, new { id = entity.Id });
        }

        // PUT /admin/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest req, CancellationToken ct)
        {
            var entity = await _db.Products.FindAsync(new object?[] { id }, ct);
            if (entity is null) return NotFound();

            entity.Name = req.Name.Trim();
            entity.ProductCode = req.ProductCode.Trim();
            entity.Barcode = req.Barcode?.Trim();
            entity.Brand = req.Brand?.Trim();
            entity.BasePrice = req.BasePrice;
            entity.TaxRate = req.TaxRate;
            entity.IsActive = req.IsActive;

            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
