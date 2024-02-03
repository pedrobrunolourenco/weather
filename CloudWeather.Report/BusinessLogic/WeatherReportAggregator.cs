using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CloudWeather.Report.BusinessLogic
{
    public interface IWeatherReportAggregator
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zip"></param>
        /// <param name="days"></param>
        /// <returns></returns>
        public Task<WeatherReport> BuildReport(string zip, int? days);
    }

    public class WeatherReportAggregator : IWeatherReportAggregator
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherConfig;
        private readonly WeatherReportDbContext _db;

        public WeatherReportAggregator(IHttpClientFactory http,
                                       ILogger<WeatherReportAggregator> logger,
                                       IOptions<WeatherDataConfig> weatherConfig,
                                       WeatherReportDbContext db ) 
        {
            _http = http;
            _logger = logger;
            _weatherConfig = weatherConfig.Value;
            _db = db;
        }

        public async Task<WeatherReport> BuildReport(string zip, int? days)
        {
            var httpClient = _http.CreateClient();
            var precipData = await FetchPreciptationData(httpClient, zip, days);
            var totalSnow = GetTotalSnow(precipData);
            var totalRain = GetTotalRain(precipData);
            _logger.LogInformation(
                $"Zip: {zip} over last {days} days: " +
                $"Total Snow: {totalSnow}, rain: {totalRain}"
            );


            var tempData = await FetchTemperatureData(httpClient, zip, days);
            var averageHigthTemp = tempData.Average(t => t.TempHighF);
            var averageLowTemp = tempData.Average(t => t.TempLowF);
            _logger.LogInformation(
                $"Zip: {zip} over last {days} days: " +
                $"lo temp: {averageLowTemp}, hi temp: {averageHigthTemp}"
            );

            var weatherReport = new WeatherReport
            {
                AverageHighF = Math.Round(averageHigthTemp, 1),
                AvergeLowF = Math.Round(averageLowTemp, 1),
                RainfallTotalInches = totalRain,
                SnowTotalInches = totalSnow,
                ZipCode = zip,
                CreatedOn = DateTime.UtcNow
            };

            _db.Add(weatherReport);
            await _db.SaveChangesAsync();

            return weatherReport;
        }

        private static decimal GetTotalSnow(List<PrecipitationModel> precipData)
        {
            var totalSnow = precipData.Where(p => p.WeatherType == "snow").Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }

        private static decimal GetTotalRain(List<PrecipitationModel> precipData)
        {
            var totalRain = precipData.Where(p => p.WeatherType == "rain").Sum(p => p.AmountInches);
            return Math.Round(totalRain, 1);
        }


        private async Task<List<TemperatureModel>> FetchTemperatureData(HttpClient httpClient, string zip, int? days)
        {
            var endPoint = BuildTemperatureServiceEndPoint(zip, days);
            var temperatureRecords = await httpClient.GetAsync(endPoint);

            var jsonSerializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var temperatureData = await temperatureRecords.Content.ReadFromJsonAsync<List<TemperatureModel>>(jsonSerializeOptions);
            return temperatureData ?? new List<TemperatureModel>();

        }

        private async Task<List<PrecipitationModel>> FetchPreciptationData(HttpClient httpClient, string zip, int? days)
        {
            var endPoint = BuildPrecipitationServiceEndPoint(zip, days);
            var precipRecords = await httpClient.GetAsync(endPoint);

            var jsonSerializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var precipData = await precipRecords.Content.ReadFromJsonAsync<List<PrecipitationModel>>(jsonSerializeOptions);
            return precipData ?? new List<PrecipitationModel>();
        }

        private string BuildTemperatureServiceEndPoint(string zip, int? days)
        {
            var tempServiceProtocol = _weatherConfig.TempDataProtocol;
            var tempServiceHost = _weatherConfig.TempDataHost;
            var tempServicePort = _weatherConfig.TempDataPort;
            return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
        }

        private string BuildPrecipitationServiceEndPoint(string zip, int? days)
        {
            var precipServiceProtocol = _weatherConfig.PrecipDataProtocol;
            var precipServiceHost = _weatherConfig.PrecipDataHost;
            var precipServicePort = _weatherConfig.PrecipDataPort;
            return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
        }



    }
}
