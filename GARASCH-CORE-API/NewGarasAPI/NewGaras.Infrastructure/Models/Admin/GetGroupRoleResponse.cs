using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetGroupRoleResponse
    {
        bool result;
        List<Error> errors;

        GroupRoleData groupRoleObj;



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
        public GroupRoleData GroupRoleObj
        {
            get
            {
                return groupRoleObj;
            }

            set
            {
                groupRoleObj = value;
            }
        }
    }
}
