using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class InventoryItemRejectedOfferSupplierResponse
    {
        List<ItemRejectedOfferSupplier> itemRejectedOfferSupplierList;
        List<ItemAcceptedOfferSupplier> itemAcceptedOfferSupplierList;
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
        public List<ItemRejectedOfferSupplier> ItemRejectedOfferSupplierList
        {
            get
            {
                return itemRejectedOfferSupplierList;
            }

            set
            {
                itemRejectedOfferSupplierList = value;
            }
        }

        [DataMember]
        public List<ItemAcceptedOfferSupplier> ItemAcceptedOfferSupplierList
        {
            get
            {
                return itemAcceptedOfferSupplierList;
            }

            set
            {
                itemAcceptedOfferSupplierList = value;
            }
        }
    }
}
