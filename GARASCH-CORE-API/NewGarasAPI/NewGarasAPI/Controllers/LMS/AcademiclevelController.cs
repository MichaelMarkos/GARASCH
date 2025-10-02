

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
    public class AcademiclevelController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public AcademiclevelController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();

        }
        [HttpGet]
        public async Task<IActionResult> GetAllAcademiclevel([FromHeader] int programId)
        {
            BaseResponseWithData<List<AcademiclevelDto>> Response = new BaseResponseWithData<List<AcademiclevelDto>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            List<AcademiclevelDto> TempData = new();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(programId==0)
                    {
                        TempData=_unitOfWork.Academiclevels.FindAll(x => x.Id>0 , new [] { "Program" }).Select(lev => new AcademiclevelDto
                        {
                            Id=lev.Id ,
                            Name=lev.Name ,
                            Level=lev.Level ,
                            MinHours=lev.MinHours ,
                            MaxHours=lev.MaxHours ,
                            ProgramName=lev.Program?.Name??null ,
                            ProgramId=lev.Program?.Id??null
                        }).ToList();
                    }
                    else
                    {
                        TempData=_unitOfWork.Academiclevels.FindAll(x => x.ProgramId==programId , new [] { "Programm" }).Select(lev => new AcademiclevelDto
                        {
                            Id=lev.Id ,
                            Name=lev.Name ,
                            Level=lev.Level ,
                            MinHours=lev.MinHours ,
                            MaxHours=lev.MaxHours ,
                            ProgramName=lev.Program?.Name??null ,
                            ProgramId=lev.Program?.Id??null
                        }).ToList();
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



        [HttpGet("GetAcademiclevelbyId")]
        public async Task<IActionResult> GetAcAcademiclevel([FromHeader] int Id)
        {
            BaseResponseWithData<Academiclevel> Response = new BaseResponseWithData<Academiclevel>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Academiclevels.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Academiclevel Id";
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
        public IActionResult AddAcademiclevel(Academiclevel academiclevel)
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
                    var newhall = _unitOfWork.Academiclevels.Add(academiclevel);
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

        [HttpPost("AddListAcademiclevel")]
        public IActionResult AddListAcademiclevel(AcademiclevelViewModel academiclevels)
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
                    var Newacademiclevels = new List<Academiclevel>();
                    foreach(var item in academiclevels.academiclevels)
                    {
                        if(string.IsNullOrEmpty(item.Name)||string.IsNullOrEmpty(item.Level))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="paramter is required";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        if(item.MinHours>item.MaxHours)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Max hour must greater Min hours";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        _unitOfWork.Academiclevels.Add(item);
                        Newacademiclevels.Add(item);
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

        [HttpPost("Remove")]
        public IActionResult Remove([FromHeader] int Id)
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
                    _unitOfWork.Academiclevels.Delete(_unitOfWork.Academiclevels.GetById(Id));
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



        [HttpPost("UpdateAcademiclevel")]
        public IActionResult UpdateAcademiclevel([FromBody] Academiclevel academiclevel)
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

                    var oldItem = _unitOfWork.Academiclevels.GetById(academiclevel.Id) ;
                    if(oldItem!=null)
                    {
                        if(academiclevel.MinHours>academiclevel.MaxHours)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Max hour must greater Min hours";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        if(academiclevel.Id==0||string.IsNullOrEmpty(academiclevel.Name)||string.IsNullOrEmpty(academiclevel.Level))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="paramter is required";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

                        oldItem.Name=academiclevel.Name;
                        oldItem.Level=academiclevel.Level;
                        oldItem.MinHours=academiclevel.MinHours;
                        oldItem.MaxHours=academiclevel.MaxHours;
                        oldItem.ProgramId=academiclevel.ProgramId??null;
                        _unitOfWork.Academiclevels.Update(oldItem);
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
