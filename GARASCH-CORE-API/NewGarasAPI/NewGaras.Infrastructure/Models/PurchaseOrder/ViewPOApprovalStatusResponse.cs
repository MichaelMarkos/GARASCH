using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchaseOrder
{
    [DataContract]
    public class ViewPOApprovalStatusResponse
    {
        List<POApprovalStatus> pOApprovalStatusList;
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
        public List<POApprovalStatus> POApprovalStatusList
        {
            get
            {
                return pOApprovalStatusList;
            }

            set
            {
                pOApprovalStatusList = value;
            }
        }
    }

}
