using CloudAffinity.Application.Configuration;
using Hylo;
using Hylo.Infrastructure.Configuration;
using Hylo.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Reactive.Linq;

namespace CloudAffinity.Application.Services;

/// <summary>
/// Represents the service used to manage <see cref="Correlation"/> resources
/// </summary>
public class CorrelationResourceManager
    : ResourceController<Correlation>
{

    /// <inheritdoc/>
    public CorrelationResourceManager(IServiceProvider serviceProvider, ILoggerFactory loggerFactory, IOptions<ResourceControllerOptions<Correlation>> controllerOptions, IRepository repository, IOptions<CorrelatorOptions> correlatorOptions)
        : base(loggerFactory, controllerOptions, repository)
    {
        this.ServiceProvider = serviceProvider;
        this.CorrelatorOptions = correlatorOptions.Value;
    }

    /// <summary>
    /// Gets the current <see cref="IServiceProvider"/>
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the running <see cref="Data.Correlator"/>'s options
    /// </summary>
    protected CorrelatorOptions CorrelatorOptions { get; }

    /// <summary>
    /// Gets the service used to monitor the current <see cref="Data.Correlator"/>
    /// </summary>
    protected IResourceMonitor<Correlator>? Correlator { get; private set; }

    /// <summary>
    /// Gets a <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the key/value mappings of handled <see cref="Correlation"/>s
    /// </summary>
    protected ConcurrentDictionary<string, CorrelationHandler> CorrelationHandlers { get; } = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken).ConfigureAwait(false);
        Data.Correlator? correlator = null;
        try
        {
            correlator = await this.Repository.GetAsync<Correlator>(this.CorrelatorOptions.Name, this.CorrelatorOptions.Namespace, cancellationToken).ConfigureAwait(false);
        }
        catch (HyloException ex) when (ex.Problem.Status == (int)HttpStatusCode.NotFound) { }
        finally
        {
            if (correlator == null)
            {
                correlator = new Correlator(new ResourceMetadata(this.CorrelatorOptions.Name, this.CorrelatorOptions.Namespace), new CorrelatorSpec());
                correlator = await this.Repository.AddAsync(correlator, false, cancellationToken).ConfigureAwait(false);
            }
            this.Correlator = await this.Repository.MonitorAsync<Correlator>(this.CorrelatorOptions.Name, this.CorrelatorOptions.Namespace, false, cancellationToken).ConfigureAwait(false);
        }
        foreach (var correlation in this.Resources.Values.ToList())
        {
            await this.OnCorrelationCreatedAsync(correlation).ConfigureAwait(false);
        }
        this.Where(e => e.Type == ResourceWatchEventType.Created).Select(e => e.Resource).SubscribeAsync(this.OnCorrelationCreatedAsync, cancellationToken);
        this.Where(e => e.Type == ResourceWatchEventType.Updated).Select(s => s.Resource).DistinctUntilChanged(s => s.Metadata.Labels).SubscribeAsync(this.OnCorrelationLabelChangedAsync, cancellationToken);
        this.Where(e => e.Type == ResourceWatchEventType.Deleted).Select(e => e.Resource).SubscribeAsync(this.OnCorrelationDeletedAsync, cancellationToken);
    }

    /// <summary>
    /// Builds a new cache key for the specified resource
    /// </summary>
    /// <param name="name">The name of the resource to build a new cache key for</param>
    /// <param name="namespace">The namespace the resource to build a new cache key for belongs to</param>
    /// <returns>A new cache key</returns>
    protected virtual string GetCorrelationHandlerCacheKey(string name, string? @namespace) => string.IsNullOrWhiteSpace(@namespace) ? name : $"{@namespace}.{name}";

    /// <summary>
    /// Handles changes to the specified correlation's labels
    /// </summary>
    /// <param name="correlation">The correlation to manage the changes of</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task OnCorrelationLabelChangedAsync(Correlation correlation)
    {
        if (this.Correlator == null) return;
        var key = this.GetCorrelationHandlerCacheKey(correlation.GetName(), correlation.GetNamespace());
        if (this.Options.LabelSelectors == null || this.Options.LabelSelectors.All(s => s.Selects(correlation)) == true)
        {
            if (this.CorrelationHandlers.TryGetValue(key, out _)) return;
            await this.OnCorrelationCreatedAsync(correlation).ConfigureAwait(false);
        }
        else
        {
            if (!this.CorrelationHandlers.TryGetValue(key, out _)) return;
            await this.OnResourceDeletedAsync(correlation).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnResourceDeletedAsync(Correlation correlation, CancellationToken cancellationToken = default)
    {
        await base.OnResourceDeletedAsync(correlation, cancellationToken).ConfigureAwait(false);
        var key = this.GetCorrelationHandlerCacheKey(correlation.GetName(), correlation.GetNamespace());
        if (!this.CorrelationHandlers.TryRemove(key, out var handler) || handler == null) return;
        await handler.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Handles the creation of a new <see cref="Correlation"/>
    /// </summary>
    /// <param name="correlation">The newly created <see cref="Correlation"/></param>
    protected virtual async Task OnCorrelationCreatedAsync(Correlation correlation)
    {
        var key = this.GetCorrelationHandlerCacheKey(correlation.GetName(), correlation.GetNamespace());
        var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this.ServiceProvider, correlation);
        await handler.InitializeAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
        this.CorrelationHandlers.AddOrUpdate(key, handler, (_, _) => handler);
    }

    /// <summary>
    /// Handles the deletion of a new <see cref="Correlation"/>
    /// </summary>
    /// <param name="correlation">The newly deleted <see cref="Correlation"/></param>
    protected virtual async Task OnCorrelationDeletedAsync(Correlation correlation)
    {
        var key = this.GetCorrelationHandlerCacheKey(correlation.GetName(), correlation.GetNamespace());
        if (this.CorrelationHandlers.Remove(key, out var handler) && handler != null) await handler.DisposeAsync().ConfigureAwait(false);
    }

}
