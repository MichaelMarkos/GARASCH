using System.Runtime.Serialization;

namespace NewGarasAPI.Models.HR
{
    public class AddEmployeesAttendanceRequest
    {
        List<AddAttendanceData> attendanceData;

        [DataMember]
        public List<AddAttendanceData> AttendanceData
        {
            get
            {
                return attendanceData;
            }

            set
            {
                attendanceData = value;
            }
        }
    }
}
