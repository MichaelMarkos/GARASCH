using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.BYCompany;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using Microsoft.Extensions.Hosting;

namespace NewGaras.Domain.Services.BYCompany
{
    public class InsuranceCompanyNamesService : IInsuranceCompanyNamesService
    {
        private readonly IMapper _mapper;
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;

        public InsuranceCompanyNamesService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {
            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<long> AddInsuranceCompany(InsuranceCompanyNameDto dto,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (dto != null)
                {
                    if (dto.Name != null && dto.Name != "")
                    {
                        var insurance = _mapper.Map<InsuranceCompanyName>(dto);
                        insurance.Active = true;
                        insurance.CreationDate = DateTime.Now;
                        insurance.CreatedBy = creator;
                        var added = _unitOfWork.InsuranceCompanyNames.Add(insurance);
                        _unitOfWork.Complete();
                        Response.ID = added.Id;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Name is Required";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "please send a valid Data";
                    Response.Errors.Add(error);
                    return Response;
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<List<InsuranceCompanyNameDto>>GetInsuranceCompanies()
        {
            BaseResponseWithData<List<InsuranceCompanyNameDto>> Response = new BaseResponseWithData<List<InsuranceCompanyNameDto>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var companies = new List<InsuranceCompanyNameDto>();
                var dbCompanies = _unitOfWork.InsuranceCompanyNames.GetAll();
                companies = _mapper.Map<List<InsuranceCompanyNameDto>>(dbCompanies);
                Response.Data = companies;

                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err-P12";
                error.ErrorMSG = "please send a valid Data";
                Response.Errors.Add(error);
                return Response;
            }
        }
    }
}
