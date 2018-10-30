// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.IoTSolutions.UIConfig.Services.External
{
    public interface IStorageAdapterClient
    {
        Task<ValueApiModel> GetAsync(string collectionId, string key);
        Task<ValueListApiModel> GetAllAsync(string collectionId);
        Task<ValueApiModel> CreateAsync(string collectionId, string value);
        Task<ValueApiModel> UpdateAsync(string collectionId, string key, string value, string etag);
        Task DeleteAsync(string collectionId, string key);
        Task<Tuple<bool, string>> PingAsync();
    }
}
