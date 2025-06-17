using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using NewGaras.Infrastructure.Models.EmailTool;
using NewGaras.Infrastructure.Models.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Domain.Services
{
    public class GraphAuthService : IGraphAuthService
    {
        private readonly IConfidentialClientApplication _app;
        private readonly string[] _scopes;
        private readonly AuthDataInit _authDataInit;

        public GraphAuthService(IOptions<AuthDataInit> authDataInit)
        {
            _authDataInit = authDataInit.Value;
            _app = ConfidentialClientApplicationBuilder.Create(authDataInit.Value.clientId)
                .WithClientSecret(authDataInit.Value.clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{authDataInit.Value.tenantId}"))
                .Build();

            _scopes = new[] { "https://graph.microsoft.com/.default" }; // .default scope for app permissions
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var result = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
            return result.AccessToken;
        }
    }
}
