using System;
using System.Collections.Generic;
using System.Text;

namespace IPB.LogicApp.Standard.Testing
{
    public class LogicAppTestManagerArgs
    {
        public bool UseDefaultCredential { get; set; }
        
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TenantId { get; set; }
        
        public string SubscriptionId { get; set; }
        public string ResourceGroupName { get; set; }
        public string LogicAppName { get; set; }
        public string WorkflowName { get; set; }
    }
}
