# Environment Variables

The service requires some mandatory environment settings and supports some
customizations via optional environment variables.

Depending on the OS and how the microservice is run, environment variables
can be defined in multiple ways. See below how to set environment variables
in the different contexts.

## Mandatory Settings

* `PCS_TELEMETRY_DOCUMENTDB_CONNSTRING` [mandatory]: contains the full connection string required to connect the telemetry to the Document DB in the Azure cloud, where the Rules, Messages and the Alarms are stored.

* `PCS_STORAGEADAPTER_WEBSERVICE_URL` [mandatory]: the URL where the storage adapter service is available, e.g. `http://127.0.0.1:9022`

* `PCS_TELEMETRY_EVENTHUB_CONNSTRING`: [mandatory]:contains the full connection string required to connect the service to the EventHub.

* `PCS_TELEMETRY_EVENTHUB_NAME`: [mandatory]: 

* `PCS_TELEMETRY_DATA_AZUREBLOB_ACCOUNT`

* `PCS_TELEMETRY_DATA_AZUREBLOB_KEY`

* `PCS_TELEMETRY_LOGICAPP_ENDPOINT_URL`

## Optional Settings

# How to define Environment Variables

### Environment Variables in Windows

### Environment Variables in Linux and MacOS

### Environment variables when using Visual Studio

### Environment variables when using IntelliJ Rider

### Environment variable when using Docker command line