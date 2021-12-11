using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace IPB.LogicApp.Standard.Testing.Model
{
    public class WorkFlowResponse
    {
        public HttpResponseMessage HttpResponse { get; set; }

        public WorkFlowResponse(HttpResponseMessage httpResponse)
        {
            HttpResponse = httpResponse;
        }

        public string WorkFlowRunId
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-workflow-run-id").Value.FirstOrDefault();               
            }
        }

        public string WorkflowName
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-workflow-name").Value.FirstOrDefault();
            }
        }

        public string ClientTrackingId
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-client-tracking-id").Value.FirstOrDefault();
            }
        }

        public string RequestId
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-request-id").Value.FirstOrDefault();
            }
        }

        public string TrackingId
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-tracking-id").Value.FirstOrDefault();
            }
        }

        public string TriggerHistoryName
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-trigger-history-name").Value.FirstOrDefault();
            }
        }

        public string WorkflowVersion
        {
            get
            {
                return HttpResponse.Headers.FirstOrDefault(i => i.Key == "x-ms-workflow-version").Value.FirstOrDefault();
            }
        }
    }
}
