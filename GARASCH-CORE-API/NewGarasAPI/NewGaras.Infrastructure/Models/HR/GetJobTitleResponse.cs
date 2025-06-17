using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.HR
{
    public class GetJobTitleResponse
    {
        bool result;
        List<Error> errors;
        List<JobTitleData> jobTitleList;



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
        public List<JobTitleData> JobTitleList
        {
            get
            {
                return jobTitleList;
            }

            set
            {
                jobTitleList = value;
            }
        }
    }
}
