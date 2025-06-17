namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InternalBackOrderInfo
    {
        public long InventoryItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        // public string ExpDate;
        public string ReqQTY { get; set; }
        public string UOM { get; set; }
        //  public string InventoryItemSerial;
        public string ProjectName { get; set; }
        public string MatrialReleaseName { get; set; }
    }
}