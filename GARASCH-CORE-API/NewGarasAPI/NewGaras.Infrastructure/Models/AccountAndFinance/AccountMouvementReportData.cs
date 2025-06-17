using NewGarasAPI.Models.Purchase;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class AccountMouvementReportData
    {
        bool result;
        List<Error> errors;

        List<PurchasePOItemList> purchasePOItemList;




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
        public List<PurchasePOItemList> PurchasePOItemList
        {
            get
            {
                return purchasePOItemList;
            }

            set
            {
                purchasePOItemList = value;
            }
        }
    }
}
