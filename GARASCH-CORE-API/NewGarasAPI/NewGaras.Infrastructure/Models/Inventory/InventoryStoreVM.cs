using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryStoreVM
    {
        public int ID { get; set; }
        public int InventoryStoreID { get; set; }
        public int InvenoryStoreLocationID { get; set; }
        public int InventoryItemID { get; set; }
        public string Code { get; set; }
        public int? CurrencyId { get; set; }
        public string InventoryStoreName { get; set; }
        public decimal? ProfitValue { get; set; }
        public decimal? FinalPRrofite { get; set; }
        public decimal FinalUnitPriceEGPIncludingCost { get; set; }
        public decimal? FinalUnitPriceEGPIncludingCostWithProfit { get; set; }
        public string CurrencyName { get; set; }
        public string InvenoryStoreLocationName { get; set; }
        public string InventoryItemName { get; set; }
        public string CreationDate { get; set; }
        public string OperationType { get; set; }
        public string ExpDate { get; set; }
        public string ItemSerial { get; set; }
        public string FinalBalance { get; set; }
        public string AddingFromPOId { get; set; }
        public string POInvoiceId { get; set; }
        public string POInvoiceTotalPrice { get; set; }
        public string POInvoiceTotalCost { get; set; }
        public string RateToEGP { get; set; }
        public string Unit { get; set; }
        public string Average { get; set; }
        public decimal? RemainBalancePricewithMainCu { get; set; }
        public decimal? RemainBalanceCostwithMainCu { get; set; }
        public decimal? RemainBalancePricewithEgp { get; set; }
        public decimal? RemainBalanceCostwithEgp { get; set; }
    }
}
