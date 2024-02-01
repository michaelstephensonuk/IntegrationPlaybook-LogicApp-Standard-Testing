using Azure.Core;
using Azure.Identity;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Text;
using System.Threading;

namespace IPB.LogicApp.Standard.Testing.Helpers
{
    public class AuthenticationHelper
    {
        public bool UseDefaultCredential { get; set; }
        
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }

        /// <summary>
        /// Gets the url for Azure AD for the tenant you provide for authentication
        /// </summary>
        /// <returns></returns>
        private string GetAzureADUrl()
        {
            return $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token";
        }

        /// <summary>
        /// Gets the access token for accessing the management api.  We will try to cache the token here to reduce
        /// the number of times we need to call azure ad
        /// </summary>
        /// <returns></returns>
        public string GetBearerToken()
        {
            const string CacheKey = "LogicApp-TestManager-AccessToken";

            var cache = MemoryCache.Default;
            var accessToken = cache[CacheKey] as string;
            if (accessToken != null)
                return accessToken;


            //Caching the token so we dont have to keep hitting Azure AD but also so we can refresh it if the test is
            //long running
            accessToken = GetBearerTokenFromAzureAD();
            cache.Add(CacheKey, accessToken, DateTimeOffset.Now.AddMinutes(4));
            return accessToken;
        }

        /// <summary>
        /// Gets an access token from Azure AD for accessing the management api
        /// </summary>
        /// <returns></returns>
        public string GetBearerTokenFromAzureAD()
        {
            const string scope = "https://management.azure.com/.default";

            if (UseDefaultCredential)
            {
                var credential = new DefaultAzureCredential();
                var tokenResult = credential.GetTokenAsync(new TokenRequestContext(new[] { scope }), CancellationToken.None).Result;
                return tokenResult.Token;
            }

            var args = new Dictionary<string, string>();
            var request = new StringBuilder();
            request.Append("grant_type=client_credentials");
            request.Append($"&scope=${scope}");
            request.Append($"&client_id={ClientId}");
            request.Append($"&client_secret={ClientSecret}");
            
            var url = GetAzureADUrl();

            using (HttpClient client = new HttpClient())
            {
                var requestBody = request.ToString();
                var content = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.PostAsync(url, content).Result;
                var responseText = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();

                var tokenJson = JObject.Parse(responseText);
                return tokenJson["access_token"].ToString();
            }

        }
        
    }
}
