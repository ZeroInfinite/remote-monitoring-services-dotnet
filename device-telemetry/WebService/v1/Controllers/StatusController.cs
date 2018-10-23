// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.CosmosDB;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.Storage.TimeSeries;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.Services.StorageAdapter;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.Runtime;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Filters;
using Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Controllers
{
    [Route(Version.PATH + "/[controller]"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    public sealed class StatusController : Controller
    {
        private readonly IConfig config;
        private readonly IStorageAdapterClient storageAdapter;
        private readonly IStorageClient cosmosDb;
        private readonly ITimeSeriesClient timeSeriesClient;
        private readonly ILogger log;

        private const string STORAGE_TYPE_KEY = "StorageType";
        private const string TIME_SERIES_KEY = "tsi";
        private const string TIME_SERIES_EXPLORER_URL_KEY = "TsiExplorerUrl";
        private const string TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR = ".";

        public StatusController(
            IConfig config,
            IStorageClient cosmosDb,
            IStorageAdapterClient storageAdapter,
            ITimeSeriesClient timeSeriesClient,
            ILogger logger)
        {
            this.config = config;
            this.cosmosDb = cosmosDb;
            this.storageAdapter = storageAdapter;
            this.timeSeriesClient = timeSeriesClient;
            this.log = logger;
        }

        [HttpGet]
        public async Task<StatusApiModel> Get()
        {
            var statusIsOk = true;
            var statusMsg = "Alive and well";
            var errors = new List<string>();
            var explorerUrl = string.Empty;
            StatusModel storageAdapterStatusModel = new StatusModel();
            StatusModel cosmosDbStatusModel = new StatusModel();
            StatusModel timeSeriesStatusModel = new StatusModel();

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
                storageAdapterStatusModel.IsConnected = false;
            }

            // Check connection to CosmosDb
            var cosmosDbStatus = this.cosmosDb.Ping();
            if (!cosmosDbStatus.Item1)
            {
                var message = "Unable to use storage";
                statusIsOk = false;
                errors.Add(message);
                cosmosDbStatusModel.Message = message;
                cosmosDbStatusModel.IsConnected = false;
            }
            else
            {
                cosmosDbStatusModel.Message = cosmosDbStatus.Item2;
                cosmosDbStatusModel.IsConnected = true;
            }

            // Add Time Series Dependencies if needed
            if (this.config.ServicesConfig.StorageType.Equals(
                TIME_SERIES_KEY,
                StringComparison.OrdinalIgnoreCase))
            {
                // Check connection to Time Series Insights
                var timeSeriesStatus = await this.timeSeriesClient.PingAsync();
                if (!timeSeriesStatus.Item1)
                {
                    var message = "Unable to use Time Series Insights";
                    statusIsOk = false;
                    errors.Add(message);
                    timeSeriesStatusModel.Message = message;
                    timeSeriesStatusModel.IsConnected = false;
                }
                else
                {
                    timeSeriesStatusModel.Message = timeSeriesStatus.Item2;
                    timeSeriesStatusModel.IsConnected = true;
                }
                // Add Time Series Insights explorer url
                var timeSeriesFqdn = this.config.ServicesConfig.TimeSeriesFqdn;
                var environmentId = timeSeriesFqdn.Substring(0, timeSeriesFqdn.IndexOf(TIME_SERIES_EXPLORER_URL_SEPARATOR_CHAR));
                explorerUrl = this.config.ServicesConfig.TimeSeriesExplorerUrl +
                    "?environmentId=" + environmentId +
                    "&tid=" + this.config.ServicesConfig.ActiveDirectoryTenant;
            }

            // Prepare status message
            if (!statusIsOk)
            {
                statusMsg = string.Join(";", errors);
            }

            // Prepare response
            var result = new StatusApiModel(statusIsOk, statusMsg);
            result.Dependencies.Add("StorageAdapter", storageAdapterStatusModel);
            result.Dependencies.Add("Storage", cosmosDbStatusModel);

            result.Properties.Add(STORAGE_TYPE_KEY, this.config.ServicesConfig.StorageType);

            // Add Time Series Dependencies if needed
            if (this.config.ServicesConfig.StorageType.Equals(
                TIME_SERIES_KEY,
                StringComparison.OrdinalIgnoreCase))
            {
                result.Dependencies.Add("TimeSeries", timeSeriesStatusModel);
                result.Properties.Add(TIME_SERIES_EXPLORER_URL_KEY, explorerUrl);
            }
            this.log.Info("Service status request", () => new { Healthy = statusIsOk, statusMsg });

            return result;
        }
    }
}
