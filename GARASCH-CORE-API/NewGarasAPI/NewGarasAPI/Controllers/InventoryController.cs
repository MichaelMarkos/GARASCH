using Azure;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Helper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Models.AccountAndFinance;
using NewGaras.Infrastructure.Models.Common;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using NewGaras.Infrastructure.Models.InventoryItem;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.Inventory;
using NewGarasAPI.Models.Inventory.Requests;
using Org.BouncyCastle.Asn1.Cmp;
using System.Data;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using static NewGaras.Domain.Helper.TreeStructure;
using Common = NewGaras.Infrastructure.Common;
using Common2 = NewGaras.Domain.Helper.Common;
using TreeViewDto = NewGarasAPI.Models.Admin.TreeViewDto;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;
using NewGaras.Infrastructure.Models.InventoryItem.Filters;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Admin;

namespace NewGarasAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private GarasTestContext _Context;
        private Helper.Helper _helper;
        static string key;
        private readonly IWebHostEnvironment _host;
        private readonly ITenantService _tenantService;
        private readonly IInventoryItemService _inventoryService;
        private readonly IInventoryItemService _inventorytemService;
        public InventoryController(IWebHostEnvironment host, ITenantService tenantService, IInventoryItemService inventoryService, IInventoryItemService inventorytemService)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _helper = new Helper.Helper();
            key = "SalesGarasPass";
            _host = host;
            _inventoryService = inventoryService;
            _inventorytemService = inventorytemService;
        }

        [HttpGet("GetParentIdWithHaveItemTrue")]
        public async Task<ParentContentIdsResponse> GetParentIdWithHaveItemTrue()
        {
            try
            {
                var parentContentInfos = await _Context.InventoryItemContents
                    .Where(x => x.Haveitem == true && x.ParentContentId != null)
                    .Select(x => new ParentContentInfo { ParentContentId = x.ParentContentId.Value, ChapterName = x.ChapterName })
                    .Distinct()
                    .ToListAsync();

                return new ParentContentIdsResponse { ParentContentInfos = parentContentInfos };
            }
            catch (Exception ex)
            {
                Response.StatusCode = 500;
                return new ParentContentIdsResponse { ParentContentInfos = new List<ParentContentInfo>(), ErrorMessage = $"An error occurred: {ex.Message}" };
            }
        }


        [HttpGet("GetInventoryItemContentTree")] // transfered to inventoryItem Service
        public async Task<GetInventoryItemContentTreeResponse> GetInventoryItemContentTree([FromHeader] long? InventoryItemId)
        {
            GetInventoryItemContentTreeResponse Response = new GetInventoryItemContentTreeResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var InventoryItemContent = await _Context.InventoryItemContents.ToListAsync();
                    if (InventoryItemId != null)
                    {
                        InventoryItemContent = InventoryItemContent.Where(a => a.InventoryItemId == InventoryItemId).ToList();
                    }
                    var TreeDtoObj = InventoryItemContent.Select(c => new TreeViewDto2
                    {
                        id = c.Id.ToString(),
                        title = c.ChapterName,
                        HasChild = c.Haveitem,
                        parentId = c.ParentContentId.ToString()
                    }).ToList();

                    var trees = Common.BuildTreeViews2("", TreeDtoObj);

                    Response.GetInventoryItemCategoryList = trees;
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);

                return Response;
            }

        }

        [HttpPost("DeleteInventoryItemContent")] // transfered to inventoryItem Service
        public BaseResponseWithId<long> DeleteInventoryItemContent([FromHeader] long InventoryItemContentId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;
                if (response.Result)
                {
                    if (InventoryItemContentId == 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Inventory Item Content Id IS required";
                        response.Errors.Add(error);
                        return response;
                    }
                    var content = _Context.InventoryItemContents.Find(InventoryItemContentId);
                    if (content == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "Inventory Item Content not found";
                        response.Errors.Add(error);
                        return response;
                    }
                    var childs = _Context.InventoryItemContents.Where(a => a.ParentContentId == InventoryItemContentId).ToList();
                    if (childs.Count > 0)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "Inventory Item Has Children and can't be deleted";
                        response.Errors.Add(error);
                        return response;
                    }
                    _Context.InventoryItemContents.Remove(content);
                    _Context.SaveChanges();
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

        [HttpPost("AddInventoryItemContent")] // transfered to inventoryItem Service
        public async Task<BaseResponseWithID> AddInventoryItemContent(AddInventoryItemContentDto request)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                InventoryItemContent InventoryItemContentDB = new InventoryItemContent();

                if (Response.Result)
                {
                    //long ParentContentId = 0;
                    //long? InventoryItemID = null;

                    var InventoryItemListQuerable = _Context.InventoryItemContents.Where(x => x.Active == true).AsQueryable();

                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                    }
                    if (string.IsNullOrEmpty(request.ChapterName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err511";
                        error.ErrorMSG = "chapter Name is required.";
                        Response.Errors.Add(error);
                    }

                    if (request.ParentContentId != null && request.ParentContentId != 0)
                    {

                        InventoryItemContentDB = await InventoryItemListQuerable.Where(x => x.Id == request.ParentContentId).FirstOrDefaultAsync();
                        if (InventoryItemContentDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err512";
                            error.ErrorMSG = "The Parent Content Id selected not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (InventoryItemContentDB.InventoryItemId != request.InventoryItemId)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err512";
                            error.ErrorMSG = "This Parent is not related to inventory item selected";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    if (Response.Result)
                    {
                        string newOrderStr = "";
                        int newOrder = 1;
                        int newDataLevel = 1;

                        if (request.ParentContentId != 0)
                        {
                            newOrder = await InventoryItemListQuerable.Where(x => x.ParentContentId == request.ParentContentId).CountAsync() + 1;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err512";
                            error.ErrorMSG = "invalid parent content id";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        if (newOrder < 10)
                        {
                            newOrderStr = "0" + newOrder.ToString();
                        }
                        else
                        {
                            newOrderStr = newOrder.ToString();
                        }

                        InventoryItemContentDB = new InventoryItemContent
                        {
                            ChapterNumber = newOrderStr,
                            ChapterName = request.ChapterName,
                            Active = true,
                            CreationDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            CreatedBy = validation.userID,
                            ModifiedBy = validation.userID,
                            Description = request.Description,
                            InventoryItemId = request.InventoryItemId,
                            ParentContentId = request.ParentContentId,
                            PreparedSearchName = Common.string_compare_prepare_function(request.ChapterName)
                        };
                        var ParentContentId = request.ParentContentId;
                        if (ParentContentId == 0)
                        {
                            InventoryItemContentDB.DataLevel = newDataLevel;
                        }
                        else
                        {
                            var ParentAccountObjDB = await InventoryItemListQuerable.Where(x => x.Id == ParentContentId).FirstOrDefaultAsync();
                            if (ParentAccountObjDB != null)
                            {
                                InventoryItemContentDB.DataLevel = ParentAccountObjDB.DataLevel + 1;
                            }
                        }

                        _Context.InventoryItemContents.Add(InventoryItemContentDB);
                        if (request.ParentContentId != 0 && request.ParentContentId != null)
                        {
                            InventoryItemContentDB.Haveitem = true;
                        }
                        var ResAccount = await _Context.SaveChangesAsync();
                        Response.ID = InventoryItemContentDB.Id;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                if (ex.InnerException != null && ex.InnerException.Message.Contains("FK_InventoryItemContent_InventoryItem"))
                {
                    error.ErrorCode = "Err513";
                    error.ErrorMSG = "The specified Inventory Item ID does not exist.";
                }
                else
                {
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = ex.InnerException?.Message ?? ex.Message;
                }
                Response.Errors.Add(error);
                return Response;
            }
        }

        [HttpPost("UpdateInventoryItemContent")] // transfered to inventoryItem Service
        public async Task<BaseResponseWithID> UpdateInventoryItemContent(UpdateInventoryItemContentDto request)
        {
            BaseResponseWithID response = new BaseResponseWithID();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var inventoryItemContentDB = await _Context.InventoryItemContents.FindAsync(request.Id);

                    if (inventoryItemContentDB == null)
                    {
                        response.Result = false;
                        Error error = new Error
                        {
                            ErrorCode = "Err404",
                            ErrorMSG = $"InventoryItemContent with ID {request.Id} not found."
                        };
                        response.Errors.Add(error);
                        return response;
                    }

                    if (request.ChapterName == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid ChapterName";
                        response.Errors.Add(error);
                    }
                    else
                    {
                        inventoryItemContentDB.ChapterName = request.ChapterName;
                        inventoryItemContentDB.PreparedSearchName = Common.string_compare_prepare_function(request.ChapterName);
                    }


                    if (request.Description == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Description";
                        response.Errors.Add(error);
                    }
                    else
                    {
                        inventoryItemContentDB.Description = request.Description;
                    }


                    inventoryItemContentDB.ModifiedDate = DateTime.Now;
                    inventoryItemContentDB.ModifiedBy = validation.userID;

                    _Context.InventoryItemContents.Update(inventoryItemContentDB);
                    await _Context.SaveChangesAsync();

                    response.Result = true;
                }
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                if (ex.InnerException != null && ex.InnerException.Message.Contains("FK_InventoryItemContent_InventoryItem"))
                {
                    error.ErrorCode = "Err513";
                    error.ErrorMSG = "The specified Inventory Item ID does not exist.";
                }
                else
                {
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = ex.InnerException?.Message ?? ex.Message;
                }
                response.Errors.Add(error);
            }
            return response;
        }


        [HttpGet("GetInventoryItemList")] // transfered to inventoryItem Service
        public InventortyItemListResponse GetInventoryItemList()
        {
            InventortyItemListResponse Response = new InventortyItemListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var DDLList = new List<InventoryItemWithCheckOpeningBalance>();
                if (Response.Result)
                {


                    long StoreId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["StoreId"]) && long.TryParse(Request.Headers["StoreId"], out StoreId))
                    {
                        long.TryParse(Request.Headers["StoreId"], out StoreId);
                    }

                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 1000000;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }
                    decimal MinBalance = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["MinBalance"]) && decimal.TryParse(Request.Headers["MinBalance"], out MinBalance))
                    {
                        decimal.TryParse(Request.Headers["MinBalance"], out MinBalance);
                    }

                    decimal MaxBalance = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["MaxBalance"]) && decimal.TryParse(Request.Headers["MaxBalance"], out MaxBalance))
                    {
                        decimal.TryParse(Request.Headers["MaxBalance"], out MaxBalance);
                    }
                    bool GetNotActive = false;
                    if (!string.IsNullOrEmpty(Request.Headers["GetNotActive"]) && bool.TryParse(Request.Headers["GetNotActive"], out GetNotActive))
                    {
                        GetNotActive = bool.Parse(Request.Headers["GetNotActive"]);
                    }

                    var QuerableListDB = _Context.VInventoryItems.Where(x => x.Active == !GetNotActive).AsQueryable();
                    if (StoreId != 0)
                    {
                        var IDSInventoryItemsList = _Context.InventoryStoreItems.Where(x => x.InventoryStoreId == StoreId && x.FinalBalance > 0).Select(x => x.InventoryItemId).Distinct().ToList();
                        //if (IDSInventoryItemsList.Count() > 0)
                        //{
                        //}
                        QuerableListDB = QuerableListDB.Where(x => IDSInventoryItemsList.Contains(x.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(Request.Headers["SearchKey"]))
                    {

                        string SearchKey = Request.Headers["SearchKey"];
                        SearchKey = HttpUtility.UrlDecode(SearchKey);

                        QuerableListDB = QuerableListDB.Where(x => x.Name.ToLower().Contains(SearchKey.ToLower())
                                                || (x.Code != null ? x.Code.ToLower() == SearchKey.ToLower() : false)
                                                || (x.MaxBalance != null ? x.MaxBalance.ToString() == SearchKey.Trim() : false)
                                                ).AsQueryable();
                    }



                    if (!string.IsNullOrEmpty(Request.Headers["MarketName"]))
                    {
                        string MarketName = Request.Headers["MarketName"];
                        MarketName = HttpUtility.UrlDecode(MarketName);
                        QuerableListDB = QuerableListDB.Where(x => x.MarketName.ToLower().Contains(MarketName.ToLower())).AsQueryable();
                    }

                    if (!string.IsNullOrEmpty(Request.Headers["CommercialName"]))
                    {
                        string CommercialName = Request.Headers["CommercialName"];
                        CommercialName = HttpUtility.UrlDecode(CommercialName);
                        QuerableListDB = QuerableListDB.Where(x => x.CommercialName.ToLower().Contains(CommercialName.ToLower())).AsQueryable();
                    }

                    if (!string.IsNullOrEmpty(Request.Headers["PartNo"]))
                    {
                        string PartNo = Request.Headers["PartNo"];
                        PartNo = HttpUtility.UrlDecode(PartNo);
                        QuerableListDB = QuerableListDB.Where(x => x.PartNo == PartNo).AsQueryable();
                    }

                    string PublicNo = Request.Headers["PublicNo"];
                    if (!string.IsNullOrEmpty(PublicNo))
                    {
                        PublicNo = HttpUtility.UrlDecode(PublicNo);
                    }

                    if (!string.IsNullOrEmpty(Request.Headers["ChapterName"]))
                    {

                        string ChapterName = Request.Headers["ChapterName"];
                        ChapterName = HttpUtility.UrlDecode(ChapterName);
                        var preparedChapterName = Common.string_compare_prepare_function(ChapterName);
                        var itemsIds = _Context.InventoryItemContents.Where(a => a.PreparedSearchName.ToLower().Trim().Contains(preparedChapterName.ToLower().Trim())).Select(a => a.InventoryItemId).ToList();

                        QuerableListDB = QuerableListDB.Where(a => itemsIds.Contains(a.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(Request.Headers["SearchCode"]))
                    {

                        string SearchCode = Request.Headers["SearchCode"];
                        SearchCode = HttpUtility.UrlDecode(SearchCode);

                        QuerableListDB = QuerableListDB.Where(x => (x.Code != null ? x.Code.ToLower() == SearchCode.ToLower() : false)
                                                ).AsQueryable();
                    }
                    long PriorityID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["PriorityID"]) && long.TryParse(Request.Headers["PriorityID"], out PriorityID))
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.PriorityId == PriorityID).AsQueryable();
                    }
                    long InventoryItemCategoryID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["InventoryItemCategoryID"]) && long.TryParse(Request.Headers["InventoryItemCategoryID"], out InventoryItemCategoryID))
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.InventoryItemCategoryId == InventoryItemCategoryID).AsQueryable();
                    }
                    if (MinBalance != 0)
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.MinBalance < MinBalance).AsQueryable();
                    }

                    if (MaxBalance != 0)
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.MaxBalance > MaxBalance).AsQueryable();
                    }
                    bool ViewOpeningBalance = false;
                    if (!string.IsNullOrEmpty(Request.Headers["Type"]))
                    {
                        if (Request.Headers["Type"] == "OpeningBalance")
                        {
                            ViewOpeningBalance = true;
                        }
                    }
                    bool HaveOpeningBalance = false;
                    if (!string.IsNullOrEmpty(Request.Headers["HaveOpeningBalance"]))
                    {
                        if (!string.IsNullOrEmpty(Request.Headers["HaveOpeningBalance"]) && bool.TryParse(Request.Headers["HaveOpeningBalance"], out HaveOpeningBalance))
                        {
                            bool.TryParse(Request.Headers["HaveOpeningBalance"], out HaveOpeningBalance);
                        }
                    }
                    if (!string.IsNullOrEmpty(Request.Headers["HaveOpeningBalance"]))
                    {
                        var IDSItemsHavingOpeningBalance = _Context.VInventoryAddingOrderItems.Where(x => x.OperationType == "Opening Balance").Select(x => x.InventoryItemId).ToList();
                        if (HaveOpeningBalance)
                        {
                            QuerableListDB = QuerableListDB.Where(x => IDSItemsHavingOpeningBalance.Contains(x.Id)).AsQueryable();
                        }
                        else
                        {
                            QuerableListDB = QuerableListDB.Where(x => !IDSItemsHavingOpeningBalance.Contains(x.Id)).AsQueryable();
                        }
                    }
                    DateTime NotReleasedFrom = new DateTime(DateTime.Now.Year, 1, 1);
                    if (!string.IsNullOrEmpty(Request.Headers["NotReleasedFrom"]))
                    {
                        bool hasfrom = DateTime.TryParse(Request.Headers["NotReleasedFrom"], out NotReleasedFrom);
                    }

                    int ParentCategoryId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["HeadParentCategoryId"]) && int.TryParse(Request.Headers["HeadParentCategoryId"], out ParentCategoryId))
                    {
                        ParentCategoryId = int.Parse(Request.Headers["HeadParentCategoryId"]);
                    }

                    
                    /*Not Release duration date from and to*/
                    var InventoryItemIDs = new List<long>();
                    if (!string.IsNullOrWhiteSpace(Request.Headers["NotReleasedFrom"]) && !string.IsNullOrWhiteSpace(Request.Headers["NotReleasedTo"]))
                    {
                        InventoryItemIDs = _Context.InventoryStoreItems.Where(x => x.OperationType.Contains("Release Order") && x.Balance > 0)
                                           .GroupBy(item => item.InventoryItemId)
                                           .Select(group => group.OrderByDescending(item => item.CreationDate).FirstOrDefault()).Where(x => x.CreationDate <= NotReleasedFrom).Select(x => x.InventoryItemId)
                                           .Distinct().ToList();
                        //InventoryItemIDs =  _Context.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRelease.CreationDate >= NotReleasedFrom && x.InventoryMatrialRelease.CreationDate <= NotReleasedTo).Select(x => x.InventoryMatrialRequestItem.InventoryItemID).ToList();
                        if (InventoryItemIDs.Count() > 0)
                        {
                            var asdad = QuerableListDB.Where(x => InventoryItemIDs.Contains(x.Id)).ToList();
                            QuerableListDB = QuerableListDB.Where(x => InventoryItemIDs.Contains(x.Id)).AsQueryable();
                        }
                    }

                    if (PublicNo != null)
                    {
                        var InventoryItemId = _Context.InventoryStoreItems.Where(x => x.OrderNumber == PublicNo).Select(x => x.InventoryItemId).FirstOrDefault();
                        QuerableListDB = QuerableListDB.Where(x => x.Id == InventoryItemId).AsQueryable();
                    }
                    if (ParentCategoryId != 0)
                    {
                        var categoryIds = Common.GetCategoryIdsIncludingChildren(ParentCategoryId,_Context);
                        QuerableListDB = QuerableListDB.Where(a => categoryIds.Contains(a.InventoryItemCategoryId)).AsQueryable();
                    }


                    var InventoryItemPagingList = PagedList<VInventoryItem>.Create(QuerableListDB.OrderBy(x => x.Id), CurrentPage, NumberOfItemsPerPage);

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = InventoryItemPagingList.TotalPages,
                        ItemsPerPage = NumberOfItemsPerPage,
                        TotalItems = InventoryItemPagingList.TotalCount
                    };


                    if (InventoryItemPagingList.Count > 0)
                    {
                        var CategoryList = _Context.InventoryItemCategories.ToList();

                        Response.TotalItemCount = InventoryItemPagingList.Count();
                        var InventoryItemsIDS = InventoryItemPagingList.Select(x => x.Id).ToList();
                        var InvItemsListDB = _Context.InventoryItems.Where(x => InventoryItemsIDS.Contains(x.Id)).ToList();
                        foreach (var item in InventoryItemPagingList)
                        {
                            var ResultParentHeadCategory = Common.GetHeadParentCategory(CategoryList, item.InventoryItemCategoryId);
                            var inv = InvItemsListDB.Where(x => x.Id == item.Id).FirstOrDefault();
                            var DLLObj = new InventoryItemWithCheckOpeningBalance();
                            if (!string.IsNullOrWhiteSpace(PublicNo) )
                            {
                               DLLObj.PublicNo = PublicNo;
                            }
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Name.Trim();
                            DLLObj.ItemCode = item.Code;
                            DLLObj.PartNo = item.PartNo;
                            DLLObj.CategoryId = item.InventoryItemCategoryId;
                            DLLObj.CategoryName = item.CategoryName;
                            DLLObj.HeadParentCategoryId = ResultParentHeadCategory.Item1;
                            DLLObj.HeadParentCategoryName = ResultParentHeadCategory.Item2;
                            DLLObj.RequstionUOMID = item.RequstionUomid;
                            DLLObj.PurchasingUOMID = item.PurchasingUomid;
                            DLLObj.Active = item.Active;
                            DLLObj.CalculationType = item.CalculationType;
                            DLLObj.CustomeUnitPrice = item.CustomeUnitPrice;
                            DLLObj.CommericalName = item.CommercialName;
                            DLLObj.Description = item.Description;
                            DLLObj.Details = item.Details;
                            DLLObj.MarketName = item.MarketName;
                            DLLObj.MaxBlanace = item.MaxBalance;
                            DLLObj.MinBalance = item.MinBalance;
                            DLLObj.SerialCounter = item.ItemSerialCounter;
                            DLLObj.UOR = item.RequestionUomshortName.Trim();
                            DLLObj.UOP = item.PurchasingUomshortName.Trim();
                            if (inv.ImageUrl != null && inv.ImageUrl != "NULL")
                            {
                                DLLObj.ImageUrl = Globals.baseURL + '/' + inv.ImageUrl;
                            }
                            DLLObj.ExchangeFactor = item.ExchangeFactor;
                            if (ViewOpeningBalance)
                            {
                                if (!string.IsNullOrEmpty(Request.Headers["HaveOpeningBalance"]))
                                {
                                    if (HaveOpeningBalance)
                                    {
                                        DLLObj.HaveOpeningBalance = true;
                                    }
                                    else
                                    {
                                        DLLObj.HaveOpeningBalance = false;
                                    }
                                }
                                else
                                {
                                    DLLObj.HaveOpeningBalance = Common.CheckInventoryItemHaveOpeningBalance(item.Id, _Context);
                                }
                            }
                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList.Distinct().ToList();
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

        [HttpGet("GetInventoryStoresIncludeLocationsList")] // transfered to inventoryItem Service

        public InventortyStoreIncludeLocationListResponse GetInventoryStoresIncludeLocationsListForBY()
        {
            InventortyStoreIncludeLocationListResponse Response = new InventortyStoreIncludeLocationListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var StoreList = new List<InventortyStoreIncludeLocation>();
                if (Response.Result)
                {
                    long userId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["userId"]) && long.TryParse(Request.Headers["userId"], out userId))
                    {
                        long.TryParse(Request.Headers["userId"], out userId);
                    }

                    var ListDB = _Context.InventoryStores.Include(x => x.InventoryStoreKeepers).Where(x => x.Active == true).ToList();
                    if (userId != 0)
                    {
                        var storeIdsListFromKeepers = _Context.InventoryStoreKeepers.Where(x => x.Active && x.UserId == userId).Select(x => x.InventoryStoreId).ToList();
                        ListDB = ListDB.Where(x => storeIdsListFromKeepers.Contains(x.Id)).ToList();

                    }
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new InventortyStoreIncludeLocation();
                            DLLObj.Id = item.Id;
                            DLLObj.StoreName = item.Name;
                            DLLObj.LocationName = item.Location;
                            DLLObj.CountOfKeepers = item.InventoryStoreKeepers?.Count();

                            StoreList.Add(DLLObj);
                        }
                    }

                }
                Response.DDLList = StoreList;
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


        [HttpGet("GetInventoryStoreItemLowStockList")] // transfered to inventoryItem Service
        public InventortyItemLowStockListResponse GetInventoryStoreItemLowStockList()
        {
            InventortyItemLowStockListResponse Response = new InventortyItemLowStockListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var DDLList = new List<InventoryStoreItemLowStock>();
                if (Response.Result)
                {
                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }


                    var InventoryItemListDB = _Context.VInventoryStoreItems.Where(x => x.Active == true && x.StoreActive == true).AsQueryable();
                    if (!string.IsNullOrEmpty(Request.Headers["SearchKey"]))
                    {

                        string SearchKey = Request.Headers["SearchKey"];
                        SearchKey = HttpUtility.UrlDecode(SearchKey);

                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                                                || (x.Code != null ? x.Code.ToLower().Contains(SearchKey.ToLower()) : false)
                                                ).AsQueryable();
                    }

                    long InventoryStoreID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["InventoryStoreID"]) && long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID))
                    {
                        long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID);
                    }
                    if (InventoryStoreID != 0)
                    {
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                    }


                    //if (InventoryStoreID != null && InventoryStoreID != 0)
                    //{
                    //    InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreID == InventoryStoreID).AsQueryable();
                    //}
                    var InventoryItemGroupingPerItem = InventoryItemListDB.GroupBy(x => new InventoryStoreItemLowStock
                    {
                        ID = x.InventoryItemId,
                        Name = x.InventoryItemName,
                        MinBalance = x.MinBalance,
                        MaxBalance = x.MaxBalance,
                        //CurrentBalance = x.finalBalance,
                        ExchangeFactor = x.ExchangeFactor,
                        ItemCode = x.Code,
                        StoreID = x.InventoryStoreId
                    });
                    // modified on 8-3-2023 change from balance to final balance
                    var QuerableDB = InventoryItemGroupingPerItem.Where(x => x.Sum(a => a.FinalBalance) <= x.Key.MinBalance).OrderBy(x => x.Key.ID).AsQueryable();



                    //var QuerabletDB = _Context.V_InventoryStoreItemMinBalance.Where(x => x.Active == true && x.StoreActive == true).Select(x=> new InventoryStoreItemLowStock
                    //{
                    //    ID = x.InventoryItemID,
                    //    StoreID = x.InventoryStoreID,
                    //    Name = x.InventoryItemName,
                    //    ItemCode = x.Code
                    //}
                    //).Distinct().AsQueryable();



                    var InventoryStoreItemPagingList = PagedList<IGrouping<InventoryStoreItemLowStock, VInventoryStoreItem>>.Create(QuerableDB, CurrentPage, NumberOfItemsPerPage);

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = InventoryStoreItemPagingList.TotalPages,
                        ItemsPerPage = NumberOfItemsPerPage,
                        TotalItems = InventoryStoreItemPagingList.TotalCount
                    };

                    if (InventoryStoreItemPagingList.Count > 0)
                    {
                        foreach (var item in InventoryStoreItemPagingList)
                        {
                            //var InventoryItemObjDB = _Context.V_InventoryItem.Where(x => x.Active == true && x.ID == item.ID).FirstOrDefault();
                            var DLLObj = new InventoryStoreItemLowStock();
                            //if (InventoryItemObjDB != null)
                            //{
                            //}
                            var ItemLowStockRes = item.Key;
                            ItemLowStockRes.CurrentBalance = item.Sum(x => x.FinalBalance);
                            ItemLowStockRes.HavePRorPO = Common.CheckInventoryItemHavePRorPO(ItemLowStockRes.ID, _Context);
                            DLLObj = ItemLowStockRes;

                            DDLList.Add(DLLObj);
                        }
                    }
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


        [HttpPost("AddInventoryItemMatrialRelease")] //converted to service
        public BaseResponseWithID AddInventoryItemMatrialRelease([FromBody] AddInventoryItemMatrialReleaseRequest BodyRequest)
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

                    long MatrialReleaseOrderId = 0;
                    if (BodyRequest.MatrialReleaseOrderId != null)
                    {
                        MatrialReleaseOrderId = (long)BodyRequest.MatrialReleaseOrderId;
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
                    if (BodyRequest.MatrialRequestOrderId != null)
                    {
                        MatrialRequestOrderId = (long)BodyRequest.MatrialRequestOrderId;
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
                    var MatrialRequestOrderDB = _Context.VInventoryMatrialRequests.Where(x => x.Id == MatrialRequestOrderId).FirstOrDefault();
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
                    if (string.IsNullOrEmpty(BodyRequest.RequestDate) || !DateTime.TryParse(BodyRequest.RequestDate, out RequestDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err33";
                        error.ErrorMSG = "Invalid Request Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (BodyRequest.MatrialReleaseItemList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemList.Count < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-415";
                        error.ErrorMSG = "please insert at least one Matrial Release Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (BodyRequest.MatrialReleaseItemList.Where(x => x.NewRecivedQTY == null).Count() > 0)
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

                    if (BodyRequest.MatrialReleaseItemList.Where(x => x.MatrialRequestItemID == 0).Count() > 0)
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
                    var MatrialRequestItemIDsList = BodyRequest.MatrialReleaseItemList.Select(x => x.MatrialRequestItemID).ToList();
                    var MatrialRequestItemListDB = _Context.InventoryMatrialRequestItems.Where(x => MatrialRequestItemIDsList.Contains(x.Id)).ToList();

                    // check If Balance Avaliable from PArent Release or NOT
                    // validate is Count distinct < count of Items => there is iteration items with same data
                    var ItemTotalCount = BodyRequest.MatrialReleaseItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Count();
                    var ItemDistinctCount = BodyRequest.MatrialReleaseItemList.Where(x => x.StockBalanceList != null).SelectMany(x => x.StockBalanceList.Select(a => a.ID).Distinct().ToList()).Distinct().Count();

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
                    foreach (var item in BodyRequest.MatrialReleaseItemList)
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
                                (MatrialRequestItemObjDB.RecivedQuantity1 + item.NewRecivedQTY) > MatrialRequestItemObjDB.ReqQuantity1)
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


                    if (BodyRequest.IsRenew == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-15";
                        error.ErrorMSG = "please select Is Renew True Or False.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    bool? IsRenew = BodyRequest.IsRenew;



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
                        if (BodyRequest.IsRenew == true && MatrialReleaseOrderId == 0) // New Release Order
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
                            if (!string.IsNullOrEmpty(BodyRequest.CreationDate) && DateTime.TryParse(BodyRequest.CreationDate, out CreationDate))
                            {
                                CreationDate = DateTime.Parse(BodyRequest.CreationDate);
                            }


                            MatrialReleaseObj.ToUserId = MatrialRequestOrderDB.FromUserId;
                            MatrialReleaseObj.FromInventoryStoreId = MatrialRequestOrderDB.ToInventoryStoreId;
                            MatrialReleaseObj.RequestDate = RequestDate;
                            MatrialReleaseObj.CreationDate = CreationDate;
                            MatrialReleaseObj.ModifiedDate = DateTime.Now;
                            MatrialReleaseObj.CreatedBy = validation.userID;
                            MatrialReleaseObj.ModifiedBy = validation.userID;
                            MatrialReleaseObj.Active = true;
                            MatrialReleaseObj.Status = "Closed";
                            MatrialReleaseObj.MatrialRequestId = MatrialRequestOrderId;

                            _Context.InventoryMatrialReleases.Add(MatrialReleaseObj);
                        }
                        else // Uppdate Release Order
                        {
                            MatrialReleaseObj = _Context.InventoryMatrialReleases.Where(x => x.Id == MatrialReleaseOrderId).FirstOrDefault();
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
                                    MatrialReleaseObj.ModifiedBy = validation.userID;
                                    MatrialReleaseObj.Active = true;
                                    MatrialReleaseObj.Status = Status;
                                    MatrialReleaseObj.MatrialRequestId = MatrialRequestOrderId;

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

                        _Context.SaveChanges();

                        int itemCount = 0;


                        // Get All InventoryStoreItems Used in this Release before loop
                        var IDSInventoryItemListRequested = MatrialRequestItemListDB.Select(x => x.InventoryItemId).Distinct().ToList();
                        var InventoryStoreItemList = _Context.InventoryStoreItems.Where(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId)).ToList();
                        var IDSSalesOfferProduct = MatrialRequestItemListDB.Select(x => x.OfferItemId).ToList();
                        var SalesOfferProductsList = _Context.SalesOfferProducts.Where(x => IDSSalesOfferProduct.Contains(x.Id)).ToList();
                        var POItemList = _Context.PurchasePoitems.Where(x => IDSInventoryItemListRequested.Contains(x.InventoryItemId)).ToList();
                        #region items Insertion And More Calc
                        foreach (var item in BodyRequest.MatrialReleaseItemList)
                        {
                            itemCount++;

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

                                var AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
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
                                    var InventoryItemObjDB = _Context.InventoryItems.Where(x => x.Id == MatrialRequestItemObjDB.InventoryItemId).FirstOrDefault(); // _Context.proc_InventoryItemLoadByPrimaryKey(MatrialRequestItemObjDB.InventoryItemID).FirstOrDefault();
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

                                    _Context.InventoryMatrialReleaseItems.Add(MatrialReleaseItem);

                                    var MatrialReleaseItemInsert = _Context.SaveChanges();
                                    if (MatrialReleaseItemInsert > 0)
                                    {
                                        long MatrialReleaseItemID = MatrialReleaseItem.Id;
                                        // Update Matrial request item

                                        //var RecivedQuantityForMR = MatrialRequestItemObjDB.RecivedQuantity + item.NewRecivedQTY;
                                        MatrialRequestItemObjDB.RecivedQuantity1 += item.NewRecivedQTY;
                                        _Context.SaveChanges();
                                        //var MatiralRequestItemUpdate = _Context.Myproc_InventoryMatrialRequestItemsUpdate_RecuvedQTY(item.MatrialRequestItemID,
                                        //                                                                                            RecivedQuantityForMR);


                                        if (MatrialRequestItemObjDB.RecivedQuantity > 0 && MatrialRequestOrderDB.RequestTypeId == 3) // Custoday Type
                                        {
                                            var CustodyDb = _Context.Hrcustodies.Where(a => a.MaterialRequestItemId == MatrialRequestItemObjDB.Id).FirstOrDefault();
                                            if (CustodyDb != null)
                                            {
                                                int CustodyStatus = 0;

                                                var MaterialReleasedItemsDb = _Context.InventoryMatrialReleaseItems
                                                    .Where(a => a.InventoryMatrialRequestItemId == CustodyDb.MaterialRequestItemId).ToList();
                                                var MaterialReleasedItemsIDs = MaterialReleasedItemsDb.Select(a => a.Id).ToList();

                                                var InterrnalBackOrdersDb = _Context.InventoryInternalBackOrderItems.Where(
                                                    a => MaterialReleasedItemsIDs.Contains(a.InventoryMatrialReleaseItemId ?? 0)).ToList();
                                                var TotalReturned = InterrnalBackOrdersDb.Sum(a => a.RecivedQuantity);

                                                if (TotalReturned > 0)
                                                {
                                                    if (MatrialRequestItemObjDB.RecivedQuantity < MatrialRequestItemObjDB.ReqQuantity)
                                                    {
                                                        if (TotalReturned < MatrialRequestItemObjDB.RecivedQuantity)
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
                                                        if (TotalReturned < MatrialRequestItemObjDB.RecivedQuantity)
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

                                                //var CustodyUpdate = _Context.proc_HRCustodyUpdate(CustodyDb.ID,
                                                //                                                    CustodyDb.UserID,
                                                //                                                    CustodyDb.IsAssetsType,
                                                //                                                    CustodyDb.InventoryItemID,
                                                //                                                    CustodyDb.MaterialRequestItemId,
                                                //                                                    CustodyDb.MaterialRequestId,
                                                //                                                    CustodyDb.Description,
                                                //                                                    null,
                                                //                                                    null,
                                                //                                                    null,
                                                //                                                    CustodyStatus,
                                                //                                                    true,
                                                //                                                    DateTime.Now.Date,
                                                //                                                    validation.userID,
                                                //                                                    DateTime.Now.Date,
                                                //                                                    validation.userID
                                                //                                                    );

                                                CustodyDb.StatusId = CustodyStatus;
                                                CustodyDb.Active = true;
                                                CustodyDb.ModifiedDate = DateTime.Now;
                                                CustodyDb.ModifiedBy = validation.userID;
                                                _Context.SaveChanges();

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
                                                            currencyId = POItemObjDB.CurrencyId ?? 0;
                                                            rateToEGP = POItemObjDB.RateToEgp ?? 0;
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
                                                InventoryStoreItemOBJ.CreatedBy = validation.userID;
                                                InventoryStoreItemOBJ.ModifiedBy = validation.userID;
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
                                                _Context.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                                                var InventoryStorItemInsertion = _Context.SaveChanges();

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
                                                        ParentInventoryStoreItem.ModifiedBy = validation.userID;


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
                                                        _Context.SaveChanges();
                                                    }
                                                }
                                            }


                                        }



                                        //var ListInventoryStoreItem = _Context.InventoryStoreItems.Where(x => x.InventoryItemID == MatrialRequestItemObjDB.InventoryItemID && x.finalBalance > 0);

                                        // -------------------------------------------------Update and Calc Average for Inventory Item ----------------------------------------------------------------
                                        var ListInvStoreItemAll = InventoryStoreItemList.Where(x => x.InventoryItemId == InventoryItemObjDB.Id);
                                        var ListInventoryStoreItem = ListInvStoreItemAll.Where(x => x.FinalBalance > 0 && x.PoinvoiceTotalCost != null);
                                        var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.PoinvoiceId).ToList();
                                        var ListIDSPOInvoicesIsFulllyPriced = _Context.PurchasePoinvoices.Where(x => ListIDSPOInvoices.Contains(x.Id)).Select(x => x.Id).ToList();
                                        ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.PoinvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.PoinvoiceId) : false);
                                        InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.PoinvoiceTotalCostEgp * x.FinalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.FinalBalance) ?? 0) : 0;

                                        // Update Avg Unit Price ..just stop for list of inventorystoreItem List
                                        //foreach (var itemId in ListIDSUpdate)
                                        //{
                                        //    var InventoryStoreItemOBJ = ListInvStoreItemAll.Where(x => x.ID == itemId).FirstOrDefault();
                                        //    InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;

                                        //}
                                        _Context.SaveChanges();


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
                                        _Context.SaveChanges();
                                        //}



                                        // Check is finish

                                        //check notification
                                        #region Save Cost Center to DB
                                        if (BodyRequest.IsFinish == true)
                                        {
                                            decimal Quantity = item.NewRecivedQTY ?? 0; //decimal.Parse(row["NewRecivedQuantity"].ToString());
                                            var UnitPrice = InventoryItemObjDB.AverageUnitPrice > 0 ? InventoryItemObjDB.AverageUnitPrice : InventoryItemObjDB.CustomeUnitPrice;
                                            decimal amountOfItem = Quantity * UnitPrice;

                                            var GeneralActiveCostCenterObjDB = _Context.GeneralActiveCostCenters.Where(x => x.CategoryId == MatrialRequestItemObjDB.ProjectId).FirstOrDefault();

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
                                                        _Context.SaveChanges();
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
                                                _Context.SaveChanges();


                                                if (costCenterID != 0)
                                                {

                                                    GeneralActiveCostCenterObjDB.CumulativeCost = GeneralActiveCostCenterObjDB.CumulativeCost + amountOfItem;
                                                    GeneralActiveCostCenterObjDB.Modified = DateTime.Now;
                                                    GeneralActiveCostCenterObjDB.ModifiedBy = validation.userID;
                                                    _Context.SaveChanges();
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

                                }
                                else
                                {
                                    // Invalid Matrial request item id
                                }
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
                        _Context.SaveChanges();

                        #endregion items

                        //if (Request.IsFinish == true)
                        //{
                        bool complete = true;
                        var InventoryMatrialRequestItemObjDB = _Context.InventoryMatrialRequestItems.Where(x => x.InventoryMatrialRequestId == MatrialRequestOrderId).ToList();
                        foreach (var item in InventoryMatrialRequestItemObjDB)
                        {
                            if (item.ReqQuantity1 > item.RecivedQuantity1)
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

                            //_Context.Myproc_InventoryMatrialRequestUpdate_HoldReleased(MatrialRequestOrderId, "Closed");
                            var MatrialRequestOrder = _Context.InventoryMatrialRequests.Where(x => x.Id == MatrialRequestOrderId).FirstOrDefault();
                            if (MatrialRequestOrder != null)
                            {
                                MatrialRequestOrder.Status = "Closed";
                                _Context.SaveChanges();
                            }

                        }
                        // }
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

        //converted to service
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


        [HttpGet("GetAccountAndFinanceInventoryStoreItemReportList")] //converted to service
        public AccountsAndFinanceInventoryStoreItemResponse GetAccountAndFinanceInventoryStoreItemReportList()
        {
            AccountsAndFinanceInventoryStoreItemResponse Response = new AccountsAndFinanceInventoryStoreItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var InventoryStoreItemList = new List<InventoryStoreItemForReport>();
                //decimal TotalStockBalance = 0;
                //decimal TotalStockBalanceValue = 0;
                //decimal TotalUnitCost = 0;
                int NoOfItems = 0;

                if (Response.Result)
                {
                    long InventoryStoreID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["InventoryStoreID"]) && long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID))
                    {
                        long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID);
                    }

                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }
                    decimal MinBalance = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["LowStock"]) && decimal.TryParse(Request.Headers["LowStock"], out MinBalance))
                    {
                        decimal.TryParse(Request.Headers["LowStock"], out MinBalance);
                    }

                    decimal MaxBalance = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["ExceedBalance"]) && decimal.TryParse(Request.Headers["ExceedBalance"], out MaxBalance))
                    {
                        decimal.TryParse(Request.Headers["ExceedBalance"], out MaxBalance);
                    }
                    string CategoryName = null;
                    if (!string.IsNullOrEmpty(Request.Headers["CategoryName"]))
                    {
                        CategoryName = Request.Headers["CategoryName"];
                        CategoryName = HttpUtility.UrlDecode(CategoryName);
                    }

                    //long POID = 0;
                    //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
                    //{
                    //    long.TryParse(headers["POID"], out POID);
                    //}

                    if (Response.Result)
                    {
                        // Not Grouped -----------------------------------------
                        //var InventoryStoreItemListDB = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        //if (InventoryStoreID != 0)
                        //{
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreID == InventoryStoreID).ToList();
                        //}
                        //if (headers["ItemSerial"] != null && headers["ItemSerial"] != "")
                        //{
                        //    string ItemSerial = headers["ItemSerial"];
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.Code == ItemSerial).ToList();
                        //}

                        //if (headers["SearchKey"] != null && headers["SearchKey"] != "")
                        //{
                        //    string SearchKey = headers["SearchKey"];
                        //    var ListItemIDFilter = _Context.V_InventoryStoreItem.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.Code.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.MarketName.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.CommercialName.ToLower().Contains(SearchKey.ToLower())).Select(x => x.InventoryItemID).Distinct().ToList();

                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemID)).ToList();
                        //}

                        #region For PagedList
                        // var AllInventoryItemPriceAllList = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        var InventoryStoreItemListDB = _Context.VInventoryStoreItemPriceReports.Where(x => x.Active == true).OrderBy(a => a.InventoryItemId).AsQueryable();
                        var InventoryStoreItemWithFilterInventoryListDB = InventoryStoreItemListDB;
                        if (InventoryStoreID != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID);
                            InventoryStoreItemWithFilterInventoryListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID);
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["ItemSerial"]))
                        {
                            string ItemSerial = Request.Headers["ItemSerial"];
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.Code == ItemSerial);
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["PartNO"]))
                        {
                            string PartNO = Request.Headers["PartNO"];
                            var ListItemIDFilter = _Context.VInventoryStoreItems.Where(x => x.PartNo.ToLower() == PartNO.ToLower()).Select(x => x.InventoryItemId).Distinct().ToList();

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemId));
                        }
                        if (!string.IsNullOrWhiteSpace(CategoryName))
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.CategoryName == CategoryName);
                        }
                        if (MinBalance != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.MinBalance < MinBalance).AsQueryable();
                        }

                        if (MaxBalance != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.MaxBalance > MaxBalance).AsQueryable();
                        }
                        bool LowStock = false;
                        if (!string.IsNullOrEmpty(Request.Headers["LowStock"]) && bool.TryParse(Request.Headers["LowStock"], out LowStock))
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0 && x.FinalBalance < x.MinBalance).AsQueryable();
                        }
                        bool ExceedBalance = false;
                        if (!string.IsNullOrEmpty(Request.Headers["ExceedBalance"]) && bool.TryParse(Request.Headers["ExceedBalance"], out ExceedBalance))
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0 && x.FinalBalance > x.MaxBalance).AsQueryable();
                        }

                        bool IsExpDate = false;
                        if (!string.IsNullOrEmpty(Request.Headers["IsExpDate"]) && bool.TryParse(Request.Headers["IsExpDate"], out IsExpDate))
                        {
                            if (IsExpDate)
                            {
                                InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.ExpDate < DateTime.Now).AsQueryable();
                            }
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["SearchKey"]))
                        {
                            string SearchKey = Request.Headers["SearchKey"];
                            SearchKey = HttpUtility.UrlDecode(SearchKey);

                            var ListItemIDFilter = _Context.VInventoryStoreItems.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Code.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.MarketName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CommercialName.ToLower().Contains(SearchKey.ToLower())).Select(x => x.InventoryItemId).Distinct().ToList();

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemId));
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["ChapterName"]))
                        {

                            string ChapterName = Request.Headers["ChapterName"];
                            ChapterName = HttpUtility.UrlDecode(ChapterName);
                            var preparedChapterName = Common.string_compare_prepare_function(ChapterName);
                            var itemsIds = _Context.InventoryItemContents.Where(a => a.PreparedSearchName.ToLower().Trim().Contains(preparedChapterName.ToLower().Trim())).Select(a => a.InventoryItemId).ToList();

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(a => itemsIds.Contains(a.InventoryItemId)).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(Request.Headers["MatrialAddingOrderSerial"]))
                        {
                            string MatrialAddingOrderSerial = Request.Headers["MatrialAddingOrderSerial"];
                            var IDSInventoryItemList = _Context.VInventoryAddingOrderItems.Where(x => x.ItemSerial.Trim() == MatrialAddingOrderSerial.Trim()).Select(x => x.InventoryItemId).ToList();
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => IDSInventoryItemList.Contains(x.InventoryItemId));
                        }
                        var InventoryStoreItemPriceDistinctList = InventoryStoreItemListDB.Select(x => new InventoryStoreItemForReport
                        {
                            ID = x.InventoryItemId,
                            InventoryStoreId = x.InventoryStoreId,
                            ItemName = x.InventoryItemName,
                            InventoryStoreName = x.InventoryStoreName,
                            HoldQTY = (decimal)_Context.VInventoryMatrialRequestWithItems.Where(a => a.InventoryItemId == x.InventoryItemId /*&& a.RequestTypeID == 20003*/ && a.ToInventoryStoreId == x.InventoryStoreId).Select(a => a.ReqQuantity1).Sum(),
                            OpenPOQTY = x.ReqQuantity > x.RecivedQuantity ? x.ReqQuantity - x.RecivedQuantity : 0,
                            ////V_PurchasePoItem_PO.Where(a => a.InventoryItemID == x.InventoryItemID && a.Status == "Open" && a.ReqQuantity != null).Sum(a => a.ReqQuantity) >=
                            //           V_InventoryAddingOrderItems.Where(a => a.InventoryItemID == x.InventoryItemID && a.RecivedQuantity != null).Sum(a => a.RecivedQuantity) ?
                            //           V_PurchasePoItem_PO.Where(a => a.InventoryItemID == x.InventoryItemID && a.Status == "Open" && a.ReqQuantity != null).Sum(a => a.ReqQuantity) -
                            //           V_InventoryAddingOrderItems.Where(a => a.InventoryItemID == x.InventoryItemID && a.RecivedQuantity != null).Sum(a => a.RecivedQuantity)
                            //           : 0,
                            Active = x.Active ?? false,
                            ItemCode = x.Code,
                            MinStock = x.MinBalance,
                            MaxStock = x.MaxBalance,
                            RequestionUOMShortName = x.RequestionUomshortName,
                            StockBalance = x.StockBalance != null && x.StockBalance >= 0 ? (decimal)x.StockBalance : 0, //x.StockFinalBalance ?? 0, // x.StockBalance ?? 0,// InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.Balance != null).Sum(a => a.Balance) ?? 0,
                            StockBalanceValue = 0,
                            //InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMAverageUnitPrice != null && a.CalculationType == 1).Select(a => a.SUMAverageUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMMaxUnitPrice != null && a.CalculationType == 2).Select(a => a.SUMMaxUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMLastUnitPrice != null && a.CalculationType == 3).Select(a => a.SUMLastUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMCustomeUnitPrice != null && a.CalculationType == 4).Select(a => a.SUMCustomeUnitPrice).Sum() ?? 0,
                            UnitCost = x.CustomeUnitPrice != null ? (decimal)x.CustomeUnitPrice : 0,
                        }).Distinct().AsQueryable();

                        //var IDInventoryItemList = InventoryStoreItemListDB.Select(x => new InventoryStoreItemPaging
                        //{
                        //    InventoryItemID = x.InventoryItemID,
                        //    SUMAverageUnitPrice = x.SUMAverageUnitPrice,
                        //    SUMMaxUnitPrice = x.SUMMaxUnitPrice,
                        //    SUMLastUnitPrice = x.SUMLastUnitPrice,
                        //    SUMCustomeUnitPrice = x.SUMCustomeUnitPrice,
                        //    InventoryItemName = x.InventoryItemName,
                        //    Code = x.Code,
                        //    RequestionUOMShortName = x.RequestionUOMShortName,
                        //    Balance = x.Balance,
                        //    CalculationType = x.CalculationType,
                        //    CustomeUnitPrice = x.CustomeUnitPrice
                        //}).Distinct().AsQueryable();
                        //var InventoryStoreItemPagingList = PagedList<InventoryStoreItemPaging>.Create(IDInventoryItemList, CurrentPage, NumberOfItemsPerPage);
                        //var InventoryStoreItemPagingList = PagedList<V_InventoryStoreItemPrice>.Create(InventoryStoreItemListDB, CurrentPage, NumberOfItemsPerPage);
                        InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ID);
                        if (!string.IsNullOrEmpty(Request.Headers["SortBy"]))
                        {
                            if (Request.Headers["SortBy"] == "ItemName")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ItemName);
                            }
                            else if (Request.Headers["SortBy"] == "ItemCode")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ItemCode);
                            }
                            else if (Request.Headers["SortBy"] == "StoreName")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.InventoryStoreName);
                            }
                        }

                        DateTime NotReleasedFrom = new DateTime(DateTime.Now.Year, 1, 1);
                        if (!string.IsNullOrEmpty(Request.Headers["NotReleasedFrom"]))
                        {
                            bool hasfrom = DateTime.TryParse(Request.Headers["NotReleasedFrom"], out NotReleasedFrom);
                        }

                        /*Not Release duration date from and to*/
                        var InventoryItemIDs = new List<long>();
                        if (!string.IsNullOrWhiteSpace(Request.Headers["NotReleasedFrom"]))
                        {
                            InventoryItemIDs = _Context.InventoryStoreItems.Where(x => x.OperationType.Contains("Release Order"))
                                               .GroupBy(item => item.InventoryItemId)
                                               .Select(group => group.OrderByDescending(item => item.CreationDate).FirstOrDefault()).Where(x => x.CreationDate <= NotReleasedFrom).Select(x => x.InventoryItemId)
                                               .Distinct().ToList();
                            //InventoryItemIDs =  _Context.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRelease.CreationDate >= NotReleasedFrom && x.InventoryMatrialRelease.CreationDate <= NotReleasedTo).Select(x => x.InventoryMatrialRequestItem.InventoryItemID).ToList();
                            if (InventoryItemIDs != null)
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.Where(x => InventoryItemIDs.Contains(x.ID) && x.StockBalance > 0).AsQueryable();
                            }
                        }

                        var InventoryStoreItemPagingList = PagedList<InventoryStoreItemForReport>.Create(InventoryStoreItemPriceDistinctList, CurrentPage, NumberOfItemsPerPage);

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = CurrentPage,
                            TotalPages = InventoryStoreItemPagingList.TotalPages,
                            ItemsPerPage = NumberOfItemsPerPage,
                            TotalItems = InventoryStoreItemPagingList.TotalCount
                        };
                        #endregion



                        // New for paging Calc
                        if (InventoryStoreItemPagingList.Count > 0)
                        {

                            foreach (var item in InventoryStoreItemPagingList)
                            {
                                var InventoryStoreItemListDBForCalc = InventoryStoreItemListDB.Where(a => a.InventoryItemId == item.ID && a.InventoryStoreId == item.InventoryStoreId).AsQueryable();
                                var V_InventoryItemObjDB = _Context.VInventoryItems.Where(x => x.Id == item.ID).FirstOrDefault();
                                if (V_InventoryItemObjDB != null)
                                {

                                    var ItemOBJ = new InventoryStoreItemForReport();
                                    ItemOBJ.ID = item.ID;
                                    ItemOBJ.InventoryStoreId = item.InventoryStoreId;
                                    ItemOBJ.InventoryStoreName = item.InventoryStoreName;
                                    ItemOBJ.HoldQTY = item.HoldQTY;
                                    ItemOBJ.OpenPOQTY = item.OpenPOQTY;
                                    ItemOBJ.Active = item.Active;
                                    ItemOBJ.ItemCode = item.ItemCode;
                                    ItemOBJ.ItemName = item.ItemName;
                                    ItemOBJ.UnitCost = item.UnitCost;
                                    ItemOBJ.RequestionUOMShortName = item.RequestionUOMShortName;
                                    ItemOBJ.StockBalance = item.StockBalance;
                                    ItemOBJ.MinStock = item.MinStock;
                                    ItemOBJ.MaxStock = item.MaxStock;
                                    ItemOBJ.StockBalanceValue = InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 1).Sum(a => a.SumaverageUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 2).Sum(a => a.SummaxUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 3).Sum(a => a.SumlastUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 4).Sum(a => a.SumcustomeUnitPrice) ?? 0;
                                    //InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMAverageUnitPrice != null && a.CalculationType == 1).Select(a => a.SUMAverageUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMMaxUnitPrice != null && a.CalculationType == 2).Select(a => a.SUMMaxUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMLastUnitPrice != null && a.CalculationType == 3).Select(a => a.SUMLastUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMCustomeUnitPrice != null && a.CalculationType == 4).Select(a => a.SUMCustomeUnitPrice).DefaultIfEmpty(0).Sum() ?? 0;
                                    ItemOBJ.ExpDate = Common.GetInventoryItemExpDateFromMatrialAddingOrder(item.ID, _Context);

                                    ItemOBJ.MarketName = V_InventoryItemObjDB.MarketName ?? "";
                                    ItemOBJ.Category = V_InventoryItemObjDB.CategoryName;
                                    ItemOBJ.RUOM = V_InventoryItemObjDB.RequestionUomshortName;
                                    ItemOBJ.CommercialName = V_InventoryItemObjDB.CommercialName ?? "";
                                    ItemOBJ.PartNO = V_InventoryItemObjDB.PartNo ?? "";
                                    ItemOBJ.Cost1 = V_InventoryItemObjDB.CostAmount1 ?? 0;
                                    ItemOBJ.Cost2 = V_InventoryItemObjDB.CostAmount2 ?? 0;
                                    ItemOBJ.Cost3 = V_InventoryItemObjDB.CostAmount3 ?? 0;
                                    ItemOBJ.ItemSerialCounter = V_InventoryItemObjDB.ItemSerialCounter != null ? V_InventoryItemObjDB.ItemSerialCounter.ToString() : "";

                                    // fill Inventory List ------------------
                                    InventoryStoreItemList.Add(ItemOBJ);
                                }
                            }
                            NoOfItems += InventoryStoreItemPagingList.Select(x => x.ID).Distinct().Count();
                        }






                        //if (InventoryStoreItemPagingList.Count > 0)
                        //{

                        //    var InventoryItemList = InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().ToList();
                        //    if (headers["MatrialAddingOrderSerial"] != null && headers["MatrialAddingOrderSerial"] != "")
                        //    {
                        //        InventoryItemList = null;
                        //        string MatrialAddingOrderSerial = headers["MatrialAddingOrderSerial"];
                        //        InventoryItemList = _Context.V_InventoryAddingOrderItems.Where(x => x.ItemSerial.Trim() == MatrialAddingOrderSerial.Trim()).Select(x => x.InventoryItemID).ToList();
                        //    }
                        //    foreach (var InventoryItemID in InventoryItemList)
                        //    {

                        //        var InventoryStoreItemOBJ = new InventoryStoreItem();
                        //        decimal StockBalanceValue = 0;

                        //        int CalculationType = 0;
                        //        var ItemPErInventoryObj = InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID).FirstOrDefault();
                        //        if (ItemPErInventoryObj != null)
                        //        {
                        //            CalculationType = ItemPErInventoryObj.CalculationType != null ? (int)ItemPErInventoryObj.CalculationType : 0;
                        //            InventoryStoreItemOBJ.ID = ItemPErInventoryObj.InventoryItemID;
                        //            InventoryStoreItemOBJ.ItemName = ItemPErInventoryObj.InventoryItemName;
                        //            InventoryStoreItemOBJ.ItemCode = ItemPErInventoryObj.Code;
                        //            InventoryStoreItemOBJ.RequestionUOMShortName = ItemPErInventoryObj.RequestionUOMShortName;
                        //        }
                        //        if (CalculationType == 1)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMAverageUnitPrice != null).Sum(x => (decimal)x.SUMAverageUnitPrice);
                        //        }
                        //        else if (CalculationType == 2)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMMaxUnitPrice != null).Sum(x => (decimal)x.SUMMaxUnitPrice);
                        //        }
                        //        else if (CalculationType == 3)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMLastUnitPrice != null).Sum(x => (decimal)x.SUMLastUnitPrice);
                        //        }
                        //        else if (CalculationType == 4)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMCustomeUnitPrice != null).Sum(x => (decimal)x.SUMCustomeUnitPrice);
                        //        }
                        //        InventoryStoreItemOBJ.StockBalance = InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.Balance != null).Sum(x => (decimal)x.Balance);
                        //        InventoryStoreItemOBJ.StockBalanceValue = StockBalanceValue;  // Unit Cost *  Qty Balance
                        //        InventoryStoreItemOBJ.UnitCost = ItemPErInventoryObj.CustomeUnitPrice != null ? (decimal)ItemPErInventoryObj.CustomeUnitPrice : 0;   // Unit Cost 
                        //        InventoryStoreItemOBJ.ExpDate = Common.GetInventoryItemExpDateFromMatrialAddingOrder(InventoryItemID);

                        //        // fill Inventory List ------------------
                        //        InventoryStoreItemList.Add(InventoryStoreItemOBJ);
                        //        TotalStockBalance += InventoryStoreItemOBJ.StockBalance;
                        //        TotalStockBalanceValue += InventoryStoreItemOBJ.StockBalanceValue;
                        //    }
                        //    NoOfItems += InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().Count();
                        //}


                        Response.TotalItems = Common.GetNoOFInventoryItem(_Context);
                        Response.TotalPricedItems = InventoryStoreItemWithFilterInventoryListDB.Select(x => x.InventoryItemId).Distinct().Count();
                        Response.TotalStockBalance = InventoryStoreItemListDB.Select(x => x.StockBalance).Sum() ?? 0;
                        Response.TotalStockBalanceValue =
                                                                 InventoryStoreItemListDB.Where(x => x.CalculationType == 1).Sum(a => a.SumaverageUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 2).Sum(a => a.SummaxUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 3).Sum(a => a.SumlastUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 4).Sum(a => a.SumcustomeUnitPrice) ?? 0;


                        //InventoryStoreItemListDB.Select(x => x.StockBalanceValue).Sum();

                    }
                    Response.InventoryStoreItemList = InventoryStoreItemList;
                    Response.NoOfItems = NoOfItems;


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

        [HttpGet("GetInventoryItem")] // transfered to inventoryItem Service
        public GetInventoryItemResponse GetInventoryItem([FromHeader] long InventoryItemID)
        {
            GetInventoryItemResponse Response = new GetInventoryItemResponse();
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

                    Response = _inventorytemService.GetInventoryItem(InventoryItemID);

                    
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

        [HttpGet("GetInventoryStore")] // transfered to inventoryItem Service
        public async Task<GetInventoryStoreResponse> GetInventoryStore()
        {
            GetInventoryStoreResponse response = new GetInventoryStoreResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                var GetInventoryStoreResponseList = new List<InventoryStoreData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var InventoryStoreDB = await _Context.InventoryStores.ToListAsync();


                        if (InventoryStoreDB != null && InventoryStoreDB.Count > 0)
                        {

                            foreach (var InventoryStoreDBOBJ in InventoryStoreDB)
                            {
                                var InventoryStoreDBResponse = new InventoryStoreData();

                                InventoryStoreDBResponse.ID = InventoryStoreDBOBJ.Id;

                                InventoryStoreDBResponse.Name = InventoryStoreDBOBJ.Name;

                                InventoryStoreDBResponse.Location = InventoryStoreDBOBJ.Location;

                                InventoryStoreDBResponse.Tel = InventoryStoreDBOBJ.Tel;

                                InventoryStoreDBResponse.Active = InventoryStoreDBOBJ.Active;




                                GetInventoryStoreResponseList.Add(InventoryStoreDBResponse);
                            }



                        }

                    }

                }
                response.InventoryStoreList = GetInventoryStoreResponseList;
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

        [HttpGet("GetInventoryStorePerID")] // transfered to inventoryItem Service
        public async Task<GetInventoryStorePerIDResponse> GetInventoryStorePerID()
        {
            GetInventoryStorePerIDResponse response = new GetInventoryStorePerIDResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                long StoreID = 0;
                if (!string.IsNullOrEmpty(Request.Headers["StoreID"]) && long.TryParse(Request.Headers["StoreID"], out StoreID))
                {
                    long.TryParse(Request.Headers["StoreID"], out StoreID);
                }

                var GetInventoryStoreResponseList = new InventoryStoreData();
                var GetInventoryLocationResponseList = new List<InventoryLocationData>();
                var GetInventoryKeeperResponseList = new List<InventoryKeeperData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var InventoryStoreDB = await _Context.InventoryStores.Where(x => x.Id == StoreID).FirstOrDefaultAsync();
                        var InventoryStoreKeepersDB = await _Context.InventoryStoreKeepers.Where(x => x.InventoryStoreId == StoreID).ToListAsync();
                        var InventoryStoreLocationDB = await _Context.InventoryStoreLocations.Where(x => x.InventoryStoreId == StoreID).ToListAsync();


                        if (InventoryStoreDB != null)
                        {


                            var InventoryStoreDBResponse = new InventoryStorePerIDData();

                            InventoryStoreDBResponse.ID = InventoryStoreDB.Id;

                            InventoryStoreDBResponse.Name = InventoryStoreDB.Name;

                            InventoryStoreDBResponse.Location = InventoryStoreDB.Location;

                            InventoryStoreDBResponse.Tel = InventoryStoreDB.Tel;

                            InventoryStoreDBResponse.Active = InventoryStoreDB.Active;





                            foreach (var InventoryStoreKeepersDBOBJ in InventoryStoreKeepersDB)
                            {
                                var InventoryStoreKeepersDBResponse = new InventoryKeeperData();

                                InventoryStoreKeepersDBResponse.ID = InventoryStoreKeepersDBOBJ.Id;

                                InventoryStoreKeepersDBResponse.InventoryStoreID = InventoryStoreKeepersDBOBJ.InventoryStoreId;

                                InventoryStoreKeepersDBResponse.UserID = (int)InventoryStoreKeepersDBOBJ.UserId;

                                InventoryStoreKeepersDBResponse.UserName = Common.GetUserName(InventoryStoreKeepersDBOBJ.UserId, _Context);


                                InventoryStoreKeepersDBResponse.Active = InventoryStoreKeepersDBOBJ.Active;




                                GetInventoryKeeperResponseList.Add(InventoryStoreKeepersDBResponse);
                            }
                            InventoryStoreDBResponse.inventoryKeeperData = GetInventoryKeeperResponseList;



                            foreach (var InventoryStoreLocationDBOBJ in InventoryStoreLocationDB)
                            {
                                var InventoryStoreLocationDBResponse = new InventoryLocationData();

                                InventoryStoreLocationDBResponse.ID = InventoryStoreLocationDBOBJ.Id;

                                InventoryStoreLocationDBResponse.Location = InventoryStoreLocationDBOBJ.Location;


                                InventoryStoreLocationDBResponse.Active = InventoryStoreLocationDBOBJ.Active;

                                GetInventoryLocationResponseList.Add(InventoryStoreLocationDBResponse);
                            }

                            InventoryStoreDBResponse.inventoryLocationData = GetInventoryLocationResponseList;

                            response.InventoryStoreObject = InventoryStoreDBResponse;

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

        /*[HttpGet("GetInventoryItemMatrialAddingAndExternalOrderList")]
        public InventoryItemMatrialAddingOrder GetInventoryItemMatrialAddingAndExternalOrderList()
        {
            InventoryItemMatrialAddingOrder Response = new InventoryItemMatrialAddingOrder();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;


                var inventoryMtrialAddingOrderByDateList = new List<InventoryMtrialAddingOrderByDate>();
                if (Response.Result)
                {

                    // filters List InternalBackOrder
                    long InventoryItemID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["InventoryItemID"]) && long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID))
                    {
                        long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID);
                    }

                    long InventoryStoreID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["ToInventoryStoreID"]) && long.TryParse(Request.Headers["ToInventoryStoreID"], out InventoryStoreID))
                    {
                        long.TryParse(Request.Headers["ToInventoryStoreID"], out InventoryStoreID);
                    }


                    long FromSupplierID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["FromSupplierID"]) && long.TryParse(Request.Headers["FromSupplierID"], out FromSupplierID))
                    {
                        long.TryParse(Request.Headers["FromSupplierID"], out FromSupplierID);
                    }



                    long CreatorUserID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["CreatorUserID"]) && long.TryParse(Request.Headers["CreatorUserID"], out CreatorUserID))
                    {
                        long.TryParse(Request.Headers["CreatorUserID"], out CreatorUserID);
                    }
                    string SupplierItemSerial = null;
                    if (!string.IsNullOrEmpty(Request.Headers["SupplierItemSerial"]))
                    {
                        SupplierItemSerial = Request.Headers["SupplierItemSerial"];
                    }

                    string OrderType = null;
                    if (!string.IsNullOrEmpty(Request.Headers["OrderType"]))
                    {
                        OrderType = Request.Headers["OrderType"];
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
                    if (!string.IsNullOrEmpty(Request.Headers["ReceiveDateFrom"]) && DateTime.TryParse(Request.Headers["ReceiveDateFrom"], out ReceiveDateFromTemp))
                    {
                        ReceiveDateFromTemp = DateTime.Parse(Request.Headers["ReceiveDateFrom"]);
                        ReceiveDateFrom = ReceiveDateFromTemp;
                    }

                    DateTime? ReceiveDateTo = null;
                    DateTime ReceiveDateToTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(Request.Headers["ReceiveDateTo"]) && DateTime.TryParse(Request.Headers["ReceiveDateTo"], out ReceiveDateToTemp))
                    {
                        ReceiveDateToTemp = DateTime.Parse(Request.Headers["ReceiveDateTo"]);
                        ReceiveDateTo = ReceiveDateToTemp;
                    }


                    // Grouped by DAte as Inquiry 
                    //var InventoryMatrialAddingOrder



                    var InventoryMatrialAddingOrder = _Context.InventoryAddingOrders.Include(a => a.CreatedByNavigation).AsQueryable();
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
                    if (InventoryStoreID != 0)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                    }

                    if (CreatorUserID != 0)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.CreatedBy == CreatorUserID).AsQueryable();
                    }

                    if (FromSupplierID != 0)
                    {
                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => x.SupplierId == FromSupplierID).AsQueryable();
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
                    if (InventoryItemID != 0)
                    {
                        var IDInventoryAddingOrder = _Context.InventoryAddingOrderItems.Where(x => x.InventoryItemId == InventoryItemID).Select(x => x.InventoryAddingOrderId).Distinct().ToList();

                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => IDInventoryAddingOrder.Contains(x.Id)).AsQueryable();
                    }
                    if (SupplierItemSerial != null)
                    {
                        var IDInventoryAddingOrder = _Context.InventoryAddingOrderItems.Where(x => x.ItemSerial == SupplierItemSerial).Select(x => x.InventoryAddingOrderId).Distinct().ToList();

                        InventoryMatrialAddingOrder = InventoryMatrialAddingOrder.Where(x => IDInventoryAddingOrder.Contains(x.Id)).AsQueryable();
                    }
                    var InventoryMatrialAddingOrderFiltered = InventoryMatrialAddingOrder.OrderByDescending(x => x.CreationDate).GroupBy(x => new { year = x.CreationDate.Year, month = x.CreationDate.Month }).ToList();









                    foreach (var InquiryPerMonth in InventoryMatrialAddingOrderFiltered)
                    {
                        var InventoryMatrialAddingOrderInfoList = new List<InventoryMatrialAddingOrderInfo>();

                        foreach (var Data in InquiryPerMonth)
                        {

                            InventoryMatrialAddingOrderInfoList.Add(new InventoryMatrialAddingOrderInfo
                            {
                                AddingOrderNo = Data.Id.ToString(),
                                SupplierName = Data.Supplier?.Name,
                                StoreName = Data.InventoryStore?.Name,
                                RecivingDate = Data.RecivingDate.ToShortDateString(),
                                CreationDate = Data.CreationDate.ToShortDateString(),
                                CreatorName = Data.CreatedByNavigation.FirstName + " " + Data.CreatedByNavigation.LastName  //Common.GetUserName(Data.CreatedBy),
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
                return Response;
            }
        }*/

        [HttpPost("AddNewInventoryItem")] // transfered to inventoryItem Service

        public BaseResponseWithID AddNewInventoryItem([FromForm] AddNewInventoryItemRequest request)
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

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.Data == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (request.Data.ID == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ID Is Required";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrEmpty(request.Data.ItemName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ItemName Is Required";
                        Response.Errors.Add(error);
                    }
                    //if (string.IsNullOrEmpty(Request.Data.ItemCode))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Item Code Is Required";
                    //    Response.Errors.Add(error);
                    //}
                    //if (!IsValidStringWithoutSpecialCharByRegex(Request.Data.ItemCode))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err25";
                    //    error.ErrorMSG = "Invalid Code Must be without any special char";
                    //    Response.Errors.Add(error);
                    //}
                    if (request.Data.CategoryId == null || request.Data.CategoryId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Item Category Is Required";
                        Response.Errors.Add(error);
                    }

                    if (request.Data.RequstionUOMID == null || request.Data.RequstionUOMID == 0)// Default set first
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "RequstionUOMID Is Required";
                        Response.Errors.Add(error);
                    }

                    if (request.Data.PurchasingUOMID == null || request.Data.PurchasingUOMID == 0) // Default set first
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "PurchasingUOMID Is Required";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        var InventoryItemQuerable = _Context.InventoryItems.Where(x => x.Active == true).AsQueryable();
                        // Check unique Name
                        var CheckItemsName = InventoryItemQuerable.Where(x => x.Name.Trim() == request.Data.ItemName.Trim() && x.Id != request.Data.ID).FirstOrDefault();
                        if (CheckItemsName != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "ItemName Is already exist";
                            Response.Errors.Add(error);
                        }
                        // Check unique Code
                        if (!string.IsNullOrWhiteSpace(request.Data.ItemCode))
                        {
                            var CheckItemsCode = InventoryItemQuerable.Where(x => x.Code.Trim() == request.Data.ItemCode.Trim() && x.Id != request.Data.ID).FirstOrDefault();
                            if (CheckItemsCode != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "ItemCode Is already exist";
                                Response.Errors.Add(error);
                            }
                        }
                        /*byte[] ImageBytes = null;
                        if (!string.IsNullOrEmpty(request.Data.ItemImage))
                        {
                            ImageBytes = Convert.FromBase64String(request.Data.ItemImage);
                        }*/

                        if (Response.Result)
                        {
                            long InventoryItemId = 0;
                            if (request.Data.ID == 0)
                            {
                                var item = new NewGaras.Infrastructure.Entities.InventoryItem();
                                item.Active = request.Data.Active;
                                item.CalculationType = request.Data.CalculationTypeID != null ? (int)request.Data.CalculationTypeID : 1;
                                if (item.CalculationType == 4 && request.Data.CustomAmount != null)
                                    item.CustomeUnitPrice = (decimal)request.Data.CustomAmount;
                                else
                                    item.CustomeUnitPrice = 0;
                                item.AverageUnitPrice = 0;
                                item.MaxUnitPrice = 0;
                                item.LastUnitPrice = 0;
                                item.CommercialName = request.Data.CommericalName;
                                item.CreatedBy = validation.userID;
                                item.CreationDate = DateTime.Now;
                                item.Description = request.Data.Description;
                                item.Details = request.Data.Details;
                                item.ExchangeFactor1 = request.Data.ConvertRateFromPurchasingToRequestionUnit;
                                item.Exported = request.Data.Type; // Local or Exported
                                item.InventoryItemCategoryId = (int)request.Data.CategoryId;
                                item.MarketName = request.Data.MarketName;
                                if (request.Data.MaxBlanace != null)
                                {
                                    item.MaxBalance1 = (decimal)request.Data.MaxBlanace;
                                }
                                else
                                {
                                    item.MaxBalance1 = 0;
                                }
                                if (request.Data.MinBalance != null)
                                {
                                    item.MinBalance1 = (decimal)request.Data.MinBalance;
                                }
                                else
                                {
                                    item.MinBalance1 = 0;
                                }
                                if (request.Data.CostAmount1 != null)
                                    item.CostAmount1 = (decimal)request.Data.CostAmount1;
                                if (request.Data.CostAmount2 != null)
                                    item.CostAmount2 = (decimal)request.Data.CostAmount2;
                                if (request.Data.CostAmount3 != null)
                                    item.CostAmount3 = (decimal)request.Data.CostAmount3;

                                item.ModifiedBy = validation.userID;
                                item.ModifiedDate = DateTime.Now;
                                item.PartNo = request.Data.PartNumber;
                                item.Name = request.Data.ItemName;

                                item.PurchasingUomid = (int)request.Data.PurchasingUOMID;
                                item.RequstionUomid = (int)request.Data.RequstionUOMID;

                                var NewItemSerial = _Context.InventoryItems.Max(p => p == null ? 0 : p.ItemSerialCounter) + 1;
                                item.ItemSerialCounter = NewItemSerial ?? 1;
                                if (request.Data.PriorityID != null && request.Data.PriorityID != 0)
                                {
                                    item.PriorityId = (int)request.Data.PriorityID;
                                }
                                if (item.ImageUrl != null)
                                {
                                    var oldpath = Path.Combine(_host.WebRootPath, item.ImageUrl);
                                    if (System.IO.File.Exists(oldpath))
                                    {
                                        System.IO.File.Delete(oldpath);
                                        item.ImageUrl = null;
                                    }
                                }
                                if (request.Data.Image != null)
                                {
                                    var fileExtension = request.Data.Image.FileName.Split('.').Last();
                                    var virtualPath = $@"Attachments\{validation.CompanyName}\InventoryItems\Images\";
                                    var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Data.Image.FileName.Trim().Replace(" ", ""));
                                    var AttachPath = Common.SaveFileIFF(virtualPath, request.Data.Image, FileName, fileExtension, _host);
                                    item.ImageUrl = AttachPath;
                                }
                                //if (FU_Photo.HasFile)
                                //{
                                //    item.Image = FU_Photo.FileBytes;
                                //    item.HasImage = true;
                                //}
                                //else
                                //    item.HasImage = false;
                                /*if (ImageBytes != null)
                                {
                                    item.Image = ImageBytes;
                                    item.HasImage = true;
                                }
                                else
                                    item.HasImage = false;*/
                                if (!string.IsNullOrWhiteSpace(request.Data.ItemCode))
                                {
                                    item.Code = request.Data.ItemCode;
                                }
                                else
                                {
                                    item.Code = "0";

                                }
                                _Context.InventoryItems.Add(item);
                                var Res = _Context.SaveChanges();
                                if (Res > 0)
                                {

                                    if (string.IsNullOrWhiteSpace(request.Data.ItemCode))
                                    {
                                        item.Code = item.Id.ToString(); // DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + item.ID;
                                        _Context.SaveChanges();
                                    }
                                    // Insert  ItemCategory
                                    var ItemCategory = _Context.InventoryItemCategories.Where(x => x.Id == request.Data.CategoryId).FirstOrDefault();
                                    ItemCategory.HaveItem = true;
                                    _Context.SaveChanges();

                                    //// Insert  ItemAttachment

                                    InventoryItemId = item.Id;
                                    Response.ID = item.Id;
                                }
                            }
                            else // Update 
                            {
                                var item = _Context.InventoryItems.Where(x => x.Id == request.Data.ID).FirstOrDefault();
                                if (item != null)
                                {
                                    item.Active = request.Data.Active;
                                    item.CalculationType = request.Data.CalculationTypeID != null ? (int)request.Data.CalculationTypeID : 1;
                                    if (item.CalculationType == 4 && request.Data.CustomAmount != null)
                                        item.CustomeUnitPrice = (decimal)request.Data.CustomAmount;
                                    else
                                        item.CustomeUnitPrice = 0;
                                    //item.AverageUnitPrice = 0;
                                    //item.MaxUnitPrice = 0;
                                    //item.LastUnitPrice = 0;
                                    item.CommercialName = request.Data.CommericalName;
                                    item.CreatedBy = validation.userID;
                                    item.CreationDate = DateTime.Now;
                                    item.Description = request.Data.Description;
                                    item.Details = request.Data.Details;
                                    item.Exported = request.Data.Type; // Local or Exported
                                    item.InventoryItemCategoryId = (int)request.Data.CategoryId;
                                    item.MarketName = request.Data.MarketName;
                                    if (request.Data.MaxBlanace != null)
                                    {
                                        item.MaxBalance1 = (decimal)request.Data.MaxBlanace;
                                    }
                                    else
                                    {
                                        item.MaxBalance1 = 0;
                                    }
                                    if (request.Data.MinBalance != null)
                                    {
                                        item.MinBalance1 = (decimal)request.Data.MinBalance;
                                    }
                                    else
                                    {
                                        item.MinBalance1 = 0;
                                    }
                                    if (request.Data.CostAmount1 != null)
                                        item.CostAmount1 = (decimal)request.Data.CostAmount1;
                                    if (request.Data.CostAmount2 != null)
                                        item.CostAmount2 = (decimal)request.Data.CostAmount2;
                                    if (request.Data.CostAmount3 != null)
                                        item.CostAmount3 = (decimal)request.Data.CostAmount3;

                                    item.ModifiedBy = validation.userID;
                                    item.ModifiedDate = DateTime.Now;
                                    item.PartNo = request.Data.PartNumber;
                                    item.Name = request.Data.ItemName;



                                    // validation check if have any movement on this item or purchase order cannot edit 
                                    if (item.PurchasingUomid != request.Data.PurchasingUOMID || item.RequstionUomid != request.Data.RequstionUOMID || item.ExchangeFactor1 != request.Data.ConvertRateFromPurchasingToRequestionUnit)
                                    {

                                        var ChkITEMinInventoryStoreItem = _Context.InventoryStoreItems.Where(x => x.InventoryItemId == item.Id).FirstOrDefault();
                                        var ChkITEMinPOItem = _Context.PurchasePoitems.Where(x => x.InventoryItemId == item.Id).FirstOrDefault();
                                        if (ChkITEMinInventoryStoreItem != null || ChkITEMinPOItem != null)
                                        {
                                            // Back Error

                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Can't update (Purchasing UOM , Requestion UOM or Convert Rate) for this Item becasue have more than movement or purchasing order";
                                            Response.Errors.Add(error);
                                        }

                                    }
                                    item.PurchasingUomid = (int)request.Data.PurchasingUOMID;
                                    item.RequstionUomid = (int)request.Data.RequstionUOMID;
                                    item.ExchangeFactor1 = request.Data.ConvertRateFromPurchasingToRequestionUnit;

                                    //var NewItemSerial = _Context.proc_InventoryItemLoadAll().Max(p => p == null ? 0 : p.ItemSerialCounter) + 1;
                                    //item.ItemSerialCounter = NewItemSerial ?? 1;
                                    if (request.Data.PriorityID != null && request.Data.PriorityID != 0)
                                    {
                                        item.PriorityId = (int)request.Data.PriorityID;
                                    }
                                    if (request.Data.Image != null)
                                    {
                                        var fileExtension = request.Data.Image.FileName.Split('.').Last();
                                        var virtualPath = $@"Attachments\{validation.CompanyName}\InventoryItems\Images\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Data.Image.FileName.Trim().Replace(" ", ""));
                                        var AttachPath = Common.SaveFileIFF(virtualPath, request.Data.Image, FileName, fileExtension, _host);
                                        item.ImageUrl = AttachPath;
                                    }
                                    /*if (ImageBytes != null)
                                    {
                                        item.Image = ImageBytes;
                                        item.HasImage = true;
                                    }*/
                                    //if (FU_Photo.HasFile)
                                    //{
                                    //    item.Image = FU_Photo.FileBytes;
                                    //    item.HasImage = true;
                                    //}
                                    //else
                                    //    item.HasImage = false;
                                    if (!string.IsNullOrWhiteSpace(request.Data.ItemCode))
                                    {
                                        item.Code = request.Data.ItemCode;
                                    }
                                    //_Context.InventoryItems.Add(item);
                                    var Res = _Context.SaveChanges();

                                    if (Res > 0)
                                    {
                                        // Insert  ItemCategory
                                        var ItemCategory = _Context.InventoryItemCategories.Where(x => x.Id == request.Data.CategoryId).FirstOrDefault();
                                        ItemCategory.HaveItem = true;
                                        _Context.SaveChanges();

                                        //// Insert  ItemAttachment

                                        InventoryItemId = item.Id;
                                        Response.ID = item.Id;
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "ID Is Not Found";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (request.Data.AttachmentsList != null)
                            {
                                foreach (var attachment in request.Data.AttachmentsList)
                                {

                                    if (attachment.Id != null && attachment.Active == false)
                                    {
                                        // Delete
                                        var AttachmentDb = _Context.InventoryItemAttachments.Where(x => x.Id == attachment.Id).FirstOrDefault();
                                        if (AttachmentDb != null)
                                        {
                                            // Ensure the second path doesn't start with any kind of directory separator
                                            var attachmentPath = AttachmentDb.AttachmentPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                            // Combine paths
                                            var oldpath = Path.Combine(_host.WebRootPath, attachmentPath);
                                            if (System.IO.File.Exists(oldpath))
                                            {
                                                System.IO.File.Delete(oldpath);
                                            }
                                            _Context.InventoryItemAttachments.Remove(AttachmentDb);
                                            _Context.SaveChanges();
                                        }
                                    }
                                    else
                                    {
                                        var fileExtension = attachment.File.FileName.Split('.').Last();
                                        var virtualPath = $@"Attachments\{validation.CompanyName}\InventoryItems\AttachmentFiles\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.File.FileName.Trim().Replace(" ", ""));
                                        var AttachPath = Common.SaveFileIFF(virtualPath, attachment.File, FileName, fileExtension, _host);
                                        var AttachmentDb = new NewGaras.Infrastructure.Entities.InventoryItemAttachment();
                                        AttachmentDb.InventoryItemId = InventoryItemId;
                                        AttachmentDb.FileExtenssion = fileExtension;
                                        AttachmentDb.FileName = FileName;
                                        AttachmentDb.AttachmentPath = AttachPath;
                                        AttachmentDb.CreatedBy = validation.userID;
                                        AttachmentDb.CreationDate = DateTime.Now;
                                        AttachmentDb.ModifiedBy = validation.userID;
                                        AttachmentDb.Modified = DateTime.Now;
                                        AttachmentDb.Type = fileExtension;
                                        AttachmentDb.Active = true;

                                        _Context.InventoryItemAttachments.Add(AttachmentDb);
                                        _Context.SaveChanges();

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

        [HttpGet("GetInventoryItemRejectedOfferSupplierList")] // transfered to inventoryItem Service
        public InventoryItemRejectedOfferSupplierResponse GetInventoryItemRejectedOfferSupplierList()
        {
            InventoryItemRejectedOfferSupplierResponse Response = new InventoryItemRejectedOfferSupplierResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                var ItemSupplierOfferRejectedList = new List<ItemRejectedOfferSupplier>();
                var ItemSupplierOfferAcceptedList = new List<ItemAcceptedOfferSupplier>();
                long InventoryItemID = 0;
                if (!string.IsNullOrEmpty(Request.Headers["InventoryItemID"]) && long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID))
                {
                    long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID);
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err99";
                    error.ErrorMSG = "Invalid Inventory Item ID";
                    Response.Errors.Add(error);
                    return Response;
                }

                long POID = 0;
                if (!string.IsNullOrEmpty(Request.Headers["POID"]) && long.TryParse(Request.Headers["POID"], out POID))
                {
                    long.TryParse(Request.Headers["POID"], out POID);
                }

                long SupplierID = 0;
                if (!string.IsNullOrEmpty(Request.Headers["SupplierID"]) && long.TryParse(Request.Headers["SupplierID"], out SupplierID))
                {
                    long.TryParse(Request.Headers["SupplierID"], out SupplierID);
                }
                if (Response.Result)
                {
                    var ListPRSupplietOfferItem = _Context.VPrsupplierOfferItems.Where(x => x.InventoryItemId == InventoryItemID).ToList();
                    var ListPRSupplierOfferItemAccepted = _Context.VPurchasePoItems.Where(x => x.InventoryItemId == InventoryItemID).ToList();
                    //.OrderBy(x => x.CreationDate).ToList();
                    // Filters --------
                    if (POID != 0)
                    {
                        ListPRSupplietOfferItem = ListPRSupplietOfferItem.Where(x => x.Poid == POID).ToList();
                        ListPRSupplierOfferItemAccepted = ListPRSupplierOfferItemAccepted.Where(x => x.Id == POID).ToList();
                    }
                    if (SupplierID != 0)
                    {
                        ListPRSupplietOfferItem = ListPRSupplietOfferItem.Where(x => x.SupplierId == SupplierID).ToList();
                        ListPRSupplierOfferItemAccepted = ListPRSupplierOfferItemAccepted.Where(x => x.ToSupplierId == SupplierID).ToList();
                    }
                    if (ListPRSupplietOfferItem.Count > 0)
                    {
                        foreach (var item in ListPRSupplietOfferItem)
                        {
                            var ItemObj = new ItemRejectedOfferSupplier();
                            ItemObj.POID = item.Poid;
                            ItemObj.SupplierID = item.SupplierId;
                            ItemObj.SupplierName = item.SupplierName;
                            ItemObj.CurrencyID = item.CurrencyId;
                            ItemObj.RecivedQuantity = item.RecivedQuantity;
                            ItemObj.ReqQuantity = item.ReqQuantity;
                            ItemObj.EstimatedCost = item.EstimatedCost;
                            ItemObj.RateToEGP = item.RateToEgp;
                            ItemObj.ItemComment = item.Comment;
                            ItemObj.SupplierOfferComment = item.SupplierOfferComment;
                            ItemObj.CreationDate = item.CreationDate.ToShortDateString();
                            ItemObj.CurrencyName = Common.GetCurrencyName(item.CurrencyId ?? 0, _Context);
                            ItemSupplierOfferRejectedList.Add(ItemObj);
                        }
                    }

                    if (ListPRSupplierOfferItemAccepted.Count > 0)
                    {
                        foreach (var item in ListPRSupplierOfferItemAccepted)
                        {
                            var ItemObj = new ItemAcceptedOfferSupplier();
                            ItemObj.POID = item.Id;
                            ItemObj.SupplierID = item.ToSupplierId;
                            ItemObj.SupplierName = Common.GetSupplierName(item.ToSupplierId ?? 0, _Context);
                            ItemObj.CurrencyID = item.CurrencyId;
                            ItemObj.RecivedQuantity = item.RecivedQuantity;
                            ItemObj.ReqQuantity = item.ReqQuantity;
                            ItemObj.EstimatedCost = item.EstimatedCost;
                            ItemObj.FinalUnitCost_EGP = item.FinalUnitCost;
                            ItemObj.ActualUnitCost_EGP = item.ActualUnitPrice;
                            ItemObj.RateToEGP = item.RateToEgp;
                            ItemObj.ItemComment = item.Comments;
                            ItemObj.CreationDate = item.PocreationDate != null ? ((DateTime)item.PocreationDate).ToShortDateString() : "";
                            ItemObj.CurrencyName = Common.GetCurrencyName(item.CurrencyId ?? 0, _Context);
                            ItemSupplierOfferAcceptedList.Add(ItemObj);
                        }
                    }
                }
                Response.ItemRejectedOfferSupplierList = ItemSupplierOfferRejectedList;
                Response.ItemAcceptedOfferSupplierList = ItemSupplierOfferAcceptedList;
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

        [HttpGet("GetRemainInventoryItemRequestedQty")] // transfered to inventoryItem Service

        public GetRemainInventoryItemRequestedQtyResponse GetRemainInventoryItemRequestedQty()
        {
            GetRemainInventoryItemRequestedQtyResponse Response = new GetRemainInventoryItemRequestedQtyResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                long InventoryItemId = 0;
                if (!string.IsNullOrEmpty(Request.Headers["InventoryItemId"]) && long.TryParse(Request.Headers["InventoryItemId"], out InventoryItemId))
                {
                    long.TryParse(Request.Headers["InventoryItemId"], out InventoryItemId);

                    var InventoryItemDb = _Context.InventoryItems.Find(InventoryItemId);
                    if (InventoryItemDb != null)
                    {
                        Response.InventoryItemId = InventoryItemId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "InventoryItem Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err99";
                    error.ErrorMSG = "InventoryItemId Is Mandatory";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (Response.Result)
                {

                    decimal TotalInventoryItemRequestedQty = 0;

                    var RemainOpenProjectsRequestedQtyDb = _Context.VInventoryMatrialReleaseItems.Where(a => a.InventoryItemId == InventoryItemId && a.ReqQuantity > 0 && a.ReqQuantity > a.RecivedQuantity).ToList().GroupBy(a => a.ProjectId).ToList();
                    if (RemainOpenProjectsRequestedQtyDb != null && RemainOpenProjectsRequestedQtyDb.Count() > 0)
                    {
                        var RemainOpenProjectsRequestedQtyList = RemainOpenProjectsRequestedQtyDb.Select(project => new OpenProjectRemainRequestedItem
                        {
                            ProjectId = project.Key ?? 0,
                            ClientId = project.Select(a => a.ClientId).FirstOrDefault(),
                            ClientName = project.Select(a => a.ClientName).FirstOrDefault(),
                            RemainRequestedQty = project.Sum(a => a.ReqQuantity ?? 0) - project.Sum(a => a.RecivedQuantity ?? 0)
                        }).ToList();
                        Response.OpenProjectsRemainRequestedItem = RemainOpenProjectsRequestedQtyList;
                        Response.TotalOpenProfjectsRemainRequestedItemsQty = RemainOpenProjectsRequestedQtyList.Sum(a => a.RemainRequestedQty);
                        TotalInventoryItemRequestedQty += Response.TotalOpenProfjectsRemainRequestedItemsQty;
                    }

                    var OpenSalesOffersRequestedQtyDb = _Context.VSalesOfferProductSalesOffers.Where(a => a.SalesOfferActive == true && a.InventoryItemId == InventoryItemId && a.Quantity > 0 && a.Status.ToLower() != "closed" && a.Status.ToLower() != "rejected").ToList().GroupBy(a => a.OfferId).ToList();
                    if (OpenSalesOffersRequestedQtyDb != null && OpenSalesOffersRequestedQtyDb.Count() > 0)
                    {
                        var OpenSalesOffersRequestedQtyList = OpenSalesOffersRequestedQtyDb.Select(salesOffer => new OpenSalesOfferRequestedItem
                        {
                            SalesOfferId = salesOffer.Key,
                            ClientId = salesOffer.Select(a => a.ClientId).FirstOrDefault(),
                            ClientName = Common.GetClientName(salesOffer.Select(a => a.ClientId).FirstOrDefault() ?? 0, _Context),
                            RequestedQty = (decimal)salesOffer.Sum(a => a.Quantity ?? 0)
                        }).ToList();
                        Response.OpenSalesOffersRequestedItem = OpenSalesOffersRequestedQtyList;
                        Response.TotalOpenSalesOffersRequestedItemsQty = OpenSalesOffersRequestedQtyList.Sum(a => a.RequestedQty);
                        TotalInventoryItemRequestedQty += Response.TotalOpenSalesOffersRequestedItemsQty;
                    }

                    Response.TotalInventoryItemRequestedQty = TotalInventoryItemRequestedQty;
                    // var HoldItems = _Context.V_InventoryMatrialRequestItems.Where(x => x.InventoryItemID == InventoryItemId && x.IsHold == true).ToList();
                    var InvStoreItemHoldQTY = _Context.InventoryStoreItems.Where(x => x.InventoryItemId == InventoryItemId && x.HoldQty != null).Select(x => x.HoldQty ?? 0).ToList().Sum();
                    if (InvStoreItemHoldQTY > 0)
                    {
                        Response.TotalStocksHoldItemsQty = InvStoreItemHoldQTY; // HoldItems.Sum(x => x.ReqQuantity ?? 0);
                    }
                    var StockAvailableItems = _Context.VInventoryStoreItems.Where(a => a.InventoryItemId == InventoryItemId && a.FinalBalance > 0 && a.StoreActive == true).ToList();
                    if (StockAvailableItems != null && StockAvailableItems.Count() > 0)
                    {
                        Response.TotalStocksAvailableItemsQty = StockAvailableItems.Select(x => x.FinalBalance ?? 0).ToList().Sum();
                    }

                    Response.TotalAvailableItemsQty = Response.TotalStocksAvailableItemsQty - Response.TotalStocksHoldItemsQty - Response.TotalInventoryItemRequestedQty;
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

        [HttpGet("GetAccountAndFinanceInventoryItemMovementListV2")] // transfered to inventoryItem Service
        public AccountAndFinanceInventoryItemMovementResponse GetAccountAndFinanceInventoryItemMovementListV2()
        {
            AccountAndFinanceInventoryItemMovementResponse Response = new AccountAndFinanceInventoryItemMovementResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
                //WebHeaderCollection headers = request.Headers;
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                var InventoryStoreItemMovmentList = new List<ItemMovement>();
                if (Response.Result)
                {

                    int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    }

                    int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }

                    long InventoryItemID = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["InventoryItemID"]) && long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID))
                    {
                        long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID);
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item ID";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    //DateTime? FromDateFilter = null;
                    //DateTime FromDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(Request.Headers["FromDate"]) && DateTime.TryParse(Request.Headers["FromDate"], out FromDateTemp))
                    //{
                    //    FromDateTemp = DateTime.Parse(Request.Headers["FromDate"]);
                    //    // FromDateFilter = FromDateTemp;
                    //}


                    //DateTime? ToDateFilter = null;
                    //DateTime ToDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(Request.Headers["ToDate"]) && DateTime.TryParse(Request.Headers["ToDate"], out ToDateTemp))
                    //{
                    //    ToDateTemp = DateTime.Parse(Request.Headers["ToDate"]);
                    //    //ToDateFilter = ToDateTemp;
                    //}


                    DateTime FromDateFilter = new DateTime(DateTime.Now.Year, 1, 1);  // Bishoy magdy modifications 2024-10-14
                    //DateTime FromDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(Request.Headers["FromDate"]) && DateTime.TryParse(Request.Headers["FromDate"], out FromDateFilter))
                    {
                        DateTime.TryParse(Request.Headers["FromDate"], out FromDateFilter);
                    }


                    DateTime ToDateFilter = DateTime.Now;
                    //DateTime ToDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(Request.Headers["ToDate"]) && DateTime.TryParse(Request.Headers["ToDate"], out ToDateFilter))
                    {
                        DateTime.TryParse(Request.Headers["ToDate"], out ToDateFilter);
                    }






                    long ClientId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["ClientId"]) && long.TryParse(Request.Headers["ClientId"], out ClientId))
                    {
                        long.TryParse(Request.Headers["ClientId"], out ClientId);
                    }
                    long SupplierId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["SupplierId"]) && long.TryParse(Request.Headers["SupplierId"], out SupplierId))
                    {
                        long.TryParse(Request.Headers["SupplierId"], out SupplierId);
                    }
                    long PoId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["PoId"]) && long.TryParse(Request.Headers["PoId"], out PoId))
                    {
                        long.TryParse(Request.Headers["PoId"], out PoId);
                    }
                    long ProjectId = 0;
                    if (!string.IsNullOrEmpty(Request.Headers["ProjectId"]) && long.TryParse(Request.Headers["ProjectId"], out ProjectId))
                    {
                        long.TryParse(Request.Headers["ProjectId"], out ProjectId);
                    }
                    string OperationType = null;
                    if (!string.IsNullOrEmpty(Request.Headers["OperationType"]))
                    {
                        OperationType = Request.Headers["OperationType"];
                    }
                    //long POID = 0;
                    //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
                    //{
                    //    long.TryParse(headers["POID"], out POID);
                    //}
                    decimal cummlativeQty = 0;
                    if (Response.Result)
                    {

                        var InventoryItemMovmentQuerable = _Context.VInventoryStoreItemMovements.Where(x => x.Active == true && x.InventoryItemId == InventoryItemID).OrderBy(x => x.CreationDate).AsQueryable();
                        // Filters --------
                        if (OperationType != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.OperationType.Contains(OperationType));
                        }

                        if (PoId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.AddingFromPoid == PoId);
                        }
                        // Filter 
                        if (!string.IsNullOrEmpty(Request.Headers["FromDate"]))
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= FromDateFilter);
                        }

                        if (!string.IsNullOrEmpty(Request.Headers["ToDate"]))
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= ToDateFilter);
                        }

                        if (ClientId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ClientId == ClientId);
                        }

                        if (SupplierId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.SupplierId == SupplierId);
                        }
                        if (ProjectId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ProjectId == ProjectId);
                        }


                        var InventoryStoreItemPagingList = PagedList<VInventoryStoreItemMovement>.Create(InventoryItemMovmentQuerable, CurrentPage, NumberOfItemsPerPage);
                        //var InventoryStoreItemPagingList = PagedList<V_InventoryStoreItem>.Create(ListInventoryItemMovmentQuerable, CurrentPage, NumberOfItemsPerPage);

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = CurrentPage,
                            TotalPages = InventoryStoreItemPagingList.TotalPages,
                            ItemsPerPage = NumberOfItemsPerPage,
                            TotalItems = InventoryStoreItemPagingList.TotalCount
                        };

                        InventoryStoreItemMovmentList = InventoryStoreItemPagingList.Select(item => new ItemMovement
                        {

                            OperationType = item.OperationType,
                            Qty = (double)item.Balance,
                            HoldQty = item.HoldQty ?? 0,
                            HoldComment = item.HoldReason,
                            OrderID = item.OrderId,
                            CumilativeQty = (double)InventoryItemMovmentQuerable.Where(x => x.CreationDate <= item.CreationDate).ToList().Select(x => x.Balance).DefaultIfEmpty(0).Sum(),
                            StoreName = item.InventoryStoreName,
                            ReqUOM = item.RequstionUomname,

                            ID = item.Id,
                            ParentID = item.ReleaseParentId,
                            POID = item.AddingFromPoid,
                            ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "",
                            ItemSerial = item.ItemSerial,
                            RemainBalance = item.FinalBalance,
                            CurrencyId = item.CurrencyId,
                            CurrencyName = item.CurrencyName,
                            RateToEGP = item.RateToEgp,
                            POInvoicePriceEGP = item.PoinvoiceTotalPriceEgp,
                            POInvoiceUnitCostEGP = item.PoinvoiceTotalCostEgp,
                            CreationDate = item.CreationDate.ToShortDateString(),
                            FromUser = item.FromUser,
                            FromSupplier = item.FromSupplier,
                            SupplierId = item.SupplierId,
                            FromDepartment = item.FromDepartment,
                            OrderType = item.OrderType,
                            DateFilter = item.DateFilter ?? item.CreationDate,
                            Date = item.DateFilter != null ? item.DateFilter?.ToString("dd-MM-yyyy") : item.CreationDate.ToString("dd-MM-yyyy"),
                            ProjectName = item.ProjectName,
                            ProjectId = item.ProjectId,
                            ClientId = item.ClientId,
                            ClientName = item.ClientName

                        }).ToList();
                        //var ListInventoryItemMovment = _Context.V_InventoryStoreItem.Where(x => x.Active == true && x.InventoryItemID == InventoryItemID).OrderBy(x => x.CreationDate).ToList(); 
                        //if (InventoryStoreItemPagingList.Count > 0)
                        //{
                        //    //  var ListInventoryItemMovmentWithOperationAndItem = ListInventoryItemMovment.Select(x => { OrderID =x.OrderID ,OperationType = x.OperationType}).;
                        //    //var V_InventoryAddingOrder =   _Context.V_InventoryAddingOrder.Where()
                        //    // Kan fi Moshkla Fi Double w Decimal fi CumilativeQty kant bttr7 0.1 zyada w sal7taha By Mark Shawky
                        //    //var ListOfStoreItemQTyWithDate = ListInventoryItemMovment.Select(x => new { x.CreationDate, x.Balance }).ToList();
                        //    foreach (var item in InventoryStoreItemPagingList)
                        //    {
                        //        var ItemMovmentObj = new ItemMovement();
                        //        ItemMovmentObj.OperationType = item.OperationType;
                        //        ItemMovmentObj.Qty = (double)item.Balance;
                        //        ItemMovmentObj.HoldQty = item.holdQty ?? 0;
                        //        ItemMovmentObj.HoldComment = item.holdReason;
                        //        ItemMovmentObj.OrderID = item.OrderID;
                        //        cummlativeQty = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= item.CreationDate).Select(x => x.Balance).DefaultIfEmpty(0).Sum();
                        //        //cummlativeQty + (decimal)ItemMovmentObj.Qty;
                        //        ItemMovmentObj.CumilativeQty = (double)cummlativeQty;
                        //        ItemMovmentObj.StoreName = item.InventoryStoreName;
                        //        ItemMovmentObj.ReqUOM = item.RequstionUOMName;

                        //        // Extra DAta PO Item
                        //        ItemMovmentObj.ID = item.ID;
                        //        ItemMovmentObj.ParentID = item.releaseParentId;
                        //        ItemMovmentObj.POID = item.addingFromPOId;
                        //        ItemMovmentObj.ExpDate = item.expDate != null ? ((DateTime)item.expDate).ToShortDateString() : "";
                        //        ItemMovmentObj.ItemSerial = item.itemSerial;
                        //        ItemMovmentObj.RemainBalance = item.finalBalance;
                        //        ItemMovmentObj.CurrencyId = item.currencyId;
                        //        ItemMovmentObj.CurrencyName = item.CurrencyName; // Common.GetCurrencyName(item.currencyId ?? 0);
                        //        ItemMovmentObj.RateToEGP = item.rateToEGP;
                        //        ItemMovmentObj.POInvoicePriceEGP = item.POInvoiceTotalPriceEGP;
                        //        ItemMovmentObj.POInvoiceUnitCostEGP = item.POInvoiceTotalCostEGP;
                        //        ItemMovmentObj.CreationDate = item.CreationDate.ToShortDateString();
                        //        if (item.Balance > 0)
                        //        {
                        //            ItemMovmentObj.remainItemCostEGP = item.remainItemCosetEGP;
                        //            ItemMovmentObj.remainItemCostOtherCU = item.remainItemCostOtherCU;

                        //        }
                        //        ItemMovmentObj.FromUser = item.FromUser;
                        //        ItemMovmentObj.FromSupplier = item.FromSupplier;
                        //        ItemMovmentObj.SupplierId = item.SupplierId;
                        //        ItemMovmentObj.FromDepartment = item.FromDepartment;
                        //        ItemMovmentObj.OrderType = item.OrderType;
                        //            ItemMovmentObj.DateFilter = item.DateFilter;
                        //            ItemMovmentObj.Date = item.DateFilter?.ToString("dd-MM-yyyy");
                        //        // V_InventoryInternalBackOrder or MatrialRelease
                        //        ItemMovmentObj.ProjectName = item.ProjectName;
                        //        ItemMovmentObj.ProjectId = item.ProjectId;
                        //        ItemMovmentObj.ClientId = item.ClientID;
                        //        ItemMovmentObj.ClientName = item.ClientName;

                        //        InventoryStoreItemMovmentList.Add(ItemMovmentObj);
                        //    }

                        //}


                        //DateTime DateFrom = DateTime.Now;
                        //DateTime DateTo = DateTime.Now;
                        //var DateFromTemp = InventoryItemMovmentQuerable.Where(x => x.DateFilter != null).OrderBy(x => x.DateFilter).Select(x => x.DateFilter).FirstOrDefault();
                        //var DateToTemp = InventoryItemMovmentQuerable.Where(x => x.DateFilter != null).OrderByDescending(x => x.DateFilter).Select(x => x.DateFilter).FirstOrDefault();
                        //if (DateFromTemp != null)
                        //{
                        //    DateFrom = (DateTime)DateFromTemp;
                        //}
                        //if (DateToTemp != null)
                        //{
                        //    DateTo = (DateTime)DateToTemp;
                        //}


                        //var InventoryStoreItemMovmentListFilter = InventoryStoreItemMovmentList.Where(x => x.DateFilter != null).OrderBy(x => x.DateFilter).ToList();
                        //DateFrom = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).FirstOrDefault();
                        //DateTO = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).LastOrDefault();
                        var InventoryItemMovmentListFilter = InventoryItemMovmentQuerable;
                        //List<V_InventoryStoreItemMovement> InventoryItemMovmentListFilter = new List<V_InventoryStoreItemMovement>();
                        double numberOfMonths = Math.Abs(Math.Ceiling(ToDateFilter.Subtract(FromDateFilter).Days / (365.25 / 12)));
                        if (FromDateFilter <= ToDateFilter)
                        {
                            InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= FromDateFilter && x.CreationDate <= ToDateFilter).AsQueryable();
                        }
                        else
                        {
                            InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= ToDateFilter && x.CreationDate <= FromDateFilter).AsQueryable();
                        }

                        //if (InventoryItemMovmentListFilter.Count > 0)
                        //{
                        var ReleaseQty = InventoryItemMovmentListFilter.Where(x => x.OperationType.Contains("Release Order") ||
                                                           x.OperationType.Contains("POS Release")
                                                           ).ToList().Select(x => Math.Abs(x.Balance)).DefaultIfEmpty(0).Sum();
                        Response.ReleaseQty = (double)ReleaseQty;
                        Response.ReleaseRate = numberOfMonths != 0 ? (Response.ReleaseQty / numberOfMonths) : 0;
                        //}
                        Response.NoOfMonth = numberOfMonths;
                        Response.DateFrom = FromDateFilter.ToShortDateString();
                        //!string.IsNullOrWhiteSpace(headers["FromDate"]) ? FromDateTemp.ToString("dd-MM-yyyy") : DateFrom.ToString("dd-MM-yyy" + "+y");
                        Response.DateTo = ToDateFilter.ToShortDateString();
                        //!string.IsNullOrWhiteSpace(headers["ToDate"]) ? ToDateTemp.ToString("dd-MM-yyyy") : DateTO.ToString("dd-MM-yyyy");
                        Response.InventoryItemMovementList = InventoryStoreItemMovmentList;
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

        //[HttpGet("GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo")]
        //public InventoryItemSupplierMatrialAddingOrderInfoResponse GetInventoryItemSupplierMatrialAddingAndExternalOrderInfo()
        //{
        //    InventoryItemSupplierMatrialAddingOrderInfoResponse Response = new InventoryItemSupplierMatrialAddingOrderInfoResponse();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {
        //        //IncomingWebRequestContext request = WebOperationContext.Current.IncomingRequest;
        //        //WebHeaderCollection headers = request.Headers;
        //        HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
        //        Response.Errors = validation.errors;
        //        Response.Result = validation.result;


        //        var InventoryItemSupplierMatrialAddingOrderInfoObj = new InventoryItemSupplierMatrialAddingOrderInfo();
        //        var MatrialAddingOrderInfList = new List<MatrialAddingOrderInfo>();
        //        if (Response.Result)
        //        {
        //            long MatrialAddingOrderID = 0;
        //            if (!string.IsNullOrEmpty(Request.Headers["MatrialAddingOrderID"]) && long.TryParse(Request.Headers["MatrialAddingOrderID"], out MatrialAddingOrderID))
        //            {
        //                long.TryParse(Request.Headers["MatrialAddingOrderID"], out MatrialAddingOrderID);
        //            }
        //            else
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err109";
        //                error.ErrorMSG = "Invalid Matrial Adding Order ID";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }
        //            //long POID = 0;
        //            //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
        //            //{
        //            //    long.TryParse(headers["POID"], out POID);
        //            //}

        //            if (Response.Result)
        //            {
        //                var InventoryAddingOrderOBJDB = _Context.VInventoryAddingOrders.Where(x => x.Id == MatrialAddingOrderID).FirstOrDefault();
        //                if (InventoryAddingOrderOBJDB != null)
        //                {
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.InventoryAddingOrderID = InventoryAddingOrderOBJDB.Id;
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.OrderType = InventoryAddingOrderOBJDB.OperationType;
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.SupplierName = InventoryAddingOrderOBJDB.SupplierName;
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.ToStore = InventoryAddingOrderOBJDB.StoreName;
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.Revision = InventoryAddingOrderOBJDB.Revision;
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.CreationDate = InventoryAddingOrderOBJDB.CreationDate.ToShortDateString();
        //                    InventoryItemSupplierMatrialAddingOrderInfoObj.RecivingDate = InventoryAddingOrderOBJDB.RecivingDate.ToShortDateString();

        //                    var ListOfMatrialAddingOrderItemListDB = _Context.InventoryAddingOrderItems.Where(x => x.InventoryAddingOrderId == MatrialAddingOrderID).ToList();
        //                    if (ListOfMatrialAddingOrderItemListDB != null)
        //                    {
        //                        foreach (var item in ListOfMatrialAddingOrderItemListDB)
        //                        {
        //                            var MatrialAddingOrderInfoObj = new MatrialAddingOrderInfo();
        //                            MatrialAddingOrderInfoObj.Id = item.Id;
        //                            MatrialAddingOrderInfoObj.InventoryItemID = item.InventoryItemId;
        //                            MatrialAddingOrderInfoObj.ItemName = item.InventoryItem?.Name;
        //                            MatrialAddingOrderInfoObj.InventoryItemID = item.InventoryItemId;
        //                            MatrialAddingOrderInfoObj.RequireQTY = item.ReqQuantity != null ? item.ReqQuantity.ToString() : "0";
        //                            MatrialAddingOrderInfoObj.ReceivedOrReturnQTY = item.RecivedQuantity != null ? item.RecivedQuantity.ToString() : "0";
        //                            MatrialAddingOrderInfoObj.ReceivedQTYUOP = item.RecivedQuantityUop != null ? item.RecivedQuantityUop.ToString() : "0";
        //                            MatrialAddingOrderInfoObj.ReceivedQTYAfter = item.RecivedQuantityAfter != null ? item.RecivedQuantityAfter.ToString() : "0";
        //                            MatrialAddingOrderInfoObj.RemainQTY = item.RemainQuantity != null ? item.RemainQuantity.ToString() : "0";
        //                            MatrialAddingOrderInfoObj.UOM = item.Uom?.Name;
        //                            MatrialAddingOrderInfoObj.PurchaseUOM = item.InventoryItem?.PurchasingUom?.Name;
        //                            MatrialAddingOrderInfoObj.PONo = item.Poid != null ? (long)item.Poid : 0;
        //                            MatrialAddingOrderInfoObj.POItemComment = _Context.PurchasePoitems.Where(x => x.PurchasePoid == MatrialAddingOrderInfoObj.PONo && x.InventoryItemId == item.InventoryItemId).Select(x => x.Comments).FirstOrDefault();
        //                            MatrialAddingOrderInfoObj.SupplierItemSerial = item.ItemSerial;
        //                            MatrialAddingOrderInfoObj.Comment = item.Comments;
        //                            MatrialAddingOrderInfoObj.QIReport = item.QcreportId;
        //                            MatrialAddingOrderInfoObj.InventoryItemSerial = Common.GeyInventoryStoreItemSerial(item.InventoryItemId, _Context);
        //                            MatrialAddingOrderInfoObj.ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "";

        //                            MatrialAddingOrderInfList.Add(MatrialAddingOrderInfoObj);
        //                        }
        //                    }

        //                }

        //                InventoryItemSupplierMatrialAddingOrderInfoObj.MatrialAddingOrderInfList = MatrialAddingOrderInfList;

        //            }
        //            Response.InventoryItemInfo = InventoryItemSupplierMatrialAddingOrderInfoObj;


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


        [HttpGet("InventoryStoreItemReportWithTabs")] //converted to service
        public BaseResponseWithData<string> InventoryStoreItemReportWithTabs()
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;
                if (Response.Result)
                {


                    Response = _inventoryService.InventoryStoreItemReportWithTabs(validation.CompanyName);

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

        [HttpGet("GetPurchaseForStore")] //converted to service
        public BaseResponseWithData<string> GetPurchaseForStore([FromHeader] int? inventoryStoreID, [FromHeader] string DateFrom, [FromHeader] string DateTo, [FromHeader] bool? internalTransferFlag)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {

                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                response.Errors = validation.errors;
                response.Result = validation.result;

                if (response.Result)
                {
                    var data = _inventoryService.GetPurchaseForStoreReport(inventoryStoreID, DateFrom, DateTo, internalTransferFlag, validation.CompanyName);
                    if (data != null)
                    {
                        response = data;
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);

                return response;
            }
        }

        [HttpGet("GetInventoryItemMovementReport")] //converted to service
        public BaseResponseWithData<string> GetAccountAndFinanceInventoryItemStockBalance([FromHeader] int InventoryItemID, [FromHeader] string DateFrom, [FromHeader] string DateTo, [FromHeader] long? storeID)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var data = _inventoryService.GetInventoryItemMovementReport(InventoryItemID, DateFrom, DateTo, storeID, validation.CompanyName);
                    if (data != null)
                    {
                        Response = data;
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

        /*[HttpGet("test")]
        public BaseResponseWithDataAndHeader< AccountAndFinanceInventoryItemMovementResponse> GetAccountAndFinanceInventoryItemMovementListV2([FromHeader]AccountAndFinanceInventoryItemMovementListV2Filters filters)
        {
            BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse> Response = new BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var data = _inventoryService.GetAccountAndFinanceInventoryItemMovementListV2(filters);
                    if (data != null)
                    {
                        Response = data;
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
        }*/

        [HttpGet("GetInventoryItemRelaseRate")] //converted to service
        public BaseResponseWithData<string> GetInventoryItemRelaseRate([FromHeader] string DateFrom, [FromHeader] string DateTo)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var data = _inventoryService.GetInventoryItemRelaseRate(DateFrom, DateTo, validation.CompanyName);
                    if (data != null)
                    {
                        Response = data;
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

        [HttpGet("GetInventoryStoreItemByOrder")] //converted to service
        public BaseResponseWithData<InventoryStoreItemByOrderResponse> GetInventoryStoreItemByOrder([FromHeader] long OrderId, [FromHeader] string OperationType)
        {
            BaseResponseWithData<InventoryStoreItemByOrderResponse> Response = new BaseResponseWithData<InventoryStoreItemByOrderResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();


            try
            {
                HearderVaidatorOutput validation = _helper.ValidateHeader(Request.Headers, ref _Context);
                Response.Errors = validation.errors;
                Response.Result = validation.result;

                if (Response.Result)
                {
                    var data = _inventoryService.GetInventoryStoreItemByOrder(OrderId, OperationType);
                    if (data != null)
                    {
                        Response = data;
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
