// Ecom.Api/Controllers/admin/ProductImagesController.cs
using Ecom.Api.Services;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers.admin
{
    public class ProductImageUploadForm
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = default!;
    }

    [ApiController]
    [Route("admin/products/{productId:guid}/images")]
    public class ProductImagesController : ControllerBase
    {
        private readonly EcomDbContext _db;
        private readonly ImageProcessingService _img;

        public ProductImagesController(EcomDbContext db, ImageProcessingService img)
        {
            _db = db; _img = img;
        }

        // VARYANT DESTEKLİ YÜKLEME
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> Upload(
            Guid productId,
            [FromQuery] Guid? variantId,                 // query ile
            [FromForm] ProductImageUploadForm form,      // dosya form model içinde
            CancellationToken ct)
        {
            var prod = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
            if (prod is null) return NotFound("Product not found");

            if (variantId.HasValue)
            {
                var exists = await _db.Variants
                    .AnyAsync(v => v.Id == variantId.Value && v.ProductId == productId, ct);
                if (!exists) return BadRequest("VariantId does not belong to product");
            }

            var file = form.File;
            if (file is null || file.Length == 0) return BadRequest("file missing");

            var fileName = $"{Guid.NewGuid():N}.jpg";
            var relDir = Path.Combine("uploads", "products", productId.ToString("N"));
            var relPath = Path.Combine(relDir, fileName);
            var absPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relPath);

            await using var input = file.OpenReadStream();
            await _img.ProcessAndSaveAsync(input, absPath, 1200, 1800, 85, ct);

            var entity = new Ecom.Domain.Entities.ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductVariantId = variantId,
                FileName = fileName,
                Url = "/" + relPath.Replace("\\", "/")
            };
            _db.ProductImages.Add(entity);
            await _db.SaveChangesAsync(ct);

            return Ok(new { id = entity.Id, url = entity.Url, variantId = entity.ProductVariantId });
        }

        [HttpGet]
        public async Task<IActionResult> List(
            Guid productId,
            [FromQuery] Guid? variantId,
            CancellationToken ct = default)
        {
            var q = _db.ProductImages.Where(x => x.ProductId == productId);
            if (variantId.HasValue) q = q.Where(x => x.ProductVariantId == variantId.Value);
            else q = q.OrderBy(x => x.SortOrder);

            var list = await q.Select(x => new
            {
                x.Id,
                x.Url,
                x.SortOrder,
                x.ProductVariantId
            }).ToListAsync(ct);

            return Ok(list);
        }

        [HttpDelete("{imageId:guid}")]
        public async Task<IActionResult> Delete(Guid productId, Guid imageId, CancellationToken ct)
        {
            var img = await _db.ProductImages
                .FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, ct);
            if (img == null) return NotFound();

            var absPath = Path.Combine(
                Directory.GetCurrentDirectory(), "wwwroot",
                img.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
            );
            if (System.IO.File.Exists(absPath))
                System.IO.File.Delete(absPath);

            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
    }
}
