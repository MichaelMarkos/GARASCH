using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest.Responses
{
    [DataContract]
    public class ManageAssignedPRItemsResponse
    {
        List<AssignedPRItems> assignedPRItemsList;
        bool result;
        List<Error> errors;

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
        public List<AssignedPRItems> AssignedPRItemsList
        {
            get
            {
                return assignedPRItemsList;
            }

            set
            {
                assignedPRItemsList = value;
            }
        }
    }
}

