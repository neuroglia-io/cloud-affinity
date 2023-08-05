using Hylo;
using Hylo.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Threading.Channels;

namespace CloudAffinity.Application.Services;

/// <summary>
/// Represents a service used to handler a specific correlation
/// </summary>
public class CorrelationHandler
    : IAsyncDisposable
{

    private IDisposable? _subscription;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="CorrelationHandler"/>
    /// </summary>
    /// <param name="loggerFactory">The service used to create <see cref="ILogger"/>s</param>
    /// <param name="repository">The service used to manage resources</param>
    /// <param name="correlationController">The service used to manage <see cref="Data.Correlation"/> resources</param>
    /// <param name="cloudEventStream">The service used to stream <see cref="CloudEvent"/>s</param>
    /// <param name="expressionEvaluator">The service used to evaluate runtime expressions</param>
    /// <param name="correlation">The <see cref="Data.Correlation"/> to handle</param>
    public CorrelationHandler(ILoggerFactory loggerFactory, IRepository repository, IResourceController<Correlation> correlationController, ICloudEventStream cloudEventStream, IExpressionEvaluator expressionEvaluator, Correlation correlation)
    {
        this.Logger = loggerFactory.CreateLogger(this.GetType());
        this.Repository = repository;
        this.CorrelationController = correlationController;
        this.CloudEventStream = cloudEventStream;
        this.ExpressionEvaluator = expressionEvaluator;
        this.Correlation = correlation;
    }

    /// <summary>
    /// Gets the service used to perform logging
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// Gets the service used to manage resources
    /// </summary>
    protected IRepository Repository { get; }

    /// <summary>
    /// Gets the service used to manage <see cref="Data.Correlation"/> resources
    /// </summary>
    protected IResourceController<Correlation> CorrelationController { get; }

    /// <summary>
    /// Gets the service used to stream <see cref="CloudEvent"/>s
    /// </summary>
    protected ICloudEventStream CloudEventStream { get; }

    /// <summary>
    /// Gets the service used to evaluate runtime expressions
    /// </summary>
    protected IExpressionEvaluator ExpressionEvaluator { get; }

    /// <summary>
    /// Gets the <see cref="Data.Correlation"/> to handle
    /// </summary>
    protected Correlation Correlation { get; private set; }

    /// <summary>
    /// Gets the <see cref="CorrelationHandler"/>'s <see cref="System.Threading.CancellationTokenSource"/>
    /// </summary>
    protected CancellationTokenSource CancellationTokenSource { get; private set; } = null!;

    /// <summary>
    /// Gets the <see cref="CorrelationHandler"/>'s <see cref="System.Threading.CancellationToken"/>
    /// </summary>
    protected CancellationToken CancellationToken => this.CancellationTokenSource.Token;

    /// <summary>
    /// Gets the <see cref="Timer"/> used to expire the managed <see cref="Data.Correlation"/> at the configured date and time, if any
    /// </summary>
    protected Timer? CorrelationTimer { get; private set; }

    /// <summary>
    /// Gets the <see cref="Channel{T}"/> used to queue and process cloud events asynchronously, in a FIFO fashion
    /// </summary>
    protected Channel<CloudEvent> CloudEventChannel { get; } = Channel.CreateUnbounded<CloudEvent>();

    /// <summary>
    /// Initializes the <see cref="CorrelationHandler"/>
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task InitializeAsync(CancellationToken cancellationToken)
    {
        this.CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        this.CorrelationController.Where(e => e.Type == ResourceWatchEventType.Updated && e.Resource.GetName() == this.Correlation.GetName() && e.Resource.GetNamespace() == this.Correlation.GetNamespace()).Select(e => e.Resource)
           .Select(correlation =>
           {
               this.Correlation = correlation;
               return correlation;
           })
           .Subscribe(this.CancellationToken);
        await this.SetCorrelationStatusPhaseAsync(CorrelationStatusPhase.Correlating).ConfigureAwait(false);
        this._subscription = this.CloudEventStream.SubscribeAsync(this.OnCloudEventAsync, onErrorAsync: this.OnCorrelationErrorAsync, null);
        _ = this.PerformCorrelationAsync();
    }

    /// <summary>
    /// Starts the <see cref="CorrelationHandler"/>'s correlation loop by polling the <see cref="CloudEventChannel"/> for events to process
    /// </summary>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task PerformCorrelationAsync()
    {
        do
        {
            try
            {
                var cloudEvent = await this.CloudEventChannel.Reader.ReadAsync(this.CancellationTokenSource.Token).ConfigureAwait(false);
                if(cloudEvent == null)
                {
                    await Task.Delay(10);
                    continue;
                }
                try
                {
                    await this.CorrelateAsync(cloudEvent).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError("An error occured while attempting to correlate cloud event with id '{eventId}': {ex}", cloudEvent.Id, ex);
                }
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                break;
            }
        }
        while (!this.CancellationToken.IsCancellationRequested);
    }

    /// <summary>
    /// Correlates the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="cloudEvent">The <see cref="CloudEvent"/> to correlate</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task CorrelateAsync(CloudEvent cloudEvent)
    {
        var rulesPerCriteria = this.Correlation.Spec.Criteria.GroupBy(c => c, c => c.Rules.Where(r => r.Matches(cloudEvent, this.ExpressionEvaluator))).Where(g => g.Any(r => r.Any()));
        if (!rulesPerCriteria.Any()) return;
        CorrelationOccurence? occurence = null;
        foreach (var rulesPerCriterion in rulesPerCriteria)
        {
            var criterion = rulesPerCriterion.Key;
            foreach (var rule in rulesPerCriterion.First())
            {
                var correlationResult = CorrelationResult.Unrelated;
                var correlationOccurences = this.Correlation.Status?.Occurences?.Where(cc => cc.Phase == CorrelationOccurenceStatusPhase.Correlating).Select(c => c.Clone()!).ToList();
                if (correlationOccurences?.Any() == true)
                {
                    for (int i = 0; i < correlationOccurences.Count; i++)
                    {
                        occurence = correlationOccurences.ElementAt(i);
                        correlationResult = occurence.TryCorrelate(cloudEvent, criterion, rule, this.ExpressionEvaluator);
                        if(correlationResult != CorrelationResult.Unrelated) await this.AddOrUpdateCorrelationOccurenceAsync(occurence).ConfigureAwait(false);
                        if (correlationResult != CorrelationResult.Unrelated) break;
                    }
                }
                switch (correlationResult)
                {
                    case CorrelationResult.Deduplicated:
                        this.Logger.LogInformation("The cloud event with id '{eventId}' has been deduplicated by occurence with id '{occurenceId}' based on rule '{criterion}/{criterionRule}'", cloudEvent.Id, occurence!.Id, criterion.Name, rule.Name);
                        break;
                    case CorrelationResult.Correlated:
                        this.Logger.LogInformation("The cloud event with id '{eventId}' has been successfully correlated to occurence with id '{occurenceId}' based on rule '{criterion}/{criterionRule}'", cloudEvent.Id, occurence!.Id, criterion.Name, rule.Name);
                        break;
                    default:
                        occurence = null;
                        break;
                }
                if (occurence != null) break;
            }
            if (occurence != null) break;
        }
        if (occurence == null)
        {

            switch (this.Correlation.Spec.Occurence.Mode)
            {
                case CorrelationOccurenceMode.Single:
                    if (this.Correlation.Status?.Occurences?.Any() == true) return;
                    break;
                case CorrelationOccurenceMode.Multiple:
                    if (this.Correlation.Spec.Occurence.Limit.HasValue && this.Correlation.Status?.Occurences?.Count >= this.Correlation.Spec.Occurence.Limit) return;
                    if (this.Correlation.Spec.Occurence.Parallelism.HasValue && this.Correlation.Status?.Occurences?.Count >= this.Correlation.Spec.Occurence.Parallelism) return;
                    break;
                default:
                    throw new NotSupportedException($"The specified {nameof(CorrelationOccurenceMode)} '{this.Correlation.Spec.Occurence.Mode}' is not supported");
            }

            this.Logger.LogDebug("Failed to find a matching occurence for the cloud event id '{cloudEvent}'. Creating a new one...", cloudEvent.Id);
            occurence = this.CreateCorrelationOccurenceFor(cloudEvent, rulesPerCriteria.First().Key, rulesPerCriteria.First().First().First());
            await this.AddOrUpdateCorrelationOccurenceAsync(occurence).ConfigureAwait(false);
            this.Logger.LogInformation("The cloud event with id '{eventId}' has been successfully correlated to occurence with id '{occurenceId}' based on rule '{criterion}/{criterionRule}'", cloudEvent.Id, occurence.Id, rulesPerCriteria.First().Key.Name, rulesPerCriteria.First().First().First().Name);
        }
        if (occurence.TryFulfill(this.Correlation))
        {
            await this.AddOrUpdateCorrelationOccurenceAsync(occurence).ConfigureAwait(false);
            this.Logger.LogInformation("Correlation occurence with id '{occurenceId}' has fulfilled the criteria of correlation '{correlationName}'", occurence.Id, this.Correlation.GetQualifiedName());
            var correlationFulfilled = this.Correlation.Spec.Occurence.Mode switch
            {
                CorrelationOccurenceMode.Single => true,
                CorrelationOccurenceMode.Multiple => this.Correlation.Spec.Occurence.Limit.HasValue && this.Correlation.Status?.Occurences?.Count == this.Correlation.Spec.Occurence.Limit,
                _ => throw new NotSupportedException($"The specified {nameof(CorrelationOccurenceMode)} '{this.Correlation.Spec.Occurence.Mode}' is not supported")
            };
            if(correlationFulfilled) await this.SetCorrelationStatusPhaseAsync(CorrelationStatusPhase.Fulfilled);
        }
        else
        {
            this.Logger.LogDebug("Correlation occurence with id '{occurenceId}' does not fulfill the criteria of correlation '{correlationName}'. Unfulfilled criteria: {criteria}", occurence.Id, this.Correlation.GetQualifiedName(), Environment.NewLine + string.Join(Environment.NewLine, this.Correlation.Spec.Criteria.Where(c => !occurence.Fulfills(c)).Select(c => $"- {c.Name}")));
        }
    }

    /// <summary>
    /// Sets the <see cref="Data.Correlation"/>'s status phase
    /// </summary>
    /// <param name="phase">The <see cref="Data.Correlation"/>'s status phase</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task SetCorrelationStatusPhaseAsync(string phase)
    {
        var resource = this.Correlation.Clone()!;
        if (resource.Status == null) resource.Status = new();
        else if (resource.Status.Phase == phase) return;
        resource.Status.Phase = phase;
        var patch = JsonPatchHelper.CreateJsonPatchFromDiff(this.Correlation, resource);
        if (!patch.Operations.Any()) return;
        this.Correlation = await this.Repository.PatchStatusAsync<Correlation>(new Patch(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, this.CancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Adds or updates the specified <see cref="CorrelationOccurence"/>
    /// </summary>
    /// <param name="occurence">The <see cref="CorrelationOccurence"/> to add or update</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual async Task AddOrUpdateCorrelationOccurenceAsync(CorrelationOccurence occurence)
    {
        var resource = this.Correlation.Clone()!;

        if (resource.Status == null) resource.Status = resource.Status = new();
        if (resource.Status.Occurences == null) resource.Status.Occurences = new();

        if(resource.Status.Occurences.Any(c => c.Id == occurence.Id) == true)
        {
            var match = resource.Status.Occurences.FirstOrDefault(c => c.Id == occurence.Id) ?? throw new NullReferenceException();
            var index = resource.Status.Occurences.IndexOf(match);
            resource.Status.Occurences.Remove(match);
            if (index >= resource.Status.Occurences.Count) resource.Status.Occurences.Add(occurence);
            else resource.Status.Occurences.Insert(index, occurence);
        }
        else
        {
            resource.Status.Occurences.Add(occurence);
        }

        var patch = JsonPatchHelper.CreateJsonPatchFromDiff(this.Correlation, resource);
        patch = new(patch.Operations.Where(o => o.Path.Segments.FirstOrDefault()?.Value == nameof(IStatus.Status).ToCamelCase()));
        if (!patch.Operations.Any()) return;

        try
        {

            this.Correlation = await this.Repository.PatchStatusAsync<Correlation>(new Patch(PatchType.JsonPatch, patch), resource.GetName(), resource.GetNamespace(), false, this.CancellationToken).ConfigureAwait(false);
        }
        catch(Exception ex)
        {
            this.Logger.LogError("An error occured while patching the status of correlation '{correlationName}': {ex}", this.Correlation.GetQualifiedName(), ex);
        }
    }

    /// <summary>
    /// Handles the specified <see cref="CloudEvent"/>
    /// </summary>
    /// <param name="cloudEvent">The <see cref="CloudEvent"/> to handle</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    public virtual async Task OnCloudEventAsync(CloudEvent cloudEvent)
    {
        await this.CloudEventChannel.Writer.WriteAsync(cloudEvent, this.CancellationToken);
    }

    /// <summary>
    /// Creates a new <see cref="CorrelationOccurence"/> for a <see cref="CloudEvent"/>, based on the specified <see cref="CorrelationCriterionRule"/>
    /// </summary>
    /// <param name="cloudEvent">The <see cref="CloudEvent"/> to create a new <see cref="CorrelationOccurence"/> for</param>
    /// <param name="criterion">The <see cref="CorrelationCriterion"/> based on which to correlate the specified <see cref="CloudEvent"/></param>
    /// <param name="criterionRule">The <see cref="CorrelationCriterionRule"/> based on which to correlate the specified <see cref="CloudEvent"/></param>
    /// <returns>A new <see cref="CorrelationOccurence"/></returns>
    protected virtual CorrelationOccurence CreateCorrelationOccurenceFor(CloudEvent cloudEvent, CorrelationCriterion criterion, CorrelationCriterionRule criterionRule)
    {
        if (cloudEvent == null) throw new ArgumentNullException(nameof(cloudEvent));
        if (criterion == null) throw new ArgumentNullException(nameof(criterion));
        if (criterionRule == null) throw new ArgumentNullException(nameof(criterionRule));
        var occurence = new CorrelationOccurence();
        if (occurence.TryCorrelate(cloudEvent, criterion, criterionRule, this.ExpressionEvaluator) == CorrelationResult.Unrelated) throw new ArgumentException($"The cloud event with id '{cloudEvent.Id}' does not satisfy the specified rule '{criterion.Name}/{criterionRule.Name}'", nameof(cloudEvent));
        return occurence;
    }

    /// <summary>
    /// Handles <see cref="Exception"/>s that have occured while observing the <see cref="Correlation"/>
    /// </summary>
    /// <param name="ex">The <see cref="Exception"/> that has been thrown</param>
    /// <returns>A new awaitable <see cref="Task"/></returns>
    protected virtual Task OnCorrelationErrorAsync(Exception ex)
    {
        this.Logger.LogError("An error occured while handling the correlation '{correlationName}': {ex}", this.Correlation.GetQualifiedName(), ex);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes of the <see cref="CorrelationHandler"/>
    /// </summary>
    /// <param name="disposing">A boolean indicating whether or not the <see cref="CorrelationHandler"/> is being disposed of</param>
    protected virtual ValueTask DisposeAsync(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                this._subscription?.Dispose();
                this.CancellationTokenSource.Dispose();
            }
            this._disposed = true;
        }
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        await this.DisposeAsync(disposing: true);
        GC.SuppressFinalize(this);
    }

}
