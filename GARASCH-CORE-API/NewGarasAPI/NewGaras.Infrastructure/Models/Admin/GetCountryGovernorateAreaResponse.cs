using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetCountryGovernorateAreaResponse
    {
        bool result;
        List<Error> errors;
        List<TreeViewCountr> getCountryGovernorateAreaResponseList;



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
        public List<TreeViewCountr> GetCountryGovernorateAreaResponseList
        {
            get
            {
                return getCountryGovernorateAreaResponseList;
            }

            set
            {
                getCountryGovernorateAreaResponseList = value;
            }
        }
    }
}
