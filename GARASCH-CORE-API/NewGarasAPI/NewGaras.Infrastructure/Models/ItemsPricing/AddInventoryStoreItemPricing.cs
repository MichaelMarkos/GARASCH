using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ItemsPricing
{
    [DataContract]
    public class AddInventoryStoreItemPricing
    {
        decimal? amount;
        bool? isInc;
        bool? allItem;
        bool? isPercent;
        bool? forCustom;
        bool? forPrice1;
        bool? forPrice2;
        bool? forPrice3;
        // Filters
        int? inventoryItemCategoryID;
        int? inventoryStoreID;
        long? inventoryItemID;
        int? priorityID;
        long? supplierID;
        string searchKey;
        bool? notPricidBefore;



        [DataMember]
        public int? InventoryItemCategoryID
        {
            get
            {
                return inventoryItemCategoryID;
            }

            set
            {
                inventoryItemCategoryID = value;
            }
        }
        [DataMember]
        public int? InventoryStoreID
        {
            get
            {
                return inventoryStoreID;
            }

            set
            {
                inventoryStoreID = value;
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
        public int? PriorityID
        {
            get
            {
                return priorityID;
            }

            set
            {
                priorityID = value;
            }
        }
        [DataMember]
        public long? SupplierID
        {
            get
            {
                return supplierID;
            }

            set
            {
                supplierID = value;
            }
        }
        [DataMember]
        public string SearchKey
        {
            get
            {
                return searchKey;
            }

            set
            {
                searchKey = value;
            }
        }

        [DataMember]
        public bool? NotPricidBefore
        {
            get
            {
                return notPricidBefore;
            }

            set
            {
                notPricidBefore = value;
            }
        }















        [DataMember]
        public decimal? Amount
        {
            get
            {
                return amount;
            }

            set
            {
                amount = value;
            }
        }
        [DataMember]
        public bool? AllItem
        {
            get
            {
                return allItem;
            }

            set
            {
                allItem = value;
            }
        }
        [DataMember]
        public bool? IsInc
        {
            get
            {
                return isInc;
            }

            set
            {
                isInc = value;
            }
        }

        [DataMember]
        public bool? IsPercent
        {
            get
            {
                return isPercent;
            }

            set
            {
                isPercent = value;
            }
        }
        [DataMember]
        public bool? ForPrice1
        {
            get
            {
                return forPrice1;
            }

            set
            {
                forPrice1 = value;
            }
        }

        [DataMember]
        public bool? ForPrice2
        {
            get
            {
                return forPrice2;
            }

            set
            {
                forPrice2 = value;
            }
        }

        [DataMember]
        public bool? ForPrice3
        {
            get
            {
                return forPrice3;
            }

            set
            {
                forPrice3 = value;
            }
        }
        [DataMember]
        public bool? ForCustom
        {
            get
            {
                return forCustom;
            }

            set
            {
                forCustom = value;
            }
        }
    }

}
