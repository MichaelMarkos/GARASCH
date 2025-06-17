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
using NewGaras.Infrastructure.Helper;
using NewGarasAPI.Models.Inventory.Requests;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Infrastructure.Entities;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGarasAPI.Models.Account;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NewGarasAPI.Models.AccountAndFinance;
using System.Web;
using Azure;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryMaterialReleaseService : IInventoryMateriaReleaseService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IInventoryItemService _inventoryItemService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";
        private HearderVaidatorOutput validation;

        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public InventoryMaterialReleaseService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork, IInventoryItemService inventoryItemService)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
            _inventoryItemService = inventoryItemService;
        }

        public InventoryItemMatrialReleaseResponse GetInventoryItemMatrialReleaseList(GetInventoryItemMatrialReleaseListFilters filters)
        {
            InventoryItemMatrialReleaseResponse Response = new InventoryItemMatrialReleaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var inventoryMtrialReleaseByDateList = new List<InventoryMtrialReleaseByDate>();
                if (Response.Result)
                {
                    var InventoryMatrialRelease = _unitOfWork.VInventoryMatrialReleases.FindAllQueryable(a=>true);

                    if (filters.InventoryStoreID != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.FromInventoryStoreId == filters.InventoryStoreID).AsQueryable();
                    }

                    if (filters.CreatorUserID != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.ToUserID != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.ToUserId == filters.ToUserID).AsQueryable();
                    }

                    if (filters.RequestDate != null)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.RequestDate == filters.RequestDate).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        var IDInventoryMatrialRelease = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryMatrialReleasetId).Distinct().ToList();

                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => IDInventoryMatrialRelease.Contains(x.Id)).AsQueryable();
                    }

                    var InventoryMatrialReleaseFiltered = InventoryMatrialRelease.ToList().OrderByDescending(x => x.CreationDate).GroupBy(a=>a.Id).Select(a=>a.FirstOrDefault()).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();
                    var creators = _unitOfWork.Users.FindAll(a=> InventoryMatrialRelease.Select(c=>c.CreatedBy).Contains(a.Id)).ToList();
                    foreach (var MatrialPerMonth in InventoryMatrialReleaseFiltered)
                    {
                        var InventoryMatrialReleaseInfoList = new List<InventoryMatrialReleaseInfo>();

                        foreach (var Data in MatrialPerMonth)
                        {

                            InventoryMatrialReleaseInfoList.Add(new InventoryMatrialReleaseInfo
                            {
                                InventoryMatrialReleasetNo = Data.Id.ToString(),
                                FromUserName = Data.FromUserLname,
                                ToUserName = Data.ToUserName,
                                StoreName = Data.FromInventoryStoreName,
                                CreatorName = creators.Where(a=>a.Id==Data.CreatedBy).Select(a=>a.FirstName+" "+a.LastName).FirstOrDefault(),
                                Status = Data.Status,
                            });;
                        }
                        inventoryMtrialReleaseByDateList.Add(new InventoryMtrialReleaseByDate()
                        {
                            DateMonth = Common.GetMonthName(MatrialPerMonth.Key.month) + " " + MatrialPerMonth.Key.year.ToString(),
                            //item.Select(x=>x.CreatedDate.Month).ToString(),
                            InventoryMatrialReleaseInfoList = InventoryMatrialReleaseInfoList,
                        });
                    }

                    Response.InventoryMtrialReleaseByDateList = inventoryMtrialReleaseByDateList;

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


        public InventoryItemMatrialReleasePagingResponse GetInventoryItemMatrialReleaseListPaging(GetInventoryItemMatrialReleaseListPagingFilters filters)
        {
            InventoryItemMatrialReleasePagingResponse Response = new InventoryItemMatrialReleasePagingResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //var inventoryMtrialReleaseByDateList = new List<InventoryMtrialReleaseByDate>();
                if (Response.Result)
                {               
                    var InventoryMatrialRelease = _unitOfWork.VInventoryMatrialReleases.FindAllQueryable(a=>true);
                    var InventoryMatrialReleaseItems = _unitOfWork.VInventoryMatrialReleaseItems.FindAllQueryable(a=>true);
                    if (filters.ReleaseNo != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.Id == filters.ReleaseNo).AsQueryable();
                    }

                    if (!string.IsNullOrWhiteSpace(filters.Status))
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.Status.ToLower() == filters.Status.ToLower()).AsQueryable();
                    }

                    if (filters.ToInventoryStoreID != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.FromInventoryStoreId == filters.ToInventoryStoreID).AsQueryable();
                    }

                    if (filters.CreatorUserID != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.CreatedBy == filters.CreatorUserID).AsQueryable();
                    }

                    if (filters.ToUserID != 0)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.ToUserId == filters.ToUserID).AsQueryable();
                    }

                    if (filters.RequestDate != null)
                    {
                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => x.RequestDate == filters.RequestDate).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        var IDInventoryMatrialRelease = InventoryMatrialReleaseItems.Where(x => x.InventoryItemId == filters.InventoryItemID).Select(x => x.InventoryMatrialReleasetId).Distinct().ToList();

                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => IDInventoryMatrialRelease.Contains(x.Id)).AsQueryable();
                    }

                    if (filters.ProjectID != 0)
                    {
                        var IDInventoryMatrialRelease = InventoryMatrialReleaseItems.Where(x => x.ProjectId == filters.ProjectID).Select(x => x.InventoryMatrialReleasetId).Distinct().ToList();

                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => IDInventoryMatrialRelease.Contains(x.Id)).AsQueryable();
                    }


                    if (filters.ClientID != 0)
                    {
                        var IDInventoryMatrialRelease = InventoryMatrialReleaseItems.Where(x => x.ClientId == filters.ClientID).Select(x => x.InventoryMatrialReleasetId).Distinct().ToList();

                        InventoryMatrialRelease = InventoryMatrialRelease.Where(x => IDInventoryMatrialRelease.Contains(x.Id)).AsQueryable();
                    }
                    var DataDistinctList = InventoryMatrialRelease.Select(x => new InventoryMatrialReleaseInfoForPaging
                    {
                        ID = x.Id,
                        InventoryMatrialReleasetNo = x.Id.ToString(),
                        ToUserName = x.ToUserName,
                        StoreName = x.FromInventoryStoreName,
                        Status = x.Status,
                        RequestType = x.RequestTypeName,
                        CreatedBy = x.CreatedBy,
                        CreationDate = x.CreationDate,
                        RequestDate = x.RequestDate,
                        ProjectName = InventoryMatrialReleaseItems.Where(i => i.InventoryMatrialReleasetId == x.Id).Select(i => i.ProjectId).Distinct().Count() > 1 ? "multiple" :
                                      ((InventoryMatrialReleaseItems.Where(i => i.InventoryMatrialReleasetId == x.Id).Select(i => i.ProjectName).FirstOrDefault() ?? "")
                                      +
                                      " - "
                                      +
                                      (InventoryMatrialReleaseItems.Where(i => i.InventoryMatrialReleasetId == x.Id).Select(i => i.ClientName).FirstOrDefault() ?? "")
                                      ),
                        //ProjectNameList = InventoryMatrialReleaseItems.Where(i => i.InventoryMatrialReleasetID == x.ID).Select(i => i.ProjectName +" - "+ i.SupplierName).Distinct().ToList()
                    }).Distinct().AsQueryable();

                    var PagingList = PagedList<InventoryMatrialReleaseInfoForPaging>.Create(DataDistinctList.OrderByDescending(x => x.CreationDate), filters.CurrentPage, filters.NumberOfItemsPerPage);

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = PagingList.TotalPages,
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = PagingList.TotalCount
                    };
                    var creators = _unitOfWork.Users.FindAll(a=> PagingList.Select(c=>c.CreatedBy).Contains(a.Id)).ToList();
                    foreach (var item in PagingList)
                    {
                        item.CreatorName = creators.Where(a=>a.Id==item.CreatedBy).Select(a=>a.FirstName+" "+a.LastName).FirstOrDefault();
                        item.ProjectNameList = InventoryMatrialReleaseItems.Where(i => i.InventoryMatrialReleasetId == item.ID).Select(i => i.ProjectName + " - " + i.ClientName).Distinct().ToList();
                    }
                    Response.MatrialReleaseList = [.. PagingList];

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


        public InventoryItemMatrialReleaseInfoResponse GetInventoryItemMatrialReleasetInfo([FromHeader] long MatrialReleaseOrderID)
        {
            InventoryItemMatrialReleaseInfoResponse Response = new InventoryItemMatrialReleaseInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemMatrialReleaseInfoObj = new InventoryItemMatrialReleaseInfo();
                var MatrialReleaseInfList = new List<MatrialReleaseItemInfo>();
                if (Response.Result)
                {
                    if (MatrialReleaseOrderID==0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err169";
                        error.ErrorMSG = "Invalid Matrial Release Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {
                        var InventoryMatrialReleaseOBJDB = _unitOfWork.VInventoryMatrialReleases.FindAll(x => x.Id == MatrialReleaseOrderID).FirstOrDefault();
                        if (InventoryMatrialReleaseOBJDB != null)
                        {
                            InventoryItemMatrialReleaseInfoObj.ID = InventoryMatrialReleaseOBJDB.Id;
                            InventoryItemMatrialReleaseInfoObj.InventoryMatrialRequestOrderID = InventoryMatrialReleaseOBJDB.MatrialRequestId;
                            InventoryItemMatrialReleaseInfoObj.FromUserName = InventoryMatrialReleaseOBJDB.FromUserFname + " " + InventoryMatrialReleaseOBJDB.FromUserLname;
                            InventoryItemMatrialReleaseInfoObj.ToUserName = InventoryMatrialReleaseOBJDB.ToUserName;
                            InventoryItemMatrialReleaseInfoObj.UserDept = InventoryMatrialReleaseOBJDB.FromUserDepartment;
                            InventoryItemMatrialReleaseInfoObj.RequestType = InventoryMatrialReleaseOBJDB.RequestTypeName;
                            InventoryItemMatrialReleaseInfoObj.RequestTypeId = InventoryMatrialReleaseOBJDB.RequestTypeId;
                            InventoryItemMatrialReleaseInfoObj.FromStoreId = InventoryMatrialReleaseOBJDB.FromInventoryStoreId;
                            InventoryItemMatrialReleaseInfoObj.FromStore = InventoryMatrialReleaseOBJDB.FromInventoryStoreName;
                            InventoryItemMatrialReleaseInfoObj.CreationDate = InventoryMatrialReleaseOBJDB.CreationDate.ToShortDateString();

                            var ListOfMatrialReleaseItemListDB = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => x.InventoryMatrialReleasetId == MatrialReleaseOrderID).ToList();
                            if (ListOfMatrialReleaseItemListDB != null)
                            {
                                ////Created By: Mark Shawky 2022-11-9
                                ////Start
                                //var ListOfMatrialReleaseItemListSingleItem = ListOfMatrialReleaseItemListDB.FirstOrDefault();
                                //InventoryItemMatrialReleaseInfoObj.TotalRecieved = ListOfMatrialReleaseItemListSingleItem.RecivedQuantity;
                                //if (ListOfMatrialReleaseItemListSingleItem.ReqQuantity != null)
                                //{
                                //    decimal? TotalRemainQty = ListOfMatrialReleaseItemListSingleItem.ReqQuantity - ListOfMatrialReleaseItemListSingleItem.RecivedQuantity ?? 0;
                                //    InventoryItemMatrialReleaseInfoObj.TotalRemainQty = TotalRemainQty;
                                //}
                                //End
                                foreach (var item in ListOfMatrialReleaseItemListDB)
                                {
                                    string ClientName = !string.IsNullOrEmpty(item.ClientName) ? " - " + item.ClientName : "";

                                    var MatrialReleaseInfoObj = new MatrialReleaseItemInfo();
                                    MatrialReleaseInfoObj.ID = item.Id;
                                    MatrialReleaseInfoObj.InventoryItemID = item.InventoryItemId;
                                    MatrialReleaseInfoObj.ItemName = item.ItemName != null ? item.ItemName.Trim() : "";
                                    MatrialReleaseInfoObj.ReqQTY = item.ReqQuantity;
                                    MatrialReleaseInfoObj.UOM = item.UomshortName;
                                    MatrialReleaseInfoObj.Comment = item.Comments;
                                    MatrialReleaseInfoObj.NewComment = item.NewComments;
                                    MatrialReleaseInfoObj.ItemCode = item.ItemCode;
                                    MatrialReleaseInfoObj.ProjectID = item.ProjectId;
                                    MatrialReleaseInfoObj.ProjectName = item.ProjectName + ClientName;
                                    MatrialReleaseInfoObj.FabOrderName = item.FabricationOrderId != null ? item.FabricationOrderId.ToString() : "";
                                    //Created By: Mark Shawky 2022-11-9
                                    //Start
                                    MatrialReleaseInfoObj.RecivedQTY = item.NewRecivedQuantity;
                                    var InventoryStoreItemDb = _unitOfWork.InventoryStoreItems.FindAll(a =>
                                    (a.OperationType.Contains("Release Order") || a.OperationType.Contains("POS Release"))
                                    && a.OrderId == InventoryItemMatrialReleaseInfoObj.ID && a.AddingOrderItemId == item.Id, includes: new[] { "InvenoryStoreLocation" }).FirstOrDefault();
                                    if (InventoryStoreItemDb != null)
                                    {
                                        MatrialReleaseInfoObj.StoreLocationID = InventoryStoreItemDb.InvenoryStoreLocationId;
                                        MatrialReleaseInfoObj.StoreLocationName = InventoryStoreItemDb.InvenoryStoreLocation?.Location;
                                        if (InventoryStoreItemDb.ExpDate != null)
                                        {
                                            MatrialReleaseInfoObj.ExpirationDate = InventoryStoreItemDb.ExpDate.ToString().Split(' ')[0];
                                        }
                                        if (InventoryStoreItemDb.ItemSerial != null)
                                        {
                                            MatrialReleaseInfoObj.Serial = InventoryStoreItemDb.ItemSerial;

                                        }

                                        // Michael 
                                        // Comment from order parent for the same item 
                                        var ParentInventoryStoreItemDb = _unitOfWork.InventoryStoreItems.FindAll(x => x.Id == InventoryStoreItemDb.ReleaseParentId).FirstOrDefault();
                                        if (ParentInventoryStoreItemDb != null)
                                        {
                                            if (ParentInventoryStoreItemDb.OperationType.Contains("Internal Back Order"))
                                            {
                                                // Query on 
                                                MatrialReleaseInfoObj.ParentItemComment = _unitOfWork.VInventoryInternalBackOrderItems.FindAll(x => x.Id == ParentInventoryStoreItemDb.AddingOrderItemId && x.InventoryItemId == ParentInventoryStoreItemDb.InventoryItemId).Select(x => x.Comments).FirstOrDefault();
                                            }
                                            else if (ParentInventoryStoreItemDb.OperationType.Contains("Transfer Order"))
                                            {
                                                MatrialReleaseInfoObj.ParentItemComment = _unitOfWork.VInventoryInternalTransferOrderItems.FindAll(x => x.Id == ParentInventoryStoreItemDb.AddingOrderItemId && x.InventoryItemId == ParentInventoryStoreItemDb.InventoryItemId).Select(x => x.Comments).FirstOrDefault();
                                            }
                                            else if (ParentInventoryStoreItemDb.OperationType.Contains("Add New Matrial"))
                                            {
                                                MatrialReleaseInfoObj.ParentItemComment = _unitOfWork.VInventoryAddingOrderItems.FindAll(x => x.Id == ParentInventoryStoreItemDb.AddingOrderItemId && x.InventoryItemId == ParentInventoryStoreItemDb.InventoryItemId).Select(x => x.Comments).FirstOrDefault();
                                            }
                                        }
                                    }
                                    //// Rem Qty for each Item From Matrial Request
                                    //var MatrialRequstObjDB = _Context.proc_InventoryMatrialRequestItemsLoadByPrimaryKey(item.InventoryMatrialRequestItemID).FirstOrDefault();
                                    //if (MatrialRequstObjDB != null)
                                    //{
                                    //}
                                    MatrialReleaseInfoObj.RemQTY = (item.ReqQuantity ?? 0) - (item.NewRecivedQuantity ?? 0);

                                    //End

                                    MatrialReleaseInfList.Add(MatrialReleaseInfoObj);
                                }
                            }
                        }

                        InventoryItemMatrialReleaseInfoObj.MatrialReleaseInfoList = MatrialReleaseInfList;

                    }
                    Response.InventoryItemInfo = InventoryItemMatrialReleaseInfoObj;


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


        public GetInventoryStoreItemBatchWithExpDateResponse GetInventoryStoreItemBatchWithExpDate(GetInventoryStoreItemBatchWithExpDateFilters filters)
        {
            GetInventoryStoreItemBatchWithExpDateResponse Response = new GetInventoryStoreItemBatchWithExpDateResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                decimal SumTotalStockBalance = 0;
                var DDLList = new List<InventoryStoreItemBatchWithExpDate>();
                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        var InventoryStorItemQueryableDB = _unitOfWork.VInventoryStoreItems.FindAllQueryable(a=>true);
                        if (filters.InventoryItemID != 0)
                        {
                            InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => x.InventoryItemId == filters.InventoryItemID);
                        }
                        if (filters.StorID != 0)
                        {
                            InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => x.InventoryStoreId == filters.StorID);
                        }

                        if (filters.StorLocationID != 0)
                        {
                            InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => x.InvenoryStoreLocationId == filters.StorLocationID);
                        }

                        if (filters.ReportID != 0)
                        {
                            var InventoryReportItemsList = _unitOfWork.InventoryReportItems.FindAll(x => x.InventoryReportId == filters.ReportID).ToList();
                            var IDSInvItemsReportItemList = InventoryReportItemsList.Select(x => x.Id).ToList();
                            var IDSInventoryItemList = InventoryReportItemsList.Select(x => x.InventoryItemId).ToList();
                            if (IDSInvItemsReportItemList.Count() > 0)
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => !IDSInventoryItemList.Contains(x.InventoryItemId));
                                var IDSInvStoreItems = _unitOfWork.InventoryReportItemParents.FindAll(x => IDSInvItemsReportItemList.Contains(x.InvReportItemId)).Select(x => x.InvStoreItemId).ToList();
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => !IDSInvStoreItems.Contains(x.Id));
                            }
                        }
                        if (filters.Serial != null)
                        {
                            InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => x.ItemSerial == filters.Serial);
                        }
                        if (filters.ExpDate!=null)
                        {
                            InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.Where(x => x.ExpDate == filters.ExpDate);
                        }
                        if (filters.SortByASC != null && filters.SortByASC != "")
                        {
                            if (filters.SortByASC.ToLower() == "name")
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.OrderBy(x => x.InventoryItemName);
                            }
                            else if (filters.SortByASC == "code")
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.OrderBy(x => x.Code);
                            }
                            else if (filters.SortByASC == "expdate")
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.OrderBy(x => x.ExpDate);
                            }
                        }
                        if (filters.SortByDESC != null && filters.SortByDESC != "")
                        {
                            if (filters.SortByDESC.ToLower() == "name")
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.OrderByDescending(x => x.InventoryItemName);
                            }
                            else if (filters.SortByDESC.ToLower() == "code")
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.OrderByDescending(x => x.Code);
                            }
                            else if (filters.SortByDESC.ToLower() == "expdate")
                            {
                                InventoryStorItemQueryableDB = InventoryStorItemQueryableDB.OrderByDescending(x => x.ExpDate);
                            }
                        }
                        var InventoryStoreItemGrouping = InventoryStorItemQueryableDB.ToList().GroupBy(x => new
                        {
                            x.ExpDate,
                            x.ItemSerial,
                            x.InventoryItemId,
                            x.InventoryItemName,
                            x.Code,
                            x.PurchasingUomshortName,
                            x.InvenoryStoreLocationId,
                            x.InventoryStoreId,
                            x.InventoryStoreName
                        }).AsQueryable();

                        var InventoryStoreItemGroupingList = InventoryStoreItemGrouping.ToList();
                        var StoreLocationList = _unitOfWork.InventoryStoreLocations.GetAll();
                        foreach (var item in InventoryStoreItemGroupingList)
                        {
                            var Obj = new InventoryStoreItemBatchWithExpDate();
                            var Key = item.Key;
                            string StoreLocation = StoreLocationList.Where(x => x.Id == Key.InvenoryStoreLocationId).Select(x => x.Location).FirstOrDefault();
                            // Common.GetStoreLocationName(Key.InvenoryStoreLocationID);
                            Obj.storeId = Key.InventoryStoreId;
                            Obj.InventoryItemID = Key.InventoryItemId;
                            Obj.ItemName = Key.InventoryItemName;
                            Obj.ItemCode = Key.Code;
                            Obj.PurchasingUOM = Key.PurchasingUomshortName;
                            Obj.storeLocationId = Key.InvenoryStoreLocationId;
                            Obj.storeName = Key.InventoryStoreName;
                            Obj.serial = Key.ItemSerial;
                            Obj.expDate = Key.ExpDate != null ? ((DateTime)Key.ExpDate).ToShortDateString() : null;
                            Obj.storelocation = string.IsNullOrEmpty(StoreLocation) ? "Missing Location" : StoreLocation;
                            Obj.totalStockBalanceWithHold = 0;
                            Obj.availableStockBalance = 0;
                            Obj.holdQty = 0;
                            Obj.stockBalanceList = new List<InventoryStoreItemIDWithQTY>();
                            foreach (var ISIBalance in item)
                            {
                                Obj.CreationDate = ISIBalance.CreationDate.ToString("yyyy-MM-dd");


                                var InventoryStoreItemIDWithQTY = new InventoryStoreItemIDWithQTY();
                                decimal HoldQty = 0;
                                decimal FinalBalance = ISIBalance.FinalBalance != null ? (decimal)ISIBalance.FinalBalance : ISIBalance.Balance;
                                if (FinalBalance > 0)
                                {
                                    InventoryStoreItemIDWithQTY.ID = ISIBalance.Id;
                                    InventoryStoreItemIDWithQTY.StockBalance = FinalBalance - (ISIBalance.HoldQty ?? 0);
                                    Obj.stockBalanceList.Add(InventoryStoreItemIDWithQTY);
                                    HoldQty = (ISIBalance.HoldQty ?? 0);
                                }

                                Obj.availableStockBalance += InventoryStoreItemIDWithQTY.StockBalance;
                                Obj.holdQty += HoldQty;
                                Obj.totalStockBalanceWithHold += (HoldQty + InventoryStoreItemIDWithQTY.StockBalance);
                            }
                            SumTotalStockBalance += Obj.availableStockBalance;
                            DDLList.Add(Obj);
                        }
                    }
                    var InventoryStoreItemGroupingQuerable = DDLList.Where(x => x.totalStockBalanceWithHold > 0).AsQueryable();

                    var PagingList = PagedList<InventoryStoreItemBatchWithExpDate>.Create(InventoryStoreItemGroupingQuerable.OrderByDescending(x => x.ItemName), filters.CurrentPage, filters.NumberOfItemsPerPage);
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = PagingList.TotalPages,
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = PagingList.TotalCount
                    };
                    Response.DDLList = PagingList.Where(x => x.totalStockBalanceWithHold > 0).ToList();
                }

                Response.SumTotalStockBalance = SumTotalStockBalance;
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


        public BaseResponseWithId<long> AddInventoryItemMatrialRelease(AddInventoryItemMatrialReleaseRequest Request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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

                    long MatrialReleaseOrderId = 0;
                    if (Request.MatrialReleaseOrderId != null)
                    {
                        MatrialReleaseOrderId = (long)Request.MatrialReleaseOrderId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Matrial Release Order Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long MatrialRequestOrderId = 0;
                    if (Request.MatrialRequestOrderId != null)
                    {
                        MatrialRequestOrderId = (long)Request.MatrialRequestOrderId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err325";
                        error.ErrorMSG = "Invalid Matrial Request Order Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialRequestOrderDB = _unitOfWork.VInventoryMatrialRequests.FindAll(x => x.Id == MatrialRequestOrderId).FirstOrDefault();
                    if (MatrialRequestOrderDB == null) // Invalid Matrial Request ID
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err325";
                        error.ErrorMSG = "Invalid Matrial Request Order Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (MatrialRequestOrderDB.Status == "Closed")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err328";
                        error.ErrorMSG = "Matrial Request Order Id is already closed.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    DateTime RequestDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RequestDate) || !DateTime.TryParse(Request.RequestDate, out RequestDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Request Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.MatrialReleaseItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.MatrialReleaseItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.MatrialReleaseItemList.Where(x => x.NewRecivedQTY == null).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-416";
                        error.ErrorMSG = "New Recived Qty required for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    //if (Request.MatrialReleaseItemList.Where(x => x.NewRecivedQTY > x.RemQTY).Count() > 0)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-416";
                    //    error.ErrorMSG = "New Recived Qty must be less than remain Qty for each item";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}

                    if (Request.MatrialReleaseItemList.Where(x => x.MatrialRequestItemID == 0).Count() > 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-417";
                        error.ErrorMSG = "Matrial Request Item ID required for each item";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    //if (Request.MatrialReleaseItemList.Where(x => x.NewRecivedQTY > x.StockQTY).Count() > 0)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-416";
                    //    error.ErrorMSG = "New Recived Qty must be less than Stock Qty for each item";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    var MatrialRequestItemIDsList = Request.MatrialReleaseItemList.Select(x => x.MatrialRequestItemID).ToList();
                    var MatrialRequestItemListDB = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => MatrialRequestItemIDsList.Contains(x.Id)).ToList();

                    // check If Balance Avaliable from PArent Release or NOT
                    // validate is Count distinct < count of Items => there is iteration items with same data
                    var ItemTotalCount = Request.MatrialReleaseItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Count();
                    var ItemDistinctCount = Request.MatrialReleaseItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Distinct().Count();

                    //var ItemDistinctCountaaaaa = Request.MatrialReleaseItemList.Where(x=> x.StockBalanceList != null).Select(x =>   x.StockBalanceList.Select(a=>a.ID).Distinct()).Distinct().Count();
                    //var ItemDistinctCount = Request.MatrialReleaseItemList.Select(x => new { x.MatrialRequestItemID, x.StockBalanceList }).Distinct().Count();
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
                    foreach (var item in Request.MatrialReleaseItemList)
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

                            if (item.NewRecivedQTY > item.StockQTY)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-416";
                                error.ErrorMSG = "Parent Release Item selected Not have balance enough on Item NO#" + Counter;
                                Response.Errors.Add(error);
                            }

                        }


                        var MatrialRequestItemObjDB = MatrialRequestItemListDB.Where(x => x.Id == item.MatrialRequestItemID).FirstOrDefault();
                        if (MatrialRequestItemObjDB != null)
                        {
                            if (MatrialRequestItemObjDB.RecivedQuantity1 >= MatrialRequestItemObjDB.ReqQuantity1 ||
                                ((decimal)(MatrialRequestItemObjDB.RecivedQuantity1 ?? 0) + item.NewRecivedQTY ?? 0) > (decimal)(MatrialRequestItemObjDB.ReqQuantity1 ?? 0))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err325";
                                error.ErrorMSG = "Can't release on Item NO #" + Counter + " because recived all quantity required";
                                Response.Errors.Add(error);
                            }

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-416";
                            error.ErrorMSG = "Invalid Matrial Request Item Id  NO#" + Counter;
                            Response.Errors.Add(error);
                        }


                    }

                    //if (Request.IsFinish == null)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-15";
                    //    error.ErrorMSG = "please select Is Finish True Or False.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    //bool? IsFinish = Request.IsFinish;


                    if (Request.IsRenew == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please select Is Renew True Or False.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    bool? IsRenew = Request.IsRenew;



                    #endregion

                    if (Response.Result)
                    {
                        Response.ID = MatrialRequestOrderId;
                        string Status = "";
                        var MatrialReleaseObj = new InventoryMatrialRelease();

                        //if (Request.IsFinish == true)
                        //    Status = "Closed";
                        //else
                        //    Status = "Open";
                        if (Request.IsRenew == true && MatrialReleaseOrderId == 0) // New Release Order
                        {
                            //ObjectParameter IDMatrialReleaseOrder = new ObjectParameter("ID", typeof(long));
                            //var MatrialReleaseInsertion = _Context.proc_InventoryMatrialReleaseInsert(IDMatrialReleaseOrder, MatrialRequestOrderDB.FromUserID, MatrialRequestOrderDB.ToInventoryStoreID, RequestDate,
                            //                                                                          DateTime.Now, validation.userID, DateTime.Now, validation.userID, true, "Closed", MatrialRequestOrderId);
                            //if (MatrialReleaseInsertion > 0)
                            //{
                            //    MatrialReleaseOrderId = (long)IDMatrialReleaseOrder.Value;
                            //}


                            // ######################### Other Method to insert for roll back transaction ###################################
                            DateTime CreationDate = DateTime.Now;
                            if (!string.IsNullOrEmpty(Request.CreationDate) && DateTime.TryParse(Request.CreationDate, out CreationDate))
                            {
                                CreationDate = DateTime.Parse(Request.CreationDate);
                            }


                            MatrialReleaseObj.ToUserId = MatrialRequestOrderDB.FromUserId;
                            MatrialReleaseObj.FromInventoryStoreId = MatrialRequestOrderDB.ToInventoryStoreId;
                            MatrialReleaseObj.RequestDate = RequestDate;
                            MatrialReleaseObj.CreationDate = CreationDate;
                            MatrialReleaseObj.ModifiedDate = DateTime.Now;
                            MatrialReleaseObj.CreatedBy = creator;
                            MatrialReleaseObj.ModifiedBy = creator;
                            MatrialReleaseObj.Active = true;
                            MatrialReleaseObj.Status = "Closed";
                            MatrialReleaseObj.MatrialRequestId = MatrialRequestOrderId;

                            _unitOfWork.InventoryMatrialReleases.Add(MatrialReleaseObj);
                            _unitOfWork.Complete();
                        }
                        else // Uppdate Release Order
                        {
                            MatrialReleaseObj = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.Id == MatrialReleaseOrderId).FirstOrDefault();
                            //_Context.V_InventoryMatrialRelease.Where(x => x.ID == MatrialReleaseOrderId).FirstOrDefault();
                            if (MatrialReleaseObj != null)
                            {
                                // Cannot Edit if status is closed 2022-10-10
                                if (MatrialReleaseObj.Status == "Open")
                                {
                                    //_Context.proc_InventoryMatrialReleaseUpdate(MatrialReleaseOrderId, MatrialRequestOrderDB.FromUserID, MatrialRequestOrderDB.ToInventoryStoreID, RequestDate, MatrialReleaseOrderDB.CreationDate, MatrialReleaseOrderDB.CreatedBy,
                                    //    DateTime.Now, validation.userID, true, Status, MatrialRequestOrderId);

                                    // ######################### Other Method to insert for roll back transaction ###################################
                                    MatrialReleaseObj.ToUserId = MatrialRequestOrderDB.FromUserId;
                                    MatrialReleaseObj.FromInventoryStoreId = MatrialRequestOrderDB.ToInventoryStoreId;
                                    MatrialReleaseObj.RequestDate = RequestDate;
                                    MatrialReleaseObj.ModifiedDate = DateTime.Now;
                                    MatrialReleaseObj.ModifiedBy = creator;
                                    MatrialReleaseObj.Active = true;
                                    MatrialReleaseObj.Status = Status;
                                    MatrialReleaseObj.MatrialRequestId = MatrialRequestOrderId;
                                    _unitOfWork.Complete();
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err425";
                                    error.ErrorMSG = "Cannot Update Matiral Release because Status is closed";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else // Invalid Matiral Release with Update (Renew = false)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err425";
                                error.ErrorMSG = "Invalid Matrial Release Order Id with Renew = false.";
                                Response.Errors.Add(error);
                                return Response;
                            }
                        }


                        int itemCount = 0;


                        // Get All InventoryStoreItems Used in this Release before loop
                        var IDSInventoryItemListRequested = MatrialRequestItemListDB.Select(x => x.InventoryItemId).Distinct().ToList();
                        var InventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId)).ToList();
                        var IDSSalesOfferProduct = MatrialRequestItemListDB.Select(x => x.OfferItemId).ToList();
                        var SalesOfferProductsList = _unitOfWork.SalesOfferProducts.FindAll(x => IDSSalesOfferProduct.Contains(x.Id)).ToList();
                        var POItemList = _unitOfWork.PurchasePOItems.FindAll(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId), includes: new[] { "PurchasePo.PurchasePoinvoices" }).ToList();
                        #region items Insertion And More Calc
                        foreach (var item in Request.MatrialReleaseItemList)
                        {
                            itemCount++;

                            #region Matrial Release Update
                            //if (item.ID != 0) // ID Release item Update
                            //{
                            //    var MatrialReleaseItemObjDB = _Context.proc_InventoryMatrialReleaseItemsLoadByPrimaryKey(item.ID).FirstOrDefault();
                            //    if (MatrialReleaseItemObjDB != null)
                            //    {

                            //        var MatrialReleaseItemUpdate = _Context.Myproc_InventoryMatrialReleaseItemsUpdate_RecivedQTY(item.ID,
                            //                                                                                    item.NewComment,
                            //                                                                                    item.NewRecivedQTY
                            //                                                                                    );
                            //        if (MatrialReleaseItemUpdate > 0)
                            //        {
                            //            var MatrialRequestItemObjDB = _Context.proc_InventoryMatrialRequestItemsLoadByPrimaryKey(MatrialReleaseItemObjDB.InventoryMatrialRequestItemID).FirstOrDefault();
                            //            if (MatrialRequestItemObjDB != null)
                            //            {
                            //                var InventoryItemObjDB = _Context.proc_InventoryItemLoadByPrimaryKey(MatrialRequestItemObjDB.InventoryItemID).FirstOrDefault();
                            //                // Update Matrial Request Item
                            //                var RecivedQuantityForMR = MatrialRequestItemObjDB.RecivedQuantity - MatrialReleaseItemObjDB.RecivedQuantity;
                            //                RecivedQuantityForMR = RecivedQuantityForMR + item.NewRecivedQTY;
                            //                //// add to store
                            //                decimal deff = (MatrialReleaseItemObjDB.RecivedQuantity ?? 0) - (item.NewRecivedQTY ?? 0);
                            //                if (deff > 0)
                            //                {
                            //                    // Add Store Item
                            //                    string OperationType = MatrialRequestOrderDB.RequestType != "" ? "Return From Release Order " + "(Material Req. Type: " + MatrialRequestOrderDB.RequestType + ")" : "Return From Release Order";
                            //                    ObjectParameter IDStoreItem = new ObjectParameter("ID", typeof(long));
                            //                    _Context.Myproc_InventoryStoreItemInsert_New(IDStoreItem,
                            //                                                           MatrialRequestOrderDB.ToInventoryStoreID,
                            //                                                           MatrialRequestItemObjDB.InventoryItemID,
                            //                                                           MatrialReleaseOrderId.ToString(),
                            //                                                           MatrialReleaseOrderId,
                            //                                                           DateTime.Now,
                            //                                                           validation.userID,
                            //                                                           DateTime.Now,
                            //                                                           validation.userID,
                            //                                                           OperationType,
                            //                                                           (decimal)(deff / (InventoryItemObjDB.ExchangeFactor ?? 1)),
                            //                                                           item.StoreLocationID,
                            //                                                           null,
                            //                                                           null,
                            //                                                           null, // Will Change
                            //                                                           (decimal)(deff / (InventoryItemObjDB.ExchangeFactor ?? 1)),
                            //                                                           item.ID,
                            //                                                           null
                            //                                                           );
                            //                    //GarasERP.InventoryItem item = new InventoryItem();
                            //                    //if (item.LoadByPrimaryKey(items.InventoryItemID))
                            //                    //{
                            //                    //    GarasERP.InventoryStoreItem storeItem = new GarasERP.InventoryStoreItem();
                            //                    //    storeItem.AddNew();
                            //                    //    storeItem.Balance = (decimal)(deff / (decimal)item.ExchangeFactor);
                            //                    //    storeItem.CreatedBy = UserID;
                            //                    //    storeItem.CreationDate = DateTime.Now;
                            //                    //    storeItem.InventoryItemID = items.InventoryItemID;
                            //                    //    storeItem.InventoryStoreID = order.ToInventoryStoreID;
                            //                    //    storeItem.ModifiedBy = UserID;
                            //                    //    storeItem.ModifiedDate = DateTime.Now;
                            //                    //    storeItem.OperationType = requestType != "" ? "Return From Release Order " + "(Material Req. Type: " + requestType + ")" : "Return From Release Order";
                            //                    //    storeItem.OrderID = release.ID;
                            //                    //    storeItem.OrderNumber = release.s_ID;
                            //                    //    storeItem.Save();
                            //                    //}
                            //                }

                            //                var MatiralRequestItemUpdate = _Context.Myproc_InventoryMatrialRequestItemsUpdate_RecuvedQTY(MatrialRequestItemObjDB.ID,
                            //                                                                                                             RecivedQuantityForMR);




                            //                // HR Custody Update -----------------

                            //                if (RecivedQuantityForMR > 0)
                            //                {
                            //                    var CustodyDb = _Context.proc_HRCustodyLoadAll().Where(a => a.MaterialRequestItemId == MatrialRequestItemObjDB.ID).FirstOrDefault();
                            //                    if (CustodyDb != null)
                            //                    {
                            //                        int CustodyStatus = 0;

                            //                        var MaterialReleasedItemsDb = _Context.proc_InventoryMatrialReleaseItemsLoadAll()
                            //                            .Where(a => a.InventoryMatrialRequestItemID == CustodyDb.MaterialRequestItemId).ToList();
                            //                        var MaterialReleasedItemsIDs = MaterialReleasedItemsDb.Select(a => a.ID).ToList();

                            //                        var InterrnalBackOrdersDb = _Context.proc_InventoryInternalBackOrderItemsLoadAll().Where(
                            //                            a => MaterialReleasedItemsIDs.Contains(a.InventoryMatrialReleaseItemID ?? 0)).ToList();
                            //                        var TotalReturned = InterrnalBackOrdersDb.Sum(a => a.RecivedQuantity);

                            //                        if (TotalReturned > 0)
                            //                        {
                            //                            if (RecivedQuantityForMR < MatrialRequestItemObjDB.ReqQuantity)
                            //                            {
                            //                                if (TotalReturned < RecivedQuantityForMR)
                            //                                {
                            //                                    CustodyStatus = 7; //Partially Recieved - Partially Returned
                            //                                }
                            //                                else
                            //                                {
                            //                                    CustodyStatus = 6; //Partially Recieved - Fully Returned
                            //                                }
                            //                            }
                            //                            else
                            //                            {
                            //                                if (TotalReturned < RecivedQuantityForMR)
                            //                                {
                            //                                    CustodyStatus = 4; //Fully Recieved - Partially Returned
                            //                                }
                            //                                else
                            //                                {
                            //                                    CustodyStatus = 5; //Fully Recieved - Fully Returned
                            //                                }
                            //                            }
                            //                        }
                            //                        else
                            //                        {
                            //                            if (RecivedQuantityForMR < MatrialRequestItemObjDB.ReqQuantity)
                            //                            {
                            //                                CustodyStatus = 3; //Partially Recieved
                            //                            }
                            //                            else
                            //                            {
                            //                                CustodyStatus = 2; //Fully Recieved
                            //                            }
                            //                        }

                            //                        var CustodyUpdate = _Context.proc_HRCustodyUpdate(CustodyDb.ID,
                            //                                                                            CustodyDb.UserID,
                            //                                                                            CustodyDb.IsAssetsType,
                            //                                                                            CustodyDb.InventoryItemID,
                            //                                                                            CustodyDb.MaterialRequestItemId,
                            //                                                                            CustodyDb.MaterialRequestId,
                            //                                                                            CustodyDb.Description,
                            //                                                                            null,
                            //                                                                            null,
                            //                                                                            null,
                            //                                                                            CustodyStatus,
                            //                                                                            true,
                            //                                                                            DateTime.Now.Date,
                            //                                                                            validation.userID,
                            //                                                                            DateTime.Now.Date,
                            //                                                                            validation.userID
                            //                                                                            );
                            //                    }

                            //                }


                            //                // Cost Center to DB IsFinish-----------------

                            //                #region Save Cost Center to DB
                            //                if (Request.IsFinish == true)
                            //                {

                            //                    //decimal amountOfItem = 0;
                            //                    //decimal Quantity = 0;
                            //                    //InventoryItem inventoryItem = new InventoryItem();
                            //                    //inventoryItem.Where.ID.Value = long.Parse(row["InventoryItemID"].ToString());
                            //                    //if (inventoryItem.Query.Load())
                            //                    //{
                            //                    //}
                            //                    decimal Quantity = item.NewRecivedQTY ?? 0; //decimal.Parse(row["NewRecivedQuantity"].ToString());
                            //                    var UnitPrice = InventoryItemObjDB.AverageUnitPrice > 0 ? InventoryItemObjDB.AverageUnitPrice : InventoryItemObjDB.CustomeUnitPrice;
                            //                    decimal amountOfItem = Quantity * UnitPrice;

                            //                    //long costCenterID = 0;

                            //                    long costCenterID = _Context.proc_GeneralActiveCostCentersLoadAll().Where(x => x.CategoryID == MatrialRequestItemObjDB.ProjectID).Select(x => x.ID).FirstOrDefault();
                            //                    if (costCenterID == 0) // Create new 
                            //                    {
                            //                        var ProjectDB = _Context.V_Project_SalesOffer.Where(x => x.ID == MatrialRequestItemObjDB.ProjectID).FirstOrDefault();
                            //                        if (ProjectDB != null)
                            //                        {
                            //                            ObjectParameter IDGeneral = new ObjectParameter("ID", typeof(long));
                            //                            _Context.proc_GeneralActiveCostCentersInsert(IDGeneral, ProjectDB.ProjectName, "Project", MatrialRequestItemObjDB.ProjectID, ProjectDB.ProjectSerial, "",
                            //                                                                        ProjectDB.FinalOfferPrice, 0, true, false, validation.userID, DateTime.Now, validation.userID, DateTime.Now);

                            //                            costCenterID = (long)IDGeneral.Value;
                            //                        }
                            //                    }
                            //                    //GeneralActiveCostCenters generalCostCentersOld = new GeneralActiveCostCenters();
                            //                    //generalCostCentersOld.Where.CategoryID.Value = long.Parse(row["ProjectID"].ToString());
                            //                    //if (generalCostCentersOld.Query.Load())
                            //                    //{
                            //                    //    if (generalCostCentersOld.DefaultView != null && generalCostCentersOld.DefaultView.Count > 0)
                            //                    //    {
                            //                    //        costCenterID = generalCostCentersOld.ID;
                            //                    //    }
                            //                    //}
                            //                    ObjectParameter IDDailyAdjustingEntry = new ObjectParameter("ID", typeof(long));
                            //                    _Context.Myproc_DailyAdjustingEntryCostCenterInsert_New(IDDailyAdjustingEntry,
                            //                                                                            0,
                            //                                                                            costCenterID,
                            //                                                                            "Matrial Release",
                            //                                                                            MatrialReleaseOrderId,
                            //                                                                            InventoryItemObjDB.ID,
                            //                                                                            InventoryItemObjDB.InventoryItemCategoryID,
                            //                                                                            "",
                            //                                                                            true,
                            //                                                                            validation.userID,
                            //                                                                            DateTime.Now,
                            //                                                                            validation.userID,
                            //                                                                            DateTime.Now,
                            //                                                                            amountOfItem,
                            //                                                                            "Automatic",
                            //                                                                            Quantity);
                            //                    if (costCenterID != 0)
                            //                    {

                            //                        var GeneralActiveCostCenterObjDB = _Context.proc_GeneralActiveCostCentersLoadByPrimaryKey(costCenterID).FirstOrDefault();
                            //                        _Context.Myproc_GeneralActiveCostCentersUpdate_New(costCenterID, GeneralActiveCostCenterObjDB.CumulativeCost + amountOfItem, validation.userID, DateTime.Now);
                            //                    }
                            //                    //DailyAdjustingEntryCostCenter CostCenterDB = new DailyAdjustingEntryCostCenter();
                            //                    //CostCenterDB.AddNew();
                            //                    //CostCenterDB.DailyAdjustingEntryID = 0;
                            //                    //CostCenterDB.CostCenterID = costCenterID;
                            //                    //CostCenterDB.TypeID = release.ID;
                            //                    //CostCenterDB.Type = "Matrial Release";
                            //                    //CostCenterDB.ProductID = inventoryItem.ID;
                            //                    //CostCenterDB.ProductGroupID = inventoryItem.InventoryItemCategoryID;
                            //                    //CostCenterDB.Quantity = Quantity;
                            //                    //CostCenterDB.Description = "";
                            //                    //CostCenterDB.Amount = amountOfItem;
                            //                    //CostCenterDB.EntryType = "Automatic";
                            //                    //CostCenterDB.Active = true;
                            //                    //CostCenterDB.CreatedBy = UserID;
                            //                    //CostCenterDB.CreationDate = DateTime.Now;
                            //                    //CostCenterDB.ModifiedBy = UserID;
                            //                    //CostCenterDB.Modified = DateTime.Now;
                            //                    //CostCenterDB.Save();

                            //                    //GeneralActiveCostCenters generalCostCenters = new GeneralActiveCostCenters();
                            //                    //generalCostCenters.Where.ID.Value = costCenterID;
                            //                    //if (generalCostCenters.Query.Load())
                            //                    //{
                            //                    //    if (generalCostCenters.DefaultView != null && generalCostCenters.DefaultView.Count > 0)
                            //                    //    {
                            //                    //        generalCostCenters.CumulativeCost = generalCostCenters.CumulativeCost + amountOfItem;
                            //                    //        generalCostCenters.Save();
                            //                    //    }
                            //                    //}
                            //                }
                            //                #endregion
                            //            }
                            //        }
                            //    }

                            //}
                            //else
                            //{
                            //}
                            #endregion
                            //add new 
                            if (item.ID == 0)
                            {
                                List<InventoryStoreItemIDWithQTY> ParentReleaseWithQTYListID = item.StockBalanceList;// Case 1 : Request ParentReleaseItemID
                                                                                                                     // Calc Parent Release , Final Balance After Release
                                var MatrialRequestItemObjDB = MatrialRequestItemListDB.Where(x => x.Id == item.MatrialRequestItemID).FirstOrDefault();
                                //if (MatrialRequestItemObjDB.RecivedQuantity >= MatrialRequestItemObjDB.ReqQuantity ||
                                //    (MatrialRequestItemObjDB.RecivedQuantity + item.NewRecivedQTY) > MatrialRequestItemObjDB.ReqQuantity)
                                //{
                                //    Response.Result = false;
                                //    Error error = new Error();
                                //    error.ErrorCode = "Err325";
                                //    error.ErrorMSG = "Can't release on Item NO #" + itemCount + " because recived all quantity required";
                                //    Response.Errors.Add(error);
                                //    return Response;
                                //}

                                var AvailableItemStockList = _inventoryItemService.GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                                                MatrialRequestItemObjDB.InventoryItemId,
                                                                                                MatrialRequestOrderDB.ToInventoryStoreId,
                                                                                                item.StoreLocationID,// store location
                                                                                                (decimal)item.NewRecivedQTY,
                                                                                                item.IsFIFO);
                                if (item.StockBalanceList == null)// Case 2 : Setting Store FIFO or LIFO for the same Item
                                {
                                    ParentReleaseWithQTYListID = AvailableItemStockList;
                                }

                                if (ParentReleaseWithQTYListID == null || ParentReleaseWithQTYListID.Count() == 0 || ParentReleaseWithQTYListID.Sum(x => x.StockBalance) < item.NewRecivedQTY)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err325";
                                    error.ErrorMSG = "Not have availble qty from parent  Item to release on Item NO #" + itemCount;
                                    Response.Errors.Add(error);
                                    //return Response;
                                }



                                if (MatrialRequestItemObjDB != null && ParentReleaseWithQTYListID.Count() > 0)
                                {
                                    ParentReleaseWithQTYListID = ParentReleaseWithQTYListID.Where(x => x.StockBalance > 0).ToList();
                                    var InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == MatrialRequestItemObjDB.InventoryItemId).FirstOrDefault(); // _Context.proc_InventoryItemLoadByPrimaryKey(MatrialRequestItemObjDB.InventoryItemID).FirstOrDefault();
                                                                                                                                                                        // add matrial release item
                                                                                                                                                                        //ObjectParameter IDMatrialReleaseItem = new ObjectParameter("ID", typeof(long));
                                                                                                                                                                        //var MatrialReleaseItemInsert = _Context.Myproc_InventoryMatrialReleaseItemsInsert_New(IDMatrialReleaseItem,
                                                                                                                                                                        //                                                                                MatrialReleaseObj.ID,
                                                                                                                                                                        //                                                                                item.NewComment,
                                                                                                                                                                        //                                                                                item.MatrialRequestItemID,
                                                                                                                                                                        //                                                                                item.NewRecivedQTY);

                                    // ######################### Other Method to insert for roll back transaction ###################################

                                    var MatrialReleaseItem = new InventoryMatrialReleaseItem();
                                    MatrialReleaseItem.InventoryMatrialReleasetId = MatrialReleaseObj.Id;
                                    MatrialReleaseItem.Comments = item.NewComment;
                                    MatrialReleaseItem.InventoryMatrialRequestItemId = item.MatrialRequestItemID;
                                    MatrialReleaseItem.RecivedQuantity1 = item.NewRecivedQTY;

                                    _unitOfWork.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);

                                    var MatrialReleaseItemInsert = _unitOfWork.Complete();
                                    if (MatrialReleaseItemInsert > 0)
                                    {
                                        long MatrialReleaseItemID = MatrialReleaseItem.Id;
                                        // Update Matrial request item

                                        //var RecivedQuantityForMR = MatrialRequestItemObjDB.RecivedQuantity + item.NewRecivedQTY;
                                        MatrialRequestItemObjDB.RecivedQuantity1 = (MatrialRequestItemObjDB.RecivedQuantity1 ?? 0) + item.NewRecivedQTY??0;
                                        _unitOfWork.Complete();
                                        //var MatiralRequestItemUpdate = _Context.Myproc_InventoryMatrialRequestItemsUpdate_RecuvedQTY(item.MatrialRequestItemID,
                                        //                                                                                            RecivedQuantityForMR);


                                        if (MatrialRequestItemObjDB.RecivedQuantity1 > 0 && MatrialRequestOrderDB.RequestTypeId == 3) // Custoday Type
                                        {
                                            var CustodyDb = _unitOfWork.Hrcustodies.FindAll(a => a.MaterialRequestItemId == MatrialRequestItemObjDB.Id).FirstOrDefault();
                                            if (CustodyDb != null)
                                            {
                                                int CustodyStatus = 0;

                                                var MaterialReleasedItemsDb = _unitOfWork.InventoryMatrialReleaseItems
                                                    .FindAll(a => a.InventoryMatrialRequestItemId == CustodyDb.MaterialRequestItemId).ToList();
                                                var MaterialReleasedItemsIDs = MaterialReleasedItemsDb.Select(a => a.Id).ToList();

                                                var InterrnalBackOrdersDb = _unitOfWork.InventoryInternalBackOrderItems.FindAll(
                                                    a => MaterialReleasedItemsIDs.Contains(a.InventoryMatrialReleaseItemId ?? 0)).ToList();
                                                decimal TotalReturned = InterrnalBackOrdersDb.Sum(a => a.RecivedQuantity1??0);

                                                if (TotalReturned > 0)
                                                {
                                                    if (MatrialRequestItemObjDB.RecivedQuantity1 < MatrialRequestItemObjDB.ReqQuantity1)
                                                    {
                                                        if (TotalReturned < MatrialRequestItemObjDB.RecivedQuantity1)
                                                        {
                                                            CustodyStatus = 7; //Partially Recieved - Partially Returned
                                                        }
                                                        else
                                                        {
                                                            CustodyStatus = 6; //Partially Recieved - Fully Returned
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (TotalReturned < MatrialRequestItemObjDB.RecivedQuantity1)
                                                        {
                                                            CustodyStatus = 4; //Fully Recieved - Partially Returned
                                                        }
                                                        else
                                                        {
                                                            CustodyStatus = 5; //Fully Recieved - Fully Returned
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    if (MatrialRequestItemObjDB.RecivedQuantity1 < MatrialRequestItemObjDB.ReqQuantity1)
                                                    {
                                                        CustodyStatus = 3; //Partially Recieved
                                                    }
                                                    else
                                                    {
                                                        CustodyStatus = 2; //Fully Recieved
                                                    }
                                                }
                                                CustodyDb.StatusId = CustodyStatus;
                                                CustodyDb.ModifiedBy = creator;
                                                CustodyDb.ModifiedDate = DateTime.Now;
                                                _unitOfWork.Complete();
                                                /*                                                var CustodyUpdate = _Context.proc_HRCustodyUpdate(CustodyDb.ID,
                                                                                                                                                    CustodyDb.UserID,
                                                                                                                                                    CustodyDb.IsAssetsType,
                                                                                                                                                    CustodyDb.InventoryItemID,
                                                                                                                                                    CustodyDb.MaterialRequestItemId,
                                                                                                                                                    CustodyDb.MaterialRequestId,
                                                                                                                                                    CustodyDb.Description,
                                                                                                                                                    null,
                                                                                                                                                    null,
                                                                                                                                                    null,
                                                                                                                                                    CustodyStatus,
                                                                                                                                                    true,
                                                                                                                                                    DateTime.Now.Date,
                                                                                                                                                    validation.userID,
                                                                                                                                                    DateTime.Now.Date,
                                                                                                                                                    validation.userID
                                                                                                                                                    );*/
                                            }
                                        }

                                        //// add to store
                                        // Add Store Item
                                        string OperationType = MatrialRequestOrderDB.RequestType != "" ? "Release Order " + "(Material Req. Type: " + MatrialRequestOrderDB.RequestType + ")" : "Release Order";



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



                                                    // Update PO Item Columns
                                                    // Check if Not call PO Item On Parent 
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
                                                InventoryStoreItemOBJ.InventoryStoreId = MatrialRequestOrderDB.ToInventoryStoreId;
                                                InventoryStoreItemOBJ.InventoryItemId = MatrialRequestItemObjDB.InventoryItemId;
                                                InventoryStoreItemOBJ.OrderNumber = MatrialReleaseObj.Id.ToString();
                                                InventoryStoreItemOBJ.OrderId = MatrialReleaseObj.Id;
                                                InventoryStoreItemOBJ.CreatedBy = creator;
                                                InventoryStoreItemOBJ.ModifiedBy = creator;
                                                InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                                InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                                InventoryStoreItemOBJ.OperationType = OperationType;
                                                InventoryStoreItemOBJ.Balance1 = (-ReleaseQTY);
                                                InventoryStoreItemOBJ.InvenoryStoreLocationId = item.StoreLocationID != null ? item.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationId;
                                                InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                                                InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                                                InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                                                InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                                                InventoryStoreItemOBJ.AddingOrderItemId = MatrialReleaseItemID;
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

                                                //ObjectParameter IDStoreItem = new ObjectParameter("ID", typeof(long));
                                                //var InsertionNewInventoryStoreItem = _Context.Myproc_InventoryStoreItemInsert_API(IDStoreItem,
                                                //                                           MatrialRequestOrderDB.ToInventoryStoreID,
                                                //                                           MatrialRequestItemObjDB.InventoryItemID,
                                                //                                           MatrialReleaseOrderId.ToString(),
                                                //                                           MatrialReleaseOrderId,
                                                //                                           DateTime.Now,
                                                //                                           validation.userID,
                                                //                                           DateTime.Now,
                                                //                                           validation.userID,
                                                //                                           OperationType,
                                                //                                          (-ReleaseQTY),
                                                //                                           item.StoreLocationID != null ? item.StoreLocationID : ParentInventoryStoreItem.InvenoryStoreLocationID,
                                                //                                           ParentInventoryStoreItem.expDate,
                                                //                                           ParentInventoryStoreItem.itemSerial,
                                                //                                           ObjParentRelease.ID,
                                                //                                           (-ReleaseQTY),
                                                //                                           MatrialReleaseItemID,
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
                                                //                                            remainItemCosetEGPForRelease,
                                                //                                            remainItemCostOtherCUForRelease,
                                                //                                                                    0,
                                                //                                                                    null
                                                //                                           );

                                                if (InventoryStorItemInsertion > 0)
                                                {
                                                    ListIDSUpdate.Add(InventoryStoreItemOBJ.Id);
                                                    // Update Parent Release on InventoryStoreItem
                                                    if (ParentInventoryStoreItem != null)
                                                    {
                                                        ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
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


                                        }



                                        //var ListInventoryStoreItem = _Context.InventoryStoreItems.Where(x => x.InventoryItemID == MatrialRequestItemObjDB.InventoryItemID && x.finalBalance > 0);

                                        // -------------------------------------------------Update and Calc Average for Inventory Item ----------------------------------------------------------------
                                        var ListInvStoreItemAll = InventoryStoreItemList.Where(x => x.InventoryItemId == InventoryItemObjDB.Id);
                                        var ListInventoryStoreItem = ListInvStoreItemAll.Where(x => x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
                                        var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
                                        var ListIDSPOInvoicesIsFulllyPriced = _unitOfWork.PurchasePoinvoices.FindAll(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
                                        ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
                                        InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;

                                        // Update Avg Unit Price ..just stop for list of inventorystoreItem List
                                        //foreach (var itemId in ListIDSUpdate)
                                        //{
                                        //    var InventoryStoreItemOBJ = ListInvStoreItemAll.Where(x => x.ID == itemId).FirstOrDefault();
                                        //    InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;

                                        //}
                                        _unitOfWork.Complete();


                                        // Uptate Release QTY in  Sales Offer Product 

                                        //var SalesOfferID = Common.GetSalesofferIDFromProjectID(MatrialRequestItemObjDB.ProjectID);
                                        //if (SalesOfferID != 0)
                                        //{
                                        // //var ListSalesOfferProductIDS = _Context.SalesOfferProducts.Where(x => x.OfferID == SalesOfferID && x.InventoryItemID == InternalBackDataOBJ.InventoryItemID).OrderBy(x=>x.ReleasedQty)
                                        //var SalesOfferProdDB = _Context.SalesOfferProducts.Where(x => x.OfferID == SalesOfferID && x.InventoryItemID == MatrialRequestItemObjDB.InventoryItemID /*&& x.Quantity >= (x.ReleasedQty ?? 0)*/)
                                        //    .OrderBy(x => x.ReleasedQty).FirstOrDefault();
                                        if (MatrialRequestItemObjDB.OfferItemId != null)
                                        {
                                            var SalesOfferProdDB = SalesOfferProductsList.Where(x => x.Id == MatrialRequestItemObjDB.OfferItemId).OrderBy(x => x.ReleasedQty).FirstOrDefault();
                                            if (SalesOfferProdDB != null)
                                            {
                                                float? ReleasedQty = MatrialRequestItemObjDB.RecivedQuantity1 != null ? (float)MatrialRequestItemObjDB.RecivedQuantity1 : 0;
                                                if (SalesOfferProdDB.ReleasedQty != null)
                                                {
                                                    SalesOfferProdDB.ReleasedQty += ReleasedQty;

                                                }
                                                else
                                                {
                                                    SalesOfferProdDB.ReleasedQty = ReleasedQty;

                                                }
                                            }

                                        }
                                        _unitOfWork.Complete();
                                        //}



                                        // Check is finish

                                        //check notification
                                        #region Save Cost Center to DB
                                        if (Request.IsFinish == true)
                                        {
                                            decimal Quantity = item.NewRecivedQTY ?? 0; //decimal.Parse(row["NewRecivedQuantity"].ToString());
                                            var UnitPrice = InventoryItemObjDB.AverageUnitPrice > 0 ? InventoryItemObjDB.AverageUnitPrice : InventoryItemObjDB.CustomeUnitPrice;
                                            decimal amountOfItem = Quantity * UnitPrice;

                                            var GeneralActiveCostCenterObjDB = _unitOfWork.GeneralActiveCostCenters.FindAll(x => x.CategoryId == MatrialRequestItemObjDB.ProjectId).FirstOrDefault();

                                            if (GeneralActiveCostCenterObjDB != null)
                                            {

                                                //long costCenterID = _Context.proc_GeneralActiveCostCentersLoadAll().Where(x => x.CategoryID == MatrialRequestItemObjDB.ProjectId).Select(x => x.ID).FirstOrDefault();
                                                long costCenterID = GeneralActiveCostCenterObjDB.Id;
                                                if (costCenterID == 0) // Create new 
                                                {
                                                    var ProjectDB = _Context.VProjectSalesOffers.Where(x => x.Id == MatrialRequestItemObjDB.ProjectId).FirstOrDefault();
                                                    if (ProjectDB != null)
                                                    {
                                                        GeneralActiveCostCenter GeneralActiveCostCenterObj = new GeneralActiveCostCenter();
                                                        GeneralActiveCostCenterObj.CostCenterName = ProjectDB.ProjectName;
                                                        GeneralActiveCostCenterObj.Category = "Project";
                                                        GeneralActiveCostCenterObj.CategoryId = MatrialRequestItemObjDB.ProjectId;
                                                        GeneralActiveCostCenterObj.Serial = ProjectDB.ProjectSerial;
                                                        GeneralActiveCostCenterObj.Description = "";
                                                        GeneralActiveCostCenterObj.SellingPrice = ProjectDB.FinalOfferPrice;
                                                        GeneralActiveCostCenterObj.CumulativeCost = 0;
                                                        GeneralActiveCostCenterObj.Active = true;
                                                        GeneralActiveCostCenterObj.Closed = false;
                                                        GeneralActiveCostCenterObj.CreationDate = DateTime.Now;
                                                        GeneralActiveCostCenterObj.Modified = DateTime.Now;
                                                        GeneralActiveCostCenterObj.CreatedBy = validation.userID;
                                                        GeneralActiveCostCenterObj.ModifiedBy = validation.userID;

                                                        //ObjectParameter IDGeneral = new ObjectParameter("ID", typeof(long));
                                                        //_Context.proc_GeneralActiveCostCentersInsert(IDGeneral, ProjectDB.ProjectName, "Project", MatrialRequestItemObjDB.ProjectID, ProjectDB.ProjectSerial, "",
                                                        //                                            ProjectDB.FinalOfferPrice, 0, true, false, validation.userID, DateTime.Now, validation.userID, DateTime.Now);
                                                        _unitOfWork.Complete();
                                                        costCenterID = GeneralActiveCostCenterObj.Id;
                                                    }
                                                }
                                                //ObjectParameter IDDailyAdjustingEntry = new ObjectParameter("ID", typeof(long));
                                                //_Context.Myproc_DailyAdjustingEntryCostCenterInsert_New(IDDailyAdjustingEntry,
                                                //                                                        0,
                                                //                                                        costCenterID,
                                                //                                                        "Matrial Release",
                                                //                                                        MatrialReleaseObj.ID,
                                                //                                                        InventoryItemObjDB.ID,
                                                //                                                        InventoryItemObjDB.InventoryItemCategoryID,
                                                //                                                        "",
                                                //                                                        true,
                                                //                                                        validation.userID,
                                                //                                                        DateTime.Now,
                                                //                                                        validation.userID,
                                                //                                                        DateTime.Now,
                                                //                                                        amountOfItem,
                                                //                                                        "Automatic",
                                                //                                                        Quantity);

                                                DailyAdjustingEntryCostCenter DailyAdjustingEntryCostCenterDB = new DailyAdjustingEntryCostCenter();
                                                DailyAdjustingEntryCostCenterDB.DailyAdjustingEntryId = 0;
                                                DailyAdjustingEntryCostCenterDB.CostCenterId = costCenterID;
                                                DailyAdjustingEntryCostCenterDB.Type = "Matrial Release";
                                                DailyAdjustingEntryCostCenterDB.TypeId = MatrialReleaseObj.Id;
                                                DailyAdjustingEntryCostCenterDB.ProductId = InventoryItemObjDB.Id;
                                                DailyAdjustingEntryCostCenterDB.ProductGroupId = InventoryItemObjDB.InventoryItemCategoryId;
                                                DailyAdjustingEntryCostCenterDB.Description = "";
                                                DailyAdjustingEntryCostCenterDB.Active = true;
                                                DailyAdjustingEntryCostCenterDB.CreatedBy = validation.userID;
                                                DailyAdjustingEntryCostCenterDB.CreationDate = DateTime.Now;
                                                DailyAdjustingEntryCostCenterDB.ModifiedBy = validation.userID;
                                                DailyAdjustingEntryCostCenterDB.Modified = DateTime.Now;
                                                DailyAdjustingEntryCostCenterDB.Amount = amountOfItem;
                                                DailyAdjustingEntryCostCenterDB.EntryType = "Automatic";
                                                DailyAdjustingEntryCostCenterDB.Quantity1 = Quantity;
                                                _unitOfWork.Complete();


                                                if (costCenterID != 0)
                                                {

                                                    GeneralActiveCostCenterObjDB.CumulativeCost = GeneralActiveCostCenterObjDB.CumulativeCost + amountOfItem;
                                                    GeneralActiveCostCenterObjDB.Modified = DateTime.Now;
                                                    GeneralActiveCostCenterObjDB.ModifiedBy = validation.userID;
                                                    _unitOfWork.Complete();
                                                    //_Context.Myproc_GeneralActiveCostCentersUpdate_New(costCenterID, GeneralActiveCostCenterObjDB.CumulativeCost + amountOfItem, validation.userID, DateTime.Now);
                                                }
                                            }

                                            // Notifications ------------------
                                            //GarasERP.InventoryStoreItem itemBalance = new GarasERP.InventoryStoreItem();
                                            //itemBalance.Where.InventoryItemID.Value = item.ID;
                                            //// itemBalance.Query.AddResultColumn(GarasERP.InventoryStoreItem.ColumnNames.Balance);
                                            //itemBalance.Query.AddGroupBy(GarasERP.InventoryStoreItem.ColumnNames.InventoryItemID);
                                            //itemBalance.Aggregate.Balance.Function = MyGeneration.dOOdads.AggregateParameter.Func.Sum;
                                            //if (itemBalance.Query.Load())
                                            //{
                                            //    if (itemBalance.DefaultView != null && itemBalance.DefaultView.Count > 0)
                                            //    {
                                            //        if (itemBalance.s_Balance != "")
                                            //        {
                                            //            if (itemBalance.Balance <= (decimal)item.MinBalance)
                                            //            {
                                            //                CommonClass.sendGroupNotifications("TopManagment", "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Min Balance" + item.MinBalance,
                                            //                    "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Min Balance" + item.MinBalance,
                                            //                    "~/inventory/ViewItem.aspx?TID=" + Server.UrlEncode(Encrypt_Decrypt.Encrypt(item.s_ID, key)));

                                            //                CommonClass.sendGroupNotifications("Secretary", "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Min Balance" + item.MinBalance,
                                            //                   "Item " + item.Name + "(" + itemBalance.Balance + ") has exceeded the Min Balance" + item.MinBalance,
                                            //                   "~/inventory/ViewItem.aspx?TID=" + Server.UrlEncode(Encrypt_Decrypt.Encrypt(item.s_ID, key)));


                                            //            }
                                            //        }
                                            //    }
                                            //}
                                        }
                                        #endregion
                                    }
                                    #region Save Cost Center to DB
                                    //else if (finish)
                                    //{
                                    //    decimal amountOfItem = 0;
                                    //    decimal Quantity = 0;
                                    //    InventoryItem inventoryItem = new InventoryItem();
                                    //    inventoryItem.Where.ID.Value = long.Parse(row["InventoryItemID"].ToString());
                                    //    if (inventoryItem.Query.Load())
                                    //    {
                                    //        Quantity = decimal.Parse(row["NewRecivedQuantity"].ToString());

                                    //        decimal UnitPrice = 0;
                                    //        if (inventoryItem.CalculationType == 1)
                                    //        {
                                    //            UnitPrice = inventoryItem.AverageUnitPrice;
                                    //        }
                                    //        else if (inventoryItem.CalculationType == 2)
                                    //        {
                                    //            UnitPrice = inventoryItem.MaxUnitPrice;
                                    //        }
                                    //        else if (inventoryItem.CalculationType == 3)
                                    //        {
                                    //            UnitPrice = inventoryItem.LastUnitPrice;
                                    //        }
                                    //        else if (inventoryItem.CalculationType == 4)
                                    //        {
                                    //            UnitPrice = inventoryItem.CustomeUnitPrice;
                                    //        }

                                    //        //var UnitPrice = inventoryItem.CustomeUnitPrice;

                                    //        amountOfItem = Quantity * UnitPrice;
                                    //    }

                                    //    long costCenterID = 0;

                                    //    GeneralActiveCostCenters generalCostCentersOld = new GeneralActiveCostCenters();
                                    //    generalCostCentersOld.Where.CategoryID.Value = long.Parse(row["ProjectID"].ToString());
                                    //    if (generalCostCentersOld.Query.Load())
                                    //    {
                                    //        if (generalCostCentersOld.DefaultView != null && generalCostCentersOld.DefaultView.Count > 0)
                                    //        {
                                    //            costCenterID = generalCostCentersOld.ID;
                                    //        }
                                    //    }

                                    //    DailyAdjustingEntryCostCenter CostCenterDB = new DailyAdjustingEntryCostCenter();
                                    //    CostCenterDB.AddNew();
                                    //    CostCenterDB.DailyAdjustingEntryID = 0;
                                    //    CostCenterDB.CostCenterID = costCenterID;
                                    //    CostCenterDB.TypeID = release.ID;
                                    //    CostCenterDB.Type = "Matrial Release";
                                    //    CostCenterDB.ProductID = inventoryItem.ID;
                                    //    CostCenterDB.ProductGroupID = inventoryItem.InventoryItemCategoryID;
                                    //    CostCenterDB.Quantity = Quantity;
                                    //    CostCenterDB.Description = "";
                                    //    CostCenterDB.Amount = amountOfItem;
                                    //    CostCenterDB.EntryType = "Automatic";
                                    //    CostCenterDB.Active = true;
                                    //    CostCenterDB.CreatedBy = UserID;
                                    //    CostCenterDB.CreationDate = DateTime.Now;
                                    //    CostCenterDB.ModifiedBy = UserID;
                                    //    CostCenterDB.Modified = DateTime.Now;
                                    //    CostCenterDB.Save();

                                    //    GeneralActiveCostCenters generalCostCenters = new GeneralActiveCostCenters();
                                    //    generalCostCenters.Where.ID.Value = costCenterID;
                                    //    if (generalCostCenters.Query.Load())
                                    //    {
                                    //        if (generalCostCenters.DefaultView != null && generalCostCenters.DefaultView.Count > 0)
                                    //        {
                                    //            generalCostCenters.CumulativeCost = generalCostCenters.CumulativeCost + amountOfItem;
                                    //            generalCostCenters.Save();
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                }
                                _unitOfWork.Complete();

                                #endregion items

                                //if (Request.IsFinish == true)
                                //{
                                bool complete = true;
                                var InventoryMatrialRequestItemObjDB = _unitOfWork.InventoryMatrialRequestItems.FindAll(x => x.InventoryMatrialRequestId == MatrialRequestOrderId).ToList();
                                foreach (var itemm in InventoryMatrialRequestItemObjDB)
                                {
                                    if (itemm.ReqQuantity1 > itemm.RecivedQuantity1)
                                    {
                                        complete = false;
                                        break;
                                    }
                                }
                                //InventoryMatrialRequestItems items = new InventoryMatrialRequestItems();
                                //items.Where.InventoryMatrialRequestID.Value = order.ID;
                                //if (items.Query.Load())
                                //{
                                //    do
                                //    {
                                //        if (items.ReqQuantity != items.RecivedQuantity)
                                //        {
                                //            complete = false;
                                //            break;
                                //        }
                                //    } while (items.MoveNext());
                                //}
                                if (complete && InventoryMatrialRequestItemObjDB != null && InventoryMatrialRequestItemObjDB.Count() > 0)
                                {
                                    var request = _unitOfWork.InventoryMatrialRequests.GetById(MatrialRequestOrderId);
                                    if (request != null)
                                    {
                                        request.Status = "Closed";
                                        _unitOfWork.InventoryMatrialRequests.Update(request);
                                        _unitOfWork.Complete();
                                    }
                                    /*_Context.Myproc_InventoryMatrialRequestUpdate_HoldReleased(MatrialRequestOrderId, "Closed");*/
                                }
                                // }
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


        public async Task<BaseResponseWithId<long>> AddInventoryItemMatrialReleasePrintInfo(AddInventoryItemMatrialReleasePrintInfoRequest Request,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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
                    if (Request.MatrialReleasePrintInfoList == null || Request.MatrialReleasePrintInfoList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Matrial Release Print List must at least one ";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long MatrialReleaseOrderId = 0;
                    if (Request.MatrialReleaseOrderId != null && Request.MatrialReleaseOrderId != 0)
                    {
                        MatrialReleaseOrderId = (long)Request.MatrialReleaseOrderId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Matrial Release Order Id.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var MatrialReleaseDB = _unitOfWork.InventoryMatrialReleases.FindAllAsync(x => x.Id == MatrialReleaseOrderId).Result.FirstOrDefault();
                    if (MatrialReleaseDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err324";
                        error.ErrorMSG = "Invalid Matrial Release Order Id not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    #endregion

                    if (Response.Result)
                    {
                        foreach (var item in Request.MatrialReleasePrintInfoList)
                        {
                            if (item.Id == null || item.Id == 0)
                            {
                                var MatrialReleasePrintInfo = new InventoryMatrialReleasePrintInfo();
                                MatrialReleasePrintInfo.InventoryMatrialReleaseId = MatrialReleaseOrderId;
                                MatrialReleasePrintInfo.ClientAddress = item.ClientAddress;
                                MatrialReleasePrintInfo.Comment = item.Comment;
                                MatrialReleasePrintInfo.ShippingMethod = item.ShippingMethod;
                                MatrialReleasePrintInfo.ContactPersonMobile = item.ContactPersonMobile;
                                MatrialReleasePrintInfo.ContactPersonName = item.ContactPersonName;
                                MatrialReleasePrintInfo.PackagingQty = item.PackagingQTY;
                                MatrialReleasePrintInfo.ProjectId = item.ProjectId;
                                MatrialReleasePrintInfo.CreatedBy = creator;
                                MatrialReleasePrintInfo.ModifiedBy = creator;
                                MatrialReleasePrintInfo.ModifiedDate = DateTime.Now;
                                MatrialReleasePrintInfo.CreationDate = DateTime.Now;
                                MatrialReleasePrintInfo.CreationDate = DateTime.Now;

                                _unitOfWork.InventoryMatrialReleasePrintInfoes.Add(MatrialReleasePrintInfo);

                            }
                            else
                            {
                                var MatrialReleasePrintInfo = _unitOfWork.InventoryMatrialReleasePrintInfoes.FindAllAsync(x => x.Id == item.Id).Result.FirstOrDefault();

                                MatrialReleasePrintInfo.ClientAddress = item.ClientAddress;
                                MatrialReleasePrintInfo.Comment = item.Comment;
                                MatrialReleasePrintInfo.ShippingMethod = item.ShippingMethod;
                                MatrialReleasePrintInfo.ContactPersonMobile = item.ContactPersonMobile;
                                MatrialReleasePrintInfo.ContactPersonName = item.ContactPersonName;
                                MatrialReleasePrintInfo.ProjectId = item.ProjectId;
                                MatrialReleasePrintInfo.PackagingQty = item.PackagingQTY;
                                MatrialReleasePrintInfo.ModifiedBy = creator;
                                MatrialReleasePrintInfo.ModifiedDate = DateTime.Now;
                            }
                        }

                        _unitOfWork.Complete();

                        Response.ID = MatrialReleaseOrderId;
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


        public InventoryMatrialReleasePrintInfoResponse GetInventoryItemMatrialReleasetPrintInfo([FromHeader] long MatrialReleaseOrderID)
        {
            InventoryMatrialReleasePrintInfoResponse Response = new InventoryMatrialReleasePrintInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var MatrialReleasePrintInfoList = new List<InventoryMatrialReleasePrintInfoVM>();
                if (Response.Result)
                {
                    /*long MatrialReleaseOrderID = 0;
                    if (!string.IsNullOrEmpty(headers["MatrialReleaseOrderID"]) && long.TryParse(headers["MatrialReleaseOrderID"], out MatrialReleaseOrderID))
                    {
                        long.TryParse(headers["MatrialReleaseOrderID"], out MatrialReleaseOrderID);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err169";
                        error.ErrorMSG = "Invalid Matrial Release Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }*/

                    if (Response.Result)
                    {
                        var MatrialReleaseObjDB = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.Id == MatrialReleaseOrderID).FirstOrDefault();
                        if (MatrialReleaseObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err169";
                            error.ErrorMSG = "This Matrial Release not exist";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        var MatrialReleasePrintInfoDB = _unitOfWork.InventoryMatrialReleasePrintInfoes.FindAll(x => x.InventoryMatrialReleaseId == MatrialReleaseOrderID).ToList();
                        if (MatrialReleasePrintInfoDB != null)
                        {
                            MatrialReleasePrintInfoList = MatrialReleasePrintInfoDB.Select(item => new InventoryMatrialReleasePrintInfoVM()
                            {
                                Id = item.Id,
                                InventoryMatrialReleaseOrderId = item.InventoryMatrialReleaseId,
                                ProjectId = item.ProjectId,
                                Comment = item.Comment,
                                ShippingMethod = item.ShippingMethod,
                                ContactPersonName = item.ContactPersonName,
                                ContactPersonMobile = item.ContactPersonMobile,
                                ClientAddress = item.ClientAddress,
                                PackagingQTY = item.PackagingQty
                            }).ToList();
                            Response.MatrialReleasePrintInfo = MatrialReleasePrintInfoList;
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

        public async Task<GetMatrialReleaseShippingAddressContactResponse> GetMatrialReleaseShippingAddressContact([FromHeader] long MatrialReleaseID)
        {
            GetMatrialReleaseShippingAddressContactResponse response = new GetMatrialReleaseShippingAddressContactResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {



                if (response.Result)
                {

                    //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                    var MainDataDB = await _unitOfWork.VMatrialReleaseSalesOffers.FindAllAsync(x => x.Id == MatrialReleaseID);


                    var MatrialReleaseMainData = new List<MainMatrialReleaseDataVM>();



                    if (MainDataDB != null)
                    {
                        // Modified By Michael 2023-2-8
                        var IDSOffers = MainDataDB.Select(x => x.SalesOfferId).ToList();
                        var ProjectListDB = (await _unitOfWork.SalesOffers.FindAllAsync(x => IDSOffers.Contains(x.Id))).Select(x => new { x.Id, x.ProjectName, x.PricingComment, x.ProjectData, x.TechnicalInfo });
                        foreach (var item in MainDataDB)
                        {


                            var MatrialReleaseVM = new MainMatrialReleaseDataVM();

                            MatrialReleaseVM.mainAddress = item.ProjectLocation;
                            MatrialReleaseVM.SalesOfferID = item.SalesOfferId;
                            MatrialReleaseVM.mainContactName = item.ContactPersonName;
                            MatrialReleaseVM.mainContactMobile = item.ContactPersonMobile;
                            MatrialReleaseVM.ClientID = item.ClientId;
                            MatrialReleaseVM.ProjectID = item.ProjectId;
                            MatrialReleaseVM.ProjectName = ProjectListDB.Where(x => x.Id == item.SalesOfferId).Select(x => x.ProjectName).FirstOrDefault();
                            MatrialReleaseVM.OfferPricingComment = ProjectListDB.Where(x => x.Id == item.SalesOfferId).Select(x => x.PricingComment ?? "").FirstOrDefault();
                            MatrialReleaseVM.OfferProjectData = ProjectListDB.Where(x => x.Id == item.SalesOfferId).Select(x => x.ProjectData ?? "").FirstOrDefault();
                            MatrialReleaseVM.OfferTechnicalInfo = ProjectListDB.Where(x => x.Id == item.SalesOfferId).Select(x => x.TechnicalInfo ?? "").FirstOrDefault();

                            var Client = (await _unitOfWork.Clients.FindAllAsync(x => x.Id == MatrialReleaseVM.ClientID, includes: new[] { "DefaultDelivaryAndShippingMethod" })).FirstOrDefault();
                            if (Client != null)
                            {
                                if (Client.DefaultDelivaryAndShippingMethodId != null)
                                {
                                    MatrialReleaseVM.shippingMethod = Client.DefaultDelivaryAndShippingMethod?.Name;

                                }
                                else if (Client.OtherDelivaryAndShippingMethodName != null)
                                {
                                    MatrialReleaseVM.shippingMethod = Client.OtherDelivaryAndShippingMethodName;
                                }
                            }


                            var GetClientAddressList = new List<ClientAddressListVM>();
                            var GetClientContactList = new List<ClientContactVM>();

                            var ClientAdressesData = await _unitOfWork.ClientAddresses.FindAllAsync(X => X.ClientId == MatrialReleaseVM.ClientID, includes: new[] { "Country" });
                            var governerates = _unitOfWork.Governorates.FindAll(a => ClientAdressesData.Select(x => x.GovernorateId).Contains(a.Id)).ToList();
                            foreach (var ClientDataitem in ClientAdressesData)
                            {
                                var ClientDataVM = new ClientAddressListVM();

                                ClientDataVM.Address = ClientDataitem.Address;
                                ClientDataVM.ClientID = ClientDataitem.ClientId;
                                ClientDataVM.CountryID = ClientDataitem.CountryId;
                                ClientDataVM.CountryName = ClientDataitem.Country.Name;
                                ClientDataVM.GovernorateID = ClientDataitem.GovernorateId;
                                ClientDataVM.GovernorateName = governerates.Where(a=>a.Id== ClientDataitem.GovernorateId).Select(a=>a.Name).FirstOrDefault();

                                GetClientAddressList.Add(ClientDataVM);

                            }

                            //var ClientDataWithMainVM = new ClientAddressListVM();
                            //ClientDataWithMainVM.Address = MatrialReleaseVM.mainAddress;
                            //ClientDataWithMainVM.IsDefault = true;
                            //GetClientAddressList.Add(ClientDataWithMainVM);


                            var ClientContactData = await _unitOfWork.ClientContactPersons.FindAllAsync(X => X.ClientId == MatrialReleaseVM.ClientID);

                            foreach (var ClientContactDataitem in ClientContactData)
                            {
                                var ClientDataVM = new ClientContactVM();

                                ClientDataVM.Name = ClientContactDataitem.Name;
                                ClientDataVM.ID = ClientContactDataitem.Id;
                                ClientDataVM.Title = ClientContactDataitem.Title;
                                ClientDataVM.Location = ClientContactDataitem.Location;
                                ClientDataVM.Mobile = ClientContactDataitem.Mobile;



                                GetClientContactList.Add(ClientDataVM);
                            }

                            //var ClientDataMainVM = new ClientContactVM();
                            //ClientDataMainVM.Name = MatrialReleaseVM.mainContactName;
                            //ClientDataMainVM.Mobile = MatrialReleaseVM.mainContactMobile;
                            //ClientDataMainVM.IsDefault = true;
                            // GetClientContactList.Add(ClientDataMainVM);

                            MatrialReleaseVM.clientAddressList = GetClientAddressList;
                            MatrialReleaseVM.clientContactList = GetClientContactList;

                            MatrialReleaseMainData.Add(MatrialReleaseVM);

                            response.MainMatrialReleaseDataVM = MatrialReleaseMainData;
                        }



                    }

                }

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public async Task<BaseMessageResponse> MatrialReleasePDFReport(MatrialReleasePDFFilters filters, GetMatrialReleaseDataResponse Request)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {                    
                    if (filters.MatrialReleaseOrderID==0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err169";
                        error.ErrorMSG = "Invalid Matrial Release Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int HeaderCounter = 5;
                    if (filters.QtyDetails!=null)
                    {
                        HeaderCounter++;
                    }
                    if (filters.FabOrder != null)
                    {
                        HeaderCounter++;
                    }
                    bool? Serial = null;
                    if (filters.Serial != null)
                    {
                        HeaderCounter++;
                    }
                    bool? Comment = null;
                    if (filters.Comment!=null)
                    {
                        HeaderCounter++;
                    }
                    // Translate To Arabic
                    // Default English
                    string PDFTitle_TXT = "Material Release Report";
                    string MatrialReleaseNO_TXT = "Matrial Release Number ";
                    string ClientName_TXT = " : Client Name ";
                    string InvoiceSerial_TXT = " : Invoice Serial";
                    string Fromstore_TXT = " : From store";
                    string RequestType_TXT = " : Request Type";
                    string SalesMan_TXT = " : Sales Man";
                    string Address_TXT = " : Address";
                    string ContactMobile_TXT = " : Contact Mobile";
                    string ProjectName_TXT = " : Project Name / Serial";
                    string OfferSerial_TXT = " : Offer Serial";
                    string ClientApprovalDate_TXT = " : Client Approval Date";
                    string ToUserName_TXT = " : To User Name";
                    string CreatedDate_TXT = " : Created Date";
                    string ShippingMethod_TXT = " : Shipping Method";
                    string ContactName_TXT = " : Contact Name";
                    string Reciever_TXT = " : Reciever";
                    string GeneralComment_TXT = " : General Comment";
                    string TotalQty_TXT = "Total Qty";
                    string Date_TXT = " : Date";
                    string StoreKeeper_TXT = " : Store Keeper";
                    if (!filters.IsEnglish)
                    {
                        //PDFTitle_TXT = "أمر صرف";
                        //MatrialReleaseNO_TXT = " أمر صرف رقم";
                        ClientName_TXT = " : أسم العميل";
                        InvoiceSerial_TXT = " : رقم الفاتوره";
                        Fromstore_TXT = " : المخزن";
                        RequestType_TXT = " : نوع الصرف";
                        SalesMan_TXT = " : المندوب";
                        Address_TXT = " : العنوان";
                        ContactMobile_TXT = " : رقم التليفون";
                        ProjectName_TXT = " : اسم / رقم الاوردر ";
                        OfferSerial_TXT = " : رقم العرض";
                        ClientApprovalDate_TXT = " : تاريخ موافقة العميل";
                        ToUserName_TXT = " : اسم المستحدم";
                        CreatedDate_TXT = " : التاريخ";
                        ShippingMethod_TXT = " : طريقة الشحن";
                        ContactName_TXT = " : اسم المسئول ";
                        Reciever_TXT = " : المستلم";
                        GeneralComment_TXT = " : ملاحظات اخرى";
                        TotalQty_TXT = "الكميه";
                        Date_TXT = " : التاريخ";
                        StoreKeeper_TXT = " : امين المخزن";
                    }




                    const int Number = 5;




                    var dt = new System.Data.DataTable("grid");


                    dt.Columns.AddRange(new DataColumn[Number]
                    {
                                 new DataColumn("No"),
                                 new DataColumn("Inv. Code"),
                                 new DataColumn("Material Name"),

                                 new DataColumn("RUOM") ,
                                 new DataColumn("Approved QTY (Rec)"),
                        //new DataColumn("Qty Details"),
                        ////new DataColumn("Project / Client") ,
                        //new DataColumn("Fab. Order"),
                        ////new DataColumn("Exp. Date"),
                        //new DataColumn("Serial/Batch"),
                        //new DataColumn("Comment"),
                        //new DataColumn("New Comment"),
                        //new DataColumn("Item Comment")
                        });
                    if (filters.QtyDetails == true)
                    {
                        dt.Columns.Add(new DataColumn("Qty Details"));

                    }
                    if (filters.FabOrder == true)
                    {
                        dt.Columns.Add(new DataColumn("FabOrder"));

                    }
                    if (filters.Serial == true)
                    {
                        dt.Columns.Add(new DataColumn("Serial / Batch"));

                    }
                    if (filters.Comment == true)
                    {
                        dt.Columns.Add(new DataColumn("Comment"));

                    }



                    var Counter = 1;
                    var InventoryItemMatrialReleaselist = GetInventoryItemMatrialReleasetInfo(filters.MatrialReleaseOrderID);
                    var IDSsSProjects = new List<long?>();

                    if (filters.SpecificProjectID != 0)
                    {
                        IDSsSProjects.Add(filters.SpecificProjectID);
                    }
                    else
                    {

                        IDSsSProjects = InventoryItemMatrialReleaselist.InventoryItemInfo?.MatrialReleaseInfoList?.Select(x => x.ProjectID).Distinct().ToList();
                    }

                    // Fill Model Matrial Release Print Infor for each Project
                    // var MatrialReleasePrintInfoList = await _Context.InventoryMatrialReleasePrintInfoes




                    MemoryStream ms = new MemoryStream();

                    //Size of page
                    Document document = new Document(PageSize.A4.Rotate());
                    PdfWriter pw = PdfWriter.GetInstance(document, ms);
                    //Call the footer Function
                    pw.PageEvent = new HeaderFooter();

                    document.Open();
                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);
                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(bf, 9, Font.NORMAL);

                    String path = _host.WebRootPath+"/Attachments";

                        string PDFp_strPath = path+"/logoMarina.png";
                    if (validation.CompanyName == "marinaplt")
                    {
                        //Image logo = Image.GetInstance(PDFp_strPath);
                        //logo.SetAbsolutePosition(80f, 50f);
                        //logo.ScaleAbsolute(600f,600f);
                        if (File.Exists(PDFp_strPath))
                        {
                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                            jpg.SetAbsolutePosition(60f, 550f);
                            //document.Add(logo);
                            document.Add(jpg);
                        }
                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -10;
                        prgHeading.SpacingAfter = 20;
                        Chunk cc = new Chunk(PDFTitle_TXT + " ", fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);
                        prgHeading.Add(cc);
                        document.Add(prgHeading);

                    }
                    else if (validation.CompanyName == "piaroma")
                    {
                         PDFp_strPath = path+"/PI-AROMA.JPEG";
                        if (File.Exists(PDFp_strPath))
                        {
                            Image logo = Image.GetInstance(PDFp_strPath);
                            logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                            logo.ScaleAbsolute(300f, 300f);
                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                            //document.Add(logo);
                            document.Add(jpg);
                        }
                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -10;
                        prgHeading.SpacingAfter = 20;
                        Chunk cc = new Chunk(PDFTitle_TXT + " ", fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 30);
                        prgHeading.Add(cc);
                        document.Add(prgHeading);
                    }
                    else if (validation.CompanyName == "Garastest")
                    {
                         PDFp_strPath = path+"/PI-AROMA.JPEG";
                        Image logo = Image.GetInstance(PDFp_strPath);
                        logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                        logo.ScaleAbsolute(300f, 300f);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                        //document.Add(logo);
                        document.Add(jpg);
                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -10;
                        prgHeading.SpacingAfter = 20;
                        Chunk cc = new Chunk(PDFTitle_TXT, fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 30);
                        prgHeading.Add(cc);
                        document.Add(prgHeading);
                    }

                    var ShippingMethodName = "";
                    var NewShippingMethodName = " ";
                    var CountryName = " ";
                    var GovernorateName = " ";

                    if (IDSsSProjects != null)
                    {
                        foreach (var ProjectID in IDSsSProjects)
                        {
                            var HeaderMatrialReleaseInfo = InventoryItemMatrialReleaselist.InventoryItemInfo;
                            var MatrialReleaseInfoList = InventoryItemMatrialReleaselist.InventoryItemInfo.MatrialReleaseInfoList.Where(x => x.ProjectID == ProjectID).ToList();
                            var ProjectName = MatrialReleaseInfoList.FirstOrDefault()?.ProjectName;
                            var SalesOfferData = new SalesOffer();
                            var InvoicesData = new Invoice();
                            var Client = new Client();
                            var ProjectContactPerson = new ProjectContactPerson();
                            string ContactPersonMobile = "N/A";
                            string ContactPersonName = "N/A";
                            string ProjectLocation = "N/A";
                            if (ProjectID != null)
                            {
                                var ProjectData = _unitOfWork.Projects.FindAll(x => x.Id == ProjectID, includes: new[] { "SalesOffer.Client", "ProjectContactPeople" }).FirstOrDefault();

                                if (ProjectData != null)
                                {
                                    SalesOfferData = ProjectData.SalesOffer;
                                    Client = SalesOfferData.Client;
                                    InvoicesData = SalesOfferData.Invoices.Where(x => x.SalesOfferId == ProjectData.SalesOfferId).FirstOrDefault();

                                    ContactPersonMobile = SalesOfferData.ContactPersonMobile;
                                    ContactPersonName = SalesOfferData.ContactPersonName;
                                    ProjectLocation = SalesOfferData.ProjectLocation;
                                    ProjectContactPerson = ProjectData.ProjectContactPeople?.FirstOrDefault();
                                    if (ProjectContactPerson != null)
                                    {
                                        CountryName = ProjectContactPerson.Country?.Name;
                                        GovernorateName = ProjectContactPerson.Governorate?.Name;
                                    }
                                    //var MaterialReleaseData = _Context.V_MatrialRelease_SalesOffer.Where(x => x.ProjectID == ProjectID).FirstOrDefault();
                                }
                            }
                            //var ProjectContactPerson = _Context.ProjectContactPersons.Where(x => x.ProjectID == ProjectID).FirstOrDefault();
                            //if (ProjectContactPerson != null)
                            //{
                            //    CountryName = Common.GetCountryName(ProjectContactPerson.CountryID);
                            //}
                            //if (ProjectContactPerson != null)
                            //{
                            //    GovernorateName = Common.GetGovernorateName(ProjectContactPerson.GovernorateID);
                            //}
                            //var CountryName = Common.GetCountryName(ProjectContactPerson.CountryID);



                            if (Client.DefaultDelivaryAndShippingMethodId != null && Client.DefaultDelivaryAndShippingMethodId != 3)
                            {
                                ShippingMethodName = Client.DefaultDelivaryAndShippingMethod.Name;
                            }
                            else
                            {
                                ShippingMethodName = Client.OtherDelivaryAndShippingMethodName;
                            }


                            if (Request.MatrialReleaseDataVM != null)
                            {

                                if (Request.MatrialReleaseDataVM != null && Request.MatrialReleaseDataVM.Count() > 0)
                                {
                                    var MatrialReleaseDataByProject = Request.MatrialReleaseDataVM.Where(x => x.ProjectID == ProjectID).FirstOrDefault();

                                    if (MatrialReleaseDataByProject != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(MatrialReleaseDataByProject.mainContactMobile))
                                            ContactPersonMobile = MatrialReleaseDataByProject.mainContactMobile;

                                        if (!string.IsNullOrWhiteSpace(MatrialReleaseDataByProject.mainContactName))
                                            ContactPersonName = MatrialReleaseDataByProject.mainContactName;

                                        if (!string.IsNullOrWhiteSpace(MatrialReleaseDataByProject.mainAddress))
                                        {
                                            ProjectLocation = MatrialReleaseDataByProject.mainAddress;
                                        }
                                        else
                                        {

                                            ProjectLocation = CountryName + GovernorateName + ProjectLocation;
                                        }

                                        NewShippingMethodName = MatrialReleaseDataByProject.shippingMethod != null ? MatrialReleaseDataByProject.shippingMethod : ShippingMethodName;
                                    }
                                }
                            }


                            if (MatrialReleaseInfoList != null)
                            {
                                foreach (var item in MatrialReleaseInfoList)
                                {
                                    DataRow NewRow;
                                    NewRow = dt.NewRow();


                                    NewRow["No"] = Counter.ToString();
                                    NewRow["Inv. Code"] = item.ItemCode != null ? item.ItemCode : "-";
                                    NewRow["Material Name"] = item.ItemName != null ? item.ItemName.Trim() : "-";
                                    NewRow["RUOM"] = item.UOM != null ? item.UOM : "-";
                                    NewRow["Approved QTY (Rec)"] = item.RecivedQTY;

                                    if (filters.QtyDetails == true)
                                    {
                                        NewRow["Qty Details"] = "T.Rec " + item.RecivedQTY + "\n" + "From " + item.ReqQTY + "\n" + "(R.Rem) " + item.RemQTY;
                                    }
                                    if (filters.FabOrder == true)
                                    {
                                        NewRow["FabOrder"] = item.FabOrderName != null ? item.FabOrderName : "-";

                                    }
                                    if (Serial == true)
                                    {
                                        NewRow["Serial / Batch"] = item.Serial != null ? item.Serial : "-";
                                    }
                                    if (Comment == true)
                                    {
                                        NewRow["Comment"] = item.Comment != null ? "Add Comment:" + " " + item.Comment + "\n" + "M. Req:" + " " + item.NewComment + "\n" + "R. Comment:" + " " + item.ParentItemComment : "-";
                                    }

                                    //dt.Rows.Add(
                                    //    Counter.ToString(),
                                    //    item.ItemCode != null ? item.ItemCode : "-",
                                    //    item.ItemName != null ? item.ItemName : "-",
                                    //    item.UOM != null ? item.UOM : "-",
                                    //    item.RecivedQTY

                                    //    );

                                    //if (QtyDetails == true)
                                    //{

                                    //    dt.Rows.Add( "T.Rec " + item.RecivedQTY + "\n" + "From " + item.ReqQTY + "\n" + "(R.Rem) " + item.RemQTY);

                                    //}
                                    //if (FabOrder == true)
                                    //{
                                    //    dt.Rows.Add(item.FabOrderName != null ? item.FabOrderName : "-");

                                    //}
                                    //if (Serial == true)
                                    //{
                                    //    dt.Rows.Add(item.Serial != null ? item.Serial : "-");

                                    //}
                                    //if (Comment == true)
                                    //{
                                    //    dt.Rows.Add(item.Comment != null ? "Add Comment:" + " " + item.Comment + "\n" + "M. Req:" + " " + item.NewComment + "\n" + "R. Comment:" + " " + item.ParentItemComment : "-");

                                    //}
                                    dt.Rows.Add(NewRow);
                                    Counter++;

                                }
                            }



                            var SumQTY = MatrialReleaseInfoList.Sum(x => x.RecivedQTY);


                            //Adding paragraph for report generated by  
                            Paragraph prgGeneratedBY = new Paragraph();
                            BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                            prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                            document.Add(prgGeneratedBY);


                            PdfPTable tableHeading = new PdfPTable(4);

                            tableHeading.WidthPercentage = 100;

                            tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                            PdfPCell cell3 = new PdfPCell();
                            string cell3text = "";
                            //string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                            cell3.AddElement(iTextSharp.text.Image.GetInstance(PDFp_strPath));
                            cell3.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cell3.BorderColor = new BaseColor(4, 189, 189);
                            cell3.PaddingBottom = 15;
                            cell3.PaddingTop = 15;
                            cell3.BackgroundColor = new BaseColor(4, 189, 189);

                            PdfPCell cell4 = new PdfPCell();
                            string cell4text = MatrialReleaseNO_TXT + filters.MatrialReleaseOrderID;
                            cell4.Phrase = new Phrase(cell4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            cell4.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell4.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cell4.BorderColor = new BaseColor(4, 189, 189);
                            cell4.PaddingBottom = 15;
                            cell4.PaddingTop = 15;
                            cell4.BackgroundColor = new BaseColor(4, 189, 189);



                            tableHeading.AddCell(cell3);
                            tableHeading.AddCell(cell4);
                            tableHeading.AddCell(cell3);
                            tableHeading.AddCell(cell3);

                            tableHeading.KeepTogether = true;



                            PdfPTable table2 = new PdfPTable(4);

                            table2.WidthPercentage = 100;

                            table2.SetTotalWidth(new float[] { 100, 340, 150, 150 });


                            PdfPCell cellA = new PdfPCell();
                            string cellAtext = ProjectName_TXT;
                            cellA.Phrase = new Phrase(cellAtext, font);
                            cellA.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            cellA.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellA.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellA.BorderColor = BaseColor.WHITE;
                            cellA.PaddingTop = 5;


                            PdfPCell cellB = new PdfPCell();
                            string cellBtext = ProjectName;
                            cellB.Phrase = new Phrase(cellBtext, font);


                            cellB.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            cellB.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellB.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellB.BorderColor = BaseColor.WHITE;
                            cellB.PaddingTop = 5;

                            PdfPCell cellC = new PdfPCell();
                            string cellCtext = ClientName_TXT;
                            cellC.Phrase = new Phrase(cellCtext, font);

                            cellC.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellC.BorderColor = BaseColor.WHITE;
                            cellC.PaddingTop = 5;

                            cellC.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellC.RunDirection = PdfWriter.RUN_DIRECTION_RTL;





                            PdfPCell cellD = new PdfPCell();
                            string cellDtext = SalesOfferData?.Client?.Name;
                            cellD.Phrase = new Phrase(cellDtext, font);
                            cellD.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellD.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellD.BorderColor = BaseColor.WHITE;
                            cellD.PaddingTop = 5;
                            cellD.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            table2.AddCell(cellA);
                            table2.AddCell(cellB);
                            table2.AddCell(cellC);
                            table2.AddCell(cellD);


                            PdfPCell CellE = new PdfPCell();
                            string CellEtext = OfferSerial_TXT;
                            CellE.Phrase = new Phrase(CellEtext, font);
                            CellE.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                            CellE.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellE.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellE.BorderColor = BaseColor.WHITE;
                            CellE.PaddingTop = 5;

                            PdfPCell CellF = new PdfPCell();
                            string CellFtext = "";
                            if (SalesOfferData == null || SalesOfferData.OfferSerial == null)
                            {
                                CellFtext = "N/A";
                            }
                            else
                            {
                                CellFtext = SalesOfferData.OfferSerial;
                            }

                            CellF.Phrase = new Phrase(CellFtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellF.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellF.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellF.BorderColor = BaseColor.WHITE;
                            CellF.PaddingTop = 5;
                            CellF.Bottom = 5;


                            PdfPCell CellG = new PdfPCell();
                            string CellGtext = InvoiceSerial_TXT;
                            CellG.Phrase = new Phrase(CellGtext, font);
                            CellG.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellG.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellG.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellG.BorderColor = BaseColor.WHITE;
                            CellG.PaddingTop = 5;
                            CellG.Bottom = 5;


                            PdfPCell CellH = new PdfPCell();
                            string CellHText = "";
                            if (InvoicesData == null || InvoicesData.Serial == null)
                            {
                                CellHText = "N/A";

                            }
                            else
                            {
                                CellHText = InvoicesData.Serial;
                            }
                            CellH.Phrase = new Phrase(CellHText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellH.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellH.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellH.BorderColor = BaseColor.WHITE;
                            CellH.PaddingTop = 5;
                            CellH.Bottom = 5;

                            table2.AddCell(CellE);
                            table2.AddCell(CellF);
                            table2.AddCell(CellG);
                            table2.AddCell(CellH);





                            PdfPCell CellK = new PdfPCell();
                            string CellKtext = ClientApprovalDate_TXT;
                            CellK.Phrase = new Phrase(CellKtext, font);
                            CellK.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellK.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellK.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellK.BorderColor = BaseColor.WHITE;
                            CellK.PaddingTop = 5;


                            PdfPCell CellQ = new PdfPCell();

                            string CellQtext = SalesOfferData?.ClientApprovalDate?.ToString();
                            CellQ.Phrase = new Phrase(CellQtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontQ = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellQ.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellQ.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellQ.BorderColor = BaseColor.WHITE;
                            CellQ.PaddingTop = 5;
                            CellQ.Bottom = 5;


                            PdfPCell CellL = new PdfPCell();
                            string CellLtext = Fromstore_TXT;
                            CellL.Phrase = new Phrase(CellLtext, font);
                            CellL.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellL.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellL.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellL.BorderColor = BaseColor.WHITE;
                            CellL.PaddingTop = 5;
                            CellL.Bottom = 5;


                            PdfPCell CellM = new PdfPCell();
                            string CellMtext = HeaderMatrialReleaseInfo.FromStore;
                            CellM.Phrase = new Phrase(CellMtext, font);


                            CellM.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                            iTextSharp.text.Font arabicFontM = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellM.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellM.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellM.BorderColor = BaseColor.WHITE;
                            CellM.PaddingTop = 5;
                            CellM.Bottom = 5;

                            table2.AddCell(CellK);
                            table2.AddCell(CellQ);
                            table2.AddCell(CellL);
                            table2.AddCell(CellM);






                            PdfPCell CellN1 = new PdfPCell();
                            string CellN1text = ToUserName_TXT;


                            CellN1.Phrase = new Phrase(CellN1text, font);
                            CellN1.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellN1.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellN1.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellN1.BorderColor = BaseColor.WHITE;
                            CellN1.PaddingTop = 5;


                            PdfPCell CellS12 = new PdfPCell();
                            string CellS12text = HeaderMatrialReleaseInfo.ToUserName;

                            CellS12.Phrase = new Phrase(CellS12text, font);


                            CellS12.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                            iTextSharp.text.Font arabicFontsS = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellS12.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellS12.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellS12.BorderColor = BaseColor.WHITE;
                            CellS12.PaddingTop = 5;
                            CellS12.Bottom = 5;


                            PdfPCell CellW1 = new PdfPCell();
                            string CellW1text = RequestType_TXT;
                            CellW1.Phrase = new Phrase(CellW1text, font);
                            CellW1.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellW1.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellW1.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellW1.BorderColor = BaseColor.WHITE;
                            CellW1.PaddingTop = 5;
                            CellW1.Bottom = 5;


                            PdfPCell CellV12 = new PdfPCell();
                            string CellV12text = HeaderMatrialReleaseInfo.RequestType;
                            CellV12.Phrase = new Phrase(CellV12text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellV12.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellV12.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV12.BorderColor = BaseColor.WHITE;
                            CellV12.PaddingTop = 5;
                            CellV12.Bottom = 5;






                            table2.AddCell(CellN1);
                            table2.AddCell(CellS12);
                            table2.AddCell(CellW1);
                            table2.AddCell(CellV12);



                            PdfPCell CellN11 = new PdfPCell();
                            string CellN11text = CreatedDate_TXT;
                            CellN11.Phrase = new Phrase(CellN11text, font);
                            CellN11.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellN11.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellN11.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellN11.BorderColor = BaseColor.WHITE;
                            CellN11.PaddingTop = 5;


                            PdfPCell CellS121 = new PdfPCell();
                            string CellS121text = "";
                            if (HeaderMatrialReleaseInfo == null || HeaderMatrialReleaseInfo.CreationDate == null)
                            {
                                CellS121text = "N/A";
                            }
                            else
                            {
                                CellS121text = HeaderMatrialReleaseInfo.CreationDate;
                            }
                            CellS121.Phrase = new Phrase(CellS121text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontsS1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellS121.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellS121.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellS121.BorderColor = BaseColor.WHITE;
                            CellS121.PaddingTop = 5;
                            CellS121.Bottom = 5;


                            PdfPCell CellW11 = new PdfPCell();
                            string CellW11text = SalesMan_TXT;
                            CellW11.Phrase = new Phrase(CellW11text, font);
                            CellW11.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellW11.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellW11.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellW11.BorderColor = BaseColor.WHITE;
                            CellW11.PaddingTop = 5;
                            CellW11.Bottom = 5;


                            PdfPCell CellV121 = new PdfPCell();
                            string CellV121text = SalesOfferData?.CreatedByNavigation?.FirstName + " " + SalesOfferData?.CreatedByNavigation?.LastName; //sales Person
                            CellV121.Phrase = new Phrase(CellV121text, font);
                            iTextSharp.text.Font arabicFontV11 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellV121.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV121.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV121.BorderColor = BaseColor.WHITE;
                            CellV121.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellV121.PaddingTop = 5;
                            CellV121.Bottom = 5;





                            table2.AddCell(CellN11);
                            table2.AddCell(CellS121);
                            table2.AddCell(CellW11);
                            table2.AddCell(CellV121);





                            PdfPCell CellReleaseN11 = new PdfPCell();

                            string CellReleaseN11text = Address_TXT;
                            CellReleaseN11.Phrase = new Phrase(CellReleaseN11text, font);
                            CellReleaseN11.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellReleaseN11.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellReleaseN11.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellReleaseN11.BorderColor = BaseColor.WHITE;
                            CellReleaseN11.PaddingTop = 5;
                            CellReleaseN11.Bottom = 5;

                            PdfPCell CellRelease121 = new PdfPCell();

                            string CellReleaseS121text = "";
                            CellReleaseS121text = ProjectLocation;

                            CellRelease121.Phrase = new Phrase(CellReleaseS121text, font);

                            CellRelease121.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellRelease121.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellRelease121.BorderColor = BaseColor.WHITE;
                            CellRelease121.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                            CellRelease121.PaddingTop = 5;
                            CellRelease121.Bottom = 5;




                            PdfPCell CellReleaseW11 = new PdfPCell();

                            string CellReleasetext = ShippingMethod_TXT;
                            CellReleaseW11.Phrase = new Phrase(CellReleasetext, font);
                            CellReleaseW11.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellReleaseW11.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellReleaseW11.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellReleaseW11.BorderColor = BaseColor.WHITE;
                            CellReleaseW11.PaddingTop = 5;






                            PdfPCell CellReleaseV121 = new PdfPCell();
                            string CellReleaseV121text = "";

                            if (NewShippingMethodName != " ")
                            {
                                CellReleaseV121text = NewShippingMethodName;

                            }
                            else
                            {
                                CellReleaseV121text = ShippingMethodName;
                            }


                            CellReleaseV121.Phrase = new Phrase(CellReleaseV121text, font);

                            CellReleaseV121.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellReleaseV121.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellReleaseV121.BorderColor = BaseColor.WHITE;
                            CellReleaseV121.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellReleaseV121.PaddingTop = 5;
                            CellReleaseV121.Bottom = 5;





                            table2.AddCell(CellReleaseW11);
                            table2.AddCell(CellReleaseV121);
                            table2.AddCell(CellReleaseN11);
                            table2.AddCell(CellRelease121);








                            PdfPCell CellReleaseN11A = new PdfPCell();
                            string CellReleasetextA = ContactName_TXT;
                            CellReleaseN11A.Phrase = new Phrase(CellReleasetextA, font);
                            CellReleaseN11A.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellReleaseN11A.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellReleaseN11A.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellReleaseN11A.BorderColor = BaseColor.WHITE;
                            CellReleaseN11A.PaddingTop = 5;


                            PdfPCell CellRelease121A = new PdfPCell();
                            string CellReleaseS121textA = "";

                            CellReleaseS121textA = ContactPersonName;




                            CellRelease121A.Phrase = new Phrase(CellReleaseS121textA, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontsS1sssA = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellRelease121A.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellRelease121A.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellRelease121A.BorderColor = BaseColor.WHITE;
                            CellRelease121A.PaddingTop = 5;
                            CellRelease121A.Bottom = 5;


                            PdfPCell CellReleaseW11A = new PdfPCell();
                            string CellReleaseW11textA = ContactMobile_TXT;
                            CellReleaseW11A.Phrase = new Phrase(CellReleaseW11textA, font);
                            CellReleaseW11A.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellReleaseW11A.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellReleaseW11A.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellReleaseW11A.BorderColor = BaseColor.WHITE;
                            CellReleaseW11A.PaddingTop = 5;
                            CellReleaseW11A.Bottom = 5;


                            PdfPCell CellReleaseV121A = new PdfPCell();
                            string CellReleaseV121textA = "";

                            CellReleaseV121textA = ContactPersonMobile;

                            CellReleaseV121A.Phrase = new Phrase(CellReleaseV121textA, font);
                            iTextSharp.text.Font arabicFontV11sssA = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellReleaseV121A.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellReleaseV121A.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellReleaseV121A.BorderColor = BaseColor.WHITE;
                            CellReleaseV121A.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CellReleaseV121A.PaddingTop = 5;
                            CellReleaseV121A.Bottom = 5;





                            table2.AddCell(CellReleaseN11A);
                            table2.AddCell(CellRelease121A);
                            table2.AddCell(CellReleaseW11A);
                            table2.AddCell(CellReleaseV121A);













                            table2.SpacingAfter = 20;












                            PdfPTable table4 = new PdfPTable(4);

                            table4.WidthPercentage = 100;

                            table4.SetTotalWidth(new float[] { 5, 100, 200, 5 });



                            //Adding PdfPTable


                            PdfPTable table = new PdfPTable(dt.Columns.Count);


                            table.WidthPercentage = 100;

                            if (dt.Columns.Count == 5)
                            {
                                table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60 });

                            }
                            if (dt.Columns.Count == 6)
                            {

                                if (filters.QtyDetails == true)
                                {
                                    table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 100 });
                                }
                                if (filters.FabOrder == true)
                                {
                                    table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 100 });
                                }
                                if (Serial == true)
                                {
                                    table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 100 });
                                }
                                if (Comment == true)
                                {
                                    table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 140 });
                                }


                            }

                            if (dt.Columns.Count == 7)
                            {


                                table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 100, 80 });


                            }
                            if (dt.Columns.Count == 8)
                            {
                                table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 100, 80, 100 });


                            }
                            if (dt.Columns.Count == 9)
                            {
                                table.SetTotalWidth(new float[] { 20, 80, 120, 35, 60, 100, 80, 100, 150 });


                            }








                            //table Width
                            table.WidthPercentage = 100;

                            //Define Sizes of Cloumns

                            table.SpacingAfter = 20;

                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                                PdfPCell cell = new PdfPCell();
                                cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                                iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));

                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.VerticalAlignment = Element.ALIGN_MIDDLE;

                                cell.BorderColor = BaseColor.BLACK;

                                //cell.Width = 20;

                                cell.PaddingBottom = 5;
                                table.AddCell(cell);


                            }


                            //writing table Data
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    string text = dt.Rows[i][j].ToString();

                                    PdfPCell cell = new PdfPCell();
                                    cell.BorderColor = BaseColor.BLACK;
                                    cell.ArabicOptions = 1;
                                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                    //     cell.Width = 20;
                                    //if (j <= 2)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    //    cell.PaddingTop = 4;
                                    // //   cell.Width = 10;
                                    //}
                                    //cell.ArabicOptions = 1;
                                    //if (j == 3)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    //    cell.PaddingTop = 4;
                                    //}
                                    //if (j == 4)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    //    cell.PaddingTop = 4;
                                    //}
                                    //if (j == 5)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    //    cell.PaddingTop = 4;
                                    //}
                                    //if (j == 7)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    //    cell.PaddingTop = 4;
                                    //}
                                    //if (j == 8)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                    //    cell.PaddingTop = 4;
                                    //}

                                    //if (cell.ArabicOptions == 1)
                                    //{
                                    //    cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                    //    cell.PaddingTop = 4;

                                    //}
                                    //else
                                    //{
                                    //    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                    //    cell.PaddingTop = 4;


                                    //string text = dt.Rows[i][j].ToString();
                                   // ct.SetSimpleColumn(new Phrase("مرحبا بالعالم", font), 50, 800, 550, 50, 20, Element.ALIGN_RIGHT);
                                    cell.Phrase = new Phrase(text, font); 
                                   // new Phrase( dt.Rows[i][j].ToString(), arabicFont_TIMES_ROMAN);
                                    //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                                    table.AddCell(cell);

                                }

                            }



                            PdfPTable FooterPart1 = new PdfPTable(4);

                            FooterPart1.WidthPercentage = 100;

                            FooterPart1.SetTotalWidth(new float[] { 65, 80, 200, 60 });



                            PdfPCell cellA00 = new PdfPCell();
                            string cellAtext00 = "";
                            cellA00.Phrase = new Phrase(cellAtext00, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont500 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            cellA00.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellA00.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellA00.BorderColor = BaseColor.WHITE;
                            //cellA00.PaddingTop = 5;


                            PdfPCell cellB00 = new PdfPCell();
                            string cellBtext00 = TotalQty_TXT;
                            cellB00.Phrase = new Phrase(cellBtext00, font);
                            cellB00.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                            cellB00.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellB00.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellB00.BorderColor = BaseColor.WHITE;
                            //cellB00.PaddingTop = 5;








                            PdfPCell cellC00 = new PdfPCell();
                            string cellCtext00 = SumQTY.ToString();
                            cellC00.Phrase = new Phrase(cellCtext00, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont600 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            cellC00.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellC00.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellC00.BorderColor = BaseColor.WHITE;
                            //cellC00.PaddingTop = 5;


                            PdfPCell cellD00 = new PdfPCell();
                            string cellDtext0 = "";
                            cellD00.Phrase = new Phrase(cellDtext0, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            cellD00.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellD00.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellD00.BorderColor = BaseColor.WHITE;
                            //cellD00.PaddingTop = 5;
                            cellD00.RunDirection = PdfWriter.RUN_DIRECTION_RTL;



                            FooterPart1.AddCell(cellA00);
                            FooterPart1.AddCell(cellB00);
                            FooterPart1.AddCell(cellC00);
                            FooterPart1.AddCell(cellD00);







                            PdfPTable FooterPart = new PdfPTable(4);

                            FooterPart.WidthPercentage = 100;

                            FooterPart.SetTotalWidth(new float[] { 100, 450, 100, 100 });





                            PdfPCell CellWhiteL = new PdfPCell();
                            CellWhiteL.Phrase = new Phrase(" ", font);
                            CellWhiteL.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellWhiteL.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellWhiteL.BorderColor = BaseColor.WHITE;
                            CellWhiteL.PaddingTop = 5;
                            CellWhiteL.Bottom = 5;


                            PdfPCell CellWhiteR = new PdfPCell();
                            CellWhiteR.Phrase = new Phrase(" ", font);
                            CellWhiteR.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellWhiteR.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellWhiteR.BorderColor = BaseColor.WHITE;
                            CellWhiteR.PaddingTop = 5;
                            CellWhiteR.Bottom = 5;



                            PdfPCell cellA0 = new PdfPCell();
                            cellA0.Phrase = new Phrase(Reciever_TXT, font);
                            cellA0.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            cellA0.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellA0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellA0.BorderColor = BaseColor.WHITE;
                            cellA0.PaddingTop = 5;
                            cellA0.Bottom = 5;


                            PdfPCell cellB0 = new PdfPCell();
                            string cellBtext0 = "...................";
                            cellB0.Phrase = new Phrase(cellBtext0, font);
                            cellB0.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            cellB0.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellB0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellB0.BorderColor = BaseColor.WHITE;
                            cellB0.PaddingTop = 5;



                            PdfPCell cellD0 = new PdfPCell();
                            cellD0.Phrase = new Phrase(StoreKeeper_TXT, font);
                            cellD0.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                            cellD0.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellD0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellD0.BorderColor = BaseColor.WHITE;
                            cellD0.PaddingTop = 5;
                            cellD0.Bottom = 5;



                            PdfPCell cellC0 = new PdfPCell();
                            string cellCtext0 = "....................";
                            cellC0.Phrase = new Phrase(cellCtext0, font);
                            cellC0.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            cellC0.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellC0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellC0.BorderColor = BaseColor.WHITE;
                            cellC0.PaddingTop = 5;






                            PdfPCell GeralComment = new PdfPCell();
                            GeralComment.Phrase = new Phrase(GeneralComment_TXT, font);
                            GeralComment.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            GeralComment.HorizontalAlignment = Element.ALIGN_LEFT;
                            GeralComment.VerticalAlignment = Element.ALIGN_MIDDLE;
                            GeralComment.BorderColor = BaseColor.WHITE;
                            GeralComment.PaddingTop = 5;


                            PdfPCell CommentValue = new PdfPCell();
                            string CommentValuetext = Request.MatrialReleaseDataVM.FirstOrDefault()?.Comment;
                            CommentValue.Phrase = new Phrase(CommentValuetext, font);

                            CommentValue.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                            CommentValue.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CommentValue.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CommentValue.BorderColor = BaseColor.WHITE;
                            CommentValue.PaddingTop = 5;



                            FooterPart.AddCell(GeralComment);
                            FooterPart.AddCell(CommentValue);
                            FooterPart.AddCell(CellWhiteL);
                            FooterPart.AddCell(CellWhiteR);
                            FooterPart.AddCell(CellWhiteL);
                            FooterPart.AddCell(CellWhiteR);
                            FooterPart.AddCell(cellA0);
                            FooterPart.AddCell(cellB0);
                            FooterPart.AddCell(CellWhiteL);
                            FooterPart.AddCell(CellWhiteR);
                            FooterPart.AddCell(cellD0);
                            FooterPart.AddCell(cellC0);


                            PdfPCell CellE0 = new PdfPCell();
                            string CellEtext0 = Date_TXT;
                            CellE0.Phrase = new Phrase(CellEtext0, font);
                            CellE0.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                            CellE0.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellE0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellE0.BorderColor = BaseColor.WHITE;
                            CellE0.PaddingTop = 10;


                            PdfPCell CellF0 = new PdfPCell();
                            string CellFtext0 = "";
                            CellF0.Phrase = new Phrase(CellFtext0, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont4s000 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellF0.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellF0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellF0.BorderColor = BaseColor.WHITE;
                            CellF0.PaddingTop = 5;
                            CellF0.Bottom = 5;


                            PdfPCell CellG0 = new PdfPCell();
                            string CellGtext0 = "";
                            CellG0.Phrase = new Phrase(CellGtext0, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontt0 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellG0.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellG0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellG0.BorderColor = BaseColor.WHITE;
                            CellG0.PaddingTop = 5;
                            CellG0.Bottom = 5;


                            PdfPCell CellH0 = new PdfPCell();
                            string CellHText0 = "";

                            CellH0.Phrase = new Phrase(CellHText0, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont780 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellH0.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellH0.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellH0.BorderColor = BaseColor.WHITE;
                            CellH0.PaddingTop = 5;
                            CellH0.Bottom = 5;

                            FooterPart.AddCell(CellF0);
                            FooterPart.AddCell(CellG0);
                            FooterPart.AddCell(CellE0);

                            document.Add(tableHeading);
                            document.Add(table2);
                            document.Add(table4);
                            document.Add(table);



                            document.Add(FooterPart1);
                            document.Add(FooterPart);


                            document.NewPage();

                        }
                    }



                    //Start PDF Service
                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;


                    var CompanyName = validation.CompanyName.ToLower();

                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "MaterialReleaseReport.pdf";
                    string PathsTR = "/Attachments/" + CompanyName + "/";
                    String Filepath = _host.WebRootPath + "/" + PathsTR;
                    string p_strPath = Filepath + "/" + FullFileName;
                    if (!System.IO.File.Exists(p_strPath))
                    {
                        var objFileStrm = System.IO.File.Create(p_strPath);
                        objFileStrm.Close();
                    }
                    File.WriteAllBytes(p_strPath, result);

                    Response.Message = Globals.baseURL + PathsTR + FullFileName;


                }



            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
            }
            return Response;
        }

        public InventoryItemMatrialReleaseInfoResponse GetInventoryItemForCreateMatrialRelease(long MatrialRequestID, long UserID)
        {
            InventoryItemMatrialReleaseInfoResponse Response = new InventoryItemMatrialReleaseInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                var InventoryItemMatrialReleaseInfoObj = new InventoryItemMatrialReleaseInfo();
                var MatrialReleaseInfList = new List<MatrialReleaseItemInfo>();
                if (Response.Result)
                {
                    if (MatrialRequestID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err109";
                        error.ErrorMSG = "Invalid Matrial Request Order ID";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Response.Result)
                    {


                        var MatrialRequestOBJDB = _unitOfWork.VInventoryMatrialRequests.Find(x => x.Id == MatrialRequestID);

                        if (MatrialRequestOBJDB != null)
                        {
                            if (!CheckISInventoryKeeper(MatrialRequestOBJDB.ToInventoryStoreId, UserID))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err309";
                                error.ErrorMSG = "This User is not store keeper";
                                Response.Errors.Add(error);
                                return Response;
                            }

                            if (MatrialRequestOBJDB.Status == "Open" || MatrialRequestOBJDB.Status == "Hold")
                            {

                                var MatrialRequestItemList = _Context.VInventoryMatrialRequestItems.Where(x => x.InventoryMatrialRequestId == MatrialRequestOBJDB.Id).ToList();
                                //var IDSInvMatrialRequestItems = MatrialRequestItemList.Select(x => new InventoryStoreIdAndItemId { InventoryItemID = x.InventoryItemID, ToInventoryStoreID = x.ToInventoryStoreID }).ToList();
                                //var InventoryStoreItemList = _Context.InventoryStoreItems.Where(x => IDSInvMatrialRequestItems.Any(z => z.InventoryItemID == x.InventoryItemID && z.ToInventoryStoreID == x.InventoryStoreID)).ToList();
                                var IDSInvstoreList = MatrialRequestItemList.Select(x => x.ToInventoryStoreId).ToList();
                                var IDSInvItemList = MatrialRequestItemList.Select(x => x.InventoryItemId).ToList();
                                var InventoryStoreItemList = _Context.InventoryStoreItems.Where(x => IDSInvstoreList.Any(storID => storID == x.InventoryStoreId) &&
                                IDSInvItemList.Any(InventoryItemID => InventoryItemID == x.InventoryItemId)).ToList();
                                bool found = false;

                                //requestType = order.s_RequestType;
                                var InventoryMatrialRelease = _unitOfWork.VInventoryMatrialReleases.Find(x => x.MatrialRequestId == MatrialRequestOBJDB.Id /*&& x.Status == "Open"*/ && x.Active == true);
                                if (InventoryMatrialRelease != null)
                                {
                                    //found = true;
                                    InventoryItemMatrialReleaseInfoObj.ReNew = false;
                                    InventoryItemMatrialReleaseInfoObj.ID = InventoryMatrialRelease.Id;
                                    InventoryItemMatrialReleaseInfoObj.InventoryMatrialRequestOrderID = MatrialRequestOBJDB.Id;
                                    InventoryItemMatrialReleaseInfoObj.ToUserName = InventoryMatrialRelease.ToUserName;
                                    InventoryItemMatrialReleaseInfoObj.FromStore = InventoryMatrialRelease.FromInventoryStoreName;
                                    InventoryItemMatrialReleaseInfoObj.FromStoreId = InventoryMatrialRelease.FromInventoryStoreId;
                                    InventoryItemMatrialReleaseInfoObj.RequestDate = InventoryMatrialRelease.RequestDate.ToShortDateString();
                                    // Get List Of Items
                                    var MatrialReleaseItemListDB = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => x.InventoryMatrialReleasetId == InventoryMatrialRelease.Id).ToList();
                                    if (MatrialReleaseItemListDB.Count() > 0)
                                    {
                                        foreach (var item in MatrialReleaseItemListDB)
                                        {
                                            string ClientName = !string.IsNullOrEmpty(item.ClientName) ? " - " + item.ClientName : "";

                                            //var InventoryStoreItem = _Context.InventoryStoreItems.Where(x => x.OrderID == InventoryMatrialRelease.ID && x.addingOrderItemId == item.ID).FirstOrDefault();
                                            var ItemObjDB = new MatrialReleaseItemInfo();
                                            ItemObjDB.ID = item.Id;
                                            ItemObjDB.MatrialRequestItemID = item.InventoryMatrialRequestItemId;
                                            ItemObjDB.InventoryItemID = item.InventoryItemId;
                                            ItemObjDB.ItemCode = item.ItemCode;
                                            ItemObjDB.ItemName = item.ItemName;
                                            ItemObjDB.Comment = item.Comments;
                                            ItemObjDB.UOM = item.UomshortName;
                                            ItemObjDB.FabOrderName = item.FabNumber.ToString();
                                            ItemObjDB.ProjectName = item.ProjectName + ClientName;
                                            ItemObjDB.NewComment = item.NewComments;
                                            ItemObjDB.NewRecivedQTY = item.NewRecivedQuantity;
                                            //if (InventoryStoreItem != null)
                                            //{
                                            //    ItemObjDB.StoreLocationID = InventoryStoreItem.InvenoryStoreLocationID;
                                            //    ItemObjDB.StoreLocationName = InventoryStoreItem.InventoryStoreLocation != null ? InventoryStoreItem.InventoryStoreLocation.Location : "";
                                            //}

                                            ItemObjDB.RecivedQTY = item.RecivedQuantity;
                                            ItemObjDB.ReqQTY = item.ReqQuantity;
                                            ItemObjDB.RemQTY = item.ReqQuantity - item.RecivedQuantity;

                                            decimal balance = GetInventoryStoreItemBalance(InventoryStoreItemList, InventoryMatrialRelease.FromInventoryStoreId, item.InventoryItemId);
                                            ItemObjDB.StockQTY = balance;

                                            MatrialReleaseInfList.Add(ItemObjDB);
                                        }
                                    }

                                }

                                if (!found)
                                {

                                    InventoryItemMatrialReleaseInfoObj.ReNew = true;
                                    InventoryItemMatrialReleaseInfoObj.ID = 0;
                                    InventoryItemMatrialReleaseInfoObj.InventoryMatrialRequestOrderID = MatrialRequestOBJDB.Id;
                                    InventoryItemMatrialReleaseInfoObj.ToUserName = MatrialRequestOBJDB.FromUserName;
                                    InventoryItemMatrialReleaseInfoObj.FromStore = MatrialRequestOBJDB.ToInventoryStoreName;
                                    InventoryItemMatrialReleaseInfoObj.FromStoreId = MatrialRequestOBJDB.ToInventoryStoreId;
                                    InventoryItemMatrialReleaseInfoObj.RequestDate = MatrialRequestOBJDB.RequestDate.ToShortDateString();
                                    // Get List Of Items
                                    var MatrialRequestItemListDB = MatrialRequestItemList.Where(x => x.RecivedQuantity < x.ReqQuantity).ToList();
                                    var IDSInvStoreItemList = MatrialRequestItemListDB.Select(x => x.InventoryItemId).ToList();
                                    var ListOfInventoryStoreItemsListDB = _Context.InventoryStoreItems.Where(x => IDSInvStoreItemList.Contains(x.InventoryItemId)).ToList();
                                    if (MatrialRequestItemListDB.Count() > 0)
                                    {
                                        foreach (var item in MatrialRequestItemListDB)
                                        {
                                            var ItemObjDB = new MatrialReleaseItemInfo();
                                            ItemObjDB.ID = 0;
                                            ItemObjDB.MatrialRequestItemID = item.Id;
                                            ItemObjDB.InventoryItemID = item.InventoryItemId;
                                            ItemObjDB.ItemCode = item.ItemCode;
                                            ItemObjDB.ItemName = item.ItemName;
                                            ItemObjDB.Comment = item.Comments;
                                            ItemObjDB.UOM = item.UomshortName;
                                            ItemObjDB.FabOrderName = item.FabNumber.ToString();
                                            ItemObjDB.ProjectName = item.ProjectName;
                                            ItemObjDB.NewComment = ""; //item.NewComments;
                                            ItemObjDB.NewRecivedQTY = 0; // item.NewRecivedQuantity;

                                            ItemObjDB.RecivedQTY = item.RecivedQuantity;
                                            ItemObjDB.ReqQTY = item.ReqQuantity;
                                            ItemObjDB.RemQTY = item.ReqQuantity - item.RecivedQuantity;

                                            decimal balance = GetInventoryStoreItemBalance(InventoryStoreItemList, MatrialRequestOBJDB.ToInventoryStoreId, item.InventoryItemId);
                                            ItemObjDB.StockQTY = balance;
                                            if (item.MaterialRequestStatus == "Hold" || item.MaterialRequestStatus == "Hold Released")
                                            {
                                                // Get List Hold 
                                                ItemObjDB.InventoryStoreItemIDsList = ListOfInventoryStoreItemsListDB.Where(x => x.HoldQty != null && x.InventoryItemId == item.InventoryItemId && x.HoldQty > 0 && x.InventoryStoreId == item.ToInventoryStoreId).Select(x => x.Id).ToList();
                                            }
                                            MatrialReleaseInfList.Add(ItemObjDB);

                                        }
                                    }


                                }

                                ///////////////////////
                            }
                            else
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err460";
                                error.ErrorMSG = "the Request was already " + MatrialRequestOBJDB.Status;
                                Response.Errors.Add(error);
                                return Response;
                            }


                        }

                        InventoryItemMatrialReleaseInfoObj.MatrialReleaseInfoList = MatrialReleaseInfList;

                    }
                    Response.InventoryItemInfo = InventoryItemMatrialReleaseInfoObj;


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

        public bool CheckISInventoryKeeper(int InventoryStoreID, long userID)
        {
            bool result = false;
            var InventoryStoreKeeperDB = _unitOfWork.VInventoryStoreKeepers.Find(x => x.InventoryStoreId == InventoryStoreID && x.UserId == userID && x.Active == true && x.StoreActive == true);
            if (InventoryStoreKeeperDB != null)
            {
                result = true;
            }
            return result;
        }

        public  decimal GetInventoryStoreItemBalance(List<InventoryStoreItem> InventoryStoreItemList, int? InventoryStoreID, long? inventoryItemID)
        {
            decimal result = 0;
            if (InventoryStoreID != null && InventoryStoreID != 0 && inventoryItemID != null && inventoryItemID != 0)
            {
                result = (decimal)(InventoryStoreItemList.Where(x => x.InventoryStoreId == InventoryStoreID && x.InventoryItemId == inventoryItemID).Select(x => x.Balance1).DefaultIfEmpty(0).Sum());
                //result = _Context.proc_InventoryStoreItemLoadAll().Where(x => x.InventoryStoreID == InventoryStoreID && x.InventoryItemID == inventoryItemID).GroupBy(x => x.InventoryItemID).Select(x => x.Sum(a => a.Balance)).FirstOrDefault();
            }
            //InventoryItem item = new InventoryItem();
            //if (item.LoadByPrimaryKey(inventoryItemID))
            //{
            //    GarasERP.InventoryStoreItem StItem = new GarasERP.InventoryStoreItem();
            //    StItem.Where.InventoryStoreID.Value = InventoryStoreID;
            //    StItem.Where.InventoryItemID.Value = inventoryItemID;
            //    StItem.Query.AddGroupBy(GarasERP.InventoryStoreItem.ColumnNames.InventoryItemID);
            //    StItem.Aggregate.Balance.Function = MyGeneration.dOOdads.AggregateParameter.Func.Sum;
            //    if (StItem.Query.Load())
            //    {
            //        if (StItem.DefaultView != null && StItem.DefaultView.Count > 0)
            //        {
            //            if (StItem.s_Balance != "" && item.s_ExchangeFactor != "")
            //            {
            //                result = StItem.Balance /** decimal.Parse(item.ExchangeFactor.ToString())*/;
            //            }
            //        }
            //    }
            //}
            return result;
        }

        public MatrialReleaseDDLResponse GetMatrialReleaseItemList(GetMatrialReleaseItemListFilters filters)
        {
            MatrialReleaseDDLResponse Response = new MatrialReleaseDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<MatrialReleaseItemDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.VInventoryMatrialReleaseItems.FindAllQueryable(a=>true);
                    var InventoryStoreItem = _unitOfWork.InventoryStoreItems.GetAll();
                    if (filters.ProjectID != 0)
                    {
                        ListDB = ListDB.Where(x => x.ProjectId == filters.ProjectID).AsQueryable();
                    }

                    if (filters.UserID != 0)
                    {
                        ListDB = ListDB.Where(x => x.FromUserId == filters.UserID).AsQueryable();
                    }
                    if (filters.InventoryItemID != 0)
                    {
                        ListDB = ListDB.Where(x => x.InventoryItemId == filters.InventoryItemID).AsQueryable();
                    }

                    if (filters.StoreID != 0)
                    {
                        var IDSMatrialRelease = _unitOfWork.InventoryMatrialReleases.FindAll(x => x.FromInventoryStoreId == filters.StoreID).Select(x => x.Id).ToList();
                        ListDB = ListDB.Where(x => IDSMatrialRelease.Contains(x.InventoryMatrialReleasetId)).AsQueryable();
                    }
                    if (filters.StoreLocationID != 0)
                    {
                        var IDSMatrialRelease = InventoryStoreItem.Where(x => x.InvenoryStoreLocationId == filters.StoreLocationID).Select(x => x.OrderId).ToList();
                        ListDB = ListDB.Where(x => IDSMatrialRelease.Contains(x.InventoryMatrialReleasetId)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.Searchkey))
                    {
                        var search = HttpUtility.UrlDecode(filters.Searchkey);
                        ListDB = ListDB.Where(a=>a.ItemName.Contains(search));
                    }
                    var MatrialReleaseItemListDB = PagedList<VInventoryMatrialReleaseItem>.Create(ListDB, filters.CurrentPage, filters.NumberOfItemsPerPage);
                    var IDSMateialRelaseItemsList = ListDB.Select(x => x.InventoryMatrialReleasetId).ToList();
                    var InventoryStoreItemList = InventoryStoreItem.Where(a => IDSMateialRelaseItemsList.Contains(a.OrderId)).ToList();
                    if (MatrialReleaseItemListDB.Count > 0)
                    {
                        foreach (var item in MatrialReleaseItemListDB)
                        {
                            var InventoryStoreItemObj = InventoryStoreItemList.Where(x => x.AddingOrderItemId == item.Id).FirstOrDefault();
                            var DLLObj = new MatrialReleaseItemDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.MatrialReleaseNo = item.InventoryMatrialReleasetId;
                            if (InventoryStoreItemObj != null)
                            {

                                DLLObj.ExpDate = InventoryStoreItemObj.ExpDate != null ? ((DateTime)InventoryStoreItemObj.ExpDate).ToShortDateString() : "";
                                DLLObj.Serial = InventoryStoreItemObj.ItemSerial;
                            }
                            DLLObj.ItemName = item.ItemName != null ? item.ItemName.Trim() : "";
                            DLLObj.ItemCode = item.ItemCode;
                            DLLObj.QtyReleased = item.RecivedQuantity != null ? (decimal)item.RecivedQuantity : 0;
                            DLLObj.ProjectName = item.ProjectName;
                            DLLObj.FabNo = item.FabNumber != null ? ((int)item.FabNumber).ToString() : "";

                            DDLList.Add(DLLObj);
                        }
                    }

                    Response.PaginationHeader = new PaginationHeader()
                    {
                        CurrentPage = MatrialReleaseItemListDB.CurrentPage,
                        TotalPages = MatrialReleaseItemListDB.TotalPages,
                        ItemsPerPage = MatrialReleaseItemListDB.PageSize,
                        TotalItems = MatrialReleaseItemListDB.TotalCount
                    };
                }
                Response.DDLList = DDLList.Distinct().ToList();
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
