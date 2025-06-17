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
using System.Net;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.Inventory;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Infrastructure.Entities;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory.Requests;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryInternalBackOrderService : IInventoryInternalBackOrderService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryItemService _inventoryItemService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;

        public InventoryInternalBackOrderService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork, IInventoryItemService inventoryItemService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            _inventoryItemService = inventoryItemService;
        }
        public string GetMatrialRelease(long ID)
        {
            string Name = "";
            if (ID != 0)
            {
                var supplierDB = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => x.Id == ID).FirstOrDefault();
                if (supplierDB != null)
                {
                    Name = supplierDB.ItemName;
                }
            }
            return Name;
        }
        public string GetInventoryStoreItemUOMName(long id)
        {
            string name = "";
            var LoadObjDB = _unitOfWork.VInventoryStoreItems.FindAll(x => x.InventoryItemId == id).FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.RequestionUomshortName;
            }
            return name;
        }
        public InventoryInternalBackOrderItemInfoResponse GetInventoryIntenralBackOrdertInfo([FromHeader] long MatrialInternalBackOrderID)
        {
            InventoryInternalBackOrderItemInfoResponse Response = new InventoryInternalBackOrderItemInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemMatrialRequestInfoObj = new InventoryItemInternalBackOrderInfo();
                var InternaalBackOrderInfList = new List<InternalBackOrderInfo>();
                if (Response.Result)
                {
                    /*long MatrialInternalBackOrderID = 0;
                    if (!string.IsNullOrEmpty(headers["MatrialInternalBackOrderID"]) && long.TryParse(headers["MatrialInternalBackOrderID"], out MatrialInternalBackOrderID))
                    {
                        long.TryParse(headers["MatrialInternalBackOrderID"], out MatrialInternalBackOrderID);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err113";
                        error.ErrorMSG = "Invalid Matrial Internal Back Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }*/

                    if (Response.Result)
                    {
                        var InventoryInternalBackOrderOBJDB = _unitOfWork.VInventoryInternalBackOrders.FindAll(x=>x.Id==MatrialInternalBackOrderID).FirstOrDefault();
                        if (InventoryInternalBackOrderOBJDB != null)
                        {
                            InventoryItemMatrialRequestInfoObj.InventoryMatrialInternalBackOrderID = MatrialInternalBackOrderID;
                            InventoryItemMatrialRequestInfoObj.FromUserName = InventoryInternalBackOrderOBJDB.FromUser;
                            InventoryItemMatrialRequestInfoObj.StoreName = InventoryInternalBackOrderOBJDB.StoreName;
                            InventoryItemMatrialRequestInfoObj.OperationType = InventoryInternalBackOrderOBJDB.OperationType;
                            InventoryItemMatrialRequestInfoObj.RecivingDate = InventoryInternalBackOrderOBJDB.RecivingDate.ToShortDateString();
                            InventoryItemMatrialRequestInfoObj.Custody = InventoryInternalBackOrderOBJDB.Custody;
                            InventoryItemMatrialRequestInfoObj.DepartmentName = InventoryInternalBackOrderOBJDB.FromUserDepartment;

                            var ListOfMInternalBackOrderItemListDB = _unitOfWork.InventoryInternalBackOrderItems.FindAll(x => x.InventoryInternalBackOrderId == MatrialInternalBackOrderID).ToList();
                            if (ListOfMInternalBackOrderItemListDB != null)
                            {
                                foreach (var item in ListOfMInternalBackOrderItemListDB)
                                {
                                    var ItemObjDB = _unitOfWork.InventoryItems.GetById(item.InventoryItemId);
                                    if (ItemObjDB != null)
                                    {

                                        var MatrialAddingOrderInfoObj = new InternalBackOrderInfo();
                                        MatrialAddingOrderInfoObj.InventoryItemID = item.InventoryItemId;
                                        MatrialAddingOrderInfoObj.ItemName = ItemObjDB.Name.Trim();
                                        MatrialAddingOrderInfoObj.MatrialReleaseName = item.InventoryMatrialReleaseItemId != null ? GetMatrialRelease((long)item.InventoryMatrialReleaseItemId) : "";
                                        MatrialAddingOrderInfoObj.ReqQTY = item.RecivedQuantity1.ToString();
                                        MatrialAddingOrderInfoObj.UOM = GetInventoryStoreItemUOMName(item.InventoryItemId);
                                        MatrialAddingOrderInfoObj.ItemCode = ItemObjDB.Code;
                                        MatrialAddingOrderInfoObj.ItemCode = ItemObjDB.Code;
                                        MatrialAddingOrderInfoObj.ProjectName = item.ProjectId != null ? Common.GetProjectName((long)item.ProjectId,_Context) : "";

                                        InternaalBackOrderInfList.Add(MatrialAddingOrderInfoObj);
                                    }
                                }
                            }
                        }

                        InventoryItemMatrialRequestInfoObj.InternalBackOrderItemInfoList = InternaalBackOrderInfList;

                    }
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

        public InventoryInternalBackOrderItemResponse GetInventoryInternalBackOrderItemList(GetInventoryInternalBackOrderFilters filters)
        {
            InventoryInternalBackOrderItemResponse Response = new InventoryInternalBackOrderItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var inventoryInternalBackOrderByDateList = new List<InventoryInternalBAckOrderByDate>();
                if (Response.Result)
                {
                    // Filter By Operation Type
                    /*string OperationType = null;
                    if (!string.IsNullOrEmpty(headers["OperationType"]))
                    {
                        OperationType = headers["OperationType"];
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err114";
                        error.ErrorMSG = "Invalid Operation Type";
                        Response.Errors.Add(error);
                        return Response;
                    }*/

                    // filters List InternalBackOrder
                    /*long InventoryItemID = 0;
                    if (!string.IsNullOrEmpty(headers["InventoryItemID"]) && long.TryParse(headers["InventoryItemID"], out InventoryItemID))
                    {
                        long.TryParse(headers["InventoryItemID"], out InventoryItemID);
                    }

                    long InventoryStoreID = 0;
                    if (!string.IsNullOrEmpty(headers["ToInventoryStoreID"]) && long.TryParse(headers["ToInventoryStoreID"], out InventoryStoreID))
                    {
                        long.TryParse(headers["ToInventoryStoreID"], out InventoryStoreID);
                    }*/


                    /*long FromUserID = 0;
                    if (!string.IsNullOrEmpty(headers["FromUserID"]) && long.TryParse(headers["FromUserID"], out FromUserID))
                    {
                        long.TryParse(headers["FromUserID"], out FromUserID);
                    }



                    long CreatorUserID = 0;
                    if (!string.IsNullOrEmpty(headers["CreatorUserID"]) && long.TryParse(headers["CreatorUserID"], out CreatorUserID))
                    {
                        long.TryParse(headers["CreatorUserID"], out CreatorUserID);
                    }


                    DateTime? ReceiveDate = null;
                    DateTime ReceiveDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(headers["ReceiveDate"]) && DateTime.TryParse(headers["ReceiveDate"], out ReceiveDateTemp))
                    {
                        ReceiveDateTemp = DateTime.Parse(headers["ReceiveDate"]);
                        ReceiveDate = ReceiveDateTemp;
                    }*/


                    // Grouped by DAte as Inquiry 
                    //var InventoryMatrialAddingOrder


                    var InventoryMatrialRequest = _unitOfWork.VInventoryInternalBackOrders.FindAllQueryable(a=>true).Select(x => new { x.Id, x.OperationType, x.InventoryStoreId, x.CreatedBy, x.FromId, x.RecivingDate, x.CreationDate, x.FromUser, x.Custody, x.StoreName }).Distinct().AsQueryable();

                    if (filters.OperationType != null)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.OperationType == filters.OperationType).AsQueryable();
                    }
                    if (filters.ToInventoryStoreID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.InventoryStoreId == filters.ToInventoryStoreID).AsQueryable();
                    }

                    if (filters.CreatorUserID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.FromUserID != 0)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.FromId == filters.FromUserID).AsQueryable();
                    }

                    if (filters.ReceiveDate != null)
                    {
                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => x.RecivingDate == filters.ReceiveDate).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        var IDInventoryInternalBackOrder = _unitOfWork.InventoryInternalBackOrderItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryInternalBackOrderId).Distinct().ToList();

                        InventoryMatrialRequest = InventoryMatrialRequest.Where(x => IDInventoryInternalBackOrder.Contains(x.Id)).AsQueryable();
                    }

                    var InventoryMatrialRequestFiltered = InventoryMatrialRequest.ToList().OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month });

                    foreach (var MatrialPerMonth in InventoryMatrialRequestFiltered)
                    {
                        var InventoryMatrialAddingOrderInfoList = new List<InventoryInternalBackOrderInfo>();

                        var MatrialPerMonthList = MatrialPerMonth.ToList();

                        foreach (var Data in MatrialPerMonthList)
                        {
                            InventoryMatrialAddingOrderInfoList.Add(new InventoryInternalBackOrderInfo
                            {
                                InventoryInternalBackOrderNo = Data.Id.ToString(),
                                FromUserName = Data.FromUser,
                                Custody = Data.Custody,
                                StoreName = Data.StoreName,
                                OperationType = Data.OperationType,
                                CreatorName = Common.GetUserName(Data.CreatedBy,_Context),
                                RecivingDate = Data.RecivingDate.ToShortDateString()
                            });
                        }
                        inventoryInternalBackOrderByDateList.Add(new InventoryInternalBAckOrderByDate()
                        {
                            DateMonth = Common.GetMonthName(MatrialPerMonth.Key.month) + " " + MatrialPerMonth.Key.year.ToString(),
                            //item.Select(x=>x.CreatedDate.Month).ToString(),
                            InventoryInternalBackOrderInfoList = InventoryMatrialAddingOrderInfoList,
                        });
                    }

                    Response.InventoryInternalBackOrderByDateList = inventoryInternalBackOrderByDateList;

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

        public BaseResponseWithId<long> AddInventoryInternalBackOrder(AddInventoryInternalBackOrderRequest Request,long creator)
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
                    //long RequestTypeID = 0;
                    if (string.IsNullOrEmpty(Request.OperationType))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err55";
                        error.ErrorMSG = "Invalid Operation Type.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Request.OperationType != "Internal Back Order" && Request.OperationType != "Final Product" && Request.OperationType != "Semi-Final Product"
                        && Request.OperationType != "Client Returns")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err55";
                        error.ErrorMSG = "Invalid Operation Type must be Internal Back Order,Client Returns , Final Product or Semi-Final Product.";
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
                    long FromUserID = 0;
                    if (Request.FromUserID != 0)
                    {
                        FromUserID = Request.FromUserID;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err29";
                        error.ErrorMSG = "Invalid From User ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    DateTime RecivingDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RecivingDate) || !DateTime.TryParse(Request.RecivingDate, out RecivingDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Request Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalBackOrderItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please insert at least one Internal back Order Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.InternalBackOrderItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please insert at least one Matrial Request Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    bool Cutoday = false;
                    if (Request.Cutoday != null)
                    {
                        Cutoday = (bool)Request.Cutoday;
                    }

                    List<InventoryMatrialReleaseItem> MatrialReleasItemsList = new List<InventoryMatrialReleaseItem>();
                    long InventoryMatrialReleaseID = 0;
                    var IDSMatrialReleaseItems = Request.InternalBackOrderItemList.Select(x => x.MatrialReleaseID).ToList();

                    if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns")
                    {
                        var MatrialReleaseItemsFirst = _unitOfWork.InventoryMatrialReleaseItems.FindAll(x => IDSMatrialReleaseItems.Contains(x.Id)).FirstOrDefault();
                        InventoryMatrialReleaseID = MatrialReleaseItemsFirst?.InventoryMatrialReleasetId ?? 0;
                        MatrialReleasItemsList = _unitOfWork.InventoryMatrialReleaseItems.FindAll(x => x.InventoryMatrialReleasetId == InventoryMatrialReleaseID).ToList();
                    }
                    int Counter = 0;
                    foreach (var item in Request.InternalBackOrderItemList)
                    {
                        // Matrial Release QTY
                        decimal MatrialReleaseQTY = 0;
                        Counter++;
                        if (_unitOfWork.InventoryStoreLocations.GetById(item.StoreLocationID??0)==null)
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


                        if (item.PojectID < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err48";
                            error.ErrorMSG = "Invalid Project Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }


                        if (item.ReqQTY < 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err49";
                            error.ErrorMSG = "Invalid Req QTY selected for item #" + Counter;
                            Response.Errors.Add(error);
                        }
                        if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns") // Check QTY Return is found
                        {
                            // Check Matrial Release and check Qty Release < ReqQTY
                            if (item.MatrialReleaseID < 0) // Matrial Release Item ID
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err48";
                                error.ErrorMSG = "Invalid Matrial Release Selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                item.InventoryStorItemMRItemID = _unitOfWork.InventoryStoreItems.FindAll(x => x.AddingOrderItemId == item.MatrialReleaseID).Select(x => x.Id).FirstOrDefault();
                                //var MatrialReleaseItemObjDB = _Context.proc_InventoryMatrialReleaseItemsLoadByPrimaryKey(item.MatrialReleaseID).FirstOrDefault();
                                var MatrialReleaseItemObjDB = MatrialReleasItemsList.Where(x => x.Id == item.MatrialReleaseID).FirstOrDefault();
                                if (MatrialReleaseItemObjDB == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err48";
                                    error.ErrorMSG = "Invalid Matrial Release Selected item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {

                                    MatrialReleaseQTY = MatrialReleaseItemObjDB.RecivedQuantity1 != null ? (decimal)MatrialReleaseItemObjDB.RecivedQuantity1 : 0;

                                    // check Qty in the same matrial release item in this internal back order item
                                    var RemainQTYMatrialRelease = _unitOfWork.InventoryInternalBackOrderItems.FindAll(x => x.InventoryMatrialReleaseItemId == item.MatrialReleaseID).Select(x => x.RemainReleaseQty).LastOrDefault();
                                    if (RemainQTYMatrialRelease != null)
                                    //&& RemainQTYMatrialRelease != 0
                                    {
                                        MatrialReleaseQTY = (decimal)RemainQTYMatrialRelease;
                                    }
                                    var TotalQTYRequestForThisItem = Request.InternalBackOrderItemList.Where(x => x.InventoryItemID == item.InventoryItemID &&
                                    x.MatrialReleaseID == item.MatrialReleaseID).Sum(x => x.ReqQTY);

                                    if (TotalQTYRequestForThisItem > MatrialReleaseQTY)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err48";
                                        error.ErrorMSG = "The matrial rlease not have any more quantity on item #" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }
                    }



                    long InventoryInternalBackOrderItemID = 0;
                    if (Response.Result)
                    {
                        // Check Inventory Report Approved and closed or not
                        var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false, includes: new[] { "InventoryStore" }).ToList();
                        if (CheckInventoryReportListDB.Count > 0)
                        {
                            foreach (var InventoryRep in CheckInventoryReportListDB)
                            {
                                if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                {
                                    string storeName = InventoryRep.InventoryStore.Name;
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
                            // Insertion Matrial InternalbackOrder
                            var backOrder = new InventoryInternalBackOrder() 
                            {
                                OperationType = Request.OperationType,
                                CreationDate = DateTime.Now,
                                CreatedBy = creator,
                                Revision = 0,
                                FromId = FromUserID,
                                InventoryStoreId = InventoryStoreID,
                                RecivingDate = RecivingDate,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = creator,
                                Custody = Cutoday
                            };

                            _unitOfWork.InventoryInternalBackOrders.Add(backOrder);
                            var InventoryInternalBackOrderInsertion = _unitOfWork.Complete();

                            if (InventoryInternalBackOrderInsertion > 0)
                            {
                                long InventoryInternalBackOrderID = backOrder.Id;
                                #region items


                                foreach (var InternalBackDataOBJ in Request.InternalBackOrderItemList)
                                {
                                    var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(InternalBackDataOBJ.InventoryItemID);
                                    if (InventoryItemObjDB != null)
                                    {

                                        decimal? RemainQTy = null;
                                        long? MatrialReleaseID = null;

                                        if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns") // Check QTY Return is found
                                        {
                                            var MatrialReleaseItemQTY = MatrialReleasItemsList.Where(x => x.Id == InternalBackDataOBJ.MatrialReleaseID).Select(x => x.RecivedQuantity1).FirstOrDefault();
                                            //var MatrialReleaseItemQTY = _Context.proc_InventoryMatrialReleaseItemsLoadByPrimaryKey(InternalBackDataOBJ.MatrialReleaseID).Select(x => x.RecivedQuantity).FirstOrDefault();
                                            // check Qty in the same matrial release item in this internal back order item
                                            var RemainQTYMatrialRelease = _unitOfWork.InventoryInternalBackOrderItems.FindAll(x => x.InventoryMatrialReleaseItemId == InternalBackDataOBJ.MatrialReleaseID).Select(x => x.RemainReleaseQty).LastOrDefault();
                                            if (RemainQTYMatrialRelease != null && RemainQTYMatrialRelease != 0)
                                            {
                                                MatrialReleaseItemQTY = RemainQTYMatrialRelease;
                                            }
                                            if (MatrialReleaseItemQTY < InternalBackDataOBJ.ReqQTY)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err48";
                                                error.ErrorMSG = "Invalid Req QTY Selected item #" + InternalBackDataOBJ.InventoryItemID + " must be less than " + MatrialReleaseItemQTY;
                                                Response.Errors.Add(error);
                                                return Response;
                                            }
                                            RemainQTy = (decimal)(MatrialReleaseItemQTY??0) - (InternalBackDataOBJ.ReqQTY);
                                            MatrialReleaseID = InternalBackDataOBJ.MatrialReleaseID;
                                        }

                                        //add new
                                        var backOrderItem = new InventoryInternalBackOrderItem()
                                        {
                                            InventoryInternalBackOrderId = InventoryInternalBackOrderID,
                                            InventoryItemId = InternalBackDataOBJ.InventoryItemID,
                                            Uomid = InventoryItemObjDB.RequstionUomid,
                                            Comments = InternalBackDataOBJ.Comment,
                                            ProjectId = InternalBackDataOBJ.PojectID,
                                            RecivedQuantity1 = InternalBackDataOBJ.ReqQTY,
                                            InventoryMatrialReleaseItemId = MatrialReleaseID,
                                            RemainReleaseQty = RemainQTy
                                        };
                                        _unitOfWork.InventoryInternalBackOrderItems.Add(backOrderItem);

                                        var InternalBackOrderItemInsertion = _unitOfWork.Complete();


                                        if (InternalBackOrderItemInsertion > 0)
                                        {

                                            InventoryInternalBackOrderItemID = backOrderItem.Id;

                                            decimal totalAmount = 0;
                                            decimal amountOfItem = 0;
                                            int productGroupID = 0;
                                            // InventoryItem inventoryItem = new InventoryItem();
                                            //  inventoryItem.Where.ID.Value = items.InventoryItemID;
                                            //GeneralActiveCostCenters generalCostCentersOld = new GeneralActiveCostCenters();
                                            //generalCostCentersOld.Where.CategoryID.Value = items.ProjectID;
                                            //if (generalCostCentersOld.Query.Load())
                                            //{
                                            //    if (generalCostCentersOld.DefaultView != null && generalCostCentersOld.DefaultView.Count > 0)
                                            //    {
                                            //        costCenterID = generalCostCentersOld.ID;
                                            //    }
                                            //}
                                            var inventoryItemLoadPrimary = _unitOfWork.InventoryItems.GetById                 (InternalBackDataOBJ.InventoryItemID);
                                            if (inventoryItemLoadPrimary != null)
                                            {
                                                productGroupID = inventoryItemLoadPrimary.InventoryItemCategoryId;
                                                var Quantity = decimal.Parse(InternalBackDataOBJ.ReqQTY.ToString());
                                                decimal UnitPrice = 0;
                                                if (inventoryItemLoadPrimary.CalculationType == 1)
                                                {
                                                    UnitPrice = inventoryItemLoadPrimary.AverageUnitPrice;
                                                }
                                                else if (inventoryItemLoadPrimary.CalculationType == 2)
                                                {
                                                    UnitPrice = inventoryItemLoadPrimary.MaxUnitPrice;
                                                }
                                                else if (inventoryItemLoadPrimary.CalculationType == 3)
                                                {
                                                    UnitPrice = inventoryItemLoadPrimary.LastUnitPrice;
                                                }
                                                else if (inventoryItemLoadPrimary.CalculationType == 4)
                                                {
                                                    UnitPrice = inventoryItemLoadPrimary.CustomeUnitPrice;
                                                }
                                                //UnitPrice = inventoryItem.CustomeUnitPrice;
                                                amountOfItem = Quantity * UnitPrice;
                                            }
                                            totalAmount = amountOfItem;

                                            #region Save Cost Center to DB


                                            if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns")
                                            {
                                                string Type = "Client Returns";
                                                if (Request.OperationType == "Internal Back Order")
                                                {
                                                    Type = "Matrial Back Order";
                                                }

                                                long costCenterID = 0;
                                                decimal GeneralCumlativeCost = 0;


                                                var GeneralCostCentersOldObjDB = _unitOfWork.GeneralActiveCostCenters.FindAll(x => x.CategoryId == InternalBackDataOBJ.PojectID).FirstOrDefault();
                                                if (GeneralCostCentersOldObjDB != null)
                                                {
                                                    costCenterID = GeneralCostCentersOldObjDB.Id;
                                                    if (GeneralCostCentersOldObjDB.CumulativeCost != null)
                                                    {
                                                        GeneralCumlativeCost = (decimal)GeneralCostCentersOldObjDB.CumulativeCost;
                                                    }
                                                }
                                                var center = new DailyAdjustingEntryCostCenter()
                                                {
                                                    DailyAdjustingEntryId = 0,
                                                    CostCenterId = costCenterID,
                                                    Type = Type,
                                                    TypeId = InventoryInternalBackOrderID,
                                                    ProductId = InventoryInternalBackOrderItemID,
                                                    ProductGroupId = productGroupID,
                                                    Description = "",
                                                    Active = true,
                                                    CreatedBy = creator,
                                                    CreationDate = DateTime.Now,
                                                    ModifiedBy = creator,
                                                    Modified = DateTime.Now,
                                                    Amount = totalAmount,
                                                    EntryType = "Automatic",
                                                    Quantity1 = InternalBackDataOBJ.ReqQTY
                                                };
                                                _unitOfWork.DailyAdjustingEntryCostCenters.Add(center);


                                                var DailyAdjustingEntryCostCenterInsertion = _unitOfWork.Complete();
                                                var GeneralCostCenter = _unitOfWork.GeneralActiveCostCenters.GetById(costCenterID);
                                                if (GeneralCostCenter != null)
                                                {
                                                    GeneralCostCenter.CumulativeCost = GeneralCumlativeCost - totalAmount;
                                                    GeneralCostCenter.ModifiedBy = creator;
                                                    GeneralCostCenter.Modified = DateTime.Now;
                                                    _unitOfWork.GeneralActiveCostCenters.Update(GeneralCostCenter);
                                                    
                                                }
                                                var GeneralActiveCostCenterUpdation = _unitOfWork.Complete();
                                            }
                                            //DailyAdjustingEntryCostCenter CostCenterDB = new DailyAdjustingEntryCostCenter();
                                            //CostCenterDB.AddNew();
                                            //CostCenterDB.DailyAdjustingEntryID = 0;
                                            //CostCenterDB.CostCenterID = costCenterID;
                                            //CostCenterDB.TypeID = order.ID;
                                            //CostCenterDB.Type = "Matrial Back Order";
                                            //CostCenterDB.ProductID = items.ID;
                                            //CostCenterDB.ProductGroupID = productGroupID;
                                            //CostCenterDB.Quantity = items.RecivedQuantity;
                                            //CostCenterDB.Description = "";
                                            //CostCenterDB.Amount = totalAmount;
                                            //CostCenterDB.EntryType = "Automatic";
                                            //CostCenterDB.Active = true;
                                            //CostCenterDB.CreatedBy = UserID;
                                            //CostCenterDB.CreationDate = DateTime.Now;
                                            //CostCenterDB.ModifiedBy = UserID;
                                            //CostCenterDB.Modified = DateTime.Now;
                                            //CostCenterDB.Save();




                                            //GeneralActiveCostCenters generalCostCenters = new GeneralActiveCostCenters();
                                            //generalCostCenters.Where.ID.Value = costCenterID;
                                            //if (generalCostCenters.Query.Load())
                                            //{
                                            //    if (generalCostCenters.DefaultView != null && generalCostCenters.DefaultView.Count > 0)
                                            //    {
                                            //        generalCostCenters.CumulativeCost = generalCostCenters.CumulativeCost + totalAmount;
                                            //        generalCostCenters.Save();
                                            //    }
                                            //}

                                            #endregion


                                            DateTime? ExpDate = null;
                                            if (InternalBackDataOBJ.ExpDate != null && InternalBackDataOBJ.ExpDate != "")
                                            {
                                                ExpDate = DateTime.Parse(InternalBackDataOBJ.ExpDate);
                                            }
                                            long? ParentInventoryStoreItemMRItemID = null;
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
                                            if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns")
                                            {
                                                ParentInventoryStoreItemMRItemID = InternalBackDataOBJ.InventoryStorItemMRItemID;
                                                if (ParentInventoryStoreItemMRItemID != null)
                                                {
                                                    var ParentInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.Id == ParentInventoryStoreItemMRItemID).FirstOrDefault();
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
                                                        decimal? finalBalance = InternalBackDataOBJ.ReqQTY;
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
                                                            }
                                                        }
                                                    }
                                                }
                                            }





                                            var InventoryStoreItemOBJ = new InventoryStoreItem();
                                            InventoryStoreItemOBJ.InventoryStoreId = InventoryStoreID;
                                            InventoryStoreItemOBJ.InventoryItemId = InternalBackDataOBJ.InventoryItemID;
                                            InventoryStoreItemOBJ.OrderNumber = InventoryInternalBackOrderID.ToString();
                                            InventoryStoreItemOBJ.OrderId = InventoryInternalBackOrderID;
                                            InventoryStoreItemOBJ.CreatedBy = creator;
                                            InventoryStoreItemOBJ.ModifiedBy = creator;
                                            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                            InventoryStoreItemOBJ.OperationType = Request.OperationType;
                                            InventoryStoreItemOBJ.Balance = (double)InternalBackDataOBJ.ReqQTY;
                                            InventoryStoreItemOBJ.InvenoryStoreLocationId = InternalBackDataOBJ.StoreLocationID;
                                            InventoryStoreItemOBJ.ExpDate = ExpDate;
                                            InventoryStoreItemOBJ.ItemSerial = InternalBackDataOBJ.Serial;
                                            InventoryStoreItemOBJ.ReleaseParentId = ParentInventoryStoreItemMRItemID;
                                            InventoryStoreItemOBJ.FinalBalance = InternalBackDataOBJ.ReqQTY;
                                            InventoryStoreItemOBJ.AddingOrderItemId = InventoryInternalBackOrderItemID;
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

                                            //ObjectParameter IDInventoryStoreItemInsertion = new ObjectParameter("ID", typeof(long));
                                            //var InventoryStorItemInsertion = _Context.Myproc_InventoryStoreItemInsert_API(IDInventoryStoreItemInsertion,
                                            //                                                                        InventoryStoreID,
                                            //                                                                        InternalBackDataOBJ.InventoryItemID,
                                            //                                                                        InventoryInternalBackOrderID.ToString(),
                                            //                                                                        InventoryInternalBackOrderID,
                                            //                                                                        // (double)MatrialDataOBJ.AddedQTYUOR,
                                            //                                                                        DateTime.Now,
                                            //                                                                        validation.userID,
                                            //                                                                        DateTime.Now,
                                            //                                                                        validation.userID,
                                            //                                                                        Request.OperationType,
                                            //                                                                        InternalBackDataOBJ.ReqQTY,
                                            //                                                                        InternalBackDataOBJ.StoreLocationID,
                                            //                                                                        ExpDate,
                                            //                                                                        InternalBackDataOBJ.Serial,
                                            //                                                                        ParentInventoryStoreItemMRItemID, // Parent
                                            //                                                                        InternalBackDataOBJ.ReqQTY,
                                            //                                                                        InventoryInternalBackOrderItemID,
                                            //                                                                        // Extra Data PO Item
                                            //                                                                        POID,
                                            //                                                                       POInvoiceId,
                                            //                                                                        POInvoiceTotalPrice,
                                            //                                                                        POInvoiceTotalCost,
                                            //                                                                        currencyId,
                                            //                                                                        rateToEGP,
                                            //                                                                        POInvoiceTotalPriceEGP,
                                            //                                                                        POInvoiceTotalCostEGP,
                                            //                                                                        remainItemPrice,
                                            //                                                                        remainItemCosetEGP,
                                            //                                                                        remainItemCostOtherCU,
                                            //                                                                        0,
                                            //                                                                        null
                                            //                                                                        );



                                            if (InventoryStorItemInsertion > 0)
                                            {
                                                if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns" && ParentInventoryStoreItemMRItemID != null)
                                                {
                                                    var ParentInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.Id == ParentInventoryStoreItemMRItemID).FirstOrDefault();

                                                    // Update Parent Release on InventoryStoreItem
                                                    if (ParentInventoryStoreItem != null)
                                                    {
                                                        ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                                                        ParentInventoryStoreItem.ModifiedBy = creator;


                                                        // Update PO Item Columns
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
                                                    }
                                                }
                                            }
                                            //  GarasERP.InventoryStoreItem storeItem = new GarasERP.InventoryStoreItem();
                                            // storeItem.AddNew();
                                            //storeItem.Balance =(decimal) (items.RecivedQuantity / decimal.Parse(row["Factor"].ToString()));
                                            //  storeItem.Balance = (decimal)items.RecivedQuantity;
                                            //  storeItem.CreatedBy = UserID;
                                            //  storeItem.CreationDate = DateTime.Now;
                                            //  storeItem.InventoryItemID = items.InventoryItemID;
                                            // storeItem.InventoryStoreID = order.InventoryStoreID;
                                            // storeItem.ModifiedBy = UserID;
                                            //storeItem.ModifiedDate = DateTime.Now;
                                            //storeItem.OperationType = order.OperationType;
                                            // storeItem.OrderID = order.ID;
                                            // storeItem.OrderNumber = order.s_ID;
                                            //  storeItem.Save();

                                            // -------------------------------------------------Update and Calc Average for Inventory Item ----------------------------------------------------------------

                                            var ListInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemObjDB.Id && x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
                                            var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
                                            var ListIDSPOInvoicesIsFulllyPriced = _unitOfWork.PurchasePOInvoices.FindAll(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
                                            ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
                                            InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;

                                            _unitOfWork.Complete();


                                            // Uptate Release QTY in  Sales Offer Product 
                                            if (Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns")
                                            {

                                                var SalesOfferID = _unitOfWork.Projects.GetById(InternalBackDataOBJ.PojectID)?.SalesOfferId;
                                                if (SalesOfferID != 0)
                                                {
                                                    //var ListSalesOfferProductIDS = _Context.SalesOfferProducts.Where(x => x.OfferID == SalesOfferID && x.InventoryItemID == InternalBackDataOBJ.InventoryItemID).OrderBy(x=>x.ReleasedQty)
                                                    var SalesOfferProdDB = _unitOfWork.SalesOfferProducts.FindAll(x => x.OfferId == SalesOfferID && x.InventoryItemId == InternalBackDataOBJ.InventoryItemID /*&& x.Quantity >= (x.ReleasedQty ?? 0)*/)
                                                        .OrderBy(x => x.ReleasedQty).FirstOrDefault();
                                                    if (SalesOfferProdDB != null)
                                                    {
                                                        SalesOfferProdDB.ReleasedQty += (float)InternalBackDataOBJ.ReqQTY;
                                                    }

                                                    _unitOfWork.Complete();
                                                }
                                            }
                                        }



                                    }

                                }

                                // Check if all Release Returend Of Not (Compare ITems with Request Items Internal Back Order)
                                if ((Request.OperationType == "Internal Back Order" || Request.OperationType == "Client Returns")
                                    //&& headers["CompanyName"].ToString().ToLower() == "ortho"
                                    )
                                {
                                    bool Complete = true;
                                    foreach (var item in MatrialReleasItemsList)
                                    {
                                        var ItemQuantityForReturn = Request.InternalBackOrderItemList?.Where(x => x.MatrialReleaseID == item.Id).Select(x => x.ReqQTY).Sum() ?? 0;
                                        if ((decimal)item.RecivedQuantity1 != ItemQuantityForReturn)
                                        {
                                            Complete = false;
                                            break;
                                        }
                                    }
                                    if (Complete) // Update Status Returned
                                    {
                                        var MatrialReleaseDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.Id == InventoryMatrialReleaseID).FirstOrDefault();
                                        MatrialReleaseDB.Status = "Returned";
                                        _unitOfWork.Complete();
                                    }
                                }
                                #endregion items
                                Response.Result = true;
                                Response.ID = InventoryInternalBackOrderID;
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
