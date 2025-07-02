using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.IdentityModel.Tokens;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DTO.AssetDepreciation;
using NewGaras.Infrastructure.DTO.ChurchAndPriest;
using NewGaras.Infrastructure.DTO.Family;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Quartz.Logging.OperationName;

namespace NewGaras.Domain.Services
{
    public class AssetDepreciationService : IAssetDepreciationService
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
        public AssetDepreciationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public BaseResponseWithId<long> AddProductionUOM(AddProductionUOMDTO dto, long creatorID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            
            try
            {
                var newUOM = new ProductionUom()
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    CreatedBy = creatorID,
                    Active = true,
                    CreationDate = DateTime.Now,
                    ModifiedBy = creatorID,
                    ModificationDate = DateTime.Now,
                };
                _unitOfWork.ProductionUoms.Add(newUOM);
                _unitOfWork.Complete();

                response.ID = newUOM.Id;
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

        public BaseResponseWithId<long> EditProductionUOM(EditProductionUOMDTO dto, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var productionUom = _unitOfWork.ProductionUoms.GetById(dto.ID);
            if (productionUom == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No ProductionUom with this ID";
                response.Errors.Add(err);
                return response;
            }

            #endregion

            try
            {

                if (!string.IsNullOrEmpty(dto.Name)) productionUom.Name = dto.Name;
                if (!string.IsNullOrEmpty(dto.Description)) productionUom.Description = dto.Description;
                if(dto.Active != null) productionUom.Active = dto.Active??true;
                productionUom.ModifiedBy = userID;
                productionUom.ModificationDate = DateTime.Now;


                _unitOfWork.Complete();

                response.ID = productionUom.Id;
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

        public SelectDDLResponse GetProductionUOMDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.ProductionUoms.FindAll(x => true).ToList();
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

        public BaseResponseWithId<long> AddDepreciationType(AddProductionUOMDTO dto, long creatorID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };


            try
            {
                var newDepreciationType = new DepreciationType()
                {
                    Name = dto.Name,
                    Description = dto.Description,
                };
                _unitOfWork.DepreciationTypes.Add(newDepreciationType);
                _unitOfWork.Complete();

                response.ID = newDepreciationType.Id;
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

        public BaseResponseWithId<long> EditDepreciationType(EditProductionUOMDTO dto, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var DepreciationType = _unitOfWork.DepreciationTypes.GetById(dto.ID);
            if (DepreciationType == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Depreciation Type with this ID";
                response.Errors.Add(err);
                return response;
            }

            #endregion

            try
            {

                if (!string.IsNullOrEmpty(dto.Name)) DepreciationType.Name = dto.Name;
                if (!string.IsNullOrEmpty(dto.Description)) DepreciationType.Description = dto.Description;

                _unitOfWork.Complete();

                response.ID = DepreciationType.Id;
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

        public SelectDDLResponse GetDepreciationTypeDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.DepreciationTypes.FindAll(x => true).ToList();
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

        public BaseResponseWithId<long> AddAssetDepreciation(AddAssetDepreciationDTO dto, long creatorID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var church = _unitOfWork.DepreciationTypes.GetById(dto.DepreciationTypeId);
            if (church == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Depreciation Types with this ID";
                response.Errors.Add(err);
                return response;
            }

            var account = _unitOfWork.Accounts.GetById(dto.AccountID);
            if (account == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Account with this ID";
                response.Errors.Add(err);
                return response;
            }

            if(account.Haveitem == true || account.AccountCategoryId == 1)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "you don't have permission ";
                response.Errors.Add(err);
                return response;
            }


            if (dto.ProductionUOMID != null)
            {

                var productionUOM = _unitOfWork.ProductionUoms.GetById(dto.ProductionUOMID??0);
                if (productionUOM == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No production UOM with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var yearOfPurchase = DateTime.Now;
            if (!DateTime.TryParse(dto.YearOfPurchase, out yearOfPurchase))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid year Of Purchase :";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var newAssetDepreciation = new AssetDepreciation()
                {
                    DepreciationTypeId = dto.DepreciationTypeId,
                    CostOfAsset = dto.CostOfAssets,
                    YearOfPurchase = yearOfPurchase,
                    ResidualValue = dto.ResidualValue,
                    ExpectedLifespanPerMonth = dto.ExpectedLifespanPerMonth,
                    ProductionUomid = dto.ProductionUOMID,
                    ProductionUomcount = dto.ProductionUOMCount,
                    RealCost = dto.RealCost,
                    AccountId = dto.AccountID,
                    CreatedBy = creatorID,
                    CreationDate = DateTime.Now,
                    ModifiedBy = creatorID,
                    ModifiedDate = DateTime.Now,
                };
                _unitOfWork.AssetDepreciations.Add(newAssetDepreciation);
                _unitOfWork.Complete();

                response.ID = newAssetDepreciation.Id;
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

        public BaseResponseWithId<long> EditAssetDepreciation(EditAssetDepreciationDTO dto, long userID)
        {
            var response = new BaseResponseWithId<long>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            #region validation
            var AssetDepreciations = _unitOfWork.AssetDepreciations.GetById(dto.ID);
            if (AssetDepreciations == null)
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E101";
                err.ErrorMSG = "No Asset Depreciation with this ID";
                response.Errors.Add(err);
                return response;
            }

            if (dto.ProductionUOMID != null)
            {
                var ProductionUoms = _unitOfWork.ProductionUoms.GetById(dto.ProductionUOMID ?? 0);
                if (ProductionUoms == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Production Uom with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            if (dto.DepreciationTypeId != null)
            {
                var DepreciationType = _unitOfWork.DepreciationTypes.GetById(dto.DepreciationTypeId ?? 0);
                if (DepreciationType == null)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.ErrorMSG = "No Depreciation Type with this ID";
                    response.Errors.Add(err);
                    return response;
                }
            }

            var yearOfPurchase = DateTime.Now;
            if(!string.IsNullOrEmpty(dto.YearOfPurchase))
            {
                if (!DateTime.TryParse(dto.YearOfPurchase, out yearOfPurchase))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid year Of Purchase :";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion

            try
            {
                if (dto.DepreciationTypeId != null) AssetDepreciations.DepreciationTypeId = dto.DepreciationTypeId ?? 0;
                if (dto.CostOfAssets != null) AssetDepreciations.CostOfAsset = dto.CostOfAssets??0;
                if (dto.ResidualValue != null) AssetDepreciations.ResidualValue = dto.ResidualValue ?? 0;
                if (dto.ExpectedLifespanPerMonth != null) AssetDepreciations.ExpectedLifespanPerMonth = dto.ExpectedLifespanPerMonth ?? 0;
                if (dto.ProductionUOMID != null) AssetDepreciations.ProductionUomid = dto.ProductionUOMID ?? 0;
                if (dto.ProductionUOMCount != null) AssetDepreciations.ProductionUomcount = dto.ProductionUOMCount ?? 0;
                if (dto.DepreciationRate != null) AssetDepreciations.DepreciationRate = dto.DepreciationRate ?? 0;
                if (dto.RealCost != null) AssetDepreciations.RealCost = dto.RealCost ?? 0;
                if (!string.IsNullOrEmpty(dto.YearOfPurchase)) AssetDepreciations.YearOfPurchase = yearOfPurchase;
                AssetDepreciations.ModifiedBy = userID;
                AssetDepreciations.ModifiedDate = DateTime.Now;


                _unitOfWork.Complete();

                response.ID = AssetDepreciations.Id;
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

        public BaseResponseWithData<List<GetAssetDepreciationDTO>> GetAssetDepreciation(GetAssetDepreciationFilters filters)
        {
            var response = new BaseResponseWithData<List<GetAssetDepreciationDTO>>()
            {
                Errors = new List<Error>(),
                Result = true
            };

            try
            {
                #region validation
                var purchaseYear = DateTime.Now;
                if(!string.IsNullOrEmpty(filters.YearOfPurchase))
                {
                    if(!DateTime.TryParse(filters.YearOfPurchase, out purchaseYear))
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "please, Enter a valid year Of Purchase :";
                        response.Errors.Add(err);
                        return response;
                    }
                }
                #endregion
                var assetsDepreciationQueryable = _unitOfWork.AssetDepreciations.FindAllQueryable(a => true, new[] { "ProductionUom", "DepreciationType", "ModifiedByNavigation", "CreatedByNavigation" });
                if (!string.IsNullOrEmpty(filters.YearOfPurchase))
                {
                    assetsDepreciationQueryable = assetsDepreciationQueryable.Where(a => a.YearOfPurchase == purchaseYear);
                }

                if (filters.AccountID != null)
                {
                    assetsDepreciationQueryable = assetsDepreciationQueryable.Where(a => a.AccountId == filters.AccountID);
                }

                var assetsDepreciationListDB = assetsDepreciationQueryable.ToList();

                var assetsDepreciationList = assetsDepreciationListDB.Select(a => new GetAssetDepreciationDTO()
                {
                    ID = a.Id,
                    DepreciationTypeID = a.DepreciationTypeId,
                    DepreciationTypeName = a.DepreciationType.Name,
                    CostOfAsset = a.CostOfAsset,
                    YearOfPurchase = a.YearOfPurchase?.ToString("yyyy-MM-dd"),
                    ResidualValue = a.ResidualValue,
                    ExpectedLifespanPerMonth = a.ExpectedLifespanPerMonth,
                    ProductionUOMID = a.ProductionUomid,
                    ProductionUOMName = a.ProductionUom?.Name,
                    ProductionUOMCount = a.ProductionUomcount,
                    DepreciationRate = a.DepreciationRate,
                    RealCost = a.RealCost,
                    CreatedByID = a.CreatedBy,
                    CreatedByName = a.CreatedByNavigation.FirstName + " " + a.CreatedByNavigation.LastName,
                    DateOfCreation = a.CreationDate.ToShortDateString(),
                    ModifiedByID = a.ModifiedBy,
                    ModifiedByName = a.ModifiedByNavigation.FirstName + " " + a.ModifiedByNavigation.LastName,
                    DateOfModification = a.ModifiedDate.ToShortDateString(),
                }).ToList();

                response.Data = assetsDepreciationList;
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
