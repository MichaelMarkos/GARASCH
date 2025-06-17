using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory.Requests
{

    public class AddInventoryItemMatrialReleaseFromMatrialRequest
    {
        long? matrialRequestOrderId;
        string status;
        string? transactionDate;




        [DataMember]
        public long? MatrialRequestOrderId
        {
            get
            {
                return matrialRequestOrderId;
            }

            set
            {
                matrialRequestOrderId = value;
            }
        }


        [DataMember]
        public string Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
            }
        }


        [DataMember]
        public string? TransactionDate
        {
            get
            {
                return transactionDate;
            }

            set
            {
                transactionDate = value;
            }
        }

    }

}
