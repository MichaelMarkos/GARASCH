using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle.UsedInResponse
{
    public class ClientVehicle : Vehicle
    {
        public long ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientLogo { get; set; }
    }
}
