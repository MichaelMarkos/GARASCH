using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.ProjectInvoiceCollected;
using NewGaras.Infrastructure.DTO.TaskMangerProject;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectInvoice;
using NewGarasAPI.Models.Project.UsedInResponses;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;

namespace NewGaras.Domain.Services
{
    public class ProjectInvoiceService : IProjectInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        public ProjectInvoiceService(IUnitOfWork unitOfWork,IMapper mapper, IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
        }

        public BaseResponseWithId<long> AddNewProjectInvoice([FromForm] AddProjectInvoiceModel invoiceModel, long creator, string CompName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            long invoiceid = 0;
            decimal total = 0;
            ProjectInvoice invoicePlaceHolder = null;
            if (invoiceModel.InvoiceId == null)
            {
                if (invoiceModel.ProjectId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project Id Is required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var project = _unitOfWork.Projects.GetById(invoiceModel.ProjectId);
                if (project == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "project not found";
                    Response.Errors.Add(error);
                    return Response;
                }

                var invoice = new ProjectInvoice()
                {
                    ProjectId = project.Id,
                    InvoiceDate = DateTime.Now,
                    Active = true,
                    InvoiceSerial = "0",
                    CreationDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    CreatedBy = creator,
                    ModifiedBy = creator
                };
                if (invoiceModel.Attachment != null)
                {
                    var fileExtension = invoiceModel.Attachment.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompName}\\Invoices\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(invoiceModel.Attachment.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, invoiceModel.Attachment, FileName, fileExtension, _host);
                    invoice.AttachmentPath = AttachPath;
                }
                invoicePlaceHolder = invoice;
                var addedInvoice = _unitOfWork.ProjectInvoices.Add(invoice);
                _unitOfWork.Complete();
                Response.ID = addedInvoice.Id;
                invoiceid = addedInvoice.Id;
            }
            else
            {
                var invoice = _unitOfWork.ProjectInvoices.GetById((long)invoiceModel.InvoiceId);
                if (invoice == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Invoice not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                invoiceid = invoice.Id;
                invoicePlaceHolder = invoice;
                Response.ID = invoice.Id;
                total = invoice.Amount;
            }
            for(int i = 0; i<invoiceModel.WorkingHoursIds.Count; i++)
            {
                var workinghours = _unitOfWork.WorkingHoursTrackings.FindAll(a=>a.Id == invoiceModel.WorkingHoursIds[i], includes: new[] {"HrUser.JobTitle"}).FirstOrDefault();
                if(workinghours == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = $"Working hours tracking of the item number {i+1} is not Found";
                    Response.Errors.Add(error);
                    //return Response;
                }
                var item = new ProjectInvoiceItem()
                {
                    ProjectInvoiceId = invoiceid,
                    ItemId = invoiceModel.WorkingHoursIds[i],
                    Type = "WTP",
                    Name = "WTP",
                    UserName = workinghours.HrUser.FirstName + " " + workinghours.HrUser.LastName,
                    JobtitleName = workinghours.HrUser.JobTitle?.Name,
                    Quantity = workinghours.TotalHours,
                    Rate = workinghours.TaskRate,
                    Total = workinghours.TotalHours * workinghours.TaskRate,
                    CreatedBy = creator,
                    ModifedBy = creator,
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now,
                    Uomid = 62
                };
                total += item.Total;
                _unitOfWork.ProjectInvoiceItems.Add(item);
                _unitOfWork.Complete();
            }
            for (int ii = 0; ii < invoiceModel.TaskExpensisIds.Count; ii++)
            {
                var taskExpensis = _unitOfWork.TaskExpensis.FindAll(a => a.Id == invoiceModel.TaskExpensisIds[ii], includes: new[] { "CreatedByNavigation.JobTitle", "ExpensisType" }).FirstOrDefault();
                if (taskExpensis == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = $"Task Expensis of the item number {ii + 1} is not Found";
                    Response.Errors.Add(error);
                    //return Response;
                }
                var item = new ProjectInvoiceItem()
                {
                    ProjectInvoiceId = invoiceid,
                    ItemId= invoiceModel.TaskExpensisIds[ii],
                    Type = "TaskExpenses",
                    Name = taskExpensis.ExpensisType?.ExpensisTypeName,
                    UserName = taskExpensis.CreatedByNavigation.FirstName + " " + taskExpensis.CreatedByNavigation.LastName,
                    JobtitleName = taskExpensis.CreatedByNavigation.JobTitle?.Name,
                    Quantity = taskExpensis.Amount,
                    Rate = 1,
                    Total = taskExpensis.Amount,
                    CreatedBy = creator,
                    ModifedBy = creator,
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now,
                    Uomid=111
                };
                total += item.Total;
                _unitOfWork.ProjectInvoiceItems.Add(item);
                _unitOfWork.Complete();
            }
            for (int iii = 0; iii < invoiceModel.UnitServiceIds.Count; iii++)
            {
                var unit = _unitOfWork.TaskUnitRateServices.FindAll(a => a.Id == invoiceModel.UnitServiceIds[iii], includes: new[] { "CreatedByNavigation.JobTitle"}).FirstOrDefault();
                if (unit == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = $"Task Unit Rate Services of the item number {iii + 1} is not Found";
                    Response.Errors.Add(error);
                    //return Response;
                }
                var item = new ProjectInvoiceItem()
                {
                    ProjectInvoiceId = invoiceid,
                    ItemId = invoiceModel.UnitServiceIds[iii],
                    Type = "TaskUnitRateServices",
                    Name = unit.ServiceName,
                    UserName = unit.CreatedByNavigation.FirstName + " " + unit.CreatedByNavigation.LastName,
                    JobtitleName = unit.CreatedByNavigation.JobTitle?.Name,
                    Quantity = unit.Quantity,
                    Rate = unit.Rate,
                    Total = unit.Total,
                    CreatedBy = creator,
                    ModifedBy = creator,
                    CreationDate = DateTime.Now,
                    ModificationDate = DateTime.Now,
                    Uomid = unit.Uomid,
                };
                total += item.Total;
                _unitOfWork.ProjectInvoiceItems.Add(item);
                _unitOfWork.Complete();
            }
            invoicePlaceHolder.InvoiceSerial = invoicePlaceHolder.Id.ToString();
            invoicePlaceHolder.Amount = total;
            _unitOfWork.ProjectInvoices.Update(invoicePlaceHolder);
            _unitOfWork.Complete();

            return Response;
        }

        public BaseResponseWithId<long> UpdateProjectInvoiceItems([FromBody] UpdateProjectInvoiceItemsModel invoiceItemList,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            if(invoiceItemList.invoiceItemList==null || invoiceItemList.invoiceItemList.Count == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invoice Items List Is required";
                Response.Errors.Add(error);
                return Response;
            }
            long invoiceId = 0;
            for(int i = 0; i< invoiceItemList.invoiceItemList.Count; i++)
            {
                var item = _unitOfWork.ProjectInvoiceItems.GetById(invoiceItemList.invoiceItemList[i].ProjectInvoiceItemId);
                invoiceId = item.ProjectInvoiceId;
                if(item== null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = $"Invoice Item {i+1} is not found";
                    Response.Errors.Add(error);
                }
                else
                {
                    var uom = _unitOfWork.InventoryUoms.GetById(invoiceItemList.invoiceItemList[i].UOMID);
                    if (uom == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = $"UOM At {i + 1} is not found";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        item.Rate = invoiceItemList.invoiceItemList[i].Rate;
                        item.Quantity = invoiceItemList.invoiceItemList[i].Quantity;
                        item.Uomid = invoiceItemList.invoiceItemList[i].UOMID;
                        item.Total = item.Rate * item.Quantity;
                        item.ModificationDate = DateTime.Now;
                        item.ModifedBy = creator;
                        _unitOfWork.ProjectInvoiceItems.Update(item);
                        _unitOfWork.Complete();
                    }
                }
            }
            if (invoiceId != 0)
            {
                var invoice = _unitOfWork.ProjectInvoices.GetById(invoiceId);
                if(invoice == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Invoice Is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var items = _unitOfWork.ProjectInvoiceItems.FindAll(a=>a.ProjectInvoiceId==invoice.Id);
                invoice.Amount = items.Select(a=>a.Total).Sum();
                invoice.ModifiedBy = creator;
                invoice.ModifiedDate = DateTime.Now;
                _unitOfWork.ProjectInvoices.Update(invoice);
                _unitOfWork.Complete();
                Response.ID = invoice.Id;

            }
            return Response;
        }

        public BaseResponseWithData<List<GetProjectInvoiceModel>> GetProjectInvoices([FromHeader] long? ProjectId)
        {
            BaseResponseWithData<List<GetProjectInvoiceModel>> Response = new BaseResponseWithData<List<GetProjectInvoiceModel>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var invoices = _unitOfWork.ProjectInvoices.GetAsQueryable();
            if (ProjectId != null)
            {
                var project = _unitOfWork.Projects.GetById((long)ProjectId);
                if (project == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "project is not found";
                    Response.Errors.Add(error);
                    return Response;
                }

                invoices = invoices.Where(a=>a.ProjectId == ProjectId);
            }
            var invoiceslist = invoices.Include(x=>x.Project).ThenInclude(x=>x.SalesOffer).ToList();
            var invoicesmodel = _mapper.Map<List<GetProjectInvoiceModel>>(invoiceslist);
            Response.Data = invoicesmodel;
            return Response;
        }

        public BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel> GetProjectInvoiceItems([FromHeader] long InvoiceId, [FromHeader] int page=1, [FromHeader] int size=10)
        {
            BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel> Response = new BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data =  new GetProjectInvoiceItemsDataModel();
            if (InvoiceId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Project id is required";
                Response.Errors.Add(error);
                return Response;
            }

            var Invoice = _unitOfWork.ProjectInvoices.Find(a => a.Id == InvoiceId, includes: new[] { "Project.SalesOffer","Project.Currency" });
            if (Invoice == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invoice is not found";
                Response.Errors.Add(error);
                return Response;
            }

            var items = _unitOfWork.ProjectInvoiceItems.FindAllPaging(a => a.ProjectInvoiceId == Invoice.Id, CurrentPage: page, NumberOfItemsPerPage: size, includes: new[] {"Uom"});
            var itemsmodel = _mapper.Map<List<GetProjectInvoiceItemsModel>>(items.ToList());
            Response.Data.Invoice = _mapper.Map<GetProjectInvoiceModel>(Invoice);
            Response.Data.Items = itemsmodel;
            Response.PaginationHeader = new PaginationHeader()
            {
                CurrentPage = items.CurrentPage,
                TotalPages = items.TotalPages,
                ItemsPerPage = items.PageSize,
                TotalItems = items.TotalCount
            };
            return Response;
        }

        public BaseResponseWithId<long> AddProjectInvoiceCollected(AddProjectInvoiceCollectedDto Dto, long creator,string CompName)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region Validation
            var projectInvoice = _unitOfWork.ProjectInvoices.GetById(Dto.ProjectInvoiceId);
            if(projectInvoice == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No project Invoice with this Id";
                response.Errors.Add(error);
                return response;
            }
            if(Dto.PaymentMethodId != null)
            {
                var paymentMehod = _unitOfWork.PaymentMethods.GetById(Dto.PaymentMethodId??0);
                if (paymentMehod == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No payment Mehod with this Id";
                    response.Errors.Add(error);
                    return response;
                }
            }
            #endregion

            #region Date validation
            var Date = DateTime.Now;
            if (!string.IsNullOrEmpty(Dto.Date))
            {
                if (!DateTime.TryParse(Dto.Date, out Date))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date:";
                    response.Errors.Add(err);
                    return response;
                }
            }
            
            #endregion

            try
            {
                var newprojectInvoiceCollected = new ProjectInvoiceCollected();
                newprojectInvoiceCollected.ProjectInvoiceId = Dto.ProjectInvoiceId;
                newprojectInvoiceCollected.Amount = Dto.Amount;
                newprojectInvoiceCollected.Status = Dto.Status;
                newprojectInvoiceCollected.Comment = Dto.Comment;
                newprojectInvoiceCollected.Date = Date;
                if (Dto.PaymentMethodId != null) newprojectInvoiceCollected.PaymentMethodId = Dto.PaymentMethodId;
                newprojectInvoiceCollected.Active = true;
                newprojectInvoiceCollected.CreationDate = DateTime.Now;
                newprojectInvoiceCollected.CreatedBy = creator;
                newprojectInvoiceCollected.ModifiedBy = creator;
                newprojectInvoiceCollected.ModifiedDate = DateTime.Now;
                _unitOfWork.ProjectInvoicesCollected.Add(newprojectInvoiceCollected);
                _unitOfWork.Complete();

                if (newprojectInvoiceCollected.Status.Trim().ToLower() == "collected")
                {
                    /* var items = _unitOfWork.ProjectInvoiceItems.FindAll(a => a.ProjectInvoiceId == projectInvoice.Id).ToList();
                     projectInvoice.Amount = items.Sum(a => a.Total);*/
                    var collected = _unitOfWork.ProjectInvoicesCollected.FindAll(a => a.Status.Trim().ToLower() == "collected" && a.ProjectInvoiceId == projectInvoice.Id).ToList();
                    if (collected.Count > 0)
                    {
                        projectInvoice.Collected = collected.Sum(a => a.Amount);
                    }
                    projectInvoice.ModifiedBy = creator;
                    projectInvoice.ModifiedDate = DateTime.Now;
                    _unitOfWork.ProjectInvoices.Update(projectInvoice);
                    _unitOfWork.Complete();
                }

                //--------------------------Attatchment-----------------------------
                if(Dto.Attachment != null && Dto.Attachment.Length >0)
                {
                    var fileExtension = Dto.Attachment.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompName}\\ProjectInvoiceCollected\\{newprojectInvoiceCollected.Id}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(Dto.Attachment.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, Dto.Attachment, FileName, fileExtension, _host);

                    newprojectInvoiceCollected.AttachmentPath = AttachPath;
                }
                _unitOfWork.Complete();

                response.ID = newprojectInvoiceCollected.Id;
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

        public BaseResponseWithId<long> AddInvoiceItem([FromBody] AddProjectInvoiceItemModel InvoiceItem,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            if (InvoiceItem.ProjectInvoiceId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invoice Id Is required";
                Response.Errors.Add(error);
                return Response;
            }

            var invoice = _unitOfWork.ProjectInvoices.GetById(InvoiceItem.ProjectInvoiceId);
            if (invoice == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invoice not found";
                Response.Errors.Add(error);
                return Response;
            }
            if (InvoiceItem.HrUserId == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Hr User Id Is required";
                Response.Errors.Add(error);
                return Response;
            }
            var User = _unitOfWork.HrUsers.Find(a => a.Id == InvoiceItem.HrUserId, includes: new[] { "JobTitle" });
            if (User == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Hr User not found";
                Response.Errors.Add(error);
                return Response;
            }
            var unit = _unitOfWork.InventoryUoms.GetById(InvoiceItem.UOMID);
            if(unit == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "UOM Is not found";
                Response.Errors.Add(error);
                return Response;
            }
            var item = new ProjectInvoiceItem()
            {
                ProjectInvoiceId = InvoiceItem.ProjectInvoiceId,
                Type = InvoiceItem.Type,
                Name = InvoiceItem.Name,
                UserName = User.FirstName + " " + User.LastName,
                JobtitleName = User.JobTitle?.Name,
                Quantity = InvoiceItem.Quantity,
                Rate = InvoiceItem.Rate,
                CreatedBy = creator,
                ModifedBy = creator,
                CreationDate = DateTime.Now,
                ModificationDate = DateTime.Now,
                Uomid = unit.Id,
            };
            if (item.Type == "discount")
            {
                item.Rate = item.Rate * -1;
            }
            item.Total=item.Quantity*item.Rate;
            var addedItem = _unitOfWork.ProjectInvoiceItems.Add(item);
            _unitOfWork.Complete();
            var items = _unitOfWork.ProjectInvoiceItems.FindAll(a => a.ProjectInvoiceId == invoice.Id);
            invoice.Amount = items.Select(a => a.Total).Sum();
            invoice.ModifiedBy = creator;
            invoice.ModifiedDate = DateTime.Now;
            _unitOfWork.ProjectInvoices.Update(invoice);
            _unitOfWork.Complete();

            Response.ID = addedItem.Id;
            return Response;
        }

        public BaseResponseWithData<List<GetProjectInvoiceCollectedDto>> GetProjectInvoiceCollectedList(long projectInvoiceID)
        {
            var response = new BaseResponseWithData<List<GetProjectInvoiceCollectedDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region check In DB
            var projectInvo = _unitOfWork.ProjectInvoices.GetById(projectInvoiceID);
            if(projectInvo == null )
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "This Task project Invoice Id is not found ";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var projectInvoiceCollectedList = _unitOfWork.ProjectInvoicesCollected.FindAll(a=>a.ProjectInvoiceId ==  projectInvoiceID, new[] { "PaymentMethod" });
                var projectInvoiceCollectedData = _mapper.Map<List<GetProjectInvoiceCollectedDto>>(projectInvoiceCollectedList);

                response.Data = projectInvoiceCollectedData;
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

        public BaseResponseWithId<long> EditProjectInvoiceCollected(EditProjectInvoiceCollectedDto Dto, long creator, string CompName)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var projectInvoiceCollected =_unitOfWork.ProjectInvoicesCollected.GetById(Dto.ID);
            if (projectInvoiceCollected == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No project Invoice Collected with this Id";
                response.Errors.Add(error);
                return response;
            }
            var projectInvoice = _unitOfWork.ProjectInvoices.GetById(projectInvoiceCollected.ProjectInvoiceId);
            if (projectInvoice == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No project Invoice with this Id";
                response.Errors.Add(error);
                return response;
            }
            #endregion
            #region Date Validation
            var Date = DateTime.Now;
            if (!string.IsNullOrEmpty(Dto.Date))
            {
                if (!DateTime.TryParse(Dto.Date, out Date))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date:";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion

            try
            {
                projectInvoiceCollected.Amount = Dto.Amount;
                projectInvoiceCollected.Status = Dto.Status;
                projectInvoiceCollected.Comment = Dto.Comment;
                _unitOfWork.Complete();
                if (projectInvoiceCollected.Status.Trim().ToLower() == "collected")
                {
/*                    var items = _unitOfWork.ProjectInvoiceItems.FindAll(a => a.ProjectInvoiceId == projectInvoice.Id).ToList();
                    projectInvoice.Amount = items.Sum(a => a.Total);*/
                    var collected = _unitOfWork.ProjectInvoicesCollected.FindAll(a=>a.Status.Trim().ToLower() == "collected" && a.ProjectInvoiceId==projectInvoice.Id).ToList();
                    if (collected.Count > 0)
                    {
                        projectInvoice.Collected = collected.Sum(a => a.Amount);
                    }
                    projectInvoice.ModifiedBy = creator;
                    projectInvoice.ModifiedDate = DateTime.Now;
                    _unitOfWork.ProjectInvoices.Update(projectInvoice);
                    _unitOfWork.Complete();
                }
                if (!string.IsNullOrEmpty(Dto.Date))projectInvoiceCollected.Date = Date;
                if(!string.IsNullOrEmpty(Dto.Date))projectInvoiceCollected.Date = Date;
                var paymentmethod = _unitOfWork.PaymentMethods.GetById(Dto.PaymentMethodID);
                if (paymentmethod == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Payment Method is not found";
                    response.Errors.Add(error);
                    return response;
                }
                projectInvoiceCollected.PaymentMethodId = Dto.PaymentMethodID;

                if(Dto.Attachment != null)
                {
                    if(projectInvoiceCollected.AttachmentPath != null)
                    {
                        string path = Path.Combine(_host.WebRootPath, projectInvoiceCollected.AttachmentPath);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }

                    var fileExtension = Dto.Attachment.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompName}\\ProjectInvoiceCollected\\{projectInvoiceCollected.Id}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(Dto.Attachment.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, Dto.Attachment, FileName, fileExtension, _host);

                    projectInvoiceCollected.AttachmentPath = AttachPath;
                }
                _unitOfWork.Complete();

                response.ID = projectInvoiceCollected.Id;
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

        public BaseResponseWithData<GetProjectFinancialDataModel> GetProjectFinancialData([FromHeader]long ProjectId)
        {
            BaseResponseWithData<GetProjectFinancialDataModel> response = new BaseResponseWithData<GetProjectFinancialDataModel>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.Data = new GetProjectFinancialDataModel();
            var Project = _unitOfWork.Projects.Find(a => a.Id == ProjectId, includes: new[] { "Tasks.TaskDetails", "WorkingHourseTrackings", "Tasks.TaskExpensis", "ProjectInvoices.ProjectInvoiceItems", "Tasks.TaskUnitRateServices" });
            if(Project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Project not found";
                response.Errors.Add(error);
                return response;
            }
            var unitservices = Project.Tasks.SelectMany(a=>a.TaskUnitRateServices).ToList();
            var items = Project.ProjectInvoices.SelectMany(a=>a.ProjectInvoiceItems).ToList();
            response.Data.ProjectBudget = Project.Budget??0;
            response.Data.TotalTasksBudget = Project.Tasks.SelectMany(x => x.TaskDetails).Sum(z=>z.ProjectBudget??0);
            response.Data.ExpensisTotalAmount = Project.WorkingHourseTrackings.Where(a=>a.WorkingHoursApproval==true).Select(a => a.TaskRate * a.TotalHours).Sum() + Project.Tasks.SelectMany(a=>a.TaskExpensis).Where(a=>a.Approved==true).Sum(a=>a.Amount) + unitservices.Sum(a => a.Total);
            response.Data.InvoicesAmountExtra = items.Where(a => a.ItemId == null).Sum(a => a.Total);
            response.Data.InvoicesAmountBasic = items.Where(a => a.ItemId != null).Sum(a => a.Total);
            response.Data.InvoicesAmount = Project.ProjectInvoices.Sum(a => a.Amount);
            response.Data.InvoicesCollected = Project.ProjectInvoices.Sum(a => a.Collected);
            response.Data.InvoicesRemain = response.Data.ExpensisTotalAmount - response.Data.InvoicesAmount;
            response.Data.GrossProfit = response.Data.InvoicesAmount- response.Data.ExpensisTotalAmount;
            response.Data.WorkingHours = Project.WorkingHourseTrackings.Where(a=>a.WorkingHoursApproval==true).Select(a => a.TaskRate * a.TotalHours).Sum();
            response.Data.UnitRateService = unitservices.Sum(a => a.Total);
            response.Data.DirectExpenses = Project.Tasks.SelectMany(a => a.TaskExpensis).Where(a => a.Approved == true).Sum(a => a.Amount);
            response.Data.RemainingTobeCollected = response.Data.InvoicesAmount - response.Data.InvoicesCollected;
            response.Data.RemainingTobeInvoiced = response.Data.ExpensisTotalAmount - response.Data.InvoicesAmountBasic;
            return response;

        }

        public BaseResponseWithId<long> DeleteProjectInvoiceItem(long InvoiceItemId,long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            if(InvoiceItemId == 0) 
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Invoice Item Id is required";
                response.Errors.Add(error);
                return response;
            }

            var item = _unitOfWork.ProjectInvoiceItems.GetById(InvoiceItemId);

            if(item == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Invoice Item Is Not Found";
                response.Errors.Add(error);
                return response;
            }
            var invoice = _unitOfWork.ProjectInvoices.GetById(item.ProjectInvoiceId);
            _unitOfWork.ProjectInvoiceItems.Delete(item);
            _unitOfWork.Complete();
            var items = _unitOfWork.ProjectInvoiceItems.FindAll(a => a.ProjectInvoiceId == invoice.Id);
            invoice.Amount = items.Select(a => a.Total).Sum();
            invoice.ModifiedBy = creator;
            invoice.ModifiedDate = DateTime.Now;
            _unitOfWork.ProjectInvoices.Update(invoice);
            _unitOfWork.Complete();
            return response;
        }

        public BaseResponseWithId<long> DeleteProjectInvoiceCollected(long Id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var projectInvoiceCollected = _unitOfWork.ProjectInvoicesCollected.GetById(Id);
            if(projectInvoiceCollected == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No project Invoice Collected with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                if(projectInvoiceCollected.Status == "Pending" || projectInvoiceCollected.Status.ToLower() == "pending")
                {
                    _unitOfWork.ProjectInvoicesCollected.Delete(projectInvoiceCollected);
                    _unitOfWork.Complete();
                }
                if(projectInvoiceCollected.Status == "Collected" || projectInvoiceCollected.Status.ToLower() == "collected")
                {
                    var projectInvoice = _unitOfWork.ProjectInvoices.GetById(projectInvoiceCollected.ProjectInvoiceId);
                    
                    _unitOfWork.ProjectInvoicesCollected.Delete(projectInvoiceCollected);
                    _unitOfWork.Complete();
                    var collected = _unitOfWork.ProjectInvoicesCollected.FindAll(a => a.Status.Trim().ToLower() == "collected" && a.ProjectInvoiceId == projectInvoice.Id).ToList();

                    projectInvoice.Collected = collected.Select(a=>a.Amount).DefaultIfEmpty(0).Sum();
                    _unitOfWork.ProjectInvoices.Update(projectInvoice);
                    _unitOfWork.Complete();
                }

                response.ID = projectInvoiceCollected.Id;
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

        public BaseResponseWithId<long> DeleteProjectInvoice(long InvoiceId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            if (InvoiceId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Project Invoice Id Is required";
                response.Errors.Add(error);
                return response;
            }
            var projectInvoice = _unitOfWork.ProjectInvoices.GetById(InvoiceId);
            if (projectInvoice == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "No project Invoice with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                _unitOfWork.ProjectInvoices.Delete(projectInvoice);
                _unitOfWork.Complete();

                response.ID = InvoiceId;
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


        public BaseResponseWithData<string> GetProjectInvoicesReport([FromHeader] long ProjectId, [FromHeader] decimal? Amount, [FromHeader] long? CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To,string CompanyName)
        {
            BaseResponseWithData<string> response = new BaseResponseWithData<string>()
            {
                Errors = new List<Error>(),
                Result = true
            };
            try
            {
                if (ProjectId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project Id Is Required";
                    response.Errors.Add(error);
                    return response;
                }

                var Project = _unitOfWork.Projects.FindAll(a => a.Id == ProjectId, includes: new[] { "SalesOffer" }).FirstOrDefault();
                if (Project == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Project Not Found";
                    response.Errors.Add(error);
                    return response;
                }
                var invoices = _unitOfWork.ProjectInvoices.FindAllQueryable(a => a.ProjectId == Project.Id, includes: new[] { "ProjectInvoiceItems", "ProjectInvoiceCollecteds" });

                if (Amount != null)
                {
                    invoices = invoices.Where(a=>a.Amount<=Amount).AsQueryable();
                }
                if (CreatorId!=null)
                {
                    invoices = invoices.Where(a => a.CreatedBy == CreatorId).AsQueryable();
                }
                if (From != null)
                {
                    invoices = invoices.Where(a => a.InvoiceDate >= From).AsQueryable();
                }
                if (To != null)
                {
                    invoices = invoices.Where(a => a.InvoiceDate <= To).AsQueryable();
                }

                var invoicesList = invoices.ToList();
                if (!invoices.Any())
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "This Project Doesn't have invoices";
                    response.Errors.Add(error);
                    return response;
                }

                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[8] {
                new DataColumn("Invoice Number"),
                new DataColumn("Project Name"),
                new DataColumn("# Items"),
                new DataColumn("Total Amount"),
                new DataColumn("Collection"),
                new DataColumn("Collected"),
                new DataColumn("Date"),
                new DataColumn("Creator Name"),
                });
                foreach (var item in invoicesList) 
                {
                    dt.Rows.Add(
                    item.InvoiceSerial,
                    Project.SalesOffer.ProjectName,
                    item.ProjectInvoiceItems.Count(),
                    item.Amount,
                    item.ProjectInvoiceCollecteds.Sum(a => a.Amount),
                    item.ProjectInvoiceCollecteds.Where(a => a.Status == "Collected").Sum(a => a.Amount),
                    item.InvoiceDate.ToShortDateString(),
                    item.CreatedByNavigation!=null?item.CreatedByNavigation.FirstName + " " + item.CreatedByNavigation.LastName:"N/A"
                    );
                }

                var workSheet = package.Workbook.Worksheets.Add("ProjectInvoice");
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(255,174,81));
                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                for (int i = 1; i <= excelRangeBase.Columns; i++)
                {
                    workSheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= excelRangeBase.Rows; i++)
                {
                    workSheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                workSheet.View.FreezePanes(2, 1);
                var newpath = $"Attachments\\{CompanyName}\\ProjectInvoiceReports";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                {
                    File.Delete(savedPath);
                }
                Directory.CreateDirectory(savedPath);
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\ProjectInvoiceReport_{date}.xlsx";
                package.SaveAs(excelPath);
                package.Dispose();
                response.Data = Globals.baseURL + '\\' + newpath + $"\\ProjectInvoiceReport_{date}.xlsx";
                return response;
            }
            catch(Exception ex)
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
