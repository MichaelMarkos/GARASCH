using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddInventoryItemMatrialReleasePrintInfoRequest
    {
        public List<InventoryMatrialReleasePrintInfoVM> MatrialReleasePrintInfoList { get; set; }
        public long? MatrialReleaseOrderId {  get; set; }
    }
}
