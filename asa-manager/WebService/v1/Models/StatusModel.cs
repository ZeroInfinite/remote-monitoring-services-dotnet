// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.IoTSolutions.AsaManager.WebService.v1.Models
{
    public class StatusModel
    {
        [JsonProperty(PropertyName = "Message", Order = 10)]
        public string Message { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "StatusType", Order = 20)]
        public StatusType Status { get; set; }
    }

    public enum StatusType
    {
        Running,
        NotRunning,
        Disabled
    }
}
