using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Infrastructure.Models.POS;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Inventory.Requests;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace NewGaras.Domain.Services
{
    public class POSService : IPOSService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        static readonly string key = "SalesGarasPass";
        private GarasTestContext _Context;

        public POSService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
        }

        public BaseResponseWithId<long> AddPosClosingDay(DateTime date, int storeId, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var store = _unitOfWork.InventoryStores.GetById(storeId);
                if (store == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Store Not found";
                    response.Errors.Add(error);
                    return response;
                }
                var orders = _unitOfWork.InventoryStoreItems.FindAll(a => a.InventoryStoreId == storeId && a.OperationType.ToLower().Contains("pos")).Select(a => a.OrderId);
                var offers = _unitOfWork.SalesOffers.FindAll(a => a.CreationDate.Date == date.Date && orders.Contains(a.Id));
                if (offers.Count() == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "No Data At this Store on This Date";
                    response.Errors.Add(error);
                    return response;
                }

                var GroupedOffers = offers.GroupBy(a => a.CreatedBy).ToList();

                foreach (var group in GroupedOffers)
                {
                    var PosClosingDay = new PosClosingDay() { };
                    PosClosingDay.Date = date.Date;
                    PosClosingDay.StoreId = store.Id;
                    PosClosingDay.UserId = group.Key;
                    PosClosingDay.SalesCount = group.Count(a => a.OfferType.ToLower() == "pos");
                    PosClosingDay.SalesAmount = group.Where(a => a.OfferType.ToLower() == "pos").Sum(a => a.OfferAmount ?? 0);
                    PosClosingDay.SalesReturnCount = group.Count(a => a.OfferType.ToLower() == "pos return");
                    PosClosingDay.SalesReturnAmount = group.Where(a => a.OfferType.ToLower() == "pos return").Sum(a => a.OfferAmount ?? 0);
                    PosClosingDay.NetSalesCount = PosClosingDay.SalesCount - PosClosingDay.SalesReturnCount;
                    PosClosingDay.NetSalesAmount = PosClosingDay.SalesAmount - PosClosingDay.SalesReturnAmount;
                    PosClosingDay.CreatedBy = creator;
                    PosClosingDay.ModifiedBy = creator;
                    PosClosingDay.CreationDate = DateTime.Now;
                    PosClosingDay.ModifiedDate = DateTime.Now;
                    _unitOfWork.PosClosingDays.Add(PosClosingDay);
                    _unitOfWork.Complete();
                    response.ID = PosClosingDay.Id;
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

        public BaseResponseWithDataAndHeader<List<GetPosClosingDay>> GetAll(int CurrentPage = 1, int NumberOfItemsPerPage = 10)
        {
            BaseResponseWithDataAndHeader<List<GetPosClosingDay>> response = new BaseResponseWithDataAndHeader<List<GetPosClosingDay>>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var posDays = _unitOfWork.PosClosingDays.FindAllPaging(a => true, CurrentPage, NumberOfItemsPerPage);
                var list = _mapper.Map<List<GetPosClosingDay>>(posDays);
                foreach (var item in list)
                {
                    var orders = _unitOfWork.InventoryStoreItems.FindAll(a => a.InventoryStoreId == item.StoreId && a.OperationType.ToLower().Contains("pos")).Select(a => a.OrderId);
                    var offers = _unitOfWork.SalesOffers.FindAll(a => a.CreationDate.Date == item.Date.Date && orders.Contains(a.Id), includes: new[] { "Client" });
                    item.HospitalityAmount = offers.Where(a => a.Client?.Name == "Hospitality").Sum(a => a.OfferAmount ?? 0);
                    item.PayableAmount = offers.Where(a => a.Client?.OwnerCoProfile != true && a.Client?.Name != "Hospitality").Sum(a => a.OfferAmount ?? 0);
                    item.CashAmount = offers.Where(a => a.Client?.OwnerCoProfile == true).Sum(a => a.OfferAmount ?? 0);
                }
                response.Data = list;
                response.PaginationHeader = new PaginationHeader()
                {
                    CurrentPage = posDays.CurrentPage,
                    ItemsPerPage = posDays.PageSize,
                    TotalItems = posDays.TotalCount,
                    TotalPages = posDays.TotalPages
                };
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

        public BaseResponseWithId<long> Update(List<UpdatePosClosingDay> dto, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (dto.Count == 0 || dto == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Data Is empty";
                    response.Errors.Add(error);
                    return response;
                }
                foreach (var item in dto)
                {
                    var pos = _unitOfWork.PosClosingDays.GetById(item.Id);
                    if (pos == null)
                    {
                        response.Errors.Add(new Error() { errorMSG = $"pos closing day not found at item {dto.IndexOf(item) + 1}" });
                    }
                    else
                    {
                        pos.Notes = item.Notes;
                        pos.ClosingDayAmount = item.ClosingDayAmount;
                        pos.ModifiedBy = creator;
                        pos.ModifiedDate = DateTime.Now;
                        _unitOfWork.PosClosingDays.Update(pos);
                        _unitOfWork.Complete();
                        response.ID = pos.Id;
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

        public bool AddInventoryStoreItemWithReturn(InventoryStoreItem ParentInventoryStoreItem, long InventoryItemId, decimal QTY, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId)
        {
            bool Result = false;
            string OperationType = "POS Return";

            var InventoryStoreItemOBJ = new InventoryStoreItem();
            InventoryStoreItemOBJ.InventoryStoreId = ParentInventoryStoreItem.InventoryStoreId;
            InventoryStoreItemOBJ.InventoryItemId = InventoryItemId;
            InventoryStoreItemOBJ.OrderNumber = SalesOfferId.ToString();
            InventoryStoreItemOBJ.OrderId = SalesOfferId;
            InventoryStoreItemOBJ.CreatedBy = ValidateUserId;
            InventoryStoreItemOBJ.ModifiedBy = ValidateUserId;
            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
            InventoryStoreItemOBJ.OperationType = OperationType;
            InventoryStoreItemOBJ.Balance = (double)QTY;
            InventoryStoreItemOBJ.Balance1 = QTY;
            InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
            InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
            InventoryStoreItemOBJ.ReleaseParentId = ParentInventoryStoreItem.Id;
            InventoryStoreItemOBJ.FinalBalance = QTY;
            InventoryStoreItemOBJ.AddingOrderItemId = SalesOfferProductId;
            InventoryStoreItemOBJ.AddingFromPoid = ParentInventoryStoreItem.AddingFromPoid;
            InventoryStoreItemOBJ.PoinvoiceId = ParentInventoryStoreItem.PoinvoiceId;
            InventoryStoreItemOBJ.PoinvoiceTotalPrice = ParentInventoryStoreItem.PoinvoiceTotalPrice;
            InventoryStoreItemOBJ.PoinvoiceTotalCost = ParentInventoryStoreItem.PoinvoiceTotalCost;
            InventoryStoreItemOBJ.CurrencyId = ParentInventoryStoreItem.CurrencyId;
            InventoryStoreItemOBJ.RateToEgp = ParentInventoryStoreItem.RateToEgp;
            InventoryStoreItemOBJ.PoinvoiceTotalPriceEgp = ParentInventoryStoreItem.PoinvoiceTotalPriceEgp;
            InventoryStoreItemOBJ.PoinvoiceTotalCostEgp = ParentInventoryStoreItem.PoinvoiceTotalCostEgp;
            InventoryStoreItemOBJ.RemainItemPrice = ParentInventoryStoreItem.RemainItemPrice;
            InventoryStoreItemOBJ.RemainItemCosetEgp = ParentInventoryStoreItem.RemainItemCosetEgp;
            InventoryStoreItemOBJ.RemainItemCostOtherCu = ParentInventoryStoreItem.RemainItemCostOtherCu;
            InventoryStoreItemOBJ.HoldQty = 0;
            InventoryStoreItemOBJ.HoldReason = null;
            _unitOfWork.InventoryStoreItems.Add(InventoryStoreItemOBJ);
            var InventoryStorItemInsertion = _unitOfWork.Complete();







            if (ParentInventoryStoreItem.AddingOrderItemId != 0)
            {
                //var ListSalesOfferProductIDS = _Context.SalesOfferProducts.Where(x => x.OfferID == SalesOfferID && x.InventoryItemID == InternalBackDataOBJ.InventoryItemID).OrderBy(x=>x.ReleasedQty)
                var SalesOfferProdDB = _unitOfWork.SalesOfferProducts.FindAll(x => x.Id == ParentInventoryStoreItem.AddingOrderItemId /*&& x.Quantity >= (x.ReleasedQty ?? 0)*/)
                    .OrderBy(x => x.ReleasedQty).FirstOrDefault();
                if (SalesOfferProdDB != null)
                {
                    SalesOfferProdDB.ReleasedQty += (double)QTY;
                }

                _unitOfWork.Complete();
            }

            if (InventoryStorItemInsertion > 0)
            {
                Result = true;
            }

            return Result;
        }

        public void AddInvntoryStoreItemWithRelease(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemId, int StoreID, decimal QTY, bool IsFIFO, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId)
        {


            List<InventoryStoreItemIDWithQTY> AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                            InventoryItemId,
                                                                            StoreID,
                                                                            null,// store location
                                                                            QTY,
                                                                            IsFIFO);
            // Add Store Item
            string OperationType = "POS Release";
            decimal RemainReleaseQTY = QTY; // 20


            // List Of IDS inserted
            List<long> ListIDSUpdate = new List<long>();

            foreach (var ObjParentRelease in AvailableItemStockList)  // 20 -  10   - 5
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

                    }
                    var InventoryStoreItemOBJ = new InventoryStoreItem();
                    InventoryStoreItemOBJ.InventoryStoreId = StoreID;
                    InventoryStoreItemOBJ.InventoryItemId = InventoryItemId;
                    InventoryStoreItemOBJ.OrderNumber = SalesOfferId.ToString();
                    InventoryStoreItemOBJ.OrderId = SalesOfferId;
                    InventoryStoreItemOBJ.CreatedBy = ValidateUserId;
                    InventoryStoreItemOBJ.ModifiedBy = ValidateUserId;
                    InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                    InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                    InventoryStoreItemOBJ.OperationType = OperationType;
                    InventoryStoreItemOBJ.Balance = (double)(-ReleaseQTY);
                    InventoryStoreItemOBJ.Balance1 = (decimal)(-ReleaseQTY);
                    InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                    InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                    InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                    InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                    InventoryStoreItemOBJ.AddingFromPoid = POID;
                    InventoryStoreItemOBJ.AddingOrderItemId = SalesOfferProductId;
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
                        ListIDSUpdate.Add(InventoryStoreItemOBJ.Id);
                        // Update Parent Release on InventoryStoreItem
                        if (ParentInventoryStoreItem != null)
                        {
                            ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                            ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                            ParentInventoryStoreItem.ModifiedBy = ValidateUserId;


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

            //// -------------------------------------------------Update and Calc Average for Inventory Item ----------------------------------------------------------------
            //var ListInvStoreItemAll = InventoryStoreItemList.Where(x => x.InventoryItemID == InventoryItemObjDB.ID);
            //var ListInventoryStoreItem = ListInvStoreItemAll.Where(x => x.finalBalance > 0 && x.POInvoiceTotalCost != null);
            //var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.POInvoiceId).ToList();
            //var ListIDSPOInvoicesIsFulllyPriced = _Context.PurchasePOInvoices.Where(x => ListIDSPOInvoices.Contains(x.ID)).Select(x => x.ID).ToList();
            //ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.POInvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.POInvoiceId) : false);
            //InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.finalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.POInvoiceTotalCostEGP * x.finalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.finalBalance) ?? 0) : 0;

            //// Update Avg Unit Price ..just stop for list of inventorystoreItem List
            ////foreach (var itemId in ListIDSUpdate)
            ////{
            ////    var InventoryStoreItemOBJ = ListInvStoreItemAll.Where(x => x.ID == itemId).FirstOrDefault();
            ////    InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;

            ////}
            //_Context.SaveChanges();
        }
        public BaseResponseWithId<long> AddNewSalesOfferWithReleaseForPOS(AddNewSalesOfferWithReleaseForPOSRequest Request, string companyname, long creator)
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

                    //string CreatedByString = null;
                    //long? CreatedBy = 0;
                    int InventoryStoreId = 0;
                    DateTime? StartDate = null;
                    DateTime StartDateTemp = DateTime.Now;

                    DateTime? EndDate = null;
                    DateTime EndDateTemp = DateTime.Now;

                    DateTime? ClientApprovalDate = null;
                    DateTime ClientApprovalDateTemp = DateTime.Now;

                    DateTime? OfferExpirationDate = null;
                    DateTime OfferExpirationDateTemp = DateTime.Now;

                    DateTime? ProjectStartDate = null;
                    DateTime ProjectStartDateTemp = DateTime.Now;

                    DateTime? ProjectEndDate = null;
                    DateTime ProjectEndDateTemp = DateTime.Now;

                    DateTime? RentStartDate = null;
                    DateTime RentStartDateTemp = DateTime.Now;

                    DateTime? RentEndDate = null;
                    DateTime RentEndDateTemp = DateTime.Now;

                    DateTime? ReminderDate = null;
                    DateTime ReminderDateTemp = DateTime.Now;

                    DateTime? SendingOfferDate = null;
                    DateTime SendingOfferDateTemp = DateTime.Now;

                    if (Request.SalesOffer != null)
                    {
                        if (Request.SalesOffer.StartDate == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Offer Start Date Is Mandatory";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            if (DateTime.TryParse(Request.SalesOffer.StartDate, out StartDateTemp))
                            {
                                StartDateTemp = DateTime.Parse(Request.SalesOffer.StartDate);
                                StartDate = StartDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid StartDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (Request.SalesOffer.SalesPersonId == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Offer Sales Person Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (Request.SalesOffer.BranchId == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Sales Offer Branch Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.EndDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.EndDate, out EndDateTemp))
                            {
                                EndDateTemp = DateTime.Parse(Request.SalesOffer.EndDate);
                                EndDate = EndDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid EndDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ClientApprovalDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ClientApprovalDate, out ClientApprovalDateTemp))
                            {
                                ClientApprovalDateTemp = DateTime.Parse(Request.SalesOffer.ClientApprovalDate);
                                ClientApprovalDate = ClientApprovalDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ClientApprovalDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.OfferExpirationDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.OfferExpirationDate, out OfferExpirationDateTemp))
                            {
                                OfferExpirationDateTemp = DateTime.Parse(Request.SalesOffer.OfferExpirationDate);
                                OfferExpirationDate = OfferExpirationDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid OfferExpirationDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectStartDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ProjectStartDate, out ProjectStartDateTemp))
                            {
                                ProjectStartDateTemp = DateTime.Parse(Request.SalesOffer.ProjectStartDate);
                                ProjectStartDate = ProjectStartDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ProjectStartDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectEndDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ProjectEndDate, out ProjectEndDateTemp))
                            {
                                ProjectEndDateTemp = DateTime.Parse(Request.SalesOffer.ProjectEndDate);
                                ProjectEndDate = ProjectEndDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ProjectEndDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.RentStartDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.RentStartDate, out RentStartDateTemp))
                            {
                                RentStartDateTemp = DateTime.Parse(Request.SalesOffer.RentStartDate);
                                RentStartDate = RentStartDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid RentStartDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.RentEndDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.RentEndDate, out RentEndDateTemp))
                            {
                                RentEndDateTemp = DateTime.Parse(Request.SalesOffer.RentEndDate);
                                RentEndDate = RentEndDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid RentEndDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.ReminderDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.ReminderDate, out ReminderDateTemp))
                            {
                                ReminderDateTemp = DateTime.Parse(Request.SalesOffer.ReminderDate);
                                ReminderDate = ReminderDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid ReminderDate";
                                Response.Errors.Add(error);
                            }
                        }

                        if (!string.IsNullOrEmpty(Request.SalesOffer.SendingOfferDate))
                        {
                            if (DateTime.TryParse(Request.SalesOffer.SendingOfferDate, out SendingOfferDateTemp))
                            {
                                SendingOfferDateTemp = DateTime.Parse(Request.SalesOffer.SendingOfferDate);
                                SendingOfferDate = SendingOfferDateTemp;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Invalid SendingOfferDate";
                                Response.Errors.Add(error);
                            }
                        }


                        //if (Request.SalesOffer.CreatedBy != null)
                        //{
                        //    CreatedByString = Request.SalesOffer.CreatedBy;
                        //}
                        //else
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "Err25";
                        //    error.ErrorMSG = "Sales Offer Created By Id Is Mandatory";
                        //    Response.Errors.Add(error);
                        //}
                        //CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                        //var user = _unitOfWork.Users.GetById((long)CreatedBy);
                        //if (user == null)
                        //{
                        //    Response.Result = false;
                        //    Error error = new Error();
                        //    error.ErrorCode = "ErrCRM1";
                        //    error.ErrorMSG = "Sales Offer Creator User Doesn't Exist!!";
                        //    Response.Errors.Add(error);
                        //}


                        // Modified By michael markos 2022-10-25
                        if (Request.SalesOffer.ParentSalesOfferID != null)
                        {
                            // check if this Offer is Found
                            var SalesOfferObj = _unitOfWork.SalesOffers.FindAll(x => x.Id == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
                            if (SalesOfferObj == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Return Sales Offer!";
                                Response.Errors.Add(error);
                            }

                            if (Request.SalesOffer.OfferType != "Sales Return")
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Return Sales Offer Type Must Be Sales Return!";
                                Response.Errors.Add(error);
                            }


                        }

                        if (Request.SalesOffer.ParentInvoiceID != null)
                        {
                            // check if this Offer is Found
                            var InvoicesObj = _unitOfWork.Invoices.FindAll(x => x.Id == Request.SalesOffer.ParentInvoiceID).FirstOrDefault();
                            if (InvoicesObj == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Return Invoice!";
                                Response.Errors.Add(error);
                            }
                        }



                        if (Request.SalesOffer.ParentSalesOfferID == null || Request.SalesOffer.ParentSalesOfferID == 0)
                        {

                            if (Request.SalesOffer.InventoryStoreId == null || Request.SalesOffer.InventoryStoreId == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Inventory Store Id in Sales Offer!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                InventoryStoreId = (int)Request.SalesOffer.InventoryStoreId;
                            }
                        }

                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Sales Offer Data!!";
                        Response.Errors.Add(error);
                    }
                    List<InventoryStoreItem> InventoryStoreItemListDB = new List<InventoryStoreItem>();
                    //if (Request.SalesOfferProductList == null || Request.SalesOfferProductList.Count() == 0)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "must be one item at least in offer";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    if (Request.SalesOfferProductList != null)
                    {
                        if (Request.SalesOfferProductList.Count() > 0)
                        {

                            var InventoryItemListIDS = Request.SalesOfferProductList.Select(x => x.InventoryItemId).ToList();

                            InventoryStoreItemListDB = _unitOfWork.InventoryStoreItems.FindAll(x => InventoryItemListIDS.Contains(x.InventoryItemId)).ToList();
                            int Counter = 0;
                            foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                            {
                                Counter++;
                                if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID > 0)
                                {
                                    if (SalesOfferProduct.ParentOfferProductId == null || SalesOfferProduct.ParentOfferProductId == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Sales Offer Product ParentId Is Mandatory For Returned Sales Offer!";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        var ParentProductcDb = _unitOfWork.SalesOfferProducts.GetById((long)SalesOfferProduct.ParentOfferProductId);
                                        if (ParentProductcDb != null)
                                        {
                                            if (SalesOfferProduct.Quantity != null && SalesOfferProduct.Quantity > 0)
                                            {
                                                if (SalesOfferProduct.Quantity > (ParentProductcDb.RemainQty ?? ParentProductcDb.Quantity ?? 0))
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err-P12";
                                                    error.ErrorMSG = "Returned Quantity For Sales Offer Product: " + SalesOfferProduct.ParentOfferProductId + " Cannot be Greater Than Remain Quantity Of Parent Product!";
                                                    Response.Errors.Add(error);
                                                }
                                            }

                                            // Validate if Return => if have balance from parent or not


                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err-P12";
                                            error.ErrorMSG = "This Sales Offer Product " + SalesOfferProduct.ParentOfferProductId + " Doesn't Exist!";
                                            Response.Errors.Add(error);
                                        }

                                    }
                                }


                                //if (SalesOfferProduct.CreatedBy != null)
                                //{
                                //    CreatedByString = SalesOfferProduct.CreatedBy;
                                //}
                                //else
                                //{
                                //    Response.Result = false;
                                //    Error error = new Error();
                                //    error.ErrorCode = "Err25";
                                //    error.ErrorMSG = "Sales Offer Product Created By Id Is Mandatory";
                                //    Response.Errors.Add(error);
                                //}


                                //CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                //var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                //if (user == null)
                                //{
                                //    Response.Result = false;
                                //    Error error = new Error();
                                //    error.ErrorCode = "ErrCRM1";
                                //    error.ErrorMSG = "Sales Offer Product Creator User Doesn't Exist!!";
                                //    Response.Errors.Add(error);
                                //}
                                long InventoryItemId = 0;
                                if (SalesOfferProduct.InventoryItemId == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "Invalid InventoryItemId on Item No #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    InventoryItemId = (long)SalesOfferProduct.InventoryItemId;
                                }
                                decimal Quantity = 0;
                                if (SalesOfferProduct.Quantity == null || SalesOfferProduct.Quantity <= 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "Invalid Sales Offer Product Quantity on Item No #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    Quantity = (decimal)SalesOfferProduct.Quantity;
                                }
                                if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID != 0)
                                {
                                    // Check if salesoffer is exist in inventory store item for return
                                    var InvetorStorItemForParentSalesOffer = InventoryStoreItemListDB.Where(x => x.OrderId == Request.SalesOffer.ParentSalesOfferID && x.AddingOrderItemId == SalesOfferProduct.ParentOfferProductId && x.FinalBalance < 0).Select(x => x.FinalBalance).Sum();
                                    if (InvetorStorItemForParentSalesOffer == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Not have balance to Return Release on Item No #" + Counter;
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    if (InventoryStoreId != 0 && InventoryItemId != 0 && Quantity != 0)
                                    {
                                        InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0).ToList();
                                        List<InventoryStoreItemIDWithQTY> AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemListDB,
                                                                    InventoryItemId,
                                                                    InventoryStoreId,
                                                                    null,// store location
                                                                    Quantity,
                                                                    true);
                                        if (AvailableItemStockList == null || AvailableItemStockList.Count() == 0 || AvailableItemStockList.Sum(x => x.StockBalance) < Quantity)
                                        //{
                                        //    Response.Result = false;
                                        //    Error error = new Error();
                                        //    error.ErrorCode = "Err325";
                                        //    error.ErrorMSG = "Not have availble qty from parent  Item to release on Item NO #" + itemCount;
                                        //    Response.Errors.Add(error);
                                        //    //return Response;
                                        //}
                                        //if (AvailableItemStockList.Count() == 0)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "Not have balance to release on Item No #" + Counter;
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Request.SalesOffer != null)
                        {
                            if (Request.SalesOffer.Id == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Sales Offer Product";
                                Response.Errors.Add(error);

                            }
                        }
                    }
                    //long? InventoryStoreID = null;
                    //if (InventoryStoreID == null)
                    //{
                    // Check if inventory Dirct is not found to add one
                    //    string StoreName = "DIRECT PR HIDDEN STORE";
                    //long? InventoryStoreID = Common.CheckInventoryStoreID(StoreName);
                    //    if (InventoryStoreID == 0)
                    //    {
                    //        // Inserty Inventory Store ID
                    //        ObjectParameter IDInventoryStore = new ObjectParameter("ID", typeof(int));
                    //        _Context.proc_InventoryStoreInsert(IDInventoryStore,
                    //                                           StoreName,
                    //                                           true,
                    //                                           null, null,
                    //                                           DateTime.Now,
                    //                                           validation.userID,
                    //                                           DateTime.Now,
                    //                                           validation.userID);

                    //        InventoryStoreID = (int)IDInventoryStore.Value;
                    //    }
                    // }
                    // Get the timezone information for Egypt
                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    // Get the current datetime in Egypt
                    DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

                    if (Response.Result)
                    {
                        long SalesOfferId = 0;
                        // Add-Edit Sales Offer
                        if (Request.SalesOffer.Id == null || Request.SalesOffer.Id == 0)
                        {

                            var NewOfferSerial = "";
                            var OfferSerialSubString = "";

                            //long newOfferNumber = 0;
                            long CountOfSalesOfferThisYear = _unitOfWork.SalesOffers.Count(x => x.Active == true && x.OfferSerial.Contains(System.DateTime.Now.Year.ToString()) ||
                            (x.OfferType == "POS" || x.OfferType == "Sales Return"));
                            //if (companyname.ToLower() == "marinaplt")
                            //{
                            //    if (Request.SalesOffer.OfferType == "New Project Offer")
                            //    {
                            //        NewOfferSerial = "S";
                            //        OfferSerialSubString = "-RFQ-#";
                            //    }
                            //    else if (Request.SalesOffer.OfferType == "New Maintenance Offer")
                            //    {
                            //        NewOfferSerial = "M";
                            //        OfferSerialSubString = "-RFM-#";
                            //    }
                            //    else if (Request.SalesOffer.OfferType == "New Rent Offer")
                            //    {
                            //        NewOfferSerial = "R";
                            //        OfferSerialSubString = "-RentOffer-#";
                            //    }
                            //    else if (Request.SalesOffer.OfferType == "New Internal Order")
                            //    {
                            //        NewOfferSerial = "I";
                            //        OfferSerialSubString = "-RFQ-#";
                            //    }

                            //    var branchName = Common.GetBranchName(Request.SalesOffer.BranchId, _Context);

                            //    if (branchName == "Alexandria")
                            //    {
                            //        NewOfferSerial += "-A";
                            //    }
                            //    else if (branchName == "Cairo")
                            //    {
                            //        NewOfferSerial += "-C";
                            //    }
                            //    else if (branchName == "Factory")
                            //    {
                            //        NewOfferSerial += "-F";
                            //    }
                            //    else if (branchName == "Show Room")
                            //    {
                            //        NewOfferSerial += "-S.R";
                            //    }
                            //    else if (branchName == "Main Office")
                            //    {
                            //        NewOfferSerial += "-M.O";
                            //    }

                            //    //if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                            //    //{
                            //    //    string strLastOfferNumber = lastSalesOfferSerial.Split('-')[4];
                            //    //    newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                            //    //    NewOfferSerial += OfferSerialSubString + newOfferNumber + "-" + System.DateTime.Now.Year.ToString();
                            //    //}
                            //    //else
                            //    NewOfferSerial += OfferSerialSubString + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Year.ToString();
                            //}
                            //else if (companyname.ToLower() == "proauto")
                            //{
                            //    var lastSalesOfferSerial = _unitOfWork.SalesOffers.FindAll(a => a.Active == true).ToList().OrderByDescending(a => a.Id).Select(a => a.OfferSerial).FirstOrDefault();
                            //    if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                            //    {
                            //        var ListSplit = lastSalesOfferSerial.Split('-');
                            //        string strLastOfferNumber = ListSplit[0];
                            //        var newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                            //        NewOfferSerial += "#" + newOfferNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                            //    }

                            //}
                            //else
                            //{
                            //    //if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                            //    //{
                            //    //    string strLastOfferNumber = lastSalesOfferSerial.Split('-')[0];
                            //    //    newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                            //    //    NewOfferSerial += "#" + newOfferNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                            //    //}
                            //    //else
                            //}
                                NewOfferSerial += "#" + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();

                            //var CreatorId = long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key));
                            // For Hospitality
                            long? ClientId = Request.SalesOffer.ClientId;
                            if (Request.SalesOffer.IsHospitality == true)
                            {
                                string ClientName = "Hospitality";
                                long ClientID = _unitOfWork.Clients.FindAll(x => x.Name == ClientName).Select(x => x.Id).FirstOrDefault();
                                if (ClientID == 0)
                                {
                                    var ClientDB = new Client();
                                    ClientDB.Name = ClientName;
                                    ClientDB.Type = "Individual";
                                    ClientDB.CreatedBy = creator;
                                    ClientDB.SalesPersonId = creator;
                                    ClientDB.CreationDate = egyptDateTime;
                                    ClientDB.FollowUpPeriod = 1;


                                    _unitOfWork.Clients.Add(ClientDB);
                                    _unitOfWork.Complete();
                                    ClientId = ClientDB.Id;
                                }
                            }
                            // Insert
                            var NewSalesOfferInsert = new SalesOffer()
                            {
                                StartDate = DateOnly.FromDateTime((DateTime)StartDate),
                                EndDate = DateOnly.FromDateTime((DateTime)EndDate),
                                Note = string.IsNullOrEmpty(Request.SalesOffer.Note) ? null : Request.SalesOffer.Note,
                                SalesPersonId = Request.SalesOffer.SalesPersonId,
                                CreatedBy = creator,
                                CreationDate = egyptDateTime,
                                ModifiedBy = null,
                                Modified = null,
                                Active = true,
                                Status = Request.SalesOffer.Status,
                                Completed = Request.SalesOffer.Completed,
                                TechnicalInfo = string.IsNullOrEmpty(Request.SalesOffer.TechnicalInfo) ? null : Request.SalesOffer.TechnicalInfo,
                                ProjectData = string.IsNullOrEmpty(Request.SalesOffer.ProjectData) ? null : Request.SalesOffer.ProjectData,
                                FinancialInfo = string.IsNullOrEmpty(Request.SalesOffer.FinancialInfo) ? null : Request.SalesOffer.FinancialInfo,
                                PricingComment = string.IsNullOrEmpty(Request.SalesOffer.PricingComment) ? null : Request.SalesOffer.PricingComment,
                                OfferAmount = Request.SalesOffer.OfferAmount == null ? null : Request.SalesOffer.OfferAmount,
                                SendingOfferConfirmation = Request.SalesOffer.SendingOfferConfirmation,
                                SendingOfferDate = SendingOfferDate,
                                SendingOfferBy = Request.SalesOffer.SendingOfferBy,
                                SendingOfferTo = Request.SalesOffer.SendingOfferTo,
                                SendingOfferComment = Request.SalesOffer.SendingOfferComment,
                                ClientApprove = Request.SalesOffer.ClientApprove,
                                ClientComment = Request.SalesOffer.ClientComment,
                                VersionNumber = Request.SalesOffer.VersionNumber,
                                ClientApprovalDate = ClientApprovalDate,
                                ClientId = Request.SalesOffer.ClientId,
                                ProductType = Request.SalesOffer.ProductType,
                                ProjectName = string.IsNullOrEmpty(Request.SalesOffer.ProjectName) ? NewOfferSerial : Request.SalesOffer.ProjectName,
                                ProjectLocation = Request.SalesOffer.ProjectLocation,
                                ContactPersonMobile = Request.SalesOffer.ContactPersonMobile,
                                ContactPersonEmail = Request.SalesOffer.ContactPersonEmail,
                                ContactPersonName = Request.SalesOffer.ContactPersonName,
                                ProjectStartDate = ProjectStartDate,
                                ProjectEndDate = ProjectEndDate,
                                BranchId = Request.SalesOffer.BranchId,
                                OfferType = Request.SalesOffer.OfferType,
                                ContractType = Request.SalesOffer.ContractType,
                                OfferSerial = NewOfferSerial,
                                ClientNeedsDiscount = Request.SalesOffer.ClientNeedsDiscount,
                                RejectionReason = Request.SalesOffer.RejectionReason,
                                NeedsInvoice = Request.SalesOffer.NeedsInvoice,
                                NeedsExtraCost = Request.SalesOffer.NeedsExtraCost,
                                OfferExpirationDate = OfferExpirationDate,
                                OfferExpirationPeriod = Request.SalesOffer.OfferExpirationPeriod,
                                ExtraOrDiscountPriceBySalesPerson = Request.SalesOffer.ExtraOrDiscountPriceBySalesPerson,
                                FinalOfferPrice = Request.SalesOffer.FinalOfferPrice,
                                ReminderDate = ReminderDate
                            };
                            _unitOfWork.SalesOffers.Add(NewSalesOfferInsert);
                            var SalesOfferInsert = _unitOfWork.Complete();

                            if (SalesOfferInsert != 0 && NewSalesOfferInsert.Id != 0)
                            {
                                SalesOfferId = (long)NewSalesOfferInsert.Id;
                                Response.Result = true;
                                Response.ID = SalesOfferId;
                                if (Request.SalesOffer.ParentSalesOfferID != null)
                                {
                                    long? ParentInvoiceID = Request.SalesOffer.ParentInvoiceID;
                                    if (ParentInvoiceID == null)
                                    {
                                        var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
                                        if (InvoiceDB != null)
                                        {
                                            ParentInvoiceID = InvoiceDB.Id;
                                        }
                                    }
                                    if (ParentInvoiceID != null)
                                    {

                                        // Modified By Michael Markos 2022-10-25
                                        // Add in table Invoice CN And DN
                                        var InvoiceCNAndDNObj = new InvoiceCnandDn();
                                        InvoiceCNAndDNObj.ParentSalesOfferId = (long)Request.SalesOffer.ParentSalesOfferID;
                                        InvoiceCNAndDNObj.ParentInvoiceId = (long)ParentInvoiceID;
                                        InvoiceCNAndDNObj.SalesOfferId = SalesOfferId;
                                        InvoiceCNAndDNObj.Active = true;
                                        InvoiceCNAndDNObj.CreatedBy = creator; //CreatedBy ?? 1;
                                        InvoiceCNAndDNObj.CreationDate = egyptDateTime;
                                        InvoiceCNAndDNObj.ModifiedBy = creator; // CreatedBy ?? 1;
                                        InvoiceCNAndDNObj.ModificationDate = egyptDateTime;

                                        _unitOfWork.InvoiceCnandDns.Add(InvoiceCNAndDNObj);
                                        _unitOfWork.Complete();
                                    }
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Offer!!";
                                Response.Errors.Add(error);
                            }
                            //if (Request.SalesOffer.Status.ToLower() == "closed")
                            //{
                            //    //if (SalesOfferDbStatus.ToLower() != "closed")
                            //    //{
                            //    //}
                            //    if (ProjectSalesOfferIsExist == null)
                            //    {
                            //        CloseSalesOffer(SalesOfferId, headers["CompanyName"].ToString(), validation.userID);
                            //    }
                            //}

                            //var SalesOfferInvoices = _Context.Invoices.Where(a => a.SalesOfferId == SalesOfferId).Count();
                            // Add-Edit Sales Offer Product
                            if (Request.SalesOfferProductList != null)
                            {
                                if (Request.SalesOfferProductList.Count() > 0)
                                {

                                    foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                                    {
                                        decimal TempProfitPercentage;
                                        decimal? ProfitPercentage = decimal.TryParse(SalesOfferProduct.ProfitPercentage, out TempProfitPercentage) ? TempProfitPercentage : (decimal?)null;

                                        string ItemComment = null;
                                        if (string.IsNullOrEmpty(SalesOfferProduct.ItemPricingComment))
                                        {
                                            var inventoryItemDb = _unitOfWork.InventoryItems.GetById((long)SalesOfferProduct.InventoryItemId);
                                            if (inventoryItemDb != null)
                                            {
                                                if (string.IsNullOrEmpty(inventoryItemDb.Description))
                                                {
                                                    ItemComment = inventoryItemDb.Name;
                                                }
                                                else
                                                {
                                                    ItemComment = inventoryItemDb.Description;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ItemComment = SalesOfferProduct.ItemPricingComment;
                                        }
                                        // Add-Edit Sales Offer
                                        if (SalesOfferProduct.Id == null || SalesOfferProduct.Id == 0)
                                        {

                                            // Insert
                                            //ObjectParameter SalesOfferProductInsertedId = new ObjectParameter("ID", typeof(long));
                                            var NewSalesOfferProductInsert = new SalesOfferProduct()
                                            {
                                                CreatedBy = creator, // long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key)),
                                                CreationDate = egyptDateTime,
                                                ModifiedBy = null,
                                                Modified = null,
                                                Active = true,
                                                OfferId = SalesOfferId,
                                                ProductId = SalesOfferProduct.ProductId,
                                                ProductGroupId = SalesOfferProduct.ProductGroupId,
                                                Quantity = SalesOfferProduct.Quantity,
                                                InventoryItemId = SalesOfferProduct.InventoryItemId,
                                                InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId,
                                                ItemPrice = SalesOfferProduct.ItemPrice,
                                                ItemPricingComment = ItemComment,
                                                ConfirmReceivingQuantity = null,
                                                ConfirmReceivingComment = null,
                                                InvoicePayerClientId = SalesOfferProduct.InvoicePayerClientId,
                                                DiscountPercentage = SalesOfferProduct.DiscountPercentage,
                                                DiscountValue = SalesOfferProduct.DiscountValue,
                                                FinalPrice = SalesOfferProduct.FinalPrice,
                                                TaxPercentage = SalesOfferProduct.TaxPercentage,
                                                TaxValue = SalesOfferProduct.TaxValue,
                                                ReturnedQty = 0,
                                                RemainQty = SalesOfferProduct.Quantity,
                                                ProfitPercentage = ProfitPercentage,
                                                ReleasedQty = null
                                            };
                                            var SalesOfferProductInsert = _unitOfWork.SalesOfferProducts.Add(NewSalesOfferProductInsert);
                                            _unitOfWork.Complete();
                                            if (SalesOfferProductInsert != null)
                                            {
                                                SalesOfferProduct.Id = (long)SalesOfferProductInsert.Id;

                                                if (SalesOfferProduct.ParentOfferProductId != null && SalesOfferProduct.ParentOfferProductId != 0) // Client Return
                                                {
                                                    var ParentSalesOfferProductDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.Id == (long)SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
                                                    if (ParentSalesOfferProductDb != null)
                                                    {
                                                        ParentSalesOfferProductDb.RemainQty -= SalesOfferProduct.Quantity;
                                                        ParentSalesOfferProductDb.ReturnedQty += SalesOfferProduct.Quantity;
                                                        _unitOfWork.Complete();
                                                    }

                                                    // Client Return
                                                    var ParentInventoryStoreItemDB = InventoryStoreItemListDB.Where(x => x.OrderId == Request.SalesOffer.ParentSalesOfferID && x.AddingOrderItemId == SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
                                                    if (ParentInventoryStoreItemDB != null)
                                                    {
                                                        AddInventoryStoreItemWithReturn(ParentInventoryStoreItemDB, (long)SalesOfferProduct.InventoryItemId, (decimal)SalesOfferProduct.Quantity, SalesOfferId, SalesOfferProduct.Id, creator);
                                                    }

                                                }
                                                else // Release 
                                                {
                                                    // Release Item inventoryStoreItem
                                                    if (InventoryStoreId != 0)
                                                    {
                                                        AddInvntoryStoreItemWithRelease(InventoryStoreItemListDB, (long)SalesOfferProduct.InventoryItemId, (int)InventoryStoreId, (decimal)SalesOfferProduct.Quantity, true, SalesOfferId, SalesOfferProduct.Id, creator);
                                                    }
                                                }
                                                Response.Result = true;
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Faild To Insert this Offer!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                    }

                                    //Deleted Invoices + Deleted InvoiceItems + Inserted InvoiceItems + Update Project Price
                                    if (Request.SalesOffer.Status != null && Request.SalesOffer.Status.ToLower() == "closed")
                                    {


                                        //When change PayerId Check Deleted Invoices And Inserted Invoices Then Delete Old and Insert New
                                        var clientsIds = Request.SalesOfferProductList.Where(a => a.Active == true).Select(a => a.InvoicePayerClientId).Distinct().ToList();

                                        var InvoicesToInsertIds = clientsIds.ToList();


                                        //Insert 

                                        if (InvoicesToInsertIds != null && InvoicesToInsertIds.Count > 0)
                                        {
                                            foreach (var clientId in InvoicesToInsertIds)
                                            {
                                                var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
                                                var OfferClientApprovalDate = SalesOfferDb.ClientApprovalDate;
                                                DateTime InvoiceDate = OfferClientApprovalDate ?? egyptDateTime;
                                                long InvoiceId = 0;
                                                //ObjectParameter InvoiceInsertedId = new ObjectParameter("ID", typeof(long));
                                                var NewInvoiceInsert = new Invoice()
                                                {
                                                    Serial = "1",
                                                    Revision = 0,
                                                    InvoiceDate = InvoiceDate,
                                                    InvoiceType = "1",
                                                    ClientId = clientId,
                                                    CreatedBy = creator,
                                                    ModifiedBy = creator,
                                                    CreationDate = egyptDateTime,
                                                    ModificationDate = egyptDateTime,
                                                    Active = true,
                                                    IsClosed = false,
                                                    CreationType = null,
                                                    InvoiceFor = null,
                                                    EInvoiceId = null,
                                                    EInvoiceStatus = null,
                                                    EInvoiceAcceptDate = null,
                                                    SalesOfferId = SalesOfferId,
                                                    EInvoiceJsonBody = null,
                                                    EInvoiceRequestToSend = null,
                                                };
                                                var InvoiceInsert = _unitOfWork.Invoices.Add(NewInvoiceInsert);
                                                _unitOfWork.Complete();

                                                if (InvoiceInsert != null)
                                                {
                                                    InvoiceId = (long)InvoiceInsert.Id;
                                                    int SerialTemp = 0;
                                                    var SerialList = _unitOfWork.Invoices.FindAll(x => x.Active == true).Select(x => x.Serial).ToList();
                                                    int Serial = SerialList.Where(x => int.TryParse(x, out SerialTemp)).OrderByDescending(x => int.Parse(x)).Select(x => int.Parse(x)).FirstOrDefault();
                                                    //int SerialNo = string.IsNullOrEmpty(Serial) && int.TryParse(Serial, out SerialNo) ? 1 : int.Parse(Serial) + 1;
                                                    var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.Id == InvoiceId).FirstOrDefault();
                                                    if (InvoiceDB != null)
                                                    {
                                                        InvoiceDB.Serial = (Serial + 1).ToString();
                                                        _unitOfWork.Complete();
                                                    }
                                                    var ClientInvoicesItemList = _unitOfWork.SalesOfferProducts.FindAll(x => x.OfferId == SalesOfferId && x.InvoicePayerClientId == clientId).ToList();
                                                    // Insert Into Invoice Items
                                                    if (ClientInvoicesItemList.Count > 0)
                                                    {
                                                        foreach (var invoiceItem in ClientInvoicesItemList)
                                                        {
                                                            //ObjectParameter InvoiceItemInsertedId = new ObjectParameter("ID", typeof(long));          
                                                            _unitOfWork.InvoiceItems.Add(new InvoiceItem()
                                                            {
                                                                InvoiceId = InvoiceId,
                                                                SalesOfferProductId = invoiceItem.Id,
                                                                Comments = null,
                                                                EInvoiceId = null,
                                                                EInvoiceStatus = null,
                                                                EInvoiceAcceptDate = null
                                                            });
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
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Invalid Id for SalesOffer POS";
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

        public GetOfferInventoryItemsListForPOSResponse GetOfferInventoryItemsListForPOS([FromHeader] GetOfferInventoryItemsFilters filters, string companyname)
        {
            GetOfferInventoryItemsListForPOSResponse Response = new GetOfferInventoryItemsListForPOSResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    /*long InventoryItemId = 0;
                    if (!string.IsNullOrEmpty(headers["InventoryItemId"]) && long.TryParse(headers["InventoryItemId"], out InventoryItemId))
                    {
                        long.TryParse(headers["InventoryItemId"], out InventoryItemId);
                    }
                    int CategoryId = 0;
                    if (!string.IsNullOrEmpty(headers["CategoryId"]) && int.TryParse(headers["CategoryId"], out CategoryId))
                    {
                        int.TryParse(headers["CategoryId"], out CategoryId);
                    }*/
                    /* int StoreId = 0;
                     if (!string.IsNullOrEmpty(headers["StoreId"]) && int.TryParse(headers["StoreId"], out StoreId))
                     {
                         int.TryParse(headers["StoreId"], out StoreId);
                     }
                     else*/
                    if (filters.StoreId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Invalid Store Id";
                        Response.Errors.Add(error);
                    }

                    if (!string.IsNullOrEmpty(filters.InventoryItemName))
                    {
                        filters.InventoryItemName = filters.InventoryItemName.ToLower();
                    }

                    /*                    int CurrentPage = 1;
                                        if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                                        {
                                            int.TryParse(headers["CurrentPage"], out CurrentPage);
                                        }*/

                    /*                    int NumberOfItemsPerPage = 10;
                                        if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                                        {
                                            int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                                        }
                    */
                    if (Response.Result)
                    {
                        var InventoryItemsListQuery = _unitOfWork.InventoryItems.FindAllQueryable(a => true, includes: new[] { "PurchasingUom", "RequstionUom", "InventoryItemCategory", "Priority" }).AsQueryable();
                        if (filters.InventoryItemId != 0)
                        {
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.Id == filters.InventoryItemId).AsQueryable();
                        }
                        if (filters.CategoryId != 0)
                        {
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.InventoryItemCategoryId == filters.CategoryId).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.InventoryItemName))
                        {
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.Name.Contains(filters.InventoryItemName)).AsQueryable();
                        }
                        var InventoryStoreItemList = _unitOfWork.VInventoryStoreItems.FindAllQueryable(a => true).AsQueryable();
                        if (filters.StoreId != 0)
                        {
                            InventoryStoreItemList = InventoryStoreItemList.Where(a => a.InventoryStoreId == filters.StoreId && a.StoreActive == true ).AsQueryable();
                            var InventoryStoreItemsIds = InventoryStoreItemList.Select(a => a.InventoryItemId).Distinct().ToList();
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => InventoryStoreItemsIds.Contains(a.Id)).AsQueryable();
                        }


                        // For Item POS 

                        var InventoryItemsPagedList = PagedList<InventoryItem>.Create(InventoryItemsListQuery.OrderBy(x => x.Name), filters.CurrentPage, filters.NumberOfItemsPerPage);
                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = InventoryItemsPagedList.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = InventoryItemsPagedList.TotalCount
                        };


                        //var InventoryItemsList = InventoryItemsListQuery.ToList();
                        // var InventoryItemsListResponse = new List<InventoryItemInfoForPOS>();
                        var InventoryItemsListResponse = InventoryItemsPagedList.Select(InventoryItemObjDB => new InventoryItemInfoForPOS
                        {
                            ID = InventoryItemObjDB.Id,
                            ItemName = InventoryItemObjDB.Name,
                            ItemCode = InventoryItemObjDB.Code,
                            Category = InventoryItemObjDB.InventoryItemCategory.Name,
                            CategoryId = InventoryItemObjDB.InventoryItemCategoryId,
                            PurchasingUnit = InventoryItemObjDB.PurchasingUom.ShortName,
                            RequestionUnit = InventoryItemObjDB.RequstionUom.ShortName,
                            Balance = Math.Max(InventoryStoreItemList.Where(item => item.InventoryItemId == InventoryItemObjDB.Id).Select(x => x.Balance).Sum(),0),
                            ConvertRateFromPurchasingToRequestionUnit = InventoryItemObjDB.ExchangeFactor1 != null ? (decimal)InventoryItemObjDB.ExchangeFactor1 : 0,

                            ItemImage = (InventoryItemObjDB.ImageUrl != null && InventoryItemObjDB.Id != 0) ? Globals.baseURL + InventoryItemObjDB.ImageUrl : null,
                            Cost1 = InventoryItemObjDB.CostAmount1,
                            Cost2 = InventoryItemObjDB.CostAmount2,
                            Cost3 = InventoryItemObjDB.CostAmount3,
                            CustomPrice = InventoryItemObjDB.CustomeUnitPrice,
                            RequestionUOMShortName = InventoryItemObjDB.RequstionUom.ShortName
                        }).ToList();

                        Response.InventoryItemsList = InventoryItemsListResponse.Where(x => x.Balance > 0).ToList() ;
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

        public SelectDDLResponse GetInventoryItemCategoryListPOS(GetInventoryItemCategoryListFilters filters)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemCategoryList = new List<SelectDDL>();
                /*bool? IsRentItem = null;
                if (!string.IsNullOrEmpty(headers["IsRentItem"]))
                {
                    if (bool.Parse(headers["IsRentItem"]) != true && bool.Parse(headers["IsRentItem"]) != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err110";
                        error.ErrorMSG = "IsRentItem must be true of false ";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        IsRentItem = bool.Parse(headers["IsRentItem"]);
                    }
                }*/
                /*bool? IsAsset = null;
                if (!string.IsNullOrEmpty(headers["IsAsset"]))
                {
                    if (bool.Parse(headers["IsAsset"]) != true && bool.Parse(headers["IsAsset"]) != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err110";
                        error.ErrorMSG = "IsAsset must be true of false ";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        IsAsset = bool.Parse(headers["IsAsset"]);
                    }
                }*/
                /*bool? IsNonStock = null;
                if (!string.IsNullOrEmpty(headers["IsNonStock"]))
                {
                    if (bool.Parse(headers["IsNonStock"]) != true && bool.Parse(headers["IsNonStock"]) != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err110";
                        error.ErrorMSG = "IsNonStock must be true of false ";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        IsNonStock = bool.Parse(headers["IsNonStock"]);
                    }
                }*/

                /*bool? IsFinalProduct = null;
                if (!string.IsNullOrEmpty(headers["IsFinalProduct"]))
                {
                    if (bool.Parse(headers["IsFinalProduct"]) != true && bool.Parse(headers["IsFinalProduct"]) != false)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err110";
                        error.ErrorMSG = "IsFinalProduct must be true of false ";
                        Response.Errors.Add(error);
                    }
                    else
                    {
                        IsFinalProduct = bool.Parse(headers["IsFinalProduct"]);
                    }
                }*/
                var InventoryItemsListQuery = _unitOfWork.VInventoryItems.FindAllQueryable(a => true).AsQueryable();

                if (filters.IsFinalProduct == true)
                {
                    InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.IsFinalProduct == true);
                }
                if (filters.IsRentItem == true)
                {
                    InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.IsRentItem == true);
                }
                if (filters.IsAsset == true)
                {
                    InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.IsAsset == true);
                }
                if (filters.IsNonStock == true)
                {
                    InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.IsNonStock == true);
                }

                if (Response.Result)
                {
                    var ItemCategoryQuerableDB = _unitOfWork.InventoryItemCategories.FindAllQueryable(a => true);
                    List<int> ItemCategoriesList = new List<int>();
                    var InventoryStoreItemsQuerable = _unitOfWork.InventoryStoreItems.FindAllQueryable(a => true);
                    if (filters.StoreId != 0)
                    {
                        InventoryStoreItemsQuerable = InventoryStoreItemsQuerable.Where(x => x.InventoryStoreId == filters.StoreId).AsQueryable();
                    }
                    var InventoryItemIDsList = InventoryStoreItemsQuerable.Where(x => x.FinalBalance > 0).Select(x => x.InventoryItemId).Distinct().ToList();
                    //if (InventoryItemIDsList.Count() > 0)
                    //{
                    InventoryItemsListQuery = InventoryItemsListQuery.Where(x => InventoryItemIDsList.Contains(x.Id)).AsQueryable();
                    ItemCategoriesList = InventoryItemsListQuery.Select(x => x.InventoryItemCategoryId).Distinct().ToList();
                    ItemCategoryQuerableDB = ItemCategoryQuerableDB.Where(item => ItemCategoriesList.Contains(item.Id)).AsQueryable();
                    //if (IsFinalProduct == true || IsRentItem == true || IsAsset == true || IsNonStock == true)
                    //{
                    //}
                    //}


                    var ItemCategoryListDB = ItemCategoryQuerableDB.ToList();
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

        public BaseResponseWithData<string> ManageInventoryStoreItemPricinigForPOS(AddOneInventoryStoreItemPricing Request, long creator)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
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
                        var InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == Request.IDSinventoryItem).FirstOrDefault();
                        if (InventoryItemObjDB != null)
                        {
                            decimal CustomeUnitPrice = InventoryItemObjDB.CustomeUnitPrice;
                            if (Request.Custom != null)
                            {
                                CustomeUnitPrice = (decimal)Request.Custom;
                                InventoryItemObjDB.CustomeUnitPrice = CustomeUnitPrice;
                            }

                            decimal? Price1 = InventoryItemObjDB.CostAmount1;
                            if (Request.Price1 != null)
                            {
                                Price1 = (decimal)Request.Price1;
                                InventoryItemObjDB.CostAmount1 = Price1;
                            }
                            decimal? Price2 = InventoryItemObjDB.CostAmount2;
                            if (Request.Price2 != null)
                            {
                                Price2 = (decimal)Request.Price2;
                                InventoryItemObjDB.CostAmount2 = Price2;
                            }
                            decimal? Price3 = InventoryItemObjDB.CostAmount3;
                            if (Request.Price3 != null)
                            {
                                Price3 = (decimal)Request.Price3;
                                InventoryItemObjDB.CostAmount3 = Price3;
                            }
                            InventoryItemObjDB.ModifiedBy = creator;
                            InventoryItemObjDB.ModifiedDate = DateTime.Now;
                            var res = _unitOfWork.InventoryItems.Update(InventoryItemObjDB);
                            _unitOfWork.Complete();
                            if (res != null)
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

        public BaseResponseWithData<string> AddInventoryItemCostNamePOS(InventoryItemPOSCostNameRequest Request)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
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

                    if (Response.Result)
                    {
                        var InventoryStoreItemListDB = _unitOfWork.InventoryItems.GetAll();
                        if (InventoryStoreItemListDB.Count() > 0)
                        {

                            foreach (var InventoryItem in InventoryStoreItemListDB)
                            {
                                InventoryItem.CostName1 = Request.CostName1;
                                InventoryItem.CostName2 = Request.CostName2;
                                InventoryItem.CostName3 = Request.CostName3;
                            }
                            _unitOfWork.Complete();
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

        public AccountsAndFinanceInventoryItemInfoResponseForPOS GetAccountAndFinanceInventoryStoreItemInfoForPOS(long InventoryItemID, long StoreId, string InventoryItemCode)
        {
            AccountsAndFinanceInventoryItemInfoResponseForPOS Response = new AccountsAndFinanceInventoryItemInfoResponseForPOS();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemInfoObj = new InventoryItemInfoForPOS();
                if (Response.Result)
                {



                    if (InventoryItemCode == null && InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Must be select At least One Id or Code";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (StoreId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Store Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {
                        long? InventoryStoreItemId = null;
                        InventoryItem InventoryItemObjDB = null;
                        if (InventoryItemID != 0)
                        {
                            InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == InventoryItemID).FirstOrDefault();
                            if (InventoryItemObjDB != null)
                            {
                                InventoryItemID = InventoryItemObjDB.Id;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(InventoryItemCode))
                        {
                            InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Code == InventoryItemCode).FirstOrDefault();
                            if (InventoryItemObjDB != null)
                            {
                                var InventoryStoreItemDB = _unitOfWork.InventoryStoreItems.FindAll(x => x.ItemSerial == InventoryItemCode).FirstOrDefault();
                                if (InventoryStoreItemDB != null)
                                {
                                    InventoryStoreItemId = InventoryStoreItemDB.Id;
                                    InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == InventoryStoreItemDB.InventoryItemId).FirstOrDefault();
                                }
                            InventoryItemID = InventoryItemObjDB.Id;
                            }
                        }
                        if (InventoryItemID == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err99";
                            error.ErrorMSG = "Inventory Item is not exist";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (InventoryItemObjDB != null)
                        {
                            var InventoryStoreItemBalance = _unitOfWork.VInventoryStoreItems.FindAll(a => a.InventoryStoreId == StoreId && a.StoreActive == true && a.FinalBalance > 0 && a.InventoryItemId == InventoryItemID).Select(x => x.FinalBalance ?? 0).DefaultIfEmpty(0).Sum();

                            InventoryItemInfoObj.ID = InventoryItemObjDB.Id;
                            InventoryItemInfoObj.ItemName = InventoryItemObjDB.Name;
                            InventoryItemInfoObj.ItemCode = InventoryItemObjDB.Code;
                            InventoryItemInfoObj.CategoryId = InventoryItemObjDB.InventoryItemCategoryId;
                            InventoryItemInfoObj.Category = InventoryItemObjDB.InventoryItemCategory?.Name; //Common.GetInventoryItemCategory(InventoryItemObjDB.InventoryItemCategoryID);
                            InventoryItemInfoObj.PurchasingUnit = InventoryItemObjDB.PurchasingUom?.Name; //Common.GetInventoryUOM(InventoryItemObjDB.PurchasingUOMID);
                            InventoryItemInfoObj.RequestionUnit = InventoryItemObjDB.RequstionUom?.Name; //Common.GetInventoryUOM(InventoryItemObjDB.RequstionUOMID);

                            InventoryItemInfoObj.ConvertRateFromPurchasingToRequestionUnit = InventoryItemObjDB.ExchangeFactor1 != null ? (decimal)InventoryItemObjDB.ExchangeFactor1 : 0;
                            InventoryItemInfoObj.Cost1 = InventoryItemObjDB.CostAmount1;
                            InventoryItemInfoObj.Cost2 = InventoryItemObjDB.CostAmount2;
                            InventoryItemInfoObj.Cost3 = InventoryItemObjDB.CostAmount3;
                            InventoryItemInfoObj.Balance = InventoryStoreItemBalance;
                            InventoryItemInfoObj.CustomPrice = InventoryItemObjDB.CustomeUnitPrice;
                            InventoryItemInfoObj.RequestionUOMShortName = InventoryItemObjDB.RequstionUom?.ShortName;
                            InventoryItemInfoObj.InventoryStoreItemId = InventoryStoreItemId;

                            if (InventoryItemObjDB.ImageUrl != null && InventoryItemObjDB.HasImage == true)
                            {
                                InventoryItemInfoObj.ItemImage = Globals.baseURL + InventoryItemObjDB.ImageUrl;
                            }
                            //decimal Amount = 0;
                            //int CalcType = InventoryItemObjDB.CalculationType;
                            //if (CalcType == 1)
                            //{
                            //    Amount = InventoryItemObjDB.AverageUnitPrice;
                            //}
                            //else if (CalcType == 2)
                            //{
                            //    Amount = InventoryItemObjDB.MaxUnitPrice;
                            //}
                            //else if (CalcType == 3)
                            //{
                            //    Amount = InventoryItemObjDB.LastUnitPrice;
                            //}
                            //else if (CalcType == 4)
                            //{
                            //    Amount = InventoryItemObjDB.CustomeUnitPrice;
                            //}
                            //InventoryItemInfoObj.Amount = Amount;

                            Response.InventoryItemInfo = InventoryItemInfoObj;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err99";
                            error.ErrorMSG = "This Item is not exist";
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
                                                _unitOfWork.Complete();


                                                // Check if Cost greater than Custom inventory Item change it
                                                if (InventoryItemObjDB.CustomeUnitPrice < POInvoiceTotalPriceEGP)
                                                {
                                                    if (POInvoiceTotalPriceEGP != null)
                                                    {
                                                        InventoryItemObjDB.CustomeUnitPrice = (decimal)POInvoiceTotalPriceEGP;
                                                        InventoryItemObjDB.ModifiedDate = DateTime.Now;
                                                        InventoryItemObjDB.ModifiedBy = UserID;
                                                    }
                                                }
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

        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePerItem(AddInventoryItemOpeningBalancePerItem Request, long UserID)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region validation
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
                    
                    // All Inventory Item  From Request
                    if (Request.InventoryStoreID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Invalid Inventory Store ID ";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    long SupplierID = 0;
                    if (Request.SupplierId != null)
                    {
                        SupplierID = (long)Request.SupplierId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Supplier ID ";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(Request.InventoryItemID);
                    if (InventoryItemObjDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Inventory Item ID ";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var inventoryStore = _unitOfWork.InventoryStores.GetById(Request.InventoryStoreID);
                    if (inventoryStore == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Inventory Store ID ";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    #endregion

                    if (Response.Result)
                    {

                        // Loop List  Of Inventory Store with each Items
                        string OperationType = "Opening Balance";

                        // Check Inventory Report Approved and closed or not
                        var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == Request.InventoryStoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();

                        if (CheckInventoryReportListDB.Count > 0)
                        {
                            foreach (var InventoryRep in CheckInventoryReportListDB)
                            {
                                if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                                {
                                    string storeName = _unitOfWork.InventoryStores.GetById(Request.InventoryStoreID).Name;//Common.getInventoryStoreName(ItemOpeningBalancePerStore.InventoryStoreID);
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
                        if (string.IsNullOrEmpty(Request.RecevingData) || !DateTime.TryParse(Request.RecevingData, out RecevingData))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err23";
                            error.ErrorMSG = "Invalid Receving Data.";
                            Response.Errors.Add(error);
                            return Response;
                        }

                        long InventoryAddingOrderID = 0;
                        
                        var newInventroyOrder = new InventoryAddingOrder();
                        newInventroyOrder.OperationType = OperationType;
                        newInventroyOrder.CreationDate = DateTime.Now;
                        newInventroyOrder.CreatedBy = UserID;
                        newInventroyOrder.Revision = 0;
                        newInventroyOrder.SupplierId = (long)Request.SupplierId;
                        newInventroyOrder.Ponumber = null;
                        newInventroyOrder.LoadBy = 0;
                        newInventroyOrder.InventoryStoreId = Request.InventoryStoreID;
                        newInventroyOrder.RecivingDate = RecevingData;
                        newInventroyOrder.ModifiedBy = UserID;
                        newInventroyOrder.ModifiedDate = DateTime.Now;

                        var InventoryAddingOrderInsertion = _unitOfWork.InventoryAddingOrders.Add(newInventroyOrder);
                        _unitOfWork.Complete();

                        if (InventoryAddingOrderInsertion != null)
                        {
                            InventoryAddingOrderID = InventoryAddingOrderInsertion.Id;
                            #region items


                            
                            //var InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(Item => Item.Id == Request.InventoryItemID).FirstOrDefault();
                            if (InventoryItemObjDB != null)
                            {
                                if(Request.InventoryItemCostUpdate == true)
                                {
                                    InventoryItemObjDB.CostAmount1 = Request.CostEGP * Request.percentage;
                                }

                                DateTime? ExpDate = null;
                                if (Request.ExpData != null && Request.ExpData != "")
                                {
                                    ExpDate = DateTime.Parse(Request.ExpData);
                                }
                                    
                                var newInventoryItem = new InventoryAddingOrderItem();
                                newInventoryItem.InventoryAddingOrderId = InventoryAddingOrderID;
                                newInventoryItem.InventoryItemId = Request.InventoryItemID;
                                newInventoryItem.Uomid = InventoryItemObjDB.RequstionUomid;
                                newInventoryItem.ExpDate = ExpDate;
                                newInventoryItem.ItemSerial = Request.Serial;
                                newInventoryItem.QcreportId = null;
                                newInventoryItem.Comments = Request.Comment;
                                newInventoryItem.Poid = null;
                                newInventoryItem.ReqQuantity1 = null;
                                newInventoryItem.RecivedQuantity1 = Request.QTY;
                                newInventoryItem.RecivedQuantityAfter = null;
                                newInventoryItem.RemainQuantity = null;


                                var AddingOrderItemInsertion = _unitOfWork.InventoryAddingOrderItems.Add(newInventoryItem);
                                _unitOfWork.Complete();

                                if (AddingOrderItemInsertion != null)
                                {
                                    long AddingOrderItemId = AddingOrderItemInsertion.Id;
                                    int? StoreLocationID = _unitOfWork.InventoryStoreLocations.Find(a => a.Id == Request.StoreLocationID) != null ? Request.StoreLocationID : null;//Common.CheckInventoryStoreLocationID(ItemDataOBJ.StoreLocationID) ? ItemDataOBJ.StoreLocationID : null;

                                    // Extra Date For PO Items
                                    long? POInvoiceId = null;
                                    decimal? POInvoiceTotalPrice = null;
                                    decimal? POInvoiceTotalCost = null;
                                    int? currencyId = Request.CurrencyId ?? _unitOfWork.Currencies.Find(a => a.IsLocal == true)?.Id ?? 1;
                                    decimal? rateToEGP = 1;
                                    decimal? POInvoiceTotalPriceEGP = null;
                                    decimal? POInvoiceTotalCostEGP = null;
                                    decimal? remainItemPrice = null;
                                    decimal? remainItemCosetEGP = null;
                                    decimal? remainItemCostOtherCU = null;


                                    decimal finalBalance = Request.QTY;
                                    rateToEGP = Request.RateToEGP ?? 1;
                                    POInvoiceTotalPriceEGP = Request.CostEGP ?? 0;
                                    POInvoiceTotalCostEGP = Request.CostEGP ?? 0;
                                    POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                    POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                    remainItemCosetEGP = finalBalance * POInvoiceTotalCostEGP ?? 0;
                                    remainItemCostOtherCU = finalBalance * POInvoiceTotalCost ?? 0;

                                    var InventoryStoreItemOBJ = new InventoryStoreItem();
                                    InventoryStoreItemOBJ.InventoryStoreId = Request.InventoryStoreID;
                                    InventoryStoreItemOBJ.InventoryItemId = Request.InventoryItemID;
                                    InventoryStoreItemOBJ.OrderNumber = InventoryAddingOrderID.ToString();
                                    InventoryStoreItemOBJ.OrderId = InventoryAddingOrderID;
                                    InventoryStoreItemOBJ.CreatedBy = UserID;
                                    InventoryStoreItemOBJ.ModifiedBy = UserID;
                                    InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                                    InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                                    InventoryStoreItemOBJ.OperationType = OperationType;
                                    InventoryStoreItemOBJ.Balance = (double)Request.QTY;
                                    InventoryStoreItemOBJ.Balance1 = Request.QTY;
                                    InventoryStoreItemOBJ.InvenoryStoreLocationId = StoreLocationID;
                                    InventoryStoreItemOBJ.ExpDate = ExpDate;
                                    InventoryStoreItemOBJ.ItemSerial = Request.Serial;
                                    InventoryStoreItemOBJ.FinalBalance = Request.QTY;
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


                                    // Check if Cost greater than Custom inventory Item change it
                                    if (InventoryItemObjDB.CustomeUnitPrice < POInvoiceTotalPriceEGP)
                                    {
                                        if (POInvoiceTotalPriceEGP != null)
                                        {
                                            InventoryItemObjDB.CustomeUnitPrice = (decimal)POInvoiceTotalPriceEGP;
                                            InventoryItemObjDB.ModifiedDate = DateTime.Now;
                                            InventoryItemObjDB.ModifiedBy = UserID;
                                        }
                                    }
                                    var InventoryStorItemInsertion = _unitOfWork.Complete();
                                }
                            }
                            
                            #endregion items
                            Response.Result = true;
                            Response.ID = InventoryAddingOrderID;
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
