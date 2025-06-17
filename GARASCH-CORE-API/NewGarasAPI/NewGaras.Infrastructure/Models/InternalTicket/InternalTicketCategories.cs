namespace NewGaras.Infrastructure.Models.InternalTicket
{
    public class InternalTicketCategories
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public decimal Sum { get; set; }

        public int GroupedById { get; set; }

        public List<DoctorsOfCriteria> DoctorsOfDepartmentList { get; set; }
    }

    public class InternalTicketItemCategories
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public decimal Sum { get; set; }

        public long GroupedById { get; set; }

        public List<DoctorsOfCriteria> DoctorsOfDepartmentList { get; set; }
    }
}