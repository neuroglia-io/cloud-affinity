using CloudAffinity.Data;

namespace CloudAffinity.Infrastructure.Services;

/// <summary>
/// Defines the fundamentals of a stream of ingested <see cref="CloudEvent"/>s
/// </summary>
public interface ICloudEventStream
    : IObservable<CloudEvent>
{

    /// <summary>
    /// Ingest the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="e">The <see cref="CloudEvent"/> to ingest</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    Task IngestAsync(CloudEvent e, CancellationToken cancellationToken = default);

}
