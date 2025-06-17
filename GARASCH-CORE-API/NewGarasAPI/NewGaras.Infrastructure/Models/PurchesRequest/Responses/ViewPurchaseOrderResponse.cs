using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Responses
{
    [DataContract]
    public class ViewPurchaseOrderResponse
    {
        long pONumber;
        string pOTypeName;
        long supplierId;
        string supplierName;
        string requestDate;
        string creationDate;
        string status;
        string accountantApprovalStatus;
        int pOApprovedCount;
        int pOApprovedRequiredCount;
        decimal pOInvoiceAmount;
        decimal pOPaidAmount;
        List<PurchaseOrderItem> purchasePOItemList;
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
        public List<PurchaseOrderItem> PurchasePOItemList
        {
            get
            {
                return purchasePOItemList;
            }

            set
            {
                purchasePOItemList = value;
            }
        }
        [DataMember]
        public string AccountantApprovalStatus
        {
            get
            {
                return accountantApprovalStatus;
            }

            set
            {
                accountantApprovalStatus = value;
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
        public string CreationDate
        {
            get
            {
                return creationDate;
            }

            set
            {
                creationDate = value;
            }
        }

        [DataMember]
        public string RequestDate
        {
            get
            {
                return requestDate;
            }

            set
            {
                requestDate = value;
            }
        }
        [DataMember]
        public string SupplierName
        {
            get
            {
                return supplierName;
            }

            set
            {
                supplierName = value;
            }
        }

        [DataMember]
        public long SupplierId
        {
            get
            {
                return supplierId;
            }

            set
            {
                supplierId = value;
            }
        }
        [DataMember]
        public string POTypeName
        {
            get
            {
                return pOTypeName;
            }

            set
            {
                pOTypeName = value;
            }
        }
        [DataMember]
        public long PONumber
        {
            get
            {
                return pONumber;
            }

            set
            {
                pONumber = value;
            }
        }

        [DataMember]
        public int POApprovedCount
        {
            get
            {
                return pOApprovedCount;
            }

            set
            {
                pOApprovedCount = value;
            }
        }

        [DataMember]
        public int POApprovedRequiredCount
        {
            get
            {
                return pOApprovedRequiredCount;
            }

            set
            {
                pOApprovedRequiredCount = value;
            }
        }


        [DataMember]
        public decimal POInvoiceAmount
        {
            get
            {
                return pOInvoiceAmount;
            }

            set
            {
                pOInvoiceAmount = value;
            }
        }

        [DataMember]
        public decimal POPaidAmount
        {
            get
            {
                return pOPaidAmount;
            }

            set
            {
                pOPaidAmount = value;
            }
        }
    }


}
