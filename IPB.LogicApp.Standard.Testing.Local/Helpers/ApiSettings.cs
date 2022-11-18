

using System;

namespace IPB.LogicApp.Standard.Testing.Local.Helpers
{
    public class ApiSettings
    {
        public static int FunctionsPort = 7071;

        public static string ManagementWorkflowBaseUrl = $"http://{Environment.MachineName}:{FunctionsPort}/runtime/webhooks/workflow/api/management/workflows";


        public static readonly string ApiVersion = "2019-10-01-edge-preview";


    }
}
