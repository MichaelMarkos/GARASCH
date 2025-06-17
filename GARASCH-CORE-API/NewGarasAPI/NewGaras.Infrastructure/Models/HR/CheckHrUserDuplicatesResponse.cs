using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGaras.Domain.Models.HR
{
    public class CheckHrUserDuplicatesResponse
    {
        List<List<HrDuplicatesModel>> hrDuplicates;
        bool result;
        List<Error> errors;
        [DataMember]
        public List<List<HrDuplicatesModel>> HrDuplicates
        {
            get { return hrDuplicates; }
            set { hrDuplicates = value; }
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

    }
}
