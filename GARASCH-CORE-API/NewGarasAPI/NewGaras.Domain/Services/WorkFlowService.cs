using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.JobTitle;
using NewGaras.Infrastructure.DTO.ProjectSprint;
using NewGaras.Infrastructure.DTO.WorkFlow;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces;
using NewGaras.Infrastructure.Models.HrUser;
using NewGarasAPI.Models.Project.UsedInResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class WorkFlowService : IWorkFlowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;

        public WorkFlowService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _host = host;
        }

        public async Task<BaseResponseWithId<long>> AddWorkFlow([FromForm]AddWorkFlowDto Dto ,long creatorID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            #region AvailabilityDB
            var project = _unitOfWork.Projects.FindAll((a => a.Id == Dto.ProjectID)).FirstOrDefault();
            if(project == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err103";
                error.ErrorMSG = "No project with This ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {
                var workFlowList = new List<ProjectWorkFlow>();
                foreach (var workFlow in Dto.WorkFlowList)
                {
                    var newWorkFlow = new ProjectWorkFlow();
                    newWorkFlow.WorkFlowName = workFlow.Name;
                    newWorkFlow.ProjectId = Dto.ProjectID;
                    newWorkFlow.OrderNo = workFlow.orderNum;
                    newWorkFlow.Active = true;
                    newWorkFlow.CreateBy = creatorID;
                    newWorkFlow.CreationDate = DateTime.Now;
                    newWorkFlow.ModifiedBy = creatorID;
                    newWorkFlow.ModifiedDate = DateTime.Now;

                    workFlowList.Add(newWorkFlow);
                }
                
                

                await _unitOfWork.Workflows.AddRangeAsync(workFlowList);
                _unitOfWork.Complete();

                response.ID = Dto.ProjectID;
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

        public  BaseResponseWithData<GetWorkFlowDto> GetWorKFlowByID(long ID)
        {
            var response = new BaseResponseWithData<GetWorkFlowDto>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var data = _unitOfWork.Workflows.Find(x => x.Id == ID);

                if(data == null) 
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.ErrorMSG = "No WorkFlow with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                var mapData = _mapper.Map<GetWorkFlowDto>(data);
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

        public BaseResponseWithId<long> EditWorkFlow(EditWorkFlowDto Dto, long editor)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var workFlowList = _unitOfWork.Workflows.FindAll((a => a.ProjectId == Dto.ProjectID));

                var projectworkFlowList = new List<ProjectWorkFlow>();
                int counterNum = 0;
                foreach (var workFlow in Dto.WorkFlowList)
                {
                    if (workFlow.Id != null && workFlow.Active == true)
                    {
                        var checkAvailabilityInDB = workFlowList.Where(a => a.Id == workFlow.Id).FirstOrDefault();
                        if (checkAvailabilityInDB == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"There is no project sprint with this ID (sprint number {counterNum})";
                            response.Errors.Add(err);
                            return response;
                        }

                        checkAvailabilityInDB.WorkFlowName = workFlow.Name;
                        checkAvailabilityInDB.OrderNo = workFlow.OrderNum;
                        checkAvailabilityInDB.ModifiedDate = DateTime.Now;
                        checkAvailabilityInDB.ModifiedBy = editor;

                        //_unitOfWork.Complete();
                    }
                    if (workFlow.Id == null && workFlow.Active == true)
                    {

                        var newProjectWorkFlow = new ProjectWorkFlow();
                        newProjectWorkFlow.WorkFlowName = workFlow.Name;
                        newProjectWorkFlow.ProjectId = Dto.ProjectID;
                        newProjectWorkFlow.OrderNo = workFlow.OrderNum;
                        newProjectWorkFlow.CreateBy = editor;
                        newProjectWorkFlow.CreationDate = DateTime.Now;
                        newProjectWorkFlow.ModifiedBy = editor;
                        newProjectWorkFlow.ModifiedDate = DateTime.Now;

                        projectworkFlowList.Add(newProjectWorkFlow);
                    }
                    if (workFlow.Id != null && workFlow.Active == false)
                    {
                        var currentWorkFlow = workFlowList.Where(a => a.Id == workFlow.Id).FirstOrDefault();
                        if (currentWorkFlow == null)
                        {
                            response.Result = false;
                            Error err = new Error();
                            err.ErrorCode = "E-1";
                            err.errorMSG = $"There is no project sprint with this ID (sprint number {counterNum})";
                            response.Errors.Add(err);
                            return response;
                        }
                        _unitOfWork.Workflows.Delete(currentWorkFlow);
                    }
                    counterNum++;
                }

                _unitOfWork.Workflows.AddRange(projectworkFlowList);
                _unitOfWork.Complete();
                response.ID = Dto.ProjectID;
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

        public BaseResponseWithData<List<GetWorkFlowDto>> GetWorKFlowByID()
        {
            var response = new BaseResponseWithData<List<GetWorkFlowDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var data = _unitOfWork.Workflows.GetAll();

                
                var mapData = _mapper.Map<List<GetWorkFlowDto>>(data);
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

        public BaseResponseWithData<List<GetWorkFlowDto>> GetProjectWorkFlowList(long ProjectId)
        {
            var response = new BaseResponseWithData<List<GetWorkFlowDto>>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var workFlowList = _unitOfWork.ProjectSprints.FindAll((a => a.ProjectId == ProjectId));
                var mapData = _mapper.Map<List<GetWorkFlowDto>>(workFlowList);
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

    }
}
