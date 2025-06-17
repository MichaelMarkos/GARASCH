using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.User;
using NewGaras.Infrastructure.Models.PoInvoice;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using System.Net;
using NewGaras.Infrastructure.Models.PoInvoice.UsedInResponse;
using Microsoft.EntityFrameworkCore;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.Purchase;
using NewGaras.Infrastructure.Models;
using System.Data;
using NewGaras.Infrastructure.Models.Client;
using iTextSharp.text;
using iTextSharp.text.pdf;
using NewGarasAPI.Models.AccountAndFinance;
using System.Web;
using DocumentFormat.OpenXml.Bibliography;
using NewGarasAPI.Models.Purchase;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGarasAPI.Helper;
using NewGaras.Infrastructure.Models.SalesOffer;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace NewGaras.Domain.Services
{
    public class PoInvoiceService : IPoInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private GarasTestContext _Context;
        static readonly string key = "SalesGarasPass";
        private readonly IAdminService _adminService;
        private readonly IPurchesRequestService _purchesRequestService;
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
        public PoInvoiceService(IPurchesRequestService purchesRequestService, IAdminService adminService, IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, GarasTestContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _Context = context;
            _adminService = adminService;
            _purchesRequestService = purchesRequestService; 
        }

        public async Task<GetPoInvoiceDataResponse> GetPoInvoiceData(long POID)
        {
            GetPoInvoiceDataResponse Response = new GetPoInvoiceDataResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                
                if (POID == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err76";
                    error.ErrorMSG = "Invalid PO ID";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (Response.Result)
                {
                    var PODailyJournalEntryDetails = new Infrastructure.Models.PoInvoice.UsedInResponse.PODailyJournalEntryDetails();
                    var ListAccountsWithAdvancedTypeSupplierList = _unitOfWork.AdvanciedSettingAccounts.FindAll(x => x.AdvanciedTypeId == 31).Select(x => x.AccountId).ToList();
                    if (ListAccountsWithAdvancedTypeSupplierList.Count() > 0)
                    {
                        var SupplierAccountsListDB = _unitOfWork.SupplierAccounts.FindAll(x => x.PurchasePoid == POID && ListAccountsWithAdvancedTypeSupplierList.Contains(x.AccountId)).ToList();
                        var IDSOfJournalEntries = SupplierAccountsListDB.Select(x => x.DailyAdjustingEntryId).ToList();
                        var DAilyJournalEntryList = _unitOfWork.DailyJournalEntries.FindAllQueryable(x => IDSOfJournalEntries.Contains(x.Id)).ToList();
                        PODailyJournalEntryDetails.SumPlusAmountSupplierAccounts = SupplierAccountsListDB.Where(x => x.AmountSign == "plus").Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        PODailyJournalEntryDetails.SumMinusAmountSupplierAccounts = SupplierAccountsListDB.Where(x => x.AmountSign == "minus").Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                        PODailyJournalEntryDetails.SumAmountOfJounralEntry = DAilyJournalEntryList.Select(x => x.TotalAmount).DefaultIfEmpty(0).Sum();
                        PODailyJournalEntryDetails.ListOfJounralEntries = DAilyJournalEntryList.Select(x => x.Id).ToList();
                        PODailyJournalEntryDetails.CountOfJournalEntry = PODailyJournalEntryDetails.ListOfJounralEntries.Count();
                    }


                    Response.PODailyJournalEntryDetails = PODailyJournalEntryDetails;

                    Response.PurchasePO = ViewPurchaseOrder(POID, null).Result;
                    //AdminWS adminWS = new AdminWS();
                    Response.PurchasePaymentMethodDDL = (await _adminService.GetPurchasePaymentMethod()).PurchasePaymentMethodList;

                    var PoInvoiceDb = await _unitOfWork.PurchasePOInvoices.FindAsync(a => a.Poid == POID && a.Active == true, new[] { "PurchasePoinvoiceType" });
                    if (PoInvoiceDb != null)
                    {
                        var PurchasePOInvoiceResponse = new PurchasePoInvoice
                        {
                            Id = PoInvoiceDb.Id,
                            InvoiceAttachementID = PoInvoiceDb.InvoiceAttachementId,
                            InvoiceCollectionDueDate = PoInvoiceDb.InvoiceCollectionDueDate != null ? PoInvoiceDb.InvoiceCollectionDueDate.ToString().Split(' ')[0] : null,
                            POID = POID,
                            InvoiceDate = PoInvoiceDb.InvoiceDate != null ? PoInvoiceDb.InvoiceDate.ToString().Split(' ')[0] : null,
                            IsClosed = PoInvoiceDb.IsClosed,
                            IsFinalPriced = PoInvoiceDb.IsFinalPriced,
                            PurchasePOInvoiceTypeID = PoInvoiceDb.PurchasePoinvoiceTypeId,
                            PurchasePOInvoiceTypeName = PoInvoiceDb.PurchasePoinvoiceType.Name,
                            TotalInvoiceCost = PoInvoiceDb.TotalInvoiceCost,
                            TotalInvoicePrice = PoInvoiceDb.TotalInvoicePrice,
                            IsSentToACC = PoInvoiceDb.IsSentToAcc,
                            TransactionId = PoInvoiceDb.TansactionId
                        };
                        Response.PurchasePOInvoice = PurchasePOInvoiceResponse;



                        var PoInvoiceTaxIncludedListDb = await _unitOfWork.PurchasePoinvoiceTaxIncluds.FindAllAsync(a => a.PoinvoiceId == PoInvoiceDb.Id && a.Active == true);
                        if (PoInvoiceTaxIncludedListDb != null && PoInvoiceTaxIncludedListDb.Count() > 0)
                        {
                            Response.PoInvoiceTaxIncludedList = PoInvoiceTaxIncludedListDb.Select(poInvoiceTaxIncluded => new PoInvoiceTaxIncluded
                            {
                                Id = poInvoiceTaxIncluded.Id,
                                Amount = poInvoiceTaxIncluded.Amount,
                                CurrencyID = poInvoiceTaxIncluded.CurrencyId,
                                Percentage = poInvoiceTaxIncluded.Percentage,
                                POInvoiceID = PoInvoiceDb.Id,
                                POTaxIncludedTypeID = poInvoiceTaxIncluded.PotaxIncludedTypeId,
                                RateToEGP = poInvoiceTaxIncluded.RateToEgp
                            }).ToList();
                        }

                        var PoInvoiceExtraFeesListDb = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAllAsync(a => a.PoinvoiceId == PoInvoiceDb.Id && a.Active == true);
                        if (PoInvoiceExtraFeesListDb != null && PoInvoiceExtraFeesListDb.Count() > 0)
                        {
                            Response.PoInvoiceExtraFeesList = PoInvoiceExtraFeesListDb.Select(poInvoiceExtraFees => new PoInvoiceExtraFees
                            {
                                Id = poInvoiceExtraFees.Id,
                                Amount = poInvoiceExtraFees.Amount,
                                CurrencyID = poInvoiceExtraFees.CurrencyId,
                                Percentage = poInvoiceExtraFees.Percentage,
                                POInvoiceID = PoInvoiceDb.Id,
                                POInvoiceExtraFeesTypeID = poInvoiceExtraFees.PoinvoiceExtraFeesTypeId,
                                RateToEGP = poInvoiceExtraFees.RateToEgp,
                                Comment = poInvoiceExtraFees.Comment,
                                POItemId = poInvoiceExtraFees.PoitemId
                            }).ToList();
                        }
                    }

                    Response.CurrencyDDL = GetCurrenciesList().Result;

                    var POInvoiceTypesDb = await _unitOfWork.PurchasePoinvoiceTypes.GetAllAsync();
                    if (POInvoiceTypesDb != null && POInvoiceTypesDb.Count() > 0)
                    {
                        Response.PoInvoiceTypesDDL = POInvoiceTypesDb.Select(poInvoiceType => new SelectDDL
                        {
                            ID = poInvoiceType.Id,
                            Name = poInvoiceType.Name
                        }).ToList();
                    }

                    var POInvoiceTaxIncludedTypesDb = await _unitOfWork.PurchasePoinvoiceTaxIncludedTypes.GetAllAsync();
                    if (POInvoiceTaxIncludedTypesDb != null && POInvoiceTaxIncludedTypesDb.Count() > 0)
                    {
                        Response.PoInvoiceTaxIncludedTypesDDL = POInvoiceTaxIncludedTypesDb.Select(poInvoiceTaxIncludedType => new SelectDDL
                        {
                            ID = poInvoiceTaxIncludedType.Id,
                            Name = poInvoiceTaxIncludedType.Name
                        }).ToList();
                    }

                    var POInvoiceExtraFeesTypesDb = await _unitOfWork.PurchasePoinvoiceExtraFeesTypes.GetAllAsync();
                    if (POInvoiceExtraFeesTypesDb != null && POInvoiceExtraFeesTypesDb.Count() > 0)
                    {
                        Response.PoInvoiceExtraFeesTypesDDL = POInvoiceExtraFeesTypesDb.Select(poInvoiceExtraFeesType => new SelectDDL
                        {
                            ID = poInvoiceExtraFeesType.Id,
                            Name = poInvoiceExtraFeesType.Name
                        }).ToList();
                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<ViewPurchaseOrderResponse> ViewPurchaseOrder(long? POID, string SupplierInvoiceSerial)
        {
            ViewPurchaseOrderResponse Response = new ViewPurchaseOrderResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    // filter By POID or Supplier Invoice Serial

                   
                    if(POID == null && (!string.IsNullOrEmpty(SupplierInvoiceSerial)))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "Invalid PO ID or Supplier Invoice Serial";
                        Response.Errors.Add(error);
                        return Response;
                    }





                    // Grouped by DAte as Inquiry 
                    //var InventoryMatrialAddingOrder


                    var POObjDB = await _unitOfWork.PurchasePOes.FindAsync(x => x.Active == true && x.Id == POID, new[] { "ToSupplier", "InventoryAddingOrderItems" });
                    if (POObjDB != null)
                    {
                        Response.PONumber = POID??0;
                        Response.RequestDate = POObjDB.RequestDate.ToShortDateString();
                        Response.CreationDate = POObjDB.CreationDate.ToShortDateString();
                        Response.Status = POObjDB.Status;
                        Response.AccountantApprovalStatus = POObjDB.ApprovalStatus;
                        Response.SupplierName = POObjDB.ToSupplier?.Name;
                        Response.SupplierId = POObjDB.ToSupplierId;

                        Response.POTypeName = "";
                        if (POObjDB.PotypeId == 1)
                        {
                            Response.POTypeName = "Local / Country";
                        }
                        else if (POObjDB.PotypeId == 2)
                        {
                            Response.POTypeName = "Import";
                        }

                    }
                    var POItemList = new List<PurchaseOrderItem>();
                    var POItemListDB = await _unitOfWork.PurchasePOItems.FindAllAsync(x => x.PurchasePoid == POID, includes: new[] { "Uom", "InventoryItem", "InventoryItem.PurchasingUom", "InventoryItem.RequstionUom", "InventoryMatrialRequestItem" });
                    foreach (var Data in POItemListDB)
                    {
                        var POItemObj = new PurchaseOrderItem();


                        string PRItemStoreName = "N/A";
                        var PurchaseRequestitemDB = await _unitOfWork.VPurchaseRequestItems.FindAsync(x => x.Id == Data.PurchaseRequestItemId);
                        if (PurchaseRequestitemDB != null)
                        {
                            var LoadObjDB = await _unitOfWork.VPurchaseRequests.FindAsync(x => x.Id == Data.PurchaseRequestItemId);
                            if (LoadObjDB != null)
                            {
                                if (LoadObjDB.FromInventoryStoreName != null && LoadObjDB.FromInventoryStoreName != "")
                                {
                                    PRItemStoreName = LoadObjDB.FromInventoryStoreName;
                                }
                                else
                                {
                                    PRItemStoreName = "N/A";
                                }
                            }
                        }

                        var InventoyItemObj = Data.InventoryItem;
                        decimal factor = InventoyItemObj?.ExchangeFactor1 != null ? (decimal)Data.InventoryItem?.ExchangeFactor1 : 0; // Data.ExchangeFactor;

                        POItemObj.ID = Data.Id;
                        POItemObj.FromStoreName = PRItemStoreName; // Common.GetPRItemStoreName(Data.PurchaseRequestItemID);
                        POItemObj.Comment = Data.Comments != null && Data.Comments != "" ? Data.Comments : "N/A";
                        POItemObj.InventoryItemID = Data.InventoryItemId;
                        POItemObj.InventoryItemCode = InventoyItemObj?.Code;
                        POItemObj.InventoryItemName = InventoyItemObj?.Name;
                        POItemObj.UOMID = Data.Uomid;
                        POItemObj.UOMShortName = Data.Uom?.ShortName;
                        POItemObj.FabricationOrderID = Data.FabricationOrderId;
                        POItemObj.FabricationOrderNumber = Data.FabricationOrder?.FabNumber?.ToString();  //!= null ? Data.FabNumber.ToString() : "N/A";
                        POItemObj.ProjectID = Data.ProjectId;
                        POItemObj.ProjectName = Data.Project?.SalesOffer?.ProjectName;  // Data.ProjectName != null ? Data.ProjectName.ToString() : "N/A";
                        POItemObj.RecivedQuantity = (decimal?)Data.RecivedQuantity1 ?? 0;
                        POItemObj.RecivedQuantityUOP = POObjDB.InventoryAddingOrderItems.Where(x => x.InventoryItemId == Data.InventoryItemId && x.Poid == POID).FirstOrDefault()?.RecivedQuantityUop ?? 0;
                        POItemObj.ReqQuantity = (decimal?)Data.ReqQuantity1 ?? 0;
                        POItemObj.RemainQty = POItemObj.ReqQuantity - POItemObj.RecivedQuantity;
                        POItemObj.StockQTY = 0;
                        POItemObj.EstimatedCost = Data.EstimatedCost != null ? Data.EstimatedCost.ToString() : "N/A";
                        POItemObj.CurrencyID = Data.CurrencyId;
                        POItemObj.ConvertRateFromPurchasingToRequestionUnit = factor; // Data.InventoryItem?.ExchangeFactor; // Data.ExchangeFactor;
                        POItemObj.PurchasedUOMShortName = InventoyItemObj?.PurchasingUom.ShortName; // Data.PurchasingUOMShortName;

                        decimal requestionQTY = Data.ReqQuantity1 != null ? (decimal)Data.ReqQuantity1 : 0;
                        decimal purchaseQTY = factor != 0 ? (requestionQTY / factor) : 0;
                        POItemObj.PurchasedQuantity = purchaseQTY;

                        // Get Comment From PRItem Comment 
                        if (PurchaseRequestitemDB != null)
                        {
                            POItemObj.PRItemComment = PurchaseRequestitemDB.Comments;
                        }

                        //Mark Shawky 2023/2/12
                        decimal? RateToEgp = 1;
                        if (Data.RateToEgp != null && Data.RateToEgp != 0)
                        {
                            RateToEgp = Data.RateToEgp;
                        }
                        List<long> InventoryAddingOrderItems =  _unitOfWork.VInventoryAddingOrderItems.FindAll(a => a.Poid == POID && a.InventoryItemId == POItemObj.InventoryItemID).Select(a => a.InventoryAddingOrderId).ToList();
                        POItemObj.MaterialAddingOrdersIds = InventoryAddingOrderItems;
                        POItemObj.InventoryMatrialRequestItemID = Data.InventoryMatrialRequestItemId;
                        POItemObj.InventoryMaterialRequestId = Data.InventoryMatrialRequestItem?.InventoryMatrialRequestId ?? 0; // await _Context.InventoryMatrialRequestItems.Where(a => a.ID == Data.InventoryMatrialRequestItemID).Select(a => a.InventoryMatrialRequestID).FirstOrDefaultAsync();
                        POItemObj.RateToEgp = RateToEgp;
                        POItemObj.PartNumber = InventoyItemObj?.PartNo; // await _Context.InventoryItems.Where(a => a.ID == POItemObj.InventoryItemID).Select(a => a.PartNO).FirstOrDefaultAsync();
                        POItemObj.RequstionUOMID = InventoyItemObj?.RequstionUomid;  //Data.RequstionUOMID;
                        POItemObj.RequstionUOMShortName = InventoyItemObj?.RequstionUom?.ShortName;  // Data.RequstionUOMShortName;
                        POItemObj.PurchasingUOMID = InventoyItemObj?.PurchasingUomid; //Data.PurchasingUOMID;
                        POItemObj.PurchasingUOMShortName = POItemObj.PurchasedUOMShortName;
                        POItemObj.ActualLocalUnitPrice = Data.ActualUnitPrice;
                        POItemObj.ActualUnitPrice = (Data.ActualUnitPrice ?? 0) / RateToEgp;
                        POItemObj.ActualLocalUnitPriceUOR = Data.ActualUnitPrice;
                        POItemObj.ActualUnitPriceUOR = (Data.ActualUnitPrice ?? 0) / RateToEgp;
                        POItemObj.TotalLocalActualPrice = (Data.ActualUnitPrice ?? 0) * ((decimal?)Data.ReqQuantity1 ?? 0);
                        POItemObj.TotalActualPrice = (POItemObj.TotalLocalActualPrice ?? 0) / RateToEgp;
                        POItemObj.FinalLocalUnitCostUOR = Data.FinalUnitCost;
                        POItemObj.FinalUnitCostUOR = (Data.FinalUnitCost ?? 0) / RateToEgp;
                        POItemObj.TotalFinalLocalUnitCost = (Data.FinalUnitCost ?? 0) * ((decimal?)Data.ReqQuantity1 ?? 0);
                        POItemObj.TotalFinalUnitCost = (Data.FinalUnitCost ?? 0) * ((decimal?)Data.ReqQuantity1 ?? 0) / RateToEgp;
                        POItemObj.TotalActualPrice = Data.TotalActualPrice / RateToEgp;
                        POItemObj.InvoiceComments = Data.InvoiceComments;
                        POItemObj.CurrencyName = Data.Currency?.Name; // Common.GetCurrencyName(Data.CurrencyID ?? 0);
                        POItemObj.ActualUnitPriceUnit = Data.ActualUnitPriceUnit ?? POItemObj.RequstionUOMShortName;
                        POItemObj.IsChecked = Data.IsChecked;
                        POItemObj.SupplierInvoiceSerial = Data.SupplierInvoiceSerial;
                        POItemList.Add(POItemObj);
                    }

                    Response.PurchasePOItemList = POItemList;

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

        public async Task<List<SelectDDL>> GetCurrenciesList(string CompanyName = null)
        {

            var CurrenciesList = await _unitOfWork.Currencies.GetAllAsync();
            if (CompanyName == "marinapltq")
            {
                CurrenciesList = CurrenciesList.Where(x => x.Id == 5).ToList();
            }
            var CurrenciesDDL = new List<SelectDDL>();
            foreach (var C in CurrenciesList)
            {
                var DDLObj = new SelectDDL();
                DDLObj.ID = C.Id;
                DDLObj.Name = C.Name;

                CurrenciesDDL.Add(DDLObj);
            }

            return CurrenciesDDL;
        }

        public async Task<BaseMessageResponse> PurchasePOInvoicePDF(string CompanyName, [FromHeader] long POID, [FromHeader] bool? GeneratePDF)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var GetPurchasePOInvoice = new PurchasePOInvoiceVM();

                var GetPurchasePOITemList = new List<PurchasePOItemList>();
                if (Response.Result)
                {
                    var PurchasePOInvoiceView = _unitOfWork.VPurchasePos.FindAll(a => a.Id == POID).FirstOrDefault();

                    var V_PurchasePOItemDB = _unitOfWork.VPurchasePoItems.FindAll(x => x.PurchasePoid == POID).ToList();

                    int CountNotHaveUnitPrice = V_PurchasePOItemDB.Where(x => x.ActualUnitPrice == 0 || x.ActualUnitPrice == null).Count();
                    bool NotHaveUnitPrice = CountNotHaveUnitPrice > 0 ? true : false;

                    if (NotHaveUnitPrice == true)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "All items not Completely Priced for this PO";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var V_PurchasePOItemUnitPrice = _unitOfWork.VPurchasePoItems.FindAll(x => x.PurchasePoid == POID).FirstOrDefault();

                    var PurchasePOItemDB = _unitOfWork.PurchasePoinvoices.FindAll(x => x.Poid == POID).FirstOrDefault();
                    if (PurchasePOItemDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "This PO ID Doesn't Exist";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var SupplierShippingID = _unitOfWork.Suppliers.FindAll(x => x.Id == PurchasePOInvoiceView.ToSupplierId).Select(x => x.DefaultDelivaryAndShippingMethodId).FirstOrDefault();

                    var OtherShippingmethodName = _unitOfWork.Suppliers.FindAll(x => x.Id == PurchasePOInvoiceView.ToSupplierId).Select(x => x.OtherDelivaryAndShippingMethodName).FirstOrDefault();

                    var DefaultShippingMethodName = _unitOfWork.DeliveryAndShippingMethods.FindAll(x => x.Id == SupplierShippingID).Select(x => x.Name).FirstOrDefault();

                    string ShippingMethodName = "";

                    if (SupplierShippingID != null)
                    {
                        ShippingMethodName = DefaultShippingMethodName;
                    }
                    else
                    {
                        ShippingMethodName = OtherShippingmethodName;
                    }



                    //var unitPrice = V_PurchasePOItemUnitPrice.ActualUnitPrice;


                    foreach (var item in V_PurchasePOItemDB)
                    {
                        var PurchasePOItemListDB = new PurchasePOItemList();

                        PurchasePOItemListDB.Code = item.Code;
                        PurchasePOItemListDB.CuName = Common.GetCurrencyName((int)item.CurrencyId, _Context);
                        PurchasePOItemListDB.For = item.ActualUnitPriceUnit;
                        PurchasePOItemListDB.UnitPrice = item.ActualUnitPrice;
                        PurchasePOItemListDB.TotalPrice = String.Format("{0:n}", item.TotalActualPrice);
                        PurchasePOItemListDB.POQTYUOP = Decimal.Round((decimal)item.RecivedQuantity, 1).ToString();
                        PurchasePOItemListDB.Code = item.Code;
                        PurchasePOItemListDB.MatrialName = item.InventoryItemName;


                        GetPurchasePOITemList.Add(PurchasePOItemListDB);
                    }

                    var TotalOfTotalPriceVAR = String.Format("{0:n}", GetPurchasePOITemList.Sum(x => string.IsNullOrEmpty(x.TotalPrice) ? 0 : decimal.Parse(x.TotalPrice)));

                    //GetPurchasePOITemList[0].TotalPrice = TotalOfTotalPriceVAR.ToString();

                    var unitPrice = GetPurchasePOITemList.Select(x => x.UnitPrice);






                    var ClientInfoDB = _unitOfWork.Clients.FindAll(x => x.OwnerCoProfile == true).Select(x => x.Id).FirstOrDefault();

                    var ClientAddressInfo = _unitOfWork.VClientAddresses.FindAll(x => x.ClientId == ClientInfoDB).FirstOrDefault();

                    var ClientFaxInfo = _unitOfWork.ClientFaxes.FindAll(x => x.ClientId == ClientInfoDB && x.Fax != null).Select(x => x.Fax).FirstOrDefault();

                    var ClientMobileNumberInfo = _unitOfWork.ClientMobiles.FindAll(x => x.ClientId == ClientInfoDB && x.Mobile != null).Select(x => x.Mobile).FirstOrDefault();

                    var ClientPhoneNumberInfo = _unitOfWork.ClientPhones.FindAll(x => x.ClientId == ClientInfoDB).Select(x => x.Phone).FirstOrDefault();

                    var ClientFullInfo = _Context.Clients.Where(x => x.OwnerCoProfile == true).FirstOrDefault();

                    var ClientInfo = new ClientInfoVM();

                    ClientInfo.Address = ClientAddressInfo?.Address;
                    ClientInfo.Country = ClientAddressInfo?.Country;
                    ClientInfo.Governorate = ClientAddressInfo?.Governorate;

                    if (ClientMobileNumberInfo != null)
                    {
                        ClientInfo.MobileNumber = ClientMobileNumberInfo;

                    }
                    if (ClientPhoneNumberInfo != null)
                    {
                        ClientInfo.PhoneNumber = ClientPhoneNumberInfo;

                    }
                    if (ClientFaxInfo != null)
                    {
                        ClientInfo.FaxNumber = ClientFaxInfo;

                    }

                    ClientInfo.CommercialRecord = ClientFullInfo?.CommercialRecord;
                    ClientInfo.TaxCard = ClientFullInfo?.TaxCard;
                    ClientInfo.Website = ClientFullInfo?.WebSite;

                    var dt = new System.Data.DataTable("grid");


                    dt.Columns.AddRange(new DataColumn[8] { new DataColumn("No"),
                                                     new DataColumn("Matrial Name"),
                                                     new DataColumn("Code"),
                                                     new DataColumn("Po QTY UOP") ,
                                                     new DataColumn("Unit Price"),
                                                     new DataColumn("CU."),
                                                     new DataColumn("for") ,
                                                     new DataColumn("Total Price"),

                    });
                    var Counter = 1;
                    var PurchasePOItemFinallist = GetPurchasePOITemList;

                    if (PurchasePOItemFinallist != null)
                    {
                        foreach (var item in PurchasePOItemFinallist)
                        {

                            //string CounterString = Counter == 0 ? "" : Counter.ToString();

                            dt.Rows.Add(
                                Counter.ToString(),
                                item.MatrialName != null ? item.MatrialName : "-",
                                item.Code != null ? item.Code : "-",
                                item.POQTYUOP != null ? item.POQTYUOP : "-",
                                item.UnitPrice,
                                item.CuName != null ? item.CuName : "-",
                                item.For != null ? item.For : "-",
                                item.TotalPrice != null ? item.TotalPrice : "-"

                                );
                            Counter++;
                        }
                    }



                    var dt2 = new System.Data.DataTable("grid");


                    dt2.Columns.AddRange(new DataColumn[4] { new DataColumn("No	"),
                                                     new DataColumn("Matrial Name"),
                                                     new DataColumn("Code"),
                                                     new DataColumn("Po QTY UOP") ,


                    });
                    var Counter2 = 1;
                    var PurchasePOItemFinallist2 = GetPurchasePOITemList;
                    if (PurchasePOItemFinallist2 != null)
                    {
                        foreach (var item in PurchasePOItemFinallist)
                        {

                            //string CounterString = Counter == 0 ? "" : Counter.ToString();

                            dt2.Rows.Add(
                                Counter2.ToString(),
                                item.MatrialName != null ? item.MatrialName : "-",
                                item.Code != null ? item.Code : "-",
                                item.POQTYUOP != null ? item.POQTYUOP : "-"


                                );
                            Counter++;
                        }
                    }







                    var PurchasePOInvoiceDB = new PurchasePOInvoiceVM();

                    PurchasePOInvoiceDB.ID = PurchasePOInvoiceView.Id;
                    //PurchasePOInvoiceDB.InvoiceAttachementID = PurchasePOInvoiceView.InvoiceAttachementID;
                    PurchasePOInvoiceDB.InvoiceCollectionDueDate = PurchasePOItemDB.InvoiceCollectionDueDate.ToString().Split(' ')[0];
                    PurchasePOInvoiceDB.InvoiceDate = PurchasePOItemDB.InvoiceDate.ToString().Split(' ')[0];
                    PurchasePOInvoiceDB.POStatus = PurchasePOInvoiceView.Status;
                    PurchasePOInvoiceDB.InvoiceStatus = PurchasePOInvoiceView.ApprovalStatus;
                    PurchasePOInvoiceDB.SupplierName = PurchasePOInvoiceView.SupplierName;
                    PurchasePOInvoiceDB.POID = PurchasePOInvoiceView.Id;
                    PurchasePOInvoiceDB.PurchasePOInvoiceTypeIDName = PurchasePOInvoiceView.TypeName;
                    PurchasePOInvoiceDB.TotalInvoicePrice = PurchasePOInvoiceView.TotalInvoicePrice != null ? PurchasePOInvoiceView.TotalInvoicePrice.ToString() : "0";
                    PurchasePOInvoiceDB.ToSupplierID = PurchasePOInvoiceView.ToSupplierId;


                    var SupplierPaymenDB = _Context.SupplierPaymentTerms.Where(x => x.SupplierId == PurchasePOInvoiceDB.ToSupplierID).ToList();

                    //Start PDF Service


                    if (GeneratePDF == null)
                    {
                        if (unitPrice != null)
                        {
                            MemoryStream ms = new MemoryStream();

                            //Size of page
                            Document document = new Document(PageSize.LETTER);


                            PdfWriter pw = PdfWriter.GetInstance(document, ms);

                            //Call the footer Function

                            pw.PageEvent = new HeaderFooter2();

                            document.Open();

                            //Handle fonts and Sizes +  Attachments images logos 

                            iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.LETTER);

                            //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                            //document.SetMargins(0, 0, 20, 20);
                            BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                            Font font = new Font(bf, 8, Font.NORMAL);

                            String path = Path.Combine(_host.WebRootPath, "/Attachments");

                            if (CompanyName == "marinaplt")
                            {
                                var PurchasePOInvoiceDB2 = new PurchasePOInvoiceVM();



                                string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                                //Image logo = Image.GetInstance(PDFp_strPath);
                                //logo.SetAbsolutePosition(80f, 50f);
                                //logo.ScaleAbsolute(600f,600f);



                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                                jpg.SetAbsolutePosition(60f, 750f);
                                //document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 18, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -20;


                                Chunk cc = new Chunk("Purchase PO Invoice " + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 190, 9, 15, 30);


                                prgHeading.Add(cc);

                                document.Add(prgHeading);

                            }
                            else if (CompanyName == "piaroma")
                            {

                                string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                                Image logo = Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -30;


                                Chunk cc = new Chunk("Purchase PO Invoice " + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                                prgHeading.Add(cc);

                                document.Add(prgHeading);


                            }
                            else if (CompanyName == "Garastest")
                            {
                                string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                                Image logo = Image.GetInstance(GarasTest_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                                logo.ScaleAbsolute(300f, 300f);


                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(GarasTest_p_strPath);
                                document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -10;



                                Chunk cc = new Chunk("Purchase PO Invoice " + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                                prgHeading.Add(cc);


                                //prgHeading.Add(new Chunk("Inventory Store Item Report".ToUpper(), fntHead));

                                document.Add(prgHeading);
                            }




                            //Adding paragraph for report generated by  
                            Paragraph prgGeneratedBY = new Paragraph();
                            BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                            prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                            document.Add(prgGeneratedBY);



                            //Adding a line  
                            Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_CENTER, 1)));
                            document.Add(p);





                            PdfPTable tableHeading = new PdfPTable(4);

                            tableHeading.WidthPercentage = 100;

                            tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                            PdfPCell cell3 = new PdfPCell();
                            string cell3text = PurchasePOInvoiceDB.PurchasePOInvoiceTypeIDName;
                            cell3.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cell3.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cell3.BorderColor = new BaseColor(4, 189, 189);
                            cell3.PaddingBottom = 15;
                            cell3.PaddingTop = 15;
                            cell3.BackgroundColor = new BaseColor(4, 189, 189);

                            PdfPCell cell4 = new PdfPCell();
                            string cell4text = "Purchase PO ID Number " + PurchasePOInvoiceDB.POID;
                            cell4.Phrase = new Phrase(cell4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
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



                            PdfPTable TableHeaders = new PdfPTable(4);

                            TableHeaders.WidthPercentage = 100;

                            TableHeaders.SetTotalWidth(new float[] { 80, 380, 150, 65 });


                            PdfPCell cellPONumber = new PdfPCell();
                            string cellPONumbertext = "PO Number: ";
                            cellPONumber.Phrase = new Phrase(cellPONumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPONumber.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellPONumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPONumber.BorderColor = BaseColor.WHITE;
                            cellPONumber.PaddingTop = 15;


                            PdfPCell cellPOID = new PdfPCell();
                            string cellPOIDtext = PurchasePOInvoiceDB.POID.ToString();
                            cellPOID.Phrase = new Phrase(cellPOIDtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPOID.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellPOID.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPOID.BorderColor = BaseColor.WHITE;
                            cellPOID.PaddingTop = 15;


                            PdfPCell cellPOType = new PdfPCell();
                            string cellPOTypetext = "PO Type:";
                            cellPOType.Phrase = new Phrase(cellPOTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPOType.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellPOType.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPOType.BorderColor = BaseColor.WHITE;
                            cellPOType.PaddingTop = 15;


                            PdfPCell cellPOTypeName = new PdfPCell();
                            string cellPOTypeNametext = PurchasePOInvoiceDB.PurchasePOInvoiceTypeIDName;
                            cellPOTypeName.Phrase = new Phrase(cellPOTypeNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPOTypeName.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellPOTypeName.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPOTypeName.BorderColor = BaseColor.WHITE;
                            cellPOTypeName.PaddingTop = 15;

                            TableHeaders.AddCell(cellPONumber);
                            TableHeaders.AddCell(cellPOID);
                            TableHeaders.AddCell(cellPOType);
                            TableHeaders.AddCell(cellPOTypeName);


                            PdfPCell CellInvoiceDate = new PdfPCell();
                            string CellInvoiceDatetext = "Invoice Date: ";
                            CellInvoiceDate.Phrase = new Phrase(CellInvoiceDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontD = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoiceDate.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceDate.BorderColor = BaseColor.WHITE;
                            CellInvoiceDate.PaddingTop = 10;


                            PdfPCell CellFInvoiceDate = new PdfPCell();
                            string CellFInvoiceDatetext = PurchasePOInvoiceDB.InvoiceDate;
                            CellFInvoiceDate.Phrase = new Phrase(CellFInvoiceDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellFInvoiceDate.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellFInvoiceDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellFInvoiceDate.BorderColor = BaseColor.WHITE;
                            CellFInvoiceDate.PaddingTop = 10;
                            CellFInvoiceDate.Bottom = 15;


                            PdfPCell CellInvoicePrice = new PdfPCell();
                            string CellInvoicePricetext = "Total Invoice Price:";
                            CellInvoicePrice.Phrase = new Phrase(CellInvoicePricetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontt = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoicePrice.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellInvoicePrice.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoicePrice.BorderColor = BaseColor.WHITE;
                            CellInvoicePrice.PaddingTop = 10;
                            CellInvoicePrice.Bottom = 15;


                            PdfPCell CellInvoicePriceText = new PdfPCell();
                            string CellFInvoicePriceText = TotalOfTotalPriceVAR.ToString();
                            CellInvoicePriceText.Phrase = new Phrase(CellFInvoicePriceText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoicePriceText.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellInvoicePriceText.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoicePriceText.BorderColor = BaseColor.WHITE;
                            CellInvoicePriceText.PaddingTop = 10;
                            CellInvoicePriceText.Bottom = 15;

                            TableHeaders.AddCell(CellInvoiceDate);
                            TableHeaders.AddCell(CellFInvoiceDate);
                            TableHeaders.AddCell(CellInvoicePrice);
                            TableHeaders.AddCell(CellInvoicePriceText);





                            PdfPCell CellSupplier = new PdfPCell();
                            string CellSuppliertext = "Supplier: ";
                            CellSupplier.Phrase = new Phrase(CellSuppliertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontK = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellSupplier.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellSupplier.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellSupplier.BorderColor = BaseColor.WHITE;
                            CellSupplier.PaddingTop = 10;


                            PdfPCell CellSupplierName = new PdfPCell();
                            string CellSupplierNametext = PurchasePOInvoiceDB.SupplierName;
                            CellSupplierName.Phrase = new Phrase(CellSupplierNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontQ = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellSupplierName.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellSupplierName.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellSupplierName.BorderColor = BaseColor.WHITE;
                            CellSupplierName.PaddingTop = 10;
                            CellSupplierName.Bottom = 15;


                            PdfPCell CellPOStatus = new PdfPCell();
                            string CellPOStatustext = "PO Status:";
                            CellPOStatus.Phrase = new Phrase(CellPOStatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontL = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellPOStatus.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellPOStatus.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellPOStatus.BorderColor = BaseColor.WHITE;
                            CellPOStatus.PaddingTop = 10;
                            CellPOStatus.Bottom = 15;


                            PdfPCell CellPOStatusText = new PdfPCell();
                            string CellgPOStatusText = PurchasePOInvoiceDB.POStatus;
                            CellPOStatusText.Phrase = new Phrase(CellgPOStatusText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontM = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellPOStatusText.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellPOStatusText.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellPOStatusText.BorderColor = BaseColor.WHITE;
                            CellPOStatusText.PaddingTop = 10;
                            CellPOStatusText.Bottom = 15;

                            TableHeaders.AddCell(CellSupplier);
                            TableHeaders.AddCell(CellSupplierName);
                            TableHeaders.AddCell(CellPOStatus);
                            TableHeaders.AddCell(CellPOStatusText);






                            PdfPCell CellInvoiceStatus = new PdfPCell();
                            string CellInvoiceStatustext = "Invoice Status: ";
                            CellInvoiceStatus.Phrase = new Phrase(CellInvoiceStatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontN = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoiceStatus.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceStatus.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceStatus.BorderColor = BaseColor.WHITE;
                            CellInvoiceStatus.PaddingTop = 10;


                            PdfPCell CellInvoiceText = new PdfPCell();
                            string CellInvoiceTexttext = PurchasePOInvoiceDB.InvoiceStatus;
                            CellInvoiceText.Phrase = new Phrase(CellInvoiceTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontS = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoiceText.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceText.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceText.BorderColor = BaseColor.WHITE;
                            CellInvoiceText.PaddingTop = 10;
                            CellInvoiceText.Bottom = 15;


                            PdfPCell CellDueDate = new PdfPCell();
                            string CellDueDatetext = "Due Date:";
                            CellDueDate.Phrase = new Phrase(CellDueDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontW = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDueDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDueDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDueDate.BorderColor = BaseColor.WHITE;
                            CellDueDate.PaddingTop = 10;
                            CellDueDate.Bottom = 30;


                            PdfPCell CellCollectionDueDate = new PdfPCell();
                            string CellCollectionDueDatetext = PurchasePOInvoiceDB.InvoiceCollectionDueDate;
                            CellCollectionDueDate.Phrase = new Phrase(CellCollectionDueDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellCollectionDueDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellCollectionDueDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellCollectionDueDate.BorderColor = BaseColor.WHITE;
                            CellCollectionDueDate.PaddingTop = 10;
                            CellCollectionDueDate.Bottom = 30;






                            PdfPCell CellVEmpty = new PdfPCell();
                            string CellVEmptytext = "";
                            CellVEmpty.Phrase = new Phrase(CellVEmptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellVEmpty.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellVEmpty.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellVEmpty.BorderColor = BaseColor.WHITE;
                            CellVEmpty.PaddingTop = 15;
                            CellVEmpty.Bottom = 30;

                            PdfPCell CellV2Empty = new PdfPCell();
                            string CellV2Emptytext = "";
                            CellV2Empty.Phrase = new Phrase(CellV2Emptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellV2Empty.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV2Empty.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV2Empty.BorderColor = BaseColor.WHITE;
                            CellV2Empty.PaddingTop = 15;
                            CellV2Empty.Bottom = 30;

                            PdfPCell CellV3Empty = new PdfPCell();
                            string CellV3Emptytext = "";
                            CellV3Empty.Phrase = new Phrase(CellV3Emptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellV3Empty.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV3Empty.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV3Empty.BorderColor = BaseColor.WHITE;
                            CellV3Empty.PaddingTop = 15;
                            CellV3Empty.Bottom = 30;

                            PdfPCell CellV4Empty = new PdfPCell();
                            string CellV4Emptytext = "";
                            CellV4Empty.Phrase = new Phrase(CellV4Emptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellV4Empty.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV4Empty.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV4Empty.BorderColor = BaseColor.WHITE;
                            CellV4Empty.PaddingTop = 15;
                            CellV4Empty.Bottom = 30;

                            PdfPCell CellTotalPrice = new PdfPCell();
                            string CellTotalPricetext = TotalOfTotalPriceVAR.ToString();
                            CellTotalPrice.Phrase = new Phrase(CellTotalPricetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellTotalPrice.HorizontalAlignment = Element.ALIGN_CENTER;
                            CellTotalPrice.BackgroundColor = (new BaseColor(4, 189, 189));
                            CellTotalPrice.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellTotalPrice.BorderColor = BaseColor.BLACK;
                            //CellTotalPrice.PaddingTop = 15;
                            CellTotalPrice.Bottom = 30;




                            TableHeaders.AddCell(CellInvoiceStatus);
                            TableHeaders.AddCell(CellInvoiceText);
                            TableHeaders.AddCell(CellDueDate);
                            TableHeaders.AddCell(CellDueDate);
                            TableHeaders.AddCell(CellVEmpty);
                            TableHeaders.AddCell(CellV2Empty);
                            TableHeaders.AddCell(CellV3Empty);
                            TableHeaders.AddCell(CellV4Empty);



                            TableHeaders.SpacingAfter = 20;






                            PdfPTable table4 = new PdfPTable(4);

                            table4.WidthPercentage = 100;

                            table4.SetTotalWidth(new float[] { 55, 400, 100, 72 });


                            table4.AddCell(CellVEmpty);
                            table4.AddCell(CellVEmpty);
                            table4.AddCell(CellVEmpty);
                            table4.AddCell(CellTotalPrice);



                            //Adding PdfPTable


                            PdfPTable table = new PdfPTable(dt.Columns.Count);







                            //table Width
                            table.WidthPercentage = 100;

                            //Define Sizes of Cloumns

                            table.SetTotalWidth(new float[] { 12, 85, 20, 25, 20, 15, 15, 25 });
                            table.PaddingTop = 20;

                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                                PdfPCell cell = new PdfPCell();
                                cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                                iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                                //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                                //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                                cell.PaddingBottom = 5;
                                table.AddCell(cell);


                            }


                            //writing table Data
                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                for (int j = 0; j < dt.Columns.Count; j++)
                                {
                                    //table.AddCell(dt.Rows[i][j].ToString());
                                    PdfPCell cell = new PdfPCell();
                                    cell.ArabicOptions = 1;
                                    if (j <= 2)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.Padding = 8;
                                    }
                                    cell.ArabicOptions = 1;
                                    if (j == 3)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        cell.Padding = 8;
                                    }
                                    if (j == 4)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        cell.Padding = 8;
                                    }
                                    if (j == 5)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.Padding = 8;
                                    }
                                    if (j == 7)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        cell.Padding = 8;
                                    }
                                    //else if (j >= 9)
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    //    cell.Padding = 8;
                                    //}
                                    //else
                                    //{
                                    //    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    //    cell.Padding = 8;
                                    //}

                                    if (cell.ArabicOptions == 1)
                                    {
                                        cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                        cell.Padding = 8;

                                    }
                                    else
                                    {
                                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                        cell.Padding = 8;

                                    }

                                    cell.Phrase = new Phrase(1, dt.Rows[i][j].ToString(), font);
                                    //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                                    table.AddCell(cell);

                                }

                            }







                            //PdfPTable tableSupplierTerm = new PdfPTable(1);

                            //tableSupplierTerm.WidthPercentage = 100;

                            //foreach (var item in SupplierPaymenDB)
                            //{
                            //    PdfPCell cellSupplierTerms = new PdfPCell();
                            //    //string CellDirectorText = "";
                            //    if (SupplierPaymenDB != null)
                            //    {
                            //        cellSupplierTerms.Phrase = new Phrase("-" + " " + item.Percentage + " " + item.Name + " " + item.Description, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                            //    }
                            //    else
                            //    {
                            //        cellSupplierTerms.Phrase = new Phrase("Not determine yet ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                            //    }
                            //    iTextSharp.text.Font cellSupplierTermsfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            //    cellSupplierTerms.HorizontalAlignment = Element.ALIGN_LEFT;
                            //    cellSupplierTerms.VerticalAlignment = Element.ALIGN_MIDDLE;
                            //    cellSupplierTerms.BorderColor = BaseColor.WHITE;
                            //    tableSupplierTerm.AddCell(cellSupplierTerms);
                            //};


                            //}















                            PdfPTable table3 = new PdfPTable(4);

                            table3.WidthPercentage = 100;

                            table3.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                            PdfPCell CellDirector = new PdfPCell();
                            string CellDirectorText = "";
                            CellDirector.Phrase = new Phrase(CellDirectorText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDirector.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector.BorderColor = BaseColor.WHITE;
                            //CellDirector.PaddingTop = 15;


                            PdfPCell CellDirector2 = new PdfPCell();
                            string CellDirectortext2 = "";
                            CellDirector2.Phrase = new Phrase(CellDirectortext2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector2.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector2.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector2.BorderColor = BaseColor.WHITE;
                            //CellDirector2.PaddingTop = 15;
                            //CellDirector2.Bottom = 15;


                            PdfPCell CellDirector3 = new PdfPCell();
                            string CellDirector3text = "Purchasing Director";
                            CellDirector3.Phrase = new Phrase(CellDirector3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector3.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDirector3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector3.BorderColor = BaseColor.WHITE;
                            //CellDirector3.PaddingTop = 30;
                            //CellDirector3.Bottom = 30;


                            PdfPCell CellDirector4 = new PdfPCell();
                            string CellDirector4text = "";
                            CellDirector4.Phrase = new Phrase(CellDirector4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontVs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector4.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector4.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector4.BorderColor = BaseColor.WHITE;
                            //CellDirector4.PaddingTop = 15;
                            //CellDirector4.Bottom = 30;









                            PdfPTable table5 = new PdfPTable(1);

                            table5.WidthPercentage = 100;


                            PdfPCell CellDirector5 = new PdfPCell();
                            string CellDirector5Text = "Supplier Payment Terms:";
                            CellDirector5.Phrase = new Phrase(CellDirector5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontNi5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector5.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector5.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector5.BorderColor = BaseColor.WHITE;
                            CellDirector.PaddingTop = 15;
                            table5.AddCell(CellDirector5);
                            if (SupplierPaymenDB != null && SupplierPaymenDB.Count != 0)
                            {
                                foreach (var item in SupplierPaymenDB)
                                {
                                    PdfPCell cellSupplierTerms = new PdfPCell();
                                    //string CellDirectorText = "";

                                    cellSupplierTerms.Phrase = new Phrase("-" + " " + item.Percentage + " " + item.Name + " " + item.Description, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                                    iTextSharp.text.Font cellSupplierTermsfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                                    cellSupplierTerms.HorizontalAlignment = Element.ALIGN_LEFT;
                                    cellSupplierTerms.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    cellSupplierTerms.BorderColor = BaseColor.WHITE;
                                    cellSupplierTerms.PaddingTop = 15;
                                    table5.AddCell(cellSupplierTerms);
                                };
                            }
                            else
                            {
                                PdfPCell cellSupplierTerms = new PdfPCell();
                                //string CellDirectorText = "";

                                cellSupplierTerms.Phrase = new Phrase("Not determine yet", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                                iTextSharp.text.Font cellSupplierTermsfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                                //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                                //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                                cellSupplierTerms.HorizontalAlignment = Element.ALIGN_LEFT;
                                cellSupplierTerms.VerticalAlignment = Element.ALIGN_MIDDLE;
                                cellSupplierTerms.BorderColor = BaseColor.WHITE;
                                cellSupplierTerms.PaddingTop = 15;
                                table5.AddCell(cellSupplierTerms);
                            }



                            PdfPCell CellDirector7 = new PdfPCell();
                            string CellDirector7text = "";
                            CellDirector7.Phrase = new Phrase(CellDirector7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontWs7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector7.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDirector7.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector7.BorderColor = BaseColor.WHITE;
                            CellDirector7.PaddingTop = 15;
                            CellDirector7.Bottom = 30;


                            PdfPCell CellDirector8 = new PdfPCell();
                            string CellDirector8text = "";
                            CellDirector8.Phrase = new Phrase(CellDirector8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontVs8 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellDirector8.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector8.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector8.BorderColor = BaseColor.WHITE;
                            CellDirector8.PaddingTop = 15;
                            CellDirector8.Bottom = 30;




                            PdfPTable ShippingMethodNametable = new PdfPTable(1);

                            ShippingMethodNametable.WidthPercentage = 100;


                            PdfPCell CellShippingMethodName = new PdfPCell();
                            string CellShippingMethodNametext = "Shipping Method:";
                            CellShippingMethodName.Phrase = new Phrase(CellShippingMethodNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font CellShippingMethodNametextfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellShippingMethodName.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellShippingMethodName.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellShippingMethodName.BorderColor = BaseColor.WHITE;
                            CellShippingMethodName.PaddingTop = 15;

                            PdfPCell CellShippingMethodNameData = new PdfPCell();
                            if (ShippingMethodName != null)
                            {
                                string CellShippingMethodNameDatatext = "-" + " " + ShippingMethodName;
                                CellShippingMethodNameData.Phrase = new Phrase(CellShippingMethodNameDatatext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                            }
                            else
                            {
                                string CellShippingMethodNameDatatext = "Not determine yet";
                                CellShippingMethodNameData.Phrase = new Phrase(CellShippingMethodNameDatatext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                            }

                            //CellShippingMethodNameData.Phrase = new Phrase(CellShippingMethodNameDatatext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font CellShippingMethodNameDatafont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellShippingMethodNameData.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellShippingMethodNameData.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellShippingMethodNameData.BorderColor = BaseColor.WHITE;
                            CellShippingMethodNameData.PaddingTop = 15;



                            ShippingMethodNametable.AddCell(CellShippingMethodName);
                            ShippingMethodNametable.AddCell(CellShippingMethodNameData);


                            Paragraph parag = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, Element.ALIGN_CENTER, 1)));





                            table3.AddCell(CellDirector);
                            table3.AddCell(CellDirector2);
                            table3.AddCell(CellDirector3);
                            table3.AddCell(CellDirector4);

                            table.SpacingAfter = 20;








                            table.SpacingAfter = 20;








                            PdfPTable FooterPart = new PdfPTable(1);

                            FooterPart.WidthPercentage = 100;


                            PdfPCell FooterCell = new PdfPCell();
                            string FooterCelltext = ClientInfo.Address;
                            string FooterCelltext2 = ClientInfo.Governorate;
                            string FooterCelltext3 = ClientInfo.Country;
                            FooterCell.Phrase = new Phrase(FooterCelltext + " , " + FooterCelltext2 + " " + "-" + " " + FooterCelltext3, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font FooterCellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            FooterCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            FooterCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            FooterCell.BorderColor = BaseColor.WHITE;
                            FooterCell.PaddingTop = 15;

                            PdfPCell FooterCell2 = new PdfPCell();
                            string FooterCelltext4 = "";
                            if (ClientMobileNumberInfo != null)
                            {
                                FooterCelltext4 = ClientInfo.MobileNumber;
                            }
                            string FooterCelltext5 = " ";
                            if (ClientPhoneNumberInfo != null)
                            {
                                FooterCelltext5 = ClientInfo.PhoneNumber;
                            }


                            string FooterCelltext7 = ClientInfo.Website;
                            FooterCell2.Phrase = new Phrase(FooterCelltext4 + FooterCelltext5 + " " + "-" + " " + FooterCelltext7, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font FooterCelltextfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            FooterCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                            FooterCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
                            FooterCell2.BorderColor = BaseColor.WHITE;

                            PdfPCell FooterCell3 = new PdfPCell();
                            string FooterCelltext8 = ClientInfo.CommercialRecord;
                            string FooterCelltext9 = ClientInfo.TaxCard;
                            string FooterCelltext10 = ClientInfo.Website;
                            FooterCell3.Phrase = new Phrase("Commercial Record" + " " + FooterCelltext8 + "   " + "Tax Card" + " " + FooterCelltext9 + " " + " Investment taxes", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font FooterCefont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            FooterCell3.HorizontalAlignment = Element.ALIGN_CENTER;
                            FooterCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            FooterCell3.BorderColor = BaseColor.WHITE;



                            FooterPart.AddCell(FooterCell);
                            FooterPart.AddCell(FooterCell2);
                            FooterPart.AddCell(FooterCell3);








                            document.Add(tableHeading);
                            document.Add(TableHeaders);
                            document.Add(table4);
                            document.Add(table);

                            document.Add(table5);

                            //tableSupplierTerm.SpacingBefore = 20;

                            //document.Add(tableSupplierTerm);

                            ShippingMethodNametable.SpacingAfter = 50;

                            document.Add(ShippingMethodNametable);


                            document.Add(table3);


                            parag.SpacingBefore = 270;

                            document.Add(parag);

                            document.Add(FooterPart);


                            document.Close();
                            byte[] result = ms.ToArray();
                            ms = new MemoryStream();
                            ms.Write(result, 0, result.Length);
                            ms.Position = 0;



                            string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                            string PathsTR = "/Attachments/" + CompanyName + "/";
                            String Filepath = Path.Combine(_host.WebRootPath, PathsTR);
                            string p_strPath = Path.Combine(Filepath, FullFileName);

                            File.WriteAllBytes(p_strPath, result);

                            Response.Message = Globals.baseURL + PathsTR + FullFileName;



                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "No Prices in this PO to be Viewd";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        if (GeneratePDF == true)
                        {

                            MemoryStream ms = new MemoryStream();

                            //Size of page
                            Document document = new Document(PageSize.LETTER);


                            PdfWriter pw = PdfWriter.GetInstance(document, ms);

                            //Call the footer Function

                            pw.PageEvent = new HeaderFooter2();

                            document.Open();

                            //Handle fonts and Sizes +  Attachments images logos 

                            iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.LETTER);

                            //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                            //document.SetMargins(0, 0, 20, 20);
                            BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                            Font font = new Font(bf, 8, Font.NORMAL);

                            String path = Path.Combine(_host.WebRootPath, "/Attachments");

                            if (CompanyName == "marinaplt")
                            {
                                var PurchasePOInvoiceDB2 = new PurchasePOInvoiceVM();



                                string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                                //Image logo = Image.GetInstance(PDFp_strPath);
                                //logo.SetAbsolutePosition(80f, 50f);
                                //logo.ScaleAbsolute(600f,600f);



                                if (File.Exists(PDFp_strPath))
                                {
                                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                                    jpg.SetAbsolutePosition(60f, 750f);
                                    //document.Add(logo);
                                    document.Add(jpg);
                                }
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -20;



                                Chunk cc = new Chunk("Purchase PO Invoice " + " ", fntHead);

                                cc.SetBackground(new BaseColor(4, 189, 189), 190, 9, 15, 40);


                                prgHeading.Add(cc);

                                document.Add(prgHeading);

                            }
                            else if (CompanyName == "piaroma")
                            {

                                string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                                Image logo = Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -30;
                                prgHeading.SpacingAfter = 20;


                                Chunk cc = new Chunk("Purchase PO Invoice " + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                                prgHeading.Add(cc);

                                document.Add(prgHeading);


                            }
                            else if (CompanyName == "Garastest")
                            {
                                string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                                Image logo = Image.GetInstance(GarasTest_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                                logo.ScaleAbsolute(300f, 300f);


                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(GarasTest_p_strPath);
                                document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -10;



                                Chunk cc = new Chunk("Purchase PO Invoice " + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 230, 9, 5, 40);


                                prgHeading.Add(cc);


                                //prgHeading.Add(new Chunk("Inventory Store Item Report".ToUpper(), fntHead));

                                document.Add(prgHeading);
                            }




                            //Adding paragraph for report generated by  
                            Paragraph prgGeneratedBY = new Paragraph();
                            BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                            prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                            document.Add(prgGeneratedBY);



                            //Adding a line  
                            //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                            //document.Add(p);





                            PdfPTable tableHeading = new PdfPTable(4);

                            tableHeading.WidthPercentage = 100;

                            tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                            PdfPCell cellPurchasePOInvoiceTypeIDName = new PdfPCell();
                            string cellPurchasePOInvoiceTypeIDNametext = PurchasePOInvoiceDB.PurchasePOInvoiceTypeIDName;
                            cellPurchasePOInvoiceTypeIDName.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPurchasePOInvoiceTypeIDName.HorizontalAlignment = Element.ALIGN_CENTER;
                            cellPurchasePOInvoiceTypeIDName.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPurchasePOInvoiceTypeIDName.BorderColor = new BaseColor(4, 189, 189);
                            cellPurchasePOInvoiceTypeIDName.PaddingBottom = 15;
                            cellPurchasePOInvoiceTypeIDName.PaddingTop = 15;
                            cellPurchasePOInvoiceTypeIDName.BackgroundColor = new BaseColor(4, 189, 189);

                            PdfPCell cellPurchasePOIDNumber = new PdfPCell();
                            string cellPurchasePOIDNumbertext = "Purchase PO ID Number " + PurchasePOInvoiceDB.POID;
                            cellPurchasePOIDNumber.Phrase = new Phrase(cellPurchasePOIDNumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPurchasePOIDNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                            cellPurchasePOIDNumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPurchasePOIDNumber.BorderColor = new BaseColor(4, 189, 189);
                            cellPurchasePOIDNumber.PaddingBottom = 15;
                            cellPurchasePOIDNumber.PaddingTop = 15;
                            cellPurchasePOIDNumber.BackgroundColor = new BaseColor(4, 189, 189);



                            tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                            tableHeading.AddCell(cellPurchasePOIDNumber);
                            tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                            tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);

                            tableHeading.KeepTogether = true;



                            PdfPTable tablePO = new PdfPTable(4);

                            tablePO.WidthPercentage = 100;

                            tablePO.SetTotalWidth(new float[] { 80, 380, 150, 65 });


                            PdfPCell cellPONumber = new PdfPCell();
                            string cellPONumbertext = "PO Number: ";
                            cellPONumber.Phrase = new Phrase(cellPONumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPONumber.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellPONumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPONumber.BorderColor = BaseColor.WHITE;
                            cellPONumber.PaddingTop = 15;


                            PdfPCell cellBPurchasePOInvoice = new PdfPCell();
                            string cellBPurchasePOInvoicetext = PurchasePOInvoiceDB.POID.ToString();
                            cellBPurchasePOInvoice.Phrase = new Phrase(cellBPurchasePOInvoicetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellBPurchasePOInvoice.HorizontalAlignment = Element.ALIGN_LEFT;
                            cellBPurchasePOInvoice.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellBPurchasePOInvoice.BorderColor = BaseColor.WHITE;
                            cellBPurchasePOInvoice.PaddingTop = 15;


                            PdfPCell cellPOType = new PdfPCell();
                            string cellPOTypetext = "PO Type:";
                            cellPOType.Phrase = new Phrase(cellPOTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPOType.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellPOType.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPOType.BorderColor = BaseColor.WHITE;
                            cellPOType.PaddingTop = 15;


                            PdfPCell cellPurchasePOInvoiceType = new PdfPCell();
                            string cellPurchasePOInvoiceTypetext = PurchasePOInvoiceDB.PurchasePOInvoiceTypeIDName;
                            cellPurchasePOInvoiceType.Phrase = new Phrase(cellPurchasePOInvoiceTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            cellPurchasePOInvoiceType.HorizontalAlignment = Element.ALIGN_RIGHT;
                            cellPurchasePOInvoiceType.VerticalAlignment = Element.ALIGN_MIDDLE;
                            cellPurchasePOInvoiceType.BorderColor = BaseColor.WHITE;
                            cellPurchasePOInvoiceType.PaddingTop = 15;

                            tablePO.AddCell(cellPONumber);
                            tablePO.AddCell(cellBPurchasePOInvoice);
                            tablePO.AddCell(cellPOType);
                            tablePO.AddCell(cellPurchasePOInvoiceType);


                            PdfPCell CellInvoiceDate = new PdfPCell();
                            string CellInvoiceDatetext = "Invoice Date: ";
                            CellInvoiceDate.Phrase = new Phrase(CellInvoiceDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontD = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoiceDate.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceDate.BorderColor = BaseColor.WHITE;
                            CellInvoiceDate.PaddingTop = 10;


                            PdfPCell CellInvoiceDateText = new PdfPCell();
                            string CellInvoiceDateTexttext = PurchasePOInvoiceDB.InvoiceDate;
                            CellInvoiceDateText.Phrase = new Phrase(CellInvoiceDateTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellInvoiceDateText.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceDateText.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceDateText.BorderColor = BaseColor.WHITE;
                            CellInvoiceDateText.PaddingTop = 10;
                            CellInvoiceDateText.Bottom = 15;


                            PdfPCell CellTotalInvoicePrice = new PdfPCell();
                            string CellTotalInvoicePricetext = "Total Invoice Price:";
                            CellTotalInvoicePrice.Phrase = new Phrase(CellTotalInvoicePricetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontt = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellTotalInvoicePrice.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellTotalInvoicePrice.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellTotalInvoicePrice.BorderColor = BaseColor.WHITE;
                            CellTotalInvoicePrice.PaddingTop = 10;
                            CellTotalInvoicePrice.Bottom = 15;


                            PdfPCell CellEmpty = new PdfPCell();
                            string CellEmptytext = " - ";
                            CellEmpty.Phrase = new Phrase(CellEmptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellEmpty.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellEmpty.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellEmpty.BorderColor = BaseColor.WHITE;
                            CellEmpty.PaddingTop = 10;
                            CellEmpty.Bottom = 15;

                            tablePO.AddCell(CellInvoiceDate);
                            tablePO.AddCell(CellInvoiceDateText);
                            tablePO.AddCell(CellTotalInvoicePrice);
                            tablePO.AddCell(CellEmpty);





                            PdfPCell CellSupplier = new PdfPCell();
                            string CellSuppliertext = "Supplier: ";
                            CellSupplier.Phrase = new Phrase(CellSuppliertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontK = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellSupplier.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellSupplier.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellSupplier.BorderColor = BaseColor.WHITE;
                            CellSupplier.PaddingTop = 10;


                            PdfPCell CellSupplierName = new PdfPCell();
                            string CellSupplierNametext = PurchasePOInvoiceDB.SupplierName;
                            CellSupplierName.Phrase = new Phrase(CellSupplierNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontQ = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellSupplierName.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellSupplierName.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellSupplierName.BorderColor = BaseColor.WHITE;
                            CellSupplierName.PaddingTop = 10;
                            CellSupplierName.Bottom = 15;


                            PdfPCell CellPOStatus = new PdfPCell();
                            string CellPOStatustext = "PO Status:";
                            CellPOStatus.Phrase = new Phrase(CellPOStatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontL = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellPOStatus.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellPOStatus.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellPOStatus.BorderColor = BaseColor.WHITE;
                            CellPOStatus.PaddingTop = 10;
                            CellPOStatus.Bottom = 15;


                            PdfPCell CellPOStatusText = new PdfPCell();
                            string CellPOStatusTexttext = PurchasePOInvoiceDB.POStatus;
                            CellPOStatusText.Phrase = new Phrase(CellPOStatusTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontM = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                            //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                            //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                            CellPOStatusText.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellPOStatusText.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellPOStatusText.BorderColor = BaseColor.WHITE;
                            CellPOStatusText.PaddingTop = 10;
                            CellPOStatusText.Bottom = 15;

                            tablePO.AddCell(CellSupplier);
                            tablePO.AddCell(CellSupplierName);
                            tablePO.AddCell(CellPOStatus);
                            tablePO.AddCell(CellPOStatusText);






                            PdfPCell CellInvoiceSatus = new PdfPCell();
                            string CellInvoiceSatustext = "Invoice Status: ";
                            CellInvoiceSatus.Phrase = new Phrase(CellInvoiceSatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontN = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellInvoiceSatus.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceSatus.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceSatus.BorderColor = BaseColor.WHITE;
                            CellInvoiceSatus.PaddingTop = 10;


                            PdfPCell CellInvoiceStatusText = new PdfPCell();
                            string CellInvoiceStatusTexttext = PurchasePOInvoiceDB.InvoiceStatus;
                            CellInvoiceStatusText.Phrase = new Phrase(CellInvoiceStatusTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontS = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellInvoiceStatusText.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellInvoiceStatusText.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellInvoiceStatusText.BorderColor = BaseColor.WHITE;
                            CellInvoiceStatusText.PaddingTop = 10;
                            CellInvoiceStatusText.Bottom = 15;


                            PdfPCell CellDueDate = new PdfPCell();
                            string CellDueDatetext = "Due Date:";
                            CellDueDate.Phrase = new Phrase(CellDueDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontW = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDueDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDueDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDueDate.BorderColor = BaseColor.WHITE;
                            CellDueDate.PaddingTop = 10;
                            CellDueDate.Bottom = 30;


                            PdfPCell CellCollectionDueDate = new PdfPCell();
                            string CellCollectionDueDatetext = PurchasePOInvoiceDB.InvoiceCollectionDueDate;
                            CellCollectionDueDate.Phrase = new Phrase(CellCollectionDueDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellCollectionDueDate.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellCollectionDueDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellCollectionDueDate.BorderColor = BaseColor.WHITE;
                            CellCollectionDueDate.PaddingTop = 10;
                            CellCollectionDueDate.Bottom = 30;






                            PdfPCell CellVEmpty = new PdfPCell();
                            string CellVEmptytext = "";
                            CellVEmpty.Phrase = new Phrase(CellVEmptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellVEmpty.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellVEmpty.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellVEmpty.BorderColor = BaseColor.WHITE;
                            CellVEmpty.PaddingTop = 15;
                            CellVEmpty.Bottom = 30;

                            PdfPCell CellV2 = new PdfPCell();
                            string CellV2text = "";
                            CellV2.Phrase = new Phrase(CellV2text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellV2.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV2.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV2.BorderColor = BaseColor.WHITE;
                            CellV2.PaddingTop = 15;
                            CellV2.Bottom = 30;

                            PdfPCell CellV3 = new PdfPCell();
                            string CellV3text = "";
                            CellV3.Phrase = new Phrase(CellV3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV3 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellV3.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV3.BorderColor = BaseColor.WHITE;
                            CellV3.PaddingTop = 15;
                            CellV3.Bottom = 30;

                            PdfPCell CellV4 = new PdfPCell();
                            string CellV4text = "";
                            CellV4.Phrase = new Phrase(CellV4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellV4.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellV4.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV4.BorderColor = BaseColor.WHITE;
                            CellV4.PaddingTop = 15;
                            CellV4.Bottom = 30;

                            PdfPCell CellV5 = new PdfPCell();
                            string CellV5text = " - ";
                            CellV5.Phrase = new Phrase(CellV5text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontV5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellV5.HorizontalAlignment = Element.ALIGN_CENTER;
                            CellV5.BackgroundColor = (new BaseColor(4, 189, 189));
                            CellV5.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellV5.BorderColor = BaseColor.BLACK;
                            CellV5.Bottom = 30;




                            tablePO.AddCell(CellInvoiceSatus);
                            tablePO.AddCell(CellInvoiceStatusText);
                            tablePO.AddCell(CellDueDate);
                            tablePO.AddCell(CellCollectionDueDate);
                            tablePO.AddCell(CellVEmpty);
                            tablePO.AddCell(CellVEmpty);
                            tablePO.AddCell(CellVEmpty);
                            tablePO.AddCell(CellVEmpty);



                            tablePO.SpacingAfter = 20;






                            PdfPTable table4 = new PdfPTable(4);

                            table4.WidthPercentage = 100;

                            table4.SetTotalWidth(new float[] { 55, 400, 100, 98 });


                            table4.AddCell(CellVEmpty);
                            table4.AddCell(CellV2);
                            table4.AddCell(CellV3);
                            table4.AddCell(CellV5);



                            //Adding PdfPTable


                            PdfPTable table = new PdfPTable(dt2.Columns.Count);


                            //table Width
                            table.WidthPercentage = 100;

                            //Define Sizes of Cloumns

                            table.SetTotalWidth(new float[] { 50, 35, 45, 45 });
                            table.PaddingTop = 20;

                            for (int i = 0; i < dt2.Columns.Count; i++)
                            {
                                string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                                PdfPCell cell = new PdfPCell();
                                cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                                iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                                //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                                //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.VerticalAlignment = Element.ALIGN_MIDDLE;

                                cell.PaddingBottom = 5;
                                table.AddCell(cell);


                            }


                            //writing table Data
                            for (int i = 0; i < dt2.Rows.Count; i++)
                            {
                                for (int j = 0; j < dt2.Columns.Count; j++)
                                {
                                    //table.AddCell(dt.Rows[i][j].ToString());
                                    PdfPCell cell = new PdfPCell();
                                    cell.ArabicOptions = 1;
                                    if (j <= 5)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.Padding = 8;
                                    }
                                    else if (j >= 9)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        cell.Padding = 8;
                                    }
                                    else
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                        cell.Padding = 8;
                                    }

                                    if (cell.ArabicOptions == 1)
                                    {
                                        cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                        cell.Padding = 8;

                                    }
                                    else
                                    {
                                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                        cell.Padding = 8;

                                    }

                                    cell.Phrase = new Phrase(1, dt2.Rows[i][j].ToString(), font);
                                    //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                                    table.AddCell(cell);

                                }

                            }







                            PdfPTable tableSupplierTerm = new PdfPTable(1);

                            tableSupplierTerm.WidthPercentage = 100;

                            foreach (var item in SupplierPaymenDB)
                            {
                                PdfPCell cellSupplierTerms = new PdfPCell();
                                //string CellDirectorText = "";
                                if (SupplierPaymenDB != null)
                                {
                                    cellSupplierTerms.Phrase = new Phrase("-" + " " + item.Percentage + " " + item.Name + " " + item.Description, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                                }
                                else
                                {
                                    cellSupplierTerms.Phrase = new Phrase("Not determine yet ", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                                }
                                iTextSharp.text.Font cellSupplierTermsfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                cellSupplierTerms.HorizontalAlignment = Element.ALIGN_LEFT;
                                cellSupplierTerms.VerticalAlignment = Element.ALIGN_MIDDLE;
                                cellSupplierTerms.BorderColor = BaseColor.WHITE;
                                tableSupplierTerm.AddCell(cellSupplierTerms);
                            };


                            //}















                            PdfPTable table3 = new PdfPTable(4);

                            table3.WidthPercentage = 100;

                            table3.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                            PdfPCell CellDirector = new PdfPCell();
                            string CellDirectorText = "";
                            CellDirector.Phrase = new Phrase(CellDirectorText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDirector.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector.BorderColor = BaseColor.WHITE;
                            //CellDirector.PaddingTop = 15;


                            PdfPCell CellDirector2 = new PdfPCell();
                            string CellDirectortext2 = "";
                            CellDirector2.Phrase = new Phrase(CellDirectortext2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellDirector2.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector2.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector2.BorderColor = BaseColor.WHITE;



                            PdfPCell CellDirector3 = new PdfPCell();
                            string CellDirector3text = "Purchasing Director";
                            CellDirector3.Phrase = new Phrase(CellDirector3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector3.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDirector3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector3.BorderColor = BaseColor.WHITE;



                            PdfPCell CellDirector4 = new PdfPCell();
                            string CellDirector4text = "";
                            CellDirector4.Phrase = new Phrase(CellDirector4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontVs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector4.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector4.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector4.BorderColor = BaseColor.WHITE;










                            PdfPTable table5 = new PdfPTable(1);

                            table5.WidthPercentage = 100;


                            PdfPCell CellDirector5 = new PdfPCell();
                            string CellDirector5Text = "Supplier Payment Terms:";
                            CellDirector5.Phrase = new Phrase(CellDirector5Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontNi5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector5.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector5.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector5.BorderColor = BaseColor.WHITE;
                            CellDirector.PaddingTop = 15;


                            PdfPCell CellDirector6 = new PdfPCell();
                            string CellDirectortext6 = "";
                            CellDirector6.Phrase = new Phrase(CellDirectortext6, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontSs6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector6.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector6.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector6.BorderColor = BaseColor.WHITE;
                            CellDirector6.PaddingTop = 15;
                            CellDirector6.Bottom = 15;


                            PdfPCell CellDirector7 = new PdfPCell();
                            string CellDirector7text = "";
                            CellDirector7.Phrase = new Phrase(CellDirector7text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontWs7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector7.HorizontalAlignment = Element.ALIGN_RIGHT;
                            CellDirector7.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector7.BorderColor = BaseColor.WHITE;
                            CellDirector7.PaddingTop = 15;
                            CellDirector7.Bottom = 30;


                            PdfPCell CellDirector8 = new PdfPCell();
                            string CellDirector8text = "";
                            CellDirector8.Phrase = new Phrase(CellDirector8text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFontVs8 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellDirector8.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellDirector8.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellDirector8.BorderColor = BaseColor.WHITE;
                            CellDirector8.PaddingTop = 15;
                            CellDirector8.Bottom = 30;




                            PdfPTable ShippingMethodNametable = new PdfPTable(1);

                            ShippingMethodNametable.WidthPercentage = 100;


                            PdfPCell CellShippingMethodName = new PdfPCell();
                            string CellShippingMethodNametext = "Shipping Method:";
                            CellShippingMethodName.Phrase = new Phrase(CellShippingMethodNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font CellShippingMethodNametextfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            CellShippingMethodName.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellShippingMethodName.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellShippingMethodName.BorderColor = BaseColor.WHITE;
                            CellShippingMethodName.PaddingTop = 15;

                            PdfPCell CellShippingMethodNameData = new PdfPCell();
                            if (ShippingMethodName != null)
                            {
                                string CellShippingMethodNameDatatext = "-" + " " + ShippingMethodName;
                                CellShippingMethodNameData.Phrase = new Phrase(CellShippingMethodNameDatatext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                            }
                            else
                            {
                                string CellShippingMethodNameDatatext = "Not determine yet";
                                CellShippingMethodNameData.Phrase = new Phrase(CellShippingMethodNameDatatext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                            }

                            iTextSharp.text.Font CellShippingMethodNameDatafont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                            CellShippingMethodNameData.HorizontalAlignment = Element.ALIGN_LEFT;
                            CellShippingMethodNameData.VerticalAlignment = Element.ALIGN_MIDDLE;
                            CellShippingMethodNameData.BorderColor = BaseColor.WHITE;
                            CellShippingMethodNameData.PaddingTop = 15;



                            ShippingMethodNametable.AddCell(CellShippingMethodName);
                            ShippingMethodNametable.AddCell(CellShippingMethodNameData);







                            table3.AddCell(CellDirector);
                            table3.AddCell(CellDirector2);
                            table3.AddCell(CellDirector3);
                            table3.AddCell(CellDirector4);

                            table.SpacingAfter = 20;

                            if (SupplierPaymenDB != null)
                            {
                                table5.AddCell(CellDirector5);
                            }

                            Paragraph parag = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, Element.ALIGN_CENTER, 1)));



                            PdfPTable FooterPart = new PdfPTable(1);

                            FooterPart.WidthPercentage = 100;


                            PdfPCell FooterCell = new PdfPCell();
                            string FooterCelltext = ClientInfo.Address;
                            string FooterCelltext2 = ClientInfo.Governorate;
                            string FooterCelltext3 = ClientInfo.Country;
                            FooterCell.Phrase = new Phrase(FooterCelltext + " , " + FooterCelltext2 + " " + "-" + " " + FooterCelltext3, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font FooterCellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            FooterCell.HorizontalAlignment = Element.ALIGN_CENTER;
                            FooterCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                            FooterCell.BorderColor = BaseColor.WHITE;
                            FooterCell.PaddingTop = 15;

                            PdfPCell FooterCell2 = new PdfPCell();
                            string FooterCelltext4 = "";
                            if (ClientMobileNumberInfo != null)
                            {
                                FooterCelltext4 = ClientInfo.MobileNumber;
                            }
                            string FooterCelltext5 = " ";
                            if (ClientPhoneNumberInfo != null)
                            {
                                FooterCelltext5 = ClientInfo.PhoneNumber;
                            }


                            string FooterCelltext7 = ClientInfo.Website;
                            FooterCell2.Phrase = new Phrase(FooterCelltext4 + FooterCelltext5 + " " + "-" + " " + FooterCelltext7, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font FooterCelltextfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            FooterCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                            FooterCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
                            FooterCell2.BorderColor = BaseColor.WHITE;

                            PdfPCell FooterCell3 = new PdfPCell();
                            string FooterCelltext8 = ClientInfo.CommercialRecord;
                            string FooterCelltext9 = ClientInfo.TaxCard;
                            string FooterCelltext10 = ClientInfo.Website;
                            FooterCell3.Phrase = new Phrase("Commercial Record" + " " + FooterCelltext8 + "   " + "Tax Card" + " " + FooterCelltext9 + " " + " Investment taxes", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font FooterCefont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            FooterCell3.HorizontalAlignment = Element.ALIGN_CENTER;
                            FooterCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                            FooterCell3.BorderColor = BaseColor.WHITE;



                            FooterPart.AddCell(FooterCell);
                            FooterPart.AddCell(FooterCell2);
                            FooterPart.AddCell(FooterCell3);







                            table.SpacingAfter = 20;

                            document.Add(tableHeading);
                            document.Add(tablePO);
                            document.Add(table4);
                            document.Add(table);

                            document.Add(table5);

                            tableSupplierTerm.SpacingBefore = 20;

                            document.Add(tableSupplierTerm);

                            ShippingMethodNametable.SpacingAfter = 50;

                            document.Add(ShippingMethodNametable);


                            document.Add(table3);
                            parag.SpacingBefore = 250;

                            document.Add(parag);

                            document.Add(FooterPart);


                            document.Close();
                            byte[] result = ms.ToArray();
                            ms = new MemoryStream();
                            ms.Write(result, 0, result.Length);
                            ms.Position = 0;



                            string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                            string PathsTR = "/Attachments/" + CompanyName + "/";
                            String Filepath = _host.WebRootPath + "/" + PathsTR;
                            string p_strPath = Filepath + "/" + FullFileName;
                            if (!System.IO.File.Exists(p_strPath))
                            {
                                var objFileStrm = System.IO.File.Create(p_strPath);
                                objFileStrm.Close();
                            }
                            File.WriteAllBytes(p_strPath, result);

                            Response.Message = Globals.baseURL + "/" + PathsTR + FullFileName;




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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithID> AddEditPoInvoice(AddEditPoInvoice Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long PoId = 0;
                    long poInvoiceId = 0;

                    if (Request.PurchasePOInvoice != null)
                    {
                        if (Request.PurchasePOInvoice.Id != null)
                        {
                            poInvoiceId = (long)Request.PurchasePOInvoice.Id;

                            var PurchasePOInvoiceDb = await _unitOfWork.PurchasePOInvoices.FindAsync(a => a.Id == Request.PurchasePOInvoice.Id);
                            if (PurchasePOInvoiceDb == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "This PO Invoice Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        if (Request.PurchasePOInvoice.POID != 0)
                        {
                            PoId = Request.PurchasePOInvoice.POID;

                            if (Request.PurchasePOInvoice.PurchasePOInvoiceTypeID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "PurchasePOInvoiceTypeID Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Please Enter POID";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Po Invoice Can't Be Null";
                        Response.Errors.Add(error);
                    }


                    List<long> purchasePOItemsDb =  _unitOfWork.PurchasePOItems.FindAll(a => a.PurchasePoid == PoId).Select(a => a.Id).ToList();

                    if (Request.PurchasePOItemList != null && Request.PurchasePOItemList.Count > 0)
                    {

                        List<long> PurchasePOItemReqIds = Request.PurchasePOItemList.Select(a => a.ID ?? 0).ToList();

                        if (!PurchasePOItemReqIds.All(purchasePOItemsDb.Contains))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "The PO Items List Is Not Compatible With Exist One In DB";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        if (purchasePOItemsDb != null && purchasePOItemsDb.Count > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "The PO Items List Can't be Null Or Empty";
                            Response.Errors.Add(error);
                        }
                    }

                    if (Request.PoInvoiceExtraFeesList != null && Request.PoInvoiceExtraFeesList.Count > 0)
                    {
                        foreach (var PoExtrFeesItm in Request.PoInvoiceExtraFeesList)
                        {
                            if (PoExtrFeesItm.Id != null)
                            {
                                var purchasePOExtraFeeDb = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAsync(a => a.Id == (PoExtrFeesItm.Id ?? 0));
                                if (purchasePOExtraFeeDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "The PO Extra Fees with Id : " + PoExtrFeesItm.Id + "Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (PoExtrFeesItm.POInvoiceExtraFeesTypeID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "POInvoiceExtraFeesTypeID Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    if (Request.PoInvoiceTaxIncludedList != null && Request.PoInvoiceTaxIncludedList.Count > 0)
                    {
                        foreach (var PoTxInc in Request.PoInvoiceTaxIncludedList)
                        {
                            if (PoTxInc.Id != null)
                            {
                                var purchasePOTaxIncludedDb = await _unitOfWork.PurchasePoinvoiceTaxIncluds.FindAsync(a => a.Id == (PoTxInc.Id ?? 0));
                                if (purchasePOTaxIncludedDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "The PO Tax Included with Id : " + PoTxInc.Id + "Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (PoTxInc.POTaxIncludedTypeID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "POTaxIncludedTypeID Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    if (Response.Result)
                    {
                        if (Request.PurchasePOInvoice.Id != null)
                        {
                            var PoInvoiceDb = await _unitOfWork.PurchasePOInvoices.FindAsync(a => a.Id == Request.PurchasePOInvoice.Id);
                            if (PoInvoiceDb != null)
                            {
                                PoInvoiceDb.ModificationDate = DateTime.Now;
                                PoInvoiceDb.ModifiedBy = UserID;
                                PoInvoiceDb.IsClosed = Request.PurchasePOInvoice.IsClosed;
                                if (!string.IsNullOrEmpty(Request.PurchasePOInvoice.InvoiceCollectionDueDate))
                                {
                                    try
                                    {

                                        PoInvoiceDb.InvoiceCollectionDueDate = DateTime.Parse(Request.PurchasePOInvoice.InvoiceCollectionDueDate);
                                    }
                                    catch (Exception)
                                    {
                                        PoInvoiceDb.InvoiceCollectionDueDate = null;
                                    }
                                }
                                if (!string.IsNullOrEmpty(Request.PurchasePOInvoice.InvoiceDate))
                                {
                                    try
                                    {
                                        PoInvoiceDb.InvoiceDate = DateTime.Parse(Request.PurchasePOInvoice.InvoiceDate);
                                    }
                                    catch (Exception)
                                    {
                                        PoInvoiceDb.InvoiceDate = null;
                                    }
                                }
                                PoInvoiceDb.IsFinalPriced = Request.PurchasePOInvoice.IsFinalPriced;
                                PoInvoiceDb.PurchasePoinvoiceTypeId = Request.PurchasePOInvoice.PurchasePOInvoiceTypeID;
                                PoInvoiceDb.TotalInvoiceCost = Request.PurchasePOInvoice.TotalInvoiceCost;
                                PoInvoiceDb.TotalInvoicePrice = Request.PurchasePOInvoice.TotalInvoicePrice;
                                if (Request.PurchasePOInvoice.TransactionId != null)
                                {
                                    PoInvoiceDb.TansactionId = Request.PurchasePOInvoice.TransactionId;
                                }
                                if (Request.PurchasePOInvoice.IsSentToACC != null)
                                {
                                    PoInvoiceDb.IsSentToAcc = Request.PurchasePOInvoice.IsSentToACC;
                                }

                            }
                        }
                        else
                        {
                            var PoInvoiceObj = new Infrastructure.Entities.PurchasePoinvoice()
                            {
                                CreationDate = DateTime.Now,
                                CreatedBy = UserID,
                                IsClosed = Request.PurchasePOInvoice.IsClosed,
                                InvoiceCollectionDueDate = DateTime.Parse(Request.PurchasePOInvoice.InvoiceCollectionDueDate),
                                InvoiceDate = DateTime.Parse(Request.PurchasePOInvoice.InvoiceDate),
                                IsFinalPriced = Request.PurchasePOInvoice.IsFinalPriced,
                                PurchasePoinvoiceTypeId = Request.PurchasePOInvoice.PurchasePOInvoiceTypeID,
                                TotalInvoiceCost = Request.PurchasePOInvoice.TotalInvoiceCost,
                                TotalInvoicePrice = Request.PurchasePOInvoice.TotalInvoicePrice,
                                Poid = PoId,
                                Active = true,
                                IsSentToAcc = Request.PurchasePOInvoice.IsSentToACC,
                                TansactionId = Request.PurchasePOInvoice.TransactionId
                            };

                            _unitOfWork.PurchasePOInvoices.Add(PoInvoiceObj);
                            _unitOfWork.Complete();
                            poInvoiceId = PoInvoiceObj.Id;
                        }



                        foreach (var PoItem in Request.PurchasePOItemList)
                        {
                            decimal? LastCalculate = 0;
                            decimal AverageCalculate = 0;
                            decimal MaxCalculate = 0;

                            var PoItemDb = await _unitOfWork.PurchasePOItems.FindAsync(a => a.Id == PoItem.ID);
                            if (PoItemDb != null)
                            {
                                PoItemDb.CurrencyId = PoItem.CurrencyID;
                                PoItemDb.RateToEgp = PoItem.RateToEgp;
                                PoItemDb.FinalUnitCost = LastCalculate = PoItem.FinalLocalUnitCostUOR;
                                PoItemDb.ActualUnitPrice = PoItem.ActualLocalUnitPrice;
                                PoItemDb.TotalActualPrice = PoItem.TotalLocalActualPrice;
                                PoItemDb.InvoiceComments = PoItem.InvoiceComments;
                                PoItemDb.ActualUnitPriceUnit = PoItem.ActualUnitPriceUnit;
                                PoItemDb.IsChecked = PoItem.IsChecked;
                                PoItemDb.SupplierInvoiceSerial = PoItem.SupplierInvoiceSerial;
                                PoItemDb.RecivedQuantity1 = PoItem.RecivedQuantity;
                                PoItemDb.ReqQuantity1 = PoItem.ReqQuantity;
                            }

                            AverageCalculate = (await _unitOfWork.PurchasePOItems.FindAllAsync(a => a.InventoryItemId == PoItemDb.InventoryItemId)).Average(a => (decimal)(a.FinalUnitCost ?? 0));
                            MaxCalculate = (await _unitOfWork.PurchasePOItems.FindAllAsync(a => a.InventoryItemId == PoItemDb.InventoryItemId)).Max(a => (decimal)(a.FinalUnitCost ?? 0));

                            var InventoryItemDb = await _unitOfWork.InventoryItems.FindAsync(a => a.Id == PoItemDb.InventoryItemId);
                            InventoryItemDb.AverageUnitPrice = AverageCalculate;
                            InventoryItemDb.MaxUnitPrice = MaxCalculate;
                            InventoryItemDb.LastUnitPrice = (decimal)LastCalculate;

                            var ParentInventoryStoreItemList = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => x.InventoryItemId == PoItemDb.InventoryItemId && x.AddingFromPoid == PoId);
                            if (ParentInventoryStoreItemList.Count() > 0)
                            {
                                long? POInvoiceId = _unitOfWork.PurchasePOInvoices.FindAsync(x => x.Poid == PoId).Id ;
                                foreach (var inventoryStoreItem in ParentInventoryStoreItemList)
                                {
                                    decimal? BalanceQTY = inventoryStoreItem.FinalBalance;
                                    //decimal? remainItemPrice = null; // Not Used Now
                                    int? currencyId = PoItemDb.CurrencyId;
                                    decimal? rateToEGP = PoItemDb.RateToEgp;
                                    decimal? POInvoiceTotalPriceEGP = PoItemDb.ActualUnitPrice;
                                    decimal? POInvoiceTotalCostEGP = PoItemDb.FinalUnitCost;
                                    decimal? remainItemCosetEGP = BalanceQTY * POInvoiceTotalCostEGP ?? 0;
                                    decimal? POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                                    decimal? POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                                    decimal? remainItemCostOtherCU = BalanceQTY * (POInvoiceTotalCost ?? 0);


                                    inventoryStoreItem.PoinvoiceId = POInvoiceId;
                                    inventoryStoreItem.CurrencyId = currencyId;
                                    inventoryStoreItem.RateToEgp = rateToEGP;
                                    inventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                                    inventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                                    inventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                                    inventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
                                    inventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
                                    inventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
                                }
                            }
                        }

                        if (Request.PoInvoiceExtraFeesList != null && Request.PoInvoiceExtraFeesList.Count > 0)
                        {
                            foreach (var extraFee in Request.PoInvoiceExtraFeesList)
                            {
                                if (extraFee.Id != null && extraFee.Id != 0)
                                {
                                    var ExtraFeeDb = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAsync(a => a.Id == extraFee.Id);
                                    if (ExtraFeeDb != null)
                                    {
                                        ExtraFeeDb.ModificationDate = DateTime.Now;
                                        ExtraFeeDb.ModifiedBy = UserID;
                                        ExtraFeeDb.Percentage = extraFee.Percentage;
                                        ExtraFeeDb.PoinvoiceExtraFeesTypeId = extraFee.POInvoiceExtraFeesTypeID;
                                        ExtraFeeDb.PoinvoiceId = poInvoiceId;
                                        ExtraFeeDb.PoitemId = extraFee.POItemId;
                                        ExtraFeeDb.RateToEgp = extraFee.RateToEGP;
                                        ExtraFeeDb.CurrencyId = extraFee.CurrencyID;
                                        ExtraFeeDb.Amount = extraFee.Amount;
                                        ExtraFeeDb.Comment = extraFee.Comment;
                                    }
                                }
                                else
                                {
                                    var ExtraFeesObj = new Infrastructure.Entities.PurchasePoinvoiceExtraFee()
                                    {
                                        Amount = extraFee.Amount,
                                        Active = true,
                                        Comment = extraFee.Comment,
                                        CreatedBy = UserID,
                                        CreationDate = DateTime.Now,
                                        CurrencyId = extraFee.CurrencyID,
                                        Percentage = extraFee.Percentage,
                                        PoinvoiceExtraFeesTypeId = extraFee.POInvoiceExtraFeesTypeID,
                                        PoitemId = extraFee.POItemId,
                                        PoinvoiceId = poInvoiceId,
                                        RateToEgp = extraFee.RateToEGP
                                    };
                                    _unitOfWork.PurchasePoinvoiceExtraFees.Add(ExtraFeesObj);
                                }
                            }
                        }

                        if (Request.PoInvoiceTaxIncludedList != null && Request.PoInvoiceTaxIncludedList.Count > 0)
                        {
                            foreach (var taxInc in Request.PoInvoiceTaxIncludedList)
                            {
                                if (taxInc.Id != null && taxInc.Id != 0)
                                {
                                    var TaxIncludedDb = await _unitOfWork.PurchasePoinvoiceTaxIncluds.FindAsync(a => a.Id == taxInc.Id);
                                    if (TaxIncludedDb != null)
                                    {
                                        TaxIncludedDb.ModificationDate = DateTime.Now;
                                        TaxIncludedDb.ModifiedBy = UserID;
                                        TaxIncludedDb.Percentage = taxInc.Percentage;
                                        TaxIncludedDb.PotaxIncludedTypeId = taxInc.POTaxIncludedTypeID;
                                        TaxIncludedDb.PoinvoiceId = poInvoiceId;
                                        TaxIncludedDb.RateToEgp = taxInc.RateToEGP;
                                        TaxIncludedDb.CurrencyId = taxInc.CurrencyID;
                                        TaxIncludedDb.Amount = taxInc.Amount;
                                    }
                                }
                                else
                                {
                                    var TaxIncludedObj = new Infrastructure.Entities.PurchasePoinvoiceTaxIncluded()
                                    {
                                        Amount = taxInc.Amount,
                                        Active = true,
                                        CreatedBy = UserID,
                                        CreationDate = DateTime.Now,
                                        CurrencyId = taxInc.CurrencyID,
                                        Percentage = taxInc.Percentage,
                                        PoinvoiceId = poInvoiceId,
                                        RateToEgp = taxInc.RateToEGP,
                                        PotaxIncludedTypeId = taxInc.POTaxIncludedTypeID
                                    };
                                    _unitOfWork.PurchasePoinvoiceTaxIncluds.Add(TaxIncludedObj);
                                }
                            }
                        }

                       _unitOfWork.Complete();

                    }
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

        public async Task<BaseResponseWithID> AddDirectPoInvoice(AddDirectPoInvoiceRequest Request, long UserID)
        {

            BaseResponseWithID Response = new BaseResponseWithID();
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long poInvoiceId = 0;

                    if (Request.PurchasePOInvoice == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Po Invoice Can't Be Null";
                        Response.Errors.Add(error);
                    }




                    if (Request.PoInvoiceExtraFeesList != null && Request.PoInvoiceExtraFeesList.Count > 0)
                    {
                        foreach (var PoExtrFeesItm in Request.PoInvoiceExtraFeesList)
                        {
                            if (PoExtrFeesItm.Id != null)
                            {
                                var purchasePOExtraFeeDb = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAsync(a => a.Id == (PoExtrFeesItm.Id ?? 0));
                                if (purchasePOExtraFeeDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "The PO Extra Fees with Id : " + PoExtrFeesItm.Id + "Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (PoExtrFeesItm.POInvoiceExtraFeesTypeID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "POInvoiceExtraFeesTypeID Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    if (Request.PoInvoiceTaxIncludedList != null && Request.PoInvoiceTaxIncludedList.Count > 0)
                    {
                        foreach (var PoTxInc in Request.PoInvoiceTaxIncludedList)
                        {
                            if (PoTxInc.Id != null)
                            {
                                var purchasePOTaxIncludedDb = await _unitOfWork.PurchasePoinvoiceTaxIncluds.FindAsync(a => a.Id == (PoTxInc.Id ?? 0));
                                if (purchasePOTaxIncludedDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "The PO Tax Included with Id : " + PoTxInc.Id + "Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (PoTxInc.POTaxIncludedTypeID == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "POTaxIncludedTypeID Is Mandatory";
                                Response.Errors.Add(error);
                            }
                        }
                    }

                    if (Response.Result)
                    {

                        //AddMatrialDirectPRRequest.
                        var AddMatrialDirectPRRequest = new AddMatrialDirectPRRequest();
                        AddMatrialDirectPRRequest.RequestgDate = DateTime.Now.ToString();
                        AddMatrialDirectPRRequest.DirectPRItemList = new List<DirectPRItem>();
                        AddMatrialDirectPRRequest.DirectPRItemList = Request.PurchasePOItemList.Select(item => new DirectPRItem
                        {
                            InventoryItemID = item.InventoryItemID,
                            ReqQTY = item.ReqQuantity ?? 0,
                            Comment = "From Direct PO Invoice",
                            DirectPrNotes = "From Direct PO Invoice"
                        }).ToList();
                        var AddMatrialDirectPRResponse = _purchesRequestService.AddMatrialDirectPR(AddMatrialDirectPRRequest, UserID);
                        long PurchaseRequestId = 0;
                        if (AddMatrialDirectPRResponse.Result)
                        {
                            PurchaseRequestId = AddMatrialDirectPRResponse.ID;
                        }
                        else
                        {
                            Response = AddMatrialDirectPRResponse;
                            return Response;
                        }


                        // Add PO and PO Items 
                        // Insert Purchase PO 
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

                            var SupplierInsertion = new Supplier();
                            SupplierInsertion.Name = SupplierName;
                            SupplierInsertion.Type = "Supplier";
                            SupplierInsertion.CreatedBy = UserID;
                            SupplierInsertion.CreationDate = DateTime.Now;
                            SupplierInsertion.Active = true;

                            _unitOfWork.Suppliers.Add(SupplierInsertion);
                            _unitOfWork.Complete();

                            if (SupplierInsertion.Id > 0)
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
                        var PurchasePODB = new Infrastructure.Entities.PurchasePo();
                        PurchasePODB.ToSupplierId = SupplierID;
                        PurchasePODB.RequestDate = DateTime.Now;
                        PurchasePODB.CreationDate = DateTime.Now;
                        PurchasePODB.CreatedBy = UserID;
                        PurchasePODB.ModifiedDate = DateTime.Now;
                        PurchasePODB.ModifiedBy = UserID;
                        PurchasePODB.Active = true;
                        PurchasePODB.Status = "Open";
                        PurchasePODB.PotypeId = 1;
                        PurchasePODB.AccountantReplyNotes = "Not Assigned To Accountant";
                        PurchasePODB.AssignedPurchasingPersonId = UserID;

                        _unitOfWork.PurchasePOes.Add(PurchasePODB);
                        var Res = _unitOfWork.Complete();

                        long POID = PurchasePODB.Id;


                        #region items


                        if (POID != 0 && Res > 0)
                        {
                            var PurchaseRequestItemListDB = _unitOfWork.VPurchaseRequestItems.FindAll(x => x.PurchaseRequestId == PurchaseRequestId).ToList();
                            foreach (var PoItem in Request.PurchasePOItemList)
                            {
                                decimal? LastCalculate = 0;
                                decimal AverageCalculate = 0;
                                decimal MaxCalculate = 0;

                                var PurchaseRequestItem = PurchaseRequestItemListDB.Where(a => a.InventoryItemId == PoItem.InventoryItemID).FirstOrDefault();
                                if (PurchaseRequestItem != null)
                                {
                                    var PoItemDb = new Infrastructure.Entities.PurchasePoitem();
                                    PoItemDb.CurrencyId = PoItem.CurrencyID;
                                    PoItemDb.RateToEgp = PoItem.RateToEgp;
                                    PoItemDb.FinalUnitCost = LastCalculate = PoItem.FinalLocalUnitCostUOR;
                                    PoItemDb.ActualUnitPrice = PoItem.ActualLocalUnitPrice;
                                    PoItemDb.TotalActualPrice = PoItem.TotalLocalActualPrice;
                                    PoItemDb.InvoiceComments = PoItem.InvoiceComments;
                                    PoItemDb.ActualUnitPriceUnit = PoItem.ActualUnitPriceUnit;
                                    PoItemDb.IsChecked = PoItem.IsChecked;
                                    PoItemDb.SupplierInvoiceSerial = PoItem.SupplierInvoiceSerial;
                                    PoItemDb.RecivedQuantity1 = PoItem.RecivedQuantity;
                                    PoItemDb.ReqQuantity1 = PoItem.ReqQuantity;

                                    PoItemDb.InventoryMatrialRequestItemId = PurchaseRequestItem.InventoryMatrialRequestItemId;
                                    PoItemDb.InventoryItemId = (long)PurchaseRequestItem.InventoryItemId;
                                    PoItemDb.Uomid = (int)PurchaseRequestItem.Uomid;
                                    PoItemDb.ProjectId = PurchaseRequestItem.ProjectId;
                                    PoItemDb.FabricationOrderId = PurchaseRequestItem.FabricationOrderId;
                                    PoItemDb.Comments = PurchaseRequestItem.Comments;
                                    PoItemDb.PurchasePoid = POID;
                                    PoItemDb.PurchaseRequestItemId = PurchaseRequestItem.Id;
                                    PoItemDb.RecivedQuantity1 = 0;

                                    _unitOfWork.PurchasePOItems.Add(PoItemDb);

                                    AverageCalculate = (await _unitOfWork.PurchasePOItems.FindAllAsync(a => a.InventoryItemId == PoItemDb.InventoryItemId)).Average(a => (decimal)(a.FinalUnitCost ?? 0));
                                    MaxCalculate = (await _unitOfWork.PurchasePOItems.FindAllAsync(a => a.InventoryItemId == PoItemDb.InventoryItemId)).Max(a => (decimal)(a.FinalUnitCost ?? 0));

                                    var InventoryItemDb = await _unitOfWork.InventoryItems.FindAsync(a => a.Id == PoItemDb.InventoryItemId);
                                    InventoryItemDb.AverageUnitPrice = AverageCalculate;
                                    InventoryItemDb.MaxUnitPrice = MaxCalculate;
                                    InventoryItemDb.LastUnitPrice = (decimal)LastCalculate;


                                    var PurchaseRequestItmesListDB = await _unitOfWork.PurchaseRequestItems.FindAllAsync(x => x.PurchaseRequestId == PurchaseRequestId);
                                    #region Update PR Status Closed 
                                    var PRItemCountDB = PurchaseRequestItmesListDB.Count();
                                    var PRItemCountPurchasedDB = PurchaseRequestItmesListDB.Where(x => x.PurchasedQuantity >= x.Quantity).Count();
                                    // Check if all Qty is Purchased 
                                    if (PRItemCountPurchasedDB >= PRItemCountDB)
                                    {
                                        var PRObjDB = await _unitOfWork.PurchaseRequests.FindAsync(x => x.Id == PurchaseRequestId);
                                        PRObjDB.Status = "Closed";
                                    }
                                    #endregion
                                    _unitOfWork.Complete();

                                }

                            }

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err1011";
                            error.ErrorMSG = "Purchase Order Not Created , Please try again";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        #endregion items




                        var PoInvoiceObj = new Infrastructure.Entities.PurchasePoinvoice()
                        {
                            CreationDate = DateTime.Now,
                            CreatedBy = UserID,
                            IsClosed = Request.PurchasePOInvoice.IsClosed,
                            InvoiceCollectionDueDate = DateTime.Parse(Request.PurchasePOInvoice.InvoiceCollectionDueDate),
                            InvoiceDate = DateTime.Parse(Request.PurchasePOInvoice.InvoiceDate),
                            IsFinalPriced = Request.PurchasePOInvoice.IsFinalPriced,
                            PurchasePoinvoiceTypeId = Request.PurchasePOInvoice.PurchasePOInvoiceTypeID,
                            TotalInvoiceCost = Request.PurchasePOInvoice.TotalInvoiceCost,
                            TotalInvoicePrice = Request.PurchasePOInvoice.TotalInvoicePrice,
                            Poid = POID,
                            Active = true,
                            IsSentToAcc = Request.PurchasePOInvoice.IsSentToACC,
                            TansactionId = Request.PurchasePOInvoice.TransactionId
                        };

                        _unitOfWork.PurchasePOInvoices.Add(PoInvoiceObj);
                        _Context.SaveChanges();
                        poInvoiceId = PoInvoiceObj.Id;

                        Response.ID = poInvoiceId;




                        if (Request.PoInvoiceExtraFeesList != null && Request.PoInvoiceExtraFeesList.Count > 0)
                        {
                            foreach (var extraFee in Request.PoInvoiceExtraFeesList)
                            {
                                if (extraFee.Id != null && extraFee.Id != 0)
                                {
                                    var ExtraFeeDb = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAsync(a => a.Id == extraFee.Id);
                                    if (ExtraFeeDb != null)
                                    {
                                        ExtraFeeDb.ModificationDate = DateTime.Now;
                                        ExtraFeeDb.ModifiedBy = UserID;
                                        ExtraFeeDb.Percentage = extraFee.Percentage;
                                        ExtraFeeDb.PoinvoiceExtraFeesTypeId = extraFee.POInvoiceExtraFeesTypeID;
                                        ExtraFeeDb.PoinvoiceId = poInvoiceId;
                                        ExtraFeeDb.PoitemId = extraFee.POItemId;
                                        ExtraFeeDb.RateToEgp = extraFee.RateToEGP;
                                        ExtraFeeDb.CurrencyId = extraFee.CurrencyID;
                                        ExtraFeeDb.Amount = extraFee.Amount;
                                        ExtraFeeDb.Comment = extraFee.Comment;
                                    }
                                }
                                else
                                {
                                    var ExtraFeesObj = new Infrastructure.Entities.PurchasePoinvoiceExtraFee()
                                    {
                                        Amount = extraFee.Amount,
                                        Active = true,
                                        Comment = extraFee.Comment,
                                        CreatedBy = UserID,
                                        CreationDate = DateTime.Now,
                                        CurrencyId = extraFee.CurrencyID,
                                        Percentage = extraFee.Percentage,
                                        PoinvoiceExtraFeesTypeId = extraFee.POInvoiceExtraFeesTypeID,
                                        PoitemId = extraFee.POItemId,
                                        PoinvoiceId = poInvoiceId,
                                        RateToEgp = extraFee.RateToEGP
                                    };
                                    _unitOfWork.PurchasePoinvoiceExtraFees.Add(ExtraFeesObj);
                                }
                            }
                        }

                        if (Request.PoInvoiceTaxIncludedList != null && Request.PoInvoiceTaxIncludedList.Count > 0)
                        {
                            foreach (var taxInc in Request.PoInvoiceTaxIncludedList)
                            {
                                if (taxInc.Id != null && taxInc.Id != 0)
                                {
                                    var TaxIncludedDb = await _unitOfWork.PurchasePoinvoiceTaxIncluds.FindAsync(a => a.Id == taxInc.Id);
                                    if (TaxIncludedDb != null)
                                    {
                                        TaxIncludedDb.ModificationDate = DateTime.Now;
                                        TaxIncludedDb.ModifiedBy = UserID;
                                        TaxIncludedDb.Percentage = taxInc.Percentage;
                                        TaxIncludedDb.PotaxIncludedTypeId = taxInc.POTaxIncludedTypeID;
                                        TaxIncludedDb.PoinvoiceId = poInvoiceId;
                                        TaxIncludedDb.RateToEgp = taxInc.RateToEGP;
                                        TaxIncludedDb.CurrencyId = taxInc.CurrencyID;
                                        TaxIncludedDb.Amount = taxInc.Amount;
                                    }
                                }
                                else
                                {
                                    var TaxIncludedObj = new Infrastructure.Entities.PurchasePoinvoiceTaxIncluded()
                                    {
                                        Amount = taxInc.Amount,
                                        Active = true,
                                        CreatedBy = UserID,
                                        CreationDate = DateTime.Now,
                                        CurrencyId = taxInc.CurrencyID,
                                        Percentage = taxInc.Percentage,
                                        PoinvoiceId = poInvoiceId,
                                        RateToEgp = taxInc.RateToEGP,
                                        PotaxIncludedTypeId = taxInc.POTaxIncludedTypeID
                                    };
                                    _unitOfWork.PurchasePoinvoiceTaxIncluds.Add(TaxIncludedObj);
                                }
                            }
                        }

                        _unitOfWork.Complete();

                    }
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

        public async Task<BaseResponseWithID> AddNewPurchasePOExtraaFees(AddNewPurchasePOInvoiceExtraFeesRequest Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                    if (Request.PurchasePOExtraFeesList != null && Request.PurchasePOExtraFeesList.Count > 0)
                    {
                        foreach (var po in Request.PurchasePOExtraFeesList)
                        {
                            long? POId = 0;
                            long? POInvoiceId = 0;
                            string CreatedByString = null;
                            long? CreatedBy = 0;
                            if (po.POInvoiceId != 0 && po.POInvoiceId != null)
                            {
                                POInvoiceId = po.POInvoiceId;

                                var POInvoiceDb = await _unitOfWork.PurchasePOInvoices.FindAsync(a => a.Id == POInvoiceId);
                                if (POInvoiceDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "This PoInvoice Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                if (po.POId != 0 && po.POId != null)
                                {
                                    POId = po.POId;

                                    var PODb = await _unitOfWork.PurchasePOes.FindAsync(a => a.Id == POId);
                                    if (PODb == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "This PO Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }

                                    if (po.CreatedBy != null)
                                    {
                                        CreatedByString = po.CreatedBy;
                                        CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invoice Created By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "You Must Enter PoId To Create New PO Invoice";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }

                            if (po.PurchasePOExtraFeesList != null)
                            {
                                if (po.PurchasePOExtraFeesList.Count > 0)
                                {
                                    var counter = 0;
                                    var LocalCurrency = _unitOfWork.Currencies.Find(a => a.IsLocal == true).Id;

                                    foreach (var extraFee in po.PurchasePOExtraFeesList)
                                    {
                                        counter++;

                                        string ExtraFeeCreatedByString = null;
                                        string ModifiedByString = null;
                                        long? ExtraFeeCreatedBy = 0;
                                        long? ModifiedBy = 0;
                                        if (extraFee.Id != 0 && extraFee.Id != null)
                                        {
                                            if (extraFee.ModifiedBy != null)
                                            {
                                                ModifiedByString = extraFee.ModifiedBy;
                                                ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Line: " + counter + ", " + "Message: Modified By Id Is Mandatory";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            if (extraFee.CreatedBy != null)
                                            {
                                                ExtraFeeCreatedByString = extraFee.CreatedBy;
                                                ExtraFeeCreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(ExtraFeeCreatedByString, key));
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Line: " + counter + ", " + "Created By Id Is Mandatory";
                                                Response.Errors.Add(error);
                                            }
                                        }

                                        if (extraFee.ExtraFeesTypeId != 0 && extraFee.ExtraFeesTypeId != null)
                                        {
                                            var ExtraFeesTypeDb = await _unitOfWork.PurchasePoinvoiceExtraFeesTypes.FindAsync(a => a.Id == extraFee.ExtraFeesTypeId);
                                            if (ExtraFeesTypeDb == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "Line: " + counter + ", " + "ExtraFeesType Doesn't Exist";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            extraFee.ExtraFeesTypeId = 4;
                                        }

                                        if (extraFee.CurrencyId != 0 && extraFee.CurrencyId != null)
                                        {
                                            var CurrencyDb = await _unitOfWork.Currencies.FindAsync(a => a.Id == extraFee.CurrencyId);
                                            if (CurrencyDb == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "Line: " + counter + ", " + "Currency Doesn't Exist";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            extraFee.CurrencyId = LocalCurrency;
                                            extraFee.RateToEgp = 1;
                                        }
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "ExtraFeesList Is Empty, Please Insert At Least One Item";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "ExtraFeesList Is Null, It Cannot Be Null";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "ErrCRM1";
                        error.ErrorMSG = "The List is Empty!!";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        foreach (var po in Request.PurchasePOExtraFeesList)
                        {
                            if (po.POInvoiceId == null || po.POInvoiceId == 0)
                            {
                                var POInvoiceDb = _unitOfWork.PurchasePOInvoices.Find(a => a.Poid == (long)po.POId);
                                if (POInvoiceDb != null)
                                {
                                    po.POInvoiceId = POInvoiceDb.Id;
                                }
                                else
                                {
                                    var InvoiceTypeId =  _unitOfWork.PurchasePoinvoiceTypes.Find(a => a.Name.ToLower() == "invoice").Id;
                                    if (InvoiceTypeId != 0)
                                    {
                                        var InvoiceObj = new Infrastructure.Entities.PurchasePoinvoice()
                                        {
                                            Active = true,
                                            Poid = (long)po.POId,
                                            CreatedBy = UserID,
                                            CreationDate = DateTime.Now,
                                            InvoiceDate = DateTime.Now,
                                            IsClosed = false,
                                            PurchasePoinvoiceTypeId = InvoiceTypeId
                                        };
                                        _unitOfWork.PurchasePOInvoices.Add(InvoiceObj);
                                        var IsPoInvoiceSaved = await _Context.SaveChangesAsync();
                                        if (IsPoInvoiceSaved > 0)
                                        {
                                            po.POInvoiceId =  _unitOfWork.PurchasePOInvoices.Find(a => a.Poid == po.POId).Id;
                                        }
                                    }
                                }
                            }
                            Response.ID = (long)po.POInvoiceId;

                            foreach (var extraFee in po.PurchasePOExtraFeesList)
                            {
                                if (extraFee.Id != null && extraFee.Id != 0)
                                {
                                    var ExtraFeeDb = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAsync(a => a.Id == extraFee.Id);
                                    if (ExtraFeeDb != null)
                                    {
                                        ExtraFeeDb.ModificationDate = DateTime.Now;
                                        ExtraFeeDb.ModifiedBy = UserID;
                                        ExtraFeeDb.Percentage = extraFee.Percentage;
                                        ExtraFeeDb.PoinvoiceExtraFeesTypeId = (long)extraFee.ExtraFeesTypeId;
                                        ExtraFeeDb.PoinvoiceId = (long)po.POInvoiceId;
                                        ExtraFeeDb.PoitemId = extraFee.POItemId;
                                        ExtraFeeDb.RateToEgp = extraFee.RateToEgp;
                                        ExtraFeeDb.CurrencyId = extraFee.CurrencyId;
                                        ExtraFeeDb.Amount = extraFee.Amount;
                                        ExtraFeeDb.Comment = extraFee.Comment;
                                        _unitOfWork.Complete();
                                    }
                                }
                                else
                                {
                                    var ExtraFeesObj = new Infrastructure.Entities.PurchasePoinvoiceExtraFee()
                                    {
                                        Amount = extraFee.Amount,
                                        Active = true,
                                        Comment = extraFee.Comment,
                                        CreatedBy = UserID,
                                        CreationDate = DateTime.Now,
                                        CurrencyId = extraFee.CurrencyId,
                                        Percentage = extraFee.Percentage,
                                        PoinvoiceExtraFeesTypeId = (long)extraFee.ExtraFeesTypeId,
                                        PoitemId = extraFee.POItemId,
                                        PoinvoiceId = (long)po.POInvoiceId,
                                        RateToEgp = extraFee.RateToEgp
                                    };
                                    _unitOfWork.PurchasePoinvoiceExtraFees.Add(ExtraFeesObj);
                                    _unitOfWork.Complete();
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public async Task<BaseMessageResponse> GetInvoiceDetailsPDF([FromHeader] long SalesOfferId)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SalesOfferDetailsVMObject = new SalesOfferDetailsVM();

                var SalesOfferProductsVMList = new List<SalesOfferProductsVM>();

                decimal? TotalTaxAmount = 0;
                decimal? TotalNetPrice = 0;
                decimal? TotalFinalUnitPrice = 0;

                //GetPurchasePOITemList.Add(ForSumObj);


                if (Response.Result)
                {
                    if(SalesOfferId==0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid SalesOfferId";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var SalesOffer = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId, includes: new[] { "Invoices", "SalesOfferProducts.InventoryItem" }).FirstOrDefault();
                    if (SalesOffer == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "This Sales Offer ID doesn't exist !";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var SalesOfferProductsList = SalesOffer.SalesOfferProducts;
                    var InvoiceDB = SalesOffer.Invoices.FirstOrDefault();
                    if (InvoiceDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "This Sales Offer ID not have any invoice !";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var ClientData = _unitOfWork.Clients.FindAll(x => x.Id == InvoiceDB.ClientId, includes: new[] { "ClientContactPeople", "ClientAddresses" }).FirstOrDefault();
                    var MainClientData = _unitOfWork.Clients.FindAll(x => x.OwnerCoProfile == true).FirstOrDefault();
                    string MainCompanyAddress = "-";
                    string TaxCard = "-";
                    string FromCompanyName = "-";
                    if (MainClientData != null)
                    {
                        var MainClientAdress = _unitOfWork.ClientAddresses.FindAll(x => x.ClientId == MainClientData.Id).FirstOrDefault();
                        var Governerate = _unitOfWork.Governorates.GetById(MainClientAdress.GovernorateId);
                        FromCompanyName = MainClientData.Name;
                        TaxCard = MainClientData.TaxCard;
                        MainCompanyAddress = MainClientAdress.Address + " " + "Building No:" + " " + MainClientAdress.BuildingNumber + " " + Governerate?.Name??"" + " " + "," + "" + MainClientAdress.Country.Name;

                    }

                    SalesOfferDetailsVMObject.InvoiceTo = ClientData.Name; // Common.GetClientName((long)InvoiceDB.ClientID);
                    SalesOfferDetailsVMObject.InvoiceClientPhoneNo = ClientData.ClientContactPeople?.FirstOrDefault()?.Mobile; // Common.GetClientName((long)InvoiceDB.ClientID);
                    SalesOfferDetailsVMObject.InvoiceSerial = InvoiceDB.Serial;
                    SalesOfferDetailsVMObject.InvoiceDate = InvoiceDB.InvoiceDate.ToString();
                    SalesOfferDetailsVMObject.Address = ClientData.ClientAddresses.FirstOrDefault()?.Address;
                    if (ClientData.ClientPaymentTerms != null && ClientData.ClientPaymentTerms.Count > 0)
                    {
                        SalesOfferDetailsVMObject.TermsOfPayment = ClientData.ClientPaymentTerms?.FirstOrDefault().Percentage + " " + ClientData.ClientPaymentTerms?.FirstOrDefault().Name;
                    }
                    SalesOfferDetailsVMObject.FromCompanyName = FromCompanyName;
                    SalesOfferDetailsVMObject.TaxCard = TaxCard;
                    SalesOfferDetailsVMObject.MainCompanyAddress = MainCompanyAddress;
                    //MainClientAdress.Address + " " + "Building No:" + " " + MainClientAdress.BuildingNumber + " " + Common.GetGovernorateName(MainClientAdress.GovernorateID) + " " + "," + "" + MainClientAdress.Country.Name;


                    string CarModel = "";
                    string PlateNo = "";
                    string ChassisNumber = "";
                    string MaintenanceName = "";
                    string CarKiloMeter = "";
                    var VehiclePerClient = _unitOfWork.VehiclePerClients.FindAll(x => x.ClientId == SalesOffer.ClientId, includes: new[] {"Model"}).FirstOrDefault();
                    if (VehiclePerClient != null)
                    {
                        CarModel = VehiclePerClient.Model?.Name;
                        PlateNo = VehiclePerClient.PlatNumber;
                        ChassisNumber = VehiclePerClient.ChassisNumber;
                        var ProjectId = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOffer.Id).Select(x => x.Id).FirstOrDefault();
                        var VehicleMaintenanceJobOrderHistory = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(x => x.VehiclePerClientId == VehiclePerClient.Id && x.JobOrderProjectId == ProjectId).FirstOrDefault();
                        if (VehicleMaintenanceJobOrderHistory != null)
                        {
                            MaintenanceName = VehicleMaintenanceJobOrderHistory.VehicleMaintenanceType?.Name;
                            CarKiloMeter = VehicleMaintenanceJobOrderHistory.Milage?.ToString();
                        }

                    }

                    if (SalesOfferProductsList != null && SalesOfferProductsList.Count != 0)
                    {

                        foreach (var item in SalesOfferProductsList)
                        {
                            var SalesOfferProductsDB = new SalesOfferProductsVM();

                            SalesOfferProductsDB.ID = item.Id;
                            SalesOfferProductsDB.InventoryItems = item.InventoryItem.Name;
                            SalesOfferProductsDB.NetPrice = decimal.Parse(item.Quantity.ToString()) * item.ItemPrice;
                            SalesOfferProductsDB.Price = item.ItemPrice;
                            SalesOfferProductsDB.QTY = item.Quantity;
                            SalesOfferProductsDB.TaxAmount = item.SalesOfferProductTaxes.Sum(x => x.Value);
                            SalesOfferProductsDB.FinalUnitPrice = SalesOfferProductsDB.NetPrice + SalesOfferProductsDB.TaxAmount;
                            SalesOfferProductsDB.Description = item.ItemPricingComment;
                            TotalTaxAmount += item.SalesOfferProductTaxes.Sum(x => x.Value);
                            TotalNetPrice += SalesOfferProductsDB.NetPrice;
                            TotalFinalUnitPrice += SalesOfferProductsDB.FinalUnitPrice;


                            SalesOfferProductsVMList.Add(SalesOfferProductsDB);
                        }
                    }




                    //Start PDF Service


                    MemoryStream ms = new MemoryStream();

                    //Size of page
                    Document document = new Document(PageSize.LETTER);


                    PdfWriter pw = PdfWriter.GetInstance(document, ms);

                    //Call the footer Function

                    pw.PageEvent = new HeaderFooter2();

                    document.Open();

                    //Handle fonts and Sizes +  Attachments images logos 

                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.LETTER);

                    //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                    //document.SetMargins(0, 0, 20, 20);
                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    Font font = new Font(bf, 11, Font.NORMAL);


                    //}



                    if (MainClientData!=null&& MainClientData.HasLogo == true && MainClientData.LogoUrl != null)
                    {

                        var clientLogo = Globals.baseURL + "/"+ MainClientData.LogoUrl;
                        ////string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                        //Image logo = Image.GetInstance(clientLogo);
                        //logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                        //logo.ScaleAbsolute(3000f, 3000f);
                        //logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(clientLogo);
                        if (File.Exists(_host.WebRootPath+ MainClientData.LogoUrl))
                        {
                            jpg.ScaleAbsolute(210, 100);
                            //document.Add(logo);
                            document.Add(jpg);
                        }
                        //iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        //Paragraph prgHeading = new Paragraph();
                        //prgHeading.Alignment = Element.ALIGN_RIGHT;
                        ////prgHeading.SpacingBefore = -10;
                        //Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);
                        //cc.SetBackground(new BaseColor(4, 189, 189), 220,0, 0, 80);




                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 30, 1, iTextSharp.text.BaseColor.WHITE);
                        Paragraph prgHeading = new Paragraph();
                        prgHeading.Alignment = Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -30;
                        prgHeading.SpacingAfter = 20;


                        Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);
                        cc.SetBackground(new BaseColor(4, 189, 189), 140, 12, 0, 25);
                        prgHeading.Add(cc);

                        document.Add(prgHeading);



                        //Adding paragraph for report generated by  
                        Paragraph prgGeneratedBY = new Paragraph();
                        BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                        prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                        document.Add(prgGeneratedBY);


                    }



                    //Adding a line  
                    //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                    //document.Add(p);
                    //String path = HttpContext.Current.Server.MapPath("/Attachments");

                    //if (headers["CompanyName"].ToString() == "marinaplt")
                    //{
                    //    var PurchasePOInvoiceDB2 = new PurchasePOInvoiceVM();



                    //    string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                    //    //Image logo = Image.GetInstance(PDFp_strPath);
                    //    //logo.SetAbsolutePosition(80f, 50f);
                    //    //logo.ScaleAbsolute(600f,600f);



                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                    //    jpg.SetAbsolutePosition(60f, 750f);
                    //    //document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -20;



                    //    Chunk cc = new Chunk("I Invoice" + " ", fntHead);

                    //    cc.SetBackground(new BaseColor(4, 189, 189), 220, 9, 15, 40);


                    //    prgHeading.Add(cc);

                    //    document.Add(prgHeading);

                    //}
                    //else if (headers["CompanyName"].ToString() == "piaroma")
                    //{

                    //    string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    //    Image logo = Image.GetInstance(Piaroma_p_strPath);
                    //    logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                    //    logo.ScaleAbsolute(300f, 300f);

                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                    //    document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 30, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -30;
                    //    prgHeading.SpacingAfter = 20;


                    //    Chunk cc = new Chunk("I Invoice" + " ", fntHead);
                    //    cc.SetBackground(new BaseColor(4, 189, 189), 220, 9, 0, 25);


                    //    prgHeading.Add(cc);

                    //    document.Add(prgHeading);


                    //}
                    //else if (headers["CompanyName"].ToString() == "Garastest")
                    //{
                    //    string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    //    Image logo = Image.GetInstance(GarasTest_p_strPath);
                    //    logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                    //    logo.ScaleAbsolute(300f, 300f);


                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(GarasTest_p_strPath);
                    //    document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -10;



                    //    Chunk cc = new Chunk("I Invoice" + " ", fntHead);
                    //    cc.SetBackground(new BaseColor(4, 189, 189), 210, 9, 5, 40);


                    //    prgHeading.Add(cc);


                    //    //prgHeading.Add(new Chunk("Inventory Store Item Report".ToUpper(), fntHead));

                    //    document.Add(prgHeading);
                    //}




                    ////Adding paragraph for report generated by  
                    //Paragraph prgGeneratedBY = new Paragraph();
                    //BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    //iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                    //prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                    //document.Add(prgGeneratedBY);



                    //Adding a line  
                    //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                    //document.Add(p);






                    var dt2 = new System.Data.DataTable("grid");


                    dt2.Columns.AddRange(new DataColumn[8] { new DataColumn("No	"),
                                                     new DataColumn("Inventory Store Items"),
                                                     new DataColumn("QTY"),
                                                     new DataColumn("Price") ,
                                                      new DataColumn("Net Price"),
                                                     new DataColumn("Tax Amount"),
                                                     new DataColumn("Description"),
                                                     new DataColumn("Final Unit Price") ,

                    });
                    var Counter2 = 1;
                    var SalesOfferProductsListVM = SalesOfferProductsVMList;
                    if (SalesOfferProductsListVM != null)
                    {
                        foreach (var item in SalesOfferProductsListVM)
                        {

                            //string CounterString = Counter == 0 ? "" : Counter.ToString();
                            dt2.Rows.Add(
                                Counter2.ToString(),
                                item.InventoryItems != null ? item.InventoryItems : "-",
                                item.QTY != null ? item.QTY : 0,
                                item.Price != null ? Math.Abs(Decimal.Round((decimal)item.Price, 2)) : 0,
                                item.NetPrice != null ? Math.Abs(Decimal.Round((decimal)item.NetPrice, 2)) : 0,
                                item.TaxAmount != null ? Math.Abs(Decimal.Round((decimal)item.TaxAmount, 2)) : 0,
                                item.Description != null ? item.Description : "-",
                                item.FinalUnitPrice != null ? Math.Abs(Decimal.Round((decimal)item.FinalUnitPrice, 2)) : 0



                                );
                            Counter2++;
                        }
                    }










                    PdfPTable tableHeading = new PdfPTable(4);

                    tableHeading.WidthPercentage = 100;

                    tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                    PdfPCell cellPurchasePOInvoiceTypeIDName = new PdfPCell();
                    string cellPurchasePOInvoiceTypeIDNametext = "I" + "Invoice";
                    cellPurchasePOInvoiceTypeIDName.Phrase = new Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceTypeIDName.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellPurchasePOInvoiceTypeIDName.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceTypeIDName.BorderColor = new BaseColor(4, 189, 189);
                    cellPurchasePOInvoiceTypeIDName.PaddingBottom = 15;
                    cellPurchasePOInvoiceTypeIDName.PaddingTop = 15;
                    cellPurchasePOInvoiceTypeIDName.BackgroundColor = new BaseColor(4, 189, 189);

                    PdfPCell cellPurchasePOIDNumber = new PdfPCell();
                    string cellPurchasePOIDNumbertext = " Invoice Serial" + " " + SalesOfferDetailsVMObject.InvoiceSerial;
                    cellPurchasePOIDNumber.Phrase = new Phrase(cellPurchasePOIDNumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOIDNumber.HorizontalAlignment = Element.ALIGN_CENTER;
                    cellPurchasePOIDNumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOIDNumber.BorderColor = new BaseColor(4, 189, 189);
                    cellPurchasePOIDNumber.PaddingBottom = 15;
                    cellPurchasePOIDNumber.PaddingTop = 15;
                    cellPurchasePOIDNumber.BackgroundColor = new BaseColor(4, 189, 189);



                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOIDNumber);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);

                    tableHeading.KeepTogether = true;



                    PdfPTable tablePO = new PdfPTable(4);

                    tablePO.WidthPercentage = 100;

                    tablePO.SetTotalWidth(new float[] { 100, 250, 100, 65 });





                    PdfPCell cellPONumber = new PdfPCell();
                    string cellPONumbertext = "Invoice To:";
                    cellPONumber.Phrase = new Phrase(cellPONumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPONumber.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellPONumber.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPONumber.BorderColor = BaseColor.WHITE;
                    cellPONumber.PaddingTop = 15;


                    PdfPCell cellBPurchasePOInvoice = new PdfPCell();
                    string cellBPurchasePOInvoicetext = SalesOfferDetailsVMObject.InvoiceTo;
                    cellBPurchasePOInvoice.Phrase = new Phrase(cellBPurchasePOInvoicetext, font);
                    cellBPurchasePOInvoice.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //cellBPurchasePOInvoice.Phrase = new Phrase(cellBPurchasePOInvoicetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellBPurchasePOInvoice.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellBPurchasePOInvoice.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellBPurchasePOInvoice.BorderColor = BaseColor.WHITE;
                    cellBPurchasePOInvoice.PaddingTop = 15;












                    PdfPCell cellPOType = new PdfPCell();
                    string cellPOTypetext = "";
                    cellPOType.Phrase = new Phrase(cellPOTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPOType.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellPOType.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPOType.BorderColor = BaseColor.WHITE;
                    cellPOType.PaddingTop = 15;


                    PdfPCell cellPurchasePOInvoiceType = new PdfPCell();
                    string cellPurchasePOInvoiceTypetext = "";
                    cellPurchasePOInvoiceType.Phrase = new Phrase(cellPurchasePOInvoiceTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceType.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellPurchasePOInvoiceType.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceType.BorderColor = BaseColor.WHITE;
                    cellPurchasePOInvoiceType.PaddingTop = 15;





                    PdfPCell cellTelCustomer = new PdfPCell();
                    string cellTelCustomerrtext = "Tel Customer:";
                    cellTelCustomer.Phrase = new Phrase(cellTelCustomerrtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    cellTelCustomer.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellTelCustomer.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellTelCustomer.BorderColor = BaseColor.WHITE;
                    cellTelCustomer.PaddingTop = 15;


                    PdfPCell cellBTelCustomer = new PdfPCell();
                    string cellBTelCustomertext = SalesOfferDetailsVMObject.InvoiceClientPhoneNo;
                    cellBTelCustomer.Phrase = new Phrase(cellBTelCustomertext, font);
                    cellBTelCustomer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    cellBTelCustomer.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellBTelCustomer.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellBTelCustomer.BorderColor = BaseColor.WHITE;
                    cellBTelCustomer.PaddingTop = 15;



                    PdfPCell cellempty = new PdfPCell();
                    string cellemptytext = "";
                    cellempty.Phrase = new Phrase(cellemptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    cellempty.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellempty.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellempty.BorderColor = BaseColor.WHITE;
                    cellempty.PaddingTop = 15;


                    PdfPCell cellemptyRes = new PdfPCell();
                    string cellemptyRestext = "";
                    cellemptyRes.Phrase = new Phrase(cellPurchasePOInvoiceTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    cellemptyRes.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellemptyRes.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellemptyRes.BorderColor = BaseColor.WHITE;
                    cellemptyRes.PaddingTop = 15;





                    #region Car Model ##################################################

                    PdfPCell cellCARMODEL = new PdfPCell();
                    string cellCARMODELtext = "Car Model:";
                    cellCARMODEL.Phrase = new Phrase(cellCARMODELtext, font);
                    cellCARMODEL.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellCARMODEL.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellCARMODEL.BorderColor = BaseColor.WHITE;
                    cellCARMODEL.PaddingTop = 15;


                    PdfPCell cellCARModelRES = new PdfPCell();
                    cellCARModelRES.Phrase = new Phrase(CarModel, font);
                    cellCARModelRES.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellCARModelRES.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellCARModelRES.BorderColor = BaseColor.WHITE;
                    cellCARModelRES.PaddingTop = 15;
                    cellCARModelRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell cellEMPTY = new PdfPCell();
                    cellEMPTY.Phrase = new Phrase("", font);
                    cellEMPTY.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellEMPTY.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellEMPTY.BorderColor = BaseColor.WHITE;
                    cellEMPTY.PaddingTop = 15;


                    PdfPCell cellEMPTYRes = new PdfPCell();
                    cellEMPTYRes.Phrase = new Phrase("", font);
                    cellEMPTYRes.HorizontalAlignment = Element.ALIGN_RIGHT;
                    cellEMPTYRes.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellEMPTYRes.BorderColor = BaseColor.WHITE;
                    cellEMPTYRes.PaddingTop = 15;
                    #endregion

                    #region Car Plate No ##################################################
                    PdfPCell cellCARPlateNo = new PdfPCell();
                    string cellCARPlateNotext = "Plate No:";
                    cellCARPlateNo.Phrase = new Phrase(cellCARPlateNotext, font);
                    cellCARPlateNo.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellCARPlateNo.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellCARPlateNo.BorderColor = BaseColor.WHITE;
                    cellCARPlateNo.PaddingTop = 15;


                    PdfPCell cellCARPlateNoRES = new PdfPCell();
                    cellCARPlateNoRES.Phrase = new Phrase(PlateNo, font);
                    cellCARPlateNoRES.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellCARPlateNoRES.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellCARPlateNoRES.BorderColor = BaseColor.WHITE;
                    cellCARPlateNoRES.PaddingTop = 15;
                    cellCARPlateNoRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car Chassis No ##################################################
                    PdfPCell cellCARChassisNo = new PdfPCell();
                    cellCARChassisNo.Phrase = new Phrase("Chassis No:", font);
                    cellCARChassisNo.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellCARChassisNo.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellCARChassisNo.BorderColor = BaseColor.WHITE;
                    cellCARChassisNo.PaddingTop = 15;


                    PdfPCell cellCARChassisNoRES = new PdfPCell();
                    cellCARChassisNoRES.Phrase = new Phrase(ChassisNumber, font);
                    cellCARChassisNoRES.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellCARChassisNoRES.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellCARChassisNoRES.BorderColor = BaseColor.WHITE;
                    cellCARChassisNoRES.PaddingTop = 15;
                    cellCARChassisNoRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car MaintenanceName  ##################################################
                    PdfPCell cellMaintenanceName = new PdfPCell();
                    cellMaintenanceName.Phrase = new Phrase("Maintenance Name:", font);
                    cellMaintenanceName.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellMaintenanceName.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellMaintenanceName.BorderColor = BaseColor.WHITE;
                    cellMaintenanceName.PaddingTop = 15;


                    PdfPCell cellMaintenanceNameRES = new PdfPCell();
                    cellMaintenanceNameRES.Phrase = new Phrase(MaintenanceName, font);
                    cellMaintenanceNameRES.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellMaintenanceNameRES.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellMaintenanceNameRES.BorderColor = BaseColor.WHITE;
                    cellMaintenanceNameRES.PaddingTop = 15;
                    cellMaintenanceNameRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car CarKiloMeter  ##################################################
                    PdfPCell cellKiloMeter = new PdfPCell();
                    cellKiloMeter.Phrase = new Phrase("KiloMeter:", font);
                    cellKiloMeter.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellKiloMeter.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellKiloMeter.BorderColor = BaseColor.WHITE;
                    cellKiloMeter.PaddingTop = 15;


                    PdfPCell cellKiloMeterRES = new PdfPCell();
                    cellKiloMeterRES.Phrase = new Phrase(CarKiloMeter, font);
                    cellKiloMeterRES.HorizontalAlignment = Element.ALIGN_LEFT;
                    cellKiloMeterRES.VerticalAlignment = Element.ALIGN_MIDDLE;
                    cellKiloMeterRES.BorderColor = BaseColor.WHITE;
                    cellKiloMeterRES.PaddingTop = 15;
                    cellKiloMeterRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion






                    tablePO.AddCell(cellPONumber);
                    tablePO.AddCell(cellBPurchasePOInvoice);

                    tablePO.AddCell(cellPOType);
                    tablePO.AddCell(cellPurchasePOInvoiceType);

                    tablePO.AddCell(cellTelCustomer);
                    tablePO.AddCell(cellBTelCustomer);

                    tablePO.AddCell(cellempty);
                    tablePO.AddCell(cellemptyRes);

                    if (!string.IsNullOrWhiteSpace(CarModel))
                    {
                        tablePO.AddCell(cellCARMODEL);
                        tablePO.AddCell(cellCARModelRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(ChassisNumber))
                    {
                        tablePO.AddCell(cellCARChassisNo);
                        tablePO.AddCell(cellCARChassisNoRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(PlateNo))
                    {
                        tablePO.AddCell(cellCARPlateNo);
                        tablePO.AddCell(cellCARPlateNoRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(MaintenanceName))
                    {
                        tablePO.AddCell(cellMaintenanceName);
                        tablePO.AddCell(cellMaintenanceNameRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(CarKiloMeter))
                    {
                        tablePO.AddCell(cellKiloMeter);
                        tablePO.AddCell(cellKiloMeterRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }


                    PdfPCell CellInvoiceDate = new PdfPCell();
                    string CellInvoiceDatetext = "Bill To: ";
                    CellInvoiceDate.Phrase = new Phrase(CellInvoiceDatetext, font);
                    //CellInvoiceDate.Phrase = new Phrase(CellInvoiceDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFontD = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellInvoiceDate.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellInvoiceDate.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellInvoiceDate.BorderColor = BaseColor.WHITE;
                    CellInvoiceDate.PaddingTop = 10;


                    PdfPCell CellInvoiceDateText = new PdfPCell();
                    CellInvoiceDateText.Phrase = new Phrase(SalesOfferDetailsVMObject.InvoiceTo, font);
                    CellInvoiceDateText.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //CellInvoiceDateText.Phrase = new Phrase(CellInvoiceDateTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellInvoiceDateText.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellInvoiceDateText.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellInvoiceDateText.BorderColor = BaseColor.WHITE;
                    CellInvoiceDateText.PaddingTop = 10;
                    CellInvoiceDateText.Bottom = 15;


                    PdfPCell CellTotalInvoicePrice = new PdfPCell();
                    string CellTotalInvoicePricetext = "";
                    CellTotalInvoicePrice.Phrase = new Phrase(CellTotalInvoicePricetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontt = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellTotalInvoicePrice.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellTotalInvoicePrice.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalInvoicePrice.BorderColor = BaseColor.WHITE;
                    CellTotalInvoicePrice.PaddingTop = 10;
                    CellTotalInvoicePrice.Bottom = 15;


                    PdfPCell CellEmpty = new PdfPCell();
                    string CellEmptytext = "";
                    CellEmpty.Phrase = new Phrase(CellEmptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellEmpty.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellEmpty.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellEmpty.BorderColor = BaseColor.WHITE;
                    CellEmpty.PaddingTop = 10;
                    CellEmpty.Bottom = 15;

                    tablePO.AddCell(CellInvoiceDate);
                    tablePO.AddCell(CellInvoiceDateText);
                    tablePO.AddCell(CellTotalInvoicePrice);
                    tablePO.AddCell(CellEmpty);





                    PdfPCell CellSupplier = new PdfPCell();
                    string CellSuppliertext = "Terms Of Payment: ";
                    CellSupplier.Phrase = new Phrase(CellSuppliertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontK = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellSupplier.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellSupplier.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellSupplier.BorderColor = BaseColor.WHITE;
                    CellSupplier.PaddingTop = 10;


                    PdfPCell CellSupplierName = new PdfPCell();
                    string CellSupplierNametext = SalesOfferDetailsVMObject.TermsOfPayment;
                    CellSupplierName.Phrase = new Phrase(CellSupplierNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontQ = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellSupplierName.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellSupplierName.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellSupplierName.BorderColor = BaseColor.WHITE;
                    CellSupplierName.PaddingTop = 10;
                    CellSupplierName.Bottom = 15;


                    PdfPCell CellPOStatus = new PdfPCell();
                    string CellPOStatustext = "Registration Card:";
                    CellPOStatus.Phrase = new Phrase(CellPOStatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontL = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellPOStatus.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellPOStatus.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellPOStatus.BorderColor = BaseColor.WHITE;
                    CellPOStatus.PaddingTop = 10;
                    CellPOStatus.Bottom = 15;


                    PdfPCell CellPOStatusText = new PdfPCell();
                    string CellPOStatusTexttext = SalesOfferDetailsVMObject.RegistrationCard != null ? SalesOfferDetailsVMObject.RegistrationCard : "N/A";
                    CellPOStatusText.Phrase = new Phrase(CellPOStatusTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontM = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellPOStatusText.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellPOStatusText.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellPOStatusText.BorderColor = BaseColor.WHITE;
                    CellPOStatusText.PaddingTop = 10;
                    CellPOStatusText.Bottom = 15;

                    tablePO.AddCell(CellSupplier);
                    tablePO.AddCell(CellSupplierName);
                    tablePO.AddCell(CellPOStatus);
                    tablePO.AddCell(CellPOStatusText);





                    tablePO.SpacingAfter = 15;






                    PdfPTable table4 = new PdfPTable(4);

                    table4.WidthPercentage = 100;

                    table4.SetTotalWidth(new float[] { 55, 400, 100, 98 });


                    //table4.AddCell(CellVEmpty);
                    //table4.AddCell(CellV2);
                    //table4.AddCell(CellV3);
                    //table4.AddCell(CellV5);



                    //Adding PdfPTable


                    PdfPTable table = new PdfPTable(dt2.Columns.Count);


                    //table Width
                    table.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    table.SetTotalWidth(new float[] { 20, 50, 35, 35, 35, 35, 35, 35 });
                    table.PaddingTop = 20;

                    for (int i = 0; i < dt2.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;

                        cell.PaddingBottom = 5;
                        table.AddCell(cell);


                    }


                    //writing table Data
                    for (int i = 0; i < dt2.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt2.Columns.Count; j++)
                        {
                            //table.AddCell(dt.Rows[i][j].ToString());
                            PdfPCell cell = new PdfPCell();
                            cell.ArabicOptions = 1;
                            if (j <= 5)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new Phrase(1, dt2.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            table.AddCell(cell);

                        }

                    }







                    PdfPTable table3 = new PdfPTable(4);

                    table3.WidthPercentage = 100;

                    table3.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellDirector = new PdfPCell();
                    string CellDirectorText = "";
                    CellDirector.Phrase = new Phrase(CellDirectorText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDirector.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellDirector2 = new PdfPCell();
                    string CellDirectortext2 = "";
                    CellDirector2.Phrase = new Phrase(CellDirectortext2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellDirector2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDirector2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellDirector3 = new PdfPCell();
                    string CellDirector3text = "Price Details";
                    CellDirector3.Phrase = new Phrase(CellDirector3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellDirector3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector3.BorderColor = BaseColor.WHITE;




                    PdfPCell CellDirector4 = new PdfPCell();
                    string CellDirector4text = "";
                    CellDirector4.Phrase = new Phrase(CellDirector4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontVs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellDirector4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellDirector4.BorderColor = BaseColor.WHITE;

                    table3.AddCell(CellDirector);
                    table3.AddCell(CellDirector2);
                    table3.AddCell(CellDirector3);
                    table3.AddCell(CellDirector4);









                    PdfPTable tablTotals = new PdfPTable(3);
                    tablTotals.WidthPercentage = 100;

                    tablTotals.SetTotalWidth(new float[] { 200, 200, 200 });


                    PdfPCell CelltablTotals1 = new PdfPCell();
                    string CelltablTotals1Text = "Final Net Price:";
                    CelltablTotals1.Phrase = new Phrase(CelltablTotals1Text + " " + TotalNetPrice.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals1.HorizontalAlignment = Element.ALIGN_CENTER;
                    CelltablTotals1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CelltablTotals1.BorderColor = BaseColor.WHITE;
                    CelltablTotals1.BackgroundColor = new BaseColor(4, 189, 189);
                    CelltablTotals1.PaddingBottom = 15;
                    CelltablTotals1.PaddingTop = 15;


                    PdfPCell CelltablTotals2 = new PdfPCell();
                    string CelltablTotals2text2 = "Tax Amount:";
                    CelltablTotals2.Phrase = new Phrase(CelltablTotals2text2 + " " + TotalTaxAmount.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals2.PaddingBottom = 15;
                    CelltablTotals2.PaddingTop = 15;



                    CelltablTotals2.HorizontalAlignment = Element.ALIGN_CENTER;
                    CelltablTotals2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CelltablTotals2.BorderColor = BaseColor.WHITE;
                    CelltablTotals2.BackgroundColor = new BaseColor(4, 189, 189);


                    PdfPCell CelltablTotals3 = new PdfPCell();
                    string CelltablTotals3text = "Final Price Amount:";
                    CelltablTotals3.Phrase = new Phrase(CelltablTotals3text + " " + TotalFinalUnitPrice.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals3.HorizontalAlignment = Element.ALIGN_CENTER;
                    CelltablTotals3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CelltablTotals3.BorderColor = BaseColor.WHITE;
                    CelltablTotals3.BackgroundColor = new BaseColor(4, 189, 189);
                    CelltablTotals3.PaddingBottom = 15;
                    CelltablTotals3.PaddingTop = 15;




                    tablTotals.AddCell(CelltablTotals1);
                    tablTotals.AddCell(CelltablTotals2);
                    tablTotals.AddCell(CelltablTotals3);

















                    PdfPTable tableOfferAmount = new PdfPTable(4);

                    tableOfferAmount.WidthPercentage = 100;

                    tableOfferAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellOfferAmount1 = new PdfPCell();
                    string CellOfferAmount1Text = "";
                    CellOfferAmount1.Phrase = new Phrase(CellOfferAmount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNiS = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellOfferAmount1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellOfferAmount1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellOfferAmount1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellOfferAmount2 = new PdfPCell();
                    string CellOfferAmount2text2 = "";
                    CellOfferAmount2.Phrase = new Phrase(CellOfferAmount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSsss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellOfferAmount2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellOfferAmount2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellOfferAmount2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellOfferAmount3 = new PdfPCell();
                    string CellOfferAmount3text = "Offer Amount:";
                    CellOfferAmount3.Phrase = new Phrase(CellOfferAmount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWsssa = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellOfferAmount3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellOfferAmount3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellOfferAmount3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellOfferAmount4 = new PdfPCell();
                    string CellOfferAmount4text = TotalNetPrice.ToString();
                    CellOfferAmount4.Phrase = new Phrase(CellOfferAmount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellOfferAmount4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellOfferAmount4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellOfferAmount4.BorderColor = BaseColor.WHITE;


                    tableOfferAmount.AddCell(CellOfferAmount1);
                    tableOfferAmount.AddCell(CellOfferAmount2);
                    tableOfferAmount.AddCell(CellOfferAmount3);
                    tableOfferAmount.AddCell(CellOfferAmount4);








                    PdfPTable tableTotalDiscount = new PdfPTable(4);

                    tableTotalDiscount.WidthPercentage = 100;

                    tableTotalDiscount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellTotalDiscount1 = new PdfPCell();
                    string CellTotalDiscount1Text = "";
                    CellTotalDiscount1.Phrase = new Phrase(CellTotalDiscount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellTotalDiscount1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalDiscount1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellTotalDiscount2 = new PdfPCell();
                    string CellTotalDiscount2text2 = "";
                    CellTotalDiscount2.Phrase = new Phrase(CellTotalDiscount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSDic = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellTotalDiscount2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellTotalDiscount2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalDiscount2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellTotalDiscount3 = new PdfPCell();
                    string CellTotalDiscount3text = "Total Discount:";
                    CellTotalDiscount3.Phrase = new Phrase(CellTotalDiscount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWsDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellTotalDiscount3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalDiscount3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellTotalDiscount4 = new PdfPCell();
                    string CellTotalDiscount4text = "0.0";
                    CellTotalDiscount4.Phrase = new Phrase(CellTotalDiscount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFonDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellTotalDiscount4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTotalDiscount4.BorderColor = BaseColor.WHITE;


                    tableTotalDiscount.AddCell(CellTotalDiscount1);
                    tableTotalDiscount.AddCell(CellTotalDiscount2);
                    tableTotalDiscount.AddCell(CellTotalDiscount3);
                    tableTotalDiscount.AddCell(CellTotalDiscount4);








                    PdfPTable tableTaxAmount = new PdfPTable(4);

                    tableTaxAmount.WidthPercentage = 100;

                    tableTaxAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellTaxAmount1 = new PdfPCell();
                    string CellTaxAmount1Text = "";
                    CellTaxAmount1.Phrase = new Phrase(CellTaxAmount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellTaxAmount1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTaxAmount1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellTaxAmount2 = new PdfPCell();
                    string CellTaxAmount2text2 = "";
                    CellTaxAmount2.Phrase = new Phrase(CellTaxAmount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTaxAmount = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellTaxAmount2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellTaxAmount2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTaxAmount2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellTaxAmount3 = new PdfPCell();
                    string CellTaxAmount3text = "Tax Amount:";
                    CellTaxAmount3.Phrase = new Phrase(CellTaxAmount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellTaxAmount3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTaxAmount3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellTaxAmount4 = new PdfPCell();
                    string CellTaxAmount4text = TotalTaxAmount.ToString();
                    CellTaxAmount4.Phrase = new Phrase(CellTaxAmount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicTax4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellTaxAmount4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellTaxAmount4.BorderColor = BaseColor.WHITE;


                    tableTaxAmount.AddCell(CellTaxAmount1);
                    tableTaxAmount.AddCell(CellTaxAmount2);
                    tableTaxAmount.AddCell(CellTaxAmount3);
                    tableTaxAmount.AddCell(CellTaxAmount4);









                    PdfPTable tableT4Amount = new PdfPTable(4);

                    tableT4Amount.WidthPercentage = 100;

                    tableT4Amount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellT4Amount1 = new PdfPCell();
                    string CellT4Amount1Text = "";
                    CellT4Amount1.Phrase = new Phrase(CellT4Amount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1T4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT4Amount1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellT4Amount1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT4Amount1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellT4Amount2 = new PdfPCell();
                    string CellT4Amount2text2 = "";
                    CellT4Amount2.Phrase = new Phrase(CellT4Amount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontT4Amount2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellT4Amount2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellT4Amount2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT4Amount2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellT4Amount3 = new PdfPCell();
                    string CellT4Amount3text222 = "T4 Amount:";
                    CellT4Amount3.Phrase = new Phrase(CellT4Amount3text222, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellT4Amount3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellT4Amount3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT4Amount3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellT4Amount4 = new PdfPCell();
                    string CellT4Amount4text11 = "1121238236";
                    CellT4Amount4.Phrase = new Phrase(CellT4Amount4text11, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellT4Amount4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellT4Amount4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT4Amount4.BorderColor = BaseColor.WHITE;


                    tableT4Amount.AddCell(CellT4Amount1);
                    tableT4Amount.AddCell(CellT4Amount2);
                    tableT4Amount.AddCell(CellT4Amount3);
                    tableT4Amount.AddCell(CellT4Amount4);







                    PdfPTable tableT1Amount = new PdfPTable(4);

                    tableT1Amount.WidthPercentage = 100;

                    tableT1Amount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellT1Amount1 = new PdfPCell();
                    string CellT1Amount1Text = "";
                    CellT1Amount1.Phrase = new Phrase(CellT1Amount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1T1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellT1Amount1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT1Amount1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellT1Amount2 = new PdfPCell();
                    string CellT1Amount2text2 = "";
                    CellT1Amount2.Phrase = new Phrase(CellT1Amount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontT1Amount2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellT1Amount2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellT1Amount2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT1Amount2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellT1Amount3 = new PdfPCell();
                    string CellT1Amount3text = "T1 Amount:";
                    CellT1Amount3.Phrase = new Phrase(CellT1Amount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontCellT1AAmount = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellT1Amount3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT1Amount3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellT1Amount4 = new PdfPCell();
                    string CellT1Amount4text = "1121238236";
                    CellT1Amount4.Phrase = new Phrase(CellT1Amount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicTaxT14 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellT1Amount4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellT1Amount4.BorderColor = BaseColor.WHITE;


                    tableT1Amount.AddCell(CellT1Amount1);
                    tableT1Amount.AddCell(CellT1Amount2);
                    tableT1Amount.AddCell(CellT1Amount3);
                    tableT1Amount.AddCell(CellT1Amount4);
























                    PdfPTable tableFinalOfferPriceAmount = new PdfPTable(4);

                    tableFinalOfferPriceAmount.WidthPercentage = 100;

                    tableFinalOfferPriceAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellFinalOfferPrice1 = new PdfPCell();
                    string CellFinalOfferPrice1Text = "";
                    CellFinalOfferPrice1.Phrase = new Phrase(CellFinalOfferPrice1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice1.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellFinalOfferPrice1.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice1.BorderColor = BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellFinalOfferPrice2 = new PdfPCell();
                    string CellFinalOfferPrice2Text = "";
                    CellFinalOfferPrice2.Phrase = new Phrase(CellFinalOfferPrice2Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellFinalOfferPrice2.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellFinalOfferPrice2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice2.BorderColor = BaseColor.WHITE;



                    PdfPCell CellFinalOfferPrice3 = new PdfPCell();
                    string CellFinalOfferPrice3text = "Final Offer Price:";
                    CellFinalOfferPrice3.Phrase = new Phrase(CellFinalOfferPrice3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice3.HorizontalAlignment = Element.ALIGN_RIGHT;
                    CellFinalOfferPrice3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice3.BorderColor = BaseColor.WHITE;



                    PdfPCell CellFinalOfferPrice4 = new PdfPCell();
                    string CellFinalOfferPrice4text = TotalFinalUnitPrice.ToString();
                    CellFinalOfferPrice4.Phrase = new Phrase(CellFinalOfferPrice4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice4.HorizontalAlignment = Element.ALIGN_LEFT;
                    CellFinalOfferPrice4.VerticalAlignment = Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice4.BorderColor = BaseColor.WHITE;


                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice1);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice2);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice3);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice4);

























                    table.SpacingAfter = 20;

                    //if (SupplierPaymenDB != null)
                    //{
                    //    table5.AddCell(CellDirector5);
                    //}

                    Paragraph parag = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, Element.ALIGN_CENTER, 1)));



                    PdfPTable FooterPart = new PdfPTable(1);

                    FooterPart.WidthPercentage = 100;

                    var CompanyName = Validation.CompanyName.ToString().ToLower();
                    PdfPCell FooterCell = new PdfPCell();
                    string FooterCelltext = FromCompanyName;

                    FooterCell.Phrase = new Phrase(FooterCelltext, font);
                    // iTextSharp.text.Font FooterCellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    FooterCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    FooterCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    FooterCell.BorderColor = BaseColor.WHITE;
                    FooterCell.PaddingTop = 15;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell FooterCell2 = new PdfPCell();


                    //var CompanyID = _Context.Clients.Where(x => x.OwnerCoProfile == true).FirstOrDefault();
                    //var CompanyAddress = _Context.ClientAddresses.Where(x => x.ClientID == CompanyID.ID).FirstOrDefault();

                    string FooterCelltext7 = MainCompanyAddress; // CompanyAddress.Address + " " + CompanyAddress.Country.Name + " " + "Building No:" + " " + CompanyAddress.BuildingNumber;
                    FooterCell2.Phrase = new Phrase(FooterCelltext7, font);
                    FooterCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                    FooterCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    FooterCell2.BorderColor = BaseColor.WHITE;
                    //FooterCell2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    PdfPCell FooterCell3 = new PdfPCell();
                    string FooterCelltext8 = "Tax ID:" + " " + TaxCard;

                    FooterCell3.Phrase = new Phrase(FooterCelltext8, font);
                    iTextSharp.text.Font FooterCefont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    FooterCell3.HorizontalAlignment = Element.ALIGN_CENTER;
                    FooterCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    FooterCell3.BorderColor = BaseColor.WHITE;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    //FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    FooterPart.AddCell(FooterCell);
                    FooterPart.AddCell(FooterCell2);
                    if (!string.IsNullOrWhiteSpace(TaxCard))
                    {
                        FooterPart.AddCell(FooterCell3);

                    }





                    table3.SpacingBefore = 30;

                    table.SpacingAfter = 20;

                    document.Add(tableHeading);
                    document.Add(tablePO);
                    document.Add(table4);
                    document.Add(table);

                    document.Add(tablTotals);
                    tablTotals.SpacingAfter = 100;
                    document.Add(table3);
                    document.Add(tableOfferAmount);
                    document.Add(tableTotalDiscount);
                    document.Add(tableTaxAmount);


                    //  document.Add(tableT1Amount);
                    //  document.Add(tableT4Amount);


                    document.Add(tableFinalOfferPriceAmount);
                    parag.SpacingBefore = 100;

                    document.Add(parag);

                    document.Add(FooterPart);


                    FooterPart.SpacingBefore = -100;

                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;



                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                    string PathsTR = "/Attachments/" + CompanyName + "/";
                    String Filepath = _host.WebRootPath+"/"+PathsTR;
                    string p_strPath = Filepath+"/"+FullFileName;
                    if (!System.IO.File.Exists(p_strPath))
                    {
                        var objFileStrm = System.IO.File.Create(p_strPath);
                        objFileStrm.Close();
                    }
                    File.WriteAllBytes(p_strPath, result);

                    Response.Message = Globals.baseURL + PathsTR + FullFileName;






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



