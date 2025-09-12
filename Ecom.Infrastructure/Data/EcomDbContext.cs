using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecom.Infrastructure.Data
{
    public class EcomDbContext : DbContext
    {
        public EcomDbContext(DbContextOptions<EcomDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductVariant> Variants => Set<ProductVariant>(); 
        public DbSet<MarketplaceShop> MarketplaceShops => Set<MarketplaceShop>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === Product ===
            modelBuilder.Entity<Product>(b =>
            {
                b.ToTable("Products");
                b.HasKey(x => x.Id);

                b.Property(x => x.Name)
                 .IsRequired()
                 .HasMaxLength(200);

                b.Property(x => x.ProductCode)
                 .IsRequired()
                 .HasMaxLength(64);

                b.HasIndex(x => x.ProductCode)
                 .IsUnique();

                b.Property(x => x.Barcode)
                 .IsRequired()
                 .HasMaxLength(64);

                b.Property(x => x.Description)
                 .HasMaxLength(2000); // isteğe göre büyütülebilir

                b.Property(x => x.Brand)
                 .HasMaxLength(128);

                b.Property(x => x.BasePrice)
                 .HasColumnType("decimal(18,2)");

                b.Property(x => x.TaxRate)
                 .HasColumnType("decimal(18,2)");

                b.Property(x => x.IsActive)
                 .HasDefaultValue(true);

                b.Property(x => x.CreatedAtUtc)
                 .HasDefaultValueSql("GETUTCDATE()");

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

                b.HasIndex(x => new { x.ProductId, x.Color, x.Size })
                 .IsUnique();

                b.HasIndex(x => x.Sku)
                 .IsUnique();
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
        }
    }
}
