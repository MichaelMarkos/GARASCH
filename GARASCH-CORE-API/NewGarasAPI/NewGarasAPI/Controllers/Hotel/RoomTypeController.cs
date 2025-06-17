

using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.Hotel;

namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomTypeController : ControllerBase
    {
        private readonly IRoomTypeRepository _roomTypeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public RoomTypeController(IRoomTypeRepository roomTypeRepository , IUnitOfWork unitOfWork , ITenantService tenantService)
        {
            _roomTypeRepository=roomTypeRepository;
            _unitOfWork=unitOfWork;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);

        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomTypes()
        {
            BaseResponseWithData<List<RoomType>> Response = new BaseResponseWithData<List<RoomType>>();
            Response.Data=new List<RoomType>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    Response.Data=(List<RoomType>)await _roomTypeRepository.GetAllAsync();
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

        [HttpGet("roomType")]
        public async Task<IActionResult> GetRoomTypeAsync([FromHeader] int roomTypeId)
        {
            BaseResponseWithData<RoomType> Response = new BaseResponseWithData<RoomType>();
            Response.Data=new RoomType();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    Response.Data=await _roomTypeRepository.GetByIdAsync(roomTypeId);
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
        public IActionResult AddRoomType(RoomType roomType)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<RoomType> Response = new BaseResponseWithData<RoomType>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var newRoomType = _roomTypeRepository.Add(roomType);
                    _unitOfWork.Complete();
                    Response.Data=newRoomType;
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
        public IActionResult RemoveRoom([FromHeader] int RoomTypeId)
        {
            BaseResponseWithData<RoomType> Response = new BaseResponseWithData<RoomType>();
            Response.Data=new RoomType();
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

                    _roomTypeRepository.Delete(_roomTypeRepository.GetById(RoomTypeId));
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
        public IActionResult UpdateRoom([FromHeader] int roomTypeId , [FromBody] RoomType roomType)
        {
            BaseResponseWithData<RoomType> Response = new BaseResponseWithData<RoomType>();
            Response.Data=new RoomType();
            Response.Errors=new List<Error>();
            Response.Result=false;

            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            if(roomType.Id!=roomTypeId)
                return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var updatedRoomType = _roomTypeRepository.Update(roomType);
                    _unitOfWork.Complete();
                    Response.Result=true;
                    Response.Data=updatedRoomType;
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