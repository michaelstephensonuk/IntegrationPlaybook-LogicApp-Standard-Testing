using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPB.LogicApp.Standard.Testing.Model.WorkflowRunActionDetails
{
    public class WorkflowRunActionDetails
    {
        public Properties properties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Properties
    {
        public string status { get; set; }
        public TriggerDetails trigger { get; set; }
        public JObject actions { get; set; }

        public ActionDetails GetAction(string name)
        {
            var action = actions[name];
            if (action == null)
                return null;

            return JsonConvert.DeserializeObject<ActionDetails>(action.ToString());
        }

        public JToken GetActionJson(string name)
        {
            var action = actions[name];
            if (action == null)
                return null;

            return action;
        }
    }
    
    public class ActionDetails
    {
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }

        public ActionStatus ActionStatus
        {
            get
            {
                return (ActionStatus)Enum.Parse(typeof(ActionStatus), status);
            }
        }

    }

    public enum ActionStatus
    {
        Succeeded,
        Failed,
        Skipped,
        ActionDoesntExistInRunHistory
    }

    public class TriggerDetails
    {
        public string name { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public string status { get; set; }

        public TriggerStatus TriggerStatus
        {
            get
            {
                return (TriggerStatus)Enum.Parse(typeof(TriggerStatus), status);
            }
        }
    }

    public enum TriggerStatus
    {
        Succeeded,
        Failed,
        Skipped
    }
}
