// Copyright (c) Microsoft. All rights reserved.

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services;
using Microsoft.Azure.IoTSolutions.StorageAdapter.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Models;
using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.StorageAdapter.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly ILogger log;
        private readonly IKeyValueContainer keyValueContainer;

        public StatusController(ILogger logger, IKeyValueContainer keyValueContainer)
        {
            this.log = logger;
            this.keyValueContainer = keyValueContainer;
        }

        public StatusApiModel Get()
        {
            // TODO: calculate the actual service status
            var isOk = true;
            var statusMsg = "Alive and well";
            var errors = new List<string>();
            StatusModel storageStatusModel = new StatusModel();

            // Check connection to CosmosDb
            var storageStatus = this.keyValueContainer.Ping();
            if (!storageStatus.Item1)
            {
                var message = "Unable to use storage";
                isOk = false;
                errors.Add(message);
                storageStatusModel.Message = message;
                storageStatusModel.IsConnected = false;
            }
            else
            {
                storageStatusModel.Message = storageStatus.Item2;
                storageStatusModel.IsConnected = true;
            }

            // Prepare response
            var result = new StatusApiModel(isOk, statusMsg);
            result.Dependencies.Add("Storage", storageStatusModel);
            this.log.Info("Service status request", () => new { Healthy = isOk });
            return new StatusApiModel(isOk, "Alive and well");
        }
    }
}
