using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class MatrialRequestItem
    {
        public long InventoryItemID { get; set; }
        public decimal ReqQTY { get; set; }
        public string Comment { get; set; }

        public long? PojectID { get; set; }
        public long? FabOrderID { get; set; }

        public long? FabOrderItemID { get; set; }
        public bool? FromBOM { get; set; }
        public long? OfferItemID { get; set; }
        public List<long> InventoryStoreItemIDsList { get; set; }

        // MatrialRequestItemID for Release Hold 
        public long? MatrialRequestItemID;
    }
}
