using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetManagementOfMaintenanceOrder
    {
        bool result;
        List<Error> errors;
        ManagementOfMaintenanceOrderData managementOfMaintenanceOrderDataObj;
        List<ManagementOfMaintenanceOrderData> managementOfMaintenanceOrderDataList;



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
        public ManagementOfMaintenanceOrderData ManagementOfMaintenanceOrderDataObj
        {
            get
            {
                return managementOfMaintenanceOrderDataObj;
            }

            set
            {
                managementOfMaintenanceOrderDataObj = value;
            }
        }


        [DataMember]
        public List<ManagementOfMaintenanceOrderData> ManagementOfMaintenanceOrderDataList
        {
            get
            {
                return managementOfMaintenanceOrderDataList;
            }

            set
            {
                managementOfMaintenanceOrderDataList = value;
            }
        }
    }
}
