using System.Runtime.Serialization;
using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{
    public class MyProjectsCRMDashboardResponse
    {
        int totalProjects;
        int totalClosed;
        int totalDeactivated;
        int totalOpenInternal;
        int totalClosedInternal;
        decimal totalProjectsPrice;
        decimal totalProjectsCollected;
        string totalCollectedPercentage;

        List<MyProjectsCRM> projectsTypesList;

        List<MaintenanceAndWarrentyCRM> maintenanceAndWarrenty;

        List<SellingProductsCRM> topSellingProducts;
        List<TopSellingProductsCategoryGrouped> topSellingProductsCategoryGrouped;

        bool result;
        List<Error> errors;

        [DataMember]
        public int TotalProjects
        {
            get
            {
                return totalProjects;
            }

            set
            {
                totalProjects = value;
            }
        }

        [DataMember]
        public int TotalClosed
        {
            get
            {
                return totalClosed;
            }

            set
            {
                totalClosed = value;
            }
        }

        [DataMember]
        public int TotalDeactivated
        {
            get
            {
                return totalDeactivated;
            }

            set
            {
                totalDeactivated = value;
            }
        }

        [DataMember]
        public int TotalOpenInternal
        {
            get
            {
                return totalOpenInternal;
            }

            set
            {
                totalOpenInternal = value;
            }
        }

        [DataMember]
        public int TotalClosedInternal
        {
            get
            {
                return totalClosedInternal;
            }

            set
            {
                totalClosedInternal = value;
            }
        }

        [DataMember]
        public decimal TotalProjectsPrice
        {
            get
            {
                return totalProjectsPrice;
            }

            set
            {
                totalProjectsPrice = value;
            }
        }

        [DataMember]
        public decimal TotalProjectsCollected
        {
            get
            {
                return totalProjectsCollected;
            }

            set
            {
                totalProjectsCollected = value;
            }
        }

        [DataMember]
        public string TotalCollectedPercentage
        {
            get
            {
                return totalCollectedPercentage;
            }

            set
            {
                totalCollectedPercentage = value;
            }
        }

        [DataMember]
        public List<MyProjectsCRM> ProjectsTypesList
        {
            get
            {
                return projectsTypesList;
            }

            set
            {
                projectsTypesList = value;
            }
        }

        [DataMember]
        public List<MaintenanceAndWarrentyCRM> MaintenanceAndWarrenty
        {
            get
            {
                return maintenanceAndWarrenty;
            }

            set
            {
                maintenanceAndWarrenty = value;
            }
        }

        [DataMember]
        public List<SellingProductsCRM> TopSellingProducts
        {
            get
            {
                return topSellingProducts;
            }

            set
            {
                topSellingProducts = value;
            }
        }

        [DataMember]
        public List<TopSellingProductsCategoryGrouped> TopSellingProductsCategoryGrouped
        {
            get
            {
                return topSellingProductsCategoryGrouped;
            }

            set
            {
                topSellingProductsCategoryGrouped = value;
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
