// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.External;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.Auth;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public class StatusController : Controller
    {

        private const string JSON_TRUE = "true";
        private const string JSON_FALSE = "false";
        private const string PREPROVISIONED_IOTHUB_KEY = "PreprovisionedIoTHub";

        private readonly ILogger log;
        private readonly IStorageAdapterClient storageAdapter;
        private readonly IUserManagementClient userManagementClient;
        private readonly IServicesConfig servicesConfig;
        private readonly IDevices devices;
        private readonly IClientAuthConfig authConfig;
        private readonly IDeviceProperties deviceProperties;

        public StatusController(
            IStorageAdapterClient storageAdapter,
            IUserManagementClient userManagementClient,
            ILogger logger,
            IServicesConfig servicesConfig,
            IDevices devices,
            IDeviceProperties deviceProperties,
            IClientAuthConfig authConfig
            )
        {
            this.log = logger;
            this.storageAdapter = storageAdapter;
            this.userManagementClient = userManagementClient;
            this.servicesConfig = servicesConfig;
            this.devices = devices;
            this.deviceProperties = deviceProperties;
            this.authConfig = authConfig;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var statusIsOk = true;
            var statusMsg = "Alive and well";
            var errors = new List<string>();

            StatusModel storageAdapterStatusModel = new StatusModel();
            StatusModel authStatusModel = new StatusModel();
            StatusModel ioTHubStatusModel = new StatusModel();

            // Check access to Storage Adapter
            var storageAdapterStatus = await this.storageAdapter.PingAsync();
            if (!storageAdapterStatus.Item1)
            {
                statusIsOk = false;
                var message = "Unable to connect to Storage Adapter service";
                errors.Add(message);
                storageAdapterStatusModel.Message = message;
                storageAdapterStatusModel.IsConnected = false;
            }
            else
            {
                storageAdapterStatusModel.Message = storageAdapterStatus.Item2;
                storageAdapterStatusModel.IsConnected = true;
            }

            if (this.authConfig.AuthRequired)
            {
                // Check access to Auth
                var authStatus = await this.userManagementClient.PingAsync();
                if (!authStatus.Item1)
                {
                    statusIsOk = false;
                    var message = "Unable to connect to Auth service";
                    errors.Add(message);
                    authStatusModel.Message = message;
                    authStatusModel.IsConnected = false;
                }
                else
                {
                    authStatusModel.Message = authStatus.Item2;
                    authStatusModel.IsConnected = true;
                }
            }

            // Check access to IoTHub
            var ioTHubStatus = await this.devices.PingRegistryAsync();
            if (!ioTHubStatus.Item1)
            {
                statusIsOk = false;
                var message = "Unable to connect to IoT Hub";
                errors.Add(message);
                ioTHubStatusModel.Message = message;
                ioTHubStatusModel.IsConnected = false;
            }
            else
            {
                ioTHubStatusModel.Message = ioTHubStatus.Item2;
                ioTHubStatusModel.IsConnected = true;
            }

            // Prepare status message
            if (!statusIsOk)
            {
                statusMsg = string.Join(";", errors);
            }

            // Prepare response
            var result = new StatusApiModel(statusIsOk, statusMsg);
            result.Dependencies.Add("StorageAdapter", storageAdapterStatusModel);
            result.Dependencies.Add("Auth", authStatusModel);
            result.Dependencies.Add("IoTHub", ioTHubStatusModel);

            // Preprovisioned IoT hub status
            var isHubPreprovisioned = this.IsHubConnectionStringConfigured();
            result.Properties.Add(PREPROVISIONED_IOTHUB_KEY, isHubPreprovisioned ? JSON_TRUE : JSON_FALSE);
            result.Properties.Add(
                "DevicePropertiesCache LastUpdated",
                this.deviceProperties.GetDevicePropertiesCacheLastUpdated().ToLongDateString());

            this.log.Info("Service status request", () => new { Healthy = statusIsOk, statusMsg });
            return result;
        }

        // Check whether the configuration contains a connection string
        private bool IsHubConnectionStringConfigured()
        {
            var cs = this.servicesConfig?.IoTHubConnString?.ToLowerInvariant().Trim();
            return (!string.IsNullOrEmpty(cs)
                    && cs.Contains("hostname=")
                    && cs.Contains("sharedaccesskeyname=")
                    && cs.Contains("sharedaccesskey="));
        }
    }
}
