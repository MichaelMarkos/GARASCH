namespace NewGarasAPI.Models.AccountAndFinance
{
    public class GetAccountCategory
    {
        long id;
        string name;
        List<GetAdvanciedType> advanciedTypeLList;

        [DataMember]
        public long Id
        {
            set { id = value; }
            get { return id; }
        }
        [DataMember]
        public string Name
        {
            set { name = value; }
            get { return name; }
        }
        [DataMember]
        public List<GetAdvanciedType> AdvanciedTypeLList
        {
            set { advanciedTypeLList = value; }
            get { return advanciedTypeLList; }
        }
    }
}