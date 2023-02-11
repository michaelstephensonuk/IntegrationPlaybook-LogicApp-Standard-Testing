using Microsoft.Extensions.Configuration;
using System.IO;


namespace IPB.LogicApp.Standard.Testing
{
    /// <summary>
    /// This is a helper class to create a logic app test manager using the configuration in your app settings file for connecting to Azure with a service principal
    /// You can also just build the LogicAppTestManager yourself with the LogicAppTestManagerArgs class but this will likely save you a few lines of code
    /// </summary>
    public class LogicAppTestManagerBuilder
    {
        /// <summary>
        /// Build a logic app test manager using all of the settings from the app settings file
        /// </summary>
        /// <param name="workflowName"></param>
        /// <returns></returns>
        public static LogicAppTestManager Build(string workflowName)
        {
            var config = GetConfiguration();
            var args = new LogicAppTestManagerArgs();

            //Can get these from Config in the real world
            args.ClientId = config["logicAppTestManager:ClientId"];
            if(string.IsNullOrEmpty(args.ClientId))
                args.ClientId = config["AZURE_CLIENT_ID"];

            args.ClientSecret = config["logicAppTestManager:ClientSecret"];
            if (string.IsNullOrEmpty(args.ClientSecret))
                args.ClientSecret = config["AZURE_CLIENT_SECRET"];

            args.TenantId = config["logicAppTestManager:TenantId"];
            if (string.IsNullOrEmpty(args.TenantId))
                args.TenantId = config["AZURE_TENANT_ID"];


            args.LogicAppName = config["logicAppTestManager:LogicAppName"];
            args.ResourceGroupName = config["logicAppTestManager:ResourceGroupName"];
            args.SubscriptionId = config["logicAppTestManager:SubscriptionId"];
            args.WorkflowName = workflowName;

            var logicAppTestManager = new LogicAppTestManager(args);
            logicAppTestManager.Setup();
            return logicAppTestManager;
        }

        /// <summary>
        /// Use most of the settings from your app settings file but override some of them if your testing in different logic apps and resource groups
        /// </summary>
        /// <param name="workflowName"></param>
        /// <param name="logicAppName"></param>
        /// <param name="resourceGroupName"></param>
        /// <returns></returns>
        public static LogicAppTestManager Build(string workflowName, string logicAppName, string resourceGroupName)
        {
            var config = GetConfiguration();
            var args = new LogicAppTestManagerArgs();

            //Can get these from Config in the real world
            args.ClientId = config["logicAppTestManager:ClientId"];
            args.ClientSecret = config["logicAppTestManager:ClientSecret"];
            args.TenantId = config["logicAppTestManager:TenantId"];
            args.SubscriptionId = config["logicAppTestManager:SubscriptionId"];
            args.LogicAppName = logicAppName;
            args.ResourceGroupName = resourceGroupName;            
            args.WorkflowName = workflowName;

            var logicAppTestManager = new LogicAppTestManager(args);
            logicAppTestManager.Setup();
            return logicAppTestManager;
        }

        public static IConfiguration GetConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .Build();

            return config;
        }
    }
}
