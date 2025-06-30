using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.Email;
using NewGaras.Infrastructure.Models.DDL;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DDLController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private readonly IDDLService _ddlservice;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        //private readonly HttpClient _httpClient;

        public DDLController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, IDDLService ddlservice)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);

            //---------------------new ----------------------------
            //_httpClient = new HttpClient();
            _ddlservice = ddlservice;
        }

        [HttpGet("GetLocalGovernorateList")]
        public SelectDDLResponse GetLocalGovernorateList()
        {
            var response = new SelectDDLResponse()
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
                    var email =  _ddlservice.GetLocalGovernorateList();
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetPurchasePOList")]
        public SelectDDLResponse GetPurchasePOList([FromHeader]long? SupplierID)
        {
            var response = new SelectDDLResponse()
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
                    var email = _ddlservice.GetPurchasePOList(SupplierID);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetMatrialRequestTypeList")]
        public SelectDDLResponse GetMatrialRequestTypeList()
        {
            var response = new SelectDDLResponse()
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
                    var email = _ddlservice.GetMatrialRequestTypeList();
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetProductOfferItemList")]
        public OfferItemDDLResponse GetProductOfferItemList([FromHeader]long ProjectFabricationID, [FromHeader]string SearchKey, [FromHeader]int CurrentPage = 1, [FromHeader]int NumberOfItemsPerPage = 10)
        {
            var response = new OfferItemDDLResponse()
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
                    var email = _ddlservice.GetProductOfferItemList(ProjectFabricationID,  SearchKey,CurrentPage,NumberOfItemsPerPage);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetCountriesGovernoratesAreasDDLs")]
        public CountriesGovernoratesAreasDDLs GetCountriesGovernoratesAreasDDLs()
        {
            var response = new CountriesGovernoratesAreasDDLs()
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
                    var email = _ddlservice.GetCountriesGovernoratesAreasDDLs(validation.CompanyName);
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetPriorityDDLs")]
        public BaseResponseWithData<List<GetPriorityModel>> GetPriority()
        {
            var response = new BaseResponseWithData<List<GetPriorityModel>>()
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
                    response = _ddlservice.GetPriority();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetSellingProductsDDL")]
        public SellingProductsDDLResponse GetSellingProductsDDL()
        {
            var response = new SellingProductsDDLResponse()
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
                    var email = _ddlservice.GetSellingProductsDDL();
                    if (!email.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(email.Errors);
                        return response;
                    }
                    response = email;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }


        [HttpGet("GetInventoryItemPartNoList")]
        public SelectDDLResponse GetInventoryItemPartNoList([FromHeader] string SearchKey)
        {
            var response = new SelectDDLResponse()
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
                    var InventoryItemList = _ddlservice.GetInventoryItemPartNoList(SearchKey);
                    if (!InventoryItemList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(InventoryItemList.Errors);
                        return response;
                    }
                    response = InventoryItemList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetNationalityDDL")]
        public SelectDDLResponse GetNationalityDDL()
        {
            var response = new SelectDDLResponse()
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
                    var NationalityList = _ddlservice.GetNationalityDDL();
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
        [HttpGet("MaritalStatus")]
        public async Task<IActionResult> MaritalStatus()
        {
            BaseResponseWithData<List<MaritalStatus>> Response = new BaseResponseWithData<List<MaritalStatus>>();
            Response.Data=new List<MaritalStatus>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                // without valdate
                Response.Data= _unitOfWork.MaritalStatus.FindAll(x=>x.Id > 0).ToList()  ;
                Response.Result=true;
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                // Response.Errors.Add(new Error { code="E-1" , message=ex.InnerException!=null ? ex.InnerException?.Message : ex.Message });
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("GetMilitaryStatusDDL")]
        public SelectDDLResponse GetMilitaryStatusDDL()
        {
            var response = new SelectDDLResponse()
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
                    var NationalityList = _ddlservice.GetMilitaryStatusDDL();
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("InvoiceType")]
        public async Task<IActionResult> InvoiceType()
        {
            BaseResponseWithData<List<InvoiceType>> Response = new BaseResponseWithData<List<InvoiceType>>();
            Response.Data=new List<InvoiceType>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                // without valdate
                Response.Data=_unitOfWork.InvoiceTypes.FindAll(x => x.Id>0).ToList();
                Response.Result=true;
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                // Response.Errors.Add(new Error { code="E-1" , message=ex.InnerException!=null ? ex.InnerException?.Message : ex.Message });
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("GetAttachmentTypeDDL")]
        public SelectDDLResponse GetAttachmentTypeDDL()
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetAttachmentTypeDDL();
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetCountriesDDL")]
        public SelectDDLResponse GetCountriesDDL()
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetCountriesDDL();
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetGovernoratesDDL")]
        public SelectDDLResponse GetGovernoratesDDL([FromHeader] int CountryId)
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetGovernoratesDDL(CountryId);
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetCitiesDDL")]
        public SelectDDLResponse GetCitiesDDL([FromHeader] int GovernorateId)
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetCitiesDDL(GovernorateId);
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetDistrictsDDL")]
        public SelectDDLResponse GetDistrictsDDL([FromHeader] int CityId)
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetDistrictsDDL(CityId);
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetAreasDDL")]
        public SelectDDLResponse GetAreasDDL([FromHeader] int DistrictId)
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetAreasDDL(DistrictId);
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        [HttpGet("GetGeographicalNamesDDL")]
        public SelectDDLResponse GetGeographicalNamesDDL()
        {
            var response = new SelectDDLResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                // without valdate
                if (response.Result)
                {
                    var NationalityList = _ddlservice.GetGeographicalNamesDDL();
                    if (!NationalityList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(NationalityList.Errors);
                        return response;
                    }
                    response = NationalityList;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
    }
}
