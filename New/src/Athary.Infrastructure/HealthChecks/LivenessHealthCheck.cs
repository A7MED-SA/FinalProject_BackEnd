using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Athary.Infrastructure.HealthChecks;

public sealed class LivenessHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthCheckResult.Healthy("Service is alive"));
    }
}
