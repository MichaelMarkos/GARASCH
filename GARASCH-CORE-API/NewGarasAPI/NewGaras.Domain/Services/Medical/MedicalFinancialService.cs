using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Medical.MedicalFinance;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services.Medical
{
    public class MedicalFinancialService : IMedicalFinancialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserService _user;
        private HearderVaidatorOutput validation;
        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public MedicalFinancialService(IUnitOfWork unitOfWork, IMapper mapper,IUserService user)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _user = user;
        }


        public BaseResponseWithId<long> AddMedicalOpenBalance(AddOpeningMedicalFinancialDTO dto,long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                if (string.IsNullOrEmpty(dto.Type))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "please add type";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                #region check if user Already have open Balance
                var userHaveOpenBalance = _unitOfWork.MedicalDailyTreasuryBalances.FindAll(a => a.CreatedBy == userID && a.IsOpeningBalance == true).FirstOrDefault();
                if (userHaveOpenBalance != null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "This user has an open balance. Please close it before opening another one.";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                #region check if POS Already been open (الخزنه مفتوحه مع يوسر تاني)
                var POSOpenBalance = _unitOfWork.MedicalDailyTreasuryBalances.FindAll(a => a.PosNumberId == dto.PosNumberId && a.IsOpeningBalance == true).FirstOrDefault();
                if (userHaveOpenBalance != null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "This user has an open balance. Please close it before opening another one.";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                var medicalFinance = new MedicalDailyTreasuryBalance();

                medicalFinance.OpeningBalance = dto.OpeningBalance;
                medicalFinance.IsOpeningBalance = true;
                medicalFinance.CreationDate = egyptDateTime;
                medicalFinance.CreatedBy = validation.userID;
                medicalFinance.ModifiedBy = validation.userID;
                medicalFinance.ClosingDate = egyptDateTime;
                medicalFinance.PosNumberId = dto.PosNumberId;
                medicalFinance.Type = dto.Type;

                if (dto.OpeningBalance > 0)
                {
                    var lastBalance = _unitOfWork.MedicalDailyTreasuryBalances.FindAll(a => a.PosNumberId!=null && a.PosNumberId==dto.PosNumberId).OrderByDescending(a=>a.Id).FirstOrDefault();
                    medicalFinance.ReceivedFrom = lastBalance != null ? lastBalance.CreatedBy : null;
                }

                _unitOfWork.MedicalDailyTreasuryBalances.Add(medicalFinance);
                _unitOfWork.Complete();

                response.ID = medicalFinance.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }


        public BaseResponseWithId<long> AddMedicalClosingBalance(AddClosingMedicalFinancialDTO dTO)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            var dailyBalance = _unitOfWork.MedicalDailyTreasuryBalances.GetById(dTO.ID);
            if (dailyBalance == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No opening Balance with this ID";
                response.Errors.Add(err);
                return response;
            }

            if (dailyBalance.IsOpeningBalance == false)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "The Balance with this ID is already closed";
                response.Errors.Add(err);
                return response;
            }

            try
            {
                var DailyReservationList = _unitOfWork.MedicalReservations.FindAll(a => a.CreatedBy == validation.userID && a.CreationDate.Date == DateTime.Now.Date);

                var totalDailyReservation = DailyReservationList.Sum(b => b.FinalAmount);


                dailyBalance.ClosingBalance = dTO.ClosingBalance;
                dailyBalance.TotalReceipts = dTO.TotalReceipts;
                dailyBalance.IsOpeningBalance = false;
                dailyBalance.ReservationAmount = totalDailyReservation;
                dailyBalance.Difference = dTO.TotalReceipts - totalDailyReservation;

                dailyBalance.ModifiedBy = validation.userID;
                TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);
                dailyBalance.ClosingDate = egyptDateTime;

                
                _unitOfWork.Complete();

                response.ID = dailyBalance.Id;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<List<SelectDDL>> GetPosNumerDDL()
        {
            var response = new BaseResponseWithData<List<SelectDDL>>()
            {
                Errors = new List<Error>(),
                Result = true
            };
            try
            {
                var ddlList = _unitOfWork.PosNumbers.GetAll().Select(a=>new SelectDDL() { ID=a.Id,Name = a.Serial}).ToList();
                response.Data = ddlList;
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<List<MedicalDailyTreasuryBalanceDto>> GetAllMedicalDailyTreasuryBalance(int PosNumberId,long CreatedById, string type,DateTime? From, DateTime?To, bool? IsOpeningBalance)
        {
            var response = new BaseResponseWithData<List<MedicalDailyTreasuryBalanceDto>>()
            {
                Errors = new List<Error>(),
                Result = true
            };
            try
            {
                var medicalFinanceQ = _unitOfWork.MedicalDailyTreasuryBalances.FindAllQueryable(a =>true, includes: new[] { "CreatedByNavigation", "ModifiedByNavigation", "PosNumber" });
                if(PosNumberId != 0)
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.PosNumberId == PosNumberId);
                }
                if (CreatedById != 0)
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.CreatedBy == CreatedById);
                }
                if (From != null)
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.CreationDate.Date >= From.Value.Date);
                }
                if (To != null)
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.CreationDate.Date <= To.Value.Date);
                }
                if(IsOpeningBalance != null)
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.IsOpeningBalance == IsOpeningBalance);
                }
                if (!string.IsNullOrEmpty(type))
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.Type == type);
                }
                var medicalFinanceList = medicalFinanceQ.ToList();
                var medicalFinanceDtoList = _mapper.Map<List<MedicalDailyTreasuryBalanceDto>>(medicalFinanceList);
                response.Data = medicalFinanceDtoList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }


        public BaseResponseWithData<MedicalDailyTreasuryBalanceDto> GetMedicalDailyTreasuryBalance(int PosNumberId, bool IsOpeningBalance, string Type, long? CreatedById)
        {
            var response = new BaseResponseWithData<MedicalDailyTreasuryBalanceDto>()
            {
                Errors = new List<Error>(),
                Result = true
            };
            try
            {
                #region
                if (PosNumberId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Please enter Pos Number";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                var medicalFinanceQ = _unitOfWork.MedicalDailyTreasuryBalances.FindAllQueryable(a => a.PosNumberId == PosNumberId && a.IsOpeningBalance == IsOpeningBalance, includes: new[] { "CreatedByNavigation", "ModifiedByNavigation", "PosNumber" });
                if (CreatedById != null)
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.CreatedBy == CreatedById);
                }
                if (!string.IsNullOrEmpty(Type))
                {
                    medicalFinanceQ = medicalFinanceQ.Where(a => a.Type == Type);
                }
                
                var medicalFinanceList = medicalFinanceQ.OrderBy(a => a.Id).LastOrDefault();
                var medicalFinanceDtoList = _mapper.Map<MedicalDailyTreasuryBalanceDto>(medicalFinanceList);
                response.Data = medicalFinanceDtoList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        
    }
}
