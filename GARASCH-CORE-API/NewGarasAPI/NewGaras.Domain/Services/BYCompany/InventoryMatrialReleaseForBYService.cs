using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.Inventory.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using NewGaras.Infrastructure.Helper;
using Microsoft.IdentityModel.Tokens;

namespace NewGaras.Domain.Services.BYCompany
{
    public class InventoryMatrialReleaseForBYService : IInventoryMatrialReleaseForBYService
    {
        private readonly IMapper _mapper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;

        public InventoryMatrialReleaseForBYService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }
        public BaseResponseWithID AddInventoryItemMatrialRelease([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest,long creator)
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
                    if (BodyRequest == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long FromUserId = 0;
                    int InventoryStoreID = 0;
                    if (BodyRequest.FromUserId != 0)
                    {
                        FromUserId = BodyRequest.FromUserId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err45";
                        error.ErrorMSG = "Invalid From User ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.InventoryStoreID != 0)
                    {
                        var CheckInventorystorDB = _unitOfWork.InventoryStores.FindAll(x => x.Id == BodyRequest.InventoryStoreID).FirstOrDefault();
                        if (CheckInventorystorDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err24";
                            error.ErrorMSG = "Invalid Inventory Store ID, not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        InventoryStoreID = BodyRequest.InventoryStoreID;
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
                    if (string.IsNullOrWhiteSpace(BodyRequest.Status))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Status is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemWithRequestItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemWithRequestItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemWithRequestItemList.Where(x => x.NewRecivedQTY == null).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-416";
                        error.ErrorMSG = "New Recived Qty required for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    int Counter = 0;
                    var IDSMatrialRequest = BodyRequest.MatrialReleaseItemWithRequestItemList.Select(x => x.InventoryItemID).Distinct().ToList();
                    var ListInventoryItems = _unitOfWork.InventoryItems.FindAll(x => IDSMatrialRequest.Contains(x.Id)).ToList();
                    var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSMatrialRequest.Contains(x.InventoryItemId)).ToList();
                    foreach (var item in BodyRequest.MatrialReleaseItemWithRequestItemList)
                    {
                        string InventoryItemName = "";
                        Counter++;
                        if (item.NewRecivedQTY == null || item.NewRecivedQTY <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-416";
                            error.ErrorMSG = "Invalid Recived QTY Selected on Item NO#" + Counter;
                            Response.Errors.Add(error);
                        }

                        if (item.InventoryItemID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err47";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        else // Check is Inventoryt Item ID is valid
                        {
                            var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == item.InventoryItemID).FirstOrDefault();
                            if (InventoryItemObjDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                InventoryItemName = InventoryItemObjDB.Name;
                            }
                        }


                        var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                (long)item.InventoryItemID,
                                                                InventoryStoreID,
                                                                null,// store location
                                                                (decimal)item.NewRecivedQTY,
                                                                item.IsFIFO);
                        //if (item.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
                        //{
                        //}

                        //List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = item.StockBalanceList;// Case 1 : Request ParentReleaseItemID
                        //                                                                                     // Calc Parent Release , Final Balance After Release


                        List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = AvailableItemStockList;

                        if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < item.NewRecivedQTY)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err325";
                            error.ErrorMSG = "Not have availble qty from Item Name " + InventoryItemName;
                            Response.Errors.Add(error);
                            //return Response;
                        }
                    }




                    #endregion

                    if (Response.Result)
                    {
                        /*
                         ###################################################################
                        #####################################################################
                        ################ Create New Matrial Request #########################
                        #####################################################################
                        #####################################################################
                         */
                        var MatrialRequest = new InventoryMatrialRequest();
                        MatrialRequest.FromUserId = FromUserId;
                        MatrialRequest.ToInventoryStoreId = InventoryStoreID;
                        MatrialRequest.RequestDate = DateTime.Now;
                        MatrialRequest.RequestTypeId = 20002;
                        MatrialRequest.ApproveResult = BodyRequest.UserInsuranceId?.ToString(); // Column Insurance Id Just for temperary
                        MatrialRequest.Status = "Closed";
                        MatrialRequest.Active = true;
                        MatrialRequest.CreatedBy = creator;
                        MatrialRequest.ModifiedBy = creator;
                        MatrialRequest.ModifiedDate = DateTime.Now;
                        MatrialRequest.CreationDate = DateTime.Now;

                        _unitOfWork.InventoryMatrialRequests.Add(MatrialRequest);
                        var MatrialRequestInsertion = _unitOfWork.Complete();

                        var MatrialReleaseObj = new InventoryMatrialRelease();
                        if (MatrialRequestInsertion > 0)
                        {
                            /*###################################################################
                            #####################################################################
                            ################ Create New Matrial Release #########################
                            #####################################################################
                            #####################################################################
                                */


                            MatrialReleaseObj.ToUserId = FromUserId;
                            MatrialReleaseObj.FromInventoryStoreId = InventoryStoreID;
                            MatrialReleaseObj.RequestDate = DateTime.Now;
                            MatrialReleaseObj.CreationDate = DateTime.Now;
                            MatrialReleaseObj.ModifiedDate = DateTime.Now;
                            MatrialReleaseObj.CreatedBy = creator;
                            MatrialReleaseObj.ModifiedBy = creator;
                            MatrialReleaseObj.Active = true;
                            MatrialReleaseObj.Status = BodyRequest.Status;
                            MatrialReleaseObj.MatrialRequestId = MatrialRequest.Id;

                            _unitOfWork.InventoryMatrialReleases.Add(MatrialReleaseObj);
                            _unitOfWork.Complete();


                            Response.ID = MatrialReleaseObj.Id;
                        }


                        int itemCount = 0;


                        // Get All InventoryStoreItems Used in this Release before loop
                        //var IDSInventoryItemListRequested = MatrialRequestItemListDB.Select(x => x.InventoryItemId).Distinct().ToList();
                        //var IDSSalesOfferProduct = MatrialRequestItemListDB.Select(x => x.OfferItemId).ToList();
                        //var SalesOfferProductsList = _Context.SalesOfferProducts.Where(x => IDSSalesOfferProduct.Contains(x.Id)).ToList();
                        //var POItemList = _Context.PurchasePoitems.Where(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId)).ToList();
                        foreach (var item in BodyRequest.MatrialReleaseItemWithRequestItemList)
                        {
                            itemCount++;

                            //add new 
                            if (item.ID == 0)
                            {
                                var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                                                (long)item.InventoryItemID,
                                                                                                InventoryStoreID,
                                                                                                null,// store location
                                                                                                (decimal)item.NewRecivedQTY,
                                                                                                item.IsFIFO);
                                //if (item.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
                                //{
                                //}

                                //List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = item.StockBalanceList;// Case 1 : Request ParentReleaseItemID
                                //                                                                                     // Calc Parent Release , Final Balance After Release


                                List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = AvailableItemStockList;

                                if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < item.NewRecivedQTY)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err325";
                                    error.ErrorMSG = "Not have availble qty from Store to release on Item NO #" + itemCount;
                                    Response.Errors.Add(error);
                                    //return Response;
                                }



                                if (ParentReleaseWithQTYListID.Count() > 0)
                                {
                                    ParentReleaseWithQTYListID = ParentReleaseWithQTYListID.Where(x => x.StockBalance > 0).ToList();


                                    /* ###################################################################
                                    #####################################################################
                                    ################ Create New Matrial Request Item ####################
                                    #####################################################################
                                    #####################################################################*/

                                    var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == item.InventoryItemID).FirstOrDefault();
                                    if (InventoryItemObjDB != null)
                                    {
                                        //add new 
                                        var InvMatrialRequestItemObjDB = new InventoryMatrialRequestItem();
                                        InvMatrialRequestItemObjDB.InventoryMatrialRequestId = MatrialRequest.Id;
                                        InvMatrialRequestItemObjDB.InventoryItemId = (long)item.InventoryItemID;
                                        InvMatrialRequestItemObjDB.Uomid = InventoryItemObjDB.RequstionUomid;
                                        InvMatrialRequestItemObjDB.Comments = item.NewComment;
                                        InvMatrialRequestItemObjDB.RecivedQuantity1 = item.NewRecivedQTY; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                        InvMatrialRequestItemObjDB.ReqQuantity1 = item.NewRecivedQTY;
                                        InvMatrialRequestItemObjDB.PurchaseQuantity = 0;

                                        _unitOfWork.InventoryMatrialRequestItems.Add(InvMatrialRequestItemObjDB);
                                        _unitOfWork.Complete();


                                        /* ###################################################################
                                        #####################################################################
                                        ################ Create New Matrial Release Item ####################
                                        #####################################################################
                                        #####################################################################*/

                                        var MatrialReleaseItem = new InventoryMatrialReleaseItem();
                                        MatrialReleaseItem.InventoryMatrialReleasetId = MatrialReleaseObj.Id;
                                        MatrialReleaseItem.Comments = item.NewComment;
                                        MatrialReleaseItem.InventoryMatrialRequestItemId = InvMatrialRequestItemObjDB.Id;
                                        MatrialReleaseItem.RecivedQuantity1 = item.NewRecivedQTY;

                                        _unitOfWork.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);

                                        var MatrialReleaseItemInsert = _unitOfWork.Complete();
                                        if (MatrialReleaseItemInsert > 0)
                                        {
                                            long MatrialReleaseItemID = MatrialReleaseItem.Id;
                                            string OperationType = "Release Order";

                                            // List Of Items To remove from Recived QTY
                                            decimal RemainReleaseQTY = (decimal)item.NewRecivedQTY; // 20


                                            // List Of IDS inserted
                                            List<long> ListIDSUpdate = new List<long>();

                                            foreach (var ObjParentRelease in ParentReleaseWithQTYListID)  // 20 -  10   - 5
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
                                                    var ParentInventoryStoreItem = InventoryStoreItemList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
                                                    long? POID = null;
                                                    //long? POInvoiceId = null;
                                                    //decimal? POInvoiceTotalPrice = null;
                                                    //decimal? POInvoiceTotalCost = null;
                                                    //int? currencyId = null;
                                                    //decimal? rateToEGP = null;
                                                    //decimal? POInvoiceTotalPriceEGP = null;
                                                    //decimal? POInvoiceTotalCostEGP = null;
                                                    //decimal? remainItemPrice = null;
                                                    //decimal? remainItemCosetEGP = null;
                                                    //decimal? remainItemCostOtherCU = null;
                                                    //decimal? remainItemCosetEGPForRelease = null;
                                                    //decimal? remainItemCostOtherCUForRelease = null;
                                                    if (ParentInventoryStoreItem != null)
                                                    {
                                                        POID = ParentInventoryStoreItem.AddingFromPoid;
                                                        //POInvoiceId = ParentInventoryStoreItem.PoinvoiceId;
                                                        //POInvoiceTotalPrice = ParentInventoryStoreItem.PoinvoiceTotalPrice;
                                                        //POInvoiceTotalCost = ParentInventoryStoreItem.PoinvoiceTotalCost;
                                                        //currencyId = ParentInventoryStoreItem.CurrencyId;
                                                        //rateToEGP = ParentInventoryStoreItem.RateToEgp;
                                                        //POInvoiceTotalPriceEGP = ParentInventoryStoreItem.PoinvoiceTotalPriceEgp;
                                                        //POInvoiceTotalCostEGP = ParentInventoryStoreItem.PoinvoiceTotalCostEgp;
                                                        //remainItemPrice = ParentInventoryStoreItem.RemainItemPrice;
                                                        //remainItemCosetEGP = ParentInventoryStoreItem.RemainItemCosetEgp;
                                                        //remainItemCostOtherCU = ParentInventoryStoreItem.RemainItemCostOtherCu;



                                                        // Update PO Item Columns
                                                        // Check if Not call PO Item On Parent 
                                                        //decimal? finalBalance = (ParentInventoryStoreItem.FinalBalance - (ReleaseQTY)) ?? 0;
                                                        //if (ParentInventoryStoreItem.AddingFromPoid != null)
                                                        //{


                                                        //    var POItemObjDB = POItemList.Where(x => x.PurchasePoid == ParentInventoryStoreItem.AddingFromPoid
                                                        //                                                       && x.InventoryItemId == ParentInventoryStoreItem.InventoryItemId
                                                        //                                                        ).FirstOrDefault();
                                                        //    if (POItemObjDB != null)
                                                        //    {
                                                        //        POInvoiceId = POItemObjDB?.PurchasePo?.PurchasePoinvoices.Where(x => x.Poid == ParentInventoryStoreItem.AddingFromPoid).Select(x => x.Id).FirstOrDefault();
                                                        //        currencyId = POItemObjDB.CurrencyId ?? 0;
                                                        //        rateToEGP = POItemObjDB.RateToEgp ?? 0;
                                                        //        POInvoiceTotalPriceEGP = POItemObjDB.ActualUnitPrice ?? 0;
                                                        //        POInvoiceTotalCostEGP = POItemObjDB.FinalUnitCost ?? 0;
                                                        //        POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                                        //        POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                                        //        remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                                        //        remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                                        //        remainItemCosetEGPForRelease = ReleaseQTY * POInvoiceTotalCostEGP ?? 0;
                                                        //        remainItemCostOtherCUForRelease = ReleaseQTY * POInvoiceTotalCost ?? 0;

                                                        //    }
                                                        //}
                                                    }
                                                    var InventoryStoreItemOBJ = new InventoryStoreItem();
                                                    InventoryStoreItemOBJ.InventoryStoreId = InventoryStoreID;
                                                    InventoryStoreItemOBJ.InventoryItemId = (long)item.InventoryItemID;
                                                    InventoryStoreItemOBJ.OrderNumber = MatrialReleaseObj.Id.ToString();
                                                    InventoryStoreItemOBJ.OrderId = MatrialReleaseObj.Id;
                                                    InventoryStoreItemOBJ.CreatedBy = creator;
                                                    InventoryStoreItemOBJ.ModifiedBy = creator;
                                                    InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                    InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                    InventoryStoreItemOBJ.OperationType = OperationType;
                                                    InventoryStoreItemOBJ.Balance1 = (-ReleaseQTY);
                                                    //InventoryStoreItemOBJ.InvenoryStoreLocationId = item.StoreLocationID != null ? item.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationId;
                                                    InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                                    InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                                    InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                                                    InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                                                    InventoryStoreItemOBJ.AddingOrderItemId = MatrialReleaseItemID;
                                                    InventoryStoreItemOBJ.AddingFromPoid = POID;
                                                    //InventoryStoreItemOBJ.PoinvoiceId = POInvoiceId;
                                                    //InventoryStoreItemOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                    //InventoryStoreItemOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                    //InventoryStoreItemOBJ.CurrencyId = currencyId;
                                                    //InventoryStoreItemOBJ.RateToEgp = rateToEGP;
                                                    //InventoryStoreItemOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                    //InventoryStoreItemOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                    //InventoryStoreItemOBJ.RemainItemPrice = remainItemPrice;
                                                    //InventoryStoreItemOBJ.RemainItemCosetEgp = remainItemCosetEGP;
                                                    //InventoryStoreItemOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
                                                    InventoryStoreItemOBJ.HoldQty = 0;
                                                    _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                                    var InventoryStorItemInsertion = _unitOfWork.Complete();
                                                    if (InventoryStorItemInsertion > 0)
                                                    {
                                                        ListIDSUpdate.Add(InventoryStoreItemOBJ.Id);
                                                        // Update Parent Release on InventoryStoreItem
                                                        if (ParentInventoryStoreItem != null)
                                                        {
                                                            ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                                                            ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                            ParentInventoryStoreItem.ModifiedBy = creator;


                                                            //// Update PO Item Columns
                                                            //ParentInventoryStoreItem.PoinvoiceId = POInvoiceId;
                                                            //ParentInventoryStoreItem.CurrencyId = currencyId;
                                                            //ParentInventoryStoreItem.RateToEgp = rateToEGP;
                                                            //ParentInventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                                            //ParentInventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                                            //ParentInventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                                            //ParentInventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
                                                            //ParentInventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
                                                            //ParentInventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
                                                            _unitOfWork.Complete();
                                                        }
                                                    }
                                                }


                                            }
                                        }

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

        public BaseResponseWithID AddInventoryItemMatrialReleaseFromMatrialRequest([FromBody] AddInventoryItemMatrialReleaseFromMatrialRequest BodyRequest,long creator)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    DateTime TransactionDate = DateTime.Now;
                    if (!string.IsNullOrEmpty(BodyRequest.TransactionDate) && DateTime.TryParse(BodyRequest.TransactionDate, out TransactionDate))
                    {
                        DateTime.TryParse(BodyRequest.TransactionDate, out TransactionDate);
                    }

                    #region Validations 
                    //check sent data
                    if (BodyRequest == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialRequestOrderId is null || BodyRequest.MatrialRequestOrderId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "MatrialRequestOrderId is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialRequestDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
                    if (MatrialRequestDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "MatrialRequestOrderId is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckMatrialReleaseDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.MatrialRequestId == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
                    if (CheckMatrialReleaseDB != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "This Transaction is already Created.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrWhiteSpace(BodyRequest.Status))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Status is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var MatrialRequestItemsList = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialRequestDB.Id).ToList();
                    var IDSMatrialRequest = MatrialRequestItemsList.Select(x => x.InventoryItemId).Distinct().ToList();
                    int itemCount = 0;
                    var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSMatrialRequest.Contains(x.InventoryItemId)).ToList();
                    var ListInventoryItems = _unitOfWork.InventoryItems.FindAll(x => IDSMatrialRequest.Contains(x.Id)).ToList();
                    foreach (var item in MatrialRequestItemsList)
                    {
                        string InventoryItemName = "";
                        itemCount++;
                        if (item.InventoryItemId < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err47";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + itemCount;
                            Response.Errors.Add(error);
                        }
                        else // Check is Inventoryt Item ID is valid
                        {
                            var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == item.InventoryItemId).FirstOrDefault();
                            if (InventoryItemObjDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "Invalid Inventory Item Selected item #" + itemCount;
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                InventoryItemName = InventoryItemObjDB.Name;
                            }
                        }


                        var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                                        (long)item.InventoryItemId,
                                                                                        MatrialRequestDB.ToInventoryStoreId,
                                                                                        null,// store location
                                                                                        (decimal)item.RecivedQuantity1,
                                                                                        null); // Default FIFO

                        List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = AvailableItemStockList;

                        if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < item.RecivedQuantity1)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err325";
                            error.ErrorMSG = "Not have availble qty from Item Name " + InventoryItemName;
                            Response.Errors.Add(error);
                            //return Response;
                        }

                    }

                    #endregion

                    if (Response.Result)
                    {

                        var MatrialReleaseObj = new InventoryMatrialRelease();

                        /*###################################################################
                        #####################################################################
                        ################ Create New Matrial Release #########################
                        #####################################################################
                        #####################################################################
                            */


                        MatrialReleaseObj.ToUserId = MatrialRequestDB.FromUserId;
                        MatrialReleaseObj.FromInventoryStoreId = MatrialRequestDB.ToInventoryStoreId;
                        if (!string.IsNullOrWhiteSpace(BodyRequest.TransactionDate))
                        {
                            MatrialReleaseObj.RequestDate = TransactionDate;
                        }
                        MatrialReleaseObj.CreationDate = DateTime.Now;
                        MatrialReleaseObj.ModifiedDate = DateTime.Now;
                        MatrialReleaseObj.CreatedBy = creator;
                        MatrialReleaseObj.ModifiedBy = creator;
                        MatrialReleaseObj.Active = true;
                        MatrialReleaseObj.Status = BodyRequest.Status;
                        MatrialReleaseObj.MatrialRequestId = MatrialRequestDB.Id;

                        _unitOfWork.InventoryMatrialReleases.Add(MatrialReleaseObj);


                        // Update Status MatrialRequest
                        MatrialRequestDB.Status = "Closed";
                        _unitOfWork.Complete();


                        Response.ID = MatrialReleaseObj.Id;



                        #region items Insertion And More Calc
                        foreach (var item in MatrialRequestItemsList)
                        {


                            var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                                            (long)item.InventoryItemId,
                                                                                            MatrialRequestDB.ToInventoryStoreId,
                                                                                            null,// store location
                                                                                            (decimal)item.RecivedQuantity1,
                                                                                            null); // Default FIFO

                            List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = AvailableItemStockList;




                            if (ParentReleaseWithQTYListID.Count() > 0)
                            {
                                ParentReleaseWithQTYListID = ParentReleaseWithQTYListID.Where(x => x.StockBalance > 0).ToList();
                                /* ###################################################################
                                #####################################################################
                                ################ Create New Matrial Release Item ####################
                                #####################################################################
                                #####################################################################*/

                                var MatrialReleaseItem = new InventoryMatrialReleaseItem();
                                MatrialReleaseItem.InventoryMatrialReleasetId = MatrialReleaseObj.Id;
                                MatrialReleaseItem.Comments = item.Comments;
                                MatrialReleaseItem.InventoryMatrialRequestItemId = item.Id;
                                MatrialReleaseItem.RecivedQuantity1 = item.RecivedQuantity1;

                                _unitOfWork.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);

                                var MatrialReleaseItemInsert = _unitOfWork.Complete();
                                if (MatrialReleaseItemInsert > 0)
                                {
                                    long MatrialReleaseItemID = MatrialReleaseItem.Id;
                                    string OperationType = "Release Order";

                                    // List Of Items To remove from Recived QTY
                                    decimal RemainReleaseQTY = (decimal)item.RecivedQuantity1; // 20


                                    // List Of IDS inserted
                                    List<long> ListIDSUpdate = new List<long>();

                                    foreach (var ObjParentRelease in ParentReleaseWithQTYListID)  // 20 -  10   - 5
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
                                            var ParentInventoryStoreItem = InventoryStoreItemList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
                                            long? POID = null;

                                            if (ParentInventoryStoreItem != null)
                                            {
                                                POID = ParentInventoryStoreItem.AddingFromPoid;

                                            }
                                            var InventoryStoreItemOBJ = new InventoryStoreItem();
                                            InventoryStoreItemOBJ.InventoryStoreId = MatrialRequestDB.ToInventoryStoreId;
                                            InventoryStoreItemOBJ.InventoryItemId = item.InventoryItemId;
                                            InventoryStoreItemOBJ.OrderNumber = MatrialReleaseObj.Id.ToString();
                                            InventoryStoreItemOBJ.OrderId = MatrialReleaseObj.Id;
                                            InventoryStoreItemOBJ.CreatedBy = creator;
                                            InventoryStoreItemOBJ.ModifiedBy = creator;
                                            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                            InventoryStoreItemOBJ.OperationType = OperationType;
                                            InventoryStoreItemOBJ.Balance1 = (-ReleaseQTY);

                                            InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                            InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                            InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                                            InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                                            InventoryStoreItemOBJ.AddingOrderItemId = MatrialReleaseItemID;
                                            InventoryStoreItemOBJ.AddingFromPoid = POID;

                                            InventoryStoreItemOBJ.HoldQty = 0;
                                            _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                            var InventoryStorItemInsertion = _unitOfWork.Complete();
                                            if (InventoryStorItemInsertion > 0)
                                            {
                                                ListIDSUpdate.Add(InventoryStoreItemOBJ.Id);
                                                // Update Parent Release on InventoryStoreItem
                                                if (ParentInventoryStoreItem != null)
                                                {
                                                    ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                                                    ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                    ParentInventoryStoreItem.ModifiedBy = creator;

                                                    _unitOfWork.Complete();
                                                }
                                            }
                                        }


                                    }


                                }

                            }

                        }

                        #endregion items


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

        public BaseResponseWithID AddInventoryItemMatrialRequest([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest,long creator)

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
                    if (BodyRequest == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long FromUserId = 0;
                    int InventoryStoreID = 0;
                    if (BodyRequest.FromUserId != 0)
                    {
                        FromUserId = BodyRequest.FromUserId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err45";
                        error.ErrorMSG = "Invalid From User ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.InventoryStoreID != 0)
                    {
                        var CheckInventorystorDB = _unitOfWork.InventoryStores.FindAll(x => x.Id == BodyRequest.InventoryStoreID).FirstOrDefault();
                        if (CheckInventorystorDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err24";
                            error.ErrorMSG = "Invalid Inventory Store ID, not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        InventoryStoreID = BodyRequest.InventoryStoreID;
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
                    if (string.IsNullOrWhiteSpace(BodyRequest.Status))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Status is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (BodyRequest.MatrialReleaseItemWithRequestItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemWithRequestItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemWithRequestItemList.Where(x => x.NewRecivedQTY == null).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-416";
                        error.ErrorMSG = "New Recived Qty required for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    int Counter = 0;
                    var IDSMatrialRequest = BodyRequest.MatrialReleaseItemWithRequestItemList.Select(x => x.InventoryItemID).Distinct().ToList();
                    var ListInventoryItems = _unitOfWork.InventoryItems.FindAll(x => IDSMatrialRequest.Contains(x.Id)).ToList();
                    var MatrialRequestItemsIDSList = BodyRequest.MatrialReleaseItemWithRequestItemList.Select(x => x.ID).ToList();
                    var MatrialRequestItemsListDB = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => MatrialRequestItemsIDSList.Contains(x.Id)).ToList();
                    foreach (var item in BodyRequest.MatrialReleaseItemWithRequestItemList)
                    {
                        Counter++;
                        if (item.NewRecivedQTY == null || item.NewRecivedQTY <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-416";
                            error.ErrorMSG = "Invalid Recived QTY Selected on Item NO#" + Counter;
                            Response.Errors.Add(error);
                        }

                        if (item.InventoryItemID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err47";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        else // Check is Inventoryt Item ID is valid
                        {
                            var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == item.InventoryItemID).FirstOrDefault();
                            if (InventoryItemObjDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }
                        // validate if want to remove or edit  MatrailRequest Item is not exist
                        if (item.ID != 0)
                        {

                            var MatrialRequestItemDB = MatrialRequestItemsListDB.Where(x => x.Id == item.ID).FirstOrDefault();
                            if (MatrialRequestItemDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "Invalid Matrial Request Item Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }
                    }




                    #endregion

                    if (Response.Result)
                    {
                        DateTime TransactionDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(BodyRequest.TransactionDate) && DateTime.TryParse(BodyRequest.TransactionDate, out TransactionDate))
                        {
                            DateTime.TryParse(BodyRequest.TransactionDate, out TransactionDate);
                        }


                        long MatrialRequestOrderId = 0;
                        if (BodyRequest.MatrialRequestOrderId != 0 && BodyRequest.MatrialRequestOrderId != null)
                        {
                            MatrialRequestOrderId = (long)BodyRequest.MatrialRequestOrderId;
                            var MatrialRquestDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
                            if (MatrialRquestDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "Invalid Matrial Request Id";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            // Check If Already Releated with Matrial Release 
                            var MatrialReleaseDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.MatrialRequestId == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
                            if (MatrialReleaseDB != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err47";
                                error.ErrorMSG = "This Transaction is Finished , Cannot Update Now ";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            if (!string.IsNullOrWhiteSpace(BodyRequest.TransactionDate))
                            {
                                MatrialRquestDB.RequestDate = TransactionDate;
                            }
                            MatrialRquestDB.FromUserId = FromUserId;
                            MatrialRquestDB.ToInventoryStoreId = InventoryStoreID;
                            MatrialRquestDB.ApproveResult = BodyRequest.UserInsuranceId?.ToString(); // Column Insurance Id Just for temperary
                            MatrialRquestDB.ModifiedBy = creator;
                            MatrialRquestDB.ModifiedDate = DateTime.Now;
                            _unitOfWork.Complete();

                        }
                        else
                        {


                            var MatrialRequest = new InventoryMatrialRequest();
                            MatrialRequest.FromUserId = FromUserId;
                            MatrialRequest.ToInventoryStoreId = InventoryStoreID;
                            MatrialRequest.RequestDate = TransactionDate;
                            MatrialRequest.RequestTypeId = 20002;
                            MatrialRequest.ApproveResult = BodyRequest.UserInsuranceId?.ToString(); // Column Insurance Id Just for temperary
                            MatrialRequest.Status = "Not Finished Yet";
                            MatrialRequest.Active = true;
                            MatrialRequest.CreatedBy = creator;
                            MatrialRequest.ModifiedBy = creator;
                            MatrialRequest.ModifiedDate = DateTime.Now;
                            MatrialRequest.CreationDate = DateTime.Now;

                            _unitOfWork.InventoryMatrialRequests.Add(MatrialRequest);
                            _unitOfWork.Complete();
                            MatrialRequestOrderId = MatrialRequest.Id;
                        }

                        if (MatrialRequestOrderId > 0)
                        {
                            Response.ID = MatrialRequestOrderId;
                            int itemCount = 0;

                            foreach (var item in BodyRequest.MatrialReleaseItemWithRequestItemList)
                            {
                                itemCount++;
                                //add new 

                                var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == item.InventoryItemID).FirstOrDefault();
                                if (InventoryItemObjDB != null)
                                {
                                    if (item.ID == 0)
                                    {
                                        /* ###################################################################
                                        #####################################################################
                                        ################ Create New Matrial Request Item ####################
                                        #####################################################################
                                        #####################################################################*/
                                        //add new 
                                        var InvMatrialRequestItemObjDB = new InventoryMatrialRequestItem();
                                        InvMatrialRequestItemObjDB.InventoryMatrialRequestId = MatrialRequestOrderId;
                                        InvMatrialRequestItemObjDB.InventoryItemId = (long)item.InventoryItemID;
                                        InvMatrialRequestItemObjDB.Uomid = InventoryItemObjDB.RequstionUomid;
                                        InvMatrialRequestItemObjDB.Comments = item.NewComment;
                                        InvMatrialRequestItemObjDB.RecivedQuantity1 = item.NewRecivedQTY; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                        InvMatrialRequestItemObjDB.ReqQuantity1 = item.NewRecivedQTY;
                                        InvMatrialRequestItemObjDB.PurchaseQuantity1 = 0;

                                        _unitOfWork.InventoryMatrialRequestItems.Add(InvMatrialRequestItemObjDB);

                                    }
                                    else
                                    {
                                        var MatrialRequestItemDB = MatrialRequestItemsListDB.Where(x => x.Id == item.ID).FirstOrDefault();
                                        if (MatrialRequestItemDB != null)
                                        {
                                            if (item.Active)
                                            {

                                                MatrialRequestItemDB.InventoryMatrialRequestId = MatrialRequestOrderId;
                                                MatrialRequestItemDB.InventoryItemId = (long)item.InventoryItemID;
                                                MatrialRequestItemDB.Uomid = InventoryItemObjDB.RequstionUomid;
                                                MatrialRequestItemDB.Comments = item.NewComment;
                                                MatrialRequestItemDB.RecivedQuantity1 = item.NewRecivedQTY;
                                                // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                                MatrialRequestItemDB.ReqQuantity1 = item.NewRecivedQTY;
                                                MatrialRequestItemDB.PurchaseQuantity = 0;
                                            }
                                            else
                                            {
                                                _unitOfWork.InventoryMatrialRequestItems.Delete(MatrialRequestItemDB);
                                            }
                                        }
                                    }
                                }
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
                return Response;
            }
        }

        public BaseResponseWithID AggregateMatrialReleaseItem([FromBody] AggregateMatrialReleaseItemRequest BodyRequest,long creator)
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
                    if (BodyRequest == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.ParentMatrialReleaseId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "ParentMatrialReleaseId is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialReleaseDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.Id == BodyRequest.ParentMatrialReleaseId).FirstOrDefault();
                    if (MatrialReleaseDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Matrial Release ID is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialRequestDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == MatrialReleaseDB.MatrialRequestId).FirstOrDefault();
                    if (MatrialRequestDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Matrial Request ID is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrWhiteSpace(BodyRequest.Status))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Status is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // Validate Matrial Release Item QTY equel aggregated

                    var ItemRequestedList = BodyRequest.AggregateMatrialReleaseItemPerUserList
                    .SelectMany(x => x.AggregateMatrialReleaseItemList)
                    .GroupBy(x => new { x.InventoryItemId, x.MatrialReleaseItemId })
                    .Select(y => new AggregateMatrialReleaseItem
                    {
                        InventoryItemId = y.Key.InventoryItemId,
                        MatrialReleaseItemId = y.Key.MatrialReleaseItemId,
                        QTY = y.Sum(q => q.QTY)
                    }).ToList();
                    var ParentMatrialRequestItemList = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialReleaseDB.MatrialRequestId, includes: new[] { "InventoryItem" }).ToList();

                    var ParentMatrialReleaseItemList = _unitOfWork.InventoryMatrialReleaseItems.FindAll(x => x.InventoryMatrialReleasetId == BodyRequest.ParentMatrialReleaseId, includes: new[] { "InventoryMatrialRequestItem", "InventoryMatrialRequestItem.InventoryItem" }).ToList();
                    if (ItemRequestedList.Count() > 0)
                    {
                        var InventoryStorItemMovement = _unitOfWork.InventoryStoreItems.FindAll(x => x.OrderId == BodyRequest.ParentMatrialReleaseId).ToList();

                        foreach (var item in ItemRequestedList)
                        {
                            var ReleaseItem = ParentMatrialReleaseItemList.Where(x => x.Id == item.MatrialReleaseItemId).FirstOrDefault();
                            if (ReleaseItem != null)
                            {
                                if (item.QTY != ReleaseItem.RecivedQuantity1)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err24";
                                    error.ErrorMSG = "Item Name : " + ReleaseItem.InventoryMatrialRequestItem?.InventoryItem?.Name +
                                        " with Quantity " + ReleaseItem.RecivedQuantity1;
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err24";
                                error.ErrorMSG = "MatrialReleaseItemId : " + item.MatrialReleaseItemId +
                                    " is not exist ";
                                Response.Errors.Add(error);
                            }

                            // Validate Quantity on inventory store ITem (Movement)
                            var QuantityOfItemMovement = InventoryStorItemMovement.Where(x => x.InventoryItemId == item.InventoryItemId).Select(x => x.FinalBalance).DefaultIfEmpty(0).Sum();
                            QuantityOfItemMovement = Math.Abs(QuantityOfItemMovement ?? 0);
                            if (QuantityOfItemMovement < item.QTY)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err24";
                                error.ErrorMSG = "Item Name : " + ReleaseItem?.InventoryMatrialRequestItem?.InventoryItem?.Name +
                                    " Not have balance on movement";
                                Response.Errors.Add(error);
                            }

                        }

                    }


                    #endregion

                    if (Response.Result)
                    {

                        //_Context.Database.BeginTransaction();
                        // Agregate in Matrial Release with New Users
                        var MatrialReleaseItemAggregatedPerUser = BodyRequest.AggregateMatrialReleaseItemPerUserList;
                        if (MatrialReleaseItemAggregatedPerUser.Count() > 0)
                        {
                            foreach (var Parentitem in MatrialReleaseItemAggregatedPerUser)
                            {
                                var InventoryStoreItemMovement = _unitOfWork.InventoryStoreItems.FindAll(x => x.OrderId == MatrialReleaseDB.Id && x.OperationType.Contains("Release Order")).ToList();



                                DateTime TransactionDate = DateTime.Now;
                                if (!string.IsNullOrEmpty(Parentitem.TransactionDate) && DateTime.TryParse(Parentitem.TransactionDate, out TransactionDate))
                                {
                                    DateTime.TryParse(Parentitem.TransactionDate, out TransactionDate);
                                }

                                long UserIDFromPatient = _unitOfWork.UserPatients.FindAll(x => x.Id == Parentitem.UserId).Select(x => x.UserId).FirstOrDefault();
                                if (UserIDFromPatient == 0)
                                {
                                    UserIDFromPatient = Parentitem.UserId;
                                }

                                //var CheckUser = _Context.
                                // Aggregate Matrial Request for each matrial Release

                                var NewMatrialRequest = new InventoryMatrialRequest();
                                NewMatrialRequest.FromUserId = UserIDFromPatient;
                                NewMatrialRequest.Active = true;
                                NewMatrialRequest.ToInventoryStoreId = MatrialRequestDB.ToInventoryStoreId;
                                NewMatrialRequest.RequestDate = TransactionDate;
                                NewMatrialRequest.Status = MatrialRequestDB.Status;
                                NewMatrialRequest.RequestTypeId = MatrialRequestDB.RequestTypeId;
                                NewMatrialRequest.ApproveResult = Parentitem.UserInsuranceId?.ToString(); // Column Insurance Id Just for temperary

                                NewMatrialRequest.Active = true;
                                NewMatrialRequest.CreatedBy = creator;
                                NewMatrialRequest.ModifiedBy = creator;
                                NewMatrialRequest.CreationDate = DateTime.Now;
                                NewMatrialRequest.ModifiedDate = DateTime.Now;

                                _unitOfWork.InventoryMatrialRequests.Add(NewMatrialRequest);
                                _unitOfWork.Complete();




                                var NewMatrialRelease = new InventoryMatrialRelease();
                                NewMatrialRelease.ToUserId = UserIDFromPatient;
                                NewMatrialRelease.FromInventoryStoreId = MatrialReleaseDB.FromInventoryStoreId;
                                NewMatrialRelease.RequestDate = TransactionDate; // MatrialReleaseDB.RequestDate;
                                NewMatrialRelease.Status = BodyRequest.Status;
                                NewMatrialRelease.MatrialRequestId = NewMatrialRequest.Id;
                                NewMatrialRelease.Active = true;
                                NewMatrialRelease.CreatedBy = creator;
                                NewMatrialRelease.ModifiedBy = creator;
                                NewMatrialRelease.CreationDate = DateTime.Now;
                                NewMatrialRelease.ModifiedDate = DateTime.Now;

                                _unitOfWork.InventoryMatrialReleases.Add(NewMatrialRelease);
                                _unitOfWork.Complete();

                                foreach (var item in Parentitem.AggregateMatrialReleaseItemList)
                                {
                                    var ParentMatrialReleaseItem = ParentMatrialReleaseItemList.Where(x => x.Id == item.MatrialReleaseItemId).FirstOrDefault();

                                    var ParentMatrialRequestItem = ParentMatrialRequestItemList.Where(x => x.Id == ParentMatrialReleaseItem.InventoryMatrialRequestItemId).FirstOrDefault();

                                    var NewMatrialRequestItem = new InventoryMatrialRequestItem();
                                    NewMatrialRequestItem.InventoryMatrialRequestId = NewMatrialRequest.Id;
                                    NewMatrialRequestItem.InventoryItemId = item.InventoryItemId;

                                    NewMatrialRequestItem.Uomid = ParentMatrialRequestItem?.InventoryItem?.RequstionUomid ?? 0;
                                    // can be back exception
                                    NewMatrialRequestItem.Comments = ParentMatrialReleaseItem.Comments;
                                    NewMatrialRequestItem.RecivedQuantity1 = item.QTY; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                    NewMatrialRequestItem.ReqQuantity1 = item.QTY;
                                    NewMatrialRequestItem.PurchaseQuantity = 0;

                                    _unitOfWork.InventoryMatrialRequestItems.Add(NewMatrialRequestItem);
                                    _unitOfWork.Complete();


                                    var MatrialReleaseItem = new InventoryMatrialReleaseItem();
                                    MatrialReleaseItem.InventoryMatrialReleasetId = NewMatrialRelease.Id;
                                    MatrialReleaseItem.InventoryMatrialRequestItemId = NewMatrialRequestItem.Id;
                                    //ParentMatrialReleaseItem.InventoryMatrialRequestItemId;
                                    MatrialReleaseItem.RecivedQuantity1 = item.QTY;
                                    MatrialReleaseItem.Comments = ParentMatrialReleaseItem.Comments;

                                    _unitOfWork.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);
                                    _unitOfWork.Complete();

                                    // Change Movement
                                    var InvStoreItemMovementForEachItem = InventoryStoreItemMovement.Where(x => x.InventoryItemId == item.InventoryItemId).ToList();
                                    decimal RemainQTY = item.QTY; // New Qty from new release Item
                                    foreach (var movement in InvStoreItemMovementForEachItem)
                                    {
                                        decimal FinalBalance = Math.Abs(movement.FinalBalance ?? 0);
                                        if (RemainQTY > 0)
                                        {

                                            if (FinalBalance >= RemainQTY) // New Item Released greater than Old Item on movement
                                            {
                                                decimal diffrenceQTY = FinalBalance - RemainQTY;


                                                // Update Old Movement or remove
                                                if (diffrenceQTY == 0)
                                                {
                                                    movement.OrderId = NewMatrialRelease.Id;
                                                    movement.OrderNumber = NewMatrialRelease.Id.ToString();
                                                    movement.AddingOrderItemId = MatrialReleaseItem.Id;
                                                    movement.ModifiedBy = creator;
                                                    movement.ModifiedDate = DateTime.Now;
                                                }
                                                else
                                                {
                                                    // New Movement
                                                    var NewMovement = new InventoryStoreItem();
                                                    NewMovement.InventoryStoreId = movement.InventoryStoreId;
                                                    NewMovement.InventoryItemId = movement.InventoryItemId;
                                                    NewMovement.InventoryItemId = movement.InventoryItemId;
                                                    NewMovement.OperationType = movement.OperationType;
                                                    NewMovement.ExpDate = movement.ExpDate;
                                                    NewMovement.ReleaseParentId = movement.ReleaseParentId;
                                                    NewMovement.AddingFromPoid = movement.AddingFromPoid;
                                                    NewMovement.OrderId = NewMatrialRelease.Id;
                                                    NewMovement.OrderNumber = NewMatrialRelease.Id.ToString();
                                                    NewMovement.AddingOrderItemId = MatrialReleaseItem.Id;
                                                    NewMovement.Balance1 = -1 * RemainQTY;
                                                    NewMovement.FinalBalance = -1 * RemainQTY;
                                                    NewMovement.CreatedBy = creator;
                                                    NewMovement.ModifiedBy = creator;
                                                    NewMovement.CreationDate = DateTime.Now;
                                                    NewMovement.ModifiedDate = DateTime.Now;

                                                    _unitOfWork.InventoryStoreItems.Add(NewMovement);


                                                    // Update Old Movement
                                                    movement.Balance1 = -1 * diffrenceQTY;
                                                    movement.FinalBalance = -1 * diffrenceQTY;
                                                    movement.ModifiedBy = creator;
                                                    movement.ModifiedDate = DateTime.Now;

                                                }

                                                _unitOfWork.Complete();
                                                break;
                                            }
                                            else  // New Item Release Movement less than 
                                            {
                                                // Update Old Movement 

                                                movement.OrderId = NewMatrialRelease.Id;
                                                movement.OrderNumber = NewMatrialRelease.Id.ToString();
                                                movement.AddingOrderItemId = MatrialReleaseItem.Id;
                                                movement.ModifiedBy = creator;
                                                movement.ModifiedDate = DateTime.Now;

                                                //// New Movement
                                                //var NewMovement = new InventoryStoreItem();

                                                //NewMovement.OrderId = NewMatrialRelease.Id;
                                                //NewMovement.OrderNumber = NewMatrialRelease.Id.ToString();
                                                //NewMovement.AddingOrderItemId = MatrialReleaseItem.Id;
                                                //NewMovement.Balance1 = -1 * FinalBalance;
                                                //NewMovement.FinalBalance = -1 * FinalBalance;
                                                //NewMovement.CreatedBy = validation.userID;
                                                //NewMovement.ModifiedBy = validation.userID;
                                                //NewMovement.CreationDate = DateTime.Now;
                                                //NewMovement.ModifiedDate = DateTime.Now;

                                                //_Context.InventoryStoreItems.Add(NewMovement);

                                                //_Context.InventoryStoreItems.Remove(movement);
                                                _unitOfWork.Complete();

                                                RemainQTY = RemainQTY - FinalBalance;
                                            }
                                        }
                                    }
                                }


                            }

                            // Remove Old Release  and Release Items

                            _unitOfWork.InventoryMatrialReleaseItems.DeleteRange(ParentMatrialReleaseItemList);
                            _unitOfWork.Complete();
                            _unitOfWork.InventoryMatrialReleases.Delete(MatrialReleaseDB);
                            _unitOfWork.Complete();

                            _unitOfWork.InventoryMatrialRequestItems.DeleteRange(ParentMatrialRequestItemList);
                            _unitOfWork.Complete();
                            _unitOfWork.InventoryMatrialRequests.Delete(MatrialRequestDB);
                            _unitOfWork.Complete();
                        }

                        //_Context.Database.CommitTransaction();
                    }

                }
                return Response;

            }
            catch (Exception ex)
            {
                //_Context.Database.RollbackTransaction();
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<MatrialReleaseInfo>> GetMatrialReleaseCardsListWithItems([FromHeader] long? UserId, [FromHeader] string FromDate, [FromHeader] string ToDate)
        {
            BaseResponseWithData<List<MatrialReleaseInfo>> Response = new BaseResponseWithData<List<MatrialReleaseInfo>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    List<MatrialReleaseInfo> MatrialReleaseInfoList = new List<MatrialReleaseInfo>();
                    var MatrialReleaseListQuerable = _unitOfWork.InventoryMatrialReleases.GetAll().AsQueryable();
                    if (UserId != null)
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.ToUserId == UserId).AsQueryable();
                    }
                    DateTime FromDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(FromDate) && DateTime.TryParse(FromDate, out FromDateTemp))
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.CreationDate >= FromDateTemp).AsQueryable();
                    }


                    DateTime ToDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(ToDate) && DateTime.TryParse(ToDate, out ToDateTemp))
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.CreationDate <= ToDateTemp).AsQueryable();
                    }
                    var IDSMatrialReleaseList = MatrialReleaseListQuerable.Select(x => x.Id).ToList();
                    var invStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x=> IDSMatrialReleaseList.Contains(x.OrderId));
                    var MatrialReleaseItemListdb = _unitOfWork.InventoryMatrialReleaseItems.FindAll(x => IDSMatrialReleaseList.Contains(x.InventoryMatrialReleasetId), includes: new[] { "InventoryMatrialRequestItem.InventoryItem", "InventoryMatrialRequestItem" }).Select(item => new MatrialReleaseItem
                    {
                        Id = item.Id,
                        MatrialReleaseId = item.InventoryMatrialReleasetId,
                        ItemName = item.InventoryMatrialRequestItem.InventoryItem.Name,
                        ItemComment = item.Comments,
                        Qty = item.RecivedQuantity1,
                        InventoryItemId = item.InventoryMatrialRequestItem.InventoryItemId,
                        StoreId = item.InventoryMatrialReleaset.FromInventoryStoreId,
                        ReleaseReturned = item.InventoryMatrialReleaset.Status == "Returned" ? true : false,
                        ReleaseParentId = invStoreItemList.Where(x=>x.OrderId == item.InventoryMatrialReleasetId && x.InventoryItemId == item.InventoryMatrialRequestItem.InventoryItemId).Select(x=>x.ReleaseParentId).FirstOrDefault(),
                        //ReturnedRemain =(item.RecivedQuantity1 ?? 0) - (item.InventoryInternalBackOrderItems.Where(x=>x.InventoryMatrialReleaseItemId == item.Id).Select(x=>x.RecivedQuantity1).DefaultIfEmpty(0).Sum() ?? 0 ),
                    }).ToList();


                    MatrialReleaseInfoList = MatrialReleaseListQuerable.ToList().Select(x => new MatrialReleaseInfo
                    {
                        Id = x.Id,
                        CreationDate = x.RequestDate.ToShortDateString(),
                        MatrialReleaseItemList = MatrialReleaseItemListdb.Where(y => y.MatrialReleaseId == x.Id).ToList(),

                    }).ToList();

                    Response.Data = MatrialReleaseInfoList;
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
                return Response;
            }
        }

        public BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>> GetMatrialReleaseIndexList([FromHeader] GetMatrialReleaseIndexListFilters filters)
        {
            BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>> Response = new BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    // List<MatrialReleaseIndex> matrialReleaseIndices = new List<MatrialReleaseIndex>();
                    var MatrialReleaseListQuerable = _unitOfWork.InventoryMatrialReleases.FindAllQueryable(a => true, includes: new[] { "ToUser", "FromInventoryStore", "MatrialRequest", "CreatedByNavigation", "ModifiedByNavigation" }).AsQueryable();
                    var MatrialRequestListQuerable = _unitOfWork.InventoryMatrialRequests.FindAllQueryable(x => x.Status == "Not Finished Yet", includes: new[] { "FromUser", "ToInventoryStore", "CreatedByNavigation", "ModifiedByNavigation"}).AsQueryable();
                    if (filters.StoreKeeperUserId != null)
                    {
                        var StoreIDSList = _unitOfWork.InventoryStoreKeepers.FindAll(x => x.UserId == filters.StoreKeeperUserId).Select(x => x.InventoryStoreId).ToList();
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => StoreIDSList.Contains(x.FromInventoryStoreId)).AsQueryable();
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => StoreIDSList.Contains(x.ToInventoryStoreId)).AsQueryable();
                    }
                    if (filters.StoreId != null)
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.FromInventoryStoreId == filters.StoreId);
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => x.ToInventoryStoreId == filters.StoreId).AsQueryable();
                    }
                    if (filters.StatusList != null && filters.StatusList.Count() > 0)
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => filters.StatusList.Contains(x.Status));
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => filters.StatusList.Contains(x.Status));
                    }
                    if (!string.IsNullOrWhiteSpace(filters.FirstName))
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.ToUser.FirstName.ToLower().Trim() == filters.FirstName.ToLower().Trim());
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => x.FromUser.FirstName.ToLower().Trim() == filters.FirstName.ToLower().Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(filters.LastName))
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.ToUser.LastName.ToLower().Trim() == filters.LastName.ToLower().Trim());
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => x.FromUser.LastName.ToLower().Trim() == filters.LastName.ToLower().Trim());
                    }
                    if (!string.IsNullOrWhiteSpace(filters.InsuranceNum))
                    {
                        var UserIdsList = _unitOfWork.UserPatientInsurances.FindAll(x => x.IncuranceNo == filters.InsuranceNum).Select(x => x.UserPatient.UserId).ToList();
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => UserIdsList.Contains(x.ToUserId));
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => UserIdsList.Contains(x.FromUserId));
                    }

                    DateTime DateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(filters.ReleaseDate) && DateTime.TryParse(filters.ReleaseDate, out DateTemp))
                    {
                        MatrialReleaseListQuerable = MatrialReleaseListQuerable.Where(x => x.CreationDate.Date >= DateTemp.Date).AsQueryable();
                        MatrialRequestListQuerable = MatrialRequestListQuerable.Where(x => x.CreationDate.Date >= DateTemp.Date).AsQueryable();
                    }
                   /* int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(filters.CurrentPage) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 20;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }*/
                    var matrialReleaseIndices = MatrialReleaseListQuerable.Select(x => new MatrialReleaseIndex
                    {
                        Id = x.Id,
                        Status = x.Status,
                        IsFinished = true,
                        UserId = x.ToUserId,
                        UserName = x.ToUser.FirstName + " " + x.ToUser.LastName,
                        StoreId = x.FromInventoryStoreId,
                        StoreName = x.FromInventoryStore.Name,
                        LocationName = x.FromInventoryStore.Location,
                        StatusComment = x.MatrialRequest.ApproveRejectNotes,
                        Date = x.RequestDate.ToShortDateString(),
                        CreatorName = x.CreatedByNavigation.FirstName + " " + x.CreatedByNavigation.LastName,
                        ReviewedBy = x.ModifiedByNavigation.FirstName + " " + x.ModifiedByNavigation.LastName

                    }).AsEnumerable();

                    var MatrialRequestList = MatrialRequestListQuerable.Select(x => new MatrialReleaseIndex
                    {
                        MatrialRequestId = x.Id,
                        Status = x.Status,
                        IsFinished = true,
                        UserId = x.FromUserId,
                        UserName = x.FromUser.FirstName + " " + x.FromUser.LastName,
                        StoreId = x.ToInventoryStoreId,
                        StoreName = x.ToInventoryStore.Name,
                        LocationName = x.ToInventoryStore.Location,
                        StatusComment = x.ApproveRejectNotes,
                        Date = x.RequestDate.ToShortDateString(),
                        CreatorName = x.CreatedByNavigation.FirstName + " " + x.CreatedByNavigation.LastName,
                        ReviewedBy = x.ModifiedByNavigation.FirstName + " " + x.ModifiedByNavigation.LastName
                    }).AsEnumerable();

                    var CombinedList = matrialReleaseIndices.Concat(MatrialRequestList);
                    var PagingList = PagedList<MatrialReleaseIndex>.Create(CombinedList.AsQueryable(), filters.CurrentPage, filters.NumberOfItemsPerPage);

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = PagingList.TotalPages,
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = PagingList.TotalCount
                    };
                    Response.Data = PagingList;
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
                return Response;
            }
        }

        public List<InventoryStoreItemIDWithQTY> GetParentReleaseIDWithSettingStore(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemID, int StoreID, int? StoreLocationID, decimal QTY, bool? IsFIFO)
        {
            List<InventoryStoreItemIDWithQTY> ParentInvStoreItemIDWithBalanceList = new List<InventoryStoreItemIDWithQTY>();
            if (IsFIFO == false) // LIFO
            {
                //ParentInvStoreItemIDWithBalanceList = _Context.proc_InventoryStoreItemLoadAll().Where(x => x.InventoryItemID == InventoryItemID
                //                                                                   && x.InventoryStoreID == StoreID
                //                                                                   && (StoreLocationID != null ? x.InvenoryStoreLocationID == StoreLocationID : true)
                //                                                                   && x.finalBalance <= QTY).OrderByDescending(x => x.CreationDate).Select(x => x.ID).FirstOrDefault();
                ParentInvStoreItemIDWithBalanceList = InventoryStoreItemList.Where(x => x.InventoryItemId == InventoryItemID
                                                                   && x.InventoryStoreId == StoreID
                                                                   && (StoreLocationID != null ? x.InvenoryStoreLocationId == StoreLocationID : true)
                                                                   && x.FinalBalance > 0
                                                                   ).OrderByDescending(x => x.CreationDate).Select(x => new InventoryStoreItemIDWithQTY { ID = x.Id, StockBalance = (decimal)x.FinalBalance }).ToList();
            }
            else // FIFO  or Default FIFO
            {
                ParentInvStoreItemIDWithBalanceList = InventoryStoreItemList.Where(x => x.InventoryItemId == InventoryItemID
                                                                                   && x.InventoryStoreId == StoreID
                                                                                   && (StoreLocationID != null ? x.InvenoryStoreLocationId == StoreLocationID : true)
                                                                                   && x.FinalBalance > 0
                                                                                   ).OrderBy(x => x.CreationDate).Select(x => new InventoryStoreItemIDWithQTY { ID = x.Id, StockBalance = (decimal)x.FinalBalance }).ToList();
            }
            return ParentInvStoreItemIDWithBalanceList;
        }

        public BaseResponseWithData<List<StoresListForKeeper>> GetStoresIdListForKeeper(long creator)

        {
            BaseResponseWithData<List<StoresListForKeeper>> Response = new BaseResponseWithData<List<StoresListForKeeper>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var storesIds = _unitOfWork.InventoryStoreKeepers.FindAll(a => a.UserId == creator && a.Active == true).Select(a => a.InventoryStoreId).ToList();
                    var stores = _unitOfWork.InventoryStores.FindAll(a => storesIds.Contains(a.Id)).ToList();
                    List<StoresListForKeeper> storeIdList = new List<StoresListForKeeper>();
                    storeIdList = stores.Select(a => new StoresListForKeeper() { Name = a.Name, StoreId = a.Id }).ToList();
                    Response.Data = storeIdList;
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
                return Response;
            }
        }

        public BaseResponseWithID UpdateMatrialReleaseStatus([FromBody] UpdateMatrialReleaseStatusRequest BodyRequest,long creator)

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
                    if (BodyRequest == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long MatrialReleaseId = 0;
                    int InventoryStoreID = 0;
                    if (BodyRequest.MatrialReleaseId != 0)
                    {
                        MatrialReleaseId = BodyRequest.MatrialReleaseId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err45";
                        error.ErrorMSG = "Invalid Matrial Release Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialReleaseDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.Id == MatrialReleaseId).FirstOrDefault();
                    if (MatrialReleaseDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err45";
                        error.ErrorMSG = "The Matrial Release is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrWhiteSpace(BodyRequest.Status))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Status is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    #endregion

                    if (Response.Result)
                    {

                        MatrialReleaseDB.Status = BodyRequest.Status;
                        MatrialReleaseDB.ModifiedBy = creator;
                        MatrialReleaseDB.ModifiedDate = DateTime.Now;


                        if (!string.IsNullOrWhiteSpace(BodyRequest.StatusComment))
                        {
                            var MatrialRequestDB = _Context.InventoryMatrialRequests.Where(x => x.Id == MatrialReleaseDB.MatrialRequestId).FirstOrDefault();
                            if (MatrialRequestDB != null)
                            {
                                MatrialRequestDB.ApproveRejectNotes = BodyRequest.StatusComment;
                            }
                        }
                        var result = _Context.SaveChanges();
                        if (result > 0)
                        {
                            Response.ID = MatrialReleaseDB.Id;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err102";
                            error.ErrorMSG = "Status is Not Changed";
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

        public BaseResponseWithData<MatrialReleaseInfoDetails> ViewMatrialReleaseWithItems([FromHeader] long? MatrialReleaseId)
        {
            var Response = new BaseResponseWithData<MatrialReleaseInfoDetails>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    MatrialReleaseInfoDetails MatrialReleaseInfo = new MatrialReleaseInfoDetails();
                    if (MatrialReleaseId == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Matrial Release Id is Required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialReleaseInfoDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.Id == MatrialReleaseId, includes: new[] { "ToUser", "FromInventoryStore", "CreatedByNavigation" }).FirstOrDefault();
                    if (MatrialReleaseInfoDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Matrial Release is not exist";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialReleaseItemList = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => x.InventoryMatrialReleasetId == MatrialReleaseId).ToList();
                    //var ItemIdList = MatrialReleaseItemList.Select(x => x.InventoryItemId).ToList();
                    //    var ItemDBList = _Context.InventoryItems.Where(x => ItemIdList.Contains(x.Id)).ToList();
                    if (MatrialReleaseInfoDB != null)
                    {
                        var MatrialRequestDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == MatrialReleaseInfoDB.MatrialRequestId).FirstOrDefault();
                        //var UserPatientInsuranceId = _Context.InventoryMatrialRequests.Where(x => x.Id == MatrialReleaseInfoDB.MatrialRequestId).Select(x => x.ApproveResult).FirstOrDefault();
                        // Temp For User Patient Insurance
                        int InsuranceId = 0;
                        if (!string.IsNullOrWhiteSpace(MatrialRequestDB?.ApproveResult) && int.TryParse(MatrialRequestDB?.ApproveResult, out InsuranceId))
                        {

                            var UserInurance = _unitOfWork.UserPatientInsurances.FindAll(x => x.Id == InsuranceId).FirstOrDefault();
                            if (UserInurance != null)
                            {
                                MatrialReleaseInfo.InsuranceName = UserInurance.IncuranceNo + " - " + UserInurance.Name;
                            }

                        }
                        var GetPatientDOB = _unitOfWork.UserPatients.FindAll(x => x.UserId == MatrialReleaseInfoDB.ToUserId).Select(
                            x => x.DateOfBirth != null ? ((DateTime)x.DateOfBirth).ToShortDateString() : null).FirstOrDefault();

                        MatrialReleaseInfo.Id = MatrialReleaseInfoDB.Id;
                        MatrialReleaseInfo.CreationDate = MatrialReleaseInfoDB.RequestDate.ToShortDateString();  // Request DAte
                        MatrialReleaseInfo.Status = MatrialReleaseInfoDB.Status;
                        MatrialReleaseInfo.StoreId = MatrialReleaseInfoDB.FromInventoryStoreId;
                        MatrialReleaseInfo.StoreName = MatrialReleaseInfoDB.FromInventoryStore?.Name;
                        MatrialReleaseInfo.UserId = MatrialReleaseInfoDB.ToUserId;
                        MatrialReleaseInfo.DateOfBirth = GetPatientDOB;
                        MatrialReleaseInfo.StatusComment = MatrialRequestDB?.ApproveRejectNotes; // Comment if Rejected
                        MatrialReleaseInfo.UserName = MatrialReleaseInfoDB.ToUser?.FirstName + " " + MatrialReleaseInfoDB.ToUser?.LastName;
                        MatrialReleaseInfo.CreatorName = MatrialReleaseInfoDB.CreatedByNavigation?.FirstName + " " + MatrialReleaseInfoDB.CreatedByNavigation?.LastName;

                        MatrialReleaseInfo.MatrialReleaseItemList = MatrialReleaseItemList.Where(y => y.InventoryMatrialReleasetId == MatrialReleaseId).Select(item => new MatrialReleaseItem
                        {
                            Id = item.Id,
                            MatrialReleaseId = item.Id,
                            ItemName = item.ItemName,
                            ItemComment = item.Comments,
                            //ItemDBList.Where(x=>x.Id == item.InventoryItemId).FirstOrDefault()?.Description,
                            InventoryItemId = item.InventoryItemId ?? 0,
                            Qty = item.RecivedQuantity
                        }).ToList();
                    }


                    Response.Data = MatrialReleaseInfo;
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
                return Response;
            }
        }

        public BaseResponseWithData<MatrialRequestInfoDetails> ViewMatrialRequestWithItems([FromHeader] long? MatrialRequestId)
        {
            var Response = new BaseResponseWithData<MatrialRequestInfoDetails>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    MatrialRequestInfoDetails MatrialRequestInfo = new MatrialRequestInfoDetails();
                    if (MatrialRequestId == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Matrial Request Id is Required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialRequestInfoDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == MatrialRequestId, includes: new[] { "FromUser", "ToInventoryStore", "CreatedByNavigation"}).FirstOrDefault();
                    if (MatrialRequestInfoDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Matrial Request is not exist";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialRequestItemList = _unitOfWork.VInventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialRequestId).ToList();
                    // var ItemIdList = MatrialRequestItemList.Select(x => x.InventoryItemId).ToList();
                    // var ItemDBList = _Context.InventoryItems.Where(x => ItemIdList.Contains(x.Id)).ToList();
                    if (MatrialRequestInfoDB != null)
                    {
                        var UserPatientInsuranceId = MatrialRequestInfoDB.ApproveResult;
                        //_Context.InventoryMatrialRequests.Where(x => x.Id == MatrialReleaseInfoDB.MatrialRequestId).Select(x => x.ApproveResult).FirstOrDefault();
                        // Temp For User Patient Insurance
                        int InsuranceId = 0;
                        if (!string.IsNullOrWhiteSpace(UserPatientInsuranceId) && int.TryParse(UserPatientInsuranceId, out InsuranceId))
                        {

                            var UserInurance = _Context.UserPatientInsurances.Where(x => x.Id == InsuranceId).FirstOrDefault();
                            if (UserInurance != null)
                            {
                                MatrialRequestInfo.InsuranceName = UserInurance.IncuranceNo + " - " + UserInurance.Name;
                            }

                        }
                        MatrialRequestInfo.Id = MatrialRequestInfoDB.Id;
                        MatrialRequestInfo.CreationDate = MatrialRequestInfoDB.RequestDate.ToShortDateString();
                        MatrialRequestInfo.UserId = MatrialRequestInfoDB.FromUserId;
                        MatrialRequestInfo.StoreId = MatrialRequestInfoDB.ToInventoryStoreId;
                        var insuranceid = 0;
                        MatrialRequestInfo.UserInsuranceId = string.IsNullOrEmpty(MatrialRequestInfoDB.ApproveResult) ? null :
                            int.TryParse(MatrialRequestInfoDB.ApproveResult,out insuranceid)==true? insuranceid:null; // User Insurance Id
                        MatrialRequestInfo.Status = MatrialRequestInfoDB.Status;
                        MatrialRequestInfo.StoreName = MatrialRequestInfoDB.ToInventoryStore?.Name;
                        MatrialRequestInfo.UserName = MatrialRequestInfoDB.FromUser?.FirstName + " " + MatrialRequestInfoDB.FromUser?.LastName;
                        MatrialRequestInfo.CreatorName = MatrialRequestInfoDB.CreatedByNavigation?.FirstName + " " + MatrialRequestInfoDB.CreatedByNavigation?.LastName;
                        MatrialRequestInfo.MatrialReleaseItemList = MatrialRequestItemList.Where(y => y.InventoryMatrialRequestId == MatrialRequestId).Select(item => new MatrialRequestItemModel
                        {
                            Id = item.Id,
                            InventoryItemId = item.InventoryItemId,
                            ItemName = item.ItemName,
                            ItemComment = item.Comments,
                            //ItemDBList.Where(x => x.Id == item.InventoryItemId).FirstOrDefault()?.Description,

                            Qty = item.RecivedQuantity
                        }).ToList();
                    }


                    Response.Data = MatrialRequestInfo;
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
                return Response;
            }
        }
    }
}