namespace NewGaras.Infrastructure.Models.Inventory
{
    public class SalesOfferItemSalesDetails
    {
        public long? InventoryItemId { set; get; }
        public string InventoryItemName { get; set; }
        public long OfferId { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }
        public string SoldDate { get; set; }
        public decimal ItemPrice { get; set; }
    }
}