using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.InventoryItemOpeningBalance.Responses;
using NewGaras.Infrastructure.Models.POS;
using NewGarasAPI.Models.Inventory.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IPOSService
    {
        public BaseResponseWithId<long> AddPosClosingDay(DateTime date, int storeId, long creator);
        public BaseResponseWithDataAndHeader<List<GetPosClosingDay>> GetAll(int CurrentPage = 1, int NumberOfItemsPerPage = 10);

        public BaseResponseWithId<long> Update(List<UpdatePosClosingDay> dto,long creator);

        public bool AddInventoryStoreItemWithReturn(InventoryStoreItem ParentInventoryStoreItem, long InventoryItemId, decimal QTY, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId);

        public void AddInvntoryStoreItemWithRelease(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemId, int StoreID, decimal QTY, bool IsFIFO, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId);

        public List<InventoryStoreItemIDWithQTY> GetParentReleaseIDWithSettingStore(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemID, int StoreID, int? StoreLocationID, decimal QTY, bool? IsFIFO);
        public BaseResponseWithId<long> AddNewSalesOfferWithReleaseForPOS(AddNewSalesOfferWithReleaseForPOSRequest Request, string companyname, long creator);

        public GetOfferInventoryItemsListForPOSResponse GetOfferInventoryItemsListForPOS(GetOfferInventoryItemsFilters filters, string companyname);

        public BaseResponseWithData<string> ManageInventoryStoreItemPricinigForPOS(AddOneInventoryStoreItemPricing Request, long creator);

        public BaseResponseWithData<string> AddInventoryItemCostNamePOS(InventoryItemPOSCostNameRequest Request);

        public SelectDDLResponse GetInventoryItemCategoryListPOS(GetInventoryItemCategoryListFilters filters);

        public AccountsAndFinanceInventoryItemInfoResponseForPOS GetAccountAndFinanceInventoryStoreItemInfoForPOS(long InventoryItemID, long StoreId, string InventoryItemCode);

        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePOS(AddInventoryItemOpeningBalanceRequest Request, long UserID);
        public BaseResponseWithId<long> AddInventoryItemOpeningBalancePerItem(AddInventoryItemOpeningBalancePerItem Request, long UserID);
    }
}
