using IpStack;
using IpStack.Services;
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
        private readonly IIpStackService _ipStackService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IMetOfficeDataPointService metOfficeDataPointService, IIpStackService ipStackService)
        {
            _logger = logger;
            _ipStackService = ipStackService;
            _metOfficeDataPointService = metOfficeDataPointService;
        }

        [HttpGet("[controller]/")]
        public async Task<ForecastResponse3Hourly> Get()
        {
            IpStack.Models.IpAddressDetails ipLocation = await _ipStackService.GetIpAddressDetailsAsync();

            GeoCoordinate coordinate = new GeoCoordinate(ipLocation.Latitude, ipLocation.Longitude);

            Location siteLocation = await _metOfficeDataPointService.GetClosestSiteAsync(coordinate);

            ForecastResponse3Hourly forecastResponse = await _metOfficeDataPointService.GetForecasts3HourlyAsync(siteLocation.ID);

            return forecastResponse;
        }
    }
}