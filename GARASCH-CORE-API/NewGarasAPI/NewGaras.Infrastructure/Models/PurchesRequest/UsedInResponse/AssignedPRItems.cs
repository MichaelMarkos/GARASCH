using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class AssignedPRItems
    {
        public long? PurchaseRequestID { get; set; }
        public long PurchaseRequestItemID { get; set; }
        public long InventoryMatrialRequestItemID { get; set; }
        public long? AssginedUserID { get; set; }
        public string AssginedUserImage { get; set; }
        public string AssginedUserName { get; set; }
        public long? InventoryItemID { get; set; }
        public string InventoryItemName { get; set; }
        public string InventoryItemCode { get; set; }
        public decimal? RecivedQuantity { get; set; }
        public string InventoryUOMShortName { get; set; }
        public string FabricationOrderNumber { get; set; }
        public decimal? PurchaseRequestItemQuantity { get; set; }
        public decimal? PurchaseRequestQuantity { get; set; }
        public decimal? RequestRecivedQuantity { get; set; }
        public decimal? PurchaseItemQuantity { get; set; }
        public string PurchasedUOMShortName { get; set; }
        public string RequstionUOMShortName { get; set; }
        public string ProjectName { get; set; }
        public decimal? ConvertRateFromPurchasingToRequestionUnit { get; set; }

    }
}

