using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.Hotel;


namespace NewGarasAPI.Controllers.Hotel
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {
        private readonly ICountryRepository _CountryRepository;
        private readonly IGovernorateRepository _governorateRepository;
        private readonly IAreaRepository _areaRepository;
        private readonly IlanguageeRepository _languageeRepository;
        private readonly INationalityRepository _clientNationality;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public CountryController(ICountryRepository countryRepository , IGovernorateRepository governorateRepository ,
                                 IAreaRepository areaRepository , IlanguageeRepository languageeRepository , INationalityRepository clientNationality , ITenantService tenantService)
        {
            _CountryRepository=countryRepository;
            _governorateRepository=governorateRepository;
            _areaRepository=areaRepository;
            _clientNationality=clientNationality;
            _languageeRepository=languageeRepository;
            _helper=new Helper.Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCountryAsync()
        {
            BaseResponseWithData<List<Country>> Response = new BaseResponseWithData<List<Country>>();
            Response.Data=new List<Country>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<Country>)await _CountryRepository.FindAllAsync(
                    r => r.Id>0);
                    ;
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }

        [HttpGet("City")]
        public async Task<IActionResult> GetAllCityfilterbycountryAsync([FromHeader] int idCountry)
        {
            BaseResponseWithData<List<Governorate>> Response = new BaseResponseWithData<List<Governorate>>();
            Response.Data=new List<Governorate>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<Governorate>)await _governorateRepository.FindAllAsync(
                    r => r.CountryId==idCountry);
                    ;
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }
        [HttpGet("Area")]
        public async Task<IActionResult> GetAllAreafilterbycityAsync([FromHeader] int idCity)
        {
            BaseResponseWithData<List<Area>> Response = new BaseResponseWithData<List<Area>>();
            Response.Data=new List<Area>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<Area>)await _areaRepository.FindAllAsync(
                    r => r.GovernorateId==idCity);
                ;
                Response.Result=true;
                }
                return Ok(Response);

            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }

        [HttpGet("Language")]
        public async Task<IActionResult> GetAllLanguages()
        {
            BaseResponseWithData<List<Languagee>> Response = new BaseResponseWithData<List<Languagee>>();
            Response.Data=new List<Languagee>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<Languagee>)await _languageeRepository.FindAllAsync(
                    r => r.Id>0);
                    ;
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }
        [HttpGet("Nationality")]
        public async Task<IActionResult> GetAllNationality()
        {
            BaseResponseWithData<List<Nationality>> Response = new BaseResponseWithData<List<Nationality>>();
            Response.Data=new List<Nationality>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<Nationality>)await _clientNationality.FindAllAsync(
                    r => r.Id>0);
                    ;
                    Response.Result=true;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }
    }
}
