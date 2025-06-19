using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using NewGaras.Infrastructure.Entities;
using NewGaras.Domain.Models;
using Org.BouncyCastle.Bcpg;
using NewGaras.Infrastructure.DTO.InventoryItem;
using NewGaras.Infrastructure.Models.InventoryItem;
using DocumentFormat.OpenXml.Bibliography;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models.Inventory.Responses;
using NewGaras.Infrastructure.Models.Inventory;
using NewGarasAPI.Helper;
using System.Net;
using System.Web;
using NewGarasAPI.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models.Inventory.Requests;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Models.Account;
using System.Data;
using OfficeOpenXml.Style;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Error = NewGarasAPI.Models.Common.Error;
using Azure;
using Path = System.IO.Path;
using NewGaras.Infrastructure.Models.SalesOffer;
using ClosedXML.Excel;
using NewGaras.Infrastructure.Models.InventoryItem.UsedInResponse;
using System.Security.Cryptography.X509Certificates;
using NewGaras.Infrastructure.Models.InventoryItem.Filters;
using Microsoft.EntityFrameworkCore;
using NewGarasAPI.Models.Admin;
using static NewGaras.Domain.Helper.TreeStructure;
using NewGarasAPI.Models.HR;
using NewGaras.Infrastructure.Models.Admin;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using NewGarasAPI.Models.Inventory;
using InventoryItem = NewGaras.Infrastructure.Entities.InventoryItem;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using NewGaras.Infrastructure.Models.InventoryAddingOrder;
using Tuple = System.Tuple;
using NewGarasAPI.Models.AccountAndFinance;
using NewGarasAPI.Models.User;
using iTextSharp.text.pdf;
using iTextSharp.text;
using HeaderFooter = NewGarasAPI.Models.AccountAndFinance.HeaderFooter;
using Newtonsoft.Json;
using System.Drawing;
using System.IO.Packaging;
using NewGaras.Infrastructure.Models.User.Response;
//using NewGaras.Infrastructure.Models.User.UsedInResponse;
using NewGaras.Infrastructure.Models.User.Filters;
using NewGaras.Infrastructure.Models.AccountAndFinance;
using NewGaras.Infrastructure.Models.Common;
//using NewGaras.Infrastructure.Models.ItemsPricing;

namespace NewGaras.Domain.Services.Inventory
{
    public class InventoryItemService : IInventoryItemService
    {
        private GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IUnitOfWork _unitOfWork;
        static readonly string key = "SalesGarasPass";
        public string BaseCurrencyConverterApiAddress = "https://api.exchangerate.host/";
        public string CurrencyConvertorAddress = "convert?format=json";
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

        public InventoryItemService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _Context = new GarasTestContext(_tenantService);
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }
        public BaseResponseWithData<string> GetInventoryItemExcelTemplete(string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();


            var filePath = System.IO.Path.Combine(_host.WebRootPath, "Attachments/Tampletes/InventoryItemTemplete.xlsx");

            var InventoryItemCategories = _unitOfWork.InventoryItemCategories.FindAllQueryable(a => a.Active == true).Select(a => new { a.Id, a.Name }).ToList();

            var RequstionUOMs = _unitOfWork.InventoryUoms.FindAllQueryable(a => a.Active == true).Select(a => new { a.Id, a.Name, a.ShortName }).ToList();
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    ExcelPackage package = new ExcelPackage(new FileInfo(filePath));
                    ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                    //fill the list of InventoryItemCategory in Excel file to let the user choose from them
                    for (int row = 0; row < InventoryItemCategories.Count(); row++)
                    {

                        sheet.Cells[row + 2, 1].Value = InventoryItemCategories[row].Name;
                        sheet.Cells[row + 2, 2].Value = InventoryItemCategories[row].Id;

                    }


                    //fill the list of RequstionUOM and PurchasingUOM in Excel file to let the user choose from them
                    for (int row = 0; row < RequstionUOMs.Count(); row++)
                    {
                        sheet.Cells[row + 2, 4].Value = RequstionUOMs[row].Name + "(" + RequstionUOMs[row].ShortName + ")";
                        sheet.Cells[row + 2, 5].Value = RequstionUOMs[row].Id;

                        sheet.Cells[row + 2, 7].Value = RequstionUOMs[row].Name + "(" + RequstionUOMs[row].ShortName + ")";
                        sheet.Cells[row + 2, 8].Value = RequstionUOMs[row].Id;

                    }

                    var dirPath = System.IO.Path.Combine(_host.WebRootPath, $"Attachments\\{CompName}\\resultTampletes\\");
                    Directory.CreateDirectory(dirPath);

                    var finalFilePath = dirPath + $"resultInventoryItemTemplete.xlsx";
                    package.SaveAs(finalFilePath);
                    package.Dispose();
                    var fixedPath = $"Attachments\\{CompName}\\resultTampletes\\resultInventoryItemTemplete.xlsx";

                    response.Data = Globals.baseURL + fixedPath;
                    return response;
                }
                else
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = "The Templete File is not exsists";
                    response.Errors.Add(error);
                    return response;
                }
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

        public BaseResponseWithMessage<string> UploadInventoryItemExcel(AddAttachment dto, long UserID)
        {
            var response = new BaseResponseWithMessage<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                var inventoryItemsList = new List<InventoryItem>();
                var ExcelErrorList = new List<ExcelInventoryItemErrors>();
                var UploadInventoryItemExcel = new UploadInventoryItemExcelResponse();
                var listOfIDAddedToDB = new List<long>();

                //----------------------log file Info. ------------------------------------------
                int FailedCount = 0;
                int SuccessCount = 0;
                int totalRows = 0;

                using (var stream = dto.Content.OpenReadStream())       //just read the file without saving it  on the disk

                using (var package = new ExcelPackage(stream))          //walking through the file
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[0];

                    int rows = sheet.Dimension.Rows;
                    int columns = sheet.Dimension.Columns;


                    //------------------------check that code and name are unique at the excel file--------------------------------
                    var listOfCodes = new List<string>();
                    var listOfNames = new List<string>();
                    var ListOfPartNumbers = new List<string>();
                    for (int row = 2; row < rows; row++)
                    {
                        if ((string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text)) &&
                            (string.IsNullOrEmpty(sheet.Cells[row, 2].Text)) &&
                            (sheet.Cells[row, 8].Text == "#N/A")) break;

                        listOfCodes.Add(sheet.Cells[row, 1].Text);
                        listOfNames.Add(sheet.Cells[row, 2].Text);
                        ListOfPartNumbers.Add(sheet.Cells[row, 4].Text);
                    }
                    //-------------------------------------------------------------------------------------------------------------


                    var inventoryItemsListFromDB = _unitOfWork.InventoryItems.FindAllQueryable(a => a.Active == true).Select(a => new { a.Name, a.Code, a.PartNo }).ToList();
                    for (int row = 2; row < rows; row++)
                    {
                        if ((string.IsNullOrWhiteSpace(sheet.Cells[row, 1].Text)) &&
                            (string.IsNullOrEmpty(sheet.Cells[row, 2].Text)) &&
                            (sheet.Cells[row, 8].Text == "#N/A")) break;                     //this for break the loop when the data ends

                        bool takeThisRecored = true;                                        //flag to determine which recored is valid to add in DB and which is not



                        var inventoryItem = new InventoryItem();

                        //---------------------check that name and code are unique-----------------------------------------------
                        string Code = sheet.Cells[row, 1].Text;
                        if (!string.IsNullOrWhiteSpace(Code))
                        {
                            if (inventoryItemsListFromDB.Where(a => a.Code.Trim() == Code.Trim()).Count() != 0 || listOfCodes.Where(a => a.Trim() == Code.Trim()).Count() > 1)
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"The Code you entered is already exists at record number {row - 1}, please enter a valid one",
                                    NoOfRow = row,
                                };
                                ExcelErrorList.Add(error);
                            }
                            else
                            {
                                inventoryItem.Code = sheet.Cells[row, 1].Text;
                            }
                        }
                        else
                        {
                            takeThisRecored = false;
                            var error = new ExcelInventoryItemErrors()
                            {
                                ErrMsg = $"please Enter a valid Code at record number {row - 1}",
                                NoOfRow = row,
                            };
                            ExcelErrorList.Add(error);
                        }


                        string name = sheet.Cells[row, 2].Text;
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            if (inventoryItemsListFromDB.Where(a => a.Name.Trim() == name.Trim()).Count() != 0 || listOfNames.Where(a => a.Trim() == name.Trim()).Count() > 1)
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"The name you entered is already exists at record number {row - 1}, please enter a valid one",
                                    NoOfRow = row,
                                };
                                ExcelErrorList.Add(error);
                            }
                            else
                            {
                                inventoryItem.Name = sheet.Cells[row, 2].Text;
                            }
                        }
                        else
                        {
                            takeThisRecored = false;
                            var error = new ExcelInventoryItemErrors()
                            {
                                ErrMsg = $"please Enter a valid name at record number {row - 1}",
                                NoOfRow = row,
                            };
                            ExcelErrorList.Add(error);
                        }
                        //------------------------------------------------------------------------------------------------------

                        //inventoryItem.Code = sheet.Cells[row, 1].Text;
                        //inventoryItem.Name = sheet.Cells[row, 2].Text;
                        inventoryItem.MarketName = sheet.Cells[row, 3].Text;

                        string partNumber = sheet.Cells[row, 4].Text;
                        if (!string.IsNullOrWhiteSpace(partNumber))
                        {
                            if (inventoryItemsListFromDB.Where(a => a.PartNo == partNumber.Trim()).Count() != 0 || listOfCodes.Where(a => a.Trim() == partNumber.Trim()).Count() > 1)
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"The part Number you entered is already exists at record number {row - 1}, please enter a valid one",
                                    NoOfRow = row,
                                };
                                ExcelErrorList.Add(error);
                            }
                            inventoryItem.PartNo = sheet.Cells[row, 4].Text;
                        }


                        //---------------------------try to parse data to datatype in DB ----------------------------------------
                        int InventoryItemCategoryId = 0;
                        if (int.TryParse(sheet.Cells[row, 6].Text, out InventoryItemCategoryId))
                        {
                            inventoryItem.InventoryItemCategoryId = InventoryItemCategoryId;
                        }
                        else
                        {
                            takeThisRecored = false;
                            var error = new ExcelInventoryItemErrors()
                            {
                                ErrMsg = $"please Enter a valid InventoryItemCategory ID (integer ) at record number {row - 1}",
                                NoOfRow = row,
                            };
                            ExcelErrorList.Add(error);
                        }

                        int PurchasingUomid = 0;
                        if (int.TryParse(sheet.Cells[row, 8].Text, out PurchasingUomid))
                        {
                            inventoryItem.PurchasingUomid = PurchasingUomid;
                        }
                        else
                        {
                            takeThisRecored = false;
                            var error = new ExcelInventoryItemErrors()
                            {
                                ErrMsg = $"please Enter a valid PurchasingUom ID (integer ) at record number {row - 1}",
                                NoOfRow = row,
                            };
                            ExcelErrorList.Add(error);
                        }

                        int RequstionUomid = 0;
                        if (int.TryParse(sheet.Cells[row, 10].Text, out RequstionUomid))
                        {
                            inventoryItem.RequstionUomid = RequstionUomid;
                        }
                        else
                        {
                            takeThisRecored = false;
                            var error = new ExcelInventoryItemErrors()
                            {
                                ErrMsg = $"please Enter a valid RequstionUom ID (integer ) at record number {row - 1}",
                                NoOfRow = row,
                            };
                            ExcelErrorList.Add(error);
                        }

                        if (!string.IsNullOrWhiteSpace(sheet.Cells[row, 11].Text))
                        {

                            double ExchangeFactor = 0.0;
                            if (double.TryParse(sheet.Cells[row, 11].Text, out ExchangeFactor))
                            {
                                inventoryItem.ExchangeFactor1 = decimal.Parse(sheet.Cells[row, 11].Text);
                            }
                            else
                            {
                                takeThisRecored = false;
                                var error = new ExcelInventoryItemErrors()
                                {
                                    ErrMsg = $"please Enter a valid ExchangeFactor (double) at record number {row - 1}",
                                    NoOfRow = row,
                                };
                                ExcelErrorList.Add(error);
                            }
                        }

                        decimal CustomeUnitPrice = 0.0m;
                        if (decimal.TryParse(sheet.Cells[row, 12].Text, out CustomeUnitPrice))
                        {
                            inventoryItem.CustomeUnitPrice = decimal.Parse(sheet.Cells[row, 12].Text);
                        }
                        //else
                        //{
                        //    takeThisRecored = false;
                        //    var error = new ExcelInventoryItemErrors()
                        //    {
                        //        ErrMsg = $"please Enter a valid CustomeUnitPrice (decimal) at record number {row - 1}",
                        //        NoOfRow = row,
                        //    };
                        //    ExcelErrorList.Add(error);
                        //}


                        if (!string.IsNullOrWhiteSpace(sheet.Cells[row, 13].Text)) inventoryItem.CostName1 = sheet.Cells[row, 13].Text;
                        if (!string.IsNullOrWhiteSpace(sheet.Cells[row, 14].Text)) inventoryItem.CostName2 = sheet.Cells[row, 14].Text;
                        if (!string.IsNullOrWhiteSpace(sheet.Cells[row, 15].Text)) inventoryItem.CostName3 = sheet.Cells[row, 15].Text;



                        //defualt values
                        inventoryItem.CalculationType = 4;
                        inventoryItem.AverageUnitPrice = 0;
                        inventoryItem.MaxUnitPrice = 0;
                        inventoryItem.LastUnitPrice = 0;
                        inventoryItem.Active = true;

                        inventoryItem.CreatedBy = UserID;
                        inventoryItem.CreationDate = DateTime.Now;
                        inventoryItem.ModifiedBy = UserID;
                        inventoryItem.ModifiedDate = DateTime.Now;

                        if (takeThisRecored)
                        {
                            inventoryItemsList.Add(inventoryItem);
                            SuccessCount++;
                        }
                        totalRows++;
                    }
                }

                //if(inventoryItemsList.Count() > 0)
                //{
                //    var newInventoryItems = _unitOfWork.InventoryItems.AddRange(inventoryItemsList);
                //    _unitOfWork.Complete();

                //    if(newInventoryItems != null)
                //    {
                //        foreach (var item in newInventoryItems)
                //        {
                //            listOfIDAddedToDB.Add(item.Id);
                //        }
                //    }
                //}

                string filePath = System.IO.Path.Combine(_host.WebRootPath, "Attachments/Tampletes/InventoryItemErrorLog.txt"); ;
                if (ExcelErrorList.Count() > 0)
                {
                    File.WriteAllText(filePath, string.Empty);
                    // Create a StreamWriter and write lines to the file
                    using (StreamWriter writer = new StreamWriter(filePath, append: false)) // `append: true` to add to an existing file
                    {
                        writer.WriteLine("Success Rows: " + SuccessCount);
                        writer.WriteLine("Failed Rows: " + (totalRows - SuccessCount));
                        writer.WriteLine("Total Rows: " + totalRows + "\n");
                        foreach (var error in ExcelErrorList)
                        {
                            writer.WriteLine($"Number of row: " + (error.NoOfRow - 1) + ", " + "Error message: " + error.ErrMsg);
                        }

                    }
                }
                else
                {
                    File.WriteAllText(filePath, string.Empty);
                    // Create a StreamWriter and write lines to the file
                    using (StreamWriter writer = new StreamWriter(filePath, append: false)) // `append: true` to add to an existing file
                    {
                        writer.WriteLine("Success Rows: " + SuccessCount);
                        writer.WriteLine("Failed Rows: " + (totalRows - SuccessCount));
                        writer.WriteLine("Total Rows: " + totalRows + "\n");
                    }
                }

                string fixedPath = "Attachments/Tampletes/InventoryItemErrorLog.txt";

                //UploadInventoryItemExcel.ListOfAddedIDs = listOfIDAddedToDB;
                //UploadInventoryItemExcel.ErrorFilePath = Globals.baseURL + fixedPath;
                response.Message = Globals.baseURL + fixedPath;
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

        public GetOfferInventoryItemsListForPOSResponse GetOfferInventoryItemsListForPOS([FromHeader] GetOfferInventoryItemsFilters filters, string companyname) // Not Used
        {
            GetOfferInventoryItemsListForPOSResponse Response = new GetOfferInventoryItemsListForPOSResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    /*long InventoryItemId = 0;
                    if (!string.IsNullOrEmpty(headers["InventoryItemId"]) && long.TryParse(headers["InventoryItemId"], out InventoryItemId))
                    {
                        long.TryParse(headers["InventoryItemId"], out InventoryItemId);
                    }
                    int CategoryId = 0;
                    if (!string.IsNullOrEmpty(headers["CategoryId"]) && int.TryParse(headers["CategoryId"], out CategoryId))
                    {
                        int.TryParse(headers["CategoryId"], out CategoryId);
                    }*/
                    /* int StoreId = 0;
                     if (!string.IsNullOrEmpty(headers["StoreId"]) && int.TryParse(headers["StoreId"], out StoreId))
                     {
                         int.TryParse(headers["StoreId"], out StoreId);
                     }
                     else*/
                    if (filters.StoreId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Invalid Store Id";
                        Response.Errors.Add(error);
                    }

                    if (!string.IsNullOrEmpty(filters.InventoryItemName))
                    {
                        filters.InventoryItemName = filters.InventoryItemName.ToLower();
                    }

                    /*                    int CurrentPage = 1;
                                        if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                                        {
                                            int.TryParse(headers["CurrentPage"], out CurrentPage);
                                        }*/

                    /*                    int NumberOfItemsPerPage = 10;
                                        if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                                        {
                                            int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                                        }
                    */
                    if (Response.Result)
                    {
                        var InventoryItemsListQuery = _unitOfWork.InventoryItems.FindAllQueryable(a => true, includes: new[] { "PurchasingUom", "RequstionUom", "InventoryItemCategory", "Priority" }).AsQueryable();
                        if (filters.InventoryItemId != 0)
                        {
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.Id == filters.InventoryItemId).AsQueryable();
                        }
                        if (filters.CategoryId != 0)
                        {
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.InventoryItemCategoryId == filters.CategoryId).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.InventoryItemName))
                        {
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => a.Name.Contains(filters.InventoryItemName)).AsQueryable();
                        }
                        var InventoryStoreItemList = _unitOfWork.VInventoryStoreItems.FindAllQueryable(a => true).AsQueryable();
                        if (filters.StoreId != 0)
                        {
                            InventoryStoreItemList = InventoryStoreItemList.Where(a => a.InventoryStoreId == filters.StoreId && a.StoreActive == true && a.FinalBalance > 0).AsQueryable();
                            var InventoryStoreItemsIds = InventoryStoreItemList.Select(a => a.InventoryItemId).Distinct().ToList();
                            InventoryItemsListQuery = InventoryItemsListQuery.Where(a => InventoryStoreItemsIds.Contains(a.Id)).AsQueryable();
                        }


                        // For Item POS 

                        var InventoryItemsPagedList = PagedList<InventoryItem>.Create(InventoryItemsListQuery.OrderBy(x => x.Name), filters.CurrentPage, filters.NumberOfItemsPerPage);
                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = InventoryItemsPagedList.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = InventoryItemsPagedList.TotalCount
                        };


                        //var InventoryItemsList = InventoryItemsListQuery.ToList();
                        // var InventoryItemsListResponse = new List<InventoryItemInfoForPOS>();
                        var InventoryItemsListResponse = InventoryItemsPagedList.Select(InventoryItemObjDB => new InventoryItemInfoForPOS
                        {
                            ID = InventoryItemObjDB.Id,
                            ItemName = InventoryItemObjDB.Name,
                            ItemCode = InventoryItemObjDB.Code,
                            Category = InventoryItemObjDB.InventoryItemCategory.Name,
                            CategoryId = InventoryItemObjDB.InventoryItemCategoryId,
                            PurchasingUnit = InventoryItemObjDB.PurchasingUom.ShortName,
                            RequestionUnit = InventoryItemObjDB.RequstionUom.ShortName,
                            Balance = InventoryStoreItemList.Where(item => item.InventoryItemId == InventoryItemObjDB.Id).Select(x => x.FinalBalance ?? 0).Sum(),
                            ConvertRateFromPurchasingToRequestionUnit = InventoryItemObjDB.ExchangeFactor1 != null ? (decimal)InventoryItemObjDB.ExchangeFactor1 : 0,

                            ItemImage = (InventoryItemObjDB.ImageUrl != null && InventoryItemObjDB.Id != 0) ? Globals.baseURL + InventoryItemObjDB.ImageUrl : null,
                            Cost1 = InventoryItemObjDB.CostAmount1,
                            Cost2 = InventoryItemObjDB.CostAmount2,
                            Cost3 = InventoryItemObjDB.CostAmount3,
                            CustomPrice = InventoryItemObjDB.CustomeUnitPrice,
                            RequestionUOMShortName = InventoryItemObjDB.RequstionUom.ShortName
                        }).ToList();

                        Response.InventoryItemsList = InventoryItemsListResponse;
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

        public List<InventoryStoreItemIDWithQTY> GetParentReleaseIDWithSettingStore(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemID, int StoreID, int? StoreLocationID, decimal QTY, bool? IsFIFO)
        {
            List<InventoryStoreItemIDWithQTY> ParentInvStoreItemIDWithBalanceList = new List<InventoryStoreItemIDWithQTY>();
            if (IsFIFO == false) // LIFO
            {
                //ParentInvStoreItemIDWithBalanceList = _Context.proc_InventoryStoreItemLoadAll().Where(x => x.InventoryItemID == InventoryItemID
                //                                                                   && x.InventoryStoreID == StoreID
                //                                                                   && (StoreLocationID != null ? x.InvenoryStoreLocationID == StoreLocationID : true)
                //                                                                   && x.finalBalance <= QTY).OrderByDescending(x => x.CreationDate).Select(x => x.ID).FirstOrDefault();
                ParentInvStoreItemIDWithBalanceList = InventoryStoreItemList.Where(x => x.InventoryItemId == InventoryItemID
                                                                   && x.InventoryStoreId == StoreID
                                                                   && (StoreLocationID != null ? x.InvenoryStoreLocationId == StoreLocationID : true)
                                                                   && x.FinalBalance > 0
                                                                   ).OrderByDescending(x => x.CreationDate).Select(x => new InventoryStoreItemIDWithQTY { ID = x.Id, StockBalance = (decimal)x.FinalBalance }).ToList();
            }
            else // FIFO  or Default FIFO
            {
                ParentInvStoreItemIDWithBalanceList = InventoryStoreItemList.Where(x => x.InventoryItemId == InventoryItemID
                                                                                   && x.InventoryStoreId == StoreID
                                                                                   && (StoreLocationID != null ? x.InvenoryStoreLocationId == StoreLocationID : true)
                                                                                   && x.FinalBalance > 0
                                                                                   ).OrderBy(x => x.CreationDate).Select(x => new InventoryStoreItemIDWithQTY { ID = x.Id, StockBalance = (decimal)x.FinalBalance }).ToList();
            }
            return ParentInvStoreItemIDWithBalanceList;
        }

        public bool AddInventoryStoreItemWithReturn(InventoryStoreItem ParentInventoryStoreItem, long InventoryItemId, decimal QTY, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId)
        {
            bool Result = false;
            string OperationType = "Sales Offer Return";

            var InventoryStoreItemOBJ = new InventoryStoreItem();
            InventoryStoreItemOBJ.InventoryStoreId = ParentInventoryStoreItem.InventoryStoreId;
            InventoryStoreItemOBJ.InventoryItemId = InventoryItemId;
            InventoryStoreItemOBJ.OrderNumber = SalesOfferId.ToString();
            InventoryStoreItemOBJ.OrderId = SalesOfferId;
            InventoryStoreItemOBJ.CreatedBy = ValidateUserId;
            InventoryStoreItemOBJ.ModifiedBy = ValidateUserId;
            InventoryStoreItemOBJ.CreationDate = DateTime.Now;
            InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
            InventoryStoreItemOBJ.OperationType = OperationType;
            InventoryStoreItemOBJ.Balance = (double)QTY;
            InventoryStoreItemOBJ.Balance1 = QTY;
            InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
            InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
            InventoryStoreItemOBJ.ReleaseParentId = ParentInventoryStoreItem.Id;
            InventoryStoreItemOBJ.FinalBalance = QTY;
            InventoryStoreItemOBJ.AddingOrderItemId = SalesOfferProductId;
            InventoryStoreItemOBJ.AddingFromPoid = ParentInventoryStoreItem.AddingFromPoid;
            InventoryStoreItemOBJ.PoinvoiceId = ParentInventoryStoreItem.PoinvoiceId;
            InventoryStoreItemOBJ.PoinvoiceTotalPrice = ParentInventoryStoreItem.PoinvoiceTotalPrice;
            InventoryStoreItemOBJ.PoinvoiceTotalCost = ParentInventoryStoreItem.PoinvoiceTotalCost;
            InventoryStoreItemOBJ.CurrencyId = ParentInventoryStoreItem.CurrencyId;
            InventoryStoreItemOBJ.RateToEgp = ParentInventoryStoreItem.RateToEgp;
            InventoryStoreItemOBJ.PoinvoiceTotalPriceEgp = ParentInventoryStoreItem.PoinvoiceTotalPriceEgp;
            InventoryStoreItemOBJ.PoinvoiceTotalCostEgp = ParentInventoryStoreItem.PoinvoiceTotalCostEgp;
            InventoryStoreItemOBJ.RemainItemPrice = ParentInventoryStoreItem.RemainItemPrice;
            InventoryStoreItemOBJ.RemainItemCosetEgp = ParentInventoryStoreItem.RemainItemCosetEgp;
            InventoryStoreItemOBJ.RemainItemCostOtherCu = ParentInventoryStoreItem.RemainItemCostOtherCu;
            InventoryStoreItemOBJ.HoldQty = 0;
            InventoryStoreItemOBJ.HoldReason = null;
            _Context.InventoryStoreItems.Add(InventoryStoreItemOBJ);
            var InventoryStorItemInsertion = _Context.SaveChanges();







            if (ParentInventoryStoreItem.AddingOrderItemId != 0)
            {
                //var ListSalesOfferProductIDS = _Context.SalesOfferProducts.Where(x => x.OfferID == SalesOfferID && x.InventoryItemID == InternalBackDataOBJ.InventoryItemID).OrderBy(x=>x.ReleasedQty)
                var SalesOfferProdDB = _Context.SalesOfferProducts.Where(x => x.Id == ParentInventoryStoreItem.AddingOrderItemId /*&& x.Quantity >= (x.ReleasedQty ?? 0)*/)
                    .OrderBy(x => x.ReleasedQty).FirstOrDefault();
                if (SalesOfferProdDB != null)
                {
                    SalesOfferProdDB.ReleasedQty += (double)QTY;
                }

                _Context.SaveChanges();
            }

            if (InventoryStorItemInsertion > 0)
            {
                Result = true;
            }

            return Result;
        }

        public void AddInvntoryStoreItemWithRelease(List<InventoryStoreItem> InventoryStoreItemList, long InventoryItemId, int StoreID, decimal QTY, bool IsFIFO, long SalesOfferId, long? SalesOfferProductId, long ValidateUserId)
        {


            List<InventoryStoreItemIDWithQTY> AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemList,
                                                                            InventoryItemId,
                                                                            StoreID,
                                                                            null,// store location
                                                                            QTY,
                                                                            IsFIFO);
            // Add Store Item
            string OperationType = "SalesOffer";
            decimal RemainReleaseQTY = QTY; // 20


            // List Of IDS inserted
            List<long> ListIDSUpdate = new List<long>();

            foreach (var ObjParentRelease in AvailableItemStockList)  // 20 -  10   - 5
            {
                decimal ReleaseQTY = 0;
                if (RemainReleaseQTY <= ObjParentRelease.StockBalance)
                {
                    ReleaseQTY = RemainReleaseQTY;
                }
                else
                {

                    ReleaseQTY = ObjParentRelease.StockBalance;
                }

                if (RemainReleaseQTY > 0)
                {
                    RemainReleaseQTY -= ReleaseQTY;
                    var ParentInventoryStoreItem = InventoryStoreItemList.Where(x => x.Id == ObjParentRelease.ID).FirstOrDefault();
                    long? POID = null;
                    long? POInvoiceId = null;
                    decimal? POInvoiceTotalPrice = null;
                    decimal? POInvoiceTotalCost = null;
                    int? currencyId = null;
                    decimal? rateToEGP = null;
                    decimal? POInvoiceTotalPriceEGP = null;
                    decimal? POInvoiceTotalCostEGP = null;
                    decimal? remainItemPrice = null;
                    decimal? remainItemCosetEGP = null;
                    decimal? remainItemCostOtherCU = null;
                    if (ParentInventoryStoreItem != null)
                    {
                        POID = ParentInventoryStoreItem.AddingFromPoid;
                        POInvoiceId = ParentInventoryStoreItem.PoinvoiceId;
                        POInvoiceTotalPrice = ParentInventoryStoreItem.PoinvoiceTotalPrice;
                        POInvoiceTotalCost = ParentInventoryStoreItem.PoinvoiceTotalCost;
                        currencyId = ParentInventoryStoreItem.CurrencyId;
                        rateToEGP = ParentInventoryStoreItem.RateToEgp;
                        POInvoiceTotalPriceEGP = ParentInventoryStoreItem.PoinvoiceTotalPriceEgp;
                        POInvoiceTotalCostEGP = ParentInventoryStoreItem.PoinvoiceTotalCostEgp;
                        remainItemPrice = ParentInventoryStoreItem.RemainItemPrice;
                        remainItemCosetEGP = ParentInventoryStoreItem.RemainItemCosetEgp;
                        remainItemCostOtherCU = ParentInventoryStoreItem.RemainItemCostOtherCu;

                        decimal? finalBalance = (ParentInventoryStoreItem.FinalBalance - (ReleaseQTY)) ?? 0;

                    }
                    var InventoryStoreItemOBJ = new InventoryStoreItem();
                    InventoryStoreItemOBJ.InventoryStoreId = StoreID;
                    InventoryStoreItemOBJ.InventoryItemId = InventoryItemId;
                    InventoryStoreItemOBJ.OrderNumber = SalesOfferId.ToString();
                    InventoryStoreItemOBJ.OrderId = SalesOfferId;
                    InventoryStoreItemOBJ.CreatedBy = ValidateUserId;
                    InventoryStoreItemOBJ.ModifiedBy = ValidateUserId;
                    InventoryStoreItemOBJ.CreationDate = DateTime.Now;
                    InventoryStoreItemOBJ.ModifiedDate = DateTime.Now;
                    InventoryStoreItemOBJ.OperationType = OperationType;
                    InventoryStoreItemOBJ.Balance = (double)(-ReleaseQTY);
                    InventoryStoreItemOBJ.Balance1 = (-ReleaseQTY);
                    InventoryStoreItemOBJ.ExpDate = ParentInventoryStoreItem.ExpDate;
                    InventoryStoreItemOBJ.ItemSerial = ParentInventoryStoreItem.ItemSerial;
                    InventoryStoreItemOBJ.ReleaseParentId = ObjParentRelease.ID;
                    InventoryStoreItemOBJ.FinalBalance = (-ReleaseQTY);
                    InventoryStoreItemOBJ.AddingFromPoid = POID;
                    InventoryStoreItemOBJ.AddingOrderItemId = SalesOfferProductId;
                    InventoryStoreItemOBJ.PoinvoiceId = POInvoiceId;
                    InventoryStoreItemOBJ.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                    InventoryStoreItemOBJ.PoinvoiceTotalCost = POInvoiceTotalCost;
                    InventoryStoreItemOBJ.CurrencyId = currencyId;
                    InventoryStoreItemOBJ.RateToEgp = rateToEGP;
                    InventoryStoreItemOBJ.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                    InventoryStoreItemOBJ.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                    InventoryStoreItemOBJ.RemainItemPrice = remainItemPrice;
                    InventoryStoreItemOBJ.RemainItemCosetEgp = remainItemCosetEGP;
                    InventoryStoreItemOBJ.RemainItemCostOtherCu = remainItemCostOtherCU;
                    InventoryStoreItemOBJ.HoldQty = 0;
                    _Context.InventoryStoreItems.Add(InventoryStoreItemOBJ);
                    var InventoryStorItemInsertion = _Context.SaveChanges();

                    if (InventoryStorItemInsertion > 0)
                    {
                        ListIDSUpdate.Add(InventoryStoreItemOBJ.Id);
                        // Update Parent Release on InventoryStoreItem
                        if (ParentInventoryStoreItem != null)
                        {
                            ParentInventoryStoreItem.FinalBalance = ParentInventoryStoreItem.FinalBalance - (ReleaseQTY);
                            ParentInventoryStoreItem.ModifiedDate = DateTime.Now;
                            ParentInventoryStoreItem.ModifiedBy = ValidateUserId;


                            // Update PO Item Columns
                            ParentInventoryStoreItem.PoinvoiceId = POInvoiceId;
                            ParentInventoryStoreItem.CurrencyId = currencyId;
                            ParentInventoryStoreItem.RateToEgp = rateToEGP;
                            ParentInventoryStoreItem.PoinvoiceTotalPriceEgp = POInvoiceTotalPriceEGP;
                            ParentInventoryStoreItem.PoinvoiceTotalCostEgp = POInvoiceTotalCostEGP;
                            ParentInventoryStoreItem.PoinvoiceTotalPrice = POInvoiceTotalPrice;
                            ParentInventoryStoreItem.PoinvoiceTotalCost = POInvoiceTotalCost;
                            ParentInventoryStoreItem.RemainItemCosetEgp = remainItemCosetEGP;
                            ParentInventoryStoreItem.RemainItemCostOtherCu = remainItemCostOtherCU;
                            _Context.SaveChanges();
                        }
                    }
                }


            }



            //var ListInventoryStoreItem = _Context.InventoryStoreItems.Where(x => x.InventoryItemID == MatrialRequestItemObjDB.InventoryItemID && x.finalBalance > 0);

            //// -------------------------------------------------Update and Calc Average for Inventory Item ----------------------------------------------------------------
            //var ListInvStoreItemAll = InventoryStoreItemList.Where(x => x.InventoryItemID == InventoryItemObjDB.ID);
            //var ListInventoryStoreItem = ListInvStoreItemAll.Where(x => x.finalBalance > 0 && x.POInvoiceTotalCost != null);
            //var ListIDSPOInvoices = ListInventoryStoreItem.Select(x => x.POInvoiceId).ToList();
            //var ListIDSPOInvoicesIsFulllyPriced = _Context.PurchasePOInvoices.Where(x => ListIDSPOInvoices.Contains(x.ID)).Select(x => x.ID).ToList();
            //ListInventoryStoreItem = ListInventoryStoreItem.Where(x => x.POInvoiceId != null ? ListIDSPOInvoicesIsFulllyPriced.Contains((long)x.POInvoiceId) : false);
            //InventoryItemObjDB.AverageUnitPrice = (ListInventoryStoreItem.Sum(x => x.finalBalance) ?? 0) != 0 ? (ListInventoryStoreItem.Sum(x => x.POInvoiceTotalCostEGP * x.finalBalance) ?? 0) / (ListInventoryStoreItem.Sum(x => x.finalBalance) ?? 0) : 0;

            //// Update Avg Unit Price ..just stop for list of inventorystoreItem List
            ////foreach (var itemId in ListIDSUpdate)
            ////{
            ////    var InventoryStoreItemOBJ = ListInvStoreItemAll.Where(x => x.ID == itemId).FirstOrDefault();
            ////    InventoryStoreItemOBJ.AverageUnitPrice = InventoryItemObjDB.AverageUnitPrice;

            ////}
            //_Context.SaveChanges();
        }

        /*        public BaseResponseWithID AddNewSalesOfferWithReleaseForPOS(AddNewSalesOfferWithReleaseForPOSRequest Request, string companyname, long creator)
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

                            string CreatedByString = null;
                            long? CreatedBy = 0;
                            int InventoryStoreId = 0;
                            DateTime? StartDate = null;
                            DateTime StartDateTemp = DateTime.Now;

                            DateTime? EndDate = null;
                            DateTime EndDateTemp = DateTime.Now;

                            DateTime? ClientApprovalDate = null;
                            DateTime ClientApprovalDateTemp = DateTime.Now;

                            DateTime? OfferExpirationDate = null;
                            DateTime OfferExpirationDateTemp = DateTime.Now;

                            DateTime? ProjectStartDate = null;
                            DateTime ProjectStartDateTemp = DateTime.Now;

                            DateTime? ProjectEndDate = null;
                            DateTime ProjectEndDateTemp = DateTime.Now;

                            DateTime? RentStartDate = null;
                            DateTime RentStartDateTemp = DateTime.Now;

                            DateTime? RentEndDate = null;
                            DateTime RentEndDateTemp = DateTime.Now;

                            DateTime? ReminderDate = null;
                            DateTime ReminderDateTemp = DateTime.Now;

                            DateTime? SendingOfferDate = null;
                            DateTime SendingOfferDateTemp = DateTime.Now;

                            if (Request.SalesOffer != null)
                            {
                                if (Request.SalesOffer.StartDate == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Offer Start Date Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.StartDate, out StartDateTemp))
                                    {
                                        StartDateTemp = DateTime.Parse(Request.SalesOffer.StartDate);
                                        StartDate = StartDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid StartDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (Request.SalesOffer.SalesPersonId == 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Offer Sales Person Id Is Mandatory";
                                    Response.Errors.Add(error);
                                }

                                if (Request.SalesOffer.BranchId == 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Offer Branch Id Is Mandatory";
                                    Response.Errors.Add(error);
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.EndDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.EndDate, out EndDateTemp))
                                    {
                                        EndDateTemp = DateTime.Parse(Request.SalesOffer.EndDate);
                                        EndDate = EndDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid EndDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.ClientApprovalDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.ClientApprovalDate, out ClientApprovalDateTemp))
                                    {
                                        ClientApprovalDateTemp = DateTime.Parse(Request.SalesOffer.ClientApprovalDate);
                                        ClientApprovalDate = ClientApprovalDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid ClientApprovalDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.OfferExpirationDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.OfferExpirationDate, out OfferExpirationDateTemp))
                                    {
                                        OfferExpirationDateTemp = DateTime.Parse(Request.SalesOffer.OfferExpirationDate);
                                        OfferExpirationDate = OfferExpirationDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid OfferExpirationDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectStartDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.ProjectStartDate, out ProjectStartDateTemp))
                                    {
                                        ProjectStartDateTemp = DateTime.Parse(Request.SalesOffer.ProjectStartDate);
                                        ProjectStartDate = ProjectStartDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid ProjectStartDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.ProjectEndDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.ProjectEndDate, out ProjectEndDateTemp))
                                    {
                                        ProjectEndDateTemp = DateTime.Parse(Request.SalesOffer.ProjectEndDate);
                                        ProjectEndDate = ProjectEndDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid ProjectEndDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.RentStartDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.RentStartDate, out RentStartDateTemp))
                                    {
                                        RentStartDateTemp = DateTime.Parse(Request.SalesOffer.RentStartDate);
                                        RentStartDate = RentStartDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid RentStartDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.RentEndDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.RentEndDate, out RentEndDateTemp))
                                    {
                                        RentEndDateTemp = DateTime.Parse(Request.SalesOffer.RentEndDate);
                                        RentEndDate = RentEndDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid RentEndDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.ReminderDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.ReminderDate, out ReminderDateTemp))
                                    {
                                        ReminderDateTemp = DateTime.Parse(Request.SalesOffer.ReminderDate);
                                        ReminderDate = ReminderDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid ReminderDate";
                                        Response.Errors.Add(error);
                                    }
                                }

                                if (!string.IsNullOrEmpty(Request.SalesOffer.SendingOfferDate))
                                {
                                    if (DateTime.TryParse(Request.SalesOffer.SendingOfferDate, out SendingOfferDateTemp))
                                    {
                                        SendingOfferDateTemp = DateTime.Parse(Request.SalesOffer.SendingOfferDate);
                                        SendingOfferDate = SendingOfferDateTemp;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Invalid SendingOfferDate";
                                        Response.Errors.Add(error);
                                    }
                                }


                                if (Request.SalesOffer.CreatedBy != null)
                                {
                                    CreatedByString = Request.SalesOffer.CreatedBy;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Sales Offer Created By Id Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                                CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                if (user == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "ErrCRM1";
                                    error.ErrorMSG = "Sales Offer Creator User Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }


                                // Modified By michael markos 2022-10-25
                                if (Request.SalesOffer.ParentSalesOfferID != null)
                                {
                                    // check if this Offer is Found
                                    var SalesOfferObj = _unitOfWork.SalesOffers.FindAll(x => x.Id == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
                                    if (SalesOfferObj == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Return Sales Offer!";
                                        Response.Errors.Add(error);
                                    }

                                    if (Request.SalesOffer.OfferType != "Sales Return")
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Return Sales Offer Type Must Be Sales Return!";
                                        Response.Errors.Add(error);
                                    }


                                }

                                if (Request.SalesOffer.ParentInvoiceID != null)
                                {
                                    // check if this Offer is Found
                                    var InvoicesObj = _unitOfWork.Invoices.FindAll(x => x.Id == Request.SalesOffer.ParentInvoiceID).FirstOrDefault();
                                    if (InvoicesObj == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Return Invoice!";
                                        Response.Errors.Add(error);
                                    }
                                }



                                if (Request.SalesOffer.ParentSalesOfferID == null || Request.SalesOffer.ParentSalesOfferID == 0)
                                {

                                    if (Request.SalesOffer.InventoryStoreId == null || Request.SalesOffer.InventoryStoreId == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Inventory Store Id in Sales Offer!";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        InventoryStoreId = (int)Request.SalesOffer.InventoryStoreId;
                                    }
                                }

                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Invalid Sales Offer Data!!";
                                Response.Errors.Add(error);
                            }
                            List<InventoryStoreItem> InventoryStoreItemListDB = new List<InventoryStoreItem>();
                            if (Request.SalesOfferProductList != null)
                            {
                                if (Request.SalesOfferProductList.Count() > 0)
                                {
                                    var InventoryItemListIDS = Request.SalesOfferProductList.Select(x => x.InventoryItemId).ToList();

                                    InventoryStoreItemListDB = _Context.InventoryStoreItems.Where(x => InventoryItemListIDS.Contains(x.InventoryItemId)).ToList();
                                    int Counter = 0;
                                    foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                                    {
                                        Counter++;
                                        if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID > 0)
                                        {
                                            if (SalesOfferProduct.ParentOfferProductId == null || SalesOfferProduct.ParentOfferProductId == 0)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err-P12";
                                                error.ErrorMSG = "Sales Offer Product ParentId Is Mandatory For Returned Sales Offer!";
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                var ParentProductcDb = _unitOfWork.SalesOfferProducts.GetById((long)SalesOfferProduct.ParentOfferProductId);
                                                if (ParentProductcDb != null)
                                                {
                                                    if (SalesOfferProduct.Quantity != null && SalesOfferProduct.Quantity > 0)
                                                    {
                                                        if (SalesOfferProduct.Quantity > (ParentProductcDb.RemainQty ?? ParentProductcDb.Quantity ?? 0))
                                                        {
                                                            Response.Result = false;
                                                            Error error = new Error();
                                                            error.ErrorCode = "Err-P12";
                                                            error.ErrorMSG = "Returned Quantity For Sales Offer Product: " + SalesOfferProduct.ParentOfferProductId + " Cannot be Greater Than Remain Quantity Of Parent Product!";
                                                            Response.Errors.Add(error);
                                                        }
                                                    }

                                                    // Validate if Return => if have balance from parent or not


                                                }
                                                else
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err-P12";
                                                    error.ErrorMSG = "This Sales Offer Product " + SalesOfferProduct.ParentOfferProductId + " Doesn't Exist!";
                                                    Response.Errors.Add(error);
                                                }

                                            }
                                        }


                                        if (SalesOfferProduct.CreatedBy != null)
                                        {
                                            CreatedByString = SalesOfferProduct.CreatedBy;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Sales Offer Product Created By Id Is Mandatory";
                                            Response.Errors.Add(error);
                                        }


                                        CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                        var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                        if (user == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "Sales Offer Product Creator User Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                        long InventoryItemId = 0;
                                        if (SalesOfferProduct.InventoryItemId == null)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "Invalid InventoryItemId on Item No #" + Counter;
                                            Response.Errors.Add(error);
                                        }
                                        else
                                        {
                                            InventoryItemId = (long)SalesOfferProduct.InventoryItemId;
                                        }
                                        decimal Quantity = 0;
                                        if (SalesOfferProduct.Quantity == null || SalesOfferProduct.Quantity <= 0)
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "ErrCRM1";
                                            error.ErrorMSG = "Invalid Sales Offer Product Quantity on Item No #" + Counter;
                                            Response.Errors.Add(error);
                                        }
                                        else
                                        {
                                            Quantity = (decimal)SalesOfferProduct.Quantity;
                                        }
                                        if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID != 0)
                                        {
                                            // Check if salesoffer is exist in inventory store item for return
                                            var InvetorStorItemForParentSalesOffer = InventoryStoreItemListDB.Where(x => x.OrderId == Request.SalesOffer.ParentSalesOfferID && x.AddingOrderItemId == SalesOfferProduct.ParentOfferProductId && x.FinalBalance < 0).Select(x => x.FinalBalance).Sum();
                                            if (InvetorStorItemForParentSalesOffer == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "ErrCRM1";
                                                error.ErrorMSG = "Not have balance to Return Release on Item No #" + Counter;
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            if (InventoryStoreId != 0 && InventoryItemId != 0 && Quantity != 0)
                                            {
                                                InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0).ToList();
                                                List<InventoryStoreItemIDWithQTY> AvailableItemStockList = GetParentReleaseIDWithSettingStore(InventoryStoreItemListDB,
                                                                            InventoryItemId,
                                                                            InventoryStoreId,
                                                                            null,// store location
                                                                            Quantity,
                                                                            true);
                                                if (AvailableItemStockList == null || AvailableItemStockList.Count() == 0 || AvailableItemStockList.Sum(x => x.StockBalance) < Quantity)
                                                //{
                                                //    Response.Result = false;
                                                //    Error error = new Error();
                                                //    error.ErrorCode = "Err325";
                                                //    error.ErrorMSG = "Not have availble qty from parent  Item to release on Item NO #" + itemCount;
                                                //    Response.Errors.Add(error);
                                                //    //return Response;
                                                //}
                                                //if (AvailableItemStockList.Count() == 0)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "ErrCRM1";
                                                    error.ErrorMSG = "Not have balance to release on Item No #" + Counter;
                                                    Response.Errors.Add(error);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (Request.SalesOffer != null)
                                {
                                    if (Request.SalesOffer.Id == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Invalid Sales Offer Product";
                                        Response.Errors.Add(error);

                                    }
                                }
                            }
                            //long? InventoryStoreID = null;
                            //if (InventoryStoreID == null)
                            //{
                            // Check if inventory Dirct is not found to add one
                            //    string StoreName = "DIRECT PR HIDDEN STORE";
                            //long? InventoryStoreID = Common.CheckInventoryStoreID(StoreName);
                            //    if (InventoryStoreID == 0)
                            //    {
                            //        // Inserty Inventory Store ID
                            //        ObjectParameter IDInventoryStore = new ObjectParameter("ID", typeof(int));
                            //        _Context.proc_InventoryStoreInsert(IDInventoryStore,
                            //                                           StoreName,
                            //                                           true,
                            //                                           null, null,
                            //                                           DateTime.Now,
                            //                                           validation.userID,
                            //                                           DateTime.Now,
                            //                                           validation.userID);

                            //        InventoryStoreID = (int)IDInventoryStore.Value;
                            //    }
                            // }


                            if (Response.Result)
                            {
                                long SalesOfferId = 0;
                                // Add-Edit Sales Offer
                                if (Request.SalesOffer.Id == null || Request.SalesOffer.Id == 0)
                                {

                                    var NewOfferSerial = "";
                                    var OfferSerialSubString = "";

                                    //long newOfferNumber = 0;
                                    long CountOfSalesOfferThisYear = _Context.SalesOffers.Where(x => x.Active == true && x.OfferSerial.Contains(System.DateTime.Now.Year.ToString())).Count();
                                    if (companyname.ToLower() == "marinaplt")
                                    {
                                        if (Request.SalesOffer.OfferType == "New Project Offer")
                                        {
                                            NewOfferSerial = "S";
                                            OfferSerialSubString = "-RFQ-#";
                                        }
                                        else if (Request.SalesOffer.OfferType == "New Maintenance Offer")
                                        {
                                            NewOfferSerial = "M";
                                            OfferSerialSubString = "-RFM-#";
                                        }
                                        else if (Request.SalesOffer.OfferType == "New Rent Offer")
                                        {
                                            NewOfferSerial = "R";
                                            OfferSerialSubString = "-RentOffer-#";
                                        }
                                        else if (Request.SalesOffer.OfferType == "New Internal Order")
                                        {
                                            NewOfferSerial = "I";
                                            OfferSerialSubString = "-RFQ-#";
                                        }

                                        var branchName = Common.GetBranchName(Request.SalesOffer.BranchId, _Context);

                                        if (branchName == "Alexandria")
                                        {
                                            NewOfferSerial += "-A";
                                        }
                                        else if (branchName == "Cairo")
                                        {
                                            NewOfferSerial += "-C";
                                        }
                                        else if (branchName == "Factory")
                                        {
                                            NewOfferSerial += "-F";
                                        }
                                        else if (branchName == "Show Room")
                                        {
                                            NewOfferSerial += "-S.R";
                                        }
                                        else if (branchName == "Main Office")
                                        {
                                            NewOfferSerial += "-M.O";
                                        }

                                        //if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                                        //{
                                        //    string strLastOfferNumber = lastSalesOfferSerial.Split('-')[4];
                                        //    newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                                        //    NewOfferSerial += OfferSerialSubString + newOfferNumber + "-" + System.DateTime.Now.Year.ToString();
                                        //}
                                        //else
                                        NewOfferSerial += OfferSerialSubString + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Year.ToString();
                                    }
                                    else if (companyname.ToLower() == "proauto")
                                    {
                                        var lastSalesOfferSerial = _Context.SalesOffers.Where(a => a.Active == true).ToList().OrderByDescending(a => a.Id).Select(a => a.OfferSerial).FirstOrDefault();
                                        if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                                        {
                                            var ListSplit = lastSalesOfferSerial.Split('-');
                                            string strLastOfferNumber = ListSplit[0];
                                            var newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                                            NewOfferSerial += "#" + newOfferNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                                        }

                                    }
                                    else
                                    {
                                        //if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                                        //{
                                        //    string strLastOfferNumber = lastSalesOfferSerial.Split('-')[0];
                                        //    newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                                        //    NewOfferSerial += "#" + newOfferNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                                        //}
                                        //else
                                        NewOfferSerial += "#" + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                                    }

                                    // Insert
                                    var NewSalesOfferInsert = new SalesOffer()
                                    {
                                        StartDate = DateOnly.FromDateTime((DateTime)StartDate),
                                        EndDate = DateOnly.FromDateTime((DateTime)EndDate),
                                        Note = string.IsNullOrEmpty(Request.SalesOffer.Note) ? null : Request.SalesOffer.Note,
                                        SalesPersonId = Request.SalesOffer.SalesPersonId,
                                        CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key)),
                                        CreationDate = DateTime.Now,
                                        ModifiedBy = null,
                                        Modified = null,
                                        Active = true,
                                        Status = Request.SalesOffer.Status,
                                        Completed = Request.SalesOffer.Completed,
                                        TechnicalInfo = string.IsNullOrEmpty(Request.SalesOffer.TechnicalInfo) ? null : Request.SalesOffer.TechnicalInfo,
                                        ProjectData = string.IsNullOrEmpty(Request.SalesOffer.ProjectData) ? null : Request.SalesOffer.ProjectData,
                                        FinancialInfo = string.IsNullOrEmpty(Request.SalesOffer.FinancialInfo) ? null : Request.SalesOffer.FinancialInfo,
                                        PricingComment = string.IsNullOrEmpty(Request.SalesOffer.PricingComment) ? null : Request.SalesOffer.PricingComment,
                                        OfferAmount = Request.SalesOffer.OfferAmount == null ? null : Request.SalesOffer.OfferAmount,
                                        SendingOfferConfirmation = Request.SalesOffer.SendingOfferConfirmation,
                                        SendingOfferDate = SendingOfferDate,
                                        SendingOfferBy = Request.SalesOffer.SendingOfferBy,
                                        SendingOfferTo = Request.SalesOffer.SendingOfferTo,
                                        SendingOfferComment = Request.SalesOffer.SendingOfferComment,
                                        ClientApprove = Request.SalesOffer.ClientApprove,
                                        ClientComment = Request.SalesOffer.ClientComment,
                                        VersionNumber = Request.SalesOffer.VersionNumber,
                                        ClientApprovalDate = ClientApprovalDate,
                                        ClientId = Request.SalesOffer.ClientId,
                                        ProductType = Request.SalesOffer.ProductType,
                                        ProjectName = string.IsNullOrEmpty(Request.SalesOffer.ProjectName) ? NewOfferSerial : Request.SalesOffer.ProjectName,
                                        ProjectLocation = Request.SalesOffer.ProjectLocation,
                                        ContactPersonMobile = Request.SalesOffer.ContactPersonMobile,
                                        ContactPersonEmail = Request.SalesOffer.ContactPersonEmail,
                                        ContactPersonName = Request.SalesOffer.ContactPersonName,
                                        ProjectStartDate = ProjectStartDate,
                                        ProjectEndDate = ProjectEndDate,
                                        BranchId = Request.SalesOffer.BranchId,
                                        OfferType = Request.SalesOffer.OfferType,
                                        ContractType = Request.SalesOffer.ContractType,
                                        OfferSerial = NewOfferSerial,
                                        ClientNeedsDiscount = Request.SalesOffer.ClientNeedsDiscount,
                                        RejectionReason = Request.SalesOffer.RejectionReason,
                                        NeedsInvoice = Request.SalesOffer.NeedsInvoice,
                                        NeedsExtraCost = Request.SalesOffer.NeedsExtraCost,
                                        OfferExpirationDate = OfferExpirationDate,
                                        OfferExpirationPeriod = Request.SalesOffer.OfferExpirationPeriod,
                                        ExtraOrDiscountPriceBySalesPerson = Request.SalesOffer.ExtraOrDiscountPriceBySalesPerson,
                                        FinalOfferPrice = Request.SalesOffer.FinalOfferPrice,
                                        ReminderDate = ReminderDate
                                    };
                                    var SalesOfferInsert = _unitOfWork.SalesOffers.Add(NewSalesOfferInsert);
                                    _unitOfWork.Complete();

                                    if (SalesOfferInsert != null)
                                    {
                                        SalesOfferId = (long)NewSalesOfferInsert.Id;
                                        Response.Result = true;
                                        Response.ID = SalesOfferId;
                                        if (Request.SalesOffer.ParentSalesOfferID != null)
                                        {
                                            long? ParentInvoiceID = Request.SalesOffer.ParentInvoiceID;
                                            if (ParentInvoiceID == null)
                                            {
                                                var InvoiceDB = _Context.Invoices.Where(x => x.SalesOfferId == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
                                                if (InvoiceDB != null)
                                                {
                                                    ParentInvoiceID = InvoiceDB.Id;
                                                }
                                            }
                                            if (ParentInvoiceID != null)
                                            {

                                                // Modified By Michael Markos 2022-10-25
                                                // Add in table Invoice CN And DN
                                                var InvoiceCNAndDNObj = new InvoiceCnandDn();
                                                InvoiceCNAndDNObj.ParentSalesOfferId = (long)Request.SalesOffer.ParentSalesOfferID;
                                                InvoiceCNAndDNObj.ParentInvoiceId = (long)ParentInvoiceID;
                                                InvoiceCNAndDNObj.SalesOfferId = SalesOfferId;
                                                InvoiceCNAndDNObj.Active = true;
                                                InvoiceCNAndDNObj.CreatedBy = CreatedBy ?? 1;
                                                InvoiceCNAndDNObj.CreationDate = DateTime.Now;
                                                InvoiceCNAndDNObj.ModifiedBy = CreatedBy ?? 1;
                                                InvoiceCNAndDNObj.ModificationDate = DateTime.Now;

                                                _Context.InvoiceCnandDns.Add(InvoiceCNAndDNObj);
                                                _Context.SaveChanges();
                                            }
                                        }



                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Insert this Offer!!";
                                        Response.Errors.Add(error);
                                    }
                                }

                                //if (Request.SalesOffer.Status.ToLower() == "closed")
                                //{
                                //    //if (SalesOfferDbStatus.ToLower() != "closed")
                                //    //{
                                //    //}
                                //    if (ProjectSalesOfferIsExist == null)
                                //    {
                                //        CloseSalesOffer(SalesOfferId, headers["CompanyName"].ToString(), validation.userID);
                                //    }
                                //}

                                //var SalesOfferInvoices = _Context.Invoices.Where(a => a.SalesOfferId == SalesOfferId).Count();
                                // Add-Edit Sales Offer Product
                                if (Request.SalesOfferProductList != null)
                                {
                                    if (Request.SalesOfferProductList.Count() > 0)
                                    {

                                        foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                                        {
                                            decimal TempProfitPercentage;
                                            decimal? ProfitPercentage = decimal.TryParse(SalesOfferProduct.ProfitPercentage, out TempProfitPercentage) ? TempProfitPercentage : (decimal?)null;

                                            string ItemComment = null;
                                            if (string.IsNullOrEmpty(SalesOfferProduct.ItemPricingComment))
                                            {
                                                var inventoryItemDb = _unitOfWork.InventoryItems.GetById((long)SalesOfferProduct.InventoryItemId);
                                                if (inventoryItemDb != null)
                                                {
                                                    if (string.IsNullOrEmpty(inventoryItemDb.Description))
                                                    {
                                                        ItemComment = inventoryItemDb.Name;
                                                    }
                                                    else
                                                    {
                                                        ItemComment = inventoryItemDb.Description;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ItemComment = SalesOfferProduct.ItemPricingComment;
                                            }
                                            // Add-Edit Sales Offer
                                            if (SalesOfferProduct.Id == null || SalesOfferProduct.Id == 0)
                                            {

                                                // Insert
                                                //ObjectParameter SalesOfferProductInsertedId = new ObjectParameter("ID", typeof(long));
                                                var NewSalesOfferProductInsert = new SalesOfferProduct()
                                                {
                                                    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key)),
                                                    CreationDate = DateTime.Now,
                                                    ModifiedBy = null,
                                                    Modified = null,
                                                    Active = true,
                                                    OfferId = SalesOfferId,
                                                    ProductId = SalesOfferProduct.ProductId,
                                                    ProductGroupId = SalesOfferProduct.ProductGroupId,
                                                    Quantity = SalesOfferProduct.Quantity,
                                                    InventoryItemId = SalesOfferProduct.InventoryItemId,
                                                    InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId,
                                                    ItemPrice = SalesOfferProduct.ItemPrice,
                                                    ItemPricingComment = ItemComment,
                                                    ConfirmReceivingQuantity = null,
                                                    ConfirmReceivingComment = null,
                                                    InvoicePayerClientId = SalesOfferProduct.InvoicePayerClientId,
                                                    DiscountPercentage = SalesOfferProduct.DiscountPercentage,
                                                    DiscountValue = SalesOfferProduct.DiscountValue,
                                                    FinalPrice = SalesOfferProduct.FinalPrice,
                                                    TaxPercentage = SalesOfferProduct.TaxPercentage,
                                                    TaxValue = SalesOfferProduct.TaxValue,
                                                    ReturnedQty = 0,
                                                    RemainQty = SalesOfferProduct.Quantity,
                                                    ProfitPercentage = ProfitPercentage,
                                                    ReleasedQty = null
                                                };
                                                var SalesOfferProductInsert = _unitOfWork.SalesOfferProducts.Add(NewSalesOfferProductInsert);
                                                _unitOfWork.Complete();
                                                if (SalesOfferProductInsert != null)
                                                {
                                                    SalesOfferProduct.Id = (long)SalesOfferProductInsert.Id;

                                                    if (SalesOfferProduct.ParentOfferProductId != null && SalesOfferProduct.ParentOfferProductId != 0) // Client Return
                                                    {
                                                        var ParentSalesOfferProductDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.Id == (long)SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
                                                        if (ParentSalesOfferProductDb != null)
                                                        {
                                                            ParentSalesOfferProductDb.RemainQty -= SalesOfferProduct.Quantity;
                                                            ParentSalesOfferProductDb.ReturnedQty += SalesOfferProduct.Quantity;
                                                            _Context.SaveChanges();
                                                        }

                                                        // Client Return
                                                        var ParentInventoryStoreItemDB = InventoryStoreItemListDB.Where(x => x.OrderId == Request.SalesOffer.ParentSalesOfferID && x.AddingOrderItemId == SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
                                                        if (ParentInventoryStoreItemDB != null)
                                                        {
                                                            AddInventoryStoreItemWithReturn(ParentInventoryStoreItemDB, (long)SalesOfferProduct.InventoryItemId, (decimal)SalesOfferProduct.Quantity, SalesOfferId, SalesOfferProduct.Id, creator);
                                                        }

                                                    }
                                                    else // Release 
                                                    {
                                                        // Release Item inventoryStoreItem
                                                        if (InventoryStoreId != 0)
                                                        {
                                                            AddInvntoryStoreItemWithRelease(InventoryStoreItemListDB, (long)SalesOfferProduct.InventoryItemId, (int)InventoryStoreId, (decimal)SalesOfferProduct.Quantity, true, SalesOfferId, SalesOfferProduct.Id, creator);
                                                        }
                                                    }
                                                    Response.Result = true;
                                                }
                                                else
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "Faild To Insert this Offer!!";
                                                    Response.Errors.Add(error);
                                                }
                                            }
                                        }

                                        //Deleted Invoices + Deleted InvoiceItems + Inserted InvoiceItems + Update Project Price
                                        if (Request.SalesOffer.Status != null && Request.SalesOffer.Status.ToLower() == "closed")
                                        {


                                            //When change PayerId Check Deleted Invoices And Inserted Invoices Then Delete Old and Insert New
                                            var clientsIds = Request.SalesOfferProductList.Where(a => a.Active == true).Select(a => a.InvoicePayerClientId).Distinct().ToList();

                                            var InvoicesToInsertIds = clientsIds.ToList();


                                            //Insert 

                                            if (InvoicesToInsertIds != null && InvoicesToInsertIds.Count > 0)
                                            {
                                                foreach (var clientId in InvoicesToInsertIds)
                                                {
                                                    var SalesOfferDb = _Context.SalesOffers.Where(a => a.Id == SalesOfferId).FirstOrDefault();
                                                    var OfferClientApprovalDate = SalesOfferDb.ClientApprovalDate;
                                                    DateTime InvoiceDate = OfferClientApprovalDate ?? DateTime.Now;
                                                    long InvoiceId = 0;
                                                    //ObjectParameter InvoiceInsertedId = new ObjectParameter("ID", typeof(long));
                                                    var NewInvoiceInsert = new Invoice()
                                                    {
                                                        Serial = "1",
                                                        Revision = 0,
                                                        InvoiceDate = InvoiceDate,
                                                        InvoiceType = "1",
                                                        ClientId = clientId,
                                                        CreatedBy = creator,
                                                        ModifiedBy = creator,
                                                        CreationDate = DateTime.Now,
                                                        ModificationDate = DateTime.Now,
                                                        Active = true,
                                                        IsClosed = false,
                                                        CreationType = null,
                                                        InvoiceFor = null,
                                                        EInvoiceId = null,
                                                        EInvoiceStatus = null,
                                                        EInvoiceAcceptDate = null,
                                                        SalesOfferId = SalesOfferId,
                                                        EInvoiceJsonBody = null,
                                                        EInvoiceRequestToSend = null,
                                                    };
                                                    var InvoiceInsert = _unitOfWork.Invoices.Add(NewInvoiceInsert);
                                                    _unitOfWork.Complete();

                                                    if (InvoiceInsert != null)
                                                    {
                                                        InvoiceId = (long)InvoiceInsert.Id;
                                                        int SerialTemp = 0;
                                                        var SerialList = _Context.Invoices.Select(x => x.Serial).ToList();
                                                        int Serial = SerialList.Where(x => int.TryParse(x, out SerialTemp)).OrderByDescending(x => int.Parse(x)).Select(x => int.Parse(x)).FirstOrDefault();
                                                        //int SerialNo = string.IsNullOrEmpty(Serial) && int.TryParse(Serial, out SerialNo) ? 1 : int.Parse(Serial) + 1;
                                                        var InvoiceDB = _Context.Invoices.Where(x => x.Id == InvoiceId).FirstOrDefault();
                                                        if (InvoiceDB != null)
                                                        {
                                                            InvoiceDB.Serial = (Serial + 1).ToString();
                                                            _Context.SaveChanges();
                                                        }
                                                        var ClientInvoicesItemList = _Context.SalesOfferProducts.Where(x => x.InvoicePayerClientId == clientId).ToList();
                                                        // Insert Into Invoice Items
                                                        if (ClientInvoicesItemList.Count > 0)
                                                        {
                                                            foreach (var invoiceItem in ClientInvoicesItemList)
                                                            {
                                                                //ObjectParameter InvoiceItemInsertedId = new ObjectParameter("ID", typeof(long));          
                                                                _unitOfWork.InvoiceItems.Add(new InvoiceItem()
                                                                {
                                                                    InvoiceId = InvoiceId,
                                                                    SalesOfferProductId = invoiceItem.Id,
                                                                    Comments = null,
                                                                    EInvoiceId = null,
                                                                    EInvoiceStatus = null,
                                                                    EInvoiceAcceptDate = null
                                                                });
                                                                _unitOfWork.Complete();

                                                            }
                                                        }

                                                    }
                                                }
                                            }
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
        */

        public ExcelWorksheet MergeCells(ExcelWorksheet worksheet)
        {
            string searchValue = worksheet.Cells[2, 12].Value == null || worksheet.Cells[2, 12].Value.ToString() == "" ? null : worksheet.Cells[2, 12].Value.ToString();
            var first = 2;
            var end = 2;
            worksheet.Row(first).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            worksheet.Row(first).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            for (int currentRow = 3; currentRow <= worksheet.Dimension.End.Row; currentRow++)
            {
                worksheet.Row(currentRow).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Row(currentRow).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                if (worksheet.Cells[currentRow, 12].Value != null && worksheet.Cells[currentRow, 12].Value.ToString() != searchValue)
                {
                    searchValue = worksheet.Cells[currentRow, 11].Value == null || worksheet.Cells[currentRow, 11].Value.ToString() == "" ? null : worksheet.Cells[currentRow, 11].Value.ToString();
                    worksheet.Cells[first, 1, end, 1].Merge = true;
                    worksheet.Cells[first, 2, end, 2].Merge = true;
                    worksheet.Cells[first, 3, end, 3].Merge = true;
                    worksheet.Cells[first, 4, end, 4].Merge = true;

                    first = currentRow;
                    end = currentRow;
                }
                else
                {
                    end++;
                }
            }
            return worksheet;
        }

        public BaseResponseWithData<string> InventoryStoreItemReportWithTabs(string companyname)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var GetInventoryList = new List<InventoryStoreVM>();
                if (Response.Result)
                {
                    var InventoryListFromView = _unitOfWork.VInventoryStoreItems.FindAll(x => x.FinalBalance > 0).OrderBy(x => x.InventoryItemName).ToList();
                    using (var package = new ExcelPackage())
                    {

                        var InventoryStoreItemGrouping = InventoryListFromView.GroupBy(x => x.InventoryStoreName).ToList();
                        var locations = _unitOfWork.InventoryStoreLocations.FindAll(a => InventoryListFromView.Select(b => b.InvenoryStoreLocationId).ToList().Contains(a.Id)).ToList();
                        foreach (var StoreItems in InventoryStoreItemGrouping)
                        {
                            string StoreName = StoreItems.Key;
                            var dt = new System.Data.DataTable("Grid");
                            dt.Columns.AddRange(new DataColumn[12] { new DataColumn("Item Name"),
                                                    new DataColumn("Item Category"),
                                                    new DataColumn("Item Code"),
                                                    new DataColumn("Remain Balance"),
                                                     new DataColumn("Store Name"),
                                                     new DataColumn("Location"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("R. Balance"),
                                                     //new DataColumn("PO ID"),
                                                     //new DataColumn("Inv. ID"),
                                                     new DataColumn("Operation Type"),
                                                     new DataColumn("The Difference"),
                                                     new DataColumn("ID"),


                                  });
                            var list = StoreItems.OrderBy(a => a.InventoryItemId).GroupBy(a => new { a.InventoryItemId, a.ExpDate, a.ItemSerial }).ToList();
                            foreach (var item in list)
                            {
                                dt.Rows.Add(
                                item.FirstOrDefault().InventoryItemName,
                                item.FirstOrDefault().CategoryName,
                                item.FirstOrDefault().Code,
                                StoreItems.Where(a => a.InventoryItemId == item.FirstOrDefault().InventoryItemId).Sum(a => a.FinalBalance),
                                item.FirstOrDefault().InventoryStoreName,
                                item.FirstOrDefault().InvenoryStoreLocationId != null ? locations.Where(a => a.Id == item.FirstOrDefault().InvenoryStoreLocationId).FirstOrDefault()?.Location : "N/A",
                                item.FirstOrDefault().ExpDate.ToString().Split(' ')[0],
                                item.FirstOrDefault().ItemSerial,
                                item.FirstOrDefault().FinalBalance != null ? Decimal.Round((decimal)item.FirstOrDefault().FinalBalance, 1) : 0,
                                item.FirstOrDefault().OperationType,
                                "",
                                item.FirstOrDefault().InventoryItemId.ToString()

                                    );
                            }


                            var workSheet = package.Workbook.Worksheets.Add(StoreName);
                            ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                            for (int i = 1; i < 13; i++)
                            {
                                workSheet.Column(i).AutoFit();
                            }
                            workSheet = MergeCells(workSheet);
                            workSheet.Column(12).Hidden = true;













                        }

                        var newpath = $"Attachments\\{companyname}\\InventoryStoreItems";
                        var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                        if (File.Exists(savedPath))
                            File.Delete(savedPath);

                        // Create excel file on physical disk  
                        Directory.CreateDirectory(savedPath);
                        //FileStream objFileStrm = File.Create(savedPath);
                        //objFileStrm.Close();
                        var excelPath = savedPath + $"\\InventoryStoreItem.xlsx";
                        package.SaveAs(excelPath);
                        // Write content to excel file  
                        //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                        //Close Excel package 
                        package.Dispose();
                        Response.Data = Globals.baseURL + '\\' + newpath + $"\\InventoryStoreItem.xlsx";

                        return Response;

                    }



                }




                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<string> GetPurchaseForStoreReport(int? inventoryStoreID, string DateFrom, string DateTo, bool? internalTransferFlag, string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var startDate = DateTime.Now;
            if (!DateTime.TryParse(DateFrom, out startDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid DateFrom";
                response.Errors.Add(err);
                return response;
            }

            var endDate = DateTime.Now;
            if (!DateTime.TryParse(DateTo, out endDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.ErrorMSG = "please, Enter a valid DateTo";
                response.Errors.Add(err);
                return response;
            }
            #endregion

            try
            {
                var storeName = string.Empty;
                if (inventoryStoreID != null)
                {
                    var storeData = _unitOfWork.InventoryStores.GetById(inventoryStoreID ?? 0);
                    if (storeData == null)
                    {
                        response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.ErrorMSG = "No Store with this ID";
                        response.Errors.Add(err);
                        return response;
                    }
                    storeName = storeData.Name;
                }


                var storeItemList = _unitOfWork.InventoryStoreItems.FindAllQueryable(a => a.CreationDate >= startDate && a.CreationDate <= endDate
                                        && (a.OperationType == "Opening Balance" || a.OperationType == "Add New Matrial"), new[] { "InventoryStore", "InventoryItem" });        //make it quarable and check for internalTransfer

                if (inventoryStoreID != null)
                {
                    storeItemList.Where(a => a.InventoryStoreId == inventoryStoreID);
                }

                if (internalTransferFlag != null)
                {
                    storeItemList.Where(a => a.OperationType.Contains("Transfer Order") && a.Balance1 > 0);
                }


                var orderIDs = storeItemList.Select(a => a.OrderId).ToList();

                var addingOrders = _unitOfWork.InventoryAddingOrders.FindAll(a => orderIDs.Contains(a.Id), new[] { "Supplier" });

                //var test = storeItemList.Where(a => a.OperationType.Contains("Transfer Order") && a.Balance1 > 0).ToList();
                var suppliersIDs = addingOrders.Select(a => a.SupplierId).ToList();

                var suppliers = _unitOfWork.Suppliers.FindAll(a => suppliersIDs.Contains(a.Id));

                var ExcelData = new List<PurchaseForStoreHelper>();
                foreach (var storeItem in storeItemList)
                {
                    var purches = new PurchaseForStoreHelper();
                    purches.InventoryItemID = storeItem.InventoryItemId;
                    purches.InventoryItemName = storeItem.InventoryItem.Name;
                    purches.ItemPrice = storeItem.PoinvoiceTotalPriceEgp;
                    purches.ItemQuantity = storeItem.Balance1;
                    purches.AddedDate = storeItem.CreationDate.ToShortDateString();
                    purches.InventoryStoreID = storeItem.InventoryStoreId;
                    purches.InventoryStoreName = storeItem.InventoryStore.Name;
                    purches.SupplierID = addingOrders.Where(a => a.Id == storeItem.OrderId).FirstOrDefault().SupplierId;
                    purches.SupplierName = addingOrders.Where(a => a.Id == storeItem.OrderId).FirstOrDefault().Supplier.Name;

                    ExcelData.Add(purches);

                }
                #region sheet1
                //--------------------------------------fill excel with data sheet 1-----------------------------------
                ExcelPackage excel = new ExcelPackage();

                var sheet = excel.Workbook.Worksheets.Add($"Purchases");

                sheet.Cells[1, 1].Value = $"Purchase of {storeName} from  {startDate.ToShortDateString()} to  {endDate.ToShortDateString()}";
                sheet.Cells[1, 1, 1, 6].Merge = true;
                sheet.Cells[3, 1].Value = "Item Name";
                sheet.Cells[3, 2].Value = "Price";
                sheet.Cells[3, 3].Value = "Quantity";
                sheet.Cells[3, 4].Value = "Creation Date";
                sheet.Cells[3, 5].Value = "Supplier Name";
                sheet.Cells[3, 6].Value = "Store Name";

                //--------------------------------------styling-------------------------------------------
                for (int col = 1; col <= 10; col++) sheet.Column(col).Width = 25;
                sheet.DefaultRowHeight = 12;
                sheet.Row(3).Height = 20;
                sheet.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(3).Style.Font.Bold = true;
                sheet.Cells[3, 1, 3, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[3, 1, 3, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[3, 1, 3, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[3, 1, 3, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                sheet.Row(1).Style.Font.Bold = true;
                //sheet.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                //sheet.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[1, 1, 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[1, 1, 1, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //----------------------------------------------------------------------------------------

                //------------------------------fill with data-------------------------------------------
                int rowIndex = 4;
                foreach (var item in ExcelData)
                {
                    sheet.Cells[rowIndex, 1].Value = item.InventoryItemName;
                    sheet.Cells[rowIndex, 2].Value = item.ItemPrice;
                    sheet.Cells[rowIndex, 3].Value = item.ItemQuantity;
                    sheet.Cells[rowIndex, 4].Value = item.AddedDate;
                    sheet.Cells[rowIndex, 5].Value = item.SupplierName;
                    sheet.Cells[rowIndex, 6].Value = item.InventoryStoreName;

                    sheet.Cells[rowIndex, 1, rowIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowIndex, 1, rowIndex, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    rowIndex++;
                }
                //-------------------------------End fill with data------------------------------------


                //--------------------------summation---------------------------
                sheet.Cells[2, 2].Formula = "SUM(B4:B3000)";
                sheet.Cells[2, 3].Formula = "SUM(C4:C3000)";

                sheet.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);

                sheet.Cells[2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[2, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet.Cells[2, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[2, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);
                //------------------------End Summiation----------------------------
                #endregion

                #region sheet2
                //-----------------------sheet 2--------------------------------
                var sheet2 = excel.Workbook.Worksheets.Add($"Purchase Group with Supplier");

                sheet2.Cells[1, 1].Value = $"Purchase Group with Supplier of {storeName} from  {startDate.ToShortDateString()} to  {endDate.ToShortDateString()}";
                sheet2.Cells[1, 1, 1, 6].Merge = true;
                sheet2.Cells[3, 1].Value = "Supplier Name";
                sheet2.Cells[3, 2].Value = "Item Name";
                sheet2.Cells[3, 3].Value = "Price";
                sheet2.Cells[3, 4].Value = "Quantity";
                sheet2.Cells[3, 5].Value = "Creation Date";
                sheet2.Cells[3, 6].Value = "Store Name";

                //--------------------------------------styling-------------------------------------------
                for (int col = 1; col <= 10; col++) sheet2.Column(col).Width = 25;
                sheet2.DefaultRowHeight = 12;
                sheet2.Row(1).Height = 20;
                sheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Row(1).Style.Font.Bold = true;
                sheet2.Cells[3, 1, 3, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet2.Cells[3, 1, 3, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet2.Cells[3, 1, 3, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Cells[3, 1, 3, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet2.Row(3).Height = 20;
                sheet2.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Row(3).Style.Font.Bold = true;
                //----------------------------------------------------------------------------------------

                var groups = ExcelData.GroupBy(a => a.SupplierID).ToList();

                var totalQuantitySum = groups.Sum(a => a.Sum(b => b.ItemQuantity));
                var totalPriceSum = groups.Sum(a => a.Sum(b => b.ItemPrice));

                int rowCount = 4;
                foreach (var group in groups)
                {
                    sheet2.Cells[rowCount, 1].Value = suppliers.Where(a => a.Id == group.Key).FirstOrDefault().Name;
                    sheet2.Row(rowCount).OutlineLevel = 1;
                    sheet2.Row(rowCount).Collapsed = true;
                    //sheet2.Row(rowCount).Hidden = true;
                    sheet2.Row(rowCount).Height = 20;
                    sheet2.Row(rowCount).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet2.Row(rowCount).Style.Font.Bold = true;
                    var supplierPriceSum = group.Sum(a => a.ItemPrice);
                    var QuantitySum = group.Sum(a => a.ItemQuantity);

                    sheet2.Cells[rowCount, 3].Value = supplierPriceSum;
                    sheet2.Cells[rowCount, 4].Value = QuantitySum;

                    sheet2.Cells[rowCount, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet2.Cells[rowCount, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

                    sheet2.Cells[rowCount, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet2.Cells[rowCount, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);


                    foreach (var item in group)
                    {
                        sheet2.Row(rowCount + 1).OutlineLevel = 2;
                        sheet2.Row(rowCount + 1).Collapsed = true;
                        sheet2.Row(rowCount + 1).Hidden = true;
                        sheet2.Cells[rowCount + 1, 2].Value = item.InventoryItemName;
                        sheet2.Cells[rowCount + 1, 3].Value = item.ItemPrice;
                        sheet2.Cells[rowCount + 1, 4].Value = item.ItemQuantity;
                        sheet2.Cells[rowCount + 1, 5].Value = item.AddedDate;
                        sheet2.Cells[rowCount + 1, 6].Value = item.InventoryStoreName;

                        sheet2.Cells[rowCount + 1, 1, rowCount + 2, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet2.Cells[rowCount + 1, 1, rowCount + 2, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                        rowCount++;
                    }

                    var range = sheet2.Cells[rowCount, 1, rowCount, 6];

                    // Set the border to bold and black
                    //range.Style.Border.Top.Style = ExcelBorderStyle.Thick;
                    //range.Style.Border.Top.Color.SetColor(System.Drawing.Color.Black);
                    //range.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
                    //range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                    //range.Style.Border.Left.Style = ExcelBorderStyle.Thick;
                    //range.Style.Border.Left.Color.SetColor(System.Drawing.Color.Black);
                    //range.Style.Border.Right.Style = ExcelBorderStyle.Thick;
                    //range.Style.Border.Right.Color.SetColor(System.Drawing.Color.Black);
                    //sheet2.Row(rowCount).Height = 1.5;

                    rowCount++;
                }



                //--------------------------summation---------------------------
                sheet2.Cells[2, 4].Value = totalQuantitySum;
                sheet2.Cells[2, 3].Value = totalPriceSum;

                sheet2.Cells[2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Cells[2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet2.Cells[2, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet2.Cells[2, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);

                sheet2.Cells[2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet2.Cells[2, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet2.Cells[2, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet2.Cells[2, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);
                //------------------------End Summiation----------------------------


                //--------------------end sheet 2-------------------------------
                #endregion

                #region sheet3
                //------------------------start sheet 3 (sales) -----------------------------------------------------
                var sheet3 = excel.Workbook.Worksheets.Add($"Sales");

                sheet3.Cells[1, 1].Value = $"Sales of {storeName} from {startDate.ToShortDateString()} to {endDate.ToShortDateString()}";
                sheet3.Cells[1, 1, 1, 7].Merge = true;
                sheet3.Cells[3, 1].Value = "sales offer Name";
                sheet3.Cells[3, 2].Value = "final Price";
                sheet3.Cells[3, 3].Value = "Creation Date, Time";
                sheet3.Cells[3, 4].Value = "Offer sheet";
                sheet3.Cells[3, 5].Value = "Product Name";
                sheet3.Cells[3, 6].Value = "Quantity";
                sheet3.Cells[3, 7].Value = "Item Unit Price";


                for (int col = 1; col <= 10; col++) sheet3.Column(col).Width = 25;
                sheet3.DefaultRowHeight = 12;
                sheet3.Row(3).Height = 20;
                sheet3.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet3.Row(3).Style.Font.Bold = true;
                sheet3.Cells[3, 1, 3, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet3.Cells[3, 1, 3, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet3.Cells[3, 1, 3, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet3.Cells[3, 1, 3, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet3.Row(1).Height = 20;
                sheet3.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet3.Row(1).Style.Font.Bold = true;
                //sheet3.Cells[1, 1, 1, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet3.Cells[1, 1, 1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet3.Cells[1, 1, 1, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                var listOfStoreData = new List<SalesOfferDueClientPOS>();


                var inventoryStoreItemsData = _unitOfWork.InventoryStoreItems.FindAllQueryable(a => a.CreationDate.Date >= startDate && a.CreationDate.Date < endDate && a.OperationType.Contains("POS"), new[] { "InventoryStore", "CreatedByNavigation" });
                if (inventoryStoreID != null)
                {
                    inventoryStoreItemsData.Where(a => a.InventoryStoreId == inventoryStoreID);
                }
                var salesOffersIDs = inventoryStoreItemsData.Select(a => a.OrderId).ToList();

                var salesOffers = _unitOfWork.SalesOffers.FindAll(a => salesOffersIDs.Contains(a.Id), new[] { "SalesOfferProducts", "CreatedByNavigation", "SalesOfferProducts.InventoryItem" }).ToList();
                var minusSummation = salesOffers.Where(a => a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice);
                var plusSummationu = salesOffers.Where(a => a.OfferType != "Sales Return").Sum(a => a.FinalOfferPrice);
                var totalSummationu = plusSummationu - minusSummation;

                //var inventoryStoreItemsDataGroupes = inventoryStoreItemsData.GroupBy(a => new { a.InventoryStoreId, a.CreatedBy }).ToList();

                //var stories = _unitOfWork.InventoryStores.GetAll();

                //var createdByUserIDs = inventoryStoreItemsData.Select(a => a.CreatedBy).ToList();
                //var createdByUserList = _unitOfWork.Users.FindAll(a => createdByUserIDs.Contains(a.Id));


                decimal sum = 0;
                int rowNum = 4;

                //newSalesOffer.OfferID = item.OrderId;
                //var salesOffer = salesOffers.Where(a => a.Id == inventoryStoreItem.OrderId && a.CreationDate.Date >= startDate && a.CreationDate.Date < endDate);
                var sumiation = salesOffers.Sum(a => a.FinalOfferPrice);
                if (salesOffers != null)
                {
                    foreach (var offer in salesOffers)
                    {
                        sheet3.Row(rowNum).OutlineLevel = 1;
                        sheet3.Row(rowNum).Collapsed = true;

                        sheet3.Cells[rowNum, 1].Value = offer.ProjectName;
                        sheet3.Cells[rowNum, 2].Value = offer.FinalOfferPrice;
                        sheet3.Cells[rowNum, 3].Value = offer.CreationDate.ToShortDateString();
                        sheet3.Cells[rowNum, 4].Value = offer.OfferType;
                        sheet3.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet3.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        foreach (var productDB in offer.SalesOfferProducts)
                        {
                            sheet3.Row(rowNum + 1).OutlineLevel = 2;
                            sheet3.Row(rowNum + 1).Collapsed = true;
                            sheet3.Row(rowNum + 1).Hidden = true;
                            sheet3.Cells[rowNum + 1, 5].Value = productDB.InventoryItem.Name;
                            sheet3.Cells[rowNum + 1, 6].Value = productDB.Quantity;
                            sheet3.Cells[rowNum + 1, 7].Value = productDB.ItemPrice;
                            sheet3.Row(rowNum + 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet3.Row(rowNum + 1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            if (offer.OfferType == "Sales Return")                    //make the row color red on Sales Return
                            {
                                sheet3.Cells[rowNum + 1, 5, rowNum + 1, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                sheet3.Cells[rowNum + 1, 5, rowNum + 1, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                sheet3.Row(rowNum + 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet3.Row(rowNum + 1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }

                            rowNum++;
                        }
                        if (offer.OfferType == "Sales Return") sum = sum - offer.FinalOfferPrice ?? 0;
                        else { sum += offer.FinalOfferPrice ?? 0; }

                        rowNum++;
                    }
                    sheet3.Cells[2, 2].Value = totalSummationu;
                    sheet3.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet3.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet3.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet3.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Green);

                    //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
                    //newSalesOffer.projectName = salesOffer.ProjectName;
                    //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
                    //newSalesOffer.OfferType = salesOffer.OfferType;
                }



                //------------------------end sheet 3 (sales) -------------------------------------------------------
                #endregion

                #region sheet4
                //------------------------------sheet 4 (الرصيد المتبقي) -----------------------------------------
                var sheet4 = excel.Workbook.Worksheets.Add($"الرصيد المتبقي");

                sheet4.Cells[1, 1].Value = $"الرصيد المتبقي في {storeName}";
                sheet4.Cells[1, 1, 1, 7].Merge = true;
                sheet4.Cells[3, 1].Value = "Product Name";
                sheet4.Cells[3, 2].Value = "Item Code";
                sheet4.Cells[3, 3].Value = "Category";
                sheet4.Cells[3, 4].Value = "Supplier";
                sheet4.Cells[3, 5].Value = "Balance";
                sheet4.Cells[3, 6].Value = "Quantity";
                sheet4.Cells[3, 7].Value = "Total Balance";


                for (int col = 1; col <= 10; col++) sheet4.Column(col).Width = 25;
                sheet4.DefaultRowHeight = 12;
                sheet4.Row(1).Height = 20;
                sheet4.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Row(1).Style.Font.Bold = true;

                sheet4.Row(3).Height = 20;
                sheet4.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Row(3).Style.Font.Bold = true;
                sheet4.Cells[3, 1, 3, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[3, 1, 3, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet4.Cells[3, 1, 3, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[3, 1, 3, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var inventiryStoreItemsFinalBalance = _unitOfWork.InventoryStoreItems.FindAllQueryable(a => a.FinalBalance > 0 &&
                (a.OperationType == "Opening Balance" || a.OperationType == "Add New Matrial"), new[] { "InventoryItem", "InventoryItem.InventoryItemCategory" });

                if (inventoryStoreID != null)
                {
                    inventiryStoreItemsFinalBalance.Where(a => a.InventoryStoreId == inventoryStoreID);
                }

                var Sheet4OrderIDs = inventiryStoreItemsFinalBalance.Select(a => a.OrderId).ToList();

                var Sheet4AddingOrders = _unitOfWork.InventoryAddingOrders.FindAll(a => Sheet4OrderIDs.Contains(a.Id), new[] { "Supplier" });

                //var test = storeItemList.Where(a => a.OperationType.Contains("Transfer Order") && a.Balance1 > 0).ToList();
                var Sheet4SuppliersIDs = Sheet4AddingOrders.Select(a => a.SupplierId).ToList();

                var sheet4Suppliers = _unitOfWork.Suppliers.FindAll(a => Sheet4SuppliersIDs.Contains(a.Id));


                var sumBalance = inventiryStoreItemsFinalBalance.Sum(a => a.FinalBalance);
                var sumQuantity = inventiryStoreItemsFinalBalance.Sum(a => a.PoinvoiceTotalCostEgp);
                var sumTotalBalance = 0.0m;

                int counterOfRow = 4;
                foreach (var item in inventiryStoreItemsFinalBalance)
                {
                    sheet4.Cells[counterOfRow, 1].Value = item.InventoryItem.Name;
                    sheet4.Cells[counterOfRow, 2].Value = item.InventoryItem.Code;
                    sheet4.Cells[counterOfRow, 3].Value = item.InventoryItem.InventoryItemCategory.Name;
                    sheet4.Cells[counterOfRow, 4].Value = Sheet4AddingOrders.Where(a => a.Id == item.OrderId).FirstOrDefault().Supplier.Name;
                    sheet4.Cells[counterOfRow, 5].Value = item.FinalBalance;
                    sheet4.Cells[counterOfRow, 6].Value = item.PoinvoiceTotalCostEgp;
                    sheet4.Cells[counterOfRow, 7].Value = item.FinalBalance * item.PoinvoiceTotalCostEgp;
                    sumTotalBalance += item.FinalBalance ?? 0 * item.PoinvoiceTotalCostEgp ?? 0;

                    sheet4.Cells[counterOfRow, 1, counterOfRow, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet4.Cells[counterOfRow, 1, counterOfRow, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    counterOfRow++;
                }

                sheet4.Cells[2, 5].Value = sumBalance;
                sheet4.Cells[2, 6].Value = sumQuantity;
                sheet4.Cells[2, 7].Value = sumTotalBalance;

                sheet4.Cells[2, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[2, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet4.Cells[2, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[2, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);

                sheet4.Cells[2, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[2, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet4.Cells[2, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[2, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);

                sheet4.Cells[2, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[2, 7].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet4.Cells[2, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[2, 7].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);

                //----------------------------end sheet 4 (الرصيد المتبقي)----------------------------------------
                #endregion

                //-----------------------------------Save file -----------------------------------------------
                var path = $"Attachments\\{CompName}\\PurchaseForStore";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\PurchaseForStore.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var fullPath = Globals.baseURL + "\\" + path + $"\\PurchaseForStore.xlsx";

                response.Data = fullPath;
                return response;
                //---------------------------------------------------------------

            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }

        }

        //public AccountAndFinanceInventoryItemMovementResponse GetAccountAndFinanceInventoryItemMovementListV2()
        //{
        //    AccountAndFinanceInventoryItemMovementResponse Response = new AccountAndFinanceInventoryItemMovementResponse();
        //    Response.Result = true;
        //    Response.Errors = new List<Error>();
        //    try
        //    {
        //        var InventoryStoreItemMovmentList = new List<ItemMovement>();
        //        if (Response.Result)
        //        {

        //            int CurrentPage = 1;
        //            if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
        //            {
        //                int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
        //            }

        //            int NumberOfItemsPerPage = 10;
        //            if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
        //            {
        //                int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
        //            }

        //            long InventoryItemID = 0;
        //            if (!string.IsNullOrEmpty(Request.Headers["InventoryItemID"]) && long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID))
        //            {
        //                long.TryParse(Request.Headers["InventoryItemID"], out InventoryItemID);
        //            }
        //            else
        //            {
        //                Response.Result = false;
        //                Error error = new Error();
        //                error.ErrorCode = "Err99";
        //                error.ErrorMSG = "Invalid Inventory Item ID";
        //                Response.Errors.Add(error);
        //                return Response;
        //            }

        //            DateTime? FromDateFilter = null;
        //            DateTime FromDateTemp = DateTime.Now;
        //            if (!string.IsNullOrEmpty(Request.Headers["FromDate"]) && DateTime.TryParse(Request.Headers["FromDate"], out FromDateTemp))
        //            {
        //                FromDateTemp = DateTime.Parse(Request.Headers["FromDate"]);
        //                // FromDateFilter = FromDateTemp;
        //            }


        //            DateTime? ToDateFilter = null;
        //            DateTime ToDateTemp = DateTime.Now;
        //            if (!string.IsNullOrEmpty(Request.Headers["ToDate"]) && DateTime.TryParse(Request.Headers["ToDate"], out ToDateTemp))
        //            {
        //                ToDateTemp = DateTime.Parse(Request.Headers["ToDate"]);
        //                //ToDateFilter = ToDateTemp;
        //            }
        //            long ClientId = 0;
        //            if (!string.IsNullOrEmpty(Request.Headers["ClientId"]) && long.TryParse(Request.Headers["ClientId"], out ClientId))
        //            {
        //                long.TryParse(Request.Headers["ClientId"], out ClientId);
        //            }
        //            long SupplierId = 0;
        //            if (!string.IsNullOrEmpty(Request.Headers["SupplierId"]) && long.TryParse(Request.Headers["SupplierId"], out SupplierId))
        //            {
        //                long.TryParse(Request.Headers["SupplierId"], out SupplierId);
        //            }
        //            long PoId = 0;
        //            if (!string.IsNullOrEmpty(Request.Headers["PoId"]) && long.TryParse(Request.Headers["PoId"], out PoId))
        //            {
        //                long.TryParse(Request.Headers["PoId"], out PoId);
        //            }
        //            long ProjectId = 0;
        //            if (!string.IsNullOrEmpty(Request.Headers["ProjectId"]) && long.TryParse(Request.Headers["ProjectId"], out ProjectId))
        //            {
        //                long.TryParse(Request.Headers["ProjectId"], out ProjectId);
        //            }
        //            string OperationType = null;
        //            if (!string.IsNullOrEmpty(Request.Headers["OperationType"]))
        //            {
        //                OperationType = Request.Headers["OperationType"];
        //            }
        //            //long POID = 0;
        //            //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
        //            //{
        //            //    long.TryParse(headers["POID"], out POID);
        //            //}
        //            decimal cummlativeQty = 0;
        //            if (Response.Result)
        //            {

        //                var InventoryItemMovmentQuerable = _Context.VInventoryStoreItemMovements.Where(x => x.Active == true && x.InventoryItemId == InventoryItemID).OrderBy(x => x.CreationDate).AsQueryable();
        //                // Filters --------
        //                if (OperationType != null)
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.OperationType.Contains(OperationType));
        //                }

        //                if (PoId != 0)
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.AddingFromPoid == PoId);
        //                }
        //                // Filter 
        //                if (!string.IsNullOrEmpty(Request.Headers["FromDate"]))
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.DateFilter >= FromDateTemp);
        //                }

        //                if (!string.IsNullOrEmpty(Request.Headers["ToDate"]))
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.DateFilter <= ToDateTemp);
        //                }

        //                if (ClientId != 0)
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ClientId == ClientId);
        //                }

        //                if (SupplierId != 0)
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.SupplierId == SupplierId);
        //                }
        //                if (ProjectId != 0)
        //                {
        //                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ProjectId == ProjectId);
        //                }


        //                var InventoryStoreItemPagingList = PagedList<VInventoryStoreItemMovement>.Create(InventoryItemMovmentQuerable, CurrentPage, NumberOfItemsPerPage);
        //                //var InventoryStoreItemPagingList = PagedList<V_InventoryStoreItem>.Create(ListInventoryItemMovmentQuerable, CurrentPage, NumberOfItemsPerPage);

        //                Response.PaginationHeader = new PaginationHeader
        //                {
        //                    CurrentPage = CurrentPage,
        //                    TotalPages = InventoryStoreItemPagingList.TotalPages,
        //                    ItemsPerPage = NumberOfItemsPerPage,
        //                    TotalItems = InventoryStoreItemPagingList.TotalCount
        //                };

        //                InventoryStoreItemMovmentList = InventoryStoreItemPagingList.Select(item => new ItemMovement
        //                {

        //                    OperationType = item.OperationType,
        //                    Qty = (double)item.Balance,
        //                    HoldQty = item.HoldQty ?? 0,
        //                    HoldComment = item.HoldReason,
        //                    OrderID = item.OrderId,
        //                    CumilativeQty = (double)InventoryItemMovmentQuerable.Where(x => x.CreationDate <= item.CreationDate).ToList().Select(x => x.Balance).DefaultIfEmpty(0).Sum(),
        //                    StoreName = item.InventoryStoreName,
        //                    ReqUOM = item.RequstionUomname,

        //                    ID = item.Id,
        //                    ParentID = item.ReleaseParentId,
        //                    POID = item.AddingFromPoid,
        //                    ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "",
        //                    ItemSerial = item.ItemSerial,
        //                    RemainBalance = item.FinalBalance,
        //                    CurrencyId = item.CurrencyId,
        //                    CurrencyName = item.CurrencyName,
        //                    RateToEGP = item.RateToEgp,
        //                    POInvoicePriceEGP = item.PoinvoiceTotalPriceEgp,
        //                    POInvoiceUnitCostEGP = item.PoinvoiceTotalCostEgp,
        //                    CreationDate = item.CreationDate.ToShortDateString(),
        //                    FromUser = item.FromUser,
        //                    FromSupplier = item.FromSupplier,
        //                    SupplierId = item.SupplierId,
        //                    FromDepartment = item.FromDepartment,
        //                    OrderType = item.OrderType,
        //                    DateFilter = item.DateFilter,
        //                    Date = item.DateFilter?.ToString("dd-MM-yyyy"),
        //                    ProjectName = item.ProjectName,
        //                    ProjectId = item.ProjectId,
        //                    ClientId = item.ClientId,
        //                    ClientName = item.ClientName

        //                }).ToList();

        //                #region comment
        //                //var ListInventoryItemMovment = _Context.V_InventoryStoreItem.Where(x => x.Active == true && x.InventoryItemID == InventoryItemID).OrderBy(x => x.CreationDate).ToList(); 
        //                //if (InventoryStoreItemPagingList.Count > 0)
        //                //{
        //                //    //  var ListInventoryItemMovmentWithOperationAndItem = ListInventoryItemMovment.Select(x => { OrderID =x.OrderID ,OperationType = x.OperationType}).;
        //                //    //var V_InventoryAddingOrder =   _Context.V_InventoryAddingOrder.Where()
        //                //    // Kan fi Moshkla Fi Double w Decimal fi CumilativeQty kant bttr7 0.1 zyada w sal7taha By Mark Shawky
        //                //    //var ListOfStoreItemQTyWithDate = ListInventoryItemMovment.Select(x => new { x.CreationDate, x.Balance }).ToList();
        //                //    foreach (var item in InventoryStoreItemPagingList)
        //                //    {
        //                //        var ItemMovmentObj = new ItemMovement();
        //                //        ItemMovmentObj.OperationType = item.OperationType;
        //                //        ItemMovmentObj.Qty = (double)item.Balance;
        //                //        ItemMovmentObj.HoldQty = item.holdQty ?? 0;
        //                //        ItemMovmentObj.HoldComment = item.holdReason;
        //                //        ItemMovmentObj.OrderID = item.OrderID;
        //                //        cummlativeQty = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= item.CreationDate).Select(x => x.Balance).DefaultIfEmpty(0).Sum();
        //                //        //cummlativeQty + (decimal)ItemMovmentObj.Qty;
        //                //        ItemMovmentObj.CumilativeQty = (double)cummlativeQty;
        //                //        ItemMovmentObj.StoreName = item.InventoryStoreName;
        //                //        ItemMovmentObj.ReqUOM = item.RequstionUOMName;

        //                //        // Extra DAta PO Item
        //                //        ItemMovmentObj.ID = item.ID;
        //                //        ItemMovmentObj.ParentID = item.releaseParentId;
        //                //        ItemMovmentObj.POID = item.addingFromPOId;
        //                //        ItemMovmentObj.ExpDate = item.expDate != null ? ((DateTime)item.expDate).ToShortDateString() : "";
        //                //        ItemMovmentObj.ItemSerial = item.itemSerial;
        //                //        ItemMovmentObj.RemainBalance = item.finalBalance;
        //                //        ItemMovmentObj.CurrencyId = item.currencyId;
        //                //        ItemMovmentObj.CurrencyName = item.CurrencyName; // Common.GetCurrencyName(item.currencyId ?? 0);
        //                //        ItemMovmentObj.RateToEGP = item.rateToEGP;
        //                //        ItemMovmentObj.POInvoicePriceEGP = item.POInvoiceTotalPriceEGP;
        //                //        ItemMovmentObj.POInvoiceUnitCostEGP = item.POInvoiceTotalCostEGP;
        //                //        ItemMovmentObj.CreationDate = item.CreationDate.ToShortDateString();
        //                //        if (item.Balance > 0)
        //                //        {
        //                //            ItemMovmentObj.remainItemCostEGP = item.remainItemCosetEGP;
        //                //            ItemMovmentObj.remainItemCostOtherCU = item.remainItemCostOtherCU;

        //                //        }
        //                //        ItemMovmentObj.FromUser = item.FromUser;
        //                //        ItemMovmentObj.FromSupplier = item.FromSupplier;
        //                //        ItemMovmentObj.SupplierId = item.SupplierId;
        //                //        ItemMovmentObj.FromDepartment = item.FromDepartment;
        //                //        ItemMovmentObj.OrderType = item.OrderType;
        //                //            ItemMovmentObj.DateFilter = item.DateFilter;
        //                //            ItemMovmentObj.Date = item.DateFilter?.ToString("dd-MM-yyyy");
        //                //        // V_InventoryInternalBackOrder or MatrialRelease
        //                //        ItemMovmentObj.ProjectName = item.ProjectName;
        //                //        ItemMovmentObj.ProjectId = item.ProjectId;
        //                //        ItemMovmentObj.ClientId = item.ClientID;
        //                //        ItemMovmentObj.ClientName = item.ClientName;

        //                //        InventoryStoreItemMovmentList.Add(ItemMovmentObj);
        //                //    }

        //                //}
        //                #endregion

        //                DateTime DateFrom = DateTime.Now;
        //                DateTime DateTo = DateTime.Now;
        //                var DateFromTemp = InventoryItemMovmentQuerable.Where(x => x.DateFilter != null).OrderBy(x => x.DateFilter).Select(x => x.DateFilter).FirstOrDefault();
        //                var DateToTemp = InventoryItemMovmentQuerable.Where(x => x.DateFilter != null).OrderByDescending(x => x.DateFilter).Select(x => x.DateFilter).FirstOrDefault();
        //                if (DateFromTemp != null)
        //                {
        //                    DateFrom = (DateTime)DateFromTemp;
        //                }
        //                if (DateToTemp != null)
        //                {
        //                    DateTo = (DateTime)DateToTemp;
        //                }


        //                //var InventoryStoreItemMovmentListFilter = InventoryStoreItemMovmentList.Where(x => x.DateFilter != null).OrderBy(x => x.DateFilter).ToList();
        //                //DateFrom = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).FirstOrDefault();
        //                //DateTO = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).LastOrDefault();
        //                var InventoryItemMovmentListFilter = InventoryItemMovmentQuerable;
        //                //List<V_InventoryStoreItemMovement> InventoryItemMovmentListFilter = new List<V_InventoryStoreItemMovement>();
        //                double numberOfMonths = Math.Abs(Math.Ceiling(DateTo.Subtract(DateFrom).Days / (365.25 / 12)));
        //                if (DateFrom <= DateTo)
        //                {
        //                    InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.DateFilter >= DateFrom && x.DateFilter <= DateTo).AsQueryable();
        //                }
        //                else
        //                {
        //                    InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.DateFilter >= DateTo && x.DateFilter <= DateFrom).AsQueryable();
        //                }

        //                //if (InventoryItemMovmentListFilter.Count > 0)
        //                //{
        //                var ReleaseQty = InventoryItemMovmentListFilter.Where(x => x.OperationType.Contains("Release Order")).ToList().Select(x => Math.Abs(x.Balance)).DefaultIfEmpty(0).Sum();
        //                Response.ReleaseQty = (double)ReleaseQty;
        //                Response.ReleaseRate = numberOfMonths != 0 ? (Response.ReleaseQty / numberOfMonths) : 0;
        //                //}
        //                Response.NoOfMonth = numberOfMonths;
        //                Response.DateFrom = DateFrom.ToShortDateString();
        //                //!string.IsNullOrWhiteSpace(headers["FromDate"]) ? FromDateTemp.ToString("dd-MM-yyyy") : DateFrom.ToString("dd-MM-yyy" + "+y");
        //                Response.DateTo = DateTo.ToShortDateString();
        //                //!string.IsNullOrWhiteSpace(headers["ToDate"]) ? ToDateTemp.ToString("dd-MM-yyyy") : DateTO.ToString("dd-MM-yyyy");
        //                Response.InventoryItemMovementList = InventoryStoreItemMovmentList;
        //            }
        //        }
        //        return Response;

        //    }
        //    catch (Exception ex)
        //    {
        //        Response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
        //        Response.Errors.Add(error);
        //        return Response;
        //    }
        //}

        public AccountAndFinanceInventoryItemStockBalanceResponse GetAccountAndFinanceInventoryItemStockBalance(long InventoryItemID)
        {
            AccountAndFinanceInventoryItemStockBalanceResponse Response = new AccountAndFinanceInventoryItemStockBalanceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var TotalAvailableStock = 0.0;

                var InventoryStoreItemMovmentList = new List<InventoryItemStockBlance>();
                if (Response.Result)
                {
                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item ID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        var InventoryStoreItemListDB = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemID).ToList();
                        var AddingOrderItemsList = _unitOfWork.InventoryAddingOrderItems.FindAll(x => x.InventoryItemId == InventoryItemID, new[] { "InventoryAddingOrder" }).ToList();
                        // Not Grouped -----------------------------------------
                        var IDSInventoryStoreItemListDB = InventoryStoreItemListDB.Select(x => x.InventoryStoreId).Distinct().ToList();
                        int Counter = 0;
                        foreach (var InventoryStoreID in IDSInventoryStoreItemListDB)
                        {
                            var Balance = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).Select(x => x.Balance1).ToList().Sum();
                            TotalAvailableStock += (double)Balance;
                            Counter++;
                            var InventoryStoreItemObj = new InventoryItemStockBlance();
                            InventoryStoreItemObj.No = Counter;
                            InventoryStoreItemObj.StoreId = InventoryStoreID;
                            InventoryStoreItemObj.StoreName = _unitOfWork.InventoryStores.GetById(InventoryStoreID).Name;//Common.getInventoryStoreName(InventoryStoreID);
                            InventoryStoreItemObj.Balance = (decimal)Balance;
                            var LocationBalanceList = new List<LocationBalance>();
                            var InventoryLocationBalanceList = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID && x.FinalBalance > 0).GroupBy(x => x.InvenoryStoreLocationId).ToList();
                            foreach (var item in InventoryLocationBalanceList)
                            {
                                //string storeLocation = _unitOfWork.InventoryStoreLocations.GetById(item.Key??0).Location;
                                var LocationObj = new LocationBalance();
                                LocationObj.LocationId = item.Key ?? 0;
                                LocationObj.LocationName = item.Key != null ? _unitOfWork.InventoryStoreLocations.GetById(item.Key ?? 0)?.Location/*Common.GetStoreLocationName(item.Key)*/ : "Missing Location";
                                LocationObj.Balance = item.Sum(x => x.FinalBalance ?? 0);
                                LocationBalanceList.Add(LocationObj);
                            }
                            InventoryStoreItemObj.LocationBalance = LocationBalanceList;
                            var AddingOrderItemsCommentsList = AddingOrderItemsList.Where(x => x.InventoryAddingOrder?.InventoryStoreId == InventoryStoreID && x.Comments != null)
                                        .Select(x => x.Comments).ToList();
                            InventoryStoreItemObj.ListOfComments = AddingOrderItemsCommentsList;
                            InventoryStoreItemMovmentList.Add(InventoryStoreItemObj);
                        }

                        Response.TotalBalance = (decimal)InventoryStoreItemListDB.Where(x => x.Balance1 > 0 && x.OperationType.Contains("Opening Balance")).Select(x => x.Balance1).DefaultIfEmpty(0).Sum();
                    }
                    Response.InventoryItemMovementList = InventoryStoreItemMovmentList;
                    Response.AvailableStock = (decimal)TotalAvailableStock;
                    Response.UOR = _unitOfWork.VInventoryStoreItems.Find(a => a.InventoryItemId == InventoryItemID)?.RequestionUomshortName;//Common.getInventoryStoreItemUOMName(InventoryItemID);
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

        public BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse> GetAccountAndFinanceInventoryItemMovementListV2(AccountAndFinanceInventoryItemMovementListV2Filters filters)
        {
            BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse> Response = new BaseResponseWithDataAndHeader<AccountAndFinanceInventoryItemMovementResponse>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region date validation
                DateTime FromDateFilter = new DateTime(DateTime.Now.Year, 1, 1);
                if (!string.IsNullOrEmpty(filters.DateFrom))
                {
                    if (!DateTime.TryParse(filters.DateFrom, out FromDateFilter))
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.errorMSG = "please, Enter a valid DateFrom";
                        Response.Errors.Add(err);
                        return Response;
                    }
                }

                DateTime ToDateFilter = DateTime.Now;
                if (!string.IsNullOrEmpty(filters.DateTo))
                {
                    if (!DateTime.TryParse(filters.DateTo, out ToDateFilter))
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "E-1";
                        err.ErrorMSG = "please, Enter a valid DateTo";
                        Response.Errors.Add(err);
                        return Response;
                    }
                }

                #endregion

                var InventoryStoreItemMovmentList = new List<ItemMovement>();
                if (Response.Result)
                {
                    decimal cummlativeQty = 0;
                    if (Response.Result)
                    {

                        var InventoryItemMovmentQuerable = _unitOfWork.VInventoryStoreItemMovements.FindAllQueryable(x => x.Active == true && x.InventoryItemId == filters.InventoryItemID);//.OrderBy(x => x.CreationDate);
                        // Filters --------
                        if (filters.OperationType != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.OperationType.Contains(filters.OperationType));
                        }

                        if (filters.PoId != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.AddingFromPoid == filters.PoId);
                        }
                        // Filter 
                        if (!string.IsNullOrEmpty(filters.DateFrom))
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= FromDateFilter);
                        }

                        if (!string.IsNullOrEmpty(filters.DateTo))
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= ToDateFilter);
                        }

                        if (filters.ClientId != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ClientId == filters.ClientId);
                        }

                        if (filters.SupplierId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.SupplierId == filters.SupplierId);
                        }
                        if (filters.ProjectId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ProjectId == filters.ProjectId);
                        }
                        if (filters.StoreID != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.InventoryStoreId == filters.StoreID);
                        }
                        InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.OrderBy(x => x.CreationDate);
                        var InventoryStoreItemList = InventoryItemMovmentQuerable.ToList();
                        InventoryStoreItemMovmentList = InventoryStoreItemList.Select(item => new ItemMovement
                        {

                            OperationType = item.OperationType,
                            Qty = (double)item.Balance,
                            HoldQty = item.HoldQty ?? 0,
                            HoldComment = item.HoldReason,
                            OrderID = item.OrderId,
                            CumilativeQty = (double)InventoryItemMovmentQuerable.Where(x => x.Id <= item.Id).ToList().Select(x => x.Balance).DefaultIfEmpty(0).Sum(),
                            StoreName = item.InventoryStoreName,
                            ReqUOM = item.RequstionUomname,
                            ID = item.Id,
                            ParentID = item.ReleaseParentId,
                            POID = item.AddingFromPoid,
                            ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "",
                            ItemSerial = item.ItemSerial,
                            RemainBalance = item.FinalBalance,
                            CurrencyId = item.CurrencyId,
                            CurrencyName = item.CurrencyName,
                            RateToEGP = item.RateToEgp,
                            POInvoicePriceEGP = item.PoinvoiceTotalPriceEgp,
                            POInvoiceUnitCostEGP = item.PoinvoiceTotalCostEgp,
                            CreationDate = item.CreationDate.ToString(),
                            FromUser = item.FromUser,
                            FromSupplier = item.FromSupplier,
                            SupplierId = item.SupplierId,
                            FromDepartment = item.FromDepartment,
                            OrderType = item.OrderType,
                            DateFilter = item.DateFilter,
                            Date = item.DateFilter?.ToString(),
                            ProjectName = item.ProjectName,
                            ProjectId = item.ProjectId,
                            ClientId = item.ClientId,
                            ClientName = item.ClientName

                        }).ToList();

                        var InventoryItemMovmentListFilter = InventoryItemMovmentQuerable;
                        double numberOfMonths = Math.Abs(Math.Ceiling(ToDateFilter.Subtract(FromDateFilter).Days / (365.25 / 12)));
                        if (FromDateFilter <= ToDateFilter)
                        {
                            InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= FromDateFilter && x.CreationDate <= ToDateFilter).AsQueryable();
                        }
                        else
                        {
                            InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= ToDateFilter && x.CreationDate <= FromDateFilter).AsQueryable();
                        }
                        var ReleaseQty = InventoryItemMovmentListFilter.Where(x => x.OperationType.Contains("Release Order") ||
                                                                                   x.OperationType.Contains("POS Release")).ToList().Select(x => Math.Abs(x.Balance)).DefaultIfEmpty(0).Sum();

                        var responseData = new AccountAndFinanceInventoryItemMovementResponse();

                        responseData.ReleaseQty = (double)ReleaseQty;
                        responseData.ReleaseRate = numberOfMonths != 0 ? (responseData.ReleaseQty / numberOfMonths) : 0;
                        responseData.NoOfMonth = numberOfMonths;
                        responseData.DateFrom = FromDateFilter.ToShortDateString();
                        responseData.DateTo = ToDateFilter.ToShortDateString();
                        responseData.InventoryItemMovementList = InventoryStoreItemMovmentList;

                        Response.Data = responseData;
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


        public BaseResponseWithData<string> GetInventoryItemMovementReport(long InventoryItemID, string DateFrom, string DateTo, long? storeID, string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            if (!string.IsNullOrEmpty(DateFrom))
            {
                var startDate = DateTime.Now;
                if (!DateTime.TryParse(DateFrom, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid DateFrom";
                    response.Errors.Add(err);
                    return response;
                }
            }
            if (!string.IsNullOrEmpty(DateTo))
            {
                var endDate = DateTime.Now;
                if (!DateTime.TryParse(DateTo, out endDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.ErrorMSG = "please, Enter a valid DateTo";
                    response.Errors.Add(err);
                    return response;
                }
            }
            #endregion
            try
            {
                if (InventoryItemID == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.ErrorMSG = "please, Enter a valid InventoryItemID";
                    response.Errors.Add(err);
                    return response;
                }
                var inventoryItemDetials = _unitOfWork.InventoryItems.GetById(InventoryItemID);
                var currentStockData = GetAccountAndFinanceInventoryItemStockBalance(InventoryItemID);

                ExcelPackage excel = new ExcelPackage();

                var sheet = excel.Workbook.Worksheets.Add($"InventoryItemMovement");


                sheet.Cells[2, 1].Value = "Current Stock";
                sheet.Cells[2, 4].Value = "UOR";

                sheet.Cells[2, 1, 2, 3].Merge = true;
                //sheet.Cells[3, 1, 3, 3].Merge = true;

                sheet.Cells[3, 3].Value = currentStockData.AvailableStock;
                sheet.Cells[3, 4].Value = currentStockData.UOR;
                sheet.Cells[3, 1, 3, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[3, 1, 3, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                //--------------------------------------styling-------------------------------------------
                for (int col = 1; col <= 24; col++)
                {
                    if (col == 7 || col == 8)
                    {
                        sheet.Column(col).Width = 8;
                    }
                    else if (col == 2 || col == 16 || col == 17 || col == 18)
                    {
                        sheet.Column(col).Width = 20;
                    }
                    else if (col == 12)
                    {
                        sheet.Column(col).Width = 15;
                    }
                    else if (col == 1)
                    {
                        sheet.Column(col).Width = 12;
                    }
                    else
                    {
                        sheet.Column(col).Width = 25;
                    }
                }
                sheet.DefaultRowHeight = 15;
                sheet.Row(2).Height = 20;
                sheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(2).Style.Font.Bold = true;
                sheet.Cells[2, 1, 2, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[2, 1, 2, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[2, 1, 2, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[2, 1, 2, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                sheet.Cells[4, 1].Value = "No.";
                sheet.Cells[4, 2].Value = "Store Name";
                sheet.Cells[4, 3].Value = "Available";
                sheet.Cells[4, 4].Value = "UOR";

                sheet.Row(4).Height = 20;
                sheet.Row(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(4).Style.Font.Bold = true;
                sheet.Cells[4, 1, 4, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[4, 1, 4, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[4, 1, 4, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[4, 1, 4, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                int rowCount = 5;
                foreach (var item in currentStockData.InventoryItemMovementList)
                {
                    sheet.Cells[rowCount, 1].Value = item.No;
                    sheet.Cells[rowCount, 2].Value = item.StoreName;
                    sheet.Cells[rowCount, 3].Value = item.Balance;
                    sheet.Cells[rowCount, 4].Value = currentStockData.UOR;

                    sheet.Cells[rowCount, 1, rowCount, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowCount, 1, rowCount, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    rowCount++;
                }

                //--------------------------------------part 2 (movment table)------------------------------
                var filter = new AccountAndFinanceInventoryItemMovementListV2Filters();
                filter.InventoryItemID = InventoryItemID;
                filter.CurrentPage = 1;
                filter.NumberOfItemsPerPage = 10000;
                filter.DateFrom = DateFrom;
                filter.DateTo = DateTo;
                if (storeID != null)
                {
                    filter.StoreID = storeID;           //for spacific store
                }

                var movementData = GetAccountAndFinanceInventoryItemMovementListV2(filter);

                //sheet.Column(2).Width = 35;
                sheet.Cells[rowCount, 1].Value = "Item Name :";
                sheet.Cells[rowCount, 2].Value = _unitOfWork.InventoryItems.GetById(InventoryItemID)?.Name;
                sheet.Cells[rowCount, 3].Value = "Release Rate :";
                sheet.Cells[rowCount, 4].Value = movementData.Data.ReleaseRate;

                sheet.Row(rowCount).Height = 20;
                sheet.Row(rowCount).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(rowCount).Style.Font.Bold = true;
                sheet.Cells[rowCount, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[rowCount, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[rowCount, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[rowCount, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                sheet.Cells[rowCount, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[rowCount, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[rowCount, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[rowCount, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                sheet.Cells[rowCount + 1, 1].Value = "ID";
                sheet.Cells[rowCount + 1, 2].Value = "Date";
                sheet.Cells[rowCount + 1, 3].Value = "OrderID";
                sheet.Cells[rowCount + 1, 4].Value = "StoreName";
                sheet.Cells[rowCount + 1, 5].Value = "FromUser";
                sheet.Cells[rowCount + 1, 6].Value = "operation Type";
                sheet.Cells[rowCount + 1, 7].Value = "Qty.";
                sheet.Cells[rowCount + 1, 8].Value = "Acc.";
                sheet.Cells[rowCount + 1, 9].Value = "Item Serial";
                sheet.Cells[rowCount + 1, 10].Value = "Exp. Date";
                sheet.Cells[rowCount + 1, 11].Value = "ReqUOM";
                sheet.Cells[rowCount + 1, 12].Value = "RemainBalance";
                sheet.Cells[rowCount + 1, 13].Value = "ParentID";
                sheet.Cells[rowCount + 1, 14].Value = "POID";
                sheet.Cells[rowCount + 1, 15].Value = "FromSupplier";
                sheet.Cells[rowCount + 1, 16].Value = "POInvoiceUnitCostEGP";
                sheet.Cells[rowCount + 1, 17].Value = "POInvoiceUnitPriceEGP";
                sheet.Cells[rowCount + 1, 18].Value = "totalcostEGP";
                sheet.Cells[rowCount + 1, 19].Value = "remainItemCostOtherCU";
                sheet.Cells[rowCount + 1, 20].Value = "ClientID/Project";
                sheet.Cells[rowCount + 1, 21].Value = "FromDepartment";
                sheet.Cells[rowCount + 1, 22].Value = "HoldQty";
                //sheet.Cells[rowCount + 1, 24].Value = "remainItemCostOtherCU";

                sheet.Row(rowCount + 1).Height = 20;
                sheet.Row(rowCount + 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(rowCount + 1).Style.Font.Bold = true;
                sheet.Cells[rowCount + 1, 1, rowCount + 1, 22].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[rowCount + 1, 1, rowCount + 1, 22].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[rowCount + 1, 1, rowCount + 1, 22].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[rowCount + 1, 1, rowCount + 1, 22].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var totalRemainRow = rowCount;
                rowCount += 2;
                decimal totalRemain = 0;
                double accu = 0;
                decimal totalOfToltalCost = 0;
                foreach (var item in movementData.Data.InventoryItemMovementList)
                {
                    accu += item.Qty;
                    decimal totalCost = 0;
                    if (item.RemainBalance > 0)
                    {
                        totalCost = (item.RemainBalance ?? 0) * (item.POInvoiceUnitCostEGP ?? 0);
                    }

                    sheet.Cells[rowCount, 1].Value = item.ID;
                    if (item.DateFilter != null)
                    {
                        sheet.Cells[rowCount, 2].Value = item.Date;
                    }
                    else
                    {
                        sheet.Cells[rowCount, 2].Value = item.CreationDate;
                    }
                    sheet.Cells[rowCount, 3].Value = item.OrderID;
                    sheet.Cells[rowCount, 4].Value = item.StoreName;
                    sheet.Cells[rowCount, 5].Value = item.FromUser;
                    sheet.Cells[rowCount, 6].Value = item.OperationType;
                    sheet.Cells[rowCount, 7].Value = item.Qty;
                    sheet.Cells[rowCount, 8].Value = accu;
                    sheet.Cells[rowCount, 9].Value = item.ItemSerial;
                    sheet.Cells[rowCount, 10].Value = item.ExpDate;
                    sheet.Cells[rowCount, 11].Value = item.ReqUOM;

                    if (item.RemainBalance > 0)
                    {
                        sheet.Cells[rowCount, 12].Value = item.RemainBalance;
                        sheet.Cells[rowCount, 18].Value = totalCost;
                        totalRemain += item.RemainBalance ?? 0;
                        totalOfToltalCost += totalCost;
                    }
                    sheet.Cells[rowCount, 13].Value = item.ParentID;
                    sheet.Cells[rowCount, 14].Value = item.POID;
                    sheet.Cells[rowCount, 15].Value = item.FromSupplier;
                    sheet.Cells[rowCount, 16].Value = item.POInvoiceUnitCostEGP;
                    sheet.Cells[rowCount, 17].Value = item.POInvoicePriceEGP;//what is unitPrice
                    //totalcost up 
                    sheet.Cells[rowCount, 19].Value = item.remainItemCostOtherCU;
                    sheet.Cells[rowCount, 20].Value = item.ClientId + "/" + item.ProjectName;
                    sheet.Cells[rowCount, 21].Value = item.FromDepartment;
                    sheet.Cells[rowCount, 22].Value = item.HoldQty;


                    sheet.Cells[rowCount, 1, rowCount, 22].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowCount, 1, rowCount, 22].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                    rowCount++;
                }
                sheet.Cells[totalRemainRow, 12, totalRemainRow, 12].Value = totalRemain;
                sheet.Cells[totalRemainRow, 18, totalRemainRow, 18].Value = totalOfToltalCost;
                //-----------------------------------Save file -----------------------------------------------
                string[] arabicWords = inventoryItemDetials?.Name.Split(' ');
                string finalArabicName = string.Join("_", arabicWords);

                var path = $"Attachments\\{CompName}\\InventoryItemMovement";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var currentDate = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\{finalArabicName}_{currentDate}.xlsx";

                if (Directory.Exists(savedPath))
                {
                    // Get all files in the directory
                    string[] files = Directory.GetFiles(savedPath);

                    // Iterate through each file and delete it
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }

                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var fullPath = Globals.baseURL + "\\" + path + $"\\{finalArabicName}_{currentDate}.xlsx";

                response.Data = fullPath;
                return response;
                //---------------------------------------------------------------

            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }

        }

        public BaseResponseWithId<long> EditInventoryStorePerID(EditInventoryStoreData Request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
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

                    if (Request.ID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Inventory Store ID.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {
                        var modifiedUser = _unitOfWork.Users.GetById(creator);

                        if (Request.ID != 0)
                        {
                            var InventoryStoreDB = _unitOfWork.InventoryStores.FindAllAsync(x => x.Id == Request.ID).Result.FirstOrDefault();
                            if (InventoryStoreDB == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Store Doesn't Exist!!";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            if (InventoryStoreDB != null)
                            {
                                var ListLocationVMCheck = Request.editInventoryStoreLocationData;
                                var ListLocationDBCheck = _unitOfWork.InventoryStoreLocations.FindAll(x => x.InventoryStoreId == Request.ID).ToList();
                                var IDSListLocationsDBCheck = ListLocationDBCheck.Select(x => x.Id).ToList();
                                var IDSListLocationsVMCheck = ListLocationVMCheck.Where(x => x.ID != 0).Select(x => x.ID).ToList();
                                var IDSListToRemoveCheck = IDSListLocationsDBCheck.Except(IDSListLocationsVMCheck).ToList();
                                var DeletedIDsCheck = _unitOfWork.InventoryStoreItems.FindAll(x => IDSListToRemoveCheck.Contains((int)x.InvenoryStoreLocationId)).ToList();



                                var ListKeeperVMCheck = Request.EditInventoryStoreKeeperData;
                                var ListKeeperDBCheck = _unitOfWork.InventoryStoreKeepers.FindAllAsync(x => x.InventoryStoreId == Request.ID).Result.ToList();
                                var IDSListKeebersDBCheck = ListKeeperDBCheck.Select(x => x.Id).ToList();
                                var IDSListKeebersVMCheck = ListKeeperVMCheck.Where(x => x.ID != 0).Select(x => x.ID).ToList();
                                var IDSListToRemoveForUserCheck = IDSListKeebersDBCheck.Except(IDSListKeebersVMCheck).ToList();

                                if (DeletedIDsCheck != null && DeletedIDsCheck.Count() > 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Store Location Has Relation Can't Delete it !!";
                                    Response.Errors.Add(error);
                                    return Response;
                                }


                                InventoryStoreDB.Name = Request.StoreName;
                                InventoryStoreDB.Active = Request.Active;
                                InventoryStoreDB.Location = Request.Location;
                                InventoryStoreDB.Tel = Request.Tel;
                                InventoryStoreDB.ModifiedBy = creator;
                                InventoryStoreDB.ModifiedDate = DateTime.Now;

                                _unitOfWork.InventoryStores.Update(InventoryStoreDB);








                                // Store Keaper List
                                // step 1  check  3 cases
                                var ListKeeperVM = Request.EditInventoryStoreKeeperData;
                                var ListKeeperDB = _unitOfWork.InventoryStoreKeepers.FindAllAsync(x => x.InventoryStoreId == Request.ID).Result.ToList();
                                // delete all then insert again
                                _unitOfWork.InventoryStoreKeepers.DeleteRange(ListKeeperDB);
                                _unitOfWork.Complete();

                                if (ListKeeperVM != null && ListKeeperVM.Count() > 0)
                                {
                                    var ListInventoryStoreKeeper = new List<InventoryStoreKeeper>();
                                    ListKeeperVM = ListKeeperVM.Where(x => x.Active == true).ToList();
                                    foreach (var item in ListKeeperVM)
                                    {
                                        var InsertKeeperObjDB = new InventoryStoreKeeper();
                                        InsertKeeperObjDB.InventoryStoreId = Request.ID;
                                        InsertKeeperObjDB.UserId = item.UserID;
                                        InsertKeeperObjDB.Active = true;
                                        InsertKeeperObjDB.CreatedBy = creator;
                                        InsertKeeperObjDB.ModifiedBy = creator;
                                        InsertKeeperObjDB.CreationDate = DateTime.Now;
                                        InsertKeeperObjDB.ModifiedDate = DateTime.Now;
                                        ListInventoryStoreKeeper.Add(InsertKeeperObjDB);
                                    }
                                    _unitOfWork.InventoryStoreKeepers.AddRange(ListInventoryStoreKeeper);
                                    _unitOfWork.Complete();

                                }

                                //if (ListKeeperVM != null && ListKeeperVM.Count() > 0)
                                //{
                                //    var IDSListKeebersDB = ListKeeperDB.Select(x => x.ID).ToList();
                                //    var IDSListKeebersVM = ListKeeperVM.Where(x => x.ID != 0).Select(x => x.ID).ToList();



                                //    foreach (var item in ListKeeperVM)
                                //    {
                                //        if (item.ID != 0) // Edit
                                //        {
                                //            var UpdateKeeperObjDB = ListKeeperDB.Where(x => x.ID == item.ID).FirstOrDefault();
                                //            UpdateKeeperObjDB.UserID = item.UserID;
                                //            UpdateKeeperObjDB.Active = item.Active;
                                //            _Context.SaveChanges();
                                //        }
                                //        else //Insert
                                //        {
                                //            var CheckIfExistBefore = ListKeeperDB.Where(x => x.UserID == item.UserID).FirstOrDefault();
                                //            if (CheckIfExistBefore == null)
                                //            {
                                //                var InsertKeeperObjDB = new InventoryStoreKeeper();
                                //                InsertKeeperObjDB.InventoryStoreID = Request.ID;
                                //                InsertKeeperObjDB.UserID = item.UserID;
                                //                InsertKeeperObjDB.Active = item.Active;
                                //                InsertKeeperObjDB.CreatedBy = validation.userID;
                                //                InsertKeeperObjDB.ModifiedBy = validation.userID;
                                //                InsertKeeperObjDB.CreationDate = DateTime.Now;
                                //                InsertKeeperObjDB.ModifiedDate = DateTime.Now;
                                //                _Context.InventoryStoreKeepers.Add(InsertKeeperObjDB);
                                //                var Res = _Context.SaveChanges();
                                //                if (Res > 0)
                                //                {
                                //                    IDSListKeebersDB.Add(InsertKeeperObjDB.ID);
                                //                    IDSListKeebersVM.Add(InsertKeeperObjDB.ID);
                                //                }
                                //            }
                                //        }
                                //    }

                                //    var DeletedKeeperListDB = ListKeeperDBCheck.Where(x => IDSListKeebersVMCheck.Contains(x.ID)).ToList();
                                //    _Context.InventoryStoreKeepers.RemoveRange(DeletedKeeperListDB);
                                //    _Context.SaveChanges();


                                //}
                                //else // List is empty must be deleted
                                //{
                                //    // delete list from DB
                                //    _Context.InventoryStoreKeepers.RemoveRange(ListKeeperDB);
                                //    _Context.SaveChanges();
                                //}









                                // Store Fronton List
                                // step 1  check  3 cases
                                var ListLocationVM = Request.editInventoryStoreLocationData;
                                var ListLocationDB = _unitOfWork.InventoryStoreLocations.FindAll(x => x.InventoryStoreId == Request.ID).ToList();
                                if (ListLocationVM != null && ListLocationVM.Count() > 0)
                                {
                                    var IDSListLocationsDB = ListLocationDB.Select(x => x.Id).ToList();
                                    var IDSListLocationsVM = ListLocationVM.Where(x => x.ID != 0).Select(x => x.ID).ToList();



                                    foreach (var itemLoc in ListLocationVM)
                                    {

                                        if (itemLoc.ID != 0) // Edit
                                        {
                                            var UpdateLocationsObjDB = ListLocationDB.Where(x => x.Id == itemLoc.ID).FirstOrDefault();
                                            if (UpdateLocationsObjDB != null)
                                            {
                                                UpdateLocationsObjDB.Location = itemLoc.Location;
                                                UpdateLocationsObjDB.Active = itemLoc.Active;
                                                //UpdateLocationsObjDB.InventoryStoreID = itemLoc.InventoryStoreID;
                                                _unitOfWork.Complete();
                                            }
                                        }
                                        else //Insert
                                        {
                                            var CheckIfExistBefore = ListLocationDB.Where(x => x.Location == itemLoc.Location).Select(x => x.Id).FirstOrDefault();
                                            if (CheckIfExistBefore == 0)
                                            {
                                                var InsertLocationsObjDB = new InventoryStoreLocation();
                                                InsertLocationsObjDB.InventoryStoreId = Request.ID;
                                                InsertLocationsObjDB.Location = itemLoc.Location;
                                                InsertLocationsObjDB.Active = itemLoc.Active;
                                                InsertLocationsObjDB.CreatedBy = creator;
                                                InsertLocationsObjDB.ModifiedBy = creator;
                                                InsertLocationsObjDB.CreationDate = DateTime.Now;
                                                InsertLocationsObjDB.ModifiedDate = DateTime.Now;
                                                _unitOfWork.InventoryStoreLocations.Add(InsertLocationsObjDB);
                                                var Res = _unitOfWork.Complete();
                                                if (Res > 0)
                                                {
                                                    IDSListLocationsDB.Add(InsertLocationsObjDB.Id);
                                                    IDSListLocationsVM.Add(InsertLocationsObjDB.Id);
                                                }
                                            }
                                        }
                                    }


                                    var DeletedLocationListDB = ListLocationDBCheck.Where(x => IDSListToRemoveCheck.Contains(x.Id)).ToList();
                                    _unitOfWork.InventoryStoreLocations.DeleteRange(DeletedLocationListDB);
                                    _unitOfWork.Complete();


                                }
                                else // List is empty must be deleted
                                {
                                    // delete list from DB
                                    _unitOfWork.InventoryStoreLocations.DeleteRange(ListLocationDB);
                                    _unitOfWork.Complete();
                                }






                            }








                        }




                    }


                    Response.ID = Request.ID;

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

        public async Task<InventoryItemStockBalanceHoldResponse> GetAccountAndFinanceInventoryItemStockBalanceHold(long InventoryItemID)
        {
            InventoryItemStockBalanceHoldResponse Response = new InventoryItemStockBalanceHoldResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                decimal TotalHoldStock = 0;
                Response.InventoryStoreHoldItemByStoreList = new List<InventoryStoreHoldItemByStore>();
                var InventoryStoreItemByStoreList = new List<InventoryStoreHoldItemByStore>();
                if (Response.Result)
                {
                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item ID";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (Response.Result)
                    {

                        //List Stores ID From Item Holded
                        // IsHold => Hold , IsHold => Hold Released


                        var InvStoreItemListGroupedByStoreName = _unitOfWork.VInventoryStoreItems.FindAllAsync(x => x.InventoryItemId == InventoryItemID && x.HoldQty > 0).Result.GroupBy(x => x.InventoryStoreName).ToList();



                        //var MatrialRequestItemGroupedByStore = _Context.V_InventoryMatrialRequestItems.Where(x => x.InventoryItemID == InventoryItemID && x.IsHold == true).GroupBy(x => x.InventoryStoreName).ToList();
                        //var InvStoreItemGroupedByStore = _Context.V_InventoryMatrialRequestItems.Where(x => x.InventoryItemID == InventoryItemID && x.IsHold == true).GroupBy(x => x.InventoryStoreName).ToList();



                        if (InvStoreItemListGroupedByStoreName.Count > 0)
                        {
                            foreach (var MRItemPerStore in InvStoreItemListGroupedByStoreName)
                            {
                                var MRItemDataList = new List<InventroryStoreItemHold>();

                                foreach (var Data in MRItemPerStore)
                                {

                                    MRItemDataList.Add(new InventroryStoreItemHold
                                    {
                                        InvStoreItemID = Data.Id,
                                        HoldQTY = Data.HoldQty,
                                        HoldReason = Data.HoldReason,
                                        UOM = Data.RequstionUomname
                                    });

                                    TotalHoldStock += Data.HoldQty != null ? (decimal)Data.HoldQty : 0;
                                }
                                // var InvStoreItemHoldQTY = _Context.InventoryStoreItems.Where(x => x.InventoryItemID == InventoryItemID && x.holdQty != null).Select(x => x.holdQty ?? 0).ToList().Sum();
                                InventoryStoreItemByStoreList.Add(new InventoryStoreHoldItemByStore()
                                {
                                    StoreName = MRItemPerStore.Key,
                                    TotalHoldQTY = /*InvStoreItemHoldQTY,*/ MRItemPerStore.Where(x => x.HoldQty != null).Select(x => x.HoldQty ?? 0).ToList().Sum(),
                                    InventoryMatrialRequestInfoList = MRItemDataList,
                                });
                            }
                        }


                        //if (MatrialRequestItemGroupedByStore.Count > 0)
                        //{
                        //    foreach (var MRItemPerStore in MatrialRequestItemGroupedByStore)
                        //    {
                        //        var MRItemDataList = new List<InventoryMatrialRequestItemInfo>();

                        //        foreach (var Data in MRItemPerStore)
                        //        {

                        //            MRItemDataList.Add(new InventoryMatrialRequestItemInfo
                        //            {
                        //                InventoryMatrialRequestID = Data.InventoryMatrialRequestID,
                        //                InventoryMatrialRequestItemID = Data.ID,
                        //                MatrialRequestStatus = Data.MaterialRequestStatus == "Hold" || Data.MaterialRequestStatus == "Hold Released" ? Data.MaterialRequestStatus : "",
                        //                MatrialRequestItemIsHold = (bool)Data.IsHold,
                        //                HoldQTY = Data.ReqQuantity,
                        //                UOM = Data.RequestedUOMShortName,
                        //                ProjectName = Data.ProjectName,
                        //                FabOrderName = Data.FabricationOrderID != null ? Data.FabricationOrderID.ToString() : ""
                        //            });

                        //            TotalAvailableStock += Data.ReqQuantity != null ? (decimal)Data.ReqQuantity : 0;
                        //        }
                        //        var InvStoreItemHoldQTY = _Context.InventoryStoreItems.Where(x => x.InventoryItemID == InventoryItemID && x.holdQty != null).Select(x => x.holdQty ?? 0).ToList().Sum();
                        //        InventoryMtrialRequestItemByStoreList.Add(new InventoryMtrialRequestItemByStore()
                        //        {
                        //            StoreName = MRItemPerStore.Key,
                        //            AvailableQTY = /*InvStoreItemHoldQTY,*/ MRItemPerStore.Where(x => x.ReqQuantity != null).Select(x => (decimal)x.ReqQuantity).Sum(),
                        //            InventoryMatrialRequestInfoList = MRItemDataList,
                        //        });
                        //    }
                        //}


                    }
                    Response.InventoryStoreHoldItemByStoreList = InventoryStoreItemByStoreList;
                    Response.TotalItemHoldStock = TotalHoldStock;
                    Response.UOR = _unitOfWork.VInventoryStoreItems.Find(a => a.InventoryItemId == InventoryItemID)?.RequestionUomshortName;
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

        public BaseResponseWithData<string> GetInventoryItemRelaseRate(string DateFrom, string DateTo, string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region date validation
            DateTime startDate = new DateTime(DateTime.Now.Year, 1, 1);
            if (!string.IsNullOrEmpty(DateFrom))
            {
                if (!DateTime.TryParse(DateFrom, out startDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid DateFrom";
                    response.Errors.Add(err);
                    return response;
                }
            }

            DateTime endDate = DateTime.Now;
            if (!string.IsNullOrEmpty(DateTo))
            {
                if (!DateTime.TryParse(DateTo, out endDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.ErrorMSG = "please, Enter a valid DateTo";
                    response.Errors.Add(err);
                    return response;
                }
            }

            #endregion



            try
            {
                //var inventoryItemsViewData = _unitOfWork.VInventoryStoreItemMovements.FindAll(a => a.date)
                var InventoryItemMovmentQuerable = _unitOfWork.VInventoryStoreItemMovements.FindAllQueryable(x => x.Active == true);

                var currentDate = DateTime.Now;
                if (!string.IsNullOrEmpty(DateFrom))
                {
                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= startDate);
                }
                else       //get year to date
                {
                    var YTDStart = currentDate.AddYears(-1);
                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= YTDStart);
                }

                if (!string.IsNullOrEmpty(DateTo))
                {
                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= endDate);
                }
                else
                {
                    InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= currentDate);
                }



                //DateTime DateStart = DateTime.Now;
                //DateTime DateEnd = DateTime.Now;
                //var DateFromTemp = InventoryItemMovmentQuerable.Where(x => x.DateFilter != null).OrderBy(x => x.DateFilter).Select(x => x.DateFilter).FirstOrDefault();
                //var DateToTemp = InventoryItemMovmentQuerable.Where(x => x.DateFilter != null).OrderByDescending(x => x.DateFilter).Select(x => x.DateFilter).FirstOrDefault();
                //if (DateFromTemp != null)
                //{
                //    DateStart = (DateTime)DateFromTemp;
                //}
                //if (DateToTemp != null)
                //{
                //    DateEnd = (DateTime)DateToTemp;
                //}
                //var InventoryStoreItemMovmentListFilter = InventoryStoreItemMovmentList.Where(x => x.DateFilter != null).OrderBy(x => x.DateFilter).ToList();
                //DateFrom = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).FirstOrDefault();
                //DateTO = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).LastOrDefault();

                double numberOfMonths = Math.Abs(Math.Ceiling(endDate.Subtract(startDate).Days / (365.25 / 12)));

                var InventoryItemMovmentListFilter = InventoryItemMovmentQuerable;
                //List<V_InventoryStoreItemMovement> InventoryItemMovmentListFilter = new List<V_InventoryStoreItemMovement>();
                if (startDate <= endDate)
                {
                    InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= startDate && x.CreationDate <= endDate).AsQueryable();
                }
                else
                {
                    InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= endDate && x.CreationDate <= startDate).AsQueryable();
                }
                var list = InventoryItemMovmentListFilter.ToList();
                //if (InventoryItemMovmentListFilter.Count > 0)
                //{


                var InventoryItemMovmentList = InventoryItemMovmentQuerable.ToList();
                var inventoryItemGroups = InventoryItemMovmentList.GroupBy(a => a.InventoryItemId).ToList();

                var itemsIDs = InventoryItemMovmentList.Select(a => a.InventoryItemId).ToList();
                var inventoryItemList = _unitOfWork.InventoryItems.FindAll(a => itemsIDs.Contains(a.Id), new[] { "InventoryItemCategory", "InventoryStoreItems", "InventoryStoreItems.InventoryStore" });

                var storesIDs = InventoryItemMovmentList.Select(a => a.InventoryStoreId).ToList();
                var storesData = _unitOfWork.InventoryStores.FindAll(a => storesIDs.Contains(a.Id)).ToList();
                var inventoryStoresList = _unitOfWork.InventoryStoreItems.FindAll(a => storesIDs.Contains(a.InventoryStoreId));
                var AddingOrderItemsList = _unitOfWork.InventoryAddingOrderItems.FindAll(x => itemsIDs.Contains(x.InventoryItemId)).ToList();

                ExcelPackage excel = new ExcelPackage();
                var sheet = excel.Workbook.Worksheets.Add($"InventoryItemRelaseRate");

                for (int col = 1; col <= 6; col++) sheet.Column(col).Width = 25;
                sheet.DefaultRowHeight = 15;
                sheet.Cells[1, 1].Value = "Item Name";
                sheet.Cells[1, 2].Value = "Item Category";
                sheet.Cells[1, 3].Value = "Current Balance";
                sheet.Cells[1, 4].Value = "Store";
                sheet.Cells[1, 5].Value = "Release Rate";
                //sheet.Cells[1, 6].Value = "FromUser";

                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Cells[1, 1, 1, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, 5].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Gray);
                sheet.Cells[1, 1, 1, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[1, 1, 1, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                var rowCount = 2;
                foreach (var group in inventoryItemGroups)
                {
                    var ReleaseQty = InventoryItemMovmentListFilter.Where(x => (x.OperationType.Contains("Release Order") ||
                                                                           x.OperationType.Contains("POS Release")) && x.InventoryItemId == group.Key).ToList().Select(x => Math.Abs(x.Balance)).DefaultIfEmpty(0).Sum();


                    //var ReleaseQty = (double)ReleaseQty;
                    var ReleaseRate = numberOfMonths != 0 ? ((double)ReleaseQty / numberOfMonths) : 0;

                    var inventoryItem = inventoryItemList.Where(a => a.Id == group.Key).FirstOrDefault();

                    sheet.Row(rowCount).OutlineLevel = 1;
                    sheet.Row(rowCount).Collapsed = false;
                    sheet.Cells[rowCount, 1].Value = inventoryItem.Name;
                    sheet.Cells[rowCount, 2].Value = inventoryItem?.InventoryItemCategory?.Name;
                    sheet.Cells[rowCount, 3].Value = inventoryItem?.InventoryStoreItems.Where(a => a.FinalBalance > 0).Sum(s => s.FinalBalance);
                    sheet.Cells[rowCount, 4].Value = "All";
                    sheet.Cells[rowCount, 5].Value = ReleaseRate;

                    sheet.Cells[rowCount, 1, rowCount, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[rowCount, 1, rowCount, 5].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rowCount++;
                    //------------------------------------Balance in each store-----------------------------------------------------

                    var IDSInventoryStoreItemListDB = inventoryStoresList.Select(x => x.InventoryStoreId).Distinct().ToList();
                    var TotalAvailableStock = 0.0;
                    foreach (var InventoryStoreID in IDSInventoryStoreItemListDB)
                    {
                        var Balance = inventoryStoresList.Where(x => x.InventoryStoreId == InventoryStoreID && x.InventoryItemId == group.Key).Select(x => x.Balance1).ToList().Sum();
                        TotalAvailableStock += (double)Balance;

                        sheet.Row(rowCount).OutlineLevel = 2;
                        sheet.Row(rowCount).Collapsed = true;
                        sheet.Row(rowCount).Hidden = true;
                        if (Balance > 0)
                        {
                            sheet.Cells[rowCount, 3].Value = Balance;
                            sheet.Cells[rowCount, 4].Value = storesData.Where(a => a.Id == InventoryStoreID).FirstOrDefault().Name;
                            sheet.Cells[rowCount, 3, rowCount, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet.Cells[rowCount, 3, rowCount, 4].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            sheet.Cells[rowCount, 3, rowCount, 4].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            sheet.Cells[rowCount, 3, rowCount, 4].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                            rowCount++;
                        }
                    }
                }

                //----------------------------file saving------------------------------
                var path = $"Attachments\\{CompName}\\InventoryItemRelase";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                //var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                //var excelPath = savedPath + $"\\InventoryItemRelase.xlsx";
                //excel.SaveAs(excelPath);
                //// Write content to excel file  
                ////File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                ////Close Excel package 
                //excel.Dispose();
                //var fullPath = Globals.baseURL + "\\" + path + $"\\InventoryItemRelase.xlsx";

                //response.Data = fullPath;
                //return response;




                var FileCurrentDate = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\InventoryItemRelase_{FileCurrentDate}.xlsx";

                if (Directory.Exists(savedPath))
                {
                    // Get all files in the directory
                    string[] files = Directory.GetFiles(savedPath);

                    // Iterate through each file and delete it
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }

                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var fullPath = Globals.baseURL + "\\" + path + $"\\InventoryItemRelase_{FileCurrentDate}.xlsx";

                response.Data = fullPath;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<InventoryStoreItemByOrderResponse> GetInventoryStoreItemByOrder(long OrderId, string OperationType)
        {
            var response = new BaseResponseWithData<InventoryStoreItemByOrderResponse>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            if (OrderId == 0)
            {
                response.Result = false;
                var err = new Error();
                err.ErrorCode = "err-10";
                err.ErrorMSG = "please enter a valid OrderID";
                response.Errors.Add(err);
                return response;
            }
            if (string.IsNullOrEmpty(OperationType))
            {
                response.Result = false;
                var err = new Error();
                err.ErrorCode = "err-10";
                err.ErrorMSG = "please enter a valid OperationType";
                response.Errors.Add(err);
                return response;
            }
            #endregion


            try
            {
                var inventoryStoreItemData = _unitOfWork.InventoryStoreItems.FindAllQueryable(a => a.OrderId == OrderId, new[] { "InventoryItem" });

                if (OperationType.Contains("Add New Matrial") || OperationType.Contains("opening balance") || OperationType.Contains("adding"))
                {
                    inventoryStoreItemData = inventoryStoreItemData.Where(a => a.OperationType.Contains("Add New Matrial") || a.OperationType.Contains("opening balance"));

                }
                else
                {
                    inventoryStoreItemData = inventoryStoreItemData.Where(a => a.OperationType.Contains(OperationType));
                }

                var inventoryStoreItemList = inventoryStoreItemData.ToList();

                var InventoryStoreItemByOrderResponse = new InventoryStoreItemByOrderResponse();

                InventoryStoreItemByOrderResponse.TotalBalance = inventoryStoreItemList.Sum(a => a.Balance1);
                InventoryStoreItemByOrderResponse.TotalCountOfItem = inventoryStoreItemList.Select(a => a.InventoryItemId).Distinct().Count();

                var InventoryStoreItemByOrderData = new List<InventoryStoreItemByOrderData>();

                foreach (var item in inventoryStoreItemList)
                {
                    var inventoryStoreItem = new InventoryStoreItemByOrderData();

                    inventoryStoreItem.ID = item.Id;
                    inventoryStoreItem.Balance = item.Balance1;
                    inventoryStoreItem.ExpDate = item.ExpDate?.ToShortDateString();
                    inventoryStoreItem.ItemSerial = item.InventoryItem.Code;
                    inventoryStoreItem.FinalBalance = item.FinalBalance ?? 0;
                    inventoryStoreItem.AddingFromPOId = item.AddingFromPoid;
                    inventoryStoreItem.POInvoiceTotalCostEGP = item.PoinvoiceTotalCostEgp;
                    inventoryStoreItem.POInvoiceTotalPriceEGP = item.PoinvoiceTotalPriceEgp;
                    inventoryStoreItem.ItemName = item.InventoryItem.Name;

                    InventoryStoreItemByOrderData.Add(inventoryStoreItem);
                }
                InventoryStoreItemByOrderResponse.InventoryStoreItems = InventoryStoreItemByOrderData;


                if (OperationType.Contains("Add New Matrial") || OperationType.Contains("opening balance") || OperationType.Contains("adding"))
                {
                    var InventoryAddingOrder = _unitOfWork.InventoryAddingOrders.Find(a => a.Id == OrderId, new[] { "CreatedByNavigation", "Supplier", "InventoryStore" });

                    if (InventoryAddingOrder != null)
                    {

                        InventoryStoreItemByOrderResponse.OrderID = InventoryAddingOrder.Id;
                        InventoryStoreItemByOrderResponse.CreatorId = InventoryAddingOrder.CreatedBy;
                        InventoryStoreItemByOrderResponse.CreatorName = InventoryAddingOrder.CreatedByNavigation.FirstName + " " + InventoryAddingOrder.CreatedByNavigation.LastName;
                        InventoryStoreItemByOrderResponse.SupplierId = InventoryAddingOrder.SupplierId;
                        InventoryStoreItemByOrderResponse.SupplierName = InventoryAddingOrder.Supplier.Name;
                        InventoryStoreItemByOrderResponse.StoreId = InventoryAddingOrder.InventoryStoreId;
                        InventoryStoreItemByOrderResponse.StoreName = InventoryAddingOrder.InventoryStore.Name;
                        InventoryStoreItemByOrderResponse.Type = InventoryAddingOrder.OperationType;
                        InventoryStoreItemByOrderResponse.Revision = InventoryAddingOrder.Revision;
                        InventoryStoreItemByOrderResponse.CreationDate = InventoryAddingOrder.CreationDate.ToShortDateString();
                        InventoryStoreItemByOrderResponse.RecivingDate = InventoryAddingOrder.RecivingDate.ToShortDateString();


                    }
                }

                //----------------------------InventoryInternalTransferOrder--------------------
                if (OperationType.ToLower().Contains("received") || OperationType.ToLower().Contains("released") || OperationType.ToLower().Contains("transfer order"))
                {
                    var InventoryInternalTransferOrder = _unitOfWork.InventoryInternalTransferOrders.Find(a => a.Id == OrderId, new[] { "CreatedByNavigation", "FromInventoryStore", "ToInventoryStore" });

                    if (InventoryInternalTransferOrder != null)
                    {
                        InventoryStoreItemByOrderResponse.OrderID = InventoryInternalTransferOrder.Id;
                        InventoryStoreItemByOrderResponse.CreatorId = InventoryInternalTransferOrder.CreatedBy;
                        InventoryStoreItemByOrderResponse.CreatorName = InventoryInternalTransferOrder.CreatedByNavigation.FirstName + " " + InventoryInternalTransferOrder.CreatedByNavigation.LastName;
                        InventoryStoreItemByOrderResponse.Type = InventoryInternalTransferOrder.OperationType;
                        InventoryStoreItemByOrderResponse.Revision = InventoryInternalTransferOrder.Revision;
                        InventoryStoreItemByOrderResponse.CreationDate = InventoryInternalTransferOrder.CreationDate.ToShortDateString();
                        InventoryStoreItemByOrderResponse.RecivingDate = InventoryInternalTransferOrder.RecivingDate.ToShortDateString();
                        InventoryStoreItemByOrderResponse.FromInventoryStoreID = InventoryInternalTransferOrder.FromInventoryStoreId;
                        InventoryStoreItemByOrderResponse.FromInventoryStoreName = InventoryInternalTransferOrder.FromInventoryStore.Name;
                        InventoryStoreItemByOrderResponse.ToInventoryStoreID = InventoryInternalTransferOrder.ToInventoryStoreId;
                        InventoryStoreItemByOrderResponse.ToInventoryStoreName = InventoryInternalTransferOrder.ToInventoryStore.Name;


                    }
                }

                //-------------------------InventoryMatrialRelease-----------------------------

                if (OperationType.ToLower().Contains("release") || OperationType.ToLower().Contains("return") || OperationType.ToLower().Contains("pos"))
                {
                    var InventoryMatrialRelease = _unitOfWork.InventoryMatrialReleases.Find(a => a.Id == OrderId, new[] { "CreatedByNavigation", "ToUser", "FromInventoryStore" });

                    if (InventoryMatrialRelease != null)
                    {
                        InventoryStoreItemByOrderResponse.OrderID = InventoryMatrialRelease.Id;
                        InventoryStoreItemByOrderResponse.CreatorId = InventoryMatrialRelease.CreatedBy;
                        InventoryStoreItemByOrderResponse.CreatorName = InventoryMatrialRelease.CreatedByNavigation.FirstName + " " + InventoryMatrialRelease.CreatedByNavigation.LastName;
                        InventoryStoreItemByOrderResponse.CreationDate = InventoryMatrialRelease.CreationDate.ToShortDateString();
                        InventoryStoreItemByOrderResponse.RequestDate = InventoryMatrialRelease.RequestDate.ToShortDateString();
                        InventoryStoreItemByOrderResponse.FromInventoryStoreID = InventoryMatrialRelease.FromInventoryStoreId;
                        InventoryStoreItemByOrderResponse.ToUserID = InventoryMatrialRelease.ToUserId;
                        InventoryStoreItemByOrderResponse.ToUserName = InventoryMatrialRelease.ToUser.FirstName + " " + InventoryMatrialRelease.ToUser.LastName;
                        InventoryStoreItemByOrderResponse.MatrialRequestID = InventoryMatrialRelease.MatrialRequestId;
                        InventoryStoreItemByOrderResponse.Status = InventoryMatrialRelease.Status;

                    }
                }

                response.Data = InventoryStoreItemByOrderResponse;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }


        public async Task<decimal> GetTotalAmountInventoryItemByListOfItemExpired(List<long> ListOFItem)
        {
            decimal totalValue = 0;
            var InventoryStoreItemQuerableDB = _unitOfWork.VInventoryStoreItemPrices.FindAllQueryable(x => x.Active == true && x.StoreActive == true);
            if (ListOFItem != null && ListOFItem.Any())
            {
                InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => ListOFItem.Contains(x.InventoryItemId)).AsQueryable();
            }
            var InventoryStoreItemListDB = await InventoryStoreItemQuerableDB.ToListAsync();
            /*foreach (var Store in InventoryStoreItemListDB)
            {
                if (Store.CalculationType == 1)
                {
                    if (Store.SumaverageUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SumaverageUnitPrice;
                    }
                }
                else if (Store.CalculationType == 2)
                {
                    if (Store.SummaxUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SummaxUnitPrice;
                    }
                }
                else if (Store.CalculationType == 3)
                {
                    if (Store.SumlastUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SumlastUnitPrice;
                    }
                }
                else if (Store.CalculationType == 4)
                {
                    if (Store.SumcustomeUnitPrice != null)
                    {
                        totalValue = totalValue + (decimal)Store.SumcustomeUnitPrice;
                    }
                }
            }*/
            totalValue = InventoryStoreItemQuerableDB
                        .Where(Store =>
                         (Store.CalculationType == 1 && Store.SumaverageUnitPrice != null) ||
                         (Store.CalculationType == 2 && Store.SummaxUnitPrice != null) ||
                         (Store.CalculationType == 3 && Store.SumlastUnitPrice != null) ||
                         (Store.CalculationType == 4 && Store.SumcustomeUnitPrice != null))
                        .Sum(Store =>
                         Store.CalculationType == 1 ? (decimal)Store.SumaverageUnitPrice :
                         Store.CalculationType == 2 ? (decimal)Store.SummaxUnitPrice :
                         Store.CalculationType == 3 ? (decimal)Store.SumlastUnitPrice :
                         Store.CalculationType == 4 ? (decimal)Store.SumcustomeUnitPrice : 0);
            return totalValue;
        }

        public Tuple<long, decimal> GetTotalAmountWithNoOFLowStockInventoryItem(long? InventoryStoreID)
        {
            long NoOfItem = 0;
            decimal TotalInvetoryItem = 0;
            var InventoryItemListDB = _unitOfWork.VInventoryStoreItemPrices.FindAllQueryable(x => x.Active == true && x.StoreActive == true);
            if (InventoryStoreID != null && InventoryStoreID != 0)
            {
                InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }
            //var ListOfItemMINBalanceList = InventoryItemListDB.Select(x => new { InventoryItemID = x.InventoryItemID, MinBalance = x.MinBalance }).Distinct().AsQueryable();
            var InventoryItemGroupingPerItem = InventoryItemListDB.GroupBy(x => new { InventoryItemID = x.InventoryItemId, MinBalance = x.MinBalance });
            NoOfItem = InventoryItemGroupingPerItem.Where(x => x.Sum(a => a.Balance) <= x.Key.MinBalance).Count();
            if (InventoryItemListDB.Count() > 0)
            {
                TotalInvetoryItem = InventoryItemListDB.Where(x => x.Balance != null).Sum(a => (decimal)a.Balance);
            }

            // Old Code Not try write this
            //----------------------------
            //foreach (var item in ListOfItemMINBalanceList)
            //{
            //    decimal ItemTotalBalance = InventoryItemListDB.Where(x => x.InventoryItemID == item.InventoryItemID).Select(x => (decimal)x.Balance).Sum();
            //    if (ItemTotalBalance <= item.MinBalance)
            //    {
            //        NoOfItem++;
            //        TotalInvetoryItem += ItemTotalBalance;
            //    }
            //}
            return Tuple.Create(NoOfItem, TotalInvetoryItem);
        }

        public int GetNoOfSupplierAddingAndExternalBackOrder(long InventoryStoreID, string OperationType)
        {
            int NoOfItem = 0;
            var InventoryAddingOrderQuerable = _unitOfWork.InventoryAddingOrders.FindAllQueryable(x => x.OperationType == OperationType);
            if (InventoryStoreID != 0)
            {
                InventoryAddingOrderQuerable = InventoryAddingOrderQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }
            NoOfItem = InventoryAddingOrderQuerable.Select(x => x.SupplierId).Distinct().Count();
            return NoOfItem;
        }

        public async Task<InventoryAndStoresDashboardResponse> GetInventoryAndStoresDashboard([FromHeader] long InventoryStoreID, [FromHeader] DateTime? DateTo)
        {
            InventoryAndStoresDashboardResponse Response = new InventoryAndStoresDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (DateTo == null)
                {
                    DateTo = DateTime.Now;
                }
                var InventoryAndStoreDashboardInfoObj = new InventoryAndStoresDashboardInfo();
                if (Response.Result)
                {
                    //  Inventory Item -----
                    InventoryAndStoreDashboardInfoObj.InventoryItemsNo = Common.GetNoOFInventoryItem(_Context);
                    var InventoryStoreItemPriceList = _unitOfWork.VInventoryStoreItemPrices.FindAllQueryable(x => x.Active == true);

                    // Adding and External back order
                    var ExternalBackOrderLisQuerable = _unitOfWork.InventoryAddingOrders.FindAllQueryable(x => x.OperationType == "Add External Back Order"
                    && x.CreationDate <= DateTo);
                    var AddingOrderQuerable = _unitOfWork.InventoryAddingOrders.FindAllQueryable(x => x.OperationType == "Add New Matrial" && x.CreationDate <= DateTo);
                    // Internal back order and final Product adding order
                    var InternalBackOrderQuerable = _unitOfWork.InventoryInternalBackOrders.FindAllQueryable(x => x.OperationType == "Internal Back Order" && x.CreationDate <= DateTo);
                    var FinalProductAddingOrderLisQuerable = _unitOfWork.InventoryInternalBackOrders.FindAllQueryable(x => (x.OperationType == "Final Product" || x.OperationType == "Semi-Final Product" && x.CreationDate <= DateTo));
                    // Internal back order and final Product adding order
                    var InternalTransferOrderQuerable = _unitOfWork.InventoryInternalTransferOrders.FindAllQueryable(x => x.CreationDate <= DateTo);
                    //Matrial Request order
                    var MatrialRequestOrderOpenQuerable = _unitOfWork.InventoryMatrialRequests.FindAllQueryable(x => x.Active == true && x.Status == "Open" && x.CreationDate <= DateTo);
                    var MatrialRequestOrderClosedQuerable = _unitOfWork.InventoryMatrialRequests.FindAllQueryable(x => x.Active == true && x.Status == "Closed" && x.CreationDate <= DateTo);
                    //Matrial Release order
                    var MatrialReleaseOrderQuerable = _unitOfWork.InventoryMatrialReleases.FindAllQueryable(x => x.Active == true && x.CreationDate <= DateTo);


                    //Inventory Ajusting Report --------------
                    // Check Inventory Report Approved and closed or not
                    var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAllQueryable(x => x.Active == true && x.Approved == false && x.Closed == false && x.DateFrom <= DateTime.Now && x.DateTo >= DateTime.Now);

                    if (InventoryStoreID != 0)
                    {
                        InventoryStoreItemPriceList = InventoryStoreItemPriceList.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                        // Adding and External back order
                        ExternalBackOrderLisQuerable = ExternalBackOrderLisQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                        AddingOrderQuerable = AddingOrderQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                        // Internal back order and final Product adding order
                        InternalBackOrderQuerable = InternalBackOrderQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                        FinalProductAddingOrderLisQuerable = FinalProductAddingOrderLisQuerable.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                        // Internal Transfer Order 
                        InternalTransferOrderQuerable = InternalTransferOrderQuerable.Where(x => (x.FromInventoryStoreId == InventoryStoreID || x.ToInventoryStoreId == InventoryStoreID)).AsQueryable();
                        //Matrial Request order
                        MatrialRequestOrderOpenQuerable = MatrialRequestOrderOpenQuerable.Where(x => x.ToInventoryStoreId == InventoryStoreID).AsQueryable();
                        MatrialRequestOrderClosedQuerable = MatrialRequestOrderClosedQuerable.Where(x => x.ToInventoryStoreId == InventoryStoreID).AsQueryable();
                        //Matrial Release order
                        MatrialReleaseOrderQuerable = MatrialReleaseOrderQuerable.Where(x => x.FromInventoryStoreId == InventoryStoreID).AsQueryable();

                        // InventoryReports 
                        CheckInventoryReportListDB = CheckInventoryReportListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                    }
                    // Adding and External back order
                    //var ExternalBackOrderLisQuerable = _Context.proc_InventoryAddingOrderLoadAll().Where(x => x.OperationType == "Add External Back Order").AsQueryable();
                    //var AddingOrderQuerable = _Context.proc_InventoryAddingOrderLoadAll().Where(x => x.OperationType == "Add New Matrial").AsQueryable();
                    //var AddingOrderAndExternalBackOrderItems = _Context.proc_InventoryAddingOrderItemsLoadAll().AsQueryable();
                    // Internal back order and final Product adding order
                    //var InternalBackOrderQuerable = InternalAndFinalProductAddingOrderQuerable.Where(x => x.OperationType == "Internal Back Order").AsQueryable();
                    //var FinalProductAddingOrderLisQuerable = InternalAndFinalProductAddingOrderQuerable.Where(x => (x.OperationType == "Final Product" || x.OperationType == "Semi-Final Product")).AsQueryable();
                    //var InternalBackOrderAndFinalProductOrderItems = _Context.proc_InventoryInternalBackOrderItemsLoadAll().AsQueryable();

                    // Internal Transfer Order 
                    var InternalTransferOrderLisQuerable = InternalTransferOrderQuerable.Where(x => (x.OperationType == "Add New Transfer")).AsQueryable();
                    var InternalTransferOrderItems = _unitOfWork.InventoryInternalTransferOrderItems.FindAllQueryable(a => true);



                    // List in Items Order =--------------------

                    // Adding Order
                    var IDSAddingOrderList = AddingOrderQuerable.Select(x => x.Id).ToList();
                    var AddingOrderItemList = _unitOfWork.InventoryAddingOrderItems.FindAllQueryable(x => IDSAddingOrderList.Contains(x.InventoryAddingOrderId)).AsQueryable();

                    // ExternalBack Order
                    var IDSExternalBackOrderOrderList = ExternalBackOrderLisQuerable.Select(x => x.Id).ToList();
                    var ExternalBackOrderItemList = _unitOfWork.InventoryAddingOrderItems.FindAllQueryable(x => IDSExternalBackOrderOrderList.Contains(x.InventoryAddingOrderId));


                    // Internal back  Order
                    var IDSInternalBackOrderOrderList = InternalBackOrderQuerable.Select(x => x.Id).ToList();
                    var InternalBackOrderItemList = _unitOfWork.InventoryInternalBackOrderItems.FindAllQueryable(x => IDSInternalBackOrderOrderList.Contains(x.InventoryInternalBackOrderId)).Select(x => x.Id).AsQueryable();

                    // Final Product Adding Order
                    var IDSFinalProductAddingOrderList = FinalProductAddingOrderLisQuerable.Select(x => x.Id).ToList();
                    var FinalProductAddingOrderItemList = _unitOfWork.InventoryInternalBackOrderItems.FindAllQueryable(x => IDSFinalProductAddingOrderList.Contains(x.InventoryInternalBackOrderId)).Select(x => x.Id).AsQueryable();


                    // Internal Transfer Order
                    var IDSInternalTransferOrderList = InternalTransferOrderLisQuerable.Select(x => x.Id).ToList();
                    var InternalTransferOrderItemList = InternalTransferOrderItems.Where(x => IDSInternalTransferOrderList.Contains(x.InventoryInternalTransferOrderId)).Select(x => x.Id).AsQueryable();

                    //Matrial Release Order
                    var IDSMatrialReleaseOrderList = MatrialReleaseOrderQuerable.Select(x => x.Id).ToList();
                    var MatrialReleaseOrderItemList = _unitOfWork.InventoryMatrialReleaseItems.FindAllQueryable(x => IDSMatrialReleaseOrderList.Contains(x.InventoryMatrialReleasetId)).Select(x => x.Id);
                    //Matrial Request Open and Closed Order
                    var IDSMatrialRequestOrderOpenList = MatrialRequestOrderOpenQuerable.Select(x => x.Id).ToList();
                    var IDSMatrialRequestOrderClosedList = MatrialRequestOrderClosedQuerable.Select(x => x.Id).ToList();
                    var MatrialRequestOrderItemOpenList = _unitOfWork.InventoryMatrialRequestItems.FindAllQueryable(x => IDSMatrialRequestOrderOpenList.Contains(x.InventoryMatrialRequestId)).Select(x => x.Id);
                    var MatrialRequestOrderItemClosedList = _unitOfWork.InventoryMatrialRequestItems.FindAllQueryable(x => IDSMatrialRequestOrderClosedList.Contains(x.InventoryMatrialRequestId)).Select(x => x.Id).AsQueryable();





                    InventoryAndStoreDashboardInfoObj.InventoryStoreItemsPricedNo = InventoryStoreItemPriceList.Select(x => x.InventoryItemId).Distinct().Count();
                    InventoryAndStoreDashboardInfoObj.InventoryStoreItemsTotalAmount = Common.GetTotalAmountInventoryItem(InventoryStoreID, _Context); //0 =>  All Items



                    // Inventory Expired Item ------

                    var ItemListExpired = AddingOrderItemList.Where(x => x.ExpDate != null ? (DateTime)x.ExpDate >= DateTime.Now : false).Select(x => x.InventoryItemId).Distinct().ToList();
                    InventoryAndStoreDashboardInfoObj.ExpiredItemsNo = ItemListExpired.Count();
                    InventoryAndStoreDashboardInfoObj.ExpiredItemsTotalAmount = await GetTotalAmountInventoryItemByListOfItemExpired(ItemListExpired); //0 =>  All Items

                    // Inventory Item low stock Item
                    var TotalAmountWithNoOFLowStockInventoryItem = GetTotalAmountWithNoOFLowStockInventoryItem(InventoryStoreID);
                    InventoryAndStoreDashboardInfoObj.LowStockItemsNo = TotalAmountWithNoOFLowStockInventoryItem.Item1;
                    InventoryAndStoreDashboardInfoObj.TotalLowStockItems = TotalAmountWithNoOFLowStockInventoryItem.Item2;












                    // Inventory Adding Order ----------
                    InventoryAndStoreDashboardInfoObj.AddingOrderNo = IDSAddingOrderList.Count();
                    InventoryAndStoreDashboardInfoObj.AddingOrderFromSupplierNo = GetNoOfSupplierAddingAndExternalBackOrder(InventoryStoreID, "Add New Matrial");
                    // InventoryAndStoreDashboardInfoObj.AddingOrderFromSupplierNo = AddingOrderQuerable.Select(x=>x.SupplierID).Distinct().AsQueryable().Count();
                    //InventoryAndStoreDashboardInfoObj.AddingOrderItemsNo = AddingOrderItemList.Count();
                    InventoryAndStoreDashboardInfoObj.AddingOrderItemsNo = _unitOfWork.InventoryAddingOrderItems.FindAll(x => IDSAddingOrderList.Contains(x.InventoryAddingOrderId)).ToList().Count();

                    // Inventory External Back Order ----------
                    InventoryAndStoreDashboardInfoObj.ExternalBackOrdersNo = IDSExternalBackOrderOrderList.Count();
                    InventoryAndStoreDashboardInfoObj.ExternalBackOrdersToSupplierNo = GetNoOfSupplierAddingAndExternalBackOrder(InventoryStoreID, "Add External Back Order");
                    //   InventoryAndStoreDashboardInfoObj.ExternalBackOrdersToSupplierNo = ExternalBackOrderLisQuerable.Select(x => x.SupplierID).Distinct().ToList().Count();
                    InventoryAndStoreDashboardInfoObj.ExternalBackOrdersItemsNo = ExternalBackOrderItemList.Count();
                    //InventoryAndStoreDashboardInfoObj.ExternalBackOrdersItemsNo = _Context.proc_InventoryAddingOrderItemsLoadAll().Where(x => IDSExternalBackOrderOrderList.Contains(x.InventoryAddingOrderID)).Count();
                    // Inventory Internal Back Order ----------
                    InventoryAndStoreDashboardInfoObj.InternalBackOrdersNo = IDSInternalBackOrderOrderList.Count();
                    InventoryAndStoreDashboardInfoObj.InternalBackOrdersItemsNo = InternalBackOrderItemList.Count();
                    //InventoryAndStoreDashboardInfoObj.InternalBackOrdersItemsNo = _Context.proc_InventoryInternalBackOrderItemsLoadAll().Where(x => IDSInternalBackOrderOrderList.Contains(x.InventoryInternalBackOrderID)).Select(x => x.ID).Count();
                    // Inventory Final Product Adding Order ----------
                    InventoryAndStoreDashboardInfoObj.FinalProductOrdersNo = IDSFinalProductAddingOrderList.Count();
                    InventoryAndStoreDashboardInfoObj.FinalProductOrdersItemsNo = FinalProductAddingOrderItemList.Count();
                    // InventoryAndStoreDashboardInfoObj.FinalProductOrdersItemsNo = _Context.proc_InventoryInternalBackOrderItemsLoadAll().Where(x => IDSFinalProductAddingOrderList.Contains(x.InventoryInternalBackOrderID)).Select(x => x.ID).Count();
                    // Inventory Final Product Adding Order ----------
                    InventoryAndStoreDashboardInfoObj.InternalTransferOrdersNo = IDSInternalTransferOrderList.Count();
                    InventoryAndStoreDashboardInfoObj.InternalTransferOrdersItemsNo = InternalTransferOrderItemList.Count();
                    // Inventory Matrial Release ----------
                    InventoryAndStoreDashboardInfoObj.MaterialReleasedOrdersNo = IDSMatrialReleaseOrderList.Count();
                    InventoryAndStoreDashboardInfoObj.MaterialReleasedOrdersItemsNo = MatrialReleaseOrderItemList.Count();

                    // Inventory Matrial Open and Closed Request ----------
                    InventoryAndStoreDashboardInfoObj.MaterialRequestOpenOrdersNo = IDSMatrialRequestOrderOpenList.Count();
                    InventoryAndStoreDashboardInfoObj.MaterialRequestClosedOrdersNo = IDSMatrialRequestOrderClosedList.Count();
                    InventoryAndStoreDashboardInfoObj.MaterialRequestOpenOrdersItemsNo = MatrialRequestOrderItemOpenList.Count();
                    InventoryAndStoreDashboardInfoObj.MaterialRequestClosedOrdersItemsNo = MatrialRequestOrderItemClosedList.Count();

                    int CountOFInventoryAdjustingReport = CheckInventoryReportListDB.Count();
                    InventoryAndStoreDashboardInfoObj.HaveInventoryAdjustingReport = CountOFInventoryAdjustingReport > 0 ? true : false;
                    InventoryAndStoreDashboardInfoObj.CountOFInventoryAdjustingReport = CountOFInventoryAdjustingReport;



                    Response.Data = InventoryAndStoreDashboardInfoObj;
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


        public InventoryStoreItemTotalPriceResponse GetInventoryStoreItemTotalPricesAndCosts([FromHeader] long InventoryStoreID, [FromHeader] DateTime? DateTo)
        {
            InventoryStoreItemTotalPriceResponse Response = new InventoryStoreItemTotalPriceResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    var InvStoreItemQuerable = _unitOfWork.InventoryStoreItems.FindAll(x => x.FinalBalance > 0 && x.RemainItemCosetEgp != null);
                    if (InventoryStoreID != 0)
                    {
                        InvStoreItemQuerable = InvStoreItemQuerable.Where(x => x.InventoryStoreId == InventoryStoreID);
                    }

                    if (DateTo == null)
                    {
                        DateTo = DateTime.Now;
                    }

                    InvStoreItemQuerable = InvStoreItemQuerable.Where(x => x.CreationDate <= DateTo);


                    decimal? TotalRemainItemCost = InvStoreItemQuerable.Sum(x => x.RemainItemCosetEgp ?? 0);

                    Response.Data.TotalRemainItemCostPerEGP = TotalRemainItemCost;
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

        public async Task<GetInventoryStoreKeeperDDLResponse> GetInventoryStoreKeeperDDL()
        {
            GetInventoryStoreKeeperDDLResponse response = new GetInventoryStoreKeeperDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetInventoryStorekeeperDDLList = new List<InventoryStorekeeperDDLData>();
                if (response.Result)
                {
                    var GetUsersDB = await _unitOfWork.Users.GetAllAsync();
                    if (GetUsersDB != null)
                    {
                        foreach (var GetUsersDLLOBJ in GetUsersDB)
                        {
                            var GetUsersDDLResponse = new InventoryStorekeeperDDLData();



                            GetUsersDDLResponse.ID = (int)GetUsersDLLOBJ.Id;

                            GetUsersDDLResponse.Name = GetUsersDLLOBJ.FirstName + GetUsersDLLOBJ.MiddleName + GetUsersDLLOBJ.LastName;


                            GetInventoryStorekeeperDDLList.Add(GetUsersDDLResponse);
                        }



                    }

                }


                response.InventoryStorekeeperDDLList = GetInventoryStorekeeperDDLList;
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

        public async Task<GetInventoryStoreLocationsDDLResponse> GetInventoryStoreLocationsDDL()
        {
            GetInventoryStoreLocationsDDLResponse response = new GetInventoryStoreLocationsDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();



            try
            {
                var GetInventoryStoreLocationsDDLList = new List<InventoryStoreLocationDDLData>();

                if (response.Result)
                {

                    var GetInventoryStoreLocationsDB = await _unitOfWork.InventoryStoreLocations.GetAllAsync();

                    if (GetInventoryStoreLocationsDB != null)
                    {

                        foreach (var GetInventoryStoreLocationsOBJ in GetInventoryStoreLocationsDB)
                        {
                            var GetInventoryStoreLocationsResponse = new InventoryStoreLocationDDLData();



                            GetInventoryStoreLocationsResponse.ID = (int)GetInventoryStoreLocationsOBJ.Id;

                            GetInventoryStoreLocationsResponse.Name = GetInventoryStoreLocationsOBJ.Location;


                            GetInventoryStoreLocationsDDLList.Add(GetInventoryStoreLocationsResponse);
                        }



                    }

                }


                response.InventoryStoreLocationDDLList = GetInventoryStoreLocationsDDLList;
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

        public async Task<GetBranchProductResponse> GetBranchProduct()
        {
            GetBranchProductResponse response = new GetBranchProductResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetBranchProductList = new List<BranchProductData>();


                if (response.Result)
                {
                    if (response.Result)
                    {
                        var BranchProductDB = await _unitOfWork.VBranchProducts.GetAllAsync();
                        if (BranchProductDB != null && BranchProductDB.Count() > 0)
                        {

                            foreach (var BranchProductDBOBJ in BranchProductDB)
                            {
                                var BranchProductResponse = new BranchProductData();

                                BranchProductResponse.ID = (int)BranchProductDBOBJ.Id;

                                BranchProductResponse.BranchID = BranchProductDBOBJ.BranchId;

                                BranchProductResponse.ProductID = (int)BranchProductDBOBJ.ProductId;

                                BranchProductResponse.BranchName = BranchProductDBOBJ.BranchName;

                                BranchProductResponse.ProductName = BranchProductDBOBJ.ProductName;

                                BranchProductResponse.Active = BranchProductDBOBJ.Active;

                                GetBranchProductList.Add(BranchProductResponse);
                            }
                        }

                    }

                }
                response.BranchProductList = GetBranchProductList;
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

        public async Task<SelectDDLResponse> GetInventoryItemListDDL([FromHeader] int InventoryItemCategoryId, [FromHeader] int CurrentPage, [FromHeader] int NumberOfItemsPerPage)
        {
            SelectDDLResponse response = new SelectDDLResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                long UserId = Validation.userID;
                var checkUserRole_IntenalTicket = Common.CheckUserRole(UserId, 169, _Context); //169 Internal Ticket in category Type 1
                var items = _unitOfWork.InventoryItems.FindAllQueryable(a => true, includes: new[] { "InventoryItemCategory" });
                var CategoryTypeIDsList = new List<int>();
                if (checkUserRole_IntenalTicket)//169 Internal Ticket in category Type 1
                {
                    CategoryTypeIDsList.Add(1);
                }
                items = items.Where(x => CategoryTypeIDsList.Contains(x.InventoryItemCategory.CategoryTypeId ?? 0) || x.InventoryItemCategory.CategoryTypeId == null);
                if (InventoryItemCategoryId != 0)
                {
                    items = items.Where(a => a.InventoryItemCategoryId == InventoryItemCategoryId).AsQueryable();
                }
                var itemsList = await PagedList<InventoryItem>.CreateAsync(items, CurrentPage, NumberOfItemsPerPage);
                response.DDLList = itemsList.Select(a => new SelectDDL() { ID = a.Id, Name = a.Name }).ToList();

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

        public BaseResponseWithId<long> DeleteInventoryStoreKeeper(AddInventoryStoreData Request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request.ID != 0)
                    {

                        // Update
                        var InventoryStoreKeeperDelete = _unitOfWork.InventoryStoreKeepers.GetById(Request.ID);
                        if (InventoryStoreKeeperDelete != null)
                        {
                            Response.ID = InventoryStoreKeeperDelete.Id;
                            _unitOfWork.InventoryStoreKeepers.Delete(InventoryStoreKeeperDelete);
                            _unitOfWork.Complete();
                        }


                    }

                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "This Inventory Store Keeper Doesn't Exist!!";
                        Response.Errors.Add(error);
                    }


                }
                ;






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

        public string Arabic1256ToUtf8(string data)
        {
            var latin = Encoding.GetEncoding("UTF-8");
            var bytes = latin.GetBytes(data); // get the bytes for your ANSI string

            //var arabic = Encoding.GetEncoding("UTF-8"); // decode it using the correct encoding
            return latin.GetString(bytes);
        }
        public BaseMessageResponse InventoryStoreItemReportWithoutPrices([FromHeader] string FileExtension)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var GetInventoryList = new List<InventoryStoreVM>();
                if (Response.Result)
                {
                    var InventoryListFromView = _unitOfWork.VInventoryStoreItems.FindAll(x => x.FinalBalance > 0).OrderBy(x => x.InventoryItemName).ToList();
                    var locations = _unitOfWork.InventoryStoreLocations.FindAll(a => InventoryListFromView.Select(x => x.InvenoryStoreLocationId).Contains(a.Id));
                    foreach (var item in InventoryListFromView)
                    {
                        var InventoryStoreItemDB = new InventoryStoreVM();

                        InventoryStoreItemDB.InventoryStoreName = Arabic1256ToUtf8(item.InventoryStoreName);
                        InventoryStoreItemDB.InvenoryStoreLocationName = item.InvenoryStoreLocationId != null ? locations.Where(a => a.Id == item.InvenoryStoreLocationId).FirstOrDefault()?.Location : "N/A";
                        InventoryStoreItemDB.InventoryItemName = item.InventoryItemName;
                        InventoryStoreItemDB.Code = item.Code;
                        InventoryStoreItemDB.CreationDate = item.CreationDate != null ? item.CreationDate.ToShortDateString() : "N/A";
                        InventoryStoreItemDB.OperationType = item.OperationType;
                        InventoryStoreItemDB.ExpDate = item.ExpDate != null ? item.ExpDate.ToString().Split(' ')[0] : "N/A";
                        InventoryStoreItemDB.ItemSerial = item.ItemSerial != null ? item.ItemSerial.ToString() : "N/A"; ;
                        InventoryStoreItemDB.FinalBalance = item.FinalBalance != null ? string.Format("{0:0.00}", item.FinalBalance) : "N/A";
                        InventoryStoreItemDB.AddingFromPOId = item.AddingFromPoid != null ? item.AddingFromPoid.ToString() : "N/A";
                        InventoryStoreItemDB.POInvoiceId = item.PoinvoiceId != null ? item.PoinvoiceId.ToString() : "N/A";
                        GetInventoryList.Add(InventoryStoreItemDB);
                    }
                    var dt = new System.Data.DataTable("Grid");
                    dt.Columns.AddRange(new DataColumn[11] {
                                                     new DataColumn("Item Code"),
                                                     new DataColumn("Item Name"),
                                                     new DataColumn("C. Date") ,
                                                     new DataColumn("Op. Type"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("R. Balance"),
                                                     new DataColumn("Store Name"),
                                                     new DataColumn("Location"),
                                                     new DataColumn("PO ID"),
                                                     new DataColumn("Inv. ID"),


                    });
                    var InventoryStoreItemList = GetInventoryList;
                    if (InventoryStoreItemList != null)
                    {
                        foreach (var item in InventoryStoreItemList)
                        {
                            dt.Rows.Add(
                                    item.Code,
                                    item.InventoryItemName,
                                    item.CreationDate,
                                    item.OperationType,
                                    item.ExpDate,
                                    item.ItemSerial,
                                    item.FinalBalance != null && item.FinalBalance != "" ? Decimal.Round(Decimal.Parse(item.FinalBalance), 1).ToString() : "",
                                    item.InventoryStoreName,
                                    item.InvenoryStoreLocationName,
                                    item.AddingFromPOId,
                                    item.POInvoiceId
                                );
                        }

                    }
                    if (FileExtension != null && FileExtension == "xml")
                    {
                        using (ExcelPackage packge = new ExcelPackage())
                        {
                            //Create the worksheet
                            ExcelWorksheet ws = packge.Workbook.Worksheets.Add("InventoryStoreItemList");
                            ws.TabColor = System.Drawing.Color.Red;
                            ws.Columns.BestFit = true;


                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells["A1:O1"])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 129, 189)); //Set color to dark blue
                                range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                            }



                            using (var package = new ExcelPackage())
                            {
                                var CompanyName = validation.CompanyName.ToLower();

                                string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.xlsx";
                                string PathsTR = "/Attachments/" + CompanyName + "/";
                                string Filepath = _host.WebRootPath + "/" + PathsTR;
                                string p_strPath = Filepath + "/" + FullFileName;


                                var workSheet = package.Workbook.Worksheets.Add("sheet");
                                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);

                                if (!System.IO.File.Exists(p_strPath))
                                {
                                    var objFileStrm = System.IO.File.Create(p_strPath);
                                    objFileStrm.Close();
                                }
                                package.Save();
                                File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                                package.Dispose();

                                Response.Message = Globals.baseURL + PathsTR + FullFileName;


                            }


                        }


                    }
                    else
                    {

                        //Start PDF Service

                        MemoryStream ms = new MemoryStream();

                        //Size of page
                        Document document = new Document(PageSize.A4.Rotate());


                        PdfWriter pw = PdfWriter.GetInstance(document, ms);

                        //Call the footer Function

                        pw.PageEvent = new HeaderFooter();

                        document.Open();

                        //Handle fonts and Sizes +  Attachments images logos 

                        iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);

                        document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                        //document.SetMargins(0, 0, 20, 20);
                        BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);

                        string path = _host.WebRootPath + "/Attachments";

                        if (validation.CompanyName == "marinaplt")
                        {
                            string PDFp_strPath = Path.Combine(path, "logoMarina.png");

                            if (File.Exists(PDFp_strPath))
                            {
                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                                jpg.SetAbsolutePosition(60f, 550f);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;


                            Chunk cc = new Chunk("Inventory Store Item Report".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);

                        }
                        else if (validation.CompanyName == "piaroma")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            if (File.Exists(Piaroma_p_strPath))
                            {
                                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                //document.Add(logo);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Inventory Store Item Report".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }
                        else if (validation.CompanyName == "Garastest")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            if (File.Exists(Piaroma_p_strPath))
                            {
                                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                //document.Add(logo);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Inventory Store Item Report", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);

                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }





                        PdfPTable table = new PdfPTable(dt.Columns.Count);







                        //table Width
                        table.WidthPercentage = 100;

                        //Define Sizes of Cloumns

                        table.SetTotalWidth(new float[] { 35, 160, 45, 38, 45, 42, 35, 35, 35, 35, 35 });


                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                            PdfPCell cell = new PdfPCell();
                            cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));

                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                            cell.PaddingBottom = 5;
                            table.AddCell(cell);


                        }


                        //writing table Data  
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                //table.AddCell(dt.Rows[i][j].ToString());
                                PdfPCell cell = new PdfPCell();
                                cell.ArabicOptions = 1;
                                if (j <= 5)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                }
                                else if (j >= 9)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }
                                else
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }

                                if (cell.ArabicOptions == 1)
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                                }
                                else
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                                }

                                cell.Phrase = new Phrase(1, dt.Rows[i][j].ToString(), font);
                                table.AddCell(cell);
                            }

                        }



                        document.Add(table);


                        document.Close();
                        byte[] result = ms.ToArray();
                        ms = new MemoryStream();
                        ms.Write(result, 0, result.Length);
                        ms.Position = 0;


                        var CompanyName = validation.CompanyName.ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        //Check if directory exist
                        string Filepath = _host.WebRootPath + "/" + PathsTR;
                        string p_strPath = Filepath + "/" + FullFileName;
                        if (!System.IO.File.Exists(p_strPath))
                        {
                            var objFileStrm = System.IO.File.Create(p_strPath);
                            objFileStrm.Close();
                        }
                        File.WriteAllBytes(p_strPath, result);

                        Response.Message = Globals.baseURL + PathsTR + FullFileName;

                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<decimal> CurrencyConverterAsync(string from, string to, decimal amount)
        {
            decimal result = 0;
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(BaseCurrencyConverterApiAddress + CurrencyConvertorAddress + "&from=" + from + "&to=" + to + "&amount=" + amount),
                };
                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    dynamic d = new { value1 = to };

                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    var ResponseJsonObject = JsonConvert.DeserializeObject<CurrencyConverter>(body);
                    var preresult = ResponseJsonObject.result;
                    result = decimal.Parse(preresult.ToString());
                    return result;
                }
            }
            catch (Exception ex)
            {

                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                return 0;
            }
            return result;
        }

        public BaseMessageResponse InventoryStoreItemReportWithProfitCalc([FromHeader] decimal Profit, [FromHeader] string FileExtension)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Profit == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "Please Insert a profit Percentage.";
                    Response.Errors.Add(error);
                    return Response;
                }


                var GetInventoryList = new List<InventoryStoreVM>();
                var ForSumObj = new InventoryStoreVM();

                ForSumObj.InventoryStoreName = "";
                ForSumObj.InvenoryStoreLocationName = "";
                ForSumObj.InventoryItemName = "";
                ForSumObj.Code = "";
                ForSumObj.CreationDate = "";
                ForSumObj.OperationType = "";
                ForSumObj.ExpDate = "";
                ForSumObj.ItemSerial = "";
                ForSumObj.FinalBalance = "";
                ForSumObj.AddingFromPOId = "";
                ForSumObj.POInvoiceId = "";
                ForSumObj.POInvoiceTotalPrice = "";
                ForSumObj.POInvoiceTotalCost = "";
                ForSumObj.CurrencyName = "";
                ForSumObj.RateToEGP = "";
                //ForSumObj.RemainBalancePricewithMainCu = 0;
                //ForSumObj.RemainBalanceCostwithMainCu = 0;
                ForSumObj.RemainBalancePricewithEgp = 0;
                ForSumObj.RemainBalanceCostwithEgp = 0;


                GetInventoryList.Add(ForSumObj);

                if (Response.Result)
                {
                    var OperatingType = new List<string> { "Add new", "Add New Matrial", "Final Product", "Internal Back Order", "Opening Balance", "Transfer Order (Received)", "Return From Release Order (Material Req. Type: Row", "Semi-Final Product" };






                    var InventoryListFromView = _unitOfWork.VInventoryStoreItems.FindAll(x => OperatingType.Contains(x.OperationType) && x.FinalBalance > 0).OrderBy(x => x.InventoryItemName).ToList();



                    var ProfitPerc = Profit / 100;
                    var locations = _unitOfWork.InventoryStoreLocations.FindAll(a => InventoryListFromView.Select(x => x.InvenoryStoreLocationId).Contains(a.Id));
                    var curr = _unitOfWork.Currencies.FindAll(a => InventoryListFromView.Select(a => a.CurrencyId).Contains(a.Id));
                    foreach (var item in InventoryListFromView)
                    {
                        var InventoryStoreItemDB = new InventoryStoreVM();

                        InventoryStoreItemDB.InventoryStoreName = Arabic1256ToUtf8(item.InventoryStoreName);
                        InventoryStoreItemDB.InvenoryStoreLocationName = item.InvenoryStoreLocationId != null ? locations.Where(a => a.Id == item.InvenoryStoreLocationId).FirstOrDefault()?.Location : "N/A";
                        InventoryStoreItemDB.InventoryItemName = item.InventoryItemName;
                        InventoryStoreItemDB.Code = item.Code;
                        InventoryStoreItemDB.CreationDate = item.CreationDate != null ? item.CreationDate.ToShortDateString() : "N/A";
                        InventoryStoreItemDB.OperationType = item.OperationType;
                        InventoryStoreItemDB.ExpDate = item.ExpDate != null ? item.ExpDate.ToString().Split(' ')[0] : "N/A";
                        InventoryStoreItemDB.ItemSerial = item.ItemSerial != null ? item.ItemSerial.ToString() : "N/A"; ;
                        InventoryStoreItemDB.FinalBalance = item.FinalBalance != null ? item.FinalBalance.ToString() : "";
                        InventoryStoreItemDB.AddingFromPOId = item.AddingFromPoid != null ? item.AddingFromPoid.ToString() : "N/A";
                        InventoryStoreItemDB.POInvoiceId = item.PoinvoiceId != null ? item.PoinvoiceId.ToString() : "N/A";
                        InventoryStoreItemDB.POInvoiceTotalPrice = item.PoinvoiceTotalPrice != null ? item.PoinvoiceTotalPrice.ToString() : "N/A";
                        InventoryStoreItemDB.POInvoiceTotalCost = item.PoinvoiceTotalCost != null ? item.PoinvoiceTotalCost.ToString() : "N/A";
                        InventoryStoreItemDB.CurrencyName = item.CurrencyId != null ? curr.Where(a => a.Id == item.CurrencyId).FirstOrDefault()?.Name.ToString() : " ";
                        InventoryStoreItemDB.RateToEGP = item.RateToEgp != null ? item.RateToEgp.ToString() : "N/A";
                        //InventoryStoreItemDB.RemainBalancePricewithMainCu = item.POInvoiceTotalPrice != null && item.finalBalance != null ? item.POInvoiceTotalPrice * item.finalBalance : 0;
                        //InventoryStoreItemDB.RemainBalancePricewithMainCu = item.POInvoiceTotalPrice != null && item.finalBalance != null ? item.POInvoiceTotalPrice * item.finalBalance : 0;
                        InventoryStoreItemDB.RemainBalanceCostwithMainCu = item.PoinvoiceTotalCost != null && item.FinalBalance != null ? item.PoinvoiceTotalCost * item.FinalBalance : 0;
                        InventoryStoreItemDB.RemainBalancePricewithEgp = item.PoinvoiceTotalCostEgp != null ? item.PoinvoiceTotalCostEgp : 0; // Waiting + Sum
                        InventoryStoreItemDB.RemainBalanceCostwithEgp = item.PoinvoiceTotalCost != null && item.FinalBalance != null && item.RateToEgp != null ? item.PoinvoiceTotalCost * item.FinalBalance * item.RateToEgp : 0;  // Waiting + Sum
                        InventoryStoreItemDB.ProfitValue = InventoryStoreItemDB.RemainBalancePricewithEgp != null ? InventoryStoreItemDB.RemainBalancePricewithEgp * ProfitPerc : 0;
                        InventoryStoreItemDB.Unit = item.RequestionUomshortName != null ? item.RequestionUomshortName : "N/A";
                        InventoryStoreItemDB.FinalPRrofite = InventoryStoreItemDB.ProfitValue + InventoryStoreItemDB.RemainBalancePricewithEgp;
                        if (InventoryStoreItemDB.CurrencyName != null && InventoryStoreItemDB.CurrencyName != " ")
                        {
                            InventoryStoreItemDB.FinalUnitPriceEGPIncludingCost = CurrencyConverterAsync(InventoryStoreItemDB.CurrencyName, "EGP", Decimal.Parse(InventoryStoreItemDB.POInvoiceTotalCost)).Result;

                        }

                        if (InventoryStoreItemDB.FinalUnitPriceEGPIncludingCost != null)
                        {
                            InventoryStoreItemDB.FinalUnitPriceEGPIncludingCostWithProfit = InventoryStoreItemDB.FinalUnitPriceEGPIncludingCost * (ProfitPerc + 1);

                        }



                        GetInventoryList.Add(InventoryStoreItemDB);
                    }

                    //var SumRemainBalancePricewithMainCuVAR = GetInventoryList.Sum(x => x.RemainBalancePricewithMainCu);
                    //var SumRemainBalanceCostwithMainCuVAR = GetInventoryList.Sum(x => x.RemainBalanceCostwithMainCu);
                    var SumRemainBalancePricewithEgpVAR = GetInventoryList.Sum(x => x.RemainBalancePricewithEgp);
                    var SumRemainBalanceCostwithEgpVAR = GetInventoryList.Sum(x => x.RemainBalanceCostwithEgp);





                    //SumRemainBalancePricewithMainCuVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalancePricewithMainCuVAR)), 1);
                    //SumRemainBalanceCostwithMainCuVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalanceCostwithMainCuVAR)), 1);
                    //SumRemainBalancePricewithEgpVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalancePricewithEgpVAR)), 1);
                    //SumRemainBalanceCostwithEgpVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalanceCostwithEgpVAR)), 1);




                    //GetInventoryList[0].RemainBalancePricewithMainCu = SumRemainBalancePricewithMainCuVAR;
                    //GetInventoryList[0].RemainBalanceCostwithMainCu = SumRemainBalanceCostwithMainCuVAR;
                    GetInventoryList[0].RemainBalancePricewithEgp = SumRemainBalancePricewithEgpVAR;
                    GetInventoryList[0].RemainBalanceCostwithEgp = SumRemainBalanceCostwithEgpVAR;

                    //var InventorySUM = new SumInventoryStoreVM();

                    //InventorySUM.SumRemainBalancePricewithMainCu = SumRemainBalancePricewithMainCuVAR;
                    //InventorySUM.SumRemainBalanceCostwithMainCu = SumRemainBalanceCostwithMainCuVAR;
                    //InventorySUM.SumRemainBalancePricewithEgp = SumRemainBalancePricewithEgpVAR;
                    //InventorySUM.SumRemainBalanceCostwithEgp = SumRemainBalanceCostwithEgpVAR;



                    var dt2 = new System.Data.DataTable("Grid2");










                    dt2.Columns.AddRange(new DataColumn[12] { new DataColumn("Item Name"),
                        new DataColumn("Item Code"),
                                                     new DataColumn("Store Name"),
                                                     new DataColumn("Store Location"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("Remain Balance"),
                                                     new DataColumn("Unit"),
                                                     //new DataColumn("U. Price"),
                                                      new DataColumn("Final U. Price (EGP)"),
                                              
                                                 
                                                     //new DataColumn("T. Final Price (EGP)"),
                                                     new DataColumn("Unit Profit (EGP)"),
                                                     new DataColumn("U. Price + Profit"),
                                                     new DataColumn("U. Price + Profit with (Cu. Rate)"),

                    });

                    //Second List to pass it to PDF

                    var InventoryStoreItemList2 = GetInventoryList;
                    if (InventoryStoreItemList2 != null)
                    {
                        foreach (var item in InventoryStoreItemList2)
                        {
                            dt2.Rows.Add(
                                    item.InventoryItemName,
                                    item.Code,
                                    item.InventoryStoreName,
                                    item.InvenoryStoreLocationName,
                                    item.ExpDate,
                                    item.ItemSerial,
                                    item.FinalBalance != null && item.FinalBalance != "" ? Decimal.Round(Decimal.Parse(item.FinalBalance), 1).ToString() : "",
                                    item.Unit,
                                     item.RemainBalancePricewithEgp != null ? Decimal.Round(decimal.Parse(string.Format("{0:n}", item.RemainBalancePricewithEgp)), 1) : 0,

                                    //item.RemainBalancePricewithMainCu != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalancePricewithMainCu)), 1) : 0,
                                    //item.RemainBalanceCostwithMainCu != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalanceCostwithMainCu)), 1) : 0,
                                    //item.RemainBalanceCostwithEgp != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalanceCostwithEgp)), 1) : 0,
                                    item.ProfitValue != null ? Decimal.Round(decimal.Parse(string.Format("{0:n}", item.ProfitValue)), 1) : 0,
                                    item.FinalPRrofite != null ? Decimal.Round(decimal.Parse(string.Format("{0:n}", item.FinalPRrofite)), 1) : 0,
                                    item.FinalUnitPriceEGPIncludingCostWithProfit != null ? Decimal.Round(decimal.Parse(string.Format("{0:n}", item.FinalUnitPriceEGPIncludingCostWithProfit)), 1) : 0



                                );
                        }

                    }


                    if (FileExtension != null && FileExtension == "xml")
                    {
                        using (ExcelPackage packge = new ExcelPackage())
                        {
                            //Create the worksheet
                            ExcelWorksheet ws = packge.Workbook.Worksheets.Add("InventoryStoreItemList");
                            ws.TabColor = Color.Red;
                            ws.Columns.BestFit = true;


                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells["A1:O1"])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                                range.Style.Font.Color.SetColor(Color.White);
                            }



                            using (var package = new ExcelPackage())
                            {
                                var CompanyName = validation.CompanyName.ToLower();

                                string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.xlsx";
                                string PathsTR = "/Attachments/" + CompanyName + "/";
                                string Filepath = _host.WebRootPath + "/" + PathsTR;
                                string p_strPath = Filepath + "/" + FullFileName;
                                var workSheet = package.Workbook.Worksheets.Add("sheet");
                                if (!System.IO.File.Exists(p_strPath))
                                {
                                    var objFileStrm = System.IO.File.Create(p_strPath);
                                    objFileStrm.Close();
                                }
                                package.Save();
                                File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                                package.Dispose();
                                Response.Message = Globals.baseURL + PathsTR + FullFileName;


                            }


                        }


                    }
                    else
                    {

                        //Start PDF Service

                        MemoryStream ms = new MemoryStream();

                        //Size of page
                        Document document = new Document(PageSize.A4.Rotate());


                        PdfWriter pw = PdfWriter.GetInstance(document, ms);

                        //Call the footer Function

                        pw.PageEvent = new HeaderFooter();

                        document.Open();

                        //Handle fonts and Sizes +  Attachments images logos 

                        iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);

                        document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                        //document.SetMargins(0, 0, 20, 20);
                        BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);

                        string path = _host.WebRootPath + "/Attachments";

                        if (validation.CompanyName == "marinaplt")
                        {
                            string PDFp_strPath = Path.Combine(path, "logoMarina.png");

                            if (File.Exists(PDFp_strPath))
                            {
                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                                jpg.SetAbsolutePosition(60f, 550f);
                                //document.Add(logo);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;


                            Chunk cc = new Chunk("Inventory Store Item Report".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);

                        }
                        else if (validation.CompanyName == "piaroma")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            if (File.Exists(Piaroma_p_strPath))
                            {
                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                //document.Add(logo);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            iTextSharp.text.Font fntHead2 = new iTextSharp.text.Font(bf, 11, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -30;
                            //prgHeading.SpacingAfter = 10;

                            Chunk cc = new Chunk("Inventory Store Item Report".ToUpper() + " ", fntHead);

                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 0, 0, 20);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);



                            Paragraph prgHeading2 = new Paragraph();
                            prgHeading2.Alignment = Element.ALIGN_RIGHT;
                            //prgHeading2.SpacingBefore = -10;
                            prgHeading2.SpacingAfter = 20;

                            Chunk cc2 = new Chunk("Prices Including Profit" + " ", fntHead2);
                            cc2.SetBackground(new BaseColor(4, 189, 189), 328, 9, 0, 10);

                            prgHeading2.Add(cc2);

                            document.Add(prgHeading2);


                        }
                        else if (validation.CompanyName == "Garastest")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            if (File.Exists(Piaroma_p_strPath))
                            {
                                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                //document.Add(logo);
                                document.Add(jpg);
                            }
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Inventory Store Item Report", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);

                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }




                        //Adding paragraph for report generated by  
                        Paragraph prgGeneratedBY = new Paragraph();
                        BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                        prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                        document.Add(prgGeneratedBY);



                        //Adding  PdfPTable  
                        PdfPTable table = new PdfPTable(dt2.Columns.Count);



                        //table Width
                        table.WidthPercentage = 100;

                        //Define Sizes of Cloumns

                        table.SetTotalWidth(new float[] { 140, 35, 45, 38, 45, 42, 35, 25, 40, 40, 40, 60 });


                        for (int i = 0; i < dt2.Columns.Count; i++)
                        {
                            string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                            PdfPCell cell = new PdfPCell();
                            cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));

                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                            cell.PaddingBottom = 5;
                            table.AddCell(cell);


                        }


                        //writing table Data  
                        for (int i = 0; i < dt2.Rows.Count; i++)
                        {
                            for (int j = 0; j < dt2.Columns.Count; j++)
                            {
                                PdfPCell cell = new PdfPCell();
                                cell.ArabicOptions = 1;
                                if (j <= 5)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                }
                                if (j == 7)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                }
                                else if (j >= 9)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }
                                else
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }

                                if (cell.ArabicOptions == 1)
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                                }
                                else
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                                }

                                if (i == 0)
                                {
                                    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                    cell.BackgroundColor = (new BaseColor(4, 189, 189));
                                    cell.Phrase = new Phrase(1, dt2.Rows[0].ToString(), new iTextSharp.text.Font(bf, 60, 1, iTextSharp.text.BaseColor.WHITE));
                                    cell.UseVariableBorders = true;
                                    cell.BorderColorBottom = BaseColor.BLACK;
                                    cell.BorderColorRight = new BaseColor(4, 189, 189);
                                    cell.BorderColorLeft = new BaseColor(4, 189, 189);
                                    cell.BorderColorTop = BaseColor.BLACK;
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                    dt2.Rows[0].AcceptChanges();
                                }

                                cell.Phrase = new Phrase(1, dt2.Rows[i][j].ToString(), font);

                                table.AddCell(cell);
                            }

                        }



                        document.Add(table);


                        document.Close();
                        byte[] result = ms.ToArray();
                        ms = new MemoryStream();
                        ms.Write(result, 0, result.Length);
                        ms.Position = 0;


                        var CompanyName = validation.CompanyName.ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        string Filepath = _host.WebRootPath + "/" + PathsTR;
                        string p_strPath = Filepath + "/" + FullFileName;
                        if (!System.IO.File.Exists(p_strPath))
                        {
                            var objFileStrm = System.IO.File.Create(p_strPath);
                            objFileStrm.Close();
                        }

                        File.WriteAllBytes(p_strPath, result);

                        Response.Message = Globals.baseURL + PathsTR + FullFileName;

                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }

        }

        public async Task<BaseMessageResponse> HoldItemsExcel()
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {



                    var InvStoreItemGrouping = (await _unitOfWork.VInventoryStoreItems.FindAllAsync(x => x.HoldQty > 0)).GroupBy(x => new { x.InventoryItemName, x.Code, x.ItemSerial, x.ExpDate, x.InventoryItemId }).OrderBy(x => x.Key.InventoryItemName).Select(x => new InventoryStoreItemHoldVM
                    {
                        ItemName = x.Key.InventoryItemName,
                        Code = x.Key.Code,
                        Serial = x.Key.ItemSerial,
                        ExpDate = x.Key.ExpDate.ToString(),
                        HoldQTY = x.Sum(y => y.HoldQty),
                        InventoryItemID = x.Key.InventoryItemId
                    }
                    ).ToList();

                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[11] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Item Name"),
                                                     new DataColumn("Code"),
                                                       new DataColumn("Batch / Serial"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Hold Qty"),
                                                     new DataColumn(" - "),
                                                     new DataColumn("in Material Req.#"),
                                                     new DataColumn("Hold M.Request Qty"),
                                                     new DataColumn("for Client"),
                                                     new DataColumn("for Project"),
                                                     new DataColumn("Material Request Comment")

                    });

                    var InventoryItemIDExist = new List<long>();
                    string Name = null;
                    bool Isrepeted = false;

                    int Counter = 0;
                    int EmployeeCounter = 0;
                    using (ExcelPackage packge = new ExcelPackage())
                    {
                        ExcelWorksheet ws = packge.Workbook.Worksheets.Add("Sheet1");
                        int StartCounter = 4;
                        int EndCounter = 0;



                        var InventoryItemIds = InvStoreItemGrouping.Select(a => a.InventoryItemID).ToList();
                        var ReqQuantitySum = (await _unitOfWork.VInventoryMatrialRequestItems.FindAllAsync(x => InventoryItemIds.Contains(x.InventoryItemId) && x.IsHold == true && x.ReqQuantity != x.RecivedQuantity)).Sum(x => x.ReqQuantity);

                        var HoldQuantitySum = InvStoreItemGrouping.Sum(x => x.HoldQTY);



                        dt.Rows.Add(

                        "",
                        "",
                        "",
                        "",
                        HoldQuantitySum,
                        "",
                        "",
                        ReqQuantitySum,
                        "",
                        "",
                        ""

                       );







                        foreach (var Item in InvStoreItemGrouping)
                        {




                            dt.Rows.Add(

                            Item.ItemName,
                            Item.Code,
                            Item.Serial,
                            Item.ExpDate,
                            Item.HoldQTY,
                            "",
                            "",
                            "",
                            "",
                            "",
                            ""

                           );
                            var CountName = InvStoreItemGrouping.Where(x => x.InventoryItemID == Item.InventoryItemID).Count();


                            var InventoryItemIDIFExist = InventoryItemIDExist.Contains(Item.InventoryItemID);

                            if (InventoryItemIDIFExist != true)
                            {
                                var InventoryMatrialRequesItemDB = (await _unitOfWork.VInventoryMatrialRequestItems.FindAllAsync(x => x.InventoryItemId == Item.InventoryItemID && x.IsHold == true && x.ReqQuantity != x.RecivedQuantity)).ToList();




                                EmployeeCounter = InventoryMatrialRequesItemDB.Count();
                                foreach (var MatrialRequestItem in InventoryMatrialRequesItemDB)
                                {



                                    InventoryItemIDExist.Add(MatrialRequestItem.InventoryItemId);




                                    dt.Rows.Add(

                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 MatrialRequestItem.InventoryMatrialRequestId,
                                 MatrialRequestItem.ReqQuantity,
                                 MatrialRequestItem.ClientName,
                                 MatrialRequestItem.ProjectName,
                                 MatrialRequestItem.Comments


                                 );

                                }






                                // Collapse  Counter Draw
                                EndCounter = StartCounter + (EmployeeCounter);
                                for (var i = StartCounter; i <= EndCounter - 1; i++)//10   //22
                                {
                                    ws.Row(i).OutlineLevel = 1;
                                    // ws.Row(i).Collapsed = true;
                                    Counter++;
                                }


                                StartCounter = EndCounter + 1;//3 //13 //25


                            }
                            else
                            {
                                StartCounter += 1;
                                Isrepeted = true;
                            }



                            //Create the worksheet



                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells[1, 1, 1, 11])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(4, 189, 189));

                                range.AutoFitColumns();
                            }


                            using (ExcelRange range = ws.Cells[1, 6, EndCounter - 1, 6])
                            {
                                //range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

                                range.AutoFitColumns();
                            }

                            using (ExcelRange range = ws.Cells[2, 1, 2, 11])
                            {

                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                            }
                            if (CountName == 1)
                            {

                                using (ExcelRange range = ws.Cells[EndCounter - 1, 1, EndCounter - 1, 11])
                                {

                                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                                }
                            }
                            else if (Isrepeted == true)
                            {
                                using (ExcelRange range = ws.Cells[EndCounter, 1, EndCounter, 11])
                                {

                                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                                }
                            }

                            ws.Protection.IsProtected = false;
                            ws.Protection.AllowSelectLockedCells = false;



                        }

                        var CompanyName = validation.CompanyName.ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "HoldItemsExcel.xlsx";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        string Filepath = _host.WebRootPath + "/" + PathsTR;
                        string p_strPath = Filepath + "/" + FullFileName;

                        ExcelRangeBase excelRangeBase = ws.Cells.LoadFromDataTable(dt, true);

                        if (!System.IO.File.Exists(p_strPath))
                        {
                            var objFileStrm = System.IO.File.Create(p_strPath);
                            objFileStrm.Close();
                        }

                        packge.Save();
                        File.WriteAllBytes(p_strPath, packge.GetAsByteArray());
                        packge.Dispose();

                        Response.Message = Globals.baseURL + PathsTR + FullFileName;


                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseMessageResponse InventoryStoreItemExcelsheet([FromHeader] string FileExtension)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var GetInventoryList = new List<InventoryStoreVM>();
                if (Response.Result)
                {
                    var InventoryListFromView = _unitOfWork.VInventoryStoreItems.FindAll(x => x.FinalBalance > 0).OrderBy(x => x.InventoryItemName).ToList();
                    var dt = new System.Data.DataTable("Grid");
                    var dt2 = new System.Data.DataTable("Grid2");

                    dt2.Columns.AddRange(new DataColumn[14] { new DataColumn("Item Name"),
                        new DataColumn("Item Code"),
                                                     new DataColumn("Store Name"),
                                                     new DataColumn("Location"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("R. Balance"),
                                                     new DataColumn("U. Price"),
                                                     new DataColumn("U. Cost"),

                                                     new DataColumn("Rate To EGP") ,
                                                     new DataColumn("T. Price (Cu)") ,
                                                     new DataColumn("T. Cost (Cu)"),
                                                     new DataColumn("T. Price (EGP)"),
                                                     new DataColumn("T. Cost (EGP)")

                    });
                    dt.Columns.AddRange(new DataColumn[19] { new DataColumn("Store Name"),
                                                     new DataColumn("Location"),
                                                     new DataColumn("Item Name"),
                                                     new DataColumn("Item Code"),
                                                     new DataColumn("C. Date") ,
                                                     new DataColumn("Op. Type"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("R. Balance"),
                                                     new DataColumn("PO ID"),
                                                     new DataColumn("Inv. ID"),
                                                     new DataColumn("U. Price"),
                                                     new DataColumn("U. Cost"),
                                                     new DataColumn("Cu."),
                                                     new DataColumn("Rate To EGP") ,
                                                     new DataColumn("T. Price (Cu)") ,
                                                     new DataColumn("T. Cost (Cu)"),
                                                     new DataColumn("T. Price (EGP)"),
                                                     new DataColumn("T. Cost (EGP)")

                    });
                    int Counter = 0;


                    var SumRemainBalancePricewithMainCuVAR = InventoryListFromView.Sum(x => (x.PoinvoiceTotalPrice ?? 0) * x.FinalBalance ?? 0);
                    var SumRemainBalanceCostwithMainCuVAR = InventoryListFromView.Sum(x => (x.PoinvoiceTotalCost ?? 0) * x.FinalBalance ?? 0);
                    var SumRemainBalancePricewithEgpVAR = InventoryListFromView.Sum(x => (x.PoinvoiceTotalPrice ?? 0) * x.FinalBalance ?? 0 * x.RateToEgp ?? 0);
                    var SumRemainBalanceCostwithEgpVAR = InventoryListFromView.Sum(x => (x.PoinvoiceTotalCost ?? 0) * x.FinalBalance ?? 0 * x.RateToEgp ?? 0);

                    var locations = _unitOfWork.InventoryStoreLocations.FindAll(a => InventoryListFromView.Select(x => x.InvenoryStoreLocationId).Contains(a.Id));
                    foreach (var item in InventoryListFromView)
                    {
                        string CurrencyName = "";

                        if (item.CurrencyId != null)
                        {
                            CurrencyName = Common.GetCurrencyName((int)item.CurrencyId, _Context);
                        }

                        //var InventoryStoreItemDB = new InventoryStoreVM();

                        //InventoryStoreItemDB.InventoryStoreName = Arabic1256ToUtf8(item.InventoryStoreName);
                        //InventoryStoreItemDB.InvenoryStoreLocationName = item.InvenoryStoreLocationID != null ? Common.GetInventoryLocationName((int)item.InvenoryStoreLocationID).ToString() : "N/A";
                        //InventoryStoreItemDB.InventoryItemName = item.InventoryItemName;
                        //InventoryStoreItemDB.Code = item.Code;
                        //InventoryStoreItemDB.CreationDate = item.CreationDate != null ? item.CreationDate.ToShortDateString() : "N/A";
                        //InventoryStoreItemDB.OperationType = item.OperationType;
                        //InventoryStoreItemDB.ExpDate = item.expDate != null ? item.expDate.ToString().Split(' ')[0] : "N/A";
                        //InventoryStoreItemDB.ItemSerial = item.itemSerial != null ? item.itemSerial.ToString() : "N/A";
                        //InventoryStoreItemDB.FinalBalance = item.finalBalance != null ? string.Format("{0:0.00}", item.finalBalance) : "N/A";
                        //InventoryStoreItemDB.AddingFromPOId = item.addingFromPOId != null ? item.addingFromPOId.ToString() : "N/A";
                        //InventoryStoreItemDB.POInvoiceId = item.POInvoiceId != null ? item.POInvoiceId.ToString() : "N/A";
                        //InventoryStoreItemDB.POInvoiceTotalPrice = item.POInvoiceTotalPrice != null ? item.POInvoiceTotalPrice.ToString() : "N/A";
                        //InventoryStoreItemDB.POInvoiceTotalCost = item.POInvoiceTotalCost != null ? item.POInvoiceTotalCost.ToString() : "N/A";
                        //InventoryStoreItemDB.CurrencyName = item.currencyId != null ? Common.GetCurrencyName((int)item.currencyId).ToString() : " ";
                        //InventoryStoreItemDB.RateToEGP = item.rateToEGP != null ? string.Format("{0:0.00}", item.rateToEGP) : "N/A";
                        //InventoryStoreItemDB.RemainBalancePricewithMainCu = item.POInvoiceTotalPrice != null && item.finalBalance != null ? item.POInvoiceTotalPrice * item.finalBalance : 0;
                        //InventoryStoreItemDB.RemainBalanceCostwithMainCu = item.POInvoiceTotalCost != null && item.finalBalance != null ? item.POInvoiceTotalCost * item.finalBalance : 0;
                        //InventoryStoreItemDB.RemainBalancePricewithEgp = item.POInvoiceTotalPrice != null && item.finalBalance != null && item.rateToEGP != null ? item.POInvoiceTotalPrice * item.finalBalance * item.rateToEGP : 0; // Waiting + Sum
                        //InventoryStoreItemDB.RemainBalanceCostwithEgp = item.POInvoiceTotalCost != null && item.finalBalance != null && item.rateToEGP != null ? item.POInvoiceTotalCost * item.finalBalance * item.rateToEGP : 0;  // Waiting + Sum


                        if (Counter == 0)
                        {
                            dt.Rows.Add(
                            "",
                            "",
                            "",
                            "",
                           "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            "",
                            SumRemainBalancePricewithMainCuVAR,
                            SumRemainBalanceCostwithMainCuVAR,
                            SumRemainBalancePricewithEgpVAR,
                            SumRemainBalanceCostwithEgpVAR);


                            //                                        dt2.Rows.Add(
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //  "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   "",
                            //   SumRemainBalancePricewithMainCuVAR,
                            //   SumRemainBalanceCostwithMainCuVAR,
                            //   SumRemainBalancePricewithEgpVAR,
                            //   SumRemainBalanceCostwithEgpVAR);
                        }




                        dt.Rows.Add(
                         item.InventoryStoreName != null ? item.InventoryStoreName : "N/A",
                         item.InvenoryStoreLocationId != null ? locations.Where(a => a.Id == (int)item.InvenoryStoreLocationId).FirstOrDefault()?.Location ?? "N/A" : "N/A",
                         item.InventoryItemName != null ? item.InventoryItemName : "N/A",
                         item.Code != null ? item.Code : "N/A",
                         item.CreationDate != null ? item.CreationDate.ToShortDateString() : "N/A",
                         item.OperationType != null ? item.OperationType : "N/A",
                         item.ExpDate != null ? item.ExpDate.ToString().Split(' ')[0] : "N/A",
                         item.ItemSerial != null ? item.ItemSerial.ToString() : "N/A",
                         item.FinalBalance != null ? Decimal.Round((decimal)item.FinalBalance, 2) : 0,
                         item.AddingFromPoid != null ? item.AddingFromPoid : 0,
                         item.PoinvoiceId != null ? item.PoinvoiceId.ToString() : "N/A",
                         item.PoinvoiceTotalPrice != null ? Decimal.Round(item.PoinvoiceTotalPrice ?? 0, 2) : 0, // Ucost
                         item.PoinvoiceTotalCost != null ? Decimal.Round(item.PoinvoiceTotalCost ?? 0, 2) : 0,
                         item.CurrencyId != null ? CurrencyName : " ",
                         item.RateToEgp != null ? (decimal)item.RateToEgp : 0,
                         item.PoinvoiceTotalPrice != null && item.FinalBalance != null ? Decimal.Round((decimal)item.PoinvoiceTotalPrice * (decimal)item.FinalBalance, 2) : 0,
                         item.PoinvoiceTotalCost != null && item.FinalBalance != null ? Decimal.Round((decimal)item.PoinvoiceTotalCost * (decimal)item.FinalBalance, 2) : 0,
                         item.PoinvoiceTotalPrice != null && item.FinalBalance != null && item.RateToEgp != null ? Decimal.Round((decimal)item.PoinvoiceTotalPrice * (decimal)item.FinalBalance * (decimal)item.RateToEgp, 2) : 0,
                         item.PoinvoiceTotalCost != null && item.FinalBalance != null && item.RateToEgp != null ? Decimal.Round((decimal)item.PoinvoiceTotalCost * (decimal)item.FinalBalance * (decimal)item.RateToEgp, 2) : 0


                     );









                        dt2.Rows.Add(



                     item.InventoryItemName != null ? item.InventoryItemName : "N/A",
                     item.Code != null ? item.Code : "N/A",
                     item.InventoryStoreName != null ? item.InventoryStoreName : "N/A",
                     item.InvenoryStoreLocationId != null ? locations.Where(a => a.Id == (int)item.InvenoryStoreLocationId).FirstOrDefault()?.Location ?? "N/A" : "N/A",
                     item.ExpDate != null ? item.ExpDate.ToString().Split(' ')[0] : "N/A",
                     item.ItemSerial != null ? item.ItemSerial.ToString() : "N/A",
                     item.FinalBalance != null ? Decimal.Round((decimal)item.FinalBalance, 2) : 0,
                     item.PoinvoiceTotalPrice != null ? Decimal.Round(item.PoinvoiceTotalPrice ?? 0, 2) : 0,
                     item.PoinvoiceTotalCost != null ? Decimal.Round(item.PoinvoiceTotalCost ?? 0, 2).ToString() + CurrencyName : "N/A", //UCost
                     item.RateToEgp != null ? (decimal)item.RateToEgp : 0,
                     item.PoinvoiceTotalPrice != null && item.FinalBalance != null ? Decimal.Round((decimal)item.PoinvoiceTotalPrice * (decimal)item.FinalBalance, 2) : 0,
                     item.PoinvoiceTotalCost != null && item.FinalBalance != null ? Decimal.Round((decimal)item.PoinvoiceTotalCost * (decimal)item.FinalBalance, 2) : 0,
                     item.PoinvoiceTotalPrice != null && item.FinalBalance != null && item.RateToEgp != null ? Decimal.Round((decimal)item.PoinvoiceTotalPrice * (decimal)item.FinalBalance * (decimal)item.RateToEgp, 2) : 0,
                     item.PoinvoiceTotalCost != null && item.FinalBalance != null && item.RateToEgp != null ? Decimal.Round((decimal)item.PoinvoiceTotalCost * (decimal)item.FinalBalance * (decimal)item.RateToEgp, 2) : 0


                            //item.InventoryItemName,
                            //item.Code,
                            //item.InventoryStoreName,
                            //item.InvenoryStoreLocationName,
                            //item.ExpDate,
                            //item.ItemSerial,
                            //item.FinalBalance != null && item.FinalBalance != "" ? Decimal.Round(Decimal.Parse(item.FinalBalance), 1).ToString() : "",
                            //item.POInvoiceTotalPrice,
                            //item.POInvoiceTotalCost + " " + item.CurrencyName,
                            //item.RateToEGP,
                            //item.RemainBalancePricewithMainCu != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalancePricewithMainCu)), 1) : 0,
                            //item.RemainBalanceCostwithMainCu != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalanceCostwithMainCu)), 1) : 0,
                            //item.RemainBalancePricewithEgp != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalancePricewithEgp)), 1) : 0,
                            //item.RemainBalanceCostwithEgp != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalanceCostwithEgp)), 1) : 0

                            );



                        Counter++;

                        //GetInventoryList.Add(InventoryStoreItemDB);
                    }



                    //SumRemainBalancePricewithMainCuVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalancePricewithMainCuVAR)), 1);
                    //SumRemainBalanceCostwithMainCuVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalanceCostwithMainCuVAR)), 1);
                    //SumRemainBalancePricewithEgpVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalancePricewithEgpVAR)), 1);
                    //SumRemainBalanceCostwithEgpVAR = Decimal.Round(decimal.Parse(String.Format("{0:n}", SumRemainBalanceCostwithEgpVAR)), 1);




                    //GetInventoryList[0].RemainBalancePricewithMainCu = SumRemainBalancePricewithMainCuVAR;
                    //GetInventoryList[0].RemainBalanceCostwithMainCu = SumRemainBalanceCostwithMainCuVAR;
                    //GetInventoryList[0].RemainBalancePricewithEgp = SumRemainBalancePricewithEgpVAR;
                    //GetInventoryList[0].RemainBalanceCostwithEgp = SumRemainBalanceCostwithEgpVAR;















                    ////Second List to pass it to PDF

                    //var InventoryStoreItemList2 = GetInventoryList;
                    //if (InventoryListFromView != null)
                    //{
                    //    foreach (var item in InventoryListFromView)
                    //    {
                    //        string CurrencyName = "";

                    //        if (item.currencyId != null)
                    //        {
                    //            CurrencyName = Common.GetCurrencyName((int)item.currencyId);
                    //        }



                    //        if (Counter == 0)
                    //        {
                    //            dt.Rows.Add(
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //      "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       "",
                    //       SumRemainBalancePricewithMainCuVAR,
                    //       SumRemainBalanceCostwithMainCuVAR,
                    //       SumRemainBalancePricewithEgpVAR,
                    //       SumRemainBalanceCostwithEgpVAR);
                    //        }



                    //        dt2.Rows.Add(



                    //     item.InventoryItemName != null ? item.InventoryItemName : "N/A",
                    //     item.Code != null ? item.Code : "N/A",
                    //     item.InventoryStoreName != null ? item.InventoryStoreName : "N/A",
                    //     item.InvenoryStoreLocationID != null ? Common.GetInventoryLocationName((int)item.InvenoryStoreLocationID).ToString() : "N/A",
                    //     item.expDate != null ? item.expDate.ToString().Split(' ')[0] : "N/A",
                    //     item.itemSerial != null ? item.itemSerial.ToString() : "N/A",
                    //     item.finalBalance != null ? Decimal.Round((decimal)item.finalBalance, 2) : 0,
                    //     item.POInvoiceTotalPrice != null ? Decimal.Round(item.POInvoiceTotalPrice ?? 0, 2) : 0,
                    //     item.POInvoiceTotalCost != null ? Decimal.Round(item.POInvoiceTotalCost ?? 0, 2).ToString() + CurrencyName : "N/A", //UCost
                    //     item.rateToEGP != null ? (decimal)item.rateToEGP : 0,
                    //     item.POInvoiceTotalPrice != null && item.finalBalance != null ? Decimal.Round((decimal)item.POInvoiceTotalPrice * (decimal)item.finalBalance, 2) : 0,
                    //     item.POInvoiceTotalCost != null && item.finalBalance != null ? Decimal.Round((decimal)item.POInvoiceTotalCost * (decimal)item.finalBalance, 2) : 0,
                    //     item.POInvoiceTotalPrice != null && item.finalBalance != null && item.rateToEGP != null ? Decimal.Round((decimal)item.POInvoiceTotalPrice * (decimal)item.finalBalance * (decimal)item.rateToEGP, 2) : 0,
                    //     item.POInvoiceTotalCost != null && item.finalBalance != null && item.rateToEGP != null ? Decimal.Round((decimal)item.POInvoiceTotalCost * (decimal)item.finalBalance * (decimal)item.rateToEGP, 2) : 0


                    //            //item.InventoryItemName,
                    //            //item.Code,
                    //            //item.InventoryStoreName,
                    //            //item.InvenoryStoreLocationName,
                    //            //item.ExpDate,
                    //            //item.ItemSerial,
                    //            //item.FinalBalance != null && item.FinalBalance != "" ? Decimal.Round(Decimal.Parse(item.FinalBalance), 1).ToString() : "",
                    //            //item.POInvoiceTotalPrice,
                    //            //item.POInvoiceTotalCost + " " + item.CurrencyName,
                    //            //item.RateToEGP,
                    //            //item.RemainBalancePricewithMainCu != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalancePricewithMainCu)), 1) : 0,
                    //            //item.RemainBalanceCostwithMainCu != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalanceCostwithMainCu)), 1) : 0,
                    //            //item.RemainBalancePricewithEgp != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalancePricewithEgp)), 1) : 0,
                    //            //item.RemainBalanceCostwithEgp != null ? Decimal.Round(decimal.Parse(String.Format("{0:n}", item.RemainBalanceCostwithEgp)), 1) : 0

                    //            );
                    //        //var x = item.RemainBalancePricewithMainCu != null && item.RemainBalancePricewithMainCu != 0 ? String.Format("{0:n}", item.RemainBalancePricewithMainCu) : "-";

                    //        Counter++;
                    //    }

                    //}


















                    var InvStoreItemGrouping = _unitOfWork.VInventoryStoreItems.FindAll(x => x.HoldQty > 0).GroupBy(x => new { x.InventoryItemName, x.Code, x.ItemSerial, x.ExpDate, x.InventoryItemId }).OrderBy(x => x.Key.InventoryItemName).Select(x => new InventoryStoreItemHoldVM
                    {
                        ItemName = x.Key.InventoryItemName,
                        Code = x.Key.Code,
                        Serial = x.Key.ItemSerial,
                        ExpDate = x.Key.ExpDate.ToString(),
                        HoldQTY = x.Sum(y => y.HoldQty),
                        InventoryItemID = x.Key.InventoryItemId
                    }
                    ).ToList();

                    var dtHoldITems = new System.Data.DataTable("Grid");

                    dtHoldITems.Columns.AddRange(new DataColumn[11] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Item Name"),
                                                     new DataColumn("Code"),
                                                       new DataColumn("Batch / Serial"),
                                                     new DataColumn("Exp. Date"),
                                                     new DataColumn("Hold Qty"),
                                                     new DataColumn(" - "),
                                                     new DataColumn("in Material Req.#"),
                                                     new DataColumn("Hold M.Request Qty"),
                                                     new DataColumn("for Client"),
                                                     new DataColumn("for Project"),
                                                     new DataColumn("Material Request Comment")

                    });

                    var InventoryItemIDExist = new List<long>();
                    string Name = null;
                    bool Isrepeted = false;

                    int CounterHoldItem = 0;
                    int EmployeeCounter = 0;
                    using (ExcelPackage packgeHoldItem = new ExcelPackage())
                    {



                        //Create the worksheet
                        ExcelWorksheet wsInv = packgeHoldItem.Workbook.Worksheets.Add("InventoryStoreItemList");
                        //wsInv.TabColor = Color.Red;
                        wsInv.Columns.BestFit = true;


                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        wsInv.Cells["A1"].LoadFromDataTable(dt, true);
                        //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                        //Format the header for column 1-3
                        using (ExcelRange range = wsInv.Cells["A1:O1"])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(4, 189, 189));
                            range.Style.Font.Color.SetColor(Color.White);
                        }







                        ExcelWorksheet ws = packgeHoldItem.Workbook.Worksheets.Add("HoldItems");
                        ws.TabColor = Color.Red;
                        ws.Columns.BestFit = true;





                        int StartCounter = 4;
                        int EndCounter = 0;



                        var InventoryItemIds = InvStoreItemGrouping.Select(a => a.InventoryItemID).ToList();
                        var ReqQuantitySum = _unitOfWork.VInventoryMatrialRequestItems.FindAll(x => InventoryItemIds.Contains(x.InventoryItemId) && x.IsHold == true && x.ReqQuantity != x.RecivedQuantity).Sum(x => x.ReqQuantity);

                        var HoldQuantitySum = InvStoreItemGrouping.Sum(x => x.HoldQTY);



                        dtHoldITems.Rows.Add(

                        "",
                        "",
                        "",
                        "",
                        HoldQuantitySum,
                        "",
                        "",
                        ReqQuantitySum,
                        "",
                        "",
                        ""

                       );







                        foreach (var Item in InvStoreItemGrouping)
                        {




                            dtHoldITems.Rows.Add(

                            Item.ItemName,
                            Item.Code,
                            Item.Serial,
                            Item.ExpDate,
                            Item.HoldQTY,
                            "",
                            "",
                            "",
                            "",
                            "",
                            ""

                           );
                            var CountName = InvStoreItemGrouping.Where(x => x.InventoryItemID == Item.InventoryItemID).Count();


                            var InventoryItemIDIFExist = InventoryItemIDExist.Contains(Item.InventoryItemID);

                            if (InventoryItemIDIFExist != true)
                            {
                                var InventoryMatrialRequesItemDB = _unitOfWork.VInventoryMatrialRequestItems.FindAll(x => x.InventoryItemId == Item.InventoryItemID && x.IsHold == true && x.ReqQuantity != x.RecivedQuantity).ToList();




                                EmployeeCounter = InventoryMatrialRequesItemDB.Count();
                                foreach (var MatrialRequestItem in InventoryMatrialRequesItemDB)
                                {



                                    InventoryItemIDExist.Add(MatrialRequestItem.InventoryItemId);




                                    dtHoldITems.Rows.Add(

                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 "",
                                 MatrialRequestItem.InventoryMatrialRequestId,
                                 MatrialRequestItem.ReqQuantity,
                                 MatrialRequestItem.ClientName,
                                 MatrialRequestItem.ProjectName,
                                 MatrialRequestItem.Comments


                                 );

                                }






                                // Collapse  Counter Draw
                                EndCounter = StartCounter + (EmployeeCounter);
                                for (var i = StartCounter; i <= EndCounter - 1; i++)//10   //22
                                {
                                    ws.Row(i).OutlineLevel = 1;
                                    // ws.Row(i).Collapsed = true;
                                    CounterHoldItem++;
                                }


                                StartCounter = EndCounter + 1;//3 //13 //25


                            }
                            else
                            {
                                StartCounter += 1;
                                Isrepeted = true;
                            }



                            //Create the worksheet



                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dtHoldITems, true);
                            //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells[1, 1, 1, 11])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(4, 189, 189));

                                range.AutoFitColumns();
                            }


                            using (ExcelRange range = ws.Cells[1, 6, EndCounter - 1, 6])
                            {
                                //range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);

                                range.AutoFitColumns();
                            }

                            using (ExcelRange range = ws.Cells[2, 1, 2, 11])
                            {

                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                            }
                            if (CountName == 1)
                            {

                                using (ExcelRange range = ws.Cells[EndCounter - 1, 1, EndCounter - 1, 11])
                                {

                                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                                }
                            }
                            else if (Isrepeted == true)
                            {
                                using (ExcelRange range = ws.Cells[EndCounter, 1, EndCounter, 11])
                                {

                                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Medium;
                                }
                            }

                            ws.Protection.IsProtected = false;
                            ws.Protection.AllowSelectLockedCells = false;



                        }








                        if (FileExtension != null && FileExtension == "xml")
                        {




                            var CompanyName = validation.CompanyName.ToLower();

                            string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.xlsx";
                            string PathsTR = "/Attachments/" + CompanyName + "/";
                            string Filepath = _host.WebRootPath + "/" + PathsTR;
                            string p_strPath = Filepath + "/" + FullFileName;
                            if (!System.IO.File.Exists(p_strPath))
                            {
                                var objFileStrm = System.IO.File.Create(p_strPath);
                                objFileStrm.Close();
                            }

                            packgeHoldItem.Save();
                            File.WriteAllBytes(p_strPath, packgeHoldItem.GetAsByteArray());
                            packgeHoldItem.Dispose();

                            Response.Message = Globals.baseURL + PathsTR + FullFileName;








                        }
                        else
                        {

                            //Start PDF Service

                            MemoryStream ms = new MemoryStream();

                            //Size of page
                            Document document = new Document(PageSize.A4.Rotate());


                            PdfWriter pw = PdfWriter.GetInstance(document, ms);

                            //Call the footer Function

                            pw.PageEvent = new HeaderFooter();

                            document.Open();

                            //Handle fonts and Sizes +  Attachments images logos 

                            iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);

                            document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                            //document.SetMargins(0, 0, 20, 20);
                            BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                            iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);

                            string path = _host.WebRootPath + "/Attachments";

                            if (validation.CompanyName == "marinaplt")
                            {
                                string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                                jpg.SetAbsolutePosition(60f, 550f);
                                //document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -10;
                                prgHeading.SpacingAfter = 50;
                                Chunk cc = new Chunk("Inventory Store Item Report".ToUpper() + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);
                                prgHeading.Add(cc);

                                document.Add(prgHeading);

                            }
                            else if (validation.CompanyName == "piaroma")
                            {
                                string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                //document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -10;
                                prgHeading.SpacingAfter = 50;

                                Chunk cc = new Chunk("Inventory Store Item Report".ToUpper() + " ", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                                prgHeading.Add(cc);

                                document.Add(prgHeading);
                            }
                            else if (validation.CompanyName == "Garastest")
                            {
                                string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                                logo.ScaleAbsolute(300f, 300f);

                                iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                                //document.Add(logo);
                                document.Add(jpg);
                                iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                Paragraph prgHeading = new Paragraph();
                                prgHeading.Alignment = Element.ALIGN_RIGHT;
                                prgHeading.SpacingBefore = -10;
                                prgHeading.SpacingAfter = 50;

                                Chunk cc = new Chunk("Inventory Store Item Report", fntHead);
                                cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);

                                prgHeading.Add(cc);

                                document.Add(prgHeading);
                            }




                            //Adding paragraph for report generated by  
                            Paragraph prgGeneratedBY = new Paragraph();
                            BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                            prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                            document.Add(prgGeneratedBY);



                            //Adding a line  
                            //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                            //document.Add(p);


                            //Adding  PdfPTable  
                            PdfPTable table = new PdfPTable(dt2.Columns.Count);







                            //table Width
                            table.WidthPercentage = 100;

                            //Define Sizes of Cloumns

                            table.SetTotalWidth(new float[] { 160, 35, 45, 38, 45, 42, 35, 25, 50, 35, 45, 40, 40, 40 });


                            for (int i = 0; i < dt2.Columns.Count; i++)
                            {
                                string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                                PdfPCell cell = new PdfPCell();
                                cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                                iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                                cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                                //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                                //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                                cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                                cell.PaddingBottom = 5;
                                table.AddCell(cell);


                            }


                            //writing table Data  
                            for (int i = 0; i < dt2.Rows.Count; i++)
                            {
                                for (int j = 0; j < dt2.Columns.Count; j++)
                                {
                                    //table.AddCell(dt.Rows[i][j].ToString());
                                    PdfPCell cell = new PdfPCell();
                                    cell.ArabicOptions = 1;
                                    if (j <= 5)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                    }
                                    else if (j >= 9)
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    }
                                    else
                                    {
                                        cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                    }

                                    if (cell.ArabicOptions == 1)
                                    {
                                        cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                                    }
                                    else
                                    {
                                        cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                                    }

                                    if (i == 0)
                                    {
                                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                                        cell.BackgroundColor = (new BaseColor(4, 189, 189));
                                        //cell.Chunk = (new Chunk(new iTextSharp.text.Font(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_CENTER, 1)));
                                        cell.Phrase = new Phrase(1, dt2.Rows[0].ToString(), new iTextSharp.text.Font(bf, 60, 1, iTextSharp.text.BaseColor.WHITE));
                                        cell.UseVariableBorders = true;
                                        cell.BorderColorBottom = BaseColor.BLACK;
                                        cell.BorderColorRight = new BaseColor(4, 189, 189);
                                        cell.BorderColorLeft = new BaseColor(4, 189, 189);
                                        cell.BorderColorTop = BaseColor.BLACK;
                                        cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                                        dt2.Rows[0].AcceptChanges();
                                    }

                                    cell.Phrase = new Phrase(1, dt2.Rows[i][j].ToString(), font);
                                    //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                                    table.AddCell(cell);
                                }

                            }



                            document.Add(table);


                            document.Close();
                            byte[] result = ms.ToArray();
                            ms = new MemoryStream();
                            ms.Write(result, 0, result.Length);
                            ms.Position = 0;


                            var CompanyName = validation.CompanyName.ToLower();

                            string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                            string PathsTR = "/Attachments/" + CompanyName + "/";
                            string Filepath = _host.WebRootPath + "/" + PathsTR;
                            string p_strPath = Filepath + "/" + FullFileName;
                            if (!System.IO.File.Exists(p_strPath))
                            {
                                var objFileStrm = System.IO.File.Create(p_strPath);
                                objFileStrm.Close();
                            }

                            File.WriteAllBytes(p_strPath, result);

                            Response.Message = Globals.baseURL + PathsTR + FullFileName;

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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public GetRemainInventoryItemRequestedQtyResponse RemainInventoryItemRequestedQTYReport([FromHeader] string FileExtension)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            GetRemainInventoryItemRequestedQtyResponse Response = new GetRemainInventoryItemRequestedQtyResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var ListIDSInventoryItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.FinalBalance > 0).Select(x => x.InventoryItemId).Distinct().ToList();
                    var InventoryMatrialReleaseItemsData = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(x => x.InventoryItemId != null && ListIDSInventoryItem.Contains((long)x.InventoryItemId)).ToList();
                    var SalesOfferProduct_SalesOfferData = _unitOfWork.VSalesOfferProductSalesOffers.FindAll(x => x.InventoryItemId != null && ListIDSInventoryItem.Contains((long)x.InventoryItemId)).ToList();
                    var InventoryMatrialRequestItemsData = _unitOfWork.VInventoryMatrialRequestItems.FindAll(x => ListIDSInventoryItem.Contains(x.InventoryItemId)).ToList();
                    var InventoryStoreItemData = _unitOfWork.VInventoryStoreItems.FindAll(x => ListIDSInventoryItem.Contains(x.InventoryItemId) && x.StoreActive == true).ToList();

                    var GetRemainInventoryItemRequestedQtyResponseList = new List<GetRemainInventoryItemRequestedQtyResponse>();

                    var clients = _unitOfWork.Clients.FindAll(a => SalesOfferProduct_SalesOfferData.Select(x => x.ClientId).Contains(a.Id));
                    foreach (var InventoryItemId in ListIDSInventoryItem)
                    {
                        var Obj = new GetRemainInventoryItemRequestedQtyResponse();

                        decimal TotalInventoryItemRequestedQty = 0;

                        var RemainOpenProjectsRequestedQtyDb = InventoryMatrialReleaseItemsData.Where(a => a.InventoryItemId == InventoryItemId && a.ReqQuantity > 0 && a.ReqQuantity > a.RecivedQuantity).GroupBy(a => a.ProjectId).ToList();
                        if (RemainOpenProjectsRequestedQtyDb != null && RemainOpenProjectsRequestedQtyDb.Count() > 0)
                        {
                            var RemainOpenProjectsRequestedQtyList = RemainOpenProjectsRequestedQtyDb.Select(project => new OpenProjectRemainRequestedItem
                            {
                                ProjectId = project.Key ?? 0,
                                ClientId = project.Select(a => a.ClientId).FirstOrDefault(),
                                ClientName = project.Select(a => a.ClientName).FirstOrDefault(),
                                RemainRequestedQty = project.Sum(a => a.ReqQuantity ?? 0) - project.Sum(a => a.RecivedQuantity ?? 0)
                            }).ToList();

                            Obj.TotalOpenProfjectsRemainRequestedItemsQty = RemainOpenProjectsRequestedQtyList.Sum(a => a.RemainRequestedQty);
                            TotalInventoryItemRequestedQty += Obj.TotalOpenProfjectsRemainRequestedItemsQty;
                        }

                        var OpenSalesOffersRequestedQtyDb = SalesOfferProduct_SalesOfferData.Where(a => a.InventoryItemId == InventoryItemId && a.Quantity > 0 && a.Status.ToLower() != "closed" && a.Status.ToLower() != "rejected").GroupBy(a => a.OfferId).ToList();
                        if (OpenSalesOffersRequestedQtyDb != null && OpenSalesOffersRequestedQtyDb.Count() > 0)
                        {
                            var OpenSalesOffersRequestedQtyList = OpenSalesOffersRequestedQtyDb.Select(salesOffer => new OpenSalesOfferRequestedItem
                            {
                                SalesOfferId = salesOffer.Key,
                                ClientId = salesOffer.Select(a => a.ClientId).FirstOrDefault(),
                                ClientName = clients.Where(a => a.Id == (salesOffer.Select(a => a.ClientId).FirstOrDefault())).FirstOrDefault()?.Name,
                                RequestedQty = (decimal)salesOffer.Sum(a => a.Quantity ?? 0)
                            }).ToList();
                            Obj.OpenSalesOffersRequestedItem = OpenSalesOffersRequestedQtyList;
                            Obj.TotalOpenSalesOffersRequestedItemsQty = OpenSalesOffersRequestedQtyList.Sum(a => a.RequestedQty);
                            TotalInventoryItemRequestedQty += Obj.TotalOpenSalesOffersRequestedItemsQty;
                        }
                        Obj.TotalInventoryItemRequestedQty = TotalInventoryItemRequestedQty;
                        var HoldItems = InventoryMatrialRequestItemsData.Where(x => x.InventoryItemId == InventoryItemId && x.IsHold == true).ToList();
                        if (HoldItems != null && HoldItems.Count() > 0)
                        {
                            Obj.TotalStocksHoldItemsQty = HoldItems.Sum(x => x.ReqQuantity ?? 0);
                        }
                        var StockAvailableItems = InventoryStoreItemData.Where(a => a.InventoryItemId == InventoryItemId && a.StoreActive == true).ToList();
                        if (StockAvailableItems != null && StockAvailableItems.Count() > 0)
                        {
                            Obj.TotalStocksAvailableItemsQty = StockAvailableItems.Sum(a => a.Balance);
                        }

                        Obj.TotalAvailableItemsQty = Obj.TotalStocksAvailableItemsQty - Obj.TotalStocksHoldItemsQty - Obj.TotalInventoryItemRequestedQty;

                        GetRemainInventoryItemRequestedQtyResponseList.Add(Obj);
                    }











                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[6] { new DataColumn("Total Stocks Available Items"),
                                                     new DataColumn("Total Inventory Item Requested"),
                                                     new DataColumn("Total Open Profjects Remain Requested Items"),
                                                     new DataColumn("Total Open Sales Offers Requested Items"),
                                                     new DataColumn("Total Stocks Hold Items") ,
                                                     new DataColumn("Total Available Items"),



                    });


                    var RemainInventoryItemRequestedList = GetRemainInventoryItemRequestedQtyResponseList;
                    if (RemainInventoryItemRequestedList != null)
                    {
                        foreach (var item in RemainInventoryItemRequestedList)
                        {
                            dt.Rows.Add(
                                    item.TotalStocksAvailableItemsQty,
                                    item.TotalInventoryItemRequestedQty,
                                    item.TotalOpenProfjectsRemainRequestedItemsQty,
                                    item.TotalOpenSalesOffersRequestedItemsQty,
                                    item.TotalStocksHoldItemsQty,
                                    item.TotalAvailableItemsQty



                                );
                        }

                    }





                    //Second List to pass it to PDF



                    if (FileExtension != null && FileExtension == "xml")
                    {
                        using (ExcelPackage packge = new ExcelPackage())
                        {
                            //Create the worksheet
                            ExcelWorksheet ws = packge.Workbook.Worksheets.Add("InventoryStoreItemList");
                            ws.TabColor = Color.Red;
                            ws.Columns.BestFit = true;


                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells["A1:O1"])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                                range.Style.Font.Color.SetColor(Color.White);
                            }



                            using (var package = new ExcelPackage())
                            {
                                var CompanyName = validation.CompanyName.ToLower();

                                string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.xlsx";
                                string PathsTR = "/Attachments/" + CompanyName + "/";
                                string Filepath = _host.WebRootPath + "/" + PathsTR;
                                string p_strPath = Filepath + "/" + FullFileName;
                                var workSheet = package.Workbook.Worksheets.Add("sheet");
                                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);

                                if (!System.IO.File.Exists(p_strPath))
                                {
                                    var objFileStrm = System.IO.File.Create(p_strPath);
                                    objFileStrm.Close();
                                }

                                package.Save();
                                File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                                package.Dispose();

                                Response.Message = Globals.baseURL + PathsTR + FullFileName;


                            }


                        }


                    }
                    else
                    {

                        //Start PDF Service

                        MemoryStream ms = new MemoryStream();

                        //Size of page
                        Document document = new Document(PageSize.A4.Rotate());


                        PdfWriter pw = PdfWriter.GetInstance(document, ms);

                        //Call the footer Function

                        pw.PageEvent = new HeaderFooter();

                        document.Open();

                        //Handle fonts and Sizes +  Attachments images logos 

                        iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);

                        document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                        //document.SetMargins(0, 0, 20, 20);
                        BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);

                        string path = _host.WebRootPath + "/Attachments";

                        if (validation.CompanyName.ToString() == "marinaplt")
                        {
                            string PDFp_strPath = Path.Combine(path, "logoMarina.png");

                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                            jpg.SetAbsolutePosition(60f, 550f);
                            //document.Add(logo);
                            document.Add(jpg);
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;


                            Chunk cc = new Chunk("Remain Inventory Store Items QTY".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);

                        }
                        else if (validation.CompanyName.ToString() == "piaroma")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                            logo.ScaleAbsolute(300f, 300f);

                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            //document.Add(logo);
                            document.Add(jpg);
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Remain Inventory Store Items QTY".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }
                        else if (validation.CompanyName == "Garastest")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                            logo.ScaleAbsolute(300f, 300f);

                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            //document.Add(logo);
                            document.Add(jpg);
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Remain Inventory Store Items QTY", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);

                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }





                        PdfPTable table = new PdfPTable(dt.Columns.Count);







                        //table Width
                        table.WidthPercentage = 100;

                        //Define Sizes of Cloumns

                        table.SetTotalWidth(new float[] { 80, 80, 80, 80, 80, 80 });


                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                            PdfPCell cell = new PdfPCell();
                            cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));

                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                            cell.PaddingBottom = 5;
                            table.AddCell(cell);


                        }


                        //writing table Data  
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                PdfPCell cell = new PdfPCell();
                                cell.ArabicOptions = 1;
                                if (j <= 5)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                }
                                else if (j >= 9)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }
                                else
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }

                                if (cell.ArabicOptions == 1)
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                                }
                                else
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                                }


                                cell.Phrase = new Phrase(1, dt.Rows[i][j].ToString(), font);

                                table.AddCell(cell);

                            }

                        }



                        document.Add(table);


                        document.Close();
                        byte[] result = ms.ToArray();
                        ms = new MemoryStream();
                        ms.Write(result, 0, result.Length);
                        ms.Position = 0;


                        var CompanyName = validation.CompanyName.ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "RemainInventoryStoreItemsQTY.pdf";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        string Filepath = _host.WebRootPath + "/" + PathsTR;
                        string p_strPath = Filepath + "/" + FullFileName;
                        if (!System.IO.File.Exists(p_strPath))
                        {
                            var objFileStrm = System.IO.File.Create(p_strPath);
                            objFileStrm.Close();
                        }

                        File.WriteAllBytes(p_strPath, result);
                        Response.Message = Globals.baseURL + PathsTR + FullFileName;

                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public AccountAndFinanceInventoryItemMovementResponse GetAccountAndFinanceInventoryItemMovementList([FromHeader] long InventoryItemID, [FromHeader] DateTime? FromDate, [FromHeader] DateTime? ToDate)
        {
            AccountAndFinanceInventoryItemMovementResponse Response = new AccountAndFinanceInventoryItemMovementResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryStoreItemMovmentList = new List<ItemMovement>();
                if (Response.Result)
                {
                    DateTime FromDateFilter = new DateTime(DateTime.Now.Year, 1, 1);  // Bishoy magdy modifications 2024-10-14
                    //DateTime FromDateTemp = DateTime.Now;
                    if (FromDate != null)
                    {
                        FromDateFilter = (DateTime)FromDate;
                    }
                    DateTime ToDateFilter = DateTime.Now;
                    //DateTime ToDateTemp = DateTime.Now;
                    if (ToDate != null)
                    {
                        ToDateFilter = (DateTime)ToDate;
                    }
                    double cummlativeQty = 0;
                    if (Response.Result)
                    {
                        var ListInventoryItemMovment = _unitOfWork.VInventoryStoreItems.FindAll(x => x.Active == true && x.InventoryItemId == InventoryItemID).OrderBy(x => x.CreationDate).ToList();
                        // Filters --------
                        if (ListInventoryItemMovment.Count() > 0)
                        {
                            foreach (var item in ListInventoryItemMovment)
                            {
                                var ItemMovmentObj = new ItemMovement();
                                ItemMovmentObj.OperationType = item.OperationType;
                                ItemMovmentObj.Qty = (double)item.Balance;
                                ItemMovmentObj.HoldQty = item.HoldQty ?? 0;
                                ItemMovmentObj.HoldComment = item.HoldReason;
                                ItemMovmentObj.OrderID = item.OrderId;
                                cummlativeQty = cummlativeQty + ItemMovmentObj.Qty;
                                ItemMovmentObj.CumilativeQty = cummlativeQty;
                                ItemMovmentObj.StoreName = item.InventoryStoreName;
                                ItemMovmentObj.ReqUOM = item.RequstionUomname;

                                // Extra DAta PO Item
                                ItemMovmentObj.ID = item.Id;
                                ItemMovmentObj.ParentID = item.ReleaseParentId;
                                ItemMovmentObj.POID = item.AddingFromPoid;
                                ItemMovmentObj.ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "";
                                ItemMovmentObj.ItemSerial = item.ItemSerial;
                                ItemMovmentObj.RemainBalance = item.FinalBalance;
                                ItemMovmentObj.CurrencyId = item.CurrencyId;
                                ItemMovmentObj.CurrencyName = Common.GetCurrencyName(item.CurrencyId ?? 0, _Context);
                                ItemMovmentObj.RateToEGP = item.RateToEgp;
                                ItemMovmentObj.POInvoicePriceEGP = item.PoinvoiceTotalPriceEgp;
                                ItemMovmentObj.POInvoiceUnitCostEGP = item.PoinvoiceTotalCostEgp;
                                if (item.Balance > 0)
                                {
                                    ItemMovmentObj.remainItemCostEGP = item.RemainItemCosetEgp;
                                    ItemMovmentObj.remainItemCostOtherCU = item.RemainItemCostOtherCu;

                                }
                                if (item.OperationType == "Add New Matrial")
                                {
                                    var V_InventoryAddingOrderObj = _unitOfWork.VInventoryAddingOrders.FindAll(x => x.Id == item.OrderId).FirstOrDefault();
                                    if (V_InventoryAddingOrderObj != null)
                                    {
                                        ItemMovmentObj.FromUser = V_InventoryAddingOrderObj.FirstName + V_InventoryAddingOrderObj.LastName;
                                        ItemMovmentObj.FromSupplier = V_InventoryAddingOrderObj.SupplierName;
                                        ItemMovmentObj.FromDepartment = V_InventoryAddingOrderObj.DepartmentName;
                                        ItemMovmentObj.OrderType = "ViewAddingOrder";

                                        if (V_InventoryAddingOrderObj.RecivingDate != null)
                                        {
                                            ItemMovmentObj.DateFilter = V_InventoryAddingOrderObj.RecivingDate;
                                            ItemMovmentObj.Date = V_InventoryAddingOrderObj.RecivingDate.ToString("dd-MM-yyyy");
                                        }
                                    }

                                }
                                else if (item.OperationType.Contains("Add External Back Order"))
                                {

                                    var V_InventoryAddingOrderObj = _unitOfWork.VInventoryAddingOrders.FindAll(x => x.Id == item.OrderId).FirstOrDefault();
                                    if (V_InventoryAddingOrderObj != null)
                                    {
                                        ItemMovmentObj.FromUser = V_InventoryAddingOrderObj.FirstName + V_InventoryAddingOrderObj.LastName;
                                        ItemMovmentObj.FromSupplier = V_InventoryAddingOrderObj.SupplierName;
                                        ItemMovmentObj.FromDepartment = V_InventoryAddingOrderObj.DepartmentName;
                                        ItemMovmentObj.OrderType = "ViewExternalBackOrder";

                                        if (V_InventoryAddingOrderObj.RecivingDate != null)
                                        {
                                            ItemMovmentObj.DateFilter = V_InventoryAddingOrderObj.RecivingDate;
                                            ItemMovmentObj.Date = V_InventoryAddingOrderObj.RecivingDate.ToString("dd-MM-yyyy");
                                        }
                                    }
                                }
                                else if (item.OperationType.Contains("Transfer Order"))
                                {

                                    var V_InventoryTransferOrderObj = _unitOfWork.VInventoryInternalTransferOrders.FindAll(x => x.Id == item.OrderId).FirstOrDefault();
                                    if (V_InventoryTransferOrderObj != null)
                                    {
                                        ItemMovmentObj.FromUser = V_InventoryTransferOrderObj.FirstName + V_InventoryTransferOrderObj.LastName;
                                        ItemMovmentObj.StoreName = V_InventoryTransferOrderObj.FromInventoryStoreName;
                                        ItemMovmentObj.OrderType = "ViewTransferOrder";

                                        if (V_InventoryTransferOrderObj.RecivingDate != null)
                                        {
                                            ItemMovmentObj.DateFilter = V_InventoryTransferOrderObj.RecivingDate;
                                            ItemMovmentObj.Date = V_InventoryTransferOrderObj.RecivingDate.ToString("dd-MM-yyyy");
                                        }
                                    }
                                }
                                else if (item.OperationType.Contains("Release Order"))
                                {

                                    var V_InventoryMatrialReleaseObj = _unitOfWork.VInventoryMatrialReleases.FindAll(x => x.Id == item.OrderId).FirstOrDefault();
                                    if (V_InventoryMatrialReleaseObj != null)
                                    {
                                        ItemMovmentObj.FromUser = V_InventoryMatrialReleaseObj.FromUserFname + V_InventoryMatrialReleaseObj.FromUserLname;
                                        ItemMovmentObj.FromSupplier = V_InventoryMatrialReleaseObj.FromUserDepartment;
                                        ItemMovmentObj.FromDepartment = V_InventoryMatrialReleaseObj.ToUserDepartment;
                                        ItemMovmentObj.OrderType = "ViewMatrialRelease";

                                        if (V_InventoryMatrialReleaseObj.RequestDate != null)
                                        {
                                            ItemMovmentObj.DateFilter = V_InventoryMatrialReleaseObj.RequestDate;
                                            ItemMovmentObj.Date = V_InventoryMatrialReleaseObj.RequestDate.ToString("dd-MM-yyyy");
                                        }
                                    }
                                }
                                else if (item.OperationType == "Internal Back Order" ||
                                    item.OperationType == "Final Product" ||
                                    item.OperationType == "Semi-Final Product" ||
                                    item.OperationType == "Client Returns"
                                    )
                                {

                                    var V_InventoryInternalBackOrderObj = _unitOfWork.VInventoryInternalBackOrders.FindAll(x => x.Id == item.OrderId).FirstOrDefault();
                                    if (V_InventoryInternalBackOrderObj != null)
                                    {
                                        ItemMovmentObj.FromUser = V_InventoryInternalBackOrderObj.FromUser;
                                        ItemMovmentObj.FromDepartment = V_InventoryInternalBackOrderObj.FromUserDepartment;
                                        ItemMovmentObj.OrderType = "ViewInternalBackOrder";
                                        if (item.OperationType == "Client Returns")
                                        {
                                            ItemMovmentObj.OrderType = "ViewClientReturns";

                                        }
                                        if (V_InventoryInternalBackOrderObj.RecivingDate != null)
                                        {
                                            ItemMovmentObj.DateFilter = V_InventoryInternalBackOrderObj.RecivingDate;
                                            ItemMovmentObj.Date = V_InventoryInternalBackOrderObj.RecivingDate.ToString("dd-MM-yyyy");
                                        }
                                    }
                                }
                                else if (item.OperationType.Contains("رصيد اول") || item.OperationType.Contains("Opening Balance"))
                                {
                                    ItemMovmentObj.OrderType = "ViewOpeningBalance";
                                    if (item.CreationDate != null)
                                    {
                                        ItemMovmentObj.DateFilter = item.CreationDate;
                                        ItemMovmentObj.Date = item.CreationDate.ToString("dd-MM-yyyy");
                                    }
                                }
                                InventoryStoreItemMovmentList.Add(ItemMovmentObj);

                                if (FromDate != null)
                                {
                                    InventoryStoreItemMovmentList = InventoryStoreItemMovmentList.Where(x => x.DateFilter >= FromDateFilter).ToList();
                                }

                                if (ToDate != null)
                                {
                                    InventoryStoreItemMovmentList = InventoryStoreItemMovmentList.Where(x => x.DateFilter <= ToDateFilter).ToList();
                                }
                            }




                        }
                        // Bishoy magdy modifications 2024-10-14
                        var InventoryStoreItemMovmentListFilter = InventoryStoreItemMovmentList.Where(x => x.CreationDate != null).OrderBy(x => x.CreationDate).ToList();
                        //DateTime DateFrom = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).FirstOrDefault();
                        //DateTime DateTO = InventoryStoreItemMovmentListFilter.Select(x => x.DateFilter != null ? (DateTime)x.DateFilter : DateTime.Now).LastOrDefault();
                        double numberOfMonths = Math.Abs(Math.Ceiling(ToDateFilter.Subtract(FromDateFilter).Days / (365.25 / 12)));
                        Response.NoOfMonth = numberOfMonths;
                        Response.DateFrom = FromDateFilter.ToString("dd-MM-yyyy");
                        //headers["FromDate"] != null && headers["FromDate"] != "" ? FromDateTemp.ToString("dd-MM-yyyy") : DateFrom.ToString("dd-MM-yyy" +"+y");
                        Response.DateTo = ToDateFilter.ToString("dd-MM-yyyy");
                        //headers["ToDate"] != null && headers["ToDate"] != "" ? ToDateTemp.ToString("dd-MM-yyyy") : DateTO.ToString("dd-MM-yyyy");

                        if (FromDateFilter <= ToDateFilter)
                        {
                            ListInventoryItemMovment = ListInventoryItemMovment.Where(x => x.CreationDate >= FromDateFilter && x.CreationDate <= ToDateFilter).ToList();
                        }
                        else
                        {
                            ListInventoryItemMovment = ListInventoryItemMovment.Where(x => x.CreationDate >= ToDateFilter && x.CreationDate <= FromDateFilter).ToList();
                        }

                        if (InventoryStoreItemMovmentListFilter.Count > 0)
                        {

                            var ReleaseQty = ListInventoryItemMovment.Where(x => x.OperationType.Contains("Release Order") ||
                                                            x.OperationType.Contains("POS Release")
                                                            ).Select(x => Math.Abs(x.Balance)).DefaultIfEmpty(0).Sum();

                            Response.ReleaseRate = numberOfMonths != 0 ? (Response.ReleaseQty / numberOfMonths) : 0;
                        }

                    }
                    Response.InventoryItemMovementList = InventoryStoreItemMovmentList;
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
        public GetInventoryStoreItemMovementReportResponse InventoryStoreItemMovementReportPDF([FromHeader] string FileExtension, [FromHeader] long InventoryItemID)
        {
            GetInventoryStoreItemMovementReportResponse Response = new GetInventoryStoreItemMovementReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Inventory Item ID is Mandatory!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var AccountAndFinanceInventoryItemMovementList = GetAccountAndFinanceInventoryItemMovementList(InventoryItemID, null, null);
                    var GetAccountAndFinanceInventoryItemMovementResponseList = new List<GetRemainInventoryItemRequestedQtyResponse>();
                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[24] {
                                                     new DataColumn("ID"),
                                                     new DataColumn("Parent ID"),
                                                     new DataColumn("PO ID"),
                                                     new DataColumn("Date"),
                                                     new DataColumn("Operation") ,
                                                     new DataColumn("From User"),
                                                     new DataColumn("From Department"),
                                                     new DataColumn("From Supplier"),
                                                     new DataColumn("To User"),
                                                     new DataColumn("To Department"),
                                                     new DataColumn("QTY (RUOM)") ,
                                                     new DataColumn("Cumulative QTY"),
                                                     new DataColumn("Req UOM"),
                                                     new DataColumn("Store"),
                                                     new DataColumn("Remain Balance"),
                                                     new DataColumn("Serial"),
                                                     new DataColumn("ExpensisType Date") ,
                                                     new DataColumn("Po Unit Price"),
                                                     new DataColumn("Po Unit Cost"),
                                                     new DataColumn("Rate To EGP"),
                                                     new DataColumn("Total Cost EGP"),
                                                     new DataColumn("Total Cost CU"),
                                                     new DataColumn("Hold Qty"),
                                                     new DataColumn("Hold Comment")


                    });
                    if (AccountAndFinanceInventoryItemMovementList != null)
                    {
                        foreach (var item in AccountAndFinanceInventoryItemMovementList.InventoryItemMovementList)
                        {
                            dt.Rows.Add(
                                    item.ID != null ? item.ID : 0,
                                    item.ParentID != null ? item.ParentID : 0,
                                    item.POID != null ? item.POID : 0,
                                    item.Date != null ? item.Date : "-",
                                    item.OperationType != null ? item.OperationType : "-",
                                    item.FromUser != null ? item.FromUser : "-",
                                    item.FromDepartment != null ? item.FromDepartment : "-",
                                    item.FromSupplier != null ? item.FromSupplier : "-",
                                    item.Qty,
                                    item.Qty,
                                    item.Qty != null ? item.Qty : 0,
                                    item.CumilativeQty != null ? item.CumilativeQty : 0,
                                    item.ReqUOM != null ? item.ReqUOM : "-",
                                    item.StoreName != null ? item.StoreName : "-",
                                    item.RemainBalance != null ? item.RemainBalance : 0,
                                    item.ItemSerial != null ? item.ItemSerial : "-",
                                    item.OrderType != null ? item.OrderType : "-",
                                    item.POInvoicePriceEGP != null ? item.POInvoicePriceEGP : 0,
                                    item.POInvoiceUnitCostEGP != null ? item.POInvoiceUnitCostEGP : 0,
                                    item.RateToEGP != null ? item.RateToEGP : 0,
                                    item.remainItemCostEGP != null ? item.remainItemCostEGP : 0,
                                    item.remainItemCostOtherCU != null ? item.remainItemCostOtherCU : 0,
                                    item.HoldQty != null ? item.HoldQty : 0,
                                    item.HoldComment != null ? item.HoldComment : "-"





                                );
                        }

                    }





                    //Second List to pass it to PDF



                    if (FileExtension != null && FileExtension == "xml")
                    {
                        using (ExcelPackage packge = new ExcelPackage())
                        {
                            //Create the worksheet
                            ExcelWorksheet ws = packge.Workbook.Worksheets.Add("InventoryStoreItemList");
                            ws.TabColor = Color.Red;
                            ws.Columns.BestFit = true;


                            //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            //ws.Cells[1, 30].LoadFromDataTable(dt2, true);

                            //Format the header for column 1-3
                            using (ExcelRange range = ws.Cells["A1:O1"])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                                range.Style.Font.Color.SetColor(Color.White);
                            }
                            //Example how to Format Column 1 as numeric 
                            //using (ExcelRange col = ws.Cells[2, 6, 2 + dt.Rows.Count, 20])
                            //{
                            //    //col.Style.Numberformat.Format = "#,##0.00";
                            //    //col.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            //    //col.Style.WrapText = true;
                            //    ws.Column(1).BestFit = true;
                            //    ws.Row(1).Diameter = 300;
                            //    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                            //}


                            using (var package = new ExcelPackage())
                            {
                                var CompanyName = validation.CompanyName.ToLower();

                                string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.xlsx";
                                string PathsTR = "/Attachments/" + CompanyName + "/";
                                string Filepath = _host.WebRootPath + "/" + PathsTR;
                                string p_strPath = Filepath + "/" + FullFileName;
                                var workSheet = package.Workbook.Worksheets.Add("sheet");
                                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);

                                if (!System.IO.File.Exists(p_strPath))
                                {
                                    var objFileStrm = System.IO.File.Create(p_strPath);
                                    objFileStrm.Close();
                                }


                                package.Save();
                                File.WriteAllBytes(p_strPath, package.GetAsByteArray());
                                package.Dispose();

                                Response.Message = Globals.baseURL + PathsTR + FullFileName;


                            }


                        }


                    }
                    else
                    {

                        //Start PDF Service

                        MemoryStream ms = new MemoryStream();

                        //Size of page
                        Document document = new Document(PageSize.A4.Rotate());


                        PdfWriter pw = PdfWriter.GetInstance(document, ms);

                        //Call the footer Function

                        pw.PageEvent = new HeaderFooter();

                        document.Open();

                        //Handle fonts and Sizes +  Attachments images logos 

                        iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(PageSize.A4);

                        document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                        //document.SetMargins(0, 0, 20, 20);
                        BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                        iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 8, iTextSharp.text.Font.NORMAL);

                        string path = _host.WebRootPath + "/Attachments";

                        if (validation.CompanyName == "marinaplt")
                        {
                            string PDFp_strPath = Path.Combine(path, "logoMarina.png");


                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                            jpg.SetAbsolutePosition(60f, 550f);
                            //document.Add(logo);
                            document.Add(jpg);
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;


                            Chunk cc = new Chunk("Inventory Store Item Mouvement".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);

                        }
                        else if (validation.CompanyName == "piaroma")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                            logo.ScaleAbsolute(300f, 300f);

                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            //document.Add(logo);
                            document.Add(jpg);
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Inventory Store Item Mouvement".ToUpper() + " ", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);


                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }
                        else if (validation.CompanyName == "Garastest")
                        {
                            string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                            logo.ScaleAbsolute(300f, 300f);

                            iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                            //document.Add(logo);
                            document.Add(jpg);
                            iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                            Paragraph prgHeading = new Paragraph();
                            prgHeading.Alignment = Element.ALIGN_RIGHT;
                            prgHeading.SpacingBefore = -10;
                            prgHeading.SpacingAfter = 50;

                            Chunk cc = new Chunk("Inventory Store Item Mouvement", fntHead);
                            cc.SetBackground(new BaseColor(4, 189, 189), 150, 9, 0, 40);

                            prgHeading.Add(cc);

                            document.Add(prgHeading);
                        }





                        PdfPTable table = new PdfPTable(dt.Columns.Count);







                        //table Width
                        table.WidthPercentage = 100;

                        //Define Sizes of Cloumns

                        table.SetTotalWidth(new float[] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 });


                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            string cellText = HttpUtility.HtmlDecode(dt.Columns[i].ColumnName);
                            PdfPCell cell = new PdfPCell();
                            cell.Phrase = new Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                            iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                            cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));

                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            cell.VerticalAlignment = Element.ALIGN_MIDDLE;





                            cell.PaddingBottom = 5;
                            table.AddCell(cell);


                        }


                        //writing table Data  
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                //table.AddCell(dt.Rows[i][j].ToString());
                                PdfPCell cell = new PdfPCell();
                                cell.ArabicOptions = 1;
                                if (j <= 5)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_LEFT;
                                }
                                else if (j >= 9)
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }
                                else
                                {
                                    cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                                }

                                if (cell.ArabicOptions == 1)
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;

                                }
                                else
                                {
                                    cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                                }


                                cell.Phrase = new Phrase(1, dt.Rows[i][j].ToString(), font);
                                //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                                table.AddCell(cell);

                            }

                        }



                        document.Add(table);


                        document.Close();
                        byte[] result = ms.ToArray();
                        ms = new MemoryStream();
                        ms.Write(result, 0, result.Length);
                        ms.Position = 0;


                        var CompanyName = validation.CompanyName.ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "RemainInventoryStoreItemsQTY.pdf";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        string Filepath = _host.WebRootPath + "/" + PathsTR;
                        string p_strPath = Filepath + "/" + FullFileName;

                        if (!System.IO.File.Exists(p_strPath))
                        {
                            var objFileStrm = System.IO.File.Create(p_strPath);
                            objFileStrm.Close();
                        }
                        Response.Message = Globals.baseURL + PathsTR + FullFileName;

                    }

                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public AccountsAndFinanceInventoryStoreItemReportResponse GetInventoryStoreItemMovementReportList(GetAccountAndFinanceInventoryStoreItemMovementReportListFilters filters)
        {
            AccountsAndFinanceInventoryStoreItemReportResponse Response = new AccountsAndFinanceInventoryStoreItemReportResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                DateTime StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                if (!string.IsNullOrEmpty(filters.FromDate))
                {
                    DateTime.TryParse(filters.FromDate, out StartDate);
                }

                DateTime EndDate = DateTime.Now;
                if (!string.IsNullOrEmpty(filters.ToDate))
                {
                    DateTime.TryParse(filters.ToDate, out EndDate);
                }

                decimal NODaysDurations = EndDate.Subtract(StartDate).Days;
                decimal numberOfMonths = NODaysDurations / (decimal)(365.25 / 12);

                // Fetch all items from the database
                var V_InventoryStoreItemDB = _unitOfWork.VInventoryStoreItems.FindAllQueryable(a => true);

                // Apply initial filters
                var InventoryStoreItemQuerableDB = V_InventoryStoreItemDB
                    .Where(x => x.Active == true && EF.Functions.Like(x.OperationType, "%Release Order%"));

                // Apply additional filters
                if (filters.InventoryItemID != 0)
                {
                    InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => x.InventoryItemId == filters.InventoryItemID);
                }
                if (filters.InventoryItemCategoryID != 0)
                {
                    InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => x.InventoryItemCategoryId == filters.InventoryItemCategoryID);
                }
                if (filters.PriorityID != 0)
                {
                    InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => x.PriorityId == filters.PriorityID);
                }
                if (!string.IsNullOrEmpty(filters.Exported))
                {
                    InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => x.Exported.ToLower() == filters.Exported.ToLower());
                }

                // Fetch filtered items from the database
                var InventoryStoreItemListDB = InventoryStoreItemQuerableDB
                    .Select(x => new Infrastructure.Models.User.UsedInResponse.InventoryStoreItemItemMovementReport
                    {
                        ID = x.InventoryItemId,
                        InventoryItemID = x.InventoryItemId,
                        OperationType = x.OperationType,
                        ItemName = x.InventoryItemName,
                        InventoryStoreID = x.InventoryStoreId,
                        InventoryStoreName = x.InventoryStoreName,
                        InventoryItemCategoryID = x.InventoryItemCategoryId,
                        CategoryName = x.CategoryName,
                        MarketName = x.MarketName,
                        CommercialName = x.CommercialName,
                        MaxUnitPrice = x.MaxUnitPrice,
                        AverageUnitPrice = x.AverageUnitPrice,
                        LastUnitPrice = x.LastUnitPrice,
                        CreationDate = x.CreationDate,
                        ItemCode = x.Code,
                        Exported = x.Exported,
                        PriorityID = x.PriorityId,
                        StockBalance = 0, // Placeholder, will be calculated later
                        NoMonths = numberOfMonths,
                        ReleaseQty = 0, // Placeholder, will be calculated later
                        ReleaseRate = 0, // Placeholder, will be calculated later
                        LowStockAfter = 0 // Placeholder, will be calculated later
                    })
                    .AsEnumerable(); // Switch to client-side evaluation

                var stockBalanceDictionary = V_InventoryStoreItemDB.ToList().GroupBy(a => a.InventoryItemId).ToDictionary(g => g.Key, g => Math.Abs(g.Sum(a => a.Balance)));

                var releaseQtyDictionary = V_InventoryStoreItemDB.Where(a => a.OperationType.Contains("Release Order")).ToList().GroupBy(a => a.InventoryItemId).ToDictionary(
                        g => g.Key, // InventoryItemId
                        g => Math.Abs(g.Sum(a => a.Balance)) // Sum of Balance
                    );


                var test = InventoryStoreItemListDB.ToList();
                // Perform client-side calculations
                foreach (var item in test)
                {
                    // Use the precomputed StockBalance
                    item.StockBalance = stockBalanceDictionary.ContainsKey(item.InventoryItemID)
                        ? stockBalanceDictionary[item.InventoryItemID]
                        : 0;

                    // Use the precomputed ReleaseQty
                    item.ReleaseQty = releaseQtyDictionary.ContainsKey(item.InventoryItemID)
                        ? releaseQtyDictionary[item.InventoryItemID]
                        : 0;

                    // Calculate ReleaseRate
                    item.ReleaseRate = item.ReleaseQty / (decimal)numberOfMonths;

                    // Calculate LowStockAfter
                    item.LowStockAfter = item.ReleaseRate != 0
                        ? Math.Abs(item.StockBalance / (decimal)item.ReleaseRate)
                        : 0;
                }


                // Apply additional client-side filters
                if (filters.InventoryStoreID != null)
                {
                    test = test.Where(x => x.InventoryStoreID == filters.InventoryStoreID).ToList();
                }
                if (!string.IsNullOrEmpty(filters.ItemSerial))
                {
                    test = test.Where(x => x.ItemCode == filters.ItemSerial).ToList();
                }
                if (filters.LowAfterNOMonths != null)
                {
                    test = test.Where(x => x.LowStockAfter < filters.LowAfterNOMonths).ToList();
                }
                if (!string.IsNullOrEmpty(filters.SearchKey))
                {
                    string SearchKey = HttpUtility.UrlDecode(filters.SearchKey).ToLower();
                    test = test.Where(x =>
                        x.ItemName.ToLower().Contains(SearchKey) ||
                        x.ItemCode.ToLower().Contains(SearchKey) ||
                        x.MarketName.ToLower().Contains(SearchKey) ||
                        x.CommercialName.ToLower().Contains(SearchKey)).ToList();
                }

                // Group and order the results
                var InventoryStoreItemListDBGrouped = test
                    .Where(x => x.StockBalance > 0)
                    .GroupBy(a => new Infrastructure.Models.User.Response.InventoryItemSort
                    {
                        InventoryItemID = a.InventoryItemID,
                        InventoryItemName = a.ItemName,
                        LowStockAfter = a.LowStockAfter,
                        AverageUnitPrice = a.AverageUnitPrice,
                        LastUnitPrice = a.LastUnitPrice,
                        StockBalance = a.StockBalance,
                        MaxUnitPrice = a.MaxUnitPrice
                    })
                    .OrderBy(x => x.Key.InventoryItemID);

                // Apply sorting
                if (!string.IsNullOrEmpty(filters.SortBy))
                {
                    if (filters.SortBy == "ItemName")
                    {
                        InventoryStoreItemListDBGrouped = InventoryStoreItemListDBGrouped.OrderBy(x => x.Key.InventoryItemName);
                    }
                    else if (filters.SortBy == "LowStockAfter")
                    {
                        InventoryStoreItemListDBGrouped = InventoryStoreItemListDBGrouped.OrderByDescending(x => x.Key.LowStockAfter);
                    }
                    else if (filters.SortBy == "AverageUnitPrice")
                    {
                        InventoryStoreItemListDBGrouped = InventoryStoreItemListDBGrouped.OrderByDescending(x => x.Key.AverageUnitPrice);
                    }
                    else if (filters.SortBy == "MaxUnitPrice")
                    {
                        InventoryStoreItemListDBGrouped = InventoryStoreItemListDBGrouped.OrderByDescending(x => x.Key.MaxUnitPrice);
                    }
                    else if (filters.SortBy == "LastUnitPrice")
                    {
                        InventoryStoreItemListDBGrouped = InventoryStoreItemListDBGrouped.OrderByDescending(x => x.Key.LastUnitPrice);
                    }
                    else if (filters.SortBy == "StockBalance")
                    {
                        InventoryStoreItemListDBGrouped = InventoryStoreItemListDBGrouped.OrderByDescending(x => x.Key.StockBalance);
                    }
                }

                // Paginate the results
                var InventoryStoreItemPagingList = PagedList<IGrouping<InventoryItemSort, Infrastructure.Models.User.UsedInResponse.InventoryStoreItemItemMovementReport>>.Create(InventoryStoreItemListDBGrouped.AsQueryable(), filters.CurrentPage, filters.NumberOfItemsPerPage);

                Response.PaginationHeader = new PaginationHeader
                {
                    CurrentPage = filters.CurrentPage,
                    TotalPages = InventoryStoreItemPagingList.TotalPages,
                    ItemsPerPage = filters.NumberOfItemsPerPage,
                    TotalItems = InventoryStoreItemPagingList.TotalCount
                };

                if (InventoryStoreItemPagingList.Count > 0)
                {
                    Response.InventoryStoreItemList = InventoryStoreItemPagingList.Select(x => x.FirstOrDefault()).ToList();
                    Response.Result = true;
                    Response.NoOfMonth = numberOfMonths;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error
                {
                    ErrorCode = "Err10",
                    ErrorMSG = ex.InnerException?.Message ?? ex.Message
                };
                Response.Errors.Add(error);
                return Response;
            }
        }

        public InventoryItemSupplierResponse GetInventoryItemSupplierList(long InventoryItemID, string SupplierItemSerial, string OrderType)
        {
            InventoryItemSupplierResponse Response = new InventoryItemSupplierResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var InventoryStoreItemSupplierList = new List<InventoryItemSupplier>();

                if (Response.Result)
                {
                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item ID";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    if (Response.Result)
                    {

                        var InventoryAddingOrderItemListDB = _unitOfWork.VInventoryAddingOrderItems.FindAll(x => x.InventoryItemId == InventoryItemID).ToList();
                        if (OrderType == "AddingOrder")
                        {
                            var IDInventoryAddingOrder = _unitOfWork.InventoryAddingOrders.FindAll(x => x.OperationType == "Add New Matrial").Select(x => x.Id).Distinct().ToList();
                            InventoryAddingOrderItemListDB = InventoryAddingOrderItemListDB.Where(x => IDInventoryAddingOrder.Contains(x.InventoryAddingOrderId)).ToList();
                        }
                        else if (OrderType == "ExternalBackOrder")
                        {
                            var IDInventoryAddingOrder = _unitOfWork.InventoryAddingOrders.FindAll(x => x.OperationType == "Add External Back Order").Select(x => x.Id).Distinct().ToList();
                            InventoryAddingOrderItemListDB = InventoryAddingOrderItemListDB.Where(x => IDInventoryAddingOrder.Contains(x.InventoryAddingOrderId)).ToList();
                        }

                        if (SupplierItemSerial != null)
                        {
                            InventoryAddingOrderItemListDB = InventoryAddingOrderItemListDB.Where(x => x.ItemSerial == SupplierItemSerial).ToList();
                        }
                        var suppliarIDs = InventoryAddingOrderItemListDB.Select(a => a.SupplierId).ToList();
                        var supplierList = _unitOfWork.Suppliers.FindAll(a => suppliarIDs.Contains(a.Id)).ToList();
                        foreach (var item in InventoryAddingOrderItemListDB)
                        {
                            var InventoryItemSupplierObj = new InventoryItemSupplier();
                            InventoryItemSupplierObj.Serial = item.ItemSerial;
                            InventoryItemSupplierObj.QTY = item.RecivedQuantity != null ? item.RecivedQuantity.ToString() : "0";
                            InventoryItemSupplierObj.OrderingNo = item.InventoryAddingOrderId.ToString();
                            InventoryItemSupplierObj.OperationType = item.OperationType;
                            InventoryItemSupplierObj.SupplierName = supplierList.Select(a => a.Name).FirstOrDefault();//Common.GetSupplierNameFromInventoryAddingOrder(item.InventoryAddingOrderID);
                            InventoryItemSupplierObj.ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "";
                            InventoryStoreItemSupplierList.Add(InventoryItemSupplierObj);
                        }


                        Response.InventoryItemSupplierList = InventoryStoreItemSupplierList;
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

        public GetContractTypeListResponse GetContractTypeList()
        {
            GetContractTypeListResponse Response = new GetContractTypeListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var List = new List<GetContractType>();
                if (Response.Result)
                {

                    var ListDB = _unitOfWork.ContractTypes.GetAll();
                    foreach (var item in ListDB)
                    {
                        var ItemCategoryrObj = new GetContractType();
                        ItemCategoryrObj.ID = item.Id;
                        ItemCategoryrObj.Name = item.Name;
                        ItemCategoryrObj.Desc = item.Description;
                        List.Add(ItemCategoryrObj);
                    }


                    Response.Contracts = List;
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

        public async Task<BaseResponse> HoldReleaseInventoryMatrialRequest(AddInventoryStoreWithMatrialRequestt Request)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                VInventoryMatrialRequest MatrialRequestDB = null;
                if (Request == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "please insert a valid data.";
                    Response.Errors.Add(error);
                    return Response;
                }
                long MatrialRequestId = 0;
                if (Request.MatrialRequestId != 0)
                {
                    MatrialRequestId = (long)Request.MatrialRequestId;
                    MatrialRequestDB = await _unitOfWork.VInventoryMatrialRequests.FindAsync(x => x.Id == MatrialRequestId);
                    if (MatrialRequestDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "Matrial Request ID is not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (MatrialRequestDB.Status != "Hold")
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err24";
                        error.ErrorMSG = "The type of matrial request is not a hold.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err24";
                    error.ErrorMSG = "Invalid Matrial Request Store ID.";
                    Response.Errors.Add(error);
                    return Response;
                }



                if (Request.MatrialRequestItemList == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-15";
                    error.ErrorMSG = "please insert at least one Matrial Request Item.";
                    Response.Errors.Add(error);
                    return Response;
                }


                if (Request.MatrialRequestItemList.Count <= 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-15";
                    error.ErrorMSG = "please insert at least one Matrial Request Item..";
                    Response.Errors.Add(error);
                    return Response;
                }

                // validate is Count distinct < count of Items => there is iteration items with same data
                var ItemDistinctCount = Request.MatrialRequestItemList.Select(x => new { x.InventoryItemID }).Distinct().Count();
                if (ItemDistinctCount < Request.MatrialRequestItemList.Count())
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err27";
                    error.ErrorMSG = "There is item itteration";
                    Response.Errors.Add(error);
                    return Response;
                }
                var IDSItems = Request.MatrialRequestItemList.Select(x => x.InventoryItemID).ToList();
                //var IDSInventoryStoreItemsRequest = Request.MatrialRequestItemList.Where(x => x.InventoryStoreItemIDsList != null).SelectMany(x => x.InventoryStoreItemIDsList).ToList();
                //var InventoryStoreItemDBList = await _Context.InventoryStoreItems.Where(x => IDSInventoryStoreItemsRequest.Contains(x.ID) || x.InventoryStoreID == InventoryStoreID).ToList();
                var IDSInventoryStoreItemsRequest = Request.MatrialRequestItemList.Where(x => x.InventoryStoreItemIDsList != null).SelectMany(x => x.InventoryStoreItemIDsList).ToList();
                var InventoryStoreItemDBList = await _unitOfWork.InventoryStoreItems.FindAllAsync(x => IDSInventoryStoreItemsRequest.Contains(x.Id) || IDSItems.Contains(x.InventoryItemId));
                int Counter = 0;
                foreach (var item in Request.MatrialRequestItemList)
                {
                    Counter++;
                    if (item.InventoryItemID < 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err47";
                        error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                        Response.Errors.Add(error);
                    }
                    else // Check is Inventoryt Item ID is valid
                    {
                        var InventoryItemObjDB = _unitOfWork.InventoryItems.GetById(item.InventoryItemID);
                        if (InventoryItemObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err47";
                            error.ErrorMSG = "Invalid Inventory Item Selected item #" + Counter;
                            Response.Errors.Add(error);
                        }
                    }


                    if (item.ReqQTY <= 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err49";
                        error.ErrorMSG = "Invalid Quantity selected for item #" + Counter;
                        Response.Errors.Add(error);
                    }

                    if (item.InventoryStoreItemIDsList == null || item.InventoryStoreItemIDsList.Count() == 0)
                    {
                        item.InventoryStoreItemIDsList = GetInvStoreIDAvailbileToReleaseHold(item.InventoryItemID,
                                                                                        (decimal)item.ReqQTY);
                    }
                    //if (item.InventoryStoreItemIDsList == null || item.InventoryStoreItemIDsList.Count() == 0)
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-44";
                    //    error.ErrorMSG = "InventoryStoreItemID is required ,because Request type is hold for item #" + Counter;
                    //    Response.Errors.Add(error);
                    //}
                    if (item.InventoryStoreItemIDsList != null && item.InventoryStoreItemIDsList.Count() > 0)
                    {
                        var CheckInventoryStoreItemListDB = InventoryStoreItemDBList.Where(x => item.InventoryStoreItemIDsList.Contains(x.Id)).ToList();
                        if (CheckInventoryStoreItemListDB.Count() > 0)
                        {
                            // Check Req Qty <= Qty on store item to make hold
                            if (item.ReqQTY > (CheckInventoryStoreItemListDB.Sum(x => (x.HoldQty ?? 0))))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-44";
                                error.ErrorMSG = "ReqQTY is greater than available Hold Qty for item #" + Counter;
                                Response.Errors.Add(error);
                            }
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-44";
                            error.ErrorMSG = "Invalid InventoryStoreItemIDs List for item #" + Counter;
                            Response.Errors.Add(error);
                        }
                    }


                    if (item.MatrialRequestItemID != null && item.MatrialRequestItemID != 0)
                    {
                        var MRItemObjDB = await _unitOfWork.InventoryMatrialRequestItems.FindAsync(x => x.Id == item.MatrialRequestItemID && x.InventoryMatrialRequestId == Request.MatrialRequestId);
                        if (MRItemObjDB == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-44";
                            error.ErrorMSG = "Invalid Matrial Request Item ID for item #" + Counter;
                            Response.Errors.Add(error);
                        }
                    }
                }



                if (Response.Result)
                {

                    foreach (var MatrialDataOBJ in Request.MatrialRequestItemList)
                    {
                        decimal RemainHoldQTY = (decimal)MatrialDataOBJ.ReqQTY; //50
                        foreach (var StoreItemID in MatrialDataOBJ.InventoryStoreItemIDsList)  // 20 -  10   - 5
                        {
                            var InventoryStoreItemObjDB = InventoryStoreItemDBList.Where(x => x.Id == StoreItemID).FirstOrDefault();
                            if (InventoryStoreItemObjDB != null)
                            {
                                decimal HoldQTY = 0;
                                decimal AvailableQTY = InventoryStoreItemObjDB.HoldQty ?? 0; //10
                                if (RemainHoldQTY <= AvailableQTY)
                                {
                                    HoldQTY = RemainHoldQTY;
                                }
                                else
                                {

                                    HoldQTY = AvailableQTY;
                                }



                                if (RemainHoldQTY > 0)
                                {
                                    RemainHoldQTY -= HoldQTY;

                                    InventoryStoreItemObjDB.HoldQty = (InventoryStoreItemObjDB.HoldQty ?? 0) - HoldQTY;
                                    InventoryStoreItemObjDB.ModifiedBy = validation.userID;
                                    InventoryStoreItemObjDB.ModifiedDate = DateTime.Now;

                                    _unitOfWork.Complete();
                                }
                            }
                        }



                        // Set Total QTY 
                        if (MatrialDataOBJ.MatrialRequestItemID != null && MatrialDataOBJ.MatrialRequestItemID != 0)
                        {
                            var MRItemObjDB = await _unitOfWork.InventoryMatrialRequestItems.FindAsync(x => x.Id == MatrialDataOBJ.MatrialRequestItemID);
                            if (MRItemObjDB != null)
                            {
                                MRItemObjDB.RecivedQuantity1 = MRItemObjDB.RecivedQuantity1 + (MatrialDataOBJ.ReqQTY - RemainHoldQTY);

                                _unitOfWork.Complete();
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


        public List<long> GetInvStoreIDAvailbileToReleaseHold(long InventoryItemID, decimal QTY)
        {
            List<long> InvStoreItemIDWithBalanceList = new List<long>();
            InvStoreItemIDWithBalanceList = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemID
                                                                                   && (x.HoldQty ?? 0) > 0
                                                                                   ).OrderBy(x => x.CreationDate).Select(x => x.Id).ToList();

            return InvStoreItemIDWithBalanceList;
        }

        public async Task<InventoryItemHoldDetailsResponse> GetInventoryItemHoldDetails(long InventoryItemID)
        {
            InventoryItemHoldDetailsResponse Response = new InventoryItemHoldDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                // var DataList = new List<InventoryMatrialRequestInfoForPaging>();
                if (Response.Result)
                {
                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Inventory Item is required";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    decimal RemainHoldQTY = 0;

                    // Basic Data  Matrial Request and Matrial Request Item
                    var MatrialRequestDetailsList = new List<MatrialRequestItemDetails>();
                    var MatrialRequestItemList = await _unitOfWork.VInventoryMatrialRequestWithItems.FindAllAsync(x => x.InventoryItemId == InventoryItemID &&
                                                                                                              x.RequestTypeId == 20003 &&
                                                                                                              (x.ReqQuantity1 ?? 0) - (x.RecivedQuantity1 ?? 0) > 0
                                                                                                              );
                    foreach (var item in MatrialRequestItemList)
                    {
                        var MRObj = new MatrialRequestItemDetails();
                        MRObj.MatrialRequestItemID = item.Id;
                        MRObj.MatrialRequestID = item.InventoryMatrialRequestId;
                        MRObj.MatrialRequestNo = item.InventoryMatrialRequestId.ToString();
                        MRObj.ProjectID = item.ProjectId;
                        MRObj.ProjectName = item.ProjectName;
                        MRObj.ProjectSerial = item.ProjectSerial;
                        MRObj.ClientName = item.ClientName;
                        MRObj.RemainHoldQTY = (decimal)((item.ReqQuantity1 ?? 0) - (item.RecivedQuantity1 ?? 0));

                        MatrialRequestDetailsList.Add(MRObj);
                        RemainHoldQTY += MRObj.RemainHoldQTY;
                    }

                    Response.MatrialRequestItemDetailsList = MatrialRequestDetailsList.Where(x => x.RemainHoldQTY > 0).ToList();
                    Response.TotalRemainHoldQTY = RemainHoldQTY;


                    // List Inv Store Item Rem QTY Hold
                    var InventoryStoreItemHoldQTYList = new List<InventoryStoreItemHoldQTY>();
                    var InventoryStoreItemDBList = await _Context.InventoryStoreItems.Where(x => x.InventoryItemId == InventoryItemID && x.HoldQty > 0).GroupBy(x => new { x.ItemSerial, x.ExpDate }).ToListAsync();
                    foreach (var item in InventoryStoreItemDBList)
                    {
                        var InvStoreItem = new InventoryStoreItemHoldQTY();
                        InvStoreItem.InventoryStoreItemIDsList = item.Select(x => x.Id).ToList();
                        InvStoreItem.Serial = item.Key.ItemSerial;
                        InvStoreItem.ExpDate = item.Key.ExpDate != null ? ((DateTime)item.Key.ExpDate).ToShortDateString() : null;
                        InvStoreItem.RemainHoldQTY = item.Sum(x => x.HoldQty ?? 0);
                        InventoryStoreItemHoldQTYList.Add(InvStoreItem);
                    }


                    Response.InventoryStoreItemHoldQTYList = InventoryStoreItemHoldQTYList;


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

        public AccountsAndFinanceInventoryStoreItemResponse GetAccountAndFinanceInventoryStoreItemReportList(GetAccountAndFinanceInventoryStoreItemReportListFilters filters)
        {
            AccountsAndFinanceInventoryStoreItemResponse Response = new AccountsAndFinanceInventoryStoreItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var InventoryStoreItemList = new List<InventoryStoreItemForReport>();
                //decimal TotalStockBalance = 0;
                //decimal TotalStockBalanceValue = 0;
                //decimal TotalUnitCost = 0;
                int NoOfItems = 0;

                if (Response.Result)
                {
                    //long InventoryStoreID = 0;
                    //if (!string.IsNullOrEmpty(Request.Headers["InventoryStoreID"]) && long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID))
                    //{
                    //    long.TryParse(Request.Headers["InventoryStoreID"], out InventoryStoreID);
                    //}

                    //int CurrentPage = 1;
                    //if (!string.IsNullOrEmpty(Request.Headers["CurrentPage"]) && int.TryParse(Request.Headers["CurrentPage"], out CurrentPage))
                    //{
                    //    int.TryParse(Request.Headers["CurrentPage"], out CurrentPage);
                    //}

                    //int NumberOfItemsPerPage = 10;
                    //if (!string.IsNullOrEmpty(Request.Headers["NumberOfItemsPerPage"]) && int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    //{
                    //    int.TryParse(Request.Headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    //}
                    decimal MinBalance = 0;
                    if (filters.LowStock != null)
                    {
                        MinBalance = filters.LowStock ?? 0;
                    }

                    decimal MaxBalance = 0;
                    if (filters.ExceedBalance != null)
                    {
                        MaxBalance = filters.ExceedBalance ?? 0;
                    }


                    //long POID = 0;
                    //if (!string.IsNullOrEmpty(headers["POID"]) && long.TryParse(headers["POID"], out POID))
                    //{
                    //    long.TryParse(headers["POID"], out POID);
                    //}

                    if (Response.Result)
                    {
                        // Not Grouped -----------------------------------------
                        //var InventoryStoreItemListDB = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        //if (InventoryStoreID != 0)
                        //{
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreID == InventoryStoreID).ToList();
                        //}
                        //if (headers["ItemSerial"] != null && headers["ItemSerial"] != "")
                        //{
                        //    string ItemSerial = headers["ItemSerial"];
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.Code == ItemSerial).ToList();
                        //}

                        //if (headers["SearchKey"] != null && headers["SearchKey"] != "")
                        //{
                        //    string SearchKey = headers["SearchKey"];
                        //    var ListItemIDFilter = _Context.V_InventoryStoreItem.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.Code.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.MarketName.ToLower().Contains(SearchKey.ToLower())
                        //                                                                || x.CommercialName.ToLower().Contains(SearchKey.ToLower())).Select(x => x.InventoryItemID).Distinct().ToList();

                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemID)).ToList();
                        //}

                        #region For PagedList
                        // var AllInventoryItemPriceAllList = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true).ToList();
                        var InventoryStoreItemListDB = _unitOfWork.VInventoryStoreItemPriceReports.FindAllQueryable(x => x.Active == true).OrderBy(a => a.InventoryItemId).AsQueryable();
                        var InventoryStoreItemWithFilterInventoryListDB = InventoryStoreItemListDB;
                        if (filters.InventoryStoreID != 0 || filters.InventoryStoreID != null)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == filters.InventoryStoreID);
                            InventoryStoreItemWithFilterInventoryListDB = InventoryStoreItemListDB.Where(x => x.InventoryStoreId == filters.InventoryStoreID);
                        }
                        if (!string.IsNullOrEmpty(filters.ItemSerial))
                        {

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.Code == filters.ItemSerial);
                        }
                        if (MinBalance != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.MinBalance < MinBalance).AsQueryable();
                        }

                        if (MaxBalance != 0)
                        {
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.MaxBalance > MaxBalance).AsQueryable();
                        }
                        //repated headers 
                        //bool LowStock = false;
                        //if (!string.IsNullOrEmpty(Request.Headers["LowStock"]) && bool.TryParse(Request.Headers["LowStock"], out LowStock))
                        //{
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0 && x.FinalBalance < x.MinBalance).AsQueryable();
                        //}
                        //bool ExceedBalance = false;
                        //if (!string.IsNullOrEmpty(Request.Headers["ExceedBalance"]) && bool.TryParse(Request.Headers["ExceedBalance"], out ExceedBalance))
                        //{
                        //    InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.FinalBalance > 0 && x.FinalBalance > x.MaxBalance).AsQueryable();
                        //}


                        if (filters.IsExpDate != null)
                        {
                            if (filters.IsExpDate == true)
                            {
                                InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => x.ExpDate < DateTime.Now).AsQueryable();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.SearchKey))
                        {
                            //string SearchKey = Request.Headers["SearchKey"];
                            var SearchKey = HttpUtility.UrlDecode(filters.SearchKey);

                            var ListItemIDFilter = _unitOfWork.VInventoryStoreItems.FindAll(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.Code.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.MarketName.ToLower().Contains(SearchKey.ToLower())
                                                                                        || x.CommercialName.ToLower().Contains(SearchKey.ToLower())).Select(x => x.InventoryItemId).Distinct().ToList();

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => ListItemIDFilter.Contains(x.InventoryItemId));
                        }
                        if (!string.IsNullOrEmpty(filters.ChapterName))
                        {

                            //string ChapterName = Request.Headers["ChapterName"]; 
                            var ChapterName = HttpUtility.UrlDecode(filters.ChapterName);
                            var preparedChapterName = Common.string_compare_prepare_function(ChapterName);
                            var itemsIds = _unitOfWork.InventoryItemContents.FindAll(a => a.PreparedSearchName.ToLower().Trim().Contains(preparedChapterName.ToLower().Trim())).Select(a => a.InventoryItemId).ToList();

                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(a => itemsIds.Contains(a.InventoryItemId)).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.MatrialAddingOrderSerial))
                        {
                            //string MatrialAddingOrderSerial = Request.Headers["MatrialAddingOrderSerial"];
                            var IDSInventoryItemList = _unitOfWork.VInventoryAddingOrderItems.FindAll(x => x.ItemSerial.Trim() == filters.MatrialAddingOrderSerial.Trim()).Select(x => x.InventoryItemId).ToList();
                            InventoryStoreItemListDB = InventoryStoreItemListDB.Where(x => IDSInventoryItemList.Contains(x.InventoryItemId));
                        }

                        var InventoryStoreItemDBList = InventoryStoreItemListDB.Select(a => a.InventoryItemId).ToList();
                        var InventoryMatrialRequestWithItems = _unitOfWork.VInventoryMatrialRequestWithItems.FindAll(a => InventoryStoreItemDBList.Contains(a.InventoryItemId));

                        var InventoryStoreItemPriceDistinctList = InventoryStoreItemListDB.Select(x => new InventoryStoreItemForReport
                        {
                            ID = x.InventoryItemId,
                            InventoryStoreId = x.InventoryStoreId,
                            ItemName = x.InventoryItemName,
                            InventoryStoreName = x.InventoryStoreName,
                            HoldQTY = InventoryMatrialRequestWithItems.Where(a => a.InventoryItemId == x.InventoryItemId && a.ToInventoryStoreId == x.InventoryStoreId).Sum(a => a.ReqQuantity1),
                            OpenPOQTY = x.ReqQuantity > x.RecivedQuantity ? x.ReqQuantity - x.RecivedQuantity : 0,
                            ////V_PurchasePoItem_PO.Where(a => a.InventoryItemID == x.InventoryItemID && a.Status == "Open" && a.ReqQuantity != null).Sum(a => a.ReqQuantity) >=
                            //           V_InventoryAddingOrderItems.Where(a => a.InventoryItemID == x.InventoryItemID && a.RecivedQuantity != null).Sum(a => a.RecivedQuantity) ?
                            //           V_PurchasePoItem_PO.Where(a => a.InventoryItemID == x.InventoryItemID && a.Status == "Open" && a.ReqQuantity != null).Sum(a => a.ReqQuantity) -
                            //           V_InventoryAddingOrderItems.Where(a => a.InventoryItemID == x.InventoryItemID && a.RecivedQuantity != null).Sum(a => a.RecivedQuantity)
                            //           : 0,
                            Active = x.Active ?? false,
                            ItemCode = x.Code,
                            MinStock = x.MinBalance,
                            MaxStock = x.MaxBalance,
                            RequestionUOMShortName = x.RequestionUomshortName,
                            StockBalance = x.StockBalance ?? 0,// InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.Balance != null).Sum(a => a.Balance) ?? 0,
                            StockBalanceValue = 0,
                            //InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMAverageUnitPrice != null && a.CalculationType == 1).Select(a => a.SUMAverageUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMMaxUnitPrice != null && a.CalculationType == 2).Select(a => a.SUMMaxUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMLastUnitPrice != null && a.CalculationType == 3).Select(a => a.SUMLastUnitPrice).Sum() ?? 0 +
                            //                    InventoryStoreItemListDB.Where(a => a.InventoryItemID == x.InventoryItemID && a.InventoryStoreID == x.InventoryStoreID && a.SUMCustomeUnitPrice != null && a.CalculationType == 4).Select(a => a.SUMCustomeUnitPrice).Sum() ?? 0,
                            UnitCost = x.CustomeUnitPrice != null ? (decimal)x.CustomeUnitPrice : 0,
                        }).Distinct().AsQueryable();

                        //var IDInventoryItemList = InventoryStoreItemListDB.Select(x => new InventoryStoreItemPaging
                        //{
                        //    InventoryItemID = x.InventoryItemID,
                        //    SUMAverageUnitPrice = x.SUMAverageUnitPrice,
                        //    SUMMaxUnitPrice = x.SUMMaxUnitPrice,
                        //    SUMLastUnitPrice = x.SUMLastUnitPrice,
                        //    SUMCustomeUnitPrice = x.SUMCustomeUnitPrice,
                        //    InventoryItemName = x.InventoryItemName,
                        //    Code = x.Code,
                        //    RequestionUOMShortName = x.RequestionUOMShortName,
                        //    Balance = x.Balance,
                        //    CalculationType = x.CalculationType,
                        //    CustomeUnitPrice = x.CustomeUnitPrice
                        //}).Distinct().AsQueryable();
                        //var InventoryStoreItemPagingList = PagedList<InventoryStoreItemPaging>.Create(IDInventoryItemList, CurrentPage, NumberOfItemsPerPage);
                        //var InventoryStoreItemPagingList = PagedList<V_InventoryStoreItemPrice>.Create(InventoryStoreItemListDB, CurrentPage, NumberOfItemsPerPage);
                        InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ID);
                        if (!string.IsNullOrEmpty(filters.SortBy))
                        {
                            if (filters.SortBy == "ItemName")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ItemName);
                            }
                            else if (filters.SortBy == "ItemCode")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.ItemCode);
                            }
                            else if (filters.SortBy == "StoreName")
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.OrderBy(x => x.InventoryStoreName);
                            }
                        }

                        //DateTime NotReleasedFrom = new DateTime(DateTime.Now.Year, 1, 1);
                        //if (filters.NotReleasedFrom != null)
                        //{
                        //    bool hasfrom = DateTime.TryParse(filters.NotReleasedFrom, out NotReleasedFrom);
                        //}

                        /*Not Release duration date from and to*/
                        var InventoryItemIDs = new List<long>();
                        if (filters.NotReleasedFrom != null)
                        {
                            InventoryItemIDs = _unitOfWork.InventoryStoreItems.FindAll(x => x.OperationType.Contains("Release Order"))
                                               .GroupBy(item => item.InventoryItemId)
                                               .Select(group => group.OrderByDescending(item => item.CreationDate).FirstOrDefault()).Where(x => x.CreationDate <= filters.NotReleasedFrom).Select(x => x.InventoryItemId)
                                               .Distinct().ToList();
                            //InventoryItemIDs =  _Context.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRelease.CreationDate >= NotReleasedFrom && x.InventoryMatrialRelease.CreationDate <= NotReleasedTo).Select(x => x.InventoryMatrialRequestItem.InventoryItemID).ToList();
                            if (InventoryItemIDs != null)
                            {
                                InventoryStoreItemPriceDistinctList = InventoryStoreItemPriceDistinctList.Where(x => InventoryItemIDs.Contains(x.ID) && x.StockBalance > 0).AsQueryable();
                            }
                        }

                        var InventoryStoreItemPagingList = PagedList<InventoryStoreItemForReport>.Create(InventoryStoreItemPriceDistinctList, filters.CurrentPage, filters.NumberOfItemsPerPage);

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = InventoryStoreItemPagingList.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = InventoryStoreItemPagingList.TotalCount
                        };
                        #endregion



                        // New for paging Calc
                        if (InventoryStoreItemPagingList.Count > 0)
                        {

                            foreach (var item in InventoryStoreItemPagingList)
                            {
                                var InventoryStoreItemListDBForCalc = InventoryStoreItemListDB.Where(a => a.InventoryItemId == item.ID && a.InventoryStoreId == item.InventoryStoreId).AsQueryable();
                                var V_InventoryItemObjDB = _unitOfWork.VInventoryItems.Find(x => x.Id == item.ID);
                                if (V_InventoryItemObjDB != null)
                                {

                                    var ItemOBJ = new InventoryStoreItemForReport();
                                    ItemOBJ.ID = item.ID;
                                    ItemOBJ.InventoryStoreName = item.InventoryStoreName;
                                    ItemOBJ.HoldQTY = item.HoldQTY;
                                    ItemOBJ.OpenPOQTY = item.OpenPOQTY;
                                    ItemOBJ.Active = item.Active;
                                    ItemOBJ.ItemCode = item.ItemCode;
                                    ItemOBJ.ItemName = item.ItemName;
                                    ItemOBJ.UnitCost = item.UnitCost;
                                    ItemOBJ.RequestionUOMShortName = item.RequestionUOMShortName;
                                    ItemOBJ.StockBalance = item.StockBalance;
                                    ItemOBJ.MinStock = item.MinStock;
                                    ItemOBJ.MaxStock = item.MaxStock;
                                    ItemOBJ.StockBalanceValue = InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 1).Sum(a => a.SumaverageUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 2).Sum(a => a.SummaxUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 3).Sum(a => a.SumlastUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDBForCalc.Where(x => x.CalculationType == 4).Sum(a => a.SumcustomeUnitPrice) ?? 0;
                                    //InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMAverageUnitPrice != null && a.CalculationType == 1).Select(a => a.SUMAverageUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMMaxUnitPrice != null && a.CalculationType == 2).Select(a => a.SUMMaxUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMLastUnitPrice != null && a.CalculationType == 3).Select(a => a.SUMLastUnitPrice).DefaultIfEmpty(0).Sum() ?? 0 +
                                    //            InventoryStoreItemListDB.Where(a => a.InventoryItemID == item.ID && a.InventoryStoreID == item.InventoryStoreId && a.SUMCustomeUnitPrice != null && a.CalculationType == 4).Select(a => a.SUMCustomeUnitPrice).DefaultIfEmpty(0).Sum() ?? 0;
                                    ItemOBJ.ExpDate = Common.GetInventoryItemExpDateFromMatrialAddingOrder(item.ID, _Context);

                                    ItemOBJ.MarketName = V_InventoryItemObjDB.MarketName ?? "";
                                    ItemOBJ.Category = V_InventoryItemObjDB.CategoryName;
                                    ItemOBJ.RUOM = V_InventoryItemObjDB.RequestionUomshortName;
                                    ItemOBJ.CommercialName = V_InventoryItemObjDB.CommercialName ?? "";
                                    ItemOBJ.PartNO = V_InventoryItemObjDB.PartNo ?? "";
                                    ItemOBJ.Cost1 = V_InventoryItemObjDB.CostAmount1 ?? 0;
                                    ItemOBJ.Cost2 = V_InventoryItemObjDB.CostAmount2 ?? 0;
                                    ItemOBJ.Cost3 = V_InventoryItemObjDB.CostAmount3 ?? 0;
                                    ItemOBJ.ItemSerialCounter = V_InventoryItemObjDB.ItemSerialCounter != null ? V_InventoryItemObjDB.ItemSerialCounter.ToString() : "";

                                    // fill Inventory List ------------------
                                    InventoryStoreItemList.Add(ItemOBJ);
                                }
                            }
                            NoOfItems += InventoryStoreItemPagingList.Select(x => x.ID).Distinct().Count();
                        }






                        //if (InventoryStoreItemPagingList.Count > 0)
                        //{

                        //    var InventoryItemList = InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().ToList();
                        //    if (headers["MatrialAddingOrderSerial"] != null && headers["MatrialAddingOrderSerial"] != "")
                        //    {
                        //        InventoryItemList = null;
                        //        string MatrialAddingOrderSerial = headers["MatrialAddingOrderSerial"];
                        //        InventoryItemList = _Context.V_InventoryAddingOrderItems.Where(x => x.ItemSerial.Trim() == MatrialAddingOrderSerial.Trim()).Select(x => x.InventoryItemID).ToList();
                        //    }
                        //    foreach (var InventoryItemID in InventoryItemList)
                        //    {

                        //        var InventoryStoreItemOBJ = new InventoryStoreItem();
                        //        decimal StockBalanceValue = 0;

                        //        int CalculationType = 0;
                        //        var ItemPErInventoryObj = InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID).FirstOrDefault();
                        //        if (ItemPErInventoryObj != null)
                        //        {
                        //            CalculationType = ItemPErInventoryObj.CalculationType != null ? (int)ItemPErInventoryObj.CalculationType : 0;
                        //            InventoryStoreItemOBJ.ID = ItemPErInventoryObj.InventoryItemID;
                        //            InventoryStoreItemOBJ.ItemName = ItemPErInventoryObj.InventoryItemName;
                        //            InventoryStoreItemOBJ.ItemCode = ItemPErInventoryObj.Code;
                        //            InventoryStoreItemOBJ.RequestionUOMShortName = ItemPErInventoryObj.RequestionUOMShortName;
                        //        }
                        //        if (CalculationType == 1)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMAverageUnitPrice != null).Sum(x => (decimal)x.SUMAverageUnitPrice);
                        //        }
                        //        else if (CalculationType == 2)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMMaxUnitPrice != null).Sum(x => (decimal)x.SUMMaxUnitPrice);
                        //        }
                        //        else if (CalculationType == 3)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMLastUnitPrice != null).Sum(x => (decimal)x.SUMLastUnitPrice);
                        //        }
                        //        else if (CalculationType == 4)
                        //        {
                        //            StockBalanceValue += InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.SUMCustomeUnitPrice != null).Sum(x => (decimal)x.SUMCustomeUnitPrice);
                        //        }
                        //        InventoryStoreItemOBJ.StockBalance = InventoryStoreItemPagingList.Where(x => x.InventoryItemID == InventoryItemID && x.Balance != null).Sum(x => (decimal)x.Balance);
                        //        InventoryStoreItemOBJ.StockBalanceValue = StockBalanceValue;  // Unit Cost *  Qty Balance
                        //        InventoryStoreItemOBJ.UnitCost = ItemPErInventoryObj.CustomeUnitPrice != null ? (decimal)ItemPErInventoryObj.CustomeUnitPrice : 0;   // Unit Cost 
                        //        InventoryStoreItemOBJ.ExpDate = Common.GetInventoryItemExpDateFromMatrialAddingOrder(InventoryItemID);

                        //        // fill Inventory List ------------------
                        //        InventoryStoreItemList.Add(InventoryStoreItemOBJ);
                        //        TotalStockBalance += InventoryStoreItemOBJ.StockBalance;
                        //        TotalStockBalanceValue += InventoryStoreItemOBJ.StockBalanceValue;
                        //    }
                        //    NoOfItems += InventoryStoreItemPagingList.Select(x => x.InventoryItemID).Distinct().Count();
                        //}


                        Response.TotalItems = Common.GetNoOFInventoryItem(_Context);
                        Response.TotalPricedItems = InventoryStoreItemWithFilterInventoryListDB.Select(x => x.InventoryItemId).Distinct().Count();
                        Response.TotalStockBalance = InventoryStoreItemListDB.Select(x => x.StockBalance).Sum() ?? 0;
                        Response.TotalStockBalanceValue =
                                                                 InventoryStoreItemListDB.Where(x => x.CalculationType == 1).Sum(a => a.SumaverageUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 2).Sum(a => a.SummaxUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 3).Sum(a => a.SumlastUnitPrice) ?? 0 +
                                                                InventoryStoreItemListDB.Where(x => x.CalculationType == 4).Sum(a => a.SumcustomeUnitPrice) ?? 0;


                        //InventoryStoreItemListDB.Select(x => x.StockBalanceValue).Sum();

                    }
                    Response.InventoryStoreItemList = InventoryStoreItemList;
                    Response.NoOfItems = NoOfItems;


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

        public InventortyStoreItemFullDataListResponse GetInventoryStoreItemFullDataList(GetInventoryStoreItemFullDataListFilters filters)
        {
            InventortyStoreItemFullDataListResponse Response = new InventortyStoreItemFullDataListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryStorItemFullDataList = new List<Infrastructure.Models.ItemsPricing.InventoryStoreItemFullData>();
                if (Response.Result)
                {
                    var InventoryItemListDB = _unitOfWork.VInventoryStoreItems.FindAllQueryable(x => x.Active == true && x.StoreActive == true);

                    if (!string.IsNullOrEmpty(filters.OperationType) && filters.OperationType.Trim().ToLower() == "openingbalance")
                    {
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.OperationType == "Opening Balance");
                    }

                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {
                        filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey).ToLower();
                        InventoryItemListDB = InventoryItemListDB.Where(x =>
                            x.InventoryItemName.ToLower().StartsWith(filters.SearchKey) ||
                            (x.Code != null && x.Code.ToLower().StartsWith(filters.SearchKey)) ||
                            (x.PartNo != null && x.PartNo.ToLower().StartsWith(filters.SearchKey))
                        );
                    }

                    if (filters.InventoryStoreID != null && filters.InventoryStoreID != 0)
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == filters.InventoryStoreID);

                    if (filters.PriorityID != null && filters.PriorityID != 0)
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.PriorityId == filters.PriorityID);

                    if (filters.InventoryItemCategoryID != null && filters.InventoryItemCategoryID != 0)
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemCategoryId == filters.InventoryItemCategoryID);

                    if (filters.InventoryItemID != null && filters.InventoryItemID != 0)
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemId == filters.InventoryItemID);

                    if (filters.SupplierID != null && filters.SupplierID != 0)
                    {
                        var IDSListInventoryItem = _unitOfWork.VPurchasePoItems
                            .FindAll(x => x.ToSupplierId == filters.SupplierID)
                            .Select(x => x.InventoryItemId)
                            .Distinct()
                            .ToList();

                        InventoryItemListDB = InventoryItemListDB.Where(x => IDSListInventoryItem.Contains(x.InventoryItemId));
                    }

                    if (filters.NotPricidBefore == true)
                    {
                        InventoryItemListDB = InventoryItemListDB.Where(x =>
                            x.CustomeUnitPrice == 0 &&
                            (x.CostAmount1 == null || x.CostAmount1 == 0) &&
                            (x.CostAmount2 == null || x.CostAmount2 == 0) &&
                            (x.CostAmount3 == null || x.CostAmount3 == 0)
                        );
                    }
                    var distinctData = InventoryItemListDB.Select(x => new
                    {
                        x.InventoryItemId,
                        x.InventoryItemName,
                        x.Active,
                        x.Code,
                        x.InventoryStoreName,
                        x.PartNo,
                        x.CategoryName,
                        x.ItemSerialCounter,
                        x.AverageUnitPrice,
                        x.LastUnitPrice,
                        x.MaxUnitPrice,
                        x.CustomeUnitPrice,
                        x.CostAmount1,
                        x.CostAmount2,
                        x.CostAmount3
                    }).Distinct().AsQueryable();
                    var ListOfInventoryItemFullData = distinctData
                        .OrderBy(x => x.InventoryItemName)
                        .Skip((filters.CurrentPage - 1) * filters.NumberOfItemsPerPage)
                        .Take(filters.NumberOfItemsPerPage)
                        .Select(x => new Infrastructure.Models.ItemsPricing.InventoryStoreItemFullData
                        {
                            ID = x.InventoryItemId,
                            ItemName = x.InventoryItemName,
                            Active = x.Active ?? false,
                            ItemCode = x.Code,
                            InventoryStoreName = x.InventoryStoreName,
                            PartNO = x.PartNo,
                            Category = x.CategoryName,
                            ItemSerialCounter = x.ItemSerialCounter != null ? x.ItemSerialCounter.ToString() : "",
                            AverageUnitPrice = x.AverageUnitPrice ?? 0,
                            LastUnitPrice = x.LastUnitPrice ?? 0,
                            MaxUnitPrice = x.MaxUnitPrice ?? 0,
                            CustomeUnitPrice = x.CustomeUnitPrice ?? 0,
                            UnitCost = x.CustomeUnitPrice ?? 0,
                            Cost1 = x.CostAmount1 ?? 0,
                            Cost2 = x.CostAmount2 ?? 0,
                            Cost3 = x.CostAmount3 ?? 0
                        })
                        .ToList();

                    // var InventoryStoreItemPagingList = PagedList<Infrastructure.Models.ItemsPricing.InventoryStoreItemFullData>
                    //     .Create(ListOfInventoryItemFullData, filters.CurrentPage, filters.NumberOfItemsPerPage);

                    //Response.PaginationHeader = new PaginationHeader
                    //{
                    //    CurrentPage = filters.CurrentPage,
                    //    TotalPages = InventoryStoreItemPagingList.TotalPages,
                    //    ItemsPerPage = filters.NumberOfItemsPerPage,
                    //    TotalItems = InventoryStoreItemPagingList.TotalCount
                    //};


                    var totalItems = InventoryItemListDB.Count();
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = (int)Math.Ceiling((double)totalItems / filters.NumberOfItemsPerPage), // Corrected total pages
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = totalItems // Corrected total items
                    };



                    Response.TotalCount = totalItems;
                    Response.InventoryStorItemFullDataList = ListOfInventoryItemFullData.ToList();
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


        //public IQueryable<Infrastructure.Models.ItemsPricing.InventoryStoreItemFullData> GetInventoryStoreItemFullInfoList(string OperationType, string SearchKey, int InventoryStoreID, int PriorityID, int InventoryItemCategoryID, long InventoryItemID, long SupplierID, bool NotPricidBefore)
        //{


        //    var InventoryItemListDB = _unitOfWork.VInventoryStoreItems.FindAllQueryable(x => x.Active == true && x.StoreActive == true).AsQueryable();
        //    if (!string.IsNullOrEmpty(OperationType))
        //    {
        //        if (OperationType.Trim().ToLower() == "openingbalance")
        //        {

        //            InventoryItemListDB = InventoryItemListDB.Where(x => x.OperationType == "Opening Balance");
        //        }
        //    }
        //    if (!string.IsNullOrEmpty(SearchKey))
        //    {
        //        SearchKey = HttpUtility.UrlDecode(SearchKey);
        //        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
        //                                || (x.Code != null ? x.Code.ToLower().Contains(SearchKey.ToLower()) : false)
        //                                || (x.PartNo != null ? x.PartNo.ToLower().Contains(SearchKey.ToLower()) : false)
        //                                ).AsQueryable();
        //    }
        //    if (InventoryStoreID != 0)
        //    {
        //        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
        //    }

        //    if (PriorityID != 0)
        //    {
        //        InventoryItemListDB = InventoryItemListDB.Where(x => x.PriorityId == PriorityID).AsQueryable();
        //    }
        //    if (InventoryItemCategoryID != 0)
        //    {
        //        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemCategoryId == InventoryItemCategoryID).AsQueryable();
        //    }
        //    if (InventoryItemID != 0)
        //    {
        //        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemId == InventoryItemID).AsQueryable();
        //    }
        //    if (SupplierID != 0)
        //    {
        //        var IDSListInventoryItem = _unitOfWork.VPurchasePoItems.FindAll(x => x.ToSupplierId == SupplierID).Select(x => x.InventoryItemId).Distinct().ToList();
        //        InventoryItemListDB = InventoryItemListDB.Where(x => IDSListInventoryItem.Contains(x.Id)).AsQueryable();
        //    }
        //    if (NotPricidBefore == true)
        //    {
        //        InventoryItemListDB = InventoryItemListDB.Where(x => x.CustomeUnitPrice == 0 && (x.CostAmount1 == null || x.CostAmount1 == 0) && (x.CostAmount2 == null || x.CostAmount2 == 0) && (x.CostAmount3 == null || x.CostAmount3 == 0)).AsQueryable();
        //    }
        //    var ListOfInventoryItemFullData = InventoryItemListDB.Select(x => new Infrastructure.Models.ItemsPricing.InventoryStoreItemFullData
        //    {
        //        ID = x.InventoryItemId,
        //        ItemName = x.InventoryItemName,
        //        Active = x.Active ?? false,
        //        ItemCode = x.Code,
        //        InventoryStoreName = x.InventoryStoreName,
        //        PartNO = x.PartNo,
        //        Category = x.CategoryName,
        //        ItemSerialCounter = x.ItemSerialCounter != null ? x.ItemSerialCounter.ToString() : "",
        //        AverageUnitPrice = x.AverageUnitPrice ?? 0,
        //        LastUnitPrice = x.LastUnitPrice ?? 0,
        //        Balance = x.Balance,
        //        MaxUnitPrice = x.MaxUnitPrice ?? 0,
        //        CustomeUnitPrice = x.CustomeUnitPrice ?? 0,
        //        UnitCost = x.CustomeUnitPrice ?? 0,
        //        Cost1 = x.CostAmount1 ?? 0,
        //        Cost2 = x.CostAmount2 ?? 0,
        //        Cost3 = x.CostAmount3 ?? 0
        //    }).Distinct().AsQueryable().OrderBy(x => x.ItemName);


        //    return ListOfInventoryItemFullData;
        //}

        public InventortyStoreListResponse GetInventoryStoreList(string Type, long userId)
        {
            InventortyStoreListResponse Response = new InventortyStoreListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                bool ViewNotAllowed = false;
                if (!string.IsNullOrEmpty(Type))
                {
                    if (Type == "OpeningBalance" || Type == "ExternalBackOrder" || Type == "AddingOrder")
                    {
                        ViewNotAllowed = true;
                    }
                }
                var DDLList = new List<InventoryStoreDDL>();
                if (Response.Result)
                {
                    if (userId != 0)
                    {
                        long clientId = userId;
                        if (CheckUserRole(clientId, 113))
                        {
                            var storesId = _Context.InventoryStoreKeepers.Where(x => x.Active && x.UserId == clientId).ToList();
                            foreach (var id in storesId)
                            {
                                var DLLObj = new InventoryStoreDDL();
                                var store = _Context.InventoryStores.Where(x => x.Active && x.Id == id.InventoryStoreId).FirstOrDefault();
                                DLLObj.ID = store.Id;
                                DLLObj.Name = store.Name;
                                if (ViewNotAllowed)
                                {
                                    DLLObj.NotAllowed = CheckInventoryStoreAllowedToUse(id.InventoryStoreId);
                                }
                                DDLList.Add(DLLObj);
                            }

                        }
                    }
                    else
                    {
                        var ListDB = _unitOfWork.InventoryStores.FindAll(x => x.Active == true).ToList();
                        if (ListDB.Count() > 0)
                        {
                            foreach (var item in ListDB)
                            {
                                var DLLObj = new InventoryStoreDDL();
                                DLLObj.ID = item.Id;
                                DLLObj.Name = item.Name;
                                if (ViewNotAllowed)
                                {
                                    DLLObj.NotAllowed = CheckInventoryStoreAllowedToUse(item.Id);
                                }
                                DDLList.Add(DLLObj);
                            }
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


        public bool CheckUserRole(long UserId, int RoleID)
        {
            bool Res = false;
            var LoadObjDB = _unitOfWork.UserRoles.Find(x => x.UserId == UserId && x.RoleId == RoleID);
            if (LoadObjDB != null)
            {
                Res = true;
            }
            return Res;
        }

        public bool CheckInventoryStoreAllowedToUse(long StoreID)
        {
            bool NotAllowed = false;
            var CheckInventoryReportListDB = _unitOfWork.InventoryReports.FindAll(x => x.InventoryStoreId == StoreID && x.Active == true && x.Approved == false && x.Closed == false).ToList();
            if (CheckInventoryReportListDB.Count > 0)
            {
                foreach (var InventoryRep in CheckInventoryReportListDB)
                {
                    if (InventoryRep.DateFrom <= System.DateTime.Now && InventoryRep.DateTo >= System.DateTime.Now)
                    {
                        NotAllowed = true;
                    }
                }
            }
            return NotAllowed;
        }

        public InventortyStoreLocationListResponse GetInventoryStoreLocationList(long InventoryStoreID, long? InventoryItemID)
        {
            InventortyStoreLocationListResponse Response = new InventortyStoreLocationListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<SelectDDL>();
                if (Response.Result)
                {
                    if (InventoryStoreID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err317";
                        error.ErrorMSG = "Invalid Inventory Store ID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (InventoryItemID != null)
                    {
                        //long.TryParse(headers["InventoryItemID"], out InventoryItemID);

                        // To Get Last Release (item.OperationType == "Add New Matrial") (item.OperationType.Contains("Release Order"))
                        Response.LastReleased = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryStoreId == InventoryStoreID && x.InventoryItemId == InventoryItemID && x.OperationType.Contains("Release Order")).OrderByDescending(x => x.CreationDate).Select(x => x.InvenoryStoreLocationId).FirstOrDefault();
                        Response.LastAdded = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryStoreId == InventoryStoreID && x.InventoryItemId == InventoryItemID && x.OperationType.Contains("Add New Matrial")).OrderByDescending(x => x.CreationDate).Select(x => x.InvenoryStoreLocationId).FirstOrDefault();
                    }
                    var ListDB = _unitOfWork.InventoryStoreLocations.FindAll(x => x.Active == true && x.InventoryStoreId == InventoryStoreID).ToList();
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new SelectDDL();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Location;

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

        public AccountsAndFinanceInventoryItemInfoResponse GetAccountAndFinanceInventoryStoreItemInfo([FromHeader] long InventoryItemID, [FromHeader] string InventoryItemCode)
        {
            AccountsAndFinanceInventoryItemInfoResponse Response = new AccountsAndFinanceInventoryItemInfoResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemInfoObj = new InventoryItemInfo();
                if (Response.Result)
                {
                    if (InventoryItemCode == null && InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Must be select At least One Id or Code";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Response.Result)
                    {
                        InventoryItem InventoryItemObjDB = null;
                        if (InventoryItemID != 0)
                        {
                            InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == InventoryItemID, includes: new[] { "InventoryItemCategory", "PurchasingUom", "RequstionUom" }).FirstOrDefault();
                        }
                        else
                        {
                            InventoryItemObjDB = _unitOfWork.InventoryItems.FindAll(x => x.Code == InventoryItemCode).FirstOrDefault();
                        }

                        if (InventoryItemObjDB != null)
                        {
                            InventoryItemInfoObj.ID = InventoryItemObjDB.Id;
                            InventoryItemInfoObj.ItemName = InventoryItemObjDB.Name;
                            InventoryItemInfoObj.ItemCode = InventoryItemObjDB.Code;
                            InventoryItemInfoObj.CommericalName = InventoryItemObjDB.CommercialName;
                            InventoryItemInfoObj.MarketName = InventoryItemObjDB.MarketName;
                            InventoryItemInfoObj.Type = InventoryItemObjDB.Exported;
                            InventoryItemInfoObj.Description = InventoryItemObjDB.Description;
                            InventoryItemInfoObj.Details = InventoryItemObjDB.Details;
                            InventoryItemInfoObj.Category = InventoryItemObjDB.InventoryItemCategory?.Name; //Common.GetInventoryItemCategory(InventoryItemObjDB.InventoryItemCategoryID);
                            InventoryItemInfoObj.PurchasingUnit = InventoryItemObjDB.PurchasingUom?.Name; //Common.GetInventoryUOM(InventoryItemObjDB.PurchasingUOMID);
                            InventoryItemInfoObj.RequestionUnit = InventoryItemObjDB.RequstionUom?.Name; //Common.GetInventoryUOM(InventoryItemObjDB.RequstionUOMID);
                            InventoryItemInfoObj.PriceCalculationMethod = Common.GetIncentoryCalculationMethod(InventoryItemObjDB.CalculationType, _Context);

                            InventoryItemInfoObj.MinBalance = InventoryItemObjDB.MinBalance1 != null ? (decimal)InventoryItemObjDB.MinBalance1 : 0;
                            InventoryItemInfoObj.MaxBlanace = InventoryItemObjDB.MaxBalance1 != null ? (decimal)InventoryItemObjDB.MaxBalance1 : 0;
                            InventoryItemInfoObj.ConvertRateFromPurchasingToRequestionUnit = InventoryItemObjDB.ExchangeFactor1 != null ? (decimal)InventoryItemObjDB.ExchangeFactor1 : 0;
                            InventoryItemInfoObj.Active = InventoryItemObjDB.Active;
                            InventoryItemInfoObj.Cost1 = InventoryItemObjDB.CostAmount1;
                            InventoryItemInfoObj.Cost2 = InventoryItemObjDB.CostAmount2;
                            InventoryItemInfoObj.Cost3 = InventoryItemObjDB.CostAmount3;
                            InventoryItemInfoObj.RequestionUOMShortName = InventoryItemObjDB.RequstionUom?.ShortName;

                            if (InventoryItemObjDB.ImageUrl != null && InventoryItemObjDB.HasImage == true)
                            {
                                InventoryItemInfoObj.ItemImage = Globals.baseURL + "/" + InventoryItemObjDB.ImageUrl;
                            }
                            decimal Amount = 0;
                            int CalcType = InventoryItemObjDB.CalculationType;
                            if (CalcType == 1)
                            {
                                Amount = InventoryItemObjDB.AverageUnitPrice;
                            }
                            else if (CalcType == 2)
                            {
                                Amount = InventoryItemObjDB.MaxUnitPrice;
                            }
                            else if (CalcType == 3)
                            {
                                Amount = InventoryItemObjDB.LastUnitPrice;
                            }
                            else if (CalcType == 4)
                            {
                                Amount = InventoryItemObjDB.CustomeUnitPrice;
                            }
                            InventoryItemInfoObj.Amount = Amount;

                            Response.InventoryItemInfo = InventoryItemInfoObj;
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

        public async Task<GetInventoryItemContentTreeResponse> GetInventoryItemContentTree([FromHeader] long? InventoryItemId)
        {
            GetInventoryItemContentTreeResponse Response = new GetInventoryItemContentTreeResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var InventoryItemContent = await _unitOfWork.InventoryItemContents.GetAllAsync();
                if (InventoryItemId != null)
                {
                    InventoryItemContent = InventoryItemContent.Where(a => a.InventoryItemId == InventoryItemId).ToList();
                }
                var TreeDtoObj = InventoryItemContent.Select(c => new TreeViewDto2
                {
                    id = c.Id.ToString(),
                    title = c.ChapterName,
                    HasChild = c.Haveitem,
                    parentId = c.ParentContentId.ToString()
                }).ToList();

                var trees = Common.BuildTreeViews2("", TreeDtoObj);

                Response.GetInventoryItemCategoryList = trees;

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

        public BaseResponseWithId<long> DeleteInventoryItemContent([FromHeader] long InventoryItemContentId)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };
            try
            {
                if (InventoryItemContentId == 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Inventory Item Content Id IS required";
                    response.Errors.Add(error);
                    return response;
                }
                var content = _unitOfWork.InventoryItemContents.GetById(InventoryItemContentId);
                if (content == null)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Inventory Item Content not found";
                    response.Errors.Add(error);
                    return response;
                }
                var childs = _unitOfWork.InventoryItemContents.FindAll(a => a.ParentContentId == InventoryItemContentId).ToList();
                if (childs.Count > 0)
                {
                    response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Inventory Item Has Children and can't be deleted";
                    response.Errors.Add(error);
                    return response;
                }
                _unitOfWork.InventoryItemContents.Delete(content);

                _unitOfWork.Complete();

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

        public async Task<BaseResponseWithId<long>> AddInventoryItemContent(AddInventoryItemContentDto request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryItemListQuerable = _unitOfWork.InventoryItemContents.FindAllQueryable(x => x.Active == true).AsQueryable();
                InventoryItemContent InventoryItemContentDB = new InventoryItemContent();

                if (request == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P12";
                    error.ErrorMSG = "please insert a valid data.";
                    Response.Errors.Add(error);
                }
                if (string.IsNullOrEmpty(request.ChapterName))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err511";
                    error.ErrorMSG = "chapter Name is required.";
                    Response.Errors.Add(error);
                }

                if (request.ParentContentId != null && request.ParentContentId != 0)
                {

                    InventoryItemContentDB = await InventoryItemListQuerable.Where(x => x.Id == request.ParentContentId).FirstOrDefaultAsync();
                    if (InventoryItemContentDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err512";
                        error.ErrorMSG = "The Parent Content Id selected not exist.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (InventoryItemContentDB.InventoryItemId != request.InventoryItemId)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err512";
                        error.ErrorMSG = "This Parent is not related to inventory item selected";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                if (Response.Result)
                {
                    string newOrderStr = "";
                    int newOrder = 1;
                    int newDataLevel = 1;

                    if (request.ParentContentId != 0)
                    {
                        newOrder = await InventoryItemListQuerable.Where(x => x.ParentContentId == request.ParentContentId).CountAsync() + 1;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err512";
                        error.ErrorMSG = "invalid parent content id";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    if (newOrder < 10)
                    {
                        newOrderStr = "0" + newOrder.ToString();
                    }
                    else
                    {
                        newOrderStr = newOrder.ToString();
                    }

                    InventoryItemContentDB = new InventoryItemContent
                    {
                        ChapterNumber = newOrderStr,
                        ChapterName = request.ChapterName,
                        Active = true,
                        CreationDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                        CreatedBy = validation.userID,
                        ModifiedBy = validation.userID,
                        Description = request.Description,
                        InventoryItemId = request.InventoryItemId,
                        ParentContentId = request.ParentContentId,
                        PreparedSearchName = Common.string_compare_prepare_function(request.ChapterName)
                    };
                    var ParentContentId = request.ParentContentId;
                    if (ParentContentId == 0)
                    {
                        InventoryItemContentDB.DataLevel = newDataLevel;
                    }
                    else
                    {
                        var ParentAccountObjDB = await InventoryItemListQuerable.Where(x => x.Id == ParentContentId).FirstOrDefaultAsync();
                        if (ParentAccountObjDB != null)
                        {
                            InventoryItemContentDB.DataLevel = ParentAccountObjDB.DataLevel + 1;
                        }
                    }

                    _unitOfWork.InventoryItemContents.Add(InventoryItemContentDB);
                    if (request.ParentContentId != 0 && request.ParentContentId != null)
                    {
                        InventoryItemContentDB.Haveitem = true;
                    }
                    var ResAccount = _unitOfWork.Complete();
                    Response.ID = InventoryItemContentDB.Id;

                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                if (ex.InnerException != null && ex.InnerException.Message.Contains("FK_InventoryItemContent_InventoryItem"))
                {
                    error.ErrorCode = "Err513";
                    error.ErrorMSG = "The specified Inventory Item ID does not exist.";
                }
                else
                {
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = ex.InnerException?.Message ?? ex.Message;
                }
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<BaseResponseWithId<long>> UpdateInventoryItemContent(UpdateInventoryItemContentDto request)
        {
            BaseResponseWithId<long> response = new BaseResponseWithId<long>();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                if (response.Result)
                {
                    var inventoryItemContentDB = await _unitOfWork.InventoryItemContents.GetByIdAsync(request.Id);

                    if (inventoryItemContentDB == null)
                    {
                        response.Result = false;
                        Error error = new Error
                        {
                            ErrorCode = "Err404",
                            ErrorMSG = $"InventoryItemContent with ID {request.Id} not found."
                        };
                        response.Errors.Add(error);
                        return response;
                    }

                    if (request.ChapterName == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid ChapterName";
                        response.Errors.Add(error);
                    }
                    else
                    {
                        inventoryItemContentDB.ChapterName = request.ChapterName;
                        inventoryItemContentDB.PreparedSearchName = Common.string_compare_prepare_function(request.ChapterName);
                    }


                    if (request.Description == null)
                    {
                        response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P112";
                        error.ErrorMSG = "Invalid Description";
                        response.Errors.Add(error);
                    }
                    else
                    {
                        inventoryItemContentDB.Description = request.Description;
                    }


                    inventoryItemContentDB.ModifiedDate = DateTime.Now;
                    inventoryItemContentDB.ModifiedBy = validation.userID;

                    _unitOfWork.InventoryItemContents.Update(inventoryItemContentDB);
                    _unitOfWork.Complete();

                    response.Result = true;
                }
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                if (ex.InnerException != null && ex.InnerException.Message.Contains("FK_InventoryItemContent_InventoryItem"))
                {
                    error.ErrorCode = "Err513";
                    error.ErrorMSG = "The specified Inventory Item ID does not exist.";
                }
                else
                {
                    error.ErrorCode = "Err10";
                    error.ErrorMSG = ex.InnerException?.Message ?? ex.Message;
                }
                response.Errors.Add(error);
            }
            return response;
        }

        public BaseResponseWithId<long> DeleteInvenotryItem([FromHeader] long InventoryItemId, [FromHeader] bool Active)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (InventoryItemId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Inventory Item Id is Required";
                    Response.Errors.Add(error);
                    return Response;
                }
                var item = _unitOfWork.InventoryItems.GetById(InventoryItemId);
                if (item == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Inventory Item not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                item.Active = Active;
                item.ModifiedBy = validation.userID;
                item.ModifiedDate = DateTime.Now;
                Response.ID = item.Id;
                _unitOfWork.InventoryItems.Update(item);
                _unitOfWork.Complete();
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

        public InventortyItemListResponse GetInventoryItemList(GetInventoryItemListFilters filters)
        {
            InventortyItemListResponse Response = new InventortyItemListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<InventoryItemWithCheckOpeningBalance>();
                if (Response.Result)
                {
                    var QuerableListDB = _unitOfWork.VInventoryItems.FindAllQueryable(x => x.Active == !filters.GetNotActive).AsQueryable();
                    if (filters.StoreId != 0)
                    {
                        var IDSInventoryItemsList = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryStoreId == filters.StoreId && x.FinalBalance > 0).Select(x => x.InventoryItemId).Distinct().ToList();
                        QuerableListDB = QuerableListDB.Where(x => IDSInventoryItemsList.Contains(x.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SearchKey))
                    {
                        filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);

                        QuerableListDB = QuerableListDB.Where(x => x.Name.ToLower().Contains(filters.SearchKey.ToLower())
                                                || (x.Code != null ? x.Code.ToLower() == filters.SearchKey.ToLower() : false)
                                                || (x.MaxBalance != null ? x.MaxBalance.ToString() == filters.SearchKey.Trim() : false)
                                                ).AsQueryable();
                    }



                    if (!string.IsNullOrEmpty(filters.MarketName))
                    {
                        filters.MarketName = HttpUtility.UrlDecode(filters.MarketName);
                        QuerableListDB = QuerableListDB.Where(x => x.MarketName.ToLower().Contains(filters.MarketName.ToLower())).AsQueryable();
                    }

                    if (!string.IsNullOrEmpty(filters.CommercialName))
                    {
                        filters.CommercialName = HttpUtility.UrlDecode(filters.CommercialName);
                        QuerableListDB = QuerableListDB.Where(x => x.CommercialName.ToLower().Contains(filters.CommercialName.ToLower())).AsQueryable();
                    }

                    if (!string.IsNullOrEmpty(filters.PartNo))
                    {
                        filters.PartNo = HttpUtility.UrlDecode(filters.PartNo);
                        QuerableListDB = QuerableListDB.Where(x => x.PartNo.ToLower().Contains(filters.PartNo.ToLower())).AsQueryable();
                    }

                    if (!string.IsNullOrEmpty(filters.ChapterName))
                    {
                        filters.ChapterName = HttpUtility.UrlDecode(filters.ChapterName);
                        var preparedChapterName = Common.string_compare_prepare_function(filters.ChapterName);
                        var itemsIds = _Context.InventoryItemContents.Where(a => a.PreparedSearchName.ToLower().Trim().Contains(preparedChapterName.ToLower().Trim())).Select(a => a.InventoryItemId).ToList();

                        QuerableListDB = QuerableListDB.Where(a => itemsIds.Contains(a.Id)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.SearchCode))
                    {
                        filters.SearchCode = HttpUtility.UrlDecode(filters.SearchCode);

                        QuerableListDB = QuerableListDB.Where(x => (x.Code != null ? x.Code.ToLower() == filters.SearchCode.ToLower() : false)
                                                ).AsQueryable();
                    }
                    if (filters.PriorityID != 0)
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.PriorityId == filters.PriorityID).AsQueryable();
                    }
                    if (filters.InventoryItemCategoryID != 0)
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.InventoryItemCategoryId == filters.InventoryItemCategoryID).AsQueryable();
                    }
                    if (filters.MinBalance != 0)
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.MinBalance < filters.MinBalance).AsQueryable();
                    }

                    if (filters.MaxBalance != 0)
                    {
                        QuerableListDB = QuerableListDB.Where(x => x.MaxBalance > filters.MaxBalance).AsQueryable();
                    }
                    bool ViewOpeningBalance = false;
                    if (!string.IsNullOrEmpty(filters.Type))
                    {
                        if (filters.Type == "OpeningBalance")
                        {
                            ViewOpeningBalance = true;
                        }
                    }
                    if (filters.HaveOpeningBalance != null)
                    {
                        var IDSItemsHavingOpeningBalance = _unitOfWork.VInventoryAddingOrderItems.FindAll(x => x.OperationType == "Opening Balance").Select(x => x.InventoryItemId).ToList();
                        if ((bool)filters.HaveOpeningBalance)
                        {
                            QuerableListDB = QuerableListDB.Where(x => IDSItemsHavingOpeningBalance.Contains(x.Id)).AsQueryable();
                        }
                        else
                        {
                            QuerableListDB = QuerableListDB.Where(x => !IDSItemsHavingOpeningBalance.Contains(x.Id)).AsQueryable();
                        }
                    }
                    DateTime NotReleasedFrom = new DateTime(DateTime.Now.Year, 1, 1);
                    if (filters.NotReleasedFrom != null)
                    {
                        NotReleasedFrom = (DateTime)filters.NotReleasedFrom;
                    }

                    /*Not Release duration date from and to*/
                    var InventoryItemIDs = new List<long>();
                    if (filters.NotReleasedFrom != null)
                    {
                        InventoryItemIDs = _unitOfWork.InventoryStoreItems.FindAll(x => x.OperationType.Contains("Release Order") && x.Balance1 > 0)
                                           .GroupBy(item => item.InventoryItemId)
                                           .Select(group => group.OrderByDescending(item => item.CreationDate).FirstOrDefault()).Where(x => x.CreationDate <= NotReleasedFrom).Select(x => x.InventoryItemId)
                                           .Distinct().ToList();
                        //InventoryItemIDs =  _Context.InventoryMatrialReleaseItems.Where(x => x.InventoryMatrialRelease.CreationDate >= NotReleasedFrom && x.InventoryMatrialRelease.CreationDate <= NotReleasedTo).Select(x => x.InventoryMatrialRequestItem.InventoryItemID).ToList();
                        if (InventoryItemIDs.Count() > 0)
                        {
                            var asdad = QuerableListDB.Where(x => InventoryItemIDs.Contains(x.Id)).ToList();
                            QuerableListDB = QuerableListDB.Where(x => InventoryItemIDs.Contains(x.Id)).AsQueryable();
                        }
                    }
                    var InventoryItemPagingList = PagedList<VInventoryItem>.Create(QuerableListDB.OrderBy(x => x.Id), filters.CurrentPage, filters.NumberOfItemsPerPage);

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = InventoryItemPagingList.CurrentPage,
                        TotalPages = InventoryItemPagingList.TotalPages,
                        ItemsPerPage = InventoryItemPagingList.PageSize,
                        TotalItems = InventoryItemPagingList.TotalCount
                    };


                    if (InventoryItemPagingList.Count > 0)
                    {
                        Response.TotalItemCount = InventoryItemPagingList.Count();
                        foreach (var item in InventoryItemPagingList)
                        {
                            var inv = _Context.InventoryItems.Find(item.Id);
                            var DLLObj = new InventoryItemWithCheckOpeningBalance();
                            DLLObj.ID = item.Id;
                            DLLObj.Name = item.Name.Trim();
                            DLLObj.ItemCode = item.Code;
                            DLLObj.PartNo = item.PartNo;
                            DLLObj.CategoryId = item.InventoryItemCategoryId;
                            DLLObj.CategoryName = item.CategoryName;
                            DLLObj.RequstionUOMID = item.RequstionUomid;
                            DLLObj.PurchasingUOMID = item.PurchasingUomid;
                            DLLObj.Active = item.Active;
                            DLLObj.CalculationType = item.CalculationType;
                            DLLObj.CommericalName = item.CommercialName;
                            DLLObj.Description = item.Description;
                            DLLObj.Details = item.Details;
                            DLLObj.MarketName = item.MarketName;
                            DLLObj.CustomeUnitPrice = item.CustomeUnitPrice;
                            DLLObj.MaxBlanace = item.MaxBalance;
                            DLLObj.MinBalance = item.MinBalance;
                            DLLObj.SerialCounter = item.ItemSerialCounter;
                            DLLObj.UOR = item.RequestionUomshortName.Trim();
                            DLLObj.UOP = item.PurchasingUomshortName.Trim();
                            if (inv.ImageUrl != null)
                            {
                                DLLObj.ImageUrl = Globals.baseURL + '/' + inv.ImageUrl;
                            }
                            DLLObj.ExchangeFactor = item.ExchangeFactor;
                            if (ViewOpeningBalance)
                            {
                                if (filters.HaveOpeningBalance != null)
                                {
                                    if ((bool)filters.HaveOpeningBalance)
                                    {
                                        DLLObj.HaveOpeningBalance = true;
                                    }
                                    else
                                    {
                                        DLLObj.HaveOpeningBalance = false;
                                    }
                                }
                                else
                                {
                                    DLLObj.HaveOpeningBalance = Common.CheckInventoryItemHaveOpeningBalance(item.Id, _Context);
                                }
                            }
                            DDLList.Add(DLLObj);
                        }
                    }
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

        public InventortyStoreIncludeLocationListResponse GetInventoryStoresIncludeLocationsListForBY([FromHeader] long userId)
        {
            InventortyStoreIncludeLocationListResponse Response = new InventortyStoreIncludeLocationListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var StoreList = new List<InventortyStoreIncludeLocation>();
                if (Response.Result)
                {
                    var ListDB = _unitOfWork.InventoryStores.FindAll(x => x.Active == true, includes: new[] { "InventoryStoreKeepers" }).ToList();
                    if (userId != 0)
                    {
                        var storeIdsListFromKeepers = _unitOfWork.InventoryStoreKeepers.FindAll(x => x.Active && x.UserId == userId).Select(x => x.InventoryStoreId).ToList();
                        ListDB = ListDB.Where(x => storeIdsListFromKeepers.Contains(x.Id)).ToList();

                    }
                    if (ListDB.Count > 0)
                    {
                        foreach (var item in ListDB)
                        {
                            var DLLObj = new InventortyStoreIncludeLocation();
                            DLLObj.Id = item.Id;
                            DLLObj.StoreName = item.Name;
                            DLLObj.LocationName = item.Location;
                            DLLObj.CountOfKeepers = item.InventoryStoreKeepers?.Count();

                            StoreList.Add(DLLObj);
                        }
                    }

                }
                Response.DDLList = StoreList;
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

        public InventortyItemLowStockListResponse GetInventoryStoreItemLowStockList([FromHeader] string SearchKey, [FromHeader] long InventoryStoreID, [FromHeader] int CurrentPage = 1, [FromHeader] int NumberOfItemsPerPage = 10)
        {
            InventortyItemLowStockListResponse Response = new InventortyItemLowStockListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var DDLList = new List<InventoryStoreItemLowStock>();
                if (Response.Result)
                {
                    var InventoryItemListDB = _unitOfWork.VInventoryStoreItems.FindAllQueryable(x => x.Active == true && x.StoreActive == true).AsQueryable();
                    if (!string.IsNullOrEmpty(SearchKey))
                    {

                        SearchKey = HttpUtility.UrlDecode(SearchKey);

                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryItemName.ToLower().Contains(SearchKey.ToLower())
                                                || (x.Code != null ? x.Code.ToLower().Contains(SearchKey.ToLower()) : false)
                                                ).AsQueryable();
                    }
                    if (InventoryStoreID != 0)
                    {
                        InventoryItemListDB = InventoryItemListDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
                    }
                    var groupedList = InventoryItemListDB.GroupBy(x => new
                    {
                        x.InventoryItemId,
                        x.InventoryItemName,
                        x.MinBalance,
                        x.MaxBalance,
                        x.ExchangeFactor,
                        x.Code,
                        x.InventoryStoreId
                    }).Select(g => new
                    {
                        Key = g.Key,
                        CurrentBalance = g.Sum(x => x.FinalBalance)
                    }).ToList().Where(g => g.CurrentBalance <= g.Key.MinBalance).OrderBy(g => g.Key.InventoryItemId).ToList();
                    int totalItems = groupedList.Count;
                    int totalPages = (int)Math.Ceiling((double)totalItems / NumberOfItemsPerPage);

                    var pagedGroupedList = groupedList
                        .Skip((CurrentPage - 1) * NumberOfItemsPerPage)
                        .Take(NumberOfItemsPerPage)
                        .ToList();

                    // Response list
                    foreach (var item in pagedGroupedList)
                    {
                        var obj = new InventoryStoreItemLowStock
                        {
                            ID = item.Key.InventoryItemId,
                            Name = item.Key.InventoryItemName,
                            MinBalance = item.Key.MinBalance,
                            MaxBalance = item.Key.MaxBalance,
                            ExchangeFactor = item.Key.ExchangeFactor,
                            ItemCode = item.Key.Code,
                            StoreID = item.Key.InventoryStoreId,
                            CurrentBalance = item.CurrentBalance,
                            HavePRorPO = Common.CheckInventoryItemHavePRorPO(item.Key.InventoryItemId, _Context)
                        };
                        DDLList.Add(obj);
                    }

                    // Set pagination info
                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = CurrentPage,
                        TotalPages = totalPages,
                        ItemsPerPage = NumberOfItemsPerPage,
                        TotalItems = totalItems
                    };
                    Response.DDLList = DDLList.Distinct().ToList();
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

        public GetInventoryItemResponse GetInventoryItem([FromHeader] long InventoryItemID)
        {
            GetInventoryItemResponse Response = new GetInventoryItemResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "InventoryItemID Is required";
                        Response.Errors.Add(error);
                    }


                    if (Response.Result)

                    {
                        var InventoryItem = new InventoryItemInfoForInsert();
                        var ItemIbjDB = _unitOfWork.InventoryItems.FindAll(x => x.Id == InventoryItemID, includes: new[] { "InventoryItemCategory", "Priority" }).FirstOrDefault();
                        if (ItemIbjDB != null)
                        {
                            InventoryItem.Active = ItemIbjDB.Active;
                            InventoryItem.ItemName = ItemIbjDB.Name;
                            InventoryItem.ItemCode = ItemIbjDB.Code;
                            InventoryItem.ConvertRateFromPurchasingToRequestionUnit = (decimal)(ItemIbjDB.ExchangeFactor1 != null ? ItemIbjDB.ExchangeFactor1 : 0);
                            InventoryItem.ID = ItemIbjDB.Id;
                            InventoryItem.ItemName = ItemIbjDB.Name;
                            InventoryItem.ItemCode = ItemIbjDB.Code;
                            InventoryItem.CommericalName = ItemIbjDB.CommercialName?.Trim();
                            InventoryItem.PartNumber = ItemIbjDB.PartNo;
                            InventoryItem.MarketName = ItemIbjDB.MarketName;
                            InventoryItem.Type = ItemIbjDB.Exported?.Trim();
                            InventoryItem.Description = ItemIbjDB.Description?.Trim();
                            InventoryItem.Details = ItemIbjDB.Details;
                            InventoryItem.PriorityID = ItemIbjDB.PriorityId;
                            InventoryItem.PriorityName = ItemIbjDB.Priority?.Name;
                            InventoryItem.PurchasingUOMID = ItemIbjDB.PurchasingUomid;
                            InventoryItem.RequstionUOMID = ItemIbjDB.RequstionUomid;
                            InventoryItem.Category = ItemIbjDB.InventoryItemCategory?.Name;
                            InventoryItem.CategoryId = ItemIbjDB.InventoryItemCategoryId;
                            InventoryItem.PurchasingUnit = Common.GetInventoryUOM(ItemIbjDB.PurchasingUomid, _Context);
                            InventoryItem.RequestionUnit = Common.GetInventoryUOM(ItemIbjDB.RequstionUomid, _Context);
                            InventoryItem.PriceCalculationMethod = Common.GetIncentoryCalculationMethod(ItemIbjDB.CalculationType, _Context);
                            if (ItemIbjDB.ImageUrl != null)
                            {
                                InventoryItem.ImageUrl = Globals.baseURL + '/' + ItemIbjDB.ImageUrl;
                            }
                            InventoryItem.MinBalance = ItemIbjDB.MinBalance1 != null ? (decimal)ItemIbjDB.MinBalance1 : 0;
                            InventoryItem.MaxBlanace = ItemIbjDB.MaxBalance1 != null ? (decimal)ItemIbjDB.MaxBalance1 : 0;
                            InventoryItem.ConvertRateFromPurchasingToRequestionUnit = ItemIbjDB.ExchangeFactor1 != null ? (decimal)ItemIbjDB.ExchangeFactor1 : 0;
                            InventoryItem.Active = ItemIbjDB.Active;
                            InventoryItem.CalculationTypeID = ItemIbjDB.CalculationType;
                            decimal Amount = 0;
                            int CalcType = ItemIbjDB.CalculationType;
                            if (CalcType == 1)
                            {
                                Amount = ItemIbjDB.AverageUnitPrice;
                            }
                            else if (CalcType == 2)
                            {
                                Amount = ItemIbjDB.MaxUnitPrice;
                            }
                            else if (CalcType == 3)
                            {
                                Amount = ItemIbjDB.LastUnitPrice;
                            }
                            else if (CalcType == 4)
                            {
                                Amount = ItemIbjDB.CustomeUnitPrice;
                            }
                            InventoryItem.Amount = Amount;
                            InventoryItem.CostAmount1 = ItemIbjDB.CostAmount1;
                            InventoryItem.CostAmount2 = ItemIbjDB.CostAmount2;
                            InventoryItem.CostAmount3 = ItemIbjDB.CostAmount3;
                            InventoryItem.CostName1 = ItemIbjDB.CostName1;
                            InventoryItem.CostName2 = ItemIbjDB.CostName2;
                            InventoryItem.CostName3 = ItemIbjDB.CostName3;
                            InventoryItem.CustomAmount = ItemIbjDB.CustomeUnitPrice;


                            var InventoryItemAttachemtsList = _unitOfWork.InventoryItemAttachments.FindAll(x => x.InventoryItemId == ItemIbjDB.Id).Select(itemAttach => new AttachmentFile
                            {
                                Id = itemAttach.Id,
                                Active = itemAttach.Active,
                                /*FileExtension = itemAttach.FileExtenssion,
                                FileName = itemAttach.FileName,*/
                                FilePath = Globals.baseURL + '/' + itemAttach.AttachmentPath
                            }).ToList();
                            InventoryItem.AttachmentsList = InventoryItemAttachemtsList;
                            Response.Data = InventoryItem;
                            //}
                            //else
                            //{
                            //    Response.Result = false;
                            //    Error error = new Error();
                            //    error.ErrorCode = "Err25";
                            //    error.ErrorMSG = "InventoryItemID is not active";
                            //    Response.Errors.Add(error);
                            //}
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "Invalid Inventory ItemID";
                            Response.Errors.Add(error);
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
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public async Task<GetInventoryStoreResponse> GetInventoryStore()
        {
            GetInventoryStoreResponse response = new GetInventoryStoreResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetInventoryStoreResponseList = new List<InventoryStoreData>();
                if (response.Result)
                {
                    if (response.Result)
                    {
                        var InventoryStoreDB = await _unitOfWork.InventoryStores.GetAllAsync();


                        if (InventoryStoreDB != null && InventoryStoreDB.Count() > 0)
                        {

                            foreach (var InventoryStoreDBOBJ in InventoryStoreDB)
                            {
                                var InventoryStoreDBResponse = new InventoryStoreData();

                                InventoryStoreDBResponse.ID = InventoryStoreDBOBJ.Id;

                                InventoryStoreDBResponse.Name = InventoryStoreDBOBJ.Name;

                                InventoryStoreDBResponse.Location = InventoryStoreDBOBJ.Location;

                                InventoryStoreDBResponse.Tel = InventoryStoreDBOBJ.Tel;

                                InventoryStoreDBResponse.Active = InventoryStoreDBOBJ.Active;




                                GetInventoryStoreResponseList.Add(InventoryStoreDBResponse);
                            }



                        }

                    }

                }
                response.InventoryStoreList = GetInventoryStoreResponseList;
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

        public async Task<GetInventoryStorePerIDResponse> GetInventoryStorePerID([FromHeader] long StoreID)
        {
            GetInventoryStorePerIDResponse response = new GetInventoryStorePerIDResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var GetInventoryStoreResponseList = new InventoryStoreData();
                var GetInventoryLocationResponseList = new List<InventoryLocationData>();
                var GetInventoryKeeperResponseList = new List<InventoryKeeperData>();
                if (response.Result)
                {



                    if (response.Result)
                    {

                        //var CheckUserDB = _Context.proc_UserLoadAll().Where(x => (x.Email.Trim() == login.Email.Trim()) && x.Password.ToLower().Trim() == Encrypt_Decrypt.Encrypt(login.Password.Trim(), key).ToLower().Trim()).FirstOrDefault();
                        var InventoryStoreDB = (await _unitOfWork.InventoryStores.FindAllAsync(x => x.Id == StoreID)).FirstOrDefault();
                        var InventoryStoreKeepersDB = (await _unitOfWork.InventoryStoreKeepers.FindAllAsync(x => x.InventoryStoreId == StoreID)).ToList();
                        var InventoryStoreLocationDB = (await _unitOfWork.InventoryStoreLocations.FindAllAsync(x => x.InventoryStoreId == StoreID)).ToList();


                        if (InventoryStoreDB != null)
                        {


                            var InventoryStoreDBResponse = new InventoryStorePerIDData();

                            InventoryStoreDBResponse.ID = InventoryStoreDB.Id;

                            InventoryStoreDBResponse.Name = InventoryStoreDB.Name;

                            InventoryStoreDBResponse.Location = InventoryStoreDB.Location;

                            InventoryStoreDBResponse.Tel = InventoryStoreDB.Tel;

                            InventoryStoreDBResponse.Active = InventoryStoreDB.Active;





                            foreach (var InventoryStoreKeepersDBOBJ in InventoryStoreKeepersDB)
                            {
                                var InventoryStoreKeepersDBResponse = new InventoryKeeperData();

                                InventoryStoreKeepersDBResponse.ID = InventoryStoreKeepersDBOBJ.Id;

                                InventoryStoreKeepersDBResponse.InventoryStoreID = InventoryStoreKeepersDBOBJ.InventoryStoreId;

                                InventoryStoreKeepersDBResponse.UserID = (int)InventoryStoreKeepersDBOBJ.UserId;

                                InventoryStoreKeepersDBResponse.UserName = Common.GetUserName(InventoryStoreKeepersDBOBJ.UserId, _Context);


                                InventoryStoreKeepersDBResponse.Active = InventoryStoreKeepersDBOBJ.Active;




                                GetInventoryKeeperResponseList.Add(InventoryStoreKeepersDBResponse);
                            }
                            InventoryStoreDBResponse.inventoryKeeperData = GetInventoryKeeperResponseList;



                            foreach (var InventoryStoreLocationDBOBJ in InventoryStoreLocationDB)
                            {
                                var InventoryStoreLocationDBResponse = new InventoryLocationData();

                                InventoryStoreLocationDBResponse.ID = InventoryStoreLocationDBOBJ.Id;

                                InventoryStoreLocationDBResponse.Location = InventoryStoreLocationDBOBJ.Location;


                                InventoryStoreLocationDBResponse.Active = InventoryStoreLocationDBOBJ.Active;

                                GetInventoryLocationResponseList.Add(InventoryStoreLocationDBResponse);
                            }

                            InventoryStoreDBResponse.inventoryLocationData = GetInventoryLocationResponseList;

                            response.InventoryStoreObject = InventoryStoreDBResponse;

                        }

                    }

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

        public BaseResponseWithId<long> AddNewInventoryItem([FromForm] AddNewInventoryItemRequest request)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    //check sent data
                    if (request == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (request.Data == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid Data.";
                        Response.Errors.Add(error);
                        return Response;
                    }




                    if (request.Data.ID == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ID Is Required";
                        Response.Errors.Add(error);
                    }

                    if (string.IsNullOrEmpty(request.Data.ItemName))
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "ItemName Is Required";
                        Response.Errors.Add(error);
                    }

                    if (request.Data.CategoryId == null || request.Data.CategoryId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Item Category Is Required";
                        Response.Errors.Add(error);
                    }

                    if (request.Data.RequstionUOMID == null || request.Data.RequstionUOMID == 0)// Default set first
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "RequstionUOMID Is Required";
                        Response.Errors.Add(error);
                    }

                    if (request.Data.PurchasingUOMID == null || request.Data.PurchasingUOMID == 0) // Default set first
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "PurchasingUOMID Is Required";
                        Response.Errors.Add(error);
                    }



                    if (Response.Result)
                    {
                        var InventoryItemQuerable = _unitOfWork.InventoryItems.FindAllQueryable(x => x.Active == true).AsQueryable();
                        // Check unique Name
                        var CheckItemsName = InventoryItemQuerable.Where(x => x.Name.Trim() == request.Data.ItemName.Trim() && x.Id != request.Data.ID).FirstOrDefault();
                        if (CheckItemsName != null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err25";
                            error.ErrorMSG = "ItemName Is already exist";
                            Response.Errors.Add(error);
                        }
                        // Check unique Code
                        if (!string.IsNullOrWhiteSpace(request.Data.ItemCode))
                        {
                            var CheckItemsCode = InventoryItemQuerable.Where(x => x.Code.Trim() == request.Data.ItemCode.Trim() && x.Id != request.Data.ID).FirstOrDefault();
                            if (CheckItemsCode != null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "ItemCode Is already exist";
                                Response.Errors.Add(error);
                            }
                        }
                        /*byte[] ImageBytes = null;
                        if (!string.IsNullOrEmpty(request.Data.ItemImage))
                        {
                            ImageBytes = Convert.FromBase64String(request.Data.ItemImage);
                        }*/

                        if (Response.Result)
                        {
                            long InventoryItemId = 0;
                            if (request.Data.ID == 0)
                            {
                                var item = new InventoryItem();
                                item.Active = request.Data.Active;
                                item.CalculationType = request.Data.CalculationTypeID != null ? (int)request.Data.CalculationTypeID : 1;
                                if (item.CalculationType == 4 && request.Data.CustomAmount != null)
                                    item.CustomeUnitPrice = (decimal)request.Data.CustomAmount;
                                else
                                    item.CustomeUnitPrice = 0;
                                item.AverageUnitPrice = 0;
                                item.MaxUnitPrice = 0;
                                item.LastUnitPrice = 0;
                                item.CommercialName = request.Data.CommericalName;
                                item.CreatedBy = validation.userID;
                                item.CreationDate = DateTime.Now;
                                item.Description = request.Data.Description;
                                item.Details = request.Data.Details;
                                item.ExchangeFactor1 = request.Data.ConvertRateFromPurchasingToRequestionUnit;
                                item.Exported = request.Data.Type; // Local or Exported
                                item.InventoryItemCategoryId = (int)request.Data.CategoryId;
                                item.MarketName = request.Data.MarketName;
                                if (request.Data.MaxBlanace != null)
                                {
                                    item.MaxBalance1 = (decimal)request.Data.MaxBlanace;
                                }
                                else
                                {
                                    item.MaxBalance1 = 0;
                                }
                                if (request.Data.MinBalance != null)
                                {
                                    item.MinBalance1 = (decimal)request.Data.MinBalance;
                                }
                                else
                                {
                                    item.MinBalance1 = 0;
                                }
                                if (request.Data.CostAmount1 != null)
                                    item.CostAmount1 = (decimal)request.Data.CostAmount1;
                                if (request.Data.CostAmount2 != null)
                                    item.CostAmount2 = (decimal)request.Data.CostAmount2;
                                if (request.Data.CostAmount3 != null)
                                    item.CostAmount3 = (decimal)request.Data.CostAmount3;

                                item.ModifiedBy = validation.userID;
                                item.ModifiedDate = DateTime.Now;
                                item.PartNo = request.Data.PartNumber;
                                item.Name = request.Data.ItemName;

                                item.PurchasingUomid = (int)request.Data.PurchasingUOMID;
                                item.RequstionUomid = (int)request.Data.RequstionUOMID;

                                var NewItemSerial = _Context.InventoryItems.Max(p => p == null ? 0 : p.ItemSerialCounter) + 1;
                                item.ItemSerialCounter = NewItemSerial ?? 1;
                                if (request.Data.PriorityID != null && request.Data.PriorityID != 0)
                                {
                                    item.PriorityId = (int)request.Data.PriorityID;
                                }
                                if (item.ImageUrl != null)
                                {
                                    var oldpath = Path.Combine(_host.WebRootPath, item.ImageUrl);
                                    if (System.IO.File.Exists(oldpath))
                                    {
                                        System.IO.File.Delete(oldpath);
                                        item.ImageUrl = null;
                                    }
                                }
                                if (request.Data.Image != null)
                                {
                                    var fileExtension = request.Data.Image.FileName.Split('.').Last();
                                    var virtualPath = $@"Attachments\{validation.CompanyName}\InventoryItems\Images\";
                                    var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Data.Image.FileName.Trim().Replace(" ", ""));
                                    var AttachPath = Common.SaveFileIFF(virtualPath, request.Data.Image, FileName, fileExtension, _host);
                                    item.ImageUrl = AttachPath;
                                }
                                //if (FU_Photo.HasFile)
                                //{
                                //    item.Image = FU_Photo.FileBytes;
                                //    item.HasImage = true;
                                //}
                                //else
                                //    item.HasImage = false;
                                /*if (ImageBytes != null)
                                {
                                    item.Image = ImageBytes;
                                    item.HasImage = true;
                                }
                                else
                                    item.HasImage = false;*/
                                if (!string.IsNullOrWhiteSpace(request.Data.ItemCode))
                                {
                                    item.Code = request.Data.ItemCode;
                                }
                                else
                                {
                                    item.Code = "0";

                                }
                                _unitOfWork.InventoryItems.Add(item);
                                var Res = _unitOfWork.Complete();
                                if (Res > 0)
                                {

                                    if (string.IsNullOrWhiteSpace(request.Data.ItemCode))
                                    {
                                        item.Code = item.Id.ToString(); // DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + item.ID;
                                        _unitOfWork.Complete();
                                    }
                                    // Insert  ItemCategory
                                    var ItemCategory = _unitOfWork.InventoryItemCategories.FindAll(x => x.Id == request.Data.CategoryId).FirstOrDefault();
                                    ItemCategory.HaveItem = true;
                                    _unitOfWork.Complete();

                                    //// Insert  ItemAttachment

                                    InventoryItemId = item.Id;
                                    Response.ID = item.Id;
                                }
                            }
                            else // Update 
                            {
                                var item = _unitOfWork.InventoryItems.FindAll(x => x.Id == request.Data.ID).FirstOrDefault();
                                if (item != null)
                                {
                                    item.Active = request.Data.Active;
                                    item.CalculationType = request.Data.CalculationTypeID != null ? (int)request.Data.CalculationTypeID : 1;
                                    if (item.CalculationType == 4 && request.Data.CustomAmount != null)
                                        item.CustomeUnitPrice = (decimal)request.Data.CustomAmount;
                                    else
                                        item.CustomeUnitPrice = 0;
                                    //item.AverageUnitPrice = 0;
                                    //item.MaxUnitPrice = 0;
                                    //item.LastUnitPrice = 0;
                                    item.CommercialName = request.Data.CommericalName;
                                    item.CreatedBy = validation.userID;
                                    item.CreationDate = DateTime.Now;
                                    item.Description = request.Data.Description;
                                    item.Details = request.Data.Details;
                                    item.Exported = request.Data.Type; // Local or Exported
                                    item.InventoryItemCategoryId = (int)request.Data.CategoryId;
                                    item.MarketName = request.Data.MarketName;
                                    if (request.Data.MaxBlanace != null)
                                    {
                                        item.MaxBalance1 = (decimal)request.Data.MaxBlanace;
                                    }
                                    else
                                    {
                                        item.MaxBalance1 = 0;
                                    }
                                    if (request.Data.MinBalance != null)
                                    {
                                        item.MinBalance1 = (decimal)request.Data.MinBalance;
                                    }
                                    else
                                    {
                                        item.MinBalance1 = 0;
                                    }
                                    if (request.Data.CostAmount1 != null)
                                        item.CostAmount1 = (decimal)request.Data.CostAmount1;
                                    if (request.Data.CostAmount2 != null)
                                        item.CostAmount2 = (decimal)request.Data.CostAmount2;
                                    if (request.Data.CostAmount3 != null)
                                        item.CostAmount3 = (decimal)request.Data.CostAmount3;

                                    item.ModifiedBy = validation.userID;
                                    item.ModifiedDate = DateTime.Now;
                                    item.PartNo = request.Data.PartNumber;
                                    item.Name = request.Data.ItemName;



                                    // validation check if have any movement on this item or purchase order cannot edit 
                                    if (item.PurchasingUomid != request.Data.PurchasingUOMID || item.RequstionUomid != request.Data.RequstionUOMID || item.ExchangeFactor1 != request.Data.ConvertRateFromPurchasingToRequestionUnit)
                                    {

                                        var ChkITEMinInventoryStoreItem = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == item.Id).FirstOrDefault();
                                        var ChkITEMinPOItem = _unitOfWork.PurchasePoitems.FindAll(x => x.InventoryItemId == item.Id).FirstOrDefault();
                                        if (ChkITEMinInventoryStoreItem != null || ChkITEMinPOItem != null)
                                        {
                                            // Back Error

                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Can't update (Purchasing UOM , Requestion UOM or Convert Rate) for this Item becasue have more than movement or purchasing order";
                                            Response.Errors.Add(error);
                                        }

                                    }
                                    item.PurchasingUomid = (int)request.Data.PurchasingUOMID;
                                    item.RequstionUomid = (int)request.Data.RequstionUOMID;
                                    item.ExchangeFactor1 = request.Data.ConvertRateFromPurchasingToRequestionUnit;

                                    //var NewItemSerial = _Context.proc_InventoryItemLoadAll().Max(p => p == null ? 0 : p.ItemSerialCounter) + 1;
                                    //item.ItemSerialCounter = NewItemSerial ?? 1;
                                    if (request.Data.PriorityID != null && request.Data.PriorityID != 0)
                                    {
                                        item.PriorityId = (int)request.Data.PriorityID;
                                    }
                                    if (request.Data.Image != null)
                                    {
                                        var fileExtension = request.Data.Image.FileName.Split('.').Last();
                                        var virtualPath = $@"Attachments\{validation.CompanyName}\InventoryItems\Images\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(request.Data.Image.FileName.Trim().Replace(" ", ""));
                                        var AttachPath = Common.SaveFileIFF(virtualPath, request.Data.Image, FileName, fileExtension, _host);
                                        item.ImageUrl = AttachPath;
                                    }
                                    /*if (ImageBytes != null)
                                    {
                                        item.Image = ImageBytes;
                                        item.HasImage = true;
                                    }*/
                                    //if (FU_Photo.HasFile)
                                    //{
                                    //    item.Image = FU_Photo.FileBytes;
                                    //    item.HasImage = true;
                                    //}
                                    //else
                                    //    item.HasImage = false;
                                    if (!string.IsNullOrWhiteSpace(request.Data.ItemCode))
                                    {
                                        item.Code = request.Data.ItemCode;
                                    }
                                    //_Context.InventoryItems.Add(item);
                                    var Res = _unitOfWork.Complete();

                                    if (Res > 0)
                                    {
                                        // Insert  ItemCategory
                                        var ItemCategory = _unitOfWork.InventoryItemCategories.FindAll(x => x.Id == request.Data.CategoryId).FirstOrDefault();
                                        ItemCategory.HaveItem = true;
                                        _unitOfWork.Complete();

                                        //// Insert  ItemAttachment

                                        InventoryItemId = item.Id;
                                        Response.ID = item.Id;
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "ID Is Not Found";
                                    Response.Errors.Add(error);
                                }
                            }

                            if (request.Data.AttachmentsList != null)
                            {
                                foreach (var attachment in request.Data.AttachmentsList)
                                {

                                    if (attachment.Id != null && attachment.Active == false)
                                    {
                                        // Delete
                                        var AttachmentDb = _unitOfWork.InventoryItemAttachments.FindAll(x => x.Id == attachment.Id).FirstOrDefault();
                                        if (AttachmentDb != null)
                                        {
                                            // Ensure the second path doesn't start with any kind of directory separator
                                            var attachmentPath = AttachmentDb.AttachmentPath.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                            // Combine paths
                                            var oldpath = Path.Combine(_host.WebRootPath, attachmentPath);
                                            if (System.IO.File.Exists(oldpath))
                                            {
                                                System.IO.File.Delete(oldpath);
                                            }
                                            _unitOfWork.InventoryItemAttachments.Delete(AttachmentDb);
                                            _unitOfWork.Complete();
                                        }
                                    }
                                    else
                                    {
                                        var fileExtension = attachment.File.FileName.Split('.').Last();
                                        var virtualPath = $@"Attachments\{validation.CompanyName}\InventoryItems\AttachmentFiles\";
                                        var FileName = System.IO.Path.GetFileNameWithoutExtension(attachment.File.FileName.Trim().Replace(" ", ""));
                                        var AttachPath = Common.SaveFileIFF(virtualPath, attachment.File, FileName, fileExtension, _host);
                                        var AttachmentDb = new NewGaras.Infrastructure.Entities.InventoryItemAttachment();
                                        AttachmentDb.InventoryItemId = InventoryItemId;
                                        AttachmentDb.FileExtenssion = fileExtension;
                                        AttachmentDb.FileName = FileName;
                                        AttachmentDb.AttachmentPath = AttachPath;
                                        AttachmentDb.CreatedBy = validation.userID;
                                        AttachmentDb.CreationDate = DateTime.Now;
                                        AttachmentDb.ModifiedBy = validation.userID;
                                        AttachmentDb.Modified = DateTime.Now;
                                        AttachmentDb.Type = fileExtension;
                                        AttachmentDb.Active = true;

                                        _unitOfWork.InventoryItemAttachments.Add(AttachmentDb);
                                        _unitOfWork.Complete();

                                    }

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

        public InventoryItemRejectedOfferSupplierResponse GetInventoryItemRejectedOfferSupplierList([FromHeader] long InventoryItemID, [FromHeader] long POID, [FromHeader] long SupplierID)
        {
            InventoryItemRejectedOfferSupplierResponse Response = new InventoryItemRejectedOfferSupplierResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var ItemSupplierOfferRejectedList = new List<ItemRejectedOfferSupplier>();
                var ItemSupplierOfferAcceptedList = new List<ItemAcceptedOfferSupplier>();

                if (InventoryItemID == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err99";
                    error.ErrorMSG = "Invalid Inventory Item ID";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (Response.Result)
                {
                    var ListPRSupplietOfferItem = _unitOfWork.VPrsupplierOfferItems.FindAll(x => x.InventoryItemId == InventoryItemID).ToList();
                    var ListPRSupplierOfferItemAccepted = _unitOfWork.VPurchasePoItems.FindAll(x => x.InventoryItemId == InventoryItemID).ToList();
                    if (POID != 0)
                    {
                        ListPRSupplietOfferItem = ListPRSupplietOfferItem.Where(x => x.Poid == POID).ToList();
                        ListPRSupplierOfferItemAccepted = ListPRSupplierOfferItemAccepted.Where(x => x.Id == POID).ToList();
                    }
                    if (SupplierID != 0)
                    {
                        ListPRSupplietOfferItem = ListPRSupplietOfferItem.Where(x => x.SupplierId == SupplierID).ToList();
                        ListPRSupplierOfferItemAccepted = ListPRSupplierOfferItemAccepted.Where(x => x.ToSupplierId == SupplierID).ToList();
                    }
                    if (ListPRSupplietOfferItem.Count > 0)
                    {
                        foreach (var item in ListPRSupplietOfferItem)
                        {
                            var ItemObj = new ItemRejectedOfferSupplier();
                            ItemObj.POID = item.Poid;
                            ItemObj.SupplierID = item.SupplierId;
                            ItemObj.SupplierName = item.SupplierName;
                            ItemObj.CurrencyID = item.CurrencyId;
                            ItemObj.RecivedQuantity = item.RecivedQuantity;
                            ItemObj.ReqQuantity = item.ReqQuantity;
                            ItemObj.EstimatedCost = item.EstimatedCost;
                            ItemObj.RateToEGP = item.RateToEgp;
                            ItemObj.ItemComment = item.Comment;
                            ItemObj.SupplierOfferComment = item.SupplierOfferComment;
                            ItemObj.CreationDate = item.CreationDate.ToShortDateString();
                            ItemObj.CurrencyName = Common.GetCurrencyName(item.CurrencyId ?? 0, _Context);
                            ItemSupplierOfferRejectedList.Add(ItemObj);
                        }
                    }

                    if (ListPRSupplierOfferItemAccepted.Count > 0)
                    {
                        foreach (var item in ListPRSupplierOfferItemAccepted)
                        {
                            var ItemObj = new ItemAcceptedOfferSupplier();
                            ItemObj.POID = item.Id;
                            ItemObj.SupplierID = item.ToSupplierId;
                            ItemObj.SupplierName = Common.GetSupplierName(item.ToSupplierId ?? 0, _Context);
                            ItemObj.CurrencyID = item.CurrencyId;
                            ItemObj.RecivedQuantity = item.RecivedQuantity;
                            ItemObj.ReqQuantity = item.ReqQuantity;
                            ItemObj.EstimatedCost = item.EstimatedCost;
                            ItemObj.FinalUnitCost_EGP = item.FinalUnitCost;
                            ItemObj.ActualUnitCost_EGP = item.ActualUnitPrice;
                            ItemObj.RateToEGP = item.RateToEgp;
                            ItemObj.ItemComment = item.Comments;
                            ItemObj.CreationDate = item.PocreationDate != null ? ((DateTime)item.PocreationDate).ToShortDateString() : "";
                            ItemObj.CurrencyName = Common.GetCurrencyName(item.CurrencyId ?? 0, _Context);
                            ItemSupplierOfferAcceptedList.Add(ItemObj);
                        }
                    }
                }
                Response.ItemRejectedOfferSupplierList = ItemSupplierOfferRejectedList;
                Response.ItemAcceptedOfferSupplierList = ItemSupplierOfferAcceptedList;
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

        public GetRemainInventoryItemRequestedQtyResponse GetRemainInventoryItemRequestedQty([FromHeader] long InventoryItemId)
        {
            GetRemainInventoryItemRequestedQtyResponse Response = new GetRemainInventoryItemRequestedQtyResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (InventoryItemId != 0)
                {
                    var InventoryItemDb = _unitOfWork.InventoryItems.GetById(InventoryItemId);
                    if (InventoryItemDb != null)
                    {
                        Response.InventoryItemId = InventoryItemId;
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "InventoryItem Doesn't Exist!!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                else
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err99";
                    error.ErrorMSG = "InventoryItemId Is Mandatory";
                    Response.Errors.Add(error);
                    return Response;
                }

                if (Response.Result)
                {
                    decimal TotalInventoryItemRequestedQty = 0;
                    var RemainOpenProjectsRequestedQtyDb = _unitOfWork.VInventoryMatrialReleaseItems.FindAll(a => a.InventoryItemId == InventoryItemId && a.ReqQuantity > 0 && a.ReqQuantity > a.RecivedQuantity).ToList().GroupBy(a => a.ProjectId).ToList();
                    if (RemainOpenProjectsRequestedQtyDb != null && RemainOpenProjectsRequestedQtyDb.Count() > 0)
                    {
                        var RemainOpenProjectsRequestedQtyList = RemainOpenProjectsRequestedQtyDb.Select(project => new OpenProjectRemainRequestedItem
                        {
                            ProjectId = project.Key ?? 0,
                            ClientId = project.Select(a => a.ClientId).FirstOrDefault(),
                            ClientName = project.Select(a => a.ClientName).FirstOrDefault(),
                            RemainRequestedQty = project.Sum(a => a.ReqQuantity ?? 0) - project.Sum(a => a.RecivedQuantity ?? 0)
                        }).ToList();
                        Response.OpenProjectsRemainRequestedItem = RemainOpenProjectsRequestedQtyList;
                        Response.TotalOpenProfjectsRemainRequestedItemsQty = RemainOpenProjectsRequestedQtyList.Sum(a => a.RemainRequestedQty);
                        TotalInventoryItemRequestedQty += Response.TotalOpenProfjectsRemainRequestedItemsQty;
                    }

                    var OpenSalesOffersRequestedQtyDb = _unitOfWork.VSalesOfferProductSalesOffers.FindAll(a => a.SalesOfferActive == true && a.InventoryItemId == InventoryItemId && a.Quantity > 0 && a.Status.ToLower() != "closed" && a.Status.ToLower() != "rejected").ToList().GroupBy(a => a.OfferId).ToList();
                    if (OpenSalesOffersRequestedQtyDb != null && OpenSalesOffersRequestedQtyDb.Count() > 0)
                    {
                        var OpenSalesOffersRequestedQtyList = OpenSalesOffersRequestedQtyDb.Select(salesOffer => new OpenSalesOfferRequestedItem
                        {
                            SalesOfferId = salesOffer.Key,
                            ClientId = salesOffer.Select(a => a.ClientId).FirstOrDefault(),
                            ClientName = Common.GetClientName(salesOffer.Select(a => a.ClientId).FirstOrDefault() ?? 0, _Context),
                            RequestedQty = (decimal)salesOffer.Sum(a => a.Quantity ?? 0)
                        }).ToList();
                        Response.OpenSalesOffersRequestedItem = OpenSalesOffersRequestedQtyList;
                        Response.TotalOpenSalesOffersRequestedItemsQty = OpenSalesOffersRequestedQtyList.Sum(a => a.RequestedQty);
                        TotalInventoryItemRequestedQty += Response.TotalOpenSalesOffersRequestedItemsQty;
                    }

                    Response.TotalInventoryItemRequestedQty = TotalInventoryItemRequestedQty;
                    // var HoldItems = _Context.V_InventoryMatrialRequestItems.Where(x => x.InventoryItemID == InventoryItemId && x.IsHold == true).ToList();
                    var InvStoreItemHoldQTY = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryItemId == InventoryItemId && x.HoldQty != null).Select(x => x.HoldQty ?? 0).ToList().Sum();
                    if (InvStoreItemHoldQTY > 0)
                    {
                        Response.TotalStocksHoldItemsQty = InvStoreItemHoldQTY; // HoldItems.Sum(x => x.ReqQuantity ?? 0);
                    }
                    var StockAvailableItems = _unitOfWork.VInventoryStoreItems.FindAll(a => a.InventoryItemId == InventoryItemId && a.FinalBalance > 0 && a.StoreActive == true).ToList();
                    if (StockAvailableItems != null && StockAvailableItems.Count() > 0)
                    {
                        Response.TotalStocksAvailableItemsQty = StockAvailableItems.Select(x => x.FinalBalance ?? 0).ToList().Sum();
                    }

                    Response.TotalAvailableItemsQty = Response.TotalStocksAvailableItemsQty - Response.TotalStocksHoldItemsQty - Response.TotalInventoryItemRequestedQty;
                }

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public AccountAndFinanceInventoryItemMovementResponse GetAccountAndFinanceInventoryItemMovementListV2(GetInventoryItemMovementListV2Filters filters)
        {
            AccountAndFinanceInventoryItemMovementResponse Response = new AccountAndFinanceInventoryItemMovementResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var InventoryStoreItemMovmentList = new List<ItemMovement>();
                if (Response.Result)
                {
                    long InventoryItemID = 0;
                    if (filters.InventoryItemID == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid Inventory Item ID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    DateTime FromDateFilter = new DateTime(DateTime.Now.Year, 1, 1);  // Bishoy magdy modifications 2024-10-14
                    //DateTime FromDateTemp = DateTime.Now;
                    if (filters.FromDate != null)
                    {
                        FromDateFilter = (DateTime)filters.FromDate;
                    }


                    DateTime ToDateFilter = DateTime.Now;
                    //DateTime ToDateTemp = DateTime.Now;
                    if (filters.ToDate != null)
                    {
                        ToDateFilter = (DateTime)filters.ToDate;
                    }
                    decimal cummlativeQty = 0;
                    if (Response.Result)
                    {

                        var InventoryItemMovmentQuerable = _unitOfWork.VInventoryStoreItemMovements.FindAllQueryable(x => x.Active == true && x.InventoryItemId == InventoryItemID).OrderBy(x => x.CreationDate).AsQueryable();
                        // Filters --------
                        if (filters.OperationType != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.OperationType.Contains(filters.OperationType));
                        }

                        if (filters.PoId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.AddingFromPoid == filters.PoId);
                        }
                        // Filter 
                        if (filters.FromDate != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= FromDateFilter);
                        }

                        if (filters.ToDate != null)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.CreationDate <= ToDateFilter);
                        }

                        if (filters.ClientId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ClientId == filters.ClientId);
                        }

                        if (filters.SupplierId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.SupplierId == filters.SupplierId);
                        }
                        if (filters.ProjectId != 0)
                        {
                            InventoryItemMovmentQuerable = InventoryItemMovmentQuerable.Where(x => x.ProjectId == filters.ProjectId);
                        }


                        var InventoryStoreItemPagingList = PagedList<VInventoryStoreItemMovement>.Create(InventoryItemMovmentQuerable, filters.CurrentPage, filters.NumberOfItemsPerPage);
                        //var InventoryStoreItemPagingList = PagedList<V_InventoryStoreItem>.Create(ListInventoryItemMovmentQuerable, CurrentPage, NumberOfItemsPerPage);

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = InventoryStoreItemPagingList.CurrentPage,
                            TotalPages = InventoryStoreItemPagingList.TotalPages,
                            ItemsPerPage = InventoryStoreItemPagingList.PageSize,
                            TotalItems = InventoryStoreItemPagingList.TotalCount
                        };

                        InventoryStoreItemMovmentList = InventoryStoreItemPagingList.Select(item => new ItemMovement
                        {

                            OperationType = item.OperationType,
                            Qty = (double)item.Balance,
                            HoldQty = item.HoldQty ?? 0,
                            HoldComment = item.HoldReason,
                            OrderID = item.OrderId,
                            CumilativeQty = (double)InventoryItemMovmentQuerable.Where(x => x.CreationDate <= item.CreationDate).ToList().Select(x => x.Balance).DefaultIfEmpty(0).Sum(),
                            StoreName = item.InventoryStoreName,
                            ReqUOM = item.RequstionUomname,

                            ID = item.Id,
                            ParentID = item.ReleaseParentId,
                            POID = item.AddingFromPoid,
                            ExpDate = item.ExpDate != null ? ((DateTime)item.ExpDate).ToShortDateString() : "",
                            ItemSerial = item.ItemSerial,
                            RemainBalance = item.FinalBalance,
                            CurrencyId = item.CurrencyId,
                            CurrencyName = item.CurrencyName,
                            RateToEGP = item.RateToEgp,
                            POInvoicePriceEGP = item.PoinvoiceTotalPriceEgp,
                            POInvoiceUnitCostEGP = item.PoinvoiceTotalCostEgp,
                            CreationDate = item.CreationDate.ToShortDateString(),
                            FromUser = item.FromUser,
                            FromSupplier = item.FromSupplier,
                            SupplierId = item.SupplierId,
                            FromDepartment = item.FromDepartment,
                            OrderType = item.OrderType,
                            DateFilter = item.DateFilter ?? item.CreationDate,
                            Date = item.DateFilter != null ? item.DateFilter?.ToString("dd-MM-yyyy") : item.CreationDate.ToString("dd-MM-yyyy"),
                            ProjectName = item.ProjectName,
                            ProjectId = item.ProjectId,
                            ClientId = item.ClientId,
                            ClientName = item.ClientName

                        }).ToList();
                        var InventoryItemMovmentListFilter = InventoryItemMovmentQuerable;
                        double numberOfMonths = Math.Abs(Math.Ceiling(ToDateFilter.Subtract(FromDateFilter).Days / (365.25 / 12)));
                        if (FromDateFilter <= ToDateFilter)
                        {
                            InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= FromDateFilter && x.CreationDate <= ToDateFilter).AsQueryable();
                        }
                        else
                        {
                            InventoryItemMovmentListFilter = InventoryItemMovmentQuerable.Where(x => x.CreationDate >= ToDateFilter && x.CreationDate <= FromDateFilter).AsQueryable();
                        }
                        var ReleaseQty = InventoryItemMovmentListFilter.Where(x => x.OperationType.Contains("Release Order") ||
                                                           x.OperationType.Contains("POS Release")
                                                           ).ToList().Select(x => Math.Abs(x.Balance)).DefaultIfEmpty(0).Sum();
                        Response.ReleaseQty = (double)ReleaseQty;
                        Response.ReleaseRate = numberOfMonths != 0 ? (Response.ReleaseQty / numberOfMonths) : 0;
                        //}
                        Response.NoOfMonth = numberOfMonths;
                        Response.DateFrom = FromDateFilter.ToShortDateString();
                        //!string.IsNullOrWhiteSpace(headers["FromDate"]) ? FromDateTemp.ToString("dd-MM-yyyy") : DateFrom.ToString("dd-MM-yyy" + "+y");
                        Response.DateTo = ToDateFilter.ToShortDateString();
                        //!string.IsNullOrWhiteSpace(headers["ToDate"]) ? ToDateTemp.ToString("dd-MM-yyyy") : DateTO.ToString("dd-MM-yyyy");
                        Response.InventoryItemMovementList = InventoryStoreItemMovmentList;
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

        public SelectDDLResponse GetInventoryItemLocationList([FromHeader] long InventoryItemId, [FromHeader] int StoreId)
        {
            SelectDDLResponse Response = new SelectDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (InventoryItemId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Inventory Item Id Is Required!";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (StoreId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Store Id Is Required!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var inventoryITem = _unitOfWork.InventoryItems.GetById(InventoryItemId);
                if (inventoryITem == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err103";
                    error.ErrorMSG = "Inventory Item Not Found!";
                    Response.Errors.Add(error);
                    return Response;
                }
                var store = _unitOfWork.InventoryStores.GetById(StoreId);
                if (store == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Store not found!";
                    Response.Errors.Add(error);
                    return Response;
                }

                var AllItem = _unitOfWork.InventoryStoreItems.FindAll(a => a.InventoryItemId == InventoryItemId && a.InventoryStoreId == StoreId && a.InvenoryStoreLocationId != null, includes: new[] { "InvenoryStoreLocation" }).Select(a => new SelectDDL()
                {
                    ID = a.InvenoryStoreLocation.Id,
                    Name = a.InvenoryStoreLocation.Location
                }).ToList();
                var list = AllItem.GroupBy(p => new { p.ID, p.Name }).Select(g => g.First()).ToList();
                Response.DDLList = list;
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
