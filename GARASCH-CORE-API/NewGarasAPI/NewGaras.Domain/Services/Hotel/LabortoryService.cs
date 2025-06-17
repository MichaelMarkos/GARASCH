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
using Microsoft.AspNetCore.Identity;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Hotel;
using NewGaras.Infrastructure.Entities;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using System.Web;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;

namespace NewGaras.Domain.Services.Hotel
{
    public class LabortoryService : ILabortoryService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LabortoryService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;

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



        public async Task<BaseResponseWithDataPaging<List<LaboratoryMessagesReport>>> GetLaboratoryMessagePagingList(LaboratoryHeader filters)
        {
            BaseResponseWithDataPaging<List<LaboratoryMessagesReport>> Response = new BaseResponseWithDataPaging<List<LaboratoryMessagesReport>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                int CurrentPage = 0;
                int TotalPages = 0;
                int ItemsPerPage = 0;
                int TotalItems = 0;
                string NameDecode = null;
                string LabNameDecode = null;
                Expression<Func<LaboratoryMessagesReport, bool>> filter = (item => item.Id > 0);

                Expression<Func<LaboratoryMessagesReport, object>> labratorymessageOrderby = (x => x.Id);


                //   var laboratoryDBlist = _unitOfWork.LaboratoryMessagesReports.FindAll(x=>x.Id >0);
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    NameDecode = HttpUtility.UrlDecode(filters.Name);
                }
                if (!string.IsNullOrWhiteSpace(filters.NameLab))
                {
                    LabNameDecode = HttpUtility.UrlDecode(filters.NameLab);
                }
                //if (!string.IsNullOrWhiteSpace(filters.Mobile))
                //{
                //    laboratoryDBlist = laboratoryDBlist.Where(x => x.Mobile == filters.Mobile).AsQueryable();
                //}
                //if (filters.FromDate != null && filters.ToDate != null)
                //{
                //    laboratoryDBlist = laboratoryDBlist.Where(x => x.CreateDate.Date >=  ((DateTime)filters.FromDate).Date  && x.CreateDate.Date <= ((DateTime)filters.ToDate).Date).AsQueryable();
                //}
                //if (filters.Date != null )
                //{
                //    laboratoryDBlist = laboratoryDBlist.Where(x => x.CreateDate.Date == ((DateTime)filters.Date).Date ).AsQueryable();
                //}
                //if (filters.CreateBy != 0)
                //{
                //    laboratoryDBlist = laboratoryDBlist.Where(x => x.CreateBy == filters.CreateBy).AsQueryable();
                //}
                //if (filters.Result != null)
                //{
                //    laboratoryDBlist = laboratoryDBlist.Where(x => x.Result == filters.Result).AsQueryable();
                //}

                filter = (x =>
                                  (filters.Name == null || x.Name.Contains(NameDecode))
                                && (filters.NameLab == null || x.NameLab.Contains(LabNameDecode))
                                && (filters.Date == null || x.CreateDate == ((DateTime)filters.Date).Date)
                                && (filters.FromDate == null || filters.ToDate == null || (x.CreateDate.Date >= ((DateTime)filters.FromDate).Date && x.CreateDate.Date <= ((DateTime)filters.ToDate).Date))
                                && (filters.Result == null || (x.Result == filters.Result))
                                && (filters.Mobile == null || (x.Mobile == filters.Mobile)));




                var _laboratoryPaging = await _unitOfWork.LaboratoryMessagesReports.FindAllPagingAsync(filter,
                                                                                        filters.PageNo, filters.NoOfItems, null, labratorymessageOrderby, "DESC"
                                                                                        );

                // _laboratoryPaging.TotalCount = laboratoryDBlist.Count();

                Response.Data = _laboratoryPaging;
                CurrentPage = filters.PageNo ?? 0;
                ItemsPerPage = filters.NoOfItems ?? 8;
                TotalItems = _laboratoryPaging.TotalCount;
                TotalPages = _laboratoryPaging.TotalPages;

                Response.Result = true;
                Response.TotalPages = TotalPages;
                Response.TotalItems = TotalItems;
                Response.PageNo = CurrentPage;
                Response.NoOfItems = ItemsPerPage;
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






        public async Task<BaseResponseWithData<string>> MessageReportExcell(LaboratoryHeader filters)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            string NameDecode = null;
            string LabNameDecode = null;

            try
            {
                Expression<Func<LaboratoryMessagesReport, bool>> filter = (item => item.Id > 0);



                //   var laboratoryDBlist = _unitOfWork.LaboratoryMessagesReports.FindAll(x=>x.Id >0);
                if (!string.IsNullOrWhiteSpace(filters.Name))
                {
                    NameDecode = HttpUtility.UrlDecode(filters.Name);
                }
                if (!string.IsNullOrWhiteSpace(filters.NameLab))
                {
                    LabNameDecode = HttpUtility.UrlDecode(filters.NameLab);
                }


                filter = (x =>
                                  (filters.Name == null || x.Name.Contains(NameDecode))
                                && (filters.NameLab == null || x.NameLab.Contains(LabNameDecode))
                && (filters.Date == null || x.CreateDate == ((DateTime)filters.Date).Date)
                && (filters.FromDate == null || filters.ToDate == null || (x.CreateDate.Date >= ((DateTime)filters.FromDate).Date && x.CreateDate.Date <= ((DateTime)filters.ToDate).Date))
                && (filters.Result == null || (x.Result == filters.Result))
                && (filters.Mobile == null || (x.Mobile == filters.Mobile)));


                var _laboratoryDB = await _unitOfWork.LaboratoryMessagesReports.FindAllAsync(filter);
                var labratorymessageOrderbyDate = _laboratoryDB.OrderByDescending(x => x.CreateDate).ToList();


                ExcelPackage.LicenseContext = LicenseContext.Commercial;


                ExcelPackage excel = new ExcelPackage();
                var sheet = excel.Workbook.Worksheets.Add("Messages Report");
                sheet.Cells[1, 1].Value = "SerialNo";
                sheet.Cells[1, 2].Value = "Id";
                sheet.Cells[1, 3].Value = "Name";
                sheet.Cells[1, 4].Value = "Mobil";
                sheet.Cells[1, 5].Value = "Namelab";
                sheet.Cells[1, 6].Value = "PdfUrl";
                sheet.Cells[1, 7].Value = "Cost";
                sheet.Cells[1, 8].Value = "Result";
                sheet.Cells[1, 9].Value = "Date";
                sheet.Cells[1, 10].Value = "Create By";

                int z = 1;
                int y = 2;
                if (labratorymessageOrderbyDate.Count > 0)
                {
                    foreach (var item in labratorymessageOrderbyDate)
                    {
                        sheet.Cells[y, 1].Value = z;
                        sheet.Cells[y, 2].Value = item.Id;
                        sheet.Cells[y, 3].Value = item.Name;
                        sheet.Cells[y, 4].Value = item.Mobile;
                        sheet.Cells[y, 5].Value = item.NameLab;
                        sheet.Cells[y, 6].Value = item.PdfUrl;
                        sheet.Cells[y, 7].Value = item.Cost;
                        sheet.Cells[y, 8].Value = item.Result;
                        sheet.Cells[y, 9].Value = item.CreateDate.ToShortDateString();
                        sheet.Cells[y, 10].Value = item.CreateBy;
                        z++;
                        y++;
                    }
                }

                var path = $"Attachments\\MessageReportExcel";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                // Check if the directory exists
                if (Directory.Exists(savedPath))
                {
                    // Delete all files in the directory
                    foreach (var file in Directory.GetFiles(savedPath))
                    {
                        System.IO.File.Delete(file);
                    }

                    // Delete all subdirectories in the directory
                    foreach (var dir in Directory.GetDirectories(savedPath))
                    {
                        Directory.Delete(dir, true); // true to delete recursively
                    }
                }
                else
                {
                    // Create the directory if it doesn't exist
                    Directory.CreateDirectory(savedPath);
                }

                var dateNow = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\DownloadMessageReport_{dateNow}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var filePath = BaseURL + '\\' + path + $"\\DownloadMessageReport_{dateNow}.xlsx";
                Response.Data = filePath;
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


    }
}
