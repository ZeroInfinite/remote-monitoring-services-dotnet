using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface INotification
    {
        Task execute();
        AlarmNotificationAsaModel alarm { get; set; }
    }

    public class Notification: INotification
    {
        private const EmailImplementationTypes EMAIL_IMPLEMENTATION_TYPE = EmailImplementationTypes.LogicApp;
        private readonly IImplementationWrapper implementationWrapper;
        private IImplementation implementation;
       
        public AlarmNotificationAsaModel alarm { get; set; }

        public Notification(IImplementationWrapper implementationWrapper)
        {
            this.implementationWrapper = implementationWrapper;
        }

        public async Task execute()
        {
            foreach (ActionAsaModel action in this.alarm.Actions)
            {
                switch (action.ActionType)
                {
                    case "Email":
                        implementation = this.implementationWrapper.GetImplementationType(EMAIL_IMPLEMENTATION_TYPE);
                        break;
                }
                implementation.setMessage((string)action.Parameters["Template"], this.alarm.Rule_id, this.alarm.Rule_description);
                if(action.Parameters["Email"] != null) implementation.setReceiver(((Newtonsoft.Json.Linq.JArray)action.Parameters["Email"]).ToObject<List<string>>());

                await implementation.execute();
            }
        }
    }
}

public enum EmailImplementationTypes
{
    LogicApp
}

