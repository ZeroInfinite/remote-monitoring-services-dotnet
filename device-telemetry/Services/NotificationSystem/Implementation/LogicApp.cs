using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation
{
    public class LogicApp : IImplementation
    {
        private readonly IHttpClient httpClient;
        private readonly IHttpRequest httpRequest;
        private readonly ILogger logger;

        private string endointURL;
        private string content;
        private List<string> email;
        private string ruleId;
        private string ruleDescription;
        private string solutionName;

        public LogicApp(string endPointUrl, string solutionName, IHttpRequest httpRequest, IHttpClient httpClient, ILogger logger)
        {
            this.endointURL = endPointUrl;
            this.solutionName = solutionName;
            this.httpClient = httpClient;
            this.httpRequest = httpRequest;
            this.logger = logger;

            // Default for other parameters:
            this.content = "";
            this.ruleId = "";
            this.ruleDescription = "";
        }

        public void setMessage(string message, string ruleId, string ruleDescription)
        {
                this.content = message;
                this.ruleId = ruleId;
                this.ruleDescription = ruleDescription;
        }

        public void setReceiver(List<string> receiver)
        {
             this.email = receiver;
        }

        private string generatePayLoad()
        {
            var emailContent = "Alarm fired for rule ID: " + this.ruleId + "  Rule Description: " +
                this.ruleDescription + " Custom Message: " + this.content + "Alarm Detail Page: " + this.GenerateRuleDetailUrl();
            return "{\"emailAddress\" : " + JArray.FromObject(this.email) + ",\"template\": \"" + emailContent + "\"}";
        }

        public async Task execute()
        {
            this.httpRequest.SetUriFromString(this.endointURL);
            string content = this.generatePayLoad();
            this.httpRequest.SetContent(content, Encoding.UTF8, "application/json");
            this.httpRequest.AddHeader("content-type", "application/json");
            // Client library handles Exception.
            var httpRespose = await this.httpClient.PostAsync(this.httpRequest);
            if(httpRespose.StatusCode == 0)
            {
                this.logger.Info("Error sending request to the LogicApp endpoiint URL", () => new { httpRespose.Content });
            }
        }

        private string GenerateRuleDetailUrl()
        {
            /*
             "storageName": {
            "type": "string",
            "defaultValue": "[concat('storage', take(uniqueString(subscription().subscriptionId, resourceGroup().id, parameters('solutionName')), 5))]",
            "metadata": {
                "description": "The name of the storageAccount"
            }
            // The deployment sets the storage account name as the solution name. 
            // Work around, can set an environment variable. 
             */
            return "https://" + this.solutionName + ".azurewebsites.net/maintenance/rule/" + this.ruleId;
        }
    }
}
