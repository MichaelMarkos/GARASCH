using NewGaras.Infrastructure.Models.Vehicle.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Vehicle
{
    [DataContract]
    public class GetClinetVehiclesData
    {
        List<ClientVehicle> clientVehicles;
        PaginationHeader paginationHeader;

        bool result;
        List<Error> errors;



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
        public List<ClientVehicle> ClientVehicles
        {
            get
            {
                return clientVehicles;
            }

            set
            {
                clientVehicles = value;
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
