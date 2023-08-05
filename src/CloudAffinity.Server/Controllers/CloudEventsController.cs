using CloudAffinity.Data;
using CloudAffinity.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace CloudAffinity.Server.Controllers;

/// <summary>
/// Represents the <see cref="ControllerBase"/> used to manage <see cref="CloudEvent"/>s
/// </summary>
[Route("events")]
public class CloudEventsController 
    : ControllerBase
{

    /// <summary>
    /// Initializes a new <see cref="CloudEventsController"/>
    /// </summary>
    /// <param name="cloudEventStream">The current <see cref="ICloudEventStream"/></param>
    public CloudEventsController(ICloudEventStream cloudEventStream)
    {
        this.CloudEventStream = cloudEventStream;
    }

    /// <summary>
    /// Gets the current <see cref="ICloudEventStream"/>
    /// </summary>
    protected ICloudEventStream CloudEventStream { get;}

    /// <summary>
    /// Publishes the specified cloud event to the Cloud Affinity correlator
    /// </summary>
    /// <param name="cloudEvent">The cloud event to publish</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    /// <returns>A new awaitable <see cref="IActionResult"/></returns>
    [HttpPost]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [ProducesErrorResponseType(typeof(ProblemDetails))]
    public virtual async Task<IActionResult> PublishCloudEvent([FromBody]CloudEvent cloudEvent, CancellationToken cancellationToken)
    {
        await this.CloudEventStream.IngestAsync(cloudEvent, cancellationToken).ConfigureAwait(false);
        return this.NoContent();
    }

}
