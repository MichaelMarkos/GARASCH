using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class CheckClientExistanceFilters
    {
        [FromHeader]
        public string ClientName { get; set; }
        [FromHeader]
        public string ClientEnglishName { get; set; }
        [FromHeader]
        public string ClientMobile {  get; set; }
        [FromHeader]
        public string ClientPhone { get; set; }
        [FromHeader]
        public string ClientFax { get; set; }
        [FromHeader]
        public string ClientEmail { get; set; }
        [FromHeader]
        public string ClientWebsite { get; set; }
        [FromHeader]
        public string ContactPersonMobile { get; set; }
        [FromHeader]
        public bool IsExact {  get; set; } = true;
    }
}
