using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{

    public interface INotification
    {
        Boolean setReceiver(List<string> receivers);
        Boolean setMessage(string message, string ruleId, string ruleDescription);
        Boolean setCredentials(Dictionary<string, string> creds);
        Task execute();
    }
    public class Notification
    {
        private const EmailImplementationTypes EMAIL_IMPLEMENTATION_TYPE = EmailImplementationTypes.LogicApp;
        private readonly IServicesConfig servicesConfig;
        private INotification implementation;

        public IList<ActionAsaModel> actionList { get; set; }
        public string ruleId { get; set; }
        public string ruleName { get; set; }
        public string ruleDescription { get; set; }

        IDictionary<EmailImplementationTypes, Func<INotification>> actionsEmail = new Dictionary<EmailImplementationTypes, Func<INotification>>(){
            {EmailImplementationTypes.LogicApp, () =>  new LogicApp()}
        };

        public Notification(IServicesConfig servicesConfig)
        {
            this.servicesConfig = servicesConfig;
        }

        public async Task execute()
        {
            foreach (ActionAsaModel action in this.actionList)
            {
                switch (action.ActionType)
                {
                    case "Email":
                        implementation = actionsEmail[EMAIL_IMPLEMENTATION_TYPE]();
                        var credentialDictionary = new Dictionary<string, string>()
                            {
                                {"endPointURL", this.servicesConfig.LogicAppEndPointUrl }
                            };
                        implementation.setCredentials(credentialDictionary);
                        break;
                }
                implementation.setMessage((string)action.Parameters["Template"], this.ruleId, this.ruleDescription);
                implementation.setReceiver(((Newtonsoft.Json.Linq.JArray)action.Parameters["Email"]).ToObject<List<string>>());
                await implementation.execute();
            }
        }
    }
}


enum EmailImplementationTypes
{
    LogicApp
}

