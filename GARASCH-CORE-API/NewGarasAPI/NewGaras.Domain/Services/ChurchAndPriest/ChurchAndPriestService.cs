using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.ChurchAndPriest;
using NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Church;
using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services.ChurchAndPriest
{
    public class ChurchAndPriestService : IChurchAndPriestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
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
        public ChurchAndPriestService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<long> AddNewPriest(AddNewPriestDTO dto, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var church = _unitOfWork.Churches.GetById(dto.ChurchID);
            if(church == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Church with this ID";
                response.Errors.Add(err);
                return response;
            }

            if (string.IsNullOrEmpty(dto.PriestName))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "please enter a vaild priest name";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var newPriest = new Priest()
                {
                    PriestName = dto.PriestName,
                    ChurchId = dto.ChurchID,
                    CreatedBy = userID,
                    CreationDate = DateTime.Now,
                    ModifiedBy = userID,
                    ModifiedDate = DateTime.Now,
                };
                _unitOfWork.Priests.Add(newPriest);
                _unitOfWork.Complete();

                response.ID = newPriest.Id;
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

        public BaseResponseWithId<long> EditPriest(EditPriestDTO dto ,long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var priest = _unitOfWork.Priests.GetById(dto.ID);
            if(priest == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Priest with this ID";
                response.Errors.Add(err);
                return response;
            }

            if (dto.ChurchID != null)
            {
                var church = _unitOfWork.Churches.GetById(dto.ChurchID??0);
                if (church == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Church with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }
            
            
            #endregion

            try
            {

                if(!string.IsNullOrEmpty(dto.PriestName))priest.PriestName = dto.PriestName;
                if(dto.ChurchID != null)priest.ChurchId = dto.ChurchID??0;
                priest.ModifiedBy = userID;
                priest.ModifiedDate = DateTime.Now;
                
                
                _unitOfWork.Complete();

                response.ID = priest.Id;
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

        public BaseResponseWithData<List<GetPriestsListDTO>> GetPriestList(GetPriestsListFilters filters)
        {
            var response = new BaseResponseWithData<List<GetPriestsListDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var priestsQueryable = _unitOfWork.Priests.FindAllQueryable(a => true, new[] { "HrUserPriests", "CreatedByNavigation" , "Church", "Church.Eparchy" });

                if (filters.ChurchID != null)
                {
                    priestsQueryable = priestsQueryable.Where(a => a.ChurchId == filters.ChurchID);
                }

                if (filters.EparchyID != null)
                {
                    priestsQueryable = priestsQueryable.Where(a => a.Church.EparchyId == filters.EparchyID);
                }

                if(!string.IsNullOrEmpty(filters.PriestName))
                {
                    priestsQueryable = priestsQueryable.Where(a => a.PriestName.Contains(filters.PriestName));
                }

                var priestsListDB = priestsQueryable.ToList();

                //var familiesList = new List<GetFamiliesListDTO>();
                var familiesList = priestsListDB.Select(a => new GetPriestsListDTO()
                {
                    ID = a.Id,
                    Name = a.PriestName,
                    ChurchID = a.ChurchId,
                    ChurchName = a.Church.ChurchName,
                    EparchyID = a.Church.EparchyId,
                    EparchyName = a.Church.EparchyId != null ? a.Church.Eparchy.Name : null,
                    CreatedBy = a.CreatedBy,
                    creatorName = a.CreatedByNavigation.FirstName + " " + a.CreatedByNavigation.LastName,

                }).ToList();

                response.Data = familiesList;
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
        
    }
}
