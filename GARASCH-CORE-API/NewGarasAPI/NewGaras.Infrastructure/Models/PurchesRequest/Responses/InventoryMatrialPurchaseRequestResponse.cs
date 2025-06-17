using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Responses
{
    [DataContract]
    public class InventoryMatrialPurchaseRequestResponse
    {
        List<InventoryMatrialPurchaseRequestByDate> inventoryMatrialPurchaseRequestByDateList;
        long totalCounter;
        bool result;
        List<Error> errors;

        [DataMember]
        public long TotalCounter
        {
            get
            {
                return totalCounter;
            }

            set
            {
                totalCounter = value;
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

        [DataMember]
        public List<InventoryMatrialPurchaseRequestByDate> InventoryMatrialPurchaseRequestByDateList
        {
            get
            {
                return inventoryMatrialPurchaseRequestByDateList;
            }

            set
            {
                inventoryMatrialPurchaseRequestByDateList = value;
            }
        }




    }

}
