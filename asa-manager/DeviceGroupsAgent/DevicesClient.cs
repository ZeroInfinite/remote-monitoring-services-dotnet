// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent
{
    public interface IDevicesClient
    {
        Task<IEnumerable<string>> GetListAsync(IEnumerable<DeviceGroupConditionApiModel> conditions);
        Task<Tuple<string, string>> PingAsync();
    }

    public class DevicesClient : IDevicesClient
    {
        private readonly ILogger logger;
        private readonly IHttpClient httpClient;
        private readonly string baseUrl;
        private readonly IServicesConfig config;

        public DevicesClient(
            IHttpClient httpClient,
            IServicesConfig config,
            ILogger logger)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            this.baseUrl = $"{config.IotHubManagerServiceUrl}/devices";
        }

        // Ping IotHubManager service for status check
        public async Task<Tuple<string, string>> PingAsync()
        {
            var status = "NotRunning";
            var message = "";
            var request = new HttpRequest();
            request.SetUriFromString($"{this.config.IotHubManagerServiceUrl}/status");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("User-Agent", "ASA Manager");

            try
            {
                var response = await this.httpClient.GetAsync(request);
                if (response.IsError)
                {
                    message = "Status code: " + response.StatusCode;
                }
                else
                {
                    var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                    message = data["Message"].ToString();
                    status = data["Status"].ToString();
                }
            }
            catch (Exception e)
            {
                var msg = "IotHubManager service check failed";
                this.logger.Error(msg, () => new { e });
                message = e.Message;
            }

            return new Tuple<string, string>(status, message);
        }

        /**
         * Given a device group definition, returns a list of the device ids in that group
         */
        public async Task<IEnumerable<string>> GetListAsync(IEnumerable<DeviceGroupConditionApiModel> conditions)
        {
            try
            {
                var query = JsonConvert.SerializeObject(conditions);

                var deviceList = await this.httpClient.GetJsonAsync<DeviceListApiModel>($"{this.baseUrl}?query={query}", $"devices by query {query}");
                return deviceList.Items.Select(d => d.Id);
            }
            catch (Exception e)
            {
                this.logger.Warn("Failed to get list of devices", () => new { e });
                throw new ExternalDependencyException("Unable to get list of devices", e);
            }
        }
    }
}
