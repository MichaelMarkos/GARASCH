
using Azure;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;
using System.Collections.Generic;


namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthLMsService _authService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public AuthController(IAuthLMsService authService , IUnitOfWork unitOfWork , IWebHostEnvironment host , ITenantService tenantService)
        {
            _authService=authService;
            _host=host;
            _unitOfWork=unitOfWork;
            _helper=new Helper.Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }


        [HttpGet("NextlectureToday")]
        public async Task<BaseResponseWithData<lectureTodayVM>> AddTransportationVehicle([FromHeader] long hruserId)
        {
            BaseResponseWithData<lectureTodayVM> response = new BaseResponseWithData<lectureTodayVM>();
            response.Result=true;
            response.Errors=new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors=validation.errors;
                response.Result=validation.result;

                if(response.Result)
                {
                    // _transportationLineService.Validation=validation;
                    response = await _authService.NextlectureToday(hruserId);

                }
                return response;
            }
            catch(Exception ex)
            {
                response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);

                return response;
            }
        }
        [HttpPost("checkUserIsExist")]
        public async Task<IActionResult> checkUserIsExistAsync([FromBody] CheckUserIsExistModel model)
        {
            BaseResponse response = new BaseResponse();
            response.Result=true;
            response.Errors=new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors=validation.errors;
                response.Result=validation.result;

                if(response.Result)
                {
                    var result = await _authService.checkUserIsExistAsync(model);

                    if(result.Count()>0)
                    {
                        response.Result=false;
                        Error error = new Error();
                        foreach(var err in result)
                        {
                            error.ErrorCode="Err10";
                            error.ErrorMSG=err;
                            response.Errors.Add(error);
                        }
                      

                        return BadRequest(response) ;
                     
                    }
                }
                return Ok(Response);

            }
            catch(Exception ex)
            {
                response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);

                return BadRequest(response);
            }
        }
        [Authorize]
        [HttpPost("StudentInfo")] //new controll
        public async Task<IActionResult> StudentInfo([FromHeader] long HrUserId)
        {
            BaseResponseWithData<YearInfoDto> Response = new BaseResponseWithData<YearInfoDto>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                var data = new List<CompetitionDTO>();
                if(Response.Result)
                {
                    var Hruser = _unitOfWork.HrUsers.FindAll(x=>x.Id  == HrUserId ).FirstOrDefault();
                    if(Hruser!=null)
                    {
                        var YearDb = _unitOfWork.UserDepartment.FindAll(x => x.HrUserId == Hruser.Id , new[] { "Year" }).FirstOrDefault();
                        var SpecialDeptDb = _unitOfWork.UserDepartment.FindAll(x => x.HrUserId == Hruser.Id , new[] { "Specialdept" }).FirstOrDefault();
                        var LevelDb = _unitOfWork.UserDepartment.FindAll(x => x.HrUserId == Hruser.Id , new[] { "Academiclevel" }).FirstOrDefault();

                        var authModel = new YearInfoDto();
                        authModel.YearName=YearDb?.Year?.Name;
                        authModel.YearId=YearDb?.Year?.Id;
                        var SpecialDeptName = SpecialDeptDb?.Specialdept?.Name;
                        authModel.SpecialDeptId=SpecialDeptDb?.Specialdept?.Id;
                        var DeptName = _unitOfWork.Specialdepts.FindAll(x => x.Id == authModel.SpecialDeptId, new[] { "Deptartment" }).Select(y => y.Deptartment.Name).FirstOrDefault();
                        authModel.SpecialDeptAndDeptName=(DeptName+" "+SpecialDeptName)??null;
                        authModel.LevelName=LevelDb?.Academiclevel?.Name;
                        authModel.LevelId=LevelDb?.Academiclevel?.Id;

                        Response.Data=authModel;
                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="لا يوجد مستخدم ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }
                   
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [Authorize(Roles = "admin")]
        [HttpGet("GetUsersListAdminCompetion")]
        public async Task<IActionResult> GetUserRoleAdminCometitionListAsync()
        {
            var Response = new BaseResponseWithData<IEnumerable<CompetitorUserInfoDTO>>();
            Response.Result=true;
            Response.Errors=new List<Error>();

            try
            {
                Response=await _authService.GetUserRoleAdminCometitionListAsync();
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
    }
}
