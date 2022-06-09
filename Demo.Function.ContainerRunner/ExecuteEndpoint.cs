using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;

namespace Demo.Function.ContainerRunner
{
    public static class ExecuteEndpoint
    {
        [FunctionName("ExecuteEndpoint")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req, 
            ExecutionContext context,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                                .SetBasePath(context.FunctionAppDirectory)
                                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                .AddEnvironmentVariables()
                                .Build();

            var creds = new AzureCredentialsFactory().FromServicePrincipal(config["ClientId"], config["ClientSecret"], config["TenantId"], AzureEnvironment.AzureGlobalCloud);
            var azure = Azure.Authenticate(creds).WithSubscription(config["SubscriptionId"]);

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<ContainerRunRequest>(requestBody);


            // Create the container group
            var containerGroup = azure.ContainerGroups.Define(request.ContainerGroupName)
                .WithRegion(request.Region)
                .WithExistingResourceGroup(request.ResourceGroupName)
                .WithLinux()
                .WithPublicImageRegistryOnly()
                .WithoutVolume()
                .DefineContainerInstance(request.ContainerGroupName + "-1")
                    .WithImage(request.ContainerImage)
                    .WithExternalTcpPort(80)
                    .WithCpuCoreCount(1.0)
                    .WithMemorySizeInGB(1)
                    .Attach()
                .WithDnsPrefix(request.ContainerGroupName)
                .WithRestartPolicy(ContainerGroupRestartPolicy.Never)
                .Create();


            var logs = containerGroup.GetLogContent(request.ContainerGroupName + "-1");


            return new OkObjectResult(logs);
        }
    }
}
