using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.Project.Headers;
using NewGaras.Infrastructure.Models.Project.Responses;
using NewGarasAPI.Models.Project.Headers;
using NewGarasAPI.Models.Project.Responses;
using NewGarasAPI.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IProjectService
    {
        public string GetProjectName(long id);
        public ProjectCard GetProjectCard(VProjectSalesOffer project);
        public ProjectsCardsDetailsResponse GetFullProjectsCardsDetails(FullProjectsCardsFilters filters);
        public GetWorkshopStationResponse GetWorkshopStationsList();

        public GetWorkshopStationsItemResponse GetWorkshopStationsItemPerId(int WorkShopID);

        public GetTeamUsersSelectResponse GetTeamUsersSelect(string SearchKey);

        public BaseResponseWithID DeleteWorkshopStation(WorkShopStationData Request, long UserID);
        public BaseResponseWithID DeleteProjectWorkshopStation(ProjectWorkShopStationData Request, long UserID);

        public BaseResponseWithID AddEditProjectContactPerson(ProjectContactPersonData Request, long UserID);
        public GetStationReceivedWorkOrdersResponse GetStationReceivedWorkOrders(int? StationID, int CurrentPage = 1, int NumberOfItemsPerPage = 10);
        public BaseResponseWithID AddNewJobOrderStations(JobOrderStationsData Request, long UserID);

        public Task<MyProjectsCRMDashboardResponse> GetMyProjectsDetailsCRM([FromHeader] GetMyProjectsDetailsCRMHeaders headers);
    }
}
