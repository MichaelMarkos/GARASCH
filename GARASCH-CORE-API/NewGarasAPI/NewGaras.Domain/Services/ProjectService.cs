using ClosedXML.Excel;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.Project.Headers;
using NewGaras.Infrastructure.Models.Project.Responses;
using NewGaras.Infrastructure.Models.Project.UsedInResponses;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Project.Headers;
using NewGarasAPI.Models.Project.Responses;
using NewGarasAPI.Models.Project.UsedInResponses;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NewGaras.Domain.Services
{
    public class ProjectService : IProjectService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;
        public ProjectService(GarasTestContext context, ITenantService tenantService,IUnitOfWork unitOfWork)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _unitOfWork = unitOfWork;
        }

        public string GetProjectName(long id)
        {
            string name = "";
            var LoadObjDB = _unitOfWork.VProjectSalesOffers.Find(x => x.Id == id);
            if (LoadObjDB != null)
            {
                name = LoadObjDB.ProjectName;
            }
            return name;
        }


        public ProjectCard GetProjectCard(VProjectSalesOffer project)             //Moved to Core WebAPI
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

                var client = _unitOfWork.Clients.FindAll(c => c.Id == project.ClientId).FirstOrDefault();
                if (client != null)
                {
                    ProjectCard.ClientName = client.Name;
                }

                //Project Manager Details
                var projectManager = _unitOfWork.Users.FindAll(a=>a.Id==project.ProjectManagerId).FirstOrDefault();
                if (projectManager != null)
                {
                    ProjectCard.ProjectManagerName = projectManager.FirstName + " " + projectManager.MiddleName + " " + projectManager.LastName;

                    if (projectManager.PhotoUrl != null)
                    {
                        ProjectCard.ProjectManagerImgUrl = Globals.baseURL + projectManager.PhotoUrl;
                    }
                }

                //Sales Person Details
                var salesPerson = _unitOfWork.Users.FindAll(a=>a.Id==project.SalesPersonId).FirstOrDefault();
                if (salesPerson != null)
                {
                    ProjectCard.ProjectSalesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;

                    if (salesPerson.PhotoUrl != null)
                    {
                        ProjectCard.ProjectSalesPersonImgUrl = Globals.baseURL + salesPerson.PhotoUrl;
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

                var offerTaxDB = _unitOfWork.SalesOfferInvoiceTaxes.FindAll(a => a.SalesOfferId == project.SalesOfferId && a.Active == true).ToList();
                decimal TotalOfferTax = 0;
                foreach (var offerTax in offerTaxDB)
                {
                    TotalOfferTax += offerTax.TaxValue;
                }
                ProjectCard.OfferTax = TotalOfferTax;

                var offerExtraCostsDB = _unitOfWork.SalesOfferExtraCosts.FindAll(a => a.SalesOfferId == project.SalesOfferId && a.Active == true).ToList();
                decimal TotalOfferExtraCosts = 0;
                foreach (var offerExtraCost in offerExtraCostsDB)
                {
                    TotalOfferExtraCosts += offerExtraCost.Amount;
                }
                ProjectCard.OfferExtraCost = TotalOfferExtraCosts;

                ProjectCard.ProjectExtraModifications = (project.ExtraCost ?? 0);

                TotalProjectCost = ProjectCard.OfferPrice + ProjectCard.OfferTax + ProjectCard.OfferExtraCost + ProjectCard.ProjectExtraModifications;
                ProjectCard.TotalProjectPrice = TotalProjectCost;

                decimal TotalProjectCollectedCost = 0;
                var clientAccounts = _unitOfWork.ClientAccounts.FindAll(x => x.ProjectId == project.Id);
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
                    TotalProjectsCollectedPercentage = (TotalProjectCollectedCost / TotalProjectCost) * 100;
                    ProjectCard.TotalProjectCollectedCostPercentage = String.Format("{0:0.0}", TotalProjectsCollectedPercentage) + "%";
                }
                else
                {
                    ProjectCard.TotalProjectCollectedCostPercentage = "N/A";
                }

                var RemainCost = TotalProjectCost - TotalProjectCollectedCost;
                ProjectCard.RemainCollection = RemainCost;

                var TotalProjectFabricationsDB = _unitOfWork.ProjectFabrications.FindAll(a => a.ProjectId == project.Id).ToList();
                var TotalProjectFabricationsCount = TotalProjectFabricationsDB.Count();
                var OpenProjectFabricationsCount = TotalProjectFabricationsDB.Where(a => a.Progress < 100 || a.PassQc == false).Count();
                ProjectCard.TotalFabOrders = TotalProjectFabricationsCount;
                ProjectCard.OpenFabOrders = OpenProjectFabricationsCount;
                if (TotalProjectFabricationsCount > 0)
                {
                    ProjectCard.FabOrdersPercentage = String.Format("{0:0.0}", ((OpenProjectFabricationsCount / TotalProjectFabricationsCount) * 100)) + "%";
                }
                else
                {
                    ProjectCard.FabOrdersPercentage = "0.0%";
                }
                decimal FabricationsTotalHours = 0;
                foreach (var fab in TotalProjectFabricationsDB)
                {
                    var fabTotalHoursDB = _unitOfWork.VProjectFabricationTotalHours.FindAll(a => a.ProjectFabId == fab.Id).FirstOrDefault();
                    if (fabTotalHoursDB != null)
                    {
                        FabricationsTotalHours += (fabTotalHoursDB.TotalHours ?? 0);
                    }
                }
                ProjectCard.TotalHoursFabOrders = FabricationsTotalHours;


                var TotalProjectInstallationsDB = _unitOfWork.ProjectInstallations.FindAll(a => a.ProjectId == project.Id).ToList();
                var TotalProjectInstallationsCount = TotalProjectInstallationsDB.Count();
                var OpenProjectInstallationsCount = TotalProjectInstallationsDB.Where(a => a.Progress < 100 || a.PassQc == false).Count();
                ProjectCard.TotalInstallationOrders = TotalProjectInstallationsCount;
                ProjectCard.OpenInstallationOrders = OpenProjectInstallationsCount;
                ProjectCard.FinishProjectReportStatus = "Not Arrived";
                if (TotalProjectInstallationsCount > 0)
                {
                    foreach (var projectInstallation in TotalProjectInstallationsDB)
                    {
                        var ProjectInstallationReports = _unitOfWork.ProjectInstallationReports.FindAll(a => a.ProjectInstallationId == projectInstallation.Id).ToList();
                        if (ProjectInstallationReports != null && ProjectInstallationReports.Count > 0)
                        {
                            foreach (var ProjectInstallationReport in ProjectInstallationReports)
                            {
                                var ProjectFinishInstallationAttachments = _unitOfWork.ProjectFinishInstallationAttachments.FindAll(a => a.ProjectInstallationReportId == ProjectInstallationReport.Id).ToList();

                                if (ProjectFinishInstallationAttachments != null && ProjectFinishInstallationAttachments.Count > 0)
                                {
                                    ProjectCard.FinishProjectReportStatus = "Arrived";
                                }
                            }
                        }
                    }

                    ProjectCard.InstallationOrdersPercentage = String.Format("{0:0.0}", ((OpenProjectInstallationsCount / TotalProjectInstallationsCount) * 100)) + "%";
                }
                else
                {
                    ProjectCard.InstallationOrdersPercentage = "0.0%";
                }
                decimal InstallationsTotalHours = 0;
                foreach (var Inst in TotalProjectInstallationsDB)
                {
                    var instTotalHoursDB = _unitOfWork.VProjectInstallationTotalHours.FindAll(a => a.ProjectInsId == Inst.Id).FirstOrDefault();
                    if (instTotalHoursDB != null)
                    {
                        InstallationsTotalHours += (instTotalHoursDB.TotalHours ?? 0);
                    }
                }
                ProjectCard.TotalHoursInstallationOrders = InstallationsTotalHours;

                decimal sumTotalExtraReleasedItemsPrice = 0;

                var projectAssignedBOMsDB = _unitOfWork.Boms.FindAll(a => a.ProjectId == project.Id).OrderBy(a => a.ModifiedDate).ToList();
                if (projectAssignedBOMsDB != null && projectAssignedBOMsDB.Count != 0)
                {
                    decimal TotalBOMsCurrentItemsCost = 0;
                    decimal TotalBOMsMaterialsCost = 0;

                    decimal TotalRemainCurrentPrice = 0;
                    decimal TotalRemainPricingTimePrice = 0;

                    decimal sumTotalBOMReleasedItemsPrice = 0;

                    foreach (var assignedBOM in projectAssignedBOMsDB)
                    {
                        var salesOfferProduct = _unitOfWork.SalesOfferProducts.GetById(assignedBOM.OfferItemId??0);
                        var assignedBOMPartitions = _unitOfWork.Bompartitions.FindAll(a => a.Bomid == assignedBOM.Id).ToList();

                        foreach (var partition in assignedBOMPartitions)
                        {
                            var assignedBOMPartitionItems = _unitOfWork.BompartitionItems.FindAll(a => a.BompartitionId == partition.Id).ToList();
                            foreach (var item in assignedBOMPartitionItems)
                            {
                                TotalBOMsMaterialsCost += item.ItemQtyPrice;

                                var inventoryItem = _unitOfWork.InventoryItems.GetById(item.ItemId);
                                //int CalculationType = 0;
                                if (inventoryItem != null)
                                {
                                    decimal BOMReleasedItemPrice = 0;
                                    decimal BOMReleasedItemQty = 0;

                                    decimal RemainCurrentItemPrice = 0;
                                    decimal RemainPricingTimeItemPrice = 0;

                                    var ProjectReleasedInventoryMaterialItemsDB = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(
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
                        ProjectCard.BOMReleasedPercentage = String.Format("{0:0.0}", ((ProjectCard.BOMReleasedMaterialCost / ProjectCard.BOMMaterialCost) * 100)) + "%";
                        ProjectCard.ExtraReleasedPercentage = String.Format("{0:0.0}", ((ProjectCard.ExtraReleasedMaterialCost / ProjectCard.BOMMaterialCost) * 100)) + "%";
                        ProjectCard.TotalReleasedPercentage = String.Format("{0:0.0}", ((ProjectCard.TotalReleasedMaterialCost / ProjectCard.BOMMaterialCost) * 100)) + "%";
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
                var ExtraReleasedDB = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(a => a.ProjectId == project.Id && a.FromBom != true).ToList();

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

        public ProjectsCardsDetailsResponse GetFullProjectsCardsDetails(FullProjectsCardsFilters filters)
        {
            ProjectsCardsDetailsResponse Response = new ProjectsCardsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ProjectsDetailsList = new List<ProjectsSummary>();

                #region Filters
                if (Response.Result)
                {
                    /*string RequestOfferType = "";
                    if (!string.IsNullOrEmpty(headers["OfferType"]))
                    {
                        RequestOfferType = headers["OfferType"];
                    }

                    string RequestProjectsStatus = "";
                    if (!string.IsNullOrEmpty(headers["ProjectsStatus"]))
                    {
                        RequestProjectsStatus = headers["ProjectsStatus"].ToLower();
                    }

                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }

                    long ProjectId = 0;
                    if (!string.IsNullOrEmpty(headers["ProjectId"]) && long.TryParse(headers["ProjectId"], out ProjectId))
                    {
                        long.TryParse(headers["ProjectId"], out ProjectId);
                    }

                    long ClientId = 0;
                    if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                    {
                        long.TryParse(headers["ClientId"], out ClientId);
                    }

                    string ProjectOfferTypesFilters = "";
                    if (!string.IsNullOrEmpty(headers["ProjectOfferTypesFilters"]))
                    {
                        ProjectOfferTypesFilters = HttpUtility.UrlDecode(headers["ProjectOfferTypesFilters"]);
                    }

                    string MaintenanceTypesFilters = "";
                    if (!string.IsNullOrEmpty(headers["MaintenanceTypesFilters"]))
                    {
                        MaintenanceTypesFilters = HttpUtility.UrlDecode(headers["MaintenanceTypesFilters"]);
                    }

                    string Location = "";
                    if (!string.IsNullOrEmpty(headers["Location"]))
                    {
                        Location = HttpUtility.UrlDecode(headers["Location"]);
                    }

                    long SalesPersonBranchId = 0;
                    if (!string.IsNullOrEmpty(headers["SalesPersonBranchId"]) && long.TryParse(headers["SalesPersonBranchId"], out SalesPersonBranchId))
                    {
                        long.TryParse(headers["SalesPersonBranchId"], out SalesPersonBranchId);
                    }

                    long ProjectManagerId = 0;
                    if (!string.IsNullOrEmpty(headers["ProjectManagerId"]) && long.TryParse(headers["ProjectManagerId"], out ProjectManagerId))
                    {
                        long.TryParse(headers["ProjectManagerId"], out ProjectManagerId);
                    }

                    long SalesPersonId = 0;
                    if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                    {
                        long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    }*/

                    /*DateTime StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    bool filterFrom = false;
                    if (!string.IsNullOrEmpty(headers["ProjectCreationFrom"]))
                    {
                        DateTime ProjectCreateFrom = DateTime.Now;
                        if (!DateTime.TryParse(headers["ProjectCreationFrom"], out ProjectCreateFrom))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Project Creation From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        StartDate = ProjectCreateFrom.Date;
                        filterFrom = true;
                    }

                    DateTime EndDate = DateTime.Now;
                    bool filterTo = false;
                    if (!string.IsNullOrEmpty(headers["ProjectCreationTo"]))
                    {
                        DateTime ProjectCreateTo = DateTime.Now;
                        if (!DateTime.TryParse(headers["ProjectCreationTo"], out ProjectCreateTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Invalid Project Creation To";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        EndDate = ProjectCreateTo.Date;
                        filterTo = true;
                    }*/

                    //bool SortByRemainCollections = false;

                    /*if (!string.IsNullOrEmpty(headers["SortByRemainCollections"]))
                    {
                        if (bool.Parse(headers["SortByRemainCollections"]) != true && bool.Parse(headers["SortByRemainCollections"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "SortByRemainCollections must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            SortByRemainCollections = bool.Parse(headers["SortByRemainCollections"]);
                        }
                    }*/

                    //bool SortByRemainType = false;

                    /*if (!string.IsNullOrEmpty(headers["SortByRemainType"]))
                    {
                        if (bool.Parse(headers["SortByRemainType"]) != true && bool.Parse(headers["SortByRemainType"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "SortByRemainType must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            SortByRemainType = bool.Parse(headers["SortByRemainType"]);
                        }
                    }*/
                    #endregion

                    if (Response.Result)
                    {
                        if (filters.OfferType != null)
                        {
                            var projectsQuery = _unitOfWork.VProjectSalesOffers.FindAllQueryable(a=>true);
                            if (filters.ProjectId != 0)
                            {
                                projectsQuery = projectsQuery.Where(p => p.Id == filters.ProjectId);
                            }
                            if (filters.ClientId != 0)
                            {
                                projectsQuery = projectsQuery.Where(p => p.ClientId == filters.ClientId);
                            }
                            if (filters.ProjectManagerId != 0)
                            {
                                projectsQuery = projectsQuery.Where(p => p.ProjectManagerId == filters.ProjectManagerId);
                            }
                            if (filters.SalesPersonId != 0)
                            {
                                projectsQuery = projectsQuery.Where(p => p.Id == filters.SalesPersonId);
                            }
                            if (filters.ProjectCreationFrom!=null)
                            {
                                projectsQuery = projectsQuery.Where(a => a.CreationDate >= filters.ProjectCreationFrom).AsQueryable();
                            }
                            if (filters.ProjectCreationTo!=null)
                            {
                                projectsQuery = projectsQuery.Where(a => a.CreationDate <= filters.ProjectCreationTo).AsQueryable();
                            }
                            if (filters.SalesPersonBranchId != 0)
                            {
                                projectsQuery = projectsQuery.Where(a => a.SalesPersonBranchId == filters.SalesPersonBranchId).AsQueryable();
                            }
                            if (filters.Location != "" && filters.Location != null)
                            {
                                projectsQuery = projectsQuery.Where(a => a.ProjectLocation == filters.Location).AsQueryable();
                            }
                            if (filters.MaintenanceTypesFilters != "" && filters.MaintenanceTypesFilters != null)
                            {
                                var MaintenanceTypesFiltersList = filters.MaintenanceTypesFilters.Split(',').ToList();
                                projectsQuery = projectsQuery.Where(a => MaintenanceTypesFiltersList.Contains(a.Type)).AsQueryable();
                            }
                            if (filters.ProjectOfferTypesFilters != "" && filters.ProjectOfferTypesFilters != null)
                            {
                                var ProjectOfferTypesFiltersList = filters.ProjectOfferTypesFilters.Split(',').ToList();
                                projectsQuery = projectsQuery.Where(a => ProjectOfferTypesFiltersList.Contains(a.OfferType)).AsQueryable();
                            }
                            if (filters.OfferType == "Warrenty")
                            {
                                projectsQuery = projectsQuery.Where(p => p.OfferType == "New Maintenance Offer" && p.MaintenanceType == "Warranty");
                            }
                            else if (filters.OfferType == "New Project Offer")
                            {
                                projectsQuery = projectsQuery.Where(a => a.OfferType == filters.OfferType || a.OfferType == "Direct Sales");
                            }
                            else
                            {
                                if (filters.OfferType != "")
                                {
                                    projectsQuery = projectsQuery.Where(a => a.OfferType == filters.OfferType);
                                }
                            }
                            
                            Expression<Func<VProjectSalesOffer, bool>> whereClause;
                            switch (filters.ProjectsStatus)
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
                                    if (filters.ProjectsStatus != "")
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

                            projectsQuery = projectsQuery.Where(whereClause).OrderByDescending(a => a.CreationDate);

                            // var ProjectsList = new List<V_Project_SalesOffer>();
                            var ProjectsList = PagedList<VProjectSalesOffer>.Create(projectsQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);

                            //if (SortByRemainCollections || SortByRemainType)
                            //{
                            //    ProjectsList = projectsQuery.ToList();
                            //}
                            //else
                            //{

                            //}


                            List<ProjectCard> ProjectsCardsList = new List<ProjectCard>();

                            foreach (var project in ProjectsList)
                            {
                                ProjectCard ProjectCard = new ProjectCard();
                                ProjectCard = GetProjectCard(project);

                                ProjectsCardsList.Add(ProjectCard);
                            }

                            //if(SortByRemainCollections)
                            //{
                            //    ProjectsCardsList.OrderBy(a => a.RemainCollection).ToList();
                            //}
                            //if(SortByRemainType)
                            //{
                            //    ProjectsCardsList.OrderBy(a => a.RemainTime).ToList();
                            //}

                            Response.ProjectCardsList = ProjectsCardsList;
                            Response.ProjectsStatus = filters.ProjectsStatus;
                            Response.ProjectsType = filters.OfferType;

                            Response.PaginationHeader = new PaginationHeader()
                            {
                                CurrentPage = ProjectsList.CurrentPage,
                                ItemsPerPage = ProjectsList.PageSize,
                                TotalItems = ProjectsList.TotalCount,
                                TotalPages = ProjectsList.TotalPages
                            };
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

        public GetWorkshopStationResponse GetWorkshopStationsList()
        {
            GetWorkshopStationResponse response = new GetWorkshopStationResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetWorkshopStationList = new List<WorkshopStationResponseData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetWorkshopStationDB = _unitOfWork.WorkshopStations.GetAll();


                        if (GetWorkshopStationDB != null && GetWorkshopStationDB.Count() > 0)
                        {

                            foreach (var GetWorkshopStationDBOBJ in GetWorkshopStationDB)
                            {
                                var GetWorkshopStationResponse = new WorkshopStationResponseData();

                                GetWorkshopStationResponse.ID = (int)GetWorkshopStationDBOBJ.Id;

                                GetWorkshopStationResponse.Location = GetWorkshopStationDBOBJ.Location;

                                GetWorkshopStationResponse.StationName = GetWorkshopStationDBOBJ.StationName;

                                GetWorkshopStationResponse.TeamName = GetWorkshopStationDBOBJ.TeamId != null ? GetWorkshopStationDBOBJ.Team.Name : null;

                                GetWorkshopStationResponse.BranchName = GetWorkshopStationDBOBJ.BranchId != null ? GetWorkshopStationDBOBJ.Branch.Name : null;

                                GetWorkshopStationResponse.BranchId = GetWorkshopStationDBOBJ.BranchId;

                                GetWorkshopStationResponse.TeamId = GetWorkshopStationDBOBJ.TeamId;

                                GetWorkshopStationResponse.StationWorkOrdersCount = _unitOfWork.ProjectFabricationWorkshopStationHistories.FindAll(a => a.ProjectWorkshopStationId == GetWorkshopStationResponse.ID).Count();




                                GetWorkshopStationList.Add(GetWorkshopStationResponse);
                            }



                        }

                    }

                }
                response.WorkshopStationResponseList = GetWorkshopStationList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public GetWorkshopStationsItemResponse GetWorkshopStationsItemPerId(int WorkShopID)
        {
            GetWorkshopStationsItemResponse response = new GetWorkshopStationsItemResponse();
            response.Result = true;
            response.Errors = new List<Error>();


            try
            {



                var GetWorkshopStationObject = new WorkshopStationsItemResponseData();




                if (response.Result)
                {

                    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    var GetWorkshopStationPerIDDB = _unitOfWork.WorkshopStations.Find(a => a.Id == WorkShopID);


                    if (GetWorkshopStationPerIDDB != null)
                    {


                        var GetWorkshopStationResponse = new WorkshopStationsItemResponseData();

                        GetWorkshopStationResponse.ID = (int)GetWorkshopStationPerIDDB.Id;

                        GetWorkshopStationResponse.Location = GetWorkshopStationPerIDDB.Location;

                        GetWorkshopStationResponse.StationName = GetWorkshopStationPerIDDB.StationName;

                        GetWorkshopStationResponse.TeamName = GetWorkshopStationPerIDDB.TeamId != null ? GetWorkshopStationPerIDDB.Team.Name : null;

                        GetWorkshopStationResponse.BranchName = GetWorkshopStationPerIDDB.BranchId != null ? GetWorkshopStationPerIDDB.Branch.Name : null;

                        GetWorkshopStationResponse.BranchId = GetWorkshopStationPerIDDB.BranchId;

                        GetWorkshopStationResponse.TeamId = GetWorkshopStationPerIDDB.TeamId;

                        GetWorkshopStationResponse.StationWorkOrdersCount = _unitOfWork.ProjectFabricationWorkshopStationHistories.FindAll(a => a.ProjectWorkshopStationId == GetWorkshopStationResponse.ID).Count();


                        response.WorkshopStationsItemResponseData = GetWorkshopStationResponse;




                    }
                    else
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Work Shop ID.";
                        response.Errors.Add(error);
                        return response;
                    }

                }



                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }


        public GetTeamUsersSelectResponse GetTeamUsersSelect(string SearchKey )
        {
            GetTeamUsersSelectResponse response = new GetTeamUsersSelectResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            
            if (!string.IsNullOrEmpty(SearchKey))
            {
                
                SearchKey = HttpUtility.UrlDecode(SearchKey);
            }

            try
            {

                var GetTeamUsersSelectList = new List<TeamUsersSelectResponseData>();

                if (response.Result)
                {
                    var usersListDbQuery = _unitOfWork.VUserInfos.FindAllQueryable(a => true);
                    if (SearchKey != null)
                    {
                        usersListDbQuery = usersListDbQuery.Where(a => a.FirstName.Contains(SearchKey) || a.MiddleName.Contains(SearchKey) || a.LastName.Contains(SearchKey) || a.UserJobTitle.Contains(SearchKey) || a.UserDepartmentName.Contains(SearchKey)).AsQueryable();
                    }

                    var usersListDB = usersListDbQuery.ToList();

                    if (usersListDB != null && usersListDB.Count > 0)
                    {

                        foreach (var usersListDBOBJ in usersListDB)
                        {
                            var usersListDBResponse = new TeamUsersSelectResponseData();

                            usersListDBResponse.ID = (int)usersListDBOBJ.Id;

                            usersListDBResponse.FirstName = usersListDBOBJ.FirstName;

                            usersListDBResponse.LastName = usersListDBOBJ.LastName;

                            usersListDBResponse.MiddleName = usersListDBOBJ.MiddleName;

                            usersListDBResponse.JobTitleID = usersListDBOBJ.JobTitleId;

                            usersListDBResponse.DepartmentName = usersListDBOBJ.UserDepartmentName;




                            GetTeamUsersSelectList.Add(usersListDBResponse);
                        }



                    }

                }


                response.TeamUsersSelectResponseDataList = GetTeamUsersSelectList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public BaseResponseWithID DeleteWorkshopStation(WorkShopStationData Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (Response.Result)
                    {
                        var modifiedUser = _unitOfWork.Users.GetById(UserID);

                        if (Request.ID != null && Request.ID != 0)
                        {
                            var WorkStationDB = _unitOfWork.WorkshopStations.GetById(Request.ID);
                            if (WorkStationDB != null)
                            {
                                // Update
                                //var WorkStationDBUpdate = _Context.proc_WorkshopStationUpdate(Request.ID,
                                //                                                                   WorkStationDB.StationName,
                                //                                                                   WorkStationDB.Location,
                                //                                                                   WorkStationDB.BranchId,
                                //                                                                   WorkStationDB.TeamId,
                                //                                                                   false,
                                //                                                                   WorkStationDB.CreatedBy,
                                //                                                                   WorkStationDB.CreationDate,
                                //                                                                   validation.userID,
                                //                                                                   DateTime.Now


                                //                                                                    );


                                WorkStationDB.Active = false;
                                WorkStationDB.ModifiedBy = UserID;
                                WorkStationDB.ModifiedDate = DateTime.Now;

                                _unitOfWork.Complete();

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Work Station  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithID DeleteProjectWorkshopStation(ProjectWorkShopStationData Request,long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(validation.userID);

                        if (Request.ID != null && Request.ID != 0)
                        {
                            var ProjectWorkStationDB = _unitOfWork.ProjectWorkshopStations.GetById(Request.ID);
                            if (ProjectWorkStationDB != null)
                            {

                                //Deactivate Project Workshop Station
                                //var ProjectStationUpdate = _Context.proc_ProjectWorkshopStationUpdate(Request.ID,
                                //                ProjectWorkStationDB.WorkshopStationId,
                                //                ProjectWorkStationDB.ProjectId,
                                //                ProjectWorkStationDB.Sequence,
                                //                false,
                                //                ProjectWorkStationDB.CreatedBy,
                                //                ProjectWorkStationDB.CreationDate,
                                //                validation.userID,
                                //                DateTime.Now.Date
                                //                );

                                ProjectWorkStationDB.Active = false;
                                ProjectWorkStationDB.ModifiedBy = UserID;
                                ProjectWorkStationDB.ModifiedDate  = DateTime.Now;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Project Work Station  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }


                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Project Work Station  Doesn't Exist!!";
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithID AddEditProjectContactPerson(ProjectContactPersonData Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }








                    if (Request.ProjectID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ProjectID Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Request.CountryID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "CountryID Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Request.GovernorateID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "GovernorateID Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (string.IsNullOrEmpty(Request.Address))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Address Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(Request.ProjectContactPersonName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Project Contact Person Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(Request.ProjectContactPersonMobile))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Project Contact Person Mobile Is Mandatory";
                        Response.Errors.Add(error);
                    }







                    if (Response.Result)
                    {
                        var modifiedUser = _unitOfWork.Users.GetById(UserID);

                        if (Request.ID != null && Request.ID != 0)
                        {
                            var ProjectContactPersonDB = _unitOfWork.ProjectContactPersons.GetById(Request.ID??0);
                            if (ProjectContactPersonDB != null)
                            {
                                // Update
                                //var ProjectContactPersonUpdate = _Context.proc_ProjectContactPersonUpdate(Request.ID,
                                //                                                                   Request.ProjectID,
                                //                                                                   Request.CountryID,
                                //                                                                   Request.GovernorateID,
                                //                                                                   Request.AreaID,
                                //                                                                   Request.Address,
                                //                                                                   Request.ProjectContactPersonName,
                                //                                                                   Request.ProjectContactPersonMobile,
                                //                                                                   true,
                                //                                                                   ProjectContactPersonDB.CreatedBy,
                                //                                                                   ProjectContactPersonDB.CreationDate,
                                //                                                                   UserID,
                                //                                                                   DateTime.Now


                                //                                                                    );

                                //var ProjectContactPersonUpdate = new ProjectContactPerson();
                                ProjectContactPersonDB.ProjectId = Request.ProjectID;
                                ProjectContactPersonDB.CountryId = Request.CountryID;
                                ProjectContactPersonDB.GovernorateId = Request.GovernorateID;
                                ProjectContactPersonDB.AreaId = Request.AreaID;
                                ProjectContactPersonDB.Address = Request.Address;
                                ProjectContactPersonDB.ProjectContactPersonName = Request.ProjectContactPersonName;
                                ProjectContactPersonDB.ProjectContactPersonMobile = Request.ProjectContactPersonMobile;
                                ProjectContactPersonDB.Active = true;
                                ProjectContactPersonDB.ModifiedBy = UserID;
                                ProjectContactPersonDB.Modified = DateTime.Now;

                                var updated = _unitOfWork.Complete();

                                if (updated > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = Request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Project Contact Person!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Project Contact Person Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            //ObjectParameter ProjectContactPersonID = new ObjectParameter("ID", typeof(long));
                            //var ProjectContactPersonInsert = _Context.proc_ProjectContactPersonInsert(ProjectContactPersonID,
                            //                                                                       Request.ProjectID,
                            //                                                                       Request.CountryID,
                            //                                                                       Request.GovernorateID,
                            //                                                                       Request.AreaID,
                            //                                                                       Request.Address,
                            //                                                                       Request.ProjectContactPersonName,
                            //                                                                       Request.ProjectContactPersonMobile,
                            //                                                                       true,
                            //                                                                       validation.userID,
                            //                                                                       DateTime.Now,
                            //                                                                       null,
                            //                                                                       null
                            //                                                               );

                            var ProjectContactPersonInsert = new ProjectContactPerson();
                            ProjectContactPersonInsert.ProjectId = Request.ProjectID;
                            ProjectContactPersonInsert.CountryId = Request.CountryID;
                            ProjectContactPersonInsert.GovernorateId = Request.GovernorateID;
                            ProjectContactPersonInsert.AreaId = Request.AreaID;
                            ProjectContactPersonInsert.Address = Request.Address;
                            ProjectContactPersonInsert.ProjectContactPersonName = Request.ProjectContactPersonName;
                            ProjectContactPersonInsert.ProjectContactPersonMobile = Request.ProjectContactPersonMobile;
                            ProjectContactPersonInsert.Active = true;
                            ProjectContactPersonInsert.CreatedBy = UserID;
                            ProjectContactPersonInsert.CreationDate = DateTime.Now;

                            _unitOfWork.ProjectContactPersons.Add(ProjectContactPersonInsert);
                            var inserted = _unitOfWork.Complete();


                            if (inserted > 0)
                            {
                                
                                Response.ID = ProjectContactPersonInsert.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert Project Contact Person!!";
                                Response.Errors.Add(error);
                            }
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetStationReceivedWorkOrdersResponse GetStationReceivedWorkOrders(int? StationID, int CurrentPage = 1, int NumberOfItemsPerPage = 10)
        {
            GetStationReceivedWorkOrdersResponse response = new GetStationReceivedWorkOrdersResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            //int StationID = 0;
            //if (!string.IsNullOrEmpty(headers["StationID"]) && int.TryParse(headers["StationID"], out StationID))
            //{
            //    int.TryParse(headers["StationID"], out StationID);
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


            try
            {





                if (response.Result)
                {
                    GetStationReceivedWorkOrdersResponse recievedFabOrdersVM = new GetStationReceivedWorkOrdersResponse();


                    if (StationID != 0)
                    {
                        recievedFabOrdersVM.WorkshopStation = (from station in _unitOfWork.WorkshopStations.FindAll(a => a.Id == StationID)
                                                               select new WorkshopStationForList
                                                               {
                                                                   ID = station.Id,
                                                                   StationName = station.StationName
                                                               }).FirstOrDefault();

                        var ReceivedWorkOrdersIdsList = _unitOfWork.ProjectFabricationWorkshopStationHistories.FindAll(a => a.ProjectWorkshopStation.WorkshopStation.Id == StationID && a.Active == true).Select(a => a.FabricationOrderId).ToList();

                        var receivedFabticationOrdersQuery = _unitOfWork.VProjectFabricationProjectOfferEntities.FindAllQueryable(a => ReceivedWorkOrdersIdsList.Contains((long)a.Id)).OrderBy(a => a.Id).AsQueryable();


                        var StationReceivedWorkOrdersPagingList = PagedList<VProjectFabricationProjectOfferEntity>.Create(receivedFabticationOrdersQuery.OrderByDescending(x => x.Id), CurrentPage, NumberOfItemsPerPage);

                        response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = CurrentPage,
                            TotalPages = StationReceivedWorkOrdersPagingList.TotalPages,
                            ItemsPerPage = NumberOfItemsPerPage,
                            TotalItems = StationReceivedWorkOrdersPagingList.TotalCount
                        };



                        if (StationReceivedWorkOrdersPagingList != null)
                        {
                            var StationReceivedWorkOrdersList = new List<ReceivedFabticationOrder>();

                            foreach (var StationReceivedWorkOrdersObjDB in StationReceivedWorkOrdersPagingList)
                            {
                                ReceivedFabticationOrder workOrderItem = new ReceivedFabticationOrder
                                {
                                    FabricationOrderId = StationReceivedWorkOrdersObjDB.Id != null ? StationReceivedWorkOrdersObjDB.Id.ToString() : "",
                                    ProjectId = StationReceivedWorkOrdersObjDB.ProjectId != 0 ? StationReceivedWorkOrdersObjDB.ProjectId.ToString() : "",
                                    ReceivingDate = StationReceivedWorkOrdersObjDB.StartDate ?? DateTime.Now,
                                    EndDate = StationReceivedWorkOrdersObjDB.EndDate ?? DateTime.Now,
                                    RemainingDays = StationReceivedWorkOrdersObjDB.EndDate != null ? (long)(DateTime.Parse(StationReceivedWorkOrdersObjDB.EndDate.ToString()) - DateTime.Now).TotalDays : 0,
                                    FabricationProgress = StationReceivedWorkOrdersObjDB.Progress != null ? StationReceivedWorkOrdersObjDB.Progress.ToString() : "",
                                    QualityInspection = StationReceivedWorkOrdersObjDB.PassQc != null ? Convert.ToBoolean(StationReceivedWorkOrdersObjDB.PassQc.ToString()) ? "Passed" : "Not Passed" : "",
                                    Revesion = StationReceivedWorkOrdersObjDB.Revision != null ? StationReceivedWorkOrdersObjDB.Revision.ToString() : "",
                                    FabricationOrderNumber = StationReceivedWorkOrdersObjDB.FabNumber != null ? Convert.ToInt64(StationReceivedWorkOrdersObjDB.FabNumber.ToString()) : 0,
                                    CivilRequestStatus = StationReceivedWorkOrdersObjDB.CivilRequestStatus != null ? Convert.ToBoolean(StationReceivedWorkOrdersObjDB.CivilRequestStatus.ToString()) : false,
                                    RequireFinFeedBack = StationReceivedWorkOrdersObjDB.RequireFinFeedBack != null ? Convert.ToBoolean(StationReceivedWorkOrdersObjDB.RequireFinFeedBack.ToString()) : false,
                                    FinFeedBackResult = StationReceivedWorkOrdersObjDB.FinFeedBackResult != null ? StationReceivedWorkOrdersObjDB.FinFeedBackResult.ToString() : "",
                                    RequireApprovalFeedBack = StationReceivedWorkOrdersObjDB.RequireApprovalFeedBack != null ? Convert.ToBoolean(StationReceivedWorkOrdersObjDB.RequireApprovalFeedBack.ToString()) : false,
                                    ApprovalFeedBackResult = StationReceivedWorkOrdersObjDB.ApprovalFeedBackResult != null ? StationReceivedWorkOrdersObjDB.ApprovalFeedBackResult.ToString() : "",
                                    ProjectName = StationReceivedWorkOrdersObjDB.ProjectName != null ? StationReceivedWorkOrdersObjDB.ProjectName.ToString() : "",
                                    ClientName = StationReceivedWorkOrdersObjDB.ClientName != null ? StationReceivedWorkOrdersObjDB.ClientName.ToString() : ""

                                };

                                if (StationReceivedWorkOrdersObjDB.Id != null && StationReceivedWorkOrdersObjDB.Id != 0)
                                {
                                    var projectFabricationTotalHours = _unitOfWork.VProjectFabricationTotalHours.FindAll(a => a.ProjectFabId == StationReceivedWorkOrdersObjDB.Id).ToList();






                                    if (projectFabricationTotalHours != null && projectFabricationTotalHours.Count > 0)
                                    {

                                        foreach (var projectFabricationTotalHoursOBJ in projectFabricationTotalHours)
                                        {
                                            workOrderItem.TotalWorkingHours = projectFabricationTotalHoursOBJ.TotalHours != null ? (decimal)projectFabricationTotalHoursOBJ.TotalHours : 0;
                                        }


                                    }

                                }





                                StationReceivedWorkOrdersList.Add(workOrderItem);
                            }

                            response.ReceivedFabticationOrderDataList = StationReceivedWorkOrdersList;


                        }

                    }


                }


                //response.TeamUsersSelectResponseDataList = GetTeamUsersSelectList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public BaseResponseWithID AddNewJobOrderStations(JobOrderStationsData Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                     
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (Response.Result)
                    {
                        var modifiedUser = _unitOfWork.Users.GetById(UserID);//Common.GetUserName(validation.userID);

                        if (Request.projectId != 0)
                        {
                            if (Request.StationsIdsList != null)
                            {
                                if (Request.StationsIdsList.Count() > 0)
                                {
                                    foreach (var station in Request.StationsIdsList)
                                    {
                                        if (station != 0)
                                        {
                                            var LastSequence = 0;
                                            var StationsDBList = _Context.ProjectWorkshopStations.Where(a => a.ProjectId == Request.projectId).ToList();
                                            if (StationsDBList != null && StationsDBList.Count() > 0)
                                            {
                                                LastSequence = StationsDBList.Max(a => a.Sequence);
                                            }
                                            //ObjectParameter ProjectStationId = new ObjectParameter("ID", typeof(long));
                                            //var ProjectStationInsertion = _Context.proc_ProjectWorkshopStationInsert(ProjectStationId,
                                            //                                                                station,
                                            //                                                                Request.projectId,
                                            //                                                                LastSequence + 1,
                                            //                                                                true,
                                            //                                                                validation.userID,
                                            //                                                                DateTime.Now,
                                            //                                                                validation.userID,
                                            //                                                                DateTime.Now
                                            //                                                                );

                                            var ProjectStationInsertion = new ProjectWorkshopStation();
                                            ProjectStationInsertion.ProjectId = Request.projectId;
                                            ProjectStationInsertion.Sequence = LastSequence + 1;
                                            ProjectStationInsertion.Active = true;
                                            ProjectStationInsertion.CreatedBy = UserID;
                                            ProjectStationInsertion.CreationDate = DateTime.Now;
                                            ProjectStationInsertion.ModifiedBy = UserID;
                                            ProjectStationInsertion.ModifiedDate = DateTime.Now;

                                            _unitOfWork.ProjectWorkshopStations.Add(ProjectStationInsertion);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }

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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<MyProjectsCRMDashboardResponse> GetMyProjectsDetailsCRM([FromHeader] GetMyProjectsDetailsCRMHeaders headers)
        {
            MyProjectsCRMDashboardResponse Response = new MyProjectsCRMDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var ProjectsDbQuery = _Context.VProjectSalesOffers.AsQueryable();
                    var SellingProductsDbQuery = _Context.VSalesOfferProductSalesOffers.Where(a => a.Status == "Closed" && a.Active == true).AsQueryable();

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
                    SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.CreationDate >= StartDate && a.CreationDate < EndDate);

                    if (headers.BranchId != 0)
                    {
                        ProjectsDbQuery = ProjectsDbQuery.Where(a => a.SalesPersonBranchId == headers.BranchId);
                        SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.BranchId == headers.BranchId);
                    }

                    if (headers.SalesPersonId != 0)
                    {
                        ProjectsDbQuery = ProjectsDbQuery.Where(a => a.SalesPersonId == headers.SalesPersonId);
                        SellingProductsDbQuery = SellingProductsDbQuery.Where(a => a.SalesPersonId == headers.SalesPersonId);
                    }

                    var ProjectsDbList = await ProjectsDbQuery.ToListAsync();
                    var totalClosedProjectsCount = ProjectsDbList.Where(a => a.Closed == true && a.Active == true).Count();

                    Response.TotalClosed = totalClosedProjectsCount;
                    Response.TotalProjects = ProjectsDbList.Count();

                    var GroupedProjects = ProjectsDbList.GroupBy(p => p.OfferType).ToList();

                    //Get All Warrenty Projects
                    var WarrantyProjects = ProjectsDbList.Where(p => p.OfferType == "New Maintenance Offer" && p.MaintenanceType == "Warranty").ToList();
                    var WarrantyProjectsGrp = WarrantyProjects.GroupBy(a => a.MaintenanceType).ToList();
                    GroupedProjects.Add(WarrantyProjectsGrp.FirstOrDefault());


                    //-----------------------------------------new add ----------------------------------------------------
                    var projectsIdList = ProjectsDbList.Select(a => a.Id).ToList();
                    var clientAccounts = await _Context.ClientAccounts.Where(x => x.ProjectId != null ? projectsIdList.Contains((long)x.ProjectId) : false).ToListAsync();

                    var ProjectReturnList = _Context.VProjectReturnSalesOffers.ToList();

                    var ProjectsTypesList = new List<MyProjectsCRM>();

                    foreach (var projectsGrp in GroupedProjects)
                    {
                        if (projectsGrp != null)
                        {
                            if (projectsGrp.Key == "Warranty")
                            {

                            }
                            else
                            {
                                var openProjectsCount = projectsGrp.Where(a => a.Closed == false && a.Active == true).Count();
                                var closedProjectsCount = projectsGrp.Where(a => a.Closed == true && a.Active == true).Count();
                                var deactivatedProjectsCount = projectsGrp.Where(a => a.Active == false).Count();

                                if (projectsGrp.Key == "New Internal Order")
                                {
                                    Response.TotalOpenInternal = openProjectsCount;
                                    Response.TotalClosedInternal = closedProjectsCount;
                                }
                                else
                                {
                                    decimal totalProjectsCost = 0;
                                    decimal totalProjectsCollectedCost = 0;
                                    decimal totalProjectsCollectedPercentage = 0;


                                    foreach (var project in projectsGrp)
                                    {
                                        var ProjectCost = project.ExtraCost + project.FinalOfferPrice;
                                        totalProjectsCost += ProjectCost ?? 0;

                                        //decimal ProjectCollected = 0;

                                        var sumProjectCollected = clientAccounts.Where(a => a.ProjectId == project.Id && a.AmountSign == "plus").Select(a => a.Amount).DefaultIfEmpty(0).Sum();
                                        var subProjectCollected = clientAccounts.Where(a => a.ProjectId == project.Id && a.AmountSign == "minus").Select(a => a.Amount).DefaultIfEmpty(0).Sum();


                                        //var clientAccounts = await _Context.ClientAccounts.Where(x => x.ProjectId == project.Id).ToListAsync();
                                        //foreach (var clientAccount in clientAccounts)
                                        //{
                                        //    if (clientAccount.AmountSign == "plus")
                                        //    {
                                        //        ProjectCollected = ProjectCollected + clientAccount.Amount;
                                        //    }
                                        //    else if (clientAccount.AmountSign == "minus")
                                        //    {
                                        //        ProjectCollected = ProjectCollected - clientAccount.Amount;
                                        //    }
                                        //}

                                        totalProjectsCollectedCost += sumProjectCollected - subProjectCollected;
                                    }
                                    var ProjectReturn = ProjectReturnList.Where(a => a.ParentOfferType == projectsGrp.Key).ToList();
                                    if (ProjectReturn != null && ProjectReturn.Count > 0)
                                    {
                                        var ProjectReturnCost = ProjectReturn.Sum(a => a.ReturnFinalOfferPrice);
                                        totalProjectsCost -= (decimal)ProjectReturnCost;
                                    }

                                    if (totalProjectsCost != 0)
                                    {
                                        totalProjectsCollectedPercentage = totalProjectsCollectedCost / totalProjectsCost * 100;
                                    }
                                    var ProjectTypeObj = new MyProjectsCRM
                                    {
                                        ProjectType = projectsGrp.Key,
                                        TotalOpen = openProjectsCount,
                                        TotalClosed = closedProjectsCount,
                                        TotalDeactivated = deactivatedProjectsCount,
                                        TotalPrice = totalProjectsCost,
                                        TotalCollected = totalProjectsCollectedCost,
                                        CollectedPercentage = string.Format("{0:0.0}", totalProjectsCollectedPercentage) + "%"
                                    };

                                    ProjectsTypesList.Add(ProjectTypeObj);
                                }
                            }

                            //Add Direct Sales to New Project Offer
                            var NewProjectOffer = ProjectsTypesList.Where(a => a.ProjectType == "New Project Offer").FirstOrDefault();
                            if (NewProjectOffer == null)
                            {
                                NewProjectOffer = new MyProjectsCRM();
                            }
                            var DirectSales = ProjectsTypesList.Where(a => a.ProjectType == "Direct Sales").FirstOrDefault();
                            if (DirectSales != null)
                            {
                                NewProjectOffer.TotalOpen += DirectSales.TotalOpen;
                                NewProjectOffer.TotalClosed += DirectSales.TotalClosed;
                                NewProjectOffer.TotalDeactivated += DirectSales.TotalDeactivated;
                                NewProjectOffer.TotalPrice += DirectSales.TotalPrice;
                                NewProjectOffer.TotalCollected += DirectSales.TotalCollected;
                                NewProjectOffer.CollectedPercentage = NewProjectOffer.TotalPrice > 0 ? string.Format("{0:0.0}", NewProjectOffer.TotalCollected / NewProjectOffer.TotalPrice * 100) + "%" : "0%";
                            }
                            ProjectsTypesList.Remove(DirectSales);
                        }
                    }

                    if (ProjectsTypesList.Where(a => a.ProjectType == "New Project Offer").FirstOrDefault() == null)
                    {
                        var ProjectTypeObj = new MyProjectsCRM
                        {
                            ProjectType = "New Project Offer",
                            TotalOpen = 0,
                            TotalClosed = 0,
                            TotalDeactivated = 0,
                            TotalPrice = 0,
                            TotalCollected = 0,
                            CollectedPercentage = "0.0%"
                        };

                        ProjectsTypesList.Add(ProjectTypeObj);
                    }

                    if (ProjectsTypesList.Where(a => a.ProjectType == "New Maintenance Offer").FirstOrDefault() == null)
                    {
                        var ProjectTypeObj = new MyProjectsCRM
                        {
                            ProjectType = "New Maintenance Offer",
                            TotalOpen = 0,
                            TotalClosed = 0,
                            TotalDeactivated = 0,
                            TotalPrice = 0,
                            TotalCollected = 0,
                            CollectedPercentage = "0.0%"
                        };

                        ProjectsTypesList.Add(ProjectTypeObj);
                    }

                    if (ProjectsTypesList.Where(a => a.ProjectType == "New Rent Offer").FirstOrDefault() == null)
                    {
                        var ProjectTypeObj = new MyProjectsCRM
                        {
                            ProjectType = "New Rent Offer",
                            TotalOpen = 0,
                            TotalClosed = 0,
                            TotalDeactivated = 0,
                            TotalPrice = 0,
                            TotalCollected = 0,
                            CollectedPercentage = "0.0%"
                        };

                        ProjectsTypesList.Add(ProjectTypeObj);
                    }
                    Response.ProjectsTypesList = ProjectsTypesList;

                    var totalProjectsPrice = ProjectsTypesList.Sum(a => a.TotalPrice);
                    var totalProjectsCollected = ProjectsTypesList.Sum(a => a.TotalCollected);


                    //var test = _Context.Projects.GroupBy(x => x.ProjectManager).ToList();
                    var SellingProductsDbList = (await SellingProductsDbQuery.Where(a => a.OfferType != "Sales Return" && a.Name != null && a.InventoryItemId != null).ToListAsync()).GroupBy(a => new { a.InventoryItemId, a.Name });
                    //var SellingProductsDbList = await SellingProductsDbQuery.Where(a => a.OfferType != "Sales Return" && a.Name != null && a.InventoryItemId != null).GroupBy(a => new { a.InventoryItemId, a.Name }).ToListAsync();

                    var ReturnSellingProductsDbList = await SellingProductsDbQuery.Where(a => a.OfferType == "Sales Return").ToListAsync();

                    var SellingProductsList = new List<SellingProductsCRM>();
                    foreach (var SellingProduct in SellingProductsDbList)
                    {
                        if (SellingProduct != null)
                        {
                            var TotalReturnPrice = ReturnSellingProductsDbList.Where(a => a.InventoryItemId == SellingProduct.Key.InventoryItemId).Sum(a => a.ItemPrice * (decimal)a.Quantity);
                            var TotalReturnCount = ReturnSellingProductsDbList.Where(a => a.InventoryItemId == SellingProduct.Key.InventoryItemId).Count();
                            var SellingProductObj = new SellingProductsCRM()
                            {
                                ProductId = SellingProduct.Key.InventoryItemId ?? 0,
                                ProductName = SellingProduct.Key.Name != null ? SellingProduct.Key.Name.Trim() : "",
                                ProductCategoryId = SellingProduct.Select(a => a.InventoryItemCategoryId).FirstOrDefault(),
                                SoldCount = SellingProduct.Count() - TotalReturnCount,
                                TotalSoldPrice = SellingProduct.Sum(a => a.ItemPrice * (decimal)a.Quantity) - (TotalReturnPrice ?? 0) ?? 0
                            };

                            SellingProductsList.Add(SellingProductObj);
                        }
                    }
                    SellingProductsList = SellingProductsList.OrderByDescending(a => a.SoldCount).ToList();
                    Response.TopSellingProducts = SellingProductsList;

                    var CategoryGroupedSellingProducts = SellingProductsList.GroupBy(a => a.ProductCategoryId).ToList();
                    var SellingProductsCategoryList = new List<TopSellingProductsCategoryGrouped>();
                    foreach (var GC in CategoryGroupedSellingProducts)
                    {
                        var SellingProductCategoryObj = new TopSellingProductsCategoryGrouped()
                        {
                            CategoryId = GC.Key,
                            CategoryName = _Context.InventoryItemCategories.Where(a => a.Id == GC.Key).Select(a => a.Name).FirstOrDefault().Trim(),
                            TopSellingProductsList = GC.ToList(),
                            TotalDealsCount = GC.Sum(a => a.SoldCount),
                            TotalDealsPrice = GC.Sum(a => a.TotalSoldPrice)
                        };

                        SellingProductsCategoryList.Add(SellingProductCategoryObj);
                    }
                    Response.TopSellingProductsCategoryGrouped = SellingProductsCategoryList.OrderBy(a => a.CategoryName).ToList();

                    Response.TotalProjectsPrice = totalProjectsPrice;
                    Response.TotalProjectsCollected = totalProjectsCollected;
                    Response.TotalCollectedPercentage = totalProjectsPrice > 0 ? string.Format("{0:0.0}", totalProjectsCollected / totalProjectsPrice * 100) + "%" : "0%";
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
