using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Infrastructure.Data.Configurations
{
    public class MarketplaceProductAttributeConfiguration : IEntityTypeConfiguration<MarketplaceProductAttribute>
    {
        public void Configure(EntityTypeBuilder<MarketplaceProductAttribute> b)
        {
            b.ToTable("MarketplaceProductAttributes");
            b.HasKey(x => x.Id);

            b.Property(x => x.AttributeName).HasMaxLength(200);
            b.Property(x => x.ValueName).HasMaxLength(200);
            b.Property(x => x.CreatedAtUtc).HasDefaultValueSql("SYSUTCDATETIME()");
            b.Property(x => x.CategoryId).IsRequired();
            b.HasIndex(x => new { x.MarketplaceProductId, x.AttributeId });

            b.HasOne<MarketplaceProduct>()
             .WithMany(x => x.Attributes)
             .HasForeignKey(x => x.MarketplaceProductId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
