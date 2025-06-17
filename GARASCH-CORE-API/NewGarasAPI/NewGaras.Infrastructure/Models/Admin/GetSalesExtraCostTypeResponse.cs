using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetSalesExtraCostTypeResponse
    {
        bool result;
        List<Error> errors;
        List<SalesExtraCostTypeData> salesExtraCostTypeList;



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
        public List<SalesExtraCostTypeData> SalesExtraCostTypeList
        {
            get
            {
                return salesExtraCostTypeList;
            }

            set
            {
                salesExtraCostTypeList = value;
            }
        }
    }
}
