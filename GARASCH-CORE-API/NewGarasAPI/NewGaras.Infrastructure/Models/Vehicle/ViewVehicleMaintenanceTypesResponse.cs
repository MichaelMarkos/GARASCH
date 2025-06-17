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
    public class ViewVehicleMaintenanceTypesResponse
    {
        VehicleMaintenanceTypes vehicleMaintenanceTypes;

        bool result;
        List<Error> errors;

        [DataMember]
        public VehicleMaintenanceTypes VehicleMaintenanceTypes
        {
            get
            {
                return vehicleMaintenanceTypes;
            }
            set
            {
                vehicleMaintenanceTypes = value;
            }
        }

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
    }

}
