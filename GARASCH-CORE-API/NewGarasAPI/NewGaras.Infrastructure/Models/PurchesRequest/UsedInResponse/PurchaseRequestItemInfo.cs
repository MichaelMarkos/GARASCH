using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class PurchaseRequestItemInfo
    {
        public long PurchaseRequestItemID { get; set; }
        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public decimal ReqQTY { get; set; }
        public string ReqUOM { get; set; }
        public decimal PurchaseQTY { get; set; }
        public string PurchaseUOM { get; set; }
        public string ProjectName { get; set; }
        public string FabOrderNo { get; set; }
        public string Comment { get; set; }
        public string MRItemComment { get; set; }
        public decimal PurchasePOItemRecivedQuantity { get; set; }
        public string RecivedQuantityRUOMShortName { get; set; }
        public long POID { get; set; }
        public decimal PurchaseOrderQTY { get; set; }
        public string SupplierName { get; set; }
        public string PurchaseRequestNotes { get; set; }
        public decimal ConvertRateFromPurchasingToRequestionUnit { get; set; }


        public List<PurchaseOrdersDetails> PurchaseOrdersDetailsList { get; set; }
    }
}
