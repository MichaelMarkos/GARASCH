namespace NewGaras.Infrastructure.Models.Attendance
{
    public class PayslipLeaveModel
    {
        public string LeaveName { get; set; }

        public decimal Balance { get; set; }

        public decimal Used {  get; set; }

        public decimal Remain { get; set; }
    }
}