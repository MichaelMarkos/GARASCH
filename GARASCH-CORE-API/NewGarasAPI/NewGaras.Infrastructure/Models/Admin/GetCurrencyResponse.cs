using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetCurrencyResponse
    {
        bool result;
        List<Error> errors;
        List<CurrencyData> currencyList;



        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }



        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        [DataMember]
        public List<CurrencyData> CurrencyList
        {
            get
            {
                return currencyList;
            }

            set
            {
                currencyList = value;
            }
        }
    }
}
