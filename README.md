# LogicAppCustomConnector
Azure Logic App Custom Built in connectors
Creating Custom Built-in connector for Logic App V2
The extension model of service provider connector for Logic App V2 enables you to add the custom connector without any changes in extension or logic app designer. This blog describes how to create the built-in connectors.  

Azure Cosmos DB Custom connector: Here in this sample I am creating the custom connector with Cosmos DB trigger , whenever the new document is added in the lease collection or container the trigger will fire and execute the Logic App with the input payload as Cosmos document which is added in the container. We need to implement the service provider interface and register as Azure function extension which are explained as:
Prerequisite:
Here we are creating the .Net Core 3.1 class library and add the Microsoft.Azure.Workflows.Webjobs.Extension Nuget package in your implementation. Once you install the Logic App extension, it installs the package locally in you Nuget cache path.
Service Provider Interface Implementation: 
In case the custom connector does not have any trigger and only actions are required then one needs to implement the interface IServiceOperationsProvider.

    /// <summary>
    /// Interface for service operations provider.
    /// </summary>
    public interface IServiceOperationsProvider
    {
        /// <summary>
        /// Gets the service.
        /// </summary>
        ServiceOperationApi GetService();

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="connectionParameters">The connection parameters.</param>
        string GetBindingConnectionInformation(string operationId, InsensitiveDictionary<JToken> connectionParameters);

        /// <summary>
        /// Gets operations supported by a service.
        /// </summary>
        /// <param name="expandManifest">Boolean to expand the result with manifest.</param>
        IEnumerable<ServiceOperation> GetOperations(bool expandManifest);

        /// <summary>
        /// Invokes a specific operation on the service.
        /// </summary>
        /// <param name="operationId">The operation id.</param>
        /// <param name="connectionParameters">The connection parameters.</param>
        /// <param name="serviceOperationRequest">The service operation request.</param>
        Task<ServiceOperationResponse> InvokeActionOperation(string operationId, InsensitiveDictionary<JToken> connectionParameters, ServiceOperationRequest serviceOperationRequest);
    }

In case the custom connector consists of trigger only or trigger with other actions, we need to implement the following interface.
    
    /// <summary>
    /// Service operations trigger provider extension.
    /// </summary>
    public interface IServiceOperationsTriggerProvider : IServiceOperationsProvider
    {
        /// <summary>
        /// Get trigger type.
        /// </summary>
        string GetFunctionTriggerType();
    }

Function Extensions and registration:
•	Create Startup job: To register the custom connector as function extension , you need to create a startup class using [assembly:WebJobsStartup] assembly attribute and implementing IWebJobsStartup interface, refer the function registration for more details. In the configure method you need to register the extension and inject the custom service provider as shown below:

    public class CosmosDbTriggerStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // Registering and extension
            builder.AddExtension<CosmosDbServiceProvider>();

            // DI the trigger service operation provider.
            builder.Services.TryAddSingleton<CosmosDbTriggerServiceOperationProvider>();
        }
    } 
•	Register service provider:  We need to register the service provider implementation as function extension. We are using the built-in Azure function trigger Cosmos DB Trigger  here in this example we register the new Cosmos DB service provider to existing list of service providers which are already part of Logic App V2 extension.


    [Extension("CosmosDbServiceProvider", configurationSection: "CosmosDbServiceProvider")]
    public class CosmosDbServiceProvider : IExtensionConfigProvider
    {        

        public CosmosDbServiceProvider(ServiceOperationsProvider serviceOperationsProvider,
            CosmosDbTriggerServiceOperationProvider operationsProvider)
        {
            serviceOperationsProvider.RegisterService(ServiceName, ServiceId, operationsProvider);
        }
        
        public void Initialize(ExtensionConfigContext context)
        {
            // Converts Cosmos Document list to JObject array.
            context.AddConverter<IReadOnlyList<Document>, JObject[]>(ConvertDocumentToJObject);
        }

        public static JObject[] ConvertDocumentToJObject(IReadOnlyList<Document> data)
        {
            List<JObject> jobjects = new List<JObject>();
            foreach(var doc in data)
            {
                jobjects.Add((JObject)doc.ToJToken());
            }
            return jobjects.ToArray();
        }
    }
    
•	Add Converter: Logic app V2 has implemented the generic way to handle any trigger using the JObject array, we need to add a converter to convert the read only list of Azure Cosmos DB document into JObject array. Once the converter is ready as shown in above example, we need to register the convert as part of ExtensionConfigContext.

    // Converts Cosmos Document list to JObject array.
    context.AddConverter<IReadOnlyList<Document>, JObject[]>(ConvertDocumentToJObject);

