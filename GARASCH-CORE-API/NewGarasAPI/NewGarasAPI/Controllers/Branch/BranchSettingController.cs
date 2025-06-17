using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.BranchSetting;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.HR;

namespace NewGarasAPI.Controllers
{
    [Route("HR/[controller]")]
    [ApiController]
    public class BranchSettingController : ControllerBase
    {
        private readonly IBranchSettingService _branchSettingService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public BranchSettingController(IBranchSettingService branchSettingService,ITenantService tenantService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _branchSettingService = branchSettingService;
            _helper = new Helper.Helper();
        }
        [HttpPost("AddBranchSetting")]
        public BaseResponseWithData<List<long>> AddBranchSetting([FromForm] BranchSettingDto dto)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _branchSettingService.AddBranchSetting(dto, validation.userID);
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
        [HttpPost("EditBranchSetting")]
        public BaseResponseWithData<List<long>> EditBranchSetting([FromForm] EditBranchSettingDto dto)
        {
            BaseResponseWithData<List<long>> Response = new BaseResponseWithData<List<long>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new List<long>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _branchSettingService.EditBranchSetting(dto, validation.userID);
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


        [HttpPost("DeleteBranchSetting")]
        public BaseResponseWithId<long> DeleteBranchSetting([FromHeader] long SettingId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _branchSettingService.DeleteBranchSetting(SettingId);     
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

        [HttpGet("GetBranchSetting")]
        public BaseResponseWithData<GetBranchSettingDto> GetBranchSetting([FromHeader] int branchId)
        {
            BaseResponseWithData<GetBranchSettingDto> Response = new BaseResponseWithData<GetBranchSettingDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _branchSettingService.GetBranchSetting(branchId);
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
