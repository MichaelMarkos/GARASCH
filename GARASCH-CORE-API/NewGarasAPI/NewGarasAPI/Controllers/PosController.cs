using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Infrastructure.Models.POS;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PosController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        private readonly string key;
        private readonly IWebHostEnvironment _host;
        private readonly IPOSService _PosService;
        private readonly ITenantService _tenantService;

        public PosController(IWebHostEnvironment host, ITenantService tenantService, IPOSService PosService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            key = "SalesGarasPass";
            _helper = new Helper.Helper();
            _host = host;
            _PosService = PosService;
        }

        [HttpPost("AddPosClosingDay")]       //Service Added
        public BaseResponseWithId<long> AddPosClosingDay([FromBody]AddPos dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            var date = DateTime.Parse(dto.Date);
            try
            {
                if (response.Result)
                {
                    var pos = _PosService.AddPosClosingDay(date, dto.StoreId,validation.userID);
                    if (!pos.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(pos.Errors);
                        return response;
                    }
                    response = pos;
                    return response;
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

        [HttpGet("GetAllPosClosingDays")]
        public BaseResponseWithDataAndHeader<List<GetPosClosingDay>> GetAll([FromHeader] int CurrentPage=1, [FromHeader] int NumberOfItemsPerPage=10)
        {
            BaseResponseWithDataAndHeader<List<GetPosClosingDay>> response = new BaseResponseWithDataAndHeader<List<GetPosClosingDay>>()
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
                    var pos = _PosService.GetAll(CurrentPage,NumberOfItemsPerPage);
                    if (!pos.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(pos.Errors);
                        return response;
                    }
                    response = pos;
                    return response;
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

        [HttpPost("UpdatePosClosingDay")]
        public BaseResponseWithId<long> Update([FromBody]List<UpdatePosClosingDay> dto)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
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
                    var pos = _PosService.Update(dto, validation.userID);
                    if (!pos.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(pos.Errors);
                        return response;
                    }
                    response = pos;
                    return response;
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


        [HttpPost("AddNewSalesOfferWithReleaseForPOS")]
        public BaseResponseWithId<long> AddNewSalesOfferWithReleaseForPOS([FromBody] AddNewSalesOfferWithReleaseForPOSRequest request)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _PosService.AddNewSalesOfferWithReleaseForPOS(request, validation.CompanyName, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(uploadedInventoryItems.Errors);
                        return response;
                    }

                    response = uploadedInventoryItems;
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

        [HttpPost("ManageInventoryStoreItemPricinigForPOS")]
        public BaseResponseWithData<string> ManageInventoryStoreItemPricinigForPOS([FromBody]AddOneInventoryStoreItemPricing request)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _PosService.ManageInventoryStoreItemPricinigForPOS(request, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(uploadedInventoryItems.Errors);
                        return response;
                    }

                    response = uploadedInventoryItems;
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

        [HttpPost("AddInventoryItemCostNamePOS")]
        public BaseResponseWithData<string> AddInventoryItemCostNamePOS(InventoryItemPOSCostNameRequest request)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _PosService.AddInventoryItemCostNamePOS(request);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(uploadedInventoryItems.Errors);
                        return response;
                    }

                    response = uploadedInventoryItems;
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

        [HttpGet("GetInventoryItemCategoryListPOS")]
        public SelectDDLResponse GetInventoryItemCategoryListPOS([FromHeader] GetInventoryItemCategoryListFilters filters)
        {
            var response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var templete = _PosService.GetInventoryItemCategoryListPOS(filters);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(templete.Errors);
                        return response;
                    }
                    response.DDLList = templete.DDLList;
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

        [HttpGet("GetOfferInventoryItemsListForPOS")]
        public GetOfferInventoryItemsListForPOSResponse GetOfferInventoryItemsListForPOS([FromHeader] GetOfferInventoryItemsFilters filters)
        {
            var response = new GetOfferInventoryItemsListForPOSResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var templete = _PosService.GetOfferInventoryItemsListForPOS(filters,validation.CompanyName);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(templete.Errors);
                        return response;
                    }
                    response = templete;
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

        [HttpGet("GetAccountAndFinanceInventoryStoreItemInfoForPOS")]
        public AccountsAndFinanceInventoryItemInfoResponseForPOS GetAccountAndFinanceInventoryStoreItemInfoForPOS([FromHeader]long InventoryItemID, [FromHeader]long StoreId, [FromHeader]string InventoryItemCode)
        {
            var response = new AccountsAndFinanceInventoryItemInfoResponseForPOS();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var templete = _PosService.GetAccountAndFinanceInventoryStoreItemInfoForPOS(InventoryItemID,StoreId,InventoryItemCode);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(templete.Errors);
                        return response;
                    }
                    response = templete;
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


        [HttpPost("AddInventoryItemOpeningBalancePOS")]
        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePOS([FromBody]AddInventoryItemOpeningBalanceRequest request)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _PosService.AddInventoryItemOpeningBalancePOS(request,validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(uploadedInventoryItems.Errors);
                        return response;
                    }

                    response = uploadedInventoryItems;
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


        [HttpPost("AddInventoryItemOpeningBalancePerItem")]
        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePerItem(AddInventoryItemOpeningBalancePerItem request)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var uploadedInventoryItems = _PosService.AddInventoryItemOpeningBalancePerItem(request, validation.userID);
                    if (!response.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(uploadedInventoryItems.Errors);
                        return response;
                    }

                    response = uploadedInventoryItems;
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
