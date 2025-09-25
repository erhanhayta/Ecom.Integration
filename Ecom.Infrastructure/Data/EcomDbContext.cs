using Ecom.Domain.Entities;
using Ecom.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Infrastructure.Data
{
    public class EcomDbContext : DbContext
    {
        public EcomDbContext(DbContextOptions<EcomDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>(); // DbSet<ProductVariant> ProductVariants da olur; tutarlı kullan
        public DbSet<MarketplaceShop> MarketplaceShops => Set<MarketplaceShop>();
        public DbSet<User> Users => Set<User>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<MarketplaceProduct> MarketplaceProducts => Set<MarketplaceProduct>();
        public DbSet<MarketplaceProductAttribute> MarketplaceProductAttributes => Set<MarketplaceProductAttribute>();
        public DbSet<MarketplaceVariantAttribute> MarketplaceVariantAttributes => Set<MarketplaceVariantAttribute>();
        public DbSet<MarketplaceAttributeMapping> MarketplaceAttributeMappings => Set<MarketplaceAttributeMapping>();



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new MarketplaceProductConfiguration());
            modelBuilder.ApplyConfiguration(new MarketplaceProductAttributeConfiguration());
            modelBuilder.ApplyConfiguration(new MarketplaceVariantAttributeConfiguration());
            modelBuilder.ApplyConfiguration(new MarketplaceAttributeMappingConfiguration());

            // === Product ===
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(x => x.Id);

                b.Property(x => x.Name).IsRequired().HasMaxLength(200);
                b.Property(x => x.ProductCode).IsRequired().HasMaxLength(64);
                b.HasIndex(x => x.ProductCode).IsUnique();

                b.Property(x => x.Barcode).IsRequired().HasMaxLength(64);
                b.Property(x => x.Description).HasMaxLength(2000);
                b.Property(x => x.Brand).HasMaxLength(128);

                b.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
                b.Property(x => x.TaxRate).HasColumnType("decimal(18,2)");

                b.Property(x => x.IsActive).HasDefaultValue(true);
                b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
                b.Property(x => x.UpdatedAtUtc);
            });

            // === ProductVariant ===
            modelBuilder.Entity<ProductVariant>(b =>
            {
                b.ToTable("ProductVariants");
                b.HasKey(x => x.Id);

                b.Property(x => x.Color).IsRequired().HasMaxLength(64);
                b.Property(x => x.Size).IsRequired().HasMaxLength(64);
                b.Property(x => x.Sku).IsRequired().HasMaxLength(64);
                b.Property(x => x.Barcode).HasMaxLength(64);
                b.Property(x => x.Price).HasColumnType("decimal(18,2)");

                b.HasOne(x => x.Product)
                 .WithMany(p => p.Variants)
                 .HasForeignKey(x => x.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => new { x.ProductId, x.Color, x.Size }).IsUnique();
                b.HasIndex(x => x.Sku).IsUnique();
            });

            // === MarketplaceShop ===
            modelBuilder.Entity<MarketplaceShop>(b =>
            {
                b.ToTable("MarketplaceShops");
                b.HasKey(x => x.Id);

                b.Property(x => x.ShopName).IsRequired().HasMaxLength(200);
                b.Property(x => x.BaseUrl).IsRequired().HasMaxLength(500);
                b.Property(x => x.ApiKey).IsRequired().HasMaxLength(500);
                b.Property(x => x.ApiSecret).IsRequired().HasMaxLength(500);

                b.Property(x => x.Token).HasMaxLength(1000);
                b.Property(x => x.AccountId).HasMaxLength(200);
                b.Property(x => x.IntegrationRefCode).HasMaxLength(200);

                b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
            });

            // === User ===
            modelBuilder.Entity<User>(b =>
            {
                b.ToTable("Users");
                b.HasKey(x => x.Id);

                b.Property(x => x.Username).IsRequired().HasMaxLength(100);
                b.HasIndex(x => x.Username).IsUnique();

                b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(200);
                b.Property(x => x.Role).IsRequired().HasMaxLength(50);
                b.Property(x => x.IsActive).HasDefaultValue(true);
                b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
            });

            // ProductImage
            modelBuilder.Entity<ProductImage>(b =>
            {
                b.ToTable("ProductImages");
                b.HasKey(x => x.Id);

                b.Property(x => x.Url).IsRequired().HasMaxLength(1024);
                b.Property(x => x.FileName).IsRequired().HasMaxLength(255);

                // Zorunlu: her resim bir ürüne bağlı
                b.HasOne(pi => pi.Product)
                 .WithMany(p => p.Images)
                 .HasForeignKey(pi => pi.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);

                // Opsiyonel: resim bir varyanta da bağlanabilir
                b.HasOne(pi => pi.ProductVariant)
                 .WithMany(v => v.Images)              // koleksiyon tutuyorsan WithMany(v => v.Images)
                 .HasForeignKey(pi => pi.ProductVariantId)
                 .OnDelete(DeleteBehavior.Restrict);   // <-- CASCADE değil; “multiple cascade paths” hatasını önler
            });

        }
    }
}
