// Copyright (c) Microsoft. All rights reserved.

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
        private readonly INotification notification;

        public NotificationEventProcessor(
            ILogger logger,
            IServicesConfig servicesConfig,
            INotification notification)
        {
            this.logger = logger;
            this.servicesConfig = servicesConfig;
            this.notification = notification;
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

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                var data = Encoding.UTF8.GetString(eventData.Body.Array);
                IEnumerable<object> alertObjects = DeserializeJsonObjectList(data);
                foreach (object jsonObject in alertObjects)
                {
                    string temp = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                    AlarmNotificationAsaModel alarmNotification = JsonConvert.DeserializeObject<AlarmNotificationAsaModel>(temp);
                    this.notification.alarm = alarmNotification;
                    await notification.execute();
                }
            }
            await Task.FromResult(0);
        }

        public IEnumerable<object> DeserializeJsonObjectList(string json)
        {
            IList<object> returnList = new List<object>();
            if ((json != null) && (json != ""))
            {
                // Confirming that the string doesn't get invalid characters. 
                json = json.Substring(json.IndexOf("{"));
                object temp = String.Empty;
                JsonSerializer serializer = new JsonSerializer();
                using (var stringReader = new StringReader(json))
                {
                    using (var jsonReader = new JsonTextReader(stringReader))
                    {
                        jsonReader.SupportMultipleContent = true;
                        while (jsonReader.Read())
                        {
                            try
                            {
                                returnList.Add(serializer.Deserialize(jsonReader));
                            }
                            catch (JsonReaderException e)
                            {
                                this.logger.Info("Error parsing the json string", () => new { e });
                                break;
                            }
                            catch (Exception e)
                            {
                                this.logger.Info("Exception parsing the json string", () => new { e });
                                break;
                            }
                        }
                    }
                }
            }
            return returnList;
        }
    }
}
