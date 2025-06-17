using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class MatrialReleasePDFFilters
    {
        [FromHeader]
        public long SpecificProjectID { get; set; }
        [FromHeader]
        public long ProjectID { get; set; }
        [FromHeader]
        public long MatrialReleaseOrderID { get; set; }
        [FromHeader]
        public bool? QtyDetails { get; set; }
        [FromHeader]
        public bool? FabOrder { get; set; }
        [FromHeader]
        public bool? Serial { get; set; }
        [FromHeader]
        public bool? Comment { get; set; }
        [FromHeader]
        public string ShippingMethod { get; set; }
        [FromHeader]
        public string ContactMobile { get; set; }
        [FromHeader]
        public string ContactName { get; set; }
        [FromHeader]
        public string MainAddress { get; set; }
        [FromHeader]
        public bool IsEnglish { get; set; }

    }
}
