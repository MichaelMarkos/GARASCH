using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryItemCategoryService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public Task<GetInventoryItemCategoryResponse> GetInventoryItemCategory(long userId);

        public BaseResponseWithId<long> AddInventoryItemCategory(InventoryCategoryPerItemData Request, long creator);

        public Task<BaseResponseWithId<long>> EditInventoryCategory(InventoryCategoryPerItemData Request, long creator);

        public BaseResponseWithId<long> DeleteInventoryCategory(InventoryCategoryPerItemData Request);

        public BaseResponseWithData<List<SelectDDL>> GetCategoryTypesDDl();

        public SelectDDLResponse GetInventoryItemCategoryList(long UserId);

        public GetInventoryCategoryStoreItemResponse GetInventoryCategoryStoreItem([FromHeader] long InventoryItemCategoryID);

        public Task<GetInventoryParentCategoryDDLResponse> GetInventoryParentCategoryDDL();

        public Task<GetInventoryParentCategoryDDLResponse> GetInventoryPerItem([FromHeader] long InventoryID);

        public BaseResponseWithId<int> DeleteInventoryItemCategory([FromHeader] int CategoryId, [FromHeader] bool Active);
    }
}
