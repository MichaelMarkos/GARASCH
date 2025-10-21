

using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.LMS;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.LMS;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.LMS;
using System.Data;
using System.Security.Claims;
using System.Text;
using Extension = NewGarasAPI.Helpers.Extension;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;


namespace NewGarasAPI.Controllers.LMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompetitionDayUserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private IPermissionService _permissionService;
        IWebHostEnvironment _hostEnvironment;
        private IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthLMsService _authService;
        private Helper.Helper _helper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;

        public CompetitionDayUserController(IAuthLMsService authService , IUnitOfWork unitOfWork , IMapper mapper ,
                                            IWebHostEnvironment hostEnvironment , IPermissionService permissionService ,
                                            IHostingEnvironment Environment , IHttpContextAccessor httpContextAccessor , ITenantService tenantService)
        {
            _unitOfWork=unitOfWork;
            _mapper=mapper;
            _hostEnvironment=hostEnvironment;
            _permissionService=permissionService;
            _environment=Environment;
            _httpContextAccessor=httpContextAccessor;
            _authService=authService;
            _tenantService=tenantService;
        }
        private string ApplicationUserId
        {
            get
            {
                string UId = User.FindFirstValue("uid");
                return UId;
            }
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


        [Authorize]
        //[Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateUsersScoresExcel([FromForm] CompetitionDayUserExcelCreateDTO dto , [FromHeader] long HrUserId)
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

                    if(dto==null)
                        throw new Exception("Object is not received...");

                    if(dto.ExcelSheet==null)
                        throw new Exception("File is Not Received...");

                    if(dto.CompetitionDayId==null)
                        throw new Exception("CompetitionDayId is Required...");

                    decimal? fromScore = null;


                    var CheckCompetitionDayDb =  await _unitOfWork.CompetitionDays.GetByIdAsync((int)dto.CompetitionDayId);

                    if(CheckCompetitionDayDb==null)
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="Invalid CompetitionDayId";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    if(!await _permissionService.CheckUserHasPermissionManageCompetition(HrUserId , CheckCompetitionDayDb.CompetitionId))
                    {
                        Response.Result=false;
                        Error error = new Error();
                        error.ErrorCode="Err10";
                        error.errorMSG="ليس لديك صلاحية ";
                        Response.Errors.Add(error);
                        return BadRequest(Response);

                    }
                    // Create the Directory if it is not exist
                    string dirPath = Path.Combine(_hostEnvironment.WebRootPath, "ReceivedReports");

                    //Get Data set
                    DataSet CompetitionDayUserDs = Extension.CreateDsOfExcelFile(dto.ExcelSheet, dirPath);

                    if(CompetitionDayUserDs!=null&&CompetitionDayUserDs.Tables.Count>0)
                    {
                        // Read the the Table
                        DataTable competitionDayUsers = CompetitionDayUserDs.Tables[0];
                        List<long> users = new List<long>();
                        for(int i = 1 ;i<competitionDayUsers.Rows.Count ;i++)
                        {
                            CompetitionDayUser competitionDayUser = new CompetitionDayUser();
                            var UserName = competitionDayUsers.Rows[i][2].ToString();
                            var PhoneNumber = competitionDayUsers.Rows[i][3].ToString();
                            var ExcelUserId = competitionDayUsers.Rows[i][5].ToString();

                            HrUser HrUser = new();
                            var HrUserMobilesDb = _unitOfWork.HrUserMobiles.FindAll(x=> x.Id > 0, new []{"HrUser"})  ;

                            if(!string.IsNullOrWhiteSpace(ExcelUserId))
                            {
                                HrUser=await _unitOfWork.HrUsers.FindAsync(c => c.Id==long.Parse(ExcelUserId));

                                if(HrUser==null&&!string.IsNullOrWhiteSpace(PhoneNumber))
                                {
                                    HrUser=HrUserMobilesDb.Where(x => x.MobileNumber==PhoneNumber).Select(r => r.HrUser).FirstOrDefault();
                                }
                            }
                            else if(!string.IsNullOrWhiteSpace(PhoneNumber))
                            {
                                HrUser=HrUserMobilesDb.Where(x => x.MobileNumber==PhoneNumber).Select(r => r.HrUser).FirstOrDefault();
                            }
                            //ApplicationUser applicationUser = await _unitOfWork.ApplicationUsers.FindAsync(c => (ExcelUserId == null || c.Id == ExcelUserId) || (UserName == null || c.FirstName + " " + c.MiddleName + " " + c.LastName == UserName) || (c.PhoneNumber == PhoneNumber));
                            //ApplicationUser applicationUser = await _unitOfWork.ApplicationUsers.FindAsync(c => ExcelUserId != null && PhoneNumber != null ? c.Id == ExcelUserId || c.PhoneNumber == PhoneNumber : PhoneNumber != null ? c.PhoneNumber == PhoneNumber : c.FirstName + " " + c.MiddleName + " " + c.LastName == UserName);

                            if(HrUser==null)
                            {
                                Response.Result=false;
                                Error error = new Error();
                                error.ErrorCode="Err10";
                                error.errorMSG="This User "+UserName+" Doesn't Exist";
                                Response.Errors.Add(error);


                            }
                            else
                            {
                                var IsAlreadyExist = await _unitOfWork.CompetitionDayUsers.FindAllAsync(a => a.HrUserId == HrUser.Id && a.CompetitionDayId == dto.CompetitionDayId, new[] { "CompetitionDay" } );
                                if(IsAlreadyExist!=null&&IsAlreadyExist.Any())
                                {
                                    Response.Result=false;
                                    Error error = new Error();
                                    error.ErrorCode="Err10";
                                    error.errorMSG="This User "+UserName+" Finished This Competition Day";
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    if(users.Any(a => a==HrUser.Id))
                                    {
                                        Response.Result=false;
                                        Error error = new Error();
                                        error.ErrorCode="Err10";
                                        error.errorMSG="This User "+UserName+" Repeated In Users List";
                                        Response.Errors.Add(error);

                                    }
                                    else
                                    {
                                        users.Add(HrUser.Id);
                                        competitionDayUser.CreationDate=DateTime.Now;
                                        competitionDayUser.HrUserId=HrUser.Id;
                                        competitionDayUser.CompetitionDayId=(int)dto.CompetitionDayId;
                                        var Score = competitionDayUsers.Rows[i][1].ToString();
                                        if(!string.IsNullOrWhiteSpace(Score))
                                        {
                                            competitionDayUser.UserScore=decimal.Parse(Score.Trim());
                                            competitionDayUser.FromScore=CheckCompetitionDayDb.FromScore;
                                            competitionDayUser.IsFinished=true;
                                        }
                                        else
                                        {
                                            competitionDayUser.IsFinished=false;
                                        }

                                        competitionDayUser.CreationBy=ApplicationUserId;
                                        var Inserted = await _unitOfWork.CompetitionDayUsers.AddAsync(competitionDayUser);
                                        if(Inserted!=null)
                                        {
                                            // Update Total Score
                                            var competitionUser = await _unitOfWork.CompetitionUsers.FindAsync(c => c.CompetitionId == CheckCompetitionDayDb.CompetitionId && c.HrUserId == HrUser.Id);
                                            if(competitionUser!=null)
                                            {
                                                competitionUser.TotalScore+=competitionDayUser.UserScore??0;
                                                _unitOfWork.CompetitionUsers.Update(competitionUser);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        _unitOfWork.Complete();
                    }
                }
                return Ok(Response);
            }
            catch(Exception ex)
            {
                Response.Result=false;
                Error error = new Error();
                error.ErrorCode="Err10";
                error.errorMSG=ex.InnerException!=null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return BadRequest(Response);
            }

        }

        //[HttpPost("InsertStudentsExcel")]
        //public async Task<IActionResult> InsertStudentsExcel([FromForm] InsertUserExcelDTO dto)
        //{
        //    BaseResponseWithData<string> Response = new  BaseResponseWithData<string>();
        //    Response.Result=true;
        //    Response.Errors=new List<Error>();
        //    var errorList = new List<Error>();

        //    try
        //    {
        //        if(dto==null)
        //            throw new Exception("Object is not received...");

        //        if(dto.ExcelSheet==null)
        //            throw new Exception("File is Not Received...");

        //        // Create the Directory if it is not exist
        //        string dirPath = Path.Combine(_hostEnvironment.WebRootPath, "ReceivedReports");

        //        // Get Data set
        //        DataSet competitionDayUserDs = Extension.CreateDsOfExcelFile(dto.ExcelSheet, dirPath);

        //        var allUsers = _unitOfWork.HrUsers.FindAll(x=>x.Id > 0 ).ToList(); // Load all users in memory for quick lookups
        //        var allHrUserMobiles = _unitOfWork.HrUserMobiles.FindAll(x=> allUsers.Select(i=>i.Id).Contains(x.HrUserId));
        //        var allEmails = new HashSet<string>(allUsers.Select(u => u.Email));
        //        var allPhoneNumbers = new HashSet<string>(allHrUserMobiles.Select(u => u.MobileNumber));
        //        var allNames = new HashSet<string>(allUsers.Select(u => u.FirstName+" "+u.MiddleName+" "+u.LastName));

        //        if(competitionDayUserDs!=null&&competitionDayUserDs.Tables.Count>0)
        //        {
        //            DataTable competitionDayUsers = competitionDayUserDs.Tables[0];
        //            List<HrUser> newUsers = new List<HrUser>();
        //            List<WhatsAppAndEmailDataFOrUsersVM> UsersForWhatsAndEmail = new List<WhatsAppAndEmailDataFOrUsersVM>();
        //            List<UserDepartment> userDepartments = new List<UserDepartment>();
        //            List<CompetitionUser> competitionUsers = new List<CompetitionUser>();
        //            List<string> errorMessages = new List<string>();

        //            Random random = new Random();

        //            // Process each row in the Excel file
        //            for(int i = 1 ;i<competitionDayUsers.Rows.Count ;i++)
        //            {
        //                var row = competitionDayUsers.Rows[i];
        //                if(string.IsNullOrWhiteSpace(row [11].ToString()))
        //                    continue;

        //                var firstName = row[0].ToString();
        //                var middleName = row[1].ToString();
        //                var lastName = row[2].ToString();
        //                var phoneNumber = row[3].ToString();
        //                var levelId = row[11].ToString();
        //                var specialDeptId = row[5].ToString();
        //                var specialDept = row[6].ToString();
        //                var email = row[7].ToString() ?? null;
        //                var nationalityId = row[8].ToString() ?? null;
        //                var yearId = row[12].ToString() ?? null;
        //                var role = row[10].ToString();
        //                var subscribeToSubjects = row[13].ToString();

        //                // Validation
        //                if(string.IsNullOrWhiteSpace(firstName)||string.IsNullOrWhiteSpace(middleName)
        //                    ||string.IsNullOrWhiteSpace(lastName)||string.IsNullOrWhiteSpace(phoneNumber)
        //                    ||string.IsNullOrWhiteSpace(levelId)||string.IsNullOrWhiteSpace(specialDeptId)
        //                    ||string.IsNullOrWhiteSpace(specialDept))
        //                {
        //                    errorMessages.Add($"This User {firstName} {middleName} {lastName} Some Parameters is Empty");
        //                    continue;
        //                }

        //                if(allEmails.Contains(email))
        //                {
        //                    errorMessages.Add($"This User {firstName} {middleName} {lastName} Email is already registered");
        //                    continue;
        //                }

        //                if(allPhoneNumbers.Contains(phoneNumber))
        //                {
        //                    errorMessages.Add($"This User {firstName} {middleName} {lastName} PhoneNumber is already registered");
        //                    continue;
        //                }

        //                if(allNames.Contains(firstName+" "+middleName+" "+lastName))
        //                {
        //                    errorMessages.Add($"This User {firstName} {middleName} {lastName} FullName is already registered");
        //                    continue;
        //                }

        //                HrUser newUser = new HrUser
        //                {
        //                    FirstName = firstName,
        //                    MiddleName = middleName,
        //                    LastName = lastName,
        //                    email = email,
        //                    PhoneNumber = phoneNumber,
        //                    Email = email,
        //                    NationalId = nationalityId,
        //                    Active = true,
        //                    SerialNum = $"S-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}",
        //                };

        //                var password = random.Next(1, 9999).ToString();
        //                var result = await _userManager.CreateAsync(newUser, password);
        //                if(result.Succeeded)
        //                {
        //                    var resultRole = await _userManager.AddToRoleAsync(newUser, role);
        //                    newUsers.Add(newUser);


        //                    UsersForWhatsAndEmail.Add(new WhatsAppAndEmailDataFOrUsersVM
        //                    {
        //                        FirstName=firstName ,
        //                        MiddleName=middleName ,
        //                        LastName=lastName ,
        //                        password=password ,
        //                        Email=email ,
        //                        PhoneNumber=phoneNumber
        //                    });

        //                    userDepartments.Add(new UserDepartment
        //                    {
        //                        UserId=newUser.Id ,
        //                        SpecialdeptId=int.Parse(specialDeptId) ,
        //                        AcademiclevelId=int.Parse(levelId) ,
        //                        YearId=int.Parse(yearId)
        //                    });

        //                    if(subscribeToSubjects=="subscribe")
        //                    {
        //                        var termIds = _unitOfWork.AcademicYears.FindAll(x => x.YearId == int.Parse(yearId)).ToList();
        //                        var competitionIds = termIds.SelectMany(item =>
        //                    _unitOfWork.AssignedSubjects.FindAll(x => x.AcademiclevelId == int.Parse(levelId)
        //                                                               && x.SpecialdeptId == int.Parse(specialDeptId)
        //                                                               && x.AcademicYearId == item.Id)
        //                    .Select(y => y.CompetitionId)).ToList();

        //                        competitionUsers.AddRange(competitionIds.Select(id =>
        //                            new CompetitionUser
        //                            {
        //                                CompetitionId=id ,
        //                                ApplicationUserId=newUser.Id ,
        //                                CreationDateOFApprovedAndReject=DateTime.Now ,
        //                                CreationBy=ApplicationUserId ,
        //                                EnrollmentStatus="approved" ,
        //                                TotalScore=0
        //                            }));
        //                    }

        //                }
        //                else
        //                {
        //                    errorMessages.Add($"This User {firstName} {middleName} {lastName} Not registered");
        //                }
        //            }

        //            // Handle bulk insert of new users, user departments, and competition users
        //            await _unitOfWork.UserDepartment.AddRangeAsync(userDepartments);
        //            await _unitOfWork.CompetitionUsers.AddRangeAsync(competitionUsers);

        //            // Send file URL or other tasks asynchronously
        //            // Assuming file URL handling can be refactored to run in parallel
        //            var WhatsAppMessage = WhatsAppMessageMethod(dto, UsersForWhatsAndEmail);
        //            var EmailMessage = EmailMessageMethod( UsersForWhatsAndEmail);

        //            await Task.WhenAll(WhatsAppMessage);

        //            _unitOfWork.Complete();


        //            string LOGPath = null;
        //            if(errorMessages.Any())
        //            {
        //                string logsPath = Path.Combine(_hostEnvironment.WebRootPath, "Logs");


        //                //string filePath = Path.Combine(_hostEnvironment.WebRootPath, "Logs", "StudentsErrorLog.txt");

        //                //// Check if the file exists
        //                //if(File.Exists(filePath))
        //                //{
        //                //    // Clear the contents of the file by writing an empty string to it
        //                //    File.WriteAllText(filePath , string.Empty);
        //                //}



        //                // string filePath = System.IO.Path.Combine(_hostEnvironment.WebRootPath, "Logs/StudentsErrorLog.txt");
        //                // var dateNow =DateTime.Now.ToString("yyyy-MM-dd-hh-ss");
        //                string FileName = $"StudentsErrorLog.txt";
        //                LOGPath="Logs/";
        //                string SavePath = Path.Combine(_hostEnvironment.WebRootPath ,LOGPath);
        //                if(!System.IO.Directory.Exists(SavePath))
        //                {
        //                    System.IO.Directory.CreateDirectory(SavePath); //Create directory if it doesn't exist
        //                }
        //                SavePath=SavePath+FileName;
        //                //using FileStream fileStream = new(SavePath, FileMode.Create);
        //                // LOGPath="/"+LOGPath+FileName;
        //                ////model.Image.CopyTo(fileStream);

        //                // Create a StreamWriter and write lines to the file
        //                using(StreamWriter writer = new StreamWriter(SavePath , append: false , encoding: Encoding.UTF8)) // append: true to add to an existing file
        //                {

        //                    foreach(var error in errorMessages)
        //                    {
        //                        writer.WriteLine($"Error message: "+error);
        //                    }

        //                }
        //                Response.Data=BaseURL+"/Logs/"+FileName;

        //            }

        //        }
        //        return Ok(Response);
        //    }
        //    catch(Exception ex)
        //    {
        //        Response.Result=false;
        //        string errorMsg = $"Exception: {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}";
        //        Response.Errors.Add(errorMsg);
        //        return BadRequest(Response);
        //    }
        //}

    }
}
