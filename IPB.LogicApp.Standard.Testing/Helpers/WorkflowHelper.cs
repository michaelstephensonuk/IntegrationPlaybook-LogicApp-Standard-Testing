using IPB.LogicApp.Standard.Testing.Model;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing.Helpers
{
    public class WorkflowHelper
    {
        public string SubscriptionId { get; set; }
        public string ResourceGroupName { get; set; }
        public string LogicAppName { get; set; }
        public string WorkflowName { get; set; }

        public ManagementApiHelper ManagementApiHelper { get;set;}

        

        /// <summary>
        /// Gets the callback url for the logic app so we can trigger it.  The default behaviour is to get the manual trigger so we can run over
        /// http
        /// </summary>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        public string GetCallBackUrl(string triggerName = "manual")
        {
            var url = $@"/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroupName}/providers/Microsoft.Web/sites/{LogicAppName}/hostruntime/runtime/webhooks/workflow/api/management/workflows/{WorkflowName}/triggers/{triggerName}/listCallbackUrl?api-version={ApiSettings.ApiVersion}";

            var client = ManagementApiHelper.GetHttpClient();
           
            var content = new StringContent("");
            HttpResponseMessage response = client.PostAsync(url, content).Result;
            var responseText = response.Content.ReadAsStringAsync().Result;
            response.EnsureSuccessStatusCode();

            var jsonResponse = JObject.Parse(responseText);
            return jsonResponse["value"].ToString();
        }

        /// <summary>
        /// Triggers the logic app with an HTTP post request
        /// </summary>
        /// <param name="content"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        public WorkFlowResponse TriggerLogicAppWithPost(StringContent content, string triggerName = "manual")
        {
            var url = GetCallBackUrl(triggerName);

            using (HttpClient client = new HttpClient())
            {   
                HttpResponseMessage response = client.PostAsync(url, content).Result;
                var workflowResponse = new WorkFlowResponse(response);
                return workflowResponse;
            }
        }

        /// <summary>
        /// Triggers the logic app with an HTTP GET request
        /// </summary>
        /// <param name="content"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        public WorkFlowResponse TriggerLogicAppWithGet(StringContent content, string triggerName = "manual")
        {
            var url = GetCallBackUrl(triggerName);

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                var workflowResponse = new WorkFlowResponse(response);
                return workflowResponse;
            }
        }

        /// <summary>
        /// Once have ran the logic app we can get a workflow run helper which will let us access details about the run history
        /// </summary>
        /// <param name="runId"></param>
        /// <returns></returns>
        public WorkflowRunHelper GetWorkflowRunHelper(string runId)
        {
            var runHelper = new WorkflowRunHelper();
            runHelper.WorkflowHelper = this;
            runHelper.RunId = runId;
            runHelper.ManagementApiHelper = ManagementApiHelper;
            return runHelper;
        }

        public RunDetails GetMostRecentRunDetails(DateTime startDate)
        {
            var dateString = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var url = $@"subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroupName}/providers/Microsoft.Web/sites/{LogicAppName}/hostruntime/runtime/webhooks/workflow/api/management/workflows/{WorkflowName}/runs?api-version={ApiSettings.ApiVersion}&$top=1&$filter=startTime ge {dateString}";

            var client = ManagementApiHelper.GetHttpClient();

            HttpResponseMessage response = client.GetAsync(url).Result;
            var responseText = response.Content.ReadAsStringAsync().Result;
            response.EnsureSuccessStatusCode();

            var runList = JsonConvert.DeserializeObject<WorkflowRunList>(responseText);

            return runList.Value.FirstOrDefault();
        }

        public WorkflowRunList GetRunsSince(DateTime startDate)
        {
            var dateString = startDate.ToString("yyyy-MM-ddTHH:mm:ssZ");
            var url = $@"subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroupName}/providers/Microsoft.Web/sites/{LogicAppName}/hostruntime/runtime/webhooks/workflow/api/management/workflows/{WorkflowName}/runs?api-version={ApiSettings.ApiVersion}&$filter=startTime ge {dateString}";

            var client = ManagementApiHelper.GetHttpClient();

            HttpResponseMessage response = client.GetAsync(url).Result;
            var responseText = response.Content.ReadAsStringAsync().Result;
            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<WorkflowRunList>(responseText);
        }
    }
}
