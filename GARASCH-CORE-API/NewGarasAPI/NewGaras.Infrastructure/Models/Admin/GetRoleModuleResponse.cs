using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetRoleModuleResponse
    {
        bool result;
        List<Error> errors;
        List<RoleModuleData> roleModuleList;



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
        public List<RoleModuleData> RoleModuleList
        {
            get
            {
                return roleModuleList;
            }

            set
            {
                roleModuleList = value;
            }
        }
    }
}
