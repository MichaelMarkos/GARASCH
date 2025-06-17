using System.Runtime.Serialization;

namespace NewGarasAPI.Models.Account
{
    public class BalancePerMonth
    {
        private int monthNo;
        private string balanceAmount;
        private decimal accumulative;

        [DataMember]
        public int MonthNo { get => monthNo; set => monthNo = value; }
        [DataMember]
        public string BalanceAmount { get => balanceAmount; set => balanceAmount = value; }
        [DataMember]
        public decimal Accumulative { get => accumulative; set => accumulative = value; }
    }
}
