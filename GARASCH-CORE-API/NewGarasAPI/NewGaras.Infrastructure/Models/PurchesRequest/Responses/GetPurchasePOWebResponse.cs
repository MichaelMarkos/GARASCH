using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Responses
{
    [DataContract]
    public class GetPurchasePOWebResponse
    {
        List<PurchasePO> purchasePOList;
        PaginationHeader paginationHeader;
        decimal? sumTotalInvoiceCost;
        decimal? sumTotalInvoicePrice;
        bool result;
        List<Error> errors;

        [DataMember]
        public decimal? SumTotalInvoicePrice
        {
            get
            {
                return sumTotalInvoicePrice;
            }

            set
            {
                sumTotalInvoicePrice = value;
            }
        }

        [DataMember]
        public decimal? SumTotalInvoiceCost
        {
            get
            {
                return sumTotalInvoiceCost;
            }

            set
            {
                sumTotalInvoiceCost = value;
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
        public List<PurchasePO> PurchasePOList
        {
            get
            {
                return purchasePOList;
            }

            set
            {
                purchasePOList = value;
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
