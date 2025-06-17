using AutoMapper;
using Azure;
using iTextSharp.text.pdf.parser.clipper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Attendance;
using NewGaras.Infrastructure.DTO.Salary.SalaryAllownces;
using NewGaras.Infrastructure.DTO.TaskExpensis;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models.TaskExpensis;
using NewGaras.Infrastructure.Models.TaskExpensis.Filters;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.AcroFields;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;

namespace NewGaras.Domain.Services
{
    public class TaskExpensisService : ITaskExpensisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly ITaskMangerProjectService _taskMangerProjectService;
        public TaskExpensisService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, ITaskMangerProjectService taskMangerProjectService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _taskMangerProjectService = taskMangerProjectService;
        }

        public BaseResponseWithData<AddExpensis> AddTaskExpensis(AddTaskExpensisDto Dto, long Creator, string CompName)
        {
            BaseResponseWithData<AddExpensis> response = new BaseResponseWithData<AddExpensis>();
            response.Result = true;
            response.Errors = new List<Error>();

            var task = _unitOfWork.Tasks.GetById(Dto.TaskId);
            if(task == null) 
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Task not Found, please Enter a valid Task ID";
                response.Errors.Add(error);
                return response;
            }

            var ExpensisType = _unitOfWork.ExpensisTypes.GetById(Dto.ExpensisTypeId);
            if (ExpensisType == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "ExpensisType not Found, please Enter a valid ExpensisType ID";
                response.Errors.Add(error);
                return response;
            }

            try
            {

                var newTaskExpensis = _mapper.Map<TaskExpensi>(Dto);
                newTaskExpensis.CreationDate = DateTime.Now;
                newTaskExpensis.CreatedBy = Creator;
                //newTaskExpensis.Approved = false;
                


                var data = _unitOfWork.TaskExpensis.Add(newTaskExpensis);
                _unitOfWork.Complete();

                string AttachPath = null;
                if (Dto.Attachment != null)
                {
                    var fileExtension = Dto.Attachment.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompName}\\TaskExpensis\\{data.Id}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(Dto.Attachment.FileName.Trim().Replace(" ", ""));
                    AttachPath = Common.SaveFileIFF(virtualPath, Dto.Attachment, FileName, fileExtension, _host);
                }

                data.ImgPath = AttachPath;
                _unitOfWork.Complete();

                //---------------------calculate the total Expensis of the project----------------------------------------
                decimal Costs = 0;
                decimal totalExpensis = 0;
                var project = _unitOfWork.Projects.FindAll((a => a.Id == task.ProjectId)).FirstOrDefault();
                if (project != null)
                {
                    var Expensis = project.Tasks.SelectMany(y => y.TaskExpensis).Sum(s => s.Amount);
                    Costs = _taskMangerProjectService.GetCostsForAllTask(project.Id).Data;
                    totalExpensis = Expensis + Costs;
                }
                
                //--------------------------------------------------------------------------------------------------------

                var newExpensis = new AddExpensis();
                newExpensis.TotalExpensis = totalExpensis;
                if (project != null)newExpensis.Budget = project.Budget;
                newExpensis.ID = data.Id;
                
                response.Data = newExpensis;
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

        public BaseResponseWithData<AddExpensis> EditTaskExpensis(EditTaskExpensisDto Dto,long Creator, string CompName)
        {
            BaseResponseWithData<AddExpensis> response = new BaseResponseWithData<AddExpensis>();
            response.Result = true;
            response.Errors = new List<Error>();

            var task = _unitOfWork.Tasks.GetById(Dto.TaskId);
            if (task == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Task not Found, please Enter a valid Task ID";
                response.Errors.Add(error);
                return response;
            }

            var ExpensisType = _unitOfWork.ExpensisTypes.GetById(Dto.ExpensisTypeId);
            if (ExpensisType == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "ExpensisType not Found, please Enter a valid ExpensisType ID";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                var TaskExpensis = _unitOfWork.TaskExpensis.Find(a => a.Id == Dto.Id);

                if (TaskExpensis == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This TaskExpensis ID is not found , please enter a valid ID";
                    response.Errors.Add(err);
                    return response;
                }

                //---------------------calculate the total Expensis of the project----------------------------------------
                var project = _unitOfWork.Projects.FindAll((a => a.Id == task.ProjectId)).FirstOrDefault();
                var Expensis = project.Tasks.SelectMany(y => y.TaskExpensis).Sum(s => s.Amount);
                var Costs = _taskMangerProjectService.GetCostsForAllTask(project.Id);
                var totalExpensis = Expensis + Costs.Data;
                //--------------------------------------------------------------------------------------------------------

                var newExpensis = new AddExpensis();
                newExpensis.TotalExpensis = totalExpensis;
                newExpensis.Budget = project.Budget;
                newExpensis.ID = TaskExpensis.Id;

                response.Data = newExpensis;

                TaskExpensis.Amount = Dto.Amount;
                TaskExpensis.Curruncy = Dto.Curruncy;
                TaskExpensis.TaskId = Dto.TaskId;
                TaskExpensis.ExpensisTypeId = Dto.ExpensisTypeId;
                TaskExpensis.Billable = Dto.Billable;
                TaskExpensis.ModifiedBy = Creator;
                TaskExpensis.ModificationDate = DateTime.Now;
                if (!string.IsNullOrWhiteSpace(Dto.Note))
                {
                    TaskExpensis.Note = Dto.Note;
                }

                if (Dto.ActiveAttach == false && TaskExpensis.ImgPath != null)
                {
                    string FilePath = Path.Combine(_host.WebRootPath, TaskExpensis.ImgPath);
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }

                    TaskExpensis.ImgPath = null;
                }

                if (Dto.Attachment !=  null )
                {
                    if(TaskExpensis.ImgPath != null)
                    {
                        string FilePath = Path.Combine(_host.WebRootPath, TaskExpensis.ImgPath);
                        if (System.IO.File.Exists(FilePath))
                        {
                            System.IO.File.Delete(FilePath);
                        }
                    }
                   
                    var fileExtension = Dto.Attachment.FileName.Split('.').Last();
                    var virtualPath = $"Attachments\\{CompName}\\TaskExpensis\\{TaskExpensis.Id}\\";
                    var FileName = System.IO.Path.GetFileNameWithoutExtension(Dto.Attachment.FileName.Trim().Replace(" ", ""));
                    var AttachPath = Common.SaveFileIFF(virtualPath, Dto.Attachment, FileName, fileExtension, _host);

                    TaskExpensis.ImgPath = AttachPath;
                }
               
                _unitOfWork.Complete();

                response.Data.ID = TaskExpensis.Id;
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

        public BaseResponseWithData<List<GetTaskExpensisDto>> GetTaskExpensisList(long TaskId)
        {
            BaseResponseWithData<List<GetTaskExpensisDto>> response = new BaseResponseWithData<List<GetTaskExpensisDto>>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            
            try
            {
                var task = _unitOfWork.Tasks.Find(a => a.Id == TaskId); 
                if(task == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Task not Found, please Enter a valid Task ID";
                    response.Errors.Add(error);
                    return response;
                }
                
                var TaskExpensisList = _unitOfWork.TaskExpensis.FindAll((a => a.TaskId == TaskId), new[] { "CreatedByNavigation", "ExpensisType" , "ApprovedByNavigation"  });
                TaskExpensisList = TaskExpensisList.OrderByDescending(a => a.CreationDate).ToList();
                var taskExpensisData = _mapper.Map<List<GetTaskExpensisDto>>(TaskExpensisList);

                foreach(var expensis in  taskExpensisData)
                {
                    var item = _unitOfWork.ProjectInvoiceItems.FindAll(a => a.ItemId == expensis.Id && a.Type == "TaskExpensis").FirstOrDefault();
                    if (item != null)
                    {
                        expensis.IsInvoiced = true;
                        expensis.InvoiceId = item.Id;
                    }
                }

                response.Data = taskExpensisData;
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

        public BaseResponseWithData<GetTaskExpensisDto> GetTaskExpensisByID(long Id)
        {
            BaseResponseWithData<GetTaskExpensisDto> response = new BaseResponseWithData<GetTaskExpensisDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                
                var TaskExpensisList = _unitOfWork.TaskExpensis.FindAll((a => a.Id == Id), new[] { "CreatedByNavigation", "ExpensisType", "ApprovedByNavigation" }).FirstOrDefault();
                var taskExpensisData = _mapper.Map<GetTaskExpensisDto>(TaskExpensisList);


                response.Data = taskExpensisData;
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

        public BaseResponseWithId<long> AcceptTaskExpensisByManger(long ExpensisId, bool Approved, long manger) 
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            

            var TaskExpensis = _unitOfWork.TaskExpensis.GetById(ExpensisId);
            if (TaskExpensis == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "Expensis not Found, please Enter a valid Expensis ID";
                response.Errors.Add(error);
                return response;
            }

            try
            {
                //var TaskExpensis = _unitOfWork.TaskExpensis.Find(a => a.Id == Dto.Id)

                TaskExpensis.Approved = Approved;
                TaskExpensis.ApprovedDate = DateTime.Now;
                TaskExpensis.ApprovedBy = manger;
                _unitOfWork.Complete();

                response.ID = TaskExpensis.Id;
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

        public BaseResponseWithData<GetExpensisForAllTasksDto> GetExpensisForAllTasks(GetExpensisForAllTasks filters, string companyName)
        {
            BaseResponseWithData<GetExpensisForAllTasksDto> response = new BaseResponseWithData<GetExpensisForAllTasksDto>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                string projectName = "";
                if (filters.ProjectID != 0)
                {
                    var projectInDB = _unitOfWork.Projects.GetById(filters.ProjectID);
                    if (projectInDB == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = " the Project selected is not exist";
                        response.Errors.Add(err);
                        return response;
                    }
                    var project = _unitOfWork.SalesOffers.GetById(projectInDB.SalesOfferId);
                    projectName = project.ProjectName;
                }

                //------init of response objects----------------
                response.Data = new GetExpensisForAllTasksDto();
                response.Data.ExpensisList = new List<GetTaskExpensisDto>();
                response.Data.GroupedExpenses = new List<GroupedExpensis>();

                //----------------------------------------------
                var taskExpensis = new List<GetTaskExpensisDto>();

                var tasksIDs = _unitOfWork.Tasks.FindAll((a => a.ProjectId == filters.ProjectID)).Select(a=> a.Id).ToList();
                var TaskExpensisList = _unitOfWork.TaskExpensis.FindAll((a => tasksIDs.Contains(a.TaskId)), new[] { "CreatedByNavigation", "ExpensisType", "Task", "CreatedByNavigation.JobTitle" }).ToList();
                //TaskExpensisList = TaskExpensisList.OrderByDescending(a => a.CreationDate).ToList();
                var taskExpensisData = _mapper.Map<List<GetTaskExpensisDto>>(TaskExpensisList);

                taskExpensis = TaskExpensisList.Select(a => new GetTaskExpensisDto()
                {
                    Id = a.Id,
                    Amount = a.Amount,
                    Note = a.Note,
                    Curruncy = a.Curruncy,
                    Imgpath = a.ImgPath != null ? Globals.baseURL + a.ImgPath : null,
                    TaskId = a.TaskId,
                    TaskName = a.Task != null ? a.Task.Name : "",
                    ExpensisTypeId = a.ExpensisTypeId,
                    ExpensisTypeName = a.ExpensisType != null ? a.ExpensisType.ExpensisTypeName : "",
                    CreationDate = a.CreationDate.ToShortDateString(),
                    CreatedBy = a.CreatedBy,
                    UserName = a.CreatedByNavigation.FirstName + " " + a.CreatedByNavigation.LastName,
                    UserImgPath = a.CreatedByNavigation.PhotoUrl != null ? Globals.baseURL + a.CreatedByNavigation.PhotoUrl : null,
                    ProjectName = projectName,
                    JobTitleID = a.CreatedByNavigation.JobTitle.Id,
                    JobTitleName = a.CreatedByNavigation.JobTitle.Name
                }).ToList();

                if (filters.GroupByTask || filters.GroupByDate || filters.GroupByUser)
                {
                    var finalExpensis = new List<IGrouping<string, GetTaskExpensisDto>>();
                    if (filters.GroupByTask && !filters.GroupByDate && !filters.GroupByUser)
                    {
                        finalExpensis = taskExpensis.GroupBy(a =>a.TaskName ).ToList();
                    }
                    else if (!filters.GroupByTask && filters.GroupByDate && !filters.GroupByUser)
                    {
                        finalExpensis = taskExpensis.GroupBy(a => a.CreationDate).ToList();
                    }
                    else if (!filters.GroupByTask && !filters.GroupByDate && filters.GroupByUser)
                    {
                        finalExpensis = taskExpensis.GroupBy(a => a.UserName).ToList();
                    }
                    if(finalExpensis.Count > 0)
                    {
                        response.Data.GroupedExpenses = finalExpensis.Select(a => new GroupedExpensis() { Key = a.Key, ExpensisList = [.. a] }).ToList();
                        ExcelPackage excel = new ExcelPackage();
                        var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                        workSheet.TabColor = System.Drawing.Color.Black;
                        workSheet.DefaultRowHeight = 12;
                        workSheet.Row(1).Height = 20;
                        workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Row(1).Style.Font.Bold = true;

                        workSheet.Cells[1, 1].Value = "Project Name";
                        workSheet.Cells[1, 2].Value = "Task Name";
                        workSheet.Cells[1, 3].Value = "User Name";
                        workSheet.Cells[1, 4].Value = "Date";
                        workSheet.Cells[1, 5].Value = "Amount";
                        workSheet.Cells[1, 6].Value = "Note";
                        workSheet.Cells[1, 7].Value = "Curruncy";

                        
                        int recordIndex = 2;
                        foreach (var expensis in finalExpensis)
                        {
                            workSheet.Cells[recordIndex, 1, recordIndex, 7].Merge = true;
                            workSheet.Cells[recordIndex, 1, recordIndex, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            workSheet.Cells[recordIndex, 1, recordIndex, 7].Style.Fill.BackgroundColor.SetColor(color: Color.DeepSkyBlue);
                            workSheet.Cells[recordIndex, 1, recordIndex, 7].Style.Font.Bold = true;
                            workSheet.Cells[recordIndex, 1, recordIndex, 7].Style.Font.Color.SetColor(Color.White);
                            workSheet.Cells[recordIndex, 1].Style.Font.Bold = true;
                            workSheet.Cells[recordIndex, 1].Style.Font.Size = 14;
                            workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                            if (filters.GroupByDate)
                            {
                                workSheet.Cells[recordIndex, 1].Value = "Progress Made In " + expensis.Key;
                            }
                            else if (filters.GroupByUser)
                            {
                                workSheet.Cells[recordIndex, 1].Value = "Progress Made By " + expensis.Key;
                            }
                            else if (filters.GroupByTask)
                            {
                                workSheet.Cells[recordIndex, 1].Value = "Progress Made For " + expensis.Key + " Task";
                            }
                            recordIndex += 1;

                            foreach (var item in expensis)
                            {
                                workSheet.Cells[recordIndex, 1].Value = projectName;
                                workSheet.Cells[recordIndex, 2].Value = item.TaskName;
                                workSheet.Cells[recordIndex, 3].Value = item.UserName;
                                workSheet.Cells[recordIndex, 4].Style.Numberformat.Format = "yyyy/mm/dd";
                                workSheet.Cells[recordIndex, 4].Value = item.CreationDate;
                                workSheet.Cells[recordIndex, 5].Value = item.Amount;
                                workSheet.Cells[recordIndex, 6].Value = item.Note;
                                workSheet.Cells[recordIndex, 7].Value = item.Curruncy;


                                workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                recordIndex++;
                            }
                        }
                        workSheet.Column(1).AutoFit();
                        workSheet.Column(2).AutoFit();
                        workSheet.Column(3).AutoFit();
                        workSheet.Column(4).AutoFit();
                        workSheet.Column(5).AutoFit();
                        workSheet.Column(6).AutoFit();
                        workSheet.Column(7).AutoFit();
                        

                        var path = $"Attachments\\{companyName}\\TaskExpensis";
                        var savedPath = Path.Combine(_host.WebRootPath, path);
                        if (File.Exists(savedPath))
                            File.Delete(savedPath);

                        // Create excel file on physical disk  
                        Directory.CreateDirectory(savedPath);
                        //FileStream objFileStrm = File.Create(savedPath);
                        //objFileStrm.Close();
                        var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                        var excelPath = savedPath + $"\\TaskExpensis_{date}.xlsx";
                        excel.SaveAs(excelPath);
                        // Write content to excel file  
                        //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                        //Close Excel package 
                        excel.Dispose();
                        response.Data.FilePath = Globals.baseURL + '\\' + path + $"\\TaskExpensis_{date}.xlsx";
                    }
                    
                }
                else
                {
                    if (taskExpensisData.Count > 0)
                    {
                        var taskExpensisPagedList = PagedList<GetTaskExpensisDto>.Create(taskExpensisData.AsQueryable(), filters.CurrrenPage, filters.ItemPerPage);
                        response.Data.ExpensisList = taskExpensisPagedList;
                        ExcelPackage excel = new ExcelPackage();
                        var workSheet = excel.Workbook.Worksheets.Add("Sheet1");
                        workSheet.TabColor = System.Drawing.Color.Black;
                        workSheet.DefaultRowHeight = 12;
                        workSheet.Row(1).Height = 20;
                        workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Row(1).Style.Font.Bold = true;

                        workSheet.Cells[1, 1].Value = "Project Name";
                        workSheet.Cells[1, 2].Value = "Task Name";
                        workSheet.Cells[1, 3].Value = "User Name";
                        workSheet.Cells[1, 4].Value = "Date";
                        workSheet.Cells[1, 5].Value = "Amount";
                        workSheet.Cells[1, 6].Value = "Note";
                        workSheet.Cells[1, 7].Value = "Curruncy";

                        int recordIndex = 2;
                        foreach (var expensis in taskExpensisPagedList)
                        {
                            workSheet.Cells[recordIndex, 1].Value = projectName;
                            workSheet.Cells[recordIndex, 2].Value = expensis.TaskName;
                            workSheet.Cells[recordIndex, 3].Value = expensis.UserName;
                            workSheet.Cells[recordIndex, 4].Style.Numberformat.Format = "yyyy/mm/dd";
                            workSheet.Cells[recordIndex, 4].Value = expensis.CreationDate;
                            workSheet.Cells[recordIndex, 5].Value = expensis.Amount;
                            workSheet.Cells[recordIndex, 6].Value = expensis.Note;
                            workSheet.Cells[recordIndex, 7].Value = expensis.Curruncy;


                            workSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            recordIndex++;

                            //-------------add project Name for all expensis----------------------
                            expensis.ProjectName = projectName;
                            //--------------------------------------------------------------------
                        }
                        workSheet.Column(1).AutoFit();
                        workSheet.Column(2).AutoFit();
                        workSheet.Column(3).AutoFit();
                        workSheet.Column(4).AutoFit();
                        workSheet.Column(5).AutoFit();
                        workSheet.Column(6).AutoFit();
                        workSheet.Column(7).AutoFit();

                        var path = $"Attachments\\{companyName}\\TaskExpensis";
                        var savedPath = Path.Combine(_host.WebRootPath, path);
                        if (File.Exists(savedPath))
                            File.Delete(savedPath);

                        // Create excel file on physical disk  
                        Directory.CreateDirectory(savedPath);
                        //FileStream objFileStrm = File.Create(savedPath);
                        //objFileStrm.Close();
                        var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                        var excelPath = savedPath + $"\\TaskExpensis_{date}.xlsx";
                        excel.SaveAs(excelPath);
                        // Write content to excel file  
                        //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                        //Close Excel package 
                        excel.Dispose();
                        response.Data.FilePath = Globals.baseURL + '\\' + path + $"\\TaskExpensis_{date}.xlsx";
                    }    
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

        public BaseResponseWithId<long> DeleteTaskExpensis(long Id)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var taskExpensis = _unitOfWork.TaskExpensis.GetById(Id);
            if (taskExpensis == null)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err101";
                error.ErrorMSG = "No Task Expensis  with this ID";
                response.Errors.Add(error);
                return response;
            }
            #endregion

            try
            {

                _unitOfWork.TaskExpensis.Delete(taskExpensis);
                _unitOfWork.Complete();

                response.ID = taskExpensis.Id;
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
