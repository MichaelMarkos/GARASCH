using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.ProjectManagment;
using NewGaras.Infrastructure.Models.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

namespace NewGaras.Domain.Services.ProjectManagment
{
    public class ProjectProgressService : IProjectProgressService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
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

        public ProjectProgressService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<int> AddNewProgressType([FromBody] ProgressTypeDto dto,long creator)

        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {

                if (Response.Result)
                {
                    var dbTypes = _unitOfWork.ProgressTypes.GetAll();
                    if (dbTypes.Count() > 0)
                    {
                        var sumPercentage = dbTypes.Select(a => a.ProgressPercentage).Sum() + dto.ProgressPercentage;
                        if (sumPercentage > 100)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Sum Of Progress Percentages can't Exceed 100%";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        var check = dbTypes.Where(a => a.Type.Trim().ToLower() == dto.Type.Trim().ToLower()).FirstOrDefault();
                        if (check != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err102";
                            error.ErrorMSG = "name of progress type can't be duplicated";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    var type = _mapper.Map<ProgressType>(dto);
                    type.Active = true;
                    type.CreationDate = DateTime.Now;
                    type.ModifiedDate = DateTime.Now;
                    type.CreatedBy = creator;
                    type.ModifiedBy = creator;

                    _unitOfWork.ProgressTypes.Add(type);
                    _unitOfWork.Complete();

                    Response.ID = type.Id;

                }
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

        public BaseResponseWithId<int> UpdateProgressType([FromBody] ProgressTypeDto dto, long creator)

        {
            BaseResponseWithId<int> Response = new BaseResponseWithId<int>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {

                if (Response.Result)
                {
                    var dbtype = _unitOfWork.ProgressTypes.Find(a=>a.Id==dto.Id);
                    if (dbtype == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "this Progress type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var dbTypes = _unitOfWork.ProgressTypes.FindAll(a => a.Id != dto.Id).ToList();
                    if (dbTypes.Count() > 0)
                    {
                        var sumPercentage = dbTypes.Select(a => a.ProgressPercentage).Sum() + dto.ProgressPercentage;
                        if (sumPercentage > 100)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err101";
                            error.ErrorMSG = "Sum Of Progress Percentages can't Exceed 100%";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        var check = dbTypes.Where(a => a.Type.Trim().ToLower() == dto.Type.Trim().ToLower()).FirstOrDefault();
                        if (check != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err102";
                            error.ErrorMSG = "name of progress type can't be duplicated";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }

                    _mapper.Map<ProgressTypeDto, ProgressType>(dto, dbtype);
                    dbtype.ModifiedDate = DateTime.Now;
                    dbtype.ModifiedBy = creator;
                    _unitOfWork.ProgressTypes.Update(dbtype);
                    _unitOfWork.Complete();

                    Response.ID = dbtype.Id;

                }
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

        public BaseResponseWithData<ProgressTypeDto> GetProgressTypeById([FromHeader] int progressId)
        {
            BaseResponseWithData<ProgressTypeDto> Response = new BaseResponseWithData<ProgressTypeDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Response.Result)
                {
                    var dbtype = _unitOfWork.ProgressTypes.Find(a=>a.Id == progressId);
                    if (dbtype == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "this Progress type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var type = _mapper.Map<ProgressTypeDto>(dbtype);
                    Response.Data = type;

                }
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

        public BaseResponseWithData<List<ProgressTypeDto>> GetProgressTypeList()
        {
            BaseResponseWithData<List<ProgressTypeDto>> Response = new BaseResponseWithData<List<ProgressTypeDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Response.Result)
                {
                    var dbtypes = _unitOfWork.ProgressTypes.GetAll();
                    var types = _mapper.Map<List<ProgressTypeDto>>(dbtypes);
                    Response.Data = types;

                }
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

        public BaseResponseWithData<List<DeliveryTypeDto>> GetDeliveryTypeList()
        {
            BaseResponseWithData<List<DeliveryTypeDto>> Response = new BaseResponseWithData<List<DeliveryTypeDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {

                if (Response.Result)
                {
                    var dbtypes = _unitOfWork.DeliveryTypes.GetAll();
                    var types = _mapper.Map<List<DeliveryTypeDto>>(dbtypes);
                    Response.Data = types;

                }
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

        public BaseResponseWithData<List<ProgressStatusDto>> GetProgressStatusList()
        {
            BaseResponseWithData<List<ProgressStatusDto>> Response = new BaseResponseWithData<List<ProgressStatusDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    var dbtypes = _unitOfWork.ProgressStatuses.GetAll();
                    var types = _mapper.Map<List<ProgressStatusDto>>(dbtypes);
                    Response.Data = types;

                }
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

        public BaseResponseWithId<long> AddNewProjectProgress([FromForm] ProjectProgressDto dto,long creator , string CompanyName)

        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {

                if (Response.Result)
                {
                    if (dto == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "You sent Invalid Data";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.ProjectId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var project = _unitOfWork.Projects.GetById(dto.ProjectId);
                    if (project == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.ProgressTypeId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Type Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var progresstype = _unitOfWork.ProgressTypes.GetById(dto.ProgressTypeId);
                    if (progresstype == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.DeliveryTypeId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Delivery Type Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var deliverytype = _unitOfWork.DeliveryTypes.GetById(dto.DeliveryTypeId);
                    if (deliverytype == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Delivery Type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.ProgressStatusId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Status Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var progressstatus = _unitOfWork.ProgressStatuses.GetById(dto.ProgressStatusId);
                    if (progressstatus == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Status is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var projectProgress = new ProjectProgress()
                    {
                        ProjectId = dto.ProjectId,
                        ProgressTypeId = progresstype.Id,
                        ProgressStatusId = progressstatus.Id,
                        DeliveryTypeId = deliverytype.Id,
                        RelatedCollectedPercent = progresstype.ProgressPercentage,
                        Date = dto.Date,
                        Active = true,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        Comment = dto.Comment,
                        CreatedBy = creator,
                        ModifiedBy = creator,
                    };
                    if (dto.Attachment != null)
                    {
                        var fileExtension = dto.Attachment.FileName.Split('.').Last();
                        var virtualPath = $"Attachments\\{CompanyName}\\ProjectProgress\\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.Attachment.FileName.Trim().Replace(" ", ""));
                        var AttachPath = Common.SaveFileIFF(virtualPath, dto.Attachment, FileName, fileExtension, _host);
                        projectProgress.AttachmentPath = AttachPath;
                    }
                    _unitOfWork.ProjectProgresses.Add(projectProgress);
                    _unitOfWork.Complete();
                    var ProgressId = projectProgress.Id;
                    dto.ProgressUsers.ForEach(a => a.ProjectProgressId = ProgressId);
                    var AddedList = dto.ProgressUsers.Select(a => _unitOfWork.ProjectProgressUsers.Add(new ProjectProgressUser()
                    {
                        ProjectProgressId = a.ProjectProgressId,
                        HrUserId = a.HrUserId,
                        DateFrom = a.DateFrom,
                        DateTo = a.DateTo,
                        HoursNum = a.HoursNum,
                        Evaluation = a.Evaluation,
                        Comment = a.Comment,
                        CreatedBy = validation.userID,
                        CreationDate = DateTime.Now,
                        ModifiedBy = validation.userID,
                        ModifiedDate = DateTime.Now,
                        InventoryItemCategoryId = a.InventoryItemCategoryId,
                        Active = true
                    })).ToList();

                    _unitOfWork.ProjectProgressUsers.AddRange(AddedList); 
                    _unitOfWork.Complete();
                    Response.ID = projectProgress.Id;
                }
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

        public BaseResponseWithId<long> UpdateProjectProgress([FromForm] ProjectProgressDto dto,long creator, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Response.Result)
                {
                    if (dto == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "You sent Invalid Data";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.Id == 0 || dto.Id == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project Progress Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var projectProgress = _unitOfWork.ProjectProgresses.FindAll(a => a.Id == dto.Id, includes: new[] { "ProjectProgressUsers" }).FirstOrDefault();
                    if (projectProgress == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.ProjectId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var project = _unitOfWork.Projects.GetById(dto.ProjectId);
                    if (project == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.ProgressTypeId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Type Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var progresstype = _unitOfWork.ProgressTypes.GetById(dto.ProgressTypeId);
                    if (progresstype == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.DeliveryTypeId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Delivery Type Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var deliverytype = _unitOfWork.DeliveryTypes.GetById(dto.DeliveryTypeId);
                    if (deliverytype == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Delivery Type is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (dto.ProgressStatusId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Status Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var progressstatus = _unitOfWork.ProgressStatuses.GetById(dto.ProgressStatusId);
                    if (progressstatus == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Progress Status is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    projectProgress.ProjectId = dto.ProjectId;
                    projectProgress.ProgressTypeId = progresstype.Id;
                    projectProgress.ProgressStatusId = progressstatus.Id;
                    projectProgress.DeliveryTypeId = deliverytype.Id;
                    projectProgress.RelatedCollectedPercent = progresstype.ProgressPercentage;
                    projectProgress.Date = dto.Date;
                    projectProgress.Active = dto.Active;
                    projectProgress.ModifiedDate = DateTime.Now;
                    projectProgress.ModifiedBy = creator;
                    projectProgress.Comment = dto.Comment;

                    if (projectProgress.AttachmentPath != null && dto.DeleteAttachment == true)
                    {
                        var oldpath = Path.Combine(_host.WebRootPath, projectProgress.AttachmentPath);
                        if (System.IO.File.Exists(oldpath))
                        {
                            System.IO.File.Delete(oldpath);
                            projectProgress.AttachmentPath = null;
                        }
                    }
                    if (dto.Attachment != null)
                    {
                        if (projectProgress.AttachmentPath != null)
                        {
                            var oldpath = Path.Combine(_host.WebRootPath, projectProgress.AttachmentPath);
                            if (System.IO.File.Exists(oldpath))
                            {
                                System.IO.File.Delete(oldpath);
                                projectProgress.AttachmentPath = null;
                            }

                        }
                        var fileExtension = dto.Attachment.FileName.Split('.').Last();
                        var virtualPath = $@"Attachments\{CompanyName}\ProjectProgress\";
                        var FileName = System.IO.Path.GetFileNameWithoutExtension(dto.Attachment.FileName.Trim().Replace(" ", ""));
                        var AttachPath = Common.SaveFileIFF(virtualPath, dto.Attachment, FileName, fileExtension, _host);
                        projectProgress.AttachmentPath = AttachPath;
                    }
                    var OldList = projectProgress.ProjectProgressUsers.ToList();
                    var NewList = dto.ProgressUsers.ToList();

                    var progressUsersDict = NewList.Where(a=>a.Id!=null).ToDictionary(pu => pu.Id);

                    // Iterate over the database list to update or delete items
                    for (int i = OldList.Count - 1; i >= 0; i--)
                    {
                        var dbUser = OldList[i];

                        if (progressUsersDict.TryGetValue(dbUser.Id, out var dtoUser))
                        {
                            // Update the existing item if there are changes
                            _mapper.Map<ProgressUsers, ProjectProgressUser>(dtoUser, dbUser);


                            // Remove the item from the dictionary to avoid adding it later
                            progressUsersDict.Remove(dbUser.Id);
                        }
                        else
                        {
                            // Delete the item if it doesn't exist in the DTO list
                            _unitOfWork.ProjectProgressUsers.Delete(dbUser);
                            _unitOfWork.Complete();
                            OldList.RemoveAt(i);
                        }
                    }

                    // Add new items from the DTO list
                    foreach (var dtoUser in NewList.Where(a => a.Id == null))
                    {
                        _unitOfWork.ProjectProgressUsers.Add(new ProjectProgressUser
                        {
                            ProjectProgressId = projectProgress.Id,
                            HrUserId = dtoUser.HrUserId,
                            DateFrom = dtoUser.DateFrom,
                            DateTo = dtoUser.DateTo,
                            HoursNum = dtoUser.HoursNum,
                            Evaluation = dtoUser.Evaluation,
                            Comment = dtoUser.Comment,
                            CreatedBy = validation.userID,
                            CreationDate = DateTime.Now,
                            ModifiedBy = validation.userID,
                            ModifiedDate = DateTime.Now,
                            InventoryItemCategoryId = dtoUser.InventoryItemCategoryId,
                            Active = true
                        });
                        _unitOfWork.Complete();
                        
                    }
                    foreach (var user in OldList)
                    {
                        _unitOfWork.ProjectProgressUsers.Update(user);
                        _unitOfWork.Complete();
                    }

                    _unitOfWork.ProjectProgresses.Update(projectProgress);
                    _unitOfWork.Complete();

                    Response.ID = projectProgress.Id;
                }
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

        public BaseResponseWithData<GetProjectProgressDto> GetProjectProgressById([FromHeader] long ProjectProgressId)
        {
            BaseResponseWithData<GetProjectProgressDto> Response = new BaseResponseWithData<GetProjectProgressDto>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Response.Result)
                {
                    if (ProjectProgressId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project Progress Id Is required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var Progress = _unitOfWork.ProjectProgresses.Find(a => a.Id == ProjectProgressId, includes: new[] { "Project.SalesOffer", "ProgressType", "DeliveryType", "ProgressStatus", "ProjectProgressUsers.HrUser", "ProjectProgressUsers.InventoryItemCategory" });
                        
                    if (Progress == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "Project Progress not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var model = new GetProjectProgressDto()
                    {
                        Id = Progress.Id,
                        ProjectName = Progress.Project.SalesOffer.ProjectName,
                        ProgressTypeName = Progress.ProgressType.Type,
                        DeliveryTypeName = Progress.DeliveryType.Type,
                        ProgressStatusName = Progress.ProgressStatus.Status,
                        Date = Progress.Date.ToShortDateString(),
                        RelatedCollectedPercent = Progress.RelatedCollectedPercent,
                        AttachmentPath = Progress.AttachmentPath != null ? Globals.baseURL + @"\" + Progress.AttachmentPath : null,
                        Comment = Progress.Comment,
                        ProgressUsers = _mapper.Map<List<GetProgressUsers>>(Progress.ProjectProgressUsers)
                    };
                    Response.Data = model;

                }
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

        public BaseResponseWithData<List<GetProjectProgressDto>> GetProjectProgressList([FromHeader] long projectId)

        {
            BaseResponseWithData<List<GetProjectProgressDto>> Response = new BaseResponseWithData<List<GetProjectProgressDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            var list = new List<GetProjectProgressDto>();
            try
            {
                #region validation
                var project = _unitOfWork.Projects.Find(a => a.Id == projectId);
                if (project == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "No Project with this Id";
                    Response.Errors.Add(error);
                    return Response;
                }
                #endregion

                if (Response.Result)
                {
                    var Progresses = 
                    _unitOfWork.ProjectProgresses.FindAll(a =>a.ProjectId == projectId, includes: new[] { "Project.SalesOffer", "ProgressType", "DeliveryType", "ProgressStatus", "ProjectProgressUsers.HrUser", "ProjectProgressUsers.InventoryItemCategory" }).ToList();

                    for (int i = 0; i < Progresses.Count; i++)
                    {
                        var model = new GetProjectProgressDto()
                        {
                            Id = Progresses[i].Id,
                            ProjectName = Progresses[i].Project.SalesOffer.ProjectName,
                            ProgressTypeID = Progresses[i].ProgressType.Id,
                            ProgressTypeName = Progresses[i].ProgressType.Type,
                            DeliveryTypeID = Progresses[i].DeliveryType.Id,
                            DeliveryTypeName = Progresses[i].DeliveryType.Type,
                            ProgressStatusID = Progresses[i].ProgressStatus.Id,
                            ProgressStatusName = Progresses[i].ProgressStatus.Status,
                            Date = Progresses[i].Date.ToShortDateString(),
                            RelatedCollectedPercent = Progresses[i].RelatedCollectedPercent,
                            AttachmentPath = Progresses[i].AttachmentPath != null ? Globals.baseURL + @"\" + Progresses[i].AttachmentPath : null,
                            Comment = Progresses[i].Comment,
                            ProgressUsers = _mapper.Map<List<GetProgressUsers>>(Progresses[i].ProjectProgressUsers)
                        };
                        list.Add(model);
                    }
                    Response.Data = list;

                }
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

        public BaseResponseWithId<long> AddProgressTypeForProject([FromHeader] long ProjectId)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {

                if (Response.Result)
                {
                    if (ProjectId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err101";
                        error.ErrorMSG = "Project Id is required";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var project = _unitOfWork.Projects.GetById(ProjectId);
                    if (project == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err102";
                        error.ErrorMSG = "Project is not found";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    var progresstypes = _unitOfWork.ProgressTypes.GetAll();


                    foreach (var type in progresstypes)
                    {
                        var prog = _unitOfWork.ProjectProgresses.FindAll(a => a.ProjectId == project.Id && a.ProgressTypeId == type.Id).FirstOrDefault();
                        if (prog == null)
                        {
                            var progress = new ProjectProgress()
                            {
                                ProjectId = project.Id,
                                ProgressTypeId = type.Id,
                                ProgressStatusId = 1,
                                DeliveryTypeId = 4,
                                RelatedCollectedPercent = 0,
                                Date = DateTime.Now,
                                AttachmentPath = null,
                                Comment = null,
                                CreatedBy = 1,
                                CreationDate = DateTime.Now,
                                ModifiedBy = 1,
                                ModifiedDate = DateTime.Now

                            };
                            _unitOfWork.ProjectProgresses.Add(progress);
                            _unitOfWork.Complete();
                        }
                    }
                    Response.ID = project.Id;
                }
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
