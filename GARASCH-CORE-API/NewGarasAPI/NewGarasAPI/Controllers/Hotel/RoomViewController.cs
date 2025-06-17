

using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.Hotel;

namespace GarasAPP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomViewController : ControllerBase
    {
        private readonly IRoomViewRepository _roomViewRepository;
        private readonly IUnitOfWork _unitOfWork;
        private Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public RoomViewController(IRoomViewRepository roomViewRepository , IUnitOfWork unitOfWork , ITenantService tenantService)
        {
            _roomViewRepository=roomViewRepository;
            _unitOfWork=unitOfWork;
            _helper=new Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoomViews()
        {
            BaseResponseWithData<List<RoomView>> Response = new BaseResponseWithData<List<RoomView>>();
            Response.Data=new List<RoomView>();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<RoomView>)await _roomViewRepository.GetAllAsync();
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

        [HttpGet("roomView")]
        public async Task<IActionResult> GetRoomViewAsync([FromHeader] int roomViewId)
        {
            BaseResponseWithData<RoomView> Response = new BaseResponseWithData<RoomView>();
            Response.Data=new RoomView();
            Response.Errors=new List<Error>();
            Response.Result=false;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=await _roomViewRepository.GetByIdAsync(roomViewId);
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
        public IActionResult AddRoomView(RoomView roomView)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<RoomView> Response = new BaseResponseWithData<RoomView>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var newRoomView = _roomViewRepository.Add(roomView);
                    _unitOfWork.Complete();
                    Response.Data=newRoomView;
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
        public IActionResult RemoveRoom([FromHeader] int RoomViewId)
        {
            BaseResponseWithData<RoomView> Response = new BaseResponseWithData<RoomView>();
            Response.Data=new RoomView();
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
                    _roomViewRepository.Delete(_roomViewRepository.GetById(RoomViewId));
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
        public IActionResult UpdateRoom([FromHeader] int roomViewId , [FromBody] RoomView roomView)
        {
            BaseResponseWithData<RoomView> Response = new BaseResponseWithData<RoomView>();
            Response.Data=new RoomView();
            Response.Errors=new List<Error>();
            Response.Result=false;

            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            if(roomView.Id!=roomViewId)
                return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {

                    var updatedRoomView = _roomViewRepository.Update(roomView);
                    _unitOfWork.Complete();
                    Response.Result=true;
                    Response.Data=updatedRoomView;
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