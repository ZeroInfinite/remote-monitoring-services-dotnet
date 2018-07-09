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
        private readonly IServicesConfig servicesConfig;

        public NotificationEventProcessorFactory(
            ILogger logger,
            IServicesConfig servicesConfig)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
        }
        public IEventProcessor CreateEventProcessor(PartitionContext context)
        {
            return new NotificationEventProcessor(this.logger, this.servicesConfig);
        }
    }
}
