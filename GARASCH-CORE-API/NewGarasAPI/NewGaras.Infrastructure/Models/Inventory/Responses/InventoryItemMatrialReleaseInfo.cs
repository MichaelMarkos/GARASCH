using NewGarasAPI.Models.Inventory.Requests;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemMatrialReleaseInfo
    {
        public long? ID {get;set;}
        public long InventoryMatrialRequestOrderID {get;set;}
        public string FromUserName {get;set;}
        public string ToUserName {get;set;}
        public string UserDept {get;set;}
        public string RequestType {get;set;}
        public long? RequestTypeId {get;set;}
        public string RequestDate {get;set;}
        public int FromStoreId {get;set;}
        public string FromStore {get;set;}
        public string CreationDate {get;set;}
        public bool ReNew {get;set;}
        public decimal? TotalRemainQty {get;set;}
        public decimal? TotalRecieved {get;set;}
        public List<MatrialReleaseItemInfo> MatrialReleaseInfoList {get;set;}
    }
}