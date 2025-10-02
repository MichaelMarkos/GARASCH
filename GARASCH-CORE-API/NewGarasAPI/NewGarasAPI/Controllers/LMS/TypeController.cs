

using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using Type = NewGaras.Infrastructure.Entities.Type;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        //private ApplicationDbContext _Context;
        public TypeController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {
            // _Context = new ApplicationDbContext();
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllType()
        {
            BaseResponseWithData<List<Type>> Response = new BaseResponseWithData<List<Type>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Types.GetAllAsync();
                    Response.Result=true;
                    Response.Data=(List<Type>)TempData;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("GetAllCompetitionType")]
        public async Task<IActionResult> GetAllCompetitionType()
        {
            BaseResponseWithData<List<CompetitionType>> Response = new BaseResponseWithData<List<CompetitionType>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.CompetitionTypes.GetAllAsync();
                    Response.Data=(List<CompetitionType>)TempData;
                }
                Response.Result=true;
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

        [HttpGet("GetTypebyId")]
        public async Task<IActionResult> GetType([FromHeader] int Id)
        {
            BaseResponseWithData<Type> Response = new BaseResponseWithData<Type>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Types.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Type Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    DateTime today = DateTime.Today;

                    // Get the current day of the week
                    DayOfWeek currentDay = today.DayOfWeek;

                    // Calculate the start of the week (Monday)
                    int daysSinceMonday = (int)currentDay - (int)DayOfWeek.Saturday;
                    DateTime startOfWeek = today.AddDays(daysSinceMonday-1);

                    // Calculate the end of the week (Sunday)
                    int daysUntilSunday = (int)DayOfWeek.Friday - (int)currentDay;
                    DateTime endOfWeek = today.AddDays(daysUntilSunday);
                    startOfWeek=startOfWeek.AddDays(7);
                    endOfWeek=endOfWeek.AddDays(7);
                    Response.Result=true;
                    Response.Data=TempData;
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }



        [HttpPost]
        public IActionResult AddTyoe(Type type)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var newhall = _unitOfWork.Types.Add(type);
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
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }

      
        [HttpDelete]
        public IActionResult RemoveType([FromHeader] int Id)
        {
            BaseResponse Response = new BaseResponse();
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
                    _unitOfWork.Types.Delete(_unitOfWork.Types.GetById(Id));
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
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }



        [HttpPut]
        public IActionResult UpdateType([FromHeader] int typeId , [FromBody] Type type)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors=new List<Error>();
            Response.Result=false;

            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            if(type.Id!=typeId)
                return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var updatedBuilding = _unitOfWork.Types.Update(type);
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
                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
    }
}
