namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class Groupedtasks
    {
        public string key { get; set; }
        public List<GetProgressForAllTasksDto> tasks { get; set; }
    }
}