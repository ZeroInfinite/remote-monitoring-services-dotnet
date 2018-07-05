using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public class NotificationEventProcessorFactory : IEventProcessorFactory
    {
        private readonly ILogger logger;

        public NotificationEventProcessorFactory(
            ILogger logger)
        {
            this.logger = logger;
        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new NotificationEventProcessor(this.logger);
        }
    }
}
