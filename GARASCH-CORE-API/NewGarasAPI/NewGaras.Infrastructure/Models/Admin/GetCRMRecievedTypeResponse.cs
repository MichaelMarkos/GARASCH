using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetCRMRecievedTypeResponse
    {
        bool result;
        List<Error> errors;
        List<CRMRecievedTypeData> cRMRecievedTypeList;



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
        public List<CRMRecievedTypeData> CRMRecievedTypeList
        {
            get
            {
                return cRMRecievedTypeList;
            }

            set
            {
                cRMRecievedTypeList = value;
            }
        }
    }
}
