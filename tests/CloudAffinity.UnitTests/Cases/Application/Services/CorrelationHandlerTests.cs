using CloudAffinity.Application.Configuration;
using CloudAffinity.Application.Services;
using CloudAffinity.Data;
using CloudAffinity.Infrastructure.Services;
using CloudAffinity.UnitTests.Services;
using FluentAssertions;
using Hylo;
using Hylo.Infrastructure;
using Hylo.Infrastructure.Services;
using Hylo.Providers.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using DatabaseInitializer = CloudAffinity.Application.Services.DatabaseInitializer;

namespace CloudAffinity.UnitTests.Cases.Application.Services;

public class CorrelationHandlerTests
    : IDisposable
{

    readonly ServiceProvider _serviceProvider;
    readonly IRepository _repository;
    readonly ICloudEventStream _cloudEventStream;
    readonly List<Resource> _toRemove = new();

    public CorrelationHandlerTests()
    {
        Environment.SetEnvironmentVariable("CLOUDAFFINITY_CORRELATOR_NAME", "test");
        var configuration = new ConfigurationBuilder().AddEnvironmentVariables(CorrelatorOptions.EnvironmentVariablePrefix).Build();
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddHylo(configuration, builder =>
        {
            builder
                .UseDatabaseInitializer<DatabaseInitializer>()
                .UseRedis("localhost");
        });
        var options = new CorrelatorOptions();
        configuration.Bind(options);

        services.AddHttpClient();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ICloudEventStream, CloudEventStream>();
        services.AddSingleton<IExpressionEvaluator, JQExpressionEvaluator>();
        services.AddResourceController<Correlation>();
        this._serviceProvider = services.BuildServiceProvider();
        foreach(var hostedService in this._serviceProvider.GetServices<IHostedService>())
        {
            hostedService.StartAsync(default).GetAwaiter().GetResult();
        }
        this._repository = this._serviceProvider.GetRequiredService<IRepository>();
        this._cloudEventStream = this._serviceProvider.GetRequiredService<ICloudEventStream>();
    }

    [Fact]
    public virtual async Task Correlate_SingleEvent_WithSingleOccurence_Should_Work()
    {
        //arrange
        var cloudEvent = CloudEventFactory.Create();
        var correlationCriteria = new CorrelationCriterion[]
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, cloudEvent)
        };
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Single), FulfillmentCondition.Any, correlationCriteria));
        this._toRemove.Add(correlation);
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        await this._cloudEventStream.IngestAsync(cloudEvent).ConfigureAwait(false);
        await Task.Delay(100);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation!.Status!.Phase.Should().Be(CorrelationStatusPhase.Fulfilled);
        correlation.Status!.Occurences.Should().ContainSingle();
        correlation.Status!.Occurences!.First().Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled);
    }

    [Fact]
    public virtual async Task Correlate_SingleEvent_WithMultipleOccurence_Should_Work()
    {
        //arrange
        var cloudEvent = CloudEventFactory.Create();
        var correlationCriteria = new CorrelationCriterion[]
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, cloudEvent)
        };
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Multiple), FulfillmentCondition.Any, correlationCriteria));
        this._toRemove.Add(correlation);
        var cloudEventStream = this._serviceProvider.GetRequiredService<ICloudEventStream>();
        var occurenceCount = 3;
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        for(int i = 0; i < occurenceCount; i++)
        {
            await cloudEventStream.IngestAsync(cloudEvent).ConfigureAwait(false);
        }

        await Task.Delay(250);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation.Status!.Occurences.Should().HaveCount(occurenceCount);
        correlation.Status!.Occurences!.Should().AllSatisfy(o => o.Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled));
    }

    [Fact]
    public virtual async Task Correlate_MultipleEvents_WithSingleOccurence_Should_Work()
    {
        //arrange
        var event1 = CloudEventFactory.Create();
        var event2 = CloudEventFactory.Create();
        event2.Subject = event1.Subject;
        var correlationCriteria = new CorrelationCriterion[]
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event1),
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event2)
        };
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Single), FulfillmentCondition.All, correlationCriteria));
        this._toRemove.Add(correlation);
        var cloudEventStream = this._serviceProvider.GetRequiredService<ICloudEventStream>();
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        await cloudEventStream.IngestAsync(event1).ConfigureAwait(false);
        await cloudEventStream.IngestAsync(event2).ConfigureAwait(false);
        await Task.Delay(250);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation!.Status!.Phase.Should().Be(CorrelationStatusPhase.Fulfilled);
        correlation.Status!.Occurences.Should().ContainSingle();
        correlation.Status!.Occurences!.First().Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled);
        correlation.Status!.Occurences!.First().Events.Should().HaveCount(2);
    }

    [Fact]
    public virtual async Task Correlate_MultipleEvents_WithSingleOccurence_FilteredByExpression_Should_Work()
    {
        //arrange
        var event1 = CloudEventFactory.Create() with 
        { 
            Data = new
            {
                foo = "bar"
            }
        };
        var event2 = CloudEventFactory.Create();
        event2.Subject = event1.Subject;
        var correlationCriteria = new CorrelationCriterion[]
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event1),
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event2)
        };
        correlationCriteria.Last().Rules.First().Correlation.Condition = $$"""${ $OCCURENCE.events | last | .event.data.foo == "bar" }""";
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Single), FulfillmentCondition.All, correlationCriteria));
        this._toRemove.Add(correlation);
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        await this._cloudEventStream.IngestAsync(event1).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(event2).ConfigureAwait(false);
        await Task.Delay(100);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation!.Status!.Phase.Should().Be(CorrelationStatusPhase.Fulfilled);
        correlation.Status!.Occurences.Should().ContainSingle();
        correlation.Status!.Occurences!.First().Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled);
        correlation.Status!.Occurences!.First().Events.Should().HaveCount(2);
    }

    [Fact]
    public virtual async Task Correlate_MultipleEvents_WithMultipleOccurence_Should_Work()
    {
        //arrange
        var subject1 = Guid.NewGuid().ToShortString();
        var subject2 = Guid.NewGuid().ToShortString();
        var event1 = CloudEventFactory.Create(subject1);
        var event2 = CloudEventFactory.Create(subject2);
        var event3 = CloudEventFactory.Create(subject1);
        var event4 = CloudEventFactory.Create(subject2);
        var correlationCriteria = new CorrelationCriterion[]
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event1, event3),
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event2, event4)
        };
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Multiple), FulfillmentCondition.Any, correlationCriteria));
        this._toRemove.Add(correlation);
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        await this._cloudEventStream.IngestAsync(event1).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(event2).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(event3).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(event4).ConfigureAwait(false);
        await Task.Delay(100);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation!.Status!.Phase.Should().Be(CorrelationStatusPhase.Correlating);
        correlation.Status!.Occurences.Should().HaveCount(2);
        correlation.Status!.Occurences!.Should().AllSatisfy(o => o.Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled));
        correlation.Status!.Occurences!.Should().AllSatisfy(o => o.Events.Should().HaveCount(2));
    }

    [Fact]
    public virtual async Task Correlate_AllCriterion_WithAnyEvents_Should_Work()
    {
        //arrange
        var subject = Guid.NewGuid().ToShortString();
        var event1 = CloudEventFactory.Create(subject);
        var event2 = CloudEventFactory.Create(subject);
        var event3 = CloudEventFactory.Create(subject);
        var correlationCriteria = new CorrelationCriterion[]
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.Any, event1, event2),
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, event3)
        };
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Single), FulfillmentCondition.All, correlationCriteria));
        this._toRemove.Add(correlation);
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        await this._cloudEventStream.IngestAsync(event1).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(event2).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(event3).ConfigureAwait(false);
        await Task.Delay(100);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation!.Status!.Phase.Should().Be(CorrelationStatusPhase.Fulfilled);
        correlation.Status!.Occurences.Should().ContainSingle();
        correlation.Status!.Occurences!.First().Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled);
        correlation.Status!.Occurences!.First().Events.Should().HaveCount(3);
    }

    [Fact]
    public virtual async Task Correlate_MultipleEvents_WithLimitedOccurenceParallelism_Should_Work()
    {
        //arrange
        var correlationKey = Guid.NewGuid().ToShortString();
        var criterion1Event1 = CloudEventFactory.Create(correlationKey);
        var criterion1Event2 = CloudEventFactory.Create(correlationKey);
        var criterion2Event1 = CloudEventFactory.Create(correlationKey);
        var criterion2Event2 = CloudEventFactory.Create(correlationKey);
        var criteria = new List<CorrelationCriterion>()
        {
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, criterion1Event1, criterion1Event2),
            CorrelationCriterionFactory.Create(FulfillmentCondition.All, criterion2Event1, criterion2Event2)
        };
        var correlation = new Correlation(new("fake-correlation"), new(new(CorrelationOccurenceMode.Multiple) { Parallelism = 1 }, FulfillmentCondition.Any, criteria));
        this._toRemove.Add(correlation);
        await this._repository.AddAsync(correlation).ConfigureAwait(false);
        await using var handler = ActivatorUtilities.CreateInstance<CorrelationHandler>(this._serviceProvider, correlation);
        await handler.InitializeAsync(default).ConfigureAwait(false);

        //act
        await this._cloudEventStream.IngestAsync(criterion1Event1).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(criterion1Event1).ConfigureAwait(false);
        await this._cloudEventStream.IngestAsync(criterion1Event2).ConfigureAwait(false);
        await Task.Delay(500);
        correlation = await this._repository.GetAsync<Correlation>(correlation.GetName()).ConfigureAwait(false);

        //assert
        correlation.Should().NotBeNull();
        correlation!.Status.Should().NotBeNull();
        correlation!.Status!.Phase.Should().Be(CorrelationStatusPhase.Correlating);
        correlation.Status!.Occurences.Should().ContainSingle();
        correlation.Status!.Occurences!.First().Phase.Should().Be(CorrelationOccurenceStatusPhase.Fulfilled);
        correlation.Status!.Occurences!.First().Events.Should().HaveCount(2);

        var yaml = Serializer.Yaml.Serialize(Serializer.Json.Deserialize<IDictionary<string, object>>(Serializer.Json.Serialize(correlation)));
    }

    void IDisposable.Dispose()
    {
        if (this._repository != null)
        {
            foreach (var resource in this._toRemove)
            {
                try
                {
                    this._repository.RemoveAsync(resource.Definition.Group, resource.Definition.Version, resource.Definition.Plural, resource.GetName()).GetAwaiter().GetResult();
                }
                catch { }
            }
        }

        this._serviceProvider?.Dispose();

        GC.SuppressFinalize(this);
    }

}
