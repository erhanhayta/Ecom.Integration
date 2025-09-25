using Ecom.Api.Services;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Ecom.Api.Controllers.admin
{
    public sealed class ProductImageUploadForm
    {
        [FromForm(Name = "file"), Required]
        public IFormFile File { get; set; } = default!;

        [FromQuery(Name = "variantId")]
        public Guid? VariantId { get; set; }

        [FromForm(Name = "sortOrder")]
        public int? SortOrder { get; set; }

        [FromForm(Name = "isMain")]
        public bool? IsMain { get; set; }
    }

    public sealed class ProductImageBulkUploadForm
    {
        [FromForm(Name = "files"), Required]
        public List<IFormFile> Files { get; set; } = new();

        [FromQuery(Name = "variantId")]
        public Guid? VariantId { get; set; }

        [FromForm(Name = "sortOrder")]
        public int? SortOrder { get; set; }

        [FromForm(Name = "isMain")]
        public bool? IsMain { get; set; }
    }

    [ApiController]
    [Route("admin/products/{productId:guid}/images")]
    public class ProductImagesController : ControllerBase
    {
        private static readonly HashSet<string> AllowedExts = new(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".webp" };

        private const int MaxImagesPerBarcode = 8;

        private readonly EcomDbContext _db;
        private readonly ImageProcessingService _img;

        public ProductImagesController(EcomDbContext db, ImageProcessingService img)
        {
            _db = db; _img = img;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> Upload(
            Guid productId,
            [FromQuery] Guid? variantId,
            [FromForm] ProductImageUploadForm form,
            CancellationToken ct)
        {
            var prod = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
            if (prod is null) return NotFound("Product not found");

            
            var f = form.File;
            if (f is null || f.Length == 0) return BadRequest("file missing");
            if (!AllowedExts.Contains(Path.GetExtension(f.FileName)))
                return BadRequest("Only jpg, jpeg, png, webp are allowed.");

            var relPath = await SaveFileAsync(productId, f, ct);
            var entity = new Ecom.Domain.Entities.ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductVariantId = variantId,
                FileName = Path.GetFileName(relPath),
                Url = "/" + relPath.Replace("\\", "/"),
                SortOrder = form.SortOrder ?? 0,
                IsMain = form.IsMain ?? false
            };
            _db.ProductImages.Add(entity);
            await _db.SaveChangesAsync(ct);

            // Not: URL relative; prod’da HTTPS altında servis edilecek.
            return Ok(new { id = entity.Id, url = entity.Url, variantId = entity.ProductVariantId, sortOrder = entity.SortOrder, isMain = entity.IsMain });
        }

        [HttpPost("bulk")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(200_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        public async Task<IActionResult> UploadBulk(
            Guid productId,
            [FromQuery] Guid? variantId,
            [FromForm] ProductImageBulkUploadForm form,
            CancellationToken ct)
        {
            var prod = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct);
            if (prod is null) return NotFound("Product not found");

            if (form.Files == null || form.Files.Count == 0)
                return BadRequest("files missing");

            var results = new List<object>();
            foreach (var f in form.Files)
            {
                if (f == null || f.Length == 0) continue;
                if (!AllowedExts.Contains(Path.GetExtension(f.FileName)))
                    return BadRequest("Only jpg, jpeg, png, webp are allowed.");

                var relPath = await SaveFileAsync(productId, f, ct);
                var entity = new Ecom.Domain.Entities.ProductImage
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ProductVariantId = variantId,
                    FileName = Path.GetFileName(relPath),
                    Url = "/" + relPath.Replace("\\", "/"),
                    SortOrder = form.SortOrder ?? 0,
                    IsMain = form.IsMain ?? false
                };
                _db.ProductImages.Add(entity);
                results.Add(new { id = entity.Id, url = entity.Url, variantId = entity.ProductVariantId, sortOrder = entity.SortOrder, isMain = entity.IsMain });
            }

            await _db.SaveChangesAsync(ct);
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> List(Guid productId, [FromQuery] Guid? variantId, CancellationToken ct = default)
        {
            var q = _db.ProductImages.AsNoTracking().Where(x => x.ProductId == productId);
            if (variantId.HasValue) q = q.Where(x => x.ProductVariantId == variantId.Value);
            else q = q.OrderBy(x => x.SortOrder);

            var list = await q.Select(x => new { x.Id, x.Url, x.SortOrder, x.IsMain, x.ProductVariantId }).ToListAsync(ct);
            return Ok(list);
        }

        [HttpDelete("{imageId:guid}")]
        public async Task<IActionResult> Delete(Guid productId, Guid imageId, CancellationToken ct)
        {
            var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId && x.ProductId == productId, ct);
            if (img == null) return NotFound();

            var absPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (System.IO.File.Exists(absPath)) System.IO.File.Delete(absPath);

            _db.ProductImages.Remove(img);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }

        // ---- Helpers ----
        private async Task<string> SaveFileAsync(Guid productId, IFormFile f, CancellationToken ct)
        {
            // Girdi uzantısını sadece kabul/ret için kullanıyoruz; çıktı HER ZAMAN .jpg
            var inputExt = Path.GetExtension(f.FileName);
            if (string.IsNullOrWhiteSpace(inputExt) ||
                !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(inputExt, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only jpg, jpeg, png, webp are allowed.");

            var relDir = Path.Combine("uploads", "products", productId.ToString("N"));
            var absDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relDir);
            Directory.CreateDirectory(absDir);

            var fileName = $"{Guid.NewGuid():N}.jpg"; // <-- ÇIKTI UZANTISI SABİT .jpg
            var relPath = Path.Combine(relDir, fileName);
            var absPath = Path.Combine(absDir, fileName);

            await using var input = f.OpenReadStream();
            await _img.ProcessAndSaveAsync(input, absPath, 1200, 1800, 85, ct); // 96 DPI servis içinde
            return relPath;
        }

    }
}
