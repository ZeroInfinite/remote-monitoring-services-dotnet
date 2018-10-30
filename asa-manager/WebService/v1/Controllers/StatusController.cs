// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.AsaManager.DeviceGroupsAgent;
using Microsoft.Azure.IoTSolutions.AsaManager.Services;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Controllers
{
    /*
     * StatusController is not accessible on deployed services because it is an internal service running in
     * the background. The below status API responds only on local machine. 
     */
    [Route(Version.PATH + "/[controller]"), ExceptionsFilter]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IDeviceGroupsClient configClient;
        private readonly IDevicesClient ioTHubManagerClient;
        private readonly IRules deviceTelemetryClient;
        private readonly IBlobStorageHelper blobStorageHelper;
        private readonly IAsaStorage asaStorage;
        private readonly IAgent eventHubHelper;

        public StatusController(
            IDeviceGroupsClient configClient,
            IDevicesClient ioTHubManagerClient,
            IRules deviceTelemetryClient,
            IBlobStorageHelper blobStorageHelper,
            IAsaStorage asaStorage,
            IAgent eventHubHelper,
            ILogger logger
            )
        {
            this.log = logger;
            this.configClient = configClient;
            this.ioTHubManagerClient = ioTHubManagerClient;
            this.blobStorageHelper = blobStorageHelper;
            this.deviceTelemetryClient = deviceTelemetryClient;
            this.asaStorage = asaStorage;
            this.eventHubHelper = eventHubHelper;
        }

        public async Task<StatusApiModel> GetAsync()
        {
            var Message = "Alive and well";
            var errors = new List<string>();
            var result = new StatusApiModel();
            result.Status = StatusType.Running;

            // Check access to Config
            var configTuple = await this.configClient.PingAsync();
            SetServiceStatus(result.Dependencies, errors, configTuple, "Config", result.Status);

            // Check access to Device Telemetry
            var deviceTelemetryTuple = await this.deviceTelemetryClient.PingAsync();
            SetServiceStatus(result.Dependencies, errors, deviceTelemetryTuple, "DeviceTelemetry", result.Status);

            // Check access to IoTHubManager
            var ioTHubmanagerTuple = await this.ioTHubManagerClient.PingAsync();
            SetServiceStatus(result.Dependencies, errors, ioTHubmanagerTuple, "IoTHubManager", result.Status);

            // Check access to Blob
            var blobTuple = await this.blobStorageHelper.PingAsync();
            SetServiceStatus(result.Dependencies, errors, blobTuple, "Blob", result.Status);

            // Check access to Storage
            var storageTuple = await this.asaStorage.PingAsync();
            SetServiceStatus(result.Dependencies, errors, storageTuple, "Storage", result.Status);

            // Check access to Event
            var eventHubTuple = await this.eventHubHelper.PingEventHubAsync();
            SetServiceStatus(result.Dependencies, errors, eventHubTuple, "EventHub", result.Status);
            result.Properties.Add("EventHubSetUp", this.eventHubHelper.IsEventHubSetupSuccessful().ToString());

            // Prepare status message
            if (result.Status == StatusType.NotRunning)
            {
                Message = string.Join(";", errors);
            }

            this.log.Info("Service status request", () => new { Healthy = result.Status, Message });
            return result;
        }

        private void SetServiceStatus(
            Dictionary<string, StatusModel> dependencies,
            List<string> errors,
            Tuple<string, string> serviceTuple,
            string dependencyName,
            StatusType status
            )
        {
            var serviceStatus = (StatusType)Enum.Parse(typeof(StatusType), serviceTuple.Item1); ;
            var serviceStatusModel = new StatusModel
            {
                Message = serviceTuple.Item2,
                Status = serviceStatus
            };

            if (serviceStatus == StatusType.NotRunning)
            {
                errors.Add("{" + dependencyName + ": " + serviceTuple.Item2 + "}, ");
                status = serviceStatus;
            }
            dependencies.Add(dependencyName, serviceStatusModel);
        }
    }
}
