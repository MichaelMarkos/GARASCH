using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetGenderResponse
    {
        bool result;
        List<Error> errors;
        List<GenderData> genderList;



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
        public List<GenderData> GenderList
        {
            get
            {
                return genderList;
            }

            set
            {
                genderList = value;
            }
        }
    }
}
