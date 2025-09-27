using Ecom.Api.Services;
using Ecom.Application.Marketplaces;
using Ecom.Infrastructure.Data;
using Ecom.Infrastructure.Marketplaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON
builder.Services.AddControllers().AddJsonOptions(opts =>
{
    opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Ecom.Application.Products.Validators.CreateProductRequestValidator>();

// CORS (geliştirme için spesifik origin)
const string CorsPolicy = "FrontendPolicy";
var origins = builder.Configuration.GetSection("Frontend:Origins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins(origins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
    );
});
builder.Services.AddValidatorsFromAssemblyContaining<Ecom.Application.Products.Validators.CreateProductRequestValidator>();

// Http & Cache
builder.Services.AddHttpClient();
builder.Services.Configure<TrendyolOptions>(builder.Configuration.GetSection("Trendyol"));
builder.Services.AddHttpClient("Trendyol", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient("Hepsiburada", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddHttpClient("N11", c => c.Timeout = TimeSpan.FromSeconds(30));
builder.Services.AddMemoryCache();

// Marketplace catalog services (SCOPED)
builder.Services.AddScoped<IMarketplaceCatalogService, TrendyolCatalogService>();
builder.Services.AddScoped<IMarketplaceCatalogService, HepsiburadaCatalogService>();
builder.Services.AddScoped<IMarketplaceCatalogService, N11CatalogService>();
builder.Services.AddScoped<IMarketplaceCatalogServiceFactory, MarketplaceCatalogServiceFactory>();

// Image processing service
builder.Services.AddScoped<ImageProcessingService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ecom.Api", Version = "v1" });
    c.SupportNonNullableReferenceTypes();

#if DEBUG

#endif
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // ModelState → { key: string, errors: string[] }
        var errors = context.ModelState
            .Where(kv => kv.Value is { Errors.Count: > 0 })
            .ToDictionary(
                kv => kv.Key,
                kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
            );

        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Title = "Validation Failed",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://datatracker.ietf.org/doc/html/rfc7807",
            Detail = "Request validation failed. See 'errors' for details."
        };

        // Ek alan: frontende kolay haritalansın
        problem.Extensions["errors"] = errors;

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

// DbContext
builder.Services.AddDbContext<EcomDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// JWT
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

var jwtKey = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Token service
builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
