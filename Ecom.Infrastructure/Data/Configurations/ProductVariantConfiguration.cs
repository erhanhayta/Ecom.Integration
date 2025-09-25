using Ecom.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecom.Infrastructure.Data.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> b)
        {
            b.ToTable("ProductVariants");
            b.HasKey(x => x.Id);

            b.Property(x => x.Color).HasMaxLength(100);
            b.Property(x => x.Size).HasMaxLength(50);
            b.Property(x => x.Sku).HasMaxLength(100);
            b.Property(x => x.Barcode).HasMaxLength(100);

            b.HasOne(x => x.Product)
             .WithMany(p => p.Variants)
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
