using NewGarasAPI.Models.Common;
using NewGarasAPI.Models.HR;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetTaxResponse
    {
        bool result;
        List<Error> errors;
        List<TaxData> taxList;



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
        public List<TaxData> TaxList
        {
            get
            {
                return taxList;
            }

            set
            {
                taxList = value;
            }
        }
    }
}
