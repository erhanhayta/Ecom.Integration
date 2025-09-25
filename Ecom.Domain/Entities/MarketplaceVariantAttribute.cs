using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.Domain.Entities
{
    public class MarketplaceVariantAttribute
    {
        public Guid Id { get; set; }
        public Guid MarketplaceProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public int AttributeId { get; set; }
        public int? ValueId { get; set; }
        public string? ValueName { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
