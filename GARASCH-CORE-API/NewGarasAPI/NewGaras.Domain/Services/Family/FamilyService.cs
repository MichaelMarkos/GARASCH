using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.Family.Filters;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
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

                var hrusersInFamily = _unitOfWork.HrUserFamilies.FindAll(a => a.FamilyId == familyID, new[] { "Relationship", "HrUser" });

                //var familiesList = new List<GetFamiliesListDTO>();
                var familyData = new GetFamiliesListDTO()
                {
                    ID = familyDB.Id,
                    FamilyName = familyDB.FamilyName,
                    FamilyStatusID = familyDB.FamilyStatusId,
                    FamilyStatusName = familyDB.FamilyStatus.StatusName
                };

                var hruserList = hrusersInFamily.Select(a => new MembersOfFamilyDTO()
                {
                    ID = a.Id,
                    Name = a.HrUser.FirstName +" "+a.HrUser.LastName,
                    RelationID = a.RelationshipId??0,
                    RelationName = a.Relationship?.RelationshipName,
                    Active = a.Active,
                    IsHeadOfTheFamily = a.IsHeadOfTheFamily,
                }).ToList();

                familyData.membersList = hruserList;

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

                var relationship = _unitOfWork.Relationships.GetById(dto.RelationshipID);
                if (relationship == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "There is no relationship with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion
                var newHrUserFamily = new HrUserFamily()
                {
                    HrUserId = dto.HrUserID,
                    FamilyId = dto.FamilyID,
                    IsHeadOfTheFamily = dto.IsHeadOfFamily,
                    RelationshipId = dto.RelationshipID,
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

        public BaseResponseWithDataAndHeader<List<GetHrUserFamiliesListDTO>> GetHrUserFamiliesList(GetHrUserFamiliesListFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetHrUserFamiliesListDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var HrUserfamiliesQueryable = _unitOfWork.HrUserFamilies.FindAllQueryable(a => true, new[] { "ModifiedByNavigation", "CreatedByNavigation", "Relationship" });
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

                var HrUSerfamiliesListDB = PagedList<HrUserFamily>.Create(HrUserfamiliesQueryable,filters.currentPage,filters.numberOfItemsPerPage);

                var pagedheader = new PaginationHeader()
                {
                    CurrentPage = filters.currentPage,
                    ItemsPerPage = filters.numberOfItemsPerPage,
                    TotalItems = HrUSerfamiliesListDB.TotalCount,
                    TotalPages = HrUSerfamiliesListDB.TotalPages
                };
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
                    ModifiedByName = a.ModifiedByNavigation.FirstName + " " + a.ModifiedByNavigation.LastName,
                    RelationshipID = a.RelationshipId,
                    RelationshipName = a.Relationship?.RelationshipName
                }).ToList();

                response.Data = HrUserfamiliesList;
                response.PaginationHeader = pagedheader;
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
                var HrUserFamilyDB = _unitOfWork.HrUserFamilies.Find(a => a.Id == HrUserfamilyID, new[] { "ModifiedByNavigation", "CreatedByNavigation", "Relationship" });


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
                    ModifiedByName = HrUserFamilyDB.ModifiedByNavigation.FirstName + " " + HrUserFamilyDB.ModifiedByNavigation.LastName,
                    RelationshipID = HrUserFamilyDB.RelationshipId,
                    RelationshipName = HrUserFamilyDB.Relationship?.RelationshipName
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

        public BaseResponseWithId<long> AddNewFamilyWithMemebers(AddNewFamilyWithMembersDTO dto, long userID)
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
                if (familyStatus == null)
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

                if(dto.HrUserAndRelationsList.Count() == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Please add family members";
                    response.Errors.Add(err);
                    return response;
                }

                if(dto.headOfFamilyID == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "Please add Head of family member ID";
                    response.Errors.Add(err);
                    return response;
                }
                var hrusesIDList = dto.HrUserAndRelationsList.Select(a => a.ID).ToList();
                if (!hrusesIDList.Contains(dto.headOfFamilyID))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The head of the family must be one of its members (head Id is not in the members list)";
                    response.Errors.Add(err);
                    return response;
                }
                var relationshipIDList = dto.HrUserAndRelationsList.Select(a =>a.RelationshipID).ToList();
                var relationshipListData = _unitOfWork.Relationships.FindAll(a => relationshipIDList.Contains(a.Id));

                var hruserListDB = _unitOfWork.HrUsers.FindAll(a => hrusesIDList.Contains(a.Id)).ToList();
                int count = 1;
                foreach (var hruserRelation in dto.HrUserAndRelationsList)
                {
                    var currentHrUser  = hruserListDB.Where(a => a.Id == hruserRelation.ID).FirstOrDefault();
                    if (currentHrUser == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = $"There is no HrUser with this ID ({hruserRelation.ID}) , recored number {count}";
                        response.Errors.Add(err);
                        return response;
                    }
                    var currentRelationship = relationshipListData.Where(a => a.Id == hruserRelation.RelationshipID).FirstOrDefault();
                    if (currentRelationship == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E101";
                        err.ErrorMSG = $"There is no Relationship with this ID ({hruserRelation.RelationshipID}) , recored number {count}";
                        response.Errors.Add(err);
                        return response;
                    }
                    count++;
                }
                #endregion
                var newFamily = new Infrastructure.Entities.Family()
                {
                    FamilyName = dto.FamilyName,
                    FamilyStatusId = dto.FamilyStatusID
                };
                _unitOfWork.Families.Add(newFamily);
                _unitOfWork.Complete();


                //---------add list of HrUsers to the family-------------------
                var newHruserFamilyMembrers = new List<HrUserFamily>();
                foreach (var hrusRelation in dto.HrUserAndRelationsList)
                {
                    var newHrUserFanily = new HrUserFamily()
                    {
                        HrUserId = hrusRelation.ID,
                        FamilyId = newFamily.Id,
                        RelationshipId = hrusRelation.RelationshipID,
                        Active = true,
                        CreatedBy = userID,
                        CreationDate = DateTime.Now,
                        ModifiedBy = userID,
                        ModifiedDate = DateTime.Now,
                    };

                    if (hrusRelation.ID == dto.headOfFamilyID) newHrUserFanily.IsHeadOfTheFamily = true;
                    newHruserFamilyMembrers.Add(newHrUserFanily);
                }

                _unitOfWork.HrUserFamilies.AddRange(newHruserFamilyMembrers);
                _unitOfWork.Complete();
                //-------------------------------------------------------------

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

        public BaseResponseWithDataAndHeader<List<GetFamilyCardsDTO>> GetFamilyCards(GetFamilyCardsFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetFamilyCardsDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var familiesQueryable = _unitOfWork.Families.FindAllQueryable(a => true, new[] { "FamilyStatus" });       //new[] { "HrUser", "Family", "Family.FamilyStatus" });

                if (!string.IsNullOrEmpty(filters.familyName))
                {
                    familiesQueryable = familiesQueryable.Where(a => a.FamilyName.Contains(filters.familyName));
                }
                
                //if(filters.ChurchOfHeadID != null)
                //{
                //    familiesQueryable = familiesQueryable.Where(a => a.HrUser.BelongToChurchId == filters.ChurchOfHeadID);
                //}

                var familiesList = PagedList<Infrastructure.Entities.Family>.Create(familiesQueryable, filters.currentPage, filters.numberOfItemsPerPage);

                var familiesIDsList = familiesList.Select(a => a.Id).ToList();

                var hruserFamiliesQueryable = _unitOfWork.HrUserFamilies.FindAll(a => familiesIDsList.Contains(a.FamilyId), new[] { "HrUser", "Family", "Family.FamilyStatus", "HrUser.BelongToChurch" });
                //---------filters to be added here ----------------
                if (filters.HeadOfTheFamilyID != null)
                {
                    hruserFamiliesQueryable = hruserFamiliesQueryable.Where(a => a.IsHeadOfTheFamily == true && a.HrUserId == filters.HeadOfTheFamilyID);
                }
                //--------------------------------------------------

                var hruserFamiliesList = hruserFamiliesQueryable.ToList();

                var listOfFamilyCards = new List<GetFamilyCardsDTO>();
                foreach (var family in familiesList)
                {
                    var headOfTheFamily = hruserFamiliesList.Where(a => a.FamilyId == family.Id && a.IsHeadOfTheFamily == true).FirstOrDefault()?.HrUser;
                    var numberOFMembersInFamily = hruserFamiliesList.Where(a => a.FamilyId == family.Id).Count();
                    var churchHeadBelongsTo = hruserFamiliesList.Where(a => a.FamilyId == family.Id && a.IsHeadOfTheFamily == true).FirstOrDefault()?.HrUser?.BelongToChurch;

                    var currentFamily = new GetFamilyCardsDTO();

                    currentFamily.familyId = family.Id;
                    currentFamily.familyName = family.FamilyName;
                    currentFamily.headOfFamilyID = headOfTheFamily?.Id;
                    currentFamily.headOfFamilyName = headOfTheFamily?.FirstName + " " + headOfTheFamily?.LastName;
                    currentFamily.familyStatusID = family.FamilyStatusId;
                    currentFamily.familyStatusName = family.FamilyStatus.StatusName;
                    currentFamily.NUmberOFMembersInFamily = numberOFMembersInFamily;
                    currentFamily.churchOfHeadID = churchHeadBelongsTo?.Id;
                    currentFamily.churchOfHeadName = churchHeadBelongsTo?.ChurchName;

                    listOfFamilyCards.Add(currentFamily);
                }

                var paginationHeader = new PaginationHeader()
                {
                    TotalItems = familiesList.TotalCount,
                    TotalPages = familiesList.TotalPages,
                    CurrentPage = filters.currentPage,
                    ItemsPerPage = filters.numberOfItemsPerPage
                };

                response.Data = listOfFamilyCards;
                response.PaginationHeader = paginationHeader;
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

        public BaseResponseWithId<long> DeleteHrUserFromFamily(DeleteHrUserFromFamilyDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var hruser = _unitOfWork.HrUsers.GetById(dto.HrUserID);
                if (hruser == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Hruser with this ID";
                    response.Errors.Add(err);
                    return response;
                }

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

                var hruserFamily = _unitOfWork.HrUserFamilies.Find(a => a.HrUserId == dto.HrUserID && a.FamilyId == dto.FamilyID);
                if (hruserFamily == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The Hruser is Not in this family already";
                    response.Errors.Add(err);
                    return response;
                }

                if(hruserFamily.IsHeadOfTheFamily == true)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "This Hruser is The head of the family can ot be deleted";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                _unitOfWork.HrUserFamilies.Delete(hruserFamily);
                _unitOfWork.Complete();

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

        public BaseResponseWithId<long> EditHrUserFamilyActive(EditHrUserFamilyActiveDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var hruser = _unitOfWork.HrUsers.GetById(dto.HruserId);
                if (hruser == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Hruser with this ID";
                    response.Errors.Add(err);
                    return response;
                }

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

                var hruserFamily = _unitOfWork.HrUserFamilies.Find(a => a.HrUserId == dto.HruserId && a.FamilyId == dto.FamilyID);
                if (hruserFamily == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The Hruser is Not in this family already";
                    response.Errors.Add(err);
                    return response;
                }
                if (hruserFamily.IsHeadOfTheFamily == true)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "This Hruser is The head of the family can ot be Edited";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion
                hruserFamily.Active = dto.Active;

                _unitOfWork.Complete();


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

        public BaseResponseWithId<long> EditTheHeadOfFamily(EditTheHeadOfFamilyDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var OldHruser = _unitOfWork.HrUsers.GetById(dto.OldHrUserHeadID);
                if (OldHruser == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Hruser with this ID (OldHrUserHeadID)";
                    response.Errors.Add(err);
                    return response;
                }

                var NewHruser = _unitOfWork.HrUsers.GetById(dto.NewHrUserHeadID);
                if (NewHruser == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Hruser with this ID (NewHrUserHeadID)";
                    response.Errors.Add(err);
                    return response;
                }

                var family = _unitOfWork.Families.GetById(dto.familyID);
                if (family == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No family with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                //old head validation
                var OldhruserFamily = _unitOfWork.HrUserFamilies.Find(a => a.HrUserId == dto.OldHrUserHeadID && a.FamilyId == dto.familyID);
                if (OldhruserFamily == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The old Hruser is Not in this family ";
                    response.Errors.Add(err);
                    return response;
                }

                
                if (OldhruserFamily.IsHeadOfTheFamily != true)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The old Hruser is Not the family Head";
                    response.Errors.Add(err);
                    return response;
                }
                //-------------------------------------
                //new head vallidation
                var NewhruserFamily = _unitOfWork.HrUserFamilies.Find(a => a.HrUserId == dto.NewHrUserHeadID && a.FamilyId == dto.familyID);
                if (NewhruserFamily == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The New Hruser is Not in this family ";
                    response.Errors.Add(err);
                    return response;
                }


                if (NewhruserFamily.IsHeadOfTheFamily == true)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The new Hruser is already the family Head";
                    response.Errors.Add(err);
                    return response;
                }

                if (NewhruserFamily.Active == false)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "The new Hruser is not active";
                    response.Errors.Add(err);
                    return response;
                }
                //-------------------------------------

                #endregion
                OldhruserFamily.IsHeadOfTheFamily = false;
                NewhruserFamily.IsHeadOfTheFamily = true;
                _unitOfWork.Complete();

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

        public BaseResponseWithId<int> AddRelationship(AddNewRelationshipDTO dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var newRelationship = new Relationship()
                {
                    RelationshipName = dto.RelationshipName
                };
                _unitOfWork.Relationships.Add(newRelationship);
                _unitOfWork.Complete();

                response.ID = newRelationship.Id;
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

        public SelectDDLResponse GetRelationshipDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.Relationships.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.RelationshipName;

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

        public BaseResponseWithId<int> EditRelationshipName(EditRelationshipDTO dto) 
        {
            var response = new BaseResponseWithId<int>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var relationship = _unitOfWork.Relationships.GetById(dto.Id);
                if (relationship == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Relationship with this ID";
                    response.Errors.Add(err);
                    return response;
                }
                
                #endregion
                relationship.RelationshipName = dto.RelationshipName;
                _unitOfWork.Complete();

                response.ID = relationship.Id;
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
