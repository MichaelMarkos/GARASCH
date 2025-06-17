using NewGaras.Infrastructure.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    [DataContract]
    public class GetStationReceivedWorkOrdersResponse
    {
        bool result;

        List<Error> errors;
        List<WorkshopStationForList> workshopStationForDataList;

        List<ReceivedFabticationOrder> receivedFabticationOrderDataList;
        public WorkshopStationForList WorkshopStation { get; set; }
        PaginationHeader paginationHeader;
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
        public List<ReceivedFabticationOrder> ReceivedFabticationOrderDataList
        {
            get
            {
                return receivedFabticationOrderDataList;
            }

            set
            {
                receivedFabticationOrderDataList = value;
            }
        }
        public List<WorkshopStationForList> WorkshopStationForDataList
        {
            get
            {
                return workshopStationForDataList;
            }

            set
            {
                workshopStationForDataList = value;
            }
        }
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
