using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("admin/[controller]")]
    public class MarketplaceShopsController : ControllerBase
    {
        private readonly EcomDbContext _db;
        public MarketplaceShopsController(EcomDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.MarketplaceShops
                .AsNoTracking()
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await _db.MarketplaceShops.FindAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MarketplaceShop dto)
        {
            dto.Id = Guid.NewGuid();
            dto.CreatedAtUtc = DateTime.UtcNow;

            _db.MarketplaceShops.Add(dto);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MarketplaceShop dto)
        {
            var existing = await _db.MarketplaceShops.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Firm = dto.Firm;
            existing.ShopName = dto.ShopName;
            existing.BaseUrl = dto.BaseUrl;
            existing.ApiKey = dto.ApiKey;
            existing.ApiSecret = dto.ApiSecret;
            existing.AccountId = dto.AccountId;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var existing = await _db.MarketplaceShops.FindAsync(id);
            if (existing is null) return NotFound();

            _db.MarketplaceShops.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
