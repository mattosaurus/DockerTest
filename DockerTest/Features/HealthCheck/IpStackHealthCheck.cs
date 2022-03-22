using IpStack.Services;
using MetOfficeDataPoint.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DockerTest.Features.HealthCheck
{
    public class IpStackHealthCheck : IHealthCheck
    {
        private readonly IIpStackService _ipStackService;

        public IpStackHealthCheck(IIpStackService ipStackService)
        {
            _ipStackService = ipStackService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _ipStackService.GetIpAddressDetailsAsync();
                if (response != null)
                {
                    return HealthCheckResult.Healthy("IP Stack API available");
                }
                else
                {
                    return HealthCheckResult.Unhealthy("IP Stack API not returning data");
                }
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("IP Stack API connection failed");
            }
        }
    }
}
