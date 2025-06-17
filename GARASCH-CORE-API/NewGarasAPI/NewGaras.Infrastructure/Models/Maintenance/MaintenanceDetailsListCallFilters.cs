using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class MaintenanceDetailsListCallFilters
    {
        [FromHeader]
        public DateTime? FromDate { get; set; }
        [FromHeader]
        public DateTime? ToDate { get; set; }
        [FromHeader]
        public long ClientID { get; set; } = 0;
        [FromHeader]
        public long CategoryID { get; set; } = 0;
        [FromHeader]
        public long AreaID { get; set; } = 0;
        [FromHeader]
        public string AreaIdsString { get; set; } = "";
        [FromHeader]
        public decimal? Latitude { get; set; }
        [FromHeader]
        public decimal? Longitude { get; set; }
        [FromHeader]
        public decimal Radius { get; set; } = 0;
        [FromHeader] 
        public int WithInDayNo { get; set; } = 0;
        [FromHeader] 
        public string ContractType { get; set; } = null;
        [FromHeader]
        public string ContractStatus { get; set; } = null;
        [FromHeader] 
        public int WeekNum { get; set; } = 0;
        [FromHeader] 
        public string MaintenanceType { get; set; } = null;
        [FromHeader] 
        public string ProductBrand { get; set; } = null;
        [FromHeader] 
        public string ProductFabricator { get; set; } = null;
        [FromHeader] 
        public string SearchKey { get; set; } = null;
        [FromHeader] 
        public int CurrentPage { get; set; } = 1;
        [FromHeader] 
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
