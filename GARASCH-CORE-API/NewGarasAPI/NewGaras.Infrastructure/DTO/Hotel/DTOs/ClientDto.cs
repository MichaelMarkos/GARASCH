
namespace NewGaras.Infrastructure.Hotel.DTOs
{
    public class ClientDto
    {
        public long? Id { get; set; }
        public string Name { get; set; } 
        public string Type { get; set; } 
        public string? Email { get; set; }
       // public string? WebSite { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public long SalesPersonId { get; set; }
        //public string? Note { get; set; }
        //public int? Rate { get; set; }
        //public DateTime? FirstContractDate { get; set; }
       // public byte[]? Logo { get; set; }
        //public string? GroupName { get; set; }
        //public string? BranchName { get; set; }
        //public string? Consultant { get; set; }
        public int FollowUpPeriod { get; set; }
        //public string? ConsultantType { get; set; }

        //public bool? SupportedByCompany { get; set; }

        //public string? SupportedBy { get; set; }

        //public bool? HasLogo { get; set; }

        //public int? BranchId { get; set; }

        //public DateTime? LastReportDate { get; set; }


        //public int? NeedApproval { get; set; }

        //public long? ClientSerialCounter { get; set; }

        //public decimal? OpeningBalance { get; set; }

        //public string? OpeningBalanceType { get; set; }

        //public DateTime? OpeningBalanceDate { get; set; }

        //public int? OpeningBalanceCurrencyId { get; set; }

        //public int? DefaultDelivaryAndShippingMethodId { get; set; }

        //public string? OtherDelivaryAndShippingMethodName { get; set; }

        //public string? CommercialRecord { get; set; }

        //public string? TaxCard { get; set; }

        //public bool? OwnerCoProfile { get; set; }

       // public long? ApprovedBy { get; set; }

       // public int? ClientClassificationId { get; set; }

        //public string? ClassificationComment { get; set; }
        public int? NationalityId { get; set; } 

    }
}
