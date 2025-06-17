using NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetSalesOfferListDDLForReleaseResponse
    {
        public List<SelectSalesOfferDDLForRelease> Data { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public PaginationHeader paginationHeader { get; set; }
    }
}
