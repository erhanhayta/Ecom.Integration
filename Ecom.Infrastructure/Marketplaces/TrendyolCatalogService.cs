using Ecom.Application.Marketplaces;
using Ecom.Contracts.Marketplaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Marketplaces; // <<< ÖNEMLİ: enum burada
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Ecom.Infrastructure.Marketplaces
{
    public class TrendyolCatalogService : IMarketplaceCatalogService
    {
        private readonly IHttpClientFactory _http;
        private readonly JsonSerializerOptions _json;
        private readonly IConfiguration _config;

        // Trendyol public kategori ağacı
        private const string CategoriesUrl =
            "https://apigw.trendyol.com/integration/product/product-categories";

        // Sizin enum: Trendyol / Hepsiburada / N11
        public Firm Firm => Firm.Trendyol;

        public TrendyolCatalogService(IHttpClientFactory http, IConfiguration config)
        {
            _http = http;
            _config = config;
            _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        #region DTOs (endpoint response'ları)
        private sealed class TrendyolCategoriesResponse
        {
            public List<TrendyolCategoryRow> Categories { get; set; } = new();
        }

        private sealed class TrendyolCategoryRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int? ParentId { get; set; }
            public List<TrendyolCategoryRow> SubCategories { get; set; } = new();
        }

        private sealed class TrendyolBrandRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }
        #endregion

        private HttpClient CreateClient()
        {
            var client = _http.CreateClient("Trendyol");
            if (!client.DefaultRequestHeaders.UserAgent.Any())
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Ecom-Integration/1.0 (+https://localhost)");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private static IEnumerable<TrendyolCategoryRow> Flatten(TrendyolCategoryRow n)
        {
            yield return n;
            if (n.SubCategories != null)
                foreach (var c in n.SubCategories)
                    foreach (var x in Flatten(c))
                        yield return x;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(
            MarketplaceShop shop,
            int? parentId,
            string? query,
            CancellationToken ct)
        {
            using var client = CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Get, CategoriesUrl);

            using var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Trendyol categories {(int)resp.StatusCode}: {txt}");
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<TrendyolCategoriesResponse>(stream, _json, ct)
                       ?? new TrendyolCategoriesResponse();

            IEnumerable<TrendyolCategoryRow> source;

            if (parentId is null)
            {
                // kök
                source = data.Categories;
            }
            else
            {
                // ilgili düğümün çocukları
                var node = data.Categories.SelectMany(Flatten).FirstOrDefault(n => n.Id == parentId.Value);
                source = node?.SubCategories ?? Enumerable.Empty<TrendyolCategoryRow>();
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var t = query.Trim().ToLowerInvariant();
                source = source.Where(s => (s.Name ?? "").ToLowerInvariant().Contains(t));
            }

            // CategoryDto(int id, string name, int? parentId, bool hasChildren)
            return source
                .Select(s => new CategoryDto(s.Id, s.Name, s.ParentId, s.SubCategories != null && s.SubCategories.Count > 0))
                .ToList();
        }

        public Task<IReadOnlyList<CategoryAttributeDto>> GetCategoryAttributesAsync(
            MarketplaceShop shop,
            int categoryId,
            CancellationToken ct)
        {
            // Trendyol kategori özellikleri için ayrı endpoint gerekir.
            // Şimdilik boş liste (UI bu durumda “özellik yok” şeklinde davranabilir).
            return Task.FromResult<IReadOnlyList<CategoryAttributeDto>>(Array.Empty<CategoryAttributeDto>());
        }

        public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(
        MarketplaceShop shop, string? query, CancellationToken ct)
        {
            using var client = CreateClient();

            // Not: Trendyol tarafı Cloudflare koruması uygulayabiliyor.
            // Eğer 556 / 403 alırsan appsettings'te "Trendyol:Cookie" tanımlayıp buradan header ekle.
            var cookie = _config["Trendyol:Cookie"];
            if (!string.IsNullOrWhiteSpace(cookie))
                client.DefaultRequestHeaders.Add("Cookie", cookie);

            var url = "https://apigw.trendyol.com/integration/product/brands";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            using var resp = await client.SendAsync(req, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Trendyol brands {(int)resp.StatusCode}: {txt}");
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<TrendyolBrandsResponse>(stream, _json, ct)
                       ?? new TrendyolBrandsResponse();

            IEnumerable<TrendyolBrandRow> src = data.Brands ?? new();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var t = query.Trim().ToLowerInvariant();
                src = src.Where(b => (b.Name ?? "").ToLowerInvariant().Contains(t));
            }

            return src.Select(b => new BrandDto(b.Id, b.Name)).ToList();
        }

        // ---- dosyanın üst kısmındaki private DTO'lara ekle:
        private sealed class TrendyolBrandsResponse
        {
            public List<TrendyolBrandRow> Brands { get; set; } = new();
        }

    }
}
