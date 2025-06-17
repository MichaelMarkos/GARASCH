using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.Models.Maintenance;
using NewGarasAPI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using System.Web;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using Microsoft.Data.SqlClient;
using NewGarasAPI.Models.Account;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Data;
using System.Drawing;
using System.IO.Packaging;
using NewGarasAPI.Models.User;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Maintenance.UsedInResponse;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using DocumentFormat.OpenXml.Spreadsheet;
using NewGaras.Infrastructure.Models.SalesOffer;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http.HttpResults;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Bcpg;

namespace NewGaras.Domain.Services
{
    public class MaintenanceAndService : IMaintenanceAndService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly ISalesOfferService _offerService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        static readonly string key = "SalesGarasPass";
        private HearderVaidatorOutput validation;

        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public MaintenanceAndService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork, ISalesOfferService offerService)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            _offerService = offerService;
        }
        public async Task<VisitsScheduleMaintenanceByDayResponse> GetMaintenanceByDay(GetMaintenanceByDayFilters filters, string companyname)
        {
            VisitsScheduleMaintenanceByDayResponse Response = new VisitsScheduleMaintenanceByDayResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                /*if (!string.IsNullOrWhiteSpace(headers["IsOpen"]) && bool.TryParse(headers["IsOpen"], out IsOpen))
                {
                    IsOpen = bool.TryParse(headers["IsOpen"], out IsOpen);
                    FilterWithDate = false;
                }
                if (!string.IsNullOrWhiteSpace(headers["IsDelayed"]) && bool.TryParse(headers["IsDelayed"], out IsDelayed))
                {
                    IsDelayed = bool.TryParse(headers["IsDelayed"], out IsDelayed);
                    FilterWithDate = false;
                }*/

                if (filters.Year == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Year Is Mandatory";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }

                if (filters.Month == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Month Is Mandatory";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }



                /*if (!string.IsNullOrWhiteSpace(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                {
                    int.TryParse(headers["Year"], out Year);
                }
                if (!string.IsNullOrWhiteSpace(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                {
                    int.TryParse(headers["Month"], out Month);
                }
                int DayFilter = 0;
                if (!string.IsNullOrWhiteSpace(headers["Day"]) && int.TryParse(headers["Day"], out DayFilter))
                {
                    int.TryParse(headers["Day"], out DayFilter);
                }

                long AssignToID = 0;
                if (!string.IsNullOrWhiteSpace(headers["AssignToID"]) && long.TryParse(headers["AssignToID"], out AssignToID))
                {
                    long.TryParse(headers["AssignToID"], out AssignToID);
                }*/


                if (Response.Result)
                {
                    var MaintenanceDayList = new List<MainDayVM>();

                    var VisitMaintenanceDB = await _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.Active && a.MaintenanceFor.Client.NeedApproval != 2 && a.ManagementOfMaintenanceOrder.ContractStatus != "Closed", includes: new[] { "AssignedTo", "MaintenanceFor.Client", "MaintenanceFor.Client.ClientContactPeople", "ManagementOfMaintenanceOrder", "MaintenanceFor.SalesOffer.SalesOfferLocations", "VisitsScheduleOfMaintenanceAttachments", "MaintenanceFor", "MaintenanceFor.InventoryItem", "MaintenanceReports" });
                    if (filters.IsOpen)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.VisitDate == null).ToList();
                    }
                    if (filters.IsDelayed)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.VisitDate == null && x.PlannedDate > DateTime.Now).ToList();
                    }
                    if (filters.Year != 0)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.PlannedDate?.Year == filters.Year).ToList();
                    }
                    if (filters.Month != 0)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.PlannedDate?.Month == filters.Month).ToList();
                    }
                    if (filters.Day != 0)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.PlannedDate?.Day == filters.Day).ToList();
                    }
                    if (filters.AssignToID != 0)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.AssignedToId == filters.AssignToID || x.AssignedToId == null).OrderByDescending(x => x.AssignedToId.HasValue) // Prioritize non-null
                                                                                                                                            .ThenBy(x => x.AssignedToId) // Sort numbers in ascending order
                                                                                                                                            .ToList();
                    }

                    if (filters.SearchKey != null)
                    {
                        var key = HttpUtility.UrlDecode(filters.SearchKey);
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.MaintenanceFor.Client.ClientContactPeople.Any(a => a.Mobile.Contains(key)) || x.MaintenanceFor.Client.Name.Contains(key)
                        || (x.MaintenanceFor.ProductSerial != null && x.MaintenanceFor.ProductSerial.Contains(key)) || (x.MaintenanceFor.ProductName != null && x.MaintenanceFor.ProductName.Contains(key))).ToList();
                    }

                    var VisitMaintenanceGroupingByDB = VisitMaintenanceDB.GroupBy(x => x.PlannedDate?.Day ?? 0).ToList();

                    int FinsishedVisits = 0;
                    int notFinsishedVisits = 0;
                    if (VisitMaintenanceGroupingByDB.Count > 0)
                    {
                        foreach (var VisitsDetailsByDayObj in VisitMaintenanceGroupingByDB)
                        {

                            var MaintenanceDay = new MainDayVM();

                            int Day = VisitsDetailsByDayObj.Key;

                            string DayName = new DateTime(filters.Year ?? 0, filters.Month ?? 0, Day).ToString("dddd") ?? "";

                            MaintenanceDay.Day = DayName + "---2";
                            MaintenanceDay.Date = Day + "-" + filters.Month + "-" + filters.Year;



                            var MaintenancebyDayDetailsList = new List<MaintenanceValuesByDay>();
                            foreach (var item in VisitsDetailsByDayObj)
                            {
                                if (item.Status == true) FinsishedVisits += 1;
                                else { notFinsishedVisits += 1; }

                                MaintenanceValuesByDay VisitMaintenanceByDayDBVM = new MaintenanceValuesByDay();
                                VisitMaintenanceByDayDBVM.WithContract = item.ManagementOfMaintenanceOrderId != null ? true : false;
                                VisitMaintenanceByDayDBVM.ManagementMaintenanctOrderID = item.ManagementOfMaintenanceOrderId;
                                VisitMaintenanceByDayDBVM.ClientID = item.MaintenanceFor?.ClientId ?? 0;
                                VisitMaintenanceByDayDBVM.ClientName = item.MaintenanceFor?.Client?.Name;
                                if (item.MaintenanceFor?.Client?.HasLogo == true && item.MaintenanceFor?.Client?.Logo != null)
                                {
                                    VisitMaintenanceByDayDBVM.ClientLogo = Globals.baseURL + "/ShowImage.ashx?ImageID=" + System.Web.HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(VisitMaintenanceByDayDBVM.ClientID.ToString(), key)) + "&type=photo&CompName=" + companyname.ToString().ToLower();
                                }
                                //VisitMaintenanceByDayDBVM.ProductSerial = "Product#" + item.MaintenanceFor?.ProductSerial;
                                VisitMaintenanceByDayDBVM.ProductSerial = item.MaintenanceFor?.ProductSerial;
                                VisitMaintenanceByDayDBVM.ProductBrand = item.MaintenanceFor?.ProductBrand;
                                VisitMaintenanceByDayDBVM.ProductFabricator = item.MaintenanceFor?.ProductFabricator;
                                VisitMaintenanceByDayDBVM.ProductType = item.MaintenanceFor?.ProductType;
                                VisitMaintenanceByDayDBVM.AssignedToID = item.AssignedToId;
                                //VisitMaintenanceByDayDBVM.VisitMaintenanceID = item.ManagementOfMaintenanceOrder?.VisitsScheduleOfMaintenances?.FirstOrDefault()?.ID;
                                VisitMaintenanceByDayDBVM.VisitMaintenanceID = item.Id;
                                VisitMaintenanceByDayDBVM.AssignToUserName = item.AssignedTo?.FirstName + " " + item.AssignedTo?.FirstName; // Common.GetUserName(item.AssignedToID);
                                VisitMaintenanceByDayDBVM.Status = item.Status;


                                VisitMaintenanceByDayDBVM.ClientSatisfactionRate = item.MaintenanceReports?.Average(x => x.ClientSatisfactionRate);
                                VisitMaintenanceByDayDBVM.ClientAddress = item.MaintenanceFor?.Client?.ClientAddresses?.FirstOrDefault()?.Address;

                                if (item.MaintenanceFor?.Client?.NeedApproval == 2)
                                {
                                    VisitMaintenanceByDayDBVM.ClientIsBlocked = true;
                                }


                                VisitMaintenanceByDayDBVM.VisitDate = item.VisitDate?.ToString();
                                VisitMaintenanceByDayDBVM.WorkerUserName = item.MaintenanceReports.FirstOrDefault()?.ByUser;
                                //if (ClientContactPersonDb != null)
                                //{
                                //}
                                var ClientContactPersonDB = item.MaintenanceFor?.Client?.ClientContactPeople?.FirstOrDefault();
                                if (ClientContactPersonDB != null)
                                {
                                    VisitMaintenanceByDayDBVM.ClientMobile = ClientContactPersonDB.Mobile;
                                    VisitMaintenanceByDayDBVM.ClientLocation = ClientContactPersonDB.Location;
                                    VisitMaintenanceByDayDBVM.ContactPersonName = ClientContactPersonDB.Name != null ? ClientContactPersonDB.Name : "";
                                    VisitMaintenanceByDayDBVM.ContactPersonMobile = ClientContactPersonDB.Mobile != null ? ClientContactPersonDB.Mobile : "";
                                }

                                //if (ClientMaintenanceForDB !=null)
                                //{
                                //}
                                var SalesOfferLocations = item.MaintenanceFor?.SalesOffer?.SalesOfferLocations?.FirstOrDefault();
                                if (SalesOfferLocations != null)
                                {
                                    VisitMaintenanceByDayDBVM.Latitude = SalesOfferLocations.LocationX;
                                    VisitMaintenanceByDayDBVM.Longitude = SalesOfferLocations.LocationY;
                                    VisitMaintenanceByDayDBVM.Location = SalesOfferLocations.Description;
                                }

                                VisitMaintenanceByDayDBVM.ProductName = item.MaintenanceFor?.ProductName != null ? item.MaintenanceFor?.ProductName : "";
                                VisitMaintenanceByDayDBVM.CollectedAmount = item.ManagementOfMaintenanceOrder?.ContractPrice;
                                VisitMaintenanceByDayDBVM.ContractPrice = item.ManagementOfMaintenanceOrder?.ContractPrice;
                                VisitMaintenanceByDayDBVM.MaintenanceForID = item.MaintenanceForId ?? 0;



                                // Extra Data 
                                VisitMaintenanceByDayDBVM.lastVisitDate = VisitMaintenanceDB.Where(x => x.MaintenanceForId == item.MaintenanceForId && x.VisitDate != null).OrderByDescending(x => x.VisitDate).Select(x => x.VisitDate).FirstOrDefault()?.ToShortDateString();
                                var TotalContractVisitNo = item.ManagementOfMaintenanceOrder?.NumberOfVisits ?? 0;
                                var TotalVisitsNo = VisitMaintenanceDB.Where(x => x.MaintenanceForId == item.MaintenanceForId && x.ManagementOfMaintenanceOrderId == item.ManagementOfMaintenanceOrderId &&
                                x.VisitDate != null && x.Status == true).Count();
                                VisitMaintenanceByDayDBVM.remainVisitsNo = TotalContractVisitNo > TotalVisitsNo ? TotalContractVisitNo - TotalVisitsNo : 0;
                                VisitMaintenanceByDayDBVM.ProjectName = item.MaintenanceFor?.SalesOffer?.ProjectName;
                                VisitMaintenanceByDayDBVM.CurrentMileageCounter = item.MileageCounter;
                                VisitMaintenanceByDayDBVM.LastMileageCounter = VisitMaintenanceDB.Where(x => x.MaintenanceForId == item.MaintenanceForId && x.VisitDate != null).OrderByDescending(x => x.VisitDate).Select(x => x.MileageCounter).FirstOrDefault();
                                VisitMaintenanceByDayDBVM.PlannedDate = item.PlannedDate.ToString();

                                if (item.VisitsScheduleOfMaintenanceAttachments != null)
                                {
                                    VisitMaintenanceByDayDBVM.MaintenanceProblemAttachments = item.VisitsScheduleOfMaintenanceAttachments != null && item.VisitsScheduleOfMaintenanceAttachments.Count > 0 ? item.VisitsScheduleOfMaintenanceAttachments.Select(attach => new Attachment
                                    {
                                        Id = attach.Id,
                                        FileExtension = attach.FileExtenssion,
                                        FileName = attach.FileName,
                                        FilePath = Globals.baseURL + attach.AttachmentPath.TrimStart('~')
                                    }).ToList() : null;
                                }
                                VisitMaintenanceByDayDBVM.InventoryItemName = item.MaintenanceFor?.InventoryItem?.Name;
                                MaintenancebyDayDetailsList.Add(VisitMaintenanceByDayDBVM);

                            }
                            MaintenanceDay.MaintenanceValuesByDayList = MaintenancebyDayDetailsList;
                            MaintenanceDayList.Add(MaintenanceDay);

                        }
                        Response.MainDayList = MaintenanceDayList;
                        Response.NUmOfVisits = FinsishedVisits + notFinsishedVisits;
                        Response.NumOFAssignedToVisits = MaintenanceDayList.SelectMany(a => a.MaintenanceValuesByDayList.Where(b => b.AssignedToID != null)).Count();
                        Response.NumOFNotAssignedToVisits = MaintenanceDayList.SelectMany(a => a.MaintenanceValuesByDayList.Where(b => b.AssignedToID == null)).Count();
                        Response.NumOfFinishedVisits = FinsishedVisits;
                        Response.NumOfNotFinishedVisits = notFinsishedVisits;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<VisitsScheduleMaintenanceByAreaResponse> GetMaintenanceByArea(GetMaintenanceByAreaFilters filters)
        {
            VisitsScheduleMaintenanceByAreaResponse Response = new VisitsScheduleMaintenanceByAreaResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                /*int DayFilter = 0;
                if (!string.IsNullOrWhiteSpace(headers["Day"]) && int.TryParse(headers["Day"], out DayFilter))
                {
                    int.TryParse(headers["Day"], out DayFilter);
                }
                int Year = 0;
                if (!string.IsNullOrWhiteSpace(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                {
                    int.TryParse(headers["Year"], out Year);
                }

                int Month = 0;
                if (!string.IsNullOrWhiteSpace(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                {
                    int.TryParse(headers["Month"], out Month);
                }

                long AssignToID = 0;
                if (!string.IsNullOrWhiteSpace(headers["AssignToID"]) && long.TryParse(headers["AssignToID"], out AssignToID))
                {
                    long.TryParse(headers["AssignToID"], out AssignToID);
                }*/
                if (Response.Result)
                {
                    var MaintenanceDayList = new List<MaintenanceAreaVM>();

                    var VisitMaintenanceDB = await _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.PlannedDate != null);
                    if (filters.AssignToID != 0)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(x => x.AssignedToId == filters.AssignToID || x.AssignedToId == null).ToList();
                    }
                    if (filters.Year != 0 && filters.Month != 0)
                    {
                        VisitMaintenanceDB = VisitMaintenanceDB.Where(a => a.PlannedDate?.Year == filters.Year && a.PlannedDate?.Month == filters.Month).ToList();

                    }
                    //var VisitMaintenanceGroupingByDB = VisitMaintenanceDB.GroupBy(x => ).ToList();
                    var MaintenanceGroupingByArea = _unitOfWork.SalesOfferLocations.FindAllAsync(x => x.SalesOffer.MaintenanceFors.Any(y => y.VisitsScheduleOfMaintenances.Any()), includes: new[] { "SalesOffer.MaintenanceFors", "Area" }).Result.GroupBy(x => x.Area.Name);
                    var SalesOfferIDS = MaintenanceGroupingByArea.SelectMany(x => x.Select(y => y.SalesOfferId)).ToList();
                    var VisitsScheduleOfMaintenances = VisitMaintenanceDB.Where(x => SalesOfferIDS.Contains(x.MaintenanceFor?.SalesOfferId ?? 0)).ToList();
                    if (MaintenanceGroupingByArea.Count() > 0)
                    {
                        foreach (var MaintenanceObj in MaintenanceGroupingByArea)
                        {

                            var MaintenanceDay = new MaintenanceAreaVM();
                            MaintenanceDay.Area = MaintenanceObj.Key;



                            var MaintenancebyDayDetailsList = new List<MaintenanceValuesByDay>();


                            //var ClientData = await _Context.V_VisitsMaintenance_Report_Users.ToListAsync();
                            //var ClientDataByIDFromClient = await _Context.Clients.ToListAsync();
                            //var ClientContactPerson = await _Context.ClientContactPersons.ToListAsync();
                            //var MaintenanceForByClientID = await _Context.MaintenanceFors.ToListAsync();
                            //var MaintenanceOrderOffer = await _Context.ManagementOfMaintenanceOrders.ToListAsync();

                            var SalesOfferIDSList = MaintenanceObj.Select(x => x.SalesOfferId).ToList();
                            if (SalesOfferIDSList.Count() > 0)
                            {
                                var VisitMaintenanceList = VisitsScheduleOfMaintenances.Where(x => SalesOfferIDSList.Contains(x.MaintenanceFor.SalesOfferId)).ToList();
                                foreach (var item in VisitMaintenanceList)
                                {
                                    // var VisitMaintenance = VisitsScheduleOfMaintenances.Where(x => x.MaintenanceFor.SalesOfferID == item.SalesOfferId).FirstOrDefault();
                                    //var ClientDataByID = ClientData.Where(x => x.ClientID == item.ClientID).FirstOrDefault();
                                    //var ClientContactPersonDb = ClientContactPerson.Where(x => x.ClientID == item.ClientID).FirstOrDefault();

                                    //var ClientDetailsDB = ClientDataByIDFromClient.Where(x => x.ID == item.ClientID).FirstOrDefault();

                                    //var ClientMaintenanceForDB = MaintenanceForByClientID.Where(x => x.ClientID == item.ClientID).FirstOrDefault();
                                    //var MaintenanceOrderOfferDB = MaintenanceOrderOffer.Where(x => x.MaintenanceForID == item.ID).FirstOrDefault();



                                    MaintenanceValuesByDay VisitMaintenanceByDayDBVM = new MaintenanceValuesByDay();
                                    if (item != null)
                                    {

                                        VisitMaintenanceByDayDBVM.WithContract = item.ManagementOfMaintenanceOrderId != null ? true : false;
                                        VisitMaintenanceByDayDBVM.ClientID = item.MaintenanceFor?.ClientId ?? 0;
                                        VisitMaintenanceByDayDBVM.ClientName = item.MaintenanceFor?.Client?.Name;
                                        VisitMaintenanceByDayDBVM.ProductSerial = "Product#" + item.MaintenanceFor?.ProductSerial;
                                        VisitMaintenanceByDayDBVM.AssignedToID = item.AssignedToId;
                                        //VisitMaintenanceByDayDBVM.VisitMaintenanceID = item.ManagementOfMaintenanceOrder?.VisitsScheduleOfMaintenances?.FirstOrDefault()?.ID;
                                        VisitMaintenanceByDayDBVM.VisitMaintenanceID = item.Id;
                                        VisitMaintenanceByDayDBVM.AssignToUserName = item.AssignedTo?.FirstName + " " + item.AssignedTo?.FirstName; // Common.GetUserName(item.AssignedToID);
                                        VisitMaintenanceByDayDBVM.Status = item.Status;


                                        VisitMaintenanceByDayDBVM.ClientSatisfactionRate = item.MaintenanceReports?.Average(x => x.ClientSatisfactionRate);
                                        VisitMaintenanceByDayDBVM.ClientAddress = item.MaintenanceFor?.Client?.ClientAddresses?.FirstOrDefault()?.Address;

                                        VisitMaintenanceByDayDBVM.VisitDate = item.VisitDate?.ToString();

                                        //if (ClientContactPersonDb != null)
                                        //{
                                        //}
                                        var ClientContactPersonDB = item.MaintenanceFor?.Client?.ClientContactPeople?.FirstOrDefault();
                                        if (ClientContactPersonDB != null)
                                        {
                                            VisitMaintenanceByDayDBVM.ClientMobile = ClientContactPersonDB.Mobile;
                                            VisitMaintenanceByDayDBVM.ClientLocation = ClientContactPersonDB.Location;
                                            VisitMaintenanceByDayDBVM.ContactPersonName = ClientContactPersonDB.Name != null ? ClientContactPersonDB.Name : "";
                                        }

                                        //if (ClientMaintenanceForDB !=null)
                                        //{
                                        //}
                                        var SalesOfferLocations = item.MaintenanceFor?.SalesOffer?.SalesOfferLocations?.FirstOrDefault();
                                        if (SalesOfferLocations != null)
                                        {
                                            VisitMaintenanceByDayDBVM.Latitude = SalesOfferLocations.LocationX;
                                            VisitMaintenanceByDayDBVM.Longitude = SalesOfferLocations.LocationY;
                                            VisitMaintenanceByDayDBVM.Location = SalesOfferLocations.Description;
                                        }

                                        VisitMaintenanceByDayDBVM.ProductName = item.MaintenanceFor?.ProductName != null ? item.MaintenanceFor?.ProductName : "";
                                        VisitMaintenanceByDayDBVM.CollectedAmount = item.ManagementOfMaintenanceOrder?.ContractPrice;
                                        VisitMaintenanceByDayDBVM.MaintenanceForID = item.MaintenanceForId ?? 0;
                                        VisitMaintenanceByDayDBVM.CurrentMileageCounter = item.MileageCounter;
                                        VisitMaintenanceByDayDBVM.LastMileageCounter = VisitMaintenanceDB.Where(x => x.MaintenanceForId == item.MaintenanceForId && x.VisitDate != null).OrderByDescending(x => x.VisitDate).Select(x => x.MileageCounter).FirstOrDefault();

                                    }

                                    MaintenancebyDayDetailsList.Add(VisitMaintenanceByDayDBVM);

                                }
                            }


                            MaintenanceDay.MaintenanceValuesByAreaList = MaintenancebyDayDetailsList;

                            MaintenanceDayList.Add(MaintenanceDay);

                        }
                        Response.MainAreaList = MaintenanceDayList;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<VisitsScheduleMaintenanceByYearResponse> GetMaintenanceByMonth([FromHeader] string SearchKey, [FromHeader] int Year = 0)
        {
            VisitsScheduleMaintenanceByYearResponse Response = new VisitsScheduleMaintenanceByYearResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {


                if (Year == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Year Is Mandatory";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }

                if (Response.Result)
                {

                    var MaintenanceDBList = new List<MaintenanceValues>();

                    var VisitMaintenance = _unitOfWork.VisitsScheduleOfMaintenances.FindAllQueryable(a => a.ManagementOfMaintenanceOrder.ContractStatus != "Closed" && a.PlannedDate != null && (int)a.PlannedDate.Value.Year == Year, includes: new[] { "MaintenanceFor.Client.ClientContactPeople", "ManagementOfMaintenanceOrder", "MaintenanceReports.MaintenanceReportUsers" });

                    if (SearchKey != null)
                    {
                        var key = HttpUtility.UrlDecode(SearchKey);
                        VisitMaintenance = VisitMaintenance.Where(x => x.MaintenanceFor.Client.ClientContactPeople.Any(a => a.Mobile.Contains(key)) || x.MaintenanceFor.Client.Name.Contains(key)
                        || x.MaintenanceFor.ProductSerial.Contains(key) || (x.MaintenanceFor.ProductName != null && x.MaintenanceFor.ProductName.Contains(key))).AsQueryable();
                    }

                    var VisitMaintenanceDB = await VisitMaintenance.ToListAsync();
                    if (VisitMaintenanceDB.Count() > 0)
                    {
                        string MonthName = null;

                        for (int i = 0; i <= 12; i++)
                        {

                            var VisitMaintenanceByMonth = VisitMaintenanceDB.Where(x => x.PlannedDate.Value.Month == i).ToList();

                            if (VisitMaintenanceByMonth.Count > 0)
                            {


                                var VisitMaintenanceUsers = VisitMaintenanceByMonth.Select(a => a.MaintenanceReports.Select(y => y.MaintenanceReportUsers)).ToList();
                                decimal UsrsWrkHrsSum = VisitMaintenanceUsers.Select(a => a.Select(b => b.Select(x => x.HourNum).Sum()).Sum()).Sum();
                                int UsrsCount = VisitMaintenanceUsers.Select(a => a.Select(b => b.Select(x => x.UserId).Count()).Sum()).Sum();



                                var UsersEvaluations = _unitOfWork.VVisitsMaintenanceReportUsers.FindAllAsync(x => x.VisitDate.Value.Month == i).Result.GroupBy(x => x.UserId).Select(x => x.Average(a => a.Evaluation)).Sum();

                                MonthName = new DateTime(Year, i, 1).ToString("MMM");


                                var WorkingHoursAverage = UsrsCount != 0 ? UsrsWrkHrsSum / UsrsCount : UsrsWrkHrsSum;

                                MaintenanceValues VisitMaintenanceDBVM = new MaintenanceValues();
                                VisitMaintenanceDBVM.PlannedVisitCount = VisitMaintenanceByMonth.Count();
                                VisitMaintenanceDBVM.Open = VisitMaintenanceByMonth.Where(x => x.Status == false && (x.VisitDate != null ? x.VisitDate.Value.Month == i : false)).Count();
                                VisitMaintenanceDBVM.Closed = VisitMaintenanceByMonth.Where(x => x.Status == true && (x.VisitDate != null ? x.VisitDate.Value.Month == i : false)).Count();
                                VisitMaintenanceDBVM.WorkingHoursAverage = WorkingHoursAverage;
                                VisitMaintenanceDBVM.UsersCount = UsrsCount;
                                VisitMaintenanceDBVM.EvaluationAverage = UsersEvaluations;
                                VisitMaintenanceDBVM.MonthName = MonthName;
                                VisitMaintenanceDBVM.MonthID = i;
                                MaintenanceDBList.Add(VisitMaintenanceDBVM);
                            }





                        }



                        Response.MaintenanceValuesList = MaintenanceDBList;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public string GetInventoryItemCategory(int id)
        {
            string name = "";
            var LoadObjDB = _unitOfWork.InventoryItemCategories.GetById(id);
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }

        public async Task<GetMaintenanceForByIDResponse> GetMaintenanceByID(string companyname, int ID = 0)
        {
            GetMaintenanceForByIDResponse Response = new GetMaintenanceForByIDResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //int ID = 0;
                if (ID == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "ID Is Mandatory";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }
                if (Response.Result)
                {
                    var MaintenanceDayObj = new MaintenanceForDataByID();


                    var MaintenanceByIDDB = await _unitOfWork.MaintenanceFors.FindAsync(a => a.Id == ID, new[] { "InventoryItem", "ProjectCheques", "Client", "SalesOffer.SalesOfferLocations", "SalesOffer.SalesOfferLocations.Country", "SalesOffer.SalesOfferLocations.Governorate", "SalesOffer.SalesOfferLocations.Area", "VisitsScheduleOfMaintenances", "VisitsScheduleOfMaintenances.VisitsScheduleOfMaintenanceAttachments" });
                    var ProjectLocationDB = await _unitOfWork.SalesOffers.GetAllAsync();
                    if (MaintenanceByIDDB != null)
                    {
                        var ProjectLocationOfItem = ProjectLocationDB.Where(x => x.Id == MaintenanceByIDDB.SalesOfferId).Select(x => x.ProjectLocation).FirstOrDefault();
                        MaintenanceDayObj.ID = MaintenanceByIDDB.Id;
                        MaintenanceDayObj.CreatedBy = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(MaintenanceByIDDB.CreatedBy?.ToString(), key));
                        MaintenanceDayObj.InventoryItemID = MaintenanceByIDDB.InventoryItemId;
                        MaintenanceDayObj.InventoryItemName = MaintenanceByIDDB.InventoryItem?.Name;
                        MaintenanceDayObj.NumVisits = MaintenanceByIDDB.NumOfVisitsPerProduct;
                        MaintenanceDayObj.ProductBrand = MaintenanceByIDDB.ProductBrand;
                        MaintenanceDayObj.ProductFabricator = MaintenanceByIDDB.ProductFabricator;
                        MaintenanceDayObj.ProductName = MaintenanceByIDDB.ProductName;
                        MaintenanceDayObj.ProductSerial = MaintenanceByIDDB.ProductSerial;
                        MaintenanceDayObj.ProjectID = MaintenanceByIDDB.ProjectId;
                        MaintenanceDayObj.ProjectLocation = ProjectLocationOfItem;

                        var location = MaintenanceByIDDB.SalesOffer.SalesOfferLocations.FirstOrDefault();
                        if (location != null)
                        {
                            MaintenanceDayObj.CountryId = location.CountryId;
                            MaintenanceDayObj.CountryName = location.Country?.Name;
                            MaintenanceDayObj.CityId = location.GovernorateId;
                            MaintenanceDayObj.CityName = location.Governorate?.Name;
                            MaintenanceDayObj.AreaId = location.AreaId;
                            MaintenanceDayObj.AreaName = location.Area?.Name;
                            MaintenanceDayObj.Building = location.BuildingNumber;
                            MaintenanceDayObj.Floor = location.Floor;
                            MaintenanceDayObj.Street = location.Street;
                            MaintenanceDayObj.Description = location.Description;
                            MaintenanceDayObj.Longitude = location.LocationX;
                            MaintenanceDayObj.Latitude = location.LocationY;
                        }

                        MaintenanceDayObj.ProjectName = MaintenanceByIDDB.SalesOffer?.ProjectName;
                        MaintenanceDayObj.SalesOfferID = MaintenanceByIDDB.SalesOfferId;
                        MaintenanceDayObj.ClientID = MaintenanceByIDDB.ClientId;
                        MaintenanceDayObj.GeneralNote = MaintenanceByIDDB.GeneralNote;
                        MaintenanceDayObj.ClientName = MaintenanceByIDDB.Client?.Name;
                        MaintenanceDayObj.NumberOfCheques = MaintenanceByIDDB.ProjectCheques.Count();
                        if (MaintenanceByIDDB?.Client?.HasLogo == true)
                        {
                            MaintenanceDayObj.ClientPhoto = Globals.baseURL + "/ShowImage.ashx?ImageID=" + System.Web.HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(MaintenanceDayObj.ClientID.ToString(), key)) + "&type=photo&CompName=" + companyname.ToString().ToLower();
                        }
                        MaintenanceDayObj.CategoryID = MaintenanceByIDDB.CategoryId;
                        MaintenanceDayObj.CategoryName = MaintenanceByIDDB.CategoryId != null ? GetInventoryItemCategory((int)MaintenanceByIDDB.CategoryId) : "";
                        MaintenanceDayObj.FabOrderID = MaintenanceByIDDB.FabOrderId;
                        // last mileage on this maintenance
                        MaintenanceDayObj.LastMileageCounter = MaintenanceByIDDB.VisitsScheduleOfMaintenances.Where(x => x.VisitDate != null).OrderByDescending(x => x.VisitDate).Select(x => x.MileageCounter).FirstOrDefault();
                        MaintenanceDayObj.InstallationDate = MaintenanceByIDDB.InstallationDate != null ? MaintenanceByIDDB.InstallationDate?.ToShortDateString() : null;
                        MaintenanceDayObj.ProductionDate = MaintenanceByIDDB.ProductionDate != null ? MaintenanceByIDDB.ProductionDate?.ToShortDateString() : null;
                        MaintenanceDayObj.Stops = MaintenanceByIDDB.Stops;
                        MaintenanceDayObj.PRNumber = MaintenanceByIDDB.Prnumber;
                        MaintenanceDayObj.ContractNumber = MaintenanceByIDDB.ContractNumber;
                        MaintenanceDayObj.Capacity = MaintenanceByIDDB.Capacity;

                        //MaintenanceDayObj.MaintenanceProblemAttachments = MaintenanceByIDDB.VisitsScheduleOfMaintenances. != null && VisitMaintenanceDb.VisitsScheduleOfMaintenanceAttachments.Count > 0 ? VisitMaintenanceDb.VisitsScheduleOfMaintenanceAttachments.Select(attach => new Attachment
                        //{
                        //    Id = attach.Id,
                        //    FileExtension = attach.FileExtenssion,
                        //    FileName = attach.FileName,
                        //    FilePath = Globals.baseURL + attach.AttachmentPath.TrimStart('~')
                        //}).ToList() : null

                        Response.MaintenanceForDataByIDObj = MaintenanceDayObj;

                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetMaintenanceForByClientResponse> GetMaintenanceByClient(int ClientID = 0)
        {
            GetMaintenanceForByClientResponse Response = new GetMaintenanceForByClientResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (ClientID == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "ClientID Is Mandatory";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }
                if (Response.Result)
                {

                    var MaintenanceByClientList = new List<MaintenanceForDataByClient>();


                    var MaintenanceByClientDB = await _unitOfWork.MaintenanceFors.FindAllAsync(a => a.ClientId == ClientID, new[] { "Category", "InventoryItem" });



                    if (MaintenanceByClientDB.Count() > 0)
                    {

                        foreach (var item in MaintenanceByClientDB)
                        {

                            var MaintenanceDayObj = new MaintenanceForDataByClient();

                            MaintenanceDayObj.ID = item.Id;
                            MaintenanceDayObj.InventoryItemID = item.InventoryItemId;
                            MaintenanceDayObj.InventoryItemName = item?.InventoryItem?.Name;
                            MaintenanceDayObj.NumVisits = item.NumOfVisitsPerProduct;
                            MaintenanceDayObj.ProductBrand = item.ProductBrand;
                            MaintenanceDayObj.ProductFabricator = item.ProductFabricator;
                            MaintenanceDayObj.ProductName = item.ProductName;
                            MaintenanceDayObj.ProductSerial = item.ProductSerial;
                            MaintenanceDayObj.ProjectID = item.ProjectId;
                            MaintenanceDayObj.SalesOfferID = item.SalesOfferId;
                            MaintenanceDayObj.ProductType = item.ProductType;
                            MaintenanceDayObj.ClientID = item.ClientId;
                            MaintenanceDayObj.CategoryID = item.CategoryId;
                            MaintenanceDayObj.CategoryName = item?.Category?.Name;
                            MaintenanceDayObj.FabOrderID = item.FabOrderId;
                            MaintenanceDayObj.Stops = item.Stops;
                            MaintenanceDayObj.Capacity = item.Capacity;

                            //By Mark Shawky 2023-10-8
                            var Contract = item.ManagementOfMaintenanceOrders != null && item.ManagementOfMaintenanceOrders.Count > 0 ? item.ManagementOfMaintenanceOrders.Where(a => a.ContractStatus == "Open").FirstOrDefault() : null;
                            if (Contract != null)
                            {
                                MaintenanceDayObj.ContractStartDate = Contract.StartDate?.ToString().Split(' ')[0];
                                MaintenanceDayObj.ContractEndDate = Contract.EndDate?.ToString().Split(' ')[0];

                                var LastVisit = Contract.VisitsScheduleOfMaintenances.Where(a => a.VisitDate != null).LastOrDefault();
                                if (LastVisit != null)
                                {
                                    MaintenanceDayObj.LastVisitDate = LastVisit.VisitDate?.ToString().Split(' ')[0]; ;
                                }
                            }
                            MaintenanceByClientList.Add(MaintenanceDayObj);
                        }
                        Response.MaintenanceForDataByClientList = MaintenanceByClientList;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithID> AddMaintenanceFor(MaintenanceForData Request, long creator)
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
                    var ClientIDCheck = _unitOfWork.Clients.FindAllAsync(x => x.Id == Request.ClientID).Result.FirstOrDefault();
                    if (Request.ClientID != 0)
                    {
                        if (ClientIDCheck == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Client ID  Is not Exist !! ";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ClientID ID  Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if ((Request.InventoryItemID != 0 && Request.InventoryItemID != null) || Request.ID == 0)
                    {
                        var InventoryItemIDCheck = _unitOfWork.InventoryItems.FindAllAsync(x => x.Id == Request.InventoryItemID).Result.FirstOrDefault();
                        if (InventoryItemIDCheck == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Inventory Item ID  Doesn't Exist !!";
                            Response.Errors.Add(error);
                        }
                    }
                    if (Request.ID == 0)
                    {
                        if (string.IsNullOrWhiteSpace(Request.ProductName))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Product Name Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (string.IsNullOrWhiteSpace(Request.ProductSerial))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Product Serial Is Mandatory";
                            Response.Errors.Add(error);
                        }
                        if (string.IsNullOrWhiteSpace(Request.ProjectName))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Project Name Is Mandatory";
                            Response.Errors.Add(error);
                        }
                    }
                    if (Request.CategoryID != 0 || Request.ID == 0)
                    {
                        var InventoryItemCategoryIDCheck = _unitOfWork.InventoryItemCategories.FindAllAsync(x => x.Id == Request.CategoryID).Result.FirstOrDefault();
                        if (InventoryItemCategoryIDCheck == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Inventory Item Category ID  Doesn't Exist !!";
                            Response.Errors.Add(error);
                        }
                    }
                    if (Request.ProductBrand?.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Product Brand";
                        Response.Errors.Add(error);
                    }

                    if (Request.ProductFabricator?.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Product Brand";
                        Response.Errors.Add(error);
                    }


                    DateTime? ProductionDate = null;
                    DateTime ProductionDateTemp = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Request.ProductionDate) && DateTime.TryParse(Request.ProductionDate, out ProductionDateTemp))
                    {
                        ProductionDateTemp = DateTime.Parse(Request.ProductionDate);
                        ProductionDate = ProductionDateTemp;
                    }


                    DateTime? InstallationDate = null;
                    DateTime InstallationDateTemp = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Request.InstallationDate) && DateTime.TryParse(Request.InstallationDate, out InstallationDateTemp))
                    {
                        InstallationDateTemp = DateTime.Parse(Request.InstallationDate);
                        InstallationDate = InstallationDateTemp;
                    }

                    if (Response.Result)
                    {
                        if (Request.ID != null && Request.ID != 0)
                        {
                            var MaintenanceForDB = _unitOfWork.MaintenanceFors.FindAllAsync(x => x.Id == Request.ID).Result.FirstOrDefault();
                            if (MaintenanceForDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Maintenance Doesn't Exist!!";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            var SalesOfferDB = _unitOfWork.SalesOffers.FindAllAsync(x => x.Id == MaintenanceForDB.SalesOfferId).Result.FirstOrDefault();

                            // Update
                            if (SalesOfferDB != null)
                            {

                                if (Request.ProjectName != null)
                                {
                                    SalesOfferDB.ProjectName = Request.ProjectName;
                                }
                                SalesOfferDB.ProjectLocation = Request.ProjectLocation;
                                if (Request.ProjectLocationDetails != null)
                                {
                                    var SalesOfferLocationDB = _unitOfWork.SalesOfferLocations.FindAllAsync(x => x.SalesOfferId == SalesOfferDB.Id).Result.FirstOrDefault();
                                    if (SalesOfferLocationDB != null)
                                    {
                                        SalesOfferLocationDB.AreaId = Request.ProjectLocationDetails.AreaId;
                                        SalesOfferLocationDB.CountryId = Request.ProjectLocationDetails.CountryId;
                                        SalesOfferLocationDB.GovernorateId = Request.ProjectLocationDetails.CityId;
                                        SalesOfferLocationDB.BuildingNumber = Request.ProjectLocationDetails.BuildingNumber;
                                        SalesOfferLocationDB.Floor = Request.ProjectLocationDetails.Floor;
                                        SalesOfferLocationDB.Street = Request.ProjectLocationDetails.Street;
                                        SalesOfferLocationDB.Description = Request.ProjectLocationDetails.Description;
                                        SalesOfferLocationDB.LocationX = Request.ProjectLocationDetails.LocationX;
                                        SalesOfferLocationDB.LocationY = Request.ProjectLocationDetails.LocationY;
                                        SalesOfferLocationDB.ModifiedBy = creator.ToString();
                                        SalesOfferLocationDB.ModifiedDate = DateTime.Now;
                                    }
                                    else
                                    {
                                        var SalesOfferLocationObj = new SalesOfferLocation();
                                        SalesOfferLocationObj.SalesOfferId = SalesOfferDB.Id;
                                        SalesOfferLocationObj.CountryId = Request.ProjectLocationDetails.CountryId;
                                        SalesOfferLocationObj.GovernorateId = Request.ProjectLocationDetails.CityId;
                                        SalesOfferLocationObj.AreaId = Request.ProjectLocationDetails.AreaId;
                                        SalesOfferLocationObj.BuildingNumber = Request.ProjectLocationDetails.BuildingNumber;
                                        SalesOfferLocationObj.Floor = Request.ProjectLocationDetails.Floor;
                                        SalesOfferLocationObj.Street = Request.ProjectLocationDetails.Street;
                                        SalesOfferLocationObj.Description = Request.ProjectLocationDetails.Description;
                                        SalesOfferLocationObj.LocationX = Request.ProjectLocationDetails.LocationX;
                                        SalesOfferLocationObj.LocationY = Request.ProjectLocationDetails.LocationY;
                                        SalesOfferLocationObj.Active = true;
                                        SalesOfferLocationObj.ModifiedDate = DateTime.Now;
                                        SalesOfferLocationObj.ModifiedBy = creator.ToString();
                                        SalesOfferLocationObj.CreatedBy = creator.ToString();
                                        SalesOfferLocationObj.CreationDate = DateTime.Now;
                                        _unitOfWork.SalesOfferLocations.Add(SalesOfferLocationObj);
                                    }
                                }
                            }
                            if (Request.CategoryID != 0)
                            {
                                MaintenanceForDB.CategoryId = Request.CategoryID;
                            }
                            if (!string.IsNullOrWhiteSpace(Request.ProductName))
                            {
                                MaintenanceForDB.ProductName = Request.ProductName;
                            }
                            if (!string.IsNullOrWhiteSpace(Request.ProductSerial))
                            {
                                MaintenanceForDB.ProductSerial = Request.ProductSerial;
                            }

                            if (!string.IsNullOrWhiteSpace(Request.ProductBrand))
                            {
                                MaintenanceForDB.ProductBrand = Request.ProductBrand;
                            }
                            if (!string.IsNullOrWhiteSpace(Request.ProductFabricator))
                            {
                                MaintenanceForDB.ProductFabricator = Request.ProductFabricator;
                            }
                            if (Request.FabOrderID != 0)
                            {
                                MaintenanceForDB.FabOrderId = Request.FabOrderID;
                            }
                            if (Request.ClientID != 0)
                            {
                                MaintenanceForDB.ClientId = Request.ClientID;
                            }
                            if (Request.InventoryItemID != 0)
                            {
                                MaintenanceForDB.InventoryItemId = Request.InventoryItemID;
                            }
                            if (Request.VichealID != 0)
                            {
                                MaintenanceForDB.VichealId = Request.VichealID;
                            }
                            if (Request.NumVisits != 0)
                            {
                                MaintenanceForDB.NumOfVisitsPerProduct = Request.NumVisits;
                            }
                            if (!string.IsNullOrEmpty(Request.GeneralNote))
                            {
                                MaintenanceForDB.GeneralNote = Request.GeneralNote;
                            }
                            if (InstallationDate != null)
                            {
                                MaintenanceForDB.InstallationDate = InstallationDate;
                            }
                            if (ProductionDate != null)
                            {
                                MaintenanceForDB.ProductionDate = ProductionDate;
                            }
                            //if (Request.MileageCounter != null)
                            //{
                            //    MaintenanceForDB.MileageCounter = Request.MileageCounter;
                            //}
                            //MaintenanceForDB.ProjectID = Request.ProjectID;
                            if (!string.IsNullOrEmpty(Request.Stops))
                            {
                                MaintenanceForDB.Stops = Request.Stops;
                            }
                            if (!string.IsNullOrEmpty(Request.Capacity))
                            {
                                MaintenanceForDB.Capacity = Request.Capacity;
                            }
                            if (!string.IsNullOrEmpty(Request.ContractNumber))
                            {
                                MaintenanceForDB.ContractNumber = Request.ContractNumber;
                            }
                            if (!string.IsNullOrEmpty(Request.PRNumber))
                            {
                                MaintenanceForDB.Prnumber = Request.PRNumber;
                            }


                            var Result = _unitOfWork.Complete();
                            if (Result == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Update this Maintenance!!";
                                Response.Errors.Add(error);
                            }

                        }
                        else
                        {
                            // Insert

                            long? SalesOfferID = 0;
                            long? ProjectID = 0;

                            var MaintenanceSalesOfeerInsertion = await CreateNewSalesOfferAndProject(creator, Request.ClientID, Request.InventoryItemID, Request.ProjectName, Request.ProjectLocation, Request.ProjectLocationDetails);

                            if (MaintenanceSalesOfeerInsertion.Result)
                            {
                                SalesOfferID = MaintenanceSalesOfeerInsertion.SalesOfferID;
                            }
                            else
                            {
                                Response.Result = false;
                                Response.Errors = MaintenanceSalesOfeerInsertion.Errors;
                                return Response;
                            }
                            if (MaintenanceSalesOfeerInsertion.Result)
                            {
                                ProjectID = MaintenanceSalesOfeerInsertion.ProjectID;
                            }
                            else
                            {
                                Response.Result = false;
                                Response.Errors = MaintenanceSalesOfeerInsertion.Errors;
                                return Response;
                            }

                            if (MaintenanceSalesOfeerInsertion.SalesOfferID != 0)
                            {
                                var check = _unitOfWork.MaintenanceFors.FindAll(a => a.ProductSerial == Request.ProductSerial).FirstOrDefault();
                                if (check != null) 
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Product Serial Already Exist!!";
                                    Response.Errors.Add(error);
                                    return Response;
                                }

                                var MaintenanceDB = new MaintenanceFor();


                                MaintenanceDB.CategoryId = Request.CategoryID;
                                MaintenanceDB.ProductName = Request.ProductName;
                                MaintenanceDB.ProductSerial = Request.ProductSerial;
                                MaintenanceDB.ProductType = "Direct";
                                MaintenanceDB.ProductBrand = Request.ProductBrand;
                                MaintenanceDB.ProductFabricator = Request.ProductFabricator;
                                MaintenanceDB.FabOrderId = Request.FabOrderID;
                                MaintenanceDB.CreatedBy = creator;
                                MaintenanceDB.CreationDate = DateTime.Now;
                                MaintenanceDB.ClientId = Request.ClientID;
                                MaintenanceDB.SalesOfferId = (long)SalesOfferID;
                                MaintenanceDB.ProjectId = (long)ProjectID;
                                MaintenanceDB.VichealId = Request.VichealID;
                                MaintenanceDB.NumOfVisitsPerProduct = Request.NumVisits;
                                MaintenanceDB.InventoryItemId = Request.InventoryItemID;
                                MaintenanceDB.GeneralNote = Request.GeneralNote;
                                MaintenanceDB.ProductionDate = ProductionDate;
                                MaintenanceDB.InstallationDate = InstallationDate;
                                MaintenanceDB.Stops = Request.Stops;
                                MaintenanceDB.Capacity = Request.Capacity;
                                MaintenanceDB.Prnumber = Request.PRNumber;
                                MaintenanceDB.ContractNumber = Request.ContractNumber;
                                //MaintenanceDB.MileageCounter = Request.MileageCounter ?? 0;

                                _unitOfWork.MaintenanceFors.Add(MaintenanceDB);
                                var Res = _unitOfWork.Complete();

                                if (Res > 0)
                                {
                                    Response.ID = MaintenanceDB.Id;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this MaintenanceDB!!";
                                    Response.Errors.Add(error);
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

        public async Task<SalesOfferIDAndProjectID> CreateNewSalesOfferAndProject(long LoginUserID, long ClientID, long? InventoryItemID, string ProjectName, string ProjectLocation, ProjectLocationDetails ProjectLocationDetails)

        {
            SalesOfferIDAndProjectID Response = new SalesOfferIDAndProjectID();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                long NewSalesOfferID = 0;
                long ProjectID = 0;


                DateTime RequestDate = DateTime.Now;
                int POTypeID = 1;


                // Insert Sales Offer 
                var SalesOfferOBJ = new SalesOffer();
                SalesOfferOBJ.Completed = true;
                SalesOfferOBJ.SalesPersonId = LoginUserID;
                SalesOfferOBJ.ClientId = ClientID;
                SalesOfferOBJ.BranchId = _unitOfWork.Users.GetById(LoginUserID)?.BranchId ?? 0;
                SalesOfferOBJ.StartDate = DateOnly.FromDateTime(DateTime.Now);
                SalesOfferOBJ.EndDate = DateOnly.FromDateTime(DateTime.Now);
                SalesOfferOBJ.CreatedBy = LoginUserID;
                SalesOfferOBJ.CreationDate = DateTime.Now;
                SalesOfferOBJ.OfferType = "NewMaintenanceOffer";
                SalesOfferOBJ.ProjectName = ProjectName;
                SalesOfferOBJ.ProjectLocation = ProjectLocation;
                SalesOfferOBJ.Modified = DateTime.Now;
                SalesOfferOBJ.ModifiedBy = LoginUserID;


                _unitOfWork.SalesOffers.Add(SalesOfferOBJ);
                _unitOfWork.Complete();
                //var Res = await _Context.SaveChangesAsync();



                #region items
                var SalesOfferProductObj = new SalesOfferProduct();
                SalesOfferProductObj.OfferId = SalesOfferOBJ.Id;
                SalesOfferProductObj.InventoryItemId = InventoryItemID;
                SalesOfferProductObj.CreatedBy = LoginUserID;
                SalesOfferProductObj.CreationDate = DateTime.Now;
                SalesOfferProductObj.Modified = DateTime.Now;
                SalesOfferProductObj.ModifiedBy = LoginUserID;
                _unitOfWork.SalesOfferProducts.Add(SalesOfferProductObj);

                var SalesMaintenanceOfferobj = new SalesMaintenanceOffer();

                SalesMaintenanceOfferobj.MaintenanceSalesOfferId = SalesOfferOBJ.Id;
                SalesMaintenanceOfferobj.Type = "Maintenance Contract";
                SalesMaintenanceOfferobj.CreationDate = DateTime.Now;
                SalesMaintenanceOfferobj.CreatedBy = LoginUserID;
                SalesMaintenanceOfferobj.Type = "Maintenance Contract";
                SalesMaintenanceOfferobj.ModificationDate = DateTime.Now;
                SalesMaintenanceOfferobj.ModifiedBy = LoginUserID;
                _unitOfWork.SalesMaintenanceOffers.Add(SalesMaintenanceOfferobj);

                var ProjectObj = new Project();
                ProjectObj.SalesOfferId = SalesOfferOBJ.Id;
                ProjectObj.Closed = true;
                ProjectObj.StartDate = DateTime.Now;
                ProjectObj.EndDate = DateTime.Now;
                ProjectObj.Revision = 0;
                ProjectObj.CreatedBy = LoginUserID;
                ProjectObj.CreationDate = DateTime.Now;
                ProjectObj.BranchId = _unitOfWork.Users.GetById(LoginUserID)?.BranchId ?? 0;
                ProjectObj.ModifiedDate = DateTime.Now;
                ProjectObj.ModifiedBy = LoginUserID;

                _unitOfWork.Projects.Add(ProjectObj);


                #endregion items


                #region Project Location
                if (ProjectLocationDetails != null)
                {
                    var user = _unitOfWork.Users.GetById(LoginUserID);
                    string LoginUserName = user?.FirstName + " " + user?.LastName;
                    var SalesOfferLocationObj = new SalesOfferLocation();
                    SalesOfferLocationObj.SalesOfferId = SalesOfferOBJ.Id;
                    SalesOfferLocationObj.CountryId = ProjectLocationDetails.CountryId;
                    SalesOfferLocationObj.GovernorateId = ProjectLocationDetails.CityId;
                    SalesOfferLocationObj.AreaId = ProjectLocationDetails.AreaId;
                    SalesOfferLocationObj.BuildingNumber = ProjectLocationDetails.BuildingNumber;
                    SalesOfferLocationObj.Floor = ProjectLocationDetails.Floor;
                    SalesOfferLocationObj.Street = ProjectLocationDetails.Street;
                    SalesOfferLocationObj.Description = ProjectLocationDetails.Description;
                    SalesOfferLocationObj.LocationX = ProjectLocationDetails.LocationX;
                    SalesOfferLocationObj.LocationY = ProjectLocationDetails.LocationY;
                    SalesOfferLocationObj.Active = true;
                    SalesOfferLocationObj.ModifiedDate = DateTime.Now;
                    SalesOfferLocationObj.ModifiedBy = LoginUserName;
                    SalesOfferLocationObj.CreatedBy = LoginUserName;
                    SalesOfferLocationObj.CreationDate = DateTime.Now;
                    _unitOfWork.SalesOfferLocations.Add(SalesOfferLocationObj);
                }
                #endregion

                var Res = _unitOfWork.Complete();



                if (Res > 0)
                {
                    NewSalesOfferID = SalesOfferOBJ.Id;
                    ProjectID = ProjectObj.Id;
                }

                Response.SalesOfferID = NewSalesOfferID;
                Response.ProjectID = ProjectID;
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

        public async Task<BaseResponseWithID> DeleteMaintenanceFor(DeleteMaintenanceRequest Request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // Just will use ID from Model 
                    if (Request.ID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "ID is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MaintenanceDB = _unitOfWork.MaintenanceFors.FindAllAsync(x => x.Id == Request.ID).Result.FirstOrDefault();
                    if (MaintenanceDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Maintenance.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var VisitMaintenanceDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(x => x.MaintenanceForId == Request.ID).Result.FirstOrDefault();
                    if (VisitMaintenanceDB != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "You Cannot Delete Maitenance Has Visits.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {

                        var ManageOfMaintenanceListDb = _unitOfWork.ManagementOfMaintenanceOrders.FindAllAsync(x => x.MaintenanceForId == Request.ID).Result.ToList();
                        if (ManageOfMaintenanceListDb != null && ManageOfMaintenanceListDb.Count > 0)
                        {
                            _unitOfWork.ManagementOfMaintenanceOrders.DeleteRange(ManageOfMaintenanceListDb);
                        }

                        _unitOfWork.MaintenanceFors.Delete(MaintenanceDB);

                        var ActionResult = _unitOfWork.Complete();
                        if (ActionResult == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Delete Maintenance not completed ,Please try again.";
                            Response.Errors.Add(error);
                            return Response;
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

        public async Task<MaintenanceForDetailsResponse> GetMaintenanceForDetailsList(MaintenanceDetailsListCallFilters filters, string CompanyName)
        {
            MaintenanceForDetailsResponse response = new MaintenanceForDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    // Method Call Get Maintinance Details List
                    response = await MaintenanceDetailsListCall(filters, CompanyName);
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public List<long> GetNearestSalesOffers(decimal latitude, decimal longitude, decimal? radius)
        {
            decimal? Proc_Radius = null;
            if (radius != null && radius > 0)
                Proc_Radius = radius;

            var Radius = new SqlParameter("Radius", System.Data.SqlDbType.Decimal);
            Radius.Value = radius;

            var Latitude = new SqlParameter("Latitude", System.Data.SqlDbType.Decimal);
            Latitude.Value = latitude;

            var Longitude = new SqlParameter("Longitude", System.Data.SqlDbType.Decimal);
            Longitude.Value = longitude;

            object[] param = new object[] { Latitude, Longitude, Radius };

            var NearsSalesOffersDb = _Context.Database.SqlQueryRaw<STP_GetNearestSalesOffers_Result>("Exec STP_GetNearestSalesOffers @Latitude, @Longitude, @Radius", param).AsEnumerable();
            List<long> NearestSalesOffersIds = new List<long>();
            if (NearsSalesOffersDb != null && NearsSalesOffersDb.Count() > 0)
            {
                NearestSalesOffersIds = NearsSalesOffersDb.Select(x => x.SalesOfferId).ToList();
            }
            return NearestSalesOffersIds;
        }
        public async Task<MaintenanceForDetailsResponse> MaintenanceDetailsListCall(MaintenanceDetailsListCallFilters filters, string CompanyName)
        {
            MaintenanceForDetailsResponse response = new MaintenanceForDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            List<long> AreaIdsList = new List<long>();
            if (!string.IsNullOrWhiteSpace(filters.AreaIdsString))
            {
                var AreaIdsString = HttpUtility.UrlDecode(filters.AreaIdsString).ToLower();
                if (!string.IsNullOrWhiteSpace(AreaIdsString))
                {
                    try
                    {
                        AreaIdsList = AreaIdsString.Split(',').Select(long.Parse).ToList();

                    }
                    catch (Exception ex)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err110";
                        error.ErrorMSG = "Area Ids incorrect format";
                        response.Errors.Add(error);
                    }
                }
            }
            bool locationFilter = false;
            if (filters.Latitude != null)
            {

                if (filters.Longitude != null)
                {
                    locationFilter = true;
                }
                else
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Longitude Is Mandatory";
                    response.Errors.Add(error);
                    response.Result = false;
                }
            }
            else
            {
                if (filters.Longitude != null)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Latitude Is Mandatory";
                    response.Errors.Add(error);
                    response.Result = false;
                }
            }
            var MaintenanceList = _unitOfWork.MaintenanceFors.FindAllQueryable(x => x.ManagementOfMaintenanceOrders.Count() > 0, includes: new[] { "VisitsScheduleOfMaintenances", "ManagementOfMaintenanceOrders", "SalesOffer", "Client.ClientContactPeople", "Category", "Client", "Client.ClientMobiles", "Client.ClientAddresses" });
            var GetManagementOfMaintenanceOrderFiltered = _unitOfWork.ManagementOfMaintenanceOrders.FindAll(x => x.Active && x.ContractStatus != "Closed");

            if (filters.ClientID != 0)
            {
                MaintenanceList = MaintenanceList.Where(x => x.ClientId == filters.ClientID).AsQueryable();
            }
            if (locationFilter)
            {
                List<long> NearestSalesOfferIds = GetNearestSalesOffers((decimal)filters.Latitude, (decimal)filters.Longitude, filters.Radius);
                MaintenanceList = MaintenanceList.Where(x => NearestSalesOfferIds.Contains(x.SalesOfferId)).AsQueryable();
            }

            if (filters.CategoryID != 0)
            {
                MaintenanceList = MaintenanceList.Where(x => x.CategoryId == filters.CategoryID).AsQueryable();
            }
            if (!string.IsNullOrWhiteSpace(filters.MaintenanceType))
            {
                MaintenanceList = MaintenanceList.Where(x => x.ProductType.Contains(filters.MaintenanceType)).AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(filters.ProductBrand))
            {
                MaintenanceList = MaintenanceList.Where(x => x.ProductBrand.Contains(filters.ProductBrand)).AsQueryable();
            }
            if (!string.IsNullOrWhiteSpace(filters.ProductFabricator))
            {
                MaintenanceList = MaintenanceList.Where(x => x.ProductFabricator.Contains(filters.ProductFabricator)).AsQueryable();
            }
            if (!string.IsNullOrWhiteSpace(filters.SearchKey))
            {
                MaintenanceList = MaintenanceList.Where(x => x.ProductName.ToLower().Contains(filters.SearchKey.ToLower())
                                                                                                 || x.ProductSerial.ToLower().Contains(filters.SearchKey.ToLower())
                                                                                                 || x.Client.ClientContactPeople.Any(a => a.Mobile.Contains(filters.SearchKey))
                                                                                                 || x.SalesOffer.ProjectName.ToLower().Contains(filters.SearchKey.ToLower())

                ).AsQueryable();
            }
            var SalesOfferLocationList = new List<SalesOfferLocation>();
            if (!string.IsNullOrWhiteSpace(filters.ContractType))
            {
                filters.ContractType = HttpUtility.UrlDecode(filters.ContractType);
                MaintenanceList = MaintenanceList.Where(x => x.ManagementOfMaintenanceOrders.Any(b => b.Active && b.ContractType.Contains(filters.ContractType))).AsQueryable();
            }
            if (AreaIdsList.Count() > 0)
            {
                SalesOfferLocationList = _unitOfWork.SalesOfferLocations.FindAll(x => AreaIdsList.Contains(x.AreaId ?? 0), new[] { "Area" }).ToList();
                var SalesOfferIdList = SalesOfferLocationList.Select(x => x.SalesOfferId).ToList();
                MaintenanceList = MaintenanceList.Where(x => SalesOfferIdList.Contains(x.SalesOfferId)).AsQueryable();
            }
            if (filters.FromDate != null && filters.FromDate != DateTime.MinValue)
            {
                MaintenanceList = MaintenanceList.Where(x => x.VisitsScheduleOfMaintenances.Where(z => z.VisitDate >= filters.FromDate).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
            }

            if (filters.ToDate != null && filters.ToDate != DateTime.MinValue)
            {
                MaintenanceList = MaintenanceList.Where(x => x.VisitsScheduleOfMaintenances.Where(z => z.VisitDate <= filters.ToDate).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
            }


            if (filters.ContractStatus != null && filters.ContractStatus == "Valid")
            {

                MaintenanceList = MaintenanceList.Where(x => x.ManagementOfMaintenanceOrders.Where(z => z.Active && z.StartDate >= DateTime.Today && z.EndDate >= DateTime.Today).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
            }

            if (filters.ContractStatus != null && filters.ContractStatus == "Expired")
            {

                MaintenanceList = MaintenanceList.Where(x => x.ManagementOfMaintenanceOrders.Where(z => z.Active && z.EndDate < DateTime.Today).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
            }
            /*
             -must be end date  > today
             -subtract end date from today if smaller than the target request 
             - return this contract because this is filter result
            */
            if (filters.ContractStatus != null && filters.ContractStatus == "ExpireSoon" && filters.WeekNum != 0)
            {
                TimeSpan WeekNumberConv = TimeSpan.Parse((7 * filters.WeekNum).ToString());

                GetManagementOfMaintenanceOrderFiltered = GetManagementOfMaintenanceOrderFiltered.Where(z => z.Active && z.EndDate != null && z.EndDate > DateTime.Now).ToList();

                if (filters.ClientID != 0)
                {
                    GetManagementOfMaintenanceOrderFiltered = GetManagementOfMaintenanceOrderFiltered.Where(x => x.MaintenanceFor?.ClientId == filters.ClientID).ToList();
                }

                var MaintenanceForIDsList = new List<long>();

                foreach (var item in GetManagementOfMaintenanceOrderFiltered)
                {
                    if (((DateTime)item.EndDate).AddDays(-7 * filters.WeekNum) <= DateTime.Today)
                    {
                        if (item.MaintenanceForId != null)
                        {

                            MaintenanceForIDsList.Add((long)item.MaintenanceForId);
                        }
                    }
                }

                MaintenanceList = MaintenanceList.Where(x => MaintenanceForIDsList.Contains(x.Id)).AsQueryable();

            }
            // Filter WithInDayNo Next Planned date with in No of Days
            if (filters.WithInDayNo != 0)
            {
                DateTime DateTodayWithinNoDaysAfter = DateTime.Now.AddDays(7);
                DateTime DateTodayWithinNoDaysBefore = DateTime.Now.AddDays(-7);
                MaintenanceList = MaintenanceList.Where(x => x.VisitsScheduleOfMaintenances.Where(y => y.PlannedDate >= DateTodayWithinNoDaysBefore && y.PlannedDate <= DateTodayWithinNoDaysAfter).Any());
            }

            // var List = MaintenanceList.ToList();

            var MaintenancePagingList = PagedList<MaintenanceFor>.Create(MaintenanceList.OrderByDescending(x => x.CreationDate), filters.CurrentPage, filters.NumberOfItemsPerPage);

            response.PaginationHeader = new PaginationHeader
            {
                CurrentPage = filters.CurrentPage,
                TotalPages = MaintenancePagingList.TotalPages,
                ItemsPerPage = filters.NumberOfItemsPerPage,
                TotalItems = MaintenancePagingList.TotalCount
            };

            // Fill MaintenanceProductPerCategory
            List<MaintenanceProductPerCategory> maintenanceProductPerCategories = MaintenanceList.Where(x => x.Category != null).GroupBy(x => x.Category).Select(x => new MaintenanceProductPerCategory { CategoryId = x.Key.Id, CategoryName = x.Key.Name, NoOFMaintenance = x.Count() }).ToList();
            response.MaintenanceProductPerCategory = maintenanceProductPerCategories;

            // Fill MaintenanceContractPerContractType
            List<MaintenanceContractPerContractType> MaintenanceContractPerContractTypes = GetManagementOfMaintenanceOrderFiltered.GroupBy(x => x.ContractType).Select(a => new MaintenanceContractPerContractType { ContractType = a.Key, NoOFContract = a.Count() }).ToList();
            response.MaintenanceContractPerContractType = MaintenanceContractPerContractTypes;

            response.NoOFclient = MaintenanceList.Select(x => x.ClientId).Distinct().Count();

            if (MaintenancePagingList != null)
            {

                var MaintenanceListVM = new List<MaintenanceForDetails>();
                foreach (var Maintenanceitem in MaintenancePagingList)
                {
                    var MaintenanceForDetailsObject = new MaintenanceForDetails();
                    MaintenanceForDetailsObject.ID = Maintenanceitem.Id;
                    MaintenanceForDetailsObject.ProjectName = Maintenanceitem.SalesOffer.ProjectName;
                    MaintenanceForDetailsObject.ProjectLocation = Maintenanceitem.SalesOffer.ProjectLocation;
                    MaintenanceForDetailsObject.AreaId = Maintenanceitem.SalesOffer.SalesOfferLocations.FirstOrDefault()?.AreaId;
                    MaintenanceForDetailsObject.AreaName = Maintenanceitem.SalesOffer.SalesOfferLocations.FirstOrDefault()?.Area?.Name;
                    // Common.GetProjectName(Maintenanceitem.ProjectID);
                    MaintenanceForDetailsObject.ClientID = Maintenanceitem.ClientId;
                    MaintenanceForDetailsObject.ClientMobile = Maintenanceitem.Client.ClientMobiles.Select(a => a.Mobile).FirstOrDefault();
                    MaintenanceForDetailsObject.ContactPersonMobile = Maintenanceitem.Client.ClientContactPeople.Select(a => a.Mobile).FirstOrDefault();
                    MaintenanceForDetailsObject.ClientAddress = Maintenanceitem.Client.ClientAddresses.Select(a => a.Address).FirstOrDefault();
                    MaintenanceForDetailsObject.ClientLocation = Maintenanceitem.Client.ClientContactPeople.Select(a => a.Location).FirstOrDefault();
                    MaintenanceForDetailsObject.ClientName = Maintenanceitem.Client?.Name; // Common.GetClientName(Maintenanceitem.ClientID);
                    MaintenanceForDetailsObject.ContactPersonName = Maintenanceitem.Client?.ClientContactPeople?.FirstOrDefault()?.Name;
                    MaintenanceForDetailsObject.ContractType = Maintenanceitem.ManagementOfMaintenanceOrders.Select(a => a.ContractType).FirstOrDefault();
                    MaintenanceForDetailsObject.VisitMaintenanceID = Maintenanceitem.VisitsScheduleOfMaintenances.Select(a => a.Id).FirstOrDefault();
                    if (Maintenanceitem?.Client?.HasLogo == true && Maintenanceitem?.Client?.Logo != null)
                    {
                        MaintenanceForDetailsObject.ClientLogo = Globals.baseURL + Maintenanceitem?.Client?.Logo;
                    }

                    if (Maintenanceitem.Client?.NeedApproval == 2)
                    {
                        MaintenanceForDetailsObject.ClientIsBlocked = true;
                    }

                    MaintenanceForDetailsObject.Brand = Maintenanceitem.ProductBrand;
                    MaintenanceForDetailsObject.ProductName = Maintenanceitem.ProductName;
                    MaintenanceForDetailsObject.ProductType = Maintenanceitem.ProductType;
                    MaintenanceForDetailsObject.ProductSerial = Maintenanceitem.ProductSerial;
                    MaintenanceForDetailsObject.InventoryCategory = Maintenanceitem.Category?.Name;
                    MaintenanceForDetailsObject.Name = Maintenanceitem.ProductName;
                    var SalesOfferLocations = Maintenanceitem.SalesOffer?.SalesOfferLocations?.FirstOrDefault();
                    if (SalesOfferLocations != null)
                    {
                        MaintenanceForDetailsObject.Latitude = SalesOfferLocations.LocationX;
                        MaintenanceForDetailsObject.Longitude = SalesOfferLocations.LocationY;
                        MaintenanceForDetailsObject.Location = SalesOfferLocations.Description;
                    }
                    MaintenanceForDetailsObject.lastVisitDate = Maintenanceitem.VisitsScheduleOfMaintenances.OrderByDescending(x => x.VisitDate).Select(y => y.VisitDate).FirstOrDefault().ToString();
                    MaintenanceForDetailsObject.NextPlannedDate = Maintenanceitem.VisitsScheduleOfMaintenances.Where(x => x.VisitDate == null).OrderBy(x => x.PlannedDate).Select(y => y.PlannedDate).FirstOrDefault().ToString();
                    MaintenanceForDetailsObject.NextVisitId = Maintenanceitem.VisitsScheduleOfMaintenances.Where(x => x.VisitDate == null).OrderBy(x => x.PlannedDate).Select(y => y.Id).FirstOrDefault();
                    var NumberOfVisitsWithOutContract = Maintenanceitem.VisitsScheduleOfMaintenances.Where(x => x.MaintenanceVisitType != null).Count();
                    var CostVisitsWithOutContract = Maintenanceitem.VisitsScheduleOfMaintenances.Sum(x => x.MaintenanceReports.Sum(z => z.CollectedAmount));

                    MaintenanceForDetailsObject.NumberOfVisitsWithOutContract = NumberOfVisitsWithOutContract;
                    MaintenanceForDetailsObject.CostVisitsWithOutContract = CostVisitsWithOutContract;
                    MaintenanceForDetailsObject.WithContract = Maintenanceitem.ManagementOfMaintenanceOrders != null;


                    var contractDB = Maintenanceitem.ManagementOfMaintenanceOrders?.LastOrDefault();
                    if (contractDB != null)
                    {

                        MaintenanceForDetailsObject.ManagementMaintenanctOrderID = contractDB.Id;
                        MaintenanceForDetailsObject.StartDate = contractDB.StartDate?.ToShortDateString();
                        MaintenanceForDetailsObject.EndDate = contractDB.EndDate?.ToShortDateString();
                        MaintenanceForDetailsObject.Cost = contractDB.ContractPrice;
                        MaintenanceForDetailsObject.remainVisitsNo = contractDB.NumberOfVisits;
                        MaintenanceForDetailsObject.ClientSatisfactionRate = Maintenanceitem.VisitsScheduleOfMaintenances
                            .Select(y => y.MaintenanceReports.Where(c => c.ClientSatisfactionRate != null).Average(c => c.ClientSatisfactionRate)).Sum();
                        //                        await _Context.V_VisitsMaintenance_Report_Users.Where(x => x.ClientID == ClientID &&
                        //                                                                                       x.ClientSatisfactionRate != null)
                        //.GroupBy(x => new { x.ClientID, x.ID }).Select(x => x.Average(y => y.ClientSatisfactionRate)).DefaultIfEmpty(0).SumAsync();
                    }
                    MaintenanceForDetailsObject.MaintenanceForID = Maintenanceitem.Id;


                    //MaintenanceForDetailsObject.ContractDetailsList = Maintenanceitem.ManagementOfMaintenanceOrders?.Select(x => new ContractDetails
                    //{


                    //    ManagementMaintenanctOrderID = x.ID,
                    //    StartDate = x.StartDate?.ToShortDateString();,
                    //    EndDate = x.EndDate?.ToShortDateString();,
                    //    Cost = x.ContractPrice,
                    //    RemainNoVisit = x.NumberOfVisits - (Maintenanceitem.VisitsScheduleOfMaintenances.Where(y => y.ManagementOfMaintenanceOrderID == x.ID).Count())
                    //}).ToList();
                    //MaintenanceForDetailsObject.ContractDetailsList = MaintenanceForDetailsObject.ContractDetailsList;
                    MaintenanceListVM.Add(MaintenanceForDetailsObject);
                }

                response.MaintenanceForDetailsList = MaintenanceListVM;
                response.TotalCost = response.MaintenanceForDetailsList.Sum(a => a.Cost ?? 0);

            }




            return response;
        }



        public async Task<GetManagementOfMaintenanceOrder> GetManagementOfMaintenanceOrderByID(int MaintenanceForID)
        {
            GetManagementOfMaintenanceOrder Response = new GetManagementOfMaintenanceOrder();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (MaintenanceForID == 0)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "MaintenanceForID Is Mandatory";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }

                if (Response.Result)
                {

                    var ManagementOfMaintenanceOrderDataObj = new ManagementOfMaintenanceOrderData();


                    var ManagementOfMaintenanceOrderDataObjDB = await _unitOfWork.ManagementOfMaintenanceOrders.FindAsync(a => a.Active && a.MaintenanceForId == MaintenanceForID, new[] { "Project.SalesOffer", "ProjectCheques", "Currency" });
                    if (ManagementOfMaintenanceOrderDataObjDB != null)
                    {
                        ManagementOfMaintenanceOrderDataObj.ID = ManagementOfMaintenanceOrderDataObjDB.Id;
                        ManagementOfMaintenanceOrderDataObj.CreatedBy = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(ManagementOfMaintenanceOrderDataObjDB.CreatedBy.ToString(), key));
                        ManagementOfMaintenanceOrderDataObj.CreationDate = ManagementOfMaintenanceOrderDataObjDB.CreationDate.ToString("yyyy-MM-dd");
                        ManagementOfMaintenanceOrderDataObj.MaintenanceForID = (long)ManagementOfMaintenanceOrderDataObjDB.MaintenanceForId;
                        ManagementOfMaintenanceOrderDataObj.MaintenanceOfferID = ManagementOfMaintenanceOrderDataObjDB.MaintenanceOfferId;
                        ManagementOfMaintenanceOrderDataObj.NumberOfVisits = ManagementOfMaintenanceOrderDataObjDB.NumberOfVisits;
                        ManagementOfMaintenanceOrderDataObj.ProjectID = ManagementOfMaintenanceOrderDataObjDB.ProjectId;
                        ManagementOfMaintenanceOrderDataObj.ProjectName = ManagementOfMaintenanceOrderDataObjDB.Project?.SalesOffer?.ProjectName;


                        var SalesOfferLocation = _unitOfWork.SalesOfferLocations.FindAll(x => x.SalesOfferId == ManagementOfMaintenanceOrderDataObjDB.Project.SalesOfferId, new[] { "Country", "Governorate", "Area" })?.FirstOrDefault(); // Location maintenance
                        if (SalesOfferLocation != null)
                        {
                            ManagementOfMaintenanceOrderDataObj.CountryId = SalesOfferLocation.CountryId;
                            ManagementOfMaintenanceOrderDataObj.CityId = SalesOfferLocation.GovernorateId;
                            ManagementOfMaintenanceOrderDataObj.AreaId = SalesOfferLocation.AreaId;

                            ManagementOfMaintenanceOrderDataObj.CountryName = SalesOfferLocation.Country?.Name;
                            ManagementOfMaintenanceOrderDataObj.CityName = SalesOfferLocation.Governorate?.Name;
                            ManagementOfMaintenanceOrderDataObj.AreaName = SalesOfferLocation.Area?.Name;

                            ManagementOfMaintenanceOrderDataObj.Building = SalesOfferLocation.BuildingNumber;
                            ManagementOfMaintenanceOrderDataObj.Floor = SalesOfferLocation.Floor;
                            ManagementOfMaintenanceOrderDataObj.Street = SalesOfferLocation.Street;
                            ManagementOfMaintenanceOrderDataObj.Description = SalesOfferLocation.Description;
                            ManagementOfMaintenanceOrderDataObj.Longitude = SalesOfferLocation.LocationX;
                            ManagementOfMaintenanceOrderDataObj.Latitude = SalesOfferLocation.LocationY;
                        }


                        ManagementOfMaintenanceOrderDataObj.RateToLocalCu = ManagementOfMaintenanceOrderDataObjDB.RateToLocalCu;
                        ManagementOfMaintenanceOrderDataObj.StartDate = ManagementOfMaintenanceOrderDataObjDB.StartDate?.ToShortDateString();
                        ManagementOfMaintenanceOrderDataObj.NumberOfCheques = ManagementOfMaintenanceOrderDataObjDB.ProjectCheques.Count();
                        if (!string.IsNullOrWhiteSpace(ManagementOfMaintenanceOrderDataObjDB.WarrentyCertificateAttachment))
                        {

                            ManagementOfMaintenanceOrderDataObj.WarrentyCertificateAttachment = Globals.baseURL + ManagementOfMaintenanceOrderDataObjDB.WarrentyCertificateAttachment;
                        }
                        ManagementOfMaintenanceOrderDataObj.Active = ManagementOfMaintenanceOrderDataObjDB.Active;
                        if (!string.IsNullOrWhiteSpace(ManagementOfMaintenanceOrderDataObjDB.ContractAttachment))
                        {
                            ManagementOfMaintenanceOrderDataObj.ContractAttachement = Globals.baseURL + ManagementOfMaintenanceOrderDataObjDB.ContractAttachment;
                        }
                        ManagementOfMaintenanceOrderDataObj.ContractPrice = ManagementOfMaintenanceOrderDataObjDB.ContractPrice;
                        ManagementOfMaintenanceOrderDataObj.ContractStatus = ManagementOfMaintenanceOrderDataObjDB.ContractStatus;
                        ManagementOfMaintenanceOrderDataObj.CurrencyID = ManagementOfMaintenanceOrderDataObjDB.CurrencyId;
                        ManagementOfMaintenanceOrderDataObj.CurrencyName = ManagementOfMaintenanceOrderDataObjDB.Currency?.Name;
                        ManagementOfMaintenanceOrderDataObj.EndDate = ManagementOfMaintenanceOrderDataObjDB.EndDate?.ToShortDateString();
                        ManagementOfMaintenanceOrderDataObj.ContractType = ManagementOfMaintenanceOrderDataObjDB.ContractType;
                        ManagementOfMaintenanceOrderDataObj.ClosingContractType = ManagementOfMaintenanceOrderDataObjDB.ClosingContractType;
                        ManagementOfMaintenanceOrderDataObj.ClosingMileageCounter = ManagementOfMaintenanceOrderDataObjDB.ClosingMileageCounter;
                        ManagementOfMaintenanceOrderDataObj.CurrentMileageCounter = ManagementOfMaintenanceOrderDataObjDB.CurrentMileageCounter;
                        ManagementOfMaintenanceOrderDataObj.ContractNumber = ManagementOfMaintenanceOrderDataObjDB.ContractNumber;

                        Response.ManagementOfMaintenanceOrderDataObj = ManagementOfMaintenanceOrderDataObj;
                    }
                    var ManagmentListdB = await _unitOfWork.ManagementOfMaintenanceOrders.FindAllAsync(a => a.Active && a.MaintenanceForId == MaintenanceForID, new[] { "Project.SalesOffer.SalesOfferLocations", "Project.SalesOffer.SalesOfferLocations.Country", "Project.SalesOffer.SalesOfferLocations.Governorate", "Project.SalesOffer.SalesOfferLocations.Area", "ProjectCheques" });
                    var AccountOfMovementListdb = ManagmentListdB.Select((item, index) =>
                    {
                        return new ManagementOfMaintenanceOrderData
                        {

                            ID = item.Id,
                            CreatedBy = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(item.CreatedBy.ToString(), key)),
                            CreationDate = item.CreationDate.ToString("yyyy-MM-dd"),
                            MaintenanceForID = (long)item.MaintenanceForId,
                            MaintenanceOfferID = item.MaintenanceOfferId,
                            NumberOfVisits = item.NumberOfVisits,
                            ProjectID = item.ProjectId,
                            ProjectName = item.Project?.SalesOffer?.ProjectName,
                            RateToLocalCu = item.RateToLocalCu,
                            StartDate = item.StartDate?.ToShortDateString(),
                            EndDate = item.EndDate?.ToShortDateString(),
                            Active = item.Active,
                            ContractPrice = item.ContractPrice,
                            ContractStatus = item.ContractStatus,
                            CurrencyID = item.CurrencyId,
                            CurrencyName = item.Currency?.Name,
                            ContractType = item.ContractType,
                            ClosingContractType = item.ClosingContractType,
                            ClosingMileageCounter = item.ClosingMileageCounter,
                            CurrentMileageCounter = item.CurrentMileageCounter,
                            ContractAttachement = !string.IsNullOrWhiteSpace(item.ContractAttachment) ? Globals.baseURL + item.ContractAttachment : null,
                            WarrentyCertificateAttachment = !string.IsNullOrWhiteSpace(item.ContractAttachment) ? Globals.baseURL + item.WarrentyCertificateAttachment : null,
                            ContractAttachementExtension = Path.GetExtension(item.ContractAttachment),
                            WarrentyCertificateAttachmentExtension = Path.GetExtension(item.WarrentyCertificateAttachment),
                            ContractNumber = item.ContractNumber,
                            NumberOfCheques = item.ProjectCheques.Count(),
                            CountryId = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.CountryId).FirstOrDefault(),
                            CountryName = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.Country?.Name).FirstOrDefault(),
                            CityId = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.GovernorateId).FirstOrDefault(),
                            CityName = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.Governorate?.Name).FirstOrDefault(),
                            AreaId = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.AreaId).FirstOrDefault(),
                            AreaName = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.Area?.Name).FirstOrDefault(),
                            Building = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.BuildingNumber).FirstOrDefault(),
                            Floor = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.Floor).FirstOrDefault(),
                            Street = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.Street).FirstOrDefault(),
                            Description = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.Description).FirstOrDefault(),
                            Longitude = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.LocationX).FirstOrDefault(),
                            Latitude = item.Project.SalesOffer.SalesOfferLocations.Select(a => a.LocationY).FirstOrDefault(),
                        };
                    }).ToList();



                    Response.ManagementOfMaintenanceOrderDataList = AccountOfMovementListdb;
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

        public async Task<BaseResponseWithID> AddManagementOfMaintenanceOrder(ManagementOfMaintenanceOrderData Request, long UserID, string CompanyName)
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

                    var maintenanceforCheck = await _Context.MaintenanceFors.Where(x => x.Id == Request.MaintenanceForID).FirstOrDefaultAsync();

                    if (maintenanceforCheck == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Maintenance For ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    // validate Closing COntract Type
                    // (Ending Date-After Period Time-After No Visit-After Mileage Counter) 
                    string ClosingContractType = "Ending Date";
                    if (Request.ClosingContractType != null)
                    {
                        ClosingContractType = Request.ClosingContractType;
                        if (ClosingContractType == "After Mileage Counter")
                        {
                            if (Request.ClosingMileageCounter == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Please Insert Mileage Counter based on mileage counter selected on closing contract type .";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                    }




                    DateTime? StartDate = null;
                    DateTime StartDateTemp = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Request.StartDate) && DateTime.TryParse(Request.StartDate, out StartDateTemp))
                    {
                        StartDateTemp = DateTime.Parse(Request.StartDate);
                        StartDate = StartDateTemp;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Start Date ";
                        Response.Errors.Add(error);
                    }

                    DateTime? EndDate = null;
                    DateTime EndDateTemp = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Request.EndDate) && DateTime.TryParse(Request.EndDate, out EndDateTemp))
                    {
                        EndDateTemp = DateTime.Parse(Request.EndDate);
                        EndDate = EndDateTemp;
                    }
                    if (ClosingContractType != "After No Visit" && ClosingContractType != "After Mileage Counter")
                    {

                        if (EndDate == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Invalid End Date ";
                            Response.Errors.Add(error);
                        }

                        if (EndDate < StartDate)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "End Date must be larger than the start Date  !!";
                            Response.Errors.Add(error);
                        }
                        if (EndDate < DateTime.Today)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "End Date must be larger than Today  !!";
                            Response.Errors.Add(error);
                        }
                    }
                    //if (StartDate <= DateTime.Today)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Start Date must be larger than today  !!";
                    //    Response.Errors.Add(error);
                    //}

                    if (Request.NumberOfVisits == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "The Number Of Visits is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    var MaintenanceManagementOrderDB = new ManagementOfMaintenanceOrder();

                    if (Response.Result)
                    {

                        if (Request.ID != 0)
                        {
                            MaintenanceManagementOrderDB = await _Context.ManagementOfMaintenanceOrders.Where(x => x.Id == Request.ID).FirstOrDefaultAsync();
                            if (MaintenanceManagementOrderDB != null)
                            {
                                // Update
                                if (Request.ContractStatus != null) // validate if sent only
                                {
                                    if (Request.ContractStatus != "Closed")
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Contract Status";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }

                                }

                                if (MaintenanceManagementOrderDB != null)
                                {
                                    MaintenanceManagementOrderDB.Active = true;
                                    MaintenanceManagementOrderDB.ContractPrice = Request.ContractPrice;
                                    if (Request.ContractStatus != null)
                                    {
                                        MaintenanceManagementOrderDB.ContractStatus = Request.ContractStatus;
                                        if (Request.ClosingReason != null)
                                        {
                                            MaintenanceManagementOrderDB.ClosingReason = Request.ClosingReason;
                                        }
                                    }
                                    MaintenanceManagementOrderDB.CreatedBy = UserID;
                                    MaintenanceManagementOrderDB.CreationDate = DateTime.Now;
                                    MaintenanceManagementOrderDB.CurrencyId = Request.CurrencyID;
                                    MaintenanceManagementOrderDB.StartDate = StartDate;
                                    MaintenanceManagementOrderDB.EndDate = EndDate;
                                    MaintenanceManagementOrderDB.MaintenanceForId = Request.MaintenanceForID;
                                    MaintenanceManagementOrderDB.ModificationDate = DateTime.Now;
                                    MaintenanceManagementOrderDB.ModifiedBy = UserID;
                                    MaintenanceManagementOrderDB.NumberOfVisits = Request.NumberOfVisits;
                                    MaintenanceManagementOrderDB.RateToLocalCu = Request.RateToLocalCu;
                                    MaintenanceManagementOrderDB.ContractType = Request.ContractType;
                                    MaintenanceManagementOrderDB.ClosingContractType = Request.ClosingContractType;
                                    MaintenanceManagementOrderDB.ClosingMileageCounter = Request.ClosingMileageCounter ?? 0;
                                    MaintenanceManagementOrderDB.CurrentMileageCounter = Request.CurrentMileageCounter ?? 0;
                                    //MaintenanceManagementOrderDB.WarrentyCertificateAttachment = Request.WarrentyCertificateAttachment;








                                    //var CompanyName = headers["CompanyName"].ToString().ToLower();
                                    string FilePathContract = null;
                                    if (Request.ContractAttachementContent != null)
                                    {
                                        //FilePathContract =  Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/", Request.ContractAttachementContent, Request.ContractAttachementName, Request.ContractAttachementExtension);

                                        var fileExtension = Request.ContractAttachementContent.FileName.Split('.').Last();
                                        var virtualPath = $"Attachments\\{CompanyName}\\Maintenance\\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.ContractAttachementContent.FileName.Trim().Replace(" ", ""));
                                        FilePathContract = Common.SaveFileIFF(virtualPath, Request.ContractAttachementContent, FileName, fileExtension, _host);



                                        if (MaintenanceManagementOrderDB.ContractAttachment != null && MaintenanceManagementOrderDB.ContractAttachment != "")
                                        {
                                            string exsistFilePath = Path.Combine(_host.WebRootPath, MaintenanceManagementOrderDB.ContractAttachment);
                                            if (System.IO.File.Exists(exsistFilePath))
                                            {
                                                System.IO.File.Delete(exsistFilePath);
                                            }
                                        }
                                        MaintenanceManagementOrderDB.ContractAttachment = FilePathContract;
                                    }


                                    string FilePathWarrenty = null;
                                    if (Request.WarrentyCertificateAttachmentContent != null)
                                    {
                                        //FilePath = await Common.SaveFileAsync("/Attachments/" + CompanyName + "/Maintenance/", Request.WarrentyCertificateAttachmentContent, Request.WarrentyCertificateAttachmentName, Request.WarrentyCertificateAttachmentExtension);

                                        var fileExtension = Request.WarrentyCertificateAttachmentContent.FileName.Split('.').Last();
                                        var virtualPath = $"Attachments\\{CompanyName}\\Maintenance\\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.WarrentyCertificateAttachmentContent.FileName.Trim().Replace(" ", ""));
                                        FilePathWarrenty = Common.SaveFileIFF(virtualPath, Request.WarrentyCertificateAttachmentContent, FileName, fileExtension, _host);



                                        if (MaintenanceManagementOrderDB.WarrentyCertificateAttachment != null && MaintenanceManagementOrderDB.WarrentyCertificateAttachment != "")
                                        {
                                            string exsistFilePath = Path.Combine(_host.WebRootPath, MaintenanceManagementOrderDB.WarrentyCertificateAttachment);
                                            if (System.IO.File.Exists(exsistFilePath))
                                            {
                                                System.IO.File.Delete(exsistFilePath);
                                            }
                                        }

                                        MaintenanceManagementOrderDB.WarrentyCertificateAttachment = FilePathWarrenty;
                                    }



                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Maintenance Management Order !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Maintenance Management Order Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert
                            var ContractDateCheck = await _Context.ManagementOfMaintenanceOrders.Where(x =>
                                                                              x.MaintenanceForId == Request.MaintenanceForID
                                                                              &&
                                                                               (
                                                                                   (x.StartDate < StartDate && x.EndDate > EndDate)
                                                                                    || x.ContractStatus == "Open"
                                                                               )
                                                                                                         ).FirstOrDefaultAsync();


                            //if (ContractDateCheck != null)
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err-P12";
                            //    error.ErrorMSG = "There are another Open Contract in the same Time ";
                            //    Response.Errors.Add(error);
                            //    return Response;
                            //}
                            var CurrencyIDCheck = await _Context.Currencies.Where(x => x.Id == Request.CurrencyID).FirstOrDefaultAsync();
                            if (CurrencyIDCheck == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "This Currency doesn't Exist.";
                                Response.Errors.Add(error);
                                return Response;
                            }


                            //var MainetanceOfferIDCheck = await _Context.SalesOffers.Where(x => x.ID == Request.MaintenanceOfferID).FirstOrDefaultAsync();


                            MaintenanceManagementOrderDB.Active = true;
                            MaintenanceManagementOrderDB.ContractPrice = Request.ContractPrice;
                            MaintenanceManagementOrderDB.ContractStatus = "Open";
                            MaintenanceManagementOrderDB.CurrencyId = Request.CurrencyID;
                            MaintenanceManagementOrderDB.StartDate = StartDate;
                            MaintenanceManagementOrderDB.EndDate = EndDate;
                            MaintenanceManagementOrderDB.MaintenanceOfferId = maintenanceforCheck.SalesOfferId;
                            MaintenanceManagementOrderDB.MaintenanceForId = Request.MaintenanceForID;
                            MaintenanceManagementOrderDB.NumberOfVisits = Request.NumberOfVisits;
                            MaintenanceManagementOrderDB.ProjectId = maintenanceforCheck.ProjectId;
                            MaintenanceManagementOrderDB.RateToLocalCu = Request.RateToLocalCu;
                            MaintenanceManagementOrderDB.CreatedBy = UserID;
                            MaintenanceManagementOrderDB.CreationDate = DateTime.Now;
                            MaintenanceManagementOrderDB.ModifiedBy = UserID;
                            MaintenanceManagementOrderDB.ModificationDate = DateTime.Now;
                            MaintenanceManagementOrderDB.ContractType = Request.ContractType;
                            MaintenanceManagementOrderDB.ClosingContractType = Request.ClosingContractType;
                            MaintenanceManagementOrderDB.ClosingMileageCounter = Request.ClosingMileageCounter ?? 0;
                            MaintenanceManagementOrderDB.CurrentMileageCounter = Request.CurrentMileageCounter ?? 0;
                            //var CompanyName = headers["CompanyName"].ToString().ToLower();
                            string FilePathContract = null;
                            if (Request.ContractAttachementContent != null)
                            {
                                //FilePathContract = await Common.SaveFileAsync("/Attachments/" + CompanyName + "/Maintenance/", Request.ContractAttachementContent, Request.ContractAttachementName, Request.ContractAttachementExtension);

                                var fileExtension = Request.ContractAttachementContent.FileName.Split('.').Last();
                                var virtualPath = $"Attachments\\{CompanyName}\\Maintenance\\";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.ContractAttachementContent.FileName.Trim().Replace(" ", ""));
                                FilePathContract = Common.SaveFileIFF(virtualPath, Request.ContractAttachementContent, FileName, fileExtension, _host);

                            }

                            MaintenanceManagementOrderDB.ContractAttachment = FilePathContract;

                            string FilePath = null;
                            if (Request.ContractAttachementContent != null)
                            {

                                //FilePath = await Common.SaveFileAsync("/Attachments/" + CompanyName + "/Maintenance/", Request.WarrentyCertificateAttachmentContent, Request.WarrentyCertificateAttachmentName, Request.WarrentyCertificateAttachmentExtension);

                                var fileExtension = Request.WarrentyCertificateAttachmentContent.FileName.Split('.').Last();
                                var virtualPath = $"Attachments\\{CompanyName}\\Maintenance\\";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.WarrentyCertificateAttachmentContent.FileName.Trim().Replace(" ", ""));
                                FilePath = Common.SaveFileIFF(virtualPath, Request.WarrentyCertificateAttachmentContent, FileName, fileExtension, _host);

                            }

                            MaintenanceManagementOrderDB.WarrentyCertificateAttachment = FilePath;


                            _Context.ManagementOfMaintenanceOrders.Add(MaintenanceManagementOrderDB);
                        }
                        var Res = await _Context.SaveChangesAsync();
                        if (Res > 0)
                        {

                            Response.ID = MaintenanceManagementOrderDB.Id;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Management Of Maintenance Order!!";
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public async Task<BaseMessageResponse> MaintenanceDetailsListCallExcel(MaintenanceDetailsListCallFilters filters, string CompanyName)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {


                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[15] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Product Name"),
                                                     new DataColumn("Product Category"),
                                                     new DataColumn("Product Brand"),
                                                     new DataColumn("Product Fabricator"),
                                                     new DataColumn("Product Serial"),
                                                     new DataColumn("Client Name"),
                                                     new DataColumn("Project Name"),
                                                     new DataColumn("Project Location"),
                                                     new DataColumn("Last Visit Date"),
                                                     new DataColumn("Contract Start Date"),
                                                     new DataColumn("ContractEnd Date"),
                                                     new DataColumn("Contract Price"),
                                                     new DataColumn("Contract Remain Visits"),
                                                     new DataColumn("Number Of Visits With Out Contract"),
                                                     new DataColumn("Cost Of Visits WithOut Contract"),

                    });


                    using (ExcelPackage packge = new ExcelPackage())
                    {
                        ExcelWorksheet ws = packge.Workbook.Worksheets.Add("Sheet1");
                        var MaintenanceDetailsListCallDB = await MaintenanceDetailsListCall(filters, CompanyName);

                        if (!MaintenanceDetailsListCallDB.Result)
                        {
                            Response.Result = MaintenanceDetailsListCallDB.Result;
                            Response.Errors = MaintenanceDetailsListCallDB.Errors;
                            return Response;
                        }
                        

                        foreach (var Item in MaintenanceDetailsListCallDB.MaintenanceForDetailsList)
                        {

                            dt.Rows.Add(

                            Item.Name,
                            Item.InventoryCategory,
                            Item.Brand,
                            Item.Fabricator,
                            Item.ProductSerial,
                            Item.ClientName,
                            Item.ProjectName,
                            Item.ProjectLocation,
                            Item.lastVisitDate,
                            Item.StartDate,
                            Item.EndDate,
                            Item.Cost,
                            Item.remainVisitsNo,
                            Item.NumberOfVisitsWithOutContract,
                            Item.CostVisitsWithOutContract
                           );

                            // Collapse  Counter Draw




                            //Create the worksheet



                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells[1, 1, 1, 15])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(4, 189, 189));

                                range.AutoFitColumns();
                            }


                            //using (ExcelRange range = ws.Cells[1, 6, 1 , 6])
                            //{
                            //    //range.Style.Font.Bold = true;
                            //    range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            //    range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

                            //    range.AutoFitColumns();
                            //}

                            using (ExcelRange range = ws.Cells[1, 1, 1, 15])
                            {

                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                            }

                            ws.Protection.IsProtected = false;
                            ws.Protection.AllowSelectLockedCells = false;



                        }

                        var newpath = $"Attachments\\{CompanyName}\\";
                        var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                        if (File.Exists(savedPath))
                        {
                            File.Delete(savedPath);
                        }
                        Directory.CreateDirectory(savedPath);
                        var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                        var excelPath = savedPath + $"\\MaintenanceDetailsExcel_{date}.xlsx";
                        packge.SaveAs(excelPath);
                        packge.Dispose();
                        Response.Message = Globals.baseURL + '\\' + newpath + $"\\MaintenanceDetailsExcel_{date}.xlsx";
                        return Response;

                        /*string FullFileName = DateTime.Now.ToFileTime() + "_" + "MaintenanceDetailsExcel.xlsx";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        //  String path = HttpContext.Current.Server.MapPath(PathsTR);
                        String path = Path.Combine(_host.WebRootPath,PathsTR);
                        if (!System.IO.Directory.Exists(path))
                        {
                            System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                        }
                        string p_strPath = Path.Combine(path, FullFileName);
                        //var workSheet = package.Workbook.Worksheets.Add("AccountTree");

                        ExcelRangeBase excelRangeBase = ws.Cells.LoadFromDataTable(dt, true);

                        File.Exists(p_strPath);
                        FileStream objFileStrm = File.Create(p_strPath);

                        objFileStrm.Close();
                        packge.Save();
                        File.WriteAllBytes(p_strPath, packge.GetAsByteArray());
                        packge.Dispose();

                        Response.Message = Globals.baseURL + PathsTR + FullFileName;*/


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

        public async Task<BaseResponseWithData<string>> MaintenanceContractDetailsListExcel(MaintenanceContractDetailsListFilters filters, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var list = _unitOfWork.ManagementOfMaintenanceOrders.FindAllQueryable(a => true && a.Active == true, includes: new[] { "MaintenanceFor.Client.ClientContactPeople", "VisitsScheduleOfMaintenances.MaintenanceReports", "MaintenanceFor.Client.ClientMobiles", "MaintenanceFor.Client.ClientAddresses.Area" }).AsQueryable();

                    if (!string.IsNullOrWhiteSpace(filters.ContractType))
                    {
                        list = list.Where(a => a.ContractType.ToLower() == filters.ContractType.ToLower()).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.ContractStatus)) 
                    {
                        if (filters.ContractStatus == "Valid")
                        {

                            list = list.Where(z => z.StartDate >= DateTime.Today && z.EndDate >= DateTime.Today).AsQueryable();
                        }

                        if (filters.ContractStatus != null && filters.ContractStatus == "Expired")
                        {
                            list = list.Where(z => z.EndDate < DateTime.Today).OrderBy(x=>x.EndDate).AsQueryable();
                        }
                        if (filters.ContractStatus == "ExpireSoon" && filters.WeekNum != 0)
                        {
                            TimeSpan WeekNumberConv = TimeSpan.Parse((7 * filters.WeekNum).ToString());

                            list = list.Where(z => z.EndDate != null && z.EndDate > DateTime.Now && ((DateTime)z.EndDate).AddDays(-7 * filters.WeekNum) <= DateTime.Today).OrderBy(x => x.EndDate).AsQueryable();

                        }
                    }
                    if (filters.Late)
                    {
                        list = list.Where(a => a.VisitsScheduleOfMaintenances.Any(x => x.PlannedDate < DateTime.Now)).AsQueryable();
                    }

                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[16] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Client Name"),
                                                     new DataColumn("Client Area"),
                                                     new DataColumn("Contact Person Mobile"),
                                                     new DataColumn("Client Mobile"),
                                                     new DataColumn("Client Status"),
                                                     new DataColumn("Product Serial"),
                                                     new DataColumn("Product Brand"),
                                                     new DataColumn("Product Creation Date"),
                                                     new DataColumn("Contract Visit Number"),
                                                     new DataColumn("Contract Start Date"),
                                                     new DataColumn("Contract End Date"),
                                                     new DataColumn("Contract Type"),
                                                     new DataColumn("Contract Price"),
                                                     //new DataColumn("Required visits"),
                                                     new DataColumn("Done visits"),
                                                     new DataColumn("Contract Remain Visits"),
                                                     new DataColumn("Last Visit Date"),

                    });


                    using (ExcelPackage packge = new ExcelPackage())
                    {
                        ExcelWorksheet ws = packge.Workbook.Worksheets.Add("Sheet1");

                        

                        var FinalList = list.ToList();
                        FinalList = FinalList.GroupBy(a => a.MaintenanceForId).SelectMany(a => a).ToList();
                       // FinalList = FinalList.OrderBy(a=>a.MaintenanceFor.Client.Name).ToList();

                        foreach (var Item in FinalList)
                        {

                            dt.Rows.Add(
                            Item.MaintenanceFor.Client.Name,
                            Item.MaintenanceFor?.Client?.ClientAddresses?.FirstOrDefault()?.Area?.Name,
                            Item.MaintenanceFor.Client.ClientContactPeople?.FirstOrDefault()?.Mobile,
                            Item.MaintenanceFor.Client.ClientMobiles?.FirstOrDefault()?.Mobile,
                            Item.MaintenanceFor.Client.NeedApproval==2?"Blocked": Item.MaintenanceFor.Client.NeedApproval == 1?"Need Approval":"Approved",
                            Item.MaintenanceFor.ProductSerial,
                            Item.MaintenanceFor.ProductBrand,
                            Item.MaintenanceFor.CreationDate?.ToString("dd/MM/yyyy h:m:s tt"),
                            Item.NumberOfVisits,
                            Item.StartDate?.ToShortDateString(),
                            Item.EndDate?.ToShortDateString(),
                            Item.ContractType,
                            Item.ContractPrice,
                            //Item.VisitsScheduleOfMaintenances.Where(a => !a.Status && a.PlannedDate < DateTime.Now).Count(),
                            Item.VisitsScheduleOfMaintenances.Where(a => a.Status).Count(),
                            Item.VisitsScheduleOfMaintenances.Where(a => !a.Status).Count(),
                            Item.VisitsScheduleOfMaintenances.LastOrDefault()?.MaintenanceReports?.LastOrDefault()?.ReportDate.ToString("dd/MM/yyyy h:m:s tt")
                           );
                            
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            using (ExcelRange range = ws.Cells[1, 1, 1, 16])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(4, 189, 189));

                                range.AutoFitColumns();
                            }
                            using (ExcelRange range = ws.Cells[1, 1, 1, 16])
                            {

                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                            }

                            ws.Protection.IsProtected = false;
                            ws.Protection.AllowSelectLockedCells = false;
                            ExcelRangeBase excelRangeBase = ws.Cells.LoadFromDataTable(dt, true);
                            for (int i = 1; i <= excelRangeBase.Rows; i++)
                            {
                                ws.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }
                        }

                        var newpath = $"Attachments\\{CompanyName}\\";
                        var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                        if (File.Exists(savedPath))
                        {
                            File.Delete(savedPath);
                        }
                        Directory.CreateDirectory(savedPath);
                        var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                        var excelPath = savedPath + $"\\MaintenanceContractDetailsListExcel_{date}.xlsx";
                        packge.SaveAs(excelPath);
                        packge.Dispose();
                        Response.Data = Globals.baseURL + '\\' + newpath + $"\\MaintenanceContractDetailsListExcel_{date}.xlsx";
                        return Response;
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

        public MaintenanceProductResponse GetMaintenanceList([FromHeader] string Serial)
        {
            MaintenanceProductResponse response = new MaintenanceProductResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                var DDListDB = new List<MaintenanceProduct>();
                var maintenanceForsListDB = _unitOfWork.MaintenanceFors.FindAll(a => true, includes: new[] { "Category", "Client" });
                if (!string.IsNullOrWhiteSpace(Serial))
                {
                    maintenanceForsListDB = maintenanceForsListDB.Where(x => x.ProductSerial == Serial.Trim()).ToList();
                }
                DDListDB = maintenanceForsListDB.Select(c => new MaintenanceProduct
                {
                    ID = c.Id,
                    Name = c.ProductName,
                    BrandName = c.ProductBrand,
                    ClientId = c.ClientId,
                    ClientName = c.Client?.Name,
                    CategoryName = c.Category?.Name
                }).ToList();
                response.DDLList = DDListDB;
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

        public async Task<GetVisitsScheduleOfMaintenanceResponse> GetVisitsScheduleOfMaintenanceByID([FromHeader] int ManagementMaintenanceOrderID)
        {
            GetVisitsScheduleOfMaintenanceResponse Response = new GetVisitsScheduleOfMaintenanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ManagementMaintenanceOrderIDCheck = _unitOfWork.ManagementOfMaintenanceOrders.FindAllAsync(x => x.Id == ManagementMaintenanceOrderID).Result.FirstOrDefault();

                if (ManagementMaintenanceOrderIDCheck == null)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Management Maintenance Order ID Doesn't Exist";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }

                if (Response.Result)
                {

                    var VisitsScheduleOfMaintenancesList = new List<GetVisitsScheduleOfMaintenanceData>();

                    var VisitsScheduleOfMaintenancesObjDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.ManagementOfMaintenanceOrderId == ManagementMaintenanceOrderID, includes: new[] { "AssignedTo", "CreatedByNavigation" }).Result.ToList();
                    if (VisitsScheduleOfMaintenancesObjDB != null)
                    {
                        foreach (var item in VisitsScheduleOfMaintenancesObjDB)
                        {
                            var VisitsScheduleOfMaintenancesObj = new GetVisitsScheduleOfMaintenanceData();

                            VisitsScheduleOfMaintenancesObj.ID = item.Id;
                            VisitsScheduleOfMaintenancesObj.AssignedToID = item.AssignedToId;
                            VisitsScheduleOfMaintenancesObj.AssignedTo = item.AssignedTo != null ? item.AssignedTo?.FirstName + " " + item.AssignedTo?.LastName : "";
                            VisitsScheduleOfMaintenancesObj.MaintenanceForID = item.MaintenanceForId;
                            VisitsScheduleOfMaintenancesObj.ManagementOfMaintenanceOrderID = item.ManagementOfMaintenanceOrderId;
                            VisitsScheduleOfMaintenancesObj.PlannedDate = item.PlannedDate?.ToShortDateString(); ;
                            VisitsScheduleOfMaintenancesObj.Serial = item.Serial;
                            VisitsScheduleOfMaintenancesObj.Status = item.Status;
                            VisitsScheduleOfMaintenancesObj.VisitDate = item.VisitDate?.ToShortDateString(); ;
                            VisitsScheduleOfMaintenancesObj.CreatedBy = item.CreatedByNavigation.FirstName + " " + item.CreatedByNavigation.LastName;
                            VisitsScheduleOfMaintenancesObj.CreatedByIDEnc = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(item.CreatedBy.ToString(), key));
                            VisitsScheduleOfMaintenancesObj.CreationDate = item.CreationDate.ToShortDateString(); ;
                            VisitsScheduleOfMaintenancesList.Add(VisitsScheduleOfMaintenancesObj);
                        }

                        Response.VisitsScheduleOfMaintenanceDataList = VisitsScheduleOfMaintenancesList;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public async Task<GetVisitsScheduleOfMaintenanceResponse> GetPreviousVisitsList([FromHeader] int MaintenanceForID)
        {
            GetVisitsScheduleOfMaintenanceResponse Response = new GetVisitsScheduleOfMaintenanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var MaintenanceForsIDCheck = _unitOfWork.MaintenanceFors.FindAllAsync(x => x.Id == MaintenanceForID).Result.FirstOrDefault();

                if (MaintenanceForsIDCheck == null)
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Maintenance For ID Doesn't Exist";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }



                var VisitsScheduleOfMaintenancesList = new List<GetVisitsScheduleOfMaintenanceData>();

                var VisitsScheduleOfMaintenancesObjDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.MaintenanceForId == MaintenanceForID, includes: new[] { "CreatedByNavigation" }).Result.ToList();
                if (VisitsScheduleOfMaintenancesObjDB.Count > 0)
                {
                    foreach (var item in VisitsScheduleOfMaintenancesObjDB)
                    {
                        var VisitsScheduleOfMaintenancesObj = new GetVisitsScheduleOfMaintenanceData();

                        VisitsScheduleOfMaintenancesObj.ID = item.Id;
                        VisitsScheduleOfMaintenancesObj.AssignedToID = item.AssignedToId;
                        VisitsScheduleOfMaintenancesObj.MaintenanceForID = item.MaintenanceForId;
                        VisitsScheduleOfMaintenancesObj.ManagementOfMaintenanceOrderID = item.ManagementOfMaintenanceOrderId;
                        VisitsScheduleOfMaintenancesObj.PlannedDate = item.PlannedDate?.ToShortDateString(); ;
                        VisitsScheduleOfMaintenancesObj.Serial = item.Serial;
                        VisitsScheduleOfMaintenancesObj.Status = item.Status;
                        VisitsScheduleOfMaintenancesObj.VisitDate = item.VisitDate?.ToShortDateString(); ;
                        VisitsScheduleOfMaintenancesObj.CreatedBy = item.CreatedByNavigation.FirstName + " " + item.CreatedByNavigation.LastName;
                        VisitsScheduleOfMaintenancesObj.CreationDate = item.CreationDate.ToShortDateString(); ;


                        var ClientSatisfaction = _unitOfWork.MaintenanceReports.FindAllAsync(a => a.MaintVisitId == item.Id).Result.GroupBy(x => x.MaintVisitId).Select(x => x.Average(a => a.ClientSatisfactionRate)).Sum();

                        VisitsScheduleOfMaintenancesObj.ClientSatisfactionAverage = ClientSatisfaction;

                        VisitsScheduleOfMaintenancesList.Add(VisitsScheduleOfMaintenancesObj);

                    }

                    Response.VisitsScheduleOfMaintenanceDataList = VisitsScheduleOfMaintenancesList;
                }


                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public async Task<GetVisitsMaintenanceReportDetailsResponse> GetVisitsReportDetailsList([FromHeader] long MaintVisitID)
        {
            GetVisitsMaintenanceReportDetailsResponse response = new GetVisitsMaintenanceReportDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                var MaintenanceReportResponseList = new List<GetVisitsMaintenanceReportDetailsData>();


                if (response.Result)
                {

                    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    var GetMaintenanceReportDB = await _unitOfWork.MaintenanceReports.FindAllAsync(x => x.MaintVisitId == MaintVisitID, includes: new[] { "MaintVisit.ManagementOfMaintenanceOrder.Currency" });

                    var GetMaintenanceReportUsersDB = await _unitOfWork.MaintenanceReportUsers.FindAllAsync(a => true, includes: new[] { "Department", "User" });

                    if (GetMaintenanceReportDB != null)
                    {

                        foreach (var item in GetMaintenanceReportDB)
                        {

                            var VisitsMaintenanceReportDetailsResponse = new GetVisitsMaintenanceReportDetailsData();

                            VisitsMaintenanceReportDetailsResponse.ID = item.Id;

                            VisitsMaintenanceReportDetailsResponse.ByUser = item.ByUser;

                            VisitsMaintenanceReportDetailsResponse.ByUserID = item.ByUserId;

                            VisitsMaintenanceReportDetailsResponse.ClientAddress = item.ClientAddress;

                            VisitsMaintenanceReportDetailsResponse.ClientCommentsAndFeedback = item.ClientCommentsAndFeedback;
                            VisitsMaintenanceReportDetailsResponse.ClientPRStatus = item.ClientPrstatus;
                            VisitsMaintenanceReportDetailsResponse.ClientSatisfactionRate = item.ClientSatisfactionRate;
                            if (!string.IsNullOrWhiteSpace(item.ClientSignature))
                            {
                                VisitsMaintenanceReportDetailsResponse.ClientSignature = Globals.baseURL + item.ClientSignature;
                            }
                            if (!string.IsNullOrWhiteSpace(item.ProblemReportImage))
                            {
                                VisitsMaintenanceReportDetailsResponse.ProblemReportImage = Globals.baseURL + item.ProblemReportImage;
                            }
                            VisitsMaintenanceReportDetailsResponse.CollectedAmount = item.CollectedAmount;
                            VisitsMaintenanceReportDetailsResponse.CurrencyName = item.MaintVisit?.ManagementOfMaintenanceOrder?.Currency?.Name;
                            VisitsMaintenanceReportDetailsResponse.CRMCommitment = item.Crmcommitment;
                            VisitsMaintenanceReportDetailsResponse.CRMFeedback = item.Crmfeedback;
                            VisitsMaintenanceReportDetailsResponse.CRMFeedbackComments = item.CrmfeedbackComments;
                            VisitsMaintenanceReportDetailsResponse.CRMFeedbackStatus = item.CrmfeedbackStatus;
                            VisitsMaintenanceReportDetailsResponse.DefectDescription = item.DefectDescription;
                            VisitsMaintenanceReportDetailsResponse.DesignPRStatus = item.DesignPrstatus;
                            VisitsMaintenanceReportDetailsResponse.FabricationPRStatus = item.FabricationPrstatus;
                            VisitsMaintenanceReportDetailsResponse.Finished = item.Finished;
                            VisitsMaintenanceReportDetailsResponse.InstallationPRStatus = item.InstallationPrstatus;
                            VisitsMaintenanceReportDetailsResponse.InternalPartComments = item.InternalPartComments;
                            VisitsMaintenanceReportDetailsResponse.MaintenanceTeamPRStatus = item.MaintenanceTeamPrstatus;
                            VisitsMaintenanceReportDetailsResponse.MaintVisitID = item.MaintVisitId;
                            VisitsMaintenanceReportDetailsResponse.ProductLifePRStatus = item.ProductLifePrstatus;
                            VisitsMaintenanceReportDetailsResponse.ReportDate = item.ReportDate.ToShortDateString(); ;
                            VisitsMaintenanceReportDetailsResponse.WorkDescription = item.WorkDescription;

                            VisitsMaintenanceReportDetailsResponse.MileageCounter = item.MaintVisit?.MileageCounter;


                            var MaintenanceReportUsersDBFiltered = GetMaintenanceReportUsersDB.Where(x => x.MaintenanceReportId == item.Id).ToList();

                            var MaintenanceReportUsersResponseList = new List<GetUsersWorkHoursAndEvaluation>();
                            foreach (var MaintenanceReportUsersDBFilteredItem in MaintenanceReportUsersDBFiltered)
                            {

                                var MaintenanceReportUsersResponseObject = new GetUsersWorkHoursAndEvaluation();

                                MaintenanceReportUsersResponseObject.ID = MaintenanceReportUsersDBFilteredItem.Id;
                                MaintenanceReportUsersResponseObject.BranchID = MaintenanceReportUsersDBFilteredItem.BranchId;
                                MaintenanceReportUsersResponseObject.Comment = MaintenanceReportUsersDBFilteredItem.Comment;
                                MaintenanceReportUsersResponseObject.DepartmentID = MaintenanceReportUsersDBFilteredItem.DepartmentId;
                                MaintenanceReportUsersResponseObject.DepartmentName = MaintenanceReportUsersDBFilteredItem.Department?.Name;
                                MaintenanceReportUsersResponseObject.Evalution = MaintenanceReportUsersDBFilteredItem.Evaluation;
                                MaintenanceReportUsersResponseObject.HourNum = MaintenanceReportUsersDBFilteredItem.HourNum;
                                MaintenanceReportUsersResponseObject.TimeFrom = MaintenanceReportUsersDBFilteredItem.TimeFrom?.ToShortTimeString();
                                MaintenanceReportUsersResponseObject.TimeTo = MaintenanceReportUsersDBFilteredItem.TimeTo?.ToShortTimeString();
                                MaintenanceReportUsersResponseObject.JobTitleID = MaintenanceReportUsersDBFilteredItem.JobTitleId;
                                MaintenanceReportUsersResponseObject.MaintenanceReportID = MaintenanceReportUsersDBFilteredItem.MaintenanceReportId;
                                MaintenanceReportUsersResponseObject.UserID = MaintenanceReportUsersDBFilteredItem.UserId;
                                if (MaintenanceReportUsersDBFilteredItem.User.PhotoUrl != null)
                                {
                                    MaintenanceReportUsersResponseObject.UserPhoto = Globals.baseURL + MaintenanceReportUsersDBFilteredItem.User.PhotoUrl;
                                }
                                MaintenanceReportUsersResponseObject.UserName = MaintenanceReportUsersDBFilteredItem.User.FirstName + " " + MaintenanceReportUsersDBFilteredItem.User.LastName;

                                MaintenanceReportUsersResponseList.Add(MaintenanceReportUsersResponseObject);

                            }
                            VisitsMaintenanceReportDetailsResponse.GetUsersWorkHoursAndEvaluationList = MaintenanceReportUsersResponseList;

                            MaintenanceReportResponseList.Add(VisitsMaintenanceReportDetailsResponse);


                        }
                        response.GetVisitsMaintenanceReportDetailsDataList = MaintenanceReportResponseList;

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

        public async Task<GetMaintenanceReportExpensesDetailsResponse> GetMaintenanceReportExpensesDetailsList([FromHeader] long MaintenanceReportId)
        {
            GetMaintenanceReportExpensesDetailsResponse response = new GetMaintenanceReportExpensesDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var MaintenanceReportExpensesDetailsList = new List<MaintenanceReportExpensesDetails>();


                if (response.Result)
                {
                    var MaintenanceReportExpensesDB = _unitOfWork.MaintenanceReportExpenses.FindAllAsync(x => x.MaintenanceReportId == MaintenanceReportId).Result.Select(item => new MaintenanceReportExpensesDetails
                    {
                        CurrencyId = item.CurrencyId,
                        ExpensesTypeId = item.ExpensisTypeId,
                        ExpensesTypeName = item.ExpensisType.ExpensisTypeName,
                        Amount = item.Amount,
                        MaintenanceReportExpensesId = item.Id,
                        Approve = item.Approve,
                        CurrencyName = item.Currency.Name,
                        FilePath = Globals.baseURL + item.FilePath
                    }).ToList();

                    response.Result = true;
                    response.MaintenanceReportExpensesDetails = MaintenanceReportExpensesDB;


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

        public async Task<GetMaintenanceVisitsWithoutContractResponse> GetMaintenanceVisitWithoutContract([FromHeader] long MaintenanceVisitId)
        {
            GetMaintenanceVisitsWithoutContractResponse Response = new GetMaintenanceVisitsWithoutContractResponse
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                if (MaintenanceVisitId != 0)
                {
                    var VisitMaintenanceDb = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.Active && a.MaintenanceFor.Client.NeedApproval != 2 && a.Id == MaintenanceVisitId && a.ManagementOfMaintenanceOrderId == null, includes: new[] { "AssignedTo", "VisitsScheduleOfMaintenanceAttachments", "MaintenanceFor.Client" }).Result.FirstOrDefault();
                    if (VisitMaintenanceDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "This Maintenance Visit Not Exist!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        GetVisitsScheduleOfMaintenance MaintenanceVisitResponse = new GetVisitsScheduleOfMaintenance
                        {
                            ID = VisitMaintenanceDb.Id,
                            AssignedToID = VisitMaintenanceDb.AssignedToId,
                            AssignedToName = VisitMaintenanceDb.AssignedTo?.FirstName,
                            DepartmentName = VisitMaintenanceDb.AssignedTo?.Department?.Name,
                            ClientProblem = VisitMaintenanceDb.ClientProblem,
                            ConfirmedDate = VisitMaintenanceDb.ConfirmedDate?.ToString().Split(' ')[0],
                            MaintenanceForID = VisitMaintenanceDb.MaintenanceForId,
                            MaintenanceVisitType = VisitMaintenanceDb.MaintenanceVisitType,
                            ManagementOfMaintenanceOrderID = VisitMaintenanceDb.ManagementOfMaintenanceOrderId,
                            PlannedDate = VisitMaintenanceDb.PlannedDate?.ToString().Split(' ')[0],
                            Serial = VisitMaintenanceDb.Serial,
                            Status = VisitMaintenanceDb.Status,
                            VisitDate = VisitMaintenanceDb.VisitDate?.ToString().Split(' ')[0],
                            MaintenanceProblemAttachments = VisitMaintenanceDb.VisitsScheduleOfMaintenanceAttachments != null && VisitMaintenanceDb.VisitsScheduleOfMaintenanceAttachments.Count > 0 ? VisitMaintenanceDb.VisitsScheduleOfMaintenanceAttachments.Select(attach => new Attachment
                            {
                                Id = attach.Id,
                                FileExtension = attach.FileExtenssion,
                                FileName = attach.FileName,
                                FilePath = Globals.baseURL + attach.AttachmentPath.TrimStart('~')
                            }).ToList() : null
                        };
                        Response.VisitsScheduleOfMaintenance = MaintenanceVisitResponse;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<VisitsScheduleOfMaintenanceWithoutContractResponse> VisitsScheduleOfMaintenanceWithoutContract([FromHeader] long ClientID)
        {
            VisitsScheduleOfMaintenanceWithoutContractResponse Response = new VisitsScheduleOfMaintenanceWithoutContractResponse
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (Response.Result)
                {
                    var VisitsMaintenanceWithoutContractDb = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.ManagementOfMaintenanceOrderId == null, includes: new[] { "MaintenanceFor.SalesOffer", "MaintenanceFor.Client.ClientContactPeople" }).Result.ToList();
                    if (ClientID != 0)
                    {
                        VisitsMaintenanceWithoutContractDb = VisitsMaintenanceWithoutContractDb.Where(x => x.MaintenanceFor.SalesOffer.ClientId == ClientID).ToList();
                    }
                    if (VisitsMaintenanceWithoutContractDb != null && VisitsMaintenanceWithoutContractDb.Count > 0)
                    {
                        List<ClientVisitsScheduleOfMaintenanceInfo> MaintenanceVisitsResponse = VisitsMaintenanceWithoutContractDb.Select(MaintenanceVisit => new ClientVisitsScheduleOfMaintenanceInfo
                        {
                            MaintenanceVisitId = MaintenanceVisit.Id,
                            ClientName = MaintenanceVisit.MaintenanceFor?.Client?.Name,
                            ContanctPersonMobile = MaintenanceVisit.MaintenanceFor?.Client?.ClientContactPeople.FirstOrDefault()?.Mobile,
                            ContanctPersonName = MaintenanceVisit.MaintenanceFor.Client.ClientContactPeople.FirstOrDefault()?.Name,
                            ProjectLocation = MaintenanceVisit.MaintenanceFor.SalesOffer?.ProjectLocation,
                            MaintenanceVisitType = MaintenanceVisit.MaintenanceVisitType,
                            MaintenanceForID = MaintenanceVisit.MaintenanceForId,
                            CurrentMileageCounter = MaintenanceVisit.MileageCounter

                        }).ToList();
                        Response.MaintenanceScheduleVisits = MaintenanceVisitsResponse;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetVisitsScheduleOfMaintenanceResponse> GetVisitsScheduleOfMaintenanceWithoutContractByMaintenanceForID([FromHeader] int MaintenanceForID)
        {
            GetVisitsScheduleOfMaintenanceResponse Response = new GetVisitsScheduleOfMaintenanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var MaintenanceForIDCheck = _unitOfWork.MaintenanceFors.FindAllAsync(x => x.Id == MaintenanceForID).Result.FirstOrDefault();

                    if (MaintenanceForIDCheck == null)
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Maintenance For ID Doesn't Exist";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;
                    }

                    if (Response.Result)
                    {
                        var VisitsScheduleOfMaintenancesList = new List<GetVisitsScheduleOfMaintenanceData>();
                        var VisitsScheduleOfMaintenancesObjDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(a => a.Active && a.MaintenanceFor.Client.NeedApproval != 2 && a.MaintenanceForId == MaintenanceForID && a.MaintenanceVisitType != null, includes: new[] { "AssignedTo", "MaintenanceFor.Client" }).Result.ToList();
                        if (VisitsScheduleOfMaintenancesObjDB != null)
                        {
                            foreach (var item in VisitsScheduleOfMaintenancesObjDB)
                            {
                                var VisitsScheduleOfMaintenancesObj = new GetVisitsScheduleOfMaintenanceData();

                                VisitsScheduleOfMaintenancesObj.ID = item.Id;
                                VisitsScheduleOfMaintenancesObj.AssignedToID = item.AssignedToId;
                                VisitsScheduleOfMaintenancesObj.AssignedTo = item.AssignedTo != null ? item.AssignedTo.FirstName + " " + item.AssignedTo?.LastName : "";
                                VisitsScheduleOfMaintenancesObj.MaintenanceForID = item.MaintenanceForId;
                                VisitsScheduleOfMaintenancesObj.ManagementOfMaintenanceOrderID = item.ManagementOfMaintenanceOrderId;
                                VisitsScheduleOfMaintenancesObj.PlannedDate = item.PlannedDate?.ToShortDateString();
                                VisitsScheduleOfMaintenancesObj.Serial = item.Serial;
                                VisitsScheduleOfMaintenancesObj.Status = item.Status;
                                VisitsScheduleOfMaintenancesObj.VisitDate = item.VisitDate?.ToShortDateString();

                                VisitsScheduleOfMaintenancesList.Add(VisitsScheduleOfMaintenancesObj);
                            }
                            Response.VisitsScheduleOfMaintenanceDataList = VisitsScheduleOfMaintenancesList;
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
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public VehicleMaintenanceTypeBOM GetVehicleMaintenanceTypeBOM(long BOMId)
        {
            VehicleMaintenanceTypeBOM BOM = new VehicleMaintenanceTypeBOM();
            var BOMObj = _unitOfWork.Boms.FindAll(a => a.Id == BOMId).FirstOrDefault();
            if (BOMObj != null)
            {
                BOM.BOMID = BOMObj.Id;
                BOM.BOMName = BOMObj.Name;

                var BOMPartitionItems = _unitOfWork.VBompartitionItemsInventoryItems.FindAll(a => a.Bomid == BOMObj.Id).ToList();
                if (BOMPartitionItems != null)
                {
                    if (BOMPartitionItems.Count > 0)
                    {
                        BOM.BOMPartitionItemsNames = BOMPartitionItems.Select(a => a.InventoryItemName).ToList();
                        decimal TotalCostType1, TotalCostType2, TotalCostType3;
                        TotalCostType1 = TotalCostType2 = TotalCostType3 = 0;

                        foreach (var partitionItem in BOMPartitionItems)
                        {
                            var totalItemPriceCostType1 = partitionItem.CostAmount1 * partitionItem.RequiredQty;
                            TotalCostType1 += (totalItemPriceCostType1 ?? 0);
                            var totalItemPriceCostType2 = partitionItem.CostAmount2 * partitionItem.RequiredQty;
                            TotalCostType2 += (totalItemPriceCostType2 ?? 0);
                            var totalItemPriceCostType3 = partitionItem.CostAmount3 * partitionItem.RequiredQty;
                            TotalCostType3 += (totalItemPriceCostType3 ?? 0);
                        }

                        BOM.TotalCostType1 = TotalCostType1;
                        BOM.TotalCostType2 = TotalCostType2;
                        BOM.TotalCostType3 = TotalCostType3;
                    }
                }
            }

            return BOM;
        }

        public GetAllMaintenanceTypesResponse GetAllMaintenanceTypes([FromHeader] int VehicleModelId, [FromHeader] int RateId, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10)
        {
            GetAllMaintenanceTypesResponse Response = new GetAllMaintenanceTypesResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                int VehicleModelBrandId = 0;
                if (VehicleModelId != 0)
                {
                    var ModelDb = _unitOfWork.VehicleModels.GetById(VehicleModelId);
                    if (ModelDb != null)
                    {
                        VehicleModelBrandId = ModelDb.VehicleBrandId;
                    }
                    else
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "This Model Doesn't Exist";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;
                    }
                }
                if (Response.Result)
                {
                    var VehicleMaintenanceTypesDBQuery = _unitOfWork.VehicleMaintenanceTypes.FindAllQueryable(a => true, includes: new[] { "VehiclePriorityLevel", "VehicleRate", "VehicleMaintenanceTypeForModels.Brand", "VehicleMaintenanceTypeForModels.Model" });

                    if (VehicleModelId != 0)
                    {
                        var VehicleMaintenanceTypesIds = _unitOfWork.VehicleMaintenanceTypeForModels.FindAll(a => a.BrandId == VehicleModelBrandId || a.ModelId == VehicleModelId || a.ForAllModles == true).Select(a => a.VehicleMaintenanceTypeId).Distinct().ToList();

                        VehicleMaintenanceTypesDBQuery = VehicleMaintenanceTypesDBQuery.Where(a => VehicleMaintenanceTypesIds.Contains(a.Id)).AsQueryable();
                    }
                    if (RateId != 0)
                    {
                        VehicleMaintenanceTypesDBQuery = VehicleMaintenanceTypesDBQuery.Where(a => a.VehicleRateId == RateId).AsQueryable();
                    }

                    VehicleMaintenanceTypesDBQuery = VehicleMaintenanceTypesDBQuery.Where(a => a.Active == true).OrderBy(a => a.Name).AsQueryable();
                    var VehicleMaintenanceTypeItemsPagedList = PagedList<VehicleMaintenanceType>.Create(VehicleMaintenanceTypesDBQuery, CurrentPage, NumberOfItemsPerPage);
                    if (VehicleMaintenanceTypeItemsPagedList != null && VehicleMaintenanceTypeItemsPagedList.Count > 0)
                    {
                        var VehicleMaintenanceTypeItemsList = VehicleMaintenanceTypeItemsPagedList.Select(a => new VehicleMaintenanceTypeItem
                        {
                            ID = a.Id,
                            Name = a.Name != null ? a.Name.ToString() : "",
                            Description = a.Description != null ? a.Description.ToString() : "",
                            VehicleMaintenanceTypeServiceSheduleCategories = a.VehicleMaintenanceTypeServiceSheduleCategories.Select(b => b.VehicleServiceScheduleCategory.ItemName).ToList(),
                            VehiclePriorityLevelId = a.VehiclePriorityLevelId,
                            VehiclePriorityLevelName = a.VehiclePriorityLevel.PriorityLevelName,
                            VehicleRateId = a.VehicleRateId,
                            Comment = a.Comment,
                            VehicleRateName = a.VehicleRate.RateName,
                            VehicleMaintenanceTypeBOM = a.Bomid != null ? GetVehicleMaintenanceTypeBOM((long)a.Bomid) : null,
                            isForAllModels = a.VehicleMaintenanceTypeForModels.FirstOrDefault(b => b.ForAllModles == true) != null,
                            VehicleMaintenanceTypeForBrandsStrings = a.VehicleMaintenanceTypeForModels.Where(b => b.BrandId != null).Select(b => b.Brand.Name).ToList(),
                            VehicleMaintenanceTypeForModelsStrings = a.VehicleMaintenanceTypeForModels.Where(b => b.ModelId != null).Select(b => b.Model.Name).ToList(),
                        }).ToList();

                        Response.MaintenanceTypeItemsList = VehicleMaintenanceTypeItemsList;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = CurrentPage,
                            TotalPages = VehicleMaintenanceTypeItemsPagedList.TotalPages,
                            ItemsPerPage = NumberOfItemsPerPage,
                            TotalItems = VehicleMaintenanceTypeItemsPagedList.TotalCount
                        };
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public GetMaintenanceTypeItemResponse GetMaintenanceTypeItemData([FromHeader] int MaintenanceTypeItemId)
        {
            GetMaintenanceTypeItemResponse Response = new GetMaintenanceTypeItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var VehicleMaintenanceTypeDB = _unitOfWork.VehicleMaintenanceTypes.FindAll(a => a.Active == true && a.Id == MaintenanceTypeItemId, includes: new[] { "VehiclePriorityLevel", "VehicleRate", "VehicleMaintenanceTypeForModels.Brand", "VehicleMaintenanceTypeForModels.Model" }).FirstOrDefault();
                    if (VehicleMaintenanceTypeDB != null)
                    {
                        VehicleMaintenanceTypeItem vehicleMaintenanceTypeItem = new VehicleMaintenanceTypeItem
                        {
                            ID = VehicleMaintenanceTypeDB.Id,
                            Name = VehicleMaintenanceTypeDB.Name != null ? VehicleMaintenanceTypeDB.Name.ToString() : "",
                            Description = VehicleMaintenanceTypeDB.Description != null ? VehicleMaintenanceTypeDB.Description.ToString() : "",
                            VehicleMaintenanceTypeServiceSheduleCategories = VehicleMaintenanceTypeDB.VehicleMaintenanceTypeServiceSheduleCategories.Select(a => a.VehicleServiceScheduleCategory.ItemName).ToList(),
                            VehicleMaintenanceTypeServiceSheduleCategoriesIds = VehicleMaintenanceTypeDB.VehicleMaintenanceTypeServiceSheduleCategories.Select(a => a.VehicleServiceScheduleCategoryId.ToString()).ToList(),
                            VehiclePriorityLevelId = VehicleMaintenanceTypeDB.VehiclePriorityLevelId,
                            VehiclePriorityLevelName = VehicleMaintenanceTypeDB.VehiclePriorityLevel.PriorityLevelName,
                            VehicleRateId = VehicleMaintenanceTypeDB.VehicleRateId,
                            Comment = VehicleMaintenanceTypeDB.Comment,
                            VehicleRateName = VehicleMaintenanceTypeDB.VehicleRate.RateName,
                            VehicleMaintenanceTypeBOM = VehicleMaintenanceTypeDB.Bomid != null ? GetVehicleMaintenanceTypeBOM((long)VehicleMaintenanceTypeDB.Bomid) : null
                        };

                        if (VehicleMaintenanceTypeDB.VehicleMaintenanceTypeForModels.Count > 0)
                        {
                            List<string> VehicleMaintenanceTypeForModelsList = new List<string>();
                            var isForAllModels = VehicleMaintenanceTypeDB.VehicleMaintenanceTypeForModels.Where(a => a.ForAllModles == true).FirstOrDefault();
                            if (isForAllModels != null)
                            {
                                vehicleMaintenanceTypeItem.isForAllModels = true;
                                VehicleMaintenanceTypeForModelsList.Add("All");
                            }
                            else
                            {
                                vehicleMaintenanceTypeItem.isForAllModels = false;

                                var ForBrands = VehicleMaintenanceTypeDB.VehicleMaintenanceTypeForModels.Where(a => a.BrandId != null && a.Active == true).ToList();

                                if (ForBrands.Count > 0)
                                {
                                    var BrandsIds = ForBrands.Select(a => "Brand-" + a.Brand.Id.ToString()).ToList();
                                    VehicleMaintenanceTypeForModelsList.AddRange(BrandsIds);
                                }

                                var ForModels = VehicleMaintenanceTypeDB.VehicleMaintenanceTypeForModels.Where(a => a.ModelId != null && a.Active == true).ToList();
                                if (ForModels.Count > 0)
                                {
                                    var ModelsIds = ForModels.Select(a => "Model-" + a.Model.Id.ToString()).ToList();
                                    VehicleMaintenanceTypeForModelsList.AddRange(ModelsIds);
                                }
                            }
                            vehicleMaintenanceTypeItem.VehicleMaintenanceTypeForModelsIds = VehicleMaintenanceTypeForModelsList;
                        }

                        Response.MaintenanceTypeItem = vehicleMaintenanceTypeItem;
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public async Task<SelectDDLResponse> GetMaintenanceBrandsList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var DDLList = new List<SelectDDL>();
                if (response.Result)
                {
                    var MaintenanceBrandsList = _unitOfWork.MaintenanceFors.FindAll(x => x.ProductBrand != null).Select(x => x.ProductBrand).Distinct().ToList();
                    foreach (var Brand in MaintenanceBrandsList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = 0;
                        DDLObj.Name = Brand;

                        DDLList.Add(DDLObj);
                    }
                }

                response.DDLList = DDLList;
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


        public async Task<WorkerStatisticsResponse> GetWorkerstatistics([FromHeader] long AssignToID, [FromHeader] DateTime? VisitDateFrom, [FromHeader] DateTime? VisitDateTo)
        {
            WorkerStatisticsResponse Response = new WorkerStatisticsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    List<WorkerStatistics> workerStatisticsList = new List<WorkerStatistics>();

                    var VisitsMaintenance = await _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(x => x.Status == true && x.VisitDate != null && x.ManagementOfMaintenanceOrderId != null && x.AssignedToId != null, includes: new[] { "AssignedTo" });

                    if (VisitDateFrom != null)
                    {
                        VisitsMaintenance = VisitsMaintenance.Where(x => x.VisitDate >= VisitDateFrom).ToList();
                    }
                    if (VisitDateTo != null)
                    {
                        VisitsMaintenance = VisitsMaintenance.Where(x => x.VisitDate <= VisitDateTo).ToList();
                    }
                    if (AssignToID != 0)
                    {
                        VisitsMaintenance = VisitsMaintenance.Where(x => x.AssignedToId == AssignToID).ToList();
                    }
                    // user2 =>  object from assign to
                    var VisistsMAintenanceByWorker = VisitsMaintenance.GroupBy(x => x.AssignedTo).Select(x => new WorkerStatistics
                    {
                        workerId = x.Key.Id,
                        workerName = x.Key.FirstName + " " + x.Key.LastName,
                        visitsNo = x.Count()
                    }).ToList();

                    Response.WorkerStatisticsList = VisistsMAintenanceByWorker;
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public async Task<ClientMaintenanceDetailsResponse> GetClientMaintenanceDetails([FromHeader] long ClientID)
        {
            ClientMaintenanceDetailsResponse response = new ClientMaintenanceDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (ClientID == 0)
                {

                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Client ID Is Mandatory";
                    response.Errors.Add(error);
                    response.Result = false;
                    return response;
                }

                if (response.Result)
                {
                    var MaintenanceDetails = (await _unitOfWork.ManagementOfMaintenanceOrders.FindAllAsync(a => !a.Active)).Select(a => a.Id).ToList();
                    var MaintenanceDetailsDB = (await _unitOfWork.VMaintenanceForDetails.FindAllAsync(x => x.ClientId == ClientID && !MaintenanceDetails.Contains(x.ManagementOfMaintenanceOrderId ?? 0))).ToList();
                    if (MaintenanceDetailsDB.Count() > 0)
                    {
                        // Get Last Visit MAintenance for this Client By Maintenance For Id
                        var LastVisitMaintenanceDB = (await _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(x => x.VisitDate != null && x.MaintenanceFor.ClientId == ClientID, includes: new[] { "MaintenanceFor" })).OrderByDescending(x => x.VisitDate).FirstOrDefault();
                        string LastVisitDate = null;
                        long LastVisitId = 0;
                        if (LastVisitMaintenanceDB != null)
                        {
                            LastVisitDate = LastVisitMaintenanceDB.VisitDate?.ToShortDateString();
                            LastVisitId = LastVisitMaintenanceDB.Id;
                        }
                        response.MaintenanceNo = MaintenanceDetailsDB.Select(x => x.Id).Distinct().Count();
                        response.ValidContractNo = MaintenanceDetailsDB
                            .Select(x => new { x.ManagementOfMaintenanceOrderId, x.ContractEndDate }).Distinct().Where(x => x.ContractEndDate >= DateTime.Today).Count();
                        response.ExpiredContractNo = MaintenanceDetailsDB
                            .Select(x => new { x.ManagementOfMaintenanceOrderId, x.ContractEndDate }).Distinct().Where(x => x.ContractEndDate < DateTime.Today).Count();
                        response.LastVisitDate = LastVisitDate;
                        //MaintenanceDetailsDB.Where(x => x.VisitDate != null).OrderBy(x => x.VisitDate)
                        //.Select(x => ((DateTime)x.VisitDate).ToShortDateString()).FirstOrDefault();
                        response.LastVisitId = LastVisitId;
                        response.LastMaintenanceForID = LastVisitMaintenanceDB?.MaintenanceForId;
                        //MaintenanceDetailsDB.Where(x => x.VisitDate != null).OrderBy(x => x.VisitDate)
                        //.Select(x => x.VisitOfMaintenanceID).FirstOrDefault() ?? 0;
                        response.ClientSatisfactionRate = (await _unitOfWork.VVisitsMaintenanceReportUsers.FindAllAsync(x => x.ClientId == ClientID &&
                                                                                                                   x.ClientSatisfactionRate != null))
                            .GroupBy(x => new { x.ClientId, x.Id }).Select(x => x.Average(y => y.ClientSatisfactionRate)).DefaultIfEmpty(0).Sum();
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


        public NearestClientVisitMaintenanceDetailsResponse GetNearestClientVisitMaintenanceDetails(NearestClientVisitFilters filters)
        {
            NearestClientVisitMaintenanceDetailsResponse response = new NearestClientVisitMaintenanceDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    bool locationFilter = false;
                    if (filters.Latitude != 0)
                    {
                        if (filters.Longitude != 0)
                        {
                            locationFilter = true;
                        }
                        else
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Longitude Is Mandatory";
                            response.Errors.Add(error);
                            response.Result = false;
                        }
                    }
                    else
                    {
                        if (filters.Longitude == 0)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Latitude Is Mandatory";
                            response.Errors.Add(error);
                            response.Result = false;
                        }
                    }

                    var NearestClientVisitMaintenanceDetailsList = new List<NearestClientVisitMaintenanceDetails>();
                    if (response.Result)
                    {
                        var VisitsScheduleOfMaintenancesQuerable = _unitOfWork.VisitsScheduleOfMaintenances.FindAllQueryable(a => true, includes: new[] { "MaintenanceFor.Client.ClientAddresses" });
                        if (filters.AssignTo != 0)
                        {
                            VisitsScheduleOfMaintenancesQuerable = VisitsScheduleOfMaintenancesQuerable.Where(x => x.AssignedToId == filters.AssignTo).AsQueryable();
                        }
                        var VisitMaintinancesListDB = VisitsScheduleOfMaintenancesQuerable.ToList();
                        List<long> NearestSalesOfferIds = new List<long>();
                        var VisitMaintinanceDetails = VisitMaintinancesListDB.Where(x => x.Id == filters.VisitMaintinanceID).FirstOrDefault();
                        if (locationFilter)
                        {
                            NearestSalesOfferIds = GetNearestSalesOffers(filters.Latitude, filters.Longitude, filters.Radius);
                        }
                        if (VisitMaintinancesListDB.Count() > 0)
                        {
                            long? CurrentClientAreaID = null;
                            if (VisitMaintinanceDetails != null)
                            {
                                CurrentClientAreaID = VisitMaintinanceDetails.MaintenanceFor?.Client?.ClientAddresses?.Select(x => x.AreaId).FirstOrDefault();
                                if (CurrentClientAreaID != null || (NearestSalesOfferIds != null && NearestSalesOfferIds.Count > 0))
                                {
                                    // Get All Clinet Nearest with the same Area from Visits for the same Client 
                                    var ListOfClientIDs = _unitOfWork.ClientAddresses.FindAllAsync(x => x.AreaId == CurrentClientAreaID).Result
                                                                                        .Select(x => x.ClientId).Distinct();
                                    if (ListOfClientIDs.Count() > 0 || (NearestSalesOfferIds != null && NearestSalesOfferIds.Count > 0))
                                    {
                                        // List Of Maintenance For in the same area 
                                        var ListMaintenanceForNearestList = _unitOfWork.MaintenanceFors.FindAll(x =>
                                        ListOfClientIDs.Contains(x.ClientId) || NearestSalesOfferIds.Contains(x.SalesOfferId), includes: new[] { "Client.ClientAddresses", "SalesOffer.SalesOfferLocations", "Client.ClientContactPeople" });
                                        List<long> ListMaintenanceForIDS = ListMaintenanceForNearestList.Select(m => m.Id).ToList();

                                        // List Of Maintenance For Have  Visits Nearest in the same area 
                                        var NearestClientVisits = VisitMaintinancesListDB.Where(x =>
                                                                    ListMaintenanceForIDS.Contains(x.MaintenanceForId ?? 0)).ToList();






                                        var MainForIDSWithVisitMainID = new List<Tuple<long, long>>();
                                        // Loop on Maintenance Nearest from my area and check last visit from 21 Days and through on 7 Day
                                        if (ListMaintenanceForNearestList.Count() > 0)
                                        {
                                            var ListCurrentContractsForMaintenance = _unitOfWork.ManagementOfMaintenanceOrders
                                                 .FindAll(x => ListMaintenanceForIDS.Contains(x.MaintenanceForId ?? 0)
                                                 && x.StartDate <= DateTime.Today
                                                 && x.EndDate >= DateTime.Today
                                                 ).ToList();

                                            if (ListCurrentContractsForMaintenance.Count() > 0)
                                            {

                                                foreach (var Maintenance in ListMaintenanceForNearestList)
                                                {
                                                    var LastMaintenanceValidContract = ListCurrentContractsForMaintenance
                                                                    .Where(x => x.MaintenanceForId == Maintenance.Id).FirstOrDefault();
                                                    //  check Maintenance Contract is valid
                                                    if (LastMaintenanceValidContract != null)
                                                    {
                                                        var LastVisitObj = NearestClientVisits.Where(x => x.VisitDate != null &&
                                                                                                          x.MaintenanceForId == Maintenance.Id)
                                                                                              .OrderByDescending(x => x.VisitDate).FirstOrDefault();


                                                        bool CanVisit = false;
                                                        bool CanCreateNewVisit = false;
                                                        if (LastVisitObj != null)
                                                        {
                                                            // Check last visit before 21 Days and planned date throug in 7 Day 
                                                            if (LastVisitObj.VisitDate <= DateTime.Today.AddDays(-21))
                                                            {
                                                                CanVisit = true;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            /*
                                                             check Maintenance Contract #No of visit
                                                            Not Have any Visit for this maintenance until now
                                                             */

                                                            var CountVisitMaintinanceForContract = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(x => x.ManagementOfMaintenanceOrderId == LastMaintenanceValidContract.Id && x.PlannedDate != null && x.MaintenanceForId == Maintenance.Id).Count();
                                                            if (LastMaintenanceValidContract.NumberOfVisits
                                                                >
                                                                CountVisitMaintinanceForContract)
                                                            {
                                                                CanCreateNewVisit = true;
                                                            }
                                                        }

                                                        if (CanVisit || CanCreateNewVisit)
                                                        {
                                                            DateTime Through7Day = DateTime.Today.AddDays(7);
                                                            var FirstPlannedDateVisit = NearestClientVisits.Where(x => x.VisitDate == null &&
                                                                                                          x.MaintenanceForId == Maintenance.Id &&
                                                                                                          x.PlannedDate != null &&
                                                                                                          x.PlannedDate >= DateTime.Today &&
                                                                                                          x.PlannedDate <= Through7Day &&
                                                                                                         x.PlannedDate != DateTime.Today &&
                                                                                               x.MaintenanceForId != VisitMaintinanceDetails.MaintenanceForId
                                                                                                          )
                                                                                              .OrderBy(x => x.PlannedDate).FirstOrDefault();

                                                            if (FirstPlannedDateVisit != null)
                                                            {
                                                                if (CanVisit)
                                                                {
                                                                    MainForIDSWithVisitMainID.Add(System.Tuple.Create(Maintenance.Id, FirstPlannedDateVisit.Id));
                                                                }
                                                                else if (CanCreateNewVisit)
                                                                {
                                                                    MainForIDSWithVisitMainID.Add(System.Tuple.Create(Maintenance.Id, (long)0));
                                                                }
                                                            }
                                                        }

                                                    }
                                                }
                                            }
                                        }
                                        if (MainForIDSWithVisitMainID.Count() > 0)
                                        {
                                            foreach (var item in MainForIDSWithVisitMainID)
                                            {
                                                var MaintenanceObjDB = ListMaintenanceForNearestList.Where(x => x.Id == item.Item1).FirstOrDefault();
                                                if (MaintenanceObjDB != null)
                                                {
                                                    var MaintinanceObj = new NearestClientVisitMaintenanceDetails();

                                                    MaintinanceObj.ClientID = MaintenanceObjDB.ClientId;
                                                    MaintinanceObj.ClientName = MaintenanceObjDB.Client.Name;
                                                    MaintinanceObj.ClientAddress =
                                                            MaintenanceObjDB.Client?.ClientAddresses?.FirstOrDefault()?.Address;
                                                    MaintinanceObj.ProductName = MaintenanceObjDB.ProductName;
                                                    MaintinanceObj.ProductSerial = MaintenanceObjDB.ProductSerial;
                                                    MaintinanceObj.ProjectName = MaintenanceObjDB.SalesOffer?.ProjectName;
                                                    MaintinanceObj.MaintenanceForID = item.Item1;
                                                    MaintinanceObj.VisitMaintenanceID = item.Item2;

                                                    var SalesOfferLocations = MaintenanceObjDB.SalesOffer?.SalesOfferLocations?.FirstOrDefault();
                                                    if (SalesOfferLocations != null)
                                                    {
                                                        MaintinanceObj.Latitude = SalesOfferLocations.LocationX;
                                                        MaintinanceObj.Longitude = SalesOfferLocations.LocationY;
                                                        MaintinanceObj.Location = SalesOfferLocations.Description;
                                                    }

                                                    var ContactPerson = MaintenanceObjDB.Client?.ClientContactPeople?.FirstOrDefault();
                                                    if (ContactPerson != null)
                                                    {
                                                        MaintinanceObj.ContactPersonName = ContactPerson.Name;
                                                        MaintinanceObj.ClientLocation = ContactPerson.Location;
                                                        MaintinanceObj.ClientMobile = ContactPerson.Mobile;
                                                    }

                                                    var MainContract = MaintenanceObjDB.ManagementOfMaintenanceOrders?
                                                        .Where(x => x.StartDate <= DateTime.Today && DateTime.Today <= x.EndDate).FirstOrDefault();
                                                    if (MainContract != null)
                                                    {
                                                        MaintinanceObj.ContractPrice = MainContract.ContractPrice;
                                                        MaintinanceObj.CurrencyName = MainContract.Currency.Name;
                                                    }

                                                    NearestClientVisitMaintenanceDetailsList.Add(MaintinanceObj);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    response.NearestClientVisitMaintenanceDetailsList = NearestClientVisitMaintenanceDetailsList;
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


        public async Task<SelectDDLResponse> GetMaintenanceCategpryList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var DDListDB = new List<SelectDDL>();

                if (response.Result)
                {
                    var maintenanceForsListDB = _unitOfWork.MaintenanceFors.FindAll(x => x.CategoryId != null, includes: new[] { "Category" }).ToList();

                    DDListDB = maintenanceForsListDB.Select(c => new SelectDDL
                    {
                        ID = (long)c.CategoryId,
                        Name = c.Category?.Name
                    }).Distinct().ToList();
                }
                response.DDLList = DDListDB;
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

        public async Task<BaseResponseWithId<long>> AddVisitsScheduleOfMaintenance(AddVisitsScheduleOfMaintenanceRequest Request, long userID)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.VisitsScheduleOfMaintenanceDataList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.VisitsScheduleOfMaintenanceDataList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert on Item At least.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int Counter = 0;

                    if (Response.Result)
                    {
                        int VisitSerialCounter = 0;
                        foreach (var item in Request.VisitsScheduleOfMaintenanceDataList)
                        {
                            Counter++;




                            if (item.ID != 0)
                            {
                                var VisitsScheduleOfMaintenancesDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(x => x.Id == item.ID).Result.FirstOrDefault();
                                if (VisitsScheduleOfMaintenancesDB != null)
                                {
                                    // Update

                                    DateTime? PlannedDate = null;
                                    DateTime PlannedDateTemp = DateTime.Now;
                                    if (!string.IsNullOrWhiteSpace(item.PlannedDate) && DateTime.TryParse(item.PlannedDate, out PlannedDateTemp))
                                    {
                                        PlannedDateTemp = DateTime.Parse(item.PlannedDate);
                                        PlannedDate = PlannedDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Planned Date #" + Counter;
                                        Response.Errors.Add(error);
                                        return Response;
                                    }
                                    DateTime? VisitDate = null;
                                    DateTime VisitDateTemp = DateTime.Now;
                                    if (!string.IsNullOrWhiteSpace(item.VisitDate))
                                    {
                                        if (DateTime.TryParse(item.VisitDate, out VisitDateTemp))
                                        {
                                            VisitDateTemp = DateTime.Parse(item.VisitDate);
                                            VisitDate = VisitDateTemp;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err-P12";
                                            error.ErrorMSG = "Invalid Visit Date #" + Counter;
                                            Response.Errors.Add(error);
                                            return Response;
                                        }
                                    }
                                    if (VisitsScheduleOfMaintenancesDB != null)
                                    {
                                        if (VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder != null)
                                        {
                                            int CountOFVisitsPerContract = VisitsScheduleOfMaintenancesDB
                                                                          .ManagementOfMaintenanceOrder
                                                                          .VisitsScheduleOfMaintenances
                                                                          .Where(x => x.ManagementOfMaintenanceOrderId == item.ManagementOfMaintenanceOrderID
                                                                          && x.VisitDate != null).Count();
                                            if (VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ContractStatus == "Closed")
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err-P12";
                                                error.ErrorMSG = "This Contract is Closed Item#" + Counter;
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                string ClosingContractType = VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ClosingContractType;
                                                if (ClosingContractType == "Ending Date" || ClosingContractType == "After Period Time")
                                                {
                                                    if (VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.EndDate < DateTime.Today)
                                                    {
                                                        VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ContractStatus = "Closed";
                                                    }
                                                }
                                                else if (ClosingContractType == "After No Visit")
                                                {
                                                    if (CountOFVisitsPerContract == VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.NumberOfVisits)
                                                    {
                                                        VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ContractStatus = "Closed";
                                                    }
                                                }
                                                else if (ClosingContractType == "After Mileage Counter")
                                                {
                                                    var LastMileageCounter = item.MileageCounter ?? 0;
                                                    var CurrentMileageCounter = VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.CurrentMileageCounter ?? 0;

                                                    var MileageUsed = LastMileageCounter - CurrentMileageCounter;
                                                    var ClosingMileageCounter = VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ClosingMileageCounter;
                                                    if (MileageUsed >= ClosingMileageCounter)
                                                    {
                                                        VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ContractStatus = "Closed";

                                                    }
                                                }
                                                else if (VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.NumberOfVisits == CountOFVisitsPerContract && VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.EndDate < DateTime.Today)
                                                {
                                                    VisitsScheduleOfMaintenancesDB.ManagementOfMaintenanceOrder.ContractStatus = "Closed";
                                                }
                                            }
                                        }

                                        VisitsScheduleOfMaintenancesDB.AssignedToId = item.AssignedToID;
                                        VisitsScheduleOfMaintenancesDB.PlannedDate = PlannedDate;
                                        if (VisitDate != null)
                                        {
                                            VisitsScheduleOfMaintenancesDB.VisitDate = VisitDate;
                                        }
                                        VisitsScheduleOfMaintenancesDB.ModificationDate = DateTime.Now;
                                        VisitsScheduleOfMaintenancesDB.ModifiedBy = userID;

                                        _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Update this Visits Schedule Of Maintenance!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Visits Schedule Of Maintenance Doesn't Exist!! Item#" + Counter;
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                DateTime? PlannedDate = null;
                                DateTime PlannedDateTemp = DateTime.Now;
                                if (!string.IsNullOrWhiteSpace(item.PlannedDate) && DateTime.TryParse(item.PlannedDate, out PlannedDateTemp))
                                {
                                    PlannedDateTemp = DateTime.Parse(item.PlannedDate);
                                    PlannedDate = PlannedDateTemp;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "Invalid Planned Date#" + Counter;
                                    Response.Errors.Add(error);
                                }

                                var VisitsListDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(x => x.ManagementOfMaintenanceOrderId == item.ManagementOfMaintenanceOrderID).Result.ToList();
                                int CountOFVisitsPerContract = VisitsListDB.Count();
                                if (CountOFVisitsPerContract > 0)
                                {
                                    // Check if the same visit  -- modified mic on 2023-4-11
                                    var CheckSameVisits = VisitsListDB.Where(x => x.MaintenanceForId == item.MaintenanceForID &&
                                                                                x.AssignedToId == item.AssignedToID &&
                                                                                x.PlannedDate == PlannedDate).Count();
                                    if (CheckSameVisits > 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "This Visit is already exist with the same Planned Date and User On Item#" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                }
                                //  Insert
                                var VisitsScheduleOfMaintenanceDataoBJ = new VisitsScheduleOfMaintenance();
                                var MaintenanceForsIDCheck = _unitOfWork.MaintenanceFors.FindAll(x => x.Id == item.MaintenanceForID).FirstOrDefault();

                                if (MaintenanceForsIDCheck == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "This Maintenance For ID doesn't Exist at Item#" + Counter;
                                    Response.Errors.Add(error);
                                }
                                if (item.AssignedToID != null)
                                {
                                    var UserIDCheck = _unitOfWork.Users.FindAll(x => x.Id == item.AssignedToID).FirstOrDefault();
                                    if (UserIDCheck == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "This User ID Id doesn't Exist Item#" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                }



                                var ManagementOfMaintenanceOrdersIDCheck = _unitOfWork.ManagementOfMaintenanceOrders
                                                                           .FindAll(x => x.Id == item.ManagementOfMaintenanceOrderID).FirstOrDefault();

                                if (ManagementOfMaintenanceOrdersIDCheck.ClosingContractType == "After Mileage Counter")
                                {
                                    if (item.MileageCounter == null || item.MileageCounter == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Mileage Counter is required because closing contract type calculate depend on mileage Item#" + Counter;
                                        Response.Errors.Add(error);
                                        return Response;
                                    }
                                }

                                if (ManagementOfMaintenanceOrdersIDCheck == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "This Management Of Maintenance Order ID doesn't Exist Item#" + Counter;
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                                else if (ManagementOfMaintenanceOrdersIDCheck.MaintenanceForId != item.MaintenanceForID)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "This managment Order ID is not releated to your maintenance selected Item#" + Counter;
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                                else if (ManagementOfMaintenanceOrdersIDCheck.ContractStatus == "Closed")
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "This managment Order ID is Closed Item#" + Counter;
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                                int SerialNo = CountOFVisitsPerContract + 1 + VisitSerialCounter;
                                VisitsScheduleOfMaintenanceDataoBJ.AssignedToId = item.AssignedToID;
                                VisitsScheduleOfMaintenanceDataoBJ.MaintenanceForId = item.MaintenanceForID;
                                VisitsScheduleOfMaintenanceDataoBJ.ManagementOfMaintenanceOrderId = item.ManagementOfMaintenanceOrderID;
                                VisitsScheduleOfMaintenanceDataoBJ.PlannedDate = PlannedDate;
                                VisitsScheduleOfMaintenanceDataoBJ.Serial = (SerialNo).ToString();
                                VisitsScheduleOfMaintenanceDataoBJ.Status = false;
                                VisitsScheduleOfMaintenanceDataoBJ.Active = true;
                                VisitsScheduleOfMaintenanceDataoBJ.VisitDate = null;
                                VisitsScheduleOfMaintenanceDataoBJ.CreationDate = DateTime.Now;
                                VisitsScheduleOfMaintenanceDataoBJ.CreatedBy = userID;
                                VisitsScheduleOfMaintenanceDataoBJ.ModificationDate = DateTime.Now;
                                VisitsScheduleOfMaintenanceDataoBJ.ModifiedBy = userID;
                                VisitsScheduleOfMaintenanceDataoBJ.MileageCounter = item.MileageCounter;

                                // Validate Must be Add Visits Maintenance For < No Visits Maintenance Contract  (Case Of Insert)
                                if (SerialNo > ManagementOfMaintenanceOrdersIDCheck.NumberOfVisits)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "Your Contract is " + ManagementOfMaintenanceOrdersIDCheck.NumberOfVisits +
                                        " Only Can't Add More Visit Item#" + Counter;
                                    Response.Errors.Add(error);
                                }
                                _unitOfWork.VisitsScheduleOfMaintenances.Add(VisitsScheduleOfMaintenanceDataoBJ);
                                _unitOfWork.Complete();
                                VisitSerialCounter++;
                            }

                        }
                        if (!Response.Result)
                        {

                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Add Visit Process not completed success , Please try again";
                            Response.Errors.Add(error);
                            return Response;
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


        public async Task<BaseResponse> DeleteVisitsScheduleOfMaintenance(DeleteVisitScheduleOfMaintenanceRequest Request)
        {
            BaseResponse Response = new BaseResponse();
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
                    // Just will use ID from Model 
                    if (Request.ID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "ID is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var VisitMaintenanceDB = await _unitOfWork.VisitsScheduleOfMaintenances.GetByIdAsync(Request.ID);
                    if (VisitMaintenanceDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Visit Maintenance.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (VisitMaintenanceDB.VisitDate != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This visit Already is visited ,can't delete.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // Validate if not releated any report
                    var CheckVisitReportListDB = _unitOfWork.MaintenanceReports.FindAll(x => x.MaintVisitId == Request.ID).ToList();
                    if (CheckVisitReportListDB.Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This visit Already has reports ,can't delete.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        _unitOfWork.VisitsScheduleOfMaintenances.Delete(VisitMaintenanceDB);
                        var ActionResult = _unitOfWork.Complete();
                        if (ActionResult == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Delete visit not completed ,Please try again.";
                            Response.Errors.Add(error);
                            return Response;
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


        public async Task<BaseResponseWithId<long>> AddVisitsReportDetailsList(AddVisitsMaintenanceReportDetailsData Request, long userID, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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

                    DateTime? NextVisitDate = null;
                    DateTime NextVisitDateTemp = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Request.NextVisitDate) && DateTime.TryParse(Request.NextVisitDate, out NextVisitDateTemp))
                    {
                        NextVisitDateTemp = DateTime.Parse(Request.NextVisitDate);
                        NextVisitDate = NextVisitDateTemp;
                    }
                    if (Response.Result)
                    {
                        var MainVisitIDCheck = _unitOfWork.VisitsScheduleOfMaintenances.FindAllAsync(x => x.Id == Request.MaintVisitID).Result.FirstOrDefault();

                        if (MainVisitIDCheck == null)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = " Visit ID Doesn't Exist";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        else if (MainVisitIDCheck.Status == true)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "The maintenance visit is closed";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        if (Request.ID != 0)
                        {
                            var MaintenanceReportsDB = _unitOfWork.MaintenanceReports.FindAllAsync(x => x.Id == Request.ID).Result.FirstOrDefault();
                            if (MaintenanceReportsDB != null)
                            {
                                DateTime? ReportDate = MaintenanceReportsDB.ReportDate;
                                DateTime ReportDateTemp = DateTime.Now;
                                if (!string.IsNullOrWhiteSpace(Request.ReportDate) && DateTime.TryParse(Request.ReportDate, out ReportDateTemp))
                                {
                                    ReportDateTemp = DateTime.Parse(Request.ReportDate);
                                    ReportDate = ReportDateTemp;
                                }
                                MaintenanceReportsDB.ByUser = Request.ByUser;
                                MaintenanceReportsDB.ByUserId = Request.ByUserID;
                                MaintenanceReportsDB.ClientAddress = Request.ClientAddress;
                                MaintenanceReportsDB.ClientCommentsAndFeedback = Request.ClientCommentsAndFeedback;
                                MaintenanceReportsDB.ClientPrstatus = Request.ClientPRStatus;
                                MaintenanceReportsDB.ClientSatisfactionRate = Request.ClientSatisfactionRate;
                                MaintenanceReportsDB.CollectedAmount = Request.CollectedAmount;
                                MaintenanceReportsDB.Crmcommitment = Request.CRMCommitment;
                                MaintenanceReportsDB.Crmfeedback = Request.CRMFeedback;
                                MaintenanceReportsDB.CrmfeedbackComments = Request.CRMFeedbackComments;
                                MaintenanceReportsDB.CrmfeedbackStatus = Request.CRMFeedbackStatus;
                                MaintenanceReportsDB.DefectDescription = Request.DefectDescription;
                                MaintenanceReportsDB.DesignPrstatus = Request.DesignPRStatus;
                                MaintenanceReportsDB.FabricationPrstatus = Request.FabricationPRStatus;
                                MaintenanceReportsDB.Finished = Request.Finished;
                                MaintenanceReportsDB.InstallationPrstatus = Request.InstallationPRStatus;
                                MaintenanceReportsDB.InternalPartComments = Request.InternalPartComments;
                                MaintenanceReportsDB.MaintenanceTeamPrstatus = Request.MaintenanceTeamPRStatus;
                                MaintenanceReportsDB.MaintVisitId = Request.MaintVisitID;
                                MaintenanceReportsDB.ProductLifePrstatus = Request.ProductLifePRStatus;
                                MaintenanceReportsDB.ReportDate = (DateTime)ReportDate;
                                MaintenanceReportsDB.WorkDescription = Request.WorkDescription;
                                MaintenanceReportsDB.ModifiedBy = userID;
                                MaintenanceReportsDB.ModifiedDate = DateTime.Now;

                                var fileExtension = Request.File.FileName.Split('.').Last();
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.File.FileName);

                                var FilePath = Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/" + "/ClientSignature/", Request.File, FileName, fileExtension, _host);


                                MaintenanceReportsDB.ClientSignature = FilePath;

                                var probfileExtension = Request.ProbelmImage.FileName.Split('.').Last();
                                var probFileName = System.IO.Path.GetFileNameWithoutExtension(Request.ProbelmImage.FileName);

                                var ProblemImageFilePath = Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/" + "/ProblemReport/", Request.ProbelmImage, probFileName, probfileExtension, _host);


                                MaintenanceReportsDB.ProblemReportImage = ProblemImageFilePath;
                                if (MaintenanceReportsDB.MaintVisit.ManagementOfMaintenanceOrder != null)
                                {
                                    var ManagementOfMaintenanceOrderDB = MaintenanceReportsDB.MaintVisit.ManagementOfMaintenanceOrder;
                                    int CountOFVisitsPerContract = ManagementOfMaintenanceOrderDB.VisitsScheduleOfMaintenances.Where(x => x.VisitDate != null).Count();
                                    string ClosingContractType = ManagementOfMaintenanceOrderDB.ClosingContractType;
                                    if (ClosingContractType == "Ending Date" || ClosingContractType == "After Period Time")
                                    {
                                        if (ManagementOfMaintenanceOrderDB.EndDate < DateTime.Today)
                                        {
                                            ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";
                                        }
                                    }
                                    else if (ClosingContractType == "After No Visit")
                                    {
                                        if (CountOFVisitsPerContract == ManagementOfMaintenanceOrderDB.NumberOfVisits)
                                        {
                                            ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";
                                        }
                                    }
                                    else if (ClosingContractType == "After Mileage Counter")
                                    {
                                        var LastMileageCounter = MaintenanceReportsDB.MaintVisit.MileageCounter ?? 0;
                                        var CurrentMileageCounter = ManagementOfMaintenanceOrderDB.CurrentMileageCounter ?? 0;

                                        var MileageUsed = LastMileageCounter - CurrentMileageCounter;
                                        var ClosingMileageCounter = ManagementOfMaintenanceOrderDB.ClosingMileageCounter;
                                        if (MileageUsed >= ClosingMileageCounter)
                                        {
                                            ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";

                                        }
                                    }
                                    else if (ManagementOfMaintenanceOrderDB.NumberOfVisits == CountOFVisitsPerContract && ManagementOfMaintenanceOrderDB.EndDate < DateTime.Today)
                                    {
                                        ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";
                                    }
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Maintenance Report Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }

                                foreach (var item in Request.GetUsersWorkHoursAndEvaluationList)
                                {
                                    if (item.ID != 0 && item.IsDeleted == true)
                                    {

                                        var MaintenanceReportUsersDelted = _unitOfWork.MaintenanceReportUsers.FindAll(x => x.Id == item.ID).FirstOrDefault();

                                        if (MaintenanceReportUsersDelted != null)
                                        {

                                            _unitOfWork.MaintenanceReportUsers.Delete(MaintenanceReportUsersDelted);
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Report User Doesn't exit to be deleted Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        var UserIDCheck = _unitOfWork.Users.FindAll(x => x.Id == item.UserID).FirstOrDefault();

                                        if (UserIDCheck == null)
                                        {
                                            Error error = new Error();
                                            error.ErrorCode = "Err-13";
                                            error.ErrorMSG = " User ID Doesn't Exist";
                                            Response.Errors.Add(error);
                                            Response.Result = false;
                                            return Response;
                                        }

                                        var UserDB = _unitOfWork.Users.FindAll(x => x.Id == item.UserID).FirstOrDefault();
                                        if (UserDB != null && UserDB.JobTitleId != null && UserDB.DepartmentId != null && UserDB.BranchId != null)
                                        {

                                            DateTime? TimeFrom = null;
                                            DateTime TimeFromTemp = DateTime.Now;
                                            if (!string.IsNullOrWhiteSpace(item.TimeFrom) && DateTime.TryParse(item.TimeFrom, out TimeFromTemp))
                                            {
                                                TimeFromTemp = DateTime.Parse(item.TimeFrom);
                                                TimeFrom = TimeFromTemp;
                                            }

                                            DateTime? TimeTo = null;
                                            DateTime TimeToTemp = DateTime.Now;
                                            if (!string.IsNullOrWhiteSpace(item.TimeTo) && DateTime.TryParse(item.TimeTo, out TimeToTemp))
                                            {
                                                TimeToTemp = DateTime.Parse(item.TimeTo);
                                                TimeTo = TimeToTemp;
                                            }

                                            var MaintenanceReportUserObject = new MaintenanceReportUser();
                                            MaintenanceReportUserObject.MaintenanceReportId = MaintenanceReportsDB.Id;
                                            MaintenanceReportUserObject.JobTitleId = (int)UserDB.JobTitleId;
                                            MaintenanceReportUserObject.UserId = item.UserID;
                                            MaintenanceReportUserObject.HourNum = item.HourNum;
                                            MaintenanceReportUserObject.TimeFrom = TimeFrom;
                                            MaintenanceReportUserObject.TimeTo = TimeTo;
                                            MaintenanceReportUserObject.Comment = item.Comment;
                                            MaintenanceReportUserObject.DepartmentId = (int)UserDB.DepartmentId;
                                            MaintenanceReportUserObject.Evaluation = item.Evalution;
                                            MaintenanceReportUserObject.BranchId = (int)UserDB.BranchId;
                                            MaintenanceReportUserObject.CreatedBy = userID;
                                            MaintenanceReportUserObject.CreationDate = DateTime.Now;
                                            _unitOfWork.MaintenanceReportUsers.Add(MaintenanceReportUserObject);
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            Error error = new Error();
                                            error.ErrorCode = "Err-13";
                                            error.ErrorMSG =
                                                "Missing User (" + UserDB.FirstName + " " + UserDB.LastName + ") data from this User (Job Title Or Department or Branch)";
                                            Response.Errors.Add(error);
                                            Response.Result = false;
                                            return Response;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var AddVisitsMaintenanceReportDetailsDataObject = new MaintenanceReport();
                            var VisitMaintenanceReportIDCheck = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(x => x.Id == Request.MaintVisitID).FirstOrDefault();
                            if (VisitMaintenanceReportIDCheck == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Maint Visit ID doesn't Exist";
                                Response.Errors.Add(error);
                                return Response;
                            }

                            DateTime? ReportDate = DateTime.Now;
                            DateTime ReportDateTemp = DateTime.Now;
                            if (!string.IsNullOrWhiteSpace(Request.ReportDate) && DateTime.TryParse(Request.ReportDate, out ReportDateTemp))
                            {
                                ReportDateTemp = DateTime.Parse(Request.ReportDate);
                                ReportDate = ReportDateTemp;
                            }
                            AddVisitsMaintenanceReportDetailsDataObject.ByUser = Request.ByUser;
                            AddVisitsMaintenanceReportDetailsDataObject.ByUserId = Request.ByUserID;
                            AddVisitsMaintenanceReportDetailsDataObject.ClientAddress = Request.ClientAddress;
                            AddVisitsMaintenanceReportDetailsDataObject.ClientCommentsAndFeedback = Request.ClientCommentsAndFeedback;
                            AddVisitsMaintenanceReportDetailsDataObject.ClientPrstatus = Request.ClientPRStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.ClientSatisfactionRate = Request.ClientSatisfactionRate;
                            AddVisitsMaintenanceReportDetailsDataObject.CollectedAmount = Request.CollectedAmount;
                            AddVisitsMaintenanceReportDetailsDataObject.Crmcommitment = Request.CRMCommitment;
                            AddVisitsMaintenanceReportDetailsDataObject.Crmfeedback = Request.CRMFeedback;
                            AddVisitsMaintenanceReportDetailsDataObject.CrmfeedbackComments = Request.CRMFeedbackComments;
                            AddVisitsMaintenanceReportDetailsDataObject.CrmfeedbackStatus = Request.CRMFeedbackStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.DefectDescription = Request.DefectDescription;
                            AddVisitsMaintenanceReportDetailsDataObject.DesignPrstatus = Request.DesignPRStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.FabricationPrstatus = Request.FabricationPRStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.Finished = Request.Finished;
                            AddVisitsMaintenanceReportDetailsDataObject.InstallationPrstatus = Request.InstallationPRStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.InternalPartComments = Request.InternalPartComments;
                            AddVisitsMaintenanceReportDetailsDataObject.MaintenanceTeamPrstatus = Request.MaintenanceTeamPRStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.MaintVisitId = Request.MaintVisitID;
                            AddVisitsMaintenanceReportDetailsDataObject.ProductLifePrstatus = Request.ProductLifePRStatus;
                            AddVisitsMaintenanceReportDetailsDataObject.ReportDate = (DateTime)ReportDate;
                            AddVisitsMaintenanceReportDetailsDataObject.WorkDescription = Request.WorkDescription;
                            AddVisitsMaintenanceReportDetailsDataObject.CreatedBy = userID;
                            AddVisitsMaintenanceReportDetailsDataObject.CreationDate = DateTime.Now;


                            if (Request.File != null)
                            {
                                var fileExtension = Request.File.FileName.Split('.').Last();
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.File.FileName);

                                var FilePath = Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/" + "/ClientSignature/", Request.File, FileName, fileExtension, _host);


                                AddVisitsMaintenanceReportDetailsDataObject.ClientSignature = FilePath;
                            }
                            if (Request.ProbelmImage != null)
                            {
                                var probfileExtension = Request.ProbelmImage.FileName.Split('.').Last();
                                var probFileName = System.IO.Path.GetFileNameWithoutExtension(Request.ProbelmImage.FileName);

                                var ProblemImageFilePath = Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/" + "/ProblemReport/", Request.ProbelmImage, probFileName, probfileExtension, _host);


                                AddVisitsMaintenanceReportDetailsDataObject.ProblemReportImage = ProblemImageFilePath;
                            }

                            _unitOfWork.MaintenanceReports.Add(AddVisitsMaintenanceReportDetailsDataObject);
                            _unitOfWork.Complete();
                            if (VisitMaintenanceReportIDCheck.ManagementOfMaintenanceOrder != null)
                            {
                                var ManagementOfMaintenanceOrderDB = VisitMaintenanceReportIDCheck.ManagementOfMaintenanceOrder;
                                int CountOFVisitsPerContract = ManagementOfMaintenanceOrderDB.VisitsScheduleOfMaintenances.Where(x => x.VisitDate != null).Count();
                                string ClosingContractType = ManagementOfMaintenanceOrderDB.ClosingContractType;
                                if (ClosingContractType == "Ending Date" || ClosingContractType == "After Period Time")
                                {
                                    if (ManagementOfMaintenanceOrderDB.EndDate < DateTime.Today)
                                    {
                                        ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";
                                    }
                                }
                                else if (ClosingContractType == "After No Visit")
                                {
                                    if (CountOFVisitsPerContract == ManagementOfMaintenanceOrderDB.NumberOfVisits)
                                    {
                                        ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";
                                    }
                                }
                                else if (ClosingContractType == "After Mileage Counter")
                                {
                                    var LastMileageCounter = VisitMaintenanceReportIDCheck.MileageCounter ?? 0;
                                    var CurrentMileageCounter = ManagementOfMaintenanceOrderDB.CurrentMileageCounter ?? 0;

                                    var MileageUsed = LastMileageCounter - CurrentMileageCounter;
                                    var ClosingMileageCounter = ManagementOfMaintenanceOrderDB.ClosingMileageCounter;
                                    if (MileageUsed >= ClosingMileageCounter)
                                    {
                                        ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";

                                    }
                                }
                                else if (ManagementOfMaintenanceOrderDB.NumberOfVisits == CountOFVisitsPerContract && ManagementOfMaintenanceOrderDB.EndDate < DateTime.Today)
                                {
                                    ManagementOfMaintenanceOrderDB.ContractStatus = "Closed";
                                }
                            }
                            foreach (var item in Request.GetUsersWorkHoursAndEvaluationList)
                            {
                                var MaintenanceReportUserObject = new MaintenanceReportUser();
                                var UserDB = _unitOfWork.Users.FindAll(x => x.Id == item.UserID).FirstOrDefault();
                                if (UserDB != null && UserDB.JobTitleId != null && UserDB.DepartmentId != null && UserDB.BranchId != null)
                                {

                                    DateTime? TimeFrom = null;
                                    DateTime TimeFromTemp = DateTime.Now;
                                    if (!string.IsNullOrWhiteSpace(item.TimeFrom) && DateTime.TryParse(item.TimeFrom, out TimeFromTemp))
                                    {
                                        TimeFromTemp = DateTime.Parse(item.TimeFrom);
                                        TimeFrom = TimeFromTemp;
                                    }

                                    DateTime? TimeTo = null;
                                    DateTime TimeToTemp = DateTime.Now;
                                    if (!string.IsNullOrWhiteSpace(item.TimeTo) && DateTime.TryParse(item.TimeTo, out TimeToTemp))
                                    {
                                        TimeToTemp = DateTime.Parse(item.TimeTo);
                                        TimeTo = TimeToTemp;
                                    }

                                    MaintenanceReportUserObject.MaintenanceReportId = AddVisitsMaintenanceReportDetailsDataObject.Id;
                                    MaintenanceReportUserObject.JobTitleId = (int)UserDB.JobTitleId;
                                    MaintenanceReportUserObject.UserId = item.UserID;
                                    MaintenanceReportUserObject.HourNum = item.HourNum;
                                    MaintenanceReportUserObject.TimeFrom = TimeFrom;
                                    MaintenanceReportUserObject.TimeTo = TimeTo;
                                    MaintenanceReportUserObject.Comment = item.Comment;
                                    MaintenanceReportUserObject.DepartmentId = (int)UserDB.DepartmentId;
                                    MaintenanceReportUserObject.Evaluation = item.Evalution;
                                    MaintenanceReportUserObject.BranchId = (int)UserDB.BranchId;
                                    MaintenanceReportUserObject.CreatedBy = userID;
                                    MaintenanceReportUserObject.CreationDate = DateTime.Now;
                                    _unitOfWork.MaintenanceReportUsers.Add(MaintenanceReportUserObject);
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Error error = new Error();
                                    error.ErrorCode = "Err-13";
                                    error.ErrorMSG = "Missing data from this User (Job Title Or Department or Branch)";
                                    Response.Errors.Add(error);
                                    Response.Result = false;
                                    return Response;
                                }
                            }

                        }

                        MainVisitIDCheck.VisitDate = DateTime.Today;
                        MainVisitIDCheck.MileageCounter = Request.CurrentMileageCounter;
                        var Res = _unitOfWork.Complete();
                        //var UpdateVisitStatusDB = await _Context.VisitsScheduleOfMaintenances.Where(x => x.ID == Request.MaintVisitID).FirstOrDefaultAsync();

                        if (Request.Finished == true)
                        {
                            MainVisitIDCheck.Status = true;
                            _unitOfWork.Complete();
                        }
                        // Add Next Visit Date 
                        if (NextVisitDate != null)
                        {
                            /*
                            Case 1 : Last Visit on contract -> don't insert
                            Case 2 : - update next planned date
                                     - Add new visit date with this planned date
                             */
                            var ListOfVisitMainListDB = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(x => x.ManagementOfMaintenanceOrderId == MainVisitIDCheck.ManagementOfMaintenanceOrderId).ToList();
                            var CheckContractNoOfVisit = _unitOfWork.ManagementOfMaintenanceOrders
                                .FindAll(x => x.Id == MainVisitIDCheck.ManagementOfMaintenanceOrderId).FirstOrDefault();
                            if (CheckContractNoOfVisit != null)
                            {

                                var LastPlannedDateVisit = ListOfVisitMainListDB.Where(x => x.PlannedDate >= MainVisitIDCheck.VisitDate
                                                                                            && x.VisitDate == null).FirstOrDefault();
                                if (LastPlannedDateVisit != null)
                                {
                                    LastPlannedDateVisit.PlannedDate = NextVisitDate;
                                }
                                else if (CheckContractNoOfVisit.NumberOfVisits > ListOfVisitMainListDB.Count())
                                {
                                    // add new Visit date 
                                    var VisitsScheduleOfMaintenanceDataoBJ = new VisitsScheduleOfMaintenance();
                                    VisitsScheduleOfMaintenanceDataoBJ.AssignedToId = MainVisitIDCheck.AssignedToId;
                                    VisitsScheduleOfMaintenanceDataoBJ.MaintenanceForId = MainVisitIDCheck.MaintenanceForId;
                                    VisitsScheduleOfMaintenanceDataoBJ.ManagementOfMaintenanceOrderId = MainVisitIDCheck.ManagementOfMaintenanceOrderId;
                                    VisitsScheduleOfMaintenanceDataoBJ.PlannedDate = NextVisitDate;
                                    VisitsScheduleOfMaintenanceDataoBJ.Serial = (ListOfVisitMainListDB.Count() + 1).ToString();
                                    VisitsScheduleOfMaintenanceDataoBJ.Status = false;
                                    VisitsScheduleOfMaintenanceDataoBJ.Active = true;
                                    VisitsScheduleOfMaintenanceDataoBJ.VisitDate = null;
                                    VisitsScheduleOfMaintenanceDataoBJ.CreationDate = DateTime.Now;
                                    VisitsScheduleOfMaintenanceDataoBJ.CreatedBy = userID;
                                    VisitsScheduleOfMaintenanceDataoBJ.ModificationDate = DateTime.Now;
                                    VisitsScheduleOfMaintenanceDataoBJ.ModifiedBy = userID;
                                    _unitOfWork.VisitsScheduleOfMaintenances.Add(VisitsScheduleOfMaintenanceDataoBJ);
                                    _unitOfWork.Complete();

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


        public async Task<BaseResponseWithId<long>> AddMaintenanceReportExpenses(AddMaintenanceReportExpensesRequest Request, long userID, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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

                    if (Request.MaintenanceReportId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Visit Maintenance Report Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckMaintenanceReportdb = _unitOfWork.MaintenanceReports.FindAll(x => x.Id == Request.MaintenanceReportId).FirstOrDefault();
                    if (CheckMaintenanceReportdb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Visit Maintenance Report Id , not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.Amount == null || Request.Amount <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Expenses Amount";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.CurrencyId == null || Request.CurrencyId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Currency";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.ExpensesTypeId != null && Request.ExpensesTypeId != 0)
                    {
                        var ExpensesTypeDB = _unitOfWork.ExpensisTypes.FindAll(x => x.Id == Request.ExpensesTypeId).FirstOrDefault();
                        if (ExpensesTypeDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Invalid Expenses Type";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    var ReportExpenses = new MaintenanceReportExpense();
                    if (Response.Result)
                    {

                        if (CheckMaintenanceReportdb != null)
                        {
                            if (Request.MaintenanceReportExpensesId == null || Request.MaintenanceReportExpensesId == 0) // Edit Expenses
                            {
                                ReportExpenses.MaintenanceReportId = Request.MaintenanceReportId;
                                ReportExpenses.Amount = (decimal)Request.Amount;
                                ReportExpenses.CurrencyId = (int)Request.CurrencyId;
                                ReportExpenses.CreatedBy = userID;
                                ReportExpenses.ModifiedBy = userID;
                                ReportExpenses.CreationDate = DateTime.Now;
                                ReportExpenses.ModifiedDate = DateTime.Now;
                                ReportExpenses.ExpensisTypeId = Request.ExpensesTypeId;

                                string FilePath = null;
                                if (Request.File != null)
                                {
                                    var fileExtension = Request.File.FileName.Split('.').Last();
                                    var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.File.FileName);

                                    FilePath = Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/ReportExpenses/" + Request.MaintenanceReportId + "/", Request.File, FileName, fileExtension, _host);
                                }
                                ReportExpenses.FilePath = FilePath;

                                _unitOfWork.MaintenanceReportExpenses.Add(ReportExpenses);
                                _unitOfWork.Complete();


                            }
                            else// Add new Expenses
                            {
                                ReportExpenses = _unitOfWork.MaintenanceReportExpenses.FindAll(x => x.Id == Request.MaintenanceReportExpensesId).FirstOrDefault();
                                if (ReportExpenses != null)
                                {
                                    ReportExpenses.ExpensisTypeId = Request.ExpensesTypeId;
                                    ReportExpenses.Amount = (decimal)Request.Amount;
                                    ReportExpenses.CurrencyId = (int)Request.CurrencyId;
                                    ReportExpenses.ModifiedBy = userID;
                                    ReportExpenses.ModifiedDate = DateTime.Now;
                                    if (Request.Approve != null)
                                    {
                                        ReportExpenses.Approve = (bool)Request.Approve;
                                    }
                                    string FilePath = null;
                                    if (Request.File != null)
                                    {
                                        if (File.Exists(ReportExpenses.FilePath))
                                        {
                                            File.Delete(ReportExpenses.FilePath);
                                        }
                                        var fileExtension = Request.File.FileName.Split('.').Last();
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(Request.File.FileName);

                                        FilePath = Common.SaveFileIFF("/Attachments/" + CompanyName + "/Maintenance/ReportExpenses/" + Request.MaintenanceReportId + "/", Request.File, FileName, fileExtension, _host);
                                    }
                                    ReportExpenses.FilePath = FilePath;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "Invalid Maintenance Report Expenses , Not exist.";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Update Visit Report Expenses!!";
                            Response.Errors.Add(error);
                        }


                        if (!Response.Result)
                        {

                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Add Visit Process not completed success , Please try again";
                            Response.Errors.Add(error);
                            return Response;

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


        public async Task<BaseResponseWithId<long>> AddEditMaintenanceVisitsWithoutContract(AddVisitsScheduleOfMaintenance Request, long userID, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime? ConfirmedDate = null;
                    DateTime? PlannedDate = null;


                    if (Request != null)
                    {
                        if (Request.ID != 0)
                        {
                            VisitsScheduleOfMaintenance MaintenanceVisitDb = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.Id == Request.ID).FirstOrDefault();
                            if (MaintenanceVisitDb == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "This Maintenance Visit Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(Request.ConfirmedDate))
                                {
                                    try
                                    {

                                        ConfirmedDate = DateTime.Parse(Request.ConfirmedDate);
                                    }
                                    catch (Exception)
                                    {
                                        ConfirmedDate = null;
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err10";
                                        error.ErrorMSG = "Confirmed Date Not Valid!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "You Must Enter Confirmed Date!!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(Request.PlannedDate))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "The Planned Date Is Mandatory!!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                try
                                {
                                    PlannedDate = DateTime.Parse(Request.PlannedDate);
                                }
                                catch (Exception)
                                {
                                    ConfirmedDate = null;
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "Planned Date Not Valid!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            if (Request.MaintenanceForID == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "MaintenanceForID Is Mandatory!!";
                                Response.Errors.Add(error);
                            }
                            if (string.IsNullOrWhiteSpace(Request.MaintenanceVisitType))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "MaintenanceVisitType Is Mandatory!!";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Po Invoice Can't Be Null";
                        Response.Errors.Add(error);
                    }

                    if (Request.MaintenanceProblemAttachments != null && Request.MaintenanceProblemAttachments.Count > 0)
                    {
                        var counter = 0;
                        foreach (var attachment in Request.MaintenanceProblemAttachments)
                        {
                            counter++;
                            if (attachment.Id == null)
                            {
                                if (attachment.File != null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Line " + counter + ": File Is Mandatory!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }

                    if (Response.Result)
                    {
                        long MaintenanceVisitId = 0;
                        var VisitsScheduleOfMaintenanceObj = new VisitsScheduleOfMaintenance();
                        if (Request.ID != 0)
                        {
                            VisitsScheduleOfMaintenance MaintenanceVisitDb = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.Id == Request.ID).FirstOrDefault();

                            MaintenanceVisitId = Request.ID;

                            MaintenanceVisitDb.AssignedToId = Request.AssignedToID;
                            MaintenanceVisitDb.ConfirmedDate = ConfirmedDate;
                            MaintenanceVisitDb.ModificationDate = DateTime.Now;
                            MaintenanceVisitDb.ModifiedBy = userID;
                        }
                        else
                        {
                            VisitsScheduleOfMaintenanceObj = new VisitsScheduleOfMaintenance()
                            {
                                AssignedToId = Request.AssignedToID,
                                ClientProblem = Request.ClientProblem,
                                ConfirmedDate = ConfirmedDate,
                                Active = true,
                                CreatedBy = userID,
                                CreationDate = DateTime.Now,
                                ModificationDate = DateTime.Now,
                                ModifiedBy = userID,
                                MaintenanceForId = Request.MaintenanceForID,
                                MaintenanceVisitType = Request.MaintenanceVisitType,
                                ManagementOfMaintenanceOrderId = null,
                                PlannedDate = PlannedDate,
                                Serial = Request.Serial,
                                Status = false,
                                VisitDate = null
                            };

                            _unitOfWork.VisitsScheduleOfMaintenances.Add(VisitsScheduleOfMaintenanceObj);
                            _unitOfWork.Complete();
                            MaintenanceVisitId = VisitsScheduleOfMaintenanceObj.Id;

                            if (Request.MaintenanceProblemAttachments != null && Request.MaintenanceProblemAttachments.Count > 0)
                            {
                                foreach (var attachment in Request.MaintenanceProblemAttachments)
                                {
                                    if (attachment.Id != null && attachment.Active == false)
                                    {
                                        // Delete
                                        var MaintenanceVisitAttach = _unitOfWork.VisitsScheduleOfMaintenanceAttachments.FindAll(a => a.Id == attachment.Id).FirstOrDefault();
                                        if (MaintenanceVisitAttach != null)
                                        {
                                            _Context.VisitsScheduleOfMaintenanceAttachments.Remove(MaintenanceVisitAttach);
                                            if (File.Exists(MaintenanceVisitAttach.AttachmentPath))
                                            {
                                                File.Delete(MaintenanceVisitAttach.AttachmentPath);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var fileExtension = attachment.File.FileName.Split('.').Last();
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.File.FileName);
                                        var virtualPath = $"Attachments\\{CompanyName}\\Maintenance\\ClientsProblems\\{VisitsScheduleOfMaintenanceObj.Id}\\";


                                        var FilePath = Common.SaveFileIFF(virtualPath, attachment.File, FileName, fileExtension, _host);

                                        // Insert
                                        var VisitsScheduleOfMaintenanceAttachObj = new VisitsScheduleOfMaintenanceAttachment()
                                        {
                                            Active = true,
                                            AttachmentPath = FilePath,
                                            Category = null,
                                            CreatedBy = userID,
                                            CreationDate = DateTime.Now,
                                            FileExtenssion = fileExtension,
                                            FileName = FileName,
                                            VisitsScheduleOfMaintenanceId = MaintenanceVisitId
                                        };
                                        _unitOfWork.VisitsScheduleOfMaintenanceAttachments.Add(VisitsScheduleOfMaintenanceAttachObj);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                        }

                        _unitOfWork.Complete();
                        Response.ID = Request.ID != 0 ? MaintenanceVisitId : VisitsScheduleOfMaintenanceObj.Id;
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

        public async Task<bool> AddImportantDateForMaintenance(DateTime ImpDate, string Comment)
        {
            var ImportantDateObj = new ImportantDate();
            ImportantDateObj.ReminderDate = ImpDate;
            ImportantDateObj.Comment = Comment;
            ImportantDateObj.Active = true;
            ImportantDateObj.Status = "Open";
            ImportantDateObj.Type = "VisitMatenance";

            _unitOfWork.ImportantDates.Add(ImportantDateObj);
            var res = _unitOfWork.Complete();

            if (res > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<BaseResponseWithId<long>> UpdateReminderDateVisitOfMaintenance(UpdateReminderDateVisitOfMaintenanceRequest Request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.VisitOfMaintenanceId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Visit Maintenance Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckVisitMaintenancedb = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(x => x.Id == Request.VisitOfMaintenanceId).FirstOrDefault();
                    if (CheckVisitMaintenancedb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Visit Maintenance Id , not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime? ReminderDate = null;
                    DateTime ReminderDateTemp = DateTime.Now;
                    if (!string.IsNullOrWhiteSpace(Request.ReminderDate) && DateTime.TryParse(Request.ReminderDate, out ReminderDateTemp))
                    {
                        ReminderDateTemp = DateTime.Parse(Request.ReminderDate);
                        ReminderDate = ReminderDateTemp;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Reminder Date ";
                        Response.Errors.Add(error);
                    }


                    if (string.IsNullOrWhiteSpace(Request.ReminderHint))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Reminder Hint.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {

                        if (CheckVisitMaintenancedb != null)
                        {

                            if (ReminderDate != null && Request.ReminderHint != null)
                            {
                                CheckVisitMaintenancedb.ReminderDate = ReminderDate;
                                CheckVisitMaintenancedb.ReminderHint = Request.ReminderHint;
                                _unitOfWork.Complete();
                            }

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Update Reminder Date for this Visits Schedule Of Maintenance!!";
                            Response.Errors.Add(error);
                        }


                        if (!Response.Result)
                        {

                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Add Visit Process not completed success , Please try again";
                            Response.Errors.Add(error);
                            return Response;

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

        public BaseResponseWithID AddEditMaintenanceType(AddEditMaintenanceTypeRequest Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                long newMaintenanceTypeId = 0;
                if (Response.Result)
                {
                    VehicleMaintenanceType MaintenanceType = null;
                    if (Request.Id == null || Request.Id == 0)
                    {
                        //Add
                        MaintenanceType = new VehicleMaintenanceType()
                        {
                            Name = Request.Name,
                            Description = Request.Description,
                            VehicleRateId = Request.VehicleRateId,
                            Comment = Request.Comment,
                            VehiclePriorityLevelId = Request.VehiclePriorityLevelId,
                            Bomid = Request.BOMID,
                            Milage = Request.Milage,
                            Active = true,
                            CreationDate = DateTime.Now,
                            CreatedBy = UserID,
                        };
                        var AddedMaintenanceType = _unitOfWork.VehicleMaintenanceTypes.Add(MaintenanceType);
                        _unitOfWork.Complete();
                        newMaintenanceTypeId = AddedMaintenanceType.Id;

                        Response.ID = newMaintenanceTypeId;
                    }
                    else
                    {
                        //Edit
                        MaintenanceType = _Context.VehicleMaintenanceTypes.Find(Request.Id);
                        if (MaintenanceType != null)
                        {
                            MaintenanceType.Name = Request.Name;
                            MaintenanceType.Description = Request.Description;
                            MaintenanceType.VehicleRateId = Request.VehicleRateId;
                            MaintenanceType.Comment = Request.Comment;
                            MaintenanceType.VehiclePriorityLevelId = Request.VehiclePriorityLevelId;
                            MaintenanceType.Bomid = Request.BOMID;
                            MaintenanceType.Milage = Request.Milage;
                            MaintenanceType.Active = true;
                            MaintenanceType.ModifiedDate = DateTime.Now;
                            MaintenanceType.ModifiedBy = UserID;

                            _unitOfWork.Complete();

                            var catList = _unitOfWork.VehicleMaintenanceTypeServiceSheduleCategories.FindAll(a => a.VehicleMaintenanceTypeId == MaintenanceType.Id).ToList();
                            var modelBrandList = _unitOfWork.VehicleMaintenanceTypeForModels.FindAll(a => a.VehicleMaintenanceTypeId == MaintenanceType.Id).ToList();
                            _unitOfWork.VehicleMaintenanceTypeServiceSheduleCategories.DeleteRange(catList);
                            _unitOfWork.VehicleMaintenanceTypeForModels.DeleteRange(modelBrandList);
                            _unitOfWork.Complete();

                            newMaintenanceTypeId = MaintenanceType.Id;
                        }

                    }
                    if (MaintenanceType != null && newMaintenanceTypeId != 0)
                    {
                        foreach (var i in Request.VehicleMaintenanceTypeServiceSheduleCategories)
                        {
                            var cat = new VehicleMaintenanceTypeServiceSheduleCategory()
                            {
                                VehicleMaintenanceTypeId = newMaintenanceTypeId,
                                VehicleServiceScheduleCategoryId = i,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,
                                Active = true,
                            };
                            _unitOfWork.VehicleMaintenanceTypeServiceSheduleCategories.Add(cat);
                        }
                        _unitOfWork.Complete();
                        if (Request.IsForAllModels)
                        {
                            var forBrand = new VehicleMaintenanceTypeForModel()
                            {
                                VehicleMaintenanceTypeId = newMaintenanceTypeId,
                                ForAllModles = true,
                                BrandId = null,
                                ModelId = null,
                                Active = true,
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,

                            };
                            _unitOfWork.VehicleMaintenanceTypeForModels.Add(forBrand);
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            foreach (var i in Request.VehicleMaintenanceTypeForBrandsStrings)
                            {
                                var forBrand = new VehicleMaintenanceTypeForModel()
                                {
                                    VehicleMaintenanceTypeId = newMaintenanceTypeId,
                                    ForAllModles = false,
                                    BrandId = i,
                                    ModelId = null,
                                    Active = true,
                                    CreationDate = DateTime.Now,
                                    CreatedBy = UserID,
                                };
                                _unitOfWork.VehicleMaintenanceTypeForModels.Add(forBrand);
                            }
                            _unitOfWork.Complete();
                            var modelsIds = Request.VehicleMaintenanceTypeForModelsStrings.Where(x => !Request.VehicleMaintenanceTypeForBrandsStrings.Contains(_Context.VehicleModels.Find(x).VehicleBrandId));
                            foreach (var i in modelsIds)
                            {
                                var forBrand = new VehicleMaintenanceTypeForModel()
                                {
                                    VehicleMaintenanceTypeId = newMaintenanceTypeId,
                                    ForAllModles = false,
                                    BrandId = null,
                                    ModelId = i,
                                    Active = true,
                                    CreationDate = DateTime.Now,
                                    CreatedBy = UserID,
                                };
                                _unitOfWork.VehicleMaintenanceTypeForModels.Add(forBrand);
                            }
                            _unitOfWork.Complete();
                        }
                    }

                }
                Response.ID = newMaintenanceTypeId;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ViewAllMaintenanceTypesResponse ViewAllMaintenanceTypes()
        {
            ViewAllMaintenanceTypesResponse Response = new ViewAllMaintenanceTypesResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    GetMaintenanceTypes VehicleMaintenanceTypeItems = new GetMaintenanceTypes();

                    var typesList = _unitOfWork.VehicleMaintenanceTypes.FindAll(a => a.Active == true, new[] { "Bom", "VehicleRate", "VehicleMaintenanceTypeServiceSheduleCategories", "VehicleMaintenanceTypeServiceSheduleCategories.VehicleServiceScheduleCategory", "VehicleMaintenanceTypeForModels" }).ToList();

                    var MaintenanceTypesList = new List<MaintenanceType>();

                    foreach (var type in typesList)
                    {
                        var BOMPartitionItems = _unitOfWork.VBompartitionItemsInventoryItems.FindAll(a => a.Bomid == type.Bomid).ToList();
                        decimal TotalCostType1, TotalCostType2, TotalCostType3;
                        TotalCostType1 = TotalCostType2 = TotalCostType3 = 0;
                        foreach (var x in BOMPartitionItems)
                        {
                            TotalCostType1 += (x.CostAmount1 * x.RequiredQty) ?? 0;
                            TotalCostType2 += (x.CostAmount2 * x.RequiredQty) ?? 0;
                            TotalCostType3 += (x.CostAmount3 * x.RequiredQty) ?? 0;
                        }
                        var isForAllModels = type.VehicleMaintenanceTypeForModels.Where(x => x.ForAllModles == true && x.Active == true).FirstOrDefault() != null ? true : false;
                        var brandIds = type.VehicleMaintenanceTypeForModels.Where(x => x.ForAllModles == false && x.Active == true).Select(x => x.BrandId).ToList();
                        List<string> brandsStrings = new List<string>();
                        List<string> modelsStrings = new List<string>();
                        foreach (var i in brandIds)
                        {
                            var brand = _unitOfWork.VehicleBrands.GetById(i ?? 0);
                            if (brand != null) brandsStrings.Add(brand.Name);
                        }
                        var modelsIds = type.VehicleMaintenanceTypeForModels.Where(x => x.ForAllModles == false && x.Active == true && !brandIds.Contains(x.BrandId)).Select(x => x.ModelId).ToList();

                        var forModelsAndBrands = new List<string>();
                        foreach (var i in modelsIds)
                        {
                            var model = _unitOfWork.VehicleModels.GetById(i ?? 0);
                            if (model != null)
                            {
                                var brand = _unitOfWork.VehicleBrands.GetById(model.VehicleBrandId);
                                if (brand != null) modelsStrings.Add(brand.Name + " " + model.Name); else modelsStrings.Add(model.Name);
                            }
                        }
                        var maintenanceType = new MaintenanceType()
                        {
                            ID = type.Id,
                            Name = type.Name,
                            VehicleRateId = type.VehicleRateId,
                            VehicleRateName = type.VehicleRate.RateName,
                            Description = type.Description,
                            Comment = type.Comment,
                            VehiclePriorityLevelId = type.VehiclePriorityLevelId,
                            isForAllModels = isForAllModels,
                            VehicleMaintenanceTypeForModelsStrings = isForAllModels == true ? new List<string>() : modelsStrings,
                            VehicleMaintenanceTypeForBrandsStrings = isForAllModels == true ? new List<string>() : brandsStrings,
                            VehicleMaintenanceTypeBOM = type.Bomid != null ? new VehicleMaintenanceTypeBOM
                            {
                                BOMID = type.Bomid,
                                BOMName = type.Bom?.Name,
                                TotalCostType1 = TotalCostType1,
                                TotalCostType2 = TotalCostType2,
                                TotalCostType3 = TotalCostType3
                            } : null,
                            VehicleMaintenanceTypeServiceSheduleCategories = type.VehicleMaintenanceTypeServiceSheduleCategories.Select(x => x.VehicleServiceScheduleCategory.ItemName).ToList(),
                            Milage = type.Milage,
                        };
                        MaintenanceTypesList.Add(maintenanceType);

                    }
                    VehicleMaintenanceTypeItems.maintenanceTypeList = MaintenanceTypesList;
                    Response.AllMaintenanceTypes = VehicleMaintenanceTypeItems;
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetMaintenanceClientsCardsData> GetClientsCardsDataResponse(GetClientsCardsDataResponseFilters filters)
        {
            GetMaintenanceClientsCardsData Response = new GetMaintenanceClientsCardsData();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    DateTime FromDateTemp = DateTime.Now;
                    if (filters.FromDate != null)
                    {
                        FromDateTemp = (DateTime)filters.FromDate;
                    }
                    DateTime ToDateTemp = DateTime.Now;
                    if (filters.ToDate != null)
                    {
                        ToDateTemp = (DateTime)filters.ToDate;
                    }
                    if (!string.IsNullOrWhiteSpace(filters.SearchKey))
                    {
                        filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                    }



                    // Queries
                    bool HasFilter = false;
                    //var ClientIDs = new List<long>();
                    var ClientsDBQuery = _unitOfWork.Clients.FindAllQueryable(a => true, includes: new[] { "ClientAddresses", });

                    var MaintenanceList = _unitOfWork.MaintenanceFors.FindAllQueryable(a => true, includes: new[] { "VisitsScheduleOfMaintenances", "ManagementOfMaintenanceOrders" });
                    if (filters.ClientID != 0)
                    {
                        MaintenanceList = MaintenanceList.Where(x => x.ClientId == filters.ClientID).AsQueryable();
                        ClientsDBQuery = ClientsDBQuery.Where(x => x.Id == filters.ClientID).AsQueryable();
                    }

                    if (filters.CategoryID != 0)
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.CategoryId == filters.CategoryID).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(filters.MaintenanceType))
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.ProductType.Contains(filters.MaintenanceType)).AsQueryable();
                    }

                    if (!string.IsNullOrWhiteSpace(filters.ProductBrand))
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.ProductBrand.Contains(filters.ProductBrand)).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(filters.ProductFabricator))
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.ProductFabricator.Contains(filters.ProductFabricator)).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(filters.SearchKey))
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.ProductName.ToLower().Contains(filters.SearchKey.ToLower())
                                                                      || x.ProductSerial.ToLower().Contains(filters.SearchKey.ToLower())
                                                                      || x.SalesOffer.ProjectName.ToLower().Contains(filters.SearchKey.ToLower())

                        ).AsQueryable();
                    }
                    if (filters.AreaId != 0)
                    {
                        ClientsDBQuery = ClientsDBQuery.Where(x => x.ClientAddresses.Where(y => y.AreaId == filters.AreaId).Any());
                        //ClientIDs.AddRange(await _Context.ClientAddresses.Where(x => x.AreaID == AreaID).Select(x => x.ClientID).ToListAsync());
                    }
                    if (filters.GovernorateId != 0)
                    {
                        ClientsDBQuery = ClientsDBQuery.Where(x => x.ClientAddresses.Where(y => y.GovernorateId == filters.GovernorateId).Any());
                    }
                    if (filters.CountryId != 0)
                    {
                        ClientsDBQuery = ClientsDBQuery.Where(x => x.ClientAddresses.Where(y => y.CountryId == filters.CountryId).Any());
                    }
                    if (filters.FromDate != null)
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.VisitsScheduleOfMaintenances.Where(z => z.VisitDate >= FromDateTemp).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
                    }

                    if (filters.ToDate != null)
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.VisitsScheduleOfMaintenances.Where(z => z.VisitDate <= ToDateTemp).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
                    }


                    if (filters.ContractType != null && filters.ContractType == "Valid")
                    {

                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.ManagementOfMaintenanceOrders.Where(z => z.StartDate >= DateTime.Today && z.EndDate >= DateTime.Today).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
                    }

                    if (filters.ContractType != null && filters.ContractType == "Expired")
                    {
                        HasFilter = true;
                        MaintenanceList = MaintenanceList.Where(x => x.ManagementOfMaintenanceOrders.Where(z => z.EndDate < DateTime.Today).Select(y => y.MaintenanceForId).Contains(x.Id)).AsQueryable();
                    }
                    /*
                     -must be end date  > today
                     -subtract end date from today if smaller than the target request 
                     - return this contract because this is filter result
                    */
                    if (filters.ContractType != null && filters.ContractType == "ExpireSoon" && filters.WeekNum != 0)
                    {
                        HasFilter = true;
                        TimeSpan WeekNumberConv = TimeSpan.Parse((7 * filters.WeekNum).ToString());

                        var GetManagementOfMaintenanceOrderFiltered = (await _unitOfWork.ManagementOfMaintenanceOrders.FindAllAsync(z => z.EndDate != null && z.EndDate > DateTime.Now)).ToList();

                        if (filters.ClientID != 0)
                        {
                            GetManagementOfMaintenanceOrderFiltered = GetManagementOfMaintenanceOrderFiltered.Where(x => x.MaintenanceFor?.ClientId == filters.ClientID).ToList();
                        }

                        var MaintenanceForIDsList = new List<long>();

                        foreach (var item in GetManagementOfMaintenanceOrderFiltered)
                        {
                            if (((DateTime)item.EndDate).AddDays(-7 * filters.WeekNum) <= DateTime.Today)
                            {
                                if (item.MaintenanceForId != null)
                                {

                                    MaintenanceForIDsList.Add((long)item.MaintenanceForId);
                                }
                            }
                        }
                        MaintenanceList = MaintenanceList.Where(x => MaintenanceForIDsList.Contains(x.Id)).AsQueryable();
                    }

                    if (Response.Result)
                    {
                        ClientsDBQuery = ClientsDBQuery.Where(x => x.NeedApproval == 0).AsQueryable(); // Approved
                        if (HasFilter)
                        {
                            var ClientsIdHaveMaintenance = MaintenanceList.Select(x => x.ClientId).ToList();
                            ClientsDBQuery = ClientsDBQuery.Where(c => ClientsIdHaveMaintenance.Contains(c.Id)).AsQueryable();
                        }
                        //if (HasFilter && ClientIDs.Count() > 0)
                        //{
                        //    ClientIDs = MaintenanceList.Select(x => x.ClientID).ToList();
                        //    ClientsDBQuery = ClientsDBQuery.Where(x => ClientIDs.Contains(x.ID));
                        //}
                        var ClientsListDB = PagedList<Client>.Create(ClientsDBQuery.OrderByDescending(a => a.CreationDate), filters.CurrentPage, filters.NumberOfItemsPerPage);
                        var ClientResponse = new List<ClientBaseData>();

                        List<MaintenanceFor> ClientsMaintenanceFors = new List<MaintenanceFor>();
                        if (ClientsListDB.Count > 0)
                        {
                            var ClientsIds = ClientsListDB.Select(a => a.Id).ToList();
                            ClientsMaintenanceFors = (await _unitOfWork.MaintenanceFors.FindAllAsync(a => ClientsIds.Contains(a.ClientId))).ToList();
                        }

                        foreach (var Client in ClientsListDB)
                        {

                            var ClientDataObj = new ClientBaseData();
                            ClientDataObj.id = Client.Id;
                            ClientDataObj.idEnc = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(Client.Id.ToString(), key));
                            ClientDataObj.name = Client.Name;
                            ClientDataObj.hasLogo = Client.HasLogo;
                            ClientDataObj.maintenanceForCount = ClientsMaintenanceFors.Count > 0 ? ClientsMaintenanceFors.Where(a => a.ClientId == Client.Id).Count() : 0;
                            ClientDataObj.NeedApproval = Client.NeedApproval??0;
                            if (Client.LogoUrl != null)
                            {
                                ClientDataObj.logo = Globals.baseURL + "/" + Client.LogoUrl;
                            }

                            ClientResponse.Add(ClientDataObj);
                        }

                        Response.ClientsData = ClientResponse;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = ClientsListDB.CurrentPage,
                            TotalPages = ClientsListDB.TotalPages,
                            ItemsPerPage = ClientsListDB.PageSize,
                            TotalItems = ClientsListDB.TotalCount
                        };

                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponse DeleteMaintenanceVisit([FromHeader] long VisitId)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (VisitId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Visit Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var Visit = _unitOfWork.VisitsScheduleOfMaintenances.GetById(VisitId);
                if (Visit == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Visit is Not found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (Visit.Status)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "can't delete Visit After it was done!";
                    Response.Errors.Add(error);
                    return Response;
                }
                Visit.Active = false;
                _unitOfWork.VisitsScheduleOfMaintenances.Update(Visit);
                _unitOfWork.Complete();

                var Management = _unitOfWork.ManagementOfMaintenanceOrders.FindAll(a => a.MaintenanceForId == Visit.MaintenanceForId).FirstOrDefault();
                Management.NumberOfVisits = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.ManagementOfMaintenanceOrderId == Management.Id && a.Active && a.MaintenanceFor.Client.NeedApproval != 2, includes: new[] { "MaintenanceFor.Client" }).Count();
                _unitOfWork.ManagementOfMaintenanceOrders.Update(Management);
                _unitOfWork.Complete();
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<string> GetVisitScheduleReport(GetVisitScheduleReportFilters filters)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                // Start with base query
                var query = _unitOfWork.VisitsScheduleOfMaintenances.FindAllQueryable(a => true);

                // Apply filters in optimal order (most restrictive first)
                if (filters.ClientId != null)
                {
                    query = query.Where(a => a.MaintenanceFor.ClientId == filters.ClientId);
                }

                if (filters.VisitStatus == "Completed")
                {
                    query = query.Where(a => a.Status);
                }
                else if (filters.VisitStatus == "Not Completed")
                {
                    query = query.Where(a => !a.Status);
                }
                else if (filters.VisitStatus == "Late")
                {
                    var now = DateTime.Now;
                    query = query.Where(a => a.PlannedDate < now && !a.Status);

                    if (filters.Months != null && filters.Months > 0)
                    {
                        var toDate = now.AddDays(-1);
                        var fromDate = toDate.AddMonths(-(int)filters.Months.Value);

                        if (filters.Months % 1 != 0) // Has fractional month
                        {
                            var daysInMonth = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);
                            fromDate = fromDate.AddDays(-(daysInMonth * (filters.Months.Value % 1)));
                        }

                        query = query.Where(a => a.PlannedDate <= toDate && a.PlannedDate >= fromDate);
                    }
                }

                // Apply date ranges
                if (filters.From != null)
                {
                    query = filters.VisitStatus == "Completed" || filters.VisitStatus == null
                        ? query.Where(a => a.VisitDate >= filters.From)
                        : query.Where(a => a.PlannedDate >= filters.From);
                }

                if (filters.To != null)
                {
                    query = filters.VisitStatus == "Completed" || filters.VisitStatus == null
                        ? query.Where(a => a.VisitDate <= filters.To)
                        : query.Where(a => a.PlannedDate <= filters.To);
                }

                // Apply search last (most expensive operation)
                if (!string.IsNullOrEmpty(filters.SearchKey))
                {
                    var searchKey = filters.SearchKey.ToLower();
                    query = query.Where(a =>
                        a.MaintenanceFor.SalesOffer.SalesOfferProducts
                            .Any(sop => sop.InventoryItem.Name.ToLower().Contains(searchKey))
                        ||
                        (a.MaintenanceFor.ProductSerial != null &&
                         a.MaintenanceFor.ProductSerial.ToLower().Contains(searchKey)));
                }

                // Optimized include strategy
                query = query
                    .Include(a => a.MaintenanceFor.Client)
                    .ThenInclude(a => a.ClientContactPeople)
                    .Include(a => a.MaintenanceFor.Category)                      
                    .Include(a => a.MaintenanceReports)
                    .Include(a => a.MaintenanceFor.ManagementOfMaintenanceOrders)
                    .Include(a => a.MaintenanceFor.VisitsScheduleOfMaintenances)
                        .ThenInclude(v => v.MaintenanceReports)
                        .OrderBy(x => x.VisitDate);

                // Final execution with pagination consideration
                var VisitList = query.ToList(); // Or ToListAsync() for async

                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[12] {
                new DataColumn("Client"),
                new DataColumn("Client Mobile"),
                new DataColumn("Maintenance Item Type"),
                new DataColumn("Serial"),
                new DataColumn("Worker"),
                new DataColumn("Maintenance Cause"),
                new DataColumn("Worker Description"),
                new DataColumn("Client Comments And Feedback"),
                //new DataColumn("Visit Date"),
                new DataColumn("Maintenance Cost"),
                new DataColumn("Last Report Date"),
                new DataColumn("Contract Type"),
                new DataColumn("Maitnenance Visit Type")
                });

                foreach (var item in VisitList)
                {
                    dt.Rows.Add(
                       item.MaintenanceFor?.Client.Name,
                       item.MaintenanceFor?.Client.ClientContactPeople?.FirstOrDefault()?.Mobile,
                       item.MaintenanceFor?.Category?.Name,
                       item.MaintenanceFor?.ProductSerial,
                       item.MaintenanceReports?.FirstOrDefault()?.ByUser,
                       item.MaintenanceReports?.FirstOrDefault()?.DefectDescription,
                       item.MaintenanceReports?.FirstOrDefault()?.WorkDescription,
                       item.MaintenanceReports?.FirstOrDefault()?.ClientCommentsAndFeedback,
                       //item.VisitDate?.ToShortDateString(),
                       item.MaintenanceReports?.FirstOrDefault()?.CollectedAmount,
                       item.MaintenanceReports?.FirstOrDefault()?.CreationDate.ToString("dd/MM/yyyy h:m:s tt"),
                       item.MaintenanceFor.ManagementOfMaintenanceOrders.FirstOrDefault()?.ContractType,
                       item.MaintenanceVisitType ?? "With Contract"
                       );
                }

                var workSheet = package.Workbook.Worksheets.Add("VisitScheduleReport");
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 12].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 12].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 174, 81));
                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                for (int i = 1; i <= excelRangeBase.Columns; i++)
                {
                    workSheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= excelRangeBase.Rows; i++)
                {
                    workSheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                workSheet.View.FreezePanes(2, 1);

                var newpath = $"Attachments\\{validation.CompanyName}\\VisitScheduleReports";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                {
                    File.Delete(savedPath);
                }
                Directory.CreateDirectory(savedPath);
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\VisitScheduleReport_{date}.xlsx";
                package.SaveAs(excelPath);
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\VisitScheduleReport_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponse DeleteManagementOfMaintenancee([FromHeader] long ContractId, [FromHeader] bool DeleteContract)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (ContractId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Contract Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var Contract = _unitOfWork.ManagementOfMaintenanceOrders.GetById(ContractId);
                if (Contract == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Contract is Not found!";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (DeleteContract)
                {
                    Contract.Active = false;
                }
                var visits = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.ManagementOfMaintenanceOrderId == ContractId).ToList();
                if (visits.Count() > 0)
                {
                    foreach (var visit in visits)
                    {
                        visit.Active = false;
                        _unitOfWork.VisitsScheduleOfMaintenances.Update(visit);
                    }
                }
                Contract.NumberOfVisits = 0;
                _unitOfWork.ManagementOfMaintenanceOrders.Update(Contract);
                _unitOfWork.Complete();
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public BaseResponseWithId<long> AddSalesOfferForMAintenance(MaintenanceOfferDTO dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (dto == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Invalid Data";
                    response.Errors.Add(error);
                    return response;
                }
                if (dto.WorkerOrder == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Worker Order Is Mendatory";
                    response.Errors.Add(error);
                    return response;
                }
                if (dto.ClientId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Client Id Is Mendatory";
                    response.Errors.Add(error);
                    return response;
                }

                bool offerUpdate = dto.OfferId != null;
                SalesOffer Offer;
                Project projectObj;
                var NewOfferSerial = "";
                long newOfferNumber = 1;
                var lastSalesOfferSerial = _unitOfWork.SalesOffers
                    .FindAllQueryable(a => a.Active)
                    .OrderByDescending(a => a.Id)
                    .Select(a => a.OfferSerial)
                    .FirstOrDefault();

                if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                {
                    var ListSplit = lastSalesOfferSerial.Split('-');
                    string strLastOfferNumber = ListSplit[0];
                    newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                }
                NewOfferSerial += "#" + newOfferNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();

                Offer = new SalesOffer()
                {
                    ClientId = dto.ClientId,
                    StartDate = DateOnly.Parse(dto.StartDate),
                    EndDate = DateOnly.Parse(dto.EndDate),
                    OfferSerial = NewOfferSerial + "-" + dto.WorkerOrder,
                    SalesPersonId = dto.SalesPersonId > 0 ? dto.SalesPersonId : validation.userID,
                    CreationDate = DateTime.Now,
                    Modified = DateTime.Now,
                    CreatedBy = validation.userID,
                    Status = dto.OfferStatus,
                    ModifiedBy = validation.userID,
                    BranchId = _unitOfWork.Branches.FindAll(a => true).FirstOrDefault()?.Id ?? 0,
                    ContactPersonName = dto.ContactPersonName,
                    ContactPersonMobile = dto.ContactPersonMobile,
                    ContactPersonEmail = dto.ContactPersonEmail,
                    ProjectLocation = dto.ProjectLocation,
                    OfferType = dto.OfferType,
                    Active = true
                };
                _unitOfWork.SalesOffers.Add(Offer);
                _unitOfWork.Complete();

                var location = new SalesOfferLocation()
                {
                    SalesOfferId = Offer.Id,
                    LocationX = dto.LocationX,
                    LocationY = dto.LocationY,
                    Description = dto.ProjectLocation,
                    Active = true,
                    CreatedBy = validation.userID.ToString(),
                    ModifiedBy = validation.userID.ToString(),
                    CreationDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };
                _unitOfWork.SalesOfferLocations.Add(location);
                _unitOfWork.Complete();


                projectObj = new Project();
                projectObj.SalesOfferId = Offer.Id;
                projectObj.Closed = true;
                projectObj.StartDate = DateTime.Parse(dto.StartDate);
                projectObj.EndDate = DateTime.Parse(dto.EndDate);
                projectObj.Revision = 0;
                projectObj.CreatedBy = validation.userID;
                projectObj.CreationDate = DateTime.Now;
                projectObj.BranchId = _unitOfWork.Users.GetById(validation.userID)?.BranchId ?? 0;
                projectObj.ModifiedDate = DateTime.Now;
                projectObj.ModifiedBy = validation.userID;
                projectObj.Active = true;

                _unitOfWork.Projects.Add(projectObj);
                _unitOfWork.Complete();
                MaintenanceFor maintenance;

                foreach (var item in dto.Maintenances)
                {

                    maintenance = new MaintenanceFor()
                    {
                        ProjectId = projectObj.Id,
                        SalesOfferId = Offer.Id,
                        ProductName = item.ProductName,
                        ProductBrand = item.ProductBrand,
                        ClientId = Offer.ClientId ?? 0,
                        GeneralNote = item.Problem,
                        CreatedBy = validation.userID,
                        CreationDate = DateTime.Now,
                        ProductFabricator = dto.ProductFabricator // Vendor
                    };
                    _unitOfWork.MaintenanceFors.Add(maintenance);

                }
                _unitOfWork.Complete();
                response.ID = Offer.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithId<long> UpdateSalesOfferForMAintenance(MaintenanceOfferDTO dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (dto == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Invalid Data";
                    response.Errors.Add(error);
                    return response;
                }
                if (dto.WorkerOrder == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Worker Order Is Mendatory";
                    response.Errors.Add(error);
                    return response;
                }
                if (dto.ClientId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Client Id Is Mendatory";
                    response.Errors.Add(error);
                    return response;
                }

                bool offerUpdate = dto.OfferId != null;
                SalesOffer Offer;
                Project projectObj;
                if (!offerUpdate)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err106";
                    error.ErrorMSG = "Invalid Data!";
                    response.Errors.Add(error);
                    return response;
                }

                Offer = _unitOfWork.SalesOffers.GetById(dto.OfferId ?? 0);
                if (Offer == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "No Offer With This Id";
                    response.Errors.Add(error);
                    return response;
                }
                Offer.ClientId = dto.ClientId;
                Offer.StartDate = DateOnly.Parse(dto.StartDate);
                Offer.EndDate = DateOnly.Parse(dto.EndDate);
                Offer.SalesPersonId = dto.SalesPersonId>0?dto.SalesPersonId:validation.userID;
                Offer.Modified =  DateTime.Now;
                Offer.ModifiedBy = validation.userID;
                Offer.Status = dto.OfferStatus;
                Offer.BranchId = _unitOfWork.Branches.FindAll(a => true).FirstOrDefault()?.Id ?? 0;
                Offer.ContactPersonName = dto.ContactPersonName;
                Offer.ContactPersonMobile = dto.ContactPersonMobile;
                Offer.ContactPersonEmail = dto.ContactPersonEmail;
                Offer.ProjectLocation = dto.ProjectLocation;
                _unitOfWork.SalesOffers.Update(Offer);

                var location = _unitOfWork.SalesOfferLocations.FindAll(a => a.SalesOfferId == Offer.Id).FirstOrDefault();
                if (location != null)
                {
                    location.Description = Offer.ProjectLocation;
                    location.LocationX = dto.LocationX;
                    location.LocationY = dto.LocationY;
                    location.ModifiedBy = validation.userID.ToString();
                    location.ModifiedDate = DateTime.Now;
                }
                else
                {
                    location = new SalesOfferLocation()
                    {
                        SalesOfferId = Offer.Id,
                        LocationX = dto.LocationX,
                        LocationY = dto.LocationY,
                        Description = dto.ProjectLocation,
                        Active = true,
                        CreatedBy = validation.userID.ToString(),
                        ModifiedBy = validation.userID.ToString(),
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    _unitOfWork.SalesOfferLocations.Add(location);
                }
                _unitOfWork.Complete();

                projectObj = _unitOfWork.Projects.FindAll(a => a.SalesOfferId == Offer.Id).FirstOrDefault();
                projectObj.StartDate = DateTime.Parse(dto.StartDate);
                projectObj.EndDate = DateTime.Parse(dto.EndDate);
                _unitOfWork.Projects.Update(projectObj);
                _unitOfWork.Complete();
                MaintenanceFor maintenance;

                foreach (var item in dto.Maintenances)
                {
                    if (item.Id != null)
                    {
                        maintenance = _unitOfWork.MaintenanceFors.GetById(item.Id ?? 0);
                        if (maintenance == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err105";
                            error.ErrorMSG = $"Maintenance number #{dto.Maintenances.IndexOf(item) + 1} is not found";
                            response.Errors.Add(error);
                        }
                        else
                        {
                            if (!item.Active)
                            {
                                _unitOfWork.MaintenanceFors.Delete(maintenance);
                            }
                            else
                            {
                                maintenance.ProjectId = projectObj.Id;
                                maintenance.SalesOfferId = Offer.Id;
                                maintenance.ProductName = item.ProductName;
                                maintenance.ProductBrand = item.ProductBrand;
                                maintenance.ClientId = Offer.ClientId ?? 0;
                                maintenance.GeneralNote = item.Problem;
                                maintenance.ProductFabricator = dto.ProductFabricator;
                                _unitOfWork.MaintenanceFors.Update(maintenance);
                            }
                        }
                    }
                    else
                    {
                        maintenance = new MaintenanceFor()
                        {
                            ProjectId = projectObj.Id,
                            SalesOfferId = Offer.Id,
                            ProductName = item.ProductName,
                            ProductBrand = item.ProductBrand,
                            ClientId = Offer.ClientId ?? 0,
                            GeneralNote = item.Problem,
                            CreatedBy = validation.userID,
                            CreationDate = DateTime.Now,
                            ProductFabricator = dto.ProductFabricator,
                        };
                        _unitOfWork.MaintenanceFors.Add(maintenance);
                    }
                }
                _unitOfWork.Complete();
                response.ID = Offer.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<List<SelectDDL>> ProductFabricatorDDL()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDList = _unitOfWork.MaintenanceFors.FindAll(a => a.ProductFabricator != null).Select(a => a.ProductFabricator).Distinct().Select(a => new SelectDDL()
                {
                    ID = 0,
                    Name = a
                }).ToList();

                Response.Data = DDList;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<MaintenanceOfferDTO> GetSalesOfferOfMaintenance([FromHeader] long OfferId)
        {
            var Response = new BaseResponseWithData<MaintenanceOfferDTO>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (OfferId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Offer Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var offer = _unitOfWork.SalesOffers.FindAll(a => a.Id == OfferId && a.Active, includes: new[] { "Client", "SalesPerson", "CreatedByNavigation", "SalesOfferLocations", "Projects.InventoryMatrialRequestItems", "Projects.InventoryMatrialRequestItems.InventoryMatrialRequest", "VisitsScheduleOfMaintenances" }).FirstOrDefault();


                if (offer == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Offer not found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var Maintenances = _unitOfWork.MaintenanceFors.FindAll(a => a.SalesOfferId == OfferId).ToList();
                var MatrialRequestItem = offer.Projects.FirstOrDefault()?.InventoryMatrialRequestItems.LastOrDefault();

                var MaintenanceOffer = new MaintenanceOfferDTO()
                {
                    OfferId = offer.Id,
                    OfferSerial = offer.OfferSerial,
                    StartDate = offer.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = offer.EndDate != null ? ((DateOnly)offer.EndDate).ToString("yyyy-MM-dd") : null,
                    ClientId = offer.ClientId ?? 0,
                    ClientName = offer.Client?.Name,
                    OfferStatus = offer.Status,
                    WorkerOrder = offer.OfferSerial,
                    SalesPersonId = offer.SalesPersonId,
                    SalesPersonName = offer.SalesPerson?.FirstName + " " + offer.SalesPerson?.LastName,
                    ContactPersonName = offer.ContactPersonName,
                    ContactPersonMobile = offer.ContactPersonMobile,
                    ContactPersonEmail = offer.ContactPersonEmail,
                    ProjectLocation = offer.ProjectLocation,
                    ProductFabricator = offer.MaintenanceFors.Count() > 0 ? offer.MaintenanceFors.Select(a => a.ProductFabricator).FirstOrDefault() : null,
                    CreatorID = offer.CreatedBy,
                    CreatorName = offer.CreatedByNavigation.FirstName + " " + offer.CreatedByNavigation?.MiddleName + " " + offer.CreatedByNavigation.LastName,
                    LocationX = offer.SalesOfferLocations.FirstOrDefault()?.LocationX ?? 0,
                    LocationY = offer.SalesOfferLocations.FirstOrDefault()?.LocationY ?? 0,
                    ProjectId = offer.Projects.Select(a => a.Id).FirstOrDefault(),
                    MaterialRequestId = MatrialRequestItem?.InventoryMatrialRequestId,
                    MaterialRequestStatus = MatrialRequestItem?.InventoryMatrialRequest?.Status,
                    VisitsDates = offer.VisitsScheduleOfMaintenances.Select(a => a.VisitDate?.ToString("yyyy-MM-dd")).ToList(),
                    Maintenances = offer.MaintenanceFors.Count() > 0 ? offer.MaintenanceFors.Select(a => new MaintenanceOfOffer()
                    {
                        Id = a.Id,
                        ProductName = a.ProductName,
                        ProductBrand = a.ProductBrand,
                        ProductSerial = a.ProductSerial,
                        Problem = a.GeneralNote,
                        Active = true
                    }).ToList() : [],
                    SalesOfferProducts = _offerService.GetSalesOfferProductsList(OfferId)
                };

                Response.Data = MaintenanceOffer;
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

        public BaseResponseWithId<long> AddSalesOfferProductList(AddSalesOfferProductListForMainenance salesOfferProducts, long creator)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                #region validation
                var salesOffer = _unitOfWork.SalesOffers.GetById(salesOfferProducts.offerId);
                if (salesOffer == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "No SalesOffer With this ID";
                    response.Errors.Add(error);
                    return response;
                }

                var salesPerson = _unitOfWork.Users.GetById(salesOfferProducts.AssginTo);
                if (salesPerson == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "No user (Assign To) With this ID";
                    response.Errors.Add(error);
                    return response;
                }

                var products = salesOfferProducts.productList.Select(a => a.ProductId).ToList();
                var productsList = _unitOfWork.product.FindAll(a => products.Contains(a.Id));

                var productGroup = salesOfferProducts.productList.Select(a => a.ProductGroupId).ToList();
                var productsGroupList = _unitOfWork.productGroup.FindAll(a => productGroup.Contains(a.Id));

                var inventoryItem = salesOfferProducts.productList.Select(a => a.InventoryItemId).ToList();
                var inventoryItemList = _unitOfWork.InventoryItems.FindAll(a => inventoryItem.Contains(a.Id));

                var inventoryItemCategory = salesOfferProducts.productList.Select(a => a.InventoryItemCategoryId).ToList();
                var inventoryItemCategoryList = _unitOfWork.InventoryItemCategories.FindAll(a => inventoryItemCategory.Contains(a.Id));

                var InvoicePayerClient = salesOfferProducts.productList.Select(a => a.InvoicePayerClientId).ToList();
                var InvoicePayerClientIdList = _unitOfWork.Clients.FindAll(a => InvoicePayerClient.Contains(a.Id));

                var counter = 1;
                foreach (var product in salesOfferProducts.productList)
                {
                    if (product.ProductId != null)
                    {
                        var prod = productsList.Where(a => a.Id == product.ProductId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No product With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.ProductGroupId != null)
                    {
                        var prod = productsGroupList.Where(a => a.Id == product.ProductGroupId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No Product Group With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.InventoryItemId != null)
                    {
                        var prod = inventoryItemList.Where(a => a.Id == product.InventoryItemId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No inventory Item With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.InventoryItemCategoryId != null)
                    {
                        var prod = inventoryItemCategoryList.Where(a => a.Id == product.InventoryItemCategoryId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No Inventory Item Category Item With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.InvoicePayerClientId != null)
                    {
                        var prod = InvoicePayerClientIdList.Where(a => a.Id == product.InvoicePayerClientId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No Invoice Payer Client With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }
                }
                #endregion
                var listOfProducts = new List<SalesOfferProduct>();
                foreach (var SalesOfferProduct in salesOfferProducts.productList)
                {
                    var currentProduct = new SalesOfferProduct()
                    {
                        CreatedBy = creator,
                        ModifiedBy = creator,
                        CreationDate = DateTime.Now,
                        Modified = DateTime.Now,
                        Active = true,
                        ProductId = SalesOfferProduct.ProductId,
                        OfferId = salesOfferProducts.offerId,
                        ProductGroupId = SalesOfferProduct.ProductGroupId,
                        Quantity = SalesOfferProduct.Quantity,
                        InventoryItemId = SalesOfferProduct.InventoryItemId,
                        InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId,
                        ItemPrice = SalesOfferProduct.ItemPrice,
                        ItemPricingComment = SalesOfferProduct.ItemPricingComment,
                        InvoicePayerClientId = SalesOfferProduct.InvoicePayerClientId,
                        DiscountPercentage = SalesOfferProduct.DiscountPercentage,
                        DiscountValue = SalesOfferProduct.DiscountValue,
                        FinalPrice = SalesOfferProduct.FinalPrice,
                        TaxPercentage = SalesOfferProduct.TaxPercentage,
                        TaxValue = SalesOfferProduct.TaxValue,
                        ReturnedQty = 0,
                        RemainQty = SalesOfferProduct.Quantity,
                        ProfitPercentage = SalesOfferProduct.ProfitPercentage,
                        ReleasedQty = null
                    };

                    listOfProducts.Add(currentProduct);
                }

                var newProduct = _unitOfWork.SalesOfferProducts.AddRange(listOfProducts);
                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithId<long> EditSalesOfferProductList(EditSalesOfferProductListForMainenance salesOfferProducts, long creator)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                #region validation
                var salesOffer = _unitOfWork.SalesOffers.GetById(salesOfferProducts.offerId);
                if (salesOffer == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "No SalesOffer With this ID";
                    response.Errors.Add(error);
                    return response;
                }



                var products = salesOfferProducts.productList.Select(a => a.ProductId).ToList();
                var productsList = _unitOfWork.product.FindAll(a => products.Contains(a.Id));

                var productGroup = salesOfferProducts.productList.Select(a => a.ProductGroupId).ToList();
                var productsGroupList = _unitOfWork.productGroup.FindAll(a => productGroup.Contains(a.Id));

                var inventoryItem = salesOfferProducts.productList.Select(a => a.InventoryItemId).ToList();
                var inventoryItemList = _unitOfWork.InventoryItems.FindAll(a => inventoryItem.Contains(a.Id));

                var inventoryItemCategory = salesOfferProducts.productList.Select(a => a.InventoryItemCategoryId).ToList();
                var inventoryItemCategoryList = _unitOfWork.InventoryItemCategories.FindAll(a => inventoryItemCategory.Contains(a.Id));

                var InvoicePayerClient = salesOfferProducts.productList.Select(a => a.InvoicePayerClientId).ToList();
                var InvoicePayerClientIdList = _unitOfWork.Clients.FindAll(a => InvoicePayerClient.Contains(a.Id));


                var counter = 1;
                foreach (var product in salesOfferProducts.productList)
                {
                    if (product.ProductId != null)
                    {
                        var prod = productsList.Where(a => a.Id == product.ProductId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No product With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.ProductGroupId != null)
                    {
                        var prod = productsGroupList.Where(a => a.Id == product.ProductGroupId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No Product Group With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.InventoryItemId != null)
                    {
                        var prod = inventoryItemList.Where(a => a.Id == product.InventoryItemId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No inventory Item With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.InventoryItemCategoryId != null)
                    {
                        var prod = inventoryItemCategoryList.Where(a => a.Id == product.InventoryItemCategoryId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No Inventory Item Category Item With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }

                    if (product.InvoicePayerClientId != null)
                    {
                        var prod = InvoicePayerClientIdList.Where(a => a.Id == product.InvoicePayerClientId).FirstOrDefault();
                        if (prod == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = $"No Invoice Payer Client With this ID at product number {counter}";
                            response.Errors.Add(error);
                            return response;
                        }
                    }
                }
                #endregion

                var listOfProducts = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == salesOfferProducts.offerId);
                foreach (var SalesOfferProduct in salesOfferProducts.productList)
                {
                    var currentProduct = listOfProducts.Where(b => b.Id == SalesOfferProduct.SalesOfferProductId).FirstOrDefault();

                    if (currentProduct == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = $"No product with this ID {SalesOfferProduct.SalesOfferProductId}";
                        response.Errors.Add(error);
                        return response;
                    }

                    if (SalesOfferProduct.Active == true)
                    {
                        currentProduct.ModifiedBy = creator;
                        currentProduct.Modified = DateTime.Now;
                        currentProduct.Active = true;
                        currentProduct.ProductId = SalesOfferProduct.ProductId;
                        currentProduct.OfferId = salesOfferProducts.offerId;
                        currentProduct.ProductGroupId = SalesOfferProduct.ProductGroupId;
                        currentProduct.Quantity = SalesOfferProduct.Quantity;
                        currentProduct.InventoryItemId = SalesOfferProduct.InventoryItemId;
                        currentProduct.InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId;
                        currentProduct.ItemPrice = SalesOfferProduct.ItemPrice;
                        currentProduct.ItemPricingComment = SalesOfferProduct.ItemPricingComment;
                        currentProduct.DiscountPercentage = SalesOfferProduct.DiscountPercentage;
                        currentProduct.DiscountValue = SalesOfferProduct.DiscountValue;
                        currentProduct.FinalPrice = SalesOfferProduct.FinalPrice;
                        currentProduct.TaxPercentage = SalesOfferProduct.TaxPercentage;
                        currentProduct.TaxValue = SalesOfferProduct.TaxValue;
                        currentProduct.ReturnedQty = 0;
                        currentProduct.RemainQty = SalesOfferProduct.Quantity;
                        currentProduct.ProfitPercentage = SalesOfferProduct.ProfitPercentage;
                        currentProduct.ReleasedQty = null;

                    }
                    else
                    {
                        _unitOfWork.SalesOfferProducts.Delete(currentProduct);
                        _unitOfWork.Complete();
                    }





                }


                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }

        }

        public BaseResponseWithDataAndHeader<List<MaintenanceOfferCardDTO>> GetSalesOfferOfMaintenanceList(GetSalesOfferOfMaintenanceFilters filters)
        {
            var Response = new BaseResponseWithDataAndHeader<List<MaintenanceOfferCardDTO>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //#region date Validation
                var visitDate = DateTime.Now;
                if (!string.IsNullOrEmpty(filters.VisitDate))
                {
                    if (!DateTime.TryParse(filters.VisitDate, out visitDate))
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "please Enter a valid Reservation Date";
                        Response.Errors.Add(err);
                        return Response;
                    }
                }
                //#endregion

                var offers = _unitOfWork.SalesOffers.FindAllQueryable(a =>a.Active, includes: new[] { "MaintenanceFors", "Client", "SalesPerson", "SalesOfferLocations", "Projects.InventoryMatrialRequestItems", "Projects.InventoryMatrialRequestItems.InventoryMatrialRequest", "VisitsScheduleOfMaintenances", "CreatedByNavigation" });
                if (filters.DateFrom != null)
                {
                    DateTime dateTime = ((DateOnly)filters.DateFrom).ToDateTime(TimeOnly.MinValue); 
                    offers = offers.Where(x => x.CreationDate >= dateTime);
                }
                if (filters.DateTo != null)
                {
                    DateTime dateTime = ((DateOnly)filters.DateTo).ToDateTime(TimeOnly.MinValue); 
                    offers = offers.Where(x => x.CreationDate <= dateTime);
                }
                if (filters.clientId != 0)
                {
                    offers = offers.Where(x => x.ClientId == filters.clientId);
                }
                if (filters.salesPersonId  != 0)
                {
                    offers = offers.Where(x => x.SalesPersonId == filters.salesPersonId);
                }
                if (!string.IsNullOrWhiteSpace(filters.Status))
                {
                    offers = offers.Where(x => x.Status.ToLower().Contains(filters.Status.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(filters.Type))
                {
                    offers = offers.Where(x => x.OfferType.ToLower().Contains(filters.Type.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(filters.searchKey))
                {
                    var SearchKey = HttpUtility.UrlDecode(filters.searchKey);

                    offers = offers.Where(x => (string.IsNullOrWhiteSpace(x.ContactPersonName) ? x.ContactPersonName.Contains(SearchKey) : false) ||
                                               (string.IsNullOrWhiteSpace(x.ContactPersonMobile) ? x.ContactPersonMobile.Contains(SearchKey) : false));
                }
                if (!string.IsNullOrWhiteSpace(filters.workerOrder))
                {
                    offers = offers.Where(x => x.OfferSerial.Contains(filters.workerOrder));
                }
                if (!string.IsNullOrWhiteSpace(filters.equipmentName) || !string.IsNullOrWhiteSpace(filters.equipmentBrand) || !string.IsNullOrWhiteSpace(filters.vendor))
                {
                    var maintenance = _unitOfWork.MaintenanceFors.FindAll(a => a.ProductName == filters.equipmentName || a.ProductBrand == filters.equipmentBrand || a.ProductFabricator == filters.vendor);
                    var ListOffersIDS = maintenance.Select(x => x.SalesOfferId).ToList();
                    offers = offers.Where(x => ListOffersIDS.Contains(x.Id));
                }
                if (!string.IsNullOrEmpty(filters.VisitDate))
                {
                    //var visitsScheduleOfMaintenace = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.VisitDate == visitDate.Date);
                    //var offersIDList = visitsScheduleOfMaintenace.Select(a => a.OfferId).ToList();

                    //offers = offers.Where(a => offersIDList.Contains(a.Id));

                    offers = offers.Where(a => a.VisitsScheduleOfMaintenances.Any(b => b.VisitDate == visitDate.Date));
                }

                var offersList = PagedList<SalesOffer>.Create(offers, filters.CurrentPage, filters.NumberOfItemsPerPage);
                var returnedList = new List<MaintenanceOfferCardDTO>();

                foreach (var offer in offersList)
                {
                    var MatrialRequestItem = offer.Projects.FirstOrDefault()?.InventoryMatrialRequestItems.LastOrDefault() ;
                    var MaintenanceOffer = new MaintenanceOfferCardDTO()
                    {
                        OfferId = offer.Id,
                        OfferSerial = offer.OfferSerial,
                        StartDate = offer.StartDate.ToString("yyyy-MM-dd"),
                        EndDate = offer.EndDate != null ? ((DateOnly)offer.EndDate).ToString("yyyy-MM-dd") : null,
                        ClientName = offer.Client?.Name,
                        SalesPersonId = offer.SalesPersonId,
                        SalesPersonName = offer.SalesPerson?.FirstName + " " + offer.SalesPerson?.LastName,
                        SalesPersonPhoto = offer.SalesPerson.PhotoUrl!=null? Globals.baseURL+'/'+offer.SalesPerson.PhotoUrl:null,
                        ContactPersonMobile = offer.SalesPerson.Mobile,
                        ProductFabricator = offer.MaintenanceFors.Count() > 0 ? offer.MaintenanceFors.Select(a => a.ProductFabricator).FirstOrDefault() : null,
                        ProjectLocation = offer.ProjectLocation,
                        LocationX = offer.SalesOfferLocations.Select(a=>a.LocationX).FirstOrDefault(),
                        LocationY = offer.SalesOfferLocations.Select(a=>a.LocationY).FirstOrDefault(),
                        MaterialRequestId = MatrialRequestItem?.InventoryMatrialRequestId,
                        MaterialRequestStatus = MatrialRequestItem?.InventoryMatrialRequest?.Status,
                        VisitsDates = offer.VisitsScheduleOfMaintenances.Select(a => a.VisitDate?.ToString("yyyy-MM-dd")).ToList(),
                        CreatorID = offer.CreatedBy,
                        CreatorName = offer.CreatedByNavigation.FirstName + " " + (offer.CreatedByNavigation?.MiddleName!=null?offer.CreatedByNavigation?.MiddleName + " ":null) + offer.CreatedByNavigation.LastName,
                        CreatorImg = offer.CreatedByNavigation?.PhotoUrl != null ? Globals.baseURL + '/' + offer.CreatedByNavigation.PhotoUrl : null,
                    };
                    returnedList.Add(MaintenanceOffer);

                }
                ;

                Response.Data = returnedList;
                Response.PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = offersList.CurrentPage,
                    ItemsPerPage = offersList.PageSize,
                    TotalItems = offersList.TotalCount,
                    TotalPages = offersList.TotalPages
                };
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

        public BaseResponseWithData<List<string>> GetMaintenanceNameList()
        {
            var response = new BaseResponseWithData<List<string>>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                var MaintenanceForList = _unitOfWork.MaintenanceFors.GetAll();
                
                var NamesList = MaintenanceForList.Select(a => a.ProductName).ToList();

                response.Data = NamesList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<GetOfferStatusSummaryModdel> GetOfferStatusSummary(GetSalesOfferOfMaintenanceFilters filters)
        {
            var response = new BaseResponseWithData<GetOfferStatusSummaryModdel>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                #region filter
                //#region date Validation
                var visitDate = DateTime.Now;
                if (!string.IsNullOrEmpty(filters.VisitDate))
                {
                    if (!DateTime.TryParse(filters.VisitDate, out visitDate))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "please Enter a valid Reservation Date";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                //#endregion

                var offersQueryable = _unitOfWork.SalesOffers.FindAllQueryable(a =>a.Active, includes: new[] { "MaintenanceFors", "Client", "SalesPerson", "SalesOfferLocations", "Projects.InventoryMatrialRequestItems", "Projects.InventoryMatrialRequestItems.InventoryMatrialRequest", "VisitsScheduleOfMaintenances" });
                if (filters.DateFrom != null)
                {
                    DateTime dateTime = ((DateOnly)filters.DateFrom).ToDateTime(TimeOnly.MinValue); 
                    offersQueryable = offersQueryable.Where(x => x.CreationDate >= dateTime);
                }
                if (filters.DateTo != null)
                {
                    DateTime dateTime = ((DateOnly)filters.DateTo).ToDateTime(TimeOnly.MinValue); 
                    offersQueryable = offersQueryable.Where(x => x.CreationDate <= dateTime);
                }
                if (filters.clientId != 0)
                {
                    offersQueryable = offersQueryable.Where(x => x.ClientId == filters.clientId);
                }
                if (filters.salesPersonId  != 0)
                {
                    offersQueryable = offersQueryable.Where(x => x.SalesPersonId == filters.salesPersonId);
                }
                if (!string.IsNullOrWhiteSpace(filters.Status))
                {
                    offersQueryable = offersQueryable.Where(x => x.Status.ToLower().Contains(filters.Status.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(filters.Type))
                {
                    offersQueryable = offersQueryable.Where(x => x.OfferType.ToLower().Contains(filters.Type.ToLower()));
                }
                if (!string.IsNullOrWhiteSpace(filters.searchKey))
                {
                    var SearchKey = HttpUtility.UrlDecode(filters.searchKey);

                    offersQueryable = offersQueryable.Where(x => (string.IsNullOrWhiteSpace(x.ContactPersonName) ? x.ContactPersonName.Contains(SearchKey) : false) ||
                                               (string.IsNullOrWhiteSpace(x.ContactPersonMobile) ? x.ContactPersonMobile.Contains(SearchKey) : false));
                }
                if (!string.IsNullOrWhiteSpace(filters.workerOrder))
                {
                    offersQueryable = offersQueryable.Where(x => x.OfferSerial.Contains(filters.workerOrder));
                }
                if (!string.IsNullOrWhiteSpace(filters.equipmentName) || !string.IsNullOrWhiteSpace(filters.equipmentBrand) || !string.IsNullOrWhiteSpace(filters.vendor))
                {
                    var maintenance = _unitOfWork.MaintenanceFors.FindAll(a => a.ProductName == filters.equipmentName || a.ProductBrand == filters.equipmentBrand || a.ProductFabricator == filters.vendor);
                    var ListoffersQueryableIDS = maintenance.Select(x => x.SalesOfferId).ToList();
                    offersQueryable = offersQueryable.Where(x => ListoffersQueryableIDS.Contains(x.Id));
                }
                if (!string.IsNullOrEmpty(filters.VisitDate))
                {
                    //var visitsScheduleOfMaintenace = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.VisitDate == visitDate.Date);
                    //var offersQueryableIDList = visitsScheduleOfMaintenace.Select(a => a.OfferId).ToList();

                    //offersQueryable = offersQueryable.Where(a => offersQueryableIDList.Contains(a.Id));

                    offersQueryable = offersQueryable.Where(a => a.VisitsScheduleOfMaintenances.Any(b => b.VisitDate == visitDate.Date));
                }

                #endregion

                var data = new GetOfferStatusSummaryModdel();
                var offers = offersQueryable.ToList().GroupBy(a => a.Status).Select(
                    a => new OfferStatusCount()
                    {
                        Name = a.Key,
                        Count = a.Count()
                    }).ToList();

                data.OfferStatusCounts = offers;
                response.Data = data;
                return response;

            }
            catch(Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        } 

        public BaseResponseWithId<long> AddSalesOfferMaintenanceVisits(AddSalesOfferMaintenanceVisitsDTO dto, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                #region validation
                var offer = _unitOfWork.SalesOffers.GetById(dto.OfferID);
                if (offer == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "No SalesOffer With this ID";
                    response.Errors.Add(error);
                    return response;
                }

                var plannedDate = DateTime.Now;
                if (!DateTime.TryParse(dto.PlannedDate, out plannedDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "please Enter a valid Reservation Date";
                    response.Errors.Add(err);
                    return response;
                }

                var vistDate = DateTime.Now;
                if (!DateTime.TryParse(dto.VisitDate, out vistDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "please Enter a valid Reservation Date";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                var visitSchedule = new VisitsScheduleOfMaintenance()
                {
                    PlannedDate = plannedDate,
                    VisitDate = vistDate,
                    CreatedBy = userID,
                    CreationDate = DateTime.Now,
                    Status = true,
                    Active = true,
                    OfferId = dto.OfferID
                };

                var NewVisit = _unitOfWork.VisitsScheduleOfMaintenances.Add(visitSchedule);
                _unitOfWork.Complete();

                response.ID = NewVisit.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<List<string>> GetVisitsDatesOfScheduleOfMaintenance(long offerID)
        {
            var response = new BaseResponseWithData<List<string>>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                var visitsData = _unitOfWork.VisitsScheduleOfMaintenances.FindAll(a => a.OfferId == offerID);

                var visitsDatesList = visitsData.Select(a => a.VisitDate?.ToShortDateString()).ToList();

                response.Data = visitsDatesList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }
    }
}
