using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.Project.UsedInResponses;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Project.Responses
{
    [DataContract]
    public class ProjectsMiniCardsDetailsResponse
    {
        List<MiniProjectCard> projectCardsList;

        string projectsStatus; //(Open, Closed, Deactivated)
        string projectsType; //(ProjectOrders, RentOrders, MaintenanceOrders, WarrantyOrders, InternalOrders)
        PaginationHeader paginationHeader;

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
        public List<MiniProjectCard> ProjectCardsList
        {
            get
            {
                return projectCardsList;
            }

            set
            {
                projectCardsList = value;
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
        public string ProjectsType
        {
            get
            {
                return projectsType;
            }

            set
            {
                projectsType = value;
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
    }
}
