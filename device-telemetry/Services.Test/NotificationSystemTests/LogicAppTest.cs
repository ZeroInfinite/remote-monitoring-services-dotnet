using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Services.Test.helpers;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation;
using Moq;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;

namespace Services.Test
{
    public class LogicAppTest
    {
        private readonly Mock<ILogger> logMock;
        private readonly Mock<IHttpRequest> httpRequestMock;
        private readonly Mock<IHttpClient> httpClientMock;

        public LogicAppTest()
        {
            this.logMock = new Mock<ILogger>();
            this.httpClientMock = new Mock<IHttpClient>();
            this.httpRequestMock = new Mock<IHttpRequest>();
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_SendPostRequestToLogicAppEndPointUrl_WhenValidEndPointUrl()
        {
            // Setup
            var tempLogicApp = new LogicApp(It.IsAny<string>(), It.IsAny<string>(),
                this.httpRequestMock.Object, this.httpClientMock.Object, this.logMock.Object);
            this.httpClientMock.Setup(x => x.PostAsync(It.IsAny<IHttpRequest>())).Returns(
                Task.FromResult<IHttpResponse>(new HttpResponse(0, It.IsAny<string>(), It.IsAny<HttpResponseHeaders>())));
            tempLogicApp.setReceiver(new List<string>() { It.IsAny<string>() });

            // Act
            tempLogicApp.execute().Wait();

            // Assert => Logger value same :( 
            this.logMock.Verify(x => x.Info("Error sending request to the LogicApp endpoiint URL", It.IsAny<Action>), Times.Once);
        }

        [Fact, Trait(Constants.TYPE, Constants.UNIT_TEST)]
        public void Should_WriteErrorToTheLogger_When_InvalidEndPointUrl()
        {
            // Setup
            var tempLogicApp = new LogicApp(It.IsAny<string>(), It.IsAny<string>(),
                this.httpRequestMock.Object, this.httpClientMock.Object, this.logMock.Object);
            tempLogicApp.setReceiver(new List<string>() { It.IsAny<string>() });
            this.httpClientMock.Setup(x => x.PostAsync(It.IsAny<IHttpRequest>())).Returns(
                Task.FromResult<IHttpResponse>(new HttpResponse(System.Net.HttpStatusCode.OK, It.IsAny<string>(), It.IsAny<HttpResponseHeaders>())));

            // Act
            tempLogicApp.execute().Wait();

            // Assert
            this.logMock.Verify(x => x.Info(It.IsAny<string>(), It.IsAny<Action>()), Times.Never);
        }

    }
}
