using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.DTO.HrUser;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.JobTitle;
using NewGaras.Infrastructure.Entities;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class JobTitleService : IJobTitleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IHrUserService _hrUserService;

        public JobTitleService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, IHrUserService hrUserService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _host = host;
            _hrUserService = hrUserService;
        }

        

        public async Task<BaseResponseWithData<List<GetAllJobTilteDto>>> GetAll()
        {
            var response = new BaseResponseWithData<List<GetAllJobTilteDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var jobTitleList =await _unitOfWork.JobTitles.FindAllAsync(a=>a.Archive!=true);
                response.Data = jobTitleList.Select(a => new GetAllJobTilteDto
                {
                    Id = a.Id,
                    Name = a.Name,
                    HourlyRate = a.HourlyRate,
                    TotalHrUserNumber = 5,
                    Currency = a.Currency,
                    Description = a.Description,
                }).ToList();

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithId<long>> AddJobTilte(AddJobTitleDto NewJobTitle , long UserId)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var data = _mapper.Map<JobTitle>(NewJobTitle);

                data.CreatedBy = UserId;
                data.CreationDate = DateTime.Now;
                data.Active = true;
                data.ModifiedBy = UserId;
                data.ModifiedDate = DateTime.Now;


                var JobTitle  = _unitOfWork.JobTitles.Add(data);
                _unitOfWork.Complete();

                response.ID = JobTitle.Id;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithData<GetJobTilteDto>> GetById(long Id)
        {
            var response = new BaseResponseWithData<GetJobTilteDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var data = await _unitOfWork.JobTitles.FindAsync(x => x.Id == Id, new[] { "CurrencyNavigation" });
                if (data == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorCode = "There is No Job Tilles with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                //var mapData = _mapper.Map<GetJobTilteDto>(data);
                GetJobTilteDto jobTitle = new GetJobTilteDto();
                jobTitle.Id = data.Id;
                jobTitle.Name = data.Name;
                jobTitle.HourlyRate = data.HourlyRate;
                jobTitle.Currency = data.Currency;
                jobTitle.CurrencyName = data.CurrencyNavigation?.Name;
                jobTitle.Description = data.Description;

                var hrUsers = await _hrUserService.GetHrUsersWithJobTitleNameImage((int)Id);
                List<HrUsersWithJobTitleNameImage> usersList = new List<HrUsersWithJobTitleNameImage>();

                foreach (var user in hrUsers.Data)
                {
                    HrUsersWithJobTitleNameImage currentUser = new HrUsersWithJobTitleNameImage();
                    currentUser.Name = user.Name;
                    currentUser.ImgPath = Globals.baseURL  + user.ImgPath;

                    usersList.Add(currentUser);
                }
                jobTitle.usersInJobTile = usersList;
                response.Data = jobTitle;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public async Task<BaseResponseWithId<long>> EditJobTitle(EditJobTitleDto newJobTitle, long UserId)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                if (response.Result)
                {

                    var OldJobTitle = await _unitOfWork.JobTitles.FindAsync(a => a.Id == newJobTitle.Id);
                    if (OldJobTitle != null)
                    {
                        if (newJobTitle.JobTitleName != null)
                        {
                            OldJobTitle.Name = newJobTitle.JobTitleName;
                        }
                        if (newJobTitle.hourlyRate != null || newJobTitle.hourlyRate != 0)
                        {
                            OldJobTitle.HourlyRate = newJobTitle.hourlyRate;
                        }
                        if (newJobTitle.currency != null || newJobTitle.currency != 0)
                        {
                            OldJobTitle.Currency = newJobTitle.currency;
                        }
                        if(newJobTitle.Description != null)
                        {
                            OldJobTitle.Description = newJobTitle.Description;
                        }

                        OldJobTitle.ModifiedBy = UserId;
                        OldJobTitle.ModifiedDate = DateTime.Now;
                        _unitOfWork.Complete();
                        response.ID = OldJobTitle.Id;
                    }
                    else
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "No Job Title with this Id";
                        response.Errors.Add(error);
                        return response;

                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> DeleteJobTitle(long JobTitleId)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (JobTitleId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorCode = "JobTitle Id Is Required";
                    response.Errors.Add(err);
                    return response;
                }
                var jobtitle = _unitOfWork.JobTitles.Find(a=>a.Id== JobTitleId, includes:new[]{ "Users", "Interviews", "JobInformations", "MaintenanceReportUsers", "ProjectFabricationJobTitles", "ProjectFabricationReportUsers", "ProjectInstallationJobTitles", "ProjectInstallationReportUsers"});
                var users = jobtitle.Users.ToList();
                if (jobtitle == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorCode = "JobTitle Is not found";
                    response.Errors.Add(err);
                    return response;
                }
                for(int i= 0;i < users.Count; i++)
                {
                     users[i].JobTitleId = null;
                    _unitOfWork.Users.Update(users[i]);
                    _unitOfWork.Complete();
                }
                response.ID = JobTitleId;
                _unitOfWork.JobTitles.Delete(jobtitle);
                _unitOfWork.Complete();
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }

        public BaseResponseWithId<long> ArchiveJobTitle(long JobTitleId, bool Archive,long creator)
        {
            var response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                if (JobTitleId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorCode = "JobTitle Id Is Required";
                    response.Errors.Add(err);
                    return response;
                }
                var jobtitle = _unitOfWork.JobTitles.Find(a => a.Id == JobTitleId);
                if (jobtitle == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E102";
                    err.errorCode = "JobTitle Is not found";
                    response.Errors.Add(err);
                    return response;
                }
                jobtitle.Archive = Archive;
                jobtitle.ModifiedBy = creator;
                jobtitle.ModifiedDate = DateTime.Now;
                _unitOfWork.JobTitles.Update(jobtitle);
                _unitOfWork.Complete();
                response.ID = JobTitleId;
                return response;
            }
            catch(Exception ex)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorCode = "Exception :" + ex.Message;
                response.Errors.Add(err);
                return response;
            }
        }
    }
}
