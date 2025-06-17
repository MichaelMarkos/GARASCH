namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryStoreData
    {
        public int ID { get; set; }
        public string Tel { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool Active { get; set; }
    }
}