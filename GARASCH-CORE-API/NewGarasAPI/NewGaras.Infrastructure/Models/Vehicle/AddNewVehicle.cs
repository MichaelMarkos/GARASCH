using NewGaras.Infrastructure.Models.Vehicle.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    [DataContract]
    public class AddNewVehicle
    {
        ClientVehicle clientVehicle;

        [DataMember]
        public ClientVehicle ClientVehicle
        {
            get
            {
                return clientVehicle;
            }

            set
            {
                clientVehicle = value;
            }
        }
    }

}
