using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace IPB.LogicApp.Standard.Testing.Local.Host
{

    public class WorkflowTestHostBuilder
    {
        /// <summary>
        /// If this is set to true then we will add the output from func.exe
        /// to the trace output so it will show in your test output.  This could be handy
        /// for troubleshooting on a build agent but it could be quite a bit of info
        /// so you might want to set it to false and just use the log written in the test folder
        /// so it doesnt give you big verbose test results
        /// </summary>
        public bool WriteFuncOutputToDebugTrace { get; set; }

        public string LogicAppFolder { get; set; }
        public string AppSettingsPath { get; set; }
        public string ConnectionsPath { get; set; }
        public string ParametersPath { get; set; }

        public string AppSettingsJson { get; set; }
        public string ConnectionsJson { get; set; }
        public string ParametersJson { get; set; }

        public string HostJson { get; set; }

        public string HostPath { get; set; }

        public List<WorkflowTestInput> WorkflowDefinitions { get; set; }

        public List<string> Workflows { get; set; }

        /// <summary>
        /// Setup the workflow test host
        /// </summary>
        /// <param name="logicAppFolder">The relative path to your logic apps based on where the tests are running from</param>
        public WorkflowTestHostBuilder(string logicAppFolder = @"..\..\..\..\logicapp\")
        {
            LogicAppFolder = logicAppFolder;
            AppSettingsPath = Path.Combine(LogicAppFolder, "local.settings.json");
            ConnectionsPath = Path.Combine(LogicAppFolder, "connections.json");
            HostPath = Path.Combine(LogicAppFolder, "host.json");
            ParametersPath = Path.Combine(LogicAppFolder, "parameters.json");
            Workflows = new List<string>();
            WorkflowDefinitions = new List<WorkflowTestInput>();
        }

        private void ValidatePath(string relativePath)
        {
            var fullPath = Path.GetFullPath(relativePath);
            if (File.Exists(fullPath))
            {
                Console.WriteLine($"The file {fullPath} does exist");
            }
            else
            {
                Console.WriteLine($"The file {fullPath} does not exist");
                throw new Exception($"The file {fullPath} does not exist");
            }
        }

        public void Load()
        {
            ValidatePath(ConnectionsPath);
            ValidatePath(AppSettingsPath);
            ValidatePath(HostPath);
            ValidatePath(ParametersPath);

            //Read the various files for the logic app
            Console.WriteLine($"Reading json files for Logic Apps (eg. connections.json, appsettings.json)");
            ConnectionsJson = File.ReadAllText(ConnectionsPath);
            AppSettingsJson = File.ReadAllText(AppSettingsPath);
            HostJson = File.ReadAllText(HostPath);
            ParametersJson = File.ReadAllText(ParametersPath);

            //Iterate over the loaded workflows and load them too
            foreach (var workflowName in Workflows)
            {
                var workflowPath = @$"{LogicAppFolder}\{workflowName}\workflow.json";
                Console.WriteLine($"Reading {workflowPath} json file");
                var workflowDefinitionJson = File.ReadAllText(workflowPath);

                WorkflowDefinitions.Add(new WorkflowTestInput(functionName: workflowName, flowDefinition: workflowDefinitionJson));
            }
        }

        public WorkflowTestHost Build()
        {
            //Load the workflow test host and return it
            var workflowTestHost = new WorkflowTestHost(
                WorkflowDefinitions.ToArray(),
                localSettings: AppSettingsJson,
                connectionDetails: ConnectionsJson,
                parameters: ParametersJson,
                host: HostJson,                
                writeFuncOutputToDebugTrace: WriteFuncOutputToDebugTrace
                );

            return workflowTestHost;
        }

        public WorkflowTestHost LoadAndBuild()
        {
            Load();
            return Build();
        }
    }
}


