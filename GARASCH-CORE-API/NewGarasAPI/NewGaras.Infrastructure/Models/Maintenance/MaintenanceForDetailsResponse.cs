using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceForDetailsResponse
    {
        bool result;
        List<Error> errors;
        List<MaintenanceForDetails> maintenanceForDetailsList;
        PaginationHeader paginationHeader;
        List<MaintenanceProductPerCategory> maintenanceProductPerCategory;
        List<MaintenanceContractPerContractType> maintenanceContractPerContractType;
        int noOFclient;
        decimal totalCost;

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
        public List<MaintenanceForDetails> MaintenanceForDetailsList
        {
            get
            {
                return maintenanceForDetailsList;
            }

            set
            {
                maintenanceForDetailsList = value;
            }
        }
        [DataMember]
        public List<MaintenanceProductPerCategory> MaintenanceProductPerCategory
        {
            get
            {
                return maintenanceProductPerCategory;
            }

            set
            {
                maintenanceProductPerCategory = value;
            }
        }
        [DataMember]
        public List<MaintenanceContractPerContractType> MaintenanceContractPerContractType
        {
            get
            {
                return maintenanceContractPerContractType;
            }

            set
            {
                maintenanceContractPerContractType = value;
            }
        }

        [DataMember]
        public int NoOFclient
        {
            get
            {
                return noOFclient;
            }

            set
            {
                noOFclient = value;
            }
        }
        
        [DataMember]
        public decimal TotalCost
        {
            get
            {
                return totalCost;
            }

            set
            {
                totalCost = value;
            }
        }
    }
}
