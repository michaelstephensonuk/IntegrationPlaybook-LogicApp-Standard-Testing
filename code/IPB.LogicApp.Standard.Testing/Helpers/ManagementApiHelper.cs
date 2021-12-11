using System;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing.Helpers
{
    /// <summary>
    /// A wrapper class to help call the management api
    /// </summary>
    public class ManagementApiHelper
    {
        private AuthenticationHelper _authHelper;
        private HttpClient _managementApiClient { get; set; }

        public ManagementApiHelper(AuthenticationHelper authHelper)
        {
            _authHelper = authHelper;
            _managementApiClient = new HttpClient();
            _managementApiClient.BaseAddress = new Uri(ApiSettings.BaseUrl);
        }


        public HttpClient GetHttpClient()
        {
            var accessToken = _authHelper.GetBearerTokenFromAzureAD();
            _managementApiClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            return _managementApiClient;
        }
    }
}
