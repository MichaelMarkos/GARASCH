namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class DoctorsOfCriteria
    {
        public long DoctorId { get; set; }
        public string DoctorName { get; set; }

        public string DoctorImg {  get; set; }

        public decimal TotalSum { get; set; }

        public int TicketsCount { get; set; }
        public int PatientsCount { get; set; }
    }
}