using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.Domain.Entities
{
    public class MarketplaceProductAttribute
    {
        public Guid Id { get; set; }
        public Guid MarketplaceProductId { get; set; }
        public int CategoryId { get; set; }
        public int AttributeId { get; set; }
        public string? AttributeName { get; set; }
        public int? ValueId { get; set; }
        public string? ValueName { get; set; }
        public bool Required { get; set; }
        public bool Varianter { get; set; }
        public bool AllowCustom { get; set; }
        public bool Slicer { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
