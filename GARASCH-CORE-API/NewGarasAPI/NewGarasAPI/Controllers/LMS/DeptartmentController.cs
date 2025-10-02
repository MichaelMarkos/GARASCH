

using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeptartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public DeptartmentController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {

            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();
        }
        [HttpGet]
        public async Task<IActionResult> GetAlldeptarment()
        {
            BaseResponseWithData<List<Dept>> Response = new BaseResponseWithData<List<Dept>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Depts.FindAllAsync(x => x.Id > 0);
                    Response.Result=true;
                    Response.Data=(List<Dept>)TempData;
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

        [HttpGet("GetdeptartmentbyId")]
        public async Task<IActionResult> Getdeptartment([FromHeader] int Id)
        {
            BaseResponseWithData<Dept> Response = new BaseResponseWithData<Dept>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Depts.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Dept Id";
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
        public IActionResult Adddept(Dept dept)
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
                    var newcatagory = _unitOfWork.Depts.Add(dept);
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

        [HttpPost("AddListCatagorys")]
        public IActionResult AddListCatagorys(DeptViewModel depts)
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
                    var Newdepts = new List<Dept>();
                    foreach(var item in depts.depts)
                    {
                        _unitOfWork.Depts.Add(item);
                        Newdepts.Add(item);
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


        [HttpPost("RemoveDept")]
        public IActionResult RemoveDept([FromHeader] int Id)
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
                    _unitOfWork.Depts.Delete(_unitOfWork.Depts.GetById(Id));
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
        public IActionResult Update([FromBody] Dept dept)
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
                    var oldItem = _unitOfWork.Depts.GetById(dept.Id) ;
                    if(oldItem!=null)
                    {
                        oldItem.Name=dept.Name;

                        _unitOfWork.Depts.Update(oldItem);
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
