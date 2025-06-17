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
using NewGaras.Infrastructure.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Net;
using System.Web;
using NewGaras.Infrastructure.Models.ItemsPricing;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
using NewGarasAPI.Models.Account;
using NewGaras.Infrastructure.Models.ItemsPricing.UsedInResponse;
using NewGaras.Infrastructure.Entities;

namespace NewGaras.Domain.Services.Inventory
{
    public class ItemsPricingService : IItemsPricingService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";

        public ItemsPricingService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;

        }

        public BaseResponse ManageInventoryStoreItemPricinig(AddOneInventoryStoreItemPricing Request, long UserID)
        {
            BaseResponse Response = new BaseResponse();
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

                    if (Request.IDSinventoryItem == null || Request.IDSinventoryItem == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-414";
                        error.ErrorMSG = "Invalid Inventory Item ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.Custom == null && Request.Price1 == null && Request.Price2 == null && Request.Price3 == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-424";
                        error.ErrorMSG = "you must be Insert at least one price.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {
                        var InventoryItemObjDB = _unitOfWork.InventoryItems.Find(a => a.Id == Request.IDSinventoryItem);
                        if (InventoryItemObjDB != null)
                        {
                            decimal CustomeUnitPrice = InventoryItemObjDB.CustomeUnitPrice;
                            if (Request.Custom != null)
                            {
                                CustomeUnitPrice = (decimal)Request.Custom;
                            }

                            decimal? Price1 = InventoryItemObjDB.CostAmount1;
                            if (Request.Price1 != null)
                            {
                                Price1 = (decimal)Request.Price1;
                            }
                            decimal? Price2 = InventoryItemObjDB.CostAmount2;
                            if (Request.Price2 != null)
                            {
                                Price2 = (decimal)Request.Price2;
                            }
                            decimal? Price3 = InventoryItemObjDB.CostAmount3;
                            if (Request.Price3 != null)
                            {
                                Price3 = (decimal)Request.Price3;
                            }
                            //var res = _Context.Myproc_InventoryItemPricingUpdate(Request.IDSinventoryItem, UserID, DateTime.Now,
                            //                                               CustomeUnitPrice,
                            //                                               Price1,
                            //                                               Price2,
                            //                                               Price3);
                            InventoryItemObjDB.ModifiedBy = UserID;
                            InventoryItemObjDB.ModifiedDate = DateTime.Now;
                            InventoryItemObjDB.CustomeUnitPrice = CustomeUnitPrice;
                            InventoryItemObjDB.CostAmount1 = Price1;
                            InventoryItemObjDB.CostAmount2 = Price2;
                            InventoryItemObjDB.CostAmount3 = Price3;

                            var res =_unitOfWork.Complete();

                            if (res > 0)
                            {
                                Response.Result = true;
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-414";
                            error.ErrorMSG = "Invalid Inventory Item ID.";
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

        public BaseResponse AddInventoryStoreItemPricinig(AddInventoryStoreItemPricing Request)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    bool IsIncValue = false;
                    bool IsPercentValue = false;
                    bool ForCustomValue = false;
                    bool ForPrice1Value = false;
                    bool ForPrice2Value = false;
                    bool ForPrice3Value = false;
                    bool AllItemValue = false;
                    decimal AmountValue = 0;
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



                    if (Request.IsInc == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-315";
                        error.ErrorMSG = "Is Inc is required";
                        Response.Errors.Add(error);
                    }
                    else if (Request.IsInc != true && Request.IsInc != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-316";
                        error.ErrorMSG = "Is Inc Must be true or false";
                        Response.Errors.Add(error);
                    }
                    if (Request.Amount == null || Request.Amount == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-315";
                        error.ErrorMSG = "Amount is required";
                        Response.Errors.Add(error);
                    }

                    if (Request.IsPercent == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-317";
                        error.ErrorMSG = "Is Percent is required";
                        Response.Errors.Add(error);
                    }
                    else if (Request.IsPercent != true && Request.IsPercent != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-317";
                        error.ErrorMSG = "Is Percent Must be true or false";
                        Response.Errors.Add(error);
                    }


                    if (Request.ForCustom != null)
                    {
                        if (Request.ForCustom != true && Request.ForCustom != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-318";
                            error.ErrorMSG = "For Custom Must be true or false";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            ForCustomValue = (bool)Request.ForCustom;
                        }
                    }
                    if (Request.ForPrice1 != null)
                    {
                        if (Request.ForPrice1 != true && Request.ForPrice1 != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-319";
                            error.ErrorMSG = "For Price1 Must be true or false";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            ForPrice1Value = (bool)Request.ForPrice1;
                        }
                    }
                    if (Request.ForPrice2 != null)
                    {
                        if (Request.ForPrice2 != true && Request.ForPrice2 != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-320";
                            error.ErrorMSG = "For Price2 Must be true or false";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            ForPrice2Value = (bool)Request.ForPrice2;
                        }
                    }
                    if (Request.ForPrice3 != null)
                    {
                        if (Request.ForPrice3 != true && Request.ForPrice3 != false)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-321";
                            error.ErrorMSG = "For Price3 Must be true or false";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            ForPrice3Value = (bool)Request.ForPrice3;
                        }
                    }

                    if (Request.AllItem != true && Request.AllItem != false)
                    {

                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-317";
                        error.ErrorMSG = "Must be select is All Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    // Filters to Get List of Items
                    int PriorityID = 0;
                    if (Request.PriorityID != null)
                    {
                        PriorityID = (int)Request.PriorityID;
                    }
                    long InventoryItemID = 0;
                    if (Request.InventoryItemID != null)
                    {
                        InventoryItemID = (long)Request.InventoryItemID;
                    }
                    int InventoryItemCategoryID = 0;
                    if (Request.InventoryItemCategoryID != null)
                    {
                        InventoryItemCategoryID = (int)Request.InventoryItemCategoryID;
                    }
                    int InventoryStoreID = 0;
                    if (Request.InventoryStoreID != null)
                    {
                        InventoryStoreID = (int)Request.InventoryStoreID;
                    }

                    long SupplierID = 0;
                    if (Request.SupplierID != null)
                    {
                        SupplierID = (long)Request.SupplierID;
                    }
                    bool NotPricidBefore = false;
                    if (Request.NotPricidBefore != null)
                    {
                        NotPricidBefore = (bool)Request.NotPricidBefore;
                    }




                    if (Response.Result)
                    {
                        if (ForCustomValue != true && ForPrice1Value != true && ForPrice2Value != true && ForPrice3Value != true)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-321";
                            error.ErrorMSG = "Must be selected at least one Column";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        IsIncValue = (bool)Request.IsInc;
                        IsPercentValue = (bool)Request.IsPercent;
                        AmountValue = (decimal)Request.Amount;
                        AllItemValue = Request.AllItem == true ? true : false;
                        // update 
                        // If All Item => true
                        // Call stored To calculation
                        // If All Item => false
                        string ListItemIDSSTRValue = "0";
                        if (AllItemValue != true)
                        {
                            // Get List Of Items with filters
                            var ListOfInventoryItemFullData = GetInventoryStoreItemFullInfoList(null, Request.SearchKey, InventoryStoreID, PriorityID, InventoryItemCategoryID, InventoryItemID, SupplierID, NotPricidBefore);
                            if (ListOfInventoryItemFullData.Count() > 0)
                            {
                                var ListIDS = ListOfInventoryItemFullData.Select(x => x.ID).ToList();
                                ListItemIDSSTRValue = string.Join(",", ListIDS.Select(n => n.ToString()).ToArray());
                            }
                        }

                        #region stored
                        var IsInc = new SqlParameter("IsInc", System.Data.SqlDbType.Bit);
                        IsInc.Value = IsIncValue;
                        var IsPercent = new SqlParameter("IsPercent", System.Data.SqlDbType.Bit);
                        IsPercent.Value = IsPercentValue;
                        var Amount = new SqlParameter("Amount", System.Data.SqlDbType.Decimal);
                        Amount.Value = AmountValue;
                        var ForCustom = new SqlParameter("ForCustom", System.Data.SqlDbType.Bit);
                        ForCustom.Value = ForCustomValue;
                        var ForPrice1 = new SqlParameter("ForPrice1", System.Data.SqlDbType.Bit);
                        ForPrice1.Value = ForPrice1Value;
                        var ForPrice2 = new SqlParameter("ForPrice2", System.Data.SqlDbType.Bit);
                        ForPrice2.Value = ForPrice2Value;
                        var ForPrice3 = new SqlParameter("ForPrice3", System.Data.SqlDbType.Bit);
                        ForPrice3.Value = ForPrice3Value;
                        var AllItem = new SqlParameter("AllItem", System.Data.SqlDbType.Bit);
                        AllItem.Value = AllItemValue;
                        var ItemListID = new SqlParameter("ItemListID", System.Data.SqlDbType.NVarChar);
                        ItemListID.Value = ListItemIDSSTRValue;

                        object[] param = new object[] { IsInc, IsPercent, Amount, ForCustom, ForPrice1, ForPrice2, ForPrice3, AllItem, ItemListID };
                        var ResultCalc = 0;
                        if (ListItemIDSSTRValue != null)
                        {
                            ResultCalc = _Context.Database.SqlQueryRaw<int>("Exec CalculateInventoryItemPrice @IsInc, @IsPercent, @Amount, @ForCustom, @ForPrice1, @ForPrice2, @ForPrice3, @AllItem, @ItemListID", param).ToList().FirstOrDefault();

                        }
                        #endregion
                        
                        if (ResultCalc <= 0)
                        {
                            Response.Result = true;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err400";
                            error.ErrorMSG = "Cannot Update Inventory Item Pricing";
                            Response.Errors.Add(error);
                        }
                        #region comment
                        //if (Request.IDSinventoryItemList.Count() > 0)
                        //{
                        //    foreach (var ItemID in Request.IDSinventoryItemList)
                        //    {
                        //        var InventoryItemObjDB = _Context.proc_InventoryItemLoadByPrimaryKey(ItemID).FirstOrDefault();
                        //        if (InventoryItemObjDB != null)
                        //        {
                        //            decimal CustomeUnitPrice = InventoryItemObjDB.CustomeUnitPrice;
                        //            decimal? Price1 = InventoryItemObjDB.CostAmount1;
                        //            decimal? Price2 = InventoryItemObjDB.CostAmount2;
                        //            decimal? Price3 = InventoryItemObjDB.CostAmount3;

                        //            decimal CustomApply = 0;
                        //            CustomApply = IsPercent ? (CustomeUnitPrice * Amount) / 100 : Amount;

                        //            if (IsInc)
                        //            {
                        //                CustomeUnitPrice += CustomApply;
                        //            }
                        //            else
                        //            {
                        //                CustomeUnitPrice = CustomeUnitPrice > CustomApply ? CustomeUnitPrice - CustomApply : 0;
                        //            }

                        //            if (Price1 != null)
                        //            {
                        //                decimal Price1Apply = 0;
                        //                Price1Apply = IsPercent ? ((decimal)Price1 * Amount) / 100 : Amount;

                        //                if (IsInc)
                        //                {
                        //                    Price1 += Price1Apply;
                        //                }
                        //                else
                        //                {
                        //                    Price1 = Price1 > Price1Apply ? Price1 - Price1Apply : 0;
                        //                }
                        //            }
                        //            if (Price2 != null)
                        //            {
                        //                decimal Price2Apply = 0;
                        //                Price2Apply = IsPercent ? ((decimal)Price2 * Amount) / 100 : Amount;

                        //                if (IsInc)
                        //                {
                        //                    Price2 += Price2Apply;
                        //                }
                        //                else
                        //                {
                        //                    Price2 = Price2 > Price2Apply ? Price2 - Price2Apply : 0;
                        //                }
                        //            }
                        //            if (Price3 != null)
                        //            {
                        //                decimal Price3Apply = 0;
                        //                Price3Apply = IsPercent ? ((decimal)Price3 * Amount) / 100 : Amount;

                        //                if (IsInc)
                        //                {
                        //                    Price3 += Price3Apply;
                        //                }
                        //                else
                        //                {
                        //                    Price3 = Price3 > Price3Apply ? Price3 - Price3Apply : 0;
                        //                }
                        //            }
                        //            _Context.Myproc_InventoryItemPricingUpdate(ItemID, validation.userID, DateTime.Now,
                        //                                                       CustomeUnitPrice,
                        //                                                       Price1,
                        //                                                       Price2,
                        //                                                       Price3);

                        //        }
                        //        else
                        //        {
                        //            Response.Result = true;
                        //            Error error = new Error();
                        //            error.ErrorCode = "Err-331";
                        //            error.ErrorMSG = "Warning , This ID Not Found on Inventory Item and not updated ID : " + ItemID;
                        //            Response.Errors.Add(error);
                        //        }
                        //    }
                        //}
                        #endregion
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

        public IQueryable<InventoryStoreItemFullData> GetInventoryStoreItemFullInfoList(string OperationType, string SearchKey, int InventoryStoreID, int PriorityID, int InventoryItemCategoryID, long InventoryItemID, long SupplierID, bool NotPricidBefore)
        {


            var InventoryItemListDB = _unitOfWork.VInventoryStoreItems.FindAllQueryable(x => x.Active == true && x.StoreActive == true).AsQueryable();
            if (!string.IsNullOrEmpty(OperationType))
            {
                if (OperationType.Trim().ToLower() == "openingbalance")
                {

                    InventoryItemListDB = InventoryItemListDB.Where(x => x.OperationType == "Opening Balance");
                }
            }
            if (!string.IsNullOrEmpty(SearchKey))
            {
                SearchKey = HttpUtility.UrlDecode(SearchKey);
                InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                                        || (x.Code != null ? x.Code.ToLower().Contains(SearchKey.ToLower()) : false)
                                        || (x.PartNo != null ? x.PartNo.ToLower().Contains(SearchKey.ToLower()) : false)
                                        ).AsQueryable();
            }
            if (InventoryStoreID != 0)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }

            if (PriorityID != 0)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.PriorityId == PriorityID).AsQueryable();
            }
            if (InventoryItemCategoryID != 0)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemCategoryId == InventoryItemCategoryID).AsQueryable();
            }
            if (InventoryItemID != 0)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemId == InventoryItemID).AsQueryable();
            }
            if (SupplierID != 0)
            {
                var IDSListInventoryItem = _unitOfWork.VPurchasePoItems.FindAll(x => x.ToSupplierId == SupplierID).Select(x => x.InventoryItemId).Distinct().ToList();
                InventoryItemListDB = InventoryItemListDB.Where(x => IDSListInventoryItem.Contains(x.Id)).AsQueryable();
            }
            if (NotPricidBefore == true)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.CustomeUnitPrice == 0 && (x.CostAmount1 == null || x.CostAmount1 == 0) && (x.CostAmount2 == null || x.CostAmount2 == 0) && (x.CostAmount3 == null || x.CostAmount3 == 0)).AsQueryable();
            }
            var ListOfInventoryItemFullData = InventoryItemListDB.Select(x => new InventoryStoreItemFullData
            {
                ID = x.InventoryItemId,
                ItemName = x.InventoryItemName,
                Active = x.Active ?? false,
                ItemCode = x.Code,
                InventoryStoreName = x.InventoryStoreName,
                PartNO = x.PartNo,
                Category = x.CategoryName,
                ItemSerialCounter = x.ItemSerialCounter != null ? x.ItemSerialCounter.ToString() : "",
                AverageUnitPrice = x.AverageUnitPrice ?? 0,
                LastUnitPrice = x.LastUnitPrice ?? 0,
                Balance = x.Balance,
                MaxUnitPrice = x.MaxUnitPrice ?? 0,
                CustomeUnitPrice = x.CustomeUnitPrice ?? 0,
                UnitCost = x.CustomeUnitPrice ?? 0,
                Cost1 = x.CostAmount1 ?? 0,
                Cost2 = x.CostAmount2 ?? 0,
                Cost3 = x.CostAmount3 ?? 0
            }).Distinct().AsQueryable().OrderBy(x => x.ItemName);


            return ListOfInventoryItemFullData;
        }

        public PurchasePoInventoryItemPriceListResponse GetInventoryItemPriceHistoryList(long InventoryItemId)
        {
            PurchasePoInventoryItemPriceListResponse Response = new PurchasePoInventoryItemPriceListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var PurchasePoInventoryItemPriceList = new List<PurchasePOInventoryItemPrice>();
                if (Response.Result)
                {
                    
                    if (InventoryItemId == 0)
                    {
                    
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item Id";
                        Response.Errors.Add(error);
                        return Response; 
                    }
                    var POInvoiceItemListDB = _unitOfWork.VPurchasePoitemInvoicePoSuppliers.FindAll(x => x.InventoryItemId == InventoryItemId).OrderBy(x => x.InvoiceDate).ToList();
                    foreach (var item in POInvoiceItemListDB)
                    {
                        var PurchasePoInventoryItemPriceObj = new PurchasePOInventoryItemPrice();
                        PurchasePoInventoryItemPriceObj.SupplierID = item.ToSupplierId;
                        PurchasePoInventoryItemPriceObj.SupplierName = item.SupplierName;
                        PurchasePoInventoryItemPriceObj.UnitPrice = item.ActualUnitPrice ?? 0;
                        PurchasePoInventoryItemPriceObj.UnitCost = item.FinalUnitCost ?? 0;
                        PurchasePoInventoryItemPriceObj.InvoiceDate = item.InvoiceDate != null ? ((DateTime)item.InvoiceDate).ToShortDateString() : "";
                        PurchasePoInventoryItemPriceObj.PurchasePOInvoiceID = item.PurchasePoinvoiceId;

                        PurchasePoInventoryItemPriceList.Add(PurchasePoInventoryItemPriceObj);
                    }


                    Response.PurchasePOInventoryItemPriceList = PurchasePoInventoryItemPriceList.ToList();
                    Response.CountOfInvoice = PurchasePoInventoryItemPriceList.Where(x => x.PurchasePOInvoiceID != null).Count();
                    Response.CountOfSupplier = PurchasePoInventoryItemPriceList.Where(x => x.SupplierID != null).Count();
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
