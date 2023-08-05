using Hylo;
using Hylo.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace CloudAffinity.Application.Services;

/// <summary>
/// Represents the service used to seed the Cloud Affinity resource database
/// </summary>
public class DatabaseInitializer
    : Hylo.Infrastructure.Services.DatabaseInitializer
{

    /// <inheritdoc/>
    public DatabaseInitializer(ILoggerFactory loggerFactory, IDatabaseProvider databaseProvider) : base(loggerFactory, databaseProvider) { }

    /// <inheritdoc/>
    protected override async Task SeedAsync(CancellationToken cancellationToken)
    {
        await base.SeedAsync(cancellationToken).ConfigureAwait(false);
        await this.SeedResourceDefinitionsAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Seeds the definitions of the resources used by CloudStreams
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task SeedResourceDefinitionsAsync(CancellationToken cancellationToken)
    {
        foreach (var definition in CloudAffinityDefaults.Resources.Definitions.AsEnumerable())
        {
            await this.DatabaseProvider.GetDatabase().CreateResourceAsync(definition, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

}
