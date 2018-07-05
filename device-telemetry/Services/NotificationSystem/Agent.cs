using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public interface IAgent
    {
        Task RunAsync(CancellationToken runState);
    }

    public class Agent : IAgent
    {
        // For temporary use.
        private const string EhConnectionString = "Endpoint=sb://eventhubnamespace-f3pvd.servicebus.windows.net/;SharedAccessKeyName=NotificationSystem;SharedAccessKey=W8C1Y/ZoBglooXxc1O1r2y5QBl7sa0nIwrYRl5h5YhA=;EntityPath=notificationsystem";
        private const string EhEntityPath = "notificationsystem";
        private const string StorageContainerName = "anothersystem";
        private const string StorageAccountName = "aayushdemo";
        private const string StorageAccountKey = "qIFS9KOWkR+GUymNElgeGGQhwvATW5SNRii4R4OTWYi0aiT/JrIFnnLyJlUVigyIoNzr5TR9utGwZoK2ffioAw==";

        private static readonly string StorageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", StorageAccountName, StorageAccountKey);


        private readonly ILogger logger;
        private readonly IServicesConfig servicesConfig;
        private readonly IEventProcessorFactory notificationEventProcessorFactory;
        private EventProcessorOptions eventProcessorOptions;
        private CancellationToken runState;

        public Agent(ILogger logger,
            IServicesConfig servicesConfig,
            IEventProcessorFactory notificationEventProcessorFactory)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.notificationEventProcessorFactory = notificationEventProcessorFactory;
        }
        public async Task RunAsync(CancellationToken runState)
        {
            this.runState = runState;

            this.logger.Info("Notification System Running", () => { });

            await this.SetupEventHub(this.runState);

            this.logger.Info("Notification System Exiting", () => { });
        }

        private async Task SetupEventHub(CancellationToken runState)
        {
            if (!runState.IsCancellationRequested)
            {
                try
                {
                    var eventProcessorHost = new EventProcessorHost(
                        EhEntityPath,
                        PartitionReceiver.DefaultConsumerGroupName,
                        EhConnectionString,
                        StorageConnectionString,
                        StorageContainerName);

                    eventProcessorOptions = new EventProcessorOptions
                    {
                        InitialOffsetProvider = (partitionId) => EventPosition.FromEnqueuedTime(DateTime.UtcNow)
                    };
                    await eventProcessorHost.RegisterEventProcessorFactoryAsync(this.notificationEventProcessorFactory, eventProcessorOptions);
                }
                catch (Exception e)
                {
                    this.logger.Error("Received error setting up event hub. Will not receive updates from devices", () => new { e });
                }
            }
        }
    }
}
