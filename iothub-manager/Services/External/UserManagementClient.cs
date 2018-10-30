﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Http;
using Microsoft.Azure.IoTSolutions.IotHubManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.External
{
    public interface IUserManagementClient
    {
        Task<IEnumerable<string>> GetAllowedActionsAsync(string userObjectId, IEnumerable<string> roles);
        Task<Tuple<bool, string>> PingAsync();
    }

    public class UserManagementClient : IUserManagementClient
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string serviceUri;

        public UserManagementClient(IHttpClient httpClient, IServicesConfig config, ILogger logger)
        {
            this.httpClient = httpClient;
            this.log = logger;
            this.serviceUri = config.UserManagementApiUrl;
        }

        public async Task<IEnumerable<string>> GetAllowedActionsAsync(string userObjectId, IEnumerable<string> roles)
        {
            var request = this.CreateRequest($"users/{userObjectId}/allowedActions", roles);
            var response = await this.httpClient.PostAsync(request);
            this.CheckStatusCode(response, request);

            return JsonConvert.DeserializeObject<IEnumerable<string>>(response.Content);
        }

        public async Task<Tuple<bool, string>> PingAsync()
        {
            try
            {
                var response = await this.httpClient.GetAsync(this.CreateRequest("status"));

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
                this.log.Error("Storage adapter check failed", () => new { e });
                return new Tuple<bool, string>(false, e.Message);
            }
        }

        private HttpRequest CreateRequest(string path, IEnumerable<string> content = null)
        {
            var request = new HttpRequest();
            request.SetUriFromString($"{this.serviceUri}/{path}");
            if (this.serviceUri.ToLowerInvariant().StartsWith("https:"))
            {
                request.Options.AllowInsecureSSLServer = true;
            }

            if (content != null)
            {
                request.SetContent(content);
            }

            return request;
        }

        private void CheckStatusCode(IHttpResponse response, IHttpRequest request)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            this.log.Info($"Auth service returns {response.StatusCode} for request {request.Uri}", () => new
            {
                request.Uri,
                response.StatusCode,
                response.Content
            });

            switch (response.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new ResourceNotFoundException($"{response.Content}, request URL = {request.Uri}");
                default:
                    throw new HttpRequestException($"Http request failed, status code = {response.StatusCode}, content = {response.Content}, request URL = {request.Uri}");
            }
        }
    }
}
