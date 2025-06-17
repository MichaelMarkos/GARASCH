using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.HR
{
    public class GetBranchesResponse
    {
        bool result;
        List<Error> errors;
        List<BranchData> branchResponseList;



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
        public List<BranchData> BranchResponseList
        {
            get
            {
                return branchResponseList;
            }

            set
            {
                branchResponseList = value;
            }
        }
    }
}
