using System;
using System.Collections.Generic;
using System.Text;

namespace IPB.LogicApp.Standard.Testing.Model.WorkflowRunOverview
{    

    public class Trigger
    {
        public string name { get; set; }
        public string status { get; set; }
    }

    public class Properties
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }
        public Trigger trigger { get; set; }

        public WorkflowRunStatus WorkflowRunStatus
        {
            get
            {
                return (WorkflowRunStatus)Enum.Parse(typeof(WorkflowRunStatus), status);
            }
        }
    }

    public class RunDetails
    {
        public Properties properties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public enum WorkflowRunStatus
    {
        Succeeded,
        Failed,
        Skipped
    }
}
