using AutoMapper;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.ChurchAndPriest;
using NewGaras.Infrastructure.DTO.ChurchAndPriest.Filters;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.DTO.General;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Church;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.User;
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
            if (church == null)
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

        public BaseResponseWithId<long> EditPriest(EditPriestDTO dto, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var priest = _unitOfWork.Priests.GetById(dto.ID);
            if (priest == null)
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
                var church = _unitOfWork.Churches.GetById(dto.ChurchID ?? 0);
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

                if (!string.IsNullOrEmpty(dto.PriestName)) priest.PriestName = dto.PriestName;
                if (dto.ChurchID != null) priest.ChurchId = dto.ChurchID ?? 0;
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

        public BaseResponseWithDataAndHeader<List<GetPriestsListDTO>> GetPriestList(GetPriestsListFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetPriestsListDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var priestsQueryable = _unitOfWork.Priests.FindAllQueryable(a => true, new[] { "HrUserPriests", "CreatedByNavigation", "Church", "Church.Eparchy" });

                if (filters.ChurchID != null)
                {
                    priestsQueryable = priestsQueryable.Where(a => a.ChurchId == filters.ChurchID);
                }

                if (filters.EparchyID != null)
                {
                    priestsQueryable = priestsQueryable.Where(a => a.Church.EparchyId == filters.EparchyID);
                }

                if (!string.IsNullOrEmpty(filters.PriestName))
                {
                    priestsQueryable = priestsQueryable.Where(a => a.PriestName.Contains(filters.PriestName));
                }

                var priestsListDB = PagedList<Priest>.Create(priestsQueryable, filters.currentPage, filters.numberOfItemsPerPage);

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

                var paginationHeader = new PaginationHeader()
                {
                    CurrentPage = filters.currentPage,
                    ItemsPerPage = filters.numberOfItemsPerPage,
                    TotalItems = priestsListDB.TotalCount,
                    TotalPages = priestsListDB.TotalPages,
                };

                response.PaginationHeader = paginationHeader;
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

        public BaseResponseWithId<long> AddNewChurch(AddChurchDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            if (string.IsNullOrEmpty(dto.ChurchName))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "please enter a vaild Church name";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var newChurch = new Church()
                {
                    ChurchName = dto.ChurchName,
                    EparchyId = dto.EparchyID,
                };
                _unitOfWork.Churches.Add(newChurch);
                _unitOfWork.Complete();

                response.ID = newChurch.Id;
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

        public BaseResponseWithId<long> EditChurch(EditChurchDTO dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var church = _unitOfWork.Churches.GetById(dto.ID);
            if (church == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No church with this ID";
                response.Errors.Add(err);
                return response;
            }

            if (dto.EparchyID != null)
            {
                var Eparchy = _unitOfWork.Eparchies.GetById(dto.EparchyID ?? 0);
                if (Eparchy == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Eparchy with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }


            #endregion

            try
            {

                if (!string.IsNullOrEmpty(dto.ChurchName)) church.ChurchName = dto.ChurchName;
                if (dto.EparchyID != null) church.EparchyId = dto.EparchyID ?? 0;


                _unitOfWork.Complete();

                response.ID = church.Id;
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

        public BaseResponseWithDataAndHeader<List<GetChurchesListDTO>> GetChurchesList(GetChurchsListFilters filters)
        {
            var response = new BaseResponseWithDataAndHeader<List<GetChurchesListDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                var churchesQueryable = _unitOfWork.Churches.FindAllQueryable(a => true, new[] { "Eparchy" });

                if (filters.EparchyID != null)
                {
                    churchesQueryable = churchesQueryable.Where(a => a.EparchyId == filters.EparchyID);
                }

                if (!string.IsNullOrEmpty(filters.ChurchName))
                {
                    churchesQueryable = churchesQueryable.Where(a => a.ChurchName.Contains(filters.ChurchName));
                }

                var ChurchesListDB = PagedList<Church>.Create(churchesQueryable,filters.currentPage,filters.numberOfItemsPerPage);

                //var familiesList = new List<GetFamiliesListDTO>();
                var churchesList = ChurchesListDB.Select(a => new GetChurchesListDTO()
                {
                    ID = a.Id,
                    ChurchName = a.ChurchName,
                    EparchyID = a.EparchyId,
                    EparchyName = a.EparchyId != null ? a.Eparchy.Name : null,
                    
                }).ToList();

                var paginationHeader = new PaginationHeader()
                {
                    CurrentPage = filters.currentPage,
                    ItemsPerPage = filters.numberOfItemsPerPage,
                    TotalItems = ChurchesListDB.TotalCount,
                    TotalPages = ChurchesListDB.TotalPages,
                };

                response.PaginationHeader = paginationHeader;
                response.Data = churchesList;
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

        public BaseResponseWithId<int> AddNewEparchy(AddEparchyDTO dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            if (string.IsNullOrEmpty(dto.eparchyName))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "please enter a vaild eparchy name";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var newChurch = new Eparchy()
                {
                    Name = dto.eparchyName,
                };
                _unitOfWork.Eparchies.Add(newChurch);
                _unitOfWork.Complete();

                response.ID = newChurch.Id;
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

        public SelectDDLResponse GetEparchyDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.Eparchies.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Name;

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

        public BaseResponseWithId<int> EditEparchy(EditEparchyDTO dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var Eparchy = _unitOfWork.Eparchies.GetById(dto.ID);
            if (Eparchy == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Eparchy with this ID";
                response.Errors.Add(err);
                return response;
            }


            #endregion

            try
            {

                if (!string.IsNullOrEmpty(dto.Name)) Eparchy.Name = dto.Name;
                


                _unitOfWork.Complete();

                response.ID = Eparchy.Id;
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


        public BaseResponseWithData<List<GetHrUserPriestHistoryDTO>> GetHrUserPriestHistory(GetHrUserPriestHistoryFilters filters)
        {
            var response = new BaseResponseWithData<List<GetHrUserPriestHistoryDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                if(filters.HrUserId == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "please enter a vaild HrUser ID";
                    response.Errors.Add(err);
                    return response;
                }
                #endregion

                var hrUserPriestsQueryable = _unitOfWork.HrUserPriests.FindAllQueryable(a => a.HrUserId == filters.HrUserId, new[] { "Priest"});

                if (filters.PriestID != null)
                {
                    hrUserPriestsQueryable = hrUserPriestsQueryable.Where(a => a.PriestId == filters.PriestID);
                }

                

                var hrUserPriestsListDB = hrUserPriestsQueryable.ToList();

                //var familiesList = new List<GetFamiliesListDTO>();
                var familiesList = hrUserPriestsListDB.Select(a => new GetHrUserPriestHistoryDTO()
                {
                    ID = a.Id,
                    HrUserID = a.HrUserId,
                    PriestID = a.PriestId,
                    PriestName = a.Priest.PriestName,
                    IsCurrent = a.IsCurrent,
                    DateFrom = a.DateFrom.ToString("yyyy-MM-dd"),
                    DateTo = a.DateTo?.ToString("yyyy-MM-dd"),
                    Reason = a.Reason,

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

        public BaseResponseWithDataAndHeader<List<GetEparchyWithChurchDTO>> GetEparchyWithChurch(GetEparchyWithChurchFilters filters)
        {
            BaseResponseWithDataAndHeader<List<GetEparchyWithChurchDTO>> Response = new BaseResponseWithDataAndHeader<List<GetEparchyWithChurchDTO>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var EparchiesList = new List<GetEparchyWithChurchDTO>();
                if (Response.Result)
                {
                    var EparchiesQueryable = _unitOfWork.Eparchies.FindAllQueryable(x => true, new[] { "Churches" });
                    if (!string.IsNullOrEmpty(filters.Name))
                    {
                        EparchiesQueryable = EparchiesQueryable.Where(a => a.Name.Contains(filters.Name));
                    }
                    //var churches = _unitOfWork.Churches.GetAll().ToList();  

                    var EparchiesDB = PagedList<Eparchy>.Create(EparchiesQueryable,filters.currentPage,filters.numberOfItemsPerPage);
                    if (EparchiesDB.Count > 0)
                    {
                        foreach (var item in EparchiesDB)
                        {
                            var DLLObj = new GetEparchyWithChurchDTO();
                            DLLObj.ID = item.Id;
                            DLLObj.EparchyName = item.Name;
                            DLLObj.NumberOfChurchs = item.Churches.Count();

                            EparchiesList.Add(DLLObj);
                        }
                    }

                    var pagiantionHeader = new PaginationHeader()
                    {
                        CurrentPage = filters.currentPage,
                        ItemsPerPage = filters.numberOfItemsPerPage,
                        TotalItems = EparchiesDB.TotalCount,
                        TotalPages = EparchiesDB.TotalPages
                    };
                    Response.PaginationHeader = pagiantionHeader;
                }
                Response.Data = EparchiesList;
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

        public BaseResponseWithId<int> DeleteEparchy(GeneralDeleteDTO<int> dto)
        {
            var response = new BaseResponseWithId<int>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var Eparchy = _unitOfWork.Eparchies.Find(a => a.Id == dto.ID, new[] { "Churches" });
            if (Eparchy == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Eparchy with this ID";
                response.Errors.Add(err);
                return response;
            }
            if (Eparchy.Churches.Count() != 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "There are Churches in this Eparchy, can not be deleted";
                response.Errors.Add(err);
                return response;
            }

            #endregion

            try
            {
                response.ID = Eparchy.Id;

                _unitOfWork.Eparchies.Delete(Eparchy);
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

        public BaseResponseWithId<long> DeleteChurch(GeneralDeleteDTO<long> dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var Church = _unitOfWork.Churches.Find(a => a.Id == dto.ID, new[] { "Priests", "HrUserChurchOfPresences", "HrUserBelongToChurches" });
            if (Church == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Church with this ID";
                response.Errors.Add(err);
                return response;
            }
            if (Church.Priests.Count() != 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "There are Priests in this Church, can not be deleted";
                response.Errors.Add(err);
                return response;
            }
            if (Church.HrUserChurchOfPresences.Count() != 0 || Church.HrUserBelongToChurches.Count() > 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "There are HrUsers in this Church, can not be deleted";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                response.ID = Church.Id;

                _unitOfWork.Churches.Delete(Church);
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

        public BaseResponseWithId<long> DeletePriest(GeneralDeleteDTO<long> dto)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var priest = _unitOfWork.Priests.Find(a => a.Id == dto.ID, new[] { "HrUserPriests" });
            if (priest == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Church with this ID";
                response.Errors.Add(err);
                return response;
            }
            if (priest.HrUserPriests.Count() != 0)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "There are HrUsers with this Priest, can not be deleted";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                response.ID = priest.Id;

                _unitOfWork.Priests.Delete(priest);
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
    }
}
