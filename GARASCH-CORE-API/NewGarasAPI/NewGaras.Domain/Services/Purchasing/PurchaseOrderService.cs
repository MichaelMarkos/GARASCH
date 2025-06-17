using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.PurchesRequest.Responses;
using NewGaras.Infrastructure.Models.PurchesRequest.UsedInResponse;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.PurchaseOrder;
using NewGaras.Infrastructure.Models.PurchaseOrder.UsedInResponse;
using NewGaras.Infrastructure.Models.PurchesRequest;
using NewGaras.Infrastructure.Models.Inventory;
using NewGarasAPI.Models.Purchase;
using NewGarasAPI.Models.User;

namespace NewGaras.Domain.Services.Purchasing
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private GarasTestContext _Context;
        static readonly string key = "SalesGarasPass";
        public PurchaseOrderService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, GarasTestContext context)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _Context = context;
        }

        public decimal GetPurchasingTotalPaidAmount()
        {
            decimal TotalPaid = 0;
            var SupplierAccounts = _unitOfWork.SupplierAccounts.FindAll(x => x.Active == true && x.CreationDate <= DateTime.Now).ToList();
            if (SupplierAccounts.Count() > 0)
            {
                foreach (var SupplierAccOBJ in SupplierAccounts)
                {
                    if (SupplierAccOBJ.AmountSign == "plus")
                    {
                        TotalPaid = TotalPaid + SupplierAccOBJ.Amount;
                    }
                    else if (SupplierAccOBJ.AmountSign == "minus")
                    {
                        TotalPaid = TotalPaid - SupplierAccOBJ.Amount;
                    }
                }
            }
            return TotalPaid;
        }

        public decimal GetTotalAmountInventoryItemByListOfItemExpired(List<long> ListOFItem)
        {
            decimal totalValue = 0;
            var InventoryStoreItemQuerableDB = _unitOfWork.VInventoryStoreItemPrices.FindAllQueryable(x => x.Active == true && x.StoreActive == true);
            if (ListOFItem != null)
            {
                InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => ListOFItem.Contains(x.InventoryItemId)).AsQueryable();
            }
            var InventoryStoreItemListDB = InventoryStoreItemQuerableDB.ToList();
            foreach (var Store in InventoryStoreItemListDB)
            {
                if (Store.CalculationType == 1)
                {
                    if (Store.SumaverageUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SumaverageUnitPrice;
                    }
                }
                else if (Store.CalculationType == 2)
                {
                    if (Store.SummaxUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SummaxUnitPrice;
                    }
                }
                else if (Store.CalculationType == 3)
                {
                    if (Store.SumlastUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SumlastUnitPrice;
                    }
                }
                else if (Store.CalculationType == 4)
                {
                    if (Store.SumcustomeUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SumcustomeUnitPrice;
                    }
                }
            }
            return totalValue;
        }

        public Tuple<long, decimal> GetTotalAmountWithNoOFLowStockInventoryItem(long? InventoryStoreID)
        {
            long NoOfItem = 0;
            decimal TotalInvetoryItem = 0;
            var InventoryItemListDB = _unitOfWork.VInventoryStoreItemPrices.FindAllQueryable(x => x.Active == true && x.StoreActive == true);
            if (InventoryStoreID != null && InventoryStoreID != 0)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }
            //var ListOfItemMINBalanceList = InventoryItemListDB.Select(x => new { InventoryItemID = x.InventoryItemID, MinBalance = x.MinBalance }).Distinct().AsQueryable();
            var InventoryItemGroupingPerItem = InventoryItemListDB.GroupBy(x => new { InventoryItemID = x.InventoryItemId, MinBalance = x.MinBalance });
            NoOfItem = InventoryItemGroupingPerItem.Where(x => x.Sum(a => a.Balance) <= x.Key.MinBalance).Count();
            if (InventoryItemListDB.Count() > 0)
            {
                TotalInvetoryItem = InventoryItemListDB.Where(x => x.Balance != null).Sum(a => (decimal)a.Balance);
            }
            return Tuple.Create(NoOfItem, TotalInvetoryItem);
        }

        public int GetNoOfSupplierAddingAndExternalBackOrder(long InventoryStoreID, string OperationType)
        {
            int NoOfItem = 0;
            var InventoryAddingOrderQuerable = _unitOfWork.InventoryAddingOrders.FindAllQueryable(x => x.OperationType == OperationType);
            if (InventoryStoreID != 0)
            {
                InventoryAddingOrderQuerable = InventoryAddingOrderQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }
            NoOfItem = InventoryAddingOrderQuerable.Select(x => x.SupplierId).Distinct().Count();
            return NoOfItem;
        }

        public int GetCountOfSupplier()
        {
            int NoOfItem = _unitOfWork.Suppliers.FindAll(x => x.Active == true).Select(x => x.Id).Distinct().Count();
            return NoOfItem;
        }

        public PurchasingAndSuppliersDashboardResponse GetPurchasingAndSuppliersDashboard([FromHeader] long InventoryStoreID)
        {
            PurchasingAndSuppliersDashboardResponse Response = new PurchasingAndSuppliersDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var PurchasingAndSuppliersDashboardInfoObj = new PurchasingAndSuppliersDashboardInfo();
                if (Response.Result)
                {
                    DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
                    // var SupplierPurchasePOListDB = _Context.V_PurchasePO.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
                    var PurchasePOListDB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true).ToList();
                    decimal PurchasedMaterialTotalAmount = PurchasePOListDB.Where(x => x.TotalInvoiceCost != null).Select(x => x.TotalInvoiceCost??0).Sum();
                    var PurchasedMaterialPOSupplierNo = PurchasePOListDB.Select(x => x.ToSupplierId).Distinct().Count();
                    var PurchasedMaterialPOItemNo = _unitOfWork.PurchasePOItems.FindAll(x => PurchasePOListDB.Select(y => y.Id).Contains(x.PurchasePoid)).Count();

                    var SupplierAccountsTotalAmountPaid = GetPurchasingTotalPaidAmount();
                    var SupplierAccountsTotalAmountRemain = PurchasedMaterialTotalAmount > SupplierAccountsTotalAmountPaid ? PurchasedMaterialTotalAmount - SupplierAccountsTotalAmountPaid : 0;
                    var SupplierAccountsPercentPaid = PurchasedMaterialTotalAmount != 0 ? (SupplierAccountsTotalAmountPaid / PurchasedMaterialTotalAmount) * 100 : 0;



                    var IDSPOOpen = PurchasePOListDB.Where(x => x.Status == "Open").Select(x => x.Id).ToList();
                    var IDSPOClosed = PurchasePOListDB.Where(x => x.Status == "Closed").Select(x => x.Id).ToList();

                    var CountOFPOItemsOpen = _unitOfWork.PurchasePOItems.FindAll(x => IDSPOOpen.Contains(x.PurchasePoid)).Count();
                    var CountOFPOItemsClosed = _unitOfWork.PurchasePOItems.FindAll(x => IDSPOClosed.Contains(x.PurchasePoid)).Count();

                    var NoOfSuppliersSPOOpen = PurchasePOListDB.Where(x => x.Status == "Open").Select(x => x.ToSupplierId).Distinct().AsQueryable();
                    var NoOfSuppliersPOClosed = PurchasePOListDB.Where(x => x.Status == "Closed").Select(x => x.ToSupplierId).Distinct().AsQueryable();




                    var PurchasingRequestOpen = _unitOfWork.PurchaseRequests.FindAllQueryable(x => x.Active == true && x.Status == "Open");
                    var PurchasingRequestClosed = _unitOfWork.PurchaseRequests.FindAllQueryable(x => x.Active == true && x.Status == "Closed");
                    // External back order
                    var ExternalBackOrderLisQuerable = _unitOfWork.InventoryAddingOrders.FindAllQueryable(x => x.OperationType == "Add External Back Order");
                    if (InventoryStoreID != 0)
                    {
                        // Adding and External back order
                        ExternalBackOrderLisQuerable = ExternalBackOrderLisQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                        // PR 
                        PurchasingRequestOpen = PurchasingRequestOpen.Where(x => x.FromInventoryStoreId == InventoryStoreID).AsQueryable();
                        PurchasingRequestClosed = PurchasingRequestClosed.Where(x => x.FromInventoryStoreId == InventoryStoreID).AsQueryable();
                    }
                    var IDSExternalBackOrderOrderList = ExternalBackOrderLisQuerable.Select(x => x.Id).ToList();
                    var ExternalBackOrderItemList = _unitOfWork.InventoryAddingOrderItems.FindAllQueryable(x => IDSExternalBackOrderOrderList.Contains(x.InventoryAddingOrderId));



                    // PO Invoices 
                    var ListOFPOInvoices = _unitOfWork.PurchasePOInvoices.FindAll(x => x.Active == true).ToList();

                    var NOPOInvoiceForOpenPO = ListOFPOInvoices.Where(x => IDSPOOpen.Contains(x.Poid)).Count();
                    var NOPOMissedInvoiceForOpenPO = IDSPOOpen.Count() > NOPOInvoiceForOpenPO ? IDSPOOpen.Count() - NOPOInvoiceForOpenPO : 0;
                    var TotalCostPOInvoiceForOpenPO = ListOFPOInvoices.Where(x => IDSPOOpen.Contains(x.Poid) && x.TotalInvoiceCost != null).Sum(x => (decimal)x.TotalInvoiceCost);

                    var NOPOInvoiceForClosedPO = ListOFPOInvoices.Where(x => IDSPOClosed.Contains(x.Poid)).Count();
                    var NOPOMissedInvoiceForClosedPO = IDSPOClosed.Count() > NOPOInvoiceForClosedPO ? IDSPOClosed.Count() - NOPOInvoiceForClosedPO : 0;
                    var TotalCostPOInvoiceForClosedPO = ListOFPOInvoices.Where(x => IDSPOClosed.Contains(x.Poid) && x.TotalInvoiceCost != null).Sum(x => (decimal)x.TotalInvoiceCost);

                    var TotalInvoicesAmountForPO = ListOFPOInvoices.Where(x => x.TotalInvoiceCost != null).Sum(x => (decimal)x.TotalInvoiceCost);


                    // Fill  Response
                    PurchasingAndSuppliersDashboardInfoObj.NOPOInvoiceForOpenPO = NOPOInvoiceForOpenPO;
                    PurchasingAndSuppliersDashboardInfoObj.NOPOMissedInvoiceForOpenPO = NOPOMissedInvoiceForOpenPO;
                    PurchasingAndSuppliersDashboardInfoObj.TotalCostPOInvoiceForOpenPO = TotalCostPOInvoiceForOpenPO;
                    PurchasingAndSuppliersDashboardInfoObj.NOPOInvoiceForClosedPO = NOPOInvoiceForClosedPO;
                    PurchasingAndSuppliersDashboardInfoObj.NOPOMissedInvoiceForClosedPO = NOPOMissedInvoiceForClosedPO;
                    PurchasingAndSuppliersDashboardInfoObj.TotalCostPOInvoiceForClosedPO = TotalCostPOInvoiceForClosedPO;
                    PurchasingAndSuppliersDashboardInfoObj.TotalInvoicesAmountForPO = TotalInvoicesAmountForPO;

                    // Inventory Data --Expired Items -Low Stock Item
                    // Inventory Expired Item ------
                    //var AddingOrderQuerable = _Context.proc_InventoryAddingOrderLoadAll().Where(x => x.OperationType == "Add New Matrial").AsQueryable();
                    var IDSAddingOrderList = _unitOfWork.InventoryAddingOrders.FindAll(x => x.OperationType == "Add New Matrial").Select(x => x.Id).ToList();
                    var AddingOrderItemList = _unitOfWork.InventoryAddingOrderItems.FindAllQueryable(x => IDSAddingOrderList.Contains(x.InventoryAddingOrderId));

                    var ItemListExpired = AddingOrderItemList.Where(x => x.ExpDate != null ? (DateTime)x.ExpDate >= DateTime.Now : false).Select(x => x.InventoryItemId).Distinct().ToList();
                    PurchasingAndSuppliersDashboardInfoObj.ExpiredItemsNo = ItemListExpired.Count();
                    PurchasingAndSuppliersDashboardInfoObj.ExpiredItemsTotalAmount = GetTotalAmountInventoryItemByListOfItemExpired(ItemListExpired); //0 =>  All Items

                    // Inventory Item low stock Item
                    var TotalAmountWithNoOFLowStockInventoryItem = GetTotalAmountWithNoOFLowStockInventoryItem(null);
                    PurchasingAndSuppliersDashboardInfoObj.LowStockItemsNo = TotalAmountWithNoOFLowStockInventoryItem.Item1;
                    PurchasingAndSuppliersDashboardInfoObj.TotalLowStockItems = TotalAmountWithNoOFLowStockInventoryItem.Item2;




                    // ----------PR
                    var IDSPurchasingRequestOpen = PurchasingRequestOpen.Select(x => x.Id).ToList();
                    var NOSPurchasingRequestOpenFromStore = PurchasingRequestOpen.Select(x => x.FromInventoryStoreId).ToList();
                    var NoSPurchasingRequestClosedFromStore = PurchasingRequestClosed.Select(x => x.FromInventoryStoreId).ToList();
                    var IDSPurchasingRequestClosed = PurchasingRequestClosed.Select(x => x.Id).ToList();
                    var PRItemesOpenLoadAll = _unitOfWork.PurchaseRequestItems.FindAllQueryable(x => IDSPurchasingRequestOpen.Contains(x.PurchaseRequestId)).Select(x => x.Id);
                    var PRItemesClosedLoadAll = _unitOfWork.PurchaseRequestItems.FindAllQueryable(x => IDSPurchasingRequestClosed.Contains(x.PurchaseRequestId)).Select(x => x.Id);


                    // PR Assign and Not Assigned
                    var PurchaseRequestItemDB = _unitOfWork.VPurchaseRequestItems.FindAllQueryable(x => x.ApprovalStatusOfPr == "Approved");
                    var PurchaseRequestItemNOTAssign = PurchaseRequestItemDB.Where(x => x.AssignedTo == null).AsQueryable();
                    var PurchaseRequestItemAssign = PurchaseRequestItemDB.Where(x => x.AssignedTo != null).AsQueryable();
                    var ListOfPersonAssignPRItem = PurchaseRequestItemAssign.Select(x => x.AssignedTo).Distinct().ToList();
                    var PurchaseRequestOBJDB = _unitOfWork.VPurchaseRequestItemsPos.FindAllQueryable(x => x.PritemAssignedTo != null && x.PurchasePoid != null
                                                                   );
                    // && x.PurchaseRequestQuantity < x.PurchaseRequestItemQuantity


                    PurchasingAndSuppliersDashboardInfoObj.PurchasedMaterialTotalAmount = PurchasedMaterialTotalAmount;
                    PurchasingAndSuppliersDashboardInfoObj.PurchasedMaterialPONo = PurchasePOListDB.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PurchasedMaterialPOSupplierNo = PurchasedMaterialPOSupplierNo;
                    PurchasingAndSuppliersDashboardInfoObj.PurchasedMaterialPOItemNo = PurchasedMaterialPOItemNo;

                    // Suuplier Account
                    PurchasingAndSuppliersDashboardInfoObj.SupplierAccountsTotalAmountPaid = SupplierAccountsTotalAmountPaid;
                    PurchasingAndSuppliersDashboardInfoObj.SupplierAccountsTotalAmountRemain = SupplierAccountsTotalAmountRemain;
                    PurchasingAndSuppliersDashboardInfoObj.SupplierAccountsPercentPaid = SupplierAccountsPercentPaid;
                    PurchasingAndSuppliersDashboardInfoObj.SupplierAccountsPercentRemain = SupplierAccountsPercentPaid < 100 ? 100 - SupplierAccountsPercentPaid : 0;

                    // PR
                    PurchasingAndSuppliersDashboardInfoObj.PRClosedNo = IDSPurchasingRequestClosed.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PRClosedFromStoreNo = NoSPurchasingRequestClosedFromStore.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PRItemClosedNo = PRItemesClosedLoadAll.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PROpenNo = IDSPurchasingRequestOpen.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PROpenFromStoreNo = NOSPurchasingRequestOpenFromStore.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PRItemOpenNo = PRItemesOpenLoadAll.Count();

                    PurchasingAndSuppliersDashboardInfoObj.TotalPRItemNo = PurchaseRequestItemDB.Count();
                    // PR Not Assigned 
                    PurchasingAndSuppliersDashboardInfoObj.NOTAssignedPRItemNo = PurchaseRequestItemNOTAssign.Count();
                    PurchasingAndSuppliersDashboardInfoObj.NOTAssignedPRItemPercent = PurchasingAndSuppliersDashboardInfoObj.TotalPRItemNo != 0 ?
                                                                            ((float)PurchasingAndSuppliersDashboardInfoObj.NOTAssignedPRItemNo / (float)PurchasingAndSuppliersDashboardInfoObj.TotalPRItemNo) * 100
                                                                               : 0;
                    // PR Assigned ---
                    PurchasingAndSuppliersDashboardInfoObj.AssignedPRItemNo = PurchaseRequestItemAssign.Count();
                    PurchasingAndSuppliersDashboardInfoObj.AssignedPRItemPercent = PurchasingAndSuppliersDashboardInfoObj.TotalPRItemNo != 0 ?
                                                                               ((float)PurchasingAndSuppliersDashboardInfoObj.AssignedPRItemNo / (float)PurchasingAndSuppliersDashboardInfoObj.TotalPRItemNo) * 100
                                                                               : 0;
                    PurchasingAndSuppliersDashboardInfoObj.NoPersonAssignedPRItem = ListOfPersonAssignPRItem.Count();

                    var ListOfPersonAssignedPRItem = new List<PersonAssignedPO>();
                    if (ListOfPersonAssignPRItem.Count > 0)
                    {
                        var usersData = _unitOfWork.Users.FindAll(a => ListOfPersonAssignPRItem.Contains(a.Id)).ToList();
                        foreach (var item in ListOfPersonAssignPRItem)
                        {
                            var PersonAsignedPRItem = PurchaseRequestItemAssign.Where(x => x.AssignedTo == item).Count();
                            var PersonAssignedPOItem = PurchaseRequestOBJDB.Where(x => x.PritemAssignedTo == item).Count();

                            var personData = usersData.Where(a => a.Id == item).FirstOrDefault();
                            ListOfPersonAssignedPRItem.Add(new PersonAssignedPO
                            {
                                PersonName = personData.FirstName + " " + personData.LastName,//Common.GetUserName((long)item,_Context),
                                PRAssignedNo = PersonAsignedPRItem,
                                PONo = PersonAssignedPOItem,
                                PONoPercent = PersonAsignedPRItem != 0 ? (PersonAssignedPOItem / PersonAsignedPRItem) * 100 : 0
                            });
                        }
                    }
                    PurchasingAndSuppliersDashboardInfoObj.PersonAssingedPOList = ListOfPersonAssignedPRItem;



                    // PO
                    PurchasingAndSuppliersDashboardInfoObj.PoOpenNo = IDSPOOpen.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PoItemOpenNo = CountOFPOItemsOpen;
                    PurchasingAndSuppliersDashboardInfoObj.PoOpenFromSupplierNo = NoOfSuppliersSPOOpen.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PoClosedNo = IDSPOClosed.Count();
                    PurchasingAndSuppliersDashboardInfoObj.PoItemClosedNo = CountOFPOItemsClosed;
                    PurchasingAndSuppliersDashboardInfoObj.PoItemClosedFromSupplierNo = NoOfSuppliersPOClosed.Count();

                    // Inventory External Back Order ----------
                    PurchasingAndSuppliersDashboardInfoObj.ExternalBackOrdersNO = IDSExternalBackOrderOrderList.Count();
                    PurchasingAndSuppliersDashboardInfoObj.ExternalBackOrdersToSupplierNO = GetNoOfSupplierAddingAndExternalBackOrder(InventoryStoreID, "Add External Back Order");
                    PurchasingAndSuppliersDashboardInfoObj.ExternalBackOrdersItemsNO = ExternalBackOrderItemList.Count();

                    PurchasingAndSuppliersDashboardInfoObj.NoOfSupplier = GetCountOfSupplier();


                    Response.Data = PurchasingAndSuppliersDashboardInfoObj;
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

        public SelectPRItemsForAddPOResponse GetSelectPRItemsForAddPO(long? InventoryItemID, long UserID)
        {
            SelectPRItemsForAddPOResponse Response = new SelectPRItemsForAddPOResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SelectPRItemsForAddPO = new List<SelectPRItemsForAssign>();
                if (Response.Result)
                {
                    //if (!CommonClass.CheckUserInRole(UserID, 82))
                    //{
                    //    return RedirectToAction("NotAuthorized", "Home");
                    //}


                    

                    if (Response.Result)
                    {
                        var PurchaseRequestItemOBJDB = _unitOfWork.VPurchaseRequestItems.FindAllQueryable(x => x.AssignedTo == UserID && x.PurchasedQuantity < x.Quantity).AsQueryable();


                        if (InventoryItemID != null)
                        {
                            PurchaseRequestItemOBJDB = PurchaseRequestItemOBJDB.Where(x => x.InventoryItemId == InventoryItemID);
                        }
                        if (PurchaseRequestItemOBJDB != null)
                        {
                            var ListOfPurchaseRequestOBJDB = PurchaseRequestItemOBJDB.ToList();
                            foreach (var items in ListOfPurchaseRequestOBJDB)
                            {
                                if (items.PurchasedQuantity < items.Quantity)
                                {
                                    SelectPRItemsForAssign assignedPRItems = new SelectPRItemsForAssign();
                                    assignedPRItems.PurchaseRequestID = items.PurchaseRequestId;
                                    assignedPRItems.ConvertRateFromPurchasingToRequestionUnit = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;

                                    assignedPRItems.PurchaseRequestItemID = items.Id;
                                    assignedPRItems.ID = items.Id;
                                    assignedPRItems.InventoryMatrialRequestID = items.InventoryMatrialRequestId;
                                    assignedPRItems.InventoryMatrialRequestItemID = items.InventoryMatrialRequestItemId;
                                    assignedPRItems.InventoryItemID = items.InventoryItemId;
                                    assignedPRItems.InventoryItemName = items.ItemName.Trim();
                                    assignedPRItems.InventoryItemCode = items.ItemCode;
                                    assignedPRItems.RecivedQuantity = items.RecivedQuantity;
                                    assignedPRItems.PurchasedUOMShortName = items.UomshortName;
                                    assignedPRItems.ReqQuantity = items.ReqQuantity;
                                    assignedPRItems.RequstionUOMID = items.RequstionUomid;
                                    assignedPRItems.RequstionUOMShortName = items.RequstionUomname;

                                    assignedPRItems.ProjectID = items.ProjectId;
                                    assignedPRItems.ProjectName = items.ProjectName;
                                    assignedPRItems.FabricationOrderID = items.FabricationOrderId;
                                    assignedPRItems.FabricationOrderNumber = items.FabNumber != null ? items.FabNumber.ToString() : "";
                                    assignedPRItems.Comment = items.Comments;
                                    assignedPRItems.POType = items.Exported;

                                    decimal Quantity = items.Quantity != null ? (decimal)items.Quantity : 0;
                                    decimal PurchasedQuantity = items.PurchasedQuantity != null ? (decimal)items.PurchasedQuantity : 0;
                                    decimal requestionQTY = Quantity - PurchasedQuantity;
                                    decimal factor = items.ExchangeFactor != null ? (decimal)items.ExchangeFactor : 0;
                                    decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                                    decimal purchaseUOMQTY = requestionQTY / factor;

                                    assignedPRItems.PurchaseItemQuantity = purchaseQTY; // Old to be removed  
                                    assignedPRItems.PRItemQuantityUOP = purchaseQTY;
                                    assignedPRItems.PurchasedQuantity = items.PurchasedQuantity;
                                    assignedPRItems.RemainQty = items.Quantity - items.PurchasedQuantity;

                                    // New 
                                    assignedPRItems.PRNO = items.PurchaseRequestId.ToString();
                                    assignedPRItems.MRNO = items.InventoryMatrialRequestId.ToString();
                                    assignedPRItems.PRNotes = items.PurchaseRequestNotes;

                                    SelectPRItemsForAddPO.Add(assignedPRItems);
                                }
                            }
                        }


                    }
                    Response.SelectPRItemsForAddPOList = SelectPRItemsForAddPO;

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

        public async Task<BaseResponseWithID> AddNewPurchaseOrder(AddNewPurchaseOrderRequest Request, long UserID)
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


                    long SupplierID = 0;
                    if (Request.SupplierID != 0)
                    {
                        SupplierID = Request.SupplierID;
                        // Check is exist
                        var CheckSupplierDB = await _unitOfWork.Suppliers.FindAsync(x => x.Id == SupplierID);
                        if (CheckSupplierDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Supplier is not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Supplier ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    int POTypeID = 0;
                    if (Request.POType != 0)
                    {
                        POTypeID = Request.POType;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err43";
                        error.ErrorMSG = "Invalid PO Type ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime RequestgDate = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.RequestDate) || !DateTime.TryParse(Request.RequestDate, out RequestgDate))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err37";
                        error.ErrorMSG = "Invalid Reqest Date.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.PurchaseRequestItmesList == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-14";
                        error.ErrorMSG = "please insert at least one Item.";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Request.PurchaseRequestItmesList.Count() <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err78";
                        error.ErrorMSG = "Invalid Inventory Matrial Request Item List";
                        Response.Errors.Add(error);
                    }
                    int Counter = 0;
                    foreach (var item in Request.PurchaseRequestItmesList)
                    {
                        Counter++;
                        if (item.PRItemID != null)
                        {
                            var PurchaseRequestItem = await _unitOfWork.VPurchaseRequestItems.FindAsync(x => x.Id == item.PRItemID && x.PurchasedQuantity < x.Quantity);
                            if (PurchaseRequestItem == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err73";
                                error.ErrorMSG = "Purchase Request Item ID not exist or Quantity is completed " + item.PRItemID + " on selected item #" + Counter;
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                if (item.ApprovedQty == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err73";
                                    error.ErrorMSG = "Invalid ApprovedQty on selected item #" + Counter;
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    decimal Quantity = PurchaseRequestItem.Quantity != null ? (decimal)PurchaseRequestItem.Quantity : 0;
                                    decimal PurchasedQuantity = PurchaseRequestItem.PurchasedQuantity != null ? (decimal)PurchaseRequestItem.PurchasedQuantity : 0;
                                    decimal requestionQTY = Quantity - PurchasedQuantity;
                                    decimal factor = PurchaseRequestItem.ExchangeFactor != null ? (decimal)PurchaseRequestItem.ExchangeFactor : 0;
                                    decimal purchaseQTY = factor != 0 ? requestionQTY / factor : 0;
                                    decimal purchaseUOMQTY = requestionQTY / factor;
                                    //if (item.ApprovedQty > purchaseUOMQTY)
                                    //{
                                    //    Response.Result = false;
                                    //    Error error = new Error();
                                    //    error.ErrorCode = "Err73";
                                    //    error.ErrorMSG = "Invalid ApprovedQty (quantity not availble with exchange factor ) on selected item #" + Counter;
                                    //    Response.Errors.Add(error);
                                    //}
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err73";
                            error.ErrorMSG = "Invalid  Purchase Request Item ID " + item.PRItemID + " on selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                    }
                    //long PurchaseRequestID = 0;
                    if (Response.Result)
                    {
                        //// Check Inventory Report Approved and closed or not
                        //var CheckToInventoryReportListDB = _Context.proc_InventoryReportLoadAll().Where(x => x.Active == true && x.Approved == false && x.Closed == false && x.InventoryStoreID == ToInventoryStorID).ToList();
                        //if (CheckToInventoryReportListDB.Count > 0)
                        //{
                        //    foreach (var InventoryRep in CheckToInventoryReportListDB)
                        //    {
                        //        if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                        //        {
                        //            string storeName = Common.getInventoryStoreName(ToInventoryStorID);
                        //            string errMsg = "Store " + storeName +
                        //                " is under inventory from " +
                        //                InventoryRep.DateFrom.ToString("dd'-'MM'-'yyyy") +
                        //                " to " + InventoryRep.DateTo.ToString("dd'-'MM'-'yyyy");

                        //            Response.Result = false;
                        //            Error error = new Error();
                        //            error.ErrorCode = "Err-44";
                        //            error.ErrorMSG = errMsg;
                        //            Response.Errors.Add(error);
                        //        }
                        //    }
                        //}




                        if (Response.Result)
                        {


                            // Insert Purchase PO 
                            var PurchasePODB = new Infrastructure.Entities.PurchasePo();
                            PurchasePODB.ToSupplierId = SupplierID;
                            PurchasePODB.RequestDate = RequestgDate;
                            PurchasePODB.CreationDate = DateTime.Now;
                            PurchasePODB.CreatedBy = UserID;
                            PurchasePODB.ModifiedDate = DateTime.Now;
                            PurchasePODB.ModifiedBy = UserID;
                            PurchasePODB.Active = true;
                            PurchasePODB.Status = "Open";
                            PurchasePODB.PotypeId = POTypeID;
                            PurchasePODB.AccountantReplyNotes = "Not Assigned To Accountant";
                            PurchasePODB.AssignedPurchasingPersonId = UserID;

                            _unitOfWork.PurchasePOes.Add(PurchasePODB);
                            var Res =  _unitOfWork.Complete();



                            //// Insert Purchase PO 
                            //ObjectParameter IDPO = new ObjectParameter("ID", typeof(long));
                            //var OPInsertion = _Context.proc_PurchasePOInsert(IDPO,
                            //                                   SupplierID, RequestgDate,
                            //                                   DateTime.Now, validation.userID, DateTime.Now, validation.userID,
                            //                                   true, "Open", POTypeID,
                            //                                   null, "Not Assigned To Accountant",
                            //                                   null, null, null, null, null, null, null, null, null, null, null, null,
                            //                                   validation.userID,
                            //                                   null, null
                            //                                   );

                            long POID = PurchasePODB.Id;
                            long PurchaseRequestID = 0;


                            #region items



                            foreach (var item in Request.PurchaseRequestItmesList)
                            {

                                var PurchaseRequestItem = await _unitOfWork.VPurchaseRequestItems.FindAsync(x => x.Id == item.PRItemID);
                                if (PurchaseRequestItem != null)
                                {
                                    PurchaseRequestID = PurchaseRequestItem.PurchaseRequestId;

                                    // Insert Purchase PO  Item
                                    var PurchasePOItemDB = new PurchasePoitem();
                                    PurchasePOItemDB.InventoryMatrialRequestItemId = PurchaseRequestItem.InventoryMatrialRequestItemId;
                                    PurchasePOItemDB.InventoryItemId = (long)PurchaseRequestItem.InventoryItemId;
                                    PurchasePOItemDB.Uomid = (int)PurchaseRequestItem.Uomid;
                                    PurchasePOItemDB.ProjectId = PurchaseRequestItem.ProjectId;
                                    PurchasePOItemDB.FabricationOrderId = PurchaseRequestItem.FabricationOrderId;
                                    PurchasePOItemDB.Comments = PurchaseRequestItem.Comments;
                                    PurchasePOItemDB.PurchasePoid = POID;
                                    PurchasePOItemDB.PurchaseRequestItemId = PurchaseRequestItem.Id;
                                    // Update on 2023-12-31 from (bishoy magdy)
                                    PurchasePOItemDB.ReqQuantity1 = item.ApprovedQty; // PurchaseRequestItem.ReqQuantity;
                                    PurchasePOItemDB.EstimatedCost = (item.EstimatedPrice ?? 0) * item.ApprovedQty;
                                    PurchasePOItemDB.EstimatedUnitCost = item.EstimatedPrice;
                                    PurchasePOItemDB.CurrencyId = item.CurrencyID;
                                    PurchasePOItemDB.RecivedQuantity = 0;

                                    _unitOfWork.PurchasePOItems.Add(PurchasePOItemDB);
                                    _unitOfWork.Complete();


                                    ////// Insert Purchase PO  Item
                                    //ObjectParameter IDPOItem = new ObjectParameter("ID", typeof(long));
                                    //_Context.Myproc_PurchasePOItemInsert(IDPOItem,
                                    //                                   PurchaseRequestItem.InventoryMatrialRequestItemID,
                                    //                                   PurchaseRequestItem.InventoryItemID,
                                    //                                   PurchaseRequestItem.UOMID,
                                    //                                   PurchaseRequestItem.ProjectID,
                                    //                                   PurchaseRequestItem.FabricationOrderID,
                                    //                                   PurchaseRequestItem.Comments,
                                    //                                   POID,
                                    //                                   PurchaseRequestItem.ID,
                                    //                                   item.EstimatedPrice * item.ApprovedQty,
                                    //                                   item.EstimatedPrice,
                                    //                                   item.CurrencyID, null, null, null, null,
                                    //                                   PurchaseRequestItem.ReqQuantity,
                                    //                                   0,
                                    //                                   null
                                    //                               );


                                    var PurchasedQuantity = PurchaseRequestItem.PurchasedQuantity + item.ApprovedQty;
                                    var PRItemObj = await _unitOfWork.PurchaseRequestItems.FindAsync(x => x.Id == PurchaseRequestItem.Id);
                                    PRItemObj.PurchasedQuantity = (double?)PurchasedQuantity;


                                    //_Context.Myproc_PurchaseRequestItemsUpdate_QTY(PurchaseRequestItem.ID, PurchasedQuantity);


                                    //var PurchaseRequestItmesListDB = await _Context.PurchaseRequestItems.Where(x => x.PurchaseRequestID == PurchaseRequestItem.PurchaseRequestID).ToListAsync();

                                    ////var PurchaseRequestItmesList = _Context.proc_PurchaseRequestItemsLoadAll().Where(x => x.PurchaseRequestID == PurchaseRequestItem.PurchaseRequestID).ToList();

                                    //bool found = false;
                                    //if (PurchaseRequestItmesListDB.Count > 0)
                                    //{
                                    //    var TotalQTY = PurchaseRequestItmesListDB.Where(x => x.Quantity != null).Sum(x => x.Quantity);
                                    //    var TotalPurchasedQuantity = PurchaseRequestItmesListDB.Where(x => x.PurchasedQuantity != null).Sum(x => x.PurchasedQuantity);
                                    //    if (TotalQTY != 0 && TotalPurchasedQuantity != null && TotalQTY == TotalPurchasedQuantity)
                                    //        found = true;

                                    //}

                                    //if (found)
                                    //{
                                    //    var PRObjDB = await _Context.PurchaseRequests.Where(x => x.ID == PurchaseRequestItem.PurchaseRequestID).FirstOrDefaultAsync();
                                    //    PRObjDB.Status = "Closed";
                                    //    // _Context.Myproc_PurchaseRequestUpdate_Status(PurchaseRequestItem.PurchaseRequestID, "Closed");
                                    //}
                                    //await _Context.SaveChangesAsync();

                                }
                            }
                            _unitOfWork.Complete();


                            if (PurchaseRequestID != 0)
                            {
                                await UpdatePRStatus(PurchaseRequestID);
                                _unitOfWork.Complete();
                            }

                            #endregion items

                            Response.ID = POID;
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<bool> UpdatePRStatus(long PurchaseRequestID)
        {
            var Res = false;
            if (PurchaseRequestID != 0)
            {
                var PurchaseRequestItmesListDB = await _unitOfWork.PurchaseRequestItems.FindAllAsync(x => x.PurchaseRequestId == PurchaseRequestID);
                #region Update PR Status Closed 
                var PRItemCountDB = PurchaseRequestItmesListDB.Count();
                var PRItemCountPurchasedDB = PurchaseRequestItmesListDB.Where(x => x.PurchasedQuantity >= x.Quantity).Count();
                // Check if all Qty is Purchased 
                if (PRItemCountPurchasedDB >= PRItemCountDB)
                {
                    var PRObjDB = await _unitOfWork.PurchaseRequests.FindAsync(x => x.Id == PurchaseRequestID);
                    PRObjDB.Status = "Closed";
                }
                Res = true;
                #endregion
            }
            return Res;
        }

        public async Task<BaseResponse> UpdatePurchaseOrder(UpdatePurchaseOrderRequest Request)
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

                    var POIDObjDB = await _unitOfWork.PurchasePOes.FindAsync(x => x.Id == Request.POID);
                    if (POIDObjDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "This Purchase Order is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // check validation if can update PO or not allowed
                    // if has POInvoice
                    var POInvoiceDB = await _unitOfWork.PurchasePOInvoices.FindAsync(x => x.Poid == Request.POID);
                    if (POInvoiceDB != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "This Purchase Order has Invoice so you don't allow to update.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var AddingOrderObjDB = await _unitOfWork.InventoryAddingOrderItems.FindAsync(x => x.Poid == Request.POID);
                    if (AddingOrderObjDB != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "This Purchase Order has adding Order so you don't allow to update.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long SupplierID = 0;
                    if (Request.SupplierID != 0)
                    {
                        SupplierID = Request.SupplierID;
                        // Check is exist
                        var CheckSupplierDB = await _unitOfWork.Suppliers.FindAsync(x => x.Id == SupplierID);
                        if (CheckSupplierDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "This Supplier is not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Supplier ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    //int POTypeID = 0;
                    //if (Request.POType != 0)
                    //{
                    //    POTypeID = Request.POType;
                    //}

                    //DateTime? RequestDate = null;
                    //DateTime RequestDateTemp = DateTime.Now;
                    //if (!string.IsNullOrEmpty(Request.RequestDate))
                    //{
                    //    if (!DateTime.TryParse(Request.RequestDate, out RequestDateTemp))
                    //    {
                    //        Response.Result = false;
                    //        Error error = new Error();
                    //        error.ErrorCode = "Err37";
                    //        error.ErrorMSG = "Invalid Request Date.";
                    //        Response.Errors.Add(error);
                    //    }
                    //    else
                    //    {
                    //        RequestDate = RequestDateTemp;
                    //    }
                    //}
                    if (Response.Result)
                    {

                        POIDObjDB.ToSupplierId = SupplierID;
                        //POIDObjDB.POTypeID = POTypeID;
                        //POIDObjDB.RequestDate = RequestDate;

                        var res = _unitOfWork.Complete();
                        if (res > 0)
                        {
                            Response.Result = true;
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponse AddImportPoSettings(ImportPoSettings Request)
        {
            BaseResponse response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    if (Request == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        response.Errors.Add(error);
                        return response;
                    }
                    if (Request.POId == null)
                    {

                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "PO Id Is null";
                        response.Errors.Add(error);
                        return response;
                    }
                    else
                    {
                        var PurchasePO = _unitOfWork.PurchasePOes.Find(a => a.Id == Request.POId);
                        if (PurchasePO == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "PO Id Is not in the database";
                            response.Errors.Add(error);
                            return response;
                        }
                        else
                        {
                            var data = new PurchaseImportPosetting();
                            data.Poid = Request.POId;
                            data.Usdrate = Request.USDRate;
                            data.Gbprate = Request.GBPRate;
                            data.Eurorate = Request.EURORate;
                            data.CustomsDollarRate = Request.CustomsdollarRate;
                            data.Date = DateTime.Now;
                            _unitOfWork.PurchaseImportPOSettings.Add(data);
                            _unitOfWork.Complete();
                            response.Result = true;
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

        public BaseResponse SentToSupplier(SentToSupplier sent, long UserID, string compName)
        {
            BaseResponse response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    if (sent == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        response.Errors.Add(error);
                        return response;
                    }
                    if (sent.POID == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "PO Id Is null";
                        response.Errors.Add(error);
                        return response;
                    }
                    else
                    {
                        var PurchasePO = _unitOfWork.PurchasePOes.GetById(sent.POID);
                        if (PurchasePO == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "PO Id Is not in the database";
                            response.Errors.Add(error);
                            return response;
                        }
                        else
                        {
                            var po = _unitOfWork.PurchasePOes.GetById(sent.POID);
                            po.SentToSupplier = sent.SentToSupp;
                            DateTime.TryParse(sent.DeliveryDate, out DateTime dt);
                            po.SupplierDeliveryDate = dt;
                            po.SentToSupplierMethod = sent.SendingMethod;
                            po.SentToSupplierContactPersonId = sent.ContactPersonID;

                            foreach (var file in sent.Files)
                            {

                                var fileExtension = file.Content.FileName.Split('.').Last();
                                var virtualPath = $"Attachments\\{compName}\\SentToSupplier\\{po.Id}";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(file.Content.FileName.Trim().Replace(" ", ""));
                                
                                string savedpath = Common.SaveFileIFF(virtualPath, file.Content, FileName, fileExtension, _host);//Common.SaveFile("/Attachments/" + po.Id + "/SentToSupplier/", file.FileContent, file.FileName, file.FileExtension);
                                var attachment = new PurchasePoattachment();
                                attachment.Poid = sent.POID;
                                attachment.AttachmentPath = savedpath;
                                attachment.Active = true;
                                attachment.CreationDate = DateTime.Now;
                                attachment.CreatedBy = UserID;
                                attachment.FileName = FileName;
                                attachment.FileExtenssion = fileExtension;
                                _unitOfWork.PurchasePOAttachments.Add(attachment);
                            }
                            _unitOfWork.Complete();
                            response.Result = true;
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

        public BaseResponse AddShippmentDocuments(AddShippmentDocuments doc, long UserID, string CompName)
        {
            BaseResponse response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    if (doc == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        response.Errors.Add(error);
                        return response;
                    }
                    if (doc.PurchasePOShipmentID == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "PurchasePO Shipment ID Is null";
                        response.Errors.Add(error);
                        return response;
                    }
                    else
                    {
                        var PuchasePOShipments = _unitOfWork.PuchasePOShipments.GetById(doc.PurchasePOShipmentID);
                        if (PuchasePOShipments == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "PurchasePO Shipment ID Is not in the database";
                            response.Errors.Add(error);
                            return response;
                        }
                        else
                        {
                            foreach (var document in doc.Documents)
                            {
                                var fileExtension = document.FileContent.FileName.Split('.').Last();
                                var virtualPath = $"Attachments\\{CompName}\\Documents\\{doc.PurchasePOShipmentID}";
                                var FileName = System.IO.Path.GetFileNameWithoutExtension(document.FileContent.FileName.Trim().Replace(" ", ""));

                                string savedpath = Common.SaveFileIFF(virtualPath, document.FileContent, FileName, fileExtension, _host);//Common.SaveFile("/Attachments/" + po.Id + "/SentToSupplier/", file.FileContent, file.FileName, file.FileExtension);


                                //string savedpath = Common.SaveFile("/Attachments/" + doc.PurchasePOShipmentID + "/Documents/", document.FileContent, document.FileName, document.FileExtension);
                                DateTime.TryParse(document.ReceivedIn, out DateTime dt);
                                var newDoc = new PurchasePoshipmentDocument()
                                {
                                    PurchasePoshipmentId = doc.PurchasePOShipmentID,
                                    AttachmentPath = savedpath,
                                    CreatedBy = UserID,
                                    CreationDate = DateTime.Now,
                                    Active = true,
                                    FileName = FileName,
                                    FileExtenssion = fileExtension,
                                    ReceivedIn = dt,
                                    Amount = document.Amount,
                                    CurrencyId = document.CurrencyID,

                                };
                                _unitOfWork.PurchasePOShipmentDocuments.Add(newDoc);
                                _unitOfWork.Complete();
                                var id = newDoc.Id;
                            }
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

        public BaseResponse AddShippingMethodDetails(ShippingMethodDetails details)
        {
            BaseResponse response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    if (details == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "please insert a valid data.";
                        response.Errors.Add(error);
                        return response;
                    }
                    if (details.ShippingMethodID == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Shipping Method ID Id Is null";
                        response.Errors.Add(error);
                        return response;
                    }
                    else
                    {
                        var PurchasePOShipmentShippingMethodDetail = _unitOfWork.PurchasePOShipmentShippingMethodDetails.GetById(details.ShippingMethodID);
                        var currency = _unitOfWork.Currencies.GetById(details.CurrencyID ?? 0);
                        if (PurchasePOShipmentShippingMethodDetail  == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Shipping Method ID Is not in the database";
                            response.Errors.Add(error);
                            return response;
                        }
                        else if (details.CurrencyID != null && currency == null)
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "Currency ID Is not in the database";
                            response.Errors.Add(error);
                            return response;
                        }
                        else
                        {
                            var data = new PurchasePoshipmentShippingMethodDetail();
                            data.ShippingMethodId = details.ShippingMethodID;
                            data.Fees = details.Fees;
                            data.CurrencyId = details.CurrencyID;
                            data.Active = true;
                            _unitOfWork.PurchasePOShipmentShippingMethodDetails.Add(data);
                            _unitOfWork.Complete();
                            response.Result = true;


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

       

        public ViewPOApprovalStatusResponse GetPOApprovalStatus(long POID)
        {
            ViewPOApprovalStatusResponse Response = new ViewPOApprovalStatusResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
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





                    var POApprovalStatusList = new List<POApprovalStatus>();

                    var POApprovalStatusListDB = _unitOfWork.VPoapprovalStatus.FindAll(x => x.Mandatory == true && x.Poid == POID).ToList();
                    if (POApprovalStatusListDB.Count() > 0)
                    {


                        foreach (var Data in POApprovalStatusListDB)
                        {
                            var POApprovalStatus = new POApprovalStatus();
                            POApprovalStatus.ApprovalName = Data.Name;
                            POApprovalStatus.ApprovalUser = Data.FirstName + " " + Data.LastName;
                            POApprovalStatus.Comment = Data.Comment;
                            POApprovalStatus.IsApproved = Data.IsApproved;

                            POApprovalStatusList.Add(POApprovalStatus);
                        }
                    }
                    Response.POApprovalStatusList = POApprovalStatusList;
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


        public async Task<BaseResponseWithID> SendFinalPOToSelectedSupplier(SendFinalPOToSelectedSupplierRequest Request, long UserID)
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
                    if (Request.PoId != 0 && Request.PoId != null)
                    {
                        PoId = (long)Request.PoId;
                        var checkPOValid = await _unitOfWork.PurchasePOes.FindAsync(x => x.Id == PoId);
                        if (checkPOValid == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid PO.";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid PO.";
                        Response.Errors.Add(error);
                    }
                    int ContactThroughId = 0;
                    if (Request.ContactThroughId != 0 && Request.ContactThroughId != null)
                    {
                        ContactThroughId = (int)Request.ContactThroughId;
                        var CheckContactThroughSelected = await _unitOfWork.ContactThroughs.FindAsync(x => x.Id == Request.ContactThroughId);

                        if (CheckContactThroughSelected == null)
                        {
                            Response.Result = false; 
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Contact Through.";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Invalid Contact Through.";
                        Response.Errors.Add(error);
                    }

                    DateTime Date = DateTime.Now;
                    if (string.IsNullOrEmpty(Request.Date) || !DateTime.TryParse(Request.Date, out Date))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err37";
                        error.ErrorMSG = "Invalid Date.";
                        Response.Errors.Add(error);
                    }

                    DateTime? RemindDate = null;
                    DateTime RemindDateTemp = DateTime.Now;
                    if (!string.IsNullOrEmpty(Request.RemindDate))
                    {
                        if (!DateTime.TryParse(Request.RemindDate, out RemindDateTemp))
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err37";
                            error.ErrorMSG = "Invalid Remind Date.";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            RemindDate = RemindDateTemp;
                        }
                    }

                    if (Response.Result)
                    {
                        if (Request.ContactThroughId == 1) // Email
                        {
                            if (Request.ClientEmailFrom == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-14";
                                error.ErrorMSG = "Invalid Client Email";
                                Response.Errors.Add(error);
                            }

                            if (Request.SupplierContactPersonID == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-14";
                                error.ErrorMSG = "Invalid Contact Person Supplier";
                                Response.Errors.Add(error);
                            }
                            if (Request.SupplietEmailTo == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-14";
                                error.ErrorMSG = "Invalid Supplier Email";
                                Response.Errors.Add(error);
                            }
                        }

                        if (Response.Result)
                        {
                            var userData = _unitOfWork.Users.GetById(UserID);

                            var Obj = new PofinalSelecteSupplier();
                            Obj.Poid = PoId;
                            Obj.ContactThroughId = ContactThroughId;
                            Obj.ClientEmailFrom = Request.ClientEmailFrom;
                            Obj.SupplierContactPersonId = Request.SupplierContactPersonID;
                            Obj.SupplietEmailTo = Request.SupplietEmailTo;
                            Obj.Active = true;
                            Obj.CreatedBy = userData.FirstName + " " + userData.LastName;
                            Obj.CreationDate = DateTime.Now;
                            _unitOfWork.PofinalSelecteSuppliers.Add(Obj);

                            var Res =  _unitOfWork.Complete();
                            if (Res > 0) // Send Email 
                            {



                                Response.Result = true;
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponse> ManagePurchaseOrderItem(ManagePurchaseOrderItemRequest Request, long UserID)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                
                if (Response.Result)
                {
                    long POID = 0;
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


                    long POItemID = 0;
                    if (Request.Id != 0)
                    {
                        POItemID = (long)Request.Id;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err225";
                        error.ErrorMSG = "Id is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    bool IsDelete = false;
                    if (Request.IsDelete != null)
                    {
                        IsDelete = (bool)Request.IsDelete;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err43";
                        error.ErrorMSG = "IsDelete is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    // In first must be Validate
                    var POItemOldDB = await _unitOfWork.PurchasePOItems.FindAsync(x => x.Id == POItemID);
                    if (POItemOldDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err433";
                        error.ErrorMSG = "Id is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // 1- PO Status must be Open
                    POID = POItemOldDB.PurchasePoid;
                    var CheckPODB = await _unitOfWork.PurchasePOes.FindAsync(x => x.Id == POItemOldDB.PurchasePoid);
                    if (CheckPODB != null)
                    {
                        if (CheckPODB.Status == "Closed")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err433";
                            error.ErrorMSG = "Can't manage this PO Item becasue This PO Status is Closed.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err433";
                        error.ErrorMSG = "Id is not exist on PO.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    // 2- PO Not Releate by Adding Order Item with the same Item

                    var CheckAddingOrderItemDB = await _unitOfWork.VInventoryAddingOrderItems.FindAllAsync(x => x.Poid == CheckPODB.Id &&
                                                         x.InventoryItemId == POItemOldDB.InventoryItemId);
                    //var CheckPOItemDB = await _Context.PurchasePOItems.Where(x => x.PurchaseRequestItemID == POItemID).FirstOrDefaultAsync();
                    if (CheckAddingOrderItemDB.Any())
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err433";
                        error.ErrorMSG = "Can't manage This PO Item becasue already releated by adding order item.";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    #endregion

                    if (Response.Result)
                    {
                        long PRItemID = POItemOldDB.PurchaseRequestItemId;
                        var PoInvoiceDB = await _unitOfWork.PurchasePoinvoices.FindAsync(x => x.Poid == POID);


                        if (IsDelete)
                        {
                            #region Update PO Invoice Sum
                            if (POItemOldDB.TotalActualPrice != null)
                            {
                            PoInvoiceDB.TotalInvoicePrice -= (POItemOldDB.TotalActualPrice ?? 0);

                            }
                            if (POItemOldDB.FinalUnitCost != null && POItemOldDB.ReqQuantity1 != null)
                            {
                            PoInvoiceDB.TotalInvoiceCost -= ((decimal?)(POItemOldDB.ReqQuantity1 ?? 0) * (POItemOldDB.FinalUnitCost ?? 0));
                            }
                            #endregion

                            // Case 1 : Delete PO Item
                            _unitOfWork.PurchasePOItems.Delete(POItemOldDB);
                            var DeleteRes =  _unitOfWork.Complete();
                            if (Request.ApplyOnPR == true && DeleteRes > 0)
                            {
                                // if selected Apply On PR Too
                                var PRItemRequest = new ManagePurchaseRequestItemRequest();
                                PRItemRequest.Id = PRItemID;
                                PRItemRequest.IsDelete = true;
                                PRItemRequest.ManagedbyPO = true;
                                //OutgoingWebRequestContext requestOut = WebOperationContext.Current.OutgoingRequest;
                                //WebHeaderCollection headersOut = request.Headers;
                                //WebOperationContext.Current.OutgoingRequest.Headers.Add("Header", "Hello");
                                var ResultPRItemRequest = await ManagePRItemCall(PRItemRequest, UserID);

                                if (ResultPRItemRequest.Result == false)
                                {
                                    if (ResultPRItemRequest.Errors.Count() > 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err433";
                                        error.ErrorMSG = "Because your are selected ApplyOnPR - Error from PR Item " + ResultPRItemRequest.Errors[0].ErrorMSG;
                                        Response.Errors.Add(error);
                                        return Response;
                                    }
                                }
                            }




                        }
                        else
                        {
                            // Case 2 : manage Reason or Qty
                            if (Request.Quantity != null)
                            {
                                POItemOldDB.ReqQuantity1 = Request.Quantity;
                                #region Update PO Invoice Cost - Price
                                var CheckPOItemListDB = await _unitOfWork.PurchasePOItems.FindAllAsync(x => x.PurchasePoid == POID);
                                if (PoInvoiceDB != null)
                                {

                                PoInvoiceDB.TotalInvoicePrice = CheckPOItemListDB.Select(x => (decimal?)(x.ReqQuantity1 ?? 0) * (x.ActualUnitPrice ?? 0))
                                                                    .DefaultIfEmpty(0).Sum();
                                PoInvoiceDB.TotalInvoiceCost = CheckPOItemListDB.Select(x => (decimal?)(x.ReqQuantity1 ?? 0) * (x.FinalUnitCost ?? 0))
                                                                    .DefaultIfEmpty(0).Sum();
                                }
                                #endregion
                            }
                            if (!string.IsNullOrEmpty(Request.Reason))
                            {
                                POItemOldDB.Comments = Request.Reason;   // Change this assign in another apies save notes
                            }

                            // Case 3 : is not direct PR
                            // manage POItem on Change  InventoryItemID

                            // Add Direct MR Item with this inventoryitemId
                            // Add New PRItem with this New MR Item

                            // Add New POItem with this New PR Item with New Inventory Item

                            if (Request.InventoryItemId != 0 && Request.InventoryItemId != null)
                            {
                                // check InventoryItem is valid
                                var CheckInvItemDB = await _unitOfWork.InventoryItems.FindAsync(x => x.Id == Request.InventoryItemId);
                                if (CheckInvItemDB != null)
                                {
                                    var PRItemRequest = new ManagePurchaseRequestItemRequest();
                                    PRItemRequest.Id = PRItemID;
                                    PRItemRequest.IsDelete = false;
                                    PRItemRequest.ManagedbyPO = true;
                                    PRItemRequest.InventoryItemId = Request.InventoryItemId;
                                    PRItemRequest.Reason = "Created automatic by PO NO " + POID +
                                                            " Instead of Requested Item " + CheckInvItemDB.Name;
                                    var ResultPRItemRequest = await ManagePRItemCall(PRItemRequest, UserID);
                                    long NewPRItemID = 0;
                                    if (ResultPRItemRequest != null)
                                    {
                                        if (ResultPRItemRequest.Result == false)
                                        {
                                            if (ResultPRItemRequest.Errors.Count() > 0)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err433";
                                                error.ErrorMSG = "Error from PR Item Manage " + ResultPRItemRequest.Errors[0].ErrorMSG;
                                                Response.Errors.Add(error);
                                                return Response;
                                            }
                                        }
                                        else
                                        {
                                            NewPRItemID = ResultPRItemRequest.ID;
                                        }
                                    }
                                    if (NewPRItemID != 0) // Create New PO Item 
                                    {
                                        var NewPRItemDB = await _unitOfWork.PurchaseRequestItems.FindAsync(x => x.Id == NewPRItemID);
                                        if (NewPRItemDB != null)
                                        {
                                            //DAL.Model.PurchasePOItem POItemObj = new PurchasePOItem();
                                            //POItemObj.InventoryMatrialRequestItemID = NewPRItemDB.InventoryMatrialRequestItemID;
                                            //POItemObj.InventoryItemID = (long)Request.InventoryItemId;
                                            //POItemObj.UOMID = POItemOldDB.UOMID;
                                            //POItemObj.ProjectID = POItemOldDB.ProjectID;
                                            //POItemObj.FabricationOrderID = POItemOldDB.FabricationOrderID;
                                            //POItemObj.Comments = "Created automatic by PO Item Manage Instead of Item " + CheckInvItemDB.Name;
                                            //POItemObj.PurchasePOID = POItemOldDB.PurchasePOID;
                                            //POItemObj.PurchaseRequestItemID = NewPRItemID;
                                            //if (Request.Quantity != null)
                                            //{
                                            //    POItemOldDB.ReqQuantity = Request.Quantity;
                                            //}
                                            //else
                                            //{
                                            //    POItemOldDB.ReqQuantity = POItemOldDB.ReqQuantity;
                                            //}
                                            //POItemObj.RecivedQuantity = 0;

                                            //_Context.PurchasePOItems.Add(POItemObj);



                                            // Update POItem
                                            POItemOldDB.InventoryMatrialRequestItemId = NewPRItemDB.InventoryMatrialRequestItemId;
                                            POItemOldDB.InventoryItemId = (long)Request.InventoryItemId;
                                            POItemOldDB.Comments = "Created automatic by PO Item Manage Instead of Item " + CheckInvItemDB.Name;
                                            if (Request.Quantity != null)
                                            {
                                                POItemOldDB.ReqQuantity1 = Request.Quantity;
                                            }
                                            else
                                            {
                                                POItemOldDB.ReqQuantity1 = POItemOldDB.ReqQuantity1;
                                            }
                                            POItemOldDB.RecivedQuantity = 0;


                                        }
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err130";
                                        error.ErrorMSG = "Process manage PO Item Not Completed ,Please try again or contact with admin";
                                        Response.Errors.Add(error);
                                        return Response;
                                    }


                                    #region Update PO Invoice Cost , Price
                                    POItemOldDB.FinalUnitCost = 0;
                                    POItemOldDB.ActualUnitPrice = 0;
                                    POItemOldDB.TotalActualPrice = 0;
                                    PoInvoiceDB.TotalInvoicePrice -= POItemOldDB.TotalActualPrice;
                                    PoInvoiceDB.TotalInvoiceCost -= ((decimal?)POItemOldDB.ReqQuantity1 * POItemOldDB.FinalUnitCost);
                                    #endregion



                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err130";
                                    error.ErrorMSG = "Invalid Inventory Item";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                        }

                        //Part on PR Supplier Offer Item
                        // check in first have PRSupplier
                        var PRSupplierOfferItemList = await _unitOfWork.PRSupplierOfferItems.FindAllAsync(x => x.PritemId == PRItemID);
                        if (PRSupplierOfferItemList != null)
                        {
                            if (PRSupplierOfferItemList.Count() > 0)
                            {
                                foreach (var item in PRSupplierOfferItemList)
                                {
                                    if (Request.IsDelete == true)
                                    {
                                        item.Poid = null;
                                        item.PoitemId = null;
                                    }
                                }
                            }
                        }

                        
                        // Check if exist on PurchasePOInvoiceExtraFees
                        var PurchasePOInvoiceExtraFeesListDB = await _unitOfWork.PurchasePoinvoiceExtraFees.FindAllAsync(x => x.PoitemId == POItemID);
                        if (PurchasePOInvoiceExtraFeesListDB.Count() > 0)
                        {
                            foreach (var item in PurchasePOInvoiceExtraFeesListDB)
                            {
                                item.PoitemId = null;
                            }
                        }


                        var Res =  _unitOfWork.Complete();
                        if (Res > 0)
                        {
                            Response.Result = true;

                            // Check if PO Items is deleted all -> delete PO After Save Changes
                            var CheckPOItemListDB = await _unitOfWork.PurchasePOItems.FindAllAsync(x => x.PurchasePoid == POID);
                            var POItemCountDB = CheckPOItemListDB.Count();
                            var POItemCountRecievedDB = CheckPOItemListDB.Where(x => x.RecivedQuantity1 >= x.ReqQuantity1).Count();
                            if (POItemCountDB == 0)
                            {
                                CheckPODB.Active = false;
                            }
                            #region Update PO Status Closed in case Delete last Item not used
                            if (IsDelete == true)
                            {
                                // Check if all Qty is recived 
                                if (POItemCountRecievedDB >= POItemCountDB)
                                {
                                    CheckPODB.Status = "Closed";
                                }
                            }
                            #endregion

                            _unitOfWork.Complete();
                        }
                        //else
                        //{
                        //    Response.Result = false;
                        //}

                    }
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

        private async Task<BaseResponseWithID> ManagePRItemCall(ManagePurchaseRequestItemRequest Request, long UserId)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();

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


            long PRItemID = 0;
            if (Request.Id != 0)
            {
                PRItemID = (long)Request.Id;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err225";
                error.ErrorMSG = "Id is required.";
                Response.Errors.Add(error);
                return Response;
            }
            bool IsDelete = false;
            if (Request.IsDelete != null)
            {
                IsDelete = (bool)Request.IsDelete;
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err43";
                error.ErrorMSG = "IsDelete is required.";
                Response.Errors.Add(error);
                return Response;
            }

            // In first must be Validate
            // 
            var PRItemOldDB = await _unitOfWork.PurchaseRequestItems.FindAsync(x => x.Id == PRItemID);
            if (PRItemOldDB == null)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err433";
                error.ErrorMSG = "Id is not exist.";
                Response.Errors.Add(error);
                return Response;
            }
            // 1- PR Status must be Open
            var CheckPRDB = await _unitOfWork.PurchaseRequests.FindAsync(x => x.Id == PRItemOldDB.PurchaseRequestId);
            if (CheckPRDB != null)
            {
                // if call from manage PO .. not need check is closed or open ,, because it just replace item with same Status
                if (Request.ManagedbyPO != true)
                {
                    if (CheckPRDB.Status == "Closed")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err433";
                        error.ErrorMSG = "Can't manage this PR Item becasue This PR Status is Closed.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
            }
            else
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err433";
                error.ErrorMSG = "Id is not exist on PR.";
                Response.Errors.Add(error);
                return Response;
            }
            // 2- PO Not Releate by this PR
            var CheckPOItemDB = await _unitOfWork.PurchasePOItems.FindAsync(x => x.PurchaseRequestItemId == PRItemID);
            if (Request.ManagedbyPO != true && Request.InventoryItemId != 0 && Request.InventoryItemId != null)
            {
                if (CheckPOItemDB != null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err433";
                    error.ErrorMSG = "Can't manage This PR Item becasue already releated on PO Item.";
                    Response.Errors.Add(error);
                    return Response;
                }
            }

            if (Response.Result)
            {

                long NewMRItemID = 0;
                decimal NewPRItemQTY = 0;
                // Note if PR is Direct must be update changes on Matrial Request Item as PR Item

                var MatrialRequestItemDB = await _unitOfWork.InventoryMatrialRequestItems.FindAsync(x => x.Id == PRItemOldDB.InventoryMatrialRequestItemId);
                if (IsDelete)
                {
                    // Case 1 : Delete PR Item

                    _unitOfWork.PurchaseRequestItems.Delete(PRItemOldDB);
                    if (CheckPRDB.IsDirectPr == true)
                    {
                        // if Direct PR Delete his matrial request item
                        if (MatrialRequestItemDB != null)
                        {
                            _unitOfWork.InventoryMatrialRequestItems.Delete(MatrialRequestItemDB);
                        }
                    }
                }
                else
                {
                    // Case 2 : manage Reason or Qty
                    if (Request.Quantity != null)
                    {
                        PRItemOldDB.Quantity = (double?)Request.Quantity;
                        if (CheckPRDB.IsDirectPr == true)
                        {
                            MatrialRequestItemDB.PurchaseQuantity = (double?)Request.Quantity;
                        }
                    }

                    if (!string.IsNullOrEmpty(Request.Reason))
                    {
                        PRItemOldDB.PurchaseRequestNotes = Request.Reason;   // Change this assign in another apies save notes
                    }

                    // Case 3 : is not direct PR
                    // manage PRItem on Change  InventoryItemID
                    if (Request.InventoryItemId != 0 && Request.InventoryItemId != null)
                    {
                        // check InventoryItem is valid
                        var CheckInvItemDB = await _unitOfWork.InventoryItems.FindAsync(x => x.Id == Request.InventoryItemId);
                        if (CheckInvItemDB != null)
                        {
                            if (CheckPRDB.IsDirectPr == true) // Direct PR
                            {
                                MatrialRequestItemDB.InventoryItemId = (long)Request.InventoryItemId;
                            }
                            else
                            {
                                // 1-add this item to matrial Request item for the same Matrial Request with comment "added by pr item instead of another item (old item)"
                                var MRItemObj = new InventoryMatrialRequestItem();
                                MRItemObj.InventoryMatrialRequestId = MatrialRequestItemDB.InventoryMatrialRequestId;
                                MRItemObj.Uomid = MatrialRequestItemDB.Uomid;
                                MRItemObj.Comments = "Created automatic by PR NO " + PRItemOldDB.PurchaseRequestId +
                                                     " Instead of Requested Item " + CheckInvItemDB.Name + " in MR NO" + MatrialRequestItemDB.InventoryMatrialRequestId;

                                MRItemObj.InventoryItemId = (long)Request.InventoryItemId;
                                MRItemObj.RecivedQuantity = 0;
                                if (Request.Quantity != null)
                                {
                                    MRItemObj.ReqQuantity = (double?)Request.Quantity;
                                    MRItemObj.PurchaseQuantity = (double?)Request.Quantity;
                                }
                                else
                                {
                                    MRItemObj.ReqQuantity = (double?)MatrialRequestItemDB.ReqQuantity1;
                                    MRItemObj.PurchaseQuantity = (double?)MatrialRequestItemDB.PurchaseQuantity1;
                                }
                                MRItemObj.FromBom = MatrialRequestItemDB.FromBom;

                                _unitOfWork.InventoryMatrialRequestItems.Add(MRItemObj);
                                NewMRItemID = MRItemObj.Id;
                                //// 2- add new PRItem with new matrial Request Item
                                //DAL.Model.PurchaseRequestItem PRItemObj = new PurchaseRequestItem();
                                //PRItemObj.PurchaseRequestID = PRItemOldDB.PurchaseRequestID;
                                //PRItemObj.Comments = MRItemObj.Comments;
                                //PRItemObj.InventoryMatrialRequestItemID = NewMRItemID;
                                //if (!string.IsNullOrEmpty(Request.Reason))
                                //{
                                //    PRItemObj.PurchaseRequestNotes = Request.Reason;
                                //}
                                //PRItemObj.Quantity = MRItemObj.ReqQuantity;
                                //PRItemObj.PurchasedQuantity = 0;

                                //_Context.PurchaseRequestItems.Add(PRItemObj);


                                // Update Old PR Item with new inventory item with new MR Item
                                PRItemOldDB.Comments = MRItemObj.Comments;
                                PRItemOldDB.InventoryMatrialRequestItemId = NewMRItemID;
                                if (!string.IsNullOrEmpty(Request.Reason))
                                {
                                    PRItemOldDB.PurchaseRequestNotes = Request.Reason;
                                }
                                PRItemOldDB.Quantity = (double?)MRItemObj.ReqQuantity1;
                                PRItemOldDB.PurchasedQuantity = 0;





                                NewPRItemQTY = (decimal)(MRItemObj.ReqQuantity1 ?? 0);
                                //NewPRItemID = PRItemObj.ID;

                                //// if managed from PO to change Item 
                                //// - Delete Old POItem first 
                                //// - Then Delete PRItem 
                                //if (Request.ManagedbyPO == true) 
                                //{

                                //}
                                //// 3- Remove Old PRItem that manage on it
                                //_Context.PurchaseRequestItems.Remove(PRItemOldDB);



                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err130";
                            error.ErrorMSG = "Invalid Inventory Item";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }


                }
                //Part on PR Supplier Offer Item
                // check in first have PRSupplier
                var PRSupplierOfferItemList = await _unitOfWork.PRSupplierOfferItems.FindAllAsync(x => x.PritemId == PRItemID);
                if (PRSupplierOfferItemList != null)
                {
                    if (PRSupplierOfferItemList.Count() > 0)
                    {
                        if (Request.IsDelete == true)
                        {
                            // in Delete PR item .. Remove from PR supplier Offer  

                            _unitOfWork.PRSupplierOfferItems.DeleteRange(PRSupplierOfferItemList);
                        }
                        // in update item in PR Item Remove Old PRItems then add new PRItem for All Supplier
                        else // Delete Old PRItem and Create New PRItem 
                        {
                            if ((Request.InventoryItemId != null && NewMRItemID != 0) || Request.Quantity != null)
                            {
                                foreach (var PRSupplierOfferObjDB in PRSupplierOfferItemList) // New 
                                {
                                    if ((Request.InventoryItemId != null && NewMRItemID != 0))
                                    {
                                        //if called from PO set POItem == null for temp until update on POItem with new POItem
                                        //if (Request.ManagedbyPO == true)
                                        //{
                                        //    PRSupplierOfferObjDB.POItemID = item.POItemID;
                                        //}
                                        PRSupplierOfferObjDB.MritemId = NewMRItemID;
                                        PRSupplierOfferObjDB.ReqQuantity = NewPRItemQTY;
                                        PRSupplierOfferObjDB.RecivedQuantity = 0;
                                        PRSupplierOfferObjDB.EstimatedCost = 0;
                                        PRSupplierOfferObjDB.TotalEstimatedCost = 0;

                                    }
                                    else if (Request.Quantity != null)
                                    {
                                        PRSupplierOfferObjDB.ReqQuantity = Request.Quantity;
                                        PRSupplierOfferObjDB.TotalEstimatedCost = PRSupplierOfferObjDB.EstimatedCost * Request.Quantity;
                                    }
                                }

                                //if ((Request.InventoryItemId != null && NewPRItemID != 0 && NewMRItemID != 0))
                                //{
                                //    _Context.PRSupplierOfferItems.RemoveRange(PRSupplierOfferItemList);
                                //}
                            }
                        }
                    }
                }




                var Res = _unitOfWork.Complete();
                // Check if PR Items is deleted all -> delete PR
                var CheckPRItemListCount = await _unitOfWork.PurchaseRequestItems.CountAsync(x => x.PurchaseRequestId == PRItemOldDB.PurchaseRequestId);
                if (CheckPRItemListCount == 0)
                {
                    _unitOfWork.PurchaseRequests.Delete(CheckPRDB);

                    await UpdatePRStatus(PRItemOldDB.PurchaseRequestId);

                    _unitOfWork.Complete();
                }
                if (Res > 0)
                {
                    Response.ID = PRItemID;
                    Response.Result = true;
                }
                else
                {
                    Response.Result = false;
                }
            }


            return Response;
        }

        public async Task<BaseResponse> ManageAddingOrderItemPO(ManageAddingOrderItemPORequest Request, long UserID)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    long? OldPOID = 0;
                    long? NewPOID = 0;
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


                    long AddingOrderItemID = 0;
                    if (Request.Id != 0)
                    {
                        AddingOrderItemID = (long)Request.Id;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err225";
                        error.ErrorMSG = "Id is required.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    // In first must be Validate
                    var AddingOrderItemDB = await _unitOfWork.InventoryAddingOrderItems.FindAsync(x => x.Id == AddingOrderItemID);
                    if (AddingOrderItemDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err433";
                        error.ErrorMSG = "Adding Order Item Id is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    /*
                     - Validations
                    1- POID Required with isnew == false or IsNew = true to create new PO
                    2- POID is already exist
                    3- POID Is Already exist on adding order Item
                    4-Inv Item is there in PO Item selected 
                     */

                    NewPOID = Request.POID;
                    OldPOID = AddingOrderItemDB.Poid;

                    // Back List Of Purchase PO Contain Old PO and New PO (for call DB just one time) 
                    var PurchasePOListDB = await _unitOfWork.PurchasePOes.FindAllAsync(x => x.Id == NewPOID || x.Id == OldPOID);
                    // Back List Of Purchase PO Item Contain Old PO and New PO (for call DB just one time) 
                    var PurchasePOItemListDB = await _unitOfWork.PurchasePOItems.FindAllAsync(x => x.PurchasePoid == NewPOID || x.PurchasePoid == OldPOID);


                    Infrastructure.Entities.PurchasePoitem NewPOItemDB = null;
                    if (Request.IsNew != true)
                    {
                        if (Request.POID == null || Request.POID == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err225";
                            error.ErrorMSG = "PO ID is required.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        var NewPODB = PurchasePOListDB.Where(x => x.Id == NewPOID).FirstOrDefault();
                        if (NewPODB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err225";
                            error.ErrorMSG = "PO ID is not exist.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        if (NewPODB.Status != "Open")
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err225";
                            error.ErrorMSG = "PO ID is selected is Closed.";
                            Response.Errors.Add(error);
                            return Response;
                        }


                        NewPOItemDB = PurchasePOItemListDB.Where(x => x.PurchasePoid == NewPOID && x.InventoryItemId == AddingOrderItemDB.InventoryItemId && x.RecivedQuantity1 <= x.ReqQuantity1).FirstOrDefault();
                        if (NewPOItemDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err225";
                            error.ErrorMSG = "This PO Not have Item or not have available QTY.";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }




                    if (AddingOrderItemDB.Poid == NewPOID)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err225";
                        error.ErrorMSG = "PO ID you are selected is already in this adding order item.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        long SuplierID =  _unitOfWork.VInventoryAddingOrders.Find(x => x.Id == AddingOrderItemDB.InventoryAddingOrderId).SupplierId;
                        /*
                         Case 1 :
                            Change POID From list PO and Replace
                            - On Old PO Update (Status - Recived QTY)
                            - on New PO //
                            - Update inventory store Item ,PO,POItem,Cost,....
                         Case 2 :
                            Create New       PO (With POItem) 
                                          <- Direct PR (with PRItem)
                                          <- MR (with MRItem)
                         */
                        if (Request.IsNew == true)
                        {
                            // Create New PO With Item in addin order Item
                            var ResponseNewPO = await CreateNewPOCall(SuplierID,
                                                          UserID,
                                                          AddingOrderItemDB.InventoryItemId,
                                                          AddingOrderItemDB.ReqQuantity1 ?? 0);
                            if (ResponseNewPO.Result)
                            {
                                NewPOID = ResponseNewPO.ID;
                                NewPOItemDB = await _unitOfWork.PurchasePOItems.FindAsync(x => x.PurchasePoid == NewPOID && x.InventoryItemId == AddingOrderItemDB.InventoryItemId && x.RecivedQuantity1 <= x.ReqQuantity1);
                                if (NewPOItemDB == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err225";
                                    error.ErrorMSG = "This PO Not have Item or not have available QTY.";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Response.Errors = ResponseNewPO.Errors;
                                return Response;
                            }

                        }

                        /*
                         * Old POID
                          -Update Old PO Status to Open 
                          -Subtract Qty from addingorderItem from recieved POItem
                         *New POID
                          -Increase Qty from adding order  to recieved POItem
                          -Check If PO Closed 

                        -- Update All Inventory store Item and all childred from old po to new po item and calc all column again
                        */



                        long? OldPOItemID = PurchasePOItemListDB.Where(x => x.PurchasePoid == OldPOID && x.InventoryItemId == AddingOrderItemDB.InventoryItemId).Select(x => x.Id).FirstOrDefault();
                        bool UpdateOldPOAndNew = await UpdatePOStatusWithPOItemRecievedQTY(PurchasePOListDB.ToList(),
                                                                                            PurchasePOItemListDB.ToList(),
                                                                                            NewPOItemDB.Id,
                                                                                            OldPOItemID,
                                                                                            AddingOrderItemDB.RecivedQuantity1);

                        if (UpdateOldPOAndNew)
                        {
                            // Update inventory store Item and all children
                            var UpdateInventoryStoreItem = await UpdateInventoryStoreItemFromManageAddingOrder(AddingOrderItemDB.Id, AddingOrderItemDB.InventoryItemId,
                                                                                                         OldPOID,
                                                                                                         NewPOID,
                                                                                                         NewPOItemDB);
                            if (UpdateInventoryStoreItem)
                            {
                                AddingOrderItemDB.Poid = NewPOID;
                                var Res =  _unitOfWork.Complete();
                                if (Res > 0)
                                {
                                    Response.Result = true;
                                }
                                else
                                {
                                    Response.Result = false;
                                }
                            }

                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err225";
                            error.ErrorMSG = "Manage Adding Order Item Process not completed,Please try again.";
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
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithID> CreateNewPOCall(long SupplierID, long LoginUserID, long InventoryItemID, decimal QTY)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            long NewPOID = 0;


            DateTime RequestDate = DateTime.Now;
            int POTypeID = 1;
            var CheckSupplierCountryID =  _unitOfWork.SupplierAddresses.Find(x => x.SupplierId == SupplierID).CountryId;

            if (CheckSupplierCountryID != 1)
            {
                POTypeID = 2;
            }

            // Insert Purchase PO 
            var PurchasePODB = new PurchasePo();
            PurchasePODB.ToSupplierId = SupplierID;
            PurchasePODB.RequestDate = RequestDate;
            PurchasePODB.CreationDate = DateTime.Now;
            PurchasePODB.CreatedBy = LoginUserID;
            PurchasePODB.ModifiedDate = DateTime.Now;
            PurchasePODB.ModifiedBy = LoginUserID;
            PurchasePODB.Active = true;
            PurchasePODB.Status = "Open";
            PurchasePODB.PotypeId = POTypeID;
            PurchasePODB.AccountantReplyNotes = "Not Assigned To Accountant";
            PurchasePODB.AssignedPurchasingPersonId = LoginUserID;

            _unitOfWork.PurchasePOes.Add(PurchasePODB);
            //var Res = await _Context.SaveChangesAsync();




            long POID = PurchasePODB.Id;



            #region items
            var AddMatrialDirectPRRequest = new AddMatrialDirectPRRequest();
            AddMatrialDirectPRRequest.FromPODirect = true;
            AddMatrialDirectPRRequest.LoginUserID = LoginUserID;
            AddMatrialDirectPRRequest.RequestgDate = RequestDate.ToShortDateString();
            var ItemObj = new DirectPRItem();
            ItemObj.ReqQTY = QTY;
            ItemObj.InventoryItemID = InventoryItemID;
            ItemObj.DirectPrNotes = "Created direct from PO and PO Created from adding Order";
            var DirectPRItemList = new List<DirectPRItem>();
            DirectPRItemList.Add(ItemObj);
            AddMatrialDirectPRRequest.DirectPRItemList = DirectPRItemList;

            var MRWithDirectPRCall = await CreateMRWithDirectPRCall(AddMatrialDirectPRRequest);
            if (MRWithDirectPRCall.Result == true)
            {
                // Insert Purchase PO  Item
                var PurchaseRequestItem = await _unitOfWork.VPurchaseRequestItems.FindAsync(x => x.Id == MRWithDirectPRCall.PRItemID);
                if (PurchaseRequestItem != null)
                {


                    // Insert Purchase PO  Item
                    var PurchasePOItemDB = new PurchasePoitem();
                    PurchasePOItemDB.InventoryMatrialRequestItemId = PurchaseRequestItem.InventoryMatrialRequestItemId;
                    PurchasePOItemDB.InventoryItemId = (long)PurchaseRequestItem.InventoryItemId;
                    PurchasePOItemDB.Uomid = (int)PurchaseRequestItem.Uomid;
                    PurchasePOItemDB.ProjectId = PurchaseRequestItem.ProjectId;
                    PurchasePOItemDB.FabricationOrderId = PurchaseRequestItem.FabricationOrderId;
                    PurchasePOItemDB.Comments = PurchaseRequestItem.Comments;
                    PurchasePOItemDB.PurchasePoid = PurchasePODB.Id;
                    PurchasePOItemDB.PurchaseRequestItemId = PurchaseRequestItem.Id;
                    PurchasePOItemDB.ReqQuantity1 = PurchaseRequestItem.ReqQuantity;
                    PurchasePOItemDB.EstimatedCost = 0;
                    PurchasePOItemDB.EstimatedUnitCost = 0;
                    PurchasePOItemDB.CurrencyId = null;
                    PurchasePOItemDB.RecivedQuantity = 0;

                    _unitOfWork.PurchasePOItems.Add(PurchasePOItemDB);
                }


            }
            else // message can't completed
            {
                Response.Result = false;
                Response.Errors = MRWithDirectPRCall.Errors;
            }



            #endregion items


            var Res =  _unitOfWork.Complete();
            if (Res > 0)
            {
                NewPOID = PurchasePODB.Id;
            }
            Response.ID = NewPOID;
            return Response;


        }

        public async Task<AddMatrialDirectPRResponse> CreateMRWithDirectPRCall(AddMatrialDirectPRRequest Request)
        {
            AddMatrialDirectPRResponse Response = new AddMatrialDirectPRResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            //try
            //{

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

                DateTime RequestgDate = DateTime.Now;
                if (string.IsNullOrEmpty(Request.RequestgDate) || !DateTime.TryParse(Request.RequestgDate, out RequestgDate))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err37";
                    error.ErrorMSG = "Invalid Reqest Date.";
                    Response.Errors.Add(error);
                    return Response;
                }


                if (Request.DirectPRItemList == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-14";
                    error.ErrorMSG = "please insert at least one Item.";
                    Response.Errors.Add(error);
                    return Response;
                }


                if (Request.DirectPRItemList.Count <= 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-14";
                    error.ErrorMSG = "please insert at least one Item.";
                    Response.Errors.Add(error);
                    return Response;
                }
                int Counter = 0;
                foreach (var item in Request.DirectPRItemList)
                {
                    Counter++;
                    if (item.InventoryItemID <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err27";
                        error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                        Response.Errors.Add(error);
                    }
                    if (item.ReqQTY <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err29";
                        error.ErrorMSG = "Invalid QTY item #" + Counter;
                        Response.Errors.Add(error);
                    }

                }
                long MatrialRequestId = 0;
                long PurchaseRequestId = 0;
                long MatrialRequestItemId = 0;
                long PurchaseRequestItemId = 0;
                if (Response.Result)
                {
                    var MRItemIDSList = new List<long>();
                    // Check if inventory Dirct is not found to add one
                    string StoreName = "DIRECT PR HIDDEN STORE";
                    var InventoryStoreID = _unitOfWork.InventoryStores.Find(a => a.Name.Contains(StoreName)).Id;
                    if (InventoryStoreID == 0)
                    {
                        // Inserty Inventory Store ID
                        var inventoryStoreObj = new InventoryStore();
                        inventoryStoreObj.Name = StoreName;
                        inventoryStoreObj.Active = true;
                        inventoryStoreObj.CreationDate = DateTime.Now;
                        inventoryStoreObj.ModifiedDate = DateTime.Now;
                        inventoryStoreObj.CreatedBy = Request.LoginUserID;
                        inventoryStoreObj.ModifiedBy = Request.LoginUserID;

                        _unitOfWork.InventoryStores.Add(inventoryStoreObj);

                        //ObjectParameter IDInventoryStore = new ObjectParameter("ID", typeof(int));
                        //_Context.proc_InventoryStoreInsert(IDInventoryStore,
                        //                                   StoreName,
                        //                                   true,
                        //                                   null, null,
                        //                                   DateTime.Now,
                        //                                   validation.userID,
                        //                                   DateTime.Now,
                        //                                   validation.userID);

                        InventoryStoreID = inventoryStoreObj.Id;
                    }



                    #region Save Matrial Request  - Save PR

                    // Insertion Matrial Request 
                    var MRObj = new InventoryMatrialRequest();
                    MRObj.FromUserId = Request.LoginUserID;
                    MRObj.ToInventoryStoreId = InventoryStoreID;
                    MRObj.RequestDate = RequestgDate;
                    MRObj.CreationDate = DateTime.Now;
                    MRObj.CreatedBy = Request.LoginUserID;
                    MRObj.ModifiedBy = Request.LoginUserID;
                    MRObj.CreationDate = DateTime.Now;
                    MRObj.ModifiedDate = DateTime.Now;
                    MRObj.Active = true;
                    MRObj.Status = "Closed";
                    MRObj.ApproveResult = "Approved";

                    _unitOfWork.InventoryMatrialRequests.Add(MRObj);


                    //ObjectParameter IDMR = new ObjectParameter("ID", typeof(long));
                    //var MRInsertion = _Context.proc_InventoryMatrialRequestInsert(IDMR,
                    //                                                              validation.userID,
                    //                                                              InventoryStoreID,
                    //                                                              RequestgDate,
                    //                                                              DateTime.Now,
                    //                                                              validation.userID,
                    //                                                              DateTime.Now,
                    //                                                              validation.userID,
                    //                                                              true,
                    //                                                              "Closed",
                    //                                                              null,
                    //                                                              "Approved",
                    //                                                              null
                    //                                                              );


                    MatrialRequestId = (long)MRObj.Id;
                    //MRItemIDSList.Add(mrId);



                    //if (MatrialRequestId != 0)
                    //{

                    var PRObj = new PurchaseRequest();
                    PRObj.FromInventoryStoreId = InventoryStoreID;
                    PRObj.RequestDate = RequestgDate;
                    PRObj.CreationDate = DateTime.Now;
                    PRObj.ModifiedDate = DateTime.Now;
                    PRObj.CreatedBy = Request.LoginUserID;
                    PRObj.ModifiedBy = Request.LoginUserID;
                    PRObj.Active = true;
                    if (Request.FromPODirect)
                    {
                        PRObj.Status = "Closed";
                    }
                    else
                    {
                        PRObj.Status = "Open";
                    }
                    PRObj.MatrialRequestId = MatrialRequestId;
                    PRObj.IsDirectPr = true;
                    PRObj.ApprovalUserId = Request.LoginUserID;
                    PRObj.ApprovalReplyData = DateTime.Now;
                    PRObj.ApprovalStatus = "Approved";
                    PRObj.ApprovalReplyNotes = "Automatically Approved";

                    _unitOfWork.PurchaseRequests.Add(PRObj);


                    //ObjectParameter IDPurchaseRequest = new ObjectParameter("ID", typeof(long));
                    //var PurchaseRequest = _Context.proc_PurchaseRequestInsert(IDPurchaseRequest,
                    //                                                          null,
                    //                                                          InventoryStoreID,
                    //                                                          RequestgDate,
                    //                                                          DateTime.Now,
                    //                                                          validation.userID,
                    //                                                          DateTime.Now,
                    //                                                          validation.userID,
                    //                                                          true,
                    //                                                          "Open",
                    //                                                          mrId,
                    //                                                          true,
                    //                                                          "Approved",
                    //                                                          validation.userID,
                    //                                                          "Automatically Approved",
                    //                                                          DateTime.Now
                    //                                                          //"Waiting For Reply",
                    //                                                          //null, null, null
                    //                                                          );
                    PurchaseRequestId = PRObj.Id;
                    //if (PurchaseRequestId != 0)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorMSG = "Unable to create Purchase Request, please try again or contact your admin";
                    //    error.ErrorCode = "Err17";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    //}
                    //else
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorMSG = "Unable to create Material Request,  please try again or  contact your admin";
                    //    error.ErrorCode = "Err14";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}

                    #region Matrial Request items with Purchase Request Item

                    var MRItemObj = new InventoryMatrialRequestItem();
                    var PRItemObj = new PurchaseRequestItem();

                    foreach (var item in Request.DirectPRItemList)
                    {
                        bool FromBom = false;
                        var InventoryItemObjDB = await _unitOfWork.InventoryItems.FindAsync(x => x.Id == item.InventoryItemID);
                        //_Context.proc_InventoryItemLoadByPrimaryKey(MatrialDataOBJ.InventoryItemID).FirstOrDefault();
                        if (InventoryItemObjDB != null)
                        {

                            //add MR Item
                            MRItemObj = new InventoryMatrialRequestItem();
                            MRItemObj.InventoryMatrialRequestId = MatrialRequestId;
                            MRItemObj.InventoryItemId = item.InventoryItemID;
                            MRItemObj.Uomid = InventoryItemObjDB.RequstionUomid;
                            MRItemObj.Comments = item.Comment;
                            //MRItemObj.Comments = "Created automatic by PR NO " + PurchaseRequestID +
                            //                     " for new PO with Item " + InventoryItemObjDB.Name + " in MR NO" + mrId;
                            MRItemObj.RecivedQuantity1 = item.ReqQTY;
                            MRItemObj.ReqQuantity1 = item.ReqQTY;
                            MRItemObj.PurchaseQuantity1 = item.ReqQTY;
                            MRItemObj.FromBom = FromBom;

                            _unitOfWork.InventoryMatrialRequestItems.Add(MRItemObj);

                            //ObjectParameter IDInventoryMatrialRequestItem = new ObjectParameter("ID", typeof(long));
                            //var MatrialRequestItemInsertion = _Context.Myproc_InventoryMatrialRequestItemsInsert_New(IDInventoryMatrialRequestItem,
                            //                                                                                mrId,
                            //                                                                                MatrialDataOBJ.InventoryItemID,
                            //                                                                                InventoryItemObjDB.RequstionUOMID,
                            //                                                                                null,
                            //                                                                                null,
                            //                                                                                MatrialDataOBJ.Comment,
                            //                                                                                0,
                            //                                                                                MatrialDataOBJ.ReqQTY,
                            //                                                                                MatrialDataOBJ.ReqQTY,
                            //                                                                                FromBom,
                            //                                                                                null,
                            //                                                                                null
                            //                                                                             );

                            MatrialRequestItemId = MRItemObj.Id;

                            //if (PurchaseRequestId != 0)
                            //{

                            PRItemObj = new PurchaseRequestItem();
                            PRItemObj.PurchaseRequestId = PurchaseRequestId;
                            PRItemObj.Comments = item.Comment;
                            PRItemObj.PurchaseRequestNotes = item.DirectPrNotes;
                            PRItemObj.InventoryMatrialRequestItemId = MatrialRequestItemId;
                            PRItemObj.Quantity = (double?)item.ReqQTY;
                            if (Request.FromPODirect)
                            {
                                PRItemObj.PurchasedQuantity = (double?)item.ReqQTY;
                            }
                            else
                            {
                                PRItemObj.PurchasedQuantity = 0;
                            }

                            _unitOfWork.PurchaseRequestItems.Add(PRItemObj);

                            PurchaseRequestItemId = PRItemObj.Id;
                            //ObjectParameter IDPurchaseRequestItem = new ObjectParameter("ID", typeof(long));
                            //_Context.Myproc_PurchaseRequestItemsInsert(IDPurchaseRequestItem,
                            //                                         PurchaseRequestID,
                            //                                         MatrialDataOBJ.Comment,
                            //                                         MRItemID,
                            //                                         MatrialDataOBJ.DirectPrNotes,
                            //                                         null,
                            //                                         MatrialDataOBJ.ReqQTY,
                            //                                         0,
                            //                                         null
                            //                                         );

                            //}
                        }

                    }

                    #endregion items

                    #endregion  Save Matrial Request  - Save PR
                    var Res =  _unitOfWork.Complete();
                    if (Res > 0)
                    {
                        Response.Result = true;
                        Response.PRID = PRObj.Id;
                        Response.MRItemID = MRItemObj.Id;
                        Response.PRItemID = PRItemObj.Id;
                    }
                    else
                    {
                        Response.Result = false;
                    }

                }


            }
            return Response;

            //}
            //catch (Exception ex)
            //{
            //    Response.Result = false;
            //    Error error = new Error();
            //    error.ErrorCode = "Err10";
            //    error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
            //    Response.Errors.Add(error);
            //    return Response;
            //}
        }

        public async Task<bool> UpdatePOStatusWithPOItemRecievedQTY(List<Infrastructure.Entities.PurchasePo> POListDB,
                                                                    List<Infrastructure.Entities.PurchasePoitem> POItemListDB,
                                                                    long? NewPOItemID,
                                                                    long? OldPOItemID,
                                                                    decimal? RecievedQTY)
        {
            bool Result = false;
            if (POItemListDB.Count() > 0)
            {
                // Update Old PO 
                if (OldPOItemID != null && OldPOItemID != 0) // subtract Qty from POItem
                {
                    /*
                     - Update PO Status to Open if closed 
                     - Update POItem Recived QTY or set assign 0
                     */
                    var OldPOItemDB = POItemListDB.Where(x => x.Id == OldPOItemID).FirstOrDefault();
                    if (OldPOItemDB != null)
                    {
                        if (RecievedQTY != null && OldPOItemDB.RecivedQuantity1 >= RecievedQTY)
                        {
                            OldPOItemDB.RecivedQuantity1 -= RecievedQTY;
                        }
                        else
                        {
                            OldPOItemDB.RecivedQuantity1 = 0;
                        }
                        var PODB = POListDB.Where(x => x.Id == OldPOItemDB.PurchasePoid).FirstOrDefault();
                        PODB.Status = "Open";
                    }
                }
                else
                {
                    return false;
                }

                // Update New PO 
                if (NewPOItemID != null && NewPOItemID != 0) // New PO increase QTY
                {
                    /*
                    - Update PO Status after check is all recieved
                    - Update Increase Recieved QTY on PO Item
                    */
                    var NewPOItemDB = POItemListDB.Where(x => x.Id == NewPOItemID).FirstOrDefault();
                    if (NewPOItemDB != null)
                    {
                        if (RecievedQTY != null)
                        {
                            if (NewPOItemDB.RecivedQuantity1 != null)
                            {
                                NewPOItemDB.RecivedQuantity1 += RecievedQTY;
                            }
                            else
                            {
                                NewPOItemDB.RecivedQuantity1 = RecievedQTY;
                            }
                        }
                        var PODB = POListDB.Where(x => x.Id == NewPOItemDB.PurchasePoid).FirstOrDefault();

                        // Check if all Qty is recived 
                        var NewPOItemListDB = POItemListDB.Where(x => x.PurchasePoid == NewPOItemDB.PurchasePoid).AsQueryable();
                        var NewPOItemCountDB = POItemListDB.Count();
                        var NewPOItemCountRecievedDB = POItemListDB.Where(x => x.RecivedQuantity1 >= x.ReqQuantity1).Count();
                        if (NewPOItemCountRecievedDB >= NewPOItemCountDB)
                        {
                            PODB.Status = "Closed";
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            var Res = _unitOfWork.Complete();
            if (Res > 0)
            {
                Result = true;
            }
            return Result;
        }

        public async Task<bool> UpdateInventoryStoreItemFromManageAddingOrder(long AddingOrderItemId,
                                                                              long InventoryItem,
                                                                              long? OldPOID,
                                                                              long? NewPOID,
                                                                              Infrastructure.Entities.PurchasePoitem NewPOItemObjDB)
        {
            bool Result = false;

            var InventoryStoreItemList = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => x.InventoryItemId == InventoryItem &&
                                                                                     x.AddingOrderItemId == AddingOrderItemId &&
                                                                                     x.AddingFromPoid == OldPOID);
            if (InventoryStoreItemList.Count() > 0)
            {
                long? POInvoiceId =  _unitOfWork.PurchasePOInvoices.FindAsync(x => x.Poid == NewPOID).Id;
                foreach (var InvStoreItemDB in InventoryStoreItemList)
                {
                    decimal BalanceQTY = InvStoreItemDB.FinalBalance ?? 0;
                    decimal? POInvoiceTotalPrice = null;
                    decimal? POInvoiceTotalCost = null;
                    int? currencyId = null;
                    decimal? rateToEGP = null;
                    decimal? POInvoiceTotalPriceEGP = null;
                    decimal? POInvoiceTotalCostEGP = null;
                    decimal? remainItemPrice = null; // Not Used Now
                    decimal? remainItemCosetEGP = null;
                    decimal? remainItemCostOtherCU = null;

                    if (NewPOItemObjDB != null)
                    {
                        currencyId = NewPOItemObjDB.CurrencyId ?? 0;
                        rateToEGP = NewPOItemObjDB.RateToEgp ?? 0;
                        POInvoiceTotalPriceEGP = NewPOItemObjDB.ActualUnitPrice ?? 0;
                        POInvoiceTotalCostEGP = NewPOItemObjDB.FinalUnitCost ?? 0;
                        remainItemCosetEGP = BalanceQTY * POInvoiceTotalCostEGP ?? 0;
                        POInvoiceTotalPrice = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalPriceEGP / rateToEGP : 0;
                        POInvoiceTotalCost = rateToEGP != null && rateToEGP != 0 ? POInvoiceTotalCostEGP / rateToEGP : 0;
                        remainItemCostOtherCU = BalanceQTY * (POInvoiceTotalCost ?? 0);
                    }

                    InvStoreItemDB.AddingFromPoid = NewPOID;
                    InvStoreItemDB.PoinvoiceId = POInvoiceId != 0 ? POInvoiceId : null;
                    InvStoreItemDB.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                    InvStoreItemDB.PoinvoiceTotalCost = POInvoiceTotalCost;
                    InvStoreItemDB.CurrencyId = currencyId;
                    InvStoreItemDB.RateToEgp = rateToEGP;
                    InvStoreItemDB.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                    InvStoreItemDB.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                    InvStoreItemDB.RemainItemPrice = remainItemPrice;
                    InvStoreItemDB.RemainItemCosetEgp = remainItemCosetEGP;
                    InvStoreItemDB.RemainItemCostOtherCu = remainItemCostOtherCU;

                }



            }
            var Res =  _unitOfWork.Complete();
            if (Res > 0)
            {
                Result = true;
            }
            return Result;
        }


        public ActivePODDLResponse GetActivePOList(long InventoryItemID, long ToSupplierID)
        {
            ActivePODDLResponse Response = new ActivePODDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<ActivePODDL>();
                if (Response.Result)
                {
                    if (Response.Result)
                    {
                        // Get active POs
                        var PurchasePOItemList = _unitOfWork.VPurchasePoItems.FindAllQueryable(x => x.RecivedQuantity != x.ReqQuantity).AsQueryable();
                        if (InventoryItemID != 0)
                        {
                            PurchasePOItemList = PurchasePOItemList.Where(x => x.InventoryItemId == InventoryItemID).AsQueryable();
                        }
                        if (ToSupplierID != 0)
                        {
                            PurchasePOItemList = PurchasePOItemList.Where(x => x.ToSupplierId == ToSupplierID).AsQueryable();
                        }
                        var IDSPurchasePOItemList = PurchasePOItemList.Select(x => x.PurchasePoid).Distinct().ToList();
                        if (IDSPurchasePOItemList.Count > 0)
                        {
                            var V_PurchasePODB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true && x.Status == "Open" && IDSPurchasePOItemList.Contains(x.Id));
                            var ListDB = V_PurchasePODB.Select(x => x.Id).Distinct().ToList();
                            if (ListDB.Count > 0)
                            {
                                foreach (var ID in ListDB)
                                {
                                    var POdbObj = V_PurchasePODB.Where(x => x.Id == ID).FirstOrDefault();
                                    var DLLObj = new ActivePODDL();
                                    DLLObj.ID = ID;
                                    DLLObj.Name = ID.ToString();
                                    if (POdbObj != null)
                                    {
                                        DLLObj.SupplierID = POdbObj.ToSupplierId;
                                        DLLObj.SupplierName = POdbObj.SupplierName;
                                    }
                                    DDLList.Add(DLLObj);
                                }
                            }
                        }
                    }
                }
                Response.DDLList = DDLList;
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

        public ActivePODDLResponse GetExternalPOList(long InventoryItemID, long ToSupplierID)
        {
            ActivePODDLResponse Response = new ActivePODDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<ActivePODDL>();
                if (Response.Result)
                {

                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item ID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        // Get active POs
                        var PurchasePOItemList = _unitOfWork.VPurchasePoItems.FindAllQueryable(x => x.InventoryItemId == InventoryItemID).AsQueryable();
                        if (ToSupplierID != 0)
                        {
                            PurchasePOItemList = PurchasePOItemList.Where(x => x.ToSupplierId == ToSupplierID).AsQueryable();
                        }
                        var IDSPurchasePOItemList = PurchasePOItemList.Select(x => x.PurchasePoid).Distinct().ToList();
                        if (IDSPurchasePOItemList.Count > 0)
                        {


                            var ListDB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true && IDSPurchasePOItemList.Contains(x.Id)).Select(x => x.Id).Distinct().ToList();
                            var suppliersIDs = PurchasePOItemList.Select(a => a.ToSupplierId).Distinct().ToList();

                            var suppliersData = _unitOfWork.Suppliers.FindAll(a => suppliersIDs.Contains(a.Id)).ToList();
                            if (ListDB.Count > 0)
                            {
                                foreach (var ID in ListDB)
                                {
                                    var DLLObj = new ActivePODDL();
                                    DLLObj.ID = ID;
                                    DLLObj.Name = ID.ToString();
                                    var PurchasePOItemObj = PurchasePOItemList.Where(x => x.PurchasePoid == ID).FirstOrDefault();
                                    if (PurchasePOItemObj != null)
                                    {
                                        DLLObj.SupplierName = suppliersData.Where(a => a.Id == PurchasePOItemObj.ToSupplierId).FirstOrDefault().Name;//Common.GetSupplierName(PurchasePOItemObj.ToSupplierId ?? 0);
                                        DLLObj.RecivedQuantity = PurchasePOItemObj.RecivedQuantity;
                                        DLLObj.ReqQuantity = PurchasePOItemObj.ReqQuantity;
                                        DLLObj.PurchasingUOMShortName = PurchasePOItemObj.PurchasingUomshortName;
                                        DLLObj.RequstionUOMShortName = PurchasePOItemObj.RequstionUomshortName;
                                        DLLObj.POCreationDateDate = PurchasePOItemObj.PocreationDate.ToString();
                                        DLLObj.ExchangeFactor = PurchasePOItemObj.ExchangeFactor;
                                    }
                                    DDLList.Add(DLLObj);
                                }
                            }
                        }
                    }

                }
                Response.DDLList = DDLList;
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

