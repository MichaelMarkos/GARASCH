using AutoMapper;
using Azure;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.Client.ClientsCardsStatistics;
using NewGaras.Infrastructure.Models.Client.Filters;
using NewGaras.Infrastructure.Models.Common;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using ClientFax = NewGaras.Infrastructure.Entities.ClientFax;
using ClientMobile = NewGaras.Infrastructure.Entities.ClientMobile;

namespace NewGaras.Domain.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private GarasTestContext _Context;
        private readonly IUserService _userService;
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
        public ClientService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, GarasTestContext context, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _Context = context;
            _userService = userService;
        }

        public GetClientsCardsData GetClientsDDL(GetClientsFilters filters, long creator, string companyname)
        {
            GetClientsCardsData Response = new GetClientsCardsData();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {

                    if (Response.Result)
                    {
                        var ClientsDBQuery = _unitOfWork.Clients.FindAllQueryable(a => a.NeedApproval == 0).AsQueryable();
                        if (!Common.CheckUserRole(creator, 5, _Context)) //ViewAllClientsAllBranches
                        {
                            if (Common.CheckUserRole(creator, 6, _Context))
                            {
                                var UserBranchID = _Context.Users.Where(x => x.Id == creator).Select(x => x.BranchId).FirstOrDefault();
                                ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == creator).AsQueryable();
                                if (UserBranchID != null && UserBranchID != 0)
                                {
                                    ClientsDBQuery = ClientsDBQuery.Where(a => a.BranchId == UserBranchID).AsQueryable();
                                }
                            }
                            else
                            if (Common.CheckUserIsGroupUser((int)creator, new List<long>() { 4 }, _Context))
                            {
                                ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == creator).AsQueryable();
                            }
                        }
                        if (filters.SalesPersonId != 0)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        }
                        if (filters.BranchId != 0)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                        }
                        if (filters.IncludeOwner == false)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.OwnerCoProfile != true).AsQueryable();
                        }

                        var ClientsVehiclesList = new List<VehiclePerClient>();

                        if (!string.IsNullOrEmpty(filters.SearchedKey))
                        {
                            var SearchKey = HttpUtility.UrlDecode(filters.SearchedKey).ToLower();
                            var string_compare_prepare_function = Common.string_compare_prepare_function(SearchKey);
                            var SearchedClientsIds = ClientsDBQuery.Where(a => a.PreparedSearchName.ToLower().Contains(string_compare_prepare_function)
                            ||
                            a.Name.ToLower().Contains(SearchKey.ToLower())
                            ||
                                (a.EnglishName != null ? a.EnglishName.ToLower().Contains(SearchKey.ToLower()) : false)).Select(a => a.Id).ToList();
                            //if (headers["CompanyName"].ToString().ToLower() == "proauto")
                            //{
                            ClientsVehiclesList = _unitOfWork.VehiclePerClients.FindAll(a => (SearchedClientsIds.Contains(a.ClientId) || (a.Vin ?? "").Contains(SearchKey) || (a.PlatNumber ?? "").Contains(SearchKey)) && a.Active == true).ToList();
                            var VehiclesClientsIds = new List<long>();
                            if (ClientsVehiclesList != null && ClientsVehiclesList.Count > 0)
                            {
                                VehiclesClientsIds = ClientsVehiclesList.Select(a => a.ClientId).ToList();
                                SearchedClientsIds.AddRange(VehiclesClientsIds);
                            }
                            //}
                            SearchedClientsIds = SearchedClientsIds.Distinct().ToList();
                            if (SearchedClientsIds.Count() > 0)
                            {
                                ClientsDBQuery = ClientsDBQuery.Where(a => SearchedClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }



                        var coSr = _unitOfWork.CompanySerialIdentifications.FindAll(a => a.Type.ToLower() == "client").Select(a => a.Identification).FirstOrDefault();
                        var ClientsListDB = ClientsDBQuery.Take(50).ToList();
                        var ClientsList = ClientsListDB.Select(a => new ClientMainData()
                        {
                            Id = a.Id,
                            Name = a.Name,
                            LogoURL = a.LogoUrl != null ? Globals.baseURL + a.LogoUrl : null,
                            ClientSerialCounter = a.ClientSerialCounter,
                            ClientSerial = a.ClientSerialCounter != null && coSr != null ? coSr + a.ClientSerialCounter : null
                        }).ToList();
                        Response.ClientsData = ClientsList;
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

        public BaseResponseWithData<List<SelectDDL>> GetClientsDropDown()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.Data = _unitOfWork.Clients.GetAll().Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();
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

        public GetClientsCardsData GetClientsCardsDataResponse(GetClientsCardsFilters filters, long creator, string companyname)
        {
            GetClientsCardsData Response = new GetClientsCardsData();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {

                    var PurchasingProductsStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var PurchasingProductsEndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);

                    string PurchasingProductsIdsString = "";
                    List<long> PurchasingProductsIdsList = new List<long>();
                    if (!string.IsNullOrEmpty(filters.PurchasingProductsIdsString))
                    {
                        PurchasingProductsIdsString = HttpUtility.UrlDecode(filters.PurchasingProductsIdsString).ToLower();
                        if (!string.IsNullOrEmpty(PurchasingProductsIdsString))
                        {
                            try
                            {
                                PurchasingProductsIdsList = PurchasingProductsIdsString.Split(',').Select(long.Parse).ToList();

                                if (!string.IsNullOrEmpty(filters.PurchasingProductsStartDate))
                                {
                                    DateTime From = DateTime.Now;
                                    if (!DateTime.TryParse(filters.PurchasingProductsStartDate, out From))
                                    {
                                        Error error = new Error();
                                        error.ErrorCode = "Err-12";
                                        error.ErrorMSG = "Invalid Purchasing Products Start Date";
                                        Response.Errors.Add(error);
                                        Response.Result = false;
                                        return Response;
                                    }
                                    PurchasingProductsStartDate = From;

                                    if (!string.IsNullOrEmpty(filters.PurchasingProductsEndDate))
                                    {
                                        DateTime To = DateTime.Now;
                                        if (!DateTime.TryParse(filters.PurchasingProductsEndDate, out To))
                                        {
                                            Error error = new Error();
                                            error.ErrorCode = "Err-13";
                                            error.ErrorMSG = "Invalid Purchasing Products End Date";
                                            Response.Errors.Add(error);
                                            Response.Result = false;
                                        }
                                        PurchasingProductsEndDate = To;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(filters.PurchasingProductsEndDate))
                                    {
                                        Error error = new Error();
                                        error.ErrorCode = "Err-13";
                                        error.ErrorMSG = "You have to Enter Purchasing Products Start Date!";
                                        Response.Errors.Add(error);
                                        Response.Result = false;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err110";
                                error.ErrorMSG = "Purchasing products Ids not in correct format";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    var RegistrationDateFrom = DateTime.Now;
                    var RegistrationDateTo = DateTime.Now;
                    var RegistrationDateFilter = false;
                    if (!string.IsNullOrEmpty(filters.RegistrationDateFrom))
                    {
                        RegistrationDateFilter = true;
                        DateTime From = DateTime.Now;
                        if (!DateTime.TryParse(filters.RegistrationDateFrom, out From))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Registration Date From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        RegistrationDateFrom = From;

                        if (!string.IsNullOrEmpty(filters.RegistrationDateTo))
                        {
                            DateTime To = DateTime.Now;
                            if (!DateTime.TryParse(filters.RegistrationDateTo, out To))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Registration Date To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                            }
                            RegistrationDateTo = To;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filters.RegistrationDateTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "You have to Enter Registration Date From!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                        }
                    }

                    var DealsDateFrom = DateTime.Now;
                    var DealsDateTo = DateTime.Now;
                    var DealsDateFilter = false;
                    if (!string.IsNullOrEmpty(filters.DealsDateFrom))
                    {
                        DealsDateFilter = true;
                        DateTime From = DateTime.Now;
                        if (!DateTime.TryParse(filters.DealsDateFrom, out From))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Deals Date From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        DealsDateFrom = From;

                        if (!string.IsNullOrEmpty(filters.DealsDateTo))
                        {
                            DateTime To = DateTime.Now;
                            if (!DateTime.TryParse(filters.DealsDateTo, out To))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Deals Date To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                            }
                            DealsDateTo = To;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filters.DealsDateTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "You have to Enter Deals Date From!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                        }
                    }

                    /* int SpecialityId = 0;
                     if (!string.IsNullOrEmpty(headers["SpecialityId"]) && int.TryParse(headers["SpecialityId"], out SpecialityId))
                     {
                         int.TryParse(headers["SpecialityId"], out SpecialityId);
                     }*/

                    /*long SalesPersonId = 0;
                    if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                    {
                        long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    }*/
                    /*int BranchId = 0;
                    if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    {
                        int.TryParse(headers["BranchId"], out BranchId);
                    }*/

                    /*bool? IsExpired = null;
                    if (!string.IsNullOrEmpty(headers["IsExpired"]))
                    {
                        if (bool.Parse(headers["IsExpired"]) != true && bool.Parse(headers["IsExpired"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "IsExpired must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            IsExpired = bool.Parse(headers["IsExpired"]);
                        }
                    }*/

                    /*bool? WithOpenOffers = null;
                    if (!string.IsNullOrEmpty(headers["WithOpenOffers"]))
                    {
                        if (bool.Parse(headers["WithOpenOffers"]) != true && bool.Parse(headers["WithOpenOffers"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "With Open Offers must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            WithOpenOffers = bool.Parse(headers["WithOpenOffers"]);
                        }
                    }*/

                    /*bool? HasRFQ = null;
                    if (!string.IsNullOrEmpty(headers["HasRFQ"]))
                    {
                        if (bool.Parse(headers["HasRFQ"]) != true && bool.Parse(headers["HasRFQ"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "HasRFQ must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            HasRFQ = bool.Parse(headers["HasRFQ"]);
                        }
                    }*/

                    /*bool? WithOpenProjects = null;
                    if (!string.IsNullOrEmpty(headers["WithOpenProjects"]))
                    {
                        if (bool.Parse(headers["WithOpenProjects"]) != true && bool.Parse(headers["WithOpenProjects"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "With Open Projects must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            WithOpenProjects = bool.Parse(headers["WithOpenProjects"]);
                        }
                    }*/

                    /*bool? WithVolume = null;
                    if (!string.IsNullOrEmpty(headers["WithVolume"]))
                    {
                        if (bool.Parse(headers["WithVolume"]) != true && bool.Parse(headers["WithVolume"]) != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "With Volume must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            WithVolume = bool.Parse(headers["WithVolume"]);
                        }
                    }*/

                    /*int ApprovalStatus = -1;
                    if (!string.IsNullOrEmpty(headers["ApprovalStatus"]) && int.TryParse(headers["ApprovalStatus"], out ApprovalStatus))
                    {
                        int.TryParse(headers["ApprovalStatus"], out ApprovalStatus);
                    }*/

                    /*int ClientClassification = 0;
                    if (!string.IsNullOrEmpty(headers["ClientClassification"]) && int.TryParse(headers["ClientClassification"], out ClientClassification))
                    {
                        int.TryParse(headers["ClientClassification"], out ClientClassification);
                    }*/

                    /*int ExpirationPeriod = 0;
                    if (!string.IsNullOrEmpty(headers["ExpirationPeriod"]) && int.TryParse(headers["ExpirationPeriod"], out ExpirationPeriod))
                    {
                        int.TryParse(headers["ExpirationPeriod"], out ExpirationPeriod);
                    }*/
                    /*long AreaId = 0;
                    if (!string.IsNullOrWhiteSpace(headers["AreaId"]) && long.TryParse(headers["AreaId"], out AreaId))
                    {
                        long.TryParse(headers["AreaId"], out AreaId);
                    }*/
                    if (filters.GovernorateId != 0)
                    {
                        if (filters.CountryId != 0)
                        {

                            if (filters.CountryId < 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err110";
                                error.ErrorMSG = "Invalid CountryId!";
                                Response.Errors.Add(error);
                            }
                            else if (filters.GovernorateId < 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err110";
                                error.ErrorMSG = "Invalid GovernorateId!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                var CountryDB = _unitOfWork.Countries.GetById(filters.CountryId);
                                if (CountryDB != null)
                                {
                                    var GovernorateDB = _unitOfWork.Governorates.GetById(filters.GovernorateId);
                                    if (GovernorateDB != null)
                                    {
                                        if (GovernorateDB.CountryId != filters.CountryId)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err110";
                                            error.ErrorMSG = "Invalid Governorate For this Country!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err110";
                                        error.ErrorMSG = "This Governorate Doesn't Exist!";
                                        Response.Errors.Add(error);
                                    }

                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err110";
                                    error.ErrorMSG = "This Country Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "You must Enter CountryId of This City!";
                            Response.Errors.Add(error);
                        }
                    }
                    else if (filters.CountryId != 0)
                    {
                        if (filters.CountryId < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "Invalid CountryId!";
                            Response.Errors.Add(error);
                        }

                        var CountryDB = _unitOfWork.Countries.GetById(filters.CountryId);
                        if (CountryDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "This Country Doesn't Exist!";
                            Response.Errors.Add(error);
                        }
                    }

                    /*int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }*/

                    if (Response.Result)
                    {
                        bool Filtered = false;

                        List<long> MobileClientsIds = new List<long>();
                        if (!string.IsNullOrEmpty(filters.Mobile))
                        {
                            MobileClientsIds.AddRange(_unitOfWork.ClientMobiles.FindAll(a => a.Mobile.Contains(filters.Mobile)).Select(a => a.Id).ToList());
                            MobileClientsIds.AddRange(_unitOfWork.ClientContactPeople.FindAll(a => a.Mobile.Contains(filters.Mobile) && a.Active).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        MobileClientsIds = MobileClientsIds.Distinct().ToList();

                        List<long> PhoneClientsIds = new List<long>();
                        if (!string.IsNullOrEmpty(filters.Phone))
                        {
                            PhoneClientsIds.AddRange(_unitOfWork.VClientPhones.FindAll(a => a.Phone.Contains(filters.Phone)).Select(a => a.Id).ToList());
                            Filtered = true;
                        }
                        PhoneClientsIds = PhoneClientsIds.Distinct().ToList();

                        List<long> FaxClientsIds = new List<long>();
                        if (!string.IsNullOrEmpty(filters.Fax))
                        {
                            FaxClientsIds.AddRange(_unitOfWork.ClientFaxes.FindAll(a => a.Fax.Contains(filters.Fax)).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        FaxClientsIds = FaxClientsIds.Distinct().ToList();

                        List<long> SpecialityClientsIds = new List<long>();
                        if (filters.SpecialityId != 0)
                        {
                            SpecialityClientsIds.AddRange(_unitOfWork.VClientSpecialities.FindAll(a => a.SpecialityId == filters.SpecialityId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        SpecialityClientsIds = SpecialityClientsIds.Distinct().ToList();

                        List<long> AddressClientsIds = new List<long>();
                        if (filters.GovernorateId != 0)
                        {
                            AddressClientsIds.AddRange(_unitOfWork.VClientAddresses.FindAll(a => a.GovernorateId == filters.GovernorateId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        else if (filters.CountryId != 0)
                        {
                            AddressClientsIds.AddRange(_Context.VClientAddresses.Where(a => a.CountryId == filters.CountryId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        if (filters.AreaId != 0)
                        {
                            AddressClientsIds.AddRange(_Context.VClientAddresses.Where(a => a.AreaId == filters.AreaId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        AddressClientsIds = AddressClientsIds.Distinct().ToList();

                        var ClientsIds = new List<long>();
                        if (MobileClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = MobileClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(MobileClientsIds).ToList();
                        }

                        if (PhoneClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = PhoneClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(PhoneClientsIds).ToList();
                        }

                        if (FaxClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = FaxClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(FaxClientsIds).ToList();
                        }

                        if (SpecialityClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = SpecialityClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(SpecialityClientsIds).ToList();
                        }

                        if (AddressClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = AddressClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(AddressClientsIds).ToList();
                        }

                        ClientsIds = ClientsIds.Distinct().ToList();

                        var ClientsDBQuery = _unitOfWork.VClientUseers.FindAllQueryable(a => true);
                        if (ClientsIds.Count() > 0 || Filtered)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => ClientsIds.Contains(a.Id)).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ClientName))
                        {
                            var string_compare_prepare_function = Common.string_compare_prepare_function(filters.ClientName).ToLower();
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.PreparedSearchName.ToLower().Contains(string_compare_prepare_function) || a.EnglishName.ToLower().Contains(filters.ClientName.ToLower())).AsQueryable();
                            var SearchedClientsIds = ClientsDBQuery.Where(a => a.PreparedSearchName.ToLower().Contains(string_compare_prepare_function)
                            ||
                            a.Name.ToLower().Contains(filters.ClientName.ToLower())
                            ||
                            a.EnglishName.ToLower().Contains(filters.ClientName.ToLower())).Select(a => a.Id).ToList();
                            var VehiclesClientsIds = _unitOfWork.VVehicles.FindAll(a => SearchedClientsIds.Contains(a.ClientId) || (a.Vin ?? "").Contains(filters.ClientName) || (a.PlatNumber ?? "").Contains(filters.ClientName)).Select(a => a.ClientId).ToList();
                            if (VehiclesClientsIds.Count() > 0)
                            {
                                ClientsDBQuery = ClientsDBQuery.Where(a => SearchedClientsIds.Contains(a.Id) || VehiclesClientsIds.Contains(a.Id));
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.SupportedBy))
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.SupportedBy.ToLower() == filters.SupportedBy).AsQueryable();
                        }

                        if (!Common.CheckUserRole(creator, 5, _Context)) //ViewAllClientsAllBranches
                        {
                            if (Common.CheckUserRole(creator, 6, _Context)) //ViewAllClientsMyBranch
                            {
                                var UserBranchID = _Context.Users.Where(x => x.Id == creator).Select(x => x.BranchId).FirstOrDefault();
                                ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == creator).AsQueryable();
                                if (UserBranchID != null && UserBranchID != 0)
                                {
                                    ClientsDBQuery = ClientsDBQuery.Where(a => a.BranchId == UserBranchID).AsQueryable();
                                }
                            }
                            else
                            if (Common.CheckUserIsGroupUser((int)creator, new List<long>() { 4 }, _Context)) // Group Sales Person
                            {
                                ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == creator).AsQueryable();
                            }
                        }
                        if (filters.SalesPersonId != 0)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        }
                        if (filters.BranchId != 0)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                        }
                        if (filters.ApprovalStatus != -1)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.NeedApproval == filters.ApprovalStatus).AsQueryable();
                        }
                        if (filters.ClientClassification != 0)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.ClientClassificationId == filters.ClientClassification).AsQueryable();
                        }

                        if (filters.IsExpired != null)
                        {
                            if (filters.IsExpired == true)
                            {
                                //var ExpiredClientsIds = _Context.V_Client_Useer.Where(a => a.ClientExpireDate < DateTime.Now).Select(a => a.ID).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => a.ClientExpireDate < DateTime.Now).AsQueryable();
                            }
                        }
                        if (filters.WithOpenOffers != null)
                        {
                            if (filters.WithOpenOffers == true && DealsDateFilter == true)
                            {
                                var WithOpenOffersClientsIds = _unitOfWork.SalesOffers.FindAll(a => a.Status.ToLower() != "closed" && a.Status.ToLower() != "rejected" && (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => WithOpenOffersClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (filters.HasRFQ != null)
                        {
                            if (filters.HasRFQ == true && DealsDateFilter == true)
                            {
                                var HasRFQClientsIds = _unitOfWork.SalesOffers.FindAll(a => (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).Distinct().ToList();

                                ClientsDBQuery = ClientsDBQuery.Where(a => HasRFQClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (filters.WithOpenProjects != null)
                        {
                            if (filters.WithOpenProjects == true && DealsDateFilter == true)
                            {
                                var WithOpenProjctsClientsIds = _unitOfWork.VProjectSalesOffers.FindAll(a => a.Closed == false && (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => WithOpenProjctsClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (filters.WithVolume != null)
                        {
                            if (filters.WithVolume == true && DealsDateFilter == true)
                            {
                                var WithVolumeClientsIds = _unitOfWork.SalesOffers.FindAll(a => a.Status.ToLower() == "closed" && (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => WithVolumeClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (PurchasingProductsIdsList != null && PurchasingProductsIdsList.Count > 0)
                        {
                            var ClientsWithPurchasingProductsIds = _Context.VSalesOfferProductSalesOffers.Where(a => PurchasingProductsIdsList.Contains(a.InventoryItemId ?? 0) && a.Status.ToLower() == "closed" && a.ClientApprovalDate >= PurchasingProductsStartDate && a.ClientApprovalDate <= PurchasingProductsEndDate).Select(a => a.ClientId).Distinct().ToList();
                            ClientsDBQuery = ClientsDBQuery.Where(a => ClientsWithPurchasingProductsIds.Contains(a.Id)).AsQueryable();
                        }
                        if (filters.ExpirationPeriod != 0)
                        {
                            var ExpiredClientsIds = _unitOfWork.VClientExpiredes.FindAll(a => a.UnReportedDays - filters.ExpirationPeriod <= 0 || a.LastOfferDiffDays - filters.ExpirationPeriod <= 0).Select(a => a.Id).ToList();
                            ClientsDBQuery = ClientsDBQuery.Where(a => ExpiredClientsIds.Contains(a.Id)).AsQueryable();
                        }
                        if (RegistrationDateFilter)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.CreationDate >= RegistrationDateFrom && a.CreationDate <= RegistrationDateTo).AsQueryable();
                        }

                        ClientsDBQuery = ClientsDBQuery.OrderBy(a => a.Name);
                        var ClientsListDB = PagedList<VClientUseer>.Create(ClientsDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);
                        //asd = SuppliersDBQuery.ToList();
                        var ClientResponse = new List<ClientMainData>();
                        var clientslogos = _unitOfWork.Clients.FindAll(a => ClientsListDB.Select(b => b.Id).Contains(a.Id)).ToList();
                        foreach (var Client in ClientsListDB)
                        {
                            var logo = clientslogos.Where(a => a.Id == Client.Id).FirstOrDefault();
                            var salesPersonName = "";
                            string salesPersonLogo = null;
                            var salesPerson = _unitOfWork.Users.GetById(Client.SalesPersonId);
                            if (salesPerson != null)
                            {
                                salesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                            }
                            if (salesPerson.PhotoUrl != null)
                            {
                                salesPersonLogo = Globals.baseURL + salesPerson.PhotoUrl;
                            }
                            var ClientStatus = "";
                            if (Client.NeedApproval != null)
                            {
                                switch (Client.NeedApproval)
                                {
                                    case 0:
                                        ClientStatus = "Approved";
                                        break;
                                    case 1:
                                        ClientStatus = "Waiting";
                                        break;
                                    case 2:
                                        ClientStatus = "Rejected";
                                        break;
                                    default:
                                        break;
                                }
                            }

                            var ClientOpenProjects = _unitOfWork.VProjectSalesOffers.FindAll(a => a.ClientId == Client.Id && a.Closed == false).ToList();

                            decimal totalProjectsRemain = 0;
                            foreach (var project in ClientOpenProjects)
                            {
                                var projectCost = project.FinalOfferPrice + project.ExtraCost ?? 0;

                                decimal totalProjectCollectedCost = 0;
                                var clientAccounts = _unitOfWork.ClientAccounts.FindAll(x => x.ClientId == project.Id);
                                foreach (var clientAccount in clientAccounts)
                                {
                                    if (clientAccount.AmountSign == "plus")
                                    {
                                        totalProjectCollectedCost += clientAccount.Amount;
                                    }
                                    else if (clientAccount.AmountSign == "minus")
                                    {
                                        totalProjectCollectedCost -= clientAccount.Amount;
                                    }
                                }

                                var projectRemainCost = projectCost - totalProjectCollectedCost;
                                totalProjectsRemain += projectRemainCost;
                            }

                            var totalClientVolume = _unitOfWork.VProjectSalesOffers.FindAll(a => a.ClientId == Client.Id).Sum(a => a.ExtraCost ?? 0 + a.FinalOfferPrice);
                            var ClientDataObj = new ClientMainData()
                            {
                                Id = Client.Id,
                                IdEnc = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(Client.Id.ToString(), key)),
                                Name = Client.Name,
                                EnglishName = Client.EnglishName,
                                SalesPersonID = Client.SalesPersonId,
                                SalesPersonName = salesPersonName,
                                ClientSerialCounter = Client.ClientSerialCounter,
                                SalesPersonLogo = salesPersonLogo,
                                Status = ClientStatus,
                                RegistrationDate = Client.CreationDate.ToShortDateString(),
                                ExpirationDate = Client.ClientExpireDate!=null? Client.ClientExpireDate?.ToShortDateString():null,
                                IsExpired = Client.ClientExpireDate != null? (Client.ClientExpireDate < DateTime.Now):false,
                                RemainingDays = Client.ClientExpireDate != null?((int)((DateTime)Client.ClientExpireDate - DateTime.Now).TotalDays):0,
                                RemainCollection = totalProjectsRemain,
                                Volume = totalClientVolume ?? 0,
                                ClassificationId = Client.ClientClassificationId,
                                ClassificationName = Client.ClientClassificationId != null ? _Context.ClientClassifications.Where(a => a.Id == Client.ClientClassificationId).Select(a => a.Name).FirstOrDefault() : null
                            };
                            if (logo.LogoUrl != null)
                            {
                                ClientDataObj.LogoURL = Globals.baseURL + logo.LogoUrl;
                            }
                            // Not Used yet  ... can use this api GetClientVehiclesDataResponse
                            //var ClientVehiclesDBList = _Context.V_Vehicle.Where(a => a.ClientId == Client.ID && a.Active == true).ToList();
                            //List<Vehicle> ClientVehiclesResponse = new List<Vehicle>();
                            //foreach (var vehicle in ClientVehiclesDBList)
                            //{
                            //    Vehicle clientVehicle = new Vehicle
                            //    {
                            //        Id = vehicle.ID,
                            //        VIN = vehicle.VIN,
                            //        PlateNumber = vehicle.PlatNumber
                            //    };
                            //    ClientVehiclesResponse.Add(clientVehicle);
                            //}
                            //ClientDataObj.ClientVehicles = ClientVehiclesResponse;

                            ClientResponse.Add(ClientDataObj);

                        }

                        Response.ClientsData = ClientResponse;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = ClientsListDB.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = ClientsListDB.TotalCount
                        };

                        /*var context = WebOperationContext.Current;
                        context.AddPaginationHeader(CurrentPage, ClientsListDB.TotalPages, NumberOfItemsPerPage, ClientsListDB.TotalCount);*/
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

        public CountriesGovernoratesAreasDDLs GetCountriesGovernoratesAreasDDLs(string CompanyName)
        {
            CountriesGovernoratesAreasDDLs Response = new CountriesGovernoratesAreasDDLs();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    var CountriesList = _unitOfWork.Countries.FindAll(x => x.Active == true).ToList();
                    if (CompanyName == "marinapltq")
                    {
                        CountriesList = CountriesList.Where(x => x.Id == 2).ToList();
                    }
                    var CountriesDDL = new List<SelectDDL>();
                    foreach (var C in CountriesList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = C.Id;
                        DDLObj.Name = C.Name;

                        CountriesDDL.Add(DDLObj);
                    }
                    Response.CountriesDDL = CountriesDDL;

                    var GovernoratesList = _unitOfWork.Governorates.FindAll(x => x.Active == true).ToList();
                    var GovernoratesDDL = new List<GovernorateDDL>();
                    foreach (var G in GovernoratesList)
                    {
                        var DDLObj = new GovernorateDDL();
                        DDLObj.ID = G.Id;
                        DDLObj.Name = G.Name;
                        DDLObj.CountryId = G.CountryId;

                        GovernoratesDDL.Add(DDLObj);
                    }
                    Response.GoverneratesDDL = GovernoratesDDL;

                    var AreasList = _unitOfWork.Areas.GetAll();
                    var AreasDDL = new List<AreaDDL>();
                    foreach (var A in AreasList)
                    {
                        var DDLObj = new AreaDDL();
                        DDLObj.ID = A.Id;
                        DDLObj.Name = A.Name;
                        DDLObj.DistrictID = A.DistrictId;

                        AreasDDL.Add(DDLObj);
                    }
                    Response.AreasDDL = AreasDDL;

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

        public SpecialitiesDDLResponse GetSpecialitiesDDL()
        {
            SpecialitiesDDLResponse Response = new SpecialitiesDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var SpecialitiesList = _unitOfWork.Specialities.GetAll();
                    var SpecialitiesDDL = new List<SelectDDL>();
                    foreach (var S in SpecialitiesList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = S.Id;
                        DDLObj.Name = S.Name;

                        SpecialitiesDDL.Add(DDLObj);
                    }
                    Response.SpecialitiesDDL = SpecialitiesDDL;
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

        public JobTitlesDDLResponse GetJobTitlesDDL()
        {
            JobTitlesDDLResponse Response = new JobTitlesDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var JobTitlesList = _unitOfWork.JobTitles.GetAll();
                    var JobTitlesDDL = new List<SelectDDL>();
                    foreach (var J in JobTitlesList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = J.Id;
                        DDLObj.Name = J.Name;

                        JobTitlesDDL.Add(DDLObj);
                    }
                    Response.JobTitlesDDL = JobTitlesDDL;

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



        public async Task<List<SelectDDL>> GetCurrenciesList(string CompanyName)
        {

            var CurrenciesList = await _unitOfWork.Currencies.GetAllAsync();
            if (CompanyName == "marinapltq")
            {
                CurrenciesList = CurrenciesList.Where(x => x.Id == 5).ToList();
            }
            var CurrenciesDDL = new List<SelectDDL>();
            foreach (var C in CurrenciesList)
            {
                var DDLObj = new SelectDDL();
                DDLObj.ID = C.Id;
                DDLObj.Name = C.Name;

                CurrenciesDDL.Add(DDLObj);
            }

            return CurrenciesDDL;
        }

        public DeliveryAnshippingMethodsDDLResponse GetDeliveryAnshippingMethodsDDL()
        {
            DeliveryAnshippingMethodsDDLResponse Response = new DeliveryAnshippingMethodsDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var DeliveryAnshippingMethodsList = _unitOfWork.DeliveryAndShippingMethods.GetAll();
                    var DeliveryAnshippingMethodsDDL = new List<SelectDDL>();
                    foreach (var D in DeliveryAnshippingMethodsList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = D.Id;
                        DDLObj.Name = D.Name;

                        DeliveryAnshippingMethodsDDL.Add(DDLObj);
                    }
                    Response.DeliveryAnshippingMethodsDDL = DeliveryAnshippingMethodsDDL;

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

        public AddNewClientScreenData GetClientScreenData(string CompanyName, int BranchId, int RoleId, long GroupId, string JobTitleId)
        {
            AddNewClientScreenData Response = new AddNewClientScreenData();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    var CountriesGovernoratesAreas = GetCountriesGovernoratesAreasDDLs(CompanyName);

                    Response.CountriesDDL = CountriesGovernoratesAreas.CountriesDDL;
                    Response.GovernorateDDL = CountriesGovernoratesAreas.GoverneratesDDL;
                    Response.AreasDDL = CountriesGovernoratesAreas.AreasDDL;

                    Response.SpecialitiesDDL = GetSpecialitiesDDL().SpecialitiesDDL;

                    Response.JobTitlesDDL = GetJobTitlesDDL().JobTitlesDDL;

                    Response.SalesPersonsDDL = _userService.GetUserList(BranchId, RoleId, GroupId, JobTitleId).DDLList;

                    Response.CurrenciesDDL = GetCurrenciesList(CompanyName).Result;

                    Response.DeliveryAndShippingMethodsDDL = GetDeliveryAnshippingMethodsDDL().DeliveryAnshippingMethodsDDL;

                    var SupportedByListDB = _Context.SupportedBies.Select(a => a.Name).ToList();

                    Response.SupportedByDDL = SupportedByListDB;
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


        public BaseResponseWithId<long> AddNewClientContacts(ClientContactsData Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                        var client = _unitOfWork.Clients.GetById(ClientId);
                        if (client == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        //Add-Edit supplier Addresses
                        if (Request.ClientAddresses != null)
                        {
                            if (Request.ClientAddresses.Count > 0)
                            {
                                var counter = 0;
                                foreach (var Adrs in Request.ClientAddresses)
                                {
                                    counter++;
                                    if (Adrs.Active != false)
                                    {

                                        int CountryId = 0;
                                        if (Adrs.CountryID != 0)
                                        {
                                            CountryId = Adrs.CountryID;
                                            var country = _unitOfWork.Countries.GetById(CountryId);
                                            if (country == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This Country Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Country ID Is Mandatory";
                                            Response.Errors.Add(error);
                                        }

                                        int GovernorateId = 0;
                                        if (Adrs.GovernorateID != 0)
                                        {
                                            GovernorateId = Adrs.GovernorateID;
                                            var governorate = _unitOfWork.Governorates.GetById(GovernorateId);
                                            if (governorate == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This City Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Governorate ID Is Mandatory";
                                            Response.Errors.Add(error);
                                        }

                                        long? AreaId = 0;
                                        if (Adrs.AreaID != null)
                                        {
                                            if (Adrs.AreaID != 0)
                                            {
                                                AreaId = Adrs.AreaID;
                                                var area = _unitOfWork.Areas.GetById((int)AreaId);
                                                if (area == null)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "ErrCRM1";
                                                    error.ErrorMSG = "This Area Doesn't Exist!!";
                                                    Response.Errors.Add(error);
                                                }
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Line: " + counter + ", " + "Address Area ID Is Mandatory";
                                                Response.Errors.Add(error);
                                            }
                                        }


                                        string Address = null;
                                        if (!string.IsNullOrEmpty(Adrs.Address))
                                        {
                                            Address = Adrs.Address;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                            if (Response.Result)
                            {
                                foreach (var Adrs in Request.ClientAddresses)
                                {
                                    int BuildingNumber = 0;
                                    int Floor = 0;
                                    if (!string.IsNullOrEmpty(Adrs.BuildingNumber) && int.TryParse(Adrs.BuildingNumber, out BuildingNumber))
                                    {
                                        int.TryParse(Adrs.BuildingNumber, out BuildingNumber);
                                    }
                                    if (!string.IsNullOrEmpty(Adrs.Floor) && int.TryParse(Adrs.Floor, out Floor))
                                    {
                                        int.TryParse(Adrs.Floor, out Floor);
                                    }
                                    if (Adrs.ID != null && Adrs.ID != 0)
                                    {
                                        var AddressDb = _unitOfWork.ClientAddresses.Find(x => x.Id == Adrs.ID);
                                        if (AddressDb != null)
                                        {
                                            if (Adrs.Active == false)
                                            {
                                                _unitOfWork.ClientAddresses.Delete(AddressDb);
                                            }
                                            else
                                            {
                                                var ClientAddressUpdate = _unitOfWork.ClientAddresses.GetById(Adrs.ID ?? 0);
                                                if (ClientAddressUpdate != null)
                                                {
                                                    ClientAddressUpdate.ClientId = Request.ClientId;
                                                    ClientAddressUpdate.CountryId = Adrs.CountryID;
                                                    ClientAddressUpdate.GovernorateId = Adrs.GovernorateID;
                                                    ClientAddressUpdate.Address = Adrs.Address;
                                                    ClientAddressUpdate.ModifiedBy = validation.userID;
                                                    ClientAddressUpdate.Modified = DateTime.Now;
                                                    ClientAddressUpdate.Active = true;
                                                    ClientAddressUpdate.BuildingNumber = BuildingNumber.ToString();
                                                    ClientAddressUpdate.Floor = Floor.ToString();
                                                    ClientAddressUpdate.Description = Adrs.Description;
                                                    ClientAddressUpdate.AreaId = Adrs.AreaID;
                                                    ClientAddressUpdate.Longtitud = Adrs.longtitud;
                                                    ClientAddressUpdate.Latitude = Adrs.latitude;
                                                }

                                            }
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Address Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        var ClientInsert = new Infrastructure.Entities.ClientAddress();
                                        ClientInsert.ClientId = Request.ClientId;
                                        ClientInsert.CountryId = Adrs.CountryID;
                                        ClientInsert.GovernorateId = Adrs.GovernorateID;
                                        ClientInsert.Address = Adrs.Address;
                                        ClientInsert.CreatedBy = validation.userID;
                                        ClientInsert.CreationDate = DateTime.Now;
                                        ClientInsert.Active = true;
                                        ClientInsert.BuildingNumber = BuildingNumber.ToString();
                                        ClientInsert.Floor = Floor.ToString();
                                        ClientInsert.Description = Adrs.Description;
                                        ClientInsert.AreaId = Adrs.AreaID;

                                        ClientInsert.Longtitud = Adrs.longtitud;
                                        ClientInsert.Latitude = Adrs.longtitud;

                                        _unitOfWork.ClientAddresses.Add(ClientInsert);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                        }

                        //Add/Edit supplier Land Lines
                        if (Request.ClientLandLines != null)
                        {
                            if (Request.ClientLandLines.Count > 0)
                            {
                                var counter = 0;
                                foreach (var LND in Request.ClientLandLines)
                                {
                                    counter++;
                                    string LandLine = null;
                                    if (!string.IsNullOrEmpty(LND.LandLine))
                                    {
                                        var ExistPhone = _unitOfWork.ClientPhones.Find(a => a.Phone == LND.LandLine);
                                        if (ExistPhone != null)
                                        {
                                            if (LND.ID == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Line: " + counter + ", " + "Land Line Is Exist Before";
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                LandLine = LND.LandLine;
                                            }
                                        }
                                        else
                                        {
                                            LandLine = LND.LandLine;
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Land Line Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                            if (Response.Result)
                            {
                                foreach (var LND in Request.ClientLandLines)
                                {
                                    if (LND.ID != null && LND.ID != 0)
                                    {
                                        var ClientLandLineDb = _unitOfWork.ClientPhones.GetById(LND.ID ?? 0);
                                        if (ClientLandLineDb != null)
                                        {
                                            if (LND.Active == false)
                                            {
                                                _unitOfWork.ClientPhones.Delete(ClientLandLineDb);
                                            }
                                            else
                                            {
                                                ClientLandLineDb.ClientId = Request.ClientId;
                                                ClientLandLineDb.Phone = LND.LandLine;
                                                ClientLandLineDb.ModifiedBy = validation.userID;
                                                ClientLandLineDb.Modified = DateTime.Now;
                                                ClientLandLineDb.Active = true;
                                            }
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Land Line Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        var ClientLandLineInsert = new ClientPhone();
                                        ClientLandLineInsert.ClientId = Request.ClientId;
                                        ClientLandLineInsert.Phone = LND.LandLine;
                                        ClientLandLineInsert.CreatedBy = validation.userID;
                                        ClientLandLineInsert.CreationDate = DateTime.Now;
                                        ClientLandLineInsert.Active = true;

                                        _unitOfWork.ClientPhones.Add(ClientLandLineInsert);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                        }

                        //Add-Edit supplier Mobiles
                        if (Request.ClientMobiles != null)
                        {
                            if (Request.ClientMobiles.Count > 0)
                            {
                                var counter = 0;
                                foreach (var MOB in Request.ClientMobiles)
                                {
                                    counter++;

                                    if (MOB.Active != false)
                                    {
                                        string Mobile = null;
                                        if (!string.IsNullOrEmpty(MOB.Mobile))
                                        {
                                            var ExistMobile = _unitOfWork.ClientPhones.Find(a => a.Phone == MOB.Mobile);
                                            if (ExistMobile != null)
                                            {
                                                if (MOB.ID == null)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "Line: " + counter + ", " + "Mobile Is Exist Before";
                                                    Response.Errors.Add(error);
                                                }
                                                else
                                                {
                                                    Mobile = MOB.Mobile;
                                                }
                                            }
                                            else
                                            {
                                                Mobile = MOB.Mobile;
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Mobile Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                            if (Response.Result)
                            {
                                foreach (var MOB in Request.ClientMobiles)
                                {
                                    if (MOB.ID != null && MOB.ID != 0)
                                    {
                                        var ClientMobileDb = _unitOfWork.ClientMobiles.Find(x => x.Id == MOB.ID);
                                        if (ClientMobileDb != null)
                                        {
                                            if (MOB.Active == false) //Delete 
                                            {
                                                _unitOfWork.ClientMobiles.Delete(ClientMobileDb);
                                            }
                                            else
                                            {
                                                var ClientMobileUpdate = _unitOfWork.ClientMobiles.GetById(MOB.ID ?? 0);

                                                ClientMobileUpdate.ClientId = Request.ClientId;
                                                ClientMobileUpdate.Mobile = MOB.Mobile;
                                                ClientMobileUpdate.ModifiedBy = validation.userID;
                                                ClientMobileUpdate.Modified = DateTime.Now;
                                                ClientMobileUpdate.Active = true;

                                            }
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Mobile Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        var ClientMobileInsert = new ClientMobile();
                                        ClientMobileInsert.ClientId = Request.ClientId;
                                        ClientMobileInsert.Mobile = MOB.Mobile;
                                        ClientMobileInsert.CreatedBy = validation.userID;
                                        ClientMobileInsert.CreationDate = DateTime.Now;
                                        ClientMobileInsert.ModifiedBy = validation.userID;
                                        ClientMobileInsert.Modified = DateTime.Now;
                                        ClientMobileInsert.Active = true;

                                        _unitOfWork.ClientMobiles.Add(ClientMobileInsert);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                        }

                        //Add-Edit supplier Faxes
                        if (Request.ClientFaxes != null)
                        {
                            if (Request.ClientFaxes.Count > 0)
                            {
                                var counter = 0;
                                foreach (var FX in Request.ClientFaxes)
                                {
                                    counter++;

                                    if (FX.Active != false)
                                    {
                                        string Fax = null;
                                        if (!string.IsNullOrEmpty(FX.Fax))
                                        {
                                            var ExistFax = _unitOfWork.ClientFaxes.Find(a => a.Fax == FX.Fax);
                                            if (ExistFax != null)
                                            {
                                                if (FX.ID == null)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "Line: " + counter + ", " + "Fax Is Exist Before";
                                                    Response.Errors.Add(error);
                                                }
                                                else
                                                {
                                                    Fax = FX.Fax;
                                                }
                                            }
                                            else
                                            {
                                                Fax = FX.Fax;
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Fax Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                            if (Response.Result)
                            {
                                foreach (var FX in Request.ClientFaxes)
                                {
                                    if (FX.ID != null && FX.ID != 0)
                                    {
                                        var ClientFaxDb = _unitOfWork.ClientFaxes.Find(x => x.Id == FX.ID);
                                        if (ClientFaxDb != null)
                                        {
                                            if (FX.Active == false) //Delete 
                                            {
                                                _unitOfWork.ClientFaxes.Delete(ClientFaxDb);
                                            }
                                            else
                                            {
                                                var ClientMobileUpdate = _unitOfWork.ClientFaxes.GetById(FX.ID ?? 0);

                                                ClientMobileUpdate.ClientId = Request.ClientId;
                                                ClientMobileUpdate.Fax = FX.Fax;
                                                ClientMobileUpdate.ModifiedBy = validation.userID;
                                                ClientMobileUpdate.Modified = DateTime.Now;
                                                ClientMobileUpdate.Active = true;

                                            }
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Fax Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        var ClientFaxInsert = new ClientFax();
                                        ClientFaxInsert.ClientId = Request.ClientId;
                                        ClientFaxInsert.Fax = FX.Fax;
                                        ClientFaxInsert.CreatedBy = validation.userID;
                                        ClientFaxInsert.CreationDate = DateTime.Now;
                                        ClientFaxInsert.ModifiedBy = validation.userID;
                                        ClientFaxInsert.Modified = DateTime.Now;
                                        ClientFaxInsert.Active = true;

                                        _unitOfWork.ClientFaxes.Add(ClientFaxInsert);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                        }

                        _unitOfWork.Complete();
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


        public BaseResponseWithID AddNewClientContactPerson(ClientContactPersonData Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                        var client = _unitOfWork.Clients.GetById(ClientId);
                        if (client == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        if (Request.ClientContactPersons.Count > 0)
                        {
                            var counter = 0;
                            foreach (var CP in Request.ClientContactPersons)
                            {
                                counter++;

                                if (CP.Active != false)
                                {
                                    string Name = null;
                                    if (!string.IsNullOrEmpty(CP.Name))
                                    {
                                        Name = CP.Name;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Name Is Mandatory";
                                        Response.Errors.Add(error);
                                    }

                                    string Title = null;
                                    if (!string.IsNullOrEmpty(CP.Title))
                                    {
                                        Title = CP.Title;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Title Is Mandatory";
                                        Response.Errors.Add(error);
                                    }

                                    string Mobile = null;
                                    if (!string.IsNullOrEmpty(CP.Mobile))
                                    {
                                        Mobile = CP.Mobile;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Mobile Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }

                        if (Response.Result)
                        {
                            foreach (var CP in Request.ClientContactPersons)
                            {
                                if (CP.ID != null && CP.ID != 0) // Delete Or Update
                                {
                                    var ClientContactPersonDb = _unitOfWork.ClientContactPersons.GetById(CP.ID ?? 0);
                                    if (ClientContactPersonDb != null)
                                    {
                                        if (CP.Active == false) //Delete
                                        {
                                            _unitOfWork.ClientContactPersons.Delete(ClientContactPersonDb);
                                        }
                                        else
                                        {

                                            // Update
                                            //var ClientContactPersonUpdate = _Context.proc_ClientContactPersonUpdate(CP.ID,
                                            //                                                                        Request.ClientId,
                                            //                                                                        ClientContactPersonDb.CreatedBy,
                                            //                                                                        ClientContactPersonDb.CreationDate,
                                            //                                                                        long.Parse(Encrypt_Decrypt.Decrypt(CP.ModifiedBy, key)),
                                            //                                                                        DateTime.Now,
                                            //                                                                        true,
                                            //                                                                        CP.Name,
                                            //                                                                        CP.Title,
                                            //                                                                        CP.Location,
                                            //                                                                        CP.Email,
                                            //                                                                        CP.Mobile
                                            //                                                                        );

                                            ClientContactPersonDb.ClientId = Request.ClientId;
                                            ClientContactPersonDb.ModifiedBy = validation.userID;
                                            ClientContactPersonDb.Modified = DateTime.Now;
                                            ClientContactPersonDb.Active = true;
                                            ClientContactPersonDb.Name = CP.Name;
                                            ClientContactPersonDb.Title = CP.Title;
                                            ClientContactPersonDb.Location = CP.Location;
                                            ClientContactPersonDb.Email = CP.Email;
                                            ClientContactPersonDb.Mobile = CP.Mobile;

                                        }
                                        _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Contact Person Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    // Insert
                                    //ObjectParameter ClientContactPersonInsertedId = new ObjectParameter("ID", typeof(long));
                                    //var ClientContactPersonInsert = _Context.proc_ClientContactPersonInsert(ClientContactPersonInsertedId,
                                    //                                                                        Request.ClientId,
                                    //                                                                        long.Parse(Encrypt_Decrypt.Decrypt(CP.CreatedBy, key)),
                                    //                                                                        DateTime.Now,
                                    //                                                                        null,
                                    //                                                                        null,
                                    //                                                                        true,
                                    //                                                                        CP.Name,
                                    //                                                                        CP.Title,
                                    //                                                                        CP.Location,
                                    //                                                                        CP.Email,
                                    //                                                                        CP.Mobile
                                    //                                                                        );

                                    var ClientContactPersonInsert = new ClientContactPerson();
                                    ClientContactPersonInsert.ClientId = Request.ClientId;
                                    ClientContactPersonInsert.CreatedBy = validation.userID;
                                    ClientContactPersonInsert.CreationDate = DateTime.Now;
                                    ClientContactPersonInsert.Active = true;
                                    ClientContactPersonInsert.Name = CP.Name;
                                    ClientContactPersonInsert.Title = CP.Title;
                                    ClientContactPersonInsert.Location = CP.Location;
                                    ClientContactPersonInsert.Email = CP.Email;
                                    ClientContactPersonInsert.Mobile = CP.Mobile;
                                    _unitOfWork.ClientContactPersons.Add(ClientContactPersonInsert);
                                    _unitOfWork.Complete();
                                }
                            }
                            // For Update(Diactivate or Remove) supplier -  supplier Mobile - supplier Contact Person - supplier Address
                            //_Context.SaveChanges();
                            _unitOfWork.Complete();
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



        public async Task<BaseResponseWithID> AddClientTaxCard(ClientTaxCardData Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        var ClientDb = await _unitOfWork.Clients.FindAsync(a => a.Id == Request.ClientId);
                        if (ClientDb != null)
                        {
                            ClientId = Request.ClientId;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Client Doesn't Exist";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ClientId Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrEmpty(Request.TaxCard))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "TaxCard Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        var ClientDb = await _unitOfWork.Clients.FindAsync(a => a.Id == ClientId);
                        ClientDb.TaxCard = Request.TaxCard;
                        var Result = _unitOfWork.Complete();
                        if (Result > 0)
                        {
                            Response.ID = ClientId;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Update This Record, Or Updated With The Same Value";
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

        public ClientsDetailsResponse GetClientsDetailsResponse(long ClientId)
        {
            ClientsDetailsResponse Response = new ClientsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err69";
                        error.ErrorMSG = "Invalid Client Id";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var ClientDetailsDB = _unitOfWork.Clients.Find(a => a.Id == ClientId, includes: new[] { "SalesPerson" });
                    if (ClientDetailsDB != null)
                    {
                        var ClientStatus = "";
                        if (ClientDetailsDB.NeedApproval != null)
                        {
                            switch (ClientDetailsDB.NeedApproval)
                            {
                                case 0:
                                    ClientStatus = "Approved";
                                    break;
                                case 1:
                                    ClientStatus = "Waiting";
                                    break;
                                case 2:
                                    ClientStatus = "Rejected";
                                    break;
                                default:
                                    break;
                            }
                        }
                        Response.ClientStatus = ClientStatus;
                        Response.ClientCalssification = ClientDetailsDB.ClientClassification?.Name;
                        Response.ClientClassificationId = ClientDetailsDB.ClientClassificationId;
                        Response.SalesPersonId = ClientDetailsDB.SalesPersonId;
                        Response.SalesPersonName = ClientDetailsDB?.SalesPerson?.FirstName + " " + ClientDetailsDB?.SalesPerson?.LastName;
                        Response.LastReportDate = DateTime.Parse(ClientDetailsDB.LastReportDate?.ToString()).ToShortDateString();

                        var LastCRMReport = _unitOfWork.VCrmreports.FindAll(a => a.ClientId == ClientId).OrderByDescending(a => a.CreationDate).FirstOrDefault();
                        if (LastCRMReport != null)
                        {
                            Response.LastCRMReportId = LastCRMReport.Id;
                            Response.LastCRMReportDate = LastCRMReport.CreationDate.ToShortDateString();
                        }

                        var LastSalesReport = _unitOfWork.DailyReportLines.FindAll(a => (a.ClientId ?? 0) == ClientId).OrderByDescending(a => a.CreationDate).FirstOrDefault();
                        if (LastSalesReport != null)
                        {
                            Response.LastSalesReportLineId = LastSalesReport.Id;
                            Response.LastSalesReportId = LastSalesReport.DailyReportId;
                            Response.LastSalesReportDate = LastSalesReport.CreationDate.ToShortDateString();
                        }
                    }

                    var ClientContactPersonsDB = _unitOfWork.ClientContactPersons.FindAll(a => a.ClientId == ClientId && a.Active == true).ToList();
                    var ClientContactPersonsList = new List<ContactPersonDetails>();
                    foreach (var CP in ClientContactPersonsDB)
                    {
                        var ContactPerson = new ContactPersonDetails
                        {
                            Id = CP.Id,
                            Name = CP.Name,
                            Mobile = CP.Mobile
                        };

                        ClientContactPersonsList.Add(ContactPerson);
                    }

                    Response.ContactPersonsList = ClientContactPersonsList;
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

        public async Task<BaseResponseWithID> UpdateClassificationOfClient(UpdateClientClassRequest Request)
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

                    if (Request.ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ClientId Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Request.ClassificationId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Classification Id Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        var Class = await _unitOfWork.ClientClassifications.FindAsync(a => a.Id == Request.ClassificationId);
                        if (Class == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Class Doesn't Exist";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            var Client = await _unitOfWork.Clients.FindAsync(a => a.Id == Request.ClientId);
                            if (Client == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Client Doesn't Exist";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                Client.ClientClassificationId = Request.ClassificationId;
                                _unitOfWork.Complete();
                                Response.ID = Request.ClientId;
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetClientsCardsStatistics GetClientsCardsStatisticsResponse(GetClientsCardsStatisticsHeaders filters)
        {
            GetClientsCardsStatistics Response = new GetClientsCardsStatistics();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //string Phone = "";
                    //if (!string.IsNullOrEmpty(headers["Phone"]))
                    //{
                    //    Phone = headers["Phone"].ToString();
                    //}

                    //string Mobile = "";
                    //if (!string.IsNullOrEmpty(headers["Mobile"]))
                    //{
                    //    Mobile = headers["Mobile"].ToString();
                    //}

                    //string SupportedBy = "";
                    //if (!string.IsNullOrEmpty(headers["SupportedBy"]))
                    //{
                    //    SupportedBy = HttpUtility.UrlDecode(headers["SupportedBy"]).ToLower();
                    //}

                    //string Fax = "";
                    //if (!string.IsNullOrEmpty(headers["Fax"]))
                    //{
                    //    Fax = headers["Fax"].ToString();
                    //}

                    //string ClientName = "";
                    //if (!string.IsNullOrEmpty(headers["ClientName"]))
                    //{
                    //    ClientName = HttpUtility.UrlDecode(headers["ClientName"]).ToLower();
                    //}

                    var PurchasingProductsStartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var PurchasingProductsEndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);

                    string PurchasingProductsIdsString = "";
                    List<long> PurchasingProductsIdsList = new List<long>();
                    if (!string.IsNullOrEmpty(filters.PurchasingProductsIdsString))
                    {
                        PurchasingProductsIdsString = HttpUtility.UrlDecode(filters.PurchasingProductsIdsString).ToLower();
                        if (!string.IsNullOrEmpty(PurchasingProductsIdsString))
                        {
                            try
                            {
                                PurchasingProductsIdsList = PurchasingProductsIdsString.Split(',').Select(long.Parse).ToList();

                                if (!string.IsNullOrEmpty(filters.PurchasingProductsStartDate))
                                {
                                    DateTime From = DateTime.Now;
                                    if (!DateTime.TryParse(filters.PurchasingProductsStartDate, out From))
                                    {
                                        Error error = new Error();
                                        error.ErrorCode = "Err-12";
                                        error.ErrorMSG = "Invalid Purchasing Products Start Date";
                                        Response.Errors.Add(error);
                                        Response.Result = false;
                                        return Response;
                                    }
                                    PurchasingProductsStartDate = From;

                                    if (!string.IsNullOrEmpty(filters.PurchasingProductsEndDate))
                                    {
                                        DateTime To = DateTime.Now;
                                        if (!DateTime.TryParse(filters.PurchasingProductsEndDate, out To))
                                        {
                                            Error error = new Error();
                                            error.ErrorCode = "Err-13";
                                            error.ErrorMSG = "Invalid Purchasing Products End Date";
                                            Response.Errors.Add(error);
                                            Response.Result = false;
                                        }
                                        PurchasingProductsEndDate = To;
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(filters.PurchasingProductsEndDate))
                                    {
                                        Error error = new Error();
                                        error.ErrorCode = "Err-13";
                                        error.ErrorMSG = "You have to Enter Purchasing Products Start Date!";
                                        Response.Errors.Add(error);
                                        Response.Result = false;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err110";
                                error.ErrorMSG = "Purchasing products Ids not in correct format";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    var RegistrationDateFrom = DateTime.Now;
                    var RegistrationDateTo = DateTime.Now;
                    var RegistrationDateFilter = false;
                    if (!string.IsNullOrEmpty(filters.RegistrationDateFrom))
                    {
                        RegistrationDateFilter = true;
                        DateTime From = DateTime.Now;
                        if (!DateTime.TryParse(filters.RegistrationDateFrom, out From))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Registration Date From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        RegistrationDateFrom = From;

                        if (!string.IsNullOrEmpty(filters.RegistrationDateTo))
                        {
                            DateTime To = DateTime.Now;
                            if (!DateTime.TryParse(filters.RegistrationDateTo, out To))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Registration Date To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                            }
                            RegistrationDateTo = To;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filters.RegistrationDateTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "You have to Enter Registration Date From!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                        }
                    }

                    //int SpecialityId = 0;
                    //if (!string.IsNullOrEmpty(headers["SpecialityId"]) && int.TryParse(headers["SpecialityId"], out SpecialityId))
                    //{
                    //    int.TryParse(headers["SpecialityId"], out SpecialityId);
                    //}

                    //long SalesPersonId = 0;
                    //if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                    //{
                    //    long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    //}

                    //int BranchId = 0;
                    //if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    //{
                    //    int.TryParse(headers["BranchId"], out BranchId);
                    //}

                    //bool? IsExpired = null;
                    //if (!string.IsNullOrEmpty(headers["IsExpired"]))
                    //{
                    //    if (bool.Parse(headers["IsExpired"]) != true && bool.Parse(headers["IsExpired"]) != false)
                    //    {
                    //        Response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err110";
                    //        error.ErrorMSG = "IsExpired must be true of false ";
                    //        Response.Errors.Add(error);
                    //    }
                    //    else
                    //    {
                    //        IsExpired = bool.Parse(headers["IsExpired"]);
                    //    }
                    //}

                    var DealsDateFrom = DateTime.Now;
                    var DealsDateTo = DateTime.Now;
                    var DealsDateFilter = false;
                    if (!string.IsNullOrEmpty(filters.DealsDateFrom))
                    {
                        DealsDateFilter = true;
                        DateTime From = DateTime.Now;
                        if (!DateTime.TryParse(filters.DealsDateFrom, out From))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Deals Date From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        DealsDateFrom = From;

                        if (!string.IsNullOrEmpty(filters.DealsDateTo))
                        {
                            DateTime To = DateTime.Now;
                            if (!DateTime.TryParse(filters.DealsDateTo, out To))
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-13";
                                error.ErrorMSG = "Invalid Deals Date To";
                                Response.Errors.Add(error);
                                Response.Result = false;
                            }
                            DealsDateTo = To;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(filters.DealsDateTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "You have to Enter Deals Date From!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                        }
                    }

                    bool? HasRFQ = null;
                    if (filters.HasRFQ != null)
                    {
                        if (filters.HasRFQ != true && filters.HasRFQ != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "HasRFQ must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            HasRFQ = filters.HasRFQ;
                        }
                    }

                    bool? WithOpenOffers = null;
                    if (filters.WithOpenOffers != null)
                    {
                        if (filters.WithOpenOffers != true && filters.WithOpenOffers != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "With Open Offers must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            WithOpenOffers = filters.WithOpenOffers;
                        }
                    }

                    bool? WithOpenProjects = null;
                    if (filters.WithOpenProjects != null)
                    {
                        if (filters.WithOpenProjects != true && filters.WithOpenProjects != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "With Open Projects must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            WithOpenProjects = filters.WithOpenProjects;
                        }
                    }

                    bool? WithVolume = null;
                    if (filters.WithVolume != null)
                    {
                        if (filters.WithVolume != true && filters.WithVolume != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "With Volume must be true of false ";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            WithVolume = filters.WithVolume;
                        }
                    }

                    //int ApprovalStatus = -1;
                    //if (!string.IsNullOrEmpty(headers["ApprovalStatus"]) && int.TryParse(headers["ApprovalStatus"], out ApprovalStatus))
                    //{
                    //    int.TryParse(headers["ApprovalStatus"], out ApprovalStatus);
                    //}

                    //int ClientClassification = 0;
                    //if (!string.IsNullOrEmpty(headers["ClientClassification"]) && int.TryParse(headers["ClientClassification"], out ClientClassification))
                    //{
                    //    int.TryParse(headers["ClientClassification"], out ClientClassification);
                    //}

                    //int ExpirationPeriod = 0;
                    //if (!string.IsNullOrEmpty(headers["ExpirationPeriod"]) && int.TryParse(headers["ExpirationPeriod"], out ExpirationPeriod))
                    //{
                    //    int.TryParse(headers["ExpirationPeriod"], out ExpirationPeriod);
                    //}
                    //long AreaId = 0;
                    //if (!string.IsNullOrWhiteSpace(headers["AreaId"]) && long.TryParse(headers["AreaId"], out AreaId))
                    //{
                    //    long.TryParse(headers["AreaId"], out AreaId);
                    //}
                    //int CountryId = 0;
                    //int GovernorateId = 0;
                    if (filters.GovernorateId != null)
                    {
                        if (filters.CountryId != null)
                        {
                            //int.TryParse(headers["CountryId"], out CountryId);
                            //int.TryParse(headers["GovernorateId"], out GovernorateId);

                            if (filters.CountryId <= 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err110";
                                error.ErrorMSG = "Invalid CountryId!";
                                Response.Errors.Add(error);
                            }
                            else if (filters.GovernorateId <= 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err110";
                                error.ErrorMSG = "Invalid GovernorateId!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                var CountryDB = _unitOfWork.Countries.GetById(filters.CountryId ?? 0);
                                if (CountryDB != null)
                                {
                                    var GovernorateDB = _unitOfWork.Governorates.GetById(filters.GovernorateId ?? 0);
                                    if (GovernorateDB != null)
                                    {
                                        if (GovernorateDB.CountryId != filters.CountryId)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err110";
                                            error.ErrorMSG = "Invalid Governorate For this Country!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err110";
                                        error.ErrorMSG = "This Governorate Doesn't Exist!";
                                        Response.Errors.Add(error);
                                    }

                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err110";
                                    error.ErrorMSG = "This Country Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err110";
                            error.ErrorMSG = "You must Enter CountryId of This City!";
                            Response.Errors.Add(error);
                        }
                    }

                    if (Response.Result)
                    {
                        bool Filtered = false;

                        List<long> MobileClientsIds = new List<long>();
                        if (!string.IsNullOrEmpty(filters.Mobile))
                        {
                            MobileClientsIds.AddRange(_unitOfWork.ClientMobiles.FindAll(a => a.Mobile.Contains(filters.Mobile)).Select(a => a.Id).ToList());
                            MobileClientsIds.AddRange(_unitOfWork.ClientContactPersons.FindAll(a => a.Mobile.Contains(filters.Mobile)).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        MobileClientsIds = MobileClientsIds.Distinct().ToList();

                        List<long> PhoneClientsIds = new List<long>();
                        if (!string.IsNullOrEmpty(filters.Phone))
                        {
                            PhoneClientsIds.AddRange(_unitOfWork.VClientPhones.FindAll(a => a.Phone.Contains(filters.Phone)).Select(a => a.Id).ToList());
                            Filtered = true;
                        }
                        PhoneClientsIds = PhoneClientsIds.Distinct().ToList();

                        List<long> FaxClientsIds = new List<long>();
                        if (!string.IsNullOrEmpty(filters.Fax))
                        {
                            FaxClientsIds.AddRange(_unitOfWork.ClientFaxes.FindAll(a => a.Fax.Contains(filters.Fax)).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        FaxClientsIds = FaxClientsIds.Distinct().ToList();

                        List<long> SpecialityClientsIds = new List<long>();
                        if (filters.SpecialityId != 0)
                        {
                            SpecialityClientsIds.AddRange(_unitOfWork.VClientSpecialities.FindAll(a => a.SpecialityId == filters.SpecialityId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        SpecialityClientsIds = SpecialityClientsIds.Distinct().ToList();

                        List<long> AddressClientsIds = new List<long>();
                        if (filters.GovernorateId != null)
                        {
                            AddressClientsIds.AddRange(_unitOfWork.VClientAddresses.FindAll(a => a.GovernorateId == filters.GovernorateId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        else if (filters.CountryId != null)
                        {
                            AddressClientsIds.AddRange(_unitOfWork.VClientAddresses.FindAll(a => a.CountryId == filters.CountryId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        if (filters.AreaId != 0)
                        {
                            AddressClientsIds.AddRange(_unitOfWork.VClientAddresses.FindAll(a => a.AreaId == filters.AreaId).Select(a => a.ClientId).ToList());
                            Filtered = true;
                        }
                        AddressClientsIds = AddressClientsIds.Distinct().ToList();

                        var ClientsIds = new List<long>();
                        if (MobileClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = MobileClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(MobileClientsIds).ToList();
                        }

                        if (PhoneClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = PhoneClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(PhoneClientsIds).ToList();
                        }

                        if (FaxClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = FaxClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(FaxClientsIds).ToList();
                        }

                        if (SpecialityClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = SpecialityClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(SpecialityClientsIds).ToList();
                        }

                        if (AddressClientsIds.Count() > 0)
                        {
                            if (ClientsIds.Count() == 0)
                                ClientsIds = AddressClientsIds.ToList();
                            else
                                ClientsIds = ClientsIds.Intersect(AddressClientsIds).ToList();
                        }

                        ClientsIds = ClientsIds.Distinct().ToList();

                        var ClientsDBQuery = _unitOfWork.VClientUseers.FindAllQueryable(a => true);
                        if (ClientsIds.Count() > 0 || Filtered)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => ClientsIds.Contains(a.Id));
                        }
                        if (!string.IsNullOrEmpty(filters.ClientName))
                        {
                            var string_compare_prepare_function = Common.string_compare_prepare_function(filters.ClientName).ToLower();
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.PreparedSearchName.ToLower().Contains(string_compare_prepare_function)).AsQueryable();
                            var SearchedClientsIds = ClientsDBQuery.Where(a => a.PreparedSearchName.ToLower().Contains(string_compare_prepare_function)).Select(a => a.Id).ToList();
                            var VehiclesClientsIds = _unitOfWork.VVehicles.FindAll(a => SearchedClientsIds.Contains(a.ClientId) || (a.Vin ?? "").Contains(string_compare_prepare_function) || (a.PlatNumber ?? "").Contains(string_compare_prepare_function)).Select(a => a.ClientId).ToList();
                            if (VehiclesClientsIds.Count() > 0)
                            {
                                ClientsDBQuery = ClientsDBQuery.Where(a => VehiclesClientsIds.Contains(a.Id));
                            }
                        }
                        if (filters.SalesPersonId != 0 && filters.SalesPersonId != null)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.SupportedBy))
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.SupportedBy.ToLower() == filters.SupportedBy).AsQueryable();
                        }
                        if (filters.BranchId != 0 && filters.BranchId != null)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                        }
                        if (filters.ApprovalStatus != -1)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.NeedApproval == filters.ApprovalStatus).AsQueryable();
                        }

                        if (filters.ClientClassification != 0)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.ClientClassificationId == filters.ClientClassification).AsQueryable();
                        }
                        if (filters.IsExpired != null)
                        {
                            if (filters.IsExpired == true)
                            {
                                var ExpiredClientsIds = _unitOfWork.VClientUseers.FindAll(a => a.CreationDate < DateTime.Now).Select(a => a.Id).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => ExpiredClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (WithOpenOffers != null)
                        {
                            if (WithOpenOffers == true)
                            {
                                var WithOpenOffersClientsIds = _Context.SalesOffers.Where(a => a.Status.ToLower() != "closed" && a.Status.ToLower() != "rejected" && (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => WithOpenOffersClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (HasRFQ != null)
                        {
                            if (HasRFQ == true)
                            {
                                var HasRFQClientsIds = _Context.SalesOffers.Where(a => (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).Distinct().ToList();

                                ClientsDBQuery = ClientsDBQuery.Where(a => HasRFQClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (WithOpenProjects != null)
                        {
                            if (WithOpenProjects == true)
                            {
                                var WithOpenProjctsClientsIds = _unitOfWork.VProjectSalesOffers.FindAll(a => a.Closed == false && (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => WithOpenProjctsClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (WithVolume != null)
                        {
                            if (WithVolume == true)
                            {
                                var WithVolumeClientsIds = _Context.SalesOffers.Where(a => a.Status.ToLower() == "closed" && (DealsDateFilter ? a.CreationDate >= DealsDateFrom && a.CreationDate < DealsDateTo : true)).Select(a => a.ClientId).ToList();
                                ClientsDBQuery = ClientsDBQuery.Where(a => WithVolumeClientsIds.Contains(a.Id)).AsQueryable();
                            }
                        }
                        if (PurchasingProductsIdsList != null && PurchasingProductsIdsList.Count > 0)
                        {
                            var ClientsWithPurchasingProductsIds = _unitOfWork.VSalesOfferProductSalesOffers.FindAll(a => PurchasingProductsIdsList.Contains(a.InventoryItemId ?? 0) && a.Status.ToLower() == "closed" && a.ClientApprovalDate >= PurchasingProductsStartDate && a.ClientApprovalDate <= PurchasingProductsEndDate).Select(a => a.ClientId).Distinct().ToList();
                            ClientsDBQuery = ClientsDBQuery.Where(a => ClientsWithPurchasingProductsIds.Contains(a.Id)).AsQueryable();
                        }
                        if (filters.ExpirationPeriod != 0)
                        {
                            DateTime DateAfterTenDays = DateTime.Now.AddDays(filters.ExpirationPeriod);
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.ClientExpireDate != null && ((DateTime)a.ClientExpireDate).Date >= DateTime.Now.Date && ((DateTime)a.ClientExpireDate).Date <= DateAfterTenDays.Date);
                            //var list = ClientsDBQuery.ToList();
                            //var fasd = ClientsDBQuery.Count();
                        }
                        if (RegistrationDateFilter)
                        {
                            ClientsDBQuery = ClientsDBQuery.Where(a => a.CreationDate >= RegistrationDateFrom && a.CreationDate <= RegistrationDateTo).AsQueryable();
                        }

                        ClientsDBQuery = ClientsDBQuery.OrderByDescending(a => a.CreationDate);

                        var FilteredClientsIds = ClientsDBQuery.Select(a => a.Id).ToList();
                        var projectsClientsIds = _unitOfWork.VProjectSalesOffers.FindAll(a => a.ClientId != null).Select(a => a.ClientId ?? 0).Distinct().ToList();
                        var MergedClientsIds = FilteredClientsIds.Intersect(projectsClientsIds).ToList();

                        var TotalProjectsExtraCosts = _unitOfWork.VProjectSalesOffers.FindAll(a => MergedClientsIds.Contains(a.ClientId ?? 0) && a.ExtraCost != null).ToList().Sum(a => a.ExtraCost);
                        var TotalProjectsCosts = _unitOfWork.VProjectSalesOffers.FindAll(a => MergedClientsIds.Contains(a.ClientId ?? 0)).ToList().Sum(a => a.FinalOfferPrice);
                        decimal TotalVolume = (TotalProjectsExtraCosts ?? 0) + (TotalProjectsCosts ?? 0);

                        Response.TotalVolume = TotalVolume;
                        Response.TotalClientsCount = ClientsDBQuery.Count();

                        var TotalClientsOpenProjects = _unitOfWork.VProjectSalesOfferClientAccounts.FindAll(a => MergedClientsIds.Contains(a.ClientId ?? 0) && a.Closed == false).ToList();
                        var TotalProjectsCost = TotalClientsOpenProjects.Select(a => new { ProjectId = a.Id, TotalCost = a.FinalOfferPrice + a.ExtraCost }).Distinct().Sum(a => a.TotalCost);
                        var totalPlus = TotalClientsOpenProjects
                            .Where(a => a.ClientAccountId != null && a.AmountSign == "plus")
                            .AsEnumerable()
                            .GroupBy(a => a.ClientAccountId)
                            .Select(g => g.First().Amount)
                            .Sum();

                        var totalMinus = TotalClientsOpenProjects
                            .Where(a => a.ClientAccountId != null && a.AmountSign == "minus")
                            .AsEnumerable()
                            .GroupBy(a => a.ClientAccountId)
                            .Select(g => g.First().Amount)
                            .Sum();
                        var TotalCollected = totalPlus - totalMinus;
                        var TotalRemain = TotalProjectsCost - TotalCollected;
                        Response.TotalRemainCollection = TotalRemain ?? 0;

                        //int CurrentPage = 1;
                        //int NumberOfItemsPerPage = 100;
                        //var PagedProjectsQuery = _Context.V_Project_SalesOffer.Where(a => a.ClientID != null && MergedClientsIds.Contains(a.ClientID ?? 0)).AsQueryable();
                        //PagedProjectsQuery = PagedProjectsQuery.OrderByDescending(a => a.CreationDate);
                        //var PagedProjects = PagedList<V_Project_SalesOffer>.Create(PagedProjectsQuery, CurrentPage, NumberOfItemsPerPage);

                        //decimal TotalProjectsExtraCosts = 0;
                        //decimal TotalProjectsCosts = 0;
                        //for (int i = 0; i < PagedProjects.TotalPages; i++)
                        //{
                        //    TotalProjectsExtraCosts += (PagedProjects.Where(a => a.ExtraCost != null).Select(a => a.ExtraCost).Sum() ?? 0);
                        //    TotalProjectsCosts += (PagedProjects.Select(a => a.FinalOfferPrice).Sum() ?? 0);

                        //    CurrentPage++;

                        //    PagedProjects = PagedList<V_Project_SalesOffer>.Create(PagedProjectsQuery, CurrentPage, NumberOfItemsPerPage);
                        //}

                        //var TotalClientsOpenProjects = _Context.V_Project_SalesOffer.Where(a => MergedClientsIds.Contains(a.ClientID ?? 0) && a.Closed == false).ToList();

                        //decimal totalClientsProjectsRemain = 0;
                        //decimal totcol = 0;
                        //foreach (var project in TotalClientsOpenProjects)
                        //{
                        //    var projectCost = project.FinalOfferPrice + project.ExtraCost ?? 0;

                        //    decimal totalProjectCollectedCost = 0;
                        //    var clientAccounts = _Context.proc_ClientAccountsLoadAll().Where(x => x.ProjectID == project.ID);
                        //    foreach (var clientAccount in clientAccounts)
                        //    {
                        //        if (clientAccount.AmountSign == "plus")
                        //        {
                        //            totcol += clientAccount.Amount;
                        //            totalProjectCollectedCost = totalProjectCollectedCost + clientAccount.Amount;
                        //        }
                        //        else if (clientAccount.AmountSign == "minus")
                        //        {
                        //            totcol -= clientAccount.Amount;
                        //            totalProjectCollectedCost = totalProjectCollectedCost - clientAccount.Amount;
                        //        }
                        //    }

                        //    var projectRemainCost = projectCost - totalProjectCollectedCost;
                        //    totalClientsProjectsRemain += projectRemainCost;
                        //}
                        //Response.TotalRemainCollection = totalClientsProjectsRemain;
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

        public BaseResponseWithID AddNewClientConsultant(ClientConsultantData Request, long creator)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                        var client = _unitOfWork.Clients.GetById(ClientId);
                        if (client == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    long ConsultantId = 0;
                    string CreatedByString = null;
                    string ModifiedByString = null;
                    long? CreatedBy = 0;
                    long? ModifiedBy = 0;
                    if (Request.Id != 0 && Request.Id != null)
                    {
                        if (Request.ModifiedBy != null)
                        {
                            ModifiedByString = Request.ModifiedBy;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Modified By Id Is Mandatory";
                            Response.Errors.Add(error);
                        }
                        ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));

                        ConsultantId = Request.Id ?? 0;
                        var consultant = _unitOfWork.ClientConsultants.GetById(ConsultantId);
                        if (consultant == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Consultant Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        if (Request.CreatedBy != null)
                        {
                            CreatedByString = Request.CreatedBy;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Created By Id Is Mandatory";
                            Response.Errors.Add(error);
                        }
                        CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));

                        string ConsultantName = null;
                        if (Request.ConsultantName != null)
                        {
                            ConsultantName = Request.ConsultantName;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Consultant Name Is Mandatory";
                            Response.Errors.Add(error);
                        }
                    }

                    if (Response.Result)
                    {
                        // Add And Update supplier Consultant
                        if (Request.Id != null && Request.Id != 0)
                        {
                            //Update
                            var ClientConsultantDb = _unitOfWork.ClientConsultants.GetById(Request.Id ?? 0);
                            if (ClientConsultantDb != null)
                            {
                                //var ClientConsultantUpdate = _Context.proc_ClientConsultantUpdate(Request.Id,
                                //                                                                    Request.ClientId,
                                //                                                                    Request.ConsultantName,
                                //                                                                    ClientConsultantDb.CreatedBy,
                                //                                                                    ClientConsultantDb.CreationDate,
                                //                                                                    ModifiedBy,
                                //                                                                    DateTime.Now,
                                //                                                                    true,
                                //                                                                    Request.Company,
                                //                                                                    Request.ConsultantFor
                                //                                                                                );

                                ClientConsultantDb.ClientId = Request.ClientId;
                                ClientConsultantDb.ConsultantName = Request.ConsultantName;
                                ClientConsultantDb.ModifiedBy = ModifiedBy;
                                ClientConsultantDb.Modified = DateTime.Now;
                                ClientConsultantDb.Active = true;
                                ClientConsultantDb.Company = Request.Company;
                                ClientConsultantDb.ConsultantFor = Request.ConsultantFor;

                                var ClientConsultantUpdate = _unitOfWork.Complete();

                                if (ClientConsultantUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = Request.Id ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Consultant!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Consultant doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            //Insert
                            //ObjectParameter ClientConsultantInsertedId = new ObjectParameter("ID", typeof(long));
                            //var ClientConsultantInsert = _Context.proc_ClientConsultantInsert(ClientConsultantInsertedId,
                            //                                                                    Request.ClientId,
                            //                                                                    Request.ConsultantName,
                            //                                                                    CreatedBy,
                            //                                                                    DateTime.Now,
                            //                                                                    null,
                            //                                                                    null,
                            //                                                                    true,
                            //                                                                    Request.Company,
                            //                                                                    Request.ConsultantFor
                            //                                                                    );

                            var ClientConsultantInsert = new ClientConsultant();
                            ClientConsultantInsert.ClientId = Request.ClientId;
                            ClientConsultantInsert.ConsultantName = Request.ConsultantName;
                            ClientConsultantInsert.CreatedBy = creator;
                            ClientConsultantInsert.CreationDate = DateTime.Now;
                            ClientConsultantInsert.Active = true;
                            ClientConsultantInsert.Company = Request.Company;
                            ClientConsultantInsert.ConsultantFor = Request.ConsultantFor;

                            _unitOfWork.ClientConsultants.Add(ClientConsultantInsert);
                            _unitOfWork.Complete();

                            if (ClientConsultantInsert.Id > 0)
                            {
                                Response.Result = true;
                                Request.Id = (long)ClientConsultantInsert.Id;
                                Response.ID = (long)ClientConsultantInsert.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Client!!";
                                Response.Errors.Add(error);
                            }
                        }

                        // Add And Update supplier Consultant Addresses
                        if (Request.ConsultantAddresses != null)
                        {
                            if (Request.ConsultantAddresses.Count > 0)
                            {
                                var counter = 0;
                                foreach (var Adrs in Request.ConsultantAddresses)
                                {
                                    counter++;


                                    int CountryId = 0;
                                    if (Adrs.CountryID != 0)
                                    {
                                        CountryId = Adrs.CountryID;
                                        var country = _unitOfWork.Country.GetById(CountryId);
                                        if (country == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "This Country Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Address Country ID Is Mandatory";
                                        Response.Errors.Add(error);
                                    }

                                    int GovernorateId = 0;
                                    if (Adrs.GovernorateID != 0)
                                    {
                                        GovernorateId = Adrs.GovernorateID;
                                        var governorate = _unitOfWork.Governorates.GetById(GovernorateId);
                                        if (governorate == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "This City Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Address Governorate ID Is Mandatory";
                                        Response.Errors.Add(error);
                                    }

                                    string Address = null;
                                    if (!string.IsNullOrEmpty(Adrs.Address))
                                    {
                                        Address = Adrs.Address;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Address Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }


                                if (Response.Result)
                                {
                                    foreach (var Adrs in Request.ConsultantAddresses)
                                    {
                                        if (Adrs.ID != null && Adrs.ID != 0)
                                        {
                                            var AddressDb = _unitOfWork.ClientConsultantAddressis.GetById(Adrs.ID ?? 0);
                                            if (AddressDb != null)
                                            {
                                                // Update
                                                //    var ClientConsultantAddressUpdate = _Context.proc_ClientConsultantAddressUpdate(Adrs.ID,
                                                //                                                                Request.Id,
                                                //                                                                Adrs.CountryID,
                                                //                                                                Adrs.GovernorateID,
                                                //                                                                Adrs.Address,
                                                //                                                                AddressDb.CreatedBy,
                                                //                                                                AddressDb.CreationDate,
                                                //                                                                long.Parse(Encrypt_Decrypt.Decrypt(Adrs.ModifiedBy, key)),
                                                //                                                                DateTime.Now,
                                                //                                                                true,
                                                //                                                                Adrs.BuildingNumber,
                                                //                                                                Adrs.Floor,
                                                //                                                                Adrs.Description
                                                //                                                                );

                                                AddressDb.ConsultantId = (long)Request.Id;
                                                AddressDb.CountryId = Adrs.CountryID;
                                                AddressDb.GovernorateId = Adrs.GovernorateID;
                                                AddressDb.Address = Adrs.Address;
                                                AddressDb.ModifiedBy = validation.userID;
                                                AddressDb.Modified = DateTime.Now;
                                                AddressDb.Active = true;
                                                AddressDb.BuildingNumber = Adrs.BuildingNumber;
                                                AddressDb.Floor = Adrs.Floor;
                                                AddressDb.Description = Adrs.Description;

                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Address Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientConsultantAddressInsertedId = new ObjectParameter("ID", typeof(long));
                                            //var ClientConsultantInsert = _Context.proc_ClientConsultantAddressInsert(ClientConsultantAddressInsertedId,
                                            //                                                                Request.Id,
                                            //                                                                Adrs.CountryID,
                                            //                                                                Adrs.GovernorateID,
                                            //                                                                Adrs.Address,
                                            //                                                                long.Parse(Encrypt_Decrypt.Decrypt(Adrs.CreatedBy, key)),
                                            //                                                                DateTime.Now,
                                            //                                                                null,
                                            //                                                                null,
                                            //                                                                true,
                                            //                                                                Adrs.BuildingNumber,
                                            //                                                                Adrs.Floor,
                                            //                                                                Adrs.Description
                                            //                                                                );

                                            var ClientConsultantInsert = new ClientConsultantAddress();
                                            ClientConsultantInsert.ConsultantId = (long)Request.Id;
                                            ClientConsultantInsert.CountryId = Adrs.CountryID;
                                            ClientConsultantInsert.GovernorateId = Adrs.GovernorateID;
                                            ClientConsultantInsert.Address = Adrs.Address;
                                            ClientConsultantInsert.CreatedBy = validation.userID;
                                            ClientConsultantInsert.CreationDate = DateTime.Now;
                                            ClientConsultantInsert.Active = true;
                                            ClientConsultantInsert.BuildingNumber = Adrs.BuildingNumber;
                                            ClientConsultantInsert.Floor = Adrs.Floor;
                                            ClientConsultantInsert.Description = Adrs.Description;

                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }

                        // Add And Update supplier Consultant ConsultantFaxes
                        if (Request.ConsultantFaxes != null)
                        {
                            if (Request.ConsultantFaxes.Count > 0)
                            {
                                var counter = 0;
                                foreach (var FAX in Request.ConsultantFaxes)
                                {
                                    counter++;
                                    string Fax = null;
                                    if (!string.IsNullOrEmpty(FAX.Fax))
                                    {
                                        Fax = FAX.Fax;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Fax Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (Response.Result)
                                {
                                    foreach (var FAX in Request.ConsultantFaxes)
                                    {
                                        if (FAX.ID != null && FAX.ID != 0)
                                        {
                                            var FaxDb = _unitOfWork.ClientConsultantFaxs.GetById(FAX.ID ?? 0);
                                            if (FaxDb != null)
                                            {
                                                // Update
                                                //var ClientConsultantFaxUpdate = _u.proc_ClientConsultantFaxUpdate(FAX.ID,
                                                //                                                            Request.Id,
                                                //                                                            FAX.Fax,
                                                //                                                            FaxDb.CreatedBy,
                                                //                                                            FaxDb.CreationDate,
                                                //                                                            long.Parse(Encrypt_Decrypt.Decrypt(FAX.ModifiedBy, key)),
                                                //                                                            DateTime.Now,
                                                //                                                            true
                                                //                                                            );

                                                if (Request.Id != null)
                                                {

                                                    FaxDb.ConsultantId = Request.Id ?? 0;
                                                    FaxDb.Fax = FAX.Fax;
                                                    FaxDb.ModifiedBy = validation.userID;
                                                    FaxDb.Modified = DateTime.Now;
                                                    FaxDb.Active = true;

                                                    _unitOfWork.Complete();
                                                }
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Fax Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientConsultantFaxInsertedId = new ObjectParameter("ID", typeof(long));
                                            //var ClientConsultantInsert = _Context.proc_ClientConsultantFaxInsert(ClientConsultantFaxInsertedId,
                                            //                                                                Request.Id,
                                            //                                                                FAX.Fax,
                                            //                                                                long.Parse(Encrypt_Decrypt.Decrypt(FAX.CreatedBy, key)),
                                            //                                                                DateTime.Now,
                                            //                                                                null,
                                            //                                                                null,
                                            //                                                                true
                                            //                                                                );

                                            var ClientConsultantInsert = new ClientConsultantFax();
                                            ClientConsultantInsert.ConsultantId = Request.Id ?? 0;
                                            ClientConsultantInsert.Fax = FAX.Fax;
                                            ClientConsultantInsert.CreatedBy = validation.userID;
                                            ClientConsultantInsert.CreationDate = DateTime.Now;
                                            ClientConsultantInsert.Active = true;

                                            _unitOfWork.ClientConsultantFaxs.Add(ClientConsultantInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }

                        // Add And Update supplier Consultant Consultant Emails
                        if (Request.ConsultantEmails != null)
                        {
                            if (Request.ConsultantEmails.Count > 0)
                            {
                                var counter = 0;
                                foreach (var email in Request.ConsultantEmails)
                                {
                                    counter++;



                                    string Email = null;
                                    if (!string.IsNullOrEmpty(email.Email))
                                    {
                                        Email = email.Email;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Email Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (Response.Result)
                                {
                                    foreach (var email in Request.ConsultantEmails)
                                    {
                                        if (email.ID != null && email.ID != 0)
                                        {
                                            var EmailDb = _unitOfWork.ClientConsultantEmails.GetById(email.ID ?? 0);
                                            if (EmailDb != null)
                                            {
                                                // Update
                                                //var ClientConsultantEmailUpdate = _Context.proc_ClientConsultantEmailUpdate(email.ID,
                                                //                                                            Request.Id,
                                                //                                                            email.Email,
                                                //                                                            EmailDb.CreatedBy,
                                                //                                                            EmailDb.CreationDate,
                                                //                                                            long.Parse(Encrypt_Decrypt.Decrypt(email.ModifiedBy, key)),
                                                //                                                            DateTime.Now,
                                                //                                                            true
                                                //                                                            );

                                                EmailDb.ConsultantId = Request.Id ?? 0;
                                                EmailDb.Email = email.Email;
                                                EmailDb.ModifiedBy = validation.userID;
                                                EmailDb.Modified = DateTime.Now;
                                                EmailDb.Active = true;

                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Email Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientConsultantEmailInsertedId = new ObjectParameter("ID", typeof(long));
                                            //var ClientConsultantEmailInsert = _Context.proc_ClientConsultantEmailInsert(ClientConsultantEmailInsertedId,
                                            //                                                                Request.Id,
                                            //                                                                email.Email,
                                            //                                                                long.Parse(Encrypt_Decrypt.Decrypt(email.CreatedBy, key)),
                                            //                                                                DateTime.Now,
                                            //                                                                null,
                                            //                                                                null,
                                            //                                                                true
                                            //                                                                );

                                            var ClientConsultantEmailInsert = new ClientConsultantEmail();
                                            ClientConsultantEmailInsert.ConsultantId = Request.Id ?? 0;
                                            ClientConsultantEmailInsert.Email = email.Email;
                                            ClientConsultantEmailInsert.CreatedBy = validation.userID;
                                            ClientConsultantEmailInsert.CreationDate = DateTime.Now;
                                            ClientConsultantEmailInsert.Active = true;

                                            _unitOfWork.ClientConsultantEmails.Add(ClientConsultantEmailInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }

                        // Add And Update supplier Consultant Consultant Mobiles
                        if (Request.ConsultantMobiles != null)
                        {
                            if (Request.ConsultantMobiles.Count > 0)
                            {
                                var counter = 0;
                                foreach (var mobile in Request.ConsultantMobiles)
                                {
                                    counter++;

                                    string MobileCreatedByString = null;
                                    string MobileModifiedByString = null;
                                    long? MobileCreatedBy = 0;
                                    long? MobileModifiedBy = 0;
                                    if (mobile.ID != 0 && mobile.ID != null)
                                    {
                                        if (mobile.ModifiedBy != null)
                                        {
                                            MobileModifiedByString = mobile.ModifiedBy;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Message: Mobile Modified By Id Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                        MobileModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(MobileModifiedByString, key));
                                    }
                                    else
                                    {
                                        if (mobile.CreatedBy != null)
                                        {
                                            MobileCreatedByString = mobile.CreatedBy;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Mobile Created By Id Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                        MobileCreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(MobileCreatedByString, key));
                                    }

                                    string Mobile = null;
                                    if (!string.IsNullOrEmpty(mobile.Mobile))
                                    {
                                        Mobile = mobile.Mobile;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Mobile Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (Response.Result)
                                {
                                    foreach (var mobile in Request.ConsultantMobiles)
                                    {
                                        if (mobile.ID != null && mobile.ID != 0)
                                        {
                                            var MobileDb = _unitOfWork.ClientConsultantMobiles.GetById(mobile.ID ?? 0);
                                            if (MobileDb != null)
                                            {
                                                //// Update
                                                //var ClientConsultantMobileUpdate = _Context.proc_ClientConsultantMobileUpdate(mobile.ID,
                                                //                                                            Request.Id,
                                                //                                                            mobile.Mobile,
                                                //                                                            MobileDb.CreatedBy,
                                                //                                                            MobileDb.CreationDate,
                                                //                                                            long.Parse(Encrypt_Decrypt.Decrypt(mobile.ModifiedBy, key)),
                                                //                                                            DateTime.Now,
                                                //                                                            true
                                                //                                                            );

                                                MobileDb.ConsultantId = Request.Id ?? 0;
                                                MobileDb.Mobile = mobile.Mobile;
                                                MobileDb.ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(mobile.ModifiedBy, key));
                                                MobileDb.Modified = DateTime.Now;
                                                MobileDb.Active = true;

                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Mobile Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientConsultantMobileInsertedId = new ObjectParameter("ID", typeof(long));
                                            //var ClientConsultantMobileInsert = _Context.proc_ClientConsultantMobileInsert(ClientConsultantMobileInsertedId,
                                            //                                                                Request.Id,
                                            //                                                                mobile.Mobile,
                                            //                                                                long.Parse(Encrypt_Decrypt.Decrypt(mobile.CreatedBy, key)),
                                            //                                                                DateTime.Now,
                                            //                                                                null,
                                            //                                                                null,
                                            //                                                                true
                                            //                                                                );

                                            var ClientConsultantMobileInsert = new ClientConsultantMobile();
                                            ClientConsultantMobileInsert.ConsultantId = Request.Id ?? 0;
                                            ClientConsultantMobileInsert.Mobile = mobile.Mobile;
                                            ClientConsultantMobileInsert.CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(mobile.CreatedBy, key));
                                            ClientConsultantMobileInsert.CreationDate = DateTime.Now;
                                            ClientConsultantMobileInsert.Active = true;

                                            _unitOfWork.ClientConsultantMobiles.Add(ClientConsultantMobileInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }

                        // Add And Update supplier Consultant Consultant Phones
                        if (Request.ConsultantLandLines != null)
                        {
                            if (Request.ConsultantLandLines.Count > 0)
                            {
                                var counter = 0;
                                foreach (var phone in Request.ConsultantLandLines)
                                {
                                    counter++;
                                    string Phone = null;
                                    if (!string.IsNullOrEmpty(phone.LandLine))
                                    {
                                        Phone = phone.LandLine;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Phone Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (Response.Result)
                                {
                                    foreach (var phone in Request.ConsultantLandLines)
                                    {
                                        if (phone.ID != null && phone.ID != 0)
                                        {
                                            var PhoneDb = _unitOfWork.ClientConsultantPhones.GetById(phone.ID ?? 0);
                                            if (PhoneDb != null)
                                            {
                                                //// Update
                                                //var ClientConsultantPhoneUpdate = _Context.proc_ClientConsultantPhoneUpdate(phone.ID,
                                                //                                                            Request.Id,
                                                //                                                            phone.LandLine,
                                                //                                                            PhoneDb.CreatedBy,
                                                //                                                            PhoneDb.CreationDate,
                                                //                                                            long.Parse(Encrypt_Decrypt.Decrypt(phone.ModifiedBy, key)),
                                                //                                                            DateTime.Now,
                                                //                                                            true
                                                //                                                            );

                                                PhoneDb.ConsultantId = Request.Id ?? 0;
                                                PhoneDb.Phone = phone.LandLine;
                                                PhoneDb.ModifiedBy = validation.userID;
                                                PhoneDb.Modified = DateTime.Now;
                                                PhoneDb.Active = true;

                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Phone Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientConsultantPhoneInsertedId = new ObjectParameter("ID", typeof(long));
                                            //var ClientConsultantPhoneInsert = _Context.proc_ClientConsultantPhoneInsert(ClientConsultantPhoneInsertedId,
                                            //                                                                Request.Id,
                                            //                                                                phone.LandLine,
                                            //                                                                long.Parse(Encrypt_Decrypt.Decrypt(phone.CreatedBy, key)),
                                            //                                                                DateTime.Now,
                                            //                                                                null,
                                            //                                                                null,
                                            //                                                                true
                                            //                                                                );

                                            var ClientConsultantPhoneInsert = new ClientConsultantPhone();
                                            ClientConsultantPhoneInsert.ConsultantId = Request.Id ?? 0;
                                            ClientConsultantPhoneInsert.Phone = phone.LandLine;
                                            ClientConsultantPhoneInsert.CreatedBy = validation.userID;
                                            ClientConsultantPhoneInsert.CreationDate = DateTime.Now;
                                            ClientConsultantPhoneInsert.Active = true;

                                            _unitOfWork.ClientConsultantPhones.Add(ClientConsultantPhoneInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }

                        // Add And Update supplier Consultant Consultant Specialities
                        if (Request.ConsultantSpecialities != null)
                        {
                            if (Request.ConsultantSpecialities.Count > 0)
                            {
                                var counter = 0;
                                foreach (var speciality in Request.ConsultantSpecialities)
                                {
                                    counter++;


                                    int SpecialityID = 0;
                                    if (speciality.SpecialityID != 0)
                                    {
                                        SpecialityID = speciality.SpecialityID;
                                        var specialityDB = _unitOfWork.Specialities.GetById(SpecialityID);
                                        if (specialityDB == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "This Speciality Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Speciality ID Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (Response.Result)
                                {
                                    var ConsultantSpecialitiesDB = _unitOfWork.ClientConsultantSpecialilties.FindAll(a => a.ConsultantId == Request.Id).ToList();
                                    if (ConsultantSpecialitiesDB != null && ConsultantSpecialitiesDB.Count > 0)
                                    {
                                        foreach (var SPC in ConsultantSpecialitiesDB)
                                        {
                                            _unitOfWork.ClientConsultantSpecialilties.Delete(SPC);
                                        }
                                    }
                                    foreach (var speciality in Request.ConsultantSpecialities)
                                    {
                                        //if (speciality.ID != null && speciality.ID != 0)
                                        //{
                                        //    var SpecialityDb = _Context.proc_ClientConsultantSpecialiltyLoadByPrimaryKey(speciality.ID).FirstOrDefault();
                                        //    if (SpecialityDb != null)
                                        //    {
                                        //        // Update
                                        //        var ClientConsultantSpecialityUpdate = _Context.proc_ClientConsultantSpecialiltyUpdate(speciality.ID,
                                        //                                                                    Request.Id,
                                        //                                                                    speciality.SpecialityID,
                                        //                                                                    SpecialityDb.CreatedBy,
                                        //                                                                    SpecialityDb.CreationDate,
                                        //                                                                    long.Parse(Encrypt_Decrypt.Decrypt(speciality.ModifiedBy, key)),
                                        //                                                                    DateTime.Now,
                                        //                                                                    true
                                        //                                                                    );
                                        //    }
                                        //    else
                                        //    {
                                        //        Response.Result = false;
                                        //        Error error = new Error();
                                        //        error.ErrorCode = "Err25";
                                        //        error.ErrorMSG = "This Speciality Doesn't Exist!!";
                                        //        Response.Errors.Add(error);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        // Insert
                                        //ObjectParameter ClientConsultantSpecialityInsertedId = new ObjectParameter("ID", typeof(long));
                                        //var ClientConsultantSpecialityInsert = _Context.proc_ClientConsultantSpecialiltyInsert(ClientConsultantSpecialityInsertedId,
                                        //                                                                Request.Id,
                                        //                                                                speciality.SpecialityID,
                                        //                                                                long.Parse(Encrypt_Decrypt.Decrypt(speciality.CreatedBy, key)),
                                        //                                                                DateTime.Now,
                                        //                                                                null,
                                        //                                                                null,
                                        //                                                                true
                                        //                                                                );

                                        var ClientConsultantSpecialityInsert = new ClientConsultantSpecialilty();
                                        ClientConsultantSpecialityInsert.ConsultantId = Request.Id ?? 0;
                                        ClientConsultantSpecialityInsert.SpecialityId = speciality.SpecialityID;
                                        ClientConsultantSpecialityInsert.CreationDate = DateTime.Now;
                                        ClientConsultantSpecialityInsert.CreatedBy = validation.userID;
                                        ClientConsultantSpecialityInsert.Active = true;

                                        _unitOfWork.ClientConsultantSpecialilties.Add(ClientConsultantSpecialityInsert);
                                        _unitOfWork.Complete();
                                        ////}
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<ClientClassificationsResponse> GetClientClassifications()
        {
            ClientClassificationsResponse Response = new ClientClassificationsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    var ClientClassList = await _unitOfWork.ClientClassifications.FindAllAsync(a => true);
                    if (ClientClassList != null && ClientClassList.Count() > 0)
                    {
                        Response.ClientClassifications = ClientClassList.Select(item => new SelectDDL { ID = item.Id, Name = item.Name }).ToList();

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
        public void SaveAndValidateClientAttachment(UploadAttachment attachment, string type, string CompanyName, long ClientId, long CreatedBy)
        {
            if (attachment.Id != null && attachment.Active == false)
            {
                // Delete
                var LicenseAttachmentDb = _unitOfWork.ClientAttachments.GetById((long)attachment.Id);
                if (LicenseAttachmentDb != null)
                {
                    var AttachmentPath = Path.Combine(_host.WebRootPath, LicenseAttachmentDb.AttachmentPath);

                    if (File.Exists(AttachmentPath))
                    {
                        File.Delete(AttachmentPath);

                    }
                    _unitOfWork.ClientAttachments.Delete(LicenseAttachmentDb);
                    _unitOfWork.Complete();
                }
            }
            else
            {
                var fileExtension = attachment.FileContent.FileName.Split('.').Last();
                var virtualPath = $"Attachments\\{CompanyName}\\Clients\\{ClientId}\\";
                var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.FileContent.FileName);
                var FilePath = Common.SaveFileIFF(virtualPath, attachment.FileContent, FileName, fileExtension, _host);

                // Insert
                var clientAttachment = new ClientAttachment()
                {
                    ClientId = ClientId,
                    AttachmentPath = FilePath,
                    CreatedBy = CreatedBy,
                    CreationDate = DateTime.Now,
                    ModifiedBy = CreatedBy,
                    Modified = DateTime.Now,
                    Active = true,
                    FileName = FileName,
                    FileExtenssion = fileExtension,
                    Type = type
                };

                _unitOfWork.ClientAttachments.Add(clientAttachment);
                _unitOfWork.Complete();
            }
        }

        public BaseResponseWithId<long> ValidateClientAttachments(UploadAttachment attachment, int counter, string type)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (attachment.Id == null)
            {
                if (attachment.FileContent == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = counter != 0 && type == null ? "Line " + counter + ": File Content Is Mandatory!" : counter == 0 && type == "Tax" ? "Commercial Record Attachment File Content Is Mandatory!" : "Tax Card Attachment File Content Is Mandatory!";
                    Response.Errors.Add(error);
                }
            }
            return Response;
        }
        public BaseResponseWithId<long> AddClientAttachments(ClientAttachmentsData Request, string companyname)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ClientId Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    string CreatedByString = null;
                    if (Request.CreatedBy != null)
                    {
                        CreatedByString = Request.CreatedBy;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Created By Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));

                    if (Response.Result)
                    {
                        if (Request.LicenseAttachements != null && Request.LicenseAttachements.Count > 0)
                        {
                            var counter = 0;
                            foreach (var attachment in Request.LicenseAttachements)
                            {
                                counter++;
                                var resp = ValidateClientAttachments(attachment, counter, null);
                                Response.Result = Response.Result && resp.Result;
                                Response.Errors.AddRange(resp.Errors);


                            }

                            counter = 0;
                            if (Response.Result)
                            {
                                counter++;
                                foreach (var attachment in Request.LicenseAttachements)
                                {
                                    SaveAndValidateClientAttachment(attachment, "License", companyname, ClientId, CreatedBy);
                                }
                            }
                        }

                        if (Request.BrochuresAttachments != null && Request.BrochuresAttachments.Count > 0)
                        {
                            var counter = 0;
                            foreach (var attachment in Request.BrochuresAttachments)
                            {
                                counter++;
                                var resp = ValidateClientAttachments(attachment, counter, null);
                                Response.Result = Response.Result && resp.Result;
                                Response.Errors.AddRange(resp.Errors);
                            }

                            counter = 0;
                            if (Response.Result)
                            {
                                counter++;
                                foreach (var attachment in Request.BrochuresAttachments)
                                {
                                    SaveAndValidateClientAttachment(attachment, "Brochure", companyname, ClientId, CreatedBy);
                                }
                            }
                        }

                        if (Request.BussinessCardsAttachments != null && Request.BussinessCardsAttachments.Count > 0)
                        {
                            var counter = 0;
                            foreach (var attachment in Request.BussinessCardsAttachments)
                            {
                                counter++;
                                var resp = ValidateClientAttachments(attachment, counter, null);
                                Response.Result = Response.Result && resp.Result;
                                Response.Errors.AddRange(resp.Errors);
                            }

                            counter = 0;
                            if (Response.Result)
                            {
                                counter++;
                                foreach (var attachment in Request.BussinessCardsAttachments)
                                {
                                    SaveAndValidateClientAttachment(attachment, "Business Cards", companyname, ClientId, CreatedBy);
                                }
                            }
                        }

                        if (Request.OtherAttachments != null && Request.OtherAttachments.Count > 0)
                        {
                            var counter = 0;
                            foreach (var attachment in Request.OtherAttachments)
                            {
                                counter++;
                                var resp = ValidateClientAttachments(attachment, counter, null);
                                Response.Result = Response.Result && resp.Result;
                                Response.Errors.AddRange(resp.Errors);
                            }

                            counter = 0;
                            if (Response.Result)
                            {
                                counter++;
                                foreach (var attachment in Request.OtherAttachments)
                                {
                                    SaveAndValidateClientAttachment(attachment,
                                        //"Other",
                                        attachment.Category,
                                        companyname, ClientId, CreatedBy);
                                }
                            }
                        }

                        if (Request.TaxCardAttachment != null)
                        {
                            var resp = ValidateClientAttachments(Request.TaxCardAttachment, 0, "Tax");
                            Response.Result = Response.Result && resp.Result;
                            Response.Errors.AddRange(resp.Errors);

                            if (Response.Result)
                            {
                                SaveAndValidateClientAttachment(Request.TaxCardAttachment, "Tax Card", companyname, ClientId, CreatedBy);

                            }
                        }

                        if (Request.CommercialRecordAttachment != null)
                        {
                            var resp = ValidateClientAttachments(Request.CommercialRecordAttachment, 0, "Tax");
                            Response.Result = Response.Result && resp.Result;
                            Response.Errors.AddRange(resp.Errors);

                            if (Response.Result)
                            {
                                SaveAndValidateClientAttachment(Request.CommercialRecordAttachment, "Commercial Record", companyname, ClientId, CreatedBy);

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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetClientDashboardResponse> GetClientDashboard([FromHeader] long ClientId)
        {
            GetClientDashboardResponse Response = new GetClientDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err110";
                        error.ErrorMSG = "ClientId Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        var ClientDb = (await _unitOfWork.VClientUseers.FindAllAsync(a => a.Id == ClientId)).FirstOrDefault();
                        var ClientOffersDataDb = (await _unitOfWork.VProjectSalesOffers.FindAllAsync(a => a.ClientId == ClientId)).ToList();

                        int TotalOffersNumber = 0, TotalOpenProjectNumber = 0, TotalClosedProjectNumber = 0, TotalCRMReportsNumber = 0, TotalSalesReportsNumber = 0, ExpirationRemainingDays = 0;
                        decimal TotalBusinessVolume = 0;
                        string ClientExpirationDate = null, FirstOfferDate = null;
                        bool IsExpired = false;

                        if (ClientOffersDataDb != null && ClientOffersDataDb.Count() > 0)
                        {
                            TotalOpenProjectNumber = ClientOffersDataDb.Where(a => a.Closed == false).Count();
                            TotalClosedProjectNumber = ClientOffersDataDb.Where(a => a.Closed == true).Count();
                            TotalBusinessVolume = ClientOffersDataDb.Where(a => a.FinalOfferPrice != null).Sum(a => a.FinalOfferPrice) ?? 0;
                            var TotalProjectsExtraCosts = ClientOffersDataDb.Where(a => a.ExtraCost != null).Sum(a => a.ExtraCost) ?? 0;
                            TotalBusinessVolume += TotalProjectsExtraCosts;
                        }

                        TotalOffersNumber = (await _unitOfWork.SalesOffers.FindAllAsync(a => a.ClientId == ClientId && a.Status.ToLower() != "closed")).Count();
                        TotalCRMReportsNumber = (await _unitOfWork.VCrmreports.FindAllAsync(a => a.ClientId == ClientId)).Count();
                        TotalSalesReportsNumber = (await _unitOfWork.VDailyReportReportLineThroughApis.FindAllAsync(a => a.ClientId == ClientId)).Count();

                        if (ClientDb.ClientExpireDate != null)
                        {
                            ClientExpirationDate = DateTime.Parse(ClientDb.ClientExpireDate.ToString()).ToShortDateString();
                            if (ClientDb.ClientExpireDate > DateTime.Now)
                            {
                                ExpirationRemainingDays = (int)(DateTime.Parse(ClientDb.ClientExpireDate.ToString()) - DateTime.Now).TotalDays;
                            }
                            else
                            {
                                IsExpired = true;
                            }
                        }

                        var FirstOfferDateDb = (await _unitOfWork.SalesOffers.FindAllAsync(a => a.ClientId == ClientId && a.Status.ToLower() == "closed" && a.ClientApprovalDate != null)).OrderBy(a => a.ClientApprovalDate).Select(a => a.ClientApprovalDate).FirstOrDefault();

                        if (FirstOfferDateDb != null)
                        {
                            FirstOfferDate = FirstOfferDateDb.ToString().Split(' ')[0];
                        }

                        var ClientDashboardResponse = new ClientDashboard
                        {
                            ClosedProjectsNumber = TotalClosedProjectNumber,
                            ExpirationDate = ClientExpirationDate,
                            ExpirationRemainingDays = ExpirationRemainingDays,
                            FirstOfferDate = FirstOfferDate,
                            FollowUpCRMReportsNumber = TotalCRMReportsNumber,
                            FollowUpSalesReportsNumber = TotalSalesReportsNumber,
                            IsExpired = IsExpired,
                            OffersNumber = TotalOffersNumber,
                            OpenProjectsNumber = TotalOpenProjectNumber,
                            TotalBusinessVolume = TotalBusinessVolume
                        };

                        Response.ClientDashboard = ClientDashboardResponse;
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

        public CheckClientExistanceResponse CheckClientExistance(CheckClientExistanceFilters filters)
        {
            CheckClientExistanceResponse Response = new CheckClientExistanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (!string.IsNullOrEmpty(filters.ClientName))
                    {
                        filters.ClientName = HttpUtility.UrlDecode(filters.ClientName).ToLower();
                    }
                    if (!string.IsNullOrEmpty(filters.ClientEnglishName))
                    {
                        filters.ClientEnglishName = filters.ClientEnglishName.ToLower();
                    }
                    if (!string.IsNullOrEmpty(filters.ClientMobile))
                    {
                        filters.ClientMobile = filters.ClientMobile;
                    }
                    if (!string.IsNullOrEmpty(filters.ClientPhone))
                    {
                        filters.ClientPhone = filters.ClientPhone;
                    }
                    if (!string.IsNullOrEmpty(filters.ClientFax))
                    {
                        filters.ClientFax = filters.ClientFax;
                    }
                    if (!string.IsNullOrEmpty(filters.ClientEmail))
                    {
                        filters.ClientEmail = filters.ClientEmail;
                    }
                    if (!string.IsNullOrEmpty(filters.ClientWebsite))
                    {
                        filters.ClientWebsite = filters.ClientWebsite;
                    }
                    if (!string.IsNullOrEmpty(filters.ContactPersonMobile))
                    {
                        filters.ContactPersonMobile = filters.ContactPersonMobile;
                    }
                    if (Response.Result)
                    {
                        var ClientsDBQuery = _unitOfWork.Clients.FindAllQueryable(a => a.NeedApproval == 0).AsQueryable();

                        List<Client> ClientsDb = new List<Client>();

                        if (!string.IsNullOrEmpty(filters.ClientName))
                        {
                            if (filters.IsExact)
                            {
                                ClientsDb = ClientsDBQuery.Where(a => a.Name.ToLower() == filters.ClientName).ToList();
                            }
                            else
                            {
                                ClientsDb = ClientsDBQuery.Where(a => a.Name.ToLower().Contains(filters.ClientName)).ToList();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.ClientEnglishName))
                        {
                            if (filters.IsExact)
                            {
                                ClientsDb = ClientsDBQuery.Where(a => a.EnglishName.ToLower() == filters.ClientEnglishName).ToList();
                            }
                            else
                            {
                                ClientsDb = ClientsDBQuery.Where(a => a.EnglishName.Contains(filters.ClientEnglishName)).ToList();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.ClientMobile))
                        {
                            var ClientsIds = _unitOfWork.ClientMobiles.FindAll(x => x.Mobile == filters.ClientMobile).Select(x => x.ClientId).ToList();
                            if (ClientsIds != null && ClientsIds.Count() > 0)
                            {
                                ClientsDb = ClientsDBQuery.Where(a => ClientsIds.Contains(a.Id)).ToList();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.ClientPhone))
                        {
                            var ClientsIds = _Context.ClientPhones.Where(x => x.Phone == filters.ClientPhone).Select(x => x.ClientId).ToList();
                            if (ClientsIds != null && ClientsIds.Count() > 0)
                            {
                                ClientsDb = ClientsDBQuery.Where(a => ClientsIds.Contains(a.Id)).ToList();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.ClientFax))
                        {
                            var ClientsIds = _unitOfWork.ClientFaxes.FindAll(x => x.Fax == filters.ClientFax).Select(x => x.ClientId).ToList();
                            if (ClientsIds != null && ClientsIds.Count() > 0)
                            {
                                ClientsDb = ClientsDBQuery.Where(a => ClientsIds.Contains(a.Id)).ToList();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.ClientEmail))
                        {
                            ClientsDb = ClientsDBQuery.Where(a => a.Email.ToLower() == filters.ClientEmail).ToList();

                        }
                        if (!string.IsNullOrEmpty(filters.ClientWebsite))
                        {
                            ClientsDb = ClientsDBQuery.Where(a => a.WebSite.ToLower() == filters.ClientWebsite).ToList();
                        }
                        if (!string.IsNullOrEmpty(filters.ContactPersonMobile))
                        {
                            var ClientsIds = _unitOfWork.ClientContactPeople.FindAll(x => x.Mobile == filters.ContactPersonMobile).Select(x => x.ClientId).ToList();
                            if (ClientsIds != null && ClientsIds.Count > 0)
                            {
                                ClientsDb = ClientsDBQuery.Where(a => ClientsIds.Contains(a.Id)).ToList();
                            }
                        }
                        if (ClientsDb != null && ClientsDb.Count > 0)
                        {
                            var ClientsList = ClientsDb.Select(a => new SelectDDL()
                            {
                                ID = a.Id,
                                Name = a.Name
                            }).ToList();

                            Response.ClientsList = ClientsList;
                            Response.IsExist = true;
                            Response.ClientsCount = ClientsList.Count();
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

        public BaseResponseWithId<long> AddNewClientMobile(ClientMobileDataDto Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        if (Request.ClientMobiles.Count > 0)
                        {
                            var counter = 0;
                            foreach (var MOB in Request.ClientMobiles)
                            {
                                counter++;


                                string CreatedByString = null;
                                string ModifiedByString = null;
                                long? CreatedBy = 0;
                                long? ModifiedBy = 0;


                                string Mobile = null;
                                if (!string.IsNullOrEmpty(MOB.Mobile))
                                {
                                    Mobile = MOB.Mobile;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Line: " + counter + ", " + "Mobile Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }
                        }

                        if (Response.Result)
                        {
                            foreach (var MOB in Request.ClientMobiles)
                            {
                                if (MOB.ID != null && MOB.ID != 0)
                                {
                                    var ClientMobileDb = _unitOfWork.ClientMobiles.GetById((long)MOB.ID);
                                    if (ClientMobileDb != null)
                                    {
                                        // Update
                                        ClientMobileDb.ClientId = Request.ClientId;
                                        ClientMobileDb.Mobile = MOB.Mobile;
                                        ClientMobileDb.ModifiedBy = validation.userID;
                                        ClientMobileDb.Modified = DateTime.Now;
                                        ClientMobileDb.Active = true;
                                        _unitOfWork.ClientMobiles.Update(ClientMobileDb);
                                        var ClientMobileUpdate = _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Mobile Person Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    _unitOfWork.ClientMobiles.Add(new ClientMobile()
                                    {
                                        ClientId = Request.ClientId,
                                        Mobile = MOB.Mobile,
                                        CreatedBy = validation.userID,
                                        CreationDate = DateTime.Now,
                                        ModifiedBy = validation.userID,
                                        Modified = DateTime.Now,
                                        Active = true
                                    });
                                    var ClientMobileInsert = _unitOfWork.Complete();
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> AddNewClientSpeciality(ClientSpecialityData Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        if (Request.ClientSpecialities.Count > 0)
                        {
                            var counter = 0;
                            foreach (var SPC in Request.ClientSpecialities)
                            {


                                int SpecialityId = 0;
                                if (SPC.SpecialityID != 0)
                                {
                                    SpecialityId = SPC.SpecialityID;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Line: " + counter + ", " + "Speciality Id Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }
                        }

                        if (Response.Result)
                        {
                            foreach (var SPC in Request.ClientSpecialities)
                            {
                                if (SPC.ID != null && SPC.ID != 0)
                                {
                                    var ClientSpecialityDb = _unitOfWork.ClientSpecialities.GetById((long)SPC.ID);
                                    if (ClientSpecialityDb != null)
                                    {
                                        // Update\
                                        ClientSpecialityDb.ClientId = Request.ClientId;
                                        ClientSpecialityDb.SpecialityId = SPC.SpecialityID;
                                        ClientSpecialityDb.ModifiedBy = validation.userID;
                                        ClientSpecialityDb.Modified = DateTime.Now;
                                        ClientSpecialityDb.Active = true;

                                        _unitOfWork.ClientSpecialities.Update(ClientSpecialityDb);
                                        _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Speciality Person Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    // Insert
                                    _unitOfWork.ClientSpecialities.Add(new ClientSpeciality()
                                    {
                                        ClientId = Request.ClientId,
                                        SpecialityId = SPC.SpecialityID,
                                        CreatedBy = validation.userID,
                                        CreationDate = DateTime.Now,
                                        ModifiedBy = validation.userID,
                                        Modified = DateTime.Now,
                                        Active = true
                                    });
                                    _unitOfWork.Complete();
                                    var ClientMobileInsert = _unitOfWork.Complete();
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> AddNewClientLandLine(ClientLandLineDataDto Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        if (Request.ClientLandLines.Count > 0)
                        {
                            var counter = 0;
                            foreach (var LND in Request.ClientLandLines)
                            {
                                counter++;



                                string LandLine = null;
                                if (!string.IsNullOrEmpty(LND.LandLine))
                                {
                                    LandLine = LND.LandLine;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Line: " + counter + ", " + "Land Line Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }
                        }

                        if (Response.Result)
                        {
                            foreach (var LND in Request.ClientLandLines)
                            {
                                if (LND.ID != null && LND.ID != 0)
                                {
                                    var ClientLandLineDb = _unitOfWork.ClientPhones.GetById((long)LND.ID);
                                    if (ClientLandLineDb != null)
                                    {
                                        // Update
                                        ClientLandLineDb.ClientId = Request.ClientId;
                                        ClientLandLineDb.Phone = LND.LandLine;
                                        ClientLandLineDb.ModifiedBy = validation.userID;
                                        ClientLandLineDb.Modified = DateTime.Now;
                                        ClientLandLineDb.Active = true;
                                        _unitOfWork.ClientPhones.Update(ClientLandLineDb);

                                        var ClientLandLineUpdate = _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Land Line Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    // Insert
                                    _unitOfWork.ClientPhones.Add(new ClientPhone()
                                    {
                                        ClientId = Request.ClientId,
                                        Phone = LND.LandLine,
                                        CreatedBy = validation.userID,
                                        CreationDate = DateTime.Now,
                                        ModifiedBy = validation.userID,
                                        Modified = DateTime.Now,
                                        Active = true
                                    });

                                    var ClientLandLineInsert = _unitOfWork.Complete();
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> AddNewClientFax(ClientFaxDataDto Request)
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

                    long ClientId = 0;
                    if (Request.ClientId != 0)
                    {
                        ClientId = Request.ClientId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Client Id Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        if (Request.ClientFaxes.Count > 0)
                        {
                            var counter = 0;
                            foreach (var FX in Request.ClientFaxes)
                            {
                                counter++;



                                string Fax = null;
                                if (!string.IsNullOrEmpty(FX.Fax))
                                {
                                    Fax = FX.Fax;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Line: " + counter + ", " + "Fax Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }
                        }

                        if (Response.Result)
                        {
                            foreach (var FX in Request.ClientFaxes)
                            {
                                if (FX.ID != null && FX.ID != 0)
                                {
                                    var ClientFaxDb = _unitOfWork.ClientFaxes.GetById((long)FX.ID);
                                    if (ClientFaxDb != null)
                                    {
                                        // Update
                                        ClientFaxDb.ClientId = Request.ClientId;
                                        ClientFaxDb.Fax = FX.Fax;
                                        ClientFaxDb.ModifiedBy = validation.userID;
                                        ClientFaxDb.Modified = DateTime.Now;
                                        ClientFaxDb.Active = true;
                                        _unitOfWork.ClientFaxes.Update(ClientFaxDb);
                                        var ClientFaxUpdate = _unitOfWork.Complete();
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Fax Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    // Insert
                                    _unitOfWork.ClientFaxes.Add(new ClientFax()
                                    {
                                        ClientId = Request.ClientId,
                                        Fax = FX.Fax,
                                        CreatedBy = validation.userID,
                                        CreationDate = DateTime.Now,
                                        ModifiedBy = validation.userID,
                                        Modified = DateTime.Now,
                                        Active = true
                                    });

                                    var ClientFaxInsert = _unitOfWork.Complete();
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public SelectDDLResponse GetClientList(GetClientListFilters filters)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var clintsQueryable = _unitOfWork.Clients.FindAllQueryable(a => true);

                    #region old headers
                    //int NeedApproval = -1;
                    //if (!string.IsNullOrEmpty(Request.Headers["NeedApproval"]) && int.TryParse(Request.Headers["NeedApproval"], out NeedApproval))
                    //{
                    //    int.TryParse(Request.Headers["NeedApproval"], out NeedApproval);
                    //}
                    //int BranchId = 0;
                    //if (!string.IsNullOrEmpty(Request.Headers["BranchId"]) && int.TryParse(Request.Headers["BranchId"], out BranchId))
                    //{
                    //    int.TryParse(Request.Headers["BranchId"], out BranchId);
                    //}

                    //string SalesPersonIdString = null;
                    //long SalesPersonId = 0;

                    //if (!string.IsNullOrEmpty(Request.Headers["SalesPersonId"]))
                    //{
                    //    SalesPersonIdString = Request.Headers["SalesPersonId"];
                    //    SalesPersonId = long.Parse(Encrypt_Decrypt.Decrypt(SalesPersonIdString, key));
                    //}
                    #endregion

                    if (filters.NeedApproval != null)
                    {
                        clintsQueryable = clintsQueryable.Where(a => a.NeedApproval == filters.NeedApproval).AsQueryable();
                    }
                    if (filters.BranchId != null)
                    {
                        clintsQueryable = clintsQueryable.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                    }
                    if (filters.SalesPersonId != null)
                    {
                        clintsQueryable = clintsQueryable.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(filters.ClientType))
                    {
                        clintsQueryable = clintsQueryable.Where(a => a.Type.ToLower() == filters.ClientType.ToLower()).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {

                        //string SearchKey = Request.Headers["SearchKey"];
                        var SearchKey = HttpUtility.UrlDecode(filters.SearchKey);

                        var ListIDSMobileClient = _unitOfWork.ClientMobiles.FindAll(x => x.Active == true && x.Mobile.ToLower().Contains(SearchKey.ToLower())).Select(x => x.ClientId).Distinct().ToList();
                        var ListIDSContactPersonClient = _unitOfWork.ClientContactPeople.FindAll(x => x.Active == true && (x.Mobile.ToLower().Contains(SearchKey.ToLower())
                                                                                                                          || x.Email != null ? x.Email.ToLower().Contains(SearchKey.ToLower()) : false)
                                                                                                                          ).Select(x => x.ClientId).Distinct().ToList();
                        var string_compare_prepare_function = Common.string_compare_prepare_function(SearchKey.ToLower());
                        clintsQueryable = clintsQueryable.Where(x => (x.Name.ToLower().Contains(SearchKey.ToLower()))
                        ||
                                                   (x.PreparedSearchName != null && x.PreparedSearchName.ToLower().Contains(string_compare_prepare_function))
                                                || (x.Email != null ? x.Email.ToLower().Contains(SearchKey.ToLower()) : false) || (x.EnglishName != null ? x.EnglishName.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || ListIDSMobileClient.Contains(x.Id) || ListIDSContactPersonClient.Contains(x.Id)
                                                ).AsQueryable();
                    }

                    var ListDB = clintsQueryable.ToList();

                    if (ListDB.Count > 0)
                    {
                        foreach (var Client in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = Client.Id;
                            DLLObj.Name = Client.Name;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList.Distinct().ToList();
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

        public BaseResponseWithID AddNewClient(ClientData request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long? NewClientSerial = _unitOfWork.Clients.GetMaxLong(p => p == null ? 0 : p.ClientSerialCounter ?? 0) + 1;

                    string Name = null;
                    string EnglishName = null;
                    long? ClientId = null;
                    bool IsSalesPersonChanged = false;
                    if (request.Id != null)
                    {
                        ClientId = request.Id;
                        var ClientDb = _unitOfWork.Clients.Find(a => a.Id == ClientId);
                        if (ClientDb != null)
                        {
                            if (request.Name != null)
                            {
                                if (ClientDb.Name.ToLower() != request.Name.ToLower())
                                {
                                    Name = request.Name.ToLower();
                                    var ExistNameClientDb = _unitOfWork.Clients.Find(a => a.Name.ToLower() == Name);
                                    if (ExistNameClientDb != null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Client Name Already Exist, Please Enter Another One!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                Name = request.Name;
                            }

                            if (request.EnglishName != null)
                            {
                                if (ClientDb.EnglishName == null || ClientDb.EnglishName.ToLower() != request.EnglishName.ToLower())
                                {
                                    EnglishName = request.EnglishName.ToLower();
                                    var ExistNameClientDb = _unitOfWork.Clients.Find(a => a.EnglishName.ToLower() == EnglishName);
                                    if (ExistNameClientDb != null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "This Client English Name Already Exist, Please Enter Another One!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                EnglishName = request.EnglishName;
                            }

                            if (request.SalesPersonID != 0)
                            {
                                if (request.SalesPersonID != ClientDb.SalesPersonId)
                                {
                                    if (string.IsNullOrEmpty(request.ChangeSalesPersonComment))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Please Enter SalesPerson Change Reason";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        IsSalesPersonChanged = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Client Doesn't Exist!!";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        if (request.ClientSerialCounter != null)
                        {
                            var ExistClientSerialDb = _unitOfWork.Clients.Find(a => a.ClientSerialCounter == (long)request.ClientSerialCounter);
                            if (ExistClientSerialDb != null)
                            {
                                if (ExistClientSerialDb.Id != ClientId)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Client Serial Already Exist, Please Enter Another One!!";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else
                            {
                                NewClientSerial = ClientDb.ClientSerialCounter;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(request.Name))
                        {
                            Name = request.Name.ToLower();
                            var ExistNameClientDb = _unitOfWork.Clients.Find(a => a.Name.ToLower() == Name);
                            if (ExistNameClientDb != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Client Name Already Exist, Please Enter Another One!!";
                                Response.Errors.Add(error);
                            }
                            Name = request.Name;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Client Name Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (request.OwnerCoProfile != null)
                        {
                            if (request.OwnerCoProfile == true)
                            {
                                var IsAlreadyExist = _unitOfWork.Clients.Find(a => a.OwnerCoProfile == true);
                                if (IsAlreadyExist != null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Owner Client Already Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }


                    string Type = null;
                    long SalesPersonId = 0;
                    int NeedApproval = 0;
                    DateTime? FirstContractDate = null;
                    byte[] LogoBytes = null;
                    int FollowUpPeriod = 0;
                    var SupportedByCompanyRequest = false;
                    int BranchId = 0;
                    DateTime LastReportDateRequest = DateTime.Now;
                    if (request.Active != false)
                    {
                        if (request.Type != null)
                        {
                            Type = request.Type;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Type Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (request.SalesPersonID != 0)
                        {
                            SalesPersonId = request.SalesPersonID;
                            var salesPerson = _unitOfWork.Users.GetById(SalesPersonId);
                            if (salesPerson != null)
                            {
                                BranchId = salesPerson.BranchId ?? 0;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Sales Person Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Person Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (request.FollowUpPeriod != 0)
                        {
                            FollowUpPeriod = request.FollowUpPeriod;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "FollowUp Period Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (request.NeedApproval != null)
                        {
                            NeedApproval = (int)request.NeedApproval;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Need Approval Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        var NeedApprovalDb = 0;
                        var CreatorRoles = _unitOfWork.UserRoles.FindAll(a => a.UserId == UserID).ToList();
                        if (CreatorRoles.Where(a => a.RoleId == 1).FirstOrDefault() != null)
                        {
                            NeedApprovalDb = 0;
                        }
                        else if (CreatorRoles.Where(a => a.RoleId == 20).FirstOrDefault() != null)
                        {
                            NeedApprovalDb = 1;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This User (Created By Id) has no permission to Add New Client!";
                            Response.Errors.Add(error);
                        }

                        if (NeedApproval != NeedApprovalDb)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Need Approval Value";
                            Response.Errors.Add(error);
                        }

                        DateTime FirstContractDateRequest = DateTime.Now;
                        if (!string.IsNullOrEmpty(request.FirstContractDate))
                        {
                            if (!DateTime.TryParse(request.FirstContractDate, out FirstContractDateRequest))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err23";
                                error.ErrorMSG = "Invalid First Contract Date";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                FirstContractDate = FirstContractDateRequest;
                            }
                        }


                        DateTime? LastReportDate = null;
                        if (!string.IsNullOrEmpty(request.LastReportDate))
                        {
                            if (!DateTime.TryParse(request.LastReportDate, out LastReportDateRequest))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err23";
                                error.ErrorMSG = "Invalid Last Report Date";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                LastReportDate = LastReportDateRequest;
                            }
                        }

                        if (request.SupportedByCompany)
                        {
                            SupportedByCompanyRequest = true;

                            if (string.IsNullOrEmpty(request.SupportedBy))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err23";
                                error.ErrorMSG = "Please Enter Supported By Value Or Set Supported By Company to False!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            request.SupportedBy = null;
                        }

                        if (request.WebSite != null)
                        {
                            var ExistClientWebsite = _unitOfWork.VClientUseers.Find(a => a.WebSite.ToLower() == request.WebSite.ToLower());
                            if (ExistClientWebsite != null)
                            {
                                if (ClientId != null)
                                {
                                    if (ExistClientWebsite.Id != ClientId)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err1";
                                        error.ErrorMSG = "This Website Is Exist Before";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err1";
                                    error.ErrorMSG = "This Website Is Exist Before";
                                    Response.Errors.Add(error);
                                }

                            }
                        }

                        if (request.ClientAddresses != null)
                        {
                            if (request.ClientAddresses.Count > 0)
                            {
                                var counter = 0;
                                foreach (var Adrs in request.ClientAddresses)
                                {
                                    counter++;
                                    if (Adrs.Active != false)
                                    {

                                        int CountryId = 0;
                                        if (Adrs.CountryID != 0)
                                        {
                                            CountryId = Adrs.CountryID;
                                            var country = _unitOfWork.Countries.GetById(CountryId);
                                            if (country == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This Country Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Country ID Is Mandatory";
                                            Response.Errors.Add(error);
                                        }

                                        int GovernorateId = 0;
                                        if (Adrs.GovernorateID != 0)
                                        {
                                            GovernorateId = Adrs.GovernorateID;
                                            var governorate = _unitOfWork.Governorates.GetById(GovernorateId);
                                            if (governorate == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This City Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Governorate ID Is Mandatory";
                                            Response.Errors.Add(error);
                                        }

                                        long? AreaId = 0;
                                        if (Adrs.AreaID != 0)
                                        {
                                            AreaId = Adrs.AreaID;
                                            var area = _unitOfWork.Areas.GetById(AreaId ?? 0);
                                            if (area == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This Area Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Area ID Is Mandatory";
                                            Response.Errors.Add(error);
                                        }

                                        string Address = null;
                                        if (!string.IsNullOrEmpty(Adrs.Address))
                                        {
                                            Address = Adrs.Address;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Address Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                        }

                        if (request.ClientLandLines != null)
                        {
                            if (request.ClientLandLines.Count > 0)
                            {
                                var counter = 0;
                                foreach (var LND in request.ClientLandLines)
                                {
                                    counter++;

                                    string LandLine = null;
                                    if (!string.IsNullOrEmpty(LND.LandLine))
                                    {
                                        var ExistPhone = _unitOfWork.ClientPhones.FindAll(a => a.Phone == LND.LandLine).ToList();
                                        if (ExistPhone != null && ExistPhone.Any())
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err2";
                                            error.ErrorMSG = "Line: " + counter;
                                            Response.Errors.Add(error);
                                        }
                                        else
                                        {
                                            LandLine = LND.LandLine;
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Land Line Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }

                        if (request.ClientMobiles != null)
                        {
                            if (request.ClientMobiles.Count > 0)
                            {
                                var counter = 0;
                                foreach (var MOB in request.ClientMobiles)
                                {
                                    counter++;
                                    if (MOB.Active != false)
                                    {
                                        string Mobile = null;
                                        if (!string.IsNullOrEmpty(MOB.Mobile))
                                        {
                                            var ExistMobile = _unitOfWork.ClientMobiles.FindAll(a => a.Mobile == MOB.Mobile).ToList();
                                            if (ExistMobile != null && ExistMobile.Any())
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err3";
                                                error.ErrorMSG = "Line: " + counter;
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                Mobile = MOB.Mobile;
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Mobile Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                        }

                        if (request.ClientFaxes != null)
                        {
                            if (request.ClientFaxes.Count > 0)
                            {
                                var counter = 0;
                                foreach (var FX in request.ClientFaxes)
                                {
                                    counter++;

                                    string Fax = null;
                                    if (!string.IsNullOrEmpty(FX.Fax))
                                    {
                                        var ExistFax = _unitOfWork.ClientFaxes.FindAll(a => a.Fax == FX.Fax).ToList();
                                        if (ExistFax != null && ExistFax.Any())
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err4";
                                            error.ErrorMSG = "Line: " + counter;
                                            Response.Errors.Add(error);
                                        }
                                        else
                                        {
                                            Fax = FX.Fax;
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "Fax Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }

                        if (request.ClientPaymentTerms != null)
                        {
                            if (request.ClientPaymentTerms.Count > 0)
                            {
                                var counter = 0;
                                foreach (var PT in request.ClientPaymentTerms)
                                {
                                    counter++;

                                    if (string.IsNullOrEmpty(PT.Name))
                                    {

                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "PaymentTerm Name Is Mandatory";
                                        Response.Errors.Add(error);
                                    }

                                    if (PT.Percentage == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Line: " + counter + ", " + "PaymentTerm Percentage Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }

                        /*if (!string.IsNullOrEmpty(request.Logo))
                        {
                            LogoBytes = Convert.FromBase64String(request.Logo);
                            request.HasLogo = true;
                        }*/

                    }





                    if (Response.Result)
                    {
                        DateTime? openingBalanceDate = null;
                        if (!string.IsNullOrEmpty(request.OpeningBalanceDate))
                        {
                            openingBalanceDate = DateTime.Parse(request.OpeningBalanceDate);
                        }

                        if (ClientId != null && ClientId != 0) // DiActive or Update
                        {
                            var Clientdb = _unitOfWork.Clients.Find(x => x.Id == ClientId);
                            if (Clientdb != null)
                            {
                                if (request.Active == false) // want to Delete -> set Approval Rejected
                                {
                                    Clientdb.NeedApproval = 2;
                                }

                                // Update
                                Name = Name ?? Clientdb.Name;
                                Clientdb.Name = Name;
                                Clientdb.Type = request.Type;
                                Clientdb.Email = request.Email;
                                Clientdb.WebSite = request.WebSite;
                                Clientdb.SalesPersonId = request.SalesPersonID;
                                Clientdb.Note = request.Note;
                                Clientdb.Rate = request.Rate;
                                Clientdb.FirstContractDate = DateOnly.FromDateTime(FirstContractDate ?? DateTime.Now);
                                Clientdb.Logo = LogoBytes != null ? LogoBytes : Clientdb.Logo;
                                Clientdb.GroupName = request.GroupName;
                                Clientdb.BranchName = request.BranchName;
                                Clientdb.Consultant = request.Consultant;
                                Clientdb.FollowUpPeriod = FollowUpPeriod;
                                Clientdb.ConsultantType = request.ConsultantType;
                                Clientdb.SupportedByCompany = SupportedByCompanyRequest;
                                Clientdb.SupportedBy = request.SupportedBy;
                                Clientdb.HasLogo = Clientdb.HasLogo != null ? (request.HasLogo != null ? request.HasLogo != null : Clientdb.HasLogo) : request.HasLogo;
                                Clientdb.BranchId = BranchId != 0 ? BranchId : Clientdb.BranchId;
                                Clientdb.LastReportDate = LastReportDateRequest;
                                Clientdb.NeedApproval = request.NeedApproval;
                                Clientdb.ClientSerialCounter = request.ClientSerialCounter != null ? request.ClientSerialCounter : NewClientSerial;
                                Clientdb.OpeningBalance = request.OpeningBalance;
                                Clientdb.OpeningBalanceType = request.OpeningBalanceType;
                                Clientdb.OpeningBalanceDate = DateOnly.FromDateTime(DateTime.Now);
                                Clientdb.OpeningBalanceCurrencyId = request.OpeningBalanceCurrencyId;
                                Clientdb.DefaultDelivaryAndShippingMethodId = request.DefaultDelivaryAndShippingMethodId;
                                Clientdb.OtherDelivaryAndShippingMethodName = request.OtherDelivaryAndShippingMethodName;
                                Clientdb.CommercialRecord = request.CommercialRecord;
                                Clientdb.TaxCard = request.TaxCard;
                                Clientdb.ClientClassificationId = request.ClassificationId;
                                Clientdb.ClassificationComment = request.ClassificationComment;
                                Clientdb.PreparedSearchName = Common.string_compare_prepare_function(Name);
                                Clientdb.EnglishName = EnglishName;
                                if (request.HasLogo != null && request.HasLogo == false && Clientdb.LogoUrl != null)
                                {
                                    var oldpath = Path.Combine(_host.WebRootPath, Clientdb.LogoUrl);
                                    if (System.IO.File.Exists(oldpath))
                                    {
                                        System.IO.File.Delete(oldpath);
                                        Clientdb.LogoUrl = null;
                                    }
                                }
                                if (request.Logo != null)
                                {
                                    var fileExtension = request.Logo.FileName.Split('.').Last();
                                    var virtualPath = $@"Attachments\{validation.CompanyName}\Clients\Images\";
                                    var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Logo.FileName.Trim().Replace(" ", ""));
                                    var AttachPath = Common.SaveFileIFF(virtualPath, request.Logo, FileName, fileExtension, _host);
                                    Clientdb.LogoUrl = AttachPath;
                                }
                                var ClientUpdate = _unitOfWork.Complete();
                                if (ClientUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = ClientId ?? 0;

                                    var sales = _unitOfWork.VClientSalesPeople.FindAll(c => c.ClientId == ClientId).OrderByDescending(c => c.StartDate).FirstOrDefault();

                                    if (IsSalesPersonChanged)
                                    {
                                        if (sales != null)
                                        {
                                            var SalesPersonDb = _unitOfWork.ClientSalesPeople.Find(a => a.SalesPersonId == sales.SalesPersonId && a.ClientId == ClientId);
                                            if (SalesPersonDb != null)
                                            {
                                                SalesPersonDb.EndDate = DateTime.Now;
                                                SalesPersonDb.Current = false;
                                                SalesPersonDb.LeaveComment = request.ChangeSalesPersonComment;
                                                SalesPersonDb.ModifiedBy = UserID;
                                                SalesPersonDb.ModifiedDate = DateTime.Now;
                                                _unitOfWork.ClientSalesPeople.Update(SalesPersonDb);
                                                _unitOfWork.Complete();

                                            }
                                        }


                                        //ObjectParameter SalesPersonInsertId = new ObjectParameter("ID", typeof(long));
                                        var SalesPersonInsert = new ClientSalesPerson()
                                        {
                                            ClientId = (long)ClientId,
                                            SalesPersonId = request.SalesPersonID,
                                            StartDate = DateTime.Now,
                                            EndDate = null,
                                            Current = true,
                                            LeaveComment = request.ChangeSalesPersonComment,
                                            CreatedBy = UserID,
                                            CreationDate = DateTime.Now,
                                            ModifiedDate = DateTime.Now,
                                            ModifiedBy = UserID
                                        };
                                        _unitOfWork.ClientSalesPeople.Add(SalesPersonInsert);
                                        _unitOfWork.Complete();
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Client!!";
                                    Response.Errors.Add(error);
                                }

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Client Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            long? ApprovedBy = null;
                            // Insert
                            if (request.NeedApproval != null && request.NeedApproval == 0)
                            {
                                ApprovedBy = UserID;
                            }
                            var Clientdb = new Client();
                            Clientdb.Name = Name;
                            Clientdb.Type = request.Type;
                            Clientdb.Email = request.Email;
                            Clientdb.WebSite = request.WebSite;
                            Clientdb.CreatedBy = UserID;
                            Clientdb.CreationDate = DateTime.Now;
                            Clientdb.SalesPersonId = request.SalesPersonID;
                            Clientdb.Note = request.Note;
                            Clientdb.Rate = request.Rate;
                            Clientdb.FirstContractDate = DateOnly.FromDateTime(FirstContractDate ?? DateTime.Now);
                            Clientdb.Logo = LogoBytes;
                            Clientdb.GroupName = request.GroupName;
                            Clientdb.BranchName = request.BranchName;
                            Clientdb.Consultant = request.Consultant;
                            Clientdb.FollowUpPeriod = FollowUpPeriod;
                            Clientdb.ConsultantType = request.ConsultantType;
                            Clientdb.SupportedByCompany = SupportedByCompanyRequest;
                            Clientdb.SupportedBy = request.SupportedBy;
                            Clientdb.HasLogo = request.HasLogo;
                            Clientdb.BranchId = BranchId;
                            Clientdb.LastReportDate = LastReportDateRequest;
                            Clientdb.NeedApproval = request.NeedApproval;
                            Clientdb.ClientSerialCounter = NewClientSerial;
                            Clientdb.OpeningBalance = request.OpeningBalance;
                            Clientdb.OpeningBalanceType = request.OpeningBalanceType;
                            Clientdb.OpeningBalanceDate = DateOnly.FromDateTime(DateTime.Now);
                            Clientdb.OpeningBalanceCurrencyId = request.OpeningBalanceCurrencyId;
                            Clientdb.DefaultDelivaryAndShippingMethodId = request.DefaultDelivaryAndShippingMethodId;
                            Clientdb.OtherDelivaryAndShippingMethodName = request.OtherDelivaryAndShippingMethodName;
                            Clientdb.CommercialRecord = request.CommercialRecord;
                            Clientdb.TaxCard = request.TaxCard;
                            Clientdb.OwnerCoProfile = request.OwnerCoProfile;
                            Clientdb.ApprovedBy = ApprovedBy;
                            Clientdb.ClientClassificationId = request.ClassificationId;
                            Clientdb.ClassificationComment = request.ClassificationComment;
                            Clientdb.PreparedSearchName = Common.string_compare_prepare_function(Name);
                            Clientdb.EnglishName = EnglishName;
                            if (request.Logo != null)
                            {
                                var fileExtension = request.Logo.FileName.Split('.').Last();
                                var virtualPath = $@"Attachments\{validation.CompanyName}\Clients\Images\";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Logo.FileName.Trim().Replace(" ", ""));
                                var AttachPath = Common.SaveFileIFF(virtualPath, request.Logo, FileName, fileExtension, _host);
                                Clientdb.LogoUrl = AttachPath;
                            }
                            _unitOfWork.Clients.Add(Clientdb);


                            var ClientInsert = _unitOfWork.Complete();

                            if (ClientInsert > 0)
                            {
                                ClientId = Clientdb.Id; //(long)ClientInsertedId.Value;
                                Response.Result = true;
                                Response.ID = ClientId ?? 0;

                                var SalesPersonInsert = new ClientSalesPerson()
                                {
                                    ClientId = (long)ClientId,
                                    SalesPersonId = request.SalesPersonID,
                                    StartDate = DateTime.Now,
                                    EndDate = null,
                                    Current = true,
                                    LeaveComment = request.ChangeSalesPersonComment,
                                    CreatedBy = UserID,
                                    CreationDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now,
                                    ModifiedBy = UserID
                                };
                                _unitOfWork.ClientSalesPeople.Add(SalesPersonInsert);
                                _unitOfWork.Complete();
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Client!!";
                                Response.Errors.Add(error);
                            }

                        }

                        if (ClientId != 0 && ClientId != null)
                        {
                            //Add-Edit supplier Speciality
                            if (request.ClientSpecialities != null)
                            {
                                if (request.ClientSpecialities.Count > 0)
                                {
                                    var counter = 0;
                                    foreach (var SPC in request.ClientSpecialities)
                                    {
                                        counter++;
                                        int SpecialityId = 0;
                                        if (SPC.SpecialityID != 0)
                                        {
                                            SpecialityId = SPC.SpecialityID;
                                            var speciality = _unitOfWork.Specialities.GetById(SpecialityId);
                                            if (speciality == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This Speciality Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Line: " + counter + ", " + "Speciality Id Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                                if (Response.Result)
                                {
                                    var ClientSpecialitiesDB = _unitOfWork.ClientSpecialities.FindAll(a => a.ClientId == ClientId).ToList();
                                    if (ClientSpecialitiesDB != null && ClientSpecialitiesDB.Count > 0)
                                    {
                                        foreach (var SPC in ClientSpecialitiesDB)
                                        {
                                            _unitOfWork.ClientSpecialities.Delete(SPC);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                    foreach (var SPC in request.ClientSpecialities)
                                    {
                                        var ClientMobileInsert = new ClientSpeciality()
                                        {
                                            ClientId = (long)ClientId,
                                            SpecialityId = SPC.SpecialityID,
                                            CreatedBy = UserID,
                                            CreationDate = DateTime.Now,
                                            ModifiedBy = UserID,
                                            Modified = DateTime.Now,
                                            Active = true
                                        };
                                        _unitOfWork.ClientSpecialities.Add(ClientMobileInsert); _unitOfWork.Complete();
                                        //}
                                    }
                                }
                            }

                            //Add-Edit supplier Addresses
                            if (request.ClientAddresses != null)
                            {
                                if (request.ClientAddresses.Count > 0)
                                {
                                    foreach (var Adrs in request.ClientAddresses)
                                    {
                                        int BuildingNumber = 0;
                                        int Floor = 0;
                                        if (!string.IsNullOrEmpty(Adrs.BuildingNumber) && int.TryParse(Adrs.BuildingNumber, out BuildingNumber))
                                        {
                                            int.TryParse(Adrs.BuildingNumber, out BuildingNumber);
                                        }
                                        if (!string.IsNullOrEmpty(Adrs.Floor) && int.TryParse(Adrs.Floor, out Floor))
                                        {
                                            int.TryParse(Adrs.Floor, out Floor);
                                        }


                                        if (Adrs.ID != null && Adrs.ID != 0) // Update or Delete
                                        {
                                            var AddressDb = _unitOfWork.ClientAddresses.Find(x => x.Id == Adrs.ID);
                                            if (AddressDb != null)
                                            {
                                                if (Adrs.Active == false) // Delete
                                                {
                                                    _unitOfWork.ClientAddresses.Delete(AddressDb);
                                                }
                                                else
                                                {

                                                    // Update
                                                    AddressDb.ClientId = (long)ClientId;
                                                    AddressDb.CountryId = Adrs.CountryID;
                                                    AddressDb.GovernorateId = Adrs.GovernorateID;
                                                    AddressDb.Address = Adrs.Address;
                                                    AddressDb.ModifiedBy = UserID;
                                                    AddressDb.Modified = DateTime.Now;
                                                    AddressDb.Active = true;
                                                    AddressDb.BuildingNumber = BuildingNumber.ToString();
                                                    AddressDb.Floor = Floor.ToString();
                                                    AddressDb.Description = Adrs.Description;
                                                    AddressDb.AreaId = Adrs.AreaID;
                                                    _unitOfWork.ClientAddresses.Update(AddressDb);
                                                    _unitOfWork.Complete();
                                                }
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Address Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientAddressInsertedId = new ObjectParameter("ID", typeof(long));
                                            var ClientInsert = new ClientAddress()
                                            {
                                                ClientId = (long)ClientId,
                                                CountryId = Adrs.CountryID,
                                                GovernorateId = Adrs.GovernorateID,
                                                Address = Adrs.Address,
                                                CreatedBy = UserID,
                                                CreationDate = DateTime.Now,
                                                ModifiedBy = UserID,
                                                Modified = DateTime.Now,
                                                Active = true,
                                                BuildingNumber = BuildingNumber.ToString(),
                                                Floor = Floor.ToString(),
                                                Description = Adrs.Description,
                                                AreaId = Adrs.AreaID
                                            };
                                            _unitOfWork.ClientAddresses.Add(ClientInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ClientId == null || ClientId == 0)
                                {
                                    if (request.Type != null && request.Type.ToLower() == "individual")
                                    {
                                        // Insert
                                        //ObjectParameter ClientAddressInsertedId = new ObjectParameter("ID", typeof(long));
                                        /*var ClientInsert = _Context.proc_ClientAddressInsert(ClientAddressInsertedId,
                                                                                                        ClientId,
                                                                                                        1,
                                                                                                        1,
                                                                                                        "",
                                                                                                        validation.userID,
                                                                                                        DateTime.Now,
                                                                                                        null,
                                                                                                        null,
                                                                                                        true,
                                                                                                        "0",
                                                                                                        "0",
                                                                                                        "",
                                                                                                        null
                                                                                                        );*/
                                        var ClientInsert = new ClientAddress()
                                        {
                                            ClientId = (long)ClientId,
                                            CountryId = 1,
                                            GovernorateId = 1,
                                            Address = "",
                                            CreatedBy = UserID,
                                            CreationDate = DateTime.Now,
                                            ModifiedBy = null,
                                            Modified = null,
                                            Active = true,
                                            BuildingNumber = "0",
                                            Floor = "0",
                                            Description = "",
                                            AreaId = null
                                        };
                                        _unitOfWork.ClientAddresses.Add(ClientInsert);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }

                            //Add-Edit supplier Land Lines
                            if (request.ClientLandLines != null)
                            {
                                if (request.ClientLandLines.Count > 0)
                                {
                                    foreach (var LND in request.ClientLandLines)
                                    {
                                        if (LND.ID != null && LND.ID != 0)
                                        {
                                            var ClientLandLineDb = _Context.ClientPhones.Find(LND.ID);
                                            if (ClientLandLineDb != null)
                                            {
                                                // Update
                                                /*var ClientLandLineUpdate = _Context.proc_ClientPhoneUpdate(LND.ID,
                                                                                                                    ClientId,
                                                                                                                    LND.LandLine,
                                                                                                                    ClientLandLineDb.CreatedBy,
                                                                                                                    ClientLandLineDb.CreationDate,
                                                                                                                    long.Parse(Encrypt_Decrypt.Decrypt(LND.ModifiedBy, key)),
                                                                                                                    DateTime.Now,
                                                                                                                    true
                                                                                                                    );*/
                                                ClientLandLineDb.ClientId = (long)ClientId;
                                                ClientLandLineDb.Phone = LND.LandLine;
                                                ClientLandLineDb.CreatedBy = ClientLandLineDb.CreatedBy;
                                                ClientLandLineDb.CreationDate = ClientLandLineDb.CreationDate;
                                                ClientLandLineDb.ModifiedBy = UserID;
                                                ClientLandLineDb.Modified = DateTime.Now;
                                                ClientLandLineDb.Active = true;
                                                _unitOfWork.ClientPhones.Update(ClientLandLineDb);
                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Land Line Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientLandLineInsertedId = new ObjectParameter("ID", typeof(long));
                                            var ClientLandLineInsert = new ClientPhone()
                                            {
                                                ClientId = (long)ClientId,
                                                Phone = LND.LandLine,
                                                CreatedBy = UserID,
                                                CreationDate = DateTime.Now,
                                                ModifiedBy = UserID,
                                                Modified = DateTime.Now,
                                                Active = true
                                            };
                                            _unitOfWork.ClientPhones.Add(ClientLandLineInsert); _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }

                            //Add-Edit supplier Mobiles
                            if (request.ClientMobiles != null)
                            {
                                if (request.ClientMobiles.Count > 0)
                                {
                                    foreach (var MOB in request.ClientMobiles)
                                    {
                                        if (MOB.ID != null && MOB.ID != 0) // Delete Or Update
                                        {
                                            var ClientMobileDb = _unitOfWork.ClientMobiles.Find(x => x.Id == MOB.ID);
                                            if (ClientMobileDb != null)
                                            {
                                                if (MOB.Active == false) // Delete 
                                                {
                                                    _unitOfWork.ClientMobiles.Delete(ClientMobileDb);
                                                }
                                                else
                                                {

                                                    // Update
                                                    /*var ClientMobileUpdate = _Context.proc_ClientMobileUpdate(MOB.ID,
                                                                                                                ClientId,
                                                                                                                MOB.Mobile,
                                                                                                                ClientMobileDb.CreatedBy,
                                                                                                                ClientMobileDb.CreationDate,
                                                                                                                long.Parse(Encrypt_Decrypt.Decrypt(MOB.ModifiedBy, key)),
                                                                                                                DateTime.Now,
                                                                                                                true
                                                                                                                );*/
                                                    ClientMobileDb.ClientId = (long)ClientId;
                                                    ClientMobileDb.Mobile = MOB.Mobile;
                                                    ClientMobileDb.ModifiedBy = UserID;
                                                    ClientMobileDb.Modified = DateTime.Now;
                                                    ClientMobileDb.Active = true;

                                                    _unitOfWork.ClientMobiles.Update(ClientMobileDb);
                                                    _unitOfWork.Complete();
                                                }
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Mobile Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientMobileInsertedId = new ObjectParameter("ID", typeof(long));
                                            var ClientMobileInsert = new ClientMobile()
                                            {
                                                ClientId = (long)ClientId,
                                                Mobile = MOB.Mobile,
                                                CreatedBy = UserID,
                                                CreationDate = DateTime.Now,
                                                ModifiedBy = UserID,
                                                Modified = DateTime.Now,
                                                Active = true
                                            };
                                            _unitOfWork.ClientMobiles.Add(ClientMobileInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }

                            //Add-Edit supplier Faxes
                            if (request.ClientFaxes != null)
                            {
                                if (request.ClientFaxes.Count > 0)
                                {
                                    foreach (var FX in request.ClientFaxes)
                                    {
                                        if (FX.ID != null && FX.ID != 0)
                                        {
                                            var ClientFaxDb = _Context.ClientFaxes.Find(FX.ID);
                                            if (ClientFaxDb != null)
                                            {
                                                // Update
                                                /*var ClientFaxUpdate = _Context.proc_ClientFaxUpdate(FX.Id,
                                                                                                                    ClientId,
                                                                                                                    FX.Fax,
                                                                                                                    ClientFaxDb.CreatedBy,
                                                                                                                    ClientFaxDb.CreationDate,
                                                                                                                    long.Parse(Encrypt_Decrypt.Decrypt(FX.ModifiedBy, key)),
                                                                                                                    DateTime.Now,
                                                                                                                    true
                                                                                                                    );*/
                                                ClientFaxDb.ClientId = (long)ClientId;
                                                ClientFaxDb.Fax = FX.Fax;
                                                ClientFaxDb.CreatedBy = ClientFaxDb.CreatedBy;
                                                ClientFaxDb.CreationDate = ClientFaxDb.CreationDate;
                                                ClientFaxDb.ModifiedBy = UserID;
                                                ClientFaxDb.Modified = DateTime.Now;
                                                ClientFaxDb.Active = true;

                                                _unitOfWork.ClientFaxes.Update(ClientFaxDb);
                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Fax Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientFaxInsertedId = new ObjectParameter("ID", typeof(long));
                                            var ClientFaxInsert = new ClientFax()
                                            {
                                                ClientId = (long)ClientId,
                                                Fax = FX.Fax,
                                                CreatedBy = UserID,
                                                CreationDate = DateTime.Now,
                                                ModifiedBy = UserID,
                                                Modified = DateTime.Now,
                                                Active = true
                                            };
                                            _unitOfWork.ClientFaxes.Add(ClientFaxInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }

                            //Add-Edit supplier Payment Terms
                            if (request.ClientPaymentTerms != null)
                            {
                                if (request.ClientPaymentTerms.Count > 0)
                                {
                                    foreach (var PT in request.ClientPaymentTerms)
                                    {
                                        if (PT.ID != null && PT.ID != 0)
                                        {
                                            var ClientPaymentTermDb = _Context.ClientPaymentTerms.Find(PT.ID);
                                            if (ClientPaymentTermDb != null)
                                            {
                                                ClientPaymentTermDb.ClientId = (long)ClientId;
                                                ClientPaymentTermDb.Name = PT.Name;
                                                ClientPaymentTermDb.Description = PT.Description;
                                                ClientPaymentTermDb.Percentage = PT.Percentage ?? 0;
                                                ClientPaymentTermDb.Time = null;
                                                ClientPaymentTermDb.ModifiedBy = UserID;
                                                ClientPaymentTermDb.ModifiedDate = DateTime.Now;

                                                _unitOfWork.ClientPaymentTerms.Update(ClientPaymentTermDb);
                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Payment Term Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientPaymentTermInsertedId = new ObjectParameter("ID", typeof(long));
                                            var ClientPaymentTermInsert = new ClientPaymentTerm()
                                            {
                                                ClientId = (long)ClientId,
                                                Name = PT.Name,
                                                Description = PT.Description,
                                                Percentage = PT.Percentage ?? 0,
                                                Time = null,
                                                Active = true,
                                                CreatedBy = UserID,
                                                CreationDate = DateTime.Now,
                                            };

                                            _unitOfWork.ClientPaymentTerms.Add(ClientPaymentTermInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }

                            //Add-Edit supplier Bank Accounts
                            if (request.ClientBankAccounts != null)
                            {
                                if (request.ClientBankAccounts.Count > 0)
                                {
                                    foreach (var BA in request.ClientBankAccounts)
                                    {
                                        if (BA.ID != null && BA.ID != 0)
                                        {
                                            var ClientBankAccountDb = _Context.ClientBankAccounts.Find(BA.ID);
                                            if (ClientBankAccountDb != null)
                                            {
                                                ClientBankAccountDb.ClientId = (long)ClientId;
                                                ClientBankAccountDb.BankDetails = BA.BankDetails;
                                                ClientBankAccountDb.BeneficiaryName = BA.BeneficiaryName;
                                                ClientBankAccountDb.Iban = BA.IBAN;
                                                ClientBankAccountDb.SwiftCode = BA.SwiftCode;
                                                ClientBankAccountDb.Account = BA.Account;
                                                ClientBankAccountDb.Active = true;
                                                ClientBankAccountDb.ModifiedBy = UserID;
                                                ClientBankAccountDb.ModifiedDate = DateTime.Now;

                                                _unitOfWork.ClientBankAccounts.Update(ClientBankAccountDb);
                                                _unitOfWork.Complete();
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "This Bank Account Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            // Insert
                                            //ObjectParameter ClientBankAccountInsertedId = new ObjectParameter("ID", typeof(long));
                                            var ClientBankAccountInsert = new ClientBankAccount()
                                            {
                                                ClientId = (long)ClientId,
                                                BankDetails = BA.BankDetails,
                                                BeneficiaryName = BA.BeneficiaryName,
                                                Iban = BA.IBAN,
                                                SwiftCode = BA.SwiftCode,
                                                Account = BA.Account,
                                                Active = true,
                                                CreatedBy = UserID,
                                                CreationDate = DateTime.Now,
                                            };
                                            _unitOfWork.ClientBankAccounts.Add(ClientBankAccountInsert);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                            }
                        }



                        // For Update(Diactivate or Remove) supplier -  supplier Mobile - supplier Contact Person - supplier Address
                        _unitOfWork.Complete();
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

        public BaseResponseWithId<long> DeleteClient(long ClientId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (Response.Result)
                {
                    if (ClientId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "client Id Is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var client = _Context.Clients.Find(ClientId);
                    if (client == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Client not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    _unitOfWork.Clients.Delete(client);
                    _unitOfWork.Complete();
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

        public async Task<GetClientData> GetClientDataResponse([FromHeader] bool? OwnerCoProfile, [FromHeader] long? ClientId)
        {
            GetClientData Response = new GetClientData();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Client ClientDataDB = new Client();
                    if (OwnerCoProfile != null)
                    {
                        if ((bool)OwnerCoProfile)
                        {
                            ClientDataDB = (await _unitOfWork.Clients.FindAllAsync(a => a.OwnerCoProfile == true, includes: new[] { "DefaultDelivaryAndShippingMethod" })).FirstOrDefault();

                            if (ClientDataDB == null)
                            {
                                Response.IsExistClientProfile = false;
                                return Response;
                            }
                            else
                            {
                                Response.IsExistClientProfile = true;
                                ClientId = ClientDataDB.Id;
                            }
                        }
                        else
                        {
                            if (ClientId != null)
                            {
                                ClientDataDB = (await _unitOfWork.Clients.FindAllAsync(a => a.Id == ClientId, includes: new[] { "DefaultDelivaryAndShippingMethod" })).FirstOrDefault();
                                if (ClientDataDB == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err69";
                                    error.ErrorMSG = "This Client Doesn't Exist";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err69";
                                error.ErrorMSG = "Invalid Client Id";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                    }

                    if (ClientDataDB.Id != 0)
                    {
                        var salesPersonName = "";
                        var salesPerson = _unitOfWork.Users.GetById(ClientDataDB.SalesPersonId);
                        if (salesPerson != null)
                        {
                            salesPersonName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                        }

                        var ClientDataObj = new ClientData()
                        {
                            Id = ClientDataDB.Id,
                            CreatedBy = HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(ClientDataDB.CreatedBy.ToString(), key)),
                            BranchID = ClientDataDB.BranchId,
                            BranchName = ClientDataDB.BranchName,
                            Consultant = ClientDataDB.Consultant,
                            ConsultantType = ClientDataDB.ConsultantType,
                            Email = ClientDataDB.Email,
                            FirstContractDate = ClientDataDB.FirstContractDate.ToString(),
                            FollowUpPeriod = ClientDataDB.FollowUpPeriod,
                            GroupName = ClientDataDB.GroupName,
                            LastReportDate = ClientDataDB.LastReportDate.ToString(),
                            Name = ClientDataDB.Name,
                            EnglishName = ClientDataDB.EnglishName,
                            NeedApproval = ClientDataDB.NeedApproval,
                            Note = ClientDataDB.Note,
                            Rate = ClientDataDB.Rate,
                            SalesPersonID = ClientDataDB.SalesPersonId,
                            SalesPersonName = salesPersonName,
                            SupportedBy = ClientDataDB.SupportedBy,
                            Type = ClientDataDB.Type,
                            WebSite = ClientDataDB.WebSite,
                            SupportedByCompany = ClientDataDB.SupportedByCompany ?? false,
                            CommercialRecord = ClientDataDB.CommercialRecord,
                            DefaultDelivaryAndShippingMethodId = ClientDataDB.DefaultDelivaryAndShippingMethodId,
                            DefaultDelivaryAndShippingMethodName = ClientDataDB.DefaultDelivaryAndShippingMethodId != null ? ClientDataDB.DefaultDelivaryAndShippingMethod?.Name : null,
                            ClientSerialCounter = ClientDataDB.ClientSerialCounter,
                            OpeningBalance = ClientDataDB.OpeningBalance,
                            OpeningBalanceCurrencyId = ClientDataDB.OpeningBalanceCurrencyId,
                            OpeningBalanceCurrencyName = ClientDataDB.OpeningBalanceCurrencyId != null ? ClientDataDB.OpeningBalanceCurrency?.ShortName : null,
                            OpeningBalanceDate = ClientDataDB.OpeningBalanceDate != null ? ClientDataDB.OpeningBalanceDate.ToString().Split(' ')[0] : null,
                            OpeningBalanceType = ClientDataDB.OpeningBalanceType,
                            OtherDelivaryAndShippingMethodName = ClientDataDB.OtherDelivaryAndShippingMethodName?.Trim(),
                            OwnerCoProfile = ClientDataDB.OwnerCoProfile,
                            TaxCard = ClientDataDB.TaxCard,
                            ApprovedById = ClientDataDB.ApprovedBy,
                            ApprovedByName = ClientDataDB.ApprovedBy != null ? ClientDataDB.ApprovedByNavigation?.FirstName + " " + ClientDataDB.ApprovedByNavigation?.LastName : null,
                            ClassificationId = ClientDataDB.ClientClassificationId,
                            ClassificationName = ClientDataDB.ClientClassification?.Name,
                            ClassificationComment = ClientDataDB.ClassificationComment,
                            RegistrationDate = ClientDataDB.CreationDate.ToShortDateString(),
                            LogoURL = ClientDataDB.LogoUrl != null ? Globals.baseURL + @"/" + ClientDataDB.LogoUrl : null
                        };
                        /*if (ClientDataDB.Logo != null)
                        {
                            ClientDataObj.Logo = Globals.baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(ClientDataObj.Id.ToString(), key)) + "&type=client&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();
                        }*/

                        var ClientSpecialitiesDB = (await _unitOfWork.VClientSpecialities.FindAllAsync(a => a.ClientId == ClientDataDB.Id && a.Active == true)).ToList();
                        if (ClientSpecialitiesDB != null && ClientSpecialitiesDB.Count() > 0)
                        {
                            var ClientSpecialitiesList = new List<GetClientSpeciality>();
                            foreach (var SPC in ClientSpecialitiesDB)
                            {

                                var ClientSpeciality = new GetClientSpeciality()
                                {
                                    ID = SPC.Id,
                                    SpecialityID = SPC.SpecialityId,
                                    SpecialityName = SPC.Speciality
                                };
                                ClientSpecialitiesList.Add(ClientSpeciality);
                            }
                            ClientDataObj.ClientSpecialities = ClientSpecialitiesList;
                        }

                        Response.ClientsData = ClientDataObj;

                        var ClientAddressesDB = (await _unitOfWork.ClientAddresses.FindAllAsync(a => a.ClientId == ClientDataDB.Id && a.Active == true, new[] { "Governorate", "Country", "Area" })).ToList();
                        if (ClientAddressesDB != null && ClientAddressesDB.Count() > 0)
                        {
                            var ClientAddressesList = new List<GetClientAddress>();
                            foreach (var Adrs in ClientAddressesDB)
                            {

                                var ClientAddress = new GetClientAddress()
                                {
                                    ID = Adrs.Id,
                                    Address = Adrs.Address,
                                    CountryID = Adrs.CountryId,
                                    CountryName = Adrs.Country.Name,
                                    GovernorateID = Adrs.GovernorateId,
                                    GovernorateName = Adrs.Governorate.Name,
                                    AreaID = Adrs.AreaId,
                                    AreaName = Adrs.Area?.Name,
                                    BuildingNumber = Adrs.BuildingNumber,
                                    Description = Adrs.Description,
                                    Floor = Adrs.Floor,
                                    longtitud = Adrs.Longtitud,
                                    latitude = Adrs.Latitude
                                };
                                ClientAddressesList.Add(ClientAddress);
                            }
                            Response.ClientAddressData = ClientAddressesList;
                        }

                        var ClientContactPersonsDB = _unitOfWork.ClientContactPeople.FindAll(a => a.ClientId == ClientDataDB.Id && a.Active == true).ToList();
                        if (ClientContactPersonsDB != null && ClientContactPersonsDB.Count > 0)
                        {
                            var ClientContactPersonsList = new List<GetClientContactPerson>();
                            foreach (var CP in ClientContactPersonsDB)
                            {

                                var ClientContactPerson = new GetClientContactPerson()
                                {
                                    ID = CP.Id,
                                    Name = CP.Name,
                                    Email = CP.Email,
                                    Location = CP.Location,
                                    Mobile = CP.Mobile,
                                    Title = CP.Title
                                };
                                ClientContactPersonsList.Add(ClientContactPerson);
                            }
                            Response.ClientContactPersonData = ClientContactPersonsList;
                        }

                        var ClientMobileDB = (await _unitOfWork.ClientMobiles.FindAllAsync(a => a.ClientId == ClientDataDB.Id && a.Active == true)).ToList();
                        if (ClientMobileDB != null && ClientMobileDB.Count() > 0)
                        {
                            var ClientMobilesList = new List<GetClientMobile>();
                            foreach (var MOB in ClientMobileDB)
                            {

                                var ClientMobile = new GetClientMobile()
                                {
                                    ID = MOB.Id,
                                    Mobile = MOB.Mobile
                                };
                                ClientMobilesList.Add(ClientMobile);
                            }
                            Response.ClientMobileData = ClientMobilesList;
                        }

                        var ClientLandLinesDB = (await _unitOfWork.ClientPhones.FindAllAsync(a => a.ClientId == ClientDataDB.Id && a.Active == true)).ToList();
                        if (ClientLandLinesDB != null && ClientLandLinesDB.Count() > 0)
                        {
                            var ClientLandLinesList = new List<GetClientLandLine>();
                            foreach (var LND in ClientLandLinesDB)
                            {
                                var ClientLandLine = new GetClientLandLine()
                                {
                                    ID = LND.Id,
                                    LandLine = LND.Phone
                                };
                                ClientLandLinesList.Add(ClientLandLine);
                            }
                            Response.ClientLandLineData = ClientLandLinesList;
                        }

                        var ClientFaxesDB = (await _unitOfWork.ClientFaxes.FindAllAsync(a => a.ClientId == ClientDataDB.Id && a.Active == true)).ToList();
                        if (ClientFaxesDB != null && ClientFaxesDB.Count > 0)
                        {
                            var ClientFaxesList = new List<GetClientFax>();
                            foreach (var FX in ClientFaxesDB)
                            {
                                var ClientFax = new GetClientFax()
                                {
                                    ID = FX.Id,
                                    Fax = FX.Fax
                                };
                                ClientFaxesList.Add(ClientFax);
                            }
                            Response.ClientFaxData = ClientFaxesList;
                        }

                        var ClientPaymentTermsDB = _unitOfWork.ClientPaymentTerms.FindAll(a => a.ClientId == ClientDataDB.Id && a.Active == true).ToList();
                        if (ClientPaymentTermsDB != null && ClientPaymentTermsDB.Count > 0)
                        {
                            var ClientPaymentTermsList = ClientPaymentTermsDB.Select(PT => new GetClientPaymentTerm
                            {
                                ID = PT.Id,
                                Name = PT.Name,
                                Description = PT.Description,
                                Percentage = PT.Percentage
                            }).ToList();

                            Response.ClientPaymentTermData = ClientPaymentTermsList;
                        }

                        var ClientBankAccountDB = _unitOfWork.ClientBankAccounts.FindAll(a => a.ClientId == ClientDataDB.Id && a.Active == true).ToList();
                        if (ClientBankAccountDB != null && ClientBankAccountDB.Count > 0)
                        {
                            var ClientBankAccountList = ClientBankAccountDB.Select(BA => new GetClientBankAccount
                            {
                                ID = BA.Id,
                                Account = BA.Account,
                                BankDetails = BA.BankDetails,
                                BeneficiaryName = BA.BeneficiaryName,
                                IBAN = BA.Iban,
                                SwiftCode = BA.SwiftCode
                            }).ToList();

                            Response.ClientBankAccountData = ClientBankAccountList;
                        }

                        Response.ClientSalesPersonHistory = new List<GetClientSalesPersonHistory>();
                        var ClientSalesPersonHistory = _unitOfWork.VClientSalesPeople.FindAll(c => c.ClientId == ClientId).ToList();
                        if (ClientSalesPersonHistory != null && ClientSalesPersonHistory.Count > 0)
                        {
                            var ClientSalesPersonList = ClientSalesPersonHistory.Select(CSP => new GetClientSalesPersonHistory
                            {
                                ID = CSP.Id,
                                ChangeReason = CSP.LeaveComment,
                                Current = CSP.Current,
                                EndDate = CSP.EndDate != null ? CSP.EndDate.ToString().Split(' ')[0] : null,
                                StartDate = CSP.StartDate != null ? CSP.StartDate.ToString().Split(' ')[0] : null,
                                SalesPersonId = CSP.SalesPersonId,
                                SalesPersonName = CSP.FirstName + " " + CSP.LastName
                            }).ToList();

                            Response.ClientSalesPersonHistory = ClientSalesPersonList;
                        }

                        var ClientConsultantDataDB = _unitOfWork.ClientConsultants.FindAll(a => a.ClientId == ClientDataDB.Id && a.Active == true).FirstOrDefault();
                        if (ClientConsultantDataDB != null)
                        {
                            var ClientConsultantDataObj = new ClientConsultantData()
                            {
                                Id = ClientConsultantDataDB.Id,
                                ClientId = ClientConsultantDataDB.ClientId,
                                Company = ClientConsultantDataDB.Company,
                                ConsultantFor = ClientConsultantDataDB.ConsultantFor,
                                ConsultantName = ClientConsultantDataDB.ConsultantName
                            };

                            var ClientConsultantSpecialitiesDB = _unitOfWork.ClientConsultantSpecialilties.FindAll(a => a.ConsultantId == ClientConsultantDataDB.Id && a.Active == true).ToList();
                            if (ClientConsultantSpecialitiesDB != null && ClientConsultantSpecialitiesDB.Count > 0)
                            {
                                var ClientConsultantSpecialitiesList = new List<ConsultantSpeciality>();
                                foreach (var SPC in ClientConsultantSpecialitiesDB)
                                {
                                    var SpecialityNameDB = _Context.Specialities.Find(SPC.SpecialityId)?.Name;
                                    var ClientConsultantSpeciality = new ConsultantSpeciality()
                                    {
                                        ID = SPC.Id,
                                        SpecialityID = SPC.SpecialityId,
                                        SpecialityName = SpecialityNameDB
                                    };
                                    ClientConsultantSpecialitiesList.Add(ClientConsultantSpeciality);
                                }
                                ClientConsultantDataObj.ConsultantSpecialities = ClientConsultantSpecialitiesList;
                            }

                            var ClientConsultantAddressesDB = (await _unitOfWork.VClientConsultantAddresses.FindAllAsync(a => a.ConsultantId == ClientConsultantDataDB.Id && a.Active == true)).ToList();
                            if (ClientConsultantAddressesDB != null && ClientConsultantAddressesDB.Count > 0)
                            {
                                var ClientConsultantAddressesList = new List<ConsultantAddress>();
                                foreach (var Adrs in ClientConsultantAddressesDB)
                                {
                                    var ClientConsultantAddress = new ConsultantAddress()
                                    {
                                        ID = Adrs.Id,
                                        Address = Adrs.Address,
                                        CountryID = Adrs.CountryId,
                                        CountryName = Adrs.Country,
                                        GovernorateID = Adrs.GovernorateId,
                                        GovernorateName = Adrs.Governorate,
                                        BuildingNumber = Adrs.BuildingNumber,
                                        Description = Adrs.Description,
                                        Floor = Adrs.Floor
                                    };
                                    ClientConsultantAddressesList.Add(ClientConsultantAddress);
                                }
                                ClientConsultantDataObj.ConsultantAddresses = ClientConsultantAddressesList;
                            }

                            var ClientConsultantMobileDB = _unitOfWork.ClientConsultantMobiles.FindAll(a => a.ConsultantId == ClientConsultantDataDB.Id && a.Active == true).ToList();
                            if (ClientConsultantMobileDB != null && ClientConsultantMobileDB.Count > 0)
                            {
                                var ClientConsultantMobilesList = new List<ConsultantMobile>();
                                foreach (var MOB in ClientConsultantMobileDB)
                                {
                                    var ClientConsultantMobile = new ConsultantMobile()
                                    {
                                        ID = MOB.Id,
                                        Mobile = MOB.Mobile
                                    };
                                    ClientConsultantMobilesList.Add(ClientConsultantMobile);
                                }
                                ClientConsultantDataObj.ConsultantMobiles = ClientConsultantMobilesList;
                            }

                            var ClientConsultantLandLinesDB = _unitOfWork.ClientConsultantPhones.FindAll(a => a.ConsultantId == ClientConsultantDataDB.Id && a.Active == true).ToList();
                            if (ClientConsultantLandLinesDB != null && ClientConsultantLandLinesDB.Count > 0)
                            {
                                var ClientConsultantLandLinesList = new List<ConsultantLandLine>();
                                foreach (var LND in ClientConsultantLandLinesDB)
                                {
                                    var ClientConsultantLandLine = new ConsultantLandLine()
                                    {
                                        ID = LND.Id,
                                        LandLine = LND.Phone
                                    };
                                    ClientConsultantLandLinesList.Add(ClientConsultantLandLine);
                                }
                                ClientConsultantDataObj.ConsultantLandLines = ClientConsultantLandLinesList;
                            }

                            var ClientConsultantFaxesDB = _unitOfWork.ClientConsultantFaxes.FindAll(a => a.ConsultantId == ClientConsultantDataDB.Id && a.Active == true).ToList();
                            if (ClientConsultantFaxesDB != null && ClientConsultantFaxesDB.Count > 0)
                            {
                                var ClientConsultantFaxesList = new List<ConsultantFax>();
                                foreach (var FX in ClientConsultantFaxesDB)
                                {
                                    var ClientConsultantFax = new ConsultantFax()
                                    {
                                        ID = FX.Id,
                                        Fax = FX.Fax
                                    };
                                    ClientConsultantFaxesList.Add(ClientConsultantFax);
                                }
                                ClientConsultantDataObj.ConsultantFaxes = ClientConsultantFaxesList;
                            }

                            var ClientConsultantEmailsDB = _unitOfWork.ClientConsultantEmails.FindAll(a => a.ConsultantId == ClientConsultantDataDB.Id && a.Active == true).ToList();
                            if (ClientConsultantEmailsDB != null && ClientConsultantEmailsDB.Count > 0)
                            {
                                var ClientConsultantEmailsList = new List<ConsultantEmail>();
                                foreach (var email in ClientConsultantEmailsDB)
                                {
                                    var ClientConsultantEmail = new ConsultantEmail()
                                    {
                                        ID = email.Id,
                                        Email = email.Email
                                    };
                                    ClientConsultantEmailsList.Add(ClientConsultantEmail);
                                }
                                ClientConsultantDataObj.ConsultantEmails = ClientConsultantEmailsList;
                            }
                            Response.ClientConsultantData = ClientConsultantDataObj;
                        }

                        var LicenseAttechementsPathsDB = _unitOfWork.ClientAttachments.FindAll(a => a.ClientId == ClientDataDB.Id && a.Type == "License").ToList();
                        if (LicenseAttechementsPathsDB.Count() > 0)
                        {
                            List<Attachment> LicenseAttachmentsList = new List<Attachment>();
                            foreach (var attachment in LicenseAttechementsPathsDB)
                            {
                                //byte[] fileArray = System.IO.File.ReadAllBytes(baseURL + attachment.AttachmentPath);
                                //string base64FileRepresentation = Convert.ToBase64String(fileArray);
                                Attachment licenseAttach = new Attachment
                                {
                                    Id = attachment.Id,
                                    //FileContent = base64FileRepresentation,
                                    FileExtension = attachment.FileExtenssion,
                                    FileName = attachment.FileName,
                                    FilePath = Globals.baseURL + attachment.AttachmentPath.TrimStart('~')
                                };

                                LicenseAttachmentsList.Add(licenseAttach);
                            }

                            Response.LicenseAttachements = LicenseAttachmentsList;
                        }

                        var BrochureAttechementsPathsDB = _unitOfWork.ClientAttachments.FindAll(a => a.ClientId == ClientDataDB.Id && a.Type == "Brochure").ToList();
                        if (BrochureAttechementsPathsDB.Count() > 0)
                        {
                            List<Attachment> BrochureAttachmentsList = new List<Attachment>();
                            foreach (var attachment in BrochureAttechementsPathsDB)
                            {
                                //byte[] fileArray = System.IO.File.ReadAllBytes(baseURL + attachment.AttachmentPath);
                                //string base64FileRepresentation = Convert.ToBase64String(fileArray);
                                Attachment brochureAttach = new Attachment
                                {
                                    Id = attachment.Id,
                                    //FileContent = base64FileRepresentation,
                                    FileExtension = attachment.FileExtenssion,
                                    FileName = attachment.FileName,
                                    FilePath = Globals.baseURL + attachment.AttachmentPath.TrimStart('~')
                                };

                                BrochureAttachmentsList.Add(brochureAttach);
                            }

                            Response.BrochuresAttachments = BrochureAttachmentsList;
                        }

                        var BusinessCardAttechementsPathsDB = _unitOfWork.ClientAttachments.FindAll(a => a.ClientId == ClientDataDB.Id && a.Type == "Business Cards").ToList();
                        if (BusinessCardAttechementsPathsDB.Count() > 0)
                        {
                            List<Attachment> BusinessCardAttachmentsList = new List<Attachment>();
                            foreach (var attachment in BusinessCardAttechementsPathsDB)
                            {
                                //byte[] fileArray = System.IO.File.ReadAllBytes(baseURL + attachment.AttachmentPath);
                                //string base64FileRepresentation = Convert.ToBase64String(fileArray);
                                Attachment businessCardAttach = new Attachment
                                {
                                    Id = attachment.Id,
                                    //FileContent = base64FileRepresentation,
                                    FileExtension = attachment.FileExtenssion,
                                    FileName = attachment.FileName,
                                    FilePath = Globals.baseURL + attachment.AttachmentPath.TrimStart('~')
                                };

                                BusinessCardAttachmentsList.Add(businessCardAttach);
                            }

                            Response.BussinessCardsAttachments = BusinessCardAttachmentsList;
                        }

                        var TaxCardAttechementPathDB = _unitOfWork.ClientAttachments.FindAll(a => a.ClientId == ClientDataDB.Id && a.Type == "Tax Card").FirstOrDefault();
                        if (TaxCardAttechementPathDB != null)
                        {
                            Attachment TaxCardAttachment = new Attachment()
                            {
                                Id = TaxCardAttechementPathDB.Id,
                                //FileContent = base64FileRepresentation,
                                FileExtension = TaxCardAttechementPathDB.FileExtenssion,
                                FileName = TaxCardAttechementPathDB.FileName,
                                FilePath = Globals.baseURL + TaxCardAttechementPathDB.AttachmentPath.TrimStart('~')
                            };

                            Response.TaxCardAttachment = TaxCardAttachment;
                        }

                        var CommercialRecordAttechementPathDB = _unitOfWork.ClientAttachments.FindAll(a => a.ClientId == ClientDataDB.Id && a.Type == "Commercial Record").FirstOrDefault();
                        if (CommercialRecordAttechementPathDB != null)
                        {
                            Attachment CommercialRecordAttachment = new Attachment()
                            {
                                Id = CommercialRecordAttechementPathDB.Id,
                                //FileContent = base64FileRepresentation,
                                FileExtension = CommercialRecordAttechementPathDB.FileExtenssion,
                                FileName = CommercialRecordAttechementPathDB.FileName,
                                FilePath = Globals.baseURL + CommercialRecordAttechementPathDB.AttachmentPath.TrimStart('~')
                            };

                            Response.CommercialRecordAttachment = CommercialRecordAttachment;
                        }

                        var OtherAttechementsPathsDB = _unitOfWork.ClientAttachments.FindAll(a => a.ClientId == ClientDataDB.Id && a.Type != "Business Cards" && a.Type != "Brochure" && a.Type != "License" && a.Type != "Tax Card" && a.Type != "Commercial Record").ToList();
                        if (OtherAttechementsPathsDB.Count() > 0)
                        {
                            List<Attachment> OtherAttachmentsList = new List<Attachment>();
                            foreach (var attachment in OtherAttechementsPathsDB)
                            {
                                //byte[] fileArray = System.IO.File.ReadAllBytes(baseURL + attachment.AttachmentPath);
                                //string base64FileRepresentation = Convert.ToBase64String(fileArray);
                                Attachment otherAttach = new Attachment
                                {
                                    Id = attachment.Id,
                                    //FileContent = base64FileRepresentation,
                                    FileExtension = attachment.FileExtenssion,
                                    FileName = attachment.FileName,
                                    Category = attachment.Type,
                                    FilePath = Globals.baseURL + attachment.AttachmentPath.TrimStart('~')
                                };

                                OtherAttachmentsList.Add(otherAttach);
                            }

                            Response.OtherAttachments = OtherAttachmentsList;
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

    }
}
