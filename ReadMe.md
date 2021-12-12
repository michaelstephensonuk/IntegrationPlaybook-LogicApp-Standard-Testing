# IPB.LogicApp.Standard.Testing

# Aim

The aim of this framework is to support automated testing of workflows implemeneted with Logic App Standard.

Microsoft have introduced a framework which is available on the below link which allows you to do unit testing of a workflow in your own
self hosted instance of the workflow runtime.
https://github.com/Azure/logicapps/tree/994b3d91d57f7ce88b7d331734dcc03fe54c5816/LogicAppsSampleTestFramework

This framework is great for unit testing scenarios which let you check your code in the workflow works well but it does not cover the full automated integration testing
scenarios which we need when you deploy your logic apps and need to check they work when deployed to Azure and connect to all of your various systems.

This framework is intended to help you do that next level of automated integration testing for Logic Apps Standard.

You will be able to read more about the approach in the integration playbook at the below address.
https://www.integration-playbook.io/docs/integration-testing

You can also get the package for this project from nuget here
https://www.nuget.org/packages/IPB.LogicApp.Standard.Testing/

# Approach

In this framework ive tried to keep it simple and easy to use and have tried to minimize the number of dependancies on external packages.
I dont think there is yet a management package for logic app standard like their used to be for consumption but I am just making calls to the 
azure management api to query the data we need to be able to find, trigger and inspect the results of your workflow runs.

# Setting up the Logic App Test Manager

To use the Logic App Test Manager we need to supply a bunch of parameters and then call the Setup() method which will connect to Azure and prepare for you to execute your tests.
The parameters needed are:

- ClientID = a client id for an Azure AD app registration which you have set up
- ClientSecret = The secret for your azure ad app registration
- TenantId = The tenant id for your azure ad
- LogicAppName = The logic app name in Azure 
- ResourceGroupName = The resource group that the logic app lives in
- SubscriptionId = The subscription id the logic app lives in
- WorkflowName = The name of the workflow within the logic app you want to test

Note:
Your azure ad app registration will need permissions for the logic app.  The easiest is to give it contributor access if your doing a sample but for the real world you should look to give it least priveledges.

TODO - Add the RBAC least priviledges here

# Using the Framework in a normal MsTest

In the below file there is an example with a simple walk through of using a test which can execute your workflow and test the results.

.\LogicApp.Testing.Example\NonSpecflowExampleTest.cs

# Using the framework in Specflow
In the below file there is an example of using the framework in a specflow test.

.\LogicApp.Testing.Example\Features\HelloWorld\HelloWorld.feature
.\LogicApp.Testing.Example\Features\HelloWorld\Steps.cs


# Key Things

## Initializing the Framework

I use a class in the following file .\LogicApp.Testing.Example\LogicAppTestManagerBuilder.cs which allows me to encapsulate the details to connect to my logic app
and set the authentication info.  I can reuse this and just pass in the name of a workflow each time I want to test a workflow.  

I then just use the code below to get the LogicAppTestManager instance.

```
var logicAppTestManager = LogicAppTestManagerBuilder.Build(workflowName);
```

Depending on how you want to use the test framework your setup code might be slightly different, for example if you want to test workflows in different logic apps.
You need 1 instance of the LogicAppTestManager for each workflow you want to test.

## Triggering your workflow

Below is an example snippet of triggering your workflow via the HTTP post request trigger

```
var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
var workflowResponse = logicAppTestManager.TriggerLogicAppWithPost(content);
```

## Testing the Trigger Result

Below is the snippet which lets you test if the trigger was successful

```
var triggerStatus = TestContext.LogicAppTestManager.GetTriggerStatus();
Assert.AreEqual(triggerStatus, TriggerStatus.Succeeded);
```

## Testing an action

Below is an example of testing the result of an action within your workflow.

```
var actionStatus = TestContext.LogicAppTestManager.GetActionStatus("Compose - Log Message Received");
Assert.AreEqual(actionStatus, ActionStatus.Succeeded);
```

# Notes

## Stateless workflows
By default the stateless workflows do not have the run history enabled.  Your tests will not be able to retrieve the run history to test what happened

You can enable the run history for a workflow within the logic app by setting the below app setting(note replace the workflow name with your own)

```
Workflows.{workflow-name}.OperationOptions = WithStatelessRunHistory
```