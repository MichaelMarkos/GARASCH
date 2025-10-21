

namespace NewGaras.Infrastructure.DTO.LMS
{
    public class CompetitionUserUpdateDTO
    {
        public string? EnrollmentStatus { get; set; }
        public string? DelayOrWithdrawalStatus { get; set; }
        public string? ReasonForDelayOrWithdrawal { get; set; }
        public DateTime? DateOfDelayOrWithdrawalRequest { get; set; }


        public string? CreationByDelayOrWithdrawal { get; set; }

        public DateTime? CreationDateDelayOrWithdrawal { get; set; }
    }
}
