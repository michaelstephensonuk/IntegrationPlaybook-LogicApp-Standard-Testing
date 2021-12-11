using IPB.LogicApp.Standard.Testing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LogicApp.Testing.Example
{
    public class LogicAppTestManagerBuilder
    {
        public static LogicAppTestManager Build(string workflowName)
        {
            var config = GetConfiguration();
            var args = new LogicAppTestManagerArgs();

            //Can get these from Config in the real world
            args.ClientId = config["ClientId"];
            args.ClientSecret = config["ClientSecret"];
            args.TenantId = config["TenantId"];
            args.LogicAppName = config["LogicAppName"];
            args.ResourceGroupName = config["ResourceGroupName"];
            args.SubscriptionId = config["SubscriptionId"];
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
