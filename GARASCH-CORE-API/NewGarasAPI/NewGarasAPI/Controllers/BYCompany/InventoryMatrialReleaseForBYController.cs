using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.Inventory;
using NewGarasAPI.Models.Inventory.Requests;
using Org.BouncyCastle.Ocsp;
using System;
using System.Linq;
using System.Net;

namespace NewGarasAPI.Controllers.BYCompany
{
    [Route("[controller]")]
    [ApiController]
    public class InventoryMatrialReleaseForBYController : Controller
    {

        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly ITenantService _tenantService;
        private readonly IInventoryMatrialReleaseForBYService _matrialReleaseForBYService;
        public InventoryMatrialReleaseForBYController(ITenantService tenantService, IInventoryMatrialReleaseForBYService matrialReleaseForBYService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _matrialReleaseForBYService = matrialReleaseForBYService;
        }




        // Just Save Add Matrial Release  (Insert in Matrial Request Only)
        [HttpPost("AddInventoryItemMatrialRequest")]
        public BaseResponseWithID AddInventoryItemMatrialRequest([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.AddInventoryItemMatrialRequest(BodyRequest, validation.userID);
                }

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
            }
            return Response;
        }

        // Just Save Add Matrial Release  (Insert in Matrial Request Only)
        [HttpPost("AddInventoryItemMatrialRequestForLIBMARK")]
        public BaseResponseWithID AddInventoryItemMatrialRequestForLIBMARK([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
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
                        var CheckInventorystorDB = _Context.InventoryStores.Where(x => x.Id == BodyRequest.InventoryStoreID).FirstOrDefault();
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
                    var ListInventoryItems = _Context.InventoryItems.Where(x => IDSMatrialRequest.Contains(x.Id)).ToList();
                    var MatrialRequestItemsIDSList = BodyRequest.MatrialReleaseItemWithRequestItemList.Select(x => x.ID).ToList();
                    var MatrialRequestItemsListDB = _Context.InventoryMatrialRequestItems.Where(x => MatrialRequestItemsIDSList.Contains(x.Id)).ToList();
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
                        if (item.PublicId <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err47";
                            error.ErrorMSG = "Invalid PublicId Number for Item Selected item #" + Counter;
                            Response.Errors.Add(error);
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



                        DateTime TransactionDate = DateTime.Now;
                        if (!string.IsNullOrEmpty(BodyRequest.TransactionDate) && DateTime.TryParse(BodyRequest.TransactionDate, out TransactionDate))
                        {
                            DateTime.TryParse(BodyRequest.TransactionDate, out TransactionDate);
                        }


                        long MatrialRequestOrderId = 0;
                        if (BodyRequest.MatrialRequestOrderId != 0 && BodyRequest.MatrialRequestOrderId != null)
                        {
                            MatrialRequestOrderId = (long)BodyRequest.MatrialRequestOrderId;
                            var MatrialRquestDB = _Context.InventoryMatrialRequests.Where(x => x.Id == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
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
                            var MatrialReleaseDB = _Context.InventoryMatrialReleases.Where(x => x.MatrialRequestId == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
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
                            MatrialRquestDB.ModifiedBy = validation.userID;
                            MatrialRquestDB.ModifiedDate = DateTime.Now;
                            _Context.SaveChanges();

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
                            MatrialRequest.CreatedBy = validation.userID;
                            MatrialRequest.ModifiedBy = validation.userID;
                            MatrialRequest.ModifiedDate = DateTime.Now;
                            MatrialRequest.CreationDate = DateTime.Now;

                            _Context.InventoryMatrialRequests.Add(MatrialRequest);
                            _Context.SaveChanges();
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
                                        // for LIBMARK frpm (comments) in adding order Item to matrial request Item (Comment) for release from inventoystoreitem (OrderNumber)
                                        InvMatrialRequestItemObjDB.Comments = item.PublicId?.ToString(); // item.NewComment;
                                        InvMatrialRequestItemObjDB.RecivedQuantity1 = item.NewRecivedQTY; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                        InvMatrialRequestItemObjDB.ReqQuantity1 = item.NewRecivedQTY;
                                        InvMatrialRequestItemObjDB.PurchaseQuantity = 0;

                                        _Context.InventoryMatrialRequestItems.Add(InvMatrialRequestItemObjDB);

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
                                                // for LIBMARK frpm (comments) in adding order Item to matrial request Item (Comment) for release from inventoystoreitem (OrderNumber)
                                                MatrialRequestItemDB.Comments = item.PublicId?.ToString(); //item.NewComment;
                                                MatrialRequestItemDB.RecivedQuantity1 = item.NewRecivedQTY;
                                                // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                                MatrialRequestItemDB.ReqQuantity1 = item.NewRecivedQTY;
                                                MatrialRequestItemDB.PurchaseQuantity = 0;
                                            }
                                            else
                                            {
                                                _Context.InventoryMatrialRequestItems.Remove(MatrialRequestItemDB);
                                            }
                                        }
                                    }
                                }
                                _Context.SaveChanges();
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



        [HttpPost("AddInventoryItemMatrialReleaseFromMatrialRequest")]
        public BaseResponseWithID AddInventoryItemMatrialReleaseFromMatrialRequest([FromBody] AddInventoryItemMatrialReleaseFromMatrialRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.AddInventoryItemMatrialReleaseFromMatrialRequest(BodyRequest, validation.userID);
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



        [HttpPost("AddInventoryItemMatrialReleaseFromMatrialRequestForLIBMARK")]
        public BaseResponseWithID AddInventoryItemMatrialReleaseFromMatrialRequestForLIBMARK([FromBody] AddInventoryItemMatrialReleaseFromMatrialRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
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
                    var MatrialRequestDB = _Context.InventoryMatrialRequests.Where(x => x.Id == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
                    if (MatrialRequestDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "MatrialRequestOrderId is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckMatrialReleaseDB = _Context.InventoryMatrialReleases.Where(x => x.MatrialRequestId == BodyRequest.MatrialRequestOrderId).FirstOrDefault();
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

                    var MatrialRequestItemsList = _Context.InventoryMatrialRequestItems.Where(x => x.InventoryMatrialRequestId == MatrialRequestDB.Id).ToList();
                    var IDSMatrialRequest = MatrialRequestItemsList.Select(x => x.InventoryItemId).Distinct().ToList();
                    int itemCount = 0;
                    var InventoryStoreItemList = _Context.InventoryStoreItems.Where(x => IDSMatrialRequest.Contains(x.InventoryItemId)).ToList();
                    var ListInventoryItems = _Context.InventoryItems.Where(x => IDSMatrialRequest.Contains(x.Id)).ToList();
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


                        //var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                        //                                                                (long)item.InventoryItemId,
                        //                                                                MatrialRequestDB.ToInventoryStoreId,
                        //                                                                null,// store location
                        //                                                                (decimal)item.RecivedQuantity1,
                        //                                                                null); // Default FIFO

                        //List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = AvailableItemStockList;

                        //if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < item.RecivedQuantity1)
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err325";
                        //    error.ErrorMSG = "Not have availble qty from Item Name " + InventoryItemName;
                        //    Response.Errors.Add(error);
                        //    //return Response;
                        //}

                        // VAlidate if have QTY in PublicId Number 

                        // Item.comments => PublicId Number for LIBMARK
                        //var CheckBalanceForItem = _Context.InventoryStoreItems.Where(x => x.InventoryItemId == (long)item.InventoryItemId && x.OrderNumber == item.Comments).Select(x => x.FinalBalance).FirstOrDefault();


                        long parsedReleaseParentId = 0;
                            long.TryParse(item.Comments, out parsedReleaseParentId);
                        var CheckBalanceForItem = _Context.InventoryStoreItems
                            .Where(x => x.InventoryItemId == (long)item.InventoryItemId &&
                                       (x.OrderNumber == item.Comments ||
                                       (parsedReleaseParentId != 0 ? x.ReleaseParentId == parsedReleaseParentId : true)))
                            .Select(x => x.FinalBalance)
                            .Sum();
                        if (CheckBalanceForItem == null || CheckBalanceForItem <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err325";
                            error.ErrorMSG = "Not have availble qty from Item Name " + InventoryItemName;
                            Response.Errors.Add(error);
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
                        MatrialReleaseObj.CreatedBy = validation.userID;
                        MatrialReleaseObj.ModifiedBy = validation.userID;
                        MatrialReleaseObj.Active = true;
                        MatrialReleaseObj.Status = BodyRequest.Status;
                        MatrialReleaseObj.MatrialRequestId = MatrialRequestDB.Id;

                        _Context.InventoryMatrialReleases.Add(MatrialReleaseObj);


                        // Update Status MatrialRequest
                        MatrialRequestDB.Status = "Closed";
                        _Context.SaveChanges();


                        Response.ID = MatrialReleaseObj.Id;



                        #region items Insertion And More Calc
                        foreach (var item in MatrialRequestItemsList)
                        {


                            //var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                            //                                                                (long)item.InventoryItemId,
                            //                                                                MatrialRequestDB.ToInventoryStoreId,
                            //                                                                null,// store location
                            //                                                                (decimal)item.RecivedQuantity1,
                            //                                                                null); // Default FIFO

                            //List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = AvailableItemStockList;
                            var ItemForSpecificPublicId = _Context.InventoryStoreItems.Where(x => x.InventoryItemId == (long)item.InventoryItemId && x.OrderNumber == item.Comments).FirstOrDefault();

                            List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = new List<InventoryStoreItemIDWithQTY>();
                            if (ItemForSpecificPublicId != null)
                            {

                                ParentReleaseWithQTYListID.Add(new InventoryStoreItemIDWithQTY
                                {
                                    ID = ItemForSpecificPublicId.Id,
                                    StockBalance = ItemForSpecificPublicId.FinalBalance ?? 0
                                });

                            }


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

                                _Context.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);

                                var MatrialReleaseItemInsert = _Context.SaveChanges();
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
                                            InventoryStoreItemOBJ.CreatedBy = validation.userID;
                                            InventoryStoreItemOBJ.ModifiedBy = validation.userID;
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
                                            _Context.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                            var InventoryStorItemInsertion = _Context.SaveChanges();
                                            if (InventoryStorItemInsertion > 0)
                                            {
                                                ListIDSUpdate.Add(InventoryStoreItemOBJ.Id);
                                                // Update Parent Release on InventoryStoreItem
                                                if (ParentInventoryStoreItem != null)
                                                {
                                                    ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                                                    ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                    ParentInventoryStoreItem.ModifiedBy = validation.userID;

                                                    _Context.SaveChanges();
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



        [HttpPost("AggregateMatrialReleaseItem")]
        public BaseResponseWithID AggregateMatrialReleaseItem([FromBody] AggregateMatrialReleaseItemRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.AggregateMatrialReleaseItem(BodyRequest, validation.userID);
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





        [HttpGet("GetInventoryStoreItemForLIBMARK")]
        public ActionResult<BaseResponseWithDataAndHeader<List<InventoryStoreItemBalanceModel>>> GetInventoryStoreItemForLIBMARK([FromHeader] long? InventoryItemId)
        {
            BaseResponseWithDataAndHeader<List<InventoryStoreItemBalanceModel>> Response = new BaseResponseWithDataAndHeader<List<InventoryStoreItemBalanceModel>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    if (InventoryItemId == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid InventoryItemId.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var CheckInventoryItem = _Context.InventoryItems.Where(x => x.Id == InventoryItemId).FirstOrDefault();
                    if (CheckInventoryItem == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This InventoryItemId is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 20;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }
                    var InventoryStoreItemList = _Context.InventoryStoreItems.Where(x => x.InventoryItemId == InventoryItemId && x.FinalBalance > 0).ToList();
                    var InventoryStoreItemObjList = InventoryStoreItemList.Select(x => new InventoryStoreItemBalanceModel
                    {
                        InventoryStoreItemId = x.Id,
                        PublicId = !string.IsNullOrWhiteSpace(x.OrderNumber) ? int.Parse(x.OrderNumber) : 0,
                        FinalBalance = x.FinalBalance,

                    }).AsQueryable();


                    var PagingList = PagedList<InventoryStoreItemBalanceModel>.Create(InventoryStoreItemObjList.AsQueryable(), CurrentPage, NumberOfItemsPerPage);

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = PagingList.TotalPages,
                        ItemsPerPage = NumberOfItemsPerPage,
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















        // before aggregate matrial request for each release
        //[HttpPost("AggregateMatrialReleaseItem")]
        //public BaseResponseWithID AggregateMatrialReleaseItem([FromBody] AggregateMatrialReleaseItemRequest BodyRequest)
        //{
        //    BaseResponseWithID Response = new BaseResponseWithID();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {
        //        //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        //WebHeaderCollection headers = request.Headers;
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        Response.Errors = validation.errors;
        //        Response.Result = validation.result;
        //        if (Response.Result)
        //        {
        //            #region Validations 
        //            //check sent data
        //            if (BodyRequest == null)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err-P12";
        //                error.ErrorMSG = "please insert a valid data.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            if (BodyRequest.ParentMatrialReleaseId == 0)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "ParentMatrialReleaseId is required.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            var MatrialReleaseDB = _Context.InventoryMatrialReleases.Where(x => x.Id == BodyRequest.ParentMatrialReleaseId).FirstOrDefault();
        //            if (MatrialReleaseDB == null)
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "Matrial Release ID is not exist.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            if (string.IsNullOrWhiteSpace(BodyRequest.Status))
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "Status is required.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            // Validate Matrial Release Item QTY equel aggregated

        //            var ItemRequestedList = BodyRequest.AggregateMatrialReleaseItemPerUserList
        //            .SelectMany(x => x.AggregateMatrialReleaseItemList)
        //            .GroupBy(x => new { x.InventoryItemId, x.MatrialReleaseItemId })
        //            .Select(y => new AggregateMatrialReleaseItem
        //            {
        //                InventoryItemId = y.Key.InventoryItemId,
        //                MatrialReleaseItemId = y.Key.MatrialReleaseItemId,
        //                QTY = y.Sum(q => q.QTY)
        //            }).ToList();
        //            var ParentMatrialReleaseItemList = _Context.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialReleasetId == BodyRequest.ParentMatrialReleaseId).Include(x => x.InventoryMatrialRequestItem).Include(x => x.InventoryMatrialRequestItem.InventoryItem).ToList();
        //            if (ItemRequestedList.Count() > 0)
        //            {
        //                var InventoryStorItemMovement = _Context.InventoryStoreItems.Where(x => x.OrderId == BodyRequest.ParentMatrialReleaseId).ToList();

        //                foreach (var item in ItemRequestedList)
        //                {
        //                    var ReleaseItem = ParentMatrialReleaseItemList.Where(x => x.Id == item.MatrialReleaseItemId).FirstOrDefault();
        //                    if (ReleaseItem != null)
        //                    {
        //                        if (item.QTY != ReleaseItem.RecivedQuantity1)
        //                        {
        //                            Response.Result = false;
        //                            Error error = new Error();
        //                            error.ErrorCode = "Err24";
        //                            error.ErrorMSG = "Item Name : " + ReleaseItem.InventoryMatrialRequestItem?.InventoryItem?.Name +
        //                                " with Quantity " + ReleaseItem.RecivedQuantity1;
        //                            Response.Errors.Add(error);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        Response.Result = false;
        //                        Error error = new Error();
        //                        error.ErrorCode = "Err24";
        //                        error.ErrorMSG = "MatrialReleaseItemId : " + item.MatrialReleaseItemId +
        //                            " is not exist ";
        //                        Response.Errors.Add(error);
        //                    }

        //                    // Validate Quantity on inventory store ITem (Movement)
        //                    var QuantityOfItemMovement = InventoryStorItemMovement.Where(x => x.InventoryItemId == item.InventoryItemId).Select(x => x.FinalBalance).DefaultIfEmpty(0).Sum();
        //                    QuantityOfItemMovement = Math.Abs(QuantityOfItemMovement ?? 0);
        //                    if (QuantityOfItemMovement < item.QTY)
        //                    {
        //                        Response.Result = false;
        //                        Error error = new Error();
        //                        error.ErrorCode = "Err24";
        //                        error.ErrorMSG = "Item Name : " + ReleaseItem.InventoryMatrialRequestItem?.InventoryItem?.Name +
        //                            " Not have balance on movement";
        //                        Response.Errors.Add(error);
        //                    }

        //                }

        //            }


        //            #endregion

        //            if (Response.Result)
        //            {
        //                //_Context.Database.BeginTransaction();
        //                // Agregate in Matrial Release with New Users
        //                var MatrialReleaseItemAggregatedPerUser = BodyRequest.AggregateMatrialReleaseItemPerUserList;
        //                if (MatrialReleaseItemAggregatedPerUser.Count() > 0)
        //                {
        //                    foreach (var Parentitem in MatrialReleaseItemAggregatedPerUser)
        //                    {
        //                        var InventoryStoreItemMovement = _Context.InventoryStoreItems.Where(x => x.OrderId == MatrialReleaseDB.Id && x.OperationType.Contains("Release Order")).ToList();
        //                        var NewMatrialRelease = new InventoryMatrialRelease();
        //                        NewMatrialRelease.ToUserId = Parentitem.UserId;
        //                        NewMatrialRelease.FromInventoryStoreId = MatrialReleaseDB.FromInventoryStoreId;
        //                        NewMatrialRelease.RequestDate = MatrialReleaseDB.RequestDate;
        //                        NewMatrialRelease.Active = true;
        //                        NewMatrialRelease.Status = BodyRequest.Status;
        //                        NewMatrialRelease.MatrialRequestId = MatrialReleaseDB.MatrialRequestId;
        //                        NewMatrialRelease.CreatedBy = validation.userID;
        //                        NewMatrialRelease.ModifiedBy = validation.userID;
        //                        NewMatrialRelease.CreationDate = DateTime.Now;
        //                        NewMatrialRelease.ModifiedDate = DateTime.Now;

        //                        _Context.InventoryMatrialReleases.Add(NewMatrialRelease);
        //                        _Context.SaveChanges();

        //                        foreach (var item in Parentitem.AggregateMatrialReleaseItemList)
        //                        {
        //                            var ParentMatrialReleaseItem = ParentMatrialReleaseItemList.Where(x => x.Id == item.MatrialReleaseItemId).FirstOrDefault();

        //                            var MatrialReleaseItem = new InventoryMatrialReleaseItem();
        //                            MatrialReleaseItem.InventoryMatrialReleasetId = NewMatrialRelease.Id;
        //                            MatrialReleaseItem.InventoryMatrialRequestItemId = ParentMatrialReleaseItem.InventoryMatrialRequestItemId;
        //                            MatrialReleaseItem.RecivedQuantity1 = item.QTY;
        //                            MatrialReleaseItem.Comments = ParentMatrialReleaseItem.Comments;

        //                            _Context.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);
        //                            _Context.SaveChanges();

        //                            // Change Movement
        //                            var InvStoreItemMovementForEachItem = InventoryStoreItemMovement.Where(x => x.InventoryItemId == item.InventoryItemId).ToList();
        //                            decimal RemainQTY = item.QTY; // New Qty from new release Item
        //                            foreach (var movement in InvStoreItemMovementForEachItem)
        //                            {
        //                                decimal FinalBalance = Math.Abs(movement.FinalBalance ?? 0);
        //                                if (RemainQTY > 0)
        //                                {

        //                                    if (FinalBalance >= RemainQTY) // New Item Released greater than Old Item on movement
        //                                    {
        //                                        decimal diffrenceQTY = FinalBalance - RemainQTY;


        //                                        // Update Old Movement or remove
        //                                        if (diffrenceQTY == 0)
        //                                        {
        //                                            movement.OrderId = NewMatrialRelease.Id;
        //                                            movement.OrderNumber = NewMatrialRelease.Id.ToString();
        //                                            movement.AddingOrderItemId = MatrialReleaseItem.Id;
        //                                            movement.ModifiedBy = validation.userID;
        //                                            movement.ModifiedDate = DateTime.Now;
        //                                        }
        //                                        else
        //                                        {
        //                                            // New Movement
        //                                            var NewMovement = new InventoryStoreItem();
        //                                            NewMovement.InventoryStoreId = movement.InventoryStoreId;
        //                                            NewMovement.InventoryItemId = movement.InventoryItemId;
        //                                            NewMovement.InventoryItemId = movement.InventoryItemId;
        //                                            NewMovement.OperationType = movement.OperationType;
        //                                            NewMovement.ExpDate = movement.ExpDate;
        //                                            NewMovement.ReleaseParentId = movement.ReleaseParentId;
        //                                            NewMovement.AddingFromPoid = movement.AddingFromPoid;
        //                                            NewMovement.OrderId = NewMatrialRelease.Id;
        //                                            NewMovement.OrderNumber = NewMatrialRelease.Id.ToString();
        //                                            NewMovement.AddingOrderItemId = MatrialReleaseItem.Id;
        //                                            NewMovement.Balance1 = -1 * RemainQTY;
        //                                            NewMovement.FinalBalance = -1 * RemainQTY;
        //                                            NewMovement.CreatedBy = validation.userID;
        //                                            NewMovement.ModifiedBy = validation.userID;
        //                                            NewMovement.CreationDate = DateTime.Now;
        //                                            NewMovement.ModifiedDate = DateTime.Now;

        //                                            _Context.InventoryStoreItems.Add(NewMovement);


        //                                            // Update Old Movement
        //                                            movement.Balance1 = -1 * diffrenceQTY;
        //                                            movement.FinalBalance = -1 * diffrenceQTY;
        //                                            movement.ModifiedBy = validation.userID;
        //                                            movement.ModifiedDate = DateTime.Now;

        //                                        }

        //                                        _Context.SaveChanges();
        //                                        break;
        //                                    }
        //                                    else  // New Item Release Movement less than 
        //                                    {
        //                                        // Update Old Movement 

        //                                        movement.OrderId = NewMatrialRelease.Id;
        //                                        movement.OrderNumber = NewMatrialRelease.Id.ToString();
        //                                        movement.AddingOrderItemId = MatrialReleaseItem.Id;
        //                                        movement.ModifiedBy = validation.userID;
        //                                        movement.ModifiedDate = DateTime.Now;

        //                                        //// New Movement
        //                                        //var NewMovement = new InventoryStoreItem();

        //                                        //NewMovement.OrderId = NewMatrialRelease.Id;
        //                                        //NewMovement.OrderNumber = NewMatrialRelease.Id.ToString();
        //                                        //NewMovement.AddingOrderItemId = MatrialReleaseItem.Id;
        //                                        //NewMovement.Balance1 = -1 * FinalBalance;
        //                                        //NewMovement.FinalBalance = -1 * FinalBalance;
        //                                        //NewMovement.CreatedBy = validation.userID;
        //                                        //NewMovement.ModifiedBy = validation.userID;
        //                                        //NewMovement.CreationDate = DateTime.Now;
        //                                        //NewMovement.ModifiedDate = DateTime.Now;

        //                                        //_Context.InventoryStoreItems.Add(NewMovement);

        //                                        //_Context.InventoryStoreItems.Remove(movement);
        //                                        _Context.SaveChanges();

        //                                        RemainQTY = RemainQTY - FinalBalance;
        //                                    }
        //                                }
        //                            }
        //                        }


        //                    }

        //                    // Remove Old Release  and Release Items

        //                    _Context.InventoryMatrialReleaseItems.RemoveRange(ParentMatrialReleaseItemList);
        //                    _Context.SaveChanges();
        //                    _Context.InventoryMatrialReleases.Remove(MatrialReleaseDB);
        //                    _Context.SaveChanges();

        //                }

        //                //_Context.Database.CommitTransaction();
        //            }

        //        }
        //        return Response;

        //    }
        //    catch (Exception ex)
        //    {
        //        //_Context.Database.RollbackTransaction();
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //}






















        [HttpPost("AddInventoryItemMatrialRelease")]
        public BaseResponseWithID AddInventoryItemMatrialRelease([FromBody] AddInventoryItemMatrialReleaseWithMatrialRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.AddInventoryItemMatrialRelease(BodyRequest, validation.userID);
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


        public static List<InventoryStoreItemIDWithQTY> GetParentReleaseIDWithSettingStore(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemID, int StoreID, int? StoreLocationID, decimal QTY, bool? IsFIFO)
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


        [HttpPost("UpdateMatrialReleaseStatus")]
        public BaseResponseWithID UpdateMatrialReleaseStatus([FromBody] UpdateMatrialReleaseStatusRequest BodyRequest)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.UpdateMatrialReleaseStatus(BodyRequest, validation.userID);
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


        // Get Matrial Release List by User filter 

        [HttpGet("GetMatrialReleaseIndexList")]
        public BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>> GetMatrialReleaseIndexList([FromHeader] GetMatrialReleaseIndexListFilters filters)
        {
            BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>> Response = new BaseResponseWithDataAndHeader<List<MatrialReleaseIndex>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.GetMatrialReleaseIndexList(filters);
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




        [HttpGet("GetMatrialReleaseCardsListWithItems")]
        public BaseResponseWithData<List<MatrialReleaseInfo>> GetMatrialReleaseCardsListWithItems([FromHeader] long? UserId, [FromHeader] string? FromDate, [FromHeader] string? ToDate)
        {
            BaseResponseWithData<List<MatrialReleaseInfo>> Response = new BaseResponseWithData<List<MatrialReleaseInfo>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.GetMatrialReleaseCardsListWithItems(UserId, FromDate, ToDate);
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


        [HttpGet("ViewMatrialRequestWithItems")]
        public ActionResult<BaseResponseWithData<MatrialRequestInfoDetails>> ViewMatrialRequestWithItems([FromHeader] long? MatrialRequestId)
        {
            var Response = new BaseResponseWithData<MatrialRequestInfoDetails>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.ViewMatrialRequestWithItems(MatrialRequestId);
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


        [HttpGet("ViewMatrialReleaseWithItems")]
        public BaseResponseWithData<MatrialReleaseInfoDetails> ViewMatrialReleaseWithItems([FromHeader] long? MatrialReleaseId)
        {
            var Response = new BaseResponseWithData<MatrialReleaseInfoDetails>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.ViewMatrialReleaseWithItems(MatrialReleaseId);
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

        //public BaseResponseWithID AddInventoryItemMatrialRequest(AddInventoryItemMatrialRequest Request)
        //{
        //    BaseResponseWithID Response = new BaseResponseWithID();
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

        //            long FromUserId = 0;
        //            int InventoryStoreID = 0;
        //            if (Request.FromUserId != 0)
        //            {
        //                FromUserId = Request.FromUserId;
        //            }
        //            else
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err45";
        //                error.ErrorMSG = "Invalid From User ID.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            if (Request.InventoryStoreID != 0)
        //            {
        //                var CheckInventorystorDB = _Context.InventoryStores.Where(x => x.Id == Request.InventoryStoreID).FirstOrDefault();
        //                if (CheckInventorystorDB == null)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err24";
        //                    error.ErrorMSG = "Invalid Inventory Store ID, not exist.";
        //                    Response.Errors.Add(error);
        //                    return Response;
        //                }
        //                InventoryStoreID = Request.InventoryStoreID;
        //            }
        //            else
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err24";
        //                error.ErrorMSG = "Invalid Inventory Store ID.";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }




        //            // validate is Count distinct < count of Items => there is iteration items with same data
        //            var ItemDistinctCount = Request.MatrialRequestItemList.Select(x => new { x.InventoryItemID }).Distinct().Count();
        //            if (ItemDistinctCount < Request.MatrialRequestItemList.Count())
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err27";
        //                error.ErrorMSG = "There is item itteration";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }

        //            int Counter = 0;
        //            var IDSMatrialRequest = Request.MatrialRequestItemList.Select(x => x.InventoryItemID).Distinct().ToList();
        //            var ListInventoryItems = _Context.InventoryItems.Where(x => IDSMatrialRequest.Contains(x.Id)).ToList();
        //            foreach (var item in Request.MatrialRequestItemList)
        //            {
        //                Counter++;
        //                if (item.InventoryItemID < 0)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "Err47";
        //                    error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
        //                    Response.Errors.Add(error);
        //                }
        //                else // Check is Inventoryt Item ID is valid
        //                {
        //                    var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == item.InventoryItemID).FirstOrDefault();
        //                    if (InventoryItemObjDB == null)
        //                    {
        //                        Response.Result = false;
        //                        Error error = new Error();
        //                        error.ErrorCode = "Err47";
        //                        error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
        //                        Response.Errors.Add(error);
        //                    }
        //                }

        //            }






        //            long InventoryMatrialRequestID = 0;
        //            if (Response.Result)
        //            {

        //                if (Response.Result)
        //                {
        //                    var MatrialRequest = new InventoryMatrialRequest();
        //                    MatrialRequest.FromUserId = FromUserId;
        //                    MatrialRequest.ToInventoryStoreId = InventoryStoreID;
        //                    MatrialRequest.RequestDate = DateTime.Now;
        //                    MatrialRequest.RequestTypeId = 20002;
        //                    MatrialRequest.ApproveResult = "Approved";
        //                    MatrialRequest.Status = "Open";
        //                    MatrialRequest.Active = true;
        //                    MatrialRequest.CreatedBy = Request.CreatorId;
        //                    MatrialRequest.ModifiedBy = Request.CreatorId;
        //                    MatrialRequest.ModifiedDate = DateTime.Now;
        //                    MatrialRequest.CreationDate = DateTime.Now;

        //                    _Context.InventoryMatrialRequests.Add(MatrialRequest);
        //                    var InventoryMatrialRequestInsertion = _Context.SaveChanges();


        //                    if (InventoryMatrialRequestInsertion > 0)
        //                    {
        //                        InventoryMatrialRequestID = MatrialRequest.Id;

        //                        #region items

        //                        if (Request.MatrialRequestItemList != null)
        //                        {
        //                            foreach (var MatrialDataOBJ in Request.MatrialRequestItemList)
        //                            {
        //                                var InventoryItemObjDB = ListInventoryItems.Where(x => x.Id == MatrialDataOBJ.InventoryItemID).FirstOrDefault();
        //                                if (InventoryItemObjDB != null)
        //                                {
        //                                    //add new 
        //                                    var InvMatrialRequestItemObjDB = new InventoryMatrialRequestItem();
        //                                    InvMatrialRequestItemObjDB.InventoryMatrialRequestId = InventoryMatrialRequestID;
        //                                    InvMatrialRequestItemObjDB.InventoryItemId = MatrialDataOBJ.InventoryItemID;
        //                                    InvMatrialRequestItemObjDB.Uomid = InventoryItemObjDB.RequstionUomid;
        //                                    InvMatrialRequestItemObjDB.Comments = MatrialDataOBJ.Comment;
        //                                    InvMatrialRequestItemObjDB.RecivedQuantity = 0; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
        //                                    InvMatrialRequestItemObjDB.ReqQuantity1 = MatrialDataOBJ.ReqQTY;
        //                                    InvMatrialRequestItemObjDB.PurchaseQuantity = 0;

        //                                    _Context.InventoryMatrialRequestItems.Add(InvMatrialRequestItemObjDB);
        //                                    _Context.SaveChanges();
        //                                }
        //                            }
        //                        }
        //                        #endregion items
        //                        Response.Result = true;
        //                        Response.ID = InventoryMatrialRequestID;
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
        //        return Response;
        //    }
        //}



        // 

        [HttpGet("GetStoresIdListForKeeper")]
        public BaseResponseWithData<List<StoresListForKeeper>> GetStoresIdListForKeeper()
        {
            BaseResponseWithData<List<StoresListForKeeper>> Response = new BaseResponseWithData<List<StoresListForKeeper>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    Response = _matrialReleaseForBYService.GetStoresIdListForKeeper(validation.userID);
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



        [HttpGet("GetItemMarketList")]
        public BaseResponseWithData<List<string>> GetItemMarketList()
        {
            BaseResponseWithData<List<string>> Response = new BaseResponseWithData<List<string>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    var InventoryItemMarketNameList = _Context.InventoryItems.Select(x => x.MarketName).Distinct().ToList();
                    Response.Data = InventoryItemMarketNameList;
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



        [HttpGet("GetItemCommercialList")]
        public BaseResponseWithData<List<string>> GetItemCommercialList()
        {
            BaseResponseWithData<List<string>> Response = new BaseResponseWithData<List<string>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {
                    var InventoryItemCommercialNameList = _Context.InventoryItems.Select(x => x.CommercialName).Distinct().ToList();
                    Response.Data = InventoryItemCommercialNameList;
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
