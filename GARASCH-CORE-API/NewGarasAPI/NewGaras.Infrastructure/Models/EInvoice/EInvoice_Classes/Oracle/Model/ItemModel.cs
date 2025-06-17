using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model
{
    [Serializable]
    public class ItemModel
    {
        private string itemTypeEINVOICE;
        private string itemInternalCode;
        private string itemCode;
        private string itemUnitTypeEINVOICE;
        private string itemName;
        private string itemDescription;
        public ItemModel()
        {
            itemTypeEINVOICE = "";
            itemInternalCode = "";
            itemCode = "";
            itemUnitTypeEINVOICE = "";
            itemName = "";
            itemDescription = "";
        }
        public ItemModel(string itemTypeEINVOICE, string itemInternalCode, string itemCode, string itemUnitTypeEINVOICE, string itemName, string itemDescription)
        {
            this.itemTypeEINVOICE = itemTypeEINVOICE;
            this.itemInternalCode = itemInternalCode;
            this.itemCode = itemCode;
            this.itemUnitTypeEINVOICE = itemUnitTypeEINVOICE;
            this.itemName = itemName;
            this.itemDescription = itemDescription;
        }

        public string ItemTypeEINVOICE
        {
            get
            {
                return itemTypeEINVOICE;
            }

            set
            {
                itemTypeEINVOICE = value;
            }
        }



        public string ItemInternalCode
        {
            get
            {
                return itemInternalCode;
            }

            set
            {
                itemInternalCode = value;
            }
        }

        public string ItemCode
        {
            get
            {
                return itemCode;
            }

            set
            {
                itemCode = value;
            }
        }

        public string ItemUnitTypeEINVOICE
        {
            get
            {
                return itemUnitTypeEINVOICE;
            }

            set
            {
                itemUnitTypeEINVOICE = value;
            }
        }



        public string ItemName
        {
            get
            {
                return itemName;
            }

            set
            {
                itemName = value;
            }
        }

        public string ItemDescription
        {
            get
            {
                return itemDescription;
            }

            set
            {
                itemDescription = value;
            }
        }
    }
}
