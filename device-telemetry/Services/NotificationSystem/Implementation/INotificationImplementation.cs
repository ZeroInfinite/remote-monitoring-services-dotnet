// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation
{
    public interface INotificationImplementation
    {
        void setReceiver(List<string> receivers);
        void setMessage(string message, string ruleId, string ruleDescription);
        Task<HttpStatusCode> execute();
    }
}
