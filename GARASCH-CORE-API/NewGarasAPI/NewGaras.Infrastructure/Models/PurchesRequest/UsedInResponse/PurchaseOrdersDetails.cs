using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class PurchaseOrdersDetails
    {
        public long POID { get; set; }
        public decimal PurchaseOrderQTY { get; set; }
        public string SupplierName { get; set; }
    }
}
