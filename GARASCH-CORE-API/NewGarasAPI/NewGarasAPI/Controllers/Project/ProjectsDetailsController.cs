using iTextSharp.text.pdf.parser;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Project.UsedInResponses;
using NewGarasAPI.Models.ProjectFabrication.Headers;
using NewGarasAPI.Models.ProjectsDetails;
using NewGarasAPI.Models.ProjectsDetails.Responses;
using NewGarasAPI.Models.ProjectsDetails.UsedInResponses;
using NewGarasAPI.Models.ProjectsDetails.ViewModels;
using QRCoder;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace NewGarasAPI.Controllers.Project
{
    [Route("/[controller]")]
    [ApiController]
    public class ProjectsDetailsController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        public ProjectsDetailsController(IWebHostEnvironment host,ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
        }

        [HttpGet("GetGeneralInfoAndProjectHistory")]
        public GeneralInfoAndProjectHistoryResponse GetGeneralInfoAndProjectHistory([FromHeader] long ProjectId)
        {

            GeneralInfoAndProjectHistoryResponse response = new GeneralInfoAndProjectHistoryResponse()
            {
                result = true,
                errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.errors = validation.errors;
            response.result = validation.result;
            #endregion
            try
            {
                if (response.result)
                {
                    var dataQuery = _Context.Projects
                    .Include(x => x.SalesOffer).ThenInclude(y => y.SalesPerson)
                    .Include(x => x.SalesOffer).ThenInclude(y => y.SalesOfferAttachments)
                    .AsQueryable();

                    #region headers
                    if (ProjectId != 0)
                    {
                        dataQuery = dataQuery.Where(a => a.Id == ProjectId);
                    }
                    #endregion

                    var data = dataQuery.ToList().FirstOrDefault();

                    if (data != null)
                    {
                        response.ProjectId = data.Id;
                        response.ProjectManagerId = data.ProjectManagerId;
                        response.StartDate = data.StartDate.ToShortDateString();
                        response.EndDate = data.EndDate.ToShortDateString();
                        response.ProjectLocation = data.SalesOffer.ProjectLocation;
                        response.Note = data.SalesOffer.Note;
                        response.ContactPersonName = data.SalesOffer.ContactPersonName;
                        response.ContactPersonEmail = data.SalesOffer.ContactPersonEmail;
                        response.ContactPersonMobile = data.SalesOffer.ContactPersonMobile;

                        //SalesOfferAttachment
                        if (data.ProjectSerial != null)
                        {

                            //QRCodeGenerator _qrCode2 = new QRCodeGenerator();
                            //QRCodeData _qrCodeData2 = _qrCode2.CreateQrCode(data.ProjectSerial, QRCodeGenerator.ECCLevel.Q);
                            //QRCode qrCode2 = new QRCode(_qrCodeData2);
                            //Bitmap qrCodeImage2 = qrCode2.GetGraphic(20);

                            //var ButmapQRCodeIMGFabSerial = Common.BitmapToBytesCode(qrCodeImage2);

                            //response.SerialQR = ButmapQRCodeIMGFabSerial;
                            response.OfferSerial = data.SalesOffer.OfferSerial;
                            response.ProjectSerial = data.ProjectSerial;
                        }

                        if (data.Closed != null && data.Active != null)
                        {
                            if (data.Closed == false && data.Active == true) { response.status = "Open"; }
                            if (data.Closed == true && data.Active == true) { response.status = "Closed"; }
                            if (data.Active == false) { response.status = "Deactivated"; }
                        }
                        response.SelasRepId = data.SalesOffer.SalesPersonId;
                        response.SelasRepFullName = data.SalesOffer.SalesPerson.FirstName + " " + data.SalesOffer.SalesPerson.MiddleName + " " + data.SalesOffer.SalesPerson.LastName;


                        if (data.SalesOffer.SalesOfferAttachments != null)
                        {

                            foreach (var Attachment in data.SalesOffer.SalesOfferAttachments)
                            {
                                string AttachmentPathGot = Attachment.AttachmentPath;

                                if (Attachment.AttachmentPath.ToString().StartsWith("~"))
                                {
                                    AttachmentPathGot = Attachment.AttachmentPath.ToString().Remove(0, 1);
                                }

                                if (Attachment.Category.ToString().ToLower() == "offerdetails")
                                {
                                    response.OfferAttachmentPath = AttachmentPathGot;
                                }
                                else if (Attachment.Category.ToString().ToLower() == "contract or purchase order")
                                {
                                    response.ContractAttachmentPath = AttachmentPathGot;
                                }
                            }
                        }

                        response.OfferLink = "~/Offers/ClientApproval.aspx?" + data.SalesOfferId;

                        var TableData = _Context.VSalesOfferProducts.Where(a => a.OfferId == data.SalesOfferId).ToList();

                        var OfferItemsIds = TableData.Select(a => a.Id).ToList();

                        var Boms = _Context.Boms.Where(a => OfferItemsIds.Contains(a.Id) && a.Active == true).ToList();

                        List<ProductVM> productList = new List<ProductVM>();

                        if (TableData != null)
                        {
                            foreach (var product in TableData)
                            {
                                ProductVM productVM = new ProductVM();
                                if (Boms != null)
                                {
                                    var CurrentBom = Boms.Where(a => a.OfferItemId == product.Id).FirstOrDefault();
                                    if (CurrentBom != null)
                                    {
                                        productVM.BOMID = CurrentBom.Id;
                                        productVM.BOMName = CurrentBom.Name;
                                        productVM.BOMUrl = "/BOM/ViewAssignedBOM?BOMID=" + CurrentBom.Id;
                                    }
                                }

                                productVM.SalesOfferProductID = product.Id;
                                productVM.CreationDate = product.CreationDate;
                                productVM.InventoryItemName = product.InventoryItemName;
                                productVM.InventoryItemCategoryName = product.CategoryName;
                                productVM.Quantity = product.Quantity ?? 0;
                                productVM.ItemPrice = product.ItemPrice ?? 0;
                                productVM.TotalPrice = productVM.ItemPrice * (decimal)productVM.Quantity;
                                productVM.ConfirmReceivingComment = product.ConfirmReceivingComment;
                                productVM.ConfirmReceivingQuantity = product.ConfirmReceivingQuantity ?? 0;

                                productList.Add(productVM);
                            }
                        }

                        response.ProductList = productList;

                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.errors.Add(error);

                return response;
            }


        }


        [HttpGet("GetInstallationOrders")]
        public GetInstallationOrdersResponse GetInstallationOrders([FromHeader] GetInstallatioAndFabricationOrdersCardsHeader headers)
        {
            GetInstallationOrdersResponse response = new GetInstallationOrdersResponse()
            {
                result = true,
                errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.errors = validation.errors;
            response.result = validation.result;
            #endregion


            try
            {
                if (response.result)
                {
                    //Query to get the total working hours
                    var TotalWorkingHoursList = _Context.VProjectInstallationTotalHours.ToList();

                    var InsQuery = _Context.ProjectInstallations
                    .Include(a => a.Project)
                    .ThenInclude(b => b.SalesOffer)
                    .ThenInclude(c => c.Client)
                    .AsQueryable();


                    #region dealing with headers
                    if (headers.ProjectID != 0)
                    {
                        InsQuery = InsQuery.Where(a => a.ProjectId == headers.ProjectID);
                    }
                    else
                    {
                        InsQuery = InsQuery.Where(a => a.CreationDate >= headers.DateFrom && a.CreationDate <= headers.DateTo);
                    }

                    #endregion

                    var pagedInsOrdersList = PagedList<ProjectInstallation>.Create(InsQuery, headers.CurrentPage, headers.NumberOfItemsPerPage);


                    //var ProjectInstallationList = InsQuery.ToList();

                    List<ProjectInstallationCards> CardsList = new List<ProjectInstallationCards>();

                    foreach (var ProjectInstallation in pagedInsOrdersList)
                    {
                        ProjectInstallationCards card = new ProjectInstallationCards();


                        card.ProjectId = ProjectInstallation.ProjectId;
                        card.ProjectSerial = ProjectInstallation.Project.ProjectSerial;
                        card.ClientName = ProjectInstallation.Project.SalesOffer.Client.Name;
                        card.InstallationSerial = ProjectInstallation.InsOrderSerial;
                        card.InstallationOrderNumber = ProjectInstallation.InsNumber;
                        card.InstallationProgress = ProjectInstallation.Progress;
                        card.RequireSalesPersonFeedBack = ProjectInstallation.RequireSalesPersonFeedBack;
                        card.SalesPersonFeedBackResult = ProjectInstallation.SalesPersonFeedBackResult;

                        if (ProjectInstallation.PassQc)
                        {
                            card.QualityInspection = "Passed";
                        }
                        else
                        {
                            card.QualityInspection = "Not Passed";
                        }

                        if (ProjectInstallation.RequireFinFeedBack)
                        {
                            if (ProjectInstallation.FinFeedBackResult != null)
                            {
                                if (ProjectInstallation.FinFeedBackResult == "Waiting for reply")
                                {
                                    card.FinFeedBackResult = "Pending";

                                }
                                else
                                {
                                    card.FinFeedBackResult = ProjectInstallation.FinFeedBackResult;
                                }

                            }
                            else
                            {
                                card.FinFeedBackResult = "Pending";
                            }
                            card.FinFeedBackResultApproved = "Yes";
                        }
                        else
                        {
                            card.FinFeedBackResultApproved = "No";
                        }


                        if (ProjectInstallation.RequireSalesPersonFeedBack)
                        {
                            if (ProjectInstallation.SalesPersonFeedBackResult != null)
                            {
                                if (ProjectInstallation.SalesPersonFeedBackResult == "Waiting for reply")
                                {
                                    card.SalesPersonFeedBackResult = "Pending";

                                }
                                else
                                {
                                    card.SalesPersonFeedBackResult = ProjectInstallation.SalesPersonFeedBackResult;
                                }

                            }
                            else
                            {
                                card.SalesPersonFeedBackResult = "Pending";
                            }
                            card.SalesPersonFeedBackResultApproved = "Yes";
                        }
                        else
                        {
                            card.SalesPersonFeedBackResultApproved = "No";
                        }


                        if (ProjectInstallation.FullInstallationEndDate != null)
                        {
                            card.RemaningDays = (int)(DateTime.Parse(ProjectInstallation.FullInstallationEndDate.ToString()) - DateTime.Now).TotalDays;
                        }
                        else
                        {
                            card.RemaningDays = 0;
                        }

                        card.TotalWorkingHours = TotalWorkingHoursList.Where(a => a.ProjectInsId == ProjectInstallation.Id).Select(a => a.TotalHours).FirstOrDefault();

                        CardsList.Add(card);
                    }

                    response.projectInstallationCards = CardsList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.errors.Add(error);

                return response;
            }
        }


        [HttpGet("CheckToCloseProject")]
        public CheckToCloseProjectResponse CheckToCloseProject([FromHeader] long? projectId, [FromHeader] long UserId)
        {
            CheckToCloseProjectResponse response = new CheckToCloseProjectResponse()
            {
                result = true,
                errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.errors = validation.errors;
            response.result = validation.result;
            #endregion



            try
            {
                if (response.result)
                {
                    if (Common.CheckUserRole(UserId, 134, _Context))
                    {
                        var ProjectQuery = _Context.Projects
                            .Include(a => a.SalesOffer)
                            .ThenInclude(b => b.SalesOfferProducts)
                            .Where(opt => opt.Id == projectId)
                            .FirstOrDefault();

                        var SalesOfferProductIds = ProjectQuery.SalesOffer.SalesOfferProducts.Select(a => a.Id).ToList();

                        var InstallationOrdersList = _Context.ProjectInstallations.Where(a => a.ProjectId == projectId).ToList();   //momkn agyb kolo (any progress,passQc = false)wa aflter ta7t
                        var CompletedInstallationOrdersList = InstallationOrdersList.Where(a => a.PassQc == true && a.Progress == 100).ToList();
                        var CompletedInstallationOrdersIds = CompletedInstallationOrdersList.Select(a => a.Id).ToList();

                        var ProjectInstallationBOQList = _Context.ProjectInstallationBoqs
                            .Where(a => SalesOfferProductIds.Contains(a.SalesOfferProductId ?? 0)
                            && CompletedInstallationOrdersIds.Contains(a.ProjectInstallationId))
                            .ToList();


                        var FabricationOrdersList = _Context.ProjectFabrications.Where(a => a.ProjectId == projectId).ToList();
                        var CompletedFabricationOrdersList = FabricationOrdersList.Where(a => a.PassQc == true && a.Progress == 100).ToList();
                        var CompletedFabricationOrdersIds = CompletedFabricationOrdersList.Select(a => a.Id).ToList();

                        var ProjectFabricationBOQList = _Context.ProjectFabricationBoqs
                            .Where(a => SalesOfferProductIds.Contains(a.SalesOfferProductId ?? 0)
                            && CompletedFabricationOrdersIds.Contains(a.ProjectFabricationId))
                            .ToList();


                        double? quantityRemainInstallation = 0;
                        double? quantityRemainFabrication = 0;
                        double? totalQuantityInAllSalesOfferProducts = ProjectQuery.SalesOffer.SalesOfferProducts.Sum(a => a.Quantity);
                        if (ProjectQuery.SalesOffer.SalesOfferProducts != null)
                        {
                            foreach (var SalesOfferProduct in ProjectQuery.SalesOffer.SalesOfferProducts)
                            {
                                var totalQuantity = SalesOfferProduct.Quantity;
                                var completedInstallationQuantity = 0;
                                var completedFabricationQuantity = 0;

                                foreach (var InstallationItem in CompletedInstallationOrdersList)
                                {
                                    var ProjectInstallationBOQ = ProjectInstallationBOQList.Where(a => a.ProjectInstallationId == InstallationItem.Id && a.SalesOfferProductId == SalesOfferProduct.Id).FirstOrDefault();

                                    if (ProjectInstallationBOQ != null)
                                    {
                                        completedInstallationQuantity = (int)ProjectInstallationBOQ.Quantity + completedInstallationQuantity;
                                    }
                                }

                                foreach (var FabricationItem in CompletedFabricationOrdersList)
                                {
                                    var ProjectFabricationBOQ = ProjectFabricationBOQList.Where(a => a.ProjectFabricationId == FabricationItem.Id && a.SalesOfferProductId == SalesOfferProduct.Id).FirstOrDefault();
                                    if (ProjectFabricationBOQ != null)
                                    {
                                        completedFabricationQuantity = (int)ProjectFabricationBOQ.Quantity + completedFabricationQuantity;
                                    }
                                }
                                quantityRemainInstallation = totalQuantity - completedInstallationQuantity;
                                quantityRemainFabrication = totalQuantity - completedFabricationQuantity;

                                if (quantityRemainInstallation != 0)
                                {
                                    response.TotalInsProgress = (completedInstallationQuantity / totalQuantity * 100).ToString() + "%";
                                }
                                else if (quantityRemainInstallation == 0)
                                {
                                    response.TotalInsProgress = "100%";
                                }
                                if (quantityRemainFabrication != 0)
                                {
                                    response.TotalFabProgress = (completedFabricationQuantity / totalQuantity * 100).ToString() + "%";
                                }
                                else if (quantityRemainFabrication == 0)
                                {
                                    response.TotalFabProgress = "100%";
                                }
                            }
                        }

                        var OpenFabOrderList = FabricationOrdersList.Where(a => a.PassQc == false && a.Progress < 100).ToList(); //get open fab orders

                        List<OpenOrders> openFabOrdeList = new List<OpenOrders>();

                        foreach (var fabOrder in OpenFabOrderList)
                        {
                            OpenOrders openOrder = new OpenOrders();
                            openOrder.OrderId = fabOrder.Id;
                            openOrder.OrderName = fabOrder.Name;

                            openFabOrdeList.Add(openOrder);                 //list of open fabs orders
                        }

                        var OpenInsOrderList = InstallationOrdersList.Where(a => a.PassQc == false && a.Progress < 100).ToList();

                        List<OpenOrders> openInsOrdeList = new List<OpenOrders>();

                        foreach (var insOrder in OpenInsOrderList)
                        {
                            OpenOrders openOrder = new OpenOrders();
                            openOrder.OrderId = insOrder.Id;
                            openOrder.OrderName = insOrder.Name;

                            openInsOrdeList.Add(openOrder);
                        }

                        response.FabOpenOrders = openFabOrdeList;           //add the list of open fab orders to the list in the response
                        response.InsOpenOrders = openInsOrdeList;           //add the list of open ins orders to the list in the response
                        response.RemainOpenFabOrders = FabricationOrdersList.Where(a => a.PassQc == false && a.Progress < 100).Count();
                        response.RemainOpenInsOrders = InstallationOrdersList.Where(a => a.PassQc == false && a.Progress < 100).Count();
                        return response;

                    }
                    else
                    {
                        response.result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Your are not authorized to Close this project ";
                        response.errors.Add(error);

                        return response;
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                response.result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.errors.Add(error);

                return response;
            }
        }


        [HttpPost("CloseProject")]
        public async Task<BaseResponseWithID> CloseProject(long projectId)
        {
            BaseResponseWithID response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var project = await _Context.Projects.Where(pro => pro.Id == projectId).FirstOrDefaultAsync();
                    if (project != null)
                    {
                        if (project.Closed == true)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "The Project is already closed";
                            response.Errors.Add(error);

                            return response;
                        }
                        project.Closed = true;

                        var costCenters = await _Context.GeneralActiveCostCenters.Where(a => a.CategoryId == projectId).FirstOrDefaultAsync();
                        if (costCenters != null)
                        {
                            costCenters.Closed = true;
                        }

                        _Context.SaveChanges();
                    }

                    response.ID = projectId;
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddEditManagementOfMaintenanceOffer")]
        public async Task<BaseResponseWithID> AddEditManagementOfMaintenanceOffer(ManagementOfMaintenanceOfferVM model)
        {
            BaseResponseWithID response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    if (validation.userID != 0 || validation.CompanyName != null)
                    {
                        if (!Common.CheckUserRole(validation.userID, 74, _Context))
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Your are not authorized to edit mangments of this project ";
                            response.Errors.Add(error);

                            return response;
                        }
                    }


                    //var ManagementOfMaintenanceOrder =await _Context.ManagementOfMaintenanceOrders.Where(a => a.ProjectId == model.ProjectID)
                    //                                    .Include(a => a.VisitsScheduleOfMaintenances)
                    //                                    .FirstOrDefaultAsync();

                    if (model.ManagementOfMaintenanceOrderID == 0)
                    {
                        ManagementOfMaintenanceOrder maintenanceOrder = new ManagementOfMaintenanceOrder();
                        maintenanceOrder.MaintenanceOfferId = model.OfferID;
                        maintenanceOrder.ProjectId = model.ProjectID;

                        DateTime StartDate;
                        if (DateTime.TryParse(model.StartDate, out StartDate))
                        {
                            maintenanceOrder.StartDate = StartDate;
                        }
                        DateTime EndDate;
                        if (DateTime.TryParse(model.StartDate, out EndDate))
                        {
                            maintenanceOrder.EndDate = EndDate;
                        }


                        maintenanceOrder.NumberOfVisits = model.NumberOfVisits;
                        maintenanceOrder.Active = true;
                        maintenanceOrder.CreatedBy = validation.userID;
                        maintenanceOrder.CreationDate = DateTime.Now;
                        maintenanceOrder.ModifiedBy = validation.userID;
                        maintenanceOrder.ModificationDate = DateTime.Now;
                        maintenanceOrder.ProjectId = model.ProjectID;

                        await _Context.ManagementOfMaintenanceOrders.AddAsync(maintenanceOrder);
                        _Context.SaveChanges();
                        //-------------------converting list of strings to list of datatime--------------
                        List<DateTime> PlannedDateList = new List<DateTime>();
                        for (int i = 0; i < model.NumberOfVisits; i++)
                        {
                            DateTime PlannedDate;
                            DateTime.TryParse(model.PlannedDate[i], out PlannedDate);
                            PlannedDateList.Add(PlannedDate);
                        }
                        //-------------------------------------------------------------------------------
                        var ManagementOfMaintenanceOrder = _Context.ManagementOfMaintenanceOrders.Where(a => a.ProjectId == model.ProjectID).FirstOrDefault();
                        List<VisitsScheduleOfMaintenance> VisitList = new List<VisitsScheduleOfMaintenance>();
                        for (var i = 0; i < model.NumberOfVisits; i++)
                        {
                            VisitsScheduleOfMaintenance visitsOfMaintenance = new VisitsScheduleOfMaintenance();
                            visitsOfMaintenance.ManagementOfMaintenanceOrderId = maintenanceOrder.Id;
                            visitsOfMaintenance.Serial = (i + 1).ToString();

                            if (PlannedDateList[i].Year != 1)
                            {
                                visitsOfMaintenance.PlannedDate = PlannedDateList[i];
                            }
                            //visitsOfMaintenance.VisitDate = DateTime.Now;
                            visitsOfMaintenance.Status = false;
                            visitsOfMaintenance.Active = true;
                            visitsOfMaintenance.CreatedBy = validation.userID;
                            visitsOfMaintenance.CreationDate = DateTime.Now;
                            visitsOfMaintenance.ModifiedBy = validation.userID;
                            visitsOfMaintenance.ModificationDate = DateTime.Now;
                            visitsOfMaintenance.ManagementOfMaintenanceOrderId = ManagementOfMaintenanceOrder.Id;

                            VisitList.Add(visitsOfMaintenance);
                        }
                        await _Context.VisitsScheduleOfMaintenances.AddRangeAsync(VisitList);
                        //-----------------------------add Notification---------------------------------------
                        DateTime reminderDate;
                        if (DateTime.TryParse(model.StartDate, out reminderDate))
                        {
                            reminderDate = reminderDate.AddDays(-14);
                        }
                        string projectName = Common.GetProjectName(model.ProjectID, _Context);

                        Notification notification = new Notification();
                        notification.Title = "Reminder to Renew the Maintenance Contract for (" + projectName + ")";
                        notification.Description = projectName + " is Reminder to Renew the Maintenance Contract.";
                        notification.Date = reminderDate;
                        notification.New = true;
                        notification.Url = "~/project/details/" + model.ProjectID;
                        notification.UserId = validation.userID;

                        _Context.SaveChanges();

                        //response.ID = ;
                    }
                    else
                    {
                        int serial = 0;

                        var visit = await _Context.VisitsScheduleOfMaintenances.Where(a => a.ManagementOfMaintenanceOrderId == model.ManagementOfMaintenanceOrderID).ToListAsync();

                        if (visit != null)
                        {
                            serial = visit.Count();
                        }

                        VisitsScheduleOfMaintenance visitsOfMaintenance = new VisitsScheduleOfMaintenance();
                        visitsOfMaintenance.ManagementOfMaintenanceOrderId = model.ManagementOfMaintenanceOrderID;
                        visitsOfMaintenance.Serial = (serial + 1).ToString();

                        DateTime PlannedDate;
                        if (DateTime.TryParse(model.ExtraPlannedDate, out PlannedDate))
                        {
                            if (PlannedDate.Year != 1)
                            {
                                visitsOfMaintenance.PlannedDate = PlannedDate;
                            }
                        }

                        visitsOfMaintenance.Status = false;
                        visitsOfMaintenance.Active = true;
                        visitsOfMaintenance.CreatedBy = validation.userID;
                        visitsOfMaintenance.CreationDate = DateTime.Now;
                        visitsOfMaintenance.ModifiedBy = validation.userID;
                        visitsOfMaintenance.ModificationDate = DateTime.Now;

                        _Context.SaveChanges();


                    }
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddEditManagementOfRentOffer")]
        public async Task<BaseResponseWithID> AddEditManagementOfRentOffer([FromForm] ManagementOfRentOfferVM model)
        {
            BaseResponseWithID response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region Casting Dates
            DateTime ReleaseDate;
            DateTime.TryParse(model.ReleaseDate, out ReleaseDate);

            DateTime PlannedReceivingDate;
            DateTime.TryParse(model.PlannedReceivingDate, out PlannedReceivingDate);

            DateTime ActualReceivingDate;
            DateTime.TryParse(model.ActualReceivingDate, out ActualReceivingDate);

            DateTime FinFeedBackReplyDate;
            DateTime.TryParse(model.FinFeedBackReplyDate, out FinFeedBackReplyDate);

            #endregion

            try
            {
                if (response.Result)
                {
                    if (!Common.CheckUserRole(validation.userID, 76, _Context))
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Your are not authorized to edit Rents of this project ";
                        response.Errors.Add(error);

                        return response;
                    }

                    var costOfExtraDays = Math.Abs(model.CostOfExtraDays);
                    var delay = ActualReceivingDate - PlannedReceivingDate;
                    var period = PlannedReceivingDate - ReleaseDate;

                    var totalCostOfExtraDays = costOfExtraDays * (int)delay.TotalDays;

                    var extraRequired = Math.Abs(model.ExtraRequired);
                    var damageOrPenaltiesCost = Math.Abs(model.DamageOrPenaltiesCost);
                    var discount = Math.Abs(model.Discount);

                    var totalRequiredExtraCost = totalCostOfExtraDays + extraRequired + damageOrPenaltiesCost - discount;

                    if (model.ManagementOfRentOrderID == 0)
                    {
                        ManagementOfRentOrder rentOrder = new ManagementOfRentOrder();
                        rentOrder.RentOfferId = model.OfferID;
                        rentOrder.ProjectId = model.ProjectID;
                        rentOrder.ReleaseDate = ReleaseDate;
                        rentOrder.PlannedReceivingDate = PlannedReceivingDate;
                        rentOrder.ActualReceivingDate = ActualReceivingDate;
                        rentOrder.Period = (int)period.TotalDays;
                        rentOrder.Delay = (int)delay.TotalDays;
                        rentOrder.CostOfExtraDays = model.CostOfExtraDays;
                        rentOrder.TotalCostOfExtraDays = totalCostOfExtraDays;
                        rentOrder.ExtraRequired = model.ExtraRequired;
                        rentOrder.Discount = model.Discount;
                        rentOrder.DamageOrPenaltiesStatus = model.DamageOrPenaltiesStatus;
                        rentOrder.DamageOrPenaltiesDesc = model.DamageOrPenaltiesDesc != null ? model.DamageOrPenaltiesDesc : "";
                        rentOrder.DamageOrPenaltiesCost = model.DamageOrPenaltiesCost;
                        rentOrder.TotalRequiredExtraCost = totalRequiredExtraCost;
                        rentOrder.FinFeedBackConfirmed = false;
                        rentOrder.Active = true;
                        rentOrder.CreatedBy = validation.userID;
                        rentOrder.CreationDate = DateTime.Now;
                        rentOrder.ModifiedBy = validation.userID;
                        rentOrder.ModificationDate = DateTime.Now;


                        var project = _Context.Projects.Where(a => a.Id == model.ProjectID).FirstOrDefault();
                        if (project != null)
                        {
                            project.ExtraCost = project.ExtraCost + totalRequiredExtraCost;
                            //project.Save();
                        }
                        _Context.ManagementOfRentOrders.Add(rentOrder);
                        _Context.SaveChanges();
                        #region Save Attachments to DB


                        List<ManagementOfRentOrderAttachment> ManagementOfRentOrderAttachmentList = new List<ManagementOfRentOrderAttachment>();
                        string fullFilePath = "";
                        if (model.RentAttachment != null)
                        {
                            foreach (var file in model.RentAttachment)
                            {
                                if (file.Length > 0)
                                {
                                    var fileExtension = file.FileName.Split('.').Last();

                                    //var fullFilePath = $"C:\\Docs\\{projectId}\\{file.FileName}";

                                    var virtualPath = $"Attachments\\{validation.CompanyName}\\Rent\\ManagementRentOrder_{rentOrder.Id}\\";
                                    var FileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);

                                    //var clientFolder = Server.MapPath(virtualPath);

                                    ManagementOfRentOrderAttachment managementOfRentOrderAttachment = new ManagementOfRentOrderAttachment();
                                    managementOfRentOrderAttachment.ManagementOfRentOrderId = rentOrder.Id;
                                    managementOfRentOrderAttachment.AttachmentPath = Common.SaveFileIFF(virtualPath, file, FileName, fileExtension, _host);
                                    managementOfRentOrderAttachment.FileName = System.IO.Path.GetFileName(managementOfRentOrderAttachment.AttachmentPath);
                                    managementOfRentOrderAttachment.FileExtenssion = !string.IsNullOrEmpty(fileExtension) ? fileExtension : "";
                                    managementOfRentOrderAttachment.Category = model.AttachmentCategory;
                                    managementOfRentOrderAttachment.Description = model.AttachmentDescription;
                                    managementOfRentOrderAttachment.Active = true;
                                    managementOfRentOrderAttachment.CreationDate = DateTime.Now;
                                    managementOfRentOrderAttachment.CreatedBy = validation.userID;
                                    managementOfRentOrderAttachment.Modified = DateTime.Now;
                                    managementOfRentOrderAttachment.ModifiedBy = validation.userID;

                                    ManagementOfRentOrderAttachmentList.Add(managementOfRentOrderAttachment);
                                }
                            }
                        }

                        _Context.ManagementOfRentOrderAttachments.AddRange(ManagementOfRentOrderAttachmentList);
                        _Context.SaveChanges();
                        #endregion
                        response.ID = rentOrder.Id;

                        #region Create Tasks & Notifications for the Financial
                        //if (totalRequiredExtraCost != 0)
                        //{
                        //    string projectName = CommonClass.GetProjectName(model.ProjectID);

                        //    string userName = CommonClass.GetUserName(UserID);

                        //    string offerSerial = CommonClass.GetOfferSerial(model.OfferID);

                        //    long salesPersonID = CommonClass.GetOfferSalesPerson(model.OfferID);
                        //    long branchID = CommonClass.GetUserBranchID(salesPersonID);

                        //    TaskType taskType = new TaskType();
                        //    taskType.Where.Name.Value = "Rent_Financial";
                        //    if (taskType.Query.Load())
                        //    {
                        //        TaskHandling.CreateTaskForGroup(
                        //        "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //        "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "FinanialMembers",
                        //                DateTime.Now.AddDays(30), taskType.ID, rentOrder.ID.ToString(), "RentFinancial",
                        //                "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                null,
                        //                "~/Project/OpenRentFinancialTask?taskTypeIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(((long)RentTaskTypes.Rent_Financial).ToString(), key)) + "&refrenceIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(rentOrder.ID.ToString(), key))
                        //                , UserID);

                        //        CommonClass.sendGroupNotifications("FinanialMembers",
                        //            "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "~/Project/OpenRentFinancialTask?taskTypeIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(((long)RentTaskTypes.Rent_Financial).ToString(), key)) + "&refrenceIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(rentOrder.ID.ToString(), key))
                        //            );
                        //    }

                        //    CommonClass.sendGroupNotifications("CRMUsers",
                        //                "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "~/Project/Details?id=" + model.ProjectID
                        //                );

                        //    CommonClass.sendGroupNotifications("FinanialMembers",
                        //                "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "~/Project/Details?id=" + model.ProjectID
                        //                );

                        //    CommonClass.sendGroupBranchNotifications(branchID, "Secretary",
                        //                "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //                "~/Project/Details?id=" + model.ProjectID
                        //                );
                        //}
                        #endregion

                    }


                    else
                    {
                        decimal oldTotalRequiredExtraCost = 0;

                        var rentOrder = _Context.ManagementOfRentOrders.Find(model.ManagementOfRentOrderID);

                        if (rentOrder != null)
                        {

                            oldTotalRequiredExtraCost = rentOrder.TotalRequiredExtraCost;
                            rentOrder.ReleaseDate = ReleaseDate;
                            rentOrder.PlannedReceivingDate = PlannedReceivingDate;
                            rentOrder.ActualReceivingDate = ActualReceivingDate;
                            rentOrder.Period = (int)period.TotalDays;
                            rentOrder.Delay = (int)delay.TotalDays;
                            rentOrder.CostOfExtraDays = model.CostOfExtraDays;
                            rentOrder.TotalCostOfExtraDays = totalCostOfExtraDays;
                            rentOrder.ExtraRequired = model.ExtraRequired;
                            rentOrder.Discount = model.Discount;
                            rentOrder.DamageOrPenaltiesStatus = model.DamageOrPenaltiesStatus;
                            rentOrder.DamageOrPenaltiesDesc = model.DamageOrPenaltiesDesc != null ? model.DamageOrPenaltiesDesc : "";
                            rentOrder.DamageOrPenaltiesCost = model.DamageOrPenaltiesCost;
                            rentOrder.TotalRequiredExtraCost = totalRequiredExtraCost;
                            rentOrder.FinFeedBackConfirmed = false;
                            rentOrder.Active = true;
                            rentOrder.CreatedBy = validation.userID;
                            rentOrder.CreationDate = DateTime.Now;
                            rentOrder.ModifiedBy = validation.userID;
                            rentOrder.ModificationDate = DateTime.Now;

                            var project = _Context.Projects.Find(rentOrder.ProjectId);

                            if (project != null)
                            {
                                project.ExtraCost = project.ExtraCost - oldTotalRequiredExtraCost + totalRequiredExtraCost;
                            }


                        }

                        #region Save Attachments to DB
                        List<ManagementOfRentOrderAttachment> ManagementOfRentOrderAttachmentList = new List<ManagementOfRentOrderAttachment>();
                        string fullFilePath = "";
                        if (model.RentAttachment.Count > 0)
                        {

                            var managementOfRentOrderAttachmentOld = _Context.ManagementOfRentOrderAttachments.Where(a => a.ManagementOfRentOrderId == rentOrder.Id).FirstOrDefault();

                            if (managementOfRentOrderAttachmentOld != null)
                            {
                                System.IO.File.Delete(managementOfRentOrderAttachmentOld.AttachmentPath);              //delete the old folder from Attachments folder
                                _Context.ManagementOfRentOrderAttachments.Remove(managementOfRentOrderAttachmentOld);
                                //var DeletePath = $"Attachments\\{validation.CompanyName}\\Rent\\ManagementRentOrder_{rentOrder.Id}\\";
                            }

                            foreach (var file in model.RentAttachment)
                            {
                                if (file.Length > 0)
                                {
                                    var fileExtension = file.FileName.Split('.').Last();

                                    //var fullFilePath = $"C:\\Docs\\{projectId}\\{file.FileName}";

                                    var virtualPath = $"Attachments\\{validation.CompanyName}\\Rent\\ManagementRentOrder_{rentOrder.Id}\\";
                                    var FileName = System.IO.Path.GetFileNameWithoutExtension(file.FileName);

                                    //var clientFolder = Server.MapPath(virtualPath);

                                    ManagementOfRentOrderAttachment managementOfRentOrderAttachment = new ManagementOfRentOrderAttachment();
                                    managementOfRentOrderAttachment.ManagementOfRentOrderId = rentOrder.Id;
                                    managementOfRentOrderAttachment.AttachmentPath = Common.SaveFileIFF(virtualPath, file, FileName, fileExtension, _host);
                                    managementOfRentOrderAttachment.FileName = System.IO.Path.GetFileName(managementOfRentOrderAttachment.AttachmentPath);
                                    managementOfRentOrderAttachment.FileExtenssion = !string.IsNullOrEmpty(fileExtension) ? fileExtension : "";
                                    managementOfRentOrderAttachment.Category = model.AttachmentCategory;
                                    managementOfRentOrderAttachment.Description = model.AttachmentDescription;
                                    managementOfRentOrderAttachment.Active = true;
                                    managementOfRentOrderAttachment.CreationDate = DateTime.Now;
                                    managementOfRentOrderAttachment.CreatedBy = validation.userID;
                                    managementOfRentOrderAttachment.Modified = DateTime.Now;
                                    managementOfRentOrderAttachment.ModifiedBy = validation.userID;

                                    ManagementOfRentOrderAttachmentList.Add(managementOfRentOrderAttachment);
                                }
                            }
                        }
                        #endregion
                        _Context.ManagementOfRentOrderAttachments.AddRange(ManagementOfRentOrderAttachmentList);
                        _Context.SaveChanges();

                        response.ID = rentOrder.Id;


                        //string projectName = Common.GetProjectName(model.ProjectID, _Context);
                        #region Create Tasks & Notifications for the Clarification

                        //string userName = CommonClass.GetUserName(UserID);

                        //string offerSerial = CommonClass.GetOfferSerial(model.OfferID);

                        //long salesPersonID = CommonClass.GetOfferSalesPerson(model.OfferID);
                        //long branchID = CommonClass.GetUserBranchID(salesPersonID);

                        //TaskType taskType = new TaskType();
                        //taskType.Where.Name.Value = "Rent_Financial";
                        //if (taskType.Query.Load())
                        //{
                        //    TaskHandling.CreateTaskForGroup(
                        //    "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //    "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "FinanialMembers",
                        //            DateTime.Now.AddDays(30), taskType.ID, rentOrder.ID.ToString(), "RentFinancial",
                        //            "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            null,
                        //            "~/Project/OpenRentFinancialTask?taskTypeIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(((long)RentTaskTypes.Rent_Financial).ToString(), key)) + "&refrenceIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(rentOrder.ID.ToString(), key))
                        //            , UserID);

                        //    CommonClass.sendGroupNotifications("FinanialMembers",
                        //        "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //        "Financial Feedback Request for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //        "~/Project/OpenRentFinancialTask?taskTypeIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(((long)RentTaskTypes.Rent_Financial).ToString(), key)) + "&refrenceIDStr=" + Server.UrlEncode(GarasERP.Encrypt_Decrypt.Encrypt(rentOrder.ID.ToString(), key))
                        //        );
                        //}

                        //CommonClass.sendGroupNotifications("CRMUsers",
                        //            "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "~/Project/Details?id=" + model.ProjectID
                        //            );

                        //CommonClass.sendGroupNotifications("FinanialMembers",
                        //            "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "~/Project/Details?id=" + model.ProjectID
                        //            );

                        //CommonClass.sendGroupBranchNotifications(branchID, "Secretary",
                        //            "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "New General Information for Rent Offer #" + offerSerial + " / " + projectName + " Project " + " - Received From " + userName + DateTime.Now.ToString("dd/MM/yyyy"),
                        //            "~/Project/Details?id=" + model.ProjectID
                        //            );
                        #endregion

                    }
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpPost("AddEditVisitsScheduleOfMaintenanceAttachment")]
        public async Task<BaseResponseWithID> AddEditVisitsScheduleOfMaintenanceAttachment([FromForm] VisitsScheduleOfMaintenanceAttachmentModel model)
        {
            BaseResponseWithID response = new BaseResponseWithID()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    if (model.AttachmentId == 0)
                    {
                        //var virtualPath = $"Attachments\\{validation.CompanyName}\\Maintenance\\ManagementMaintenanceOrder_{model.ManagementOfMaintenanceOrderId}\\VisitsScheduleOfMaintenance_{model.VisitsScheduleOfMaintenanceId}";
                        var virtualPath = "Attachments\\" + validation.CompanyName + "\\Maintenance\\" + "ManagementMaintenanceOrder_" + model.ManagementOfMaintenanceOrderId + "\\VisitsScheduleOfMaintenance_" + model.VisitsScheduleOfMaintenanceId + "\\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(model.VisitsScheduleAttachment.FileName);
                        var fileExtension = model.VisitsScheduleAttachment.FileName.Split('.').Last();
                        //Common.SaveFileIFF(virtualPath, model.VisitsScheduleAttachment, FileName, fileExtension, _host);


                        VisitsScheduleOfMaintenanceAttachment visitsScheduleOfMaintenanceAttachment = new VisitsScheduleOfMaintenanceAttachment();
                        visitsScheduleOfMaintenanceAttachment.VisitsScheduleOfMaintenanceId = model.VisitsScheduleOfMaintenanceId;
                        visitsScheduleOfMaintenanceAttachment.CreatedBy = validation.userID;
                        visitsScheduleOfMaintenanceAttachment.CreationDate = DateTime.Now;
                        visitsScheduleOfMaintenanceAttachment.ModifiedBy = validation.userID;
                        visitsScheduleOfMaintenanceAttachment.Modified = DateTime.Now;                                  //should named ModifiedDate 
                        visitsScheduleOfMaintenanceAttachment.Active = model.Active;
                        visitsScheduleOfMaintenanceAttachment.Category = null;
                        visitsScheduleOfMaintenanceAttachment.AttachmentPath = Common.SaveFileIFF(virtualPath, model.VisitsScheduleAttachment, FileName, fileExtension, _host);
                        visitsScheduleOfMaintenanceAttachment.FileExtenssion = fileExtension;
                        visitsScheduleOfMaintenanceAttachment.FileName = System.IO.Path.GetFileName(visitsScheduleOfMaintenanceAttachment.AttachmentPath);

                        await _Context.VisitsScheduleOfMaintenanceAttachments.AddAsync(visitsScheduleOfMaintenanceAttachment);
                        await _Context.SaveChangesAsync();

                        response.ID = visitsScheduleOfMaintenanceAttachment.Id;


                    }
                    else
                    {
                        var Attachment = await _Context.VisitsScheduleOfMaintenanceAttachments.Where(a => a.Id == model.AttachmentId).FirstOrDefaultAsync();

                        if (Attachment != null)
                        {
                            if (model.VisitsScheduleAttachment != null)
                            {

                                var pathToDelete = Attachment.AttachmentPath;
                                System.IO.File.Delete(Attachment.AttachmentPath);              //delete the old folder from Attachments folder


                                var path = "Attachments\\" + validation.CompanyName + "\\Maintenance\\" + "ManagementMaintenanceOrder_" + model.ManagementOfMaintenanceOrderId + "\\VisitsScheduleOfMaintenance_" + model.VisitsScheduleOfMaintenanceId + "\\";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(model.VisitsScheduleAttachment.FileName);
                                var fileExtension = model.VisitsScheduleAttachment.FileName.Split('.').Last();


                                Attachment.AttachmentPath = Common.SaveFileIFF(path, model.VisitsScheduleAttachment, FileName, fileExtension, _host);
                                Attachment.FileExtenssion = fileExtension;
                                Attachment.FileName = System.IO.Path.GetFileName(Attachment.AttachmentPath);
                            }
                            Attachment.Modified = DateTime.Now;
                            Attachment.ModifiedBy = validation.userID;
                            Attachment.Active = model.Active;


                            await _Context.SaveChangesAsync();

                            response.ID = Attachment.Id;
                        }
                    }
                }
                return response;
            }
            catch (Exception e)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = e.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }
    }
}
