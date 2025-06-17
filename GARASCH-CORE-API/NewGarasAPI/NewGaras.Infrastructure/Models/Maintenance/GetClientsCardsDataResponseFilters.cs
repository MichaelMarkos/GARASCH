using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class GetClientsCardsDataResponseFilters
    {
        [FromHeader]
        public DateTime? FromDate { get; set; }
        [FromHeader]
        public DateTime? ToDate { get; set; }
        [FromHeader]
        public long ClientID { get; set; }
        [FromHeader]
        public long CategoryID { get; set; }
        [FromHeader]
        public long GovernorateId { get; set; }
        [FromHeader]
        public long CountryId { get; set; }
        [FromHeader]
        public long AreaId { get; set; }
        [FromHeader]
        public string ContractType { get; set; }
        [FromHeader]
        public int WeekNum { get; set; }
        [FromHeader]
        public string MaintenanceType { get; set; }
        [FromHeader]
        public string ProductBrand { get; set; }
        [FromHeader]
        public string ProductFabricator { get; set; }
        [FromHeader]
        public string SearchKey { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
