using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.Domain.Entities
{
    public class MarketplaceAttributeMapping
    {
        public Guid Id { get; set; }
        public int Firm { get; set; } // Trendyol
        public int AttributeId { get; set; } // 47,338...
        public string LocalField { get; set; } = ""; // 'Color' | 'Size'
    }
}
