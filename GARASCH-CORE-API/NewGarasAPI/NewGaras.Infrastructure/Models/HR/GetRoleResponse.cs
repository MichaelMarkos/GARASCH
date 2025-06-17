using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.HR
{
    public class GetRoleResponse
    {
        bool result;
        List<Error> errors;
        List<RoleData> roleDataList;



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
        public List<RoleData> RoleDataList
        {
            get
            {
                return roleDataList;
            }

            set
            {
                roleDataList = value;
            }
        }
    }
}
