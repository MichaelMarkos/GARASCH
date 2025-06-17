using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Models.Hotel;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Domain.Services.Hotel;
using Azure;
using Org.BouncyCastle.Ocsp;
using NewGaras.Domain.Models;

namespace NewGarasAPI.Controllers.Hotel
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaboratoryController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly ILabortoryService _labortoryService;

        public LaboratoryController(IUnitOfWork unitOfWork, IWebHostEnvironment host, ITenantService tenantService, ILabortoryService labortoryService)
        {
            _host = host;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _labortoryService= labortoryService;

        }


        [HttpGet("GetLaboratoryMessagePagingList")]
        public async Task<IActionResult> GetLaboratoryMessageList( LaboratoryHeader dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors = new List<Error>();
            Response.Result = true;
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                var resultpaginglist = new BaseResponseWithDataPaging<List<LaboratoryMessagesReport>>();

                if (Response.Result)
                {

                    resultpaginglist = await _labortoryService.GetLaboratoryMessagePagingList(dto);
                    return Ok(resultpaginglist);

                }
                return Ok (Response); 

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                //error.ErrorCode="Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }



        [HttpGet("LabortoryMessageReportExcell")]
        public async Task<IActionResult> MessageReportExcell(LaboratoryHeader filters)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Errors=new List<Error>();
            Response.Result=true;
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {

                    Response = await  _labortoryService.MessageReportExcell(filters);
                    return Ok(Response);

                }
                return Ok(Response);

            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                //error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }


    }
}
