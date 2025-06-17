using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGaras.Infrastructure.Models.InternalTicket;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.SalesOffer.InternalTicket;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NewGarasAPI.Models.AccountAndFinance;
using NewGaras.Infrastructure.DTO.HrUser;
using AutoMapper;


namespace NewGaras.Domain.Services.Medical
{
    public class MedicalService : IMedicalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private HearderVaidatorOutput validation;
        private readonly IWebHostEnvironment _host;
        private readonly IMapper _mapper;
        private readonly string key;
        private readonly IHrUserService _hrUserService;
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
        public MedicalService(IUnitOfWork unitOfWork,IWebHostEnvironment host, IHrUserService hrUserService)
        {
            _unitOfWork = unitOfWork;
            _host = host;
            key = "SalesGarasPass";
            _hrUserService = hrUserService;
        }

        public BaseResponseWithId<long> AddNewPatient(NewPatientDto patient)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                if(patient == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Invalid Data";
                    response.Errors.Add(err);
                    return response;
                }

                if (patient.Id != null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E109";
                    err.ErrorMSG = "Id Can't have a value";
                    response.Errors.Add(err);
                    return response;
                }

                if (string.IsNullOrWhiteSpace(patient.Name))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.ErrorMSG = "Name Is Mendatory";
                    response.Errors.Add(err);
                    return response;
                }

                if (string.IsNullOrWhiteSpace(patient.Mobile))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.ErrorMSG = "Mobile Is Mendatory";
                    response.Errors.Add(err);
                    return response;
                }

                if (string.IsNullOrWhiteSpace(patient.NationalId))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E104";
                    err.ErrorMSG = "NationalId Is Mendatory";
                    response.Errors.Add(err);
                    return response;
                }

                if (patient.SalesPersonId != null)
                {
                    var salesPerson = _unitOfWork.Users.GetById(patient.SalesPersonId??0);
                    if (salesPerson == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E105";
                        err.ErrorMSG = "sales person not found";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                var checkIdExist = _unitOfWork.Clients.FindAll(a=>a.TaxCard.ToLower().Trim()==patient.NationalId.ToLower().Trim()).Any();
                if(checkIdExist)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E107";
                    err.ErrorMSG = "National Id Already Exist";
                    response.Errors.Add(err);
                    return response;
                }

                var newPatient = new Client()
                {
                    Name = patient.Name,
                    Type = "Individual",
                    SalesPersonId = patient.SalesPersonId ?? validation.userID,
                    TaxCard = patient.NationalId,
                    PreparedSearchName = Common.string_compare_prepare_function(patient.Name),
                    CreatedBy = validation.userID,
                    CreationDate = DateTime.Now,
                };
                _unitOfWork.Clients.Add(newPatient);
                var res = _unitOfWork.Complete();

                if (res > 0)
                {
                    var checkMobileExist = _unitOfWork.ClientMobiles.FindAll(a=>a.Mobile.Trim()==patient.Mobile.Trim()).Any();
                    if (checkMobileExist)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E108";
                        err.ErrorMSG = "Mobile Already Exist";
                        response.Errors.Add(err);
                        return response;
                    }
                    var newMobile = new ClientMobile()
                    {
                        ClientId = newPatient.Id,
                        Mobile = patient.Mobile,
                        CreatedBy = validation.userID,
                        CreationDate = DateTime.Now,
                        ModifiedBy = validation.userID,
                        Active = true,
                        Modified = DateTime.Now
                    };
                    _unitOfWork.ClientMobiles.Add(newMobile);
                    _unitOfWork.Complete();
                }
                else
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E106";
                    err.ErrorMSG = "Error Occured";
                    response.Errors.Add(err);
                    return response;
                }

                response.ID = newPatient.Id;
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }


        public BaseResponseWithId<long> EditPatient(NewPatientDto patient)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                if (patient == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Invalid Data";
                    response.Errors.Add(err);
                    return response;
                }

                if (patient.Id == null || patient.Id==0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E109";
                    err.ErrorMSG = "Id Can't be null";
                    response.Errors.Add(err);
                    return response;
                }

                var Patient = _unitOfWork.Clients.GetById(patient.Id??0);
                if(Patient== null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E110";
                    err.ErrorMSG = "Patient Not Found";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrWhiteSpace(patient.Name))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.ErrorMSG = "Name Is Mendatory";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrWhiteSpace(patient.Mobile))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E103";
                    err.ErrorMSG = "Mobile Is Mendatory";
                    response.Errors.Add(err);
                    return response;
                }
                if (string.IsNullOrWhiteSpace(patient.NationalId))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E104";
                    err.ErrorMSG = "NationalId Is Mendatory";
                    response.Errors.Add(err);
                    return response;
                }

                if (patient.SalesPersonId != null)
                {
                    var salesPerson = _unitOfWork.Users.GetById(patient.SalesPersonId ?? 0);
                    if (salesPerson == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E105";
                        err.ErrorMSG = "sales person not found";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                var checkIdExist = _unitOfWork.Clients.FindAll(a => a.TaxCard.ToLower().Trim() == patient.NationalId.ToLower().Trim() && a.Id!=patient.Id).Any();
                if (checkIdExist)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E107";
                    err.ErrorMSG = "National Id Already Exist";
                    response.Errors.Add(err);
                    return response;
                }

                Patient.Name = patient.Name;
                Patient.PreparedSearchName = Common.string_compare_prepare_function(patient.Name);
                Patient.SalesPersonId = patient.SalesPersonId ?? validation.userID;
                Patient.TaxCard = patient.NationalId;
                
                
                _unitOfWork.Clients.Update(Patient);
                var res = _unitOfWork.Complete();

                if (res > 0)
                {
                    var checkMobileExist = _unitOfWork.ClientMobiles.FindAll(a => a.Mobile.Trim() == patient.Mobile.Trim() && a.ClientId!=patient.Id).Any();
                    if (checkMobileExist)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E108";
                        err.ErrorMSG = "Mobile Already Exist";
                        response.Errors.Add(err);
                        return response;
                    }
                    var mobile = _unitOfWork.ClientMobiles.FindAll(a => a.ClientId == patient.Id).FirstOrDefault();
                    if (mobile != null)
                    {
                        mobile.Mobile = patient.Mobile;
                        mobile.Modified = DateTime.Now;
                        mobile.ModifiedBy = validation.userID;
                        mobile.Active = true;
                        _unitOfWork.ClientMobiles.Update(mobile);
                        _unitOfWork.Complete();
                    }
                    else
                    {
                        var newMobile = new ClientMobile()
                        {
                            ClientId = patient.Id??0,
                            Mobile = patient.Mobile,
                            CreatedBy = validation.userID,
                            CreationDate = DateTime.Now,
                            ModifiedBy = validation.userID,
                            Modified = DateTime.Now,
                            Active = true,
                        };
                        _unitOfWork.ClientMobiles.Add(newMobile);
                        _unitOfWork.Complete();
                    }
                    
                }
                else
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E106";
                    err.ErrorMSG = "Error Occured";
                    response.Errors.Add(err);
                    return response;
                }

                response.ID = patient.Id ?? 0;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<GetPatientModel> GetPatient([FromHeader] long PatientId)
        {
            BaseResponseWithData<GetPatientModel> response = new BaseResponseWithData<GetPatientModel>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if(PatientId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Patient Id Is Required";
                    response.Errors.Add(err);
                    return response;
                }

                var Patient = _unitOfWork.Clients.FindAll(a => a.Id == PatientId, includes: new[] { "SalesPerson", "ClientMobiles" }).FirstOrDefault();
                if (Patient == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.ErrorMSG = "Patient not found";
                    response.Errors.Add(err);
                    return response;
                }
                var GetPatient = new GetPatientModel()
                {
                    Id = Patient.Id,
                    Name = Patient.Name,
                    NationalId = Patient.TaxCard,
                    SalesPersonId = Patient.SalesPersonId,
                    SalesPersonName = Patient.SalesPerson != null ? Patient.SalesPerson.FirstName + " " + Patient.SalesPerson.LastName : null,
                    Mobile = Patient.ClientMobiles.Select(a => a.Mobile).FirstOrDefault()
                };
                response.Data = GetPatient;
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        } 

        public BaseResponseWithData<GetPatientsListModel> GetPatientsList([FromHeader] string SearchKey, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage=10)
        {
            BaseResponseWithData<GetPatientsListModel> response = new BaseResponseWithData<GetPatientsListModel>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var AllPatients = _unitOfWork.Clients.FindAllQueryable(a => true, includes: new[] { "SalesPerson", "ClientMobiles" }).AsQueryable();
                if (!string.IsNullOrWhiteSpace(SearchKey))
                {
                    var key = HttpUtility.UrlDecode(SearchKey);
                    AllPatients = AllPatients.Where(a=>a.PreparedSearchName.Contains(key) || a.TaxCard.Contains(key) || a.ClientMobiles.Select(x=>x.Mobile).FirstOrDefault().Contains(key)).AsQueryable();
                }

                var List = PagedList<Client>.Create(AllPatients, CurrentPage, NumberOfItemsPerPage);
                var ReturnedList = List.Select(a=>new GetPatientModel() { Id=a.Id, Name=a.Name, NationalId = a.TaxCard,SalesPersonId = a.SalesPersonId,SalesPersonName = a.SalesPerson.FirstName+" "+a.SalesPerson.LastName,Mobile = a.ClientMobiles.Select(x=>x.Mobile).FirstOrDefault()}).ToList();

                response.Data = new GetPatientsListModel();
                response.Data.Patients = ReturnedList;
                response.Data.paginationHeader = new PaginationHeader()
                {
                    CurrentPage = List.CurrentPage,
                    ItemsPerPage = List.PageSize,
                    TotalItems = List.TotalCount,
                    TotalPages = List.TotalPages
                };


                return response;
            }
            catch( Exception ex )
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E10";
                err.ErrorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithData<List<TopData>> GetTopDoctors(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType,string OfferTypeReturn)
        {
            BaseResponseWithData<List<TopData>> response = new BaseResponseWithData<List<TopData>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (day != 0)
                {
                    if (month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "you have to enter a month if you want to filter with a day";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (month != 0)
                {
                    if (year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "you have to enter a Year if you want to filter with a Month";
                        response.Errors.Add(error);
                        return response;
                    }
                }

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains(OfferType), includes: new[] { "SalesPerson.Department" });

                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == year).AsQueryable();
                }
                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == month).AsQueryable();
                }
                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == day).AsQueryable();
                }
                if (From != null && To != null)
                {
                    salesOffers = salesOffers.Where(a => a.OfferType.Contains(OfferType) && a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
                }
                if (!string.IsNullOrWhiteSpace(CreatorId))
                {
                    List<long> numbers = CreatorId.Split(',').Select(s => s.Trim()).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                    salesOffers = salesOffers.Where(a => numbers.Contains(a.CreatedBy)).AsQueryable();
                }
                var byDoctors = salesOffers.ToList().GroupBy(a => a.SalesPerson.FirstName + " " + (a.SalesPerson.MiddleName != null ? a.SalesPerson.MiddleName + " " : "") + a.SalesPerson.LastName).Select(a => new TopData()
                {
                    Name = a.Key,
                    Count = a.Count(),
                    Sum = a.Where(x => x.OfferType == OfferType).Sum(x => x.FinalOfferPrice ?? 0) - a.Where(x => x.OfferType == OfferTypeReturn).Sum(x => x.FinalOfferPrice ?? 0),
                    Img = a.FirstOrDefault()?.SalesPerson?.PhotoUrl != null ? Globals.baseURL + a.FirstOrDefault()?.SalesPerson?.PhotoUrl : null
                }).ToList();

                byDoctors = byDoctors.OrderByDescending(a => a.Sum).ToList();

                response.Data = byDoctors;

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


        public BaseResponseWithData<List<TopData>> GetTopDepartmentsOrCategories(int year, int month, int day, string type, string OfferType, string OfferTypeReturn)
        {
            BaseResponseWithData<List<TopData>> response = new BaseResponseWithData<List<TopData>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (day != 0)
                {
                    if (month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "you have to enter a month if you want to filter with a day";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (month != 0)
                {
                    if (year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "you have to enter a Year if you want to filter with a Month";
                        response.Errors.Add(error);
                        return response;
                    }
                }

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains(OfferType), includes: new[] { "SalesPerson.Department" });
                var salesOffersV = _unitOfWork.VSalesOffers.FindAllQueryable(a => a.OfferType.Contains(OfferType));
                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == year).AsQueryable();
                    salesOffersV = salesOffersV.Where(a => a.CreationDate.Year == year).AsQueryable();
                }
                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == month).AsQueryable();
                    salesOffersV = salesOffersV.Where(a => a.CreationDate.Month == month).AsQueryable();
                }
                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == day).AsQueryable();
                    salesOffersV = salesOffersV.Where(a => a.CreationDate.Day == day).AsQueryable();
                }
                if (type == null || type.ToLower() != "category")
                {
                    var byDepartments = salesOffers.ToList().GroupBy(a => a.SalesPerson?.Department?.Name).Select(a => new TopData()
                    {
                        Name = a.Key,
                        Count = a.Count(),
                        Sum = a.Where(x => x.OfferType == OfferType).Sum(x => x.FinalOfferPrice ?? 0) - a.Where(x => x.OfferType == OfferTypeReturn).Sum(x => x.FinalOfferPrice ?? 0),
                    }).ToList();

                    byDepartments = byDepartments.OrderByDescending(a => a.Sum).ToList();

                    response.Data = byDepartments;
                }
                else
                {
                    var byCategories = salesOffersV.GroupBy(a => a.CategoryName).Select(a => new TopData()
                    {
                        Name = a.Key,
                        Count = a.Count(),
                        Sum = a.Where(x => x.OfferType == OfferType).Sum(x => x.FinalOfferPrice ?? 0) - a.Where(x => x.OfferType == OfferTypeReturn).Sum(x => x.FinalOfferPrice ?? 0),
                    }).ToList();

                    byCategories = byCategories.OrderByDescending(a => a.Sum).ToList();
                    response.Data = byCategories;
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


        public BaseResponseWithData<InternalTicketDashboardSummary> GetMedicalDashboard(int year, int month, int day, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn)
        {
            BaseResponseWithData<InternalTicketDashboardSummary> response = new BaseResponseWithData<InternalTicketDashboardSummary>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var date = DateTime.Now;

                if (day != 0)
                {
                    date = new DateTime(date.Year, date.Month, day);
                }
                if (month != 0)
                {
                    date = new DateTime(date.Year, month, date.Day);
                }
                if (year != 0)
                {
                    date = new DateTime(year, date.Month, date.Day);
                }
                /*if (day != 0)
                {
                    if (month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "you have to enter a month if you want to filter with a day";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (month != 0)
                {
                    if (year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "you have to enter a Year if you want to filter with a Month";
                        response.Errors.Add(error);
                        return response;
                    }
                }*/

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains(OfferType), includes: new[] { "SalesPerson.Department" });
                response.Data = new InternalTicketDashboardSummary();
                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == date.Year).AsQueryable();

                    response.Data.Year = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == OfferType).Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType ==OfferTypeReturn).Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
                }

                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == date.Month).AsQueryable();

                    response.Data.Month = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == OfferType).Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == OfferTypeReturn).Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
                }

                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == date.Day).AsQueryable();

                    response.Data.Day = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == OfferType).Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == OfferTypeReturn).Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
                }
                if (From != null && To != null)
                {
                    salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains(OfferType) && a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
                    response.Data.Duration = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == OfferType).Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == OfferTypeReturn).Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
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

        public BaseResponseWithData<GetInternalTicketsByDepartmentResponse> GetMedicalByTeams(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn)
        {
            BaseResponseWithData<GetInternalTicketsByDepartmentResponse> response = new BaseResponseWithData<GetInternalTicketsByDepartmentResponse>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (day != 0)
                {
                    if (month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "you have to enter a month if you want to filter with a day";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (month != 0)
                {
                    if (year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "you have to enter a Year if you want to filter with a Month";
                        response.Errors.Add(error);
                        return response;
                    }
                }

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType == OfferType || a.OfferType == OfferTypeReturn, includes: new[] { "SalesPerson.HrUserUsers.Team", "Client" });

                if (!string.IsNullOrWhiteSpace(CreatorId))
                {
                    List<long> numbers = CreatorId.Split(',').Select(s => s.Trim()).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                    salesOffers = salesOffers.Where(a => numbers.Contains(a.CreatedBy)).AsQueryable();
                }

                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == year).AsQueryable();
                }
                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == month).AsQueryable();
                }
                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == day).AsQueryable();
                }
                if (From != null && To != null)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
                }
                response.Data = new GetInternalTicketsByDepartmentResponse();

                var list = salesOffers.ToList();

                var departments = list.GroupBy(a => a.SalesPerson.HrUserUsers?.FirstOrDefault()?.Team?.Name).ToList();

                var data = departments.Select(a => new InternalTicketDepartments()
                {
                    Name = a.Key,
                    Count = a.Count(),
                    Sum = a.Where(b => b.OfferType == OfferType).Sum(b => b.FinalOfferPrice ?? 0) - a.Where(b => b.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice ?? 0),
                    GroupedById = a.FirstOrDefault()?.SalesPerson?.Department?.Id ?? 0,
                    DoctorsOfDepartmentList = a.GroupBy(x => x.SalesPerson).Select(x => new DoctorsOfCriteria()
                    {
                        DoctorId = x.Key.Id,
                        DoctorName = x.Key.FirstName + " " + (x.Key.MiddleName != null ? x.Key.MiddleName + " " : "") + x.Key.LastName,
                        DoctorImg = x.Key.PhotoUrl != null ? Globals.baseURL + x.Key.PhotoUrl : null,
                        TotalSum = x.Where(a => a.OfferType == OfferType).Sum(b => b.FinalOfferPrice ?? 0) - x.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice ?? 0),
                        TicketsCount = x.Count(),
                        PatientsCount = x.Select(b => b.Client).Distinct().Count()
                    }).ToList()
                }).ToList();

                response.Data.InternalTicketDepartmentList = data;

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


        public BaseResponseWithData<GetInternalTicketsByCategoryResponse> GetMedicalByCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn)
        {
            BaseResponseWithData<GetInternalTicketsByCategoryResponse> response = new BaseResponseWithData<GetInternalTicketsByCategoryResponse>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (day != 0)
                {
                    if (month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "you have to enter a month if you want to filter with a day";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (month != 0)
                {
                    if (year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "you have to enter a Year if you want to filter with a Month";
                        response.Errors.Add(error);
                        return response;
                    }
                }

                var salesOffers = _unitOfWork.VSalesOffers.FindAllQueryable(a => a.OfferType == OfferType || a.OfferType == OfferTypeReturn);



                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == year).AsQueryable();
                }
                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == month).AsQueryable();
                }
                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == day).AsQueryable();
                }
                if (From != null && To != null)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
                }
                if (!string.IsNullOrWhiteSpace(CreatorId))
                {
                    List<long> numbers = CreatorId.Split(',').Select(s => s.Trim()).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                    var offers = _unitOfWork.SalesOffers.FindAll(a => numbers.Contains(a.CreatedBy)).Select(a => a.Id).ToList();

                    salesOffers = salesOffers.Where(a => offers.Contains(a.Id)).AsQueryable();
                }
                response.Data = new GetInternalTicketsByCategoryResponse();

                var list = salesOffers.ToList();

                var categories = list.GroupBy(a => a.CategoryName).ToList();

                var data = categories.Select(a => new InternalTicketCategories()
                {
                    Name = a.Key,
                    Count = a.Count(),
                    Sum = a.Where(b => b.OfferType == OfferType).Sum(b => b.FinalOfferPrice ?? 0) - a.Where(b => b.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice ?? 0),
                    GroupedById = a.FirstOrDefault()?.InventoryItemCategoryId ?? 0,
                    DoctorsOfDepartmentList = a.GroupBy(x => x.SalesPersonFullName).Select(x => new DoctorsOfCriteria()
                    {
                        DoctorId = x.FirstOrDefault().SalesPersonId,
                        DoctorName = x.Key,
                        DoctorImg = x.FirstOrDefault()?.SalesPersonPhoto != null ? Globals.baseURL + x.FirstOrDefault()?.SalesPersonPhoto : null,
                        TotalSum = x.Where(a => a.OfferType == OfferType).Sum(b => b.FinalOfferPrice ?? 0) - x.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice ?? 0),
                        TicketsCount = x.Count(),
                        PatientsCount = x.Select(a => a.ClientId).Distinct().Count()
                    }).ToList()
                }).ToList();

                response.Data.InternalTicketDepartmentList = data;

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


        public BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> GetMedicalByItemCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To, string OfferType, string OfferTypeReturn)
        {
            BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> response = new BaseResponseWithData<GetInternalTicketsByItemCategoryResponse>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (day != 0)
                {
                    if (month == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "you have to enter a month if you want to filter with a day";
                        response.Errors.Add(error);
                        return response;
                    }
                }
                if (month != 0)
                {
                    if (year == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "you have to enter a Year if you want to filter with a Month";
                        response.Errors.Add(error);
                        return response;
                    }
                }

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType == OfferType || a.OfferType == OfferTypeReturn, includes: new[] { "SalesPerson" });



                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == year).AsQueryable();
                }
                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == month).AsQueryable();
                }
                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == day).AsQueryable();
                }
                if (From != null && To != null)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
                }
                if (!string.IsNullOrWhiteSpace(CreatorId))
                {
                    List<long> numbers = CreatorId.Split(',').Select(s => s.Trim()).Where(s => long.TryParse(s, out _)).Select(long.Parse).ToList();
                    salesOffers = salesOffers.Where(a => numbers.Contains(a.CreatedBy)).AsQueryable();
                }
                response.Data = new GetInternalTicketsByItemCategoryResponse();

                var offersIds = salesOffers.Select(a => a.Id).ToList();
                var list = _unitOfWork.VSalesOfferProducts.FindAll(a => a.OfferId != null && offersIds.Contains(a.OfferId ?? 0)).ToList();

                var categories = list.GroupBy(a => a.InventoryItemId).ToList();

                var data = categories.Select(a =>
                {
                    var offers = salesOffers.Where(x => a.Select(y => y.OfferId).Contains(x.Id)).ToList();
                    return new InternalTicketItemCategories()
                    {
                        Name = a.FirstOrDefault()?.InventoryItemName,
                        Count = a.Count(),
                        Sum = offers.Where(b => b.OfferType == OfferType).Sum(b => b.FinalOfferPrice ?? 0) - offers.Where(b => b.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice ?? 0),
                        GroupedById = a.FirstOrDefault()?.InventoryItemId ?? 0,
                        DoctorsOfDepartmentList = offers.GroupBy(x => x.SalesPerson).Select(x => new DoctorsOfCriteria()
                        {
                            DoctorId = x.FirstOrDefault().SalesPersonId,
                            DoctorName = x.Key.FirstName + " " + (x.Key.MiddleName != null ? x.Key.MiddleName + " " : "") + x.Key.LastName,
                            DoctorImg = x.Key.PhotoUrl != null ? Globals.baseURL + x.Key.PhotoUrl : null,
                            TotalSum = x.Where(a => a.OfferType == OfferType).Sum(b => b.FinalOfferPrice ?? 0) - x.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice ?? 0),
                            TicketsCount = x.Count(),
                            PatientsCount = x.Select(a => a.ClientId).Distinct().Count()
                        }).ToList()
                    };
                }).ToList();

                response.Data.InternalTicketDepartmentList = data;

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

        public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForMedical(GetSalesOfferInternalTicketListFilters filters, string OfferStatusParam)
        {
            GetSalesOfferListForInternalTicketResponse Response = new GetSalesOfferListForInternalTicketResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {

                    if (!string.IsNullOrEmpty(filters.OfferType))
                    {
                        filters.OfferType = filters.OfferType.ToLower();
                    }


                    if (!string.IsNullOrEmpty(filters.ClientName))
                    {
                        filters.ClientName = HttpUtility.UrlDecode(filters.ClientName).ToLower();
                    }

                    if (!string.IsNullOrEmpty(OfferStatusParam))
                    {
                        if (OfferStatusParam.ToLower() != "all")
                        {
                            filters.OfferStatus = OfferStatusParam.ToLower();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(filters.OfferStatus))
                            {
                                filters.OfferStatus = filters.OfferStatus.ToLower();
                            }
                        }
                    }

                    var StartDate = filters.From ?? DateTime.Now;
                    var EndDate = filters.To ?? DateTime.Now;
                    var DateFilter = false;

                    if (filters.From != null)
                    {
                        DateFilter = true;
                        StartDate = (DateTime)filters.From;

                        if (filters.To != null)
                        {
                            EndDate = (DateTime)filters.To;
                        }
                    }
                    else
                    {
                        if (filters.To != null)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "You have to Enter Offer From Date!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                    }

                    if (Response.Result)
                    {
                        var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "SalesPerson", "ClientAccounts", "Client", "CreatedByNavigation", "SalesOfferProducts" }).AsQueryable();
                        //var test1 = SalesOfferDBQuery.ToList();

                        if (filters.SalesOfferClassifiction == "POS")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "POS" || a.OfferType == "Sales Return").AsQueryable();
                        }
                        else if (filters.SalesOfferClassifiction == "InternalTicket")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "Internal Ticket return" || a.OfferType == "Internal Ticket").AsQueryable();
                        }
                        else if (filters.SalesOfferClassifiction == "ExtClinics")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "ExtClinics" || a.OfferType == "ExtClinics Return").AsQueryable();
                        }
                        if (filters.CreatorUserId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.CreatedBy == filters.CreatorUserId).AsQueryable();
                        }
                        if (filters.DepartmentId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.SalesPerson.DepartmentId == filters.DepartmentId).AsQueryable();
                        }
                        if (filters.CategoryId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.SalesOfferProducts.Any(a => a.InventoryItemCategoryId == filters.CategoryId)).AsQueryable();
                        }
                        // supplier Name , Offer Serial ,Project Name
                        if (!string.IsNullOrEmpty(filters.SearchKey))
                        {
                            filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
                                                       (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.Id.ToString() == filters.SearchKey)
                                                    || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false))
                                                    //|| SalesOfferIDS.Contains(x.ID)
                                                    ).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.OfferType))
                        {
                            if (filters.OfferType == "new project offer")
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == "new project offer" || a.OfferType.ToLower() == "direct sales").AsQueryable();
                            else
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == filters.OfferType).AsQueryable();
                        }

                        if (filters.SalesPersonId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ClientName))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filters.ClientName)).AsQueryable();
                        }

                        if (DateFilter)
                        {
                            if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= StartDate && a.ClientApprovalDate <= EndDate).AsQueryable();
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime(StartDate) && a.EndDate <= DateOnly.FromDateTime(EndDate)).AsQueryable();
                            }
                        }
                        else
                        {
                            if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                            {
                                var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                            }
                        }


                        if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate).OrderByDescending(a => a.CreationDate);
                        }
                        else
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
                        }


                        var OffersListDB = PagedList<SalesOffer>.Create(SalesOfferDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);
                        var TotalOffersCount = OffersListDB.TotalCount;
                        //var TotalOffersPriceWithReturned = SalesOfferDBQuery.Sum(a => a.FinalOfferPrice) ?? 0;
                        decimal TotalOffersPriceWithoutReturn = 0;
                        decimal TotalOffersReturnedPrice = 0;
                        if (filters.SalesOfferClassifiction.Contains("InternalTicket"))
                        {
                            TotalOffersPriceWithoutReturn = SalesOfferDBQuery.Where(x => x.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice) ?? 0;

                            TotalOffersReturnedPrice = SalesOfferDBQuery.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice) ?? 0;
                        }
                        else if(filters.SalesOfferClassifiction == "ExtClinics")
                        {
                            TotalOffersPriceWithoutReturn = SalesOfferDBQuery.Where(x => x.OfferType == "ExtClinics").Sum(a => a.FinalOfferPrice) ?? 0;

                            TotalOffersReturnedPrice = SalesOfferDBQuery.Where(a => a.OfferType == "ExtClinics return").Sum(a => a.FinalOfferPrice) ?? 0;
                        }
                        var TotalOffersPrice = TotalOffersPriceWithoutReturn - TotalOffersReturnedPrice;
                        SalesOfferForInternalTicket salesOfferTypeDetails = new SalesOfferForInternalTicket()
                        {
                            TotalOffersCount = TotalOffersCount,
                            TotalOffersPrice = TotalOffersPrice
                        };

                        var OffersListResponse = new List<SalesOfferDetailsForInternalTicket>();
                        var IDSSalesOffer = OffersListDB.Select(x => x.Id).ToList();
                        var ParentSalesOfferListDB = _unitOfWork.InvoiceCnandDns.FindAll((x => IDSSalesOffer.Contains(x.SalesOfferId) || IDSSalesOffer.Contains(x.ParentSalesOfferId)), new[] { "SalesOffer", "ParentSalesOffer" }).ToList();
                        
                        var medicalReservationListDB = _unitOfWork.MedicalReservations.FindAll(x => IDSSalesOffer.Contains(x.OfferId));
                        foreach (var offer in OffersListDB)
                        {
                            long? projectId = null;
                            decimal QTYOfMatrialReleaseItem = 0;
                            double QTYOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.RemainQty ?? 0) ?? 0;
                            var SalesOfferProductCount = offer.SalesOfferProducts?.Count ?? 0;
                            if (offer.Projects.Count > 0)
                            {
                                var offerProject = offer.Projects.FirstOrDefault();
                                projectId = offerProject.Id;
                                if (offerProject.InventoryMatrialRequestItems.Count > 0)
                                {
                                    QTYOfMatrialReleaseItem = offerProject.InventoryMatrialRequestItems?.Sum(x => x.RecivedQuantity1 ?? 0) ?? 0;
                                }
                            }
                            long? ParentSalesOfferId = null;
                            string ParentSalesOfferSErial = "";
                            var ListChildrenSalesOffer = new List<ChildrenSalesOfferInternalTicket>();
                            if (offer.OfferType.Contains("Ticket"))
                            {

                                ParentSalesOfferId = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOfferId).FirstOrDefault();
                                ParentSalesOfferSErial = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOffer?.OfferSerial).FirstOrDefault();
                                ListChildrenSalesOffer = ParentSalesOfferListDB.Where(x => x.ParentSalesOfferId == offer.Id).Select(x => new ChildrenSalesOfferInternalTicket { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial, CreationDate = x.SalesOffer?.CreationDate.ToShortDateString(), TotalPrice = x.SalesOffer?.FinalOfferPrice }).ToList();
                            }
                            
                            if (offer.OfferType.Contains("ExtClinics"))
                            {
                                var currentMedicalReservationOfOffer = medicalReservationListDB.Where(a => a.OfferId == offer.Id).FirstOrDefault();
                                long? parentMedicalReservationID = currentMedicalReservationOfOffer?.ParentId;
                                if(parentMedicalReservationID != null)
                                {
                                    var parentSalesOffer = medicalReservationListDB.Where(a => a.Id == parentMedicalReservationID).FirstOrDefault()?.Offer;
                                    ParentSalesOfferSErial = parentSalesOffer?.OfferSerial;
                                    ParentSalesOfferId = parentSalesOffer?.Id;
                                }

                                var parentReservationOfChildId = medicalReservationListDB.Where(a => a.OfferId == offer.Id).FirstOrDefault();
                                if(parentReservationOfChildId != null)
                                {
                                    var childSalesofferListIDs = medicalReservationListDB.Where(a => a.ParentId == parentReservationOfChildId.Id).ToList().Select(b => b.OfferId);
                                    ListChildrenSalesOffer = OffersListDB.Where(a => childSalesofferListIDs.Contains(a.Id)).Select(x => new ChildrenSalesOfferInternalTicket { SalesOfferId = x.Id, SalesOfferSerial = x.OfferSerial, CreationDate = x.CreationDate.ToShortDateString(), TotalPrice = x.FinalOfferPrice }).ToList();
                                }

                            }

                            SalesOfferDetailsForInternalTicket salesOfferObj = new SalesOfferDetailsForInternalTicket()
                            {
                                Id = offer.Id,
                                SalesPersonId = offer.SalesPersonId,
                                OfferType = offer.OfferType,
                                SalesPersonName = offer.SalesPerson.FirstName + ' ' + offer.SalesPerson.MiddleName + ' ' + offer.SalesPerson.LastName,
                                ClientId = offer.ClientId,
                                ClientName = offer.Client.Name,
                                ContactPersonName = offer.ContactPersonName,
                                OfferSerial = offer.OfferSerial,
                                FinalOfferPrice = offer.FinalOfferPrice,
                                CreationDate = offer.CreationDate.ToString(),
                                CreatorName = offer.CreatedByNavigation?.FirstName + " " + offer.CreatedByNavigation?.LastName,
                                ParentSalesOfferID = ParentSalesOfferId,
                                ParentSalesOfferSerial = ParentSalesOfferSErial,
                                ChildrenSalesOfferList = ListChildrenSalesOffer
                            };

                            OffersListResponse.Add(salesOfferObj);
                        }
                        salesOfferTypeDetails.SalesOfferList = OffersListResponse;
                        Response.SalesOfferList = salesOfferTypeDetails;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = OffersListDB.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = OffersListDB.TotalCount
                        };
                    }
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

        public string GetExcelColumnName(int columnIndex)
        {
            //if (columnIndex < 1)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index must be greater than 0.");
            //}

            string columnName = string.Empty;
            while (columnIndex > 0)
            {
                int remainder = (columnIndex - 1) % 26; // Get the remainder (0-25)
                columnName = Convert.ToChar('A' + remainder) + columnName; // Convert to corresponding letter
                columnIndex = (columnIndex - 1) / 26; // Move to the next higher place
            }
            return columnName;
        }

        public BaseResponseWithData<string> GetSalesOfferForStoreExcel(string From, string To, string UserId, string CompName, string OfferType, string OfferTypeReturn, long createdBy)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region Validation
                var startDate = DateTime.Now;
                if (!DateTime.TryParse(From, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    response.Errors.Add(err);
                    return response;
                }

                var endDate = DateTime.Now;
                if (!DateTime.TryParse(To, out endDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    response.Errors.Add(err);
                    return response;
                }

                var userRole = _unitOfWork.UserRoles.FindAll(a => a.UserId == createdBy).Select(b => b.RoleId);
                if (!userRole.Contains(167) && !userRole.Contains(170) && !userRole.Contains(171))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "You are not authorized to view this report";
                    response.Errors.Add(err);
                    return response;
                }

                #endregion

                var listOfStoreData = new List<SalesOfferDueClientPOS>();



                var salesOffersData = _unitOfWork.SalesOffers.FindAllQueryable(a => a.CreationDate >= startDate && a.CreationDate <= endDate && a.OfferType.Contains(OfferType),
                    new[] { "SalesOfferProducts", "CreatedByNavigation", "Client", "CreatedByNavigation", "SalesPerson", "SalesOfferProducts.InventoryItemCategory", "Client.SalesPerson", "SalesPerson.UserTeamUsers", "SalesOfferProducts.InventoryItem" });


                if (!string.IsNullOrEmpty(UserId))
                {
                    var usersIDsList = UserId.Split(',')
                                         .Select(s => long.Parse(s))
                                         .ToList();

                    if (userRole.Contains(167) && !userRole.Contains(170) && !userRole.Contains(171))
                    {
                        usersIDsList.Clear();
                        usersIDsList.Add(createdBy);
                        salesOffersData = salesOffersData.Where(a => usersIDsList.Contains(a.CreatedBy));
                    }

                    else if (usersIDsList.Count() > 0)
                    {

                        salesOffersData = salesOffersData.Where(a => usersIDsList.Contains(a.CreatedBy));
                    }
                }
                else
                {
                    if (userRole.Contains(167) && !userRole.Contains(170) && !userRole.Contains(171))
                    {
                        salesOffersData = salesOffersData.Where(a => a.CreatedBy == createdBy);
                    }

                }


                var salesOffers = salesOffersData.ToList();

                var minusSummation = salesOffers.Where(a => a.OfferType == OfferTypeReturn).Sum(a => a.FinalOfferPrice);
                var plusSummationu = salesOffers.Where(a => a.OfferType == OfferType).Sum(a => a.FinalOfferPrice);
                var totalSummationu = plusSummationu - minusSummation;

                var salesOfferGroupByCreator = salesOffers.GroupBy(a => new { a.CreatedBy }).ToList();


                //----------------------------------------excel----------------------------------------------
                ExcelPackage excel = new ExcelPackage();

                //-------------------------------------------------------------------------------------------
                if (salesOfferGroupByCreator.Count() == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This date selected not have sales offer";
                    response.Errors.Add(err);
                    return response;
                }

                #region All_Users_Collaps
                //var salesOfferList = new List<SalesOfferDueClientPOS>();
                var sheet = excel.Workbook.Worksheets.Add($"All_Users_Collaps");
                var teams = _unitOfWork.Teams.GetAll();

                int rowCounter = 1;
                int rowNum = 2;
                int code = 1;

                decimal sum = 0;
                decimal cashSum = 0;

                decimal finalTotalSum = 0;
                foreach (var group in salesOfferGroupByCreator)
                {
                    //-------------------------------two lines before the headers (name, total)--------------------------
                    rowNum++;
                    sheet.Cells[rowNum, 1].Value = "Name";
                    sheet.Cells[rowNum, 2].Value = "Total for this user";
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rowNum++;

                    var finalOfferPricesForTickets = group.Where(a => a.OfferType == OfferType).Sum(a => a.FinalOfferPrice);
                    var finalOfferPricesForTicketsReturn = group.Where(a => a.OfferType == OfferTypeReturn).Sum(a => a.FinalOfferPrice);


                    var firstSalesOfferInGroup = group.FirstOrDefault();
                    var FirstlastName = firstSalesOfferInGroup.CreatedByNavigation.LastName;
                    if (FirstlastName == ".") FirstlastName = " ";
                    sheet.Cells[rowNum, 1].Value = firstSalesOfferInGroup.CreatedByNavigation.FirstName + " " + FirstlastName;
                    sheet.Cells[rowNum, 2].Value = finalOfferPricesForTickets - finalOfferPricesForTicketsReturn;
                    sheet.Cells[rowNum, 2].Style.Numberformat.Format = "#,##0.00";

                    sheet.Cells[rowNum, 1, rowNum, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowNum, 1, rowNum, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    //---------------------------------------------------------------------------------------------------

                    var userDataOfthisGroup = salesOffers.Where(a => a.CreatedBy == group.Key.CreatedBy).FirstOrDefault().CreatedByNavigation;


                    sheet.Row(rowNum).OutlineLevel = 1;
                    sheet.Row(rowNum).Collapsed = false;
                    //---------naming of Excel file--------------
                    for (int col = 1; col <= 13; col++) sheet.Column(col).Width = 20;

                    //var teams = _unitOfWork.Teams.GetAll();
                    var salesOffersDrawedAll = new List<long>();
                    rowNum++;
                    int counterForCondition = 0;
                    foreach (var salesOffer in group)
                    {
                        sheet.Row(rowNum).OutlineLevel = 2;
                        sheet.Row(rowNum).Collapsed = true;
                        if (counterForCondition == 0)
                        {

                            sheet.Cells[rowNum, 1].Value = "Created By \n المستخدم";
                            sheet.Cells[rowNum, 2].Value = "Code";
                            sheet.Cells[rowNum, 3].Value = "Ticket Price" + " \r\n " + "القيمه";
                            sheet.Cells[rowNum, 4].Value = "Category \n القسم";
                            sheet.Cells[rowNum, 5].Value = "Doctor Name \n الطبيب";
                            sheet.Cells[rowNum, 6].Value = "Ticket number \n  رقم التذكره";
                            sheet.Cells[rowNum, 7].Value = "patient \n المريض";
                            sheet.Cells[rowNum, 8].Value = "Notes \n الملاحظات";
                            sheet.Cells[rowNum, 9].Value = "Contact Person \n بيد";
                            sheet.Cells[rowNum, 10].Value = "Creation Date \n التاريخ";
                            sheet.Cells[rowNum, 11].Value = "offer Type \n النوع";
                            sheet.Cells[rowNum, 12].Value = "Doctor Team \n التخصص";
                            sheet.Cells[rowNum, 13].Value = "Item Name \n التشخيص";

                            sheet.Cells[rowNum, 1, rowNum, 13].Style.WrapText = true;


                            sheet.Column(3).Style.Numberformat.Format = "#,##0.00";

                            sheet.DefaultRowHeight = 20;
                            sheet.Row(rowNum).Height = 40;
                            sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet.Row(rowNum).Style.Font.Bold = true;
                            sheet.Cells[rowNum, 1, rowNum, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[rowNum, 1, rowNum, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                            sheet.Cells[rowNum, 1, rowNum, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowNum, 1, rowNum, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            counterForCondition++;
                            rowNum++;
                        }

                        //newSalesOffer.OfferID = item.OrderId;
                        //var salesOffer = salesOffers.Where(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
                        //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);

                        if (salesOffer != null)
                        {
                            var alreadyDrawed = salesOffersDrawedAll.Contains(salesOffer.Id);
                            if (salesOffer != null && !alreadyDrawed)
                            {
                                var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;


                                sheet.Cells[rowNum, 1].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
                                sheet.Cells[rowNum, 2].Value = code; code++;
                                sheet.Cells[rowNum, 3].Value = salesOffer.FinalOfferPrice;

                                var lastName = salesOffer.SalesPerson.LastName;
                                if (lastName == ".") lastName = " ";
                                sheet.Cells[rowNum, 5].Value = salesOffer.SalesPerson.FirstName + " " + lastName;
                                sheet.Cells[rowNum, 5].Value = salesOffer.ProjectName;
                                sheet.Cells[rowNum, 7].Value = salesOffer.Client.Name;
                                //sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;sheet.Cells[rowNum, 6].Value = salesOffer.Id;
                                sheet.Cells[rowNum, 9].Value = salesOffer.ContactPersonName;//salesOffer.Client.SalesPerson.FirstName + " " + salesOffer.Client.SalesPerson.LastName;
                                sheet.Cells[rowNum, 10].Value = salesOffer.CreationDate.ToString();
                                sheet.Cells[rowNum, 11].Value = salesOffer.OfferType;
                                sheet.Cells[rowNum, 12].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
                                sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                                //---------------sum of cash-----------------------------
                                if (salesOffer.OfferType == OfferTypeReturn) cashSum -= salesOffer.FinalOfferPrice ?? 0;
                                else { cashSum += salesOffer.FinalOfferPrice ?? 0; }


                                if (salesOffer.OfferType == OfferTypeReturn)                    //make the row color red on Sales Return
                                {
                                    sheet.Cells[rowNum, 1, rowNum, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    sheet.Cells[rowNum, 1, rowNum, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                    sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                //-------------------------------------------------------
                                foreach (var productDB in salesOffer.SalesOfferProducts)
                                {
                                    sheet.Row(rowNum).OutlineLevel = 2;
                                    sheet.Row(rowNum).Collapsed = true;
                                    sheet.Cells[rowNum, 8].Value = productDB.ItemPricingComment;
                                    //sheet.Cells[rowNum , 9].Value = productDB.Quantity;
                                    //sheet.Cells[rowNum , 4].Value = productDB.ItemPrice;
                                    sheet.Cells[rowNum, 4].Value = productDB.InventoryItemCategory?.Name ?? productDB.ItemPricingComment; 
                                    sheet.Cells[rowNum, 13].Value = productDB.InventoryItem?.Name ?? productDB.ItemPricingComment;
                                    sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                                    //rowNum++;
                                }
                                if (salesOffer.OfferType == OfferTypeReturn) { sum = sum - salesOffer.FinalOfferPrice ?? 0; }
                                else { sum += salesOffer.FinalOfferPrice ?? 0; }


                                salesOffersDrawedAll.Add(salesOffer.Id);

                                //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
                                //newSalesOffer.projectName = salesOffer.ProjectName;
                                //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
                                //newSalesOffer.OfferType = salesOffer.OfferType;
                            }

                            //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                            //{
                            //    ItemPrice = a.ItemPrice,
                            //    Quantity = a.Quantity,
                            //    ProductID = a.Id,
                            //    productComment = a.ItemPricingComment
                            //});

                            //productList.AddRange(product);
                            //newSalesOffer.ProductList = productList;
                            //salesOfferList.Add(newSalesOffer);

                        }
                        rowNum++;
                    }
                    finalTotalSum += sum;

                    salesOffersDrawedAll.Clear();
                    sum = 0;

                    rowCounter = rowNum;


                }
                sheet.Cells[1, 2].Value = finalTotalSum;
                sheet.Cells[1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                sheet.Cells[1, 2].Style.Numberformat.Format = "#,##0.00";
                #endregion

                /*
                #region all Users
                //-----------------------sheet for all users--------------------------------------------------
                var sheet2 = excel.Workbook.Worksheets.Add($"All_Users");

                 
                    
                //---------naming of Excel file--------------
                for (int col = 1; col <= 13; col++) sheet2.Column(col).Width = 20;

                sheet2.Cells[1, 1].Value = "Code";
                sheet2.Cells[1, 2].Value = "Ticket Price" + " \r\n " + "القيمه";
                sheet2.Cells[1, 3].Value = "Category \n القسم";
                sheet2.Cells[1, 4].Value = "Doctor Name \n الطبيب";
                sheet2.Cells[1, 5].Value = "Ticket number \n  رقم التذكره";
                sheet2.Cells[1, 6].Value = "patient \n المريض";
                sheet2.Cells[1, 7].Value = "Notes \n الملاحظات";
                sheet2.Cells[1, 8].Value = "Contact Person \n بيد";
                sheet2.Cells[1, 9].Value = "Creation Date \n التاريخ";
                sheet2.Cells[1, 10].Value = "offer Type \n النوع";
                sheet2.Cells[1, 11].Value = "Doctor Team \n التخصص";
                sheet2.Cells[1, 12].Value = "Item Name \n التشخيص";
                sheet2.Cells[1, 13].Value = "Created By \n المستخدم";

                sheet2.Cells[1, 1, 1, 13].Style.WrapText = true;
                //sheet.Cells[1, 4].Value = "Offer Name";
                //sheet.Cells[1, 5].Value = "final Offer Price";
                //sheet.Cells[1, 10].Value = "Item Price";
                //sheet.Cells[1, 6].Value = "Internal ID";
                //sheet.Cells[1, 5].Value = "Category Name";

                sheet2.Column(2).Style.Numberformat.Format = "#,##0.00";

                sheet2.DefaultRowHeight = 20;
                sheet2.Row(1).Height = 40;
                sheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Row(1).Style.Font.Bold = true;
                sheet2.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet2.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                sheet2.Cells[1, 1, 1, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Cells[1, 1, 1, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                decimal testSumAll = 0;
                decimal testSubAll = 0;
                decimal sumAll = 0;
                decimal cashSumAll = 0;
                int rowNumAll = 3;

                int codeAll = 1;
                
                var salesOffersDrawed = new List<long>();
                foreach (var salesOffer in salesOffers)
                {

                    //newSalesOffer.OfferID = item.OrderId;
                    //var salesOffer = salesOffers.Where(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
                    //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);
                    if (salesOffer != null)
                    {
                        var alreadyDrawed = salesOffersDrawed.Contains(salesOffer.Id);
                        if (salesOffer != null && !alreadyDrawed)
                        {
                            var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;

                            sheet2.Row(rowNumAll).OutlineLevel = 1;
                            sheet2.Row(rowNumAll).Collapsed = false;
                            sheet2.Cells[rowNumAll, 1].Value = codeAll; codeAll++;
                            sheet2.Cells[rowNumAll, 2].Value = salesOffer.FinalOfferPrice;
                            sheet2.Cells[rowNumAll, 4].Value = salesOffer.SalesPerson.FirstName + salesOffer.SalesPerson.LastName;
                            sheet2.Cells[rowNumAll, 5].Value = salesOffer.ProjectName;
                            sheet2.Cells[rowNumAll, 6].Value = salesOffer.Client.Name;
                            //sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;sheet.Cells[rowNum, 6].Value = salesOffer.Id;
                            sheet2.Cells[rowNumAll, 8].Value = salesOffer.Client.SalesPerson.FirstName + salesOffer.Client.SalesPerson.LastName;
                            sheet2.Cells[rowNumAll, 9].Value = salesOffer.CreationDate.ToString();
                            sheet2.Cells[rowNumAll, 10].Value = salesOffer.OfferType;
                            sheet2.Cells[rowNumAll, 11].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
                            sheet2.Cells[rowNumAll, 13].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
                            sheet2.Row(rowNumAll).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet2.Row(rowNumAll).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            //---------------sum of cash-----------------------------
                            if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSumAll -= salesOffer.FinalOfferPrice ?? 0;
                            else { cashSumAll += salesOffer.FinalOfferPrice ?? 0; }


                            if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
                            {
                                sheet2.Cells[rowNumAll, 1, rowNumAll, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                sheet2.Cells[rowNumAll, 1, rowNumAll, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                sheet2.Row(rowNumAll).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet2.Row(rowNumAll).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }
                            //-------------------------------------------------------
                            foreach (var productDB in salesOffer.SalesOfferProducts)
                            {
                                sheet2.Row(rowNumAll).OutlineLevel = 2;
                                sheet2.Row(rowNumAll).Collapsed = true;
                                sheet2.Cells[rowNumAll, 7].Value = productDB.ItemPricingComment;
                                //sheet.Cells[rowNum , 9].Value = productDB.Quantity;
                                //sheet.Cells[rowNum , 4].Value = productDB.ItemPrice;
                                sheet2.Cells[rowNumAll, 3].Value = productDB.InventoryItemCategory.Name;
                                sheet2.Cells[rowNumAll, 12].Value = productDB.InventoryItem.Name;
                                sheet2.Row(rowNumAll).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet2.Row(rowNumAll).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                                //rowNum++;
                            }
                            if (salesOffer.OfferType.Contains("Internal Ticket return")) { sumAll = sumAll - salesOffer.FinalOfferPrice ?? 0; }
                            else { sumAll += salesOffer.FinalOfferPrice ?? 0; }


                            salesOffersDrawed.Add(salesOffer.Id);

                            //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
                            //newSalesOffer.projectName = salesOffer.ProjectName;
                            //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
                            //newSalesOffer.OfferType = salesOffer.OfferType;
                        }

                        //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                        //{
                        //    ItemPrice = a.ItemPrice,
                        //    Quantity = a.Quantity,
                        //    ProductID = a.Id,
                        //    productComment = a.ItemPricingComment
                        //});

                        //productList.AddRange(product);
                        //newSalesOffer.ProductList = productList;
                        //salesOfferList.Add(newSalesOffer);

                    }
                    rowNumAll++;
                }
                sheet2.Cells[2, 2].Value = sumAll;
                sheet2.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet2.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet2.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                salesOffersDrawed.Clear();
                sumAll = 0;
                #endregion
                */


                #region sheet for return (For all users)
                //------------------------------------sheet for return (For all users)----------------------------------------

                var sheet4 = excel.Workbook.Worksheets.Add($"Return_All_Users");



                //---------naming of Excel file--------------

                var salesOffersReturn = salesOffers.Where(a => a.OfferType == OfferTypeReturn).GroupBy(b => new { b.CreatedBy }).ToList();

                int rowCounterReturn = 1;
                int rowNumReturn = 2;
                int codeReturn = 1;

                decimal sumReturn = 0;
                decimal cashSumReturn = 0;

                decimal finalTotalSumReturn = 0;
                foreach (var group in salesOffersReturn)
                {
                    sumReturn = 0;
                    //-------------------------------two lines before the headers (name, total)--------------------------
                    rowNumReturn++;
                    sheet4.Cells[rowNumReturn, 1].Value = "Name";
                    sheet4.Cells[rowNumReturn, 2].Value = "Total for this user";
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rowNumReturn++;

                    var firstSalesOfferInGroup = group.FirstOrDefault();
                    var FirstlastName = firstSalesOfferInGroup.CreatedByNavigation.LastName;
                    if (FirstlastName == ".") FirstlastName = " ";
                    sheet4.Cells[rowNumReturn, 1].Value = firstSalesOfferInGroup.CreatedByNavigation.FirstName + " " + FirstlastName;
                    sheet4.Cells[rowNumReturn, 2].Value = group.Sum(a => a.FinalOfferPrice);
                    sheet4.Cells[rowNumReturn, 2].Style.Numberformat.Format = "#,##0.00";

                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    //---------------------------------------------------------------------------------------------------

                    var userDataOfthisGroup = salesOffers.Where(a => a.CreatedBy == group.Key.CreatedBy).FirstOrDefault().CreatedByNavigation;


                    sheet4.Row(rowNumReturn).OutlineLevel = 1;
                    sheet4.Row(rowNumReturn).Collapsed = false;
                    //---------naming of Excel file--------------
                    for (int col = 1; col <= 13; col++) sheet4.Column(col).Width = 20;


                    //var teams = _unitOfWork.Teams.GetAll();
                    var salesOffersDrawedAll = new List<long>();
                    rowNumReturn++;
                    var counterForConditionReturn = 0;
                    foreach (var salesOffer in group)
                    {
                        sheet4.Row(rowNumReturn).OutlineLevel = 2;
                        sheet4.Row(rowNumReturn).Collapsed = true;
                        if (counterForConditionReturn == 0)
                        {

                            sheet4.Cells[rowNumReturn, 1].Value = "Created By \n المستخدم";
                            sheet4.Cells[rowNumReturn, 2].Value = "Code";
                            sheet4.Cells[rowNumReturn, 3].Value = "Ticket Price" + " \r\n " + "القيمه";
                            sheet4.Cells[rowNumReturn, 4].Value = "Category \n القسم";
                            sheet4.Cells[rowNumReturn, 5].Value = "Doctor Name \n الطبيب";
                            sheet4.Cells[rowNumReturn, 6].Value = "Ticket number \n  رقم التذكره";
                            sheet4.Cells[rowNumReturn, 7].Value = "patient \n المريض";
                            sheet4.Cells[rowNumReturn, 8].Value = "Notes \n الملاحظات";
                            sheet4.Cells[rowNumReturn, 9].Value = "Contact Person \n بيد";
                            sheet4.Cells[rowNumReturn, 10].Value = "Creation Date \n التاريخ";
                            sheet4.Cells[rowNumReturn, 11].Value = "offer Type \n النوع";
                            sheet4.Cells[rowNumReturn, 12].Value = "Doctor Team \n التخصص";
                            sheet4.Cells[rowNumReturn, 13].Value = "Item Name \n التشخيص";

                            sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.WrapText = true;


                            sheet4.Column(3).Style.Numberformat.Format = "#,##0.00";

                            sheet4.DefaultRowHeight = 20;
                            sheet4.Row(rowNumReturn).Height = 40;
                            sheet4.Row(rowNumReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet4.Row(rowNumReturn).Style.Font.Bold = true;
                            sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                            sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            counterForConditionReturn++;
                            rowNumReturn++;
                        }
                        if (salesOffer != null)
                        {
                            var alreadyDrawed = salesOffersDrawedAll.Contains(salesOffer.Id);
                            if (salesOffer != null && !alreadyDrawed)
                            {
                                var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;


                                sheet4.Cells[rowNumReturn, 1].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
                                sheet4.Cells[rowNumReturn, 2].Value = codeReturn; codeReturn++;
                                sheet4.Cells[rowNumReturn, 3].Value = salesOffer.FinalOfferPrice;

                                var lastName = salesOffer.SalesPerson.LastName;
                                if (lastName == ".") lastName = " ";
                                sheet4.Cells[rowNumReturn, 5].Value = salesOffer.SalesPerson.FirstName + " " + lastName;
                                sheet4.Cells[rowNumReturn, 5].Value = salesOffer.ProjectName;
                                sheet4.Cells[rowNumReturn, 7].Value = salesOffer.Client.Name;
                                //sheet4.Cells[rowNumReturn, 4].Value = salesOffer.OfferType;sheet4.Cells[rowNumReturn, 6].Value = salesOffer.Id;
                                sheet4.Cells[rowNumReturn, 9].Value = salesOffer.ContactPersonName;//salesOffer.Client.SalesPerson.FirstName + " " + salesOffer.Client.SalesPerson.LastName;
                                sheet4.Cells[rowNumReturn, 10].Value = salesOffer.CreationDate.ToString();
                                sheet4.Cells[rowNumReturn, 11].Value = salesOffer.OfferType;
                                sheet4.Cells[rowNumReturn, 12].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
                                sheet4.Row(rowNumReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet4.Row(rowNumReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                                //---------------sum of cash-----------------------------
                                if (salesOffer.OfferType == OfferTypeReturn) cashSumReturn -= salesOffer.FinalOfferPrice ?? 0;
                                else { cashSumReturn += salesOffer.FinalOfferPrice ?? 0; }


                                if (salesOffer.OfferType == OfferTypeReturn)                    //make the row color red on Sales Return
                                {
                                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    sheet4.Cells[rowNumReturn, 1, rowNumReturn, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                    sheet4.Row(rowNumReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet4.Row(rowNumReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                }
                                //-------------------------------------------------------
                                foreach (var productDB in salesOffer.SalesOfferProducts)
                                {
                                    sheet4.Row(rowNumReturn).OutlineLevel = 2;
                                    sheet4.Row(rowNumReturn).Collapsed = true;
                                    sheet4.Cells[rowNumReturn, 8].Value = productDB.ItemPricingComment;
                                    //sheet4.Cells[rowNumReturn , 9].Value = productDB.Quantity;
                                    //sheet4.Cells[rowNumReturn , 4].Value = productDB.ItemPrice;
                                    sheet.Cells[rowNum, 4].Value = productDB.InventoryItemCategory?.Name ?? productDB.ItemPricingComment;
                                    sheet.Cells[rowNum, 13].Value = productDB.InventoryItem?.Name ?? productDB.ItemPricingComment;
                                    sheet4.Row(rowNumReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet4.Row(rowNumReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                                    //rowNumReturn++;
                                }
                                if (salesOffer.OfferType == OfferTypeReturn) { sumReturn = sumReturn - salesOffer.FinalOfferPrice ?? 0; }
                                else { sumReturn += salesOffer.FinalOfferPrice ?? 0; }


                                salesOffersDrawedAll.Add(salesOffer.Id);

                                //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
                                //newSalesOffer.projectName = salesOffer.ProjectName;
                                //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
                                //newSalesOffer.OfferType = salesOffer.OfferType;
                            }

                            //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                            //{
                            //    ItemPrice = a.ItemPrice,
                            //    Quantity = a.Quantity,
                            //    ProductID = a.Id,
                            //    productComment = a.ItemPricingComment
                            //});

                            //productList.AddRange(product);
                            //newSalesOffer.ProductList = productList;
                            //salesOfferList.Add(newSalesOffer);

                        }
                        rowNumReturn++;
                    }
                    finalTotalSumReturn += sumReturn;

                    salesOffersDrawedAll.Clear();
                    sum = 0;

                    rowCounterReturn = rowNumReturn;


                }
                sheet4.Cells[1, 2].Value = finalTotalSumReturn;
                sheet4.Cells[1, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[1, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet4.Cells[1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[1, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                sheet4.Cells[1, 2].Style.Numberformat.Format = "#,##0.00";
                //------------------------------------------------------------------------------------------------------------
                #endregion

                if (userRole.Contains(170) || userRole.Contains(171))
                {


                    #region TotalAmountSheet
                    //-----------------------sheet for TotalAmountForEachCategory--------------------------------------------------
                    var sheet3 = excel.Workbook.Worksheets.Add($"TotalAmountForEachCategory");

                    TimeSpan difference = endDate - startDate;

                    // Get the number of days
                    int daysDifference = difference.Days;

                    var inventoryCategories = _unitOfWork.InventoryItemCategories.GetAll();

                    var salesOfferList = _unitOfWork.SalesOffers.FindAll(a => a.CreationDate >= startDate && a.CreationDate <= endDate && a.OfferType.Contains(OfferType));

                    var salesOfferIDs = salesOfferList.Select(a => a.Id).ToList();

                    // the real data to show
                    var salesOfferProduct = _unitOfWork.SalesOfferProducts.FindAll(a => a.CreationDate >= startDate && a.CreationDate <= endDate && salesOfferIDs.Contains(a.OfferId), new[] { "Offer", "InventoryItem", "InventoryItemCategory" });

                    var categoriesIDs = salesOfferProduct.Select(a => a.InventoryItemCategoryId).Distinct().ToList();
                    var categoriesNamesList = inventoryCategories.Where(a => categoriesIDs.Contains(a.Id)).ToList();

                    //ExcelPackage excel = new ExcelPackage();
                    //var sheet = excel.Workbook.Worksheets.Add($"sheet1");
                    for (int col = 1; col <= categoriesNamesList.Count() + 1; col++) sheet3.Column(col).Width = 20;
                    sheet3.DefaultRowHeight = 12;
                    sheet3.Row(1).Height = 20;
                    sheet3.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet3.Row(1).Style.Font.Bold = true;
                    sheet3.Cells[1, 1, 1, categoriesNamesList.Count() + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet3.Cells[1, 1, 1, categoriesNamesList.Count() + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet3.Cells[1, 1, 100, categoriesNamesList.Count() + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet3.Cells[1, 1, 100, categoriesNamesList.Count() + 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    var drawCol = 2;
                    foreach (var cat in categoriesNamesList)
                    {
                        sheet3.Cells[1, drawCol].Value = cat.Name;
                        drawCol++;

                    }

                    sheet3.Cells[1, drawCol, 1, drawCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet3.Cells[1, drawCol, 1, drawCol].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);

                    sheet3.Cells[1, 1].Value = "التاريخ";
                    sheet3.Cells[1, drawCol].Value = "الاجمالي";

                    var column = 2;
                    var row = 2;
                    var curserDate = startDate.Date;
                    for (int i = 0; i <= daysDifference; i++)
                    {
                        column = 2;
                        sheet3.Cells[row, 1].Value = curserDate.ToShortDateString();
                        decimal rowTotal = 0;
                        foreach (var category in categoriesNamesList)
                        {

                            var categoryDayTotalSales = salesOfferProduct.Where(a => a.Offer.OfferType == OfferType && !a.Offer.OfferType.Contains(OfferTypeReturn) && a.CreationDate >= curserDate && a.CreationDate < curserDate.AddDays(1) && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

                            var categoryDayTotalReturn = salesOfferProduct.Where(a => a.Offer.OfferType == OfferTypeReturn && a.CreationDate >= curserDate && a.CreationDate < curserDate.AddDays(1) && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

                            var categoryDayTotal = categoryDayTotalSales - categoryDayTotalReturn;
                            sheet3.Cells[row, column].Value = categoryDayTotal;
                            rowTotal = rowTotal + categoryDayTotal ?? 0;

                            column++;
                        }
                        if (i == 0)
                        {
                            curserDate = curserDate.AddDays(i + 1);
                            i++;
                        }
                        else
                        {
                            curserDate = startDate.AddDays(i);
                        }
                        sheet3.Cells[row, column].Value = rowTotal;
                        sheet3.Cells[row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet3.Cells[row, column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                        sheet3.Cells[2, 2, row + 1, column].Style.Numberformat.Format = "#,##0.00"; // Apply to range A1:D10
                        sheet3.Column(column).Width = 20;                                       // width of the Total column

                        row++;
                    }

                    var columnTotalcurser = 2;
                    for (int i = 0; i < column; i++)
                    {
                        var columnLetter = GetExcelColumnName(columnTotalcurser);
                        string formula = $"SUM({columnLetter}{2}:{columnLetter}{row - 1})";  // Dynamic formula for column (e.g., SUM(A:A))
                        sheet3.Cells[row, columnTotalcurser].Formula = formula; columnTotalcurser++;
                    }

                    sheet3.Cells[row, 1, row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet3.Cells[row, 1, row, column].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                    sheet3.Cells[row, 1].Value = "الاجمالي";


                    sheet3.Cells[1, 1, row - 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet3.Cells[1, 1, row - 1, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

                    //--------------------------------------------------------------------------------------------
                    #endregion

                    #region Total_For_Each_User
                    var sheet5 = excel.Workbook.Worksheets.Add($"Total_For_Each_User");

                    //---------naming of Excel file--------------
                    for (int col = 1; col <= 6; col++) sheet5.Column(col).Width = 20;

                    //sheet.Cells[1, 1].Value = "Code";
                    sheet5.Cells[1, 1].Value = "Created By \n المستخدم";
                    sheet5.Cells[1, 2].Value = "Number of Tickets";
                    sheet5.Cells[1, 3].Value = "Total sales";
                    sheet5.Cells[1, 4].Value = "Number of return Tickets";
                    sheet5.Cells[1, 5].Value = "Total Returns";
                    sheet5.Cells[1, 6].Value = "Net";


                    sheet5.Cells[1, 1, 1, 13].Style.WrapText = true;
                    //sheet.Cells[1, 4].Value = "Offer Name";
                    //sheet.Cells[1, 5].Value = "final Offer Price";
                    //sheet.Cells[1, 10].Value = "Item Price";
                    //sheet.Cells[1, 6].Value = "Internal ID";
                    //sheet.Cells[1, 5].Value = "Category Name";

                    sheet5.Column(3).Style.Numberformat.Format = "#,##0.00";
                    sheet5.Column(5).Style.Numberformat.Format = "#,##0.00";
                    sheet5.Column(6).Style.Numberformat.Format = "#,##0.00";

                    sheet5.DefaultRowHeight = 20;
                    sheet5.Row(1).Height = 40;
                    sheet5.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet5.Row(1).Style.Font.Bold = true;
                    sheet5.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet5.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                    sheet5.Cells[1, 1, 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet5.Cells[1, 1, 1, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                    //--------------------------totals-------------------
                    var totalNumOfTickets = 0;
                    decimal? finalTotalSales = 0;
                    var FinalTotalNumOfReturnTickets = 0;
                    decimal? finalTotalReturns = 0;
                    decimal? totalNet = 0;
                    //---------------------------------------------------

                    int rowIndex = 2;
                    foreach (var group in salesOfferGroupByCreator)
                    {

                        var userDataOfthisGroup = salesOffers.Where(a => a.CreatedBy == group.Key.CreatedBy).FirstOrDefault().CreatedByNavigation;

                        var totalAmount = group.Where(a => a.OfferType.Contains(OfferType) && !a.OfferType.Contains(OfferTypeReturn)).Sum(b => b.FinalOfferPrice);
                        var totalReturnAmount = group.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice);

                        var netAmount = totalAmount - totalReturnAmount;


                        var currentNumOfTickets = group.Count();
                        var currentTotalTicketsPrice = group.Where(a => a.OfferType.Contains(OfferType) && !a.OfferType.Contains(OfferTypeReturn)).Sum(b => b.FinalOfferPrice);
                        var currentTotalNumOfReturnTickets = group.Where(a => a.OfferType == OfferTypeReturn).Count();
                        var currentTotalReturns = group.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice); ;

                        //sheet5.Cells[rowNum, 1].Value = code; code++;
                        sheet5.Cells[rowIndex, 1].Value = userDataOfthisGroup.FirstName + " " + userDataOfthisGroup.LastName;
                        sheet5.Cells[rowIndex, 2].Value = currentNumOfTickets;
                        sheet5.Cells[rowIndex, 3].Value = currentTotalTicketsPrice;
                        sheet5.Cells[rowIndex, 4].Value = currentTotalNumOfReturnTickets;
                        sheet5.Cells[rowIndex, 5].Value = currentTotalReturns;
                        sheet5.Cells[rowIndex, 6].Value = netAmount;
                        sheet5.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet5.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                        totalNumOfTickets += currentNumOfTickets;
                        finalTotalSales += currentTotalTicketsPrice;
                        FinalTotalNumOfReturnTickets += currentTotalNumOfReturnTickets;
                        finalTotalReturns += currentTotalReturns;
                        totalNet += netAmount;

                        rowIndex++;

                    }
                    //sheet5.Cells[rowIndex, 1].Value = userDataOfthisGroup.FirstName + " " + userDataOfthisGroup.LastName;
                    sheet5.Cells[rowIndex, 2].Value = totalNumOfTickets;
                    sheet5.Cells[rowIndex, 3].Value = finalTotalSales;
                    sheet5.Cells[rowIndex, 4].Value = FinalTotalNumOfReturnTickets;
                    sheet5.Cells[rowIndex, 5].Value = finalTotalReturns;
                    sheet5.Cells[rowIndex, 6].Value = totalNet;

                    sheet5.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet5.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet5.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet5.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet5.Cells[rowIndex, 1].Value = "الاجمالي";
                    #endregion

                    #region Total_By_Item_Name
                    var sheet6 = excel.Workbook.Worksheets.Add($"Total_By_Item_Name");

                    var salesOfferGroupByItemName = salesOffers.GroupBy(a => a.SalesOfferProducts.Select(b => b.InventoryItem).FirstOrDefault());
                    //---------naming of Excel file--------------
                    for (int col = 1; col <= 6; col++) sheet6.Column(col).Width = 20;

                    //sheet.Cells[1, 1].Value = "Code";
                    sheet6.Cells[1, 1].Value = "Item Name \n التشخيص";
                    sheet6.Cells[1, 2].Value = "Number of Tickets";
                    sheet6.Cells[1, 3].Value = "Total sales";
                    sheet6.Cells[1, 4].Value = "Number of return Tickets";
                    sheet6.Cells[1, 5].Value = "Total Returns";
                    sheet6.Cells[1, 6].Value = "Net";


                    sheet6.Cells[1, 1, 1, 13].Style.WrapText = true;
                    //sheet.Cells[1, 4].Value = "Offer Name";
                    //sheet.Cells[1, 5].Value = "final Offer Price";
                    //sheet.Cells[1, 10].Value = "Item Price";
                    //sheet.Cells[1, 6].Value = "Internal ID";
                    //sheet.Cells[1, 5].Value = "Category Name";

                    sheet6.Column(3).Style.Numberformat.Format = "#,##0.00";
                    sheet6.Column(5).Style.Numberformat.Format = "#,##0.00";
                    sheet6.Column(6).Style.Numberformat.Format = "#,##0.00";

                    sheet6.DefaultRowHeight = 20;
                    sheet6.Row(1).Height = 40;
                    sheet6.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet6.Row(1).Style.Font.Bold = true;
                    sheet6.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet6.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                    sheet6.Cells[1, 1, 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet6.Cells[1, 1, 1, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                    //--------------------------totals-------------------
                    var totalNumOfTickets2 = 0;
                    decimal? finalTotalSales2 = 0;
                    var FinalTotalNumOfReturnTickets2 = 0;
                    decimal? finalTotalReturns2 = 0;
                    decimal? totalNet2 = 0;
                    //---------------------------------------------------

                    int rowIndex2 = 2;
                    foreach (var group in salesOfferGroupByItemName)
                    {

                        //var userDataOfthisGroup = salesOffers.Where(a => a.SalesOfferProducts.FirstOrDefault().Id == group.Key.Id).FirstOrDefault().;

                        var totalAmount = group.Sum(b => b.FinalOfferPrice);
                        var totalReturnAmount = group.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice);

                        var netAmount = totalAmount - totalReturnAmount;


                        var currentNumOfTickets = group.Count();
                        var currentTotalTicketsPrice = group.Sum(b => b.FinalOfferPrice);
                        var currentTotalNumOfReturnTickets = group.Where(a => a.OfferType == OfferTypeReturn).Count();
                        var currentTotalReturns = group.Where(a => a.OfferType == OfferTypeReturn).Sum(b => b.FinalOfferPrice);

                        //sheet5.Cells[rowNum, 1].Value = code; code++;
                        sheet6.Cells[rowIndex2, 1].Value = group.FirstOrDefault()?.SalesOfferProducts.FirstOrDefault()?.InventoryItem.Name;
                        sheet6.Cells[rowIndex2, 2].Value = currentNumOfTickets;
                        sheet6.Cells[rowIndex2, 3].Value = currentTotalTicketsPrice;
                        sheet6.Cells[rowIndex2, 4].Value = currentTotalNumOfReturnTickets;
                        sheet6.Cells[rowIndex2, 5].Value = currentTotalReturns;
                        sheet6.Cells[rowIndex2, 6].Value = netAmount;
                        sheet6.Row(rowIndex2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet6.Row(rowIndex2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                        totalNumOfTickets2 += currentNumOfTickets;
                        finalTotalSales2 += currentTotalTicketsPrice;
                        FinalTotalNumOfReturnTickets2 += currentTotalNumOfReturnTickets;
                        finalTotalReturns2 += currentTotalReturns;
                        totalNet2 += netAmount;

                        rowIndex2++;

                    }
                    //sheet5.Cells[rowIndex, 1].Value = userDataOfthisGroup.FirstName + " " + userDataOfthisGroup.LastName;
                    sheet6.Cells[rowIndex2, 2].Value = totalNumOfTickets;
                    sheet6.Cells[rowIndex2, 3].Value = finalTotalSales;
                    sheet6.Cells[rowIndex2, 4].Value = FinalTotalNumOfReturnTickets;
                    sheet6.Cells[rowIndex2, 5].Value = finalTotalReturns;
                    sheet6.Cells[rowIndex2, 6].Value = totalNet;

                    sheet6.Row(rowIndex2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet6.Row(rowIndex2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet6.Cells[rowIndex2, 1, rowIndex2, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet6.Cells[rowIndex2, 1, rowIndex2, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    sheet6.Cells[rowIndex2, 1].Value = "الاجمالي";
                    #endregion

                }
                var path = $"Attachments\\{CompName}\\GetSalesOfferTicketsForStore";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                //delete older files
                if (Directory.Exists(savedPath))
                {
                    foreach (var file in Directory.GetFiles(savedPath))
                    {
                        File.Delete(file);
                    }
                }
                ;

                // Create excel file on physical disk  
                if (!Directory.Exists(savedPath)) Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var dateNow = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\GetSalesOfferTicketsForStoreReport_{dateNow}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var filePath = Globals.baseURL + '/' + path + $"\\GetSalesOfferTicketsForStoreReport_{dateNow}.xlsx";

                response.Data = filePath;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<string> GetSalesOfferForStoreForAllUsersPDF(InternalticketheaderPdf header, long createdBy)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {

                #region Validation
                var startDate = DateTime.Now;
                if (!DateTime.TryParse(header.FromDate, out startDate))
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    Response.Errors.Add(err);
                    return Response;
                }

                var endDate = DateTime.Now;
                if (!DateTime.TryParse(header.ToDate, out endDate))
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    Response.Errors.Add(err);
                    return Response;
                }

                var userRole = _unitOfWork.UserRoles.FindAll(a => a.UserId == createdBy).Select(b => b.RoleId);
                if (!userRole.Contains(167) && !userRole.Contains(170) && !userRole.Contains(171))
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "You are not authorized to view this report";
                    Response.Errors.Add(err);
                    return Response;
                }


                #endregion

                var listOfStoreData = new List<SalesOfferDueClientPOS>();



                var salesOffersData = _unitOfWork.SalesOffers.FindAllQueryable(a => a.CreationDate.Date >= startDate.Date && a.CreationDate.Date <= endDate.Date && a.OfferType.Contains(header.OfferType), new[] { "SalesOfferProducts", "CreatedByNavigation", "Client", "CreatedByNavigation" });

                if (!string.IsNullOrEmpty(header.UserID))
                {
                    var usersIDsList = header.UserID.Split(',')
                                         .Select(s => long.Parse(s))
                                         .ToList();

                    if (userRole.Contains(167) && !userRole.Contains(170) && !userRole.Contains(171))
                    {
                        usersIDsList.Clear();
                        usersIDsList.Add(createdBy);
                        salesOffersData = salesOffersData.Where(a => usersIDsList.Contains(a.CreatedBy));
                    }

                    else if (usersIDsList.Count() > 0)
                    {

                        salesOffersData = salesOffersData.Where(a => usersIDsList.Contains(a.CreatedBy));
                    }
                }
                else
                {
                    if (userRole.Contains(167) && !userRole.Contains(170) && !userRole.Contains(171))
                    {
                        salesOffersData = salesOffersData.Where(a => a.CreatedBy == createdBy);
                    }

                }

                var salesOffers = salesOffersData.ToList();

                var minusSummation = salesOffers.Where(a => a.OfferType == header.OfferTypeReturn).Sum(a => a.FinalOfferPrice);
                var plusSummationu = salesOffers.Where(a => a.OfferType == header.OfferType).Sum(a => a.FinalOfferPrice);
                var totalSummationu = plusSummationu - minusSummation;


                //-------------------------------------------------------------------------------------------


                var StartYear = new DateTime(2000, 1, 1);
                var AccountAdvancedType = _unitOfWork.VAccounts.GetAll().GroupBy(a => a.AdvanciedTypeId);
                if (!string.IsNullOrEmpty(header.FromDate) && DateTime.TryParse(header.FromDate, out StartYear))
                {
                    StartYear = DateTime.Parse(header.FromDate);
                }


                var CurrentEndYear = new DateTime(DateTime.Now.Year, 12, 30);

                if (!string.IsNullOrEmpty(header.ToDate) && DateTime.TryParse(header.ToDate, out CurrentEndYear))
                {
                    CurrentEndYear = DateTime.Parse(header.ToDate);
                }
                var DateFrom = header.FromDate;
                var DateTo = CurrentEndYear.ToShortDateString();
                /*if (string.IsNullOrEmpty(Request.Headers["DateTo"]))
                {
                    Request.Headers["DateTo"] = DateTo;
                }*/

                //End Filters
                //   ------------------------------------------- Pdf -----------------------------------
                MemoryStream ms = new MemoryStream();

                //Size of page
                iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.LETTER);


                PdfWriter pw = PdfWriter.GetInstance(document, ms);

                //Call the footer Function

                pw.PageEvent = new HeaderFooter2();

                document.Open();

                //Handle fonts and Sizes +  Attachments images logos 

                iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.LETTER);


                BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font font = new Font(bf, 7, Font.NORMAL);

                String path = Path.Combine(_host.WebRootPath, "Attachments");

                if (header.CompanyName.ToString().ToLower() == "marinaplt")
                {
                    string PDFp_strPath122 = Path.Combine(path, "logoMarina.png");
                    if (System.IO.File.Exists(PDFp_strPath122))
                    {
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath122);
                        jpg.SetAbsolutePosition(60f, 750f);
                        document.Add(jpg);
                    }
                    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 18, 1, iTextSharp.text.BaseColor.WHITE);
                    Paragraph prgHeading = new Paragraph();
                    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    prgHeading.SpacingBefore = -20;
                    prgHeading.SpacingAfter = 20;


                    Chunk cc = new Chunk(" Multi-Step Income Statement" + " ", fntHead);
                    cc.SetBackground(new BaseColor(4, 189, 189), 100, 9, 15, 22);


                    prgHeading.Add(cc);

                    document.Add(prgHeading);

                }
                else if (header.CompanyName.ToString().ToLower() == "piaroma")
                {
                    string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    if (System.IO.File.Exists(Piaroma_p_strPath))
                    {
                        Image logo = Image.GetInstance(Piaroma_p_strPath);
                        logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) - 20);
                        logo.ScaleToFit(100f, 50f);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                        document.Add(logo);
                    }

                    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 18, 1, iTextSharp.text.BaseColor.WHITE);
                    Paragraph prgHeading = new Paragraph();
                    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    prgHeading.SpacingBefore = -20;
                    prgHeading.SpacingAfter = 20;

                    Chunk cc = new Chunk(" Multi-Step Income Statement" + " ", fntHead);
                    cc.SetBackground(new BaseColor(4, 189, 189), 100, 9, 15, 22);

                    prgHeading.Add(cc);

                    document.Add(prgHeading);


                }

                else if (header.CompanyName.ToString().ToLower() == "garastest")
                {
                    string Piaroma_p_strPath = Path.Combine(path, "STMark-Logo.png");
                    if (System.IO.File.Exists(Piaroma_p_strPath))
                    {
                        Image logo = Image.GetInstance(Piaroma_p_strPath);
                        logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) - 50);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                        logo.ScaleToFit(100f, 50f);
                        document.Add(logo);
                    }

                    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 18, 1, iTextSharp.text.BaseColor.WHITE);
                    PdfPTable table = new PdfPTable(1); // جدول بعمود واحد
                    table.RunDirection = PdfWriter.RUN_DIRECTION_RTL; // اتجاه الكتابة من اليمين إلى اليسار

                    // إعداد النص في خلية
                    //PdfPCell cell = new PdfPCell(new Phrase("تقرير التذاكر الداخلى", fntHead))
                    //{
                    //    HorizontalAlignment = Element.ALIGN_RIGHT, // محاذاة النص إلى اليمين
                    //    VerticalAlignment = Element.ALIGN_MIDDLE, // محاذاة النص عموديًا
                    //    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    //    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                    //};
                    //table.AddCell(cell);


                    table.AddCell(new PdfPCell(new Phrase("تقرير التذاكر الداخلى", new iTextSharp.text.Font(bf, 14, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                    {
                        RunDirection = PdfWriter.RUN_DIRECTION_RTL,
                        HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                        VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                        Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                        BackgroundColor = new BaseColor(4, 189, 189),
                        FixedHeight = 40f,
                    });
                    // إضافة الخلية إلى الجدول


                    // table.WidthPercentage = 50;
                    // المسافة بعد الجدول
                    table.WidthPercentage = 40;
                    table.SpacingBefore = -75; // المسافة قبل الجدول
                    table.SpacingAfter = 40;


                    // إضافة الجدول إلى المستند
                    document.Add(table);

                }

                else if (header.CompanyName.ToString().ToLower() == "stmark")
                {
                    string Piaroma_p_strPath = Path.Combine(path, "STMark-Logo.png");
                    if (System.IO.File.Exists(Piaroma_p_strPath))
                    {
                        Image logo = Image.GetInstance(Piaroma_p_strPath);
                        logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) - 50);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                        logo.ScaleToFit(100f, 50f);
                        document.Add(logo);
                    }

                    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 18, 1, iTextSharp.text.BaseColor.WHITE);
                    PdfPTable table = new PdfPTable(1); // جدول بعمود واحد
                    table.RunDirection = PdfWriter.RUN_DIRECTION_RTL; // اتجاه الكتابة من اليمين إلى اليسار

                    // إعداد النص في خلية
                    //PdfPCell cell = new PdfPCell(new Phrase("تقرير التذاكر الداخلى", fntHead))
                    //{
                    //    HorizontalAlignment = Element.ALIGN_RIGHT, // محاذاة النص إلى اليمين
                    //    VerticalAlignment = Element.ALIGN_MIDDLE, // محاذاة النص عموديًا
                    //    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    //    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                    //};
                    //table.AddCell(cell);


                    table.AddCell(new PdfPCell(new Phrase("تقرير التذاكر الداخلى", new iTextSharp.text.Font(bf, 14, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                    {
                        RunDirection = PdfWriter.RUN_DIRECTION_RTL,
                        HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                        VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                        Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                        BackgroundColor = new BaseColor(4, 189, 189),
                        FixedHeight = 40f,
                    });
                    // إضافة الخلية إلى الجدول


                    // table.WidthPercentage = 50;
                    // المسافة بعد الجدول
                    table.WidthPercentage = 40;
                    table.SpacingBefore = -75; // المسافة قبل الجدول
                    table.SpacingAfter = 40;


                    // إضافة الجدول إلى المستند
                    document.Add(table);



                }


                // Start of dt ID = 10
                #region

                PdfPTable dt = new PdfPTable(10);

                // var dt = new System.Data.DataTable("grid");


                dt.DefaultCell.BorderWidth = 0.1f; // Set thin borders globally
                dt.DefaultCell.BorderColor = BaseColor.GRAY; // Optional: Set border color
                dt.DefaultCell.BorderWidthLeft = 0.1f;
                dt.DefaultCell.BorderWidthRight = 0.1f;
                dt.DefaultCell.BorderWidthTop = 0.1f;
                dt.DefaultCell.BorderWidthBottom = 0.1f;

                dt.DefaultCell.Border = Rectangle.BOX; // استخدام إطار خارجي للجدول (BOX)
                dt.DefaultCell.BorderWidth = 5f; // سمك الإطار الخارجي

                dt.WidthPercentage = 100;



                // ------------------------------------------------------------------- headertable-------------------------------------------------

                dt.AddCell(new PdfPCell(new Phrase("Ticket Name", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE,  // محاذاة النص عموديًا في المنتصف
                    Border = PdfPCell.NO_BORDER,               // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });

                dt.AddCell(new PdfPCell(new Phrase("Ticket Price", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Creation Date", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Offer Type", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Client", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Internal ID", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Item Name", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Quantity", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Item Price", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });
                dt.AddCell(new PdfPCell(new Phrase("Cash Amount", new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.BOLD, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                    VerticalAlignment = Element.ALIGN_MIDDLE, // Right-to-Left for Arabic
                    Border = PdfPCell.NO_BORDER,             // إزالة الحدود
                    BackgroundColor = new BaseColor(4, 189, 189) // لون الخلفية
                });

                PdfPCell boldLineCell = new PdfPCell(new Phrase(" "));
                boldLineCell.FixedHeight = 5f; // تقليل الارتفاع
                boldLineCell.BorderWidthBottom = 2.5f; // Set bold bottom border
                boldLineCell.BorderWidthTop = 2.5f;    // Remove top border
                boldLineCell.BorderWidthLeft = 0f;   // Remove left border
                boldLineCell.BorderWidthRight = 0f;  // Remove right border
                boldLineCell.BorderColorBottom = BaseColor.DARK_GRAY; // Set bottom border color
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);



                decimal sum = 0;
                decimal cashSum = 0;
                var salesOffersDrawed = new List<long>();
                var Counter = 1;

                foreach (var salesOffer in salesOffers)
                {
                    if (Counter > 1)
                    {
                        PdfPCell boldLineCell1 = new PdfPCell(new Phrase(" "));
                        boldLineCell1.FixedHeight = 5f; // تقليل الارتفاع
                        boldLineCell1.BorderWidthBottom = 2.5f; // Set bold bottom border
                        boldLineCell1.BorderWidthTop = 2.5f;    // Remove top border
                        boldLineCell1.BorderWidthLeft = 0f;   // Remove left border
                        boldLineCell1.BorderWidthRight = 0f;  // Remove right border
                        boldLineCell1.BorderColorBottom = BaseColor.DARK_GRAY; // Set bottom border color
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                        dt.AddCell(boldLineCell1);
                    }


                    if (salesOffer != null)
                    {
                        ////var alreadyDrawed = salesOffersDrawed.Contains(salesOffer.Id);
                        if (salesOffer != null)
                        {


                            dt.AddCell(new PdfPCell(new Phrase(salesOffer.ProjectName, new iTextSharp.text.Font(bf, 5, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                            {
                                RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                                HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                                VerticalAlignment = Element.ALIGN_MIDDLE// Right-to-Left for Arabic
                            });

                            dt.AddCell(new PdfPCell(new Phrase(salesOffer.FinalOfferPrice.ToString(), font)));
                            dt.AddCell(new PdfPCell(new Phrase(salesOffer.CreationDate.ToString(), font)));
                            dt.AddCell(new PdfPCell(new Phrase(salesOffer.OfferType.ToString(), font)));
                            dt.AddCell(new PdfPCell(new Phrase(salesOffer.Client.Name, new iTextSharp.text.Font(bf, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                            {
                                RunDirection = PdfWriter.RUN_DIRECTION_RTL, // Right-to-Left for Arabic
                                HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                                VerticalAlignment = Element.ALIGN_MIDDLE// Right-to-Left for Arabic
                            });
                            dt.AddCell(new PdfPCell(new Phrase(salesOffer.Id.ToString(), new iTextSharp.text.Font(bf, 6, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                            {
                                RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                                HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                                VerticalAlignment = Element.ALIGN_MIDDLE// Right-to-Left for Arabic
                            });
                            dt.AddCell("        ");
                            dt.AddCell("        ");
                            dt.AddCell("        ");
                            dt.AddCell("        ");



                            //---------------sum of cash-----------------------------
                            if (salesOffer.OfferType == header.OfferTypeReturn) cashSum -= salesOffer.FinalOfferPrice ?? 0;
                            else { cashSum += salesOffer.FinalOfferPrice ?? 0; }
                            //-------------------------------------------------------
                            foreach (var productDB in salesOffer.SalesOfferProducts)
                            {
                                dt.AddCell("        ");
                                dt.AddCell("        ");
                                dt.AddCell("        ");
                                dt.AddCell("        ");
                                dt.AddCell("        ");
                                dt.AddCell("        ");
                                dt.AddCell(new PdfPCell(new Phrase(productDB.ItemPricingComment, new iTextSharp.text.Font(bf, 7, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                                {
                                    RunDirection = PdfWriter.RUN_DIRECTION_RTL // Right-to-Left for Arabic
                                });
                                dt.AddCell(new PdfPCell(new Phrase(productDB.Quantity.ToString(), new iTextSharp.text.Font(bf, 6, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                                {
                                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                                    HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                                    VerticalAlignment = Element.ALIGN_MIDDLE// Right-to-Left for Arabic
                                });
                                dt.AddCell(new PdfPCell(new Phrase(Convert.ToDecimal(productDB.ItemPrice).ToString("N2"), new iTextSharp.text.Font(bf, 6, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000")))))
                                {
                                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,
                                    //HorizontalAlignment = Element.ALIGN_CENTER, // محاذاة النص أفقيًا في المنتصف
                                    VerticalAlignment = Element.ALIGN_MIDDLE// Right-to-Left for Arabic
                                });

                                dt.AddCell(new PdfPCell(new Phrase("      ", font)));



                                //----------------------------------------------------------



                                Counter++;
                            }
                            if (salesOffer.OfferType == header.OfferTypeReturn) { sum = sum - salesOffer.FinalOfferPrice ?? 0; }
                            else { sum += salesOffer.FinalOfferPrice ?? 0; }


                            salesOffersDrawed.Add(salesOffer.Id);

                        }


                    }
                    Counter++;







                }


                PdfPCell boldLineCell3 = new PdfPCell(new Phrase(" "));
                boldLineCell.FixedHeight = 5f; // تقليل الارتفاع
                boldLineCell.BorderWidthBottom = 2.5f; // Set bold bottom border
                boldLineCell.BorderWidthTop = 2.5f;    // Remove top border
                boldLineCell.BorderWidthLeft = 0f;   // Remove left border
                boldLineCell.BorderWidthRight = 0f;  // Remove right border
                boldLineCell.BorderColorBottom = BaseColor.PINK; // Set bottom border color
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);



                dt.AddCell("   ");
                dt.AddCell(new PdfPCell(new Phrase(sum.ToString(), new iTextSharp.text.Font(bf, 7, 1, new BaseColor(0, 0, 255)))) // Green color
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,// Right-to-Left for Arabic
                    BackgroundColor = BaseColor.YELLOW
                });

                dt.AddCell("    ");
                dt.AddCell("    ");
                dt.AddCell("    ");
                dt.AddCell("    ");
                dt.AddCell("    ");
                dt.AddCell("    ");
                dt.AddCell("    ");
                //dt.AddCell(cashSum.ToString());
                dt.AddCell(new PdfPCell(new Phrase(cashSum.ToString(), new iTextSharp.text.Font(bf, 7, 1, new BaseColor(255, 0, 0)))) // Red color
                {
                    RunDirection = PdfWriter.RUN_DIRECTION_LTR,// Right-to-Left for Arabic
                    BackgroundColor = BaseColor.GREEN

                });




                PdfPCell boldLineCell4 = new PdfPCell(new Phrase(" "));
                boldLineCell.FixedHeight = 5f; // تقليل الارتفاع
                boldLineCell.BorderWidthBottom = 2.5f; // Set bold bottom border
                boldLineCell.BorderWidthTop = 2.5f;    // Remove top border
                boldLineCell.BorderWidthLeft = 0f;   // Remove left border
                boldLineCell.BorderWidthRight = 0f;  // Remove right border
                boldLineCell.BorderColorBottom = BaseColor.PINK; // Set bottom border color
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);
                dt.AddCell(boldLineCell);



                salesOffersDrawed.Clear();
                sum = 0;




                //dt.AddCell("");
                //dt.AddCell(new PdfPCell(new Phrase(sum.ToString(), new iTextSharp.text.Font(bf, 7, 1, new BaseColor(0, 0, 255)))) // Green color
                //{
                //    RunDirection = PdfWriter.RUN_DIRECTION_LTR // Right-to-Left for Arabic
                //});

                //dt.AddCell("");
                //dt.AddCell("");
                //dt.AddCell("");
                //dt.AddCell("");
                //dt.AddCell("");
                //dt.AddCell("");
                //dt.AddCell("");
                ////dt.AddCell(cashSum.ToString());
                //dt.AddCell(new PdfPCell(new Phrase(cashSum.ToString(), new iTextSharp.text.Font(bf, 7, 1, new BaseColor(255, 0, 0)))) // Red color
                //{
                //    RunDirection = PdfWriter.RUN_DIRECTION_LTR // Right-to-Left for Arabic
                //});


                #endregion

                //End Calculations

                PdfPTable tableHeadingDate = new PdfPTable(4);

                tableHeadingDate.WidthPercentage = 100;

                tableHeadingDate.SetTotalWidth(new float[] { 80, 100, 80, 600 });



                PdfPCell cell3Date = new PdfPCell();
                string cell3Datetext = "From Date:" + " " + DateFrom + " " + " " + "-" + " " + " " + "To Date:" + " " + DateTo;
                cell3Date.Phrase = new Phrase(cell3Datetext, new iTextSharp.text.Font(bf, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                iTextSharp.text.Font arabicFontsasd2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                cell3Date.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                cell3Date.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell3Date.VerticalAlignment = Element.ALIGN_CENTER;
                cell3Date.BorderColor = BaseColor.WHITE;
                cell3Date.PaddingBottom = 8;

                PdfPCell cell4Date = new PdfPCell();
                string cell4DateTest = "";
                iTextSharp.text.Font arabicFont22sasds = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                cell4Date.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                cell4Date.HorizontalAlignment = Element.ALIGN_CENTER;
                cell4Date.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell4Date.BorderColor = BaseColor.WHITE;



                tableHeadingDate.AddCell(cell4Date);
                tableHeadingDate.AddCell(cell4Date);
                tableHeadingDate.AddCell(cell4Date);
                tableHeadingDate.AddCell(cell3Date);

                tableHeadingDate.KeepTogether = true;



                PdfPTable tableHeading = new PdfPTable(4);

                tableHeading.WidthPercentage = 100;

                tableHeading.SetTotalWidth(new float[] { 600, 100, 80, 80 });



                PdfPCell cell3 = new PdfPCell();
                string cell3text = "Sales Offer Tickets For Store Report ";
                cell3.Phrase = new Phrase(cell3text, new iTextSharp.text.Font(bf, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                cell3.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                cell3.HorizontalAlignment = Element.ALIGN_CENTER;
                cell3.VerticalAlignment = Element.ALIGN_CENTER;
                cell3.BorderColor = new BaseColor(4, 189, 189);
                cell3.PaddingTop = 3;
                cell3.PaddingBottom = 8;
                cell3.BackgroundColor = new BaseColor(4, 189, 189);
                cell3.BorderColor = BaseColor.WHITE;



                PdfPCell cell4 = new PdfPCell();
                string cell4text = "";
                iTextSharp.text.Font arabicFont22ss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                cell4.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                cell4.HorizontalAlignment = Element.ALIGN_CENTER;
                cell4.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell4.BorderColor = new BaseColor(4, 189, 189);
                cell4.PaddingTop = 3;
                cell4.PaddingBottom = 6;
                cell4.BackgroundColor = new BaseColor(4, 189, 189);



                tableHeading.AddCell(cell4);
                tableHeading.AddCell(cell4);
                tableHeading.AddCell(cell4);

                document.Add(tableHeadingDate);
                document.Add(tableHeading);

                document.Add(dt);


                tableHeading.KeepTogether = true;

                document.Close();
                byte[] result = ms.ToArray();
                ms = new MemoryStream();
                ms.Write(result, 0, result.Length);
                ms.Position = 0;


                var CompanyName = header.CompanyName.ToString().ToLower();

                string FullFileName = DateTime.Now.ToFileTime() + "_" + "GetSalesOfferTicketsForStore.pdf";
                string PathsTR = "Attachments/" + CompanyName + "/";
                String Filepath = Path.Combine(_host.WebRootPath, PathsTR);
                string p_strPath = Path.Combine(Filepath, FullFileName);

                System.IO.File.WriteAllBytes(p_strPath, result);

                Response.Data = Globals.baseURL + '/' + PathsTR + FullFileName;

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

        public async Task<BaseResponseWithId<long>> CreateDoctorUserAsync(AddDoctorUserDTO NewHrUser, long UserId, string CompanyName)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                List<long> teamsIDList = new List<long>();
                if (!string.IsNullOrEmpty(NewHrUser.TeamsIdList))
                {
                    teamsIDList = NewHrUser.TeamsIdList
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(long.Parse)
                    .ToList();
                }
                
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
                if (NewHrUser.FirstName == null || NewHrUser.FirstName == "")
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    errorMessage = errorMessage + "FirstName - ";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (NewHrUser.ARLastName == null || NewHrUser.ARLastName == "")
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    errorMessage = errorMessage + "ArlastName - ";
                    //response.Errors.Add(err);
                    //return response;
                }
                if (NewHrUser.LastName == null || NewHrUser.LastName == "")
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
                bool emailWasNull = false;
                var random = new Random();
                int number = random.Next(0, 9999);
                if (NewHrUser.Email == null || NewHrUser.Email == "")
                {
                    //response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    //errorMessage = errorMessage + "Email - ";
                    //response.Errors.Add(err);
                    //return response;
                    NewHrUser.Email = $"{NewHrUser.FirstName}+{NewHrUser.LastName}+{number}@st.com";
                    emailWasNull = true;
                }
                bool mobileWasNull = false;
                if (string.IsNullOrWhiteSpace(NewHrUser.Mobile))
                {
                    //response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    //errorMessage = errorMessage + "Mobile ";
                    //response.Errors.Add(err);
                    //return response;
                    NewHrUser.Mobile = $"012+{number}";
                    mobileWasNull = true;
                }
                //if (NewHrData.Gender == null || NewHrData.Gender == "")
                //{
                //    response.Result = false;
                //    Error err = new Error();
                //    err.ErrorCode = "E-1";
                //    err.errorMSG = "please, Enter a valid Gender :";
                //    response.Errors.Add(err);
                //    return response;
                //}
                if (!string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = errorMessage.Insert(0, "Please add( ");
                    string finalErrorMessage = errorMessage + $") before Submission";
                    Error error = new Error();
                    error.ErrorMSG = finalErrorMessage;
                    response.Errors.Add(error);
                }
                #endregion

                var allHrUsers = await _unitOfWork.HrUsers.GetAllAsync();
                var allHrUsersFullName = allHrUsers.Select(a => a.FirstName.ToLower() + a.MiddleName.ToLower() + a.LastName.ToLower()).ToList();

                var newUserName = "";
                if (!string.IsNullOrWhiteSpace(NewHrUser.FirstName) && !string.IsNullOrWhiteSpace(NewHrUser.LastName))
                {
                    newUserName = NewHrUser.FirstName.Replace(" ", "") + MiddleName.Replace(" ", "") + NewHrUser.LastName.Replace(" ", "");
                }

                #region not repeating
                var notRepatingMessage = "";
                if (allHrUsersFullName.Contains(newUserName.ToLower()))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    notRepatingMessage = notRepatingMessage + " This FullName is already Exists please, Choose another one -";
                    //response.Errors.Add(err);
                    //return response;
                }
                if(emailWasNull ==  false)
                {

                    var allHrUsersEmails = allHrUsers.Select(a => a.Email);
                    if (allHrUsersEmails.Contains(NewHrUser.Email) || allHrUsersEmails.Contains(NewHrUser.Email.ToLower()))
                    {
                        response.Result = false;
                        //Error err = new Error();
                        //err.ErrorCode = "E-1";
                        notRepatingMessage = notRepatingMessage + " This Email is already Exists please, Enter a valid Email -";
                        //response.Errors.Add(err);
                        //return response;
                    }
                }
                var allHrUsersLandLines = allHrUsers.Where(a => a.LandLine != null).Select(a => a.LandLine);
                if (allHrUsersLandLines.Contains(NewHrUser.LandLine))
                {
                    response.Result = false;
                    //Error err = new Error();
                    //err.ErrorCode = "E-1";
                    notRepatingMessage = notRepatingMessage + " This Home is already Exists please, Enter a valid Home -";
                    //response.Errors.Add(err);
                    //return response;
                }
                if(mobileWasNull == false)
                {

                    if (NewHrUser.Mobile != "0")
                    {
                        var allHrUsersMobileNumbers = allHrUsers.Select(a => a.Mobile);
                        if (allHrUsersMobileNumbers.Contains(NewHrUser.Mobile))
                        {
                            response.Result = false;
                            //Error err = new Error();
                            //err.ErrorCode = "E-1";
                            notRepatingMessage = notRepatingMessage + " This Mobile is already Exists please, Enter a valid Mobile ";
                            //response.Errors.Add(err);
                            //return response;
                        }
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
                if (NewHrUser.BranchID != null)
                {
                    var branch = _unitOfWork.Branches.FindAll(a => a.Id == NewHrUser.BranchID).FirstOrDefault();
                    if (branch == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "No Branch with this ID :";
                        response.Errors.Add(err);
                        return response;
                    }
                }
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
                if (NewHrUser.DepartmentID != null)
                {
                    var branch = _unitOfWork.Departments.FindAll(a => a.Id == NewHrUser.DepartmentID).FirstOrDefault();
                    if (branch == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "No Department with this ID :";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                if (NewHrUser.TeamId != null)
                {
                    var branch = _unitOfWork.Teams.FindAll(a => a.Id == NewHrUser.TeamId).FirstOrDefault();
                    if (branch == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "No Team with this ID :";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                if (!string.IsNullOrEmpty(NewHrUser.TeamsIdList))
                {
                    var teamsList = _unitOfWork.Teams.FindAll(a => teamsIDList.Contains(a.Id));
                    if (teamsList == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "No Team with this ID :";
                        response.Errors.Add(err);
                        return response;
                    }
                    foreach (var id in teamsIDList)
                    {
                        var currentTeam = teamsList.Where(a => a.Id ==  id).FirstOrDefault();
                        if (currentTeam == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"No Team with ID : {id}";
                            response.Errors.Add(err);
                            return response;

                        }
                    }
                }
                #endregion


                IFormFile ImgInMemory = null;
                if (NewHrUser.Photo != null)
                {
                    ImgInMemory = NewHrUser.Photo;
                }
                var user = new HrUser()
                {
                    FirstName = NewHrUser.FirstName,
                    MiddleName = NewHrUser.MiddleName,
                    LastName = NewHrUser.LastName,
                    Active = NewHrUser.Active ?? true,
                    ArfirstName = NewHrUser.ARFirstName,
                    ArlastName = NewHrUser.ARLastName,
                    ArmiddleName = NewHrUser.ARMiddleName,
                    BranchId = NewHrUser.BranchID,
                    DepartmentId = NewHrUser.DepartmentID,
                    LandLine = NewHrUser.LandLine,
                    TeamId = NewHrUser.TeamId,
                    IsUser = true,
                    Mobile = NewHrUser.Mobile,
                    Gender = NewHrUser.Gender,
                    CreationDate = DateTime.Now,
                    CreatedById = UserId,
                    ModifiedById = UserId,
                    Modified = DateTime.Now,
                    Email = NewHrUser.Email
                };
                user.Email = user.Email.ToLower();
                if (NewHrUser.DateOfBirth != null) user.DateOfBirth = DoB;
                //--------------Trim() spaces from full name-------------------------
                user.FirstName = user.FirstName.Trim();
                user.MiddleName = MiddleName.Trim();
                user.LastName = user.LastName.Trim();
                //-------------------------------------------------------------------
                user.ImgPath = null; //Common.SaveFileIFF(virtualPath, file, FileName, fileExtension, _host);
                user.CreationDate = DateTime.Now;
                user.ModifiedById = UserId;
                user.CreatedById = UserId;
                user.Modified = DateTime.Now;
                user.IsDeleted = false;
                //user.OldId = 0;
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

                if (NewHrUser.TeamId != null)
                {

                    var newTeamUser = new UserTeam();
                    newTeamUser.TeamId = NewHrUser.TeamId ?? 0;
                    newTeamUser.HrUserId = HrUser.Id;
                    newTeamUser.CreatedBy = UserId;
                    newTeamUser.CreatedDate = DateTime.Now;
                    newTeamUser.ModifiedBy = UserId;
                    newTeamUser.ModifiedDate = DateTime.Now;

                    var teamUser = await _unitOfWork.UserTeams.AddAsync(newTeamUser);
                    _unitOfWork.Complete();

                }

                if (teamsIDList.Count() > 0)
                {
                    var listOfTeams = new List<UserTeam>();
                    foreach (var team in NewHrUser.TeamsIdList)
                    {
                        var newTeamUser = new UserTeam();
                        newTeamUser.TeamId = team;
                        newTeamUser.HrUserId = HrUser.Id;
                        newTeamUser.CreatedBy = UserId;
                        newTeamUser.CreatedDate = DateTime.Now;
                        newTeamUser.ModifiedBy = UserId;
                        newTeamUser.ModifiedDate = DateTime.Now;

                        listOfTeams.Add(newTeamUser) ;
                    }
                    

                    var teamUser = await _unitOfWork.UserTeams.AddRangeAsync(listOfTeams);
                    _unitOfWork.Complete();

                }

                #region add HrUser to User Table
                if(string.IsNullOrEmpty(NewHrUser.Password) && string.IsNullOrEmpty(NewHrUser.ConfirmPass))
                {
                    NewHrUser.Password = "qw@3345NAA";
                    NewHrUser.ConfirmPass = "qw@3345NAA";
                }
                var newUserData = new AddHrEmployeeToUserDTO()
                {
                    HrUserId = HrUser.Id,
                    Password = NewHrUser.Password,
                    ConfirmPass = NewHrUser.ConfirmPass,
                    Email = NewHrUser.Email
                };

                var userAdded = await _hrUserService.AddHrEmployeeToUserAsync(newUserData, UserId, key);

                HrUser.IsUser = true;
                HrUser.UserId = userAdded.Data.Id;

                _unitOfWork.Complete();
                #endregion


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

    }
}
