using IpRegistry.Services;
using MetOfficeDataPoint.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DockerTest.Features.HealthCheck
{
    public class IpRegistryHealthCheck : IHealthCheck
    {
        private readonly IIpRegistryService _ipRegistryService;

        public IpRegistryHealthCheck(IIpRegistryService ipRegistryService)
        {
            _ipRegistryService = ipRegistryService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _ipRegistryService.GetIpAddressDetailsAsync();
                if (response != null)
                {
                    return HealthCheckResult.Healthy("IP Registry API available");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("IP Registry API not returning data");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("IP Registry API connection failed");
            }
        }
    }
}
