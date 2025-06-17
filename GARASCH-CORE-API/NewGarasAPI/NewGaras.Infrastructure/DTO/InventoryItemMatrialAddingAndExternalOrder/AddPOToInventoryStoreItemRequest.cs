using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.InventoryItemMatrialAddingAndExternalOrder
{
    public class AddPOToInventoryStoreItemRequest
    {
        long? pOID;
        long? inventoryItemID;
        int? currencyID;
        decimal? actualUnitPrice;
        decimal? finalUnitCost;
        decimal? rateToEGP;


        [DataMember]
        public long? POID
        {
            get
            {
                return pOID;
            }

            set
            {
                pOID = value;
            }
        }

        [DataMember]
        public long? InventoryItemID
        {
            get
            {
                return inventoryItemID;
            }

            set
            {
                inventoryItemID = value;
            }
        }

        [DataMember]
        public int? CurrencyID
        {
            get
            {
                return currencyID;
            }

            set
            {
                currencyID = value;
            }
        }


        [DataMember]
        public decimal? ActualUnitPrice
        {
            get
            {
                return actualUnitPrice;
            }

            set
            {
                actualUnitPrice = value;
            }
        }

        [DataMember]
        public decimal? FinalUnitCost
        {
            get
            {
                return finalUnitCost;
            }

            set
            {
                finalUnitCost = value;
            }
        }

        [DataMember]
        public decimal? RateToEGP
        {
            get
            {
                return rateToEGP;
            }

            set
            {
                rateToEGP = value;
            }
        }
    }
}
