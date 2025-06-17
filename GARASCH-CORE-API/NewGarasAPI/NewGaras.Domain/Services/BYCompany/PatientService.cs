using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Helper;
using System.Web;
using System.Security.Policy;

namespace NewGaras.Domain.Services.BYCompany
{
    public class PatientService : IPatientService
    {
        private readonly IMapper _mapper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        private readonly string key;

        public PatientService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            key = "SalesGarasPass";
        }
        public BaseResponseWithID AddNewPatient([FromForm] PatientDto request, long creator, string CompanyName)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region checkInDB
                    if (request.CountryId != null)
                    {

                        var country = _unitOfWork.Countries.FindAll(a => a.Id == request.CountryId).FirstOrDefault();
                        if (country == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No Country with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (request.CityId != null)
                    {

                        var city = _unitOfWork.Governorates.FindAll(a => a.Id == request.CityId).FirstOrDefault();
                        if (city == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No city with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (request.AreaId != null)
                    {
                        var area = _unitOfWork.Areas.FindAll(a => a.Id == request.AreaId).FirstOrDefault();
                        if (area == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No area with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    #endregion

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrWhiteSpace(request.FirstName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your FirstName";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrWhiteSpace(request.LastName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your LastName";
                        Response.Errors.Add(error);
                    }

                    //if (string.IsNullOrWhiteSpace(request.Email))
                    //{

                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P7";
                    //    error.ErrorMSG = "please write your Email";
                    //    Response.Errors.Add(error);
                    //}

                    if (string.IsNullOrWhiteSpace(request.Mobile))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your Mobile";
                        Response.Errors.Add(error);
                    }
                    if (request.Insurances != null && request.Insurances?.Count > 0)
                    {
                        var insurances = _unitOfWork.UserPatientInsurances.GetAll().ToList();
                        foreach (var a in request.Insurances)
                        {
                            if (insurances.Where(x => x.IncuranceNo == a.IncuranceNo).ToList().Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P8";
                                error.ErrorMSG = "the insurance number of " + (request.Insurances.IndexOf(a) + 1) + " is already added";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    string MiddleName = "";
                    if (!string.IsNullOrWhiteSpace(request.MiddleName))
                    {
                        MiddleName = request.MiddleName.Trim().ToLower();
                    }
                    if (_unitOfWork.Users.FindAll(a => a.FirstName.Trim().ToLower() == request.FirstName.Trim().ToLower() && a.MiddleName.Trim().ToLower() == MiddleName && a.LastName.Trim().ToLower() == request.LastName.Trim().ToLower()).FirstOrDefault() != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P9";
                        error.ErrorMSG = "This Full Name is Already in our database";
                        Response.Errors.Add(error);
                    }
                    if (_unitOfWork.Users.FindAll(a => a.Mobile.Trim() == request.Mobile.Trim()).FirstOrDefault() != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P10";
                        error.ErrorMSG = "This Mobile Number is Already in our database";
                        Response.Errors.Add(error);
                    }
                    //if (_Context.Users.Where(a => a.Email.Trim().ToLower() == request.Email.Trim().ToLower()).FirstOrDefault() != null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P10";
                    //    error.ErrorMSG = "This Email is Already in our database";
                    //    Response.Errors.Add(error);
                    //}

                    if (Response.Result)
                    {

                        var user = new NewGaras.Infrastructure.Entities.User()
                        {
                            FirstName = request.FirstName,
                            LastName = request.LastName,
                            MiddleName = request.MiddleName,
                            Email = request.Email,
                            Mobile = request.Mobile,
                            Password = Encrypt_Decrypt.Encrypt(key, key),
                            Active = false,
                            CreatedBy = creator,
                            ModifiedBy = creator,
                            CreationDate = DateTime.Now,
                            Modified = DateTime.Now,
                            Gender = request.Gender,
                        };
                        var addedUser = _unitOfWork.Users.Add(user);
                        _unitOfWork.Complete();
                        byte[] EmployeePhoto = null;
                        //if (string.IsNullOrEmpty(Request.Photo)){
                        if (request.Photo != null)
                        {
                            var fileExtension = request.Photo.FileName.Split('.').Last();
                            var virtualPath = $"Attachments\\{CompanyName}\\UserPatient\\{addedUser.Id}\\";
                            var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Photo.FileName);
                            addedUser.PhotoUrl = Common.SaveFileIFF(virtualPath, request.Photo, FileName, fileExtension, _host);
                            _unitOfWork.Complete();
                        }

                        DateTime? DOB = null;
                        DateTime ToDateTemp = DateTime.Now;
                        if (request.DateOfBirth != null)
                        {
                            DOB = request.DateOfBirth;
                        }
                        var patient = new UserPatient()
                        {
                            UserId = addedUser.Id,
                            DateOfBirth = DOB,
                            Active = true,
                            CreatedBy = creator,
                            ModifiedBy = creator,
                            CreationDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            CountryId = request.CountryId,
                            CityId = request.CityId,
                            AreaId = request.AreaId,
                            Address = request.Address,
                            Description = request.Description
                        };
                        var addedPatient = _unitOfWork.UserPatients.Add(patient);
                        _unitOfWork.Complete();
                        var insurances = new List<UserPatientInsurance>();
                        if (request.Insurances != null && request.Insurances.Count > 0)
                        {
                            insurances = request.Insurances.Select(a => new UserPatientInsurance() { IncuranceNo = a.IncuranceNo, UserPatientId = addedPatient.Id, ExpireDate = a.ExpireDate, Name = a.Name, CreationDate = DateTime.Now, ModifiedDate = DateTime.Now, CreationBy = creator, ModifiedBy = creator }).ToList();
                            _Context.UserPatientInsurances.AddRange(insurances);
                            _Context.SaveChanges();
                        }
                        Response.ID = addedPatient.Id;

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

        public BaseResponseWithID EditPatient([FromForm] PatientDto request, long creator, string CompanyName)

        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region checkInDB
                    if (request.CountryId != null)
                    {
                        var country = _unitOfWork.Countries.FindAll(a => a.Id == request.CountryId).FirstOrDefault();
                        if (country == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No Country with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (request.CityId != null)
                    {
                        var city = _unitOfWork.Governorates.FindAll(a => a.Id == request.CityId).FirstOrDefault();
                        if (city == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No city with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (request.AreaId != null)
                    {
                        var area = _unitOfWork.Areas.FindAll(a => a.Id == request.AreaId).FirstOrDefault();
                        if (area == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "No area with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    #endregion
                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.PatientId == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Patient Id Is Required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrWhiteSpace(request.FirstName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your FirstName";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrWhiteSpace(request.LastName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your LastName";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrWhiteSpace(request.Email))
                    {

                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P7";
                        error.ErrorMSG = "please write your Email";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrWhiteSpace(request.Mobile))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P8";
                        error.ErrorMSG = "please write your Mobile";
                        Response.Errors.Add(error);
                    }
                    DateTime CreationDate = DateTime.Now;
                    if (request.CreationDate != null)
                    {
                        CreationDate = (DateTime)request.CreationDate;
                    }

                    if (Response.Result)
                    {
                        var patient = _unitOfWork.UserPatients.GetById(request.PatientId ?? 0);
                        if (patient != null)
                        {
                            DateTime? DOB = null;
                            DateTime ToDateTemp = DateTime.Now;
                            if (request.DateOfBirth != null)
                            {
                                DOB = request.DateOfBirth;
                            }
                            if (request.Active != null)
                            {
                                patient.Active = (bool)request.Active;

                            }
                            patient.DateOfBirth = DOB;
                            patient.ModifiedBy = creator;
                            patient.CreationDate = CreationDate;
                            patient.ModifiedDate = DateTime.Now;
                            if (request.CountryId != null) patient.CountryId = request.CountryId;
                            if (request.CityId != null) patient.CityId = request.CityId;
                            if (request.AreaId != null) patient.AreaId = request.AreaId;
                            if (!string.IsNullOrWhiteSpace(request.Address)) patient.Address = request.Address;
                            if (!string.IsNullOrWhiteSpace(request.Description)) patient.Description = request.Description;
                            _unitOfWork.Complete();
                            var user = _unitOfWork.Users.GetById(patient.UserId);

                            user.FirstName = request.FirstName;
                            user.LastName = request.LastName;
                            user.MiddleName = request.MiddleName;
                            user.Email = request.Email;
                            user.Mobile = request.Mobile;
                            user.CreationDate = CreationDate;
                            user.ModifiedBy = creator;
                            user.Modified = DateTime.Now;
                            user.Gender = request.Gender;
                            if (request.Photo != null)
                            {
                                if (user.PhotoUrl != null && user.PhotoUrl != "")
                                {
                                    string FilePath = Path.Combine(_host.WebRootPath, user.PhotoUrl);
                                    if (System.IO.File.Exists(FilePath))
                                    {
                                        System.IO.File.Delete(FilePath);
                                    }
                                }
                                //System.IO.File.Delete(HrUser.ImgPath);
                                var fileExtension = request.Photo.FileName.Split('.').Last();
                                var virtualPath = $"Attachments\\{CompanyName}\\UserPatient\\{user.Id}\\";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Photo.FileName);
                                user.PhotoUrl = Common.SaveFileIFF(virtualPath, request.Photo, FileName, fileExtension, _host);
                            }

                            _unitOfWork.Complete();
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P8";
                            error.ErrorMSG = "Patient not found";
                            Response.Errors.Add(error);
                        }
                        var insurances = new List<UserPatientInsurance>();
                        if (request.Insurances != null && request.Insurances.Count > 0)
                        {
                            foreach (var a in request.Insurances)
                            {
                                if (a.Id != null)
                                {
                                    var insurance = _unitOfWork.UserPatientInsurances.GetById(a.Id ?? 0);

                                    if (insurance != null)
                                    {
                                        if (a.Active == false)
                                        {
                                            _unitOfWork.UserPatientInsurances.Delete(insurance);
                                            _unitOfWork.Complete();
                                        }
                                        else
                                        {
                                            insurance.IncuranceNo = a.IncuranceNo;
                                            insurance.Name = a.Name;
                                            insurance.ExpireDate = a.ExpireDate;
                                            insurance.ModifiedBy = creator;
                                            insurance.ModifiedDate = DateTime.Now;
                                            _unitOfWork.Complete();
                                        }
                                    }
                                }
                                else
                                {
                                    if (a.Active == true)
                                    {
                                        var insurance = new UserPatientInsurance() { IncuranceNo = a.IncuranceNo, UserPatientId = (long)request.PatientId, ExpireDate = a.ExpireDate, Name = a.Name, CreationDate = DateTime.Now, ModifiedDate = DateTime.Now, CreationBy = creator, ModifiedBy = creator };
                                        _Context.UserPatientInsurances.Add(insurance);
                                        _Context.SaveChanges();
                                    }
                                }
                            }

                            _Context.SaveChanges();
                        }
                        Response.ID = patient.Id;

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

        public BaseResponseWithData<GetPatientDetailsDto> GetPatientById([FromHeader] long patientId)

        {
            BaseResponseWithData<GetPatientDetailsDto> Response = new BaseResponseWithData<GetPatientDetailsDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (patientId != 0)
                    {
                        var dbPatient = _unitOfWork.UserPatients.FindAll(a => a.Id == patientId, includes: new[] { "UserPatientInsurances", "City", "Area", "Country" }).FirstOrDefault();
                        if (dbPatient != null)
                        {
                            var user = _unitOfWork.Users.GetById(dbPatient.UserId);
                            var patient = new GetPatientDetailsDto();
                            if (user is not null)
                            {
                                patient = new GetPatientDetailsDto()
                                {
                                    Id = dbPatient.Id,
                                    DateOfBirth = (DateTime)dbPatient.DateOfBirth,
                                    FirstName = user.FirstName,
                                    LastName = user.LastName,
                                    MiddleName = user.MiddleName,
                                    Gender = user.Gender,
                                    Email = user.Email,
                                    Mobile = user.Mobile,
                                    Photo = Globals.baseURL + '/' + user.PhotoUrl,
                                    Insurance = dbPatient.UserPatientInsurances.Select(a => new InsuranceDto() { Name = a.Name, IncuranceNo = a.IncuranceNo, ExpireDate = a.ExpireDate, UserPatientId = a.UserPatientId, Id = a.Id }).ToList(),
                                    userId = user.Id,
                                    countryid = dbPatient.CountryId,
                                    countryName = dbPatient?.Country?.Name,
                                    cityid = dbPatient.CityId,
                                    cityName = dbPatient?.City?.Name,
                                    AreaId = dbPatient.AreaId,
                                    AreaName = dbPatient?.Area?.Name,
                                    Address = dbPatient?.Address,
                                    Description = dbPatient?.Description,
                                    CreationDate = dbPatient?.CreationDate,
                                };
                            }
                            Response.Data = patient;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Patient Id Is Incorrect.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Patient Id.";
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithDataAndHeader<List<GetPatientDto>> GetPatients([FromHeader] string firstname, [FromHeader] string lastname, [FromHeader] DateTime? DOB, [FromHeader] string phone, [FromHeader] string IncuranceNo, [FromHeader] bool GetAll, [FromHeader] int CurrentPage = 1, [FromHeader] int PageSize = 1000)
        {
            BaseResponseWithDataAndHeader<List<GetPatientDto>> Response = new BaseResponseWithDataAndHeader<List<GetPatientDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var patients = new List<GetPatientDto>();
                    var usersIds = new List<long>();
                    var users = _unitOfWork.Users.GetAsQueryable();

                    if (firstname != null || lastname != null || DOB != null || phone != null || IncuranceNo != null || GetAll == true)
                    {
                        if (!string.IsNullOrEmpty(firstname))
                        {
                            //firstname = Encoding.UTF8.GetString(Convert.FromBase64String(firstname));
                            firstname = HttpUtility.UrlDecode(firstname);
                        }
                        if (!string.IsNullOrEmpty(lastname))
                        {
                            //lastname = Encoding.UTF8.GetString(Convert.FromBase64String(lastname));
                            firstname = HttpUtility.UrlDecode(lastname);
                        }
                        var dbPatients = _unitOfWork.UserPatients.FindAllQueryable(a => true, includes: new[] { "UserPatientInsurances", "Country", "City", "Area" }).AsQueryable();
                        if (!string.IsNullOrEmpty(firstname))
                        {
                            users = users.Where(a => a.FirstName.Trim().Contains(firstname.Trim())).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(lastname))
                        {
                            users = users.Where(a => a.LastName.Trim().Contains(lastname.Trim())).AsQueryable();
                        }
                        if (phone != null)
                        {
                            users = users.Where(a => a.Mobile.Trim().Contains(phone.Trim())).AsQueryable();
                        }

                        usersIds = users.ToList().Select(a => a.Id).ToList();


                        dbPatients = dbPatients.Where(a => usersIds.Contains(a.UserId)).AsQueryable();

                        if (DOB != null)
                        {
                            dbPatients = dbPatients.Where(a => ((DateTime)a.DateOfBirth).Date == ((DateTime)DOB).Date).AsQueryable();
                        }
                        if (IncuranceNo != null)
                        {
                            dbPatients = dbPatients.Where(a => a.UserPatientInsurances.Where(b => b.IncuranceNo.Trim() == IncuranceNo.Trim()).Count() > 0).AsQueryable();
                        }
                        if (dbPatients.Count() > 0)
                        {
                            patients = dbPatients.ToList().Select(
                                a => new GetPatientDto()
                                {
                                    Id = a.Id,
                                    FirstName = users.Where(x => x.Id == a.UserId).FirstOrDefault()?.FirstName,
                                    LastName = users.Where(x => x.Id == a.UserId).FirstOrDefault()?.LastName,
                                    Email = users.Where(x => x.Id == a.UserId).FirstOrDefault()?.Email,
                                    Mobile = users.Where(x => x.Id == a.UserId).FirstOrDefault()?.Mobile,
                                    userId = a.UserId,
                                    DateOfBirth = (DateTime)a.DateOfBirth,
                                    Photo = users.Where(x => x.Id == a.UserId).FirstOrDefault().PhotoUrl != null ? Globals.baseURL + '/' + users.Where(x => x.Id == a.UserId).FirstOrDefault().PhotoUrl : null,
                                    Insurance = a.UserPatientInsurances.Count > 0 ? new InsuranceDto() { Id = a.UserPatientInsurances.FirstOrDefault()?.Id, UserPatientId = a.Id, IncuranceNo = a.UserPatientInsurances.FirstOrDefault()?.IncuranceNo, Name = a.UserPatientInsurances.FirstOrDefault()?.Name } : null,
                                    countryid = a.CountryId,
                                    countryName = a?.Country?.Name,
                                    cityid = a.CityId,
                                    cityName = a?.City?.Name,
                                    AreaId = a.AreaId,
                                    AreaName = a?.Area?.Name,
                                    Address = a?.Address,
                                    Description = a?.Description,
                                    CreationDate = a?.CreationDate
                                }).ToList();
                            //patients = PagedList<GetPatientDto>.Create(patients.AsQueryable(), CurrentPage, PageSize);
                            var test = PagedList<GetPatientDto>.Create(patients.AsQueryable(), CurrentPage, PageSize);
                            patients = test;

                            Response.PaginationHeader = new PaginationHeader
                            {
                                CurrentPage = CurrentPage,
                                TotalPages = test.TotalPages,
                                ItemsPerPage = PageSize,
                                TotalItems = test.TotalCount
                            };
                        }
                    }



                    Response.Data = patients;
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

        public BaseResponseWithData<List<InsuranceDto>> GetUserInsurances([FromHeader] long patientId)
        {
            BaseResponseWithData<List<InsuranceDto>> Response = new BaseResponseWithData<List<InsuranceDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ins = new List<InsuranceDto>();
                if (Response.Result)
                {
                    if (patientId != 0)
                    {
                        var insurances = _unitOfWork.UserPatientInsurances.FindAll(a => a.UserPatientId == patientId).ToList();
                        ins = insurances.Select(a => new InsuranceDto() { Id = a.Id, UserPatientId = a.UserPatientId, Name = a.Name, IncuranceNo = a.IncuranceNo }).ToList();
                        Response.Data = ins;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "please send Patient Id";
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<GetPatientsDDl>> GetUserPatients()
        {
            BaseResponseWithData<List<GetPatientsDDl>> Response = new BaseResponseWithData<List<GetPatientsDDl>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var pat = new List<GetPatientsDDl>();
                if (Response.Result)
                {
                    var patients = _unitOfWork.UserPatients.GetAll();
                    var users = _unitOfWork.Users.FindAll(a => patients.Select(x => x.UserId).ToList().Contains(a.Id)).ToList();


                    pat = patients.Select(a => new GetPatientsDDl()
                    {
                        PatientId = a.Id,
                        UserId = a.UserId,
                        Name = users.Where(x => x.Id == a.UserId).Select(z => z.FirstName + " " + z.LastName).FirstOrDefault(),

                    }).ToList();
                    Response.Data = pat;
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
    }
}
