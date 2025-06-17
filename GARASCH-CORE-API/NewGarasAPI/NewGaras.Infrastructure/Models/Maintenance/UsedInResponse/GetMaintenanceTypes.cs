using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance.UsedInResponse
{
    public class GetMaintenanceTypes
    {
        public List<MaintenanceType> maintenanceTypeList { get; set; }

        [DataMember]
        public List<MaintenanceType> MaintenanceTypesList
        {
            get { return maintenanceTypeList; }
            set { maintenanceTypeList = value; }
        }
    }
}
