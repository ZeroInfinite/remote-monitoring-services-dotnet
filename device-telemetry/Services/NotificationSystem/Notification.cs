// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface INotification
    {
        Task execute();
        AlarmNotificationAsaModel alarm { get; set; }
    }

    public class Notification : INotification
    {
        private const EmailImplementationTypes EMAIL_IMPLEMENTATION_TYPE = EmailImplementationTypes.LogicApp;
        private readonly IImplementationWrapper implementationWrapper;
        private ILogger logger;
        private INotificationImplementation implementation;

        public AlarmNotificationAsaModel alarm { get; set; }

        public Notification(IImplementationWrapper implementationWrapper,
                            ILogger logger)
        {
            this.implementationWrapper = implementationWrapper;
            this.logger = logger;
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
                if (action.Parameters["Email"] != null) implementation.setReceiver(((Newtonsoft.Json.Linq.JArray)action.Parameters["Email"]).ToObject<List<string>>());
                if (await implementation.execute() == 0)
                {
                    this.logger.Info("Error executing the action", () => { });
                }
            }
        }
    }
}

public enum EmailImplementationTypes
{
    LogicApp
}

