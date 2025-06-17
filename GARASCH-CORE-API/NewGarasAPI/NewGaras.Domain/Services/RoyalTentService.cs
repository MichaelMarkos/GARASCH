using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Models;
using NewGaras.Infrastructure.Models.RoyalTent;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NewGaras.Domain.Services
{
    public class RoyalTentService : IRoyalTentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
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
        public RoyalTentService(IUnitOfWork unitOfWork, IMapper mapper, IWebHostEnvironment host)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _host = host;
        }

        public FileInfo GetFileRandom()
        {
            Random rnd = new Random();
            int NO = rnd.Next(1, 10);
            string sourceFile = @"C:\inetpub\wwwroot\GarasWebShared\Attachments\royaltent\RoyalTentPricing.xlsx";
            //string sourceFile = @"D:\RoyalTentPricing.xlsx";
            string destinationFile = @"C:\inetpub\wwwroot\GarasWebShared\Attachments\royaltent\RoyalTentPricing_" + NO + DateTime.Now.ToString("HHMMSS") + ".xlsx";
            //string destinationFile = @"D:\RoyalTentPricing_" + NO+DateTime.Now.ToString("HHMMSS")+".xlsx";
            File.Copy(sourceFile, destinationFile, true);
            FileInfo fileWrite = new FileInfo(destinationFile);
            return fileWrite;
        }

        public async Task<BaseMessageExcelResponse> RoyalTelesqupUmbrellaCalculator(string Paint, string Sales, string Size, string Cloth, string Fronton)
        {

            BaseMessageExcelResponse Response = new BaseMessageExcelResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                FileInfo fileWrite = GetFileRandom();

                using (ExcelPackage excelPackage = new ExcelPackage(fileWrite))
                {
                    ExcelWorkbook excelWorkBook = excelPackage.Workbook;
                    ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.Where(x => x.Name == "تسعير").FirstOrDefault();
                    excelWorksheet.Cells["AG28"].Value = Sales;
                    excelWorksheet.Cells["AH28"].Value = Size;
                    excelWorksheet.Cells["AI28"].Value = Paint;
                    excelWorksheet.Cells["AJ28"].Value = Cloth;
                    excelWorksheet.Cells["AK28"].Value = Fronton;

                    excelPackage.Workbook.Calculate();

                    await excelPackage.SaveAsAsync(fileWrite);

                    var TotalPrice = excelWorksheet.Cells["AG22"].Value.ToString();

                    try
                    {
                        Response.TotalPrice = Decimal.Round(decimal.Parse(TotalPrice), 2);
                    }
                    catch (Exception ex)
                    {
                        Response.Result = false;
                        Error error = new Error();
                        error.ErrorCode = "Err-Excel";
                        error.ErrorMSG = "Error From Excel";
                        Response.Errors.Add(error);
                    }

                    Response.TotalPrice = Decimal.Round(decimal.Parse(TotalPrice), 2);
                    Response.Message = fileWrite.ToString();

                    excelPackage.Dispose();
                    if (File.Exists(Response.Message))
                    {
                        File.Delete(Response.Message);
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

        public async Task<BaseMessageExcelResponse> RoyalTelesqupUmbrellaExcel(RoyalTelesqupUmbrellaFilters filters)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseMessageExcelResponse Response = new BaseMessageExcelResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    var CompanyName = validation.CompanyName.ToLower();
                    if (string.IsNullOrEmpty(filters.Sales))
                    
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Sales Is Mandatory";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;

                    }
                    if (string.IsNullOrEmpty(filters.Size))                 
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Size Is Mandatory";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;

                    }
                    if (string.IsNullOrEmpty(filters.Paint))                   
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Paint Is Mandatory";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;

                    }
                    if (string.IsNullOrEmpty(filters.Cloth))                   
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Cloth Is Mandatory";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;

                    }
                    if (string.IsNullOrEmpty(filters.Fronton))                  
                    {
                        Error error = new Error();
                        error.ErrorCode = "Err-13";
                        error.ErrorMSG = "Fronton Is Mandatory";
                        Response.Errors.Add(error);
                        Response.Result = false;
                        return Response;

                    }

                    Response = await RoyalTelesqupUmbrellaCalculator(filters.Paint, filters.Sales, filters.Size, filters.Cloth, filters.Fronton);
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

        public async Task<BaseMessageMainVariablesExcelResponse> MainVariablesRoyalTentExcel(MainVariablesRoyalTentFilters filters)
        {
            //BaseMessageResponse Response = new BaseMessageResponse();
            BaseMessageMainVariablesExcelResponse Response = new BaseMessageMainVariablesExcelResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                var CompanyName =validation.CompanyName.ToLower();
                if (Response.Result)
                {
                    FileInfo fileWrite = new FileInfo(@"C:\inetpub\wwwroot\GarasWebShared\Attachments\royaltent\RoyalTentPricing.xlsx");

                    //FileInfo fileWrite = new FileInfo(@"D:\RoyalTentPricing.xlsx");

                    using (ExcelPackage excelPackage = new ExcelPackage(fileWrite))
                    {
                        ExcelWorkbook excelWorkBook = excelPackage.Workbook;
                        ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets.Where(x => x.Name == "Sheet2").FirstOrDefault();

                        if (filters.Iron != 0)
                        {
                            excelWorksheet.Cells["B12"].Value = filters.Iron;
                        }
                        if (filters.Kemer != 0)
                        {
                            excelWorksheet.Cells["B13"].Value = filters.Kemer;
                        }
                        if (filters.Aluminium != 0)
                        {
                            excelWorksheet.Cells["B14"].Value = filters.Aluminium;

                        }
                        if (filters.WoodPaint != 0)
                        {
                            excelWorksheet.Cells["B15"].Value = filters.WoodPaint;

                        }
                        if (filters.BFC != 0)
                        {
                            excelWorksheet.Cells["B16"].Value = filters.BFC;
                        }
                        if (filters.PergolaSior != 0)
                        {

                            excelWorksheet.Cells["B17"].Value = filters.PergolaSior;
                        }
                        if (filters.Jotamastic87Grey != 0)
                        {
                            excelWorksheet.Cells["B18"].Value = filters.Jotamastic87Grey;

                        }
                        if (filters.Jotamastic80Grey != 0)
                        {
                            excelWorksheet.Cells["B19"].Value = filters.Jotamastic80Grey;

                        }
                        if (filters.HardTopXPWhiteBlackGrey != 0)
                        {
                            excelWorksheet.Cells["B20"].Value = filters.HardTopXPWhiteBlackGrey;

                        }

                        if (filters.HardTopXPColor != 0)
                        {

                            excelWorksheet.Cells["B21"].Value = filters.HardTopXPColor;
                        }
                        if (filters.Thinner17 != 0)
                        {

                            excelWorksheet.Cells["B22"].Value = filters.Thinner17;
                        }
                        if (filters.Thinner10 != 0)
                        {
                            excelWorksheet.Cells["B23"].Value = filters.Thinner10;

                        }
                        if (filters.IronMash != 0)
                        {
                            excelWorksheet.Cells["B24"].Value = filters.IronMash;

                        }

                        if (filters.Thinner != 0)
                        {

                            excelWorksheet.Cells["B25"].Value = filters.Thinner;
                        }
                        if (filters.PaintDoko != 0)
                        {

                            excelWorksheet.Cells["B26"].Value = filters.PaintDoko;
                        }
                        if (filters.KimaBoxCMB != 0)
                        {
                            excelWorksheet.Cells["B27"].Value = filters.KimaBoxCMB;

                        }
                        if (filters.WeldingWire != 0)
                        {

                            excelWorksheet.Cells["B28"].Value = filters.WeldingWire;
                        }
                        if (filters.WaterTrackByMeter != 0)
                        {
                            excelWorksheet.Cells["B29"].Value = filters.WaterTrackByMeter;

                        }
                        if (filters.OrdinaryConcreteBase != 0)
                        {

                            excelWorksheet.Cells["B30"].Value = filters.OrdinaryConcreteBase;
                        }
                        if (filters.SikaCMB != 0)
                        {

                            excelWorksheet.Cells["B31"].Value = filters.SikaCMB;
                        }
                        if (filters.PolyCarbonate10Min != 0)
                        {

                            excelWorksheet.Cells["B32"].Value = filters.PolyCarbonate10Min;
                        }
                        if (filters.PergolaMotor != 0)
                        {

                            excelWorksheet.Cells["B33"].Value = filters.PergolaMotor;
                        }
                        if (filters.RemoteControl != 0)
                        {

                            excelWorksheet.Cells["B34"].Value = filters.RemoteControl;
                        }
                        if (filters.FrontAccessorskitWithAluminumCover != 0)
                        {

                            excelWorksheet.Cells["B35"].Value = filters.FrontAccessorskitWithAluminumCover;
                        }
                        if (filters.BackAccessorskit != 0)
                        {

                            excelWorksheet.Cells["B36"].Value = filters.BackAccessorskit;
                        }
                        if (filters.RemoteControlForEngine != 0)
                        {

                            excelWorksheet.Cells["B37"].Value = filters.RemoteControlForEngine;
                        }
                        if (filters.Powerkey != 0)
                        {

                            excelWorksheet.Cells["B38"].Value = filters.Powerkey;
                        }
                        if (filters.LEDBulbsWithCover != 0)
                        {
                            excelWorksheet.Cells["B39"].Value = filters.LEDBulbsWithCover;

                        }
                        if (filters.GoldenPaintForAluminum != 0)
                        {
                            excelWorksheet.Cells["B40"].Value = filters.GoldenPaintForAluminum;

                        }
                        if (filters.RegularPaintForAluminum != 0)
                        {

                            excelWorksheet.Cells["B41"].Value = filters.RegularPaintForAluminum;
                        }



                        excelPackage.Workbook.Calculate();

                        await excelPackage.SaveAsAsync(fileWrite);


                        List<MainVariableExcel> ExcelVariables = new List<MainVariableExcel>();
                        ExcelVariables.AddRange(new List<MainVariableExcel>
                        {
                            new MainVariableExcel{Name = "Iron", Value = decimal.Parse(excelWorksheet.Cells["B12"].Value.ToString()) },
                            new MainVariableExcel{Name = "Kemer", Value = decimal.Parse(excelWorksheet.Cells["B13"].Value.ToString())},
                            new MainVariableExcel{Name = "Aluminium", Value = decimal.Parse(excelWorksheet.Cells["B14"].Value.ToString())},
                            new MainVariableExcel{Name = "WoodPaint", Value = decimal.Parse(excelWorksheet.Cells["B15"].Value.ToString())},
                            new MainVariableExcel{Name = "BFC", Value = decimal.Parse(excelWorksheet.Cells["B16"].Value.ToString())},
                            new MainVariableExcel{Name = "PergolaSir", Value = decimal.Parse(excelWorksheet.Cells["B17"].Value.ToString())},
                            new MainVariableExcel{Name = "Jotamastic87Grey", Value = decimal.Parse(excelWorksheet.Cells["B18"].Value.ToString())},
                            new MainVariableExcel{Name = "Jotamastic80Grey", Value = decimal.Parse(excelWorksheet.Cells["B19"].Value.ToString())},
                            new MainVariableExcel{Name = "HardTopXPWhiteBlackGrey", Value = decimal.Parse(excelWorksheet.Cells["B20"].Value.ToString())},
                            new MainVariableExcel{Name = "HardTopXPColor", Value = decimal.Parse(excelWorksheet.Cells["B21"].Value.ToString())},
                            new MainVariableExcel{Name = "Thinner17", Value = decimal.Parse(excelWorksheet.Cells["B22"].Value.ToString())},
                            new MainVariableExcel{Name = "Thinner10", Value = decimal.Parse(excelWorksheet.Cells["B23"].Value.ToString())},
                            new MainVariableExcel{Name = "IronMash", Value = decimal.Parse(excelWorksheet.Cells["B24"].Value.ToString())},
                            new MainVariableExcel{Name = "Thinner", Value = decimal.Parse(excelWorksheet.Cells["B25"].Value.ToString())},
                            new MainVariableExcel{Name = "PaintDoko", Value = decimal.Parse(excelWorksheet.Cells["B26"].Value.ToString())},
                            new MainVariableExcel{Name = "KimaBoxCMB", Value = decimal.Parse(excelWorksheet.Cells["B27"].Value.ToString())},
                            new MainVariableExcel{Name = "WeldingWire", Value = decimal.Parse(excelWorksheet.Cells["B28"].Value.ToString())},
                            new MainVariableExcel{Name = "WaterTrackByMeter", Value = decimal.Parse(excelWorksheet.Cells["B29"].Value.ToString())},
                            new MainVariableExcel{Name = "OrdinaryConcreteBase", Value = decimal.Parse(excelWorksheet.Cells["B30"].Value.ToString())},
                            new MainVariableExcel{Name = "SikaCMB", Value = decimal.Parse(excelWorksheet.Cells["B31"].Value.ToString())},
                            new MainVariableExcel{Name = "PolyCarbonate10Min", Value = decimal.Parse(excelWorksheet.Cells["B32"].Value.ToString())},
                            new MainVariableExcel{Name = "PergolaMotor", Value = decimal.Parse(excelWorksheet.Cells["B33"].Value.ToString())},
                            new MainVariableExcel{Name = "RemoteControl", Value = decimal.Parse(excelWorksheet.Cells["B34"].Value.ToString())},
                            new MainVariableExcel{Name = "FrontAccessorskitWithAluminumCover", Value = decimal.Parse(excelWorksheet.Cells["B35"].Value.ToString())},
                            new MainVariableExcel{Name = "BackAccessorskit", Value = decimal.Parse(excelWorksheet.Cells["B36"].Value.ToString())},
                            new MainVariableExcel{Name = "RemoteControlForEngine", Value = decimal.Parse(excelWorksheet.Cells["B37"].Value.ToString())},
                            new MainVariableExcel{Name = "Powerkey", Value = decimal.Parse(excelWorksheet.Cells["B38"].Value.ToString())},
                            new MainVariableExcel{Name = "LEDBulbsWithCover", Value = decimal.Parse(excelWorksheet.Cells["B39"].Value.ToString())},
                            new MainVariableExcel{Name = "GoldenPaintForAluminum", Value = decimal.Parse(excelWorksheet.Cells["B40"].Value.ToString())},
                            new MainVariableExcel{Name = "RegularPaintForAluminum", Value = decimal.Parse(excelWorksheet.Cells["B41"].Value.ToString())}
                         });
                        Response.MainVariablesList = ExcelVariables;

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

        public BaseResponse UpdateRoyalTentExcel(Stream stream)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                if (Response.Result)
                {
                    bool success = false;
                    string fileName = "RoyalTentPricing.xlsx";

                    string FilePath = Path.Combine(_host.WebRootPath+"/Attachments/royaltent", fileName);

                    if (File.Exists(FilePath))
                    {
                        File.Delete(FilePath);
                    }

                    int length = 0;
                    using (FileStream writer = new FileStream(FilePath, FileMode.Create))
                    {

                        int readCount;
                        var buffer = new byte[8192];
                        while ((readCount = stream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            writer.Write(buffer, 0, readCount);
                            length += readCount;
                        }
                        if (length > 0)
                        {
                            success = true;
                        }
                    }

                    Response.Result = success;
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
    }
}
