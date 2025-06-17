using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Branch;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;

//using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IDepartmentService _departmentService;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        public BranchService(IUnitOfWork unitOfWork, IMapper mapper, IDepartmentService departmentService, ITaskMangerProjectService taskMangerProjectService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _departmentService = departmentService;
            _taskMangerProjectService = taskMangerProjectService;
        }
        public BaseResponseWithId<long> AddBranch(AddBranchDto branchDto, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (string.IsNullOrEmpty(branchDto.Name))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Name Of Branch Is Required";
                response.Errors.Add(error);
                return response;
            }
            var repetedBranchName = _unitOfWork.Branches.FindAll(a => a.Name == branchDto.Name).FirstOrDefault();
            if (repetedBranchName != null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Name Of Branch Is Already exsists";
                response.Errors.Add(error);
                return response;
            }
            if((branchDto.Longitude != null && branchDto.Latitude == null )  || (branchDto.Longitude == null && branchDto.Latitude != null))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Both of Longitude and Latitude must be entered ";
                response.Errors.Add(error);
                return response;
            }

            var branch = _mapper.Map<Branch>(branchDto);
            if (branch != null)
            {
                if (branch.IsMain == true)
                {
                    var testBranch = _unitOfWork.Branches.FindAll(x => x.IsMain == true).FirstOrDefault();
                    if (testBranch != null)
                    {
                        testBranch.IsMain = false;
                        _unitOfWork.Complete();
                    }
                }
                branch.CreationDate = DateTime.Now;
                branch.ModifiedDate = DateTime.Now;
                branch.CreatedBy = creator;
                branch.ModifiedBy = creator;
                branch.Active=true;
                var AddedBranch = _unitOfWork.Branches.Add(branch);
                _unitOfWork.Complete();
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    var newDay = new WeekDay()
                    {
                        Day = day.ToString(),
                        IsWeekEnd = false,
                        BranchId = AddedBranch.Id,
                    };
                    _unitOfWork.WeekDays.Add(newDay);
                    _unitOfWork.Complete();
                }
                response.ID = AddedBranch.Id;
            }
            return response;
        }

        public BaseResponseWithId<long> EditBranch(AddBranchDto branchDto, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;
            if (string.IsNullOrEmpty(branchDto.Name))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Name Of Branch Is Required";
                response.Errors.Add(error);
                return response;
            }

            if ((branchDto.Longitude != null && branchDto.Latitude == null) || (branchDto.Longitude == null && branchDto.Latitude != null))
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "Both of Longitude and Latitude must be entered ";
                response.Errors.Add(error);
                return response;
            }

            if (branchDto.Id!=0 && branchDto.Id!=null) 
            {
                var branch = _unitOfWork.Branches.GetById((int)branchDto.Id);
                if (branch != null)
                {
                    _mapper.Map<AddBranchDto, Branch>(branchDto, branch);
                    var branches = _unitOfWork.Branches.FindAll(a=>a.Id!=branch.Id).ToList();
                    if (branches != null && branches.Count>0)
                    {
                        if (branches.Select(a => a.Name.Trim().ToLower()).Contains(branch.Name.Trim().ToLower()))
                        {
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err103";
                            error.ErrorMSG = "this Branch Name is duplicated";
                            response.Errors.Add(error);
                            return response;
                        }
                    }
                    if (branch.IsMain == true)
                    {
                        var testBranch = _unitOfWork.Branches.FindAll(x => x.IsMain == true).FirstOrDefault();
                        if (testBranch != null)
                        {
                            testBranch.IsMain = false;
                            _unitOfWork.Complete();
                        }
                    }
                    branch.ModifiedDate = DateTime.Now;
                    branch.ModifiedBy = creator;
                    var EditedBranch = _unitOfWork.Branches.Update(branch);
                    _unitOfWork.Complete();
                    response.ID = EditedBranch.Id;
                }
            }
            return response;
        }

        public BaseResponseWithData<List<GetBranchDto>> GetAllBranches()
        {
            BaseResponseWithData<List<GetBranchDto>> response = new BaseResponseWithData<List<GetBranchDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            List<GetBranchDto> branches = new List<GetBranchDto>();

            var dbBranches = _unitOfWork.Branches.FindAll(criteria:a=>a.Archive!=true,includes: new[] { "Governorate", "Area","Country"}).ToList();

            branches = _mapper.Map<List<GetBranchDto>>(dbBranches);
            branches = branches.Select(a => { a.DepartmentNum = _unitOfWork.Departments.FindAll(x => x.BranchId == a.Id).Count(); return a; }).ToList();
            if(branches.Count > 0)
            {
                response.Data = branches;
            }
            return response;
        }

        public BaseResponseWithData<GetBranchDto> GetBranch(int BranchId)
        {
            BaseResponseWithData<GetBranchDto> response = new BaseResponseWithData<GetBranchDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            var branch = new GetBranchDto();

            var dbBranch = _unitOfWork.Branches.FindAll(criteria: a => a.Id==BranchId, includes: new[] { "Governorate", "Area", "Country" }).FirstOrDefault();
            
            if(dbBranch == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "Branch Is not Found";
                response.Errors.Add(error);
                return response;
            }
            branch = _mapper.Map<GetBranchDto>(dbBranch);
            response.Data = branch;
            return response;
        }

        public BaseResponseWithId<long> DeleteBranch(int BranchId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;

            if(BranchId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "please send a valid branch Id";
                response.Errors.Add(error);
                return response;
            }
            var Branch = _unitOfWork.Branches.GetById(BranchId);
            if (Branch == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch Not Found";
                response.Errors.Add(error);
                return response;
            }
            var users = _unitOfWork.HrUsers.FindAll(x => x.BranchId == BranchId).ToList();
            /*if(users != null && users.Count > 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "there are users associated with this branch";
                response.Errors.Add(error);
                return response;
            }*/
            var departments = _unitOfWork.Departments.FindAll(a=>a.BranchId==BranchId).ToList();
            /*if(departments != null && departments.Count > 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err104";
                error.ErrorMSG = "there are departments associated with this branch";
                response.Errors.Add(error);
                return response;
            }*/
            var attendances = _unitOfWork.Attendances.FindAll(a => a.BranchId == BranchId).ToList();
            var workingHours = _unitOfWork.WorkingHoursTrackings.FindAll(a=>a.BranchId== BranchId).ToList();
            /*if(attendances.Count>0 || workingHours.Count>0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err105";
                error.ErrorMSG = "there's attendances associated with this branch";
                response.Errors.Add(error);
                return response;
            }*/
            var rates = _unitOfWork.OverTimeAndDeductionRates.FindAll(a=>a.BranchId==BranchId).ToList();
            var settings = _unitOfWork.BranchSetting.FindAll(a=>a.BranchId==BranchId).ToList();
            var shifts = _unitOfWork.BranchSchedules.FindAll(a => a.BranchId == BranchId).ToList();

            users.ForEach(a => { _unitOfWork.HrUsers.Delete(a); _unitOfWork.Complete(); });
            departments.ForEach(a => { _unitOfWork.Departments.Delete(a); _unitOfWork.Complete(); });
            workingHours.ForEach(a => { _unitOfWork.WorkingHoursTrackings.Delete(a); _unitOfWork.Complete(); });
            attendances.ForEach(a => { _unitOfWork.Attendances.Delete(a); _unitOfWork.Complete(); });
            rates.ForEach(a => { _unitOfWork.OverTimeAndDeductionRates.Delete(a); _unitOfWork.Complete(); });
            settings.ForEach(a => { _unitOfWork.BranchSetting.Delete(a); _unitOfWork.Complete(); });
            shifts.ForEach(a => { _unitOfWork.BranchSchedules.Delete(a); _unitOfWork.Complete(); });

            _unitOfWork.Branches.Delete(Branch);
            _unitOfWork.Complete();

            return response;
        }

        public BaseResponseWithId<long> DeleteBranchByInclude(int BranchId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;

            if (BranchId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "please send a valid branch Id";
                response.Errors.Add(error);
                return response;
            }
            var Branch = _unitOfWork.Branches.FindAll(a=>a.Id == BranchId, new[] { "WorkingHourseTrackings" }).FirstOrDefault();
            if (Branch == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch Not Found";
                response.Errors.Add(error);
                return response;
            }
            var departments = _unitOfWork.Departments.FindAll(a => a.BranchId == Branch.Id).ToList();
            if (departments.Count > 0)
            {
                for(int i=0; i < departments.Count; i++)
                {
                    _departmentService.DeleteDepartment(departments[i].Id);
                }
            }

            var projects = _unitOfWork.Projects.FindAll(a => a.BranchId == BranchId).ToList();
            if (projects.Count > 0)
            {
                for (int i = 0; i < projects.Count; i++)
                {
                    _taskMangerProjectService.DeleteProject(projects[i].Id);
                }
            }
            _unitOfWork.Branches.Delete(Branch);
            _unitOfWork.Complete();

            return response;

        }


        public BaseResponseWithId<long> ArchiveBranch(int BranchId,bool Archive,long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.ID = 0;

            if (BranchId == 0)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "please send a valid branch Id";
                response.Errors.Add(error);
                return response;
            }
            var Branch = _unitOfWork.Branches.Find(a => a.Id == BranchId);
            if (Branch == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err102";
                error.ErrorMSG = "Branch Not Found";
                response.Errors.Add(error);
                return response;
            }
            Branch.Archive = Archive;
            Branch.ModifiedBy=creator;
            Branch.ModifiedDate = DateTime.Now;
            _unitOfWork.Branches.Update(Branch);
            _unitOfWork.Complete();

            return response;

        }

        public async Task<SelectDDLResponse> GetBranchesList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = (await _unitOfWork.Branches.FindAllAsync(x => x.Active == true)).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var branch in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = branch.Id;
                            DLLObj.Name = branch.Name;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }


    }
}
