using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class ClientContactsData
    {
        long clientId;
        List<ClientAddressData> clientAddresses;
        List<ClientLandLineData> clientLandLines;
        List<ClientMobileData> clientMobiles;
        List<ClientFaxData> clientFaxes;

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
        public List<ClientAddressData> ClientAddresses
        {
            get
            {
                return clientAddresses;
            }

            set
            {
                clientAddresses = value;
            }
        }

        [DataMember]
        public List<ClientLandLineData> ClientLandLines
        {
            get
            {
                return clientLandLines;
            }

            set
            {
                clientLandLines = value;
            }
        }

        [DataMember]
        public List<ClientMobileData> ClientMobiles
        {
            get
            {
                return clientMobiles;
            }

            set
            {
                clientMobiles = value;
            }
        }

        [DataMember]
        public List<ClientFaxData> ClientFaxes
        {
            get
            {
                return clientFaxes;
            }

            set
            {
                clientFaxes = value;
            }
        }
    }
}
