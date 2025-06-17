using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Responses
{
    public class InventoryItemSupplierMatrialAddingOrderInfoResponse
    {
        InventoryItemSupplierMatrialAddingOrderInfo inventoryItemInfo;
        bool result;
        List<Error> errors;

        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        [DataMember]
        public InventoryItemSupplierMatrialAddingOrderInfo InventoryItemInfo
        {
            get
            {
                return inventoryItemInfo;
            }

            set
            {
                inventoryItemInfo = value;
            }
        }
    }
}
