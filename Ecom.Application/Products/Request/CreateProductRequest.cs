using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.Application.Products.Request
{
    public class CreateProductRequest   // <- sealed YOK
    {
        public string Name { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public string? Barcode { get; set; }
        public string? Brand { get; set; }
        public decimal BasePrice { get; set; }
        public int TaxRate { get; set; } // 0..100
        public bool IsActive { get; set; } = true;
    }

}
