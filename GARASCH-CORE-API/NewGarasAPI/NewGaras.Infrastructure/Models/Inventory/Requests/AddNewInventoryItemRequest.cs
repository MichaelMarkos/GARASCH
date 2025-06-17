using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{
    public class AddNewInventoryItemRequest
    {
        InventoryItemInfoForInsert data;

        [DataMember]
        public InventoryItemInfoForInsert Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }
    }
}
