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
    public class GetVehicleMaintenanceJobOrderHistoryResponse
    {
        List<VehicleMaintenanceJobOrderHistoryModel> vehicleMaintenanceJobOrderHistories;
        PaginationHeader paginationHeader;

        bool result;
        List<Error> errors;

        [DataMember]
        public List<VehicleMaintenanceJobOrderHistoryModel> VehicleMaintenanceJobOrderHistories
        {
            get
            {
                return vehicleMaintenanceJobOrderHistories;
            }
            set
            {
                vehicleMaintenanceJobOrderHistories = value;
            }
        }
        [DataMember]
        public PaginationHeader PaginationHeader
        {
            get
            {
                return paginationHeader;
            }
            set
            {
                paginationHeader = value;
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
