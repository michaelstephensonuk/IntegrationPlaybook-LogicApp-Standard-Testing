using IPB.LogicApp.Standard.Testing.Local;
using IPB.LogicApp.Standard.Testing.Local.Host;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails;
using IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace LogicApp.Testing.Example.Features.LocalWorkflow
{
    [TestClass]
    public class Tests
    {
        /// <summary>
        /// This test will run the basic workflow which just returns a response
        /// </summary>
        [TestMethod]
        public void EchoWorkflowTest()
        {
            var pathToFiles = @"..\..\..\Features\LocalWorkflow";
            
            //Point to the folder containing the workflows to test 
            //and bundle up the files so they can be ran by the test
            var workflowToTestName = "Echo";
            var workflowTestHostBuilder = new WorkflowTestHostBuilder(pathToFiles);
            workflowTestHostBuilder.Workflows.Add(workflowToTestName);

            //Create the test manager to act as the client for testing the logic app
            var logicAppTestManager = new LogicAppTestManager(new LogicAppTestManagerArgs
            {
                WorkflowName = workflowToTestName
            });
            logicAppTestManager.Setup();

            //Spin up the workflow host wrapper to run the workflows locally
            using (var workflowTestHost = workflowTestHostBuilder.LoadAndBuild())
            {
                //Trigger the workflow
                var content = new StringContent("{}", Encoding.UTF8, "application/json");
                var response = logicAppTestManager.TriggerLogicAppWithPost(content);

                //Check you have a run id
                Assert.IsNotNull(response.WorkFlowRunId);

                //If the workflow started running we can load the run history at this point to start checking it later
                logicAppTestManager.LoadWorkflowRunHistory();

                //We can check the trigger status was successful
                var triggerStatus = logicAppTestManager.GetTriggerStatus();
                Assert.AreEqual(triggerStatus, TriggerStatus.Succeeded);

                //Check the response action worked
                var actionStatus = logicAppTestManager.GetActionStatus("Response");
                Assert.AreEqual(actionStatus, ActionStatus.Succeeded);

                //Check the run status completed successfully
                var workflowRunStatus = logicAppTestManager.GetWorkflowRunStatus();
                Assert.AreEqual(WorkflowRunStatus.Succeeded, workflowRunStatus);
            }
        }

        /// <summary>
        /// This test will run of a workflow where we will use the mock to return a response
        /// rather than calling out to postman
        /// </summary>
        [TestMethod]
        public void EchoPostmanMockWorkflowTest()
        {
            var pathToFiles = @"..\..\..\Features\LocalWorkflow";

            //Create the mock response we want to return to the workflow
            dynamic mockedResponseObject = new ExpandoObject();
            mockedResponseObject.url = "mockedurl";
            var mockedResponseMessage = JsonConvert.SerializeObject(mockedResponseObject);


            //Point to the folder containing the workflows to test 
            //and bundle up the files so they can be ran by the test
            var workflowToTestName = "Echo-Postman";
            var workflowTestHostBuilder = new WorkflowTestHostBuilder(pathToFiles);
            workflowTestHostBuilder.WriteFuncOutputToDebugTrace = false;
            workflowTestHostBuilder.Workflows.Add(workflowToTestName);
            workflowTestHostBuilder.Load();

            using (var mockHost = new MockHttpHost())
            {
                //We will update some of the app settings to inject the mock host url
                dynamic appSettings = JsonConvert.DeserializeObject(workflowTestHostBuilder.AppSettingsJson);
                appSettings.Values.postman_echo_url = mockHost.HostUri + "/get";
                workflowTestHostBuilder.AppSettingsJson = JsonConvert.SerializeObject(appSettings);

                //Spin up the workflow host wrapper to run the workflows locally
                using (var workflowTestHost = workflowTestHostBuilder.Build())
                {
                
                    //Setup the mock server to return responses
                    mockHost.Server.Given(
                        Request.Create().WithPath("/get").UsingGet()
                    )
                    .RespondWith(
                        Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/plain")
                        .WithBody(mockedResponseMessage)
                    );

                    //Create the test manager to act as the client for testing the logic app
                    var logicAppTestManager = new LogicAppTestManager(new LogicAppTestManagerArgs
                    {
                        WorkflowName = workflowToTestName
                    });
                    logicAppTestManager.Setup();

                    //Trigger the workflow
                    var content = new StringContent("{}", Encoding.UTF8, "application/json");
                    var response = logicAppTestManager.TriggerLogicAppWithPost(content);

                    //Check you have a run id
                    Assert.IsNotNull(response.WorkFlowRunId);

                    //If the workflow started running we can load the run history at this point to start checking it later
                    logicAppTestManager.LoadWorkflowRunHistory();

                    //We can check the trigger status was successful
                    var triggerStatus = logicAppTestManager.GetTriggerStatus();
                    Assert.AreEqual(triggerStatus, TriggerStatus.Succeeded);

                    //Check the response action worked
                    var actionStatus = logicAppTestManager.GetActionStatus("Response");
                    Assert.AreEqual(actionStatus, ActionStatus.Succeeded);

                    //Check the run status completed successfully
                    var workflowRunStatus = logicAppTestManager.GetWorkflowRunStatus();
                    Assert.AreEqual(WorkflowRunStatus.Succeeded, workflowRunStatus);
                }
            }
        }
    }
}
