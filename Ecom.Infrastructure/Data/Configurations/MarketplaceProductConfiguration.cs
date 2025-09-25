using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Infrastructure.Data.Configurations
{
    public class MarketplaceProductConfiguration : IEntityTypeConfiguration<MarketplaceProduct>
    {
        public void Configure(EntityTypeBuilder<MarketplaceProduct> b)
        {
            b.ToTable("MarketplaceProducts");
            b.HasKey(x => x.Id);

            b.Property(x => x.CategoryName).HasMaxLength(200);
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(x => x.UpdatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            // Index’ler
            b.HasIndex(x => new { x.ProductId, x.ShopId, x.Firm }).IsUnique();
            b.HasIndex(x => x.CategoryId);

            // İlişkiler (shadow FK’ler varsa ekleyebilirsin)
            // b.HasOne<Product>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            // b.HasOne<MarketplaceShop>().WithMany().HasForeignKey(x => x.ShopId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
