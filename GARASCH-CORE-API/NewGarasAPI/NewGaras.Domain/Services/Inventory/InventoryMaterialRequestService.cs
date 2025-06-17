using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Infrastructure.Entities;
using System.Net;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Helper;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryMaterialRequestService : IInventoryMaterialRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IAccountMovementService _movementService;
        private GarasTestContext _Context;
        public InventoryMaterialRequestService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, IAccountMovementService movementService, GarasTestContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _movementService = movementService;
            _Context = context;
        }

        public List<long> GetInvStoreIDAvailbileToHold(long InventoryItemID, int StoreID, decimal QTY)
        {
            List<long> InvStoreItemIDWithBalanceList = new List<long>();
            InvStoreItemIDWithBalanceList = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemID
                                                                                   && x.InventoryStoreId == StoreID
                                                                                   && (x.FinalBalance - (x.HoldQty ?? 0)) > 0
                                                                                   ).OrderBy(x => x.CreationDate).Select(x => x.Id).ToList();

            return InvStoreItemIDWithBalanceList;
        }

        public BaseResponseWithId<long> AddInventoryStoreWithMatrialRequest(AddInventoryStoreWithMatrialRequest Request,long creator)
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


                    long MatrialRequestId = 0;
                    InventoryMatrialRequest MatrialRequestDB = null;
                    if (Request.MatrialRequestId != 0 && Request.MatrialRequestId != null)
                    {
                        MatrialRequestId = (long)Request.MatrialRequestId;
                        MatrialRequestDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == MatrialRequestId).FirstOrDefault();
                        if (MatrialRequestDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-15";
                            error.ErrorMSG = "Invalid Matrial Request Id ,not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (MatrialRequestDB.Status != "Draft" && (Request.MatrialRequestItemList != null))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-15";
                            error.ErrorMSG = "This Request not draft to add extra itmes.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (Request.IsFinish == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please select Is Finish True Or False.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long RequestTypeID = 0;
                    long FromUserId = 0;
                    int InventoryStoreID = 0;
                    bool? IsFinish = Request.IsFinish;
                    DateTime RequestDate = DateTime.Now;
                    var InventoryStoreItemDBList = new List<InventoryStoreItem>();
                    string Status = "Draft";
                    string ApproveResult = "";
                    bool? IsHold = null;
                    if (_unitOfWork.InventoryMaterialRequestTypes.GetById(RequestTypeID)?.TypeName == "Hold")
                    {
                        IsHold = true;
                        Status = "Hold";
                    }
                    else
                        if (IsFinish == true)
                    {
                        Status = "Open";
                        ApproveResult = "Waiting For Reply";
                    }

                    if (Request.RequestTypeID != 0)
                    {
                        RequestTypeID = Request.RequestTypeID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err45";
                        error.ErrorMSG = "Invalid Request Type ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.FromUserId != 0)
                    {
                        FromUserId = Request.FromUserId;
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
                    if (Request.InventoryStoreID != 0)
                    {
                        var CheckInventorystorDB = _unitOfWork.InventoryStores.FindAll(x => x.Id == Request.InventoryStoreID).FirstOrDefault();
                        if (CheckInventorystorDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err24";
                            error.ErrorMSG = "Invalid Inventory Store ID, not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
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

                    if (string.IsNullOrEmpty(Request.RequestDate) || !DateTime.TryParse(Request.RequestDate, out RequestDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Request Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (MatrialRequestId == 0 || (MatrialRequestId != 0 && MatrialRequestDB.Status == "Draft" && Request.MatrialRequestItemList != null && Request.MatrialRequestItemList.Count() > 0))  // Insert will validate
                    {

                        if (Request.MatrialRequestItemList == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-15";
                            error.ErrorMSG = "please insert at least one Matrial Request Item.";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        if (Request.MatrialRequestItemList.Count <= 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-15";
                            error.ErrorMSG = "please insert at least one Matrial Request Item..";
                            Response.Errors.Add(error);
                            return Response;
                        }



                        // validate is Count distinct < count of Items => there is iteration items with same data
                        var ItemDistinctCount = Request.MatrialRequestItemList.Select(x => new { x.InventoryItemID, x.PojectID, x.Comment }).Distinct().Count();
                        if (ItemDistinctCount < Request.MatrialRequestItemList.Count())
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err27";
                            error.ErrorMSG = "There is item itteration";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (MatrialRequestId != 0 && MatrialRequestDB.Status == "Draft") //in case Update to add itmes to matrial request check Items selected not exist before with the same Project again 
                        {
                            var MatrialRequestItmesListModel = Request.MatrialRequestItemList.Select(x => new InvetoryItemIdWithProjectId
                            {
                                InventoryItemID = x.InventoryItemID,
                                PojectID = x.PojectID
                                ,
                                Comment = x.Comment
                            }).Distinct().ToList();
                            var MatrialRequestItmesListdb = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialRequestId)
                                .Select(x => new InvetoryItemIdWithProjectId { InventoryItemID = x.InventoryItemId, PojectID = x.ProjectId, Comment = x.Comments }).Distinct().ToList();
                            var existingObjects = MatrialRequestItmesListdb.Where(item => MatrialRequestItmesListModel.Any(obj => obj.InventoryItemID == item.InventoryItemID && obj.PojectID == item.PojectID
                              && obj.Comment == item.Comment)).Count();
                            if (existingObjects > 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err27";
                                error.ErrorMSG = "There is item itteration with same Itme and Project";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }
                        if (Status == "Hold") // conditions for Hold Status Only
                        {
                            var IDSItems = Request.MatrialRequestItemList.Select(x => x.InventoryItemID).ToList();
                            var IDSInventoryStoreItemsRequest = Request.MatrialRequestItemList.Where(x => x.InventoryStoreItemIDsList != null).SelectMany(x => x.InventoryStoreItemIDsList).ToList();
                            InventoryStoreItemDBList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSInventoryStoreItemsRequest.Contains(x.Id) || (x.InventoryStoreId == InventoryStoreID && IDSItems.Contains(x.InventoryItemId))).ToList();
                        }

                        int Counter = 0;

                        var ProjectsIDSList = Request.MatrialRequestItemList.Where(x => x.PojectID != null).Select(x => x.PojectID).ToList();
                        var FabOrderListDB = _unitOfWork.ProjectFabrications.FindAll(x => ProjectsIDSList.Contains(x.ProjectId)).ToList();
                        var IDSFab = FabOrderListDB.Select(x => x.Id).ToList();
                        var FabOrderItemsListDB = _unitOfWork.ProjectFabricationBOQs.FindAll(x => IDSFab.Contains(x.ProjectFabricationId)).ToList();
                        foreach (var item in Request.MatrialRequestItemList)
                        {
                            Counter++;
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
                                var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(item.InventoryItemID);
                                if (InventoryItemObjDB == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err47";
                                    error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                            }
                            if (item.PojectID != null)
                            {

                                if (item.PojectID < 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err48";
                                    error.ErrorMSG = "Invalid Project Selected item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                // Check Project is valid
                                var CheckProjectDB = _unitOfWork.Projects.FindAll(x => x.Id == item.PojectID).FirstOrDefault();
                                if (CheckProjectDB == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err48";
                                    error.ErrorMSG = "Invalid Project Selected item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    // Validate Releations Between Project ID , Fab Order , Fab BOQ
                                    if (item.FabOrderID != null)
                                    {
                                        var CheckProjectFab = FabOrderListDB.Where(x => x.Id == item.FabOrderID).FirstOrDefault();
                                        if (CheckProjectFab != null)
                                        {
                                            if (CheckProjectFab.ProjectId != item.PojectID)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err48";
                                                error.ErrorMSG = "Fab ID not releated to this Project Selected item #" + Counter;
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err48";
                                            error.ErrorMSG = "Fab Order ID not exist Selected item #" + Counter;
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }

                            }
                            if (item.FabOrderID != null && item.FabOrderID != 0 && (item.PojectID == null || item.PojectID == 0))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err48";
                                error.ErrorMSG = "Can't select Fab ID because the project is not selected yet #" + Counter;
                                Response.Errors.Add(error);
                            }
                            if (item.FabOrderItemID != null && item.FabOrderItemID != 0 && (item.PojectID == null || item.PojectID == 0))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err48";
                                error.ErrorMSG = "Can't select Fab Order Item ID because the project is not selected yet #" + Counter;
                                Response.Errors.Add(error);
                            }

                            if (item.FabOrderItemID != null && item.FabOrderItemID != 0 && (item.FabOrderID == null || item.FabOrderID == 0))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err48";
                                error.ErrorMSG = "Can't select Fab Order Item ID because the Fab ID is not selected yet #" + Counter;
                                Response.Errors.Add(error);
                            }

                            //if (item.FabOrderID < 0)
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err46";
                            //    error.ErrorMSG = "Invalid Fabrication order selected for item #" + Counter;
                            //    Response.Errors.Add(error);
                            //}

                            //if (item.FabOrderID == null)
                            //{
                            //    item.FabOrderID = 0;
                            //}
                            if (item.OfferItemID < 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err56";
                                error.ErrorMSG = "Invalid Offer Item selected for item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            if (item.ReqQTY <= 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err49";
                                error.ErrorMSG = "Invalid Quantity selected for item #" + Counter;
                                Response.Errors.Add(error);
                            }

                            if (item.FromBOM == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-116";
                                error.ErrorMSG = "please select From Bom True Or False for item #" + Counter;
                                Response.Errors.Add(error);
                                return Response;
                            }


                            // Modifications on 2022-12-21
                            // Check if Request Type Hold .. must be select Batch and expDate and back InventoryStoreItemID
                            if (Status == "Hold")
                            {



                                if (item.InventoryStoreItemIDsList == null || item.InventoryStoreItemIDsList.Count() == 0)
                                {
                                    item.InventoryStoreItemIDsList = GetInvStoreIDAvailbileToHold(item.InventoryItemID,
                                                                                                    Request.InventoryStoreID,
                                                                                                    (decimal)item.ReqQTY);

                                    //Response.Result = false;
                                    //Error error = new Error();
                                    //error.ErrorCode = "Err-44";
                                    //error.ErrorMSG = "InventoryStoreItemID is required ,because Request type is hold for item #" + Counter;
                                    //Response.Errors.Add(error);
                                }

                                if (item.InventoryStoreItemIDsList != null && item.InventoryStoreItemIDsList.Count() > 0)
                                {
                                    var CheckInventoryStoreItemListDB = InventoryStoreItemDBList.Where(x => item.InventoryStoreItemIDsList.Contains(x.Id)).ToList();
                                    if (CheckInventoryStoreItemListDB.Count() > 0)
                                    {
                                        // Check Req Qty <= Qty on store item to make hold
                                        if (item.ReqQTY > (CheckInventoryStoreItemListDB.Sum(x => x.FinalBalance - (x.HoldQty ?? 0))))
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err-44";
                                            error.ErrorMSG = "ReqQTY is greater than available Qty for item #" + Counter;
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-44";
                                        error.ErrorMSG = "Invalid InventoryStoreItemIDs List for item #" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-44";
                                    error.ErrorMSG = "This Item not have stock for item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                            }
                        }


                    }




                    long InventoryMatrialRequestID = 0;
                    if (Response.Result)
                    {
                        // Check Inventory Report Approved and closed or not
                        var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();
                        if (CheckInventoryReportListDB.Count > 0)
                        {
                            foreach (var InventoryRep in CheckInventoryReportListDB)
                            {
                                if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                {
                                    string storeName = _unitOfWork.InventoryStores.GetById(InventoryStoreID)?.Name;
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



                            if (MatrialRequestId != 0) // Update 
                            {
                                MatrialRequestDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == MatrialRequestId).FirstOrDefault();
                                if (MatrialRequestDB != null)
                                {
                                    MatrialRequestDB.Status = Status;
                                    if (FromUserId != 0)
                                    {
                                        MatrialRequestDB.FromUserId = FromUserId;
                                    }
                                    if (InventoryStoreID != 0)
                                    {
                                        MatrialRequestDB.ToInventoryStoreId = InventoryStoreID;
                                    }
                                    MatrialRequestDB.RequestDate = RequestDate;
                                    MatrialRequestDB.Active = true;
                                    if (RequestTypeID != 0)
                                    {
                                        MatrialRequestDB.RequestTypeId = RequestTypeID;
                                    }
                                    if (ApproveResult != null)
                                    {
                                        MatrialRequestDB.ApproveResult = ApproveResult;
                                    }


                                    if (Request.MatrialRequestItemList != null && MatrialRequestDB.Status == "Draft")
                                    {
                                        var IDSProjects = Request.MatrialRequestItemList.Select(y => y.PojectID).ToList();
                                        var ProjectListDB = _unitOfWork.Projects.FindAll(x => IDSProjects.Contains(x.Id)).ToList();
                                        var IDSalesOffers = ProjectListDB.Select(y => y.SalesOfferId);
                                        var SalesOfferProdListDB = _unitOfWork.SalesOfferProducts.FindAll(x =>                          IDSalesOffers.Contains(x.OfferId)).ToList();
                                        foreach (var MatrialDataOBJ in Request.MatrialRequestItemList)
                                        {
                                            bool FromBom = false;
                                            if (MatrialDataOBJ.FromBOM != null)
                                            {
                                                FromBom = (bool)MatrialDataOBJ.FromBOM;
                                            }
                                            var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(MatrialDataOBJ.InventoryItemID);
                                            if (InventoryItemObjDB != null)
                                            {
                                                //add new 
                                                var InvMatrialRequestItemObjDB = new InventoryMatrialRequestItem();
                                                InvMatrialRequestItemObjDB.InventoryMatrialRequestId = MatrialRequestId;
                                                InvMatrialRequestItemObjDB.InventoryItemId = MatrialDataOBJ.InventoryItemID;
                                                InvMatrialRequestItemObjDB.Uomid = InventoryItemObjDB.RequstionUomid;
                                                InvMatrialRequestItemObjDB.ProjectId = MatrialDataOBJ.PojectID;
                                                InvMatrialRequestItemObjDB.FabricationOrderId = MatrialDataOBJ.FabOrderID;
                                                InvMatrialRequestItemObjDB.FabricationOrderItemId = MatrialDataOBJ.FabOrderItemID;
                                                InvMatrialRequestItemObjDB.Comments = MatrialDataOBJ.Comment;
                                                InvMatrialRequestItemObjDB.RecivedQuantity1 = 0; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                                InvMatrialRequestItemObjDB.ReqQuantity1 = MatrialDataOBJ.ReqQTY;
                                                InvMatrialRequestItemObjDB.PurchaseQuantity1 = 0;
                                                InvMatrialRequestItemObjDB.FromBom = FromBom;
                                                InvMatrialRequestItemObjDB.IsHold = IsHold;

                                                if (MatrialDataOBJ.OfferItemID != null && MatrialDataOBJ.OfferItemID != 0)
                                                {
                                                    InvMatrialRequestItemObjDB.OfferItemId = MatrialDataOBJ.OfferItemID;
                                                }
                                                else
                                                {
                                                    // Get Offer Item
                                                    if (MatrialDataOBJ.PojectID != null && MatrialDataOBJ.InventoryItemID != 0)
                                                    {
                                                        var SalesOfferID = ProjectListDB.Where(x => x.Id == MatrialDataOBJ.PojectID).Select(x => x.SalesOfferId).FirstOrDefault();
                                                        var OfferItemID = SalesOfferProdListDB.Where(x => x.OfferId == SalesOfferID && x.InventoryItemId == MatrialDataOBJ.InventoryItemID).Select(x => x.Id).FirstOrDefault();
                                                        InvMatrialRequestItemObjDB.OfferItemId = OfferItemID;
                                                    }
                                                }

                                                _unitOfWork.InventoryMatrialRequestItems.Add(InvMatrialRequestItemObjDB);
                                            }

                                        }
                                    }


                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-15";
                                    error.ErrorMSG = "Invalid Matrial Request Id ,not exist.";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else
                            {
                                var InventoryMatrialRequest = new InventoryMatrialRequest()
                                {
                                    FromUserId = FromUserId,
                                    ToInventoryStoreId = InventoryStoreID,
                                    RequestDate = RequestDate,
                                    CreationDate = DateTime.Now,
                                    CreatedBy = creator,
                                    ModifiedDate = DateTime.Now,
                                    ModifiedBy = creator,
                                    Active = true,
                                    Status = Status,
                                    RequestTypeId = RequestTypeID,
                                    ApproveResult = ApproveResult,
                                    ApproveRejectNotes = null
                                };

                                _unitOfWork.InventoryMatrialRequests.Add(InventoryMatrialRequest);
                                var InventoryMatrialRequestInsertion = _unitOfWork.Complete();

                                if (InventoryMatrialRequestInsertion > 0)
                                {
                                    InventoryMatrialRequestID = (long)InventoryMatrialRequest.Id;
                                    #region items

                                    if (Request.MatrialRequestItemList != null)
                                    {
                                        var IDSProjects = Request.MatrialRequestItemList.Select(y => y.PojectID).ToList();
                                        var ProjectListDB = _unitOfWork.Projects.FindAll(x => IDSProjects.Contains(x.Id)).ToList();
                                        var IDSalesOffers = ProjectListDB.Select(y => y.SalesOfferId);
                                        var SalesOfferProdListDB = _unitOfWork.SalesOfferProducts.FindAll(x => IDSalesOffers.Contains(x.OfferId)).ToList();
                                        foreach (var MatrialDataOBJ in Request.MatrialRequestItemList)
                                        {
                                            bool FromBom = false;
                                            if (MatrialDataOBJ.FromBOM != null)
                                            {
                                                FromBom = (bool)MatrialDataOBJ.FromBOM;
                                            }
                                            var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(MatrialDataOBJ.InventoryItemID);
                                            if (InventoryItemObjDB != null)
                                            {
                                                //add new 
                                                var InvMatrialRequestItemObjDB = new InventoryMatrialRequestItem();
                                                InvMatrialRequestItemObjDB.InventoryMatrialRequestId = InventoryMatrialRequestID;
                                                InvMatrialRequestItemObjDB.InventoryItemId = MatrialDataOBJ.InventoryItemID;
                                                InvMatrialRequestItemObjDB.Uomid = InventoryItemObjDB.RequstionUomid;
                                                InvMatrialRequestItemObjDB.ProjectId = MatrialDataOBJ.PojectID;
                                                InvMatrialRequestItemObjDB.FabricationOrderId = MatrialDataOBJ.FabOrderID;
                                                InvMatrialRequestItemObjDB.FabricationOrderItemId = MatrialDataOBJ.FabOrderItemID;
                                                InvMatrialRequestItemObjDB.Comments = MatrialDataOBJ.Comment;
                                                InvMatrialRequestItemObjDB.RecivedQuantity1 = 0; // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                                InvMatrialRequestItemObjDB.ReqQuantity1 = MatrialDataOBJ.ReqQTY;
                                                InvMatrialRequestItemObjDB.PurchaseQuantity1 = 0;
                                                InvMatrialRequestItemObjDB.FromBom = FromBom;
                                                InvMatrialRequestItemObjDB.IsHold = IsHold;

                                                if (MatrialDataOBJ.OfferItemID != null && MatrialDataOBJ.OfferItemID != 0)
                                                {
                                                    InvMatrialRequestItemObjDB.OfferItemId = MatrialDataOBJ.OfferItemID;
                                                }
                                                else
                                                {
                                                    // Get Offer Item
                                                    if (MatrialDataOBJ.PojectID != null && MatrialDataOBJ.InventoryItemID != 0)
                                                    {
                                                        var SalesOfferID = ProjectListDB.Where(x => x.Id == MatrialDataOBJ.PojectID).Select(x => x.SalesOfferId).FirstOrDefault();
                                                        var OfferItemID = SalesOfferProdListDB.Where(x => x.OfferId == SalesOfferID && x.InventoryItemId == MatrialDataOBJ.InventoryItemID).Select(x => x.Id).FirstOrDefault();
                                                        InvMatrialRequestItemObjDB.OfferItemId = OfferItemID;
                                                    }
                                                }

                                                _unitOfWork.InventoryMatrialRequestItems.Add(InvMatrialRequestItemObjDB);
                                                _unitOfWork.Complete();

                                                //ObjectParameter IDInventoryMatrialRequestItem = new ObjectParameter("ID", typeof(long));
                                                //var MatrialRequestItemInsertion = _Context.Myproc_InventoryMatrialRequestItemsInsert_New(IDInventoryMatrialRequestItem,
                                                //                                                                                InventoryMatrialRequestID,
                                                //                                                                                MatrialDataOBJ.InventoryItemID,
                                                //                                                                                InventoryItemObjDB.RequstionUOMID,
                                                //                                                                                MatrialDataOBJ.PojectID,
                                                //                                                                                MatrialDataOBJ.FabOrderID,
                                                //                                                                                MatrialDataOBJ.Comment,
                                                //                                                                                0, // Recived Qty when Matrial Type is Hold ..... this QTY is Hold QTY
                                                //                                                                                MatrialDataOBJ.ReqQTY,
                                                //                                                                                0,
                                                //                                                                                FromBom,
                                                //                                                                                MatrialDataOBJ.OfferItemID,
                                                //                                                                                IsHold
                                                //                                                                             );


                                                // if Select Request Type update inventory store item id 
                                                if (Status == "Hold" && IsHold == true)
                                                {

                                                    decimal RemainHoldQTY = (decimal)MatrialDataOBJ.ReqQTY;
                                                    foreach (var StoreItemID in MatrialDataOBJ.InventoryStoreItemIDsList)  // 20 -  10   - 5
                                                    {
                                                        var InventoryStoreItemObjDB = InventoryStoreItemDBList.Where(x => x.Id == StoreItemID).FirstOrDefault();
                                                        if (InventoryStoreItemObjDB != null)
                                                        {
                                                            decimal HoldQTY = 0;
                                                            decimal AvailableQTY = (decimal)(InventoryStoreItemObjDB.FinalBalance - (InventoryStoreItemObjDB.HoldQty ?? 0));
                                                            if (RemainHoldQTY <= AvailableQTY)
                                                            {
                                                                HoldQTY = RemainHoldQTY;
                                                            }
                                                            else
                                                            {
                                                                HoldQTY = (decimal)(InventoryStoreItemObjDB.FinalBalance - (InventoryStoreItemObjDB.HoldQty ?? 0));
                                                            }



                                                            if (RemainHoldQTY > 0)
                                                            {
                                                                RemainHoldQTY -= HoldQTY;

                                                                InventoryStoreItemObjDB.HoldQty = (InventoryStoreItemObjDB.HoldQty ?? 0) + HoldQTY;
                                                                InventoryStoreItemObjDB.HoldReason = Request.HoldReason;
                                                                InventoryStoreItemObjDB.ModifiedBy = creator;
                                                                InventoryStoreItemObjDB.ModifiedDate = DateTime.Now;

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
                                    Response.ID = InventoryMatrialRequestID;
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

        public List<long> GetInvStoreIDAvailbileToReleaseHold(long InventoryItemID, decimal QTY)
        {
            List<long> InvStoreItemIDWithBalanceList = new List<long>();
            InvStoreItemIDWithBalanceList = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemID
                                                                                   && (x.HoldQty ?? 0) > 0
                                                                                   ).OrderBy(x => x.CreationDate).Select(x => x.Id).ToList();

            return InvStoreItemIDWithBalanceList;
        }
        public InventoryItemMatrialRequest GetInventoryItemMatrialRequestList(GetInventoryItemMatrialFilters filters)
        {
            InventoryItemMatrialRequest Response = new InventoryItemMatrialRequest();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var inventoryMtrialRequestByDateList = new List<InventoryMtrialRequestByDate>();
                if (Response.Result)
                {
                    var InventoryMatrialRequest = _unitOfWork.VInventoryMatrialRequests.FindAllQueryable(a=>true);

                    if (filters.InventoryStoreID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.ToInventoryStoreId == filters.InventoryStoreID).AsQueryable();
                    }

                    if (filters.CreatorUserID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.FromUserID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.FromUserId == filters.FromUserID).AsQueryable();
                    }
                    if (filters.MatrialRequestTypeID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.RequestTypeId == filters.MatrialRequestTypeID).AsQueryable();
                    }
                    if (filters.RequestDate != null)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.RequestDate == filters.RequestDate).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        var IDInventoryMatrialRequest = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryMatrialRequestId).Distinct().ToList();

                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => IDInventoryMatrialRequest.Contains(x.Id)).AsQueryable();
                    }

                    var InventoryMatrialRequestFiltered = InventoryMatrialRequest.ToList().OrderByDescending(x => x.CreationDate).GroupBy(a=>a.Id).Select(a=>a.FirstOrDefault()).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();
                    foreach (var MatrialPerMonth in InventoryMatrialRequestFiltered)
                    {
                        var InventoryMatrialAddingOrderInfoList = new List<InventoryMatrialRequestInfo>();

                        foreach (var Data in MatrialPerMonth)
                        {

                            InventoryMatrialAddingOrderInfoList.Add(new InventoryMatrialRequestInfo
                            {
                                InventoryMatrialRequestNo = Data.Id.ToString(),
                                FromUserName = Data.FromUserName,
                                StoreName = Data.ToInventoryStoreName,
                                Status = Data.Status,
                                IsHold = Data.RequestType == "Hold" && Data.Status == "Hold" ? true : false
                            });
                        }
                        inventoryMtrialRequestByDateList.Add(new InventoryMtrialRequestByDate()
                        {
                            DateMonth = Common.GetMonthName(MatrialPerMonth.Key.month) + " " + MatrialPerMonth.Key.year.ToString(),
                            //item.Select(x=>x.CreatedDate.Month).ToString(),
                            InventoryMatrialRequestInfoList = InventoryMatrialAddingOrderInfoList,
                        });
                    }

                    Response.InventoryMtrialRequestByDateList = inventoryMtrialRequestByDateList;

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


        public async Task<InventoryItemMatrialRequestPagingResponse> GetInventoryMatrialRequestListPaging(InventoryItemMatrialPagingFilters filters,long creator)
        {
            InventoryItemMatrialRequestPagingResponse Response = new InventoryItemMatrialRequestPagingResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    if (filters.Status!=null)
                    {
                        if (filters.Status.ToLower() != "open" && filters.Status.ToLower() != "closed" && filters.Status.ToLower() != "draft")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err413";
                            error.ErrorMSG = "Status must be open , closed or Draft";
                            Response.Errors.Add(error);
                        }
                    }
                    var InventoryMatrialRequest = _unitOfWork.InventoryMatrialRequests.FindAllQueryable(a => true, includes: new[] { "FromUser", "ToInventoryStore", "CreatedByNavigation", "RequestType", "InventoryMatrialRequestItems" });
                    // filter if have role to view all branchs
                    if (!await Common.CheckUserRoleAsync(creator, 35,_Context))
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.FromUserId == creator ||
                        x.ToInventoryStore.InventoryStoreKeepers.Select(k => k.UserId).FirstOrDefault() == creator || x.CreatedBy == creator);
                    }
                    var InventoryMatrialRequestItems = _unitOfWork.VInventoryMatrialRequestItems.FindAllQueryable(a=>true);
                    if (filters.RequestNo != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.Id == filters.RequestNo).AsQueryable();
                    }
                    if (filters.Status!=null)
                    {
                        string Status = filters.Status;
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.Status.ToLower() == Status).AsQueryable();
                    }
                    if (filters.InventoryStoreID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.ToInventoryStoreId == filters.InventoryStoreID).AsQueryable();
                    }

                    if (filters.CreatorUserID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.FromUserID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.FromUserId == filters.FromUserID).AsQueryable();
                    }
                    if (filters.MatrialRequestTypeID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.RequestTypeId == filters.MatrialRequestTypeID).AsQueryable();
                    }
                    if (filters.RequestDate != null)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.RequestDate == filters.RequestDate).AsQueryable();
                    }
                    if (filters.Comment != null)
                    {
                        var IDInventoryMatrialRequest = InventoryMatrialRequestItems.Where(x => x.Comments.Contains(filters.Comment)).Select(x => x.InventoryMatrialRequestId).Distinct().ToList();

                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => IDInventoryMatrialRequest.Contains(x.Id)).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        var IDInventoryMatrialRequest = InventoryMatrialRequestItems.Where(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryMatrialRequestId).Distinct().ToList();

                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => IDInventoryMatrialRequest.Contains(x.Id)).AsQueryable();
                    }

                    if (filters.ProjectID != 0)
                    {
                        var IDInventoryMatrialRequest = InventoryMatrialRequestItems.Where(x => x.ProjectId == filters.ProjectID).Select(x => x.InventoryMatrialRequestId).Distinct().ToList();

                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => IDInventoryMatrialRequest.Contains(x.Id)).AsQueryable();
                    }


                    if (filters.ClientID != 0)
                    {
                        var IDInventoryMatrialRequest = InventoryMatrialRequestItems.Where(x => x.ClientId == filters.ClientID).Select(x => x.InventoryMatrialRequestId).Distinct().ToList();

                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => IDInventoryMatrialRequest.Contains(x.Id)).AsQueryable();
                    }
                    if (filters.UserId != 0)
                    {
                        if (Common.CheckUserRole(filters.UserId, 113,_Context))
                        {
                            var storesId = _Context.InventoryStoreKeepers.Where(x => x.Active && x.UserId == filters.UserId).Select(x => x.InventoryStoreId).ToList();
                            if (storesId.Count() > 0)
                            {
                                InventoryMatrialRequest = InventoryMatrialRequest.Where(a => storesId.Contains(a.ToInventoryStoreId)).AsQueryable();
                            }
                        }

                    }
                    var DataDistinctList = InventoryMatrialRequest.Select(x => new InventoryMatrialRequestInfoForPaging
                    {
                        ID = x.Id,
                        InventoryMatrialRequestNo = x.Id.ToString(),
                        FromUserName = x.FromUser.FirstName + " " + x.FromUser.LastName,
                        StoreName = x.ToInventoryStore.Name,
                        Status = x.Status,
                        CreatorName = x.CreatedByNavigation.FirstName + " " + x.CreatedByNavigation.LastName, // Created By
                        CreatedBy = x.CreatedBy,
                        RequestType = x.RequestType.TypeName,
                        CreationDate = x.CreationDate.ToString("yyyy-MM-dd"),
                        RequestDate = x.RequestDate.ToString("yyyy-MM-dd"),
                        ProjectNameList = null,

                        IsHold = x.RequestType.TypeName == "Hold" && x.Status == "Hold" ? true : false,
                    }).AsQueryable();
                    var PagingList = PagedList<InventoryMatrialRequestInfoForPaging>.Create(DataDistinctList, filters.CurrentPage, filters.NumberOfItemsPerPage);
                    var requestlist = _unitOfWork.InventoryMatrialRequestItems.FindAll(a => PagingList.Select(x=>x.ID).Contains(a.InventoryMatrialRequestId), includes: new[] { "Project.SalesOffer.Client" });
                    PagingList.ForEach(x => x.ProjectNameList = requestlist.Where(a => a.InventoryMatrialRequestId == x.ID)?.Select(i => i.Project?.SalesOffer?.ProjectName + " - " + i.Project?.SalesOffer?.Client?.Name).Distinct().ToList());

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = PagingList.TotalPages,
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = PagingList.TotalCount
                    };
                    Response.MatrialRequestDataList = PagingList.ToList();

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
        public InventoryItemMatrialRequestInfoResponse GetInventoryItemMatrialRequestInfo([FromHeader] long MatrialRequestOrderID,long creator)
        {
            InventoryItemMatrialRequestInfoResponse Response = new InventoryItemMatrialRequestInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemMatrialRequestInfoObj = new InventoryItemMatrialRequestInfo();
                var MatrialRequestInfList = new List<MatrialRequestInfo>();
                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        var MatrialRequestOBJDB = _unitOfWork.InventoryMatrialRequests.FindAll(x => x.Id == MatrialRequestOrderID, includes: new[] { "FromUser", "ToInventoryStore" }).FirstOrDefault();
                        // inseted proc <- from view 
                        if (MatrialRequestOBJDB != null)
                        {
                            InventoryItemMatrialRequestInfoObj.InventoryMatrialRequestOrderID = MatrialRequestOrderID;
                            InventoryItemMatrialRequestInfoObj.FromUserId = MatrialRequestOBJDB.FromUserId;
                            InventoryItemMatrialRequestInfoObj.FromUserName = MatrialRequestOBJDB.FromUser?.FirstName + " " + MatrialRequestOBJDB.FromUser?.LastName;// Common.GetUserName(InventoryAddingOrderOBJDB.FromUserID);
                            InventoryItemMatrialRequestInfoObj.UserDept = _unitOfWork.Users.Find(a => a.Id == creator, includes: new[] { "Department" })?.Department?.Name;
                            InventoryItemMatrialRequestInfoObj.RequestTypeId = MatrialRequestOBJDB.RequestTypeId;
                            InventoryItemMatrialRequestInfoObj.RequestType = MatrialRequestOBJDB.RequestTypeId != null ? _unitOfWork.InventoryMaterialRequestTypes.GetById((long)MatrialRequestOBJDB.RequestTypeId)?.TypeName : "";
                            InventoryItemMatrialRequestInfoObj.ToStore = MatrialRequestOBJDB.ToInventoryStore?.Name; //  Common.getInventoryStoreName(InventoryAddingOrderOBJDB.ToInventoryStoreID);
                            InventoryItemMatrialRequestInfoObj.ToStoreId = MatrialRequestOBJDB.ToInventoryStoreId;
                            InventoryItemMatrialRequestInfoObj.CreationDate = MatrialRequestOBJDB.CreationDate.ToShortDateString();
                            InventoryItemMatrialRequestInfoObj.RequestDate = MatrialRequestOBJDB.RequestDate.ToShortDateString() ?? "";
                            InventoryItemMatrialRequestInfoObj.Status = MatrialRequestOBJDB.Status;

                            if (MatrialRequestOBJDB.Status == "Open")
                            {
                                InventoryItemMatrialRequestInfoObj.CanCreateRelease = true;
                                InventoryItemMatrialRequestInfoObj.CanCreatePR = false;

                                // Check Inventory Report Approved and closed or not
                                var CheckToInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.Active == true && x.Approved == false && x.Closed == false && x.InventoryStoreId == MatrialRequestOBJDB.ToInventoryStoreId).ToList();
                                if (CheckToInventoryReportListDB.Count > 0)
                                {
                                    foreach (var InventoryRep in CheckToInventoryReportListDB)
                                    {
                                        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                        {
                                            InventoryItemMatrialRequestInfoObj.CanCreateRelease = false;
                                            break;
                                        }
                                    }
                                }
                            }




                            var ListOfMatrialRequestItemListDB = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialRequestOrderID, includes: new[] {"Uom", "InventoryItem", "Project.ProjectFabrications", "Project.SalesOffer", "Project.SalesOffer.Client" }).ToList();
                            var IDSMatrialRequestItemList = ListOfMatrialRequestItemListDB.Select(x => x.Id).ToList();
                            var IDSInvStoreItemList = ListOfMatrialRequestItemListDB.Select(x => x.InventoryItemId).ToList();
                            var ListOfInventoryStoreItemsListDB = _Context.InventoryStoreItems.Where(x => IDSInvStoreItemList.Contains(x.InventoryItemId)).ToList();
                            if (ListOfMatrialRequestItemListDB != null)
                            {

                                string MaterialRequestStatus = InventoryItemMatrialRequestInfoObj.Status == "Hold" || InventoryItemMatrialRequestInfoObj.Status == "Hold Released" ? InventoryItemMatrialRequestInfoObj.Status : "";
                                var CheckISInventoryKeeper = _unitOfWork.VInventoryStoreKeepers.FindAll(x => x.InventoryStoreId == MatrialRequestOBJDB.ToInventoryStoreId && x.UserId == creator && x.Active == true && x.StoreActive == true).FirstOrDefault() != null?true:false ;
                                var InventoryStoreItemList = _unitOfWork.InventoryStoreItems
                                 .FindAll(x => x.InventoryStoreId == MatrialRequestOBJDB.ToInventoryStoreId && IDSInvStoreItemList.Contains(x.InventoryItemId)).ToList();
                                var V_InventoryMatrialReleaseItems = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => IDSMatrialRequestItemList.Contains(x.InventoryMatrialRequestItemId)).ToList();
                                var V_PurchaseRequestItems = _unitOfWork.VPurchaseRequestItems.FindAll(x => IDSMatrialRequestItemList.Contains(x.InventoryMatrialRequestItemId)).ToList();
                                foreach (var item in ListOfMatrialRequestItemListDB)
                                {
                                    var MatrialAddingOrderInfoObj = new MatrialRequestInfo();
                                    MatrialAddingOrderInfoObj.InventoryMatrialRequestID = item.InventoryMatrialRequestId;
                                    MatrialAddingOrderInfoObj.InventoryMatrialRequestItemID = item.Id;
                                    MatrialAddingOrderInfoObj.HoldReleaseQTY = item.RecivedQuantity1 != null ? item.RecivedQuantity1.ToString() : "0"; // Set Hold Released on Recived qty and used on matrial request item info
                                    MatrialAddingOrderInfoObj.MatrialRequestStatus = MaterialRequestStatus;
                                    //item.MaterialRequestStatus == "Hold" || item.MaterialRequestStatus == "Hold Released" ? item.MaterialRequestStatus : "";
                                    MatrialAddingOrderInfoObj.MatrialRequestItemIsHold = item.IsHold != null ? (bool)item.IsHold : false;

                                    MatrialAddingOrderInfoObj.InventoryItemID = item.InventoryItemId;
                                    MatrialAddingOrderInfoObj.ItemName = item.InventoryItem?.Name;
                                    // MatrialAddingOrderInfoObj.ProductSerial = item.OfferItemID != null ?  Common.GetProductSerialFromOfferItem(item.OfferItemID) : "";
                                    MatrialAddingOrderInfoObj.ReqQTY = item.ReqQuantity1 != null ? item.ReqQuantity1.ToString() : "0";
                                    MatrialAddingOrderInfoObj.UOM = item.Uom?.ShortName;
                                    MatrialAddingOrderInfoObj.ItemCode = item.InventoryItem?.Code;
                                    MatrialAddingOrderInfoObj.ProjectName = item.Project?.SalesOffer?.ProjectName;
                                    MatrialAddingOrderInfoObj.ProjectSerial = item.Project?.ProjectSerial;
                                    MatrialAddingOrderInfoObj.ClientName = item.Project?.SalesOffer?.Client?.Name;
                                    MatrialAddingOrderInfoObj.Comment = item.Comments;
                                    MatrialAddingOrderInfoObj.ProjectId = item.ProjectId;
                                    MatrialAddingOrderInfoObj.FabricationOrderId = item.FabricationOrderId;
                                    MatrialAddingOrderInfoObj.FabOrderName = item.Project != null ? item.Project?.ProjectFabrications?.Where(x => x.Id == item.FabricationOrderId).Select(x => x.FabNumber)?.FirstOrDefault()?.ToString() : "";
                                    MatrialAddingOrderInfoObj.IDSOrderReleaseList =//item.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRequestItemID == item.ID).Select(x => new OrderReleaseObj { Id = x.InventoryMatrialReleasetID, Qty = x.NewRecivedQuantity ?? 0 }).ToList();
                                    V_InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRequestItemId == item.Id).Select(x => new OrderReleaseObj { Id = x.InventoryMatrialReleasetId, Qty = x.NewRecivedQuantity ?? 0 }).ToList();
                                    //_Context.V_InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRequestItemID == item.ID).Select(x => new OrderReleaseObj { Id = x.InventoryMatrialReleasetID, Qty = /x.NewRecivedQuantity ?? 0 }).ToList();
                                    MatrialAddingOrderInfoObj.IDSpurchaseRequestList = V_PurchaseRequestItems.Where(x => x.InventoryMatrialRequestItemId == item.Id).Select(x => new OrderReleaseObj { Id = x.PurchaseRequestId, Qty = x.Quantity ?? 0 }).ToList();
                                    //var test = InventoryStoreItemList.Where(x => x.InventoryItemID == item.InventoryItemID).GroupBy(x => x.InventoryItemID).ToList();
                                    // modified from sum Column balance to column final balance in 2024-5-2
                                    decimal balance = InventoryStoreItemList.Where(x => x.InventoryItemId == item.InventoryItemId && x.FinalBalance > 0 && x.InventoryStoreId == MatrialRequestOBJDB.ToInventoryStoreId).GroupBy(x => x.InventoryItemId).Select(x => x.Sum(a => a.FinalBalance ?? 0)).FirstOrDefault();
                                    //Common.GetInventoryStoreItemBalance(MatrialRequestOBJDB.ToInventoryStoreID, item.InventoryItemID);
                                    MatrialAddingOrderInfoObj.StockBalance = balance;
                                    MatrialRequestInfList.Add(MatrialAddingOrderInfoObj);

                                    decimal remain = (decimal)(item.ReqQuantity1??0 - item.RecivedQuantity1??0 - item.PurchaseQuantity1??0);
                                    if (MatrialRequestOBJDB.Status == "Open" && CheckISInventoryKeeper && remain > balance)
                                    {
                                        InventoryItemMatrialRequestInfoObj.CanCreatePR = true;
                                    }


                                    if (MaterialRequestStatus == "Hold" || MaterialRequestStatus == "Hold Released")
                                    {
                                        // Get List Hold 
                                        MatrialAddingOrderInfoObj.InventoryStoreItemIDsList = ListOfInventoryStoreItemsListDB.Where(x => x.HoldQty != null && x.InventoryItemId == item.InventoryItemId && x.HoldQty > 0 && x.InventoryStoreId == InventoryItemMatrialRequestInfoObj.ToStoreId).Select(x => x.Id).ToList();
                                    }
                                }


                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err109";
                            error.ErrorMSG = "Invalid Matrial Request Order ID";
                            Response.Errors.Add(error);
                            return Response;
                        }

                    }
                    if (InventoryItemMatrialRequestInfoObj.CanCreateRelease == true)
                    {
                        if (MatrialRequestInfList.Where(x => x.StockBalance > 0).Count() == 0)
                        {
                            InventoryItemMatrialRequestInfoObj.CanCreateRelease = false;
                        }
                    }
                    InventoryItemMatrialRequestInfoObj.MatrialRequestInfoList = MatrialRequestInfList;
                    Response.InventoryItemInfo = InventoryItemMatrialRequestInfoObj;


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
