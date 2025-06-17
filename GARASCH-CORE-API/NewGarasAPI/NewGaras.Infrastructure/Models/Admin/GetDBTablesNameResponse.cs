using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetDBTablesNameResponse
    {
        bool result;
        List<Error> errors;
        List<string> getDBTablesNameList;



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
        public List<string> GetDBTablesNameList
        {
            get
            {
                return getDBTablesNameList;
            }

            set
            {
                getDBTablesNameList = value;
            }
        }
    }
}
