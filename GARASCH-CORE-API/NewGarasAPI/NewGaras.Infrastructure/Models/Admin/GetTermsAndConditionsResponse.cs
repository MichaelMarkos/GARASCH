using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetTermsAndConditionsResponse
    {
        bool result;
        List<Error> errors;
        List<TermsAndConditionsData> termsAndConditionsList;



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
        public List<TermsAndConditionsData> TermsAndConditionsList
        {
            get
            {
                return termsAndConditionsList;
            }

            set
            {
                termsAndConditionsList = value;
            }
        }

    }
}
