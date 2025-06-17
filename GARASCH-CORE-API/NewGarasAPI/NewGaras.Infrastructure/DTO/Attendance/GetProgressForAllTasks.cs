using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.DTO.Attendance
{
    public class GetProgressForAllTasks
    {
        public string CurrencyName { get; set; }
        public List<GetProgressForAllTasksDto> tasks {  get; set; }

        //public List<IGrouping<string,GetProgressForAllTasksDto>> Groupedtasks { get; set; }
        public List<Groupedtasks> groupedtasks { get; set; }
        public string SavedPath { get; set; }

    }
}
