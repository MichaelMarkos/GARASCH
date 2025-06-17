using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetDepartmentResponse
    {
        bool result;
        List<Error> errors;
        List<DepartmentData> departmentResponseList;



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
        public List<DepartmentData> DepartmentResponseList
        {
            get
            {
                return departmentResponseList;
            }

            set
            {
                departmentResponseList = value;
            }
        }
    }
}
