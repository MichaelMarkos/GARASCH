using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetMaintenanceForByClientResponse
    {
        bool result;
        List<Error> errors;
        List<MaintenanceForDataByClient> maintenanceForDataByClientList;



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

        [DataMember]
        public List<MaintenanceForDataByClient> MaintenanceForDataByClientList
        {
            get
            {
                return maintenanceForDataByClientList;
            }

            set
            {
                maintenanceForDataByClientList = value;
            }
        }


    }
}
