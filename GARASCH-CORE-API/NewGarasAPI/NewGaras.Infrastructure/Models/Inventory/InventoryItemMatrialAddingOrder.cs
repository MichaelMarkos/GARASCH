using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemMatrialAddingOrder
    {
        List<InventoryMtrialAddingOrderByDate> inventoryMtrialAddingOrderByDateList;
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
        public List<InventoryMtrialAddingOrderByDate> InventoryMtrialAddingOrderByDateList
        {
            get
            {
                return inventoryMtrialAddingOrderByDateList;
            }

            set
            {
                inventoryMtrialAddingOrderByDateList = value;
            }
        }
    }
}
