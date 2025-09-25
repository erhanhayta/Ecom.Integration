using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Infrastructure.Data.Configurations
{
    public class MarketplaceVariantAttributeConfiguration : IEntityTypeConfiguration<MarketplaceVariantAttribute>
    {
        public void Configure(EntityTypeBuilder<MarketplaceVariantAttribute> b)
        {
            b.ToTable("MarketplaceVariantAttributes");
            b.HasKey(x => x.Id);

            b.Property(x => x.ValueName).HasMaxLength(200);
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");

            b.HasIndex(x => new { x.MarketplaceProductId, x.ProductVariantId, x.AttributeId }).IsUnique();

            b.HasOne<MarketplaceProduct>()
             .WithMany() // ayrı koleksiyon ihtiyacın yoksa
             .HasForeignKey(x => x.MarketplaceProductId)
             .OnDelete(DeleteBehavior.Cascade);

            // Variant FK’n var ise:
            // b.HasOne<ProductVariant>().WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
