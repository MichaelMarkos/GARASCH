

using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.Hotel;

namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealTypeController : ControllerBase
    {
        private readonly IMealRepository _MealTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public MealTypeController(IMealRepository MealRepository , IUnitOfWork unitOfWork , ITenantService tenantService)
        {
            _MealTypeRepository=MealRepository;
            _unitOfWork=unitOfWork;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMealTypies()
        {
            BaseResponseWithData<List<MealType>> Response = new BaseResponseWithData<List<MealType>>();
            Response.Data=new List<MealType>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<MealType>)await _MealTypeRepository.GetAllAsync();
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

        [HttpGet("MealType")]
        public async Task<IActionResult> GetMealingAsync([FromHeader] int MealId)
        {
            BaseResponseWithData<MealType> Response = new BaseResponseWithData<MealType>();
            Response.Data=new MealType();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=await _MealTypeRepository.GetByIdAsync(MealId);
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
        public IActionResult AddMealingType(MealType Meal)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<MealType> Response = new BaseResponseWithData<MealType>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var newBuilding = _MealTypeRepository.Add(Meal);
                    _unitOfWork.Complete();
                    Response.Data=newBuilding;
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

        [HttpDelete]
        public IActionResult RemoveMealType([FromHeader] int MealId)
        {
            BaseResponseWithData<MealType> Response = new BaseResponseWithData<MealType>();
            Response.Data=new MealType();
            Response.Errors=new List<Error>();
            Response.Result=false;

            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    _MealTypeRepository.Delete(_MealTypeRepository.GetById(MealId));
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

        [HttpPut]
        public IActionResult UpdateMealType([FromHeader] int MealTypeId , [FromBody] MealType mealType)
        {
            BaseResponseWithData<MealType> Response = new BaseResponseWithData<MealType>();
            Response.Data=new MealType();
            Response.Errors=new List<Error>();
            Response.Result=false;

            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            if(mealType.Id!=MealTypeId)
                return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var updatedBuilding = _MealTypeRepository.Update(mealType);
                    _unitOfWork.Complete();
                    Response.Result=true;
                    Response.Data=updatedBuilding;
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
