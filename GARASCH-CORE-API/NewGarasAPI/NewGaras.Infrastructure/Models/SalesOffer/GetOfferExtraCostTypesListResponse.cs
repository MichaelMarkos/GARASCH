using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetOfferExtraCostTypesListResponse
    {
        public List<SelectDDL> ExtraCostTypesList { get; set; }
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
