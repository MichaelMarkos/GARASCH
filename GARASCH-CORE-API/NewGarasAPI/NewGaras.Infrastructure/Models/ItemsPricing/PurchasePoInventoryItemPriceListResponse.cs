using NewGaras.Infrastructure.Models.ItemsPricing.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ItemsPricing
{
    [DataContract]
    public class PurchasePoInventoryItemPriceListResponse
    {
        List<PurchasePOInventoryItemPrice> purchasePOInventoryItemPriceList;
        int countOfSupplier;
        int countOfInvoice;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<PurchasePOInventoryItemPrice> PurchasePOInventoryItemPriceList
        {
            get
            {
                return purchasePOInventoryItemPriceList;
            }

            set
            {
                purchasePOInventoryItemPriceList = value;
            }
        }
        [DataMember]
        public int CountOfSupplier
        {
            get
            {
                return countOfSupplier;
            }

            set
            {
                countOfSupplier = value;
            }
        }
        [DataMember]
        public int CountOfInvoice
        {
            get
            {
                return countOfInvoice;
            }

            set
            {
                countOfInvoice = value;
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
