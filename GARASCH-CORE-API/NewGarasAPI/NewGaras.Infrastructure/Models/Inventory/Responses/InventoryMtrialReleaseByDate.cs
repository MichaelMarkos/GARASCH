namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryMtrialReleaseByDate
    {
        public string DateMonth { get; set; }
        public List<InventoryMatrialReleaseInfo> InventoryMatrialReleaseInfoList { get; set; }
    }
}