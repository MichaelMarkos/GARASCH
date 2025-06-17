using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemSupplierMatrialAddingOrderInfo
    {
        public long InventoryAddingOrderID { get; set; }
        public string SupplierName { get; set; }
        public string OrderType { get; set; }
        public string ToStore {  get; set; }
        public string CreationDate { get; set; }
        public string CreationBy { get; set; }
        public int Revision { get; set; }
        public string RecivingDate { get; set; }
        public List<MatrialAddingOrderInfo> MatrialAddingOrderInfList { get; set; }
    }
}
