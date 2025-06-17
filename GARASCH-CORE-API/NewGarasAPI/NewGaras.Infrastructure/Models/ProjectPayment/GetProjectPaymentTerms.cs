using NewGaras.Infrastructure.DTO.ProjectPayment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.ProjectPayment
{
    public class GetProjectPaymentTerms
    {
        public decimal TotalRemain { get; set; }
        public decimal TotalCollected { get; set; }
        public List<GetProjectPaymentTermsDto> PaymentTermsList { get; set; }
        public PaginationHeader pagination { get; set; }
    }
}
