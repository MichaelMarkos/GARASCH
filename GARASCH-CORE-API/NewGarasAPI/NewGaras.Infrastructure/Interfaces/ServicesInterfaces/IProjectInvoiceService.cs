using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DTO.ProjectInvoiceCollected;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.ProjectInvoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IProjectInvoiceService
    {
        public BaseResponseWithId<long> AddNewProjectInvoice([FromBody] AddProjectInvoiceModel invoiceModel, long creator, string CompName);
        public BaseResponseWithId<long> UpdateProjectInvoiceItems([FromBody] UpdateProjectInvoiceItemsModel invoiceItemList, long creator);

        public BaseResponseWithData<List<GetProjectInvoiceModel>> GetProjectInvoices([FromHeader] long? ProjectId);
        public BaseResponseWithDataAndHeader<GetProjectInvoiceItemsDataModel> GetProjectInvoiceItems([FromHeader] long InvoiceId, [FromHeader] int page = 1, [FromHeader] int size = 10);
        public BaseResponseWithId<long> AddInvoiceItem([FromBody] AddProjectInvoiceItemModel InvoiceItem, long creator);
        public BaseResponseWithId<long> AddProjectInvoiceCollected(AddProjectInvoiceCollectedDto Dto, long creator, string CompName);
        public BaseResponseWithData<List<GetProjectInvoiceCollectedDto>> GetProjectInvoiceCollectedList(long projectInvoiceID);
        public BaseResponseWithId<long> EditProjectInvoiceCollected(EditProjectInvoiceCollectedDto Dto, long creator, string CompName);
        public BaseResponseWithData<GetProjectFinancialDataModel> GetProjectFinancialData([FromHeader] long ProjectId);
        public BaseResponseWithId<long> DeleteProjectInvoiceItem(long InvoiceItemId, long creator);
        public BaseResponseWithId<long> DeleteProjectInvoiceCollected(long Id);
        public BaseResponseWithId<long> DeleteProjectInvoice(long InvoiceId);

        public BaseResponseWithData<string> GetProjectInvoicesReport([FromHeader] long ProjectId, [FromHeader] decimal? Amount, [FromHeader] long? CreatorId, [FromHeader] DateTime? From, [FromHeader] DateTime? To, string CompanyName);
    }
}
