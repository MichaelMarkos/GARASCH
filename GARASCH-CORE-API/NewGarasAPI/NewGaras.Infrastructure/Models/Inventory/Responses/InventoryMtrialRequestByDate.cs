namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryMtrialRequestByDate
    {
        public string DateMonth { get; set; }
        public List<InventoryMatrialRequestInfo> InventoryMatrialRequestInfoList { get; set; }
    }
}