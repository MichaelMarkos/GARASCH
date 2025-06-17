using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Responses
{
    [DataContract]
    public class InventoryMatrialPurchaseRequestResponse2
    {
        List<InventoryMatrialPurchaseRequestInfo> inventoryMatrialPurchaseRequestByDateList;
        PaginationHeader paginationHeader;
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
        public List<InventoryMatrialPurchaseRequestInfo> InventoryMatrialPurchaseRequestByDateList
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
        [DataMember]
        public PaginationHeader PaginationHeader
        {
            get
            {
                return paginationHeader;
            }

            set
            {
                paginationHeader = value;
            }
        }




    }

}
