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
using DocumentFormat.OpenXml.Bibliography;
using NewGarasAPI.Models.Inventory.Requests;
using System.Net;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Entities;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.Inventory;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Engineering;
using Task = System.Threading.Tasks.Task;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryTransferOrderService : IInventoryTransferOrderService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryItemService _inventoryItemService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogService _logService;

        public InventoryTransferOrderService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork, IInventoryItemService inventoryItemService, ILogService logService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            _inventoryItemService = inventoryItemService;
            _logService = logService;
        }
        public async Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrderPOS(AddInventoryInternalTransferOrderRequest Request, long creator, string compName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (!Common.CheckUserRole(creator, 160, _Context))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P101";
                        error.ErrorMSG = "You don't have permission to do this Action";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    int FromInventoryStorID = 0;
                    if (Request.FromInventoryStorID != 0)
                    {
                        FromInventoryStorID = Request.FromInventoryStorID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid From Inventory Store ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    int ToInventoryStorID = 0;
                    if (Request.ToInventoryStorID != 0)
                    {
                        ToInventoryStorID = Request.ToInventoryStorID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid To Inventory Store ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.ToInventoryStorID == Request.FromInventoryStorID)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid From Inventory store and  To Inventory Store must be different.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime RecevingData = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.ReceivingDate) || !DateTime.TryParse(Request.ReceivingDate, out RecevingData))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err23";
                        error.ErrorMSG = "Invalid Receving Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalTransferOrderItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalTransferOrderItemList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var ItemTotalCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Count();
                    var ItemDistinctCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Distinct().Count();
                    if (ItemDistinctCount < ItemTotalCount)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "There is item itteration with same data Location and Serial";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var inventoryItemsIDs = Request.InternalTransferOrderItemList.Select(a => a.InventoryItemID).ToList();
                    var inventoryItemsList = await _unitOfWork.InventoryItems.FindAllAsync(a => inventoryItemsIDs.Contains(a.Id));

                    var InventoryStoreLocationsIDs = Request.InternalTransferOrderItemList.Select(a => a.StoreLocationID).ToList();
                    var InventoryStoreLocationsList = await _unitOfWork.InventoryStoreLocations.FindAllAsync(a => InventoryStoreLocationsIDs.Contains(a.Id));

                    var CheckToInventoryReportListDB = (await _unitOfWork.InventoryReports.FindAllAsync(x => x.Active == true && x.Approved == false && x.Closed == false && x.InventoryStoreId == ToInventoryStorID)).ToList();

                    for (int i = 0; i < Request.InternalTransferOrderItemList.Count; i++)
                    {
                        var item = Request.InternalTransferOrderItemList[i];
                        var itemData = inventoryItemsList.Where(a => a.Id == item.InventoryItemID).FirstOrDefault();
                        if (itemData == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err347";
                            error.ErrorMSG = "Invalid Inventory Item ID at Selected item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        var location = InventoryStoreLocationsList.Where(a => a.Id == item.StoreLocationID).FirstOrDefault();
                        if (item.StoreLocationID != null && location == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err347";
                            error.ErrorMSG = "Invalid Inventory Store Location Selected item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if (item.InventoryItemID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if (item.TransferredQTY < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err29";
                            error.ErrorMSG = "Invalid QTY item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if ((item.StockBalanceList == null || item.StockBalanceList.Count < 1) && item.IsFIFO == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-416";
                            error.ErrorMSG = "you must select  StockBalanceList or Setting IsFIFO on item NO#" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if (item.StockBalanceList != null)
                        {
                            if (item.StockQTY != null)
                            {
                                if (item.TransferredQTY > item.StockQTY)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-416";
                                    error.ErrorMSG = "Parent Release Item selected Not have balance enough on Item NO#" + i + 1;
                                    Response.Errors.Add(error);
                                }
                            }

                        }
                    }

                    long InventoryInternalTransferOrderID = 0;
                    if (Response.Result)
                    {
                        // Check Inventory Report Approved and closed or not
                        if (CheckToInventoryReportListDB.Count > 0)
                        {
                            string storeName = (await _unitOfWork.InventoryStores.GetByIdAsync(ToInventoryStorID))?.Name;
                            for (int i = 0; i < CheckToInventoryReportListDB.Count; i++)
                            {
                                var InventoryRep = CheckToInventoryReportListDB[i];
                                if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                {
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
                            // Insertion Internal Transfer Order
                            var InventoryAddingOrder = new InventoryInternalTransferOrder()
                            {
                                OperationType = "Add New Transfer",
                                Revision = 0,
                                FromInventoryStoreId = FromInventoryStorID,
                                ToInventoryStoreId = ToInventoryStorID,
                                RecivingDate = RecevingData,
                                CreatedBy = creator,
                                ModifiedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                            };

                            await _unitOfWork.InventoryInternalTransferOrders.AddAsync(InventoryAddingOrder);
                            var InventoryAddingOrderInsertion = await _unitOfWork.CompleteAsync();

                            if (InventoryAddingOrderInsertion > 0)
                            {
                                InventoryInternalTransferOrderID = InventoryAddingOrder.Id;
                                #region items

                                // Get All InventoryStoreItems Used in this Release before loop
                                var IDSInventoryItemListRequested = Request.InternalTransferOrderItemList.Select(x => x.InventoryItemID).Distinct().ToList();
                                var InventoryStoreItemList = (await _unitOfWork.InventoryStoreItems.FindAllAsync(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId))).ToList();
                                var POItemList = (await _unitOfWork.PurchasePOItems.FindAllAsync(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId), includes: new[] { "PurchasePo.PurchasePoinvoices" })).ToList();
                                var InventoryStoreItemListForInsertDB = new List<InventoryStoreItem>();

                                for (int i = 0; i < Request.InternalTransferOrderItemList.Count; i++)
                                {
                                    var InternalTransferDataOBJ = Request.InternalTransferOrderItemList[i];
                                    // Check QTY Release From Parent is enough or not

                                    List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = InternalTransferDataOBJ.StockBalanceList;// Case 1 : Request ParentReleaseItemID
                                                                                                                                            // Calc Parent Release , Final Balance After Release

                                    if (InternalTransferDataOBJ.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
                                    {
                                        ParentReleaseWithQTYListID = _inventoryItemService.GetParentReleaseIDWithSettingStore(InventoryStoreItemList
                                                                                                    , InternalTransferDataOBJ.InventoryItemID,
                                                                                                    FromInventoryStorID,
                                                                                                    InternalTransferDataOBJ.StoreLocationID,// store location
                                                                                                    (decimal)InternalTransferDataOBJ.TransferredQTY,
                                                                                                    InternalTransferDataOBJ.IsFIFO);
                                    }

                                    var InventoryItemObjDB = inventoryItemsList.Where(x => x.Id == InternalTransferDataOBJ.InventoryItemID).FirstOrDefault();
                                    if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < InternalTransferDataOBJ.TransferredQTY)
                                    {

                                        Response.Result = true;
                                        Error error = new Error();
                                        error.ErrorCode = "Err325";
                                        error.ErrorMSG = "Not have availble qty from parent  Item to release on Item " + InventoryItemObjDB.Name + "- NO #" + i + 1; // Warning
                                        Response.Errors.Add(error);
                                    }



                                    if (InventoryItemObjDB != null && ParentReleaseWithQTYListID.Count() > 0)
                                    {
                                        var TransferItem = new InventoryInternalTransferOrderItem()
                                        {
                                            InventoryInternalTransferOrderId = InventoryInternalTransferOrderID,
                                            InventoryItemId = InternalTransferDataOBJ.InventoryItemID,
                                            Uomid = InventoryItemObjDB.RequstionUomid,
                                            Comments = InternalTransferDataOBJ.Comment,
                                            TransferredQty = InternalTransferDataOBJ.TransferredQTY
                                        };
                                        await _unitOfWork.InventoryInternalTransferOrderItems.AddAsync(TransferItem);

                                        var TransferItemInsertion = await _unitOfWork.CompleteAsync();
                                        //add new 

                                        if (TransferItemInsertion > 0)
                                        {

                                            long TransferOrderItemId = TransferItem.Id;
                                            decimal RemainReleaseQTY = (decimal)InternalTransferDataOBJ.TransferredQTY;
                                            //var inventoryStoreItemsIDs = ParentReleaseWithQTYListID.Select(a => a.ID);
                                            //var inventoryStoreItemsList = _unitOfWork.InventoryStoreItems.FindAll(x => inventoryStoreItemsIDs.Contains(x.Id)).ToList();
                                            for (int j = 0; j < ParentReleaseWithQTYListID.Count; j++)
                                            {
                                                var ObjParentRelease = ParentReleaseWithQTYListID[j];
                                                decimal ReleaseQTY = 0;
                                                if (RemainReleaseQTY <= ObjParentRelease.StockBalance)
                                                {
                                                    ReleaseQTY = RemainReleaseQTY;
                                                }
                                                else
                                                {
                                                    ReleaseQTY = ObjParentRelease.StockBalance;
                                                }

                                                if (RemainReleaseQTY > 0)
                                                {
                                                    RemainReleaseQTY -= ReleaseQTY;
                                                    var ParentInventoryStoreItem = InventoryStoreItemList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
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
                                                    decimal? remainItemCosetEGPForRelease = null;
                                                    decimal? remainItemCostOtherCUForRelease = null;
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
                                                        decimal? finalBalance = (ParentInventoryStoreItem.FinalBalance - (ReleaseQTY)) ?? 0;
                                                        if (ParentInventoryStoreItem.AddingFromPoid != null)
                                                        {

                                                            var POItemObjDB = POItemList.Where(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
                                                                                                               && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
                                                                                                                ).FirstOrDefault();
                                                            if (POItemObjDB != null)
                                                            {
                                                                POInvoiceId = POItemObjDB?.PurchasePo?.PurchasePoinvoices.Where(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid).Select(x => x.Id).FirstOrDefault();
                                                                currencyId = POItemObjDB.CurrencyId ?? 1;
                                                                rateToEGP = POItemObjDB.RateToEgp ?? 1;
                                                                POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                                POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                                remainItemCosetEGPForRelease = ReleaseQTY * POInvoiceTotalCostEGP ?? 0;
                                                                remainItemCostOtherCUForRelease = ReleaseQTY * POInvoiceTotalCost ?? 0;


                                                            }
                                                        }
                                                    }
                                                    var InventoryStoreItemOBJ = new InventoryStoreItem();
                                                    InventoryStoreItemOBJ.InventoryStoreId = FromInventoryStorID;
                                                    InventoryStoreItemOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
                                                    InventoryStoreItemOBJ.OrderNumber = InventoryInternalTransferOrderID.ToString();
                                                    InventoryStoreItemOBJ.OrderId = InventoryInternalTransferOrderID;
                                                    InventoryStoreItemOBJ.CreatedBy = creator;
                                                    InventoryStoreItemOBJ.ModifiedBy = creator;
                                                    InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                    InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                    InventoryStoreItemOBJ.OperationType = "Transfer Order (Released)";
                                                    InventoryStoreItemOBJ.Balance = (double)(-ReleaseQTY);
                                                    InventoryStoreItemOBJ.Balance1 = (decimal)(-ReleaseQTY);
                                                    InventoryStoreItemOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID != null ? InternalTransferDataOBJ.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationId;
                                                    InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                                    InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                                    InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                                                    InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                                                    InventoryStoreItemOBJ.AddingOrderItemId = TransferOrderItemId;
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
                                                    InventoryStoreItemListForInsertDB.Add(InventoryStoreItemOBJ);
                                                    //_unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                                    //var InventoryStorItemInsertion = _unitOfWork.Complete();
                                                    //if (InventoryStorItemInsertion > 0)
                                                    //{
                                                    if (ParentInventoryStoreItem != null)
                                                    {
                                                        ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                                                        ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                        ParentInventoryStoreItem.ModifiedBy = creator;
                                                        ParentInventoryStoreItem.PoinvoiceId = POInvoiceId;
                                                        ParentInventoryStoreItem.CurrencyId = currencyId;
                                                        ParentInventoryStoreItem.RateToEgp = rateToEGP;
                                                        ParentInventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                        ParentInventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                        ParentInventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                        ParentInventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                        ParentInventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
                                                        ParentInventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
                                                        await _unitOfWork.CompleteAsync();
                                                        var InventoryStoreItemTransferReceivedOBJ = new InventoryStoreItem();
                                                        InventoryStoreItemTransferReceivedOBJ.InventoryStoreId = ToInventoryStorID;
                                                        InventoryStoreItemTransferReceivedOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
                                                        InventoryStoreItemTransferReceivedOBJ.OrderNumber = InventoryInternalTransferOrderID.ToString();
                                                        InventoryStoreItemTransferReceivedOBJ.OrderId = InventoryInternalTransferOrderID;
                                                        InventoryStoreItemTransferReceivedOBJ.CreatedBy = creator;
                                                        InventoryStoreItemTransferReceivedOBJ.ModifiedBy = creator;
                                                        InventoryStoreItemTransferReceivedOBJ.CreationDate = DateTime.Now;
                                                        InventoryStoreItemTransferReceivedOBJ.ModifiedDate = DateTime.Now;
                                                        InventoryStoreItemTransferReceivedOBJ.OperationType = "Transfer Order (Received)";
                                                        InventoryStoreItemTransferReceivedOBJ.Balance = (double)ReleaseQTY;
                                                        InventoryStoreItemTransferReceivedOBJ.Balance1 = (decimal)ReleaseQTY;
                                                        InventoryStoreItemTransferReceivedOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID;
                                                        InventoryStoreItemTransferReceivedOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                                        InventoryStoreItemTransferReceivedOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                                        InventoryStoreItemTransferReceivedOBJ.ReleaseParentId = ObjParentRelease.ID;
                                                        InventoryStoreItemTransferReceivedOBJ.FinalBalance = ReleaseQTY;
                                                        InventoryStoreItemTransferReceivedOBJ.AddingOrderItemId = TransferOrderItemId;
                                                        InventoryStoreItemTransferReceivedOBJ.AddingFromPoid = POID;
                                                        InventoryStoreItemTransferReceivedOBJ.PoinvoiceId = POInvoiceId;
                                                        InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                        InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                        InventoryStoreItemTransferReceivedOBJ.CurrencyId = currencyId;
                                                        InventoryStoreItemTransferReceivedOBJ.RateToEgp = rateToEGP;
                                                        InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                        InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                        InventoryStoreItemTransferReceivedOBJ.RemainItemPrice = remainItemPrice;
                                                        InventoryStoreItemTransferReceivedOBJ.RemainItemCosetEgp = remainItemCosetEGP;
                                                        InventoryStoreItemTransferReceivedOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
                                                        InventoryStoreItemTransferReceivedOBJ.HoldQty = 0;
                                                        InventoryStoreItemListForInsertDB.Add(InventoryStoreItemTransferReceivedOBJ);

                                                        //_unitOfWork.InventoryStoreItems.Add(InventoryStoreItemTransferReceivedOBJ);
                                                        //var InventoryStorItemTransferReceivedInsertion = _unitOfWork.Complete();




                                                        //var ListInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemObjDB.Id && x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
                                                        //var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
                                                        //var ListIDSPOInvoicesIsFulllyPriced = _unitOfWork.PurchasePOInvoices.FindAll(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
                                                        //ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
                                                        //InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;
                                                        //InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
                                                        //InventoryStoreItemTransferReceivedOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
                                                        //_unitOfWork.Complete();
                                                    }
                                                    //}

                                                }
                                            }
                                        }
                                    }
                                }

                                if (InventoryStoreItemListForInsertDB.Count() > 0)
                                {
                                    await _unitOfWork.InventoryStoreItems.AddRangeAsync(InventoryStoreItemListForInsertDB);
                                    await _unitOfWork.CompleteAsync();
                                }
                                else
                                {
                                    _unitOfWork.InventoryInternalTransferOrders.Delete(InventoryAddingOrder);
                                    await _unitOfWork.CompleteAsync();
                                }
                                #endregion items
                                Response.Result = true;
                                Response.ID = InventoryInternalTransferOrderID;
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

                _logService.AddLogError("AddInvnetoryInternalTransferOrder", message, creator, compName, innerExceptionMessage);

                return Response;
            }
        }

        public async Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrderItemsPOS(AddInventoryInternalTransferOrderItemsRequest Request, long creator, string compName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (!Common.CheckUserRole(creator, 160, _Context))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P101";
                        error.ErrorMSG = "You don't have permission to do this Action";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var transferOrder = await _unitOfWork.InventoryInternalTransferOrders.GetByIdAsync(Request.TransferOrderId);
                    if (transferOrder == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err108";
                        error.ErrorMSG = "Transfer Order ID is Not Right";
                        Response.Errors.Add(error);
                    }
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.InternalTransferOrderItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalTransferOrderItemList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var ItemTotalCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Count();
                    var ItemDistinctCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Distinct().Count();
                    if (ItemDistinctCount < ItemTotalCount)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "There is item itteration with same data Location and Serial";
                        Response.Errors.Add(error);
                        return Response;

                    }
                    var inventoryItemsIDs = Request.InternalTransferOrderItemList.Select(a => a.InventoryItemID).ToList();
                    var inventoryItemsList = await _unitOfWork.InventoryItems.FindAllAsync(a => inventoryItemsIDs.Contains(a.Id));

                    var InventoryStoreLocationsIDs = Request.InternalTransferOrderItemList.Select(a => a.StoreLocationID).ToList();
                    var InventoryStoreLocationsList = await _unitOfWork.InventoryStoreLocations.FindAllAsync(a => InventoryStoreLocationsIDs.Contains(a.Id));

                    for (int i = 0; i < Request.InternalTransferOrderItemList.Count; i++)
                    {
                        var item = Request.InternalTransferOrderItemList[i];
                        var itemData = inventoryItemsList.Where(a => a.Id == item.InventoryItemID).FirstOrDefault();
                        if (itemData == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err347";
                            error.ErrorMSG = "Invalid Inventory Item ID at Selected item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        var location = InventoryStoreLocationsList.Where(a => a.Id == item.StoreLocationID).FirstOrDefault();
                        if (item.StoreLocationID != null && location == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err347";
                            error.ErrorMSG = "Invalid Inventory Store Location Selected item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if (item.InventoryItemID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if (item.TransferredQTY < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err29";
                            error.ErrorMSG = "Invalid QTY item #" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if ((item.StockBalanceList == null || item.StockBalanceList.Count < 1) && item.IsFIFO == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-416";
                            error.ErrorMSG = "you must select  StockBalanceList or Setting IsFIFO on item NO#" + i + 1;
                            Response.Errors.Add(error);
                        }
                        if (item.StockBalanceList != null)
                        {
                            if (item.StockQTY != null)
                            {
                                if (item.TransferredQTY > item.StockQTY)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-416";
                                    error.ErrorMSG = "Parent Release Item selected Not have balance enough on Item NO#" + i + 1;
                                    Response.Errors.Add(error);
                                }
                            }

                        }
                    }

                    var InventoryInternalTransferOrderID = transferOrder.Id;

                    // Get All InventoryStoreItems Used in this Release before loop
                    var IDSInventoryItemListRequested = Request.InternalTransferOrderItemList.Select(x => x.InventoryItemID).Distinct().ToList();
                    var InventoryStoreItemList = (await _unitOfWork.InventoryStoreItems.FindAllAsync(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId))).ToList();
                    var POItemList = (await _unitOfWork.PurchasePOItems.FindAllAsync(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId), includes: new[] { "PurchasePo.PurchasePoinvoices" })).ToList();
                    var InventoryStoreItemListForInsertDB = new List<InventoryStoreItem>();

                    for (int i = 0; i < Request.InternalTransferOrderItemList.Count; i++)
                    {
                        var InternalTransferDataOBJ = Request.InternalTransferOrderItemList[i];
                        // Check QTY Release From Parent is enough or not

                        List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = InternalTransferDataOBJ.StockBalanceList;// Case 1 : Request ParentReleaseItemID
                                                                                                                                // Calc Parent Release , Final Balance After Release

                        if (InternalTransferDataOBJ.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
                        {
                            ParentReleaseWithQTYListID = _inventoryItemService.GetParentReleaseIDWithSettingStore(InventoryStoreItemList
                                                                                        , InternalTransferDataOBJ.InventoryItemID,
                                                                                        transferOrder.FromInventoryStoreId,
                                                                                        InternalTransferDataOBJ.StoreLocationID,// store location
                                                                                        (decimal)InternalTransferDataOBJ.TransferredQTY,
                                                                                        InternalTransferDataOBJ.IsFIFO);
                        }

                        var InventoryItemObjDB = inventoryItemsList.Where(x => x.Id == InternalTransferDataOBJ.InventoryItemID).FirstOrDefault();
                        if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < InternalTransferDataOBJ.TransferredQTY)
                        {

                            Response.Result = true;
                            Error error = new Error();
                            error.ErrorCode = "Err325";
                            error.ErrorMSG = "Not have availble qty from parent  Item to release on Item " + InventoryItemObjDB.Name + "- NO #" + i + 1; // Warning
                            Response.Errors.Add(error);
                        }

                        var check = _unitOfWork.InventoryInternalTransferOrderItems.Any(a => a.InventoryInternalTransferOrderId == transferOrder.Id && a.InventoryItemId == InternalTransferDataOBJ.InventoryItemID && a.Comments == InternalTransferDataOBJ.Comment && a.TransferredQty == InternalTransferDataOBJ.TransferredQTY);

                        if (InventoryItemObjDB != null && ParentReleaseWithQTYListID.Count() > 0 && check==false)
                        {
                            var TransferItem = new InventoryInternalTransferOrderItem()
                            {
                                InventoryInternalTransferOrderId = transferOrder.Id,
                                InventoryItemId = InternalTransferDataOBJ.InventoryItemID,
                                Uomid = InventoryItemObjDB.RequstionUomid,
                                Comments = InternalTransferDataOBJ.Comment,
                                TransferredQty = InternalTransferDataOBJ.TransferredQTY
                            };
                            await _unitOfWork.InventoryInternalTransferOrderItems.AddAsync(TransferItem);

                            var TransferItemInsertion = await _unitOfWork.CompleteAsync();
                            //add new 

                            if (TransferItemInsertion > 0)
                            {

                                long TransferOrderItemId = TransferItem.Id;
                                decimal RemainReleaseQTY = (decimal)InternalTransferDataOBJ.TransferredQTY;
                                //var inventoryStoreItemsIDs = ParentReleaseWithQTYListID.Select(a => a.ID);
                                //var inventoryStoreItemsList = _unitOfWork.InventoryStoreItems.FindAll(x => inventoryStoreItemsIDs.Contains(x.Id)).ToList();
                                for (int j = 0; j < ParentReleaseWithQTYListID.Count; j++)
                                {
                                    var ObjParentRelease = ParentReleaseWithQTYListID[j];
                                    decimal ReleaseQTY = 0;
                                    if (RemainReleaseQTY <= ObjParentRelease.StockBalance)
                                    {
                                        ReleaseQTY = RemainReleaseQTY;
                                    }
                                    else
                                    {
                                        ReleaseQTY = ObjParentRelease.StockBalance;
                                    }

                                    if (RemainReleaseQTY > 0)
                                    {
                                        RemainReleaseQTY -= ReleaseQTY;
                                        var ParentInventoryStoreItem = InventoryStoreItemList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
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
                                        decimal? remainItemCosetEGPForRelease = null;
                                        decimal? remainItemCostOtherCUForRelease = null;
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
                                            decimal? finalBalance = (ParentInventoryStoreItem.FinalBalance - (ReleaseQTY)) ?? 0;
                                            if (ParentInventoryStoreItem.AddingFromPoid != null)
                                            {

                                                var POItemObjDB = POItemList.Where(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
                                                                                                   && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
                                                                                                    ).FirstOrDefault();
                                                if (POItemObjDB != null)
                                                {
                                                    POInvoiceId = POItemObjDB?.PurchasePo?.PurchasePoinvoices.Where(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid).Select(x => x.Id).FirstOrDefault();
                                                    currencyId = POItemObjDB.CurrencyId ?? 1;
                                                    rateToEGP = POItemObjDB.RateToEgp ?? 1;
                                                    POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                    POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                    POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                    POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                    remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                    remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                    remainItemCosetEGPForRelease = ReleaseQTY * POInvoiceTotalCostEGP ?? 0;
                                                    remainItemCostOtherCUForRelease = ReleaseQTY * POInvoiceTotalCost ?? 0;


                                                }
                                            }
                                        }
                                        var InventoryStoreItemOBJ = new InventoryStoreItem();
                                        InventoryStoreItemOBJ.InventoryStoreId = transferOrder.FromInventoryStoreId;
                                        InventoryStoreItemOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
                                        InventoryStoreItemOBJ.OrderNumber = transferOrder.Id.ToString();
                                        InventoryStoreItemOBJ.OrderId = transferOrder.Id;
                                        InventoryStoreItemOBJ.CreatedBy = creator;
                                        InventoryStoreItemOBJ.ModifiedBy = creator;
                                        InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                        InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                        InventoryStoreItemOBJ.OperationType = "Transfer Order (Released)";
                                        InventoryStoreItemOBJ.Balance = (double)(-ReleaseQTY);
                                        InventoryStoreItemOBJ.Balance1 = (decimal)(-ReleaseQTY);
                                        InventoryStoreItemOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID != null ? InternalTransferDataOBJ.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationId;
                                        InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                        InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                        InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                                        InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                                        InventoryStoreItemOBJ.AddingOrderItemId = TransferOrderItemId;
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
                                        InventoryStoreItemListForInsertDB.Add(InventoryStoreItemOBJ);
                                        if (ParentInventoryStoreItem != null)
                                        {
                                            ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                                            ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                            ParentInventoryStoreItem.ModifiedBy = creator;
                                            ParentInventoryStoreItem.PoinvoiceId = POInvoiceId;
                                            ParentInventoryStoreItem.CurrencyId = currencyId;
                                            ParentInventoryStoreItem.RateToEgp = rateToEGP;
                                            ParentInventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                            ParentInventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                            ParentInventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                            ParentInventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
                                            ParentInventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
                                            ParentInventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
                                            await _unitOfWork.CompleteAsync();
                                            var InventoryStoreItemTransferReceivedOBJ = new InventoryStoreItem();
                                            InventoryStoreItemTransferReceivedOBJ.InventoryStoreId = transferOrder.ToInventoryStoreId;
                                            InventoryStoreItemTransferReceivedOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
                                            InventoryStoreItemTransferReceivedOBJ.OrderNumber = transferOrder.Id.ToString();
                                            InventoryStoreItemTransferReceivedOBJ.OrderId = transferOrder.Id;
                                            InventoryStoreItemTransferReceivedOBJ.CreatedBy = creator;
                                            InventoryStoreItemTransferReceivedOBJ.ModifiedBy = creator;
                                            InventoryStoreItemTransferReceivedOBJ.CreationDate = DateTime.Now;
                                            InventoryStoreItemTransferReceivedOBJ.ModifiedDate = DateTime.Now;
                                            InventoryStoreItemTransferReceivedOBJ.OperationType = "Transfer Order (Received)";
                                            InventoryStoreItemTransferReceivedOBJ.Balance = (double)ReleaseQTY;
                                            InventoryStoreItemTransferReceivedOBJ.Balance1 = (decimal)ReleaseQTY;
                                            InventoryStoreItemTransferReceivedOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID;
                                            InventoryStoreItemTransferReceivedOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                            InventoryStoreItemTransferReceivedOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                            InventoryStoreItemTransferReceivedOBJ.ReleaseParentId = ObjParentRelease.ID;
                                            InventoryStoreItemTransferReceivedOBJ.FinalBalance = ReleaseQTY;
                                            InventoryStoreItemTransferReceivedOBJ.AddingOrderItemId = TransferOrderItemId;
                                            InventoryStoreItemTransferReceivedOBJ.AddingFromPoid = POID;
                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceId = POInvoiceId;
                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
                                            InventoryStoreItemTransferReceivedOBJ.CurrencyId = currencyId;
                                            InventoryStoreItemTransferReceivedOBJ.RateToEgp = rateToEGP;
                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                            InventoryStoreItemTransferReceivedOBJ.RemainItemPrice = remainItemPrice;
                                            InventoryStoreItemTransferReceivedOBJ.RemainItemCosetEgp = remainItemCosetEGP;
                                            InventoryStoreItemTransferReceivedOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
                                            InventoryStoreItemTransferReceivedOBJ.HoldQty = 0;
                                            InventoryStoreItemListForInsertDB.Add(InventoryStoreItemTransferReceivedOBJ);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (InventoryStoreItemListForInsertDB.Count() > 0)
                    {
                        await _unitOfWork.InventoryStoreItems.AddRangeAsync(InventoryStoreItemListForInsertDB);
                        await _unitOfWork.CompleteAsync();
                    }
                    Response.Result = true;
                    Response.ID = InventoryInternalTransferOrderID;
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

                _logService.AddLogError("AddInvnetoryInternalTransferOrder", message, creator, compName, innerExceptionMessage);

                return Response;
            }
        }

        public BaseResponseWithId<long> AddInvnetoryInternalTransferOrder(AddInventoryInternalTransferOrderRequest Request, long creator, string compName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
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

                    int FromInventoryStorID = 0;
                    if (Request.FromInventoryStorID != 0)
                    {
                        FromInventoryStorID = Request.FromInventoryStorID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid From Inventory Store ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    int ToInventoryStorID = 0;
                    if (Request.ToInventoryStorID != 0)
                    {
                        ToInventoryStorID = Request.ToInventoryStorID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid To Inventory Store ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.ToInventoryStorID == Request.FromInventoryStorID)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid From Inventory store and  To Inventory Store must be different.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime RecevingData = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.ReceivingDate) || !DateTime.TryParse(Request.ReceivingDate, out RecevingData))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err23";
                        error.ErrorMSG = "Invalid Receving Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalTransferOrderItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalTransferOrderItemList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var ItemTotalCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Count();
                    var ItemDistinctCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Distinct().Count();
                    if (ItemDistinctCount < ItemTotalCount)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "There is item itteration with same data Location and Serial";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int Counter = 0;

                    var inventoryItemsIDs = Request.InternalTransferOrderItemList.Select(a => a.InventoryItemID).ToList();
                    var inventoryItemsList = _unitOfWork.InventoryItems.FindAll(a => inventoryItemsIDs.Contains(a.Id)).ToList();

                    var InventoryStoreLocationsIDs = Request.InternalTransferOrderItemList.Select(a => a.StoreLocationID).ToList();
                    var InventoryStoreLocationsList = _unitOfWork.InventoryStoreLocations.FindAll(a => InventoryStoreLocationsIDs.Contains(a.Id));

                    var CheckToInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.Active == true && x.Approved == false && x.Closed == false && x.InventoryStoreId == ToInventoryStorID).ToList();

                    foreach (var item in Request.InternalTransferOrderItemList)
                    {
                        Counter++;
                        var itemData = inventoryItemsList.Where(a => a.Id == item.InventoryItemID).FirstOrDefault();
                        if (itemData == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err347";
                            error.ErrorMSG = "Invalid Inventory Item ID at Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        var location = InventoryStoreLocationsList.Where(a => a.Id == item.StoreLocationID).FirstOrDefault();
                        if (item.StoreLocationID != null && location == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err347";
                            error.ErrorMSG = "Invalid Inventory Store Location Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.InventoryItemID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.TransferredQTY < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err29";
                            error.ErrorMSG = "Invalid QTY item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if ((item.StockBalanceList == null || item.StockBalanceList.Count < 1) && item.IsFIFO == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-416";
                            error.ErrorMSG = "you must select  StockBalanceList or Setting IsFIFO on item NO#" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.StockBalanceList != null)
                        {
                            if (item.StockQTY != null)
                            {
                                if (item.TransferredQTY > item.StockQTY)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-416";
                                    error.ErrorMSG = "Parent Release Item selected Not have balance enough on Item NO#" + Counter;
                                    Response.Errors.Add(error);
                                }
                            }

                        }
                    }

                    long InventoryInternalTransferOrderID = 0;
                    if (Response.Result)
                    {
                        // Check Inventory Report Approved and closed or not
                        if (CheckToInventoryReportListDB.Count > 0)
                        {
                            string storeName = _unitOfWork.InventoryStores.GetById(ToInventoryStorID)?.Name;
                            foreach (var InventoryRep in CheckToInventoryReportListDB)
                            {
                                if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                {
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
                            // Insertion Internal Transfer Order
                            var InventoryAddingOrder = new InventoryInternalTransferOrder()
                            {
                                OperationType = "Add New Transfer",
                                Revision = 0,
                                FromInventoryStoreId = FromInventoryStorID,
                                ToInventoryStoreId = ToInventoryStorID,
                                RecivingDate = RecevingData,
                                CreatedBy = creator,
                                ModifiedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                            };

                            _unitOfWork.InventoryInternalTransferOrders.Add(InventoryAddingOrder);
                            var InventoryAddingOrderInsertion = _unitOfWork.Complete();

                            if (InventoryAddingOrderInsertion > 0)
                            {
                                InventoryInternalTransferOrderID = InventoryAddingOrder.Id;
                                #region items

                                int itemCount = 0;
                                // Get All InventoryStoreItems Used in this Release before loop
                                var IDSInventoryItemListRequested = Request.InternalTransferOrderItemList.Select(x => x.InventoryItemID).Distinct().ToList();
                                var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId)).ToList();
                                foreach (var InternalTransferDataOBJ in Request.InternalTransferOrderItemList)
                                {
                                    itemCount++;

                                    // Check QTY Release From Parent is enough or not

                                    List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = InternalTransferDataOBJ.StockBalanceList;// Case 1 : Request ParentReleaseItemID
                                                                                                                                            // Calc Parent Release , Final Balance After Release

                                    if (InternalTransferDataOBJ.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
                                    {
                                        ParentReleaseWithQTYListID = _inventoryItemService.GetParentReleaseIDWithSettingStore(InventoryStoreItemList
                                                                                                    , InternalTransferDataOBJ.InventoryItemID,
                                                                                                    FromInventoryStorID,
                                                                                                    InternalTransferDataOBJ.StoreLocationID,// store location
                                                                                                    (decimal)InternalTransferDataOBJ.TransferredQTY,
                                                                                                    InternalTransferDataOBJ.IsFIFO);
                                    }

                                    var InventoryItemObjDB = inventoryItemsList.Where(x => x.Id == InternalTransferDataOBJ.InventoryItemID).FirstOrDefault();
                                    if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < InternalTransferDataOBJ.TransferredQTY)
                                    {

                                        Response.Result = true;
                                        Error error = new Error();
                                        error.ErrorCode = "Err325";
                                        error.ErrorMSG = "Not have availble qty from parent  Item to release on Item " + InventoryItemObjDB.Name + "- NO #" + itemCount; // Warning
                                        Response.Errors.Add(error);
                                    }



                                    if (InventoryItemObjDB != null && ParentReleaseWithQTYListID.Count() > 0)
                                    {
                                        var TransferItem = new InventoryInternalTransferOrderItem()
                                        {
                                            InventoryInternalTransferOrderId = InventoryInternalTransferOrderID,
                                            InventoryItemId = InternalTransferDataOBJ.InventoryItemID,
                                            Uomid = InventoryItemObjDB.RequstionUomid,
                                            Comments = InternalTransferDataOBJ.Comment,
                                            TransferredQty = InternalTransferDataOBJ.TransferredQTY
                                        };
                                        _unitOfWork.InventoryInternalTransferOrderItems.Add(TransferItem);

                                        var TransferItemInsertion = _unitOfWork.Complete();
                                        //add new 

                                        if (TransferItemInsertion > 0)
                                        {

                                            long TransferOrderItemId = TransferItem.Id;
                                            decimal RemainReleaseQTY = (decimal)InternalTransferDataOBJ.TransferredQTY;
                                            var inventoryStoreItemsIDs = ParentReleaseWithQTYListID.Select(a => a.ID);
                                            var inventoryStoreItemsList = _unitOfWork.InventoryStoreItems.FindAll(x => inventoryStoreItemsIDs.Contains(x.Id)).ToList();

                                            foreach (var ObjParentRelease in ParentReleaseWithQTYListID)
                                            {
                                                decimal ReleaseQTY = 0;
                                                if (RemainReleaseQTY <= ObjParentRelease.StockBalance)
                                                {
                                                    ReleaseQTY = RemainReleaseQTY;
                                                }
                                                else
                                                {
                                                    ReleaseQTY = ObjParentRelease.StockBalance;
                                                }

                                                if (RemainReleaseQTY > 0)
                                                {
                                                    RemainReleaseQTY -= ReleaseQTY;
                                                    var ParentInventoryStoreItem = inventoryStoreItemsList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
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
                                                    decimal? remainItemCosetEGPForRelease = null;
                                                    decimal? remainItemCostOtherCUForRelease = null;
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
                                                        decimal? finalBalance = (ParentInventoryStoreItem.FinalBalance - (ReleaseQTY)) ?? 0;
                                                        if (ParentInventoryStoreItem.AddingFromPoid != null)
                                                        {
                                                            var POItemObjDB = _unitOfWork.VPurchasePoItems.FindAll(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
                                                                                                               && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
                                                                                                                ).FirstOrDefault();
                                                            if (POItemObjDB != null)
                                                            {
                                                                POInvoiceId = _unitOfWork.PurchasePOInvoices.FindAll(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid).Select(x => x.Id).FirstOrDefault();
                                                                currencyId = POItemObjDB.CurrencyId ?? 1;
                                                                rateToEGP = POItemObjDB.RateToEgp ?? 1;
                                                                POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                                POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                                remainItemCosetEGPForRelease = ReleaseQTY * POInvoiceTotalCostEGP ?? 0;
                                                                remainItemCostOtherCUForRelease = ReleaseQTY * POInvoiceTotalCost ?? 0;


                                                            }
                                                        }
                                                    }
                                                    var InventoryStoreItemOBJ = new InventoryStoreItem();
                                                    InventoryStoreItemOBJ.InventoryStoreId = FromInventoryStorID;
                                                    InventoryStoreItemOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
                                                    InventoryStoreItemOBJ.OrderNumber = InventoryInternalTransferOrderID.ToString();
                                                    InventoryStoreItemOBJ.OrderId = InventoryInternalTransferOrderID;
                                                    InventoryStoreItemOBJ.CreatedBy = creator;
                                                    InventoryStoreItemOBJ.ModifiedBy = creator;
                                                    InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                    InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                    InventoryStoreItemOBJ.OperationType = "Transfer Order (Released)";
                                                    InventoryStoreItemOBJ.Balance = (double)(-ReleaseQTY);
                                                    InventoryStoreItemOBJ.Balance1 = (decimal)(-ReleaseQTY);
                                                    InventoryStoreItemOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID != null ? InternalTransferDataOBJ.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationId;
                                                    InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                                    InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                                    InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                                                    InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                                                    InventoryStoreItemOBJ.AddingOrderItemId = TransferOrderItemId;
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
                                                    _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                                    var InventoryStorItemInsertion = _unitOfWork.Complete();
                                                    if (InventoryStorItemInsertion > 0)
                                                    {
                                                        if (ParentInventoryStoreItem != null)
                                                        {
                                                            ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                                                            ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                            ParentInventoryStoreItem.ModifiedBy = creator;
                                                            ParentInventoryStoreItem.PoinvoiceId = POInvoiceId;
                                                            ParentInventoryStoreItem.CurrencyId = currencyId;
                                                            ParentInventoryStoreItem.RateToEgp = rateToEGP;
                                                            ParentInventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                            ParentInventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                            ParentInventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                            ParentInventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                            ParentInventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
                                                            ParentInventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
                                                            _unitOfWork.Complete();
                                                            var InventoryStoreItemTransferReceivedOBJ = new InventoryStoreItem();
                                                            InventoryStoreItemTransferReceivedOBJ.InventoryStoreId = ToInventoryStorID;
                                                            InventoryStoreItemTransferReceivedOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
                                                            InventoryStoreItemTransferReceivedOBJ.OrderNumber = InventoryInternalTransferOrderID.ToString();
                                                            InventoryStoreItemTransferReceivedOBJ.OrderId = InventoryInternalTransferOrderID;
                                                            InventoryStoreItemTransferReceivedOBJ.CreatedBy = creator;
                                                            InventoryStoreItemTransferReceivedOBJ.ModifiedBy = creator;
                                                            InventoryStoreItemTransferReceivedOBJ.CreationDate = DateTime.Now;
                                                            InventoryStoreItemTransferReceivedOBJ.ModifiedDate = DateTime.Now;
                                                            InventoryStoreItemTransferReceivedOBJ.OperationType = "Transfer Order (Received)";
                                                            InventoryStoreItemTransferReceivedOBJ.Balance = (double)ReleaseQTY;
                                                            InventoryStoreItemTransferReceivedOBJ.Balance1 = (decimal)ReleaseQTY;
                                                            InventoryStoreItemTransferReceivedOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID;
                                                            InventoryStoreItemTransferReceivedOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                                            InventoryStoreItemTransferReceivedOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                                            InventoryStoreItemTransferReceivedOBJ.ReleaseParentId = ObjParentRelease.ID;
                                                            InventoryStoreItemTransferReceivedOBJ.FinalBalance = ReleaseQTY;
                                                            InventoryStoreItemTransferReceivedOBJ.AddingOrderItemId = TransferOrderItemId;
                                                            InventoryStoreItemTransferReceivedOBJ.AddingFromPoid = POID;
                                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceId = POInvoiceId;
                                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                            InventoryStoreItemTransferReceivedOBJ.CurrencyId = currencyId;
                                                            InventoryStoreItemTransferReceivedOBJ.RateToEgp = rateToEGP;
                                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                            InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                            InventoryStoreItemTransferReceivedOBJ.RemainItemPrice = remainItemPrice;
                                                            InventoryStoreItemTransferReceivedOBJ.RemainItemCosetEgp = remainItemCosetEGP;
                                                            InventoryStoreItemTransferReceivedOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
                                                            InventoryStoreItemTransferReceivedOBJ.HoldQty = 0;
                                                            _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemTransferReceivedOBJ);
                                                            var InventoryStorItemTransferReceivedInsertion = _unitOfWork.Complete();
                                                            var ListInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemObjDB.Id && x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
                                                            var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
                                                            var ListIDSPOInvoicesIsFulllyPriced = _unitOfWork.PurchasePOInvoices.FindAll(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
                                                            ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
                                                            InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;
                                                            InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
                                                            InventoryStoreItemTransferReceivedOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
                                                            _unitOfWork.Complete();
                                                        }
                                                    }

                                                }
                                            }
                                        }
                                    }
                                }

                                #endregion items
                                Response.Result = true;
                                Response.ID = InventoryInternalTransferOrderID;
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

                _logService.AddLogError("AddInvnetoryInternalTransferOrder", message, creator, compName, innerExceptionMessage);

                return Response;
            }
        }


        public async Task<BaseResponseWithId<long>> AddInvnetoryInternalTransferOrder2(AddInventoryInternalTransferOrderRequest Request, long creator, string compName)
        {
            var Response = new BaseResponseWithId<long> { Result = true, Errors = new List<Error>() };

            try
            {
                if (Request == null)
                    return AddError(Response, "Err-P12", "Please insert valid data.");

                if (Request.FromInventoryStorID == 0 || Request.ToInventoryStorID == 0)
                    return AddError(Response, "Err24", "Invalid From/To Inventory Store ID.");

                if (Request.ToInventoryStorID == Request.FromInventoryStorID)
                    return AddError(Response, "Err24", "From and To Inventory Stores must be different.");

                if (!DateTime.TryParse(Request.ReceivingDate, out DateTime ReceivingDate))
                    return AddError(Response, "Err23", "Invalid Receiving Date.");

                if (Request.InternalTransferOrderItemList == null || !Request.InternalTransferOrderItemList.Any())
                    return AddError(Response, "Err-14", "Please insert at least one Item.");

                // Fetch necessary data
                var inventoryItemIds = Request.InternalTransferOrderItemList.Select(x => x.InventoryItemID).Distinct().ToHashSet();
                var inventoryItems = (await _unitOfWork.InventoryItems.FindAllAsync(x => inventoryItemIds.Contains(x.Id)))
                                     .ToDictionary(x => x.Id);

                var activeInventoryReports = await _unitOfWork.InventoryReports.FindAllAsync(
                    x => x.Active && !x.Approved && !x.Closed && x.InventoryStoreId == Request.ToInventoryStorID
                );

                // Check if the store is under an inventory process
                var activeInventoryReport = activeInventoryReports.FirstOrDefault(r => r.DateFrom <= DateTime.Now && r.DateTo >= DateTime.Now);
                if (activeInventoryReport != null)
                {
                    string storeName = (await _unitOfWork.InventoryStores.GetByIdAsync(Request.ToInventoryStorID))?.Name;
                    return AddError(Response, "Err-44", $"Store {storeName} is under inventory from {activeInventoryReport.DateFrom:dd-MM-yyyy} to {activeInventoryReport.DateTo:dd-MM-yyyy}");
                }

                // Insert Internal Transfer Order
                var inventoryTransferOrder = new InventoryInternalTransferOrder
                {
                    OperationType = "Add New Transfer",
                    Revision = 0,
                    FromInventoryStoreId = Request.FromInventoryStorID,
                    ToInventoryStoreId = Request.ToInventoryStorID,
                    RecivingDate = ReceivingDate,
                    CreatedBy = creator,
                    ModifiedBy = creator,
                    CreationDate = DateTime.Now,
                    ModifiedDate = DateTime.Now
                };

                await _unitOfWork.InventoryInternalTransferOrders.AddAsync(inventoryTransferOrder);
                await _unitOfWork.CompleteAsync();

                if (inventoryTransferOrder.Id == 0)
                    return AddError(Response, "Err-500", "Failed to create inventory transfer order.");

                long transferOrderId = inventoryTransferOrder.Id;

                // Process Transfer Order Items
                var transferItems = Request.InternalTransferOrderItemList.Select(item => new InventoryInternalTransferOrderItem
                {
                    InventoryInternalTransferOrderId = transferOrderId,
                    InventoryItemId = item.InventoryItemID,
                    Uomid = inventoryItems[item.InventoryItemID].RequstionUomid,
                    Comments = item.Comment,
                    TransferredQty = item.TransferredQTY
                }).ToList();

                await _unitOfWork.InventoryInternalTransferOrderItems.AddRangeAsync(transferItems);
                await _unitOfWork.CompleteAsync();

                // Process Inventory Movements
                var inventoryMovements = new List<InventoryStoreItem>();
                DateTime currentTime = DateTime.Now;

                foreach (var item in Request.InternalTransferOrderItemList)
                {
                    var inventoryItem = inventoryItems[item.InventoryItemID];

                    // Create Released Entry (From Store)
                    inventoryMovements.Add(new InventoryStoreItem
                    {
                        InventoryStoreId = Request.FromInventoryStorID,
                        InventoryItemId = item.InventoryItemID,
                        OrderNumber = transferOrderId.ToString(),
                        OrderId = transferOrderId,
                        CreatedBy = creator,
                        ModifiedBy = creator,
                        CreationDate = currentTime,
                        ModifiedDate = currentTime,
                        OperationType = "Transfer Order (Released)",
                        Balance = (double)(-item.TransferredQTY),
                        Balance1 = -item.TransferredQTY,
                        FinalBalance = -item.TransferredQTY,
                        ExpDate = null,  // Add expiration date logic if needed
                        ItemSerial = null,  // Add serial logic if needed
                        HoldQty = 0
                    });

                    // Create Received Entry (To Store)
                    inventoryMovements.Add(new InventoryStoreItem
                    {
                        InventoryStoreId = Request.ToInventoryStorID,
                        InventoryItemId = item.InventoryItemID,
                        OrderNumber = transferOrderId.ToString(),
                        OrderId = transferOrderId,
                        CreatedBy = creator,
                        ModifiedBy = creator,
                        CreationDate = currentTime,
                        ModifiedDate = currentTime,
                        OperationType = "Transfer Order (Received)",
                        Balance = (double)item.TransferredQTY,
                        Balance1 = item.TransferredQTY,
                        FinalBalance = item.TransferredQTY,
                        ExpDate = null,  // Add expiration date logic if needed
                        ItemSerial = null,  // Add serial logic if needed
                        HoldQty = 0
                    });
                }

                await _unitOfWork.InventoryStoreItems.AddRangeAsync(inventoryMovements);
                await _unitOfWork.CompleteAsync();

                Response.Result = true;
                Response.ID = transferOrderId;
                return Response;
            }
            catch (Exception ex)
            {
                return await HandleExceptionAsync(Response, ex, creator, compName);
            }
        }

        // Helper Methods
        public BaseResponseWithId<long> AddError(BaseResponseWithId<long> response, string errorCode, string errorMessage)
        {
            response.Result = false;
            response.Errors.Add(new Error { ErrorCode = errorCode, ErrorMSG = errorMessage });
            return response;
        }

        public async Task<BaseResponseWithId<long>> HandleExceptionAsync(BaseResponseWithId<long> response, Exception ex, long creator, string compName)
        {
            response.Result = false;
            response.Errors.Add(new Error
            {
                ErrorCode = "Err10",
                ErrorMSG = ex.InnerException?.Message ?? ex.Message
            });

            _logService.AddLogError("AddInventoryInternalTransferOrder", ex.Message, creator, compName, ex.InnerException?.Message);
            return response;
        }








        //public BaseResponseWithId<long> AddInvnetoryInternalTransferOrderV2(AddInventoryInternalTransferOrderRequest Request, long creator, string compName)
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
        //                error.ErrorMSG = "please insert a valid data.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            int FromInventoryStorID = 0;
        //            if (Request.FromInventoryStorID != 0)
        //            {
        //                FromInventoryStorID = Request.FromInventoryStorID;
        //            }
        //            else
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "Invalid From Inventory Store ID.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            int ToInventoryStorID = 0;
        //            if (Request.ToInventoryStorID != 0)
        //            {
        //                ToInventoryStorID = Request.ToInventoryStorID;
        //            }
        //            else
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "Invalid To Inventory Store ID.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            if (Request.ToInventoryStorID == Request.FromInventoryStorID)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "Invalid From Inventory store and  To Inventory Store must be different.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            DateTime RecevingData = DateTime.Now;
        //            if (string.IsNullOrEmpty(Request.ReceivingDate) || !DateTime.TryParse(Request.ReceivingDate, out RecevingData))
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err23";
        //                error.ErrorMSG = "Invalid Receving Date.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            if (Request.InternalTransferOrderItemList == null)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err-14";
        //                error.ErrorMSG = "please insert at least one Item.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            if (Request.InternalTransferOrderItemList.Count() == 0)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err-14";
        //                error.ErrorMSG = "please insert at least one Item.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            var ItemTotalCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Count();
        //            var ItemDistinctCount = Request.InternalTransferOrderItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Distinct().Count();
        //            if (ItemDistinctCount < ItemTotalCount)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err27";
        //                error.ErrorMSG = "There is item itteration with same data Location and Serial";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            int Counter = 0;
        //            var inventoryItemsIDs = Request.InternalTransferOrderItemList.Select(a => a.InventoryItemID).ToList();
        //            var inventoryItemsList = _unitOfWork.InventoryItems.FindAll(a => inventoryItemsIDs.Contains(a.Id)).ToList();
        //            var InventoryStoreLocationsIDs = Request.InternalTransferOrderItemList.Select(a => a.StoreLocationID).ToList();
        //            var InventoryStoreLocationsList = _unitOfWork.InventoryStoreLocations.FindAll(a => InventoryStoreLocationsIDs.Contains(a.Id));
        //            var CheckToInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.Active == true && x.Approved == false && x.Closed == false && x.InventoryStoreId == ToInventoryStorID).ToList();
        //            foreach (var item in Request.InternalTransferOrderItemList)
        //            {
        //                Counter++;
        //                //var itemData = _unitOfWork.InventoryItems.GetById(item.InventoryItemID);

        //                var itemData = inventoryItemsList.Where(a => a.Id == item.InventoryItemID).FirstOrDefault();
        //                if (itemData == null)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err347";
        //                    error.ErrorMSG = "Invalid Inventory Item ID at Selected item #" + Counter;
        //                    Response.Errors.Add(error);
        //                }
        //                var location = InventoryStoreLocationsList.Where(a => a.Id == item.StoreLocationID).FirstOrDefault();
        //                if (item.StoreLocationID != null && location == null)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err347";
        //                    error.ErrorMSG = "Invalid Inventory Store Location Selected item #" + Counter;
        //                    Response.Errors.Add(error);
        //                }
        //                if (item.InventoryItemID < 0)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err27";
        //                    error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
        //                    Response.Errors.Add(error);
        //                }
        //                if (item.TransferredQTY < 0)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err29";
        //                    error.ErrorMSG = "Invalid QTY item #" + Counter;
        //                    Response.Errors.Add(error);
        //                }
        //                if ((item.StockBalanceList == null || item.StockBalanceList.Count < 1) && item.IsFIFO == null)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err-416";
        //                    error.ErrorMSG = "you must select  StockBalanceList or Setting IsFIFO on item NO#" + Counter;
        //                    Response.Errors.Add(error);
        //                }
        //                if (item.StockBalanceList != null)
        //                {
        //                    if (item.StockQTY != null)
        //                    {
        //                        if (item.TransferredQTY > item.StockQTY)
        //                        {
        //                            Response.Result = false;
        //                            Error error = new Error();
        //                            error.ErrorCode = "Err-416";
        //                            error.ErrorMSG = "Parent Release Item selected Not have balance enough on Item NO#" + Counter;
        //                            Response.Errors.Add(error);
        //                        }
        //                    }
        //                }
        //            }
        //            long InventoryInternalTransferOrderID = 0;
        //            if (Response.Result)
        //            {
        //                // Check Inventory Report Approved and closed or not
        //                if (CheckToInventoryReportListDB.Count > 0)
        //                {
        //                    foreach (var InventoryRep in CheckToInventoryReportListDB)
        //                    {
        //                        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
        //                        {
        //                            string storeName = _unitOfWork.InventoryStores.GetById(ToInventoryStorID)?.Name;
        //                            string errMsg = "Store " + storeName +
        //                                " is under inventory from " +
        //                                InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
        //                                " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

        //                            Response.Result = false;
        //                            Error error = new Error();
        //                            error.ErrorCode = "Err-44";
        //                            error.ErrorMSG = errMsg;
        //                            Response.Errors.Add(error);
        //                        }
        //                    }
        //                }
        //                if (Response.Result)
        //                {
        //                    // Insertion Internal Transfer Order
        //                    var InventoryAddingOrder = new InventoryInternalTransferOrder()
        //                    {
        //                        OperationType = "Add New Transfer",
        //                        Revision = 0,
        //                        FromInventoryStoreId = FromInventoryStorID,
        //                        ToInventoryStoreId = ToInventoryStorID,
        //                        RecivingDate = RecevingData,
        //                        CreatedBy = creator,
        //                        ModifiedBy = creator,
        //                        CreationDate = DateTime.Now,
        //                        ModifiedDate = DateTime.Now,
        //                    };
        //                    _unitOfWork.InventoryInternalTransferOrders.Add(InventoryAddingOrder);
        //                    var InventoryAddingOrderInsertion = _unitOfWork.Complete();

        //                    if (InventoryAddingOrderInsertion > 0)
        //                    {
        //                        InventoryInternalTransferOrderID = InventoryAddingOrder.Id;
        //                        #region items
        //                        int itemCount = 0;
        //                        // Get All InventoryStoreItems Used in this Release before loop
        //                        var IDSInventoryItemListRequested = Request.InternalTransferOrderItemList.Select(x => x.InventoryItemID).Distinct().ToList();
        //                        var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId)).ToList();
        //                        foreach (var InternalTransferDataOBJ in Request.InternalTransferOrderItemList)
        //                        {
        //                            itemCount++;
        //                            // Check QTY Release From Parent is enough or not
        //                            List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = InternalTransferDataOBJ.StockBalanceList;// Case 1 : Request ParentReleaseItemID
        //                                                                                                                                    // Calc Parent Release , Final Balance After Release

        //                            if (InternalTransferDataOBJ.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
        //                            {
        //                                ParentReleaseWithQTYListID = _inventoryItemService.GetParentReleaseIDWithSettingStore(InventoryStoreItemList
        //                                                                                            , InternalTransferDataOBJ.InventoryItemID,
        //                                                                                            FromInventoryStorID,
        //                                                                                            InternalTransferDataOBJ.StoreLocationID,// store location
        //                                                                                            (decimal)InternalTransferDataOBJ.TransferredQTY,
        //                                                                                            InternalTransferDataOBJ.IsFIFO);
        //                            }
        //                            var InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == InternalTransferDataOBJ.InventoryItemID).FirstOrDefault();
        //                            if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < InternalTransferDataOBJ.TransferredQTY)
        //                            {

        //                                Response.Result = true;
        //                                Error error = new Error();
        //                                error.ErrorCode = "Err325";
        //                                error.ErrorMSG = "Not have availble qty from parent  Item to release on Item " + InventoryItemObjDB.Name + "- NO #" + itemCount; // Warning
        //                                Response.Errors.Add(error);
        //                            }
        //                            if (InventoryItemObjDB != null && ParentReleaseWithQTYListID.Count() > 0)
        //                            {
        //                                var TransferItem = new InventoryInternalTransferOrderItem()
        //                                {
        //                                    InventoryInternalTransferOrderId = InventoryInternalTransferOrderID,
        //                                    InventoryItemId = InternalTransferDataOBJ.InventoryItemID,
        //                                    Uomid = InventoryItemObjDB.RequstionUomid,
        //                                    Comments = InternalTransferDataOBJ.Comment,
        //                                    TransferredQty = InternalTransferDataOBJ.TransferredQTY
        //                                };
        //                                _unitOfWork.InventoryInternalTransferOrderItems.Add(TransferItem);
        //                                var TransferItemInsertion = _unitOfWork.Complete();
        //                                //add new 
        //                                if (TransferItemInsertion > 0)
        //                                {
        //                                    long TransferOrderItemId = TransferItem.Id;
        //                                    // List Of Items To remove from Recived QTY
        //                                    decimal RemainReleaseQTY = (decimal)InternalTransferDataOBJ.TransferredQTY;
        //                                    var inventoryStoreItemsIDs = ParentReleaseWithQTYListID.Select(a => a.ID);
        //                                    var inventoryStoreItemsList = _unitOfWork.InventoryStoreItems.FindAll(x => inventoryStoreItemsIDs.Contains(x.Id)).ToList();

        //                                    foreach (var ObjParentRelease in ParentReleaseWithQTYListID)
        //                                    {
        //                                        decimal ReleaseQTY = 0;
        //                                        if (RemainReleaseQTY <= ObjParentRelease.StockBalance)
        //                                        {
        //                                            ReleaseQTY = RemainReleaseQTY;
        //                                        }
        //                                        else
        //                                        {
        //                                            // RemainReleaseQTY -= ObjParentRelease.StockBalance;
        //                                            ReleaseQTY = ObjParentRelease.StockBalance;
        //                                        }
        //                                        if (RemainReleaseQTY > 0)
        //                                        {
        //                                            RemainReleaseQTY -= ReleaseQTY;
        //                                            var ParentInventoryStoreItem = inventoryStoreItemsList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
        //                                            long? POID = null;
        //                                            long? POInvoiceId = null;
        //                                            decimal? POInvoiceTotalPrice = null;
        //                                            decimal? POInvoiceTotalCost = null;
        //                                            int? currencyId = null;
        //                                            decimal? rateToEGP = null;
        //                                            decimal? POInvoiceTotalPriceEGP = null;
        //                                            decimal? POInvoiceTotalCostEGP = null;
        //                                            decimal? remainItemPrice = null;
        //                                            decimal? remainItemCosetEGP = null;
        //                                            decimal? remainItemCostOtherCU = null;
        //                                            decimal? remainItemCosetEGPForRelease = null;
        //                                            decimal? remainItemCostOtherCUForRelease = null;
        //                                            if (ParentInventoryStoreItem != null)
        //                                            {
        //                                                POID = ParentInventoryStoreItem.AddingFromPoid;
        //                                                POInvoiceId = ParentInventoryStoreItem.PoinvoiceId;
        //                                                POInvoiceTotalPrice = ParentInventoryStoreItem.PoinvoiceTotalPrice;
        //                                                POInvoiceTotalCost = ParentInventoryStoreItem.PoinvoiceTotalCost;
        //                                                currencyId = ParentInventoryStoreItem.CurrencyId;
        //                                                rateToEGP = ParentInventoryStoreItem.RateToEgp;
        //                                                POInvoiceTotalPriceEGP = ParentInventoryStoreItem.PoinvoiceTotalPriceEgp;
        //                                                POInvoiceTotalCostEGP = ParentInventoryStoreItem.PoinvoiceTotalCostEgp;
        //                                                remainItemPrice = ParentInventoryStoreItem.RemainItemPrice;
        //                                                remainItemCosetEGP = ParentInventoryStoreItem.RemainItemCosetEgp;
        //                                                remainItemCostOtherCU = ParentInventoryStoreItem.RemainItemCostOtherCu;
        //                                                // Update PO Item Columns
        //                                                // Check if Not call PO Item On Parent 
        //                                                decimal? finalBalance = (ParentInventoryStoreItem.FinalBalance - (ReleaseQTY)) ?? 0;
        //                                                if (ParentInventoryStoreItem.AddingFromPoid != null)
        //                                                {
        //                                                    var POItemObjDB = _unitOfWork.VPurchasePoItems.FindAll(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
        //                                                                                                       && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
        //                                                                                                        ).FirstOrDefault();
        //                                                    if (POItemObjDB != null)
        //                                                    {
        //                                                        POInvoiceId = _unitOfWork.PurchasePOInvoices.FindAll(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid).Select(x => x.Id).FirstOrDefault();
        //                                                        currencyId = POItemObjDB.CurrencyId ?? 1;
        //                                                        rateToEGP = POItemObjDB.RateToEgp ?? 1;
        //                                                        POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
        //                                                        POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
        //                                                        POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
        //                                                        POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
        //                                                        remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
        //                                                        remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

        //                                                        remainItemCosetEGPForRelease = ReleaseQTY * POInvoiceTotalCostEGP ?? 0;
        //                                                        remainItemCostOtherCUForRelease = ReleaseQTY * POInvoiceTotalCost ?? 0;
        //                                                    }
        //                                                }
        //                                            }
        //                                            var InventoryStoreItemOBJ = new InventoryStoreItem();
        //                                            InventoryStoreItemOBJ.InventoryStoreId = FromInventoryStorID;
        //                                            InventoryStoreItemOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
        //                                            InventoryStoreItemOBJ.OrderNumber = InventoryInternalTransferOrderID.ToString();
        //                                            InventoryStoreItemOBJ.OrderId = InventoryInternalTransferOrderID;
        //                                            InventoryStoreItemOBJ.CreatedBy = creator;
        //                                            InventoryStoreItemOBJ.ModifiedBy = creator;
        //                                            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
        //                                            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
        //                                            InventoryStoreItemOBJ.OperationType = "Transfer Order (Released)";
        //                                            InventoryStoreItemOBJ.Balance = (double)(-ReleaseQTY);
        //                                            InventoryStoreItemOBJ.Balance1 = (decimal)(-ReleaseQTY);
        //                                            InventoryStoreItemOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID != null ? InternalTransferDataOBJ.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationId;
        //                                            InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
        //                                            InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
        //                                            InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
        //                                            InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
        //                                            InventoryStoreItemOBJ.AddingOrderItemId = TransferOrderItemId;
        //                                            InventoryStoreItemOBJ.AddingFromPoid = POID;
        //                                            InventoryStoreItemOBJ.PoinvoiceId = POInvoiceId;
        //                                            InventoryStoreItemOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
        //                                            InventoryStoreItemOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
        //                                            InventoryStoreItemOBJ.CurrencyId = currencyId;
        //                                            InventoryStoreItemOBJ.RateToEgp = rateToEGP;
        //                                            InventoryStoreItemOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
        //                                            InventoryStoreItemOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
        //                                            InventoryStoreItemOBJ.RemainItemPrice = remainItemPrice;
        //                                            InventoryStoreItemOBJ.RemainItemCosetEgp = remainItemCosetEGP;
        //                                            InventoryStoreItemOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
        //                                            InventoryStoreItemOBJ.HoldQty = 0;
        //                                            _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
        //                                            var InventoryStorItemInsertion = _unitOfWork.Complete();
        //                                            if (InventoryStorItemInsertion > 0)
        //                                            {
        //                                                // Update Parent Release on InventoryStoreItem
        //                                                if (ParentInventoryStoreItem != null)
        //                                                {
        //                                                    ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
        //                                                    ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
        //                                                    ParentInventoryStoreItem.ModifiedBy = creator;
        //                                                    // Update PO Item Columns
        //                                                    ParentInventoryStoreItem.PoinvoiceId = POInvoiceId;
        //                                                    ParentInventoryStoreItem.CurrencyId = currencyId;
        //                                                    ParentInventoryStoreItem.RateToEgp = rateToEGP;
        //                                                    ParentInventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
        //                                                    ParentInventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
        //                                                    ParentInventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
        //                                                    ParentInventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
        //                                                    ParentInventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
        //                                                    ParentInventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
        //                                                    _unitOfWork.Complete();

        //                                                    var InventoryStoreItemTransferReceivedOBJ = new InventoryStoreItem();
        //                                                    InventoryStoreItemTransferReceivedOBJ.InventoryStoreId = ToInventoryStorID;
        //                                                    InventoryStoreItemTransferReceivedOBJ.InventoryItemId = InternalTransferDataOBJ.InventoryItemID;
        //                                                    InventoryStoreItemTransferReceivedOBJ.OrderNumber = InventoryInternalTransferOrderID.ToString();
        //                                                    InventoryStoreItemTransferReceivedOBJ.OrderId = InventoryInternalTransferOrderID;
        //                                                    InventoryStoreItemTransferReceivedOBJ.CreatedBy = creator;
        //                                                    InventoryStoreItemTransferReceivedOBJ.ModifiedBy = creator;
        //                                                    InventoryStoreItemTransferReceivedOBJ.CreationDate = DateTime.Now;
        //                                                    InventoryStoreItemTransferReceivedOBJ.ModifiedDate = DateTime.Now;
        //                                                    InventoryStoreItemTransferReceivedOBJ.OperationType = "Transfer Order (Received)";
        //                                                    InventoryStoreItemTransferReceivedOBJ.Balance = (double)ReleaseQTY;
        //                                                    InventoryStoreItemTransferReceivedOBJ.Balance1 = (decimal)ReleaseQTY;
        //                                                    InventoryStoreItemTransferReceivedOBJ.InvenoryStoreLocationId = InternalTransferDataOBJ.StoreLocationID;
        //                                                    InventoryStoreItemTransferReceivedOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
        //                                                    InventoryStoreItemTransferReceivedOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
        //                                                    InventoryStoreItemTransferReceivedOBJ.ReleaseParentId = ObjParentRelease.ID;
        //                                                    InventoryStoreItemTransferReceivedOBJ.FinalBalance = ReleaseQTY;
        //                                                    InventoryStoreItemTransferReceivedOBJ.AddingOrderItemId = TransferOrderItemId;
        //                                                    InventoryStoreItemTransferReceivedOBJ.AddingFromPoid = POID;
        //                                                    InventoryStoreItemTransferReceivedOBJ.PoinvoiceId = POInvoiceId;
        //                                                    InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
        //                                                    InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
        //                                                    InventoryStoreItemTransferReceivedOBJ.CurrencyId = currencyId;
        //                                                    InventoryStoreItemTransferReceivedOBJ.RateToEgp = rateToEGP;
        //                                                    InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
        //                                                    InventoryStoreItemTransferReceivedOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
        //                                                    InventoryStoreItemTransferReceivedOBJ.RemainItemPrice = remainItemPrice;
        //                                                    InventoryStoreItemTransferReceivedOBJ.RemainItemCosetEgp = remainItemCosetEGP;
        //                                                    InventoryStoreItemTransferReceivedOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
        //                                                    InventoryStoreItemTransferReceivedOBJ.HoldQty = 0;
        //                                                    _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemTransferReceivedOBJ);
        //                                                    var InventoryStorItemTransferReceivedInsertion = _unitOfWork.Complete();
        //                                                    var ListInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemObjDB.Id && x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
        //                                                    var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
        //                                                    var ListIDSPOInvoicesIsFulllyPriced = _unitOfWork.PurchasePOInvoices.FindAll(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
        //                                                    ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
        //                                                    InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;
        //                                                    InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
        //                                                    InventoryStoreItemTransferReceivedOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;
        //                                                    _unitOfWork.Complete();
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                        #endregion items
        //                        Response.Result = true;
        //                        Response.ID = InventoryInternalTransferOrderID;
        //                    }
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
        //        var innerExceptionMessage = (!string.IsNullOrEmpty(ex.InnerException.Message.ToString())) == true ? ex.InnerException.Message.ToString() : null;
        //        var message = ex.Message;
        //        _logService.AddLogError("AddInvnetoryInternalTransferOrder", message, creator, compName, innerExceptionMessage);

        //        return Response;
        //    }
        //}





        public BaseResponseWithId<long> ReverseInvnetoryInternalTransferOrder(ReverseInvnetoryInternalTransferOrder Request, long creator, string compName)
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
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "please insert a valid data.";
                    Response.Errors.Add(error);
                    return Response;
                }

                var transferOrder = _unitOfWork.InventoryInternalTransferOrders.GetById(Request.TransferOrderId);
                if (transferOrder == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Transfer Order Not Found.";
                    Response.Errors.Add(error);
                    return Response;
                }

                var InventoryAddingOrder = new InventoryInternalTransferOrder()
                {
                    OperationType = "Add New Transfer Reverse",
                    Revision = 0,
                    FromInventoryStoreId = transferOrder.ToInventoryStoreId,
                    ToInventoryStoreId = transferOrder.FromInventoryStoreId,
                    RecivingDate = transferOrder.RecivingDate,
                    CreatedBy = creator,
                    ModifiedBy = creator,
                    CreationDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                };

                _unitOfWork.InventoryInternalTransferOrders.Add(InventoryAddingOrder);
                _unitOfWork.Complete();

                var transferItems = _unitOfWork.InventoryInternalTransferOrderItems.FindAll(a => a.InventoryInternalTransferOrderId == transferOrder.Id).ToList();
                if (transferItems.Count() > 0)
                {
                    foreach (var item in transferItems)
                    {
                        var TransferItem = new InventoryInternalTransferOrderItem()
                        {
                            InventoryInternalTransferOrderId = InventoryAddingOrder.Id,
                            InventoryItemId = item.InventoryItemId,
                            Uomid = item.Uomid,
                            Comments = item.Comments,
                            TransferredQty = item.TransferredQty
                        };
                        _unitOfWork.InventoryInternalTransferOrderItems.Add(TransferItem);
                        _unitOfWork.Complete();
                    }
                }

                var StoreItems = _unitOfWork.InventoryStoreItems.FindAll(a => a.OrderId == transferOrder.Id && a.OperationType.Contains("Transfer Order")).ToList();

                if (StoreItems.Where(a => a.Balance1 != a.FinalBalance).Count() > 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "تم سحب كمية من هذا المنتج من قبل";
                    Response.Errors.Add(error);
                    return Response;
                }

                Response.ID = transferOrder.Id;

                if (StoreItems.Count() > 0)
                {
                    foreach (var item in StoreItems)
                    {
                        if (Request.IsReverse)
                        {
                            if (item.OperationType == "Transfer Order (Released)")
                            {
                                var InventoryStoreItemOBJ = item;
                                InventoryStoreItemOBJ.Id = 0;
                                InventoryStoreItemOBJ.OrderNumber = InventoryAddingOrder.Id.ToString();
                                InventoryStoreItemOBJ.OrderId = InventoryAddingOrder.Id;
                                InventoryStoreItemOBJ.CreatedBy = creator;
                                InventoryStoreItemOBJ.ModifiedBy = creator;
                                InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                InventoryStoreItemOBJ.OperationType = "Transfer Order (Received)";
                                InventoryStoreItemOBJ.Balance1 = -1 * (item.Balance1);
                                InventoryStoreItemOBJ.FinalBalance = (decimal)(-1 * item.FinalBalance);
                                _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                _unitOfWork.Complete();
                            }
                            else if (item.OperationType == "Transfer Order (Received)")
                            {
                                var InventoryStoreItemOBJ = item;
                                InventoryStoreItemOBJ.Id = 0;
                                InventoryStoreItemOBJ.OrderNumber = InventoryAddingOrder.Id.ToString();
                                InventoryStoreItemOBJ.OrderId = InventoryAddingOrder.Id;
                                InventoryStoreItemOBJ.CreatedBy = creator;
                                InventoryStoreItemOBJ.ModifiedBy = creator;
                                InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                InventoryStoreItemOBJ.OperationType = "Transfer Order (Released)";
                                InventoryStoreItemOBJ.Balance1 = -1 * (item.Balance1);
                                InventoryStoreItemOBJ.FinalBalance = (decimal)(-1 * item.FinalBalance);
                                _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                _unitOfWork.Complete();
                            }
                        }
                        else if (!Request.IsReverse)
                        {
                            if (item.Balance1 == item.FinalBalance)
                            {
                                _unitOfWork.InventoryStoreItems.Delete(item);
                                _unitOfWork.Complete();
                                _unitOfWork.InventoryInternalTransferOrders.Delete(transferOrder);
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

                _logService.AddLogError("ReverseInvnetoryInternalTransferOrder", message, creator, compName, innerExceptionMessage);


                return Response;
            }
        }

        public InventoryInternalTransferOrderResponse GetInventoryInternalTransferItemList(GetInventoryInternalTransferFilters filters, long creator, string compName)
        {
            InventoryInternalTransferOrderResponse Response = new InventoryInternalTransferOrderResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var inventoryInternalTransferOrderByDateList = new List<InventoryInternalTransferOrderByDate>();
                /*long InventoryItemID = 0;
                if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                {
                    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                }*/

                /*long ToInventoryStoreID = 0;
                if (!string.IsNullOrEmpty(headers["ToInventoryStoreID"]) && long.TryParse(headers["ToInventoryStoreID"], out ToInventoryStoreID))
                {
                    long.TryParse(headers["ToInventoryStoreID"], out ToInventoryStoreID);
                }*/

                /*long FormInventoryStoreID = 0;
                if (!string.IsNullOrEmpty(headers["FormInventoryStoreID"]) && long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID))
                {
                    long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID);
                }*/


                /*long CreatorUserID = 0;
                if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                {
                    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                }*/


                /*DateTime? ReceiveDate = null;
                DateTime ReceiveDateTemp = DateTime.Now;
                if (!string.IsNullOrEmpty(headers["ReceiveDate"]) && DateTime.TryParse(headers["ReceiveDate"], out ReceiveDateTemp))
                {
                    ReceiveDateTemp = DateTime.Parse(headers["ReceiveDate"]);
                    ReceiveDate = ReceiveDateTemp;
                }*/


                // Grouped by DAte as Inquiry 
                //var InventoryMatrialAddingOrder


                var InventoryInternalTransferOrderList = _unitOfWork.InventoryInternalTransferOrders.FindAllQueryable(a => true, includes: new[] { "FromInventoryStore", "ToInventoryStore", "CreatedByNavigation" });

                if (filters.FormInventoryStoreID != 0)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.FromInventoryStoreId == filters.FormInventoryStoreID).AsQueryable();
                }
                if (filters.ToInventoryStoreID != 0)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.ToInventoryStoreId == filters.ToInventoryStoreID).AsQueryable();
                }
                if (filters.CreatorUserID != 0)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                }
                if (filters.ReceiveDate != null)
                {

                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.RecivingDate.Date == ((DateTime)filters.ReceiveDate).Date).AsQueryable();
                }
                if (filters.InventoryItemID != 0)
                {
                    var IDInventoryInternalTransferOrder = _unitOfWork.InventoryInternalTransferOrderItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryInternalTransferOrderId).Distinct().ToList();

                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => IDInventoryInternalTransferOrder.Contains(x.Id)).AsQueryable();
                }

                var InventoryOrderTransferFiltered = InventoryInternalTransferOrderList.OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();

                foreach (var InternalTransferPerMonth in InventoryOrderTransferFiltered)
                {
                    var InventoryTransferOrderInfoList = new List<InventoryInternalTransferOrderInfo>();

                    // var InternalTransferPerMonthList = InternalTransferPerMonth.ToList();

                    foreach (var Data in InternalTransferPerMonth)
                    {

                        InventoryTransferOrderInfoList.Add(new InventoryInternalTransferOrderInfo
                        {
                            InventoryInternalTransferOrderNo = Data.Id.ToString(),
                            FromInventoryStoreName = Data.FromInventoryStore?.Name,
                            ToInventoryStoreName = Data.ToInventoryStore?.Name,
                            RecivingDate = Data.RecivingDate.ToShortDateString(),
                            CreationDate = Data.CreationDate.ToShortDateString(),
                            CreatorName = Data.CreatedByNavigation?.FirstName + " " + Data.CreatedByNavigation?.LastName,
                        });
                    }
                    inventoryInternalTransferOrderByDateList.Add(new InventoryInternalTransferOrderByDate()
                    {
                        DateMonth = Common.GetMonthName(InternalTransferPerMonth.Key.month) + " " + InternalTransferPerMonth.Key.year.ToString(),
                        //item.Select(x=>x.CreatedDate.Month).ToString(),
                        InventoryInternalTransferOrderInfoList = InventoryTransferOrderInfoList.Distinct().ToList(),
                    });
                }

                Response.InventoryInternalTransferOrderByDateList = inventoryInternalTransferOrderByDateList;


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

                _logService.AddLogError("GetInventoryInternalTransferItemList", message, creator, compName, innerExceptionMessage);


                return Response;
            }
        }

        public InventoryInternalTransferOrderItemInfoResponse GetInventoryIntenralTransferItemInfo([FromHeader] long InternalTransferOrderID, long creator, string compName)
        {
            InventoryInternalTransferOrderItemInfoResponse Response = new InventoryInternalTransferOrderItemInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemTransferInfoObj = new InventoryItemInternalTransferOrderInfo();
                var InternalTransferOrderInfList = new List<InternalTransferOrderInfo>();
                if (Response.Result)
                {
                    /*long InternalTransferOrderID = 0;
                    if (!string.IsNullOrEmpty(headers["InternalTransferOrderID"]) && long.TryParse(headers["InternalTransferOrderID"], out InternalTransferOrderID))
                    {
                        long.TryParse(headers["InternalTransferOrderID"], out InternalTransferOrderID);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err123";
                        error.ErrorMSG = "Invalid Internal Transfer Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }*/

                    if (Response.Result)
                    {
                        var InventoryInternalTransferOrderOBJDB = _unitOfWork.VInventoryInternalTransferOrders.FindAll(x => x.Id == InternalTransferOrderID).FirstOrDefault();
                        if (InventoryInternalTransferOrderOBJDB != null)
                        {
                            InventoryItemTransferInfoObj.InventoryInternalTransferOrderID = InternalTransferOrderID;
                            InventoryItemTransferInfoObj.FromStoreName = InventoryInternalTransferOrderOBJDB.FromInventoryStoreName;
                            InventoryItemTransferInfoObj.CreatorName = InventoryInternalTransferOrderOBJDB.CreatorUserName;
                            InventoryItemTransferInfoObj.ToStoreName = InventoryInternalTransferOrderOBJDB.ToInventoryStoreName;
                            InventoryItemTransferInfoObj.RecivingDate = InventoryInternalTransferOrderOBJDB.RecivingDate.ToShortDateString();

                            var ListOfMInternalTransferOrderItemListDB = _unitOfWork.InventoryInternalTransferOrderItems.FindAll(x => x.InventoryInternalTransferOrderId == InternalTransferOrderID, includes: new[] { "InventoryItem", "Uom" }).ToList();
                            if (ListOfMInternalTransferOrderItemListDB != null)
                            {
                                foreach (var item in ListOfMInternalTransferOrderItemListDB)
                                {

                                    var TransferOrderItemInfoObj = new InternalTransferOrderInfo();
                                    TransferOrderItemInfoObj.InventoryItemID = item.InventoryItemId;
                                    TransferOrderItemInfoObj.ItemName = item.InventoryItem?.Name.Trim();
                                    TransferOrderItemInfoObj.InventoryItemID = item.InventoryItemId;
                                    TransferOrderItemInfoObj.TransferedQTY = item.TransferredQty.ToString();
                                    TransferOrderItemInfoObj.UOM = item.Uom?.ShortName;
                                    TransferOrderItemInfoObj.ItemCode = item.InventoryItem?.Code;

                                    InternalTransferOrderInfList.Add(TransferOrderItemInfoObj);
                                }
                            }
                        }

                        InventoryItemTransferInfoObj.InternalBackOrderItemInfoList = InternalTransferOrderInfList;

                    }
                    Response.InventoryItemInfo = InventoryItemTransferInfoObj;


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

                _logService.AddLogError("GetInventoryIntenralTransferItemInfo", message, creator, compName, innerExceptionMessage);


                return Response;
            }
        }

        public BaseResponseWithDataAndHeader<List<InventoryInternalTransferOrderInfo>> GetInventoryInternalTransferItemListForWeb(GetInventoryInternalTransferForWebFilters filters, long creator, string compName)
        {
            var Response = new BaseResponseWithDataAndHeader<List<InventoryInternalTransferOrderInfo>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //var inventoryInternalTransferOrderByDateList = new List<InventoryInternalTransferOrderByDate>();
                #region old Filters
                /*long InventoryItemID = 0;
                if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                {
                    long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                }*/

                /*long ToInventoryStoreID = 0;
                if (!string.IsNullOrEmpty(headers["ToInventoryStoreID"]) && long.TryParse(headers["ToInventoryStoreID"], out ToInventoryStoreID))
                {
                    long.TryParse(headers["ToInventoryStoreID"], out ToInventoryStoreID);
                }*/

                /*long FormInventoryStoreID = 0;
                if (!string.IsNullOrEmpty(headers["FormInventoryStoreID"]) && long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID))
                {
                    long.TryParse(headers["FormInventoryStoreID"], out FormInventoryStoreID);
                }*/


                /*long CreatorUserID = 0;
                if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                {
                    long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                }*/


                /*DateTime? ReceiveDate = null;
                DateTime ReceiveDateTemp = DateTime.Now;
                if (!string.IsNullOrEmpty(headers["ReceiveDate"]) && DateTime.TryParse(headers["ReceiveDate"], out ReceiveDateTemp))
                {
                    ReceiveDateTemp = DateTime.Parse(headers["ReceiveDate"]);
                    ReceiveDate = ReceiveDateTemp;
                }*/


                // Grouped by DAte as Inquiry 
                //var InventoryMatrialAddingOrder
                #endregion

                var InventoryInternalTransferOrderList = _unitOfWork.InventoryInternalTransferOrders.FindAllQueryable(a => true, includes: new[] { "FromInventoryStore", "ToInventoryStore", "CreatedByNavigation" });

                if (filters.FormInventoryStoreID != 0)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.FromInventoryStoreId == filters.FormInventoryStoreID).AsQueryable();
                }
                if (filters.ToInventoryStoreID != 0)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.ToInventoryStoreId == filters.ToInventoryStoreID).AsQueryable();
                }
                if (filters.CreatorUserID != 0)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                }
                if (filters.ReceiveDate != null)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.RecivingDate.Date == ((DateTime)filters.ReceiveDate).Date).AsQueryable();
                }
                if (filters.CreationDate != null)
                {
                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => x.CreationDate.Date == ((DateTime)filters.CreationDate).Date).AsQueryable();
                }
                if (filters.InventoryItemID != 0)
                {
                    var IDInventoryInternalTransferOrder = _unitOfWork.InventoryInternalTransferOrderItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryInternalTransferOrderId).Distinct().ToList();

                    InventoryInternalTransferOrderList = InventoryInternalTransferOrderList.Where(x => IDInventoryInternalTransferOrder.Contains(x.Id)).AsQueryable();
                }

                var InventoryOrderTransferFiltered = InventoryInternalTransferOrderList.OrderByDescending(x => x.CreationDate).AsQueryable();

                var InventoryOrderTransferPaged = PagedList<InventoryInternalTransferOrder>.Create(InventoryOrderTransferFiltered, filters.CurrentPage, filters.NumberOfItemsPerPage);

                var finalData = new GetInventoryInternalTransferItemForWebList();

                var InventoryTransferOrderInfoList = new List<InventoryInternalTransferOrderInfo>();

                foreach (var Data in InventoryOrderTransferPaged)
                {


                    InventoryTransferOrderInfoList.Add(new InventoryInternalTransferOrderInfo
                    {
                        InventoryInternalTransferOrderNo = Data.Id.ToString(),
                        FromInventoryStoreName = Data.FromInventoryStore?.Name,
                        ToInventoryStoreName = Data.ToInventoryStore?.Name,
                        RecivingDate = Data.RecivingDate.ToShortDateString(),
                        CreationDate = Data.CreationDate.ToShortDateString(),
                        CreatorName = Data.CreatedByNavigation?.FirstName + " " + Data.CreatedByNavigation?.LastName,
                    });

                }
                //finalData.InternalTransferOrderList = InventoryTransferOrderInfoList;
                Response.Data = InventoryTransferOrderInfoList;

                filters.CurrentPage = filters.CurrentPage != 0 ? filters.CurrentPage : 1;
                filters.NumberOfItemsPerPage = filters.NumberOfItemsPerPage != 0 ? filters.NumberOfItemsPerPage : 10;
                Response.PaginationHeader = new PaginationHeader
                {
                    CurrentPage = filters.CurrentPage,
                    TotalPages = InventoryOrderTransferPaged.TotalPages,
                    ItemsPerPage = filters.NumberOfItemsPerPage,
                    TotalItems = InventoryOrderTransferPaged.TotalCount
                };
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

                _logService.AddLogError("GetInventoryInternalTransferItemList", message, creator, compName, innerExceptionMessage);


                return Response;
            }
        }

    }
}
