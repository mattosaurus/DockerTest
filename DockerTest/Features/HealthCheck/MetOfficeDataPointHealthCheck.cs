using MetOfficeDataPoint.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DockerTest.Features.HealthCheck
{
    public class MetOfficeDataPointHealthCheck : IHealthCheck
    {
        private readonly IMetOfficeDataPointService _metOfficeDataPointService;

        public MetOfficeDataPointHealthCheck(IMetOfficeDataPointService metOfficeDataPointService)
        {
            _metOfficeDataPointService = metOfficeDataPointService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _metOfficeDataPointService.GetAvailableTimestampsAsync();
                if (response != null)
                {
                    return HealthCheckResult.Healthy("Met Office DataPoint API available");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("Met Office DataPoint API not returning data");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Met Office DataPoint API connection failed");
            }
        }
    }
}
