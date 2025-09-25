using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ecom.Api.Services
{
    public class ImageProcessingService
    {
        /// <summary>
        /// Görseli oku, EXIF orientation düzelt, 1200x1800'e pad'le, DPI=96 yaz, JPEG olarak kaydet.
        /// </summary>
        public async Task ProcessAndSaveAsync(
            Stream input,
            string absolutePath,
            int targetWidth,
            int targetHeight,
            int jpegQuality,
            CancellationToken ct)
        {
            input.Position = 0;

            using var image = await Image.LoadAsync<Rgba32>(input, ct);

            // EXIF orientation normalle
            image.Mutate(x => x.AutoOrient());

            // 1200x1800 içine sığdır ve pad ile beyaz zeminle tamamla
            var bg = Color.White;
            var resizeOpts = new ResizeOptions
            {
                Size = new Size(targetWidth, targetHeight),
                Mode = ResizeMode.Pad,
                Position = AnchorPositionMode.Center,
                Sampler = KnownResamplers.Bicubic,
                PremultiplyAlpha = true,
                PadColor = bg
            };

            image.Mutate(x => x.Resize(resizeOpts).BackgroundColor(bg));

            // 96 DPI yaz
            var meta = image.Metadata ?? new ImageMetadata();
            meta.HorizontalResolution = 96;
            meta.VerticalResolution = 96;

            // Klasörü oluştur
            var dir = Path.GetDirectoryName(absolutePath)!;
            Directory.CreateDirectory(dir);

            // JPEG olarak kaydet (quality)
            var encoder = new JpegEncoder { Quality = jpegQuality };
            await image.SaveAsJpegAsync(absolutePath, encoder, ct);
        }
    }
}
