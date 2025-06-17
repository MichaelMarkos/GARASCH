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
using Microsoft.EntityFrameworkCore;
using System.Net;
using NewGaras.Infrastructure.Models.InventoryStoreReports;
using NewGaras.Infrastructure.Models.InventoryStoreReports.UsedInResponse;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using DocumentFormat.OpenXml.Bibliography;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryStoreReportsService : IInventoryStoreReportsService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";

        public InventoryStoreReportsService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;

        }

        public async Task<GetInventoryStoreReportResponse> GetInventoryStoreReportList()
        {
            GetInventoryStoreReportResponse Response = new GetInventoryStoreReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DataList = new List<InventoryStoreReport>();
                if (Response.Result)
                {
                    var InventoryReportListDB = await _unitOfWork.InventoryReports.FindAllAsync(a => true, includes: new[] { "InventoryStore" });
                    if (InventoryReportListDB.Count() > 0)
                    {
                        foreach (var item in InventoryReportListDB)
                        {
                            var InventoryStoreReportDB = new InventoryStoreReport();
                            InventoryStoreReportDB.ID = item.Id;
                            InventoryStoreReportDB.StoreID = item.InventoryStoreId;
                            InventoryStoreReportDB.StoreName = item.InventoryStore != null ? item.InventoryStore.Name : "";
                            InventoryStoreReportDB.ReportSubject = item.ReportSubject;
                            InventoryStoreReportDB.ReportDesc = item.Description;
                            InventoryStoreReportDB.DateFrom = item.DateFrom.ToShortDateString();
                            InventoryStoreReportDB.DateTo = item.DateTo.ToShortDateString();
                            InventoryStoreReportDB.Status = item.Closed == true ? "Closed" : "Open";
                            InventoryStoreReportDB.Approval = item.Approved == true ? "Approved" : "Not Approved";



                            // Check if Report All Finished To send to Inv Store Item 
                            //if (await Common.CheckReportIsFinishedToSendInvStoreItem(item.ID))
                            //{
                            //}
                            InventoryStoreReportDB.IsFinished = await CheckReportIsFinishedToSendInvStoreItem(item.Id);
                            DataList.Add(InventoryStoreReportDB);
                        }
                    }

                }
                Response.DataLList = DataList;
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

        public async Task<bool> CheckReportIsFinishedToSendInvStoreItem(long ReportID)
        {
            bool IsFinished = false;
            if (ReportID != 0)
            {
                var IDSReportItemList = (await _unitOfWork.InventoryReportItems.FindAllAsync(x => x.InventoryReportId == ReportID)).Select(x => x.Id);
                if (IDSReportItemList.Count() > 0)
                {
                    var ReportItemParentList = _unitOfWork.InventoryReportItemParents.FindAll(x => IDSReportItemList.Contains(x.InvReportItemId));
                    var CountOfReportItemsParent =  ReportItemParentList.Count();
                    var CountOfReportItemParentIsFinished =  ReportItemParentList.Where(x => x.Finished == true).Count();
                    if (CountOfReportItemParentIsFinished == CountOfReportItemsParent)
                    {
                        IsFinished = true;
                    }
                }
            }
            return IsFinished;
        }

        public async Task<GetInventoryStoreReportItemResponse> GetInventoryStoreReportItemList(long ReportID)
        {
            GetInventoryStoreReportItemResponse Response = new GetInventoryStoreReportItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                
                if (ReportID == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err99";
                    error.ErrorMSG = "Invalid Report ID";
                    Response.Errors.Add(error);
                    return Response;
                }

                var DataList = new List<InventoryStoreReportItem>();
                if (Response.Result)
                {
                    var ReportDB = await _unitOfWork.InventoryReports.FindAsync(x => x.Id == ReportID);
                    if (ReportDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Report ID is not exist";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var ReportCreationDate = ReportDB.CreatedDate.ToString("dd MMM yyyy");

                    string OpentationType = "Inventory Report " + ReportDB.Id.ToString() + " " + ReportCreationDate;
                    var InventoryReportItemsListDB = await _unitOfWork.InventoryReportItems.FindAllAsync(x => x.InventoryReportId == ReportID, includes: new[] { "InventoryItem", "InventoryReportItemParents" });
                    var IDsInventoryStoreItemListDB = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => x.OrderId == ReportID && x.OperationType == OpentationType);
                    if (InventoryReportItemsListDB.Count() > 0)
                    {
                        foreach (var item in InventoryReportItemsListDB)
                        {
                            var ReportItemDB = new InventoryStoreReportItem();
                            ReportItemDB.ID = item.Id;
                            ReportItemDB.IsFinished = item.InventoryReportItemParents.Where(x => x.InvReportItemId == item.Id).FirstOrDefault()?.Finished;
                            ReportItemDB.InventoryItemID = item.InventoryItemId;
                            ReportItemDB.ItemName = item.InventoryItem != null ? item.InventoryItem.Name : "";
                            ReportItemDB.CurrentBalance = item.CurrentBalance;
                            ReportItemDB.PhysicalBalance = item.PhysicalBalance;
                            ReportItemDB.Comment = item.Comment;
                            ReportItemDB.UnitCost = IDsInventoryStoreItemListDB.Where(x => x.AddingOrderItemId == item.Id).Select(x => x.PoinvoiceTotalCostEgp).FirstOrDefault() ?? 0;
                            DataList.Add(ReportItemDB);
                        }
                    }

                }
                Response.DataLList = DataList;
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
        //Attachment change from Body to form
        public async Task<BaseResponseWithID> AddInventoryStoreReport(AddInventoryStoreReportRequest Request, long UserID, string CompName)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region Validations 
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    int StoreID = 0;
                    if (Request.StoreId != null)
                    {
                        StoreID = (int)Request.StoreId;
                        var StoreDB = await _unitOfWork.InventoryStores.FindAsync(x => x.Id == StoreID);
                        if (StoreDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "Invalid Store Id.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Store Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    DateTime DateFrom = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.DateFrom) || !DateTime.TryParse(Request.DateFrom, out DateFrom))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Date From.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime DateTo = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.DateTo) || !DateTime.TryParse(Request.DateTo, out DateTo))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Date To.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.ReportSubject == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "Invalid Report Subject.";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    #endregion
                    if (Response.Result)
                    {
                        long? InventoryReportID = Request.ID;
                        if (Request.ID == null || Request.ID == 0)
                        {
                            var InventoryReportDB = new InventoryReport();
                            InventoryReportDB.InventoryStoreId = StoreID;
                            InventoryReportDB.ReportSubject = Request.ReportSubject;
                            InventoryReportDB.Description = Request.ReportDesc;
                            InventoryReportDB.DateFrom = DateFrom;
                            InventoryReportDB.DateTo = DateTo;
                            InventoryReportDB.ByUserId = UserID;
                            InventoryReportDB.Active = true;
                            InventoryReportDB.Closed = false;
                            InventoryReportDB.Approved = false;
                            InventoryReportDB.CreatedDate = DateTime.Now;
                            InventoryReportDB.ModifiedDate = DateTime.Now;

                            _unitOfWork.InventoryReports.Add(InventoryReportDB);
                            var Res =  _unitOfWork.Complete();
                            if (Res > 0)
                            {
                                InventoryReportID = InventoryReportDB.Id;
                            }
                        }
                        else  // Update 
                        {
                            var CheckReportDB = await _unitOfWork.InventoryReports.FindAsync(x => x.Id == Request.ID);
                            if (CheckReportDB != null)
                            {
                                CheckReportDB.ReportSubject = Request.ReportSubject;
                                CheckReportDB.Description = Request.ReportDesc;
                                CheckReportDB.DateFrom = DateFrom;
                                CheckReportDB.DateTo = DateTo;
                                CheckReportDB.Active = true;
                                CheckReportDB.Closed = Request.Closed != null ? (bool)Request.Closed : CheckReportDB.Closed;
                                CheckReportDB.Approved = Request.Approved != null ? (bool)Request.Approved : CheckReportDB.Approved;
                                CheckReportDB.ModifiedDate = DateTime.Now;
                                _unitOfWork.Complete();

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err324";
                                error.ErrorMSG = "Invalid Report Id.";
                                Response.Errors.Add(error);
                                return Response;
                            }

                        }






                        if (InventoryReportID != null && InventoryReportID != 0)
                        {
                            Response.ID = (long)InventoryReportID;

                            if (Request.AttachmentList != null)
                            {
                                var AttachIds = Request.AttachmentList.Select(a => a.Id).ToList();
                                var attachData = _unitOfWork.InventoryReportAttachments.FindAll(a => AttachIds.Contains(a.Id));

                                foreach (var attachment in Request.AttachmentList)
                                {

                                    if (attachment.Id != null && attachment.Active == false)
                                    {
                                        // Delete
                                        var AttachmentDb = attachData.Where(x => x.Id == attachment.Id).FirstOrDefault();
                                        if (AttachmentDb != null)
                                        {
                                            var AttachmentPath = Path.Combine(_host.WebRootPath, AttachmentDb.AttachmentPath);//HttpContext.Current.Server.MapPath(AttachmentDb.AttachmentPath);

                                            if (File.Exists(AttachmentPath))
                                            {
                                                File.Delete(AttachmentPath);
                                            }
                                            _unitOfWork.InventoryReportAttachments.Delete(AttachmentDb);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                    else
                                    {
                                        if (attachment.FileContent == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err324";
                                            error.ErrorMSG = "file content is mendatory";
                                            Response.Errors.Add(error);
                                            return Response;
                                        }

                                        var fileExtension = attachment.FileContent.FileName.Split('.').Last();
                                        var virtualPath = $"Attachments\\{CompName}\\InventoryReports\\{InventoryReportID}\\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.FileContent.FileName.Trim().Replace(" ", ""));
                                        var AttachPath = Common.SaveFileIFF(virtualPath, attachment.FileContent, FileName, fileExtension, _host);



                                        var CompanyName = CompName;
                                        //var FilePath =  Common.SaveFileIFF("/Attachments/" + CompanyName + "/InventoryReports/" + InventoryReportID + "/", attachment.FileContent, attachment.FileName, attachment.FileExtension, _host);
                                        var AttachmentDb = new InventoryReportAttachment();
                                        AttachmentDb.InventoryReportId = (long)InventoryReportID;
                                        AttachmentDb.FileExtenssion = fileExtension;
                                        AttachmentDb.FileName = FileName;
                                        AttachmentDb.AttachmentPath = AttachPath;
                                        AttachmentDb.CreatedBy = UserID;
                                        AttachmentDb.CreationDate = DateTime.Now;
                                        AttachmentDb.Type = fileExtension;


                                        _unitOfWork.InventoryReportAttachments.Add(AttachmentDb);
                                        _unitOfWork.Complete();

                                    }

                                }
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponse> UpdateInventoryStoreReportItem(UpdateInventoryStoreReportItemRequest Request, long UserID)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    #region Validations 
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long ReportID = 0;
                    InventoryReport ReportDB = null;
                    if (Request.ReportID != null)
                    {
                        ReportID = (long)Request.ReportID;
                        ReportDB = await _unitOfWork.InventoryReports.FindAsync(x => x.Id == ReportID);
                        if (ReportDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "Invalid Report Id.";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        if (ReportDB.Approved != true)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "This Report is not Approved yet.";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        if (ReportDB.Closed == true)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "This Report is Closed.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Report Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (Request.ItemPhysicalBalanceList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.ItemPhysicalBalanceList.Count <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    // validate is Count distinct < count of Items => there is iteration items with same data
                    var ItemDistinctCount = Request.ItemPhysicalBalanceList.Select(x => x.InventoryItemId).Distinct().Count();
                    var IDSStoreItemDistinctCount = Request.ItemPhysicalBalanceList.SelectMany(x => x.IDSInventoryStoreItemList).Distinct().Count();
                    if (ItemDistinctCount < Request.ItemPhysicalBalanceList.Count() && IDSStoreItemDistinctCount < Request.ItemPhysicalBalanceList.SelectMany(x => x.IDSInventoryStoreItemList).Count())
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "There is item itteration";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    // Check on Report Item is not exist in same report
                    var IDSInventoryItems = Request.ItemPhysicalBalanceList.Select(x => x.InventoryItemId).ToList();
                    var CheckReportItemListFound = _unitOfWork.InventoryReportItems.Find(x => x.InventoryReportId == Request.ReportID && IDSInventoryItems.Contains(x.InventoryItemId));
                    if (CheckReportItemListFound != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "There is item already exist on this Report";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var IDSInvStoreItems = Request.ItemPhysicalBalanceList.SelectMany(x => x.IDSInventoryStoreItemList).ToList();
                    var InvStoreItemsDBList = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => IDSInvStoreItems.Contains(x.Id));
                    int Counter = 0;
                    foreach (var item in Request.ItemPhysicalBalanceList)
                    {
                        Counter++;
                        if (item.InventoryItemId < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err47";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        else // Check is Inventoryt Item ID is valid
                        {
                            var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(item.InventoryItemId);
                            if (InventoryItemObjDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }


                        //if (item.PhysicalBalance <= 0)
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err49";
                        //    error.ErrorMSG = "Invalid Physical Quantity selected for item #" + Counter;
                        //    Response.Errors.Add(error);
                        //}


                        if (item.IDSInventoryStoreItemList == null || item.IDSInventoryStoreItemList.Count() == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-44";
                            error.ErrorMSG = "IDSInventoryStoreItemList is required ,because Request type is hold for item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        //else
                        //{
                        //    var CheckInventoryStoreItemListDB = InvStoreItemsDBList.Where(x => item.IDSInventoryStoreItemList.Contains(x.ID)).ToList();
                        //    if (CheckInventoryStoreItemListDB.Count() > 0)
                        //    {
                        //        // Check Req Qty <= Qty on store item to make hold
                        //        if (item.PhysicalBalance > (CheckInventoryStoreItemListDB.Sum(x => (x.holdQty ?? 0))))
                        //        {
                        //            Response.Result = false;
                        //            Error error = new Error();
                        //            error.ErrorCode = "Err-44";
                        //            error.ErrorMSG = "ReqQTY is greater than available Hold Qty for item #" + Counter;
                        //            Response.Errors.Add(error);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        Response.Result = false;
                        //        Error error = new Error();
                        //        error.ErrorCode = "Err-44";
                        //        error.ErrorMSG = "Invalid InventoryStoreItemIDs List for item #" + Counter;
                        //        Response.Errors.Add(error);
                        //    }
                        //}
                    }



                    #endregion
                    if (Response.Result)
                    {
                        // Insert on Report Item List 
                        if (Request.ItemPhysicalBalanceList != null)
                        {
                            foreach (var item in Request.ItemPhysicalBalanceList)
                            {
                                var InvReportItem = new InventoryReportItem();
                                InvReportItem.InventoryItemId = item.InventoryItemId;
                                InvReportItem.InventoryReportId = ReportID;
                                InvReportItem.CurrentBalance = item.CurrentBalance ?? 0;
                                InvReportItem.PhysicalBalance = item.PhysicalBalance ?? 0;
                                InvReportItem.Comment = item.Comment;
                                InvReportItem.CreatedDate = DateTime.Now;
                                InvReportItem.ModifiedDate = DateTime.Now;
                                InvReportItem.CreatedBy = UserID;
                                InvReportItem.ModifiedBy = UserID;

                                _unitOfWork.InventoryReportItems.Add(InvReportItem);
                                var ResReportItem =  _unitOfWork.Complete();

                                // Insertion on inventory report Item Parent 
                                if (ResReportItem > 0)
                                {
                                    if (item.IDSInventoryStoreItemList != null)
                                    {
                                        foreach (var InvStoreItemID in item.IDSInventoryStoreItemList)
                                        {
                                            var InvReportItemParentObjDB = new InventoryReportItemParent();
                                            InvReportItemParentObjDB.InvReportItemId = InvReportItem.Id;
                                            InvReportItemParentObjDB.InvStoreItemId = InvStoreItemID;
                                            InvReportItemParentObjDB.Finished = false;
                                            InvReportItemParentObjDB.CreatedBy = UserID;
                                            InvReportItemParentObjDB.ModifiedBy = UserID;
                                            InvReportItemParentObjDB.CreationDate = DateTime.Now;
                                            InvReportItemParentObjDB.ModifiedDate = DateTime.Now;

                                            _unitOfWork.InventoryReportItemParents.Add(InvReportItemParentObjDB);
                                            _unitOfWork.Complete();

                                        }
                                    }
                                }
                                #region comment
                                //if (ResReportItem > 0)
                                //{
                                //    decimal RemainQTY = (decimal)item.PhysicalBalance; //50

                                //    if (RemainQTY < 0 && item.IDSInventoryStoreItemList.Count() > 0)
                                //    {
                                //        RemainQTY = Math.Abs(RemainQTY);
                                //        foreach (var InvStoreItemId in item.IDSInventoryStoreItemList)  // 20 -  10   - 5
                                //        {
                                //            var InventoryStoreItemObjDB = InvStoreItemsDBList.Where(x => x.ID == InvStoreItemId).FirstOrDefault();
                                //            if (InventoryStoreItemObjDB != null)
                                //            {
                                //                decimal NewRmeainQTY = 0;
                                //                decimal AvailableQTY = InventoryStoreItemObjDB.finalBalance ?? 0; //10
                                //                if (RemainQTY <= AvailableQTY)
                                //                {
                                //                    NewRmeainQTY = RemainQTY;
                                //                }
                                //                else
                                //                {

                                //                    NewRmeainQTY = AvailableQTY;
                                //                }

                                //                if (RemainQTY > 0)
                                //                {
                                //                    RemainQTY -= NewRmeainQTY;








                                //                    long? POID = null;
                                //                    long? POInvoiceId = null;
                                //                    decimal? POInvoiceTotalPrice = null;
                                //                    decimal? POInvoiceTotalCost = null;
                                //                    int? currencyId = null;
                                //                    decimal? rateToEGP = null;
                                //                    decimal? POInvoiceTotalPriceEGP = null;
                                //                    decimal? POInvoiceTotalCostEGP = null;
                                //                    decimal? remainItemPrice = null;
                                //                    decimal? remainItemCosetEGP = null;
                                //                    decimal? remainItemCostOtherCU = null;
                                //                        decimal? finalBalance = InventoryStoreItemObjDB.finalBalance - NewRmeainQTY; 
                                //                    if (InventoryStoreItemObjDB != null)
                                //                    {
                                //                        POID = InventoryStoreItemObjDB.addingFromPOId;
                                //                        POInvoiceId = InventoryStoreItemObjDB.POInvoiceId;
                                //                        POInvoiceTotalPrice = InventoryStoreItemObjDB.POInvoiceTotalPrice;
                                //                        POInvoiceTotalCost = InventoryStoreItemObjDB.POInvoiceTotalCost;
                                //                        currencyId = InventoryStoreItemObjDB.currencyId;
                                //                        rateToEGP = InventoryStoreItemObjDB.rateToEGP;
                                //                        POInvoiceTotalPriceEGP = InventoryStoreItemObjDB.POInvoiceTotalPriceEGP;
                                //                        POInvoiceTotalCostEGP = InventoryStoreItemObjDB.POInvoiceTotalCostEGP;
                                //                        remainItemPrice = InventoryStoreItemObjDB.remainItemPrice;
                                //                        remainItemCosetEGP = InventoryStoreItemObjDB.remainItemCosetEGP;
                                //                        remainItemCostOtherCU = InventoryStoreItemObjDB.remainItemCostOtherCU;



                                //                        // Update PO Item Columns
                                //                        // Check if Not call PO Item On Parent 
                                //                        if (InventoryStoreItemObjDB.addingFromPOId != null)
                                //                        {


                                //                            var POItemObjDB = _Context.V_PurchasePoItem.Where(x => x.PurchasePOID == InventoryStoreItemObjDB.addingFromPOId
                                //                                                                               && x.InventoryItemID == InventoryStoreItemObjDB.InventoryItemID
                                //                                                                                ).FirstOrDefault();
                                //                            if (POItemObjDB != null)
                                //                            {
                                //                                POInvoiceId = _Context.proc_PurchasePOInvoiceLoadAll().Where(x => x.POID == InventoryStoreItemObjDB.addingFromPOId).Select(x => x.POID).FirstOrDefault();
                                //                                currencyId = POItemObjDB.CurrencyID ?? 0;
                                //                                rateToEGP = POItemObjDB.RateToEGP ?? 0;
                                //                                POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                //                                POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                //                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                //                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                //                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                //                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;
                                //                            }
                                //                        }
                                //                    }



                                //                    InventoryStoreItemObjDB.finalBalance = finalBalance; // InventoryStoreItemObjDB.finalBalance - NewRmeainQTY;
                                //                    InventoryStoreItemObjDB.ModifiedBy = validation.userID;
                                //                    InventoryStoreItemObjDB.ModifiedDate = DateTime.Now;

                                //                    InventoryStoreItemObjDB.POInvoiceId = POInvoiceId;
                                //                    InventoryStoreItemObjDB.currencyId = currencyId;
                                //                    InventoryStoreItemObjDB.rateToEGP = rateToEGP;
                                //                    InventoryStoreItemObjDB.POInvoiceTotalPriceEGP = POInvoiceTotalPriceEGP;
                                //                    InventoryStoreItemObjDB.POInvoiceTotalCostEGP = POInvoiceTotalCostEGP;
                                //                    InventoryStoreItemObjDB.POInvoiceTotalPrice = POInvoiceTotalPrice;
                                //                    InventoryStoreItemObjDB.POInvoiceTotalCost = POInvoiceTotalCost;
                                //                    InventoryStoreItemObjDB.remainItemCosetEGP = remainItemCosetEGP;
                                //                    InventoryStoreItemObjDB.remainItemCostOtherCU = remainItemCostOtherCU;

                                //                    _Context.SaveChanges();
                                //                }
                                //            }
                                //        }


                                //    }

                                //        // Insert on inventorystoreitem

                                //        long FirstParentInvStoreItemID = item.IDSInventoryStoreItemList.FirstOrDefault();
                                //        if (FirstParentInvStoreItemID != 0)
                                //        {
                                //            var ParentInventoryStoreItem = InvStoreItemsDBList.Where(x => x.ID == FirstParentInvStoreItemID).FirstOrDefault();
                                //            long? POID = null;
                                //            long? POInvoiceId = null;
                                //            decimal? POInvoiceTotalPrice = null;
                                //            decimal? POInvoiceTotalCost = null;
                                //            int? currencyId = null;
                                //            decimal? rateToEGP = null;
                                //            decimal? POInvoiceTotalPriceEGP = null;
                                //            decimal? POInvoiceTotalCostEGP = null;
                                //            decimal? remainItemPrice = null;
                                //            decimal? remainItemCosetEGP = null;
                                //            decimal? remainItemCostOtherCU = null;
                                //            if (ParentInventoryStoreItem != null)
                                //            {
                                //                POID = ParentInventoryStoreItem.addingFromPOId;
                                //                POInvoiceId = ParentInventoryStoreItem.POInvoiceId;
                                //                POInvoiceTotalPrice = ParentInventoryStoreItem.POInvoiceTotalPrice;
                                //                POInvoiceTotalCost = ParentInventoryStoreItem.POInvoiceTotalCost;
                                //                currencyId = ParentInventoryStoreItem.currencyId;
                                //                rateToEGP = ParentInventoryStoreItem.rateToEGP;
                                //                POInvoiceTotalPriceEGP = ParentInventoryStoreItem.POInvoiceTotalPriceEGP;
                                //                POInvoiceTotalCostEGP = ParentInventoryStoreItem.POInvoiceTotalCostEGP;
                                //                remainItemPrice = ParentInventoryStoreItem.remainItemPrice;
                                //                remainItemCosetEGP = ParentInventoryStoreItem.remainItemCosetEGP;
                                //                remainItemCostOtherCU = ParentInventoryStoreItem.remainItemCostOtherCU;



                                //                // Update PO Item Columns
                                //                // Check if Not call PO Item On Parent 
                                //                decimal? finalBalance =Math.Abs(item.PhysicalBalance ?? 0);
                                //                if (ParentInventoryStoreItem.addingFromPOId != null)
                                //                {


                                //                    var POItemObjDB = _Context.V_PurchasePoItem.Where(x => x.PurchasePOID == ParentInventoryStoreItem.addingFromPOId
                                //                                                                       && x.InventoryItemID == ParentInventoryStoreItem.InventoryItemID
                                //                                                                        ).FirstOrDefault();
                                //                    if (POItemObjDB != null)
                                //                    {
                                //                        POInvoiceId = _Context.proc_PurchasePOInvoiceLoadAll().Where(x => x.POID == ParentInventoryStoreItem.addingFromPOId).Select(x => x.POID).FirstOrDefault();
                                //                        currencyId = POItemObjDB.CurrencyID ?? 0;
                                //                        rateToEGP = POItemObjDB.RateToEGP ?? 0;
                                //                        POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                //                        POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                //                        POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                //                        POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                //                        remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                //                        remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                //                    }
                                //                }
                                //            }
                                //            ObjectParameter IDStoreItem = new ObjectParameter("ID", typeof(long));
                                //            var InsertionNewInventoryStoreItem = _Context.Myproc_InventoryStoreItemInsert_API(IDStoreItem,
                                //                                                       ParentInventoryStoreItem.InventoryStoreID,
                                //                                                       ParentInventoryStoreItem.InventoryItemID,
                                //                                                       ReportDB.ID.ToString(),
                                //                                                       ReportDB.ID,
                                //                                                       DateTime.Now,
                                //                                                       validation.userID,
                                //                                                       DateTime.Now,
                                //                                                       validation.userID,
                                //                                                       "Inventory Report "+ ReportDB.ID.ToString() +" "+ ReportDB.CreatedDate.ToShortDateString(),
                                //                                                      (item.PhysicalBalance),
                                //                                                       ParentInventoryStoreItem.InvenoryStoreLocationID,
                                //                                                       ParentInventoryStoreItem.expDate,
                                //                                                       ParentInventoryStoreItem.itemSerial,
                                //                                                       ParentInventoryStoreItem.ID,
                                //                                                       (item.PhysicalBalance),
                                //                                                       InvReportItem.ID,
                                //                                                       POID,
                                //                                                       // Extra Data PO Item
                                //                                                       POInvoiceId,
                                //                                                        POInvoiceTotalPrice,
                                //                                                        POInvoiceTotalCost,
                                //                                                        currencyId,
                                //                                                        rateToEGP,
                                //                                                        POInvoiceTotalPriceEGP,
                                //                                                        POInvoiceTotalCostEGP,
                                //                                                        remainItemPrice,
                                //                                                        remainItemCosetEGP,
                                //                                                        remainItemCostOtherCU,
                                //                                                        0,
                                //                                                        null
                                //                                                       );


                                //        }

                                //}

                                #endregion
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponse> UpdateReportToInvStoreItem(UpdateInventoryStoreReportItemRequest Request, long UserID)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region Validations 
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long ReportID = 0;
                    InventoryReport ReportDB = null;
                    if (Request.ReportID != null)
                    {
                        ReportID = (long)Request.ReportID;
                        ReportDB = await _unitOfWork.InventoryReports.FindAsync(x => x.Id == ReportID);
                        if (ReportDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "Invalid Report Id.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Report Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    #endregion
                    if (Response.Result)
                    {
                        var ReportItemListDB = await _unitOfWork.InventoryReportItems.FindAllAsync(x => x.InventoryReportId == ReportID);
                        var IDsReportItem = ReportItemListDB.Select(x => x.Id).ToList();
                        var InventoryReportItemParentListAll = await _unitOfWork.InventoryReportItemParents.FindAllAsync(x => IDsReportItem.Contains(x.InvReportItemId));
                        var InventoryReportItemParentList = InventoryReportItemParentListAll.Where(x => x.Finished != true).ToList();
                        if (InventoryReportItemParentList.Count() == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "This Report Item Already send to inventory store item and  Finished .";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (ReportItemListDB.Count() > 0)
                        {
                            foreach (var item in ReportItemListDB)
                            {
                                var InventoryReportItemParentsList = InventoryReportItemParentList.Where(x => x.InvReportItemId == item.Id && x.Finished != true).ToList();
                                var IDSInventoryStoreItemList = InventoryReportItemParentsList.Select(x => x.InvStoreItemId).ToList();
                                var InvStoreItemsDBList = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => IDSInventoryStoreItemList.Contains(x.Id));
                                if (IDSInventoryStoreItemList.Count() > 0)
                                {

                                    decimal RemainQTY = (decimal)item.PhysicalBalance; //50

                                    if (RemainQTY < 0 && IDSInventoryStoreItemList.Count() > 0)
                                    {
                                        RemainQTY = Math.Abs(RemainQTY);
                                        foreach (var InvStoreItemId in IDSInventoryStoreItemList)  // 20 -  10   - 5
                                        {
                                            var InventoryStoreItemObjDB = InvStoreItemsDBList.Where(x => x.Id == InvStoreItemId).FirstOrDefault();
                                            if (InventoryStoreItemObjDB != null)
                                            {
                                                decimal NewRmeainQTY = 0;
                                                decimal AvailableQTY = InventoryStoreItemObjDB.FinalBalance ?? 0; //10
                                                if (RemainQTY <= AvailableQTY)
                                                {
                                                    NewRmeainQTY = RemainQTY;
                                                }
                                                else
                                                {

                                                    NewRmeainQTY = AvailableQTY; // will set remain balance (final balance )
                                                }

                                                if (RemainQTY > 0)
                                                {
                                                    RemainQTY -= NewRmeainQTY; // for second round


                                                    long? POID = null;
                                                    long? POInvoiceId = null;
                                                    decimal? POInvoiceTotalPrice = null;
                                                    decimal? POInvoiceTotalCost = null;
                                                    int? currencyId = null;
                                                    decimal? rateToEGP = null;
                                                    decimal? POInvoiceTotalPriceEGP = null;
                                                    decimal? POInvoiceTotalCostEGP = null;
                                                    decimal? remainItemPrice = null;
                                                    decimal? remainItemCosetEGP = null;
                                                    decimal? remainItemCostOtherCU = null;
                                                    decimal? finalBalance = InventoryStoreItemObjDB.FinalBalance - NewRmeainQTY;
                                                    if (InventoryStoreItemObjDB != null)
                                                    {
                                                        POID = InventoryStoreItemObjDB.AddingFromPoid;
                                                        POInvoiceId = InventoryStoreItemObjDB.PoinvoiceId;
                                                        POInvoiceTotalPrice = InventoryStoreItemObjDB.PoinvoiceTotalPrice;
                                                        POInvoiceTotalCost = InventoryStoreItemObjDB.PoinvoiceTotalCost;
                                                        currencyId = InventoryStoreItemObjDB.CurrencyId;
                                                        rateToEGP = InventoryStoreItemObjDB.RateToEgp;
                                                        POInvoiceTotalPriceEGP = InventoryStoreItemObjDB.PoinvoiceTotalPriceEgp;
                                                        POInvoiceTotalCostEGP = InventoryStoreItemObjDB.PoinvoiceTotalCostEgp;
                                                        remainItemPrice = InventoryStoreItemObjDB.RemainItemPrice;
                                                        remainItemCosetEGP = InventoryStoreItemObjDB.RemainItemCosetEgp;
                                                        remainItemCostOtherCU = InventoryStoreItemObjDB.RemainItemCostOtherCu;



                                                        // Update PO Item Columns
                                                        // Check if Not call PO Item On Parent 
                                                        if (InventoryStoreItemObjDB.AddingFromPoid != null)
                                                        {


                                                            var POItemObjDB = _unitOfWork.VPurchasePoItems.Find(x => x.PurchasePoid == InventoryStoreItemObjDB.AddingFromPoid
                                                                                                               && x.InventoryItemId == InventoryStoreItemObjDB.InventoryItemId
                                                                                                                );
                                                            if (POItemObjDB != null)
                                                            {
                                                                POInvoiceId = _unitOfWork.PurchasePOInvoices.Find(x => x.Poid == InventoryStoreItemObjDB.AddingFromPoid).Id;
                                                                currencyId = POItemObjDB.CurrencyId ?? 0;
                                                                rateToEGP = POItemObjDB.RateToEgp ?? 0;
                                                                POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                                POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;
                                                            }
                                                        }
                                                    }



                                                    InventoryStoreItemObjDB.FinalBalance = finalBalance; // InventoryStoreItemObjDB.finalBalance - NewRmeainQTY;
                                                    InventoryStoreItemObjDB.ModifiedBy = UserID;
                                                    InventoryStoreItemObjDB.ModifiedDate = DateTime.Now;

                                                    InventoryStoreItemObjDB.PoinvoiceId = POInvoiceId;
                                                    InventoryStoreItemObjDB.CurrencyId = currencyId;
                                                    InventoryStoreItemObjDB.RateToEgp = rateToEGP;
                                                    InventoryStoreItemObjDB.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                    InventoryStoreItemObjDB.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                    InventoryStoreItemObjDB.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                    InventoryStoreItemObjDB.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                    InventoryStoreItemObjDB.RemainItemCosetEgp = remainItemCosetEGP;
                                                    InventoryStoreItemObjDB.RemainItemCostOtherCu = remainItemCostOtherCU;

                                                    _unitOfWork.Complete();
                                                }
                                            }
                                        }


                                    }

                                    // Insert on inventorystoreitem

                                    long FirstParentInvStoreItemID = IDSInventoryStoreItemList.FirstOrDefault();
                                    if (FirstParentInvStoreItemID != 0)
                                    {
                                        var ParentInventoryStoreItem = InvStoreItemsDBList.Where(x => x.Id == FirstParentInvStoreItemID).FirstOrDefault();
                                        long? POID = null;
                                        long? POInvoiceId = null;
                                        decimal? POInvoiceTotalPrice = null;
                                        decimal? POInvoiceTotalCost = null;
                                        int? currencyId = null;
                                        decimal? rateToEGP = null;
                                        decimal? POInvoiceTotalPriceEGP = null;
                                        decimal? POInvoiceTotalCostEGP = null;
                                        decimal? remainItemPrice = null;
                                        decimal? remainItemCosetEGP = null;
                                        decimal? remainItemCostOtherCU = null;
                                        if (ParentInventoryStoreItem != null)
                                        {
                                            POID = ParentInventoryStoreItem.AddingFromPoid;
                                            POInvoiceId = ParentInventoryStoreItem.PoinvoiceId;
                                            POInvoiceTotalPrice = ParentInventoryStoreItem.PoinvoiceTotalPrice;
                                            POInvoiceTotalCost = ParentInventoryStoreItem.PoinvoiceTotalCost;
                                            currencyId = ParentInventoryStoreItem.CurrencyId;
                                            rateToEGP = ParentInventoryStoreItem.RateToEgp;
                                            POInvoiceTotalPriceEGP = ParentInventoryStoreItem.PoinvoiceTotalPriceEgp;
                                            POInvoiceTotalCostEGP = ParentInventoryStoreItem.PoinvoiceTotalCostEgp;
                                            remainItemPrice = ParentInventoryStoreItem.RemainItemPrice;
                                            remainItemCosetEGP = ParentInventoryStoreItem.RemainItemCosetEgp;
                                            remainItemCostOtherCU = ParentInventoryStoreItem.RemainItemCostOtherCu;



                                            // Update PO Item Columns
                                            // Check if Not call PO Item On Parent 
                                            decimal? finalBalance = Math.Abs(item.PhysicalBalance);
                                            if (ParentInventoryStoreItem.AddingFromPoid != null)
                                            {


                                                var POItemObjDB = _unitOfWork.VPurchasePoItems.Find(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
                                                                                                   && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
                                                                                                    );
                                                if (POItemObjDB != null)
                                                {
                                                    POInvoiceId = _unitOfWork.PurchasePOInvoices.Find(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid).Id;
                                                    currencyId = POItemObjDB.CurrencyId ?? 1;
                                                    rateToEGP = POItemObjDB.RateToEgp ?? 1;
                                                    POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                    POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                    POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                    POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                    remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                    remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                }
                                            }
                                        }
                                        var ReportCreationDate = ReportDB.CreatedDate.ToString("dd MMM yyyy");



                                        var InventoryStoreItemOBJ = new InventoryStoreItem();
                                        InventoryStoreItemOBJ.InventoryStoreId = ParentInventoryStoreItem.InventoryStoreId;
                                        InventoryStoreItemOBJ.InventoryItemId = ParentInventoryStoreItem.InventoryItemId;
                                        InventoryStoreItemOBJ.OrderNumber = ReportDB.Id.ToString();
                                        InventoryStoreItemOBJ.OrderId = ReportDB.Id;
                                        InventoryStoreItemOBJ.CreatedBy = UserID;
                                        InventoryStoreItemOBJ.ModifiedBy = UserID;
                                        InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                        InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                        InventoryStoreItemOBJ.OperationType = "Inventory Report " + ReportDB.Id.ToString() + " " + ReportCreationDate;
                                        InventoryStoreItemOBJ.Balance = (double?)(item.PhysicalBalance);
                                        InventoryStoreItemOBJ.InvenoryStoreLocationId = ParentInventoryStoreItem.InvenoryStoreLocationId;
                                        InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                        InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                        InventoryStoreItemOBJ.ReleaseParentId = ParentInventoryStoreItem.Id;
                                        InventoryStoreItemOBJ.FinalBalance = (item.PhysicalBalance);
                                        InventoryStoreItemOBJ.AddingOrderItemId = item.Id;
                                        InventoryStoreItemOBJ.AddingFromPoid = POID;
                                        InventoryStoreItemOBJ.PoinvoiceId = POInvoiceId;
                                        InventoryStoreItemOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                        InventoryStoreItemOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
                                        InventoryStoreItemOBJ.CurrencyId = currencyId;
                                        InventoryStoreItemOBJ.RateToEgp = rateToEGP;
                                        InventoryStoreItemOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                        InventoryStoreItemOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                        InventoryStoreItemOBJ.RemainItemPrice = remainItemPrice;
                                        InventoryStoreItemOBJ.RemainItemCosetEgp = remainItemCosetEGP;
                                        InventoryStoreItemOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
                                        InventoryStoreItemOBJ.HoldQty = 0;
                                        InventoryStoreItemOBJ.HoldReason = null;
                                        _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                        var InventoryStorItemInsertion = _unitOfWork.Complete();
                                        //ObjectParameter IDStoreItem = new ObjectParameter("ID", typeof(long));
                                        //var InsertionNewInventoryStoreItem = _Context.Myproc_InventoryStoreItemInsert_API(IDStoreItem,
                                        //                                           ParentInventoryStoreItem.InventoryStoreID,
                                        //                                           ParentInventoryStoreItem.InventoryItemID,
                                        //                                           ReportDB.ID.ToString(),
                                        //                                           ReportDB.ID,
                                        //                                           DateTime.Now,
                                        //                                           validation.userID,
                                        //                                           DateTime.Now,
                                        //                                           validation.userID,
                                        //                                           "Inventory Report " + ReportDB.ID.ToString() + " " + ReportCreationDate,
                                        //                                          (item.PhysicalBalance),
                                        //                                           ParentInventoryStoreItem.InvenoryStoreLocationID,
                                        //                                           ParentInventoryStoreItem.expDate,
                                        //                                           ParentInventoryStoreItem.itemSerial,
                                        //                                           ParentInventoryStoreItem.ID,
                                        //                                           (item.PhysicalBalance),
                                        //                                           item.ID,
                                        //                                           POID,
                                        //                                           // Extra Data PO Item
                                        //                                           POInvoiceId,
                                        //                                            POInvoiceTotalPrice,
                                        //                                            POInvoiceTotalCost,
                                        //                                            currencyId,
                                        //                                            rateToEGP,
                                        //                                            POInvoiceTotalPriceEGP,
                                        //                                            POInvoiceTotalCostEGP,
                                        //                                            remainItemPrice,
                                        //                                            remainItemCosetEGP,
                                        //                                            remainItemCostOtherCU,
                                        //                                            0,
                                        //                                            null
                                        //                                           );


                                    }




                                    // Update InventoryReportItem Parent
                                    foreach (var itemReportItemParent in InventoryReportItemParentsList)
                                    {
                                        itemReportItemParent.Finished = true;
                                        itemReportItemParent.ModifiedBy = UserID;
                                        itemReportItemParent.ModifiedDate = DateTime.Now;

                                        _unitOfWork.Complete();
                                    }

                                    var CountOfAllItem = InventoryReportItemParentListAll.Count();
                                    var CountOItemFinished =  _unitOfWork.InventoryReportItemParents.FindAll(x => IDsReportItem.Contains(x.InvReportItemId) && x.Finished == true).Count();
                                    if (CountOfAllItem <= CountOItemFinished) // closed Report 
                                    {
                                        ReportDB.Closed = true;
                                        _unitOfWork.Complete();
                                    }

                                }
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponse> UpdateInventoryStoreReportOneItem(UpdateInventoryStoreReportOneItemRequest Request, long UserID)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region Validations 
                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long ReportItemId = 0;
                    InventoryReportItem ReportItemDB = null;
                    if (Request.ReportItemId != null)
                    {
                        ReportItemId = (long)Request.ReportItemId;
                        ReportItemDB = await _unitOfWork.InventoryReportItems.FindAsync(x => x.Id == ReportItemId);
                        if (ReportItemDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err324";
                            error.ErrorMSG = "Invalid Report Item Id.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Report Item Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.PhysicalBalance == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Physical Balance";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.PhysicalBalance == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Physical Balance";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    #endregion
                    if (Response.Result)
                    {

                        // Check if Report is Closed Or Not Approved  or not Active
                        var ReportDB = await _unitOfWork.InventoryReports.FindAsync(x => x.Id == ReportItemDB.InventoryReportId);
                        if (ReportDB != null)
                        {
                            if (ReportDB.Closed == true)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err324";
                                error.ErrorMSG = "Can't Update Report Item because this report is Closed";
                                Response.Errors.Add(error);
                                return Response;
                            }

                            if (ReportDB.Approved == false)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err324";
                                error.ErrorMSG = "Can't Update Report Item because this report is not approved yet";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            ReportItemDB.PhysicalBalance = (decimal)Request.PhysicalBalance;
                            ReportItemDB.ModifiedBy = UserID;
                            ReportItemDB.ModifiedDate = DateTime.Now;

                            Response.Result = false;
                            var Res = _unitOfWork.Complete();
                            if (Res > 0)
                            {
                                Response.Result = true;

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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

    }
}
