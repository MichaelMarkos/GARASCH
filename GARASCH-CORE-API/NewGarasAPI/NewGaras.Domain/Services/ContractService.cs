using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Branch;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class ContractService : IContractService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ContractService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public BaseResponseWithId<long> AddContract(AddContractDto contractDto, long Creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (contractDto.HrUserId == 0 || contractDto.HrUserId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Hr User Id";
                response.Errors.Add(error);
                return response;
            }
            var hruser = _unitOfWork.HrUsers.Find(x => x.Id == contractDto.HrUserId);
            if (hruser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Employee Not Found";
                response.Errors.Add(error);
                return response;
            }
            if (contractDto.FirstReportToID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "At least one reporting to Id is required";
                response.Errors.Add(error);
                return response;
            }
            var FirstReportTo = _unitOfWork.Users.GetById(contractDto.FirstReportToID);
            if (FirstReportTo == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "first reporting to user is not found";
                response.Errors.Add(error);
                return response;
            }
            User SecondReportTo = null;
            if (contractDto.SecondReportToID != 0 && contractDto.SecondReportToID != null)
            {
                SecondReportTo = _unitOfWork.Users.GetById((long)contractDto.SecondReportToID);
                if (SecondReportTo == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err106";
                    error.ErrorMSG = "second reporting to user is not found";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (contractDto.EndDate <= contractDto.StartDate || (contractDto.EndDate.Subtract(contractDto.StartDate).Days / (365.25 / 12)) < 1)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "contract duration should be at least one month";
                response.Errors.Add(error);
                return response;
            }
            var contract = _mapper.Map<ContractDetail>(contractDto);
            if (contract.IsCurrent == true)
            {
                var contractcheck = _unitOfWork.Contracts.FindAll(a => a.HrUserId == contractDto.HrUserId && a.IsCurrent).FirstOrDefault();
                if (contractcheck != null)
                {
                    contractcheck.IsCurrent = false;
                    _unitOfWork.Complete();
                }

            }
            if (contract != null)
            {
                contract.CreatedDate = DateTime.Now;
                contract.ModifiedDate = DateTime.Now;
                contract.CreatedBy = Creator;
                contract.ModifiedBy = Creator;
                var AddedContract = _unitOfWork.Contracts.Add(contract);
                var Res = _unitOfWork.Complete();
                if (Res > 0)
                {
                    var ContractLeaveSettingDB = _unitOfWork.ContractLeaveSetting.FindAll(x => x.Active == true && (x.Archive == false || x.Archive == null));
                    var ContractLeaveEmployeeList = ContractLeaveSettingDB.Select(item => new ContractLeaveEmployee
                    {
                        UserId = contract.UserId != null ? contract.UserId : null,
                        HrUserId = contract.HrUserId,
                        ContractId = contract.Id,
                        ContractLeaveSettingId = item.Id,
                        Active = true,
                        LeaveAllowed = "Allowed",
                        Balance = item.BalancePerMonth != null ? (int)(item.BalancePerMonth * ((contract.EndDate.Month - contract.StartDate.Month) + (12 * (contract.EndDate.Year - contract.StartDate.Year)))) : 0,
                        Used = 0,
                        Remain = item.BalancePerMonth != null ? (int)(item.BalancePerMonth * ((contract.EndDate.Month - contract.StartDate.Month) + (12 * (contract.EndDate.Year - contract.StartDate.Year)))) : 0,
                        BalancePerMonth = item.BalancePerMonth,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedBy = Creator,
                        ModifiedBy = Creator,
                    }).AsEnumerable();
                    ContractLeaveEmployeeList = ContractLeaveEmployeeList.Select(a => { a.Remain = a.Balance - a.Used; return a; }).ToList();
                    _unitOfWork.ContractLeaveEmployees.AddRange(ContractLeaveEmployeeList);
                    _unitOfWork.Complete();

                    var contractReport = new ContractReportTo()
                    {
                        ReportToId = FirstReportTo.Id,
                        ContractId = contract.Id,
                        CreatedBy = Creator,
                        ModifiedBy = Creator,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };
                    _unitOfWork.ContractReportTos.Add(contractReport);
                    _unitOfWork.Complete();

                    if (SecondReportTo != null)
                    {
                        var contractReport2 = new ContractReportTo()
                        {
                            ReportToId = SecondReportTo.Id,
                            ContractId = contract.Id,
                            CreatedBy = Creator,
                            ModifiedBy = Creator,
                            CreationDate = DateTime.Now,
                            ModifiedDate = DateTime.Now
                        };
                        _unitOfWork.ContractReportTos.Add(contractReport2);
                        _unitOfWork.Complete();
                    }
                }
                response.ID = AddedContract.Id;
            }
            return response;
        }
        public BaseResponseWithId<long> EditContract(AddContractDto contractDto, long Creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (contractDto.Id == 0 || contractDto.Id == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "you Must Enter The Contract Id";
                response.Errors.Add(error);
                return response;
            }
            if (contractDto.HrUserId == 0 || contractDto.HrUserId == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "you Must Enter The Hr User Id";
                response.Errors.Add(error);
                return response;
            }
            var hruser = _unitOfWork.HrUsers.Find(x => x.Id == contractDto.HrUserId);
            if (hruser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Employee Not Found";
                response.Errors.Add(error);
                return response;
            }
            var contract = _unitOfWork.Contracts.FindAll(a => a.Id == contractDto.Id).FirstOrDefault();
            if (contract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Contract not found";
                response.Errors.Add(error);
                return response;
            }
            if (contractDto.FirstReportToID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "At least one reporting to Id is required";
                response.Errors.Add(error);
                return response;
            }
            var FirstReportTo = _unitOfWork.Users.GetById(contractDto.FirstReportToID);
            if (FirstReportTo == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "first reporting to user is not found";
                response.Errors.Add(error);
                return response;
            }
            User SecondReportTo = null;
            if (contractDto.SecondReportToID != 0 && contractDto.SecondReportToID != null)
            {
                SecondReportTo = _unitOfWork.Users.GetById((long)contractDto.SecondReportToID);
                if (SecondReportTo == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err106";
                    error.ErrorMSG = "second reporting to user is not found";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (contractDto.EndDate <= contractDto.StartDate || (contractDto.EndDate.Subtract(contractDto.StartDate).Days / (365.25 / 12)) < 1)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "contract duration should be at least one month";
                response.Errors.Add(error);
                return response;
            }
            _mapper.Map<AddContractDto, ContractDetail>(contractDto, contract);

            contract.ModifiedBy = Creator;
            contract.ModifiedDate = DateTime.Now;
            if (contract.IsCurrent == true)
            {
                var contractcheck = _unitOfWork.Contracts.FindAll(a => a.HrUserId == contractDto.HrUserId && a.IsCurrent && a.Id != contract.Id).FirstOrDefault();
                if (contractcheck != null)
                {
                    contractcheck.IsCurrent = false;
                    _unitOfWork.Complete();
                }

            }
            if (contract.IsCurrent == false)
            {
                hruser.IsUser = false;
                _unitOfWork.HrUsers.Update(hruser);
                _unitOfWork.Complete();
            }
            _unitOfWork.Contracts.Update(contract);
            _unitOfWork.Complete();

            var ReportTos = _unitOfWork.ContractReportTos.FindAll(a => a.ContractId == contract.Id);
            if (ReportTos.FirstOrDefault() != null)
            {
                var Report = ReportTos.FirstOrDefault();
                Report.ReportToId = FirstReportTo.Id;
                Report.ModifiedDate = DateTime.Now;
                Report.ModifiedBy = Creator;
                _unitOfWork.ContractReportTos.Update(Report);
                _unitOfWork.Complete();
            }
            else
            {
                if (FirstReportTo != null)
                {
                    var report = new ContractReportTo()
                    {
                        ReportToId = FirstReportTo.Id,
                        ContractId = contract.Id,
                        ModifiedBy = Creator,
                        CreatedBy = Creator,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                    };
                    _unitOfWork.ContractReportTos.Add(report);
                    _unitOfWork.Complete();
                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "first report to is required";
                    response.Errors.Add(error);
                    return response;
                }
            }
            if (SecondReportTo == null && ReportTos.LastOrDefault() != null && ReportTos.Count() > 1)
            {
                _unitOfWork.ContractReportTos.Delete(ReportTos.LastOrDefault());
                _unitOfWork.Complete();
            }
            if (SecondReportTo != null)
            {
                if (ReportTos.Count() > 1 && ReportTos.LastOrDefault() != null)
                {
                    var Report = ReportTos.LastOrDefault();
                    Report.ReportToId = SecondReportTo.Id;
                    Report.ModifiedDate = DateTime.Now;
                    Report.ModifiedBy = Creator;
                    _unitOfWork.ContractReportTos.Update(Report);
                    _unitOfWork.Complete();
                }
                else
                {
                    var report = new ContractReportTo()
                    {
                        ReportToId = SecondReportTo.Id,
                        ContractId = contract.Id,
                        ModifiedBy = Creator,
                        CreatedBy = Creator,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                    };
                    _unitOfWork.ContractReportTos.Add(report);
                    _unitOfWork.Complete();
                }
            }

            response.ID = contract.Id;
            return response;

        }
        public BaseResponseWithData<GetContractDto> GetContract(long HrUserId)
        {
            BaseResponseWithData<GetContractDto> response = new BaseResponseWithData<GetContractDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            var contract = new GetContractDto();
            var hrUser = _unitOfWork.HrUsers.GetById(HrUserId);
            if (hrUser == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "User not Found";
                response.Errors.Add(error);
                return response;
            }
            var dbContract = _unitOfWork.Contracts.Find(x => x.HrUserId == HrUserId && x.IsCurrent == true);
            if (dbContract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "No Current Contract is Found For this User";
                response.Errors.Add(error);
                return response;
            }
            contract = _mapper.Map<GetContractDto>(dbContract);
            response.Data = contract;
            return response;
        }

        public BaseResponseWithId<long> GetContractReportToUser(long UserID)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            //var contract = new GetContractDto();
            var contract = _unitOfWork.Contracts.FindAll(a => a.HrUserId == UserID && a.IsCurrent).FirstOrDefault();
            if (contract == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "User Contract Details not Found";
                response.Errors.Add(error);
                return response;
            }

            if (UserID == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Please Enter UserID";
                response.Errors.Add(error);
                return response;

            }
            if (contract.ReportToId != null) response.ID = contract.ReportToId ?? 0;
            return response;
        }


        public BaseResponse ExtendContracts(ITenantService tenant)
        {
            BaseResponse response = new BaseResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var contracts = _unitOfWork.Contracts.FindAll(a => a.EndDate <= DateTime.Now && a.IsCurrent);
                if (tenant == null)
                {
                    throw new ArgumentNullException(nameof(tenant));
                }
                foreach (var contract in contracts)
                {
                    contract.EndDate = contract.EndDate.AddYears(1);
                    _unitOfWork.Contracts.Update(contract);
                    _unitOfWork.Complete();
                }

                return response;
            } 
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }

        }
    }


}