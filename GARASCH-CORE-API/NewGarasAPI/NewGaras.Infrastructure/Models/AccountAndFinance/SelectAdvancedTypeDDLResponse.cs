namespace NewGarasAPI.Models.AccountAndFinance
{
    public class SelectAdvancedTypeDDLResponse
    {
        List<SelectAdvancedTypeDDL> dDLList;
        bool result;
        List<Error> errors;


        [DataMember]
        public List<SelectAdvancedTypeDDL> DDLList
        {
            get
            {
                return dDLList;
            }

            set
            {
                dDLList = value;
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
    }
}
