using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports
{

    [DataContract]
    public class UpdateInventoryStoreReportOneItemRequest
    {
        //InventoryItemPhysicalBalance itemPhysicalBalance;
        long? reportItemId;
        decimal? physicalBalance;

        [DataMember]
        public long? ReportItemId
        {
            get
            {
                return reportItemId;
            }

            set
            {
                reportItemId = value;
            }
        }

        [DataMember]
        public decimal? PhysicalBalance
        {
            get
            {
                return physicalBalance;
            }

            set
            {
                physicalBalance = value;
            }
        }
    }

}
