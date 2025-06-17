using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Inventory
{
    public class AccountAndFinanceInventoryItemMovementResponse
    {
        List<ItemMovement> itemMovementList;
        PaginationHeader paginationHeader;
        bool result;
        List<Error> errors;
        string dateFrom;
        string dateTo;
        double releaseQty;
        double noOfMonth;
        double releaseRate;


        [DataMember]
        public string DateFrom
        {
            get
            {
                return dateFrom;
            }

            set
            {
                dateFrom = value;
            }
        }
        [DataMember]
        public string DateTo
        {
            get
            {
                return dateTo;
            }

            set
            {
                dateTo = value;
            }
        }

        [DataMember]
        public double ReleaseQty
        {
            get
            {
                return releaseQty;
            }

            set
            {
                releaseQty = value;
            }
        }
        [DataMember]
        public double NoOfMonth
        {
            get
            {
                return noOfMonth;
            }

            set
            {
                noOfMonth = value;
            }
        }
        [DataMember]
        public double ReleaseRate
        {
            get
            {
                return releaseRate;
            }

            set
            {
                releaseRate = value;
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
        public List<ItemMovement> InventoryItemMovementList
        {
            get
            {
                return itemMovementList;
            }

            set
            {
                itemMovementList = value;
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
