using NewGaras.Infrastructure.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    [DataContract]
    public class GetWorkshopStationResponse
    {
        bool result;
        List<Error> errors;
        List<WorkshopStationResponseData> workshopStationResponseList;



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
        public List<WorkshopStationResponseData> WorkshopStationResponseList
        {
            get
            {
                return workshopStationResponseList;
            }

            set
            {
                workshopStationResponseList = value;
            }
        }


    }
}
