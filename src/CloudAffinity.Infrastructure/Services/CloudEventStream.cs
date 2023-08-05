using System.Reactive.Subjects;
using CloudAffinity.Data;

namespace CloudAffinity.Infrastructure.Services;

/// <summary>
/// Represents the default implementation of the <see cref="ICloudEventStream"/> interface
/// </summary>
public class CloudEventStream
    : ICloudEventStream
{

    /// <summary>
    /// Initializes a new <see cref="CloudEventStream"/>
    /// </summary>
    public CloudEventStream()
    {
        this.Subject = new Subject<CloudEvent>();
    }

    /// <summary>
    /// Gets the <see cref="CloudEventStream"/>'s underlying <see cref="ISubject{T}"/>
    /// </summary>
    protected ISubject<CloudEvent> Subject { get; }

    /// <inheritdoc/>
    public virtual IDisposable Subscribe(IObserver<CloudEvent> observer) => this.Subject.Subscribe(observer);

    /// <inheritdoc/>
    public virtual Task IngestAsync(CloudEvent cloudEvent, CancellationToken cancellationToken = default)
    {
        this.Subject.OnNext(cloudEvent);
        return Task.CompletedTask;
    }

}
