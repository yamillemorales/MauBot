using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;


namespace MyFirstBot.Models
{
    public class DeShowService
    {

        public class Location
        {
            public string name { get; set; }
            public string slug { get; set; }
        }

        public class Property
        {
            public int post_id { get; set; }
            public string post_title { get; set; }
            public string post_content { get; set; }
            public Location location { get; set; }
            public string operation_type { get; set; }
            public int property_id { get; set; }
            public string estate_property_status_update { get; set; }
            public string estate_property_address { get; set; }
            public int estate_property_price { get; set; }
            public string estate_property_price_text { get; set; }
            public int estate_property_size { get; set; }
            public string estate_property_size_unit { get; set; }
            public int estate_property_rooms { get; set; }
            public int estate_property_bedrooms { get; set; }
            public int estate_property_bathrooms { get; set; }
            public int estate_property_parkings { get; set; }
            public string thumbnail_url { get; set; }
            public List<List<object>> images { get; set; }
        }

        public class DeShowResponse
        {
            public int total_count { get; set; }
            public int page { get; set; }
            public int total_pages { get; set; }
            public List<Property> properties { get; set; }
        }
      
        public static string GetPropInfo(string message)
        {
            message = "guaynabo­es"; //TODO Remove

            var additional = "&location=" + message;
            var ret = "No encontrado";
            using (WebClient wc = new WebClient())
            {
                var jsonResponseString = wc.DownloadString($"http://listcompare.it/wp-json/wp/v2/search-properties?lat=18.415449&lng=-66.092548&rad=3&operation_type=Venta&bathrooms=3&orderby=price-low"); //TODO lat lon  
                var response = JsonConvert.DeserializeObject<DeShowResponse>(jsonResponseString);

                ret = "Encontramos " + response.total_count + " propiedades cerca de ti. ¿Deseas verlas?";

                //TODO: SEND properties as 
              
            }

            return ret;
        }


        public static string GetPropCount(string message)
        {
            message = "guaynabo­es "; //TODO Remove

            var additional = "&location=" + message;
            var ret = "No encontrado";
            using (WebClient wc = new WebClient())
            {
                var jsonResponseString = wc.DownloadString($"http://listcompare.it/wp­json/wp/v2/search­properties?lat=18.416415&lng=-66.072739&rad=3" + additional); //TODO lat lon  
                var response = JsonConvert.DeserializeObject<DeShowResponse>(jsonResponseString);

                ret = "Encontramos " + response.total_count + "propiedades cerca de ti.";

                //TODO: SEND properties as 

            }

            return ret;
        }




    }
}