using System;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing.Local.Helpers
{
    /// <summary>
    /// A wrapper class to help call the management api
    /// </summary>
    public class ManagementApiHelper
    {
        private HttpClient _managementApiClient { get; set; }

        public ManagementApiHelper()
        {
            _managementApiClient = new HttpClient();
            _managementApiClient.BaseAddress = new Uri(ApiSettings.ManagementWorkflowBaseUrl);
        }


        public HttpClient GetHttpClient()
        {
            return _managementApiClient;
        }
    }
}
