using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetMaintenanceForByIDResponse
    {
        bool result;
        List<Error> errors;
        MaintenanceForDataByID maintenanceForDataByIDObj;



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
        public MaintenanceForDataByID MaintenanceForDataByIDObj
        {
            get
            {
                return maintenanceForDataByIDObj;
            }

            set
            {
                maintenanceForDataByIDObj = value;
            }
        }


    }
}
