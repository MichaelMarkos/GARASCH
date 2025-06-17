namespace NewGaras.Infrastructure.Models.Inventory
{
    public class OpenSalesOfferRequestedItem
    {
        public long SalesOfferId { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }
        public decimal RequestedQty { get; set; }
    }
}