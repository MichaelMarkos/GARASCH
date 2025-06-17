using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.Vehicle;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Infrastructure.Entities;
using System.Net;
using NewGaras.Domain.Models;
using Microsoft.EntityFrameworkCore;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.User;
using NewGarasAPI.Helper;
using Org.BouncyCastle.Bcpg;
using NewGaras.Infrastructure.Models.Vehicle.UsedInResponse;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models.Maintenance;
using NewGaras.Infrastructure.Models.Supplier;
using System.Web;
using NewGaras.Infrastructure.Models.Vehicle.filters;

namespace NewGaras.Domain.Services
{
    public class VehicleService : IVehicleService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";

        public VehicleService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;

        }
        public async Task<GetVehiclePerBrandResponse> GetVehicleModel([FromHeader] long VehicleBrandId)
        {
            GetVehiclePerBrandResponse response = new GetVehiclePerBrandResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {

                var GetVehiclePerBrandList = new List<VehiclePerBrandData>();
                if (response.Result)
                {
                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var GetVehiclePerBrandDB = await _unitOfWork.VehicleModels.GetAllAsync();
                        if (VehicleBrandId != 0)
                        {
                            GetVehiclePerBrandDB = GetVehiclePerBrandDB.Where(x => x.VehicleBrandId == VehicleBrandId).ToList();
                        }

                        if (GetVehiclePerBrandDB != null && GetVehiclePerBrandDB.Count() > 0)
                        {
                            var brands = _unitOfWork.VehicleBrands.FindAll(a => GetVehiclePerBrandDB.Select(x => x.VehicleBrandId).Contains(a.Id)).ToList();
                            foreach (var GetVehiclePerBrandOBJ in GetVehiclePerBrandDB)
                            {
                                var GetVehiclePerBrandOBJResponse = new VehiclePerBrandData();

                                GetVehiclePerBrandOBJResponse.ID = GetVehiclePerBrandOBJ.Id;
                                GetVehiclePerBrandOBJResponse.VehicleBrandID = GetVehiclePerBrandOBJ.VehicleBrandId;

                                GetVehiclePerBrandOBJResponse.Name = GetVehiclePerBrandOBJ.Name;

                                GetVehiclePerBrandOBJResponse.ModelName = brands.Where(a => a.Id == GetVehiclePerBrandOBJ.VehicleBrandId).Select(a => a.Name).FirstOrDefault();

                                GetVehiclePerBrandOBJResponse.Active = GetVehiclePerBrandOBJ.Active;

                                GetVehiclePerBrandList.Add(GetVehiclePerBrandOBJResponse);
                            }



                        }

                    }

                }
                response.VehiclePerBrandList = GetVehiclePerBrandList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public BaseResponseWithId<long> AddEditVehicleModel(VehiclePerBrandData Request,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrEmpty(Request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }

                    long VehicleBrandID = 0;

                    if (Request.VehicleBrandID != 0 && Request.VehicleBrandID != null)
                    {

                        VehicleBrandID = (long)Request.VehicleBrandID;
                        var VehicleBrandIDDB = _unitOfWork.VehicleBrands.FindAll(a => a.Id == VehicleBrandID).FirstOrDefault();

                        if (VehicleBrandIDDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Vehicle Brand Doesn't Exist";
                            Response.Errors.Add(error);
                        }

                    }

                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Vehicle Brand Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        if (Request.ID != null && Request.ID != 0)
                        {
                            var VehicleBrandDB = _unitOfWork.VehicleModels.GetById((int)Request.ID);
                            if (VehicleBrandDB != null)
                            {
                                // Update

                                VehicleBrandDB.VehicleBrandId = (int)Request.VehicleBrandID;
                                VehicleBrandDB.Name = Request.Name;
                                VehicleBrandDB.Active = Request.Active;
                                VehicleBrandDB.ModifiedBy = creator;
                                VehicleBrandDB.ModifiedDate = DateTime.Now;

                                var VehicleBrandDBUpdate = _unitOfWork.Complete();

                                var DbBodyTypes = _unitOfWork.VehicleBodyTypes.FindAll(x => x.VehicleModelId == Request.ID);
                                var dbBodyTypesNames = DbBodyTypes.Select(x => x.Name).ToList();
                                foreach (var x in dbBodyTypesNames)
                                {
                                    x.Trim().ToLower();
                                }

                                var bodyTypesList = DbBodyTypes.ToList();


                                foreach (var i in Request.VehicleBodyTypeList)
                                {
                                    if (!dbBodyTypesNames.Contains(i.Name.Trim().ToLower()))
                                    {
                                        var bodytyp = new VehicleBodyType()
                                        {
                                            Name = i.Name,
                                            VehicleModelId = (int)Request.ID,
                                            Active = true,
                                            CreatedBy = creator,
                                            CreationDate = DateTime.Now,
                                        };
                                        _unitOfWork.VehicleBodyTypes.Add(bodytyp);
                                        _unitOfWork.Complete();
                                    }
                                }
                                _unitOfWork.Complete();

                                if (VehicleBrandDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = Request.ID ?? 0;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Vehicle Brand !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Vehicle Brand  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            // Insert

                            var vehicle = new VehicleModel()
                            {
                                Name = Request.Name,
                                Active = Request.Active,
                                CreatedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedBy = creator,
                                ModifiedDate = DateTime.Now
                            };
                            _unitOfWork.VehicleModels.Add(vehicle);

                            var VehicleBrandIDInsert = _unitOfWork.Complete();
                            foreach (var i in Request.VehicleBodyTypeList)
                            {
                                var bodytype = new VehicleBodyType()
                                {
                                    Name = i.Name,
                                    VehicleModelId = (int)Request.ID,
                                    Active = true,
                                    CreatedBy = creator,
                                    CreationDate = DateTime.Now,
                                };
                                _unitOfWork.VehicleBodyTypes.Add(bodytype);
                                _unitOfWork.Complete();
                            }


                            if (VehicleBrandIDInsert > 0)
                            {
                                
                                Response.ID = vehicle.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Vehicle Brand !!";
                                Response.Errors.Add(error);
                            }
                        }



                    }


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

        public async Task<GetVehiclePerCategoryResponse> GetVehiclePerCategory()
        {
            GetVehiclePerCategoryResponse response = new GetVehiclePerCategoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetVehicleBrandPerModelList = new List<TreeView>();
                if (response.Result)
                {

                    var VehicalMainCategory = (await _unitOfWork.VehicleServiceScheduleCategories.FindAllAsync(x => x.ParentId == null)).ToList();
                    var TreeDtoObj = VehicalMainCategory.Select(c => new TreeViewDto
                    {
                        id = c.Id.ToString(),
                        title = c.ItemName,
                        parentId = "",
                        HasChild = c.HasChild
                    }).ToList();

                    var VehicalSubCategory = (await _unitOfWork.VehicleServiceScheduleCategories.FindAllAsync(x => x.ParentId != null)).ToList();
                    var modelsDto = VehicalSubCategory.Select(c => new TreeViewDto
                    {
                        id = c.Id.ToString(),
                        title = c.ItemName,
                        parentId = c.ParentId.ToString(),
                        HasChild = c.HasChild
                    }).ToList();


                    TreeDtoObj.AddRange(modelsDto);

                    var trees = Common.BuildTreeViews("", TreeDtoObj);
                    response.GetVehiclePerCategoryList = trees;
                }




                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public BaseResponseWithId<long> AddEditVehiclePerCategory(VehiclePerCategoryData Request,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (string.IsNullOrEmpty(Request.ItemName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = " Name Is Mandatory";
                        Response.Errors.Add(error);
                    }
                    if (Response.Result)
                    {
                        if (Request.ID != null && Request.ID != 0)
                        {

                            var VehiclePerCategoryDB = _unitOfWork.VehicleServiceScheduleCategories.GetById((long)Request.ID);
                            if (VehiclePerCategoryDB != null)
                            {

                                VehiclePerCategoryDB.ItemName = Request.ItemName;
                                VehiclePerCategoryDB.ParentId = Request.ParentID;
                                VehiclePerCategoryDB.HasChild = Request.HasChild;
                                VehiclePerCategoryDB.Active = Request.Active;
                                VehiclePerCategoryDB.ModifiedBy = creator;
                                VehiclePerCategoryDB.ModifiedDate = DateTime.Now;

                                _unitOfWork.VehicleServiceScheduleCategories.Update(VehiclePerCategoryDB);
                                var VehiclePerCategoryDBUpdate = _unitOfWork.Complete();

                                if (VehiclePerCategoryDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = VehiclePerCategoryDB.Id;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Failed To Update this Vehicle Category !!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Vehicle Category  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            var category = new VehicleServiceScheduleCategory()
                            {
                                ItemName = Request.ItemName,
                                ParentId = Request.ParentID,
                                HasChild = Request.HasChild,
                                Active = Request.Active,
                                CreatedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedBy = creator,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.VehicleServiceScheduleCategories.Add(category);
                            var VehiclePerCategoryIDInsert = _unitOfWork.Complete();
                            if (VehiclePerCategoryIDInsert > 0)
                            {
                                Response.ID = category.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Failed To Insert this Vehicle Category !!";
                                Response.Errors.Add(error);
                            }
                        }
                    }
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

        public async Task<GetVehicleBrandPerModelResponse> GetVehicleBrandPerModel()
        {
            GetVehicleBrandPerModelResponse response = new GetVehicleBrandPerModelResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetVehicleBrandPerModelList = new List<TreeView>();
                if (response.Result)
                {

                    var brands = await _unitOfWork.VehicleBrands.GetAllAsync();
                    var TreeDtoObj = brands.Select(c => new TreeViewDto
                    {
                        id = "Brand-" + c.Id.ToString(),
                        title = c.Name,
                        parentId = ""
                    }).ToList();

                    var models = await _unitOfWork.VehicleModels.GetAllAsync();
                    var modelsDto = models.Select(c => new TreeViewDto
                    {
                        id = "Model-" + c.Id.ToString(),
                        title = c.Name,
                        parentId = "Brand-" + c.VehicleBrandId.ToString()
                    }).ToList();


                    TreeDtoObj.AddRange(modelsDto);

                    var trees = Common.BuildTreeViews("", TreeDtoObj);
                    response.GetVehicleBrandPerModelList = trees;
                }



                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }


        public BaseResponseWithId<long> AddEditVehicleBrand(VehicleBrandData Request,long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }



                    if (string.IsNullOrEmpty(Request.Name))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Vehicle Brand Name Is Mandatory";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)
                    {
                        if (Request.ID != null && Request.ID != 0)
                        {
                            var VehicleBrandDB = _unitOfWork.VehicleBrands.GetById((int)Request.ID);
                            if (VehicleBrandDB != null)
                            {
                                // Update
                                VehicleBrandDB.Name = Request.Name;
                                VehicleBrandDB.Active = Request.Active;
                                VehicleBrandDB.ModifiedBy = creator;
                                VehicleBrandDB.ModifiedDate = DateTime.Now;
                                _unitOfWork.VehicleBrands.Update(VehicleBrandDB);
                                var VehicleBrandDBUpdate = _unitOfWork.Complete();
                                if (VehicleBrandDBUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = VehicleBrandDB.Id;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Vehicle Brand!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Vehicle Brand  Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            var brand = new VehicleBrand()
                            {
                                Name = Request.Name,
                                Active = Request.Active,
                                CreatedBy = creator,
                                CreationDate = DateTime.Now,
                                ModifiedBy = creator,
                                ModifiedDate = DateTime.Now,
                            };
                            _unitOfWork.VehicleBrands.Add(brand);
                            var VehicleBrandInsert = _unitOfWork.Complete();


                            if (VehicleBrandInsert > 0)
                            {
                                Response.ID = brand.Id;
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Faild To Insert this Vehicle Brand!!";
                                Response.Errors.Add(error);
                            }
                        }



                    }




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

        public SelectDDLResponse GetVehicleBrandType()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var SelectDDLList = new List<SelectDDL>();
                if (response.Result)
                {
                    var ListDB = _unitOfWork.VehicleBrands.GetAll();
                    if (ListDB != null)
                    {
                        foreach (var item in ListDB)
                        {
                            var SelectDDL = new SelectDDL();
                            SelectDDL.ID = item.Id;
                            SelectDDL.Name = item.Name;
                            SelectDDLList.Add(SelectDDL);
                        }

                    }

                }


                response.DDLList = SelectDDLList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }


        public SelectDDLResponse GetVehicleBodyType([FromHeader] long VehicleModelId)
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var SelectDDLList = new List<SelectDDL>();

                if (response.Result)
                {
                    
                    var ListDB = _unitOfWork.VehicleBodyTypes.GetAll();
                    if (VehicleModelId != 0)
                    {
                        ListDB = ListDB.Where(x => x.VehicleModelId == VehicleModelId).ToList();
                    }
                    if (ListDB != null)
                    {
                        foreach (var item in ListDB)
                        {
                            var SelectDDL = new SelectDDL();
                            SelectDDL.ID = item.Id;
                            SelectDDL.Name = item.Name;
                            SelectDDLList.Add(SelectDDL);
                        }
                    }
                }
                response.DDLList = SelectDDLList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }
        }

        public SelectDDLResponse GetVehicleColorList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var SelectDDLList = new List<SelectDDL>();

                if (response.Result)
                {

                    var ListDB = _unitOfWork.VehicleColors.GetAll();
                    if (ListDB != null)
                    {
                        foreach (var item in ListDB)
                        {
                            var SelectDDL = new SelectDDL();
                            SelectDDL.ID = item.Id;
                            SelectDDL.Name = item.Name;
                            SelectDDLList.Add(SelectDDL);
                        }

                    }
                }
                response.DDLList = SelectDDLList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public SelectDDLResponse GetVehicleTransmissionList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var SelectDDLList = new List<SelectDDL>();

                if (response.Result)
                {
                    var ListDB = _unitOfWork.VehicleTransmissions.GetAll();
                    if (ListDB != null)
                    {
                        foreach (var item in ListDB)
                        {
                            var SelectDDL = new SelectDDL();
                            SelectDDL.ID = item.Id;
                            SelectDDL.Name = item.Name;
                            SelectDDLList.Add(SelectDDL);
                        }
                    }
                }
                response.DDLList = SelectDDLList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public SelectDDLResponse GetVehicleWheelsDriveList()
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var SelectDDLList = new List<SelectDDL>();

                if (response.Result)
                {
                    var ListDB = _unitOfWork.VehicleWheelsDrives.GetAll();
                    if (ListDB != null)
                    {
                        foreach (var item in ListDB)
                        {
                            var SelectDDL = new SelectDDL();
                            SelectDDL.ID = item.Id;
                            SelectDDL.Name = item.Name;
                            SelectDDLList.Add(SelectDDL);
                        }
                    }
                }
                response.DDLList = SelectDDLList;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public async Task<GetVehicleCategoryResponse> GetVehicleServiceCategoryDDL()
        {
            GetVehicleCategoryResponse response = new GetVehicleCategoryResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var VehiclePerCategoryDataDDl = new List<VehiclePerCategoryData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var VehicleServiceScheduleCategoriesDDlDB = (await _unitOfWork.VehicleServiceScheduleCategories.FindAllAsync(x => x.HasChild == true)).ToList();


                        if (VehicleServiceScheduleCategoriesDDlDB != null && VehicleServiceScheduleCategoriesDDlDB.Count() > 0)
                        {

                            foreach (var VehicleServiceScheduleCategoriesDDlOBJ in VehicleServiceScheduleCategoriesDDlDB)
                            {
                                var GetVehicleServiceScheduleCategoriesDDlResponse = new VehiclePerCategoryData();

                                GetVehicleServiceScheduleCategoriesDDlResponse.ID = VehicleServiceScheduleCategoriesDDlOBJ.Id;

                                GetVehicleServiceScheduleCategoriesDDlResponse.ItemName = VehicleServiceScheduleCategoriesDDlOBJ.ItemName;

                                GetVehicleServiceScheduleCategoriesDDlResponse.HasChild = VehicleServiceScheduleCategoriesDDlOBJ.HasChild;
                                GetVehicleServiceScheduleCategoriesDDlResponse.ParentID = VehicleServiceScheduleCategoriesDDlOBJ.ParentId;
                                GetVehicleServiceScheduleCategoriesDDlResponse.Active = VehicleServiceScheduleCategoriesDDlOBJ.Active;





                                VehiclePerCategoryDataDDl.Add(GetVehicleServiceScheduleCategoriesDDlResponse);
                            }



                        }

                    }

                }
                response.VehicleCategoryList2 = VehiclePerCategoryDataDDl;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException.Message;
                response.Errors.Add(error);

                return response;
            }

        }

        public BaseResponseWithID AddNewClientVehicle(AddNewVehicle Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.ClientVehicle != null)
                    {
                        long ClientId = 0;
                        if (Request.ClientVehicle.ClientId != 0)
                        {
                            ClientId = Request.ClientVehicle.ClientId;
                            var client = _unitOfWork.Clients.GetById(ClientId);
                            if (client == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Client Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Client Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        string VIN = null;
                        if (!string.IsNullOrEmpty(Request.ClientVehicle.VIN))
                        {
                            VIN = Request.ClientVehicle.VIN;
                            var ExistVIN = _unitOfWork.VehiclePerClients.Find(a => a.Vin.ToLower() == VIN.ToLower());
                            if (ExistVIN != null)
                            {
                                if (Request.ClientVehicle.Id != null && Request.ClientVehicle.Id != 0)
                                {
                                    if (ExistVIN.Id != (long)Request.ClientVehicle.Id)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "VINErr";
                                        error.ErrorMSG = "This VIN Number Is Exist Before";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "VINErr";
                                    error.ErrorMSG = "This VIN Number Is Exist Before";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "VIN Number Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        int BrandId = 0;
                        if (Request.ClientVehicle.BrandId != 0)
                        {
                            BrandId = Request.ClientVehicle.BrandId;
                            var brand = _unitOfWork.VehicleBrands.GetById(BrandId);
                            if (brand == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Brand Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Brand Id Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        int ColorId = 0;
                        if (Request.ClientVehicle.ColorId != 0 && Request.ClientVehicle.ColorId != null)
                        {
                            ColorId = Request.ClientVehicle.ColorId ?? 0;
                            var color = _unitOfWork.VehicleColors.GetById(ColorId);
                            if (color == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Color Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int ModelId = 0;
                        if (Request.ClientVehicle.ModelId != 0 && Request.ClientVehicle.ModelId != null)
                        {
                            ModelId = Request.ClientVehicle.ModelId ?? 0;
                            var model = _unitOfWork.VehicleModels.GetById(ModelId);
                            if (model == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Model Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int BodyTypeId = 0;
                        if (Request.ClientVehicle.BodyTypeId != 0 && Request.ClientVehicle.BodyTypeId != null)
                        {
                            BodyTypeId = Request.ClientVehicle.BodyTypeId ?? 0;
                            var bodyType = _unitOfWork.VehicleBodyTypes.GetById(BodyTypeId);
                            if (bodyType == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This BodyType Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int IssuerId = 0;
                        if (Request.ClientVehicle.IssuerId != 0 && Request.ClientVehicle.IssuerId != null)
                        {
                            IssuerId = Request.ClientVehicle.IssuerId ?? 0;
                            var issuer = _unitOfWork.VehicleIssuers.GetById(IssuerId);
                            if (issuer == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Issuer Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int WheelDriveId = 0;
                        if (Request.ClientVehicle.WheelsDriveId != 0 && Request.ClientVehicle.WheelsDriveId != null)
                        {
                            WheelDriveId = Request.ClientVehicle.WheelsDriveId ?? 0;
                            var wheelDrive = _unitOfWork.VehicleWheelsDrives.GetById(WheelDriveId);
                            if (wheelDrive == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Wheels Drive Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int TransmissionId = 0;
                        if (Request.ClientVehicle.TransmissionId != 0 && Request.ClientVehicle.TransmissionId != null)
                        {
                            TransmissionId = Request.ClientVehicle.TransmissionId ?? 0;
                            var transmission = _unitOfWork.VehicleTransmissions.GetById(TransmissionId);
                            if (transmission == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Transmission Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int CountryId = 0;
                        if (Request.ClientVehicle.CountryId != 0 && Request.ClientVehicle.CountryId != null)
                        {
                            CountryId = Request.ClientVehicle.CountryId ?? 0;
                            var country = _unitOfWork.Countries.GetById(CountryId);
                            if (country == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Country Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        int GovernorateId = 0;
                        if (Request.ClientVehicle.CityId != 0 && Request.ClientVehicle.CityId != null)
                        {
                            GovernorateId = Request.ClientVehicle.CityId ?? 0;
                            var governorate = _unitOfWork.Governorates.GetById(GovernorateId);
                            if (governorate == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This City Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        string Year = null;
                        if (!string.IsNullOrEmpty(Request.ClientVehicle.Year))
                        {
                            Year = Request.ClientVehicle.Year;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Year Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        if (!string.IsNullOrEmpty(Request.ClientVehicle.ChassisNumber))
                        {
                            if (Request.ClientVehicle.ChassisNumber.Length != 17)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "Chassis Length must be 17 Characters";
                                Response.Errors.Add(error);
                            }
                        }


                        string PlateNumber = null;
                        if (!string.IsNullOrEmpty(Request.ClientVehicle.PlateNumber))
                        {
                            PlateNumber = Request.ClientVehicle.PlateNumber;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Plate Number Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        string CreatedByString = null;
                        string ModifiedByString = null;
                        long? CreatedBy = 0;
                        long? ModifiedBy = 0;
                        if (Request.ClientVehicle.Id != 0 && Request.ClientVehicle.Id != null)
                        {
                            //if (Request.ClientVehicle.ModifiedBy != null)
                            //{
                            //    ModifiedByString = Request.ClientVehicle.ModifiedBy;
                            //}
                            //else
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err25";
                            //    error.ErrorMSG = "Modified By Id Is Mandatory";
                            //    Response.Errors.Add(error);
                            //}
                            ModifiedBy = UserID;
                            var user = _unitOfWork.Users.GetById(ModifiedBy??0);
                            if (user == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Modifier User Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            //if (Request.ClientVehicle.CreatedBy != null)
                            //{
                            //    CreatedByString = Request.ClientVehicle.CreatedBy;
                            //}
                            //else
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err25";
                            //    error.ErrorMSG = "Created By Id Is Mandatory";
                            //    Response.Errors.Add(error);
                            //}
                            CreatedBy = UserID;
                            var user = _unitOfWork.Users.GetById(CreatedBy??0);
                            if (user == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Creator User Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        if (Response.Result)
                        {
                            if (Request.ClientVehicle.Id != null && Request.ClientVehicle.Id != 0)
                            {
                                var ClientVehicleDb = _unitOfWork.VehiclePerClients.GetById(Request.ClientVehicle.Id??0);
                                if (ClientVehicleDb != null)
                                {
                                    // Update
                                    //var ClientVehicleUpdate = _Context.proc_VehiclePerClientUpdate(Request.ClientVehicle.Id,
                                    //                                                                    Request.ClientVehicle.ClientId,
                                    //                                                                    Request.ClientVehicle.PlateNumber,
                                    //                                                                    Request.ClientVehicle.BrandId,
                                    //                                                                    Request.ClientVehicle.ModelId,
                                    //                                                                    Request.ClientVehicle.BodyTypeId,
                                    //                                                                    Request.ClientVehicle.Year,
                                    //                                                                    Request.ClientVehicle.ColorId,
                                    //                                                                    Request.ClientVehicle.Doors,
                                    //                                                                    Request.ClientVehicle.TransmissionId,
                                    //                                                                    Request.ClientVehicle.Cylinders,
                                    //                                                                    Request.ClientVehicle.Power,
                                    //                                                                    Request.ClientVehicle.WheelsDriveId,
                                    //                                                                    Request.ClientVehicle.VIN,
                                    //                                                                    Request.ClientVehicle.LicenseNumber,
                                    //                                                                    Request.ClientVehicle.CountryId,
                                    //                                                                    Request.ClientVehicle.CityId,
                                    //                                                                    Request.ClientVehicle.IssuerId,
                                    //                                                                    Request.ClientVehicle.Odometer,
                                    //                                                                    null,
                                    //                                                                    null,
                                    //                                                                    null,
                                    //                                                                    Request.ClientVehicle.ChassisNumber,
                                    //                                                                    Request.ClientVehicle.MotorNumber,
                                    //                                                                    null,
                                    //                                                                    null,
                                    //                                                                    null,
                                    //                                                                    Request.ClientVehicle.PriceRate,
                                    //                                                                    true,
                                    //                                                                    ClientVehicleDb.CreatedBy,
                                    //                                                                    ClientVehicleDb.CreationDate,
                                    //                                                                    long.Parse(Encrypt_Decrypt.Decrypt(Request.ClientVehicle.ModifiedBy, key)),
                                    //                                                                    DateTime.Now
                                    //                                                                    );


                                    ClientVehicleDb.ClientId = Request.ClientVehicle.ClientId;
                                    ClientVehicleDb.PlatNumber = Request.ClientVehicle.PlateNumber;
                                    ClientVehicleDb.BrandId = Request.ClientVehicle.BrandId;
                                    ClientVehicleDb.ModelId = Request.ClientVehicle.ModelId;
                                    ClientVehicleDb.BodyTypeId = Request.ClientVehicle.BodyTypeId;
                                    ClientVehicleDb.Year = Request.ClientVehicle.Year;
                                    ClientVehicleDb.ColorId = Request.ClientVehicle.ColorId;
                                    ClientVehicleDb.Doors = Request.ClientVehicle.Doors;
                                    ClientVehicleDb.TransmissionId = Request.ClientVehicle.TransmissionId;
                                    ClientVehicleDb.Cylinders = Request.ClientVehicle.Cylinders;
                                    ClientVehicleDb.Power = Request.ClientVehicle.Power;
                                    ClientVehicleDb.WheelsDriveId = Request.ClientVehicle.WheelsDriveId;
                                    ClientVehicleDb.Vin = Request.ClientVehicle.VIN;
                                    ClientVehicleDb.LicenseNumber = Request.ClientVehicle.LicenseNumber;
                                    ClientVehicleDb.CountryId = Request.ClientVehicle.CountryId;
                                    ClientVehicleDb.CityId = Request.ClientVehicle.CityId;
                                    ClientVehicleDb.IssuerId = Request.ClientVehicle.IssuerId;
                                    ClientVehicleDb.Odometer = Request.ClientVehicle.Odometer;
                                    ClientVehicleDb.ChassisNumber = Request.ClientVehicle.ChassisNumber;
                                    ClientVehicleDb.MotorNumber = Request.ClientVehicle.MotorNumber;
                                    ClientVehicleDb.PriceRate = Request.ClientVehicle.PriceRate;
                                    ClientVehicleDb.ModifiedBy = UserID;
                                    ClientVehicleDb.ModifiedDate = DateTime.Now;

                                    _unitOfWork.Complete();

                                    if (ClientVehicleDb != null)
                                    {
                                        Response.Result = true;
                                        Response.ID = Request.ClientVehicle.Id ?? 0;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Update this Vehicle!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Land Vehicle Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                // Insert
                                //ObjectParameter ClientVehicleInsertedId = new ObjectParameter("ID", typeof(long));
                                //var ClientVehicleInsert = _Context.proc_VehiclePerClientInsert(ClientVehicleInsertedId,
                                //                                                                Request.ClientVehicle.ClientId,
                                //                                                                Request.ClientVehicle.PlateNumber,
                                //                                                                Request.ClientVehicle.BrandId,
                                //                                                                Request.ClientVehicle.ModelId,
                                //                                                                Request.ClientVehicle.BodyTypeId,
                                //                                                                Request.ClientVehicle.Year,
                                //                                                                Request.ClientVehicle.ColorId,
                                //                                                                Request.ClientVehicle.Doors,
                                //                                                                Request.ClientVehicle.TransmissionId,
                                //                                                                Request.ClientVehicle.Cylinders,
                                //                                                                Request.ClientVehicle.Power,
                                //                                                                Request.ClientVehicle.WheelsDriveId,
                                //                                                                Request.ClientVehicle.VIN,
                                //                                                                Request.ClientVehicle.LicenseNumber,
                                //                                                                Request.ClientVehicle.CountryId,
                                //                                                                Request.ClientVehicle.CityId,
                                //                                                                Request.ClientVehicle.IssuerId,
                                //                                                                Request.ClientVehicle.Odometer,
                                //                                                                null,
                                //                                                                null,
                                //                                                                null,
                                //                                                                Request.ClientVehicle.ChassisNumber,
                                //                                                                Request.ClientVehicle.MotorNumber,
                                //                                                                null,
                                //                                                                null,
                                //                                                                null,
                                //                                                                Request.ClientVehicle.PriceRate,
                                //                                                                true,
                                //                                                                long.Parse(Encrypt_Decrypt.Decrypt(Request.ClientVehicle.CreatedBy, key)),
                                //                                                                DateTime.Now,
                                //                                                                null,
                                //                                                                null
                                //);
                                var newClientVehicle = new VehiclePerClient();
                                newClientVehicle.ClientId = Request.ClientVehicle.ClientId;
                                newClientVehicle.PlatNumber = Request.ClientVehicle.PlateNumber;
                                newClientVehicle.BrandId = Request.ClientVehicle.BrandId;
                                newClientVehicle.ModelId = Request.ClientVehicle.ModelId;
                                newClientVehicle.BodyTypeId = Request.ClientVehicle.BodyTypeId;
                                newClientVehicle.Year = Request.ClientVehicle.Year;
                                newClientVehicle.ColorId = Request.ClientVehicle.ColorId;
                                newClientVehicle.Doors = Request.ClientVehicle.Doors;
                                newClientVehicle.TransmissionId = Request.ClientVehicle.TransmissionId;
                                newClientVehicle.Cylinders = Request.ClientVehicle.Cylinders;
                                newClientVehicle.Power = Request.ClientVehicle.Power;
                                newClientVehicle.WheelsDriveId = Request.ClientVehicle.WheelsDriveId;
                                newClientVehicle.Vin = Request.ClientVehicle.VIN;
                                newClientVehicle.LicenseNumber = Request.ClientVehicle.LicenseNumber;
                                newClientVehicle.CountryId = Request.ClientVehicle.CountryId;
                                newClientVehicle.CityId = Request.ClientVehicle.CityId;
                                newClientVehicle.IssuerId = Request.ClientVehicle.IssuerId;
                                newClientVehicle.Odometer = Request.ClientVehicle.Odometer;
                                newClientVehicle.ChassisNumber = Request.ClientVehicle.ChassisNumber;
                                newClientVehicle.MotorNumber = Request.ClientVehicle.MotorNumber;
                                newClientVehicle.PriceRate = Request.ClientVehicle.PriceRate;
                                newClientVehicle.CreatedBy = UserID;
                                newClientVehicle.CreationDate = DateTime.Now;

                                var ClientVehicleInsert =_unitOfWork.VehiclePerClients.Add(newClientVehicle);
                                _unitOfWork.Complete();

                                if (ClientVehicleInsert.Id > 0)
                                {
                                    ClientId = ClientVehicleInsert.Id;
                                    Response.Result = true;
                                    Response.ID = ClientId;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this Vehicle!!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }
                }

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

        public BaseResponseWithID AddNextVehicleMaintenanceInCurrentOpenJobOrder(AddNewVehicleMaitenanceJobOrder Request, long userID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (Request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    DateTime? NextVisitDate = null;
                    DateTime CreationDate = DateTime.Now;
                    long? ProjectId = null;

                    if (Request.VehicleMaintenanceJobOrder != null)
                    {
                        if (Request.VehicleMaintenanceJobOrder.CuurentOpenJobOrderId != 0)
                        {
                            var JobOrderSalesOfferDb = _unitOfWork.SalesOffers.GetById(Request.VehicleMaintenanceJobOrder.CuurentOpenJobOrderId);
                            if (JobOrderSalesOfferDb == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This JobOrder Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                var JobOrderProjectDb = _unitOfWork.Projects.Find(a => a.SalesOfferId == Request.VehicleMaintenanceJobOrder.CuurentOpenJobOrderId);
                                if (JobOrderProjectDb != null)
                                {
                                    ProjectId = JobOrderProjectDb.Id;
                                }
                                if (Request.VehicleMaintenanceJobOrder.CurrentMaintenanceTypeId != 0)
                                {
                                    var VehicleMaintenanceTypeDb = _unitOfWork.VehicleMaintenanceTypes.Find(a => a.Id == Request.VehicleMaintenanceJobOrder.CurrentMaintenanceTypeId);
                                    if (VehicleMaintenanceTypeDb == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "This MaintenanceType Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        if (Request.VehicleMaintenanceJobOrder.ClientVehicleId != 0)
                                        {
                                            var VehiclePerClientDb = _unitOfWork.VehiclePerClients.GetById(Request.VehicleMaintenanceJobOrder.ClientVehicleId);
                                            if (VehiclePerClientDb == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "This ClientVehicle Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                if (Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage == null)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "Milage Is Mandatory";
                                                    Response.Errors.Add(error);
                                                }
                                                else
                                                {
                                                    if (Request.VehicleMaintenanceJobOrder.NextVisitForId != null && Request.VehicleMaintenanceJobOrder.NextVisitForId != 0)
                                                    {
                                                        var VehicleMaintenanceTypeNextVisitDb = _unitOfWork.VehicleMaintenanceTypes.Find(a => a.Id == Request.VehicleMaintenanceJobOrder.NextVisitForId);
                                                        if (VehicleMaintenanceTypeNextVisitDb == null)
                                                        {
                                                            Response.Result = false;
                                                            Error error = new Error();
                                                            error.ErrorCode = "ErrCRM1";
                                                            error.ErrorMSG = "This Next Visit MaintenanceType Doesn't Exist!!";
                                                            Response.Errors.Add(error);
                                                        }
                                                        else
                                                        {
                                                            if (Request.VehicleMaintenanceJobOrder.NextVisitMilage != null)
                                                            {
                                                                var ClientApprovalDate = JobOrderSalesOfferDb.ClientApprovalDate;
                                                                var VehicleMaintenanceJobOrderDate = DateTime.Now;
                                                                if (ClientApprovalDate != null)
                                                                {
                                                                    VehicleMaintenanceJobOrderDate = (DateTime)ClientApprovalDate;
                                                                    CreationDate = (DateTime)ClientApprovalDate;
                                                                }
                                                                var SubstractedMonths = 0;
                                                                int SubstractedMilage = 0;
                                                                var LastClientVehicleJobOrder = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(a => a.VehiclePerClientId == Request.VehicleMaintenanceJobOrder.ClientVehicleId).ToList().OrderByDescending(a => a.CreationDate).FirstOrDefault();
                                                                //_Context.proc_VehicleMaintenanceJobOrderHistoryLoadAll().Where(a => a.VehiclePerClientId == Request.VehicleMaintenanceJobOrder.ClientVehicleId).ToList().OrderByDescending(a => a.CreationDate).FirstOrDefault();
                                                                if (LastClientVehicleJobOrder != null)
                                                                {
                                                                    var LastClientVehicleJobOrderDate = LastClientVehicleJobOrder.CreationDate;
                                                                    var LastClientVehicleJobOrderMilage = LastClientVehicleJobOrder.Milage ?? 0;

                                                                    SubstractedMonths = ((VehicleMaintenanceJobOrderDate.Year - LastClientVehicleJobOrderDate.Year) * 12) + VehicleMaintenanceJobOrderDate.Month - LastClientVehicleJobOrderDate.Month;
                                                                    SubstractedMilage = (Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage ?? 0) - LastClientVehicleJobOrderMilage;
                                                                }
                                                                else
                                                                {
                                                                    var ClientVehicleOdometer = VehiclePerClientDb.Odometer ?? 0;
                                                                    var ClientVehicleDate = DateTime.Now;
                                                                    if (ClientVehicleOdometer == 0)
                                                                    {
                                                                        var CarModelYear = VehiclePerClientDb.Year;
                                                                        if (CarModelYear != null)
                                                                        {
                                                                            ClientVehicleDate = new DateTime(int.Parse(CarModelYear), 6, 1);
                                                                        }
                                                                        else
                                                                        {
                                                                            ClientVehicleDate = VehiclePerClientDb.CreationDate;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        ClientVehicleDate = VehiclePerClientDb.CreationDate;
                                                                    }
                                                                    SubstractedMonths = ((VehicleMaintenanceJobOrderDate.Year - ClientVehicleDate.Year) * 12) + VehicleMaintenanceJobOrderDate.Month - ClientVehicleDate.Month;
                                                                    SubstractedMilage = (Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage ?? 0) - ClientVehicleOdometer;

                                                                }
                                                                if (SubstractedMonths > 0 && SubstractedMilage > 0)
                                                                {
                                                                    var VehicleKmPerMonthRate = SubstractedMilage / SubstractedMonths;

                                                                    if (VehicleKmPerMonthRate > 0)
                                                                    {
                                                                        var NextVisitNumberOfMonths = (Request.VehicleMaintenanceJobOrder.NextVisitMilage - Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage) / VehicleKmPerMonthRate;

                                                                        NextVisitDate = VehicleMaintenanceJobOrderDate.AddMonths((int)NextVisitNumberOfMonths);
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Response.Result = false;
                                                                Error error = new Error();
                                                                error.ErrorCode = "ErrCRM1";
                                                                error.ErrorMSG = "Next Visit Milage Is Mandatory";
                                                                Response.Errors.Add(error);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "ClientVehicleId Is Mandatory";
                                            Response.Errors.Add(error);
                                        }
                                    }

                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "MaintenanceTypeId Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "JobOrderSalesOfferId Is Mandatory";
                            Response.Errors.Add(error);
                        }

                        string CreatedByString = null;
                        string ModifiedByString = null;
                        long? CreatedBy = 0;
                        long? ModifiedBy = 0;
                        if (Request.VehicleMaintenanceJobOrder.Id != 0 && Request.VehicleMaintenanceJobOrder.Id != null)
                        {
                            
                            ModifiedBy = userID;
                            var user = _unitOfWork.Users.GetById(ModifiedBy??0);
                            if (user == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Modifier User Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            
                            CreatedBy = userID;
                            var user = _unitOfWork.Users.GetById(CreatedBy??0);
                            if (user == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "ErrCRM1";
                                error.ErrorMSG = "This Creator User Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }

                        if (Response.Result)
                        {
                            if (Request.VehicleMaintenanceJobOrder.Id != null && Request.VehicleMaintenanceJobOrder.Id != 0)
                            {
                                //var VehicleMaintenanceJobOrderDb = _Context.proc_VehicleMaintenanceJobOrderHistoryLoadByPrimaryKey(Request.VehicleMaintenanceJobOrder.Id).FirstOrDefault();
                                var VehicleMaintenanceJobOrderDb = _unitOfWork.VehicleMaintenanceJobOrderHistories.Find(x => x.Id == Request.VehicleMaintenanceJobOrder.Id);
                                if (VehicleMaintenanceJobOrderDb != null)
                                {
                                    // Update
                                    //Not Applied
                                    VehicleMaintenanceJobOrderDb.VehicleMaintenanceTypeId = Request.VehicleMaintenanceJobOrder.CurrentMaintenanceTypeId;
                                    VehicleMaintenanceJobOrderDb.JobOrderProjectId = ProjectId;
                                    VehicleMaintenanceJobOrderDb.VehiclePerClientId = Request.VehicleMaintenanceJobOrder.ClientVehicleId;
                                    VehicleMaintenanceJobOrderDb.Milage = Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage;
                                    VehicleMaintenanceJobOrderDb.NextVisitForId = Request.VehicleMaintenanceJobOrder.NextVisitForId;
                                    VehicleMaintenanceJobOrderDb.NextVisitDate = NextVisitDate;
                                    VehicleMaintenanceJobOrderDb.NextVisitMilage = Request.VehicleMaintenanceJobOrder.NextVisitMilage;
                                    VehicleMaintenanceJobOrderDb.NextVisitComment = Request.VehicleMaintenanceJobOrder.NextVisitComment;
                                    VehicleMaintenanceJobOrderDb.Active = true;
                                    VehicleMaintenanceJobOrderDb.ModifiedDate = DateTime.Now;
                                    VehicleMaintenanceJobOrderDb.ModifiedBy = ModifiedBy;
                                    VehicleMaintenanceJobOrderDb.SalesOfferId = Request.VehicleMaintenanceJobOrder.CuurentOpenJobOrderId;


                                    var VehicleMaintenanceJobOrderUpdate = _unitOfWork.Complete();
                                    //var VehicleMaintenanceJobOrderUpdate = _Context.proc_VehicleMaintenanceJobOrderHistoryUpdate(Request.VehicleMaintenanceJobOrder.Id,
                                    //                                                                   Request.VehicleMaintenanceJobOrder.CurrentMaintenanceTypeId,
                                    //                                                                   ProjectId,
                                    //                                                                   Request.VehicleMaintenanceJobOrder.ClientVehicleId,
                                    //                                                                   Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage,
                                    //                                                                   Request.VehicleMaintenanceJobOrder.NextVisitForId,
                                    //                                                                   NextVisitDate,
                                    //                                                                   Request.VehicleMaintenanceJobOrder.NextVisitMilage,
                                    //                                                                   Request.VehicleMaintenanceJobOrder.NextVisitComment,
                                    //                                                                   true,
                                    //                                                                   VehicleMaintenanceJobOrderDb.CreatedBy,
                                    //                                                                   VehicleMaintenanceJobOrderDb.CreationDate,
                                    //                                                                   ModifiedBy,
                                    //                                                                   DateTime.Now
                                    //                                                                    );
                                    if (VehicleMaintenanceJobOrderUpdate > 0)
                                    {
                                        Response.Result = true;
                                        Response.ID = Request.VehicleMaintenanceJobOrder.Id ?? 0;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Update this VehicleMaintenance!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Vehicle Maintenance Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                // Insert
                                var Obj = new VehicleMaintenanceJobOrderHistory();
                                Obj.VehicleMaintenanceTypeId = Request.VehicleMaintenanceJobOrder.CurrentMaintenanceTypeId;
                                Obj.JobOrderProjectId = ProjectId;
                                Obj.VehiclePerClientId = Request.VehicleMaintenanceJobOrder.ClientVehicleId;
                                Obj.Milage = Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage;
                                Obj.NextVisitForId = Request.VehicleMaintenanceJobOrder.NextVisitForId;
                                Obj.NextVisitDate = NextVisitDate;
                                Obj.NextVisitMilage = Request.VehicleMaintenanceJobOrder.NextVisitMilage;
                                Obj.NextVisitComment = Request.VehicleMaintenanceJobOrder.NextVisitComment;
                                Obj.Active = true;
                                Obj.CreationDate = DateTime.Now;
                                Obj.CreatedBy = userID;
                                Obj.ModifiedDate = DateTime.Now;
                                Obj.ModifiedBy = userID;
                                Obj.SalesOfferId = Request.VehicleMaintenanceJobOrder.CuurentOpenJobOrderId;

                                _unitOfWork.VehicleMaintenanceJobOrderHistories.Add(Obj);
                                var VehicleMaintenanceJobOrderInsert = _unitOfWork.Complete();

                                //ObjectParameter VehicleMaintenanceJobOrderInsertedId = new ObjectParameter("ID", typeof(long));
                                //var VehicleMaintenanceJobOrderInsert = _Context.proc_VehicleMaintenanceJobOrderHistoryInsert(VehicleMaintenanceJobOrderInsertedId,
                                //                                                                           Request.VehicleMaintenanceJobOrder.CurrentMaintenanceTypeId,
                                //                                                                           ProjectId,
                                //                                                                           Request.VehicleMaintenanceJobOrder.ClientVehicleId,
                                //                                                                           Request.VehicleMaintenanceJobOrder.CurentOpenJobOrderMilage,
                                //                                                                           Request.VehicleMaintenanceJobOrder.NextVisitForId,
                                //                                                                           NextVisitDate,
                                //                                                                           Request.VehicleMaintenanceJobOrder.NextVisitMilage,
                                //                                                                           Request.VehicleMaintenanceJobOrder.NextVisitComment,
                                //                                                                           true,
                                //                                                                           CreatedBy,
                                //                                                                           CreationDate,
                                //                                                                           null,
                                //                                                                           null
                                //                                                                );

                                if (VehicleMaintenanceJobOrderInsert > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = Obj.Id; //(long)VehicleMaintenanceJobOrderInsertedId.Value;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this Vehicle!!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }
                }

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

        public GetClinetVehiclesDataForGet GetClientVehiclesDataResponse(GetClientVehiclesDataResponseFilters filters, string CompName)
        {
            GetClinetVehiclesDataForGet Response = new GetClinetVehiclesDataForGet();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //long ClientId = 0;
                    if (filters.ClientId != null)
                    {
                        
                        var client = _unitOfWork.Clients.GetById(filters.ClientId??0);
                        if (client == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }
                    

                    //long VehicleId = 0;
                    if (filters.VehicleId != null)
                    {
                        
                        var vehicle = _unitOfWork.VehiclePerClients.GetById(filters.VehicleId??0);
                        if (vehicle == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Vehicle Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    string SearchedKey = "";
                    if (!string.IsNullOrEmpty(filters.SearchedKey))
                    {
                        SearchedKey = HttpUtility.UrlDecode(filters.SearchedKey).ToLower();
                    }

                    //int Month = 0;
                    //if (!string.IsNullOrEmpty(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                    //{
                    //    int.TryParse(headers["Month"], out Month);
                    //}

                    //int Year = 0;
                    //if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                    //{
                    //    int.TryParse(headers["Year"], out Year);
                    //}

                    //int CurrentPage = 1;
                    //if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    //{
                    //    int.TryParse(headers["CurrentPage"], out CurrentPage);
                    //}

                    //int NumberOfItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    //{
                    //    int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    //}

                    var VehiclesDBQuery = _unitOfWork.VVehicles.FindAllQueryable(a => a.Active == true).AsQueryable();

                    if (filters.ClientId != null)
                    {
                        VehiclesDBQuery = VehiclesDBQuery.Where(a => a.ClientId == filters.ClientId);
                    }

                    if (filters.VehicleId != null)
                    {
                        VehiclesDBQuery = VehiclesDBQuery.Where(a => a.Id == filters.VehicleId);
                    }

                    if (!string.IsNullOrEmpty(SearchedKey))
                    {
                        VehiclesDBQuery = VehiclesDBQuery.Where(a => (a.Vin ?? "").Contains(SearchedKey) || (a.PlatNumber ?? "").Contains(SearchedKey) || (a.ClientName ?? "").Contains(SearchedKey));
                    }
                    var FilterWithStartDate = false;
                    var FilterWithEndDate = false;
                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    if (filters.Year > 0 && filters.Year != null)
                    {
                        FilterWithStartDate = true;
                        FilterWithEndDate = true;
                        if (filters.Month > 0 && filters.Month != null)
                        {
                            StartDate = new DateTime(filters.Year??0, filters.Month??0, 1);

                            if (filters.Month != 12)
                            {
                                EndDate = new DateTime(filters.Year??0, filters.Month??0 + 1, 1);
                            }
                            else
                            {
                                EndDate = new DateTime(filters.Year??0 + 1, 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(filters.Year??0 , 1, 1);
                            EndDate = new DateTime(filters.Year??0 + 1, 1, 1);
                        }

                    }
                    else
                    {
                        if (filters.Month > 0 && filters.Month != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    if (!string.IsNullOrEmpty(filters.VehicleCreationFrom))
                    {
                        DateTime VehicleCreationFrom = DateTime.Now;
                        if (!DateTime.TryParse(filters.VehicleCreationFrom, out VehicleCreationFrom))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Vehicle Creation From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        StartDate = VehicleCreationFrom;
                        FilterWithStartDate = true;
                    }

                    if (!string.IsNullOrEmpty(filters.VehicleCreationTo))
                    {
                        DateTime VehicleCreationTo = DateTime.Now;
                        if (!DateTime.TryParse(filters.VehicleCreationTo, out VehicleCreationTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Invalid Vehicle Creation To";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        EndDate = VehicleCreationTo;
                        FilterWithEndDate = true;
                    }
                    if (FilterWithStartDate)
                    {
                        VehiclesDBQuery = VehiclesDBQuery.Where(a => a.CreationDate >= StartDate);
                    }
                    if (FilterWithEndDate)
                    {
                        VehiclesDBQuery = VehiclesDBQuery.Where(a => a.CreationDate < EndDate);
                    }

                    VehiclesDBQuery = VehiclesDBQuery.OrderByDescending(a => a.CreationDate);
                    var VehiclesListDB = PagedList<VVehicle>.Create(VehiclesDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = VehiclesListDB.TotalPages,
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = VehiclesListDB.TotalCount
                    };
                    var VehicleResponseList = new List<ClientVehicleForGet>();
                    var IDSClients = VehiclesListDB.Select(x => x.ClientId).Distinct().ToList();
                    var ClientListDB = _unitOfWork.Clients.FindAll(x => IDSClients.Contains(x.Id)).ToList();
                    foreach (var vehicle in VehiclesListDB)
                    {
                        var ClientLogo = ClientListDB.Where(x => x.Id == vehicle.ClientId).Select(x => x.Logo).FirstOrDefault();
                        var ClientVehicle = new ClientVehicleForGet()
                        {
                            Id = vehicle.Id,
                            ClientId = vehicle.ClientId,
                            ClientName = vehicle.ClientName,
                            BrandId = vehicle.BrandId,
                            BrandName = vehicle.VehicleBrandName,
                            ModelId = vehicle.ModelId,
                            ModelName = vehicle.VehicleModelName,
                            BodyTypeId = vehicle.BodyTypeId,
                            BodyTypeName = vehicle.VehicleBodyTypeName,
                            ChassisNumber = vehicle.ChassisNumber,
                            CountryId = vehicle.CountryId,
                            CountryName = vehicle.CountryName,
                            CityId = vehicle.CityId,
                            CityName = vehicle.GovernorateName,
                            ColorId = vehicle.ColorId,
                            ColorName = vehicle.VehicleColorName,
                            IssuerId = vehicle.IssuerId,
                            IssuerName = vehicle.VehicleIssuerName,
                            Doors = vehicle.Doors,
                            LicenseNumber = vehicle.LicenseNumber,
                            Cylinders = vehicle.Cylinders,
                            MotorNumber = vehicle.MotorNumber,
                            Odometer = vehicle.Odometer,
                            PlateNumber = vehicle.PlatNumber,
                            Power = vehicle.Power,
                            TransmissionId = vehicle.TransmissionId,
                            TransmissionName = vehicle.VehicleTransmissionName,
                            VIN = vehicle.Vin,
                            WheelsDriveId = vehicle.WheelsDriveId,
                            WheelsDriveName = vehicle.VehicleWheelsDriveName,
                            PriceRate = vehicle.PriceRate,
                            Year = vehicle.Year,
                            CreatedBy = vehicle.CreatedBy.ToString(),
                            CreationDate = vehicle.CreationDate.ToShortDateString(),
                            ModifiedBy = vehicle.ModifiedBy.ToString(),
                            ClientLogo = ClientLogo != null ? Globals.baseURL + "/ShowImage.ashx?ImageID=" + HttpUtility.UrlEncode(Encrypt_Decrypt.Encrypt(vehicle.ClientId.ToString(), key)) + "&type=client&CompName=" + CompName.ToString().ToLower() : null,
                        };

                        var VehicleMaintenanceTypeItemsDB = _unitOfWork.VehicleMaintenanceTypes.FindAll(a => a.Active == true && a.VehicleMaintenanceTypeForModels.Where(b => b.ModelId == ClientVehicle.ModelId || b.BrandId == ClientVehicle.BrandId || b.ForAllModles == true).Select(b => b.VehicleMaintenanceTypeId).ToList().Contains(a.Id), new[] { "VehiclePriorityLevel", "VehicleRate", "VehicleMaintenanceJobOrderHistoryVehicleMaintenanceTypes" }).ToList();

                        if (VehicleMaintenanceTypeItemsDB != null)
                        {
                            if (VehicleMaintenanceTypeItemsDB.Count > 0)
                            {
                                List<VehicleMaintenanceTypeItem> VehicleMaintenanceTypeItemsList = new List<VehicleMaintenanceTypeItem>();
                                var VehicleMaintenanceJobOrderList = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(a => a.VehiclePerClientId == vehicle.Id ).ToList();
                                foreach (var item in VehicleMaintenanceTypeItemsDB)
                                {
                                    VehicleMaintenanceTypeItem vehicleMaintenanceTypeItem = new VehicleMaintenanceTypeItem();

                                    vehicleMaintenanceTypeItem.ID = item.Id;
                                    vehicleMaintenanceTypeItem.Name = item.Name != null ? item.Name.ToString() : "";
                                    vehicleMaintenanceTypeItem.Description = item.Description != null ? item.Description.ToString() : "";
                                    vehicleMaintenanceTypeItem.VehiclePriorityLevelId = item.VehiclePriorityLevelId;
                                    vehicleMaintenanceTypeItem.VehiclePriorityLevelName = item.VehiclePriorityLevel.PriorityLevelName;
                                    vehicleMaintenanceTypeItem.VehicleRateId = item.VehicleRateId;
                                    vehicleMaintenanceTypeItem.Comment = item.Comment;


                                    var VehicleMaintenanceJobOrder = VehicleMaintenanceJobOrderList.Where(a => a.VehicleMaintenanceTypeId == vehicleMaintenanceTypeItem.ID).FirstOrDefault();
                                    if (VehicleMaintenanceJobOrder != null)
                                    {
                                        vehicleMaintenanceTypeItem.IsUsed = true;
                                    }
                                    vehicleMaintenanceTypeItem.VehicleRateName = item.VehicleRate.RateName;
                                    vehicleMaintenanceTypeItem.NumberOfUses = item.VehicleMaintenanceJobOrderHistoryVehicleMaintenanceTypes.Count();
                                    vehicleMaintenanceTypeItem.VehicleMaintenanceTypeBOM = new VehicleMaintenanceTypeBOM();
                                    if (item.Bomid != null)
                                    {
                                        if (item.Bomid > 0)
                                        {
                                            vehicleMaintenanceTypeItem.VehicleMaintenanceTypeBOM = GetVehicleMaintenanceTypeBOM((long)item.Bomid);
                                        }
                                    }

                                    VehicleMaintenanceTypeItemsList.Add(vehicleMaintenanceTypeItem);
                                }
                                ClientVehicle.VehicleMaintenanceTypeItems = VehicleMaintenanceTypeItemsList;
                            }
                        }

                        VehicleResponseList.Add(ClientVehicle);
                    }

                    Response.ClientVehicles = VehicleResponseList;

                    //var context = WebOperationContext.Current;
                    //context.AddPaginationHeader(CurrentPage, VehiclesListDB.TotalPages, NumberOfItemsPerPage, VehiclesListDB.TotalCount);
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        public  VehicleMaintenanceTypeBOM GetVehicleMaintenanceTypeBOM(long BOMId)
        {
            VehicleMaintenanceTypeBOM BOM = new VehicleMaintenanceTypeBOM();
            var BOMObj = _unitOfWork.Boms.Find(a => a.Id == BOMId);
            if (BOMObj != null)
            {
                BOM.BOMID = BOMObj.Id;
                BOM.BOMName = BOMObj.Name;

                var BOMPartitionItems = _unitOfWork.VBompartitionItemsInventoryItems.FindAll(a => a.Bomid == BOMObj.Id).ToList();
                if (BOMPartitionItems != null)
                {
                    if (BOMPartitionItems.Count > 0)
                    {
                        BOM.BOMPartitionItemsNames = BOMPartitionItems.Select(a => a.InventoryItemName).ToList();
                        decimal TotalCostType1, TotalCostType2, TotalCostType3;
                        TotalCostType1 = TotalCostType2 = TotalCostType3 = 0;

                        foreach (var partitionItem in BOMPartitionItems)
                        {
                            var totalItemPriceCostType1 = partitionItem.CostAmount1 * partitionItem.RequiredQty;
                            TotalCostType1 += (totalItemPriceCostType1 ?? 0);
                            var totalItemPriceCostType2 = partitionItem.CostAmount2 * partitionItem.RequiredQty;
                            TotalCostType2 += (totalItemPriceCostType2 ?? 0);
                            var totalItemPriceCostType3 = partitionItem.CostAmount3 * partitionItem.RequiredQty;
                            TotalCostType3 += (totalItemPriceCostType3 ?? 0);
                        }

                        BOM.TotalCostType1 = TotalCostType1;
                        BOM.TotalCostType2 = TotalCostType2;
                        BOM.TotalCostType3 = TotalCostType3;
                    }
                }
            }

            return BOM;
        }

        public async Task<GetVehicleMaintenanceJobOrderHistoryResponse> GetVehicleMaintenanceJobOrderHistory(GetVehicleMaintenanceJobOrderHistoryFilters filters)
        {
            GetVehicleMaintenanceJobOrderHistoryResponse Response = new GetVehicleMaintenanceJobOrderHistoryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    //long ClientId = 0;
                    if (filters.ClientId != null )
                    {
                        //long.TryParse(headers["ClientId"], out ClientId);
                        var client = await _unitOfWork.Clients.FindAsync(a => a.Id == filters.ClientId);
                        if (client == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    //long ClientVehicleId = 0;
                    if (filters.ClientVehicleId != null)
                    {
                        //long.TryParse(headers["ClientVehicleId"], out ClientVehicleId);
                        var vehicle = _unitOfWork.VehiclePerClients.FindAsync(a => a.Id == filters.ClientVehicleId);
                        if (vehicle == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Vehicle Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    //long SalesOfferId = 0;
                    if (filters.SalesOfferId != null)
                    {
                        //long.TryParse(headers["SalesOfferId"], out SalesOfferId);
                        var SalesOfferDB = await _unitOfWork.SalesOffers.FindAsync(a => a.Id == filters.SalesOfferId);
                        if (SalesOfferDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Offer Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    //int Month = 0;
                    //if (!string.IsNullOrEmpty(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                    //{
                    //    int.TryParse(headers["Month"], out Month);
                    //}

                    //int Year = 0;
                    //if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                    //{
                    //    int.TryParse(headers["Year"], out Year);
                    //}

                    //int CurrentPage = 1;
                    //if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    //{
                    //    int.TryParse(headers["CurrentPage"], out CurrentPage);
                    //}

                    //int NumberOfItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    //{
                    //    int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    //}

                    var VehicleMaintenanceJobOrderDBQuery = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAllQueryable(a => a.Active == true, new[] { "JobOrderProject", "JobOrderProject", "JobOrderProject.SalesOffer", "VehiclePerClient.Client", "VehicleMaintenanceType" }).AsQueryable();

                    if (filters.ClientId != 0 && filters.ClientId != null)
                    {
                        VehicleMaintenanceJobOrderDBQuery = VehicleMaintenanceJobOrderDBQuery.Where(a => a.VehiclePerClient.ClientId == filters.ClientId);
                    }

                    if (filters.ClientVehicleId != 0 && filters.ClientVehicleId != null)
                    {
                        VehicleMaintenanceJobOrderDBQuery = VehicleMaintenanceJobOrderDBQuery.Where(a => a.VehiclePerClientId == filters.ClientVehicleId);
                    }

                    if (filters.SalesOfferId != 0 && filters.SalesOfferId != null)
                    {
                        VehicleMaintenanceJobOrderDBQuery = VehicleMaintenanceJobOrderDBQuery.Where(a => a.SalesOfferId == filters.SalesOfferId);
                    }

                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    bool StartDateFiltered = false;
                    var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);
                    bool EndDateFiltered = false;
                    if (filters.Year > 0)
                    {
                        StartDateFiltered = true;
                        EndDateFiltered = true;
                        if (filters.Month > 0)
                        {
                            StartDate = new DateTime(filters.Year??0, filters.Month??0, 1);

                            if (filters.Month != 12)
                            {
                                EndDate = new DateTime(filters.Year??0, filters.Month??0 + 1, 1);
                            }
                            else
                            {
                                EndDate = new DateTime(filters.Year??0 + 1, 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(filters.Year??0 , 1, 1);
                            EndDate = new DateTime(filters.Year??0 + 1, 1, 1);
                        }
                    }
                    else
                    {
                        if (filters.Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }

                    if (!string.IsNullOrEmpty(filters.MaintenanceCreationFrom))
                    {
                        StartDateFiltered = true;
                        DateTime MaintenanceCreationFrom = DateTime.Now;
                        if (!DateTime.TryParse(filters.MaintenanceCreationFrom, out MaintenanceCreationFrom))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Maintenance Creation From";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        StartDate = MaintenanceCreationFrom;
                    }

                    if (!string.IsNullOrEmpty(filters.MaintenanceCreationTo))
                    {
                        EndDateFiltered = true;
                        DateTime MaintenanceCreationTo = DateTime.Now;
                        if (!DateTime.TryParse(filters.MaintenanceCreationTo, out MaintenanceCreationTo))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "Invalid Maintenance Creation To";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        EndDate = MaintenanceCreationTo;
                    }

                    if (StartDateFiltered)
                    {
                        VehicleMaintenanceJobOrderDBQuery = VehicleMaintenanceJobOrderDBQuery.Where(a => a.CreationDate >= StartDate);
                    }
                    if (EndDateFiltered)
                    {
                        VehicleMaintenanceJobOrderDBQuery = VehicleMaintenanceJobOrderDBQuery.Where(a => a.CreationDate < EndDate);
                    }

                    VehicleMaintenanceJobOrderDBQuery = VehicleMaintenanceJobOrderDBQuery.OrderByDescending(a => a.CreationDate);

                    var VehiclesMaintenanceJobOrderListDB = await PagedList<VehicleMaintenanceJobOrderHistory>.CreateAsync(VehicleMaintenanceJobOrderDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);

                    if (VehiclesMaintenanceJobOrderListDB != null && VehiclesMaintenanceJobOrderListDB.Count() > 0)
                    {
                        List<VehicleMaintenanceJobOrderHistoryModel> VehiclesMaintenanceJobOrderListResponse = VehiclesMaintenanceJobOrderListDB.Select(vmtjb => new VehicleMaintenanceJobOrderHistoryModel
                        {
                            Id = vmtjb.Id,
                            ClientVehicleId = vmtjb.VehiclePerClientId,
                            CurentOpenJobOrderMilage = vmtjb.Milage,
                            JobOrderProjectId = vmtjb.JobOrderProjectId ?? 0,
                            ClientId = vmtjb.VehiclePerClient?.ClientId ?? 0,
                            ClientName = vmtjb.VehiclePerClient?.Client?.Name,
                            NextVisitComment = vmtjb.NextVisitComment,
                            NextVisitMilage = vmtjb.NextVisitMilage,
                            NextVisitDate = vmtjb.NextVisitDate != null ? vmtjb.NextVisitDate.ToString().Split(' ')[0] : null,
                            NextVisitForId = vmtjb.NextVisitForId,
                            NextVisitForName = vmtjb.VehicleMaintenanceType?.Name,
                            CurrentMaintenanceTypeId = vmtjb.VehicleMaintenanceTypeId,
                            CuurentOpenJobOrderId = vmtjb.JobOrderProjectId ?? 0,
                            JobOrderProjectName = vmtjb.JobOrderProject?.SalesOffer?.ProjectName,
                            VehicleMaintenanceTypeName = vmtjb.VehicleMaintenanceType?.Name,
                            CreationDate = vmtjb.CreationDate.ToShortDateString()
                        }).ToList();

                        Response.VehicleMaintenanceJobOrderHistories = VehiclesMaintenanceJobOrderListResponse;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = VehiclesMaintenanceJobOrderListDB.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = VehiclesMaintenanceJobOrderListDB.TotalCount
                        };
                    }
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        //public BaseResponse DeleteVehicleMaintenanceType(DeleteMaintenanceType Request)
        //{
        //    BaseResponse Response = new BaseResponse();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {
        //        if (Response.Result)
        //        {
        //            long maintenanceId = 0;
        //            if (!string.IsNullOrEmpty(headers["MaintenanceId"]) && long.TryParse(headers["MaintenanceId"], out maintenanceId))
        //            {
        //                long.TryParse(headers["MaintenanceId"], out maintenanceId);
        //                var maintenanceType = _unitOfWork.VehicleMaintenanceTypes.GetById(maintenanceId);
        //                if (maintenanceType == null)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "ErrCRM1";
        //                    error.ErrorMSG = "This Maintenance Type Doesn't Exist!!";
        //                    Response.Errors.Add(error);
        //                }
        //                var VehicleMaintenanceTypeCategories = _unitOfWork.VehicleMaintenanceTypeServiceSheduleCategories.FindAll(a => a.VehicleMaintenanceTypeId == maintenanceType.Id).ToList();
        //                foreach (var x in VehicleMaintenanceTypeCategories)
        //                {
        //                    x.Active = false;
        //                }
        //                maintenanceType.Active = false;
        //                _unitOfWork.Complete();

        //            }
        //        }
        //        return Response;
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.Message;
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //}



        //public BaseResponseWithID TransferVehicle(transferRequest Request)
        //{
        //    BaseResponseWithID Response = new BaseResponseWithID();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {

        //        if (Response.Result)
        //        {
        //            long ClientVehicleId = 0;
        //            if (!string.IsNullOrEmpty(headers["ClientVehicleId"]) && long.TryParse(headers["ClientVehicleId"], out ClientVehicleId))
        //            {
        //                long.TryParse(headers["ClientVehicleId"], out ClientVehicleId);
        //                var vehicle = _unitOfWork.VehiclePerClients.Find(a => a.Id == ClientVehicleId);
        //                if (vehicle == null)
        //                {
        //                    Response.Result = false;
        //                    Error error = new Error();
        //                    error.ErrorCode = "ErrCRM1";
        //                    error.ErrorMSG = "This Client Vehicle Doesn't Exist!!";
        //                    Response.Errors.Add(error);
        //                }
        //                long clientId = 0;
        //                if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out clientId))
        //                {
        //                    long.TryParse(headers["ClientId"], out clientId);
        //                    var client = _unitOfWork.Clients.GetById(clientId);
        //                    if (client == null)
        //                    {
        //                        Response.Result = false;
        //                        Error error = new Error();
        //                        error.ErrorCode = "ErrCRM1";
        //                        error.ErrorMSG = "This Client Doesn't Exist!!";
        //                        Response.Errors.Add(error);
        //                    }
        //                    vehicle.ClientId = client.Id;
        //                    _unitOfWork.Complete();
        //                    Response.ID = ClientVehicleId;
        //                }

        //            }
        //        }
        //        return Response;
        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.Message;
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //}

        public ViewVehicleMaintenanceTypesResponse ViewVehicleMaintenanceTypes(long? ClientVehicleId)
        {
            ViewVehicleMaintenanceTypesResponse Response = new ViewVehicleMaintenanceTypesResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    //long ClientVehicleId = 0;
                    if (ClientVehicleId != null )
                    {
                        //long.TryParse(headers["ClientVehicleId"], out ClientVehicleId);
                        var vehicle = _unitOfWork.VehiclePerClients.Find(a => a.Id == ClientVehicleId);
                        if (vehicle == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "This Client Vehicle Doesn't Exist!!";
                            Response.Errors.Add(error);
                        }
                    }

                    VehicleMaintenanceTypes VehicleMaintenanceTypeItems = new VehicleMaintenanceTypes();
                    var ClientVehicle = _unitOfWork.VVehicles.Find(a => a.Id == ClientVehicleId);
                    if (ClientVehicle != null)
                    {
                        VehicleMaintenanceTypeItems.VehicleId = ClientVehicle.Id;
                        var VehicleMaintenanceTypeItemsDB = _unitOfWork.VehicleMaintenanceTypes.FindAll(a => a.Active == true && a.VehicleMaintenanceTypeForModels.Where(b => b.ModelId == ClientVehicle.ModelId || b.BrandId == ClientVehicle.BrandId || b.ForAllModles == true).Select(b => b.VehicleMaintenanceTypeId).ToList().Contains(a.Id), new[] { "VehicleRate", "VehiclePriorityLevel", "VehicleMaintenanceTypeServiceSheduleCategories", "VehicleMaintenanceTypeServiceSheduleCategories.VehicleServiceScheduleCategory", "VehicleMaintenanceTypeForModels.Brand" }).OrderBy(a => a.Milage).ToList();

                        if (VehicleMaintenanceTypeItemsDB != null)
                        {
                            if (VehicleMaintenanceTypeItemsDB.Count() > 0)
                            {
                                List<VehicleMaintenanceTypeItem> VehicleMaintenanceTypeItemsList = new List<VehicleMaintenanceTypeItem>();

                                for (int item = 0; item < VehicleMaintenanceTypeItemsDB.Count(); item++)
                                {
                                    VehicleMaintenanceTypeItem vehicleMaintenanceTypeItem = new VehicleMaintenanceTypeItem();

                                    vehicleMaintenanceTypeItem.ID = VehicleMaintenanceTypeItemsDB[item].Id;
                                    vehicleMaintenanceTypeItem.Name = VehicleMaintenanceTypeItemsDB[item].Name != null ? VehicleMaintenanceTypeItemsDB[item].Name.ToString() : "";
                                    vehicleMaintenanceTypeItem.Description = VehicleMaintenanceTypeItemsDB[item].Description != null ? VehicleMaintenanceTypeItemsDB[item].Description.ToString() : "";
                                    vehicleMaintenanceTypeItem.VehicleMaintenanceTypeServiceSheduleCategories = VehicleMaintenanceTypeItemsDB[item].VehicleMaintenanceTypeServiceSheduleCategories.Select(a => a.VehicleServiceScheduleCategory.ItemName).ToList();
                                    //vehicleMaintenanceTypeItem.VehicleMaintenanceTypeServiceSheduleCategoriesIds = VehicleMaintenanceTypeItemsDB[item].VehicleMaintenanceTypeServiceSheduleCategories.Select(a => a.VehicleServiceScheduleCategoryId.ToString()).ToList();
                                    //vehicleMaintenanceTypeItem.VehiclePriorityLevelId = VehicleMaintenanceTypeItemsDB[item].VehiclePriorityLevelId;
                                    vehicleMaintenanceTypeItem.VehiclePriorityLevelName = VehicleMaintenanceTypeItemsDB[item].VehiclePriorityLevel.PriorityLevelName;
                                    //vehicleMaintenanceTypeItem.VehicleRateId = VehicleMaintenanceTypeItemsDB[item].VehicleRateId;
                                    vehicleMaintenanceTypeItem.Comment = VehicleMaintenanceTypeItemsDB[item].Comment;
                                    vehicleMaintenanceTypeItem.VehicleRateName = VehicleMaintenanceTypeItemsDB[item].VehicleRate.RateName;
                                    vehicleMaintenanceTypeItem.Milage = VehicleMaintenanceTypeItemsDB[item].Milage;

                                    vehicleMaintenanceTypeItem.VehicleMaintenanceTypeBOM = new VehicleMaintenanceTypeBOM();
                                    if (VehicleMaintenanceTypeItemsDB[item].Bomid != null)
                                    {
                                        if (VehicleMaintenanceTypeItemsDB[item].Bomid > 0)
                                        {
                                            vehicleMaintenanceTypeItem.VehicleMaintenanceTypeBOM = GetVehicleMaintenanceTypeBOM((long)VehicleMaintenanceTypeItemsDB[item].Bomid);
                                        }
                                    }

                                    if (VehicleMaintenanceTypeItemsDB[item].VehicleMaintenanceTypeForModels.Count > 0)
                                    {
                                        List<string> VehicleMaintenanceTypeForModelsList = new List<string>();
                                        var isForAllModels = VehicleMaintenanceTypeItemsDB[item].VehicleMaintenanceTypeForModels.Where(a => a.ForAllModles == true).FirstOrDefault();
                                        if (isForAllModels != null)
                                        {
                                            vehicleMaintenanceTypeItem.isForAllModels = true;
                                            VehicleMaintenanceTypeForModelsList.Add("All");
                                        }
                                        else
                                        {
                                            vehicleMaintenanceTypeItem.isForAllModels = false;

                                            var ForBrands = VehicleMaintenanceTypeItemsDB[item].VehicleMaintenanceTypeForModels.Where(a => a.BrandId != null && a.Active == true).ToList();

                                            if (ForBrands.Count() > 0)
                                            {
                                                var BrandsIds = ForBrands.Select(a => "Brand-" + a.BrandId.ToString()).ToList();
                                                VehicleMaintenanceTypeForModelsList.AddRange(BrandsIds);
                                            }

                                            var ForModels = VehicleMaintenanceTypeItemsDB[item].VehicleMaintenanceTypeForModels.Where(a => a.ModelId != null && a.Active == true).ToList();
                                            if (ForModels.Count() > 0)
                                            {
                                                var ModelsIds = ForModels.Select(a => "Model-" + a.ModelId.ToString()).ToList();
                                                VehicleMaintenanceTypeForModelsList.AddRange(ModelsIds);
                                            }
                                        }
                                        vehicleMaintenanceTypeItem.VehicleMaintenanceTypeForModelsIds = VehicleMaintenanceTypeForModelsList;
                                    }

                                    var VehicleMaintenanceJobOrder = _Context.VehicleMaintenanceJobOrderHistories.Where(a => a.VehiclePerClientId == ClientVehicleId && a.VehicleMaintenanceTypeId == vehicleMaintenanceTypeItem.ID).OrderByDescending(a => a.CreationDate).FirstOrDefault();
                                    if (VehicleMaintenanceJobOrder != null)
                                    {
                                        vehicleMaintenanceTypeItem.LastJobOrderDate = VehicleMaintenanceJobOrder.CreationDate.ToShortDateString();
                                        vehicleMaintenanceTypeItem.IsUsed = true;
                                    }
                                    else
                                    {
                                        var MaintenanceTypeMilage = VehicleMaintenanceTypeItemsDB[item].Milage;
                                        var PreviousMaintenanceTypeMilage = 0;
                                        var PrePreviousMaintenanceTypeMilage = 0;
                                        if (MaintenanceTypeMilage != null && MaintenanceTypeMilage > 0)
                                        {
                                            VehicleMaintenanceJobOrderHistory PreviousMaintenanceJobOrder = new VehicleMaintenanceJobOrderHistory();
                                            long PreviousMaintenanceJobOrderId = 0;
                                            var PreviousMaintenanceCount = 0;
                                            if (VehicleMaintenanceTypeItemsList.Count > 0)
                                            {
                                                PreviousMaintenanceCount = VehicleMaintenanceTypeItemsList.Count;
                                                while (PreviousMaintenanceJobOrderId == 0 && PreviousMaintenanceCount > 0)
                                                {
                                                    var PreviousMaintenance = VehicleMaintenanceTypeItemsList[PreviousMaintenanceCount - 1];
                                                    if (PreviousMaintenance != null)
                                                    {
                                                        PreviousMaintenanceTypeMilage = PreviousMaintenance.Milage ?? 0;
                                                        PreviousMaintenanceJobOrder = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(a => a.Active == true && a.VehiclePerClientId == ClientVehicleId && a.VehicleMaintenanceTypeId == PreviousMaintenance.ID).OrderByDescending(a => a.CreationDate).FirstOrDefault();
                                                        if (PreviousMaintenanceJobOrder != null)
                                                        {
                                                            PreviousMaintenanceJobOrderId = PreviousMaintenanceJobOrder.Id;
                                                        }
                                                    }
                                                    PreviousMaintenanceCount--;
                                                }
                                            }
                                            if (PreviousMaintenanceJobOrderId != 0)
                                            {
                                                if (PreviousMaintenanceCount > 0)
                                                {
                                                    VehicleMaintenanceJobOrderHistory PrePreviousMaintenanceJobOrder = new VehicleMaintenanceJobOrderHistory();
                                                    long PrePreviousMaintenanceJobOrderId = 0;
                                                    var PrePreviousMaintenanceCount = VehicleMaintenanceTypeItemsList.Count - PreviousMaintenanceCount;
                                                    if (PrePreviousMaintenanceCount > 0)
                                                    {
                                                        while (PrePreviousMaintenanceJobOrderId == 0 && PrePreviousMaintenanceCount > 0)
                                                        {
                                                            var PrePreviousMaintenance = VehicleMaintenanceTypeItemsList[PreviousMaintenanceCount - 1];
                                                            if (PrePreviousMaintenance != null)
                                                            {
                                                                PrePreviousMaintenanceTypeMilage = PrePreviousMaintenance.Milage ?? 0;
                                                                PrePreviousMaintenanceJobOrder = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(a => a.Active == true && a.VehiclePerClientId == ClientVehicleId && a.VehicleMaintenanceTypeId == PrePreviousMaintenance.ID).OrderByDescending(a => a.CreationDate).FirstOrDefault();
                                                                if (PrePreviousMaintenanceJobOrder != null)
                                                                {
                                                                    PrePreviousMaintenanceJobOrderId = PrePreviousMaintenanceJobOrder.Id;
                                                                }
                                                            }
                                                            PrePreviousMaintenanceCount--;
                                                        }
                                                    }
                                                    if (PrePreviousMaintenanceJobOrderId != 0)
                                                    {
                                                        var VehicleKiloPerMonthRate = (((PreviousMaintenanceJobOrder.CreationDate.Year - PrePreviousMaintenanceJobOrder.CreationDate.Year) * 12) + PreviousMaintenanceJobOrder.CreationDate.Month - PrePreviousMaintenanceJobOrder.CreationDate.Month) != 0 ? (PreviousMaintenanceJobOrder.Milage - PrePreviousMaintenanceJobOrder.Milage) / (((PreviousMaintenanceJobOrder.CreationDate.Year - PrePreviousMaintenanceJobOrder.CreationDate.Year) * 12) + PreviousMaintenanceJobOrder.CreationDate.Month - PrePreviousMaintenanceJobOrder.CreationDate.Month) : 0;

                                                        var NumberOfMonths = VehicleKiloPerMonthRate != 0 ? (VehicleMaintenanceTypeItemsDB[item].Milage - PreviousMaintenanceTypeMilage) / VehicleKiloPerMonthRate : 0;
                                                        vehicleMaintenanceTypeItem.ExpectedDate = PreviousMaintenanceJobOrder.CreationDate.AddMonths((int)NumberOfMonths).ToShortDateString();
                                                    }
                                                }
                                                else
                                                {
                                                    var VehicleKiloPerMonthRate = ClientVehicle.Odometer != null && ClientVehicle.Odometer != 0 ?
                                                        ((((PreviousMaintenanceJobOrder.CreationDate.Year - ClientVehicle.CreationDate.Year) * 12) + PreviousMaintenanceJobOrder.CreationDate.Month - ClientVehicle.CreationDate.Month) != 0 ? (PreviousMaintenanceJobOrder.Milage - (ClientVehicle.Odometer ?? 0)) / (((PreviousMaintenanceJobOrder.CreationDate.Year - ClientVehicle.CreationDate.Year) * 12) + PreviousMaintenanceJobOrder.CreationDate.Month - ClientVehicle.CreationDate.Month) : 0) :
                                                        ((((PreviousMaintenanceJobOrder.CreationDate.Year - int.Parse(ClientVehicle.Year)) * 12) + PreviousMaintenanceJobOrder.CreationDate.Month - 6) != 0 ? PreviousMaintenanceJobOrder.Milage / (((PreviousMaintenanceJobOrder.CreationDate.Year - int.Parse(ClientVehicle.Year)) * 12) + PreviousMaintenanceJobOrder.CreationDate.Month - 6) : 0);

                                                    var NumberOfMonths = VehicleKiloPerMonthRate != 0 ? (ClientVehicle.Odometer != null && ClientVehicle.Odometer != 0 ? (VehicleMaintenanceTypeItemsDB[item].Milage - ClientVehicle.Odometer) * VehicleKiloPerMonthRate : VehicleMaintenanceTypeItemsDB[item].Milage / VehicleKiloPerMonthRate) : 0;
                                                    vehicleMaintenanceTypeItem.ExpectedDate = PreviousMaintenanceJobOrder.CreationDate.AddMonths((int)NumberOfMonths).ToShortDateString();
                                                }
                                            }
                                            else
                                            {
                                                if (ClientVehicle.Odometer != null && ClientVehicle.Odometer != 0)
                                                {
                                                    var VehicleKiloPerMonthRate = (decimal?)ClientVehicle.Odometer / ((((decimal)ClientVehicle.CreationDate.Year - decimal.Parse(ClientVehicle.Year)) * 12) + (decimal)ClientVehicle.CreationDate.Month - 6);
                                                    //VehicleKiloPerMonthRate = (int)VehicleKiloPerMonthRate;
                                                    if (VehicleKiloPerMonthRate <= 1)
                                                    {
                                                        Response.Result = false;
                                                        Error error = new Error();
                                                        error.ErrorCode = "ErrCRM1";
                                                        error.ErrorMSG = "Vehicle Kilo Per Month Rate is 0";
                                                        Response.Errors.Add(error);
                                                        return Response;
                                                    }
                                                    var NumberOfMonths = (VehicleMaintenanceTypeItemsDB[item].Milage - ClientVehicle.Odometer) / VehicleKiloPerMonthRate;
                                                    vehicleMaintenanceTypeItem.ExpectedDate = ClientVehicle.CreationDate.AddMonths((int)NumberOfMonths).ToShortDateString();
                                                }
                                            }
                                        }
                                    }

                                    VehicleMaintenanceTypeItemsList.Add(vehicleMaintenanceTypeItem);
                                }
                                VehicleMaintenanceTypeItems.VehicleMaintenanceTypeList = VehicleMaintenanceTypeItemsList;
                                Response.VehicleMaintenanceTypes = VehicleMaintenanceTypeItems;
                            }
                        }
                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


    }
}
