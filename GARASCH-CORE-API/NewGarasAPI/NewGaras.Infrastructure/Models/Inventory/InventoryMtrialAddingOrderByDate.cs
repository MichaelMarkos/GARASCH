namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryMtrialAddingOrderByDate
    {
        public string DateMonth { get; set; }
        public List<InventoryMatrialAddingOrderInfo> InventoryMatrialAddingOrderInfoList { get; set; }
    }
}