using Microsoft.EntityFrameworkCore;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.DDL;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces
{
    public interface IDDLService
    {
        public SelectDDLResponse GetLocalGovernorateList();
        public SelectDDLResponse GetInventoryItemPartNoList(string SearchKey);
        public SelectDDLResponse GetPurchasePOList(long? SupplierID);
        public SelectDDLResponse GetMatrialRequestTypeList();
        public OfferItemDDLResponse GetProductOfferItemList(long ProjectFabricationID, string SearchKey, int CurrentPage = 1, int NumberOfItemsPerPage = 10);
        public CountriesGovernoratesAreasDDLs GetCountriesGovernoratesAreasDDLs(string CompName);
        public SellingProductsDDLResponse GetSellingProductsDDL();

        public BaseResponseWithData<List<GetPriorityModel>> GetPriority();
        public SelectDDLResponse GetNationalityDDL();

        public SelectDDLResponse GetMilitaryStatusDDL();

        public SelectDDLResponse GetAttachmentTypeDDL();
    }
}
