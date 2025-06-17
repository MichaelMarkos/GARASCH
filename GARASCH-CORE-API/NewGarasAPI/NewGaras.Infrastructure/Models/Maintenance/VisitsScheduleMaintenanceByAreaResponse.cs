using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class VisitsScheduleMaintenanceByAreaResponse
    {
        List<MaintenanceAreaVM> mainAreaList;
        bool result;
        List<Error> errors;
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
        public List<MaintenanceAreaVM> MainAreaList
        {
            get
            {
                return mainAreaList;
            }

            set
            {
                mainAreaList = value;
            }
        }

    }
}
