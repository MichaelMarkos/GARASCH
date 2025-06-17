using NewGaras.Infrastructure.Models.InventoryStoreReports.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports
{
    [DataContract]
    public class UpdateInventoryStoreReportItemRequest
    {
        long? reportID;
        List<InventoryItemPhysicalBalance> itemPhysicalBalanceList;

        [DataMember]
        public List<InventoryItemPhysicalBalance> ItemPhysicalBalanceList
        {
            get
            {
                return itemPhysicalBalanceList;
            }

            set
            {
                itemPhysicalBalanceList = value;
            }
        }

        [DataMember]
        public long? ReportID
        {
            get
            {
                return reportID;
            }

            set
            {
                reportID = value;
            }
        }



    }

}
