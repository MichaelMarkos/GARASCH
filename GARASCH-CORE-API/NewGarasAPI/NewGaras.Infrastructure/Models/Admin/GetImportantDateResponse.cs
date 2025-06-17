using NewGarasAPI.Models.Common;
using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetImportantDateResponse
    {
        bool result;
        List<Error> errors;
        List<ImportantDateModel> importantDateList;




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
        public List<ImportantDateModel> ImportantDateList
        {
            get
            {
                return importantDateList;
            }

            set
            {
                importantDateList = value;
            }
        }
    }
}
