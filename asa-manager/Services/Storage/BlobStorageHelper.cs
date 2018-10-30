// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.AsaManager.Services.Runtime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Microsoft.Azure.IoTSolutions.AsaManager.Services.Storage
{
    public interface IBlobStorageHelper
    {
        Task WriteBlobFromFileAsync(string blobName, string fileName);
        Task<Tuple<string, string>> PingAsync();
    }

    public class BlobStorageHelper : IBlobStorageHelper
    {
        private readonly ICloudStorageWrapper cloudStorageWrapper;
        private CloudBlobContainer cloudBlobContainer;
        private readonly IBlobStorageConfig blobStorageConfig;
        private readonly ILogger logger;
        private bool isInitialized;

        public BlobStorageHelper(
            IBlobStorageConfig blobStorageConfig,
            ICloudStorageWrapper cloudStorageWrapper,
            ILogger logger)
        {
            this.logger = logger;
            this.blobStorageConfig = blobStorageConfig;
            this.cloudStorageWrapper = cloudStorageWrapper;
            this.isInitialized = false;
        }

        private async Task InitializeBlobStorage()
        {
            if (!this.isInitialized)
            {
                string storageConnectionString =
                    $"DefaultEndpointsProtocol=https;AccountName={this.blobStorageConfig.AccountName};AccountKey={this.blobStorageConfig.AccountKey};EndpointSuffix={this.blobStorageConfig.EndpointSuffix}";
                CloudStorageAccount account = this.cloudStorageWrapper.Parse(storageConnectionString);
                CloudBlobClient blobClient = this.cloudStorageWrapper.CreateCloudBlobClient(account);

                this.cloudBlobContainer = this.cloudStorageWrapper.GetContainerReference(blobClient, this.blobStorageConfig.ReferenceDataContainer);
                await this.cloudStorageWrapper.CreateIfNotExistsAsync(
                    this.cloudBlobContainer,
                    BlobContainerPublicAccessType.Blob, 
                    new BlobRequestOptions(), 
                    new OperationContext());
                this.isInitialized = true;
            }
        }

        public async Task WriteBlobFromFileAsync(string blobName, string fileName)
        {
            try
            {
                await this.InitializeBlobStorage();
                CloudBlockBlob blockBlob = this.cloudStorageWrapper.GetBlockBlobReference(this.cloudBlobContainer, blobName);
                await this.cloudStorageWrapper.UploadFromFileAsync(blockBlob, fileName);
            }
            catch (Exception e)
            {
                this.logger.Error("Unable to upload reference data to blob", () => new { e });
            }
        }

        public async Task<Tuple<string, string>> PingAsync()
        {
            var status = "NotRunning";
            var message = "";

            try
            {
                var response = await cloudBlobContainer.ExistsAsync();
                if (response)
                {
                    message = "Connected to cloudBlobContainer";
                    status = "Running";
                }
                else
                {
                    message = "Unable to Connect to cloudBlobContainer";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return new Tuple<string, string>(status, message);
        }
    }
}
