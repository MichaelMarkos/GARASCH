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
using NewGarasAPI.Models.User;
using System.Net;
using NewGaras.Infrastructure.Models.Inventory;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.Models.Admin;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.Admin;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using DocumentFormat.OpenXml.Spreadsheet;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryItemCategoryService : IInventoryItemCategoryService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
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
        public InventoryItemCategoryService(GarasTestContext context, ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {
            _Context = context;
            _tenantService = tenantService;
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }
        public async Task<GetInventoryItemCategoryResponse> GetInventoryItemCategory(long UserId)
        {
            GetInventoryItemCategoryResponse response = new GetInventoryItemCategoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    var checkUserRole_IntenalTicket = Common.CheckUserRole(UserId, 169, _Context); //169 Internal Ticket in category Type 1
                    var InventoryItemCategory_querable = _unitOfWork.InventoryItemCategories.FindAllQueryable(a => true, includes: new[] { "InventoryItems", "CategoryType" });
                    var CategoryTypeIDsList = new List<int>();
                    if (checkUserRole_IntenalTicket)//169 Internal Ticket in category Type 1
                    {
                        CategoryTypeIDsList.Add(1);
                    }
                    //else if() // second type with second role
                    
                    InventoryItemCategory_querable = InventoryItemCategory_querable.Where(x => CategoryTypeIDsList.Contains(x.CategoryTypeId ?? 0) || x.CategoryTypeId == null);
                    var ItemCategoryListDB = InventoryItemCategory_querable.ToList();


                    var InventoryItemCategory = await InventoryItemCategory_querable.ToListAsync();
                    var IDSCatItem = InventoryItemCategory.Select(x => x.Id).ToList();
                    var Myproc_InventoryStoreItemCalcGroupingByCategoryList = _Context.Database.SqlQueryRaw<Myproc_InventoryStoreItemCalcGroupingByCategory_Result>("Exec Myproc_InventoryStoreItemCalcGroupingByCategory").AsEnumerable().Where(x => x.InventoryItemCategoryID != null && IDSCatItem.Contains((int)x.InventoryItemCategoryID)).ToList();
                    var TreeDtoObj = InventoryItemCategory.Select(c => new TreeViewDto2
                    {

                        id = c.Id.ToString(),
                        title = c.Name,
                        HaveItem = c.HaveItem,
                        ItemCount = c.HaveItem == true ? c.InventoryItems.Count() : 0,
                        parentId = c.ParentCategoryId.ToString(),
                        CategoryTypeId = c.CategoryTypeId,
                        CategoryTypeName = c.CategoryType?.Name,
                        SumOfRemainBalanceCostwithMainCu = Myproc_InventoryStoreItemCalcGroupingByCategoryList.Where(x => x.InventoryItemCategoryID == c.Id).Select(x => x.POInvTotalCost).FirstOrDefault(),
                        SumOfRemainBalanceCostwithEgp = Myproc_InventoryStoreItemCalcGroupingByCategoryList.Where(x => x.InventoryItemCategoryID == c.Id).Select(x => x.POInvTotalCostWithRate).FirstOrDefault()
                    }).ToList();
                    var trees = Common.BuildTreeViews2("", TreeDtoObj);
                    response.GetInventoryItemCategoryList = new List<TreeViewDto2>();
                    response.GetInventoryItemCategoryList = trees;

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

        public BaseResponseWithId<int> DeleteInventoryItemCategory([FromHeader] int CategoryId, [FromHeader] bool Active)
        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if(CategoryId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Category Id is required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var category = _unitOfWork.InventoryItemCategories.GetById(CategoryId);
                if (category == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Category not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                category.Active = Active;
                category.ModifiedDate = DateTime.Now;
                category.ModifiedBy = validation.userID;
                Response.ID = category.Id;
                _unitOfWork.InventoryItemCategories.Update(category);
                _unitOfWork.Complete();
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public SelectDDLResponse GetInventoryItemCategoryList(long UserId)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemCategoryList = new List<SelectDDL>();

                if (Response.Result)
                {
                    var checkUserRole_IntenalTicket = Common.CheckUserRole(UserId, 169, _Context); //169 Internal Ticket in category Type 1
                    var ItemCategoryQuerable = _unitOfWork.InventoryItemCategories.FindAllQueryable(a=>a.Active);
                    var CategoryTypeIDsList = new List<int>();
                    if (checkUserRole_IntenalTicket)//169 Internal Ticket in category Type 1
                    {
                        CategoryTypeIDsList.Add(1);
                    }
                    //else if() // second type with second role

                        ItemCategoryQuerable = ItemCategoryQuerable.Where(x => CategoryTypeIDsList.Contains(x.CategoryTypeId ?? 0) || x.CategoryTypeId == null);
                    var ItemCategoryListDB = ItemCategoryQuerable.ToList();
                    foreach (var item in ItemCategoryListDB)
                    {
                        var ItemCategoryrObj = new SelectDDL();
                        ItemCategoryrObj.ID = item.Id;
                        ItemCategoryrObj.Name = item.Name;
                        ItemCategoryList.Add(ItemCategoryrObj);
                    }
                    Response.DDLList = ItemCategoryList;
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

        public BaseResponseWithId<long> AddInventoryItemCategory(InventoryCategoryPerItemData Request,long creator)
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
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        var NewInventory = new InventoryItemCategory()
                        {
                            Name = Request.Name,
                            Description = Request.Description,
                            Active = Request.Active,
                            CreatedBy = creator,
                            CreationDate = DateTime.Now,
                            ModifiedBy = creator,
                            ModifiedDate = DateTime.Now,
                            ParentCategoryId = Request.ParentCategoryID,
                            HaveItem = false,
                            IsFinalProduct = Request.IsFinalProduct,
                            IsRentItem = Request.IsRentItem,
                            IsAsset = Request.IsAsset,
                            IsNonStock = Request.IsNonStock,
                            CategoryTypeId = Request.CategoryTypeId
                        };
                        _unitOfWork.InventoryItemCategories.Add(NewInventory);
                        var InventoryInserted = _unitOfWork.Complete();

                        if (InventoryInserted > 0)
                        {
                            var InventoryinstertedID = NewInventory.Id;
                        }

                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Faild To Insert this Inventory Item Category !!";
                            Response.Errors.Add(error);
                        }



                    };




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

        public async Task<BaseResponseWithId<long>> EditInventoryCategory(InventoryCategoryPerItemData Request,long creator)
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
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        if (Request.ID != 0 && Request.ID != null)
                        {
                            var InventoryCategoryDB = await _unitOfWork.InventoryItemCategories.GetByIdAsync((int)Request.ID);
                            if (InventoryCategoryDB.ParentCategoryId == null && Request.ParentCategoryID != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P13";
                                error.ErrorMSG = "Parent Can't Be a Child";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            else if (Request.ParentCategoryID == InventoryCategoryDB.Id)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P13";
                                error.ErrorMSG = "parent Id can't be the same as Category Id";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            if(InventoryCategoryDB.ParentCategoryId != null && (InventoryCategoryDB.CategoryTypeId != Request.CategoryTypeId))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P13";
                                error.ErrorMSG = "You can't change category type of a child";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            var InventoryItemCount = _unitOfWork.InventoryItems.FindAll(x => x.InventoryItemCategoryId == Request.ID).Count();
                            if (InventoryCategoryDB != null)
                            {
                                if (Response.Result == true)
                                {

                                    InventoryCategoryDB.Name = Request.Name;
                                    InventoryCategoryDB.Description = Request.Description;
                                    InventoryCategoryDB.Active = Request.Active;
                                    InventoryCategoryDB.ModifiedBy = creator;
                                    InventoryCategoryDB.ModifiedDate = DateTime.Now;
                                    if (Request.ParentCategoryID != null)
                                    {
                                        InventoryCategoryDB.ParentCategoryId = Request.ParentCategoryID;
                                    }
                                    InventoryCategoryDB.HaveItem = false;
                                    InventoryCategoryDB.IsFinalProduct = Request.IsFinalProduct;
                                    InventoryCategoryDB.IsRentItem = Request.IsRentItem;
                                    InventoryCategoryDB.IsAsset = Request.IsAsset;
                                    InventoryCategoryDB.IsNonStock = Request.IsNonStock;

                                    if (InventoryCategoryDB.ParentCategoryId == null)
                                    {
                                        InventoryCategoryDB.CategoryTypeId = Request.CategoryTypeId;
                                        var childItems = _unitOfWork.InventoryItemCategories.FindAll(a => a.ParentCategoryId == InventoryCategoryDB.Id).ToList();
                                        foreach(var childItem in childItems)
                                        {
                                            childItem.CategoryTypeId = Request.CategoryTypeId;
                                            _unitOfWork.InventoryItemCategories.Update(childItem);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                    _unitOfWork.InventoryItemCategories.Update(InventoryCategoryDB);
                                    var InventoryCategoryUpdate = _unitOfWork.Complete();
                                    if (InventoryCategoryUpdate > 0)
                                    {
                                        Response.Result = true;
                                        Response.ID = InventoryCategoryDB.Id;
                                    }

                                }


                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Inventory Category Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }

                    };




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

        public BaseResponseWithId<long> DeleteInventoryCategory(InventoryCategoryPerItemData Request)
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
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        if (Request.ID != 0 && Request.ID != null)
                        {
                            var InventoryCategoryDB = _unitOfWork.InventoryItemCategories.GetById((int)Request.ID);
                            var InventoryCategoryCount = _unitOfWork.InventoryItemCategories.FindAll(x => x.ParentCategoryId == Request.ID).Count();
                            var InventoryItemCount = _unitOfWork.InventoryItems.FindAll(x => x.InventoryItemCategoryId == Request.ID).Count();
                            if (InventoryCategoryDB != null)
                            {
                                // Update
                                if (InventoryCategoryCount > 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Inventory Category Has Child , please edit the Relation";
                                    Response.Errors.Add(error);
                                }
                                // Update
                                if (InventoryItemCount > 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Inventory Category Has Item , please edit the Relation";
                                    Response.Errors.Add(error);
                                }
                                if (Response.Result == true)
                                {
                                    {
                                        _unitOfWork.InventoryItemCategories.Delete(InventoryCategoryDB);
                                        _unitOfWork.Complete();
                                                                                                       

                                        if (Request.ID == null || Request.ID != InventoryCategoryDB.Id)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Update Category !!";
                                            Response.Errors.Add(error);
                                        }

                                    }
                                }


                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Inventory Category Doesn't Exist!!";
                                Response.Errors.Add(error);


                            }
                        }

                    };




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

        public BaseResponseWithData<List<SelectDDL>> GetCategoryTypesDDl()
        {
            BaseResponseWithData<List<SelectDDL>> Response = new BaseResponseWithData<List<SelectDDL>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var list = _unitOfWork.CategoryTypes.GetAll();

                var ddl = list.Select(x => new SelectDDL { Name = x.Name, ID=x.Id}).ToList();

                Response.Data = ddl;

                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public GetInventoryCategoryStoreItemResponse GetInventoryCategoryStoreItem([FromHeader] long InventoryItemCategoryID)
        {
            GetInventoryCategoryStoreItemResponse response = new GetInventoryCategoryStoreItemResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var InventoryStoreItemCategoryList = new List<InventoryItemCategoryVM>();
                var InventoryItemCategorySum = new InventoryItemCategoryOBJ();

                if (response.Result)
                {
                    
                    var SumOfRemainBalanceCostwithMainCu = _unitOfWork.VInventoryStoreItems.FindAll(x => x.InventoryItemCategoryId == InventoryItemCategoryID && x.PoinvoiceTotalCost != null && x.FinalBalance != null).Sum(x => x.PoinvoiceTotalCost * x.FinalBalance);
                    var SumOfRemainBalanceCostwithEgp = _unitOfWork.VInventoryStoreItems.FindAll(x => x.InventoryItemCategoryId == InventoryItemCategoryID && x.PoinvoiceTotalCost != null && x.FinalBalance != null && x.RateToEgp != null).Sum(x => x.PoinvoiceTotalCost * x.FinalBalance * x.RateToEgp);

                    InventoryItemCategorySum.SumOfRemainBalanceCostwithMainCu = SumOfRemainBalanceCostwithMainCu; // InventoryStoreItemCategoryList.Sum(x => x.RemainBalanceCostwithMainCu);
                    InventoryItemCategorySum.SumOfRemainBalanceCostwithEgp = SumOfRemainBalanceCostwithEgp; // InventoryStoreItemCategoryList.Sum(x => x.RemainBalanceCostwithEgp);
                    //InventoryItemCategorySum.Count = InventoryItemCategoryCount;




                }

                response.InventoryItemCategorySumm = InventoryItemCategorySum;
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

        public async Task<GetInventoryParentCategoryDDLResponse> GetInventoryPerItem([FromHeader] long InventoryID)
        {
            GetInventoryParentCategoryDDLResponse response = new GetInventoryParentCategoryDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetInventoryCategoryPerItemList = new List<InventoryCategoryPerItemData>();
                if (response.Result)
                {

                    var GetInventoryCategoryPerItemDB = (await _unitOfWork.InventoryItemCategories.FindAllAsync(x => x.Id == InventoryID, includes: new[] { "ParentCategory", "CategoryType" })).ToList();
                    var InventoryCategoryCount = _unitOfWork.InventoryItemCategories.FindAll(x => x.ParentCategoryId == InventoryID).Count();


                    if (GetInventoryCategoryPerItemDB != null)
                    {

                        foreach (var GetInventoryCategoryPerItemOBJ in GetInventoryCategoryPerItemDB)
                        {
                            var GetInventoryCategoryPerItemResponse = new InventoryCategoryPerItemData();



                            GetInventoryCategoryPerItemResponse.ID = (int)GetInventoryCategoryPerItemOBJ.Id;

                            GetInventoryCategoryPerItemResponse.Name = GetInventoryCategoryPerItemOBJ.Name;
                            GetInventoryCategoryPerItemResponse.ParentCategoryID = GetInventoryCategoryPerItemOBJ.ParentCategoryId;
                            if (GetInventoryCategoryPerItemOBJ.ParentCategoryId != null)
                            {
                                GetInventoryCategoryPerItemResponse.ParentName = GetInventoryCategoryPerItemOBJ.ParentCategory.Name;

                            }
                            GetInventoryCategoryPerItemResponse.IsRentItem = GetInventoryCategoryPerItemOBJ.IsRentItem;
                            GetInventoryCategoryPerItemResponse.IsNonStock = GetInventoryCategoryPerItemOBJ.IsNonStock;
                            GetInventoryCategoryPerItemResponse.IsFinalProduct = GetInventoryCategoryPerItemOBJ.IsFinalProduct;
                            GetInventoryCategoryPerItemResponse.IsAsset = GetInventoryCategoryPerItemOBJ.IsAsset;
                            GetInventoryCategoryPerItemResponse.HaveItem = GetInventoryCategoryPerItemOBJ.HaveItem;
                            GetInventoryCategoryPerItemResponse.Description = GetInventoryCategoryPerItemOBJ.Description;
                            GetInventoryCategoryPerItemResponse.Active = GetInventoryCategoryPerItemOBJ.Active;
                            GetInventoryCategoryPerItemResponse.CategoryTypeId = GetInventoryCategoryPerItemOBJ.CategoryTypeId;
                            GetInventoryCategoryPerItemResponse.CategoryTypeName = GetInventoryCategoryPerItemOBJ.CategoryType?.Name;

                            if (InventoryCategoryCount > 0)
                            {
                                GetInventoryCategoryPerItemResponse.HasChild = true;
                            }
                            else
                            {
                                GetInventoryCategoryPerItemResponse.HasChild = false;
                            }




                            GetInventoryCategoryPerItemList.Add(GetInventoryCategoryPerItemResponse);
                        }



                    }

                }


                response.InventoryCategoryPerItemList = GetInventoryCategoryPerItemList;
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

        public async Task<GetInventoryParentCategoryDDLResponse> GetInventoryParentCategoryDDL()
        {
            GetInventoryParentCategoryDDLResponse response = new GetInventoryParentCategoryDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetInventoryCategoryDDLList = new List<InventoryParentCategoryDDLData>();
                if (response.Result)
                {

                    var GetInventoryCategoryDDLListDB = (await _unitOfWork.InventoryItemCategories.FindAllAsync(x => x.HaveItem == false)).ToList();

                    if (GetInventoryCategoryDDLListDB != null)
                    {

                        foreach (var GetCategoryDDLOBJ in GetInventoryCategoryDDLListDB)
                        {
                            var GetInventoryCategoryDDLResponse = new InventoryParentCategoryDDLData();

                            GetInventoryCategoryDDLResponse.ID = (int)GetCategoryDDLOBJ.Id;

                            GetInventoryCategoryDDLResponse.Name = GetCategoryDDLOBJ.Name;

                            GetInventoryCategoryDDLList.Add(GetInventoryCategoryDDLResponse);
                        }
                    }

                }

                response.InventoryParentCategoryDDLList = GetInventoryCategoryDDLList;
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
    }
}
