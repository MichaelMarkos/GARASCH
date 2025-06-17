
using Azure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.Hotel;


namespace NewGarasAPI.Controllers.Hotel
{
    [Route("api/[controller]")]
    [ApiController]
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingRepository _buildingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public BuildingController(IBuildingRepository buildingRepository , IUnitOfWork unitOfWork , ITenantService tenantService)
        {
            _buildingRepository=buildingRepository;
            _unitOfWork=unitOfWork;
            _helper=new Helper.Helper();
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);

        }
        [HttpGet]
        public async Task<IActionResult> GetAllBuildings()
        {
            BaseResponseWithData<List<Building>> Response = new BaseResponseWithData<List<Building>>();
            Response.Data=new List<Building>();
            Response.Errors=new List<Error>();
            Response.Result=true;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=(List<Building>)await _buildingRepository.GetAllAsync();
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                // Response.Errors.Add(new Error { code="E-1" , message=ex.InnerException!=null ? ex.InnerException?.Message : ex.Message });
                Error error = new Error();
                error.ErrorCode="Err10";
                error.ErrorMSG=ex.InnerException.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }
        }
        [HttpGet("building")]
        public async Task<IActionResult> GetBuildingAsync([FromHeader] int buildingId)
        {
            BaseResponseWithData<Building> Response = new BaseResponseWithData<Building>();
            Response.Data=new Building();
            Response.Errors=new List<Error>();
            Response.Result=true;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    Response.Data=await _buildingRepository.GetByIdAsync(buildingId);
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
                return BadRequest(Response);
            }
        }

        [HttpPost]
        public IActionResult AddBuilding(Building building)
        {
            //if(!ModelState.IsValid)
            //    return BadRequest(ModelState);
            BaseResponseWithData<Building> Response = new BaseResponseWithData<Building>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var newBuilding = _buildingRepository.Add(building);
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
                return BadRequest(Response);
            }
        }

        [HttpDelete]
        public IActionResult RemoveRoom([FromHeader] int buildingId)
        {
            BaseResponseWithData<Building> Response = new BaseResponseWithData<Building>();
            Response.Data=new Building();
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
                    _buildingRepository.Delete(_buildingRepository.GetById(buildingId));
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
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public IActionResult UpdateRoom([FromHeader] int buildingId , [FromBody] Building building)
        {
            BaseResponseWithData<Building> Response = new BaseResponseWithData<Building>();
            Response.Data=new Building();
            Response.Errors=new List<Error>();
            Response.Result=false;

            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            if(building.Id!=buildingId)
                return BadRequest(ModelState);

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var updatedBuilding = _buildingRepository.Update(building);
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
                return BadRequest(Response);
            }
        }
    }
}
