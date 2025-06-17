using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.ProjectPayment;
using System.Collections.Generic;
using NewGaras.Infrastructure.DTO.ProjectLetterOfCredit;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using Grpc.Core;
using NewGaras.Infrastructure.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using NewGarasAPI.Models.Project.UsedInResponses;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using NewGaras.Infrastructure.Models.ProjectPayment;
using DocumentFormat.OpenXml.Drawing;
using System.Diagnostics.Metrics;
using NewGaras.Infrastructure.Helper;
using NewGarasAPI.Helper;
using NewGaras.Infrastructure.Helper.TenantService;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Office.CustomUI;
using Path = System.IO.Path;

namespace NewGarasAPI.Controllers.ProjectManagement
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectPaymentController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private Helper.Helper _helper;
        static string key;
        private GarasTestContext _Context;
        private readonly IMapper _mapper;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        private readonly ITenantService _tenantService;
        public ProjectPaymentController(IUnitOfWork unitOfWork, IWebHostEnvironment host, IMapper mapper,
            IHrUserService hrUserService, IMailService mailService, ITaskMangerProjectService taskMangerProjectService,ITenantService tenantService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _taskMangerProjectService = taskMangerProjectService;
        }

        [HttpPost("AddProjectPaymentTerms")]            
        public BaseResponseWithId<long> AddProjectPaymentTerms([FromBody]AddProjectPaymentTermsDto Dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (Dto.PaymentTermID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "PaymentTermID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Currency ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            //if (Dto.DailyJournalEntryID == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err103";
            //    error.ErrorMSG = "DailyJournalEntryID Is Required";
            //    response.Errors.Add(error);
            //    return response;
            //}
            if(Dto.Amount == null && Dto.Percentage == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Amount and percentage can not be null at the same time, please enter one of them";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Amount != null && Dto.Percentage != null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Amount and percentage can not be not null at the same time, please enter only one of them";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            #region Date Validation
            DateTime DueDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.DueDate, out DueDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }

            //DateTime CollectionDate = DateTime.Now;
            //if (!DateTime.TryParse(Dto.CollectionDate, out CollectionDate))
            //{
            //    response.Result = false;
            //    Error err = new Error();
            //    err.ErrorCode = "E-1";
            //    err.errorMSG = "please, Enter a valid Collection Date:";
            //    response.Errors.Add(err);
            //    return response;
            //}
            #endregion

            try
            {
                if (response.Result)
                {
                    //var salaeOfferTotalAmount = project?.SalesOffer?.FinalOfferPrice ?? 0;

                    //var newProjectPaymentTerm = new ProjectPaymentTerm();
                    //newProjectPaymentTerm.ProjectId = Dto.ProjectID;
                    //newProjectPaymentTerm.PaymentTermId = Dto.PaymentTermID;
                    //newProjectPaymentTerm.Percentage = (decimal)(Dto.Percentage != null ? Dto.Percentage : (salaeOfferTotalAmount / Dto.Amount) * 100);
                    //newProjectPaymentTerm.Amount = (decimal)(Dto.Amount != null ? Dto.Amount : Dto.Percentage * salaeOfferTotalAmount);
                    //newProjectPaymentTerm.CurrencyId = Dto.CurrencyID;
                    //newProjectPaymentTerm.DueDate = DueDate;
                    //newProjectPaymentTerm.Collected = Dto.Collected;
                    //newProjectPaymentTerm.CollectionDate = CollectionDate;
                    //newProjectPaymentTerm.Remain = newProjectPaymentTerm.Amount - newProjectPaymentTerm.Collected;
                    //newProjectPaymentTerm.CreatedBy = validation.userID;
                    //newProjectPaymentTerm.CreationDate = DateTime.Now;
                    //newProjectPaymentTerm.ModifiedBy = validation.userID;
                    //newProjectPaymentTerm.ModificationDate = DateTime.Now;
                    //newProjectPaymentTerm.Active = true;

                    //_Context.ProjectPaymentTerms.Add(newProjectPaymentTerm);
                    //_Context.SaveChanges();

                    ////var ProjectPaymentJournalEntry = new ProjectPaymentJournalEntry();
                    ////ProjectPaymentJournalEntry.ProjectPaymentTermId = ProjectPaymentTermDB.Id;
                    ////ProjectPaymentJournalEntry.DailyJournalEntryId = Dto.DailyJournalEntryID;

                    //response.ID = newProjectPaymentTerm.Id;
                    var ProjectPaymentTerm = _taskMangerProjectService.AddProjectPaymentTerms(Dto, validation.userID);
                    if (!ProjectPaymentTerm.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectPaymentTerm.Errors);
                        return response;
                    }
                    response.ID = ProjectPaymentTerm.ID;
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

        [HttpGet("GetProjectPaymentTermsList")]         
        public BaseResponseWithData<GetProjectPaymentTerms> GetProjectPaymentTerms([FromHeader] GetprojectPaymentTermsFilters filters)
        {
            var response = new BaseResponseWithData<GetProjectPaymentTerms>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion


            #region DateValidation
            
            DateTime DueDateFrom = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateFrom))
            {
                if (!DateTime.TryParse(filters.DueDateFrom, out DueDateFrom))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate From ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            DateTime DueDateTo = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateTo))
            {
                if (!DateTime.TryParse(filters.DueDateTo, out DueDateTo))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate To ";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    //var projectPaymentTerm = _Context.ProjectPaymentTerms.Include(a => a.Project.SalesOffer).Include(a => a.Currency).Include(a => a.PaymentTerm).AsQueryable();

                    //if (filters.ProjectID != null)
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where((a => a.ProjectId == filters.ProjectID)).AsQueryable();
                    //}
                    //if (!string.IsNullOrWhiteSpace(filters.DueDateFrom))
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.DueDate >= DueDateFrom).AsQueryable();
                    //}
                    //if(!string.IsNullOrWhiteSpace(filters.DueDateTo))
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.DueDate <= DueDateTo).AsQueryable();
                    //}
                    //if(filters.NumGreaterThanRemain != null)
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.Remain > filters.NumGreaterThanRemain).AsQueryable();
                    //}
                    //if(filters.IsCollected == true)
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.Remain == 0).AsQueryable();

                    //}





                    //var projectPaymentTermList = new GetProjectPaymentTerms();
                    //projectPaymentTermList.PaymentTermsList = new List<GetProjectPaymentTermsDto>();
                    //projectPaymentTermList.pagination = new PaginationHeader();

                    //var projectPaymentTermsPagedList = Helper.PagedList<ProjectPaymentTerm>.Create(projectPaymentTerm,filters.CurrentPage, filters.NumberOfItemsPerPage);

                    //var PaginationHeaderl = new PaginationHeader()
                    //{
                    //    CurrentPage = filters.CurrentPage,
                    //    ItemsPerPage = filters.NumberOfItemsPerPage,
                    //    TotalItems = projectPaymentTermsPagedList.TotalCount,
                    //    TotalPages = projectPaymentTermsPagedList.TotalPages
                    //};
                    ////var projectPaymentTerm = _Context.ProjectPaymentTerms.Where((a => a.Active == true)).Include(a => a.Project.SalesOffer).Include(a => a.Currency).Include(a => a.PaymentTerm).ToList();
                    //var projectPaymentTermData = _mapper.Map<List<GetProjectPaymentTermsDto>>(projectPaymentTermsPagedList);
                    //projectPaymentTermList.PaymentTermsList.AddRange(projectPaymentTermData);

                    //response.Data = projectPaymentTermList;
                    //response.Data.pagination = PaginationHeaderl;
                    //response.Data.TotalRemain = projectPaymentTermData.Sum(a =>a.Remain);
                    //response.Data.TotalCollected = projectPaymentTermData.Sum(a => a.Collected);
                    //return response;
                    var ProjectPaymentTerm = _taskMangerProjectService.GetProjectPaymentTerms(filters);
                    if (!ProjectPaymentTerm.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectPaymentTerm.Errors);
                        return response;
                    }
                    response = ProjectPaymentTerm;

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


        [HttpGet("GetProjectPaymentTermsReport")]
        public BaseResponseWithData<string> GetProjectPaymentTermsReport([FromHeader] GetprojectPaymentTermsFilters filters)
        {
            var response = new BaseResponseWithData<string>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion


            #region DateValidation

            DateTime DueDateFrom = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateFrom))
            {
                if (!DateTime.TryParse(filters.DueDateFrom, out DueDateFrom))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate From ";
                    response.Errors.Add(err);
                    return response;
                }
            }

            DateTime DueDateTo = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(filters.DueDateTo))
            {
                if (!DateTime.TryParse(filters.DueDateTo, out DueDateTo))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid DueDate To ";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            try
            {
                if (response.Result)
                {
                    #region old code
                    //var projectPaymentTerm = _Context.ProjectPaymentTerms.Include(a => a.Project.SalesOffer).Include(a => a.Currency).Include(a => a.PaymentTerm).AsQueryable();

                    //if (filters.ProjectID != null)
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where((a => a.ProjectId == filters.ProjectID)).AsQueryable();
                    //}
                    //if (!string.IsNullOrWhiteSpace(filters.DueDateFrom))
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.DueDate >= DueDateFrom).AsQueryable();
                    //}
                    //if (!string.IsNullOrWhiteSpace(filters.DueDateTo))
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.DueDate <= DueDateTo).AsQueryable();
                    //}
                    //if (filters.NumGreaterThanRemain != null)
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.Remain > filters.NumGreaterThanRemain).AsQueryable();
                    //}
                    //if (filters.IsCollected == true)
                    //{
                    //    projectPaymentTerm = projectPaymentTerm.Where(a => a.Remain == 0).AsQueryable();

                    //}

                    //ExcelPackage excel = new ExcelPackage();
                    //var worksheet = excel.Workbook.Worksheets.Add("ProjectPaymentTerm");
                    //worksheet.DefaultRowHeight = 12;
                    //worksheet.Row(1).Height = 20;
                    //worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //worksheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //worksheet.Row(1).Style.Font.Bold = true;
                    //worksheet.Cells[1, 1, 1, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    //worksheet.Cells[1, 1, 1, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                    //worksheet.Cells[1, 1, 1, 10].Style.Font.Color.SetColor(System.Drawing.Color.White);

                    //worksheet.Cells[1, 1].Value = "ID";
                    //worksheet.Cells[1, 2].Value = "Project";
                    //worksheet.Cells[1, 3].Value = "Payment Term";
                    //worksheet.Cells[1, 4].Value = "Percentage";
                    //worksheet.Cells[1, 5].Value = "Amount";
                    //worksheet.Cells[1, 6].Value = "Currency";
                    //worksheet.Cells[1, 7].Value = "Due Date";
                    //worksheet.Cells[1, 8].Value = "Collected";
                    //worksheet.Cells[1, 9].Value = "CollectionDate";
                    //worksheet.Cells[1, 10].Value = "Remain";

                    //if (projectPaymentTerm.Count() > 0)
                    //{
                    //    var list = projectPaymentTerm.ToList();
                    //    int recordIndex = 2;
                    //    foreach (var item in list)
                    //    {
                    //        worksheet.Cells[recordIndex, 1].Value = item.Id;
                    //        worksheet.Cells[recordIndex, 2].Value = item.Project.SalesOffer.ProjectName;
                    //        worksheet.Cells[recordIndex, 3].Value = item.PaymentTerm.PaymentTermName;
                    //        worksheet.Cells[recordIndex, 4].Value = item.Percentage;
                    //        worksheet.Cells[recordIndex, 5].Value = item.Amount;
                    //        worksheet.Cells[recordIndex, 6].Value = item.Currency.Name;
                    //        worksheet.Cells[recordIndex, 7].Value = item.DueDate.ToShortDateString();
                    //        worksheet.Cells[recordIndex, 8].Value = item.Collected;
                    //        worksheet.Cells[recordIndex, 9].Value = item.CollectionDate.ToShortDateString();
                    //        worksheet.Cells[recordIndex, 10].Value = item.Remain;
                    //        worksheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    //        worksheet.Row(recordIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    //        recordIndex++;
                    //    }
                    //    worksheet.Column(4).Style.Numberformat.Format = "#0\\.00%";
                    //    worksheet.Column(5).Style.Numberformat.Format = "#,##0.00";
                    //    worksheet.Column(7).Style.Numberformat.Format = "yyyy/mm/dd";
                    //    worksheet.Column(8).Style.Numberformat.Format = "#,##0.00";
                    //    worksheet.Column(9).Style.Numberformat.Format = "yyyy/mm/dd";
                    //    worksheet.Column(10).Style.Numberformat.Format = "#,##0.00";

                    //    worksheet.Column(1).AutoFit();
                    //    worksheet.Column(2).AutoFit();
                    //    worksheet.Column(3).AutoFit();
                    //    worksheet.Column(4).AutoFit();
                    //    worksheet.Column(5).AutoFit();
                    //    worksheet.Column(6).AutoFit();
                    //    worksheet.Column(7).AutoFit();
                    //    worksheet.Column(8).AutoFit();
                    //    worksheet.Column(9).AutoFit();
                    //    worksheet.Column(10).AutoFit();

                    //    var path = $"Attachments\\{validation.CompanyName}\\ProjectPaymentTerms";
                    //    var savedPath = Path.Combine(_host.WebRootPath, path);
                    //    if (System.IO.File.Exists(savedPath))
                    //        System.IO.File.Delete(savedPath);

                    //    // Create excel file on physical disk  
                    //    Directory.CreateDirectory(savedPath);
                    //    //FileStream objFileStrm = File.Create(savedPath);
                    //    //objFileStrm.Close();
                    //    var excelPath = savedPath + $"\\ProjectPaymentTermReport.xlsx";
                    //    excel.SaveAs(excelPath);
                    //    // Write content to excel file  
                    //    //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                    //    //Close Excel package 
                    //    excel.Dispose();
                    //    response.Data = Globals.baseURL + '\\' + path + $"\\ProjectPaymentTermReport.xlsx";
                    //}
                    #endregion

                    var ProjectPaymentTerm = _taskMangerProjectService.GetProjectPaymentTermsReport(filters, validation.CompanyName);
                    if (!ProjectPaymentTerm.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectPaymentTerm.Errors);
                        return response;
                    }
                    response = ProjectPaymentTerm;
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

        [HttpGet("GetPaymentTermDDL")]                      
        public BaseResponseWithData<List<GetPaymentTermDDL>> GetPaymentTermsDDL()
        {
            var response = new BaseResponseWithData<List<GetPaymentTermDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                //if (response.Result)
                //{
                //    var PaymentTermList = _Context.PaymentTerms.OrderBy(a => a.PaymentTermName).ToList();
                //    //PaymentTermList = PaymentTermList.OrderBy(a => a.PaymentTermName);
                //    var PaymentTermDDL = new List<GetPaymentTermDDL>();
                //    foreach (var PaymentTerm in PaymentTermList)
                //    {
                //        var DDLObj = new GetPaymentTermDDL();
                //        DDLObj.ID = PaymentTerm.Id;
                //        DDLObj.Name = PaymentTerm.PaymentTermName.Trim();

                //        PaymentTermDDL.Add(DDLObj);
                //    }
                //    response.Data = PaymentTermDDL;

                //}

                //return response;

                if (response.Result)
                {
                    var PaymentTermsList = _taskMangerProjectService.GetPaymentTermsDDL();
                    if (!PaymentTermsList.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(PaymentTermsList.Errors);
                    }
                    response = PaymentTermsList;
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

        [HttpPost("AddPaymentTerm")]                        
        public BaseResponseWithId<int> AddPaymentTerm([FromForm]string paymentTermName)
        {
            var response = new BaseResponseWithId<int>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            try
            {
                if (response.Result)
                {
                    var newPaymentTerm = _taskMangerProjectService.AddPaymentTerms(paymentTermName);
                    if (!newPaymentTerm.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(newPaymentTerm.Errors);
                        return response;
                    }
                    response.ID = newPaymentTerm.ID;
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

        [HttpPost("EditProjectPaymentTerms")]               
        public BaseResponseWithId<long> EditProjectPaymentTerms([FromBody]EditProjectPaymentTermsDto Dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region user Auth
            HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
            response.Errors = validation.errors;
            response.Result = validation.result;
            #endregion

            #region validation
            if (Dto.PaymentTermID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "PaymentTermID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.ProjectID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "project ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.CurrencyID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Currency ID Is Required";
                response.Errors.Add(error);
                return response;
            }
            //if (Dto.DailyJournalEntryID == 0)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err103";
            //    error.ErrorMSG = "DailyJournalEntryID Is Required";
            //    response.Errors.Add(error);
            //    return response;
            //}
            if (Dto.Amount == null && Dto.Percentage == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Amount and percentage can not be null at the same time, please enter one of them";
                response.Errors.Add(error);
                return response;
            }
            if (Dto.Amount != null && Dto.Percentage != null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Amount and percentage can not be not null at the same time, please enter only one of them";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            #region DB check
            //var projectPaymentTerm = _Context.ProjectPaymentTerms.Where(a => a.Id == Dto.ID).FirstOrDefault();
            //if (projectPaymentTerm == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No projectPaymentTerm with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var project = _Context.Projects.Where(a => a.Id == Dto.ProjectID).Include(a =>a.SalesOffer).FirstOrDefault();
            //if (project == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Project with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var paymentTerm = _Context.PaymentTerms.Find(Dto.PaymentTermID);
            //if (paymentTerm == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No Payment Term with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            //var currency = _Context.Currencies.Find(Dto.CurrencyID);
            //if (currency == null)
            //{
            //    response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err101";
            //    error.ErrorMSG = "No currency with This ID";
            //    response.Errors.Add(error);
            //    return response;
            //}
            ////var DailyJournalEntry = _unitOfWork.DailyJournalEntries.GetById(Dto.DailyJournalEntryID);
            ////if (DailyJournalEntry == null)
            ////{
            ////    response.Result = false;
            ////    Error error = new Error();
            ////    error.ErrorCode = "Err101";
            ////    error.ErrorMSG = "No Daily Journal Entry with This ID";
            ////    response.Errors.Add(error);
            ////    return response;
            ////}
            #endregion

            #region Date Validation
            DateTime DueDate = DateTime.Now;
            if (!DateTime.TryParse(Dto.DueDate, out DueDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid Start Date:";
                response.Errors.Add(err);
                return response;
            }

            //DateTime CollectionDate = DateTime.Now;
            //if (!DateTime.TryParse(Dto.CollectionDate, out CollectionDate))
            //{
            //    response.Result = false;
            //    Error err = new Error();
            //    err.ErrorCode = "E-1";
            //    err.errorMSG = "please, Enter a valid Collection Date:";
            //    response.Errors.Add(err);
            //    return response;
            //}
            #endregion

            try
            {
                if (response.Result)
                {
                    
                    var ProjectPaymentTerm = _taskMangerProjectService.EditProjectPaymentTerms(Dto, validation.userID);
                    if (!ProjectPaymentTerm.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(ProjectPaymentTerm.Errors);
                        return response;
                    }
                    response.ID = ProjectPaymentTerm.ID;
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

        [HttpGet("GetPaymentMethodDDl")]
        public BaseResponseWithData<List<CostTypeDDL>> GetPaymentMethodDDl()
        {
            var response = new BaseResponseWithData<List<CostTypeDDL>>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {
                    var paymentMethods = _taskMangerProjectService.GetPaymentMethodDDl();
                    if(!paymentMethods.Result)
                    {
                        response.Result = false;
                        response.Errors.AddRange(paymentMethods.Errors);
                        return response;
                    }
                    response = paymentMethods;

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
    }
}
