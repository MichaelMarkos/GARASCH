using System.Runtime.Serialization;
using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{

    public class ProjectsTypesDetailsResponse
    {
        List<ProjectsSummary> projectsDetails;
        string projectsStatus; //(Open, Closed, Deactivated)     cheack for duplication in ProjectsSummry
        decimal projectCost;
        decimal rentCost;
        decimal maintenanceCost;
        decimal internalCost;
        decimal jobCost;
        decimal warantycost;
        bool result;
        List<Error> errors;

        [DataMember]
        public decimal ProjectCost
        {
            set { projectCost = value; }
            get { return projectCost; }
        }
        [DataMember]
        public decimal RentCost
        {
            set { rentCost = value; }
            get { return rentCost; }
        }
        [DataMember]
        public decimal MaintenanceCost
        {
            set { maintenanceCost = value; }
            get { return maintenanceCost; }
        }
        [DataMember]
        public decimal InternalCost
        {
            set { internalCost = value; }
            get { return internalCost; }
        }
        [DataMember]
        public decimal JobCost
        {
            set { jobCost = value; }
            get { return jobCost; }
        }
        [DataMember]
        public decimal Warantycost
        {
            set { warantycost = value; }
            get { return warantycost; }
        }

        [DataMember]
        public List<ProjectsSummary> ProjectsDetails
        {
            get
            {
                return projectsDetails;
            }

            set
            {
                projectsDetails = value;
            }
        }
        [DataMember]
        public string ProjectsStatus
        {
            get
            {
                return projectsStatus;
            }

            set
            {
                projectsStatus = value;
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
