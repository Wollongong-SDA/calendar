using Azure.Identity;
using Microsoft.Graph;

namespace CalendarAggregator.Util
{
    public static class GraphServiceBuilder
    {
        public static GraphServiceClient GetGraphService(GraphCredentials config)
        {
            var clientSecretCredential = new ClientSecretCredential(
                config.TenantId,
                config.ClientId,
                config.ClientSecret,
                new ClientSecretCredentialOptions { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud }
            );
            return new GraphServiceClient(clientSecretCredential, ["https://graph.microsoft.com/.default"]);
        }

    }
}