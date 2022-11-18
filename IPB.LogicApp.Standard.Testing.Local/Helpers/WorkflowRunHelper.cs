using IPB.LogicApp.Standard.Testing.Model;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing.Local.Helpers
{
    public class WorkflowRunHelper
    {
        private WorkflowRunActionDetails _runActions;
        private RunDetails _runDetails;

        public WorkflowHelper WorkflowHelper { get; set; }
        

        public ManagementApiHelper ManagementApiHelper { get; set; }

        public string RunId { get; set; }

        public bool WasRunSuccessful()
        {
            var runDetails = GetRunDetails();
            if (runDetails.properties.status == "Succeeded")
                return true;
            else
                return false;
        }

        /// <summary>
        /// This will get the run actions for the workflow if we dont already have them and get the specific action for the name we want and
        /// return the action status.
        /// Note that if the action name has spaces in it the default behaviour is to replace spaces with underscore which is what azure does
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="refreshActions"></param>
        /// <param name="formatActionName"></param>
        /// <returns></returns>
        public ActionStatus GetActionStatus(string actionName, bool refreshActions = false, bool formatActionName = true)
        {
            //The run history usually formats the name of the action to not have spaces and it replaces then with underscores
            if (formatActionName)
                actionName = actionName.Replace(" ", "_");

            if(refreshActions || _runActions == null)
                _runActions = GetRunActions();

            var action = _runActions.properties.GetAction(actionName);
            if (action == null)
                return ActionStatus.ActionDoesntExistInRunHistory;

            return action.ActionStatus;
        }

        public JToken GetActionJson(string actionName, bool refreshActions = false, bool formatActionName = true)
        {
            //The run history usually formats the name of the action to not have spaces and it replaces then with underscores
            if (formatActionName)
                actionName = actionName.Replace(" ", "_");

            if (refreshActions || _runActions == null)
                _runActions = GetRunActions();

            var action = _runActions.properties.GetActionJson(actionName);
            if (action == null)
                throw new Exception("The action does not exist");

            return action;
        }

        public TriggerStatus GetTriggerStatus(bool refresh = false)
        {
            if (refresh || _runActions == null)
                _runActions = GetRunActions(refresh);

            return _runActions.properties.trigger.TriggerStatus;
        }



        /// <summary>
        /// Gets the run details from Azure if we dont already have them
        /// </summary>
        /// <returns></returns>
        public RunDetails GetRunDetails(bool refresh = false)
        {
            var url = $@"{ApiSettings.ManagementWorkflowBaseUrl}/{WorkflowHelper.WorkflowName}/runs/{RunId}?api-version={ApiSettings.ApiVersion}";

            if (refresh || _runDetails == null)
            {
                var client = ManagementApiHelper.GetHttpClient();

                HttpResponseMessage response = client.GetAsync(url).Result;
                var responseText = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();

                _runDetails = JsonConvert.DeserializeObject<RunDetails>(responseText);
            }
            return _runDetails;
        }

        

        public WorkflowRunActionDetails GetRunActions(bool refresh = false)
        {
            var url = $@"{ApiSettings.ManagementWorkflowBaseUrl}/{WorkflowHelper.WorkflowName}/runs/{RunId}?api-version={ApiSettings.ApiVersion}&$expand=properties/actions,workflow/properties";
            if (refresh || _runActions == null)
            {
                var client = ManagementApiHelper.GetHttpClient();

                HttpResponseMessage response = client.GetAsync(url).Result;
                var responseText = response.Content.ReadAsStringAsync().Result;
                response.EnsureSuccessStatusCode();

                _runActions = JsonConvert.DeserializeObject<WorkflowRunActionDetails>(responseText);
            }
            return _runActions;
        }
    }
}
