using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ecom.Api.Services
{
    public class ImageProcessingService
    {
        public async Task ProcessAndSaveAsync(Stream input, string outPath, int width = 1200, int height = 1800, int quality = 85, CancellationToken ct = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

            using var image = await Image.LoadAsync<Rgba32>(input, ct);

            // Canvas: 1200x1800, beyaz arkaplan
            using var canvas = new Image<Rgba32>(width, height, Color.White);

            // Resmi canvas içine sığdır (aspect protect)
            var ratio = Math.Min((double)width / image.Width, (double)height / image.Height);
            var newW = (int)Math.Round(image.Width * ratio);
            var newH = (int)Math.Round(image.Height * ratio);

            image.Mutate(x => x.Resize(new Size(newW, newH)));

            // ortala
            var posX = (width - newW) / 2;
            var posY = (height - newH) / 2;

            canvas.Mutate(x => x.DrawImage(image, new Point(posX, posY), 1f));

            // 96 DPI
            canvas.Metadata.VerticalResolution = 96;
            canvas.Metadata.HorizontalResolution = 96;

            var encoder = new JpegEncoder { Quality = quality };
            await canvas.SaveAsync(outPath, encoder, ct);
        }
    }
}
