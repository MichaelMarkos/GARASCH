using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetTargetOfLast5YearsResponse
    {
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public List<TargetLast5yearsData> TargetLast5yearsList { get; set; }
    }
}
