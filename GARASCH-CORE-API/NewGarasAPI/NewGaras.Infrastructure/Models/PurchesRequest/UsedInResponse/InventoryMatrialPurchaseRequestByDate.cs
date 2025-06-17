using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class InventoryMatrialPurchaseRequestByDate
    {
        public string DateMonth {  get; set; }
        public List<InventoryMatrialPurchaseRequestInfo> InventoryMatrialPurchaseRequestInfoList { get; set; }
    }
}
