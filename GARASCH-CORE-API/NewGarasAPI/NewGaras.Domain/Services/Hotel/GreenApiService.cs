using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Models.Hotel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services.Hotel
{
    public class GreenApiService : IGreenApiService
    {
        private readonly GreenApiSettings _settings;
        private IHostingEnvironment _environment;
        private readonly IUnitOfWork _unitOfWork;

        public GreenApiService(IOptions<GreenApiSettings> settings, IHostingEnvironment environment , IUnitOfWork unitOfWork)
        {
            _settings = settings.Value;
            _environment = environment;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> SendMessage(string mobile, string ContentMessage)
        {
            using HttpClient httpClient = new();


            BodyOfMessage body = new()
            {
                ChatId = mobile,
                Message = ContentMessage
            };


            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync(new Uri(_settings.ApiUrlMessage), body);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendPdfbyUrl(string mobile, string urlFile, string fileName, string caption)
        {
            using HttpClient httpClient = new();




            Sendpdfbyurl body = new()
            {
                chatId = mobile,
                urlFile = urlFile,
                fileName = fileName,
                caption = caption
            };


            HttpResponseMessage response =
                await httpClient.PostAsJsonAsync(new Uri(_settings.ApiUrlsendFileByUrl), body);

            return response.IsSuccessStatusCode;
        }


        public async Task<bool> UploadFormDataAsync( FormOfFileMessage formData ,long UserId)
        {
            using HttpClient httpClient = new();

            // Create a new MultipartFormDataContent to send the data
            var content = new MultipartFormDataContent();

            // Add the text fields to the form-data
            if (!string.IsNullOrEmpty(formData.chatId))
                content.Add(new StringContent((formData.chatId)+ "@c.us"), "chatId");

            if (!string.IsNullOrEmpty(formData.caption))
                content.Add(new StringContent(formData.caption), "caption");

            if (!string.IsNullOrEmpty(formData.fileName))
                content.Add(new StringContent(formData.fileName), "fileName");

            // Add the file to the form-data
            if (formData.file != null)
            {
                var fileContent = new StreamContent(formData.file.OpenReadStream());
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(formData.file.ContentType);
                content.Add(fileContent, "file", formData.file.FileName);
            }

            // Send the POST request with form-data
            // var response = await httpClient.PostAsync(apiUrl, content);


            HttpResponseMessage response =
                await httpClient.PostAsync(new Uri(_settings.ApiUrlMedia), content);
            if (response.IsSuccessStatusCode) 
            {
                 var resultResponse = await httpClient.GetAsync(_settings.ApiUrlMedia);
                var contents = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>( contents);

                _unitOfWork.LaboratoryMessagesReports.Add(new LaboratoryMessagesReport 
                {
                    Name = formData.Name,
                    Mobile = formData.chatId,
                    NameLab = formData.NameLab,
                    PdfUrl = apiResponse.urlFile,
                    Result = true,
                    CreateBy = UserId,
                    CreateDate = DateTime.Now,
                });
                _unitOfWork.Complete();
            }

            return response.IsSuccessStatusCode;
        }

    }
}
