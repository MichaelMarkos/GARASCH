using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Admin
{
    public class GetTablesCloumnsResponse
    {
        bool result;
        List<Error> errors;
        List<GetDBTablesColumnsData> getDBColumnsNames;



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
        public List<GetDBTablesColumnsData> GetDBColumnsNames
        {
            get
            {
                return getDBColumnsNames;
            }

            set
            {
                getDBColumnsNames = value;
            }
        }
    }
}
