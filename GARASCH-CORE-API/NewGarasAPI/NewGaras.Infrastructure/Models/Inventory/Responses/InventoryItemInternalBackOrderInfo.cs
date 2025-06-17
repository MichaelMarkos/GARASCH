namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemInternalBackOrderInfo
    {
        public long InventoryMatrialInternalBackOrderID { get; set; }
        public string FromUserName { get; set; }
        public bool Custody {  get; set; }
        public string StoreName { get; set; }
        public string OperationType { get; set; }
        public string RecivingDate { get; set; }
        public string DepartmentName { get; set; }
        public List<InternalBackOrderInfo> InternalBackOrderItemInfoList { get; set; }
    }
}