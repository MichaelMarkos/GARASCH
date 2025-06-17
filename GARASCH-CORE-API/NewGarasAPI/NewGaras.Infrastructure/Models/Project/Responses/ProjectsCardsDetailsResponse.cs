using NewGarasAPI.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Project.Responses
{
    public class ProjectsCardsDetailsResponse
    {
        public List<ProjectCard> ProjectCardsList {  get; set; }
        public string ProjectsStatus { get; set; } //(Open, Closed, Deactivated)
        public string ProjectsType { get; set; } //(ProjectOrders, RentOrders, MaintenanceOrders, WarrantyOrders, InternalOrders)

        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
        public PaginationHeader PaginationHeader { get; set; }
    }
}
