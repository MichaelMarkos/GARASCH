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

        public BaseResponseWithId<long> EditFamily(EditFamilyDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                if(dto.FamilyStatusID != null)
                {

                    var familyStatus = _unitOfWork.FamilyStatus.GetById(dto.FamilyStatusID??0);
                    if (familyStatus == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "No family status with this ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                var family = _unitOfWork.Families.GetById(dto.Id);
                if (family == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "There is no family with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                var repetedFamilyName = _unitOfWork.Families.Find(a => a.FamilyName == dto.FamilyName && a.Id != dto.Id);
                if (repetedFamilyName != null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "There is family with the same name";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                if (dto.FamilyStatusID != null)family.FamilyStatusId = dto.FamilyStatusID??0;
                if(!string.IsNullOrEmpty(dto.FamilyName)) family.FamilyName = dto.FamilyName;
               
                _unitOfWork.Complete();

                response.ID = family.Id;
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

        public BaseResponseWithId<long> AddHrUserFamily(AddHrUserFamilyDTO dto, long userId)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var family = _unitOfWork.Families.GetById(dto.FamilyID);
                if (family == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No family with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                var hruser = _unitOfWork.HrUsers.GetById(dto.HrUserID);
                if (hruser == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "There is no user with this ID";
                    response.Errors.Add(err);
                    return response;
                }

                #endregion
                var newHrUserFamily = new HrUserFamily()
                {
                    HrUserId = dto.HrUserID,
                    FamilyId = dto.FamilyID,
                    IsHeadOfTheFamily = dto.IsHeadOfFamily,
                    CreatedBy = userId,
                    CreationDate = DateTime.Now,
                    ModifiedBy = userId,
                    ModifiedDate = DateTime.Now,
                    Active = true,
                };

                _unitOfWork.HrUserFamilies.Add(newHrUserFamily);
                _unitOfWork.Complete();

                response.ID = newHrUserFamily.Id;
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

        public BaseResponseWithData<List<GetHrUserFamiliesListDTO>> GetHrUserFamiliesList(GetHrUserFamiliesListFilters filters)
        {
            var response = new BaseResponseWithData<List<GetHrUserFamiliesListDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var HrUserfamiliesQueryable = _unitOfWork.HrUserFamilies.FindAllQueryable(a => true, new[] { "ModifiedByNavigation", "CreatedByNavigation" });
                //var t1 = HrUserfamiliesQueryable.ToList();
                if (filters.HrUserID != null)
                {
                    HrUserfamiliesQueryable = HrUserfamiliesQueryable.Where(a => a.HrUserId == filters.HrUserID);
                }

                if (filters.FamilyID != null)
                {
                    HrUserfamiliesQueryable = HrUserfamiliesQueryable.Where(a => a.FamilyId == filters.FamilyID);
                }

                if (filters.IsHeadOfFamily != null)
                {
                    HrUserfamiliesQueryable = HrUserfamiliesQueryable.Where(a => a.IsHeadOfTheFamily == filters.IsHeadOfFamily);
                }

                if (filters.Active != null)
                {
                    HrUserfamiliesQueryable = HrUserfamiliesQueryable.Where(a => a.Active == filters.Active);
                }

                var HrUSerfamiliesListDB = HrUserfamiliesQueryable.ToList();

                //var familiesList = new List<GetFamiliesListDTO>();
                var HrUserfamiliesList = HrUSerfamiliesListDB.Select(a => new GetHrUserFamiliesListDTO()
                {
                    ID = a.Id,
                    HrUserID = a.HrUserId,
                    FamilyID = a.FamilyId,
                    IsHeadOfFamily = a.IsHeadOfTheFamily,
                    Active = a.Active,
                    CreatorID = a.CreatedBy,
                    CreatorName = a.CreatedByNavigation.FirstName + " " +a.CreatedByNavigation.LastName,
                    ModifiedByID = a.ModifiedBy,
                    MOdifiedByName = a.ModifiedByNavigation.FirstName + " " + a.ModifiedByNavigation.LastName,
                }).ToList();

                response.Data = HrUserfamiliesList;
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

        public BaseResponseWithData<GetHrUserFamiliesListDTO> GetHrUserFamilyByID(long HrUserfamilyID)
        {
            var response = new BaseResponseWithData<GetHrUserFamiliesListDTO>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            if(HrUserfamilyID == 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "please enter HrUser Family ID";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var HrUserFamilyDB = _unitOfWork.HrUserFamilies.Find(a => a.Id == HrUserfamilyID, new[] { "ModifiedByNavigation", "CreatedByNavigation" });


                //var familiesList = new List<GetFamiliesListDTO>();
                var familyData = new GetHrUserFamiliesListDTO()
                {
                    ID = HrUserFamilyDB.Id,
                    HrUserID = HrUserFamilyDB.HrUserId,
                    FamilyID = HrUserFamilyDB.FamilyId,
                    IsHeadOfFamily = HrUserFamilyDB.IsHeadOfTheFamily,
                    Active = HrUserFamilyDB.Active,
                    CreatorID = HrUserFamilyDB.CreatedBy,
                    CreatorName = HrUserFamilyDB.CreatedByNavigation.FirstName + " " + HrUserFamilyDB.CreatedByNavigation.LastName,
                    ModifiedByID = HrUserFamilyDB.ModifiedBy,
                    MOdifiedByName = HrUserFamilyDB.ModifiedByNavigation.FirstName + " " + HrUserFamilyDB.ModifiedByNavigation.LastName,
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

        public BaseResponseWithId<long> EditHrUserFamily(EditHrUserFamilyDTO dto, long userId)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var hrUserFamily = _unitOfWork.HrUserFamilies.GetById(dto.Id);
                if (hrUserFamily == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No HrUserFamily with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                if(dto.FamilyID != null)
                {

                    var family = _unitOfWork.Families.GetById(dto.FamilyID??0);
                    if (family == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "No family with this ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                
                if (dto.HrUserID != null)
                {

                    var hruser = _unitOfWork.HrUsers.GetById(dto.HrUserID??0);
                    if (hruser == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = "There is no HrUser with this ID";
                        response.Errors.Add(err);
                        return response;
                    }
                }

                #endregion

                if(dto.HrUserID != null) hrUserFamily.HrUserId = dto.HrUserID ?? 0;
                if (dto.FamilyID != null) hrUserFamily.FamilyId = dto.FamilyID ?? 0;
                if (dto.IsHeadOfFamily != null) hrUserFamily.IsHeadOfTheFamily = dto.IsHeadOfFamily;
                if (dto.Active != null) hrUserFamily.Active = dto.Active??true;
                hrUserFamily.ModifiedDate = DateTime.Now;
                hrUserFamily.ModifiedBy = userId;
                

                _unitOfWork.Complete();

                response.ID = hrUserFamily.Id;
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
