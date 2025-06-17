
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Models.Hotel;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Models;
using Azure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;

namespace NewGarasAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationServiceController : ControllerBase
    {
        private readonly IGreenApiService _greenApiService;
        private readonly IUnitOfWork _unitOfWork;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public IntegrationServiceController(IGreenApiService greenApiService, IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _greenApiService = greenApiService;
            _unitOfWork = unitOfWork;
            _helper = new Helper.Helper();
            _tenantService = tenantService;

            _Context = new GarasTestContext(_tenantService);


        }


        [HttpPost("send-welcome-message")]
        public async Task<IActionResult> SendWelcomeMessage(BodyOfMessage dto)
        {
            //var language = Request.Headers["language"].ToString();

            var result = await _greenApiService.SendMessage(dto.ChatId, dto.Message);

            if (!result)
                throw new Exception("Something went wrong!");

            return Ok("Sent successfully");
        }


        [HttpPost("sendpdfbyurl")]
        public async Task<IActionResult> Sendpdfbyurl(Sendpdfbyurl dto)
        {
            //var language = Request.Headers["language"].ToString();

            var result = await _greenApiService.SendPdfbyUrl(dto.chatId, dto.urlFile, dto.fileName, dto.caption);

            if (!result)
                throw new Exception("Something went wrong!");

            return Ok("Sent successfully");
        }

        [HttpPost("UploadFormDataAsync")]
        public async Task<IActionResult> UploadFormDataAsync([FromForm] FormOfFileMessage dto, [FromHeader] long UserId) 
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors = new List<Error>();
            Response.Result = true;
            try
            {
                Request.Headers["UserToken"] = "H05Y%2fQGpV1U9eC13Zu1jaA%3d%3d"; // Token with Id =43534 for User Lab Integration in stmark company UserId =20076
                Request.Headers["CompanyName"] = "stmark";
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                //var CompanyName = Request.Headers["CompanyName"].ToString().ToLower();


                if (Response.Result)
                {

                    var result = await _greenApiService.UploadFormDataAsync(dto, UserId);

                    if (!result)
                        Response.Result = false;

                }
                return Ok(Response);
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

    }
}
