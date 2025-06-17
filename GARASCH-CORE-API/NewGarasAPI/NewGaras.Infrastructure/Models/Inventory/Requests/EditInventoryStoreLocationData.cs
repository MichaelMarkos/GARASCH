namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class EditInventoryStoreLocationData
    {
        public int ID { get; set; }
        public int InventoryStoreID { get; set; }
        public string Location { get; set; }
        public bool Active { get; set; }
    }
}