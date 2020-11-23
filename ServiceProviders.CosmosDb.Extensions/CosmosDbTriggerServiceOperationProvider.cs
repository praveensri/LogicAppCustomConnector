using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.Workflows.ServiceProviders.Abstractions;
using Microsoft.Azure.Workflows.ServiceProviders.WebJobs.Abstractions.Providers;
using Microsoft.WindowsAzure.ResourceStack.Common.Collections;
using Microsoft.WindowsAzure.ResourceStack.Common.Extensions;
using Microsoft.WindowsAzure.ResourceStack.Common.Storage.Cosmos;
using Microsoft.WindowsAzure.ResourceStack.Common.Swagger.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceProviders.CosmosDb.Extensions
{
    [ServiceOperationsProvider(Id = CosmosDbTriggerServiceOperationProvider.ServiceId, Name = CosmosDbTriggerServiceOperationProvider.ServiceName)]
    public class CosmosDbTriggerServiceOperationProvider : IServiceOperationsTriggerProvider
    {
        /// <summary>
        /// The service name.
        /// </summary>
        public const string ServiceName = "cosmosdb";

        /// <summary>
        /// The service id.
        /// </summary>
        public const string ServiceId = "/serviceProviders/cosmosdb";

        /// <summary>
        /// Gets or sets service Operations.
        /// </summary>
        private readonly List<ServiceOperation> serviceOperationsList;

        /// <summary>
        /// The set of all API Operations.
        /// </summary>
        private readonly InsensitiveDictionary<ServiceOperation> apiOperationsList;

        public CosmosDbTriggerServiceOperationProvider()
        {
            this.serviceOperationsList = new List<ServiceOperation>();
            this.apiOperationsList = new InsensitiveDictionary<ServiceOperation>();

            this.apiOperationsList.AddRange(new InsensitiveDictionary<ServiceOperation>
            {
                { "receiveDocument", ReceiveDocument },
            });

            this.serviceOperationsList.AddRange(new List<ServiceOperation>
            {
                { ReceiveDocument.CloneWithManifest(new ServiceOperationManifest
        {
            ConnectionReference = new ConnectionReferenceFormat
            {
                ReferenceKeyFormat = ConnectionReferenceKeyFormat.ServiceProvider,
            },
            Settings = new OperationManifestSettings
            {
                SecureData = new OperationManifestSettingWithOptions<SecureDataOptions>(),
                TrackedProperties = new OperationManifestSetting
                {
                    Scopes = new OperationScope[] { OperationScope.Trigger },
                },
            },
            InputsLocation = new InputsLocation[]
            {
                InputsLocation.Inputs,
                InputsLocation.Parameters,
            },
            Outputs = new SwaggerSchema
            {
                Type = SwaggerSchemaType.Object,
                Properties = new OrdinalDictionary<SwaggerSchema>
                {
                    {
                        "body", new SwaggerSchema
                        {
                            Type = SwaggerSchemaType.Array,
                            Title = "Receive document",
                            Description = "Receive document description",
                            Items = new SwaggerSchema
                            {
                                Type = SwaggerSchemaType.Object,
                                Properties = new OrdinalDictionary<SwaggerSchema>
        {
            {
                "contentData", new SwaggerSchema
                {
                    Type = SwaggerSchemaType.String,
                    Title = "Content",
                    Format = "byte",
                    Description = "content",
                }
            },
                {
                    "Properties", new SwaggerSchema
                    {
                        Type = SwaggerSchemaType.Object,
                        Title = "documentProperties",
                        AdditionalProperties = new JObject
                        {
                            { "type", "object" },
                            { "properties", new JObject { } },
                            { "required", new JObject { } },
                        },
                        Description = "document data properties",
                    }
                },
            },
                            },
                        }
                    },
                },
            },
            Inputs = new SwaggerSchema
            {
                Type = SwaggerSchemaType.Object,
                Properties = new OrdinalDictionary<SwaggerSchema>
            {
                {
                    "databaseName", new SwaggerSchema
                    {
                        Type = SwaggerSchemaType.String,
                        Title = "database name",
                        Description = "database name",
                    }
                },
                {
                     "collectionName", new SwaggerSchema
                    {
                        Type = SwaggerSchemaType.String,
                        Title = "collection name",
                        Description = "collection name",
                    } 
                },
            },
                Required = new string[]
            {
                "databaseName",
            },
            },
            Connector = CosmosDbOperationApi,
            Trigger = TriggerType.Batch,
            Recurrence = new RecurrenceSetting
            {
                Type = RecurrenceType.None,
            },
        }) },
            });
        }

        public string GetBindingConnectionInformation(string operationId, InsensitiveDictionary<JToken> connectionParameters)
        {
            return ServiceOperationsProviderUtilities
                    .GetRequiredParameterValue(
                        serviceId: ServiceId,
                        operationId: operationId,
                        parameterName: "connectionString",
                        parameters: connectionParameters)?
                    .ToValue<string>();
        }

        public string GetFunctionTriggerType()
        {
            return "cosmosDBTrigger";
        }

        public IEnumerable<ServiceOperation> GetOperations(bool expandManifest)
        {
            return expandManifest ? serviceOperationsList : GetApiOperations();
        }

        /// <summary>
        /// Gets the operations.
        /// </summary>
        private IEnumerable<ServiceOperation> GetApiOperations()
        {
            return this.apiOperationsList.Values;
        }

        public ServiceOperationApi GetService()
        {
            return CosmosDbOperationApi;
        }

        public Task<ServiceOperationResponse> InvokeActionOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The receive documents operation.
        /// </summary>
        public static readonly ServiceOperation ReceiveDocument = new ServiceOperation
        {
            Name = "receiveDocument",
            Id = "receiveDocument",
            Type = "receiveDocument",
            Properties = new ServiceOperationProperties
            {
                Api = new ServiceOperationApi
                {
                    Name = "cosmosdb",
                    Id = "/serviceProviders/cosmosdb",
                    Type = DesignerApiType.ServiceProvider,
                    Properties = new ServiceOperationApiProperties
                    {
                        BrandColor = 0xC4D5FF,
                        Description = "Connect to Azure Cosmos db to receive document.",
                        DisplayName = "Cosmos Db",
                        IconUri = new Uri("https://github.com/praveensri/LogicAppCustomConnector/blob/main/ServiceProviders.CosmosDb.Extensions/icon.png"),
                        Capabilities = new ApiCapability[] { ApiCapability.Triggers },
                        ConnectionParameters = new ConnectionParameters
                        {
                            ConnectionString = new ConnectionStringParameters
                            {
                                Type = ConnectionStringType.SecureString,
                                ParameterSource = ConnectionParameterSource.AppConfiguration,
                                UIDefinition = new UIDefinition
                                {
                                    DisplayName = "Connection String",
                                    Description = "Azure Cosmos db Connection String",
                                    Tooltip = "Provide Azure Cosmos db Connection String",
                                    Constraints = new Constraints
                                    {
                                        Required = "true",
                                    },
                                },
                            },
                        },
                    },
                }.GetFlattenedApi(),
                Summary = "receive document",
                Description = "receive document",
                Visibility = Visibility.Important,
                OperationType = OperationType.ServiceProvider,
                BrandColor = 0x1C3A56,
                IconUri = new Uri("https://github.com/praveensri/LogicAppCustomConnector/blob/main/ServiceProviders.CosmosDb.Extensions/icon.png"),
                Trigger = TriggerType.Batch,
            },
        };

        /// <summary>
        /// The Azure cosmos db API.
        /// </summary>
        public static ServiceOperationApi CosmosDbOperationApi = new ServiceOperationApi
        {
            Name = "cosmosdb",
            Id = "/serviceProviders/cosmosdb",
            Type = DesignerApiType.ServiceProvider,
            Properties = new ServiceOperationApiProperties
            {
                BrandColor = 0xC4D5FF,
                Description = "Connect to Azure Cosmos db to receive document.",
                DisplayName = "Cosmos Db",
                IconUri = new Uri("https://github.com/praveensri/LogicAppCustomConnector/blob/main/ServiceProviders.CosmosDb.Extensions/icon.png"),
                Capabilities = new ApiCapability[] { ApiCapability.Triggers },
                ConnectionParameters = new ConnectionParameters
                {
                    ConnectionString = new ConnectionStringParameters
                    {
                        Type = ConnectionStringType.SecureString,
                        ParameterSource = ConnectionParameterSource.AppConfiguration,
                        UIDefinition = new UIDefinition
                        {
                            DisplayName = "Connection String",
                            Description = "Azure Cosmos db Connection String",
                            Tooltip = "Provide Azure Cosmos db Connection String",
                            Constraints = new Constraints
                            {
                                Required = "true",
                            },
                        },
                    },
                },
            },
        };
    }
}
