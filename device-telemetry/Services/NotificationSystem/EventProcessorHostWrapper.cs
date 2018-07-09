using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs.Processor;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface IEventProcessorHostWrapper
    {
        EventProcessorHost CreateEventProcessorHost(
            string eventHubPath,
            string consumerGroupName,
            string eventHubConnectionString,
            string storageConnectionString,
            string leaseContainerName);

        Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory);

        Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory, EventProcessorOptions options);
    }

    public class EventProcessorHostWrapper : IEventProcessorHostWrapper
    {
        public EventProcessorHost CreateEventProcessorHost(
            string eventHubPath,
            string consumerGroupName,
            string eventHubConnectionString,
            string storageConnectionString,
            string leaseContainerName)
        {
            return new EventProcessorHost(eventHubPath, consumerGroupName, eventHubConnectionString, storageConnectionString, leaseContainerName);
        }

        public Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory)
        {
            return host.RegisterEventProcessorFactoryAsync(factory);
        }

        public Task RegisterEventProcessorFactoryAsync(EventProcessorHost host, IEventProcessorFactory factory, EventProcessorOptions options)
        {
            return host.RegisterEventProcessorFactoryAsync(factory, options);
        }
    }
}
