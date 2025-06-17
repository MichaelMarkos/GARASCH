using NewGaras.Infrastructure.Entities;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferProduct
    {
        public long? Id { get; set; }
        public long? ParentOfferProductId { get; set; }
        public long? ProductId { get; set; }
        public long? OfferId { get; set; }
        public int? ProductGroupId { get; set; }
        public double? Quantity { get; set; }
        public double? ReturnedQty { get; set; }
        public double? RemainQty { get; set; }
        public double? ReleasedQty { get; set; }
        public string ReleasedPercentage { get; set; }
        public string ProfitPercentage { get; set; }
        public long? InventoryItemId { get; set; }
        public string InventoryItemName { get; set; }
        public string InventoryItemCode { get; set; }
        public string CommercialName { get; set; }
        public string InventoryItemUOM { get; set; }
        public string InventoryItemPartNO { get; set; }
        public int? InventoryItemCategoryId { get; set; }
        public string InventoryItemCategoryName { get; set; }
        public decimal? ItemPrice { get; set; }
        public string ItemPricingComment { get; set; }
        public double? ConfirmReceivingQuantity { get; set; }
        public string ConfirmReceivingComment { get; set; }
        public long? InvoicePayerClientId { get; set; }
        public string InvoicePayerClientName { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? TaxPercentage { get; set; }
        public decimal? TaxValue { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? FinalPriceWithoutTax { get; set; }
        public decimal? TotalTaxValue { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool? Active { get; set; }

        public List<GetSalesOfferProductTax> SalesOfferProductTaxsList { get; set; }
        public List<Attachment> SalesOfferProductAttachments { get; set; }
    }
}