using IPB.LogicApp.Standard.Testing;
using IPB.LogicApp.Standard.Testing.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LogicApp.Testing.Example
{
    public class CommonTestContext
    {
        public CommonTestContext()
        {
        }


        public LogicAppTestManager LogicAppTestManager { get; set; }
        
        public string Request { get; set; }

        public WorkFlowResponse Response { get; set; }

        public string WorkflowName { get; set; }


    }
}
