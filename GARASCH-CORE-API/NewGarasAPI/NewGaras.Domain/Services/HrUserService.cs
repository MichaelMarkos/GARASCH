using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NewGarasAPI.Helper;
using System.Xml.Linq;
using NewGaras.Infrastructure.Models.HrUser;
using NewGaras.Infrastructure.Models;
using System.Linq.Expressions;
using Microsoft.Extensions.Configuration;
using NewGaras.Infrastructure.DBContext;
using NewGarasAPI.Models.User;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Models.HR;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using NewGaras.Infrastructure.Helper;
using NewGarasAPI.Models.HR;
using System.Web;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http.HttpResults;
using Azure;


namespace NewGaras.Domain.Services
{
    public class HrUserService : IHrUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private UnitOfWork unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly string key;
        private readonly ILogService _logService;
        private HearderVaidatorOutput validation;
        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public HrUserService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, ILogService logService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _host = host;
            key = "SalesGarasPass";
            _logService = logService;
        }

        public async Task<BaseResponseWithId<long>> CreateHrUser(HrUserDto NewHrUser, long UserId, string CompanyName)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                #region validation 
                string errorMessage = "";
                DateTime DoB = DateTime.Now;
                if (NewHrUser.DateOfBirth != null)
                {

                    if (!DateTime.TryParse(NewHrUser.DateOfBirth, out DoB))
                    {
                        response.Result = false;
                        //Error err = new Error();
                        //err.ErrorCode = "E-1";
                        errorMessage = errorMessage + "a valid Date of Birth - ";
                        //response.Errors.Add(err);
                        //return response;
                    }

                }
                if (string.IsNullOrWhiteSpace(NewHrUser.FirstName))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    errorMessage = errorMessage + "FirstName - ";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (string.IsNullOrWhiteSpace(NewHrUser.ARFirstName))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    errorMessage = errorMessage + "ArFirstName - ";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (string.IsNullOrWhiteSpace(NewHrUser.ARLastName))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    errorMessage = errorMessage + "ArlastName - ";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (string.IsNullOrWhiteSpace(NewHrUser.LastName))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    errorMessage = errorMessage + "lastName - ";
                    //response.Errors.Add(err);
                    //return response;
                }
                string MiddleName = "";
                if (!string.IsNullOrWhiteSpace(NewHrUser.MiddleName))
                {
                    MiddleName = NewHrUser.MiddleName;
                }
                string AMiddleName = "";
                if (!string.IsNullOrWhiteSpace(NewHrUser.ARMiddleName))
                {
                    AMiddleName = NewHrUser.ARMiddleName;
                }

                //if (string.IsNullOrWhiteSpace(NewHrUser.Email))
                //{
                //    response.Result = false;
                //    //Error err = new Error();
                //    //err.ErrorCode = "E-1";
                //    errorMessage = errorMessage + "Email - ";
                //    //response.Errors.Add(err);
                //    //return response;
                //}

                if (string.IsNullOrWhiteSpace(NewHrUser.Gender))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Gender :";
                    response.Errors.Add(err);
                    return response;
                }
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = errorMessage.Insert(0, "Please add( ");
                    string finalErrorMessage = errorMessage + $") before Submission";
                    Error error = new Error();
                    error.ErrorMSG = finalErrorMessage;
                    response.Errors.Add(error);
                }

                if (NewHrUser.NationalityId == null)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Nationality is required.";
                    response.Errors.Add(error);
                }

                if (NewHrUser.MaritalStatusId == null)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Marital status is required.";
                    response.Errors.Add(error);
                }

                if (NewHrUser.PlaceOfBirthID == null)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Place of birth is required.";
                    response.Errors.Add(error);
                }


                if (string.IsNullOrWhiteSpace(NewHrUser.AcademicYearName))
                {
                    Error error = new Error();
                    error.ErrorMSG = "Academic Year Name is required.";
                    response.Errors.Add(error);
                }

                if (NewHrUser.AcademicYearName?.Length > 250)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Academic Year Name must not exceed 250 characters.";
                    response.Errors.Add(error);
                }

                if (NewHrUser.AcademicYearDate == null)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Academic Year Date is required.";
                    response.Errors.Add(error);
                }

                if (!NewHrUser.IsALive && NewHrUser.DateOfDeath == null)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Date of death is required when the person is not alive.";
                    response.Errors.Add(error);
                }

                if (NewHrUser.IsALive && NewHrUser.DateOfDeath != null)
                {
                    Error error = new Error();
                    error.ErrorMSG = "Date of death must be empty when the person is alive.";
                    response.Errors.Add(error);
                }

                #endregion

                //var allHrUsers = await _unitOfWork.HrUsers.GetAllAsync();
                //var allHrUsersFullName = allHrUsers.Select(a => a.FirstName.ToLower() + a.MiddleName.ToLower() + a.LastName.ToLower()).ToList();
                //var allHrUsersFullAName = allHrUsers.Select(a => a.ArfirstName.ToLower() + a.ArmiddleName.ToLower() + a.ArlastName.ToLower()).ToList();

                var newUserName = "";
                if (!string.IsNullOrWhiteSpace(NewHrUser.FirstName) && !string.IsNullOrWhiteSpace(NewHrUser.LastName))
                {
                    newUserName = NewHrUser.FirstName.Replace(" ", "") + MiddleName.Replace(" ", "") + NewHrUser.LastName.Replace(" ", "");
                }

                var newUserAName = "";
                if (!string.IsNullOrWhiteSpace(NewHrUser.ARFirstName) && !string.IsNullOrWhiteSpace(NewHrUser.ARLastName))
                {
                    newUserAName = NewHrUser.ARFirstName.Replace(" ", "") + AMiddleName.Replace(" ", "") + NewHrUser.ARLastName.Replace(" ", "");
                }

                #region not repeating
                var notRepatingMessage = "";
                if (await _unitOfWork.HrUsers.AnyAsync(a => newUserName.ToLower() == a.FirstName.ToLower() + a.MiddleName.ToLower() + a.LastName.ToLower()))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    notRepatingMessage = notRepatingMessage + " This FullName is already Exists please, Choose another one -";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (await _unitOfWork.HrUsers.AnyAsync(a => newUserAName.ToLower() == a.ArfirstName.ToLower() + a.ArmiddleName.ToLower() + a.ArlastName.ToLower()))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    notRepatingMessage = notRepatingMessage + " This Arabic FullName is already Exists please, Choose another one -";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (NewHrUser.Email != null)
                {
                    if (await _unitOfWork.HrUsers.AnyAsync(a => a.Email.ToLower() == NewHrUser.Email.ToLower()))
                    {
                        response.Result = false;
                        //Error err = new Error();
                        //err.ErrorCode = "E-1";
                        notRepatingMessage = notRepatingMessage + " This Email is already Exists please, Enter a valid Email -";
                        //response.Errors.Add(err);
                        //return response;
                    }
                }
                if (NewHrUser.LandLine != null)
                {
                    if (await _unitOfWork.HrUsers.AnyAsync(a => a.LandLine.ToLower() == NewHrUser.LandLine.ToLower()))
                    {
                        response.Result = false;
                        //Error err = new Error();
                        //err.ErrorCode = "E-1";
                        notRepatingMessage = notRepatingMessage + " This Home is already Exists please, Enter a valid Home -";
                        //response.Errors.Add(err);
                        //return response;
                    }
                }



                if (!string.IsNullOrWhiteSpace(notRepatingMessage))
                {
                    Error error = new Error();
                    error.ErrorMSG = notRepatingMessage;
                    response.Errors.Add(error);
                }

                if (!string.IsNullOrWhiteSpace(errorMessage) || !string.IsNullOrWhiteSpace(notRepatingMessage)) return response;

                #endregion

                #region check in DB
                if (NewHrUser.JobTitleID != null)
                {
                    var branch = _unitOfWork.JobTitles.FindAll(a => a.Id == NewHrUser.JobTitleID).FirstOrDefault();
                    if (branch == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "No JobTilte with this ID :";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                #endregion


                IFormFile ImgInMemory = null;
                if (NewHrUser.Photo != null)
                {
                    ImgInMemory = NewHrUser.Photo;
                }
                var user = _mapper.Map<HrUser>(NewHrUser);
                user.Email = user.Email?.ToLower();
                if (NewHrUser.DateOfBirth != null) user.DateOfBirth = DoB;
                //--------------Trim() spaces from full name-------------------------
                user.FirstName = user.FirstName.Trim();
                user.MiddleName = MiddleName.Trim();
                user.LastName = user.LastName.Trim();
                //--------------Trim() spaces from arabic full name-------------------------
                user.ArfirstName = user.ArfirstName.Trim();
                user.ArmiddleName = AMiddleName.Trim();
                user.ArlastName = user.ArlastName.Trim();
                //-------------------------------------------------------------------
                user.ImgPath = null;
                user.CreationDate = DateTime.Now;
                user.Active = true;
                user.ModifiedById = UserId;
                user.CreatedById = UserId;
                user.Modified = DateTime.Now;
                user.IsDeleted = false;
                var HrUser = await _unitOfWork.HrUsers.AddAsync(user);
                _unitOfWork.Complete();
                response.ID = HrUser.Id;
                long lastUserId = HrUser.Id;

                if (ImgInMemory != null)
                {
                    var fileExtension = ImgInMemory.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompanyName}\\HrUser\\{HrUser.Id}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(ImgInMemory.FileName.Trim().Replace(" ", ""));
                    HrUser.ImgPath = Common.SaveFileIFF(virtualPath, ImgInMemory, FileName, fileExtension, _host);
                }
                _unitOfWork.Complete();



                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponse> AddAddressToHrUser(List<HrUserAddressDto> dtos)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (dtos.Count < 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Addresses List is Empty";
                    response.Errors.Add(err);
                    return response;
                }
                foreach (var address in dtos)
                {
                    var ad = _mapper.Map<HrUserAddress>(address);
                    ad.HrUserId = address.HrUserID;
                    ad.CreatedBy = validation.userID;
                    ad.ModifiedBy = validation.userID;
                    ad.CreationDate = DateTime.Now;
                    ad.ModifiedDate = DateTime.Now;
                    await _unitOfWork.HrUserAddresses.AddAsync(ad);
                }
                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        public async Task<BaseResponseWithId<long>> EditHrUserAddress(HrUserAddressDto dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                if (dto == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Adress Data is not sent";
                    response.Errors.Add(err);
                    return response;
                }
                if(dto.ID==0 || dto.ID == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorMSG = "Id Is Required!";
                    response.Errors.Add(err);
                    return response;
                }
                var address = await _unitOfWork.HrUserAddresses.GetByIdAsync((long)dto.ID);
                if (address == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.errorMSG = "addres you are trying to edit Is not found!";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.Active == false)
                {
                    _unitOfWork.HrUserAddresses.Delete(address);
                }
                else
                {
                    var ad = _mapper.Map<HrUserAddressDto, HrUserAddress>(dto, address);
                    ad.ModifiedBy = validation.userID;
                    ad.ModifiedDate = DateTime.Now;
                    _unitOfWork.HrUserAddresses.Update(ad);
                }

                _unitOfWork.Complete();
                response.ID = (long)dto.ID;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        public async Task<BaseResponseWithId<long>> EditHrUserSocialMedia(HrUserSocialMediaDto dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                if (dto == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Adress Data is not sent";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.ID == 0 || dto.ID == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorMSG = "Id Is Required!";
                    response.Errors.Add(err);
                    return response;
                }
                var link = await _unitOfWork.HrUserSocialMedias.GetByIdAsync((long)dto.ID);
                if (link == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.errorMSG = "Link you are trying to edit Is not found!";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.Active == false)
                {
                    _unitOfWork.HrUserSocialMedias.Delete(link);
                }
                else
                {
                    var Lin = _mapper.Map<HrUserSocialMediaDto, HrUserSocialMedium>(dto, link);
                    Lin.ModifiedBy = validation.userID;
                    Lin.ModifiedDate = DateTime.Now;
                    _unitOfWork.HrUserSocialMedias.Update(Lin);
                }

                _unitOfWork.Complete();
                response.ID = (long)dto.ID;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        public async Task<BaseResponseWithId<long>> EditHrUserMobile(HrUserMobileDto dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                if (dto == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Adress Data is not sent";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.ID == 0 || dto.ID == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorMSG = "Id Is Required!";
                    response.Errors.Add(err);
                    return response;
                }
                var Mobile = await _unitOfWork.HrUserMobiles.GetByIdAsync((long)dto.ID);
                if (Mobile == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.errorMSG = "Mobile you are trying to edit Is not found!";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.Active == false)
                {
                    _unitOfWork.HrUserMobiles.Delete(Mobile);
                }
                else
                {
                    var mob = _mapper.Map<HrUserMobileDto, HrUserMobile>(dto, Mobile);
                    mob.ModifiedBy = validation.userID;
                    mob.ModifiedDate = DateTime.Now;
                    _unitOfWork.HrUserMobiles.Update(mob);
                }

                _unitOfWork.Complete();
                response.ID = (long)dto.ID;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        public async Task<BaseResponseWithId<long>> EditHrUserLandLine(HrUserLandLineDto dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {

                if (dto == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Adress Data is not sent";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.ID == 0 || dto.ID == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorMSG = "Id Is Required!";
                    response.Errors.Add(err);
                    return response;
                }
                var LandLine = await _unitOfWork.HrUserLandLines.GetByIdAsync((long)dto.ID);
                if (LandLine == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.errorMSG = "LandLine you are trying to edit Is not found!";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.Active == false)
                {
                    _unitOfWork.HrUserLandLines.Delete(LandLine);
                }
                else
                {
                    var land = _mapper.Map<HrUserLandLineDto, HrUserLandLine>(dto, LandLine);
                    land.ModifiedBy = validation.userID;
                    land.ModifiedDate = DateTime.Now;
                    _unitOfWork.HrUserLandLines.Update(land);
                }

                _unitOfWork.Complete();
                response.ID = (long)dto.ID;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        public async Task<BaseResponse> AddAttachmentsToHrUser([FromForm] List<HrUserAttachmentDto> Attachments)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (Attachments.Count < 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Attachments List is Empty";
                    response.Errors.Add(err);
                    return response;
                }
                foreach (var Attachment in Attachments)
                {
                    if (Attachment.ID != null)
                    {
                        if (Attachment.Active == false)
                        {
                            var attach = await _unitOfWork.HrUserAttachments.GetByIdAsync((long)Attachment.ID);
                            if (attach != null)
                            {
                                _unitOfWork.HrUserAttachments.Delete(attach);
                            }
                        }
                    }
                    else
                    {
                        var Attach = _mapper.Map<HrUserAttachment>(Attachment);
                        Attach.HrUserId = Attachment.HrUserID;
                        Attach.CreatedBy = validation.userID;
                        Attach.ModifiedBy = validation.userID;
                        Attach.CreationDate = DateTime.Now;
                        Attach.ModifiedDate = DateTime.Now;
                        if (Attachment.AttachmentFile == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E101";
                            err.errorMSG = $"Attachments file is null at {Attachments.IndexOf(Attachment) + 1}";
                            response.Errors.Add(err);
                        }
                        var fileExtension = Attachment.AttachmentFile.FileName.Split('.').Last();
                        var virtualPath = $"Attachments\\{validation.CompanyName}\\HrUser\\{Attachment.HrUserID}\\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(Attachment.AttachmentFile.FileName.Trim().Replace(" ", ""));
                        Attach.AttachmentPath = Common.SaveFileIFF(virtualPath, Attachment.AttachmentFile, FileName, fileExtension, _host);

                        await _unitOfWork.HrUserAttachments.AddAsync(Attach);
                    }
                }
                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }

        public async Task<BaseResponse> AddContactsToHrUser(HrUserContactsDto dto)
        {
            var response = new BaseResponse()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (dto == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "Contact data is required";
                    response.Errors.Add(err);
                    return response;
                }
                if (dto.HrUserId <= 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorMSG = "User Id Is Required";
                    response.Errors.Add(err);
                    return response;
                }
                var user = await _unitOfWork.HrUsers.GetByIdAsync(dto.HrUserId);
                if (user == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.errorMSG = "User not found!";
                    response.Errors.Add(err);
                    return response;
                }
                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    user.Email = dto.Email;
                }
                if (dto.SocialMediaList.Count > 0)
                {
                    foreach (var social in dto.SocialMediaList)
                    {
                        var Media = _mapper.Map<HrUserSocialMedium>(social);
                        Media.HrUserId = dto.HrUserId;
                        Media.CreatedBy = validation.userID;
                        Media.ModifiedBy = validation.userID;
                        Media.CreationDate = DateTime.Now;
                        Media.ModifiedDate = DateTime.Now;
                        _unitOfWork.HrUserSocialMedias.Add(Media);
                        //_unitOfWork.Complete();
                    }
                }
                if (dto.HrUserMobiles.Count > 0)
                {
                    foreach (var mobile in dto.HrUserMobiles)
                    {
                        var mob = _mapper.Map<HrUserMobile>(mobile);
                        mob.HrUserId = dto.HrUserId;
                        mob.CreatedBy = validation.userID;
                        mob.ModifiedBy = validation.userID;
                        mob.CreationDate = DateTime.Now;
                        mob.ModifiedDate = DateTime.Now;
                        _unitOfWork.HrUserMobiles.Add(mob);
                        //_unitOfWork.Complete();
                    }
                }
                if (dto.HrUserLandlines.Count > 0)
                {
                    foreach (var landline in dto.HrUserLandlines)
                    {
                        var land = _mapper.Map<HrUserLandLine>(landline);
                        land.HrUserId = dto.HrUserId;
                        land.CreatedBy = validation.userID;
                        land.ModifiedBy = validation.userID;
                        land.CreationDate = DateTime.Now;
                        land.ModifiedDate = DateTime.Now;
                        _unitOfWork.HrUserLandLines.Add(land);
                        //_unitOfWork.Complete();
                    }
                }

                _unitOfWork.Complete();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }


        public async Task<BaseResponseWithDataAndHeader<List<HrUserCardDto>>> GetAll(int CurrentPage, int NumberOfItemsPerPage, string? searchKey,
            bool? active, int? DepId, int? jobTilteId, int? BranchId, bool? isUser, string? Email, string? mobile, bool? isDeleted, bool? ActiveUser)
        {
            var response = new BaseResponseWithDataAndHeader<List<HrUserCardDto>>();
            response.Result = true;
            response.Errors = new List<Error>();
            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string BaseURL = MyConfig.GetValue<string>("AppSettings:baseURL");

            ////string BaseURL = "https://garascore.garassolutions.com\\";
            //string BaseURL = "https://byapi.garassolutions.com\\";



            try
            {
                if (response.Result)
                {
                    //var data = await _unitOfWork.HrUsers.FindAllAsync((a => a.Active == true), new[] { "JobTitle" });
                    var data = _unitOfWork.HrUsers.FindAllQueryable(a => true, new[] { "JobTitle", "User" });

                    //if (!string.IsNullOrEmpty(searchKey))
                    //{
                    //    data = data.Where(a => (a.FirstName + a.MiddleName + a.LastName).Contains(searchKey.Replace(" ", "")) || (a.Email).Contains(searchKey) || (a.Mobile).Contains(searchKey));
                    //}
                    if (active != null)
                    {
                        data = data.Where(a => a.Active == active).AsQueryable();
                    }
                    //if (DepId != null)
                    //{
                    //    data = data.Where(a => a.DepartmentId == DepId).AsQueryable();
                    //}
                    if (jobTilteId != null)
                    {
                        data = data.Where(a => a.JobTitleId == jobTilteId).AsQueryable();
                    }
                    //if (BranchId != null)
                    //{
                    //    data = data.Where(a => a.BranchId == BranchId).AsQueryable();
                    //}
                    if (isUser != null)
                    {
                        data = data.Where(a => a.IsUser == isUser).AsQueryable();
                    }
                    if (isDeleted == true)
                    {
                        data = data.Where(a => a.IsDeleted == true).AsQueryable();
                    }
                    else
                    {
                        data = data.Where(a => a.IsDeleted != true).AsQueryable();
                    }

                    data = data.OrderBy(a => (a.FirstName + " " + (a.LastName != null ? (a.LastName + " ") : "") + a.LastName));

                    var hrUserData = PagedList<HrUser>.Create(data, CurrentPage, NumberOfItemsPerPage);
                    response.Data = hrUserData.Select(x => new HrUserCardDto
                    {
                        Id = x.Id,
                        FirstName = x.FirstName,
                        MiddleName = x.MiddleName,
                        LastName = x.LastName,
                        Email = x.Email,
                        //Mobile = x.Mobile,
                        JobTitle = x.JobTitle?.Name,
                        IsUser = x.IsUser,
                        ImgPath = x.ImgPath != null ? BaseURL + x.ImgPath : null
                    }).ToList();
                    PaginationHeader paginationHeader = new PaginationHeader();
                    paginationHeader.CurrentPage = CurrentPage;
                    paginationHeader.ItemsPerPage = NumberOfItemsPerPage;
                    paginationHeader.TotalPages = hrUserData.TotalPages;
                    paginationHeader.TotalItems = hrUserData.TotalCount;

                    response.PaginationHeader = paginationHeader;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithDataAndHeader<HrUserListDDL> GetHrUserListDDl(int CurrentPage, int NumberOfItemsPerPage, string? searchKey, long? DoctorSpecialtyId)
        {
            var response = new BaseResponseWithDataAndHeader<HrUserListDDL>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var userIDsOfTeam = new List<long>();
                if (DoctorSpecialtyId != null)
                {
                    var userTeamDB = _unitOfWork.UserTeams.FindAll(a => a.TeamId == DoctorSpecialtyId);
                    var usersIDs = userTeamDB.Select(a => a.UserId).ToList();

                    foreach (var id in usersIDs)
                    {
                        userIDsOfTeam.Add(id ?? 0);
                    }
                }

                var AllHrUsers = _unitOfWork.HrUsers.FindAllQueryable(a => true);

                if (searchKey != null)
                {
                    searchKey = HttpUtility.UrlDecode(searchKey);
                    AllHrUsers = AllHrUsers.Where(a => (a.FirstName + a.LastName).Contains(searchKey)).AsQueryable();
                }
                if (DoctorSpecialtyId != null)
                {
                    AllHrUsers = AllHrUsers.Where(a => userIDsOfTeam.Contains(a.UserId ?? 0)).AsQueryable();
                }
                var finalList = PagedList<HrUser>.Create(AllHrUsers, CurrentPage, NumberOfItemsPerPage);
                var ModelList = _mapper.Map<List<HrUserListDDLModel>>(finalList.ToList());
                response.Data = new HrUserListDDL()
                {
                    HrUserLists = ModelList
                };
                response.PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = finalList.CurrentPage,
                    TotalPages = finalList.TotalPages,
                    TotalItems = finalList.TotalCount,
                    ItemsPerPage = finalList.PageSize
                };
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<GetHrUserDto>> GetHrUser(long HrUserId)
        {
            var response = new BaseResponseWithData<GetHrUserDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            //#region user Auth
            //HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            //response.Errors = validation.errors;
            //response.Result = validation.result;
            //#endregion


            try
            {
                if (response.Result)
                {
                    if (HrUserId != 0)
                    {
                        //using (GarasTestContext _context = new GarasTestContext())
                        //    Helper _helper = new Helper();
                        //string conn=_helper.GetConnectonString("garastest");
                        //_context.Database.SetConnectionString(conn);
                        //unitOfWork = new UnitOfWork(_context);

                        var UserDtoData = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == HrUserId), new[] {"JobTitle", "Nationality", "MilitaryStatus", "MaritalStatus", "ChurchOfPresence", "BelongToChurch" });
                        if (UserDtoData == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "This HR User Id is not found ";
                            response.Errors.Add(err);
                            return response;
                        }
                        response.Data = _mapper.Map<GetHrUserDto>(UserDtoData);
                    }
                   
                    

                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<UserEmployeeResponse>> AddHrEmployeeToUserAsync(AddHrEmployeeToUserDTO InData, long userId, string key)
        {
            var response = new BaseResponseWithData<UserEmployeeResponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var allUsers = await _unitOfWork.Users.GetAllAsync();
                    var allUsersEmails = allUsers.Select(x => x.Email);
                    var HrUser = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == InData.HrUserId), new[] { "Department", "JobTitle", "Branch" });
                    if (HrUser != null)
                    {
                        if (HrUser.IsUser == true && HrUser.UserId != null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-20";
                            err.errorMSG = "This HR Employee is already a User";
                            response.Errors.Add(err);
                            return response;
                        }
                        NewGaras.Infrastructure.Entities.User usr = new NewGaras.Infrastructure.Entities.User();

                        if (InData.Email == null || InData.Email == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-20";
                            err.errorMSG = "Please, enter a valid Email";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (allUsersEmails.Contains(InData.Email) || allUsersEmails.Contains(InData.Email.ToLower()))
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-20";
                            err.errorMSG = "This Email is already Exists ,Please enter a valid Email";
                            response.Errors.Add(err);
                            return response;
                        }
                        usr.Email = InData.Email.ToLower();

                        if (InData.Password == null || InData.Password == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-20";
                            err.errorMSG = "Please, enter a valid password";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (InData.ConfirmPass == null || InData.ConfirmPass == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-20";
                            err.errorMSG = "Please, enter a valid Confirm password";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (InData.Password == InData.ConfirmPass)
                        {
                            usr.Password = Encrypt_Decrypt.Encrypt(InData.Password, key);
                        }
                        else
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-20";
                            err.errorMSG = "The password and confirm password did not match!";
                            response.Errors.Add(err);
                            return response;
                        }

                        #region add Data to user table
                        usr.FirstName = HrUser.FirstName;
                        usr.LastName = HrUser.LastName;
                        usr.MiddleName = HrUser.MiddleName;
                        //usr.Mobile = HrUser.Mobile;
                        usr.Active = HrUser.Active;
                        usr.CreationDate = DateTime.Now;
                        usr.CreatedBy = userId;
                        usr.ModifiedBy = userId;
                        usr.Modified = DateTime.Now;
                        usr.Gender = HrUser.Gender;
                        //usr.BranchId = HrUser.BranchId;
                        //usr.DepartmentId = HrUser.DepartmentId;
                        usr.JobTitleId = HrUser.JobTitleId;
                        usr.PhotoUrl = HrUser.ImgPath;
                        #endregion

                        var User = _unitOfWork.Users.Add(usr);
                        _unitOfWork.Complete();

                        HrUser.IsUser = true;
                        HrUser.UserId = User.Id;


                        _unitOfWork.Complete();
                        UserEmployeeResponse UserEmployeeResponse = new UserEmployeeResponse();
                        UserEmployeeResponse.Id = User.Id;
                        UserEmployeeResponse.UserName = usr.FirstName + " " + usr.LastName;
                        UserEmployeeResponse.UserEmail = HrUser.Email;
                        response.Data = UserEmployeeResponse;

                        //Common.CreateNotification(validation.userID, "Welcome , " + HrUser.FirstName, "Thank you for joining our system", "#", true, usr.Id, 0, _Context);

                    }
                    else
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "The ID of HrUser is Not Valid , no HrUser with this Id";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<UserDataResponse>> EditHrEmployee(EditHrEmployeeDto NewHrData, long userId, string CompanyName, string key)
        {

            var response = new BaseResponseWithData<UserDataResponse>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            UserDataResponse UserDataResponse = new UserDataResponse();


            var allHrUsers = await _unitOfWork.HrUsers.GetAllAsync(); // to be Edited

            var HrUser = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == NewHrData.HrUserId), new[] { "User" });

            #region validation 
            DateTime DoB = DateTime.Now;
            if (NewHrData.DateOfBirth != null)
            {
                if (!DateTime.TryParse(NewHrData.DateOfBirth, out DoB))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date of Birth :";
                    response.Errors.Add(err);
                    return response;
                }
                if (DoB.Year >= DateTime.Now.Year)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date :";
                    response.Errors.Add(err);
                    return response;
                }
            }
            if (NewHrData.FirstName == null || NewHrData.FirstName == "")
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid FrstName :";
                response.Errors.Add(err);
                return response;
            }
            if (NewHrData.ARLastName == null || NewHrData.ARLastName == "")
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid ArlastName :";
                response.Errors.Add(err);
                return response;
            }
            if (NewHrData.LastName == null || NewHrData.LastName == "")
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid lastName :";
                response.Errors.Add(err);
                return response;
            }
            string MiddleName = "";
            if (!string.IsNullOrWhiteSpace(NewHrData.MiddleName))
            {
                MiddleName = NewHrData.MiddleName;
            }
            //if (NewHrData.Mobile == null || NewHrData.Mobile == "")
            //{
            //    response.Result = false;
            //    Error err = new Error();
            //    err.ErrorCode = "E-1";
            //    err.errorMSG = "please, Enter a valid Mobile number :";
            //    response.Errors.Add(err);
            //    return response;
            //}
            if (NewHrData.Email == null || NewHrData.Email == "")
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Email:";
                response.Errors.Add(err);
                return response;
            }
            //if (string.IsNullOrWhiteSpace(NewHrData.Mobile))
            //{
            //    response.Result = false;
            //    Error err = new Error();
            //    err.ErrorCode = "E-1";
            //    err.errorMSG = "please, Enter a valid Mobile:";
            //    response.Errors.Add(err);
            //    return response;
            //}
            //if (NewHrData.Gender == null || NewHrData.Gender == "")
            //{
            //    response.Result = false;
            //    Error err = new Error();
            //    err.ErrorCode = "E-1";
            //    err.errorMSG = "please, Enter a valid Gender :";
            //    response.Errors.Add(err);
            //    return response;
            //}
            if (NewHrData.IsUser == true)
            {
                if (string.IsNullOrWhiteSpace(NewHrData.systemEmail))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid user Email :";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrWhiteSpace(NewHrData.password) && HrUser.UserId is null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid password :";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrWhiteSpace(NewHrData.confirmPassword) && HrUser.UserId is null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Confirm password :";
                    response.Errors.Add(err);
                    return response;
                }
                if (NewHrData.password != NewHrData.confirmPassword)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "The password and confirm password did not match:";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            #region check in DB
            //if (NewHrData.BranchID != null)
            //{
            //    var branch = _unitOfWork.Branches.FindAll(a => a.Id == NewHrData.BranchID).FirstOrDefault();
            //    if (branch == null)
            //    {
            //        response.Result = false;
            //        Error err = new Error();
            //        err.ErrorCode = "E-1";
            //        err.errorMSG = "No Branch with this ID :";
            //        response.Errors.Add(err);
            //        return response;
            //    }
            //}
            if (NewHrData.JobTitleID != null)
            {
                var branch = _unitOfWork.JobTitles.FindAll(a => a.Id == NewHrData.JobTitleID).FirstOrDefault();
                if (branch == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "No JobTilte with this ID :";
                    response.Errors.Add(err);
                    return response;
                }
            }
            //if (NewHrData.DepartmentID != null)
            //{
            //    var branch = _unitOfWork.Departments.FindAll(a => a.Id == NewHrData.DepartmentID).FirstOrDefault();
            //    if (branch == null)
            //    {
            //        response.Result = false;
            //        Error err = new Error();
            //        err.ErrorCode = "E-1";
            //        err.errorMSG = "No Department with this ID :";
            //        response.Errors.Add(err);
            //        return response;
            //    }
            //}
            //if (NewHrData.TeamId != null)
            //{
            //    var branch = _unitOfWork.Teams.FindAll(a => a.Id == NewHrData.TeamId).FirstOrDefault();
            //    if (branch == null)
            //    {
            //        response.Result = false;
            //        Error err = new Error();
            //        err.ErrorCode = "E-1";
            //        err.errorMSG = "No Team with this ID :";
            //        response.Errors.Add(err);
            //        return response;
            //    }
            //}
            #endregion

            var allHrUsersFullName = allHrUsers.Select(a => a.FirstName.ToLower() + a.MiddleName?.ToLower() ?? "" + a.LastName.ToLower()).ToList();

            var newUserName = NewHrData.FirstName.Replace(" ", "").ToLower() + MiddleName.Replace(" ", "").ToLower() + NewHrData.LastName.Replace(" ", "").ToLower();

            #region not repeating
            var FulLNameHrUserFromDB = HrUser.FirstName.Replace(" ", "") + HrUser.MiddleName.Replace(" ", "") + HrUser.LastName.Replace(" ", "");
            if (FulLNameHrUserFromDB.ToLower() != newUserName)
            {
                if (allHrUsersFullName.Contains(newUserName.ToLower()))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This FullName is already Exists please, Enter a valid FullName :";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (HrUser.Email != NewHrData.Email)
            {
                var allHrUsersEmails = allHrUsers.Select(a => a.Email);
                if (allHrUsersEmails.Contains(NewHrData.Email) || allHrUsersEmails.Contains(NewHrData.Email.ToLower()))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This Email is already Exists please, Enter a valid Email :";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (HrUser.LandLine != NewHrData.LandLine && NewHrData.LandLine != null)
            {
                var allHrUsersLandLines = allHrUsers.Where(a => a.LandLine != null).Select(a => a.LandLine);
                if (allHrUsersLandLines.Contains(NewHrData.LandLine))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This Home is already Exists please, Enter a valid Home Number :";
                    response.Errors.Add(err);
                    return response;
                }
            }

            //if (HrUser.Mobile != NewHrData.Mobile && NewHrData.Mobile != "0")
            //{
            //    var allHrUsersMobileNumbers = allHrUsers.Select(a => a.Mobile);
            //    if (allHrUsersMobileNumbers.Contains(NewHrData.Mobile))
            //    {
            //        response.Result = false;
            //        Error err = new Error();
            //        err.ErrorCode = "E-1";
            //        err.errorMSG = "This Mobile is already Exists please, Enter a valid Mobile :";
            //        response.Errors.Add(err);
            //        return response;
            //    }
            //}

            #endregion


            try
            {
                if (response.Result)
                {

                    IFormFile ImgInMemory = null;
                    var savedpath = "";
                    if (NewHrData.Photo != null)
                    {
                        ImgInMemory = NewHrData.Photo;
                    }

                    if (HrUser != null)
                    {
                        #region validation 
                        if (NewHrData.FirstName == null || NewHrData.FirstName == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "please, Enter a valid FrstName :";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (NewHrData.ARLastName == null || NewHrData.ARLastName == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "please, Enter a valid ArlastName :";
                            response.Errors.Add(err);
                            return response;
                        }
                        if (NewHrData.LastName == null || NewHrData.LastName == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "please, Enter a valid lastName :";
                            response.Errors.Add(err);
                            return response;
                        }

                        //if (NewHrData.Mobile == null || NewHrData.Mobile == "")
                        //{
                        //    response.Result = false;
                        //    Error err = new Error();
                        //    err.ErrorCode = "E-1";
                        //    err.errorMSG = "please, Enter a valid Mobile number :";
                        //    response.Errors.Add(err);
                        //    return response;
                        //}
                        if (NewHrData.Email == null || NewHrData.Email == "")
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "please, Enter a valid Email:";
                            response.Errors.Add(err);
                            return response;
                        }
                        //if (string.IsNullOrWhiteSpace(NewHrData.Mobile))
                        //{
                        //    response.Result = false;
                        //    Error err = new Error();
                        //    err.ErrorCode = "E-1";
                        //    err.errorMSG = "please, Enter a valid Mobile:";
                        //    response.Errors.Add(err);
                        //    return response;
                        //}
                        //if (NewHrData.Gender == null || NewHrData.Gender == "")
                        //{
                        //    response.Result = false;
                        //    Error err = new Error();
                        //    err.ErrorCode = "E-1";
                        //    err.errorMSG = "please, Enter a valid Gender :";
                        //    response.Errors.Add(err);
                        //    return response;
                        //}
                        if (NewHrData.IsUser == true)
                        {
                            if (string.IsNullOrWhiteSpace(NewHrData.systemEmail))
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E-1";
                                err.errorMSG = "please, Enter a valid user Email :";
                                response.Errors.Add(err);
                                return response;
                            }
                            var userOldData = _unitOfWork.Users.Find((a => a.Email == NewHrData.systemEmail));
                            if (userOldData != null && userOldData.Id != HrUser.UserId)
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E-1";
                                err.errorMSG = "The System Email is already Exists, please Enter a valid system Email:";
                                response.Errors.Add(err);
                                return response;
                            }
                            if (string.IsNullOrWhiteSpace(NewHrData.password) && HrUser.UserId is null)
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E-1";
                                err.errorMSG = "please, Enter a valid password :";
                                response.Errors.Add(err);
                                return response;
                            }
                            if (string.IsNullOrWhiteSpace(NewHrData.confirmPassword) && HrUser.UserId is null)
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E-1";
                                err.errorMSG = "please, Enter a valid Confirm password :";
                                response.Errors.Add(err);
                                return response;
                            }
                        }

                        #endregion

                        DateTime ForCheckOnly = new DateTime(1900, 01, 01);

                        HrUser.FirstName = NewHrData.FirstName;
                        if (NewHrData.Active != null)
                        {
                            //HrUser.Active = NewHrData.Active ?? false;
                        }
                        HrUser.ModifiedById = userId;
                        HrUser.Modified = DateTime.Now;
                        HrUser.ArlastName = NewHrData.ARLastName;
                        HrUser.LastName = NewHrData.LastName;
                        HrUser.MiddleName = MiddleName;
                        //HrUser.IsUser = NewHrData
                        //HrUser.Mobile = NewHrData.Mobile;
                        HrUser.Email = NewHrData.Email.ToLower();
                        HrUser.Gender = NewHrData.Gender;
                        //------------------------------CanBeNull-------------------
                        HrUser.ArfirstName = NewHrData.ARFirstName;
                        HrUser.ArmiddleName = NewHrData.ARMiddleName;
                        //HrUser.BranchId = NewHrData.BranchID;
                        //HrUser.DepartmentId = NewHrData.DepartmentID;
                        //HrUser.TeamId = NewHrData.TeamId;
                        HrUser.JobTitleId = NewHrData.JobTitleID;
                        HrUser.IsUser = NewHrData.IsUser;
                        HrUser.LandLine = NewHrData.LandLine;
                        HrUser.IsDeleted = NewHrData.IsDeleted;
                        if (NewHrData.IsDeleted == true)
                        {
                            HrUser.Active = false;
                            if (HrUser.User != null)
                            {
                                HrUser.User.Active = false;
                            }
                        }
                        if (HrUser.DateOfBirth != DoB && DoB > ForCheckOnly)
                        {
                            if (NewHrData.DateOfBirth != null) HrUser.DateOfBirth = DoB;
                        }
                        if (NewHrData.Photo != null)
                        {
                            if (HrUser.ImgPath != null && HrUser.ImgPath != "")
                            {
                                string FilePath = Path.Combine(_host.WebRootPath, HrUser.ImgPath);
                                if (System.IO.File.Exists(FilePath))
                                {
                                    System.IO.File.Delete(FilePath);
                                }
                            }
                            //System.IO.File.Delete(HrUser.ImgPath);
                            var fileExtension = NewHrData.Photo.FileName.Split('.').Last();
                            var virtualPath = $"Attachments\\{CompanyName}\\HrUser\\{HrUser.Id}\\";
                            var FileName = System.IO.Path.GetFileNameWithoutExtension(NewHrData.Photo.FileName.Trim().Replace(" ", ""));
                            HrUser.ImgPath = Common.SaveFileIFF(virtualPath, NewHrData.Photo, FileName, fileExtension, _host);
                        }
                        //if (NewHrData.TeamId != null)
                        //{
                        //    var alreadyAtTisTeam = _unitOfWork.UserTeams.FindAll(a => a.HrUserId == HrUser.Id).FirstOrDefault();

                        //    //if (HrUser.TeamId != null)
                        //    //{
                        //    //    if (alreadyAtTisTeam == null)
                        //    //    {
                        //    //        var newTeamUser = new UserTeam();
                        //    //        newTeamUser.TeamId = NewHrData.TeamId ?? 0;
                        //    //        newTeamUser.HrUserId = HrUser.Id;
                        //    //        newTeamUser.CreatedBy = userId;
                        //    //        newTeamUser.CreatedDate = DateTime.Now;
                        //    //        newTeamUser.ModifiedBy = userId;
                        //    //        newTeamUser.ModifiedDate = DateTime.Now;

                        //    //        var teamUser = await _unitOfWork.UserTeams.AddAsync(newTeamUser);
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        _unitOfWork.UserTeams.Delete(alreadyAtTisTeam);

                        //    //        var newTeamUser = new UserTeam();
                        //    //        newTeamUser.TeamId = NewHrData.TeamId ?? 0;
                        //    //        newTeamUser.HrUserId = HrUser.Id;
                        //    //        newTeamUser.CreatedBy = userId;
                        //    //        newTeamUser.CreatedDate = DateTime.Now;
                        //    //        newTeamUser.ModifiedBy = userId;
                        //    //        newTeamUser.ModifiedDate = DateTime.Now;

                        //    //        var teamUser = await _unitOfWork.UserTeams.AddAsync(newTeamUser);
                        //    //    }
                        //    //    _unitOfWork.Complete();
                        //    //}
                        //}

                        if (HrUser.User != null)
                        {

                            if (!string.IsNullOrWhiteSpace(NewHrData.systemEmail))
                            {

                                HrUser.User.Email = NewHrData.systemEmail;
                            }
                            if (!string.IsNullOrWhiteSpace(NewHrData.password) && !string.IsNullOrWhiteSpace(NewHrData.confirmPassword))
                            {

                                if (NewHrData.password == NewHrData.confirmPassword)
                                {
                                    HrUser.User.Password = Encrypt_Decrypt.Encrypt(NewHrData.password, key); ;
                                }

                            }
                            if (NewHrData.Active != null)
                            {
                                //HrUser.User.Active = NewHrData.Active ?? false;
                                if (!HrUser.IsUser)
                                {
                                    HrUser.User.Active = false;
                                }
                            }
                            if (NewHrData.Photo != null)
                            {
                                HrUser.User.PhotoUrl = HrUser.ImgPath;
                            }
                            if (!string.IsNullOrWhiteSpace(NewHrData.FirstName))
                            {
                                HrUser.User.FirstName = NewHrData.FirstName;
                            }
                            if (!string.IsNullOrWhiteSpace(NewHrData.MiddleName))
                            {
                                HrUser.User.MiddleName = NewHrData.MiddleName;
                            }
                            if (!string.IsNullOrWhiteSpace(NewHrData.LastName))
                            {
                                HrUser.User.LastName = NewHrData.LastName;
                            }


                            if (!string.IsNullOrWhiteSpace(NewHrData.Gender))
                            {
                                HrUser.User.Gender = NewHrData.Gender;
                            }
                            HrUser.User.JobTitleId = NewHrData.JobTitleID;
                            HrUser.User.ModifiedBy = userId;
                            HrUser.User.Modified = DateTime.Now;

                            if (NewHrData.IsDeleted == true)
                            {
                                HrUser.User.Active = false;
                            }
                        }
                        else
                        {
                            if (NewHrData.IsUser == true)
                            {
                                if (NewHrData.systemEmail != null || NewHrData.systemEmail != "" || NewHrData.password != null || NewHrData.password != "")
                                {
                                    NewGaras.Infrastructure.Entities.User usr = new NewGaras.Infrastructure.Entities.User();

                                    #region add Data to user table
                                    usr.Password = Encrypt_Decrypt.Encrypt(NewHrData.password, key);
                                    usr.Email = NewHrData.systemEmail;
                                    usr.FirstName = HrUser.FirstName;
                                    usr.LastName = HrUser.LastName;
                                    usr.MiddleName = HrUser.MiddleName;
                                    //usr.Mobile = HrUser.Mobile;
                                    usr.Active = HrUser.Active;
                                    usr.CreationDate = DateTime.Now;
                                    usr.CreatedBy = userId;
                                    usr.ModifiedBy = userId;
                                    usr.Modified = DateTime.Now;
                                    usr.Gender = HrUser.Gender;
                                    //usr.BranchId = HrUser.BranchId;
                                    //usr.DepartmentId = HrUser.DepartmentId;
                                    usr.JobTitleId = HrUser.JobTitleId;
                                    usr.PhotoUrl = HrUser.ImgPath;
                                    #endregion

                                    var User = _unitOfWork.Users.Add(usr);
                                    _unitOfWork.Complete();

                                    HrUser.UserId = User.Id;

                                    //Common.CreateNotification(validation.userID, "Welcome , " + HrUser.FirstName, "Thank you for joining our system", "#", true, usr.Id, 0, _Context);

                                    UserDataResponse.UserSystemId = User.Id;
                                    UserDataResponse.UserSystemName = HrUser.FirstName;
                                    UserDataResponse.UserSystemEmail = NewHrData.systemEmail;
                                }
                            }
                        }
                        _unitOfWork.Complete();

                    }
                }
                UserDataResponse.HRUserId = NewHrData.HrUserId;
                response.Data = UserDataResponse;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + (ex.InnerException != null ? ex.InnerException?.Message : ex.Message);
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<GetHrTeamUsersDto>> GetHrTeamUsers(long TeamId)
        {
            var response = new BaseResponseWithData<List<GetHrTeamUsersDto>>();
            response.Result = true;
            response.Errors = new List<Error>();
            //var users = _unitOfWork.HrUsers.FindAll(a => a.TeamId == TeamId);
            //var teamUsers = _mapper.Map<List<GetHrTeamUsersDto>>(users);
            //response.Data = teamUsers;
            return response;
        }

        public async Task<BaseResponseWithData<List<HrUserJobTitleDto>>> GetAllUsersWithJobTitle(int? JobTitleId = null)
        {
            var response = new BaseResponseWithData<List<HrUserJobTitleDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (response.Result)
                {
                    var HrUsersWithJobTitleId = await _unitOfWork.HrUsers.FindAllAsync(a => a.JobTitleId != JobTitleId);
                    response.Data = HrUsersWithJobTitleId.Select(x => new HrUserJobTitleDto
                    {
                        Id = x.Id,
                        JobTitleId = x.JobTitleId

                    }).ToList();
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<List<HrUsersWithJobTitleNameImage>>> GetHrUsersWithJobTitleNameImage(int JobTitleId)
        {
            var response = new BaseResponseWithData<List<HrUsersWithJobTitleNameImage>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var HrUsersWithJobTitleId = await _unitOfWork.HrUsers.FindAllAsync(a => a.JobTitleId == JobTitleId);
                response.Data = HrUsersWithJobTitleId.Select(x => new HrUsersWithJobTitleNameImage
                {
                    Name = x.FirstName + " " + x.LastName,
                    ImgPath = x.ImgPath

                }).ToList();
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithId<long>> RetriveDeletedUser(long id)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var allUsers = await _unitOfWork.HrUsers.GetAllAsync();
                    //var InData = allUsers.Where(a => a.Id == id).FirstOrDefault();
                    var allUsersEmails = allUsers.Select(x => x.Email);
                    var allHrUsersFullName = allUsers.Where(a => a.Id != id).Select(a => a.FirstName.ToLower() + a.MiddleName.ToLower() + a.LastName.ToLower()).ToList();

                    var user = await _unitOfWork.HrUsers.FindAsync((HU => HU.Id == id), new[] { "Department", "JobTitle", "Branch" });
                    var newUserName = user.FirstName.Replace(" ", "") + user.MiddleName.Replace(" ", "") + user.LastName.Replace(" ", "");

                    if (user != null)
                    {
                        if (user.IsDeleted == false || user.IsDeleted == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "This User is already Not deleted";
                            response.Errors.Add(err);
                            return response;
                        }
                        #region not repeating
                        if (allHrUsersFullName.Contains(newUserName.ToLower()))
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "This FullName is already Exists .";
                            response.Errors.Add(err);
                            return response;
                        }
                        var allHrUsersEmails = allUsers.Where(a => a.Id != id).Select(a => a.Email);
                        if (allHrUsersEmails.Contains(user.Email) || allHrUsersEmails.Contains(user.Email.ToLower()))
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "This Email is already Exists.";
                            response.Errors.Add(err);
                            return response;
                        }
                        var allHrUsersLandLines = allUsers.Where(a => a.LandLine != null && a.Id != id).Select(a => a.LandLine);
                        if (allHrUsersLandLines.Contains(user.LandLine))
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = "This Home is already Exists.";
                            response.Errors.Add(err);
                            return response;
                        }
                        //if (user.Mobile != "0")
                        //{
                        //    var allHrUsersMobileNumbers = allUsers.Where(a => a.Id != id).Select(a => a.Mobile);
                        //    if (allHrUsersMobileNumbers.Contains(user.Mobile))
                        //    {
                        //        response.Result = false;
                        //        Error err = new Error();
                        //        err.ErrorCode = "E-1";
                        //        err.errorMSG = "This Mobile is already Exists .";
                        //        response.Errors.Add(err);
                        //        return response;
                        //    }

                        //}
                        #endregion
                        user.IsDeleted = false;
                        _unitOfWork.Complete();


                        //Common.CreateNotification(validation.userID, "Welcome , " + HrUser.FirstName, "Thank you for joining our system", "#", true, usr.Id, 0, _Context);

                    }
                    else
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "The ID of HrUser is Not Valid , no HrUser with this Id";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetAbsenceHistoryModel> GetAbsenceHistoryForUser(GetAbsenceHistoryRequest request)
        {
            BaseResponseWithData<GetAbsenceHistoryModel> Response = new BaseResponseWithData<GetAbsenceHistoryModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new GetAbsenceHistoryModel();

            if (request.HrUserId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "HrUser Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var HrUser = _unitOfWork.HrUsers.GetById(request.HrUserId);
            if (HrUser == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Hr User is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (request.AbsenceTypeId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Absence Type Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var Absence = _unitOfWork.ContractLeaveSetting.GetById(request.AbsenceTypeId);
            if (Absence == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Absence Type is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var Attendances = _unitOfWork.Attendances.FindAll(a => a.AbsenceTypeId == request.AbsenceTypeId && a.HrUserId == request.HrUserId && a.IsApprovedAbsence == true, includes: new[] { "AbsenceType", "HrUser", "ApprovedByUser" }).ToList();

            var Past = Attendances.Where(a => DateOnly.FromDateTime(DateTime.Now) > a.AttendanceDate).Select(a => new AbsenceHistory { Id = a.Id, Date = a.AttendanceDate, HrUserId = (long)a.HrUserId, AbsenceCause = a.AbsenceCause, AbsenceName = a.AbsenceType.HolidayName, HrUserName = a.HrUser.FirstName + " " + a.HrUser.LastName, ApprovedAbsenceById = (long)a.ApprovedByUserId, ApprovedAbsenceName = a.ApprovedByUser.FirstName + " " + a.ApprovedByUser.LastName, ApprovedCause = a.AbsenceRejectCause, ApprovedDate = a.ModificationDate }).ToList();

            var planned = Attendances.Where(a => DateOnly.FromDateTime(DateTime.Now) <= a.AttendanceDate).Select(a => new AbsenceHistory { Id = a.Id, Date = a.AttendanceDate, HrUserId = (long)a.HrUserId, AbsenceCause = a.AbsenceCause, AbsenceName = a.AbsenceType.HolidayName, HrUserName = a.HrUser.FirstName + " " + a.HrUser.LastName, ApprovedAbsenceById = (long)a.ApprovedByUserId, ApprovedAbsenceName = a.ApprovedByUser.FirstName + " " + a.ApprovedByUser.LastName, ApprovedCause = a.AbsenceRejectCause, ApprovedDate = a.ModificationDate }).ToList();

            Response.Data.PastAbsence = Past;
            Response.Data.PlannedAbsencee = planned;

            return Response;
        }

        public BaseResponseWithData<GetHrUserAbsneceRequestList> GetHrUserAbsneceRequest(long HrUserId, int AbsenceTypeId)
        {
            BaseResponseWithData<GetHrUserAbsneceRequestList> Response = new BaseResponseWithData<GetHrUserAbsneceRequestList>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = new GetHrUserAbsneceRequestList();

            if (HrUserId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "HrUser Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var HrUser = _unitOfWork.HrUsers.GetById(HrUserId);
            if (HrUser == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Hr User is not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (AbsenceTypeId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Absence Type Id Is Required";
                Response.Errors.Add(error);
                return Response;
            }
            var Absence = _unitOfWork.ContractLeaveSetting.GetById(AbsenceTypeId);
            if (Absence == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Absence Type is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var LeaveEmployee = _unitOfWork.ContractLeaveEmployees.FindAll(a => a.HrUserId == HrUserId && a.ContractLeaveSettingId == AbsenceTypeId, orderBy: a => a.CreationDate, take: null, skip: null).LastOrDefault();
            if (LeaveEmployee == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = "this user doesn't have this Absence Type";
                Response.Errors.Add(error);
                return Response;
            }
            Response.Data.AbsenceTypeName = Absence.HolidayName;
            Response.Data.TotalBalance = LeaveEmployee.Balance ?? 0;
            Response.Data.Requested = LeaveEmployee.Used ?? 0;
            Response.Data.Remain = LeaveEmployee.Remain ?? 0;

            var leaveRequests = _unitOfWork.LeaveRequests.FindAll(a => a.HrUserId == HrUserId && a.VacationTypeId == AbsenceTypeId, includes: new[] { "FirstApprovedByNavigation", "SecondApprovedByNavigation" }).OrderBy(a => a.From).ToList();
            var list = leaveRequests.Select(a => new AbsenceRequest()
            {
                From = a.From.ToShortDateString(),
                To = a.To.ToShortDateString(),
                Approval = (a.FirstApproval == null && a.SecondApproval == null) ? "Pending" : (a.FirstApproval == true || a.SecondApproval == true) ? "Approved" : "Rejected",
                AbsenceCause = a.AbsenceCause,
                FirstApproval = a.FirstApproval,
                FirstRejectionCause = a.FirstRejectionCause,
                FirstApprovedByName = a.FirstApprovedByNavigation != null ? a.FirstApprovedByNavigation?.FirstName + " " + a.FirstApprovedByNavigation?.LastName : "",

                FirstApprovedByImg = a.FirstApprovedByNavigation != null && a.FirstApprovedByNavigation?.PhotoUrl != null ? Globals.baseURL + "\\" + a.FirstApprovedByNavigation?.PhotoUrl : "",
                SecondApproval = a.SecondApproval,
                SecondRejectionCause = a.SecondRejectionCause,
                SecondApprovedByName = a.SecondApprovedByNavigation != null ? a.SecondApprovedByNavigation.FirstName + " " + a.SecondApprovedByNavigation.LastName : "",
                SecondApprovedByImg = a.SecondApprovedByNavigation != null && a.SecondApprovedByNavigation.PhotoUrl != null ? Globals.baseURL + "\\" + a.SecondApprovedByNavigation.PhotoUrl : ""
            }).ToList();
            Response.Data.AbsenceRequestsList = list;
            return Response;
        }

        public BaseResponseWithData<string> GetUsersReportExcell(bool? Active, int? DeptID, long? teamID, string CompName, bool? IsUser)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                //var inventoryItemsViewData = _unitOfWork.VInventoryStoreItemMovements.FindAll(a => a.date)
                var HrUsersQueryable = _unitOfWork.HrUsers.FindAllQueryable(x => true, new[] { "Department", "Branch", "User", "MilitaryStatus", "MaritalStatus", "JobTitle", "Team" });

                var currentDate = DateTime.Now;
                if (Active != null)
                {
                    HrUsersQueryable = HrUsersQueryable.Where(x => x.Active == Active);
                }
                //if (DeptID != null)
                //{
                //    HrUsersQueryable = HrUsersQueryable.Where(x => x.DepartmentId == DeptID);
                //}
                //if (teamID != null)
                //{
                //    HrUsersQueryable = HrUsersQueryable.Where(x => x.TeamId == teamID);
                //}

                if (IsUser != null)
                {
                    HrUsersQueryable = HrUsersQueryable.Where(x => x.IsUser == IsUser);
                }


                var HrUsersList = HrUsersQueryable.ToList();

                var nationalities = _unitOfWork.Nationalities.GetAll();

                ExcelPackage excel = new ExcelPackage();
                var sheet = excel.Workbook.Worksheets.Add($"Users Report");

                for (int col = 1; col <= 6; col++) sheet.Column(col).Width = 25;
                sheet.DefaultRowHeight = 15;
                sheet.Cells[1, 1].Value = "FirstName";
                sheet.Cells[1, 2].Value = "MiddleName";
                sheet.Cells[1, 3].Value = "LastName";
                sheet.Cells[1, 4].Value = "Active";
                sheet.Cells[1, 5].Value = "CreationDate";
                sheet.Cells[1, 6].Value = "CreatedBy";
                sheet.Cells[1, 7].Value = "Branch Name";
                sheet.Cells[1, 8].Value = "Department Name";
                sheet.Cells[1, 9].Value = "JobTitle";
                sheet.Cells[1, 10].Value = "DateOfBirth";
                sheet.Cells[1, 11].Value = "LandLine";
                sheet.Cells[1, 12].Value = "NationalityId";
                sheet.Cells[1, 13].Value = "MaritalStatus";
                sheet.Cells[1, 14].Value = "MilitaryStatus";
                sheet.Cells[1, 15].Value = "Team Name";
                sheet.Cells[1, 16].Value = "IsUser";
                sheet.Cells[1, 17].Value = "Mobile";
                sheet.Cells[1, 18].Value = "Email";
                sheet.Cells[1, 19].Value = "Gender";
                //sheet.Cells[1, 20].Value = "IsDeleted";
                sheet.Cells[1, 20].Value = "Active 'User'";
                sheet.Cells[1, 21].Value = "User CreationDate";


                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Cells[1, 1, 1, 21].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, 21].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[1, 1, 1, 21].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[1, 1, 1, 21].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                var rowCount = 2;
                foreach (var HrUser in HrUsersList)
                {
                    sheet.Cells[rowCount, 1].Value = HrUser.FirstName;
                    sheet.Cells[rowCount, 2].Value = HrUser.MiddleName != null ? HrUser.MiddleName : "";
                    sheet.Cells[rowCount, 3].Value = HrUser.LastName;
                    sheet.Cells[rowCount, 4].Value = HrUser.Active;
                    sheet.Cells[rowCount, 5].Value = HrUser.CreationDate.ToString("yyyy-MM-dd");
                    sheet.Cells[rowCount, 6].Value = HrUser.CreatedBy.FirstName + " " + HrUser.CreatedBy.LastName;
                    //sheet.Cells[rowCount, 7].Value = HrUser.Branch != null ? HrUser.Branch.Name : "";
                    //sheet.Cells[rowCount, 8].Value = HrUser.Department != null ? HrUser.Department.Name : "";
                    sheet.Cells[rowCount, 9].Value = HrUser.JobTitle != null ? HrUser.JobTitle.Name : "";
                    sheet.Cells[rowCount, 10].Value = HrUser.DateOfBirth != null ? ((DateTime)HrUser.DateOfBirth).ToString("yyyy-MM-dd") : "";
                    sheet.Cells[rowCount, 11].Value = HrUser.LandLine;
                    sheet.Cells[rowCount, 12].Value = nationalities.Where(a => a.Id == HrUser.NationalityId).FirstOrDefault() != null ? nationalities.Where(a => a.Id == HrUser.NationalityId).FirstOrDefault().Nationality1 : "";
                    sheet.Cells[rowCount, 13].Value = HrUser.MaritalStatus != null ? HrUser.MaritalStatus.Name : "";
                    sheet.Cells[rowCount, 14].Value = HrUser.MilitaryStatus != null ? HrUser.MilitaryStatus.Name : "";
                    //sheet.Cells[rowCount, 15].Value = HrUser.Team != null ? HrUser.Team.Name : "";
                    sheet.Cells[rowCount, 16].Value = HrUser.IsUser;
                    //sheet.Cells[rowCount, 17].Value = HrUser.Mobile;
                    sheet.Cells[rowCount, 18].Value = HrUser.Email;
                    sheet.Cells[rowCount, 19].Value = HrUser.Gender;
                    //sheet.Cells[rowCount, 20].Value = HrUser.IsDeleted;
                    sheet.Cells[rowCount, 20].Value = HrUser.User != null ? HrUser.User.Active : "";
                    sheet.Cells[rowCount, 21].Value = HrUser.User != null ? HrUser.User.CreationDate.ToString("yyyy-MM-dd") : "";


                    sheet.Cells[rowCount, 1, rowCount, 21].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowCount, 1, rowCount, 21].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    rowCount++;
                }

                for (int i = 1; i < 23; i++)
                {
                    sheet.Column(i).AutoFit();
                }
                //----------------------------file saving------------------------------
                var path = $"Attachments\\{CompName}\\UsersReport";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                var excelPath = savedPath + $"\\UsersReport_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var fullPath = Globals.baseURL + "\\" + path + $"\\UsersReport_{date}.xlsx";

                response.Data = fullPath;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponse AddVacationTypeForUser(AddVacationTypeForUserDto dto, long creator)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (dto != null)
                {
                    if (dto.HrUserId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "HrUser Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var HrUser = _unitOfWork.HrUsers.GetById(dto.HrUserId);
                    if (HrUser == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "HrUser not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.VacationTypesIds.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "Vacation Types Id List is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == HrUser.Id && a.IsCurrent).FirstOrDefault();
                    if (contract == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err104";
                        error.ErrorMSG = "HrUser Doesn't Have Contract";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var vacationtypes = _unitOfWork.ContractLeaveSetting.FindAll(x => x.Active == true && (x.Archive == false || x.Archive == null)).ToList();
                    var UserVacations = _unitOfWork.ContractLeaveEmployees.FindAll(x => x.Active == true && x.HrUserId == HrUser.Id && x.ContractId == contract.Id).ToList();
                    var list = new List<ContractLeaveEmployee>();
                    foreach (var a in dto.VacationTypesIds)
                    {
                        if (!UserVacations.Select(x => x.ContractLeaveSettingId).Contains(a))
                        {
                            if (!vacationtypes.Select(a => a.Id).Contains(a))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err104";
                                error.ErrorMSG = $"Vacation Type not found at {dto.VacationTypesIds.IndexOf(a) + 1}";
                                Response.Errors.Add(error);
                            }
                            var item = vacationtypes.FirstOrDefault(x => x.Id == a);
                            var contractleave = new ContractLeaveEmployee()
                            {
                                ContractId = contract.Id,
                                ContractLeaveSettingId = a,
                                Active = true,
                                LeaveAllowed = "Allowed",
                                Balance = item.BalancePerMonth != null ? (int)item.BalancePerMonth * ((contract.EndDate.Month - contract.StartDate.Month) + 12 * (contract.EndDate.Year - contract.StartDate.Year)) : 0,
                                Used = 0,
                                Remain = item.BalancePerMonth != null ? (int)item.BalancePerMonth * ((contract.EndDate.Month - contract.StartDate.Month) + 12 * (contract.EndDate.Year - contract.StartDate.Year)) : 0,
                                HrUserId = HrUser.Id,
                                BalancePerMonth = item.BalancePerMonth,
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                CreatedBy = creator,
                                ModifiedBy = creator
                            };
                            list.Add(contractleave);
                        }
                    }
                    _unitOfWork.ContractLeaveEmployees.AddRange(list);
                    _unitOfWork.Complete();
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "Invalid Data";
                    Response.Errors.Add(error);
                    return Response;
                }

                Response.Result = true;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<GetListOfEmployeeResponse> GetListOfEmployeeInfo(GetEmployeeInfoHeader header, long userID)
        {
            GetListOfEmployeeResponse response = new GetListOfEmployeeResponse();
            response.Result = true;
            response.Errors = new List<Error>();

            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");
            try
            {

                /*IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                WebHeaderCollection Request.Headers = request.Headers;*/
                //HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                //response.Errors = validation.errors;
                //response.Result = validation.result;
                if (response.Result)
                {
                    var GetEmployeeResponseList = new List<EmployeeInfoData>();

                    UserLogin login = new UserLogin();

                    //string CompName = "";

                    //if (CompName == "proauto")
                    //{
                    //    _Context = GlobalEntity.ContextGarasAuto;
                    //}
                    //else if (CompName == "marinaplt")
                    //{
                    //    _Context = GlobalEntity.ContextGarasMarina;
                    //}
                    //else if (CompName == "piaroma")
                    //{
                    //    _Context = GlobalEntity.ContextGarasAroma;
                    //}


                    if (!string.IsNullOrEmpty(header.SearchKey))
                    {
                        header.SearchKey = header.SearchKey;
                        header.SearchKey = HttpUtility.UrlDecode(header.SearchKey);
                    }
                    /*int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(header.CurrentPage) && int.TryParse(header.CurrentPage, out CurrentPage))
                    {
                        int.TryParse(header.CurrentPage, out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(header.NumberOfItemsPerPage) && int.TryParse(header.NumberOfItemsPerPage, out NumberOfItemsPerPage))
                    {
                        int.TryParse(header.NumberOfItemsPerPage, out NumberOfItemsPerPage);
                    }

                    long BranchID = 0;
                    if (!string.IsNullOrEmpty(header.BranchID) && long.TryParse(header.BranchID, out BranchID))
                    {
                        long.TryParse(header.BranchID, out BranchID);
                    }
                    long DepartmentID = 0;
                    if (!string.IsNullOrEmpty(header.DepartmentID) && long.TryParse(header.DepartmentID, out DepartmentID))
                    {
                        long.TryParse(header.DepartmentID, out DepartmentID);
                    }

                    long UserID = 0;
                    if (!string.IsNullOrEmpty(header.UserID) && long.TryParse(header.UserID, out UserID))
                    {
                        long.TryParse(header.UserID, out UserID);
                    }
                    bool isExpired = false;
                    if (!string.IsNullOrEmpty(header.isExpired))
                    {
                        isExpired = bool.Parse(header.isExpired);
                    }
                    DateTime expiredIn = DateTime.Now;
                    if (!string.IsNullOrEmpty(header.expiredIn) && DateTime.TryParse(header.expiredIn, out expiredIn))
                    {
                        expiredIn = DateTime.Parse(header.expiredIn);
                    }*/

                    if (response.Result)
                    {
                        var GetEmployeeInfoDB = _unitOfWork.Users.FindAllQueryable(a => true, new[] { "Department", "Branch", "JobTitle" });
                        if (userID != 1)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.Id != 1);
                        }
                        if (!string.IsNullOrEmpty(header.SearchKey))
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.FirstName.Contains(header.SearchKey) ||
                                                                             x.LastName.Contains(header.SearchKey) ||
                                                                             x.MiddleName.Contains(header.SearchKey) ||
                                                                             x.Email.Contains(header.SearchKey) ||
                                                                             x.Mobile == header.SearchKey
                                                                             ).AsQueryable();
                        }
                        if (header.isExpired)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(a => _unitOfWork.HremployeeAttachments.Any(x => x.EmployeeUserId == a.Id && x.Active && x.ExpiredDate < header.expiredIn)).AsQueryable();
                            //Where(a => _unitOfWork.HremployeeAttachments.Any(x => x.EmployeeUserId == a.Id && x.Active && x.ExpiredDate < header.expiredIn))

                        }
                        if (header.DepartmentID != 0)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.DepartmentId == header.DepartmentID).AsQueryable();
                        }
                        if (header.BranchID != 0)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.BranchId == header.BranchID).AsQueryable();
                        }
                        if (header.UserID != 0)
                        {
                            GetEmployeeInfoDB = GetEmployeeInfoDB.Where(x => x.Id == header.UserID).AsQueryable();
                        }
                        var EmployeePagingList = PagedList<User>.CreateOrdered(GetEmployeeInfoDB.OrderBy(x => x.FirstName), header.CurrentPage, header.NumberOfItemsPerPage);
                        response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = header.CurrentPage,
                            TotalPages = EmployeePagingList.TotalPages,
                            ItemsPerPage = header.NumberOfItemsPerPage,
                            TotalItems = EmployeePagingList.TotalCount
                        };



                        if (EmployeePagingList != null)
                        {

                            foreach (var GetEmployeeOBJ in EmployeePagingList)
                            {
                                var GetEmployeeResponse = new EmployeeInfoData();

                                GetEmployeeResponse.ID = (int)GetEmployeeOBJ.Id;

                                GetEmployeeResponse.EmployeeName = GetEmployeeOBJ.FirstName + " " + GetEmployeeOBJ?.MiddleName + " " + GetEmployeeOBJ.LastName;//Common.GetUserName(GetEmployeeOBJ.Id, _Context);

                                GetEmployeeResponse.Age = GetEmployeeOBJ.Age;

                                GetEmployeeResponse.FirstName = GetEmployeeOBJ.FirstName;
                                GetEmployeeResponse.LastName = GetEmployeeOBJ.LastName;
                                GetEmployeeResponse.Active = GetEmployeeOBJ.Active;

                                GetEmployeeResponse.expiredDocumentsCount = _unitOfWork.HremployeeAttachments.FindAll(a => a.Active && a.EmployeeUserId == (int)GetEmployeeOBJ.Id && a.ExpiredDate < header.expiredIn).Count();

                                GetEmployeeResponse.MiddleName = GetEmployeeOBJ.MiddleName;

                                GetEmployeeResponse.BranchID = GetEmployeeOBJ.BranchId;

                                GetEmployeeResponse.JobTitleID = GetEmployeeOBJ.JobTitleId;


                                GetEmployeeResponse.Password = Encrypt_Decrypt.Decrypt(GetEmployeeOBJ.Password, key).Trim();

                                if (GetEmployeeOBJ.BranchId != 0 && GetEmployeeOBJ.BranchId != null)
                                {

                                    GetEmployeeResponse.BranchName = GetEmployeeOBJ?.Branch?.Name;//Common.GetBranchName((int)GetEmployeeOBJ.BranchId, _Context);

                                }
                                GetEmployeeResponse.DepartmentID = GetEmployeeOBJ.DepartmentId;

                                if (GetEmployeeOBJ.DepartmentId != 0 && GetEmployeeOBJ.DepartmentId != null)
                                {
                                    GetEmployeeResponse.DepartmentName = GetEmployeeOBJ?.Department.Name;//Common.GetDepartmentName((int)GetEmployeeOBJ.DepartmentId, _Context);

                                }


                                GetEmployeeResponse.Email = GetEmployeeOBJ.Email;

                                GetEmployeeResponse.Gender = GetEmployeeOBJ.Gender;

                                if (GetEmployeeOBJ.JobTitleId != 0 && GetEmployeeOBJ.JobTitleId != null)
                                {
                                    GetEmployeeResponse.JobTitleName = GetEmployeeOBJ?.JobTitle?.Name;//Common.GetJobTitleName((int)GetEmployeeOBJ.DepartmentId, _Context);

                                }


                                GetEmployeeResponse.Mobile = GetEmployeeOBJ.Mobile;

                                GetEmployeeResponse.Photo = response.UserImageURL = baseURL + GetEmployeeOBJ.PhotoUrl;
                                /* "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(GetEmployeeOBJ.Id.ToString(), key)) + "&type=photo&CompName=" + Request.Headers["CompanyName"].ToString().ToLower();*/







                                GetEmployeeResponseList.Add(GetEmployeeResponse);
                            }



                        }

                    }


                    response.EmployeeInfoList = GetEmployeeResponseList;

                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public ActionResult<BaseResponseWithID> AddEditEmployeeAttachments(AddEditEmployeeAttachmentsRequest request, string CompanyName, long userID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            string baseURL = MyConfig.GetValue<string>("AppSettings:baseURL");
            try
            {
                if (Response.Result)
                {
                    if (request.Attachments.Count > 0)
                    {
                        int Count = 0;
                        foreach (var att in request.Attachments)
                        {
                            Count++;
                            //Insert
                            //edit-delete
                            if (att.ID != null && att.Active == false)
                            {
                                var employeeAttachment = _unitOfWork.HremployeeAttachments.GetById(att.ID ?? 0);
                                if (employeeAttachment != null)
                                {
                                    var AttachmentPath = Path.Combine(_host.WebRootPath, "Attachments/", employeeAttachment?.AttachmentPath);

                                    if (System.IO.File.Exists(AttachmentPath))
                                    {
                                        System.IO.File.Delete(AttachmentPath);
                                    }

                                    _unitOfWork.HremployeeAttachments.Delete(employeeAttachment);
                                }

                            }
                            else if (att.ID == null)
                            {
                                var cat = "";
                                DateTime expireDate = DateTime.Now;
                                if (string.IsNullOrEmpty(att.ExpiredDate.ToString()) || !DateTime.TryParse(att.ExpiredDate.ToString(), out expireDate))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "Invalid Expire Date on Object count : " + Count;
                                    Response.Errors.Add(error);
                                    return Response;
                                }

                                //DateTime date = DateTime.Now;
                                //DateTime.TryParse(att.ExpiredDate, out date);
                                if (att.CategoryName.ToLower().Trim() == "other" && att.OtherValue != null)
                                {
                                    cat = "Other_" + att.OtherValue;
                                }
                                else
                                {
                                    cat = att.CategoryName;
                                }
                                var employeeAttachment = new HremployeeAttachment()
                                {
                                    EmployeeUserId = request.EmployeeId,
                                    FileName = att.Attachment.FileName,
                                    FileExtention = att.Attachment.FileExtension,
                                    AttachmentPath = Common.SaveFile("Attachments/" + CompanyName + "/" + request.EmployeeId + "/", att.Attachment.FileContent, att.Attachment.FileName, att.Attachment.FileExtension, _host),
                                    CategoryName = cat,
                                    Active = true,
                                    ExpiredDate = expireDate,
                                    //expireDate,
                                    CreationDate = DateTime.Now,
                                    CreatedBy = userID,
                                    ModifiedDate = DateTime.Now,
                                    ModifiedBy = userID
                                };
                                _unitOfWork.HremployeeAttachments.Add(employeeAttachment);
                            }

                        }

                        _unitOfWork.Complete();
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "No Attachments Found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                Response.ID = request.EmployeeId;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }

        }

        public ActionResult<HrEmployeeAttachmentResponse> GetHREmployeeAttachment(long attachmentId)
        {
            HrEmployeeAttachmentResponse Response = new HrEmployeeAttachmentResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    /* long attachmentID = 0;
                     if (string.IsNullOrEmpty(attachmentId) || !long.TryParse(attachmentID, out attachmentId))
                     {
                         Response.Result = false;
                         Error error = new Error();
                         error.ErrorCode = "Err25";
                         error.ErrorMSG = "attachmentId Is required";
                         Response.Errors.Add(error);
                     }*/
                    var att = _unitOfWork.HremployeeAttachments.Find(a => a.Id == attachmentId && a.Active);
                    if (att != null)
                    {
                        var attachment = new HrEmployeeAttachment()
                        {
                            ID = att.Id,
                            Attachment = new Attachment()
                            {
                                FileName = att.FileName,
                                FileExtension = att.FileExtention,
                                FilePath = att.AttachmentPath
                            },
                            CategoryName = att.CategoryName,
                            ExpiredDate = att.ExpiredDate.ToString(),
                            Active = att.Active,
                        };
                        Response.Attachment = attachment;

                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "attachment not found";
                        Response.Errors.Add(error);
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err25";
                    error.ErrorMSG = "Error";
                    Response.Errors.Add(error);
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<GetEmployeeExpiredDocumentsResponse> GetEmployeeDocuments(GetEmployeeDocumentsHeader header)
        {
            GetEmployeeExpiredDocumentsResponse Response = new GetEmployeeExpiredDocumentsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    /* long EmployeeId = 0;
                     if (string.IsNullOrEmpty(Request.Headers["EmployeeId"]) || !long.TryParse(Request.Headers["EmployeeId"], out EmployeeId))
                     {
                         EmployeeId = 0;
                     }
                     string category = "";
                     if (!string.IsNullOrEmpty(Request.Headers["category"]))
                     {
                         category = Request.Headers["category"];
                     }
                     DateTime expireDate = DateTime.MinValue;
                     if (!string.IsNullOrEmpty(Request.Headers["expireDate"]) || DateTime.TryParse(Request.Headers["expireDate"], out expireDate))
                     {
                         expireDate = DateTime.Parse(Request.Headers["expireDate"].ToString());
                     }*/
                    IQueryable<HremployeeAttachment> attachments = _unitOfWork.HremployeeAttachments.FindAllQueryable(a => a.Active).AsQueryable();
                    /*_Context.HREmployeeAttachments.Where(a => a.Active).Select(x => new HrEmployeeAttachment() {ID = x.ID, CategoryName = x.CategoryName, ExpiredDate = x.ExpiredDate.ToString(), Attachment = new Attachment() { FileName = x.FileName, FilePath = x.AttachmentPath, FileExtension = x.FileExtention } }).AsQueryable();*/
                    if (header.EmployeeId != 0)
                    {
                        attachments = attachments.Where(a => a.EmployeeUserId == header.EmployeeId).AsQueryable();
                    }
                    if (header.expireDate != DateTime.MinValue)
                    {
                        attachments = attachments.Where(a => a.ExpiredDate < header.expireDate).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(header.category))
                    {
                        attachments = attachments.Where(a => a.CategoryName.ToLower().Trim() == header.category.ToLower().Trim()).AsQueryable();
                    }
                    Response.Attachments = attachments.Select(x => new HrEmployeeAttachment() { ID = x.Id, CategoryName = x.CategoryName, ExpiredDate = x.ExpiredDate.ToString(), Attachment = new Attachment() { FileName = x.FileName, FilePath = x.AttachmentPath, FileExtension = x.FileExtention }, Active = x.Active }).ToList();
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<ActionResult<BaseResponseWithID>> AddEditJobTitle(JobTitleData request, long userID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (string.IsNullOrEmpty(request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Job Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        //var modifiedUser = Common.GetUserName(userID, _Context);

                        if (request.ID != null && request.ID != 0)
                        {
                            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                            ID.Value = request.ID;


                            object[] param = new object[] { ID };
                            var JobTitleDB = _unitOfWork.JobTitles.GetById(request.ID ?? 0);//_Context.Database.SqlQueryRaw<proc_JobTitleLoadByPrimaryKey_Result>("Exec proc_JobTitleLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();
                            if (JobTitleDB != null)
                            {
                                // Update

                                //var JobTitleUpdateDB = await _unitOfWork.JobTitles.FindAsync(x => x.Id == request.ID);


                                if (JobTitleDB != null)
                                {
                                    JobTitleDB.Name = request.Name;
                                    JobTitleDB.Description = request.Description;
                                    JobTitleDB.Active = request.Active;
                                    JobTitleDB.ModifiedBy = userID;
                                    JobTitleDB.ModifiedDate = DateTime.Now;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Job Title!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Job Title  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert


                            var JobTitleDB = new JobTitle();


                            JobTitleDB.Name = request.Name;
                            JobTitleDB.Description = request.Description;
                            JobTitleDB.CreatedBy = userID;
                            JobTitleDB.CreationDate = DateTime.Now;
                            JobTitleDB.ModifiedBy = userID;
                            JobTitleDB.ModifiedDate = DateTime.Now;
                            JobTitleDB.Active = true;
                            _unitOfWork.JobTitles.Add(JobTitleDB);
                            var Res = _unitOfWork.Complete();




                            if (Res > 0)
                            {

                                Response.ID = JobTitleDB.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Job Title!!";
                                Response.Errors.Add(error);
                            }
                        }



                    }




                }

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

        public async Task<BaseResponseWithId<long>> EditEmployeeRoleNew(EmployeeRoleData request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {

                        var ListUserRoleVM = request.userRolesData;
                        var ListUserRoleDB = (await _unitOfWork.UserRoles.FindAllAsync(x => x.UserId == request.UserID)).ToList();
                        if (ListUserRoleVM != null && ListUserRoleVM.Count() > 0)
                        {
                            var IDSListUserRoleDB = ListUserRoleDB.Select(x => x.Id).ToList();
                            var IDSListUserRoleVM = new List<long>();
                            foreach (var item in ListUserRoleVM)
                            {
                                var CheckIfExistBefore = ListUserRoleDB.Where(x => x.UserId == request.UserID && x.RoleId == item.RoleID).FirstOrDefault();
                                if (CheckIfExistBefore == null)
                                {
                                    var InsertUserRolesObjDB = new UserRole();
                                    InsertUserRolesObjDB.UserId = request.UserID;
                                    InsertUserRolesObjDB.RoleId = item.RoleID;


                                    InsertUserRolesObjDB.CreatedBy = validation.userID;

                                    InsertUserRolesObjDB.CreationDate = DateTime.Now;

                                    _unitOfWork.UserRoles.Add(InsertUserRolesObjDB);
                                    var Res = _unitOfWork.Complete();
                                    if (Res > 0)
                                    {
                                        IDSListUserRoleDB.Add(InsertUserRolesObjDB.Id);
                                        IDSListUserRoleVM.Add(InsertUserRolesObjDB.Id);
                                    }
                                    var message = $"Add User Role\nadd Role Id {item.RoleID} To User Id {request.UserID}";

                                    _logService.AddLogError("EditEmployeeRole", message, validation.userID, validation.CompanyName);
                                }
                                else
                                {
                                    IDSListUserRoleVM.Add(CheckIfExistBefore.Id);
                                }
                            }
                            var IDSListToRemove = IDSListUserRoleDB.Except(IDSListUserRoleVM).ToList();

                            var DeletedUserRoleListDB = ListUserRoleDB.Where(x => IDSListToRemove.Contains(x.Id)).ToList();
                            _unitOfWork.UserRoles.DeleteRange(DeletedUserRoleListDB);
                            _unitOfWork.Complete();
                            DeletedUserRoleListDB.Select(a =>
                            {
                                var message = $"Delete User Role\nadd Role Id {a.RoleId} of User Id {request.UserID}";

                                _logService.AddLogError("EditEmployeeRole", message, validation.userID, validation.CompanyName);
                                return 0;
                            });
                        }
                        else // List is empty must be deleted
                        {
                            // delete list from DB
                            _unitOfWork.UserRoles.DeleteRange(ListUserRoleDB);
                            _unitOfWork.Complete();
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Faild To Insert this Role !!";
                        Response.Errors.Add(error);
                    }

                }
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

        public ActionResult<BaseResponseWithID> AddEditEmployeeInfo(EmployeeInfoDataList request, long userID, string CompName)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (string.IsNullOrEmpty(request.Email) || request.Email.Trim() == "")
                    {

                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P7";
                        error.ErrorMSG = "please write your Email";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.Password) || request.Password.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your password";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.Mobile) || request.Mobile.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your Mobile";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.FirstName) || request.FirstName.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your FirstName";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.LastName) || request.LastName.Trim() == "")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your LastName";
                        Response.Errors.Add(error);
                    }
                    byte[] EmployeePhoto = null;
                    //if (string.IsNullOrEmpty(Request.Photo)){
                    if (request.Photo != null)
                    {
                        EmployeePhoto = Convert.FromBase64String(request.Photo);
                    }


                    if (Response.Result)
                    {
                        if (request.ID != 0 && request.ID != null)
                        {

                            var EmployeeDB = _unitOfWork.Users.GetById((long)request.ID);

                            if (EmployeeDB != null)
                            {
                                DateTime DateOfBirth = DateTime.Now;
                                DateTime.TryParse(request.DateOfBirth, out DateOfBirth);
                                EmployeeDB.Password = Encrypt_Decrypt.Encrypt(request.Password, key);
                                EmployeeDB.FirstName = request.FirstName;
                                EmployeeDB.LastName = request.LastName;
                                EmployeeDB.MiddleName = request.MiddleName;
                                EmployeeDB.Mobile = request.Mobile;
                                EmployeeDB.Active = request.Active;
                                EmployeeDB.Email = request.Email;
                                EmployeeDB.Age = request.Age;
                                if (request.Photo != null)
                                {
                                    EmployeeDB.Photo = EmployeePhoto;
                                }
                                EmployeeDB.Modified = DateTime.Now;
                                EmployeeDB.ModifiedBy = userID;
                                EmployeeDB.BranchId = request.BranchID;
                                EmployeeDB.DepartmentId = request.DepartmentID;
                                EmployeeDB.JobTitleId = request.JobTitleID;
                                EmployeeDB.OldId = request.OldID;
                                _unitOfWork.Users.Update(EmployeeDB);
                                var EmployeeDBUpdate = _unitOfWork.Complete();
                                if (EmployeeDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = (long)request.ID;
                                    if (request.Photo != null)
                                    {
                                        if (System.IO.File.Exists(_host.WebRootPath + '/' + EmployeeDB.PhotoUrl))
                                        {
                                            System.IO.File.Delete(_host.WebRootPath + '/' + EmployeeDB.PhotoUrl);
                                        }
                                        var FilePath = Common.SaveFile("Attachments/" + CompName + "/UsersImages/", request.Photo, EmployeeDB.Id + "_" + EmployeeDB.FirstName + "_" + EmployeeDB.LastName, "png", _host);
                                        EmployeeDB.PhotoUrl = FilePath;
                                        _unitOfWork.Complete();
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Employee!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Employee Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                            HrUser HrUser = _unitOfWork.HrUsers.FindAll(a => a.IsUser && a.UserId == EmployeeDB.Id).FirstOrDefault();
                            if (HrUser == null && request.AddHrUser == true)
                            {
                                HrUser = new HrUser()
                                {
                                    FirstName = EmployeeDB.FirstName,
                                    ArfirstName = EmployeeDB.FirstName,
                                    MiddleName = EmployeeDB.MiddleName,
                                    ArmiddleName = EmployeeDB.MiddleName,
                                    LastName = EmployeeDB.LastName,
                                    ArlastName = EmployeeDB.LastName,
                                    Active = true,
                                    CreationDate = DateTime.Now,
                                    Modified = DateTime.Now,
                                    CreatedById = userID,
                                    ModifiedById = userID,
                                    //BranchId = EmployeeDB.BranchId,
                                    //DepartmentId = EmployeeDB.DepartmentId,
                                    JobTitleId = EmployeeDB.JobTitleId,
                                    LandLine = EmployeeDB.Mobile,
                                    //Mobile = EmployeeDB.Mobile,
                                    Email = EmployeeDB.Email,
                                    UserId = EmployeeDB.Id,
                                    IsUser = true
                                };
                                _unitOfWork.HrUsers.Add(HrUser);
                                _unitOfWork.Complete();
                                if (request.Photo != null)
                                {
                                    var virtualPath = $"Attachments\\{CompName}\\HrUser\\{HrUser.Id}\\";
                                    var FilePath = Common.SaveFile(virtualPath, request.Photo, HrUser.Id + "_" + HrUser.FirstName + "_" + HrUser.LastName, "png", _host);
                                    HrUser.ImgPath = FilePath;
                                    _unitOfWork.Complete();
                                }

                                else
                                {
                                    HrUser.FirstName = EmployeeDB.FirstName;
                                    HrUser.LastName = EmployeeDB.LastName;
                                    HrUser.Active = true;
                                    HrUser.CreationDate = DateTime.Now;
                                    HrUser.Modified = DateTime.Now;
                                    HrUser.CreatedById = userID;
                                    HrUser.ModifiedById = userID;
                                    //HrUser.BranchId = EmployeeDB.BranchId;
                                    //HrUser.DepartmentId = EmployeeDB.DepartmentId;
                                    HrUser.JobTitleId = EmployeeDB.JobTitleId;
                                    HrUser.LandLine = EmployeeDB.Mobile;
                                    //HrUser.Mobile = EmployeeDB.Mobile;
                                    HrUser.Email = EmployeeDB.Email;
                                    HrUser.IsUser = true;
                                    HrUser.UserId = EmployeeDB.Id;
                                    if (request.Photo != null)
                                    {
                                        if (System.IO.File.Exists(_host.WebRootPath + '/' + HrUser.ImgPath))
                                        {
                                            System.IO.File.Delete(_host.WebRootPath + '/' + HrUser.ImgPath);
                                        }
                                        var virtualPath = $"Attachments\\{CompName}\\HrUser\\{HrUser.Id}\\";
                                        var FilePath = Common.SaveFile(virtualPath, request.Photo, HrUser.Id + "_" + HrUser.FirstName + "_" + HrUser.LastName, "png", _host);
                                        HrUser.ImgPath = FilePath;
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                        }
                        else
                        {

                            DateTime DateOfBirth = DateTime.Now;
                            DateTime.TryParse(request.DateOfBirth, out DateOfBirth);
                            // Insert
                            var user = new User()
                            {
                                Password = Encrypt_Decrypt.Encrypt(request.Password, key),
                                FirstName = request.FirstName,
                                LastName = request.LastName,
                                MiddleName = request.MiddleName,
                                Mobile = request.Mobile,
                                Active = request.Active,
                                Email = request.Email,
                                Age = request.Age,
                                Photo = EmployeePhoto,
                                Modified = DateTime.Now,
                                ModifiedBy = userID,
                                BranchId = request.BranchID,
                                DepartmentId = request.DepartmentID,
                                JobTitleId = request.JobTitleID,
                                OldId = request.OldID,
                                CreatedBy = userID,
                                CreationDate = DateTime.Now
                            };
                            var newEmp = _unitOfWork.Users.Add(user);
                            var EmployeeInserted = _unitOfWork.Complete();
                            long EInvoiceCompanyActivityInsertedID = 0;
                            if (EmployeeInserted > 0)
                            {
                                EInvoiceCompanyActivityInsertedID = user.Id;
                                Response.ID = EInvoiceCompanyActivityInsertedID;
                                if (request.Photo != null)
                                {
                                    var FilePath = Common.SaveFile("Attachments/" + CompName + "/UsersImages/", request.Photo, user.Id + "_" + user.FirstName + "_" + user.LastName, "png", _host);
                                    user.PhotoUrl = FilePath;
                                    _unitOfWork.Complete();
                                }
                            }

                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Employee !!";
                                Response.Errors.Add(error);
                            }
                            if (request.AddHrUser == true)
                            {
                                HrUser HrUser = new HrUser()
                                {
                                    FirstName = user.FirstName,
                                    ArfirstName = user.FirstName,
                                    MiddleName = user.MiddleName,
                                    ArmiddleName = user.MiddleName,
                                    LastName = user.LastName,
                                    ArlastName = user.LastName,
                                    Active = true,
                                    CreationDate = DateTime.Now,
                                    Modified = DateTime.Now,
                                    CreatedById = userID,
                                    ModifiedById = userID,
                                    //BranchId = user.BranchId,
                                    //DepartmentId = user.DepartmentId,
                                    JobTitleId = user.JobTitleId,
                                    LandLine = user.Mobile,
                                    //Mobile = user.Mobile,
                                    Email = user.Email,
                                    UserId = user.Id,
                                    IsUser = true
                                };
                                _unitOfWork.HrUsers.Add(HrUser);
                                _unitOfWork.Complete();
                                if (request.Photo != null)
                                {
                                    var virtualPath = $"Attachments\\{CompName}\\HrUser\\{HrUser.Id}\\";
                                    var FilePath = Common.SaveFile(virtualPath, request.Photo, HrUser.Id + "_" + HrUser.FirstName + "_" + HrUser.LastName, "png", _host);
                                    HrUser.ImgPath = FilePath;
                                    _unitOfWork.Complete();
                                }
                            }
                        }


                    }
                    ;




                }

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

        public ActionResult<GetTeamUserResponse> GetTeamUser([FromHeader] long TeamID = 0)
        {
            GetTeamUserResponse response = new GetTeamUserResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var TeamUserDBResponse = new TeamUserData();
                var UserList = new List<UserData>();
                if (response.Result)
                {
                    /*
                                        long TeamID = 0;
                                        if (!string.IsNullOrEmpty(Request.Headers["TeamID"]) && long.TryParse(Request.Headers["TeamID"], out TeamID))
                                        {
                                            long.TryParse(Request.Headers["TeamID"], out TeamID);
                                        }*/

                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var TeamDB = _unitOfWork.Teams.Find(x => x.Id == TeamID, new[] { "" });
                        var TeamUsersDB = _unitOfWork.UserTeams.FindAll(x => x.TeamId == TeamID, new[] { "User" }).ToList();

                        var dept = _unitOfWork.Departments.GetById(TeamDB.DepartmentId);
                        if (TeamDB != null)
                        {



                            TeamUserDBResponse.ID = TeamDB.Id;

                            TeamUserDBResponse.DepartmentID = TeamDB.DepartmentId;
                            TeamUserDBResponse.DepartmentName = dept?.Name;//Common.GetDepartmentName(TeamDB.DepartmentId, _Context);

                            TeamUserDBResponse.CreatedBy = TeamDB.CreatedBy.ToString();

                            TeamUserDBResponse.ModifiedBy = TeamDB.ModifiedBy.ToString();

                            foreach (var User in TeamUsersDB)
                            {

                                var UserRepsonseObj = new UserData();

                                UserRepsonseObj.UserID = (long)User.UserId;
                                UserRepsonseObj.ID = User.Id;
                                UserRepsonseObj.Name = User.User.FirstName + " " + User.User.MiddleName + " " + User.User.LastName;//Common.GetUserName((long)User.UserId, _Context);

                                UserList.Add(UserRepsonseObj);
                                TeamUserDBResponse.UserDataList = UserList;

                            }







                        }

                    }

                }
                response.TeamUserObj = TeamUserDBResponse;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public ActionResult<GetTeamResponse> GetTeamsIndex()
        {
            GetTeamResponse response = new GetTeamResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var TeamResponseList = new List<TeamData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var TeamDB = _unitOfWork.Teams.GetAll();
                        var deptsIDs = TeamDB.Select(a => a.DepartmentId).ToList();

                        var depData = _unitOfWork.Departments.FindAll(a => deptsIDs.Contains(a.Id)).ToList();

                        if (TeamDB != null && TeamDB.Count() > 0)
                        {

                            foreach (var TeamDBOBJ in TeamDB)
                            {
                                var currentDept = depData.Where(a => a.Id == TeamDBOBJ.DepartmentId).FirstOrDefault();
                                var TeamDBResponse = new TeamData();

                                TeamDBResponse.ID = TeamDBOBJ.Id;

                                TeamDBResponse.DepartmentID = TeamDBOBJ.DepartmentId;

                                TeamDBResponse.DepartmentName = currentDept.Name;//Common.GetDepartmentName(TeamDBOBJ.DepartmentId, _Context);

                                TeamDBResponse.Name = TeamDBOBJ.Name;

                                TeamDBResponse.Description = TeamDBOBJ.Description;

                                TeamDBResponse.Active = TeamDBOBJ.Active;

                                TeamDBResponse.CreatedBy = TeamDBOBJ.CreatedBy.ToString();

                                TeamDBResponse.ModifiedBy = TeamDBOBJ.ModifiedBy.ToString();


                                TeamResponseList.Add(TeamDBResponse);
                            }



                        }

                    }

                }
                response.TeamList = TeamResponseList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public async Task<ActionResult<GetUserRolegAndRoleResponse>> GetUserRoleAndGroup(int UserID = 0)
        {
            GetUserRolegAndRoleResponse response = new GetUserRolegAndRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetUserRoleList = new List<GetUserRoleData>();
                var GetUserGroupList = new List<GetUserGroupData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        /*int UserID = 0;
                        if (!string.IsNullOrEmpty(Request.Headers["UserID"]) && int.TryParse(Request.Headers["UserID"], out UserID))
                        {
                            int.TryParse(Request.Headers["UserID"], out UserID);
                        }*/


                        var UserRoleDB = await _unitOfWork.UserRoles.FindAllAsync(x => x.HrUserId == UserID);
                        var UserGroupDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.HrUserId == UserID);

                        var roles = _unitOfWork.Roles.FindAll(x => UserRoleDB.Select(a => a.RoleId).Contains(x.Id)).ToList();
                        var Groups = _unitOfWork.Groups.FindAll(x => UserGroupDB.Select(a => a.GroupId).Contains(x.Id)).ToList();
                        var user = _unitOfWork.HrUsers.Find(x => x.Id == UserID);
                        if (UserRoleDB != null && UserRoleDB.Count() > 0)
                        {

                            foreach (var UserRoleOBJ in UserRoleDB)
                            {
                                var UserRoleOBJResponse = new GetUserRoleData();

                                UserRoleOBJResponse.ID = UserRoleOBJ.Id;

                                UserRoleOBJResponse.RoleName = roles.Where(x => x.Id == UserRoleOBJ.RoleId).FirstOrDefault()?.Name;

                                UserRoleOBJResponse.UserName = user?.FirstName + " " + user?.LastName;

                                UserRoleOBJResponse.RoleID = UserRoleOBJ.RoleId;


                                GetUserRoleList.Add(UserRoleOBJResponse);
                            }



                        }

                        if (UserGroupDB != null && UserGroupDB.Count() > 0)
                        {

                            foreach (var UserGroupOBJ in UserGroupDB)
                            {
                                var UserGroupOBJResponse = new GetUserGroupData();

                                UserGroupOBJResponse.ID = UserGroupOBJ.Id;

                                UserGroupOBJResponse.GroupName = Groups.Where(x => x.Id == UserGroupOBJ.GroupId).FirstOrDefault()?.Name;

                                UserGroupOBJResponse.UserName = user?.FirstName + " " + user?.LastName;
                                UserGroupOBJResponse.GroupID = UserGroupOBJ.GroupId;


                                GetUserGroupList.Add(UserGroupOBJResponse);
                            }



                        }
                    }

                }
                response.UserRoleList = GetUserRoleList;
                response.UserGroupList = GetUserGroupList;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public async Task<GetUserRolegAndRoleResponse> GetUserRoleAndGroupNew(long UserID)
        {
            GetUserRolegAndRoleResponse response = new GetUserRolegAndRoleResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetUserRoleList = new List<GetUserRoleData>();
                var GetUserGroupList = new List<GetUserGroupData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        var UserRoleDB = await _unitOfWork.UserRoles.FindAllAsync(x => x.UserId == UserID, new[] { "Role", "User" });
                        var UserGroupDB = await _unitOfWork.GroupUsers.FindAllAsync(x => x.UserId == UserID, new[] { "User", "Group" });


                        if (UserRoleDB != null && UserRoleDB.Count() > 0)
                        {

                            foreach (var UserRoleOBJ in UserRoleDB)
                            {
                                var UserRoleOBJResponse = new GetUserRoleData();

                                UserRoleOBJResponse.ID = UserRoleOBJ.Id;

                                UserRoleOBJResponse.RoleName = UserRoleOBJ?.Role.Name;

                                UserRoleOBJResponse.UserName = UserRoleOBJ?.User.FirstName + " " + UserRoleOBJ?.User.MiddleName + " " + UserRoleOBJ.User.LastName;//Common.GetUserName((long)UserRoleOBJ.UserId, _Context);

                                UserRoleOBJResponse.RoleID = UserRoleOBJ.RoleId;


                                GetUserRoleList.Add(UserRoleOBJResponse);
                            }



                        }

                        if (UserGroupDB != null && UserGroupDB.Count() > 0)
                        {

                            foreach (var UserGroupOBJ in UserGroupDB)
                            {
                                var UserGroupOBJResponse = new GetUserGroupData();

                                UserGroupOBJResponse.ID = UserGroupOBJ.Id;

                                UserGroupOBJResponse.GroupName = UserGroupOBJ.Group.Name;//Common.GetGroupName(UserGroupOBJ.GroupId, _Context);

                                UserGroupOBJResponse.UserName = UserGroupOBJ.User.FirstName + " " + UserGroupOBJ?.User.MiddleName + " " + UserGroupOBJ.User.LastName;//Common.GetUserName((long)UserGroupOBJ.UserId, _Context);
                                UserGroupOBJResponse.GroupID = UserGroupOBJ.GroupId;


                                GetUserGroupList.Add(UserGroupOBJResponse);
                            }



                        }
                    }

                }
                response.UserRoleList = GetUserRoleList;
                response.UserGroupList = GetUserGroupList;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public ActionResult<SelectDDLResponse> GetAbsenceTypeList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var List = new List<SelectDDL>();

                if (Response.Result)
                {

                    var ListDB = _unitOfWork.ContractLeaveSetting.GetAll();
                    foreach (var item in ListDB)
                    {
                        var ItemCategoryrObj = new SelectDDL();
                        ItemCategoryrObj.ID = item.Id;
                        ItemCategoryrObj.Name = item.HolidayName;
                        List.Add(ItemCategoryrObj);
                    }


                    Response.DDLList = List;
                }


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<ContractLeaveSettingListRespoonse> GetVacationTypesList()
        {
            ContractLeaveSettingListRespoonse Response = new ContractLeaveSettingListRespoonse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var List = new List<GetContractLeaveSetting>();
                if (Response.Result)
                {

                    var ListDB = _unitOfWork.ContractLeaveSetting.GetAll();
                    foreach (var item in ListDB)
                    {
                        var ItemCategoryrObj = new GetContractLeaveSetting();
                        ItemCategoryrObj.iD = item.Id;
                        ItemCategoryrObj.holidayName = item.HolidayName;
                        ItemCategoryrObj.allowedPerYear = (int)item.BalancePerYear;
                        ItemCategoryrObj.note = item.Notes;
                        List.Add(ItemCategoryrObj);
                    }


                    Response.ContractLeaveSettings = List;
                }


                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<BaseResponseWithId> AddEditVacationType(ContractLeaveSettingRequest request, long userID)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (request.ID != null)
                    {
                        //edit
                        var contractLeave = _unitOfWork.ContractLeaveSetting.GetById((int)request.ID);
                        if (contractLeave != null)
                        {
                            if (request.BalancePerMonth != 0)
                            {
                                contractLeave.BalancePerMonth = request.BalancePerMonth;
                                contractLeave.BalancePerYear = request.BalancePerMonth * 12;
                            }
                            else if (request.BalancePerYear != 0)
                            {
                                contractLeave.BalancePerYear = request.BalancePerYear;
                                contractLeave.BalancePerMonth = request.BalancePerYear / 12;
                            }
                            else if (request.BalancePerYear == 0 && request.BalancePerMonth == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err101";
                                error.ErrorMSG = "Balance per month or balance per year should be added";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            contractLeave.HolidayName = request.HolidayName;
                            contractLeave.Notes = request.Note;
                            contractLeave.Active = true;
                            contractLeave.ModifiedBy = userID;
                            contractLeave.ModifiedDate = DateTime.Now;
                            _unitOfWork.Complete();
                            Response.Result = true;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "This contract leave is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        //insert
                        var contractLeave = new ContractLeaveSetting()
                        {
                            HolidayName = request.HolidayName,
                            Notes = request.Note,
                            Active = true,
                            CreatedBy = userID,
                            CreationDate = DateTime.Now,
                            ModifiedBy = userID,
                            ModifiedDate = DateTime.Now,
                        };
                        if (request.BalancePerMonth != 0)
                        {
                            contractLeave.BalancePerMonth = request.BalancePerMonth;
                            contractLeave.BalancePerYear = request.BalancePerMonth * 12;
                        }
                        else if (request.BalancePerYear != 0)
                        {
                            contractLeave.BalancePerYear = request.BalancePerYear;
                            contractLeave.BalancePerMonth = request.BalancePerYear / 12;
                        }
                        else if (request.BalancePerYear == 0 && request.BalancePerMonth == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Balance per month or balance per year should be added";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        _unitOfWork.ContractLeaveSetting.Add(contractLeave);
                        _unitOfWork.Complete();
                        Response.Result = true;

                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Validation Error";
                    Response.Errors.Add(error);
                    return Response;
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }

        }

        public ActionResult<BaseResponseWithId> EditContractDetails(EditContractDetailModel request, long userID)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (request.Id != 0)
                    {
                        var contract = _unitOfWork.Contracts.GetById(request.Id);
                        if (contract != null)
                        {
                            contract.EndDate = DateTime.Parse(request.EndDate);
                            contract.StartDate = DateTime.Parse(request.StartDate);
                            contract.ContactTypeId = request.ContactTypeID;
                            contract.ProbationPeriod = request.ProbationPeriod;
                            contract.NoticedByCompany = request.NoticedByCompany;
                            contract.NoticedByEmployee = request.NoticedByEmployee;
                            contract.IsAllowOverTime = request.IsAllowOverTime;
                            contract.Isautomatic = request.IsAutomatic;
                            contract.ModifiedDate = DateTime.Now;
                            contract.ModifiedBy = userID;
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "This Contract is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Id must be provided";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<BaseResponseWithId> EditContractEmployeeAbsence(ContractLeaveEmployeeModel contractLeave, long userID)
        {
            BaseResponseWithId Response = new BaseResponseWithId();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var leave = _unitOfWork.ContractLeaveEmployees.GetById(contractLeave.Id ?? 0);
                    if (leave != null)
                    {
                        leave.Balance = contractLeave.Balance;
                        leave.ContractLeaveSettingId = contractLeave.ContractLeaveSettingID;
                        leave.Active = true;
                        leave.LeaveAllowed = contractLeave.LeaveAllowed;
                        leave.Used = contractLeave.Used;
                        leave.Remain = contractLeave.Remain;
                        leave.ModifiedBy = userID;
                        leave.ModifiedDate = DateTime.Now;

                        _unitOfWork.Complete();
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Contract Leave Is Not Found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }

        }

        public ActionResult<GetContractDetailsResponse> GetContractDetails(long userId = 0)
        {
            GetContractDetailsResponse Response = new GetContractDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    /*long userId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["UserId"]) && long.TryParse(Request.Headers["UserId"], out userId))
                    {
                        userId = long.Parse(Request.Headers["UserId"]);
                    }*/
                    var contract = _unitOfWork.Contracts.Find(c => c.UserId == userId && DateTime.Now > c.StartDate && DateTime.Now < c.EndDate);
                    if (contract != null)
                    {
                        Response.Contract = new ContractDetailModel()
                        {
                            Id = contract.Id,
                            UserID = (long)contract.UserId,
                            ContactTypeID = contract.ContactTypeId,
                            StartDate = contract.StartDate.ToString(),
                            EndDate = contract.EndDate.ToString(),
                            ProbationPeriod = (int)contract.ProbationPeriod,
                            NoticedByEmployee = (int)contract.NoticedByEmployee,
                            NoticedByCompany = (int)contract.NoticedByCompany,
                            IsCurrent = contract.IsCurrent,
                            IsAllowOverTime = contract.IsAllowOverTime,
                            IsAutomatic = contract.Isautomatic
                        };
                        var vacationList = _unitOfWork.ContractLeaveEmployees.FindAll(c => c.ContractId == contract.Id);
                        var vacationResponseList = new List<ContractLeaveEmployeeModel>();
                        foreach (var vacation in vacationList)
                        {
                            var vac = new ContractLeaveEmployeeModel()
                            {
                                Id = vacation.Id,
                                ContractLeaveSettingID = vacation.ContractLeaveSettingId,
                                Balance = (int)vacation.Balance,
                                LeaveAllowed = vacation.LeaveAllowed,
                                Remain = (int)vacation.Remain,
                                Used = (int)vacation.Used,
                            };
                            vacationResponseList.Add(vac);
                        }
                        Response.Contract.LeaveEmployees = vacationResponseList;

                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Contract Is Not Found";
                        Response.Errors.Add(error);
                        return Response;
                    }

                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<JobTitlesDDLResponse> GetJobTitlesDDL()
        {
            JobTitlesDDLResponse Response = new JobTitlesDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var JobTitlesList = _unitOfWork.JobTitles.GetAll();
                    var JobTitlesDDL = new List<SelectDDL>();
                    foreach (var J in JobTitlesList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = J.Id;
                        DDLObj.Name = J.Name;

                        JobTitlesDDL.Add(DDLObj);
                    }
                    Response.JobTitlesDDL = JobTitlesDDL;

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        //public ActionResult<BaseResponseWithID> AddAttendanceData(AddEmployeesAttendanceRequest request, long userID)   // Not Used
        //{
        //    BaseResponseWithID Response = new BaseResponseWithID();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {
        //        if (Response.Result)
        //        {

        //            var ErrorsList = new List<Error>();

        //            var SuccessInsertCount = 0;
        //            var SuccessUpdateCount = 0;
        //            var FailedInsertCount = 0;
        //            var FailedUpdateCount = 0;
        //            var TotalRows = 0;
        //            if (request.AttendanceData != null && request.AttendanceData.Any())
        //            {
        //                var IDSEmployeeList = request.AttendanceData.Select(x => x.EmployeeId).ToList();
        //                var UserEmployeeList = _unitOfWork.Users.FindAll(x => IDSEmployeeList.Contains(x.Id)).ToList();
        //                var IDSDepartmentList = request.AttendanceData.Select(x => x.DepartmentId).ToList();
        //                var DepartmentList = _unitOfWork.Departments.FindAll(x => IDSDepartmentList.Contains(x.Id)).ToList();
        //                var Counter = 1;
        //                foreach (var item in request.AttendanceData)

        //                {
        //                    var EmployeeErrorsList = new List<Error>();

        //                    var AttendanceRecord = new AddAttendanceData();

        //                    var NoOfWorkingHours = 0;
        //                    var NoOfWorkingMins = 0;
        //                    var DelayHours = 0;
        //                    var DelayMins = 0;
        //                    var OverTimeHours = 0;
        //                    var OverTimeMins = 0;
        //                    var AbsenceTypeId = 0;

        //                    if (item.EmployeeId != 0)
        //                    {
        //                        // Check User is exist
        //                        if (UserEmployeeList.Where(x => x.Id == item.EmployeeId).FirstOrDefault() == null)
        //                        {
        //                            var Error = new Error();
        //                            Error.ErrorMSG = "Employee ID is not exist! for Counter : " + Counter;
        //                            Error.ErrorCode = Counter.ToString();
        //                            EmployeeErrorsList.Add(Error);
        //                        }
        //                        AttendanceRecord.EmployeeId = item.EmployeeId;
        //                    }
        //                    else
        //                    {
        //                        var Error = new Error();
        //                        Error.ErrorMSG = "Employee ID is required! for Counter : " + Counter;
        //                        Error.ErrorCode = Counter.ToString();
        //                        EmployeeErrorsList.Add(Error);
        //                    }
        //                    if (item.DepartmentId != 0)
        //                    {
        //                        if (DepartmentList.Where(x => x.Id == item.DepartmentId).FirstOrDefault() == null)
        //                        {
        //                            var Error = new Error();
        //                            Error.ErrorMSG = "Department ID is not exist! for Counter : " + Counter;
        //                            Error.ErrorCode = Counter.ToString();
        //                            EmployeeErrorsList.Add(Error);
        //                        }
        //                        AttendanceRecord.DepartmentId = item.DepartmentId;
        //                    }
        //                    else
        //                    {
        //                        var Error = new Error();
        //                        Error.ErrorMSG = "Employee ID is required! for Counter : " + Counter;
        //                        Error.ErrorCode = Counter.ToString();
        //                        EmployeeErrorsList.Add(Error);
        //                    }

        //                    DateTime AttendanceDateConverted = DateTime.Now;
        //                    if (!DateTime.TryParse(item.AttendanceDateSTR, out AttendanceDateConverted))
        //                    {
        //                        var Error = new Error();
        //                        Error.ErrorMSG = "Invalid Attendance Date for Counter : " + Counter;
        //                        Error.ErrorCode = Counter.ToString();
        //                        EmployeeErrorsList.Add(Error);
        //                    }

        //                    if (!string.IsNullOrWhiteSpace(item.AttendanceDateSTR))
        //                    {
        //                        try
        //                        {

        //                            DateTime DayDate = AttendanceDateConverted.Date;


        //                            if (AttendanceDateConverted.Date <= DateTime.Now.Date)
        //                            {
        //                                //error when IsCompleteEmplyeePayslip is NULL (need to be edited according to logic)
        //                                var IsCompleteEmplyeePayslip = _unitOfWork.AttendancePaySlips.Find(a => a.EmployeeUserId == AttendanceRecord.EmployeeId && a.PaySlipDate.Year == DayDate.Year && a.PaySlipDate.Month == DayDate.Month).IsCompleted;

        //                                if (!IsCompleteEmplyeePayslip)
        //                                {
        //                                    AttendanceRecord.AttendanceDate = AttendanceDateConverted;
        //                                }
        //                                else
        //                                {
        //                                    var Error = new Error();
        //                                    Error.ErrorMSG = "You Can't take Attendance for This Employee Because his Payslip for this Month is Completed!!";
        //                                    Error.ErrorCode = Counter.ToString();
        //                                    EmployeeErrorsList.Add(Error);
        //                                }

        //                            }
        //                            else
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "You Can't take Attendance for Upcoming Date!!";
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                            }
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            var Error = new Error();
        //                            Error.ErrorMSG = "Worng Date format!";
        //                            Error.ErrorCode = Counter.ToString();
        //                            EmployeeErrorsList.Add(Error);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var Error = new Error();
        //                        Error.ErrorMSG = "Attendance Date is required! for Counter : " + Counter;
        //                        Error.ErrorCode = Counter.ToString();
        //                        EmployeeErrorsList.Add(Error);
        //                    }

        //                    if (item.AbsenceTypeId != null)
        //                        AttendanceRecord.AbsenceTypeId = item.AbsenceTypeId; //AbsenceTypeId;

        //                    if (AttendanceRecord.AbsenceTypeId == null)
        //                    {
        //                        if (item.CheckInHour != null)
        //                        {
        //                            var CheckInHour = item.CheckInHour;
        //                            if (CheckInHour < 0 || CheckInHour > 24)
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "CheckIn Hour must be between 0 and 24! for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                            }
        //                            else
        //                            {
        //                                AttendanceRecord.CheckInHour = CheckInHour;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var Error = new Error();
        //                            Error.ErrorMSG = "CheckIn Hour is required! for Counter : " + Counter;
        //                            Error.ErrorCode = Counter.ToString();
        //                            EmployeeErrorsList.Add(Error);
        //                        }

        //                        if (item.CheckInMin != null)
        //                        {
        //                            var CheckInMin = item.CheckInMin;
        //                            if (CheckInMin < 0 || CheckInMin > 59)
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "CheckIn Min must be between 0 and 59! for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                            }
        //                            else
        //                            {
        //                                AttendanceRecord.CheckInMin = CheckInMin;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var Error = new Error();
        //                            Error.ErrorMSG = "CheckIn Min is required! for Counter : " + Counter;
        //                            Error.ErrorCode = Counter.ToString();
        //                            EmployeeErrorsList.Add(Error);
        //                        }

        //                        if (item.CheckOutHour != null)
        //                        {
        //                            var CheckOutHour = item.CheckOutHour;
        //                            if (CheckOutHour < 0 || CheckOutHour > 24)
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "CheckOut Hour must be between 0 and 24! for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                            }
        //                            if (CheckOutHour < AttendanceRecord.CheckInHour)
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "Check Out Hour must be Greater than Check In Hour (In 24 Hours Format)! for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                            }
        //                            else
        //                            {
        //                                AttendanceRecord.CheckOutHour = CheckOutHour;
        //                            }
        //                        }
        //                        //else
        //                        //{
        //                        //    var Error = new Error();
        //                        //    Error.ErrorMSG = "CheckOut Hour is required!";
        //                        //    Error.ErrorCode = Counter.ToString();
        //                        //    EmployeeErrorsList.Add(Error);
        //                        //}

        //                        if (item.CheckOutMin != null)
        //                        {
        //                            var CheckOutMin = item.CheckOutMin;
        //                            if (CheckOutMin < 0 || CheckOutMin > 59)
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "CheckOut Min must be between 0 and 59! for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                            }
        //                            else
        //                            {
        //                                AttendanceRecord.CheckOutMin = CheckOutMin;
        //                            }
        //                        }
        //                        //else
        //                        //{
        //                        //    var Error = new Error();
        //                        //    Error.ErrorMSG = "CheckOut Min is required!";
        //                        //    Error.ErrorCode = Counter.ToString();
        //                        //    EmployeeErrorsList.Add(Error);
        //                        //}
        //                        if (AttendanceRecord.CheckInHour != null && AttendanceRecord.CheckInMin != null && AttendanceRecord.CheckOutMin != null && AttendanceRecord.CheckOutHour != null)
        //                        {
        //                            var CheckInTime = new TimeSpan(AttendanceRecord.CheckInHour ?? 0, AttendanceRecord.CheckInMin ?? 0, 0);
        //                            var CheckOutTime = new TimeSpan(AttendanceRecord.CheckOutHour ?? 0, AttendanceRecord.CheckOutMin ?? 0, 0);
        //                            var WorkingTimePeriod = CheckOutTime - CheckInTime;
        //                            NoOfWorkingHours = WorkingTimePeriod.Hours;
        //                            NoOfWorkingMins = WorkingTimePeriod.Minutes;

        //                            var DayOfWeek = AttendanceRecord.AttendanceDate.DayOfWeek.ToString();
        //                            var DayIdDb = _unitOfWork.WeekDays.Find(a => a.Day == DayOfWeek && a.BranchId == ).Id;
        //                            if (DayIdDb != 0)
        //                            {
        //                                var WorkingDay = _unitOfWork.WorkingHours.Find(a => a.Day == DayIdDb);
        //                                if (WorkingDay != null)
        //                                {
        //                                    var IntervalFromTime = new TimeSpan(WorkingDay.IntervalFromHour, WorkingDay.IntervalFromMin, 0);
        //                                    var IntervalToTime = new TimeSpan(WorkingDay.IntervalToHour, WorkingDay.IntervalToMin, 0);

        //                                    if (CheckInTime > IntervalFromTime)
        //                                    {
        //                                        var DelayTime = CheckInTime - IntervalFromTime;
        //                                        DelayHours = DelayTime.Hours;
        //                                        DelayMins = DelayTime.Minutes;
        //                                    }
        //                                    if (CheckOutTime > IntervalToTime)
        //                                    {
        //                                        var OverTime = CheckOutTime - IntervalToTime;
        //                                        OverTimeHours = OverTime.Hours;
        //                                        OverTimeMins = OverTime.Minutes;
        //                                    }
        //                                }
        //                            }
        //                        }

        //                        AttendanceRecord.AbsenceTypeId = null;
        //                    }
        //                    else
        //                    {
        //                        AttendanceRecord.CheckInHour = 0;
        //                        AttendanceRecord.CheckInMin = 0;
        //                        AttendanceRecord.CheckOutHour = 0;
        //                        AttendanceRecord.CheckOutMin = 0;

        //                        AbsenceTypeId = (int)item.AbsenceTypeId;

        //                        int? ExistAbsenceTypeId = _unitOfWork.Attendances.Find(a => a.EmployeeId == AttendanceRecord.EmployeeId && a.AttendanceDate == DateOnly.FromDateTime(AttendanceRecord.AttendanceDate.Date)).AbsenceTypeId;


        //                        if (!UpdateEmployeAbsence(AttendanceRecord.EmployeeId, AbsenceTypeId, ExistAbsenceTypeId))
        //                        {
        //                            var Error = new Error();
        //                            Error.ErrorMSG = "There is no Absence balance For This Employee! for Counter : " + Counter;
        //                            Error.ErrorCode = Counter.ToString();
        //                            EmployeeErrorsList.Add(Error);
        //                        }
        //                        else
        //                        {
        //                            AttendanceRecord.AbsenceTypeId = AbsenceTypeId;
        //                        }
        //                    }

        //                    if (EmployeeErrorsList.Count() > 0)
        //                    {
        //                        FailedInsertCount++;
        //                    }
        //                    else
        //                    {
        //                        var ExistEmpAttendance = _unitOfWork.Attendances.Find(a => a.EmployeeId == AttendanceRecord.EmployeeId && a.AttendanceDate == DateOnly.FromDateTime(AttendanceRecord.AttendanceDate.Date));

        //                        if (ExistEmpAttendance == null)
        //                        {
        //                            var attendance = new Attendance()
        //                            {
        //                                EmployeeId = AttendanceRecord.EmployeeId,
        //                                DepartmentId = item.DepartmentId,
        //                                TeamId = null,
        //                                AttendanceDate = DateOnly.FromDateTime(AttendanceRecord.AttendanceDate.Date),
        //                                CheckInHour = AttendanceRecord.CheckInHour,
        //                                CheckInMin = AttendanceRecord.CheckInMin,
        //                                CheckOutHour = AttendanceRecord.CheckOutHour,
        //                                CheckOutMin = AttendanceRecord.CheckOutMin,
        //                                NoHours = NoOfWorkingHours,
        //                                NoMin = NoOfWorkingMins,
        //                                DelayHours = DelayHours,
        //                                DelayMin = DelayMins,
        //                                OverTimeHour = OverTimeHours,
        //                                OverTimeMin = OverTimeMins,
        //                                AbsenceTypeId = AttendanceRecord.AbsenceTypeId,
        //                                IsApprovedAbsence = AttendanceRecord.AbsenceTypeId != null ? true : false,
        //                                ApprovedByUserId = null,
        //                                CreatedBy = userID,
        //                                CreationDate = DateOnly.FromDateTime(DateTime.Now),
        //                                ModifiedBy = userID,
        //                                ModificationDate = DateOnly.FromDateTime(DateTime.Now),
        //                                Active = true
        //                            };
        //                            /*ObjectParameter AttendanceId = new ObjectParameter("ID", typeof(long));
        //                            var AttendanceInsertion = _Context.proc_AttendanceInsert(AttendanceId,
        //                                AttendanceRecord.EmployeeId,
        //                                item.DepartmentId,
        //                                null,
        //                                AttendanceRecord.AttendanceDate,
        //                                AttendanceRecord.CheckInHour,
        //                                AttendanceRecord.CheckInMin,
        //                                AttendanceRecord.CheckOutHour,
        //                                AttendanceRecord.CheckOutMin,
        //                                NoOfWorkingHours,
        //                                NoOfWorkingMins,
        //                                DelayHours,
        //                                DelayMins,
        //                                OverTimeHours,
        //                                OverTimeMins,
        //                                AttendanceRecord.AbsenceTypeId,
        //                                AttendanceRecord.AbsenceTypeId != null ? true : false,
        //                                null,
        //                                validation.userID, //ExistEmpAttendance.CreatedBy,
        //                                DateTime.Now,
        //                                validation.userID,
        //                                DateTime.Now,
        //                                true
        //                            );*/
        //                            var AttendanceInsertion = _unitOfWork.Attendances.Add(attendance);
        //                            _unitOfWork.Complete();
        //                            if (AttendanceInsertion != null)
        //                            {
        //                                UpdateEmployeeAttendencePayslip(attendance.Id, null, userID);
        //                                SuccessInsertCount++;
        //                            }
        //                            else
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "Error in insertion this record for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                                FailedInsertCount++;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var OldRecord = new BeforeUpdatRecordEmployeeHours()
        //                            {
        //                                AbsenceTypeId = ExistEmpAttendance.AbsenceTypeId,
        //                                CheckInHour = ExistEmpAttendance.CheckInHour ?? 0,
        //                                CheckInMin = ExistEmpAttendance.CheckInMin ?? 0,
        //                                CheckOutHour = ExistEmpAttendance.CheckOutHour ?? 0,
        //                                CheckOutMin = ExistEmpAttendance.CheckOutMin ?? 0,
        //                                DelayHours = ExistEmpAttendance.DelayHours ?? 0,
        //                                DeleyMin = ExistEmpAttendance.DelayMin ?? 0,
        //                                OverTimeHours = ExistEmpAttendance.OverTimeHour ?? 0,
        //                                OverTimeMin = ExistEmpAttendance.OverTimeMin ?? 0
        //                            };
        //                            ExistEmpAttendance = _unitOfWork.Attendances.GetById(ExistEmpAttendance.Id);
        //                            if (ExistEmpAttendance != null)
        //                            {
        //                                ExistEmpAttendance.CheckInHour = AttendanceRecord.CheckInHour;
        //                                ExistEmpAttendance.CheckInMin = AttendanceRecord.CheckInMin;
        //                                ExistEmpAttendance.CheckOutHour = AttendanceRecord.CheckOutHour;
        //                                ExistEmpAttendance.CheckOutMin = AttendanceRecord.CheckOutMin;
        //                                ExistEmpAttendance.NoHours = NoOfWorkingHours;
        //                                ExistEmpAttendance.NoMin = NoOfWorkingMins;
        //                                ExistEmpAttendance.AbsenceTypeId = AttendanceRecord.AbsenceTypeId;
        //                                ExistEmpAttendance.IsApprovedAbsence = true;
        //                                ExistEmpAttendance.ApprovedByUserId = null;
        //                                ExistEmpAttendance.ModificationDate = DateOnly.FromDateTime(DateTime.Now);
        //                                ExistEmpAttendance.ModifiedBy = userID;
        //                                ExistEmpAttendance.Active = true;


        //                            }
        //                            var AttendanceUpdate = _unitOfWork.Complete();
        //                            /*var AttendanceUpdate = _Context.proc_AttendanceUpdate(ExistEmpAttendance.Id,
        //                                                                                    ExistEmpAttendance.EmployeeId,
        //                                                                                    ExistEmpAttendance.DepartmentId,
        //                                                                                    null,
        //                                                                                    ExistEmpAttendance.AttendanceDate,
        //                                                                                    AttendanceRecord.CheckInHour,
        //                                                                                    AttendanceRecord.CheckInMin,
        //                                                                                    AttendanceRecord.CheckOutHour,
        //                                                                                    AttendanceRecord.CheckOutMin,
        //                                                                                    NoOfWorkingHours,
        //                                                                                    NoOfWorkingMins,
        //                                                                                    DelayHours,
        //                                                                                    DelayMins,
        //                                                                                    OverTimeHours,
        //                                                                                    OverTimeMins,
        //                                                                                    AttendanceRecord.AbsenceTypeId,
        //                                                                                    true,
        //                                                                                    null,
        //                                                                                    ExistEmpAttendance.CreatedBy,
        //                                                                                    DateTime.Now,
        //                                                                                    validation.userID,
        //                                                                                    DateTime.Now,
        //                                                                                    true
        //                                                                                );*/

        //                            if (AttendanceUpdate > 0)
        //                            {
        //                                UpdateEmployeeAttendencePayslip(ExistEmpAttendance.Id, OldRecord, userID);
        //                                SuccessUpdateCount++;
        //                            }
        //                            else
        //                            {
        //                                var Error = new Error();
        //                                Error.ErrorMSG = "Error in Update this record for Counter : " + Counter;
        //                                Error.ErrorCode = Counter.ToString();
        //                                EmployeeErrorsList.Add(Error);
        //                                FailedUpdateCount++;
        //                            }
        //                        }
        //                    }
        //                    ErrorsList.AddRange(EmployeeErrorsList);
        //                    Counter++;
        //                }



        //            }
        //            else
        //            {
        //                var Error = new Error();
        //                Error.ErrorMSG = "Null Request, Failed to upload Employees Attendance";
        //                ErrorsList.Add(Error);
        //            }

        //            StringBuilder sb = new StringBuilder();
        //            sb.AppendLine("Success Inserted Rows: " + SuccessInsertCount);
        //            sb.AppendLine("Failed Inserted Rows: " + FailedInsertCount);
        //            sb.AppendLine("Success Updated Rows: " + SuccessUpdateCount);
        //            sb.AppendLine("Failed Updated Rows: " + FailedUpdateCount);
        //            sb.AppendLine("Total Rows: " + TotalRows + "\n");

        //            if (ErrorsList.Count() > 0)
        //            {
        //                var RowNumber = int.Parse(ErrorsList.Select(a => a.ErrorCode).FirstOrDefault());
        //                foreach (Error e in ErrorsList)
        //                {
        //                    if (e.ErrorCode != RowNumber.ToString())
        //                    {
        //                        RowNumber++;
        //                        sb.AppendLine("\n");
        //                    }
        //                    sb.AppendLine("Number of row: " + e.ErrorCode + ", " + "Error message: " + e.ErrorMSG);
        //                    Response.Errors.Add(e);
        //                }
        //            }
        //        }

        //        return Response;
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //}


        public bool UpdateEmployeAbsence(long EmployeeID, int AbsenceTypeID, int? ExistAbsenceType)
        {
            bool Sucessed = false;
            if (EmployeeID != 0 && AbsenceTypeID != 0)
            {
                if (ExistAbsenceType == null) // Insert First Time
                {
                    Sucessed = DeductEmployeAbsence(EmployeeID, AbsenceTypeID, true);
                }
                else // update 
                {
                    if (ExistAbsenceType != 0) // Change from absence  To another  Absence 
                    {
                        // check if have balance on AbsenceTypeID and decrease balance .... then increase balance from old absnece again
                        bool CeckHaveBalanceOnNewAbsnce = DeductEmployeAbsence(EmployeeID, AbsenceTypeID, true);
                        if (CeckHaveBalanceOnNewAbsnce)
                        {
                            Sucessed = DeductEmployeAbsence(EmployeeID, (int)ExistAbsenceType, false);
                        }
                    }
                    else if (ExistAbsenceType == 0)
                    {
                        // employee take this day attend then update to absence .... check if have balance
                        Sucessed = DeductEmployeAbsence(EmployeeID, AbsenceTypeID, true);
                    }
                }


            }

            return Sucessed;
        }


        public bool DeductEmployeAbsence(long EmployeeID, int AbsenceTypeID, bool IsDeduct)
        {
            bool Sucessed = false;
            if (EmployeeID != 0 && AbsenceTypeID != 0)
            {
                var EmployeeContractLeaveDB = _unitOfWork.ContractLeaveEmployees.Find(x => x.UserId == EmployeeID && x.ContractLeaveSettingId == AbsenceTypeID && x.LeaveAllowed == "Yes");
                if (EmployeeContractLeaveDB != null)
                {
                    if (IsDeduct)
                    {
                        if (EmployeeContractLeaveDB.Remain > 0)
                        {
                            EmployeeContractLeaveDB.Used = EmployeeContractLeaveDB.Used + 1;
                            EmployeeContractLeaveDB.Remain = EmployeeContractLeaveDB.Remain - 1;
                            _unitOfWork.Complete();
                            Sucessed = true;
                        }
                    }
                    else
                    {
                        if (EmployeeContractLeaveDB.Used > 0)
                        {
                            EmployeeContractLeaveDB.Used = EmployeeContractLeaveDB.Used - 1;
                            EmployeeContractLeaveDB.Remain = EmployeeContractLeaveDB.Remain + 1;
                            _unitOfWork.Complete();
                            Sucessed = true;
                        }
                    }

                }
            }

            return Sucessed;
        }


        public bool UpdateEmployeeAttendencePayslip(long AttendenceID, BeforeUpdatRecordEmployeeHours OldRecord, long Creator)
        {
            decimal OverTimePerDay = 0;
            decimal DelayTimePerDay = 0;
            bool IsSuccessed = false;
            var UserAttendance = _unitOfWork.Attendances.GetById(AttendenceID);
            // var UserAttendance = _context.proc_AttendanceLoadAll().Where(x => x.Active == true && x.EmployeeID == EmployeeID && x.AttendanceDate == PaySlipDate).FirstOrDefault();
            if (UserAttendance != null)
            {
                long EmployeeID = (long)UserAttendance.EmployeeId;
                bool CheckISAllowOverTime = CheckISAllowOverTimeAutomatic(EmployeeID);
                DateTime PaySlipDate = DateTime.Now;


                // Check if WeekEnd to calculate hours work is overtime + attendance overtime
                // Check if OffDay  to calculate Hour  work is overtime + attendance overtime
                // Check if subtract Attendatce Day with latest payslip date if bigger than offday with week end or absence ... is not completed 

                // Check if allow OverTime
                if (CheckISAllowOverTime)
                {
                    // OffDay or weekend
                    if (CheckISAttendanceDateOFFDayOrWeekEnd(UserAttendance.AttendanceDate.ToDateTime(TimeOnly.Parse("8:00 Am"))))
                    {
                        OverTimePerDay += CalculdateOffDayOverTime(UserAttendance.CheckInHour, UserAttendance.CheckOutHour, UserAttendance.CheckInMin, UserAttendance.CheckOutMin);
                    }
                }


                // 


                var EmployeeAttendancePayslipOBJDB = _unitOfWork.AttendancePaySlips.Find(x => x.EmployeeUserId == EmployeeID && x.CreationDate.Year == PaySlipDate.Year &&
                                                                                                             x.CreationDate.Month == PaySlipDate.Month);
                if (CheckISAllowOverTime)
                {
                    if (UserAttendance.OverTimeHour != null && UserAttendance.OverTimeMin != null)
                    {
                        OverTimePerDay += ((decimal)UserAttendance.OverTimeHour * 60 + (decimal)UserAttendance.OverTimeMin) / 60;
                    }

                    if (UserAttendance.DelayHours != null && UserAttendance.DelayMin != null)
                    {
                        DelayTimePerDay += ((decimal)UserAttendance.DelayHours * 60 + (decimal)UserAttendance.DelayMin) / 60;
                    }
                }
                if (EmployeeAttendancePayslipOBJDB == null) //  Insert First Day in SelectedMonth on payslip
                {
                    if (UserAttendance.AbsenceTypeId == null)
                    {
                        int WorkingDays = CalcWorkingDaysPerMonth(UserAttendance.AttendanceDate.ToDateTime(TimeOnly.Parse("8:00 Am")));
                        var attendancePaySlip = new AttendancePaySlip()
                        {
                            EmployeeUserId = EmployeeID,
                            PaySlipDate = DateTime.Now,
                            Rev = 1,
                            IsCompleted = false,
                            OverTimeSum = OverTimePerDay < 0 ? 0 : OverTimePerDay,
                            DelaySum = DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                            CreationDate = DateTime.Now,
                            CreatedyBy = Creator,
                            ModifiedBy = Creator,
                            ModifiedDate = DateTime.Now,
                            NoOfworkingDays = WorkingDays
                        };
                        var PaySlipInsertion = _unitOfWork.AttendancePaySlips.Add(attendancePaySlip);
                        /*ObjectParameter PaySlipId = new ObjectParameter("ID", typeof(long));
                        var PaySlipInsertion = _Context.proc_AttendancePaySlipInsert(PaySlipId,
                                                                                       EmployeeID,
                                                                                       DateTime.Now,
                                                                                       1,
                                                                                       false,
                                                                                       OverTimePerDay < 0 ? 0 : OverTimePerDay,
                                                                                       DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                                                                                       DateTime.Now,
                                                                                       Creator,
                                                                                       DateTime.Now,
                                                                                       Creator,
                                                                                       WorkingDays);*/
                        if (PaySlipInsertion != null)
                        {
                            IsSuccessed = true;
                        }
                        _unitOfWork.Complete();
                    }

                }// Update
                else
                {
                    if (!EmployeeAttendancePayslipOBJDB.IsCompleted)
                    {
                        if (CheckISAllowOverTime)
                        {

                            // merge OverTime - Delay
                            OverTimePerDay += EmployeeAttendancePayslipOBJDB.OverTimeSum;
                            DelayTimePerDay += EmployeeAttendancePayslipOBJDB.DelaySum;
                        }


                        var LastPayslipDateForThisUser = EmployeeAttendancePayslipOBJDB.PaySlipDate;
                        bool isCompleted = EmployeeAttendancePayslipOBJDB.IsCompleted;
                        int REV = EmployeeAttendancePayslipOBJDB.Rev;
                        if (LastPayslipDateForThisUser.Year == UserAttendance.AttendanceDate.Year && LastPayslipDateForThisUser.Month == UserAttendance.AttendanceDate.Month)
                        {
                            if (EmployeeAttendancePayslipOBJDB.NoOfworkingDays <= EmployeeAttendancePayslipOBJDB.Rev - 1)
                            {
                                isCompleted = false;
                            }
                            if (OldRecord != null)
                            {

                                // Case 1 : Check if old attend time  then update another attend time   => update Overtime and Delay 
                                if ((OldRecord.AbsenceTypeId == null || OldRecord.AbsenceTypeId == 0) && (UserAttendance.AbsenceTypeId == null || UserAttendance.AbsenceTypeId == 0))
                                {
                                    if (CheckISAllowOverTime)
                                    {
                                        decimal OldOverTime = ((decimal)OldRecord.OverTimeHours * 60 + OldRecord.OverTimeMin) / 60;
                                        decimal OldDelayTime = ((decimal)OldRecord.DelayHours * 60 + OldRecord.DeleyMin) / 60;

                                        OverTimePerDay -= OldOverTime;
                                        DelayTimePerDay -= OldDelayTime;
                                    }
                                }
                                // Case 2 : Check if old Attend time then Update to absence       => roll back overTime and Delay time and decrease (Rev)
                                else if ((OldRecord.AbsenceTypeId == null || OldRecord.AbsenceTypeId == 0) && UserAttendance.AbsenceTypeId != null && UserAttendance.AbsenceTypeId != 0)
                                {
                                    if (CheckISAllowOverTime)
                                    {
                                        decimal OldOverTime = ((decimal)OldRecord.OverTimeHours * 60 + OldRecord.OverTimeMin) / 60;
                                        decimal OldDelayTime = ((decimal)OldRecord.DelayHours * 60 + OldRecord.DeleyMin) / 60;

                                        OverTimePerDay -= OldOverTime;
                                        DelayTimePerDay -= OldDelayTime;
                                    }
                                    REV = REV > 0 ? REV - 1 : 0;
                                }
                                // Case 4 : Check if old Absence then Update to attend       =>  update rev
                                else if (OldRecord.AbsenceTypeId != null && OldRecord.AbsenceTypeId != 0 && (UserAttendance.AbsenceTypeId == null || UserAttendance.AbsenceTypeId == 0))
                                {
                                    REV = REV + 1;
                                }
                                // Case 3 : Check if old Absence then Update to another absence       => No update

                                var PaySlipUpdate = _unitOfWork.AttendancePaySlips.GetById(EmployeeAttendancePayslipOBJDB.Id);
                                PaySlipUpdate.EmployeeUserId = EmployeeAttendancePayslipOBJDB.EmployeeUserId;
                                PaySlipUpdate.PaySlipDate = DateTime.Now;
                                PaySlipUpdate.Rev = REV;
                                PaySlipUpdate.IsCompleted = isCompleted;
                                PaySlipUpdate.OverTimeSum = OverTimePerDay < 0 ? 0 : OverTimePerDay;
                                PaySlipUpdate.DelaySum = DelayTimePerDay < 0 ? 0 : DelayTimePerDay;
                                PaySlipUpdate.CreationDate = EmployeeAttendancePayslipOBJDB.CreationDate;
                                PaySlipUpdate.CreatedyBy = EmployeeAttendancePayslipOBJDB.CreatedyBy;
                                PaySlipUpdate.ModifiedDate = DateTime.Now;
                                PaySlipUpdate.CreatedyBy = Creator;
                                PaySlipUpdate.NoOfworkingDays = EmployeeAttendancePayslipOBJDB.NoOfworkingDays;


                                var PaySlipUpdatation = _unitOfWork.Complete();



                                /*var PaySlipUpdatation = _Context.proc_AttendancePaySlipUpdate(EmployeeAttendancePayslipOBJDB.ID,
                                                                                               EmployeeAttendancePayslipOBJDB.EmployeeUserId,
                                                                                               DateTime.Now,
                                                                                               REV,
                                                                                               isCompleted,
                                                                                               OverTimePerDay < 0 ? 0 : OverTimePerDay,
                                                                                               DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                                                                                               EmployeeAttendancePayslipOBJDB.CreationDate,
                                                                                               EmployeeAttendancePayslipOBJDB.CreatedyBy,
                                                                                               DateTime.Now,
                                                                                               Creator,
                                                                                               EmployeeAttendancePayslipOBJDB.NoOfworkingDays);*/
                                if (PaySlipUpdatation > 0)
                                {
                                    IsSuccessed = true;
                                }

                            }
                            else // 
                            // Update Payslip Not Abscence
                            {
                                if (UserAttendance.AbsenceTypeId == null || UserAttendance.AbsenceTypeId == 0)
                                {
                                    var PaySlipUpdate = _unitOfWork.AttendancePaySlips.GetById(EmployeeAttendancePayslipOBJDB.Id);
                                    PaySlipUpdate.EmployeeUserId = EmployeeAttendancePayslipOBJDB.EmployeeUserId;
                                    PaySlipUpdate.PaySlipDate = DateTime.Now;
                                    PaySlipUpdate.Rev = REV + 1;
                                    PaySlipUpdate.IsCompleted = isCompleted;
                                    PaySlipUpdate.OverTimeSum = OverTimePerDay < 0 ? 0 : OverTimePerDay;
                                    PaySlipUpdate.DelaySum = DelayTimePerDay < 0 ? 0 : DelayTimePerDay;
                                    PaySlipUpdate.CreationDate = EmployeeAttendancePayslipOBJDB.CreationDate;
                                    PaySlipUpdate.CreatedyBy = EmployeeAttendancePayslipOBJDB.CreatedyBy;
                                    PaySlipUpdate.ModifiedDate = DateTime.Now;
                                    PaySlipUpdate.CreatedyBy = Creator;
                                    PaySlipUpdate.NoOfworkingDays = EmployeeAttendancePayslipOBJDB.NoOfworkingDays;


                                    var PaySlipUpdatation = _unitOfWork.Complete();


                                    /* var PaySlipUpdatation = _Context.proc_AttendancePaySlipUpdate(EmployeeAttendancePayslipOBJDB.ID,
                                                                EmployeeAttendancePayslipOBJDB.EmployeeUserId,
                                                                DateTime.Now,
                                                                REV + 1,
                                                                isCompleted,
                                                                OverTimePerDay < 0 ? 0 : OverTimePerDay,
                                                                DelayTimePerDay < 0 ? 0 : DelayTimePerDay,
                                                                EmployeeAttendancePayslipOBJDB.CreationDate,
                                                                EmployeeAttendancePayslipOBJDB.CreatedyBy,
                                                                DateTime.Now,
                                                                Creator,
                                                                EmployeeAttendancePayslipOBJDB.NoOfworkingDays);*/
                                    if (PaySlipUpdatation > 0)
                                    {
                                        IsSuccessed = true;
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return IsSuccessed;
        }


        public bool CheckISAllowOverTimeAutomatic(long EmployeeID)
        {
            bool ISAllowOverTimeAutomatic = false;
            var ContractDetailOBJDB = _unitOfWork.Contracts.Find(x => x.UserId == EmployeeID && x.IsAllowOverTime && x.IsCurrent && x.Isautomatic);

            if (ContractDetailOBJDB != null)
            {
                ISAllowOverTimeAutomatic = true;
            }

            return ISAllowOverTimeAutomatic;
        }


        public decimal CalculdateOffDayOverTime(int? CheckHourIn, int? CheckHourOut, int? CheckMinuteIn, int? CheckMinuteOut)
        {
            decimal OverTimeHour = 0;
            CheckHourIn = CheckHourIn != null ? (int)CheckHourIn : 0;
            CheckHourOut = CheckHourOut != null ? (int)CheckHourOut : 0;
            CheckMinuteIn = CheckMinuteIn != null ? (int)CheckMinuteIn : 0;
            CheckMinuteOut = CheckMinuteOut != null ? (int)CheckMinuteOut : 0;

            decimal CheckInTime = ((decimal)CheckHourIn * 60 + (decimal)CheckMinuteIn) / 60;
            decimal CheckOutTime = ((decimal)CheckHourOut * 60 + (decimal)CheckMinuteOut) / 60;
            if (CheckOutTime > CheckInTime)
            {
                OverTimeHour = CheckOutTime - CheckInTime;
            }

            return OverTimeHour;
        }

        public int CalcWorkingDaysPerMonth(DateTime AttendanceDate)
        {
            int WorkingDays = 0;
            int NOOfDaysPerMonth = DateTime.DaysInMonth(AttendanceDate.Year, AttendanceDate.Month);
            for (int Counter = 1; Counter <= NOOfDaysPerMonth; Counter++)
            {
                DateTime PayslipDatePerDay = new DateTime(AttendanceDate.Year, AttendanceDate.Month, AttendanceDate.Day);
                if (!CheckISAttendanceDateOFFDayOrWeekEnd(PayslipDatePerDay))
                {
                    WorkingDays++;
                }
            }
            return WorkingDays;
        }

        public bool CheckISAttendanceDateOFFDayOrWeekEnd(DateTime AttendanceDate)
        {
            bool ISAttendanceDateOFFDayOrWeekEnd = false;
            var OffDay = _unitOfWork.OffDays.Find(x => x.Day.Year == AttendanceDate.Year && x.Day.Month == AttendanceDate.Month
                                                                  && x.Day.Day == AttendanceDate.Day && x.Active == true && x.AllowWorking == true);

            var WeekEnd = _unitOfWork.WeekEnds.Find(x => x.Day.ToLower() == AttendanceDate.DayOfWeek.ToString().ToLower());

            if (OffDay != null || WeekEnd != null)
            {
                ISAttendanceDateOFFDayOrWeekEnd = true;
            }

            return ISAttendanceDateOFFDayOrWeekEnd;
        }

        public async Task<ActionResult<UserAttendanceListResponse>> GetEmployeeAttendence(GetEmployeeAttendenceHeader header)
        {
            UserAttendanceListResponse Response = new UserAttendanceListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    /*long EmployeeId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["EmployeeId"]) && long.TryParse(Request.Headers["EmployeeId"], out EmployeeId))
                    {
                        EmployeeId = long.Parse(Request.Headers["EmployeeId"]);
                    }
                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }*/
                    /*DateTime fromDate = DateTime.Now;
                    DateTime toDate = DateTime.Now;*/
                    var UserAttendanceList = new List<UserAttendance>();
                    var AttendanceByDateList = _unitOfWork.Attendances.FindAllQueryable(a => a.EmployeeId == header.EmployeeId).AsQueryable();
                    var DayName = DateTime.Now.ToString("dddd");
                    /*if (!string.IsNullOrEmpty(Request.Headers["fromDate"]) && DateTime.TryParse(Request.Headers["fromDate"], out fromDate) && !string.IsNullOrEmpty(Request.Headers["toDate"]) && DateTime.TryParse(Request.Headers["toDate"], out toDate))
                    {
                        fromDate = DateTime.Parse(Request.Headers["fromDate"]);
                        toDate = DateTime.Parse(Request.Headers["ToDate"]);
                        AttendanceByDateList = AttendanceByDateList.Where(a => a.AttendanceDate >= DateOnly.FromDateTime(fromDate) && a.AttendanceDate <= DateOnly.FromDateTime(toDate)).AsQueryable();
                    }*/
                    AttendanceByDateList = AttendanceByDateList.Where(a => a.AttendanceDate >= DateOnly.FromDateTime(header.fromDate) && a.AttendanceDate <= DateOnly.FromDateTime(header.toDate)).AsQueryable();
                    var list = PagedList<Attendance>.Create(AttendanceByDateList.OrderBy(a => a.CreationDate), header.CurrentPage, header.NumberOfItemsPerPage);
                    if (AttendanceByDateList.Count() > 0)
                    {
                        foreach (var item in list.ToList())
                        {
                            var dayname = item.AttendanceDate.ToString("dddd").ToLower();
                            var employee = _unitOfWork.Users.Find(u => u.Id == item.EmployeeId);
                            var attendance = new UserAttendance()
                            {
                                UserName = employee.FirstName + " " + employee.LastName,
                                UserID = employee.Id,
                                DepartmentId = employee.DepartmentId,
                                DepartmentName = employee.Department?.Name,
                                TeamList = _unitOfWork.UserTeams.FindAll(x => x.UserId == item.Id).Select(UT => new TeamModel { TeamId = UT.TeamId, TeamName = UT.Team.Name }).ToList(),
                                AttendanceId = item.Id,
                                CheckInHour = item.CheckInHour,
                                CheckOutHour = item.CheckOutHour,
                                CheckInMin = item.CheckInMin,
                                CheckOutMin = item.CheckOutMin,
                                AbsenceTypeId = item.AbsenceTypeId,
                                AbsenceTypeName = item.AbsenceType?.HolidayName,
                                IsApprovedAbsence = item.IsApprovedAbsence,
                                IsOffDay = _unitOfWork.OffDays.Find(x => x.Active == true && (DateOnly.FromDateTime(x.Day) == item.AttendanceDate || x.WeekEndDay.Trim().ToLower() == dayname)) != null ? true : false,
                                OffDayName = _unitOfWork.OffDays.Find(x => x.Active == true && (DateOnly.FromDateTime(x.Day) == item.AttendanceDate || x.WeekEndDay.Trim().ToLower() == dayname)) != null ? _unitOfWork.OffDays.Find(x => x.Active == true && (DateOnly.FromDateTime(x.Day) == item.AttendanceDate || x.WeekEndDay.Trim().ToLower() == dayname)).Holiday.Name : null,

                            };
                            UserAttendanceList.Add(attendance);
                        }
                        Response.UserAttendanceList = UserAttendanceList;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "this employee doesn't have attendence";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<ActionResult<UserAttendanceListResponse>> GetUserAttendanceList(GetUserAttendanceListHeader header)
        {
            UserAttendanceListResponse Response = new UserAttendanceListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                /*DateTime AttendanceDate = DateTime.Now;

                if (DateTime.TryParse(Request.Headers["AttendanceDate"], out AttendanceDate))
                {
                    AttendanceDate = DateTime.Parse(Request.Headers["AttendanceDate"]);
                }
                bool notAttended = false;
                if (!string.IsNullOrEmpty(Request.Headers["notAttended"]) && bool.TryParse(Request.Headers["notAttended"], out notAttended))
                {
                    notAttended = bool.Parse(Request.Headers["notAttended"]);
                }
                long EmployeeId = 0;
                if (!string.IsNullOrEmpty(Request.Headers["EmployeeId"]) && long.TryParse(Request.Headers["EmployeeId"], out EmployeeId))
                {
                    EmployeeId = long.Parse(Request.Headers["EmployeeId"]);
                }
                int CurrentPage = 1;
                if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                {
                    int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                }

                int NumberOfItemsPerPage = 10;
                if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                {
                    int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                }*/
                var DayName = DateTime.Now.ToString("dddd");
                var UserAttendanceList = new List<UserAttendance>();
                if (Response.Result)
                {
                    var OffDaysList = await _unitOfWork.OffDays.FindAllAsync(x => x.Active == true);
                    var ListDBQuery = _unitOfWork.Users.FindAllQueryable(x => x.Active == true, new[] { "Department" }).AsQueryable();
                    var AttendanceByDateList = _unitOfWork.Attendances.FindAllQueryable(a => true);
                    if (header.EmployeeId != 0)
                    {
                        ListDBQuery = ListDBQuery.Where(x => x.Id == header.EmployeeId).AsQueryable();
                        DateTime fromDate = DateTime.Now;
                        DateTime toDate = DateTime.Now;
                        if (header.fromDate != DateTime.MinValue && header.toDate != DateTime.MinValue)
                        {
                            /*fromDate = DateTime.Parse(Request.Headers["fromDate"]);
                            toDate = DateTime.Parse(Request.Headers["ToDate"]);*/
                            AttendanceByDateList = _unitOfWork.Attendances.FindAllQueryable(a => a.AttendanceDate >= DateOnly.FromDateTime(fromDate) && a.AttendanceDate <= DateOnly.FromDateTime(toDate)).AsQueryable();
                        }
                        else
                        {
                            AttendanceByDateList = _unitOfWork.Attendances.FindAllQueryable(a => a.AttendanceDate == DateOnly.FromDateTime(header.AttendanceDate)).AsQueryable();
                        }

                    }
                    else
                    {
                        AttendanceByDateList = _unitOfWork.Attendances.FindAllQueryable(a => a.AttendanceDate == DateOnly.FromDateTime(header.AttendanceDate)).AsQueryable();
                    }
                    OffDaysList = OffDaysList.Where(x => x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).ToList();
                    var ListIDS = ListDBQuery.Select(x => x.Id).AsQueryable();
                    var UserTeamsList = _unitOfWork.UserTeams.FindAllQueryable(UT => ListIDS.Contains((long)UT.UserId));
                    /*                    AttendanceByDateList = await _Context.Attendances.Where(a => DbFunctions.TruncateTime(a.AttendanceDate) == AttendanceDate).ToListAsync();
                    */

                    if (header.notAttended)
                    {
                        var Attendedids = AttendanceByDateList.Select(x => x.EmployeeId).AsQueryable();
                        ListDBQuery = ListDBQuery.Where(x => !Attendedids.Contains(x.Id)).AsQueryable();
                    }

                    var OffDay = await _unitOfWork.OffDays.FindAsync(x => x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower());

                    bool IsOffDay = false;
                    string OffDayName = null;
                    string HolidayName = null;
                    if (OffDay != null)
                    {
                        IsOffDay = true;
                        OffDayName = OffDay.Holiday.Name;
                    }
                    var UserListPagination = PagedList<User>.Create(ListDBQuery.OrderBy(x => x.FirstName), header.CurrentPage, header.NumberOfItemsPerPage);
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = header.CurrentPage,
                        TotalPages = UserListPagination.TotalPages,
                        ItemsPerPage = header.NumberOfItemsPerPage,
                        TotalItems = UserListPagination.TotalCount
                    };
                    foreach (var item in UserListPagination)
                    {
                        var AttendanceByUser = AttendanceByDateList.Where(x => x.EmployeeId == item.Id).ToList();
                        var Obj = new List<UserAttendance>();
                        /*                        Obj.UserName = item.FirstName + " " + item.LastName;
                                                Obj.UserID = item.ID;
                                                Obj.DepartmentId = item.DepartmentID;
                                                Obj.DepartmentName = item.Department?.Name;*/

                        //Obj.TeamList = UserTeamsList.Where(x => x.UserID == item.ID).Select(UT => new TeamModel { TeamId = UT.TeamID, TeamName = UT.Team.Name }).ToList();
                        // Attendance
                        if (AttendanceByUser != null && AttendanceByUser.Count > 0)
                        {
                            Obj = AttendanceByUser.Select(a => new UserAttendance()
                            {
                                UserName = item.FirstName + " " + item.LastName,
                                UserID = item.Id,
                                DepartmentId = item.DepartmentId,
                                DepartmentName = item.Department?.Name,
                                TeamList = UserTeamsList.Where(x => x.UserId == item.Id).Select(UT => new TeamModel { TeamId = UT.TeamId, TeamName = UT.Team.Name }).ToList(),
                                AttendanceId = a.Id,
                                CheckInHour = a.CheckInHour,
                                CheckOutHour = a.CheckOutHour,
                                CheckInMin = a.CheckInMin,
                                CheckOutMin = a.CheckOutMin,
                                AbsenceTypeId = a.AbsenceTypeId,
                                AbsenceTypeName = a.AbsenceType?.HolidayName,
                                IsApprovedAbsence = a.IsApprovedAbsence,
                                IsOffDay = OffDaysList.Where(x => DateOnly.FromDateTime(x.Day.Date) == a.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).FirstOrDefault() != null ? true : false,
                                OffDayName = OffDaysList.Where(x => DateOnly.FromDateTime(x.Day.Date) == a.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).FirstOrDefault() != null ? OffDaysList.Where(x => DateOnly.FromDateTime(x.Day.Date) == a.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).FirstOrDefault().Holiday.Name : null,
                            }).ToList();


                        }
                        else
                        {
                            Obj.Add(new UserAttendance()
                            {
                                UserName = item.FirstName + " " + item.LastName,
                                UserID = item.Id,
                                DepartmentId = item.DepartmentId,
                                DepartmentName = item.Department?.Name,
                                TeamList = UserTeamsList.Where(x => x.UserId == item.Id).Select(UT => new TeamModel { TeamId = UT.TeamId, TeamName = UT.Team.Name }).ToList(),
                                IsOffDay = OffDaysList.Where(x => x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).FirstOrDefault() != null ? true : false,
                                OffDayName = OffDaysList.Where(x => x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).FirstOrDefault() != null ? OffDaysList.Where(x => x.Day.Date == header.AttendanceDate || x.WeekEndDay.Trim().ToLower() == DayName.ToLower()).FirstOrDefault().Holiday.Name : null,
                            });
                        }
                        // check If This Off Day
                        /*         Obj.IsOffDay = IsOffDay;
                                 Obj.OffDayName = OffDayName;*/

                        UserAttendanceList.AddRange(Obj.AsEnumerable());
                    }
                }
                Response.UserAttendanceList = UserAttendanceList.ToList();
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public ActionResult<GetAbsenceDetailsResponse> GetEmployeeAbsenceDetails(long UserId = 0)
        {
            GetAbsenceDetailsResponse Response = new GetAbsenceDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    /*long UserId = 0;
                    if (string.IsNullOrEmpty(Request.Headers["UserId"]) || !long.TryParse(Request.Headers["UserId"], out UserId))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "InventoryItemID Is required";
                        Response.Errors.Add(error);
                    }*/
                    if (Response.Result)
                    {
                        var AbsenceDetails = new List<AbsenceDetailsViewModel>();
                        var details = _unitOfWork.ContractLeaveEmployees.FindAll(x => x.UserId == UserId);

                        if (details != null)
                        {
                            foreach (var item in details)
                            {
                                var absence = new AbsenceDetailsViewModel();
                                var holiday = _unitOfWork.ContractLeaveSetting.Find(x => x.Id == item.ContractLeaveSettingId);
                                absence.ContractLeaveSettingID = holiday.Id;
                                absence.HolidayName = holiday.HolidayName;
                                absence.leaveAllowed = item.LeaveAllowed;
                                absence.balance = (int)item.Balance;
                                absence.remain = (int)item.Remain;
                                absence.used = (int)item.Used;
                                AbsenceDetails.Add(absence);
                            }
                            Response.Data = AbsenceDetails;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Inventory ItemID";
                            Response.Errors.Add(error);
                        }

                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithId<long>> CreateHrUserWorker(AddHrUserWorker Worker, long UserId)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    #region validation
                    if (string.IsNullOrEmpty(Worker.FirstName))
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Please, enter first name";
                        response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(Worker.LastName))
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Please, enter last name";
                        response.Errors.Add(error);
                    }
                    #endregion

                    var newHrUser = new HrUser()
                    {
                        FirstName = Worker.FirstName,
                        LastName = Worker.LastName,
                        MiddleName = string.IsNullOrEmpty(Worker.MiddleName) == false ? Worker.MiddleName : string.Empty,
                        //Mobile = "0",
                        Email = "@",
                        ArlastName = Worker.LastName,
                        Active = true,
                        CreatedById = UserId,
                        CreationDate = DateTime.Now,
                        ModifiedById = UserId,
                        Modified = DateTime.Now,
                        IsUser = false
                    };

                    var newHruser = _unitOfWork.HrUsers.Add(newHrUser);
                    _unitOfWork.Complete();

                    response.ID = newHrUser.Id;
                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }


    }
}
