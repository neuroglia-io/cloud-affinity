using CloudAffinity.Application.Configuration;
using CloudAffinity.Application.Services;
using Hylo.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CloudAffinity.Application;

/// <summary>
/// Defines extensions for <see cref="IServiceCollection"/>s
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Adds and configures Cloud Affinity services
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure</param>
    /// <param name="configuration">The current <see cref="IConfiguration"/></param>
    /// <returns>The configured <see cref="IServiceCollection"/></returns>
    public static IServiceCollection AddCloudAffinity(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new CorrelatorOptions();
        configuration.Bind(options);

        services.AddHttpClient();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ICloudEventStream, CloudEventStream>();
        services.AddSingleton<IExpressionEvaluator, JQExpressionEvaluator>();
        services.AddSingleton<CorrelationResourceManager>();
        services.AddSingleton<IResourceController<Correlation>>(provider => provider.GetRequiredService<CorrelationResourceManager>());
        services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<CorrelationResourceManager>());
        return services;
    }

}