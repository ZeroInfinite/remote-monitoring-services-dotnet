using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Models;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem
{
    public class NotificationEventProcessor : IEventProcessor
    {
        private readonly ILogger logger;
        private readonly IServicesConfig servicesConfig;

        public NotificationEventProcessor(
            ILogger logger,
            IServicesConfig servicesConfig)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            this.logger.Info($"Notification Event Processor Shutting Down. Partition '{context.PartitionId}', Reason: '{reason}'.", () => { });
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            this.logger.Info($"Notification EventProcessor initialized. Partition: '{context.PartitionId}'", () => { });
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            this.logger.Info($"Error on Partition: {context.PartitionId}, Error: {error.Message}", () => { });
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                //// Parse into Action object.
                IEnumerable<dynamic> alertObjects = DeserializeJsonObjectList(data);
                foreach (object jsonObject in alertObjects)
                {
                    string temp = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                    Console.WriteLine(temp);
                    AlarmNotificationAsaModel alarmNotification = JsonConvert.DeserializeObject<AlarmNotificationAsaModel>(temp);
                    // Send Notification.
                    Notification notification = new Notification(servicesConfig)
                    {
                        actionList = alarmNotification.Actions,
                        ruleId = alarmNotification.Rule_id,
                        ruleDescription = alarmNotification.Rule_description
                    };
                    notification.execute().Wait();
                }
            }
            return Task.FromResult(0);
        }

        IEnumerable<object> DeserializeJsonObjectList(string json)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (var stringReader = new StringReader(json))
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    jsonReader.SupportMultipleContent = true;
                    while (jsonReader.Read())
                    {
                        yield return serializer.Deserialize(jsonReader);
                    }
                }
            }
        }
    }
}
