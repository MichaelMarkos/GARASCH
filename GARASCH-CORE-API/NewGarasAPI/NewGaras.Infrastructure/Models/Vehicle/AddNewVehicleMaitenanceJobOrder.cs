using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.Vehicle.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    [DataContract]
    public class AddNewVehicleMaitenanceJobOrder
    {
        VehicleMaintenanceJobOrderHistoryModel vehicleMaintenanceJobOrder;

        [DataMember]
        public VehicleMaintenanceJobOrderHistoryModel VehicleMaintenanceJobOrder
        {
            get
            {
                return vehicleMaintenanceJobOrder;
            }

            set
            {
                vehicleMaintenanceJobOrder = value;
            }
        }
    }

}
