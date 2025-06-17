using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class GetInventoryStoreResponse
    {
        bool result;
        List<Error> errors;
        List<InventoryStoreData> inventoryItemCategoryList;




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
        public List<InventoryStoreData> InventoryStoreList
        {
            get
            {
                return inventoryItemCategoryList;
            }

            set
            {
                inventoryItemCategoryList = value;
            }
        }
    }
}
