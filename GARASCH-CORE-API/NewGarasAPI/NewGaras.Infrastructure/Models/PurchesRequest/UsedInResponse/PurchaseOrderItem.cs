using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse
{
    public class PurchaseOrderItem
    {
        public long? ID { get; set; }
        public long PurchasePOID { get; set; }
        public long PurchaseRequestItemID { get; set; }
        public long InventoryItemID { get; set; }
        public long InventoryMatrialRequestItemID { get; set; }
        public int UOMID { get; set; }
        public string UOMShortName { get; set; }
        public string FromStoreName { get; set; }

        public long? ProjectID { get; set; }
        public string ProjectName { get; set; }

        public string FabricationOrderNumber { get; set; }
        public long? FabricationOrderID { get; set; }

        public string InventoryItemName { get; set; }
        public string InventoryItemCode { get; set; }



        public string Comment { get; set; }
        public string POType { get; set; }
        public decimal? ConvertRateFromPurchasingToRequestionUnit { get; set; }
        public string EstimatedCost { get; set; }
        public decimal? EstimatedUnitCost { get; set; }
        public int? CurrencyID { get; set; }
        public int StockQTY { get; set; }


        public decimal? RecivedQuantity { get; set; }
        public decimal? RecivedQuantityUOP { get; set; }
        public decimal? PurchasedQuantity { get; set; }
        public string PurchasedUOMShortName { get; set; }
        public decimal? ReqQuantity { get; set; }
        public decimal? RemainQty { get; set; }




        // New 
        public string PRNotes { get; set; }
        public string PRNO { get; set; }
        public string PRItemComment { get; set; }
        public string MRNO { get; set; }

        // New 2023-12-31
        public bool? IsChecked { get; set; }
        public string SupplierInvoiceSerial { get; set; }


        //Mark Shawky 2023/2/12
        public decimal? RateToEgp { get; set; }
        public string PartNumber { get; set; }
        public int? RequstionUOMID { get; set; }
        public string RequstionUOMShortName { get; set; }
        public int? PurchasingUOMID { get; set; }
        public string PurchasingUOMShortName { get; set; }
        public decimal? ActualLocalUnitPrice { get; set; }
        public decimal? ActualUnitPrice { get; set; }
        public decimal? ActualLocalUnitPriceUOR { get; set; }
        public decimal? ActualUnitPriceUOR { get; set; }
        public decimal? TotalLocalActualPrice { get; set; }
        public decimal? TotalActualPriceUOR { get; set; }
        public decimal? FinalLocalUnitCostUOR { get; set; }
        public decimal? FinalUnitCostUOR { get; set; }
        public decimal? TotalFinalLocalUnitCost { get; set; }
        public decimal? TotalFinalUnitCost { get; set; }
        public decimal? TotalActualPrice { get; set; }
        public string InvoiceComments { get; set; }
        public string CurrencyName { get; set; }
        public string ActualUnitPriceUnit { get; set; }
        public long InventoryMaterialRequestId { get; set; }
        public List<long> MaterialAddingOrdersIds { get; set; }
    }


}
