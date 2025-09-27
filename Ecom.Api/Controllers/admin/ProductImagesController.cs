using Ecom.Api.Services;
using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Api.Controllers.admin
{
    public class ProductImageUploadForm
    {
        [FromForm(Name = "file")]
        public IFormFile File { get; set; } = default!;
    }
    [Authorize(Roles = "Admin,admin")]
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

        // 1) Yükle: tek fiziksel dosya kaydı (ProductImages)
        //    İstersek ürüne ilk link de oluşturabiliriz (linkToProduct=true)
        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
        public async Task<IActionResult> Upload(
            Guid productId,
            [FromForm] ProductImageUploadForm form,
            [FromQuery] bool linkToProduct = true,
            [FromQuery] int sortOrder = 0,
            [FromQuery] bool isMain = false,
            CancellationToken ct = default)
        {
            var prodExists = await _db.Products.AnyAsync(p => p.Id == productId, ct);
            if (!prodExists) return NotFound("Product not found");

            var file = form.File;
            if (file is null || file.Length == 0) return BadRequest("file missing");

            var fileName = $"{Guid.NewGuid():N}.jpg";
            var relDir = Path.Combine("uploads", "products", productId.ToString("N"));
            var relPath = Path.Combine(relDir, fileName);
            var absPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relPath);

            Directory.CreateDirectory(Path.GetDirectoryName(absPath)!);
            await using (var input = file.OpenReadStream())
                await _img.ProcessAndSaveAsync(input, absPath, 1200, 1800, 96, ct);

            var img = new ProductImage
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ProductVariantId = null, // yeni mimaride fiziksel kayıt varyantsız
                FileName = fileName,
                Url = "/" + relPath.Replace("\\", "/")
            };
            _db.ProductImages.Add(img);
            await _db.SaveChangesAsync(ct);

            // opsiyonel ilk link (ürüne)
            if (linkToProduct)
            {
                var link = new ProductImageLink
                {
                    Id = Guid.NewGuid(),
                    ProductId = productId,
                    ProductImageId = img.Id,
                    ProductVariantId = null,
                    SortOrder = sortOrder,
                    IsMain = isMain,
                    CreatedAtUtc = DateTime.UtcNow
                };
                _db.ProductImageLinks.Add(link);
                await _db.SaveChangesAsync(ct);
            }

            return Ok(new { imageId = img.Id, url = img.Url });
        }

        // 2) Link oluştur: upload edilmiş imageId’leri ürüne/variantlara bağla
        public sealed class LinkRequest
        {
            public List<Guid> ImageIds { get; set; } = new();
            public bool LinkToProduct { get; set; } = false;
            public List<Guid> VariantIds { get; set; } = new();
            public int SortOrder { get; set; } = 0;
            public bool IsMain { get; set; } = false;
        }

        [HttpPost("links")]
        public async Task<IActionResult> CreateLinks(Guid productId, [FromBody] LinkRequest req, CancellationToken ct)
        {
            if (!req.ImageIds?.Any() ?? true) return BadRequest("ImageIds required");

            var now = DateTime.UtcNow;

            foreach (var imgId in (req.ImageIds ?? Enumerable.Empty<Guid>()).Distinct())
            {
                var exists = await _db.ProductImages.AnyAsync(i => i.Id == imgId && i.ProductId == productId, ct);
                if (!exists) return BadRequest($"ImageId not belongs to product: {imgId}");

                if (req.LinkToProduct)
                {
                    _db.ProductImageLinks.Add(new ProductImageLink
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        ProductImageId = imgId,
                        ProductVariantId = null,
                        SortOrder = req.SortOrder,
                        IsMain = req.IsMain,
                        CreatedAtUtc = now
                    });
                }

                foreach (var vid in (req.VariantIds ?? new()).Distinct())
                {
                    var vExists = await _db.ProductVariants.AnyAsync(v => v.Id == vid && v.ProductId == productId, ct);
                    if (!vExists) return BadRequest($"Variant not found: {vid}");

                    _db.ProductImageLinks.Add(new ProductImageLink
                    {
                        Id = Guid.NewGuid(),
                        ProductId = productId,
                        ProductImageId = imgId,
                        ProductVariantId = vid,
                        SortOrder = req.SortOrder,
                        IsMain = false,
                        CreatedAtUtc = now
                    });
                }
            }

            await _db.SaveChangesAsync(ct);
            return Ok();
        }

        // 3) Link sil (dosyayı değil, bağlantıyı sil)
        //    Eğer bu image için hiç link kalmazsa fiziksel dosyayı ve ProductImage kaydını da sil
        [HttpDelete("links/{linkId:guid}")]
        public async Task<IActionResult> DeleteLink(Guid productId, Guid linkId, CancellationToken ct)
        {
            var link = await _db.ProductImageLinks
                .FirstOrDefaultAsync(x => x.Id == linkId && x.ProductId == productId, ct);
            if (link is null) return NotFound();

            var imgId = link.ProductImageId;
            _db.ProductImageLinks.Remove(link);
            await _db.SaveChangesAsync(ct);

            var hasMore = await _db.ProductImageLinks.AnyAsync(x => x.ProductImageId == imgId, ct);
            if (!hasMore)
            {
                var img = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == imgId, ct);
                if (img != null)
                {
                    var absPath = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        img.Url.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString())
                    );
                    if (System.IO.File.Exists(absPath))
                        System.IO.File.Delete(absPath);

                    _db.ProductImages.Remove(img);
                    await _db.SaveChangesAsync(ct);
                }
            }

            return NoContent();
        }


        [HttpGet]
        public async Task<IActionResult> List(Guid productId, [FromQuery] Guid? variantId, CancellationToken ct = default)
        {
            var q =
                from l in _db.ProductImageLinks.AsNoTracking()
                where l.ProductId == productId
                join i in _db.ProductImages.AsNoTracking() on l.ProductImageId equals i.Id
                select new
                {
                    linkId = l.Id,
                    imageId = i.Id,
                    url = i.Url,
                    sortOrder = l.SortOrder,
                    isMain = l.IsMain,
                    productVariantId = l.ProductVariantId
                };

            // Varyant görünümünde artık ürün (null) linkleri gelmesin:
            if (variantId.HasValue)
                q = q.Where(x => x.productVariantId == variantId.Value);
            else
                q = q.Where(x => x.productVariantId == null);

            var list = await q.OrderBy(x => x.sortOrder).ToListAsync(ct);
            return Ok(list);
        }


    }
}
