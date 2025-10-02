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
    public class SubjectController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg", ".svg" };
        private long _maxAllowedPosterSize = 15728640;
        private Microsoft.AspNetCore.Hosting.IHostingEnvironment _environment;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public SubjectController(IUnitOfWork unitOfWork , IMapper mapper , IHttpContextAccessor httpContextAccessor , Microsoft.AspNetCore.Hosting.IHostingEnvironment environment , ITenantService tenantService)
        {
            // _Context = new ApplicationDbContext();
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _httpContextAccessor=httpContextAccessor;
            _environment=environment;
            _tenantService=tenantService;
            _Context=new GarasTestContext(_tenantService);
            _helper=new Helper.Helper();

        }
        private string BaseURL
        {
            get
            {
                var uri = _httpContextAccessor?.HttpContext?.Request;
                string Host = uri?.Scheme + "://" + uri?.Host.Value.ToString();
                return Host;
            }
        }



        [HttpGet("GetAllSubjects")]
        public async Task<IActionResult> GetAllSubjects()
        {
            BaseResponseWithData<List<SubjectDto>> Response = new BaseResponseWithData<List<SubjectDto>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var SubjectRelationshipDB = _unitOfWork.SubjectRelationship.FindAll(x=>x.Id> 0 ,new []{"SubSubject"});
                    var TempData =  _unitOfWork.Subjects.FindAll(x=>x.Id > 0).Select(sub => new SubjectDto
                    {
                        Id = sub.Id,
                        Name = sub.Name,
                        Description = sub.Description,
                        Objective = sub.Objective,
                        ImagePath = sub.ImagePath != null ? BaseURL+sub.ImagePath : null,
                        Active = sub.Active,
                        Days = sub.Days,
                        StudyingHours = sub.StudyingHours,
                        Accreditedhours = sub.Accreditedhours,
                        RequiedofSubject = sub.RequiedofSubject,
                        SubjectScore = sub.SubjectScore,
                        Code = sub.Code,
                        CreationBy = sub.CreationBy,
                        CreationDate = sub.CreationDate,
                        ApprovedHours = sub.ApprovedHours,
                        GPAScale = sub. Gpascale,
                        subjectsListRequried = SubjectRelationshipDB.Where(x=>x.MainSubjectId == sub.Id && x.Status == "with").Select(a=> new SubjectsRequried
                        {
                            Id = a.SubSubjectId,
                            Name = a.SubSubject.Name
                        }).ToList(),
                        subjectsListReject = SubjectRelationshipDB.Where(x=>x.MainSubjectId == sub.Id && x.Status == "without").Select(a=> new SubjectsReject
                        {
                            Id = a.SubSubjectId,
                            Name = a.SubSubject.Name
                        }).ToList(),

                    }).ToList();
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




        [HttpGet("GetSubjectId")]
        public async Task<IActionResult> GetSubject([FromHeader] int Id)
        {
            BaseResponseWithData<Subject> Response = new BaseResponseWithData<Subject>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Subjects.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Subject Id";
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

        [HttpGet("GetSubjectbycode")]
        public  IActionResult GetSubjectbycode([FromHeader] string code)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                var TempData = new List<Subject>();
                if(Response.Result)
                {
                    TempData=_unitOfWork.Subjects.FindAll(x => x.Code==code).ToList();
                    if(TempData==null||TempData.Count()==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="This code does not exist.";
                        Response.Errors.Add(error);
                        return BadRequest(Response);


                    }
                    Response.Result=true;
                }
                return Ok(TempData);
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
        public IActionResult AddSubject(Subject subject)
        {

            BaseResponse Response = new BaseResponse();
            Response.Result=true;
            Response.Errors=new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;
                var TempData = new List<Subject>();
                if(Response.Result)
                {
                    var uniquTest = _unitOfWork.Subjects.FindAll(x => x.Name == subject.Name && x.Id != subject.Id).FirstOrDefault();
                    if(uniquTest==null)
                    {
                        var newhall = _unitOfWork.Subjects.Add(subject);
                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذه المادة مسجلة بالفعل ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

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

        [HttpPost("AddListSubjects")]
        public IActionResult AddListSubjects(SubjectViewModel subjects)
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
                    var NewSubjects = new List<Subject>();
                    foreach(var item in subjects.subjects)
                    {
                        var uniquTest = _unitOfWork.Subjects.FindAll(x => x.Name == item.Name && x.Id != item.Id).FirstOrDefault();
                        if(uniquTest==null)
                        {
                            _unitOfWork.Subjects.Add(item);
                            NewSubjects.Add(item);
                        }
                        else
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="هذه المادة مسجلة بالفعل ";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

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

        [HttpPost("AddListOfSubjects")]
        public IActionResult AddListOfSubjects(SubjectViewModel subjects)
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
                    var NewSubjects = new List<Subject>();
                    foreach(var item in subjects.subjects)
                    {
                        var uniquTest = _unitOfWork.Subjects.FindAll(x => x.Name == item.Name && x.Id != item.Id).FirstOrDefault();
                        if(uniquTest==null)
                        {
                            _unitOfWork.Subjects.Add(item);
                            NewSubjects.Add(item);
                        }
                        else
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="هذه المادة مسجلة بالفعل ";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }

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

        [HttpPost("RemoveSubject")]
        public IActionResult RemoveSubject([FromHeader] int Id)
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
                    var relationSubject = _unitOfWork.SubjectRelationship.FindAll(x=>x.SubSubjectId == Id || x.MainSubjectId == Id) ;
                    _unitOfWork.SubjectRelationship.DeleteRange(relationSubject);
                    _unitOfWork.Subjects.Delete(_unitOfWork.Subjects.GetById(Id));
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




        [HttpPost("UpdateSubject")]
        public IActionResult UpdateSubject([FromForm] SubjectUpdateDto dto)
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
                    if(dto.Image!=null)
                    {
                        if(!_allowedExtenstions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Image Only .png , .jpg , .jpeg and .svg images are allowed!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);

                        }
                        if(dto.Image.Length>_maxAllowedPosterSize)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="Max allowed size for Cover Image greater than 5MB!";
                            Response.Errors.Add(error);
                            return BadRequest(Response);


                        }
                    }



                    var uniquTest = _unitOfWork.Subjects.FindAll(x => x.Name == dto.Name && x.Id != dto.Id).FirstOrDefault();
                    if(uniquTest==null)
                    {
                        var oldItem = _unitOfWork.Subjects.GetById(dto.Id) ;

                        if(oldItem!=null)
                        {
                            //Image
                            string IMGPathDB = null;
                            if(dto.Image!=null)
                            {
                                IMGPathDB="Subjects/";
                                string SaveIMGPath = Path.Combine(this._environment.WebRootPath, IMGPathDB);
                                if(!System.IO.Directory.Exists(SaveIMGPath))
                                {
                                    System.IO.Directory.CreateDirectory(SaveIMGPath); //Create directory if it doesn't exist
                                }
                                string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_SubjectLogo_" + dto.Image.FileName.Trim().Replace(" ", "");
                                SaveIMGPath=SaveIMGPath+FileName;
                                using FileStream fileStream = new(SaveIMGPath, FileMode.Create);
                                IMGPathDB="/"+IMGPathDB+FileName;
                                dto.Image.CopyTo(fileStream);
                            }

                            if(!string.IsNullOrWhiteSpace(IMGPathDB))
                            {
                                // check if have image before for remove first
                                if(!string.IsNullOrWhiteSpace(oldItem.ImagePath))
                                {
                                    string webRootPath = _environment.WebRootPath;
                                    string fullPath = webRootPath + oldItem.ImagePath;
                                    if(System.IO.File.Exists(fullPath))
                                    {
                                        System.IO.File.Delete(fullPath);
                                    }
                                }
                                oldItem.ImagePath=IMGPathDB;
                            }



                            oldItem.Name=dto.Name;
                            oldItem.Description=dto.Description;
                            oldItem.CreationDate=dto.CreationDate;
                            oldItem.CreationBy=dto.CreationBy;
                            oldItem.Code=dto.Code;
                            oldItem.StudyingHours=dto.StudyingHours;
                            oldItem.RequiedofSubject=dto.RequiedofSubject;
                            oldItem.SubjectScore=dto.SubjectScore;
                            oldItem.Active=dto.Active;
                            oldItem.Objective=dto.Objective;
                            oldItem.Accreditedhours=dto.Accreditedhours;
                            oldItem.ApprovedHours=dto.ApprovedHours;
                            oldItem.Gpascale=dto.GPAScale;

                            _unitOfWork.Subjects.Update(oldItem);

                            var oldListSubjectRelationship =_unitOfWork.SubjectRelationship.FindAll(x => x.MainSubjectId==dto.Id);
                            if(oldListSubjectRelationship.Count()>0)
                            {
                                _unitOfWork.SubjectRelationship.DeleteRange(oldListSubjectRelationship);

                            }



                            if(dto.SubjectRelationshipLists?.Count()>0)
                            {
                                foreach(var item in dto.SubjectRelationshipLists)
                                {


                                    _unitOfWork.SubjectRelationship.Add(new SubjectRelationship
                                    {
                                        MainSubjectId=dto.Id ,
                                        SubSubjectId=item.SubSubjectId ,
                                        Status=item.Status
                                    });


                                }
                            }




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

                    }
                    else
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="هذه المادة مسجلة بالفعل ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

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
    }
}
