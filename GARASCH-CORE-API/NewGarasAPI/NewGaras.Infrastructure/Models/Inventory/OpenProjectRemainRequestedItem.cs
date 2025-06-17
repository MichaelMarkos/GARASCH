namespace NewGaras.Infrastructure.Models.Inventory
{
    public class OpenProjectRemainRequestedItem
    {
        public long ProjectId { get; set; }
        public long? ClientId { get; set; }
        public string ClientName { get; set; }
        public decimal RemainRequestedQty { get; set; }
    }
}