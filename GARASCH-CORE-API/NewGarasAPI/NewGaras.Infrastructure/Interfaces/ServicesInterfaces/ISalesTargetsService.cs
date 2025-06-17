using NewGaras.Infrastructure.Models.SalesTargets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface ISalesTargetsService
    {
        public Task<BaseResponseWithID> AddEditSalesTarget(AddSalesTarget Request, long UserID);
        public Task<BaseResponseWithID> AddEditSalesBranchTarget(AddSalesBranchTargetResponse Request, long UserID);
        public Task<BaseResponseWithID> AddEditSalesBranchUserTarget(AddSalesBranchUserTargetResponse Request, long UserID);
        public Task<BaseResponseWithID> AddEditSalesBranchProductTarget(AddSalesBranchProductTargetResponse Request, long UserID);
        public Task<GetSalesTargetListResponse> GetLastSalesTargetList(int? filterYear);
        public Task<GetSalesBranchTargetResponse> GetSalesBranchTargetList(int TargetId);
        public Task<GetSalesBranchUserTargetResponse> GetSalesBranchUserTargetList(int TargetId, int? BranchId);
        public Task<GetSalesBranchProductTargetResponse> GetSalesBranchProductTargetList(int TargetId, int? BranchId);
        public Task<TopSellingAndProfitProductsResponse> GetTopSellingAndProfitProductsList(int? TargetYear, int? StartYear);
        public Task<GetSalesTargetDDLResponse> GetTargetList();
    }
}
