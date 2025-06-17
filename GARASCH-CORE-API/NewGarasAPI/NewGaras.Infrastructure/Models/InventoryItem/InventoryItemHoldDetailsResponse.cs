using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryItem
{
    [DataContract]
    public class InventoryItemHoldDetailsResponse
    {
        List<MatrialRequestItemDetails> matrialRequestItemDetailsList;
        decimal totalRemainHoldQTY;
        List<InventoryStoreItemHoldQTY> inventoryStoreItemHoldQTYList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<MatrialRequestItemDetails> MatrialRequestItemDetailsList
        {
            get
            {
                return matrialRequestItemDetailsList;
            }

            set
            {
                matrialRequestItemDetailsList = value;
            }
        }

        [DataMember]
        public decimal TotalRemainHoldQTY
        {
            get
            {
                return totalRemainHoldQTY;
            }

            set
            {
                totalRemainHoldQTY = value;
            }
        }
        [DataMember]
        public List<InventoryStoreItemHoldQTY> InventoryStoreItemHoldQTYList
        {
            get
            {
                return inventoryStoreItemHoldQTYList;
            }

            set
            {
                inventoryStoreItemHoldQTYList = value;
            }
        }


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


    }

}
