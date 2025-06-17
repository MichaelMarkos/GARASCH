namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddInventoryStoreKeeperData
    {
        public int ID { get; set; }
        public int InventoryStoreID { get; set; }
        public int UserID { get; set; }
        public bool Active { get; set; }
    }
}