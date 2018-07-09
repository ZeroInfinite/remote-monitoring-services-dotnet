using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.NotificationSystem.Implementation
{
    public class LogicApp : INotification
    {

        private string endointURL { get; set; }
        private string content;
        private List<string> email;
        private string ruleId;
        private string ruleDescription;

        public LogicApp() { }

        public bool setCredentials(Dictionary<string, string> creds)
        {
            try
            {
                this.endointURL = creds["endPointURL"];
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public bool setMessage(string message, string ruleId, string ruleDescription)
        {
            try
            {
                this.content = message;
                this.ruleId = ruleId;
                this.ruleDescription = ruleDescription;
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public bool setReceiver(List<string> receiver)
        {
            try
            {
                this.email = receiver;
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private string generatePayLoad()
        {
            var emailContent = "Alarm fired for rule ID: " + this.ruleId + "  Rule Description: " + this.ruleDescription + " Custom Message: " + this.content;
            if (this.email == null || this.content == null) Console.WriteLine("No data provided");
            return "{\"emailAddress\" : " + JArray.FromObject(this.email) + ",\"template\": \"" + emailContent + "\"}";
        }

        public async Task execute()
        {
            var clientHandler = new HttpClientHandler();
            using (var client = new HttpClient(clientHandler))
            {
                var httpRequest = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(this.endointURL)
                };
                string content = this.generatePayLoad();
                httpRequest.Content = new StringContent(content, Encoding.UTF8, "application/json");
                httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                try
                {
                    HttpResponseMessage response = await client.SendAsync(httpRequest);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error reading contact Info.");
                }
            }
        }
    }
}
