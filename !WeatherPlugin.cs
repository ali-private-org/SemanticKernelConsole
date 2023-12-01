using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Http; // Add this for HttpClient  

namespace SemanticKernelConsole
{
    public class WeatherPlugin
    {
        [SKFunction, Description("Gives the weather for a given latitude longitude")]
        public async static Task<string> Weather(
            [Description("first is latitude")] double latitude,
            [Description("second is longitude")] double longitude,
            [Description("location for the latitude and longitude")] string location)
        {
            var weather = await new HttpClient().GetStringAsync($"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true");

            return (await Program.SemanticKernel.InvokeSemanticFunctionAsync($"Summarize weather for: {location} \n weather data: {weather}"))
                .FunctionResults.First().GetValue<string>()!.Trim();
        }

        [SKFunction, Description("Converts location to latitude")]
        public async static Task<double> ConvertLocationToLatitude(
         [Description("location to convert to latitude")] string location)
        {
            var response = await GetLatLongResponse(location);
            return ParseLatitude(response);
        }

        [SKFunction, Description("Converts location to longitude")]
        public async static Task<double> ConvertLocationToLongitude(
         [Description("location to convert to longitude")] string location)
        {
            var response = await GetLatLongResponse(location);
            return ParseLongitude(response);
        }

        private async static Task<string> GetLatLongResponse(string location)
        {
            return (await Program.SemanticKernel.InvokeSemanticFunctionAsync($"What is longitude and latitude for: {location}"))
                .FunctionResults.First().GetValue<string>()!.Trim();
        }

        private static double ParseLatitude(string response)
        {
            Regex regex = new(@"(-?\d+\.\d+)");
            MatchCollection matches = regex.Matches(response);

            if (matches.Count >= 2)
            {
                return double.Parse(matches[0].Value);
            }
            throw new Exception("Could not parse latitude from the response.");
        }

        private static double ParseLongitude(string response)
        {
            Regex regex = new(@"(-?\d+\.\d+)");
            MatchCollection matches = regex.Matches(response);

            if (matches.Count >= 2)
            {
                return double.Parse(matches[1].Value);
            }
            throw new Exception("Could not parse longitude from the response.");
        }
    }
}
