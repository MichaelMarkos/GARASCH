using NewGaras.Infrastructure.Models.InventoryAddingOrder;
using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    public class InventoryStoreItemByOrderResponse
    {
        public decimal TotalBalance { get; set; }
        public int TotalCountOfItem { get; set; }
        public List<InventoryStoreItemByOrderData> InventoryStoreItems { get; set; }
        public long OrderID { get; set; }
        public long CreatorId { get; set; }
        public string CreatorName { get; set; }
        public long SupplierId { get; set; }
        public string SupplierName { get; set; }
        public long StoreId { get; set; }
        public string StoreName { get; set; }
        public string Type { get; set; }
        public int Revision { get; set; }
        public string CreationDate { get; set; }
        public string RecivingDate { get; set; }

        //---------------InventoryInternalTransferOrder-------------------
        public int FromInventoryStoreID { get; set; }
        public string FromInventoryStoreName { get; set; }
        public int ToInventoryStoreID { get; set; }
        public string ToInventoryStoreName { get; set; }

        //-------------------InventoryMatrialRelease----------------------
        public long ToUserID { get; set; }
        public string ToUserName { get; set; }
        public long MatrialRequestID { get; set; }
        public string RequestDate { get; set; }
        public string Status { get; set; }
    }
}
