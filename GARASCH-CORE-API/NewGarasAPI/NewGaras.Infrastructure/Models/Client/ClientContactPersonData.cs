using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientContactPersonData
    {
        long clientId;
        List<ClientContactPersonDataResponse> clientContactPersons;


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
        public List<ClientContactPersonDataResponse> ClientContactPersons
        {
            get
            {
                return clientContactPersons;
            }

            set
            {
                clientContactPersons = value;
            }
        }
    }
}
