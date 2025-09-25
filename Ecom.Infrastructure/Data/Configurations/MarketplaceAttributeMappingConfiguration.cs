using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Infrastructure.Data.Configurations
{
    public class MarketplaceAttributeMappingConfiguration : IEntityTypeConfiguration<MarketplaceAttributeMapping>
    {
        public void Configure(EntityTypeBuilder<MarketplaceAttributeMapping> b)
        {
            b.ToTable("MarketplaceAttributeMappings");
            b.HasKey(x => x.Id);

            b.Property(x => x.LocalField).HasMaxLength(50).IsRequired();

            b.HasIndex(x => new { x.Firm, x.AttributeId }).IsUnique();
        }
    }
}
