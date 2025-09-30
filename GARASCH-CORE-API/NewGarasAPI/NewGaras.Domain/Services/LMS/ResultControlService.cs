

using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;

namespace NewGaras.Domain.Services.LMS
{
    public class ResultControlService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ResultControlService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithData<decimal> SumStudentSubjectsForYear(long userId, int YearId)
        {
            BaseResponseWithData<decimal> Response = new BaseResponseWithData<decimal>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SubjectsData = _unitOfWork.ResultControls.FindAll(a => a.HrUserId == userId && a.YearId == YearId).ToList();

                var GPA = SubjectsData.Sum(a => a.Accreditedhours) != 0 ? SubjectsData.Sum(a => a.Gpa * a.Accreditedhours) / SubjectsData.Sum(a => a.Accreditedhours) : 0;

                Response.Data = GPA ?? 0;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
