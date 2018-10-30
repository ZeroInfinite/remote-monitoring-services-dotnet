// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Models;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.UIConfig.Services.External;
using Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IStorageAdapterClient storageAdapterClient;
        private readonly IUserManagementClient userManagementClient;
        private readonly IDeviceSimulationClient deviceSimulationClient;
        private readonly IDeviceTelemetryClient deviceTelemetryClient;

        public StatusController(
            IStorageAdapterClient storageAdapterClient,
            IUserManagementClient userManagementClient,
            IDeviceSimulationClient deviceSimulationClient,
            IDeviceTelemetryClient deviceTelemetryClient,
            ILogger logger
            )
        {
            this.log = logger;
            this.storageAdapterClient = storageAdapterClient;
            this.userManagementClient = userManagementClient;
            this.deviceSimulationClient = deviceSimulationClient;
            this.deviceTelemetryClient = deviceTelemetryClient;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var statusIsOk = true;
            var statusMsg = "Alive and well";
            var errors = new List<string>();

            StatusModel storageAdapterStatusModel = new StatusModel();
            StatusModel deviceSimulationStatusModel = new StatusModel();
            StatusModel deviceTelemetryStatusModel = new StatusModel();
            StatusModel authStatusModel = new StatusModel();

            // Check access to Storage Adapter
            var storageAdapterStatus = await this.storageAdapterClient.PingAsync();
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

            // Check access to Device Telemetry
            var deviceTelemetryStatus = await this.deviceTelemetryClient.PingAsync();
            if (!storageAdapterStatus.Item1)
            {
                statusIsOk = false;
                var message = "Unable to connect to Device Telemetry service";
                errors.Add(message);
                deviceTelemetryStatusModel.Message = message;
                deviceTelemetryStatusModel.IsConnected = false;
            }
            else
            {
                deviceTelemetryStatusModel.Message = deviceTelemetryStatus.Item2;
                deviceTelemetryStatusModel.IsConnected = true;
            }

            // Check access to Device Simulation
            var devicesSmulationStatus = await this.deviceSimulationClient.PingAsync();
            if (!devicesSmulationStatus.Item1)
            {
                statusIsOk = false;
                var message = "Unable to connect to DeviceSimulation service";
                errors.Add(message);
                deviceSimulationStatusModel.Message = message;
                deviceSimulationStatusModel.IsConnected = false;
            }
            else
            {
                deviceSimulationStatusModel.Message = devicesSmulationStatus.Item2;
                deviceSimulationStatusModel.IsConnected = true;
            }

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

            // Prepare status message
            if (!statusIsOk)
            {
                statusMsg = string.Join(";", errors);
            }

            // Prepare response
            var result = new StatusApiModel(statusIsOk, statusMsg);
            result.Dependencies.Add("StorageAdapter", storageAdapterStatusModel);
            result.Dependencies.Add("DeviceTelemetry", deviceTelemetryStatusModel);
            result.Dependencies.Add("DeviceSimulation", deviceSimulationStatusModel);
            result.Dependencies.Add("Auth", authStatusModel);
            
            this.log.Info("Service status request", () => new { Healthy = statusIsOk, statusMsg });
            return result;
        }
    }
}
