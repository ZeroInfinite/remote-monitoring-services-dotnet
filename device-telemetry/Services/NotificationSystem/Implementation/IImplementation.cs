using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation
{
    public interface IImplementation
    {
        void setReceiver(List<string> receivers);
        void setMessage(string message, string ruleId, string ruleDescription);
        Task execute();
    }
}
