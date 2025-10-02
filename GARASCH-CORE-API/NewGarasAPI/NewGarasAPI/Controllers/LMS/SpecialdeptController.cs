
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
    public class SpecialdeptController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        //private ApplicationDbContext _Context;
        public SpecialdeptController(IUnitOfWork unitOfWork , IMapper mapper , ITenantService tenantService)
        {
            // _Context = new ApplicationDbContext();
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllSpecialdept()
        {
            BaseResponseWithData<List<SpecialdeptDTO>> Response = new BaseResponseWithData<List<SpecialdeptDTO>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Specialdepts.GetAllAsync();
                    var dept = _unitOfWork.Depts.FindAll(x => x.Id > 0);
                    var TempData2 = TempData.Select(sub => new SpecialdeptDTO
                    {
                        Id = sub.Id,
                        Name = sub.Name,
                        deptartmentId = sub.DeptartmentId,
                        Namedept = dept.Where(x=>x.Id == sub.DeptartmentId).Select(y=>y.Name).FirstOrDefault() ?? " ",
                    }).ToList();
                    Response.Result=true;
                    Response.Data=TempData2;
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



        [HttpGet("GetAllSpecialdeptMapper")]
        public async Task<IActionResult> GetAllSpecialdeptMapper()
        {
            BaseResponseWithData<List<SpecialdeptDTO>> Response = new BaseResponseWithData<List<SpecialdeptDTO>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData =  _unitOfWork.Specialdepts.FindAll(x=>x.Id >0, new[] { "dept" } );
                    var TempData2 = _mapper.Map<List<SpecialdeptDTO>>(TempData);
                    Response.Result=true;
                    Response.Data=TempData2;
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
            BaseResponseWithData<Specialdept> Response = new BaseResponseWithData<Specialdept>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Specialdepts.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Specialdepts Id";
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

        [HttpGet("GetAcademicYearbyDeptId")]
        public async Task<IActionResult> GetAcademicYearbyDeptId([FromHeader] int DeptId)
        {
            BaseResponseWithData<List<SpecialdeptDTO2>> Response = new BaseResponseWithData<List<SpecialdeptDTO2>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            List<SpecialdeptDTO2> specialdeptsDB = new ();
            var specialdeptDB =  _unitOfWork.Specialdepts.FindAll(x=>x.Id > 0 , new[] {"dept"});
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(DeptId!=0)
                    {
                        specialdeptsDB=specialdeptDB.Where(x => x.DeptartmentId==DeptId).Select(a => new SpecialdeptDTO2
                        {
                            Id=a.Id ,
                            Name=a.Name ,
                            deptartmentId=a.DeptartmentId ,
                            dept=a.Deptartment.Name
                        }).ToList();

                    }
                    else
                    {
                        specialdeptsDB=specialdeptDB.Select(a => new SpecialdeptDTO2
                        {
                            Id=a.Id ,
                            Name=a.Name ,
                            deptartmentId=a.DeptartmentId ,
                            dept=a.Deptartment.Name
                        }).ToList();

                    }
                    Response.Result=true;
                    Response.Data=specialdeptsDB;
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
        public IActionResult AddAcademicYear(Specialdept specialdept)
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
                    var newspecialdept = _unitOfWork.Specialdepts.Add(specialdept);
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
        public IActionResult AddListAcademicYear(SpecialdeptViewModel Specialdepts)
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
                    var NewSpecialdepts = new List<Specialdept>();
                    foreach(var item in Specialdepts.Specialdepts)
                    {
                        _unitOfWork.Specialdepts.Add(item);
                        NewSpecialdepts.Add(item);
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

        [HttpPost("RemoveSpecialdept")]
        public IActionResult RemoveSpecialdept([FromHeader] int Id)
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
                    _unitOfWork.Specialdepts.Delete(_unitOfWork.Specialdepts.GetById(Id));
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
        public IActionResult Update([FromBody] Specialdept specialdept)
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

                    var oldItem = _unitOfWork.Specialdepts.GetById(specialdept.Id) ;
                    if(oldItem!=null)
                    {
                        oldItem.Name=specialdept.Name;
                        oldItem.DeptartmentId=specialdept.DeptartmentId;

                        _unitOfWork.Specialdepts.Update(oldItem);
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

        [HttpGet("GetAllcompetitonBySpecialDeptId")]
        public async Task<IActionResult> GetAllcompetitonBySpecialDeptId([FromHeader] int specialdeptId , [FromHeader] int levelId = 0)
        {
            BaseResponseWithData<List< FiltercompetitionBySpecialDeptViewModel>> Response = new BaseResponseWithData<List<FiltercompetitionBySpecialDeptViewModel>>();
            Response.Result=true;
            Response.Errors=new List<Error>();
            var test = new  List<FiltercompetitionBySpecialDeptViewModel>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                if(Response.Result)
                {
                    if(levelId==0)
                    {
                        var competition = _unitOfWork.AssignedSubjects.FindAll(x => x.SpecialdeptId == specialdeptId, new[] { "Specialdept", "Competition", "Academiclevel" }).ToList();
                        test=competition.Select(sup => new FiltercompetitionBySpecialDeptViewModel
                        {
                            CompetitionId=sup.CompetitionId ,
                            CompetitionName=sup.Competition.Name ,
                            SpecialDeptId=sup.SpecialdeptId ,
                            SpecialDeptName=sup.Specialdept.Name ,
                            DeptId=sup.Specialdept.DeptartmentId ,
                            LevelId=sup.AcademiclevelId ,
                            LevelName=sup.Academiclevel.Name ,
                            DeptName=_unitOfWork.Depts.FindAll(x => x.Id==sup.Specialdept.DeptartmentId).Select(y => y.Name).FirstOrDefault()
                        }).ToList();

                    }
                    else
                    {
                        var competition = _unitOfWork.AssignedSubjects.FindAll((x => x.SpecialdeptId == specialdeptId&& x.AcademiclevelId == levelId), new[] { "Specialdept", "Competition", "Academiclevel" }).ToList();
                        test=competition.Select(sup => new FiltercompetitionBySpecialDeptViewModel
                        {
                            CompetitionId=sup.CompetitionId ,
                            CompetitionName=sup.Competition.Name ,
                            SpecialDeptId=sup.SpecialdeptId ,
                            SpecialDeptName=sup.Specialdept.Name ,
                            DeptId=sup.Specialdept.DeptartmentId ,
                            LevelId=levelId ,
                            LevelName=sup.Academiclevel.Name ,
                            DeptName=_unitOfWork.Depts.FindAll(x => x.Id==sup.Specialdept.DeptartmentId).Select(y => y.Name).FirstOrDefault()
                        }).ToList();
                    }

                    Response.Result=true;
                    Response.Data=test;
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
