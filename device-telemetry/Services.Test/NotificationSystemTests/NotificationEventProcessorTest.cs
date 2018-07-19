using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Runtime;
using Moq;
using Newtonsoft.Json;
using Services.Test.helpers;
using Xunit;

namespace Services.Test
{
    public class NotificationEventProcessorTest
    {
        private readonly Mock<INotification> notificationMock;
        private readonly Mock<ILogger> logMock;
        private readonly Mock<IServicesConfig> servicesConfigMock;

        private IEventProcessor notificationEventProcessor;

        public NotificationEventProcessorTest()
        {
            this.notificationMock = new Mock<INotification>();
            this.servicesConfigMock = new Mock<IServicesConfig>();
            this.logMock = new Mock<ILogger>();

            this.notificationEventProcessor = new NotificationEventProcessor(this.logMock.Object, this.servicesConfigMock.Object, this.notificationMock.Object);
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_CallExecuteForNTimesEqualToNumberOfJsonTokenInEventData_When_NJsonObjectsInOneEventData(int numJson, int numCalls)
        {
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventData = new EventData(getSamplePayLoadDataWithNalerts(numJson));

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), new EventData[] { tempEventData });

            // Assert
            this.notificationMock.Verify(e => e.execute(), Times.Exactly(numCalls));
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_CallExecuteForNTimesEqualToNumberOfEventDataInMessages_When_EachEventDataHasOneJsonObject(int numEventData, int numCalls)
        { // Task.Completed Task is being passed over or Deserialize JsonObjectList is returning directly to this method.
            // Setup
            this.notificationMock.Setup(a => a.execute()).Returns(Task.CompletedTask);
            var tempEventData = new EventData(getSamplePayLoadDataWithNalerts(1));

            // Act
            this.notificationEventProcessor.ProcessEventsAsync(It.IsAny<PartitionContext>(), Enumerable.Repeat<EventData>(tempEventData, numEventData).ToArray<EventData>());

            // Assert
            this.notificationMock.Verify(e => e.execute(), Times.Exactly(numCalls));
        }

        [Theory, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        [InlineData(2, 2)]
        [InlineData(0, 0)]
        public void Should_ReturnAnEnumeratorOverAListOfProperJsonStrings_When_ValidInputJsonStringWithNnumberOfJsonTokens(int numJson, int numReturnJsonString)
        {
            // Setup
            var tempJson = getSampleJsonRepeatedNTimes(numJson);
            var tempNotificationEventProcessor = new NotificationEventProcessor(this.logMock.Object, this.servicesConfigMock.Object, this.notificationMock.Object);

            // Act and Assert
            Assert.Equal(tempNotificationEventProcessor.DeserializeJsonObjectList(tempJson).ToArray().Length, numReturnJsonString);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_NotCallExecuteMethod_WhenEmptyString()
        {
            // Setup
            var tempJson = getSampleJsonRepeatedNTimes(1).Substring(0, getSampleJsonRepeatedNTimes(1).Length - 2);
            var tempNotificationEventProcessor = new NotificationEventProcessor(this.logMock.Object, this.servicesConfigMock.Object, this.notificationMock.Object);

            // Act
            tempNotificationEventProcessor.DeserializeJsonObjectList(tempJson);

            // Assert
            this.logMock.Verify(a => a.Info(It.IsAny<string>(), It.IsAny<Action>()), Times.Never);
        }

        private static byte[] getSamplePayLoadDataWithNalerts(int n)
        {
            var dictionary = new Dictionary<string, object>()
            {
                {"created", "342874237482374" },
                {"modified", "1234123123123" },
                {"rule.description", "Pressure > 380 Aayush" },
                {"rule.severity", "Critical" },
                {"rule.id", "12345" },
                {"rule.actions", new List<object>()
                {
                    new Dictionary<string, object>(){
                    {"Type", "Email" },
                    {"Parameters", new Dictionary<string, object>(){
                        {"Template", "This is a test email."},
                        {"Email", new List<string>(){ "agupta.aayush8484@gmail.com", "t-aagupt@microsoft.com" } }
                    }
                    }
                }
                }
                },
                {"device.id", "213123" },
                {"device.msg.received", "1234123123123" }
            };
            var dictionaryList = Enumerable.Repeat<Dictionary<string, object>>(dictionary, n);
            var jsonDictionary = JsonConvert.SerializeObject(dictionaryList);

            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();
            binaryFormatter.Serialize(memoryStream, jsonDictionary);
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream.ToArray();
        }

        private static string getSampleJsonRepeatedNTimes(int n)
        {
            var dictionary = new Dictionary<string, object>()
            {
                {"created", "342874237482374" },
                {"modified", "1234123123123" },
                {"rule.description", "Pressure > 380 Aayush" },
                {"rule.severity", "Critical" },
                {"rule.id", "12345" },
                {"rule.actions", new List<object>()
                {
                    new Dictionary<string, object>(){
                    {"Type", "Email" },
                    {"Parameters", new Dictionary<string, object>(){
                        {"Template", "This is a test email."},
                        {"Email", new List<string>(){ "agupta.aayush8484@gmail.com", "t-aagupt@microsoft.com" } }
                    }
                    }
                }
                }
                },
                {"device.id", "213123" },
                {"device.msg.received", "1234123123123" }

            };
            var a = JsonConvert.SerializeObject(dictionary);
            return String.Join("", Enumerable.Repeat<string>(a, n).ToArray());
        }
    }
}
