
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;


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
        public AuthController(IAuthLMsService authService , IUnitOfWork unitOfWork , IWebHostEnvironment host , ITenantService tenantService  )
        {
            _authService=authService;
            _host=host;
            _unitOfWork=unitOfWork;
            _helper=new Helper.Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }


        [HttpGet("NextlectureToday")]
        public async Task< BaseResponseWithData<lectureTodayVM>> AddTransportationVehicle([FromHeader] long hruserId)
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


    }
}
