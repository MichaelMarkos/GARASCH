using Microsoft.AspNetCore.Hosting;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models;
using NewGarasAPI.Helper;
using NewGarasAPI.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace NewGaras.Domain.Services.ApplicationsVersion
{
    public class ApplicationVersionService : IApplicationVersionService
    {
        private readonly IWebHostEnvironment _host;
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
        public ApplicationVersionService(IWebHostEnvironment host)
        {
            _host = host;
        }
        public BaseResponseWithData<GetAppsVersionModel> GetApplicationVersion([FromHeader] string CompanyName)
        {
            BaseResponseWithData<GetAppsVersionModel> Response = new BaseResponseWithData<GetAppsVersionModel>();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                string filePath = Path.Combine(_host.WebRootPath, $"Attachments/{CompanyName.ToLower().Trim()}/ApplicationVersion.json");
                if(!File.Exists(filePath))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Application Version file is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                string jsonString = File.ReadAllText(filePath);

                var versions = JsonSerializer.Deserialize<GetAppsVersionModel>(jsonString);

                Response.Data = versions;
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

        public BaseResponse UpdateApplicationVersion(GetAppsVersionModel newVersion)
        {
            BaseResponse Response = new BaseResponse();
            Response.Result = true;
            Response.Errors = new List<Error>();
            try
            {
                string filePath = Path.Combine(_host.WebRootPath, $"Attachments/{validation.CompanyName.ToLower().Trim()}/ApplicationVersion.json");
                if (!File.Exists(filePath))
                {
                    Response.Result = false;
                    Error error = new Error();
                    error.ErrorCode = "Err101";
                    error.ErrorMSG = "Application Version file is not found";
                    Response.Errors.Add(error);
                    return Response;
                }
                string jsonString = File.ReadAllText(filePath);

                var versions = JsonSerializer.Deserialize<GetAppsVersionModel>(jsonString);
                if (newVersion.DesktopVersion != null)
                {
                    versions.DesktopVersion = newVersion.DesktopVersion.Trim().ToString();
                }
                if (newVersion.MobileVersion != null)
                {
                    versions.MobileVersion = newVersion.MobileVersion.Trim().ToString();
                }

                string updatedJsonString = JsonSerializer.Serialize(versions, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(filePath, updatedJsonString);

                Response.Result = true;
                return Response;
            }
            catch(Exception ex)
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
