using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.User;
using System.Net;
using System.Web;
using NewGarasAPI.Helper;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using NewGarasAPI.Models.Project.Responses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using NewGarasAPI.Models.Project.Headers;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGarasAPI.Models.AccountAndFinance;
using NewGarasAPI.Models.Account;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using NewGarasAPI.Models.HR;
using NewGaras.Infrastructure.DBContext;
using System.Collections.Generic;
using Error = NewGarasAPI.Models.Common.Error;
using DocumentFormat.OpenXml.Spreadsheet;
using NewGaras.Infrastructure;
using NewGaras.Domain.Models;
using Azure;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.Project.Headers;
using NewGaras.Infrastructure.Models.Project.Responses;



namespace NewGarasAPI.Controllers.Project
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITaskService _taskService;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        private readonly ISprintService _sprintService;
        private readonly ITenantService _tenantService;
        private readonly IProjectService _projectService;
        public ProjectController(IWebHostEnvironment host,ITaskService taskService,ISprintService sprintService,ITenantService tenantService,ITaskMangerProjectService taskMangerProjectService,IProjectService projectService)

        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
            _taskService = taskService;
            _sprintService = sprintService;
            _taskMangerProjectService = taskMangerProjectService;
            _projectService = projectService;
        }

        [HttpGet("GetProjectTypeList")]
        public SelectDDLResponse GetProjectTypeList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    DDLList.Add(new SelectDDL { ID = 1, Name = "New Project" });
                    DDLList.Add(new SelectDDL { ID = 2, Name = "Maintenance" });
                    DDLList.Add(new SelectDDL { ID = 3, Name = "Rent" });
                    DDLList.Add(new SelectDDL { ID = 4, Name = "Internal Project" });
                    DDLList.Add(new SelectDDL { ID = 5, Name = "Warranty" });
                }
                Response.DDLList = DDLList;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetProjectsStatistics")]
        public ProjectsTypesDetailsResponse GetProjectsStatistics([FromHeader] GetProjectsStatisticsHeaders headers)
        {
            ProjectsTypesDetailsResponse Response = new ProjectsTypesDetailsResponse
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                #region User Authentication Token 
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion


                #region dealing with headers the old way
                //long ProjectId = 0;
                //if (!string.IsNullOrEmpty(headers["ProjectId"]) && long.TryParse(headers["ProjectId"], out ProjectId))
                //{
                //    long.TryParse(headers["ProjectId"], out ProjectId);
                //}

                //long ClientId = 0;
                //if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                //{
                //    long.TryParse(headers["ClientId"], out ClientId);
                //}

                //string ProjectOfferTypesFilters = "";
                //if (!string.IsNullOrEmpty(headers["ProjectOfferTypesFilters"]))
                //{
                //    ProjectOfferTypesFilters = HttpUtility.UrlDecode(headers["ProjectOfferTypesFilters"]);
                //}

                //string MaintenanceTypesFilters = "";
                //if (!string.IsNullOrEmpty(headers["MaintenanceTypesFilters"]))
                //{
                //    MaintenanceTypesFilters = HttpUtility.UrlDecode(headers["MaintenanceTypesFilters"]);
                //}

                //string Location = "";
                //if (!string.IsNullOrEmpty(headers["Location"]))
                //{
                //    Location = HttpUtility.UrlDecode(headers["Location"]);
                //}

                //long SalesPersonBranchId = 0;
                //if (!string.IsNullOrEmpty(headers["SalesPersonBranchId"]) && long.TryParse(headers["SalesPersonBranchId"], out SalesPersonBranchId))
                //{
                //    long.TryParse(headers["SalesPersonBranchId"], out SalesPersonBranchId);
                //}

                //long SalesPersonId = 0;
                //if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                //{
                //    long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                //}
                #endregion

                #region dealing with headers
                DateTime StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                bool filterFrom = false;
                if (headers.ProjectCreationFrom != null)
                {
                    DateTime ProjectCreateFrom = DateTime.Now;
                    if (!DateTime.TryParse(headers.ProjectCreationFrom, out ProjectCreateFrom))
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-12";
                        error.ErrorMSG = "Invalid Project Creation From";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;
                    }
                    StartDate = ProjectCreateFrom.Date;
                    //StartDate = headers.ProjectCreationFrom;
                    filterFrom = true;
                }

                if (headers.ProjectId != 0 || headers.ClientId != 0 || headers.MaintenanceTypesFilters != null || headers.SalesPersonBranchId != 0 || headers.ProjectOfferTypesFilters != null || headers.Location != null || headers.SalesPersonId != 0)
                {
                    StartDate = new DateTime(2000, 1, 1);
                }

                var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                bool filterTo = false;
                if (headers.ProjectCreationTo != null)
                {
                    DateTime ProjectCreateTo = DateTime.Now;
                    if (!DateTime.TryParse(headers.ProjectCreationTo, out ProjectCreateTo))
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Invalid Project Creation To";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;
                    }
                    EndDate = ProjectCreateTo.Date.AddTicks(-1);
                    filterTo = true;
                }
                #endregion

                List<ProjectsSummary> ProjectsStatistics = new List<ProjectsSummary>();

                if (Response.Result)
                {

                    var projects = _Context.VProjectSalesOffers.AsQueryable();



                    #region Filters
                    if (headers.Year != 0)
                    {
                        if (headers.Month > 0 && headers.Month <= 12)
                        {
                            StartDate = new DateTime(headers.Year, headers.Month, 1);
                            EndDate = new DateTime(headers.Year, headers.Month + 1, 1);
                        }
                        else
                        {
                            StartDate = new DateTime(headers.Year, 1, 1);
                            EndDate = new DateTime(headers.Year + 1, 1, 1);
                        }

                    }

                    projects = projects.Where(a => a.CreationDate >= StartDate && a.CreationDate <= EndDate).AsQueryable();
                    if (headers.ProjectId != 0)
                    {
                        projects = projects.Where(a => a.Id == headers.ProjectId).AsQueryable();
                    }
                    if (headers.ClientId != 0)
                    {
                        projects = projects.Where(a => a.ClientId == headers.ClientId).AsQueryable();
                    }
                    if (headers.SalesPersonBranchId != 0)
                    {
                        projects = projects.Where(a => a.SalesPersonBranchId == headers.SalesPersonBranchId).AsQueryable();
                    }
                    if (headers.SalesPersonId != 0)
                    {
                        projects = projects.Where(p => p.SalesPersonId == headers.SalesPersonId);
                    }
                    if (headers.Location != "" && headers.Location != null)
                    {
                        projects = projects.Where(a => a.ProjectLocation == headers.Location).AsQueryable();
                    }
                    if (headers.MaintenanceTypesFilters != "" && headers.MaintenanceTypesFilters != null)
                    {
                        var MaintenanceTypesFiltersList = headers.MaintenanceTypesFilters.Split(',').ToList();
                        projects = projects.Where(a => MaintenanceTypesFiltersList.Contains(a.Type)).AsQueryable();
                    }
                    if (headers.ProjectOfferTypesFilters != "" && headers.ProjectOfferTypesFilters != null)
                    {
                        var ProjectOfferTypesFiltersList = headers.ProjectOfferTypesFilters.Split(',').ToList();
                        projects = projects.Where(a => ProjectOfferTypesFiltersList.Contains(a.OfferType)).AsQueryable();
                    }
                    #endregion

                    var projectsList = projects.ToList();

                    var Groupedprojects = projectsList.GroupBy(p => p.OfferType).ToList();

                    //Get All Warrenty Projects
                    var WarrantyProjects = projectsList.Where(p => p.OfferType == "New Maintenance Offer" && p.MaintenanceType == "Warranty").ToList();
                    var WarrantyProjectsGrp = WarrantyProjects.GroupBy(a => a.MaintenanceType).ToList();
                    Groupedprojects.Add(WarrantyProjectsGrp.FirstOrDefault());

                    decimal projectCost = 0; decimal rentCost = 0; decimal maintenanceCost = 0; decimal internalCost = 0; decimal jobCost = 0; decimal warantycost = 0;

                    foreach (var projectsGrp in Groupedprojects)
                    {
                        var stats = new ProjectSummaryTotalcost();
                        if (projectsGrp != null)
                        {
                            var totalcost = projectsGrp.Select(p => p.ExtraCost + p.FinalOfferPrice).Sum();
                            var ProjectReturn = _Context.VProjectReturnSalesOffers.Where(a => a.ParentOfferType == projectsGrp.Key).ToList();
                            if (ProjectReturn != null)
                            {
                                var ProjectReturnCost = ProjectReturn.Select(a => a.ReturnFinalOfferPrice).Sum();
                                totalcost -= (decimal)ProjectReturnCost;
                            }
                            if (projectsGrp.Key == "New Project Offer") projectCost = (decimal)totalcost;
                            else if (projectsGrp.Key == "New Maintenance Offer") maintenanceCost = (decimal)totalcost;
                            else if (projectsGrp.Key == "New Rent Offer") rentCost = (decimal)totalcost;
                            else if (projectsGrp.Key == "New Internal Order") internalCost = (decimal)totalcost;
                            else if (projectsGrp.Key == "New Job Order") jobCost = (decimal)totalcost;
                            else if (projectsGrp.Key == "Warranty") warantycost = (decimal)totalcost;

                            var OpenProjectsDetails = new ProjectsSummary();
                            var ClosedProjectsDetails = new ProjectsSummary();
                            var DeactivatedProjectsDetails = new ProjectsSummary();

                            OpenProjectsDetails.ProjectsType = projectsGrp.Key;
                            ClosedProjectsDetails.ProjectsType = projectsGrp.Key;
                            DeactivatedProjectsDetails.ProjectsType = projectsGrp.Key;

                            var projectsListQuery = projectsGrp.AsQueryable();

                            var openProjectsCount = projectsListQuery.Where(a => a.Closed == false && a.Active == true).Count();
                            OpenProjectsDetails.ProjectsStatus = "Open";//---------------------------------------------------
                            OpenProjectsDetails.CountOfProjects = openProjectsCount;

                            var closedProjectsCount = projectsListQuery.Where(a => a.Closed == true && a.Active == true).Count();
                            ClosedProjectsDetails.ProjectsStatus = "Closed";
                            ClosedProjectsDetails.CountOfProjects = closedProjectsCount;

                            var deactivatedProjectsCount = projectsListQuery.Where(a => a.Active == false).Count();
                            DeactivatedProjectsDetails.ProjectsStatus = "Deactivated";
                            DeactivatedProjectsDetails.CountOfProjects = deactivatedProjectsCount;

                            ProjectsStatistics.Add(OpenProjectsDetails);
                            ProjectsStatistics.Add(ClosedProjectsDetails);
                            ProjectsStatistics.Add(DeactivatedProjectsDetails);
                        }
                    }

                    //Adding Direct Sales Projects to New Project Offer Projects
                    var OpenNewProjectOffer = ProjectsStatistics.Where(a => a.ProjectsType == "New Project Offer" && a.ProjectsStatus == "Open").FirstOrDefault();
                    var OpenDirectSales = ProjectsStatistics.Where(a => a.ProjectsType == "Direct Sales" && a.ProjectsStatus == "Open").FirstOrDefault();
                    if (OpenDirectSales != null)
                    {
                        if (OpenNewProjectOffer != null)
                        {
                            OpenNewProjectOffer.CountOfProjects += OpenDirectSales.CountOfProjects;
                            ProjectsStatistics.Remove(OpenDirectSales);
                        }
                        else
                        {
                            OpenDirectSales.ProjectsType = "New Project Offer";
                        }
                    }

                    var ClosedNewProjectOffer = ProjectsStatistics.Where(a => a.ProjectsType == "New Project Offer" && a.ProjectsStatus == "Closed").FirstOrDefault();
                    var ClosedDirectSales = ProjectsStatistics.Where(a => a.ProjectsType == "Direct Sales" && a.ProjectsStatus == "Closed").FirstOrDefault();
                    if (ClosedDirectSales != null)
                    {
                        if (ClosedNewProjectOffer != null)
                        {
                            ClosedNewProjectOffer.CountOfProjects += ClosedDirectSales.CountOfProjects;
                            ProjectsStatistics.Remove(ClosedDirectSales);
                        }
                        else
                        {
                            ClosedDirectSales.ProjectsType = "New Project Offer";
                        }
                    }

                    var DeactivatedNewProjectOffer = ProjectsStatistics.Where(a => a.ProjectsType == "New Project Offer" && a.ProjectsStatus == "Deactivated").FirstOrDefault();
                    var DeactivatedDirectSales = ProjectsStatistics.Where(a => a.ProjectsType == "Direct Sales" && a.ProjectsStatus == "Deactivated").FirstOrDefault();
                    if (DeactivatedDirectSales != null)
                    {
                        if (DeactivatedNewProjectOffer != null)
                        {
                            DeactivatedNewProjectOffer.CountOfProjects += DeactivatedDirectSales.CountOfProjects;
                            ProjectsStatistics.Remove(DeactivatedDirectSales);
                        }
                        else
                        {
                            DeactivatedDirectSales.ProjectsType = "New Project Offer";
                        }
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Project Offer" && a.ProjectsStatus == "Open").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Project Offer";
                        projectsDetails.ProjectsStatus = "Open";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Project Offer" && a.ProjectsStatus == "Closed").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Project Offer";
                        projectsDetails.ProjectsStatus = "Closed";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Project Offer" && a.ProjectsStatus == "Deactivated").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Project Offer";
                        projectsDetails.ProjectsStatus = "Deactivated";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Maintenance Offer" && a.ProjectsStatus == "Open").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Maintenance Offer";
                        projectsDetails.ProjectsStatus = "Open";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Maintenance Offer" && a.ProjectsStatus == "Closed").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Maintenance Offer";
                        projectsDetails.ProjectsStatus = "Closed";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Maintenance Offer" && a.ProjectsStatus == "Deactivated").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Maintenance Offer";
                        projectsDetails.ProjectsStatus = "Deactivated";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Rent Offer" && a.ProjectsStatus == "Open").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Rent Offer";
                        projectsDetails.ProjectsStatus = "Open";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Rent Offer" && a.ProjectsStatus == "Closed").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Rent Offer";
                        projectsDetails.ProjectsStatus = "Closed";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Rent Offer" && a.ProjectsStatus == "Deactivated").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Rent Offer";
                        projectsDetails.ProjectsStatus = "Deactivated";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "Warranty" && a.ProjectsStatus == "Open").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Warranty";
                        projectsDetails.ProjectsStatus = "Open";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "Warranty" && a.ProjectsStatus == "Closed").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Warranty";
                        projectsDetails.ProjectsStatus = "Closed";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "Warranty" && a.ProjectsStatus == "Deactivated").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Warranty";
                        projectsDetails.ProjectsStatus = "Deactivated";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Internal Order" && a.ProjectsStatus == "Open").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Internal Order";
                        projectsDetails.ProjectsStatus = "Open";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Internal Order" && a.ProjectsStatus == "Closed").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Internal Order";
                        projectsDetails.ProjectsStatus = "Closed";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "New Internal Order" && a.ProjectsStatus == "Deactivated").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Internal Order";
                        projectsDetails.ProjectsStatus = "Deactivated";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "Sales Return" && a.ProjectsStatus == "Open").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Sales Return";
                        projectsDetails.ProjectsStatus = "Open";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "Sales Return" && a.ProjectsStatus == "Closed").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Sales Return";
                        projectsDetails.ProjectsStatus = "Closed";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    if (ProjectsStatistics.Where(a => a.ProjectsType == "Sales Return" && a.ProjectsStatus == "Deactivated").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Sales Return";
                        projectsDetails.ProjectsStatus = "Deactivated";

                        ProjectsStatistics.Add(projectsDetails);
                    }

                    Response.ProjectsDetails = ProjectsStatistics;

                    Response.ProjectCost = projectCost;
                    Response.RentCost = rentCost;
                    Response.MaintenanceCost = maintenanceCost;
                    Response.InternalCost = internalCost;
                    Response.JobCost = jobCost;
                    Response.Warantycost = warantycost;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetProjectsCardsDetails")]
        public ProjectsMiniCardsDetailsResponse GetProjectsCardsDetails([FromHeader] GetProjectsCardsDetailsHeaders headers)
        {
            ProjectsMiniCardsDetailsResponse Response = new ProjectsMiniCardsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region User Authentication Token 
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion

                //var ProjectsDetailsList = new List<ProjectsSummary>();

                if (Response.Result)
                {
                    #region dealing with headers the old way
                    //string RequestOfferType = "";
                    //if (!string.IsNullOrEmpty(headers["OfferType"]))
                    //{
                    //    RequestOfferType = headers["OfferType"];
                    //}

                    //string RequestProjectsStatus = "";
                    //if (!string.IsNullOrEmpty(headers["ProjectsStatus"]))
                    //{
                    //    RequestProjectsStatus = headers["ProjectsStatus"].ToLower();
                    //}

                    //int CurrentPage = 1;
                    //if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    //{
                    //    int.TryParse(headers["CurrentPage"], out CurrentPage);
                    //}

                    //int NumberOfItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    //{
                    //    int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    //}

                    //long ProjectId = 0;
                    //if (!string.IsNullOrEmpty(headers["ProjectId"]) && long.TryParse(headers["ProjectId"], out ProjectId))
                    //{
                    //    long.TryParse(headers["ProjectId"], out ProjectId);
                    //}

                    //long ClientId = 0;
                    //if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                    //{
                    //    long.TryParse(headers["ClientId"], out ClientId);
                    //}

                    //int BranchId = 0;
                    //if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    //{
                    //    int.TryParse(headers["BranchId"], out BranchId);
                    //}

                    //int Month = 0;
                    //if (!string.IsNullOrEmpty(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                    //{
                    //    int.TryParse(headers["Month"], out Month);
                    //}

                    //int Year = 0;
                    //if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                    //{
                    //    int.TryParse(headers["Year"], out Year);
                    //}

                    //bool SortByRemainCollections = false;

                    //if (!string.IsNullOrEmpty(headers["SortByRemainCollections"]))
                    //{
                    //    if (bool.Parse(headers["SortByRemainCollections"]) != true && bool.Parse(headers["SortByRemainCollections"]) != false)
                    //    {
                    //        Response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err110";
                    //        error.ErrorMSG = "SortByRemainCollections must be true of false ";
                    //        Response.Errors.Add(error);
                    //    }
                    //    else
                    //    {
                    //        SortByRemainCollections = bool.Parse(headers["SortByRemainCollections"]);
                    //    }
                    //}

                    //bool SortByRemainType = false;

                    //if (!string.IsNullOrEmpty(headers["SortByRemainType"]))
                    //{
                    //    if (bool.Parse(headers["SortByRemainType"]) != true && bool.Parse(headers["SortByRemainType"]) != false)
                    //    {
                    //        Response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err110";
                    //        error.ErrorMSG = "SortByRemainType must be true of false ";
                    //        Response.Errors.Add(error);
                    //    }
                    //    else
                    //    {
                    //        SortByRemainType = bool.Parse(headers["SortByRemainType"]);
                    //    }
                    //}

                    //---------------------------------------- New Added Filters -------------------------------------------------
                    //string ProjectOfferTypesFilters = "";
                    //if (!string.IsNullOrEmpty(headers["ProjectOfferTypesFilters"]))
                    //{
                    //    ProjectOfferTypesFilters = HttpUtility.UrlDecode(headers["ProjectOfferTypesFilters"]);
                    //}

                    //string MaintenanceTypesFilters = "";
                    //if (!string.IsNullOrEmpty(headers["MaintenanceTypesFilters"]))
                    //{
                    //    MaintenanceTypesFilters = HttpUtility.UrlDecode(headers["MaintenanceTypesFilters"]);
                    //}

                    //string Location = "";
                    //if (!string.IsNullOrEmpty(headers["Location"]))
                    //{
                    //    Location = HttpUtility.UrlDecode(headers["Location"]);
                    //}

                    //long SalesPersonBranchId = 0;
                    //if (!string.IsNullOrEmpty(headers["SalesPersonBranchId"]) && long.TryParse(headers["SalesPersonBranchId"], out SalesPersonBranchId))
                    //{
                    //    long.TryParse(headers["SalesPersonBranchId"], out SalesPersonBranchId);
                    //}


                    //---------------------------------------- End new Add  filters -----------------------------------------
                    //long ProjectManagerId = 0;
                    //if (!string.IsNullOrEmpty(headers["ProjectManagerId"]) && long.TryParse(headers["ProjectManagerId"], out ProjectManagerId))
                    //{
                    //    long.TryParse(headers["ProjectManagerId"], out ProjectManagerId);
                    //}

                    //long SalesPersonId = 0;
                    //if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                    //{
                    //    long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    //}

                    //------------------------------------------more new added filters (used in MVC only)---------------------------------------
                    #endregion


                    DateTime StartDateInHeader = new DateTime(DateTime.Now.Year, 1, 1);
                    bool filterFrom = false;
                    if (headers.ProjectCreationFrom != null)
                    {
                        DateTime ProjectCreateFrom = DateTime.Now;
                        if (!DateTime.TryParse(headers.ProjectCreationFrom, out ProjectCreateFrom))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Project Creation From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        StartDateInHeader = ProjectCreateFrom.Date;
                        filterFrom = true;
                    }

                    if (headers.ProjectId != 0 || headers.ClientId != 0 || headers.MaintenanceTypesFilters != null || headers.SalesPersonBranchId != 0 || headers.ProjectOfferTypesFilters != null || headers.Location != null || headers.SalesPersonId != 0)
                    {
                        StartDateInHeader = new DateTime(2000, 1, 1);
                    }

                    DateTime EndDateInHeader = DateTime.Now;
                    bool filterTo = false;
                    if (headers.ProjectCreationTo != null)
                    {
                        DateTime ProjectCreateTo = DateTime.Now;
                        if (!DateTime.TryParse(headers.ProjectCreationTo, out ProjectCreateTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Invalid Project Creation To";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        EndDateInHeader = ProjectCreateTo.Date;
                        filterTo = true;
                    }


                    //------------------------------------------End new add filters ----------------------------------------




                    if (Response.Result)
                    {
                        if (headers.OfferType != null)
                        {
                            var projectsQuery = _Context.VProjectSalesOffers.AsQueryable();                //RequestProjectsStatus
                            //var projectsQuery = _Context.V_Project_SalesOffer.AsQueryable();
                            if (headers.ProjectId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.Id == headers.ProjectId);
                            }
                            if (headers.ClientId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.ClientId == headers.ClientId);
                            }
                            if (headers.ProjectManagerId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.ProjectManagerId == headers.ProjectManagerId);
                            }
                            if (headers.SalesPersonId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.SalesPersonId == headers.SalesPersonId);
                            }
                            if (headers.BranchId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.SalesPersonBranchId == headers.BranchId);
                            }
                            //---------------------------------------new add filters -------------------------------
                            if (headers.SalesPersonBranchId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.SalesPersonBranchId == headers.SalesPersonBranchId).AsQueryable();
                            }

                            //if (headers.ProjectCreationFrom != null || headers.ProjectCreationFrom != "")
                            //{
                            //    projectsQuery = projectsQuery.Where(a => a.CreationDate >= StartDateInHeader).AsQueryable();            //used in MVC only
                            //}
                            if (filterTo)
                            {
                                projectsQuery = projectsQuery.Where(a => a.CreationDate <= EndDateInHeader).AsQueryable();               //used in MVC only
                            }

                            if (headers.ProjectOfferTypesFilters != "" && headers.ProjectOfferTypesFilters != null)
                            {
                                var ProjectOfferTypesFiltersList = headers.ProjectOfferTypesFilters.Split(',').ToList();
                                projectsQuery = projectsQuery.Where(a => ProjectOfferTypesFiltersList.Contains(a.OfferType)).AsQueryable();
                            }

                            if (headers.MaintenanceTypesFilters != "" && headers.MaintenanceTypesFilters != null)
                            {
                                var MaintenanceTypesFiltersList = headers.MaintenanceTypesFilters.Split(',').ToList();
                                projectsQuery = projectsQuery.Where(a => MaintenanceTypesFiltersList.Contains(a.Type)).AsQueryable();
                            }
                            //---------------------------------------End new added filters -------------------------
                            if (headers.OfferType == "Warrenty")
                            {
                                projectsQuery = projectsQuery.Where(a => a.OfferType == "New Maintenance Offer" && a.MaintenanceType == "Warranty");
                            }
                            else if (headers.OfferType == "New Project Offer")
                            {
                                projectsQuery = projectsQuery.Where(a => a.OfferType == headers.OfferType || a.OfferType == "Direct Sales");
                            }
                            else
                            {
                                projectsQuery = projectsQuery.Where(a => a.OfferType == headers.OfferType);
                            }

                            Expression<Func<VProjectSalesOffer, bool>> whereClause;
                            switch (headers.ProjectsStatus)
                            {
                                case "open":
                                    whereClause = a => a.Closed == false && a.Active == true;
                                    break;
                                case "closed":
                                    whereClause = a => a.Closed == true && a.Active == true;
                                    break;
                                case "deactivated":
                                    whereClause = a => a.Active == false;
                                    break;
                                default:
                                    if (headers.ProjectsStatus != "")
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err111";
                                        error.ErrorMSG = "Invalid Project Status must be in (open, closed, deactivated)";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }

                                    whereClause = a => a.Active == true;
                                    break;
                            }

                            if (headers.Year > 0 && (headers.ProjectCreationFrom == null || headers.ProjectCreationFrom == ""))
                            {
                                var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                                var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);

                                if (headers.Month > 0)
                                {
                                    StartDate = new DateTime(headers.Year, headers.Month, 1);

                                    if (headers.Month != 12)
                                    {
                                        EndDate = new DateTime(headers.Year, headers.Month + 1, 1);
                                    }
                                    else
                                    {
                                        EndDate = new DateTime(headers.Year + 1, 1, 1);
                                    }
                                }
                                if (headers.Year != 0 && headers.Month == 0)
                                {
                                    StartDate = new DateTime(headers.Year, 1, 1);
                                    EndDate = new DateTime(headers.Year + 1, 1, 1);
                                }

                                projectsQuery = projectsQuery.Where(a => a.CreationDate.Date >= StartDate.Date && a.CreationDate.Date < EndDate.Date);

                            }
                            else if (headers.Year > 0 && (headers.ProjectCreationFrom != null || headers.ProjectCreationFrom != ""))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err111";
                                error.ErrorMSG = "You can not choose year and creation date at the same Time!";
                                Response.Errors.Add(error);

                                return Response;
                            }
                            else
                            {
                                if (headers.Month > 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                                    Response.Errors.Add(error);

                                    return Response;
                                }
                            }
                            if (headers.Year == 0 && (headers.ProjectCreationFrom != null || headers.ProjectCreationFrom != ""))
                            {
                                projectsQuery = projectsQuery.Where(a => a.CreationDate >= StartDateInHeader).AsQueryable();
                            };

                            var testList = projectsQuery.ToList();

                            projectsQuery = projectsQuery.Where(whereClause).OrderByDescending(a => a.CreationDate);

                            // var ProjectsList = new List<V_Project_SalesOffer>();
                            var ProjectsList = PagedList<VProjectSalesOffer>.Create(projectsQuery, headers.CurrentPage, headers.NumberOfItemsPerPage);

                            //if (SortByRemainCollections || SortByRemainType)
                            //{
                            //    ProjectsList = projectsQuery.ToList();
                            //}
                            //else
                            //{

                            //}


                            List<MiniProjectCard> ProjectsCardsList = new List<MiniProjectCard>();


                            // Modifications in 2024-1-9 (Michael)
                            // Prepare Lists query before loop
                            var IDSClientsList = ProjectsList.Select(x => x.ClientId).ToList();
                            var ClientsList = _Context.Clients.Where(x => IDSClientsList.Contains(x.Id));

                            var IDSProjectMangersList = ProjectsList.Select(x => x.ProjectManagerId).ToList();
                            var IDSSalesPersonsList = ProjectsList.Select(x => x.SalesPersonId).ToList();
                            var UsersList = _Context.Users.Where(x => IDSProjectMangersList.Contains(x.Id) || IDSSalesPersonsList.Contains(x.Id));

                            var IDSSalesoffer = ProjectsList.Select(x => x.SalesOfferId).ToList();
                            var ListOfOfferTaxs = _Context.SalesOfferInvoiceTaxes.Where(x => IDSSalesoffer.Contains(x.SalesOfferId) && x.Active == true).ToList();

                            var ListSalesOfferExtraCosts = _Context.SalesOfferExtraCosts.Where(x => IDSSalesoffer.Contains(x.SalesOfferId) && x.Active == true).ToList();

                            foreach (var project in ProjectsList)
                            {
                                MiniProjectCard ProjectCard = new MiniProjectCard();

                                ProjectCard.ProjectId = project.Id;
                                ProjectCard.ProjectName = project.ProjectName;
                                ProjectCard.ProjectSerial = project.ProjectSerial;
                                ProjectCard.projectType = project.OfferType;
                                ProjectCard.projectStatus = headers.ProjectsStatus;                     //was RequestProjectsStatus -> headers.ProjectsStatus

                                var client = ClientsList.Where(c => c.Id == project.ClientId).FirstOrDefault();
                                if (client != null)
                                {
                                    ProjectCard.ClientName = client.Name;
                                }

                                //var projectManager = _Context.proc_UserLoadByPrimaryKey(project.ProjectManagerID).FirstOrDefault();
                                var projectManager = UsersList.Where(x => x.Id == project.ProjectManagerId).FirstOrDefault();
                                //if (projectManager != null)
                                //{
                                //    ProjectCard.ProjectManagerName = projectManager.FirstName + " " + projectManager.MiddleName + " " + projectManager.LastName;

                                //    if (projectManager.Photo != null)
                                //    {
                                //        ProjectCard.ProjectManagerImgUrl = baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(projectManager.ID.ToString(), key)) + "&type=photo&CompName=" + headers["CompanyName"].ToString().ToLower();
                                //    }
                                //}

                                //var salesPerson = _Context.proc_UserLoadByPrimaryKey(project.SalesPersonID).FirstOrDefault();
                                var salesPerson = UsersList.Where(x => x.Id == project.SalesPersonId).FirstOrDefault();
                                if (salesPerson != null)
                                {
                                    ProjectCard.ProjectSalesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;

                                    if (salesPerson.Photo != null)
                                    {
                                        ProjectCard.ProjectSalesPersonImgUrl = Globals.baseURL + Common.GetUserPhoto(salesPerson.Id, _Context);
                                        /* "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(salesPerson.Id.ToString(), key)) + "&type=photo&CompName=" + validation.CompanyName.ToString().ToLower(); */      //companyName may be return NULL
                                    }
                                }
                                // Modifications in 2024-1-9 (Michael)

                                //decimal TotalProjectCost = 0;
                                //if (project.ExtraCost == null)
                                //{
                                //    project.ExtraCost = 0;
                                //}
                                //if (project.FinalOfferPrice == null)
                                //{
                                //    project.FinalOfferPrice = 0;
                                //}
                                //TotalProjectCost = (decimal)(project.ExtraCost + project.FinalOfferPrice);
                                //decimal TotalProjectCollectedCost = 0;

                                //var clientAccounts = _Context.ClientAccounts.Where(x => x.ProjectID == project.ID);
                                //foreach (var clientAccount in clientAccounts)
                                //{
                                //    if (clientAccount.AmountSign == "plus")
                                //    {
                                //        TotalProjectCollectedCost = TotalProjectCollectedCost + clientAccount.Amount;
                                //    }
                                //    else if (clientAccount.AmountSign == "minus")
                                //    {
                                //        TotalProjectCollectedCost = TotalProjectCollectedCost - clientAccount.Amount;
                                //    }
                                //}
                                //var RemainCost = TotalProjectCost - TotalProjectCollectedCost;
                                //ProjectCard.RemainCollection = RemainCost;

                                //ProjectCard.ProjectStartDate = project.StartDate.ToShortDateString();
                                //ProjectCard.ProjectEndDate = project.EndDate.ToShortDateString();
                                //ProjectCard.RemainTime = (int)(project.EndDate - DateTime.Now).TotalDays;





                                // Modifications in 2024-1-9 (Michael)


                                //var offerTaxDB = _Context.proc_SalesOfferInvoiceTaxLoadAll().Where(a => a.SalesOfferID == project.SalesOfferID && a.Active == true).ToList();
                                //var offerTaxDB = ListOfOfferTaxs.Where(a => a.SalesOfferID == project.SalesOfferID && a.Active == true).ToList();
                                //decimal TotalOfferTax = 0;
                                //foreach (var offerTax in offerTaxDB)
                                //{
                                //    TotalOfferTax += offerTax.TaxValue;
                                //}
                                //ProjectCard.OfferTax = TotalOfferTax;


                                //var offerExtraCostsDB = _Context.proc_SalesOfferExtraCostsLoadAll().Where(a => a.SalesOfferID == project.SalesOfferID && a.Active == true).ToList();
                                //decimal TotalOfferExtraCosts = 0;
                                //foreach (var offerExtraCost in offerExtraCostsDB)
                                //{
                                //    TotalOfferExtraCosts += offerExtraCost.Amount;
                                //}
                                //ProjectCard.OfferExtraCost = _Context.SalesOfferExtraCosts.Where(a => a.SalesOfferID == project.SalesOfferID && a.Active == true).Select(x=>x.Amount).DefaultIfEmpty(0).Sum();

                                //------------------------------------------------------------------
                                var OfferPrice = project.OfferAmount ?? 0;
                                //var list = ListOfOfferTaxs.ToList();
                                var OfferTax = ListOfOfferTaxs.Where(a => a.SalesOfferId == project.SalesOfferId).Select(x => x.TaxValue).DefaultIfEmpty(0).Sum();
                                //var OfferTax = ListOfOfferTaxs.Where(a => a.SalesOfferID == project.SalesOfferID).Select(x => x.TaxValue).DefaultIfEmpty(0).Sum();
                                var OfferExtraCost = ListSalesOfferExtraCosts.Where(a => a.SalesOfferId == project.SalesOfferId).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                                // TotalOfferExtraCosts;
                                ProjectCard.ProjectExtraModifications = project.ExtraCost ?? 0;

                                decimal TotalProjectCost = OfferPrice + OfferTax + OfferExtraCost + ProjectCard.ProjectExtraModifications;
                                ProjectCard.TotalProjectPrice = TotalProjectCost;
                                //------------------------------------------------------------------------------
                                ProjectsCardsList.Add(ProjectCard);
                            }

                            //if (SortByRemainCollections)
                            //{
                            //    ProjectsCardsList.OrderBy(a => a.RemainCollection).ToList();
                            //}
                            //if (SortByRemainType)
                            //{
                            //    ProjectsCardsList.OrderBy(a => a.RemainTime).ToList();
                            //}

                            Response.ProjectCardsList = ProjectsCardsList;
                            //Response.ProjectsStatus = RequestProjectsStatus;
                            //Response.ProjectsType = RequestOfferType;

                            Response.PaginationHeader = new PaginationHeader
                            {
                                CurrentPage = headers.CurrentPage,
                                TotalPages = ProjectsList.TotalPages,
                                ItemsPerPage = headers.NumberOfItemsPerPage,
                                TotalItems = ProjectsList.TotalCount
                            };

                            //var context = WebOperationContext.Current;
                            //HttpContext.Response.Headers.AddPaginationHeader(headers.CurrentPage, ProjectsList.TotalPages, headers.NumberOfItemsPerPage, ProjectsList.TotalCount);

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrProject1";
                            error.ErrorMSG = "No Selected Offer Type";
                            Response.Errors.Add(error);
                        }
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetFullProjectCardDetails")]
        public FullProjectCardDetailsResponse GetFullProjectCardDetails([FromHeader] long ProjectId)
        {
            FullProjectCardDetailsResponse Response = new FullProjectCardDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                #region User Authentication Token 
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion

                var ProjectsDetailsList = new List<ProjectsSummary>();

                if (Response.Result)
                {

                    //long ProjectId = 0;
                    //if (!string.IsNullOrEmpty(headers["ProjectId"]) && long.TryParse(headers["ProjectId"], out ProjectId))
                    //{
                    //    long.TryParse(headers["ProjectId"], out ProjectId);
                    //}

                    if (ProjectId != 0)
                    {
                        var project = _Context.VProjectSalesOffers.Where(a => a.Id == ProjectId).FirstOrDefault();

                        if (project != null)
                        {
                            ProjectCard ProjectCard = new ProjectCard();
                            ProjectCard = GetProjectCard(project);

                            Response.ProjectCard = ProjectCard;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrProject2";
                            error.ErrorMSG = "This Project doesn't exist in DB, please select a valid project with valid Id";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "ErrProject3";
                        error.ErrorMSG = "No Project Id, Please Select Project";
                        Response.Errors.Add(error);
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        [HttpGet("GetProjectMaterialReleaseItemsDetails")]
        public GetProjectMaterialReleaseItemsDetailsResponse GetProjectMaterialReleaseItemsDetails(GetProjectMaterialReleaseItemsDetailsHeader headers)
        {
            GetProjectMaterialReleaseItemsDetailsResponse Response = new GetProjectMaterialReleaseItemsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region User Authentication Token 
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion

                var ProjectsDetailsList = new List<ProjectsSummary>();

                if (Response.Result)
                {

                    //long ProjectId = 0;
                    if (headers.ProjectId != 0)
                    {


                        var projectDB = _Context.Projects.Where(a => a.Id == headers.ProjectId).FirstOrDefault();

                        if (projectDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrProject2";
                            error.ErrorMSG = "This Project doesn't exist in DB, please select a valid project with valid Id";
                            Response.Errors.Add(error);
                        }
                    }
                    //else
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "ErrProject3";
                    //    error.ErrorMSG = "Please Insert a Valid ProjectId";
                    //    Response.Errors.Add(error);
                    //}

                    if (Response.Result)
                    {
                        #region filters
                        var ProjectReleasedItemsQuery = _Context.VInventoryMatrialReleaseItems.AsQueryable();

                        if (headers.ProjectId != 0)
                        {
                            ProjectReleasedItemsQuery = ProjectReleasedItemsQuery.Where(a => a.ProjectId == headers.ProjectId).AsQueryable();
                        }
                        if (headers.clientId != 0)
                        {
                            ProjectReleasedItemsQuery = ProjectReleasedItemsQuery.Where(a => a.ClientId == headers.clientId).AsQueryable();
                        }
                        if (headers.DateFrom != null)
                        {
                            DateTime creationDate;
                            if (!DateTime.TryParse(headers.DateFrom, out creationDate))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Project Creation To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                                return Response;
                            }
                            ProjectReleasedItemsQuery = ProjectReleasedItemsQuery.Where(a => a.CreationDate >= creationDate).AsQueryable();
                        }
                        if (headers.DateTo != null)
                        {
                            DateTime EndDate;
                            if (!DateTime.TryParse(headers.DateTo, out EndDate))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Project Creation To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                                return Response;
                            }
                            ProjectReleasedItemsQuery = ProjectReleasedItemsQuery.Where(a => a.CreationDate < EndDate).AsQueryable();
                        }

                        #endregion
                        var sumTotalPriceBOM = ProjectReleasedItemsQuery.Where(a => a.FromBom == true).Sum(a => a.CustomeUnitPrice);
                        var sumTotalPrice = ProjectReleasedItemsQuery.Where(a => a.FromBom == false).Sum(a => a.CustomeUnitPrice);
                        //decimal sumTotalPrice = 0;
                        //decimal sumTotalPriceBOM = 0;

                        var ProjectReleasedItemsList = ProjectReleasedItemsQuery.ToList() ;//remove wa sh8l el t7tha
                        //List<VInventoryMatrialReleaseItem> ProjectReleasedItemsList = ProjectReleasedItemsQuery.ToList();
                        var ProjectReleasedItemsListIDs = ProjectReleasedItemsList.Select(a => a.InventoryItemCategoryId).ToList();

                        var InventoryItemCategoryList = _Context.InventoryItemCategories.Where(a => ProjectReleasedItemsListIDs.Contains(a.Id)).ToList();


                        var InventoryItemCategoryLoadAllData = _Context.InventoryItemCategories.ToList();

                        if (ProjectReleasedItemsList != null && ProjectReleasedItemsList.Count > 0)
                        {
                            var InventoryMatrialReleaseItemsForProjectListVM = new List<InventoryMatrialReleaseItemsForProject>();

                            foreach (var ProjectReleasedItem in ProjectReleasedItemsList)
                            {
                                InventoryMatrialReleaseItemsForProject ProjectReleasedItemObj = new InventoryMatrialReleaseItemsForProject();
                                ProjectReleasedItemObj.InventoryMatrialReleasetID = ProjectReleasedItem.InventoryMatrialReleasetId;
                                ProjectReleasedItemObj.InventoryMatrialRequestID = ProjectReleasedItem.InventoryMatrialRequestId ?? 0;
                                ProjectReleasedItemObj.InventoryItemID = ProjectReleasedItem.InventoryItemId ?? 0;
                                ProjectReleasedItemObj.InventoryItemName = ProjectReleasedItem.ItemName;
                                ProjectReleasedItemObj.InventoryItemCode = ProjectReleasedItem.ItemCode;
                                ProjectReleasedItemObj.RecivedQuantity = ProjectReleasedItem.RecivedQuantity ?? 0;
                                ProjectReleasedItemObj.UOMShortName = ProjectReleasedItem.UomshortName;
                                ProjectReleasedItemObj.FabricationOrderID = ProjectReleasedItem.FabricationOrderId;
                                ProjectReleasedItemObj.FabricationOrderNumber = ProjectReleasedItem.FabNumber;
                                ProjectReleasedItemObj.CreationDate = ProjectReleasedItem.CreationDate.ToShortDateString();
                                ProjectReleasedItemObj.FromBOM = (bool)ProjectReleasedItem.FromBom;
                                ProjectReleasedItemObj.ProjectName = ProjectReleasedItem.ProjectName;
                                ProjectReleasedItemObj.ClientName = ProjectReleasedItem.ClientName;
                                ProjectReleasedItemObj.ProjectID = ProjectReleasedItem.ProjectId;

                                if (ProjectReleasedItem.CalculationType == 1)
                                {
                                    ProjectReleasedItemObj.InventoryItemPrice = ProjectReleasedItem.AverageUnitPrice;
                                    ProjectReleasedItemObj.InventoryTotalPrice = ProjectReleasedItem.AverageUnitPrice * ProjectReleasedItem.RecivedQuantity;
                                }
                                else if (ProjectReleasedItem.CalculationType == 2)
                                {
                                    ProjectReleasedItemObj.InventoryItemPrice = ProjectReleasedItem.MaxUnitPrice;
                                    ProjectReleasedItemObj.InventoryTotalPrice = ProjectReleasedItem.MaxUnitPrice * ProjectReleasedItem.RecivedQuantity;
                                }
                                else if (ProjectReleasedItem.CalculationType == 3)
                                {
                                    ProjectReleasedItemObj.InventoryItemPrice = ProjectReleasedItem.LastUnitPrice;
                                    ProjectReleasedItemObj.InventoryTotalPrice = ProjectReleasedItem.LastUnitPrice * ProjectReleasedItem.RecivedQuantity;
                                }
                                else if (ProjectReleasedItem.CalculationType == 4)
                                {
                                    ProjectReleasedItemObj.InventoryItemPrice = ProjectReleasedItem.CustomeUnitPrice;
                                    ProjectReleasedItemObj.InventoryTotalPrice = ProjectReleasedItem.CustomeUnitPrice * ProjectReleasedItem.RecivedQuantity;
                                }

                                if (ProjectReleasedItem.InventoryItemCategoryId != null)
                                {
                                    ProjectReleasedItemObj.InventoryItemParentCategoryID = Common.GetParentCategory((int)ProjectReleasedItem.InventoryItemCategoryId, InventoryItemCategoryLoadAllData);
                                    ProjectReleasedItemObj.InventoryItemParentCategoryName = InventoryItemCategoryList.Where(a => a.Id == (int)ProjectReleasedItem.InventoryItemCategoryId).Select(a => a.Name).FirstOrDefault();

                                }

                                InventoryMatrialReleaseItemsForProjectListVM.Add(ProjectReleasedItemObj);

                                //if ((bool)ProjectReleasedItem.FromBom)
                                //{
                                //    sumTotalPriceBOM += (decimal)ProjectReleasedItemObj.InventoryTotalPrice;
                                //}
                                //else
                                //{
                                //    sumTotalPrice += (decimal)ProjectReleasedItemObj.InventoryTotalPrice;
                                //}
                            }

                            //-----------------------------------------Back order ---------------------------------------------------------
                            //VInventoryInternalBackOrderItem backOrderItems = new VInventoryInternalBackOrderItem();
                            var backOrderItems = _Context.VInventoryInternalBackOrderItems.AsQueryable();

                            if (headers.ProjectId != 0 || headers.clientId != 0 || headers.DateFrom != null || headers.DateTo != null)
                            {
                                if (headers.ProjectId != 0)
                                {
                                    backOrderItems = backOrderItems.Where(a => a.ProjectId == headers.ProjectId);
                                }
                                if (headers.clientId != 0)
                                {
                                    backOrderItems = backOrderItems.Where(a => a.ClientId == headers.clientId);
                                }
                                if (headers.DateFrom != null)
                                {
                                    DateTime dateFrom;
                                    if (DateTime.TryParse(headers.DateFrom, out dateFrom))
                                    {
                                        backOrderItems = backOrderItems.Where(a => a.CreationDate > dateFrom);
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err10";
                                        error.ErrorMSG = "Invalid Date From";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }
                                }
                                if (headers.DateTo != null)
                                {
                                    DateTime dateTo;
                                    if (DateTime.TryParse(headers.DateFrom, out dateTo))
                                    {
                                        backOrderItems = backOrderItems.Where(a => a.CreationDate <= dateTo);
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err10";
                                        error.ErrorMSG = "Invalid Date From";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }
                                }

                                var backOrderItemsList = backOrderItems.ToList();

                                foreach (var backOrderItem in backOrderItemsList)
                                {
                                    InventoryMatrialReleaseItemsForProject inventoryMatrialReleaseItemsForProject = new InventoryMatrialReleaseItemsForProject();
                                    inventoryMatrialReleaseItemsForProject.InventoryMatrialReleasetID = 0;
                                    inventoryMatrialReleaseItemsForProject.InventoryMatrialRequestID = 0;
                                    inventoryMatrialReleaseItemsForProject.InventoryBackOrderID = backOrderItem.InventoryInternalBackOrderId;
                                    inventoryMatrialReleaseItemsForProject.InventoryItemID = backOrderItem.InventoryItemId;
                                    inventoryMatrialReleaseItemsForProject.InventoryItemName = backOrderItem.ItemName;
                                    inventoryMatrialReleaseItemsForProject.InventoryItemCode = backOrderItem.ItemCode;
                                    inventoryMatrialReleaseItemsForProject.RecivedQuantity = backOrderItem.RecivedQuantity ?? 0;
                                    inventoryMatrialReleaseItemsForProject.UOMShortName = backOrderItem.UomshortName;
                                    inventoryMatrialReleaseItemsForProject.ClientName = !string.IsNullOrEmpty(backOrderItem.ClientName) ? backOrderItem.ClientName : "";
                                    inventoryMatrialReleaseItemsForProject.ProjectID = backOrderItem.ProjectId ?? 0;
                                    inventoryMatrialReleaseItemsForProject.ProjectName = backOrderItem.ProjectName;
                                    inventoryMatrialReleaseItemsForProject.FabricationOrderID = 0;
                                    inventoryMatrialReleaseItemsForProject.FabricationOrderNumber = 0;
                                    //inventoryMatrialReleaseItemsForProject.InventoryMatrialReleaseItemsID = backOrderItems.ID;
                                    //inventoryMatrialReleaseItemsForProject.InventoryMatrialRequestItemID = backOrderItems.InventoryMatrialRequestItemID;
                                    inventoryMatrialReleaseItemsForProject.CreationDate = backOrderItem.CreationDate.ToString();
                                    InventoryMatrialReleaseItemsForProjectListVM.Add(inventoryMatrialReleaseItemsForProject);
                                }


                            }

                            //--------------------------------------------------------------------------------------------------


                            Response.ProjectReleasedItemsList = InventoryMatrialReleaseItemsForProjectListVM;
                            Response.SumTotalPrice = sumTotalPrice;
                            Response.SumTotalPriceBOM = sumTotalPriceBOM;
                        }
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        [HttpGet("GetProjectsTypesDetails")]
        public ProjectsTypesDetailsResponse GetProjectsTypesDetails([FromHeader] GetProjectsTypesDetailsHeaders headers)
        {
            ProjectsTypesDetailsResponse Response = new ProjectsTypesDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region User Authentication Token 
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion

                var ProjectsDetailsList = new List<ProjectsSummary>();



                Expression<Func<VProjectSalesOffer, bool>> whereClause;

                switch (headers.ProjectsStatus)
                {
                    case "open":
                        whereClause = a => a.Closed == false && a.Active == true;
                        break;
                    case "closed":
                        whereClause = a => a.Closed == true && a.Active == true;
                        break;
                    case "deactivated":
                        whereClause = a => a.Active == false;
                        break;
                    default:
                        if (headers.ProjectsStatus != "")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err111";
                            error.ErrorMSG = "Invalid Project Status must be in (open, closed, deactivated)";
                            Response.Errors.Add(error);
                        }

                        whereClause = a => a.Active == true;
                        break;
                }

                if (Response.Result)
                {
                    var projects = new List<VProjectSalesOffer>();

                    var projectsQuery = _Context.VProjectSalesOffers.AsQueryable();

                    projectsQuery = projectsQuery.Where(whereClause);

                    if (headers.SalesPersonId != 0)
                    {
                        projectsQuery = projectsQuery.Where(a => a.SalesPersonId == headers.SalesPersonId);
                    }
                    if (headers.BranchId != 0)
                    {
                        projectsQuery = projectsQuery.Where(a => a.SalesPersonBranchId == headers.BranchId);
                    }

                    if (headers.Year > 0)
                    {
                        var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                        var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);

                        if (headers.Month > 0)
                        {
                            StartDate = new DateTime(headers.Year, headers.Month, 1);

                            if (headers.Month != 12)
                            {
                                EndDate = new DateTime(headers.Year, headers.Month + 1, 1);
                            }
                            else
                            {
                                EndDate = new DateTime(headers.Year + 1, 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(headers.Year, 1, 1);
                            EndDate = new DateTime(headers.Year + 1, 1, 1);
                        }

                        projectsQuery = projectsQuery.Where(a => a.CreationDate >= StartDate && a.CreationDate < EndDate);
                    }
                    else
                    {
                        if (headers.Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    projects = projectsQuery.ToList();

                    var Groupedprojects = projects.GroupBy(p => p.OfferType).ToList();

                    //Get All Warrenty Projects
                    var WarrantyProjects = projects.Where(p => p.OfferType == "New Maintenance Offer" && p.MaintenanceType == "Warranty").ToList();
                    var WarrantyProjectsGrp = WarrantyProjects.GroupBy(a => a.MaintenanceType).ToList();
                    Groupedprojects.Add(WarrantyProjectsGrp.FirstOrDefault());

                    foreach (var projectsGrp in Groupedprojects)
                    {
                        if (projectsGrp != null)
                        {

                            var projectsDetails = new ProjectsSummary();

                            projectsDetails.CountOfProjects = projectsGrp.Count();
                            projectsDetails.ProjectsType = projectsGrp.Key;
                            projectsDetails.ProjectsStatus = headers.ProjectsStatus == "" ? "All" : headers.ProjectsStatus;
                            decimal totalProjectsCost = 0;
                            decimal TotalProjectsCollectedCost = 0;
                            decimal TotalProjectsCollectedPercentage = 0;

                            foreach (var project in projectsGrp)
                            {
                                totalProjectsCost += project.ExtraCost ?? 0 + project.FinalOfferPrice ?? 0;

                                decimal ProjectCollected = 0;

                                var clientAccounts = _Context.ClientAccounts.Where(x => x.ProjectId == project.Id);
                                foreach (var clientAccount in clientAccounts)
                                {
                                    if (clientAccount.AmountSign == "plus")
                                    {
                                        ProjectCollected = ProjectCollected + clientAccount.Amount;
                                    }
                                    else if (clientAccount.AmountSign == "minus")
                                    {
                                        ProjectCollected = ProjectCollected - clientAccount.Amount;
                                    }
                                }

                                TotalProjectsCollectedCost += ProjectCollected;
                            }

                            if (totalProjectsCost != 0)
                            {
                                TotalProjectsCollectedPercentage = TotalProjectsCollectedCost / totalProjectsCost * 100;
                            }

                            projectsDetails.TotalCost = totalProjectsCost;
                            projectsDetails.TotalCollectedCost = TotalProjectsCollectedCost;
                            projectsDetails.TotalCollectedPercentage = string.Format("{0:0.0}", TotalProjectsCollectedPercentage) + "%";

                            ProjectsDetailsList.Add(projectsDetails);
                        }
                    }

                    //Adding Direct Sales Projects to New Project Offer Projects
                    var NewProjectOffer = ProjectsDetailsList.Where(a => a.ProjectsType == "New Project Offer").FirstOrDefault();
                    var DirectSales = ProjectsDetailsList.Where(a => a.ProjectsType == "Direct Sales").FirstOrDefault();
                    if (DirectSales != null)
                    {
                        NewProjectOffer.CountOfProjects += DirectSales.CountOfProjects;
                        NewProjectOffer.TotalCost += DirectSales.TotalCost;
                        NewProjectOffer.TotalCollectedCost += DirectSales.TotalCollectedCost;
                        if (NewProjectOffer.TotalCost != 0)
                        {
                            NewProjectOffer.TotalCollectedPercentage = string.Format("{0:0.0}", NewProjectOffer.TotalCollectedCost / NewProjectOffer.TotalCost * 100) + "%";
                        }
                    }
                    ProjectsDetailsList.Remove(DirectSales);

                    if (ProjectsDetailsList.Where(a => a.ProjectsType == "New Project Offer").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Project Offer";
                        projectsDetails.TotalCost = 0;
                        projectsDetails.TotalCollectedCost = 0;
                        projectsDetails.TotalCollectedPercentage = "0.0%";

                        ProjectsDetailsList.Add(projectsDetails);
                    }
                    if (ProjectsDetailsList.Where(a => a.ProjectsType == "New Maintenance Offer").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Maintenance Offer";
                        projectsDetails.TotalCost = 0;
                        projectsDetails.TotalCollectedCost = 0;
                        projectsDetails.TotalCollectedPercentage = "0.0%";

                        ProjectsDetailsList.Add(projectsDetails);
                    }
                    if (ProjectsDetailsList.Where(a => a.ProjectsType == "New Rent Offer").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Rent Offer";
                        projectsDetails.TotalCost = 0;
                        projectsDetails.TotalCollectedCost = 0;
                        projectsDetails.TotalCollectedPercentage = "0.0%";

                        ProjectsDetailsList.Add(projectsDetails);
                    }
                    if (ProjectsDetailsList.Where(a => a.ProjectsType == "New Internal Order").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "New Internal Order";
                        projectsDetails.TotalCost = 0;
                        projectsDetails.TotalCollectedCost = 0;
                        projectsDetails.TotalCollectedPercentage = "0.0%";

                        ProjectsDetailsList.Add(projectsDetails);
                    }
                    if (ProjectsDetailsList.Where(a => a.ProjectsType == "Warranty").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Warrenty";
                        projectsDetails.TotalCost = 0;
                        projectsDetails.TotalCollectedCost = 0;
                        projectsDetails.TotalCollectedPercentage = "0.0%";

                        ProjectsDetailsList.Add(projectsDetails);
                    }
                    if (ProjectsDetailsList.Where(a => a.ProjectsType == "Sales Return").FirstOrDefault() == null)
                    {
                        var projectsDetails = new ProjectsSummary();

                        projectsDetails.CountOfProjects = 0;
                        projectsDetails.ProjectsType = "Sales Return";
                        projectsDetails.TotalCost = 0;
                        projectsDetails.TotalCollectedCost = 0;
                        projectsDetails.TotalCollectedPercentage = "0.0%";

                        ProjectsDetailsList.Add(projectsDetails);
                    }

                    Response.ProjectsDetails = ProjectsDetailsList;
                    Response.ProjectsStatus = headers.ProjectsStatus == "" ? "All" : headers.ProjectsStatus;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetProjectList")]
        public ProjectDDLResponse GetProjectList([FromHeader] GetProjectListHeaders headers)
        {
            ProjectDDLResponse Response = new ProjectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                //long ClientID = 0;
                //if (!string.IsNullOrEmpty(headers["ClientID"]) && long.TryParse(headers["ClientID"], out ClientID))
                //{
                //    long.TryParse(headers["ClientID"], out ClientID);
                //}

                //bool ProjectReturn = false;
                //if (!string.IsNullOrEmpty(headers["ProjectReturn"]) && bool.TryParse(headers["ProjectReturn"], out ProjectReturn))
                //{
                //    bool.TryParse(headers["ProjectReturn"], out ProjectReturn);
                //}

                var DDLList = new List<ProjectDDL>();
                if (Response.Result)
                {
                    var QuerableDB = _Context.VProjectSalesOffers.Where(x => x.Active == true).AsQueryable();
                    if (headers.ProjectReturn != null)
                    {
                        if (!headers.ProjectReturn) //return  = false
                        {
                            // List Sales offer IDS Return
                            var IDSalesOfferReturned = _Context.InvoiceCnandDns.Select(x => x.SalesOfferId).ToList();
                            // List of Project List IDS Returns
                            QuerableDB = QuerableDB.Where(x => !IDSalesOfferReturned.Contains(x.SalesOfferId)).AsQueryable();
                        }
                    }
                    if (headers.SearchKey != null && headers.SearchKey != "")
                    {

                        string SearchKey = headers.SearchKey;
                        SearchKey = HttpUtility.UrlDecode(SearchKey);

                        QuerableDB = QuerableDB.Where(x => x.ProjectName.ToLower().Contains(SearchKey.ToLower()) ||
                                                x.ProjectSerial == SearchKey).AsQueryable();
                    }

                    if (headers.ClientID != 0)
                    {
                        QuerableDB = QuerableDB.Where(x => x.ClientId == headers.ClientID).AsQueryable();
                    }
                    var ListDB = QuerableDB.ToList();

                    if (ListDB.Count() > 0)
                    {
                        foreach (var account in ListDB)
                        {
                            string ClientName = account.ClientId != null ? " - " + Common.GetClientName((long)account.ClientId, _Context) : "";
                            var DLLObj = new ProjectDDL();
                            DLLObj.ID = account.Id;
                            DLLObj.Name = account.ProjectName + ClientName;
                            DLLObj.ProjectSerial = account.ProjectSerial;
                            DLLObj.SalesOfferId = account.SalesOfferId;
                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetProjectFabricationList")]
        public ProjectFabDLResponse GetProjectFabricationList([FromHeader] long ProjectID, [FromHeader] string SearchKey)
        {
            ProjectFabDLResponse Response = new ProjectFabDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var DDLList = new List<ProjectFabItemDDL>();
                //long ProjectID = 0;
                //if (ProjectID != null && ProjectID != 0)
                //{
                //    ProjectID = long.Parse(headers["ProjectID"]);
                //}
                //else
                //{
                //    Response.Result = false;
                //    Error error = new Error();
                //    error.ErrorCode = "Err69";
                //    error.ErrorMSG = "Invalid Project ID";
                //    Response.Errors.Add(error);
                //    return Response;
                //}
                if (Response.Result)
                {
                    var ListDB = _Context.ProjectFabrications.AsQueryable();

                    if (ProjectID != 0)
                    {
                        ListDB = ListDB.Where(x => x.ProjectId == ProjectID);
                    }
                    if (SearchKey != null && SearchKey != "")
                    {

                        //string SearchKey = headers["SearchKey"];
                        SearchKey = HttpUtility.UrlDecode(SearchKey);

                        ListDB = ListDB.Where(x => x.FabNumber != null ? x.FabNumber.ToString() == SearchKey : false ||
                                                   x.FabOrderSerial == SearchKey
                                                );
                    }
                    //var ListFilteredDB = ListDB.ToList();
                    var DBList = ListDB.ToList();
                    if (DBList.Count() > 0)
                    {
                        foreach (var item in DBList)
                        {
                            var DLLObj = new ProjectFabItemDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.FabNumber.ToString();
                            DLLObj.FabSerial = item.FabOrderSerial;
                            DLLObj.ProjectID = item.ProjectId;
                            DLLObj.ProjectSerial = Common.GetProjectSerial(item.ProjectId, _Context);
                            DLLObj.ProjectName = Common.GetProjectName(item.ProjectId, _Context);

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetSalesPersonProjectsDetails")]
        public async Task<SalesPersonsProjectsDetailsResponse> GetSalesPersonProjectsDetails([FromHeader] GetSalesPersonProjectsDetailsHeaders headers)
        {
            SalesPersonsProjectsDetailsResponse Response = new SalesPersonsProjectsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    #region
                    //int Month = 0;
                    //if (!string.IsNullOrEmpty(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                    //{
                    //    int.TryParse(headers["Month"], out Month);
                    //}

                    //int Year = 0;
                    //if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                    //{
                    //    int.TryParse(headers["Year"], out Year);
                    //}

                    //int BranchId = 0;
                    //if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    //{
                    //    int.TryParse(headers["BranchId"], out BranchId);
                    //}
                    #endregion

                    var ProjectsDbQuery = _Context.VProjectSalesOffers.Where(a => a.OfferType == "New Project Offer" || a.OfferType == "Direct Sales" ||
                                                                                   a.OfferType == "New Rent Offer" || a.OfferType == "New Maintenance Offer").AsQueryable();

                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    if (headers.Year > 0)
                    {

                        if (headers.Month > 0)
                        {
                            StartDate = new DateTime(headers.Year, headers.Month, 1);

                            if (headers.Month != 12)
                            {
                                EndDate = new DateTime(headers.Year, headers.Month + 1, 1);
                            }
                            else
                            {
                                EndDate = new DateTime(headers.Year + 1, 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(headers.Year, 1, 1);
                            EndDate = new DateTime(headers.Year + 1, 1, 1);
                        }
                    }
                    else
                    {
                        if (headers.Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    ProjectsDbQuery = ProjectsDbQuery.Where(a => a.CreationDate >= StartDate && a.CreationDate < EndDate);

                    if (headers.BranchId != 0)
                    {
                        ProjectsDbQuery = ProjectsDbQuery.Where(a => a.SalesPersonBranchId == headers.BranchId);
                    }

                    var ProjectsDbList = await ProjectsDbQuery.ToListAsync();

                    var GroupedProjects = ProjectsDbList.GroupBy(p => p.SalesPersonId).ToList();

                    var SalesPerosnsProjectsDetailsList = new List<SalesPersonProjectsDetails>();

                    foreach (var projectsGrp in GroupedProjects)
                    {
                        if (projectsGrp != null)
                        {
                            decimal totalProjectsCost = 0;
                            decimal totalProjectsCollectedCost = 0;
                            decimal totalProjectsRemainCost = 0;
                            decimal totalProjectsCollectedPercentage = 0;
                            decimal totalProjectsRemainPercentage = 0;

                            foreach (var project in projectsGrp)
                            {
                                var ProjectCost = project.ExtraCost + project.FinalOfferPrice;
                                totalProjectsCost += ProjectCost ?? 0;

                                decimal ProjectCollected = 0;

                                var clientAccounts = await _Context.ClientAccounts.Where(x => x.ProjectId == project.Id).ToListAsync();
                                foreach (var clientAccount in clientAccounts)
                                {
                                    if (clientAccount.AmountSign == "plus")
                                    {
                                        ProjectCollected = ProjectCollected + clientAccount.Amount;
                                    }
                                    else if (clientAccount.AmountSign == "minus")
                                    {
                                        ProjectCollected = ProjectCollected - clientAccount.Amount;
                                    }
                                }

                                totalProjectsCollectedCost += ProjectCollected;
                            }

                            var ProjectReturn = _Context.VProjectReturnSalesOffers.Where(a => a.ParentSalesPersonId == projectsGrp.Key).ToList();
                            if (ProjectReturn != null && ProjectReturn.Count > 0)
                            {
                                var ProjectReturnCost = ProjectReturn.Sum(a => a.ReturnFinalOfferPrice);
                                totalProjectsCost -= (decimal)ProjectReturnCost;
                            }
                            if (totalProjectsCost != 0)
                            {
                                totalProjectsRemainCost = totalProjectsCost - totalProjectsCollectedCost;
                                totalProjectsCollectedPercentage = totalProjectsCollectedCost / totalProjectsCost * 100;
                                totalProjectsRemainPercentage = 100 - totalProjectsCollectedPercentage;
                            }

                            var salesPersonName = "";
                            var salesPerson = _Context.Users.Where(a => a.Id == projectsGrp.Key).FirstOrDefault();
                            if (salesPerson != null)
                            {
                                salesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                            }

                            var SalesPersonProjectDetailsObj = new SalesPersonProjectsDetails
                            {
                                SalesPersonId = projectsGrp.Key ?? 0,
                                SalesPersonName = salesPersonName,
                                ClientsCount = projectsGrp.Select(a => a.ClientId).Distinct().Count(),
                                DealsCount = projectsGrp.Count(),
                                AchievedTarget = totalProjectsCost,
                                TotalCollected = totalProjectsCollectedCost,
                                TotalRemain = totalProjectsRemainCost,
                                CollectedPercentage = string.Format("{0:0.0}", totalProjectsCollectedPercentage) + "%",
                                RemainPercentage = string.Format("{0:0.0}", totalProjectsRemainPercentage) + "%",
                            };

                            SalesPerosnsProjectsDetailsList.Add(SalesPersonProjectDetailsObj);
                        }
                    }

                    Response.SalesPersonsProjectsDetailsList = SalesPerosnsProjectsDetailsList;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        //---------------------------------------Error in GroupBy() (not working without .ToList() ) ----------------------------------

        [HttpGet("GetMyProjectsDetailsCRM")]
        public async Task<MyProjectsCRMDashboardResponse> GetMyProjectsDetailsCRM([FromHeader] GetMyProjectsDetailsCRMHeaders headers)
        {
            MyProjectsCRMDashboardResponse Response = new MyProjectsCRMDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = await _projectService.GetMyProjectsDetailsCRM(headers);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        //---------------------------------------Error in GroupBy() -----------------------------------


        [HttpGet("TopSealingProductExcel")]
        public async Task<ActionResult<BaseMessageResponse>> TopSealingProductExcel([FromHeader] GetMyProjectsDetailsCRMHeaders header)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;




                if (Response.Result)
                {
                    /*int Month = 0;
                    if (!string.IsNullOrEmpty(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                    {
                        int.TryParse(headers["Month"], out Month);
                    }

                    int Year = 0;
                    if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                    {
                        int.TryParse(headers["Year"], out Year);
                    }

                    long SalesPersonId = 0;
                    if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                    {
                        long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    }

                    int BranchId = 0;
                    if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    {
                        int.TryParse(headers["BranchId"], out BranchId);
                    }*/



                    var GetMyProjectsList = await GetMyProjectsDetailsCRM(header);





                    var dt = new DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[4] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Category Name"),
                                                     new DataColumn("Product Name"),
                                                     new DataColumn("Quantity"),
                                                     new DataColumn("Value")




                    });



                    if (GetMyProjectsList != null)
                    {

                        foreach (var CategoryName in GetMyProjectsList.TopSellingProductsCategoryGrouped)
                        {

                            foreach (var ProductsDetails in CategoryName.TopSellingProductsList)
                            {
                                dt.Rows.Add(
                                //item != null ? item.ID : 0

                                //item.Id != null ? item.Id : 0,
                                CategoryName.CategoryName != null ? CategoryName.CategoryName : "N/A",
                                ProductsDetails.ProductName != null ? ProductsDetails.ProductName : "N/A",
                                ProductsDetails.SoldCount,
                                ProductsDetails.TotalSoldPrice
                                );

                            };


                        }

                    }


                    var dtProduct = new DataTable("Grid");

                    dtProduct.Columns.AddRange(new DataColumn[3] {
                                                     //new DataColumn("ID"),
                                                 
                                                     new DataColumn("Product Name"),
                                                     new DataColumn("Quantity"),
                                                     new DataColumn("Value")




                    });



                    if (GetMyProjectsList != null)
                    {



                        foreach (var ProductsDetails in GetMyProjectsList.TopSellingProducts)
                        {
                            dtProduct.Rows.Add(


                                ProductsDetails.ProductName != null ? ProductsDetails.ProductName : "N/A",
                                ProductsDetails.SoldCount,
                                ProductsDetails.TotalSoldPrice
                                );

                        };




                    }



                    var dtProductSum = new DataTable("Grid");

                    dtProductSum.Columns.AddRange(new DataColumn[3] {
                                                     //new DataColumn("ID"),
                                                 
                                                     new DataColumn("Category"),
                                                     new DataColumn("Quantity"),
                                                     new DataColumn("Value")




                    });



                    if (GetMyProjectsList != null)
                    {



                        foreach (var ProductsSum in GetMyProjectsList.TopSellingProductsCategoryGrouped)
                        {
                            dtProductSum.Rows.Add(


                                ProductsSum.CategoryName,
                                ProductsSum.TotalDealsCount,
                                ProductsSum.TotalDealsPrice
                                );

                        };




                    }






                    //Second List to pass it to PDF



                    //if (FileExtension != null && FileExtension == "xml")
                    //{
                    using (ExcelPackage packge = new ExcelPackage())
                    {
                        //Create the worksheet
                        ExcelWorksheet ws = packge.Workbook.Worksheets.Add("SalesOffer");
                        ws.TabColor = System.Drawing.Color.Red;
                        ws.Columns.BestFit = true;


                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        ws.Cells["A1"].LoadFromDataTable(dt, true);
                        //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                        //Format the header for column 1-3
                        using (ExcelRange range = ws.Cells["A1:O1"])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                        }






                        using (var package = new ExcelPackage())
                        {





                            var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();

                            string FullFileName = DateTime.Now.ToFileTime() + "_" + "ProjectDetails.xlsx";
                            string PathsTR = "Attachments/" + CompanyName + "/";
                            string path = Path.Combine(_host.WebRootPath, PathsTR);
                            string p_strPath = Path.Combine(path, FullFileName);
                            var workSheet = package.Workbook.Worksheets.Add("Project Details Grouped by Category");
                            ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                            var workSheetByProduct = package.Workbook.Worksheets.Add("Project Details Product");
                            ExcelRangeBase excelRangeBase2 = workSheetByProduct.Cells.LoadFromDataTable(dtProduct, true);
                            var workSheetProductSum = package.Workbook.Worksheets.Add("Project Product Sum");
                            ExcelRangeBase excelRangeBase3 = workSheetProductSum.Cells.LoadFromDataTable(dtProductSum, true);

                            System.IO.File.Exists(p_strPath);
                            FileStream objFileStrm = System.IO.File.Create(p_strPath);

                            objFileStrm.Close();
                            package.Save();
                            System.IO.File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                            package.Dispose();

                            Response.Message = Globals.baseURL + '/' + PathsTR + FullFileName;


                        }


                    }


                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }



        #region helper methods not used as API methods
        //[ApiExplorerSettings(IgnoreApi = true)]                             //to ignore the method from the Api (helper method used in GetFullProjectCardDetails() )
        [NonAction]                                                           //to ignore the method from the Api (helper method used in GetFullProjectCardDetails() )
        public ProjectCard GetProjectCard(VProjectSalesOffer project)
        {
            ProjectCard ProjectCard = new ProjectCard();

            if (project != null)
            {
                ProjectCard.OfferId = project.SalesOfferId;
                ProjectCard.ProjectId = project.Id;
                ProjectCard.ProjectName = project.ProjectName;
                ProjectCard.ProjectSerial = project.ProjectSerial;

                if (project.MaintenanceType == "Warranty")
                {
                    ProjectCard.projectType = "Warrenty";
                }
                //else if (project.OfferType == "Direct Sales")
                //{
                //    ProjectCard.projectType = "New Project Offer";
                //}
                else
                {
                    ProjectCard.projectType = project.OfferType;
                }

                if (project.OfferType == "New Maintenance Offer")
                {
                    ProjectCard.MaintnanceType = project.MaintenanceType;
                }

                if (project.Active == false)
                {
                    ProjectCard.projectStatus = "Deactivated";
                }
                else if (project.Closed == true)
                {
                    ProjectCard.projectStatus = "Closed";
                }
                else
                {
                    ProjectCard.projectStatus = "Open";
                }

                var client = _Context.Clients.Where(c => c.Id == project.ClientId).FirstOrDefault();
                if (client != null)
                {
                    ProjectCard.ClientName = client.Name;
                }

                //Project Manager Details
                var projectManager = _Context.Users.Where(a => a.Id == project.ProjectManagerId).FirstOrDefault();
                if (projectManager != null)
                {
                    ProjectCard.ProjectManagerName = projectManager.FirstName + " " + projectManager.MiddleName + " " + projectManager.LastName;

                    if (projectManager.Photo != null)
                    {
                        ProjectCard.ProjectManagerImgUrl = Globals.baseURL + Common.GetUserPhoto(projectManager.Id, _Context);
                        /*                            "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(projectManager.Id.ToString(), key)) + "&type=photo";
                        */
                    }
                }

                //Sales Person Details
                var salesPerson = _Context.Users.Where(a => a.Id == project.SalesPersonId).FirstOrDefault();
                if (salesPerson != null)
                {
                    ProjectCard.ProjectSalesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;

                    if (salesPerson.Photo != null)
                    {
                        ProjectCard.ProjectSalesPersonImgUrl = Globals.baseURL + Common.GetUserPhoto(salesPerson.Id, _Context);
                        /*                            "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(salesPerson.Id.ToString(), key)) + "&type=photo";
                        */
                    }
                }

                ProjectCard.ProjectRevision = project.Revision.ToString();

                decimal TotalProjectCost = 0;
                if (project.ExtraCost == null)
                {
                    project.ExtraCost = 0;
                }
                if (project.FinalOfferPrice == null)
                {
                    project.FinalOfferPrice = 0;
                }
                ProjectCard.OfferPrice = project.OfferAmount ?? 0;

                var offerTaxDB = _Context.SalesOfferInvoiceTaxes.Where(a => a.SalesOfferId == project.SalesOfferId && a.Active == true).ToList();
                decimal TotalOfferTax = 0;
                foreach (var offerTax in offerTaxDB)
                {
                    TotalOfferTax += offerTax.TaxValue;
                }
                ProjectCard.OfferTax = TotalOfferTax;

                var offerExtraCostsDB = _Context.SalesOfferExtraCosts.Where(a => a.SalesOfferId == project.SalesOfferId && a.Active == true).ToList();
                decimal TotalOfferExtraCosts = 0;
                foreach (var offerExtraCost in offerExtraCostsDB)
                {
                    TotalOfferExtraCosts += offerExtraCost.Amount;
                }
                ProjectCard.OfferExtraCost = TotalOfferExtraCosts;

                ProjectCard.ProjectExtraModifications = project.ExtraCost ?? 0;

                TotalProjectCost = ProjectCard.OfferPrice + ProjectCard.OfferTax + ProjectCard.OfferExtraCost + ProjectCard.ProjectExtraModifications;
                ProjectCard.TotalProjectPrice = TotalProjectCost;

                decimal TotalProjectCollectedCost = 0;
                var clientAccounts = _Context.ClientAccounts.Where(x => x.ProjectId == project.Id);
                foreach (var clientAccount in clientAccounts)
                {
                    if (clientAccount.AmountSign == "plus")
                    {
                        TotalProjectCollectedCost = TotalProjectCollectedCost + clientAccount.Amount;
                    }
                    else if (clientAccount.AmountSign == "minus")
                    {
                        TotalProjectCollectedCost = TotalProjectCollectedCost - clientAccount.Amount;
                    }
                }
                ProjectCard.TotalProjectCollectedCost = TotalProjectCollectedCost;
                decimal TotalProjectsCollectedPercentage = 0;
                if (TotalProjectCost != 0)
                {
                    TotalProjectsCollectedPercentage = TotalProjectCollectedCost / TotalProjectCost * 100;
                    ProjectCard.TotalProjectCollectedCostPercentage = string.Format("{0:0.0}", TotalProjectsCollectedPercentage) + "%";
                }
                else
                {
                    ProjectCard.TotalProjectCollectedCostPercentage = "N/A";
                }

                var RemainCost = TotalProjectCost - TotalProjectCollectedCost;
                ProjectCard.RemainCollection = RemainCost;

                var TotalProjectFabricationsDB = _Context.ProjectFabrications.Where(a => a.ProjectId == project.Id).ToList();
                var TotalProjectFabricationsCount = TotalProjectFabricationsDB.Count();
                var OpenProjectFabricationsCount = TotalProjectFabricationsDB.Where(a => a.Progress < 100 || a.PassQc == false).Count();
                ProjectCard.TotalFabOrders = TotalProjectFabricationsCount;
                ProjectCard.OpenFabOrders = OpenProjectFabricationsCount;
                if (TotalProjectFabricationsCount > 0)
                {
                    ProjectCard.FabOrdersPercentage = string.Format("{0:0.0}", OpenProjectFabricationsCount / TotalProjectFabricationsCount * 100) + "%";
                }
                else
                {
                    ProjectCard.FabOrdersPercentage = "0.0%";
                }
                decimal FabricationsTotalHours = 0;
                foreach (var fab in TotalProjectFabricationsDB)
                {
                    var fabTotalHoursDB = _Context.VProjectFabricationTotalHours.Where(a => a.ProjectFabId == fab.Id).FirstOrDefault();
                    if (fabTotalHoursDB != null)
                    {
                        FabricationsTotalHours += fabTotalHoursDB.TotalHours ?? 0;
                    }
                }
                ProjectCard.TotalHoursFabOrders = FabricationsTotalHours;


                var TotalProjectInstallationsDB = _Context.ProjectInstallations.Where(a => a.ProjectId == project.Id).ToList();
                var TotalProjectInstallationsCount = TotalProjectInstallationsDB.Count();
                var OpenProjectInstallationsCount = TotalProjectInstallationsDB.Where(a => a.Progress < 100 || a.PassQc == false).Count();
                ProjectCard.TotalInstallationOrders = TotalProjectInstallationsCount;
                ProjectCard.OpenInstallationOrders = OpenProjectInstallationsCount;
                ProjectCard.FinishProjectReportStatus = "Not Arrived";
                if (TotalProjectInstallationsCount > 0)
                {
                    foreach (var projectInstallation in TotalProjectInstallationsDB)
                    {
                        var ProjectInstallationReports = _Context.ProjectInstallationReports.Where(a => a.ProjectInstallationId == projectInstallation.Id).ToList();
                        if (ProjectInstallationReports != null && ProjectInstallationReports.Count > 0)
                        {
                            foreach (var ProjectInstallationReport in ProjectInstallationReports)
                            {
                                var ProjectFinishInstallationAttachments = _Context.ProjectFinishInstallationAttachments.Where(a => a.ProjectInstallationReportId == ProjectInstallationReport.Id).ToList();

                                if (ProjectFinishInstallationAttachments != null && ProjectFinishInstallationAttachments.Count > 0)
                                {
                                    ProjectCard.FinishProjectReportStatus = "Arrived";
                                }
                            }
                        }
                    }

                    ProjectCard.InstallationOrdersPercentage = string.Format("{0:0.0}", OpenProjectInstallationsCount / TotalProjectInstallationsCount * 100) + "%";
                }
                else
                {
                    ProjectCard.InstallationOrdersPercentage = "0.0%";
                }
                decimal InstallationsTotalHours = 0;
                foreach (var Inst in TotalProjectInstallationsDB)
                {
                    var instTotalHoursDB = _Context.VProjectInstallationTotalHours.Where(a => a.ProjectInsId == Inst.Id).FirstOrDefault();
                    if (instTotalHoursDB != null)
                    {
                        InstallationsTotalHours += instTotalHoursDB.TotalHours ?? 0;
                    }
                }
                ProjectCard.TotalHoursInstallationOrders = InstallationsTotalHours;
                decimal sumTotalExtraReleasedItemsPrice = 0;

                var projectAssignedBOMsDB = _Context.Boms.Where(a => a.ProjectId == project.Id).OrderBy(a => a.ModifiedDate).ToList();
                if (projectAssignedBOMsDB != null && projectAssignedBOMsDB.Count != 0)
                {
                    decimal TotalBOMsCurrentItemsCost = 0;
                    decimal TotalBOMsMaterialsCost = 0;

                    decimal TotalRemainCurrentPrice = 0;
                    decimal TotalRemainPricingTimePrice = 0;

                    decimal sumTotalBOMReleasedItemsPrice = 0;

                    foreach (var assignedBOM in projectAssignedBOMsDB)
                    {
                        var salesOfferProduct = _Context.SalesOfferProducts.Where(a => a.Id == assignedBOM.OfferItemId).FirstOrDefault();
                        var assignedBOMPartitions = _Context.Bompartitions.Where(a => a.Bomid == assignedBOM.Id).ToList();

                        foreach (var partition in assignedBOMPartitions)
                        {
                            var assignedBOMPartitionItems = _Context.BompartitionItems.Where(a => a.BompartitionId == partition.Id).ToList();
                            foreach (var item in assignedBOMPartitionItems)
                            {
                                TotalBOMsMaterialsCost += item.ItemQtyPrice;

                                var inventoryItem = _Context.InventoryItems.Where(a => a.Id == item.ItemId).FirstOrDefault();
                                //int CalculationType = 0;
                                if (inventoryItem != null)
                                {
                                    decimal BOMReleasedItemPrice = 0;
                                    decimal BOMReleasedItemQty = 0;

                                    decimal RemainCurrentItemPrice = 0;
                                    decimal RemainPricingTimeItemPrice = 0;

                                    var ProjectReleasedInventoryMaterialItemsDB = _Context.VInventoryMatrialReleaseItems.Where(
                                            a => a.ProjectId == project.Id && a.OfferItemId == assignedBOM.OfferItemId && a.InventoryItemId == inventoryItem.Id).ToList();

                                    if (ProjectReleasedInventoryMaterialItemsDB.Count > 0)
                                    {
                                        foreach (var ReleasedItem in ProjectReleasedInventoryMaterialItemsDB)
                                        {
                                            decimal ReleasedItemPrice = 0;
                                            if (ReleasedItem.CalculationType == 1)
                                            {
                                                ReleasedItemPrice = (ReleasedItem.AverageUnitPrice ?? 0) * (ReleasedItem.RecivedQuantity ?? 0);
                                            }
                                            else if (ReleasedItem.CalculationType == 2)
                                            {
                                                ReleasedItemPrice = (ReleasedItem.MaxUnitPrice ?? 0) * (ReleasedItem.RecivedQuantity ?? 0);
                                            }
                                            else if (ReleasedItem.CalculationType == 3)
                                            {
                                                ReleasedItemPrice = (ReleasedItem.LastUnitPrice ?? 0) * (ReleasedItem.RecivedQuantity ?? 0);
                                            }
                                            else if (ReleasedItem.CalculationType == 4)
                                            {
                                                ReleasedItemPrice = (ReleasedItem.CustomeUnitPrice ?? 0) * (ReleasedItem.RecivedQuantity ?? 0);
                                            }

                                            if (ReleasedItem.FromBom == true)
                                            {
                                                BOMReleasedItemPrice = ReleasedItemPrice;
                                                BOMReleasedItemQty = ReleasedItem.RecivedQuantity ?? 0;
                                                sumTotalBOMReleasedItemsPrice += ReleasedItemPrice;
                                            }
                                            else
                                            {
                                                sumTotalExtraReleasedItemsPrice += ReleasedItemPrice;
                                            }
                                        }
                                    }

                                    if (item.RequiredQty == 0)
                                    {
                                        RemainPricingTimeItemPrice = 0;
                                    }
                                    else
                                    {
                                        RemainPricingTimeItemPrice = item.ItemQtyPrice / item.RequiredQty * (item.RequiredQty - BOMReleasedItemQty);
                                    }
                                    TotalRemainPricingTimePrice += RemainPricingTimeItemPrice;

                                    if (item.ItemPriceType == "Custome")
                                    {
                                        TotalBOMsCurrentItemsCost += inventoryItem.CustomeUnitPrice * item.RequiredQty;

                                        RemainCurrentItemPrice = inventoryItem.CustomeUnitPrice * (item.RequiredQty - BOMReleasedItemQty);
                                    }
                                    else if (item.ItemPriceType == "Avr")
                                    {
                                        TotalBOMsCurrentItemsCost += inventoryItem.MaxUnitPrice * item.RequiredQty;

                                        RemainCurrentItemPrice = inventoryItem.MaxUnitPrice * (item.RequiredQty - BOMReleasedItemQty);
                                    }
                                    else if (item.ItemPriceType == "Max")
                                    {
                                        TotalBOMsCurrentItemsCost += inventoryItem.AverageUnitPrice * item.RequiredQty;

                                        RemainCurrentItemPrice = inventoryItem.AverageUnitPrice * (item.RequiredQty - BOMReleasedItemQty);
                                    }
                                    else
                                    {
                                        TotalBOMsCurrentItemsCost += inventoryItem.LastUnitPrice * item.RequiredQty;

                                        RemainCurrentItemPrice = inventoryItem.LastUnitPrice * (item.RequiredQty - BOMReleasedItemQty);
                                    }

                                    TotalRemainCurrentPrice += RemainCurrentItemPrice;

                                }
                            }
                        }

                        ProjectCard.BOMReleasedMaterialCost += sumTotalBOMReleasedItemsPrice;

                        ProjectCard.RemainMaterialCurrentCost += TotalRemainCurrentPrice * (decimal)salesOfferProduct.Quantity;
                        ProjectCard.RemainMaterialPricingTimeCost += TotalRemainPricingTimePrice * (decimal)salesOfferProduct.Quantity;
                        ProjectCard.RemainMaterialDiffCost += ProjectCard.RemainMaterialPricingTimeCost - ProjectCard.RemainMaterialCurrentCost;

                        ProjectCard.BOMMaterialCost += TotalBOMsMaterialsCost * (decimal)salesOfferProduct.Quantity;
                        ProjectCard.BOMCurrentMaterialCost += TotalBOMsCurrentItemsCost * (decimal)salesOfferProduct.Quantity;
                    }

                    if (ProjectCard.BOMMaterialCost > 0)
                    {
                        ProjectCard.BOMReleasedPercentage = string.Format("{0:0.0}", ProjectCard.BOMReleasedMaterialCost / ProjectCard.BOMMaterialCost * 100) + "%";
                        ProjectCard.ExtraReleasedPercentage = string.Format("{0:0.0}", ProjectCard.ExtraReleasedMaterialCost / ProjectCard.BOMMaterialCost * 100) + "%";
                        ProjectCard.TotalReleasedPercentage = string.Format("{0:0.0}", ProjectCard.TotalReleasedMaterialCost / ProjectCard.BOMMaterialCost * 100) + "%";
                    }
                    else
                    {
                        ProjectCard.BOMReleasedPercentage = "N/A";
                        ProjectCard.ExtraReleasedPercentage = "N/A";
                        ProjectCard.TotalReleasedPercentage = "N/A";
                    }

                    var lastModifiedDate = DateTime.Parse(projectAssignedBOMsDB[projectAssignedBOMsDB.Count() - 1].ModifiedDate.ToString());
                    ProjectCard.LastBOMMaterialCostModifiedDate = lastModifiedDate.ToShortDateString();

                    ProjectCard.BOMMaterialCostDate = project.CreationDate.ToShortDateString();
                }
                var ExtraReleasedDB = _Context.VInventoryMatrialReleaseItems.Where(a => a.ProjectId == project.Id && a.FromBom != true).ToList();

                foreach (var releasedItem in ExtraReleasedDB)
                {
                    if (releasedItem.CalculationType == 1)
                    {
                        sumTotalExtraReleasedItemsPrice += (releasedItem.AverageUnitPrice ?? 0) * (releasedItem.RecivedQuantity ?? 0);
                    }
                    else if (releasedItem.CalculationType == 2)
                    {
                        sumTotalExtraReleasedItemsPrice += (releasedItem.MaxUnitPrice ?? 0) * (releasedItem.RecivedQuantity ?? 0);
                    }
                    else if (releasedItem.CalculationType == 3)
                    {
                        sumTotalExtraReleasedItemsPrice += (releasedItem.LastUnitPrice ?? 0) * (releasedItem.RecivedQuantity ?? 0);
                    }
                    else if (releasedItem.CalculationType == 4)
                    {
                        sumTotalExtraReleasedItemsPrice += (releasedItem.CustomeUnitPrice ?? 0) * (releasedItem.RecivedQuantity ?? 0);
                    }

                }
                ProjectCard.ExtraReleasedMaterialCost = sumTotalExtraReleasedItemsPrice;
                ProjectCard.TotalReleasedMaterialCost += sumTotalExtraReleasedItemsPrice;

                ProjectCard.ProjectStartDate = project.StartDate.ToShortDateString();
                ProjectCard.ProjectEndDate = project.EndDate.ToShortDateString();
                ProjectCard.RemainTime = (int)(project.EndDate - DateTime.Now).TotalDays;

            }

            return ProjectCard;
        }
        #endregion
        #region project fabrication

        [HttpGet("GetProjectFabrication")]
        public MiniProjectCard GetProjectFabrication()
        {
            MiniProjectCard newCard = new MiniProjectCard();
            newCard.TotalProjectPrice = 10000;
            return newCard;
        }

        #endregion


        [HttpGet("GetFabInsMainReports")]
        public FabInsMainReportsResponse GetFabInsMainReports([FromHeader] FabInsMainReportsHeader headers)
        {
            FabInsMainReportsResponse response = new FabInsMainReportsResponse()
            {
                result = true,
                errors = new List<Error>()
            };
            try
            {
                #region User Authentication Token 
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.errors = validation.errors;
                response.result = validation.result;
                #endregion

                if (response.result == true)
                {
                    var FabInsMainReportsQuery = _Context.VMainFabInsReports.AsQueryable();

                    #region headers handling
                    if (headers.ProjectId != 0)
                    {
                        FabInsMainReportsQuery = FabInsMainReportsQuery.Where(a => a.ProjectId == headers.ProjectId);
                    }
                    if (headers.ClientId != 0)
                    {
                        FabInsMainReportsQuery = FabInsMainReportsQuery.Where(a => a.UserId == headers.ClientId);
                    }
                    if (headers.DateFrom != null)
                    {
                        DateTime dateFrom;
                        if (DateTime.TryParse(headers.DateFrom, out dateFrom))
                        {
                            FabInsMainReportsQuery = FabInsMainReportsQuery.Where(a => a.StartDate >= dateFrom);
                        }
                        else
                        {
                            response.result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Invalid Date From";
                            response.errors.Add(error);
                            return response;
                        }
                    }
                    if (headers.DateTo != null)
                    {
                        DateTime dateTo;
                        if (DateTime.TryParse(headers.DateTo, out dateTo))
                        {
                            FabInsMainReportsQuery = FabInsMainReportsQuery.Where(a => a.StartDate < dateTo);
                        }
                        else
                        {
                            response.result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Invalid Date To";
                            response.errors.Add(error);
                            return response;
                        }
                    }
                    #endregion

                    //var FabInsMainReports = FabInsMainReportsQuery.ToList();
                    var SumOfHours = FabInsMainReportsQuery.Sum(a => a.HourNum);
                    var AverageOFEvaluation = FabInsMainReportsQuery.Sum(a => a.Evaluation) ?? 0;

                    var FabInsMainReportsList = PagedList<VMainFabInsReport>.Create(FabInsMainReportsQuery, headers.CurrentPage, headers.NumberOfItemsPerPage);

                    List<FabInsMainData> ReportsList = new List<FabInsMainData>();

                    foreach (var report in FabInsMainReportsList)
                    {
                        FabInsMainData fabInsMainData = new FabInsMainData();
                        fabInsMainData.UserName = report.UserName;
                        fabInsMainData.DepName = report.DepName;
                        fabInsMainData.ProjectName = report.ProjectName;
                        fabInsMainData.Date = report.StartDate?.ToString();
                        fabInsMainData.HourNum = report.HourNum;
                        fabInsMainData.Evaluation = report.Evaluation ?? 0;
                        fabInsMainData.Comment = report.Comment;
                        fabInsMainData.RequestType = report.RequestType;

                        ReportsList.Add(fabInsMainData);
                    }
                    response.paginationHeader = new PaginationHeader
                    {
                        CurrentPage = headers.CurrentPage,
                        TotalPages = FabInsMainReportsList.TotalPages,
                        ItemsPerPage = headers.NumberOfItemsPerPage,
                        TotalItems = FabInsMainReportsList.TotalCount
                    };
                    response.SumOfHours = SumOfHours;
                    response.AverageOfEvaluation = AverageOFEvaluation;
                    response.FabInsMainData = ReportsList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.errors.Add(error);
                return response;
            }
        }


        /*[HttpGet("GetInventoryMatrialReleaseUnionInternalBackOrderItems")]
        public InventoryMatrialReleaseUnionInternalBackOrderItemsResponse GetInventoryMatrialReleaseUnionInternalBackOrderItems
            ([FromHeader] InventoryMatrialReleaseUnionInternalBackOrderItemsheaders headers)
        {
            InventoryMatrialReleaseUnionInternalBackOrderItemsResponse response = new InventoryMatrialReleaseUnionInternalBackOrderItemsResponse()
            {
                result = true,
                errors = new List<Error>()
            };

            try
            {
                #region User Authentication Token 
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.errors = validation.errors;
                response.result = validation.result;
                #endregion

                if(response.result)
                {
                    #region dealing with headers
                    var ItemsQuery = _Context.VInventoryMatrialReleaseUnionInternalBackOrderItems.AsQueryable();

                    if(headers.ProjectID != 0)
                    {
                        ItemsQuery = ItemsQuery.Where(a => a.ProjectId == headers.ProjectID);
                    }
                    if (headers.clientId != 0)
                    {
                        ItemsQuery = ItemsQuery.Where(a => a.ClientId == headers.clientId);
                    }
                    if (headers.CreateFrom != null)
                    {
                        DateTime creationDate;
                        if (!DateTime.TryParse(headers.CreateFrom, out creationDate))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Invalid Project Creation To";
                            response.errors.Add(error);
                            response.result = false;
                            return response;
                        }
                        ItemsQuery = ItemsQuery.Where(a => a.CreationDate >= creationDate).AsQueryable();
                    }
                    if (headers.createTo != null)
                    {
                        DateTime EndDate;
                        if (!DateTime.TryParse(headers.createTo, out EndDate))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Invalid Project Creation To";
                            response.errors.Add(error);
                            response.result = false;
                            return response;
                        }
                        ItemsQuery = ItemsQuery.Where(a => a.CreationDate < EndDate).AsQueryable();
                    }
                    #endregion

                    //var Items = ItemsQuery.ToList();
                    var OrdersItems = PagedList<VInventoryMatrialReleaseUnionInternalBackOrderItem>.Create(ItemsQuery, headers.CurrentPage, headers.NumberOfItemsPerPage);
                   
                    List<InventoryMatrialReleaseUnionInternalBackOrderItems> ItemsList = new List<InventoryMatrialReleaseUnionInternalBackOrderItems>();
                    foreach (var item in OrdersItems)
                    {
                        InventoryMatrialReleaseUnionInternalBackOrderItems TempItem = new InventoryMatrialReleaseUnionInternalBackOrderItems();
                        TempItem.InventoryMatrialReleasetID = item.InventoryMatrialReleasetId;
                        TempItem.InventoryMatrialRequestID = item.InventoryMatrialRequestId;
                        TempItem.InventoryBackOrderID = item.InventoryInternalBackOrderId;
                        TempItem.InventoryItemName = item.ItemName;
                        TempItem.InventoryItemCode = item.ItemCode;
                        TempItem.Quantity = item.RecivedQuantity;
                        TempItem.UOMShortName = item.UomshortName;
                        TempItem.ProjectName = item.ProjectName;
                        TempItem.FabOrderNum = item.FabNum;
                        TempItem.clientName = item.ClientName;
                        TempItem.CreationDate = item.CreationDate.Date.ToShortDateString();

                        ItemsList.Add(TempItem);
                    }

                    response.ListOfItems = ItemsList;
                    response.paginationHeader = new PaginationHeader
                    {
                        CurrentPage = headers.CurrentPage,
                        TotalPages = OrdersItems.TotalPages,
                        ItemsPerPage = headers.NumberOfItemsPerPage,
                        TotalItems = OrdersItems.TotalCount
                    };
                }
                
                return response;
            }
            catch (Exception ex)
            {
                response.result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.errors.Add(error);
                return response;
            }
        }*/

        [HttpPost("DeleteProject")]

        public BaseResponseWithId<long> DeleteProject([FromHeader]long ProjectId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _taskMangerProjectService.DeleteProject(ProjectId);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetFullProjectsCardsDetails")]
        public ProjectsCardsDetailsResponse GetFullProjectsCardsDetails(FullProjectsCardsFilters filters)
        {
            ProjectsCardsDetailsResponse Response = new ProjectsCardsDetailsResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.GetFullProjectsCardsDetails(filters);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        //------------------------------------moved from WCF----------------------------------------
        [HttpGet("GetWorkshopStationsList")]
        public GetWorkshopStationResponse GetWorkshopStationsList()
        {
            GetWorkshopStationResponse Response = new GetWorkshopStationResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.GetWorkshopStationsList();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetWorkshopStationsItemPerId")]
        public GetWorkshopStationsItemResponse GetWorkshopStationsItemPerId([FromHeader]int WorkShopID)
        {
            GetWorkshopStationsItemResponse Response = new GetWorkshopStationsItemResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.GetWorkshopStationsItemPerId(WorkShopID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpGet("GetTeamUsersSelect")]
        public GetTeamUsersSelectResponse GetTeamUsersSelect([FromHeader]string SearchKey)
        {
            GetTeamUsersSelectResponse Response = new GetTeamUsersSelectResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.GetTeamUsersSelect(SearchKey);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        [HttpPost("DeleteWorkshopStation")]
        public BaseResponseWithID DeleteWorkshopStation([FromBody]WorkShopStationData RequestData)
        {
            var Response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.DeleteWorkshopStation(RequestData, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("DeleteProjectWorkshopStation")]
        public BaseResponseWithID DeleteProjectWorkshopStation([FromBody]ProjectWorkShopStationData RequestData)
        {
            var Response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.DeleteProjectWorkshopStation(RequestData, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("AddEditProjectContactPerson")]
        public BaseResponseWithID AddEditProjectContactPerson([FromBody] ProjectContactPersonData RequestData)
        {
            var Response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.AddEditProjectContactPerson(RequestData, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


        [HttpGet("GetStationReceivedWorkOrders")]
        public GetStationReceivedWorkOrdersResponse GetStationReceivedWorkOrders([FromHeader] int? StationID, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10)
        {
            GetStationReceivedWorkOrdersResponse Response = new GetStationReceivedWorkOrdersResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.GetStationReceivedWorkOrders(StationID, CurrentPage, NumberOfItemsPerPage);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("AddNewJobOrderStations")]
        public BaseResponseWithID AddNewJobOrderStations(JobOrderStationsData RequestData)
        {
            var Response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    Response = _projectService.AddNewJobOrderStations(RequestData, validation.userID);
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

    }

}
