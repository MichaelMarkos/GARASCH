namespace NewGaras.Infrastructure.DTO.Medical.DoctorRooms
{
    public class RoomSchedule
    {
        public long Id { get; set; }

        public long DoctorId { get; set; }

        public string DoctorName { get; set; }

        public long SpecialityId { get; set; }

        public string SpecialityName { get; set; }

        public int Capacity { get; set; }

        public decimal ExaminationPrice { get; set; }
        public decimal consultationPrice { get; set; }

        public TimeOnly IntervalFrom { get; set; }
        public TimeOnly IntervalTo { get; set; }
        public int WeekdayId { get; set; }
        public string WeekdayName { get; set; }

        public long PercentageTypeId { get; set; }
        public string PercentageTypeName { get; set; }

        public long DoctorStatusId { get; set; }
        public string DoctorStatusName { get;set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int HoldQuantity { get; set; }

        public int? BranchID { get; set; }
        public string BranchName { get; set; }

    }
}