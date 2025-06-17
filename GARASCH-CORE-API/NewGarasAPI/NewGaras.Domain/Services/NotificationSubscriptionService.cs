using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure;
using Task = System.Threading.Tasks.Task;
using NewGaras.Domain.Models;

namespace NewGaras.Domain.Services
{
    public class NotificationSubscriptionService : INotificationSubscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly IGraphAuthService _graphAuthService;
        private readonly IUnitOfWork _unitOfWork;


        public NotificationSubscriptionService(IGraphAuthService graphAuthService, IUnitOfWork unitOfWork)
        {
            _httpClient = new HttpClient();
            _graphAuthService = graphAuthService;
            _unitOfWork = unitOfWork;
        }

        public async Task<NewGaras.Domain.Models.BaseResponseWithId<long>> CreateSubscriptionAsync(string UserEmail)
        {
            var response = new BaseResponseWithId<long>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var accessToken = await _graphAuthService.GetAccessTokenAsync();

                // Set the authorization header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Define the subscription request payload
                var subscription = new
                {
                    changeType = "created",
                    notificationUrl = "https://a620-41-233-14-228.ngrok-free.app/test",
                    resource = $"/users/{UserEmail}/mailFolders('Inbox')/messages",
                    expirationDateTime = DateTime.UtcNow.AddMinutes(4230).ToString("o"),
                    clientState = "customClientState"
                };

                // Send the subscription request to Microsoft Graph
                var content = new StringContent(JsonConvert.SerializeObject(subscription), Encoding.UTF8, "application/json");
                var microsoftResponse = await _httpClient.PostAsync("https://graph.microsoft.com/v1.0/subscriptions", content);

                if (microsoftResponse.IsSuccessStatusCode)
                {
                    microsoftResponse.EnsureSuccessStatusCode();

                    // Read and parse the response content
                    var responseContent = await microsoftResponse.Content.ReadAsStringAsync();

                    // Deserialize the response JSON to extract the fields
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);

                    var subscriptionId = responseData.GetProperty("id").GetString();
                    var expirationDateString = responseData.GetProperty("expirationDateTime").GetString();
                    var expirationDate = DateTime.Parse(expirationDateString);

                    //var newNotification = new NotificationSubscription();
                    //newNotification.SubscriptionId = subscriptionId;
                    //newNotification.ExpirationDateTime = expirationDate;
                    //_unitOfWork.Dispose();
                    //_unitOfWork.NotificationSubscriptions.Add(newNotification);
                    //_unitOfWork.Complete();
                   var not =  _unitOfWork.Notifications.GetAll();

                   // response.ID = newNotification.Id;
                }
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
            // Acquire the access token

        }

       

        public async Task RenewSubscriptionAsync(string subscriptionId)
        {
            // Acquire access token using GraphAuthService
            var accessToken = await _graphAuthService.GetAccessTokenAsync();

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // Define the renewal payload
            var renewalPayload = new
            {
                expirationDateTime = DateTime.UtcNow.AddMinutes(4230).ToString("o")
            };

            var content = new StringContent(JsonConvert.SerializeObject(renewalPayload), Encoding.UTF8, "application/json");

            // Send the request to renew the subscription
            var response = await _httpClient.PatchAsync($"https://graph.microsoft.com/v1.0/subscriptions/{subscriptionId}", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Subscription renewed successfully.");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to renew subscription: {errorResponse}");
            }
        }


        //public Task Execute(IJobExecutionContext context)
        //{
        //    var test =  RenewSubscriptionAsync("1");

        //    return Task.CompletedTask;
        //}
    }
}
