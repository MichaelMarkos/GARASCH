using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.Family.Filters;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services.Family
{
    public class FamilyService : IFamilyService
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
        public FamilyService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<int> AddFamilyStatus(AddFamilyStatusDTO dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var newFamilyStatus = new FamilyStatus()
                {
                    StatusName = dto.StatusName,
                    Description = dto.Description,
                };
                _unitOfWork.FamilyStatus.Add(newFamilyStatus);
                _unitOfWork.Complete();

                response.ID = newFamilyStatus.Id;
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

        public SelectDDLResponse GetFamilyStatusDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.FamilyStatus.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.StatusName;

                            DDLList.Add(DLLObj);
                        }
                    }
                }
                Response.DDLList = DDLList;
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

        public BaseResponseWithId<long> AddFamily(AddFamilyDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var familyStatus = _unitOfWork.FamilyStatus.GetById(dto.FamilyStatusID);
                if(familyStatus == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No family status with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                var familyName = _unitOfWork.Families.FindAll(a => a.FamilyName == dto.FamilyName).FirstOrDefault();
                if (familyName != null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "There is a family with this name already";
                    response.Errors.Add(err);
                    return response;
                }

                #endregion
                var newFamily = new Infrastructure.Entities.Family()
                {
                    FamilyName = dto.FamilyName,
                    FamilyStatusId = dto.FamilyStatusID
                };
                _unitOfWork.Families.Add(newFamily);
                _unitOfWork.Complete();

                response.ID = newFamily.Id;
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

        public BaseResponseWithData<List<GetFamiliesListDTO>> GetFamiliesList(GetFamiliesListFilters filters)
        {
            var response = new BaseResponseWithData<List<GetFamiliesListDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var familiesQueryable = _unitOfWork.Families.FindAllQueryable(a => true , new[] { "FamilyStatus" } );

                if (!string.IsNullOrEmpty(filters.FamilyName))
                {
                    familiesQueryable = familiesQueryable.Where(a => a.FamilyName.Contains(filters.FamilyName));
                }

                if(filters.FamilyStatusID != null)
                {
                    familiesQueryable = familiesQueryable.Where(a => a.FamilyStatusId == filters.FamilyStatusID);
                }

                var familiesListDB = familiesQueryable.ToList();

                //var familiesList = new List<GetFamiliesListDTO>();
                var familiesList = familiesListDB.Select(a => new GetFamiliesListDTO()
                {
                    ID = a.Id,
                    FamilyName = a.FamilyName,
                    FamilyStatusID = a.FamilyStatusId,
                    FamilyStatusName = a.FamilyStatus.StatusName
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

        public BaseResponseWithData<GetFamiliesListDTO> GetFamilyByID(long familyID)
        {
            var response = new BaseResponseWithData<GetFamiliesListDTO>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var familyDB = _unitOfWork.Families.Find(a => a.Id == familyID, new[] { "FamilyStatus" });

                
                //var familiesList = new List<GetFamiliesListDTO>();
                var familyData = new GetFamiliesListDTO()
                {
                    ID = familyDB.Id,
                    FamilyName = familyDB.FamilyName,
                    FamilyStatusID = familyDB.FamilyStatusId,
                    FamilyStatusName = familyDB.FamilyStatus.StatusName
                };

                response.Data = familyData;
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
