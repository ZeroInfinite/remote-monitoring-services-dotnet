// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Http;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Models;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services
{
    public interface IRules
    {
        // Fetch all the rules from the telemetry service
        Task<IList<RuleApiModel>> GetActiveRulesSortedByIdAsync();

        // Compare two list of rules
        bool RulesAreEquivalent(IList<RuleApiModel> newRules, IList<RuleApiModel> rules);

        // Ping Device Telemetry Service
        Task<Tuple<string, string>> PingAsync();
    }

    public class Rules : IRules
    {
        private readonly IHttpClient httpClient;
        private readonly ILogger log;
        private readonly string rulesWebServiceUrl;
        private readonly int rulesWebServiceTimeout;

        public Rules(
            IServicesConfig config,
            IHttpClient httpClient,
            ILogger logger)
        {
            this.log = logger;
            this.httpClient = httpClient;
            this.rulesWebServiceUrl = config.RulesWebServiceUrl;
            this.rulesWebServiceTimeout = config.RulesWebServiceTimeout;
        }

        // Ping DevioceTelemetry service for status check
        public async Task<Tuple<string, string>> PingAsync()
        {
            var status = "NotRunning";
            var message = "";
            var request = new HttpRequest();
            request.SetUriFromString($"{this.rulesWebServiceUrl}/status");
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
                var msg = "Telemetry service check failed";
                this.log.Error(msg, () => new { e });
                message = e.Message;
            }

            return new Tuple<string, string>(status, message);
        }

        public async Task<IList<RuleApiModel>> GetActiveRulesSortedByIdAsync()
        {
            var request = new HttpRequest();
            request.SetUriFromString(this.rulesWebServiceUrl);
            request.Options.Timeout = this.rulesWebServiceTimeout;
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Cache-Control", "no-cache");
            request.Headers.Add("User-Agent", "ASA Manager");

            var response = await this.httpClient.GetAsync(request);

            if (response.IsError)
            {
                this.log.Error("Request failed", () => new { uri = this.rulesWebServiceUrl, response.StatusCode });
                throw new ExternalDependencyException($"Failed to retrieve the list of rules");
            }

            this.log.Debug("List of rules retrieved", () => new { response.Content });

            // return only active rules, sorted by ID to facilitate comparison
            var list = JsonConvert.DeserializeObject<RuleListApiModel>(response.Content);
            var activeRules = list.Items.Where(x => x.Enabled && !x.Deleted).OrderBy(x => x.Id).ToList();
            this.log.Debug("List of rules deserialized", () => new { ActiveCount = activeRules.Count, TotalCount = list.Items.Count() });

            return activeRules;
        }

        public bool RulesAreEquivalent(IList<RuleApiModel> a, IList<RuleApiModel> b)
        {
            if (a.Count != b.Count) return false;

            return !a.Where((rule, i) => !rule.Equals(b[i])).Any();
        }
    }
}
