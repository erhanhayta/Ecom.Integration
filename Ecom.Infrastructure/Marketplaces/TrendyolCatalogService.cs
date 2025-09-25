using Ecom.Application.Marketplaces;
using Ecom.Contracts.Marketplaces;
using Ecom.Domain.Entities;
using Ecom.Domain.Marketplaces; // <<< ÖNEMLİ: enum burada
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        // Trendyol marka listesi
        private const string BrandsUrl =
            "https://apigw.trendyol.com/integration/product/brands";

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

        private static void ApplyShopAuthAndBase(HttpClient client, MarketplaceShop shop)
        {
            var baseUrl = string.IsNullOrWhiteSpace(shop.BaseUrl)
                ? "https://apigw.trendyol.com"
                : shop.BaseUrl.TrimEnd('/');

            client.BaseAddress = new Uri(baseUrl);

            // Trendyol Basic Auth: base64(apiKey:apiSecret)
            var creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{shop.ApiKey}:{shop.ApiSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", creds);
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
                .Select(s => new CategoryDto(
                    s.Id,
                    s.Name,
                    s.ParentId,
                    s.SubCategories != null && s.SubCategories.Count > 0))
                .ToList();
        }

        public async Task<IReadOnlyList<CategoryAttributeDto>> GetCategoryAttributesAsync(
            MarketplaceShop shop,
            int categoryId,
            CancellationToken ct)
        {
            using var client = CreateClient();
            ApplyShopAuthAndBase(client, shop);

            var path = $"/integration/product/product-categories/{categoryId}/attributes";

            using var resp = await client.GetAsync(path, ct);
            resp.EnsureSuccessStatusCode();

            var payload = await resp.Content.ReadFromJsonAsync<TrendyolCategoryAttributesResponse>(_json, ct)
                          ?? new TrendyolCategoryAttributesResponse();

            var list = (payload.CategoryAttributes ?? new List<TrendyolCategoryAttributeRow>())
                .Select(row => new CategoryAttributeDto(
                    AttributeId: row.Attribute?.Id ?? 0,
                    Name: row.Attribute?.Name ?? string.Empty,
                    Required: row.Required,
                    AllowCustom: row.AllowCustom,
                    Varianter: row.Varianter,
                    Slicer: row.Slicer,
                    Values: (row.AttributeValues ?? new List<TrendyolAttributeValueRow>())
                                .Select(v => new CategoryAttributeValueDto(v.Id, v.Name ?? string.Empty))
                                .ToList()
                ))
                .ToList();

            return list;
        }

        public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(
            MarketplaceShop shop, string? query, CancellationToken ct)
        {
            using var client = CreateClient();
            ApplyShopAuthAndBase(client, shop);

            // (Opsiyonel) Cloudflare engeli için cookie
            var cookie = _config["Trendyol:Cookie"];
            if (!string.IsNullOrWhiteSpace(cookie))
                client.DefaultRequestHeaders.Add("Cookie", cookie);

            // Absolute kullanmak yerine BaseAddress + relative path de kullanılabilir.
            var path = "/integration/product/brands";
            using var resp = await client.GetAsync(path, ct);

            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Trendyol brands {(int)resp.StatusCode}: {txt}");
            }

            await using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var data = await JsonSerializer.DeserializeAsync<TrendyolBrandsResponse>(stream, _json, ct)
                       ?? new TrendyolBrandsResponse();

            IEnumerable<TrendyolBrandRow> src = data.Brands ?? Enumerable.Empty<TrendyolBrandRow>();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var t = query.Trim().ToLowerInvariant();
                src = src.Where(b => (b.Name ?? "").ToLowerInvariant().Contains(t));
            }

            return src.Select(b => new BrandDto(b.Id, b.Name ?? string.Empty)).ToList();
        }

        // ---- Response modelleri ----
        private sealed class TrendyolBrandsResponse
        {
            public List<TrendyolBrandRow> Brands { get; set; } = new();
        }

        private sealed class TrendyolCategoryAttributesResponse
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string? Name { get; set; }
            [JsonPropertyName("displayName")]
            public string? DisplayName { get; set; }
            [JsonPropertyName("categoryAttributes")]
            public List<TrendyolCategoryAttributeRow>? CategoryAttributes { get; set; }
        }

        private sealed class TrendyolCategoryAttributeRow
        {
            [JsonPropertyName("categoryId")]
            public int CategoryId { get; set; }

            [JsonPropertyName("attribute")]
            public TrendyolAttributeMeta? Attribute { get; set; }

            [JsonPropertyName("required")]
            public bool Required { get; set; }

            [JsonPropertyName("allowCustom")]
            public bool AllowCustom { get; set; }

            [JsonPropertyName("varianter")]
            public bool Varianter { get; set; }

            [JsonPropertyName("slicer")]
            public bool Slicer { get; set; }

            [JsonPropertyName("attributeValues")]
            public List<TrendyolAttributeValueRow>? AttributeValues { get; set; }
        }

        private sealed class TrendyolAttributeMeta
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        private sealed class TrendyolAttributeValueRow
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }
    }
}
