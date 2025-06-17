using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientTaxCardData
    {
        long clientId;
        string taxCard;

        [DataMember]
        public long ClientId
        {
            get
            {
                return clientId;
            }

            set
            {
                clientId = value;
            }
        }

        [DataMember]
        public string TaxCard
        {
            get
            {
                return taxCard;
            }

            set
            {
                taxCard = value;
            }
        }
    }

}
