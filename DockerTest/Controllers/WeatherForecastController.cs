using IpRegistry;
using IpRegistry.Models;
using IpRegistry.Services;
using MetOfficeDataPoint.Models;
using MetOfficeDataPoint.Models.GeoCoordinate;
using MetOfficeDataPoint.Services;
using Microsoft.AspNetCore.Mvc;

namespace DockerTest.Controllers
{
    [ApiController]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IMetOfficeDataPointService _metOfficeDataPointService;
        private readonly IIpRegistryService _ipRegistryService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMetOfficeDataPointService metOfficeDataPointService, IIpRegistryService ipRegistryService)
        {
            _logger = logger;
            _ipRegistryService = ipRegistryService;
            _metOfficeDataPointService = metOfficeDataPointService;
        }

        [HttpGet("[controller]/")]
        public async Task<ForecastResponse3Hourly> Get()
        {
            IpAddressDetails ipAddress = await _ipRegistryService.GetIpAddressDetailsAsync();

            GeoCoordinate coordinate = new GeoCoordinate(ipAddress.Location.Latitude, ipAddress.Location.Longitude);

            MetOfficeDataPoint.Models.Location siteLocation = await _metOfficeDataPointService.GetClosestSiteAsync(coordinate);

            ForecastResponse3Hourly forecastResponse = await _metOfficeDataPointService.GetForecasts3HourlyAsync(siteLocation.ID);

            return forecastResponse;
        }
    }
}