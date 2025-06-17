using AutoMapper;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.ProjectSprint;
using NewGaras.Infrastructure.DTO.WorkFlow;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class SprintService : ISprintService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;

        public SprintService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _host = host;
        }

        public async Task<BaseResponseWithId<long>> AddProjectSprint(AddProjectSprintDto Dto, long creatorID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            var project = _unitOfWork.Projects.GetById(Dto.projectID);
            if (project == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = $"NO Project with This ID";
                response.Errors.Add(err);
                return response;
            }

            #region DateValidation
            int counter = 1;
            foreach (var sprint in Dto.sprintsList)
            {
                
                DateTime startDate = DateTime.Now;
                if (!DateTime.TryParse(sprint.stratDate, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid start Date at sprint number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                DateTime endDate = DateTime.Now;
                if (!DateTime.TryParse(sprint.EndDate, out endDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"please, Enter a valid end Date at sprint number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                if (endDate < startDate)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"The End Date cannot be smaller than Start date at sprint number {counter}";
                    response.Errors.Add(err);
                    return response;
                }
                if(startDate < project.StartDate)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"The Start Date of sprint number {counter} cant not be before the Start date of the project";
                    response.Errors.Add(err);
                    return response;
                }
                if (endDate > project.EndDate)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = $"The End Date of sprint number {counter} {(sprint.name)} cant not be after the End date of the project";
                    response.Errors.Add(err);
                    return response;
                }
                counter++;

            }
            #endregion
            


            try
            {
                var sprintList = new List<ProjectSprint>();
                foreach(var sprint in Dto.sprintsList)
                {
                    var newProjectSprint = new ProjectSprint();
                    newProjectSprint.Name = sprint.name;
                    newProjectSprint.ProjectId = Dto.projectID;
                    newProjectSprint.OrderNo = sprint.orderNo;
                    newProjectSprint.StartDate = DateTime.Parse(sprint.stratDate);
                    newProjectSprint.EndDate = DateTime.Parse(sprint.EndDate);
                    newProjectSprint.CreatedBy = creatorID;
                    newProjectSprint.CreatedDate = DateTime.Now;
                    newProjectSprint.ModifiedBy = creatorID;
                    newProjectSprint.ModifiedDate = DateTime.Now;

                    sprintList.Add(newProjectSprint);
                }
                

                await _unitOfWork.ProjectSprints.AddRangeAsync(sprintList);
                _unitOfWork.Complete();

                response.ID = 1;//newProjectSprint.Id;
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

        public BaseResponseWithData<GetProjectsprintDto> GetProjectSprintByID(long ID)
        {
            var response = new BaseResponseWithData<GetProjectsprintDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var data = _unitOfWork.ProjectSprints.FindAll((a=> a.Id == ID)).FirstOrDefault();

                if (data == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.ErrorMSG = "No Project Sprint with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                var mapData = _mapper.Map<GetProjectsprintDto>(data);
                //GetWorkFlowDto workflow = new GetWorkFlowDto();


                response.Data = mapData;
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

        public BaseResponseWithData<List<GetProjectsprintDto>> GetProjectSprintList(long ProjectId)
        {
            var response = new BaseResponseWithData<List<GetProjectsprintDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var sprintList = _unitOfWork.ProjectSprints.FindAll((a => a.ProjectId == ProjectId));
                var mapData = _mapper.Map<List<GetProjectsprintDto>>(sprintList);
                //GetWorkFlowDto workflow = new GetWorkFlowDto();


                response.Data = mapData;
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

        public BaseResponseWithId<long> EditProjectSprint(EditProjectSprint Dto , long editor)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            var project = _unitOfWork.Projects.GetById(Dto.ProjectId);
            if (project == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = $"NO Project with This ID";
                response.Errors.Add(err);
                return response;
            }
            #region DataValidation
            int counter = 1;
            foreach (var sprint in Dto.sprintsList)
            {
                if (sprint.Active)
                {
                    DateTime startDate = DateTime.Now;
                    if (!DateTime.TryParse(sprint.stratDate, out startDate))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please, Enter a valid start Date at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    DateTime endDate = DateTime.Now;
                    if (!DateTime.TryParse(sprint.EndDate, out endDate))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"please, Enter a valid end Date at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (endDate < startDate)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"The End Date cannot be smaller than Start date at sprint number {counter}";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (startDate < project.StartDate)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"The Start Date of sprint number {counter} cant not be before the Start date of the project";
                        response.Errors.Add(err);
                        return response;
                    }
                    if (endDate > project.EndDate)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = $"The End Date of sprint number {counter} {(sprint.name)} cant not be after the End date of the project";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                
                counter++;

            }
            #endregion


            try
            {
                var projectSprint = _unitOfWork.ProjectSprints.FindAll((a => a.ProjectId == Dto.ProjectId));
                

                
                var sprintList = new List<ProjectSprint>();
                int counterNum = 0;
                foreach (var sprint in Dto.sprintsList)
                {
                    if(sprint.ID != null && sprint.Active == true)
                    {
                        var checkAvailabilityInDB = projectSprint.Where(a => a.Id == sprint.ID).FirstOrDefault();
                        if (checkAvailabilityInDB == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"There is no project sprint with this ID (sprint number {counterNum})";
                            response.Errors.Add(err);
                            return response;
                        }

                        checkAvailabilityInDB.Name = sprint.name;
                        checkAvailabilityInDB.OrderNo = sprint.orderNo;
                        checkAvailabilityInDB.ModifiedDate = DateTime.Now;
                        checkAvailabilityInDB.ModifiedBy = editor;

                        var tastsOfSprint = _unitOfWork.Tasks.FindAll((a => a.ProjectSprintId ==  sprint.ID)).ToList();
                        
                        foreach (var task in tastsOfSprint)  // validat the date of sprint according to tasks dates in side it
                        {
                            if(task.CreationDate.Date < DateTime.Parse(sprint.stratDate).Date) 
                            {
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E-1";
                                err.errorMSG = $"Invalid sprint start Date there is Task outside sprint duration at sprint number {counterNum})";
                                response.Errors.Add(err);
                                return response;
                            }
                            if(task.ExpireDate > DateTime.Parse(sprint.EndDate).Date)
                            {
                                
                                response.Result = false;
                                Error err = new Error();
                                err.ErrorCode = "E-1";
                                err.errorMSG = $"Invalid sprint End Date there is Task outside sprint duration at sprint number {counterNum})";
                                response.Errors.Add(err);
                                return response;
                                
                            }
                        }
                        checkAvailabilityInDB.StartDate = DateTime.Parse(sprint.stratDate);
                        checkAvailabilityInDB.EndDate = DateTime.Parse(sprint.EndDate);
                        //_unitOfWork.Complete();
                    }
                    if(sprint.ID == null && sprint.Active == true)
                    {

                        var newProjectSprint = new ProjectSprint();
                        newProjectSprint.Name = sprint.name;
                        newProjectSprint.ProjectId = Dto.ProjectId;
                        newProjectSprint.OrderNo = sprint.orderNo;
                        newProjectSprint.StartDate = DateTime.Parse(sprint.stratDate);
                        newProjectSprint.EndDate = DateTime.Parse(sprint.EndDate);
                        newProjectSprint.CreatedBy = editor;
                        newProjectSprint.CreatedDate = DateTime.Now;
                        newProjectSprint.ModifiedBy = editor;
                        newProjectSprint.ModifiedDate = DateTime.Now;

                        sprintList.Add(newProjectSprint);
                    }
                    if(sprint.ID != null && sprint.Active == false)
                    {
                        var currentSprint = projectSprint.Where(a => a.Id == sprint.ID).FirstOrDefault();
                        if (currentSprint == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"There is no project sprint with this ID (sprint number {counterNum})";
                            response.Errors.Add(err);
                            return response;
                        }
                        var tasksOfSprint = _unitOfWork.Tasks.FindAll((a => a.ProjectSprintId == sprint.ID)).ToList();
                        if(tasksOfSprint != null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"There is Tasks in this sprint (sprint number {counterNum})";
                            response.Errors.Add(err);
                            return response;
                        }
                        _unitOfWork.ProjectSprints.Delete(currentSprint);
                    }
                    counterNum++;
                }


                _unitOfWork.ProjectSprints.AddRangeAsync(sprintList);
                _unitOfWork.Complete();

                response.ID = Dto.ProjectId;//newProjectSprint.Id;
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

        public BaseResponseWithId<long> DeleteProjectSprint(long Id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var projectSprint = _unitOfWork.ProjectSprints.GetById(Id);
            if (projectSprint == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No project sprint with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var tasksWithThisSprintID = _unitOfWork.Tasks.FindAll(a => a.ProjectSprintId == Id);
                //foreach (var task in tasksWithThisSprintID)
                //{
                //    task.ProjectSprintId = null;
                //}
                _unitOfWork.Tasks.DeleteRange(tasksWithThisSprintID);

                _unitOfWork.ProjectSprints.Delete(projectSprint);
                _unitOfWork.Complete();

                response.ID = projectSprint.Id;
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
    }
}
