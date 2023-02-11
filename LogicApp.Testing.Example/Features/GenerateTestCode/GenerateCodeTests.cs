using IPB.LogicApp.Standard.Testing;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LogicApp.Testing.Example.Features.GenerateTestCode
{
   
    [TestClass]
    public class GenerateCodeTests
    {
        /// <summary>
        /// This test will run the basic workflow which just returns a response
        /// </summary>
        [TestMethod]
        public void GenerateCodeTest()
        {
            var workflowName = "hello-world-stateful";
            var logicAppTestManager = LogicAppTestManagerBuilder.Build(workflowName);

            var request = File.ReadAllText(@"..\..\..\Features\GenerateTestCode\SampleMessage.json");

            var content = new StringContent(request, Encoding.UTF8, "application/json");
            var response = logicAppTestManager.TriggerLogicAppWithPost(content);
            
            //Generate Test Steps for the test we just ran
            var runId = response.WorkFlowRunId;
            var sampleCode = logicAppTestManager.GenerateTestSample(runId);
        }

        [TestMethod]
        public void Test()
        {
            var actionStatus = ActionStatus.Succeeded;

            //Arrange
            var workflowName = "hello-world-stateful";
            var logicAppTestManager = LogicAppTestManagerBuilder.Build(workflowName);

            var request = File.ReadAllText(@"..\..\..\Features\GenerateTestCode\SampleMessage.json");

            //TODO: Check your content type
            var content = new StringContent(request, Encoding.UTF8, "application/json");

            //Act
            var response = logicAppTestManager.TriggerLogicAppWithPost(content);
            logicAppTestManager.LoadWorkflowRunHistory();

            //Assert
            Assert.IsNotNull(response.WorkFlowRunId);

            var triggerStatus = logicAppTestManager.GetTriggerStatus();
            Assert.AreEqual(triggerStatus, TriggerStatus.Succeeded);

            var workflowRunStatus = logicAppTestManager.GetWorkflowRunStatus();
            Assert.AreEqual(workflowRunStatus, WorkflowRunStatus.Succeeded);

            actionStatus = logicAppTestManager.GetActionStatus("Compose_-_Log_Message_Received");
            Assert.AreEqual(actionStatus, ActionStatus.Succeeded);

            actionStatus = logicAppTestManager.GetActionStatus("Response");
            Assert.AreEqual(actionStatus, ActionStatus.Succeeded);

        }

    }
}
