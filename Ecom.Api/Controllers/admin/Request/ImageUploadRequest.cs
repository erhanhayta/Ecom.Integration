using Microsoft.AspNetCore.Http;
using System;

namespace Ecom.Api.Controllers.admin.Requests
{
    public class ImageUploadRequest
    {
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }  // opsiyonel

        // Dosya alanı mutlaka IFormFile olmalı ve FromForm ile gelecek
        public IFormFile File { get; set; } = null!;
    }
}
