using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class PrSupplierOfferItem
    {
        public long? Id { get; set; }
        public long PRSupplierOfferId { get; set; }
        public string RequstionUOMShortName { get; set; }
        public int? RequstionUOMId { get; set; }
        public string PurchasingUOMShortName { get; set; }
        public int? PurchasingUOMId { get; set; }
        public decimal? ConversionRate { get; set; }
        public long? PrId { get; set; }
        public long? PoId { get; set; }
        public long? MrItemId { get; set; }
        public long PrItemId { get; set; }
        public long? PoItemId { get; set; }
        public long? InventoryItemId { get; set; }
        public string InventoryItemCode { get; set; }
        public string InventoryItemName { get; set; }
        public string InventoryItemPartNo { get; set; }
        public int UOMId { get; set; }
        public string UOMName { get; set; }
        public string Comment { get; set; }
        public decimal? ReqQuantity { get; set; }
        public decimal? RecivedQuantity { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? TotalEstimatedCost { get; set; }
        public int? CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public decimal? RateToEGP { get; set; }
        public string Status { get; set; }
        public List<PrRejectOfferItem> PrItemRejectedHistory { get; set; }
    }

}
