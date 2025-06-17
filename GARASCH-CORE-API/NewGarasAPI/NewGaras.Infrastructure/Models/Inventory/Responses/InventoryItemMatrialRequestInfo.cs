namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemMatrialRequestInfo
    {
        public long InventoryMatrialRequestOrderID { get; set; }
        public long? FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string UserDept { get; set; }
        public long? RequestTypeId { get; set; }
        public string RequestType { get; set; }
        public int? ToStoreId { get; set; }
        public string ToStore {  get; set; }
        public string CreationDate { get; set; }
        public string RequestDate { get; set; }
        public bool CanCreateRelease { get; set; }
        public bool CanCreatePR { get; set; }
        public string Status { get; set; }

        public List<MatrialRequestInfo> MatrialRequestInfoList { get; set; }
    }
}