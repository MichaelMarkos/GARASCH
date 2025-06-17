using NewGaras.Infrastructure.Entities;

namespace NewGarasAPI.Models.AccountAndFinance
{
    public class DailyJournalEntryPrimaryInfo : DailyJournalEntryView
    {
        public bool? Old {  get; set; }
        public string EntryDateSTR {  get; set; }
        public string CreationDateSTR {  get; set; }

        // List Of Entry Account -From -To
        public List<EntryAccount> EntryAccountList { set; get; }
        // cost Center
        public bool CostCenterStatus { get; set; }
        public CostCenter CostCenter {  get; set; }
        // beneficiary To User
        public beneficiaryToUser BeneficiaryToUser { get; set; }
        // Adjusting Entry
        public List<AdjustingEntryAccount> AdjustingEntryAccountList { set; get; }
        public int? BranchID { get; set; }

    }
}