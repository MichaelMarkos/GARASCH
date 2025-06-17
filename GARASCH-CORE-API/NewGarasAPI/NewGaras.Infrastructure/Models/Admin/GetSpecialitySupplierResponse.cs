using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetSpecialitySupplierResponse
    {
        bool result;
        List<Error> errors;
        List<SpecialitySupplierResponseDataList> specialitySupplierResponseList;



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
        public List<SpecialitySupplierResponseDataList> SpecialitySupplierResponseList
        {
            get
            {
                return specialitySupplierResponseList;
            }

            set
            {
                specialitySupplierResponseList = value;
            }
        }
    }
}
