using AutoMapper;
using Azure;
using Microsoft.IdentityModel.Tokens;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Department;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.Team;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGarasAPI.Models.Admin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;

namespace NewGaras.Domain.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHrUserService _userService;
        private readonly IMapper _mapper;
        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper,IHrUserService userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
        }
        public BaseResponseWithId<int> AddDepartmnet(AddDepartmentDto departmentDto,long creator)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();
            _unitOfWork.BeginTransaction();
            try
            {
                var branch = _unitOfWork.Branches.Find(x=>x.Id== departmentDto.BranchId);
                if (branch == null)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Branch not Found";
                    response.Errors.Add(error);
                    return response;
                }
                var department = _mapper.Map<Department>(departmentDto);
                var depCheck = _unitOfWork.Departments.FindAll(a=>  a.BranchId == departmentDto.BranchId && a.Name.Trim().ToLower() == departmentDto.Name.Trim().ToLower()).FirstOrDefault();
                if(depCheck != null)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Department Name Is Duplicated";
                    response.Errors.Add(error);
                    return response;
                }
                department.CreationDate = DateTime.Now;
                department.CreatedBy = creator;
                department.ModifiedDate = DateTime.Now;
                department.ModifiedBy = creator;
                department.Active = true;
                var HrDepartment = _unitOfWork.Departments.Add(department);
                _unitOfWork.Complete();
                var departmentId = HrDepartment.Id;
                if (departmentDto.Teams.Count() > 0)
                {
                    var teamsCheck = departmentDto.Teams.Select(a=>a.Name.Trim().ToLower()).Distinct().ToList();
                    if(teamsCheck.Count()< departmentDto.Teams.Count())
                    {
                        _unitOfWork.RollbackTransaction();
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "Team Name Was Entered Twice";
                        response.Errors.Add(error);
                        return response;
                    }
                    foreach (var item in departmentDto.Teams)
                    {
                        var team = _mapper.Map<Team>(item);
                        var teamcheck = _unitOfWork.Teams.FindAll(a=> a.DepartmentId == departmentId && a.Name.Trim().ToLower()==team.Name.Trim().ToLower()).FirstOrDefault();
                        if(teamcheck != null)
                        {
                            _unitOfWork.RollbackTransaction();
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err103";
                            error.ErrorMSG = "Team Name Is Duplicated";
                            response.Errors.Add(error);
                            return response;
                        }
                        team.DepartmentId = departmentId;
                        team.CreatedDate = DateTime.Now;
                        team.CreatedBy = creator;
                        team.ModifiedDate = DateTime.Now;
                        team.ModifiedBy = creator;
                        team.Active = true;
                        var Team = _unitOfWork.Teams.Add(team);
                        _unitOfWork.Complete();
                        var teamId = Team.Id;
                        if (item.HrUsersIds.Count() > 0)
                        {
                            var hruserteams = item.HrUsersIds.Select(userId => new UserTeam { TeamId = teamId, HrUserId = userId, CreatedDate = DateTime.Now, CreatedBy = creator, ModifiedDate = DateTime.Now, ModifiedBy = creator });
                            _unitOfWork.UserTeams.AddRange(hruserteams);


                        }
                    }
                }
                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
                response.ID = departmentId;
                return response;
            }
            catch (Exception ex) 
            {
                _unitOfWork.RollbackTransaction();
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<GetDepartmentDto> GetDepartment(long DepartmentId)
        {
            BaseResponseWithData<GetDepartmentDto> response = new BaseResponseWithData<GetDepartmentDto>();
            response.Result = true;
            response.Errors = new List<Error>();
            if (DepartmentId != 0 && DepartmentId != null)
            {
                var department = _unitOfWork.Departments.Find(a => a.Id == DepartmentId);
                if (DepartmentId != null)
                {
                    var dep = _mapper.Map<GetDepartmentDto>(department);
                    var dbTeams = _unitOfWork.Teams.FindAll(a => a.DepartmentId == dep.Id);
                    var teams = _mapper.Map<List<GetTeamDto>>(dbTeams);
                    var dbTeamUsers = 
                    teams.Select(a => {
                        a.HrUsers = new List<GetHrTeamUsersDto>();
                        var dbTeamUsers =_unitOfWork.UserTeams.FindAll(x=>x.TeamId==a.Id).Select(x=>x.HrUserId).ToList();
                        var hrs = _unitOfWork.HrUsers.FindAll(x => dbTeamUsers.Contains(x.Id)).ToList();
                        var hrlist = _mapper.Map<List<GetHrTeamUsersDto>>(hrs);
                        a.HrUsers = hrlist; 
                        return a; 
                    }).ToList();
                    dep.Teams = dbTeamUsers;
                    response.Data = dep;
                    return response;
                    
                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Department not found";
                    response.Errors.Add(error);
                    return response;
                }
            }
            else
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Department Id not Correct";
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithId<int> EditDepartmnet(AddDepartmentDto departmentDto, long creator)
        {
            BaseResponseWithId<int> response = new BaseResponseWithId<int>();
            response.Result = true;
            response.Errors = new List<Error>();
            _unitOfWork.BeginTransaction();
            try
            {
                var branch = _unitOfWork.Branches.Find(x => x.Id == departmentDto.BranchId);
                if (branch == null)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Branch not Found";
                    response.Errors.Add(error);
                    return response;
                }
                if (departmentDto.Id == null || departmentDto.Id == 0)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Department Id is missing";
                    response.Errors.Add(error);
                    return response;
                }
                var department = _unitOfWork.Departments.FindAll(a => a.Id == departmentDto.Id).FirstOrDefault();
                if (department == null)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Department is not found";
                    response.Errors.Add(error);
                    return response;
                }
                _mapper.Map<AddDepartmentDto, Department>(departmentDto, department);
                department.ModifiedBy = creator;
                department.ModifiedDate = DateTime.Now;
                _unitOfWork.Departments.Update(department);
                _unitOfWork.Complete();

                if (departmentDto.Teams != null)
                {
                    var teamsDb = _unitOfWork.Teams.FindAll(a => a.DepartmentId == departmentDto.Id);
                    var teamsCheck = departmentDto.Teams.Select(a => a.Name.Trim().ToLower()).Distinct().ToList();
                    if (teamsCheck.Count() < departmentDto.Teams.Count())
                    {
                        _unitOfWork.RollbackTransaction();
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err104";
                        error.ErrorMSG = "Team Name Was Entered Twice";
                        response.Errors.Add(error);
                        return response;
                    }
                    foreach (var team in departmentDto.Teams)
                    {
                        var teamcheck = _unitOfWork.Teams.FindAll(a => a.DepartmentId == departmentDto.Id && a.Name.Trim().ToLower() == team.Name.Trim().ToLower() && team.Id!=a.Id).FirstOrDefault();
                        if (teamcheck != null)
                        {
                            _unitOfWork.RollbackTransaction();
                            response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err105";
                            error.ErrorMSG = "Team Name Is Duplicated";
                            response.Errors.Add(error);
                            return response;
                        }
                        if (team.Id != null && team.Id != 0)
                        {
                            var teamdb = _unitOfWork.Teams.FindAll(a => a.Id == team.Id).FirstOrDefault();
                            if (teamdb == null)
                            {
                                _unitOfWork.RollbackTransaction();
                                response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err106";
                                error.ErrorMSG = "Team Id Is not right";
                                response.Errors.Add(error);
                                return response;
                            }
                            _mapper.Map<TeamDto, Team>(team, teamdb);
                            teamdb.ModifiedBy = creator;
                            teamdb.ModifiedDate = DateTime.Now;
                            _unitOfWork.Complete();
                        }
                        else
                        {
                            var teamdb = _mapper.Map<Team>(team);
                            teamdb.DepartmentId = (int)departmentDto.Id;
                            teamdb.CreatedDate = DateTime.Now;
                            teamdb.CreatedBy = creator;
                            teamdb.ModifiedDate = DateTime.Now;
                            teamdb.ModifiedBy = creator;
                            teamdb.Active = true;
                            var Team = _unitOfWork.Teams.Add(teamdb);
                            _unitOfWork.Complete();

                        }
                    }
                        var unselectedTeams = teamsDb.Where(x => !departmentDto.Teams.Where(a => a.Id != null).Select(a => a.Id).Contains(x.Id));
                        foreach(var t in unselectedTeams)
                        {
                            
                            //if (_unitOfWork.UserTeams.GetAll().Select(a => a.TeamId).Contains(t.Id) || _unitOfWork.HrUsers.GetAll().Select(a=>a.TeamId).Contains(t.Id))
                            //{
                            //_unitOfWork.RollbackTransaction();
                            //response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err107";
                            //    error.ErrorMSG = "Team can't be removed because it has employees, Remove Them First";
                            //    response.Errors.Add(error);
                            //    return response;
                            //}
                            //else
                            //{
                            //    _unitOfWork.Teams.Delete(t);
                            //    _unitOfWork.Complete();
                            //}
                        }
                    
                }

                _unitOfWork.CommitTransaction();
                response.ID = department.Id;
                return response;
            }
            catch (Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithId<long> EditTeam(TeamDto teamDto, long creator)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            _unitOfWork.BeginTransaction();
            try
            {
                var department = _unitOfWork.Departments.Find(x => x.Id == teamDto.DepartmentId);
                if (department == null)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "department not Found";
                    response.Errors.Add(error);
                    return response;
                }
                var Team = _unitOfWork.Teams.FindAll(a=>a.Id==teamDto.Id).FirstOrDefault();
                if (Team == null)
                {
                    _unitOfWork.RollbackTransaction();
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Team Is Not Found";
                    response.Errors.Add(error);
                    return response;
                }
                _mapper.Map<TeamDto, Team>(teamDto, Team);
                Team.ModifiedBy = creator;
                Team.ModifiedDate = DateTime.Now;

                var teamUsersIds = _unitOfWork.UserTeams.FindAll(a=>a.TeamId==Team.Id).Select(a=>a.HrUserId).ToList();

                var addedUsersIds = teamDto.HrUsersIds.Where(a => !teamUsersIds.Contains(a)).ToList();

                var removedUsersIds = teamDto.HrUsersIds.Count>0? teamUsersIds.Where(a=>!teamDto.HrUsersIds.Contains((long)a)).ToList():teamUsersIds;

                var hruserteams = addedUsersIds.Select(a => new UserTeam { TeamId = Team.Id, HrUserId = a, CreatedDate = DateTime.Now, CreatedBy = creator, ModifiedDate = DateTime.Now, ModifiedBy = creator });
                _unitOfWork.UserTeams.AddRange(hruserteams);
                _unitOfWork.Complete();

                var RemovedTeamUsers = _unitOfWork.UserTeams.FindAll(a => removedUsersIds.Contains(a.HrUserId) && a.TeamId==Team.Id).ToList();
                //var users = teamDto.HrUsersIds.Count > 0 ? _unitOfWork.HrUsers.FindAll(a => !teamDto.HrUsersIds.Contains(a.Id) && a.TeamId==teamDto.Id).ToList(): _unitOfWork.HrUsers.FindAll(a => a.TeamId == teamDto.Id).ToList();
               // users.ForEach(a => { a.TeamId = null; _unitOfWork.HrUsers.Update(a); _unitOfWork.Complete(); });
                _unitOfWork.UserTeams.DeleteRange(RemovedTeamUsers);

                _unitOfWork.Complete();
                _unitOfWork.CommitTransaction();
                response.ID = Team.Id;
                return response;


            }
            catch(Exception ex)
            {
                _unitOfWork.RollbackTransaction();
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<List<GetDepartmentDto>> GetBranchDepartments(int BranchId)
        {
            BaseResponseWithData<List<GetDepartmentDto>> response = new BaseResponseWithData<List<GetDepartmentDto>>();
            response.Result = true;
            response.Errors = new List<Error>();
            response.Data = new List<GetDepartmentDto>();
            if (BranchId != 0 && BranchId != null)
            {
                var branch = _unitOfWork.Branches.GetById(BranchId);
                if (branch != null)
                {
                    var departments = _unitOfWork.Departments.FindAll(a=>a.BranchId== BranchId && a.Archived!=true).ToList();
                    foreach (var department in departments)
                    {
                        var dep = _mapper.Map<GetDepartmentDto>(department);
                        var dbTeams = _unitOfWork.Teams.FindAll(a => a.DepartmentId == dep.Id);
                        var teams = _mapper.Map<List<GetTeamDto>>(dbTeams);
                        var dbTeamUsers =
                        teams.Select(a =>
                        {
                            a.HrUsers = new List<GetHrTeamUsersDto>();
                            var dbTeamUsers = _unitOfWork.UserTeams.FindAll(x => x.TeamId == a.Id).Select(x => x.HrUserId).ToList();
                            var hrs = _unitOfWork.HrUsers.FindAll(x => dbTeamUsers.Contains(x.Id)).ToList();
                            var hrlist = _mapper.Map<List<GetHrTeamUsersDto>>(hrs);
                            a.HrUsers = hrlist;
                            return a;
                        }).ToList();
                        dep.Teams = dbTeamUsers;
                        response.Data.Add(dep);
                    }
                    return response;

                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "branch not found";
                    response.Errors.Add(error);
                    return response;
                }
            }
            else
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "branch Id not Correct";
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithId<long> DeleteDepartment(int DepartmentId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (DepartmentId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Department Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var department = _unitOfWork.Departments.Find(a => a.Id == DepartmentId, includes: new[] { "Attendances", "MaintenanceReportUsers", "ProjectFabricationReportUsers" });
                if (department == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Department is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                var users = _unitOfWork.Users.FindAll(a=>a.DepartmentId==department.Id).ToList();
                if (users.Count > 0)
                {
                    for(int i=0;i<users.Count; i++)
                    {
                        var user = users[i];
                        user.DepartmentId = null;
                        _unitOfWork.Users.Update(user);
                        _unitOfWork.Complete();
                    }
                }
                _unitOfWork.Departments.Delete(department);
                _unitOfWork.Complete();
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithId<long> ArchiveDepartment(int DepartmentId,bool Archive,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (DepartmentId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Department Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var department = _unitOfWork.Departments.GetById(DepartmentId);
                if(department == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Department not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                department.Archived = Archive;
                department.ModifiedBy = creator;
                department.ModifiedDate = DateTime.Now;
                _unitOfWork.Departments.Update(department);
                _unitOfWork.Complete();
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
