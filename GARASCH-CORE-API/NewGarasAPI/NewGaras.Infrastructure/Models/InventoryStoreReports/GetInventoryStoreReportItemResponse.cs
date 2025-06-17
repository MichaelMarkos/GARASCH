using NewGaras.Infrastructure.Models.InventoryStoreReports.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.InventoryStoreReports
{
    [DataContract]
    public class GetInventoryStoreReportItemResponse
    {
        List<InventoryStoreReportItem> dataLList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<InventoryStoreReportItem> DataLList
        {
            get
            {
                return dataLList;
            }

            set
            {
                dataLList = value;
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
