using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetGroupResponse
    {
        bool result;
        List<Error> errors;
        List<GroupData> groupDataList;



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
        public List<GroupData> GroupDataList
        {
            get
            {
                return groupDataList;
            }

            set
            {
                groupDataList = value;
            }
        }

    }
}
