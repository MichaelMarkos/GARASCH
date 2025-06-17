namespace NewGarasAPI.Models.Admin
{
    public class DashboardResponse
    {
        DashboardInfo data;
        bool result;
        List<Error> errors;

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
        public DashboardInfo Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }
    }
}
