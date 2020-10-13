using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;

[assembly: Microsoft.Azure.WebJobs.Hosting.WebJobsStartup(typeof(ServiceProviders.CosmosDb.Extensions.CosmosDbTriggerStartup))]

namespace ServiceProviders.CosmosDb.Extensions
{
    public class CosmosDbTriggerStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<CosmosDbServiceProvider>();
            builder.Services.TryAddSingleton<CosmosDbTriggerServiceOperationProvider>();
        }
    }
}
