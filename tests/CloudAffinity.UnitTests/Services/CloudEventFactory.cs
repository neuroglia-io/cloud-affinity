using CloudAffinity.Data;
using Hylo;
using System.Net.Mime;

namespace CloudAffinity.UnitTests.Services;

public static class CloudEventFactory
{

    public static CloudEvent Create(string? subject = null)
    {
        if (string.IsNullOrWhiteSpace(subject)) subject = Guid.NewGuid().ToShortString();
        return new CloudEvent()
        {
            Id = Guid.NewGuid().ToShortString(),
            Time = DateTimeOffset.Now,
            Source = new Uri($"https://unit-tests.cloud-affinity.io/{Guid.NewGuid().ToShortString()}"),
            Type = $"io.cloud-affinity.unit-tests/{Guid.NewGuid().ToShortString()}",
            Subject = subject,
            DataContentType = MediaTypeNames.Application.Json,
            Data = new { }
        };
    }

}
