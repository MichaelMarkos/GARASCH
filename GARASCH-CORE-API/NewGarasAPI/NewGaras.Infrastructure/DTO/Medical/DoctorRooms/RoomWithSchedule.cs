namespace NewGaras.Infrastructure.DTO.Medical.DoctorRooms
{
    public class RoomWithSchedule
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<RoomSchedule> Schedules { get; set; }
    }
}