using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models;
using OfficeOpenXml;
using NewGaras.Infrastructure.DTO.Branch;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.HR;

namespace NewGaras.Domain.Services
{
    public class WeekDayService : IWeekDayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        static readonly string key = "SalesGarasPass";
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
        public WeekDayService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
        }
        public BaseResponseWithData<List<WeekDayDto>> GetWeekDays([FromHeader] int BranchId)
        {
            BaseResponseWithData<List<WeekDayDto>> Response = new BaseResponseWithData<List<WeekDayDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var branch = _unitOfWork.Branches.GetById(BranchId);
                if (branch == null) 
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG ="Branch Not Found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var days = _unitOfWork.WeekDays.FindAll(a=>a.BranchId == BranchId).ToList();
                var list = _mapper.Map<List<WeekDayDto>>(days);
                Response.Data = list;
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

        public BaseResponse UpdateWeekDays(UpdateWeekDaysModel Dto)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (!Dto.WeekDays.Any())
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Empty Data!";
                    Response.Errors.Add(error);
                    return Response;
                }
                foreach(var dto in Dto.WeekDays)
                {
                    if (dto.Id != 0)
                    {
                        var day = _unitOfWork.WeekDays.GetById(dto.Id);
                        _mapper.Map<WeekDayDto, WeekDay>(dto, day);
                        _unitOfWork.WeekDays.Update(day);

                    }
                }
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
    }
}
