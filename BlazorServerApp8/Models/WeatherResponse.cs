using System.Collections.Generic;

namespace BlazorWasmApp.Models
{
    public class WeatherResponse
    {
        public Coord coord { get; set; } = new Coord();
        public List<Weather> weather { get; set; } = new List<Weather>();
        public string weatherBase { get; set; } = string.Empty; // Changed from 'base' to 'weatherBase'
        public Main main { get; set; } = new Main();
        public int visibility { get; set; }
        public Wind wind { get; set; } = new Wind();
        public Clouds clouds { get; set; } = new Clouds();
        public int dt { get; set; }
        public Sys sys { get; set; } = new Sys();
        public int timezone { get; set; }
        public int id { get; set; }
        public string name { get; set; } = string.Empty;
        public int cod { get; set; }
    }

    public class Coord
    {
        public float lon { get; set; }
        public float lat { get; set; }
    }

    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;
    }

    public class Main
    {
        public float temp { get; set; }
        public float feels_like { get; set; }
        public float temp_min { get; set; }
        public float temp_max { get; set; }
        public int pressure { get; set; }
        public int humidity { get; set; }
        public int sea_level { get; set; }
        public int grnd_level { get; set; }
    }

    public class Wind
    {
        public float speed { get; set; }
        public int deg { get; set; }
    }

    public class Clouds
    {
        public int all { get; set; }
    }

    public class Sys
    {
        public int type { get; set; }
        public int id { get; set; }
        public string country { get; set; } = string.Empty;
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }
}
