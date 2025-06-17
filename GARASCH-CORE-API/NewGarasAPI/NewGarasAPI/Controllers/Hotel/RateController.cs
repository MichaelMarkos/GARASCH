using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Hotel.DTOs;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Models;



namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RateController : Controller
    {
        private readonly IRateRepository _rateRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public RateController(IRateRepository rateRepository , IMapper mapper , IUnitOfWork unitOfWork , ITenantService tenantService)
        {
            _rateRepository=rateRepository;
            _mapper=mapper;
            _unitOfWork=unitOfWork;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }


        [HttpGet]
        public async Task<IActionResult> GetAllRatesAsync()
        {

            BaseResponseWithData<List<Rate>> Response = new BaseResponseWithData<List<Rate>>();
            Response.Data=new List<Rate>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var tempData = (await _rateRepository.FindAllAsync(
                    r => r.Id > 0));
                    Response.Data=(List<Rate>)tempData;
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

        [HttpPost]
        public async Task<IActionResult> AddRates(NewRateDto newRates)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            BaseResponseWithData<List<Rate>> Response = new BaseResponseWithData<List<Rate>>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    Response=_rateRepository.AddSpecialOffer(newRates);
                    _unitOfWork.Complete();
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


        [HttpGet("RateListRoom")]
        public async Task<IActionResult> RateListRoom([FromHeader] RatelistRoomDto dto)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            BaseResponseWithData<List<RatelistRoomDto2>> Response = new BaseResponseWithData<List<RatelistRoomDto2>>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    Response=await _rateRepository.RateListRoom(dto);
                    Response.Result=true;
                    //_unitOfWork.Complete();
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


        [HttpPost("addrate")]
        public async Task<IActionResult> AddRate(List<AddRateDto> newRates)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            BaseResponse Response = new BaseResponse();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    _rateRepository.AddRate(newRates);
                    Response.Result=true;
                    //_unitOfWork.Complete();
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


        [HttpGet("ratesrooms")]
        public async Task<IActionResult> test()
        {

            var test = _rateRepository.DailyUpdate();
            _unitOfWork.Complete();

            return Ok(test);
        }
        [HttpGet("roomrate")]
        public async Task<IActionResult> roomrate([FromHeader] int roomId)
        {
            BaseResponseWithData<Rate> Response = new BaseResponseWithData<Rate>();
            Response.Data=new Rate();
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var rate = _rateRepository.rateroomandoffers(roomId);
                    Response.Result=rate.Result;
                    Response.Data=rate.Data;
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
        [HttpGet("getroomoffer")]
        public async Task<IActionResult> getroomoffer([FromHeader] DurationDto2 durationDto , [FromHeader] int roomId)
        {
            BaseResponseWithData<List<SpecialOfferFlag>> Response = new BaseResponseWithData<List<SpecialOfferFlag>>();
            Response.Data=new List<SpecialOfferFlag>();
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var rate = await _rateRepository.GetoffersforRoom(durationDto, roomId);
                    Response.Result = rate.Result;
                    Response.Data=rate.Data;
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

        [HttpDelete]
        public IActionResult RemoveRoom([FromHeader] int rateId)
        {
            BaseResponseWithData<Rate> Response = new BaseResponseWithData<Rate>();
            Response.Data=new Rate();
            Response.Errors=new List<Error>();
            Response.Result=false;

            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    _rateRepository.Delete(_rateRepository.GetById(rateId));
                    _unitOfWork.Complete();
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
