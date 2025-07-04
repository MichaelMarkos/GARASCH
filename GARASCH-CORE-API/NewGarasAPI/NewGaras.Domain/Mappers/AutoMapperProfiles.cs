﻿using AutoMapper;
using NewGaras.Domain.DTO.Salary;
using NewGaras.Infrastructure.DTO.Branch;
using NewGaras.Infrastructure.DTO.Contract;
using NewGaras.Infrastructure.DTO.HrUser;
using NewGaras.Infrastructure.DTO.Department;
using NewGaras.Infrastructure.DTO.Team;
using NewGaras.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.DTO.Salary;
using NewGaras.Infrastructure.DTO.Log;
using NewGaras.Infrastructure.DTO.JobTitle;
using NewGaras.Infrastructure.DTO.Shift;
using NewGaras.Infrastructure.DTO.Salary.SalaryAllownces;
using NewGaras.Infrastructure.DTO.BYCompany;
using NewGaras.Infrastructure.DTO.Salary.SalaryTax;
using NewGaras.Infrastructure.DTO.VacationType;
using NewGaras.Infrastructure.DTO.Salary.SalaryDeduction;
using NewGaras.Infrastructure.DTO.Salary.SalaryDeductionTax;
using NewGaras.Infrastructure.DTO.BranchSetting;
using NewGaras.Infrastructure.DTO.OverTimeAndDeductionRate;
using Microsoft.Extensions.Configuration;
using NewGaras.Infrastructure.DTO.VacationOverTimeAndDeductionRates;
using NewGaras.Infrastructure.DTO.VacationDay;
using NewGaras.Infrastructure.DTO.WorkFlow;
using NewGaras.Infrastructure.DTO.ProjectSprint;
using NewGaras.Infrastructure.DTO.Salary.AllowncesType;
using NewGaras.Infrastructure.DTO.Payment;
using NewGaras.Infrastructure.Models.SalaryAllownce;
using NewGaras.Infrastructure.DTO.TaskUnitRateService;
using NewGaras.Infrastructure.Models.ProjectInvoice;
using NewGaras.Infrastructure.DTO.ProjectInvoiceCollected;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.DTO.Medical.DoctorSchedule;
using NewGaras.Infrastructure.DTO.Medical.MedicalReservation;
using NewGaras.Infrastructure.DTO.Medical.MedicalFinance;
using NewGaras.Infrastructure.Models.HrUser;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.HR;

namespace NewGaras.Domain.Mappers
{
    public class AutoMapperProfiles : Profile
    {
        //private string BaseURL = "https://garascore.garassolutions.com/";
        //private string BaseURL = "https://byapi.garassolutions.com/";
        //var MyConfig = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
        //string BaseURL = MyConfig.GetValue<string>("AppSettings:baseURL");
        string BaseURL = Globals.baseURL;
        public AutoMapperProfiles() 
        {
            CreateMap<AddSalaryDto, Salary>();
            CreateMap<AddContractDto, ContractDetail>();
            CreateMap<HrUserDto, HrUser>();
            CreateMap<HrUserSocialMediaDto, HrUserSocialMedium>();
            CreateMap<HrUserSocialMedium, HrUserSocialMediaDto>().ForMember(a => a.Active, dto => dto.MapFrom(a => true));
            CreateMap<HrUserMobileDto, HrUserMobile>();
            CreateMap<HrUserMobile, HrUserMobileDto>().ForMember(a=>a.Active,dto=>dto.MapFrom(a=>true));
            CreateMap<HrUserLandLineDto, HrUserLandLine>();
            CreateMap<HrUserLandLine, HrUserLandLineDto>().ForMember(a => a.Active, dto => dto.MapFrom(a => true));


            CreateMap<AddBranchDto, Branch>();
            CreateMap<Branch, AddBranchDto>();

            CreateMap<HrUser, GetHrUserDto>()
                .ForMember(HrUser => HrUser.NationalityName, GetDto => GetDto.MapFrom(a => a.Nationality.Nationality1))
                .ForMember(HrUser => HrUser.JobTitleName, GetDto => GetDto.MapFrom(a => a.JobTitle.Name))
                .ForMember(HrUser => HrUser.MilitaryStatusName, GetDto => GetDto.MapFrom(a => a.MilitaryStatus.Name))
                .ForMember(HrUser => HrUser.MaritalStatusName, GetDto => GetDto.MapFrom(a => a.MaritalStatus.Name))
                .ForMember(HrUser => HrUser.ChurchOfPresenceName, GetDto => GetDto.MapFrom(a => a.ChurchOfPresence.ChurchName))
                .ForMember(HrUser => HrUser.BelongToChurchName, GetDto => GetDto.MapFrom(a => a.BelongToChurch.ChurchName))
                .ForMember(HrUser => HrUser.DateOfBirth, GetDto => GetDto.MapFrom(a => a.DateOfBirth.ToString()))
                .ForMember(HrUser => HrUser.CreationDate, GetDto => GetDto.MapFrom(a => a.CreationDate.ToString()))
                .ForMember(HrUser => HrUser.Modified, GetDto => GetDto.MapFrom(a => a.Modified.ToString()))
                .ForMember(HrUser => HrUser.AcademicYearDate, GetDto => GetDto.MapFrom(a => a.AcademicYearDate.ToString()))
                .ForMember(HrUser => HrUser.DateOfDeath, GetDto => GetDto.MapFrom(a => a.DateOfDeath.ToString()))
                .ForMember(HrUser => HrUser.PlaceOfBirthName, GetDto => GetDto.MapFrom(a => a.PlaceOfBirth.Name))
                .ForMember(HrUser => HrUser.ImgPath, GetDto => GetDto.MapFrom(a => Globals.baseURL+"/"+ a.ImgPath));

            CreateMap<HrUserAddress, GetHrUserAddressDto>()
                .ForMember(HrUser => HrUser.CountryName, GetDto => GetDto.MapFrom(a => a.Country.Name))
                .ForMember(HrUser => HrUser.GovernorateName, GetDto => GetDto.MapFrom(a => a.Governorate.Name))
                .ForMember(HrUser => HrUser.CityName, GetDto => GetDto.MapFrom(a => a.City.Name))
                .ForMember(HrUser => HrUser.DistrictName, GetDto => GetDto.MapFrom(a => a.District.DistrictName))
                .ForMember(HrUser => HrUser.AreaName, GetDto => GetDto.MapFrom(a => a.Area.Name))
                .ForMember(HrUser => HrUser.GeographicalName, GetDto => GetDto.MapFrom(a => a.GeographicalName.GeographicalName1))
                .ForMember(HrUser => HrUser.HrUserName, GetDto => GetDto.MapFrom(a => a.HrUser.FirstName+" "+(a.HrUser.MiddleName!=null?a.HrUser.MiddleName+" ":"")+a.HrUser.LastName));

            CreateMap<HrUser, GetHrTeamUsersDto>();
            CreateMap<AddDepartmentDto, Department>();
            CreateMap<Department,GetDepartmentDto>().ForMember(dto=>dto.Teams,o=>o.Ignore());
            CreateMap<TeamDto, Team>();
            CreateMap<Team, GetTeamDto>().ForMember(dto => dto.HrUsers, o => o.Ignore());
            CreateMap<ContractDetail, GetContractDto>()
                .ForMember(contract => contract.ContactTypeName, dto => dto.MapFrom(a => a.ContactType.Name))
                .ForMember(contract => contract.StartDate, dto => dto.MapFrom(a => a.StartDate.ToShortDateString()))
                .ForMember(contract => contract.EndDate, dto => dto.MapFrom(a => a.EndDate.ToShortDateString()));
            CreateMap<Salary, GetSalaryDto>();
            CreateMap<SystemLog, GetSystemLogDto>().ForMember(log=>log.CreatedByName,Dto=>Dto.MapFrom(a=>a.CreatedByNavigation.FirstName+" "+a.CreatedByNavigation.LastName)).ForMember(log=>log.OldValue,dto=>dto.MapFrom(a=>a.ColumnName== "ImgPath"? BaseURL+'/'+a.OldValue:a.OldValue)).ForMember(log => log.NewValue, dto => dto.MapFrom(a => a.ColumnName == "ImgPath" ? BaseURL+'/' + a.NewValue : a.NewValue));

            CreateMap<AddShiftDto,BranchSchedule>();
            CreateMap<BranchSchedule, BranchScheduleDto>();
            CreateMap<InsuranceDto, UserPatientInsurance>();
            CreateMap<HrUserAddress, HrUserAddressDto>().ReverseMap();
            CreateMap<HrUserAttachment, HrUserAttachmentDto>().ReverseMap();
            CreateMap<HrUserAttachment, GetHrUserAttachmentDto>().ForMember(dto => dto.HrUserName, opt => opt.MapFrom(src => src.HrUser.FirstName+" "+src.HrUser.LastName)).
            ForMember(dto => dto.AttachmentTypeName, opt => opt.MapFrom(src => src.AttachmentType.Type)).ForMember(dto => dto.AttachmentPath, opt => opt.MapFrom(src => Globals.baseURL+"/"+ src.AttachmentPath));
            
            













            //------------------------------------------------------Patrick----------------------------------------------------
            CreateMap<SalaryAllownce, GetSalaryAllowncesDto>().ForMember(a => a.AllowncesTypeName, dto => dto.MapFrom(b => b.AllowncesType.Type??null));
            CreateMap<Salary, GetSalaryDto>().ReverseMap();
            CreateMap<Salary, GetSalaryDto>().ForMember(a => a.CurrencyName, dto => dto.MapFrom(x => x.Currency.Name??null)).ForMember(a=>a.SalaryTypeName,dto=>dto.MapFrom(x=>x.PaymentStrategy.Name));
            CreateMap<AddSalaryAllowncesDto, SalaryAllownce>();
            CreateMap<AddSalaryTaxDto, SalaryTax>();
            CreateMap<SalaryTax, GetSalaryTaxDto>();
            CreateMap<EditDeductionTypeDto, DeductionType>().ReverseMap();
            CreateMap<SalaryDeductionTax,AddSalaryDeductionTaxDto>().ReverseMap();
            CreateMap<EditSalaryDeductionTaxDto, SalaryDeductionTax>().ReverseMap();
            CreateMap<AddJobTitleDto, JobTitle>().ForMember(JT => JT.Name, dto => dto.MapFrom(a => a.JobTitleName));
            CreateMap<ProjectWorkFlow, GetWorkFlowDto>().ReverseMap();
            CreateMap<ProjectSprint, GetProjectsprintDto>();
            CreateMap<AddAllowanceTypeDto, AllowncesType>();
            CreateMap<EditAllownceTypeDto, AllowncesType>();
            CreateMap<SalaryAllownce, EditSalaryAllownce>();
            CreateMap<SalaryDeductionTax, GetSalaryTaxDto>();
            CreateMap<BankDetail, GetPaymentForUserDto>().ForMember(BD => BD.BankDetailsID, dto=> dto.MapFrom(a => a.Id))
                .ForMember(BD => BD.AccountHolderFullName, dto => dto.MapFrom(a =>a.AccountHolder));
            CreateMap<TaskUnitRateService, GetTaskUnitRateServiceDto>()
                .ForMember(Tu => Tu.CreatedByName,dto => dto.MapFrom(a => a.CreatedByNavigation != null ? a.CreatedByNavigation.FirstName+ " " + a.CreatedByNavigation.MiddleName + " " + a.CreatedByNavigation.LastName : null))
                .ForMember(Tu => Tu.CreatorImgPath, dto => dto.MapFrom(a =>a.CreatedByNavigation != null ? BaseURL + a.CreatedByNavigation.PhotoUrl : null))
                .ForMember(Tu => Tu.UOMName,dto=>dto.MapFrom(a =>a.Uom.Name.Trim()))
                .ForMember(Tu => Tu.CreationDate, dto=>dto.MapFrom(a =>a.CreationDate.ToString()))
                .ForMember(Tu => Tu.JobtitleID, dto=> dto.MapFrom(a => a.CreatedByNavigation.JobTitleId))
                .ForMember(Tu => Tu.JobtitleName, dto => dto.MapFrom(a => a.CreatedByNavigation.JobTitle.Name));
            CreateMap<ProjectInvoiceCollected, GetProjectInvoiceCollectedDto>()
                .ForMember(a => a.AttachmentPath, dto => dto.MapFrom(a => a.AttachmentPath != null ? BaseURL + a.AttachmentPath : null))
                .ForMember(a => a.PaymentMethodName, dto => dto.MapFrom(a => a.PaymentMethod != null ? a.PaymentMethod.Name : null));

            CreateMap<DoctorScheduleDTO, DoctorSchedule>()
                .ForMember(dto => dto.DoctorSpecialityId, DocSch => DocSch.MapFrom(a => a.TeamID));
                
            CreateMap<AddMedicalExaminationOfferDTO, MedicalExaminationOffer>();
            CreateMap<AddMedicalReservationDTO, MedicalReservation>();
            CreateMap<AddMedicalFinancialDTO, MedicalDailyTreasuryBalance>();
            CreateMap<AddDoctorUserDTO, HrUser>();

            //-----------------------------------------------------------------------------------------------------------------



















            //Gerges Abdullah
            CreateMap<InsuranceCompanyNameDto, InsuranceCompanyName>().ReverseMap();
            CreateMap<ContractLeaveSettingDto, ContractLeaveSetting>().ReverseMap();
            CreateMap<BranchSettingDto, BranchSetting>();
            CreateMap<EditBranchSettingDto, BranchSetting>().ForMember(x => x.Id ,b => b.Ignore()).ForMember(x=>x.PayrollFrom,b=>b.MapFrom(a=>a.PayrollFrom)).ForMember(x => x.PayrollTo, b => b.MapFrom(a => a.PayrollTo));
            CreateMap<BranchSetting, GetBranchSettingDto>();
            CreateMap<OverTimeDeductionRateDto, OverTimeAndDeductionRate>();
            CreateMap<OverTimeAndDeductionRate, OverTimeDeductionRateDto>();
            CreateMap<VacationOverTimeDeductionRateDto, VacationOverTimeAndDeductionRate>().ForMember(dest => dest.ModifiedDate, opt => opt.PreCondition((src, dest) => false));
            CreateMap<VacationOverTimeAndDeductionRate, VacationOverTimeDeductionRateDto>();
            CreateMap<VacationOverTimeDeductionRateDto, VacationOverTimeAndDeductionRate>().ForMember(dto=>dto.Id,o=>o.Ignore()).ForMember(dto => dto.CreationDate, o => o.Ignore());
            CreateMap<AddVacationDayDto,VacationDay>().ReverseMap();
            CreateMap<VacationDay, GetVacationDayDto>().ForMember(v=>v.From,dto=>dto.MapFrom(a=>a.From.ToShortDateString())).ForMember(v => v.To, dto => dto.MapFrom(a => a.To.ToShortDateString()));
            CreateMap<Branch, GetBranchDto>().ForMember(d=>d.Area,s=>s.MapFrom(a=>a.Area.Name)).ForMember(d => d.Country, s => s.MapFrom(a => a.Country.Name)).ForMember(d=>d.Governorate,s=>s.MapFrom(a=>a.Governorate.Name)).ForMember(d => d.AreaId, s => s.MapFrom(a => a.Area.Id))
                .ForMember(d => d.CountryId, s => s.MapFrom(a => a.Country.Id)).ForMember(d => d.GovernorateId, s => s.MapFrom(a => a.Governorate.Id));

            CreateMap<VacationTypesForUser, ContractLeaveEmployee>().ForMember(a=>a.Used,b=>b.Ignore()).ForMember(a => a.Remain, b => b.Ignore());

            CreateMap<ContractLeaveEmployee, VacationTypesForUser>()
                .ForMember(v => v.HolidayName, dto => dto.MapFrom(a => a.ContractLeaveSetting.HolidayName))
                ;

            CreateMap<ProjectInvoice, GetProjectInvoiceModel>()
            .ForMember(a => a.ProjectName, model => model.MapFrom(c => c.Project.SalesOffer.ProjectName))
            .ForMember(a => a.InvoiceDate, model => model.MapFrom(c => c.InvoiceDate.ToShortDateString()))
            .ForMember(a=>a.CurrencyName,model=>model.MapFrom(c=>c.Project.Currency.Name??""));
            



            CreateMap<HrUser, HrUserListDDLModel>().ForMember(a => a.Name, db => db.MapFrom(x => x.FirstName + " " + x.LastName)).ForMember(a => a.ImgPath, db => db.MapFrom(a => a.ImgPath != null ? Globals.baseURL + "/" + a.ImgPath : null));

            CreateMap<Priority, GetPriorityModel>();

            CreateMap<WeekDay, WeekDayDto>();
            CreateMap<WeekDayDto, WeekDay>();

            CreateMap<MedicalDailyTreasuryBalance, MedicalDailyTreasuryBalanceDto>()
            .ForMember(dest => dest.CreatedByName,
                       opt => opt.MapFrom(src => src.CreatedByNavigation != null ? src.CreatedByNavigation.FirstName + " " + src.CreatedByNavigation.LastName : null))
            .ForMember(dest => dest.ModifiedByName,
                       opt => opt.MapFrom(src => src.ModifiedByNavigation != null ? src.ModifiedByNavigation.FirstName + " " + src.ModifiedByNavigation.LastName : null))
            .ForMember(dest => dest.ReceivedFromName,
                       opt => opt.MapFrom(src => src.ReceivedFromNavigation != null ? src.ReceivedFromNavigation.FirstName + " " + src.ReceivedFromNavigation.LastName : null))
            .ForMember(dest => dest.PosNumberName,
                       opt => opt.MapFrom(src => src.PosNumber != null ? src.PosNumber.Serial : null))
            .ForMember(dest => dest.CreationDate, opt => opt.MapFrom(src => src.CreationDate.ToString("dd-MM-yyyy h:m:s tt")))
            .ForMember(dest => dest.ClosingDate, opt => opt.MapFrom(src => src.ClosingDate.ToString("dd-MM-yyyy h:m:s tt")));

        }
    }
}
