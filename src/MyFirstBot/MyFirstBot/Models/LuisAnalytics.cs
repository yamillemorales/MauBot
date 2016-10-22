using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace MyFirstBot.Models
{
    public class Resolution
    {
    }

    public class Value
    {
        public string entity { get; set; }
        public string type { get; set; }
        public Resolution resolution { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public bool required { get; set; }
        public List<Value> value { get; set; }
    }

    public class Action
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public List<Parameter> parameters { get; set; }
    }

    //public class TopScoringIntent
    //{
    //    public string intent { get; set; }
    //    public double score { get; set; }
    //    public List<Action> actions { get; set; }
    //}
    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
        public List<Action> actions { get; set; }
    }

    public class Resolution2
    {
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public double score { get; set; }
        public Resolution2 resolution { get; set; }
    }

  
    public class Dialog
    {
        public string contextId { get; set; }
        public string status { get; set; }
    }

    public class LuisAnalytics
    {
        public string query { get; set; }
        public List<Intent> intents { get; set; }

        public List<Entity> entities { get; set; }
        public Dialog dialog { get; set; }

        public static string AnalyzeTemperature(string message, out string entity)
        {
            var ret = "No encontrado";
            using (WebClient wc = new WebClient())
            {
                var jsonLuisString = wc.DownloadString($"https://api.projectoxford.ai/luis/v1/application?id=31451800-8ad6-44e8-ab1f-c2c9562daec0&subscription-key=5c34a70127b14f3f87acec6360c42f41&q={message}");
                var jsonLuis = JsonConvert.DeserializeObject<LuisAnalytics>(jsonLuisString);
                var intent = jsonLuis.intents[0].intent;
                entity = jsonLuis.entities.Count > 0 ? jsonLuis?.entities[0].entity : null;
                if (intent == "GetWeather" && entity != null)
                {
                    var temperature = WeatherAnalytics.GetTemperature(entity);
                    ret = temperature != 0 ? temperature.ToString() : ret;
                }
            }
            return ret;
        }
    }
}