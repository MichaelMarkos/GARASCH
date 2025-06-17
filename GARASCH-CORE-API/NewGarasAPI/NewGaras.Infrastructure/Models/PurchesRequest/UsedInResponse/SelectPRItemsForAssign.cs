using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class SelectPRItemsForAssign
    {
        public long ID {  get; set; }
        public long? PurchaseRequestID { get; set; }
        public long? InventoryMatrialRequestID { get; set; }
        public long PurchaseRequestItemID { get; set; }
        public long? InventoryItemID { get; set; }
        public long InventoryMatrialRequestItemID { get; set; }

        public string InventoryItemName { get; set; }
        public string InventoryItemCode { get; set; }

        public decimal? RecivedQuantity { get; set; }
        public int? RequstionUOMID { get; set; }
        public string RequstionUOMShortName { get; set; }

        public string FabricationOrderNumber { get; set; }
        public long? FabricationOrderID { get; set; }

        public decimal? PurchaseItemQuantity { get; set; }
        public decimal? PRItemQuantityUOP { get; set; }
        public decimal? PurchasedQuantity { get; set; }
        public string PurchasedUOMShortName { get; set; }

        public long? ProjectID { get; set; }
        public string ProjectName { get; set; }

        public decimal? ReqQuantity { get; set; }
        public decimal? RemainQty { get; set; }

        public string Comment { get; set; }
        public string POType { get; set; }
        public decimal? ConvertRateFromPurchasingToRequestionUnit { get; set; }

        // New 
        public string PRNotes { get; set; }
        public string PRNO { get; set; }
        public string MRNO { get; set; }
    }
}
