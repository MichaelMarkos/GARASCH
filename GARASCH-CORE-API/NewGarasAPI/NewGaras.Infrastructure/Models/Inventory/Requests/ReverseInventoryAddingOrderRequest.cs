using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class ReverseInventoryAddingOrderRequest
    {
        public long AddingOrderId { get; set; }
        public bool IsReverse { get; set; }
    }
}
