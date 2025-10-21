
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Identity;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;
using System.Security.Claims;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg", ".svg" , ".pdf" , ".word" };
        private long _maxAllowedPosterSize = 15728640;
        private IHostingEnvironment _environment;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        //private ApplicationDbContext _Context;
        public NoticesController(IUnitOfWork unitOfWork , IMapper mapper , IHostingEnvironment Environment ,
                                           IHttpContextAccessor httpContextAccessor , ITenantService tenantService)
        {
            // _Context = new ApplicationDbContext();
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _environment=Environment;
            _httpContextAccessor=httpContextAccessor;
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
        private string ApplicationUserId
        {
            get
            {
                string UserId = User.FindFirstValue("uid");
                return UserId;
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetAllNotices()
        {
            BaseResponseWithData<List<Notice>> Response = new BaseResponseWithData<List<Notice>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {

                    var TempData = await _unitOfWork.Notices.GetAllAsync();
                    Response.Data=(List<Notice>)TempData;
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

        [HttpGet("GetNoticebyId")]
        public async Task<IActionResult> GetNotice([FromHeader] int Id)
        {
            BaseResponseWithData<Notice> Response = new BaseResponseWithData<Notice>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var TempData = await _unitOfWork.Notices.GetByIdAsync(Id);
                    if(TempData==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="Invalid Notice Id";
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

        [HttpGet("GetNoticebyspecialDeptIdAndLevelId")]
        public async Task<IActionResult> GetNoticebyspecialDeptIdAndLevelId([FromHeader] int specialDeptId , [FromHeader] int levelId)
        {
            BaseResponseWithData<List<Notice>> Response = new BaseResponseWithData<List<Notice>>();
            Response.Result=true;
            Response.Errors=new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var NoticeSpecailDeptAndLevelDB = _unitOfWork.NoticeSpecailDeptAndLevel.FindAll(x=>x.Id > 0 ,new [] {"Specialdept" ,"Academiclevel"});

                    var NoticesIds = NoticeSpecailDeptAndLevelDB.Where(x => (x.SpecialdeptId==specialDeptId&&x.AcademicYearId==levelId)).Select(n=>n.NoticesId);
                    var  noticesList= _unitOfWork.Notices.FindAll(x => NoticesIds.Contains(x.Id)).ToList();


                    Response.Result=true;
                    Response.Data=noticesList;
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


        // ---------------------------------------------- -1  for all -----------------------------------------------------

        [HttpPost]
        public IActionResult AddNotice([FromForm] NoticeDto dto)
        {
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);
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
                    if(dto.SpecialdeptId==0&&dto.AcademicYearId==0)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="يجب ادخال الدفعة والشعبة ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(dto.Date==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.ErrorMSG="يجب ادخال التاريخ ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    string ImagePath = null;
                    if(dto.Image!=null)
                    {

                        if(!_allowedExtenstions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="هذا الامتداد غير موجود";
                            Response.Errors.Add(error);
                            return BadRequest(Response);


                        }
                        if(dto.Image.Length>_maxAllowedPosterSize)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="لقد تجاوزت المساحة المسموحة";
                            Response.Errors.Add(error);
                            return BadRequest(Response);


                        }

                        string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + Path.GetExtension(dto.Image.FileName).ToLower();
                        ImagePath="Notice/";
                        string SaveImagePath = Path.Combine(this._environment.WebRootPath, ImagePath);
                        if(!System.IO.Directory.Exists(SaveImagePath))
                        {
                            System.IO.Directory.CreateDirectory(SaveImagePath); //Create directory if it doesn't exist
                        }
                        SaveImagePath=SaveImagePath+FileName;
                        using FileStream fileStream = new(SaveImagePath, FileMode.Create);
                        ImagePath="/"+ImagePath+FileName;
                        dto.Image.CopyTo(fileStream);
                    }
                    dto.Filepath=ImagePath;
                    var SpecialDeptDB = _unitOfWork.Specialdepts.FindAll(x=>x.Id > 0);
                    var LevelDB = _unitOfWork.Academiclevels.FindAll(x=>x.Id > 0);
                    //var 
                    if(dto.TypeId==1)
                    {
                        if(dto.SpecialdeptId==-1&&dto.AcademicYearId!=0&&dto.AcademicYearId!=-1)

                        {
                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 1
                            });
                            _unitOfWork.Complete();
                            foreach(var item in SpecialDeptDB)
                            {
                                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                                {
                                    NoticesId=notice.Id ,
                                    SpecialdeptId=item.Id ,
                                    AcademicYearId=dto.AcademicYearId
                                });
                            }

                        }

                        else if
                         (dto.AcademicYearId==-1&&dto.SpecialdeptId!=0&&dto.SpecialdeptId!=-1)
                        {
                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 1

                            });
                            _unitOfWork.Complete();
                            foreach(var item in LevelDB)
                            {
                                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                                {
                                    NoticesId=notice.Id ,
                                    SpecialdeptId=dto.SpecialdeptId ,
                                    AcademicYearId=item.Id ,
                                });
                            }

                        }
                        else if(dto.AcademicYearId==-1&&dto.SpecialdeptId==-1)

                        {


                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 1

                            });
                            _unitOfWork.Complete();
                            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                            {
                                NoticesId=notice.Id ,

                            });

                        }

                        else

                        {
                            var notice = _unitOfWork.Notices.Add(new  Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date,
                                CreationDate = DateTime.Now ,
                                CompetitionId = dto.CompetitionId,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 1
                            });
                            _unitOfWork.Complete();
                            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                            {
                                NoticesId=notice.Id ,
                                SpecialdeptId=dto.SpecialdeptId ,
                                AcademicYearId=dto.AcademicYearId ,
                            });
                        }
                    }
                    if(dto.TypeId==2)
                    {
                        if(dto.SpecialdeptId==-1&&dto.AcademicYearId!=0&&dto.AcademicYearId!=-1)

                        {
                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 2
                            });
                            _unitOfWork.Complete();
                            foreach(var item in SpecialDeptDB)
                            {

                                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                                {
                                    NoticesId=notice.Id ,
                                    SpecialdeptId=item.Id ,
                                    AcademicYearId=dto.AcademicYearId ,
                                });
                            }

                        }

                        else if
                         (dto.AcademicYearId==-1&&dto.SpecialdeptId!=0&&dto.SpecialdeptId!=-1)
                        {
                            var notice =   _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId=2,

                            });
                            _unitOfWork.Complete();
                            foreach(var item in LevelDB)
                            {
                                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                                {
                                    NoticesId=notice.Id ,
                                    SpecialdeptId=dto.SpecialdeptId ,
                                    AcademicYearId=item.Id ,
                                });
                            }

                        }
                        else if(dto.AcademicYearId==-1&&dto.SpecialdeptId==-1)

                        {


                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 2

                            });
                            _unitOfWork.Complete();

                            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                            {
                                NoticesId=notice.Id ,

                            });
                        }

                        else

                        {
                            var notice = _unitOfWork.Notices.Add(new  Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date,
                                CreationDate = DateTime.Now ,
                                CompetitionId = dto.CompetitionId,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 2
                            });
                            _unitOfWork.Complete();

                            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                            {
                                NoticesId=notice.Id ,
                                SpecialdeptId=dto.SpecialdeptId ,
                                AcademicYearId=dto.AcademicYearId ,
                            });
                        }
                    }
                    if(dto.TypeId==3)
                    {
                        if(dto.SpecialdeptId==-1&&dto.AcademicYearId!=0&&dto.AcademicYearId!=-1)

                        {
                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationBy=validation.userID ,
                                CreationDate = DateTime.Now ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId = 3
                            });
                            _unitOfWork.Complete();

                            foreach(var item in SpecialDeptDB)
                            {
                                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                                {
                                    NoticesId=notice.Id ,
                                    SpecialdeptId=item.Id ,
                                    AcademicYearId=dto.AcademicYearId ,
                                });
                            }

                        }

                        else if
                         (dto.AcademicYearId==-1&&dto.SpecialdeptId!=0&&dto.SpecialdeptId!=-1)
                        {
                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 3

                            });
                            _unitOfWork.Complete();
                            foreach(var item in LevelDB)
                            {
                                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                                {
                                    NoticesId=notice.Id ,
                                    SpecialdeptId=dto.SpecialdeptId ,
                                    AcademicYearId=item.Id ,
                                });
                            }

                        }
                        else if(dto.AcademicYearId==-1&&dto.SpecialdeptId==-1)

                        {


                            var notice = _unitOfWork.Notices.Add(new Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date ,
                                CreationDate = DateTime.Now ,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 3

                            });
                            _unitOfWork.Complete();
                            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                            {
                                NoticesId=notice.Id ,

                            });

                        }

                        else

                        {
                            var notice =  _unitOfWork.Notices.Add(new  Notice
                            {

                                Thetopic=dto.Thetopic ,
                                Description=dto.Description ,
                                Filepath=dto.Filepath ,
                                Date=(DateTime)dto.Date,
                                CreationDate = DateTime.Now ,
                                CompetitionId = dto.CompetitionId,
                                CreationBy=validation.userID ,
                                NewsOrAlertsFlag=dto.NewsOrAlertsFlag ,
                                ReceiverTypeId= 3
                            });
                            _unitOfWork.Complete();
                            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
                            {
                                NoticesId=notice.Id ,
                                SpecialdeptId=dto.SpecialdeptId ,
                                AcademicYearId=dto.AcademicYearId ,

                            });
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



        [HttpPost("RemoveNotice")]
        public IActionResult RemoveNotice([FromHeader] int Id)
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
                    var oldNotice = _unitOfWork.Notices.GetById(Id);
                    var oldNoticeSpecailDeptAndLevel = _unitOfWork.NoticeSpecailDeptAndLevel.FindAll(x=>x.NoticesId == oldNotice.Id);
                    _unitOfWork.NoticeSpecailDeptAndLevel.DeleteRange(oldNoticeSpecailDeptAndLevel);
                    _unitOfWork.Notices.Delete(oldNotice);
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




        //[HttpPost("UpdateNOtices")]
        //public IActionResult UpdateNOtices([FromForm] NoticeDto dto)
        //{
        //    BaseResponse Response = new BaseResponse();
        //    Response.Errors=new List<string>();
        //    Response.Result=false;


        //    if(dto.Id==0)
        //    {
        //        Response.Result=false;
        //        Error error = new Error();
        //        error.ErrorCode="Err10";
        //        error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(error);
        //        return BadRequest(Response);

        //        Response.Result=false;
        //        Response.Errors.Add("يجب ادخال الرقم التعريفي ");
        //        return Ok(Response);

        //    }
        //    var oldNotice = _unitOfWork.Notices.GetById(dto.Id);
        //    if(oldNotice==null)
        //    {
        //        Response.Result=false;
        //        Error error = new Error();
        //        error.ErrorCode="Err10";
        //        error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(error);
        //        return BadRequest(Response);
        //        Response.Result=false;
        //        Response.Errors.Add("لا يوجد اخبار او تنبيهات بهذا الرقم التعريفي");
        //        return Ok(Response);
        //    }
        //    try
        //    {
        //        var SpecialDeptDB = _unitOfWork.Specialdepts.FindAll(x=>x.Id > 0);
        //        var LevelDB = _unitOfWork.Academiclevels.FindAll(x=>x.Id > 0);

        //        string ImagePath = null;
        //        if(dto.Image!=null)
        //        {

        //            if(!_allowedExtenstions.Contains(Path.GetExtension(dto.Image.FileName).ToLower()))
        //            {
        //                Response.Result=false;
        //                Error error = new Error();
        //                error.ErrorCode="Err10";
        //                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
        //                Response.Errors.Add(error);
        //                return BadRequest(Response);
        //                Response.Result=false;
        //                Response.Errors.Add("هذا الامتداد غير موجود");
        //                return Ok(Response);

        //            }
        //            if(dto.Image.Length>_maxAllowedPosterSize)
        //            {
        //                Response.Result=false;
        //                Error error = new Error();
        //                error.ErrorCode="Err10";
        //                error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
        //                Response.Errors.Add(error);
        //                return BadRequest(Response);
        //                Response.Result=false;
        //                Response.Errors.Add("لقد تجاوزت المساحة المسموحة");
        //                return Ok(Response);

        //            }

        //            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + dto.Image.FileName.Trim().Replace(" ", "");
        //            ImagePath="Notice/";
        //            string SaveImagePath = Path.Combine(this._environment.WebRootPath, ImagePath);
        //            if(!System.IO.Directory.Exists(SaveImagePath))
        //            {
        //                System.IO.Directory.CreateDirectory(SaveImagePath); //Create directory if it doesn't exist
        //            }
        //            if(!string.IsNullOrWhiteSpace(oldNotice.Filepath))
        //            {
        //                string webRootPath = _environment.WebRootPath;
        //                string fullPath = webRootPath + oldNotice.Filepath;
        //                if(System.IO.File.Exists(fullPath))
        //                {
        //                    System.IO.File.Delete(fullPath);
        //                }
        //            }
        //            SaveImagePath=SaveImagePath+FileName;
        //            using FileStream fileStream = new(SaveImagePath, FileMode.Create);
        //            ImagePath="/"+ImagePath+FileName;
        //            dto.Image.CopyTo(fileStream);

        //        }
        //        else
        //        {
        //            dto.Filepath=oldNotice.Filepath;
        //        }
        //        var oldNoticeSpecailDeptAndLevel = _unitOfWork.NoticeSpecailDeptAndLevel.FindAll(x=>x.NoticesId == oldNotice.Id);
        //        _unitOfWork.NoticeSpecailDeptAndLevel.DeleteRange(oldNoticeSpecailDeptAndLevel);


        //        if(dto.SpecialdeptId==-1&&dto.AcademicYearId!=0&&dto.AcademicYearId!=-1)

        //        {

        //            foreach(var item in SpecialDeptDB)
        //            {
        //                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
        //                {
        //                    NoticesId=dto.Id ,
        //                    SpecialdeptId=item.Id ,
        //                    AcademicYearId=dto.AcademicYearId
        //                });
        //            }

        //        }

        //        else if
        //         (dto.AcademicYearId==-1&&dto.SpecialdeptId!=0&&dto.SpecialdeptId!=-1)
        //        {

        //            foreach(var item in LevelDB)
        //            {
        //                _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
        //                {
        //                    NoticesId=dto.Id ,
        //                    SpecialdeptId=dto.SpecialdeptId ,
        //                    AcademicYearId=item.Id ,
        //                });
        //            }

        //        }
        //        else if(dto.AcademicYearId==-1&&dto.SpecialdeptId==-1)

        //        {

        //            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
        //            {
        //                NoticesId=dto.Id ,

        //            });

        //        }
        //        else

        //        {

        //            _unitOfWork.NoticeSpecailDeptAndLevel.Add(new NoticeSpecailDeptAndLevel
        //            {
        //                NoticesId=dto.Id ,
        //                SpecialdeptId=dto.SpecialdeptId ,
        //                AcademicYearId=dto.AcademicYearId ,
        //            });
        //        }
        //        _mapper.Map(dto , oldNotice);


        //        if(dto.Image!=null)
        //        {
        //            oldNotice.Filepath=ImagePath;

        //        }




        //        var updatedBuilding = _unitOfWork.Notices.Update(oldNotice);
        //        _unitOfWork.Complete();
        //        Response.Result=true;
        //        return Ok(Response);
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        Error error = new Error();
        //        error.ErrorCode="Err10";
        //        error.ErrorMSG="Exception :"+(ex.InnerException!=null ? ex.InnerException.Message : ex.Message);
        //        Response.Errors.Add(error);
        //        return BadRequest(Response);
        //    }
        //}

        [HttpDelete("RemoveCompetitionUser")]
        public IActionResult RemoveCompetitionUser([FromHeader] int Id)
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
                    _unitOfWork.CompetitionUsers.Delete(_unitOfWork.CompetitionUsers.GetById(Id));
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





        [HttpGet("GetAllNoticsPagedList")]
        public async Task<IActionResult> GetAllPages([FromHeader] int? specialDeptId , [FromHeader] int? levelId , [FromHeader] bool? NewsOrAlertsFlag ,
                                                       [FromHeader] long userId , [FromHeader] int PageNo = 1 , [FromHeader] int NoOfItems = 50)
        {

            ResponsePagelistNoticesVM Response = new ResponsePagelistNoticesVM();
            Response.Result=true;
            Response.Errors=new List<Error>();

            var userIsdoctor = false;
            var userIsstudent = false;
            var userIsadminCompetition = false;
            var userIsAdmin = false;
            //userId=ApplicationUserId;

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors=validation.errors;
                Response.Result=validation.result;

                if(Response.Result)
                {
                    var noticesDB = _unitOfWork.Notices.FindAll(x=>x.Id > 0 ,new [] {"CreationByNavigation"});
                    var NoticeSpecailDeptAndLevelDB = _unitOfWork.NoticeSpecailDeptAndLevel.FindAll(x=>x.Id > 0 ,new [] {"Specialdept" ,"Academiclevel"});



                    var noticesList  = new List<Notice>();

                    if(userId>0)
                    {
                        var _user =  _unitOfWork.HrUsers.FindAll(x=>x.Id == userId).FirstOrDefault();
                        if(_user==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="هذا المستخدم غير موجود";
                            Response.Errors.Add(error);
                            return BadRequest(Response);
                        }

                        var userRoles = _unitOfWork.UserRoles.FindAllQueryable(x=>x.Id > 0 ,new[]{"Role"}).Select(n=>n.Role.Name);

                        userIsAdmin=(userRoles.Any()&&userRoles.Contains("admin"));
                        userIsstudent=(userRoles.Any()&&userRoles.Contains("student"));
                        userIsdoctor=(userRoles.Any()&&userRoles.Contains("doctor"));
                        userIsadminCompetition=(userRoles.Any()&&userRoles.Contains("adminCompetition"));
                        // var userIsadmincontrol = (userRoles.Any()&&userRoles.Contains( "admincontrol"));


                        if(!(userIsstudent||userIsdoctor||userIsAdmin||userIsadminCompetition))
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="ليس لديك صلاحية لهذا الامر";
                            Response.Errors.Add(error);
                            return BadRequest(Response);


                        }
                    }


                    if(userIsstudent)
                    {
                        if(specialDeptId==null&&levelId==null)
                        {
                            Response.Result=false;
                            Error error = new Error();
                            error.ErrorCode="Err10";
                            error.ErrorMSG="  يجب تحديد القسم والسنة  ";
                            Response.Errors.Add(error);
                            return BadRequest(Response);


                        }

                        if(specialDeptId!=null&&levelId!=null)
                        {
                            var NoticesIds = NoticeSpecailDeptAndLevelDB.Where(x => (x.SpecialdeptId==specialDeptId&&x.AcademicYearId==levelId)||(x.SpecialdeptId==null&&x.AcademicYearId==null)).Select(n=>n.NoticesId);
                            noticesList=noticesDB.Where(x => (x.ReceiverTypeId==1||x.ReceiverTypeId==3)&&NoticesIds.Contains(x.Id)).ToList();

                        }


                    }


                    if(userIsdoctor||userIsadminCompetition)
                    {
                        var competitionsiDS =  _unitOfWork.CompetitionMemberAdmins.FindAll(x => x.HrUserId == userId , new[] { "Competition" }).Select(y => y.Competition.Id).ToList();
                        var AssignedSubject = _unitOfWork.AssignedSubjects .FindAll(X=> competitionsiDS.Contains(X.CompetitionId));
                        var noticesList2  = new List<Notice>();

                        foreach(var item in AssignedSubject)
                        {
                            var notice = new Notice();
                            var NoticesIds = NoticeSpecailDeptAndLevelDB.Where(x => (x.SpecialdeptId==item.SpecialdeptId&&x.AcademicYearId==item.AcademiclevelId)||(x.SpecialdeptId==null&&x.AcademicYearId==null)).Select(n=>n.NoticesId);
                            notice=_unitOfWork.Notices.FindAll(x => (NoticesIds.Contains(x.Id))&&(x.ReceiverTypeId==2||x.ReceiverTypeId==3)).FirstOrDefault();
                            if(notice!=null)
                            {
                                noticesList2.Add(notice);

                            }
                        }


                        if(noticesList2!=null)
                        {
                            noticesList=noticesList2.Distinct().ToList();

                        }
                    }
                    if(userIsAdmin)
                    {
                        noticesList=noticesDB.ToList();
                    }

                    if(NewsOrAlertsFlag!=null)
                    {
                        noticesList=noticesList.Where(x => x.NewsOrAlertsFlag==NewsOrAlertsFlag).ToList();
                    }


                    //------------------------------------------------ without AutoMapper ----------------------------------------------------------------------------

                    // var noticesPagedList = PagedList<Notices>.Create(noticesList.AsQueryable().OrderBy(x => x.Date), PageNo, NoOfItems);
                    // Response.NoticsItemsList = noticesPagedList;
                    // and change type of ResponsePagelistNoticesVM 
                    var tempdata = new List<NoticesDto>();

                    var competitionIds = noticesList
        .Where(n => n.CompetitionId.HasValue)
        .Select(n => n.CompetitionId.Value)
        .Distinct()
        .ToList();

                    var competitions = _unitOfWork.Competitions
                        .FindAll(x => competitionIds.Contains(x.Id))
                        .Select(y => new { y.Id, y.Name })
                        .ToList();






                    var noticesPagedList = PagedList<Notice>.Create(noticesList.AsQueryable().OrderByDescending(x => x.Date), PageNo, NoOfItems);


                    tempdata=noticesPagedList.Select(a => new NoticesDto
                    {
                        Id=a.Id ,
                        Description=a.Description ,
                        Thetopic=a.Thetopic ,
                        Filepath=a.Filepath!=null ? BaseURL+a.Filepath : null ,
                        Date=a.Date?.ToString("yyyy-MM-dd HH:mm:ss") ,
                        FullName=a.CreationByNavigation !=null ? (a.CreationByNavigation?.FirstName+" "+a.CreationByNavigation?.MiddleName+" "+a.CreationByNavigation?.LastName) : null ,
                        ImageOfUser=a.CreationByNavigation?.ImgPath!=null ? BaseURL+a.CreationByNavigation.ImgPath : null ,
                        CreationBy=a.CreationBy.ToString() ,
                        SpecialdeptId=NoticeSpecailDeptAndLevelDB.Where(x => x.NoticesId==a.Id).Select(s => s.SpecialdeptId).FirstOrDefault() ,
                        AcademicYearId=NoticeSpecailDeptAndLevelDB.Where(x => x.NoticesId==a.Id).Select(s => s.AcademicYearId).FirstOrDefault() ,
                        NewsOrAlertsFlag=a.NewsOrAlertsFlag ,
                        CompetitionId=a.CompetitionId ,
                        CompetitionName=competitions.Where(x => x.Id==a.CompetitionId).Select(w => w.Name).FirstOrDefault() ,
                        AcademicYearName=NoticeSpecailDeptAndLevelDB.Where(x => x.NoticesId==a.Id).Select(s => s.AcademicYear?.Name).FirstOrDefault() ,
                        SpecialdeptName=NoticeSpecailDeptAndLevelDB.Where(x => x.NoticesId==a.Id).Select(s => s.Specialdept?.Name).FirstOrDefault() ,
                        receiverTypeName=a.ReceiverTypeId==1 ? "طلاب" : a.ReceiverTypeId==2 ? "دكتور" : a.ReceiverTypeId==2 ? "دكاترةوطلاب" : null ,
                    }).ToList();



                    Response.data=tempdata;
                    Response.PaginationHeader=new PaginationHeader
                    {
                        CurrentPage=PageNo ,
                        TotalPages=noticesPagedList.TotalPages ,
                        ItemsPerPage=NoOfItems ,
                        TotalItems=noticesPagedList.TotalCount
                    };
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
