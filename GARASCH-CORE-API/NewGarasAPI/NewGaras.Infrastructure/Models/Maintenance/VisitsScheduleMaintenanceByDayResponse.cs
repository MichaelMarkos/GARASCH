using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Models.Maintenance
{
    public class VisitsScheduleMaintenanceByDayResponse
    {
        public int NUmOfVisits { get; set; }
        public int NumOFAssignedToVisits { get; set; }
        public int NumOFNotAssignedToVisits { get; set; }
        public int NumOfFinishedVisits { get; set; }
        public int NumOfNotFinishedVisits { get; set; }
        List<MainDayVM> mainDayList;
        bool result;
        List<Error> errors;
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }

        [DataMember]
        public List<Error> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }

        [DataMember]
        public List<MainDayVM> MainDayList
        {
            get
            {
                return mainDayList;
            }

            set
            {
                mainDayList = value;
            }
        }

    }
}
