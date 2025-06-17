using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse
{
    public class MatrialRequestItemDetails
    {
        public long MatrialRequestItemID { get; set; }
        public long MatrialRequestID { get; set; }
        public decimal RemainHoldQTY { get; set; }
        public string MatrialRequestNo { get; set; }
        public string ClientName { get; set; }
        public long? ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectSerial { get; set; }

    }
}
