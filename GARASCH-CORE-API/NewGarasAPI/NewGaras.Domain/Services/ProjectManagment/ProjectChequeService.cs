using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectManagement;
using NewGarasAPI.Models.ProjectManagement;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace NewGaras.Domain.Services.ProjectManagment
{
    public class ProjectChequeService : IProjectChequeService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectChequeService(ITenantService tenantService, IMapper mapper,IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<long> AddNewCheque(ProjectChequeDto dto,string CompanyName,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                var chequeChasingStatus = _unitOfWork.ChequeCashingStatuses.Find(a=>a.Id==dto.ChequeChashingStatusId);
                if (chequeChasingStatus == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No chequeCashingStatus with This ID";
                    Response.Errors.Add(error);
                    return Response;
                }

                var currency = _unitOfWork.Currencies.Find(a => a.Id == dto.CurrencyId);
                if (currency == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No currency with this ID";
                    Response.Errors.Add(error);
                    return Response;
                }

                Project project = null;
                if (dto.ProjectId != null)
                {
                    
                    project = _unitOfWork.Projects.FindAll(a => a.Id == dto.ProjectId, includes: new[] { "SalesOffer.Client", "SalesOffer.SalesPerson" }).FirstOrDefault();
                    //_Context.Projects.Include(a => a.SalesOffer.Client).Include(a => a.SalesOffer.SalesPerson).Where(a => a.Id == dto.ProjectId).FirstOrDefault();
                    if (project == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project Is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                MaintenanceFor maintenanceFor = null;
                if (dto.MaintenanceForID != null)
                {

                    maintenanceFor = _unitOfWork.MaintenanceFors.GetById(dto.MaintenanceForID??0);
                    if (maintenanceFor == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Product Is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                ManagementOfMaintenanceOrder maintenanceOrder = null;
                if (dto.MaintenanceOrderID != null)
                {

                    maintenanceOrder = _unitOfWork.ManagementOfMaintenanceOrders.GetById(dto.MaintenanceOrderID??0);
                    if (maintenanceOrder == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Product Contract Is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }



                var cheque = _mapper.Map<ProjectCheque>(dto);
                cheque.ModifiedDate = DateTime.Now;
                cheque.CreationDate = DateTime.Now;
                cheque.Active = true;
                cheque.ModifiedBy = creator;
                cheque.CreatedBy = creator;
                if (project != null)
                {
                    cheque.ProjectId = project.Id;
                    cheque.ProjectName = project.SalesOffer.ProjectName;
                    cheque.ProjectNumber = project.ProjectSerial;
                    cheque.ClientName = project.SalesOffer.Client.Name;
                    cheque.SalesPersonName = project.SalesOffer.SalesPerson.FirstName + " " + project.SalesOffer.SalesPerson.LastName;
                }
                if (dto.Attachment != null)
                {
                    var fileExtension = dto.Attachment.FileName.Split('.').Last();
                    var virtualPath = $@"Attachments\{CompanyName}\ProjectCheque\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.Attachment.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, dto.Attachment, FileName, fileExtension, _host);
                    cheque.AttachmentPath = AttachPath;
                }
                _unitOfWork.ProjectCheques.Add(cheque);
                _unitOfWork.Complete();

                Response.ID = cheque.Id;
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithId<long>> AddNewChequeList(List<ProjectChequeDto> dto, string CompanyName, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            var chequeslist =new List<ProjectCheque>(){ };
            try
            {
                foreach(var item in dto)
                {
                    var chequeChasingStatus = _unitOfWork.ChequeCashingStatuses.Find(a => a.Id == item.ChequeChashingStatusId);
                    if (chequeChasingStatus == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "No chequeChasingStatus with This ID for cheque "+ dto.IndexOf(item)+1 ;
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var currency = _unitOfWork.Currencies.Find(a => a.Id == item.CurrencyId);
                    if (currency == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "No currency with this ID For Cheque "+dto.IndexOf(item)+1;
                        Response.Errors.Add(error);
                        return Response;
                    }

                    Project project = null;
                    if (item.ProjectId != null)
                    {
                        project = _unitOfWork.Projects.FindAll(a => a.Id == item.ProjectId, includes: new[] { "SalesOffer.Client", "SalesOffer.SalesPerson" }).FirstOrDefault();
                        //_Context.Projects.Include(a => a.SalesOffer.Client).Include(a => a.SalesOffer.SalesPerson).Where(a => a.Id == dto.ProjectId).FirstOrDefault();
                        if (project == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Project Is not found for Cheque "+dto.IndexOf(item)+1;
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    var cheque = _mapper.Map<ProjectCheque>(item);
                    cheque.ModifiedDate = DateTime.Now;
                    cheque.CreationDate = DateTime.Now;
                    cheque.Active = true;
                    cheque.ModifiedBy = creator;
                    cheque.CreatedBy = creator;
                    if (project != null)
                    {
                        cheque.ProjectId = project.Id;
                        cheque.ProjectName = project.SalesOffer.ProjectName;
                        cheque.ProjectNumber = project.ProjectSerial;
                        cheque.ClientName = project.SalesOffer.Client.Name;
                        cheque.SalesPersonName = project.SalesOffer.SalesPerson.FirstName + " " + project.SalesOffer.SalesPerson.LastName;
                    }
                    if (item.Attachment != null)
                    {
                        var fileExtension = item.Attachment.FileName.Split('.').Last();
                        var virtualPath = $@"Attachments\{CompanyName}\ProjectCheque\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(item.Attachment.FileName.Trim().Replace(" ", ""));
                        var AttachPath = Common.SaveFileIFF(virtualPath, item.Attachment, FileName, fileExtension, _host);
                        cheque.AttachmentPath = AttachPath;
                    }
                    chequeslist.Add(cheque);
                }
                await _unitOfWork.ProjectCheques.AddRangeAsync(chequeslist);
                _unitOfWork.Complete();
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithDataAndHeader<GetAllProjectChequesResponse> GetAllProjectCheques(GetAllProjectChequesFilter filter,string CompanyName)
        {
            BaseResponseWithDataAndHeader<GetAllProjectChequesResponse> Response = new BaseResponseWithDataAndHeader<GetAllProjectChequesResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var Cheques = _unitOfWork.ProjectCheques.FindAll(a => true, includes: new[] { "ChequeChashingStatus", "Currency", "WithdrawedByNavigation", "Project.SalesOffer.Client", "CreatedByNavigation" }).AsQueryable();
                
                    if (!string.IsNullOrEmpty(filter.Bank))
                    {
                        Cheques = Cheques.Where(a => a.Bank.Trim().ToLower().Contains(filter.Bank.Trim().ToLower())).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filter.Branch))
                    {
                        Cheques = Cheques.Where(a => a.BankBranch.Trim().ToLower().Contains(filter.Branch.Trim().ToLower())).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filter.clientName))
                    {
                        Cheques = Cheques.Where(a => a.ClientName.Trim().ToLower().Contains(filter.clientName.Trim().ToLower())).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filter.ProjectName))
                    {
                        Cheques = Cheques.Where(a => a.ProjectName.Trim().ToLower().Contains(filter.ProjectName.Trim().ToLower())).AsQueryable();
                    }
                    if (filter.cashingStatus != null)
                    {
                        Cheques = Cheques.Where(a => a.ChequeChashingStatusId == filter.cashingStatus).AsQueryable();
                    }
                    if (filter.month != null)
                    {
                        Cheques = Cheques.Where(a => a.ChequeDate.Month == filter.month).AsQueryable();
                    }
                    if (filter.year != null)
                    {
                        Cheques = Cheques.Where(a => a.ChequeDate.Year == filter.year).AsQueryable();
                    }
                    if (filter.IsCrossed != null)
                    {
                        Cheques = Cheques.Where(a => a.IsCrossedCheque==filter.IsCrossed).AsQueryable();
                    }
                    if (filter.MaintenanceForID != null)
                    {
                        Cheques = Cheques.Where(a => a.MaintenanceForId == filter.MaintenanceForID).AsQueryable();
                    }
                    if (filter.MaintenanceOrderID != null)
                    {
                        Cheques = Cheques.Where(a => a.MaintenanceOrderId == filter.MaintenanceOrderID).AsQueryable();
                    }
                /*if (chequeStatusId != null && chequeStatusId != 0)
                {
                    if (chequeStatusId == 1)
                    {
                        Cheques = Cheques.Where(a => a.ChequeChashingStatusId == null).AsQueryable();
                    }
                    else if (chequeStatusId == 2)
                    {
                        Cheques = Cheques.Where(a => a.ChequeChashingStatusId == 1).AsQueryable();
                    }
                    else if (chequeStatusId == 3)
                    {
                        Cheques = Cheques.Where(a => a.ChequeChashingStatusId == 2 || a.ChequeChashingStatusId == 5).AsQueryable();
                    }
                    else if (chequeStatusId == 4)
                    {
                        Cheques = Cheques.Where(a => a.ChequeChashingStatusId == 6).AsQueryable();
                    }
                }*/


                var chequesList = Cheques.ToList();

                var List = _mapper.Map<List<GetProjectChequeDto>>(chequesList);
                Response.Data = new GetAllProjectChequesResponse();
                var NotRecieved = chequesList.Where(a => a.ChequeChashingStatusId == 5).GroupBy(a => a.Currency.Name).ToList();
                var Recieved = chequesList.Where(a => a.ChequeChashingStatusId == 1).GroupBy(a => a.Currency.Name).ToList();
                var UnderCollection = chequesList.Where(a => a.ChequeChashingStatusId == 2).GroupBy(a => a.Currency.Name).ToList();
                var Refused = chequesList.Where(a => a.ChequeChashingStatusId == 3).GroupBy(a => a.Currency.Name).ToList();
                var Collected = chequesList.Where(a => a.ChequeDate.Month == DateTime.Now.Month && a.ChequeDate.Year == DateTime.Now.Year && (a.ChequeChashingStatusId==4 || a.ChequeChashingStatusId == 6)).GroupBy(a => a.Currency.Name).ToList();
                Response.Data.NotRecieved = NotRecieved.Select(a => new ChequeStatusSummary() { Currancy = a.Key, Count = a.Count(), Sum = a.Sum(x => x.ChequeAmount) }).ToList();
               
                Response.Data.Recieved = Recieved.Select(a => new ChequeStatusSummary() { Currancy = a.Key, Count = a.Count(), Sum = a.Sum(x => x.ChequeAmount) }).ToList();

                Response.Data.UnderCollection = UnderCollection.Select(a => new ChequeStatusSummary() { Currancy = a.Key, Count = a.Count(), Sum = a.Sum(x => x.ChequeAmount) }).ToList();

                Response.Data.Refused = Refused.Select(a => new ChequeStatusSummary() { Currancy = a.Key, Count = a.Count(), Sum = a.Sum(x => x.ChequeAmount) }).ToList();

                Response.Data.Collected = Collected.Select(a => new ChequeStatusSummary() { Currancy = a.Key, Count = a.Count(), Sum = a.Sum(x => x.ChequeAmount) }).ToList();
                #region Excel sheet
                ExcelPackage excel = new ExcelPackage();
                var ChequesSheet = excel.Workbook.Worksheets.Add("ProjectCheques");
                ChequesSheet.DefaultRowHeight = 12;
                ChequesSheet.Row(1).Height = 20;
                ChequesSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ChequesSheet.Row(1).Style.Font.Bold = true;
                ChequesSheet.Cells[1, 1, 1, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ChequesSheet.Cells[1, 1, 1, 18].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                ChequesSheet.Cells[1, 1, 1, 18].Style.Font.Color.SetColor(Color.White);
                ChequesSheet.Cells[1, 1].Value = "Cheque Number";
                ChequesSheet.Cells[1, 2].Value = "Date";
                ChequesSheet.Cells[1, 3].Value = "Bank";
                ChequesSheet.Cells[1, 4].Value = "Bank Branch";
                ChequesSheet.Cells[1, 5].Value = "Amount";
                ChequesSheet.Cells[1, 6].Value = "Currency";
                ChequesSheet.Cells[1, 7].Value = "";
                ChequesSheet.Cells[1, 8].Value = "Cheque Cashing Status";
                ChequesSheet.Cells[1, 9].Value = "Is Crossed Cheque";
                ChequesSheet.Cells[1, 10].Value = "Reject Cause";
                ChequesSheet.Cells[1, 11].Value = "Notes";
                ChequesSheet.Cells[1, 12].Value = "WithDraw Date";
                ChequesSheet.Cells[1, 13].Value = "WithDrawed By";
                ChequesSheet.Cells[1, 14].Value = "";
                ChequesSheet.Cells[1, 15].Value = "Client";
                ChequesSheet.Cells[1, 16].Value = "Sales Person";
                ChequesSheet.Cells[1, 17].Value = "Project";
                ChequesSheet.Cells[1, 18].Value = "Project Number";
                if(List.Count > 0)
                {
                    int recordIndex = 2;
                    foreach (var item in List)
                    {
                        ChequesSheet.Cells[recordIndex, 1].Value = item.ChequeNumber;
                        ChequesSheet.Cells[recordIndex, 2].Value = item.ChequeDate;
                        ChequesSheet.Cells[recordIndex, 3].Value = item.Bank ?? "N/A";
                        ChequesSheet.Cells[recordIndex, 4].Value = item.BankBranch ?? "N/A";
                        ChequesSheet.Cells[recordIndex, 5].Value = item.ChequeAmount;
                        ChequesSheet.Cells[recordIndex, 6].Value = item.Currency ?? "N/A";

                        ChequesSheet.Cells[recordIndex, 8].Value = item.ChequeCashingStatus??"Not Recieved - لم يستلم";
                        ChequesSheet.Cells[recordIndex, 9].Value = item.IsCrossedCheque;
                        ChequesSheet.Cells[recordIndex, 10].Value = item.RejectCause ?? "N/A";
                        ChequesSheet.Cells[recordIndex, 11].Value = item.Notes ?? "N/A";
                        ChequesSheet.Cells[recordIndex, 12].Value = item.WithDrawDate;
                        ChequesSheet.Cells[recordIndex, 13].Value = item.WithDrawedByName;

                        ChequesSheet.Cells[recordIndex, 15].Value = item.ClientName??"N/A";
                        ChequesSheet.Cells[recordIndex, 16].Value = item.SalesPersonName??"N/A";
                        ChequesSheet.Cells[recordIndex, 17].Value = item.ProjectName??"N/A";
                        ChequesSheet.Cells[recordIndex, 18].Value = item.ProjectNumber??"N/A";
                        ChequesSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        recordIndex++;

                    }

                    ChequesSheet.Column(1).AutoFit();
                    ChequesSheet.Column(2).AutoFit();
                    ChequesSheet.Column(2).Style.Numberformat.Format = "yyyy/mm/dd";
                    ChequesSheet.Column(3).AutoFit();
                    ChequesSheet.Column(4).AutoFit();
                    ChequesSheet.Column(5).AutoFit();
                    ChequesSheet.Column(5).Style.Numberformat.Format = "#,##0.00";
                    ChequesSheet.Column(6).AutoFit();

                    ChequesSheet.Column(7).Width = 0.58;
                    ChequesSheet.Cells[1, 7, recordIndex, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ChequesSheet.Cells[1, 7, recordIndex, 7].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);

                    ChequesSheet.Column(8).AutoFit();
                    ChequesSheet.Column(9).AutoFit();
                    ChequesSheet.Column(10).AutoFit();
                    ChequesSheet.Column(11).AutoFit();
                    ChequesSheet.Column(12).AutoFit();
                    ChequesSheet.Column(12).Style.Numberformat.Format = "yyyy/mm/dd";
                    ChequesSheet.Column(13).AutoFit();

                    ChequesSheet.Column(14).Width = 0.58;
                    ChequesSheet.Cells[1,14,recordIndex,14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ChequesSheet.Cells[1, 14, recordIndex, 14].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);

                    ChequesSheet.Column(15).AutoFit();
                    ChequesSheet.Column(16).AutoFit();
                    ChequesSheet.Column(17).AutoFit();
                    ChequesSheet.Column(18).AutoFit();

                    ChequesSheet.Row(recordIndex).Height = 3;
                    ChequesSheet.Cells[recordIndex, 1, recordIndex, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ChequesSheet.Cells[recordIndex, 1, recordIndex, 18].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);

                    var path = $"Attachments\\{CompanyName}\\ProjectChequesReports";
                    var savedPath = Path.Combine(_host.WebRootPath, path);
                    if (File.Exists(savedPath))
                        File.Delete(savedPath);

                    // Create excel file on physical disk  
                    Directory.CreateDirectory(savedPath);
                    //FileStream objFileStrm = File.Create(savedPath);
                    //objFileStrm.Close();
                    var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                    var excelPath = savedPath + $"\\ProjectChequeReport_{date}.xlsx";
                    excel.SaveAs(excelPath);
                    // Write content to excel file  
                    //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                    //Close Excel package 
                    excel.Dispose();
                    Response.Data.ReportPath = Globals.baseURL + '\\' + path + $"\\ProjectChequeReport_{date}.xlsx";
                }
                #endregion

                var returnedList = PagedList<GetProjectChequeDto>.Create(List.AsQueryable(), filter.pageNumber, filter.pageSize);
                Response.Data.Cheques = returnedList;
                Response.PaginationHeader = new PaginationHeader
                {
                    CurrentPage = returnedList.CurrentPage,
                    TotalPages = returnedList.TotalPages,
                    ItemsPerPage = returnedList.PageSize,
                    TotalItems = returnedList.TotalCount
                };

                return Response;
            }
            catch( Exception ex )
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<GetProjectChequeDto> GetChequeById([FromHeader] long ChequeId)
        {
            BaseResponseWithData<GetProjectChequeDto> Response = new BaseResponseWithData<GetProjectChequeDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var cheque = _unitOfWork.ProjectCheques.Find(a=>a.Id==ChequeId, includes: new[] { "ChequeChashingStatus", "Currency", "WithdrawedByNavigation", "Project.SalesOffer.Client", "CreatedByNavigation"});
                if (cheque == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project cheque Is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var data = _mapper.Map<GetProjectChequeDto>(cheque);
                Response.Data = data;

                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<GetChequeStatusModel>> GetChequeStatusDDL()
        {
            BaseResponseWithData<List<GetChequeStatusModel>> Response = new BaseResponseWithData<List<GetChequeStatusModel>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ChequeCashingStatuses = _unitOfWork.ChequeCashingStatuses.GetAll();
                var ChequeStatusList = new List<GetChequeStatusModel>();

                foreach (var cheque in ChequeCashingStatuses)
                {
                    var ChequeStatus = new GetChequeStatusModel();
                    ChequeStatus.Id = cheque.Id;
                    ChequeStatus.Name = cheque.Status;

                    ChequeStatusList.Add(ChequeStatus);
                }
                Response.Data = ChequeStatusList;
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

        public BaseResponseWithId<long> UpdateCheque([FromForm] ProjectChequeDto dto, string CompanyName, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                {
                    if (dto.Id == null || dto.Id == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "cheque Id Is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var cheque = _Context.ProjectCheques.Find(dto.Id);
                    if (cheque == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project cheque Is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    NewGaras.Infrastructure.Entities.Project project = null;
                    if (dto.ProjectId != null)
                    {
                        project = _unitOfWork.Projects.Find(a => a.Id == dto.ProjectId, includes: new[] { "SalesOffer.Client", "SalesOffer.SalesPerson" });
                        if (project == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Project Is not found";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    _mapper.Map<ProjectChequeDto, ProjectCheque>(dto, cheque);
                    cheque.ModifiedDate = DateTime.Now;
                    cheque.ModifiedBy = creator;
                    cheque.Active = true;
                    if (project != null)
                    {
                        cheque.ProjectId = project.Id;
                        cheque.ProjectName = project.SalesOffer.ProjectName;
                        cheque.ProjectNumber = project.ProjectSerial;
                        cheque.ClientName = project.SalesOffer.Client.Name;
                        cheque.SalesPersonName = project.SalesOffer.SalesPerson.FirstName + " " + project.SalesOffer.SalesPerson.LastName;
                    }

                    if (cheque.AttachmentPath != null && dto.DeleteAttachment == true)
                    {
                        var oldpath = Path.Combine(_host.WebRootPath, cheque.AttachmentPath);
                        if (System.IO.File.Exists(oldpath))
                        {
                            System.IO.File.Delete(oldpath);
                            cheque.AttachmentPath = null;
                        }
                    }
                    if (dto.Attachment != null)
                    {
                        if (cheque.AttachmentPath != null)
                        {
                            var oldpath = Path.Combine(_host.WebRootPath, cheque.AttachmentPath);
                            if (System.IO.File.Exists(oldpath))
                            {
                                System.IO.File.Delete(oldpath);
                                cheque.AttachmentPath = null;
                            }
                        }

                        var fileExtension = dto.Attachment.FileName.Split('.').Last();
                        var virtualPath = $@"Attachments\{CompanyName}\ProjectCheque\"; ;
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.Attachment.FileName.Trim().Replace(" ", ""));
                        var AttachPath = Common.SaveFileIFF(virtualPath, dto.Attachment, FileName, fileExtension, _host);
                        cheque.AttachmentPath = AttachPath;
                    }

                    _Context.ProjectCheques.Update(cheque);
                    _Context.SaveChanges();

                    Response.ID = cheque.Id;
                    return Response;
                }
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<int> AddBankChequeTemplate(BankChequeTemplatedto dto,long creator,string CompanyName)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (dto == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "you sent Invalid Data";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (dto.Id != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "you can't sent an Id";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (dto.BankName == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "BankName Is required";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (dto.Template == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Template File is Required";
                    Response.Errors.Add(error);
                    return Response;
                }

                var bank = _unitOfWork.BankChequeTemplates.FindAll(a=>a.BankName.Trim().ToLower() == dto.BankName.Trim().ToLower()).FirstOrDefault();
                if (bank != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err105";
                    error.ErrorMSG = "Bank Name Already Exist!";
                    Response.Errors.Add(error);
                    return Response;
                }

                var fileExtension = dto.Template.FileName.Split('.').Last();
                var virtualPath = $"Attachments\\{CompanyName}\\BankChequeTemplate\\";
                var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.Template.FileName);
                var finalPath = Common.SaveFileIFF(virtualPath, dto.Template, FileName, fileExtension, _host);

                var newTemplate = new BankChequeTemplate()
                {
                    BankName = dto.BankName,
                    ChequeTemnplatePath = finalPath,
                    CreatedBy = creator,
                    CreationDate = DateTime.Now,
                    Active = true,
                    ModifiedBy = creator,
                    ModifiedDate = DateTime.Now
                };
                _unitOfWork.BankChequeTemplates.Add(newTemplate);
                _unitOfWork.Complete();

                Response.ID = newTemplate.Id;

                return Response;
            }
            catch( Exception ex )
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }

        }


        public BaseResponseWithId<int> EditBankChequeTemplate(BankChequeTemplatedto dto, long creator, string CompanyName)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (dto == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "you sent Invalid Data";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (dto.Id == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Id Is required";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (dto.BankName == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "BankName Is required";
                    Response.Errors.Add(error);
                    return Response;
                }

                var bank = _unitOfWork.BankChequeTemplates.FindAll(a => a.BankName.Trim().ToLower() == dto.BankName.Trim().ToLower() && a.Id!=dto.Id).FirstOrDefault();
                if (bank != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Bank Name Already Exist!";
                    Response.Errors.Add(error);
                    return Response;
                }

                var editedBank = _unitOfWork.BankChequeTemplates.GetById((int)dto.Id);
                if (editedBank == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err105";
                    error.ErrorMSG = "Bank Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (dto.Template != null)
                {
                    var oldPath = Path.Combine(_host.WebRootPath, editedBank.ChequeTemnplatePath);
                    if (File.Exists(oldPath))
                    {
                        File.Delete(oldPath);
                    }

                    var fileExtension = dto.Template.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompanyName}\\BankChequeTemplate\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.Template.FileName);
                    var finalPath = Common.SaveFileIFF(virtualPath, dto.Template, FileName, fileExtension, _host);

                    editedBank.ChequeTemnplatePath = finalPath;
                }
                editedBank.BankName = dto.BankName;
                editedBank.ModifiedBy = creator;
                editedBank.ModifiedDate = DateTime.Now;

                _unitOfWork.BankChequeTemplates.Update(editedBank);
                _unitOfWork.Complete();

                Response.ID = editedBank.Id;

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


        public BaseResponseWithData<GetChequeTemplatesReponse> GetChequeTemplateList()
        {
            BaseResponseWithData<GetChequeTemplatesReponse> Response = new BaseResponseWithData<GetChequeTemplatesReponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {             
                var cheques = _unitOfWork.BankChequeTemplates.FindAll(a => a.Active).ToList();

                var list = cheques.Select(a=> {
                    var resolution = Common.GetImageResolution(Globals.baseURL +"/"+ a.ChequeTemnplatePath);
                    return
                    new ChequeTemplatesList()
                    {
                        Id = a.Id,
                        BankName = a.BankName,
                        TemplatePath = Globals.baseURL + a.ChequeTemnplatePath,
                        ImgHeight = resolution.Height,
                        ImgWidth = resolution.Width
                    };
                }).ToList();

                Response.Data = new GetChequeTemplatesReponse();
                Response.Data.TemplatesLists = list;
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
