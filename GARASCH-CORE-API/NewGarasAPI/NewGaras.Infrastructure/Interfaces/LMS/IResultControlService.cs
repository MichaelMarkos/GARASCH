

namespace NewGaras.Infrastructure.Interfaces.LMS
{
    public interface IResultControlService
    {
        public BaseResponseWithData<decimal> SumStudentSubjectsForYear(long userId , int YearId);

    }
}
