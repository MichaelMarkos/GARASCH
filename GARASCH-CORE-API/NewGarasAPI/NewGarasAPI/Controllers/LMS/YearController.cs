

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
    public class YearController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public YearController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {
            // _Context = new ApplicationDbContext();
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAcademicYear()
        {
            BaseResponseWithData<List<Year>> Response = new BaseResponseWithData<List<Year>> ();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Years.GetAllAsync();

                    Response.Result=true;
                    Response.Data=(List<Year>)TempData;
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

        [HttpGet("GetAcademicYearbyId")]
        public async Task<IActionResult> GetAcademicYear([FromHeader] int Id)
        {
            BaseResponseWithData<Year> Response = new BaseResponseWithData<Year>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Years.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Year Id";
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

        [HttpGet("GetAcademicYearbyName")]
        public async Task<IActionResult> GetAcademicYearbyName([FromHeader] int yearId)
        {
            BaseResponseWithData<List<Year>> Response = new BaseResponseWithData<List<Year>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.AcademicYears.FindAllAsync(x => x.YearId == yearId);
                    if(TempData==null||TempData.Count()==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Id  in AcademicYears";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }

                    Response.Result=true;
                    Response.Data=(List<Year>)TempData;
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
        public IActionResult AddYear(Year year)
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
                    var NewYear = _unitOfWork.Years.Add(year);
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




        [HttpPost("RemoveYear")]
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
                    _unitOfWork.Years.Delete(_unitOfWork.Years.GetById(Id));
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



        [HttpPost("Updateyear")]
        public IActionResult Updateyear([FromBody] Year year)
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
                    var oldItem = _unitOfWork.Years.GetById(year.Id) ;
                    if(oldItem!=null)
                    {
                        oldItem.Name=year.Name;

                        _unitOfWork.Years.Update(oldItem);
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
