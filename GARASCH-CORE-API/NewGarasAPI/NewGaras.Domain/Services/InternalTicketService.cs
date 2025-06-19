using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.InternalTicket;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Helper;
using System.Web;
using NewGaras.Infrastructure.Models.SalesOffer.InternalTicket;
using iTextSharp.text.pdf;
using iTextSharp.text;
using NewGarasAPI.Models.AccountAndFinance;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.ProjectsDetails;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using Microsoft.AspNetCore.Http.HttpResults;
using Azure;
//using System.Drawing;

namespace NewGaras.Domain.Services
{
    public class InternalTicketService : IInternalTicketService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";

        public InternalTicketService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;

        }


        public BaseResponseWithData<List<TopData>> GetTopDoctors(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To)
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

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains("Internal Ticket"), includes: new[] { "SalesPerson.Department" });

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
                    salesOffers = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket") && a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
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
                    Sum = a.Where(x => x.OfferType == "Internal Ticket").Sum(x => x.FinalOfferPrice ?? 0) - a.Where(x => x.OfferType == "Internal Ticket return").Sum(x => x.FinalOfferPrice ?? 0),
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


        public BaseResponseWithData<List<TopData>> GetTopDepartmentsOrCategories(int year, int month, int day, string type)
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

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains("Internal Ticket"), includes: new[] { "SalesPerson.Department" });
                var salesOffersV = _unitOfWork.VSalesOffers.FindAllQueryable(a => a.OfferType.Contains("Internal Ticket"));
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
                    var byDepartments = salesOffers.ToList().GroupBy(a => a.SalesPerson.Department.Name).Select(a => new TopData()
                    {
                        Name = a.Key,
                        Count = a.Count(),
                        Sum = a.Where(x => x.OfferType == "Internal Ticket").Sum(x => x.FinalOfferPrice ?? 0) - a.Where(x => x.OfferType == "Internal Ticket return").Sum(x => x.FinalOfferPrice ?? 0),
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
                        Sum = a.Where(x => x.OfferType == "Internal Ticket").Sum(x => x.FinalOfferPrice ?? 0) - a.Where(x => x.OfferType == "Internal Ticket return").Sum(x => x.FinalOfferPrice ?? 0),
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


        public BaseResponseWithData<InternalTicketDashboardSummary> GetInternalTicketDashboard(int year, int month, int day, DateTime? From, DateTime? To)
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

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains("Internal Ticket"), includes: new[] { "SalesPerson.Department" });
                response.Data = new InternalTicketDashboardSummary();
                if (year != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Year == date.Year).AsQueryable();

                    response.Data.Year = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
                }

                if (month != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Month == date.Month).AsQueryable();

                    response.Data.Month = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
                }

                if (day != 0)
                {
                    salesOffers = salesOffers.Where(a => a.CreationDate.Day == date.Day).AsQueryable();

                    response.Data.Day = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice ?? 0),
                        Count = salesOffers.Count(),
                    };
                }
                if (From != null && To != null)
                {
                    salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType.Contains("Internal Ticket") && a.CreationDate >= ((DateTime)From) && a.CreationDate <= ((DateTime)To)).AsQueryable();
                    response.Data.Duration = new InternalTicketDashboard()
                    {
                        Sum = salesOffers.Where(a => a.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice ?? 0) - salesOffers.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice ?? 0),
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

        public BaseResponseWithData<GetInternalTicketsByDepartmentResponse> GetInternalTicketsByTeams(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To)
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

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType == "Internal Ticket" || a.OfferType == "Internal Ticket return", includes: new[] { "SalesPerson.HrUserUsers.Team", "Client" });

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

                //var departments = list.GroupBy(a => a.SalesPerson.HrUserUsers?.FirstOrDefault()?.Team?.Name).ToList();

                //var data = departments.Select(a => new InternalTicketDepartments()
                //{
                //    Name = a.Key,
                //    Count = a.Count(),
                //    Sum = a.Where(b => b.OfferType == "Internal Ticket").Sum(b => b.FinalOfferPrice ?? 0) - a.Where(b => b.OfferType == "Internal Ticket return").Sum(b => b.FinalOfferPrice ?? 0),
                //    GroupedById = a.FirstOrDefault()?.SalesPerson?.Department?.Id ?? 0,
                //    DoctorsOfDepartmentList = a.GroupBy(x => x.SalesPerson).Select(x => new DoctorsOfCriteria()
                //    {
                //        DoctorId = x.Key.Id,
                //        DoctorName = x.Key.FirstName + " " + (x.Key.MiddleName != null ? x.Key.MiddleName + " " : "") + x.Key.LastName,
                //        DoctorImg = x.Key.PhotoUrl != null ? Globals.baseURL + x.Key.PhotoUrl : null,
                //        TotalSum = x.Where(a => a.OfferType == "Internal Ticket").Sum(b => b.FinalOfferPrice ?? 0) - x.Where(a => a.OfferType == "Internal Ticket return").Sum(b => b.FinalOfferPrice ?? 0),
                //        TicketsCount = x.Count(),
                //        PatientsCount = x.Select(b => b.Client).Distinct().Count()
                //    }).ToList()
                //}).ToList();

                //response.Data.InternalTicketDepartmentList = data;

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


        public BaseResponseWithData<GetInternalTicketsByCategoryResponse> GetInternalTicketsByCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To)
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

                var salesOffers = _unitOfWork.VSalesOffers.FindAllQueryable(a => a.OfferType == "Internal Ticket" || a.OfferType == "Internal Ticket return");



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
                    Sum = a.Where(b => b.OfferType == "Internal Ticket").Sum(b => b.FinalOfferPrice ?? 0) - a.Where(b => b.OfferType == "Internal Ticket return").Sum(b => b.FinalOfferPrice ?? 0),
                    GroupedById = a.FirstOrDefault()?.InventoryItemCategoryId ?? 0,
                    DoctorsOfDepartmentList = a.GroupBy(x => x.SalesPersonFullName).Select(x => new DoctorsOfCriteria()
                    {
                        DoctorId = x.FirstOrDefault().SalesPersonId,
                        DoctorName = x.Key,
                        DoctorImg = x.FirstOrDefault()?.SalesPersonPhoto != null ? Globals.baseURL + x.FirstOrDefault()?.SalesPersonPhoto : null,
                        TotalSum = x.Where(a => a.OfferType == "Internal Ticket").Sum(b => b.FinalOfferPrice ?? 0) - x.Where(a => a.OfferType == "Internal Ticket return").Sum(b => b.FinalOfferPrice ?? 0),
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


        public BaseResponseWithData<GetInternalTicketsByItemCategoryResponse> GetInternalTicketsByItemCategories(int year, int month, int day, string CreatorId, DateTime? From, DateTime? To)
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

                var salesOffers = _unitOfWork.SalesOffers.FindAllQueryable(a => a.OfferType == "Internal Ticket" || a.OfferType == "Internal Ticket return", includes: new[] { "SalesPerson" });



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
                        Sum = offers.Where(b => b.OfferType == "Internal Ticket").Sum(b => b.FinalOfferPrice ?? 0) - offers.Where(b => b.OfferType == "Internal Ticket return").Sum(b => b.FinalOfferPrice ?? 0),
                        GroupedById = a.FirstOrDefault()?.InventoryItemId ?? 0,
                        DoctorsOfDepartmentList = offers.GroupBy(x => x.SalesPerson).Select(x => new DoctorsOfCriteria()
                        {
                            DoctorId = x.FirstOrDefault().SalesPersonId,
                            DoctorName = x.Key.FirstName + " " + (x.Key.MiddleName != null ? x.Key.MiddleName + " " : "") + x.Key.LastName,
                            DoctorImg = x.Key.PhotoUrl != null ? Globals.baseURL + x.Key.PhotoUrl : null,
                            TotalSum = x.Where(a => a.OfferType == "Internal Ticket").Sum(b => b.FinalOfferPrice ?? 0) - x.Where(a => a.OfferType == "Internal Ticket return").Sum(b => b.FinalOfferPrice ?? 0),
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

        public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket(AddNewSalesOfferForInternalTicketRequest Request, string companyname, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime? StartDate = null;
                    DateTime StartDateTemp = DateTime.Now;

                    DateTime? EndDate = null;
                    DateTime EndDateTemp = DateTime.Now;

                    DateTime? ClientApprovalDate = null;
                    DateTime ClientApprovalDateTemp = DateTime.Now;

                    DateTime? OfferExpirationDate = null;
                    DateTime OfferExpirationDateTemp = DateTime.Now;

                    DateTime? ProjectStartDate = null;
                    DateTime ProjectStartDateTemp = DateTime.Now;

                    DateTime? ProjectEndDate = null;
                    DateTime ProjectEndDateTemp = DateTime.Now;

                    DateTime? RentStartDate = null;
                    DateTime RentStartDateTemp = DateTime.Now;

                    DateTime? RentEndDate = null;
                    DateTime RentEndDateTemp = DateTime.Now;

                    DateTime? ReminderDate = null;
                    DateTime ReminderDateTemp = DateTime.Now;

                    DateTime? SendingOfferDate = null;
                    DateTime SendingOfferDateTemp = DateTime.Now;

                    if (Request.SalesOffer != null)
                    {
                        if (Request.SalesOffer.StartDate == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Offer Start Date Is Mandatory";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            if (DateTime.TryParse(Request.SalesOffer.StartDate, out StartDateTemp))
                            {
                                StartDateTemp = DateTime.Parse(Request.SalesOffer.StartDate);
                                StartDate = StartDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid StartDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (Request.SalesOffer.SalesPersonId == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Offer Sales Person Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (Request.SalesOffer.BranchId == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Offer Branch Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.EndDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.EndDate, out EndDateTemp))
                            {
                                EndDateTemp = DateTime.Parse(Request.SalesOffer.EndDate);
                                EndDate = EndDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid EndDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ClientApprovalDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ClientApprovalDate, out ClientApprovalDateTemp))
                            {
                                ClientApprovalDateTemp = DateTime.Parse(Request.SalesOffer.ClientApprovalDate);
                                ClientApprovalDate = ClientApprovalDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ClientApprovalDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.OfferExpirationDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.OfferExpirationDate, out OfferExpirationDateTemp))
                            {
                                OfferExpirationDateTemp = DateTime.Parse(Request.SalesOffer.OfferExpirationDate);
                                OfferExpirationDate = OfferExpirationDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid OfferExpirationDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectStartDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ProjectStartDate, out ProjectStartDateTemp))
                            {
                                ProjectStartDateTemp = DateTime.Parse(Request.SalesOffer.ProjectStartDate);
                                ProjectStartDate = ProjectStartDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ProjectStartDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectEndDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ProjectEndDate, out ProjectEndDateTemp))
                            {
                                ProjectEndDateTemp = DateTime.Parse(Request.SalesOffer.ProjectEndDate);
                                ProjectEndDate = ProjectEndDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ProjectEndDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.RentStartDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.RentStartDate, out RentStartDateTemp))
                            {
                                RentStartDateTemp = DateTime.Parse(Request.SalesOffer.RentStartDate);
                                RentStartDate = RentStartDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid RentStartDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.RentEndDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.RentEndDate, out RentEndDateTemp))
                            {
                                RentEndDateTemp = DateTime.Parse(Request.SalesOffer.RentEndDate);
                                RentEndDate = RentEndDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid RentEndDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ReminderDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ReminderDate, out ReminderDateTemp))
                            {
                                ReminderDateTemp = DateTime.Parse(Request.SalesOffer.ReminderDate);
                                ReminderDate = ReminderDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ReminderDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.SendingOfferDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.SendingOfferDate, out SendingOfferDateTemp))
                            {
                                SendingOfferDateTemp = DateTime.Parse(Request.SalesOffer.SendingOfferDate);
                                SendingOfferDate = SendingOfferDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid SendingOfferDate";
                                Response.Errors.Add(error);
                            }
                        }


                        // Modified By michael markos 2022-10-25
                        if (Request.SalesOffer.ParentSalesOfferID != null)
                        {
                            // check if this Offer is Found
                            var SalesOfferObj = _unitOfWork.SalesOffers.FindAll(x => x.Id == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
                            if (SalesOfferObj == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Return Sales Offer!";
                                Response.Errors.Add(error);
                            }

                            if (Request.SalesOffer.OfferType != "Internal Ticket return")
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Return Sales Offer Type Must Be Sales Return!";
                                Response.Errors.Add(error);
                            }


                            // If Returned Only
                            if (Request.SalesOffer.FinalOfferPrice != null && Request.SalesOffer.FinalOfferPrice > 0) // check  -- Refund must be less than final offer price
                            {
                                // Check if refunded before (offer returned)
                                var OffersReturned = _unitOfWork.InvoiceCnandDns.FindAll(x => x.ParentSalesOfferId == Request.SalesOffer.ParentSalesOfferID);
                                decimal TotalRefundCost = 0;
                                if (OffersReturned.Count() > 0)
                                {
                                    var ListOfferChildrenIds = OffersReturned.Select(x => x.SalesOfferId).ToList();
                                    var ListOfferRefunded = _unitOfWork.SalesOffers.FindAll(x => ListOfferChildrenIds.Contains(x.Id));
                                    TotalRefundCost = ListOfferRefunded.Count() > 0 ? ListOfferRefunded.Select(x => x.FinalOfferPrice).DefaultIfEmpty(0).Sum() ?? 0 : 0;
                                }
                                if (Request.SalesOffer.FinalOfferPrice > (SalesOfferObj.FinalOfferPrice - TotalRefundCost))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "Refund Cost for this ticket must be less than Total Remain Ticket !";
                                    Response.Errors.Add(error);
                                }
                            }

                        }

                        if (Request.SalesOffer.ParentInvoiceID != null)
                        {
                            // check if this Offer is Found
                            var InvoicesObj = _unitOfWork.Invoices.FindAll(x => x.Id == Request.SalesOffer.ParentInvoiceID).FirstOrDefault();
                            if (InvoicesObj == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Return Invoice!";
                                Response.Errors.Add(error);
                            }
                        }

                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Sales Offer Data!!";
                        Response.Errors.Add(error);
                    }
                    if (Request.SalesOfferProductList == null || Request.SalesOfferProductList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "must be one item at least in offer";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.SalesOfferProductList != null)
                    {
                        if (Request.SalesOfferProductList.Count() > 0)
                        {

                            var InventoryItemListIDS = Request.SalesOfferProductList.Select(x => x.InventoryItemId).ToList();

                            int Counter = 0;
                            foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                            {
                                Counter++;
                                if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID > 0)
                                {

                                    var ParentProductcDb = _unitOfWork.SalesOfferProducts.Find((x => x.OfferId == Request.SalesOffer.ParentSalesOfferID));
                                    if (ParentProductcDb != null)
                                    {

                                        // for internal ticket
                                        //if (SalesOfferProduct.Quantity != null && SalesOfferProduct.Quantity > 0)
                                        //{
                                        //    if (SalesOfferProduct.Quantity > (ParentProductcDb.RemainQty ?? ParentProductcDb.Quantity ?? 0))
                                        //    {
                                        //        Response.Result = false;
                                        //        Error error = new Error();
                                        //        error.ErrorCode = "Err-P12";
                                        //        error.ErrorMSG = "Returned Quantity For Sales Offer Product: " + SalesOfferProduct.ParentOfferProductId + " Cannot be Greater Than Remain Quantity Of Parent Product!";
                                        //        Response.Errors.Add(error);
                                        //    }
                                        //}

                                        // Validate if Return => if have balance from parent or not


                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "This Sales Offer Product " + SalesOfferProduct.ParentOfferProductId + " Doesn't Exist!";
                                        Response.Errors.Add(error);
                                    }

                                }

                                long InventoryItemId = 0;
                                if (SalesOfferProduct.InventoryItemId == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "Invalid InventoryItemId on Item No #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    InventoryItemId = (long)SalesOfferProduct.InventoryItemId;
                                }
                                decimal Quantity = 0;
                                if (SalesOfferProduct.Quantity == null || SalesOfferProduct.Quantity <= 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "Invalid Sales Offer Product Quantity on Item No #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    Quantity = (decimal)SalesOfferProduct.Quantity;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Request.SalesOffer != null)
                        {
                            if (Request.SalesOffer.Id == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Sales Offer Product";
                                Response.Errors.Add(error);

                            }
                        }
                    }

                    // Get the timezone information for Egypt
                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    // Get the current datetime in Egypt
                    DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

                    if (Response.Result)
                    {
                        long SalesOfferId = 0;
                        // Add-Edit Sales Offer
                        if (Request.SalesOffer.Id == null || Request.SalesOffer.Id == 0)
                        {

                            var NewOfferSerial = "";
                            var OfferSerialSubString = "";

                            //long newOfferNumber = 0;
                            long CountOfSalesOfferThisYear = _unitOfWork.SalesOffers.Count(x => x.Active == true && x.OfferSerial.Contains(System.DateTime.Now.Year.ToString()) &&
                            (x.OfferType == "Internal Ticket return" || x.OfferType == "Internal Ticket"));

                            NewOfferSerial += "#" + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();

                            // Insert
                            var NewSalesOfferInsert = new SalesOffer()
                            {
                                StartDate = DateOnly.FromDateTime((DateTime)StartDate),
                                EndDate = DateOnly.FromDateTime((DateTime)EndDate),
                                Note = string.IsNullOrEmpty(Request.SalesOffer.Note) ? null : Request.SalesOffer.Note,
                                SalesPersonId = Request.SalesOffer.SalesPersonId,
                                CreatedBy = creator,
                                CreationDate = egyptDateTime,
                                ModifiedBy = null,
                                Modified = null,
                                Active = true,
                                Status = Request.SalesOffer.Status,
                                Completed = Request.SalesOffer.Completed,
                                TechnicalInfo = string.IsNullOrEmpty(Request.SalesOffer.TechnicalInfo) ? null : Request.SalesOffer.TechnicalInfo,
                                ProjectData = string.IsNullOrEmpty(Request.SalesOffer.ProjectData) ? null : Request.SalesOffer.ProjectData,
                                FinancialInfo = string.IsNullOrEmpty(Request.SalesOffer.FinancialInfo) ? null : Request.SalesOffer.FinancialInfo,
                                PricingComment = string.IsNullOrEmpty(Request.SalesOffer.PricingComment) ? null : Request.SalesOffer.PricingComment,
                                OfferAmount = Request.SalesOffer.OfferAmount == null ? null : Request.SalesOffer.OfferAmount,
                                SendingOfferConfirmation = Request.SalesOffer.SendingOfferConfirmation,
                                SendingOfferDate = SendingOfferDate ?? egyptDateTime,
                                SendingOfferBy = Request.SalesOffer.SendingOfferBy,
                                SendingOfferTo = Request.SalesOffer.SendingOfferTo,
                                SendingOfferComment = Request.SalesOffer.SendingOfferComment,
                                ClientApprove = Request.SalesOffer.ClientApprove,
                                ClientComment = Request.SalesOffer.ClientComment,
                                VersionNumber = Request.SalesOffer.VersionNumber,
                                ClientApprovalDate = ClientApprovalDate ?? egyptDateTime,
                                ClientId = Request.SalesOffer.ClientId,
                                ProductType = Request.SalesOffer.ProductType,
                                ProjectName = string.IsNullOrEmpty(Request.SalesOffer.ProjectName) ? NewOfferSerial : Request.SalesOffer.ProjectName,
                                ProjectLocation = Request.SalesOffer.ProjectLocation,
                                ContactPersonMobile = Request.SalesOffer.ContactPersonMobile,
                                ContactPersonEmail = Request.SalesOffer.ContactPersonEmail,
                                ContactPersonName = Request.SalesOffer.ContactPersonName,
                                ProjectStartDate = ProjectStartDate ?? egyptDateTime,
                                ProjectEndDate = ProjectEndDate ?? egyptDateTime,
                                BranchId = Request.SalesOffer.BranchId,
                                OfferType = Request.SalesOffer.OfferType,
                                ContractType = Request.SalesOffer.ContractType,
                                OfferSerial = NewOfferSerial,
                                ClientNeedsDiscount = Request.SalesOffer.ClientNeedsDiscount,
                                RejectionReason = Request.SalesOffer.RejectionReason,
                                NeedsInvoice = Request.SalesOffer.NeedsInvoice,
                                NeedsExtraCost = Request.SalesOffer.NeedsExtraCost,
                                OfferExpirationDate = OfferExpirationDate,
                                OfferExpirationPeriod = Request.SalesOffer.OfferExpirationPeriod,
                                ExtraOrDiscountPriceBySalesPerson = Request.SalesOffer.ExtraOrDiscountPriceBySalesPerson,
                                FinalOfferPrice = Request.SalesOffer.FinalOfferPrice,
                                ReminderDate = ReminderDate
                            };
                            _unitOfWork.SalesOffers.Add(NewSalesOfferInsert);
                            var SalesOfferInsert = _unitOfWork.Complete();

                            if (SalesOfferInsert != 0 && NewSalesOfferInsert.Id != 0)
                            {
                                SalesOfferId = (long)NewSalesOfferInsert.Id;
                                Response.Result = true;
                                Response.ID = SalesOfferId;
                                if (Request.SalesOffer.ParentSalesOfferID != null)
                                {
                                    long? ParentInvoiceID = Request.SalesOffer.ParentInvoiceID;
                                    if (ParentInvoiceID == null)
                                    {
                                        var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
                                        if (InvoiceDB != null)
                                        {
                                            ParentInvoiceID = InvoiceDB.Id;
                                        }
                                    }
                                    if (ParentInvoiceID != null)
                                    {
                                        // Modified By Michael Markos 2022-10-25
                                        // Add in table Invoice CN And DN
                                        var InvoiceCNAndDNObj = new InvoiceCnandDn();
                                        InvoiceCNAndDNObj.ParentSalesOfferId = (long)Request.SalesOffer.ParentSalesOfferID;
                                        InvoiceCNAndDNObj.ParentInvoiceId = (long)ParentInvoiceID;
                                        InvoiceCNAndDNObj.SalesOfferId = SalesOfferId;
                                        InvoiceCNAndDNObj.Active = true;
                                        InvoiceCNAndDNObj.CreatedBy = creator; //CreatedBy ?? 1;
                                        InvoiceCNAndDNObj.CreationDate = egyptDateTime;
                                        InvoiceCNAndDNObj.ModifiedBy = creator; // CreatedBy ?? 1;
                                        InvoiceCNAndDNObj.ModificationDate = egyptDateTime;

                                        _unitOfWork.InvoiceCnandDns.Add(InvoiceCNAndDNObj);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Offer!!";
                                Response.Errors.Add(error);
                            }

                            //var SalesOfferInvoices = _Context.Invoices.Where(a => a.SalesOfferId == SalesOfferId).Count();
                            // Add-Edit Sales Offer Product
                            if (Request.SalesOfferProductList != null)
                            {
                                if (Request.SalesOfferProductList.Count() > 0)
                                {
                                    foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                                    {
                                        decimal TempProfitPercentage;
                                        decimal? ProfitPercentage = decimal.TryParse(SalesOfferProduct.ProfitPercentage, out TempProfitPercentage) ? TempProfitPercentage : (decimal?)null;
                                        // Anton samir in 2025-2-27 remove this

                                        //string ItemComment = null; 
                                        //if (string.IsNullOrEmpty(SalesOfferProduct.ItemPricingComment))
                                        //{
                                        //    var inventoryItemDb = _unitOfWork.InventoryItems.GetById((long)SalesOfferProduct.InventoryItemId);
                                        //    if (inventoryItemDb != null)
                                        //    {
                                        //        if (string.IsNullOrEmpty(inventoryItemDb.Description))
                                        //        {
                                        //            ItemComment = inventoryItemDb.Name;
                                        //        }
                                        //        else
                                        //        {
                                        //            ItemComment = inventoryItemDb.Description;
                                        //        }
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    ItemComment = SalesOfferProduct.ItemPricingComment;
                                        //}
                                        string ItemComment = SalesOfferProduct.ItemPricingComment;
                                        // Add-Edit Sales Offer
                                        if (SalesOfferProduct.Id == null || SalesOfferProduct.Id == 0)
                                        {

                                            // Insert
                                            //ObjectParameter SalesOfferProductInsertedId = new ObjectParameter("ID", typeof(long));
                                            var NewSalesOfferProductInsert = new SalesOfferProduct()
                                            {
                                                CreatedBy = creator, // long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key)),
                                                CreationDate = egyptDateTime,
                                                ModifiedBy = null,
                                                Modified = null,
                                                Active = true,
                                                OfferId = SalesOfferId,
                                                ProductId = SalesOfferProduct.ProductId,
                                                ProductGroupId = SalesOfferProduct.ProductGroupId,
                                                Quantity = SalesOfferProduct.Quantity,
                                                InventoryItemId = SalesOfferProduct.InventoryItemId,
                                                InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId,
                                                ItemPrice = SalesOfferProduct.ItemPrice,
                                                ItemPricingComment = ItemComment,
                                                ConfirmReceivingQuantity = null,
                                                ConfirmReceivingComment = null,
                                                InvoicePayerClientId = SalesOfferProduct.InvoicePayerClientId,
                                                DiscountPercentage = SalesOfferProduct.DiscountPercentage,
                                                DiscountValue = SalesOfferProduct.DiscountValue,
                                                FinalPrice = SalesOfferProduct.FinalPrice,
                                                TaxPercentage = SalesOfferProduct.TaxPercentage,
                                                TaxValue = SalesOfferProduct.TaxValue,
                                                ReturnedQty = 0,
                                                RemainQty = SalesOfferProduct.Quantity,
                                                ProfitPercentage = ProfitPercentage,
                                                ReleasedQty = null
                                            };
                                            var SalesOfferProductInsert = _unitOfWork.SalesOfferProducts.Add(NewSalesOfferProductInsert);
                                            _unitOfWork.Complete();
                                            if (SalesOfferProductInsert != null)
                                            {
                                                SalesOfferProduct.Id = (long)SalesOfferProductInsert.Id;

                                                if (SalesOfferProduct.ParentOfferProductId != null && SalesOfferProduct.ParentOfferProductId != 0) // Client Return
                                                {
                                                    var ParentSalesOfferProductDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.Id == (long)SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
                                                    if (ParentSalesOfferProductDb != null)
                                                    {
                                                        ParentSalesOfferProductDb.RemainQty -= SalesOfferProduct.Quantity;
                                                        ParentSalesOfferProductDb.ReturnedQty += SalesOfferProduct.Quantity;
                                                        _unitOfWork.Complete();
                                                    }

                                                }

                                                Response.Result = true;
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Faild To Insert this Offer!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                    }


                                    //When change PayerId Check Deleted Invoices And Inserted Invoices Then Delete Old and Insert New
                                    var clientsIds = Request.SalesOfferProductList.Where(a => a.Active == true).Select(a => a.InvoicePayerClientId).Distinct().ToList();

                                    var InvoicesToInsertIds = clientsIds.ToList();


                                    //Insert 

                                    if (InvoicesToInsertIds != null && InvoicesToInsertIds.Count > 0)
                                    {
                                        foreach (var clientId in InvoicesToInsertIds)
                                        {
                                            var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
                                            var OfferClientApprovalDate = SalesOfferDb.ClientApprovalDate;
                                            DateTime InvoiceDate = OfferClientApprovalDate ?? egyptDateTime;
                                            long InvoiceId = 0;
                                            //ObjectParameter InvoiceInsertedId = new ObjectParameter("ID", typeof(long));
                                            var NewInvoiceInsert = new Invoice()
                                            {
                                                Serial = "1",
                                                Revision = 0,
                                                InvoiceDate = InvoiceDate,
                                                InvoiceType = "1",
                                                ClientId = clientId,
                                                CreatedBy = creator,
                                                ModifiedBy = creator,
                                                CreationDate = egyptDateTime,
                                                ModificationDate = egyptDateTime,
                                                Active = true,
                                                IsClosed = false,
                                                CreationType = null,
                                                InvoiceFor = null,
                                                EInvoiceId = null,
                                                EInvoiceStatus = null,
                                                EInvoiceAcceptDate = null,
                                                SalesOfferId = SalesOfferId,
                                                EInvoiceJsonBody = null,
                                                EInvoiceRequestToSend = null,
                                            };
                                            var InvoiceInsert = _unitOfWork.Invoices.Add(NewInvoiceInsert);
                                            _unitOfWork.Complete();

                                            if (InvoiceInsert != null)
                                            {
                                                InvoiceId = (long)InvoiceInsert.Id;
                                                int SerialTemp = 0;
                                                var SerialList = _unitOfWork.Invoices.FindAll(x => x.Active == true).Select(x => x.Serial).ToList();
                                                int Serial = SerialList.Where(x => int.TryParse(x, out SerialTemp)).OrderByDescending(x => int.Parse(x)).Select(x => int.Parse(x)).FirstOrDefault();
                                                //int SerialNo = string.IsNullOrEmpty(Serial) && int.TryParse(Serial, out SerialNo) ? 1 : int.Parse(Serial) + 1;
                                                var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.Id == InvoiceId).FirstOrDefault();
                                                if (InvoiceDB != null)
                                                {
                                                    InvoiceDB.Serial = (Serial + 1).ToString();
                                                    _unitOfWork.Complete();
                                                }
                                                var ClientInvoicesItemList = _unitOfWork.SalesOfferProducts.FindAll(x => x.OfferId == SalesOfferId && x.InvoicePayerClientId == clientId).ToList();
                                                // Insert Into Invoice Items
                                                if (ClientInvoicesItemList.Count > 0)
                                                {
                                                    foreach (var invoiceItem in ClientInvoicesItemList)
                                                    {
                                                        //ObjectParameter InvoiceItemInsertedId = new ObjectParameter("ID", typeof(long));          
                                                        _unitOfWork.InvoiceItems.Add(new InvoiceItem()
                                                        {
                                                            InvoiceId = InvoiceId,
                                                            SalesOfferProductId = invoiceItem.Id,
                                                            Comments = null,
                                                            EInvoiceId = null,
                                                            EInvoiceStatus = null,
                                                            EInvoiceAcceptDate = null
                                                        });
                                                        _unitOfWork.Complete();

                                                    }
                                                }

                                            }
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Invalid Id for SalesOffer Internal Ticket ";
                            Response.Errors.Add(error);
                            return Response;
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }



        public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForInternalTicket(GetSalesOfferInternalTicketListFilters filters, string OfferStatusParam)
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
                        var TotalOffersPriceWithoutReturn = SalesOfferDBQuery.Where(x => x.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice) ?? 0;

                        var TotalOffersReturnedPrice = SalesOfferDBQuery.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice) ?? 0;
                        var TotalOffersPrice = TotalOffersPriceWithoutReturn - TotalOffersReturnedPrice;
                        SalesOfferForInternalTicket salesOfferTypeDetails = new SalesOfferForInternalTicket()
                        {
                            TotalOffersCount = TotalOffersCount,
                            TotalOffersPrice = TotalOffersPrice
                        };

                        var OffersListResponse = new List<SalesOfferDetailsForInternalTicket>();
                        var IDSSalesOffer = OffersListDB.Select(x => x.Id).ToList();
                        var ParentSalesOfferListDB = _unitOfWork.InvoiceCnandDns.FindAll((x => IDSSalesOffer.Contains(x.SalesOfferId) || IDSSalesOffer.Contains(x.ParentSalesOfferId)), new[] { "SalesOffer", "ParentSalesOffer" }).ToList();
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

                            long? ParentSalesOfferId = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOfferId).FirstOrDefault();
                            string ParentSalesOfferSErial = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOffer?.OfferSerial).FirstOrDefault();
                            var ListChildrenSalesOffer = ParentSalesOfferListDB.Where(x => x.ParentSalesOfferId == offer.Id).Select(x => new ChildrenSalesOfferInternalTicket { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial, CreationDate = x.SalesOffer?.CreationDate.ToShortDateString(), TotalPrice = x.SalesOffer?.FinalOfferPrice }).ToList();

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


        public GetSalesOfferDetailsForInternalTicketResponse GetSalesOfferDetailsForInternalTicket(long SalesOfferId)
        {
            GetSalesOfferDetailsForInternalTicketResponse Response = new GetSalesOfferDetailsForInternalTicketResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var SalesOffer = GetSalesOfferInfo(SalesOfferId);
                    if (SalesOffer.Id != null)
                    {
                        Response.SalesOfferDetails = SalesOffer;
                        Response.SalesOfferProducts = GetSalesOfferProductsList(SalesOfferId);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "This Sales Offer Doesn't Exist!!";
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

        public SalesOfferDetailsForInternalTicket GetSalesOfferInfo(long SalesOfferId)
        {

            var SalesOfferObj = new SalesOfferDetailsForInternalTicket();
            var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId, includes: new[] { "SalesPerson", "CreatedByNavigation" }).FirstOrDefault();
            if (SalesOfferDb != null)
            {
                SalesOfferObj.Id = SalesOfferDb.Id;
                SalesOfferObj.SalesPersonId = SalesOfferDb.SalesPersonId;
                if (SalesOfferDb.SalesPersonId != 0)
                {
                    var Hruser = _unitOfWork.HrUsers.FindAll(a => a.UserId == SalesOfferDb.SalesPersonId, includes: new[] { "Team" }).FirstOrDefault();
                    if (Hruser != null)
                    {
                        //SalesOfferObj.TeamId = Hruser?.TeamId; // Default 
                        //SalesOfferObj.TeamName = Hruser.Team?.Name;
                    }
                }
                SalesOfferObj.SalesPersonName = SalesOfferDb.SalesPerson.FirstName + ' ' + SalesOfferDb.SalesPerson.MiddleName + ' ' + SalesOfferDb.SalesPerson.LastName;
                SalesOfferObj.CreatorName = SalesOfferDb.CreatedByNavigation?.FirstName + " " + SalesOfferDb.CreatedByNavigation?.LastName;
                if (SalesOfferDb.ClientId != null)
                {
                    var ClientDb = _unitOfWork.Clients.FindAll(a => a.Id == (long)SalesOfferDb.ClientId).FirstOrDefault();
                    SalesOfferObj.ClientId = SalesOfferDb.ClientId;
                    SalesOfferObj.ClientName = ClientDb.Name;
                }
                SalesOfferObj.ContactPersonName = SalesOfferDb.ContactPersonName;
                SalesOfferObj.OfferSerial = SalesOfferDb.OfferSerial;
                SalesOfferObj.CreationDate = SalesOfferDb.CreationDate.ToShortDateString();
                SalesOfferObj.CreationTime = SalesOfferDb.CreationDate.ToShortTimeString();
                SalesOfferObj.FinalOfferPrice = SalesOfferDb.FinalOfferPrice;
                SalesOfferObj.RemainPrice = SalesOfferDb.FinalOfferPrice ?? 0;

                var CheckChildrenOrParentsList = _unitOfWork.InvoiceCnandDns.FindAll(x => x.ParentSalesOfferId == SalesOfferId || x.SalesOfferId == SalesOfferId, includes: new[] { "SalesOffer", "ParentSalesOffer" });


                var OfferChildrenList = CheckChildrenOrParentsList.Where(x => x.ParentSalesOfferId == SalesOfferId).ToList();
                if (OfferChildrenList.Count() > 0)   // This Offer is Parent
                {

                    SalesOfferObj.ChildrenSalesOfferList = OfferChildrenList.Select(x => new ChildrenSalesOfferInternalTicket { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial, CreationDate = x.SalesOffer?.CreationDate.ToShortDateString(), TotalPrice = x.SalesOffer?.FinalOfferPrice }).ToList();

                    var TotalRefundCost = SalesOfferObj.ChildrenSalesOfferList.Select(x => x.TotalPrice).DefaultIfEmpty(0).Sum();
                    SalesOfferObj.RemainPrice = (SalesOfferDb.FinalOfferPrice ?? 0) - (TotalRefundCost ?? 0);
                }
                else
                {
                    var ParentSalesOfferDB = CheckChildrenOrParentsList.Where((x => x.SalesOfferId == SalesOfferDb.Id)).FirstOrDefault();
                    if (ParentSalesOfferDB != null)
                    {
                        SalesOfferObj.ParentSalesOfferID = ParentSalesOfferDB.ParentSalesOfferId;
                        SalesOfferObj.ParentSalesOfferSerial = ParentSalesOfferDB.ParentSalesOffer?.OfferSerial;


                        var TotalRefundCost = CheckChildrenOrParentsList.Where(x => x.ParentSalesOfferId == SalesOfferObj.ParentSalesOfferID).Select(x => x.SalesOffer?.FinalOfferPrice).DefaultIfEmpty(0).Sum();
                        SalesOfferObj.RemainPrice = (ParentSalesOfferDB.ParentSalesOffer?.FinalOfferPrice ?? 0) - (TotalRefundCost ?? 0);
                    }

                }
            }
            return SalesOfferObj;
        }


        public List<SalesOfferProductForInternalTicket> GetSalesOfferProductsList(long SalesOfferId)
        {
            var SalesOfferProducts = new List<SalesOfferProductForInternalTicket>();
            var SalesOfferProductsDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == SalesOfferId && a.Active == true, new[] { "InventoryItem", "CreatedByNavigation", "ModifiedByNavigation" }).ToList();
            if (SalesOfferProductsDb.Count > 0)
            {
                SalesOfferProducts = SalesOfferProductsDb.Select(product => new SalesOfferProductForInternalTicket
                {
                    Id = product.Id,
                    CreatedBy = product.CreatedByNavigation != null ? product.CreatedByNavigation?.FirstName + " " + product.CreatedByNavigation?.LastName : null,
                    ModifiedBy = product.ModifiedByNavigation != null ? product.ModifiedByNavigation?.FirstName + " " + product.ModifiedByNavigation?.LastName : null,
                    InventoryItemCategoryId = product.InventoryItemCategoryId ?? null,
                    InventoryItemCategoryName = product.InventoryItemCategoryId != null ? _unitOfWork.InventoryItemCategories.GetById((int)product.InventoryItemCategoryId)?.Name : null,
                    InventoryItemId = product.InventoryItemId ?? null,
                    InventoryItemName = product.InventoryItemId != null && product.InventoryItem != null ? product.InventoryItem.Name : null, //_unitOfWork.InventoryItems.GetById((long)product.InventoryItemId)?.Name : null,
                    ItemPrice = product.ItemPrice ?? null,
                    ItemPricingComment = product.ItemPricingComment ?? null
                }).ToList();
            }

            return SalesOfferProducts;
        }



        public async Task<BaseResponseWithData<string>> GetSalesOfferTicketsForStoreForAllUsersPDF(InternalticketheaderPdf header, long createdBy)
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



                var salesOffersData = _unitOfWork.SalesOffers.FindAllQueryable(a => a.CreationDate.Date >= startDate.Date && a.CreationDate.Date <= endDate.Date && a.OfferType.Contains("ticket"), new[] { "SalesOfferProducts", "CreatedByNavigation", "Client", "CreatedByNavigation" });

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

                var minusSummation = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(a => a.FinalOfferPrice);
                var plusSummationu = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket")).Sum(a => a.FinalOfferPrice);
                var totalSummationu = plusSummationu - minusSummation;


                //-------------------------------------------------------------------------------------------


                var StartYear = new DateTime(2000, 1, 1);
                var AccountAdvancedType = _Context.VAccounts.ToList().GroupBy(a => a.AdvanciedTypeId);
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
                            if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSum -= salesOffer.FinalOfferPrice ?? 0;
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
                            if (salesOffer.OfferType.Contains("Internal Ticket return")) { sum = sum - salesOffer.FinalOfferPrice ?? 0; }
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

        public BaseResponseWithData<string> GetSalesOfferTicketsForStore(string From, string To, string UserId, string CompName, long createdBy)
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



                var salesOffersData = _unitOfWork.SalesOffers.FindAllQueryable(a => a.CreationDate >= startDate && a.CreationDate <= endDate.Date && a.OfferType.Contains("ticket"),
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
                var t5 = salesOffers.Where(a => a.CreatedBy == 20431 && a.OfferType.ToLower() == "internal ticket").ToList().Sum(b => b.FinalOfferPrice);
                var minusSummation = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(a => a.FinalOfferPrice);
                var plusSummationu = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket")).Sum(a => a.FinalOfferPrice);
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
                    
                    var finalOfferPricesForTickets = group.Where(a => a.OfferType.Contains("Internal Ticket") && !a.OfferType.Contains("Internal Ticket return")).Sum(a => a.FinalOfferPrice);
                    var finalOfferPricesForTicketsReturn = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(a => a.FinalOfferPrice);


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
                                if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSum -= salesOffer.FinalOfferPrice ?? 0;
                                else { cashSum += salesOffer.FinalOfferPrice ?? 0; }


                                if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
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
                                    sheet.Cells[rowNum, 4].Value = productDB.InventoryItemCategory.Name;
                                    sheet.Cells[rowNum, 13].Value = productDB.InventoryItem.Name;
                                    sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                                    //rowNum++;
                                }
                                if (salesOffer.OfferType.Contains("Internal Ticket return")) { sum = sum - salesOffer.FinalOfferPrice ?? 0; }
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

                var salesOffersReturn = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket return")).GroupBy(b => new { b.CreatedBy }).ToList();

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
                                if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSumReturn -= salesOffer.FinalOfferPrice ?? 0;
                                else { cashSumReturn += salesOffer.FinalOfferPrice ?? 0; }


                                if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
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
                                    sheet4.Cells[rowNumReturn, 4].Value = productDB.InventoryItemCategory.Name;
                                    sheet4.Cells[rowNumReturn, 13].Value = productDB.InventoryItem.Name;
                                    sheet4.Row(rowNumReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet4.Row(rowNumReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                                    //rowNumReturn++;
                                }
                                if (salesOffer.OfferType.Contains("Internal Ticket return")) { sumReturn = sumReturn - salesOffer.FinalOfferPrice ?? 0; }
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

                    var salesOfferList = _unitOfWork.SalesOffers.FindAll(a => a.CreationDate >= startDate && a.CreationDate <= endDate && a.OfferType.Contains("Ticket"));

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

                            var categoryDayTotalSales = salesOfferProduct.Where(a => a.Offer.OfferType.Contains("Internal Ticket") && !a.Offer.OfferType.Contains("Internal Ticket return") && a.CreationDate >= curserDate && a.CreationDate < curserDate.AddDays(1) && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

                            var categoryDayTotalReturn = salesOfferProduct.Where(a => a.Offer.OfferType.Contains("Internal Ticket return") && a.CreationDate >= curserDate && a.CreationDate < curserDate.AddDays(1) && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

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

                        var totalAmount = group.Where(a => a.OfferType.Contains("Internal Ticket") && !a.OfferType.Contains("Internal Ticket return")).Sum(b => b.FinalOfferPrice);
                        var totalReturnAmount = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(b => b.FinalOfferPrice);

                        var netAmount = totalAmount - totalReturnAmount;


                        var currentNumOfTickets = group.Count();
                        var currentTotalTicketsPrice = group.Where(a => a.OfferType.Contains("Internal Ticket") && !a.OfferType.Contains("Internal Ticket return")).Sum(b => b.FinalOfferPrice);
                        var currentTotalNumOfReturnTickets = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Count();
                        var currentTotalReturns = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(b => b.FinalOfferPrice); 

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
                        var totalReturnAmount = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(b => b.FinalOfferPrice);

                        var netAmount = totalAmount - totalReturnAmount;


                        var currentNumOfTickets = group.Count();
                        var currentTotalTicketsPrice = group.Sum(b => b.FinalOfferPrice);
                        var currentTotalNumOfReturnTickets = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Count();
                        var currentTotalReturns = group.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(b => b.FinalOfferPrice);

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
                };

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
        #region Old Service before Merge

            //public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket(AddNewSalesOfferForInternalTicketRequest Request, string companyname, long creator)
            //{
            //    BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            //    Response.Result = true;
            //    Response.Errors = new List<Error>();
            //    try
            //    {

            //        if (Response.Result)
            //        {
            //            //check sent data
            //            if (Request == null)
            //            {
            //                Response.Result = false;
            //                Error error = new Error();
            //                error.ErrorCode = "Err-P12";
            //                error.ErrorMSG = "Please Insert a Valid Data.";
            //                Response.Errors.Add(error);
            //                return Response;
            //            }

            //            DateTime? StartDate = null;
            //            DateTime StartDateTemp = DateTime.Now;

            //            DateTime? EndDate = null;
            //            DateTime EndDateTemp = DateTime.Now;

            //            DateTime? ClientApprovalDate = null;
            //            DateTime ClientApprovalDateTemp = DateTime.Now;

            //            DateTime? OfferExpirationDate = null;
            //            DateTime OfferExpirationDateTemp = DateTime.Now;

            //            DateTime? ProjectStartDate = null;
            //            DateTime ProjectStartDateTemp = DateTime.Now;

            //            DateTime? ProjectEndDate = null;
            //            DateTime ProjectEndDateTemp = DateTime.Now;

            //            DateTime? RentStartDate = null;
            //            DateTime RentStartDateTemp = DateTime.Now;

            //            DateTime? RentEndDate = null;
            //            DateTime RentEndDateTemp = DateTime.Now;

            //            DateTime? ReminderDate = null;
            //            DateTime ReminderDateTemp = DateTime.Now;

            //            DateTime? SendingOfferDate = null;
            //            DateTime SendingOfferDateTemp = DateTime.Now;

            //            if (Request.SalesOffer != null)
            //            {
            //                if (Request.SalesOffer.StartDate == null)
            //                {
            //                    Response.Result = false;
            //                    Error error = new Error();
            //                    error.ErrorCode = "Err25";
            //                    error.ErrorMSG = "Sales Offer Start Date Is Mandatory";
            //                    Response.Errors.Add(error);
            //                }
            //                else
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.StartDate, out StartDateTemp))
            //                    {
            //                        StartDateTemp = DateTime.Parse(Request.SalesOffer.StartDate);
            //                        StartDate = StartDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid StartDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (Request.SalesOffer.SalesPersonId == 0)
            //                {
            //                    Response.Result = false;
            //                    Error error = new Error();
            //                    error.ErrorCode = "Err25";
            //                    error.ErrorMSG = "Sales Offer Sales Person Id Is Mandatory";
            //                    Response.Errors.Add(error);
            //                }

            //                if (Request.SalesOffer.BranchId == 0)
            //                {
            //                    Response.Result = false;
            //                    Error error = new Error();
            //                    error.ErrorCode = "Err25";
            //                    error.ErrorMSG = "Sales Offer Branch Id Is Mandatory";
            //                    Response.Errors.Add(error);
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.EndDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.EndDate, out EndDateTemp))
            //                    {
            //                        EndDateTemp = DateTime.Parse(Request.SalesOffer.EndDate);
            //                        EndDate = EndDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid EndDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.ClientApprovalDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.ClientApprovalDate, out ClientApprovalDateTemp))
            //                    {
            //                        ClientApprovalDateTemp = DateTime.Parse(Request.SalesOffer.ClientApprovalDate);
            //                        ClientApprovalDate = ClientApprovalDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid ClientApprovalDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.OfferExpirationDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.OfferExpirationDate, out OfferExpirationDateTemp))
            //                    {
            //                        OfferExpirationDateTemp = DateTime.Parse(Request.SalesOffer.OfferExpirationDate);
            //                        OfferExpirationDate = OfferExpirationDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid OfferExpirationDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectStartDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.ProjectStartDate, out ProjectStartDateTemp))
            //                    {
            //                        ProjectStartDateTemp = DateTime.Parse(Request.SalesOffer.ProjectStartDate);
            //                        ProjectStartDate = ProjectStartDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid ProjectStartDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectEndDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.ProjectEndDate, out ProjectEndDateTemp))
            //                    {
            //                        ProjectEndDateTemp = DateTime.Parse(Request.SalesOffer.ProjectEndDate);
            //                        ProjectEndDate = ProjectEndDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid ProjectEndDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.RentStartDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.RentStartDate, out RentStartDateTemp))
            //                    {
            //                        RentStartDateTemp = DateTime.Parse(Request.SalesOffer.RentStartDate);
            //                        RentStartDate = RentStartDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid RentStartDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.RentEndDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.RentEndDate, out RentEndDateTemp))
            //                    {
            //                        RentEndDateTemp = DateTime.Parse(Request.SalesOffer.RentEndDate);
            //                        RentEndDate = RentEndDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid RentEndDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.ReminderDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.ReminderDate, out ReminderDateTemp))
            //                    {
            //                        ReminderDateTemp = DateTime.Parse(Request.SalesOffer.ReminderDate);
            //                        ReminderDate = ReminderDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid ReminderDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //                if (!string.IsNullOrEmpty(Request.SalesOffer.SendingOfferDate))
            //                {
            //                    if (DateTime.TryParse(Request.SalesOffer.SendingOfferDate, out SendingOfferDateTemp))
            //                    {
            //                        SendingOfferDateTemp = DateTime.Parse(Request.SalesOffer.SendingOfferDate);
            //                        SendingOfferDate = SendingOfferDateTemp;
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Invalid SendingOfferDate";
            //                        Response.Errors.Add(error);
            //                    }
            //                }


            //                // Modified By michael markos 2022-10-25
            //                if (Request.SalesOffer.ParentSalesOfferID != null)
            //                {
            //                    // check if this Offer is Found
            //                    var SalesOfferObj = _unitOfWork.SalesOffers.FindAll(x => x.Id == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
            //                    if (SalesOfferObj == null)
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err-P12";
            //                        error.ErrorMSG = "Invalid Return Sales Offer!";
            //                        Response.Errors.Add(error);
            //                    }

            //                    if (Request.SalesOffer.OfferType != "Internal Ticket return")
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err-P12";
            //                        error.ErrorMSG = "Invalid Return Sales Offer Type Must Be Sales Return!";
            //                        Response.Errors.Add(error);
            //                    }


            //                    // If Returned Only
            //                    if (Request.SalesOffer.FinalOfferPrice != null && Request.SalesOffer.FinalOfferPrice > 0) // check  -- Refund must be less than final offer price
            //                    {
            //                        // Check if refunded before (offer returned)
            //                        var OffersReturned = _unitOfWork.InvoiceCnandDns.FindAll(x => x.ParentSalesOfferId == Request.SalesOffer.ParentSalesOfferID);
            //                        decimal TotalRefundCost = 0;
            //                        if (OffersReturned.Count() > 0)
            //                        {
            //                            var ListOfferChildrenIds = OffersReturned.Select(x => x.SalesOfferId).ToList();
            //                            var ListOfferRefunded = _unitOfWork.SalesOffers.FindAll(x => ListOfferChildrenIds.Contains(x.Id));
            //                            TotalRefundCost = ListOfferRefunded.Count() > 0 ? ListOfferRefunded.Select(x => x.FinalOfferPrice).DefaultIfEmpty(0).Sum() ?? 0 : 0;
            //                        }
            //                        if (Request.SalesOffer.FinalOfferPrice > (SalesOfferObj.FinalOfferPrice - TotalRefundCost))
            //                        {
            //                            Response.Result = false;
            //                            Error error = new Error();
            //                            error.ErrorCode = "Err-P12";
            //                            error.ErrorMSG = "Refund Cost for this ticket must be less than Total Remain Ticket !";
            //                            Response.Errors.Add(error);
            //                        }
            //                    }

            //                }

            //                if (Request.SalesOffer.ParentInvoiceID != null)
            //                {
            //                    // check if this Offer is Found
            //                    var InvoicesObj = _unitOfWork.Invoices.FindAll(x => x.Id == Request.SalesOffer.ParentInvoiceID).FirstOrDefault();
            //                    if (InvoicesObj == null)
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err-P12";
            //                        error.ErrorMSG = "Invalid Return Invoice!";
            //                        Response.Errors.Add(error);
            //                    }
            //                }

            //            }
            //            else
            //            {
            //                Response.Result = false;
            //                Error error = new Error();
            //                error.ErrorCode = "Err-P12";
            //                error.ErrorMSG = "Invalid Sales Offer Data!!";
            //                Response.Errors.Add(error);
            //            }
            //            if (Request.SalesOfferProductList == null || Request.SalesOfferProductList.Count() == 0)
            //            {
            //                Response.Result = false;
            //                Error error = new Error();
            //                error.ErrorCode = "Err-P12";
            //                error.ErrorMSG = "must be one item at least in offer";
            //                Response.Errors.Add(error);
            //                return Response;
            //            }
            //            if (Request.SalesOfferProductList != null)
            //            {
            //                if (Request.SalesOfferProductList.Count() > 0)
            //                {

            //                    var InventoryItemListIDS = Request.SalesOfferProductList.Select(x => x.InventoryItemId).ToList();

            //                    int Counter = 0;
            //                    foreach (var SalesOfferProduct in Request.SalesOfferProductList)
            //                    {
            //                        Counter++;
            //                        if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID > 0)
            //                        {

            //                            var ParentProductcDb = _unitOfWork.SalesOfferProducts.Find((x => x.OfferId == Request.SalesOffer.ParentSalesOfferID));
            //                            if (ParentProductcDb != null)
            //                            {

            //                                // for internal ticket
            //                                //if (SalesOfferProduct.Quantity != null && SalesOfferProduct.Quantity > 0)
            //                                //{
            //                                //    if (SalesOfferProduct.Quantity > (ParentProductcDb.RemainQty ?? ParentProductcDb.Quantity ?? 0))
            //                                //    {
            //                                //        Response.Result = false;
            //                                //        Error error = new Error();
            //                                //        error.ErrorCode = "Err-P12";
            //                                //        error.ErrorMSG = "Returned Quantity For Sales Offer Product: " + SalesOfferProduct.ParentOfferProductId + " Cannot be Greater Than Remain Quantity Of Parent Product!";
            //                                //        Response.Errors.Add(error);
            //                                //    }
            //                                //}

            //                                // Validate if Return => if have balance from parent or not


            //                            }
            //                            else
            //                            {
            //                                Response.Result = false;
            //                                Error error = new Error();
            //                                error.ErrorCode = "Err-P12";
            //                                error.ErrorMSG = "This Sales Offer Product " + SalesOfferProduct.ParentOfferProductId + " Doesn't Exist!";
            //                                Response.Errors.Add(error);
            //                            }

            //                        }

            //                        long InventoryItemId = 0;
            //                        if (SalesOfferProduct.InventoryItemId == null)
            //                        {
            //                            Response.Result = false;
            //                            Error error = new Error();
            //                            error.ErrorCode = "ErrCRM1";
            //                            error.ErrorMSG = "Invalid InventoryItemId on Item No #" + Counter;
            //                            Response.Errors.Add(error);
            //                        }
            //                        else
            //                        {
            //                            InventoryItemId = (long)SalesOfferProduct.InventoryItemId;
            //                        }
            //                        decimal Quantity = 0;
            //                        if (SalesOfferProduct.Quantity == null || SalesOfferProduct.Quantity <= 0)
            //                        {
            //                            Response.Result = false;
            //                            Error error = new Error();
            //                            error.ErrorCode = "ErrCRM1";
            //                            error.ErrorMSG = "Invalid Sales Offer Product Quantity on Item No #" + Counter;
            //                            Response.Errors.Add(error);
            //                        }
            //                        else
            //                        {
            //                            Quantity = (decimal)SalesOfferProduct.Quantity;
            //                        }
            //                    }
            //                }
            //            }
            //            else
            //            {
            //                if (Request.SalesOffer != null)
            //                {
            //                    if (Request.SalesOffer.Id == null)
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err-P12";
            //                        error.ErrorMSG = "Invalid Sales Offer Product";
            //                        Response.Errors.Add(error);

            //                    }
            //                }
            //            }

            //            // Get the timezone information for Egypt
            //            TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            //            // Get the current datetime in Egypt
            //            DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

            //            if (Response.Result)
            //            {
            //                long SalesOfferId = 0;
            //                // Add-Edit Sales Offer
            //                if (Request.SalesOffer.Id == null || Request.SalesOffer.Id == 0)
            //                {

            //                    var NewOfferSerial = "";
            //                    var OfferSerialSubString = "";

            //                    //long newOfferNumber = 0;
            //                    long CountOfSalesOfferThisYear = _unitOfWork.SalesOffers.Count(x => x.Active == true && x.OfferSerial.Contains(System.DateTime.Now.Year.ToString()) &&
            //                    (x.OfferType == "Internal Ticket return" || x.OfferType == "Internal Ticket"));

            //                    NewOfferSerial += "#" + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();

            //                    // Insert
            //                    var NewSalesOfferInsert = new SalesOffer()
            //                    {
            //                        StartDate = DateOnly.FromDateTime((DateTime)StartDate),
            //                        EndDate = DateOnly.FromDateTime((DateTime)EndDate),
            //                        Note = string.IsNullOrEmpty(Request.SalesOffer.Note) ? null : Request.SalesOffer.Note,
            //                        SalesPersonId = Request.SalesOffer.SalesPersonId,
            //                        CreatedBy = creator,
            //                        CreationDate = egyptDateTime,
            //                        ModifiedBy = null,
            //                        Modified = null,
            //                        Active = true,
            //                        Status = Request.SalesOffer.Status,
            //                        Completed = Request.SalesOffer.Completed,
            //                        TechnicalInfo = string.IsNullOrEmpty(Request.SalesOffer.TechnicalInfo) ? null : Request.SalesOffer.TechnicalInfo,
            //                        ProjectData = string.IsNullOrEmpty(Request.SalesOffer.ProjectData) ? null : Request.SalesOffer.ProjectData,
            //                        FinancialInfo = string.IsNullOrEmpty(Request.SalesOffer.FinancialInfo) ? null : Request.SalesOffer.FinancialInfo,
            //                        PricingComment = string.IsNullOrEmpty(Request.SalesOffer.PricingComment) ? null : Request.SalesOffer.PricingComment,
            //                        OfferAmount = Request.SalesOffer.OfferAmount == null ? null : Request.SalesOffer.OfferAmount,
            //                        SendingOfferConfirmation = Request.SalesOffer.SendingOfferConfirmation,
            //                        SendingOfferDate = SendingOfferDate ?? egyptDateTime,
            //                        SendingOfferBy = Request.SalesOffer.SendingOfferBy,
            //                        SendingOfferTo = Request.SalesOffer.SendingOfferTo,
            //                        SendingOfferComment = Request.SalesOffer.SendingOfferComment,
            //                        ClientApprove = Request.SalesOffer.ClientApprove,
            //                        ClientComment = Request.SalesOffer.ClientComment,
            //                        VersionNumber = Request.SalesOffer.VersionNumber,
            //                        ClientApprovalDate = ClientApprovalDate ?? egyptDateTime,
            //                        ClientId = Request.SalesOffer.ClientId,
            //                        ProductType = Request.SalesOffer.ProductType,
            //                        ProjectName = string.IsNullOrEmpty(Request.SalesOffer.ProjectName) ? NewOfferSerial : Request.SalesOffer.ProjectName,
            //                        ProjectLocation = Request.SalesOffer.ProjectLocation,
            //                        ContactPersonMobile = Request.SalesOffer.ContactPersonMobile,
            //                        ContactPersonEmail = Request.SalesOffer.ContactPersonEmail,
            //                        ContactPersonName = Request.SalesOffer.ContactPersonName,
            //                        ProjectStartDate = ProjectStartDate ?? egyptDateTime,
            //                        ProjectEndDate = ProjectEndDate ?? egyptDateTime,
            //                        BranchId = Request.SalesOffer.BranchId,
            //                        OfferType = Request.SalesOffer.OfferType,
            //                        ContractType = Request.SalesOffer.ContractType,
            //                        OfferSerial = NewOfferSerial,
            //                        ClientNeedsDiscount = Request.SalesOffer.ClientNeedsDiscount,
            //                        RejectionReason = Request.SalesOffer.RejectionReason,
            //                        NeedsInvoice = Request.SalesOffer.NeedsInvoice,
            //                        NeedsExtraCost = Request.SalesOffer.NeedsExtraCost,
            //                        OfferExpirationDate = OfferExpirationDate,
            //                        OfferExpirationPeriod = Request.SalesOffer.OfferExpirationPeriod,
            //                        ExtraOrDiscountPriceBySalesPerson = Request.SalesOffer.ExtraOrDiscountPriceBySalesPerson,
            //                        FinalOfferPrice = Request.SalesOffer.FinalOfferPrice,
            //                        ReminderDate = ReminderDate
            //                    };
            //                    _unitOfWork.SalesOffers.Add(NewSalesOfferInsert);
            //                    var SalesOfferInsert = _unitOfWork.Complete();

            //                    if (SalesOfferInsert != 0 && NewSalesOfferInsert.Id != 0)
            //                    {
            //                        SalesOfferId = (long)NewSalesOfferInsert.Id;
            //                        Response.Result = true;
            //                        Response.ID = SalesOfferId;
            //                        if (Request.SalesOffer.ParentSalesOfferID != null)
            //                        {
            //                            long? ParentInvoiceID = Request.SalesOffer.ParentInvoiceID;
            //                            if (ParentInvoiceID == null)
            //                            {
            //                                var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
            //                                if (InvoiceDB != null)
            //                                {
            //                                    ParentInvoiceID = InvoiceDB.Id;
            //                                }
            //                            }
            //                            if (ParentInvoiceID != null)
            //                            {
            //                                // Modified By Michael Markos 2022-10-25
            //                                // Add in table Invoice CN And DN
            //                                var InvoiceCNAndDNObj = new InvoiceCnandDn();
            //                                InvoiceCNAndDNObj.ParentSalesOfferId = (long)Request.SalesOffer.ParentSalesOfferID;
            //                                InvoiceCNAndDNObj.ParentInvoiceId = (long)ParentInvoiceID;
            //                                InvoiceCNAndDNObj.SalesOfferId = SalesOfferId;
            //                                InvoiceCNAndDNObj.Active = true;
            //                                InvoiceCNAndDNObj.CreatedBy = creator; //CreatedBy ?? 1;
            //                                InvoiceCNAndDNObj.CreationDate = egyptDateTime;
            //                                InvoiceCNAndDNObj.ModifiedBy = creator; // CreatedBy ?? 1;
            //                                InvoiceCNAndDNObj.ModificationDate = egyptDateTime;

            //                                _unitOfWork.InvoiceCnandDns.Add(InvoiceCNAndDNObj);
            //                                _unitOfWork.Complete();
            //                            }
            //                        }
            //                    }
            //                    else
            //                    {
            //                        Response.Result = false;
            //                        Error error = new Error();
            //                        error.ErrorCode = "Err25";
            //                        error.ErrorMSG = "Faild To Insert this Offer!!";
            //                        Response.Errors.Add(error);
            //                    }

            //                    //var SalesOfferInvoices = _Context.Invoices.Where(a => a.SalesOfferId == SalesOfferId).Count();
            //                    // Add-Edit Sales Offer Product
            //                    if (Request.SalesOfferProductList != null)
            //                    {
            //                        if (Request.SalesOfferProductList.Count() > 0)
            //                        {
            //                            foreach (var SalesOfferProduct in Request.SalesOfferProductList)
            //                            {
            //                                decimal TempProfitPercentage;
            //                                decimal? ProfitPercentage = decimal.TryParse(SalesOfferProduct.ProfitPercentage, out TempProfitPercentage) ? TempProfitPercentage : (decimal?)null;
            //                                // Anton samir in 2025-2-27 remove this

            //                                //string ItemComment = null; 
            //                                //if (string.IsNullOrEmpty(SalesOfferProduct.ItemPricingComment))
            //                                //{
            //                                //    var inventoryItemDb = _unitOfWork.InventoryItems.GetById((long)SalesOfferProduct.InventoryItemId);
            //                                //    if (inventoryItemDb != null)
            //                                //    {
            //                                //        if (string.IsNullOrEmpty(inventoryItemDb.Description))
            //                                //        {
            //                                //            ItemComment = inventoryItemDb.Name;
            //                                //        }
            //                                //        else
            //                                //        {
            //                                //            ItemComment = inventoryItemDb.Description;
            //                                //        }
            //                                //    }
            //                                //}
            //                                //else
            //                                //{
            //                                //    ItemComment = SalesOfferProduct.ItemPricingComment;
            //                                //}
            //                                string ItemComment = SalesOfferProduct.ItemPricingComment;
            //                                // Add-Edit Sales Offer
            //                                if (SalesOfferProduct.Id == null || SalesOfferProduct.Id == 0)
            //                                {

            //                                    // Insert
            //                                    //ObjectParameter SalesOfferProductInsertedId = new ObjectParameter("ID", typeof(long));
            //                                    var NewSalesOfferProductInsert = new SalesOfferProduct()
            //                                    {
            //                                        CreatedBy = creator, // long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key)),
            //                                        CreationDate = egyptDateTime,
            //                                        ModifiedBy = null,
            //                                        Modified = null,
            //                                        Active = true,
            //                                        OfferId = SalesOfferId,
            //                                        ProductId = SalesOfferProduct.ProductId,
            //                                        ProductGroupId = SalesOfferProduct.ProductGroupId,
            //                                        Quantity = SalesOfferProduct.Quantity,
            //                                        InventoryItemId = SalesOfferProduct.InventoryItemId,
            //                                        InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId,
            //                                        ItemPrice = SalesOfferProduct.ItemPrice,
            //                                        ItemPricingComment = ItemComment,
            //                                        ConfirmReceivingQuantity = null,
            //                                        ConfirmReceivingComment = null,
            //                                        InvoicePayerClientId = SalesOfferProduct.InvoicePayerClientId,
            //                                        DiscountPercentage = SalesOfferProduct.DiscountPercentage,
            //                                        DiscountValue = SalesOfferProduct.DiscountValue,
            //                                        FinalPrice = SalesOfferProduct.FinalPrice,
            //                                        TaxPercentage = SalesOfferProduct.TaxPercentage,
            //                                        TaxValue = SalesOfferProduct.TaxValue,
            //                                        ReturnedQty = 0,
            //                                        RemainQty = SalesOfferProduct.Quantity,
            //                                        ProfitPercentage = ProfitPercentage,
            //                                        ReleasedQty = null
            //                                    };
            //                                    var SalesOfferProductInsert = _unitOfWork.SalesOfferProducts.Add(NewSalesOfferProductInsert);
            //                                    _unitOfWork.Complete();
            //                                    if (SalesOfferProductInsert != null)
            //                                    {
            //                                        SalesOfferProduct.Id = (long)SalesOfferProductInsert.Id;

            //                                        if (SalesOfferProduct.ParentOfferProductId != null && SalesOfferProduct.ParentOfferProductId != 0) // Client Return
            //                                        {
            //                                            var ParentSalesOfferProductDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.Id == (long)SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
            //                                            if (ParentSalesOfferProductDb != null)
            //                                            {
            //                                                ParentSalesOfferProductDb.RemainQty -= SalesOfferProduct.Quantity;
            //                                                ParentSalesOfferProductDb.ReturnedQty += SalesOfferProduct.Quantity;
            //                                                _unitOfWork.Complete();
            //                                            }

            //                                        }

            //                                        Response.Result = true;
            //                                    }
            //                                    else
            //                                    {
            //                                        Response.Result = false;
            //                                        Error error = new Error();
            //                                        error.ErrorCode = "Err25";
            //                                        error.ErrorMSG = "Faild To Insert this Offer!!";
            //                                        Response.Errors.Add(error);
            //                                    }
            //                                }
            //                            }


            //                            //When change PayerId Check Deleted Invoices And Inserted Invoices Then Delete Old and Insert New
            //                            var clientsIds = Request.SalesOfferProductList.Where(a => a.Active == true).Select(a => a.InvoicePayerClientId).Distinct().ToList();

            //                            var InvoicesToInsertIds = clientsIds.ToList();


            //                            //Insert 

            //                            if (InvoicesToInsertIds != null && InvoicesToInsertIds.Count > 0)
            //                            {
            //                                foreach (var clientId in InvoicesToInsertIds)
            //                                {
            //                                    var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
            //                                    var OfferClientApprovalDate = SalesOfferDb.ClientApprovalDate;
            //                                    DateTime InvoiceDate = OfferClientApprovalDate ?? egyptDateTime;
            //                                    long InvoiceId = 0;
            //                                    //ObjectParameter InvoiceInsertedId = new ObjectParameter("ID", typeof(long));
            //                                    var NewInvoiceInsert = new Invoice()
            //                                    {
            //                                        Serial = "1",
            //                                        Revision = 0,
            //                                        InvoiceDate = InvoiceDate,
            //                                        InvoiceType = "1",
            //                                        ClientId = clientId,
            //                                        CreatedBy = creator,
            //                                        ModifiedBy = creator,
            //                                        CreationDate = egyptDateTime,
            //                                        ModificationDate = egyptDateTime,
            //                                        Active = true,
            //                                        IsClosed = false,
            //                                        CreationType = null,
            //                                        InvoiceFor = null,
            //                                        EInvoiceId = null,
            //                                        EInvoiceStatus = null,
            //                                        EInvoiceAcceptDate = null,
            //                                        SalesOfferId = SalesOfferId,
            //                                        EInvoiceJsonBody = null,
            //                                        EInvoiceRequestToSend = null,
            //                                    };
            //                                    var InvoiceInsert = _unitOfWork.Invoices.Add(NewInvoiceInsert);
            //                                    _unitOfWork.Complete();

            //                                    if (InvoiceInsert != null)
            //                                    {
            //                                        InvoiceId = (long)InvoiceInsert.Id;
            //                                        int SerialTemp = 0;
            //                                        var SerialList = _unitOfWork.Invoices.FindAll(x => x.Active == true).Select(x => x.Serial).ToList();
            //                                        int Serial = SerialList.Where(x => int.TryParse(x, out SerialTemp)).OrderByDescending(x => int.Parse(x)).Select(x => int.Parse(x)).FirstOrDefault();
            //                                        //int SerialNo = string.IsNullOrEmpty(Serial) && int.TryParse(Serial, out SerialNo) ? 1 : int.Parse(Serial) + 1;
            //                                        var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.Id == InvoiceId).FirstOrDefault();
            //                                        if (InvoiceDB != null)
            //                                        {
            //                                            InvoiceDB.Serial = (Serial + 1).ToString();
            //                                            _unitOfWork.Complete();
            //                                        }
            //                                        var ClientInvoicesItemList = _unitOfWork.SalesOfferProducts.FindAll(x => x.OfferId == SalesOfferId && x.InvoicePayerClientId == clientId).ToList();
            //                                        // Insert Into Invoice Items
            //                                        if (ClientInvoicesItemList.Count > 0)
            //                                        {
            //                                            foreach (var invoiceItem in ClientInvoicesItemList)
            //                                            {
            //                                                //ObjectParameter InvoiceItemInsertedId = new ObjectParameter("ID", typeof(long));          
            //                                                _unitOfWork.InvoiceItems.Add(new InvoiceItem()
            //                                                {
            //                                                    InvoiceId = InvoiceId,
            //                                                    SalesOfferProductId = invoiceItem.Id,
            //                                                    Comments = null,
            //                                                    EInvoiceId = null,
            //                                                    EInvoiceStatus = null,
            //                                                    EInvoiceAcceptDate = null
            //                                                });
            //                                                _unitOfWork.Complete();

            //                                            }
            //                                        }

            //                                    }
            //                                }
            //                            }

            //                        }
            //                    }
            //                }
            //                else
            //                {
            //                    Response.Result = false;
            //                    Error error = new Error();
            //                    error.ErrorCode = "Err10";
            //                    error.ErrorMSG = "Invalid Id for SalesOffer Internal Ticket ";
            //                    Response.Errors.Add(error);
            //                    return Response;
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



            //public GetSalesOfferListForInternalTicketResponse GetSalesOfferListForInternalTicket(GetSalesOfferInternalTicketListFilters filters, string OfferStatusParam)
            //{
            //    GetSalesOfferListForInternalTicketResponse Response = new GetSalesOfferListForInternalTicketResponse();
            //    Response.Result = true;
            //    Response.Errors = new List<Error>();
            //    try
            //    {

            //        if (Response.Result)
            //        {

            //            if (!string.IsNullOrEmpty(filters.OfferType))
            //            {
            //                filters.OfferType = filters.OfferType.ToLower();
            //            }


            //            if (!string.IsNullOrEmpty(filters.ClientName))
            //            {
            //                filters.ClientName = HttpUtility.UrlDecode(filters.ClientName).ToLower();
            //            }

            //            if (!string.IsNullOrEmpty(OfferStatusParam))
            //            {
            //                if (OfferStatusParam.ToLower() != "all")
            //                {
            //                    filters.OfferStatus = OfferStatusParam.ToLower();
            //                }
            //                else
            //                {
            //                    if (!string.IsNullOrEmpty(filters.OfferStatus))
            //                    {
            //                        filters.OfferStatus = filters.OfferStatus.ToLower();
            //                    }
            //                }
            //            }

            //            var StartDate = filters.From ?? DateTime.Now;
            //            var EndDate = filters.To ?? DateTime.Now;
            //            var DateFilter = false;

            //            if (filters.From != null)
            //            {
            //                DateFilter = true;
            //                StartDate = (DateTime)filters.From;

            //                if (filters.To != null)
            //                {
            //                    EndDate = (DateTime)filters.To;
            //                }
            //            }
            //            else
            //            {
            //                if (filters.To != null)
            //                {
            //                    Error error = new Error();
            //                    error.ErrorCode = "Err-13";
            //                    error.ErrorMSG = "You have to Enter Offer From Date!";
            //                    Response.Errors.Add(error);
            //                    Response.Result = false;
            //                    return Response;
            //                }
            //            }

            //            if (Response.Result)
            //            {
            //                var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "SalesPerson", "ClientAccounts", "Client", "CreatedByNavigation", "SalesOfferProducts" }).AsQueryable();
            //                //var test1 = SalesOfferDBQuery.ToList();

            //                if (filters.SalesOfferClassifiction == "POS")
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "POS" || a.OfferType == "Sales Return").AsQueryable();
            //                }
            //                else if (filters.SalesOfferClassifiction == "InternalTicket")
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "Internal Ticket return" || a.OfferType == "Internal Ticket").AsQueryable();
            //                }
            //                if (filters.CreatorUserId != 0)
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.CreatedBy == filters.CreatorUserId).AsQueryable();
            //                }
            //                if (filters.DepartmentId != 0)
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.SalesPerson.DepartmentId == filters.DepartmentId).AsQueryable();
            //                }
            //                if (filters.CategoryId != 0)
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.SalesOfferProducts.Any(a => a.InventoryItemCategoryId == filters.CategoryId)).AsQueryable();
            //                }
            //                // supplier Name , Offer Serial ,Project Name
            //                if (!string.IsNullOrEmpty(filters.SearchKey))
            //                {
            //                    filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
            //                                               (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(filters.SearchKey.ToLower()) : false)
            //                                            || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false)
            //                                            || (x.Id.ToString() == filters.SearchKey)
            //                                            || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(filters.SearchKey.ToLower()) : false)
            //                                            || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false))
            //                                            //|| SalesOfferIDS.Contains(x.ID)
            //                                            ).AsQueryable();
            //                }
            //                if (!string.IsNullOrEmpty(filters.OfferType))
            //                {
            //                    if (filters.OfferType == "new project offer")
            //                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == "new project offer" || a.OfferType.ToLower() == "direct sales").AsQueryable();
            //                    else
            //                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == filters.OfferType).AsQueryable();
            //                }

            //                if (filters.SalesPersonId != 0)
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
            //                }
            //                if (!string.IsNullOrEmpty(filters.ClientName))
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filters.ClientName)).AsQueryable();
            //                }

            //                if (DateFilter)
            //                {
            //                    if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
            //                    {
            //                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= StartDate && a.ClientApprovalDate <= EndDate).AsQueryable();
            //                    }
            //                    else
            //                    {
            //                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime(StartDate) && a.EndDate <= DateOnly.FromDateTime(EndDate)).AsQueryable();
            //                    }
            //                }
            //                else
            //                {
            //                    if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
            //                    {
            //                        var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
            //                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
            //                    }
            //                }


            //                if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate).OrderByDescending(a => a.CreationDate);
            //                }
            //                else
            //                {
            //                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
            //                }


            //                var OffersListDB = PagedList<SalesOffer>.Create(SalesOfferDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);
            //                var TotalOffersCount = OffersListDB.TotalCount;
            //                //var TotalOffersPriceWithReturned = SalesOfferDBQuery.Sum(a => a.FinalOfferPrice) ?? 0;
            //                var TotalOffersPriceWithoutReturn = SalesOfferDBQuery.Where(x => x.OfferType == "Internal Ticket").Sum(a => a.FinalOfferPrice) ?? 0;

            //                var TotalOffersReturnedPrice = SalesOfferDBQuery.Where(a => a.OfferType == "Internal Ticket return").Sum(a => a.FinalOfferPrice) ?? 0;
            //                var TotalOffersPrice = TotalOffersPriceWithoutReturn - TotalOffersReturnedPrice;
            //                SalesOfferForInternalTicket salesOfferTypeDetails = new SalesOfferForInternalTicket()
            //                {
            //                    TotalOffersCount = TotalOffersCount,
            //                    TotalOffersPrice = TotalOffersPrice
            //                };

            //                var OffersListResponse = new List<SalesOfferDetailsForInternalTicket>();
            //                var IDSSalesOffer = OffersListDB.Select(x => x.Id).ToList();
            //                var ParentSalesOfferListDB = _unitOfWork.InvoiceCnandDns.FindAll((x => IDSSalesOffer.Contains(x.SalesOfferId) || IDSSalesOffer.Contains(x.ParentSalesOfferId)), new[] { "SalesOffer", "ParentSalesOffer" }).ToList();
            //                foreach (var offer in OffersListDB)
            //                {
            //                    long? projectId = null;
            //                    decimal QTYOfMatrialReleaseItem = 0;
            //                    double QTYOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.RemainQty ?? 0) ?? 0;
            //                    var SalesOfferProductCount = offer.SalesOfferProducts?.Count ?? 0;
            //                    if (offer.Projects.Count > 0)
            //                    {
            //                        var offerProject = offer.Projects.FirstOrDefault();
            //                        projectId = offerProject.Id;
            //                        if (offerProject.InventoryMatrialRequestItems.Count > 0)
            //                        {
            //                            QTYOfMatrialReleaseItem = offerProject.InventoryMatrialRequestItems?.Sum(x => x.RecivedQuantity1 ?? 0) ?? 0;
            //                        }
            //                    }

            //                    long? ParentSalesOfferId = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOfferId).FirstOrDefault();
            //                    string ParentSalesOfferSErial = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOffer?.OfferSerial).FirstOrDefault();
            //                    var ListChildrenSalesOffer = ParentSalesOfferListDB.Where(x => x.ParentSalesOfferId == offer.Id).Select(x => new ChildrenSalesOfferInternalTicket { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial, CreationDate = x.SalesOffer?.CreationDate.ToShortDateString(), TotalPrice = x.SalesOffer?.FinalOfferPrice }).ToList();

            //                    SalesOfferDetailsForInternalTicket salesOfferObj = new SalesOfferDetailsForInternalTicket()
            //                    {
            //                        Id = offer.Id,
            //                        SalesPersonId = offer.SalesPersonId,
            //                        OfferType = offer.OfferType,
            //                        SalesPersonName = offer.SalesPerson.FirstName + ' ' + offer.SalesPerson.MiddleName + ' ' + offer.SalesPerson.LastName,
            //                        ClientId = offer.ClientId,
            //                        ClientName = offer.Client.Name,
            //                        ContactPersonName = offer.ContactPersonName,
            //                        OfferSerial = offer.OfferSerial,
            //                        FinalOfferPrice = offer.FinalOfferPrice,
            //                        CreationDate = offer.CreationDate.ToString(),
            //                        CreatorName = offer.CreatedByNavigation?.FirstName + " " + offer.CreatedByNavigation?.LastName,
            //                        ParentSalesOfferID = ParentSalesOfferId,
            //                        ParentSalesOfferSerial = ParentSalesOfferSErial,
            //                        ChildrenSalesOfferList = ListChildrenSalesOffer
            //                    };

            //                    OffersListResponse.Add(salesOfferObj);
            //                }
            //                salesOfferTypeDetails.SalesOfferList = OffersListResponse;
            //                Response.SalesOfferList = salesOfferTypeDetails;

            //                Response.PaginationHeader = new PaginationHeader
            //                {
            //                    CurrentPage = filters.CurrentPage,
            //                    TotalPages = OffersListDB.TotalPages,
            //                    ItemsPerPage = filters.NumberOfItemsPerPage,
            //                    TotalItems = OffersListDB.TotalCount
            //                };
            //            }
            //        }

            //        return Response;
            //    }
            //    catch (Exception ex)
            //    {
            //        Response.Result = false;
            //        Error error = new Error();
            //        error.ErrorCode = "Err10";
            //        error.ErrorMSG = ex.Message;
            //        Response.Errors.Add(error);
            //        return Response;
            //    }
            //}


            //public GetSalesOfferDetailsForInternalTicketResponse GetSalesOfferDetailsForInternalTicket(long SalesOfferId)
            //{
            //    GetSalesOfferDetailsForInternalTicketResponse Response = new GetSalesOfferDetailsForInternalTicketResponse();
            //    Response.Result = true;
            //    Response.Errors = new List<Error>();
            //    try
            //    {
            //        if (Response.Result)
            //        {
            //            var SalesOffer = GetSalesOfferInfo(SalesOfferId);
            //            if (SalesOffer.Id != null)
            //            {
            //                Response.SalesOfferDetails = SalesOffer;
            //                Response.SalesOfferProducts = GetSalesOfferProductsList(SalesOfferId);
            //            }
            //            else
            //            {
            //                Response.Result = false;
            //                Error error = new Error();
            //                error.ErrorCode = "Err99";
            //                error.ErrorMSG = "This Sales Offer Doesn't Exist!!";
            //                Response.Errors.Add(error);
            //                return Response;
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

            //public SalesOfferDetailsForInternalTicket GetSalesOfferInfo(long SalesOfferId)
            //{

            //    var SalesOfferObj = new SalesOfferDetailsForInternalTicket();
            //    var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId, includes: new[] { "SalesPerson", "CreatedByNavigation" }).FirstOrDefault();
            //    if (SalesOfferDb != null)
            //    {
            //        SalesOfferObj.Id = SalesOfferDb.Id;
            //        SalesOfferObj.SalesPersonId = SalesOfferDb.SalesPersonId;
            //        if (SalesOfferDb.SalesPersonId != 0)
            //        {
            //            var Hruser = _unitOfWork.HrUsers.FindAll(a => a.UserId == SalesOfferDb.SalesPersonId, includes: new[] { "Team" }).FirstOrDefault();
            //            if (Hruser != null)
            //            {
            //                SalesOfferObj.TeamId = Hruser.TeamId; // Default 
            //                SalesOfferObj.TeamName = Hruser.Team.Name;
            //            }
            //        }
            //        SalesOfferObj.SalesPersonName = SalesOfferDb.SalesPerson.FirstName + ' ' + SalesOfferDb.SalesPerson.MiddleName + ' ' + SalesOfferDb.SalesPerson.LastName;
            //        SalesOfferObj.CreatorName = SalesOfferDb.CreatedByNavigation?.FirstName + " " + SalesOfferDb.CreatedByNavigation?.LastName;
            //        if (SalesOfferDb.ClientId != null)
            //        {
            //            var ClientDb = _unitOfWork.Clients.FindAll(a => a.Id == (long)SalesOfferDb.ClientId).FirstOrDefault();
            //            SalesOfferObj.ClientId = SalesOfferDb.ClientId;
            //            SalesOfferObj.ClientName = ClientDb.Name;
            //        }
            //        SalesOfferObj.ContactPersonName = SalesOfferDb.ContactPersonName;
            //        SalesOfferObj.OfferSerial = SalesOfferDb.OfferSerial;
            //        SalesOfferObj.CreationDate = SalesOfferDb.CreationDate.ToShortDateString();
            //        SalesOfferObj.CreationTime = SalesOfferDb.CreationDate.ToShortTimeString();
            //        SalesOfferObj.FinalOfferPrice = SalesOfferDb.FinalOfferPrice;
            //        SalesOfferObj.RemainPrice = SalesOfferDb.FinalOfferPrice ?? 0;

            //        var CheckChildrenOrParentsList = _unitOfWork.InvoiceCnandDns.FindAll(x => x.ParentSalesOfferId == SalesOfferId || x.SalesOfferId == SalesOfferId, includes: new[] { "SalesOffer", "ParentSalesOffer" });


            //        var OfferChildrenList = CheckChildrenOrParentsList.Where(x => x.ParentSalesOfferId == SalesOfferId).ToList();
            //        if (OfferChildrenList.Count() > 0)   // This Offer is Parent
            //        {

            //            SalesOfferObj.ChildrenSalesOfferList = OfferChildrenList.Select(x => new ChildrenSalesOfferInternalTicket { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial, CreationDate = x.SalesOffer?.CreationDate.ToShortDateString(), TotalPrice = x.SalesOffer?.FinalOfferPrice }).ToList();

            //            var TotalRefundCost = SalesOfferObj.ChildrenSalesOfferList.Select(x => x.TotalPrice).DefaultIfEmpty(0).Sum();
            //            SalesOfferObj.RemainPrice = (SalesOfferDb.FinalOfferPrice ?? 0) - (TotalRefundCost ?? 0);
            //        }
            //        else
            //        {
            //            var ParentSalesOfferDB = CheckChildrenOrParentsList.Where((x => x.SalesOfferId == SalesOfferDb.Id)).FirstOrDefault();
            //            if (ParentSalesOfferDB != null)
            //            {
            //                SalesOfferObj.ParentSalesOfferID = ParentSalesOfferDB.ParentSalesOfferId;
            //                SalesOfferObj.ParentSalesOfferSerial = ParentSalesOfferDB.ParentSalesOffer?.OfferSerial;


            //                var TotalRefundCost = CheckChildrenOrParentsList.Where(x => x.ParentSalesOfferId == SalesOfferObj.ParentSalesOfferID).Select(x => x.SalesOffer?.FinalOfferPrice).DefaultIfEmpty(0).Sum();
            //                SalesOfferObj.RemainPrice = (ParentSalesOfferDB.ParentSalesOffer?.FinalOfferPrice ?? 0) - (TotalRefundCost ?? 0);
            //            }

            //        }
            //    }
            //    return SalesOfferObj;
            //}


            //public List<SalesOfferProductForInternalTicket> GetSalesOfferProductsList(long SalesOfferId)
            //{
            //    var SalesOfferProducts = new List<SalesOfferProductForInternalTicket>();
            //    var SalesOfferProductsDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == SalesOfferId && a.Active == true, new[] { "InventoryItem", "CreatedByNavigation", "ModifiedByNavigation" }).ToList();
            //    if (SalesOfferProductsDb.Count > 0)
            //    {
            //        SalesOfferProducts = SalesOfferProductsDb.Select(product => new SalesOfferProductForInternalTicket
            //        {
            //            Id = product.Id,
            //            CreatedBy = product.CreatedByNavigation != null ? product.CreatedByNavigation?.FirstName + " " + product.CreatedByNavigation?.LastName : null,
            //            ModifiedBy = product.ModifiedByNavigation != null ? product.ModifiedByNavigation?.FirstName + " " + product.ModifiedByNavigation?.LastName : null,
            //            InventoryItemCategoryId = product.InventoryItemCategoryId ?? null,
            //            InventoryItemCategoryName = product.InventoryItemCategoryId != null ? _unitOfWork.InventoryItemCategories.GetById((int)product.InventoryItemCategoryId)?.Name : null,
            //            InventoryItemId = product.InventoryItemId ?? null,
            //            InventoryItemName = product.InventoryItemId != null && product.InventoryItem != null ? product.InventoryItem.Name : null, //_unitOfWork.InventoryItems.GetById((long)product.InventoryItemId)?.Name : null,
            //            ItemPrice = product.ItemPrice ?? null,
            //            ItemPricingComment = product.ItemPricingComment ?? null
            //        }).ToList();
            //    }

            //    return SalesOfferProducts;
            //}


        #endregion







    }
}
