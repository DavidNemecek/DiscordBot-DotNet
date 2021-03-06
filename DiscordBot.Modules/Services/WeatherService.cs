﻿using DiscordBot.Domain.Abstractions;
using DiscordBot.Domain.Weather.Dto;
using DiscordBot.Domain.Weather.OpenWeatherMapModel;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBot.Modules.Services
{
    /// <summary>
    /// Returns Weather Information from the privided Location
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient.CreateClient();
        }

        /// <summary>
        /// Get Picture of current weather
        /// </summary>
        /// <param name="location">Location of the picture</param>
        /// <returns>Stream of the Picture</returns>
        public async Task<Stream> GetWeatherStream(string location)
        {
            try
            {
                //Get Weather from the API
                var weather = await GetOpenWeatherMapModuleByLocation(location);

                //Get Picture
                if ((weather?.Weather?.Length ?? 0) == 0 || weather.Weather[0].Icon == null)
                {
                    return null;
                }
                string weatherIcon = weather.Weather[0].Icon;

                var pictureResponse = await _httpClient.GetAsync($"http://openweathermap.org/img/wn/{weatherIcon}@2x.png");
                //Send back to Discord Server
                return await pictureResponse.Content.ReadAsStreamAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get Temperature at the given Location
        /// </summary>
        /// <param name="location">Location of the Temperature</param>
        /// <returns>Temperature</returns>
        public async Task<double> GetTemperature(string location) => (await GetOpenWeatherMapModuleByLocation(location)).Main.Temp;

        public async Task<WeatherDto> GetWeatherAsync(string location)
        {
            //Get Weather from the API
            var weatherModel = await GetOpenWeatherMapModuleByLocation(location);

            //Get Picture
            if ((weatherModel?.Weather?.Length ?? 0) == 0 || weatherModel.Weather[0].Icon == null)
            {
                return null;
            }

            var firstData = weatherModel.Weather[0];


            
            return new WeatherDto
            {
                Main = firstData.Main,
                Description = firstData.Description,
                Temparature = $"{(weatherModel.Main.Temp - 273.15):##.##}°C",
                ThumbnailUrl = $"http://openweathermap.org/img/wn/{firstData.Icon}@2x.png",
                Humidity = weatherModel.Main.Humidity.ToString(),
                Pressure = weatherModel.Main.Pressure.ToString(),
                RealFeelTemp = $"{(weatherModel.Main.FeelsLike - 273.15):##.##}°C",
                
            };
        }

        private async Task<OpenWeatherMapModel> GetOpenWeatherMapModuleByLocation(string location)
        {
            var result = await _httpClient.GetAsync($"http://api.openweathermap.org/data/2.5/weather?q={location}&appid=ab2446e861742c6758a49c789d0f4e6a");
            return JsonConvert.DeserializeObject<OpenWeatherMapModel>(await result.Content.ReadAsStringAsync());
        }
    }
}