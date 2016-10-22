using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace MyFirstBot.Models
{
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main
    {
        public double temp { get; set; }
        public double pressure { get; set; }
        public int humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
        public double sea_level { get; set; }
        public double grnd_level { get; set; }
    }

    public class Wind
    {
        public double speed { get; set; }
        public double deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }

    public class WeatherAnalytics
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }

        public static double GetTemperature(string city)
        {
            var temp = 0d;
            using (var hc = new HttpClient())
            {
                hc.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/weather");
                var jsonWeatherString = hc.GetStringAsync($"?q={city}&APPID=b45f3fb5f8b4200062d1bbb572ffed97").Result;
                var jsonWeather = JsonConvert.DeserializeObject<WeatherAnalytics>(jsonWeatherString);
                var kelvin = jsonWeather?.main?.temp ?? 0;
                if (kelvin != 0)
                {
                    temp = kelvin - 273.15;
                }
            }
            return temp;
        }
    }
}