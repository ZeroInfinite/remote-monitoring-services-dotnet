using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{

    public interface INotification
    {
        Boolean setReceiver(string receivers);
        Boolean setMessage(string message, string ruleId, string ruleDescription);
        Boolean setCredentials(Dictionary<string, string> creds);
        Task execute();
    }
    public class Notification
    {
        private const EmailImplementationTypes EMAIL_IMPLEMENTATION_TYPE = EmailImplementationTypes.LogicApp;

        public IList<ActionAsaModel> actionList;
        public INotification implementation;

        private string ruleId;
        private string ruleDescription;

        // Mapping to the Implementation type based on the setup.
        IDictionary<EmailImplementationTypes, Func<INotification>> actionsEmail = new Dictionary<EmailImplementationTypes, Func<INotification>>(){
        {EmailImplementationTypes.LogicApp, () =>  new LogicApp()}
        };

        public Notification() { }

        public async Task execute()
        {
            foreach (ActionAsaModel action in this.actionList)
            {
                if (action.ActionType == "Email")
                {
                    implementation = actionsEmail[EMAIL_IMPLEMENTATION_TYPE]();
                    var credentialDictionary = new Dictionary<string, string>();
                    credentialDictionary.Add("endPointURL", @"https://prod-00.southeastasia.logic.azure.com:443/workflows/1f2493004aea43e1ac661f071a15f330/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=DIfPL17M7qydXwHxD7g-_K-P3mE6dqYuv7aDfbQji94");
                    implementation.setCredentials(credentialDictionary);
                }
                implementation.setMessage((string)action.Parameters["Template"], this.ruleId, this.ruleDescription);
                implementation.setReceiver(((Newtonsoft.Json.Linq.JArray)action.Parameters["Email"]).ToObject<List<string>>()[0]);
                await implementation.execute();
            }
        }

        public void setActionList(IList<ActionAsaModel> actionList)
        {
            this.actionList = actionList;
        }

        public void setAlarmInformation(string ruleId, string ruleDescription)
        {
            this.ruleId = ruleId;
            this.ruleDescription = ruleDescription;
        }
    }
}


enum EmailImplementationTypes
{
    LogicApp
}

