using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Client
{
    public class AddNewClientScreenData
    {
        public List<SelectDDL> CountriesDDL {  get; set; }
        public List<string> SupportedByDDL { get; set; }
        public List<GovernorateDDL> GovernorateDDL { get; set; }
        public List<AreaDDL> AreasDDL { get; set; }
        public List<SelectDDL> SpecialitiesDDL { get; set; }
        public List<SelectDDL> JobTitlesDDL { get; set; }
        public List<UserDDL> SalesPersonsDDL { get; set; }
        public List<SelectDDL> CurrenciesDDL { get; set; }
        public List<SelectDDL> DeliveryAndShippingMethodsDDL { get; set; }
        public bool Result {  get; set; }
        public List<Error> Errors { get; set; }
    }
}
