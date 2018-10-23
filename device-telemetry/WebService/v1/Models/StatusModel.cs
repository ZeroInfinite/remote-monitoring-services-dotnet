// Copyright (c) Microsoft. All rights reserved.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.DeviceTelemetry.WebService.v1.Models
{
    public class StatusModel
    {
        [JsonProperty(PropertyName = "Message", Order = 10)]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "IsConnected", Order = 20)]
        public Boolean IsConnected { get; set; }
    }
}
