using IPB.LogicApp.Standard.Testing.Helpers;
using IPB.LogicApp.Standard.Testing.Model;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using System;
using System.Net.Http;

namespace IPB.LogicApp.Standard.Testing
{
    public class LogicAppTestManager
    {
        private AuthenticationHelper _authHelper;
        private ManagementApiHelper _managementApiHelper;
        private WorkflowHelper _workflowHelper;
        private LogicAppTestManagerArgs _args;
        private WorkFlowResponse _workflowResponse;
        private WorkflowRunHelper _workflowRunHelper;

        public LogicAppTestManager(LogicAppTestManagerArgs args)
        {
            _args = args;
        }

        public void Setup()
        {
            //Setup authentication helper
            _authHelper = new AuthenticationHelper();
            _authHelper.ClientId = _args.ClientId;
            _authHelper.ClientSecret = _args.ClientSecret;
            _authHelper.TenantId = _args.TenantId;

            //Test we can authenticate and get a bearer token
            var bearerToken = _authHelper.GetBearerToken();

            //Setup Management API Helper
            _managementApiHelper = new ManagementApiHelper(_authHelper);

            //Setup Workflow Helper
            _workflowHelper = new WorkflowHelper();
            _workflowHelper.LogicAppName = _args.LogicAppName;
            _workflowHelper.ResourceGroupName = _args.ResourceGroupName;
            _workflowHelper.SubscriptionId = _args.SubscriptionId;
            _workflowHelper.WorkflowName = _args.WorkflowName;
            _workflowHelper.ManagementApiHelper = _managementApiHelper;
        }

        /// <summary>
        /// Trigger the logic app with an HTTP post
        /// </summary>
        /// <param name="content"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        public WorkFlowResponse TriggerLogicAppWithPost(StringContent content, string triggerName = "manual")
        {
            _workflowResponse = _workflowHelper.TriggerLogicAppWithPost(content, triggerName);
            return _workflowResponse;
        }

        /// <summary>
        /// Trigger the logic app with an HTTP get
        /// </summary>
        /// <param name="content"></param>
        /// <param name="triggerName"></param>
        /// <returns></returns>
        public WorkFlowResponse TriggerLogicAppWithGet(StringContent content, string triggerName = "manual")
        {
            _workflowResponse = _workflowHelper.TriggerLogicAppWithGet(content, triggerName);
            return _workflowResponse;
        }

        
        /// <summary>
        /// Loads the run history of the executed logic app.  This needs to be called before you start testing the actions
        /// that got executed
        /// </summary>
        public void LoadWorkflowRunHistory()
        {
            if (_workflowResponse == null)
                throw new Exception("You havent triggered the logic app");

            _workflowRunHelper = _workflowHelper.GetWorkflowRunHelper(_workflowResponse.WorkFlowRunId);
            _workflowRunHelper.GetRunActions();
            _workflowRunHelper.GetRunDetails();
        }

        public void LoadWorkflowRunHistory(string runId)
        {            
            _workflowRunHelper = _workflowHelper.GetWorkflowRunHelper(runId);
            _workflowRunHelper.GetRunActions();
            _workflowRunHelper.GetRunDetails();
        }


        /// <summary>
        /// Get the overall status of the workfow
        /// </summary>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public WorkflowRunStatus GetWorkflowRunStatus(bool refresh = false)
        {
            var runDetails = _workflowRunHelper.GetRunDetails(refresh);
            return runDetails.properties.WorkflowRunStatus;
        }

        /// <summary>
        /// Get the action from the run history so you can check if it was successful
        /// </summary>
        /// <param name="actionName"></param>
        /// <param name="refreshActions"></param>
        /// <param name="formatActionName"></param>
        /// <returns></returns>
        public ActionStatus GetActionStatus(string actionName, bool refreshActions = false, bool formatActionName = true)
        {
            return _workflowRunHelper.GetActionStatus(actionName, refreshActions, formatActionName);
        }

        /// <summary>
        /// Get the trigger result so you can check its status
        /// </summary>
        /// <param name="refresh"></param>
        /// <returns></returns>
        public TriggerStatus GetTriggerStatus(bool refresh = false)
        {
            return _workflowRunHelper.GetTriggerStatus(refresh);
        }
        }
}
