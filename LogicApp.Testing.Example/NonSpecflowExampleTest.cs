using IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace LogicApp.Testing.Example
{
    [TestClass]
    public class NonSpecflowExampleTest
    {
        [TestMethod]
        public void GreenPath()
        {
            var startDateTime = DateTime.UtcNow;
            Thread.Sleep(new TimeSpan(0, 0, 10)); //Sleep here to handle any clock sync issues

            Console.WriteLine($"Date for start of test: {startDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}");

            var workflowName = "hello-world-stateful";

            var logicAppTestManager = LogicAppTestManagerBuilder.Build(workflowName);

            //I have a message to send to the workflow
            var message = new Dictionary<string, object>();
            message.Add("first_name", "mike");
            message.Add("last_name", "stephenson");
            var requestJson = JsonConvert.SerializeObject(message);

            //Send a message to the workflow
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var workflowResponse = logicAppTestManager.TriggerLogicAppWithPost(content);

            //If we get a run id then we know the logic app got the message
            Assert.IsNotNull(workflowResponse.WorkFlowRunId);

            //If the logic app started running we can load the run history at this point to start checking it later
            logicAppTestManager.LoadWorkflowRunHistory();

            //We can check the trigger status was successful
            var triggerStatus = logicAppTestManager.GetTriggerStatus();
            Assert.AreEqual(triggerStatus, TriggerStatus.Succeeded);

            //Check that an action was successful
            var actionStatus = logicAppTestManager.GetActionStatus("Compose - Log Message Received");
            Assert.AreEqual(actionStatus, ActionStatus.Succeeded);

            //Check that another action was successful
            actionStatus = logicAppTestManager.GetActionStatus("Response");
            Assert.AreEqual(actionStatus, ActionStatus.Succeeded);

            //Check the workflow run was successful
            var workflowRunStatus = logicAppTestManager.GetWorkflowRunStatus();
            Assert.AreEqual(WorkflowRunStatus.Succeeded, workflowRunStatus);


            //Check some of the additional helper methods
            var actionStatusJson = logicAppTestManager.GetActionJson("Response");
            var inputMessage = logicAppTestManager.GetActionInputMessage("Response");
            var outputMessage = logicAppTestManager.GetActionOutputMessage("Response");

            var runsSince = logicAppTestManager.GetRunsSince(startDateTime);
            Assert.IsTrue(runsSince.Value.Count > 0);

            var runSince = logicAppTestManager.GetMostRecentRunSince(startDateTime);
            Assert.IsTrue(runsSince.Value.Count == 1);

            var runDetails = logicAppTestManager.GetMostRecentRun();
            Assert.IsNotNull(runDetails);

            var runidSince = logicAppTestManager.GetMostRecentRunIdSince(startDateTime);
            Assert.IsNotNull(runidSince);
        }
    }
}
