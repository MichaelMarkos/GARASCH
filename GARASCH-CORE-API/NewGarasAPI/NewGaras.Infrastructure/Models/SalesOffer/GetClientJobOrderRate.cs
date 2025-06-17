using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.SalesOffer
{
    public class GetClientJobOrderRate
    {
        public string ClientName { get; set; }

        public decimal YtdVolume { get; set; }
        public decimal YtdCount { get; set; }
        public decimal AllVolume { get; set; }
        public decimal AllCount { get; set; }
        public string FirstOrderDate { get; set; }
        public string LastOrderDate { get; set; }

        public decimal MonthlyClientRate 
        {
            get { return decimal.Round((YtdCount/DateTime.Now.Month), 2); }
            
        }
        public decimal OrderRate 
        {
            get 
            {
                if (!string.IsNullOrEmpty(LastOrderDate) && !string.IsNullOrEmpty(FirstOrderDate)&&FirstOrderDate!=LastOrderDate)
                {
                    return Decimal.Round( AllCount / ((DateTime.Parse(LastOrderDate) - DateTime.Parse(FirstOrderDate)).Days / (decimal)(365.2425 / 12)),2);
                }
                else
                {
                    return 0;
                }
            }
        }

    }
}
