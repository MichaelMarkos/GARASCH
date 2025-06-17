using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    public class transferRequest
    {
        public long clientId { get; set; }

        public long vehiclePerClient { get; set; }

        [DataMember]
        public long ClientId
        {
            set { clientId = value; }
            get { return clientId; }
        }
        [DataMember]
        public long VehiclePerClient
        {
            set { vehiclePerClient = value; }
            get { return vehiclePerClient; }
        }
    }


}
