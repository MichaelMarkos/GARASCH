using Azure;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.InventoryItemMatrialAddingAndExternalOrder;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItemMatrialAddingAndExternalOrder.Filters;
using NewGarasAPI.Models.Purchase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NewGaras.Domain.Services
{
    public class InventoryItemMatrialAddingAndExternalOrderService : IInventoryItemMatrialAddingAndExternalOrderService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private readonly ILogService _logService;

        public InventoryItemMatrialAddingAndExternalOrderService(IUnitOfWork unitOfWork, IWebHostEnvironment host, ILogService logService)
        {
            _unitOfWork = unitOfWork;
            _host = host;
            _logService = logService;
        }


        public InventoryItemSupplierMatrialAddingOrderInfoResponse GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo(long MatrialAddingOrderID, long creator, string CompName)
        {
            InventoryItemSupplierMatrialAddingOrderInfoResponse Response = new InventoryItemSupplierMatrialAddingOrderInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemSupplierMatrialAddingOrderInfoObj = new InventoryItemSupplierMatrialAddingOrderInfo();
                var MatrialAddingOrderInfList = new List<MatrialAddingOrderInfo>();
                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        var InventoryAddingOrderOBJDB = _unitOfWork.InventoryAddingOrders.FindAll(x => x.Id == MatrialAddingOrderID, includes: new[] { "Supplier", "InventoryStore", "CreatedByNavigation" }).FirstOrDefault();
                        if (InventoryAddingOrderOBJDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err109";
                            error.ErrorMSG = "No Matrial Adding Order with this ID";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (InventoryAddingOrderOBJDB != null)
                        {
                            InventoryItemSupplierMatrialAddingOrderInfoObj.InventoryAddingOrderID = InventoryAddingOrderOBJDB.Id;
                            InventoryItemSupplierMatrialAddingOrderInfoObj.OrderType = InventoryAddingOrderOBJDB.OperationType;
                            InventoryItemSupplierMatrialAddingOrderInfoObj.SupplierName = InventoryAddingOrderOBJDB.Supplier?.Name;
                            InventoryItemSupplierMatrialAddingOrderInfoObj.ToStore = InventoryAddingOrderOBJDB.InventoryStore?.Name;
                            InventoryItemSupplierMatrialAddingOrderInfoObj.Revision = InventoryAddingOrderOBJDB.Revision;
                            InventoryItemSupplierMatrialAddingOrderInfoObj.CreationDate = InventoryAddingOrderOBJDB.CreationDate.ToShortDateString();
                            InventoryItemSupplierMatrialAddingOrderInfoObj.CreationBy = InventoryAddingOrderOBJDB.CreatedByNavigation?.FirstName + " " + InventoryAddingOrderOBJDB.CreatedByNavigation?.LastName;
                            InventoryItemSupplierMatrialAddingOrderInfoObj.RecivingDate = InventoryAddingOrderOBJDB.RecivingDate.ToShortDateString();

                            var ListOfMatrialAddingOrderItemListDB = _unitOfWork.InventoryAddingOrderItems.FindAll(x => x.InventoryAddingOrderId == MatrialAddingOrderID, new[] { "InventoryAddingOrder", "InventoryAddingOrder.Supplier", "InventoryItem", "Uom" }).ToList();
                            var MatrialAddingOrderIDs = ListOfMatrialAddingOrderItemListDB.Select(a => a.InventoryItemId).ToList();

                            var inventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(a => MatrialAddingOrderIDs.Contains(a.OrderId));
                            if (ListOfMatrialAddingOrderItemListDB != null)
                            {
                                foreach (var item in ListOfMatrialAddingOrderItemListDB)
                                {
                                    var POInvoicetotalCost = inventoryStoreItemList.Where(a => a.OrderId == item.Id).FirstOrDefault()?.PoinvoiceTotalCostEgp;

                                    var MatrialAddingOrderInfoObj = new MatrialAddingOrderInfo();
                                    MatrialAddingOrderInfoObj.Id = item.Id;
                                    MatrialAddingOrderInfoObj.InventoryItemID = item.InventoryItemId;
                                    MatrialAddingOrderInfoObj.ItemName = item.InventoryItem?.Name;
                                    MatrialAddingOrderInfoObj.ItemCode = item.InventoryItem?.Code;
                                    MatrialAddingOrderInfoObj.InventoryItemID = item.InventoryItemId;
                                    MatrialAddingOrderInfoObj.RequireQTY = item.ReqQuantity1 != null ? item.ReqQuantity1.ToString() : "0";
                                    MatrialAddingOrderInfoObj.ReceivedOrReturnQTY = item.RecivedQuantity1 != null ? item.RecivedQuantity1.ToString() : "0";
                                    MatrialAddingOrderInfoObj.ReceivedQTYUOP = item.RecivedQuantityUop != null ? item.RecivedQuantityUop.ToString() : "0";
                                    MatrialAddingOrderInfoObj.ReceivedQTYAfter = item.RecivedQuantityAfter != null ? item.RecivedQuantityAfter.ToString() : "0";
                                    MatrialAddingOrderInfoObj.RemainQTY = item.RemainQuantity != null ? item.RemainQuantity.ToString() : "0";
                                    MatrialAddingOrderInfoObj.UOM = item.Uom?.Name;
                                    MatrialAddingOrderInfoObj.PurchaseUOM = item.InventoryItem?.PurchasingUom?.Name;
                                    MatrialAddingOrderInfoObj.PONo = item.Poid != null ? (long)item.Poid : 0;
                                    MatrialAddingOrderInfoObj.POItemComment = _unitOfWork.PurchasePOItems.FindAll(x => x.PurchasePoid == MatrialAddingOrderInfoObj.PONo && x.InventoryItemId == item.InventoryItemId).Select(x => x.Comments).FirstOrDefault();
                                    MatrialAddingOrderInfoObj.SupplierItemSerial = item.ItemSerial;
                                    MatrialAddingOrderInfoObj.Comment = item.Comments;
                                    MatrialAddingOrderInfoObj.QIReport = item.QcreportId;
                                    MatrialAddingOrderInfoObj.InventoryItemSerial = _unitOfWork.VInventoryStoreItemPrices.Find(a => a.InventoryItemId == item.InventoryItemId).Code;//Common.GeyInventoryStoreItemSerial(item.InventoryItemId);
                                    MatrialAddingOrderInfoObj.ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "";
                                    MatrialAddingOrderInfoObj.Cost = POInvoicetotalCost;
                                    MatrialAddingOrderInfoObj.SupplierID = item.InventoryAddingOrder?.SupplierId;
                                    MatrialAddingOrderInfoObj.SupplierName = item.InventoryAddingOrder?.Supplier?.Name;

                                    MatrialAddingOrderInfList.Add(MatrialAddingOrderInfoObj);
                                }
                            }

                        }

                        InventoryItemSupplierMatrialAddingOrderInfoObj.MatrialAddingOrderInfList = MatrialAddingOrderInfList;

                    }
                    var inventoryItemSupplierMatrialAddingOrderInfo = new InventoryItemSupplierMatrialAddingOrderInfoResponse();
                    inventoryItemSupplierMatrialAddingOrderInfo.InventoryItemInfo = InventoryItemSupplierMatrialAddingOrderInfoObj;

                    Response = inventoryItemSupplierMatrialAddingOrderInfo;
                    Response.Result = true;


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

                var innerExceptionMessage = (!string.IsNullOrEmpty(ex.InnerException.Message.ToString())) == true ? ex.InnerException.Message.ToString() : null;
                var message = ex.Message;

                _logService.AddLogError("GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo", message, creator, CompName, innerExceptionMessage);


                return Response;
            }
        }

        public InventoryItemMatrialAddingOrder GetInventoryItemMatrialAddingAndExternalOrderList(GetInventoryItemMatrialAddingAndExternalOrderListFilters filters, long creator, string CompName)
        {
            InventoryItemMatrialAddingOrder Response = new InventoryItemMatrialAddingOrder();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var inventoryMtrialAddingOrderByDateList = new List<InventoryMtrialAddingOrderByDate>();
                if (Response.Result)
                {
                    #region old headers
                    // filters List InternalBackOrder
                    //long InventoryItemID = 0;
                    //if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    //{
                    //    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    //}

                    //long InventoryStoreID = 0;
                    //if (!string.IsNullOrEmpty(headers["ToInventoryStoreID"]) && long.TryParse(headers["ToInventoryStoreID"], out InventoryStoreID))
                    //{
                    //    long.TryParse(headers["ToInventoryStoreID"], out InventoryStoreID);
                    //}


                    //long FromSupplierID = 0;
                    //if (!string.IsNullOrEmpty(headers["FromSupplierID"]) && long.TryParse(headers["FromSupplierID"], out FromSupplierID))
                    //{
                    //    long.TryParse(headers["FromSupplierID"], out FromSupplierID);
                    //}



                    //long CreatorUserID = 0;
                    //if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                    //{
                    //    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                    //}
                    //string SupplierItemSerial = null;
                    //if (!string.IsNullOrEmpty(headers["SupplierItemSerial"]))
                    //{
                    //    SupplierItemSerial = headers["SupplierItemSerial"];
                    //}
                    #endregion
                    string OrderType = null;
                    if (!string.IsNullOrEmpty(filters.OrderType))
                    {
                        OrderType = filters.OrderType;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err70";
                        error.ErrorMSG = "Invalid Order Type";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    //DateTime? ReceiveDate = null;
                    //DateTime ReceiveDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(headers["ReceiveDate"]) && DateTime.TryParse(headers["ReceiveDate"], out ReceiveDateTemp))
                    //{
                    //    ReceiveDateTemp = DateTime.Parse(headers["ReceiveDate"]);
                    //    ReceiveDate = ReceiveDateTemp;
                    //}


                    DateTime? ReceiveDateFrom = null;
                    DateTime ReceiveDateFromTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(filters.ReceiveDateFrom) && DateTime.TryParse(filters.ReceiveDateFrom, out ReceiveDateFromTemp))
                    {
                        ReceiveDateFromTemp = DateTime.Parse(filters.ReceiveDateFrom);
                        ReceiveDateFrom = ReceiveDateFromTemp;
                    }

                    DateTime? ReceiveDateTo = null;
                    DateTime ReceiveDateToTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(filters.ReceiveDateTo) && DateTime.TryParse(filters.ReceiveDateTo, out ReceiveDateToTemp))
                    {
                        ReceiveDateToTemp = DateTime.Parse(filters.ReceiveDateTo);
                        ReceiveDateTo = ReceiveDateToTemp;
                    }


                    // Grouped by DAte as Inquiry 
                    //var InventoryMatrialAddingOrder



                    var InventoryMatrialAddingOrder = _unitOfWork.InventoryAddingOrders.FindAllQueryable(a => true, new[] { "CreatedByNavigation", "Supplier", "InventoryStore" });
                    if (OrderType == "AddingOrder")
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.OperationType == "Add New Matrial").AsQueryable();
                    }
                    else if (OrderType == "ExternalBackOrder")
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.OperationType == "Add External Back Order").AsQueryable();
                    }
                    else if (OrderType == "OpeningBalance")
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.OperationType == "Opening Balance").AsQueryable();
                    }
                    if (filters.ToInventoryStoreID != 0)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.InventoryStoreId == filters.ToInventoryStoreID).AsQueryable();
                    }

                    if (filters.CreatorUserID != 0)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.FromSupplierID != 0)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.SupplierId == filters.FromSupplierID).AsQueryable();
                    }

                    //if (ReceiveDate != null)
                    //{
                    //    InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.RecivingDate == ReceiveDate).AsQueryable();
                    //}
                    //if (ReceiveDateFrom != null && ReceiveDateTo != null)
                    //{
                    //    if (ReceiveDateFrom == ReceiveDateTo)
                    //    {
                    //        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.RecivingDate == ReceiveDate).AsQueryable();
                    //    }
                    //}

                    if (ReceiveDateFrom != null)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.RecivingDate >= ReceiveDateFrom).AsQueryable();
                    }

                    if (ReceiveDateTo != null)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.RecivingDate <= ReceiveDateTo).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        var IDInventoryAddingOrder = _unitOfWork.InventoryAddingOrderItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryAddingOrderId).Distinct().ToList();

                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => IDInventoryAddingOrder.Contains(x.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SupplierItemSerial))
                    {
                        var IDInventoryAddingOrder = _unitOfWork.InventoryAddingOrderItems.FindAll(x => x.ItemSerial == filters.SupplierItemSerial).Select(x => x.InventoryAddingOrderId).Distinct().ToList();

                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => IDInventoryAddingOrder.Contains(x.Id)).AsQueryable();
                    }
                    var InventoryMatrialAddingOrderFiltered = InventoryMatrialAddingOrder.OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();

                    //var test = InventoryMatrialAddingOrder.ToList();







                    foreach (var InquiryPerMonth in InventoryMatrialAddingOrderFiltered)
                    {
                        var InventoryMatrialAddingOrderInfoList = new List<InventoryMatrialAddingOrderInfo>();

                        foreach (var Data in InquiryPerMonth)
                        {

                            InventoryMatrialAddingOrderInfoList.Add(new InventoryMatrialAddingOrderInfo
                            {
                                AddingOrderNo = Data.Id.ToString(),
                                SupplierName = Data.Supplier?.Name,
                                StoreName = Data.InventoryStore.Name,
                                RecivingDate = Data.RecivingDate.ToShortDateString(),
                                CreationDate = Data.CreationDate.ToShortDateString(),
                                CreatorName = Data?.CreatedByNavigation?.FirstName + " " + Data?.CreatedByNavigation?.LastName  //Common.GetUserName(Data.CreatedBy),

                            });
                        }
                        inventoryMtrialAddingOrderByDateList.Add(new InventoryMtrialAddingOrderByDate()
                        {
                            DateMonth = Common.GetMonthName(InquiryPerMonth.Key.month) + " " + InquiryPerMonth.Key.year.ToString(),
                            //item.Select(x=>x.CreatedDate.Month).ToString(),
                            InventoryMatrialAddingOrderInfoList = InventoryMatrialAddingOrderInfoList,
                        });
                    }

                    Response.InventoryMtrialAddingOrderByDateList = inventoryMtrialAddingOrderByDateList;

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

                var innerExceptionMessage = (!string.IsNullOrEmpty(ex.InnerException.Message.ToString())) == true ? ex.InnerException.Message.ToString() : null;
                var message = ex.Message;
                _logService.AddLogError("GetInventoryItemMatrialAddingAndExternalOrderList", message, creator, CompName, innerExceptionMessage);


                return Response;
            }
        }

        public BaseResponseWithID AddInventoryAddingAndExternalBackOrder(AddSupplierAndStoreWithMatrialAddingAndExternalBackOrderRequest Request, long UserID, string CompanyName)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    // Check if Type adding order or External Back Order
                    if (Request.OrderType != "AddingOrder" && Request.OrderType != "ExternalBackOrder")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err35";
                        error.ErrorMSG = "Invalid Order Type must be (AddingOrder) or (ExternalBackOrder).";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    long SupplierID = 0;
                    if (Request.SupplierID != 0)
                    {
                        SupplierID = Request.SupplierID;
                        var CheckSupplier = _unitOfWork.Suppliers.GetById(Request.SupplierID);
                        if (CheckSupplier == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Supplier ID.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Supplier ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    int InventoryStoreID = 0;
                    if (Request.InventoryStoreID != 0)
                    {
                        InventoryStoreID = Request.InventoryStoreID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid Inventory Store ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime RecevingData = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RecevingData) || !DateTime.TryParse(Request.RecevingData, out RecevingData))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err23";
                        error.ErrorMSG = "Invalid Receving Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.MatrialAddingOrderItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.MatrialAddingOrderItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // validate is Count distinct < count of Items => there is iteration items with same data
                    var ItemDistinctCount = Request.MatrialAddingOrderItemList.Select(x => new { x.InventoryItemID, x.POID, x.StoreLocationID, x.ExpData, x.Serial }).Distinct().Count();
                    if (ItemDistinctCount < Request.MatrialAddingOrderItemList.Count())
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "There is item itteration with same data";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int Counter = 0;
                    foreach (var item in Request.MatrialAddingOrderItemList)
                    {
                        Counter++;
                        if (item.InventoryItemID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.POID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err28";
                            error.ErrorMSG = "Invalid PO ID Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.StoreLocationID != null)
                        {
                            var storeLoc = _unitOfWork.InventoryStoreLocations.GetById(item.StoreLocationID ?? 0);
                            if (storeLoc == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err347";
                                error.ErrorMSG = "Invalid Inventory Store Location Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }
                        if (item.QTYUOR < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err29";
                            error.ErrorMSG = "Invalid QTY item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (Request.OrderType == "ExternalBackOrder")
                        {


                            // Validate Qty From Inventory Store Item
                            var CheckAddingOrder = _unitOfWork.VInventoryAddingOrderItems.Find(x => x.InventoryItemId == item.InventoryItemID && x.Poid == item.POID && x.SupplierId == SupplierID);
                            if (CheckAddingOrder != null)
                            {
                                var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => x.OrderId == CheckAddingOrder.InventoryAddingOrderId && x.InventoryItemId == item.InventoryItemID &&
                                                                                               x.OperationType == "Add New Matrial" && x.AddingFromPoid == item.POID).ToList();
                                if (InventoryStoreItemList.Count() > 0)
                                {
                                    DateTime? ExpDate = null;
                                    if (item.ExpData != null && item.ExpData != "")
                                    {
                                        ExpDate = DateTime.Parse(item.ExpData);
                                    }
                                    // Validate is found with same serial and exp date 
                                    var ParentInvStoreItemObj = InventoryStoreItemList.Where(x => x.ExpDate == ExpDate && x.ItemSerial == item.Serial).FirstOrDefault();
                                    if (ParentInvStoreItemObj != null)
                                    {
                                        if (ParentInvStoreItemObj.FinalBalance >= item.QTYUOR)
                                        {
                                            item.ParentInventoryStoreItem = ParentInvStoreItemObj.Id;

                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err30";
                                            error.ErrorMSG = "item #" + Counter + " Not have balance enough";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err30";
                                        error.ErrorMSG = "Can't Found item #" + Counter + " in with the same expire date and serial";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err30";
                                    error.ErrorMSG = "Can't Found item #" + Counter + " in adding order with the same PO";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err30";
                                error.ErrorMSG = "Can't Found item #" + Counter + " in adding order with the same PO and Supplier";
                                Response.Errors.Add(error);
                            }

                            //var LastItemAddingOrderList = _Context.V_InventoryAddingOrderItems.Where(x => x.InventoryItemID == item.InventoryItemID && x.POID == item.POID && x.SupplierID == SupplierID).ToList();
                            //  var LastItemAddingOrderObjLoad = LastItemAddingOrderList.LastOrDefault();
                            //if (LastItemAddingOrderObjLoad != null)
                            //{
                            //    if (LastItemAddingOrderObjLoad.RecivedQuantityAfter != null)
                            //    {
                            //        var SumOFQTyForItem = Request.MatrialAddingOrderItemList.Where(x => x.InventoryItemID == item.InventoryItemID && x.POID == item.POID).Select(x => x.QTYUOR).Sum();
                            //        if (SumOFQTyForItem > (decimal)LastItemAddingOrderObjLoad.RecivedQuantityAfter)
                            //        {
                            //            Response.Result = false;
                            //            Error error = new Error();
                            //            error.ErrorCode = "Err30";
                            //            error.ErrorMSG = "Invalid QTY item #" + Counter + " this Qty not permission to backing order more than " + LastItemAddingOrderObjLoad.RecivedQuantityAfter;
                            //            Response.Errors.Add(error);
                            //        }
                            //    }

                            //}
                            //else
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err30";
                            //    error.ErrorMSG = "Invalid QTY item #" + Counter + " this Qty not permission to backing order";
                            //    Response.Errors.Add(error);
                            //}




                        }
                    }
                    //if (!long.TryParse(Encrypt_Decrypt.Decrypt(logoutRequest.SessionID, key), out UserSessionID))
                    //{
                    //    response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P14";
                    //    error.ErrorMSG = "Invalid Session ID.";
                    //    response.Errors.Add(error);
                    //}
                    //if (string.IsNullOrEmpty(Request.SessionID) || Request.SessionID.Trim() == "")
                    //{

                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P13";
                    //    error.ErrorMSG = "please write your session ID";
                    //    Response.Errors.Add(error);
                    //}
                    long InventoryAddingOrderID = 0;
                    if (Response.Result)
                    {
                        // Check Inventory Report Approved and closed or not
                        var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();
                        if (CheckInventoryReportListDB.Count > 0)
                        {
                            var storesIDs = CheckInventoryReportListDB.Select(a => a.InventoryStoreId).ToList();
                            var stores = _unitOfWork.InventoryStores.FindAll(a => storesIDs.Contains(a.Id));

                            foreach (var InventoryRep in CheckInventoryReportListDB)
                            {
                                if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                {
                                    string storeName = stores.Where(a => a.Id == InventoryStoreID).FirstOrDefault().Name;
                                    string errMsg = "Store " + storeName +
                                        " is under inventory from " +
                                        InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
                                        " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-44";
                                    error.ErrorMSG = errMsg;
                                    Response.Errors.Add(error);
                                }
                            }
                        }


                        if (Response.Result)
                        {
                            string OperationType = "Add New Matrial"; //  Default Adding Order
                            if (Request.OrderType == "ExternalBackOrder")
                            {
                                OperationType = "Add External Back Order";
                            }
                            // Insertion Adding Order
                            //ObjectParameter IDInventoryAddingOrder = new ObjectParameter("ID", typeof(long));
                            //var InventoryAddingOrderInsertion = _Context.proc_InventoryAddingOrderInsert(IDInventoryAddingOrder, OperationType, DateTime.Now, UserID, 0, SupplierID,
                            //                                                                             null, 0, InventoryStoreID, RecevingData, UserID, DateTime.Now);


                            var newInventoryAddingOrder = new InventoryAddingOrder();
                            newInventoryAddingOrder.OperationType = OperationType;
                            newInventoryAddingOrder.CreationDate = DateTime.Now;
                            newInventoryAddingOrder.CreatedBy = UserID;
                            newInventoryAddingOrder.Revision = 0;
                            newInventoryAddingOrder.SupplierId = SupplierID;
                            newInventoryAddingOrder.Ponumber = null;
                            newInventoryAddingOrder.LoadBy = 0;
                            newInventoryAddingOrder.InventoryStoreId = InventoryStoreID;
                            newInventoryAddingOrder.RecivingDate = RecevingData;
                            newInventoryAddingOrder.ModifiedBy = UserID;
                            newInventoryAddingOrder.ModifiedDate = DateTime.Now;

                            var InventoryAddingOrderInsertion = _unitOfWork.InventoryAddingOrders.Add(newInventoryAddingOrder);
                            _unitOfWork.Complete();

                            if (InventoryAddingOrderInsertion != null)
                            {
                                #region prepare 
                                InventoryAddingOrderID = InventoryAddingOrderInsertion.Id;
                                #region items
                                var matrialDataIDs = Request.MatrialAddingOrderItemList.Select(a => a.InventoryItemID).ToList();
                                var MatrialData = _unitOfWork.InventoryItems.FindAll(a => matrialDataIDs.Contains(a.Id));

                                var POItemIDs = Request.MatrialAddingOrderItemList.Select(a => a.POID).ToList();
                                var POItemList = _unitOfWork.VPurchasePoItems.FindAll(a => matrialDataIDs.Contains(a.InventoryItemId) && POItemIDs.Contains(a.PurchasePoid)).ToList();

                                var LastItemAddingOrderList = _unitOfWork.VInventoryAddingOrderItems.FindAll(a => matrialDataIDs.Contains(a.InventoryItemId) && POItemIDs.Contains(a.Poid ?? 0) && a.SupplierId == SupplierID).ToList();
                                #endregion
                                foreach (var MatrialDataOBJ in Request.MatrialAddingOrderItemList)
                                {
                                    //var InventoryItemObjDB2 = _unitOfWork.InventoryItems.Find(x => x.Id == MatrialDataOBJ.InventoryItemID);//to be removed

                                    var InventoryItemObjDB = MatrialData.Where(x => x.Id == MatrialDataOBJ.InventoryItemID).FirstOrDefault();
                                    if (InventoryItemObjDB != null)
                                    {
                                        //var POItemObjDB2 = _unitOfWork.VPurchasePoItems.Find(x => x.PurchasePoid == MatrialDataOBJ.POID && x.InventoryItemId == MatrialDataOBJ.InventoryItemID);

                                        var POItemObjDB = POItemList.Where(x => x.PurchasePoid == MatrialDataOBJ.POID && x.InventoryItemId == MatrialDataOBJ.InventoryItemID).FirstOrDefault();
                                        decimal ReqQuantity = 0;
                                        if (POItemObjDB != null)
                                        {
                                            ReqQuantity = (decimal)POItemObjDB.ReqQuantity;
                                        }
                                        decimal ReceeivedOrReturnQTY = MatrialDataOBJ.QTYUOR;
                                        if (Request.OrderType == "ExternalBackOrder")
                                        {
                                            ReceeivedOrReturnQTY = ReceeivedOrReturnQTY * -1;
                                        }


                                        decimal ReceeivedOrReturnQTYForSum = MatrialDataOBJ.QTYUOR;
                                        decimal ReceivedQTYAfter = ReceeivedOrReturnQTYForSum;
                                        //var LastItemAddingOrderObjLoad2 = _unitOfWork.VInventoryAddingOrderItems.FindAll(x => x.InventoryItemId == MatrialDataOBJ.InventoryItemID && x.Poid == MatrialDataOBJ.POID && x.SupplierId == SupplierID).ToList().LastOrDefault();

                                        var LastItemAddingOrderObjLoad = LastItemAddingOrderList.Where(x => x.InventoryItemId == MatrialDataOBJ.InventoryItemID && x.Poid == MatrialDataOBJ.POID && x.SupplierId == SupplierID).ToList().LastOrDefault();
                                        if (LastItemAddingOrderObjLoad != null && LastItemAddingOrderObjLoad.RecivedQuantityAfter != null)
                                        {
                                            if (Request.OrderType == "ExternalBackOrder")
                                            {
                                                ReceeivedOrReturnQTYForSum = ReceeivedOrReturnQTYForSum * -1;
                                            }
                                            ReceivedQTYAfter = ReceeivedOrReturnQTYForSum + (decimal)LastItemAddingOrderObjLoad.RecivedQuantityAfter;
                                        }
                                        decimal RemainQTY = ReqQuantity - ReceivedQTYAfter;




                                        DateTime? ExpDate = null;
                                        if (MatrialDataOBJ.ExpData != null && MatrialDataOBJ.ExpData != "")
                                        {
                                            ExpDate = DateTime.Parse(MatrialDataOBJ.ExpData);
                                        }
                                        //add new 

                                        var AddingOrderItemObj = new InventoryAddingOrderItem();
                                        AddingOrderItemObj.InventoryAddingOrderId = InventoryAddingOrderID;
                                        AddingOrderItemObj.InventoryItemId = MatrialDataOBJ.InventoryItemID;
                                        AddingOrderItemObj.Uomid = InventoryItemObjDB.RequstionUomid;
                                        AddingOrderItemObj.ExpDate = ExpDate;
                                        AddingOrderItemObj.ItemSerial = MatrialDataOBJ.Serial;
                                        AddingOrderItemObj.QcreportId = MatrialDataOBJ.QIReport;
                                        AddingOrderItemObj.Comments = MatrialDataOBJ.Comment;
                                        AddingOrderItemObj.Poid = MatrialDataOBJ.POID;
                                        AddingOrderItemObj.ReqQuantity1 = ReqQuantity;// PO Req QTY
                                        AddingOrderItemObj.RecivedQuantity1 =ReceeivedOrReturnQTY;
                                        AddingOrderItemObj.RecivedQuantityUop = MatrialDataOBJ.QTYUOP;
                                        AddingOrderItemObj.RecivedQuantityAfter = ReceivedQTYAfter;
                                        AddingOrderItemObj.RemainQuantity = RemainQTY;

                                        _unitOfWork.InventoryAddingOrderItems.Add(AddingOrderItemObj);
                                        var AddingOrderItemInsertion = _unitOfWork.Complete();


                                        //ObjectParameter IDInventoryAddingOrderItem = new ObjectParameter("ID", typeof(long));
                                        //var AddingOrderItemInsertion = _Context.Myproc_InventoryAddingOrderItemsInsert_New(IDInventoryAddingOrderItem, InventoryAddingOrderID, MatrialDataOBJ.InventoryItemID,
                                        //                                                                             InventoryItemObjDB.RequstionUOMID,
                                        //                                                                             // null,null,// (float)MatrialDataOBJ.AddedQTYUOR, //ReqQuantity,
                                        //                                                                             //(float)MatrialDataOBJ.AddedQTYUOR,
                                        //                                                                             ExpDate,
                                        //                                                                             MatrialDataOBJ.Serial, MatrialDataOBJ.QIReport, MatrialDataOBJ.Comment, MatrialDataOBJ.POID,
                                        //                                                                                ReqQuantity,// PO Req QTY
                                        //                                                                                ReceeivedOrReturnQTY,
                                        //                                                                                ReceivedQTYAfter,
                                        //                                                                                RemainQTY
                                        //                                                                             );
                                        if (AddingOrderItemInsertion > 0)
                                        {
                                            long AddingOrderItemId = AddingOrderItemObj.Id;
                                            if (Request.OrderType == "AddingOrder" && POItemObjDB != null)
                                            {

                                                var res = UpdatePoStatus(POItemObjDB.Id,
                                                                MatrialDataOBJ.POID,
                                                                  MatrialDataOBJ.QTYUOR);
                                            }

                                            decimal BalanceQTY = MatrialDataOBJ.QTYUOR;
                                            long? ParentInvStoreItem = null;
                                            if (Request.OrderType == "ExternalBackOrder")
                                            {
                                                BalanceQTY = BalanceQTY * -1;
                                                ParentInvStoreItem = MatrialDataOBJ.ParentInventoryStoreItem;
                                            }
                                            long? POInvoiceId = null;
                                            decimal? POInvoiceTotalPrice = null;
                                            decimal? POInvoiceTotalCost = null;
                                            int? currencyId = null;
                                            decimal? rateToEGP = null;
                                            decimal? POInvoiceTotalPriceEGP = null;
                                            decimal? POInvoiceTotalCostEGP = null;
                                            decimal? remainItemPrice = null; // Not Used Now
                                            decimal? remainItemCosetEGP = null;
                                            decimal? remainItemCostOtherCU = null;
                                            if (Request.OrderType == "AddingOrder")
                                            {

                                                if (POItemObjDB != null)
                                                {
                                                    POInvoiceId = _unitOfWork.PurchasePOInvoices.FindAll(x => x.Poid == MatrialDataOBJ.POID).Select(x => x.Id).FirstOrDefault();
                                                    currencyId = POItemObjDB.CurrencyId ?? 1;
                                                    rateToEGP = POItemObjDB.RateToEgp ?? 1;
                                                    POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                    POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                    remainItemCosetEGP = BalanceQTY * POInvoiceTotalCostEGP ?? 0;
                                                    POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                    POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                    remainItemCostOtherCU = BalanceQTY * (POInvoiceTotalCost ?? 0);
                                                }
                                            }


                                            var InventoryStoreItemOBJ = new InventoryStoreItem();
                                            InventoryStoreItemOBJ.InventoryStoreId = InventoryStoreID;
                                            InventoryStoreItemOBJ.InventoryItemId = MatrialDataOBJ.InventoryItemID;
                                            InventoryStoreItemOBJ.OrderNumber = InventoryAddingOrderID.ToString();
                                            InventoryStoreItemOBJ.OrderId = InventoryAddingOrderID;
                                            InventoryStoreItemOBJ.CreatedBy = UserID;
                                            InventoryStoreItemOBJ.ModifiedBy = UserID;
                                            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                            InventoryStoreItemOBJ.OperationType = OperationType;
                                            InventoryStoreItemOBJ.Balance = (double)BalanceQTY;
                                            InventoryStoreItemOBJ.Balance1 = BalanceQTY;
                                            InventoryStoreItemOBJ.InvenoryStoreLocationId = MatrialDataOBJ.StoreLocationID;
                                            InventoryStoreItemOBJ.ExpDate = ExpDate;
                                            InventoryStoreItemOBJ.ItemSerial = MatrialDataOBJ.Serial;
                                            InventoryStoreItemOBJ.ReleaseParentId = ParentInvStoreItem;
                                            InventoryStoreItemOBJ.FinalBalance = BalanceQTY;
                                            InventoryStoreItemOBJ.AddingOrderItemId = AddingOrderItemId;
                                            InventoryStoreItemOBJ.AddingFromPoid = MatrialDataOBJ.POID;
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
                                            _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                            var InventoryStorItemInsertion = _unitOfWork.Complete();
                                            //ObjectParameter IDInventoryStoreItemInsertion = new ObjectParameter("ID", typeof(long));
                                            //var InventoryStorItemInsertion = _Context.Myproc_InventoryStoreItemInsert_API(IDInventoryStoreItemInsertion,
                                            //                                                                        InventoryStoreID,
                                            //                                                                        MatrialDataOBJ.InventoryItemID,
                                            //                                                                        InventoryAddingOrderID.ToString(),
                                            //                                                                        InventoryAddingOrderID,
                                            //                                                                        // (double)MatrialDataOBJ.AddedQTYUOR,
                                            //                                                                        DateTime.Now,
                                            //                                                                        validation.userID,
                                            //                                                                        DateTime.Now,
                                            //                                                                        validation.userID,
                                            //                                                                        OperationType,
                                            //                                                                        BalanceQTY,
                                            //                                                                        MatrialDataOBJ.StoreLocationID,
                                            //                                                                        ExpDate,
                                            //                                                                         MatrialDataOBJ.Serial,
                                            //                                                                         ParentInvStoreItem,
                                            //                                                                         BalanceQTY,
                                            //                                                                         AddingOrderItemId,
                                            //                                                                         MatrialDataOBJ.POID,
                                            //                                                                         // Extra Data PO Item
                                            //                                                                         POInvoiceId,
                                            //                                                                         POInvoiceTotalPrice,
                                            //                                                                         POInvoiceTotalCost,
                                            //                                                                         currencyId,
                                            //                                                                         rateToEGP,
                                            //                                                                         POInvoiceTotalPriceEGP,
                                            //                                                                         POInvoiceTotalCostEGP,
                                            //                                                                         remainItemPrice,
                                            //                                                                         remainItemCosetEGP,
                                            //                                                                         remainItemCostOtherCU,
                                            //                                                                         0,
                                            //                                                                         null
                                            //                                                                        );


                                            if (InventoryStorItemInsertion > 0 && Request.OrderType == "ExternalBackOrder" && ParentInvStoreItem != null)
                                            {
                                                // Update Parent  on InventoryStoreItem
                                                var ParentInventoryStoreItem = _unitOfWork.InventoryStoreItems.Find(x => x.Id == ParentInvStoreItem);
                                                if (ParentInventoryStoreItem != null)
                                                {
                                                    ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance + (BalanceQTY);
                                                    ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                    ParentInventoryStoreItem.ModifiedBy = UserID;
                                                    // Update PO Item Columns
                                                    // Check if Not call PO Item On Parent 
                                                    decimal finalBalance = ParentInventoryStoreItem.FinalBalance ?? 0;
                                                    if (ParentInventoryStoreItem.AddingFromPoid != null)
                                                    {
                                                        var POParentItemObjDB = _unitOfWork.VPurchasePoItems.Find(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
                                                                                                           && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
                                                                                                            );
                                                        if (POParentItemObjDB != null)
                                                        {
                                                            var POInvoice = _unitOfWork.PurchasePOInvoices.Find(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid);
                                                            POInvoiceId = POInvoice.Id;
                                                            currencyId = POParentItemObjDB.CurrencyId ?? 0;
                                                            rateToEGP = POParentItemObjDB.RateToEgp ?? 0;
                                                            POInvoiceTotalPriceEGP = POParentItemObjDB.ActualUnitPrice ?? 0;
                                                            POInvoiceTotalCostEGP = POParentItemObjDB.FinalUnitCost ?? 0;
                                                            POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                            POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                        }
                                                    }
                                                    ParentInventoryStoreItem.RemainItemCosetEgp = ParentInventoryStoreItem.FinalBalance ?? 0 * ParentInventoryStoreItem.PoinvoiceTotalCostEgp ?? 0;
                                                    ParentInventoryStoreItem.RemainItemCostOtherCu = ParentInventoryStoreItem.FinalBalance ?? 0 * ParentInventoryStoreItem.PoinvoiceTotalCost ?? 0;
                                                    _unitOfWork.Complete();
                                                }
                                            }


                                            //if (Request.OrderType == "AddingOrder")
                                            //{
                                            // -------------------------------------------------Update and Calc Average for Inventory Item ----------------------------------------------------------------
                                            var ListInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemObjDB.Id && x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
                                            var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
                                            var ListIDSPOInvoicesIsFulllyPriced = _unitOfWork.PurchasePOInvoices.FindAll(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
                                            ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
                                            InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;
                                            // Update Avg Unit Price
                                            InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
                                            _unitOfWork.Complete();
                                            //}


                                        }


                                    }

                                    //GarasERP.InventoryStoreItem storeItem = new GarasERP.InventoryStoreItem();
                                    //storeItem.AddNew();
                                    //storeItem.Balance = (decimal)items.RecivedQuantity;
                                    //storeItem.CreatedBy = UserID;
                                    //storeItem.CreationDate = DateTime.Now;
                                    //storeItem.InventoryItemID = items.InventoryItemID;
                                    //storeItem.InventoryStoreID = order.InventoryStoreID;
                                    //storeItem.ModifiedBy = UserID;
                                    //storeItem.ModifiedDate = DateTime.Now;
                                    //storeItem.OperationType = order.OperationType;
                                    //storeItem.OrderID = order.ID;
                                    //storeItem.OrderNumber = order.s_ID;
                                    //storeItem.Save();

                                    ////check notification
                                    //InventoryItem item = new InventoryItem();
                                    //item.Where.ID.Value = items.InventoryItemID;
                                    //item.Query.AddResultColumn(InventoryItem.ColumnNames.MaxBalance);
                                    //item.Query.AddResultColumn(InventoryItem.ColumnNames.Name);
                                    //if (item.Query.Load())
                                    //{
                                    //    if (item.DefaultView != null && item.DefaultView.Count > 0)
                                    //    {
                                    //        GarasERP.InventoryStoreItem itemBalance = new GarasERP.InventoryStoreItem();
                                    //        itemBalance.Where.InventoryItemID.Value = items.InventoryItemID;
                                    //        // itemBalance.Query.AddResultColumn(GarasERP.InventoryStoreItem.ColumnNames.Balance);
                                    //        itemBalance.Query.AddGroupBy(GarasERP.InventoryStoreItem.ColumnNames.InventoryItemID);
                                    //        itemBalance.Aggregate.Balance.Function = MyGeneration.dOOdads.AggregateParameter.Func.Sum;
                                    //        if (itemBalance.Query.Load())
                                    //        {
                                    //            if (itemBalance.DefaultView != null && itemBalance.DefaultView.Count > 0)
                                    //            {
                                    //                if (itemBalance.s_Balance != "")
                                    //                {
                                    //                    if (itemBalance.Balance > (decimal)item.MaxBalance)
                                    //                    {
                                    //                        CommonClass.sendGroupNotifications("TopManagment", "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                                    //                            "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                                    //                            "~/inventory/ViewItem.aspx?TID=" + Server.UrlEncode(Encrypt_Decrypt.Encrypt(items.s_InventoryItemID, key)));

                                    //                        CommonClass.sendGroupNotifications("Secretary", "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                                    //                           "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                                    //                           "~/inventory/ViewItem.aspx?TID=" + Server.UrlEncode(Encrypt_Decrypt.Encrypt(items.s_InventoryItemID, key)));
                                    //                    }
                                    //                }
                                    //            }
                                    //        }

                                    //    }
                                    //}

                                }

                                #endregion items
                                Response.Result = true;
                                Response.ID = InventoryAddingOrderID;
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

                var innerExceptionMessage = (!string.IsNullOrEmpty(ex.InnerException.Message.ToString())) == true ? ex.InnerException.Message.ToString() : null;
                var message = ex.Message;

                _logService.AddLogError("AddInventoryAddingAndExternalBackOrder", message, UserID, CompanyName, innerExceptionMessage);


                return Response;
            }
        }


        public BaseResponseWithId<long> ReverseInventoryAddingOrder(ReverseInventoryAddingOrderRequest Request, long UserID, string compName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Request == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "please insert a valid data.";
                    Response.Errors.Add(error);
                    return Response;
                }

                var addingOrder = _unitOfWork.InventoryAddingOrders.GetById(Request.AddingOrderId);
                if (addingOrder == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P13";
                    error.ErrorMSG = "Adding Order Not Found.";
                    Response.Errors.Add(error);
                    return Response;
                }

                var newInventoryAddingOrder = new InventoryAddingOrder();
                newInventoryAddingOrder = addingOrder;
                newInventoryAddingOrder.Id = 0;
                newInventoryAddingOrder.OperationType = "Add External Back Order";
                newInventoryAddingOrder.CreationDate = DateTime.Now;
                newInventoryAddingOrder.CreatedBy = UserID;
                newInventoryAddingOrder.ModifiedBy = UserID;
                newInventoryAddingOrder.ModifiedDate = DateTime.Now;
                var InventoryAddingOrderInsertion = _unitOfWork.InventoryAddingOrders.Add(newInventoryAddingOrder);
                _unitOfWork.Complete();

                Response.ID = newInventoryAddingOrder.Id;


                var items = _unitOfWork.InventoryStoreItems.FindAll(a => a.OrderId == Request.AddingOrderId && a.OperationType == "Add New Matrial");

                if (items.Where(a => a.Balance1 != a.FinalBalance).Count() > 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P14";
                    error.ErrorMSG = "تم سحب كمية من هذا المنتج من قبل";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (items.Count() > 0)
                {
                    foreach (var item in items)
                    {
                        if (Request.IsReverse)
                        {
                            var InventoryStoreItemOBJ = new InventoryStoreItem();
                            InventoryStoreItemOBJ = item;
                            InventoryStoreItemOBJ.Id = 0;
                            InventoryStoreItemOBJ.CreatedBy = UserID;
                            InventoryStoreItemOBJ.ModifiedBy = UserID;
                            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                            InventoryStoreItemOBJ.OperationType = "Add External Back Order";
                            InventoryStoreItemOBJ.Balance = (double)item.Balance1 * -1;
                            InventoryStoreItemOBJ.Balance1 = (decimal)item.Balance1;
                            InventoryStoreItemOBJ.FinalBalance = (decimal)item.Balance1;
                            InventoryStoreItemOBJ.OrderId = newInventoryAddingOrder.Id;
                            InventoryStoreItemOBJ.OrderNumber = newInventoryAddingOrder.Id.ToString();
                            _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                            _unitOfWork.Complete();
                        }
                        else if (!Request.IsReverse)
                        {
                            if (item.Balance1 == item.FinalBalance)
                            {
                                _unitOfWork.InventoryStoreItems.Delete(item);
                                _unitOfWork.Complete();
                                _unitOfWork.InventoryAddingOrders.Delete(addingOrder);
                                _unitOfWork.Complete();
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

                var innerExceptionMessage = (!string.IsNullOrEmpty(ex.InnerException.Message.ToString())) == true ? ex.InnerException.Message.ToString() : null;
                var message = ex.Message;

                _logService.AddLogError("ReverseInventoryAddingOrder", message, UserID, compName, innerExceptionMessage);


                return Response;
            }
        }

        private bool UpdatePoStatus(long POItemID, long poId, decimal receivedQuantity)
        {
            //PurchasePOItem purchasePOItem = new PurchasePOItem();
            //PurchasePO purchasePO = new PurchasePO();

            var PurchasePOItemListdb = _unitOfWork.PurchasePOItems.FindAll(x => x.PurchasePoid == poId).ToList();


            // Save received quantity
            var PurchasePOItemLoadPrimary = PurchasePOItemListdb.Where(x => x.Id == POItemID).FirstOrDefault();
            if (PurchasePOItemLoadPrimary != null)
            {
                decimal PurchasePOItemRecivedQTY = PurchasePOItemLoadPrimary.RecivedQuantity1 != null ? (decimal)PurchasePOItemLoadPrimary.RecivedQuantity1 : 0;
                PurchasePOItemLoadPrimary.RecivedQuantity1 = (PurchasePOItemRecivedQTY + receivedQuantity);
                //_Context.Myproc_PurchasePOItemUpdate_RecivedQuantity(POItemID, PurchasePOItemRecivedQTY + receivedQuantity);
            }


            // Check PO to be close or open
            bool result = false;
            //var PurchasesPOItemLoad = _Context.proc_PurchasePOItemLoadAll().Where(x => x.PurchasePOID == poId).ToList();
            foreach (var purchasePOItem in PurchasePOItemListdb)
            {
                if (purchasePOItem.RecivedQuantity1 >= purchasePOItem.ReqQuantity1)
                {
                    result = true;
                }
                else
                {
                    result = false;
                    break;
                }
            }
            //purchasePOItem = new PurchasePOItem();
            //purchasePOItem.Where.PurchasePOID.Value = poId;
            //if (purchasePOItem.Query.Load())
            //{
            //    if (purchasePOItem.DefaultView != null && purchasePOItem.DefaultView.Count > 0)
            //    {
            //        do
            //        {
            //            if (purchasePOItem.RecivedQuantity >= purchasePOItem.ReqQuantity)
            //            {
            //                result = true;
            //            }
            //            else
            //            {
            //                result = false;
            //                break;
            //            }

            //        } while (purchasePOItem.MoveNext());
            //    }
            //}
            var CheckPurchasePOIsClosed = PurchasePOItemListdb.Where(x => x.RecivedQuantity1 < x.ReqQuantity1).Count();
            //if (result)
            if (CheckPurchasePOIsClosed == 0)
            {
                var PO = _unitOfWork.PurchasePos.GetById(poId);
                PO.Status = "Closed";

                _unitOfWork.Complete();

                //var PurchasePOUpdate = _Context.Myproc_PurchasePOUpdate_StatusFlag(poId, "Closed");
                //purchasePO.Where.ID.Value = poId;
                //if (purchasePO.Query.Load())
                //{
                //    if (purchasePO.DefaultView != null && purchasePO.DefaultView.Count > 0)
                //    {
                //        purchasePO.Status = "Closed";
                //        purchasePO.Save();
                //    }
                //}
            }



            return true;
        }

        public async Task<BaseResponseWithID> AddPOToInventoryStoreItem([FromBody] AddPOToInventoryStoreItemRequest Request, long userId, string CompName)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long? POID = 0;
                    if (Request.POID == null || Request.POID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err35";
                        error.ErrorMSG = "Invalid POID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        POID = Request.POID;
                        var CheckPO = await _unitOfWork.PurchasePos.FindAsync(x => x.Id == POID);
                        if (CheckPO == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err35";
                            error.ErrorMSG = "Invalid POID , Is not exist !";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }



                    long InventoryItemID = 0;
                    if (Request.InventoryItemID != 0)
                    {
                        var CheckInventoryItem = await _unitOfWork.InventoryItems.FindAsync(x => x.Id == Request.InventoryItemID);
                        if (CheckInventoryItem == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Inventory Item ID.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        else
                        {
                            InventoryItemID = (long)Request.InventoryItemID;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Inventory Item ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.RateToEGP == null || Request.RateToEGP == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid RateTOEGP.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {
                        var POItemObjDB = await _unitOfWork.VPurchasePoItems.FindAsync(x => x.PurchasePoid == POID && x.InventoryItemId == InventoryItemID);
                        if (POItemObjDB != null)
                        {

                            var ParentInventoryStoreItemList = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => x.InventoryItemId == InventoryItemID && x.AddingFromPoid == POID);
                            if (ParentInventoryStoreItemList.Count() > 0)
                            {
                                long? POInvoiceId = _unitOfWork.PurchasePOInvoices.Find(x => x.Poid == POID).Id;
                                foreach (var item in ParentInventoryStoreItemList)
                                {
                                    decimal? BalanceQTY = item.FinalBalance;
                                    //decimal? remainItemPrice = null; // Not Used Now
                                    int? currencyId = POItemObjDB.CurrencyId ?? 0;
                                    decimal? rateToEGP = POItemObjDB.RateToEgp ?? 0;
                                    decimal? POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                    decimal? POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                    decimal? remainItemCosetEGP = BalanceQTY * POInvoiceTotalCostEGP ?? 0;
                                    decimal? POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                    decimal? POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                    decimal? remainItemCostOtherCU = BalanceQTY * (POInvoiceTotalCost ?? 0);


                                    item.PoinvoiceId = POInvoiceId;
                                    item.CurrencyId = currencyId;
                                    item.RateToEgp = rateToEGP;
                                    item.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                    item.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                    item.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                    item.PoinvoiceTotalCost = POInvoiceTotalCost;
                                    item.RemainItemCosetEgp = remainItemCosetEGP;
                                    item.RemainItemCostOtherCu = remainItemCostOtherCU;
                                    _unitOfWork.Complete();

                                }
                            }
                        }
                        Response.Result = true;

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

                var innerExceptionMessage = (!string.IsNullOrEmpty(ex.InnerException.Message.ToString())) == true ? ex.InnerException.Message.ToString() : null;
                var message = ex.Message;

                _logService.AddLogError("AddPOToInventoryStoreItem", message, userId, CompName, innerExceptionMessage);


                return Response;
            }
        }


    }
}
