using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class MatrialReleaseItemWithRequestItem
    {
        public long ID { get; set; }
        public long? InventoryItemID { get; set; }
        public string Comment { get; set; }
        public string NewComment { get; set; }
        public decimal? NewRecivedQTY { get; set; }
        public bool? IsFIFO { get; set; }
        public bool Active { get; set; }

        // for LIBMARK frpm (comments) in adding order Item to matrial request Item (Comment) for release from inventoystoreitem (OrderNumber) 
        public int? PublicId { get; set; }
    }
}
