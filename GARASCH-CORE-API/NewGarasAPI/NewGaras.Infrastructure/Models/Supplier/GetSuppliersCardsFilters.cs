using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Supplier
{
    public class GetSuppliersCardsFilters
    {
        [FromHeader]
        public string Phone {  get; set; }
        [FromHeader]
        public string Mobile { get; set; }
        [FromHeader]
        public string Fax { get; set; }
        [FromHeader]
        public string SupplierName { get; set; }
        [FromHeader]
        public DateTime? RegistrationDateFrom { get; set; }
        [FromHeader]
        public DateTime? RegistrationDateTo { get; set; }
        [FromHeader]
        public int SpecialityId { get; set; }
        [FromHeader]
        public long AreaId { get; set; }
        [FromHeader]
        public int? CountryId { get; set; }
        [FromHeader]
        public int? GovernorateId { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
