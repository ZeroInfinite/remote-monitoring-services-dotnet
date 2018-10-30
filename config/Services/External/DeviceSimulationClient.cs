// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Helpers;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Http;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IDeviceSimulationClient
    {
        Task<SimulationApiModel> GetDefaultSimulationAsync();
        Task UpdateSimulationAsync(SimulationApiModel model);
        Task<Tuple<bool, string>> PingAsync();
    }

    public class DeviceSimulationClient : IDeviceSimulationClient
    {
        private const int DEFAULT_SIMULATION_ID = 1;
        private readonly IHttpClientWrapper httpClient;
        private readonly string serviceUri;
        private readonly IHttpClient statusClient;

        public DeviceSimulationClient(
            IHttpClientWrapper httpClient,
            IHttpClient statusClient,
            IServicesConfig config)
        {
            this.httpClient = httpClient;
            this.serviceUri = config.DeviceSimulationApiUrl;
            this.statusClient = statusClient;
        }

        public async Task<SimulationApiModel> GetDefaultSimulationAsync()
        {
            return await this.httpClient.GetAsync<SimulationApiModel>($"{this.serviceUri}/simulations/{DEFAULT_SIMULATION_ID}", $"Simulation {DEFAULT_SIMULATION_ID}", true);
        }

        public async Task UpdateSimulationAsync(SimulationApiModel model)
        {
            await this.httpClient.PutAsync($"{this.serviceUri}/simulations/{model.Id}", $"Simulation {model.Id}", model);
        }

        public async Task<Tuple<bool, string>> PingAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/status");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("User-Agent", "Config");

            try
            {
                IHttpResponse response = await this.statusClient.GetAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    return new Tuple<bool, string>(false, "Status code: " + response.StatusCode);
                }

                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(response.Content);
                if (Convert.ToBoolean(data["IsConnected"]).Equals(true))
                {
                    return new Tuple<bool, string>(true, data["Status"].ToString());
                }

                return new Tuple<bool, string>(false, data["Status"].ToString());
            }
            catch (Exception e)
            {
                return new Tuple<bool, string>(false, e.Message);
            }
        }
    }
}