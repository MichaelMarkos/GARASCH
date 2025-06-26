using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Identity.Client;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.Client;
using NewGaras.Infrastructure.Models.DDL;
using NewGaras.Infrastructure.Models.DDL.UsedInResponse;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NewGaras.Domain.Services
{
    public class DDLService : IDDLService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _host;
        private readonly IMapper _mapper;
        //private readonly IConfidentialClientApplication _app;
        private readonly string[] _scopes;

        public DDLService(IUnitOfWork unitOfWork, IWebHostEnvironment host,IMapper mapper)
        {

            _unitOfWork = unitOfWork;
            _host = host;
            _mapper = mapper;
        }

        public SelectDDLResponse GetLocalGovernorateList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.Governorates.FindAll(x => x.Active == true && x.CountryId == 1).ToList();
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


        public SelectDDLResponse GetInventoryItemPartNoList(string SearchKey)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var QuerableDB = _unitOfWork.InventoryItems.FindAllQueryable(x => x.Active == true && !string.IsNullOrWhiteSpace(x.PartNo));
                    if (!string.IsNullOrEmpty(SearchKey))
                    {
                        SearchKey = HttpUtility.UrlDecode(SearchKey);
                        QuerableDB = QuerableDB.Where(x => x.PartNo.ToLower().Contains(SearchKey.ToLower())).AsQueryable();
                    }
                    var ListDB = QuerableDB.Take(10).ToList();
                    if (ListDB.Count() > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.PartNo;

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

        public SelectDDLResponse GetPurchasePOList(long? SupplierID)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.VPurchasePos.FindAll(x => x.Active == true).ToList();
                    if (SupplierID != 0)
                    {
                        ListDB = ListDB.Where(x => x.ToSupplierId == SupplierID).ToList();
                    }
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = "PO_" + item.Id;

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

        public SelectDDLResponse GetMatrialRequestTypeList()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.InventoryMaterialRequestTypes.FindAll(x => x.Active == true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.TypeName;

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

        public OfferItemDDLResponse GetProductOfferItemList(long ProjectFabricationID, string SearchKey, int CurrentPage = 1, int NumberOfItemsPerPage = 10)
        {
            OfferItemDDLResponse Response = new OfferItemDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<OfferItemDDL>();
                if (Response.Result)
                {
                    var ProjectFabricationBOQAsQueryable = _unitOfWork.ProjectFabricationBOQs.FindAllQueryable(x =>x.Active == true).AsQueryable();
                    if (ProjectFabricationID != 0)
                    {
                        ProjectFabricationBOQAsQueryable = ProjectFabricationBOQAsQueryable.Where(x => x.ProjectFabricationId == ProjectFabricationID).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(SearchKey))
                    {

                        SearchKey = HttpUtility.UrlDecode(SearchKey);
                        ProjectFabricationBOQAsQueryable = ProjectFabricationBOQAsQueryable.Where(x => x.ItemSerial == SearchKey).AsQueryable();

                    }
                    var ProjectFabricationBOQAsList = PagedList<ProjectFabricationBoq>.Create(ProjectFabricationBOQAsQueryable.OrderBy(x => x.CreationDate), CurrentPage, NumberOfItemsPerPage);

                    var OfferItemListDB = ProjectFabricationBOQAsList.ToList();
                    if (OfferItemListDB.Count > 0)
                    {
                        var ListIDSProjectFabrication = OfferItemListDB.Select(x => x.ProjectFabricationId).ToList();
                        var ProjectFabricationsList = _unitOfWork.ProjectFabrications.FindAll(x =>ListIDSProjectFabrication.Contains(x.Id), includes: new[] { "Project.SalesOffer" }).ToList();

                        var SalesOfferProductIdList = OfferItemListDB.Select(x => x.SalesOfferProductId).ToList();
                        var V_SalesOfferProductList = _unitOfWork.VSalesOfferProducts.FindAll(a => SalesOfferProductIdList.Contains(a.Id)).ToList();
                        foreach (var item in OfferItemListDB)
                        {
                            long? ProdID = item.SalesOfferProductId;

                            if (ProdID != null)
                            {
                                var DLLObj = new OfferItemDDL();
                                var SalesOfferProduct = V_SalesOfferProductList.Where(a => a.Id == ProdID).FirstOrDefault();
                                if (SalesOfferProduct != null)
                                {
                                    DLLObj.ProductName = SalesOfferProduct.InventoryItemName;
                                }
                                string FabNumber = "";
                                var PricingProductObjDB = ProjectFabricationsList.Where(x => x.Id == item.ProjectFabricationId).FirstOrDefault();
                                if (PricingProductObjDB != null)
                                {
                                    if (PricingProductObjDB.FabNumber != null)
                                    {
                                        FabNumber = PricingProductObjDB.FabNumber.ToString();
                                    }
                                }
                                DLLObj.ID = (long)ProdID;
                                DLLObj.ItemSerial = item.ItemSerial;
                                DLLObj.FabID = item.ProjectFabricationId;
                                DLLObj.FabNo = FabNumber;
                                DLLObj.FabSerial = PricingProductObjDB.FabOrderSerial;
                                DLLObj.FabOrderItemSerial = item.ItemSerial;
                                DLLObj.FabOrderItemID = item.Id;
                                DLLObj.ProjectID = PricingProductObjDB.ProjectId;
                                DLLObj.ProjectName = PricingProductObjDB.Project?.SalesOffer.ProjectName;
                                DLLObj.ProjectSerial = PricingProductObjDB.Project.ProjectSerial;
                                DDLList.Add(DLLObj);
                            }

                        }
                    }




                    if (!string.IsNullOrEmpty(SearchKey))
                    {
                        DDLList = DDLList.Where(x => SearchKey.Contains(x.ProjectName) || SearchKey.Contains(x.ItemSerial)).ToList();
                    }
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = ProjectFabricationBOQAsList.TotalPages,
                        ItemsPerPage = NumberOfItemsPerPage,
                        TotalItems = ProjectFabricationBOQAsList.TotalCount
                    };
                }

                Response.DDLList = DDLList.Distinct().ToList();
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

        public CountriesGovernoratesAreasDDLs GetCountriesGovernoratesAreasDDLs(string CompName)
        {
            CountriesGovernoratesAreasDDLs Response = new CountriesGovernoratesAreasDDLs();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var CompanyName = CompName.ToString().ToLower();

                    var CountriesList = _unitOfWork.Countries.FindAll(x => x.Active == true).ToList();
                    if (CompanyName == "marinapltq")
                    {
                        CountriesList = CountriesList.Where(x => x.Id == 2).ToList();
                    }
                    var CountriesDDL = new List<SelectDDL>();
                    foreach (var C in CountriesList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = C.Id;
                        DDLObj.Name = C.Name;

                        CountriesDDL.Add(DDLObj);
                    }
                    Response.CountriesDDL = CountriesDDL;

                    var GovernoratesList = _unitOfWork.Governorates.FindAll(x => x.Active == true).ToList();
                    var GovernoratesDDL = new List<GovernorateDDL>();
                    foreach (var G in GovernoratesList)
                    {
                        var DDLObj = new GovernorateDDL();
                        DDLObj.ID = G.Id;
                        DDLObj.Name = G.Name;
                        DDLObj.CountryId = G.CountryId;

                        GovernoratesDDL.Add(DDLObj);
                    }
                    Response.GoverneratesDDL = GovernoratesDDL;

                    var AreasList = _unitOfWork.Areas.GetAll();
                    var AreasDDL = new List<AreaDDL>();
                    foreach (var A in AreasList)
                    {
                        var DDLObj = new AreaDDL();
                        DDLObj.ID = A.Id;
                        DDLObj.Name = A.Name;
                        DDLObj.GovernorateId = A.GovernorateId;

                        AreasDDL.Add(DDLObj);
                    }
                    Response.AreasDDL = AreasDDL;

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

        public SellingProductsDDLResponse GetSellingProductsDDL()
        {
            SellingProductsDDLResponse Response = new SellingProductsDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var ProductsList = _unitOfWork.VSalesOfferProductSalesOffers.FindAll(a => a.Status == "Closed" && a.Active == true).GroupBy(a => new { a.InventoryItemId, a.Name }).ToList();
                    var ProductsDDL = new List<SelectDDL>();
                    foreach (var S in ProductsList)
                    {
                        var DDLObj = new SelectDDL();
                        DDLObj.ID = S.Key.InventoryItemId ?? 0;
                        DDLObj.Name = S.Key.Name;

                        ProductsDDL.Add(DDLObj);
                    }
                    Response.SellingProductsDDL = ProductsDDL;
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

        public BaseResponseWithData<List<GetPriorityModel>> GetPriority()
        {
            BaseResponseWithData<List<GetPriorityModel>> Response = new BaseResponseWithData<List<GetPriorityModel>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var priorities = _unitOfWork.Prioritys.GetAll();
                var PriorityList = _mapper.Map<List<GetPriorityModel>>(priorities);
                Response.Data = PriorityList;
                return Response;
            }
            catch(Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public SelectDDLResponse GetNationalityDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.Nationalities.FindAll(x =>true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Nationality1;

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

        public SelectDDLResponse GetMilitaryStatusDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.MilitaryStatuses.FindAll(x => true).ToList();
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

        public SelectDDLResponse GetAttachmentTypeDDL()
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.AttachmentTypes.FindAll(x => true).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Type;

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

    }
}
