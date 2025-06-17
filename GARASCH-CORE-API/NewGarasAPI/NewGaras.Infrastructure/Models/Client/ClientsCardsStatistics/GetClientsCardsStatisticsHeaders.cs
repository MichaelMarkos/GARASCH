using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client.ClientsCardsStatistics
{
    public class GetClientsCardsStatisticsHeaders
    {
        [FromHeader]
        public string Phone {  get; set; }
        [FromHeader]
        public string Mobile { get; set; }
        [FromHeader]
        public string SupportedBy { get; set; }
        [FromHeader]
        public string Fax { get; set; }
        [FromHeader]
        public string ClientName { get; set; }
        [FromHeader]
        public string PurchasingProductsIdsString { get; set; }
        [FromHeader]
        public string PurchasingProductsStartDate { get; set; }
        [FromHeader]
        public string PurchasingProductsEndDate { get; set; }
        [FromHeader]
        public string RegistrationDateFrom { get; set; }
        [FromHeader]
        public string RegistrationDateTo { get; set; }
        [FromHeader]
        public int SpecialityId { get; set; }
        [FromHeader]
        public long? SalesPersonId { get; set; }
        [FromHeader]
        public int? BranchId { get; set; }
        [FromHeader]
        public bool? IsExpired { get; set; }
        [FromHeader]
        public string DealsDateFrom { get; set; }
        [FromHeader]
        public string DealsDateTo { get; set; }
        [FromHeader]
        public bool? HasRFQ { get; set; }
        [FromHeader]
        public bool? WithOpenOffers { get; set; }
        [FromHeader]
        public bool? WithOpenProjects { get; set; }
        [FromHeader]
        public bool? WithVolume { get; set; }
        [FromHeader]
        public int ApprovalStatus { get; set; } = -1;
        [FromHeader]
        public int ClientClassification { get; set; } = 0;
        [FromHeader]
        public int ExpirationPeriod { get; set; } = 0;
        [FromHeader]
        public long AreaId { get; set; }
        [FromHeader]
        public int? GovernorateId { get; set; }
        [FromHeader]
        public int? CountryId { get; set; }

    }
}
