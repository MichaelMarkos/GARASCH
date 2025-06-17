using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectPayment
{
    public class GetprojectPaymentTermsFilters
    {
        [FromHeader]
        public long? ProjectID { get; set; }
        [FromHeader]
        public string DueDateFrom { get; set; }
        [FromHeader]
        public string DueDateTo {  get; set; }
        [FromHeader]
        public long? NumGreaterThanRemain { get; set; }
        [FromHeader]
        public bool? IsCollected { get; set; }
        [FromHeader]
        public int CurrentPage { get; set; } = 1;
        [FromHeader]
        public int NumberOfItemsPerPage { get; set; } = 10;
    }
}
