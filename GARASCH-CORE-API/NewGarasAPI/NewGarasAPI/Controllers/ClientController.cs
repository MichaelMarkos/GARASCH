using Azure;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Log;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.Client.ClientsCardsStatistics;
using NewGaras.Infrastructure.Models.Client.Filters;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.User;
using System.Net;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ClientAddress = NewGaras.Infrastructure.Entities.ClientAddress;
using ClientFax = NewGaras.Infrastructure.Entities.ClientFax;
using ClientMobile = NewGaras.Infrastructure.Entities.ClientMobile;
using Error = NewGarasAPI.Models.Common.Error;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IClientService _clientService;
        public ClientController(IWebHostEnvironment host, ITenantService tenantService, IClientService clientService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
            _clientService = clientService;
        }

        [HttpGet("GetClientList")]      //service used
        public SelectDDLResponse GetClientList([FromHeader]GetClientListFilters filters)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region Validation
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                #endregion
                if (validation.result)
                {
                    var data = _clientService.GetClientList(filters);
                    if (!data.Result)
                    {
                        Response.Result = false;
                        Response.Errors.AddRange(data.Errors);
                        return Response;
                    }
                    Response = data;
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

        [HttpPost("AddNewClient")]
        public BaseResponseWithID AddNewClient([FromForm] ClientData request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                _clientService.Validation = validation;
                var data = _clientService.AddNewClient(request, validation.userID);
                if (!data.Result)
                {
                    Response.Result = false;
                    Response.Errors.AddRange(data.Errors);
                    return Response;
                }
                Response = data;

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

        [HttpGet("GetClientDataResponse")]
        public async Task<GetClientData> GetClientDataResponse([FromHeader] bool? OwnerCoProfile, [FromHeader] long? ClientId)
        {
            GetClientData response = new GetClientData();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _clientService.GetClientDataResponse(OwnerCoProfile, ClientId);
                }
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

        [HttpPost("DeleteClient")]
        public BaseResponseWithId<long> DeleteClient([FromHeader] long ClientId)
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

                var data = _clientService.DeleteClient(ClientId);
                if (!data.Result)
                {
                    Response.Result = false;
                    Response.Errors.AddRange(data.Errors);
                    return Response;
                }
                Response = data;

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

        [HttpGet("GetClientsDDL")]
        public GetClientsCardsData GetClientsDDL([FromHeader] GetClientsFilters filters)
        {
            GetClientsCardsData response = new GetClientsCardsData();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.GetClientsDDL(filters, validation.userID, validation.CompanyName);
                }
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

        [HttpGet("GetClientsCardsDataResponse")]
        public GetClientsCardsData GetClientsCardsDataResponse([FromHeader] GetClientsCardsFilters filters)
        {
            GetClientsCardsData response = new GetClientsCardsData();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.GetClientsCardsDataResponse(filters, validation.userID, validation.CompanyName);
                }
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


        [HttpGet("GetClientScreenData")]
        public AddNewClientScreenData GetClientScreenData([FromHeader] int BranchId, [FromHeader] int RoleId, [FromHeader] long GroupId, [FromHeader] string JobTitleId)
        {
            AddNewClientScreenData response = new AddNewClientScreenData();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.GetClientScreenData(validation.CompanyName, BranchId, RoleId, GroupId,JobTitleId);
                }
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

        [HttpPost("AddNewClientContacts")]
        public BaseResponseWithId<long> AddNewClientContacts([FromBody] ClientContactsData data)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _clientService.Validation = validation;
                    response = _clientService.AddNewClientContacts(data);
                }
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

        [HttpPost("AddNewClientContactPerson")]
        public BaseResponseWithID AddNewClientContactPerson(ClientContactPersonData data)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _clientService.Validation = validation;
                    response = _clientService.AddNewClientContactPerson(data);
                }
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

        [HttpPost("AddClientTaxCard")]
        public async Task<BaseResponseWithID> AddClientTaxCard([FromBody]ClientTaxCardData data)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response =await _clientService.AddClientTaxCard(data);
                }
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

        [HttpGet("GetClientsDetailsResponse")]
        public ClientsDetailsResponse GetClientsDetailsResponse([FromHeader]long ClientId)
        {
            ClientsDetailsResponse response = new ClientsDetailsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.GetClientsDetailsResponse(ClientId);
                }
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

        [HttpPost("UpdateClassificationOfClient")]
        public async Task<BaseResponseWithID> UpdateClassificationOfClient([FromBody]UpdateClientClassRequest data)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _clientService.UpdateClassificationOfClient(data);
                }
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

        [HttpGet("GetClientsCardsStatisticsResponse")]
        public GetClientsCardsStatistics GetClientsCardsStatisticsResponse([FromHeader]GetClientsCardsStatisticsHeaders filters)
        {
            GetClientsCardsStatistics response = new GetClientsCardsStatistics();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = _clientService.GetClientsCardsStatisticsResponse(filters);
                if (!data.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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

        [HttpPost("AddNewClientConsultant")]
        public BaseResponseWithID AddNewClientConsultant([FromBody]ClientConsultantData ConsultantData)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data = _clientService.AddNewClientConsultant(ConsultantData, validation.userID);
                if (!response.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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

        [HttpGet("GetClientClassifications")]
        public async Task<ClientClassificationsResponse> GetClientClassifications()
        {
            ClientClassificationsResponse response = new ClientClassificationsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                var data =await _clientService.GetClientClassifications();
                if (!response.Result)
                {
                    response.Result = false;
                    response.Errors.AddRange(data.Errors);
                    return response;
                }
                response = data;
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

        [HttpPost("AddClientAttachments")]
        public BaseResponseWithId<long> AddClientAttachments([FromForm]ClientAttachmentsData request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.AddClientAttachments(request,validation.CompanyName);
                }
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

        [HttpGet("GetClientsDropDown")]
        public BaseResponseWithData<List<SelectDDL>> GetClientsDropDown()
        {
            BaseResponseWithData<List<SelectDDL>> response = new BaseResponseWithData<List<SelectDDL>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.GetClientsDropDown();
                }
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

        [HttpGet("GetClientDashboard")]
        public async Task<GetClientDashboardResponse> GetClientDashboard([FromHeader] long ClientId)
        {
            GetClientDashboardResponse response = new GetClientDashboardResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = await _clientService.GetClientDashboard(ClientId);
                }
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

        [HttpGet("CheckClientExistance")]
        public CheckClientExistanceResponse CheckClientExistance(CheckClientExistanceFilters filters)
        {
            CheckClientExistanceResponse response = new CheckClientExistanceResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    response = _clientService.CheckClientExistance(filters);
                }
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

        [HttpPost("AddNewClientMobile")]
        public BaseResponseWithId<long> AddNewClientMobile(ClientMobileDataDto request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _clientService.Validation = validation;
                    response = _clientService.AddNewClientMobile(request);
                }
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

        [HttpPost("AddNewClientSpeciality")]
        public BaseResponseWithId<long> AddNewClientSpeciality(ClientSpecialityData request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _clientService.Validation = validation;
                    response = _clientService.AddNewClientSpeciality(request);
                }
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

        [HttpPost("AddNewClientLandLine")]
        public BaseResponseWithId<long> AddNewClientLandLine(ClientLandLineDataDto request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _clientService.Validation = validation;
                    response = _clientService.AddNewClientLandLine(request);
                }
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


        [HttpPost("AddNewClientFax")]
        public BaseResponseWithId<long> AddNewClientFax(ClientFaxDataDto request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    _clientService.Validation = validation;
                    response = _clientService.AddNewClientFax(request);
                }
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
