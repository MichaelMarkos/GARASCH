using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.PurchesRequest
{
    [DataContract]
    public class AssignPRItemRequest
    {
        long assignTo;
        List<long> purchaseRequestItmesList;


        [DataMember]
        public long AssignTo
        {
            get
            {
                return assignTo;
            }

            set
            {
                assignTo = value;
            }
        }


        [DataMember]
        public List<long> PurchaseRequestItmesList
        {
            get
            {
                return purchaseRequestItmesList;
            }

            set
            {
                purchaseRequestItmesList = value;
            }
        }

    }

}
