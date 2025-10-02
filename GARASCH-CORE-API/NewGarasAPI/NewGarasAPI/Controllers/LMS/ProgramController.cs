

using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public ProgramController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {
            // _Context = new ApplicationDbContext();
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllProgram()
        {
            BaseResponseWithData<List<Programm>> Response = new BaseResponseWithData<List<Programm>> ();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Programm.GetAllAsync();

                    Response.Result=true;
                    Response.Data=(List<Programm>)TempData;
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

        [HttpGet("GetProgrambyId")]
        public async Task<IActionResult> GetProgramm([FromHeader] int Id)
        {
            BaseResponseWithData<Programm> Response = new BaseResponseWithData<Programm>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Programm.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Program Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
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
        public IActionResult AddProgram(Programm dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors=new List<Error>();
            Response.Result=false;



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(string.IsNullOrEmpty(dto.Name))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="paramter is required";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    var NewProgramm = _unitOfWork.Programm.Add(dto);
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




        [HttpPost("RemoveProgram")]
        public IActionResult RemoveYear([FromHeader] int Id)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors=new List<Error>();
            Response.Result=false;


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(Id==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="paramter is required";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    _unitOfWork.Programm.Delete(_unitOfWork.Programm.GetById(Id));
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



        [HttpPost("UpdateProgram")]
        public IActionResult Updateyear([FromBody] Programm dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Errors=new List<Error>();
            Response.Result=false;



            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(dto.Id==0||string.IsNullOrEmpty(dto.Name))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="paramter is required";
                        Response.Errors.Add(error);
                        return BadRequest(Response);
                    }




                    var oldItem = _unitOfWork.Programm.GetById(dto.Id) ;
                    if(oldItem!=null)
                    {
                        oldItem.Name=dto.Name;
                        oldItem.ApprovedHours=dto.ApprovedHours;

                        _unitOfWork.Programm.Update(oldItem);
                        _unitOfWork.Complete();
                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
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
