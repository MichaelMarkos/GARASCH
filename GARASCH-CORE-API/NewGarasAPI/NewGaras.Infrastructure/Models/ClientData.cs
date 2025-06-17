using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.SalesOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models
{
    public class ClientData : ClientMainData
    {
        public string Type { get; set; }
        public string Email { get; set; }
        public string WebSite { get; set; }
        public string Note { get; set; }
        public int? Rate { get; set; }
        public string FirstContractDate { get; set; }
        public string GroupName { get; set; }
        public string BranchName { get; set; }
        public string Consultant {  get; set; }
        public int FollowUpPeriod { get; set; }
        public string ConsultantType { get; set; }
        public bool SupportedByCompany { get; set; }
        public string SupportedBy {  get; set; }
        public int? BranchID { get; set; }
        public string LastReportDate { get; set; }
        public int? NeedApproval { get; set; }
        public decimal? OpeningBalance { get; set; }
        public string OpeningBalanceType { get; set; }
        public string OpeningBalanceDate { get; set; }
        public int? OpeningBalanceCurrencyId { get; set; }
        public string OpeningBalanceCurrencyName { get; set; }
        public int? DefaultDelivaryAndShippingMethodId { get; set; }
        public string DefaultDelivaryAndShippingMethodName { get; set; }
        public string OtherDelivaryAndShippingMethodName { get; set; }
        public string CommercialRecord {  get; set; }
        public string TaxCard {  get; set; }
        public string ChangeSalesPersonComment { get; set; }
        public string CreatedBy { get; set; }
        public bool? OwnerCoProfile { get; set; }
        public bool? Active {  get; set; }
        public List<GetClientSpeciality> ClientSpecialities { get; set; }
        public List<GetClientAddress> ClientAddresses { get; set; }
        public List<GetClientLandLine> ClientLandLines { get; set; }
        public List<GetClientMobile> ClientMobiles { get; set; }
        public List<GetClientFax> ClientFaxes { get; set; }
        public List<GetClientPaymentTerm> ClientPaymentTerms { get; set; }
        public List<GetClientBankAccount> ClientBankAccounts { get; set; }
    }
}
