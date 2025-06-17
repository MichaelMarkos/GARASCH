using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.InventoryStoreReports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IInventoryStoreReportsService
    {
        public Task<GetInventoryStoreReportResponse> GetInventoryStoreReportList();
        public Task<GetInventoryStoreReportItemResponse> GetInventoryStoreReportItemList(long ReportID);
        public Task<BaseResponseWithID> AddInventoryStoreReport(AddInventoryStoreReportRequest Request, long UserID, string CompName);
        public Task<BaseResponse> UpdateInventoryStoreReportItem(UpdateInventoryStoreReportItemRequest Request, long UserID);
        public Task<BaseResponse> UpdateReportToInvStoreItem(UpdateInventoryStoreReportItemRequest Request, long UserID);
        public Task<BaseResponse> UpdateInventoryStoreReportOneItem(UpdateInventoryStoreReportOneItemRequest Request, long UserID);
    }
}
