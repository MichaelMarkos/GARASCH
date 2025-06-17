namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class InternalTicketDepartments
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public decimal Sum { get; set; }

        public int GroupedById { get; set; }

        public List<DoctorsOfCriteria> DoctorsOfDepartmentList { get; set; }
    }
}