using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.InventoryItem;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.InventoryItem;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.UsedInResponse;
using NewGarasAPI.Models.Inventory;
using OfficeOpenXml;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class InventoryOpeningBalanceService : IInventoryOpeningBalanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private readonly IPOSService _pOSService;

        public InventoryOpeningBalanceService(IUnitOfWork unitOfWork, IWebHostEnvironment host,IPOSService pOSService)
        {
            _unitOfWork = unitOfWork;
            _host = host;
            _pOSService = pOSService;
        }

            
        public BaseResponseWithId<long> AddInventoryItemOpeningBalance(AddInventoryItemOpeningBalanceRequest Request, long UserID)
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.InventoryItemOpeningBalanceHeadList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int Counter = 0;

                    // All Inventory Item  From Request
                    var IDSInvItemList = Request.InventoryItemOpeningBalanceHeadList.SelectMany(x => x.ItemList.Select(y => y.InventoryItemID)).ToList();
                    var InventoryItemListDB = _unitOfWork.InventoryItems.FindAll(x => IDSInvItemList.Contains(x.Id)).ToList();
                    foreach (var item in Request.InventoryItemOpeningBalanceHeadList)
                    {
                        Counter++;

                        if (item.InventoryStoreID == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err24";
                            error.ErrorMSG = "Invalid Inventory Store ID #" + Counter;
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (item.ItemList == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-14";
                            error.ErrorMSG = "please insert at least one Item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        else if (item.ItemList.Count <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-14";
                            error.ErrorMSG = "please insert at least one Item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.ItemList != null)
                        {
                            var OpeningBalanceItemList = item.ItemList;
                            var InventoryItemList = OpeningBalanceItemList.Where(x => x.InventoryItemID == 0).ToList();
                            var InventoryItemQTYList = OpeningBalanceItemList.Where(x => x.QTY <= 0).ToList();
                            if (InventoryItemList.Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err28";
                                error.ErrorMSG = "Invalid Inventory Item ID Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }

                            if (InventoryItemQTYList.Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err29";
                                error.ErrorMSG = "Invalid Inventory Item QTY item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            var CheckIfInventoryItemExist = InventoryItemListDB.Where(x => OpeningBalanceItemList.Select(y => y.InventoryItemID).ToList().Contains(x.Id)).Count();
                            if (CheckIfInventoryItemExist != OpeningBalanceItemList.Count())
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err28";
                                error.ErrorMSG = "Invalid Inventory Item ID item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }


                    }







                    if (Response.Result)
                    {

                        // Loop List  Of Inventory Store with each Items

                        var ListOfInventoryStoreList = Request.InventoryItemOpeningBalanceHeadList;
                        if (ListOfInventoryStoreList.Count() > 0)
                        {
                            string OperationType = "Opening Balance";
                            string SupplierName = "DIRECT PR HIDDEN SUPPLIER";
                            long? SupplierID = _unitOfWork.Suppliers.Find(a => a.Name == SupplierName).Id;//Common.GetSupplierID(SupplierName);
                            if (SupplierID == null)
                            {
                                // Insert supplier
                                //ObjectParameter IDSupplier = new ObjectParameter("ID", typeof(long));
                                //var SupplierInsertion = _Context.proc_SupplierInsert(IDSupplier,
                                //                                                     SupplierName,
                                //                                                     "Supplier",
                                //                                                     null, null,
                                //                                                     UserID,
                                //                                                     DateTime.Now,
                                //                                                     null, null, null, null, null,
                                //                                                     true,
                                //                                                     null, null, null, null, null,
                                //                                                     null, null, null, null, null, null, null
                                //                                                     );

                                var newSupplier = new Supplier();
                                newSupplier.Name = SupplierName;
                                newSupplier.Type = "Supplier";
                                newSupplier.CreatedBy = UserID;
                                newSupplier.CreationDate = DateTime.Now;
                                newSupplier.Active = true;

                                var SupplierInsertion = _unitOfWork.Suppliers.Add(newSupplier);
                                _unitOfWork.Complete();

                                if (SupplierInsertion.Id != 0)
                                {
                                    SupplierID = SupplierInsertion.Id;
                                }

                            }
                            if (SupplierID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err23";
                                error.ErrorMSG = "Invalid Supplier ID.";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            foreach (var ItemOpeningBalancePerStore in ListOfInventoryStoreList)
                            {
                                // Check Inventory Report Approved and closed or not
                                var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == ItemOpeningBalancePerStore.InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();
                                if (CheckInventoryReportListDB.Count > 0)
                                {
                                    foreach (var InventoryRep in CheckInventoryReportListDB)
                                    {
                                        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                        {
                                            string storeName = _unitOfWork.InventoryStores.GetById(ItemOpeningBalancePerStore.InventoryStoreID).Name;//Common.getInventoryStoreName(ItemOpeningBalancePerStore.InventoryStoreID);
                                            string errMsg = "Note : Store " + storeName +
                                                " is under inventory from " +
                                                InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
                                                " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

                                            Response.Result = true; // Warning
                                            Error error = new Error();
                                            error.ErrorCode = "Err-44";
                                            error.ErrorMSG = errMsg;
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                                // ------------------------------------------------
                                DateTime RecevingData = DateTime.Now;
                                if (string.IsNullOrEmpty(ItemOpeningBalancePerStore.RecevingData) || !DateTime.TryParse(ItemOpeningBalancePerStore.RecevingData, out RecevingData))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err23";
                                    error.ErrorMSG = "Invalid Receving Data.";
                                    Response.Errors.Add(error);
                                    return Response;
                                }

                                long InventoryAddingOrderID = 0;
                                // Insertion Adding Order
                                //ObjectParameter IDInventoryAddingOrder = new ObjectParameter("ID", typeof(long));
                                //var InventoryAddingOrderInsertion = _unitOfWork.proc_InventoryAddingOrderInsert(IDInventoryAddingOrder, OperationType, DateTime.Now, UserID, 0, SupplierID,
                                //                                                                             null, 0, ItemOpeningBalancePerStore.InventoryStoreID, RecevingData, UserID, DateTime.Now);

                                var newInventroyOrder = new InventoryAddingOrder();
                                newInventroyOrder.OperationType = OperationType;
                                newInventroyOrder.CreationDate = DateTime.Now;
                                newInventroyOrder.CreatedBy = UserID;
                                newInventroyOrder.Revision = 0;
                                newInventroyOrder.SupplierId = SupplierID??0;
                                newInventroyOrder.Ponumber = null;
                                newInventroyOrder.LoadBy = 0;
                                newInventroyOrder.InventoryStoreId = ItemOpeningBalancePerStore.InventoryStoreID;
                                newInventroyOrder.RecivingDate = RecevingData;
                                newInventroyOrder.ModifiedBy = UserID;
                                newInventroyOrder.ModifiedDate = DateTime.Now;

                                var InventoryAddingOrderInsertion = _unitOfWork.InventoryAddingOrders.Add(newInventroyOrder);
                                _unitOfWork.Complete();

                                if (InventoryAddingOrderInsertion != null)
                                {
                                    InventoryAddingOrderID = InventoryAddingOrderInsertion.Id;
                                    #region items


                                    foreach (var ItemDataOBJ in ItemOpeningBalancePerStore.ItemList)
                                    {
                                        //var InventoryItemObjDB = _unitOfWork.proc_InventoryItemLoadByPrimaryKey(ItemDataOBJ.InventoryItemID).FirstOrDefault();
                                        var InventoryItemObjDB = _unitOfWork.InventoryItems.Find(a => a.Id == ItemDataOBJ.InventoryItemID);
                                        if (InventoryItemObjDB != null)
                                        {

                                            DateTime? ExpDate = null;
                                            if (ItemDataOBJ.ExpData != null && ItemDataOBJ.ExpData != "")
                                            {
                                                ExpDate = DateTime.Parse(ItemDataOBJ.ExpData);
                                            }
                                            //add new 
                                            //ObjectParameter IDInventoryAddingOrderItem = new ObjectParameter("ID", typeof(long));
                                            //var AddingOrderItemInsertion = _Context.Myproc_InventoryAddingOrderItemsInsert_New(IDInventoryAddingOrderItem, InventoryAddingOrderID, ItemDataOBJ.InventoryItemID,
                                            //                                                                             InventoryItemObjDB.RequstionUomid,
                                            //                                                                             // null,null,// (float)MatrialDataOBJ.AddedQTYUOR, //ReqQuantity,
                                            //                                                                             //(float)MatrialDataOBJ.AddedQTYUOR,
                                            //                                                                             ExpDate,
                                            //                                                                             ItemDataOBJ.Serial, null, ItemDataOBJ.Comment, null,
                                            //                                                                                null,// PO Req QTY
                                            //                                                                                ItemDataOBJ.QTY,
                                            //                                                                                null,
                                            //                                                                                null
                                            //                                                                             );

                                            var newInventoryItem = new InventoryAddingOrderItem();
                                            newInventoryItem.InventoryAddingOrderId = InventoryAddingOrderID;
                                            newInventoryItem.InventoryItemId = ItemDataOBJ.InventoryItemID;
                                            newInventoryItem.Uomid = InventoryItemObjDB.RequstionUomid;
                                            newInventoryItem.ExpDate = ExpDate;
                                            newInventoryItem.ItemSerial = ItemDataOBJ.Serial;
                                            newInventoryItem.Comments = ItemDataOBJ.Comment;
                                            newInventoryItem.RecivedQuantity1 = ItemDataOBJ.QTY;

                                                                           
                                            var AddingOrderItemInsertion = _unitOfWork.InventoryAddingOrderItems.Add(newInventoryItem);
                                            _unitOfWork.Complete();

                                            if (AddingOrderItemInsertion != null)
                                            {
                                                long AddingOrderItemId = AddingOrderItemInsertion.Id;
                                                int? StoreLocationID = _unitOfWork.InventoryStoreLocations.Find(a => a.Id ==ItemDataOBJ.StoreLocationID) != null ? ItemDataOBJ.StoreLocationID : null;

                                                // Extra Date For PO Items
                                                long? POInvoiceId = null;
                                                decimal? POInvoiceTotalPrice = null;
                                                decimal? POInvoiceTotalCost = null;
                                                int? currencyId = ItemDataOBJ.CurrencyId ?? _unitOfWork.Currencies.Find(a => a.IsLocal == true)?.Id??1;
                                                decimal? rateToEGP = 1;
                                                decimal? POInvoiceTotalPriceEGP = null;
                                                decimal? POInvoiceTotalCostEGP = null;
                                                decimal? remainItemPrice = null;
                                                decimal? remainItemCosetEGP = null;
                                                decimal? remainItemCostOtherCU = null;


                                                decimal finalBalance = ItemDataOBJ.QTY;
                                                rateToEGP = ItemDataOBJ.RateToEGP ?? 1;
                                                POInvoiceTotalPriceEGP = ItemDataOBJ.CostEGP ?? 0;
                                                POInvoiceTotalCostEGP = ItemDataOBJ.CostEGP ?? 0;
                                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                var InventoryStoreItemOBJ = new InventoryStoreItem();
                                                InventoryStoreItemOBJ.InventoryStoreId = ItemOpeningBalancePerStore.InventoryStoreID;
                                                InventoryStoreItemOBJ.InventoryItemId = ItemDataOBJ.InventoryItemID;
                                                InventoryStoreItemOBJ.OrderNumber = InventoryAddingOrderID.ToString();
                                                InventoryStoreItemOBJ.OrderId = InventoryAddingOrderID;
                                                InventoryStoreItemOBJ.CreatedBy = UserID;
                                                InventoryStoreItemOBJ.ModifiedBy = UserID;
                                                InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                InventoryStoreItemOBJ.OperationType = OperationType;
                                                InventoryStoreItemOBJ.Balance = (double)ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.Balance1 = ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.InvenoryStoreLocationId = StoreLocationID;
                                                InventoryStoreItemOBJ.ExpDate = ExpDate;
                                                InventoryStoreItemOBJ.ItemSerial = ItemDataOBJ.Serial;
                                                InventoryStoreItemOBJ.FinalBalance = ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.AddingOrderItemId = AddingOrderItemId;
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

                                                //ObjectParameter IDInventoryStoreItemInsertion = new ObjectParameter("ID", typeof(long));
                                                //var InventoryStorItemInsertion = _Context.Myproc_InventoryStoreItemInsert_API(IDInventoryStoreItemInsertion,
                                                //                                                                        ItemOpeningBalancePerStore.InventoryStoreID,
                                                //                                                                        ItemDataOBJ.InventoryItemID,
                                                //                                                                        InventoryAddingOrderID.ToString(),
                                                //                                                                        InventoryAddingOrderID,
                                                //                                                                        // (double)MatrialDataOBJ.AddedQTYUOR,
                                                //                                                                        DateTime.Now,
                                                //                                                                        validation.userID,
                                                //                                                                        DateTime.Now,
                                                //                                                                        validation.userID,
                                                //                                                                        OperationType,
                                                //                                                                        ItemDataOBJ.QTY,
                                                //                                                                        StoreLocationID,
                                                //                                                                        ExpDate,
                                                //                                                                         ItemDataOBJ.Serial,
                                                //                                                                         null,
                                                //                                                                         ItemDataOBJ.QTY,
                                                //                                                                         AddingOrderItemId,
                                                //                                                                         null,
                                                //                                                                         // Extra Data POItem
                                                //                                                                         pOInvoiceId: POInvoiceId,
                                                //                                                                         pOInvoiceTotalPrice: POInvoiceTotalPrice,
                                                //                                                                         pOInvoiceTotalCost: POInvoiceTotalCost,
                                                //                                                                         currencyId: currencyId,
                                                //                                                                         rateToEGP: rateToEGP,
                                                //                                                                         pOInvoiceTotalPriceEGP: POInvoiceTotalPriceEGP,
                                                //                                                                         pOInvoiceTotalCostEGP: POInvoiceTotalCostEGP,
                                                //                                                                         remainItemPrice: remainItemPrice,
                                                //                                                                         remainItemCosetEGP: remainItemCosetEGP,
                                                //                                                                         remainItemCostOtherCU: remainItemCostOtherCU,
                                                //                                                                    0,
                                                //                                                                    null
                                                //                                                                        );


                                            }



                                        }

                                    }

                                    #endregion items
                                    Response.Result = true;
                                    Response.ID = InventoryAddingOrderID;
                                }



                            }




                        }






                        //if (Response.Result)
                        //{
                        //    string OperationType = "Add New Matrial"; //  Default Adding Order
                        //    if (Request.OrderType == "ExternalBackOrder")
                        //    {
                        //        OperationType = "Add External Back Order";
                        //    }
                        //    // Insertion Adding Order
                        //    ObjectParameter IDInventoryAddingOrder = new ObjectParameter("ID", typeof(long));
                        //    var InventoryAddingOrderInsertion = _Context.proc_InventoryAddingOrderInsert(IDInventoryAddingOrder, OperationType, DateTime.Now, validation.userID, 0, SupplierID,
                        //                                                                                 null, 0, InventoryStoreID, RecevingData, validation.userID, DateTime.Now);

                        //    if (InventoryAddingOrderInsertion > 0)
                        //    {
                        //        InventoryAddingOrderID = (long)IDInventoryAddingOrder.Value;
                        //        #region items


                        //        foreach (var MatrialDataOBJ in Request.MatrialAddingOrderItemList)
                        //        {
                        //            var InventoryItemObjDB = _Context.proc_InventoryItemLoadByPrimaryKey(MatrialDataOBJ.InventoryItemID).FirstOrDefault();
                        //            if (InventoryItemObjDB != null)
                        //            {
                        //                var ActivePOObjDB = _Context.proc_PurchasePOItemLoadAll().Where(x => x.PurchasePOID == MatrialDataOBJ.POID && x.InventoryItemID == MatrialDataOBJ.InventoryItemID).FirstOrDefault();
                        //                decimal ReqQuantity = 0;
                        //                if (ActivePOObjDB != null)
                        //                {
                        //                    ReqQuantity = (decimal)ActivePOObjDB.ReqQuantity;
                        //                }
                        //                decimal ReceeivedOrReturnQTY = MatrialDataOBJ.QTYUOR;
                        //                if (Request.OrderType == "ExternalBackOrder")
                        //                {
                        //                    ReceeivedOrReturnQTY = ReceeivedOrReturnQTY * -1;
                        //                }





                        //                decimal ReceeivedOrReturnQTYForSum = MatrialDataOBJ.QTYUOR;
                        //                decimal ReceivedQTYAfter = ReceeivedOrReturnQTYForSum;
                        //                var LastItemAddingOrderObjLoad = _Context.V_InventoryAddingOrderItems.Where(x => x.InventoryItemID == MatrialDataOBJ.InventoryItemID && x.POID == MatrialDataOBJ.POID && x.SupplierID == SupplierID).ToList().LastOrDefault();
                        //                if (LastItemAddingOrderObjLoad != null && LastItemAddingOrderObjLoad.RecivedQuantityAfter != null)
                        //                {
                        //                    if (Request.OrderType == "ExternalBackOrder")
                        //                    {
                        //                        ReceeivedOrReturnQTYForSum = ReceeivedOrReturnQTYForSum * -1;
                        //                    }
                        //                    ReceivedQTYAfter = ReceeivedOrReturnQTYForSum + (decimal)LastItemAddingOrderObjLoad.RecivedQuantityAfter;
                        //                }
                        //                decimal RemainQTY = ReqQuantity - ReceivedQTYAfter;




                        //                DateTime? ExpDate = null;
                        //                if (MatrialDataOBJ.ExpData != null && MatrialDataOBJ.ExpData != "")
                        //                {
                        //                    ExpDate = DateTime.Parse(MatrialDataOBJ.ExpData);
                        //                }
                        //                //add new 
                        //                ObjectParameter IDInventoryAddingOrderItem = new ObjectParameter("ID", typeof(long));
                        //                var AddingOrderItemInsertion = _Context.Myproc_InventoryAddingOrderItemsInsert_New(IDInventoryAddingOrderItem, InventoryAddingOrderID, MatrialDataOBJ.InventoryItemID,
                        //                                                                                             InventoryItemObjDB.RequstionUOMID,
                        //                                                                                             // null,null,// (float)MatrialDataOBJ.AddedQTYUOR, //ReqQuantity,
                        //                                                                                             //(float)MatrialDataOBJ.AddedQTYUOR,
                        //                                                                                             ExpDate,
                        //                                                                                             MatrialDataOBJ.Serial, MatrialDataOBJ.QIReport, MatrialDataOBJ.Comment, MatrialDataOBJ.POID,
                        //                                                                                                ReqQuantity,// PO Req QTY
                        //                                                                                                ReceeivedOrReturnQTY,
                        //                                                                                                ReceivedQTYAfter,
                        //                                                                                                RemainQTY
                        //                                                                                             );
                        //                if (AddingOrderItemInsertion > 0)
                        //                {
                        //                    if (Request.OrderType == "AddingOrder")
                        //                    {

                        //                        var res = UpdatePoStatus(ActivePOObjDB.ID,
                        //                                        MatrialDataOBJ.POID,
                        //                                          MatrialDataOBJ.QTYUOR);
                        //                    }

                        //                    decimal BalanceQTY = MatrialDataOBJ.QTYUOR;
                        //                    if (Request.OrderType == "ExternalBackOrder")
                        //                    {
                        //                        BalanceQTY = BalanceQTY * -1;
                        //                    }

                        //                    ObjectParameter IDInventoryStoreItemInsertion = new ObjectParameter("ID", typeof(long));
                        //                    var InventoryStorItemInsertion = _Context.Myproc_InventoryStoreItemInsert_New(IDInventoryStoreItemInsertion,
                        //                                                                                            InventoryStoreID,
                        //                                                                                            MatrialDataOBJ.InventoryItemID,
                        //                                                                                            InventoryAddingOrderID.ToString(),
                        //                                                                                            InventoryAddingOrderID,
                        //                                                                                            // (double)MatrialDataOBJ.AddedQTYUOR,
                        //                                                                                            DateTime.Now,
                        //                                                                                            validation.userID,
                        //                                                                                            DateTime.Now,
                        //                                                                                            validation.userID,
                        //                                                                                            OperationType,
                        //                                                                                            BalanceQTY
                        //                                                                                            );


                        //                }



                        //            }

                        //            //GarasERP.InventoryStoreItem storeItem = new GarasERP.InventoryStoreItem();
                        //            //storeItem.AddNew();
                        //            //storeItem.Balance = (decimal)items.RecivedQuantity;
                        //            //storeItem.CreatedBy = UserID;
                        //            //storeItem.CreationDate = DateTime.Now;
                        //            //storeItem.InventoryItemID = items.InventoryItemID;
                        //            //storeItem.InventoryStoreID = order.InventoryStoreID;
                        //            //storeItem.ModifiedBy = UserID;
                        //            //storeItem.ModifiedDate = DateTime.Now;
                        //            //storeItem.OperationType = order.OperationType;
                        //            //storeItem.OrderID = order.ID;
                        //            //storeItem.OrderNumber = order.s_ID;
                        //            //storeItem.Save();

                        //            ////check notification
                        //            //InventoryItem item = new InventoryItem();
                        //            //item.Where.ID.Value = items.InventoryItemID;
                        //            //item.Query.AddResultColumn(InventoryItem.ColumnNames.MaxBalance);
                        //            //item.Query.AddResultColumn(InventoryItem.ColumnNames.Name);
                        //            //if (item.Query.Load())
                        //            //{
                        //            //    if (item.DefaultView != null && item.DefaultView.Count > 0)
                        //            //    {
                        //            //        GarasERP.InventoryStoreItem itemBalance = new GarasERP.InventoryStoreItem();
                        //            //        itemBalance.Where.InventoryItemID.Value = items.InventoryItemID;
                        //            //        // itemBalance.Query.AddResultColumn(GarasERP.InventoryStoreItem.ColumnNames.Balance);
                        //            //        itemBalance.Query.AddGroupBy(GarasERP.InventoryStoreItem.ColumnNames.InventoryItemID);
                        //            //        itemBalance.Aggregate.Balance.Function = MyGeneration.dOOdads.AggregateParameter.Func.Sum;
                        //            //        if (itemBalance.Query.Load())
                        //            //        {
                        //            //            if (itemBalance.DefaultView != null && itemBalance.DefaultView.Count > 0)
                        //            //            {
                        //            //                if (itemBalance.s_Balance != "")
                        //            //                {
                        //            //                    if (itemBalance.Balance > (decimal)item.MaxBalance)
                        //            //                    {
                        //            //                        CommonClass.sendGroupNotifications("TopManagment", "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                        //            //                            "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                        //            //                            "~/inventory/ViewItem.aspx?TID=" + Server.UrlEncode(Encrypt_Decrypt.Encrypt(items.s_InventoryItemID, key)));

                        //            //                        CommonClass.sendGroupNotifications("Secretary", "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                        //            //                           "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Max Balance" + item.MaxBalance,
                        //            //                           "~/inventory/ViewItem.aspx?TID=" + Server.UrlEncode(Encrypt_Decrypt.Encrypt(items.s_InventoryItemID, key)));
                        //            //                    }
                        //            //                }
                        //            //            }
                        //            //        }

                        //            //    }
                        //            //}







                        //        }

                        //        #endregion items
                        //        Response.Result = true;
                        //        Response.ID = InventoryAddingOrderID;
                        //    }

                        //}


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

        public BaseResponseWithId<long> AddInventoryItemOpeningBalanceForLIBMARK(AddInventoryItemOpeningBalanceRequest Request, long UserID)
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.InventoryItemOpeningBalanceHeadList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int Counter = 0;

                    // All Inventory Item  From Request
                    var IDSInvItemList = Request.InventoryItemOpeningBalanceHeadList.SelectMany(x => x.ItemList.Select(y => y.InventoryItemID)).ToList();
                    var InventoryItemListDB = _unitOfWork.InventoryItems.FindAll(x => IDSInvItemList.Contains(x.Id)).ToList();
                    foreach (var item in Request.InventoryItemOpeningBalanceHeadList)
                    {
                        Counter++;

                        if (item.InventoryStoreID == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err24";
                            error.ErrorMSG = "Invalid Inventory Store ID #" + Counter;
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (item.ItemList == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-14";
                            error.ErrorMSG = "please insert at least one Item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        else if (item.ItemList.Count <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-14";
                            error.ErrorMSG = "please insert at least one Item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (item.ItemList != null)
                        {
                            var OpeningBalanceItemList = item.ItemList;
                            var InventoryItemList = OpeningBalanceItemList.Where(x => x.InventoryItemID == 0).ToList();
                            var InventoryItemQTYList = OpeningBalanceItemList.Where(x => x.QTY <= 0).ToList();
                            if (InventoryItemList.Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err28";
                                error.ErrorMSG = "Invalid Inventory Item ID Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }

                            if (InventoryItemQTYList.Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err29";
                                error.ErrorMSG = "Invalid Inventory Item QTY item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            var CheckIfInventoryItemExist = InventoryItemListDB.Where(x => OpeningBalanceItemList.Select(y => y.InventoryItemID).ToList().Contains(x.Id)).Count();
                            if (CheckIfInventoryItemExist != OpeningBalanceItemList.Count())
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err28";
                                error.ErrorMSG = "Invalid Inventory Item ID item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }


                    }
                    if (Response.Result)
                    {

                        // Loop List  Of Inventory Store with each Items

                        var ListOfInventoryStoreList = Request.InventoryItemOpeningBalanceHeadList;
                        if (ListOfInventoryStoreList.Count() > 0)
                        {
                            string OperationType = "Opening Balance";
                            string SupplierName = "DIRECT PR HIDDEN SUPPLIER";
                            long SupplierID = _unitOfWork.Suppliers.Find(a => a.Name == SupplierName).Id; //Common.GetSupplierID(SupplierName);
                            if (SupplierID == 0)
                            {
                                // Insert supplier
                                //ObjectParameter IDSupplier = new ObjectParameter("ID", typeof(long));
                                //var SupplierInsertion = _Context.proc_SupplierInsert(IDSupplier,
                                //                                                     SupplierName,
                                //                                                     "Supplier",
                                //                                                     null, null,
                                //                                                     UserID,
                                //                                                     DateTime.Now,
                                //                                                     null, null, null, null, null,
                                //                                                     true,
                                //                                                     null, null, null, null, null,
                                //                                                     null, null, null, null, null, null, null
                                //                                                     );
                                var newSupplier = new Supplier();
                                newSupplier.Name = SupplierName;
                                newSupplier.Type = "Supplier";
                                newSupplier.CreatedBy = UserID;
                                newSupplier.CreationDate = DateTime.Now;
                                newSupplier.Active = true;

                                var SupplierInsertion = _unitOfWork.Suppliers.Add(newSupplier);
                                _unitOfWork.Complete();

                                if (SupplierInsertion != null)
                                {
                                    SupplierID = SupplierInsertion.Id;
                                }

                            }
                            if (SupplierID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err23";
                                error.ErrorMSG = "Invalid Supplier ID.";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            foreach (var ItemOpeningBalancePerStore in ListOfInventoryStoreList)
                            {
                                // Check Inventory Report Approved and closed or not
                                var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == ItemOpeningBalancePerStore.InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();
                                if (CheckInventoryReportListDB.Count > 0)
                                {
                                    foreach (var InventoryRep in CheckInventoryReportListDB)
                                    {
                                        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                        {
                                            string storeName = _unitOfWork.InventoryStores.GetById(ItemOpeningBalancePerStore.InventoryStoreID).Name;//Common.getInventoryStoreName(ItemOpeningBalancePerStore.InventoryStoreID);
                                            string errMsg = "Note : Store " + storeName +
                                                " is under inventory from " +
                                                InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
                                                " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

                                            Response.Result = true; // Warning
                                            Error error = new Error();
                                            error.ErrorCode = "Err-44";
                                            error.ErrorMSG = errMsg;
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                                // ------------------------------------------------
                                DateTime RecevingData = DateTime.Now;
                                if (string.IsNullOrEmpty(ItemOpeningBalancePerStore.RecevingData) || !DateTime.TryParse(ItemOpeningBalancePerStore.RecevingData, out RecevingData))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err23";
                                    error.ErrorMSG = "Invalid Receving Data.";
                                    Response.Errors.Add(error);
                                    return Response;
                                }

                                long InventoryAddingOrderID = 0;
                                // Insertion Adding Order
                                //ObjectParameter IDInventoryAddingOrder = new ObjectParameter("ID", typeof(long));
                                //var InventoryAddingOrderInsertion = _Context.proc_InventoryAddingOrderInsert(IDInventoryAddingOrder, OperationType, DateTime.Now, validation.userID, 0, SupplierID,
                                //                                                                             null, 0, ItemOpeningBalancePerStore.InventoryStoreID, RecevingData, validation.userID, DateTime.Now);

                                var newInventroyOrder = new InventoryAddingOrder();
                                newInventroyOrder.OperationType = OperationType;
                                newInventroyOrder.CreationDate = DateTime.Now;
                                newInventroyOrder.CreatedBy = UserID;
                                newInventroyOrder.Revision = 0;
                                newInventroyOrder.SupplierId = SupplierID;
                                newInventroyOrder.Ponumber = null;
                                newInventroyOrder.LoadBy = 0;
                                newInventroyOrder.InventoryStoreId = ItemOpeningBalancePerStore.InventoryStoreID;
                                newInventroyOrder.RecivingDate = RecevingData;
                                newInventroyOrder.ModifiedBy = UserID;
                                newInventroyOrder.ModifiedDate = DateTime.Now;

                                var InventoryAddingOrderInsertion = _unitOfWork.InventoryAddingOrders.Add(newInventroyOrder);
                                _unitOfWork.Complete();


                                if (InventoryAddingOrderInsertion != null)
                                {
                                    InventoryAddingOrderID = InventoryAddingOrderInsertion.Id;
                                    #region items

                                    int Count = 0;
                                    foreach (var ItemDataOBJ in ItemOpeningBalancePerStore.ItemList)
                                    {
                                        Count++;
                                        int PublicId = ItemDataOBJ.PublicId ?? Count;
                                        string PublicIdNumber = PublicId.ToString();
                                        var InventoryItemObjDB = _unitOfWork.InventoryItems.Find(a => a.Id == ItemDataOBJ.InventoryItemID);//_Context.proc_InventoryItemLoadByPrimaryKey(ItemDataOBJ.InventoryItemID).FirstOrDefault();
                                        if (InventoryItemObjDB != null)
                                        {

                                            DateTime? ExpDate = null;
                                            if (ItemDataOBJ.ExpData != null && ItemDataOBJ.ExpData != "")
                                            {
                                                ExpDate = DateTime.Parse(ItemDataOBJ.ExpData);
                                            }


                                            //add new 
                                            //ObjectParameter IDInventoryAddingOrderItem = new ObjectParameter("ID", typeof(long));
                                            //var AddingOrderItemInsertion = _Context.Myproc_InventoryAddingOrderItemsInsert_New(IDInventoryAddingOrderItem, InventoryAddingOrderID, ItemDataOBJ.InventoryItemID,
                                            //                                                                             InventoryItemObjDB.RequstionUOMID,
                                            //                                                                             // null,null,// (float)MatrialDataOBJ.AddedQTYUOR, //ReqQuantity,
                                            //                                                                             //(float)MatrialDataOBJ.AddedQTYUOR,
                                            //                                                                             ExpDate,
                                            //                                                                             ItemDataOBJ.Serial,
                                            //                                                                             null,
                                            //                                                                             // Comment on LIBMARK this Public Id for Library
                                            //                                                                             PublicIdNumber,
                                            //                                                                             null,
                                            //                                                                                null,// PO Req QTY
                                            //                                                                                ItemDataOBJ.QTY,
                                            //                                                                                null,
                                            //                                                                                null
                                            //                                                                             );

                                            var newInventoryItem = new InventoryAddingOrderItem();
                                            newInventoryItem.InventoryAddingOrderId = InventoryAddingOrderID;
                                            newInventoryItem.InventoryItemId = ItemDataOBJ.InventoryItemID;
                                            newInventoryItem.Uomid = InventoryItemObjDB.RequstionUomid;
                                            newInventoryItem.ExpDate = ExpDate;
                                            newInventoryItem.ItemSerial = ItemDataOBJ.Serial;
                                            newInventoryItem.QcreportId = null;
                                            newInventoryItem.Comments = PublicIdNumber;
                                            newInventoryItem.Poid = null;
                                            newInventoryItem.ReqQuantity1 = null;
                                            newInventoryItem.RecivedQuantity1 = ItemDataOBJ.QTY;
                                            newInventoryItem.RecivedQuantityAfter = null;
                                            newInventoryItem.RemainQuantity = null;
                                            

                                            var AddingOrderItemInsertion = _unitOfWork.InventoryAddingOrderItems.Add(newInventoryItem);
                                            _unitOfWork.Complete();

                                            if (AddingOrderItemInsertion != null)
                                            {
                                                long AddingOrderItemId = AddingOrderItemInsertion.Id;
                                                int? StoreLocationID = _unitOfWork.InventoryStoreLocations.Find(a => a.Id == ItemDataOBJ.StoreLocationID) != null ? ItemDataOBJ.StoreLocationID : null;
                                                // Extra Date For PO Items
                                                long? POInvoiceId = null;
                                                decimal? POInvoiceTotalPrice = null;
                                                decimal? POInvoiceTotalCost = null;
                                                int? currencyId = ItemDataOBJ.CurrencyId ?? _unitOfWork.Currencies.Find(a => a.IsLocal == true)?.Id??1;
                                                decimal? rateToEGP = 1;
                                                decimal? POInvoiceTotalPriceEGP = null;
                                                decimal? POInvoiceTotalCostEGP = null;
                                                decimal? remainItemPrice = null;
                                                decimal? remainItemCosetEGP = null;
                                                decimal? remainItemCostOtherCU = null;


                                                decimal finalBalance = ItemDataOBJ.QTY;
                                                rateToEGP = ItemDataOBJ.RateToEGP ?? 1;
                                                POInvoiceTotalPriceEGP = ItemDataOBJ.CostEGP ?? 0;
                                                POInvoiceTotalCostEGP = ItemDataOBJ.CostEGP ?? 0;
                                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                var InventoryStoreItemOBJ = new InventoryStoreItem();
                                                InventoryStoreItemOBJ.InventoryStoreId = ItemOpeningBalancePerStore.InventoryStoreID;
                                                InventoryStoreItemOBJ.InventoryItemId = ItemDataOBJ.InventoryItemID;
                                                //on LIBMARK this Public Id for Library
                                                InventoryStoreItemOBJ.OrderNumber = PublicIdNumber; // InventoryAddingOrderID.ToString();
                                                InventoryStoreItemOBJ.OrderId = InventoryAddingOrderID;
                                                InventoryStoreItemOBJ.CreatedBy = UserID;
                                                InventoryStoreItemOBJ.ModifiedBy = UserID;
                                                InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                InventoryStoreItemOBJ.OperationType = OperationType;
                                                InventoryStoreItemOBJ.Balance = (double)ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.Balance1 = ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.InvenoryStoreLocationId = StoreLocationID;
                                                InventoryStoreItemOBJ.ExpDate = ExpDate;
                                                InventoryStoreItemOBJ.ItemSerial = ItemDataOBJ.Serial;
                                                InventoryStoreItemOBJ.FinalBalance = ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.AddingOrderItemId = AddingOrderItemId;
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
                                            }
                                        }
                                    }

                                    #endregion items
                                    Response.Result = true;
                                    Response.ID = InventoryAddingOrderID;
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

        

        public BaseResponseWithData<string> GetInventoryItemOpeningBalancePOSTemplete(string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();


            var filePath = System.IO.Path.Combine(_host.WebRootPath, "Attachments/Tampletes/InventoryItemOpeningBalancePOSTemplete.xlsx");

            var inventoryItemList = _unitOfWork.InventoryItems.FindAllQueryable(a=> true).Select(a=> new {a.Id, a.Name}).ToList();

            var suppliersList = _unitOfWork.Suppliers.FindAllQueryable(a => a.Active == true).Select(a => new { a.Id, a.Name }).ToList();

            var currenciesList = _unitOfWork.Currencies.FindAllQueryable(a => a.Active == true).Select(a => new { a.Id, a.Name, a.IsLocal }).ToList();

            var inventoryStoersList = _unitOfWork.InventoryStores.FindAllQueryable(a => a.Active == true).Select(a => new { a.Id, a.Name }).ToList();

            var inventoryItems = _unitOfWork.InventoryUoms.FindAllQueryable(a => a.Active == true).Select(a => new { a.Id, a.Name, a.ShortName }).ToList();
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    ExcelPackage package = new ExcelPackage(new FileInfo(filePath));
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    //fill the list of InventoryItemCategory in Excel file to let the user choose from them
                    
                    for (int row = 0; row < inventoryItemList.Count(); row++)
                    {

                        sheet.Cells[row + 2, 1].Value = inventoryItemList[row].Name;
                        sheet.Cells[row + 2, 2].Value = inventoryItemList[row].Id;
                        
                    }


                    //fill the list of RequstionUOM and PurchasingUOM in Excel file to let the user choose from them
                    
                    for (int row = 0; row < suppliersList.Count(); row++)
                    {
                        sheet.Cells[row+2, 4].Value = suppliersList[row].Name;
                        sheet.Cells[row+2, 5].Value = suppliersList[row].Id;
                        
                    }
                    
                    for (int row = 0; row < currenciesList.Count(); row++)
                    {
                        sheet.Cells[row + 2, 7].Value = currenciesList[row].Name;
                        sheet.Cells[row + 2, 8].Value = currenciesList[row].Id;
                        
                    }

                    
                    for (int row = 0; row < inventoryStoersList.Count(); row++)
                    {
                        sheet.Cells[row + 2, 10].Value = inventoryStoersList[row].Name;
                        sheet.Cells[row + 2, 11].Value = inventoryStoersList[row].Id;
                        
                    }
                    
                    for(int row = 0;row < inventoryItems.Count(); row++)
                    {
                        sheet.Cells[row + 2, 13].Value = inventoryItems[row].Name;
                        sheet.Cells[row + 2, 14].Value = inventoryItems[row].Id;
                        
                    }

                    var dirPath = System.IO.Path.Combine(_host.WebRootPath, $"Attachments\\{CompName}\\resultTampletes\\");
                    Directory.CreateDirectory(dirPath);

                    var finalFilePath = dirPath + $"resultInventoryItemOpeningBalancePOSTemplete.xlsx";

                    if (File.Exists(finalFilePath))
                        File.Delete(finalFilePath);

                    package.SaveAs(finalFilePath);
                    package.Dispose();
                    var fixedPath = $"Attachments/{CompName}/resultTampletes/resultInventoryItemOpeningBalancePOSTemplete.xlsx";

                    response.Data = Globals.baseURL + fixedPath;
                    return response;
                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "The Templete File is not exsists";
                    response.Errors.Add(error);
                    return response;
                }
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

        public BaseResponseWithMessage<string> UploadInventoryItemOpeningBalancePOSExcel(AddAttachment dto, long UserID)
        {
            var response = new BaseResponseWithMessage<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            var UploadInventoryItemExcel = new UploadInventoryItemExcelResponse();
            //response.Data = UploadInventoryItemExcel;
            //response.Data.ListOfAddedIDs = new List<long>();
            var ExcelErrorList = new List<ExcelInventoryItemErrors>();
            var inventoryItemOpeningBalanceRequest = new AddInventoryItemOpeningBalanceRequest();

            var inventoryItemOpeningBalanceHeadList = new List<InventoryItemOpeningBalanceHead>();        //here is supplier ID

            var inventoryItemList = new InventoryItemOpeningBalance();

            var listOfValidGroups = new List<InventoryItemOpeningBalancePOSExcelParsedHelper>();

            try
            {
                //----------------------log file Info. ------------------------------------------
                int FailedCount = 0;
                int SuccessCount = 0;
                int totalRows = 0;

                var cuurency = _unitOfWork.Currencies.Find(a => a.IsLocal == true);


                using (var stream = dto.Content.OpenReadStream())       //just read the file without saving it  on the disk

                using (var package = new ExcelPackage(stream))          //walking through the file
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[0];

                    int rows = sheet.Dimension.Rows;
                    int columns = sheet.Dimension.Columns;

                    
                    var dimension = sheet.Dimension;

                    if (dimension != null)
                    {
                        // Read only non-empty rows into a list of string arrays
                        var data = Enumerable.Range(3, dimension.Rows - 1) // Start from row 2 (skip headers)
                                        .Select(row => new InventoryItemOpeningBalancePOSExcelHelper
                                        {
                                                SupplierID = sheet.Cells[row, 2].Text, // Assuming column 1 is "Category"
                                                InventoryItemID = sheet.Cells[row, 4].Text,
                                                UORQYT = sheet.Cells[row, 5].Text,
                                                UORUnit = sheet.Cells[row, 7].Text,//t
                                                UOPQYT = sheet.Cells[row, 8].Text,
                                                UOPUnit = sheet.Cells[row, 10].Text,
                                                ConvRate = sheet.Cells[row, 11].Text,
                                                UnitPrice = sheet.Cells[row, 12].Text,
                                                UnitPriceUOR = sheet.Cells[row, 13].Text,
                                                CurrencyId = sheet.Cells[row, 15].Text,
                                                RateToLocal = sheet.Cells[row, 16].Text,
                                                Serial = sheet.Cells[row, 17].Text,
                                                ExpDate = sheet.Cells[row, 18].Text,
                                                RecevingData = sheet.Cells[row, 19].Text,
                                                InventoryStoreID = sheet.Cells[row, 21].Text,
                                                customPrice = sheet.Cells[row, 22].Text,
                                                Price1 = sheet.Cells[row, 23].Text,
                                                Price2 = sheet.Cells[row, 24].Text,
                                                Price3 = sheet.Cells[row, 25].Text,
                                                Cost = sheet.Cells[row, 26].Text
                                        })
                            .Where(rowData => rowData.SupplierID != "#N/A" ) // Ignore rows with empty Category
                            .ToList();

                        //var groupes = data.GroupBy(a => new { a.SupplierID, a.InventoryStoreID, a.RecevingData });


                        if(data.Count() == 0)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err103";
                            error.ErrorMSG = "the file you uploaded is Empty";
                            response.Errors.Add(error);
                            return response;
                        }

                        int Count = 1;
                        foreach (var recored in data)
                        {
                            var valiadGroup = new InventoryItemOpeningBalancePOSExcelParsedHelper();
                            bool takeThisRecored = true;

                            long supplierID = 0;
                            if(long.TryParse(recored.SupplierID,out supplierID))
                            {
                                valiadGroup.SupplierID = supplierID;
                            }
                            else
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid supplier ID (integer) at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            long InventoryItemID = 0;
                            if(long.TryParse(recored.InventoryItemID,out InventoryItemID))
                            {
                                valiadGroup.InventoryItemID = InventoryItemID;
                            }
                            else
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid InventoryItem ID (integer ) at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            DateTime RecevingData = DateTime.Now;
                            if(DateTime.TryParse(recored.RecevingData,out RecevingData))
                            {
                                valiadGroup.RecevingData = RecevingData;
                            }
                            else
                            {
                                //takeThisRecored = false;
                                //var error = new ExcelInventoryItemErrors()
                                //{
                                //    ErrMsg = $"please Enter a valid RecevingData at record number {Count }",
                                //    NoOfRow = Count,
                                //};
                                //ExcelErrorList.Add(error);
                                RecevingData = DateTime.Now;
                            }

                            decimal unitPrice = 0.0m;
                            if (!string.IsNullOrWhiteSpace(recored.UnitPrice))
                            {
                                if (decimal.TryParse(recored.UnitPrice, out unitPrice))
                                {
                                    valiadGroup.UnitPrice = unitPrice;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid unitPrice  (decimal ) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }
                            

                            int currencyID = 0;         //need to handle IsLocal and rate to laocal
                            if (int.TryParse(recored.CurrencyId, out currencyID))
                            {
                                if((currencyID != cuurency.Id) && string.IsNullOrWhiteSpace(recored.RateToLocal) )
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid Conv. Rate or enter price in local currency at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                                valiadGroup.CurrencyId = currencyID;
                            }
                            else
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid currencyID  (integer) at record number {Count }",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            decimal rateToLocal = 0.0m;
                            if (decimal.TryParse(recored.RateToLocal, out rateToLocal))
                            {
                                valiadGroup.RateToLocal = rateToLocal;
                            }
                            else
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid RateToLocal  (decimal ) at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            string serial = "";
                            if (!string.IsNullOrWhiteSpace(recored.Serial))
                            {
                                valiadGroup.Serial = serial;
                            }
                            else
                            {
                                //takeThisRecored = false;
                                //var error = new ExcelInventoryItemErrors()
                                //{
                                //    ErrMsg = $"please Enter a valid serial at record number {Count }",
                                //    NoOfRow = Count,
                                //};
                                //ExcelErrorList.Add(error);
                                serial = null;
                            }

                            DateTime expDate = DateTime.Now;
                            if (DateTime.TryParse(recored.ExpDate, out expDate))
                            {
                                valiadGroup.ExpDate = expDate;
                            }
                            //else
                            //{
                            //    //takeThisRecored = false;
                            //    //var error = new ExcelInventoryItemErrors()
                            //    //{
                            //    //    ErrMsg = $"please Enter a valid ExpDate at record number {Count}",
                            //    //    NoOfRow = Count,
                            //    //};
                            //    //ExcelErrorList.Add(error);
                                
                            //}

                            int inventoryStoreID = 0;
                            if (int.TryParse(recored.InventoryStoreID, out inventoryStoreID))
                            {
                                valiadGroup.InventoryStoreID = inventoryStoreID;
                            }
                            else
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid inventoryStoreID  (long ) at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }


                            if(string.IsNullOrWhiteSpace(recored.UOPQYT) && string.IsNullOrWhiteSpace(recored.UORQYT))
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid UOP QYT or UOR QYT at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            if (!string.IsNullOrWhiteSpace(recored.UOPQYT) && !string.IsNullOrWhiteSpace(recored.UORQYT))
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"you can only enter UOP QYT or UOR QYT at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            if (string.IsNullOrWhiteSpace(recored.UOPUnit) && string.IsNullOrWhiteSpace(recored.UORUnit))
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid UOP unit or UOR unit at record number {Count}",
                                    NoOfRow = Count,
                                };
                                ExcelErrorList.Add(error);
                            }

                            int UOPQYT = 0;
                            if(!string.IsNullOrWhiteSpace(recored.UOPQYT))
                            {

                                if (int.TryParse(recored.UOPQYT, out UOPQYT))
                                {
                                    valiadGroup.UOPQYT = UOPQYT;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid UOPQYT  (long ) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            long UOPUnit = 0;
                            if (!string.IsNullOrWhiteSpace(recored.UOPUnit) && recored.UOPUnit != "#N/A")
                            {

                                if (long.TryParse(recored.UOPUnit, out UOPUnit))
                                {
                                    valiadGroup.UOPUnit = UOPUnit;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid UOPUnit  (long ) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            int UORQYT = 0;
                            if(!string.IsNullOrWhiteSpace(recored.UORQYT))
                            {

                                if (int.TryParse(recored.UORQYT, out UORQYT))
                                {
                                    valiadGroup.UORQYT = UORQYT;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid UOPQYT  (long ) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            
                            long UORUnit = 0;
                            if (!string.IsNullOrWhiteSpace(recored.UORUnit) && recored.UORUnit  != "#N/A")
                            {

                                if (long.TryParse(recored.UORUnit, out UORUnit))
                                {
                                    valiadGroup.UORUnit = UORUnit;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid UORUnit  (long ) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            decimal unitPriceUOP = 0;
                            if (!string.IsNullOrWhiteSpace(recored.UnitPrice) && recored.UOPUnit != "#N/A")
                            {

                                if (decimal.TryParse(recored.UnitPrice, out unitPriceUOP))
                                {
                                    valiadGroup.UnitPrice = unitPriceUOP;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid unitPrice  (long) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            decimal unitPriceUOR = 0;
                            if (!string.IsNullOrWhiteSpace(recored.UnitPriceUOR))
                            {

                                if (decimal.TryParse(recored.UnitPriceUOR, out unitPriceUOP))
                                {
                                    valiadGroup.UnitPriceUOR = unitPriceUOR;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid unitPriceUOR  (long) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            //--------------------------------------------customPrice, Price1, price2, price3--------------------------------
                            decimal customPrice = 0;
                            if (!string.IsNullOrWhiteSpace(recored.customPrice))
                            {

                                if (decimal.TryParse(recored.customPrice, out customPrice))
                                {
                                    valiadGroup.customPrice = customPrice;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid customPrice  (decimal) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            decimal price1 = 0;
                            if (!string.IsNullOrWhiteSpace(recored.Price1))
                            {

                                if (decimal.TryParse(recored.Price1, out price1))
                                {
                                    valiadGroup.Price1 = price1;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid Price1  (decimal) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            decimal price2 = 0;
                            if (!string.IsNullOrWhiteSpace(recored.Price2))
                            {

                                if (decimal.TryParse(recored.Price2, out price2))
                                {
                                    valiadGroup.Price2  = price2;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid Price2  (decimal) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            decimal price3 = 0;
                            if (!string.IsNullOrWhiteSpace(recored.Price3))
                            {

                                if (decimal.TryParse(recored.Price3, out price3))
                                {
                                    valiadGroup.Price3 = price3;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid Price3  (decimal) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }

                            decimal Cost = 0;
                            if (!string.IsNullOrWhiteSpace(recored.Cost))
                            {

                                if (decimal.TryParse(recored.Cost, out Cost))
                                {
                                    valiadGroup.Cost = Cost;
                                }
                                else
                                {
                                    takeThisRecored = false;
                                    var error = new ExcelInventoryItemErrors()
                                    {
                                        ErrMsg = $"please Enter a valid Cost  (decimal) at record number {Count}",
                                        NoOfRow = Count,
                                    };
                                    ExcelErrorList.Add(error);
                                }
                            }
                            //------------------------------------------End customPrice, Price1, price2, price3------------------------------

                            if (takeThisRecored)
                            {
                                listOfValidGroups.Add(valiadGroup);
                                SuccessCount++;
                            }

                            Count++;
                        }
                        totalRows = Count;
                    }
                    var groupes = listOfValidGroups.GroupBy(a => new { a.SupplierID, a.InventoryStoreID, a.RecevingData });


                    var request = new AddInventoryItemOpeningBalanceRequest();
                    var listInReq = new List<InventoryItemOpeningBalanceHead>();                    //supplierId is Here
                    //var InventoryItemOpeningBalanceList = new List<InventoryItemOpeningBalance>();


                    foreach (var group in groupes)
                    {
                        var inventoryItemOpeningBalanceHead = new InventoryItemOpeningBalanceHead();
                        var listInHead = new List<InventoryItemOpeningBalance>();
                        inventoryItemOpeningBalanceHead.ItemList = listInHead;
                        inventoryItemOpeningBalanceHead.SupplierId = group.Key.SupplierID;
                        inventoryItemOpeningBalanceHead.RecevingData = group.Key.RecevingData.ToString();
                        inventoryItemOpeningBalanceHead.InventoryStoreID = group.Key.InventoryStoreID;

                        double convesionRate = 0;
                        foreach (var recored in group)
                        {
                            var itemOpeningBalance = new InventoryItemOpeningBalance();
                            itemOpeningBalance.InventoryItemID = recored.InventoryItemID;
                            itemOpeningBalance.ExpData = recored.ExpDate.ToString();
                            itemOpeningBalance.Serial = recored.Serial;
                            itemOpeningBalance.CurrencyId = recored.CurrencyId;
                            itemOpeningBalance.RateToEGP = recored.RateToLocal;

                            var convRate = _unitOfWork.InventoryItems.GetById(recored.UOPUnit)?.ExchangeFactor1??1;
                            convesionRate = (double)convRate;
                            if(recored.UOPQYT != 0)
                            {
                                itemOpeningBalance.QTY = recored.UOPQYT * (decimal)convRate;
                            }
                            if(recored.UORQYT  != 0)
                            {
                                itemOpeningBalance.QTY = recored.UORQYT;
                            }
                            if(recored.UnitPriceUOR > 0 && recored.UOPUnit == 0)
                            {
                                itemOpeningBalance.CostEGP = recored.UnitPriceUOR;

                            }
                            else
                            {
                                if(convesionRate > 0)
                                {
                                    itemOpeningBalance.CostEGP = recored.UnitPrice / (decimal)convesionRate;
                                }
                                //itemOpeningBalance.CostEGP = recored.;
                            }

                            listInHead.Add(itemOpeningBalance);
                        }
                        //inventoryItemOpeningBalanceHead.ItemList.AddRange(listInHead);
                        listInReq.Add(inventoryItemOpeningBalanceHead);

                    }
                    request.InventoryItemOpeningBalanceHeadList = listInReq;
                    var dataAddedToDB = AddInventoryItemOpeningBalancePOS(request, UserID);
                    //if (!dataAddedToDB.Result)
                    //{
                    //    response.Result = false;

                    //    foreach (var error in dataAddedToDB.Errors)
                    //    {
                    //        var errors = new ExcelInventoryItemErrors();
                    //        errors.ErrMsg = error.ErrorMSG;
                    //        ExcelErrorList.Add(errors);
                    //    }
                    //}
                    //var listOfIDs = new List<long>();
                    //if(dataAddedToDB.ID != 0)
                    //{
                    //    listOfIDs.Add(dataAddedToDB.ID);
                    //}
                    //response.Data.ListOfAddedIDs = listOfIDs;
                    var itemsList = new List<InventoryItemCustomPriceAdding>();
                    foreach (var item in listOfValidGroups)
                    {
                        var itemNewData = new InventoryItemCustomPriceAdding();
                        itemNewData.InventoryItemID = item.InventoryItemID;
                        itemNewData.customPrice = item.customPrice;
                        itemNewData.Price1 = item.Price1;
                        itemNewData.Price2 = item.Price2;
                        itemNewData.Price3 = item.Price3;
                        itemNewData.Cost = item.Cost;
                        itemsList.Add(itemNewData);
                        
                    }
                    var itemsIDs = itemsList.Select(a => a.InventoryItemID).ToList();
                    var inventoryitemsDB = _unitOfWork.InventoryItems.FindAll(a => itemsIDs.Contains(a.Id));

                    foreach (var item in itemsList)
                    {
                        var itemNewData = inventoryitemsDB.Where(a => a.Id == item.InventoryItemID).FirstOrDefault();
                        if(itemNewData != null)
                        {
                            itemNewData.CustomeUnitPrice = item.customPrice;
                            itemNewData.CostAmount1 = item.Price1;
                            itemNewData.CostAmount2 = item.Price2;
                            itemNewData.CostAmount3 = item.Price3;
                            itemNewData.ModifiedBy = UserID;
                            itemNewData.ModifiedDate = DateTime.Now;
                            //itemNewData.CreationDate = DateTime.Now;
                            //_unitOfWork.InventoryItems.Update(itemNewData);
                        }
                    }
                    _unitOfWork.Complete();

                    var inventoryStoreDB = _unitOfWork.InventoryStoreItems.FindAll(a => itemsIDs.Contains(a.InventoryItemId));
                    foreach (var itemlist in itemsList)
                    {
                        var itemNewData = inventoryStoreDB.Where(a => a.InventoryItemId == itemlist.InventoryItemID).ToList();
                        if (itemNewData != null)
                        {
                            foreach (var item in itemNewData)
                            {
                                if(item.PoinvoiceTotalCostEgp  == null  && item.PoinvoiceTotalPriceEgp == null )
                                {
                                    item.PoinvoiceTotalCostEgp = itemlist.Cost;
                                    item.PoinvoiceTotalPriceEgp = itemlist.Cost;
                                    item.ModifiedBy = UserID;
                                    item.ModifiedDate = DateTime.Now;

                                }
                            }

                            //itemNewData.CreationDate = DateTime.Now;
                            //_unitOfWork.InventoryItems.Update(itemNewData);
                        }
                    }
                    _unitOfWork.Complete();
                }

                if (ExcelErrorList.Count() > 0)
                {
                    string filePath = System.IO.Path.Combine(_host.WebRootPath, "Attachments/Tampletes/InventoryItemOpeningBalancePOS.txt"); ;
                 
                    // Create a StreamWriter and write lines to the file
                    using (StreamWriter writer = new StreamWriter(filePath, append: false)) // `append: true` to add to an existing file
                    {
                        writer.WriteLine("Success Rows: " + SuccessCount);
                        writer.WriteLine("Failed Rows: " + ((totalRows - 1) - SuccessCount));
                        writer.WriteLine("Total Rows: " + (totalRows-1) + "\n");
                        foreach (var error in ExcelErrorList)
                        {
                            writer.WriteLine($"Number of row: " + error.NoOfRow + ", " + "Error message: " + error.ErrMsg);
                        }

                    }
                }



                string fixedPath = "Attachments/Tampletes/InventoryItemOpeningBalancePOS.txt";

                


                //UploadInventoryItemExcel.ListOfAddedIDs = listOfIDAddedToDB;
                //UploadInventoryItemExcel.ErrorFilePath = Globals.baseURL + fixedPath;
                response.Message = Globals.baseURL + fixedPath;
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

        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePOS(AddInventoryItemOpeningBalanceRequest Request, long UserID)
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.InventoryItemOpeningBalanceHeadList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    int Counter = 0;

                    // All Inventory Item  From Request
                    var IDSInvItemList = Request.InventoryItemOpeningBalanceHeadList.SelectMany(x => x.ItemList.Select(y => y.InventoryItemID)).ToList();
                    var InventoryItemListDB = _unitOfWork.InventoryItems.FindAll(x => IDSInvItemList.Contains(x.Id)).ToList();
                    foreach (var item in Request.InventoryItemOpeningBalanceHeadList)
                    {
                        Counter++;

                        if (item.InventoryStoreID == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err24";
                            error.ErrorMSG = "Invalid Inventory Store ID #" + Counter;
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (item.ItemList == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-14";
                            error.ErrorMSG = "please insert at least one Item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        else if (item.ItemList.Count <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-14";
                            error.ErrorMSG = "please insert at least one Item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        long SupplierID = 0;
                        if (item.SupplierId != null)
                        {
                            SupplierID = (long)item.SupplierId;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Supplier ID #" + Counter;
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (item.ItemList != null)
                        {
                            var OpeningBalanceItemList = item.ItemList;
                            var InventoryItemList = OpeningBalanceItemList.Where(x => x.InventoryItemID == 0).ToList();
                            var InventoryItemQTYList = OpeningBalanceItemList.Where(x => x.QTY <= 0).ToList();
                            if (InventoryItemList.Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err28";
                                error.ErrorMSG = "Invalid Inventory Item ID Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }

                            if (InventoryItemQTYList.Count() > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err29";
                                error.ErrorMSG = "Invalid Inventory Item QTY item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            var CheckIfInventoryItemExist = InventoryItemListDB.Where(x => OpeningBalanceItemList.Select(y => y.InventoryItemID).ToList().Contains(x.Id)).Count();
                            if (CheckIfInventoryItemExist != OpeningBalanceItemList.Count())
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err28";
                                error.ErrorMSG = "Invalid Inventory Item ID item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }


                    }







                    if (Response.Result)
                    {

                        // Loop List  Of Inventory Store with each Items

                        var ListOfInventoryStoreList = Request.InventoryItemOpeningBalanceHeadList;
                        if (ListOfInventoryStoreList.Count() > 0)
                        {
                            string OperationType = "Opening Balance";

                            #region comment
                            //string SupplierName = "DIRECT PR HIDDEN SUPPLIER";
                            //long SupplierID = Common.GetSupplierID(SupplierName);
                            //if (SupplierID == 0)
                            //{
                            //    // Insert supplier
                            //    ObjectParameter IDSupplier = new ObjectParameter("ID", typeof(long));
                            //    var SupplierInsertion = _Context.proc_SupplierInsert(IDSupplier,
                            //                                                         SupplierName,
                            //                                                         "Supplier",
                            //                                                         null, null,
                            //                                                         validation.userID,
                            //                                                         DateTime.Now,
                            //                                                         null, null, null, null, null,
                            //                                                         true,
                            //                                                         null, null, null, null, null,
                            //                                                         null, null, null, null, null, null, null
                            //                                                         );
                            //    if (SupplierInsertion > 0)
                            //    {
                            //        SupplierID = (long)IDSupplier.Value;
                            //    }

                            //}
                            //if (SupplierID == 0)
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err23";
                            //    error.ErrorMSG = "Invalid Supplier ID.";
                            //    Response.Errors.Add(error);
                            //    return Response;
                            //}
                            #endregion

                            foreach (var ItemOpeningBalancePerStore in ListOfInventoryStoreList)
                            {
                                // Check Inventory Report Approved and closed or not
                                var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == ItemOpeningBalancePerStore.InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();

                                if (CheckInventoryReportListDB.Count > 0)
                                {
                                    foreach (var InventoryRep in CheckInventoryReportListDB)
                                    {
                                        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                        {
                                            string storeName = _unitOfWork.InventoryStores.GetById(ItemOpeningBalancePerStore.InventoryStoreID).Name;//Common.getInventoryStoreName(ItemOpeningBalancePerStore.InventoryStoreID);
                                            string errMsg = "Note : Store " + storeName +
                                                " is under inventory from " +
                                                InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
                                                " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

                                            Response.Result = true; // Warning
                                            Error error = new Error();
                                            error.ErrorCode = "Err-44";
                                            error.ErrorMSG = errMsg;
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                                // ------------------------------------------------
                                DateTime RecevingData = DateTime.Now;
                                if (!string.IsNullOrEmpty(ItemOpeningBalancePerStore.RecevingData))
                                {
                                    if (string.IsNullOrEmpty(ItemOpeningBalancePerStore.RecevingData) || !DateTime.TryParse(ItemOpeningBalancePerStore.RecevingData, out RecevingData))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err23";
                                        error.ErrorMSG = "Invalid Receving Data.";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }
                                }

                                long InventoryAddingOrderID = 0;
                                // Insertion Adding Order
                                //ObjectParameter IDInventoryAddingOrder = new ObjectParameter("ID", typeof(long));
                                //var InventoryAddingOrderInsertion = _Context.proc_InventoryAddingOrderInsert(IDInventoryAddingOrder, OperationType, DateTime.Now, validation.userID, 0, ItemOpeningBalancePerStore.SupplierId,
                                //                                                                             null, 0, ItemOpeningBalancePerStore.InventoryStoreID, RecevingData, validation.userID, DateTime.Now);



                                var newInventroyOrder = new InventoryAddingOrder();
                                newInventroyOrder.OperationType = OperationType;
                                newInventroyOrder.CreationDate = DateTime.Now;
                                newInventroyOrder.CreatedBy = UserID;
                                newInventroyOrder.Revision = 0;
                                newInventroyOrder.SupplierId = (long)ItemOpeningBalancePerStore.SupplierId;
                                newInventroyOrder.Ponumber = null;
                                newInventroyOrder.LoadBy = 0;
                                newInventroyOrder.InventoryStoreId = ItemOpeningBalancePerStore.InventoryStoreID;
                                newInventroyOrder.RecivingDate = RecevingData;
                                newInventroyOrder.ModifiedBy = UserID;
                                newInventroyOrder.ModifiedDate = DateTime.Now;

                                var InventoryAddingOrderInsertion = _unitOfWork.InventoryAddingOrders.Add(newInventroyOrder);
                                _unitOfWork.Complete();

                                if (InventoryAddingOrderInsertion != null)
                                {
                                    InventoryAddingOrderID = InventoryAddingOrderInsertion.Id;
                                    #region items


                                    foreach (var ItemDataOBJ in ItemOpeningBalancePerStore.ItemList)
                                    {
                                        var InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(Item => Item.Id == ItemDataOBJ.InventoryItemID).FirstOrDefault();
                                        if (InventoryItemObjDB != null)
                                        {

                                            DateTime? ExpDate = null;
                                            if (ItemDataOBJ.ExpData != null && ItemDataOBJ.ExpData != "")
                                            {
                                                ExpDate = DateTime.Parse(ItemDataOBJ.ExpData);
                                            }
                                            //add new 
                                            //ObjectParameter IDInventoryAddingOrderItem = new ObjectParameter("ID", typeof(long));
                                            //var AddingOrderItemInsertion = _Context.Myproc_InventoryAddingOrderItemsInsert_New(IDInventoryAddingOrderItem, InventoryAddingOrderID, ItemDataOBJ.InventoryItemID,
                                            //                                                                             InventoryItemObjDB.RequstionUomid,
                                            //                                                                             // null,null,// (float)MatrialDataOBJ.AddedQTYUOR, //ReqQuantity,
                                            //                                                                             //(float)MatrialDataOBJ.AddedQTYUOR,
                                            //                                                                             ExpDate,
                                            //                                                                             ItemDataOBJ.Serial, null, ItemDataOBJ.Comment, null,
                                            //                                                                                null,// PO Req QTY
                                            //                                                                                ItemDataOBJ.QTY,
                                            //                                                                                null,
                                            //                                                                                null
                                            //                                                                             );

                                            var newInventoryItem = new InventoryAddingOrderItem();
                                            newInventoryItem.InventoryAddingOrderId = InventoryAddingOrderID;
                                            newInventoryItem.InventoryItemId = ItemDataOBJ.InventoryItemID;
                                            newInventoryItem.Uomid = InventoryItemObjDB.RequstionUomid;
                                            newInventoryItem.ExpDate = ExpDate;
                                            newInventoryItem.ItemSerial = ItemDataOBJ.Serial;
                                            newInventoryItem.QcreportId = null;
                                            newInventoryItem.Comments = ItemDataOBJ.Comment;
                                            newInventoryItem.ReqQuantity1 = (decimal)ItemDataOBJ.QTY;
                                            newInventoryItem.RecivedQuantity1 = (decimal)ItemDataOBJ.QTY;
                                            newInventoryItem.RecivedQuantityAfter = (decimal)ItemDataOBJ.QTY;
                                            newInventoryItem.RemainQuantity = null;


                                            var AddingOrderItemInsertion = _unitOfWork.InventoryAddingOrderItems.Add(newInventoryItem);
                                            _unitOfWork.Complete();

                                            if (AddingOrderItemInsertion != null)
                                            {
                                                long AddingOrderItemId = AddingOrderItemInsertion.Id;
                                                int? StoreLocationID = _unitOfWork.InventoryStoreLocations.Find(a => a.Id == ItemDataOBJ.StoreLocationID) != null ? ItemDataOBJ.StoreLocationID : null;//Common.CheckInventoryStoreLocationID(ItemDataOBJ.StoreLocationID) ? ItemDataOBJ.StoreLocationID : null;

                                                // Extra Date For PO Items
                                                long? POInvoiceId = null;
                                                decimal? POInvoiceTotalPrice = null;
                                                decimal? POInvoiceTotalCost = null;
                                                int? currencyId = ItemDataOBJ.CurrencyId ?? _unitOfWork.Currencies.Find(a => a.IsLocal == true)?.Id ?? 1;
                                                decimal? rateToEGP = 1;
                                                decimal? POInvoiceTotalPriceEGP = null;
                                                decimal? POInvoiceTotalCostEGP = null;
                                                decimal? remainItemPrice = null;
                                                decimal? remainItemCosetEGP = null;
                                                decimal? remainItemCostOtherCU = null;


                                                decimal finalBalance = ItemDataOBJ.QTY;
                                                rateToEGP = ItemDataOBJ.RateToEGP ?? 1;
                                                POInvoiceTotalPriceEGP = ItemDataOBJ.CostEGP ?? 0;
                                                POInvoiceTotalCostEGP = ItemDataOBJ.CostEGP ?? 0;
                                                POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                var InventoryStoreItemOBJ = new InventoryStoreItem();
                                                InventoryStoreItemOBJ.InventoryStoreId = ItemOpeningBalancePerStore.InventoryStoreID;
                                                InventoryStoreItemOBJ.InventoryItemId = ItemDataOBJ.InventoryItemID;
                                                InventoryStoreItemOBJ.OrderNumber = InventoryAddingOrderID.ToString();
                                                InventoryStoreItemOBJ.OrderId = InventoryAddingOrderID;
                                                InventoryStoreItemOBJ.CreatedBy = UserID;
                                                InventoryStoreItemOBJ.ModifiedBy = UserID;
                                                InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                InventoryStoreItemOBJ.OperationType = OperationType;
                                                InventoryStoreItemOBJ.Balance = (double)ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.Balance1 = (decimal)ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.InvenoryStoreLocationId = StoreLocationID;
                                                InventoryStoreItemOBJ.ExpDate = ExpDate;
                                                InventoryStoreItemOBJ.ItemSerial = ItemDataOBJ.Serial;
                                                InventoryStoreItemOBJ.FinalBalance = ItemDataOBJ.QTY;
                                                InventoryStoreItemOBJ.AddingOrderItemId = AddingOrderItemId;
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
                                                _unitOfWork.Complete();


                                                //// Check if Cost greater than Custom inventory Item change it
                                                //if (InventoryItemObjDB.CustomeUnitPrice < POInvoiceTotalPriceEGP)
                                                //{
                                                //    if (POInvoiceTotalPriceEGP != null)
                                                //    {
                                                //        InventoryItemObjDB.CustomeUnitPrice = (decimal)POInvoiceTotalPriceEGP;
                                                //    }
                                                //}
                                                //var InventoryStorItemInsertion = _unitOfWork.Complete();
                                            }
                                        }
                                    }
                                    #endregion items
                                    Response.Result = true;
                                    Response.ID = InventoryAddingOrderID;
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

        public BaseResponseWithId<long> UpdateInventoryItemCost(long inventoryStoreItemID, decimal Cost)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            #region validation
            if(inventoryStoreItemID == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err-P12";
                error.ErrorMSG = "please insert a valid inventory store ID.";
                Response.Errors.Add(error);
                return Response;
            }

            if (Cost == 0)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err-P12";
                error.ErrorMSG = "please insert a valid inventory cost";
                Response.Errors.Add(error);
                return Response;
            }
            #endregion

            try
            {
                var inventoryStoreItem = _unitOfWork.InventoryStoreItems.Find(a => a.Id == inventoryStoreItemID);

                if (inventoryStoreItem != null)
                {
                    //remove the condition on 15-12-2024 (momkn yrg3)
                    //if(inventoryStoreItem.PoinvoiceTotalCostEgp == null && inventoryStoreItem.PoinvoiceTotalPriceEgp == null) 
                    //{
                        inventoryStoreItem.PoinvoiceTotalPriceEgp = Cost;
                        inventoryStoreItem.PoinvoiceTotalCostEgp = Cost;

                        inventoryStoreItem.RemainItemCosetEgp = inventoryStoreItem.FinalBalance * Cost;
                        inventoryStoreItem.RemainItemPrice = inventoryStoreItem.FinalBalance * Cost;
                        _unitOfWork.Complete();
                    //}
                    Response.ID = inventoryStoreItem.Id;
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
