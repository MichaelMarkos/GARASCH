using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class InventoryItemSupplierResponse
    {
        List<InventoryItemSupplier> inventoryItemSupplierList;
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
        public List<InventoryItemSupplier> InventoryItemSupplierList
        {
            get
            {
                return inventoryItemSupplierList;
            }

            set
            {
                inventoryItemSupplierList = value;
            }
        }
    }
}
