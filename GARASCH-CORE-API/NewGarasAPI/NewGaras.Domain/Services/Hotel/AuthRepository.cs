
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Hotel;
using NewGaras.Infrastructure.DTO.Hotel.DTOs.Auth;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.Hotel;
using NewGaras.Infrastructure.Repositories;
using NewGarasAPI.Helper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace NewGaras.Domain.Services.Hotel
{
    public class AuthRepository : BaseRepository<User, long>, IAuthRepository
    {
        static readonly string key = "SalesGarasPass";
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png", ".jpeg", ".svg" };
        private long _maxAllowedPosterSize = 15728640;
        private readonly JWT _jwt;
        private IHostingEnvironment _environment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        protected GarasTestContext _context;
        protected IMapper _mapper;

        // private readonly UserManager<ApplicationDbContext> _userManager;

        public AuthRepository(GarasTestContext context, IOptions<JWT> jwt, IHostingEnvironment Environment,
            IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(context)
        {
            _context = context;

            _jwt = jwt.Value;
            _environment = Environment;
            _httpContextAccessor = httpContextAccessor;
            // _userManager = userManager;
            _mapper = mapper;
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

        public async Task<BaseResponseWithData<AuthModel>> GetTokenAsync(LoginRequestModel model)
        {
            BaseResponseWithData<AuthModel> Response = new BaseResponseWithData<AuthModel>();
            Response.Data = new AuthModel();
            Response.Errors = new List<Error>();
            Response.Result = false;
            AuthModel authModel = new AuthModel();
            try
            {

                string PassEncrypted = Encrypt_Decrypt.Encrypt(model.Password.Trim(), key).ToLower().Trim();
                var _user = await _context.Users.Where(u => u.Email == model.UserName && u.Password == PassEncrypted).FirstOrDefaultAsync();

                if (_user is null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid Email or password";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (_user.Active != true)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "This Email was not active ";
                    Response.Errors.Add(error);
                    return Response;
                }
                var jwtSecurityToken = await CreateJwtToken(_user);
                //var rolesList = await _userManager.GetRolesAsync(_user);





                // authModel.ImagePath = user.ImagePath != null ? BaseURL + user.ImagePath : null;
                authModel.UserId = _user.Id;
                authModel.IsAuthenticated = true;
                authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authModel.Email = _user.Email;
                authModel.Phone = _user.Mobile;
                authModel.Username = _user.FirstName + " " + _user.LastName;
                authModel.ImagePath = _user.PhotoUrl;
                authModel.ExpiresOn = jwtSecurityToken.ValidTo; //jwtSecurityToken.ValidTo;
                //authModel.Roles = _user.UserRoleUsers.Select(r => new RoleModel { RoleId = r.Role?.Id ?? 0, RoleName = r.Role?.Description }).ToList();
                var Roles = _context.UserRoles.Where(x => x.UserId == _user.Id).Select(r => r.Role).ToList();

                var tempdata = new List<RoleModel>();
                for (int i = 0; Roles.Count > i; i++)
                {
                    var temp = new RoleModel();

                    temp.RoleId = Roles[i].Id;
                    temp.RoleName = Roles[i].Name;
                    tempdata.Add(temp);
                }
                authModel.Roles = tempdata;
                Response.Result = true;
                Response.Data = authModel;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        private async Task<JwtSecurityToken> CreateJwtToken(User user)
        {
            //var userClaims = await _userManager.GetClaimsAsync(user);
            //var roles = await _userManager.GetRolesAsync(user);
            //var roleClaims = new List<Claim>();

            //foreach (var role in roles)
            //    roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                //new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString())
            };
            //.Union(userClaims)
            //.Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        public async Task<BaseResponseWithData<AuthModel>> GetUserDataAsync(string ApplicationUserId)
        {
            BaseResponseWithData<AuthModel> Response = new BaseResponseWithData<AuthModel>();
            Response.Data = new AuthModel();
            Response.Errors = new List<Error>();
            Response.Result = false;

            try
            {

                if (string.IsNullOrWhiteSpace(ApplicationUserId))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid User";
                    Response.Errors.Add(error);
                    return Response;
                }
                long uId = 0;
                if (string.IsNullOrEmpty(ApplicationUserId) || !long.TryParse(ApplicationUserId, out uId))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid User";
                    Response.Errors.Add(error);
                    return Response;
                }

                var _user = await _context.Users.Where(u => u.Id == uId).Include("UserRoleUsers").Include("UserRoleUsers.Role").FirstOrDefaultAsync();
                AuthModel authModel = new AuthModel();

                if (_user is null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid User";
                    Response.Errors.Add(error);
                    return Response;
                }


                authModel.UserId = _user.Id;
                authModel.IsAuthenticated = true;
                authModel.Email = _user.Email;
                authModel.Phone = _user.Mobile;
                authModel.Username = _user.FirstName + " " + _user.LastName;
                // authModel.Roles = _user.UserRoleUsers.Select(r => new RoleModel { RoleId = r.Role?.Id ?? 0, RoleName = r.Role?.Description }).ToList();

                Response.Result = true;
                Response.Data = authModel;
                return Response;


            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error); return Response;
            }
        }

        public async Task<BaseResponseWithData<Role>> AddRoleName(Role model)
        {
            BaseResponseWithData<Role> Response = new BaseResponseWithData<Role>();
            Response.Data = new Role();
            Response.Errors = new List<Error>();
            Response.Result = false;
            // AuthModel authModel = new AuthModel();
            try
            {

                var _role = await _context.Roles.Where(u => u.Name == model.Name).FirstOrDefaultAsync();

                if (_role != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "This Rolename already exists";
                    Response.Errors.Add(error);
                    return Response;
                }
                await _context.Roles.AddAsync(model);
                _context.SaveChanges();
                Response.Result = true;
                Response.Data = model;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }



        public async Task<BaseResponseWithData<AuthModel>> Register(UserDto model)
        {
            BaseResponseWithData<AuthModel> Response = new BaseResponseWithData<AuthModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            List<string> errors = new List<string>();
            AuthModel authModel = new AuthModel();

            try
            {
                var _user = await _context.Users.Where(u => u.Email == model.Email).FirstOrDefaultAsync();

                if (_user != null)
                {
                    // errors.Add(" Email is already exist");

                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = " Email is already exist";
                    Response.Errors.Add(error);
                    return Response;
                }
                string ImagePath = null;

                if (model.Image != null)
                {

                    if (!_allowedExtenstions.Contains(Path.GetExtension(model.Image.FileName).ToLower()))
                    {
                        errors.Add("Logo Only .png , .jpg , .jpeg and .svg images are allowed!");
                    }
                    if (model.Image.Length > _maxAllowedPosterSize)
                    {
                        errors.Add("Max allowed size for Cover Image greater than 10MB!");
                    }

                    string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + model.Image.FileName.Trim().Replace(" ", "");
                    ImagePath = "Users/";
                    string SaveImagePath = Path.Combine(this._environment.WebRootPath, ImagePath);
                    if (!System.IO.Directory.Exists(SaveImagePath))
                    {
                        System.IO.Directory.CreateDirectory(SaveImagePath); //Create directory if it doesn't exist
                    }
                    SaveImagePath = SaveImagePath + FileName;
                    using FileStream fileStream = new(SaveImagePath, FileMode.Create);
                    ImagePath = "/" + ImagePath + FileName;
                    model.Image.CopyTo(fileStream);
                }
                model.PhotoURL = ImagePath;
                var _User = await _context.Users.AddAsync(_mapper.Map<User>(model));
                _context.SaveChanges();
                if (_User == null)
                {
                    // errors.Add(" error in  Register");

                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = " error in  Register";
                    Response.Errors.Add(error);
                    return Response;

                }
                var _User2 = _context.Users.Where(x => x.Email == model.Email).FirstOrDefault();

                var jwtSecurityToken = await CreateJwtToken(_User2);

                authModel.UserId = _User2.Id;
                authModel.IsAuthenticated = true;
                authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                authModel.Email = _User2.Email;
                authModel.Phone = _User2.Mobile;
                authModel.Username = _User2.FirstName + " " + _User2.LastName;
                authModel.ExpiresOn = jwtSecurityToken.ValidTo; //jwtSecurityToken.ValidTo;
                //authModel.Roles = _user.UserRoleUsers.Select(r => new RoleModel { RoleId = r.Role?.Id ?? 0, RoleName = r.Role?.Description }).ToList();
                var Roles = _context.UserRoles.Where(x => x.UserId == _User2.Id).Select(r => r.Role).ToList();

                var tempdata = new List<RoleModel>();
                for (int i = 0; Roles.Count > i; i++)
                {
                    var temp = new RoleModel();

                    temp.RoleId = Roles[i].Id;
                    temp.RoleName = Roles[i].Name;
                    tempdata.Add(temp);
                }
                authModel.Roles = tempdata;
                Response.Result = true;
                Response.Data = authModel;
                return Response;
            }

            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);                //errors.Add(" error in  Register");
                return Response;

            }
        }



        public async Task<BaseResponseWithData<AddRoleModel>> AddRoleforUser(AddRoleModel model)
        {
            BaseResponseWithData<AddRoleModel> Response = new BaseResponseWithData<AddRoleModel>();
            Response.Data = new AddRoleModel();
            Response.Errors = new List<Error>();
            Response.Result = false;
            // AuthModel authModel = new AuthModel();
            try
            {

                var _role = await _context.UserRoles.Where(u => u.UserId == model.UserId && u.RoleId == model.RoleId).FirstOrDefaultAsync();

                if (_role != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "This Rolename already exists";
                    Response.Errors.Add(error);
                    return Response;
                }
                await _context.UserRoles.AddAsync(_mapper.Map<UserRole>(model));
                _context.SaveChanges();
                Response.Result = true;
                Response.Data = model;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error); return Response;
            }
        }

        public async Task<BaseResponseWithData<List<RoleViewModel>>> GetRoleList()
        {
            BaseResponseWithData<List<RoleViewModel>> Response = new BaseResponseWithData<List<RoleViewModel>>();
            Response.Data = new();
            Response.Errors = new List<Error>();
            Response.Result = false;
            // AuthModel authModel = new AuthModel();
            try
            {

                var _role = _context.Roles.Select(sp => new RoleViewModel
                {
                    Id = sp.Id,
                    Name = sp.Name,
                    Description = sp.Description,
                    CreatedBy = sp.CreatedBy,
                    CreationDate = sp.CreationDate,
                    Active = sp.Active,
                    ModifiedBy = sp.ModifiedBy,
                    ModifiedDate = sp.ModifiedDate

                }).ToList();

                if (_role == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "There are no roles.";
                    Response.Errors.Add(error);

                    return Response;
                }
                Response.Result = true;
                Response.Data = _role;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        //public async Task<string> AddRoleAsync(AddRoleModel model)
        //{
        //    var user = await _userManager.FindByIdAsync(model.UserId);

        //    if (user is null )
        //        return "Invalid user ID or Role";

        //    if (await _userManager.IsInRoleAsync(user, model.Role.ToLower()))
        //        return "User already assigned to this role";

        //    var result = await _userManager.AddToRoleAsync(user, model.Role.ToLower());

        //    return result.Succeeded ? string.Empty : "Sonething went wrong";
        //}
        #region Old Data


        //[Authorize(Roles = "admin")]
        //public async Task<string> AddRoleAsync(AddRoleModel model)
        //{
        //    var user = await _userManager.FindByIdAsync(model.UserId);

        //    if (user is null || !await _roleManager.RoleExistsAsync(model.Role.ToLower()))
        //        return "Invalid user ID or Role";

        //    if (await _userManager.IsInRoleAsync(user, model.Role.ToLower()))
        //        return "User already assigned to this role";

        //    var result = await _userManager.AddToRoleAsync(user, model.Role.ToLower());

        //    return result.Succeeded ? string.Empty : "Sonething went wrong";
        //}


        //public async Task<BaseResponseWithData<AuthModel>> RegisterAsync(RegisterModel model)
        //{
        //    BaseResponseWithData<AuthModel> Response = new BaseResponseWithData<AuthModel>();
        //    Response.Data = new AuthModel();
        //    Response.Errors = new List<Error>();
        //    Response.Result = false;


        //    if (model.Email is not null && await _userManager.FindByEmailAsync(model.Email) is not null)
        //    {
        //        errors.Add("هذا البريد الالكترونى مسجل بالفعل ");
        //        return new AuthModel { Errors = errors };
        //    }
        //    model.Username = model.Username.Trim();
        //    if (model.Username is not null && await _userManager.FindByNameAsync(model.Username) is not null)
        //    {
        //        errors.Add("هذا المستخدم مسجل بالفعل");
        //        return new AuthModel { Errors = errors };
        //    }
        //    // validate user data itteration
        //    CheckUserIsExistModel modelCheckUser = new CheckUserIsExistModel();
        //    modelCheckUser.Email = model.Email;
        //    modelCheckUser.PhoneNumber = model.PhoneNumber;
        //    modelCheckUser.UserName = model.Username;
        //    modelCheckUser.NationalId = model.NationalId;

        //    var CheckUserResponse = await checkUserIsExistAsync(modelCheckUser);
        //    if (CheckUserResponse != null && CheckUserResponse.Count > 0)
        //    {
        //        errors = CheckUserResponse;
        //        return new AuthModel { Result = false, Errors = errors };
        //    }
        //    string ImagePath = null;
        //    if (model.Image != null)
        //    {
        //        if (!_allowedExtenstions.Contains(Path.GetExtension(model.Image.FileName).ToLower()))
        //        {
        //            errors.Add("Logo Only .png , .jpg , .jpeg and .svg images are allowed!");
        //        }
        //        if (model.Image.Length > _maxAllowedPosterSize)
        //        {
        //            errors.Add("Max allowed size for Cover Image greater than 10MB!");
        //        }

        //        string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + model.Image.FileName.Trim().Replace(" ", "");
        //        ImagePath = "Users/";
        //        string SaveImagePath = Path.Combine(this._environment.WebRootPath, ImagePath);
        //        if (!System.IO.Directory.Exists(SaveImagePath))
        //        {
        //            System.IO.Directory.CreateDirectory(SaveImagePath); //Create directory if it doesn't exist
        //        }
        //        SaveImagePath = SaveImagePath + FileName;
        //        using FileStream fileStream = new(SaveImagePath, FileMode.Create);
        //        ImagePath = "/" + ImagePath + FileName;
        //        model.Image.CopyTo(fileStream);
        //    }

        //    var user = new ApplicationUser
        //    {
        //        UserName = model.Username,
        //        Email = model.Email,
        //        FirstName = model.FirstName,
        //        MiddleName = model.MiddleName,
        //        LastName = model.LastName,
        //        NationalId = model.NationalId,
        //        AddressArea = model.AddressArea,
        //        Street = model.Street,
        //        BuildingNumber = model.BuildingNumber,
        //        FloorNumber = model.FloorNumber,
        //        BirthDate = model.BirthDate,
        //        Gender = model.Gender,
        //        ChurchName = model.ChurchName,
        //        ConfessionFatherName = model.ConfessionFatherName,
        //        ChurchService = model.ChurchService,
        //        PhoneNumber = model.PhoneNumber,
        //        LandLine = model.LandLine,
        //        ImagePath = ImagePath,
        //        Age = model.Age
        //    };

        //    var result = await _userManager.CreateAsync(user, model.Password);

        //    if (!result.Succeeded)
        //    {
        //        //var errors = string.Empty;
        //        errors.AddRange(result.Errors.Select(x => x.Description).ToList());
        //        // foreach (var error in result.Errors)
        //        //errors += $"{error.Description},";

        //        return new AuthModel { Result = false, Errors = errors };
        //    }

        //    if (model.IdentityQuestionList != null && model.IdentityQuestionList.Count() > 0)
        //    {
        //        foreach (var item in model.IdentityQuestionList)
        //        {
        //            var checkQuestionIsValid = await _unitOfWork.IdentityQuestions.GetByIdAsync(item.IdentityQuestionId);
        //            if (checkQuestionIsValid != null)
        //            {
        //                var _identityUserQuestion = new IdentityUserQuestion();
        //                _identityUserQuestion.ApplicationUserId = user.Id;
        //                _identityUserQuestion.IdentityQuestionId = item.IdentityQuestionId;
        //                _identityUserQuestion.Answer = HttpUtility.UrlDecode(item.Answer);
        //                _identityUserQuestion.Question = checkQuestionIsValid.Question;
        //                _identityUserQuestion.CreationDate = DateTime.Now;
        //                _identityUserQuestion.Active = true;
        //                await _unitOfWork.IdentityUserQuestions.AddAsync(_identityUserQuestion);
        //            }
        //        }
        //        _unitOfWork.Complete();
        //    }
        //    //if (model.RolesList != null && model.RolesList.Count() > 0)
        //    //{
        //    //    // check If Role Exist 
        //    //   await AddRoleAsync(user.Id,)
        //    //   // await _userManager.AddToRoleAsync(user, model.RolesList);
        //    //}
        //    //await _userManager.AddToRoleAsync(user, "Admin");
        //    var jwtSecurityToken = await CreateJwtToken(user);

        //    return new AuthModel
        //    {
        //        Result = true,
        //        UserId = user.Id,
        //        ImagePath = user.ImagePath != null ? BaseURL + user.ImagePath : null,
        //        Email = user.Email,
        //        ExpiresOn = jwtSecurityToken.ValidTo,
        //        IsAuthenticated = true,
        //        Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
        //        Username = user.UserName
        //    };
        //}


        //public async Task<List<string>> checkUserIsExistAsync(CheckUserIsExistModel model)
        //{
        //    List<string> errors = new List<string>();

        //    if (!string.IsNullOrWhiteSpace(model.Email))
        //    {
        //        var CheckUserEmail = await _userManager.FindByEmailAsync(model.Email);
        //        if (CheckUserEmail is not null && CheckUserEmail.Id != model.UserId)
        //            errors.Add("هذا البريد الالكترونى مسجل بالفعل ");
        //    }

        //    if (!string.IsNullOrWhiteSpace(model.UserName))
        //    {

        //        var CheckUserName = await _userManager.FindByNameAsync(model.UserName);
        //        if (CheckUserName is not null && CheckUserName.Id != model.UserId)
        //            errors.Add("هذا المستخدم مسجل بالفعل");
        //    }

        //    if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
        //    {

        //        var CheckUserPhoneIsExist = await _userManager.Users.Where(x => x.PhoneNumber == model.PhoneNumber && x.Id != model.UserId).FirstOrDefaultAsync();
        //        if (CheckUserPhoneIsExist is not null)
        //        {
        //            errors.Add("رقم الموبيل مسجل بالفعل");
        //        }
        //    }
        //    if (!string.IsNullOrWhiteSpace(model.NationalId))
        //    {

        //        var CheckUserNationalIdIsExist = await _userManager.Users.Where(x => x.NationalId == model.NationalId && x.Id != model.UserId).FirstOrDefaultAsync();
        //        if (CheckUserNationalIdIsExist is not null)
        //        {
        //            errors.Add("رقم البطاقة مسجل بالفعل");
        //        }
        //    }
        //    return errors;
        //}

        //public async Task<AuthModel> resetPasswordTokenAsync(ResetPasswordTokenModel model)
        //{
        //    var authModel = new AuthModel();
        //    authModel.Errors = new List<string>();
        //    if (!string.IsNullOrWhiteSpace(model.UserName))
        //    {
        //        var CheckUserIsExist = await _userManager.Users.Where(x => x.PhoneNumber == model.UserName ||
        //                                                                        x.Email == model.UserName ||
        //                                                                        x.UserName == model.UserName ||
        //                                                                        x.NationalId == model.UserName).FirstOrDefaultAsync();
        //        if (CheckUserIsExist is not null)
        //        {
        //            //if (!string.IsNullOrWhiteSpace(model.Answer1))
        //            //{
        //            //    string SearchedKey = SearchClientByName(model.Answer1);
        //            //    ClientsDBQuery = ClientsDBQuery.Where(a => SqlFunctions.PatIndex(SearchedKey, a.Name) > 0).AsQueryable();
        //            //}
        //            if ((model.Question1 != 0 && !string.IsNullOrWhiteSpace(model.Answer1)) || (model.Question2 != 0 && !string.IsNullOrWhiteSpace(model.Answer2)))
        //            {
        //                string Answer1 = null;
        //                if (!string.IsNullOrWhiteSpace(model.Answer1))
        //                {
        //                    Answer1 = HttpUtility.UrlDecode(model.Answer1);
        //                    Answer1 = Extension.SearchedKey(Answer1);
        //                }

        //                string Answer2 = null;
        //                if (!string.IsNullOrEmpty(model.Answer2))
        //                {
        //                    Answer2 = HttpUtility.UrlDecode(model.Answer2);
        //                    Answer2 = Extension.SearchedKey(Answer2);
        //                }

        //                // Check Question
        //                var CheckQuestionList = await _unitOfWork.IdentityUserQuestions.
        //                    FindAllAsync(
        //                    (i => i.ApplicationUserId == CheckUserIsExist.Id &&
        //                        ((i.IdentityQuestionId == model.Question1 && EF.Functions.Like(i.Answer, Answer1)) ||
        //                         //(i.IdentityQuestionId == model.Question2 && i.Answer == model.Answer2) ||
        //                         (i.IdentityQuestionId == model.Question2 && EF.Functions.Like(i.Answer, Answer2))

        //                                                                                          )));
        //                if (CheckQuestionList.FirstOrDefault() != null)
        //                {
        //                    // var User =await _userManager.FindByNameAsync(CheckUserIsExist.UserName);
        //                    authModel.Result = true;
        //                    authModel.UserId = CheckUserIsExist.Id;
        //                    authModel.Email = CheckUserIsExist.Email;
        //                    authModel.Username = CheckUserIsExist.UserName;
        //                    authModel.Token = await _userManager.GeneratePasswordResetTokenAsync(CheckUserIsExist);
        //                    return authModel;
        //                }
        //            }


        //        }
        //    }
        //    authModel.Errors.Add("هذا المستخدم غير موجود , يمكنك المحاولة فى وقت اخر او التواصل مع الادمن");
        //    return authModel;
        //}
        //public async Task<AuthModel> resetPasswordAsync(ResetPasswordModel model)
        //{
        //    var authModel = new AuthModel();
        //    authModel.Errors = new List<string>();
        //    authModel.Result = true;


        //    var _user = await _userManager.FindByIdAsync(model.UserId);
        //    if (_user == null)
        //    {
        //        authModel.Errors.Add("هذا المستخدم غير موجود");
        //        authModel.Result = false;
        //        return authModel;
        //    }
        //    if (string.Compare(model.NewPassword, model.ConfirmNewPassword) != 0)
        //    {
        //        authModel.Errors.Add("الباسورد و تاكيد الباسورد غير متطابقين");
        //        authModel.Result = false;
        //        return authModel;
        //    }
        //    var result = await _userManager.ResetPasswordAsync(_user, model.Token, model.NewPassword);
        //    if (!result.Succeeded)
        //    {
        //        authModel.Result = false;
        //        authModel.Errors.AddRange(result.Errors.Select(x => x.Description).ToList());
        //    }
        //    else
        //    {
        //        authModel.Result = true;
        //        authModel.UserId = _user.Id;
        //        authModel.Email = _user.Email;
        //        authModel.Username = _user.UserName;
        //    }
        //    return authModel;
        //}



        //public async Task<AuthModel> GetUserDataAsync(string ApplicationUserId)
        //{
        //    var authModel = new AuthModel();

        //    authModel.Errors = new List<string>();
        //    var user = await _userManager.FindByIdAsync(ApplicationUserId);

        //    if (string.IsNullOrWhiteSpace(ApplicationUserId) || user is null)
        //    {
        //        authModel.Errors.Add("هذا المستخدم غير موجود");
        //        return authModel;
        //    }

        //    var rolesList = await _userManager.GetRolesAsync(user);

        //    authModel.ImagePath = user.ImagePath != null ? BaseURL + user.ImagePath : null;
        //    authModel.UserId = user.Id;
        //    authModel.Phone = user.PhoneNumber;
        //    authModel.Result = true;
        //    authModel.IsAuthenticated = true;
        //    authModel.Email = user.Email;
        //    authModel.Username = user.UserName;
        //    authModel.Roles = rolesList.ToList();

        //    return authModel;
        //}


        //public async Task<BaseResponseWithData<UserProfileDataModel>> GetUserProfileDataAsync(string ApplicationUserId)
        //{
        //    var response = new BaseResponseWithData<UserProfileDataModel>();
        //    response.Errors = new List<string>();
        //    response.Result = true;

        //    var _data = new UserProfileDataModel();
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(ApplicationUserId))
        //        {
        //            response.Result = false;
        //            response.Errors.Add("هذا المستخدم غير موجود");
        //            return response;
        //        }
        //        var _user = await _unitOfWork.ApplicationUsers.FindAsync((x => x.Id == ApplicationUserId), new[] { "IdentityUserQuestion", "IdentityUserQuestion.IdentityQuestion" }); //await _userManager.FindByIdAsync(ApplicationUserId);

        //        if (_user is null)
        //        {
        //            response.Result = false;
        //            response.Errors.Add("هذا المستخدم غير موجود");
        //            return response;
        //        }

        //        _data = _mapper.Map<UserProfileDataModel>(_user);
        //        response.Data = _data;
        //        return response;

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Errors.Add("ex:" + ex.InnerException != null ? ex.InnerException?.Message : ex.Message);
        //        return response;
        //    }
        //}

        //public async Task<BaseResponse> UpdateProfileDataAsync(UserProfileDataModel model)
        //{
        //    var response = new BaseResponse();
        //    response.Errors = new List<string>();
        //    response.Result = true;

        //    try
        //    {
        //        if (model == null)
        //        {
        //            response.Result = false;
        //            response.Errors.Add("املاء البيانات من فضلك");
        //            return response;
        //        }
        //        if (string.IsNullOrWhiteSpace(model.Id))
        //        {
        //            response.Result = false;
        //            response.Errors.Add("هذا المستخدم غير موجود");
        //            return response;
        //        }
        //        var _user = await _userManager.FindByIdAsync(model.Id);

        //        if (_user is null)
        //        {
        //            response.Result = false;
        //            response.Errors.Add("هذا المستخدم غير موجود");
        //            return response;
        //        }

        //        CheckUserIsExistModel modelCheckUser = new CheckUserIsExistModel();
        //        modelCheckUser.UserId = model.Id;
        //        modelCheckUser.Email = model.Email;
        //        modelCheckUser.PhoneNumber = model.PhoneNumber;
        //        modelCheckUser.UserName = model.Username;
        //        modelCheckUser.NationalId = model.NationalId;

        //        var CheckUserResponse = await checkUserIsExistAsync(modelCheckUser);
        //        if (CheckUserResponse != null && CheckUserResponse.Count > 0)
        //        {
        //            response.Result = false;
        //            response.Errors = CheckUserResponse;
        //            return response;
        //        }
        //        string ImagePath = null;
        //        if (model.Image != null)
        //        {
        //            if (!_allowedExtenstions.Contains(Path.GetExtension(model.Image.FileName).ToLower()))
        //            {
        //                response.Errors.Add("Logo Only .png , .jpg , .jpeg and .svg images are allowed!");
        //            }
        //            if (model.Image.Length > _maxAllowedPosterSize)
        //            {
        //                response.Errors.Add("Max allowed size for Cover Image greater than 10MB!");
        //            }

        //            string FileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + "_" + model.Image.FileName.Trim().Replace(" ", "");
        //            ImagePath = "Users/";
        //            string SaveImagePath = Path.Combine(this._environment.WebRootPath, ImagePath);
        //            if (!System.IO.Directory.Exists(SaveImagePath))
        //            {
        //                System.IO.Directory.CreateDirectory(SaveImagePath); //Create directory if it doesn't exist
        //            }
        //            SaveImagePath = SaveImagePath + FileName;
        //            using FileStream fileStream = new(SaveImagePath, FileMode.Create);
        //            ImagePath = "/" + ImagePath + FileName;
        //            model.Image.CopyTo(fileStream);
        //        }
        //        _mapper.Map<UserProfileDataModel, ApplicationUser>(model, _user);
        //        //var _userUpdated = _mapper.Map<ApplicationUser>(model);
        //        if (!string.IsNullOrWhiteSpace(ImagePath))
        //        {
        //            _user.ImagePath = ImagePath;
        //        }
        //        _unitOfWork.ApplicationUsers.Update(_user);
        //        var userUpdateResponse = _unitOfWork.Complete();
        //        if (userUpdateResponse > 0)
        //        {
        //            if (!string.IsNullOrWhiteSpace(_user.ImagePath))
        //            {
        //                string OldImage = this._environment.WebRootPath + _user.ImagePath;
        //                if (System.IO.File.Exists(OldImage))
        //                {
        //                    System.IO.File.Delete(OldImage);
        //                }
        //            }

        //            // Update Identity Questions 
        //            if (model.IdentityQuestionList != null && model.IdentityQuestionList.Count > 0)
        //            {
        //                int Counter = 0;
        //                foreach (var item in model.IdentityQuestionList)
        //                {
        //                    Counter++;
        //                    // validate Id User identity Question
        //                    if (item.Id != 0)
        //                    {
        //                        var _validateUserIdentityQuestion = await _unitOfWork.IdentityUserQuestions.GetByIdAsync(item.Id);
        //                        if (_validateUserIdentityQuestion != null)
        //                        {
        //                            var checkQuestionIsValid = await _unitOfWork.IdentityQuestions.GetByIdAsync(item.IdentityQuestionId);
        //                            if (checkQuestionIsValid == null)
        //                            {
        //                                response.Result = false;
        //                                response.Errors.Add("لا يوجد هذا السؤال من الاسئله لتعديله  فى رقم " + Counter);
        //                            }
        //                            item.ApplicationUserId = model.Id;
        //                            //var _identityUserQuestion = _mapper.Map<IdentityUserQuestion>(item);
        //                            _mapper.Map<IdentityUserQuestionDTO, IdentityUserQuestion>(item, _validateUserIdentityQuestion);
        //                            _validateUserIdentityQuestion.Answer = HttpUtility.UrlDecode(_validateUserIdentityQuestion.Answer);
        //                            _validateUserIdentityQuestion.Question = checkQuestionIsValid.Question;
        //                            _validateUserIdentityQuestion.Active = true;
        //                            //_validateUserIdentityQuestion.CreationDate = DateTime.Now;
        //                            _unitOfWork.IdentityUserQuestions.Update(_validateUserIdentityQuestion);
        //                        }
        //                        else
        //                        {
        //                            response.Result = false;
        //                            response.Errors.Add("لا يوجد هذا السؤال من الاسئله لتعديله  فى رقم " + Counter);
        //                        }
        //                    }
        //                }
        //                if (response.Result == true)
        //                {
        //                    _unitOfWork.Complete();
        //                }
        //            }
        //        }
        //        return response;

        //    }
        //    catch (Exception ex)
        //    {
        //        response.Errors.Add("ex:" + ex.InnerException != null ? ex.InnerException?.Message : ex.Message);
        //        return response;
        //    }
        //}

        //public async Task<BaseResponse> DeleteUSerAsync(string ApplicationUserId)
        //{
        //    var response = new BaseResponse();
        //    response.Errors = new List<string>();
        //    response.Result = true;

        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(ApplicationUserId))
        //        {
        //            response.Result = false;
        //            response.Errors.Add("هذا المستخدم غير موجود");
        //            return response;
        //        }
        //        var _user = await _userManager.FindByIdAsync(ApplicationUserId);

        //        if (_user is null)
        //        {
        //            response.Result = false;
        //            response.Errors.Add("هذا المستخدم غير موجود");
        //            return response;
        //        }
        //        var Result = await _userManager.DeleteAsync(_user); // await _unitOfWork.ApplicationUsers.GetByIdAsync(id);

        //        if (!Result.Succeeded)
        //        {
        //            response.Result = false;
        //            response.Errors.Add("لا تستطيع حذف هذا المستخدم يرجى التواصل مع الادمن");
        //        }
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Errors.Add("ex:" + ex.InnerException != null ? ex.InnerException?.Message : ex.Message);
        //        return response;
        //    }
        //}



        //public async Task<bool> SendMail(MailData mailData)
        //{
        //    try
        //    {
        //        using (MimeMessage emailMessage = new MimeMessage())
        //        {
        //            MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
        //            emailMessage.From.Add(emailFrom);
        //            MailboxAddress emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToId);
        //            emailMessage.To.Add(emailTo);

        //            //emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", "cc@example.com"));
        //            //emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", "bcc@example.com"));

        //            emailMessage.Subject = mailData.EmailSubject;

        //            BodyBuilder emailBodyBuilder = new BodyBuilder();
        //            emailBodyBuilder.TextBody = mailData.EmailBody;

        //            emailMessage.Body = emailBodyBuilder.ToMessageBody();
        //            //this is the SmtpClient from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
        //            //using (SmtpClient mailClient = new SmtpClient())
        //            //{

        //            //    mailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        //            //    mailClient.Authenticate(_mailSettings.UserName, _mailSettings.Password);
        //            //    mailClient.Send(emailMessage);
        //            //    mailClient.Disconnect(true);
        //            //}


        //            #region Send Mail

        //            using var smtp = new SmtpClient();

        //            //if (_settings.UseSSL)
        //            //{
        //            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        //            //}
        //            //else if (_settings.UseStartTls)
        //            //{
        //            //    await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
        //            //}
        //            await smtp.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password, default);
        //            await smtp.SendAsync(emailMessage, default);
        //            await smtp.DisconnectAsync(true, default);

        //            #endregion



        //            //string smtpServer = "smtp.example.com";
        //            //int smtpPort = 587;
        //            //string smtpUsername = _mailSettings.UserName;
        //            //string smtpPassword = _mailSettings.Password;

        //            //var smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort)
        //            //{
        //            //    UseDefaultCredentials = false,
        //            //    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
        //            //    EnableSsl = true // or false, depending on your email service provider's requirements
        //            //};
        //            //smtpClient.Send(emailMessage);
        //            //smtpClient.Disconnect(true);
        //            // use the smtpClient to send emails

        //            // use the smtpClient to send emails
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Exception Details
        //        return false;
        //    }
        //}



        //public async Task<bool> SendAsync(MailData mailData, CancellationToken ct = default)
        //{
        //    try
        //    {
        //        // Initialize a new instance of the MimeKit.MimeMessage class
        //        var mail = new MimeMessage();

        //        #region Sender / Receiver
        //        // Sender
        //        mail.From.Add(new MailboxAddress(_settings.DisplayName, mailData.From ?? _settings.From));
        //        mail.Sender = new MailboxAddress(mailData.DisplayName ?? _settings.DisplayName, mailData.From ?? _settings.From);

        //        // Receiver
        //        foreach (string mailAddress in mailData.To)
        //            mail.To.Add(MailboxAddress.Parse(mailAddress));

        //        // Set Reply to if specified in mail data
        //        if (!string.IsNullOrEmpty(mailData.ReplyTo))
        //            mail.ReplyTo.Add(new MailboxAddress(mailData.ReplyToName, mailData.ReplyTo));

        //        // BCC
        //        // Check if a BCC was supplied in the request
        //        if (mailData.Bcc != null)
        //        {
        //            // Get only addresses where value is not null or with whitespace. x = value of address
        //            foreach (string mailAddress in mailData.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
        //                mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
        //        }

        //        // CC
        //        // Check if a CC address was supplied in the request
        //        if (mailData.Cc != null)
        //        {
        //            foreach (string mailAddress in mailData.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
        //                mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
        //        }
        //        #endregion

        //        #region Content

        //        // Add Content to Mime Message
        //        var body = new BodyBuilder();
        //        mail.Subject = mailData.Subject;
        //        body.HtmlBody = mailData.Body;
        //        mail.Body = body.ToMessageBody();

        //        #endregion

        //        #region Send Mail

        //        using var smtp = new SmtpClient();

        //        if (_settings.UseSSL)
        //        {
        //            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect, ct);
        //        }
        //        else if (_settings.UseStartTls)
        //        {
        //            await smtp.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, ct);
        //        }
        //        await smtp.AuthenticateAsync(_settings.UserName, _settings.Password, ct);
        //        await smtp.SendAsync(mail, ct);
        //        await smtp.DisconnectAsync(true, ct);

        //        #endregion

        //        return true;

        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}
        #endregion

    }
}
