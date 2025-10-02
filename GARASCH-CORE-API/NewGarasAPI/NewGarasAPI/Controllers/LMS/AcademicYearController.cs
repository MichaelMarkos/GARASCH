

using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class AcademicYearController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public AcademicYearController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAcademicYear([FromHeader] int yearId)
        {
            BaseResponseWithData<List<AcademicYear>> Response = new BaseResponseWithData<List<AcademicYear>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var TempData = new List<AcademicYear>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(yearId==0)
                    {
                        TempData=(List<AcademicYear>)await _unitOfWork.AcademicYears.GetAllAsync();

                    }
                    else
                    {
                        TempData=(List<AcademicYear>)await _unitOfWork.AcademicYears.FindAllAsync(x => x.YearId==yearId);

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

        [HttpGet("GetAcademicYearbyId")]
        public async Task<IActionResult> GetAcademicYear([FromHeader] int Id)
        {
            BaseResponseWithData<AcademicYear> Response = new BaseResponseWithData<AcademicYear>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.AcademicYears.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid AcademicYear Id";
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
            BaseResponseWithData<List<AcademicYear>> Response = new BaseResponseWithData<List<AcademicYear>>();
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
                        error.ErrorMSG="Invalid AcademicYear Id";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    Response.Result=true;
                    Response.Data=(List<AcademicYear>)TempData;
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
        public IActionResult AddAcademicYear(AcademicYearDto dto)
        {

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
                    _unitOfWork.Years.Add(new Year
                    {
                        Name=dto.YearName
                    });
                    var newhall = _unitOfWork.AcademicYears.Add(_mapper.Map<AcademicYear>(dto));
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

        [HttpPost("AddListAcademicYears")]
        public IActionResult AddListAcademicYear(AcademicYearViewModel dto)
        {

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
                    var yearDB = _unitOfWork.Years.FindAll(x=>x.Name == dto.YearName).FirstOrDefault();
                    var year = new Year();
                    if(yearDB!=null)
                    {
                        year=yearDB;
                    }
                    else
                    {
                        year=_unitOfWork.Years.Add(new Year
                        {
                            Name=dto.YearName
                        });
                        _unitOfWork.Complete();
                    }
                    var Newacademicyears = new List<AcademicYear>();
                    foreach(var item in dto.dtolist)
                    {
                        item.YearId=year.Id;
                        var YearDb = _unitOfWork.AcademicYears.FindAll(x=>x.YearId == item.YearId && x.Term == item.Term) ;
                        if(YearDb.Count()>0)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="This term and year does already exist.";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        //tempdate = _unitOfWork.AcademicYears.Add(_mapper.Map<AcademicYear>(item));
                        var tempdate = _unitOfWork.AcademicYears.Add(new AcademicYear
                        {
                            YearId = item.YearId,
                            Term = item.Term,
                            From = item.From,
                            To = item.To
                        });
                        Newacademicyears.Add(tempdate);
                    }

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

        [HttpPost("RemoveAcademicYears")]
        public IActionResult RemoveAcademicYears([FromHeader] int Id)
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
                    _unitOfWork.AcademicYears.Delete(_unitOfWork.AcademicYears.GetById(Id));
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



        [HttpPost("Update")]
        public IActionResult Update([FromBody] AcademicYear academicyear)
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
                    var oldItem = _unitOfWork.AcademicYears.GetById(academicyear.Id) ;
                    if(oldItem!=null)
                    {
                        oldItem.Term=academicyear.Term;
                        oldItem.YearId=academicyear.YearId;
                        oldItem.From=academicyear.From;
                        oldItem.To=academicyear.To;

                        _unitOfWork.AcademicYears.Update(oldItem);
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
