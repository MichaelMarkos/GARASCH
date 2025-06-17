using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance.UsedInResponse
{
    public class AddSalesOfferProductForMainenance
    {
        public double? Quantity { get; set; }
        public bool? Active { get; set; }
        public long? InventoryItemId { get; set; }
        public int? InventoryItemCategoryId { get; set; }
        public decimal ItemPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public string ItemPricingComment { get; set; }
        public decimal? ProfitPercentage { get; set; }
        public long? ProductId { get; set; }
        public int? ProductGroupId { get; set; }
        public long? InvoicePayerClientId { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? TaxValue { get; set; }
       /* public long? ParentOfferProductId { get; set; }
        public long OfferId { get; set; }
        public double? ReturnedQty { get; set; }
        public double? RemainQty { get; set; }
        public double? ReleasedQty { get; set; }
        public string ReleasedPercentage { get; set; }
        public string InventoryItemName { get; set; }
        public string InventoryItemCode { get; set; }
        public string CommercialName { get; set; }
        public string InventoryItemUOM { get; set; }
        public string InventoryItemPartNO { get; set; }
        public string InventoryItemCategoryName { get; set; }
        public double? ConfirmReceivingQuantity { get; set; }
        public string ConfirmReceivingComment { get; set; }
        public string InvoicePayerClientName { get; set; }
        public decimal? FinalPriceWithoutTax { get; set; }
        public decimal? TotalTaxValue { get; set; }*/


        //"Quantity": 1,
        //    "ActiveActive": true,
        //    "InventoryItemId": "22154",
        //    "InventoryItemCategoryId": "2022",
        //    "ItemPrice": 100,
        //    "FinalPrice": 100,
        //    "DiscountPercentage": 0,
        //    "DiscountValue": 0,
        //    "ItemPricingComment": "",
        //    "ProfitPercentage": "0",
    }
}
