using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace CalendarAggregator.Util
{
    public sealed class GraphCredentials
    {
        public required string TenantId { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }

        public static GraphCredentials Get(IConfiguration config, string path)
        {
            return config.GetRequiredSection("GraphCredentials").GetRequiredSection(path).Get<GraphCredentials>() ??
                throw new InvalidConfigurationException("Invalid Ms365GroupCred");
        }
    }
}