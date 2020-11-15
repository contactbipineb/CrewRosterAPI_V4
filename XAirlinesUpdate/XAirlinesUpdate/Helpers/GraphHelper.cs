using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace XAirlinesUpdate.Helpers
{
    public class GraphHelper
    {
        private readonly string _token;
        protected readonly IConfiguration configuration;

        public GraphHelper(string token, IConfiguration configuration)
        {
            this.configuration = configuration;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            _token = token;
        }

        public GraphHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> GetUserEmployeeIdAsync(string AadId)
        {
            var graphClient = GetApplicationClient();
            var user = await graphClient.Users[AadId].Request().Select(x => x.EmployeeId).GetAsync() as User;
            return user.EmployeeId;
        }


        private GraphServiceClient GetApplicationClient()
        {
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(this.configuration["MicrosoftAppId"])
                .WithTenantId(this.configuration["TenantId"])
                .WithClientSecret(this.configuration["MicrosoftAppPassword"])
                .Build();

            var scopes = new string[] { "https://graph.microsoft.com/.default" };

            GraphServiceClient graphServiceClient =
                new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) => {

                    // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
                    var authResult = await confidentialClientApplication
                        .AcquireTokenForClient(scopes)
                        .ExecuteAsync();

                    // Add the access token in the Authorization header of the API request.
                    requestMessage.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
                })
            );

            return graphServiceClient;//new GraphServiceClient(authProvider);
        }

        // Get an Authenticated Microsoft Graph client using the token issued to the user.
        private GraphServiceClient GetAuthenticatedClient()
        {
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(
                    requestMessage =>
                    {
                        // Append the access token to the request.
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", _token);

                        // Get event times in the current time zone.
                        requestMessage.Headers.Add("Prefer", "outlook.timezone=\"" + TimeZoneInfo.Local.Id + "\"");

                        return Task.CompletedTask;
                    }));
            return graphClient;
        }
    }
}
