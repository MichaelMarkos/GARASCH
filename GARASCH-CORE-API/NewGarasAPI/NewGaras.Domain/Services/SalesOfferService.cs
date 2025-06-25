using AutoMapper;
using Azure;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Grpc.Core;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.DTO.Attendance;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.AccountAndFinance;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.SalesOffer;
using NewGaras.Infrastructure.Models.SalesOffer.Filters;
using NewGaras.Infrastructure.Models.SalesOffer.UsedInResponses;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.AccountAndFinance;
using NewGarasAPI.Models.User;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using Color = System.Drawing.Color;

namespace NewGaras.Domain.Services
{
    public class SalesOfferService : ISalesOfferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private readonly IAccountMovementService _movementService;
        private readonly IAccountAndFinanceService _accountAndFinanceService;
        private GarasTestContext _Context;
        static readonly string key = "SalesGarasPass";
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
        public SalesOfferService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host, IAccountMovementService movementService, GarasTestContext context, IAccountAndFinanceService accountAndFinanceService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
            _movementService = movementService;
            _accountAndFinanceService = accountAndFinanceService;
            _Context = context;
        }



        public ExcelWorksheet MergeCells(ExcelWorksheet worksheet, string from)
        {
            if (from == "all")
            {
                string searchValue = worksheet.Cells[4, 24].Value == null || worksheet.Cells[4, 24].Value.ToString() == "" ? null : worksheet.Cells[4, 24].Value.ToString();
                var first = 4;
                var end = 4;
                for (int currentRow = 5; currentRow <= worksheet.Dimension.End.Row; currentRow++)
                {
                    if (worksheet.Cells[currentRow, 24].Value != null && worksheet.Cells[currentRow, 24].Value.ToString() != searchValue)
                    {
                        searchValue = worksheet.Cells[currentRow, 24].Value == null || worksheet.Cells[currentRow, 24].Value.ToString() == "" ? null : worksheet.Cells[currentRow, 24].Value.ToString();
                        worksheet.Cells[first, 3, end, 3].Merge = true;
                        worksheet.Cells[first, 4, end, 4].Merge = true;
                        //worksheet.Cells[first, 5, end, 5].Merge = true;
                        worksheet.Cells[first, 15, end, 15].Merge = true;
                        worksheet.Cells[first, 16, end, 16].Merge = true;
                        worksheet.Cells[first, 17, end, 17].Merge = true;
                        worksheet.Cells[first, 18, end, 18].Merge = true;
                        worksheet.Cells[first, 19, end, 19].Merge = true;
                        //worksheet.Cells[first, 19, end, 19].Merge = true;
                        worksheet.Cells[first, 21, end, 21].Merge = true;
                        worksheet.Cells[first, 22, end, 22].Merge = true;
                        worksheet.Cells[first, 23, end, 23].Merge = true;
                        first = currentRow;
                        end = currentRow;
                    }
                    else
                    {
                        end++;
                    }
                }
            }
            else if (from == "rent")
            {
                string searchValue = worksheet.Cells[4, 27].Value == null || worksheet.Cells[4, 27].Value.ToString() == "" ? null : worksheet.Cells[4, 27].Value.ToString();
                var first = 4;
                var end = 4;

                for (int currentRow = 5; currentRow <= worksheet.Dimension.End.Row; currentRow++)
                {
                    if (currentRow == 46)
                    {
                        searchValue = "";
                    }
                    if (worksheet.Cells[currentRow, 27].Value != null && worksheet.Cells[currentRow, 27].Value.ToString() != searchValue)
                    {
                        if (worksheet.Cells[first, 27].Value == null || string.IsNullOrEmpty(worksheet.Cells[first, 27].Value.ToString()))
                        {
                            first = currentRow;
                            end = first;
                        }
                        else
                        {
                            searchValue = worksheet.Cells[currentRow, 27].Value == null || worksheet.Cells[currentRow, 27].Value.ToString() == "" ? null : worksheet.Cells[currentRow, 27].Value.ToString();
                            worksheet.Cells[first, 15, end, 15].Merge = true;
                            worksheet.Cells[first, 16, end, 16].Merge = true;
                            worksheet.Cells[first, 17, end, 17].Merge = true;
                            worksheet.Cells[first, 18, end, 18].Merge = true;
                            worksheet.Cells[first, 19, end, 19].Merge = true;
                            worksheet.Cells[first, 20, end, 20].Merge = true;
                            worksheet.Cells[first, 21, end, 21].Merge = true;
                            worksheet.Cells[first, 25, end, 25].Merge = true;
                            worksheet.Cells[first, 26, end, 26].Merge = true;
                            first = currentRow;
                            end = currentRow;
                        }
                    }
                    else
                    {
                        if (worksheet.Cells[currentRow, 27].Value == null || string.IsNullOrEmpty(worksheet.Cells[currentRow, 27].Value.ToString()))
                        {
                            first++;
                            end = first;
                        }
                        else
                        {
                            end++;
                        }
                    }
                }
            }
            return worksheet;
        }

        public BaseResponseWithData<ExcelWorksheet> SalesOfferItemsReport(List<long> offersId, string CompanyName, DateTime From, DateTime To)
        {
            BaseResponseWithData<ExcelWorksheet> Response = new BaseResponseWithData<ExcelWorksheet>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            long test = 0;
            try
            {
                var products = _unitOfWork.SalesOfferProducts.FindAllQueryable(a =>offersId.Contains(a.OfferId), includes: new[] { "Offer.Client", "Offer.SalesPerson", "Product", "InventoryItemCategory", "Offer.Invoices", "SalesOfferProductTaxes", "Offer.Projects.ClientAccounts.AccountOfJe.Account.AdvanciedSettingAccounts", "InventoryItem.InventoryStoreItems" });
                /*if (salesperson != null)
                {
                    products = products.Where(a => a.Offer.SalesPersonId == SalesPersonId);
                }*/
                var items = products.OrderBy(a => a.Offer.OfferSerial).ToList();
                string fileInfo = _host.WebRootPath + @$"\Attachments\{CompanyName}\Templates\SalesOffersItemsTemplate.xlsx";

                if (!File.Exists(fileInfo))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err104";
                    error.ErrorMSG = "Template not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                ExcelPackage package = new ExcelPackage(new FileInfo(fileInfo));
                var MonthNamefrom = From.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("ar"));
                var MonthNameTo = To.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("ar"));
                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                worksheet.Cells["A1"].Value = $"مبيعات - الفترة من {MonthNamefrom} {From.Year} الي {MonthNameTo} {To.Year}";
                //worksheet.Cells["Q3"].Value = $"{MonthName}";
                /*if (salesperson != null)
                {
                    worksheet.Cells["A4"].Value = $" مبيعات مارينا للسقالات والروافع -  {salesperson.FirstName + ' ' + salesperson.LastName}";
                }
                else
                {
                    worksheet.Cells["A4"].Value = $"مبيعات مارينا للسقالات والروافع";
                }*/
                worksheet.DefaultRowHeight = 56;
                int rowIndex = 4;
                int counter = 1;
                foreach (var item in items)
                {
                    test = item.Id;
                    worksheet.Cells[rowIndex, 1].Value = counter;
                    worksheet.Cells[rowIndex, 2].Value = item.Offer?.OfferSerial == null ? "" : item.Offer?.OfferSerial.ToString();
                    worksheet.Cells[rowIndex, 3].Value = item.Offer?.Client.Name ?? "";
                    worksheet.Cells[rowIndex, 4].Value = (item.Offer?.SalesPerson.FirstName + " " + item.Offer.SalesPerson.LastName) ?? "";
                    worksheet.Cells[rowIndex, 5].Value = (item.InventoryItemCategory?.Name ?? "") + " / " + (item.InventoryItem?.Name ?? "");
                    worksheet.Cells[rowIndex, 6].Value = item.Quantity;
                    worksheet.Cells[rowIndex, 7].Value = item.Offer?.ProjectStartDate != null ? ((DateTime)item.Offer?.ProjectStartDate).ToShortDateString() : "";
                    worksheet.Cells[rowIndex, 8].Value = string.Join(@"\n", item.Offer?.Invoices.Where(a => a.Serial != null).Select(a => a.Serial)).ToString();
                    worksheet.Cells[rowIndex, 9].Value = item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate != null ? ((DateTime)item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate).ToShortDateString() : "";
                    worksheet.Cells[rowIndex, 9].Value = item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate != null ? ((DateTime)item.Offer?.Invoices.FirstOrDefault()?.InvoiceDate).ToShortDateString() : "";
                    worksheet.Cells[rowIndex, 10].Value = item.FinalPrice;
                    worksheet.Cells[rowIndex, 11].Value = item.DiscountValue ?? 0;
                    worksheet.Cells[rowIndex, 12].Value = item.FinalPrice - (item.DiscountValue) ?? 0;
                    worksheet.Cells[rowIndex, 13].Value = item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                    worksheet.Cells[rowIndex, 24].Value = item.Offer?.Id.ToString();
                    worksheet.Cells[rowIndex, 14].Value = (item.FinalPrice - item.DiscountValue ?? 0) + item.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum();
                    worksheet.Cells[rowIndex, 15].Value = items.Where(a => a.OfferId == item.OfferId).Sum(a => (a.FinalPrice - a.DiscountValue ?? 0) + a.SalesOfferProductTaxes.Select(a => a.Value ?? 0).Sum());
                    worksheet.Cells[rowIndex, 25].Value = item.InventoryItem.InventoryStoreItems.OrderByDescending(a => a.Id).Select(a => a.PoinvoiceTotalCostEgp).FirstOrDefault();
                    worksheet.Cells[rowIndex, 26].Value = item.InventoryItem.InventoryStoreItems.Where(a => a.FinalBalance > 0).Select(a => a.PoinvoiceTotalCostEgp).Average() ?? item.InventoryItem.InventoryStoreItems.Select(a=>a.AverageUnitPrice).FirstOrDefault();
                    worksheet.Cells[rowIndex, 27].Value = item.InventoryItem.InventoryStoreItems.Select(a => a.PoinvoiceTotalCostEgp).Max();
                    /*worksheet.Cells[rowIndex, 16].Value =
                        item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount) -
                        item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount);*/

                    /*worksheet.Cells[rowIndex, 17].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.AccountOfJe != null && a.CreationDate.Month == Month && a.CreationDate.Year == Year && a.AccountOfJe.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 30).Any()).Sum(a => a.Amount);*/

                    /*var projectId = item.Offer.Projects.Select(a => a.Id).FirstOrDefault();
                    decimal TreasuaryAmount = 0;
                    decimal PermissoryAmount = 0;
                    var ClientAccountList = _unitOfWork.ClientAccounts.FindAll(x => x.Project.Id == projectId).ToList();
                    var AccountsIDSList = ClientAccountList.Select(x => x.AccountId).ToList();
                    var EntriesIDSList = ClientAccountList.Select(x => x.DailyAdjustingEntryId).ToList();

                    if (AccountsIDSList.Count > 0 && EntriesIDSList.Count > 0)
                    {
                        var AccountOfJonalEntriesList = _unitOfWork.AccountOfJournalEntries
                            .FindAll(x => EntriesIDSList.Contains(x.EntryId) && !x.Account.AdvanciedSettingAccounts.Where(a => a.AdvanciedTypeId == 30).Any() && x.CreationDate.Month == Month && x.CreationDate.Year == DateTime.Now.Year, includes: new[] { "Account.AdvanciedSettingAccounts" }).ToList();

                        TreasuaryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null &&
                            x.Account.AdvanciedSettingAccounts.ToList().W   here(x => x.AdvanciedTypeId == 2).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();



                        PermissoryAmount = AccountOfJonalEntriesList.Where(x => x.SignOfAccount == "plus" && x.Account != null && x.Account.AdvanciedSettingAccounts.Where(x => x.AdvanciedTypeId == 3).Any()).Select(x => x.Amount).DefaultIfEmpty(0).Sum();


                    }
                    worksheet.Cells[rowIndex, 18].Value = PermissoryAmount;
                    worksheet.Cells[rowIndex, 19].Value = TreasuaryAmount;*/

                    decimal taxpercentage = decimal.Parse(worksheet.Cells[rowIndex, 12].Value.ToString()) != 0 ? ((decimal.Parse(worksheet.Cells[rowIndex, 13].Value.ToString()) / decimal.Parse(worksheet.Cells[rowIndex, 12].Value.ToString())) * 100) : 0;
                    //worksheet.Cells[rowIndex, 20].Value = decimal.Parse(worksheet.Cells[rowIndex, 17].Value.ToString()) / (1 + taxpercentage);


                    worksheet.Cells[rowIndex, 21].Value = item.Offer.Projects.SelectMany(a => a.ClientAccounts).Where(a => a.CreationDate >= From && a.CreationDate <= To && a.AmountSign == "plus").Sum(a => a.Amount);

                    worksheet.Cells[rowIndex, 22].Value = (decimal.Parse(worksheet.Cells[rowIndex, 15].Value.ToString()) - decimal.Parse(worksheet.Cells[rowIndex, 21].Value.ToString()));

                    worksheet.Cells[rowIndex, 23].Value = decimal.Parse(worksheet.Cells[rowIndex, 15].Value.ToString()) != 0 ? ((decimal.Parse(worksheet.Cells[rowIndex, 21].Value.ToString()) / decimal.Parse(worksheet.Cells[rowIndex, 15].Value.ToString())) * 100) : 0;


                    worksheet.Row(rowIndex).CustomHeight = false;
                    worksheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    counter++;
                    rowIndex++;
                }
                worksheet.Column(1).AutoFit();
                worksheet.Column(2).AutoFit();
                worksheet.Column(3).Style.WrapText = true;
                worksheet.Column(3).Width *= 2;
                worksheet.Column(4).AutoFit();
                worksheet.Column(5).AutoFit();
                worksheet.Column(6).AutoFit();
                worksheet.Column(7).AutoFit();
                worksheet.Column(8).AutoFit();
                worksheet.Cells[6, 10, worksheet.Dimension.End.Row, 22].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[6, 25, worksheet.Dimension.End.Row, 27].Style.Numberformat.Format = "#,##0.00";
                worksheet.Column(9).AutoFit();
                worksheet.Column(11).AutoFit();
                worksheet.Column(12).AutoFit();
                worksheet.Column(13).AutoFit();
                worksheet.Column(15).AutoFit();
                worksheet.Column(16).AutoFit();
                worksheet.Column(17).AutoFit();
                worksheet.Column(18).AutoFit();
                worksheet.Column(20).AutoFit();
                worksheet.Column(23).Style.Numberformat.Format = "#0\\.00%";
                worksheet.Column(21).AutoFit();
                worksheet.Column(22).AutoFit();
                worksheet.Column(23).AutoFit();
                worksheet.Column(25).AutoFit();
                worksheet.Column(26).AutoFit();
                worksheet.Column(27).AutoFit();
                worksheet.Column(24).Hidden = true;
                worksheet = MergeCells(worksheet, "all");
                worksheet.View.FreezePanes(4, 1);
                worksheet.DeleteColumn(16);
                worksheet.DeleteColumn(16);
                worksheet.DeleteColumn(16);
                worksheet.DeleteColumn(16);
                worksheet.DeleteColumn(16);

                Response.Data = worksheet;

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message + " " + test;
                Response.Errors.Add(error);
                return Response;
            }
        }


        public bool GetParentCategory(InventoryItemCategory category, long parentId)
        {
            bool result = false;
            if (category.Id == parentId)
            {
                result = true;
            }
            if (category?.ParentCategoryId != null)
            {
                if (category?.ParentCategoryId == parentId)
                {
                    result = true;
                }
                else
                {
                    result = GetParentCategory(category.ParentCategory, parentId);
                }
            }
            return result;
        }

        public BaseResponseWithData<string> SalesOfferItemsCategoryReport(string CompanyName, int Year, int Month, long SalesPersonId)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Month < 1 || Month > 12)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "month value should be between 1 and 12";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (Year < DateTime.MinValue.Year || Year > DateTime.Now.Year)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "Year Value not right";
                    Response.Errors.Add(error);
                    return Response;
                }
                User? salesperson = null;
                if (SalesPersonId != 0)
                {
                    salesperson = _unitOfWork.Users.GetById(SalesPersonId);
                    if (salesperson == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "salesperson not found";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                var datetime = new DateTime(Year, Month, 1);
                var MonthName = datetime.ToString("MMMM", System.Globalization.CultureInfo.GetCultureInfo("ar"));
                var products = _unitOfWork.SalesOfferProducts.FindAllQueryable(a => ((DateTime)a.Offer.ClientApprovalDate).Year == Year && ((DateTime)a.Offer.ClientApprovalDate).Month == Month 
                && a.Offer.Status == "Closed", includes: new[] { "Offer.Client", "Offer.SalesPerson", "Product", "InventoryItemCategory.ParentCategory", "Offer.Invoices", "SalesOfferProductTaxes", "Offer.Projects.ClientAccounts.AccountOfJe.Account.AdvanciedSettingAccounts", "InventoryItem", "Offer.Projects.ProjectProgresses.ProjectProgressUsers.InventoryItemCategory" });

                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[8] { new DataColumn("م"),
                                                    new DataColumn("رقم امر الشغل"),
                                                    new DataColumn("اسم العميل"),
                                                    new DataColumn("المندوب"),
                                                    new DataColumn("اسم المعدة"),
                                                    new DataColumn("العدد"),
                                                    new DataColumn("Creation Date"),
                                                    new DataColumn("Client Approval Date"),
                                  });

                var categories = _unitOfWork.InventoryItemCategories.FindAll(a => a.ParentCategoryId == null).ToList();
                var items = products.ToList().OrderBy(a=>a.Offer.OfferSerial).ToList();
                foreach (var category in categories)
                {
                    dt.Columns.Add(new DataColumn(category.Name + $" ({items.Where(a => GetParentCategory(a.InventoryItemCategory, category.Id)).Count()})", typeof(decimal)));
                }
                foreach (var item in items)
                {
                    dt.Rows.Add(items.IndexOf(item) + 1,
                            item.Offer.OfferSerial == null ? "" : item.Offer.OfferSerial.ToString(),
                            item.Offer.Client.Name ?? "-",
                            (item.Offer.SalesPerson.FirstName + " " + item.Offer.SalesPerson.LastName) ?? "-",
                            (item.InventoryItem?.Name ?? "-"),
                            item.Quantity,
                            item.Offer.CreationDate.ToShortDateString(),
                            item.Offer.ClientApprovalDate?.ToShortDateString() ?? ""
                        );
                    //if (item.Quantity == 200)
                    //{
                    //    var test = "hello";
                    //}
                    categories.ForEach(a =>
                    {
                        //if(item.Offer.OfferSerial== "#5210-12-2024")
                        //{
                        //    var test = "hi";
                        //}
                        dt.Rows[items.IndexOf(item)][8 + categories.IndexOf(a)] = GetParentCategory(item.InventoryItemCategory, a.Id) ? ((item.FinalPrice ?? 0) ) : 0;
                    });

                }



                var workSheet = package.Workbook.Worksheets.Add("SalesOfferItemsCategory");
                workSheet.View.RightToLeft = true;
                workSheet.Cells.LoadFromDataTable(dt, true);
                workSheet.InsertRow(2, 1);
                workSheet.Cells[2, 1].Value = "الإجمالي";
                workSheet.Cells[2, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[2, 8].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                workSheet.Cells[2, 8].Style.Font.Color.SetColor(Color.Black);
                workSheet.Cells[2, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                foreach (var category in categories)
                {
                    workSheet.Cells[2, 9 + categories.IndexOf(category)].Value = products.Where(a => a.InventoryItemCategoryId == category.Id || a.InventoryItemCategory.ParentCategoryId == category.Id).Sum(a => a.FinalPrice ?? 0 * (decimal)(a.Quantity ?? 0));
                    workSheet.Column(9 + categories.IndexOf(category)).Style.Numberformat.Format = "#,##0.00";
                }


                workSheet.Cells[1, 1, workSheet.Dimension.End.Row, workSheet.Dimension.End.Column].Style.Font.Size = 12;
                workSheet.Cells[3, 7, workSheet.Dimension.End.Row, 6 + categories.Count()].Style.Numberformat.Format = "#,##0.00";
               
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 25;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;

                workSheet.Row(2).Height = 25;
                workSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(2).Style.Font.Bold = true;

                foreach (var category in categories)
                {
                    workSheet.Cells[1, workSheet.Dimension.End.Column + 1].Value = category.Name;
                    workSheet.Cells[1, workSheet.Dimension.End.Column + 1].Value = category.Name;
                    workSheet.Cells[1, workSheet.Dimension.End.Column - 1, 1, workSheet.Dimension.End.Column].Merge = true;
                    workSheet.Cells[2, workSheet.Dimension.End.Column-1].Value = "عدد ساعات";
                    workSheet.Cells[2, workSheet.Dimension.End.Column].Value = "عدد عمال";
                }
                var counter = 3;
                foreach(var item in items)
                {
                    var progressUsers =  item.Offer.Projects.SelectMany(a => a.ProjectProgresses.SelectMany(x => x.ProjectProgressUsers)).ToList();
                    foreach (var category in categories)
                    {
                        workSheet.Cells[counter, workSheet.Dimension.End.Column - 2 * categories.Count() + 2 * categories.IndexOf(category) + 1].Value = progressUsers.Where(a => GetParentCategory(a.InventoryItemCategory, category.Id)).Sum(a => a.HoursNum);
                        workSheet.Cells[counter, workSheet.Dimension.End.Column - 2 * categories.Count() + 2 * categories.IndexOf(category) + 2].Value = progressUsers.Where(a => GetParentCategory(a.InventoryItemCategory, category.Id)).Count();
                    }
                    counter++;
                }
                for (int i = 1; i <= workSheet.Dimension.End.Column; i++)
                {
                    workSheet.Column(i).AutoFit();
                    workSheet.Cells[1, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    workSheet.Column(i).Width *= 1.1;
                    if (i > 6)
                    {
                        workSheet.Cells[2, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }
                for (int i = 1; i <= workSheet.Dimension.End.Row; i++)
                {
                    workSheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(201, 201, 201));
                workSheet.Cells[1, 1, 1, workSheet.Dimension.End.Column].Style.Font.Color.SetColor(Color.Black);

                workSheet.Cells[2, 1, 2, workSheet.Dimension.End.Column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[2, 1, 2, workSheet.Dimension.End.Column].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                workSheet.Cells[2, 1, 2, workSheet.Dimension.End.Column].Style.Font.Color.SetColor(Color.Black);
                workSheet.Cells[2, 1, 2, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                workSheet.Cells[2, 1, 2, 6].Merge = true;
                workSheet.InsertRow(1, 1);
                workSheet.Row(1).Height = 25;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 8 + categories.Count()].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 8 + categories.Count()].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                workSheet.Cells[1, 1, 1, 8 + categories.Count()].Style.Font.Size = 12;
                workSheet.Cells[1, 1, 1, 8 + categories.Count()].Style.Font.Color.SetColor(Color.Black);
                workSheet.Cells["A1"].Value = $"مبيعات - شهر {MonthName} {datetime.Year}";
                workSheet.Cells[1, 1, 1, 8 + categories.Count()].Merge = true;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.InsertColumn(9+categories.Count(), 1);
                workSheet.View.FreezePanes(4, 1);
                workSheet.Column(9 + categories.Count()).Width = 1;
                workSheet.Cells[1, 10 + categories.Count(), 1, workSheet.Dimension.End.Column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 10 + categories.Count(), 1, workSheet.Dimension.End.Column].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                workSheet.Cells[1, 10 + categories.Count(), 1, workSheet.Dimension.End.Column].Style.Font.Size = 12;
                workSheet.Cells[1, 10 + categories.Count(), 1, workSheet.Dimension.End.Column].Style.Font.Color.SetColor(Color.Black);
                workSheet.Cells["M1"].Value = $"ساعات العمل";
                workSheet.Cells[1, 10 + categories.Count(), 1, workSheet.Dimension.End.Column].Merge = true;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var newpath = $"Attachments\\{CompanyName}\\SalesOfferItemsCategoryReports";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\SalesOfferItemsCategoryReport_{date}.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\SalesOfferItemsCategoryReport_{date}.xlsx";

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
        public GetCalcDetailsResponse GetCalcSupplierCollectedDetails(long clientId)
        {
            GetCalcDetailsResponse Response = new GetCalcDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            if (Response.Result)
            {
                var ClientObjDB = _unitOfWork.Clients.FindAll(x => x.Id == clientId).FirstOrDefault();
                if (ClientObjDB == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-P212";
                    error.ErrorMSG = "Invalid Client Id";
                    Response.Errors.Add(error);
                    return Response;
                }
                var TotalProjectAmount = ClientObjDB.SalesOffers.Sum(x => x.Projects.Where(y => y.Active == true).Select(t => t.TotalCost).Sum());
                Response.TotalCollected = ClientObjDB.ClientAccounts.Where(x => x.Active == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum();
                Response.TotalAmount = TotalProjectAmount ?? 0;
                Response.Remain = Response.TotalAmount > Response.TotalCollected ? Response.TotalAmount - Response.TotalCollected : 0;
            }
            return Response;
        }
        public BaseResponseWithData<string> SalesOfferReport(SalesOfferReportFilter filters, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "SalesOfferProducts", "Projects.InventoryMatrialRequestItems", "SalesPerson" }).AsQueryable();
                if (filters.SalesPersonId != 0)
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                }
                if (filters.BranchId != 0)
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.ProductType))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProductType.ToLower().Trim() == filters.ProductType.ToLower().Trim()).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.ClientName))
                {
                    filters.ClientName = HttpUtility.UrlDecode(filters.ClientName);

                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filters.ClientName)).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.ProjectName))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(filters.ProjectName)).AsQueryable();
                }
                if (!string.IsNullOrWhiteSpace(filters.OfferStatus))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status.ToLower().Trim() == filters.OfferStatus);
                }

                if (filters.From.Date != DateTime.Now.Date || filters.To.Date != DateTime.Now.Date)
                {
                    if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= filters.From && a.ClientApprovalDate <= filters.To).AsQueryable();
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime(filters.From) && a.EndDate <= DateOnly.FromDateTime(filters.To)).AsQueryable();
                    }
                }
                else
                {
                    if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                    {
                        var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                    }
                }
                if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate);
                }
                else
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
                }

                var OffersListDB = SalesOfferDBQuery.ToList();
                var OffersListResponse = new List<SalesOfferForReport>();

                foreach (var offer in OffersListDB)
                {

                    decimal TotalSalesOfferAvgPrice = 0;
                    decimal TotalSalesOfferProfitMarginValue = 0;
                    decimal TotalSalesOfferProfitMarginPer = 0;
                    decimal totalproductcost = 0;
                    foreach (var product in offer.SalesOfferProducts)
                    {
                        var ItemPrice = product.ItemPrice ?? 0;
                        var ItemProfitPer = product.ProfitPercentage ?? 0;
                        var ItemRemainQty = product.RemainQty ?? product.Quantity ?? 0;

                        var ItemAvg = (100 + ItemProfitPer) > 0 ? 100 * ItemPrice / (100 + ItemProfitPer) : 0;
                        totalproductcost += ItemAvg;
                        TotalSalesOfferProfitMarginValue += (100 + ItemProfitPer) > 0 ? (ItemPrice - (100 * ItemPrice / (100 + ItemProfitPer))) * (decimal)ItemRemainQty : 0;

                        TotalSalesOfferAvgPrice += ItemAvg * (decimal)ItemRemainQty;

                    }

                    if (TotalSalesOfferAvgPrice > 0)
                    {
                        TotalSalesOfferProfitMarginPer = TotalSalesOfferProfitMarginValue / TotalSalesOfferAvgPrice * 100;
                    }
                    long? projectId = null;
                    decimal QTYOfMatrialReleaseItem = 0;
                    decimal PriceOfMatrialReleaseItem = 0;
                    var QTYOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.RemainQty ?? 0) ?? 0;
                    var PriceOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.FinalPrice ?? 0) ?? 0;
                    var SalesOfferProductCount = offer.SalesOfferProducts?.Count ?? 0;
                    if (offer.Projects.Count > 0)
                    {
                        var offerProject = offer.Projects.FirstOrDefault();
                        projectId = offerProject.Id;
                        if (offerProject.InventoryMatrialRequestItems.Count > 0)
                        {
                            QTYOfMatrialReleaseItem = (decimal)(offerProject.InventoryMatrialRequestItems?.Sum(x => x.RecivedQuantity1 ?? 0) ?? 0);
                            var releasedIds = offerProject.InventoryMatrialRequestItems?.Select(a => a.OfferItemId).ToList();
                            var releasedList = offer.SalesOfferProducts?.Where(a => releasedIds.Contains(a.Id)).ToList();
                            PriceOfMatrialReleaseItem = releasedList.Sum(a => a.FinalPrice ?? 0);
                        }
                    }
                    decimal PercentQty = 0;
                    decimal PercentValue = 0;
                    string ReleaseStatus = "";
                    if (QTYOfSalesOfferProduct > 0)
                    {
                        ReleaseStatus = "Part";
                        PercentQty = QTYOfSalesOfferProduct != 0 ? QTYOfMatrialReleaseItem / (decimal)QTYOfSalesOfferProduct * 100 : 0;
                        PercentValue = PriceOfSalesOfferProduct != 0 ? PriceOfMatrialReleaseItem / PriceOfSalesOfferProduct * 100 : 0;
                        if (PercentQty > 100)
                        {
                            ReleaseStatus = "Exceeded";
                        }
                        if (PercentQty == 100)
                        {
                            ReleaseStatus = "Full";
                        }
                    }
                    var amounts = GetCalcSupplierCollectedDetails((long)offer.ClientId);
                    SalesOfferForReport salesOfferObj = new SalesOfferForReport()
                    {
                        Id = offer.Id,
                        ProjectName = offer.ProjectName,
                        SalesPersonName = offer.SalesPerson.FirstName + ' ' + offer.SalesPerson.MiddleName + ' ' + offer.SalesPerson.LastName,
                        ClientName = offer.Client?.Name ?? "",
                        OfferSerial = offer.OfferSerial,
                        OfferStatus = offer.Status,
                        PercentReleasedQty = PercentQty,
                        PercentReleasedValue = PercentValue,
                        FinalOfferPrice = offer.FinalOfferPrice,
                        ClientApprovalDate = offer.ClientApprovalDate != null ? offer.ClientApprovalDate.ToString().Split(' ')[0] : null,
                        OfferType = offer.OfferType,
                        CreationDate = offer.CreationDate.ToShortDateString(),
                        GrossProfitPercentage = TotalSalesOfferProfitMarginPer,
                        GrossProfitValue = TotalSalesOfferProfitMarginValue,
                        DiscountOrExtraCostPerSalesPerson = offer.ExtraOrDiscountPriceBySalesPerson,
                        OfferAmount = offer.OfferAmount,
                        TaxValue = (offer.SalesOfferProducts.SelectMany(x => x.SalesOfferProductTaxes?.Select(y => y.Value ?? 0).ToList()).DefaultIfEmpty(0).Sum()) +
                        (offer.SalesOfferInvoiceTaxes?.Where(x => x.Active == true).Select(x => x.TaxValue).DefaultIfEmpty(0).Sum()),
                        Discount = offer.SalesOfferProducts.Where(x => x.Active == true).Select(x => x.DiscountValue).DefaultIfEmpty(0).Sum() +
                        (offer.SalesOfferDiscounts?.Where(x => x.Active == true).Select(x => x.DiscountValue).DefaultIfEmpty(0).Sum()),
                        ExtraCost = (offer.SalesOfferExtraCosts?.Where(x => x.Active == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum()),
                        Totalcost = (decimal)amounts.TotalAmount,
                        CollectedPayment = (decimal)amounts.TotalCollected,
                        Remain = (decimal)(offer.FinalOfferPrice ?? 0 - amounts.TotalCollected),
                        ProductsCost = totalproductcost
                    };

                    var OfferClientAccount = _unitOfWork.ClientAccounts.FindAll(a => a.OfferId == offer.Id).FirstOrDefault();
                    if (OfferClientAccount != null)
                    {
                        salesOfferObj.HasJournalEntryId = OfferClientAccount.DailyAdjustingEntryId;
                    }
                    OffersListResponse.Add(salesOfferObj);

                }
                //salesOffer Tab
                var SalesOfferList = OffersListResponse;
                ExcelPackage excel = new ExcelPackage();
                var SalesOfferSheet = excel.Workbook.Worksheets.Add("SalesOffer");
                SalesOfferSheet.DefaultRowHeight = 12;
                SalesOfferSheet.Row(2).Height = 20;
                SalesOfferSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                SalesOfferSheet.Row(2).Style.Font.Bold = true;
                SalesOfferSheet.Cells[2, 1, 2, 21].Style.Fill.PatternType = ExcelFillStyle.Solid;
                SalesOfferSheet.Cells[2, 1, 2, 21].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                SalesOfferSheet.Cells[2, 1, 2, 21].Style.Font.Color.SetColor(Color.White);
                SalesOfferSheet.Cells[2, 1].Value = "Project Name";
                SalesOfferSheet.Cells[2, 2].Value = "Sales Person";
                SalesOfferSheet.Cells[2, 3].Value = "Client Name";
                SalesOfferSheet.Cells[2, 4].Value = "Serial";
                SalesOfferSheet.Cells[2, 5].Value = "Offer Amount";
                SalesOfferSheet.Cells[2, 6].Value = "Final Offer Price";
                SalesOfferSheet.Cells[2, 7].Value = "Released Values %";
                SalesOfferSheet.Cells[2, 8].Value = "Products Price in Stock";
                SalesOfferSheet.Cells[2, 9].Value = "Collected Payment";
                SalesOfferSheet.Cells[2, 10].Value = "Remaining Amount";
                SalesOfferSheet.Cells[2, 11].Value = "Client approval date";
                SalesOfferSheet.Cells[2, 12].Value = "Project Type";
                SalesOfferSheet.Cells[2, 13].Value = "Creation Date";
                SalesOfferSheet.Cells[2, 14].Value = "Gross Profit Value";
                SalesOfferSheet.Cells[2, 15].Value = "Gross Profit Percentage";
                SalesOfferSheet.Cells[2, 16].Value = "Journal Entry ID";
                SalesOfferSheet.Cells[2, 17].Value = "Has Journal Entry";
                SalesOfferSheet.Cells[2, 18].Value = "Invoice ID";
                SalesOfferSheet.Cells[2, 19].Value = "Invoice Serial";
                SalesOfferSheet.Cells[2, 20].Value = "Has E Invoice";
                SalesOfferSheet.Cells[2, 21].Value = "Status";

                var data = SalesOfferItemsReport(SalesOfferList.Select(a => a.Id).ToList(), CompanyName, filters.From, filters.To);
                Response.Errors = data.Errors;
                excel.Workbook.Worksheets.Add("SalesOfferItems", data.Data);
                //salesofferdetails tab
                var OfferDetailsSheet = excel.Workbook.Worksheets.Add("SalesOfferDetails");
                OfferDetailsSheet.DefaultRowHeight = 12;
                OfferDetailsSheet.Row(1).Height = 20;
                OfferDetailsSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                OfferDetailsSheet.Row(1).Style.Font.Bold = true;
                OfferDetailsSheet.Cells[1, 1, 1, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                OfferDetailsSheet.Cells[1, 1, 1, 10].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                OfferDetailsSheet.Cells[1, 1, 1, 10].Style.Font.Color.SetColor(Color.White);
                OfferDetailsSheet.Cells[1, 1].Value = "Project Name";
                OfferDetailsSheet.Cells[1, 2].Value = "Sales Person";
                OfferDetailsSheet.Cells[1, 3].Value = "Client Name";
                OfferDetailsSheet.Cells[1, 4].Value = "Serial";
                OfferDetailsSheet.Cells[1, 5].Value = "Offer Amount";
                OfferDetailsSheet.Cells[1, 6].Value = "Tax Value";
                OfferDetailsSheet.Cells[1, 7].Value = "Extra Cost";
                OfferDetailsSheet.Cells[1, 8].Value = "Discount";
                OfferDetailsSheet.Cells[1, 9].Value = "Extra cost or discount by sales Person";
                OfferDetailsSheet.Cells[1, 10].Value = "Final Offer Price";

                if (SalesOfferList.Count > 0)
                {
                    var IDsalesofferList = SalesOfferList.Select(y => y.Id).ToList();
                    var InvoiceListDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId != null ? IDsalesofferList.Contains((long)x.SalesOfferId) : false).ToList();

                    SalesOfferSheet.Cells[1, 5].Value = SalesOfferList.Sum(a => a.OfferAmount ?? 0);
                    SalesOfferSheet.Cells[1, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SalesOfferSheet.Cells[1, 5].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    SalesOfferSheet.Cells[1, 6].Value = SalesOfferList.Sum(a => a.FinalOfferPrice ?? 0);
                    SalesOfferSheet.Cells[1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SalesOfferSheet.Cells[1, 6].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    SalesOfferSheet.Cells[1, 8].Value = "سعر الشراء";
                    SalesOfferSheet.Cells[1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SalesOfferSheet.Cells[1, 8].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    SalesOfferSheet.Cells[1, 9].Value = SalesOfferList.Sum(a => a.CollectedPayment);
                    SalesOfferSheet.Cells[1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SalesOfferSheet.Cells[1, 9].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    SalesOfferSheet.Cells[1, 10].Value = SalesOfferList.Sum(a => a.Remain);
                    SalesOfferSheet.Cells[1, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SalesOfferSheet.Cells[1, 10].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    SalesOfferSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    int recordIndex = 3;

                    foreach (var item in SalesOfferList)
                    {
                        var InvoiceDB = InvoiceListDB.Where(x => x.SalesOfferId == item.Id).FirstOrDefault();
                        long InvoiceID = 0;
                        string InvoiceSerial = "";
                        bool HasEinvoice = false;
                        if (InvoiceDB != null)
                        {
                            InvoiceID = InvoiceDB.Id;
                            InvoiceSerial = InvoiceDB.Serial;
                            HasEinvoice = InvoiceDB.EInvoiceId != null ? true : false;
                        }
                        //feeding First sheet
                        SalesOfferSheet.Cells[recordIndex, 1].Value = item.ProjectName != null ? item.ProjectName : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 2].Value = item.SalesPersonName != null ? item.SalesPersonName : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 3].Value = item.ClientName != null ? item.ClientName : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 4].Value = item.OfferSerial != null ? item.OfferSerial : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 5].Value = item.OfferAmount;
                        SalesOfferSheet.Cells[recordIndex, 6].Value = item.FinalOfferPrice != null ? item.FinalOfferPrice : 0;
                        SalesOfferSheet.Cells[recordIndex, 7].Value = item.PercentReleasedValue;
                        SalesOfferSheet.Cells[recordIndex, 8].Value = item.ProductsCost;
                        SalesOfferSheet.Cells[recordIndex, 9].Value = item.CollectedPayment;
                        SalesOfferSheet.Cells[recordIndex, 10].Value = item.Remain;
                        SalesOfferSheet.Cells[recordIndex, 11].Value = item.ClientApprovalDate != null ? item.ClientApprovalDate : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 12].Value = item.OfferType != null ? item.OfferType : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 13].Value = item.CreationDate != null ? item.CreationDate : "N/A";
                        SalesOfferSheet.Cells[recordIndex, 14].Value = item.GrossProfitValue;
                        SalesOfferSheet.Cells[recordIndex, 15].Value = item.GrossProfitPercentage;
                        SalesOfferSheet.Cells[recordIndex, 16].Value = item.HasJournalEntryId;
                        SalesOfferSheet.Cells[recordIndex, 17].Value = item.HasJournalEntryId != 0 ? true : false;
                        SalesOfferSheet.Cells[recordIndex, 18].Value = InvoiceID;
                        SalesOfferSheet.Cells[recordIndex, 19].Value = InvoiceSerial;
                        SalesOfferSheet.Cells[recordIndex, 20].Value = HasEinvoice;
                        SalesOfferSheet.Cells[recordIndex, 21].Value = item.OfferStatus;
                        SalesOfferSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        //feeding second sheet
                        OfferDetailsSheet.Cells[recordIndex - 1, 1].Value = item.ProjectName != null ? item.ProjectName : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 2].Value = item.SalesPersonName != null ? item.SalesPersonName : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 3].Value = item.ClientName != null ? item.ClientName : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 4].Value = item.OfferSerial != null ? item.OfferSerial : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 5].Value = item.OfferAmount;
                        OfferDetailsSheet.Cells[recordIndex - 1, 6].Value = item.TaxValue != null ? item.TaxValue : 0;
                        OfferDetailsSheet.Cells[recordIndex - 1, 7].Value = item.ExtraCost != null ? item.ExtraCost : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 8].Value = item.Discount != null ? item.Discount : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 9].Value = item.DiscountOrExtraCostPerSalesPerson != null ? item.DiscountOrExtraCostPerSalesPerson : "N/A";
                        OfferDetailsSheet.Cells[recordIndex - 1, 10].Value = item.FinalOfferPrice;
                        OfferDetailsSheet.Row(recordIndex - 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        recordIndex++;
                    }

                }


                var clientsSumWorkSheet = excel.Workbook.Worksheets.Add("Clients Summary");

                var clientsIdss = _unitOfWork.AdvanciedSettingAccounts.FindAll(a => a.AdvanciedTypeId == 30).Select(a => a.AccountId).ToList();
                var clientsIdStringg = string.Join(',', clientsIdss);
                var GetAccountMouvementReportListt = clientsIdss.Count > 0 ? _movementService.GetAccountMovementList_WithListAccountIds(clientsIdStringg, filters.CalcWithoutPrivate, filters.OrderByCreationDate, filters.From, filters.To, 0, 0, filters.BranchId) : null;

                clientsSumWorkSheet.DefaultRowHeight = 12;
                clientsSumWorkSheet.Row(1).Height = 20;
                clientsSumWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                clientsSumWorkSheet.Row(1).Style.Font.Bold = true;
                clientsSumWorkSheet.Cells[1, 1, 1, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                clientsSumWorkSheet.Cells[1, 1, 1, 18].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                clientsSumWorkSheet.Cells[1, 1, 1, 18].Style.Font.Color.SetColor(Color.White);
                clientsSumWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                clientsSumWorkSheet.Cells[1, 2].Value = "From or To";
                clientsSumWorkSheet.Cells[1, 3].Value = "Account Name";
                clientsSumWorkSheet.Cells[1, 4].Value = "Related Account";
                clientsSumWorkSheet.Cells[1, 5].Value = "Account Code";
                clientsSumWorkSheet.Cells[1, 6].Value = "Account Category";
                clientsSumWorkSheet.Cells[1, 7].Value = "Cu.";
                clientsSumWorkSheet.Cells[1, 8].Value = "Credit";
                clientsSumWorkSheet.Cells[1, 9].Value = "Debit";
                clientsSumWorkSheet.Cells[1, 10].Value = "Sum";
                clientsSumWorkSheet.Cells[1, 11].Value = "AccBalance";
                clientsSumWorkSheet.Cells[1, 12].Value = "Journal Entry #";
                clientsSumWorkSheet.Cells[1, 13].Value = "Description";
                clientsSumWorkSheet.Cells[1, 14].Value = "Ref. Doc#";
                clientsSumWorkSheet.Cells[1, 15].Value = "Created By";
                clientsSumWorkSheet.Cells[1, 16].Value = "Method";
                clientsSumWorkSheet.Cells[1, 17].Value = "Client Name";
                clientsSumWorkSheet.Cells[1, 18].Value = "Project Name";

                if (GetAccountMouvementReportListt != null)
                {
                    int recordIndex = 2;
                    foreach (var item in GetAccountMouvementReportListt.GroupBy(a => a.ClientID))
                    {

                        if (item.FirstOrDefault().FromOrTo.ToLower().Contains("from"))
                        {
                            item.FirstOrDefault().FromOrTo = "To";
                        }
                        else
                        {
                            item.FirstOrDefault().FromOrTo = "From";
                        }
                        clientsSumWorkSheet.Cells[recordIndex, 1].Value = "C.D " + item.FirstOrDefault().CreationDate + "\n" + " " + "E.D " + item.FirstOrDefault().EntryDate;
                        clientsSumWorkSheet.Cells[recordIndex, 2].Value = item.FirstOrDefault().FromOrTo;
                        clientsSumWorkSheet.Cells[recordIndex, 3].Value = item.FirstOrDefault().AccountName;
                        clientsSumWorkSheet.Cells[recordIndex, 4].Value = item.FirstOrDefault().ReleatedAccount != null ? item.FirstOrDefault().ReleatedAccount : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 5].Value = item.FirstOrDefault().AccountCode != null ? item.FirstOrDefault().AccountCode : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 6].Value = item.FirstOrDefault().AccountCategory + " " + "(" + item.FirstOrDefault().AccountType + ")";
                        clientsSumWorkSheet.Cells[recordIndex, 7].Value = item.FirstOrDefault().Currency != null ? item.FirstOrDefault().Currency : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 8].Value = item.Sum(a => a.Credit ?? 0);
                        clientsSumWorkSheet.Cells[recordIndex, 9].Value = item.Sum(a => a.Debit ?? 0); ;
                        clientsSumWorkSheet.Cells[recordIndex, 10].Value = (item.Sum(a => a.Debit ?? 0) > item.Sum(a => a.Credit ?? 0)) ? item.Sum(a => a.Debit ?? 0) - item.Sum(a => a.Credit ?? 0) + "D" : item.Sum(a => a.Credit ?? 0) - item.Sum(a => a.Debit ?? 0) + "C";
                        clientsSumWorkSheet.Cells[recordIndex, 11].Value = item.Sum(a => a.Accumulative ?? 0);
                        clientsSumWorkSheet.Cells[recordIndex, 12].Value = item.FirstOrDefault().EntryDate != null ? item.FirstOrDefault().EntryDate : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 13].Value = item.FirstOrDefault().Description != null ? item.FirstOrDefault().Description : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 14].Value = item.FirstOrDefault().Document != null ? item.FirstOrDefault().Document : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 15].Value = item.FirstOrDefault().CreatedBy != null ? item.FirstOrDefault().CreatedBy : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 16].Value = item.FirstOrDefault().MethodName != null ? item.FirstOrDefault().MethodName : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 17].Value = item.FirstOrDefault().ClientName != null ? item.FirstOrDefault().ClientName : "-";
                        clientsSumWorkSheet.Cells[recordIndex, 18].Value = item.FirstOrDefault().ProjectName != null ? item.FirstOrDefault().ProjectName : "-";
                        clientsSumWorkSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        recordIndex++;
                    }

                }



                var clientsWorkSheet = excel.Workbook.Worksheets.Add("Clients");

                var clientsIds = _unitOfWork.AdvanciedSettingAccounts.FindAll(a => a.AdvanciedTypeId == 30).Select(a => a.AccountId).ToList();
                var clientsIdString = string.Join(',', clientsIds);
                var GetAccountMouvementReportList = clientsIds.Count > 0 ? _movementService.GetAccountMovementList_WithListAccountIds(clientsIdString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, filters.From, filters.To, 0, 0, filters.BranchId) : null;

                clientsWorkSheet.DefaultRowHeight = 12;
                clientsWorkSheet.Row(1).Height = 20;
                clientsWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                clientsWorkSheet.Row(1).Style.Font.Bold = true;
                clientsWorkSheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                clientsWorkSheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                clientsWorkSheet.Cells[1, 1, 1, 17].Style.Font.Color.SetColor(Color.White);
                clientsWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                clientsWorkSheet.Cells[1, 2].Value = "From or To";
                clientsWorkSheet.Cells[1, 3].Value = "Account Name";
                clientsWorkSheet.Cells[1, 4].Value = "Related Account";
                clientsWorkSheet.Cells[1, 5].Value = "Account Code";
                clientsWorkSheet.Cells[1, 6].Value = "Account Category";
                clientsWorkSheet.Cells[1, 7].Value = "Cu.";
                clientsWorkSheet.Cells[1, 8].Value = "Credit";
                clientsWorkSheet.Cells[1, 9].Value = "Debit";
                clientsWorkSheet.Cells[1, 10].Value = "AccBalance";
                clientsWorkSheet.Cells[1, 11].Value = "Journal Entry #";
                clientsWorkSheet.Cells[1, 12].Value = "Description";
                clientsWorkSheet.Cells[1, 13].Value = "Ref. Doc#";
                clientsWorkSheet.Cells[1, 14].Value = "Created By";
                clientsWorkSheet.Cells[1, 15].Value = "Method";
                clientsWorkSheet.Cells[1, 16].Value = "Client Name";
                clientsWorkSheet.Cells[1, 17].Value = "Project Name";

                if (GetAccountMouvementReportList != null)
                {
                    int recordIndex = 2;
                    foreach (var item in GetAccountMouvementReportList)
                    {
                        if (item.FromOrTo.ToLower().Contains("from"))
                        {
                            item.FromOrTo = "To";
                        }
                        else
                        {
                            item.FromOrTo = "From";
                        }
                        clientsWorkSheet.Cells[recordIndex, 1].Value = "C.D " + item.CreationDate + "\n" + " " + "E.D " + item.EntryDate;
                        clientsWorkSheet.Cells[recordIndex, 2].Value = item.FromOrTo;
                        clientsWorkSheet.Cells[recordIndex, 3].Value = item.AccountName;
                        clientsWorkSheet.Cells[recordIndex, 4].Value = item.ReleatedAccount != null ? item.ReleatedAccount : "-";
                        clientsWorkSheet.Cells[recordIndex, 5].Value = item.AccountCode != null ? item.AccountCode : "-";
                        clientsWorkSheet.Cells[recordIndex, 6].Value = item.AccountCategory + " " + "(" + item.AccountType + ")";
                        clientsWorkSheet.Cells[recordIndex, 7].Value = item.Currency != null ? item.Currency : "-";
                        clientsWorkSheet.Cells[recordIndex, 8].Value = item.Credit != null ? item.Credit : 0;
                        clientsWorkSheet.Cells[recordIndex, 9].Value = item.Debit != null ? item.Debit : 0;
                        clientsWorkSheet.Cells[recordIndex, 10].Value = item.Accumulative != null ? item.Accumulative : 0;
                        clientsWorkSheet.Cells[recordIndex, 11].Value = item.EntryDate != null ? item.EntryDate : "-";
                        clientsWorkSheet.Cells[recordIndex, 12].Value = item.Description != null ? item.Description : "-";
                        clientsWorkSheet.Cells[recordIndex, 13].Value = item.Document != null ? item.Document : "-";
                        clientsWorkSheet.Cells[recordIndex, 14].Value = item.CreatedBy != null ? item.CreatedBy : "-";
                        clientsWorkSheet.Cells[recordIndex, 15].Value = item.MethodName != null ? item.MethodName : "-";
                        clientsWorkSheet.Cells[recordIndex, 16].Value = item.ClientName != null ? item.ClientName : "-";
                        clientsWorkSheet.Cells[recordIndex, 17].Value = item.ProjectName != null ? item.ProjectName : "-";
                        clientsWorkSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        recordIndex++;
                    }
                }


                var SuppliersSumWorkSheet = excel.Workbook.Worksheets.Add("Suppliers Summary");

                var suppliersSumIds = _unitOfWork.AdvanciedSettingAccounts.FindAll(a => a.AdvanciedTypeId == 31).Select(a => a.AccountId).ToList();
                var suppliersSumString = string.Join(',', suppliersSumIds);
                var GetSupplierSumMovementReportList = suppliersSumIds.Count > 0 ? _movementService.GetAccountMovementList_WithListAccountIds(suppliersSumString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, filters.From, filters.To, 0, 0, filters.BranchId) : null;

                SuppliersSumWorkSheet.DefaultRowHeight = 12;
                SuppliersSumWorkSheet.Row(1).Height = 20;
                SuppliersSumWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                SuppliersSumWorkSheet.Row(1).Style.Font.Bold = true;
                SuppliersSumWorkSheet.Cells[1, 1, 1, 18].Style.Fill.PatternType = ExcelFillStyle.Solid;
                SuppliersSumWorkSheet.Cells[1, 1, 1, 18].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                SuppliersSumWorkSheet.Cells[1, 1, 1, 18].Style.Font.Color.SetColor(Color.White);
                SuppliersSumWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                SuppliersSumWorkSheet.Cells[1, 2].Value = "From or To";
                SuppliersSumWorkSheet.Cells[1, 3].Value = "Account Name";
                SuppliersSumWorkSheet.Cells[1, 4].Value = "Related Account";
                SuppliersSumWorkSheet.Cells[1, 5].Value = "Account Code";
                SuppliersSumWorkSheet.Cells[1, 6].Value = "Account Category";
                SuppliersSumWorkSheet.Cells[1, 7].Value = "Cu.";
                SuppliersSumWorkSheet.Cells[1, 8].Value = "Credit";
                SuppliersSumWorkSheet.Cells[1, 9].Value = "Debit";
                SuppliersSumWorkSheet.Cells[1, 10].Value = "Sum";
                SuppliersSumWorkSheet.Cells[1, 11].Value = "AccBalance";
                SuppliersSumWorkSheet.Cells[1, 12].Value = "Journal Entry #";
                SuppliersSumWorkSheet.Cells[1, 13].Value = "Description";
                SuppliersSumWorkSheet.Cells[1, 14].Value = "Ref. Doc#";
                SuppliersSumWorkSheet.Cells[1, 15].Value = "Created By";
                SuppliersSumWorkSheet.Cells[1, 16].Value = "Method";
                SuppliersSumWorkSheet.Cells[1, 17].Value = "Supplier Name";
                SuppliersSumWorkSheet.Cells[1, 18].Value = "PO";

                if (GetSupplierSumMovementReportList != null)
                {
                    int recordIndex = 2;
                    foreach (var item in GetSupplierSumMovementReportList.GroupBy(a => a.SupplierID))
                    {
                        if (item.FirstOrDefault().FromOrTo.ToLower().Contains("from"))
                        {
                            item.FirstOrDefault().FromOrTo = "To";
                        }
                        else
                        {
                            item.FirstOrDefault().FromOrTo = "From";
                        }
                        SuppliersSumWorkSheet.Cells[recordIndex, 1].Value = "C.D " + item.FirstOrDefault().CreationDate + "\n" + " " + "E.D " + item.FirstOrDefault().EntryDate;
                        SuppliersSumWorkSheet.Cells[recordIndex, 2].Value = item.FirstOrDefault().FromOrTo;
                        SuppliersSumWorkSheet.Cells[recordIndex, 3].Value = item.FirstOrDefault().AccountName;
                        SuppliersSumWorkSheet.Cells[recordIndex, 4].Value = item.FirstOrDefault().ReleatedAccount != null ? item.FirstOrDefault().ReleatedAccount : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 5].Value = item.FirstOrDefault().AccountCode != null ? item.FirstOrDefault().AccountCode : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 6].Value = item.FirstOrDefault().AccountCategory + " " + "(" + item.FirstOrDefault().AccountType + ")";
                        SuppliersSumWorkSheet.Cells[recordIndex, 7].Value = item.FirstOrDefault().Currency != null ? item.FirstOrDefault().Currency : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 8].Value = item.Sum(a => a.Credit ?? 0);
                        SuppliersSumWorkSheet.Cells[recordIndex, 9].Value = item.Sum(a => a.Debit ?? 0);
                        SuppliersSumWorkSheet.Cells[recordIndex, 10].Value = (item.Sum(a => a.Debit ?? 0) > item.Sum(a => a.Credit ?? 0)) ? item.Sum(a => a.Debit ?? 0) - item.Sum(a => a.Credit ?? 0) + "D" : item.Sum(a => a.Credit ?? 0) - item.Sum(a => a.Debit ?? 0) + "C";
                        SuppliersSumWorkSheet.Cells[recordIndex, 11].Value = item.Sum(a => a.Accumulative ?? 0);
                        SuppliersSumWorkSheet.Cells[recordIndex, 12].Value = item.FirstOrDefault().EntryDate != null ? item.FirstOrDefault().EntryDate : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 13].Value = item.FirstOrDefault().Description != null ? item.FirstOrDefault().Description : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 14].Value = item.FirstOrDefault().Document != null ? item.FirstOrDefault().Document : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 15].Value = item.FirstOrDefault().CreatedBy != null ? item.FirstOrDefault().CreatedBy : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 16].Value = item.FirstOrDefault().MethodName != null ? item.FirstOrDefault().MethodName : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 17].Value = item.FirstOrDefault().SupplierName != null ? item.FirstOrDefault().SupplierName : "-";
                        SuppliersSumWorkSheet.Cells[recordIndex, 18].Value = item.FirstOrDefault().POID != null ? item.FirstOrDefault().POID : "-";
                        SuppliersSumWorkSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        recordIndex++;
                    }
                }



                var SuppliersWorkSheet = excel.Workbook.Worksheets.Add("Suppliers");

                var suppliersIds = _unitOfWork.AdvanciedSettingAccounts.FindAll(a => a.AdvanciedTypeId == 31).Select(a => a.AccountId).ToList();
                var suppliersString = string.Join(',', suppliersIds);
                var GetSupplierMovementReportList = suppliersIds.Count > 0 ? _movementService.GetAccountMovementList_WithListAccountIds(suppliersString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, filters.From, filters.To, 0, 0, filters.BranchId) : null;

                SuppliersWorkSheet.DefaultRowHeight = 12;
                SuppliersWorkSheet.Row(1).Height = 20;
                SuppliersWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                SuppliersWorkSheet.Row(1).Style.Font.Bold = true;
                SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Font.Color.SetColor(Color.White);
                SuppliersWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                SuppliersWorkSheet.Cells[1, 2].Value = "From or To";
                SuppliersWorkSheet.Cells[1, 3].Value = "Account Name";
                SuppliersWorkSheet.Cells[1, 4].Value = "Related Account";
                SuppliersWorkSheet.Cells[1, 5].Value = "Account Code";
                SuppliersWorkSheet.Cells[1, 6].Value = "Account Category";
                SuppliersWorkSheet.Cells[1, 7].Value = "Cu.";
                SuppliersWorkSheet.Cells[1, 8].Value = "Credit";
                SuppliersWorkSheet.Cells[1, 9].Value = "Debit";
                SuppliersWorkSheet.Cells[1, 10].Value = "AccBalance";
                SuppliersWorkSheet.Cells[1, 11].Value = "Journal Entry #";
                SuppliersWorkSheet.Cells[1, 12].Value = "Description";
                SuppliersWorkSheet.Cells[1, 13].Value = "Ref. Doc#";
                SuppliersWorkSheet.Cells[1, 14].Value = "Created By";
                SuppliersWorkSheet.Cells[1, 15].Value = "Method";
                SuppliersWorkSheet.Cells[1, 16].Value = "Supplier Name";
                SuppliersWorkSheet.Cells[1, 17].Value = "PO";

                if (GetSupplierMovementReportList != null)
                {
                    int recordIndex = 2;
                    foreach (var item in GetSupplierMovementReportList)
                    {
                        if (item.FromOrTo.ToLower().Contains("from"))
                        {
                            item.FromOrTo = "To";
                        }
                        else
                        {
                            item.FromOrTo = "From";
                        }
                        SuppliersWorkSheet.Cells[recordIndex, 1].Value = "C.D " + item.CreationDate + "\n" + " " + "E.D " + item.EntryDate;
                        SuppliersWorkSheet.Cells[recordIndex, 2].Value = item.FromOrTo;
                        SuppliersWorkSheet.Cells[recordIndex, 3].Value = item.AccountName;
                        SuppliersWorkSheet.Cells[recordIndex, 4].Value = item.ReleatedAccount != null ? item.ReleatedAccount : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 5].Value = item.AccountCode != null ? item.AccountCode : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 6].Value = item.AccountCategory + " " + "(" + item.AccountType + ")";
                        SuppliersWorkSheet.Cells[recordIndex, 7].Value = item.Currency != null ? item.Currency : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 8].Value = item.Credit != null ? item.Credit : 0;
                        SuppliersWorkSheet.Cells[recordIndex, 9].Value = item.Debit != null ? item.Debit : 0;
                        SuppliersWorkSheet.Cells[recordIndex, 10].Value = item.Accumulative != null ? item.Accumulative : 0;
                        SuppliersWorkSheet.Cells[recordIndex, 11].Value = item.EntryDate != null ? item.EntryDate : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 12].Value = item.Description != null ? item.Description : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 13].Value = item.Document != null ? item.Document : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 14].Value = item.CreatedBy != null ? item.CreatedBy : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 15].Value = item.MethodName != null ? item.MethodName : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 16].Value = item.SupplierName != null ? item.SupplierName : "-";
                        SuppliersWorkSheet.Cells[recordIndex, 17].Value = item.POID != null ? item.POID : "-";
                        SuppliersWorkSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        recordIndex++;
                    }
                }

                SalesOfferSheet.Column(1).AutoFit();
                SalesOfferSheet.Column(2).AutoFit();
                SalesOfferSheet.Column(3).AutoFit();
                SalesOfferSheet.Column(4).AutoFit();
                SalesOfferSheet.Column(5).AutoFit();
                SalesOfferSheet.Column(5).Style.Numberformat.Format = "#,##0.00";
                SalesOfferSheet.Column(6).AutoFit();
                SalesOfferSheet.Column(6).Style.Numberformat.Format = "#,##0.00";
                SalesOfferSheet.Column(9).AutoFit();
                SalesOfferSheet.Column(9).Style.Numberformat.Format = "#,##0.00";
                SalesOfferSheet.Column(8).AutoFit();
                SalesOfferSheet.Column(8).Style.Numberformat.Format = "#0\\.00%";
                SalesOfferSheet.Column(7).AutoFit();
                SalesOfferSheet.Column(7).Style.Numberformat.Format = "#0\\.00%";
                SalesOfferSheet.Column(10).AutoFit();
                SalesOfferSheet.Column(10).Style.Numberformat.Format = "#,##0.00";
                SalesOfferSheet.Column(11).AutoFit();
                SalesOfferSheet.Column(11).Style.Numberformat.Format = "#,##0.00";
                SalesOfferSheet.Column(12).AutoFit();
                SalesOfferSheet.Column(12).Style.Numberformat.Format = "yyyy/mm/dd";
                SalesOfferSheet.Column(13).AutoFit();
                SalesOfferSheet.Column(14).AutoFit();
                SalesOfferSheet.Column(14).Style.Numberformat.Format = "yyyy/mm/dd";
                SalesOfferSheet.Column(15).AutoFit();
                SalesOfferSheet.Column(15).Style.Numberformat.Format = "#,##0.00";
                SalesOfferSheet.Column(16).AutoFit();
                SalesOfferSheet.Column(16).Style.Numberformat.Format = "#0\\.00%";
                SalesOfferSheet.Column(17).AutoFit();
                SalesOfferSheet.Column(18).AutoFit();
                SalesOfferSheet.Column(19).AutoFit();
                SalesOfferSheet.Column(20).AutoFit();
                SalesOfferSheet.Column(21).AutoFit();
                SalesOfferSheet.Column(22).AutoFit();

                OfferDetailsSheet.Column(1).AutoFit();
                OfferDetailsSheet.Column(2).AutoFit();
                OfferDetailsSheet.Column(3).AutoFit();
                OfferDetailsSheet.Column(4).AutoFit();
                OfferDetailsSheet.Column(5).AutoFit();
                OfferDetailsSheet.Column(5).Style.Numberformat.Format = "#,##0.00";
                OfferDetailsSheet.Column(6).AutoFit();
                OfferDetailsSheet.Column(6).Style.Numberformat.Format = "#,##0.00";
                OfferDetailsSheet.Column(7).AutoFit();
                OfferDetailsSheet.Column(7).Style.Numberformat.Format = "#,##0.00";
                OfferDetailsSheet.Column(8).AutoFit();
                OfferDetailsSheet.Column(8).Style.Numberformat.Format = "#,##0.00";
                OfferDetailsSheet.Column(9).AutoFit();
                OfferDetailsSheet.Column(9).Style.Numberformat.Format = "#,##0.00";
                OfferDetailsSheet.Column(10).AutoFit();
                OfferDetailsSheet.Column(10).Style.Numberformat.Format = "#,##0.00";

                clientsSumWorkSheet.Column(1).AutoFit();
                clientsSumWorkSheet.Column(2).AutoFit();
                clientsSumWorkSheet.Column(3).AutoFit();
                clientsSumWorkSheet.Column(4).AutoFit();
                clientsSumWorkSheet.Column(5).AutoFit();
                clientsSumWorkSheet.Column(6).AutoFit();
                clientsSumWorkSheet.Column(7).AutoFit();
                clientsSumWorkSheet.Column(8).AutoFit();
                clientsSumWorkSheet.Column(9).AutoFit();
                clientsSumWorkSheet.Column(10).AutoFit();
                clientsSumWorkSheet.Column(11).AutoFit();
                clientsSumWorkSheet.Column(12).AutoFit();
                clientsSumWorkSheet.Column(13).AutoFit();
                clientsSumWorkSheet.Column(14).AutoFit();
                clientsSumWorkSheet.Column(15).AutoFit();
                clientsSumWorkSheet.Column(16).AutoFit();
                clientsSumWorkSheet.Column(17).AutoFit();
                clientsSumWorkSheet.Column(18).AutoFit();

                clientsWorkSheet.Column(1).AutoFit();
                clientsWorkSheet.Column(2).AutoFit();
                clientsWorkSheet.Column(3).AutoFit();
                clientsWorkSheet.Column(4).AutoFit();
                clientsWorkSheet.Column(5).AutoFit();
                clientsWorkSheet.Column(6).AutoFit();
                clientsWorkSheet.Column(7).AutoFit();
                clientsWorkSheet.Column(8).AutoFit();
                clientsWorkSheet.Column(9).AutoFit();
                clientsWorkSheet.Column(10).AutoFit();
                clientsWorkSheet.Column(11).AutoFit();
                clientsWorkSheet.Column(12).AutoFit();
                clientsWorkSheet.Column(13).AutoFit();
                clientsWorkSheet.Column(14).AutoFit();
                clientsWorkSheet.Column(15).AutoFit();
                clientsWorkSheet.Column(16).AutoFit();
                clientsWorkSheet.Column(17).AutoFit();


                SuppliersSumWorkSheet.Column(1).AutoFit();
                SuppliersSumWorkSheet.Column(2).AutoFit();
                SuppliersSumWorkSheet.Column(3).AutoFit();
                SuppliersSumWorkSheet.Column(4).AutoFit();
                SuppliersSumWorkSheet.Column(5).AutoFit();
                SuppliersSumWorkSheet.Column(6).AutoFit();
                SuppliersSumWorkSheet.Column(7).AutoFit();
                SuppliersSumWorkSheet.Column(8).AutoFit();
                SuppliersSumWorkSheet.Column(9).AutoFit();
                SuppliersSumWorkSheet.Column(10).AutoFit();
                SuppliersSumWorkSheet.Column(11).AutoFit();
                SuppliersSumWorkSheet.Column(12).AutoFit();
                SuppliersSumWorkSheet.Column(13).AutoFit();
                SuppliersSumWorkSheet.Column(14).AutoFit();
                SuppliersSumWorkSheet.Column(15).AutoFit();
                SuppliersSumWorkSheet.Column(16).AutoFit();
                SuppliersSumWorkSheet.Column(17).AutoFit();
                SuppliersSumWorkSheet.Column(18).AutoFit();


                SuppliersWorkSheet.Column(1).AutoFit();
                SuppliersWorkSheet.Column(2).AutoFit();
                SuppliersWorkSheet.Column(3).AutoFit();
                SuppliersWorkSheet.Column(4).AutoFit();
                SuppliersWorkSheet.Column(5).AutoFit();
                SuppliersWorkSheet.Column(6).AutoFit();
                SuppliersWorkSheet.Column(7).AutoFit();
                SuppliersWorkSheet.Column(8).AutoFit();
                SuppliersWorkSheet.Column(9).AutoFit();
                SuppliersWorkSheet.Column(10).AutoFit();
                SuppliersWorkSheet.Column(11).AutoFit();
                SuppliersWorkSheet.Column(12).AutoFit();
                SuppliersWorkSheet.Column(13).AutoFit();
                SuppliersWorkSheet.Column(14).AutoFit();
                SuppliersWorkSheet.Column(15).AutoFit();
                SuppliersWorkSheet.Column(16).AutoFit();
                SuppliersWorkSheet.Column(17).AutoFit();


                var path = $"Attachments\\{CompanyName}\\SalesOfferReports";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\SalesOfferReport_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + $"\\SalesOfferReport_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }

        public BaseResponseWithData<List<GetClientJobOrderRate>> ClientsJobOrderRate(SalesOfferReportFilter filters)
        {
            BaseResponseWithData<List<GetClientJobOrderRate>> Response = new BaseResponseWithData<List<GetClientJobOrderRate>>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var Start = new DateTime(DateTime.Now.Year, 1, 1);
                var End = new DateTime(DateTime.Now.Year, 12, 31);

                var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true && a.OfferType == "New Job Order", includes: new[] { "SalesOfferProducts", "Projects.InventoryMatrialRequestItems", "SalesPerson", "VehicleMaintenanceJobOrderHistories.VehiclePerClient.Model", "VehicleMaintenanceJobOrderHistories.VehiclePerClient.Brand", "Branch", "Client" }).AsQueryable();
                if (filters.SalesPersonId != 0)
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                }
                if (filters.BranchId != 0)
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.ProjectName))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(filters.ProjectName)).AsQueryable();
                }
                if (!string.IsNullOrWhiteSpace(filters.OfferStatus))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status.ToLower().Trim() == filters.OfferStatus);
                }


                var OffersListDB = SalesOfferDBQuery.ToList();




                var Clients = OffersListDB.GroupBy(a => a.ClientId).ToList();

                var list = Clients.Select(a => new GetClientJobOrderRate()
                {
                    ClientName = a.FirstOrDefault()?.Client.Name,
                    YtdVolume = a.Where(a => a.ClientApprovalDate >= Start && a.ClientApprovalDate <= End).Sum(a => a.FinalOfferPrice ?? 0),
                    YtdCount = a.Where(a => a.ClientApprovalDate >= Start && a.ClientApprovalDate <= End).Count(),
                    AllVolume = a.Sum(a => a.FinalOfferPrice ?? 0),
                    AllCount = a.Count(),
                    FirstOrderDate = a.Where(a => a.ClientApprovalDate != null).OrderBy(a => a.ClientApprovalDate).Select(a => ((DateTime)a.ClientApprovalDate).ToShortDateString()).FirstOrDefault(),
                    LastOrderDate = a.Where(a => a.ClientApprovalDate != null).OrderBy(a => a.ClientApprovalDate).Select(a => ((DateTime)a.ClientApprovalDate).ToShortDateString()).LastOrDefault()

                }).OrderByDescending(a => a.MonthlyClientRate).ToList();
                Response.Data = list;

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;

            }
        }

        public BaseResponseWithData<string> SalesOfferReportForVehicle(SalesOfferReportFilter filters, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                var companySpeciality = _unitOfWork.CompanySpecialties.FindAll(a => a.SpecialityId == 10).FirstOrDefault();
                if (companySpeciality == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err-101";
                    error.ErrorMSG = "Company Speciality is invalid";
                    Response.Errors.Add(error);
                    return Response;
                }

                var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true && a.OfferType == "New Job Order", includes: new[] { "SalesOfferProducts", "Projects.InventoryMatrialRequestItems", "SalesPerson", "VehicleMaintenanceJobOrderHistories.VehiclePerClient.Model", "VehicleMaintenanceJobOrderHistories.VehiclePerClient.Brand", "Branch" }).AsQueryable();
                if (filters.SalesPersonId != 0)
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                }
                if (filters.BranchId != 0)
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                }

                if (!string.IsNullOrEmpty(filters.ClientName))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filters.ClientName)).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.ProjectName))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(filters.ProjectName)).AsQueryable();
                }
                if (!string.IsNullOrWhiteSpace(filters.OfferStatus))
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status.ToLower().Trim() == filters.OfferStatus);
                }

                if (filters.From.Date != DateTime.Now.Date || filters.To.Date != DateTime.Now.Date)
                {
                    if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= filters.From && a.ClientApprovalDate <= filters.To).AsQueryable();
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime(filters.From) && a.EndDate <= DateOnly.FromDateTime(filters.To)).AsQueryable();
                    }
                }
                else
                {
                    if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                    {
                        var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                    }
                }
                if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate);
                }
                else
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
                }

                var OffersListDB = SalesOfferDBQuery.ToList();
                var OffersListResponse = new List<SalesOfferForVehicleReport>();
                var ids = OffersListDB.Select(a => a.Id).ToList();
                var history = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(a => ids.Contains(a.SalesOfferId ?? 0), includes: new[] { "VehiclePerClient", "VehiclePerClient.Model", "VehiclePerClient.Brand" });
                foreach (var offer in OffersListDB)
                {
                    var amounts = GetCalcSupplierCollectedDetails((long)offer.ClientId);
                    SalesOfferForVehicleReport salesOfferObj = new SalesOfferForVehicleReport()
                    {
                        Id = offer.Id,
                        ProjectName = offer.ProjectName,
                        SalesPersonName = offer.SalesPerson.FirstName + ' ' + offer.SalesPerson.MiddleName + ' ' + offer.SalesPerson.LastName,
                        ClientName = offer.Client?.Name ?? "",
                        OfferSerial = offer.OfferSerial,
                        OfferStatus = offer.Status,
                        FinalOfferPrice = offer.FinalOfferPrice,
                        ClientApprovalDate = offer.ClientApprovalDate != null ? offer.ClientApprovalDate.ToString().Split(' ')[0] : null,
                        OfferType = offer.OfferType,
                        CreationDate = offer.CreationDate.ToShortDateString(),
                        DiscountOrExtraCostPerSalesPerson = offer.ExtraOrDiscountPriceBySalesPerson,
                        OfferAmount = offer.OfferAmount,
                        TaxValue = 0,
                        Discount = 0,
                        ExtraCost = (offer.SalesOfferExtraCosts?.Where(x => x.Active == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum()),
                        Totalcost = (decimal)amounts.TotalAmount,
                        CollectedPayment = (decimal)amounts.TotalCollected,
                        Remain = (decimal)(offer.FinalOfferPrice ?? 0 - amounts.TotalCollected),
                        Model = history.Where(a => a.SalesOfferId == offer.Id).FirstOrDefault()?.VehiclePerClient?.Model?.Name ?? "-",
                        Brand = history.Where(a => a.SalesOfferId == offer.Id).FirstOrDefault()?.VehiclePerClient?.Brand?.Name ?? "-",
                        Plate = history.Where(a => a.SalesOfferId == offer.Id).FirstOrDefault()?.VehiclePerClient?.PlatNumber ?? "-",
                        Chasse = history.Where(a => a.SalesOfferId == offer.Id).FirstOrDefault()?.VehiclePerClient?.ChassisNumber ?? "-",
                        BranchName = offer.Branch?.Name
                    };
                    var OfferClientAccount = _unitOfWork.ClientAccounts.FindAll(a => a.OfferId == offer.Id).FirstOrDefault();
                    if (OfferClientAccount != null)
                    {
                        salesOfferObj.HasJournalEntryId = OfferClientAccount.DailyAdjustingEntryId;
                    }
                    OffersListResponse.Add(salesOfferObj);
                }
                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[22] { new DataColumn("Project Name"),
                                                    new DataColumn("Sales Person"),
                                                    new DataColumn("Client Name"),
                                                    new DataColumn("Branch"),
                                                    new DataColumn("Model"),
                                                    new DataColumn("Brand"),
                                                    new DataColumn("Chassis"),
                                                    new DataColumn("Plate"),
                                                    new DataColumn("Serial"),
                                                    new DataColumn("Offer Amount"),
                                                    new DataColumn("Final Offer Price"),
                                                    new DataColumn("Collected Payment"),
                                                    new DataColumn("Remaining Amount"),
                                                    new DataColumn("Client approval date"),
                                                    new DataColumn("Project Type") ,
                                                    new DataColumn("Creation Date"),
                                                    new DataColumn("Journal Entry ID"),
                                                    new DataColumn("Has Journal Entry"),
                                                    new DataColumn("Invoice ID"),
                                                    new DataColumn("Invoice Serial"),
                                                    new DataColumn("Has E Invoice"),
                                                    new DataColumn("Status"),
                                  });
                var SalesOfferList = OffersListResponse;
                var IDsalesofferList = SalesOfferList.Select(y => y.Id).ToList();
                var InvoiceListDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId != null ? IDsalesofferList.Contains((long)x.SalesOfferId) : false).ToList();
                foreach (var item in SalesOfferList)
                {
                    var InvoiceDB = InvoiceListDB.Where(x => x.SalesOfferId == item.Id).FirstOrDefault();
                    long InvoiceID = 0;
                    string InvoiceSerial = "";
                    bool HasEinvoice = false;
                    if (InvoiceDB != null)
                    {
                        InvoiceID = InvoiceDB.Id;
                        InvoiceSerial = InvoiceDB.Serial;
                        HasEinvoice = InvoiceDB.EInvoiceId != null ? true : false;
                    }
                    dt.Rows.Add(
                        item.ProjectName,
                            item.SalesPersonName,
                            item.ClientName,
                            item.BranchName,
                            item.Model,
                            item.Brand,
                            item.Chasse,
                            item.Plate,
                            item.OfferSerial,
                            item.OfferAmount,
                            item.FinalOfferPrice ?? 0,
                            item.CollectedPayment,
                            item.Remain,
                            item.ClientApprovalDate,
                            item.OfferType,
                            item.CreationDate,
                            item.HasJournalEntryId != 0 ? true : false,
                            item.HasJournalEntryId,
                            InvoiceID,
                            InvoiceSerial,
                            HasEinvoice,
                    item.OfferStatus
                        );
                }
                var workSheet = package.Workbook.Worksheets.Add("SalesOffer");
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 22].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 22].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                workSheet.Cells[1, 1, 1, 22].Style.Font.Color.SetColor(Color.White);
                ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                for (int i = 1; i <= excelRangeBase.Columns; i++)
                {
                    workSheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= excelRangeBase.Rows; i++)
                {
                    workSheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                workSheet.View.FreezePanes(2, 1);


                var data = ClientsJobOrderRate(filters).Data;

                var dtt = new System.Data.DataTable("Grid");
                dtt.Columns.AddRange(new DataColumn[9] { new DataColumn("Client Name"),
                                                    new DataColumn("YTD Volume"),
                                                    new DataColumn("YTD Count"),
                                                    new DataColumn("Monthly Client Rate"),
                                                    new DataColumn("All Volume"),
                                                    new DataColumn("All Count"),
                                                    new DataColumn("Order Rate"),
                                                    new DataColumn("1st Order Date"),
                                                    new DataColumn("Last Order Date"),
                                  });

                foreach (var item in data)
                {
                    dtt.Rows.Add(
                        item.ClientName,
                            item.YtdVolume,
                            item.YtdCount,
                            item.MonthlyClientRate,
                            item.AllVolume,
                            item.AllCount,
                            item.OrderRate,
                            item.FirstOrderDate,
                            item.LastOrderDate
                        );
                }

                var workSheet2 = package.Workbook.Worksheets.Add("Client Order Rate");
                workSheet2.DefaultRowHeight = 12;
                workSheet2.Row(1).Height = 20;
                workSheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet2.Row(1).Style.Font.Bold = true;
                workSheet2.Cells[1, 1, 1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet2.Cells[1, 1, 1, 9].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                workSheet2.Cells[1, 1, 1, 9].Style.Font.Color.SetColor(Color.White);
                ExcelRangeBase excelRangeBasee = workSheet2.Cells.LoadFromDataTable(dtt, true);
                for (int i = 1; i <= excelRangeBasee.Columns; i++)
                {
                    workSheet2.Column(i).AutoFit();
                }
                for (int i = 1; i <= excelRangeBasee.Rows; i++)
                {
                    workSheet2.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet2.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                workSheet2.View.FreezePanes(2, 1);

                var newpath = $"Attachments\\{CompanyName}\\SalesOfferReports";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\SalesOfferReport_{date}.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\SalesOfferReport_{date}.xlsx";

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }
        public BaseResponseWithData<string> GetClientOrdeRate(SalesOfferReportFilter filters, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var package = new ExcelPackage();
                var dt = new System.Data.DataTable("Grid");
                dt.Columns.AddRange(new DataColumn[9] { new DataColumn("Client Name"),
                                                    new DataColumn("YTD Volume"),
                                                    new DataColumn("YTD Count"),
                                                    new DataColumn("Monthly Client Rate"),
                                                    new DataColumn("All Volume"),
                                                    new DataColumn("All Count"),
                                                    new DataColumn("Order Rate"),
                                                    new DataColumn("1st Order Date"),
                                                    new DataColumn("Last Order Date"),
                                  });
                var data = ClientsJobOrderRate(filters).Data;
                foreach (var item in data)
                {
                    dt.Rows.Add(
                        item.ClientName,
                            item.YtdVolume,
                            item.YtdCount,
                            item.MonthlyClientRate,
                            item.AllVolume,
                            item.AllCount,
                            item.OrderRate,
                            item.FirstOrderDate,
                            item.LastOrderDate
                        );
                }
                var workSheet = package.Workbook.Worksheets.Add("Client Order Rate");
                workSheet.DefaultRowHeight = 12;
                workSheet.Row(1).Height = 20;
                workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Row(1).Style.Font.Bold = true;
                workSheet.Cells[1, 1, 1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[1, 1, 1, 9].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                workSheet.Cells[1, 1, 1, 9].Style.Font.Color.SetColor(Color.White);
                ExcelRangeBase excelRangeBasee = workSheet.Cells.LoadFromDataTable(dt, true);
                for (int i = 1; i <= excelRangeBasee.Columns; i++)
                {
                    workSheet.Column(i).AutoFit();
                }
                for (int i = 1; i <= excelRangeBasee.Rows; i++)
                {
                    workSheet.Row(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }
                workSheet.View.FreezePanes(2, 1);
                var newpath = $"Attachments\\{CompanyName}\\ClientOrderRate";
                var savedPath = System.IO.Path.Combine(_host.WebRootPath, newpath);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\ClientOrderRateReport_{date}.xlsx";
                package.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                package.Dispose();
                Response.Data = Globals.baseURL + '\\' + newpath + $"\\ClientOrderRateReport_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }
        public BaseResponseWithData<string> CalculateOfferFinalPrice(long SalesOfferId)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (SalesOfferId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Sales Offer Id Is Required";
                    Response.Errors.Add(error);
                    return Response;
                }

                var SalesOffer = _unitOfWork.SalesOffers.GetById(SalesOfferId);

                if (SalesOffer == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "This Sales Offer Is Not Found";
                    Response.Errors.Add(error);
                    return Response;
                }

                var discount = _unitOfWork.SalesOfferDiscounts.FindAll(a => a.SalesOfferId == SalesOfferId).ToList();

                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }
        public BaseResponseWithData<bool> ValidateProductsPrices(List<OfferProductValidation> OfferProducts)
        {
            BaseResponseWithData<bool> Response = new BaseResponseWithData<bool>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = true;
            try
            {
                if (OfferProducts == null)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "List is null";
                    Response.Errors.Add(error);
                    return Response;
                }
                if (OfferProducts.Count == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err102";
                    error.ErrorMSG = "List is Empty";
                    Response.Errors.Add(error);
                    return Response;
                }

                for (int i = 0; i < OfferProducts.Count; i++)
                {
                    var afterdiscount = OfferProducts[i].ItemPrice;
                    if (OfferProducts[i].DiscountPercentage != null)
                    {
                        afterdiscount -= (OfferProducts[i].DiscountPercentage ?? 0 * OfferProducts[i].ItemPrice);
                    }
                    else if (OfferProducts[i].DiscountValue != null)
                    {
                        afterdiscount -= OfferProducts[i].DiscountValue ?? 0;
                    }

                    afterdiscount = afterdiscount * OfferProducts[i].Quantity;

                    decimal price = CalculateProductFinalPrice(OfferProducts[i].OfferTaxes, afterdiscount);
                    if (price != OfferProducts[i].PriceAfterTax)
                    {
                        Response.Result = false;
                        Response.Data = false;
                        Error error = new Error();
                        error.ErrorCode = "Err103";
                        error.ErrorMSG = "price of Product " + i + 1 + " not equal the calculated pice from backend which equals " + price;
                        Response.Errors.Add(error);
                        return Response;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }
        public decimal CalculateProductFinalPrice(List<GetTax> OfferTaxes, decimal PriceBeforeTax)
        {
            decimal finalPrice = 0;
            for (int i = 0; i < OfferTaxes.Count; i++)
            {
                if (OfferTaxes[i].TaxType != "T1" &&
                    OfferTaxes[i].TaxType != "T2" &&
                    OfferTaxes[i].IsPercentage == true &&
                    PriceBeforeTax > 0)
                {
                    OfferTaxes[i].TaxValue = ((decimal)OfferTaxes[i].TaxPercentage / 100) * PriceBeforeTax;
                }
            };

            decimal totalTaxTaxableTypeAmount = OfferTaxes.Aggregate<GetTax, decimal>(
        0,
        (total, element) => element.TaxType == "Taxable Types" &&
                element.TaxType != "T1" &&
                element.TaxType != "T2" &&
                element.TaxType != "T3" &&
                element.TaxType != "T4"
            ? (element.TaxValue ?? 0 + total)
            : total);

            decimal totalTaxT3Amount = OfferTaxes.Aggregate<GetTax, decimal>(
        0,
        (total, element) => element.TaxType == "T3"
            ? (element.TaxValue ?? 0 + total)
            : total);

            for (int i = 0; i < OfferTaxes.Count; i++)
            {
                if (OfferTaxes[i].TaxType == "T2" &&
                    PriceBeforeTax > 0)
                {
                    OfferTaxes[i].TaxValue = ((decimal)OfferTaxes[i].TaxPercentage / 100) * (totalTaxTaxableTypeAmount + totalTaxT3Amount + PriceBeforeTax);
                }
            }

            decimal totalTaxT2Amount = OfferTaxes.Aggregate<GetTax, decimal>(
        0,
        (total, element) => element.TaxType == "T2"
            ? (element.TaxValue ?? 0 + total)
            : total);

            for (int i = 0; i < OfferTaxes.Count; i++)
            {
                if (OfferTaxes[i].TaxType == "T1" &&
                    PriceBeforeTax > 0)
                {
                    OfferTaxes[i].TaxValue = ((decimal)OfferTaxes[i].TaxPercentage / 100) * (totalTaxTaxableTypeAmount + totalTaxT2Amount + totalTaxT3Amount + PriceBeforeTax);
                }
            }

            decimal totalTaxT4Amount = OfferTaxes.Aggregate<GetTax, decimal>(
                0,
                (total, element) => element.TaxType == "T4"
                    ? (element.TaxValue ?? 0 + total)
                    : total);

            decimal totalTaxAmount = OfferTaxes.Aggregate<GetTax, decimal>(
            0,
            (total, element) => element.TaxType != "T4"
                ? (element.TaxValue ?? 0 + total)
                : total) -
        totalTaxT4Amount;

            finalPrice = totalTaxAmount + PriceBeforeTax;

            return finalPrice;
        }
        public BaseResponseWithData<bool> UpdateProductsPricesInDB(long OfferID)
        {
            BaseResponseWithData<bool> Response = new BaseResponseWithData<bool>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            Response.Data = true;
            try
            {
                var offer = _unitOfWork.SalesOffers.GetById(OfferID);
                if (offer == null)
                {
                    Response.Result = false;
                    Response.Data = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Offer not found";
                    Response.Errors.Add(error);
                    return Response;
                }

                var products = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == OfferID, includes: new[] { "SalesOfferProductTaxes.Tax" }).ToList();

                List<OfferProductValidation> OfferProducts = products.Select(a => new OfferProductValidation()
                {
                    PriceAfterTax = 0,
                    ItemPrice = a.ItemPrice ?? 0,
                    Quantity = (decimal)(a.Quantity ?? 0),
                    DiscountPercentage = a.DiscountPercentage,
                    DiscountValue = a.DiscountValue,
                    OfferTaxes = a.SalesOfferProductTaxes.Select(x => new GetTax()
                    {
                        Id = x.Id,
                        TaxPercentage = x.Tax.TaxPercentage,
                        TaxValue = 0,
                        TaxName = x.Tax.TaxName,
                        TaxCode = x.Tax.TaxCode,
                        TaxType = x.Tax.TaxType,
                        IsPercentage = x.Tax.IsPercentage,

                    }).ToList(),
                }).ToList();
                for (int i = 0; i < OfferProducts.Count; i++)
                {
                    var afterdiscount = OfferProducts[i].ItemPrice;
                    if (OfferProducts[i].DiscountPercentage != null && OfferProducts[i].DiscountPercentage > 0)
                    {
                        afterdiscount -= (OfferProducts[i].DiscountPercentage ?? 0 * OfferProducts[i].ItemPrice);
                    }
                    else if (OfferProducts[i].DiscountValue != null && OfferProducts[i].DiscountValue > 0)
                    {
                        afterdiscount -= OfferProducts[i].DiscountValue ?? 0;
                    }

                    afterdiscount = afterdiscount * OfferProducts[i].Quantity;

                    products[i].SalesOfferProductTaxes.ToList().ForEach(a => { a.Value = afterdiscount * (a.Percentage / 100); _unitOfWork.Complete(); });
                    _unitOfWork.Complete();
                    decimal price = CalculateProductFinalPrice(OfferProducts[i].OfferTaxes, afterdiscount);
                    products[i].FinalPrice = price;
                    _unitOfWork.Complete();
                }
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }


        public BaseResponseWithData<string> GetAllClientsAccumulativeByMonths(AccountSalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                ExcelPackage excel = new ExcelPackage();
                var data = _unitOfWork.VAccounts.FindAllQueryable(a => true);
                if (filters.AdvancedTypeId != 0 && filters.AdvancedTypeId != null)
                {
                    data = data.Where(a => a.AdvanciedTypeId == filters.AdvancedTypeId).AsQueryable();
                }
                if (filters.AccountCategoryId != 0 && filters.AccountCategoryId != null)
                {
                    data = data.Where(a => a.AccountCategoryId == filters.AccountCategoryId).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.AccountIds))
                {
                    var ids = filters.AccountIds.Split(',');
                    var list = ids.Select(long.Parse).ToList();
                    data = data.Where(a => list.Contains(a.Id)).AsQueryable();
                }
                var clientsIds = data.ToList().Select(a => a.Id).ToList();
                if (clientsIds.Count() == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorMSG = "No Ids with these parameters";
                    Response.Errors.Add(err);
                    return Response;
                }
                var clientsIdString = string.Join(',', clientsIds);
                var GetAccountMouvementReportList = _unitOfWork.ClientAccounts.FindAll(a => clientsIds.Contains(a.AccountId), includes: new[] { "Client" }).GroupBy(a => a.ClientId);
                var clientsWorkSheet = excel.Workbook.Worksheets.Add("AllClients");

                if (GetAccountMouvementReportList.Count() == 0)
                {
                    clientsWorkSheet.DefaultRowHeight = 12;
                    clientsWorkSheet.Row(1).Height = 20;
                    clientsWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    clientsWorkSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    clientsWorkSheet.Row(1).Style.Font.Bold = true;
                    for (int i = 2; i <= 25; i++)
                    {
                        clientsWorkSheet.Cells[1, i].Value = i % 2 == 0 ? "Credit" : "Debit";
                    }
                    clientsWorkSheet.Row(2).Height = 20;
                    clientsWorkSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    clientsWorkSheet.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    clientsWorkSheet.Row(2).Style.Font.Bold = true;
                    clientsWorkSheet.Cells[2, 1, 2, 27].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    clientsWorkSheet.Cells[2, 1, 2, 27].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    clientsWorkSheet.Cells[2, 1, 2, 27].Style.Font.Color.SetColor(Color.White);

                    clientsWorkSheet.Cells[2, 1].Value = "Client Name";
                    clientsWorkSheet.Cells[2, 2, 2, 3].Value = "January/يناير";
                    clientsWorkSheet.Cells[2, 4, 2, 5].Value = "February/فبراير";
                    clientsWorkSheet.Cells[2, 6, 2, 7].Value = "March/مارس";
                    clientsWorkSheet.Cells[2, 8, 2, 9].Value = "April/إبريل";
                    clientsWorkSheet.Cells[2, 10, 2, 11].Value = "May/مايو";
                    clientsWorkSheet.Cells[2, 12, 2, 13].Value = "June/يونيو";
                    clientsWorkSheet.Cells[2, 14, 2, 15].Value = "July/يوليو";
                    clientsWorkSheet.Cells[2, 16, 2, 17].Value = "August/أغسطس";
                    clientsWorkSheet.Cells[2, 18, 2, 19].Value = "September/سبتمبر";
                    clientsWorkSheet.Cells[2, 20, 2, 21].Value = "October/أكتوبر";
                    clientsWorkSheet.Cells[2, 22, 2, 23].Value = "November/نوفمبر";
                    clientsWorkSheet.Cells[2, 24, 2, 25].Value = "December/ديسمبر";
                    clientsWorkSheet.Cells[2, 26].Value = "Credit";
                    clientsWorkSheet.Cells[2, 27].Value = "Debit";
                }
                else
                {
                    clientsWorkSheet.DefaultRowHeight = 12;
                    clientsWorkSheet.Row(1).Height = 20;
                    clientsWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    clientsWorkSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    clientsWorkSheet.Row(1).Style.Font.Bold = true;
                    for (int i = 2; i <= 25; i++)
                    {
                        clientsWorkSheet.Cells[1, i].Value = i % 2 == 0 ? "Credit" : "Debit";
                    }
                    clientsWorkSheet.Row(2).Height = 20;
                    clientsWorkSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    clientsWorkSheet.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    clientsWorkSheet.Row(2).Style.Font.Bold = true;
                    clientsWorkSheet.Cells[2, 1, 2, 27].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    clientsWorkSheet.Cells[2, 1, 2, 27].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    clientsWorkSheet.Cells[2, 1, 2, 27].Style.Font.Color.SetColor(Color.White);

                    clientsWorkSheet.Cells[2, 1].Value = "Client Name";
                    clientsWorkSheet.Cells[2, 2, 2, 3].Value = "January/يناير";
                    clientsWorkSheet.Cells[2, 4, 2, 5].Value = "February/فبراير";
                    clientsWorkSheet.Cells[2, 6, 2, 7].Value = "March/مارس";
                    clientsWorkSheet.Cells[2, 8, 2, 9].Value = "April/إبريل";
                    clientsWorkSheet.Cells[2, 10, 2, 11].Value = "May/مايو";
                    clientsWorkSheet.Cells[2, 12, 2, 13].Value = "June/يونيو";
                    clientsWorkSheet.Cells[2, 14, 2, 15].Value = "July/يوليو";
                    clientsWorkSheet.Cells[2, 16, 2, 17].Value = "August/أغسطس";
                    clientsWorkSheet.Cells[2, 18, 2, 19].Value = "September/سبتمبر";
                    clientsWorkSheet.Cells[2, 20, 2, 21].Value = "October/أكتوبر";
                    clientsWorkSheet.Cells[2, 22, 2, 23].Value = "November/نوفمبر";
                    clientsWorkSheet.Cells[2, 24, 2, 25].Value = "December/ديسمبر";
                    clientsWorkSheet.Cells[2, 26].Value = "Credit";
                    clientsWorkSheet.Cells[2, 27].Value = "Debit";

                    var rowIndex = 3;

                    foreach (var client in GetAccountMouvementReportList)
                    {
                        clientsWorkSheet.Cells[rowIndex, 1].Value = client.FirstOrDefault().Client.Name;

                        var from = new DateTime(DateTime.Now.Year, 1, 1);
                        var to = new DateTime(DateTime.Now.Year, 1, 30);

                        var colIndex = 2;
                        decimal sumCredit = 0;
                        decimal sumDebit = 0;
                        for (int i = 0; i < 12; i++)
                        {
                            var accumulative = _movementService.GetAccountMovementList_WithListAccountIds(clientsIdString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, from, to, client.FirstOrDefault()?.Client?.Id ?? 0, filters.SupplierId, filters.BranchId);

                            clientsWorkSheet.Cells[rowIndex, colIndex].Value = accumulative.Sum(a => a.Credit);
                            clientsWorkSheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "#,##0.00";
                            sumCredit += accumulative.Sum(a => a.Credit ?? 0);
                            clientsWorkSheet.Cells[rowIndex, colIndex + 1].Value = accumulative.Sum(a => a.Debit);
                            clientsWorkSheet.Cells[rowIndex, colIndex + 1].Style.Numberformat.Format = "#,##0.00";
                            sumDebit += accumulative.Sum(a => a.Debit ?? 0);

                            from = from.AddMonths(1);
                            to = to.AddMonths(1);
                            colIndex += 2;
                        }
                        clientsWorkSheet.Cells[rowIndex, 26].Value = sumCredit;
                        clientsWorkSheet.Cells[rowIndex, 26].Style.Numberformat.Format = "#,##0.00";
                        clientsWorkSheet.Cells[rowIndex, 27].Value = sumDebit;
                        clientsWorkSheet.Cells[rowIndex, 27].Style.Numberformat.Format = "#,##0.00";

                        rowIndex++;
                    }
                    for (int i = 1; i <= 27; i++)
                    {
                        clientsWorkSheet.Column(i).AutoFit();
                        clientsWorkSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        clientsWorkSheet.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    for (int i = 2; i <= 25; i++)
                    {
                        clientsWorkSheet.Column(i).OutlineLevel = 2;
                        clientsWorkSheet.Column(i).Collapsed = true;
                        if (i % 2 == 0)
                        {
                            clientsWorkSheet.Cells[2, i, 2, i + 1].Merge = true;
                        }

                    }
                }
                var path = $"Attachments\\{validation.CompanyName}\\AllClientFromSalesOfferReports";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\AllClientFromSalesOfferReport_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + $"\\AllClientFromSalesOfferReport_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
            
        }

        public BaseResponseWithData<string> GetClientFromSalesOfferReport(AccountSalesOfferReportFilter filters, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                ExcelPackage excel = new ExcelPackage();

                var data = _unitOfWork.VAccounts.FindAllQueryable(a => true);
                if (filters.AdvancedTypeId != 0 && filters.AdvancedTypeId != null)
                {
                    data = data.Where(a => a.AdvanciedTypeId == filters.AdvancedTypeId).AsQueryable();
                }
                if (filters.AccountCategoryId != 0 && filters.AccountCategoryId != null)
                {
                    data = data.Where(a => a.AccountCategoryId == filters.AccountCategoryId).AsQueryable();
                }
                if (!string.IsNullOrEmpty(filters.AccountIds))
                {
                    var ids = filters.AccountIds.Split(',');
                    var list = ids.Select(long.Parse).ToList();
                    data = data.Where(a => list.Contains(a.Id)).AsQueryable();
                }
                var clientsIds = data.ToList().Select(a => a.Id).ToList();
                if (clientsIds.Count() == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorMSG = "No Ids with these parameters";
                    Response.Errors.Add(err);
                    return Response;
                }
                var clientsIdString = string.Join(',', clientsIds);
                var GetAccountMouvementReportList = _unitOfWork.ClientAccounts.FindAll(a => clientsIds.Contains(a.AccountId), includes: new[] { "Client" }).GroupBy(a => a.ClientId);

                if (GetAccountMouvementReportList.Count() == 0)
                {
                    var clientsWorkSheet = excel.Workbook.Worksheets.Add("Client");
                    clientsWorkSheet.DefaultRowHeight = 12;
                    clientsWorkSheet.Row(1).Height = 20;
                    clientsWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    clientsWorkSheet.Row(1).Style.Font.Bold = true;
                    clientsWorkSheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    clientsWorkSheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    clientsWorkSheet.Cells[1, 1, 1, 17].Style.Font.Color.SetColor(Color.White);
                    clientsWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                    clientsWorkSheet.Cells[1, 2].Value = "From or To";
                    clientsWorkSheet.Cells[1, 3].Value = "Account Name";
                    clientsWorkSheet.Cells[1, 4].Value = "Related Account";
                    clientsWorkSheet.Cells[1, 5].Value = "Account Code";
                    clientsWorkSheet.Cells[1, 6].Value = "Account Category";
                    clientsWorkSheet.Cells[1, 7].Value = "Cu.";
                    clientsWorkSheet.Cells[1, 8].Value = "Credit";
                    clientsWorkSheet.Cells[1, 9].Value = "Debit";
                    clientsWorkSheet.Cells[1, 10].Value = "AccBalance";
                    clientsWorkSheet.Cells[1, 11].Value = "Journal Entry #";
                    clientsWorkSheet.Cells[1, 12].Value = "Description";
                    clientsWorkSheet.Cells[1, 13].Value = "Ref. Doc#";
                    clientsWorkSheet.Cells[1, 14].Value = "Created By";
                    clientsWorkSheet.Cells[1, 15].Value = "Method";
                    clientsWorkSheet.Cells[1, 16].Value = "Client Name";
                    clientsWorkSheet.Cells[1, 17].Value = "Project Name";
                    clientsWorkSheet.Column(1).AutoFit();
                    clientsWorkSheet.Column(2).AutoFit();
                    clientsWorkSheet.Column(3).AutoFit();
                    clientsWorkSheet.Column(4).AutoFit();
                    clientsWorkSheet.Column(5).AutoFit();
                    clientsWorkSheet.Column(6).AutoFit();
                    clientsWorkSheet.Column(7).AutoFit();
                    clientsWorkSheet.Column(8).AutoFit();
                    clientsWorkSheet.Column(9).AutoFit();
                    clientsWorkSheet.Column(10).AutoFit();
                    clientsWorkSheet.Column(11).AutoFit();
                    clientsWorkSheet.Column(12).AutoFit();
                    clientsWorkSheet.Column(13).AutoFit();
                    clientsWorkSheet.Column(14).AutoFit();
                    clientsWorkSheet.Column(15).AutoFit();
                    clientsWorkSheet.Column(16).AutoFit();
                    clientsWorkSheet.Column(17).AutoFit();
                }
                else
                {
                    foreach (var item in GetAccountMouvementReportList)
                    {

                        var clientsWorkSheet = excel.Workbook.Worksheets.Add(item.FirstOrDefault().Client.Name);
                        clientsWorkSheet.DefaultRowHeight = 12;
                        clientsWorkSheet.Row(1).Height = 20;
                        clientsWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        clientsWorkSheet.Row(1).Style.Font.Bold = true;
                        clientsWorkSheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        clientsWorkSheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                        clientsWorkSheet.Cells[1, 1, 1, 17].Style.Font.Color.SetColor(Color.White);
                        clientsWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                        clientsWorkSheet.Cells[1, 2].Value = "From or To";
                        clientsWorkSheet.Cells[1, 3].Value = "Account Name";
                        clientsWorkSheet.Cells[1, 4].Value = "Related Account";
                        clientsWorkSheet.Cells[1, 5].Value = "Account Code";
                        clientsWorkSheet.Cells[1, 6].Value = "Account Category";
                        clientsWorkSheet.Cells[1, 7].Value = "Cu.";
                        clientsWorkSheet.Cells[1, 8].Value = "Credit";
                        clientsWorkSheet.Cells[1, 9].Value = "Debit";
                        clientsWorkSheet.Cells[1, 10].Value = "AccBalance";
                        clientsWorkSheet.Cells[1, 11].Value = "Journal Entry #";
                        clientsWorkSheet.Cells[1, 12].Value = "Description";
                        clientsWorkSheet.Cells[1, 13].Value = "Ref. Doc#";
                        clientsWorkSheet.Cells[1, 14].Value = "Created By";
                        clientsWorkSheet.Cells[1, 15].Value = "Method";
                        clientsWorkSheet.Cells[1, 16].Value = "Client Name";
                        clientsWorkSheet.Cells[1, 17].Value = "Project Name";
                        if (item.Count() > 0)
                        {
                            int recordIndex = 2;
                            var items = _movementService.GetAccountMovementList_WithListAccountIds(clientsIdString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, filters.From, filters.To, item.FirstOrDefault()?.Client?.Id ?? 0, filters.SupplierId, filters.BranchId);
                            foreach (var client in items)
                            {

                                if (client.FromOrTo.ToLower().Contains("from"))
                                {
                                    client.FromOrTo = "To";
                                }
                                else
                                {
                                    client.FromOrTo = "From";
                                }
                                clientsWorkSheet.Cells[recordIndex, 1].Value = "C.D " + client.CreationDate + "\n" + " " + "E.D " + client.EntryDate;
                                clientsWorkSheet.Cells[recordIndex, 1].Style.WrapText = true;
                                clientsWorkSheet.Cells[recordIndex, 2].Value = client.FromOrTo;
                                clientsWorkSheet.Cells[recordIndex, 3].Value = client.AccountName;
                                clientsWorkSheet.Cells[recordIndex, 4].Value = client.ReleatedAccount != null ? client.ReleatedAccount : "-";
                                clientsWorkSheet.Cells[recordIndex, 5].Value = client.AccountCode != null ? client.AccountCode : "-";
                                clientsWorkSheet.Cells[recordIndex, 6].Value = client.AccountCategory + " " + "(" + client.AccountType + ")";
                                clientsWorkSheet.Cells[recordIndex, 7].Value = client.Currency != null ? client.Currency : "-";
                                clientsWorkSheet.Cells[recordIndex, 8].Value = client.Credit != null ? client.Credit : 0;
                                clientsWorkSheet.Cells[recordIndex, 9].Value = client.Debit != null ? client.Debit : 0;
                                clientsWorkSheet.Cells[recordIndex, 10].Value = client.Accumulative != null ? client.Accumulative : 0;
                                clientsWorkSheet.Cells[recordIndex, 11].Value = client.EntryDate != null ? client.EntryDate : "-";
                                clientsWorkSheet.Cells[recordIndex, 12].Value = client.Description != null ? client.Description : "-";
                                clientsWorkSheet.Cells[recordIndex, 13].Value = client.Document != null ? client.Document : "-";
                                clientsWorkSheet.Cells[recordIndex, 14].Value = client.CreatedBy != null ? client.CreatedBy : "-";
                                clientsWorkSheet.Cells[recordIndex, 15].Value = client.MethodName != null ? client.MethodName : "-";
                                clientsWorkSheet.Cells[recordIndex, 16].Value = client.ClientName != null ? client.ClientName : "-";
                                clientsWorkSheet.Cells[recordIndex, 17].Value = client.ProjectName != null ? client.ProjectName : "-";
                                clientsWorkSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                clientsWorkSheet.Row(recordIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                                recordIndex++;
                            }
                            clientsWorkSheet.Column(1).AutoFit();
                            clientsWorkSheet.Column(2).AutoFit();
                            clientsWorkSheet.Column(3).AutoFit();
                            clientsWorkSheet.Column(4).AutoFit();
                            clientsWorkSheet.Column(5).AutoFit();
                            clientsWorkSheet.Column(6).AutoFit();
                            clientsWorkSheet.Column(7).AutoFit();
                            clientsWorkSheet.Column(8).AutoFit();
                            clientsWorkSheet.Column(9).AutoFit();
                            clientsWorkSheet.Column(10).AutoFit();
                            clientsWorkSheet.Column(11).AutoFit();
                            clientsWorkSheet.Column(12).AutoFit();
                            clientsWorkSheet.Column(13).AutoFit();
                            clientsWorkSheet.Column(14).AutoFit();
                            clientsWorkSheet.Column(15).AutoFit();
                            clientsWorkSheet.Column(16).AutoFit();
                            clientsWorkSheet.Column(17).AutoFit();
                        }

                    }
                }
                var path = $"Attachments\\{CompanyName}\\ClientFromSalesOfferReports";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\ClientFromSalesOfferReport_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + $"\\ClientFromSalesOfferReport_{date}.xlsx";
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }


        public BaseResponseWithData<string> GetAllSuppliersAccumulativeByMonths(AccountSalesOfferReportFilter filters)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                ExcelPackage excel = new ExcelPackage();
                var suppliersIds = _unitOfWork.AdvanciedSettingAccounts.FindAll(a => a.AdvanciedTypeId == 31).Select(a => a.AccountId).ToList();
                if (suppliersIds.Count == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "No Accounts with Supplier AdvancedT";
                    Response.Errors.Add(err);
                    return Response;
                }
                var suppliersString = string.Join(',', suppliersIds);
                var GetSupplierMovementReportList = _unitOfWork.SupplierAccounts.FindAll(a => suppliersIds.Contains(a.AccountId), includes: new[] { "Supplier" }).GroupBy(a => a.SupplierId);
                var SuppliersWorkSheet = excel.Workbook.Worksheets.Add("AllSuppliers");

                if (GetSupplierMovementReportList.Count() == 0)
                {
                    SuppliersWorkSheet.DefaultRowHeight = 12;
                    SuppliersWorkSheet.Row(1).Height = 20;
                    SuppliersWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SuppliersWorkSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    SuppliersWorkSheet.Row(1).Style.Font.Bold = true;
                    for (int i = 2; i <= 25; i++)
                    {
                        SuppliersWorkSheet.Cells[1, i].Value = i % 2 == 0 ? "Credit" : "Debit";
                    }
                    SuppliersWorkSheet.Row(2).Height = 20;
                    SuppliersWorkSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SuppliersWorkSheet.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    SuppliersWorkSheet.Row(2).Style.Font.Bold = true;
                    SuppliersWorkSheet.Cells[2, 1, 2, 27].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SuppliersWorkSheet.Cells[2, 1, 2, 27].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    SuppliersWorkSheet.Cells[2, 1, 2, 27].Style.Font.Color.SetColor(Color.White);

                    SuppliersWorkSheet.Cells[2, 1].Value = "Supplier Name";
                    SuppliersWorkSheet.Cells[2, 2, 2, 3].Value = "January/يناير";
                    SuppliersWorkSheet.Cells[2, 4, 2, 5].Value = "February/فبراير";
                    SuppliersWorkSheet.Cells[2, 6, 2, 7].Value = "March/مارس";
                    SuppliersWorkSheet.Cells[2, 8, 2, 9].Value = "April/إبريل";
                    SuppliersWorkSheet.Cells[2, 10, 2, 11].Value = "May/مايو";
                    SuppliersWorkSheet.Cells[2, 12, 2, 13].Value = "June/يونيو";
                    SuppliersWorkSheet.Cells[2, 14, 2, 15].Value = "July/يوليو";
                    SuppliersWorkSheet.Cells[2, 16, 2, 17].Value = "August/أغسطس";
                    SuppliersWorkSheet.Cells[2, 18, 2, 19].Value = "September/سبتمبر";
                    SuppliersWorkSheet.Cells[2, 20, 2, 21].Value = "October/أكتوبر";
                    SuppliersWorkSheet.Cells[2, 22, 2, 23].Value = "November/نوفمبر";
                    SuppliersWorkSheet.Cells[2, 24, 2, 25].Value = "December/ديسمبر";
                    SuppliersWorkSheet.Cells[2, 26].Value = "Credit";
                    SuppliersWorkSheet.Cells[2, 27].Value = "Debit";
                }
                else
                {
                    SuppliersWorkSheet.DefaultRowHeight = 12;
                    SuppliersWorkSheet.Row(1).Height = 20;
                    SuppliersWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SuppliersWorkSheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    SuppliersWorkSheet.Row(1).Style.Font.Bold = true;
                    for (int i = 2; i <= 25; i++)
                    {
                        SuppliersWorkSheet.Cells[1, i].Value = i % 2 == 0 ? "Credit" : "Debit";
                    }
                    SuppliersWorkSheet.Row(2).Height = 20;
                    SuppliersWorkSheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SuppliersWorkSheet.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    SuppliersWorkSheet.Row(2).Style.Font.Bold = true;
                    SuppliersWorkSheet.Cells[2, 1, 2, 27].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SuppliersWorkSheet.Cells[2, 1, 2, 27].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    SuppliersWorkSheet.Cells[2, 1, 2, 27].Style.Font.Color.SetColor(Color.White);

                    SuppliersWorkSheet.Cells[2, 1].Value = "Supplier Name";
                    SuppliersWorkSheet.Cells[2, 2, 2, 3].Value = "January/يناير";
                    SuppliersWorkSheet.Cells[2, 4, 2, 5].Value = "February/فبراير";
                    SuppliersWorkSheet.Cells[2, 6, 2, 7].Value = "March/مارس";
                    SuppliersWorkSheet.Cells[2, 8, 2, 9].Value = "April/إبريل";
                    SuppliersWorkSheet.Cells[2, 10, 2, 11].Value = "May/مايو";
                    SuppliersWorkSheet.Cells[2, 12, 2, 13].Value = "June/يونيو";
                    SuppliersWorkSheet.Cells[2, 14, 2, 15].Value = "July/يوليو";
                    SuppliersWorkSheet.Cells[2, 16, 2, 17].Value = "August/أغسطس";
                    SuppliersWorkSheet.Cells[2, 18, 2, 19].Value = "September/سبتمبر";
                    SuppliersWorkSheet.Cells[2, 20, 2, 21].Value = "October/أكتوبر";
                    SuppliersWorkSheet.Cells[2, 22, 2, 23].Value = "November/نوفمبر";
                    SuppliersWorkSheet.Cells[2, 24, 2, 25].Value = "December/ديسمبر";
                    SuppliersWorkSheet.Cells[2, 26].Value = "Credit";
                    SuppliersWorkSheet.Cells[2, 27].Value = "Debit";

                    var rowIndex = 3;

                    foreach (var supplier in GetSupplierMovementReportList)
                    {
                        SuppliersWorkSheet.Cells[rowIndex, 1].Value = supplier.FirstOrDefault().Supplier.Name;

                        var from = new DateTime(DateTime.Now.Year, 1, 1);
                        var to = from.AddMonths(1).AddDays(-1);

                        var colIndex = 2;
                        decimal sumCredit = 0;
                        decimal sumDebit = 0;
                        for (int i = 0; i < 12; i++)
                        {
                            var accumulative = _movementService.GetAccountMovementList_WithListAccountIds(suppliersString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, from, to, filters.ClientId, supplier.FirstOrDefault()?.Supplier?.Id ?? 0, filters.BranchId);

                            SuppliersWorkSheet.Cells[rowIndex, colIndex].Value = accumulative.Sum(a => a.Credit);
                            SuppliersWorkSheet.Cells[rowIndex, colIndex].Style.Numberformat.Format = "#,##0.00";
                            sumCredit += accumulative.Sum(a => a.Credit ?? 0);
                            SuppliersWorkSheet.Cells[rowIndex, colIndex + 1].Value = accumulative.Sum(a => a.Debit);
                            SuppliersWorkSheet.Cells[rowIndex, colIndex + 1].Style.Numberformat.Format = "#,##0.00";
                            sumDebit += accumulative.Sum(a => a.Debit ?? 0);

                            from = from.AddMonths(1);
                            to = from.AddMonths(1).AddDays(-1);
                            colIndex += 2;
                        }
                        SuppliersWorkSheet.Cells[rowIndex, 26].Value = sumCredit;
                        SuppliersWorkSheet.Cells[rowIndex, 26].Style.Numberformat.Format = "#,##0.00";
                        SuppliersWorkSheet.Cells[rowIndex, 27].Value = sumDebit;
                        SuppliersWorkSheet.Cells[rowIndex, 27].Style.Numberformat.Format = "#,##0.00";

                        rowIndex++;
                    }
                    for (int i = 1; i <= 27; i++)
                    {
                        SuppliersWorkSheet.Column(i).AutoFit();
                        SuppliersWorkSheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        SuppliersWorkSheet.Column(i).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    for (int i = 2; i <= 25; i++)
                    {
                        SuppliersWorkSheet.Column(i).OutlineLevel = 2;
                        SuppliersWorkSheet.Column(i).Collapsed = true;
                        if (i % 2 == 0)
                        {
                            SuppliersWorkSheet.Cells[2, i, 2, i + 1].Merge = true;
                        }

                    }
                }
                var path = $"Attachments\\{validation.CompanyName}\\AllSuppliersFromSalesOfferReports";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\AllSuppliersFromSalesOfferReports_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + $"\\AllSuppliersFromSalesOfferReports_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }

        }

        public BaseResponseWithData<string> GetSupplierFromSalesOfferReport(AccountSalesOfferReportFilter filters, string CompanyName)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                ExcelPackage excel = new ExcelPackage();
                var suppliersIds = _unitOfWork.AdvanciedSettingAccounts.FindAll(a => a.AdvanciedTypeId == 31).Select(a => a.AccountId).ToList();
                if (suppliersIds.Count == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E101";
                    err.errorMSG = "No Accounts with Supplier AdvancedT";
                    Response.Errors.Add(err);
                    return Response;
                }
                var suppliersString = string.Join(',', suppliersIds);
                var GetSupplierMovementReportList = _unitOfWork.SupplierAccounts.FindAll(a => suppliersIds.Contains(a.AccountId), includes: new[] { "Supplier" }).GroupBy(a => a.SupplierId);
                Dictionary<string, string> AllSuppliers = new Dictionary<string, string>();
                if (GetSupplierMovementReportList.Count() == 0)
                {
                    var SuppliersWorkSheet = excel.Workbook.Worksheets.Add("Supplier");
                    SuppliersWorkSheet.DefaultRowHeight = 12;
                    SuppliersWorkSheet.Row(1).Height = 20;
                    SuppliersWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SuppliersWorkSheet.Row(1).Style.Font.Bold = true;
                    SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Font.Color.SetColor(Color.White);
                    SuppliersWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                    SuppliersWorkSheet.Cells[1, 2].Value = "From or To";
                    SuppliersWorkSheet.Cells[1, 3].Value = "Account Name";
                    SuppliersWorkSheet.Cells[1, 4].Value = "Related Account";
                    SuppliersWorkSheet.Cells[1, 5].Value = "Account Code";
                    SuppliersWorkSheet.Cells[1, 6].Value = "Account Category";
                    SuppliersWorkSheet.Cells[1, 7].Value = "Cu.";
                    SuppliersWorkSheet.Cells[1, 8].Value = "Credit";
                    SuppliersWorkSheet.Cells[1, 9].Value = "Debit";
                    SuppliersWorkSheet.Cells[1, 10].Value = "AccBalance";
                    SuppliersWorkSheet.Cells[1, 11].Value = "Journal Entry #";
                    SuppliersWorkSheet.Cells[1, 12].Value = "Description";
                    SuppliersWorkSheet.Cells[1, 13].Value = "Ref. Doc#";
                    SuppliersWorkSheet.Cells[1, 14].Value = "Created By";
                    SuppliersWorkSheet.Cells[1, 15].Value = "Method";
                    SuppliersWorkSheet.Cells[1, 16].Value = "Supplier Name";
                    SuppliersWorkSheet.Cells[1, 17].Value = "PO";
                    SuppliersWorkSheet.Column(1).AutoFit();
                    SuppliersWorkSheet.Column(2).AutoFit();
                    SuppliersWorkSheet.Column(3).AutoFit();
                    SuppliersWorkSheet.Column(4).AutoFit();
                    SuppliersWorkSheet.Column(5).AutoFit();
                    SuppliersWorkSheet.Column(6).AutoFit();
                    SuppliersWorkSheet.Column(7).AutoFit();
                    SuppliersWorkSheet.Column(8).AutoFit();
                    SuppliersWorkSheet.Column(9).AutoFit();
                    SuppliersWorkSheet.Column(10).AutoFit();
                    SuppliersWorkSheet.Column(11).AutoFit();
                    SuppliersWorkSheet.Column(12).AutoFit();
                    SuppliersWorkSheet.Column(13).AutoFit();
                    SuppliersWorkSheet.Column(14).AutoFit();
                    SuppliersWorkSheet.Column(15).AutoFit();
                    SuppliersWorkSheet.Column(16).AutoFit();
                    SuppliersWorkSheet.Column(17).AutoFit();
                }
                foreach (var item in GetSupplierMovementReportList)
                {
                    var SuppliersWorkSheet = excel.Workbook.Worksheets.Add(item.FirstOrDefault().Supplier.Name);
                    SuppliersWorkSheet.DefaultRowHeight = 12;
                    SuppliersWorkSheet.Row(1).Height = 20;
                    SuppliersWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    SuppliersWorkSheet.Row(1).Style.Font.Bold = true;
                    SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    SuppliersWorkSheet.Cells[1, 1, 1, 17].Style.Font.Color.SetColor(Color.White);
                    SuppliersWorkSheet.Cells[1, 1].Value = "Creation Date / Entry Date";
                    SuppliersWorkSheet.Cells[1, 2].Value = "From or To";
                    SuppliersWorkSheet.Cells[1, 3].Value = "Account Name";
                    SuppliersWorkSheet.Cells[1, 4].Value = "Related Account";
                    SuppliersWorkSheet.Cells[1, 5].Value = "Account Code";
                    SuppliersWorkSheet.Cells[1, 6].Value = "Account Category";
                    SuppliersWorkSheet.Cells[1, 7].Value = "Cu.";
                    SuppliersWorkSheet.Cells[1, 8].Value = "Credit";
                    SuppliersWorkSheet.Cells[1, 9].Value = "Debit";
                    SuppliersWorkSheet.Cells[1, 10].Value = "AccBalance";
                    SuppliersWorkSheet.Cells[1, 11].Value = "Journal Entry #";
                    SuppliersWorkSheet.Cells[1, 12].Value = "Description";
                    SuppliersWorkSheet.Cells[1, 13].Value = "Ref. Doc#";
                    SuppliersWorkSheet.Cells[1, 14].Value = "Created By";
                    SuppliersWorkSheet.Cells[1, 15].Value = "Method";
                    SuppliersWorkSheet.Cells[1, 16].Value = "Supplier Name";
                    SuppliersWorkSheet.Cells[1, 17].Value = "PO";
                    if (item.Count() > 0)
                    {
                        int recordIndex = 2;
                        var items = _movementService.GetAccountMovementList_WithListAccountIds(suppliersString, filters.CalcWithoutPrivate, filters.OrderByCreationDate, filters.From, filters.To, filters.ClientId, item.FirstOrDefault()?.Supplier?.Id ?? 0, filters.BranchId);
                        foreach (var supplier in items)
                        {
                            if (supplier.FromOrTo.ToLower().Contains("from"))
                            {
                                supplier.FromOrTo = "To";
                            }
                            else
                            {
                                supplier.FromOrTo = "From";
                            }
                            SuppliersWorkSheet.Cells[recordIndex, 1].Value = "C.D " + supplier.CreationDate + "\n" + " " + "E.D " + supplier.EntryDate;
                            SuppliersWorkSheet.Cells[recordIndex, 2].Value = supplier.FromOrTo;
                            SuppliersWorkSheet.Cells[recordIndex, 3].Value = supplier.AccountName;
                            SuppliersWorkSheet.Cells[recordIndex, 4].Value = supplier.ReleatedAccount != null ? supplier.ReleatedAccount : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 5].Value = supplier.AccountCode != null ? supplier.AccountCode : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 6].Value = supplier.AccountCategory + " " + "(" + supplier.AccountType + ")";
                            SuppliersWorkSheet.Cells[recordIndex, 7].Value = supplier.Currency != null ? supplier.Currency : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 8].Value = supplier.Credit != null ? supplier.Credit : 0;
                            SuppliersWorkSheet.Cells[recordIndex, 9].Value = supplier.Debit != null ? supplier.Debit : 0;
                            SuppliersWorkSheet.Cells[recordIndex, 10].Value = supplier.Accumulative != null ? supplier.Accumulative : 0;
                            SuppliersWorkSheet.Cells[recordIndex, 11].Value = supplier.EntryDate != null ? supplier.EntryDate : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 12].Value = supplier.Description != null ? supplier.Description : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 13].Value = supplier.Document != null ? supplier.Document : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 14].Value = supplier.CreatedBy != null ? supplier.CreatedBy : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 15].Value = supplier.MethodName != null ? supplier.MethodName : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 16].Value = supplier.SupplierName != null ? supplier.SupplierName : "-";
                            SuppliersWorkSheet.Cells[recordIndex, 17].Value = supplier.POID != null ? supplier.POID : "-";
                            SuppliersWorkSheet.Row(recordIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            SuppliersWorkSheet.Row(recordIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            recordIndex++;
                        }
                       
                            AllSuppliers[$"{SuppliersWorkSheet.Cells[recordIndex - 1, 16].Value??item.Select(a=>a.Supplier.Name).FirstOrDefault()}"] = SuppliersWorkSheet.Cells[recordIndex-1, 10].Value?.ToString()??"0";
                        
                        SuppliersWorkSheet.Column(1).AutoFit();
                        SuppliersWorkSheet.Column(2).AutoFit();
                        SuppliersWorkSheet.Column(3).AutoFit();
                        SuppliersWorkSheet.Column(4).AutoFit();
                        SuppliersWorkSheet.Column(5).AutoFit();
                        SuppliersWorkSheet.Column(6).AutoFit();
                        SuppliersWorkSheet.Column(7).AutoFit();
                        SuppliersWorkSheet.Column(8).AutoFit();
                        SuppliersWorkSheet.Column(9).AutoFit();
                        SuppliersWorkSheet.Column(10).AutoFit();
                        SuppliersWorkSheet.Column(11).AutoFit();
                        SuppliersWorkSheet.Column(12).AutoFit();
                        SuppliersWorkSheet.Column(13).AutoFit();
                        SuppliersWorkSheet.Column(14).AutoFit();
                        SuppliersWorkSheet.Column(15).AutoFit();
                        SuppliersWorkSheet.Column(16).AutoFit();
                        SuppliersWorkSheet.Column(17).AutoFit();

                       
                    }
                }
                var AllSupplierSheet = excel.Workbook.Worksheets.Add("AllSuppliers");
                AllSupplierSheet.DefaultRowHeight = 12;
                AllSupplierSheet.Row(1).Height = 20;
                AllSupplierSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                AllSupplierSheet.Row(1).Style.Font.Bold = true;
                AllSupplierSheet.Cells[1, 1, 1, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                AllSupplierSheet.Cells[1, 1, 1, 2].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                AllSupplierSheet.Cells[1, 1, 1, 2].Style.Font.Color.SetColor(Color.White);
                AllSupplierSheet.Cells[1, 1].Value = "Supplier Name";
                AllSupplierSheet.Cells[1, 2].Value = "Accumulative";
                var Index = 2;
                foreach (var supp in AllSuppliers)
                {
                    AllSupplierSheet.Cells[Index, 1].Value = supp.Key;
                    AllSupplierSheet.Cells[Index, 2].Value = supp.Value;
                    AllSupplierSheet.Row(Index).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    AllSupplierSheet.Row(Index).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    Index++;
                }
                AllSupplierSheet.Column(1).AutoFit();
                AllSupplierSheet.Column(2).AutoFit();
                var path = $"Attachments\\{CompanyName}\\SupplierFromSalesOfferReports";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\SupplierFromSalesOfferReport_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + $"\\SupplierFromSalesOfferReport_{date}.xlsx";
                return Response;

            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }

        public BaseResponseWithData<string> GetAccountAmountByAdvancedType(string CompanyName, int Year, bool ByEntryDate, long AccountCategoryId, long? AdvancedTypeId, int? BranchId)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Year == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorMSG = "Year Is Mendatory";
                    Response.Errors.Add(err);
                    return Response;
                }
                if (AccountCategoryId == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err102";
                    err.errorMSG = "Account Category Id Is Mendatory";
                    Response.Errors.Add(err);
                    return Response;
                }
                var category = _unitOfWork.AccountCategories.GetById(AccountCategoryId);
                if (category == null)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err103";
                    err.errorMSG = "Category Is not Found";
                    Response.Errors.Add(err);
                    return Response;
                }
                if (AdvancedTypeId != null)
                {
                    var type = _unitOfWork.AdvanciedTypes.GetById(AdvancedTypeId ?? 0);
                    if (type == null)
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "Err104";
                        err.errorMSG = "Advanced Type Is not Found";
                        Response.Errors.Add(err);
                        return Response;
                    }
                }
                var _Accounts = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllQueryable(a => a.AccountCategoryId == AccountCategoryId);
                if (AdvancedTypeId != null)
                {
                    _Accounts = _Accounts.Where(a => a.AdvanciedTypeId == AdvancedTypeId).AsQueryable();
                }
                if (ByEntryDate)
                {
                    _Accounts = _Accounts.Where(a => a.EntryDate.Year == Year).AsQueryable();
                }
                else
                {
                    _Accounts = _Accounts.Where(a => a.CreationDate.Year == Year).AsQueryable();
                }
                var Accounts = _Accounts.ToList();
                var Branches = Accounts.Select(a => a.BranchId).Distinct().ToList();

                ExcelPackage excel = new ExcelPackage();
                if (BranchId == null)
                {
                    var ExpensesWorkSheet = excel.Workbook.Worksheets.Add($"{category.AccountCategoryName}ByBranch");
                    ExpensesWorkSheet.DefaultRowHeight = 12;
                    ExpensesWorkSheet.Row(1).Height = 20;
                    ExpensesWorkSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ExpensesWorkSheet.Row(1).Style.Font.Bold = true;
                    ExpensesWorkSheet.Cells[1, 1, 1, 2 + Branches.Count()].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ExpensesWorkSheet.Cells[1, 1, 1, 2 + Branches.Count()].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    ExpensesWorkSheet.Cells[1, 1, 1, 2 + Branches.Count()].Style.Font.Color.SetColor(Color.White);
                    ExpensesWorkSheet.Cells[1, 1].Value = "AdvancedType Name";
                    ExpensesWorkSheet.Cells[1, 2].Value = "Account Name";
                    var columnIndex = 3;
                    var BranchNames = _unitOfWork.Branches.FindAll(a => Accounts.Select(b => b.BranchId).Contains(a.Id)).ToList();
                    foreach (var Branch in Branches)
                    {
                        var BranchName = BranchNames.Where(a => a.Id == Branch).FirstOrDefault()?.Name ?? "";
                        if (BranchName != null)
                        {
                            ExpensesWorkSheet.Cells[1, columnIndex].Value = $"فرع {BranchName}";
                        }
                        else
                        {
                            ExpensesWorkSheet.Cells[1, columnIndex].Value = "بدون فرع";
                        }
                        columnIndex++;
                    }
                    var AccountsOfJournal = Accounts.GroupBy(a => new { a.AdvanciedTypeName, a.AccountTypeName }).ToList();
                    //var UniqueAccounts = AccountsOfJournal.GroupBy(a => new {a.AccountName,a.BranchId}).Select(a=>a.FirstOrDefault()).ToList();
                    var AccountsOfJe = AccountsOfJournal;
                    var rowIndex = 2;

                    foreach (var acc in AccountsOfJe)
                    {
                        ExpensesWorkSheet.Row(rowIndex).OutlineLevel = 1;
                        ExpensesWorkSheet.Row(rowIndex).Collapsed = false;
                        ExpensesWorkSheet.Cells[rowIndex, 1].Value = acc.Key.AdvanciedTypeName ?? "General";
                        ExpensesWorkSheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        ExpensesWorkSheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        foreach (var Branch in Branches)
                        {
                            ExpensesWorkSheet.Cells[rowIndex, 3 + Branches.IndexOf(Branch)].Value = acc.Where(a => a.BranchId == Branch).Select(a => Math.Abs(a.Amount)).Sum();
                            var x = acc.Where(a => a.BranchId == Branch).ToList();
                            ExpensesWorkSheet.Column(3 + Branches.IndexOf(Branch)).Style.Numberformat.Format = "#,##0.00";
                            ExpensesWorkSheet.Column(3 + Branches.IndexOf(Branch)).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        rowIndex++;
                        var uniqueNames = acc.Select(a => a.AccountName).ToList().Distinct();
                        foreach (var Name in uniqueNames)
                        {
                            ExpensesWorkSheet.Cells[rowIndex, 2].Value = Name;
                            foreach (var Branch in Branches)
                            {
                                ExpensesWorkSheet.Cells[rowIndex, 3 + Branches.IndexOf(Branch)].Value = Accounts.Where(a => a.AccountName == Name && a.BranchId == Branch).Sum(a => Math.Abs(a.Amount));
                                ExpensesWorkSheet.Column(3 + Branches.IndexOf(Branch)).Style.Numberformat.Format = "#,##0.00";
                            }
                            ExpensesWorkSheet.Row(rowIndex).OutlineLevel = 2;
                            ExpensesWorkSheet.Row(rowIndex).Collapsed = true;
                            ExpensesWorkSheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            ExpensesWorkSheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            rowIndex++;
                        }
                    }
                    for (int i = 1; i <= 2 + Branches.Count(); i++)
                    {
                        ExpensesWorkSheet.Column(i).AutoFit();
                    }
                    ExpensesWorkSheet.View.FreezePanes(2, 3 + Branches.Count());
                }
                else
                {
                    var Branch = _unitOfWork.Branches.GetById((int)BranchId);
                    if (Branch == null)
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "Err105";
                        err.errorMSG = "Branch Is not Found";
                        Response.Errors.Add(err);
                        return Response;
                    }

                    var AccountsOfJournal = Accounts.Where(a => a.BranchId == Branch.Id).GroupBy(a => new { a.AdvanciedTypeName, a.AccountTypeName }).ToList();
                    var AccountsOfJe = AccountsOfJournal;
                    var BranchName = Branch.Name;
                    var branchSheet = BranchName != null ? excel.Workbook.Worksheets.Add($"{BranchName}_{Year}") : excel.Workbook.Worksheets.Add($"بدون فرع");

                    branchSheet.DefaultRowHeight = 12;
                    branchSheet.Row(1).Height = 20;
                    branchSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    branchSheet.Row(1).Style.Font.Bold = true;
                    branchSheet.Cells[1, 1, 1, 14].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    branchSheet.Cells[1, 1, 1, 14].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    branchSheet.Cells[1, 1, 1, 14].Style.Font.Color.SetColor(Color.White);
                    branchSheet.Cells[1, 1].Value = "AdvancedType Name";
                    branchSheet.Cells[1, 2].Value = "Account Name";
                    branchSheet.Cells[1, 3].Value = "January/يناير";
                    branchSheet.Cells[1, 4].Value = "February/فبراير";
                    branchSheet.Cells[1, 5].Value = "March/مارس";
                    branchSheet.Cells[1, 6].Value = "April/إبريل";
                    branchSheet.Cells[1, 7].Value = "May/مايو";
                    branchSheet.Cells[1, 8].Value = "June/يونيو";
                    branchSheet.Cells[1, 9].Value = "July/يوليو";
                    branchSheet.Cells[1, 10].Value = "August/أغسطس";
                    branchSheet.Cells[1, 11].Value = "September/سبتمبر";
                    branchSheet.Cells[1, 12].Value = "October/أكتوبر";
                    branchSheet.Cells[1, 13].Value = "November/نوفمبر";
                    branchSheet.Cells[1, 14].Value = "December/ديسمبر";
                    var rowIndex = 2;
                    foreach (var acc in AccountsOfJe)
                    {
                        branchSheet.Row(rowIndex).OutlineLevel = 1;
                        branchSheet.Row(rowIndex).Collapsed = false;
                        branchSheet.Cells[rowIndex, 1].Value = acc.Key.AdvanciedTypeName ?? "General";
                        branchSheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        branchSheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        for (int i = 1; i <= 12; i++)
                        {
                            if (ByEntryDate)
                            {
                                branchSheet.Cells[rowIndex, i + 2].Value = acc.Where(a => a.EntryDate.Month == i).Select(a => Math.Abs(a.Amount)).Sum();
                            }
                            else
                            {
                                branchSheet.Cells[rowIndex, i + 2].Value = Math.Abs(acc.Where(a => a.CreationDate.Month == i).Select(a => Math.Abs(a.Amount)).Sum());
                            }

                            branchSheet.Column(i + 2).Style.Numberformat.Format = "#,##0.00";
                            branchSheet.Column(i + 2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        rowIndex++;
                        var uniqueNames = acc.Select(a => a.AccountName).ToList().Distinct();
                        foreach (var Name in uniqueNames)
                        {
                            branchSheet.Cells[rowIndex, 2].Value = Name;
                            for (int i = 1; i <= 12; i++)
                            {
                                if (ByEntryDate)
                                {
                                    branchSheet.Cells[rowIndex, i + 2].Value = Accounts.Where(a => a.AccountName == Name && a.BranchId == Branch.Id && a.EntryDate.Month == i).Sum(a => Math.Abs(a.Amount));
                                }
                                else
                                {
                                    branchSheet.Cells[rowIndex, i + 2].Value = Accounts.Where(a => a.AccountName == Name && a.BranchId == Branch.Id && a.CreationDate.Month == i).Sum(a => Math.Abs(a.Amount));
                                }

                                branchSheet.Column(i + 2).Style.Numberformat.Format = "#,##0.00";
                                branchSheet.Column(i + 2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            }
                            branchSheet.Row(rowIndex).OutlineLevel = 2;
                            branchSheet.Row(rowIndex).Collapsed = true;
                            branchSheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                            branchSheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            rowIndex++;

                        }

                    }
                    for (int i = 1; i <= 14; i++)
                    {
                        branchSheet.Column(i).AutoFit();
                    }
                    branchSheet.View.FreezePanes(2, 15);

                }







                var path = $"Attachments\\{CompanyName}\\{category.AccountCategoryName}Reports";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\{category.AccountCategoryName}Report_{date}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + $"\\{category.AccountCategoryName}Report_{date}.xlsx";
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }


        public BaseResponseWithData<string> GetAccountAmountByCategoryName(string CompanyName, int Year, bool ByEntryDate, int? BranchId, string AccountIds)
        {
            BaseResponseWithData<string> Response = new BaseResponseWithData<string>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                if (Year == 0)
                {
                    Response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "Err101";
                    err.errorMSG = "Year Is Mendatory";
                    Response.Errors.Add(err);
                    return Response;
                }
                Branch? Branch = null;
                if (BranchId != null)
                {
                    Branch = _unitOfWork.Branches.GetById((int)BranchId);
                    if (Branch == null)
                    {
                        Response.Result = false;
                        Error err = new Error();
                        err.ErrorCode = "Err102";
                        err.errorMSG = "Branch not found";
                        Response.Errors.Add(err);
                        return Response;
                    }
                }
                List<long> accs = new List<long>() { };
                var AccountsIds = _unitOfWork.Accounts.FindAll(a => a.Haveitem == false).Select(a => a.Id).ToList();
                if (!string.IsNullOrEmpty(AccountIds))
                {
                    accs = AccountIds.Split(',').Select(long.Parse).ToList();
                    AccountsIds = AccountsIds.Where(a => accs.Contains(a)).ToList();
                }
                var _Accounts = _unitOfWork.VAccountOfJournalEntryWithDailies.FindAllQueryable(a => AccountsIds.Contains(a.AccountId));

                if (ByEntryDate)
                {
                    _Accounts = _Accounts.Where(a => a.EntryDate.Year == Year).AsQueryable();
                }
                else
                {
                    _Accounts = _Accounts.Where(a => a.CreationDate.Year == Year).AsQueryable();
                }
                if (Branch != null)
                {
                    _Accounts = _Accounts.Where(a => a.BranchId == BranchId).AsQueryable();
                }
                var Accounts = _Accounts.ToList();
                ExcelPackage excel = new ExcelPackage();
                var CategorySheet = excel.Workbook.Worksheets.Add("temp");
                if (Branch != null)
                {
                    excel.Workbook.Worksheets[0].Name = $"BalanceSheet_{Branch.Name}";
                }
                else
                {
                    excel.Workbook.Worksheets[0].Name = $"BalanceSheet";
                }
                var AccountsOfJournal = Accounts.GroupBy(a => new { a.AccountCategoryName, a.AccountTypeName }).ToList();
                var AccountsOfJe = AccountsOfJournal;
                CategorySheet.DefaultRowHeight = 12;
                CategorySheet.Row(1).Height = 20;
                CategorySheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                CategorySheet.Row(1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                CategorySheet.Row(1).Style.Font.Bold = true;
                for (int i = 3; i <= 26; i++)
                {
                    CategorySheet.Cells[1, i].Value = i % 2 != 0 ? "Credit" : "Debit";
                }
                CategorySheet.Row(2).Height = 20;
                CategorySheet.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                CategorySheet.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                CategorySheet.Row(2).Style.Font.Bold = true;
                CategorySheet.Cells[2, 1, 2, 28].Style.Fill.PatternType = ExcelFillStyle.Solid;
                CategorySheet.Cells[2, 1, 2, 28].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                CategorySheet.Cells[2, 1, 2, 28].Style.Font.Color.SetColor(Color.White);
                CategorySheet.Cells[2, 1].Value = "Category Name";
                CategorySheet.Cells[2, 2].Value = "Account Name";
                CategorySheet.Cells[2, 3, 2, 4].Value = "January/يناير";
                CategorySheet.Cells[2, 5, 2, 6].Value = "February/فبراير";
                CategorySheet.Cells[2, 7, 2, 8].Value = "March/مارس";
                CategorySheet.Cells[2, 9, 2, 10].Value = "April/إبريل";
                CategorySheet.Cells[2, 11, 2, 12].Value = "May/مايو";
                CategorySheet.Cells[2, 13, 2, 14].Value = "June/يونيو";
                CategorySheet.Cells[2, 15, 2, 16].Value = "July/يوليو";
                CategorySheet.Cells[2, 17, 2, 18].Value = "August/أغسطس";
                CategorySheet.Cells[2, 19, 2, 20].Value = "September/سبتمبر";
                CategorySheet.Cells[2, 21, 2, 22].Value = "October/أكتوبر";
                CategorySheet.Cells[2, 23, 2, 24].Value = "November/نوفمبر";
                CategorySheet.Cells[2, 25, 2, 26].Value = "December/ديسمبر";
                CategorySheet.Cells[2, 25, 2, 26].Value = "December/ديسمبر";
                CategorySheet.Cells[2, 27].Value = "Credit";
                CategorySheet.Cells[2, 28].Value = "Debit";
                var rowIndex = 3;

                foreach (var acc in AccountsOfJe)
                {
                    CategorySheet.Row(rowIndex).OutlineLevel = 1;
                    CategorySheet.Row(rowIndex).Collapsed = false;
                    CategorySheet.Cells[rowIndex, 1].Value = acc.Key.AccountCategoryName ?? "General";
                    CategorySheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    CategorySheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    CategorySheet.Cells[rowIndex, 27].Value = acc.Select(a => Math.Abs(a.Credit)).Sum();
                    CategorySheet.Cells[rowIndex, 28].Value = acc.Select(a => Math.Abs(a.Debit)).Sum();

                    for (int i = 3; i <= 26; i++)
                    {
                        if (i % 2 != 0)
                        {
                            var month = (int)(i / 2);
                            if (ByEntryDate)
                            {
                                CategorySheet.Cells[rowIndex, i].Value = acc.Where(a => a.EntryDate.Month == month).Select(a => Math.Abs(a.Credit)).Sum();

                                CategorySheet.Cells[rowIndex, i + 1].Value = acc.Where(a => a.EntryDate.Month == month).Select(a => Math.Abs(a.Debit)).Sum();
                            }
                            else
                            {
                                CategorySheet.Cells[rowIndex, i].Value = Math.Abs(acc.Where(a => a.CreationDate.Month == month).Select(a => Math.Abs(a.Credit)).Sum());

                                CategorySheet.Cells[rowIndex, i + 1].Value = Math.Abs(acc.Where(a => a.CreationDate.Month == month).Select(a => Math.Abs(a.Debit)).Sum());
                            }
                        }

                        CategorySheet.Column(i).Style.Numberformat.Format = "#,##0.00";
                        CategorySheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    rowIndex++;
                    var uniqueNames = acc.Select(a => a.AccountName).ToList().Distinct();
                    foreach (var Name in uniqueNames)
                    {
                        CategorySheet.Cells[rowIndex, 2].Value = Name;
                        for (int i = 3; i <= 26; i++)
                        {
                            if (i % 2 != 0)
                            {
                                var month = (int)(i / 2);
                                if (ByEntryDate)
                                {
                                    CategorySheet.Cells[rowIndex, i].Value = Accounts.Where(a => a.AccountName == Name && a.EntryDate.Month == month).Sum(a => Math.Abs(a.Credit));
                                    CategorySheet.Cells[rowIndex, i + 1].Value = Accounts.Where(a => a.AccountName == Name && a.EntryDate.Month == month).Sum(a => Math.Abs(a.Debit));
                                }
                                else
                                {
                                    CategorySheet.Cells[rowIndex, i].Value = Accounts.Where(a => a.AccountName == Name && a.CreationDate.Month == month).Sum(a => Math.Abs(a.Credit));
                                    CategorySheet.Cells[rowIndex, i + 1].Value = Accounts.Where(a => a.AccountName == Name && a.CreationDate.Month == month).Sum(a => Math.Abs(a.Debit));
                                }
                            }
                            CategorySheet.Column(i).Style.Numberformat.Format = "#,##0.00";
                            CategorySheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        CategorySheet.Row(rowIndex).OutlineLevel = 2;
                        CategorySheet.Row(rowIndex).Collapsed = true;
                        CategorySheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        CategorySheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        rowIndex++;

                    }


                }
                CategorySheet.Column(27).Style.Numberformat.Format = "#,##0.00";
                CategorySheet.Column(27).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                CategorySheet.Column(28).Style.Numberformat.Format = "#,##0.00";
                CategorySheet.Column(28).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                for (int i = 1; i <= 28; i++)
                {
                    CategorySheet.Column(i).AutoFit();
                }
                for (int i = 3; i <= 26; i++)
                {
                    CategorySheet.Column(i).OutlineLevel = 2;
                    CategorySheet.Column(i).Collapsed = true;
                    if (i % 2 != 0)
                    {
                        CategorySheet.Cells[2, i, 2, i + 1].Merge = true;
                    }

                }
                CategorySheet.Row(rowIndex).OutlineLevel = 1;
                CategorySheet.Row(rowIndex).Collapsed = false;
                CategorySheet.Row(rowIndex + 1).OutlineLevel = 2;
                CategorySheet.Row(rowIndex + 1).Collapsed = true;
                var path = $"Attachments\\{CompanyName}\\BalanceSheets";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var temp = "";
                var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                if (Branch != null)
                {
                    temp = $"\\BalanceSheet_{Branch.Name}_{date}.xlsx";
                }
                else
                {
                    temp = $"\\BalanceSheet_{date}.xlsx";
                }
                var excelPath = savedPath + temp;
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                Response.Data = Globals.baseURL + '\\' + path + temp;
                return Response;
            }



            catch (Exception ex)
            {
                Response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "Exception :" + ex.Message;
                Response.Errors.Add(err);
                return Response;
            }
        }

        public List<GetInvoiceData> GetSalesOfferInvoicesList(long SalesOfferId)
        {
            var SalesOfferInvoices = new List<GetInvoiceData>();
            // Modified By michael markos 2022-10-25
            // Check if this sales offer parent for other sales offer in case Invoice Type 2 or 3
            var SalesOfferObjDB = _unitOfWork.SalesOffers.FindAll(x => x.Id == SalesOfferId).FirstOrDefault();
            var IDSSalesOfferChild = _unitOfWork.InvoiceCnandDns.FindAll(x => x.ParentSalesOfferId == SalesOfferId).Select(x => x.SalesOfferId).ToList();
            IDSSalesOfferChild.Add(SalesOfferId);
            var SalesOfferInvoicesDb = _unitOfWork.Invoices.FindAll(a => (a.SalesOfferId != null ? IDSSalesOfferChild.Contains((long)a.SalesOfferId) : false) && a.Active == true).ToList();
            //var SalesOfferInvoicesDb = _Context.Invoices.Where(a => a.SalesOfferId == SalesOfferId && a.Active == true).ToList();
            if (SalesOfferInvoicesDb.Count > 0)
            {
                var InvoicesIds = SalesOfferInvoicesDb.Select(a => a.Id).ToList();
                var SalesOfferInvoicesItemsDb = _unitOfWork.InvoiceItems.FindAll(a => InvoicesIds.Contains(a.InvoiceId)).ToList();
                var SalesOfferProductsIds = SalesOfferInvoicesItemsDb.Select(a => a.SalesOfferProductId).ToList();
                var SalesOfferProductsDb = _unitOfWork.SalesOfferProducts.FindAll(a => SalesOfferProductsIds.Contains(a.Id), includes: new[] { "InventoryItem" }).ToList();
                // Update Modified Dublicated
                var SalesOfferProductListDb = _unitOfWork.VSalesOfferProducts.FindAll(a => SalesOfferProductsIds.Contains(a.Id)).ToList();
                var SalesOfferProductTaxDb = _unitOfWork.SalesOfferProductTaxes.GetAll().ToList();

                SalesOfferInvoices = SalesOfferInvoicesDb.Select(invoice => new GetInvoiceData
                {
                    Id = invoice.Id,
                    Active = invoice.Active,
                    PayerClientId = invoice.ClientId,
                    PayerClientName = invoice.ClientId != null ? Common.GetClientName((long)invoice.ClientId, _Context) : null,
                    CreationType = invoice.CreationType,
                    eInvoiceAcceptDate = invoice.EInvoiceAcceptDate != null ? invoice.EInvoiceAcceptDate.ToString().Split(' ')[0] : null,
                    eInvoiceId = invoice.EInvoiceId,
                    eInvoiceStatus = invoice.EInvoiceStatus,
                    InvoiceDate = invoice.InvoiceDate != null ? invoice.InvoiceDate.ToString().Split(' ')[0] : null,
                    InvoiceFor = invoice.InvoiceFor,
                    InvoiceType = invoice.InvoiceType,
                    IsClosed = invoice.IsClosed,
                    Revision = invoice.Revision,
                    SalesOfferId = invoice.SalesOfferId,
                    Serial = invoice.Serial,
                    TotalInvoiceAmount = SalesOfferObjDB.FinalOfferPrice,
                    InvoiceItemsList = SalesOfferInvoicesItemsDb.Where(a => a.InvoiceId == invoice.Id).Count() > 0 ? SalesOfferInvoicesItemsDb.Where(a => a.InvoiceId == invoice.Id).Select(invoiceItem => new InvoiceItemData
                    {
                        Id = invoiceItem.Id,
                        Comment = invoiceItem.Comments,
                        eInvoiceAcceptDate = invoiceItem.EInvoiceAcceptDate != null ? invoiceItem.EInvoiceAcceptDate.ToString().Split(' ')[0] : null,
                        eInvoiceId = invoiceItem.EInvoiceId,
                        eInvoiceStatus = invoiceItem.EInvoiceStatus,
                        SalesOfferProductId = invoiceItem.SalesOfferProductId,
                        ItemName = SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId) != null ?
                                                        SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId).InventoryItem.Name : "",
                        ItemDescription = SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId) != null ?
                                                        SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId).ItemPricingComment : "",
                        Qty = SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId) != null ?
                                                        SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId).Quantity : 0,
                        ItemPrice = SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId) != null ?
                                                        SalesOfferProductsDb.FirstOrDefault(x => x.Id == invoiceItem.SalesOfferProductId).ItemPrice : 0,
                        ItemTotalTax = SalesOfferProductTaxDb.Where(x => x.SalesOfferProductId == invoiceItem.SalesOfferProductId).Count() > 0 ?
                                                        SalesOfferProductTaxDb.Where(x => x.SalesOfferProductId == invoiceItem.SalesOfferProductId).Sum(x => x.Value) : 0,
                    }).ToList() : null
                }).ToList();
            }

            return SalesOfferInvoices;
        }

        public GetSalesOfferListResponse GetSalesOfferList(GetSalesOfferListFilters filters, string OfferStatusParam)
        {
            GetSalesOfferListResponse Response = new GetSalesOfferListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {

                    if (!string.IsNullOrEmpty(filters.OfferType))
                    {
                        filters.OfferType = filters.OfferType.ToLower();
                    }

                    if (!string.IsNullOrEmpty(filters.SupportedBy))
                    {
                        filters.SupportedBy = HttpUtility.UrlDecode(filters.SupportedBy).ToLower();
                    }

                    List<long> ProductsIdsList = new List<long>();
                    if (!string.IsNullOrEmpty(filters.ProductsListString))
                    {
                        filters.ProductsListString = filters.ProductsListString.ToString();
                        ProductsIdsList = filters.ProductsListString.Split(',').Select(s => long.Parse(s.Trim())).ToList();
                    }

                    if (!string.IsNullOrEmpty(filters.ReleaseFilterString))
                    {
                        filters.ReleaseFilterString = filters.ReleaseFilterString.ToLower();
                        if (filters.ReleaseFilterString == "fully")
                        {
                            var FullyReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty != null && a.RemainQty != null && a.ReleasedQty >= a.RemainQty).Select(a => a.Id).ToList();
                            if (FullyReleasedProductsId.Count > 0)
                            {
                                ProductsIdsList.AddRange(FullyReleasedProductsId);
                            }
                        }
                        else if (filters.ReleaseFilterString == "partially")
                        {
                            var PartiallyReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty != null && a.RemainQty != null && a.ReleasedQty > 0 && a.ReleasedQty < a.RemainQty).Select(a => a.Id).ToList();
                            if (PartiallyReleasedProductsId.Count > 0)
                            {
                                ProductsIdsList.AddRange(PartiallyReleasedProductsId);
                            }
                        }
                        else if (filters.ReleaseFilterString == "not")
                        {
                            var NotReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty == null || a.ReleasedQty == 0).Select(a => a.Id).ToList();
                            if (NotReleasedProductsId.Count > 0)
                            {
                                ProductsIdsList.AddRange(NotReleasedProductsId);
                            }
                        }
                        else
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Release Filter!!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                        }
                    }

                    if (!string.IsNullOrEmpty(filters.ProductType))
                    {
                        filters.ProductType = filters.ProductType.ToLower();
                    }

                    if (!string.IsNullOrEmpty(filters.ClientName))
                    {
                        filters.ClientName = HttpUtility.UrlDecode(filters.ClientName).ToLower();
                    }

                    if (!string.IsNullOrEmpty(filters.ProjectName))
                    {
                        filters.ProjectName = HttpUtility.UrlDecode(filters.ProjectName).ToLower();
                    }

                    if (!string.IsNullOrEmpty(OfferStatusParam))
                    {
                        if (OfferStatusParam.ToLower() != "all")
                        {
                            filters.OfferStatus = OfferStatusParam.ToLower();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(filters.OfferStatus))
                            {
                                filters.OfferStatus = filters.OfferStatus.ToLower();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(filters.ReminderDateFilterString))
                    {
                        if (filters.OfferStatus != "" && filters.OfferStatus.ToLower() == "clientapproval")
                        {
                            filters.ReminderDateFilterString = filters.ReminderDateFilterString.ToLower();
                            if (filters.ReminderDateFilterString != "delay" && filters.ReminderDateFilterString != "today")
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-12";
                                error.ErrorMSG = "Invalid ReminderDate Filter!!";
                                Response.Errors.Add(error);
                                Response.Result = false;
                            }
                        }
                    }

                    var StartDate = filters.From ?? DateTime.Now;
                    var EndDate = filters.To ?? DateTime.Now;
                    var DateFilter = false;

                    if (filters.From != null)
                    {
                        DateFilter = true;
                        StartDate = (DateTime)filters.From;

                        if (filters.To != null)
                        {
                            EndDate = (DateTime)filters.To;
                        }
                    }
                    else
                    {
                        if (filters.To != null)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-13";
                            error.ErrorMSG = "You have to Enter Offer From Date!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                    }
                    /*if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                    {
                        long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    }*/

                    /*int BranchId = 0;
                    if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    {
                        int.TryParse(headers["BranchId"], out BranchId);
                    }*/

                    /*int CurrentPage = 1;
                    if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
                    {
                        int.TryParse(headers["CurrentPage"], out CurrentPage);
                    }*/

                    /*int NumberOfItemsPerPage = 10;
                    if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
                    {
                        int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
                    }*/
                    /*string SearchKey = headers["SearchKey"];
                    if (headers["SearchKey"] != null && headers["SearchKey"] != "")
                    {
                        SearchKey = headers["SearchKey"];
                    }*/

                    if (Response.Result)
                    {
                        var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "SalesPerson", "ClientAccounts", "Client", "CreatedByNavigation", "SalesOfferExtraCosts", "Projects" }).AsQueryable();
                        //var test1 = SalesOfferDBQuery.ToList();

                        if(filters.SalesOfferClassifiction== "POS")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "POS" || a.OfferType == "Sales Return").AsQueryable();
                        }
                        else if(filters.SalesOfferClassifiction == "InternalTicket")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType == "Internal Ticket return" || a.OfferType == "Internal Ticket").AsQueryable();
                        }
                        if (filters.CreatorUserId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.CreatedBy == filters.CreatorUserId).AsQueryable();
                        }
                        if (filters.StoreId != 0)
                        {
                            var InventoryStoreItemSalesofferIDsList = _unitOfWork.InventoryStoreItems.FindAll(x => x.InventoryStoreId == filters.StoreId && x.OperationType.Contains("POS")).Select(x => x.OrderId).ToList();

                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => InventoryStoreItemSalesofferIDsList.Contains(x.Id)).AsQueryable();
                        }
                        if (filters.HasInvoice != null)
                        {
                            if ((bool)filters.HasInvoice)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Invoices.Count > 0);
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.Invoices.Count > 0));
                            }
                        }
                        if (filters.InvoiceDate != null)
                        {

                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Invoices.Count > 0 && x.Invoices.Any((a => a.CreationDate.Date == ((DateTime)filters.InvoiceDate).Date)));

                        }

                        if (filters.HasProject != null)
                        {
                            if ((bool)filters.HasProject)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Projects.Count > 0);
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.Projects.Count > 0));
                            }
                        }

                        if (filters.ProjectDate != null)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Projects.Count > 0 && x.Projects.Any(a => a.CreationDate.Date == ((DateTime)filters.ProjectDate).Date));
                        }

                        if (filters.HasAutoJE != null)
                        {
                            if ((bool)filters.HasAutoJE)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.OfferId != null));
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.OfferId != null)));
                            }
                        }
                        if (filters.HasJournalEntry != null)
                        {
                            if ((bool)filters.HasJournalEntry)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectId != null));
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectId == null));
                                var test = SalesOfferDBQuery.ToList();
                            }
                        }
                        if (filters.JournalEntryDate != null)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectId != null && a.CreationDate.Date == ((DateTime)filters.JournalEntryDate).Date));
                        }




                        // supplier Name , Offer Serial ,Project Name
                        if (!string.IsNullOrEmpty(filters.SearchKey))
                        {
                            filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
                                                       (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false))
                                                    //|| SalesOfferIDS.Contains(x.ID)
                                                    ).AsQueryable();
                        }


                        // For Vehicle Chassis and Plat No
                        if (!string.IsNullOrEmpty(filters.SearchKeyForChassisAndPlatNo))
                        {
                            var SearchKeyForChassisAndPlatNo = HttpUtility.UrlDecode(filters.SearchKeyForChassisAndPlatNo);
                            var SalesOfferIDSList = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll((x => (x.VehiclePerClient.ChassisNumber.Contains(SearchKeyForChassisAndPlatNo)) ||
                            (x.VehiclePerClient.PlatNumber.Contains(SearchKeyForChassisAndPlatNo) ) && x.SalesOfferId != null ), new[] { "VehiclePerClient" })
                                .Select(x=>(long)x.SalesOfferId).Distinct().ToList();
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => SalesOfferIDSList.Contains(x.Id)).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.OfferType))
                        {
                            if (filters.OfferType == "new project offer")
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == "new project offer" || a.OfferType.ToLower() == "direct sales").AsQueryable();
                            else
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == filters.OfferType).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.SupportedBy))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.SupportedBy.ToLower() == filters.SupportedBy).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.OfferStatus))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Status.ToLower() == filters.OfferStatus).AsQueryable();

                            if (filters.ReminderDateFilterString != "")
                            {
                                DateTime TodayDate = DateTime.Now.Date;
                                if (filters.ReminderDateFilterString == "today")
                                {
                                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ReminderDate == TodayDate).AsQueryable();
                                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ReminderDate);
                                }
                                else if (filters.ReminderDateFilterString == "delay")
                                {
                                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ReminderDate < TodayDate).AsQueryable();
                                }
                            }
                        }
                        if (filters.SalesPersonId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        }
                        if (filters.BranchId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ProductType))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProductType.ToLower() == filters.ProductType).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ClientName))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filters.ClientName)).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ProjectName))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(filters.ProjectName)).AsQueryable();
                        }
                        if (DateFilter)
                        {
                            if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= StartDate && a.ClientApprovalDate <= EndDate).AsQueryable();
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime(StartDate) && a.EndDate <= DateOnly.FromDateTime(EndDate)).AsQueryable();
                            }
                        }
                        else
                        {
                            if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                            {
                                var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                            }
                        }
                        if (ProductsIdsList.Count > 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesOfferProducts.Any(x => ProductsIdsList.Contains(x.InventoryItemId ?? 0))).AsQueryable();
                        }

                        if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate).OrderByDescending(a => a.CreationDate);
                        }
                        else
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
                        }


                        var OffersListDB = PagedList<SalesOffer>.Create(SalesOfferDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);

                        var TotalOffersCount = OffersListDB.TotalCount;
                        var TotalOffersPriceWithReturned = SalesOfferDBQuery.Sum(a => a.FinalOfferPrice) ?? 0;
                        var TotalOffersReturnedPrice = SalesOfferDBQuery.Where(a => a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                        var TotalOffersPrice = TotalOffersPriceWithReturned - 2 * TotalOffersReturnedPrice;
                        SalesOfferTypeDetails salesOfferTypeDetails = new SalesOfferTypeDetails()
                        {
                            TotalOffersCount = TotalOffersCount,
                            TotalOffersPrice = TotalOffersPrice
                        };

                        var OffersListResponse = new List<GetSalesOffer>();
                        var IDSSalesOffer = OffersListDB.Select(x => x.Id).ToList();
                        var ParentSalesOfferListDB = _unitOfWork.InvoiceCnandDns.FindAll((x => IDSSalesOffer.Contains(x.SalesOfferId) || IDSSalesOffer.Contains(x.ParentSalesOfferId)), new[] { "SalesOffer", "ParentSalesOffer" }).ToList();
                        foreach (var offer in OffersListDB)
                        {
                            long? projectId = null;
                            decimal QTYOfMatrialReleaseItem = 0;
                            double QTYOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.RemainQty ?? 0) ?? 0;
                            var SalesOfferProductCount = offer.SalesOfferProducts?.Count ?? 0;
                            if (offer.Projects.Count > 0)
                            {
                                var offerProject = offer.Projects.FirstOrDefault();
                                projectId = offerProject.Id;
                                if (offerProject.InventoryMatrialRequestItems.Count > 0)
                                {
                                    QTYOfMatrialReleaseItem = offerProject.InventoryMatrialRequestItems?.Sum(x => x.RecivedQuantity1 ?? 0) ?? 0;
                                }
                            }
                            // Calc Percentage Product Released

                            decimal Percent = 0;
                            string ReleaseStatus = "";
                            if (QTYOfSalesOfferProduct > 0)
                            {
                                ReleaseStatus = "Part";
                                Percent = (QTYOfMatrialReleaseItem / (decimal)QTYOfSalesOfferProduct) * 100;

                                if (Percent > 100)
                                {
                                    ReleaseStatus = "Exceeded";
                                }
                                if (Percent == 100)
                                {
                                    ReleaseStatus = "Full";
                                }
                            }

                            var totalExtraCostAmount = offer.SalesOfferExtraCosts?.Sum(a => (decimal?)a.Amount);
                            var totalTaxAmount = offer.SalesOfferInvoiceTaxes.Where(a => a.Active == true).Sum(a => (decimal?)a.TaxValue);
                            var totalDiscountAmount = offer.SalesOfferDiscounts?.Where(a => a.DiscountApproved == true && a.Active == true).Sum(a => (decimal?)a.DiscountValue);

                            decimal TotalSalesOfferAvgPrice = 0;
                            decimal TotalSalesOfferProfitMarginValue = 0;
                            decimal TotalSalesOfferProfitMarginPer = 0;

                            foreach (var product in offer.SalesOfferProducts)
                            {
                                var ItemPrice = product.ItemPrice ?? 0;
                                if (ItemPrice != 0)
                                {

                                    var ItemProfitPer = product.ProfitPercentage ?? 0;
                                    var ItemRemainQty = product.RemainQty ?? product.Quantity ?? 0;

                                    var ItemAvg = 100 * ItemPrice / (100 + ItemProfitPer);

                                    TotalSalesOfferProfitMarginValue += (ItemPrice - (100 * ItemPrice / (100 + ItemProfitPer))) * (decimal)ItemRemainQty;

                                    TotalSalesOfferAvgPrice += ItemAvg * (decimal)ItemRemainQty;
                                }
                            }

                            if (TotalSalesOfferAvgPrice > 0)
                            {
                                TotalSalesOfferProfitMarginPer = TotalSalesOfferProfitMarginValue / TotalSalesOfferAvgPrice * 100;
                            }
                            long? ParentSalesOfferId = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOfferId).FirstOrDefault();
                            string ParentSalesOfferSErial = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOffer?.OfferSerial).FirstOrDefault();
                            var ListChildrenSalesOffer = ParentSalesOfferListDB.Where(x => x.ParentSalesOfferId == offer.Id).Select(x => new ChildrenSalesOFfer { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial }).ToList();

                            GetSalesOffer salesOfferObj = new GetSalesOffer()
                            {
                                Id = offer.Id,
                                StartDate = offer.StartDate.ToShortDateString(),
                                EndDate = offer.EndDate != null ? offer.EndDate.ToString().Split(' ')[0] : null,
                                Note = offer.Note,
                                SalesPersonId = offer.SalesPersonId,
                                SalesPersonName = offer.SalesPerson.FirstName + ' ' + offer.SalesPerson.MiddleName + ' ' + offer.SalesPerson.LastName,
                                Status = offer.Status,
                                Completed = offer.Completed,
                                TechnicalInfo = offer.TechnicalInfo,
                                ProjectData = offer.ProjectData,
                                ProjectID = projectId,
                                FinancialInfo = offer.FinancialInfo,
                                PricingComment = offer.PricingComment,
                                OfferAmount = offer.OfferAmount,
                                SendingOfferConfirmation = offer.SendingOfferConfirmation,
                                SendingOfferDate = offer.SendingOfferDate != null ? offer.SendingOfferDate.ToString().Split(' ')[0] : null,
                                SendingOfferBy = offer.SendingOfferBy,
                                SendingOfferTo = offer.SendingOfferTo,
                                SendingOfferComment = offer.SendingOfferComment,
                                ClientApprove = offer.ClientApprove,
                                ClientComment = offer.ClientComment,
                                VersionNumber = offer.VersionNumber,
                                ClientApprovalDate = offer.ClientApprovalDate != null ? offer.ClientApprovalDate.ToString().Split(' ')[0] : null,
                                ClientId = offer.ClientId,
                                ClientName = offer.Client.Name,
                                ProductType = offer.ProductType,
                                ProjectName = offer.ProjectName,
                                ProjectLocation = offer.ProjectLocation,
                                ContactPersonMobile = offer.ContactPersonMobile,
                                ContactPersonEmail = offer.ContactPersonEmail,
                                ContactPersonName = offer.ContactPersonName,
                                ProjectStartDate = offer.ProjectStartDate != null ? offer.ProjectStartDate.ToString().Split(' ')[0] : null,
                                ProjectEndDate = offer.ProjectEndDate != null ? offer.ProjectEndDate.ToString().Split(' ')[0] : null,
                                BranchId = offer.BranchId,
                                OfferType = offer.OfferType,
                                ContractType = offer.ContractType,
                                OfferSerial = offer.OfferSerial,
                                ClientNeedsDiscount = offer.ClientNeedsDiscount,
                                RejectionReason = offer.RejectionReason,
                                NeedsInvoice = offer.NeedsInvoice,
                                NeedsExtraCost = offer.NeedsExtraCost,
                                OfferExpirationDate = offer.OfferExpirationDate != null ? offer.OfferExpirationDate.ToString().Split(' ')[0] : null,
                                OfferExpirationPeriod = offer.OfferExpirationPeriod,
                                ExtraOrDiscountPriceBySalesPerson = offer.ExtraOrDiscountPriceBySalesPerson,
                                FinalOfferPrice = offer.FinalOfferPrice,
                                TotalExtraCostAmount = totalExtraCostAmount ?? 0,
                                TotalDiscountAmount = totalDiscountAmount ?? 0,
                                TotalTaxAmount = totalTaxAmount ?? 0,
                                ReminderDate = offer.ReminderDate != null ? offer.ReminderDate.ToString().Split(' ')[0] : null,
                                CreationDate = offer.CreationDate.ToString(),
                                CreatorName = offer.CreatedByNavigation?.FirstName + " "+ offer.CreatedByNavigation?.LastName,
                                GrossProfitPercentage = TotalSalesOfferProfitMarginPer,
                                GrossProfitValue = TotalSalesOfferProfitMarginValue,
                                // Michael Markos 2022-11-28
                                ReleaseStatus = ReleaseStatus,
                                PercentReleased = Percent,
                                SalesOfferProductsCount = SalesOfferProductCount,
                                SalesOfferProductsQuantity = (decimal)QTYOfSalesOfferProduct,
                                ParentSalesOfferID = ParentSalesOfferId,
                                ParentSalesOfferSerial = ParentSalesOfferSErial,
                                ChildrenSalesOfferList = ListChildrenSalesOffer,
                            };

                            var OfferClientAccount = _unitOfWork.ClientAccounts.FindAll(a => a.OfferId == offer.Id).FirstOrDefault();
                            if (OfferClientAccount != null)
                            {
                                salesOfferObj.HasJournalEntryId = OfferClientAccount.DailyAdjustingEntryId;
                            }

                            var SalesOfferInvoices = GetSalesOfferInvoicesList(offer.Id);
                            if (SalesOfferInvoices != null && SalesOfferInvoices.Count > 0)
                            {
                                salesOfferObj.SalesOfferInvoices = SalesOfferInvoices;
                            }
                            OffersListResponse.Add(salesOfferObj);
                        }
                        salesOfferTypeDetails.SalesOfferList = OffersListResponse;
                        Response.SalesOfferList = salesOfferTypeDetails;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = filters.CurrentPage,
                            TotalPages = OffersListDB.TotalPages,
                            ItemsPerPage = filters.NumberOfItemsPerPage,
                            TotalItems = OffersListDB.TotalCount
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

        public GetSalesOffer GetSalesOfferInfo(long SalesOfferId)
        {

            var SalesOfferObj = new GetSalesOffer();
            var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId, includes: new[] { "SalesPerson", "VehicleMaintenanceJobOrderHistories.VehiclePerClient.Model", "ModifiedByNavigation", "CreatedByNavigation" }).FirstOrDefault();
            if (SalesOfferDb != null)
            {
                var SalesOfferProductsIds = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == SalesOfferDb.Id).Select(a => a.Id).ToList();
                var totalExtraCostAmount = _unitOfWork.VSalesOfferExtraCosts.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true).Sum(a => (decimal?)a.Amount);
                var totalTaxAmount = _unitOfWork.SalesOfferProductTaxes.FindAll(a => SalesOfferProductsIds.Contains(a.SalesOfferProductId)).Sum(a => (decimal?)a.Value);
                var totalDiscountAmount = _unitOfWork.SalesOfferDiscounts.FindAll(a => a.SalesOfferId == SalesOfferId && a.DiscountApproved == true && a.Active == true).Sum(a => (decimal?)a.DiscountValue);

                decimal TotalSalesOfferAvgPrice = 0;
                decimal TotalSalesOfferProfitMarginValue = 0;
                decimal TotalSalesOfferProfitMarginPer = 0;
                decimal TotalSalesOfferReleasedQty = 0;
                decimal TotalSalesOfferRemainQty = 0;
                decimal OfferReleasedItemsPercentage = 0;

                var SalesOfferProducts = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == SalesOfferDb.Id && a.Active == true).ToList();
                if (SalesOfferProducts != null && SalesOfferProducts.Count() > 0)
                {
                    foreach (var product in SalesOfferProducts)
                    {
                        var ItemPrice = product.ItemPrice ?? 0;
                        var ItemProfitPer = product.ProfitPercentage ?? 0;
                        var ItemRemainQty = product.RemainQty ?? product.Quantity ?? 0;
                        var ItemReleasedQty = product.ReleasedQty ?? product.ReleasedQty ?? 0;

                        var ItemAvg = (100 + ItemProfitPer) != 0 ? 100 * ItemPrice / (100 + ItemProfitPer) : 0;
                        TotalSalesOfferProfitMarginValue += (ItemPrice - ItemAvg) * (decimal)ItemRemainQty;
                        TotalSalesOfferAvgPrice += ItemAvg * (decimal)ItemRemainQty;
                        TotalSalesOfferRemainQty += (decimal)ItemRemainQty;
                        TotalSalesOfferReleasedQty += (decimal)ItemReleasedQty;
                    }
                }
                if (TotalSalesOfferAvgPrice > 0)
                {
                    TotalSalesOfferProfitMarginPer = TotalSalesOfferAvgPrice != 0 ? TotalSalesOfferProfitMarginValue / TotalSalesOfferAvgPrice * 100 : 0;
                }
                if (TotalSalesOfferRemainQty > 0)
                {
                    OfferReleasedItemsPercentage = TotalSalesOfferRemainQty != 0 ? TotalSalesOfferReleasedQty / TotalSalesOfferRemainQty * 100 : 0;
                }

                SalesOfferObj.ProjectID = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOfferId).Select(a => a.Id).FirstOrDefault();
                SalesOfferObj.Id = SalesOfferDb.Id;
                SalesOfferObj.StartDate = SalesOfferDb.StartDate.ToShortDateString();
                SalesOfferObj.EndDate = SalesOfferDb.EndDate != null ? SalesOfferDb.EndDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.Note = SalesOfferDb.Note;
                SalesOfferObj.SalesPersonId = SalesOfferDb.SalesPersonId;
                SalesOfferObj.SalesPersonName = SalesOfferDb.SalesPerson.FirstName + ' ' + SalesOfferDb.SalesPerson.MiddleName + ' ' + SalesOfferDb.SalesPerson.LastName;
                SalesOfferObj.Status = SalesOfferDb.Status;
                SalesOfferObj.Completed = SalesOfferDb.Completed;
                SalesOfferObj.TechnicalInfo = SalesOfferDb.TechnicalInfo;
                SalesOfferObj.ProjectData = SalesOfferDb.TechnicalInfo;
                SalesOfferObj.FinancialInfo = SalesOfferDb.FinancialInfo;
                SalesOfferObj.PricingComment = SalesOfferDb.PricingComment;
                SalesOfferObj.OfferAmount = SalesOfferDb.OfferAmount;
                SalesOfferObj.SendingOfferConfirmation = SalesOfferDb.SendingOfferConfirmation;
                SalesOfferObj.SendingOfferDate = SalesOfferDb.SendingOfferDate != null ? SalesOfferDb.SendingOfferDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.SendingOfferBy = SalesOfferDb.SendingOfferBy;
                SalesOfferObj.SendingOfferTo = SalesOfferDb.SendingOfferTo;
                SalesOfferObj.SendingOfferComment = SalesOfferDb.SendingOfferComment;
                SalesOfferObj.ClientApprove = SalesOfferDb.ClientApprove;
                SalesOfferObj.ClientComment = SalesOfferDb.ClientComment;
                SalesOfferObj.VersionNumber = SalesOfferDb.VersionNumber;
                SalesOfferObj.GrossProfitPercentage = TotalSalesOfferProfitMarginPer;
                SalesOfferObj.GrossProfitValue = TotalSalesOfferProfitMarginValue;
                SalesOfferObj.PercentReleased = OfferReleasedItemsPercentage;
                SalesOfferObj.ClientApprovalDate = SalesOfferDb.ClientApprovalDate != null ? SalesOfferDb.ClientApprovalDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.CreatorName = SalesOfferDb.CreatedByNavigation?.FirstName + " " + SalesOfferDb.CreatedByNavigation?.LastName;
                SalesOfferObj.ModifierName = SalesOfferDb.ModifiedByNavigation?.FirstName + " " + SalesOfferDb.ModifiedByNavigation?.LastName;
                if (SalesOfferDb.ClientId != null)
                {
                    var VehicleMaintenanceJobOrderHistories = SalesOfferDb.VehicleMaintenanceJobOrderHistories;
                    var ClientDb = _unitOfWork.Clients.FindAll(a => a.Id == (long)SalesOfferDb.ClientId).FirstOrDefault();
                    SalesOfferObj.ClientId = SalesOfferDb.ClientId;
                    SalesOfferObj.ClientVehicleId = VehicleMaintenanceJobOrderHistories?.Where(x => x.SalesOfferId == SalesOfferDb.Id).Select(x => x.VehiclePerClientId).FirstOrDefault();
                    SalesOfferObj.ClientVehicleName = VehicleMaintenanceJobOrderHistories?.Where(x => x.SalesOfferId == SalesOfferDb.Id).FirstOrDefault()?.VehiclePerClient?.PlatNumber;
                    SalesOfferObj.VehicleChassisNumber = VehicleMaintenanceJobOrderHistories?.Where(x => x.SalesOfferId == SalesOfferDb.Id).FirstOrDefault()?.VehiclePerClient?.ChassisNumber;
                    SalesOfferObj.VehicleModelName = VehicleMaintenanceJobOrderHistories?.Where(x => x.SalesOfferId == SalesOfferDb.Id).FirstOrDefault()?.VehiclePerClient?.Model?.Name;
                    var ClientAddresses = _unitOfWork.VClientAddresses.FindAll(a => a.ClientId == (long)SalesOfferDb.ClientId && a.Active == true).ToList();
                    if (ClientAddresses != null && ClientAddresses.Count > 0)
                    {
                        SalesOfferObj.ClientAddress = ClientAddresses.Select(address => new GetClientAddress
                        {
                            ID = address.Id,
                            Address = address.Address,
                            AreaID = address.AreaId,
                            AreaName = address.Area,
                            BuildingNumber = address.BuildingNumber,
                            CountryID = address.CountryId,
                            CountryName = address.Country,
                            Description = address.Description,
                            Floor = address.Floor,
                            GovernorateID = address.GovernorateId,
                            GovernorateName = address.Governorate
                        }).ToList();
                    }
                    SalesOfferObj.ClientTaxCard = ClientDb.TaxCard;
                    SalesOfferObj.ClientName = ClientDb.Name;
                }
                SalesOfferObj.ProductType = SalesOfferDb.ProductType;
                SalesOfferObj.ProjectName = SalesOfferDb.ProjectName;
                SalesOfferObj.ProjectLocation = SalesOfferDb.ProjectLocation;
                SalesOfferObj.ContactPersonMobile = SalesOfferDb.ContactPersonMobile;
                SalesOfferObj.ContactPersonEmail = SalesOfferDb.ContactPersonEmail;
                SalesOfferObj.ContactPersonName = SalesOfferDb.ContactPersonName;
                SalesOfferObj.ProjectStartDate = SalesOfferDb.ProjectStartDate != null ? SalesOfferDb.ProjectStartDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.ProjectEndDate = SalesOfferDb.ProjectEndDate != null ? SalesOfferDb.ProjectEndDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.BranchId = SalesOfferDb.BranchId;
                SalesOfferObj.BranchName = Common.GetBranchName(SalesOfferDb.BranchId, _Context);
                SalesOfferObj.OfferType = SalesOfferDb.OfferType;
                SalesOfferObj.ContractType = SalesOfferDb.ContractType;
                SalesOfferObj.OfferSerial = SalesOfferDb.OfferSerial;
                SalesOfferObj.ClientNeedsDiscount = SalesOfferDb.ClientNeedsDiscount;
                SalesOfferObj.RejectionReason = SalesOfferDb.RejectionReason;
                SalesOfferObj.NeedsInvoice = SalesOfferDb.NeedsInvoice;
                SalesOfferObj.NeedsExtraCost = SalesOfferDb.NeedsExtraCost;
                SalesOfferObj.OfferExpirationDate = SalesOfferDb.OfferExpirationDate != null ? SalesOfferDb.OfferExpirationDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.OfferExpirationPeriod = SalesOfferDb.OfferExpirationPeriod;
                SalesOfferObj.ExtraOrDiscountPriceBySalesPerson = SalesOfferDb.ExtraOrDiscountPriceBySalesPerson;
                SalesOfferObj.FinalOfferPrice = SalesOfferDb.FinalOfferPrice;
                SalesOfferObj.TotalExtraCostAmount = totalExtraCostAmount ?? 0;
                SalesOfferObj.TotalDiscountAmount = totalDiscountAmount ?? 0;
                SalesOfferObj.TotalTaxAmount = totalTaxAmount ?? 0;
                SalesOfferObj.ReminderDate = SalesOfferDb.ReminderDate != null ? SalesOfferDb.ReminderDate.ToString().Split(' ')[0] : null;
                SalesOfferObj.CreationDate = SalesOfferDb.CreationDate.ToShortDateString();
                SalesOfferObj.CreationTime = SalesOfferDb.CreationDate.ToShortTimeString();
                //var ListChildrenSalesOffer = _unitOfWork.sales.Where(x => x.ParentSalesOfferId == offer.Id).Select(x => new ChildrenSalesOFfer { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial }).ToList();


            }
            if (SalesOfferObj.Status != null)
            {
                if (SalesOfferObj.Status.ToLower() == "closed")
                {
                    var project = _Context.Projects.Where(a => a.SalesOfferId == SalesOfferObj.Id).FirstOrDefault();
                    if (project != null)
                    {
                        var ProjectStatus = "";

                        if (project.Active)
                            if (project.Closed)
                                ProjectStatus = "Closed";
                            else
                                ProjectStatus = "Open";
                        else
                            ProjectStatus = "Deactivated";


                        SalesOfferObj.ProjectStatus = ProjectStatus;
                        SalesOfferObj.ProjectSerial = project.ProjectSerial;

                        var ProjectExtraModifications = _unitOfWork.InvoiceExtraModifications.FindAll(a => a.ProjectId == project.Id).ToList();
                        if (ProjectExtraModifications != null && ProjectExtraModifications.Count > 0)
                        {
                            SalesOfferObj.ProjectExtraModifications = ProjectExtraModifications.Sum(a => a.ApprovalPrice);
                        }
                    }
                }
            }

            var OfferClientAccount = _unitOfWork.ClientAccounts.FindAll(a => a.OfferId == SalesOfferObj.Id).FirstOrDefault();
            if (OfferClientAccount != null)
            {
                SalesOfferObj.HasJournalEntryId = OfferClientAccount.DailyAdjustingEntryId;
            }

            return SalesOfferObj;
        }

        public List<Attachment> GetSalesOfferAttachmentsList(long SalesOfferId)
        {
            var SalesOfferAttachments = new List<Attachment>();
            var SalesOfferAttachmentsDb = _unitOfWork.SalesOfferAttachments.FindAll(a => a.OfferId == SalesOfferId && a.Active == true).ToList();
            if (SalesOfferAttachmentsDb.Count > 0)
            {
                SalesOfferAttachments = SalesOfferAttachmentsDb.Select(attachment => new Attachment
                {
                    Id = attachment.Id,
                    FilePath = attachment.AttachmentPath == null ? null : Globals.baseURL + attachment.AttachmentPath.TrimStart('~'),
                    FileExtension = attachment.FileExtenssion ?? null,
                    FileName = attachment.FileName ?? null,
                    Category = attachment.Category ?? null
                }).ToList();
            }

            return SalesOfferAttachments;
        }

        public List<GetSalesOfferProduct> GetSalesOfferProductsList(long SalesOfferId)
        {
            var SalesOfferProducts = new List<GetSalesOfferProduct>();
            var SalesOfferProductsDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == SalesOfferId && a.Active == true, new[] { "InventoryItem", "InvoicePayerClient" }).ToList();
            if (SalesOfferProductsDb.Count > 0)
            {
                var SalesOfferProductsIds = SalesOfferProductsDb.Select(a => a.Id).ToList();
                var SalesOfferProductsTaxes = _unitOfWork.SalesOfferProductTaxes.FindAll(a => SalesOfferProductsIds.Contains(a.SalesOfferProductId), includes: new[] { "Tax" }).ToList();
                SalesOfferProducts = SalesOfferProductsDb.Select(product => new GetSalesOfferProduct
                {
                    Id = product.Id,
                    ConfirmReceivingComment = product.ConfirmReceivingComment ?? null,
                    ConfirmReceivingQuantity = product.ConfirmReceivingQuantity ?? null,
                    DiscountPercentage = product.DiscountPercentage ?? null,
                    DiscountValue = product.DiscountValue ?? null,
                    FinalPrice = product.FinalPrice ?? null,
                    FinalPriceWithoutTax = (product.ItemPrice ?? 0) * (decimal)(product.Quantity ?? 0.0),
                    TotalTaxValue = SalesOfferProductsTaxes.Where(a => a.SalesOfferProductId == product.Id).Count() > 0 ? SalesOfferProductsTaxes.Where(a => a.SalesOfferProductId == product.Id && a.Value != null).Select(x => x.Value).Sum() : 0,
                    InventoryItemCategoryId = product.InventoryItemCategoryId ?? null,
                    InventoryItemCategoryName = product.InventoryItemCategoryId != null ? _unitOfWork.InventoryItemCategories.GetById((int)product.InventoryItemCategoryId)?.Name : null,
                    InventoryItemId = product.InventoryItemId ?? null,
                    InventoryItemName = product.InventoryItemId != null ? _unitOfWork.InventoryItems.GetById((long)product.InventoryItemId)?.Name : null,
                    InventoryItemCode = product.InventoryItemId != null ? _unitOfWork.InventoryItems.GetById((long)product.InventoryItemId)?.Code : null,
                    InventoryItemPartNO = product.InventoryItemId != null ? _unitOfWork.InventoryItems.GetById((long)product.InventoryItemId)?.PartNo : null,
                    InventoryItemUOM = product.InventoryItemId != null ? _unitOfWork.VInventoryItems.FindAll(a => a.Id == (long)product.InventoryItemId).FirstOrDefault()?.RequestionUomshortName : null,
                    InvoicePayerClientId = product.InvoicePayerClientId ?? null,
                    InvoicePayerClientName = product.InvoicePayerClientId != null ? product.InvoicePayerClient.Name : null,
                    ItemPrice = product.ItemPrice ?? null,
                    ItemPricingComment = product.ItemPricingComment ?? null,
                    OfferId = product.OfferId,
                    ProductGroupId = product.ProductGroupId ?? null,
                    ProductId = product.ProductId ?? null,
                    Quantity = product.Quantity ?? null,
                    ProfitPercentage = product.ProfitPercentage.ToString(),
                    CommercialName = product.InventoryItem?.CommercialName,
                    ReleasedQty = product.ReleasedQty,
                    ReleasedPercentage = product.ReleasedQty != null && product.RemainQty != null && product.RemainQty > 0 ? (product.ReleasedQty / product.RemainQty * 100).ToString() + "%" : product.RemainQty != null && product.RemainQty == 0 && product.ReleasedQty != null && product.ReleasedQty > 0 ? "Need Return" : null,
                    RemainQty = product.RemainQty,
                    ReturnedQty = product.ReturnedQty,
                    TaxPercentage = product.TaxPercentage ?? null,
                    TaxValue = product.TaxValue ?? null,
                    SalesOfferProductAttachments = _unitOfWork.SalesOfferItemAttachments.FindAll(a => a.SalesOfferProductId == product.Id).Count() > 0 ?
                                                        _Context.SalesOfferItemAttachments.Where(a => a.SalesOfferProductId == product.Id).ToList().Select(attach => new Attachment
                                                        {
                                                            Id = attach.Id,
                                                            FilePath = attach.AttachmentPath ?? null,
                                                            FileExtension = attach.FileExtenssion ?? null,
                                                            FileName = attach.FileName ?? null,
                                                            Category = attach.Category
                                                        }).ToList() : null,
                    SalesOfferProductTaxsList = SalesOfferProductsTaxes.Where(a => a.SalesOfferProductId == product.Id).Count() > 0 ? SalesOfferProductsTaxes.Where(a => a.SalesOfferProductId == product.Id).Select(productTax => new GetSalesOfferProductTax
                    {
                        ID = productTax.Id,
                        Percentage = productTax.Percentage,
                        TaxID = productTax.TaxId,
                        Value = productTax.Value,
                        TaxName = productTax.Tax.TaxName,
                        TaxCode = productTax.Tax.TaxCode,
                        ParentTax = productTax.Tax != null ? productTax.Tax.TaxType != null ? _unitOfWork.Taxes.FindAll(a => a.TaxCode == productTax.Tax.TaxType).FirstOrDefault() != null ? _Context.Taxes.Where(a => a.TaxCode == productTax.Tax.TaxType).Select(parentTax => new GetTax
                        {
                            Id = parentTax.Id,
                            TaxPercentage = parentTax.TaxPercentage,
                            TaxCode = parentTax.TaxCode,
                            Description = parentTax.Description,
                            TaxName = parentTax.TaxName
                        }).FirstOrDefault() : null : null : null
                        ,
                        // Modifiecation 3la el sare3
                        ParentTaxID = productTax.Tax != null ? productTax.Tax.TaxType != null ? _Context.Taxes.Where(a => a.TaxCode == productTax.Tax.TaxType).FirstOrDefault() != null ? _Context.Taxes.Where(a => a.TaxCode == productTax.Tax.TaxType).Select(x => x.Id).FirstOrDefault() : 0 : 0 : 0
                    }).ToList() : null
                }).ToList();
            }

            return SalesOfferProducts;
        }

        public List<GetTax> GetSalesOfferTaxList(long SalesOfferId)
        {
            var SalesOfferTaxes = new List<GetTax>();
            var SalesOfferTaxesDb = _unitOfWork.SalesOfferInvoiceTaxes.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true, includes: new[] { "InvoicePayerClient" }).ToList();
            if (SalesOfferTaxesDb.Count > 0)
            {
                SalesOfferTaxes = SalesOfferTaxesDb.Select(tax => new GetTax
                {
                    Id = tax.Id,
                    InvoicePayerClientId = tax.InvoicePayerClientId,
                    InvoicePayerClientName = tax.InvoicePayerClientId != null ? tax.InvoicePayerClient.Name : null,
                    TaxName = tax.TaxName,
                    TaxPercentage = tax.TaxPercentage,
                    TaxValue = tax.TaxValue
                }).ToList();
            }

            return SalesOfferTaxes;
        }

        public List<GetSalesOfferDiscount> GetSalesOfferDiscountList(long SalesOfferId)
        {
            var SalesOfferDoscounts = new List<GetSalesOfferDiscount>();
            var SalesOfferDiscountsDb = _unitOfWork.SalesOfferDiscounts.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true).ToList();
            if (SalesOfferDiscountsDb.Count > 0)
            {
                SalesOfferDoscounts = SalesOfferDiscountsDb.Select(discount => new GetSalesOfferDiscount
                {
                    Id = discount.Id,
                    InvoicePayerClientId = discount.InvoicePayerClientId,
                    InvoicePayerClientName = discount.InvoicePayerClientId != null ? discount.InvoicePayerClient.Name : null,
                    DiscountApproved = discount.DiscountApproved,
                    ClientApproveDiscount = discount.ClientApproveDiscount,
                    DiscountApprovedBy = discount.DiscountApprovedBy,
                    DiscountApprovedByName = discount.DiscountApprovedBy != null ? Common.GetUserName((long)discount.DiscountApprovedBy, _Context) : null,
                    DiscountPercentage = discount.DiscountPercentage,
                    DiscountValue = discount.DiscountValue
                }).ToList();
            }

            return SalesOfferDoscounts;
        }


        public List<ExtraCost> GetSalesOfferExtraCostList(long SalesOfferId)
        {
            var SalesOfferExtraCosts = new List<ExtraCost>();
            var SalesOfferExtraCostsDb = _unitOfWork.SalesOfferExtraCosts.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true).ToList();
            if (SalesOfferExtraCostsDb.Count > 0)
            {
                SalesOfferExtraCosts = SalesOfferExtraCostsDb.Select(extraCost => new ExtraCost
                {
                    Id = extraCost.Id,
                    InvoicePayerClientId = extraCost.InvoicePayerClientId,
                    InvoicePayerClientName = extraCost.InvoicePayerClientId != null ? extraCost.InvoicePayerClient.Name : null,
                    Amount = extraCost.Amount
                }).ToList();
            }

            return SalesOfferExtraCosts;
        }

        public GetSalesOfferDetailsResponse GetSalesOfferDetails(long SalesOfferId)
        {
            GetSalesOfferDetailsResponse Response = new GetSalesOfferDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {

                    if (Response.Result)
                    {
                        var SalesOffer = GetSalesOfferInfo(SalesOfferId);
                        if (SalesOffer.Id != null)
                        {
                            Response.SalesOfferDetails = SalesOffer;
                            Response.SalesOfferAttachments = GetSalesOfferAttachmentsList(SalesOfferId);
                            Response.SalesOfferProducts = GetSalesOfferProductsList(SalesOfferId);
                            Response.SalesOfferTaxes = GetSalesOfferTaxList(SalesOfferId);
                            Response.SalesOfferDiscounts = GetSalesOfferDiscountList(SalesOfferId);
                            Response.SalesOfferExtraCosts = GetSalesOfferExtraCostList(SalesOfferId);
                            Response.SalesOfferInvoices = GetSalesOfferInvoicesList(SalesOfferId);
                            Response.TotalSalesOfferInvoicesAmount = Response.SalesOfferInvoices.Count > 0 ? Response.SalesOfferInvoices.Where(a => a.SalesOfferId == SalesOfferId).Sum(a => a.TotalInvoiceAmount ?? 0) : 0;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err99";
                            error.ErrorMSG = "This Sales Offer Doesn't Exist!!";
                            Response.Errors.Add(error);
                            return Response;
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


        public GetSalesOfferProductsDetailsResponse GetSalesOfferProductsDetails(long SalesOfferId)
        {
            GetSalesOfferProductsDetailsResponse Response = new GetSalesOfferProductsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    Response.SalesOfferProducts = GetSalesOfferProductsList(SalesOfferId);

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


        public GetOfferExtraCostTypesListResponse GetOfferExtraCostTypesList()
        {
            GetOfferExtraCostTypesListResponse Response = new GetOfferExtraCostTypesListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var ExtraCostTypesList = _unitOfWork.SalesExtraCostTypes.GetAll();
                    var ExtraCostDDL = new List<SelectDDL>();
                    foreach (var Ec in ExtraCostTypesList)
                    {
                        var DLLObj = new SelectDDL();
                        DLLObj.ID = Ec.Id;
                        DLLObj.Name = Ec.Name;

                        ExtraCostDDL.Add(DLLObj);
                    }
                    Response.ExtraCostTypesList = ExtraCostDDL;
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

        public GetOfferTaxListResponse GetOfferTaxTypesList()
        {
            GetOfferTaxListResponse Response = new GetOfferTaxListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    var TaxDBList = _Context.Taxes.Where(x => x.Active == true && (x.TaxType.Contains("Taxable Types") || x.TaxType.Contains("Non-Taxable Types"))).ToList();
                    var TaxList = new List<GetTax>();
                    foreach (var taxParent in TaxDBList)
                    {
                        var TaxChildList = new List<GetTax>();
                        var TaxChildListDB = _Context.Taxes.Where(x => x.Active == true && x.TaxType == taxParent.TaxCode).ToList();
                        foreach (var taxChild in TaxChildListDB)
                        {
                            var TaxChildModel = new GetTax();
                            TaxChildModel.Id = taxChild.Id;
                            TaxChildModel.TaxCode = taxChild.TaxCode;
                            TaxChildModel.TaxType = taxParent.TaxType; // Tax type for parent
                            TaxChildModel.TaxName = taxChild.TaxName;
                            TaxChildModel.TaxPercentage = taxChild.TaxPercentage;
                            TaxChildModel.Description = taxChild.Description;
                            TaxChildModel.IsPercentage = taxChild.IsPercentage;
                            TaxChildList.Add(TaxChildModel);
                        }
                        var TaxObj = new GetTax()
                        {
                            Id = taxParent.Id,
                            TaxCode = taxParent.TaxCode,
                            TaxName = taxParent.TaxName,
                            TaxPercentage = taxParent.TaxPercentage,
                            Description = taxParent.Description,
                            TaxType = taxParent.TaxType,
                            IsPercentage = taxParent.IsPercentage,
                            TaxChildList = TaxChildList
                        };
                        TaxList.Add(TaxObj);

                    }
                    Response.TaxList = TaxList;
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

        public GetOfferTermsAndConditionsListResponse GetOfferTermsAndConditionsList()
        {
            GetOfferTermsAndConditionsListResponse Response = new GetOfferTermsAndConditionsListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var TermsAndConditionsCategoryList = _unitOfWork.TermsAndConditionsCategories.GetAll();
                    var TermsAndConditionCategoryDDL = new List<GetTermsAndConditionsCategory>();
                    foreach (var termAndConditionCategory in TermsAndConditionsCategoryList)
                    {
                        var DLLObj = new GetTermsAndConditionsCategory()
                        {
                            Id = termAndConditionCategory.Id,
                            Name = termAndConditionCategory.Name
                        };

                        var TermsAndConditionsList = _unitOfWork.TermsAndConditions.GetAll();
                        var TermsAndConditionDDL = new List<GetTermsAndConditions>();
                        foreach (var termAndCondition in TermsAndConditionsList)
                        {
                            var TermAndConditionObj = new GetTermsAndConditions()
                            {
                                Id = termAndCondition.Id,
                                Name = termAndCondition.Name,
                                Description = termAndCondition.Description,
                                TermsCategoryId = termAndCondition.TermsCategoryId
                            };
                            TermsAndConditionDDL.Add(TermAndConditionObj);
                        }
                        DLLObj.TermsAndConditionsList = TermsAndConditionDDL;

                        TermsAndConditionCategoryDDL.Add(DLLObj);
                    }
                    Response.TermsAndConditionsList = TermsAndConditionCategoryDDL;
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

        public async Task<GetSalesOfferListDDLResponse> GetSalesOfferListDDLData(long ClientId, string SearchKey, bool? StatusIsOpenFilter)
        {
            GetSalesOfferListDDLResponse Response = new GetSalesOfferListDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SalesOfferList = new List<SelectSalesOfferDDL>();

                if (Response.Result)
                {
                    var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true).AsQueryable();

                    // Client Name , Offer Serial ,Project Name
                    if (!string.IsNullOrEmpty(SearchKey))
                    {
                        SearchKey = HttpUtility.UrlDecode(SearchKey);
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
                                                   (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(SearchKey.ToLower()) : false))
                                                //|| SalesOfferIDS.Contains(x.ID)
                                                ).AsQueryable();
                    }
                    if (ClientId != 0)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientId == ClientId);
                    }
                    if (StatusIsOpenFilter != null)
                    {
                        if (StatusIsOpenFilter == true)//  ( Pricing, Recieved, ClientApproval )
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status != "Closed" && x.Status != "Rejected");
                        }
                        else
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status == "Closed" || x.Status != "Rejected");
                        }
                    }
                    SalesOfferList = await SalesOfferDBQuery.Select(x => new SelectSalesOfferDDL
                    {
                        ID = x.Id,
                        ProjectName = x.ProjectName,
                        OfferSerial = x.OfferSerial,
                        ProjectSerial = x.Projects.FirstOrDefault() != null
                        ? x.Projects.FirstOrDefault().ProjectSerial : null,

                    }).ToListAsync();

                    Response.Data = SalesOfferList;
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

        public GetSalesOfferProductListDDLResponse GetSalesOfferProductListDDLData(long SalesOfferId)
        {
            GetSalesOfferProductListDDLResponse Response = new GetSalesOfferProductListDDLResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (SalesOfferId == 0)
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err11";
                    error.ErrorMSG = "Invalid Sales offer Id";
                    Response.Errors.Add(error);
                    return Response;
                }

                var SalesOfferList = new List<SalesOfferProductDDL>();
                var SalesOfferProductsDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.OfferId == SalesOfferId && a.Active == true, includes: new[] { "InventoryItem" }).ToList();
                if (SalesOfferProductsDb.Count > 0)
                {
                    SalesOfferList = SalesOfferProductsDb.Select(product => new SalesOfferProductDDL
                    {
                        Id = product.Id,
                        InventoryItemName = product.InventoryItemId != null ? product.InventoryItem?.Name : null
                    }).ToList();
                }
                Response.SalesOfferProducts = SalesOfferList;

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

        /*public GetSalesOfferStatisticsPerOfferStatusResponse GetSalesOfferStatisticsPerOfferStatus(SalesOfferStatisticsFilters filters)
        {
            GetSalesOfferStatisticsPerOfferStatusResponse Response = new GetSalesOfferStatisticsPerOfferStatusResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                   *//* string OfferType = "";
                    if (!string.IsNullOrEmpty(headers["OfferType"]))
                    {
                        OfferType = headers["OfferType"].ToLower();
                    }*/

        /*string SupportedBy = "";
        if (!string.IsNullOrEmpty(headers["SupportedBy"]))
        {
            SupportedBy = HttpUtility.UrlDecode(headers["SupportedBy"]).ToLower();
        }*//*

        List<long> ProductsIdsList = new List<long>();
        if (!string.IsNullOrEmpty(filters.ProductsListString))
        {
            ProductsIdsList = filters.ProductsListString.Split(',').Select(s => long.Parse(s.Trim())).ToList();
        }

        if (!string.IsNullOrEmpty(filters.ReleaseFilterString))
        {
            if (filters.ReleaseFilterString == "fully")
            {
                var FullyReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty != null && a.RemainQty != null && a.ReleasedQty >= a.RemainQty).Select(a => a.Id).ToList();
                if (FullyReleasedProductsId.Count > 0)
                {
                    ProductsIdsList.AddRange(FullyReleasedProductsId);
                }
            }
            else if (filters.ReleaseFilterString == "partially")
            {
                var PartiallyReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty != null && a.RemainQty != null && a.ReleasedQty > 0 && a.ReleasedQty < a.RemainQty).Select(a => a.Id).ToList();
                if (PartiallyReleasedProductsId.Count > 0)
                {
                    ProductsIdsList.AddRange(PartiallyReleasedProductsId);
                }
            }
            else if (filters.ReleaseFilterString == "not")
            {
                var NotReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty == null || a.ReleasedQty == 0).Select(a => a.Id).ToList();
                if (NotReleasedProductsId.Count > 0)
                {
                    ProductsIdsList.AddRange(NotReleasedProductsId);
                }
            }
            else
            {
                Error error = new Error();
                error.ErrorCode = "Err-12";
                error.ErrorMSG = "Invalid Release Filter!!";
                Response.Errors.Add(error);
                Response.Result = false;
            }
        }

        *//*string ProductType = ""; //(Standard, Special)
        if (!string.IsNullOrEmpty(headers["ProductType"]))
        {
            ProductType = headers["ProductType"].ToLower();
        }*/

        /*string ClientName = "";
        if (!string.IsNullOrEmpty(headers["ClientName"]))
        {
            ClientName = HttpUtility.UrlDecode(headers["ClientName"]).ToLower();
        }*/

        /*string ProjectName = "";
        if (!string.IsNullOrEmpty(headers["ProjectName"]))
        {
            ProjectName = HttpUtility.UrlDecode(headers["ProjectName"]).ToLower();
        }*//*
        string OfferStatus = "";
        if (!string.IsNullOrEmpty(filters.OfferStatus))
        {
            filters.OfferStatus = filters.OfferStatus.ToLower();

        }

        string ReminderDateFilterString = "";
        if (!string.IsNullOrEmpty(headers["ReminderDateFilter"]))
        {
            if (OfferStatus != "" && OfferStatus.ToLower() == "clientapproval")
            {
                ReminderDateFilterString = headers["ReminderDateFilter"].ToLower();
                if (ReminderDateFilterString != "delay" && ReminderDateFilterString != "today")
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-12";
                    error.ErrorMSG = "Invalid ReminderDate Filter!!";
                    Response.Errors.Add(error);
                    Response.Result = false;
                }
            }
        }

        var StartDate = DateTime.Now;
        var EndDate = DateTime.Now;
        var DateFilter = false;

        if (!string.IsNullOrEmpty(headers["From"]))
        {
            DateFilter = true;
            DateTime From = DateTime.Now;
            if (!DateTime.TryParse(headers["From"], out From))
            {
                Error error = new Error();
                error.ErrorCode = "Err-12";
                error.ErrorMSG = "Invalid Offer Creation From";
                Response.Errors.Add(error);
                Response.Result = false;
                return Response;
            }
            StartDate = From;

            if (!string.IsNullOrEmpty(headers["To"]))
            {
                DateTime To = DateTime.Now;
                if (!DateTime.TryParse(headers["To"], out To))
                {
                    Error error = new Error();
                    error.ErrorCode = "Err-13";
                    error.ErrorMSG = "Invalid Offer Creation To";
                    Response.Errors.Add(error);
                    Response.Result = false;
                    return Response;
                }
                EndDate = To;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(headers["To"]))
            {
                Error error = new Error();
                error.ErrorCode = "Err-13";
                error.ErrorMSG = "You have to Enter Offer From Date!";
                Response.Errors.Add(error);
                Response.Result = false;
                return Response;
            }
        }

        long SalesPersonId = 0;
        if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
        {
            long.TryParse(headers["SalesPersonId"], out SalesPersonId);
        }

        int BranchId = 0;
        if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
        {
            int.TryParse(headers["BranchId"], out BranchId);
        }

        int CurrentPage = 1;
        if (!string.IsNullOrEmpty(headers["CurrentPage"]) && int.TryParse(headers["CurrentPage"], out CurrentPage))
        {
            int.TryParse(headers["CurrentPage"], out CurrentPage);
        }

        int NumberOfItemsPerPage = 10;
        if (!string.IsNullOrEmpty(headers["NumberOfItemsPerPage"]) && int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage))
        {
            int.TryParse(headers["NumberOfItemsPerPage"], out NumberOfItemsPerPage);
        }
        string SearchKey = headers["SearchKey"];
        if (headers["SearchKey"] != null && headers["SearchKey"] != "")
        {
            SearchKey = headers["SearchKey"];
        }


        int StoreId = 0;
        if (!string.IsNullOrEmpty(headers["StoreId"]) && int.TryParse(headers["StoreId"], out StoreId))
        {
            int.TryParse(headers["StoreId"], out StoreId);
        }
        if (Response.Result)
        {
            var SalesOfferDBQuery = _Context.SalesOffers.Where(a => a.Active == true).AsQueryable();
            if (StoreId != 0)
            {
                var InventoryStoreItemSalesofferIDsList = _Context.InventoryStoreItems.Where(x => x.InventoryStoreID == StoreId && x.OperationType.Contains("SalesOffer")).Select(x => x.OrderID).ToList();

                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => InventoryStoreItemSalesofferIDsList.Contains(x.ID)).AsQueryable();
            }
            bool HasInvoice = false;
            if (headers["HasInvoice"] != null && headers["HasInvoice"] != "")
            {
                if (!string.IsNullOrEmpty(headers["HasInvoice"]) && bool.TryParse(headers["HasInvoice"], out HasInvoice))
                {
                    bool.TryParse(headers["HasInvoice"], out HasInvoice);
                    if (HasInvoice)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Invoices.Count > 0);
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.Invoices.Count > 0));
                    }
                }
            }
            DateTime InvoiceDate = DateTime.Now;
            if (headers["InvoiceDate"] != null && headers["InvoiceDate"] != "")
            {
                if (!string.IsNullOrEmpty(headers["InvoiceDate"]) && DateTime.TryParse(headers["InvoiceDate"], out InvoiceDate))
                {
                    DateTime.TryParse(headers["InvoiceDate"], out InvoiceDate);
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Invoices.Count > 0 && x.Invoices.Any((a => DbFunctions.TruncateTime(a.CreationDate) == InvoiceDate.Date)));
                }
            }

            bool HasProject = false;
            if (headers["HasProject"] != null && headers["HasProject"] != "")
            {
                if (!string.IsNullOrEmpty(headers["HasProject"]) && bool.TryParse(headers["HasProject"], out HasProject))
                {
                    bool.TryParse(headers["HasProject"], out HasProject);
                    if (HasProject)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Projects.Count > 0);
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.Projects.Count > 0));
                    }
                }
            }
            DateTime ProjectDate = DateTime.Now;
            if (headers["ProjectDate"] != null && headers["ProjectDate"] != "")
            {
                if (!string.IsNullOrEmpty(headers["ProjectDate"]) && DateTime.TryParse(headers["ProjectDate"], out ProjectDate))
                {
                    DateTime.TryParse(headers["ProjectDate"], out ProjectDate);
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Projects.Count > 0 && x.Projects.Any(a => DbFunctions.TruncateTime(a.CreationDate) == ProjectDate.Date));
                }
            }
            bool HasAutoJE = false;
            if (headers["HasAutoJE"] != null && headers["HasAutoJE"] != "")
            {
                if (!string.IsNullOrEmpty(headers["HasAutoJE"]) && bool.TryParse(headers["HasAutoJE"], out HasAutoJE))
                {
                    bool.TryParse(headers["HasAutoJE"], out HasAutoJE);
                    if (HasAutoJE)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.OfferID != null));
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.OfferID != null)));
                    }
                }
            }
            bool HasJournalEntry = false;
            if (headers["HasJournalEntry"] != null && headers["HasJournalEntry"] != "")
            {
                if (!string.IsNullOrEmpty(headers["HasJournalEntry"]) && bool.TryParse(headers["HasJournalEntry"], out HasJournalEntry))
                {
                    bool.TryParse(headers["HasJournalEntry"], out HasJournalEntry);
                    if (HasJournalEntry)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectID != null));
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectID == null));
                    }
                }
            }
            DateTime JournalEntryDate = DateTime.Now;
            if (headers["JournalEntryDate"] != null && headers["JournalEntryDate"] != "")
            {
                if (!string.IsNullOrEmpty(headers["JournalEntryDate"]) && DateTime.TryParse(headers["JournalEntryDate"], out JournalEntryDate))
                {
                    DateTime.TryParse(headers["JournalEntryDate"], out JournalEntryDate);
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectID != null && DbFunctions.TruncateTime(a.CreationDate) == JournalEntryDate.Date));
                }
            }


            // supplier Name , Offer Serial ,Project Name
            if (!string.IsNullOrEmpty(SearchKey))
            {
                SearchKey = HttpUtility.UrlDecode(SearchKey);
                SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
                                           (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(SearchKey.ToLower()) : false)
                                        || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(SearchKey.ToLower()) : false)
                                        || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(SearchKey.ToLower()) : false)
                                        || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(SearchKey.ToLower()) : false))
                                        //|| SalesOfferIDS.Contains(x.ID)
                                        ).AsQueryable();
            }
            if (!string.IsNullOrEmpty(OfferType))
            {
                if (OfferType == "new project offer")
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == "new project offer" || a.OfferType.ToLower() == "direct sales").AsQueryable();
                else
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == OfferType).AsQueryable();
            }
            if (!string.IsNullOrEmpty(SupportedBy))
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.SupportedBy.ToLower() == SupportedBy).AsQueryable();
            }
            if (!string.IsNullOrEmpty(OfferStatus))
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Status.ToLower() == OfferStatus).AsQueryable();

                if (ReminderDateFilterString != "")
                {
                    DateTime TodayDate = DateTime.Now.Date;
                    if (ReminderDateFilterString == "today")
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ReminderDate == TodayDate).AsQueryable();
                        SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ReminderDate);
                    }
                    else if (ReminderDateFilterString == "delay")
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ReminderDate < TodayDate).AsQueryable();
                    }
                }
            }
            if (SalesPersonId != 0)
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonID == SalesPersonId).AsQueryable();
            }
            if (BranchId != 0)
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchID == BranchId).AsQueryable();
            }
            if (!string.IsNullOrEmpty(ProductType))
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProductType.ToLower() == ProductType).AsQueryable();
            }
            if (!string.IsNullOrEmpty(ClientName))
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(ClientName)).AsQueryable();
            }
            if (!string.IsNullOrEmpty(ProjectName))
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(ProjectName)).AsQueryable();
            }
            var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
            if (DateFilter)
            {
                if (OfferStatus == "closed" || OfferStatus == "rejected")
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= StartDate && a.ClientApprovalDate <= EndDate).AsQueryable();
                }
                else
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= StartDate && a.EndDate <= EndDate).AsQueryable();
                }
            }
            else
            {
                if (OfferStatus == "closed" || OfferStatus == "rejected")
                {
                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                }
            }
            if (ProductsIdsList.Count > 0)
            {
                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesOfferProducts.Any(x => ProductsIdsList.Contains(x.InventoryItemID ?? 0))).AsQueryable();
            }

            if (OfferStatus == "closed" || OfferStatus == "rejected")
            {
                SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate);
            }
            else
            {
                SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
            }







            SalesOfferStatisticsPerOfferStatus salesOfferPerStatus = new SalesOfferStatisticsPerOfferStatus();
            var SalesOfferDB = _Context.SalesOffers.Where(a => a.Active == true).AsQueryable();

            var SalesOfferDBPerPricing = SalesOfferDBQuery.Where(a => a.Status == "Pricing").AsQueryable();
            salesOfferPerStatus.TotalCountForPricingStatus = SalesOfferDBPerPricing.Count();
            salesOfferPerStatus.TotalCostForPricingStatus = SalesOfferDBPerPricing.Sum(x => x.FinalOfferPrice);


            var SalesOfferDBPerRecieved = SalesOfferDBQuery.Where(a => a.Status == "Recieved").AsQueryable();
            salesOfferPerStatus.TotalCountForRecievedStatus = SalesOfferDBPerRecieved.Count();
            salesOfferPerStatus.TotalCostForRecievedStatus = SalesOfferDBPerRecieved.Sum(x => x.FinalOfferPrice);


            var SalesOfferDBPerClientApproval = SalesOfferDBQuery.Where(a => a.Status == "ClientApproval").AsQueryable();
            salesOfferPerStatus.TotalCountForClientApprovalStatus = SalesOfferDBPerClientApproval.Count();
            salesOfferPerStatus.TotalCostForClientApprovalStatus = SalesOfferDBPerClientApproval.Sum(x => x.FinalOfferPrice);


            var SalesOfferDBPerClosed = SalesOfferDBQuery.Where(a => a.Status == "Closed" && a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
            salesOfferPerStatus.TotalCountForClosedStatus = SalesOfferDBPerClosed.Count();
            //var test = SalesOfferDBPerClosed.ToList();
            //var testreturn = SalesOfferDBPerClosed.Where(a => a.OfferType == "Sales Return").ToList();
            var SalesOfferSUM = SalesOfferDBPerClosed.Where(a => a.OfferType != "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
            var SalesOfferReturnSUM = SalesOfferDBPerClosed.Where(a => a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
            salesOfferPerStatus.TotalCostForClosedStatus = SalesOfferSUM - SalesOfferReturnSUM;

            var SalesOfferDBPerRejected = SalesOfferDBQuery.Where(a => a.Status == "Rejected" && a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
            salesOfferPerStatus.TotalCountForRejectedStatus = SalesOfferDBPerRejected.Count();
            salesOfferPerStatus.TotalCostForRejectedStatus = SalesOfferDBPerRejected.Sum(x => x.FinalOfferPrice);


            Response.Data = salesOfferPerStatus;
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
}*/

        /*Test()
        {
            addTax() async {
                if (taxNameController.text == "T1")
                {
                    await calculationTaxOne();
                }
                else if (taxNameController.text == "T2")
                {
                    await calculationTaxTwo();
                }
                else
                {
                    await calculationDefaultTax();
                }

                await saveExtraTaxToBill();
                await updateTaxesList();
                await clearExtraTaxParameters();

                notifyListeners();
            }


            calculationTaxOne() {
                double totalTaxTaxableTypeAmount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxTaxTaxableTypes == "true" &&
                            element.taxType != "T1" &&
                            element.taxType != "T2" &&
                            element.taxType != "T3" &&
                            element.taxType != "T4"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);
                double totalTaxT2Amount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxType == "T2"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);
                double totalTaxT3Amount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxType == "T3"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);

                if (unitPriceBeforeDiscountController.text.isNotEmpty &&
                    taxPercentageController.text.isNotEmpty)
                {
                    taxAmountController.text =
                        '${(double.parse(taxPercentageController.text) / 100) * (totalTaxTaxableTypeAmount + totalTaxT2Amount + totalTaxT3Amount + double.parse(totalPriceAfterDiscountController.text))}';
                }

                notifyListeners();
            }

            calculationTaxTwo() {
                double totalTaxTaxableTypeAmount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxTaxTaxableTypes == "true" &&
                            element.taxType != "T1" &&
                            element.taxType != "T2" &&
                            element.taxType != "T3" &&
                            element.taxType != "T4"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);
                double totalTaxT3Amount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxType == "T3"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);

                if (unitPriceBeforeDiscountController.text.isNotEmpty &&
                    taxPercentageController.text.isNotEmpty)
                {
                    taxAmountController.text =
                        '${(double.parse(taxPercentageController.text) / 100) * (totalTaxTaxableTypeAmount + totalTaxT3Amount + double.parse(totalPriceAfterDiscountController.text))}';
                }

                notifyListeners();
            }

            calculationDefaultTax() {
                if (unitPriceBeforeDiscountController.text.isNotEmpty &&
                    taxPercentageController.text.isNotEmpty)
                {
                    taxAmountController.text =
                        '${(double.parse(taxPercentageController.text) / 100) * double.parse(totalPriceAfterDiscountController.text)}';
                }

                notifyListeners();
            }

            saveExtraTaxToBill() {
                savedExtraTaxesToBillList.add(
                  ExtraTax(
                    id: '',
                    taxType: taxNameController.text,
                    taxTypeName: taxTypeNameList,
                    taxSubType: taxSubTypeController.text,
                    taxSubTypeId: taxSubTypeIdController.text,
                    taxPercentage:
                        taxIsPercentage && taxPercentageController.text.isNotEmpty
                            ? taxPercentageController.text
                            : "0",
                    taxValue: !taxIsPercentage && taxAmountController.text.isNotEmpty
                        ? taxAmountController.text
                        : "0",
                    taxTotalAmount: taxAmountController.text,
                    taxTaxTaxableTypes: taxTaxTaxableTypes,
                    taxIsPercentage: taxIsPercentage,
            
                  ),
            
                );

                notifyListeners();
            }

            updateTaxesList() async {
                for (int i = 0; i < savedExtraTaxesToBillList.length; i++)
                {
                    if (savedExtraTaxesToBillList[i].taxType != "T1" &&
                        savedExtraTaxesToBillList[i].taxType != "T2" &&
                        savedExtraTaxesToBillList[i].taxIsPercentage == true &&
                        totalPriceAfterDiscountController.text.isNotEmpty)
                    {
                        savedExtraTaxesToBillList[i].taxTotalAmount =
                            '${(double.parse(savedExtraTaxesToBillList[i].taxPercentage) / 100) * double.parse(totalPriceAfterDiscountController.text)}';
                    }
                }

                double totalTaxTaxableTypeAmount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxTaxTaxableTypes == "true" &&
                            element.taxType != "T1" &&
                            element.taxType != "T2" &&
                            element.taxType != "T3" &&
                            element.taxType != "T4"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);
                double totalTaxT3Amount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxType == "T3"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);

                for (int i = 0; i < savedExtraTaxesToBillList.length; i++)
                {
                    if (savedExtraTaxesToBillList[i].taxType == "T2" &&
                        totalPriceAfterDiscountController.text.isNotEmpty)
                    {
                        savedExtraTaxesToBillList[i].taxTotalAmount =
                            '${(double.parse(savedExtraTaxesToBillList[i].taxPercentage) / 100) * (totalTaxTaxableTypeAmount + totalTaxT3Amount + double.parse(totalPriceAfterDiscountController.text))}';
                    }
                }

                double totalTaxT2Amount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxType == "T2"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);
        
                for (int i = 0; i < savedExtraTaxesToBillList.length; i++)
                {
                    if (savedExtraTaxesToBillList[i].taxType == "T1" &&
                        totalPriceAfterDiscountController.text.isNotEmpty)
                    {
                        savedExtraTaxesToBillList[i].taxTotalAmount =
                            '${(double.parse(savedExtraTaxesToBillList[i].taxPercentage) / 100) * (totalTaxTaxableTypeAmount + totalTaxT2Amount + totalTaxT3Amount + double.parse(totalPriceAfterDiscountController.text))}';
                    }
                }
        --
                double totalTaxT4Amount = 0;
                totalTaxT4Amount = savedExtraTaxesToBillList.fold(
                    0,
                    (total, element) => element.taxType == "T4"
                        ? double.parse(element.taxTotalAmount) + total
                        : total);
                totalTaxAmount = savedExtraTaxesToBillList.fold(
                        0.0,
                        (total, element) => element.taxType != "T4"
                            ? double.parse(element.taxTotalAmount) + total
                            : total) -
                    totalTaxT4Amount;
                if (totalPriceAfterDiscountController.text.isNotEmpty)
                {
                    totalPriceAfterTaxAndDiscountController.text =
                        '${double.parse(totalPriceAfterDiscountController.text) + totalTaxAmount}';
                }

                await clearExtraTaxParameters();

                notifyListeners();
            }
        }*/





        public BaseResponseWithData<string> GetSalesOfferDueClientPOS(string Datefrom, string Dateto, string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            #region validation
            var startDate = DateTime.Now;
            if (!DateTime.TryParse(Datefrom, out startDate))
            {
                response.Result = false;
                Error err = new Error();
                err.ErrorCode = "E-1";
                err.errorMSG = "please, Enter a valid DateFrom";
                response.Errors.Add(err);
                return response;
            }

            var endDate = DateTime.Now;
            if (!DateTime.TryParse(Dateto, out endDate))
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
                var fullResponse = new BaseResponseWithData<List<SalesOfferDueClientPOSResponse>>();
                var responseData = new List<SalesOfferDueClientPOSResponse>();

                var ownerGroup = new List<SalesOfferDueClientPOS>();
                var productListOfOwner = new List<SalesOfferProductPOS>();

                var data = _unitOfWork.SalesOffers.FindAllQueryable(a => true, new[] { "Client", "SalesOfferProducts" });

                var ownerData = data.Where(a => a.Client.OwnerCoProfile == true &&
                a.SendingOfferDate >= startDate && a.SendingOfferDate <= endDate).GroupBy(a => a.Client.Name).ToList();

                var salesOffersIDsOfOwner = data.Select(a => a.Id).ToList();

                var inventoryStoreItems = _unitOfWork.InventoryStoreItems.FindAll(a => salesOffersIDsOfOwner.Contains(a.OrderId) && (a.OperationType.Contains("POS Release")
                || a.OperationType.Contains("POS Return"))).ToList();

                var storesList = _unitOfWork.InventoryStores.FindAll(a => inventoryStoreItems.Select(a => a.InventoryStoreId).Contains(a.Id));

                foreach (var group in ownerData)
                {
                    var newGroup = new SalesOfferDueClientPOSResponse();
                    newGroup.ClientName = group.Key;

                    var ownerSalesOfferList = new List<SalesOfferDueClientPOS>();
                    //newSalesOffer.ClientName = group.Key;
                    foreach (var item in group)
                    {
                        var newSalesOffer = new SalesOfferDueClientPOS();
                        var productList = new List<SalesOfferProductPOS>();

                        newSalesOffer.OfferID = item.Id;
                        newSalesOffer.finalOfferPrice = item.FinalOfferPrice ?? 0;
                        newSalesOffer.projectName = item.ProjectName;
                        newSalesOffer.CreationDate = item.CreationDate.ToShortDateString();
                        newSalesOffer.OfferType = item.OfferType;

                        var inventoryItemid = inventoryStoreItems.Where(a => a.OrderId == item.Id).FirstOrDefault();
                        if (inventoryItemid != null)
                        {
                            var storeData = storesList.Where(a => a.Id == inventoryItemid.InventoryStoreId).FirstOrDefault();
                            newSalesOffer.StoreName = storeData.Name;
                        }

                        var product = item.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                        {
                            ItemPrice = a.ItemPrice,
                            Quantity = a.Quantity,
                            ProductID = a.Id,
                            productComment = a.ItemPricingComment
                        });

                        productList.AddRange(product);
                        newSalesOffer.ProductList = productList;
                        ownerSalesOfferList.Add(newSalesOffer);
                    }
                    newGroup.SalesOfferDueClientPOSList = ownerSalesOfferList;
                    responseData.Add(newGroup);
                }

                var otherClientData = data.Where(a => a.Client.OwnerCoProfile != true &&
                a.SendingOfferDate >= startDate && a.SendingOfferDate <= endDate).ToList().GroupBy(a => a.Client.Name);

                foreach (var group in otherClientData)
                {
                    var newGroup = new SalesOfferDueClientPOSResponse();
                    newGroup.ClientName = group.Key;

                    var ownerSalesOfferList = new List<SalesOfferDueClientPOS>();
                    //newSalesOffer.ClientName = group.Key;
                    foreach (var item in group)
                    {
                        var newSalesOffer = new SalesOfferDueClientPOS();
                        var productList = new List<SalesOfferProductPOS>();

                        newSalesOffer.OfferID = item.Id;
                        newSalesOffer.finalOfferPrice = item.FinalOfferPrice ?? 0;
                        newSalesOffer.projectName = item.ProjectName;
                        newSalesOffer.CreationDate = item.CreationDate.ToShortDateString();
                        newSalesOffer.OfferType = item.OfferType;

                        var inventoryStoreItem = inventoryStoreItems.Where(a => a.OrderId == item.Id).FirstOrDefault();
                        var storeData = storesList.Where(a => a.Id == inventoryStoreItem?.InventoryStoreId).FirstOrDefault();
                        newSalesOffer.StoreName = storeData.Name;


                        var product = item.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                        {
                            ItemPrice = a.ItemPrice,
                            Quantity = a.Quantity,
                            ProductID = a.Id,
                            productComment = a.ItemPricingComment

                        });

                        productList.AddRange(product);
                        newSalesOffer.ProductList = productList;

                        ownerSalesOfferList.Add(newSalesOffer);
                    }
                    newGroup.SalesOfferDueClientPOSList = ownerSalesOfferList;
                    responseData.Add(newGroup);
                }
                fullResponse.Data = responseData;

                if (responseData.Count() > 0)
                {

                    //--------------------------------------fill excel with data-----------------------------------
                    ExcelPackage excel = new ExcelPackage();

                    var sheet = excel.Workbook.Worksheets.Add($"sheet1");


                    sheet.Cells[1, 1].Value = "Client Name";
                    sheet.Cells[1, 2].Value = "sales offer Name";
                    sheet.Cells[1, 3].Value = "final Offer Price";
                    //sheet.Cells[1, 4].Value = "Offer Name";
                    //sheet.Cells[1, 5].Value = "final Offer Price";
                    sheet.Cells[1, 4].Value = "Creation Date";
                    sheet.Cells[1, 5].Value = "Offer sheet";
                    sheet.Cells[1, 6].Value = "Store Name";
                    sheet.Cells[1, 7].Value = "Product";
                    sheet.Cells[1, 8].Value = "Quantity";
                    sheet.Cells[1, 9].Value = "Item Price";


                    int rowNum = 3;
                    foreach (var item in responseData)
                    {
                        sheet.Row(rowNum).OutlineLevel = 1;
                        sheet.Row(rowNum).Collapsed = false;
                    }

                    for (int col = 1; col <= 10; col++) sheet.Column(col).Width = 20;
                    sheet.DefaultRowHeight = 12;
                    sheet.Row(1).Height = 20;
                    sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Row(1).Style.Font.Bold = true;
                    sheet.Cells[1, 1, 1, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, 1, 1, 10].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    sheet.Cells[1, 1, 1, 10].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[1, 1, 1, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    int rowIndex = 3;
                    foreach (var group in fullResponse.Data)
                    {
                        int startRow = rowIndex;
                        sheet.Row(rowIndex).OutlineLevel = 1;
                        sheet.Row(rowIndex).Collapsed = false;
                        sheet.Cells[rowIndex, 1].Value = group.ClientName;
                        sheet.Cells[rowIndex, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        sheet.Cells[rowIndex, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.CadetBlue);
                        sheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        sheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        foreach (var item in group.SalesOfferDueClientPOSList)
                        {
                            sheet.Row(rowIndex).OutlineLevel = 2;
                            sheet.Row(rowIndex).Collapsed = true;
                            sheet.Cells[rowIndex, 2].Value = item.projectName;
                            sheet.Cells[rowIndex, 3].Value = item.finalOfferPrice;
                            sheet.Cells[rowIndex, 4].Value = item.CreationDate.ToString();
                            sheet.Cells[rowIndex, 5].Value = item.OfferType;
                            sheet.Cells[rowIndex, 6].Value = item.StoreName;

                            sheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            if (item.OfferType == "Sales Return")                    //make the row color red on Sales Return
                            {
                                sheet.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                sheet.Cells[rowIndex, 1, rowIndex, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                            }
                            rowIndex++;

                            foreach (var product in item.ProductList)
                            {
                                sheet.Row(rowIndex).OutlineLevel = 3;
                                sheet.Row(rowIndex).Collapsed = true;
                                sheet.Cells[rowIndex, 7].Value = product.productComment;
                                sheet.Cells[rowIndex, 8].Value = product.Quantity;
                                sheet.Cells[rowIndex, 9].Value = product.ItemPrice;
                                sheet.Row(rowIndex).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet.Row(rowIndex).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                if (item.OfferType == "Sales Return")                    //make the row color red on Sales Return
                                {
                                    sheet.Cells[rowIndex, 7, rowIndex, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                    sheet.Cells[rowIndex, 7, rowIndex, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                }
                                rowIndex++;
                            }
                        }
                        rowIndex++;
                        sheet.Cells[startRow, 1, rowIndex - 1, 1].Merge = true;

                        //---------------------border of meraged Cells------------------------------
                        var border = sheet.Cells[startRow, 1, rowIndex - 1, 1].Style.Border;

                        // Set border style (thick for bold)
                        border.Top.Style = ExcelBorderStyle.Thick;
                        border.Bottom.Style = ExcelBorderStyle.Thick;
                        border.Left.Style = ExcelBorderStyle.Thick;
                        border.Right.Style = ExcelBorderStyle.Thick;

                        // Set the color to black for all sides
                        border.Top.Color.SetColor(System.Drawing.Color.Black);
                        border.Bottom.Color.SetColor(System.Drawing.Color.Black);
                        border.Left.Color.SetColor(System.Drawing.Color.Black);
                        border.Right.Color.SetColor(System.Drawing.Color.Black);
                        //--------------------------------------------------------------------------
                    }
                    //ExpensesWorkSheet.Cells[1, 1, 1, 2 ].Style.Font.Color.SetColor(Color.White);

                    //--------------------------summation---------------------------
                    sheet.Cells[2, 3].Formula = "SUM(C3:C1500)";
                    sheet.Cells[2, 9].Formula = "SUM(I3:I1500)";

                    sheet.Cells[2, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[2, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    sheet.Cells[2, 3].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[2, 3].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);

                    sheet.Cells[2, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[2, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    sheet.Cells[2, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[2, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.DarkGreen);
                    //--------------------------------------------------------------


                    //-----------------------------------Save file -----------------------------------------------
                    var path = $"Attachments\\{CompName}\\SalesOfferDueClientPOSReports";
                    var savedPath = Path.Combine(_host.WebRootPath, path);
                    if (File.Exists(savedPath))
                        File.Delete(savedPath);

                    // Create excel file on physical disk  
                    Directory.CreateDirectory(savedPath);
                    //FileStream objFileStrm = File.Create(savedPath);
                    //objFileStrm.Close();
                    var date = DateTime.Now.ToString("yyyyMMddHHssFFF");
                    var excelPath = savedPath + $"\\SalesOfferDueClientPOSReport.xlsx";
                    excel.SaveAs(excelPath);
                    // Write content to excel file  
                    //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                    //Close Excel package 
                    excel.Dispose();

                    fullResponse.Data[0].FilePath = Globals.baseURL + '\\' + path + $"\\SalesOfferDueClientPOSReport.xlsx";

                    response.Data = fullResponse.Data[0].FilePath;
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }


        public BaseResponseWithData<string> GetSalesOfferDueForStore(string date, string CompName, bool type = false)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region Validation
                var ValidationDate = DateTime.Now;
                if (!DateTime.TryParse(date, out ValidationDate))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    response.Errors.Add(err);
                    return response;
                }
                var nextDay = ValidationDate.AddDays(1);

                #endregion

                var listOfStoreData = new List<SalesOfferDueClientPOS>();

                var inventoryStoreItemsData = new List<InventoryStoreItem>();

                if (type == false)
                {

                    inventoryStoreItemsData = _unitOfWork.InventoryStoreItems.FindAll(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay && a.OperationType.Contains("POS"), new[] { "InventoryStore", "CreatedByNavigation" }).ToList();
                }
                else
                {
                    inventoryStoreItemsData = _unitOfWork.InventoryStoreItems.FindAll(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay && a.OperationType.Contains("Ticket"), new[] { "InventoryStore", "CreatedByNavigation" }).ToList();
                }
                var salesOffersIDs = inventoryStoreItemsData.Select(a => a.OrderId).ToList();

                var salesOffers = _unitOfWork.SalesOffers.FindAll(a => salesOffersIDs.Contains(a.Id), new[] { "SalesOfferProducts", "CreatedByNavigation", "Client" }).ToList();
                var minusSummation = salesOffers.Where(a => a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice);
                var plusSummationu = salesOffers.Where(a => a.OfferType != "Sales Return").Sum(a => a.FinalOfferPrice);
                var totalSummationu = plusSummationu - minusSummation;

                var inventoryStoreItemsDataGroupes = inventoryStoreItemsData.GroupBy(a => new { a.InventoryStoreId, a.CreatedBy }).ToList();

                var stories = _unitOfWork.InventoryStores.GetAll();

                var createdByUserIDs = inventoryStoreItemsData.Select(a => a.CreatedBy).ToList();
                var createdByUserList = _unitOfWork.Users.FindAll(a => createdByUserIDs.Contains(a.Id));
                //----------------------------------------excel----------------------------------------------
                ExcelPackage excel = new ExcelPackage();

                //-------------------------------------------------------------------------------------------
                if (inventoryStoreItemsDataGroupes.Count() == 0)
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "This date selected not have sales offer";
                    response.Errors.Add(err);
                    return response;
                }

                //var salesOfferList = new List<SalesOfferDueClientPOS>();
                foreach (var group in inventoryStoreItemsDataGroupes)
                {

                    //var newSalesOffer = new SalesOfferDueClientPOS();
                    //var productList = new List<SalesOfferProductPOS>();
                    //var DB = salesOffers.Where(a => a.Id == item.OrderId).GroupBy(a => new { a.CreatedBy});
                    //var salesOfferOfthisStoreData = salesOffers.Where(a => a.Id == group.Key.CreatedBy).FirstOrDefault();
                    var userDataOfthisGroup = createdByUserList.Where(a => a.Id == group.Key.CreatedBy).FirstOrDefault();
                    var storeData = stories.Where(a => a.Id == group.Key.InventoryStoreId).FirstOrDefault();
                    var sheet = excel.Workbook.Worksheets.Add($"{storeData.Name.Trim()}_{userDataOfthisGroup.FirstName.Trim()}_{userDataOfthisGroup.LastName.Trim()}");

                    //---------naming of Excel file--------------
                    sheet.Cells[1, 1].Value = "sales offer Name";
                    sheet.Cells[1, 2].Value = "final Offer Price";
                    //sheet.Cells[1, 4].Value = "Offer Name";
                    //sheet.Cells[1, 5].Value = "final Offer Price";
                    sheet.Cells[1, 3].Value = "Creation Date";
                    sheet.Cells[1, 4].Value = "Offer Type";
                    sheet.Cells[1, 6].Value = "Product Comment";
                    sheet.Cells[1, 5].Value = "client";
                    sheet.Cells[1, 7].Value = "Quantity";
                    sheet.Cells[1, 8].Value = "Item Price";
                    sheet.Cells[1, 9].Value = "Cash Amount";



                    for (int col = 1; col <= 10; col++) sheet.Column(col).Width = 20;
                    sheet.DefaultRowHeight = 12;
                    sheet.Row(1).Height = 20;
                    sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Row(1).Style.Font.Bold = true;
                    sheet.Cells[1, 1, 1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, 1, 1, 9].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    sheet.Cells[1, 1, 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[1, 1, 1, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    decimal testSum = 0;
                    decimal testSub = 0;
                    decimal sum = 0;
                    decimal cashSum = 0;
                    int rowNum = 3;
                    var salesOffersDrawed = new List<long>();
                    foreach (var inventoryStoreItem in group)
                    {
                        //newSalesOffer.OfferID = item.OrderId;
                        var salesOffer = salesOffers.Where(a => a.Id == inventoryStoreItem.OrderId && a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
                        //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);
                        if (salesOffer != null)
                        {
                            var alreadyDrawed = salesOffersDrawed.Contains(salesOffer.Id);
                            if (salesOffer != null && !alreadyDrawed)
                            {

                                sheet.Row(rowNum).OutlineLevel = 1;
                                sheet.Row(rowNum).Collapsed = false;
                                sheet.Cells[rowNum, 1].Value = salesOffer.ProjectName;
                                sheet.Cells[rowNum, 2].Value = salesOffer.FinalOfferPrice;
                                sheet.Cells[rowNum, 3].Value = salesOffer.CreationDate.ToString();
                                sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;
                                sheet.Cells[rowNum, 5].Value = salesOffer.Client.Name;
                                sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                                //---------------sum of cash-----------------------------
                                if (salesOffer.OfferType == "Sales Return") cashSum -= salesOffer.FinalOfferPrice ?? 0;
                                else { cashSum += salesOffer.FinalOfferPrice ?? 0; }
                                //-------------------------------------------------------
                                foreach (var productDB in salesOffer.SalesOfferProducts)
                                {
                                    sheet.Row(rowNum + 1).OutlineLevel = 2;
                                    sheet.Row(rowNum + 1).Collapsed = true;
                                    sheet.Cells[rowNum + 1, 6].Value = productDB.ItemPricingComment;
                                    sheet.Cells[rowNum + 1, 7].Value = productDB.Quantity;
                                    sheet.Cells[rowNum + 1, 8].Value = productDB.ItemPrice;
                                    sheet.Row(rowNum + 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                    sheet.Row(rowNum + 1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;


                                    if (salesOffer.OfferType == "Sales Return")                    //make the row color red on Sales Return
                                    {
                                        sheet.Cells[rowNum + 1, 6, rowNum + 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                        sheet.Cells[rowNum + 1, 6, rowNum + 1, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                        sheet.Row(rowNum + 1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                        sheet.Row(rowNum + 1).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                                    }

                                    rowNum++;
                                }
                                if (salesOffer.OfferType == "Sales Return") { sum = sum - salesOffer.FinalOfferPrice ?? 0; }
                                else { sum += salesOffer.FinalOfferPrice ?? 0; }


                                salesOffersDrawed.Add(salesOffer.Id);

                                //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
                                //newSalesOffer.projectName = salesOffer.ProjectName;
                                //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
                                //newSalesOffer.OfferType = salesOffer.OfferType;
                            }

                            //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                            //{
                            //    ItemPrice = a.ItemPrice,
                            //    Quantity = a.Quantity,
                            //    ProductID = a.Id,
                            //    productComment = a.ItemPricingComment
                            //});

                            //productList.AddRange(product);
                            //newSalesOffer.ProductList = productList;
                            //salesOfferList.Add(newSalesOffer);
                            if (!alreadyDrawed) rowNum++;
                        }
                    }
                    sheet.Cells[2, 2].Value = sum;
                    sheet.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                    salesOffersDrawed.Clear();
                    sum = 0;


                    //---------------sum of cash drawing-----------------------
                    sheet.Cells[2, 9].Value = cashSum;
                    sheet.Cells[2, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    sheet.Cells[2, 9].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    sheet.Cells[2, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[2, 9].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

                }

                var path = $"Attachments\\{CompName}\\GetSalesOfferDueForStore";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var dateNow = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\GetSalesOfferDueForStorePOSReport.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var filePath = Globals.baseURL + '\\' + path + $"\\GetSalesOfferDueForStorePOSReport.xlsx";

                response.Data = filePath;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }
        }

        public async Task<RejectedOfferScreenDataResponse> RejectedOfferScreenData(long? POID, long? PRID)
        {
            RejectedOfferScreenDataResponse Response = new RejectedOfferScreenDataResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                //long POID = 0;
                //long PRID = 0;
                if (POID != null)
                {
                    var PoDb = await _unitOfWork.VPurchaseRequestItemsPo.FindAsync(a => a.PurchasePoid == POID);
                    if (PoDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "Po Not Exist!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        PRID = (long)PoDb.PurchaseRequestId;
                    }
                }
                else
                {
                    if (PRID != null)
                    {
                        var PrDb = await _unitOfWork.VPurchaseRequestItemsPo.FindAsync(a => a.PurchaseRequestId == PRID);
                        if (PrDb == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err76";
                            error.ErrorMSG = "Pr Not Exist!";
                            Response.Errors.Add(error);
                            return Response;
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "You Must Enter Either POID Or PRID";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }


                if (Response.Result)
                {
                    Response.CurrencyDDL = GetCurrenciesList().Result;

                    Response.SuppliersDDL = await _Context.Suppliers.Select(supplier => new SelectDDL
                    {
                        ID = supplier.Id,
                        Name = supplier.Name
                    }).ToListAsync();

                    var prItemsDbQuery = _unitOfWork.VPurchaseRequestItemsPo.FindAllQueryable(a => true);
                    if (POID != 0)
                    {
                        prItemsDbQuery = prItemsDbQuery.Where(a => a.PurchasePoid == POID).AsQueryable();
                    }
                    else
                    {
                        prItemsDbQuery = prItemsDbQuery.Where(a => a.PurchasePoid == null).AsQueryable();
                        if (PRID != 0)
                        {
                            prItemsDbQuery = prItemsDbQuery.Where(a => a.PurchaseRequestId == PRID).AsQueryable();
                        }
                    }

                    var PrItemsDb = await prItemsDbQuery.ToListAsync();
                    if (PrItemsDb != null && PrItemsDb.Count > 0)
                    {
                        var PrItemsIds = PrItemsDb.Select(a => a.PurchaseRequestItemsId).ToList();
                        var RejectPrItemsDb = await _unitOfWork.PRSupplierOfferItems.FindAllAsync(a => PrItemsIds.Contains(a.PritemId));
                        var PrItemsResponse = PrItemsDb.Select(prItem => new PrSupplierOfferItem
                        {
                            ConversionRate = prItem.ExchangeFactor,
                            InventoryItemCode = prItem.InventoryItemCode,
                            InventoryItemId = prItem.InventoryItemId,
                            InventoryItemName = prItem.InventoryItemName,
                            InventoryItemPartNo = prItem.PartNo,
                            MrItemId = prItem.InventoryMatrialRequestId,
                            PoId = prItem.PurchasePoid,
                            PoItemId = prItem.PurchasePoitemId,
                            PrId = prItem.PurchaseRequestId,
                            PrItemId = prItem.PurchaseRequestItemsId,
                            PurchasingUOMShortName = prItem.PurchasedUomshortName,
                            PurchasingUOMId = prItem.PurchasedUomid,
                            ReqQuantity = prItem.PurchaseRequestItemQuantity,
                            RequstionUOMShortName = prItem.RequstionUomshortName,
                            RequstionUOMId = prItem.RequstionUomid,
                            UOMId = prItem.InventoryUomid ?? 0,
                            UOMName = prItem.InventoryUomshortName,
                            PrItemRejectedHistory = RejectPrItemsDb != null && RejectPrItemsDb.Count() > 0 ? RejectPrItemsDb.Where(a => a.PoitemId == prItem.PurchaseRequestItemsId).Select(RejectItem => new PrRejectOfferItem
                            {
                                Id = RejectItem.Id,
                                CurrencyName = RejectItem.Currency.Name,
                                RateToEGP = RejectItem.RateToEgp,
                                SupplierName = RejectItem.PrsupplierOffer.Supplier.Name,
                                TotalEstimatedCost = RejectItem.TotalEstimatedCost
                            }).ToList() : null
                        }).ToList();

                        Response.PrSupplierOfferItems = PrItemsResponse;
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

        public async Task<List<SelectDDL>> GetCurrenciesList(string CompanyName = null)
        {

            var CurrenciesList = await _unitOfWork.Currencies.GetAllAsync();
            if (CompanyName == "marinapltq")
            {
                CurrenciesList = CurrenciesList.Where(x => x.Id == 5).ToList();
            }
            var CurrenciesDDL = new List<SelectDDL>();
            foreach (var C in CurrenciesList)
            {
                var DDLObj = new SelectDDL();
                DDLObj.ID = C.Id;
                DDLObj.Name = C.Name;

                CurrenciesDDL.Add(DDLObj);
            }

            return CurrenciesDDL;
        }


        public bool CloseSalesOffer(long SalesOfferId, string CompanyName, long CreatorId)
        {
            bool Result = false;

            var SalesOfferDb = _unitOfWork.SalesOffers.GetById(SalesOfferId);

            var lastProjectSerial = _unitOfWork.Projects.FindAll(a => a.Active == true).ToList().OrderByDescending(a => a.Id).Select(a => a.ProjectSerial).FirstOrDefault();
            long newProjectNumber = 0;
            string NewProjectSerial = "";
            if (CompanyName.ToLower() == "marinaplt" || lastProjectSerial.Contains("Pro#"))
            {
                if (lastProjectSerial != null && lastProjectSerial.Contains(System.DateTime.Now.Year.ToString()))
                {
                    string strLastProjectNumber = lastProjectSerial.Split('#')[2].Split('-')[0];
                    newProjectNumber = long.Parse(strLastProjectNumber) + 1;
                    NewProjectSerial += SalesOfferDb.OfferSerial + "-Pro#" + newProjectNumber + "-" + DateTime.Now.Year.ToString();
                }
                else
                    NewProjectSerial += SalesOfferDb.OfferSerial + "-Pro#" + "1-" + System.DateTime.Now.Year.ToString();
            }
            else
            {

                if (lastProjectSerial != null && lastProjectSerial.Contains(System.DateTime.Now.Year.ToString()))
                {
                    string strLastProjectNumber = lastProjectSerial.Split('-')[0];
                    newProjectNumber = long.Parse(strLastProjectNumber.Substring(1)) + 1;
                    NewProjectSerial += "#" + newProjectNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                }
                else
                    NewProjectSerial += "#1-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
            }
            // Insert New Project
            long ProjectId = 0;

            var ProjectInserting = new Project()
            {
                SalesOfferId = SalesOfferId,
                Closed = false,
                Revision = 0,
                StartDate = SalesOfferDb.ProjectStartDate ?? DateTime.Now,
                EndDate = SalesOfferDb.ProjectEndDate ?? DateTime.Now,
                InstallStartDate = null,
                InstallEndDate = null,
                InstallDuration = null,
                CreatedBy = CreatorId,
                CreationDate = SalesOfferDb.ClientApprovalDate ?? DateTime.Now,
                ModifiedBy = null,
                ModifiedDate = DateTime.Now,
                BranchId = SalesOfferDb.BranchId,
                ProjectManagerId = SalesOfferDb.SalesPersonId,
                MaintenanceType = "Offer",
                ExtraCost = 0,
                TotalCost = SalesOfferDb.FinalOfferPrice,
                ProjectSerial = NewProjectSerial,
                Active = true
            };
            _unitOfWork.Projects.Add(ProjectInserting);

            var ProjectInsert = _unitOfWork.Complete();

            if (ProjectInsert > 0)
            {
                Result = true;
                ProjectId = ProjectInserting.Id;
                var OfferAssignedBOM = _unitOfWork.Boms.FindAll(a => a.OfferId == SalesOfferId).ToList();
                if (OfferAssignedBOM != null && OfferAssignedBOM.Count > 0)
                {
                    foreach (var bom in OfferAssignedBOM)
                    {
                        bom.ProjectId = ProjectId;
                        bom.ModifiedBy = CreatorId;
                        bom.ModifiedDate = DateTime.Now;
                        _unitOfWork.Complete();
                    }
                }

                //Add New GeneralActiveCostCenters
                long GeneralActiveCostCenterId = 0;
                var costCenterCategory = "";
                if (SalesOfferDb.OfferType == "New Project Offer")
                {
                    costCenterCategory = "Project";
                }
                else if (SalesOfferDb.OfferType == "New Maintenance Offer")
                {
                    costCenterCategory = "Maintenance";
                }
                else if (SalesOfferDb.OfferType == "New Rent Offer")
                {
                    costCenterCategory = "Rent";
                }

                var GeneralActiveCostCenterInserting = new GeneralActiveCostCenter()
                {
                    CostCenterName = SalesOfferDb.ProjectName,
                    Category = costCenterCategory,
                    CategoryId = ProjectId,
                    Serial = SalesOfferDb.OfferSerial,
                    Description = "",
                    SellingPrice = SalesOfferDb.FinalOfferPrice,
                    CumulativeCost = 0,
                    Active = true,
                    Closed = true,
                    CreatedBy = CreatorId,
                    CreationDate = DateTime.Now,
                    ModifiedBy = CreatorId,
                    Modified = DateTime.Now
                };
                _unitOfWork.GeneralActiveCostCenters.Add(GeneralActiveCostCenterInserting);

                var GeneralActiveCostCenterInsert = _unitOfWork.Complete();


                if (GeneralActiveCostCenterInsert > 0)
                {
                    Result = true;
                    GeneralActiveCostCenterId = GeneralActiveCostCenterInserting.Id;
                }
                else
                {
                    Result = false;
                }

            }
            else
            {
                Result = false;
            }
            return Result;
        }

        public BaseResponseWithId<long> AddNewSalesOffer(AddNewSalesOfferData Request, long creator, string CompanyName)
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
                        if (Request.SalesOffer.ParentSalesOfferID != null)
                        {
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
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Sales Offer Data!!";
                        Response.Errors.Add(error);
                    }

                    if (Request.SalesOfferProductList != null)
                    {
                        if (Request.SalesOfferProductList.Count() > 0)
                        {
                            foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                            {
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
                                if (SalesOfferProduct.Id != 0 && SalesOfferProduct.Id != null)
                                {
                                    if (SalesOfferProduct.SalesOfferProductAttachments != null)
                                    {
                                        if (SalesOfferProduct.SalesOfferProductAttachments.Count() > 0)
                                        {
                                            foreach (var attachment in SalesOfferProduct.SalesOfferProductAttachments)
                                            {
                                                if (string.IsNullOrEmpty(attachment.FileContent))
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "File Content Is Mandatory!";
                                                    Response.Errors.Add(error);
                                                }
                                                if (string.IsNullOrEmpty(attachment.FileName))
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "File Name Is Mandatory!";
                                                    Response.Errors.Add(error);
                                                }
                                                if (string.IsNullOrEmpty(attachment.FileExtension))
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "File Extension Is Mandatory!";
                                                    Response.Errors.Add(error);
                                                }
                                                if (string.IsNullOrEmpty(attachment.Category))
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "File Category Is Mandatory!";
                                                    Response.Errors.Add(error);
                                                }
                                            }
                                        }
                                    }
                                }
                                //else
                                //{
                                //    if (SalesOfferProduct.CreatedBy != null)
                                //    {
                                //        CreatedByString = SalesOfferProduct.CreatedBy;
                                //    }
                                //    else
                                //    {
                                //        Response.Result = false;
                                //        Error error = new Error();
                                //        error.ErrorCode = "Err25";
                                //        error.ErrorMSG = "Sales Offer Product Created By Id Is Mandatory";
                                //        Response.Errors.Add(error);
                                //    }
                                //    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                //    var user = _Context.proc_UserLoadByPrimaryKey(CreatedBy).FirstOrDefault();
                                //    if (user == null)
                                //    {
                                //        Response.Result = false;
                                //        Error error = new Error();
                                //        error.ErrorCode = "ErrCRM1";
                                //        error.ErrorMSG = "Sales Offer Product Creator User Doesn't Exist!!";
                                //        Response.Errors.Add(error);
                                //    }
                                //}

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

                    if (Request.SalesOfferAttachmentList != null)
                    {
                        if (Request.SalesOfferAttachmentList.Count() > 0)
                        {
                            foreach (var attachment in Request.SalesOfferAttachmentList)
                            {
                                if (string.IsNullOrEmpty(attachment.FileContent))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "File Content Is Mandatory!";
                                    Response.Errors.Add(error);
                                }
                                if (string.IsNullOrEmpty(attachment.FileName))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "File Name Is Mandatory!";
                                    Response.Errors.Add(error);
                                }
                                if (string.IsNullOrEmpty(attachment.FileExtension))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "File Extension Is Mandatory!";
                                    Response.Errors.Add(error);
                                }
                                if (string.IsNullOrEmpty(attachment.Category))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "File Category Is Mandatory!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }

                    if (Response.Result)
                    {
                        long CreatedBy = creator;
                        long ModifiedBy = creator;
                        long SalesOfferId = 0;
                        string SalesOfferDbStatus = null;
                        Project ProjectSalesOfferIsExist = null;
                        // Add-Edit Sales Offer
                        if (Request.SalesOffer.Id != null && Request.SalesOffer.Id != 0)
                        {
                            var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == Request.SalesOffer.Id).FirstOrDefault();
                            if (SalesOfferDb != null)
                            {

                                SalesOfferDbStatus = SalesOfferDb.Status;
                            }
                            if (SalesOfferDb != null)
                            {
                                SalesOfferId = (long)Request.SalesOffer.Id;
                                // Update

                                SalesOfferDb.ModifiedBy = ModifiedBy;
                                SalesOfferDb.StartDate = DateOnly.FromDateTime((DateTime)StartDate);
                                SalesOfferDb.EndDate = DateOnly.FromDateTime((DateTime)EndDate);
                                SalesOfferDb.Status = string.IsNullOrEmpty(Request.SalesOffer.Status) ? SalesOfferDb.Status : Request.SalesOffer.Status;
                                SalesOfferDb.ClientId = Request.SalesOffer.ClientId == null || Request.SalesOffer.ClientId == 0 ? SalesOfferDb.ClientId : Request.SalesOffer.ClientId;
                                SalesOfferDb.BranchId = Request.SalesOffer.BranchId == 0 ? SalesOfferDb.BranchId : Request.SalesOffer.BranchId;
                                SalesOfferDb.SalesPersonId = Request.SalesOffer.SalesPersonId == 0 ? SalesOfferDb.SalesPersonId : Request.SalesOffer.SalesPersonId;
                                SalesOfferDb.Note = string.IsNullOrEmpty(Request.SalesOffer.TechnicalInfo) ? SalesOfferDb.Note : Request.SalesOffer.TechnicalInfo;
                                SalesOfferDb.ProjectData = string.IsNullOrEmpty(Request.SalesOffer.ProjectData) ? SalesOfferDb.ProjectData : Request.SalesOffer.ProjectData;
                                SalesOfferDb.FinancialInfo = string.IsNullOrEmpty(Request.SalesOffer.FinancialInfo) ? SalesOfferDb.FinancialInfo : Request.SalesOffer.FinancialInfo;
                                SalesOfferDb.PricingComment = string.IsNullOrEmpty(Request.SalesOffer.PricingComment) ? SalesOfferDb.PricingComment : Request.SalesOffer.PricingComment;
                                SalesOfferDb.OfferAmount = Request.SalesOffer.OfferAmount == null ? SalesOfferDb.OfferAmount : Request.SalesOffer.OfferAmount;
                                SalesOfferDb.SendingOfferConfirmation = Request.SalesOffer.SendingOfferConfirmation == null ? SalesOfferDb.SendingOfferConfirmation : Request.SalesOffer.SendingOfferConfirmation;
                                SalesOfferDb.SendingOfferDate = Request.SalesOffer.SendingOfferDate == null ? SalesOfferDb.SendingOfferDate : DateTime.Parse(Request.SalesOffer.SendingOfferDate);
                                SalesOfferDb.SendingOfferBy = string.IsNullOrEmpty(Request.SalesOffer.SendingOfferBy) ? SalesOfferDb.SendingOfferBy : Request.SalesOffer.SendingOfferBy;
                                SalesOfferDb.SendingOfferTo = string.IsNullOrEmpty(Request.SalesOffer.SendingOfferTo) ? SalesOfferDb.SendingOfferTo : Request.SalesOffer.SendingOfferTo;
                                SalesOfferDb.SendingOfferComment = string.IsNullOrEmpty(Request.SalesOffer.SendingOfferComment) ? SalesOfferDb.SendingOfferComment : Request.SalesOffer.SendingOfferComment;
                                SalesOfferDb.ClientApprove = Request.SalesOffer.ClientApprove == null ? SalesOfferDb.ClientApprove : Request.SalesOffer.ClientApprove;
                                SalesOfferDb.ClientComment = string.IsNullOrEmpty(Request.SalesOffer.ClientComment) ? SalesOfferDb.ClientComment : Request.SalesOffer.ClientComment;
                                SalesOfferDb.VersionNumber += 1;
                                SalesOfferDb.ClientApprovalDate = string.IsNullOrEmpty(Request.SalesOffer.ClientApprovalDate) ? SalesOfferDb.ClientApprovalDate : DateTime.Parse(Request.SalesOffer.ClientApprovalDate);
                                SalesOfferDb.ProductType = string.IsNullOrEmpty(Request.SalesOffer.ProductType) ? SalesOfferDb.ProductType : Request.SalesOffer.ProductType;
                                SalesOfferDb.ProjectName = string.IsNullOrEmpty(Request.SalesOffer.ProjectName) ? SalesOfferDb.ProjectName : Request.SalesOffer.ProjectName;
                                SalesOfferDb.ProjectLocation = string.IsNullOrEmpty(Request.SalesOffer.ProjectLocation) ? SalesOfferDb.ProjectLocation : Request.SalesOffer.ProjectLocation;
                                SalesOfferDb.ContactPersonMobile = string.IsNullOrEmpty(Request.SalesOffer.ContactPersonMobile) ? SalesOfferDb.ContactPersonMobile : Request.SalesOffer.ContactPersonMobile;
                                SalesOfferDb.ContactPersonEmail = string.IsNullOrEmpty(Request.SalesOffer.ContactPersonEmail) ? SalesOfferDb.ContactPersonEmail : Request.SalesOffer.ContactPersonEmail;
                                SalesOfferDb.ContactPersonName = string.IsNullOrEmpty(Request.SalesOffer.ContactPersonName) ? SalesOfferDb.ContactPersonName : Request.SalesOffer.ContactPersonName;
                                SalesOfferDb.ProjectStartDate = string.IsNullOrEmpty(Request.SalesOffer.ProjectStartDate) ? SalesOfferDb.ProjectStartDate : DateTime.Parse(Request.SalesOffer.ProjectStartDate);
                                SalesOfferDb.ProjectEndDate = string.IsNullOrEmpty(Request.SalesOffer.ProjectEndDate) ? SalesOfferDb.ProjectEndDate : DateTime.Parse(Request.SalesOffer.ProjectEndDate);
                                SalesOfferDb.OfferType = string.IsNullOrEmpty(Request.SalesOffer.OfferType) ? SalesOfferDb.OfferType : Request.SalesOffer.OfferType;
                                SalesOfferDb.ContractType = string.IsNullOrEmpty(Request.SalesOffer.ContractType) ? SalesOfferDb.ContractType : Request.SalesOffer.ContractType;
                                SalesOfferDb.ClientNeedsDiscount = Request.SalesOffer.ClientNeedsDiscount == null ? SalesOfferDb.ClientNeedsDiscount : Request.SalesOffer.ClientNeedsDiscount;
                                SalesOfferDb.RejectionReason = string.IsNullOrEmpty(Request.SalesOffer.RejectionReason) ? SalesOfferDb.RejectionReason : Request.SalesOffer.RejectionReason;
                                SalesOfferDb.NeedsInvoice = Request.SalesOffer.NeedsInvoice == null ? SalesOfferDb.NeedsInvoice : Request.SalesOffer.NeedsInvoice;
                                SalesOfferDb.NeedsExtraCost = Request.SalesOffer.NeedsExtraCost == null ? SalesOfferDb.NeedsExtraCost : Request.SalesOffer.NeedsExtraCost;
                                SalesOfferDb.OfferExpirationDate = string.IsNullOrEmpty(Request.SalesOffer.OfferExpirationDate) ? SalesOfferDb.OfferExpirationDate : DateTime.Parse(Request.SalesOffer.OfferExpirationDate);
                                SalesOfferDb.OfferExpirationPeriod = Request.SalesOffer.OfferExpirationPeriod == null ? SalesOfferDb.OfferExpirationPeriod : Request.SalesOffer.OfferExpirationPeriod;
                                SalesOfferDb.ExtraOrDiscountPriceBySalesPerson = Request.SalesOffer.ExtraOrDiscountPriceBySalesPerson == null ? SalesOfferDb.ExtraOrDiscountPriceBySalesPerson : Request.SalesOffer.ExtraOrDiscountPriceBySalesPerson;
                                SalesOfferDb.FinalOfferPrice = Request.SalesOffer.FinalOfferPrice == null ? SalesOfferDb.FinalOfferPrice : Request.SalesOffer.FinalOfferPrice;
                                SalesOfferDb.ReminderDate = string.IsNullOrEmpty(Request.SalesOffer.ReminderDate) ? SalesOfferDb.ReminderDate : DateTime.Parse(Request.SalesOffer.ReminderDate);
                                var SalesOfferUpdate = _unitOfWork.Complete();

                                if (SalesOfferUpdate > 0)
                                {
                                    Response.Result = true;
                                    Response.ID = Request.SalesOffer.Id ?? 0;

                                    if (SalesOfferDbStatus != null && SalesOfferDbStatus.ToLower() == "closed")
                                    {
                                        var UpdatedSalesOffer = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
                                        var SalesOfferProject = _unitOfWork.Projects.FindAll(a => a.SalesOfferId == SalesOfferId).FirstOrDefault();
                                        if (SalesOfferProject != null)
                                        {
                                            ProjectSalesOfferIsExist = SalesOfferProject;
                                            SalesOfferProject.TotalCost = UpdatedSalesOffer.FinalOfferPrice + (SalesOfferProject.ExtraCost ?? 0);
                                            _unitOfWork.Complete();

                                            var SalesOfferGeneralActiveCostCenter = _unitOfWork.GeneralActiveCostCenters.FindAll(a => a.CategoryId == SalesOfferProject.Id).FirstOrDefault();
                                            if (SalesOfferGeneralActiveCostCenter != null)
                                            {
                                                SalesOfferGeneralActiveCostCenter.SellingPrice = UpdatedSalesOffer.FinalOfferPrice + (SalesOfferProject.ExtraCost ?? 0);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Update this Offer!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err25";
                                error.ErrorMSG = "This Offer Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }



                        }
                        else
                        {
                            var NewOfferSerial = "";
                            var OfferSerialSubString = "";




                            //long newOfferNumber = 0;
                            long CountOfSalesOfferThisYear = _unitOfWork.SalesOffers.FindAll(x => x.Active == true && x.OfferSerial.Contains(System.DateTime.Now.Year.ToString())).Count();
                            if (CompanyName.ToLower() == "marinaplt")
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

                                var branchName = _unitOfWork.Branches.GetById(Request.SalesOffer.BranchId)?.Name ?? "";

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
                                NewOfferSerial += OfferSerialSubString + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Year.ToString();
                            }
                            else if (CompanyName.ToLower() == "proauto")
                            {
                                var lastSalesOfferSerial = _unitOfWork.SalesOffers.FindAll(a => a.Active == true).ToList().OrderByDescending(a => a.Id).Select(a => a.OfferSerial).FirstOrDefault();
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
                                NewOfferSerial += "#" + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                            }

                            // Insert
                            var SalesOfferInserting = new SalesOffer()
                            {
                                StartDate = DateOnly.FromDateTime((DateTime)StartDate),
                                EndDate = DateOnly.FromDateTime((DateTime)EndDate),
                                Note = string.IsNullOrEmpty(Request.SalesOffer.Note) ? null : Request.SalesOffer.Note,
                                SalesPersonId = Request.SalesOffer.SalesPersonId,
                                CreatedBy = CreatedBy,
                                CreationDate = DateTime.Now,
                                ModifiedBy = ModifiedBy,
                                Modified = DateTime.Now,
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
                            _unitOfWork.SalesOffers.Add(SalesOfferInserting);
                            var SalesOfferInsert = _unitOfWork.Complete();

                            if (SalesOfferInsert > 0)
                            {
                                SalesOfferId = SalesOfferInserting.Id;
                                Response.Result = true;
                                Response.ID = SalesOfferId;

                                if (Request.SalesOffer.OfferType == "New Maintenance Offer")
                                {

                                    var SalesMaintenanceOfferInseting = new SalesMaintenanceOffer()
                                    {
                                        MaintenanceSalesOfferId = SalesOfferId,
                                        LinkedSalesOfferId = Request.SalesOffer.LinkedSalesOfferId,
                                        Type = Request.SalesOffer.MaintenanceType,
                                        CreationDate = DateTime.Now,
                                        CreatedBy = CreatedBy,
                                        ModificationDate = DateTime.Now,
                                        ModifiedBy = ModifiedBy
                                    };
                                    _unitOfWork.SalesMaintenanceOffers.Add(SalesMaintenanceOfferInseting);
                                    var SalesMaintenanceOfferInsert = _unitOfWork.Complete();
                                }
                                if (Request.SalesOffer.OfferType == "New Rent Offer")
                                {
                                    var SalesRentOfferInserting = new SalesRentOffer()
                                    {
                                        RentSalesOfferId = SalesOfferId,
                                        RentFromDate = (DateTime)RentStartDate,
                                        RentToDate = (DateTime)RentEndDate,
                                        CreatedBy = CreatedBy,
                                        CreationDate = DateTime.Now,
                                        Active = true,
                                        ModifiedBy = ModifiedBy,
                                        ModificationDate = DateTime.Now,
                                    };
                                    _unitOfWork.SalesRentOffers.Add(SalesRentOfferInserting);
                                    var SalesRentOfferInsert = _unitOfWork.Complete();
                                }
                                var SalesOfferPricingInternalApprovalInserting = new SalesOfferInternalApproval()
                                {
                                    SalesOfferId = SalesOfferId,
                                    Type = "Pricing",
                                    UserId = null,
                                    GroupId = null,
                                    ByUser = null,
                                    Reply = null,
                                    Comment = null,
                                    Date = null,
                                    Active = true,
                                    CreatedBy = CreatedBy,
                                    CreationDate = DateTime.Now,
                                    ModifiedBy = ModifiedBy,
                                    ModifiedDate = DateTime.Now,
                                };
                                _unitOfWork.SalesOfferInternalApprovals.Add(SalesOfferPricingInternalApprovalInserting);
                                var SalesOfferPricingInternalApprovalInsert = _unitOfWork.Complete();

                                var SalesOfferTechnicalInternalApprovalInserting = new SalesOfferInternalApproval()
                                {
                                    SalesOfferId = SalesOfferId,
                                    Type = "Technical",
                                    UserId = null,
                                    GroupId = null,
                                    ByUser = null,
                                    Reply = null,
                                    Comment = null,
                                    Date = null,
                                    Active = true,
                                    CreatedBy = CreatedBy,
                                    CreationDate = DateTime.Now,
                                    ModifiedBy = ModifiedBy,
                                    ModifiedDate = DateTime.Now,
                                };
                                _unitOfWork.SalesOfferInternalApprovals.Add(SalesOfferTechnicalInternalApprovalInserting);
                                var SalesOfferTechnicalInternalApprovalInsert = _unitOfWork.Complete();

                                var SalesOfferHeadOfSalesInternalApprovalInserting = new SalesOfferInternalApproval()
                                {
                                    SalesOfferId = SalesOfferId,
                                    Type = "Head Of Sales",
                                    UserId = null,
                                    GroupId = null,
                                    ByUser = null,
                                    Reply = null,
                                    Comment = null,
                                    Date = null,
                                    Active = true,
                                    CreatedBy = CreatedBy,
                                    CreationDate = DateTime.Now,
                                    ModifiedBy = ModifiedBy,
                                    ModifiedDate = DateTime.Now,
                                };
                                _unitOfWork.SalesOfferInternalApprovals.Add(SalesOfferHeadOfSalesInternalApprovalInserting);
                                var SalesOfferHeadOfSalesInternalApprovalInsert = _unitOfWork.Complete();

                                if (Request.SalesOffer.ParentInvoiceID != null && Request.SalesOffer.ParentSalesOfferID != null)
                                {
                                    var InvoiceCNAndDNObj = new InvoiceCnandDn();
                                    InvoiceCNAndDNObj.ParentSalesOfferId = (long)Request.SalesOffer.ParentSalesOfferID;
                                    InvoiceCNAndDNObj.ParentInvoiceId = (long)Request.SalesOffer.ParentInvoiceID;
                                    InvoiceCNAndDNObj.SalesOfferId = SalesOfferId;
                                    InvoiceCNAndDNObj.Active = true;
                                    InvoiceCNAndDNObj.CreatedBy = CreatedBy;
                                    InvoiceCNAndDNObj.CreationDate = DateTime.Now;
                                    InvoiceCNAndDNObj.ModifiedBy = CreatedBy;
                                    InvoiceCNAndDNObj.ModificationDate = DateTime.Now;

                                    _unitOfWork.InvoiceCnandDns.Add(InvoiceCNAndDNObj);
                                    _unitOfWork.Complete();
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

                        if (Request.SalesOffer.Status.ToLower() == "closed")
                        {
                            if (ProjectSalesOfferIsExist == null)
                            {
                                CloseSalesOffer(SalesOfferId, CompanyName, creator);
                            }
                        }

                        var SalesOfferInvoices = _unitOfWork.Invoices.FindAll(a => a.SalesOfferId == SalesOfferId).Count();

                        // Add-Edit Sales Offer Product
                        if (Request.SalesOfferProductList != null)
                        {
                            if (Request.SalesOfferProductList.Count() > 0)
                            {
                                var ChangedSalesOfferProductsIds = Request.SalesOfferProductList.Where(a => a.Id != null).Select(a => (long)a.Id).ToList();
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
                                    if (SalesOfferProduct.Id != null && SalesOfferProduct.Id != 0)
                                    {
                                        var SalesOfferProcuctDb = _unitOfWork.SalesOfferProducts.GetById((long)SalesOfferProduct.Id);
                                        if (SalesOfferProcuctDb != null)
                                        {
                                            if (SalesOfferProduct.Active != null && SalesOfferProduct.Active != true)
                                            {
                                                // Delete Product Tax before delete Product
                                                var ProductTaxListdb = _unitOfWork.SalesOfferProductTaxes.FindAll(x => x.SalesOfferProductId == SalesOfferProduct.Id).ToList();
                                                if (ProductTaxListdb.Count() > 0)
                                                {
                                                    _unitOfWork.SalesOfferProductTaxes.DeleteRange(ProductTaxListdb);
                                                    var ResultOfRemove = _unitOfWork.Complete();
                                                }

                                                //Delete from salesoffer product
                                                _unitOfWork.SalesOfferProducts.Delete(SalesOfferProcuctDb);
                                                var SalesOfferProductDeleted = _unitOfWork.Complete();
                                                if (SalesOfferProductDeleted > 0)
                                                {
                                                    if (SalesOfferInvoices > 0)
                                                    {
                                                        var ProductInvoice = _unitOfWork.InvoiceItems.FindAll(a => a.SalesOfferProductId == SalesOfferProduct.Id).FirstOrDefault();
                                                        if (ProductInvoice != null)
                                                        {
                                                            _unitOfWork.InvoiceItems.Delete(ProductInvoice);
                                                        }
                                                    }
                                                    Response.Result = true;
                                                    Response.ID = SalesOfferId;
                                                }
                                            }
                                            else
                                            {

                                                // Update
                                                var UpdatedSalesOfferProductDb = _unitOfWork.SalesOfferProducts.GetById((long)SalesOfferProduct.Id);
                                                if (UpdatedSalesOfferProductDb != null)
                                                {

                                                    UpdatedSalesOfferProductDb.ModifiedBy = ModifiedBy;
                                                    UpdatedSalesOfferProductDb.Modified = DateTime.Now;
                                                    UpdatedSalesOfferProductDb.Active = true;
                                                    UpdatedSalesOfferProductDb.Quantity = SalesOfferProduct.Quantity;
                                                    UpdatedSalesOfferProductDb.ItemPrice = SalesOfferProduct.ItemPrice;
                                                    UpdatedSalesOfferProductDb.ItemPricingComment = ItemComment;
                                                    UpdatedSalesOfferProductDb.InvoicePayerClientId = SalesOfferProduct.InvoicePayerClientId;
                                                    UpdatedSalesOfferProductDb.DiscountPercentage = SalesOfferProduct.DiscountPercentage;
                                                    UpdatedSalesOfferProductDb.DiscountValue = SalesOfferProduct.DiscountValue;
                                                    UpdatedSalesOfferProductDb.FinalPrice = SalesOfferProduct.FinalPrice;
                                                    UpdatedSalesOfferProductDb.TaxPercentage = SalesOfferProduct.TaxPercentage;
                                                    UpdatedSalesOfferProductDb.TaxValue = SalesOfferProduct.TaxValue;
                                                    UpdatedSalesOfferProductDb.ProfitPercentage = ProfitPercentage;

                                                    var SalesOfferProductUpdate = _unitOfWork.Complete();

                                                    if (SalesOfferProductUpdate > 0)
                                                    {
                                                        Response.Result = true;
                                                    }
                                                    else
                                                    {
                                                        Response.Result = false;
                                                        Error error = new Error();
                                                        error.ErrorCode = "Err25";
                                                        error.ErrorMSG = "Faild To Update this Offer!!";
                                                        Response.Errors.Add(error);
                                                    }
                                                }
                                                else
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err25";
                                                    error.ErrorMSG = "SalesOfferProduct: " + SalesOfferProduct.Id + " Deoesn't exist";
                                                    Response.Errors.Add(error);
                                                }

                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Offer Product Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        // Insert
                                        var SalesOfferProductInserting = new SalesOfferProduct()
                                        {
                                            CreatedBy = CreatedBy,
                                            ModifiedBy = ModifiedBy,
                                            CreationDate = DateTime.Now,
                                            Modified = DateTime.Now,
                                            Active = true,
                                            ProductId = SalesOfferProduct.ProductId,
                                            OfferId = SalesOfferId,
                                            ProductGroupId = SalesOfferProduct.ProductGroupId,
                                            Quantity = SalesOfferProduct.Quantity,
                                            InventoryItemId = SalesOfferProduct.InventoryItemId,
                                            InventoryItemCategoryId = SalesOfferProduct.InventoryItemCategoryId,
                                            ItemPrice = SalesOfferProduct.ItemPrice,
                                            ItemPricingComment = ItemComment,
                                            ConfirmReceivingQuantity = SalesOfferProduct.ConfirmReceivingQuantity,
                                            ConfirmReceivingComment = SalesOfferProduct.ConfirmReceivingComment,
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
                                        _unitOfWork.SalesOfferProducts.Add(SalesOfferProductInserting);
                                        var SalesOfferProductInsert = _unitOfWork.Complete();

                                        if (SalesOfferProductInsert > 0)
                                        {
                                            ChangedSalesOfferProductsIds.Add((long)SalesOfferProductInserting.Id);
                                            SalesOfferProduct.Id = SalesOfferProductInserting.Id;

                                            if (SalesOfferProduct.ParentOfferProductId != null)
                                            {
                                                var ParentSalesOfferProductDb = _unitOfWork.SalesOfferProducts.FindAll(a => a.Id == (long)SalesOfferProduct.ParentOfferProductId).FirstOrDefault();
                                                if (ParentSalesOfferProductDb != null)
                                                {
                                                    ParentSalesOfferProductDb.RemainQty -= SalesOfferProduct.Quantity;
                                                    ParentSalesOfferProductDb.ReturnedQty += SalesOfferProduct.Quantity;
                                                    _unitOfWork.Complete();
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
                                    if (SalesOfferProduct.Id != null)
                                    {
                                        if (SalesOfferProduct.SalesOfferProductTaxsList != null)
                                        {
                                            if (SalesOfferProduct.SalesOfferProductTaxsList.Count() > 0)
                                            {
                                                var IDSProductTaxes = SalesOfferProduct.SalesOfferProductTaxsList.Select(x => x.ID).ToList();
                                                var ProductTaxListdb = _unitOfWork.SalesOfferProductTaxes.FindAll(x => x.SalesOfferProductId == SalesOfferProduct.Id && !IDSProductTaxes.Contains(x.Id)).ToList();
                                                if (ProductTaxListdb.Count() > 0)
                                                {
                                                    _unitOfWork.SalesOfferProductTaxes.DeleteRange(ProductTaxListdb);
                                                    var ResultOfRemove = _unitOfWork.Complete();
                                                }

                                                foreach (var ItemTax in SalesOfferProduct.SalesOfferProductTaxsList)
                                                {
                                                    if (ItemTax.ID != null && ItemTax.ID != 0)
                                                    {
                                                        var OfferProductTaxDb = _unitOfWork.SalesOfferProductTaxes.FindAll(a => a.Id == ItemTax.ID).FirstOrDefault();
                                                        if (OfferProductTaxDb != null)
                                                        {
                                                            OfferProductTaxDb.TaxId = ItemTax.TaxID;
                                                            OfferProductTaxDb.SalesOfferProductId = (long)SalesOfferProduct.Id;
                                                            OfferProductTaxDb.Percentage = ItemTax.Percentage ?? 0;
                                                            OfferProductTaxDb.Value = ItemTax.Value;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Insert
                                                        var OfferProductTaxDb = new SalesOfferProductTax();
                                                        OfferProductTaxDb.TaxId = ItemTax.TaxID;
                                                        OfferProductTaxDb.SalesOfferProductId = (long)SalesOfferProduct.Id;
                                                        OfferProductTaxDb.Percentage = ItemTax.Percentage ?? 0;
                                                        OfferProductTaxDb.Value = ItemTax.Value;

                                                        _unitOfWork.SalesOfferProductTaxes.Add(OfferProductTaxDb);
                                                    }
                                                    _unitOfWork.Complete();
                                                }
                                            }
                                            else // delete all taxes
                                            {
                                                var ProductTaxListdb = _unitOfWork.SalesOfferProductTaxes.FindAll(x => x.SalesOfferProductId == SalesOfferProduct.Id ).ToList();
                                                if (ProductTaxListdb.Count() > 0)
                                                {
                                                    _unitOfWork.SalesOfferProductTaxes.DeleteRange(ProductTaxListdb);
                                                    var ResultOfRemove = _unitOfWork.Complete();
                                                }
                                            }
                                        }
                                    }





                                    // Add-Delete Sales Offer Item Attachment
                                    if (SalesOfferProduct.SalesOfferProductAttachments != null)
                                    {
                                        if (SalesOfferProduct.SalesOfferProductAttachments.Count() > 0)
                                        {
                                            CompanyName = CompanyName.ToLower();
                                            foreach (var attachment in SalesOfferProduct.SalesOfferProductAttachments)
                                            {
                                                if (attachment.Id != null && attachment.Active == false)
                                                {
                                                    // Delete
                                                    var OfferProductAttachmentDb = _unitOfWork.SalesOfferItemAttachments.FindAll(a => a.SalesOfferProductId == attachment.Id).FirstOrDefault();
                                                    if (OfferProductAttachmentDb != null)
                                                    {
                                                        var AttachmentPath = Path.Combine(_host.WebRootPath, OfferProductAttachmentDb.AttachmentPath);

                                                        if (File.Exists(AttachmentPath))
                                                        {
                                                            File.Delete(AttachmentPath);
                                                            _unitOfWork.SalesOfferItemAttachments.Delete(OfferProductAttachmentDb);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    var FilePath = Common.SaveFile("~/Attachments/" + CompanyName + "/Offers/" + SalesOfferId, attachment.FileContent, attachment.FileName, attachment.FileExtension, _host);

                                                    // Insert

                                                    var SalesOfferAttachmentInserting = new SalesOfferItemAttachment()
                                                    {
                                                        OfferId = SalesOfferId,
                                                        InventoryItemId = (long)SalesOfferProduct.InventoryItemId,
                                                        AttachmentPath = FilePath,
                                                        CreatedBy = CreatedBy,
                                                        CreationDate = DateTime.Now,
                                                        Modified = DateTime.Now,
                                                        ModifiedBy = ModifiedBy,
                                                        Active = true,
                                                        FileName = attachment.FileName,
                                                        FileExtenssion = attachment.FileExtension,
                                                        Category = attachment.Category,
                                                        SalesOfferProductId = SalesOfferProduct.Id
                                                    };

                                                    _unitOfWork.SalesOfferItemAttachments.Add(SalesOfferAttachmentInserting);

                                                    var SalesOfferAttachmentInsert = _unitOfWork.Complete();
                                                }
                                            }
                                        }
                                    }
                                }

                                //Deleted Invoices + Deleted InvoiceItems + Inserted InvoiceItems + Update Project Price
                                if (SalesOfferDbStatus != null && SalesOfferDbStatus.ToLower() == "closed")
                                {
                                    //Deleted InvoiceItems when he delete OfferProduct (Active = false)
                                    var SalesOfferInvoicesDb = _unitOfWork.Invoices.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true).ToList();
                                    if (SalesOfferInvoicesDb != null && SalesOfferInvoicesDb.Count > 0)
                                    {
                                        var OldInvoiceForOffer = SalesOfferInvoicesDb.FirstOrDefault();
                                        var SalesOfferInvoicesIds = _unitOfWork.Invoices.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true).Select(a => a.Id).ToList();
                                        var SalesOfferInvoiceItemsDb = _unitOfWork.InvoiceItems.FindAll(a => SalesOfferInvoicesIds.Contains(a.InvoiceId)).ToList();

                                        var DeletedOfferProducts = Request.SalesOfferProductList.Where(a => a.Active == false).Select(a => a.Id).ToList();
                                        if (DeletedOfferProducts != null && DeletedOfferProducts.Count > 0)
                                        {
                                            var DeletedInvoiceItems = SalesOfferInvoiceItemsDb.Where(a => DeletedOfferProducts.Contains(a.SalesOfferProductId)).ToList();
                                            if (DeletedInvoiceItems != null && DeletedInvoiceItems.Count > 0)
                                            {
                                                foreach (var invoiceItem in DeletedInvoiceItems)
                                                {
                                                    _unitOfWork.InvoiceItems.Delete(invoiceItem);
                                                    _unitOfWork.Complete();
                                                }
                                            }
                                        }

                                        //Update new added InvoiceItems for new OfferProducts with new InvoiceId 
                                        var UpdatedOfferProducts = Request.SalesOfferProductList.Where(a => a.Active != false && a.Id != null).ToList();
                                        if (UpdatedOfferProducts != null && UpdatedOfferProducts.Count > 0)
                                        {
                                            var UpdatedOfferProductsIds = UpdatedOfferProducts.Select(a => a.Id).ToList();
                                            var UpdatedInvoiceItems = SalesOfferInvoiceItemsDb.Where(a => UpdatedOfferProductsIds.Contains(a.SalesOfferProductId)).ToList();
                                            if (UpdatedInvoiceItems != null && UpdatedInvoiceItems.Count > 0)
                                            {
                                                foreach (var offerProduct in UpdatedOfferProducts)
                                                {
                                                    var InvoiceItemDb = UpdatedInvoiceItems.Where(a => a.SalesOfferProductId == offerProduct.Id).FirstOrDefault();
                                                    var InvoiceDb = SalesOfferInvoicesDb.Where(a => a.ClientId == offerProduct.InvoicePayerClientId && a.SalesOfferId == SalesOfferId).FirstOrDefault();
                                                    if (InvoiceItemDb != null && InvoiceDb != null)
                                                    {
                                                        InvoiceItemDb.InvoiceId = InvoiceDb.Id;
                                                        _unitOfWork.Complete();
                                                    }
                                                }
                                            }
                                        }

                                        //When change PayerId Check Deleted Invoices And Inserted Invoices Then Delete Old and Insert New
                                        var clientsIds = Request.SalesOfferProductList.Where(a => a.Active == true).Select(a => a.InvoicePayerClientId).Distinct().ToList();
                                        var invoiceClientsIds = SalesOfferInvoicesDb.Select(a => a.ClientId).ToList();

                                        var InvoicesDeletedIds = invoiceClientsIds.Where(db => clientsIds.All(nw => nw != db)).ToList();
                                        var InvoicesToInsertIds = clientsIds.Where(nw => invoiceClientsIds.All(db => db != nw)).ToList();

                                        //Delete Old Invoices
                                        if (InvoicesDeletedIds != null && InvoicesDeletedIds.Count > 0)
                                        {
                                            var DeletedInvoices = SalesOfferInvoicesDb.Where(a => InvoicesDeletedIds.Contains(a.ClientId)).ToList();
                                            if (DeletedInvoices != null && DeletedInvoices.Count > 0)
                                            {
                                                foreach (var invoice in DeletedInvoices)
                                                {
                                                    var InvoiceItems = SalesOfferInvoiceItemsDb.Where(a => a.InvoiceId == invoice.Id).ToList();
                                                    if (InvoiceItems != null && InvoiceItems.Count > 0)
                                                    {
                                                        foreach (var invoiceItem in InvoiceItems)
                                                        {
                                                            _unitOfWork.InvoiceItems.Delete(invoiceItem);
                                                            _unitOfWork.Complete();
                                                        }
                                                    }
                                                    _unitOfWork.Invoices.Delete(invoice);
                                                }
                                            }
                                        }
                                        if (InvoicesToInsertIds != null && InvoicesToInsertIds.Count > 0)
                                        {
                                            foreach (var clientId in InvoicesToInsertIds)
                                            {
                                                var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
                                                var OfferClientApprovalDate = SalesOfferDb.ClientApprovalDate;
                                                DateTime InvoiceDate = OfferClientApprovalDate ?? DateTime.Now;
                                                long InvoiceId = 0;

                                                var InvoiceInserting = new Invoice()
                                                {
                                                    Serial = "1",
                                                    Revision = 0,
                                                    InvoiceDate = InvoiceDate,
                                                    InvoiceType = OldInvoiceForOffer.InvoiceType,
                                                    ClientId = clientId,
                                                    CreatedBy = creator,
                                                    CreationDate = DateTime.Now,
                                                    ModifiedBy = creator,
                                                    ModificationDate = DateTime.Now,
                                                    Active = true,
                                                    IsClosed = false,
                                                    CreationType = OldInvoiceForOffer.CreationType,
                                                    InvoiceFor = OldInvoiceForOffer.InvoiceFor,
                                                    EInvoiceId = null,
                                                    EInvoiceStatus = null,
                                                    EInvoiceAcceptDate = null,
                                                    SalesOfferId = SalesOfferId,
                                                    EInvoiceJsonBody = null,
                                                    EInvoiceRequestToSend = false
                                                };
                                                _unitOfWork.Invoices.Add(InvoiceInserting);
                                                var InvoiceInsert = _unitOfWork.Complete();

                                                if (InvoiceInsert > 0)
                                                {
                                                    InvoiceId = InvoiceInserting.Id;
                                                    int SerialTemp = 0;
                                                    var SerialList = _unitOfWork.Invoices.GetAll().Select(x => x.Serial).ToList();
                                                    int Serial = SerialList.Where(x => int.TryParse(x, out SerialTemp)).OrderByDescending(x => int.Parse(x)).Select(x => int.Parse(x)).FirstOrDefault();
                                                    //int SerialNo = string.IsNullOrEmpty(Serial) && int.TryParse(Serial, out SerialNo) ? 1 : int.Parse(Serial) + 1;
                                                    var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.Id == InvoiceId).FirstOrDefault();
                                                    if (InvoiceDB != null)
                                                    {
                                                        InvoiceDB.Serial = (Serial + 1).ToString();
                                                        _unitOfWork.Complete();
                                                    }
                                                    var ClientInvoicesItemList = _unitOfWork.SalesOfferProducts.FindAll(x => x.OfferId == SalesOfferId && x.InvoicePayerClientId == clientId).ToList();
                                                    // Insert Into Invoice Items
                                                    if (ClientInvoicesItemList.Count > 0)
                                                    {
                                                        foreach (var invoiceItem in ClientInvoicesItemList)
                                                        {
                                                            _unitOfWork.InvoiceItems.Add(new InvoiceItem()
                                                            {
                                                                InvoiceId = InvoiceId,
                                                                SalesOfferProductId = invoiceItem.Id,
                                                                Comments = null,
                                                                EInvoiceId = null,
                                                                EInvoiceAcceptDate = null,
                                                                EInvoiceStatus = null
                                                            });
                                                            _unitOfWork.Complete();
                                                        }
                                                    }

                                                }

                                            }
                                        }
                                        var SalesOfferProductsListDB = _unitOfWork.SalesOfferProducts.FindAll(item => item.OfferId == SalesOfferId).ToList();

                                        var ProductIDSListFromInvoiceItemList = SalesOfferInvoiceItemsDb.Select(x => x.SalesOfferProductId).ToList();
                                        // Check if New product added and not have invoice item
                                        var ProductIDSListNotAdded = SalesOfferProductsListDB.Where(item => !ProductIDSListFromInvoiceItemList.Contains(item.Id)).Select(x => x.Id).ToList();
                                        //if (SalesOfferInvoiceItemsDb.Count() < Request.SalesOfferProductList.Where(x=>x.Active == true).Count())
                                        if (ProductIDSListNotAdded.Count() > 0)
                                        {
                                            var ProductNotAddedInvoiceList = SalesOfferProductsListDB.Where(x => ProductIDSListNotAdded.Contains(x.Id)).ToList();
                                            foreach (var SalesOfferProduct in ProductNotAddedInvoiceList)
                                            {
                                                var InvoiceId = SalesOfferInvoicesDb.Where(x => x.SalesOfferId == SalesOfferId && x.ClientId == SalesOfferProduct.InvoicePayerClientId).Select(x => x.Id).FirstOrDefault();
                                                if (InvoiceId != 0)
                                                {
                                                    _unitOfWork.InvoiceItems.Add(new InvoiceItem()
                                                    {
                                                        InvoiceId = InvoiceId,
                                                        SalesOfferProductId = SalesOfferProduct.Id,
                                                        Comments = null,
                                                        EInvoiceId = null,
                                                        EInvoiceAcceptDate = null,
                                                        EInvoiceStatus = null
                                                    });
                                                    _unitOfWork.Complete();
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // Add-Delete Sales Offer Attachment
                        if (Request.SalesOfferAttachmentList != null)
                        {
                            if (Request.SalesOfferAttachmentList.Count() > 0)
                            {
                                CompanyName = CompanyName.ToLower();
                                foreach (var attachment in Request.SalesOfferAttachmentList)
                                {
                                    if (attachment.Id != null && attachment.Active == false)
                                    {
                                        // Delete
                                        var OfferAttachmentDb = _unitOfWork.SalesOfferAttachments.GetById((long)attachment.Id);
                                        if (OfferAttachmentDb != null)
                                        {
                                            var AttachmentPath = Path.Combine(_host.WebRootPath, OfferAttachmentDb.AttachmentPath);

                                            if (File.Exists(AttachmentPath))
                                            {
                                                File.Delete(AttachmentPath);
                                                _unitOfWork.SalesOfferAttachments.Delete(OfferAttachmentDb);
                                                var OfferAttachmentDelete = _unitOfWork.Complete();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var FilePath = Common.SaveFile("~/Attachments/" + CompanyName + "/Offers/" + SalesOfferId, attachment.FileContent, attachment.FileName, attachment.FileExtension, _host);

                                        // Insert
                                        var SalesOfferAttachmentInserting = new SalesOfferAttachment()
                                        {
                                            OfferId = SalesOfferId,
                                            AttachmentPath = FilePath,
                                            CreatedBy = CreatedBy == 0 ? ModifiedBy : CreatedBy,
                                            CreationDate = DateTime.Now,
                                            Modified = DateTime.Now,
                                            ModifiedBy = null,
                                            Active = true,
                                            FileName = attachment.FileName,
                                            FileExtenssion = attachment.FileExtension,
                                            Category = attachment.Category
                                        };

                                        var SalesOfferAttachmentInsert = _unitOfWork.Complete();
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

        public BaseResponseWithId<long> AddNewSalesOfferForInternalTicket(AddNewSalesOfferForInternalTicketRequest Request, string companyname, long creator)
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

                            if (Request.SalesOffer.OfferType != "Internal Ticket return")
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

                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Invalid Sales Offer Data!!";
                        Response.Errors.Add(error);
                    }
                    if (Request.SalesOfferProductList == null || Request.SalesOfferProductList.Count() == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "must be one item at least in offer";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    if (Request.SalesOfferProductList != null)
                    {
                        if (Request.SalesOfferProductList.Count() > 0)
                        {

                            var InventoryItemListIDS = Request.SalesOfferProductList.Select(x => x.InventoryItemId).ToList();

                            int Counter = 0;
                            foreach (var SalesOfferProduct in Request.SalesOfferProductList)
                            {
                                Counter++;
                                if (Request.SalesOffer.ParentSalesOfferID != null && Request.SalesOffer.ParentSalesOfferID > 0)
                                {

                                        var ParentProductcDb = _unitOfWork.SalesOfferProducts.Find((x=>x.OfferId == Request.SalesOffer.ParentSalesOfferID));
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

                    // Get the timezone information for Egypt
                    TimeZoneInfo egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
                    // Get the current datetime in Egypt
                    DateTime egyptDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, egyptTimeZone);

                    if (Response.Result)
                    {
                        long SalesOfferId = 0;
                        // Add-Edit Sales Offer
                        if (Request.SalesOffer.Id == null || Request.SalesOffer.Id == 0)
                        {

                            var NewOfferSerial = "";
                            var OfferSerialSubString = "";

                            //long newOfferNumber = 0;
                            long CountOfSalesOfferThisYear = _unitOfWork.SalesOffers.Count(x => x.Active == true && x.OfferSerial.Contains(System.DateTime.Now.Year.ToString()) &&
                            (x.OfferType == "Internal Ticket return" || x.OfferType == "Internal Ticket"));
                            
                            //if (companyname.ToLower() == "marinaplt")
                            //{
                            //    if (Request.SalesOffer.OfferType == "New Project Offer")
                            //    {
                            //        NewOfferSerial = "S";
                            //        OfferSerialSubString = "-RFQ-#";
                            //    }
                            //    else if (Request.SalesOffer.OfferType == "New Maintenance Offer")
                            //    {
                            //        NewOfferSerial = "M";
                            //        OfferSerialSubString = "-RFM-#";
                            //    }
                            //    else if (Request.SalesOffer.OfferType == "New Rent Offer")
                            //    {
                            //        NewOfferSerial = "R";
                            //        OfferSerialSubString = "-RentOffer-#";
                            //    }
                            //    else if (Request.SalesOffer.OfferType == "New Internal Order")
                            //    {
                            //        NewOfferSerial = "I";
                            //        OfferSerialSubString = "-RFQ-#";
                            //    }

                            //    var branchName = Common.GetBranchName(Request.SalesOffer.BranchId, _Context);

                            //    if (branchName == "Alexandria")
                            //    {
                            //        NewOfferSerial += "-A";
                            //    }
                            //    else if (branchName == "Cairo")
                            //    {
                            //        NewOfferSerial += "-C";
                            //    }
                            //    else if (branchName == "Factory")
                            //    {
                            //        NewOfferSerial += "-F";
                            //    }
                            //    else if (branchName == "Show Room")
                            //    {
                            //        NewOfferSerial += "-S.R";
                            //    }
                            //    else if (branchName == "Main Office")
                            //    {
                            //        NewOfferSerial += "-M.O";
                            //    }

                            //    //if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                            //    //{
                            //    //    string strLastOfferNumber = lastSalesOfferSerial.Split('-')[4];
                            //    //    newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                            //    //    NewOfferSerial += OfferSerialSubString + newOfferNumber + "-" + System.DateTime.Now.Year.ToString();
                            //    //}
                            //    //else
                            //    NewOfferSerial += OfferSerialSubString + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Year.ToString();
                            //}
                            //else if (companyname.ToLower() == "proauto")
                            //{
                            //    var lastSalesOfferSerial = _unitOfWork.SalesOffers.FindAll(a => a.Active == true).ToList().OrderByDescending(a => a.Id).Select(a => a.OfferSerial).FirstOrDefault();
                            //    if (lastSalesOfferSerial != null && lastSalesOfferSerial.Contains(System.DateTime.Now.Year.ToString()))
                            //    {
                            //        var ListSplit = lastSalesOfferSerial.Split('-');
                            //        string strLastOfferNumber = ListSplit[0];
                            //        var newOfferNumber = long.Parse(strLastOfferNumber.Substring(1)) + 1;
                            //        NewOfferSerial += "#" + newOfferNumber + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();
                            //    }

                            //}
                            //else
                            //{
                            //}
                            NewOfferSerial += "#" + (CountOfSalesOfferThisYear + 1) + "-" + System.DateTime.Now.Month.ToString() + "-" + System.DateTime.Now.Year.ToString();

                            // Insert
                            var NewSalesOfferInsert = new SalesOffer()
                            {
                                StartDate = DateOnly.FromDateTime((DateTime)StartDate),
                                EndDate = DateOnly.FromDateTime((DateTime)EndDate),
                                Note = string.IsNullOrEmpty(Request.SalesOffer.Note) ? null : Request.SalesOffer.Note,
                                SalesPersonId = Request.SalesOffer.SalesPersonId,
                                CreatedBy = creator,
                                CreationDate = egyptDateTime,
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
                            _unitOfWork.SalesOffers.Add(NewSalesOfferInsert);
                            var SalesOfferInsert = _unitOfWork.Complete();

                            if (SalesOfferInsert != 0 && NewSalesOfferInsert.Id != 0)
                            {
                                SalesOfferId = (long)NewSalesOfferInsert.Id;
                                Response.Result = true;
                                Response.ID = SalesOfferId;
                                if (Request.SalesOffer.ParentSalesOfferID != null)
                                {
                                    long? ParentInvoiceID = Request.SalesOffer.ParentInvoiceID;
                                    if (ParentInvoiceID == null)
                                    {
                                        var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId == Request.SalesOffer.ParentSalesOfferID).FirstOrDefault();
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
                                        InvoiceCNAndDNObj.CreatedBy = creator; //CreatedBy ?? 1;
                                        InvoiceCNAndDNObj.CreationDate = egyptDateTime;
                                        InvoiceCNAndDNObj.ModifiedBy = creator; // CreatedBy ?? 1;
                                        InvoiceCNAndDNObj.ModificationDate = egyptDateTime;

                                        _unitOfWork.InvoiceCnandDns.Add(InvoiceCNAndDNObj);
                                        _unitOfWork.Complete();
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
                                                CreatedBy = creator, // long.Parse(Encrypt_Decrypt.Decrypt(Request.SalesOffer.CreatedBy, key)),
                                                CreationDate = egyptDateTime,
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
                                                        _unitOfWork.Complete();
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


                                    //When change PayerId Check Deleted Invoices And Inserted Invoices Then Delete Old and Insert New
                                    var clientsIds = Request.SalesOfferProductList.Where(a => a.Active == true).Select(a => a.InvoicePayerClientId).Distinct().ToList();

                                    var InvoicesToInsertIds = clientsIds.ToList();


                                    //Insert 

                                    if (InvoicesToInsertIds != null && InvoicesToInsertIds.Count > 0)
                                    {
                                        foreach (var clientId in InvoicesToInsertIds)
                                        {
                                            var SalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
                                            var OfferClientApprovalDate = SalesOfferDb.ClientApprovalDate;
                                            DateTime InvoiceDate = OfferClientApprovalDate ?? egyptDateTime;
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
                                                CreationDate = egyptDateTime,
                                                ModificationDate = egyptDateTime,
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
                                                var SerialList = _unitOfWork.Invoices.FindAll(x => x.Active == true).Select(x => x.Serial).ToList();
                                                int Serial = SerialList.Where(x => int.TryParse(x, out SerialTemp)).OrderByDescending(x => int.Parse(x)).Select(x => int.Parse(x)).FirstOrDefault();
                                                //int SerialNo = string.IsNullOrEmpty(Serial) && int.TryParse(Serial, out SerialNo) ? 1 : int.Parse(Serial) + 1;
                                                var InvoiceDB = _unitOfWork.Invoices.FindAll(x => x.Id == InvoiceId).FirstOrDefault();
                                                if (InvoiceDB != null)
                                                {
                                                    InvoiceDB.Serial = (Serial + 1).ToString();
                                                    _unitOfWork.Complete();
                                                }
                                                var ClientInvoicesItemList = _unitOfWork.SalesOfferProducts.FindAll(x => x.OfferId == SalesOfferId && x.InvoicePayerClientId == clientId).ToList();
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
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Invalid Id for SalesOffer POS";
                            Response.Errors.Add(error);
                            return Response;
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




        public BaseResponseWithId<long> ClosingSalesOffer(ClosingSalesOfferData Request, string CompanyName)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
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

                if (Response.Result)
                {
                    long SalesOfferId = 0;
                    if (Request.SalesOfferId == null || Request.SalesOfferId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid SalesOfferId.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        SalesOfferId = (long)Request.SalesOfferId;

                        if (SalesOfferId != 0)
                        {
                            var IsSalesOfferDbExist = _unitOfWork.SalesOffers.GetById(SalesOfferId);
                            if (IsSalesOfferDbExist == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "This Offer Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    //long CreatedBy = 0;
                    //if (string.IsNullOrEmpty(Request.CreatedBy))
                    //{
                    //    Response.Result = false;
                    //    Error error = new Error();
                    //    error.ErrorCode = "Err-P12";
                    //    error.ErrorMSG = "CreatedBy Is Mandatory.";
                    //    Response.Errors.Add(error);
                    //    return Response;
                    //}
                    //else
                    //{
                    //    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(Request.CreatedBy, key));
                    //}
                    var CheckProjectDB = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOfferId).FirstOrDefault();
                    if (CheckProjectDB != null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "This Sales Offer Id Already have Project.";
                        Response.Errors.Add(error);
                        return Response;

                    }
                    if (Response.Result)
                    {
                        Response.Result = CloseSalesOffer(SalesOfferId, CompanyName, validation.userID);
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


        //Not Used
        public GetSalesOfferDashboardResponse GetSalesOffersDashboard()
        {
            GetSalesOfferDashboardResponse Response = new GetSalesOfferDashboardResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var All = GetSalesOfferList(new GetSalesOfferListFilters(), "all").SalesOfferList;
                var pricing = All.SalesOfferList.Where(a => a.Status == "Pricing").ToList();
                var recieved = All.SalesOfferList.Where(a => a.Status == "Recieved").ToList();
                var clientApproval = All.SalesOfferList.Where(a => a.Status == "ClientApproval").ToList();
                var closed = All.SalesOfferList.Where(a => a.Status == "Closed").ToList();
                var rejected = All.SalesOfferList.Where(a => a.Status == "Rejected").ToList();
                if (Response.Result)
                {
                    Response.UnderPricingOffersList = GetSalesOfferList(new GetSalesOfferListFilters(), "Pricing").SalesOfferList;
                    Response.SendingOfferToClientOffersList = GetSalesOfferList(new GetSalesOfferListFilters(), "Recieved").SalesOfferList;
                    Response.WaitingClientApprovalOffersList = GetSalesOfferList(new GetSalesOfferListFilters(), "ClientApproval").SalesOfferList;
                    Response.ClosedOffersList = GetSalesOfferList(new GetSalesOfferListFilters(), "Closed").SalesOfferList;
                    Response.RejectedOffersList = GetSalesOfferList(new GetSalesOfferListFilters(), "Rejected").SalesOfferList;
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


        public GetSalesOfferInternalApprovalResponse GetSalesOfferInternalApproval([FromHeader] long SalesOfferId)
        {
            GetSalesOfferInternalApprovalResponse Response = new GetSalesOfferInternalApprovalResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {

                if (Response.Result)
                {
                    var SalesOfferInternalApprovalDb = _unitOfWork.SalesOfferInternalApprovals.FindAll(a => a.SalesOfferId == SalesOfferId && a.Active == true, includes: new[] { "ByUserNavigation", "User" }).ToList();
                    if (SalesOfferInternalApprovalDb != null && SalesOfferInternalApprovalDb.Count > 0)
                    {
                        var SalesOfferInternalApprovals = SalesOfferInternalApprovalDb.Select(internalApproval => new SalesOfferInternalApporvalData
                        {
                            Id = internalApproval.Id,
                            ByUserId = internalApproval.ByUser.ToString(),
                            ByUserName = internalApproval.ByUserNavigation == null ? null : ( internalApproval.ByUserNavigation?.FirstName + " " + internalApproval.ByUserNavigation?.LastName),
                            GroupId = internalApproval.GroupId,
                            GroupName = internalApproval.Group?.Name,
                            Comment = internalApproval.Comment,
                            Date = internalApproval.Date?.ToString("yyyy-MM-dd"),
                            Reply = internalApproval.Reply,
                            SalesOfferId = internalApproval.SalesOfferId,
                            Type = internalApproval.Type,
                            UserId = internalApproval.UserId?.ToString(),
                            UserName = internalApproval.User  == null ? null : (internalApproval.User?.FirstName + " " + internalApproval.User?.LastName)
                        }).ToList();

                        Response.SalesOfferInternalApprovals = SalesOfferInternalApprovals;
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

        public BaseResponseWithId<long> AddSalesOfferPricingDetails(AddNewSalesPricingDetailsData Request)
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

                    if (Request.SalesOfferId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err25";
                        error.ErrorMSG = "Sales Offer Created By Id Is Mandatory";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    string CreatedByString = null;
                    string ModifiedByString = null;
                    long? CreatedBy = 0;
                    long? ModifiedBy = 0;

                    if (Request.SalesOfferExtraCostList != null)
                    {
                        if (Request.SalesOfferExtraCostList.Count() > 0)
                        {
                            foreach (var ExtraCost in Request.SalesOfferExtraCostList)
                            {
                                if (ExtraCost.Id != 0 && ExtraCost.Id != null)
                                {
                                    if (ExtraCost.ModifiedBy != null)
                                    {
                                        ModifiedByString = ExtraCost.ModifiedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Extra Cost Product Modified By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)ModifiedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Extra Cost Modifier User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    if (ExtraCost.Amount == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "ExtraCostAmount is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (ExtraCost.ExtraCostTypeId == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "ExtraCostTypeId is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (ExtraCost.CreatedBy != null)
                                    {
                                        CreatedByString = ExtraCost.CreatedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Extra Cost Created By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Extra Cost Creator User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }
                    }

                    if (Request.SalesOfferTaxList != null)
                    {
                        if (Request.SalesOfferTaxList.Count() > 0)
                        {
                            foreach (var Tax in Request.SalesOfferTaxList)
                            {
                                if (Tax.Id != 0 && Tax.Id != null)
                                {
                                    if (Tax.ModifiedBy != null)
                                    {
                                        ModifiedByString = Tax.ModifiedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Tax Modified By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)ModifiedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Tax Modifier User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    if (Tax.TaxPercentage == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "TaxPercentage is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (Tax.TaxValue == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "TaxValue is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (Tax.CreatedBy != null)
                                    {
                                        CreatedByString = Tax.CreatedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Tax Created By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Tax Creator User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }
                    }

                    if (Request.SalesOfferDiscountList != null)
                    {
                        if (Request.SalesOfferDiscountList.Count() > 0)
                        {
                            foreach (var Discount in Request.SalesOfferDiscountList)
                            {
                                if (Discount.Id != 0 && Discount.Id != null)
                                {
                                    if (Discount.ModifiedBy != null)
                                    {
                                        ModifiedByString = Discount.ModifiedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Discount Modified By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)ModifiedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Discount Modifier User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    if (Discount.DiscountPercentage == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "DiscountPercentage is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (Discount.DiscountValue == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "DiscountValue is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (Discount.CreatedBy != null)
                                    {
                                        CreatedByString = Discount.CreatedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Discount Created By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Discount Creator User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }
                    }

                    if (Request.SalesOfferTermsAndConditionsList != null)
                    {
                        if (Request.SalesOfferTermsAndConditionsList.Count() > 0)
                        {
                            foreach (var term in Request.SalesOfferTermsAndConditionsList)
                            {
                                if (term.Id != 0 && term.Id != null)
                                {
                                    if (term.ModifiedBy != null)
                                    {
                                        ModifiedByString = term.ModifiedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Term Modified By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    ModifiedBy = long.Parse(Encrypt_Decrypt.Decrypt(ModifiedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)ModifiedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Term Modifier User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(term.TermName))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "TermName is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (string.IsNullOrEmpty(term.TermCategoryName))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "TermCategoryName is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (string.IsNullOrEmpty(term.TermDescription))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "TermDescription is required";
                                        Response.Errors.Add(error);
                                    }

                                    if (term.CreatedBy != null)
                                    {
                                        CreatedByString = term.CreatedBy;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Discount Created By Id Is Mandatory";
                                        Response.Errors.Add(error);
                                    }
                                    CreatedBy = long.Parse(Encrypt_Decrypt.Decrypt(CreatedByString, key));
                                    var user = _unitOfWork.Users.GetById((long)CreatedBy);
                                    if (user == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "ErrCRM1";
                                        error.ErrorMSG = "Discount Creator User Doesn't Exist!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                            }
                        }
                    }

                    if (Response.Result)
                    {

                        //Add-Edit Sales Offer Extra Cost
                        if (Request.SalesOfferExtraCostList != null)
                        {
                            if (Request.SalesOfferExtraCostList.Count() > 0)
                            {
                                foreach (var ExtraCost in Request.SalesOfferExtraCostList)
                                {
                                    long SalesOfferExtraCostId = 0;
                                    if (ExtraCost.Id != null && ExtraCost.Id != 0)
                                    {
                                        var SalesOfferExtraCostDb = _unitOfWork.SalesOfferExtraCosts.GetById((long)ExtraCost.Id);
                                        if (SalesOfferExtraCostDb != null)
                                        {
                                            SalesOfferExtraCostId = (long)ExtraCost.Id;
                                            // Update
                                            SalesOfferExtraCostDb.ExtraCostTypeId = ExtraCost.ExtraCostTypeId == null ? SalesOfferExtraCostDb.ExtraCostTypeId : (long)ExtraCost.ExtraCostTypeId;
                                            SalesOfferExtraCostDb.Amount = ExtraCost.Amount == null ? SalesOfferExtraCostDb.Amount : (decimal)ExtraCost.Amount;
                                            SalesOfferExtraCostDb.Active = true;
                                            SalesOfferExtraCostDb.ModifiedBy = ModifiedBy;
                                            SalesOfferExtraCostDb.ModifiedDate = DateTime.Now;
                                            SalesOfferExtraCostDb.InvoicePayerClientId = ExtraCost.InvoicePayerClientId == null ? SalesOfferExtraCostDb.InvoicePayerClientId : ExtraCost.InvoicePayerClientId;
                                            SalesOfferExtraCostDb.SalesOfferId = Request.SalesOfferId;
                                            _unitOfWork.SalesOfferExtraCosts.Update(SalesOfferExtraCostDb);
                                            var SalesOfferExtraCostUpdate = _unitOfWork.Complete();
                                            if (SalesOfferExtraCostUpdate > 0)
                                            {
                                                Response.Result = true;
                                                Response.ID = Request.SalesOfferId;
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Faild To Update this ExtraCost!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This ExtraCost Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        // Insert
                                        var salesOfferExtraCost = new SalesOfferExtraCost()
                                        {
                                            SalesOfferId = Request.SalesOfferId,
                                            ExtraCostTypeId = (long)ExtraCost.ExtraCostTypeId,
                                            Amount = (decimal)ExtraCost.Amount,
                                            Active = true,
                                            CreationDate = DateTime.Now,
                                            CreatedBy = (long)CreatedBy,
                                            ModifiedBy = CreatedBy,
                                            ModifiedDate = DateTime.Now,
                                            InvoicePayerClientId = ExtraCost.InvoicePayerClientId == null ? null : ExtraCost.InvoicePayerClientId
                                        };
                                        _unitOfWork.SalesOfferExtraCosts.Add(salesOfferExtraCost);
                                        var SalesOfferExtraCostInsert = _unitOfWork.Complete();

                                        if (SalesOfferExtraCostInsert > 0)
                                        {
                                            Response.Result = true;
                                            Response.ID = Request.SalesOfferId;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Insert this ExtraCost!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                        }

                        // Add-Edit Sales Offer Tax
                        if (Request.SalesOfferTaxList != null)
                        {
                            if (Request.SalesOfferTaxList.Count() > 0)
                            {
                                foreach (var Tax in Request.SalesOfferTaxList)
                                {
                                    long SalesOfferTaxId = 0;
                                    if (Tax.Id != null && Tax.Id != 0)
                                    {
                                        var SalesOfferTaxDb = _unitOfWork.SalesOfferInvoiceTaxes.GetById((long)Tax.Id);
                                        if (SalesOfferTaxDb != null)
                                        {
                                            SalesOfferTaxId = (long)Tax.Id;
                                            // Update
                                            SalesOfferTaxDb.SalesOfferId = Request.SalesOfferId;
                                            SalesOfferTaxDb.TaxPercentage = Tax.TaxPercentage == null ? SalesOfferTaxDb.TaxPercentage : (decimal)Tax.TaxPercentage;
                                            SalesOfferTaxDb.Active = true;
                                            SalesOfferTaxDb.ModifiedBy = ModifiedBy;
                                            SalesOfferTaxDb.ModifiedDate = DateTime.Now;
                                            SalesOfferTaxDb.TaxValue = Tax.TaxValue == null ? SalesOfferTaxDb.TaxValue : (decimal)Tax.TaxValue;
                                            SalesOfferTaxDb.TaxName = Tax.TaxName == null ? SalesOfferTaxDb.TaxName : Tax.TaxName;
                                            SalesOfferTaxDb.TaxType = Tax.TaxType == null ? SalesOfferTaxDb.TaxType : Tax.TaxType;
                                            SalesOfferTaxDb.InvoicePayerClientId = Tax.InvoicePayerClientId == null ? SalesOfferTaxDb.InvoicePayerClientId : Tax.InvoicePayerClientId;


                                            _unitOfWork.SalesOfferInvoiceTaxes.Update(SalesOfferTaxDb);
                                            var SalesOfferTaxUpdate = _unitOfWork.Complete();
                                            if (SalesOfferTaxUpdate > 0)
                                            {
                                                Response.Result = true;
                                                Response.ID = Request.SalesOfferId;
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Faild To Update this Tax!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Tax Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        // Insert
                                        var salesOfferTax = new SalesOfferInvoiceTax()
                                        {
                                            SalesOfferId = Request.SalesOfferId,
                                            TaxPercentage = (decimal)Tax.TaxPercentage,
                                            Active = true,
                                            CreatedBy = (long)CreatedBy,
                                            CreationDate = DateTime.Now,
                                            ModifiedBy = CreatedBy,
                                            ModifiedDate = DateTime.Now,
                                            TaxValue = (decimal)Tax.TaxValue,
                                            TaxName = Tax.TaxName,
                                            TaxType = Tax.TaxType,
                                            InvoicePayerClientId = Tax.InvoicePayerClientId
                                        };
                                        _unitOfWork.SalesOfferInvoiceTaxes.Add(salesOfferTax);

                                        var SalesOfferTaxInsert = _unitOfWork.Complete();

                                        if (SalesOfferTaxInsert > 0)
                                        {
                                            Response.Result = true;
                                            Response.ID = Request.SalesOfferId;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Insert this Tax!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                        }

                        // Add-Edit Sales Offer Discount
                        if (Request.SalesOfferDiscountList != null)
                        {
                            if (Request.SalesOfferDiscountList.Count() > 0)
                            {
                                foreach (var Discount in Request.SalesOfferDiscountList)
                                {
                                    long SalesOfferDiscountId = 0;
                                    if (Discount.Id != null && Discount.Id != 0)
                                    {
                                        var SalesOfferDiscountDb = _unitOfWork.SalesOfferDiscounts.GetById((long)Discount.Id);
                                        if (SalesOfferDiscountDb != null)
                                        {
                                            SalesOfferDiscountId = (long)Discount.Id;
                                            // Update
                                            SalesOfferDiscountDb.SalesOfferId = Request.SalesOfferId;
                                            SalesOfferDiscountDb.DiscountPercentage = Discount.DiscountPercentage == null ? SalesOfferDiscountDb.DiscountPercentage : (decimal)Discount.DiscountPercentage;
                                            SalesOfferDiscountDb.DiscountValue = Discount.DiscountValue == null ? SalesOfferDiscountDb.DiscountValue : (decimal)Discount.DiscountValue;
                                            SalesOfferDiscountDb.DiscountApproved = Discount.DiscountApproved == null ? SalesOfferDiscountDb.DiscountApproved : Discount.DiscountApproved;
                                            SalesOfferDiscountDb.ClientApproveDiscount = Discount.ClientApproveDiscount == null ? SalesOfferDiscountDb.ClientApproveDiscount : Discount.ClientApproveDiscount;
                                            SalesOfferDiscountDb.DiscountApprovedBy = Discount.DiscountApprovedBy == null ? SalesOfferDiscountDb.DiscountApprovedBy : Discount.DiscountApprovedBy;
                                            SalesOfferDiscountDb.Active = true;
                                            SalesOfferDiscountDb.InvoicePayerClientId = Discount.InvoicePayerClientId == null ? SalesOfferDiscountDb.InvoicePayerClientId : Discount.InvoicePayerClientId;
                                            SalesOfferDiscountDb.ModifiedBy = ModifiedBy;
                                            SalesOfferDiscountDb.ModificationDate = DateTime.Now;
                                            _unitOfWork.SalesOfferDiscounts.Update(SalesOfferDiscountDb);

                                            var SalesOfferDiscountUpdate = _unitOfWork.Complete();
                                            if (SalesOfferDiscountUpdate > 0)
                                            {
                                                Response.Result = true;
                                                Response.ID = Request.SalesOfferId;
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Faild To Update this Discount!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Discount Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        // Insert
                                        var salesOfferDiscount = new SalesOfferDiscount()
                                        {
                                            SalesOfferId = Request.SalesOfferId,
                                            DiscountPercentage = (decimal)Discount.DiscountPercentage,
                                            DiscountValue = (decimal)Discount.DiscountValue,
                                            DiscountApproved = false,
                                            ClientApproveDiscount = false,
                                            CreatedBy = (long)CreatedBy,
                                            CreationDate = DateTime.Now,
                                            ModifiedBy = CreatedBy,
                                            ModificationDate = DateTime.Now,
                                            Active = true,
                                            InvoicePayerClientId = Discount.InvoicePayerClientId
                                        };
                                        _unitOfWork.SalesOfferDiscounts.Add(salesOfferDiscount);

                                        var SalesOfferDiscountInsert = _unitOfWork.Complete();

                                        if (SalesOfferDiscountInsert > 0)
                                        {
                                            Response.Result = true;
                                            Response.ID = Request.SalesOfferId;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Insert this Discount!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                        }

                        // Add-Edit Sales Offer Discount
                        if (Request.SalesOfferTermsAndConditionsList != null)
                        {
                            if (Request.SalesOfferTermsAndConditionsList.Count() > 0)
                            {
                                foreach (var Term in Request.SalesOfferTermsAndConditionsList)
                                {
                                    long SalesOfferTermId = 0;
                                    if (Term.Id != null && Term.Id != 0)
                                    {
                                        var SalesOfferTermDb = _unitOfWork.SalesOfferTermsAndConditions.GetById((long)Term.Id);
                                        if (SalesOfferTermDb != null)
                                        {
                                            SalesOfferTermId = (long)Term.Id;
                                            // Update
                                            SalesOfferTermDb.SalesOfferId = Request.SalesOfferId;
                                            SalesOfferTermDb.TermsCategoryName = Term.TermCategoryName;
                                            SalesOfferTermDb.TermsName = Term.TermName;
                                            SalesOfferTermDb.TermsDescription = Term.TermDescription;
                                            SalesOfferTermDb.ModifiedBy = ModifiedBy;
                                            SalesOfferTermDb.ModificationDate = DateTime.Now;
                                            SalesOfferTermDb.Active = true;

                                            _unitOfWork.SalesOfferTermsAndConditions.Update(SalesOfferTermDb);

                                            var SalesOfferTermUpdate = _unitOfWork.Complete();
                                            if (SalesOfferTermUpdate > 0)
                                            {
                                                Response.Result = true;
                                                Response.ID = Request.SalesOfferId;
                                            }
                                            else
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err25";
                                                error.ErrorMSG = "Faild To Update this Term!!";
                                                Response.Errors.Add(error);
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "This Term Doesn't Exist!!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                    else
                                    {
                                        // Insert
                                        var salesOfferTerm = new SalesOfferTermsAndCondition()
                                        {
                                            SalesOfferId = Request.SalesOfferId,
                                            TermsCategoryName = Term.TermCategoryName,
                                            TermsName = Term.TermName,
                                            TermsDescription = Term.TermDescription,
                                            CreatedBy = (long)CreatedBy,
                                            ModifiedBy = (long)CreatedBy,
                                            Active = true,
                                            CreationDate = DateTime.Now,
                                            ModificationDate = DateTime.Now
                                        };
                                        _unitOfWork.SalesOfferTermsAndConditions.Add(salesOfferTerm);

                                        var SalesOfferTermInsert = _unitOfWork.Complete();

                                        if (SalesOfferTermInsert > 0)
                                        {
                                            Response.Result = true;
                                            Response.ID = Request.SalesOfferId;
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err25";
                                            error.ErrorMSG = "Faild To Insert this Term!!";
                                            Response.Errors.Add(error);
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

        public BaseResponseWithId<long> AddSalesOfferInternalApproval(AddNewSalesOfferInternalApprovalData Request)
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


                    string ByUserIdString = null;
                    string UserIdString = null;
                    long? ByUserId = 0;
                    long? UserId = 0;

                    if (Request.SalesOfferInternalApporvalList != null && Request.SalesOfferInternalApporvalList.Count > 0)
                    {
                        var Counter = 1;
                        foreach (var internalApproval in Request.SalesOfferInternalApporvalList)
                        {
                            if (internalApproval.SalesOfferId == 0)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "Item #" + Counter + ": Please Insert a Valid SalesOfferId.";
                                Response.Errors.Add(error);
                                return Response;
                            }
                            if (internalApproval.Id != 0 && internalApproval.Id != null)
                            {

                                if (!string.IsNullOrEmpty(internalApproval.Reply))
                                {
                                    if (string.IsNullOrEmpty(internalApproval.ByUserId))
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "Item #" + Counter + ": ByUserId Is Mandatory!";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        ByUserIdString = internalApproval.ByUserId;
                                        ByUserId = long.Parse(Encrypt_Decrypt.Decrypt(ByUserIdString, key));

                                        var SalesOfferInternalApprovalDB = _unitOfWork.SalesOfferInternalApprovals.GetById((long)internalApproval.Id);
                                        if (SalesOfferInternalApprovalDB != null)
                                        {
                                            if (SalesOfferInternalApprovalDB.UserId != null)
                                            {
                                                if (SalesOfferInternalApprovalDB.UserId != ByUserId)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err-315";
                                                    error.ErrorMSG = "Item #" + Counter + ": Wrong ByUserId!";
                                                    Response.Errors.Add(error);
                                                }
                                            }
                                            else if (SalesOfferInternalApprovalDB.GroupId != null)
                                            {
                                                var IsUserInGroup = _unitOfWork.GroupUsers.FindAll(a => a.UserId == ByUserId && a.GroupId == SalesOfferInternalApprovalDB.GroupId).FirstOrDefault();
                                                if (IsUserInGroup == null)
                                                {
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err-315";
                                                    error.ErrorMSG = "Item #" + Counter + ": This ByUserId Not In The Group!";
                                                    Response.Errors.Add(error);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Response.Result = false;
                                            Error error = new Error();
                                            error.ErrorCode = "Err-315";
                                            error.ErrorMSG = "Item #" + Counter + ": This Internal Approval Doesn't Exist!";
                                            Response.Errors.Add(error);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(internalApproval.UserId))
                                {
                                    if (internalApproval.GroupId == null)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-315";
                                        error.ErrorMSG = "Item #" + Counter + ": You Must Enter Either UserId Or GroupId";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    UserIdString = internalApproval.UserId;
                                    UserId = long.Parse(Encrypt_Decrypt.Decrypt(UserIdString, key));
                                }

                                if (string.IsNullOrEmpty(internalApproval.Type))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-315";
                                    error.ErrorMSG = "Item #" + Counter + ": Type is required";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }

                    if (Response.Result)
                    {
                        foreach (var internalApproval in Request.SalesOfferInternalApporvalList)
                        {
                            if (internalApproval.Id != null && internalApproval.Id != 0)
                            {
                                var SalesOfferInternalApprovalDb = _unitOfWork.SalesOfferInternalApprovals.GetById((long)internalApproval.Id);
                                if (SalesOfferInternalApprovalDb != null)
                                {
                                    // Update
                                    SalesOfferInternalApprovalDb.SalesOfferId = internalApproval.SalesOfferId;
                                    SalesOfferInternalApprovalDb.ByUser = string.IsNullOrEmpty(internalApproval.ByUserId) ? SalesOfferInternalApprovalDb.ByUser : long.Parse(Encrypt_Decrypt.Decrypt(internalApproval.ByUserId, key));
                                    SalesOfferInternalApprovalDb.Reply = internalApproval.Reply;
                                    SalesOfferInternalApprovalDb.Comment = internalApproval.Comment != null ? internalApproval.Comment : SalesOfferInternalApprovalDb.Comment;
                                    SalesOfferInternalApprovalDb.Date = internalApproval.Date != null ? DateOnly.FromDateTime(DateTime.Parse(internalApproval.Date)) : SalesOfferInternalApprovalDb.Date;
                                    SalesOfferInternalApprovalDb.Active = true;
                                    SalesOfferInternalApprovalDb.ModifiedDate = DateTime.Now;
                                    SalesOfferInternalApprovalDb.ModifiedBy = validation.userID ;
                                    _unitOfWork.SalesOfferInternalApprovals.Update(SalesOfferInternalApprovalDb);


                                    var SalesOfferInternalApprovalUpdate = _unitOfWork.Complete();
                                    if (SalesOfferInternalApprovalUpdate > 0)
                                    {
                                        Response.Result = true;
                                    }
                                    else
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err25";
                                        error.ErrorMSG = "Faild To Update this Internal Approval!!";
                                        Response.Errors.Add(error);
                                    }
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "This Internal Approval Doesn't Exist!!";
                                    Response.Errors.Add(error);
                                }
                            }
                            else
                            {
                                long? itemUserId = null;
                                if (!string.IsNullOrEmpty(internalApproval.UserId))
                                {
                                    itemUserId = long.Parse(Encrypt_Decrypt.Decrypt(internalApproval.UserId, key));
                                }
                                DateTime Date = DateTime.Now;
                                if (internalApproval.Date != null)
                                {
                                    Date = DateTime.Parse(internalApproval.Date);
                                }
                                // Insert
                                var salesOfferInternalApproval = new SalesOfferInternalApproval()
                                {
                                    SalesOfferId = internalApproval.SalesOfferId,
                                    Type = internalApproval.Type,
                                    UserId = itemUserId,
                                    GroupId = internalApproval.GroupId,
                                    CreatedBy = validation.userID,
                                    ModifiedBy = validation.userID,
                                    CreationDate = DateTime.Now,
                                    ModifiedDate = DateTime.Now,
                                    Comment = internalApproval.Comment,
                                    Date = DateOnly.FromDateTime(Date),
                                    Reply = null,
                                    ByUser = null
                                };
                                _unitOfWork.SalesOfferInternalApprovals.Add(salesOfferInternalApproval);

                                var SalesOfferInternalApprovalInsert = _unitOfWork.Complete();

                                if (SalesOfferInternalApprovalInsert > 0)
                                {
                                    Response.Result = true;
                                }
                                else
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err25";
                                    error.ErrorMSG = "Faild To Insert this Internal Approval!!";
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


        public BaseResponseWithId<long> DeleteInvoices(DeletedInvoices Request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    if (Request != null)
                    {
                        if (Request.InvoicesIds != null && Request.InvoicesIds.Count > 0)
                        {
                            if (!Common.CheckUserRole(creator, 122, _Context))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err-P12";
                                error.ErrorMSG = "This User Hasn't Permission To Delete Invoice";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                foreach (var invoiceId in Request.InvoicesIds)
                                {
                                    long InvoiceId = 0;
                                    if (invoiceId == 0)
                                    {
                                        Response.Result = false;
                                        Error error = new Error();
                                        error.ErrorCode = "Err-P12";
                                        error.ErrorMSG = "Please Insert a Valid InvoiceId.";
                                        Response.Errors.Add(error);
                                    }
                                    else
                                    {
                                        InvoiceId = invoiceId;

                                        if (InvoiceId != 0)
                                        {
                                            var IsInvoiceDbExist = _unitOfWork.Invoices.GetById(InvoiceId);
                                            if (IsInvoiceDbExist == null)
                                            {
                                                Response.Result = false;
                                                Error error = new Error();
                                                error.ErrorCode = "Err-P12";
                                                error.ErrorMSG = "This Invoice (" + InvoiceId + ") Doesn't Exist!!";
                                                Response.Errors.Add(error);
                                            }
                                            else
                                            {
                                                if (IsInvoiceDbExist.InvoiceType != "1")
                                                {
                                                    var ReturnOfferSerial = _unitOfWork.SalesOffers.FindAll(a => a.Id == IsInvoiceDbExist.SalesOfferId).Select(a => a.OfferSerial).FirstOrDefault();
                                                    Response.Result = false;
                                                    Error error = new Error();
                                                    error.ErrorCode = "Err-P12";
                                                    error.ErrorMSG = "You Cannot Delete This Invoice #(" + IsInvoiceDbExist.Serial + ") Because It Is A Return Invoice For Return Sales Offer #(" + ReturnOfferSerial + ")";
                                                    Response.Errors.Add(error);
                                                }
                                                else
                                                {
                                                    if (IsInvoiceDbExist.EInvoiceId != null)
                                                    {
                                                        Response.Result = false;
                                                        Error error = new Error();
                                                        error.ErrorCode = "Err-P12";
                                                        error.ErrorMSG = "You Cannot Delete This Invoice (" + InvoiceId + ") Because It Has E-Invoice!!";
                                                        Response.Errors.Add(error);
                                                    }
                                                    else
                                                    {
                                                        var CheckInvoiceCNandDN = _unitOfWork.InvoiceCnandDns.FindAll(a => a.ParentInvoiceId == InvoiceId).FirstOrDefault();
                                                        if (CheckInvoiceCNandDN != null)
                                                        {
                                                            var ReturnOfferSerial = CheckInvoiceCNandDN.SalesOffer.OfferSerial;
                                                            var ReturnOfferInvoiceSerial = CheckInvoiceCNandDN.SalesOffer.Invoices.FirstOrDefault().Serial;

                                                            Response.Result = false;
                                                            Error error = new Error();
                                                            error.ErrorCode = "Err-P12";
                                                            error.ErrorMSG = "You Cannot Delete This Invoice #(" + IsInvoiceDbExist.Serial + ") Because It Has Return Sales Offer #(" + ReturnOfferSerial + ") With Return Invoice #(" + ReturnOfferInvoiceSerial + ")";
                                                            Response.Errors.Add(error);
                                                        }
                                                    }
                                                }
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
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "InvoicesIds List Is Empty!!";
                            Response.Errors.Add(error);
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Request Must Be Not Null";
                        Response.Errors.Add(error);
                    }

                    if (Response.Result)
                    {
                        var InvoicesItemsIds = _unitOfWork.InvoiceItems.FindAll(a => Request.InvoicesIds.Contains(a.InvoiceId)).ToList();
                        foreach (var invoiceItemId in InvoicesItemsIds)
                        {
                            _unitOfWork.InvoiceItems.Delete(invoiceItemId);
                            var IsDeleted = _unitOfWork.Complete();
                        }
                        foreach (var invoiceId in Request.InvoicesIds)
                        {
                            var invoice = _unitOfWork.Invoices.GetById(invoiceId);
                            if (invoice != null)
                            {
                                _unitOfWork.Invoices.Delete(invoice);
                                var IsDeleted = _unitOfWork.Complete();
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

        public BaseResponseWithId<long> RejectClosedSalesOffer(RejectClosedSalesOfferData Request, long creator)
        {
            BaseResponseWithId<long> Response = new BaseResponseWithId<long>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
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

                if (Response.Result)
                {
                    long SalesOfferId = 0;

                    if (Request.SalesOfferId == null || Request.SalesOfferId == 0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-P12";
                        error.ErrorMSG = "Please Insert a Valid SalesOfferId.";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    else
                    {
                        SalesOfferId = (long)Request.SalesOfferId;

                        var IsSalesOfferDbExist = _unitOfWork.SalesOffers.GetById(SalesOfferId);
                        if (IsSalesOfferDbExist == null)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = "This Offer Doesn't Exist!!";
                            Response.Errors.Add(error);
                            return Response;
                        }
                        else
                        {
                            var OfferStatus = IsSalesOfferDbExist.Status;
                            if (OfferStatus == "Closed")
                            {
                                if (!Common.CheckUserRole(creator, 121, _Context))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "This User Hasn't Permission To Cancel Offers";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else
                            {
                                if (!Common.CheckUserRole(creator, 123, _Context))
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "This User Hasn't Permission To Cancel Offers";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }

                            var CheckHasReturnOffers = _unitOfWork.InvoiceCnandDns.FindAll(a => a.ParentSalesOfferId == SalesOfferId).Select(a => a.SalesOfferId).ToList();
                            if (CheckHasReturnOffers != null && CheckHasReturnOffers.Count > 0)
                            {
                                var ReturnOffersSerialsList = _unitOfWork.SalesOffers.FindAll(a => CheckHasReturnOffers.Contains(a.Id) && a.Status != "Rejected").Select(a => a.OfferSerial).ToList();
                                if (ReturnOffersSerialsList != null && ReturnOffersSerialsList.Count > 0)
                                {
                                    var ReturnOffersSerialsString = string.Join(" &", ReturnOffersSerialsList.Select(n => n).ToArray());

                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "You Cannot Reject This Offer Because It Has A Return Offer(s) #(" + ReturnOffersSerialsString.Replace(" &", " &" + System.Environment.NewLine) + ") If You Want To Reject This Offer Please Reject First Return Offers Then Try To Reject It";
                                    Response.Errors.Add(error);
                                    return Response;
                                }
                            }
                            else
                            {
                                var CheckEInvoiceDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId == SalesOfferId && !string.IsNullOrEmpty(x.EInvoiceId)).FirstOrDefault();
                                if (CheckEInvoiceDB != null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "You Cannot Reject This Offer Because It Has E-Invoice!!";
                                    Response.Errors.Add(error);
                                }

                                var CheckClosedProjectDB = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOfferId && x.Closed == true).FirstOrDefault();
                                if (CheckClosedProjectDB != null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "You Cannot Reject This Offer Because It Has A Closed Project!!";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }

                    if (Response.Result)
                    {
                        var IsSuccess = false;

                        var CheckSalesOfferDb = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();
                        string RejectionReason = "Cancled Offer(Closed) " + CheckSalesOfferDb.OfferSerial;

                        var CheckProjectDB = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOfferId).FirstOrDefault();
                        if (CheckProjectDB != null)
                        {
                            CheckProjectDB.Active = false;
                            CheckProjectDB.ModifiedBy = creator;
                            CheckProjectDB.ModifiedDate = DateTime.Now;
                            RejectionReason += ", & Deactivated Project(Open) #" + CheckProjectDB.ProjectSerial;

                            var CheckCostCenterDB = _unitOfWork.GeneralActiveCostCenters.FindAll(x => x.CategoryId == CheckProjectDB.Id).FirstOrDefault();
                            if (CheckCostCenterDB != null)
                            {
                                CheckCostCenterDB.Active = false;
                                CheckCostCenterDB.ModifiedBy = creator;
                                RejectionReason += ", Deactivated Cost Center #" + CheckCostCenterDB.Serial;
                            }
                        }

                        var CheckInvoiceDB = _Context.Invoices.Where(x => x.SalesOfferId == SalesOfferId).FirstOrDefault();

                        if (CheckInvoiceDB != null)
                        {
                            if (CheckInvoiceDB.EInvoiceId == null)
                            {
                                if (Request.DeleteInvoice)
                                {
                                    var isDeleted = DeleteInvoices(new DeletedInvoices { InvoicesIds = new List<long> { CheckInvoiceDB.Id } }, creator);
                                }
                                else
                                {

                                    CheckInvoiceDB.Active = false;
                                    CheckInvoiceDB.ModifiedBy = creator;
                                    RejectionReason += ", Deactivated Invoice #" + CheckInvoiceDB.Serial;

                                }
                            }
                        }


                        var CheckBomDB = _unitOfWork.Boms.FindAll(x => x.OfferId == SalesOfferId).ToList();
                        if (CheckBomDB != null && CheckBomDB.Count > 0)
                        {
                            RejectionReason += ", Deactivated BOM(s) #(";
                            foreach (var bom in CheckBomDB)
                            {
                                bom.Active = false;
                                bom.ModifiedBy = creator;
                                bom.ModifiedDate = DateTime.Now;
                                RejectionReason += bom.Serial + ",";
                            }
                            RejectionReason += ")";

                            var BomsIds = CheckBomDB.Select(a => a.Id).ToList();
                            var CheckBomPartitionsDb = _unitOfWork.Bompartitions.FindAll(a => BomsIds.Contains(a.Bomid)).ToList();
                            if (CheckBomPartitionsDb != null && CheckBomPartitionsDb.Count > 0)
                            {
                                foreach (var bomPartition in CheckBomPartitionsDb)
                                {
                                    bomPartition.Active = false;
                                    bomPartition.ModifiedBy = creator;
                                    bomPartition.ModifiedDate = DateTime.Now;
                                }

                                var PartitionsIds = CheckBomPartitionsDb.Select(a => a.Id).ToList();
                                var CheckBomPartitionItemsDb = _unitOfWork.BompartitionItems.FindAll(a => PartitionsIds.Contains(a.BompartitionId)).ToList();
                                if (CheckBomPartitionItemsDb != null && CheckBomPartitionItemsDb.Count > 0)
                                {
                                    foreach (var bomPartitionItem in CheckBomPartitionItemsDb)
                                    {
                                        bomPartitionItem.Active = false;
                                        bomPartitionItem.ModifiedBy = creator;
                                        bomPartitionItem.ModifiedDate = DateTime.Now;
                                    }
                                }
                            }
                        }

                        try
                        {
                            var ContextSuccess = _Context.SaveChanges();
                            IsSuccess = true;
                        }
                        catch (Exception ex)
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-P12";
                            error.ErrorMSG = ex.Message + " " + ex.InnerException;
                            Response.Errors.Add(error);
                        }

                        if (Request.DeleteAutomaticJE)
                        {
                            var OfferClientAccount = _unitOfWork.ClientAccounts.FindAll(a => a.OfferId == SalesOfferId).FirstOrDefault();
                            if (OfferClientAccount != null)
                            {
                                AddReverseDailyJournalEntryRequest JEBody = new AddReverseDailyJournalEntryRequest
                                {
                                    DailyJournalEntryId = OfferClientAccount.DailyAdjustingEntryId,
                                    IsReverse = false
                                };
                                var IsJEDeleted = _accountAndFinanceService.AddReverseDailyJournalEntry(JEBody, creator).Result.Result;

                                if (!IsJEDeleted)
                                {
                                    IsSuccess = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err-P12";
                                    error.ErrorMSG = "Failed To Delete JE";
                                    Response.Errors.Add(error);
                                }
                                else
                                {
                                    var JEserial = _unitOfWork.DailyJournalEntries.FindAll(a => a.Id == OfferClientAccount.DailyAdjustingEntryId).Select(a => a.Serial).FirstOrDefault();
                                    RejectionReason += ", Deleted JE #" + JEserial;
                                }
                            }
                        }

                        if (CheckSalesOfferDb != null)
                        {
                            CheckSalesOfferDb.Status = "Rejected";
                            CheckSalesOfferDb.RejectionReason = RejectionReason;
                            Response.ID = SalesOfferId;
                        }
                        _Context.SaveChanges();

                        Response.Result = IsSuccess;
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

        public GetSalesPersonSalesOfferListResponse GetSalesPersonSalesOfferList(GetSalesPersonSalesOfferListFilters filters, string OfferStatusParam)
        {
            GetSalesPersonSalesOfferListResponse Response = new GetSalesPersonSalesOfferListResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region Filters

                    if (!string.IsNullOrEmpty(filters.OfferType))
                    {
                        filters.OfferType = filters.OfferType.ToLower();
                    }

                    string ProductsListString = "";
                    List<long> ProductsIdsList = new List<long>();
                    if (!string.IsNullOrEmpty(filters.ProductsList))
                    {
                        ProductsListString = filters.ProductsList.ToString();
                        ProductsIdsList = ProductsListString.Split(',').Select(s => long.Parse(s.Trim())).ToList();
                    }

                    if (!string.IsNullOrEmpty(filters.ReleaseFilter))
                    {
                        filters.ReleaseFilter = filters.ReleaseFilter.ToLower();
                        if (filters.ReleaseFilter == "fully")
                        {
                            var FullyReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty != null && a.RemainQty != null && a.ReleasedQty >= a.RemainQty).Select(a => a.Id).ToList();
                            if (FullyReleasedProductsId.Count() > 0)
                            {
                                ProductsIdsList.AddRange(FullyReleasedProductsId);
                            }
                        }
                        else if (filters.ReleaseFilter == "partially")
                        {
                            var PartiallyReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty != null && a.RemainQty != null && a.ReleasedQty > 0 && a.ReleasedQty < a.RemainQty).Select(a => a.Id).ToList();
                            if (PartiallyReleasedProductsId.Count() > 0)
                            {
                                ProductsIdsList.AddRange(PartiallyReleasedProductsId);
                            }
                        }
                        else if (filters.ReleaseFilter == "not")
                        {
                            var NotReleasedProductsId = _unitOfWork.SalesOfferProducts.FindAll(a => a.ReleasedQty == null || a.ReleasedQty == 0).Select(a => a.Id).ToList();
                            if (NotReleasedProductsId.Count() > 0)
                            {
                                ProductsIdsList.AddRange(NotReleasedProductsId);
                            }
                        }
                        else
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Release Filter!!";
                            Response.Errors.Add(error);
                            Response.Result = false;
                        }
                    }

                    //(Standard, Special)
                    if (!string.IsNullOrEmpty(filters.ProductType))
                    {
                        filters.ProductType = filters.ProductType.ToLower();
                    }


                    if (!string.IsNullOrEmpty(filters.ClientName))
                    {
                        filters.ClientName = HttpUtility.UrlDecode(filters.ClientName).ToLower();
                    }

                    if (!string.IsNullOrEmpty(filters.ProjectName))
                    {
                        filters.ProjectName = HttpUtility.UrlDecode(filters.ProjectName).ToLower();
                    }

                    if (!string.IsNullOrEmpty(OfferStatusParam))
                    {
                        if (OfferStatusParam.ToLower() != "all")
                        {
                            filters.OfferStatus = OfferStatusParam.ToLower();
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(filters.OfferStatus))
                            {
                                filters.OfferStatus = filters.OfferStatus.ToLower();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(filters.ReminderDateFilter))
                    {
                        if (filters.OfferStatus != "" && filters.OfferStatus.ToLower() == "clientapproval")
                        {
                            filters.ReminderDateFilter = filters.ReminderDateFilter.ToLower();
                            if (filters.ReminderDateFilter != "delay" && filters.ReminderDateFilter != "today")
                            {
                                Error error = new Error();
                                error.ErrorCode = "Err-12";
                                error.ErrorMSG = "Invalid ReminderDate Filter!!";
                                Response.Errors.Add(error);
                                Response.Result = false;
                            }
                        }
                    }

                    var DateFilter = false;

                    if (filters.From != null || filters.To != null)
                    {
                        DateFilter = true;
                    }
                    #endregion
                    if (Response.Result)
                    {
                        var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "SalesPerson" });

                        if (filters.HasInvoice != null)
                        {
                            if ((bool)filters.HasInvoice)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Invoices.Count > 0);
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.Invoices.Count > 0));
                            }

                        }
                        if (filters.InvoiceDate != null)
                        {


                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Invoices.Count > 0 && x.Invoices.Any(a => DateOnly.FromDateTime(a.CreationDate) == DateOnly.FromDateTime((DateTime)filters.InvoiceDate)));

                        }

                        if (filters.HasProject != null)
                        {

                            if ((bool)filters.HasProject)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Projects.Count > 0);
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.Projects.Count > 0));
                            }

                        }
                        if (filters.ProjectDate != null)
                        {


                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Projects.Count > 0 && x.Projects.Any(a => DateOnly.FromDateTime(a.CreationDate) == DateOnly.FromDateTime((DateTime)filters.ProjectDate)));

                        }
                        if (filters.HasAutoJE != null)
                        {

                            if ((bool)filters.HasAutoJE)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.OfferId != null));
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => !(x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.OfferId != null)));
                            }

                        }
                        if (filters.HasJournalEntry != null)
                        {

                            if ((bool)filters.HasJournalEntry)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectId != null));
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectId == null));
                            }

                        }
                        if (filters.JournalEntryDate != null)
                        {


                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientAccounts.Count > 0 && x.ClientAccounts.Any(a => a.ProjectId != null && DateOnly.FromDateTime(a.CreationDate) == DateOnly.FromDateTime((DateTime)filters.JournalEntryDate)));

                        }


                        // supplier Name , Offer Serial ,Project Name
                        if (!string.IsNullOrEmpty(filters.SearchKey))
                        {
                            filters.SearchKey = HttpUtility.UrlDecode(filters.SearchKey);
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
                                                       (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(filters.SearchKey.ToLower()) : false)
                                                    || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(filters.SearchKey.ToLower()) : false))
                                                    //|| SalesOfferIDS.Contains(x.ID)
                                                    ).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.OfferType))
                        {
                            if (filters.OfferType == "new project offer")
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == "new project offer" || a.OfferType.ToLower() == "direct sales").AsQueryable();
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.OfferType.ToLower() == filters.OfferType).AsQueryable();
                            }
                        }
                        if (!string.IsNullOrEmpty(filters.OfferStatus))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Status.ToLower() == filters.OfferStatus).AsQueryable();

                            if (filters.ReminderDateFilter != "")
                            {
                                DateTime TodayDate = DateTime.Now.Date;
                                if (filters.ReminderDateFilter == "today")
                                {
                                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ReminderDate == TodayDate).AsQueryable();
                                    SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ReminderDate);
                                }
                                else if (filters.ReminderDateFilter == "delay")
                                {
                                    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ReminderDate < TodayDate).AsQueryable();
                                }
                            }
                        }
                        if (filters.SalesPersonId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                        }
                        if (filters.BranchId != 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ProductType))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProductType.ToLower() == filters.ProductType).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ClientName))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filters.ClientName)).AsQueryable();
                        }
                        if (!string.IsNullOrEmpty(filters.ProjectName))
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(filters.ProjectName)).AsQueryable();
                        }
                        if (DateFilter)
                        {
                            if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= filters.From && a.ClientApprovalDate <= filters.To).AsQueryable();
                            }
                            else
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime((DateTime)filters.From) && a.EndDate <= DateOnly.FromDateTime((DateTime)filters.To)).AsQueryable();
                            }
                        }
                        else
                        {
                            if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                            {
                                var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                            }
                        }
                        if (ProductsIdsList.Count > 0)
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesOfferProducts.Any(x => ProductsIdsList.Contains(x.InventoryItemId ?? 0))).AsQueryable();
                        }

                        if (filters.OfferStatus == "closed" || filters.OfferStatus == "rejected")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate);
                        }
                        else
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
                        }

                        var SalesOfferGroupedDBQuery = SalesOfferDBQuery.Where(a => a.Active == true).GroupBy(x => new { x.SalesPersonId, x.SalesPerson.FirstName, x.SalesPerson.LastName })
                            .Select(SalesOffer => new SalesPersonTotalSalesOffer
                            {
                                SalesPersonId = SalesOffer.Key.SalesPersonId,
                                SalesPersonName = SalesOffer.Key.FirstName + " " + SalesOffer.Key.LastName,
                                SalesOfferCount = SalesOffer.Count(),
                                SumFinalOfferPrice = SalesOffer.Select(x => x.FinalOfferPrice).Sum()
                            }).OrderByDescending(a => a.SalesPersonName).AsQueryable();

                        var OffersListDB = PagedList<SalesPersonTotalSalesOffer>.Create(SalesOfferGroupedDBQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);

                        var TotalOffersCount = OffersListDB.Sum(x => x.SalesOfferCount);
                        //var TotalOffersPriceWithReturned = SalesOfferDBQuery.Sum(a => a.FinalOfferPrice) ?? 0;
                        //var TotalOffersReturnedPrice = SalesOfferDBQuery.Where(a => a.OfferType == "Sales Return").Sum(a => a.FinalOfferPrice) ?? 0;
                        //var TotalOffersPrice = TotalOffersPriceWithReturned - 2 * TotalOffersReturnedPrice;
                        var TotalOffersPrice = OffersListDB.Sum(x => x.SumFinalOfferPrice);
                        SalesPersonSalesOfferDetails SalesPersonSalesOfferDetails = new SalesPersonSalesOfferDetails()
                        {
                            TotalOffersCount = TotalOffersCount,
                            TotalOffersPrice = TotalOffersPrice ?? 0,
                            TotalSalesOfferPerSalesPersonList = OffersListDB.ToList()
                        };

                        Response.Data = SalesPersonSalesOfferDetails;

                        Response.PaginationHeader = new PaginationHeader
                        {
                            CurrentPage = OffersListDB.CurrentPage,
                            TotalPages = OffersListDB.TotalPages,
                            ItemsPerPage = OffersListDB.PageSize,
                            TotalItems = OffersListDB.TotalCount
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

        public async Task<GetTargetOfLast5YearsResponse> GetTargetOfLast5Years()
        {
            GetTargetOfLast5YearsResponse response = new GetTargetOfLast5YearsResponse();
            response.Result = true;
            response.Errors = new List<Error>();
            try
            {
                var TargetLast5yearsList = new List<TargetLast5yearsData>();

                var YearDate = DateTime.Now.Year - 5;

                if (response.Result)
                {
                    if (response.Result)
                    {
                        var TargetLast5yearsDataDB = (await _unitOfWork.SalesTargets.FindAllAsync(x => x.Year >= YearDate)).ToList();


                        if (TargetLast5yearsDataDB != null && TargetLast5yearsDataDB.Count > 0)
                        {
                            var offers = _unitOfWork.VProjectSalesOfferClients.FindAll(x => x.ProjectActive == true && x.SalesOfferStatus == "Closed").ToList();
                            foreach (var TargetLast5yearsDataDBOBJ in TargetLast5yearsDataDB)
                            {
                                var TargetLast5yearsDataResponse = new TargetLast5yearsData();

                                TargetLast5yearsDataResponse.ID = (int)TargetLast5yearsDataDBOBJ.Id;

                                TargetLast5yearsDataResponse.Year = TargetLast5yearsDataDBOBJ.Year;

                                TargetLast5yearsDataResponse.Active = TargetLast5yearsDataDBOBJ.Active;

                                TargetLast5yearsDataResponse.PlannedTarget = TargetLast5yearsDataDBOBJ.Target;

                                TargetLast5yearsDataResponse.CanEdit = TargetLast5yearsDataDBOBJ.CanEdit;

                                TargetLast5yearsDataResponse.CurrencyID = TargetLast5yearsDataDBOBJ.CurrencyId;

                                var StartDate = new DateTime(TargetLast5yearsDataResponse.Year, 1, 1);


                                var EndDate = new DateTime(TargetLast5yearsDataResponse.Year + 1, 1, 1);


                                var SalesOfferPriceSumm = offers.Where(x => x.ProjectCreationDate >= StartDate && x.ProjectCreationDate < EndDate).Sum(x => x.FinalOfferPrice);

                                TargetLast5yearsDataResponse.AchievedTarget = SalesOfferPriceSumm != null ? (decimal)SalesOfferPriceSumm : 0;

                                TargetLast5yearsList.Add(TargetLast5yearsDataResponse);
                            }



                        }

                    }

                }
                response.TargetLast5yearsList = TargetLast5yearsList;
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


        public async Task<GetRejectedOfferResponse> GetRejectedOffer(GetRejectedOfferFilters filters)
        {
            GetRejectedOfferResponse Response = new GetRejectedOfferResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (filters.SupplierOfferId != null)
                {
                    var SupOffDb = await _unitOfWork.PRSupplierOffers.FindAsync(a => a.Id == filters.SupplierOfferId);
                    if (SupOffDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "This Supplier Offer Not Exist!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                //long PoId = 0;
                //if (!string.IsNullOrEmpty(headers["PoId"]) && long.TryParse(headers["PoId"], out PoId))
                //{
                //    long.TryParse(headers["PoId"], out PoId);
                //}

                //long PrId = 0;
                //if (!string.IsNullOrEmpty(headers["PrId"]) && long.TryParse(headers["PrId"], out PrId))
                //{
                //    long.TryParse(headers["PrId"], out PrId);
                //}

                //string Status = "";
                //if (!string.IsNullOrEmpty(headers["Status"]))
                //{
                //    Status = headers["Status"].ToString();
                //}

                if (Response.Result)
                {
                    var SupOfferListQ = _unitOfWork.PRSupplierOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "Supplier" }).AsQueryable();

                    if (!string.IsNullOrEmpty(filters.Status))
                    {
                        SupOfferListQ = SupOfferListQ.Where(a => a.Status.ToLower() == filters.Status.ToLower()).AsQueryable();
                    }
                    if (filters.PoId != 0 && filters.PoId!=null)
                    {
                        SupOfferListQ = SupOfferListQ.Where(a => a.Poid == filters.PoId).AsQueryable();
                    }
                    if (filters.PrId != 0 && filters.PrId!=null)
                    {
                        SupOfferListQ = SupOfferListQ.Where(a => a.Prid == filters.PrId).AsQueryable();
                    }

                    List<PrOffer> SupOfferListDb = SupOfferListQ.Select(off => new PrOffer{
                        PrOfferId = off.Id,
                        SupplierId = off.SupplierId,
                        SupplierName = off.Supplier.Name
                    }).ToList();

                    Response.PrOffers = SupOfferListDb;

                    var SupOfferDb = await _unitOfWork.PRSupplierOffers.FindAsync(a => a.Id == filters.SupplierOfferId, new[] { "PrsupplierOfferItems.Uom" , "PrsupplierOfferItems.InventoryItem", "PrsupplierOfferItems.Mritem", "PrsupplierOfferItems.InventoryItem.PurchasingUom", "PrsupplierOfferItems.InventoryItem.RequstionUom", "PrsupplierOfferItems.Currency" });
                    if (SupOfferDb != null)
                    {
                        PrSupplierOffer SupOfferResponse = new PrSupplierOffer
                        {
                            Id = SupOfferDb.Id,
                            Comment = SupOfferDb.Comment,
                            PoId = SupOfferDb.Poid,
                            PrId = SupOfferDb.Prid,
                            Status = SupOfferDb.Status,
                            SupplierId = SupOfferDb.SupplierId,
                            SupplierName = SupOfferDb.Supplier.Name
                        };
                        Response.PrSupplierOffer = SupOfferResponse;

                        if (SupOfferDb.PrsupplierOfferItems != null && SupOfferDb.PrsupplierOfferItems.Count > 0)
                        {
                            var SupOfferItemsResponse = SupOfferDb.PrsupplierOfferItems.Select(prItem => new PrSupplierOfferItem
                            {
                                ConversionRate = (decimal?)prItem.InventoryItem?.ExchangeFactor,
                                InventoryItemCode = prItem.InventoryItem?.Code,
                                InventoryItemId = prItem.InventoryItemId,
                                InventoryItemName = prItem.InventoryItem?.Name,
                                MrItemId = prItem.Mritem.Id,
                                PoId = prItem.Poid,
                                PoItemId = prItem.PoitemId,
                                PrId = prItem.Prid,
                                PrItemId = prItem.PritemId,
                                PurchasingUOMShortName = prItem.InventoryItem?.PurchasingUom.ShortName,
                                PurchasingUOMId = prItem.InventoryItem?.PurchasingUomid,
                                ReqQuantity = prItem.ReqQuantity,
                                RequstionUOMShortName = prItem.InventoryItem?.RequstionUom.ShortName,
                                RequstionUOMId = prItem.InventoryItem?.RequstionUomid,
                                Status = prItem.Status,
                                Comment = prItem.Comment,
                                CurrencyName = prItem.Currency.Name,
                                CurrencyId = prItem.CurrencyId,
                                EstimatedCost = prItem.EstimatedCost,
                                Id = prItem.Id,
                                InventoryItemPartNo = prItem.InventoryItem?.PartNo,
                                PRSupplierOfferId = prItem.PrsupplierOfferId,
                                RateToEGP = prItem.RateToEgp,
                                RecivedQuantity = prItem.RecivedQuantity,
                                TotalEstimatedCost = prItem.TotalEstimatedCost,
                                UOMId = prItem.Uomid,
                                UOMName = prItem.Uom.ShortName
                            }).ToList();
                            Response.PrSupplierOfferItems = SupOfferItemsResponse;
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

        public async Task<GetPrOfferItemHistoryResponse> GetPrOfferItemHistory(long? InventoryItemId)
        {
            GetPrOfferItemHistoryResponse Response = new GetPrOfferItemHistoryResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                
                if (InventoryItemId != null)
                {
                    var InvItmDb = await _unitOfWork.InventoryItems.FindAsync(a => a.Id == InventoryItemId);
                    if (InvItmDb == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err76";
                        error.ErrorMSG = "This Inventory Item Not Exist!";
                        Response.Errors.Add(error);
                        return Response;
                    }
                }

                if (Response.Result)
                {
                    var SupOfferItemsDb = await _unitOfWork.PRSupplierOfferItems.FindAllAsync(a => a.InventoryItemId == InventoryItemId, new[] { "Uom", "InventoryItem", "Mritem", "InventoryItem.PurchasingUom", "InventoryItem.RequstionUom", "Currency" });

                    if (SupOfferItemsDb != null && SupOfferItemsDb.Count() > 0)
                    {
                        var SupOfferItemsResponse = SupOfferItemsDb.Select(prItem => new PrSupplierOfferItem
                        {
                            ConversionRate = (decimal?)prItem.InventoryItem.ExchangeFactor,
                            InventoryItemCode = prItem.InventoryItem.Code,
                            InventoryItemId = prItem.InventoryItemId,
                            InventoryItemName = prItem.InventoryItem.Name,
                            MrItemId = prItem.Mritem.Id,
                            PoId = prItem.Poid,
                            PoItemId = prItem.PoitemId,
                            PrId = prItem.Prid,
                            PrItemId = prItem.PritemId,
                            PurchasingUOMShortName = prItem.InventoryItem.PurchasingUom.ShortName,
                            ReqQuantity = prItem.ReqQuantity,
                            RequstionUOMShortName = prItem.InventoryItem.RequstionUom.ShortName,
                            Status = prItem.Status,
                            Comment = prItem.Comment,
                            CurrencyName = prItem.Currency.Name,
                            CurrencyId = prItem.CurrencyId,
                            EstimatedCost = prItem.EstimatedCost,
                            Id = prItem.Id,
                            InventoryItemPartNo = prItem.InventoryItem.PartNo,
                            PRSupplierOfferId = prItem.PrsupplierOfferId,
                            RateToEGP = prItem.RateToEgp,
                            RecivedQuantity = prItem.RecivedQuantity,
                            TotalEstimatedCost = prItem.TotalEstimatedCost,
                            UOMId = prItem.Uomid,
                            UOMName = prItem.Uom.ShortName
                        }).ToList();
                        Response.PrSupplierOfferItemHistory = SupOfferItemsResponse;
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

        public async Task<BaseResponseWithID> AddEditRejectedSupplierOffer(AddEditSupplierOfferResponse Request, long UserID)
        {
            BaseResponseWithID Response = new BaseResponseWithID();
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
                        error.ErrorMSG = "please insert a valid data.";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    long prOfferId = 0;
                    long PrId = 0;
                    long? PoId = null;
                    string OfferStatus = "";

                    if (Request.PrSupplierOffer != null)
                    {
                        if (Request.PrSupplierOffer.Id != null)
                        {
                            var PrSupplierOfferDb = await _unitOfWork.PRSupplierOffers.FindAsync(a => a.Id == (long)Request.PrSupplierOffer.Id);
                            if (PrSupplierOfferDb == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "This Supplier Offer Doesn't Exist!!";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                prOfferId = (long)Request.PrSupplierOffer.Id;

                            }
                        }

                        if (Request.PrSupplierOffer.PrId != null)
                        {
                            PrId = (long)Request.PrSupplierOffer.PrId;
                        }
                        else
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Please Enter PrId";
                            Response.Errors.Add(error);
                        }

                        if (Request.PrSupplierOffer.PoId != null)
                        {
                            PoId = (long)Request.PrSupplierOffer.PoId;
                            OfferStatus = "Rejected";
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Request.PrSupplierOffer.Status))
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "Please Enter Supplier Offer Status";
                                Response.Errors.Add(error);
                            }
                            else
                            {
                                OfferStatus = Request.PrSupplierOffer.Status;
                            }
                        }

                        if (Request.PrSupplierOffer.SupplierId == 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "Err10";
                            error.ErrorMSG = "Please Enter SupplierId";
                            Response.Errors.Add(error);
                        }
                        else
                        {
                            var SupplierDb = await _unitOfWork.Suppliers.FindAsync(a => a.Id == Request.PrSupplierOffer.SupplierId && a.Active == true);
                            if (SupplierDb == null)
                            {
                                Response.Result = false;
                                Error error = new Error();
                                error.ErrorCode = "Err10";
                                error.ErrorMSG = "This Supplier Doesn't Exist!";
                                Response.Errors.Add(error);
                            }
                        }
                    }
                    else
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err10";
                        error.ErrorMSG = "Supplier Offer Can't Be Null";
                        Response.Errors.Add(error);
                    }

                    if (Request.PrSupplierOfferItems != null && Request.PrSupplierOfferItems.Count > 0)
                    {
                        foreach (var supOfrItm in Request.PrSupplierOfferItems)
                        {
                            if (supOfrItm.Id != null)
                            {
                                var PrSupplierOfferItemDb = await _unitOfWork.PRSupplierOfferItems.FindAsync(a => a.Id == (long)supOfrItm.Id);
                                if (PrSupplierOfferItemDb == null)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "The Supplier Offer Item with Id : " + supOfrItm.Id + "Doesn't Exist!";
                                    Response.Errors.Add(error);
                                }

                                if (supOfrItm.MrItemId == 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "MrItemId Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                                if (supOfrItm.PrItemId == 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "PRItemID Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                                if (supOfrItm.InventoryItemId == 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "InventoryItemID Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                                if (supOfrItm.UOMId == 0)
                                {
                                    Response.Result = false;
                                    Error error = new Error();
                                    error.ErrorCode = "Err10";
                                    error.ErrorMSG = "UOMID Is Mandatory";
                                    Response.Errors.Add(error);
                                }
                            }
                        }
                    }

                    if (Response.Result)
                    {
                        if (Request.PrSupplierOffer.Id != null)
                        {
                            var SupplierOfferDb = await _unitOfWork.PRSupplierOffers.FindAsync(a => a.Id == (long)Request.PrSupplierOffer.Id);
                            if (SupplierOfferDb != null)
                            {
                                SupplierOfferDb.ModifiedDate = DateTime.Now;
                                SupplierOfferDb.ModifiedBy = UserID;
                                SupplierOfferDb.Status = OfferStatus;
                                SupplierOfferDb.SupplierId = Request.PrSupplierOffer.SupplierId;
                            }
                        }
                        else
                        {
                            var SupplierOfferObj = new Infrastructure.Entities.PrsupplierOffer()
                            {
                                CreationDate = DateTime.Now,
                                ModifiedDate = DateTime.Now,
                                ModifiedBy = UserID,
                                CreatedBy = UserID,
                                Status = OfferStatus,
                                SupplierId = Request.PrSupplierOffer.SupplierId,
                                Poid = PoId,
                                Prid = PrId,
                                Active = true,
                                Comment = Request.PrSupplierOffer.Comment
                            };

                            _unitOfWork.PRSupplierOffers.Add(SupplierOfferObj);
                            await _Context.SaveChangesAsync();
                            prOfferId = SupplierOfferObj.Id;
                        }

                        if (Request.PrSupplierOfferItems != null && Request.PrSupplierOfferItems.Count > 0)
                        {
                            foreach (var SupOfrItm in Request.PrSupplierOfferItems)
                            {
                                if (SupOfrItm.Id != null && SupOfrItm.Id != 0)
                                {
                                    var SupplierOfferItemDb = await _unitOfWork.PRSupplierOfferItems.FindAsync(a => a.Id == SupOfrItm.Id);
                                    if (SupplierOfferItemDb != null)
                                    {
                                        SupplierOfferItemDb.Comment = SupOfrItm.Comment;
                                        SupplierOfferItemDb.PritemId = SupOfrItm.PrItemId;
                                        SupplierOfferItemDb.PoitemId = SupOfrItm.PoItemId;
                                        SupplierOfferItemDb.RateToEgp = SupOfrItm.RateToEGP;
                                        SupplierOfferItemDb.CurrencyId = SupOfrItm.CurrencyId;
                                        SupplierOfferItemDb.EstimatedCost = SupOfrItm.EstimatedCost;
                                        SupplierOfferItemDb.Status = PoId != null ? "Rejected" : SupOfrItm.Status;
                                        SupplierOfferItemDb.TotalEstimatedCost = SupOfrItm.TotalEstimatedCost;
                                        SupplierOfferItemDb.Poid = PoId;
                                        SupplierOfferItemDb.Prid = PrId;
                                        SupplierOfferItemDb.InventoryItemId = (long)SupOfrItm.InventoryItemId;
                                        SupplierOfferItemDb.MritemId = (long)SupOfrItm.MrItemId;
                                        SupplierOfferItemDb.RecivedQuantity = SupOfrItm.RecivedQuantity;
                                        SupplierOfferItemDb.ReqQuantity = SupOfrItm.ReqQuantity;
                                        SupplierOfferItemDb.Uomid = SupOfrItm.UOMId;
                                    }
                                }
                                else
                                {
                                    var SupplierOfferItem = new Infrastructure.Entities.PrsupplierOfferItem()
                                    {
                                        Comment = SupOfrItm.Comment,
                                        PritemId = SupOfrItm.PrItemId,
                                        PoitemId = SupOfrItm.PoItemId,
                                        RateToEgp = SupOfrItm.RateToEGP,
                                        CurrencyId = SupOfrItm.CurrencyId,
                                        EstimatedCost = SupOfrItm.EstimatedCost,
                                        Status = PoId != null ? "Rejected" : SupOfrItm.Status,
                                        TotalEstimatedCost = SupOfrItm.TotalEstimatedCost,
                                        Poid = PoId,
                                        Prid = PrId,
                                        InventoryItemId = (long)SupOfrItm.InventoryItemId,
                                        MritemId = (long)SupOfrItm.MrItemId,
                                        RecivedQuantity = SupOfrItm.RecivedQuantity,
                                        ReqQuantity = SupOfrItm.ReqQuantity,
                                        Uomid = SupOfrItm.UOMId,
                                        CreationDate = DateTime.Now,
                                        CreatedBy = UserID,
                                        PrsupplierOfferId = prOfferId
                                    };

                                    _unitOfWork.PRSupplierOfferItems.Add(SupplierOfferItem);
                                }
                            }
                        }

                        _unitOfWork.Complete();
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


        public async Task<BaseMessageResponse> GetSalesPersonsClientsDetailsExcel([FromHeader] int Month, [FromHeader] int Year, [FromHeader] int BranchId, [FromHeader] bool WithProjectExtraModifications)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    var EndDate = new DateTime((DateTime.Now.Year + 1), 1, 1);

                    if (Year > 0)
                    {
                        if (Month > 0)
                        {
                            StartDate = new DateTime(Year, Month, 1);

                            if (Month != 12)
                            {
                                EndDate = new DateTime(Year, (Month + 1), 1);
                            }
                            else
                            {
                                EndDate = new DateTime((Year + 1), 1, 1);
                            }
                        }
                        else
                        {
                            StartDate = new DateTime(Year, 1, 1);
                            EndDate = new DateTime((Year + 1), 1, 1);
                        }
                    }
                    else
                    {
                        if (Month > 0)
                        {
                            Response.Result = false;
                            Error error = new Error();
                            error.ErrorCode = "ErrCRM1";
                            error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                            Response.Errors.Add(error);

                            return Response;
                        }
                    }
                    var StartDateLastYear = StartDate.AddYears(-1);
                    var EndDateLastYear = EndDate.AddYears(-1);


                    var SalesPersons = (await _unitOfWork.VGroupUsers.FindAllAsync(a => a.Id == 4)).ToList();

                    int? Sp_BranchId = null;
                    if (BranchId != 0)
                    {
                        Sp_BranchId = BranchId;
                    }


                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[7] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Sales Person"),
                                                     new DataColumn("Supported By"),
                                                       new DataColumn("Old And New"),
                                                     new DataColumn("# Clients"),
                                                     new DataColumn("Clients Deals"),
                                                     new DataColumn("Number Of Deals"),
                                                     new DataColumn("Clients Project")



                    });


                    using (ExcelPackage packge = new ExcelPackage())
                    {
                        ExcelWorksheet ws = packge.Workbook.Worksheets.Add("Sheet1");
                        int StartCounter = 3;
                        foreach (var SP in SalesPersons)
                        {
                            var sp_salesPersonId = new SqlParameter("SalesPersonId", System.Data.SqlDbType.BigInt);
                            sp_salesPersonId.Value = SP.UserId;
                            var sp_branchId = new SqlParameter("BranchId", System.Data.SqlDbType.Int);
                            sp_branchId.Value = BranchId;
                            var sp_startdate = new SqlParameter("StartDate", System.Data.SqlDbType.DateTime);
                            sp_startdate.Value = StartDate;
                            var sp_enddate = new SqlParameter("EndDate", System.Data.SqlDbType.DateTime);
                            sp_enddate.Value = EndDate;
                            var sp_WithProjectExtraModifications = new SqlParameter("WithProjectExtraModifications", System.Data.SqlDbType.Bit);
                            sp_WithProjectExtraModifications.Value = WithProjectExtraModifications;
                            object[] param = new object[] { sp_salesPersonId, sp_branchId, sp_startdate, sp_enddate, sp_WithProjectExtraModifications };

                            var ClientsCRM = _Context.Database.SqlQueryRaw<STP_ClientsCRM_Result>("Exec STP_ClientsCRM @SalesPersonId ,@BranchId ,@StartDate ,@EndDate ,@WithProjectExtraModifications", param).AsEnumerable().ToList();
                            dt.Rows.Add(

                           SP.UserName,
                           "",
                           "",
                           "",
                           "",
                           "",
                           ""


                           );
                            int EmployeeCounter = ClientsCRM.Count();
                            foreach (var item in ClientsCRM)
                            {

                                bool NewClientSign = false;
                                bool TotalDealsPriceSign = false;

                                if (item.NewClientsCount > item.NewClientsCountLastYear)
                                {
                                    NewClientSign = true;
                                }

                                if (item.TotalDealsPriceNewClients > item.TotalDealsPriceNewClientsLastYear)
                                {
                                    TotalDealsPriceSign = true;
                                }




                                dt.Rows.Add(

                                 "",
                                 item.SupportedBy,
                                 "Old",
                                 item.OldClientsCount,
                                 item.OldClientsDeals,
                                 item.OldClientsProjectExtraModifications,
                                 item.OldDealedClients


                                 );
                                dt.Rows.Add(

                                    " ",
                                    " ",
                                     "New",
                                    item.NewClientsCount,
                                    item.NewClientsDeals,
                                    item.NewClientsProjectExtraModifications,
                                    item.NewDealedClients
                                    );


                            }
                            int EndCounter = StartCounter + (EmployeeCounter * 2);
                            for (var i = StartCounter; i <= EndCounter - 1; i++)
                            {

                                ws.Row(i).OutlineLevel = 1;

                                ws.Row(i).Collapsed = true;
                            }
                            StartCounter = EndCounter + 1;
                            ws.Cells["A1"].LoadFromDataTable(dt, true);
                            using (ExcelRange range = ws.Cells[1, 1, 1, 7])
                            {
                                range.Style.Font.Bold = true;
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      
                                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(4, 189, 189));
                                range.AutoFitColumns();
                            }

                            ws.Protection.IsProtected = false;
                            ws.Protection.AllowSelectLockedCells = false;




                        }

                        var CompanyName =validation.CompanyName.ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "ProjectDetails.xlsx";
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

        

        public async Task<BaseMessageResponse> GetSalesOfferDetailsPDF([FromHeader] long SalesOfferId)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SalesOfferDetailsVMObject = new SalesOfferDetailsVM();

                var SalesOfferProductsVMList = new List<SalesOfferProductsVM>();

                decimal? TotalTaxAmount = 0;
                decimal? TotalNetPrice = 0;
                decimal? TotalFinalUnitPrice = 0;

                //GetPurchasePOITemList.Add(ForSumObj);


                if (Response.Result)
                {
                    if(SalesOfferId==0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid SalesOfferId";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var SalesOffer = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId).FirstOrDefault();

                    if (SalesOffer == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "This Sales Offer ID doesn't exist !";
                        Response.Errors.Add(error);
                        return Response;
                    }

                    string CarModel = "";
                    string PlateNo = "";
                    string ChassisNumber = "";
                    string MaintenanceName = "";
                    string CarKiloMeter = "";
                    var VehiclePerClient = _unitOfWork.VehiclePerClients.FindAll(x => x.ClientId == SalesOffer.ClientId, includes: new[] { "Model" }).FirstOrDefault();
                    if (VehiclePerClient != null)
                    {
                        CarModel = VehiclePerClient.Model?.Name;
                        PlateNo = VehiclePerClient.PlatNumber;
                        ChassisNumber = VehiclePerClient.ChassisNumber;
                        var ProjectId = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOffer.Id).Select(x => x.Id).FirstOrDefault();
                        var VehicleMaintenanceJobOrderHistory = _unitOfWork.VehicleMaintenanceJobOrderHistories.FindAll(x => x.VehiclePerClientId == VehiclePerClient.Id && x.JobOrderProjectId == ProjectId).FirstOrDefault();
                        if (VehicleMaintenanceJobOrderHistory != null)
                        {
                            MaintenanceName = VehicleMaintenanceJobOrderHistory.VehicleMaintenanceType?.Name;
                            CarKiloMeter = VehicleMaintenanceJobOrderHistory.Milage?.ToString();
                        }

                    }
                    var SalesOfferProductsList = SalesOffer.SalesOfferProducts;
                    var ClientData = _unitOfWork.Clients.FindAll(x => x.Id == SalesOffer.ClientId).FirstOrDefault();
                    var MainClientData = _unitOfWork.Clients.FindAll(x => x.OwnerCoProfile == true).FirstOrDefault();
                    string MainCompanyAddress = "";
                    string TaxCard = "";
                    string FromCompanyName = "";
                    if (MainClientData != null)
                    {
                        var MainClientAdress = _unitOfWork.ClientAddresses.FindAll(x => x.ClientId == MainClientData.Id).FirstOrDefault();
                        FromCompanyName = MainClientData.Name;
                        var governerate = _unitOfWork.Governorates.GetById(MainClientAdress.GovernorateId);
                        TaxCard = MainClientData.TaxCard;
                        MainCompanyAddress = MainClientAdress.Address + " " + "Building No:" + " " + MainClientAdress.BuildingNumber + " " + governerate?.Name + " " + "," + "" + MainClientAdress.Country.Name;

                    }
                    SalesOfferDetailsVMObject.FromCompanyName = FromCompanyName;
                    SalesOfferDetailsVMObject.TaxCard = TaxCard;
                    SalesOfferDetailsVMObject.MainCompanyAddress = MainCompanyAddress;
                    SalesOfferDetailsVMObject.ClientName = SalesOffer.Client.Name;
                    SalesOfferDetailsVMObject.SalesOfferSerial = SalesOffer.OfferSerial;
                    SalesOfferDetailsVMObject.SalesOfferDate = SalesOffer.CreationDate.ToShortDateString();
                    SalesOfferDetailsVMObject.Address = ClientData.ClientAddresses.FirstOrDefault()?.Address;
                    if (ClientData.ClientPaymentTerms != null && ClientData.ClientPaymentTerms.Count > 0)
                    {
                        SalesOfferDetailsVMObject.TermsOfPayment = ClientData.ClientPaymentTerms?.FirstOrDefault().Percentage + " " + ClientData.ClientPaymentTerms?.FirstOrDefault().Name;
                    }




                    if (SalesOfferProductsList != null && SalesOfferProductsList.Count != 0)
                    {

                        foreach (var item in SalesOfferProductsList)
                        {
                            var SalesOfferProductsDB = new SalesOfferProductsVM();

                            SalesOfferProductsDB.ID = item.Id;
                            SalesOfferProductsDB.InventoryItems = item.InventoryItem.Name;
                            SalesOfferProductsDB.NetPrice = decimal.Parse(item.Quantity.ToString()) * item.ItemPrice;
                            SalesOfferProductsDB.Price = item.ItemPrice;
                            SalesOfferProductsDB.QTY = item.Quantity;
                            SalesOfferProductsDB.TaxAmount = item.SalesOfferProductTaxes.Sum(x => x.Value);
                            SalesOfferProductsDB.FinalUnitPrice = SalesOfferProductsDB.NetPrice + SalesOfferProductsDB.TaxAmount;
                            SalesOfferProductsDB.Description = item.ItemPricingComment;
                            TotalTaxAmount += item.SalesOfferProductTaxes.Sum(x => x.Value);
                            TotalNetPrice += SalesOfferProductsDB.NetPrice;
                            TotalFinalUnitPrice += SalesOfferProductsDB.FinalUnitPrice;


                            SalesOfferProductsVMList.Add(SalesOfferProductsDB);
                        }
                    }




                    //Start PDF Service


                    MemoryStream ms = new MemoryStream();

                    //Size of page
                   iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.LETTER);


                    PdfWriter pw = PdfWriter.GetInstance(document, ms);

                    //Call the footer Function

                    pw.PageEvent = new HeaderFooter2();

                    document.Open();

                    //Handle fonts and Sizes +  Attachments images logos 

                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.LETTER);

                    //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                    //document.SetMargins(0, 0, 20, 20);
                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.NORMAL);

                    //BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    //Font font = new Font(bf, 9, Font.NORMAL);

                    String path = _host.WebRootPath+"/Attachments";

                    //if (headers["CompanyName"].ToString() == "marinaplt")
                    //{
                    //    var PurchasePOInvoiceDB2 = new PurchasePOInvoiceVM();



                    //    string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                    //    //Image logo = Image.GetInstance(PDFp_strPath);
                    //    //logo.SetAbsolutePosition(80f, 50f);
                    //    //logo.ScaleAbsolute(600f,600f);



                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                    //    jpg.SetAbsolutePosition(60f, 750f);
                    //    //document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -20;



                    //    Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);

                    //    cc.SetBackground(new BaseColor(4, 189, 189), 220, 9, 15, 40);


                    //    prgHeading.Add(cc);

                    //    document.Add(prgHeading);

                    //}
                    //else if (headers["CompanyName"].ToString() == "piaroma")
                    //{

                    //    string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    //    Image logo = Image.GetInstance(Piaroma_p_strPath);
                    //    logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                    //    logo.ScaleAbsolute(300f, 300f);

                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                    //    document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 30, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -30;
                    //    prgHeading.SpacingAfter = 20;


                    //    Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);
                    //    cc.SetBackground(new BaseColor(4, 189, 189), 220, 9, 0, 25);


                    //    prgHeading.Add(cc);

                    //    document.Add(prgHeading);


                    //}
                    //else if (headers["CompanyName"].ToString() == "Garastest")
                    //{
                    //    string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    //    Image logo = Image.GetInstance(GarasTest_p_strPath);
                    //    logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                    //    logo.ScaleAbsolute(300f, 300f);


                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(GarasTest_p_strPath);
                    //    document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -10;



                    //    Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);
                    //    cc.SetBackground(new BaseColor(4, 189, 189), 180, 9, 5, 40);


                    //    prgHeading.Add(cc);


                    //    //prgHeading.Add(new Chunk("Inventory Store Item Report".ToUpper(), fntHead));

                    //    document.Add(prgHeading);
                    //}
                    if (MainClientData != null && MainClientData.HasLogo == true && MainClientData.LogoUrl != null)
                    {

                        var clientLogo = Globals.baseURL + MainClientData.LogoUrl;
                        ////string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                        //Image logo = Image.GetInstance(clientLogo);
                        //logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                        //logo.ScaleAbsolute(3000f, 3000f);
                        //logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(clientLogo);
                        jpg.ScaleAbsolute(210, 100);
                        //document.Add(logo);
                        document.Add(jpg);
                        //iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        //Paragraph prgHeading = new Paragraph();
                        //prgHeading.Alignment = Element.ALIGN_RIGHT;
                        ////prgHeading.SpacingBefore = -10;
                        //Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);
                        //cc.SetBackground(new BaseColor(4, 189, 189), 220,0, 0, 80);




                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 30, 1, iTextSharp.text.BaseColor.WHITE);
                        iTextSharp.text.Paragraph prgHeading = new iTextSharp.text.Paragraph();
                        prgHeading.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -30;
                        prgHeading.SpacingAfter = 20;


                        iTextSharp.text.Chunk cc = new iTextSharp.text.Chunk("I Sales Offer" + " ", fntHead);
                        cc.SetBackground(new iTextSharp.text.BaseColor(4, 189, 189), 140, 12, 0, 25);
                        prgHeading.Add(cc);

                        document.Add(prgHeading);



                        //Adding paragraph for report generated by  
                        iTextSharp.text.Paragraph prgGeneratedBY = new iTextSharp.text.Paragraph();
                        BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                        prgGeneratedBY.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;

                        document.Add(prgGeneratedBY);


                    }

                    //Adding a line  
                    //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                    //document.Add(p);






                    var dt2 = new System.Data.DataTable("grid");


                    dt2.Columns.AddRange(new DataColumn[8] { new DataColumn("No	"),
                                                     new DataColumn("Inventory Store Items"),
                                                     new DataColumn("QTY"),
                                                     new DataColumn("Price") ,
                                                      new DataColumn("Net Price"),
                                                     new DataColumn("Tax Amount"),
                                                     new DataColumn("Description"),
                                                     new DataColumn("Final Unit Price") ,

                    });
                    var Counter2 = 1;
                    var SalesOfferProductsListVM = SalesOfferProductsVMList;
                    if (SalesOfferProductsListVM != null)
                    {
                        foreach (var item in SalesOfferProductsListVM)
                        {

                            //string CounterString = Counter == 0 ? "" : Counter.ToString();

                            dt2.Rows.Add(
                                Counter2.ToString(),
                                item.InventoryItems != null ? item.InventoryItems : "-",
                                item.QTY != null ? String.Format("{0:0.000}", item.QTY) : "0",
                                item.Price != null ? String.Format("{0:0.000}", item.Price) : "0",
                                item.NetPrice != null ? String.Format("{0:0.000}", item.NetPrice) : "0",
                                item.TaxAmount != null ? String.Format("{0:0.000}", item.TaxAmount) : "0",
                                item.Description != null ? item.Description : "-",
                                item.FinalUnitPrice != null ? String.Format("{0:0.000}", item.FinalUnitPrice) : "0"



                                );
                            Counter2++;
                        }
                    }










                    PdfPTable tableHeading = new PdfPTable(4);

                    tableHeading.WidthPercentage = 100;

                    tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                    PdfPCell cellPurchasePOInvoiceTypeIDName = new PdfPCell();
                    string cellPurchasePOInvoiceTypeIDNametext = "I" + "Sales Offer";
                    cellPurchasePOInvoiceTypeIDName.Phrase = new iTextSharp.text.Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceTypeIDName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cellPurchasePOInvoiceTypeIDName.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceTypeIDName.BorderColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    cellPurchasePOInvoiceTypeIDName.PaddingBottom = 15;
                    cellPurchasePOInvoiceTypeIDName.PaddingTop = 15;
                    cellPurchasePOInvoiceTypeIDName.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);

                    PdfPCell cellPurchasePOIDNumber = new PdfPCell();
                    string cellPurchasePOIDNumbertext = " Sales Offer Serial" + " " + SalesOfferDetailsVMObject.SalesOfferSerial;
                    cellPurchasePOIDNumber.Phrase = new iTextSharp.text.Phrase(cellPurchasePOIDNumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOIDNumber.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cellPurchasePOIDNumber.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPurchasePOIDNumber.BorderColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    cellPurchasePOIDNumber.PaddingBottom = 15;
                    cellPurchasePOIDNumber.PaddingTop = 15;
                    cellPurchasePOIDNumber.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);



                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOIDNumber);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);

                    tableHeading.KeepTogether = true;



                    PdfPTable tablePO = new PdfPTable(4);

                    tablePO.WidthPercentage = 100;

                    tablePO.SetTotalWidth(new float[] { 100, 250, 100, 65 });









                    PdfPCell cellPONumber = new PdfPCell();
                    string cellPONumbertext = "Sales Offer To:";
                    cellPONumber.Phrase = new iTextSharp.text.Phrase(cellPONumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPONumber.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellPONumber.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPONumber.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellPONumber.PaddingTop = 15;


                    PdfPCell cellBPurchasePOInvoice = new PdfPCell();
                    string cellBPurchasePOInvoicetext = SalesOfferDetailsVMObject.ClientName;

                    //new Phrase(cellBPurchasePOInvoicetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellBPurchasePOInvoice.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellBPurchasePOInvoice.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellBPurchasePOInvoice.PaddingTop = 15;
                    cellBPurchasePOInvoice.Phrase = new iTextSharp.text.Phrase(cellBPurchasePOInvoicetext, font);
                    cellBPurchasePOInvoice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellBPurchasePOInvoice.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell cellPOType = new PdfPCell();
                    string cellPOTypetext = "";
                    cellPOType.Phrase = new iTextSharp.text.Phrase(cellPOTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPOType.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellPOType.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPOType.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellPOType.PaddingTop = 15;


                    PdfPCell cellPurchasePOInvoiceType = new PdfPCell();
                    string cellPurchasePOInvoiceTypetext = "";
                    cellPurchasePOInvoiceType.Phrase = new iTextSharp.text.Phrase(cellPurchasePOInvoiceTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceType.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellPurchasePOInvoiceType.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceType.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellPurchasePOInvoiceType.PaddingTop = 15;




                    #region Car Model ##################################################

                    PdfPCell cellCARMODEL = new PdfPCell();
                    string cellCARMODELtext = "Car Model:";
                    cellCARMODEL.Phrase = new iTextSharp.text.Phrase(cellCARMODELtext, font);
                    cellCARMODEL.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARMODEL.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARMODEL.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARMODEL.PaddingTop = 15;


                    PdfPCell cellCARModelRES = new PdfPCell();
                    cellCARModelRES.Phrase = new iTextSharp.text.Phrase(CarModel, font);
                    cellCARModelRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARModelRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARModelRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARModelRES.PaddingTop = 15;
                    cellCARModelRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell cellEMPTY = new PdfPCell();
                    cellEMPTY.Phrase = new iTextSharp.text.Phrase("", font);
                    cellEMPTY.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellEMPTY.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellEMPTY.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellEMPTY.PaddingTop = 15;


                    PdfPCell cellEMPTYRes = new PdfPCell();
                    cellEMPTYRes.Phrase = new iTextSharp.text.Phrase("", font);
                    cellEMPTYRes.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellEMPTYRes.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellEMPTYRes.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellEMPTYRes.PaddingTop = 15;
                    #endregion

                    #region Car Plate No ##################################################
                    PdfPCell cellCARPlateNo = new PdfPCell();
                    string cellCARPlateNotext = "Plate No:";
                    cellCARPlateNo.Phrase = new iTextSharp.text.Phrase(cellCARPlateNotext, font);
                    cellCARPlateNo.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARPlateNo.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARPlateNo.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARPlateNo.PaddingTop = 15;


                    PdfPCell cellCARPlateNoRES = new PdfPCell();
                    cellCARPlateNoRES.Phrase = new iTextSharp.text.Phrase(PlateNo, font);
                    cellCARPlateNoRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARPlateNoRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARPlateNoRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARPlateNoRES.PaddingTop = 15;
                    cellCARPlateNoRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car Chassis No ##################################################
                    PdfPCell cellCARChassisNo = new PdfPCell();
                    cellCARChassisNo.Phrase = new iTextSharp.text.Phrase("Chassis No:", font);
                    cellCARChassisNo.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARChassisNo.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARChassisNo.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARChassisNo.PaddingTop = 15;


                    PdfPCell cellCARChassisNoRES = new PdfPCell();
                    cellCARChassisNoRES.Phrase = new iTextSharp.text.Phrase(ChassisNumber, font);
                    cellCARChassisNoRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARChassisNoRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARChassisNoRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARChassisNoRES.PaddingTop = 15;
                    cellCARChassisNoRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car MaintenanceName  ##################################################
                    PdfPCell cellMaintenanceName = new PdfPCell();
                    cellMaintenanceName.Phrase = new iTextSharp.text.Phrase("Maintenance Name:", font);
                    cellMaintenanceName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellMaintenanceName.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellMaintenanceName.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellMaintenanceName.PaddingTop = 15;


                    PdfPCell cellMaintenanceNameRES = new PdfPCell();
                    cellMaintenanceNameRES.Phrase = new iTextSharp.text.Phrase(MaintenanceName, font);
                    cellMaintenanceNameRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellMaintenanceNameRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellMaintenanceNameRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellMaintenanceNameRES.PaddingTop = 15;
                    cellMaintenanceNameRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car CarKiloMeter  ##################################################
                    PdfPCell cellKiloMeter = new PdfPCell();
                    cellKiloMeter.Phrase = new iTextSharp.text.Phrase("KiloMeter:", font);
                    cellKiloMeter.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellKiloMeter.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellKiloMeter.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellKiloMeter.PaddingTop = 15;


                    PdfPCell cellKiloMeterRES = new PdfPCell();
                    cellKiloMeterRES.Phrase = new iTextSharp.text.Phrase(CarKiloMeter, font);
                    cellKiloMeterRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellKiloMeterRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellKiloMeterRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellKiloMeterRES.PaddingTop = 15;
                    cellKiloMeterRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion



                    tablePO.AddCell(cellPONumber);
                    tablePO.AddCell(cellBPurchasePOInvoice);
                    tablePO.AddCell(cellPOType);
                    tablePO.AddCell(cellPurchasePOInvoiceType);
                    if (!string.IsNullOrWhiteSpace(CarModel))
                    {
                        tablePO.AddCell(cellCARMODEL);
                        tablePO.AddCell(cellCARModelRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(ChassisNumber))
                    {
                        tablePO.AddCell(cellCARChassisNo);
                        tablePO.AddCell(cellCARChassisNoRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(PlateNo))
                    {
                        tablePO.AddCell(cellCARPlateNo);
                        tablePO.AddCell(cellCARPlateNoRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(MaintenanceName))
                    {
                        tablePO.AddCell(cellMaintenanceName);
                        tablePO.AddCell(cellMaintenanceNameRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(CarKiloMeter))
                    {
                        tablePO.AddCell(cellKiloMeter);
                        tablePO.AddCell(cellKiloMeterRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }


                    PdfPCell CellInvoiceDate = new PdfPCell();
                    string CellInvoiceDatetext = "Bill To: ";
                    CellInvoiceDate.Phrase = new iTextSharp.text.Phrase(CellInvoiceDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontD = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellInvoiceDate.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellInvoiceDate.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellInvoiceDate.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellInvoiceDate.PaddingTop = 10;


                    PdfPCell CellInvoiceDateText = new PdfPCell();
                    string CellInvoiceDateTexttext = SalesOfferDetailsVMObject.ClientName;
                    //CellInvoiceDateText.Phrase = new Phrase(CellInvoiceDateTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellInvoiceDateText.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellInvoiceDateText.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellInvoiceDateText.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellInvoiceDateText.PaddingTop = 10;
                    CellInvoiceDateText.Bottom = 15;
                    CellInvoiceDateText.Phrase = new iTextSharp.text.Phrase(CellInvoiceDateTexttext, font);
                    CellInvoiceDateText.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    PdfPCell CellTotalInvoicePrice = new PdfPCell();
                    string CellTotalInvoicePricetext = "";
                    CellTotalInvoicePrice.Phrase = new iTextSharp.text.Phrase(CellTotalInvoicePricetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontt = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellTotalInvoicePrice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTotalInvoicePrice.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalInvoicePrice.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellTotalInvoicePrice.PaddingTop = 10;
                    CellTotalInvoicePrice.Bottom = 15;


                    PdfPCell CellEmpty = new PdfPCell();
                    string CellEmptytext = "";
                    CellEmpty.Phrase = new iTextSharp.text.Phrase(CellEmptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellEmpty.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellEmpty.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellEmpty.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellEmpty.PaddingTop = 10;
                    CellEmpty.Bottom = 15;

                    tablePO.AddCell(CellInvoiceDate);
                    tablePO.AddCell(CellInvoiceDateText);
                    tablePO.AddCell(CellTotalInvoicePrice);
                    tablePO.AddCell(CellEmpty);





                    PdfPCell CellSupplier = new PdfPCell();
                    string CellSuppliertext = "Terms Of Payment: ";
                    CellSupplier.Phrase = new iTextSharp.text.Phrase(CellSuppliertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontK = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellSupplier.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellSupplier.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellSupplier.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellSupplier.PaddingTop = 10;


                    PdfPCell CellSupplierName = new PdfPCell();
                    string CellSupplierNametext = SalesOfferDetailsVMObject.TermsOfPayment;
                    CellSupplierName.Phrase = new iTextSharp.text.Phrase(CellSupplierNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontQ = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellSupplierName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellSupplierName.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellSupplierName.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellSupplierName.PaddingTop = 10;
                    CellSupplierName.Bottom = 15;


                    PdfPCell CellPOStatus = new PdfPCell();
                    string CellPOStatustext = "Registration Card:";
                    CellPOStatus.Phrase = new iTextSharp.text.Phrase(CellPOStatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontL = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellPOStatus.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellPOStatus.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellPOStatus.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellPOStatus.PaddingTop = 10;
                    CellPOStatus.Bottom = 15;


                    PdfPCell CellPOStatusText = new PdfPCell();
                    string CellPOStatusTexttext = SalesOfferDetailsVMObject.RegistrationCard != null ? SalesOfferDetailsVMObject.RegistrationCard : "N/A";
                    CellPOStatusText.Phrase = new iTextSharp.text.Phrase(CellPOStatusTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontM = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellPOStatusText.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellPOStatusText.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellPOStatusText.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellPOStatusText.PaddingTop = 10;
                    CellPOStatusText.Bottom = 15;

                    tablePO.AddCell(CellSupplier);
                    tablePO.AddCell(CellSupplierName);
                    tablePO.AddCell(CellPOStatus);
                    tablePO.AddCell(CellPOStatusText);





                    tablePO.SpacingAfter = 20;






                    PdfPTable table4 = new PdfPTable(4);

                    table4.WidthPercentage = 100;

                    table4.SetTotalWidth(new float[] { 55, 400, 100, 98 });


                    //table4.AddCell(CellVEmpty);
                    //table4.AddCell(CellV2);
                    //table4.AddCell(CellV3);
                    //table4.AddCell(CellV5);



                    //Adding PdfPTable


                    PdfPTable table = new PdfPTable(dt2.Columns.Count);


                    //table Width
                    table.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    table.SetTotalWidth(new float[] { 20, 50, 35, 35, 35, 35, 35, 35 });
                    table.PaddingTop = 20;

                    for (int i = 0; i < dt2.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new iTextSharp.text.Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;

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
                                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new iTextSharp.text.Phrase(1, dt2.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            table.AddCell(cell);

                        }

                    }







                    PdfPTable table3 = new PdfPTable(4);

                    table3.WidthPercentage = 100;

                    table3.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellDirector = new PdfPCell();
                    string CellDirectorText = "";
                    CellDirector.Phrase = new iTextSharp.text.Phrase(CellDirectorText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellDirector.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellDirector2 = new PdfPCell();
                    string CellDirectortext2 = "";
                    CellDirector2.Phrase = new iTextSharp.text.Phrase(CellDirectortext2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellDirector2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellDirector2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellDirector3 = new PdfPCell();
                    string CellDirector3text = "Price Details";
                    CellDirector3.Phrase = new iTextSharp.text.Phrase(CellDirector3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellDirector3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector3.BorderColor = iTextSharp.text.BaseColor.WHITE;




                    PdfPCell CellDirector4 = new PdfPCell();
                    string CellDirector4text = "";
                    CellDirector4.Phrase = new iTextSharp.text.Phrase(CellDirector4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontVs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellDirector4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector4.BorderColor = iTextSharp.text.BaseColor.WHITE;

                    table3.AddCell(CellDirector);
                    table3.AddCell(CellDirector2);
                    table3.AddCell(CellDirector3);
                    table3.AddCell(CellDirector4);









                    PdfPTable tablTotals = new PdfPTable(3);
                    tablTotals.WidthPercentage = 100;

                    tablTotals.SetTotalWidth(new float[] { 200, 200, 200 });


                    PdfPCell CelltablTotals1 = new PdfPCell();
                    string CelltablTotals1Text = "Final Net Price:";
                    CelltablTotals1.Phrase = new iTextSharp.text.Phrase(CelltablTotals1Text + " " + TotalNetPrice.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    CelltablTotals1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CelltablTotals1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CelltablTotals1.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    CelltablTotals1.PaddingBottom = 15;
                    CelltablTotals1.PaddingTop = 15;


                    PdfPCell CelltablTotals2 = new PdfPCell();
                    string CelltablTotals2text2 = "Tax Amount:";
                    CelltablTotals2.Phrase = new iTextSharp.text.Phrase(CelltablTotals2text2 + " " + TotalTaxAmount.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals2.PaddingBottom = 15;
                    CelltablTotals2.PaddingTop = 15;



                    CelltablTotals2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    CelltablTotals2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CelltablTotals2.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CelltablTotals2.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);


                    PdfPCell CelltablTotals3 = new PdfPCell();
                    string CelltablTotals3text = "Final Price Amount:";
                    CelltablTotals3.Phrase = new iTextSharp.text.Phrase(CelltablTotals3text + " " + TotalFinalUnitPrice.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    CelltablTotals3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CelltablTotals3.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CelltablTotals3.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    CelltablTotals3.PaddingBottom = 15;
                    CelltablTotals3.PaddingTop = 15;




                    tablTotals.AddCell(CelltablTotals1);
                    tablTotals.AddCell(CelltablTotals2);
                    tablTotals.AddCell(CelltablTotals3);

















                    PdfPTable tableOfferAmount = new PdfPTable(4);

                    tableOfferAmount.WidthPercentage = 100;

                    tableOfferAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellOfferAmount1 = new PdfPCell();
                    string CellOfferAmount1Text = "";
                    CellOfferAmount1.Phrase = new iTextSharp.text.Phrase(CellOfferAmount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNiS = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellOfferAmount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellOfferAmount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellOfferAmount2 = new PdfPCell();
                    string CellOfferAmount2text2 = "";
                    CellOfferAmount2.Phrase = new iTextSharp.text.Phrase(CellOfferAmount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSsss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellOfferAmount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellOfferAmount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellOfferAmount3 = new PdfPCell();
                    string CellOfferAmount3text = "Offer Amount:";
                    CellOfferAmount3.Phrase = new iTextSharp.text.Phrase(CellOfferAmount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWsssa = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellOfferAmount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellOfferAmount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellOfferAmount4 = new PdfPCell();
                    string CellOfferAmount4text = TotalNetPrice.ToString();
                    CellOfferAmount4.Phrase = new iTextSharp.text.Phrase(CellOfferAmount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellOfferAmount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellOfferAmount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableOfferAmount.AddCell(CellOfferAmount1);
                    tableOfferAmount.AddCell(CellOfferAmount2);
                    tableOfferAmount.AddCell(CellOfferAmount3);
                    tableOfferAmount.AddCell(CellOfferAmount4);








                    PdfPTable tableTotalDiscount = new PdfPTable(4);

                    tableTotalDiscount.WidthPercentage = 100;

                    tableTotalDiscount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellTotalDiscount1 = new PdfPCell();
                    string CellTotalDiscount1Text = "";
                    CellTotalDiscount1.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTotalDiscount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellTotalDiscount2 = new PdfPCell();
                    string CellTotalDiscount2text2 = "";
                    CellTotalDiscount2.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSDic = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellTotalDiscount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTotalDiscount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTotalDiscount3 = new PdfPCell();
                    string CellTotalDiscount3text = "Total Discount:";
                    CellTotalDiscount3.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWsDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTotalDiscount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTotalDiscount4 = new PdfPCell();
                    string CellTotalDiscount4text = "0.0";
                    CellTotalDiscount4.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFonDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTotalDiscount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableTotalDiscount.AddCell(CellTotalDiscount1);
                    tableTotalDiscount.AddCell(CellTotalDiscount2);
                    tableTotalDiscount.AddCell(CellTotalDiscount3);
                    tableTotalDiscount.AddCell(CellTotalDiscount4);








                    PdfPTable tableTaxAmount = new PdfPTable(4);

                    tableTaxAmount.WidthPercentage = 100;

                    tableTaxAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellTaxAmount1 = new PdfPCell();
                    string CellTaxAmount1Text = "";
                    CellTaxAmount1.Phrase = new iTextSharp.text.Phrase(CellTaxAmount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTaxAmount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellTaxAmount2 = new PdfPCell();
                    string CellTaxAmount2text2 = "";
                    CellTaxAmount2.Phrase = new iTextSharp.text.Phrase(CellTaxAmount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTaxAmount = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellTaxAmount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTaxAmount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTaxAmount3 = new PdfPCell();
                    string CellTaxAmount3text = "Tax Amount:";
                    CellTaxAmount3.Phrase = new iTextSharp.text.Phrase(CellTaxAmount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTaxAmount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTaxAmount4 = new PdfPCell();
                    string CellTaxAmount4text = TotalTaxAmount.ToString();
                    CellTaxAmount4.Phrase = new iTextSharp.text.Phrase(CellTaxAmount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicTax4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTaxAmount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableTaxAmount.AddCell(CellTaxAmount1);
                    tableTaxAmount.AddCell(CellTaxAmount2);
                    tableTaxAmount.AddCell(CellTaxAmount3);
                    tableTaxAmount.AddCell(CellTaxAmount4);









                    PdfPTable tableT4Amount = new PdfPTable(4);

                    tableT4Amount.WidthPercentage = 100;

                    tableT4Amount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellT4Amount1 = new PdfPCell();
                    string CellT4Amount1Text = "";
                    CellT4Amount1.Phrase = new iTextSharp.text.Phrase(CellT4Amount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1T4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT4Amount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT4Amount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellT4Amount2 = new PdfPCell();
                    string CellT4Amount2text2 = "";
                    CellT4Amount2.Phrase = new iTextSharp.text.Phrase(CellT4Amount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontT4Amount2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellT4Amount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT4Amount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT4Amount3 = new PdfPCell();
                    string CellT4Amount3text222 = "T4 Amount:";
                    CellT4Amount3.Phrase = new iTextSharp.text.Phrase(CellT4Amount3text222, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellT4Amount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT4Amount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT4Amount4 = new PdfPCell();
                    string CellT4Amount4text11 = "1121238236";
                    CellT4Amount4.Phrase = new iTextSharp.text.Phrase(CellT4Amount4text11, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellT4Amount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT4Amount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableT4Amount.AddCell(CellT4Amount1);
                    tableT4Amount.AddCell(CellT4Amount2);
                    tableT4Amount.AddCell(CellT4Amount3);
                    tableT4Amount.AddCell(CellT4Amount4);







                    PdfPTable tableT1Amount = new PdfPTable(4);

                    tableT1Amount.WidthPercentage = 100;

                    tableT1Amount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellT1Amount1 = new PdfPCell();
                    string CellT1Amount1Text = "";
                    CellT1Amount1.Phrase = new iTextSharp.text.Phrase(CellT1Amount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1T1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT1Amount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellT1Amount2 = new PdfPCell();
                    string CellT1Amount2text2 = "";
                    CellT1Amount2.Phrase = new iTextSharp.text.Phrase(CellT1Amount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontT1Amount2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellT1Amount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT1Amount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT1Amount3 = new PdfPCell();
                    string CellT1Amount3text = "T1 Amount:";
                    CellT1Amount3.Phrase = new iTextSharp.text.Phrase(CellT1Amount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontCellT1AAmount = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT1Amount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT1Amount4 = new PdfPCell();
                    string CellT1Amount4text = "1121238236";
                    CellT1Amount4.Phrase = new iTextSharp.text.Phrase(CellT1Amount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicTaxT14 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT1Amount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableT1Amount.AddCell(CellT1Amount1);
                    tableT1Amount.AddCell(CellT1Amount2);
                    tableT1Amount.AddCell(CellT1Amount3);
                    tableT1Amount.AddCell(CellT1Amount4);
























                    PdfPTable tableFinalOfferPriceAmount = new PdfPTable(4);

                    tableFinalOfferPriceAmount.WidthPercentage = 100;

                    tableFinalOfferPriceAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellFinalOfferPrice1 = new PdfPCell();
                    string CellFinalOfferPrice1Text = "";
                    CellFinalOfferPrice1.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellFinalOfferPrice1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellFinalOfferPrice2 = new PdfPCell();
                    string CellFinalOfferPrice2Text = "";
                    CellFinalOfferPrice2.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice2Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellFinalOfferPrice2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellFinalOfferPrice2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellFinalOfferPrice3 = new PdfPCell();
                    string CellFinalOfferPrice3text = "Final Offer Price:";
                    CellFinalOfferPrice3.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellFinalOfferPrice3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellFinalOfferPrice4 = new PdfPCell();
                    string CellFinalOfferPrice4text = TotalFinalUnitPrice.ToString();
                    CellFinalOfferPrice4.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellFinalOfferPrice4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice1);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice2);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice3);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice4);

























                    table.SpacingAfter = 20;

                    //if (SupplierPaymenDB != null)
                    //{
                    //    table5.AddCell(CellDirector5);
                    //}







                    iTextSharp.text.Paragraph parag = new iTextSharp.text.Paragraph(new iTextSharp.text.Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, iTextSharp.text.Element.ALIGN_CENTER, 1)));



                    PdfPTable FooterPart = new PdfPTable(1);

                    FooterPart.WidthPercentage = 100;



                    var CompanyName = validation.CompanyName;
                    PdfPCell FooterCell = new PdfPCell();
                    string FooterCelltext = FromCompanyName;

                    FooterCell.Phrase = new iTextSharp.text.Phrase(FooterCelltext, font);
                    // iTextSharp.text.Font FooterCellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    FooterCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    FooterCell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    FooterCell.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    FooterCell.PaddingTop = 15;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell FooterCell2 = new PdfPCell();


                    //var CompanyID = _Context.Clients.Where(x => x.OwnerCoProfile == true).FirstOrDefault();
                    //var CompanyAddress = _Context.ClientAddresses.Where(x => x.ClientID == CompanyID.ID).FirstOrDefault();

                    string FooterCelltext7 = MainCompanyAddress; // CompanyAddress.Address + " " + CompanyAddress.Country.Name + " " + "Building No:" + " " + CompanyAddress.BuildingNumber;
                    FooterCell2.Phrase = new iTextSharp.text.Phrase(FooterCelltext7, font);
                    FooterCell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    FooterCell2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    FooterCell2.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //FooterCell2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    PdfPCell FooterCell3 = new PdfPCell();
                    string FooterCelltext8 = "Tax ID:" + " " + TaxCard;

                    FooterCell3.Phrase = new iTextSharp.text.Phrase(FooterCelltext8, font);
                    iTextSharp.text.Font FooterCefont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    FooterCell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    FooterCell3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    FooterCell3.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    //FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    FooterPart.AddCell(FooterCell);
                    FooterPart.AddCell(FooterCell2);
                    if (!string.IsNullOrWhiteSpace(TaxCard))
                    {
                        FooterPart.AddCell(FooterCell3);

                    }
                    //var CompanyName = headers["CompanyName"].ToString().ToLower();
                    //PdfPCell FooterCell = new PdfPCell();
                    //string FooterCelltext = "From: " + " " + FromCompanyName;

                    //FooterCell.Phrase = new Phrase(FooterCelltext, font);
                    //iTextSharp.text.Font FooterCellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //FooterCell.HorizontalAlignment = Element.ALIGN_CENTER;
                    //FooterCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //FooterCell.BorderColor = BaseColor.WHITE;
                    //FooterCell.PaddingTop = 15;

                    //PdfPCell FooterCell2 = new PdfPCell();


                    ////var CompanyID = _Context.Clients.Where(x => x.OwnerCoProfile == true).FirstOrDefault();
                    //// var CompanyAddress = _Context.ClientAddresses.Where(x => x.ClientID == MainClientData.ID).FirstOrDefault();
                    //string FooterCelltext7 = MainCompanyAddress;
                    ////CompanyAddress.Address + " " + CompanyAddress.Country.Name + " " + "Building No:" + " " + CompanyAddress.BuildingNumber;
                    //FooterCell2.Phrase = new Phrase(FooterCelltext7, font);
                    //FooterCell2.HorizontalAlignment = Element.ALIGN_CENTER;
                    //FooterCell2.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //FooterCell2.BorderColor = BaseColor.WHITE;
                    //FooterCell2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    //PdfPCell FooterCell3 = new PdfPCell();
                    //string FooterCelltext8 = "Tax ID:" + " " + TaxCard;

                    //FooterCell3.Phrase = new Phrase(FooterCelltext8, font);
                    //iTextSharp.text.Font FooterCefont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //FooterCell3.HorizontalAlignment = Element.ALIGN_CENTER;
                    //FooterCell3.VerticalAlignment = Element.ALIGN_MIDDLE;
                    //FooterCell3.BorderColor = BaseColor.WHITE;



                    //FooterPart.AddCell(FooterCell);
                    //FooterPart.AddCell(FooterCell2);
                    //if (!string.IsNullOrWhiteSpace(TaxCard))
                    //{
                    //FooterPart.AddCell(FooterCell3);
                    //}





                    table3.SpacingBefore = 30;

                    table.SpacingAfter = 20;

                    document.Add(tableHeading);
                    document.Add(tablePO);
                    document.Add(table4);
                    document.Add(table);

                    document.Add(tablTotals);
                    tablTotals.SpacingAfter = 200;
                    document.Add(table3);
                    document.Add(tableOfferAmount);
                    document.Add(tableTotalDiscount);
                    document.Add(tableTaxAmount);


                    //  document.Add(tableT1Amount);
                    //  document.Add(tableT4Amount);


                    document.Add(tableFinalOfferPriceAmount);
                    //parag.SpacingBefore = 200;

                    document.Add(parag);

                    document.Add(FooterPart);


                    FooterPart.SpacingBefore = -100;

                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;



                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                    string PathsTR = "/Attachments/" + CompanyName + "/";
                    String Filepath = _host.WebRootPath+"/"+PathsTR;
                    //String Filepath = HttpContext.Current.Server.MapPath(PathsTR);
                    if (!System.IO.Directory.Exists(Filepath))
                    {
                        System.IO.Directory.CreateDirectory(Filepath); //Create directory if it doesn't exist
                    }
                    string p_strPath = Path.Combine(Filepath, FullFileName);

                    File.WriteAllBytes(p_strPath, result);

                    Response.Message = Globals.baseURL + PathsTR + FullFileName;
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


        public async Task<BaseMessageResponse> GetInvoiceDetailsPDF([FromHeader] long SalesOfferId)
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SalesOfferDetailsVMObject = new SalesOfferDetailsVM();

                var SalesOfferProductsVMList = new List<SalesOfferProductsVM>();

                decimal? TotalTaxAmount = 0;
                decimal? TotalNetPrice = 0;
                decimal? TotalFinalUnitPrice = 0;

                //GetPurchasePOITemList.Add(ForSumObj);


                if (Response.Result)
                {
                   if(SalesOfferId==0)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "Invalid SalesOfferId";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var SalesOffer = _unitOfWork.SalesOffers.FindAll(a => a.Id == SalesOfferId, includes: new[] { "Invoices" }).FirstOrDefault();

                    if (SalesOffer == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "This Sales Offer ID doesn't exist !";
                        Response.Errors.Add(error);
                        return Response;
                    }


                    var SalesOfferProductsList = SalesOffer.SalesOfferProducts;


                    var InvoiceDB = SalesOffer.Invoices.FirstOrDefault();
                    if (InvoiceDB == null)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err99";
                        error.ErrorMSG = "This Sales Offer ID not have any invoice !";
                        Response.Errors.Add(error);
                        return Response;
                    }
                    var ClientData = _unitOfWork.Clients.FindAll(x => x.Id == InvoiceDB.ClientId, includes: new[] { "ClientContactPeople", "ClientAddresses" }).FirstOrDefault();
                    var MainClientData = _unitOfWork.Clients.FindAll(x => x.OwnerCoProfile == true).FirstOrDefault();
                    string MainCompanyAddress = "-";
                    string TaxCard = "-";
                    string FromCompanyName = "-";
                    if (MainClientData != null)
                    {
                        var MainClientAdress = _unitOfWork.ClientAddresses.FindAll(x => x.ClientId == MainClientData.Id).FirstOrDefault();
                        FromCompanyName = MainClientData.Name;
                        var governerate = _unitOfWork.Governorates.GetById(MainClientAdress.GovernorateId);
                        TaxCard = MainClientData.TaxCard;
                        MainCompanyAddress = MainClientAdress.Address + " " + "Building No:" + " " + MainClientAdress.BuildingNumber + " " + governerate?.Name + " " + "," + "" + MainClientAdress.Country.Name;

                    }

                    SalesOfferDetailsVMObject.InvoiceTo = ClientData.Name; // Common.GetClientName((long)InvoiceDB.ClientID);
                    SalesOfferDetailsVMObject.InvoiceClientPhoneNo = ClientData.ClientContactPeople?.FirstOrDefault()?.Mobile; // Common.GetClientName((long)InvoiceDB.ClientID);
                    SalesOfferDetailsVMObject.InvoiceSerial = InvoiceDB.Serial;
                    SalesOfferDetailsVMObject.InvoiceDate = InvoiceDB.InvoiceDate.ToString();
                    SalesOfferDetailsVMObject.Address = ClientData.ClientAddresses.FirstOrDefault()?.Address;
                    if (ClientData.ClientPaymentTerms != null && ClientData.ClientPaymentTerms.Count > 0)
                    {
                        SalesOfferDetailsVMObject.TermsOfPayment = ClientData.ClientPaymentTerms?.FirstOrDefault().Percentage + " " + ClientData.ClientPaymentTerms?.FirstOrDefault().Name;
                    }
                    SalesOfferDetailsVMObject.FromCompanyName = FromCompanyName;
                    SalesOfferDetailsVMObject.TaxCard = TaxCard;
                    SalesOfferDetailsVMObject.MainCompanyAddress = MainCompanyAddress;
                    //MainClientAdress.Address + " " + "Building No:" + " " + MainClientAdress.BuildingNumber + " " + Common.GetGovernorateName(MainClientAdress.GovernorateID) + " " + "," + "" + MainClientAdress.Country.Name;


                    string CarModel = "";
                    string PlateNo = "";
                    string ChassisNumber = "";
                    string MaintenanceName = "";
                    string CarKiloMeter = "";
                    var VehiclePerClient = _unitOfWork.VehiclePerClients.FindAll(x => x.ClientId == SalesOffer.ClientId, includes: new[] {"Model"}).FirstOrDefault();
                    if (VehiclePerClient != null)
                    {
                        CarModel = VehiclePerClient.Model?.Name;
                        PlateNo = VehiclePerClient.PlatNumber;
                        ChassisNumber = VehiclePerClient.ChassisNumber;
                        var ProjectId = _unitOfWork.Projects.FindAll(x => x.SalesOfferId == SalesOffer.Id).Select(x => x.Id).FirstOrDefault();
                        var VehicleMaintenanceJobOrderHistory = _Context.VehicleMaintenanceJobOrderHistories.Where(x => x.VehiclePerClientId == VehiclePerClient.Id && x.JobOrderProjectId == ProjectId).FirstOrDefault();
                        if (VehicleMaintenanceJobOrderHistory != null)
                        {
                            MaintenanceName = VehicleMaintenanceJobOrderHistory.VehicleMaintenanceType?.Name;
                            CarKiloMeter = VehicleMaintenanceJobOrderHistory.Milage?.ToString();
                        }

                    }

                    if (SalesOfferProductsList != null && SalesOfferProductsList.Count != 0)
                    {

                        foreach (var item in SalesOfferProductsList)
                        {
                            var SalesOfferProductsDB = new SalesOfferProductsVM();

                            SalesOfferProductsDB.ID = item.Id;
                            SalesOfferProductsDB.InventoryItems = item.InventoryItem.Name;
                            SalesOfferProductsDB.NetPrice = decimal.Parse(item.Quantity.ToString()) * item.ItemPrice;
                            SalesOfferProductsDB.Price = item.ItemPrice;
                            SalesOfferProductsDB.QTY = item.Quantity;
                            SalesOfferProductsDB.TaxAmount = item.SalesOfferProductTaxes.Sum(x => x.Value);
                            SalesOfferProductsDB.FinalUnitPrice = SalesOfferProductsDB.NetPrice + SalesOfferProductsDB.TaxAmount;
                            SalesOfferProductsDB.Description = item.ItemPricingComment;
                            TotalTaxAmount += item.SalesOfferProductTaxes.Sum(x => x.Value);
                            TotalNetPrice += SalesOfferProductsDB.NetPrice;
                            TotalFinalUnitPrice += SalesOfferProductsDB.FinalUnitPrice;


                            SalesOfferProductsVMList.Add(SalesOfferProductsDB);
                        }
                    }




                    //Start PDF Service


                    MemoryStream ms = new MemoryStream();

                    //Size of page
                    iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.LETTER);


                    PdfWriter pw = PdfWriter.GetInstance(document, ms);

                    //Call the footer Function

                    pw.PageEvent = new HeaderFooter2();

                    document.Open();

                    //Handle fonts and Sizes +  Attachments images logos 

                    iTextSharp.text.Rectangle rec = new iTextSharp.text.Rectangle(iTextSharp.text.PageSize.LETTER);

                    //document.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());

                    //document.SetMargins(0, 0, 20, 20);
                    BaseFont bf = BaseFont.CreateFont(Environment.GetEnvironmentVariable("windir") + @"\arial.TTF", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                    iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 11, iTextSharp.text.Font.NORMAL);


                    //}



                    if (MainClientData.HasLogo == true && MainClientData.LogoUrl != null)
                    {

                        var clientLogo = Globals.baseURL + MainClientData.LogoUrl;
                        ////string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                        //Image logo = Image.GetInstance(clientLogo);
                        //logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                        //logo.ScaleAbsolute(3000f, 3000f);
                        //logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                        iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(clientLogo);
                        jpg.ScaleAbsolute(210, 100);
                        //document.Add(logo);
                        document.Add(jpg);
                        //iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                        //Paragraph prgHeading = new Paragraph();
                        //prgHeading.Alignment = Element.ALIGN_RIGHT;
                        ////prgHeading.SpacingBefore = -10;
                        //Chunk cc = new Chunk("I Sales Offer" + " ", fntHead);
                        //cc.SetBackground(new BaseColor(4, 189, 189), 220,0, 0, 80);




                        iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 30, 1, iTextSharp.text.BaseColor.WHITE);
                        iTextSharp.text.Paragraph prgHeading = new iTextSharp.text.Paragraph();
                        prgHeading.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;
                        prgHeading.SpacingBefore = -30;
                        prgHeading.SpacingAfter = 20;


                        iTextSharp.text.Chunk cc = new iTextSharp.text.Chunk("I Sales Offer" + " ", fntHead);
                        cc.SetBackground(new iTextSharp.text.BaseColor(4, 189, 189), 140, 12, 0, 25);
                        prgHeading.Add(cc);

                        document.Add(prgHeading);



                        //Adding paragraph for report generated by  
                        iTextSharp.text.Paragraph prgGeneratedBY = new iTextSharp.text.Paragraph();
                        BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                        iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                        prgGeneratedBY.Alignment = iTextSharp.text.Element.ALIGN_RIGHT;

                        document.Add(prgGeneratedBY);


                    }



                    //Adding a line  
                    //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                    //document.Add(p);
                    //String path = HttpContext.Current.Server.MapPath("/Attachments");

                    //if (headers["CompanyName"].ToString() == "marinaplt")
                    //{
                    //    var PurchasePOInvoiceDB2 = new PurchasePOInvoiceVM();



                    //    string PDFp_strPath = Path.Combine(path, "logoMarina.png");
                    //    //Image logo = Image.GetInstance(PDFp_strPath);
                    //    //logo.SetAbsolutePosition(80f, 50f);
                    //    //logo.ScaleAbsolute(600f,600f);



                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(PDFp_strPath);
                    //    jpg.SetAbsolutePosition(60f, 750f);
                    //    //document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -20;



                    //    Chunk cc = new Chunk("I Invoice" + " ", fntHead);

                    //    cc.SetBackground(new BaseColor(4, 189, 189), 220, 9, 15, 40);


                    //    prgHeading.Add(cc);

                    //    document.Add(prgHeading);

                    //}
                    //else if (headers["CompanyName"].ToString() == "piaroma")
                    //{

                    //    string Piaroma_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    //    Image logo = Image.GetInstance(Piaroma_p_strPath);
                    //    logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 10);
                    //    logo.ScaleAbsolute(300f, 300f);

                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(Piaroma_p_strPath);
                    //    document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 30, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -30;
                    //    prgHeading.SpacingAfter = 20;


                    //    Chunk cc = new Chunk("I Invoice" + " ", fntHead);
                    //    cc.SetBackground(new BaseColor(4, 189, 189), 220, 9, 0, 25);


                    //    prgHeading.Add(cc);

                    //    document.Add(prgHeading);


                    //}
                    //else if (headers["CompanyName"].ToString() == "Garastest")
                    //{
                    //    string GarasTest_p_strPath = Path.Combine(path, "PI-AROMA.JPEG");
                    //    Image logo = Image.GetInstance(GarasTest_p_strPath);
                    //    logo.SetAbsolutePosition(50f, pw.PageSize.GetTop(document.TopMargin) + 2);
                    //    logo.ScaleAbsolute(300f, 300f);


                    //    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(GarasTest_p_strPath);
                    //    document.Add(logo);
                    //    document.Add(jpg);
                    //    iTextSharp.text.Font fntHead = new iTextSharp.text.Font(bf, 17, 1, iTextSharp.text.BaseColor.WHITE);
                    //    Paragraph prgHeading = new Paragraph();
                    //    prgHeading.Alignment = Element.ALIGN_RIGHT;
                    //    prgHeading.SpacingBefore = -10;



                    //    Chunk cc = new Chunk("I Invoice" + " ", fntHead);
                    //    cc.SetBackground(new BaseColor(4, 189, 189), 210, 9, 5, 40);


                    //    prgHeading.Add(cc);


                    //    //prgHeading.Add(new Chunk("Inventory Store Item Report".ToUpper(), fntHead));

                    //    document.Add(prgHeading);
                    //}




                    ////Adding paragraph for report generated by  
                    //Paragraph prgGeneratedBY = new Paragraph();
                    //BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                    //iTextSharp.text.Font fntAuthor = new iTextSharp.text.Font(btnAuthor, 8, 2, iTextSharp.text.BaseColor.WHITE);
                    //prgGeneratedBY.Alignment = Element.ALIGN_RIGHT;

                    //document.Add(prgGeneratedBY);



                    //Adding a line  
                    //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.WHITE, Element.ALIGN_LEFT, 1)));
                    //document.Add(p);






                    var dt2 = new System.Data.DataTable("grid");


                    dt2.Columns.AddRange(new DataColumn[8] { new DataColumn("No	"),
                                                     new DataColumn("Inventory Store Items"),
                                                     new DataColumn("QTY"),
                                                     new DataColumn("Price") ,
                                                      new DataColumn("Net Price"),
                                                     new DataColumn("Tax Amount"),
                                                     new DataColumn("Description"),
                                                     new DataColumn("Final Unit Price") ,

                    });
                    var Counter2 = 1;
                    var SalesOfferProductsListVM = SalesOfferProductsVMList;
                    if (SalesOfferProductsListVM != null)
                    {
                        foreach (var item in SalesOfferProductsListVM)
                        {

                            //string CounterString = Counter == 0 ? "" : Counter.ToString();
                            dt2.Rows.Add(
                                Counter2.ToString(),
                                item.InventoryItems != null ? item.InventoryItems : "-",
                                item.QTY != null ? item.QTY : 0,
                                item.Price != null ? Math.Abs(Decimal.Round((decimal)item.Price, 2)) : 0,
                                item.NetPrice != null ? Math.Abs(Decimal.Round((decimal)item.NetPrice, 2)) : 0,
                                item.TaxAmount != null ? Math.Abs(Decimal.Round((decimal)item.TaxAmount, 2)) : 0,
                                item.Description != null ? item.Description : "-",
                                item.FinalUnitPrice != null ? Math.Abs(Decimal.Round((decimal)item.FinalUnitPrice, 2)) : 0



                                );
                            Counter2++;
                        }
                    }










                    PdfPTable tableHeading = new PdfPTable(4);

                    tableHeading.WidthPercentage = 100;

                    tableHeading.SetTotalWidth(new float[] { 150, 400, 80, 80 });


                    PdfPCell cellPurchasePOInvoiceTypeIDName = new PdfPCell();
                    string cellPurchasePOInvoiceTypeIDNametext = "I" + "Invoice";
                    cellPurchasePOInvoiceTypeIDName.Phrase = new iTextSharp.text.Phrase("", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceTypeIDName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cellPurchasePOInvoiceTypeIDName.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceTypeIDName.BorderColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    cellPurchasePOInvoiceTypeIDName.PaddingBottom = 15;
                    cellPurchasePOInvoiceTypeIDName.PaddingTop = 15;
                    cellPurchasePOInvoiceTypeIDName.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);

                    PdfPCell cellPurchasePOIDNumber = new PdfPCell();
                    string cellPurchasePOIDNumbertext = " Invoice Serial" + " " + SalesOfferDetailsVMObject.InvoiceSerial;
                    cellPurchasePOIDNumber.Phrase = new iTextSharp.text.Phrase(cellPurchasePOIDNumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 12, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOIDNumber.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    cellPurchasePOIDNumber.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPurchasePOIDNumber.BorderColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    cellPurchasePOIDNumber.PaddingBottom = 15;
                    cellPurchasePOIDNumber.PaddingTop = 15;
                    cellPurchasePOIDNumber.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);



                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOIDNumber);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);
                    tableHeading.AddCell(cellPurchasePOInvoiceTypeIDName);

                    tableHeading.KeepTogether = true;



                    PdfPTable tablePO = new PdfPTable(4);

                    tablePO.WidthPercentage = 100;

                    tablePO.SetTotalWidth(new float[] { 100, 250, 100, 65 });





                    PdfPCell cellPONumber = new PdfPCell();
                    string cellPONumbertext = "Invoice To:";
                    cellPONumber.Phrase = new iTextSharp.text.Phrase(cellPONumbertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont5 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPONumber.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellPONumber.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPONumber.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellPONumber.PaddingTop = 15;


                    PdfPCell cellBPurchasePOInvoice = new PdfPCell();
                    string cellBPurchasePOInvoicetext = SalesOfferDetailsVMObject.InvoiceTo;
                    cellBPurchasePOInvoice.Phrase = new iTextSharp.text.Phrase(cellBPurchasePOInvoicetext, font);
                    cellBPurchasePOInvoice.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //cellBPurchasePOInvoice.Phrase = new Phrase(cellBPurchasePOInvoicetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFont4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellBPurchasePOInvoice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellBPurchasePOInvoice.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellBPurchasePOInvoice.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellBPurchasePOInvoice.PaddingTop = 15;












                    PdfPCell cellPOType = new PdfPCell();
                    string cellPOTypetext = "";
                    cellPOType.Phrase = new iTextSharp.text.Phrase(cellPOTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont6 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPOType.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellPOType.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPOType.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellPOType.PaddingTop = 15;


                    PdfPCell cellPurchasePOInvoiceType = new PdfPCell();
                    string cellPurchasePOInvoiceTypetext = "";
                    cellPurchasePOInvoiceType.Phrase = new iTextSharp.text.Phrase(cellPurchasePOInvoiceTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont7 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    cellPurchasePOInvoiceType.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellPurchasePOInvoiceType.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellPurchasePOInvoiceType.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellPurchasePOInvoiceType.PaddingTop = 15;





                    PdfPCell cellTelCustomer = new PdfPCell();
                    string cellTelCustomerrtext = "Tel Customer:";
                    cellTelCustomer.Phrase = new iTextSharp.text.Phrase(cellTelCustomerrtext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    cellTelCustomer.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellTelCustomer.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellTelCustomer.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellTelCustomer.PaddingTop = 15;


                    PdfPCell cellBTelCustomer = new PdfPCell();
                    string cellBTelCustomertext = SalesOfferDetailsVMObject.InvoiceClientPhoneNo;
                    cellBTelCustomer.Phrase = new iTextSharp.text.Phrase(cellBTelCustomertext, font);
                    cellBTelCustomer.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    cellBTelCustomer.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellBTelCustomer.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellBTelCustomer.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellBTelCustomer.PaddingTop = 15;



                    PdfPCell cellempty = new PdfPCell();
                    string cellemptytext = "";
                    cellempty.Phrase = new iTextSharp.text.Phrase(cellemptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    cellempty.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellempty.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellempty.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellempty.PaddingTop = 15;


                    PdfPCell cellemptyRes = new PdfPCell();
                    string cellemptyRestext = "";
                    cellemptyRes.Phrase = new iTextSharp.text.Phrase(cellPurchasePOInvoiceTypetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    cellemptyRes.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellemptyRes.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellemptyRes.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellemptyRes.PaddingTop = 15;





                    #region Car Model ##################################################

                    PdfPCell cellCARMODEL = new PdfPCell();
                    string cellCARMODELtext = "Car Model:";
                    cellCARMODEL.Phrase = new iTextSharp.text.Phrase(cellCARMODELtext, font);
                    cellCARMODEL.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARMODEL.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARMODEL.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARMODEL.PaddingTop = 15;


                    PdfPCell cellCARModelRES = new PdfPCell();
                    cellCARModelRES.Phrase = new iTextSharp.text.Phrase(CarModel, font);
                    cellCARModelRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARModelRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARModelRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARModelRES.PaddingTop = 15;
                    cellCARModelRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell cellEMPTY = new PdfPCell();
                    cellEMPTY.Phrase = new iTextSharp.text.Phrase("", font);
                    cellEMPTY.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellEMPTY.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellEMPTY.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellEMPTY.PaddingTop = 15;


                    PdfPCell cellEMPTYRes = new PdfPCell();
                    cellEMPTYRes.Phrase = new iTextSharp.text.Phrase("", font);
                    cellEMPTYRes.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    cellEMPTYRes.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellEMPTYRes.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellEMPTYRes.PaddingTop = 15;
                    #endregion

                    #region Car Plate No ##################################################
                    PdfPCell cellCARPlateNo = new PdfPCell();
                    string cellCARPlateNotext = "Plate No:";
                    cellCARPlateNo.Phrase = new iTextSharp.text.Phrase(cellCARPlateNotext, font);
                    cellCARPlateNo.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARPlateNo.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARPlateNo.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARPlateNo.PaddingTop = 15;


                    PdfPCell cellCARPlateNoRES = new PdfPCell();
                    cellCARPlateNoRES.Phrase = new iTextSharp.text.Phrase(PlateNo, font);
                    cellCARPlateNoRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARPlateNoRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARPlateNoRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARPlateNoRES.PaddingTop = 15;
                    cellCARPlateNoRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car Chassis No ##################################################
                    PdfPCell cellCARChassisNo = new PdfPCell();
                    cellCARChassisNo.Phrase = new iTextSharp.text.Phrase("Chassis No:", font);
                    cellCARChassisNo.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARChassisNo.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARChassisNo.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARChassisNo.PaddingTop = 15;


                    PdfPCell cellCARChassisNoRES = new PdfPCell();
                    cellCARChassisNoRES.Phrase = new iTextSharp.text.Phrase(ChassisNumber, font);
                    cellCARChassisNoRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellCARChassisNoRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellCARChassisNoRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellCARChassisNoRES.PaddingTop = 15;
                    cellCARChassisNoRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car MaintenanceName  ##################################################
                    PdfPCell cellMaintenanceName = new PdfPCell();
                    cellMaintenanceName.Phrase = new iTextSharp.text.Phrase("Maintenance Name:", font);
                    cellMaintenanceName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellMaintenanceName.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellMaintenanceName.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellMaintenanceName.PaddingTop = 15;


                    PdfPCell cellMaintenanceNameRES = new PdfPCell();
                    cellMaintenanceNameRES.Phrase = new iTextSharp.text.Phrase(MaintenanceName, font);
                    cellMaintenanceNameRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellMaintenanceNameRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellMaintenanceNameRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellMaintenanceNameRES.PaddingTop = 15;
                    cellMaintenanceNameRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion

                    #region Car CarKiloMeter  ##################################################
                    PdfPCell cellKiloMeter = new PdfPCell();
                    cellKiloMeter.Phrase = new iTextSharp.text.Phrase("KiloMeter:", font);
                    cellKiloMeter.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellKiloMeter.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellKiloMeter.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellKiloMeter.PaddingTop = 15;


                    PdfPCell cellKiloMeterRES = new PdfPCell();
                    cellKiloMeterRES.Phrase = new iTextSharp.text.Phrase(CarKiloMeter, font);
                    cellKiloMeterRES.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    cellKiloMeterRES.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    cellKiloMeterRES.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    cellKiloMeterRES.PaddingTop = 15;
                    cellKiloMeterRES.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    #endregion






                    tablePO.AddCell(cellPONumber);
                    tablePO.AddCell(cellBPurchasePOInvoice);

                    tablePO.AddCell(cellPOType);
                    tablePO.AddCell(cellPurchasePOInvoiceType);

                    tablePO.AddCell(cellTelCustomer);
                    tablePO.AddCell(cellBTelCustomer);

                    tablePO.AddCell(cellempty);
                    tablePO.AddCell(cellemptyRes);

                    if (!string.IsNullOrWhiteSpace(CarModel))
                    {
                        tablePO.AddCell(cellCARMODEL);
                        tablePO.AddCell(cellCARModelRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(ChassisNumber))
                    {
                        tablePO.AddCell(cellCARChassisNo);
                        tablePO.AddCell(cellCARChassisNoRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(PlateNo))
                    {
                        tablePO.AddCell(cellCARPlateNo);
                        tablePO.AddCell(cellCARPlateNoRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(MaintenanceName))
                    {
                        tablePO.AddCell(cellMaintenanceName);
                        tablePO.AddCell(cellMaintenanceNameRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }
                    if (!string.IsNullOrWhiteSpace(CarKiloMeter))
                    {
                        tablePO.AddCell(cellKiloMeter);
                        tablePO.AddCell(cellKiloMeterRES);
                        tablePO.AddCell(cellEMPTY);
                        tablePO.AddCell(cellEMPTYRes);
                    }


                    PdfPCell CellInvoiceDate = new PdfPCell();
                    string CellInvoiceDatetext = "Bill To: ";
                    CellInvoiceDate.Phrase = new iTextSharp.text.Phrase(CellInvoiceDatetext, font);
                    //CellInvoiceDate.Phrase = new Phrase(CellInvoiceDatetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFontD = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellInvoiceDate.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellInvoiceDate.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellInvoiceDate.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellInvoiceDate.PaddingTop = 10;


                    PdfPCell CellInvoiceDateText = new PdfPCell();
                    CellInvoiceDateText.Phrase = new iTextSharp.text.Phrase(SalesOfferDetailsVMObject.InvoiceTo, font);
                    CellInvoiceDateText.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //CellInvoiceDateText.Phrase = new Phrase(CellInvoiceDateTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    //iTextSharp.text.Font arabicFont4s = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellInvoiceDateText.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellInvoiceDateText.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellInvoiceDateText.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellInvoiceDateText.PaddingTop = 10;
                    CellInvoiceDateText.Bottom = 15;


                    PdfPCell CellTotalInvoicePrice = new PdfPCell();
                    string CellTotalInvoicePricetext = "";
                    CellTotalInvoicePrice.Phrase = new iTextSharp.text.Phrase(CellTotalInvoicePricetext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontt = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellTotalInvoicePrice.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTotalInvoicePrice.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalInvoicePrice.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellTotalInvoicePrice.PaddingTop = 10;
                    CellTotalInvoicePrice.Bottom = 15;


                    PdfPCell CellEmpty = new PdfPCell();
                    string CellEmptytext = "";
                    CellEmpty.Phrase = new iTextSharp.text.Phrase(CellEmptytext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFont78 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellEmpty.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellEmpty.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellEmpty.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellEmpty.PaddingTop = 10;
                    CellEmpty.Bottom = 15;

                    tablePO.AddCell(CellInvoiceDate);
                    tablePO.AddCell(CellInvoiceDateText);
                    tablePO.AddCell(CellTotalInvoicePrice);
                    tablePO.AddCell(CellEmpty);





                    PdfPCell CellSupplier = new PdfPCell();
                    string CellSuppliertext = "Terms Of Payment: ";
                    CellSupplier.Phrase = new iTextSharp.text.Phrase(CellSuppliertext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontK = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellSupplier.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellSupplier.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellSupplier.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellSupplier.PaddingTop = 10;


                    PdfPCell CellSupplierName = new PdfPCell();
                    string CellSupplierNametext = SalesOfferDetailsVMObject.TermsOfPayment;
                    CellSupplierName.Phrase = new iTextSharp.text.Phrase(CellSupplierNametext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontQ = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellSupplierName.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellSupplierName.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellSupplierName.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellSupplierName.PaddingTop = 10;
                    CellSupplierName.Bottom = 15;


                    PdfPCell CellPOStatus = new PdfPCell();
                    string CellPOStatustext = "Registration Card:";
                    CellPOStatus.Phrase = new iTextSharp.text.Phrase(CellPOStatustext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontL = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellPOStatus.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellPOStatus.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellPOStatus.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellPOStatus.PaddingTop = 10;
                    CellPOStatus.Bottom = 15;


                    PdfPCell CellPOStatusText = new PdfPCell();
                    string CellPOStatusTexttext = SalesOfferDetailsVMObject.RegistrationCard != null ? SalesOfferDetailsVMObject.RegistrationCard : "N/A";
                    CellPOStatusText.Phrase = new iTextSharp.text.Phrase(CellPOStatusTexttext, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontM = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    //cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                    //cell2.Phrase = new Phrase(cell2Text, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                    //cell2.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                    CellPOStatusText.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellPOStatusText.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellPOStatusText.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CellPOStatusText.PaddingTop = 10;
                    CellPOStatusText.Bottom = 15;

                    tablePO.AddCell(CellSupplier);
                    tablePO.AddCell(CellSupplierName);
                    tablePO.AddCell(CellPOStatus);
                    tablePO.AddCell(CellPOStatusText);





                    tablePO.SpacingAfter = 15;






                    PdfPTable table4 = new PdfPTable(4);

                    table4.WidthPercentage = 100;

                    table4.SetTotalWidth(new float[] { 55, 400, 100, 98 });


                    //table4.AddCell(CellVEmpty);
                    //table4.AddCell(CellV2);
                    //table4.AddCell(CellV3);
                    //table4.AddCell(CellV5);



                    //Adding PdfPTable


                    PdfPTable table = new PdfPTable(dt2.Columns.Count);


                    //table Width
                    table.WidthPercentage = 100;

                    //Define Sizes of Cloumns

                    table.SetTotalWidth(new float[] { 20, 50, 35, 35, 35, 35, 35, 35 });
                    table.PaddingTop = 20;

                    for (int i = 0; i < dt2.Columns.Count; i++)
                    {
                        string cellText = HttpUtility.HtmlDecode(dt2.Columns[i].ColumnName);
                        PdfPCell cell = new PdfPCell();
                        cell.Phrase = new iTextSharp.text.Phrase(cellText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                        iTextSharp.text.Font arabicFont444 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                        cell.BackgroundColor = new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#C8C8C8"));
                        //cell.Phrase = new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, new BaseColor(grdStudent.HeaderStyle.ForeColor)));  
                        //cell.BackgroundColor = new BaseColor(grdStudent.HeaderStyle.BackColor);  
                        cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                        cell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;

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
                                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                                cell.Padding = 8;
                            }
                            else if (j >= 9)
                            {
                                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }
                            else
                            {
                                cell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                                cell.Padding = 8;
                            }

                            if (cell.ArabicOptions == 1)
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_LTR;
                                cell.Padding = 8;

                            }
                            else
                            {
                                cell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                                cell.Padding = 8;

                            }

                            cell.Phrase = new iTextSharp.text.Phrase(1, dt2.Rows[i][j].ToString(), font);
                            //cell.HorizontalAlignment = Element.ALIGN_CENTER;

                            table.AddCell(cell);

                        }

                    }







                    PdfPTable table3 = new PdfPTable(4);

                    table3.WidthPercentage = 100;

                    table3.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellDirector = new PdfPCell();
                    string CellDirectorText = "";
                    CellDirector.Phrase = new iTextSharp.text.Phrase(CellDirectorText, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNi = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellDirector.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellDirector2 = new PdfPCell();
                    string CellDirectortext2 = "";
                    CellDirector2.Phrase = new iTextSharp.text.Phrase(CellDirectortext2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellDirector2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellDirector2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellDirector3 = new PdfPCell();
                    string CellDirector3text = "Price Details";
                    CellDirector3.Phrase = new iTextSharp.text.Phrase(CellDirector3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#0007EE"))));
                    iTextSharp.text.Font arabicFontWss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellDirector3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector3.BorderColor = iTextSharp.text.BaseColor.WHITE;




                    PdfPCell CellDirector4 = new PdfPCell();
                    string CellDirector4text = "";
                    CellDirector4.Phrase = new iTextSharp.text.Phrase(CellDirector4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontVs = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellDirector4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellDirector4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellDirector4.BorderColor = iTextSharp.text.BaseColor.WHITE;

                    table3.AddCell(CellDirector);
                    table3.AddCell(CellDirector2);
                    table3.AddCell(CellDirector3);
                    table3.AddCell(CellDirector4);









                    PdfPTable tablTotals = new PdfPTable(3);
                    tablTotals.WidthPercentage = 100;

                    tablTotals.SetTotalWidth(new float[] { 200, 200, 200 });


                    PdfPCell CelltablTotals1 = new PdfPCell();
                    string CelltablTotals1Text = "Final Net Price:";
                    CelltablTotals1.Phrase = new iTextSharp.text.Phrase(CelltablTotals1Text + " " + TotalNetPrice.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    CelltablTotals1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CelltablTotals1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CelltablTotals1.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    CelltablTotals1.PaddingBottom = 15;
                    CelltablTotals1.PaddingTop = 15;


                    PdfPCell CelltablTotals2 = new PdfPCell();
                    string CelltablTotals2text2 = "Tax Amount:";
                    CelltablTotals2.Phrase = new iTextSharp.text.Phrase(CelltablTotals2text2 + " " + TotalTaxAmount.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals2.PaddingBottom = 15;
                    CelltablTotals2.PaddingTop = 15;



                    CelltablTotals2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    CelltablTotals2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CelltablTotals2.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CelltablTotals2.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);


                    PdfPCell CelltablTotals3 = new PdfPCell();
                    string CelltablTotals3text = "Final Price Amount:";
                    CelltablTotals3.Phrase = new iTextSharp.text.Phrase(CelltablTotals3text + " " + TotalFinalUnitPrice.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CelltablTotals3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    CelltablTotals3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CelltablTotals3.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    CelltablTotals3.BackgroundColor = new iTextSharp.text.BaseColor(4, 189, 189);
                    CelltablTotals3.PaddingBottom = 15;
                    CelltablTotals3.PaddingTop = 15;




                    tablTotals.AddCell(CelltablTotals1);
                    tablTotals.AddCell(CelltablTotals2);
                    tablTotals.AddCell(CelltablTotals3);

















                    PdfPTable tableOfferAmount = new PdfPTable(4);

                    tableOfferAmount.WidthPercentage = 100;

                    tableOfferAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellOfferAmount1 = new PdfPCell();
                    string CellOfferAmount1Text = "";
                    CellOfferAmount1.Phrase = new iTextSharp.text.Phrase(CellOfferAmount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontNiS = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellOfferAmount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellOfferAmount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellOfferAmount2 = new PdfPCell();
                    string CellOfferAmount2text2 = "";
                    CellOfferAmount2.Phrase = new iTextSharp.text.Phrase(CellOfferAmount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSsss = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellOfferAmount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellOfferAmount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellOfferAmount3 = new PdfPCell();
                    string CellOfferAmount3text = "Offer Amount:";
                    CellOfferAmount3.Phrase = new iTextSharp.text.Phrase(CellOfferAmount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWsssa = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellOfferAmount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellOfferAmount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellOfferAmount4 = new PdfPCell();
                    string CellOfferAmount4text = TotalNetPrice.ToString();
                    CellOfferAmount4.Phrase = new iTextSharp.text.Phrase(CellOfferAmount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellOfferAmount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellOfferAmount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellOfferAmount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableOfferAmount.AddCell(CellOfferAmount1);
                    tableOfferAmount.AddCell(CellOfferAmount2);
                    tableOfferAmount.AddCell(CellOfferAmount3);
                    tableOfferAmount.AddCell(CellOfferAmount4);








                    PdfPTable tableTotalDiscount = new PdfPTable(4);

                    tableTotalDiscount.WidthPercentage = 100;

                    tableTotalDiscount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellTotalDiscount1 = new PdfPCell();
                    string CellTotalDiscount1Text = "";
                    CellTotalDiscount1.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTotalDiscount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellTotalDiscount2 = new PdfPCell();
                    string CellTotalDiscount2text2 = "";
                    CellTotalDiscount2.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontSDic = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellTotalDiscount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTotalDiscount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTotalDiscount3 = new PdfPCell();
                    string CellTotalDiscount3text = "Total Discount:";
                    CellTotalDiscount3.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontWsDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTotalDiscount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTotalDiscount4 = new PdfPCell();
                    string CellTotalDiscount4text = "0.0";
                    CellTotalDiscount4.Phrase = new iTextSharp.text.Phrase(CellTotalDiscount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFonDisc = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTotalDiscount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTotalDiscount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTotalDiscount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableTotalDiscount.AddCell(CellTotalDiscount1);
                    tableTotalDiscount.AddCell(CellTotalDiscount2);
                    tableTotalDiscount.AddCell(CellTotalDiscount3);
                    tableTotalDiscount.AddCell(CellTotalDiscount4);








                    PdfPTable tableTaxAmount = new PdfPTable(4);

                    tableTaxAmount.WidthPercentage = 100;

                    tableTaxAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellTaxAmount1 = new PdfPCell();
                    string CellTaxAmount1Text = "";
                    CellTaxAmount1.Phrase = new iTextSharp.text.Phrase(CellTaxAmount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTaxAmount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellTaxAmount2 = new PdfPCell();
                    string CellTaxAmount2text2 = "";
                    CellTaxAmount2.Phrase = new iTextSharp.text.Phrase(CellTaxAmount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTaxAmount = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellTaxAmount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTaxAmount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTaxAmount3 = new PdfPCell();
                    string CellTaxAmount3text = "Tax Amount:";
                    CellTaxAmount3.Phrase = new iTextSharp.text.Phrase(CellTaxAmount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellTaxAmount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellTaxAmount4 = new PdfPCell();
                    string CellTaxAmount4text = TotalTaxAmount.ToString();
                    CellTaxAmount4.Phrase = new iTextSharp.text.Phrase(CellTaxAmount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicTax4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellTaxAmount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellTaxAmount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellTaxAmount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableTaxAmount.AddCell(CellTaxAmount1);
                    tableTaxAmount.AddCell(CellTaxAmount2);
                    tableTaxAmount.AddCell(CellTaxAmount3);
                    tableTaxAmount.AddCell(CellTaxAmount4);









                    PdfPTable tableT4Amount = new PdfPTable(4);

                    tableT4Amount.WidthPercentage = 100;

                    tableT4Amount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellT4Amount1 = new PdfPCell();
                    string CellT4Amount1Text = "";
                    CellT4Amount1.Phrase = new iTextSharp.text.Phrase(CellT4Amount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1T4 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT4Amount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT4Amount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellT4Amount2 = new PdfPCell();
                    string CellT4Amount2text2 = "";
                    CellT4Amount2.Phrase = new iTextSharp.text.Phrase(CellT4Amount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontT4Amount2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellT4Amount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT4Amount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT4Amount3 = new PdfPCell();
                    string CellT4Amount3text222 = "T4 Amount:";
                    CellT4Amount3.Phrase = new iTextSharp.text.Phrase(CellT4Amount3text222, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellT4Amount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT4Amount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT4Amount4 = new PdfPCell();
                    string CellT4Amount4text11 = "1121238236";
                    CellT4Amount4.Phrase = new iTextSharp.text.Phrase(CellT4Amount4text11, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellT4Amount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT4Amount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT4Amount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableT4Amount.AddCell(CellT4Amount1);
                    tableT4Amount.AddCell(CellT4Amount2);
                    tableT4Amount.AddCell(CellT4Amount3);
                    tableT4Amount.AddCell(CellT4Amount4);







                    PdfPTable tableT1Amount = new PdfPTable(4);

                    tableT1Amount.WidthPercentage = 100;

                    tableT1Amount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellT1Amount1 = new PdfPCell();
                    string CellT1Amount1Text = "";
                    CellT1Amount1.Phrase = new iTextSharp.text.Phrase(CellT1Amount1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontTax1T1 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT1Amount1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellT1Amount2 = new PdfPCell();
                    string CellT1Amount2text2 = "";
                    CellT1Amount2.Phrase = new iTextSharp.text.Phrase(CellT1Amount2text2, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontT1Amount2 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);

                    CellT1Amount2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT1Amount2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT1Amount3 = new PdfPCell();
                    string CellT1Amount3text = "T1 Amount:";
                    CellT1Amount3.Phrase = new iTextSharp.text.Phrase(CellT1Amount3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicFontCellT1AAmount = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellT1Amount3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellT1Amount4 = new PdfPCell();
                    string CellT1Amount4text = "1121238236";
                    CellT1Amount4.Phrase = new iTextSharp.text.Phrase(CellT1Amount4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    iTextSharp.text.Font arabicTaxT14 = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    CellT1Amount4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellT1Amount4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellT1Amount4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableT1Amount.AddCell(CellT1Amount1);
                    tableT1Amount.AddCell(CellT1Amount2);
                    tableT1Amount.AddCell(CellT1Amount3);
                    tableT1Amount.AddCell(CellT1Amount4);
























                    PdfPTable tableFinalOfferPriceAmount = new PdfPTable(4);

                    tableFinalOfferPriceAmount.WidthPercentage = 100;

                    tableFinalOfferPriceAmount.SetTotalWidth(new float[] { 55, 400, 100, 60 });


                    PdfPCell CellFinalOfferPrice1 = new PdfPCell();
                    string CellFinalOfferPrice1Text = "";
                    CellFinalOfferPrice1.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice1Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellFinalOfferPrice1.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice1.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //CellDirector.PaddingTop = 15;


                    PdfPCell CellFinalOfferPrice2 = new PdfPCell();
                    string CellFinalOfferPrice2Text = "";
                    CellFinalOfferPrice2.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice2Text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));

                    CellFinalOfferPrice2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellFinalOfferPrice2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice2.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellFinalOfferPrice3 = new PdfPCell();
                    string CellFinalOfferPrice3text = "Final Offer Price:";
                    CellFinalOfferPrice3.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice3text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT;
                    CellFinalOfferPrice3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice3.BorderColor = iTextSharp.text.BaseColor.WHITE;



                    PdfPCell CellFinalOfferPrice4 = new PdfPCell();
                    string CellFinalOfferPrice4text = TotalFinalUnitPrice.ToString();
                    CellFinalOfferPrice4.Phrase = new iTextSharp.text.Phrase(CellFinalOfferPrice4text, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9, 1, new iTextSharp.text.BaseColor(System.Drawing.ColorTranslator.FromHtml("#000000"))));
                    CellFinalOfferPrice4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT;
                    CellFinalOfferPrice4.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    CellFinalOfferPrice4.BorderColor = iTextSharp.text.BaseColor.WHITE;


                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice1);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice2);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice3);
                    tableFinalOfferPriceAmount.AddCell(CellFinalOfferPrice4);

























                    table.SpacingAfter = 20;

                    //if (SupplierPaymenDB != null)
                    //{
                    //    table5.AddCell(CellDirector5);
                    //}

                    iTextSharp.text.Paragraph parag = new iTextSharp.text.Paragraph(new iTextSharp.text.Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, iTextSharp.text.BaseColor.BLACK, iTextSharp.text.Element.ALIGN_CENTER, 1)));



                    PdfPTable FooterPart = new PdfPTable(1);

                    FooterPart.WidthPercentage = 100;

                    var CompanyName = validation.CompanyName.ToLower();
                    PdfPCell FooterCell = new PdfPCell();
                    string FooterCelltext = FromCompanyName;

                    FooterCell.Phrase = new iTextSharp.text.Phrase(FooterCelltext, font);
                    // iTextSharp.text.Font FooterCellfont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    FooterCell.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    FooterCell.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    FooterCell.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    FooterCell.PaddingTop = 15;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    PdfPCell FooterCell2 = new PdfPCell();


                    //var CompanyID = _Context.Clients.Where(x => x.OwnerCoProfile == true).FirstOrDefault();
                    //var CompanyAddress = _Context.ClientAddresses.Where(x => x.ClientID == CompanyID.ID).FirstOrDefault();

                    string FooterCelltext7 = MainCompanyAddress; // CompanyAddress.Address + " " + CompanyAddress.Country.Name + " " + "Building No:" + " " + CompanyAddress.BuildingNumber;
                    FooterCell2.Phrase = new iTextSharp.text.Phrase(FooterCelltext7, font);
                    FooterCell2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    FooterCell2.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    FooterCell2.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //FooterCell2.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    PdfPCell FooterCell3 = new PdfPCell();
                    string FooterCelltext8 = "Tax ID:" + " " + TaxCard;

                    FooterCell3.Phrase = new iTextSharp.text.Phrase(FooterCelltext8, font);
                    iTextSharp.text.Font FooterCefont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN);
                    FooterCell3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                    FooterCell3.VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE;
                    FooterCell3.BorderColor = iTextSharp.text.BaseColor.WHITE;
                    //FooterCell.HorizontalAlignment = Element.ALIGN_LEFT;
                    //FooterCell.RunDirection = PdfWriter.RUN_DIRECTION_RTL;


                    FooterPart.AddCell(FooterCell);
                    FooterPart.AddCell(FooterCell2);
                    if (!string.IsNullOrWhiteSpace(TaxCard))
                    {
                        FooterPart.AddCell(FooterCell3);

                    }





                    table3.SpacingBefore = 30;

                    table.SpacingAfter = 20;

                    document.Add(tableHeading);
                    document.Add(tablePO);
                    document.Add(table4);
                    document.Add(table);

                    document.Add(tablTotals);
                    tablTotals.SpacingAfter = 100;
                    document.Add(table3);
                    document.Add(tableOfferAmount);
                    document.Add(tableTotalDiscount);
                    document.Add(tableTaxAmount);


                    //  document.Add(tableT1Amount);
                    //  document.Add(tableT4Amount);


                    document.Add(tableFinalOfferPriceAmount);
                    parag.SpacingBefore = 100;

                    document.Add(parag);

                    document.Add(FooterPart);


                    FooterPart.SpacingBefore = -100;

                    document.Close();
                    byte[] result = ms.ToArray();
                    ms = new MemoryStream();
                    ms.Write(result, 0, result.Length);
                    ms.Position = 0;



                    string FullFileName = DateTime.Now.ToFileTime() + "_" + "InventoryStoreItem.pdf";
                    string PathsTR = "/Attachments/" + CompanyName + "/";
                    String Filepath = _host.WebRootPath+"/"+PathsTR;
                    string p_strPath = Filepath + "/" + FullFileName;

                    File.WriteAllBytes(p_strPath, result);

                    Response.Message = Globals.baseURL + PathsTR + FullFileName;






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

        public GetReportStatiscsGroupbyDateResponse GetSalesReportLineStatisticsPerDate(GetSalesReportLineStatisticsPerDateFilters filters)
        {
            GetReportStatiscsGroupbyDateResponse Response = new GetReportStatiscsGroupbyDateResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                #region old headers
                //int year = 0;
                //if (!string.IsNullOrEmpty(headers["Year"]) || int.TryParse(headers["Year"], out year))
                //{
                //    year = int.Parse(headers["Year"]);
                //}
                //long ReportCreator = 0;
                //if (!string.IsNullOrEmpty(headers["ReportCreator"]) && long.TryParse(headers["ReportCreator"], out ReportCreator))
                //{
                //    ReportCreator = long.Parse(headers["ReportCreator"]);
                //}
                //long SalesPersonId = 0;
                //if (!string.IsNullOrEmpty(headers["SalesPersonId"]) && long.TryParse(headers["SalesPersonId"], out SalesPersonId))
                //{
                //    SalesPersonId = long.Parse(headers["SalesPersonId"]);
                //}
                //long ClientId = 0;
                //if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                //{
                //    ClientId = long.Parse(headers["ClientId"]);
                //}
                //long BranchId = 0;
                //if (!string.IsNullOrEmpty(headers["BranchId"]) && long.TryParse(headers["BranchId"], out BranchId))
                //{
                //    BranchId = long.Parse(headers["BranchId"]);
                //}
                //bool? isReviewed = null;
                //if (!string.IsNullOrEmpty(headers["isReviewed"]) && bool.Parse(headers["isReviewed"]) != null)
                //{
                //    isReviewed = bool.Parse(headers["isReviewed"]);
                //}
                //string ThroughName = null;
                //if (!string.IsNullOrEmpty(headers["ThroughName"]))
                //{
                //    ThroughName = headers["ThroughName"];
                //}
                #endregion

                if (Response.Result)
                {
                    List<ReportStatisctsPerDate> SalesReportLinesListPerDate = new List<ReportStatisctsPerDate>();
                    var SalesReportsList = _unitOfWork.VDailyReportReportLineThroughApis.FindAllQueryable(a => true);
                    if (filters.Year > 0 && filters.Year != null)
                    {
                        SalesReportsList = SalesReportsList.Where(a => ((DateTime)a.ReprotDate).Year == filters.Year && a.Status != "Not Filled").AsQueryable();
                    }
                    if (filters.ReportCreator > 0 && filters.ReportCreator != null)
                    {
                        SalesReportsList = SalesReportsList.Where(a => a.UserId == filters.ReportCreator).AsQueryable();
                    }
                    if (filters.SalesPersonId > 0)
                    {
                        SalesReportsList = SalesReportsList.Where(a => a.SalesPersonId == filters.SalesPersonId).AsQueryable();
                    }
                    if (filters.ClientId > 0 && filters.ClientId != null)
                    {
                        SalesReportsList = SalesReportsList.Where(a => a.ClientId == filters.ClientId).AsQueryable();
                    }
                    if (filters.BranchId > 0 && filters.BranchId != null)
                    {
                        SalesReportsList = SalesReportsList.Where(a => a.BranchId == filters.BranchId).AsQueryable();
                    }
                    if (filters.isReviewed != null)
                    {
                        SalesReportsList = SalesReportsList.Where(a => a.Reviewed == filters.isReviewed).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filters.ThroughName))
                    {
                        if (filters.ThroughName.Trim().ToLower() == "other")
                        {
                            SalesReportsList = SalesReportsList.Where(a => a.Name.Trim().ToLower() == "other").AsQueryable();
                        }
                        else if (filters.ThroughName.ToLower() == "meeting")
                        {
                            SalesReportsList = SalesReportsList.Where(a => a.Name.ToLower().Contains("visit") || a.Name.ToLower().Contains("visit of client"));
                        }
                        else
                        {
                            SalesReportsList = SalesReportsList.Where(a => a.Name.Trim().ToLower() == filters.ThroughName.Trim().ToLower()).AsQueryable();
                        }
                    }
                    var SalesReportsListDB = SalesReportsList.ToList().GroupBy(x => new { x.CreationDate.Year }).ToList();
                    var SalesReportsListPerYeatDB = SalesReportsListDB.Select((item, index) =>
                    {
                        int CountOfItemsPerYear = item.Count();
                        var SalesReportsListPerMonthDB = item.GroupBy(x => new { x.CreationDate.Year, x.CreationDate.Month }).ToList();


                        var SalesReportsPerMonths = SalesReportsListPerMonthDB.Select((itemPerMonth, indexPerMonth) =>
                        {
                            int CountOfItemsPerMonth = itemPerMonth.Count();
                            var SalesReportsListPerDayDB = itemPerMonth.GroupBy(x => new { ((DateTime)x.ReprotDate).Year, ((DateTime)x.ReprotDate).Month, ((DateTime)x.ReprotDate).Day }).ToList();


                            var SalesReportsPerDay = SalesReportsListPerDayDB.Select((itemPerDay, indexPerDay) =>
                            {
                                int CountOfItemsPerDay = itemPerDay.Count();

                                return new ReportStatisctsPerDate  // Per Day
                                {
                                    Count = CountOfItemsPerDay,
                                    DatePerType = itemPerDay.Key.Day,
                                    CreationDate = new DateTime(itemPerDay.Key.Year, itemPerDay.Key.Month, itemPerDay.Key.Day).ToShortDateString()
                                };
                            }).OrderBy(a => a.DatePerType).ToList();

                            return new ReportStatisctsPerDate   // Per Month
                            {
                                ReportLinesPerDateList = SalesReportsPerDay,
                                Count = CountOfItemsPerMonth,
                                DatePerType = itemPerMonth.Key.Month
                            };
                        }).OrderBy(a => a.DatePerType).ToList();

                        return new ReportStatisctsPerDate   // Per Year
                        {

                            ReportLinesPerDateList = SalesReportsPerMonths,
                            Count = CountOfItemsPerYear,
                            DatePerType = item.Key.Year
                        };
                    }).OrderBy(a => a.DatePerType).ToList();


                    Response.Data = SalesReportsListPerYeatDB;
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

        public ClientsSalesReportsDetailsResponse SalesReportsDetails(SalesReportsDetailsFilters filters)
        {
            ClientsSalesReportsDetailsResponse Response = new ClientsSalesReportsDetailsResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var StartDate = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(filters.StartDate))
                    {
                        DateTime FilterStartDate = DateTime.Now;
                        if (!DateTime.TryParse(filters.StartDate, out FilterStartDate))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Start Date";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        StartDate = FilterStartDate.Date;
                    }
                    var EndDate = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(filters.EndDate))
                    {
                        DateTime FilterEndDate = DateTime.Now;
                        if (!DateTime.TryParse(filters.EndDate, out FilterEndDate))
                        {
                            Error error = new Error();
                            error.ErrorCode = "Err-12";
                            error.ErrorMSG = "Invalid Start Date";
                            Response.Errors.Add(error);
                            Response.Result = false;
                            return Response;
                        }
                        EndDate = FilterEndDate.Date;
                    }
                    /* int Month = 0;
                     if (!string.IsNullOrEmpty(headers["Month"]) && int.TryParse(headers["Month"], out Month))
                     {
                         int.TryParse(headers["Month"], out Month);
                         Response.FilteredMonth = Month;
                     }

                     int Year = 0;
                     if (!string.IsNullOrEmpty(headers["Year"]) && int.TryParse(headers["Year"], out Year))
                     {
                         int.TryParse(headers["Year"], out Year);
                         Response.FilteredYear = Year;
                     }*/

                    //long SalesPersonId = 0;
                    //if (filters.SalesPersonId != null)
                    //{
                    //    long.TryParse(headers["SalesPersonId"], out SalesPersonId);
                    //    Response.FilteredSalesPersonId = SalesPersonId;
                    //}
                    //long ReportCreator = 0;
                    //if (!string.IsNullOrEmpty(headers["ReportCreator"]) && long.TryParse(headers["ReportCreator"], out ReportCreator))
                    //{
                    //    long.TryParse(headers["ReportCreator"], out ReportCreator);
                    //    Response.FilteredReportCreator = ReportCreator;
                    //}
                    //int BranchId = 0;
                    //if (!string.IsNullOrEmpty(headers["BranchId"]) && int.TryParse(headers["BranchId"], out BranchId))
                    //{
                    //    int.TryParse(headers["BranchId"], out BranchId);
                    //    Response.FilteredBranchId = BranchId;
                    //}

                    //long ClientId = 0;
                    //if (!string.IsNullOrEmpty(headers["ClientId"]) && long.TryParse(headers["ClientId"], out ClientId))
                    //{
                    //    long.TryParse(headers["ClientId"], out ClientId);
                    //    Response.FilteredClientId = ClientId;
                    //    Response.FilteredClientName = Common.GetClientName(ClientId);
                    //}
                    //string ThroughName = "";
                    //if (!string.IsNullOrEmpty(headers["ThroughName"]))
                    //{
                    //    ThroughName = headers["ThroughName"].ToString();
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
                    DateTime ReminderDate = DateTime.MinValue;
                    if (!string.IsNullOrEmpty(filters.ReminderDate) && DateTime.TryParse(filters.ReminderDate, out ReminderDate))
                    {
                        ReminderDate = DateTime.Parse(filters.ReminderDate);
                    }


                    var SalesReportsDbQuery = _unitOfWork.VDailyReportReportLineThroughApis.FindAllQueryable(a => a.Status != "Not Filled").AsQueryable();

                    if (StartDate == DateTime.MinValue)
                    {
                        StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                    }
                    if (EndDate == DateTime.MinValue)
                    {
                        EndDate = new DateTime(StartDate.Year + 1, 1, 1);
                    }
                    /* var EndDate = new DateTime(DateTime.Now.Year + 1, 1, 1);

                     if (Month > 0)
                     {
                         StartDate = new DateTime(Year, Month, 1);

                         if (Month != 12)
                         {
                             EndDate = new DateTime(Year, (Month + 1), 1);
                         }
                         else
                         {
                             EndDate = new DateTime((Year + 1), 1, 1);
                         }
                     }
                     else
                     {
                         StartDate = new DateTime(Year, 1, 1);
                         EndDate = new DateTime((Year + 1), 1, 1);
                     }
                 }*/
                    SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.ReprotDate >= StartDate && a.ReprotDate <= EndDate);
                    /* else
                     {
                         if (Month > 0)
                         {
                             Response.Result = false;
                             Error error = new Error();
                             error.ErrorCode = "ErrCRM1";
                             error.ErrorMSG = "You have to choose the Year of the Month you choosed!!";
                             Response.Errors.Add(error);

                             return Response;
                         }
                         else
                         {
                             if (ReminderDate != DateTime.MinValue)
                             {
                                 SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.ReminderDate == ReminderDate);
                             }

                         }
                     }*/

                    if (filters.BranchId != 0 && filters.BranchId != null)
                    {
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.BranchId == filters.BranchId);
                    }

                    var SalesName = "";
                    if (filters.SalesPersonId != 0 && filters.SalesPersonId != null)
                    {
                        var salesPerson = _unitOfWork.Users.GetById(filters.SalesPersonId??0);
                        if (salesPerson != null)
                        {
                            SalesName = salesPerson.FirstName + " " + salesPerson.MiddleName + " " + salesPerson.LastName;
                        }
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.SalesPersonId == filters.SalesPersonId);
                    }
                    if (filters.ReportCreator > 0 && filters.ReportCreator != null)
                    {
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(s => s.UserId == filters.ReportCreator).AsQueryable();
                    }
                    if (filters.ClientId != 0 && filters.ClientId != null)
                    {
                        SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.ClientId == filters.ClientId);
                    }
                    if (!string.IsNullOrEmpty(filters.ThroughName))
                    {
                        if (filters.ThroughName.ToLower() == "other")
                        {
                            SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.Name.ToLower().Contains("other"));
                        }
                        else if (filters.ThroughName.ToLower() == "meeting")
                        {
                            SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.Name.ToLower().Contains("visit") || a.Name.ToLower().Contains("visit of client"));
                        }
                        else
                        {
                            SalesReportsDbQuery = SalesReportsDbQuery.Where(a => a.Name.ToLower().Contains(filters.ThroughName.ToLower()));
                        }
                    }
                    SalesReportsDbQuery = SalesReportsDbQuery.OrderByDescending(a => a.CreationDate);
                    var SalesReportsListDB = PagedList<VDailyReportReportLineThroughApi>.Create(SalesReportsDbQuery, filters.CurrentPage, filters.NumberOfItemsPerPage);

                    List<CrmSalesClientReport> SalesReportsListResponse = new List<CrmSalesClientReport>();
                    var usersIDs = SalesReportsListDB.Where(a => a.SalesPersonId != null).Select(a => a.SalesPersonId).ToList();
                    usersIDs.AddRange(SalesReportsListDB.Select(a => a.ReviewedBy).ToList());

                    var clientsIDs = SalesReportsListDB.Select(a => a.ClientId).ToList();
                    var clientsData = _unitOfWork.Clients.FindAll(a => clientsIDs.Contains(a.Id));

                    var usersData = _unitOfWork.Users.FindAll(a => usersIDs.Contains(a.Id)).ToList();

                    var salesPersonsIDs = clientsData.Select(a => a.SalesPersonId).ToList();
                    //usersIDs.AddRange((IEnumerable<long?>)salesPersonsIDs);

                    var userDataForSalesPerson = _unitOfWork.Users.FindAll(a => salesPersonsIDs.Contains(a.Id)).ToList();




                    foreach (var report in SalesReportsListDB)
                    {
                        var SalesPersonData = usersData.Where(a => a.Id == report.SalesPersonId).FirstOrDefault();
                        var ReviewedByData = usersData.Where(a => a.Id == report.ReviewedBy).FirstOrDefault();

                        var ClientDb = _unitOfWork.Clients.Find(a => a.Id == report.ClientId, new[] { "ClientAddresses" });
                        string ClientCurrentStatus = null;
                        if (ClientDb.NeedApproval != null)
                        {
                            switch (ClientDb.NeedApproval)
                            {
                                case 0:
                                    ClientCurrentStatus = "Approved";
                                    break;
                                case 1:
                                    ClientCurrentStatus = "Waiting";
                                    break;
                                case 2:
                                    ClientCurrentStatus = "Rejected";
                                    break;
                                default:
                                    break;
                            }
                        }

                        var LinesCount = _unitOfWork.VDailyReportReportLineThroughApis.FindAll(a => a.DailyReportId == report.DailyReportId).Count();

                        var clientSalesPersonID = clientsData.Where(a => a.Id == report.ClientId).FirstOrDefault().SalesPersonId;

                        var reportVM = new CrmSalesClientReport
                        {
                            ID = report.Id,
                            ClientID = report.ClientId ?? 0,
                            ClientName = report.ClientName,
                            LinesCount = LinesCount,
                            ContactPersonMobile = report.ContactPersonMobile,
                            ContactPersonName = report.ContactPerson,
                            ClientCalssification = ClientDb.ClientClassification?.Name,
                            ClientClassificationId = ClientDb.ClientClassification?.Id,
                            ClientStatus = ClientCurrentStatus,
                            ClientAddress = ClientDb.ClientAddresses.Count > 0 ? ClientDb.ClientAddresses.FirstOrDefault().Address : null,
                            IsNew = report.New ?? false,
                            NewClientName = report.NewClientName,
                            NewClientAddress = report.NewClientAddress,
                            NewClientTele = report.NewClientTel,
                            Location = report.Location,
                            FromTime = report.FromTime ?? 0,
                            ToTime = report.ToTime ?? 0,
                            ReportDate = report.ReprotDate?.ToShortDateString(),
                            CreationDate = report.CreationDate.ToShortDateString(),
                            ThroughID = report.DailyReportThroughId,
                            ThroughName = report.Name,
                            SalesName = SalesPersonData.FirstName + " " + SalesPersonData.LastName,//Common.GetUserName(report.SalesPersonId ?? 0),
                            ClientSalesPersonName = report.ClientId != null ? clientSalesPersonID != null ? userDataForSalesPerson.Where(d => d.Id == clientSalesPersonID).FirstOrDefault().FirstName + "  " + userDataForSalesPerson.Where(d => d.Id == clientSalesPersonID).FirstOrDefault().LastName : null : null,
                            Comment = report.Reason,
                            Reason = report.ReasonTypeName,
                            Review = report.Review,
                            ReviewedBy = report.ReviewedBy,
                            ReviewedByName = report.ReviewedBy != null ? ReviewedByData.FirstName + " " + ReviewedByData.LastName : null,
                            Longitude = report.Longitude,
                            Latitude = report.Latitude,
                            ReminderDate = report.ReminderDate != null ? report.ReminderDate.Value.ToShortDateString() : null,
                            IsReviewed = report.Reviewed,
                            SalesReportCreator = report.FirstName + " " + report.LastName

                        };

                        var expensesDb = _Context.DailyReportExpenses.Where(a => a.DailyReportLineId == report.Id).ToList();
                        if (expensesDb != null && expensesDb.Any())
                        {
                            reportVM.SalesReportLineExpenses = expensesDb.Select(expense => new SalesReportLineExpense
                            {
                                Amount = expense.Amount ?? 0,
                                CurrencyId = expense.CurrencyId,
                                CurrencyName = expense.Currency.Name,
                                DailyReportLineID = report.Id,
                                Id = expense.Id,
                                Type = expense.Type
                            }).ToList();
                        }
                        if (report.RelatedToInventoryItemId != null)
                        {
                            var productDb = _Context.InventoryItems.Where(a => a.Id == report.RelatedToInventoryItemId).FirstOrDefault();
                            if (productDb != null)
                            {
                                reportVM.RelatedToInventoryItemId = report.RelatedToInventoryItemId;
                                reportVM.RelatedToInventoryItemName = productDb.Name;
                            }
                        }
                        else if (report.RelatedToSalesOfferId != null)
                        {
                            var offerDb = _Context.SalesOffers.Where(a => a.Id == report.RelatedToSalesOfferId).FirstOrDefault();


                            if (offerDb.Status.ToLower() == "closed")
                            {
                                var projectDb = offerDb.Projects.FirstOrDefault();
                                reportVM.RelatedToProjectId = projectDb.Id;
                                reportVM.RelatedToProjectSerial = projectDb.ProjectSerial;
                                reportVM.RelatedToProjectName = offerDb.ProjectName;

                                if (report.RelatedToSalesOfferProductId != null)
                                {
                                    var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                    if (productDb != null)
                                    {
                                        reportVM.RelatedToProjectProductId = report.RelatedToSalesOfferProductId;
                                        reportVM.RelatedToProjectProductName = productDb.InventoryItem?.Name;
                                    }
                                }
                            }
                            else
                            {
                                reportVM.RelatedToSalesOfferId = report.RelatedToSalesOfferId;
                                reportVM.RelatedToSalesOfferSerial = offerDb.OfferSerial;
                                reportVM.RelatedToSalesOfferName = offerDb.ProjectName;

                                if (report.RelatedToSalesOfferProductId != null)
                                {
                                    var productDb = offerDb.SalesOfferProducts.Where(a => a.Id == report.RelatedToSalesOfferProductId).FirstOrDefault();
                                    if (productDb != null)
                                    {
                                        reportVM.RelatedToSalesOfferProductId = report.RelatedToSalesOfferProductId;
                                        reportVM.RelatedToSalesOfferProductName = productDb.InventoryItem?.Name;
                                    }
                                }
                            }
                        }
                        SalesReportsListResponse.Add(reportVM);
                    }
                    Response.SalesReports = SalesReportsListResponse;

                    Response.PaginationHeader = new PaginationHeader
                    {
                        CurrentPage = filters.CurrentPage,
                        TotalPages = SalesReportsListDB.TotalPages,
                        ItemsPerPage = filters.NumberOfItemsPerPage,
                        TotalItems = SalesReportsListDB.TotalCount
                    };
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

        public async Task<GetSalesOfferListDDLForReleaseResponse> GetSalesOfferListDDLForRelease(long ClientId, string SearchKey, bool? StatusIsOpenFilter, int CurrentPage = 1, int NumberOfItemsPerPage = 10)
        {
            var Response = new GetSalesOfferListDDLForReleaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var SalesOfferList = new List<SelectSalesOfferDDLForRelease>();
                                              
                if (Response.Result)
                {
                    var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, new[] { "SalesPerson" , "Client", "Projects" }).AsQueryable();

                    // Client Name , Offer Serial ,Project Name
                    if (!string.IsNullOrEmpty(SearchKey))
                    {
                        SearchKey = HttpUtility.UrlDecode(SearchKey);
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x =>
                                                   (x.Client != null && x.Client.Name != null ? x.Client.Name.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || (x.OfferSerial != null ? x.OfferSerial.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || (x.ProjectName != null ? x.ProjectName.ToLower().Contains(SearchKey.ToLower()) : false)
                                                || (x.Projects.Any(a => a.ProjectSerial != null ? a.ProjectSerial.ToLower().Contains(SearchKey.ToLower()) : false))
                                                //|| SalesOfferIDS.Contains(x.ID)
                                                ).AsQueryable();
                    }
                    if (ClientId != 0)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.ClientId == ClientId);
                    }
                    if (StatusIsOpenFilter != null)
                    {
                        if (StatusIsOpenFilter == true)//  ( Pricing, Recieved, ClientApproval )
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status != "Closed" && x.Status != "Rejected");
                        }
                        else
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status == "Closed" || x.Status != "Rejected");
                        }
                    }

                    //var OffersListDB = SalesOfferDBQuery.ToList();

                    var OffersListDB = PagedList<SalesOffer>.Create(SalesOfferDBQuery, CurrentPage, NumberOfItemsPerPage);

                    foreach (var offer in OffersListDB)
                    {
                        long? projectId = null;
                        decimal QTYOfMatrialReleaseItem = 0;
                        double QTYOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.RemainQty ?? 0) ?? 0;
                        var SalesOfferProductCount = offer.SalesOfferProducts?.Count ?? 0;
                        if (offer.Projects.Count > 0)
                        {
                            var offerProject = offer.Projects.FirstOrDefault();
                            projectId = offerProject.Id;
                            if (offerProject.InventoryMatrialRequestItems.Count > 0)
                            {
                                QTYOfMatrialReleaseItem = offerProject.InventoryMatrialRequestItems?.Sum(x => x.RecivedQuantity1 ?? 0) ?? 0;
                            }
                        }
                        // Calc Percentage Product Released

                        decimal Percent = 0;
                        string ReleaseStatus = "";
                        if (QTYOfSalesOfferProduct > 0)
                        {
                            ReleaseStatus = "Part";
                            Percent = (QTYOfMatrialReleaseItem / (decimal)QTYOfSalesOfferProduct) * 100;

                            if (Percent > 100)
                            {
                                ReleaseStatus = "Exceeded";
                            }
                            if (Percent == 100)
                            {
                                ReleaseStatus = "Full";
                            }
                        }

                        var totalExtraCostAmount = offer.SalesOfferExtraCosts?.Sum(a => (decimal?)a.Amount);
                        var totalTaxAmount = offer.SalesOfferInvoiceTaxes.Where(a => a.Active == true).Sum(a => (decimal?)a.TaxValue);
                        var totalDiscountAmount = offer.SalesOfferDiscounts?.Where(a => a.DiscountApproved == true && a.Active == true).Sum(a => (decimal?)a.DiscountValue);

                        decimal TotalSalesOfferAvgPrice = 0;
                        decimal TotalSalesOfferProfitMarginValue = 0;
                        decimal TotalSalesOfferProfitMarginPer = 0;

                        foreach (var product in offer.SalesOfferProducts)
                        {
                            var ItemPrice = product.ItemPrice ?? 0;
                            if (ItemPrice != 0)
                            {

                                var ItemProfitPer = product.ProfitPercentage ?? 0;
                                var ItemRemainQty = product.RemainQty ?? product.Quantity ?? 0;

                                var ItemAvg = 100 * ItemPrice / (100 + ItemProfitPer);

                                TotalSalesOfferProfitMarginValue += (ItemPrice - (100 * ItemPrice / (100 + ItemProfitPer))) * (decimal)ItemRemainQty;

                                TotalSalesOfferAvgPrice += ItemAvg * (decimal)ItemRemainQty;
                            }
                        }

                        if (TotalSalesOfferAvgPrice > 0)
                        {
                            TotalSalesOfferProfitMarginPer = TotalSalesOfferProfitMarginValue / TotalSalesOfferAvgPrice * 100;
                        }
                        //long? ParentSalesOfferId = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOfferId).FirstOrDefault();
                        //string ParentSalesOfferSErial = ParentSalesOfferListDB.Where(x => x.SalesOfferId == offer.Id).Select(x => x.ParentSalesOffer?.OfferSerial).FirstOrDefault();
                        //var ListChildrenSalesOffer = ParentSalesOfferListDB.Where(x => x.ParentSalesOfferId == offer.Id).Select(x => new ChildrenSalesOFfer { SalesOfferId = x.SalesOfferId, SalesOfferSerial = x.SalesOffer?.OfferSerial }).ToList();

                        var salesOfferObj = new SelectSalesOfferDDLForRelease()
                        {
                            ID = offer.Id,
                            SalesPersonId = offer.SalesPersonId,
                            SalesPersonName = offer.SalesPerson.FirstName + ' ' + offer.SalesPerson.MiddleName + ' ' + offer.SalesPerson.LastName,
                            ClientId = offer.ClientId,
                            ClientName = offer.Client.Name,
                            ProjectName = offer.ProjectName,
                            OfferSerial = offer.OfferSerial,
                            // Michael Markos 2022-11-28
                            ReleaseStatus = ReleaseStatus,
                            PercentReleased = Percent,
                            ProjectSerial = offer.Projects.FirstOrDefault() != null? offer.Projects.FirstOrDefault().ProjectSerial : null,

                        };


                        SalesOfferList.Add(salesOfferObj);
                    }


                    //SalesOfferList = await SalesOfferDBQuery.Select(x => new SelectSalesOfferDDLForRelease
                    //{
                    //    ID = x.Id,
                    //    ProjectName = x.ProjectName,
                    //    OfferSerial = x.OfferSerial,
                    //    ProjectSerial = x.Projects.FirstOrDefault() != null
                    //    ? x.Projects.FirstOrDefault().ProjectSerial : null,

                    //}).ToListAsync();

                    var pagination = new PaginationHeader()
                    {
                        TotalPages = OffersListDB.TotalPages,
                        TotalItems = OffersListDB.TotalCount,
                        CurrentPage = CurrentPage,
                        ItemsPerPage = NumberOfItemsPerPage
                    };
                    Response.paginationHeader = pagination;
                    Response.Data = SalesOfferList;
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


        public BaseResponseWithData<string> GetTotalAmountForEachCategory(GetTotalAmountForEachCategoryFilters filters, string CompName)
        {
            var response = new BaseResponseWithData<string>();
            response.Result = true;
            response.Errors = new List<Error>();

            try
            {
                #region Validation
                var from = DateTime.Now;
                if (!DateTime.TryParse(filters.from, out from))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    response.Errors.Add(err);
                    return response;
                }
                var nextDay = from.AddDays(1);

                var to = DateTime.Now;
                if (!DateTime.TryParse(filters.to, out to))
                {
                    response.Result = false;
                    Error err = new Error();
                    err.ErrorCode = "E-1";
                    err.errorMSG = "please, Enter a valid Date";
                    response.Errors.Add(err);
                    return response;
                }

                #endregion
                TimeSpan difference = to - from;

                // Get the number of days
                int daysDifference = difference.Days;

                var inventoryCategories = _unitOfWork.InventoryItemCategories.GetAll();

                var salesOfferList = _unitOfWork.SalesOffers.FindAll(a => a.CreationDate.Date >= from.Date && a.CreationDate.Date <= to.Date && a.OfferType.Contains("Ticket"));

                var salesOfferIDs = salesOfferList.Select(a => a.Id).ToList();

                // the real data to show
                var salesOfferProduct = _unitOfWork.SalesOfferProducts.FindAll(a => a.CreationDate.Date >= from.Date && a.CreationDate.Date <= to.Date && salesOfferIDs.Contains(a.OfferId), new[] { "Offer", "InventoryItem" , "InventoryItemCategory" });

                var categoriesIDs = salesOfferProduct.Select(a => a.InventoryItemCategoryId).Distinct().ToList();
                var categoriesNamesList = inventoryCategories.Where(a => categoriesIDs.Contains(a.Id)).ToList();

                ExcelPackage excel = new ExcelPackage();
                var sheet = excel.Workbook.Worksheets.Add($"sheet1");
                for (int col = 1; col <= categoriesNamesList.Count() + 1; col++) sheet.Column(col).Width = 20;
                sheet.DefaultRowHeight = 12;
                sheet.Row(1).Height = 20;
                sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Row(1).Style.Font.Bold = true;
                sheet.Cells[1, 1, 1, categoriesNamesList.Count() + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, 1, categoriesNamesList.Count() + 2].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                sheet.Cells[1, 1, 100, categoriesNamesList.Count() + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[1, 1, 100, categoriesNamesList.Count() + 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                var drawCol = 2;
                foreach (var cat in categoriesNamesList)
                {
                    sheet.Cells[1, drawCol].Value = cat.Name;
                    drawCol++;

                }

                sheet.Cells[1, drawCol, 1, drawCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, drawCol, 1, drawCol].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);

                sheet.Cells[1, 1].Value = "التاريخ";
                sheet.Cells[1, drawCol].Value = "الاجمالي";

                var column = 2;
                var row = 2;
                var curserDate = from.Date;
                for (int i = 0; i <= daysDifference; i++)
                {
                    column = 2;
                    sheet.Cells[row, 1].Value = curserDate.ToShortDateString();
                    decimal rowTotal = 0;
                    foreach (var category in categoriesNamesList)
                    {
                        
                        var categoryDayTotal = salesOfferProduct.Where(a => a.CreationDate.Date >= curserDate.Date && a.CreationDate.Date < curserDate.AddDays(1).Date && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

                        sheet.Cells[row, column].Value = categoryDayTotal;
                        rowTotal = rowTotal + categoryDayTotal??0;

                        column++;
                    }
                    if(i == 0 ) curserDate = curserDate.AddDays(i + 1);
                    else
                    {
                        curserDate = from.AddDays(i);
                    }
                    sheet.Cells[row, column ].Value = rowTotal;
                    sheet.Cells[row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[row, column].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                    sheet.Cells[2, 2, row+1, column].Style.Numberformat.Format = "#,##0.00"; // Apply to range A1:D10
                    sheet.Column(column).Width = 20;                                       // width of the Total column

                    row++;
                }

                var columnTotalcurser = 2;
                for (int i = 0; i < column; i++)
                {
                    var columnLetter = GetExcelColumnName(columnTotalcurser);
                    string formula = $"SUM({columnLetter}{2}:{columnLetter}{row-1})";  // Dynamic formula for column (e.g., SUM(A:A))
                    sheet.Cells[row, columnTotalcurser].Formula = formula; columnTotalcurser++;
                }

                sheet.Cells[row,1,row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[row,1,row, column].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                sheet.Cells[row, 1].Value = "الاجمالي";


                sheet.Cells[1, 1, row-1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[1, 1, row-1, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                var path = $"Attachments\\{CompName}\\GetTotalAmountForEachCategory";
                var savedPath = Path.Combine(_host.WebRootPath, path);
                if (File.Exists(savedPath))
                    File.Delete(savedPath);

                if(Directory.Exists(savedPath))Directory.Delete(savedPath, true);
                // Create excel file on physical disk  
                Directory.CreateDirectory(savedPath);
                //FileStream objFileStrm = File.Create(savedPath);
                //objFileStrm.Close();
                var dateNow = DateTime.Now.ToString("yyyyMMddHHssFFF");
                var excelPath = savedPath + $"\\GetSalesOfferDueForStorePOSReport_{dateNow}.xlsx";
                excel.SaveAs(excelPath);
                // Write content to excel file  
                //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
                //Close Excel package 
                excel.Dispose();
                var filePath = Globals.baseURL + '\\' + path + $"\\GetSalesOfferDueForStorePOSReport_{dateNow}.xlsx";

                response.Data = filePath;
                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.Message;
                response.Errors.Add(error);
                return response;
            }

        }

        public string GetExcelColumnName(int columnIndex)
        {
            //if (columnIndex < 1)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(columnIndex), "Column index must be greater than 0.");
            //}

            string columnName = string.Empty;
            while (columnIndex > 0)
            {
                int remainder = (columnIndex - 1) % 26; // Get the remainder (0-25)
                columnName = Convert.ToChar('A' + remainder) + columnName; // Convert to corresponding letter
                columnIndex = (columnIndex - 1) / 26; // Move to the next higher place
            }
            return columnName;
        }


        #region old_GetSalesOfferTicketsForStore
        //public BaseResponseWithData<string> GetSalesOfferTicketsForStore(string From, string To, long? UserID, string CompName)
        //{
        //    var response = new BaseResponseWithData<string>();
        //    response.Result = true;
        //    response.Errors = new List<Error>();

        //    try
        //    {
        //        #region Validation
        //        var startDate = DateTime.Now;
        //        if (!DateTime.TryParse(From, out startDate))
        //        {
        //            response.Result = false;
        //            Error err = new Error();
        //            err.ErrorCode = "E-1";
        //            err.errorMSG = "please, Enter a valid Date";
        //            response.Errors.Add(err);
        //            return response;
        //        }

        //        var endDate = DateTime.Now;
        //        if (!DateTime.TryParse(To, out endDate))
        //        {
        //            response.Result = false;
        //            Error err = new Error();
        //            err.ErrorCode = "E-1";
        //            err.errorMSG = "please, Enter a valid Date";
        //            response.Errors.Add(err);
        //            return response;
        //        }



        //        #endregion

        //        var listOfStoreData = new List<SalesOfferDueClientPOS>();



        //        var salesOffersData = _unitOfWork.SalesOffers.FindAllQueryable(a => a.CreationDate >= startDate && a.CreationDate <= endDate && a.OfferType.Contains("ticket"),
        //            new[] { "SalesOfferProducts", "CreatedByNavigation", "Client", "CreatedByNavigation", "SalesPerson", "SalesOfferProducts.InventoryItemCategory", "Client.SalesPerson", "SalesPerson.UserTeamUsers", "SalesOfferProducts.InventoryItem" });

        //        if (UserID != null)
        //        {
        //            salesOffersData = salesOffersData.Where(a => a.CreatedBy == UserID);
        //        }
        //        var salesOffers = salesOffersData.ToList();

        //        var minusSummation = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket return")).Sum(a => a.FinalOfferPrice);
        //        var plusSummationu = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket")).Sum(a => a.FinalOfferPrice);
        //        var totalSummationu = plusSummationu - minusSummation;

        //        var salesOfferGroupByCreator = salesOffers.GroupBy(a => new { a.CreatedBy }).ToList();


        //        //----------------------------------------excel----------------------------------------------
        //        ExcelPackage excel = new ExcelPackage();

        //        //-------------------------------------------------------------------------------------------
        //        if (salesOfferGroupByCreator.Count() == 0)
        //        {
        //            response.Result = false;
        //            Error err = new Error();
        //            err.ErrorCode = "E-1";
        //            err.errorMSG = "This date selected not have sales offer";
        //            response.Errors.Add(err);
        //            return response;
        //        }

        //        //var salesOfferList = new List<SalesOfferDueClientPOS>();
        //        var teams = _unitOfWork.Teams.GetAll();
        //        foreach (var group in salesOfferGroupByCreator)
        //        {

        //            //var newSalesOffer = new SalesOfferDueClientPOS();
        //            //var productList = new List<SalesOfferProductPOS>();
        //            //var DB = salesOffers.Where(a => a.Id == item.OrderId).GroupBy(a => new { a.CreatedBy});
        //            //var salesOfferOfthisStoreData = salesOffers.Where(a => a.Id == group.Key.CreatedBy).FirstOrDefault();
        //            var userDataOfthisGroup = salesOffers.Where(a => a.CreatedBy == group.Key.CreatedBy).FirstOrDefault().CreatedByNavigation;

        //            var sheet = excel.Workbook.Worksheets.Add($"{userDataOfthisGroup.FirstName.Trim()}_{userDataOfthisGroup.LastName.Trim()}");

        //            //---------naming of Excel file--------------
        //            for (int col = 1; col <= 13; col++) sheet.Column(col).Width = 20;

        //            sheet.Cells[1, 1].Value = "Code";
        //            sheet.Cells[1, 2].Value = "Ticket Price" + " \r\n " + "القيمه";
        //            sheet.Cells[1, 3].Value = "Category \n القسم";
        //            sheet.Cells[1, 4].Value = "Doctor Name \n الطبيب";
        //            sheet.Cells[1, 5].Value = "Ticket number \n  رقم التذكره";
        //            sheet.Cells[1, 6].Value = "patient \n المريض";
        //            sheet.Cells[1, 7].Value = "Notes \n الملاحظات";
        //            sheet.Cells[1, 8].Value = "Contact Person \n بيد";
        //            sheet.Cells[1, 9].Value = "Creation Date \n التاريخ";
        //            sheet.Cells[1, 10].Value = "offer Type \n النوع";
        //            sheet.Cells[1, 11].Value = "Doctor Team \n التخصص";
        //            sheet.Cells[1, 12].Value = "Item Name \n التشخيص";
        //            sheet.Cells[1, 13].Value = "Created By \n المستخدم";

        //            sheet.Cells[1, 1, 1, 13].Style.WrapText = true;
        //            //sheet.Cells[1, 4].Value = "Offer Name";
        //            //sheet.Cells[1, 5].Value = "final Offer Price";
        //            //sheet.Cells[1, 10].Value = "Item Price";
        //            //sheet.Cells[1, 6].Value = "Internal ID";
        //            //sheet.Cells[1, 5].Value = "Category Name";

        //            sheet.Column(2).Style.Numberformat.Format = "#,##0.00";

        //            sheet.DefaultRowHeight = 20;
        //            sheet.Row(1).Height = 40;
        //            sheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            sheet.Row(1).Style.Font.Bold = true;
        //            sheet.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            sheet.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
        //            sheet.Cells[1, 1, 1, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            sheet.Cells[1, 1, 1, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //            decimal testSum = 0;
        //            decimal testSub = 0;
        //            decimal sum = 0;
        //            decimal cashSum = 0;
        //            int rowNum = 3;

        //            int code = 1;
        //            //var teams = _unitOfWork.Teams.GetAll();
        //            var salesOffersDrawedAll = new List<long>();
        //            foreach (var salesOffer in group)
        //            {

        //                //newSalesOffer.OfferID = item.OrderId;
        //                //var salesOffer = salesOffers.Where(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
        //                //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);
        //                if (salesOffer != null)
        //                {
        //                    var alreadyDrawed = salesOffersDrawedAll.Contains(salesOffer.Id);
        //                    if (salesOffer != null && !alreadyDrawed)
        //                    {
        //                        var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;

        //                        sheet.Row(rowNum).OutlineLevel = 1;
        //                        sheet.Row(rowNum).Collapsed = false;
        //                        sheet.Cells[rowNum, 1].Value = code; code++;
        //                        sheet.Cells[rowNum, 2].Value = salesOffer.FinalOfferPrice;

        //                        var lastName = salesOffer.SalesPerson.LastName;
        //                        if (lastName == ".") lastName = " ";
        //                        sheet.Cells[rowNum, 4].Value = salesOffer.SalesPerson.FirstName + " " + lastName;
        //                        sheet.Cells[rowNum, 5].Value = salesOffer.ProjectName;
        //                        sheet.Cells[rowNum, 6].Value = salesOffer.Client.Name;
        //                        //sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;sheet.Cells[rowNum, 6].Value = salesOffer.Id;
        //                        sheet.Cells[rowNum, 8].Value = salesOffer.ContactPersonName;//salesOffer.Client.SalesPerson.FirstName + " " + salesOffer.Client.SalesPerson.LastName;
        //                        sheet.Cells[rowNum, 9].Value = salesOffer.CreationDate.ToString();
        //                        sheet.Cells[rowNum, 10].Value = salesOffer.OfferType;
        //                        sheet.Cells[rowNum, 11].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
        //                        sheet.Cells[rowNum, 13].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
        //                        sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //                        //---------------sum of cash-----------------------------
        //                        if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSum -= salesOffer.FinalOfferPrice ?? 0;
        //                        else { cashSum += salesOffer.FinalOfferPrice ?? 0; }


        //                        if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
        //                        {
        //                            sheet.Cells[rowNum, 1, rowNum, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //                            sheet.Cells[rowNum, 1, rowNum, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
        //                            sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                            sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                        }
        //                        //-------------------------------------------------------
        //                        foreach (var productDB in salesOffer.SalesOfferProducts)
        //                        {
        //                            sheet.Row(rowNum).OutlineLevel = 2;
        //                            sheet.Row(rowNum).Collapsed = true;
        //                            sheet.Cells[rowNum, 7].Value = productDB.ItemPricingComment;
        //                            //sheet.Cells[rowNum , 9].Value = productDB.Quantity;
        //                            //sheet.Cells[rowNum , 4].Value = productDB.ItemPrice;
        //                            sheet.Cells[rowNum, 3].Value = productDB.InventoryItemCategory.Name;
        //                            sheet.Cells[rowNum, 12].Value = productDB.InventoryItem.Name;
        //                            sheet.Row(rowNum).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                            sheet.Row(rowNum).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



        //                            //rowNum++;
        //                        }
        //                        if (salesOffer.OfferType.Contains("Internal Ticket return")) { sum = sum - salesOffer.FinalOfferPrice ?? 0; }
        //                        else { sum += salesOffer.FinalOfferPrice ?? 0; }


        //                        salesOffersDrawedAll.Add(salesOffer.Id);

        //                        //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
        //                        //newSalesOffer.projectName = salesOffer.ProjectName;
        //                        //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
        //                        //newSalesOffer.OfferType = salesOffer.OfferType;
        //                    }

        //                    //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
        //                    //{
        //                    //    ItemPrice = a.ItemPrice,
        //                    //    Quantity = a.Quantity,
        //                    //    ProductID = a.Id,
        //                    //    productComment = a.ItemPricingComment
        //                    //});

        //                    //productList.AddRange(product);
        //                    //newSalesOffer.ProductList = productList;
        //                    //salesOfferList.Add(newSalesOffer);

        //                }
        //                rowNum++;
        //            }
        //            sheet.Cells[2, 2].Value = sum;
        //            sheet.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //            sheet.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            sheet.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            sheet.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
        //            salesOffersDrawedAll.Clear();
        //            sum = 0;



        //        }

        //        #region all Users
        //        //-----------------------sheet for all users--------------------------------------------------
        //        var sheet2 = excel.Workbook.Worksheets.Add($"All_Users");



        //        //---------naming of Excel file--------------
        //        for (int col = 1; col <= 13; col++) sheet2.Column(col).Width = 20;

        //        sheet2.Cells[1, 1].Value = "Code";
        //        sheet2.Cells[1, 2].Value = "Ticket Price" + " \r\n " + "القيمه";
        //        sheet2.Cells[1, 3].Value = "Category \n القسم";
        //        sheet2.Cells[1, 4].Value = "Doctor Name \n الطبيب";
        //        sheet2.Cells[1, 5].Value = "Ticket number \n  رقم التذكره";
        //        sheet2.Cells[1, 6].Value = "patient \n المريض";
        //        sheet2.Cells[1, 7].Value = "Notes \n الملاحظات";
        //        sheet2.Cells[1, 8].Value = "Contact Person \n بيد";
        //        sheet2.Cells[1, 9].Value = "Creation Date \n التاريخ";
        //        sheet2.Cells[1, 10].Value = "offer Type \n النوع";
        //        sheet2.Cells[1, 11].Value = "Doctor Team \n التخصص";
        //        sheet2.Cells[1, 12].Value = "Item Name \n التشخيص";
        //        sheet2.Cells[1, 13].Value = "Created By \n المستخدم";

        //        sheet2.Cells[1, 1, 1, 13].Style.WrapText = true;
        //        //sheet.Cells[1, 4].Value = "Offer Name";
        //        //sheet.Cells[1, 5].Value = "final Offer Price";
        //        //sheet.Cells[1, 10].Value = "Item Price";
        //        //sheet.Cells[1, 6].Value = "Internal ID";
        //        //sheet.Cells[1, 5].Value = "Category Name";

        //        sheet2.Column(2).Style.Numberformat.Format = "#,##0.00";

        //        sheet2.DefaultRowHeight = 20;
        //        sheet2.Row(1).Height = 40;
        //        sheet2.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet2.Row(1).Style.Font.Bold = true;
        //        sheet2.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet2.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
        //        sheet2.Cells[1, 1, 1, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet2.Cells[1, 1, 1, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        decimal testSumAll = 0;
        //        decimal testSubAll = 0;
        //        decimal sumAll = 0;
        //        decimal cashSumAll = 0;
        //        int rowNumAll = 3;

        //        int codeAll = 1;

        //        var salesOffersDrawed = new List<long>();
        //        foreach (var salesOffer in salesOffers)
        //        {

        //            //newSalesOffer.OfferID = item.OrderId;
        //            //var salesOffer = salesOffers.Where(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
        //            //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);
        //            if (salesOffer != null)
        //            {
        //                var alreadyDrawed = salesOffersDrawed.Contains(salesOffer.Id);
        //                if (salesOffer != null && !alreadyDrawed)
        //                {
        //                    var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;

        //                    sheet2.Row(rowNumAll).OutlineLevel = 1;
        //                    sheet2.Row(rowNumAll).Collapsed = false;
        //                    sheet2.Cells[rowNumAll, 1].Value = codeAll; codeAll++;
        //                    sheet2.Cells[rowNumAll, 2].Value = salesOffer.FinalOfferPrice;
        //                    sheet2.Cells[rowNumAll, 4].Value = salesOffer.SalesPerson.FirstName + salesOffer.SalesPerson.LastName;
        //                    sheet2.Cells[rowNumAll, 5].Value = salesOffer.ProjectName;
        //                    sheet2.Cells[rowNumAll, 6].Value = salesOffer.Client.Name;
        //                    //sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;sheet.Cells[rowNum, 6].Value = salesOffer.Id;
        //                    sheet2.Cells[rowNumAll, 8].Value = salesOffer.Client.SalesPerson.FirstName + salesOffer.Client.SalesPerson.LastName;
        //                    sheet2.Cells[rowNumAll, 9].Value = salesOffer.CreationDate.ToString();
        //                    sheet2.Cells[rowNumAll, 10].Value = salesOffer.OfferType;
        //                    sheet2.Cells[rowNumAll, 11].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
        //                    sheet2.Cells[rowNumAll, 13].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
        //                    sheet2.Row(rowNumAll).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                    sheet2.Row(rowNumAll).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //                    //---------------sum of cash-----------------------------
        //                    if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSumAll -= salesOffer.FinalOfferPrice ?? 0;
        //                    else { cashSumAll += salesOffer.FinalOfferPrice ?? 0; }


        //                    if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
        //                    {
        //                        sheet2.Cells[rowNumAll, 1, rowNumAll, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //                        sheet2.Cells[rowNumAll, 1, rowNumAll, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
        //                        sheet2.Row(rowNumAll).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        sheet2.Row(rowNumAll).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                    }
        //                    //-------------------------------------------------------
        //                    foreach (var productDB in salesOffer.SalesOfferProducts)
        //                    {
        //                        sheet2.Row(rowNumAll).OutlineLevel = 2;
        //                        sheet2.Row(rowNumAll).Collapsed = true;
        //                        sheet2.Cells[rowNumAll, 7].Value = productDB.ItemPricingComment;
        //                        //sheet.Cells[rowNum , 9].Value = productDB.Quantity;
        //                        //sheet.Cells[rowNum , 4].Value = productDB.ItemPrice;
        //                        sheet2.Cells[rowNumAll, 3].Value = productDB.InventoryItemCategory.Name;
        //                        sheet2.Cells[rowNumAll, 12].Value = productDB.InventoryItem.Name;
        //                        sheet2.Row(rowNumAll).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        sheet2.Row(rowNumAll).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



        //                        //rowNum++;
        //                    }
        //                    if (salesOffer.OfferType.Contains("Internal Ticket return")) { sumAll = sumAll - salesOffer.FinalOfferPrice ?? 0; }
        //                    else { sumAll += salesOffer.FinalOfferPrice ?? 0; }


        //                    salesOffersDrawed.Add(salesOffer.Id);

        //                    //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
        //                    //newSalesOffer.projectName = salesOffer.ProjectName;
        //                    //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
        //                    //newSalesOffer.OfferType = salesOffer.OfferType;
        //                }

        //                //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
        //                //{
        //                //    ItemPrice = a.ItemPrice,
        //                //    Quantity = a.Quantity,
        //                //    ProductID = a.Id,
        //                //    productComment = a.ItemPricingComment
        //                //});

        //                //productList.AddRange(product);
        //                //newSalesOffer.ProductList = productList;
        //                //salesOfferList.Add(newSalesOffer);

        //            }
        //            rowNumAll++;
        //        }
        //        sheet2.Cells[2, 2].Value = sumAll;
        //        sheet2.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet2.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //        sheet2.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet2.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
        //        salesOffersDrawed.Clear();
        //        sumAll = 0;
        //        #endregion

        //        #region sheet for return (For all users)
        //        //------------------------------------sheet for return (For all users)----------------------------------------

        //        var sheet4 = excel.Workbook.Worksheets.Add($"Return_All_Users");



        //        //---------naming of Excel file--------------
        //        for (int col = 1; col <= 13; col++) sheet4.Column(col).Width = 20;

        //        sheet4.Cells[1, 1].Value = "Code";
        //        sheet4.Cells[1, 2].Value = "Ticket Price" + " \r\n " + "القيمه";
        //        sheet4.Cells[1, 3].Value = "Category \n القسم";
        //        sheet4.Cells[1, 4].Value = "Doctor Name \n الطبيب";
        //        sheet4.Cells[1, 5].Value = "Ticket number \n  رقم التذكره";
        //        sheet4.Cells[1, 6].Value = "patient \n المريض";
        //        sheet4.Cells[1, 7].Value = "Notes \n الملاحظات";
        //        sheet4.Cells[1, 8].Value = "Contact Person \n بيد";
        //        sheet4.Cells[1, 9].Value = "Creation Date \n التاريخ";
        //        sheet4.Cells[1, 10].Value = "offer Type \n النوع";
        //        sheet4.Cells[1, 11].Value = "Doctor Team \n التخصص";
        //        sheet4.Cells[1, 12].Value = "Item Name \n التشخيص";
        //        sheet4.Cells[1, 13].Value = "Created By \n المستخدم";

        //        sheet4.Cells[1, 1, 1, 13].Style.WrapText = true;
        //        //sheet.Cells[1, 4].Value = "Offer Name";
        //        //sheet.Cells[1, 5].Value = "final Offer Price";
        //        //sheet.Cells[1, 10].Value = "Item Price";
        //        //sheet.Cells[1, 6].Value = "Internal ID";
        //        //sheet.Cells[1, 5].Value = "Category Name";

        //        sheet4.Column(2).Style.Numberformat.Format = "#,##0.00";

        //        sheet4.DefaultRowHeight = 20;
        //        sheet4.Row(1).Height = 40;
        //        sheet4.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet4.Row(1).Style.Font.Bold = true;
        //        sheet4.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet4.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
        //        sheet4.Cells[1, 1, 1, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet4.Cells[1, 1, 1, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        decimal testSumAllReturn = 0;
        //        decimal testSubAllReturn = 0;
        //        decimal sumAllReturn = 0;
        //        decimal cashSumAllReturn = 0;
        //        int rowNumAllReturn = 3;

        //        int codeAllReturn = 1;

        //        var salesOffersDrawedReturn = new List<long>();

        //        var salesOffersReturn = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket return")).ToList();
        //        foreach (var salesOffer in salesOffersReturn)
        //        {

        //            //newSalesOffer.OfferID = item.OrderId;
        //            //var salesOffer = salesOffers.Where(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
        //            //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);
        //            if (salesOffer != null)
        //            {
        //                var alreadyDrawed = salesOffersDrawedReturn.Contains(salesOffer.Id);
        //                if (salesOffer != null && !alreadyDrawed)
        //                {
        //                    var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;

        //                    sheet4.Row(rowNumAllReturn).OutlineLevel = 1;
        //                    sheet4.Row(rowNumAllReturn).Collapsed = false;
        //                    sheet4.Cells[rowNumAllReturn, 1].Value = codeAllReturn; codeAllReturn++;
        //                    sheet4.Cells[rowNumAllReturn, 2].Value = salesOffer.FinalOfferPrice;
        //                    sheet4.Cells[rowNumAllReturn, 4].Value = salesOffer.SalesPerson.FirstName + " " + salesOffer.SalesPerson.LastName;
        //                    sheet4.Cells[rowNumAllReturn, 5].Value = salesOffer.ProjectName;
        //                    sheet4.Cells[rowNumAllReturn, 6].Value = salesOffer.Client.Name;
        //                    //sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;sheet.Cells[rowNum, 6].Value = salesOffer.Id;
        //                    sheet4.Cells[rowNumAllReturn, 8].Value = salesOffer.Client.SalesPerson.FirstName + " " + salesOffer.Client.SalesPerson.LastName;
        //                    sheet4.Cells[rowNumAllReturn, 9].Value = salesOffer.CreationDate.ToString();
        //                    sheet4.Cells[rowNumAllReturn, 10].Value = salesOffer.OfferType;
        //                    sheet4.Cells[rowNumAllReturn, 11].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
        //                    sheet4.Cells[rowNumAllReturn, 13].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
        //                    sheet4.Row(rowNumAllReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                    sheet4.Row(rowNumAllReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //                    //---------------sum of cash-----------------------------
        //                    if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSumAllReturn -= salesOffer.FinalOfferPrice ?? 0;
        //                    else { cashSumAll += salesOffer.FinalOfferPrice ?? 0; }


        //                    if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
        //                    {
        //                        sheet4.Cells[rowNumAllReturn, 1, rowNumAllReturn, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //                        sheet4.Cells[rowNumAllReturn, 1, rowNumAllReturn, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
        //                        sheet4.Row(rowNumAllReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        sheet4.Row(rowNumAllReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //                    }
        //                    //-------------------------------------------------------
        //                    foreach (var productDB in salesOffer.SalesOfferProducts)
        //                    {
        //                        sheet4.Row(rowNumAllReturn).OutlineLevel = 2;
        //                        sheet4.Row(rowNumAllReturn).Collapsed = true;
        //                        sheet4.Cells[rowNumAllReturn, 7].Value = productDB.ItemPricingComment;
        //                        //sheet.Cells[rowNum , 9].Value = productDB.Quantity;
        //                        //sheet.Cells[rowNum , 4].Value = productDB.ItemPrice;
        //                        sheet4.Cells[rowNumAllReturn, 3].Value = productDB.InventoryItemCategory.Name;
        //                        sheet4.Cells[rowNumAllReturn, 12].Value = productDB.InventoryItem.Name;
        //                        sheet4.Row(rowNumAllReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //                        sheet4.Row(rowNumAllReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



        //                        //rowNum++;
        //                    }
        //                    if (salesOffer.OfferType.Contains("Internal Ticket return")) { sumAllReturn = sumAllReturn - salesOffer.FinalOfferPrice ?? 0; }
        //                    else { sumAllReturn += salesOffer.FinalOfferPrice ?? 0; }


        //                    salesOffersDrawed.Add(salesOffer.Id);

        //                    //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
        //                    //newSalesOffer.projectName = salesOffer.ProjectName;
        //                    //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
        //                    //newSalesOffer.OfferType = salesOffer.OfferType;
        //                }

        //                //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
        //                //{
        //                //    ItemPrice = a.ItemPrice,
        //                //    Quantity = a.Quantity,
        //                //    ProductID = a.Id,
        //                //    productComment = a.ItemPricingComment
        //                //});

        //                //productList.AddRange(product);
        //                //newSalesOffer.ProductList = productList;
        //                //salesOfferList.Add(newSalesOffer);

        //            }
        //            rowNumAllReturn++;
        //        }
        //        sheet4.Cells[2, 2].Value = sumAllReturn;
        //        sheet4.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet4.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //        sheet4.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet4.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
        //        salesOffersDrawedReturn.Clear();
        //        sumAll = 0;
        //        //------------------------------------------------------------------------------------------------------------
        //        #endregion


        //        //-----------------------sheet for TotalAmountForEachCategory--------------------------------------------------
        //        var sheet3 = excel.Workbook.Worksheets.Add($"TotalAmountForEachCategory");

        //        TimeSpan difference = endDate - startDate;

        //        // Get the number of days
        //        int daysDifference = difference.Days;

        //        var inventoryCategories = _unitOfWork.InventoryItemCategories.GetAll();

        //        var salesOfferList = _unitOfWork.SalesOffers.FindAll(a => a.CreationDate.Date >= startDate.Date && a.CreationDate.Date <= endDate.Date && a.OfferType.Contains("Ticket"));

        //        var salesOfferIDs = salesOfferList.Select(a => a.Id).ToList();

        //        // the real data to show
        //        var salesOfferProduct = _unitOfWork.SalesOfferProducts.FindAll(a => a.CreationDate.Date >= startDate.Date && a.CreationDate.Date <= endDate.Date && salesOfferIDs.Contains(a.OfferId), new[] { "Offer", "InventoryItem", "InventoryItemCategory" });

        //        var categoriesIDs = salesOfferProduct.Select(a => a.InventoryItemCategoryId).Distinct().ToList();
        //        var categoriesNamesList = inventoryCategories.Where(a => categoriesIDs.Contains(a.Id)).ToList();

        //        //ExcelPackage excel = new ExcelPackage();
        //        //var sheet = excel.Workbook.Worksheets.Add($"sheet1");
        //        for (int col = 1; col <= categoriesNamesList.Count() + 1; col++) sheet3.Column(col).Width = 20;
        //        sheet3.DefaultRowHeight = 12;
        //        sheet3.Row(1).Height = 20;
        //        sheet3.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet3.Row(1).Style.Font.Bold = true;
        //        sheet3.Cells[1, 1, 1, categoriesNamesList.Count() + 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet3.Cells[1, 1, 1, categoriesNamesList.Count() + 2].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        //        sheet3.Cells[1, 1, 100, categoriesNamesList.Count() + 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        sheet3.Cells[1, 1, 100, categoriesNamesList.Count() + 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

        //        var drawCol = 2;
        //        foreach (var cat in categoriesNamesList)
        //        {
        //            sheet3.Cells[1, drawCol].Value = cat.Name;
        //            drawCol++;

        //        }

        //        sheet3.Cells[1, drawCol, 1, drawCol].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet3.Cells[1, drawCol, 1, drawCol].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);

        //        sheet3.Cells[1, 1].Value = "التاريخ";
        //        sheet3.Cells[1, drawCol].Value = "الاجمالي";

        //        var column = 2;
        //        var row = 2;
        //        var curserDate = startDate.Date;
        //        for (int i = 0; i <= daysDifference; i++)
        //        {
        //            column = 2;
        //            sheet3.Cells[row, 1].Value = curserDate.ToShortDateString();
        //            decimal rowTotal = 0;
        //            foreach (var category in categoriesNamesList)
        //            {

        //                var categoryDayTotalSales = salesOfferProduct.Where(a => a.Offer.OfferType.Contains("Internal Ticket") && a.CreationDate.Date >= curserDate.Date && a.CreationDate.Date < curserDate.AddDays(1).Date && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

        //                var categoryDayTotalReturn = salesOfferProduct.Where(a => a.Offer.OfferType.Contains("Internal Ticket return") && a.CreationDate.Date >= curserDate.Date && a.CreationDate.Date < curserDate.AddDays(1).Date && a.InventoryItemCategoryId == category.Id).Sum(a => a.FinalPrice);

        //                var categoryDayTotal = categoryDayTotalSales - categoryDayTotalReturn;
        //                sheet3.Cells[row, column].Value = categoryDayTotal;
        //                rowTotal = rowTotal + categoryDayTotal ?? 0;

        //                column++;
        //            }
        //            if (i == 0)
        //            {
        //                curserDate = curserDate.AddDays(i + 1);
        //                i++;
        //            }
        //            else
        //            {
        //                curserDate = startDate.AddDays(i);
        //            }
        //            sheet3.Cells[row, column].Value = rowTotal;
        //            sheet3.Cells[row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            sheet3.Cells[row, column].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
        //            sheet3.Cells[2, 2, row + 1, column].Style.Numberformat.Format = "#,##0.00"; // Apply to range A1:D10
        //            sheet3.Column(column).Width = 20;                                       // width of the Total column

        //            row++;
        //        }

        //        var columnTotalcurser = 2;
        //        for (int i = 0; i < column; i++)
        //        {
        //            var columnLetter = GetExcelColumnName(columnTotalcurser);
        //            string formula = $"SUM({columnLetter}{2}:{columnLetter}{row - 1})";  // Dynamic formula for column (e.g., SUM(A:A))
        //            sheet3.Cells[row, columnTotalcurser].Formula = formula; columnTotalcurser++;
        //        }

        //        sheet3.Cells[row, 1, row, column].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet3.Cells[row, 1, row, column].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
        //        sheet3.Cells[row, 1].Value = "الاجمالي";


        //        sheet3.Cells[1, 1, row - 1, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
        //        sheet3.Cells[1, 1, row - 1, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

        //        //--------------------------------------------------------------------------------------------


        //        var path = $"Attachments\\{CompName}\\GetSalesOfferTicketsForStore";
        //        var savedPath = Path.Combine(_host.WebRootPath, path);
        //        if (File.Exists(savedPath))
        //            File.Delete(savedPath);

        //        //delete older files
        //        if (Directory.Exists(savedPath))
        //        {
        //            foreach (var file in Directory.GetFiles(savedPath))
        //            {
        //                File.Delete(file);
        //            }
        //        };

        //        // Create excel file on physical disk  
        //        if (!Directory.Exists(savedPath)) Directory.CreateDirectory(savedPath);
        //        //FileStream objFileStrm = File.Create(savedPath);
        //        //objFileStrm.Close();
        //        var dateNow = DateTime.Now.ToString("yyyyMMddHHssFFF");
        //        var excelPath = savedPath + $"\\GetSalesOfferTicketsForStoreReport_{dateNow}.xlsx";
        //        excel.SaveAs(excelPath);
        //        // Write content to excel file  
        //        //File.WriteAllBytes(savedPath, excel.GetAsByteArray());
        //        //Close Excel package 
        //        excel.Dispose();
        //        var filePath = Globals.baseURL + '/' + path + $"\\GetSalesOfferTicketsForStoreReport_{dateNow}.xlsx";

        //        response.Data = filePath;
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.Result = false;
        //        Error error = new Error();
        //        error.ErrorCode = "Err10";
        //        error.ErrorMSG = ex.Message;
        //        response.Errors.Add(error);
        //        return response;
        //    }
        //}

        #endregion
        /*
         #region sheet for return (For all users)
                //------------------------------------sheet for return (For all users)----------------------------------------

                var sheet4 = excel.Workbook.Worksheets.Add($"Return_All_Users");



                //---------naming of Excel file--------------
                for (int col = 1; col <= 13; col++) sheet4.Column(col).Width = 20;

                sheet4.Cells[1, 1].Value = "Code";
                sheet4.Cells[1, 2].Value = "Ticket Price" + " \r\n " + "القيمه";
                sheet4.Cells[1, 3].Value = "Category \n القسم";
                sheet4.Cells[1, 4].Value = "Doctor Name \n الطبيب";
                sheet4.Cells[1, 5].Value = "Ticket number \n  رقم التذكره";
                sheet4.Cells[1, 6].Value = "patient \n المريض";
                sheet4.Cells[1, 7].Value = "Notes \n الملاحظات";
                sheet4.Cells[1, 8].Value = "Contact Person \n بيد";
                sheet4.Cells[1, 9].Value = "Creation Date \n التاريخ";
                sheet4.Cells[1, 10].Value = "offer Type \n النوع";
                sheet4.Cells[1, 11].Value = "Doctor Team \n التخصص";
                sheet4.Cells[1, 12].Value = "Item Name \n التشخيص";
                sheet4.Cells[1, 13].Value = "Created By \n المستخدم";

                sheet4.Cells[1, 1, 1, 13].Style.WrapText = true;
                //sheet.Cells[1, 4].Value = "Offer Name";
                //sheet.Cells[1, 5].Value = "final Offer Price";
                //sheet.Cells[1, 10].Value = "Item Price";
                //sheet.Cells[1, 6].Value = "Internal ID";
                //sheet.Cells[1, 5].Value = "Category Name";

                sheet4.Column(2).Style.Numberformat.Format = "#,##0.00";

                sheet4.DefaultRowHeight = 20;
                sheet4.Row(1).Height = 40;
                sheet4.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Row(1).Style.Font.Bold = true;
                sheet4.Cells[1, 1, 1, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[1, 1, 1, 13].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
                sheet4.Cells[1, 1, 1, 13].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[1, 1, 1, 13].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                decimal testSumAllReturn = 0;
                decimal testSubAllReturn = 0;
                decimal sumAllReturn = 0;
                decimal cashSumAllReturn = 0;
                int rowNumAllReturn = 3;

                int codeAllReturn = 1;

                var salesOffersDrawedReturn = new List<long>();

                var salesOffersReturn = salesOffers.Where(a => a.OfferType.Contains("Internal Ticket return")).GroupBy(b => b.CreatedBy).ToList();
                foreach (var salesOffer in salesOffersReturn)
                {

                    //newSalesOffer.OfferID = item.OrderId;
                    //var salesOffer = salesOffers.Where(a => a.CreationDate.Date >= ValidationDate && a.CreationDate.Date < nextDay).FirstOrDefault();
                    //var sumiation = salesOffer.Sum(a => a.FinalOfferPrice);
                    if (salesOffer != null)
                    {
                        var alreadyDrawed = salesOffersDrawedReturn.Contains(salesOffer.Id);
                        if (salesOffer != null && !alreadyDrawed)
                        {
                            var teamId = salesOffer?.SalesPerson?.UserTeamUsers?.FirstOrDefault()?.TeamId;

                            sheet4.Row(rowNumAllReturn).OutlineLevel = 1;
                            sheet4.Row(rowNumAllReturn).Collapsed = false;
                            sheet4.Cells[rowNumAllReturn, 1].Value = codeAllReturn; codeAllReturn++;
                            sheet4.Cells[rowNumAllReturn, 2].Value = salesOffer.FinalOfferPrice;
                            sheet4.Cells[rowNumAllReturn, 4].Value = salesOffer.SalesPerson.FirstName + " " + salesOffer.SalesPerson.LastName;
                            sheet4.Cells[rowNumAllReturn, 5].Value = salesOffer.ProjectName;
                            sheet4.Cells[rowNumAllReturn, 6].Value = salesOffer.Client.Name;
                            //sheet.Cells[rowNum, 4].Value = salesOffer.OfferType;sheet.Cells[rowNum, 6].Value = salesOffer.Id;
                            sheet4.Cells[rowNumAllReturn, 8].Value = salesOffer.Client.SalesPerson.FirstName + " " + salesOffer.Client.SalesPerson.LastName;
                            sheet4.Cells[rowNumAllReturn, 9].Value = salesOffer.CreationDate.ToString();
                            sheet4.Cells[rowNumAllReturn, 10].Value = salesOffer.OfferType;
                            sheet4.Cells[rowNumAllReturn, 11].Value = teams?.Where(a => a.Id == teamId).FirstOrDefault()?.Name;
                            sheet4.Cells[rowNumAllReturn, 13].Value = salesOffer.CreatedByNavigation.FirstName + " " + salesOffer.CreatedByNavigation.LastName;
                            sheet4.Row(rowNumAllReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            sheet4.Row(rowNumAllReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            //---------------sum of cash-----------------------------
                            if (salesOffer.OfferType.Contains("Internal Ticket return")) cashSumAllReturn -= salesOffer.FinalOfferPrice ?? 0;
                            else { cashSumAll += salesOffer.FinalOfferPrice ?? 0; }


                            if (salesOffer.OfferType.Contains("Internal Ticket return"))                    //make the row color red on Sales Return
                            {
                                sheet4.Cells[rowNumAllReturn, 1, rowNumAllReturn, 13].Style.Fill.PatternType = ExcelFillStyle.Solid;
                                sheet4.Cells[rowNumAllReturn, 1, rowNumAllReturn, 13].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Salmon);
                                sheet4.Row(rowNumAllReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet4.Row(rowNumAllReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                            }
                            //-------------------------------------------------------
                            foreach (var productDB in salesOffer.SalesOfferProducts)
                            {
                                sheet4.Row(rowNumAllReturn).OutlineLevel = 2;
                                sheet4.Row(rowNumAllReturn).Collapsed = true;
                                sheet4.Cells[rowNumAllReturn, 7].Value = productDB.ItemPricingComment;
                                //sheet.Cells[rowNum , 9].Value = productDB.Quantity;
                                //sheet.Cells[rowNum , 4].Value = productDB.ItemPrice;
                                sheet4.Cells[rowNumAllReturn, 3].Value = productDB.InventoryItemCategory.Name;
                                sheet4.Cells[rowNumAllReturn, 12].Value = productDB.InventoryItem.Name;
                                sheet4.Row(rowNumAllReturn).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                sheet4.Row(rowNumAllReturn).Style.VerticalAlignment = ExcelVerticalAlignment.Center;



                                //rowNum++;
                            }
                            if (salesOffer.OfferType.Contains("Internal Ticket return")) { sumAllReturn = sumAllReturn - salesOffer.FinalOfferPrice ?? 0; }
                            else { sumAllReturn += salesOffer.FinalOfferPrice ?? 0; }


                            salesOffersDrawed.Add(salesOffer.Id);

                            //newSalesOffer.finalOfferPrice = salesOffer.FinalOfferPrice ?? 0;
                            //newSalesOffer.projectName = salesOffer.ProjectName;
                            //newSalesOffer.CreationDate = salesOffer.CreationDate.ToShortDateString();
                            //newSalesOffer.OfferType = salesOffer.OfferType;
                        }

                        //var product = salesOffer.SalesOfferProducts.Select(a => new SalesOfferProductPOS()
                        //{
                        //    ItemPrice = a.ItemPrice,
                        //    Quantity = a.Quantity,
                        //    ProductID = a.Id,
                        //    productComment = a.ItemPricingComment
                        //});

                        //productList.AddRange(product);
                        //newSalesOffer.ProductList = productList;
                        //salesOfferList.Add(newSalesOffer);

                    }
                    rowNumAllReturn++;
                }
                sheet4.Cells[2, 2].Value = sumAllReturn;
                sheet4.Cells[2, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet4.Cells[2, 2].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                sheet4.Cells[2, 2].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet4.Cells[2, 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                salesOffersDrawedReturn.Clear();
                sumAll = 0;
                //------------------------------------------------------------------------------------------------------------
                #endregion*/

        public async Task<BaseMessageResponse> SalesOfferExcel(SalesOfferExcelfilters filter)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    #region SalesOffer Query list
                    var SalesOfferDBQuery = _unitOfWork.SalesOffers.FindAllQueryable(a => a.Active == true, includes: new[] { "SalesPerson", "SalesOfferExtraCosts", "SalesOfferInvoiceTaxes", "SalesOfferDiscounts", "SalesOfferProducts", "Client", "Projects.InventoryMatrialRequestItems" }).AsQueryable();

                    if (filter.SalesPersonId != 0)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesPersonId == filter.SalesPersonId).AsQueryable();
                    }
                    if (filter.BranchId != 0)
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.BranchId == filter.BranchId).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filter.ProductType))
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProductType.ToLower() == filter.ProductType).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filter.ClientName))
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.Client.Name.ToLower().Contains(filter.ClientName)).AsQueryable();
                    }
                    if (!string.IsNullOrEmpty(filter.ProjectName))
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ProjectName.ToLower().Contains(filter.ProjectName)).AsQueryable();
                    }
                    if (!string.IsNullOrWhiteSpace(filter.OfferStatus))
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.Where(x => x.Status.ToLower().Trim() == filter.OfferStatus);
                    }
                    if (filter.DateFilter)
                    {
                        if (filter.OfferStatus == "closed" || filter.OfferStatus == "rejected")
                        {
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= filter.FromDate && a.ClientApprovalDate <= filter.ToDate).AsQueryable();
                        }
                        else
                        {
                            if (filter.FromDate is DateTime from && filter.ToDate is DateTime to)
                            {
                                SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.StartDate >= DateOnly.FromDateTime(from) && a.EndDate <= DateOnly.FromDateTime(to)).AsQueryable();
                            }
                        }
                    }
                    else
                    {
                        if (filter.OfferStatus == "closed" || filter.OfferStatus == "rejected")
                        {
                            var ThisYearStartDate = new DateTime(DateTime.Now.Year, 1, 1).Date;
                            SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.ClientApprovalDate >= ThisYearStartDate).AsQueryable();
                        }
                    }
                    //if (ProductsIdsList.Count > 0)
                    //{
                    //    SalesOfferDBQuery = SalesOfferDBQuery.Where(a => a.SalesOfferProducts.Any(x => ProductsIdsList.Contains(x.InventoryItemID ?? 0))).AsQueryable();
                    //}

                    if (filter.OfferStatus == "closed" || filter.OfferStatus == "rejected")
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.ClientApprovalDate);
                    }
                    else
                    {
                        SalesOfferDBQuery = SalesOfferDBQuery.OrderByDescending(a => a.CreationDate);
                    }


                    var OffersListDB = PagedList<SalesOffer>.Create(SalesOfferDBQuery, filter.CurrentPage, filter.NumberOfItemsPerPage);


                    var OffersListResponse = new List<SalesOfferForReport>();

                    foreach (var offer in OffersListDB)
                    {
                        //var totalExtraCostAmount = offer.SalesOfferExtraCosts?.Sum(a => (decimal?)a.Amount);
                        //var totalTaxAmount = offer.SalesOfferInvoiceTaxes.Where(a => a.Active == true).Sum(a => (decimal?)a.TaxValue);
                        //var totalDiscountAmount = offer.SalesOfferDiscounts?.Where(a => a.DiscountApproved == true && a.Active == true).Sum(a => (decimal?)a.DiscountValue);

                        decimal TotalSalesOfferAvgPrice = 0;
                        decimal TotalSalesOfferProfitMarginValue = 0;
                        decimal TotalSalesOfferProfitMarginPer = 0;

                        foreach (var product in offer.SalesOfferProducts)
                        {
                            var ItemPrice = product.ItemPrice ?? 0;
                            var ItemProfitPer = product.ProfitPercentage ?? 0;
                            var ItemRemainQty = product.RemainQty ?? product.Quantity ?? 0;

                            var ItemAvg = 100 * ItemPrice / (100 + ItemProfitPer);

                            TotalSalesOfferProfitMarginValue += (ItemPrice - (100 * ItemPrice / (100 + ItemProfitPer))) * (decimal)ItemRemainQty;

                            TotalSalesOfferAvgPrice += ItemAvg * (decimal)ItemRemainQty;
                        }

                        if (TotalSalesOfferAvgPrice > 0)
                        {
                            TotalSalesOfferProfitMarginPer = TotalSalesOfferProfitMarginValue / TotalSalesOfferAvgPrice * 100;
                        }






                        long? projectId = null;
                        decimal QTYOfMatrialReleaseItem = 0;
                        var QTYOfSalesOfferProduct = offer.SalesOfferProducts?.Sum(a => a.RemainQty ?? 0) ?? 0;
                        var SalesOfferProductCount = offer.SalesOfferProducts?.Count ?? 0;
                        if (offer.Projects.Count > 0)
                        {
                            var offerProject = offer.Projects.FirstOrDefault();
                            projectId = offerProject.Id;
                            if (offerProject.InventoryMatrialRequestItems.Count > 0)
                            {
                                QTYOfMatrialReleaseItem = (decimal)(offerProject.InventoryMatrialRequestItems?.Sum(x => x.RecivedQuantity ?? 0) ?? 0);
                            }
                        }
                        decimal Percent = 0;
                        string ReleaseStatus = "";
                        if (QTYOfSalesOfferProduct > 0)
                        {
                            ReleaseStatus = "Part";
                            Percent = QTYOfMatrialReleaseItem / (decimal)QTYOfSalesOfferProduct * 100;

                            if (Percent > 100)
                            {
                                ReleaseStatus = "Exceeded";
                            }
                            if (Percent == 100)
                            {
                                ReleaseStatus = "Full";
                            }
                        }
                        SalesOfferForReport salesOfferObj = new SalesOfferForReport()
                        {
                            Id = offer.Id,
                            ProjectName = offer.ProjectName,
                            SalesPersonName = offer.SalesPerson?.FirstName + ' ' + offer.SalesPerson?.MiddleName + ' ' + offer.SalesPerson?.LastName,
                            ClientName = offer.Client.Name,
                            OfferSerial = offer.OfferSerial,
                            OfferStatus = offer.Status,
                            PercentReleasedValue = Percent,
                            FinalOfferPrice = offer.FinalOfferPrice,
                            ClientApprovalDate = offer.ClientApprovalDate != null ? offer.ClientApprovalDate.ToString().Split(' ')[0] : null,
                            OfferType = offer.OfferType,
                            CreationDate = offer.CreationDate.ToShortDateString(),
                            GrossProfitPercentage = TotalSalesOfferProfitMarginPer,
                            GrossProfitValue = TotalSalesOfferProfitMarginValue,
                            DiscountOrExtraCostPerSalesPerson = offer.ExtraOrDiscountPriceBySalesPerson,
                            OfferAmount = offer.OfferAmount,
                            TaxValue = (offer.SalesOfferProducts.SelectMany(x => x.SalesOfferProductTaxes?.Select(y => y.Value ?? 0).ToList()).DefaultIfEmpty(0).Sum()) +
                            (offer.SalesOfferInvoiceTaxes?.Where(x => x.Active == true).Select(x => x.TaxValue).DefaultIfEmpty(0).Sum()),
                            Discount = offer.SalesOfferProducts.Where(x => x.Active == true).Select(x => x.DiscountValue).DefaultIfEmpty(0).Sum() +
                            (offer.SalesOfferDiscounts?.Where(x => x.Active == true).Select(x => x.DiscountValue).DefaultIfEmpty(0).Sum()),
                            ExtraCost = (offer.SalesOfferExtraCosts?.Where(x => x.Active == true).Select(x => x.Amount).DefaultIfEmpty(0).Sum()),

                        };

                        var OfferClientAccount = _Context.ClientAccounts.Where(a => a.OfferId == offer.Id).FirstOrDefault();
                        if (OfferClientAccount != null)
                        {
                            salesOfferObj.HasJournalEntryId = OfferClientAccount.DailyAdjustingEntryId;
                        }

                        //var SalesOfferInvoices = GetSalesOfferInvoicesList(offer.ID);
                        //if (SalesOfferInvoices != null && SalesOfferInvoices.Count > 0)
                        //{
                        //    salesOfferObj.SalesOfferInvoices = SalesOfferInvoices;
                        //}
                        OffersListResponse.Add(salesOfferObj);
                    }
                    var SalesOfferList = OffersListResponse;
                    //Response.SalesOfferList = salesOfferTypeDetails;

                    #endregion
                    var dt = new System.Data.DataTable("Grid");

                    dt.Columns.AddRange(new DataColumn[17] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Project Name"),
                                                     new DataColumn("Sales Person"),
                                                     new DataColumn("Client Name"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("Release Percentage"),
                                                     new DataColumn("Final Offer Price"),
                                                     new DataColumn("Client approval date"),
                                                     new DataColumn("Project Type"),
                                                     new DataColumn("Creation Date"),
                                                     new DataColumn("Gross Profit Value"),
                                                     new DataColumn("Gross Profit Percentage"),
                                                     new DataColumn("Journal Entry ID"),
                                                     new DataColumn("Has Journal Entry"),
                                                     new DataColumn("Invoice ID"),
                                                     new DataColumn("Invoice Serial"),
                                                     new DataColumn("Has E Invoice"),
                                                     new DataColumn("Status")



                    });


                    var SalesOfferDetails = new System.Data.DataTable("Grid");

                    SalesOfferDetails.Columns.AddRange(new DataColumn[10] {
                                                     //new DataColumn("ID"),
                                                     new DataColumn("Project Name"),
                                                     new DataColumn("Sales Person"),
                                                     new DataColumn("Client Name"),
                                                     new DataColumn("Serial") ,
                                                     new DataColumn("Offer Amount"),
                                                     new DataColumn("Tax Value"),
                                                     new DataColumn("Extra Cost"),
                                                     new DataColumn("Discount"),
                                                     new DataColumn("Extra cost or discount by sales Person"),
                                                     new DataColumn("Final Offer Price")
                    });



                    if (SalesOfferList != null)
                    {
                        var SalesOfferListdb = SalesOfferList;
                        if (SalesOfferListdb != null)
                        {
                            var IDsalesofferList = SalesOfferListdb.Select(y => y.Id).ToList();
                            var InvoiceListDB = _unitOfWork.Invoices.FindAll(x => x.SalesOfferId != null ? IDsalesofferList.Contains((long)x.SalesOfferId) : false).ToList();
                            foreach (var item in SalesOfferListdb)
                            {

                                // Check if Sales offer Has Automatic JE Or Not


                                // Back Invoice ID , Serial , Have E-invoice
                                var InvoiceDB = InvoiceListDB.Where(x => x.SalesOfferId == item.Id).FirstOrDefault();
                                long InvoiceID = 0;
                                string InvoiceSerial = "";
                                bool HasEinvoice = false;
                                if (InvoiceDB != null)
                                {
                                    InvoiceID = InvoiceDB.Id;
                                    InvoiceSerial = InvoiceDB.Serial;
                                    HasEinvoice = InvoiceDB.EInvoiceId != null ? true : false;
                                }
                                //Math.Abs(-50)
                                dt.Rows.Add(
                                    //item != null ? item.ID : 0

                                    //item.Id != null ? item.Id : 0,
                                    item.ProjectName != null ? item.ProjectName : "N/A",
                                    item.SalesPersonName != null ? item.SalesPersonName : "N/A",
                                    item.ClientName != null ? item.ClientName : "N/A",
                                    item.OfferSerial != null ? item.OfferSerial : "N/A",
                                    item.PercentReleasedValue,
                                    item.FinalOfferPrice != null ? item.FinalOfferPrice : 0,
                                    item.ClientApprovalDate != null ? item.ClientApprovalDate : "N/A",
                                    item.OfferType != null ? item.OfferType : "N/A",
                                    item.CreationDate != null ? item.CreationDate : "N/A",
                                    item.GrossProfitValue != null ? item.GrossProfitValue : 0,
                                    item.GrossProfitPercentage != null ? item.GrossProfitPercentage : 0,
                                    item.HasJournalEntryId != null ? item.HasJournalEntryId : 0,
                                    item.HasJournalEntryId != 0 && item.HasJournalEntryId != null ? true : false,
                                    InvoiceID,
                                    InvoiceSerial,
                                    HasEinvoice,
                                    item.OfferStatus

                                    );

                                //decimal OfferTax = item.salesofferpr

                                SalesOfferDetails.Rows.Add(
                                    item.ProjectName != null ? item.ProjectName : "N/A",
                                    item.SalesPersonName != null ? item.SalesPersonName : "N/A",
                                    item.ClientName != null ? item.ClientName : "N/A",
                                    item.OfferSerial != null ? item.OfferSerial : "N/A",
                                    item.OfferAmount ?? 0,
                                    item.TaxValue ?? 0,
                                    item.ExtraCost ?? 0,
                                    item.Discount ?? 0,
                                    item.DiscountOrExtraCostPerSalesPerson ?? 0,
                                    item.FinalOfferPrice ?? 0


                                    );
                            }
                        }


                    }
                    using (var package = new ExcelPackage())
                    {





                        var CompanyName = validation.CompanyName.ToString().ToLower();

                        string FullFileName = DateTime.Now.ToFileTime() + "_" + "SalesOffer.xlsx";
                        string PathsTR = "/Attachments/" + CompanyName + "/";
                        String path = Path.Combine(_host.WebRootPath,PathsTR);
                        string p_strPath = Path.Combine(path, FullFileName);


                        var workSheet = package.Workbook.Worksheets.Add("SalesOffer");
                        //workSheet.TabColor = Color.Red;
                        workSheet.Columns.BestFit = true;
                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        workSheet.Cells["A1"].LoadFromDataTable(dt, true);
                        //Format the header for column 1-3
                        using (ExcelRange range = workSheet.Cells["A1:O1"])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            range.Style.Font.Color.SetColor(Color.White);
                        }

                        var workSheetSalesOfferDetails = package.Workbook.Worksheets.Add("SalesOfferDetails");
                        //workSheet.TabColor = Color.Red;
                        workSheetSalesOfferDetails.Columns.BestFit = true;
                        //Load the datatable into the sheet, starting from cell A1. Print the column names on row 1
                        workSheetSalesOfferDetails.Cells["A1"].LoadFromDataTable(SalesOfferDetails, true);
                        //Format the header for column 1-3
                        using (ExcelRange range = workSheet.Cells["A1:O1"])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189)); //Set color to dark blue
                            range.Style.Font.Color.SetColor(Color.White);
                        }




                        //  var workSheet2 = package.Workbook.Worksheets.Add("SalesOffer2");
                        ExcelRangeBase excelRangeBase = workSheet.Cells.LoadFromDataTable(dt, true);
                        File.Exists(p_strPath);
                        FileStream objFileStrm = File.Create(p_strPath);


                        objFileStrm.Close();
                        await package.SaveAsync();
                        File.WriteAllBytes(p_strPath, await package.GetAsByteArrayAsync());
                        package.Dispose();

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
                error.ErrorMSG = ex.Message + " - " + ex.InnerException.Message + " - " + ex.InnerException.InnerException.Message + " - " + ex.InnerException.InnerException.InnerException.Message;//ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

    }
}