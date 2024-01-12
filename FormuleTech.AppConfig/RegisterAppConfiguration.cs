namespace Chroneos;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public static class RegisterAppConfiguration
{
    public static void AddAppConfiguration(this WebApplicationBuilder builder)
    {

        if (IsAzureEnvironment())
        {
            System.Diagnostics.Trace.TraceError(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                var kvClient = GetKeyVaultConnection(builder);
                string appConfigConnectionString = kvClient.GetSecret("AppConfigurationConnectionString").Value.Value;

                options.Connect(appConfigConnectionString);
                options.Select("*", GetAppConfigurationLabel());
                options.ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                    kv.SetSecretRefreshInterval(TimeSpan.FromSeconds(60));
                });

                options.ConfigureRefresh(x =>
                {
                    x.Register("*", GetAppConfigurationLabel(), true);
                    x.SetCacheExpiration(TimeSpan.FromMinutes(5));
                });
            });
        }
    }

    private static SecretClient GetKeyVaultConnection(WebApplicationBuilder builder)
    {
        var options = new SecretClientOptions()
        {
            Retry =
                    {
                        Delay = TimeSpan.FromSeconds(2),
                        MaxDelay = TimeSpan.FromSeconds(16),
                        MaxRetries = 5,
                        Mode = Azure.Core.RetryMode.Exponential
                    }
        };
        var config = builder.Configuration;

        return new SecretClient(new Uri($"https://{config["KEY_VAULT"]}.vault.azure.net"), new DefaultAzureCredential(), options);
    }

    private static bool IsAzureEnvironment()
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        return string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase)
                || string.Equals(environment, "Recette", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetAppConfigurationLabel()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    }
}
