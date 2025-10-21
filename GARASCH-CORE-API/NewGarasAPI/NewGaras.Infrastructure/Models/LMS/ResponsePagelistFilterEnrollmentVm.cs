

using NewGaras.Infrastructure.DTO.LMS;

namespace NewGaras.Infrastructure.Models.LMS
{
    public class ResponsePagelistFilterEnrollmentVm
    {
        public List<FilterEnrollmentStatusDto> filterlist { get; set; }
        public bool Result { get; set; }
        public List<Error> Errors { get; set; }
        public PaginationHeader PaginationHeader { get; set; }
    }
}
