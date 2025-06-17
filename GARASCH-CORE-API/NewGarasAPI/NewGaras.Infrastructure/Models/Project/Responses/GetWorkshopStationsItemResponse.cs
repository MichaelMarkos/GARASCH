using NewGaras.Infrastructure.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    [DataContract]
    public class GetWorkshopStationsItemResponse
    {
        bool result;
        List<Error> errors;
        WorkshopStationsItemResponseData workshopStationsItemResponseData;



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
        public WorkshopStationsItemResponseData WorkshopStationsItemResponseData
        {
            get
            {
                return workshopStationsItemResponseData;
            }

            set
            {
                workshopStationsItemResponseData = value;
            }
        }


    }
}
