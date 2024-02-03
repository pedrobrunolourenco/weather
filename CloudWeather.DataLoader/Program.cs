using CloudWeather.DataLoader.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

IConfiguration config = new ConfigurationBuilder().
                 AddJsonFile("appsettings.json").
                 AddEnvironmentVariables().
                 Build();

var servicesConfig = config.GetSection("Services");

var tempServiceConfig = servicesConfig.GetSection("Temperature");
var tempServiceHost = tempServiceConfig["Host"];
var tempServicePort = tempServiceConfig["Port"];


var precipServiceConfig = servicesConfig.GetSection("Precipitation");
var precipServiceHost = precipServiceConfig["Host"];
var precipServicePort = precipServiceConfig["Port"];


var zipCodes = new List<string>
{
    "73026",
    "68104",
    "04401",
    "32808",
    "19717",
};

Console.WriteLine("Starting Data Load");

var temperatureHttpClient = new HttpClient();
temperatureHttpClient.BaseAddress = new Uri($"http://{tempServiceHost}:{tempServicePort}");

var precipitationHttpClient = new HttpClient();
precipitationHttpClient.BaseAddress = new Uri($"http://{precipServiceHost}:{precipServicePort}");


foreach (var zip in zipCodes)
{
    Console.WriteLine($"Processing ZipCode: {zip}");
    var from = DateTime.Now.AddYears(-2);
    var thru = DateTime.Now;

    for (var day = from; day.Date <= thru; day = day.AddDays(1))
    {
        var temps = PostTemp(zip, day, temperatureHttpClient);
        postPrecip(temps[0], zip, day, precipitationHttpClient);
    };
}

void postPrecip(int lowTemp, string zip, DateTime day, HttpClient httpclient)
{
    var rand = new Random();
    var isPrecip = rand.Next(2) < 1;

    PrecipitationModel precipitation;

    if (isPrecip)
    {
        var precipInches = rand.Next(1, 16);
        if(lowTemp < 32)
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "snow",
                ZipCode = zip,
                CreatedOn = day
            };
        }
        else
        {
            precipitation = new PrecipitationModel
            {
                AmountInches = precipInches,
                WeatherType = "rain",
                ZipCode = zip,
                CreatedOn = day
            };
        }
    }
    else
    {
        precipitation = new PrecipitationModel
        {
            AmountInches = 0,
            WeatherType = "none",
            ZipCode = zip,
            CreatedOn = day
        };

    }

    var precipResponse = precipitationHttpClient.
        PostAsJsonAsync("observation", precipitation).Result;

    if (precipResponse.IsSuccessStatusCode)
    {
        Console.Write($"Posted Precipitation: Date: {day:d}" +
                      $"Zip: {zip}" +
                      $"Type: {precipitation.WeatherType}" +
                      $"Amount: {precipitation.AmountInches}");
    };
}

List<int> PostTemp(string zip, DateTime day, HttpClient httpclient)
{
    var rand = new Random();
    var t1 = rand.Next(0,100);
    var t2 = rand.Next(0, 100);
    var hiloTempos = new List<int> { t1, t2 };
    hiloTempos.Sort();

    var temperatureObservation = new TemperatureModel
    {
        TempLowF = hiloTempos[0],
        TempHighF = hiloTempos[1],
        ZipCode = zip,
        CreatedOn = day
    };

    var tempoResponse = temperatureHttpClient.
        PostAsJsonAsync("observation", temperatureObservation).Result;

    if (tempoResponse.IsSuccessStatusCode)
    {
        Console.Write($"Posted Temperature: Date: {day:d}" +
                      $"Zip: {zip}" +
                      $"LO (F): {hiloTempos[0]}" +
                      $"Hi (F): {hiloTempos[1]}");
    };


    return hiloTempos;
}