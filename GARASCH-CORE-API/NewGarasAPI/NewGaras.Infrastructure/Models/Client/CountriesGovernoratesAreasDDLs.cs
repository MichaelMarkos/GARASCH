using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class CountriesGovernoratesAreasDDLs
    {
        public List<SelectDDL> CountriesDDL {  get; set; }
        public List<GovernorateDDL> GoverneratesDDL { get; set; }
        public List<AreaDDL> AreasDDL { get; set; }

        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
    }
}
