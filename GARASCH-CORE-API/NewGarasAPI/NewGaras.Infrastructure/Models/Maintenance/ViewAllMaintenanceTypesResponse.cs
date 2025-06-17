using NewGaras.Infrastructure.Models.Maintenance.UsedInResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class ViewAllMaintenanceTypesResponse
    {
        GetMaintenanceTypes allMaintenanceTypes;

        bool result;
        List<Error> errors;

        [DataMember]
        public GetMaintenanceTypes AllMaintenanceTypes
        {
            get
            {
                return allMaintenanceTypes;
            }
            set
            {
                allMaintenanceTypes = value;
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
